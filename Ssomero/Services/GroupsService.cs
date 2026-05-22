using System.Net.Http.Json;
using Microsoft.Extensions.Logging;
using Ssomero.Interfaces;
using Ssomero.Models;

namespace Ssomero.Services;

public class GroupsService : IGroupsService
{
    private readonly IApiService _api;
    private readonly ILogger<GroupsService> _logger;

    public GroupsService(IApiService api, ILogger<GroupsService> logger)
    {
        _api = api;
        _logger = logger;
    }

    public async Task<IEnumerable<StudyGroupDto>> GetGroupsAsync(CancellationToken ct = default)
    {
        var resp = await _api.GetAsync("groups", ct);
        if (!resp.IsSuccessStatusCode)
        {
            _logger.LogWarning("GetGroups returned {StatusCode}", resp.StatusCode);
            return [];
        }
        return await resp.Content.ReadFromJsonAsync<IEnumerable<StudyGroupDto>>(cancellationToken: ct) ?? [];
    }

    public async Task<IEnumerable<GroupMessageDto>> GetGroupMessagesAsync(string groupId, CancellationToken ct = default)
    {
        var resp = await _api.GetAsync($"groups/{groupId}/messages", ct);
        if (!resp.IsSuccessStatusCode)
        {
            _logger.LogWarning("GetGroupMessages({Id}) returned {StatusCode}", groupId, resp.StatusCode);
            return [];
        }
        return await resp.Content.ReadFromJsonAsync<IEnumerable<GroupMessageDto>>(cancellationToken: ct) ?? [];
    }

    public async Task<bool> SendMessageAsync(string groupId, string text, CancellationToken ct = default)
    {
        var resp = await _api.PostAsync($"groups/{groupId}/messages",
            JsonContent.Create(new { text }), ct);
        return resp.IsSuccessStatusCode;
    }
}
