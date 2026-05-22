using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Ssomero.Interfaces;
using Ssomero.Models;

namespace Ssomero.Services;

public class AttendanceService : IAttendanceService
{
    private readonly IApiService _api;
    private readonly ILogger<AttendanceService> _logger;

    public AttendanceService(IApiService api, ILogger<AttendanceService> logger)
    {
        _api    = api;
        _logger = logger;
    }

    public async Task<StudentAttendanceReportDto?> GetMyReportAsync(CancellationToken ct = default)
    {
        var resp = await _api.GetAsync("attendance/my-report", ct);
        if (!resp.IsSuccessStatusCode)
        {
            _logger.LogWarning("GetMyReport returned {StatusCode}", resp.StatusCode);
            return null;
        }
        return await resp.Content.ReadFromJsonAsync<StudentAttendanceReportDto>(cancellationToken: ct);
    }

    public async Task<AttendanceMarkResult> MarkAttendanceAsync(
        Guid sessionId,
        double? latitude,
        double? longitude,
        Stream? selfieStream,
        string? selfieFileName,
        CancellationToken ct = default)
    {
        try
        {
            using var content = new MultipartFormDataContent();
            content.Add(JsonContent.Create(new
            {
                sessionId,
                timestamp = DateTime.UtcNow,
                latitude,
                longitude
            }), "json");

            if (selfieStream is not null && selfieFileName is not null)
                content.Add(new StreamContent(selfieStream), "selfie", selfieFileName);

            // Fall back to JSON body if no selfie (simpler path)
            HttpResponseMessage resp;
            if (selfieStream is null)
            {
                resp = await _api.PostAsync("attendance/mark",
                    JsonContent.Create(new
                    {
                        sessionId,
                        timestamp = DateTime.UtcNow,
                        latitude,
                        longitude
                    }), ct);
            }
            else
            {
                resp = await _api.PostAsync("attendance/mark", content, ct);
            }

            if (resp.IsSuccessStatusCode)
            {
                var body = await resp.Content.ReadFromJsonAsync<JsonElement>(cancellationToken: ct);
                var id   = body.TryGetProperty("attendanceId", out var idProp)
                           ? idProp.GetGuid()
                           : (Guid?)null;
                return new AttendanceMarkResult(true, null, id);
            }

            var error = await resp.Content.ReadAsStringAsync(ct);
            _logger.LogWarning("MarkAttendance {Status}: {Body}", resp.StatusCode, error);

            // Parse structured error from API
            string message;
            try
            {
                var doc = JsonDocument.Parse(error);
                message = doc.RootElement.TryGetProperty("error", out var errProp)
                    ? errProp.GetString() ?? resp.ReasonPhrase ?? "Failed"
                    : resp.ReasonPhrase ?? "Failed";
            }
            catch { message = resp.ReasonPhrase ?? "Attendance submission failed."; }

            return new AttendanceMarkResult(false, message, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "MarkAttendanceAsync threw");
            return new AttendanceMarkResult(false, ex.Message, null);
        }
    }

    public async Task<IReadOnlyList<AttendanceRecordDto>> GetHistoryAsync(CancellationToken ct = default)
    {
        var resp = await _api.GetAsync("attendance/my-history", ct);
        if (!resp.IsSuccessStatusCode)
        {
            _logger.LogWarning("GetHistory returned {StatusCode}", resp.StatusCode);
            return [];
        }
        return await resp.Content.ReadFromJsonAsync<List<AttendanceRecordDto>>(cancellationToken: ct) ?? [];
    }
}
