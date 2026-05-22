using Hangfire.Dashboard;

namespace Ssomero.Api.Configuration;

/// <summary>
/// Restricts the Hangfire dashboard to Admin-role JWT holders (or localhost in development).
/// </summary>
public sealed class HangfireAuthorizationFilter : IDashboardAuthorizationFilter
{
    private readonly IWebHostEnvironment _env;

    public HangfireAuthorizationFilter(IWebHostEnvironment env)
    {
        _env = env;
    }

    public bool Authorize(DashboardContext context)
    {
        var http = context.GetHttpContext();

        // Always allow in development
        if (_env.IsDevelopment())
            return true;

        // In production: require authenticated Admin
        return http.User.Identity?.IsAuthenticated == true
            && http.User.IsInRole("Admin");
    }
}
