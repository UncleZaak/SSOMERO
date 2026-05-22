using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Ssomero.Api.Data;
using Ssomero.Api.DTOs.Common;
using Ssomero.Api.Entities;
using Ssomero.Api.Services.Interfaces;

namespace Ssomero.Api.Controllers.v1.Admin;

[ApiController]
[Route("api/v1/admin")]
[Authorize(Roles = "Admin")]
public class UserManagementController : ControllerBase
{
    private readonly SsomeroDbContext _db;
    private readonly IAuditLogService _audit;
    private readonly ILogger<UserManagementController> _logger;

    public UserManagementController(
        SsomeroDbContext db,
        IAuditLogService audit,
        ILogger<UserManagementController> logger)
    {
        _db = db;
        _audit = audit;
        _logger = logger;
    }

    // ---- Lecturers ----

    [HttpGet("lecturers")]
    public async Task<IActionResult> GetLecturers(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? search = null)
    {
        var query = _db.Lecturers.IgnoreQueryFilters().Where(l => !l.IsDeleted);

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(l => l.Email.Contains(search) || l.FirstName.Contains(search) || l.LastName.Contains(search));

        var total = await query.CountAsync();
        var items = await query
            .OrderByDescending(l => l.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(l => new { l.Id, Name = l.FirstName + " " + l.LastName, l.Email, l.StaffId, l.IsApproved, Status = l.Status.ToString(), l.CreatedAt })
            .ToListAsync();

        return Ok(new PaginatedResponse<object> { Data = items, TotalCount = total, Page = page, PageSize = pageSize });
    }

    [HttpGet("lecturers/pending")]
    public async Task<IActionResult> GetPendingLecturers()
    {
        var list = await _db.Lecturers
            .Where(l => !l.IsApproved && l.IsVerified)
            .Select(l => new { l.Id, l.FirstName, l.LastName, l.Email, l.StaffId, l.CreatedAt })
            .ToListAsync();
        return Ok(list);
    }

    [HttpPost("lecturers/{id:guid}/approve")]
    public async Task<IActionResult> ApproveLecturer(Guid id)
    {
        var lecturer = await _db.Lecturers.FindAsync(id);
        if (lecturer is null) return NotFound();

        lecturer.IsApproved = true;
        await _db.SaveChangesAsync();
        await _audit.LogAsync("Approve", nameof(Lecturer), id.ToString());
        _logger.LogInformation("Lecturer approved: {Id}", id);
        return Ok(new { message = "Lecturer approved" });
    }

    [HttpPost("lecturers/{id:guid}/suspend")]
    public async Task<IActionResult> SuspendLecturer(Guid id)
    {
        var lecturer = await _db.Lecturers.FindAsync(id);
        if (lecturer is null) return NotFound();

        lecturer.Status = UserStatus.Suspended;
        await _db.SaveChangesAsync();
        await _audit.LogAsync("Suspend", nameof(Lecturer), id.ToString());
        return Ok(new { message = "Lecturer suspended" });
    }

    [HttpPost("lecturers/{id:guid}/activate")]
    public async Task<IActionResult> ActivateLecturer(Guid id)
    {
        var lecturer = await _db.Lecturers.FindAsync(id);
        if (lecturer is null) return NotFound();

        lecturer.Status = UserStatus.Active;
        await _db.SaveChangesAsync();
        await _audit.LogAsync("Activate", nameof(Lecturer), id.ToString());
        return Ok(new { message = "Lecturer activated" });
    }

    [HttpDelete("lecturers/{id:guid}")]
    public async Task<IActionResult> DeleteLecturer(Guid id)
    {
        var lecturer = await _db.Lecturers.FindAsync(id);
        if (lecturer is null) return NotFound();

        var mangled = $"deleted_{id}_{lecturer.Email}";
        lecturer.Email = mangled[..Math.Min(mangled.Length, 200)];
        lecturer.IsDeleted = true;
        lecturer.DeletedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        await _audit.LogAsync("Delete", nameof(Lecturer), id.ToString());
        return Ok(new { message = "Lecturer deleted" });
    }

    // ---- Students ----

    [HttpGet("students")]
    public async Task<IActionResult> GetStudents(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? search = null)
    {
        var query = _db.Students.IgnoreQueryFilters().Where(s => !s.IsDeleted);

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(s => s.Email.Contains(search) || s.FirstName.Contains(search) || s.SecondName.Contains(search));

        var total = await query.CountAsync();
        var items = await query
            .OrderByDescending(s => s.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(s => new { s.Id, Name = s.FirstName + " " + s.SecondName, s.Email, Status = s.Status.ToString(), s.CreatedAt })
            .ToListAsync();

        return Ok(new PaginatedResponse<object> { Data = items, TotalCount = total, Page = page, PageSize = pageSize });
    }

    [HttpPost("students/{id:guid}/suspend")]
    public async Task<IActionResult> SuspendStudent(Guid id)
    {
        var student = await _db.Students.FindAsync(id);
        if (student is null) return NotFound();

        student.Status = UserStatus.Suspended;
        await _db.SaveChangesAsync();
        await _audit.LogAsync("Suspend", nameof(Student), id.ToString());
        return Ok(new { message = "Student suspended" });
    }

    [HttpPost("students/{id:guid}/activate")]
    public async Task<IActionResult> ActivateStudent(Guid id)
    {
        var student = await _db.Students.FindAsync(id);
        if (student is null) return NotFound();

        student.Status = UserStatus.Active;
        await _db.SaveChangesAsync();
        await _audit.LogAsync("Activate", nameof(Student), id.ToString());
        return Ok(new { message = "Student activated" });
    }

    [HttpDelete("students/{id:guid}")]
    public async Task<IActionResult> DeleteStudent(Guid id)
    {
        var student = await _db.Students.FindAsync(id);
        if (student is null) return NotFound();

        student.IsDeleted = true;
        student.DeletedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        await _audit.LogAsync("Delete", nameof(Student), id.ToString());
        return Ok(new { message = "Student deleted" });
    }

    // ---- Audit Logs ----

    [HttpGet("audit-logs")]
    public async Task<IActionResult> GetAuditLogs(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        [FromQuery] string? entityName = null,
        [FromQuery] string? action = null)
    {
        var query = _db.AuditLogs.AsQueryable();

        if (!string.IsNullOrWhiteSpace(entityName))
            query = query.Where(a => a.EntityName == entityName);
        if (!string.IsNullOrWhiteSpace(action))
            query = query.Where(a => a.Action == action);

        var total = await query.CountAsync();
        var items = await query
            .OrderByDescending(a => a.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return Ok(new PaginatedResponse<object> { Data = items, TotalCount = total, Page = page, PageSize = pageSize });
    }
}
