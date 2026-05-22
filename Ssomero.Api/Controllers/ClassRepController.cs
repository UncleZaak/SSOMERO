using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ssomero.Api.Dtos;
using Ssomero.Api.Services.Interfaces;

namespace Ssomero.Api.Controllers;

[ApiController]
[Route("api/classrep")]
[Authorize(Roles = "ClassRepresentative")]
public class ClassRepController : ControllerBase
{
    private readonly IClassRepService _service;
    private readonly ILogger<ClassRepController> _logger;

    public ClassRepController(IClassRepService service, ILogger<ClassRepController> logger)
    {
        _service = service;
        _logger  = logger;
    }

    private Guid CurrentUserId =>
        Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)
                   ?? User.FindFirstValue("sub")
                   ?? throw new InvalidOperationException("User ID claim missing."));

    // ── GET /api/classrep/my-class ───────────────────────────────────────────

    [HttpGet("my-class")]
    public async Task<IActionResult> GetMyClass(CancellationToken ct)
    {
        var result = await _service.GetMyClassAsync(CurrentUserId, ct);
        return result is null ? NotFound(new { error = "No managed class found." }) : Ok(result);
    }

    // ── GET /api/classrep/subclasses ─────────────────────────────────────────

    [HttpGet("subclasses")]
    public async Task<IActionResult> GetSubclasses(CancellationToken ct)
    {
        var result = await _service.GetSubclassesAsync(CurrentUserId, ct);
        return Ok(result);
    }

    // ── POST /api/classrep/subclasses ────────────────────────────────────────

    [HttpPost("subclasses")]
    public async Task<IActionResult> CreateSubclass([FromBody] CreateSubclassDto dto, CancellationToken ct)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        try
        {
            var result = await _service.CreateSubclassAsync(CurrentUserId, dto, ct);
            return CreatedAtAction(nameof(GetSubclasses), result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    // ── PUT /api/classrep/subclasses/{id} ────────────────────────────────────

    [HttpPut("subclasses/{id:guid}")]
    public async Task<IActionResult> RenameSubclass(Guid id, [FromBody] RenameSubclassDto dto, CancellationToken ct)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var result = await _service.RenameSubclassAsync(CurrentUserId, id, dto, ct);
        return result is null ? NotFound(new { error = "Subclass not found or access denied." }) : Ok(result);
    }

    // ── GET /api/classrep/classes/{id}/students ──────────────────────────────

    [HttpGet("classes/{id:guid}/students")]
    public async Task<IActionResult> GetStudents(Guid id, CancellationToken ct)
    {
        var result = await _service.GetStudentsAsync(CurrentUserId, id, ct);
        return Ok(result);
    }

    // ── DELETE /api/classrep/classes/{id}/students/{studentId} ───────────────

    [HttpDelete("classes/{id:guid}/students/{studentId:guid}")]
    public async Task<IActionResult> RemoveStudent(Guid id, Guid studentId, CancellationToken ct)
    {
        var removed = await _service.RemoveStudentAsync(CurrentUserId, id, studentId, ct);
        return removed ? NoContent() : NotFound(new { error = "Student not found in this class or access denied." });
    }

    // ── GET /api/classrep/lecturers ──────────────────────────────────────────

    [HttpGet("lecturers")]
    public async Task<IActionResult> GetLecturers(CancellationToken ct)
    {
        var result = await _service.GetApprovedLecturersAsync(CurrentUserId, ct);
        return Ok(result);
    }

    // ── POST /api/classrep/subclasses/{id}/assign-lecturer ───────────────────

    [HttpPost("subclasses/{id:guid}/assign-lecturer")]
    public async Task<IActionResult> AssignLecturer(Guid id, [FromBody] AssignLecturerDto dto, CancellationToken ct)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var success = await _service.AssignLecturerAsync(CurrentUserId, id, dto, ct);
        if (!success)
            return BadRequest(new { error = "Assignment failed. Check subclass ownership, lecturer status, or duplicate assignment." });
        return NoContent();
    }

    // ── GET /api/classrep/attendance/summary ─────────────────────────────────

    [HttpGet("attendance/summary")]
    public async Task<IActionResult> GetAttendanceSummary(CancellationToken ct)
    {
        var result = await _service.GetAttendanceSummaryAsync(CurrentUserId, ct);
        return Ok(result);
    }

    // ── GET /api/classrep/stats ──────────────────────────────────────────────

    [HttpGet("stats")]
    public async Task<IActionResult> GetStats(CancellationToken ct)
    {
        var result = await _service.GetStatsAsync(CurrentUserId, ct);
        return Ok(result);
    }
}
