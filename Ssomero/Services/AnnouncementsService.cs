using System.Collections.Generic;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Ssomero.Interfaces;
using Ssomero.Models;

namespace Ssomero.Services;

public class AnnouncementsService : IAnnouncementsService
{
    private readonly IApiService _api;
    private readonly ILogger<AnnouncementsService> _logger;

    public AnnouncementsService(IApiService api, ILogger<AnnouncementsService> logger)
    {
        _api = api;
        _logger = logger;
    }

    public async Task<IEnumerable<AnnouncementDto>> GetAnnouncementsAsync()
    {
        var resp = await _api.GetAsync("announcements");
        if (!resp.IsSuccessStatusCode)
        {
            _logger.LogWarning("GetAnnouncements returned {StatusCode}", resp.StatusCode);
            return [];
        }
        var list = await resp.Content.ReadFromJsonAsync<IEnumerable<AnnouncementDto>>();
        return list ?? [];
    }
}
