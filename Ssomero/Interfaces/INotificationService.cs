using Ssomero.Models;

namespace Ssomero.Interfaces;

public interface INotificationService
{
    // ── Semantic high-level notifications ────────────────────────────────────

    /// <summary>Schedule an OS notification 15 minutes before the session starts.</summary>
    Task ScheduleClassReminderAsync(ClassSessionDto session);

    /// <summary>Show an attendance warning notification (below-threshold alert).</summary>
    Task SendAttendanceWarningAsync(string courseName, double percent);

    /// <summary>Notify the student that new material was uploaded.</summary>
    Task SendNewMaterialNotificationAsync(string courseName, string topic);

    /// <summary>Notify the student of a new announcement.</summary>
    Task SendAnnouncementNotificationAsync(string title, string body);

    // ── Low-level scheduling primitives ──────────────────────────────────────

    /// <summary>Schedule a local OS notification at a specific UTC time.</summary>
    Task ScheduleNotificationAsync(int id, string title, string message, DateTime notifyAtUtc);

    /// <summary>Cancel a scheduled or delivered notification by id.</summary>
    Task CancelNotificationAsync(int id);

    /// <summary>Cancel all scheduled and delivered notifications.</summary>
    Task CancelAllNotificationsAsync();

    // ── Lifecycle ─────────────────────────────────────────────────────────────

    /// <summary>Request OS notification permission. Safe to call multiple times.</summary>
    Task RequestPermissionAsync();

    /// <summary>
    /// Cancel all existing session notifications and reschedule them from the
    /// supplied session list. Skips sessions in the past.
    /// </summary>
    Task RebuildForSessionsAsync(IReadOnlyList<ClassSessionDto> sessions);
}
