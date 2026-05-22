namespace Ssomero.Api.Services.Interfaces;

/// <summary>
/// API-level distributed cache abstraction. All methods are fire-and-forget safe:
/// a cache failure never propagates to the caller — it is logged and swallowed.
/// </summary>
public interface IApiCacheService
{
    /// <summary>Returns the cached value, or <c>default</c> on miss or failure.</summary>
    Task<T?> GetAsync<T>(string key);

    /// <summary>Stores a value with the specified TTL. Silently skips on failure.</summary>
    Task SetAsync<T>(string key, T value, TimeSpan ttl);

    /// <summary>Removes a single cache entry. Silently skips on failure.</summary>
    Task RemoveAsync(string key);

    /// <summary>Removes multiple cache entries atomically (best-effort).</summary>
    Task RemoveManyAsync(params string[] keys);
}
