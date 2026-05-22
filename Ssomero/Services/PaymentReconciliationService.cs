using System.Net.Http.Json;
using Microsoft.Extensions.Logging;
using Ssomero.Interfaces;

namespace Ssomero.Services;

/// <summary>
/// Asks the server to re-verify all pending payments for the current user.
/// On success, notifies the refresh coordinator so the payments page,
/// dashboard and profile all update immediately — no restart required.
/// </summary>
public sealed class PaymentReconciliationService : IPaymentReconciliationService
{
    private readonly IApiService _api;
    private readonly IRefreshCoordinator _refresh;
    private readonly ILogger<PaymentReconciliationService> _logger;

    public PaymentReconciliationService(
        IApiService api,
        IRefreshCoordinator refresh,
        ILogger<PaymentReconciliationService> logger)
    {
        _api     = api;
        _refresh = refresh;
        _logger  = logger;
    }

    public async Task<int> ReconcilePendingPaymentsAsync(CancellationToken ct = default)
    {
        try
        {
            var resp = await _api.PostAsync("payments/reconcile", null!, ct);

            if (!resp.IsSuccessStatusCode)
            {
                _logger.LogDebug(
                    "ReconcilePendingPaymentsAsync: server returned {Status}", resp.StatusCode);
                return -1;
            }

            var result = await resp.Content.ReadFromJsonAsync<ReconcileResponse>(cancellationToken: ct);
            var recovered = result?.Recovered ?? 0;

            _logger.LogInformation(
                "Reconciliation complete: Recovered={Recovered}, StillPending={Pending}",
                recovered, result?.StillPending ?? 0);

            // If any pending payments were recovered, refresh all subscription-dependent UI.
            if (recovered > 0)
            {
                await _refresh.NotifyAsync(RefreshKeys.Subscription);
                await _refresh.NotifyAsync(RefreshKeys.Dashboard);
            }

            return recovered;
        }
        catch (OperationCanceledException)
        {
            return -1;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "ReconcilePendingPaymentsAsync failed");
            return -1;
        }
    }

    private sealed record ReconcileResponse(int Recovered, int StillPending, int Total);
}
