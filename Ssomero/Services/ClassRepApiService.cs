using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Ssomero.Interfaces;
using Ssomero.Models;

namespace Ssomero.Services;

public class ClassRepApiService : IClassRepApiService
{
    private readonly IApiService _api;
    private readonly ILogger<ClassRepApiService> _logger;

    private static readonly JsonSerializerOptions JsonOpts = new(JsonSerializerDefaults.Web);

    public ClassRepApiService(IApiService api, ILogger<ClassRepApiService> logger)
    {
        _api    = api;
        _logger = logger;
    }

    private StringContent Json<T>(T obj) =>
        new(JsonSerializer.Serialize(obj, JsonOpts), Encoding.UTF8, "application/json");

    public async Task<ClassRepMyClassModel?> GetMyClassAsync(CancellationToken ct = default)
    {
        try
        {
            var resp = await _api.GetAsync("classrep/my-class", ct);
            if (!resp.IsSuccessStatusCode) return null;
            return await resp.Content.ReadFromJsonAsync<ClassRepMyClassModel>(JsonOpts, ct);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "GetMyClassAsync failed");
            return null;
        }
    }

    public async Task<List<ClassRepSubclassModel>> GetSubclassesAsync(CancellationToken ct = default)
    {
        try
        {
            var resp = await _api.GetAsync("classrep/subclasses", ct);
            if (!resp.IsSuccessStatusCode) return [];
            return await resp.Content.ReadFromJsonAsync<List<ClassRepSubclassModel>>(JsonOpts, ct) ?? [];
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "GetSubclassesAsync failed");
            return [];
        }
    }

    public async Task<ClassRepSubclassModel?> CreateSubclassAsync(CreateSubclassRequest request, CancellationToken ct = default)
    {
        try
        {
            var resp = await _api.PostAsync("classrep/subclasses", Json(new { name = request.Name, description = request.Description }), ct);
            if (!resp.IsSuccessStatusCode) return null;
            return await resp.Content.ReadFromJsonAsync<ClassRepSubclassModel>(JsonOpts, ct);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "CreateSubclassAsync failed");
            return null;
        }
    }

    public async Task<ClassRepSubclassModel?> RenameSubclassAsync(Guid subclassId, RenameSubclassRequest request, CancellationToken ct = default)
    {
        try
        {
            var resp = await _api.PutAsync($"classrep/subclasses/{subclassId}", Json(new { name = request.Name }), ct);
            if (!resp.IsSuccessStatusCode) return null;
            return await resp.Content.ReadFromJsonAsync<ClassRepSubclassModel>(JsonOpts, ct);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "RenameSubclassAsync failed");
            return null;
        }
    }

    public async Task<List<ClassRepStudentModel>> GetStudentsAsync(Guid classId, CancellationToken ct = default)
    {
        try
        {
            var resp = await _api.GetAsync($"classrep/classes/{classId}/students", ct);
            if (!resp.IsSuccessStatusCode) return [];
            return await resp.Content.ReadFromJsonAsync<List<ClassRepStudentModel>>(JsonOpts, ct) ?? [];
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "GetStudentsAsync failed for {ClassId}", classId);
            return [];
        }
    }

    public async Task<bool> RemoveStudentAsync(Guid classId, Guid studentId, CancellationToken ct = default)
    {
        try
        {
            var resp = await _api.DeleteAsync($"classrep/classes/{classId}/students/{studentId}", ct);
            return resp.IsSuccessStatusCode;
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "RemoveStudentAsync failed");
            return false;
        }
    }

    public async Task<List<ClassRepLecturerModel>> GetApprovedLecturersAsync(CancellationToken ct = default)
    {
        try
        {
            var resp = await _api.GetAsync("classrep/lecturers", ct);
            if (!resp.IsSuccessStatusCode) return [];
            return await resp.Content.ReadFromJsonAsync<List<ClassRepLecturerModel>>(JsonOpts, ct) ?? [];
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "GetApprovedLecturersAsync failed");
            return [];
        }
    }

    public async Task<bool> AssignLecturerAsync(Guid subclassId, Guid lecturerId, CancellationToken ct = default)
    {
        try
        {
            var resp = await _api.PostAsync($"classrep/subclasses/{subclassId}/assign-lecturer", Json(new { lecturerId }), ct);
            return resp.IsSuccessStatusCode;
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "AssignLecturerAsync failed");
            return false;
        }
    }

    public async Task<ClassRepAttendanceSummaryModel?> GetAttendanceSummaryAsync(CancellationToken ct = default)
    {
        try
        {
            var resp = await _api.GetAsync("classrep/attendance/summary", ct);
            if (!resp.IsSuccessStatusCode) return null;
            return await resp.Content.ReadFromJsonAsync<ClassRepAttendanceSummaryModel>(JsonOpts, ct);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "GetAttendanceSummaryAsync failed");
            return null;
        }
    }

    public async Task<ClassRepStatsModel?> GetStatsAsync(CancellationToken ct = default)
    {
        try
        {
            var resp = await _api.GetAsync("classrep/stats", ct);
            if (!resp.IsSuccessStatusCode) return null;
            return await resp.Content.ReadFromJsonAsync<ClassRepStatsModel>(JsonOpts, ct);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "GetStatsAsync failed");
            return null;
        }
    }
}
