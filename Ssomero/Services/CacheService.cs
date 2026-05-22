using Ssomero.Interfaces;

namespace Ssomero.Services;

public class CacheService : ICacheService
{
    private readonly record struct CacheEntry(object Value, DateTimeOffset Expires);
    private readonly Dictionary<string, CacheEntry> _store = new();
    private readonly Lock _lock = new();

    public T? Get<T>(string key)
    {
        lock (_lock)
        {
            if (!_store.TryGetValue(key, out var entry)) return default;
            if (DateTimeOffset.UtcNow > entry.Expires)
            {
                _store.Remove(key);
                return default;
            }
            return entry.Value is T typed ? typed : default;
        }
    }

    public void Set<T>(string key, T value, TimeSpan expiry)
    {
        if (value is null) return;
        lock (_lock)
        {
            _store[key] = new CacheEntry(value, DateTimeOffset.UtcNow.Add(expiry));
        }
    }

    public void Invalidate(string key)
    {
        lock (_lock) { _store.Remove(key); }
    }

    public void InvalidatePrefix(string prefix)
    {
        lock (_lock)
        {
            var keys = _store.Keys.Where(k => k.StartsWith(prefix)).ToList();
            foreach (var k in keys) _store.Remove(k);
        }
    }

    public void Clear()
    {
        lock (_lock) { _store.Clear(); }
    }
}
