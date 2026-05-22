using System.Net.Http.Json;
using Microsoft.Extensions.Logging;
using Ssomero.Interfaces;
using Ssomero.Models;

namespace Ssomero.Services;

public class LecturerApiService : ILecturerApiService
{
    private readonly IApiService _api;
    private readonly ILogger<LecturerApiService> _logger;

    public LecturerApiService(IApiService api, ILogger<LecturerApiService> logger)
    {
        _api    = api;
        _logger = logger;
    }

    public async Task<List<LecturerClassDto>> GetClassesAsync(CancellationToken ct = default)
    {
        try
        {
            var resp = await _api.GetAsync("lecturer/classes", ct);
            if (!resp.IsSuccessStatusCode) return [];
            return await resp.Content.ReadFromJsonAsync<List<LecturerClassDto>>(cancellationToken: ct) ?? [];
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetClassesAsync failed");
            return [];
        }
    }

    public async Task<LecturerClassDetailDto?> GetClassDetailAsync(Guid classId, CancellationToken ct = default)
    {
        try
        {
            var resp = await _api.GetAsync($"lecturer/classes/{classId}", ct);
            if (!resp.IsSuccessStatusCode) return null;
            return await resp.Content.ReadFromJsonAsync<LecturerClassDetailDto>(cancellationToken: ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetClassDetailAsync failed for {ClassId}", classId);
            return null;
        }
    }

    public async Task<List<LecturerStudentDto>> GetClassStudentsAsync(Guid classId, CancellationToken ct = default)
    {
        try
        {
            var resp = await _api.GetAsync($"lecturer/classes/{classId}/students", ct);
            if (!resp.IsSuccessStatusCode) return [];
            return await resp.Content.ReadFromJsonAsync<List<LecturerStudentDto>>(cancellationToken: ct) ?? [];
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetClassStudentsAsync failed for {ClassId}", classId);
            return [];
        }
    }

    public async Task<List<SessionAttendanceDto>> GetSessionAttendanceAsync(Guid sessionId, CancellationToken ct = default)
    {
        try
        {
            var resp = await _api.GetAsync($"lecturer/sessions/{sessionId}/attendance", ct);
            if (!resp.IsSuccessStatusCode) return [];
            return await resp.Content.ReadFromJsonAsync<List<SessionAttendanceDto>>(cancellationToken: ct) ?? [];
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetSessionAttendanceAsync failed for {SessionId}", sessionId);
            return [];
        }
    }

    public async Task<(bool Success, string? Error)> MarkAttendanceAsync(
        Guid sessionId, Guid studentId, bool isPresent, string? notes, CancellationToken ct = default)
    {
        try
        {
            var resp = await _api.PostAsync(
                $"lecturer/sessions/{sessionId}/attendance",
                JsonContent.Create(new { sessionId, studentId, isPresent, notes }),
                ct);

            if (resp.IsSuccessStatusCode) return (true, null);
            var body = await resp.Content.ReadAsStringAsync(ct);
            return (false, body);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "MarkAttendanceAsync failed");
            return (false, ex.Message);
        }
    }

    public async Task<List<LecturerMaterialDto>> GetMaterialsAsync(Guid classId, CancellationToken ct = default)
    {
        try
        {
            var resp = await _api.GetAsync($"lecturer/classes/{classId}/materials", ct);
            if (!resp.IsSuccessStatusCode) return [];
            return await resp.Content.ReadFromJsonAsync<List<LecturerMaterialDto>>(cancellationToken: ct) ?? [];
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetMaterialsAsync failed for {ClassId}", classId);
            return [];
        }
    }

    public async Task<(bool Success, string? Error)> UploadMaterialAsync(
        Guid classId, string title, string? fileUrl, CancellationToken ct = default)
    {
        try
        {
            var resp = await _api.PostAsync(
                $"lecturer/classes/{classId}/materials",
                JsonContent.Create(new { classId, title, fileUrl }),
                ct);

            if (resp.IsSuccessStatusCode) return (true, null);
            var body = await resp.Content.ReadAsStringAsync(ct);
            return (false, body);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "UploadMaterialAsync failed");
            return (false, ex.Message);
        }
    }
}
