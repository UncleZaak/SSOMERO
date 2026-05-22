using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ssomero.Api.Dtos;
using Ssomero.Api.Services.Interfaces;

namespace Ssomero.Api.Controllers;

[ApiController]
[Route("api/classrep")]
[Authorize(Roles = "ClassRepresentative")]
public class ClassAnnouncementsController : ControllerBase
{
    private readonly IClassAnnouncementService _service;
    private readonly ILogger<ClassAnnouncementsController> _logger;

    public ClassAnnouncementsController(
        IClassAnnouncementService service,
        ILogger<ClassAnnouncementsController> logger)
    {
        _service = service;
        _logger  = logger;
    }

    private Guid CurrentUserId =>
        Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)
                   ?? User.FindFirstValue("sub")
                   ?? throw new InvalidOperationException("User ID claim missing."));

    // ── GET /api/classrep/announcements ──────────────────────────────────────

    [HttpGet("announcements")]
    public async Task<IActionResult> GetAnnouncements(CancellationToken ct)
    {
        var result = await _service.GetAnnouncementsAsync(CurrentUserId, ct);
        return Ok(result);
    }

    // ── POST /api/classrep/announcements ─────────────────────────────────────

    [HttpPost("announcements")]
    public async Task<IActionResult> CreateAnnouncement(
        [FromBody] CreateClassAnnouncementDto dto, CancellationToken ct)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        try
        {
            var result = await _service.CreateAnnouncementAsync(CurrentUserId, dto, ct);
            return CreatedAtAction(nameof(GetAnnouncements), result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    // ── DELETE /api/classrep/announcements/{id} ──────────────────────────────

    [HttpDelete("announcements/{id:guid}")]
    public async Task<IActionResult> DeleteAnnouncement(Guid id, CancellationToken ct)
    {
        var deleted = await _service.DeleteAnnouncementAsync(CurrentUserId, id, ct);
        if (!deleted)
            return NotFound(new { error = "Announcement not found or access denied." });
        return NoContent();
    }

    // ── GET /api/classrep/analytics ──────────────────────────────────────────

    [HttpGet("analytics")]
    public async Task<IActionResult> GetAnalytics(CancellationToken ct)
    {
        var result = await _service.GetAnalyticsAsync(CurrentUserId, ct);
        return Ok(result);
    }
}
