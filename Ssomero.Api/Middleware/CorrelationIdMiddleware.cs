using Serilog.Context;

namespace Ssomero.Api.Middleware;

/// <summary>
/// Reads the incoming X-Correlation-Id header (or generates a new GUID when absent),
/// echoes it in the response header, and pushes it into the Serilog log context so
/// every log entry produced during the request carries the correlation ID.
/// </summary>
public sealed class CorrelationIdMiddleware
{
    public const string HeaderName = "X-Correlation-Id";
    private readonly RequestDelegate _next;

    public CorrelationIdMiddleware(RequestDelegate next) => _next = next;

    public async Task InvokeAsync(HttpContext context)
    {
        var correlationId = context.Request.Headers[HeaderName].FirstOrDefault()
            ?? Guid.NewGuid().ToString("N");

        // Echo back so the caller can correlate its own logs
        context.Response.Headers[HeaderName] = correlationId;

        // Make available to downstream code via Items
        context.Items[HeaderName] = correlationId;

        using (LogContext.PushProperty("CorrelationId", correlationId))
        {
            await _next(context);
        }
    }
}
