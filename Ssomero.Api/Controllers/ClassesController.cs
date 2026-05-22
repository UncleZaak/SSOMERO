using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Ssomero.Api.Data;
using Ssomero.Api.Dtos;

namespace Ssomero.Api.Controllers;

[ApiController]
[Route("api")]
[Authorize]
public class ClassesController : ControllerBase
{
    private readonly SsomeroDbContext _db;

    public ClassesController(SsomeroDbContext db)
    {
        _db = db;
    }

    private Guid GetUserId() => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    private string GetRole() => User.FindFirstValue(ClaimTypes.Role) ?? "Student";

    /// <summary>GET /api/my-classes — returns the authenticated user's classes.</summary>
    [HttpGet("my-classes")]
    public async Task<IActionResult> GetMyClasses()
    {
        var userId = GetUserId();
        var role = GetRole();

        if (role == "Student" || role == "ClassRepresentative")
        {
            var classes = await _db.StudentClasses
                .Where(sc => sc.StudentId == userId && sc.Status == "active")
                .AsSplitQuery()
                .Select(sc => new ClassDto(
                    sc.Class.Id,
                    sc.Class.Name,
                    sc.Class.CourseCode,
                    sc.Class.ParentClassId,
                    sc.Class.StudentClasses.Count(s => s.Status == "active"),
                    sc.Class.LecturerClasses.Select(lc => lc.Lecturer.FirstName + " " + lc.Lecturer.LastName).FirstOrDefault()
                ))
                .ToListAsync();
            return Ok(classes);
        }

        if (role == "Lecturer")
        {
            var classes = await _db.LecturerClasses
                .Where(lc => lc.LecturerId == userId)
                .Include(lc => lc.Class).ThenInclude(c => c.StudentClasses)
                .Select(lc => new ClassDto(
                    lc.Class.Id,
                    lc.Class.Name,
                    lc.Class.CourseCode,
                    lc.Class.ParentClassId,
                    lc.Class.StudentClasses.Count(s => s.Status == "active"),
                    null
                ))
                .ToListAsync();
            return Ok(classes);
        }

        return Forbid();
    }

    /// <summary>GET /api/courses — alias for my-classes, maps to frontend service expectations.</summary>
    [HttpGet("courses")]
    public Task<IActionResult> GetCourses() => GetMyClasses();

    /// <summary>GET /api/courses/{id}</summary>
    [HttpGet("courses/{id:guid}")]
    public async Task<IActionResult> GetCourse(Guid id)
    {
        var userId = GetUserId();
        var role = GetRole();

        if (role == "Lecturer")
        {
            var owns = await _db.LecturerClasses.AnyAsync(lc => lc.LecturerId == userId && lc.ClassId == id);
            if (!owns) return Forbid();
        }

        var cls = await _db.Classes
            .Include(c => c.StudentClasses)
            .Include(c => c.LecturerClasses).ThenInclude(lc => lc.Lecturer)
            .AsSplitQuery()
            .FirstOrDefaultAsync(c => c.Id == id);

        if (cls is null) return NotFound();

        var dto = new ClassDto(
            cls.Id,
            cls.Name,
            cls.CourseCode,
            cls.ParentClassId,
            cls.StudentClasses.Count(s => s.Status == "active"),
            cls.LecturerClasses.Select(lc => lc.Lecturer.FirstName + " " + lc.Lecturer.LastName).FirstOrDefault()
        );
        return Ok(dto);
    }
}
