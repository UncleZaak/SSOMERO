using Ssomero.Models;

namespace Ssomero.Interfaces;

public interface IPaymentsService
{
    Task<PaymentDto?> GetCurrentPlanAsync(CancellationToken ct = default);

    /// <summary>
    /// Initiates a payment with the provider.
    /// Returns (Success, ErrorMessage, TxRef).
    /// </summary>
    Task<(bool Success, string? Error, string? TxRef)> InitiatePaymentAsync(
        PaymentPlan plan, string phoneNumber, CancellationToken ct = default);

    /// <summary>
    /// Server-side verification of a pending transaction.
    /// Returns (Success, ErrorMessage).
    /// </summary>
    Task<(bool Success, string? Error)> VerifyPaymentAsync(
        string txRef, CancellationToken ct = default);

    Task<IReadOnlyList<PaymentHistoryDto>> GetHistoryAsync(CancellationToken ct = default);

    /// <summary>
    /// Polls the current status of a payment by transaction reference.
    /// </summary>
    Task<PaymentDto?> PollPaymentStatusAsync(string txRef, CancellationToken ct = default);

    /// <summary>
    /// Reconciles any pending payments by re-verifying them with the provider.
    /// </summary>
    Task<ReconcileResultDto?> ReconcilePendingAsync(CancellationToken ct = default);

    /// <summary>Legacy combined initiate+verify helper (kept for backward compatibility).</summary>
    Task<bool> SubscribeAsync(PaymentPlan plan, string phoneNumber, CancellationToken ct = default);
}

public record ReconcileResultDto(int Recovered, int StillPending, int Total);
