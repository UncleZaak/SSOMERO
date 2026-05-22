using Ssomero.Models;

namespace Ssomero.Interfaces;

public interface IAttendanceService
{
    Task<StudentAttendanceReportDto?> GetMyReportAsync(CancellationToken ct = default);

    /// <summary>Submit attendance for a specific session with GPS + optional selfie.</summary>
    Task<AttendanceMarkResult> MarkAttendanceAsync(
        Guid sessionId,
        double? latitude,
        double? longitude,
        Stream? selfieStream,
        string? selfieFileName,
        CancellationToken ct = default);

    /// <summary>Returns the student's attendance history (latest 100 records).</summary>
    Task<IReadOnlyList<AttendanceRecordDto>> GetHistoryAsync(CancellationToken ct = default);
}

public record AttendanceMarkResult(bool Success, string? ErrorMessage, Guid? AttendanceId);
