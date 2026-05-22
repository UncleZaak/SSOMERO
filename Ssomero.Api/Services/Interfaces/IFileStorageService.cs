namespace Ssomero.Api.Services.Interfaces;

/// <summary>
/// Abstraction over binary file storage (local disk in dev, Azure Blob in production).
/// </summary>
public interface IFileStorageService
{
    /// <summary>
    /// Uploads a file stream and returns the public URL of the stored blob/file.
    /// </summary>
    /// <param name="stream">Readable stream of file data.</param>
    /// <param name="fileName">
    ///     Safe, sanitised file name (extension preserved, original name NEVER trusted).
    /// </param>
    /// <param name="contentType">MIME type, e.g. image/jpeg.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Absolute or relative URL to the stored file.</returns>
    Task<string> UploadAsync(
        Stream stream,
        string fileName,
        string contentType,
        CancellationToken ct = default);

    /// <summary>
    /// Deletes a previously uploaded file identified by its URL.
    /// Implementations must silently ignore missing files — never throw on 404.
    /// </summary>
    /// <param name="fileUrl">The URL returned by <see cref="UploadAsync"/>.</param>
    /// <param name="ct">Cancellation token.</param>
    Task DeleteAsync(string fileUrl, CancellationToken ct = default);
}
