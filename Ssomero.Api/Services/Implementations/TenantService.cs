using System.Security.Claims;
using Ssomero.Api.Services.Interfaces;

namespace Ssomero.Api.Services.Implementations;

public class TenantService : ITenantService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public TenantService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid? GetCurrentTenantId()
    {
        var claim = _httpContextAccessor.HttpContext?.User
            .FindFirstValue("university_id");

        return Guid.TryParse(claim, out var id) ? id : null;
    }

    public string? GetCurrentTenantName()
    {
        return _httpContextAccessor.HttpContext?.Items["TenantName"] as string;
    }
}
