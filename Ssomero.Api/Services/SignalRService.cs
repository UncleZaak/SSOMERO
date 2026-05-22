using Microsoft.AspNetCore.SignalR;
using Ssomero.Api.Hubs;

namespace Ssomero.Api.Services;

/// <summary>
/// Centralised wrapper around SignalR hub contexts.
/// Inject this service instead of IHubContext directly to keep controllers thin.
/// </summary>
public class SignalRService
{
    private readonly IHubContext<NotificationHub> _notificationHub;
    private readonly IHubContext<ChatHub> _chatHub;
    private readonly ILogger<SignalRService> _logger;

    public SignalRService(
        IHubContext<NotificationHub> notificationHub,
        IHubContext<ChatHub> chatHub,
        ILogger<SignalRService> logger)
    {
        _notificationHub = notificationHub;
        _chatHub = chatHub;
        _logger = logger;
    }

    // ---- Notification Hub ----

    /// <summary>Push a notification to a specific user by their user identifier.</summary>
    public async Task NotifyUserAsync(string userId, string message, object? payload = null)
    {
        await _notificationHub.Clients.User(userId).SendAsync("ReceiveNotification", new
        {
            Message = message,
            Payload = payload,
            Timestamp = DateTime.UtcNow
        });
        _logger.LogDebug("[SignalR] Notification sent to user {UserId}", userId);
    }

    /// <summary>Push a notification to all members of a group.</summary>
    public async Task NotifyGroupAsync(string groupName, string message, object? payload = null)
    {
        await _notificationHub.Clients.Group(groupName).SendAsync("ReceiveNotification", new
        {
            Message = message,
            Payload = payload,
            Timestamp = DateTime.UtcNow
        });
        _logger.LogDebug("[SignalR] Notification sent to group {Group}", groupName);
    }

    /// <summary>Broadcast a notification to all connected clients.</summary>
    public async Task BroadcastAsync(string message, object? payload = null)
    {
        await _notificationHub.Clients.All.SendAsync("ReceiveNotification", new
        {
            Message = message,
            Payload = payload,
            Timestamp = DateTime.UtcNow
        });
        _logger.LogInformation("[SignalR] Broadcast: {Message}", message);
    }

    /// <summary>Send a chat message to a room group.</summary>
    public async Task SendChatMessageAsync(string roomId, string senderId, string message)
    {
        await _chatHub.Clients.Group(roomId).SendAsync("ReceiveMessage", new
        {
            SenderId = senderId,
            Message = message,
            Timestamp = DateTime.UtcNow
        });
        _logger.LogDebug("[SignalR] Chat message sent to room {RoomId}", roomId);
    }

    /// <summary>Send a private chat message to a specific user.</summary>
    public async Task SendPrivateMessageAsync(string recipientUserId, string senderId, string message)
    {
        await _chatHub.Clients.User(recipientUserId).SendAsync("ReceivePrivateMessage", new
        {
            SenderId = senderId,
            Message = message,
            Timestamp = DateTime.UtcNow
        });
        _logger.LogDebug("[SignalR] Private message from {Sender} to {Recipient}", senderId, recipientUserId);
    }

    /// <summary>Notify all clients that a user's status has changed (e.g., lecturer approved).</summary>
    public async Task NotifyStatusChangeAsync(string entityType, string entityId, string newStatus)
    {
        await _notificationHub.Clients.All.SendAsync("StatusChanged", new
        {
            EntityType = entityType,
            EntityId = entityId,
            NewStatus = newStatus,
            Timestamp = DateTime.UtcNow
        });
    }
}
