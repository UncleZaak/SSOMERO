using Microsoft.Extensions.Logging;
using Ssomero.Interfaces;

namespace Ssomero.Services;

/// <summary>
/// Background polling service. Fires <see cref="IRefreshCoordinator"/> events
/// on a configurable interval so ViewModels can refresh without coupling to timers.
/// Call <see cref="Start"/> once from App.xaml.cs after login.
/// Designed to be replaced with SignalR when the hub is ready.
/// </summary>
public class PollingService
{
    private readonly IRefreshCoordinator _coordinator;
    private readonly ILogger<PollingService> _logger;
    private CancellationTokenSource? _cts;

    // Tracks the last time each key was actually notified (by polling or forced).
    private readonly Dictionary<string, DateTime> _lastFired = new();
    private readonly Lock _fireLock = new();

    // Minimum gap between polling-driven notifications per key.
    // Prevents the poller from redundantly firing a key that was just
    // triggered by a user action (e.g. marking attendance).
    private static readonly TimeSpan MinKeyInterval = TimeSpan.FromSeconds(30);

    public static readonly TimeSpan DefaultInterval = TimeSpan.FromSeconds(60);
    // Reduced interval used while the app is backgrounded.
    public static readonly TimeSpan BackgroundInterval = TimeSpan.FromSeconds(180);

    private bool _appActive = true;

    public bool IsRunning => _cts is not null && !_cts.IsCancellationRequested;

    public PollingService(IRefreshCoordinator coordinator, ILogger<PollingService> logger)
    {
        _coordinator = coordinator;
        _logger      = logger;
    }

    public void Start(TimeSpan? interval = null)
    {
        if (IsRunning) return;

        _cts = new CancellationTokenSource();
        var delay = interval ?? DefaultInterval;
        var token = _cts.Token;

        _ = Task.Run(async () =>
        {
            _logger.LogInformation("PollingService started (interval={Interval}s)", delay.TotalSeconds);
            while (!token.IsCancellationRequested)
            {
                try
                {
                    var currentDelay = _appActive ? delay : BackgroundInterval;
                    await Task.Delay(currentDelay, token);
                    await TickAsync();
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "PollingService tick error");
                }
            }
            _logger.LogInformation("PollingService stopped.");
        }, token);
    }

    public void Stop()
    {
        _cts?.Cancel();
        _cts?.Dispose();
        _cts = null;
    }

    /// <summary>
    /// Call when the app moves to foreground. Resets to the standard polling interval
    /// and immediately fires a tick so the UI refreshes without waiting.
    /// </summary>
    public void OnAppResumed()
    {
        _appActive = true;
        _ = TickAsync();
    }

    /// <summary>
    /// Call when the app moves to background. Switches to a longer polling interval
    /// to conserve battery.
    /// </summary>
    public void OnAppSleeping()
    {
        _appActive = false;
    }

    /// <summary>
    /// Records that a key was just externally notified (e.g. by a ViewModel after a
    /// user action) so the next poll tick will skip it if the gap is too short.
    /// </summary>
    public void RecordExternalNotify(string key)
    {
        lock (_fireLock)
            _lastFired[key] = DateTime.UtcNow;
    }

    private async Task TickAsync()
    {
        _logger.LogDebug("PollingService tick — notifying subscribers");
        await MaybeNotifyAsync(RefreshKeys.Dashboard);
        await MaybeNotifyAsync(RefreshKeys.Announcements);
        await MaybeNotifyAsync(RefreshKeys.Schedule);
        await MaybeNotifyAsync(RefreshKeys.Materials);
        await MaybeNotifyAsync(RefreshKeys.Attendance);
    }

    private async Task MaybeNotifyAsync(string key)
    {
        lock (_fireLock)
        {
            if (_lastFired.TryGetValue(key, out var last) &&
                DateTime.UtcNow - last < MinKeyInterval)
            {
                _logger.LogDebug("PollingService skipping '{Key}' — notified {Elapsed:F0}s ago",
                    key, (DateTime.UtcNow - last).TotalSeconds);
                return;
            }
            _lastFired[key] = DateTime.UtcNow;
        }

        await _coordinator.NotifyAsync(key);
    }
}
