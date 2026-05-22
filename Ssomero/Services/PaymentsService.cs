using System.Net.Http.Json;
using Microsoft.Extensions.Logging;
using Ssomero.Interfaces;
using Ssomero.Models;

namespace Ssomero.Services;

public class PaymentsService : IPaymentsService
{
    private readonly IApiService _api;
    private readonly ILogger<PaymentsService> _logger;

    public PaymentsService(IApiService api, ILogger<PaymentsService> logger)
    {
        _api = api;
        _logger = logger;
    }

    public async Task<PaymentDto?> GetCurrentPlanAsync(CancellationToken ct = default)
    {
        try
        {
            var resp = await _api.GetAsync("payments/current", ct);
            if (!resp.IsSuccessStatusCode)
            {
                _logger.LogWarning("GetCurrentPlan returned {StatusCode}", resp.StatusCode);
                return null;
            }

            var result = await resp.Content.ReadFromJsonAsync<CurrentPlanResponse>(cancellationToken: ct);
            if (result is null) return null;

            var sub = result.Subscription;
            var pay = result.LatestPayment;

            if (sub is not null)
            {
                return new PaymentDto
                {
                    Id             = sub.Id,
                    StudentId      = string.Empty,
                    Plan           = Enum.TryParse<PaymentPlan>(sub.Plan, out var p) ? p : PaymentPlan.Monthly,
                    Amount         = pay?.Amount ?? 0,
                    Currency       = pay?.Currency ?? "UGX",
                    Status         = PaymentStatus.Completed,
                    PaidAt         = sub.StartDate,
                    ExpiresAt      = sub.EndDate,
                    TransactionRef = pay?.ExternalReference ?? string.Empty,
                    Provider       = pay?.Provider,
                    PhoneNumber    = pay?.PhoneNumber,
                    ReceiptUrl     = pay?.ReceiptUrl,
                };
            }

            if (pay is not null)
            {
                return new PaymentDto
                {
                    Id             = pay.Id,
                    StudentId      = string.Empty,
                    Plan           = Enum.TryParse<PaymentPlan>(pay.Plan, out var pl) ? pl : PaymentPlan.Monthly,
                    Amount         = pay.Amount,
                    Currency       = pay.Currency,
                    Status         = Enum.TryParse<PaymentStatus>(pay.Status, out var st) ? st : PaymentStatus.Pending,
                    PaidAt         = pay.CreatedAt,
                    ExpiresAt      = pay.VerifiedAt ?? pay.CreatedAt,
                    TransactionRef = pay.ExternalReference,
                    Provider       = pay.Provider,
                    PhoneNumber    = pay.PhoneNumber,
                    FailureReason  = pay.FailureReason,
                    ReceiptUrl     = pay.ReceiptUrl,
                };
            }

            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetCurrentPlanAsync failed");
            return null;
        }
    }

    public async Task<(bool Success, string? Error, string? TxRef)> InitiatePaymentAsync(
        PaymentPlan plan, string phoneNumber, CancellationToken ct = default)
    {
        try
        {
            var resp = await _api.PostAsync(
                "payments/initiate",
                JsonContent.Create(new { plan = plan.ToString(), phoneNumber }),
                ct);

            if (!resp.IsSuccessStatusCode)
            {
                var body = await resp.Content.ReadAsStringAsync(ct);
                _logger.LogWarning("InitiatePayment returned {StatusCode}: {Body}", resp.StatusCode, body);
                return (false, "Payment initiation failed. Please try again.", null);
            }

            var initiated = await resp.Content.ReadFromJsonAsync<InitiateResponse>(cancellationToken: ct);
            if (initiated?.TxRef is null)
                return (false, "Invalid response from payment service.", null);

            return (true, null, initiated.TxRef);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "InitiatePaymentAsync failed for plan {Plan}", plan);
            return (false, "Payment service unavailable. Please try again.", null);
        }
    }

    public async Task<(bool Success, string? Error)> VerifyPaymentAsync(
        string txRef, CancellationToken ct = default)
    {
        try
        {
            var resp = await _api.PostAsync(
                "payments/verify",
                JsonContent.Create(new { txRef }),
                ct);

            if (resp.IsSuccessStatusCode)
                return (true, null);

            if (resp.StatusCode == System.Net.HttpStatusCode.BadRequest)
            {
                var err = await resp.Content.ReadFromJsonAsync<ErrorResponse>(cancellationToken: ct);
                return (false, err?.Error ?? "Verification failed.");
            }

            return (false, "Verification failed. Please try again.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "VerifyPaymentAsync failed for TxRef={TxRef}", txRef);
            return (false, "Verification service unavailable.");
        }
    }

    public async Task<IReadOnlyList<PaymentHistoryDto>> GetHistoryAsync(CancellationToken ct = default)
    {
        try
        {
            var resp = await _api.GetAsync("payments/history", ct);
            if (!resp.IsSuccessStatusCode)
            {
                _logger.LogWarning("GetHistory returned {StatusCode}", resp.StatusCode);
                return [];
            }

            var items = await resp.Content.ReadFromJsonAsync<List<PaymentHistoryItemDto>>(cancellationToken: ct);
            if (items is null) return [];

            return items.Select(i => new PaymentHistoryDto
            {
                Id                = i.Id,
                Plan              = i.Plan,
                Amount            = i.Amount,
                Currency          = i.Currency,
                Status            = i.Status,
                ExternalReference = i.ExternalReference,
                Provider          = i.Provider,
                PhoneNumber       = i.PhoneNumber,
                CreatedAt         = i.CreatedAt,
                VerifiedAt        = i.VerifiedAt,
                FailureReason     = i.FailureReason,
                ReceiptUrl        = i.ReceiptUrl,
            }).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetHistoryAsync failed");
            return [];
        }
    }

    /// <summary>
    /// Polls the current status of a payment by transaction reference.
    /// Called by the app in a loop after payment initiation to detect completion.
    /// </summary>
    public async Task<PaymentDto?> PollPaymentStatusAsync(string txRef, CancellationToken ct = default)
    {
        try
        {
            var resp = await _api.GetAsync($"payments/{txRef}/status", ct);
            if (!resp.IsSuccessStatusCode)
            {
                _logger.LogWarning("PollPaymentStatus returned {StatusCode} for TxRef={TxRef}", resp.StatusCode, txRef);
                return null;
            }

            var payment = await resp.Content.ReadFromJsonAsync<PaymentRecordDto>(cancellationToken: ct);
            if (payment is null) return null;

            return new PaymentDto
            {
                Id             = payment.Id,
                StudentId      = string.Empty,
                Plan           = Enum.TryParse<PaymentPlan>(payment.Plan, out var p) ? p : PaymentPlan.Monthly,
                Amount         = payment.Amount,
                Currency       = payment.Currency,
                Status         = Enum.TryParse<PaymentStatus>(payment.Status, out var st) ? st : PaymentStatus.Pending,
                PaidAt         = payment.CreatedAt,
                ExpiresAt      = payment.VerifiedAt ?? payment.CreatedAt,
                TransactionRef = payment.ExternalReference,
                Provider       = payment.Provider,
                PhoneNumber    = payment.PhoneNumber,
                FailureReason  = payment.FailureReason,
                ReceiptUrl     = payment.ReceiptUrl,
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "PollPaymentStatusAsync failed for TxRef={TxRef}", txRef);
            return null;
        }
    }

    /// <summary>
    /// Reconciles any pending payments by re-verifying them with the provider.
    /// Called by the app on resume to recover from interrupted payment flows.
    /// </summary>
    public async Task<ReconcileResultDto?> ReconcilePendingAsync(CancellationToken ct = default)
    {
        try
        {
            var resp = await _api.PostAsync("payments/reconcile", null, ct);
            if (!resp.IsSuccessStatusCode)
            {
                _logger.LogWarning("ReconcilePending returned {StatusCode}", resp.StatusCode);
                return null;
            }

            return await resp.Content.ReadFromJsonAsync<ReconcileResultDto>(cancellationToken: ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "ReconcilePendingAsync failed");
            return null;
        }
    }

    public async Task<bool> SubscribeAsync(PaymentPlan plan, string phoneNumber, CancellationToken ct = default)
    {
        var (initiated, _, txRef) = await InitiatePaymentAsync(plan, phoneNumber, ct);
        if (!initiated || txRef is null) return false;

        var (verified, _) = await VerifyPaymentAsync(txRef, ct);
        return verified;
    }

    // â”€â”€ local response shapes â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

    private sealed record InitiateResponse(string? TxRef, string? Message);
    private sealed record ErrorResponse(string? Error);

    private sealed record CurrentPlanResponse(
        SubscriptionDto? Subscription,
        PaymentRecordDto? LatestPayment);

    private sealed record SubscriptionDto(
        Guid Id, string Plan, DateTime StartDate, DateTime EndDate, bool IsActive);

    private sealed record PaymentRecordDto(
        Guid Id, string Plan, decimal Amount, string Currency,
        string Status, string ExternalReference, DateTime CreatedAt, DateTime? VerifiedAt,
        string? Provider = null, string? PhoneNumber = null,
        string? FailureReason = null, string? ReceiptUrl = null);

    private sealed record PaymentHistoryItemDto(
        Guid Id, string Plan, decimal Amount, string Currency,
        string Status, string ExternalReference,
        string? Provider, string? PhoneNumber,
        DateTime CreatedAt, DateTime? VerifiedAt,
        string? FailureReason, string? ReceiptUrl);
}
