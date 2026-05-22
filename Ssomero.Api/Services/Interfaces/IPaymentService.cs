using Ssomero.Api.Dtos;

namespace Ssomero.Api.Services.Interfaces;

public interface IPaymentService
{
    /// <summary>
    /// Initiates a payment with the configured provider.
    /// Returns a provider-issued transaction reference on success.
    /// </summary>
    Task<(bool Success, string? Error, string? TxRef)> InitiatePaymentAsync(
        Guid studentId,
        string plan,
        string phoneNumber,
        CancellationToken ct = default);

    /// <summary>
    /// Verifies a transaction with the provider server-side and, on success,
    /// creates or renews the student's subscription.
    /// </summary>
    Task<(bool Success, string? Error)> VerifyAndActivateAsync(
        Guid studentId,
        string txRef,
        CancellationToken ct = default);

    /// <summary>
    /// Processes a provider webhook. Idempotent — safe to call multiple times
    /// for the same <paramref name="txRef"/>.
    /// </summary>
    Task HandleWebhookAsync(string txRef, string status, CancellationToken ct = default);

    /// <summary>Returns the student's currently active subscription, or null.</summary>
    Task<SubscriptionResponse?> GetActiveSubscriptionAsync(
        Guid studentId,
        CancellationToken ct = default);

    /// <summary>Returns the student's most recent payment record, or null.</summary>
    Task<PaymentResponse?> GetLatestPaymentAsync(
        Guid studentId,
        CancellationToken ct = default);

    /// <summary>Returns a list of the student's payment history, most recent first.</summary>
    Task<IReadOnlyList<PaymentHistoryItemResponse>> GetPaymentHistoryAsync(
        Guid studentId,
        int limit = 20,
        CancellationToken ct = default);

    /// <summary>
    /// Retrieves a single payment by its external reference (transaction ID).
    /// Used for polling the status of a pending payment.
    /// </summary>
    Task<PaymentResponse?> GetPaymentByReferenceAsync(
        Guid studentId,
        string txRef,
        CancellationToken ct = default);
}
