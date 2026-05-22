namespace Ssomero.Api.Services.Interfaces;

public interface IAuditLogService
{
    Task LogAsync(
        string action,
        string entityName,
        string? entityId = null,
        string? oldValues = null,
        string? newValues = null);
}
