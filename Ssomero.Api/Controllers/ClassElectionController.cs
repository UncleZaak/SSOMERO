using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ssomero.Api.Dtos;
using Ssomero.Api.Services.Interfaces;

namespace Ssomero.Api.Controllers;

[ApiController]
[Route("api/class-elections")]
[Authorize(Roles = "Student,ClassRepresentative")]
public class ClassElectionController : ControllerBase
{
    private readonly IClassElectionService _service;
    private readonly ILogger<ClassElectionController> _logger;

    public ClassElectionController(IClassElectionService service, ILogger<ClassElectionController> logger)
    {
        _service = service;
        _logger = logger;
    }

    private Guid CurrentUserId =>
        Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)
                   ?? User.FindFirstValue("sub")
                   ?? throw new InvalidOperationException("User ID claim missing."));

    // ── POST /api/class-elections/start ─────────────────────────────────────

    [HttpPost("start")]
    public async Task<IActionResult> StartElection([FromBody] StartElectionRequestDto dto, CancellationToken ct)
    {
        try
        {
            var result = await _service.StartElectionAsync(CurrentUserId, dto, ct);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    // ── GET /api/class-elections/active/{classId} ────────────────────────────

    [HttpGet("active/{classId:guid}")]
    public async Task<IActionResult> GetActiveElection(Guid classId, CancellationToken ct)
    {
        var result = await _service.GetActiveElectionAsync(CurrentUserId, classId, ct);
        return result is null ? NotFound(new { error = "No active election found for this class." }) : Ok(result);
    }

    // ── POST /api/class-elections/{electionId}/vote ──────────────────────────

    [HttpPost("{electionId:guid}/vote")]
    public async Task<IActionResult> Vote(Guid electionId, [FromBody] VoteRequestDto dto, CancellationToken ct)
    {
        try
        {
            var result = await _service.VoteAsync(CurrentUserId, electionId, dto, ct);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    // ── POST /api/class-elections/{electionId}/finalize ──────────────────────

    [HttpPost("{electionId:guid}/finalize")]
    public async Task<IActionResult> Finalize(Guid electionId, CancellationToken ct)
    {
        var result = await _service.FinalizeElectionAsync(electionId, ct);
        return result is null ? NotFound(new { error = "Election not found." }) : Ok(result);
    }
}
