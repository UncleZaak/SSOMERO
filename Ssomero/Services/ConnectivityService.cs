#if DEBUG
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Networking;

namespace Ssomero.Services;

/// <summary>
/// Simple connectivity monitor for debug diagnostics.
/// </summary>
public class ConnectivityService
{
    private readonly ILogger<ConnectivityService> _logger;

    public ConnectivityService(ILogger<ConnectivityService> logger)
    {
        _logger = logger;
        Connectivity.ConnectivityChanged += OnConnectivityChanged;
    }

    private void OnConnectivityChanged(object? sender, ConnectivityChangedEventArgs e)
    {
        _logger.LogInformation("Connectivity changed: NetworkAccess={Access}, Profiles={Profiles}", e.NetworkAccess, string.Join(',', e.ConnectionProfiles));
    }
}
#endif
