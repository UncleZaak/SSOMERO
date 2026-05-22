using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Ssomero.Interfaces;
using Ssomero.Models;

namespace Ssomero.Services;

public class ClassAnnouncementApiService : IClassAnnouncementApiService
{
    private readonly IApiService _api;
    private readonly ILogger<ClassAnnouncementApiService> _logger;

    private static readonly JsonSerializerOptions JsonOpts = new(JsonSerializerDefaults.Web);

    public ClassAnnouncementApiService(IApiService api, ILogger<ClassAnnouncementApiService> logger)
    {
        _api    = api;
        _logger = logger;
    }

    private StringContent Json<T>(T obj) =>
        new(JsonSerializer.Serialize(obj, JsonOpts), Encoding.UTF8, "application/json");

    public async Task<List<ClassAnnouncementModel>> GetAnnouncementsAsync(CancellationToken ct = default)
    {
        try
        {
            var resp = await _api.GetAsync("classrep/announcements", ct);
            if (!resp.IsSuccessStatusCode) return [];
            return await resp.Content.ReadFromJsonAsync<List<ClassAnnouncementModel>>(JsonOpts, ct) ?? [];
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "GetAnnouncementsAsync failed");
            return [];
        }
    }

    public async Task<ClassAnnouncementModel?> CreateAnnouncementAsync(
        CreateClassAnnouncementRequest request, CancellationToken ct = default)
    {
        try
        {
            var body = Json(new { classId = request.ClassId, title = request.Title, message = request.Message });
            var resp = await _api.PostAsync("classrep/announcements", body, ct);
            if (!resp.IsSuccessStatusCode) return null;
            return await resp.Content.ReadFromJsonAsync<ClassAnnouncementModel>(JsonOpts, ct);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "CreateAnnouncementAsync failed");
            return null;
        }
    }

    public async Task<bool> DeleteAnnouncementAsync(Guid announcementId, CancellationToken ct = default)
    {
        try
        {
            var resp = await _api.DeleteAsync($"classrep/announcements/{announcementId}", ct);
            return resp.IsSuccessStatusCode;
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "DeleteAnnouncementAsync failed");
            return false;
        }
    }

    public async Task<ClassRepAnalyticsModel?> GetAnalyticsAsync(CancellationToken ct = default)
    {
        try
        {
            var resp = await _api.GetAsync("classrep/analytics", ct);
            if (!resp.IsSuccessStatusCode) return null;
            return await resp.Content.ReadFromJsonAsync<ClassRepAnalyticsModel>(JsonOpts, ct);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "GetAnalyticsAsync failed");
            return null;
        }
    }
}
