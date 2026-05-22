using Ssomero.Models;

namespace Ssomero.Interfaces;

public interface ILecturerApiService
{
    Task<List<LecturerClassDto>> GetClassesAsync(CancellationToken ct = default);
    Task<LecturerClassDetailDto?> GetClassDetailAsync(Guid classId, CancellationToken ct = default);
    Task<List<LecturerStudentDto>> GetClassStudentsAsync(Guid classId, CancellationToken ct = default);
    Task<List<SessionAttendanceDto>> GetSessionAttendanceAsync(Guid sessionId, CancellationToken ct = default);
    Task<(bool Success, string? Error)> MarkAttendanceAsync(Guid sessionId, Guid studentId, bool isPresent, string? notes, CancellationToken ct = default);
    Task<List<LecturerMaterialDto>> GetMaterialsAsync(Guid classId, CancellationToken ct = default);
    Task<(bool Success, string? Error)> UploadMaterialAsync(Guid classId, string title, string? fileUrl, CancellationToken ct = default);
}
