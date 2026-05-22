using System.Diagnostics;
using System.Security.Claims;

namespace Ssomero.Api.Middleware;

/// <summary>
/// Logs an audit record for every mutating request (POST / PUT / DELETE).
/// Does NOT read or log the request or response body.
/// Runs after UseAuthorization so the user identity is available.
/// </summary>
public sealed class AuditRequestMiddleware
{
    private static readonly HashSet<string> _auditMethods =
        new(StringComparer.OrdinalIgnoreCase) { "POST", "PUT", "DELETE" };

    private readonly RequestDelegate _next;
    private readonly ILogger<AuditRequestMiddleware> _logger;

    public AuditRequestMiddleware(RequestDelegate next, ILogger<AuditRequestMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (!_auditMethods.Contains(context.Request.Method))
        {
            await _next(context);
            return;
        }

        var sw = Stopwatch.StartNew();
        await _next(context);
        sw.Stop();

        var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier)
                  ?? context.User.FindFirstValue("sub")
                  ?? "anonymous";

        var correlationId = context.Items.TryGetValue("CorrelationId", out var cid)
            ? cid?.ToString() ?? "-"
            : "-";

        _logger.LogInformation(
            "AUDIT {Method} {Path} | User={UserId} | Correlation={CorrelationId} | Status={StatusCode} | {ElapsedMs}ms",
            context.Request.Method,
            context.Request.Path,
            userId,
            correlationId,
            context.Response.StatusCode,
            sw.ElapsedMilliseconds);
    }
}
