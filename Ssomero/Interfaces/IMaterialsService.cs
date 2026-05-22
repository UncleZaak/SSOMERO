using Ssomero.Models;

namespace Ssomero.Interfaces;

public interface IMaterialsService
{
    Task<IEnumerable<StudyMaterialDto>> GetMaterialsAsync(string? courseId = null, CancellationToken ct = default);
}
