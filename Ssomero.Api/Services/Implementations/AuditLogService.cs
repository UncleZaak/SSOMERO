using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Ssomero.Api.Data;
using Ssomero.Api.Entities;
using Ssomero.Api.Services.Interfaces;

namespace Ssomero.Api.Services.Implementations;

public class AuditLogService : IAuditLogService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<AuditLogService> _logger;

    public AuditLogService(
        IServiceScopeFactory scopeFactory,
        IHttpContextAccessor httpContextAccessor,
        ILogger<AuditLogService> logger)
    {
        _scopeFactory = scopeFactory;
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
    }

    public Task LogAsync(
        string action,
        string entityName,
        string? entityId = null,
        string? oldValues = null,
        string? newValues = null)
    {
        var ctx = _httpContextAccessor.HttpContext;
        var user = ctx?.User;

        var log = new AuditLog
        {
            Id = Guid.NewGuid(),
            Action = action,
            EntityName = entityName,
            EntityId = entityId,
            UserId = Guid.TryParse(user?.FindFirstValue(ClaimTypes.NameIdentifier), out var uid) ? uid : null,
            UserEmail = user?.FindFirstValue(ClaimTypes.Email),
            UserRole = user?.FindFirstValue(ClaimTypes.Role),
            OldValues = oldValues,
            NewValues = newValues,
            IpAddress = ctx?.Connection.RemoteIpAddress?.ToString(),
            UserAgent = ctx?.Request.Headers.UserAgent.ToString() is { Length: > 0 } ua ? ua[..Math.Min(ua.Length, 500)] : null,
            CreatedAt = DateTime.UtcNow
        };

        // Fire-and-forget using a dedicated scope so we never contend with the
        // request's DbContext SaveChanges transaction.
        _ = Task.Run(async () =>
        {
            try
            {
                await using var scope = _scopeFactory.CreateAsyncScope();
                var db = scope.ServiceProvider.GetRequiredService<SsomeroDbContext>();
                db.AuditLogs.Add(log);
                await db.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to persist audit log for action {Action} on {Entity}:{EntityId}",
                    action, entityName, entityId);
            }
        });

        return Task.CompletedTask;
    }
}
