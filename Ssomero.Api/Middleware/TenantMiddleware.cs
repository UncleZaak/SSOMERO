using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Ssomero.Api.Data;

namespace Ssomero.Api.Middleware;

public class TenantMiddleware
{
    private readonly RequestDelegate _next;

    public TenantMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, SsomeroDbContext db)
    {
        var universityIdClaim = context.User.FindFirstValue("university_id");

        if (Guid.TryParse(universityIdClaim, out var universityId))
        {
            var university = await db.Universities
                .AsNoTracking()
                .Where(u => u.Id == universityId)
                .Select(u => new { u.Id, u.Name })
                .FirstOrDefaultAsync();

            if (university is not null)
            {
                context.Items["TenantId"] = university.Id;
                context.Items["TenantName"] = university.Name;
            }
        }

        await _next(context);
    }
}
