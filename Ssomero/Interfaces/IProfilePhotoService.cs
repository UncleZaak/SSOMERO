namespace Ssomero.Interfaces;

/// <summary>
/// Handles profile photo picking, compression, upload and removal.
/// All camera/gallery interaction is isolated here — pages never touch MediaPicker directly.
/// </summary>
public interface IProfilePhotoService
{
    /// <summary>Opens the system photo gallery. Returns local file path or null on cancel.</summary>
    Task<string?> PickFromGalleryAsync();

    /// <summary>Opens the device camera. Returns local file path or null on cancel/unavailable.</summary>
    Task<string?> CapturePhotoAsync();

    /// <summary>
    /// Uploads the image at <paramref name="localFilePath"/> to POST /api/profile/photo.
    /// On success, refreshes <see cref="ITopBarService"/> immediately and returns the new photo URL.
    /// Returns null on failure.
    /// </summary>
    Task<string?> UploadAsync(string localFilePath, CancellationToken ct = default);

    /// <summary>
    /// Removes the current profile photo.
    /// Refreshes <see cref="ITopBarService"/> to fall back to initials.
    /// </summary>
    Task RemoveAsync(CancellationToken ct = default);
}
