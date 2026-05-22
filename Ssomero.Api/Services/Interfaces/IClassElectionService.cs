using Ssomero.Api.Dtos;

namespace Ssomero.Api.Services.Interfaces;

public interface IClassElectionService
{
    Task<ClassElectionDto> StartElectionAsync(Guid userId, StartElectionRequestDto dto, CancellationToken ct = default);
    Task<ClassElectionDto?> GetActiveElectionAsync(Guid userId, Guid classId, CancellationToken ct = default);
    Task<ClassElectionDto> VoteAsync(Guid userId, Guid electionId, VoteRequestDto dto, CancellationToken ct = default);
    Task<ClassElectionDto?> FinalizeElectionAsync(Guid electionId, CancellationToken ct = default);
}
