using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using Ssomero.Api.Services.Interfaces;

namespace Ssomero.Api.Services.Implementations;

/// <summary>
/// <see cref="IApiCacheService"/> backed by <see cref="IDistributedCache"/>.
/// Uses the same cache instance registered in DI — in-memory for dev, Redis for production.
/// Every public method catches and logs cache exceptions so a cache outage never
/// brings down the API.
/// </summary>
public sealed class ApiCacheService : IApiCacheService
{
    private static readonly JsonSerializerOptions _opts = new(JsonSerializerDefaults.Web);
    private readonly IDistributedCache _cache;
    private readonly ILogger<ApiCacheService> _logger;

    public ApiCacheService(IDistributedCache cache, ILogger<ApiCacheService> logger)
    {
        _cache = cache;
        _logger = logger;
    }

    public async Task<T?> GetAsync<T>(string key)
    {
        try
        {
            var json = await _cache.GetStringAsync(key);
            if (json is null) return default;
            return JsonSerializer.Deserialize<T>(json, _opts);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Cache read failed for key {Key} — proceeding without cache", key);
            return default;
        }
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan ttl)
    {
        try
        {
            var json = JsonSerializer.Serialize(value, _opts);
            await _cache.SetStringAsync(key, json,
                new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = ttl });
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Cache write failed for key {Key} — response served from DB", key);
        }
    }

    public async Task RemoveAsync(string key)
    {
        try
        {
            await _cache.RemoveAsync(key);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Cache eviction failed for key {Key}", key);
        }
    }

    public async Task RemoveManyAsync(params string[] keys)
    {
        // Execute removals concurrently; tolerate individual failures
        var tasks = keys.Select(k => RemoveAsync(k));
        await Task.WhenAll(tasks);
    }
}
