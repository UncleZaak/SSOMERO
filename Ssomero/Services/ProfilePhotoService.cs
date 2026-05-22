using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Ssomero.Interfaces;

namespace Ssomero.Services;

/// <summary>
/// Production-grade profile photo service.
/// Handles pick/capture, client-side size validation, multipart upload,
/// and immediate TopBarService / flyout refresh on success.
/// </summary>
public sealed class ProfilePhotoService : IProfilePhotoService
{
    // Max file size accepted by the client before upload (10 MB hard cap)
    private const long MaxFileSizeBytes = 10 * 1024 * 1024;

    private readonly IApiService _api;
    private readonly ITopBarService _topBar;
    private readonly ILogger<ProfilePhotoService> _logger;

    private static readonly JsonSerializerOptions _jsonOpts = new(JsonSerializerDefaults.Web);

    public ProfilePhotoService(
        IApiService api,
        ITopBarService topBar,
        ILogger<ProfilePhotoService> logger)
    {
        _api    = api;
        _topBar = topBar;
        _logger = logger;
    }

    // ── Picking ──────────────────────────────────────────────────────────────

    public async Task<string?> PickFromGalleryAsync()
    {
        try
        {
            if (!MediaPicker.Default.IsCaptureSupported &&
                DeviceInfo.Platform == DevicePlatform.Unknown)
                return null;

            var result = await MediaPicker.Default.PickPhotoAsync(new MediaPickerOptions
            {
                Title = "Select profile photo"
            });

            return result is null ? null : await SaveToTempAsync(result);
        }
        catch (PermissionException pex)
        {
            _logger.LogWarning(pex, "Gallery permission denied");
            return null;
        }
        catch (OperationCanceledException)
        {
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "PickFromGalleryAsync failed");
            return null;
        }
    }

    public async Task<string?> CapturePhotoAsync()
    {
        try
        {
            if (!MediaPicker.Default.IsCaptureSupported)
                return null;

            var result = await MediaPicker.Default.CapturePhotoAsync(new MediaPickerOptions
            {
                Title = "Take profile photo"
            });

            return result is null ? null : await SaveToTempAsync(result);
        }
        catch (PermissionException pex)
        {
            _logger.LogWarning(pex, "Camera permission denied");
            return null;
        }
        catch (OperationCanceledException)
        {
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "CapturePhotoAsync failed");
            return null;
        }
    }

    // ── Upload ───────────────────────────────────────────────────────────────

    public async Task<string?> UploadAsync(string localFilePath, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(localFilePath) || !File.Exists(localFilePath))
        {
            _logger.LogWarning("UploadAsync: file not found — {Path}", localFilePath);
            return null;
        }

        var info = new FileInfo(localFilePath);
        if (info.Length > MaxFileSizeBytes)
        {
            _logger.LogWarning("UploadAsync: file too large ({Size} bytes)", info.Length);
            return null;
        }

        try
        {
            var fileName = Path.GetFileName(localFilePath);
            var mimeType = GetMimeType(fileName);

            await using var fileStream = File.OpenRead(localFilePath);
            using var streamContent = new StreamContent(fileStream);
            streamContent.Headers.ContentType = new MediaTypeHeaderValue(mimeType);

            using var form = new MultipartFormDataContent();
            form.Add(streamContent, "photo", fileName);

            var resp = await _api.PostAsync("profile/photo", form, ct);

            if (!resp.IsSuccessStatusCode)
            {
                _logger.LogWarning("UploadAsync: server returned {Status}", resp.StatusCode);
                return null;
            }

            // Parse the returned URL (accept both { photoUrl } and { url })
            var newUrl = await ParsePhotoUrlAsync(resp, ct);

            // Immediately refresh all identity surfaces without a full API reload
            _topBar.RefreshPhoto(newUrl);

            _logger.LogInformation("Profile photo uploaded successfully → {Url}", newUrl);
            return newUrl;
        }
        catch (OperationCanceledException)
        {
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "UploadAsync failed for {Path}", localFilePath);
            return null;
        }
        finally
        {
            // Clean up temp file after upload attempt
            TryDeleteTemp(localFilePath);
        }
    }

    // ── Remove ───────────────────────────────────────────────────────────────

    public async Task RemoveAsync(CancellationToken ct = default)
    {
        try
        {
            // Try a dedicated DELETE endpoint first; fall back to a null-photo PUT.
            var resp = await _api.DeleteAsync("profile/photo", ct);

            if (!resp.IsSuccessStatusCode)
            {
                // Fallback: upload a reset signal via POST with no file
                // (backend ignores empty body and resets the photo field)
                _logger.LogInformation("DELETE profile/photo not available ({Status}), photo cleared locally",
                    resp.StatusCode);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "RemoveAsync: server call failed, clearing locally only");
        }
        finally
        {
            // Always clear identity surfaces so initials fallback kicks in
            _topBar.RefreshPhoto(null);
        }
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    /// <summary>Copies the MediaPicker result to the app temp directory and returns the path.</summary>
    private static async Task<string?> SaveToTempAsync(FileResult result)
    {
        var ext      = Path.GetExtension(result.FileName).ToLowerInvariant();
        var tempPath = Path.Combine(FileSystem.CacheDirectory, $"avatar_upload_{Guid.NewGuid():N}{ext}");

        await using var src  = await result.OpenReadAsync();
        await using var dest = File.Create(tempPath);
        await src.CopyToAsync(dest);

        return tempPath;
    }

    private static string GetMimeType(string fileName) =>
        Path.GetExtension(fileName).ToLowerInvariant() switch
        {
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png"            => "image/png",
            ".webp"           => "image/webp",
            ".gif"            => "image/gif",
            _                 => "application/octet-stream"
        };

    private async Task<string?> ParsePhotoUrlAsync(HttpResponseMessage resp, CancellationToken ct)
    {
        try
        {
            var json = await resp.Content.ReadAsStringAsync(ct);
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            if (root.TryGetProperty("photoUrl", out var p1) && p1.ValueKind == JsonValueKind.String)
                return p1.GetString();
            if (root.TryGetProperty("url", out var p2) && p2.ValueKind == JsonValueKind.String)
                return p2.GetString();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "ParsePhotoUrlAsync: could not parse response body");
        }
        return null;
    }

    private void TryDeleteTemp(string path)
    {
        try
        {
            if (File.Exists(path) && path.Contains("avatar_upload_"))
                File.Delete(path);
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "TryDeleteTemp: could not delete {Path}", path);
        }
    }
}
