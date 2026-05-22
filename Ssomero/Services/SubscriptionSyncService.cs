using Ssomero.Interfaces;

namespace Ssomero.Services;

/// <summary>
/// Singleton that bridges <see cref="IRefreshCoordinator"/> subscription events
/// with a typed <see cref="ISubscriptionSyncService.SubscriptionChanged"/> event.
/// ViewModels and pages subscribe to <see cref="SubscriptionChanged"/> to avoid
/// coupling to the string-keyed coordinator directly.
/// </summary>
public sealed class SubscriptionSyncService : ISubscriptionSyncService
{
    private readonly IRefreshCoordinator _coordinator;

    public event EventHandler? SubscriptionChanged;

    public SubscriptionSyncService(IRefreshCoordinator coordinator)
    {
        _coordinator = coordinator;

        // Mirror RefreshKeys.Subscription into the typed event.
        _coordinator.Subscribe(RefreshKeys.Subscription, OnCoordinatorNotifiedAsync);
    }

    public async Task NotifySubscriptionChangedAsync()
    {
        // Notify via the coordinator so all IRefreshCoordinator subscribers also fire.
        await _coordinator.NotifyAsync(RefreshKeys.Subscription);
        // SubscriptionChanged is raised by OnCoordinatorNotifiedAsync in response.
    }

    private Task OnCoordinatorNotifiedAsync()
    {
        MainThread.BeginInvokeOnMainThread(() =>
            SubscriptionChanged?.Invoke(this, EventArgs.Empty));
        return Task.CompletedTask;
    }
}
