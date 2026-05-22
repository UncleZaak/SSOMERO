using System.Collections.Generic;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Ssomero.Interfaces;
using Ssomero.Models;

namespace Ssomero.Services;

public class ScheduleService : IScheduleService
{
    private readonly IApiService _api;
    private readonly ILogger<ScheduleService> _logger;

    public ScheduleService(IApiService api, ILogger<ScheduleService> logger)
    {
        _api = api;
        _logger = logger;
    }

    public async Task<IEnumerable<ScheduleDto>> GetSchedulesAsync()
    {
        var resp = await _api.GetAsync("schedules");
        if (!resp.IsSuccessStatusCode)
        {
            _logger.LogWarning("GetSchedules returned {StatusCode}", resp.StatusCode);
            return [];
        }
        return await resp.Content.ReadFromJsonAsync<IEnumerable<ScheduleDto>>() ?? [];
    }

    public async Task<bool> CreateScheduleAsync(ScheduleDto schedule)
    {
        var resp = await _api.PostAsync("schedules", JsonContent.Create(schedule));
        if (!resp.IsSuccessStatusCode)
            _logger.LogWarning("CreateSchedule returned {StatusCode}", resp.StatusCode);
        return resp.IsSuccessStatusCode;
    }

    public async Task<bool> CancelScheduleAsync(string scheduleId)
    {
        var resp = await _api.DeleteAsync($"schedules/{scheduleId}");
        if (!resp.IsSuccessStatusCode)
            _logger.LogWarning("CancelSchedule returned {StatusCode}", resp.StatusCode);
        return resp.IsSuccessStatusCode;
    }
}
