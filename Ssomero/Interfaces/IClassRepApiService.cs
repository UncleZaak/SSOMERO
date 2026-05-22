using Ssomero.Models;

namespace Ssomero.Interfaces;

public interface IClassRepApiService
{
    Task<ClassRepMyClassModel?> GetMyClassAsync(CancellationToken ct = default);
    Task<List<ClassRepSubclassModel>> GetSubclassesAsync(CancellationToken ct = default);
    Task<ClassRepSubclassModel?> CreateSubclassAsync(CreateSubclassRequest request, CancellationToken ct = default);
    Task<ClassRepSubclassModel?> RenameSubclassAsync(Guid subclassId, RenameSubclassRequest request, CancellationToken ct = default);
    Task<List<ClassRepStudentModel>> GetStudentsAsync(Guid classId, CancellationToken ct = default);
    Task<bool> RemoveStudentAsync(Guid classId, Guid studentId, CancellationToken ct = default);
    Task<List<ClassRepLecturerModel>> GetApprovedLecturersAsync(CancellationToken ct = default);
    Task<bool> AssignLecturerAsync(Guid subclassId, Guid lecturerId, CancellationToken ct = default);
    Task<ClassRepAttendanceSummaryModel?> GetAttendanceSummaryAsync(CancellationToken ct = default);
    Task<ClassRepStatsModel?> GetStatsAsync(CancellationToken ct = default);
}
