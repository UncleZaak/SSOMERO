using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Ssomero.Api.Controllers;
using Ssomero.Api.Dtos;
using Ssomero.Api.Services.Interfaces;
using System.Security.Claims;

namespace Ssomero.Api.Controllers.UnitTests;

[TestClass]
public class ProfilePhotoControllerTests
{
    // ── Helpers ───────────────────────────────────────────────────────────────

    private static readonly Guid TestUserId = Guid.NewGuid();
    private const string TestRole = "Student";

    private static ProfileController CreateController(
        Mock<IProfileService> profileSvc,
        Mock<IFileStorageService> fileSvc)
    {
        var logger = new Mock<ILogger<ProfileController>>();
        var ctrl   = new ProfileController(profileSvc.Object, fileSvc.Object, logger.Object);

        // Inject a fake authenticated user identity.
        ctrl.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(
                [
                    new Claim(ClaimTypes.NameIdentifier, TestUserId.ToString()),
                    new Claim(ClaimTypes.Role, TestRole)
                ], "TestAuth"))
            }
        };

        return ctrl;
    }

    private static IFormFile MakeFormFile(
        string contentType = "image/jpeg",
        long sizeBytes = 1024,
        string fileName = "avatar.jpg")
    {
        var bytes  = new byte[sizeBytes];
        var stream = new MemoryStream(bytes);
        var file   = new Mock<IFormFile>();
        file.Setup(f => f.ContentType).Returns(contentType);
        file.Setup(f => f.Length).Returns(sizeBytes);
        file.Setup(f => f.FileName).Returns(fileName);
        file.Setup(f => f.OpenReadStream()).Returns(stream);
        return file.Object;
    }

    // ── Tests ─────────────────────────────────────────────────────────────────

    [TestMethod]
    public async Task UploadPhoto_HappyPath_Returns200WithPhotoUrl()
    {
        var profileSvc = new Mock<IProfileService>();
        profileSvc
            .Setup(s => s.GetProfileAsync(TestUserId, TestRole, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new StudentProfileDto { Id = TestUserId, Email = "test@test.com", Role = TestRole });

        var expectedUrl = "/uploads/avatars/abc123.jpg";
        var fileSvc = new Mock<IFileStorageService>();
        fileSvc
            .Setup(s => s.UploadAsync(It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedUrl);

        var ctrl   = CreateController(profileSvc, fileSvc);
        var result = await ctrl.UploadPhoto(MakeFormFile(), CancellationToken.None);

        var ok = result as OkObjectResult;
        Assert.IsNotNull(ok, $"Expected 200 OK, got {result?.GetType().Name}");

        var body = ok.Value as UploadPhotoResponse;
        Assert.IsNotNull(body);
        Assert.AreEqual(expectedUrl, body.PhotoUrl);

        profileSvc.Verify(s => s.UpdatePhotoUrlAsync(TestUserId, TestRole, expectedUrl, It.IsAny<CancellationToken>()), Times.Once);
    }

    [TestMethod]
    public async Task UploadPhoto_NoFile_Returns400()
    {
        var ctrl   = CreateController(new Mock<IProfileService>(), new Mock<IFileStorageService>());
        var result = await ctrl.UploadPhoto(null, CancellationToken.None);

        Assert.IsInstanceOfType<BadRequestObjectResult>(result);
    }

    [TestMethod]
    public async Task UploadPhoto_EmptyFile_Returns400()
    {
        var ctrl   = CreateController(new Mock<IProfileService>(), new Mock<IFileStorageService>());
        var result = await ctrl.UploadPhoto(MakeFormFile(sizeBytes: 0), CancellationToken.None);

        Assert.IsInstanceOfType<BadRequestObjectResult>(result);
    }

    [TestMethod]
    public async Task UploadPhoto_FileTooLarge_Returns400()
    {
        var ctrl = CreateController(new Mock<IProfileService>(), new Mock<IFileStorageService>());

        // 6 MB — exceeds the 5 MB limit
        var result = await ctrl.UploadPhoto(
            MakeFormFile(sizeBytes: 6 * 1024 * 1024), CancellationToken.None);

        var bad = result as BadRequestObjectResult;
        Assert.IsNotNull(bad, $"Expected 400, got {result?.GetType().Name}");
    }

    [TestMethod]
    public async Task UploadPhoto_InvalidMimeType_Returns400()
    {
        var ctrl   = CreateController(new Mock<IProfileService>(), new Mock<IFileStorageService>());
        var result = await ctrl.UploadPhoto(
            MakeFormFile(contentType: "image/gif", fileName: "anim.gif"), CancellationToken.None);

        var bad = result as BadRequestObjectResult;
        Assert.IsNotNull(bad, $"Expected 400 for GIF, got {result?.GetType().Name}");
    }

    [TestMethod]
    public async Task UploadPhoto_ExecutableMimeType_Returns400()
    {
        var ctrl   = CreateController(new Mock<IProfileService>(), new Mock<IFileStorageService>());
        var result = await ctrl.UploadPhoto(
            MakeFormFile(contentType: "application/octet-stream", fileName: "virus.exe"), CancellationToken.None);

        Assert.IsInstanceOfType<BadRequestObjectResult>(result);
    }

    [TestMethod]
    public async Task UploadPhoto_DeletesOldLocalFile_WhenPreviousPhotoExists()
    {
        var oldUrl = "/uploads/avatars/old.jpg";

        var profileSvc = new Mock<IProfileService>();
        profileSvc
            .Setup(s => s.GetProfileAsync(TestUserId, TestRole, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new StudentProfileDto { Id = TestUserId, Email = "t@t.com", Role = TestRole, PhotoUrl = oldUrl });

        var fileSvc = new Mock<IFileStorageService>();
        fileSvc
            .Setup(s => s.UploadAsync(It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("/uploads/avatars/new.jpg");

        var ctrl = CreateController(profileSvc, fileSvc);
        await ctrl.UploadPhoto(MakeFormFile(), CancellationToken.None);

        fileSvc.Verify(s => s.DeleteAsync(oldUrl, It.IsAny<CancellationToken>()), Times.Once);
    }

    [TestMethod]
    public async Task UploadPhoto_DoesNotDeleteOldFile_WhenPreviousPhotoIsExternalUrl()
    {
        var externalUrl = "https://cdn.example.com/avatar.jpg";

        var profileSvc = new Mock<IProfileService>();
        profileSvc
            .Setup(s => s.GetProfileAsync(TestUserId, TestRole, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new StudentProfileDto { Id = TestUserId, Email = "t@t.com", Role = TestRole, PhotoUrl = externalUrl });

        var fileSvc = new Mock<IFileStorageService>();
        fileSvc
            .Setup(s => s.UploadAsync(It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("/uploads/avatars/new.jpg");

        var ctrl = CreateController(profileSvc, fileSvc);
        await ctrl.UploadPhoto(MakeFormFile(), CancellationToken.None);

        // DeleteAsync must NOT be called for external URLs.
        fileSvc.Verify(s => s.DeleteAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
