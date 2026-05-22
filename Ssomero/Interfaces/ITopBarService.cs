using System.ComponentModel;

namespace Ssomero.Interfaces;

/// <summary>
/// Singleton source of truth for the authenticated user's identity state.
/// Both the Shell flyout header and the AppTopBar bind from this service,
/// ensuring a single consistent identity surface across the entire app.
/// </summary>
public interface ITopBarService : INotifyPropertyChanged
{
    // ── Observable properties ─────────────────────────────────────────────

    /// <summary>User's full name, e.g. "Isaac Kabuye".</summary>
    string FullName { get; }

    /// <summary>Raw role string as returned by the API, e.g. "Admin", "Lecturer".</summary>
    string Role { get; }

    /// <summary>Computed initials, e.g. "IK" for "Isaac Kabuye".</summary>
    string Initials { get; }

    /// <summary>Profile photo URL as stored on the server (may be null/empty).</summary>
    string? PhotoUrl { get; }

    /// <summary>Photo URL with a cache-busting version query parameter appended.</summary>
    string? PhotoUrlWithVersion { get; }

    /// <summary>True when <see cref="PhotoUrl"/> contains a non-empty value.</summary>
    bool HasPhoto { get; }

    /// <summary>True once <see cref="LoadAsync"/> has completed at least once.</summary>
    bool IsLoaded { get; }

    // ── Events ────────────────────────────────────────────────────────────

    /// <summary>
    /// Raised whenever name, role, or photo changes so consumers can react
    /// (e.g. flyout header, top bar avatar).
    /// </summary>
    event EventHandler? ProfileChanged;

    // ── Methods ───────────────────────────────────────────────────────────

    /// <summary>
    /// Loads or refreshes user identity from SecureStorage + API.
    /// If <paramref name="forceRefresh"/> is false and already loaded, this is a no-op.
    /// On API failure, cached values are preserved (never blanks the UI).
    /// </summary>
    Task LoadAsync(bool forceRefresh = false);

    /// <summary>Resets all state. Call on logout.</summary>
    void Clear();

    /// <summary>Updates just the photo without a full API reload (e.g. after profile edit).</summary>
    void RefreshPhoto(string? newUrl);
}
