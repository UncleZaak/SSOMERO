using Ssomero.Models;

namespace Ssomero.Interfaces;

public interface IClassElectionApiService
{
    Task<ClassElectionModel?>  StartElectionAsync(Guid classId, CancellationToken ct = default);
    Task<ClassElectionModel?>  GetActiveElectionAsync(Guid classId, CancellationToken ct = default);
    Task<ClassElectionModel?>  VoteAsync(Guid electionId, Guid candidateStudentId, CancellationToken ct = default);
    Task<ClassElectionModel?>  FinalizeElectionAsync(Guid electionId, CancellationToken ct = default);

    /// <summary>Returns the student's enrolled classes so the ViewModel can auto-resolve the main class when ClassId is not set.</summary>
    Task<List<ClassDto>> GetMyClassesAsync(CancellationToken ct = default);
}
