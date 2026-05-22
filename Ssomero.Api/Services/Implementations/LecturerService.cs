using Microsoft.EntityFrameworkCore;
using Ssomero.Api.Data;
using Ssomero.Api.Dtos;
using Ssomero.Api.Entities;
using Ssomero.Api.Services.Interfaces;

namespace Ssomero.Api.Services.Implementations;

public sealed class LecturerService : ILecturerService
{
    private readonly SsomeroDbContext _db;
    private readonly ILogger<LecturerService> _logger;

    public LecturerService(SsomeroDbContext db, ILogger<LecturerService> logger)
    {
        _db = db;
        _logger = logger;
    }

    // ── Ownership helper ─────────────────────────────────────────────────────

    private Task<bool> OwnsClassAsync(Guid lecturerId, Guid classId, CancellationToken ct) =>
        _db.LecturerClasses.AnyAsync(lc => lc.LecturerId == lecturerId && lc.ClassId == classId, ct);

    // ── 1. GetLecturerClassesAsync ───────────────────────────────────────────

    public async Task<IEnumerable<LecturerClassDto>> GetLecturerClassesAsync(
        Guid lecturerId, CancellationToken ct = default)
    {
        return await _db.LecturerClasses
            .Where(lc => lc.LecturerId == lecturerId)
            .Select(lc => new LecturerClassDto(
                lc.Class.Id,
                lc.Class.Name,
                lc.Class.CourseCode,
                lc.Class.StudentClasses.Count(sc => sc.Status == "active"),
                lc.Class.Sessions.Count(s => s.IsActive)
            ))
            .ToListAsync(ct);
    }

    // ── 2. GetClassDetailsAsync ──────────────────────────────────────────────

    public async Task<LecturerClassDetailDto?> GetClassDetailsAsync(
        Guid lecturerId, Guid classId, CancellationToken ct = default)
    {
        if (!await OwnsClassAsync(lecturerId, classId, ct)) return null;

        return await _db.Classes
            .Where(c => c.Id == classId)
            .Select(c => new LecturerClassDetailDto(
                c.Id,
                c.Name,
                c.CourseCode,
                c.StudentClasses.Count(sc => sc.Status == "active"),
                c.Sessions.Select(s => new SessionSummaryDto(
                    s.Id,
                    s.DayOfWeek.ToString(),
                    s.StartTime.ToString("HH:mm"),
                    s.EndTime.ToString("HH:mm"),
                    s.Location,
                    s.IsActive
                ))
            ))
            .FirstOrDefaultAsync(ct);
    }

    // ── 3. GetClassStudentsAsync ─────────────────────────────────────────────

    public async Task<IEnumerable<LecturerStudentDto>?> GetClassStudentsAsync(
        Guid lecturerId, Guid classId, CancellationToken ct = default)
    {
        if (!await OwnsClassAsync(lecturerId, classId, ct)) return null;

        return await _db.StudentClasses
            .Where(sc => sc.ClassId == classId && sc.Status == "active")
            .Select(sc => new LecturerStudentDto(
                sc.Student.Id,
                sc.Student.FirstName + " " + sc.Student.SecondName,
                sc.Student.Email,
                sc.Student.Status.ToString()
            ))
            .ToListAsync(ct);
    }

    // ── 4. GetSessionAttendanceAsync ─────────────────────────────────────────

    public async Task<IEnumerable<SessionAttendanceDto>?> GetSessionAttendanceAsync(
        Guid lecturerId, Guid sessionId, CancellationToken ct = default)
    {
        var session = await _db.ClassSessions
            .Where(s => s.Id == sessionId)
            .Select(s => new { s.ClassId })
            .FirstOrDefaultAsync(ct);

        if (session is null) return null;

        if (!await OwnsClassAsync(lecturerId, session.ClassId, ct)) return null;

        return await _db.Attendances
            .Where(a => a.SessionId == sessionId)
            .Select(a => new SessionAttendanceDto(
                a.Id,
                a.StudentId,
                a.Student.FirstName + " " + a.Student.SecondName,
                a.IsPresent,
                a.SubmittedAt
            ))
            .ToListAsync(ct);
    }

    // ── 5. MarkAttendanceAsync ───────────────────────────────────────────────

    public async Task<(bool Success, string? Error)> MarkAttendanceAsync(
        Guid lecturerId, LecturerMarkAttendanceDto dto, CancellationToken ct = default)
    {
        // Validate session exists
        var session = await _db.ClassSessions
            .Where(s => s.Id == dto.SessionId)
            .Select(s => new { s.ClassId, s.IsActive })
            .FirstOrDefaultAsync(ct);

        if (session is null)
            return (false, "Session not found.");

        if (!session.IsActive)
            return (false, "Session is not active.");

        // Ownership check
        if (!await OwnsClassAsync(lecturerId, session.ClassId, ct))
            return (false, "Not authorised for this class.");

        // Student must be enrolled
        var enrolled = await _db.StudentClasses
            .AnyAsync(sc => sc.StudentId == dto.StudentId
                         && sc.ClassId == session.ClassId
                         && sc.Status == "active", ct);

        if (!enrolled)
            return (false, "Student is not enrolled in this class.");

        // Upsert: update existing record or create a new one
        var existing = await _db.Attendances
            .FirstOrDefaultAsync(a => a.StudentId == dto.StudentId && a.SessionId == dto.SessionId, ct);

        if (existing is not null)
        {
            existing.IsPresent = dto.IsPresent;
            existing.Notes = dto.Notes;
        }
        else
        {
            _db.Attendances.Add(new Attendance
            {
                Id        = Guid.NewGuid(),
                StudentId = dto.StudentId,
                ClassId   = session.ClassId,
                SessionId = dto.SessionId,
                Date      = DateTime.UtcNow.Date,
                IsPresent = dto.IsPresent,
                Notes     = dto.Notes
            });
        }

        await _db.SaveChangesAsync(ct);
        _logger.LogInformation(
            "Lecturer {LecturerId} marked attendance for Student {StudentId} on Session {SessionId}: Present={IsPresent}",
            lecturerId, dto.StudentId, dto.SessionId, dto.IsPresent);

        return (true, null);
    }

    // ── 6. UploadMaterialAsync ───────────────────────────────────────────────

    public async Task<(bool Success, string? Error)> UploadMaterialAsync(
        Guid lecturerId, UploadMaterialDto dto, CancellationToken ct = default)
    {
        if (!await OwnsClassAsync(lecturerId, dto.ClassId, ct))
            return (false, "Not authorised for this class.");

        _db.ClassMaterials.Add(new ClassMaterial
        {
            Id         = Guid.NewGuid(),
            ClassId    = dto.ClassId,
            Title      = dto.Title,
            FileUrl    = dto.FileUrl,
            UploadedBy = lecturerId,
            CreatedAt  = DateTime.UtcNow
        });

        await _db.SaveChangesAsync(ct);
        _logger.LogInformation("Lecturer {LecturerId} uploaded material '{Title}' for class {ClassId}",
            lecturerId, dto.Title, dto.ClassId);

        return (true, null);
    }

    // ── 7. GetMaterialsAsync ─────────────────────────────────────────────────

    public async Task<IEnumerable<ClassMaterialDto>?> GetMaterialsAsync(
        Guid lecturerId, Guid classId, CancellationToken ct = default)
    {
        if (!await OwnsClassAsync(lecturerId, classId, ct)) return null;

        return await _db.ClassMaterials
            .Where(m => m.ClassId == classId)
            .OrderByDescending(m => m.CreatedAt)
            .Select(m => new ClassMaterialDto(m.Id, m.Title, m.FileUrl, m.CreatedAt))
            .ToListAsync(ct);
    }
}
