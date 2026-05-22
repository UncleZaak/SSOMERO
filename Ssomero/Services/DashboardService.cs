using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Ssomero.Interfaces;
using Ssomero.Models;

namespace Ssomero.Services;

public class DashboardService : IDashboardService
{
    private readonly IApiService _api;
    private readonly ICacheService _cache;
    private readonly ILogger<DashboardService> _logger;

    private const string CacheKey = "dashboard:summary";
    private static readonly TimeSpan CacheExpiry = TimeSpan.FromMinutes(2);

    public DashboardService(IApiService api, ICacheService cache, ILogger<DashboardService> logger)
    {
        _api    = api;
        _cache  = cache;
        _logger = logger;
    }

    public async Task<DashboardDto> GetDashboardAsync(bool forceRefresh = false)
    {
        if (!forceRefresh)
        {
            var cached = _cache.Get<DashboardDto>(CacheKey);
            if (cached is not null) return cached;
        }

        var resp = await _api.GetAsync("dashboard");
        if (!resp.IsSuccessStatusCode)
        {
            _logger.LogWarning("GetDashboard returned {StatusCode}", resp.StatusCode);
            return new DashboardDto();
        }
        var dto = await resp.Content.ReadFromJsonAsync<DashboardDto>();
        var result = dto ?? new DashboardDto();
        _cache.Set(CacheKey, result, CacheExpiry);
        return result;
    }
}
