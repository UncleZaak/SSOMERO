using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Ssomero.Api.Data;
using Ssomero.Api.Dtos;

namespace Ssomero.Api.Controllers;

[ApiController]
[Route("api/student")]
[Authorize(Roles = "Student,ClassRepresentative")]
public class StudentScheduleController : ControllerBase
{
    private readonly SsomeroDbContext _db;

    public StudentScheduleController(SsomeroDbContext db) => _db = db;

    private Guid GetStudentId() => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    /// <summary>
    /// GET /api/student/schedule?from=2026-01-01&amp;to=2026-01-07
    /// Returns all timetable sessions for the authenticated student's enrolled classes
    /// within the requested date range.
    /// </summary>
    [HttpGet("schedule")]
    public async Task<IActionResult> GetSchedule(
        [FromQuery] DateOnly? from,
        [FromQuery] DateOnly? to)
    {
        var studentId = GetStudentId();
        var start = from ?? DateOnly.FromDateTime(DateTime.UtcNow);
        var end   = to   ?? start.AddDays(6);

        if (end < start) return BadRequest("'to' must be >= 'from'.");
        if ((end.DayNumber - start.DayNumber) > 31) return BadRequest("Date range cannot exceed 31 days.");

        // Enrolled active class IDs
        var classIds = await _db.StudentClasses
            .Where(sc => sc.StudentId == studentId && sc.Status == "active")
            .Select(sc => sc.ClassId)
            .ToListAsync();

        if (classIds.Count == 0)
            return Ok(new StudentScheduleResponse([]));

        // Get lecturer names in a separate lookup to avoid multiple-collection warning
        var lecturerMap = await _db.LecturerClasses
            .Where(lc => classIds.Contains(lc.ClassId))
            .AsSplitQuery()
            .Select(lc => new { lc.ClassId, Name = lc.Lecturer.FirstName + " " + lc.Lecturer.LastName })
            .ToListAsync();

        var lecturerByClass = lecturerMap
            .GroupBy(x => x.ClassId)
            .ToDictionary(g => g.Key, g => g.First().Name);

        // All days of week in the requested range
        var daysInRange = Enumerable.Range(0, end.DayNumber - start.DayNumber + 1)
            .Select(i => start.AddDays(i).DayOfWeek)
            .Distinct()
            .ToHashSet();

        var sessions = await _db.ClassSessions
            .Where(cs => classIds.Contains(cs.ClassId)
                      && cs.IsActive
                      && daysInRange.Contains(cs.DayOfWeek))
            .Include(cs => cs.Class)
            .OrderBy(cs => cs.DayOfWeek)
            .ThenBy(cs => cs.StartTime)
            .ToListAsync();

        // Expand recurring sessions into concrete date/time instances within the range
        var result = new List<ClassSessionResponse>();
        for (var d = start; d <= end; d = d.AddDays(1))
        {
            foreach (var s in sessions.Where(s => s.DayOfWeek == d.DayOfWeek))
            {
                result.Add(new ClassSessionResponse(
                    s.Id,
                    s.ClassId,
                    s.Class.Name,
                    s.Class.CourseCode,
                    d.ToDateTime(s.StartTime, DateTimeKind.Local),
                    d.ToDateTime(s.EndTime, DateTimeKind.Local),
                    s.Location,
                    lecturerByClass.GetValueOrDefault(s.ClassId)
                ));
            }
        }

        return Ok(new StudentScheduleResponse(result));
    }
}
