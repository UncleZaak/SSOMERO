namespace Ssomero.Api.Services.Interfaces;

/// <summary>
/// Verifies pending payment transactions against the payment provider and
/// activates subscriptions that were completed but not yet acknowledged.
/// All operations are idempotent and safe to call repeatedly.
/// </summary>
public interface IPaymentReconciliationService
{
    /// <summary>
    /// Verifies all pending payments for <paramref name="userId"/> that were
    /// created within the last 48 hours.
    /// Returns counts of recovered, still-pending, and total payments examined.
    /// </summary>
    Task<ReconcileResult> ReconcilePendingPaymentsAsync(Guid userId, CancellationToken ct = default);

    /// <summary>
    /// Verifies and finalizes a single payment by its database ID.
    /// Returns <c>true</c> if the payment transitioned to Completed.
    /// </summary>
    Task<bool> VerifyAndFinalizeAsync(Guid paymentId, CancellationToken ct = default);

    /// <summary>
    /// Handles an inbound provider callback for <paramref name="reference"/>.
    /// Idempotent — duplicate calls for the same reference are logged and ignored.
    /// </summary>
    Task HandleProviderCallbackAsync(string provider, string reference, CancellationToken ct = default);
}

/// <summary>Summary of a reconciliation pass.</summary>
public record ReconcileResult(int Recovered, int StillPending, int Total);
