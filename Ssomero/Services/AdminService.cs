using System;
using System.Collections.Generic;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Ssomero.Interfaces;
using Ssomero.Models;

namespace Ssomero.Services;

public class AdminService : IAdminService
{
    private readonly IApiService _api;
    private readonly ILogger<AdminService> _logger;

    public AdminService(IApiService api, ILogger<AdminService> logger)
    {
        _api = api;
        _logger = logger;
    }

    public async Task<List<UserItem>> GetStudentsAsync(CancellationToken ct = default)
    {
        var response = await _api.GetAsync("admin/students", ct);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<List<UserItem>>(ct) ?? [];
    }

    public async Task<List<UserItem>> GetLecturersAsync(CancellationToken ct = default)
    {
        var response = await _api.GetAsync("admin/lecturers", ct);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<List<UserItem>>(ct) ?? [];
    }

    public Task<bool> SuspendStudentAsync(Guid id, CancellationToken ct = default)
        => PostActionAsync($"admin/students/{id}/suspend", ct);

    public Task<bool> ActivateStudentAsync(Guid id, CancellationToken ct = default)
        => PostActionAsync($"admin/students/{id}/activate", ct);

    public Task<bool> DeleteStudentAsync(Guid id, CancellationToken ct = default)
        => PostActionAsync($"admin/students/{id}/delete", ct);

    public Task<bool> SuspendLecturerAsync(Guid id, CancellationToken ct = default)
        => PostActionAsync($"admin/lecturers/{id}/suspend", ct);

    public Task<bool> ActivateLecturerAsync(Guid id, CancellationToken ct = default)
        => PostActionAsync($"admin/lecturers/{id}/activate", ct);

    public Task<bool> DeleteLecturerAsync(Guid id, CancellationToken ct = default)
        => PostActionAsync($"admin/lecturers/{id}/delete", ct);

    public Task<bool> ApproveLecturerAsync(Guid id, CancellationToken ct = default)
        => PostActionAsync($"admin/lecturers/{id}/approve", ct);

    public async Task<AdminStatsDto?> GetAdminStatsAsync(CancellationToken ct = default)
    {
        try
        {
            var resp = await _api.GetAsync("admin/stats", ct);
            if (!resp.IsSuccessStatusCode) return null;
            return await resp.Content.ReadFromJsonAsync<AdminStatsDto>(ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch admin stats");
            return null;
        }
    }

    public async Task<List<ClassDto>> GetAllClassesAsync(string? search = null, CancellationToken ct = default)
    {
        try
        {
            var path = "admin/all-classes";
            if (!string.IsNullOrWhiteSpace(search))
                path += $"?search={Uri.EscapeDataString(search)}";
            var resp = await _api.GetAsync(path, ct);
            if (!resp.IsSuccessStatusCode) return [];
            return await resp.Content.ReadFromJsonAsync<List<ClassDto>>(ct) ?? [];
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch admin classes");
            return [];
        }
    }

    public async Task<List<AdminAttendanceSummaryDto>> GetAttendanceSummaryAsync(CancellationToken ct = default)
    {
        try
        {
            var resp = await _api.GetAsync("admin/attendance/summary", ct);
            if (!resp.IsSuccessStatusCode) return [];
            return await resp.Content.ReadFromJsonAsync<List<AdminAttendanceSummaryDto>>(ct) ?? [];
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch attendance summary");
            return [];
        }
    }

    public async Task<bool> SendNotificationAsync(AdminNotificationRequestDto request, CancellationToken ct = default)
    {
        try
        {
            var json = JsonSerializer.Serialize(new
            {
                title = request.Title,
                body = request.Body,
                targetRole = request.TargetRole,
                targetClassId = request.TargetClassId
            });
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var resp = await _api.PostAsync("admin/notify", content, ct);
            return resp.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send notification");
            return false;
        }
    }

    private async Task<bool> PostActionAsync(string path, CancellationToken ct)
    {
        try
        {
            var response = await _api.PostAsync(path, new StringContent("", Encoding.UTF8, "application/json"), ct);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Admin action failed: {Path}", path);
            return false;
        }
    }

    public async Task<AuditLogPagedResult?> GetAuditLogsAsync(
        int page = 1,
        int pageSize = 20,
        string? action = null,
        string? entity = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        string? search = null,
        CancellationToken ct = default)
    {
        try
        {
            var query = $"admin/audit-logs?page={page}&pageSize={pageSize}";
            if (!string.IsNullOrWhiteSpace(action))  query += $"&action={Uri.EscapeDataString(action)}";
            if (!string.IsNullOrWhiteSpace(entity))  query += $"&entity={Uri.EscapeDataString(entity)}";
            if (!string.IsNullOrWhiteSpace(search))  query += $"&search={Uri.EscapeDataString(search)}";
            if (fromDate.HasValue) query += $"&from={fromDate.Value:yyyy-MM-dd}";
            if (toDate.HasValue)   query += $"&to={toDate.Value:yyyy-MM-dd}";

            var resp = await _api.GetAsync(query, ct);
            if (!resp.IsSuccessStatusCode) return null;
            return await resp.Content.ReadFromJsonAsync<AuditLogPagedResult>(ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch audit logs");
            return null;
        }
    }

    public async Task<AdminTrendsDto?> GetTrendsAsync(
        DateTime from,
        DateTime to,
        string granularity,
        CancellationToken ct = default)
    {
        try
        {
            var url = $"admin/analytics/trends" +
                      $"?from={from:yyyy-MM-dd}" +
                      $"&to={to:yyyy-MM-dd}" +
                      $"&granularity={Uri.EscapeDataString(granularity)}";

            var resp = await _api.GetAsync(url, ct);
            if (!resp.IsSuccessStatusCode) return null;
            return await resp.Content.ReadFromJsonAsync<AdminTrendsDto>(ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch analytics trends");
            return null;
        }
    }
}
