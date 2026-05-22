using Microsoft.Extensions.Logging;
using Plugin.LocalNotification;
using Plugin.LocalNotification.AndroidOption;
using Ssomero.Models;
using AppNotificationService = Ssomero.Interfaces.INotificationService;

namespace Ssomero.Services;

/// <summary>
/// Production notification service backed by Plugin.LocalNotification.
/// Uses the OS scheduler so notifications fire even when the app is backgrounded.
///
/// Architecture: all logic goes through this service via INotificationService.
/// To switch to Firebase/APNs push, replace this class — no ViewModel changes needed.
/// </summary>
public class NotificationService : AppNotificationService
{
    private readonly ILogger<NotificationService> _logger;

    // ── Notification ID ranges ─────────────────────────────────────────────
    // 1–49 999   : class reminders  (1 per session)
    // 50 001–99 999 : attendance reminders (1 per session)
    // 100 001–149 999 : attendance warnings (by course hash)
    // 150 001–199 999 : announcements / materials (by content hash)

    private const int ClassReminderBase    = 1;
    private const int AttendanceBase       = 50_001;
    private const int WarningBase          = 100_001;
    private const int AnnouncementBase     = 150_001;
    private const int IdRange              = 49_998;
    private const string ChannelId        = "ssomero_classes";
    private const string ReminderLeadMins = "15";

    public NotificationService(ILogger<NotificationService> logger)
    {
        _logger = logger;
    }

    // ── Semantic helpers ───────────────────────────────────────────────────

    public Task ScheduleClassReminderAsync(ClassSessionDto session)
    {
        var notifyAt = session.StartTime.AddMinutes(-15);
        if (notifyAt <= DateTime.Now) return Task.CompletedTask;

        var id      = SessionNotifId(session.SessionId, ClassReminderBase);
        var title   = "📚 Class Starting Soon";
        var message = $"{session.CourseName} starts in {ReminderLeadMins} min — {session.Location}";
        return ScheduleNotificationAsync(id, title, message, notifyAt.ToUniversalTime());
    }

    public Task SendAttendanceWarningAsync(string courseName, double percent)
    {
        var id      = StringNotifId(courseName, WarningBase);
        var title   = "⚠️ Attendance Warning";
        var message = $"Your attendance in {courseName} is {percent:F0}% — below the 75% threshold.";
        // Fire immediately (no schedule → now)
        return ScheduleNotificationAsync(id, title, message, DateTime.UtcNow.AddSeconds(2));
    }

    public Task SendNewMaterialNotificationAsync(string courseName, string topic)
    {
        var id      = StringNotifId(courseName + topic, AnnouncementBase);
        var title   = "📄 New Material Uploaded";
        var message = $"\"{topic}\" is now available for {courseName}.";
        return ScheduleNotificationAsync(id, title, message, DateTime.UtcNow.AddSeconds(2));
    }

    public Task SendAnnouncementNotificationAsync(string title, string body)
    {
        var id = StringNotifId(title + body, AnnouncementBase + 10_000);
        return ScheduleNotificationAsync(id, $"📢 {title}", body, DateTime.UtcNow.AddSeconds(2));
    }

    // ── Low-level scheduling ───────────────────────────────────────────────

    public Task ScheduleNotificationAsync(int id, string title, string message, DateTime notifyAtUtc)
    {
        try
        {
            var local = notifyAtUtc.ToLocalTime();
            if (local <= DateTime.Now.AddSeconds(1))
            {
                _logger.LogDebug("[Notif] Skipped past notification id={Id} title={Title}", id, title);
                return Task.CompletedTask;
            }

            var request = new NotificationRequest
            {
                NotificationId = id,
                Title          = title,
                Description    = message,
                Schedule = new NotificationRequestSchedule
                {
                    NotifyTime   = local,
                    RepeatType   = NotificationRepeat.No
                },
                Android = new AndroidOptions
                {
                    ChannelId  = ChannelId,
                    Priority   = AndroidPriority.High,
                    IsGroupSummary = false
                }
            };

            LocalNotificationCenter.Current.Show(request);
            _logger.LogDebug("[Notif] Scheduled id={Id} '{Title}' at {Time:HH:mm}", id, title, local);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "[Notif] Failed to schedule id={Id}", id);
        }

        return Task.CompletedTask;
    }

    public Task CancelNotificationAsync(int id)
    {
        try { LocalNotificationCenter.Current.Cancel(id); }
        catch (Exception ex) { _logger.LogWarning(ex, "[Notif] Cancel({Id}) failed", id); }
        return Task.CompletedTask;
    }

    public Task CancelAllNotificationsAsync()
    {
        try { LocalNotificationCenter.Current.CancelAll(); }
        catch (Exception ex) { _logger.LogWarning(ex, "[Notif] CancelAll failed"); }
        return Task.CompletedTask;
    }

    // ── Permissions ────────────────────────────────────────────────────────

    public async Task RequestPermissionAsync()
    {
        try
        {
            var permission = new NotificationPermission
            {
                AskPermission = true
            };
            var granted = await LocalNotificationCenter.Current.RequestNotificationPermission(permission);
            _logger.LogInformation("[Notif] Permission granted={Granted}", granted);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "[Notif] RequestPermission failed — notifications may not work");
        }
    }

    // ── Schedule rebuild ───────────────────────────────────────────────────

    public async Task RebuildForSessionsAsync(IReadOnlyList<ClassSessionDto> sessions)
    {
        try
        {
            // Cancel only session-range IDs (leave warning/announcement notifs intact)
            var sessionIds = sessions
                .SelectMany(s => new[]
                {
                    SessionNotifId(s.SessionId, ClassReminderBase),
                    SessionNotifId(s.SessionId, AttendanceBase)
                })
                .ToArray();

            if (sessionIds.Length > 0)
                LocalNotificationCenter.Current.Cancel(sessionIds);

            var now = DateTime.Now;
            var scheduled = 0;

            foreach (var s in sessions)
            {
                // ── 1. Class reminder: 15 min before start ──────────────
                var reminderTime = s.StartTime.AddMinutes(-15);
                if (reminderTime > now)
                {
                    await ScheduleNotificationAsync(
                        SessionNotifId(s.SessionId, ClassReminderBase),
                        "📚 Class Starting Soon",
                        $"{s.CourseName} starts in 15 min — {s.Location}",
                        reminderTime.ToUniversalTime());
                    scheduled++;
                }

                // ── 2. Attendance reminder: at session start ─────────────
                if (s.StartTime > now)
                {
                    await ScheduleNotificationAsync(
                        SessionNotifId(s.SessionId, AttendanceBase),
                        "✅ Mark Your Attendance",
                        $"Don't forget to mark attendance for {s.CourseName}.",
                        s.StartTime.ToUniversalTime());
                    scheduled++;
                }
            }

            _logger.LogInformation("[Notif] Rebuilt schedule: {Count} notifications for {Sessions} sessions",
                scheduled, sessions.Count);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "[Notif] RebuildForSessionsAsync failed — no notifications scheduled");
        }
    }

    // ── ID helpers ─────────────────────────────────────────────────────────

    /// <summary>Deterministic int ID from a session GUID in the given range base.</summary>
    private static int SessionNotifId(Guid sessionId, int rangeBase)
        => rangeBase + Math.Abs(sessionId.GetHashCode()) % IdRange;

    /// <summary>Deterministic int ID from an arbitrary string in the given range base.</summary>
    private static int StringNotifId(string key, int rangeBase)
        => rangeBase + Math.Abs(key.GetHashCode()) % IdRange;
}
