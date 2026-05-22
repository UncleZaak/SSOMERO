using Microsoft.AspNetCore.Hosting;
using Ssomero.Api.Services.Interfaces;

namespace Ssomero.Api.Services.Implementations;

/// <summary>
/// Stores uploaded files on the local file system under wwwroot/uploads/avatars/.
/// Intended for development and testing only.
/// </summary>
public sealed class LocalFileStorageService : IFileStorageService
{
    // Allowed extensions mapped from MIME types validated upstream.
    // This is a second-line defence — the controller validates MIME before calling us.
    private static readonly HashSet<string> AllowedExtensions =
        new(StringComparer.OrdinalIgnoreCase) { ".jpg", ".jpeg", ".png", ".webp" };

    private readonly IWebHostEnvironment _env;
    private readonly ILogger<LocalFileStorageService> _logger;

    public LocalFileStorageService(
        IWebHostEnvironment env,
        ILogger<LocalFileStorageService> logger)
    {
        _env    = env;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<string> UploadAsync(
        Stream stream,
        string fileName,
        string contentType,
        CancellationToken ct = default)
    {
        // Sanitise extension — never trust the caller-supplied file name blindly.
        var ext = SanitiseExtension(Path.GetExtension(fileName));

        // Generate an unpredictable storage name to prevent enumeration.
        var storedName = $"{Guid.NewGuid():N}{ext}";

        // Resolve absolute path, guarding against path traversal.
        var avatarDir = Path.Combine(_env.WebRootPath, "uploads", "avatars");
        Directory.CreateDirectory(avatarDir);

        var fullPath = Path.GetFullPath(Path.Combine(avatarDir, storedName));

        // Guard: full path must be inside the avatar directory.
        if (!fullPath.StartsWith(Path.GetFullPath(avatarDir), StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("Path traversal attempt detected.");

        await using var fs = new FileStream(fullPath, FileMode.Create, FileAccess.Write,
            FileShare.None, bufferSize: 81920, useAsync: true);
        await stream.CopyToAsync(fs, ct);

        var relativeUrl = $"/uploads/avatars/{storedName}";
        _logger.LogInformation("Avatar saved locally: {Path}", fullPath);
        return relativeUrl;
    }

    /// <inheritdoc />
    public Task DeleteAsync(string fileUrl, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(fileUrl) || !IsLocalPath(fileUrl))
            return Task.CompletedTask;

        // Strip the leading slash and resolve from WebRootPath.
        var relative  = fileUrl.TrimStart('/');
        var fullPath  = Path.GetFullPath(Path.Combine(_env.WebRootPath, relative));
        var avatarDir = Path.GetFullPath(Path.Combine(_env.WebRootPath, "uploads", "avatars"));

        // Only delete files that live inside the expected directory.
        if (!fullPath.StartsWith(avatarDir, StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogWarning("DeleteAsync: path traversal guard triggered for {Url}", fileUrl);
            return Task.CompletedTask;
        }

        if (File.Exists(fullPath))
        {
            File.Delete(fullPath);
            _logger.LogInformation("Avatar deleted: {Path}", fullPath);
        }

        return Task.CompletedTask;
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    /// <summary>Returns true when the URL is a relative local path.</summary>
    internal static bool IsLocalPath(string url) =>
        url.StartsWith('/') && !url.StartsWith("//");

    private static string SanitiseExtension(string ext)
    {
        if (string.IsNullOrWhiteSpace(ext)) return ".jpg";

        // Normalise to lower-case and strip anything suspicious.
        var clean = ext.ToLowerInvariant();
        return AllowedExtensions.Contains(clean) ? clean : ".jpg";
    }
}
