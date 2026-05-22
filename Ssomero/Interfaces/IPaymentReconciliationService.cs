namespace Ssomero.Interfaces;

/// <summary>
/// Client-side service that asks the server to re-verify all pending payments
/// for the authenticated user. Called automatically on app resume and can also
/// be triggered manually from the payments page.
/// </summary>
public interface IPaymentReconciliationService
{
    /// <summary>
    /// Posts to <c>POST /api/payments/reconcile</c>.
    /// Returns the number of payments that were recovered (Completed), or -1 on error.
    /// </summary>
    Task<int> ReconcilePendingPaymentsAsync(CancellationToken ct = default);
}
