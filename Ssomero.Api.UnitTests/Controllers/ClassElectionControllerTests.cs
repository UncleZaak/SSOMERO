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
public class ClassElectionControllerTests
{
    private static readonly Guid UserId = Guid.NewGuid();

    private static ClassElectionController CreateController(IClassElectionService service)
    {
        var controller = new ClassElectionController(service, Mock.Of<ILogger<ClassElectionController>>());
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, UserId.ToString()),
            new Claim(ClaimTypes.Role, "Student"),
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

    private static ClassElectionDto MakeDto(Guid? winner = null) => new(
        Guid.NewGuid(), Guid.NewGuid(), "Class A",
        DateTime.UtcNow, DateTime.UtcNow.AddMinutes(1),
        "Active", 55, true, false,
        winner, null,
        []);

    // ── StartElection ────────────────────────────────────────────────────────

    [TestMethod]
    public async Task StartElection_ReturnsOk_WhenSuccessful()
    {
        var dto = MakeDto();
        var svc = new Mock<IClassElectionService>();
        svc.Setup(s => s.StartElectionAsync(UserId, It.IsAny<StartElectionRequestDto>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(dto);

        var result = await CreateController(svc.Object).StartElection(new StartElectionRequestDto(Guid.NewGuid()), CancellationToken.None);

        Assert.IsInstanceOfType<OkObjectResult>(result);
    }

    [TestMethod]
    public async Task StartElection_ReturnsBadRequest_WhenInvalidOperation()
    {
        var svc = new Mock<IClassElectionService>();
        svc.Setup(s => s.StartElectionAsync(UserId, It.IsAny<StartElectionRequestDto>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Not enrolled."));

        var result = await CreateController(svc.Object).StartElection(new StartElectionRequestDto(Guid.NewGuid()), CancellationToken.None);

        Assert.IsInstanceOfType<BadRequestObjectResult>(result);
    }

    // ── GetActiveElection ────────────────────────────────────────────────────

    [TestMethod]
    public async Task GetActiveElection_ReturnsOk_WhenFound()
    {
        var classId = Guid.NewGuid();
        var dto = MakeDto();
        var svc = new Mock<IClassElectionService>();
        svc.Setup(s => s.GetActiveElectionAsync(UserId, classId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(dto);

        var result = await CreateController(svc.Object).GetActiveElection(classId, CancellationToken.None);

        Assert.IsInstanceOfType<OkObjectResult>(result);
    }

    [TestMethod]
    public async Task GetActiveElection_ReturnsNotFound_WhenNoElection()
    {
        var classId = Guid.NewGuid();
        var svc = new Mock<IClassElectionService>();
        svc.Setup(s => s.GetActiveElectionAsync(UserId, classId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((ClassElectionDto?)null);

        var result = await CreateController(svc.Object).GetActiveElection(classId, CancellationToken.None);

        Assert.IsInstanceOfType<NotFoundObjectResult>(result);
    }

    // ── Vote ──────────────────────────────────────────────────────────────────

    [TestMethod]
    public async Task Vote_ReturnsOk_WhenSuccessful()
    {
        var electionId = Guid.NewGuid();
        var dto = MakeDto();
        var svc = new Mock<IClassElectionService>();
        svc.Setup(s => s.VoteAsync(UserId, electionId, It.IsAny<VoteRequestDto>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(dto);

        var result = await CreateController(svc.Object).Vote(electionId, new VoteRequestDto(Guid.NewGuid()), CancellationToken.None);

        Assert.IsInstanceOfType<OkObjectResult>(result);
    }

    [TestMethod]
    public async Task Vote_ReturnsBadRequest_WhenAlreadyVoted()
    {
        var electionId = Guid.NewGuid();
        var svc = new Mock<IClassElectionService>();
        svc.Setup(s => s.VoteAsync(UserId, electionId, It.IsAny<VoteRequestDto>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Already voted."));

        var result = await CreateController(svc.Object).Vote(electionId, new VoteRequestDto(Guid.NewGuid()), CancellationToken.None);

        Assert.IsInstanceOfType<BadRequestObjectResult>(result);
    }

    // ── Finalize ──────────────────────────────────────────────────────────────

    [TestMethod]
    public async Task Finalize_ReturnsOk_WhenSuccessful()
    {
        var electionId = Guid.NewGuid();
        var dto = MakeDto(Guid.NewGuid());
        var svc = new Mock<IClassElectionService>();
        svc.Setup(s => s.FinalizeElectionAsync(electionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(dto);

        var result = await CreateController(svc.Object).Finalize(electionId, CancellationToken.None);

        Assert.IsInstanceOfType<OkObjectResult>(result);
    }

    [TestMethod]
    public async Task Finalize_ReturnsNotFound_WhenElectionMissing()
    {
        var electionId = Guid.NewGuid();
        var svc = new Mock<IClassElectionService>();
        svc.Setup(s => s.FinalizeElectionAsync(electionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((ClassElectionDto?)null);

        var result = await CreateController(svc.Object).Finalize(electionId, CancellationToken.None);

        Assert.IsInstanceOfType<NotFoundObjectResult>(result);
    }
}
