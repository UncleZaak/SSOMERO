using Ssomero.Api.Dtos;

namespace Ssomero.Api.Services.Interfaces;

public interface IClassRepService
{
    Task<ClassRepMyClassDto?> GetMyClassAsync(Guid userId, CancellationToken ct = default);
    Task<IReadOnlyList<ClassRepSubclassDto>> GetSubclassesAsync(Guid userId, CancellationToken ct = default);
    Task<ClassRepSubclassDto> CreateSubclassAsync(Guid userId, CreateSubclassDto dto, CancellationToken ct = default);
    Task<ClassRepSubclassDto?> RenameSubclassAsync(Guid userId, Guid subclassId, RenameSubclassDto dto, CancellationToken ct = default);
    Task<IReadOnlyList<ClassRepStudentDto>> GetStudentsAsync(Guid userId, Guid classId, CancellationToken ct = default);
    Task<bool> RemoveStudentAsync(Guid userId, Guid classId, Guid studentId, CancellationToken ct = default);
    Task<IReadOnlyList<ClassRepLecturerDto>> GetApprovedLecturersAsync(Guid userId, CancellationToken ct = default);
    Task<bool> AssignLecturerAsync(Guid userId, Guid subclassId, AssignLecturerDto dto, CancellationToken ct = default);
    Task<ClassRepAttendanceSummaryDto> GetAttendanceSummaryAsync(Guid userId, CancellationToken ct = default);
    Task<ClassRepStatsDto> GetStatsAsync(Guid userId, CancellationToken ct = default);
}
