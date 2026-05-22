namespace Ssomero.Api.Middleware;

/// <summary>
/// Injects security-related HTTP response headers on every response
/// and removes the Server header to reduce information disclosure.
/// </summary>
public sealed class SecurityHeadersMiddleware
{
    private readonly RequestDelegate _next;

    public SecurityHeadersMiddleware(RequestDelegate next) => _next = next;

    public async Task InvokeAsync(HttpContext context)
    {
        context.Response.OnStarting(() =>
        {
            var headers = context.Response.Headers;

            headers["X-Content-Type-Options"]  = "nosniff";
            headers["X-Frame-Options"]          = "DENY";
            headers["Referrer-Policy"]          = "strict-origin-when-cross-origin";
            headers["Permissions-Policy"]       = "camera=(), microphone=(), geolocation=(self)";
            // Content Security Policy: restrict sources to same-origin by default. Adjust for your app (fonts, scripts, styles)
            headers["Content-Security-Policy"]  = "default-src 'self'; img-src 'self' data:; script-src 'self'; style-src 'self' 'unsafe-inline'";
            headers.Remove("Server");

            return Task.CompletedTask;
        });

        await _next(context);
    }
}
