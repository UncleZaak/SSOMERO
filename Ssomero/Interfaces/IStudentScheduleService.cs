using Ssomero.Models;

namespace Ssomero.Interfaces;

public interface IStudentScheduleService
{
    /// <summary>Returns timetable sessions for the given week (defaults to current week).</summary>
    Task<IReadOnlyList<ClassSessionDto>> GetWeekScheduleAsync(
        DateOnly? from = null,
        DateOnly? to = null,
        bool forceRefresh = false,
        CancellationToken ct = default);

    /// <summary>The currently active session, or null if no class is in progress.</summary>
    Task<ClassSessionDto?> GetCurrentSessionAsync(CancellationToken ct = default);

    /// <summary>The next upcoming session today, or null.</summary>
    Task<ClassSessionDto?> GetNextSessionAsync(CancellationToken ct = default);
}
