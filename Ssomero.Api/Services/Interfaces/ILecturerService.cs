using Ssomero.Api.Dtos;

namespace Ssomero.Api.Services.Interfaces;

public interface ILecturerService
{
    Task<IEnumerable<LecturerClassDto>> GetLecturerClassesAsync(Guid lecturerId, CancellationToken ct = default);

    /// <returns>Null when lecturer is not assigned to that class.</returns>
    Task<LecturerClassDetailDto?> GetClassDetailsAsync(Guid lecturerId, Guid classId, CancellationToken ct = default);

    /// <returns>Null when lecturer is not assigned to that class.</returns>
    Task<IEnumerable<LecturerStudentDto>?> GetClassStudentsAsync(Guid lecturerId, Guid classId, CancellationToken ct = default);

    /// <returns>Null when lecturer does not own the class that the session belongs to.</returns>
    Task<IEnumerable<SessionAttendanceDto>?> GetSessionAttendanceAsync(Guid lecturerId, Guid sessionId, CancellationToken ct = default);

    Task<(bool Success, string? Error)> MarkAttendanceAsync(Guid lecturerId, LecturerMarkAttendanceDto dto, CancellationToken ct = default);

    Task<(bool Success, string? Error)> UploadMaterialAsync(Guid lecturerId, UploadMaterialDto dto, CancellationToken ct = default);

    /// <returns>Null when lecturer is not assigned to that class.</returns>
    Task<IEnumerable<ClassMaterialDto>?> GetMaterialsAsync(Guid lecturerId, Guid classId, CancellationToken ct = default);
}
