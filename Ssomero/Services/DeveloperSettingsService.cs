#if DEBUG
using Microsoft.Maui.Storage;
using Microsoft.Extensions.Logging;
using Ssomero.Configuration;

namespace Ssomero.Services;

/// <summary>
/// Debug-only developer settings persisted in Preferences (non-sensitive).
/// Controls BaseUrl, environment selection, timeouts, and debug toggles.
/// This service is only registered in DEBUG builds.
/// </summary>
public class DeveloperSettingsService
{
    private const string Key_Prefix = "dev:";
    private const string Key_BaseUrl = Key_Prefix + "BaseUrl";
    private const string Key_Environment = Key_Prefix + "Environment";
    private const string Key_Timeout = Key_Prefix + "TimeoutSeconds";
    private const string Key_DebugLogging = Key_Prefix + "DebugLogging";

    private readonly ILogger<DeveloperSettingsService> _logger;

    public DeveloperSettingsService(ILogger<DeveloperSettingsService> logger)
    {
        _logger = logger;
    }

    public string? GetBaseUrl() => Preferences.Get(Key_BaseUrl, null);
    public void SetBaseUrl(string? url)
    {
        if (string.IsNullOrWhiteSpace(url)) Preferences.Remove(Key_BaseUrl);
        else Preferences.Set(Key_BaseUrl, url);
        _logger.LogInformation("DeveloperSettings: BaseUrl set to {Url}", url);
    }

    public string GetEnvironment() => Preferences.Get(Key_Environment, "Development");
    public void SetEnvironment(string env) => Preferences.Set(Key_Environment, env);

    public int GetTimeoutSeconds() => Preferences.Get(Key_Timeout, 30);
    public void SetTimeoutSeconds(int seconds) => Preferences.Set(Key_Timeout, seconds);

    public bool GetDebugLogging() => Preferences.Get(Key_DebugLogging, false);
    public void SetDebugLogging(bool enabled) => Preferences.Set(Key_DebugLogging, enabled);

    public void Reset()
    {
        Preferences.Remove(Key_BaseUrl);
        Preferences.Remove(Key_Environment);
        Preferences.Remove(Key_Timeout);
        Preferences.Remove(Key_DebugLogging);
        _logger.LogInformation("DeveloperSettings: reset to defaults");
    }
}
#endif
