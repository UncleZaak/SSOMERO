using Hangfire;
using Microsoft.AspNetCore.SignalR;
using Ssomero.Api.Hubs;

namespace Ssomero.Api.BackgroundJobs;

public class NotificationJobs
{
    private readonly IHubContext<NotificationHub> _notificationHub;
    private readonly ILogger<NotificationJobs> _logger;

    public NotificationJobs(
        IHubContext<NotificationHub> notificationHub,
        ILogger<NotificationJobs> logger)
    {
        _notificationHub = notificationHub;
        _logger = logger;
    }

    /// <summary>Push a real-time notification to a specific user.</summary>
    [AutomaticRetry(Attempts = 2)]
    public async Task SendNotificationAsync(string userId, string message)
    {
        await _notificationHub.Clients.User(userId).SendAsync("ReceiveNotification", message, DateTime.UtcNow);
        _logger.LogInformation("[NotificationJobs] Notification sent to user {UserId}", userId);
    }

    /// <summary>Broadcast an announcement to all connected clients.</summary>
    [AutomaticRetry(Attempts = 2)]
    public async Task BroadcastAnnouncementAsync(string announcement)
    {
        await _notificationHub.Clients.All.SendAsync("ReceiveNotification", announcement, DateTime.UtcNow);
        _logger.LogInformation("[NotificationJobs] Broadcast announcement: {Announcement}", announcement);
    }

    /// <summary>Push a notification to all members of a group (e.g., class group).</summary>
    [AutomaticRetry(Attempts = 2)]
    public async Task SendGroupNotificationAsync(string groupName, string message)
    {
        await _notificationHub.Clients.Group(groupName).SendAsync("ReceiveNotification", message, DateTime.UtcNow);
        _logger.LogInformation("[NotificationJobs] Notification sent to group {Group}", groupName);
    }
}
