using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Ssomero.Api.HealthChecks;

/// <summary>
/// Probes the distributed cache by writing and reading back a sentinel value.
/// A failure is reported as <see cref="HealthStatus.Degraded"/> so it does not
/// mark the service as Unhealthy — the app can still operate without the cache.
/// </summary>
public sealed class CacheHealthCheck : IHealthCheck
{
    private const string Key = "health:cache:probe";
    private const string Value = "ok";

    private readonly IDistributedCache _cache;
    private readonly ILogger<CacheHealthCheck> _logger;

    public CacheHealthCheck(IDistributedCache cache, ILogger<CacheHealthCheck> logger)
    {
        _cache = cache;
        _logger = logger;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await _cache.SetStringAsync(Key, Value,
                new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(10) },
                cancellationToken);

            var result = await _cache.GetStringAsync(Key, cancellationToken);
            if (result == Value)
                return HealthCheckResult.Healthy();

            return HealthCheckResult.Degraded("Cache returned unexpected value.");
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Cache health check probe failed");
            return HealthCheckResult.Degraded("Cache is unavailable: " + ex.Message);
        }
    }
}
