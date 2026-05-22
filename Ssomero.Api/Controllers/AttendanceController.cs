using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Ssomero.Api.Data;
using Ssomero.Api.Dtos;
using Ssomero.Api.Entities;

namespace Ssomero.Api.Controllers;

[ApiController]
[Route("api/attendance")]
[Authorize]
public class AttendanceController : ControllerBase
{
    private readonly SsomeroDbContext _db;
    private readonly ILogger<AttendanceController> _logger;

    /// <summary>Grace period before session start: student may enter up to 15 minutes early.</summary>
    private static readonly TimeSpan GracePeriodBefore = TimeSpan.FromMinutes(15);

    /// <summary>Grace period after session end: student may submit up to 10 minutes after class ends.</summary>
    private static readonly TimeSpan GracePeriodAfter = TimeSpan.FromMinutes(10);

    public AttendanceController(SsomeroDbContext db, ILogger<AttendanceController> logger)
    {
        _db = db;
        _logger = logger;
    }

    private Guid GetUserId() => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    // ──────────────────────────────────────────────────────────────────────────
    // POST /api/attendance/mark
    // ──────────────────────────────────────────────────────────────────────────
    [HttpPost("mark")]
    [Authorize(Roles = "Student,ClassRepresentative")]
    public async Task<IActionResult> Mark([FromBody] MarkAttendanceRequest req)
    {
        var studentId = GetUserId();

        // ── 1. Session must exist and be active ───────────────────────────────
        var session = await _db.ClassSessions
            .Include(cs => cs.Class)
            .FirstOrDefaultAsync(cs => cs.Id == req.SessionId && cs.IsActive);

        if (session is null)
            return NotFound(new { error = "Session not found." });

        // ── 2. Student must be enrolled in the class ──────────────────────────
        var enrolled = await _db.StudentClasses
            .AnyAsync(sc => sc.StudentId == studentId
                         && sc.ClassId == session.ClassId
                         && sc.Status == "active");

        if (!enrolled)
        {
            _logger.LogWarning("Student {StudentId} attempted to mark attendance for class {ClassId} — not enrolled.",
                studentId, session.ClassId);
            return Forbid();
        }

        // ── 3. Time-window validation ─────────────────────────────────────────
        var now          = DateTime.UtcNow;
        var today        = DateOnly.FromDateTime(now);
        var sessionStart = today.ToDateTime(session.StartTime, DateTimeKind.Utc);
        var sessionEnd   = today.ToDateTime(session.EndTime,   DateTimeKind.Utc);
        var windowOpen   = sessionStart - GracePeriodBefore;
        var windowClose  = sessionEnd   + GracePeriodAfter;

        if (now < windowOpen || now > windowClose)
        {
            return UnprocessableEntity(new
            {
                error = $"Attendance not within allowed time window. " +
                        $"Allowed: {windowOpen:HH:mm}–{windowClose:HH:mm} UTC. " +
                        $"Current time: {now:HH:mm} UTC."
            });
        }

        // ── 4. Duplicate check (only one submission per session per student) ──
        var alreadyMarked = await _db.Attendances
            .AnyAsync(a => a.StudentId == studentId
                        && a.SessionId == req.SessionId);

        if (alreadyMarked)
            return Conflict(new { error = "Attendance already marked for this session." });

        // ── 5. GPS validation ─────────────────────────────────────────────────
        if (session.Latitude.HasValue && session.Longitude.HasValue)
        {
            // Location is configured for this class — coordinates are mandatory
            if (!req.Latitude.HasValue || !req.Longitude.HasValue)
            {
                return UnprocessableEntity(new
                {
                    error = "Location is required to mark attendance for this class. Please enable GPS and try again."
                });
            }

            var distance = HaversineDistance(
                req.Latitude.Value, req.Longitude.Value,
                session.Latitude.Value, session.Longitude.Value);

            var allowed = session.AllowedRadiusMetres > 0
                ? session.AllowedRadiusMetres
                : 200;

            if (distance > allowed)
            {
                _logger.LogWarning(
                    "GPS check failed for Student={StudentId} Session={SessionId}: {Distance:F0}m > {Allowed:F0}m",
                    studentId, req.SessionId, distance, allowed);

                return UnprocessableEntity(new
                {
                    error = $"You are too far from the class location ({distance:F0} m away, limit is {allowed:F0} m). " +
                             "Please mark attendance from within the classroom."
                });
            }
        }

        // ── 6. Persist ────────────────────────────────────────────────────────
        var record = new Attendance
        {
            Id          = Guid.NewGuid(),
            StudentId   = studentId,
            ClassId     = session.ClassId,
            SessionId   = req.SessionId,
            Date        = DateTime.UtcNow.Date,
            SubmittedAt = req.Timestamp,
            IsPresent   = true,
            Latitude    = req.Latitude,
            Longitude   = req.Longitude,
            Notes       = req.SelfieBase64 is not null ? "selfie-submitted" : null
        };

        _db.Attendances.Add(record);
        await _db.SaveChangesAsync();

        _logger.LogInformation("Attendance marked: Student={StudentId} Session={SessionId}", studentId, req.SessionId);

        return Ok(new { message = "Attendance recorded.", attendanceId = record.Id });
    }

    // ──────────────────────────────────────────────────────────────────────────
    // GET /api/attendance/my-report
    // ──────────────────────────────────────────────────────────────────────────
    [HttpGet("my-report")]
    [Authorize(Roles = "Student,ClassRepresentative")]
    public async Task<IActionResult> MyReport()
    {
        var studentId = GetUserId();

        var classIds = await _db.StudentClasses
            .Where(sc => sc.StudentId == studentId && sc.Status == "active")
            .Select(sc => sc.ClassId)
            .ToListAsync();

        if (classIds.Count == 0)
            return Ok(new StudentAttendanceReport(0, []));

        // Total scheduled sessions per class (based on timetable entries)
        var sessionCounts = await _db.ClassSessions
            .Where(cs => classIds.Contains(cs.ClassId) && cs.IsActive)
            .GroupBy(cs => cs.ClassId)
            .Select(g => new { ClassId = g.Key, Total = g.Count() })
            .ToListAsync();

        // Attended sessions per class
        var attendedCounts = await _db.Attendances
            .Where(a => a.StudentId == studentId && classIds.Contains(a.ClassId) && a.IsPresent)
            .GroupBy(a => a.ClassId)
            .Select(g => new { ClassId = g.Key, Attended = g.Count() })
            .ToListAsync();

        // Class names
        var classNames = await _db.Classes
            .Where(c => classIds.Contains(c.Id))
            .Select(c => new { c.Id, c.Name })
            .ToListAsync();

        // Class-wide average: total attended / total enrolled students per class
        var classAvgData = await _db.Attendances
            .Where(a => classIds.Contains(a.ClassId) && a.IsPresent)
            .GroupBy(a => a.ClassId)
            .Select(g => new { ClassId = g.Key, TotalAttended = g.Count(), StudentCount = g.Select(a => a.StudentId).Distinct().Count() })
            .ToListAsync();

        var stats = classIds.Select(cid =>
        {
            var total    = sessionCounts.FirstOrDefault(x => x.ClassId == cid)?.Total ?? 0;
            var attended = attendedCounts.FirstOrDefault(x => x.ClassId == cid)?.Attended ?? 0;
            var name     = classNames.FirstOrDefault(x => x.Id == cid)?.Name ?? "Unknown";
            var pct      = total > 0 ? Math.Round((double)attended / total * 100, 1) : 0;

            // Class average: average attendance percentage across all enrolled students
            var avgData   = classAvgData.FirstOrDefault(x => x.ClassId == cid);
            double classAvg = 0;
            if (avgData is not null && avgData.StudentCount > 0 && total > 0)
                classAvg = Math.Round((double)avgData.TotalAttended / (avgData.StudentCount * total) * 100, 1);

            return new CourseAttendanceStat(cid, name, total, attended, pct, classAvg);
        }).ToList();

        var overall = stats.Count > 0
            ? Math.Round(stats.Average(s => s.Percent), 1)
            : 0;

        return Ok(new StudentAttendanceReport(overall, stats));
    }

    // ──────────────────────────────────────────────────────────────────────────
    // GET /api/attendance/my-history
    // ──────────────────────────────────────────────────────────────────────────
    [HttpGet("my-history")]
    [Authorize(Roles = "Student,ClassRepresentative")]
    public async Task<IActionResult> MyHistory()
    {
        var studentId = GetUserId();

        var records = await _db.Attendances
            .Where(a => a.StudentId == studentId)
            .Include(a => a.Class)
            .OrderByDescending(a => a.Date)
            .Take(100)
            .Select(a => new AttendanceRecordResponse(
                a.Id,
                a.ClassId,
                a.SessionId,
                a.Class.Name,
                a.Date,
                a.IsPresent,
                a.SubmittedAt
            ))
            .ToListAsync();

        return Ok(records);
    }

    // ──────────────────────────────────────────────────────────────────────────
    // Helper
    // ──────────────────────────────────────────────────────────────────────────
    private static double HaversineDistance(double lat1, double lon1, double lat2, double lon2)
    {
        const double R = 6371000; // metres
        var dLat = (lat2 - lat1) * Math.PI / 180;
        var dLon = (lon2 - lon1) * Math.PI / 180;
        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2)
              + Math.Cos(lat1 * Math.PI / 180) * Math.Cos(lat2 * Math.PI / 180)
              * Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
        return R * 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
    }
}
