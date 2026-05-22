namespace Ssomero.Api.Services.Interfaces;

public interface ITenantService
{
    Guid? GetCurrentTenantId();
    string? GetCurrentTenantName();
}
