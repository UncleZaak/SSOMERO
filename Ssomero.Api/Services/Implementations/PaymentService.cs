using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Ssomero.Api.Configuration;
using Ssomero.Api.Data;
using Ssomero.Api.Dtos;
using Ssomero.Api.Entities;
using Ssomero.Api.Services.Interfaces;

namespace Ssomero.Api.Services.Implementations;

public sealed class PaymentService : IPaymentService
{
    // Duration granted per plan
    private static readonly Dictionary<PaymentPlan, TimeSpan> PlanDurations = new()
    {
        [PaymentPlan.Monthly]  = TimeSpan.FromDays(30),
        [PaymentPlan.Semester] = TimeSpan.FromDays(120),
    };

    private static readonly Dictionary<PaymentPlan, decimal> PlanAmounts = new()
    {
        [PaymentPlan.Monthly]  = 5000m,
        [PaymentPlan.Semester] = 15000m,
    };

    private readonly SsomeroDbContext _db;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly PaymentSettings _settings;
    private readonly IAuditLogService _audit;
    private readonly ILogger<PaymentService> _logger;

    public PaymentService(
        SsomeroDbContext db,
        IHttpClientFactory httpClientFactory,
        IOptions<PaymentSettings> settings,
        IAuditLogService audit,
        ILogger<PaymentService> logger)
    {
        _db = db;
        _httpClientFactory = httpClientFactory;
        _settings = settings.Value;
        _audit = audit;
        _logger = logger;
    }

    // ── Initiate ─────────────────────────────────────────────────────────────

    public async Task<(bool Success, string? Error, string? TxRef)> InitiatePaymentAsync(
        Guid studentId, string plan, string phoneNumber, CancellationToken ct = default)
    {
        if (!Enum.TryParse<PaymentPlan>(plan, ignoreCase: true, out var parsedPlan))
            return (false, $"Unknown plan '{plan}'. Valid values: Monthly, Semester.", null);

        var txRef = $"SSOMERO-{studentId:N}-{DateTime.UtcNow.Ticks}";
        var amount = PlanAmounts[parsedPlan];

        var payment = new Payment
        {
            Id                = Guid.NewGuid(),
            UserId            = studentId,
            Plan              = parsedPlan,
            Amount            = amount,
            Currency          = "UGX",
            Status            = PaymentStatus.Pending,
            Provider          = _settings.UseMock ? PaymentProvider.Mock : PaymentProvider.Flutterwave,
            ExternalReference = txRef,
            PhoneNumber       = phoneNumber,
            CreatedAt         = DateTime.UtcNow,
        };

        _db.Payments.Add(payment);
        await _db.SaveChangesAsync(ct);

        await _audit.LogAsync("payment_initiated", "Payment", payment.Id.ToString(),
            null, $"Plan={parsedPlan},TxRef={txRef}");

        if (_settings.UseMock)
        {
            _logger.LogInformation("[MockPayment] Initiated TxRef={TxRef} for Student={StudentId}", txRef, studentId);
            return (true, null, txRef);
        }

        // ── Flutterwave: trigger USSD / mobile-money prompt ──────────────────
        try
        {
            var client = _httpClientFactory.CreateClient("Flutterwave");
            var body = new
            {
                phone_number = phoneNumber,
                amount,
                currency = "UGX",
                tx_ref   = txRef,
                network  = "MTN",
            };
            var response = await client.PostAsJsonAsync("charges?type=mobile_money_uganda", body, ct);
            if (!response.IsSuccessStatusCode)
            {
                var err = await response.Content.ReadAsStringAsync(ct);
                _logger.LogWarning("Flutterwave initiate failed: {Error}", err);
                return (false, "Payment provider returned an error. Please try again.", null);
            }

            return (true, null, txRef);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Flutterwave initiate threw for TxRef={TxRef}", txRef);
            return (false, "Payment provider unavailable. Please try again later.", null);
        }
    }

    // ── Verify & Activate ────────────────────────────────────────────────────

    public async Task<(bool Success, string? Error)> VerifyAndActivateAsync(
        Guid studentId, string txRef, CancellationToken ct = default)
    {
        var payment = await _db.Payments
            .FirstOrDefaultAsync(p => p.ExternalReference == txRef && p.UserId == studentId, ct);

        if (payment is null)
            return (false, "Transaction not found.");

        if (payment.Status == PaymentStatus.Completed)
            return (true, null); // idempotent — already verified

        if (payment.Status is PaymentStatus.Failed or PaymentStatus.Cancelled or PaymentStatus.Expired)
            return (false, "This transaction has already been finalised and cannot be retried.");

        // ── Mock: auto-complete ───────────────────────────────────────────────
        if (_settings.UseMock || payment.Provider == PaymentProvider.Mock)
        {
            await CompletePaymentAsync(payment, ct);
            return (true, null);
        }

        // ── Flutterwave: server-side verification ─────────────────────────────
        try
        {
            var client = _httpClientFactory.CreateClient("Flutterwave");
            var response = await client.GetAsync($"transactions/verify_by_reference?tx_ref={txRef}", ct);
            if (!response.IsSuccessStatusCode)
                return (false, "Could not verify with payment provider.");

            var result = await response.Content.ReadFromJsonAsync<FlutterwaveVerifyResponse>(cancellationToken: ct);
            if (result?.Status != "success" || result.Data?.Status != "successful")
            {
                payment.Status        = PaymentStatus.Failed;
                payment.FailureReason = $"Provider status: {result?.Data?.Status ?? "unknown"}";
                await _db.SaveChangesAsync(ct);
                return (false, "Payment was not successful according to provider.");
            }

            await CompletePaymentAsync(payment, ct);
            return (true, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Flutterwave verify threw for TxRef={TxRef}", txRef);
            return (false, "Payment verification failed. Please try again.");
        }
    }

    // ── Webhook ──────────────────────────────────────────────────────────────

    public async Task HandleWebhookAsync(string txRef, string status, CancellationToken ct = default)
    {
        var payment = await _db.Payments
            .FirstOrDefaultAsync(p => p.ExternalReference == txRef, ct);

        if (payment is null)
        {
            _logger.LogWarning("Webhook: payment with TxRef={TxRef} not found.", txRef);
            return;
        }

        if (payment.Status == PaymentStatus.Completed)
        {
            await _audit.LogAsync("webhook_duplicate_ignored", "Payment",
                payment.Id.ToString(), null, $"TxRef={txRef},Status=AlreadyCompleted");
            return; // idempotent
        }

        if (status.Equals("successful", StringComparison.OrdinalIgnoreCase))
        {
            await CompletePaymentAsync(payment, ct);
        }
        else
        {
            payment.Status        = PaymentStatus.Failed;
            payment.FailureReason = $"Webhook status: {status}";
            await _db.SaveChangesAsync(ct);

            await _audit.LogAsync("payment_failed", "Payment", payment.Id.ToString(),
                null, $"TxRef={txRef},WebhookStatus={status}");
        }
    }

    // ── Read ──────────────────────────────────────────────────────────────────

    public async Task<SubscriptionResponse?> GetActiveSubscriptionAsync(
        Guid studentId, CancellationToken ct = default)
    {
        var now = DateTime.UtcNow;

        // Expire any lapsed subscriptions for this student
        var lapsed = await _db.Subscriptions
            .Where(s => s.UserId == studentId && s.IsActive && s.EndDate <= now)
            .ToListAsync(ct);

        foreach (var s in lapsed) s.IsActive = false;
        if (lapsed.Count > 0) await _db.SaveChangesAsync(ct);

        var sub = await _db.Subscriptions
            .Where(s => s.UserId == studentId && s.IsActive && s.EndDate > now)
            .OrderByDescending(s => s.EndDate)
            .FirstOrDefaultAsync(ct);

        if (sub is null) return null;

        return new SubscriptionResponse(sub.Id, sub.Plan.ToString(), sub.StartDate, sub.EndDate, sub.IsActive);
    }

    public async Task<PaymentResponse?> GetLatestPaymentAsync(
        Guid studentId, CancellationToken ct = default)
    {
        var p = await _db.Payments
            .Where(x => x.UserId == studentId)
            .OrderByDescending(x => x.CreatedAt)
            .FirstOrDefaultAsync(ct);

        if (p is null) return null;

        return new PaymentResponse(
            p.Id, p.Plan.ToString(), p.Amount, p.Currency,
            p.Status.ToString(), p.ExternalReference, p.CreatedAt, p.VerifiedAt,
            p.Provider.ToString(), p.PhoneNumber, p.FailureReason, p.ReceiptUrl);
    }

    // ── Private helpers ───────────────────────────────────────────────────────

    private async Task CompletePaymentAsync(Payment payment, CancellationToken ct)
    {
        if (payment.Status == PaymentStatus.Completed) return; // idempotent

        // ── State machine: reject invalid regressions ─────────────────────
        // Only Pending → Completed is allowed here.
        if (payment.Status != PaymentStatus.Pending)
        {
            _logger.LogWarning(
                "CompletePaymentAsync: payment {PaymentId} is in terminal state {Status} — skipping.",
                payment.Id, payment.Status);
            return;
        }

        payment.Status     = PaymentStatus.Completed;
        payment.VerifiedAt = DateTime.UtcNow;

        var duration = PlanDurations[payment.Plan];
        var now      = DateTime.UtcNow;

        // Extend from the end of any existing active subscription (stacking)
        var existing = await _db.Subscriptions
            .Where(s => s.UserId == payment.UserId && s.IsActive && s.EndDate > now)
            .OrderByDescending(s => s.EndDate)
            .FirstOrDefaultAsync(ct);

        var start = existing?.EndDate ?? now;

        _db.Subscriptions.Add(new Subscription
        {
            Id        = Guid.NewGuid(),
            UserId    = payment.UserId,
            Plan      = payment.Plan,
            StartDate = start,
            EndDate   = start.Add(duration),
            IsActive  = true,
            PaymentId = payment.Id,
        });

        await _db.SaveChangesAsync(ct);
        _logger.LogInformation("Subscription activated for Student={StudentId} Plan={Plan}", payment.UserId, payment.Plan);

        await _audit.LogAsync("payment_completed", "Payment", payment.Id.ToString(),
            null, $"Plan={payment.Plan},TxRef={payment.ExternalReference}");
    }

    public async Task<IReadOnlyList<PaymentHistoryItemResponse>> GetPaymentHistoryAsync(
        Guid studentId, int limit = 20, CancellationToken ct = default)
    {
        var items = await _db.Payments
            .Where(p => p.UserId == studentId)
            .OrderByDescending(p => p.CreatedAt)
            .Take(limit)
            .ToListAsync(ct);

        return items.Select(p => new PaymentHistoryItemResponse(
            p.Id, p.Plan.ToString(), p.Amount, p.Currency,
            p.Status.ToString(), p.ExternalReference,
            p.Provider.ToString(), p.PhoneNumber,
            p.CreatedAt, p.VerifiedAt,
            p.FailureReason, p.ReceiptUrl)).ToList();
    }

    public async Task<PaymentResponse?> GetPaymentByReferenceAsync(
        Guid studentId, string txRef, CancellationToken ct = default)
    {
        var p = await _db.Payments
            .FirstOrDefaultAsync(x => x.UserId == studentId && x.ExternalReference == txRef, ct);

        if (p is null) return null;

        return new PaymentResponse(
            p.Id, p.Plan.ToString(), p.Amount, p.Currency,
            p.Status.ToString(), p.ExternalReference, p.CreatedAt, p.VerifiedAt,
            p.Provider.ToString(), p.PhoneNumber, p.FailureReason, p.ReceiptUrl);
    }

    // ── Flutterwave response shape ────────────────────────────────────────────

    private sealed record FlutterwaveVerifyResponse(string? Status, FlutterwaveData? Data);
    private sealed record FlutterwaveData(string? Status);
}
