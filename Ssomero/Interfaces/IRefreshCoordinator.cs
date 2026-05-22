namespace Ssomero.Interfaces;

public interface IRefreshCoordinator
{
    /// <summary>Subscribe a callback to be invoked when the given key is notified.</summary>
    void Subscribe(string key, Func<Task> callback);

    /// <summary>Remove a previously registered callback.</summary>
    void Unsubscribe(string key, Func<Task> callback);

    /// <summary>Fire all callbacks registered under <paramref name="key"/>.</summary>
    Task NotifyAsync(string key);
}

public static class RefreshKeys
{
    public const string Schedule      = "schedule-updated";
    public const string Attendance    = "attendance-updated";
    public const string Dashboard     = "dashboard-updated";
    public const string Materials     = "materials-updated";
    public const string Announcements = "announcements-updated";
    public const string Subscription  = "subscription-updated";

    // Admin
    public const string Departments  = "admin-departments-updated";
    public const string Programs     = "admin-programs-updated";
    public const string Curriculum   = "admin-curriculum-updated";
    public const string Users        = "admin-users-updated";
}
