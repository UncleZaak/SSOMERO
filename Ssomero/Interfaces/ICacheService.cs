namespace Ssomero.Interfaces;

public interface ICacheService
{
    T? Get<T>(string key);
    void Set<T>(string key, T value, TimeSpan expiry);
    void Invalidate(string key);
    void InvalidatePrefix(string prefix);
    void Clear();
}
