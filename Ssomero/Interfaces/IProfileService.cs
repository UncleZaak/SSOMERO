using System.Threading;
using System.Threading.Tasks;
using Ssomero.Models;

namespace Ssomero.Interfaces;

public interface IProfileService
{
    Task<ProfileDto?> GetProfileAsync(CancellationToken ct = default);
    Task<bool> UpdateProfileAsync(UpdateProfileRequest dto, CancellationToken ct = default);
    /// <summary>Returns null on success; an error message string on failure.</summary>
    Task<string?> ChangePasswordAsync(ChangePasswordRequest dto, CancellationToken ct = default);
}
