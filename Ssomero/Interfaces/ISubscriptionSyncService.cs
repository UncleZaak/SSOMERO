namespace Ssomero.Interfaces;

/// <summary>
/// Centralized subscription synchronization service.
/// Raises <see cref="SubscriptionChanged"/> whenever the subscription state changes
/// (payment success, reconciliation recovery, expiry), allowing dashboard, profile,
/// and premium-feature views to react without coupling to <see cref="IRefreshCoordinator"/>
/// string keys directly.
/// </summary>
public interface ISubscriptionSyncService
{
    /// <summary>
    /// Raised on the main thread whenever the subscription state has changed.
    /// Subscribe in ViewModels or ContentPages that gate premium features.
    /// </summary>
    event EventHandler? SubscriptionChanged;

    /// <summary>
    /// Broadcasts a subscription-change notification.
    /// Fires <see cref="SubscriptionChanged"/> and notifies the refresh coordinator
    /// so all subscribers are updated in one call.
    /// </summary>
    Task NotifySubscriptionChangedAsync();
}
