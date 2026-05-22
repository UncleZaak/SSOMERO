using System.Net.Http.Json;
using Microsoft.Extensions.Logging;
using Ssomero.Interfaces;
using Ssomero.Models;

namespace Ssomero.Services;

public class StudentScheduleService : IStudentScheduleService
{
    private readonly IApiService _api;
    private readonly ICacheService _cache;
    private readonly ILogger<StudentScheduleService> _logger;

    private static readonly TimeSpan CacheExpiry = TimeSpan.FromMinutes(15);

    public StudentScheduleService(IApiService api, ICacheService cache, ILogger<StudentScheduleService> logger)
    {
        _api    = api;
        _cache  = cache;
        _logger = logger;
    }

    public async Task<IReadOnlyList<ClassSessionDto>> GetWeekScheduleAsync(
        DateOnly? from = null,
        DateOnly? to = null,
        bool forceRefresh = false,
        CancellationToken ct = default)
    {
        var start = from ?? GetMonday(DateOnly.FromDateTime(DateTime.Now));
        var end   = to   ?? start.AddDays(6);
        var cacheKey = $"schedule:{start:yyyyMMdd}:{end:yyyyMMdd}";

        if (!forceRefresh)
        {
            var cached = _cache.Get<IReadOnlyList<ClassSessionDto>>(cacheKey);
            if (cached is not null) return cached;
        }

        try
        {
            var url  = $"student/schedule?from={start:yyyy-MM-dd}&to={end:yyyy-MM-dd}";
            var resp = await _api.GetAsync(url, ct);

            if (!resp.IsSuccessStatusCode)
            {
                _logger.LogWarning("GetWeekSchedule returned {Status}", resp.StatusCode);
                return [];
            }

            var envelope = await resp.Content.ReadFromJsonAsync<ScheduleEnvelope>(cancellationToken: ct);
            var sessions  = envelope?.Sessions
                            .Select(s => new ClassSessionDto
                            {
                                SessionId  = s.SessionId,
                                ClassId    = s.ClassId,
                                CourseName = s.CourseName,
                                CourseCode = s.CourseCode,
                                StartTime  = s.StartTime,
                                EndTime    = s.EndTime,
                                Location   = s.Location ?? string.Empty,
                                Lecturer   = s.Lecturer ?? string.Empty
                            })
                            .ToList()
                            as IReadOnlyList<ClassSessionDto>
                ?? [];

            _cache.Set(cacheKey, sessions, CacheExpiry);
            return sessions;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load week schedule");
            return [];
        }
    }

    public async Task<ClassSessionDto?> GetCurrentSessionAsync(CancellationToken ct = default)
    {
        var sessions = await GetWeekScheduleAsync(ct: ct);
        var now = DateTime.Now;
        return sessions.FirstOrDefault(s => s.StartTime <= now && s.EndTime >= now);
    }

    public async Task<ClassSessionDto?> GetNextSessionAsync(CancellationToken ct = default)
    {
        var sessions = await GetWeekScheduleAsync(ct: ct);
        var now = DateTime.Now;
        return sessions
            .Where(s => s.StartTime > now && s.StartTime.Date == now.Date)
            .MinBy(s => s.StartTime);
    }

    private static DateOnly GetMonday(DateOnly date)
    {
        int diff = (7 + (date.DayOfWeek - DayOfWeek.Monday)) % 7;
        return date.AddDays(-diff);
    }

    // Local deserialization shape to match API response envelope
    private sealed record ScheduleEnvelope(IEnumerable<SessionItem> Sessions);
    private sealed record SessionItem(
        Guid SessionId, Guid ClassId, string CourseName, string? CourseCode,
        DateTime StartTime, DateTime EndTime, string? Location, string? Lecturer);
}
