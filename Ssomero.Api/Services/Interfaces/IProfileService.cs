using Ssomero.Api.Dtos;

namespace Ssomero.Api.Services.Interfaces;

public interface IProfileService
{
    /// <summary>Returns a role-specific profile DTO for the authenticated user. Returns null when the user record no longer exists.</summary>
    Task<ProfileDto?> GetProfileAsync(Guid userId, string role, CancellationToken ct = default);

    /// <summary>Applies allowed field updates (name, phone, photo). Returns false when the user is not found.</summary>
    Task<bool> UpdateProfileAsync(Guid userId, string role, UpdateProfileDto dto, CancellationToken ct = default);

    /// <summary>
    /// Persists a new photo URL against the correct role-specific entity.
    /// Throws <see cref="InvalidOperationException"/> when the user is not found.
    /// </summary>
    Task UpdatePhotoUrlAsync(Guid userId, string role, string url, CancellationToken ct = default);

    /// <summary>
    /// Verifies <paramref name="dto"/>.CurrentPassword, validates the new password, and re-hashes.
    /// Returns a <see cref="ChangePasswordResult"/> that distinguishes between user-not-found,
    /// wrong-current-password, same-password reuse, and success.
    /// </summary>
    Task<ChangePasswordResult> ChangePasswordAsync(Guid userId, string role, ChangePasswordDto dto, CancellationToken ct = default);
}

public enum ChangePasswordResult
{
    Success,
    UserNotFound,
    WrongCurrentPassword,
    SameAsCurrentPassword
}
