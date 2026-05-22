using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Ssomero.Api.Hubs;

[Authorize]
public class ChatHub : Hub
{
    private readonly ILogger<ChatHub> _logger;

    public ChatHub(ILogger<ChatHub> logger)
    {
        _logger = logger;
    }

    public override async Task OnConnectedAsync()
    {
        _logger.LogInformation("Chat client connected: {ConnectionId}", Context.ConnectionId);
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        _logger.LogInformation("Chat client disconnected: {ConnectionId}", Context.ConnectionId);
        await base.OnDisconnectedAsync(exception);
    }

    public async Task JoinRoom(string roomId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, roomId);
        await Clients.Group(roomId).SendAsync("UserJoined", Context.UserIdentifier, DateTime.UtcNow);
    }

    public async Task LeaveRoom(string roomId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, roomId);
        await Clients.Group(roomId).SendAsync("UserLeft", Context.UserIdentifier, DateTime.UtcNow);
    }

    public async Task SendMessage(string roomId, string message)
    {
        await Clients.Group(roomId).SendAsync("ReceiveMessage", new
        {
            SenderId = Context.UserIdentifier,
            Message = message,
            Timestamp = DateTime.UtcNow
        });
    }

    public async Task SendPrivateMessage(string recipientUserId, string message)
    {
        await Clients.User(recipientUserId).SendAsync("ReceivePrivateMessage", new
        {
            SenderId = Context.UserIdentifier,
            Message = message,
            Timestamp = DateTime.UtcNow
        });
    }
}
