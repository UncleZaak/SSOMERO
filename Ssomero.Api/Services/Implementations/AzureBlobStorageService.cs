using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Ssomero.Api.Services.Interfaces;

namespace Ssomero.Api.Services.Implementations;

/// <summary>
/// Stores uploaded files in Azure Blob Storage under the "avatars" container.
/// Use in production by setting AzureStorage:ConnectionString in configuration.
/// </summary>
public sealed class AzureBlobStorageService : IFileStorageService
{
    private readonly BlobServiceClient _serviceClient;
    private readonly string _containerName;
    private readonly ILogger<AzureBlobStorageService> _logger;

    public AzureBlobStorageService(
        IConfiguration configuration,
        ILogger<AzureBlobStorageService> logger)
    {
        var connStr = configuration["AzureStorage:ConnectionString"]
            ?? throw new InvalidOperationException(
                "AzureStorage:ConnectionString is required for AzureBlobStorageService.");

        _containerName = configuration["AzureStorage:AvatarContainer"] ?? "avatars";
        _serviceClient = new BlobServiceClient(connStr);
        _logger        = logger;
    }

    /// <inheritdoc />
    public async Task<string> UploadAsync(
        Stream stream,
        string fileName,
        string contentType,
        CancellationToken ct = default)
    {
        var container = await GetContainerAsync(ct);

        // Generate an unpredictable blob name — never use the original file name.
        var ext      = SanitiseExtension(Path.GetExtension(fileName));
        var blobName = $"avatars/{Guid.NewGuid():N}{ext}";

        var blob = container.GetBlobClient(blobName);

        var uploadOpts = new BlobUploadOptions
        {
            HttpHeaders = new BlobHttpHeaders { ContentType = contentType }
        };

        await blob.UploadAsync(stream, uploadOpts, ct);

        var url = blob.Uri.ToString();
        _logger.LogInformation("Avatar uploaded to Azure Blob: {Url}", url);
        return url;
    }

    /// <inheritdoc />
    public async Task DeleteAsync(string fileUrl, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(fileUrl)) return;

        try
        {
            var container = await GetContainerAsync(ct);

            // Extract the blob name from the full URL.
            var uri      = new Uri(fileUrl);
            var blobName = uri.AbsolutePath.TrimStart('/');

            // Strip the container name prefix if present (e.g., /avatars/avatars/xxx → avatars/xxx).
            var containerPrefix = _containerName + "/";
            if (blobName.StartsWith(containerPrefix, StringComparison.OrdinalIgnoreCase))
                blobName = blobName[containerPrefix.Length..];

            var blob = container.GetBlobClient(blobName);
            await blob.DeleteIfExistsAsync(DeleteSnapshotsOption.IncludeSnapshots, cancellationToken: ct);
            _logger.LogInformation("Azure blob deleted: {BlobName}", blobName);
        }
        catch (Exception ex)
        {
            // Deletion failure should never block the main flow.
            _logger.LogWarning(ex, "Failed to delete Azure blob at {Url}", fileUrl);
        }
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private async Task<BlobContainerClient> GetContainerAsync(CancellationToken ct)
    {
        var container = _serviceClient.GetBlobContainerClient(_containerName);

        // Create the container with public blob access if it does not exist yet.
        await container.CreateIfNotExistsAsync(PublicAccessType.Blob, cancellationToken: ct);
        return container;
    }

    private static string SanitiseExtension(string ext)
    {
        if (string.IsNullOrWhiteSpace(ext)) return ".jpg";
        return ext.ToLowerInvariant() switch
        {
            ".jpg" or ".jpeg" => ".jpg",
            ".png"            => ".png",
            ".webp"           => ".webp",
            _                 => ".jpg"
        };
    }
}
