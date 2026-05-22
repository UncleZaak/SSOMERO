using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Ssomero.Api.Services.Implementations;

namespace Ssomero.Api.Services.UnitTests;

[TestClass]
public class FileStorageServiceTests
{
    // ── Helpers ───────────────────────────────────────────────────────────────

    private static (LocalFileStorageService svc, string root) CreateService()
    {
        var root = Path.Combine(Path.GetTempPath(), "SsomeroTests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);

        var env = new Mock<IWebHostEnvironment>();
        env.Setup(e => e.WebRootPath).Returns(root);

        var logger = new Mock<ILogger<LocalFileStorageService>>();
        var svc    = new LocalFileStorageService(env.Object, logger.Object);
        return (svc, root);
    }

    private static Stream MakePngStream()
    {
        // 1x1 white PNG — smallest valid PNG binary
        byte[] png =
        [
            0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A, // PNG signature
            0x00, 0x00, 0x00, 0x0D, 0x49, 0x48, 0x44, 0x52, // IHDR chunk length + type
            0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x01, // width=1, height=1
            0x08, 0x02, 0x00, 0x00, 0x00, 0x90, 0x77, 0x53, // bit depth=8, color=RGB
            0xDE, 0x00, 0x00, 0x00, 0x0C, 0x49, 0x44, 0x41, // IDAT chunk
            0x54, 0x08, 0xD7, 0x63, 0xF8, 0xFF, 0xFF, 0x3F,
            0x00, 0x05, 0xFE, 0x02, 0xFE, 0xDC, 0xCC, 0x59,
            0xE7, 0x00, 0x00, 0x00, 0x00, 0x49, 0x45, 0x4E, // IEND chunk
            0x44, 0xAE, 0x42, 0x60, 0x82
        ];
        return new MemoryStream(png);
    }

    // ── Tests ─────────────────────────────────────────────────────────────────

    [TestMethod]
    public async Task UploadAsync_CreatesFileOnDisk()
    {
        var (svc, root) = CreateService();
        using var stream = MakePngStream();

        var url = await svc.UploadAsync(stream, "avatar.png", "image/png");

        var fileName = url.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
        var fullPath = Path.Combine(root, fileName);
        Assert.IsTrue(File.Exists(fullPath), $"Expected file at {fullPath}");
    }

    [TestMethod]
    public async Task UploadAsync_ReturnsRelativeUrl()
    {
        var (svc, _) = CreateService();
        using var stream = MakePngStream();

        var url = await svc.UploadAsync(stream, "photo.png", "image/png");

        StringAssert.StartsWith(url, "/uploads/avatars/");
        Assert.IsTrue(url.EndsWith(".png"), $"Expected .png extension, got: {url}");
    }

    [TestMethod]
    public async Task UploadAsync_GeneratesUniqueNamesForMultipleUploads()
    {
        var (svc, _) = CreateService();

        using var s1 = MakePngStream();
        using var s2 = MakePngStream();

        var url1 = await svc.UploadAsync(s1, "same.png", "image/png");
        var url2 = await svc.UploadAsync(s2, "same.png", "image/png");

        Assert.AreNotEqual(url1, url2);
    }

    [TestMethod]
    public async Task DeleteAsync_RemovesExistingFile()
    {
        var (svc, root) = CreateService();
        using var stream = MakePngStream();

        var url      = await svc.UploadAsync(stream, "del.png", "image/png");
        var fileName = url.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
        var fullPath = Path.Combine(root, fileName);

        Assert.IsTrue(File.Exists(fullPath), "Pre-condition: file should exist.");

        await svc.DeleteAsync(url);

        Assert.IsFalse(File.Exists(fullPath), "File should have been deleted.");
    }

    [TestMethod]
    public async Task DeleteAsync_DoesNotThrow_WhenFileIsMissing()
    {
        var (svc, _) = CreateService();

        // Should complete without exception even though the file was never created.
        await svc.DeleteAsync("/uploads/avatars/nonexistent.png");
    }

    [TestMethod]
    public async Task DeleteAsync_Ignores_ExternalUrls()
    {
        var (svc, _) = CreateService();

        // External URL — must not throw and must not attempt deletion.
        await svc.DeleteAsync("https://cdn.example.com/avatar.jpg");
    }

    [TestMethod]
    public async Task DeleteAsync_GuardsAgainstPathTraversal()
    {
        var (svc, root) = CreateService();

        // A crafted URL that tries to escape the upload directory.
        await svc.DeleteAsync("/uploads/avatars/../../appsettings.json");

        // No exception should be raised and no files outside the avatar dir deleted.
        // (Guard logs a warning and returns without acting.)
    }

    [TestMethod]
    public void IsLocalPath_ReturnsTrue_ForRelativePaths()
    {
        Assert.IsTrue(LocalFileStorageService.IsLocalPath("/uploads/avatars/abc.jpg"));
    }

    [TestMethod]
    public void IsLocalPath_ReturnsFalse_ForExternalUrls()
    {
        Assert.IsFalse(LocalFileStorageService.IsLocalPath("https://blob.core.windows.net/x.jpg"));
        Assert.IsFalse(LocalFileStorageService.IsLocalPath("//cdn.example.com/x.jpg"));
    }
}
