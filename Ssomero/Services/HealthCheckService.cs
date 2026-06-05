#if DEBUG
using System.Net.Http.Json;
using Microsoft.Extensions.Logging;
using System;

namespace Ssomero.Services;

/// <summary>
/// Lightweight health check service for debug/dev use. Uses a short timeout and checks API /health endpoint.
/// </summary>
public class HealthCheckService
{
    private readonly System.Net.Http.IHttpClientFactory _factory;
    private readonly ILogger<HealthCheckService> _logger;

    public HealthCheckService(IHttpClientFactory factory, ILogger<HealthCheckService> logger)
    {
        _factory = factory ?? throw new ArgumentNullException(nameof(factory));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<bool> CheckApiHealthAsync(string? overrideBaseUrl = null)
    {
        try
        {
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(3));
            var client = _factory.CreateClient("ApiClient");
            if (!string.IsNullOrWhiteSpace(overrideBaseUrl))
                client.BaseAddress = new Uri(overrideBaseUrl);

            var resp = await client.GetAsync("health", cts.Token);
            _logger.LogInformation("Health check returned {Status}", resp.StatusCode);
            return resp.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Health check failed");
            return false;
        }
    }
}
#endif
