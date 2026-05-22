using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ssomero.Api.Dtos;
using Ssomero.Api.Services.Interfaces;

namespace Ssomero.Api.Controllers;

/// <summary>
/// Endpoints for the Lecturer role: class management, attendance, and materials.
/// </summary>
[ApiController]
[Route("api/lecturer")]
[Authorize(Roles = "Lecturer")]
public class LecturerController : ControllerBase
{
    private readonly ILecturerService _lecturerService;
    private readonly ILogger<LecturerController> _logger;

    public LecturerController(ILecturerService lecturerService, ILogger<LecturerController> logger)
    {
        _lecturerService = lecturerService;
        _logger = logger;
    }

    private Guid GetLecturerId() => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    // ── GET /api/lecturer/classes ────────────────────────────────────────────
    /// <summary>Returns all subclasses assigned to the authenticated lecturer.</summary>
    [HttpGet("classes")]
    public async Task<IActionResult> GetClasses(CancellationToken ct)
    {
        var result = await _lecturerService.GetLecturerClassesAsync(GetLecturerId(), ct);
        return Ok(result);
    }

    // ── GET /api/lecturer/classes/{id} ───────────────────────────────────────
    /// <summary>Returns class details (info, student count, session summary) for an assigned class.</summary>
    [HttpGet("classes/{id:guid}")]
    public async Task<IActionResult> GetClassDetails(Guid id, CancellationToken ct)
    {
        var result = await _lecturerService.GetClassDetailsAsync(GetLecturerId(), id, ct);
        if (result is null) return Forbid();
        return Ok(result);
    }

    // ── GET /api/lecturer/classes/{id}/students ──────────────────────────────
    /// <summary>Returns the list of students enrolled in an assigned class.</summary>
    [HttpGet("classes/{id:guid}/students")]
    public async Task<IActionResult> GetClassStudents(Guid id, CancellationToken ct)
    {
        var result = await _lecturerService.GetClassStudentsAsync(GetLecturerId(), id, ct);
        if (result is null) return Forbid();
        return Ok(result);
    }

    // ── GET /api/lecturer/sessions/{id}/attendance ───────────────────────────
    /// <summary>Returns the attendance list for a session belonging to an assigned class.</summary>
    [HttpGet("sessions/{id:guid}/attendance")]
    public async Task<IActionResult> GetSessionAttendance(Guid id, CancellationToken ct)
    {
        var result = await _lecturerService.GetSessionAttendanceAsync(GetLecturerId(), id, ct);
        if (result is null) return Forbid();
        return Ok(result);
    }

    // ── POST /api/lecturer/sessions/{id}/attendance ──────────────────────────
    /// <summary>Allows a lecturer to manually mark or update a student's attendance for a session.</summary>
    [HttpPost("sessions/{id:guid}/attendance")]
    public async Task<IActionResult> MarkAttendance(Guid id, [FromBody] LecturerMarkAttendanceDto dto, CancellationToken ct)
    {
        if (!ModelState.IsValid) return ValidationProblem();

        // Route id must match body SessionId
        if (id != dto.SessionId)
            return BadRequest(new { error = "Route session id does not match request body." });

        var (success, error) = await _lecturerService.MarkAttendanceAsync(GetLecturerId(), dto, ct);
        if (!success)
        {
            if (error is "Not authorised for this class.") return Forbid();
            if (error is "Session not found." or "Student is not enrolled in this class.") return NotFound(new { error });
            return BadRequest(new { error });
        }

        return Ok(new { message = "Attendance recorded." });
    }

    // ── GET /api/lecturer/classes/{id}/materials ─────────────────────────────
    /// <summary>Returns materials uploaded for an assigned class.</summary>
    [HttpGet("classes/{id:guid}/materials")]
    public async Task<IActionResult> GetMaterials(Guid id, CancellationToken ct)
    {
        var result = await _lecturerService.GetMaterialsAsync(GetLecturerId(), id, ct);
        if (result is null) return Forbid();
        return Ok(result);
    }

    // ── POST /api/lecturer/classes/{id}/materials ────────────────────────────
    /// <summary>Uploads (registers) a material for an assigned class.</summary>
    [HttpPost("classes/{id:guid}/materials")]
    public async Task<IActionResult> UploadMaterial(Guid id, [FromBody] UploadMaterialDto dto, CancellationToken ct)
    {
        if (!ModelState.IsValid) return ValidationProblem();

        if (id != dto.ClassId)
            return BadRequest(new { error = "Route class id does not match request body." });

        var (success, error) = await _lecturerService.UploadMaterialAsync(GetLecturerId(), dto, ct);
        if (!success)
        {
            if (error is "Not authorised for this class.") return Forbid();
            return BadRequest(new { error });
        }

        return Ok(new { message = "Material uploaded." });
    }
}
