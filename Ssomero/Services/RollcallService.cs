using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Ssomero.Interfaces;
using Ssomero.Models;

namespace Ssomero.Services;

public class RollcallService : IRollcallService
{
    private readonly IApiService _api;
    private readonly ILogger<RollcallService> _logger;

    public RollcallService(IApiService api, ILogger<RollcallService> logger)
    {
        _api = api;
        _logger = logger;
    }

    public async Task<IEnumerable<RollcallDto>> GetMyRollcallsAsync()
    {
        var resp = await _api.GetAsync("rollcall/my");
        if (!resp.IsSuccessStatusCode)
        {
            _logger.LogWarning("GetMyRollcalls returned {StatusCode}", resp.StatusCode);
            return [];
        }
        return await resp.Content.ReadFromJsonAsync<IEnumerable<RollcallDto>>() ?? [];
    }

    public async Task<IEnumerable<RollcallDto>> GetPendingApprovalsAsync()
    {
        var resp = await _api.GetAsync("rollcall/pending");
        if (!resp.IsSuccessStatusCode)
        {
            _logger.LogWarning("GetPendingApprovals returned {StatusCode}", resp.StatusCode);
            return [];
        }
        return await resp.Content.ReadFromJsonAsync<IEnumerable<RollcallDto>>() ?? [];
    }

    public async Task<bool> SubmitRollcallAsync(string scheduleId, Stream selfieStream, string fileName)
    {
        using var content = new MultipartFormDataContent();
        content.Add(new StringContent(scheduleId), "scheduleId");
        content.Add(new StreamContent(selfieStream), "selfie", fileName);

        var resp = await _api.PostAsync("rollcall/submit", content);
        if (!resp.IsSuccessStatusCode)
            _logger.LogWarning("SubmitRollcall returned {StatusCode}", resp.StatusCode);
        return resp.IsSuccessStatusCode;
    }

    public async Task<bool> ApproveRollcallAsync(string rollcallId)
    {
        var resp = await _api.PostAsync($"rollcall/{rollcallId}/approve", JsonContent.Create(new { }));
        return resp.IsSuccessStatusCode;
    }

    public async Task<bool> RejectRollcallAsync(string rollcallId)
    {
        var resp = await _api.PostAsync($"rollcall/{rollcallId}/reject", JsonContent.Create(new { }));
        return resp.IsSuccessStatusCode;
    }
}
