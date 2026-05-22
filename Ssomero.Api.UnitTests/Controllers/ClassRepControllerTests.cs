using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Ssomero.Api.Controllers;
using Ssomero.Api.Dtos;
using Ssomero.Api.Services.Interfaces;
using System.Security.Claims;

namespace Ssomero.Api.Controllers.UnitTests;

[TestClass]
public class ClassRepControllerTests
{
    private static readonly Guid UserId = Guid.NewGuid();

    private static ClassRepController CreateController(IClassRepService service)
    {
        var controller = new ClassRepController(service, Mock.Of<Microsoft.Extensions.Logging.ILogger<ClassRepController>>());
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, UserId.ToString()),
            new Claim(ClaimTypes.Role, "ClassRepresentative"),
        };
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(claims, "Test"))
            }
        };
        return controller;
    }

    // ── GetMyClass ───────────────────────────────────────────────────────────

    [TestMethod]
    public async Task GetMyClass_ReturnsOk_WhenClassFound()
    {
        var dto = new ClassRepMyClassDto(Guid.NewGuid(), "CS Year 1", "BSc CS", 30, 2, 3);
        var svc = new Mock<IClassRepService>();
        svc.Setup(s => s.GetMyClassAsync(UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(dto);

        var result = await CreateController(svc.Object).GetMyClass(CancellationToken.None);

        Assert.IsInstanceOfType<OkObjectResult>(result);
        Assert.AreEqual(dto, ((OkObjectResult)result).Value);
    }

    [TestMethod]
    public async Task GetMyClass_ReturnsNotFound_WhenNoClass()
    {
        var svc = new Mock<IClassRepService>();
        svc.Setup(s => s.GetMyClassAsync(UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((ClassRepMyClassDto?)null);

        var result = await CreateController(svc.Object).GetMyClass(CancellationToken.None);

        Assert.IsInstanceOfType<NotFoundObjectResult>(result);
    }

    // ── GetSubclasses ────────────────────────────────────────────────────────

    [TestMethod]
    public async Task GetSubclasses_ReturnsOkWithList()
    {
        var list = new List<ClassRepSubclassDto>
        {
            new(Guid.NewGuid(), "Group A", null, 15, 1, DateTime.UtcNow),
        };
        var svc = new Mock<IClassRepService>();
        svc.Setup(s => s.GetSubclassesAsync(UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(list);

        var result = await CreateController(svc.Object).GetSubclasses(CancellationToken.None);

        Assert.IsInstanceOfType<OkObjectResult>(result);
    }

    // ── CreateSubclass ───────────────────────────────────────────────────────

    [TestMethod]
    public async Task CreateSubclass_ReturnsCreated_WhenSuccessful()
    {
        var sub = new ClassRepSubclassDto(Guid.NewGuid(), "Group B", null, 0, 0, DateTime.UtcNow);
        var svc = new Mock<IClassRepService>();
        svc.Setup(s => s.CreateSubclassAsync(UserId, It.IsAny<CreateSubclassDto>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(sub);

        var result = await CreateController(svc.Object).CreateSubclass(new CreateSubclassDto("Group B", null), CancellationToken.None);

        Assert.IsInstanceOfType<CreatedAtActionResult>(result);
    }

    [TestMethod]
    public async Task CreateSubclass_ReturnsBadRequest_WhenDuplicateName()
    {
        var svc = new Mock<IClassRepService>();
        svc.Setup(s => s.CreateSubclassAsync(UserId, It.IsAny<CreateSubclassDto>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("duplicate"));

        var result = await CreateController(svc.Object).CreateSubclass(new CreateSubclassDto("Group A", null), CancellationToken.None);

        Assert.IsInstanceOfType<BadRequestObjectResult>(result);
    }

    // ── RenameSubclass ───────────────────────────────────────────────────────

    [TestMethod]
    public async Task RenameSubclass_ReturnsNotFound_WhenNotOwned()
    {
        var svc = new Mock<IClassRepService>();
        svc.Setup(s => s.RenameSubclassAsync(UserId, It.IsAny<Guid>(), It.IsAny<RenameSubclassDto>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ClassRepSubclassDto?)null);

        var result = await CreateController(svc.Object).RenameSubclass(Guid.NewGuid(), new RenameSubclassDto("X"), CancellationToken.None);

        Assert.IsInstanceOfType<NotFoundObjectResult>(result);
    }

    // ── RemoveStudent ────────────────────────────────────────────────────────

    [TestMethod]
    public async Task RemoveStudent_ReturnsNoContent_WhenSuccessful()
    {
        var svc = new Mock<IClassRepService>();
        svc.Setup(s => s.RemoveStudentAsync(UserId, It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var result = await CreateController(svc.Object).RemoveStudent(Guid.NewGuid(), Guid.NewGuid(), CancellationToken.None);

        Assert.IsInstanceOfType<NoContentResult>(result);
    }

    [TestMethod]
    public async Task RemoveStudent_ReturnsNotFound_WhenNotOwned()
    {
        var svc = new Mock<IClassRepService>();
        svc.Setup(s => s.RemoveStudentAsync(UserId, It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var result = await CreateController(svc.Object).RemoveStudent(Guid.NewGuid(), Guid.NewGuid(), CancellationToken.None);

        Assert.IsInstanceOfType<NotFoundObjectResult>(result);
    }

    // ── AssignLecturer ───────────────────────────────────────────────────────

    [TestMethod]
    public async Task AssignLecturer_ReturnsNoContent_WhenSuccessful()
    {
        var svc = new Mock<IClassRepService>();
        svc.Setup(s => s.AssignLecturerAsync(UserId, It.IsAny<Guid>(), It.IsAny<AssignLecturerDto>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var result = await CreateController(svc.Object).AssignLecturer(Guid.NewGuid(), new AssignLecturerDto(Guid.NewGuid()), CancellationToken.None);

        Assert.IsInstanceOfType<NoContentResult>(result);
    }

    [TestMethod]
    public async Task AssignLecturer_ReturnsBadRequest_WhenFailed()
    {
        var svc = new Mock<IClassRepService>();
        svc.Setup(s => s.AssignLecturerAsync(UserId, It.IsAny<Guid>(), It.IsAny<AssignLecturerDto>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var result = await CreateController(svc.Object).AssignLecturer(Guid.NewGuid(), new AssignLecturerDto(Guid.NewGuid()), CancellationToken.None);

        Assert.IsInstanceOfType<BadRequestObjectResult>(result);
    }

    // ── Stats ────────────────────────────────────────────────────────────────

    [TestMethod]
    public async Task GetStats_ReturnsOk()
    {
        var stats = new ClassRepStatsDto(1, 30, 2, 3, 85.5);
        var svc = new Mock<IClassRepService>();
        svc.Setup(s => s.GetStatsAsync(UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(stats);

        var result = await CreateController(svc.Object).GetStats(CancellationToken.None);

        Assert.IsInstanceOfType<OkObjectResult>(result);
        Assert.AreEqual(stats, ((OkObjectResult)result).Value);
    }

    // ── AttendanceSummary ────────────────────────────────────────────────────

    [TestMethod]
    public async Task GetAttendanceSummary_ReturnsOk()
    {
        var summary = new ClassRepAttendanceSummaryDto(72.5, 40, 29);
        var svc = new Mock<IClassRepService>();
        svc.Setup(s => s.GetAttendanceSummaryAsync(UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(summary);

        var result = await CreateController(svc.Object).GetAttendanceSummary(CancellationToken.None);

        Assert.IsInstanceOfType<OkObjectResult>(result);
    }
}
