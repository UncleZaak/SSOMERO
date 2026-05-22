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
public class DashboardController : ControllerBase
{
    private readonly SsomeroDbContext _db;

    public DashboardController(SsomeroDbContext db)
    {
        _db = db;
    }

    /// <summary>GET /api/dashboard — returns summary stats for the authenticated user.</summary>
    [HttpGet("dashboard")]
    public async Task<IActionResult> GetDashboard()
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var role = (User.FindFirstValue(ClaimTypes.Role) ?? "Student").ToLowerInvariant();

        // Common values
        var upcomingAssignments = 0;
        var attendancePercent = 0.0;
        var recentAnnouncements = Array.Empty<AnnouncementResponse>();

        // Student-specific
        IEnumerable<ClassDto>? myClasses = null;

        // Lecturer-specific
        IEnumerable<ClassDto>? teachingClasses = null;

        // ClassRep-specific
        IEnumerable<ClassDto>? managedClasses = null;

        // Admin-specific
        int? totalStudents = null;
        int? totalLecturers = null;
        int? totalPrograms = null;

        if (role == "student")
        {
            var activeCourses = await _db.StudentClasses
                .CountAsync(sc => sc.StudentId == userId && sc.Status == "active");

            myClasses = await _db.StudentClasses
                .Where(sc => sc.StudentId == userId && sc.Status == "active")
                .Select(sc => new ClassDto(sc.Class.Id, sc.Class.Name, sc.Class.CourseCode, sc.Class.ParentClassId, sc.Class.StudentClasses.Count, null))
                .ToListAsync();

            var dto = new DashboardResponse(activeCourses, upcomingAssignments, attendancePercent, recentAnnouncements, MyClasses: myClasses);
            return Ok(dto);
        }

        if (role == "lecturer")
        {
            var activeCourses = await _db.LecturerClasses.CountAsync(lc => lc.LecturerId == userId);

            teachingClasses = await _db.LecturerClasses
                .Where(lc => lc.LecturerId == userId)
                .Select(lc => new ClassDto(lc.Class.Id, lc.Class.Name, lc.Class.CourseCode, lc.Class.ParentClassId, lc.Class.StudentClasses.Count, null))
                .ToListAsync();

            var dto = new DashboardResponse(activeCourses, upcomingAssignments, attendancePercent, recentAnnouncements, TeachingClasses: teachingClasses);
            return Ok(dto);
        }

        if (role == "classrep" || role == "classrepresentative")
        {
            var activeCourses = await _db.StudentClasses
                .CountAsync(sc => sc.StudentId == userId && sc.Role == "class_rep" && sc.Status == "active");

            managedClasses = await _db.StudentClasses
                .Where(sc => sc.StudentId == userId && sc.Role == "class_rep")
                .Select(sc => new ClassDto(sc.Class.Id, sc.Class.Name, sc.Class.CourseCode, sc.Class.ParentClassId, sc.Class.StudentClasses.Count, null))
                .ToListAsync();

            var dto = new DashboardResponse(activeCourses, upcomingAssignments, attendancePercent, recentAnnouncements, ManagedClasses: managedClasses);
            return Ok(dto);
        }

        if (role == "admin")
        {
            // lightweight aggregate counts
            totalStudents = await _db.Students.CountAsync();
            totalLecturers = await _db.Lecturers.CountAsync();
            totalPrograms = await _db.Programs.CountAsync();

            var dto = new DashboardResponse(0, upcomingAssignments, attendancePercent, recentAnnouncements, TotalStudents: totalStudents, TotalLecturers: totalLecturers, TotalPrograms: totalPrograms);
            return Ok(dto);
        }

        // Fallback — treat as student
        var fallbackActive = await _db.StudentClasses.CountAsync(sc => sc.StudentId == userId && sc.Status == "active");
        var fallbackDto = new DashboardResponse(fallbackActive, upcomingAssignments, attendancePercent, recentAnnouncements, MyClasses: myClasses);
        return Ok(fallbackDto);
    }

    /// <summary>GET /api/announcements — placeholder for announcements.</summary>
    [HttpGet("announcements")]
    public IActionResult GetAnnouncements()
    {
        return Ok(Array.Empty<AnnouncementResponse>());
    }

    /// <summary>GET /api/schedules — placeholder for schedules.</summary>
    [HttpGet("schedules")]
    public IActionResult GetSchedules()
    {
        return Ok(Array.Empty<object>());
    }
}
