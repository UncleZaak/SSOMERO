using System.ComponentModel;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;
using Ssomero.Interfaces;

namespace Ssomero.Services;

/// <summary>
/// Singleton identity service that owns the authenticated user's display state.
/// <para>
/// Reads SecureStorage for fast initial population, then calls the profile API
/// for authoritative data. On any failure, previously cached values are kept so
/// the UI never blanks unexpectedly.
/// </para>
/// </summary>
public sealed class TopBarService : ITopBarService
{
    private readonly IProfileService _profile;
    private readonly ILogger<TopBarService> _logger;

    // ── Backing fields ────────────────────────────────────────────────────

    private string _fullName = string.Empty;
    private string _role = string.Empty;
    private string _initials = "S";
    private string? _photoUrl;
    private string? _photoUrlWithVersion;
    private bool _hasPhoto;
    private bool _isLoaded;

    public event PropertyChangedEventHandler? PropertyChanged;
    public event EventHandler? ProfileChanged;

    // ── Observable properties ─────────────────────────────────────────────

    public string FullName           { get => _fullName;           private set => Set(ref _fullName, value); }
    public string Role               { get => _role;               private set => Set(ref _role, value); }
    public string Initials           { get => _initials;           private set => Set(ref _initials, value); }
    public string? PhotoUrl          { get => _photoUrl;           private set => Set(ref _photoUrl, value); }
    public string? PhotoUrlWithVersion { get => _photoUrlWithVersion; private set => Set(ref _photoUrlWithVersion, value); }
    public bool HasPhoto             { get => _hasPhoto;           private set => Set(ref _hasPhoto, value); }
    public bool IsLoaded             { get => _isLoaded;           private set => Set(ref _isLoaded, value); }

    public TopBarService(IProfileService profile, ILogger<TopBarService> logger)
    {
        _profile = profile;
        _logger  = logger;
    }

    // ── LoadAsync ─────────────────────────────────────────────────────────

    public async Task LoadAsync(bool forceRefresh = false)
    {
        if (IsLoaded && !forceRefresh) return;

        // ── Step 1: Populate from SecureStorage immediately (zero latency) ──
        try
        {
            var cachedName = await SecureStorage.Default.GetAsync("user_name");
            var cachedRole = await SecureStorage.Default.GetAsync("user_role") ?? string.Empty;

            if (!string.IsNullOrWhiteSpace(cachedName))
            {
                FullName = cachedName;
                Initials = ComputeInitials(cachedName);
            }

            if (!string.IsNullOrWhiteSpace(cachedRole))
                Role = cachedRole;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "TopBarService: SecureStorage read failed");
        }

        // ── Step 2: Fetch fresh data from API ─────────────────────────────
        try
        {
            var dto = await _profile.GetProfileAsync();
            if (dto is not null)
            {
                var name = $"{dto.FirstName} {dto.LastName}".Trim();
                if (!string.IsNullOrWhiteSpace(name))
                {
                    FullName = name;
                    Initials = ComputeInitials(name);

                    // Keep SecureStorage in sync for next cold start
                    try { await SecureStorage.Default.SetAsync("user_name", name); }
                    catch { /* non-fatal */ }
                }

                if (!string.IsNullOrWhiteSpace(dto.Role))
                    Role = dto.Role;

                RefreshPhoto(dto.PhotoUrl);
            }
        }
        catch (Exception ex)
        {
            // Non-fatal — keep whatever cached values we already populated
            _logger.LogWarning(ex, "TopBarService: API profile fetch failed; keeping cached values");
        }

        IsLoaded = true;
        ProfileChanged?.Invoke(this, EventArgs.Empty);
    }

    // ── RefreshPhoto ──────────────────────────────────────────────────────

    public void RefreshPhoto(string? newUrl)
    {
        var hasPhoto = !string.IsNullOrWhiteSpace(newUrl);
        PhotoUrl     = hasPhoto ? newUrl : null;

        // Append a cache-busting version so stale images are not shown after photo changes
        PhotoUrlWithVersion = hasPhoto
            ? $"{newUrl}?v={DateTimeOffset.UtcNow.ToUnixTimeSeconds()}"
            : null;

        HasPhoto = hasPhoto;
        ProfileChanged?.Invoke(this, EventArgs.Empty);
    }

    // ── Clear ─────────────────────────────────────────────────────────────

    public void Clear()
    {
        FullName           = string.Empty;
        Role               = string.Empty;
        Initials           = "S";
        PhotoUrl           = null;
        PhotoUrlWithVersion = null;
        HasPhoto           = false;
        IsLoaded           = false;
    }

    // ── Helpers ───────────────────────────────────────────────────────────

    private static string ComputeInitials(string fullName)
    {
        if (string.IsNullOrWhiteSpace(fullName)) return "S";
        var parts = fullName.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
        return parts.Length >= 2
            ? $"{char.ToUpper(parts[0][0])}{char.ToUpper(parts[^1][0])}"
            : char.ToUpper(parts[0][0]).ToString();
    }

    private void Set<T>(ref T field, T value, [CallerMemberName] string? name = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return;
        field = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
