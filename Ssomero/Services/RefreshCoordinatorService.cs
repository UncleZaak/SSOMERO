using Ssomero.Interfaces;

namespace Ssomero.Services;

public class RefreshCoordinatorService : IRefreshCoordinator
{
    private readonly Dictionary<string, List<Func<Task>>> _handlers = new();
    private readonly Lock _lock = new();

    public void Subscribe(string key, Func<Task> callback)
    {
        lock (_lock)
        {
            if (!_handlers.TryGetValue(key, out var list))
            {
                list = [];
                _handlers[key] = list;
            }
            if (!list.Contains(callback))
                list.Add(callback);
        }
    }

    public void Unsubscribe(string key, Func<Task> callback)
    {
        lock (_lock)
        {
            if (_handlers.TryGetValue(key, out var list))
                list.Remove(callback);
        }
    }

    public async Task NotifyAsync(string key)
    {
        List<Func<Task>> handlers;
        lock (_lock)
        {
            if (!_handlers.TryGetValue(key, out var list)) return;
            handlers = [.. list];
        }

        foreach (var h in handlers)
        {
            try { await h(); }
            catch (Exception ex)
            {
                // One failing subscriber must not block the rest
                System.Diagnostics.Debug.WriteLine($"[RefreshCoordinator] Handler for '{key}' threw: {ex.Message}");
            }
        }
    }
}
