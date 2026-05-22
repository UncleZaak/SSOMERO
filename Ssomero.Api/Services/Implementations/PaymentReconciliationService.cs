using Microsoft.EntityFrameworkCore;
using Ssomero.Api.Data;
using Ssomero.Api.Entities;
using Ssomero.Api.Services.Interfaces;

namespace Ssomero.Api.Services.Implementations;

/// <summary>
/// Reconciles pending payments by re-verifying them with the payment provider.
/// Used both by the app-resume endpoint and as a scheduled recovery mechanism.
/// </summary>
public sealed class PaymentReconciliationService : IPaymentReconciliationService
{
    // Only attempt recovery for payments created within this window.
    private static readonly TimeSpan MaxPendingAge = TimeSpan.FromHours(48);

    // Safety cap: never attempt to reconcile more than this many in a single pass.
    private const int MaxPerPass = 10;

    private readonly SsomeroDbContext _db;
    private readonly IPaymentService _paymentService;
    private readonly IAuditLogService _audit;
    private readonly ILogger<PaymentReconciliationService> _logger;

    public PaymentReconciliationService(
        SsomeroDbContext db,
        IPaymentService paymentService,
        IAuditLogService audit,
        ILogger<PaymentReconciliationService> logger)
    {
        _db             = db;
        _paymentService = paymentService;
        _audit          = audit;
        _logger         = logger;
    }

    // ── ReconcilePendingPaymentsAsync ─────────────────────────────────────

    public async Task<ReconcileResult> ReconcilePendingPaymentsAsync(
        Guid userId, CancellationToken ct = default)
    {
        var cutoff = DateTime.UtcNow.Subtract(MaxPendingAge);

        var pending = await _db.Payments
            .Where(p => p.UserId == userId
                     && p.Status == PaymentStatus.Pending
                     && p.CreatedAt >= cutoff)
            .OrderByDescending(p => p.CreatedAt)
            .Take(MaxPerPass)
            .ToListAsync(ct);

        if (pending.Count == 0)
            return new ReconcileResult(0, 0, 0);

        int recovered = 0;

        foreach (var payment in pending)
        {
            try
            {
                var finalized = await VerifyAndFinalizeAsync(payment.Id, ct);
                if (finalized) recovered++;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex,
                    "Reconciliation attempt failed for Payment={PaymentId}", payment.Id);
            }
        }

        _logger.LogInformation(
            "Reconciliation complete for User={UserId}: Recovered={Recovered}/{Total}",
            userId, recovered, pending.Count);

        await _audit.LogAsync(
            "payment_reconciliation", "Payment",
            userId.ToString(),
            null,
            $"Recovered={recovered},StillPending={pending.Count - recovered},Total={pending.Count}");

        return new ReconcileResult(recovered, pending.Count - recovered, pending.Count);
    }

    // ── VerifyAndFinalizeAsync ────────────────────────────────────────────

    public async Task<bool> VerifyAndFinalizeAsync(Guid paymentId, CancellationToken ct = default)
    {
        var payment = await _db.Payments.FindAsync([paymentId], ct);
        if (payment is null) return false;

        // Already in a terminal state — nothing to do.
        if (payment.Status == PaymentStatus.Completed)
            return true;

        if (payment.Status is PaymentStatus.Failed
                           or PaymentStatus.Cancelled
                           or PaymentStatus.Expired)
            return false;

        var (success, error) = await _paymentService
            .VerifyAndActivateAsync(payment.UserId, payment.ExternalReference, ct);

        if (success)
        {
            _logger.LogInformation(
                "Payment recovered via reconciliation: PaymentId={PaymentId}", paymentId);

            await _audit.LogAsync(
                "payment_recovered", "Payment",
                paymentId.ToString(),
                null,
                $"TxRef={payment.ExternalReference}");
        }
        else
        {
            _logger.LogDebug(
                "Reconciliation: payment still pending PaymentId={PaymentId} Error={Error}",
                paymentId, error);
        }

        return success;
    }

    // ── HandleProviderCallbackAsync ───────────────────────────────────────

    public async Task HandleProviderCallbackAsync(
        string provider, string reference, CancellationToken ct = default)
    {
        var payment = await _db.Payments
            .FirstOrDefaultAsync(p => p.ExternalReference == reference, ct);

        if (payment is null)
        {
            _logger.LogWarning(
                "HandleProviderCallbackAsync: no payment found for Ref={Reference}", reference);
            return;
        }

        // Idempotency guard — duplicate callback after polling already succeeded.
        if (payment.Status == PaymentStatus.Completed)
        {
            _logger.LogInformation(
                "HandleProviderCallbackAsync: duplicate callback ignored for Ref={Reference}", reference);

            await _audit.LogAsync(
                "webhook_duplicate_ignored", "Payment", reference,
                null, $"Provider={provider},Status=AlreadyCompleted");
            return;
        }

        await VerifyAndFinalizeAsync(payment.Id, ct);
    }
}
