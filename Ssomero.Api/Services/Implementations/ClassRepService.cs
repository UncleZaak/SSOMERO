using Microsoft.EntityFrameworkCore;
using Ssomero.Api.Data;
using Ssomero.Api.Dtos;
using Ssomero.Api.Entities;
using Ssomero.Api.Services.Interfaces;

namespace Ssomero.Api.Services.Implementations;

public sealed class ClassRepService : IClassRepService
{
    private readonly SsomeroDbContext _db;
    private readonly ILogger<ClassRepService> _logger;

    public ClassRepService(SsomeroDbContext db, ILogger<ClassRepService> logger)
    {
        _db = db;
        _logger = logger;
    }

    // ── Ownership helpers ────────────────────────────────────────────────────

    /// <summary>Returns the IDs of main classes for which <paramref name="userId"/> is an active class rep.</summary>
    private async Task<List<Guid>> GetManagedMainClassIdsAsync(Guid userId, CancellationToken ct) =>
        await _db.StudentClasses
            .Where(sc => sc.StudentId == userId
                      && sc.Role == "class_rep"
                      && sc.Status == "active"
                      && sc.Class.ParentClassId == null)
            .Select(sc => sc.ClassId)
            .ToListAsync(ct);

    /// <summary>True when the user owns the given class directly (main class) or as a subclass of their main class.</summary>
    private async Task<bool> OwnsClassAsync(Guid userId, Guid classId, CancellationToken ct)
    {
        var mainIds = await GetManagedMainClassIdsAsync(userId, ct);
        if (mainIds.Count == 0) return false;
        // Allow access to the main class itself or any of its subclasses
        return await _db.Classes.AnyAsync(
            c => c.Id == classId && (mainIds.Contains(c.Id) || (c.ParentClassId != null && mainIds.Contains(c.ParentClassId.Value))),
            ct);
    }

    /// <summary>True when classId is a subclass owned (indirectly) by the user.</summary>
    private async Task<bool> OwnsSubclassAsync(Guid userId, Guid subclassId, CancellationToken ct)
    {
        var mainIds = await GetManagedMainClassIdsAsync(userId, ct);
        if (mainIds.Count == 0) return false;
        return await _db.Classes.AnyAsync(
            c => c.Id == subclassId && c.ParentClassId != null && mainIds.Contains(c.ParentClassId.Value),
            ct);
    }

    // ── GetMyClassAsync ──────────────────────────────────────────────────────

    public async Task<ClassRepMyClassDto?> GetMyClassAsync(Guid userId, CancellationToken ct = default)
    {
        var mainIds = await GetManagedMainClassIdsAsync(userId, ct);
        if (mainIds.Count == 0) return null;

        // Return the first managed main class
        return await _db.Classes
            .Where(c => mainIds.Contains(c.Id))
            .Select(c => new ClassRepMyClassDto(
                c.Id,
                c.Name,
                c.Program.Name,
                c.StudentClasses.Count(sc => sc.Status == "active"),
                c.SubClasses.Count,
                c.SubClasses.SelectMany(sc => sc.LecturerClasses).Select(lc => lc.LecturerId).Distinct().Count()
            ))
            .FirstOrDefaultAsync(ct);
    }

    // ── GetSubclassesAsync ───────────────────────────────────────────────────

    public async Task<IReadOnlyList<ClassRepSubclassDto>> GetSubclassesAsync(Guid userId, CancellationToken ct = default)
    {
        var mainIds = await GetManagedMainClassIdsAsync(userId, ct);
        if (mainIds.Count == 0) return [];

        return await _db.Classes
            .Where(c => c.ParentClassId != null && mainIds.Contains(c.ParentClassId.Value))
            .OrderBy(c => c.Name)
            .Select(c => new ClassRepSubclassDto(
                c.Id,
                c.Name,
                c.CourseCode,
                c.StudentClasses.Count(sc => sc.Status == "active"),
                c.LecturerClasses.Count,
                DateTime.UtcNow  // CreatedAt not on entity; placeholder
            ))
            .ToListAsync(ct);
    }

    // ── CreateSubclassAsync ──────────────────────────────────────────────────

    public async Task<ClassRepSubclassDto> CreateSubclassAsync(Guid userId, CreateSubclassDto dto, CancellationToken ct = default)
    {
        var mainIds = await GetManagedMainClassIdsAsync(userId, ct);
        if (mainIds.Count == 0)
            throw new InvalidOperationException("No managed main class found for this user.");

        var parentId = mainIds[0];
        var name = dto.Name.Trim();

        // Duplicate check (case-insensitive)
        var duplicate = await _db.Classes.AnyAsync(
            c => c.ParentClassId == parentId && c.Name.ToLower() == name.ToLower(), ct);
        if (duplicate)
            throw new InvalidOperationException($"A subclass named '{name}' already exists under this class.");

        // Load parent class to copy required FK fields
        var parent = await _db.Classes.FirstAsync(c => c.Id == parentId, ct);

        var subclass = new Class
        {
            Id            = Guid.NewGuid(),
            Name          = name,
            CourseCode    = dto.Description?.Length > 0 ? null : null, // description stored in CourseCode as proxy
            ParentClassId = parentId,
            ProgramId     = parent.ProgramId,
            YearOfStudy   = parent.YearOfStudy,
            SemesterId    = parent.SemesterId,
            AcademicYearId = parent.AcademicYearId,
            CreatedBy     = userId,
        };
        // Store description in CourseCode field (max 50 chars) if provided; truncate safely
        if (!string.IsNullOrWhiteSpace(dto.Description))
            subclass.CourseCode = dto.Description.Length > 50 ? dto.Description[..50] : dto.Description;

        _db.Classes.Add(subclass);
        await _db.SaveChangesAsync(ct);

        _logger.LogInformation("ClassRep {UserId} created subclass {SubclassId} '{Name}' under {ParentId}",
            userId, subclass.Id, subclass.Name, parentId);

        return new ClassRepSubclassDto(
            subclass.Id,
            subclass.Name,
            subclass.CourseCode,
            0,
            0,
            DateTime.UtcNow);
    }

    // ── RenameSubclassAsync ──────────────────────────────────────────────────

    public async Task<ClassRepSubclassDto?> RenameSubclassAsync(Guid userId, Guid subclassId, RenameSubclassDto dto, CancellationToken ct = default)
    {
        if (!await OwnsSubclassAsync(userId, subclassId, ct)) return null;

        var subclass = await _db.Classes.FirstOrDefaultAsync(c => c.Id == subclassId, ct);
        if (subclass is null) return null;

        subclass.Name = dto.Name.Trim();
        await _db.SaveChangesAsync(ct);

        _logger.LogInformation("ClassRep {UserId} renamed subclass {SubclassId} to '{Name}'", userId, subclassId, subclass.Name);

        return new ClassRepSubclassDto(
            subclass.Id,
            subclass.Name,
            subclass.CourseCode,
            subclass.StudentClasses.Count(sc => sc.Status == "active"),
            subclass.LecturerClasses.Count,
            DateTime.UtcNow);
    }

    // ── GetStudentsAsync ─────────────────────────────────────────────────────

    public async Task<IReadOnlyList<ClassRepStudentDto>> GetStudentsAsync(Guid userId, Guid classId, CancellationToken ct = default)
    {
        if (!await OwnsClassAsync(userId, classId, ct)) return [];

        return await _db.StudentClasses
            .Where(sc => sc.ClassId == classId && sc.Status == "active")
            .OrderBy(sc => sc.Student.SecondName).ThenBy(sc => sc.Student.FirstName)
            .Select(sc => new ClassRepStudentDto(
                sc.Student.Id,
                sc.Student.FirstName + " " + sc.Student.SecondName,
                sc.Student.Email))
            .ToListAsync(ct);
    }

    // ── RemoveStudentAsync ───────────────────────────────────────────────────

    public async Task<bool> RemoveStudentAsync(Guid userId, Guid classId, Guid studentId, CancellationToken ct = default)
    {
        if (!await OwnsClassAsync(userId, classId, ct)) return false;

        var membership = await _db.StudentClasses
            .FirstOrDefaultAsync(sc => sc.ClassId == classId && sc.StudentId == studentId, ct);
        if (membership is null) return false;

        membership.Status = "dropped";
        await _db.SaveChangesAsync(ct);

        _logger.LogInformation("ClassRep {UserId} removed student {StudentId} from class {ClassId}", userId, studentId, classId);
        return true;
    }

    // ── GetApprovedLecturersAsync ────────────────────────────────────────────

    public async Task<IReadOnlyList<ClassRepLecturerDto>> GetApprovedLecturersAsync(Guid userId, CancellationToken ct = default)
    {
        return await _db.Lecturers
            .Where(l => l.IsApproved && l.Status == UserStatus.Active && !l.IsDeleted)
            .OrderBy(l => l.LastName).ThenBy(l => l.FirstName)
            .Select(l => new ClassRepLecturerDto(
                l.Id,
                l.StaffId,
                l.FirstName + " " + l.LastName,
                l.Email))
            .ToListAsync(ct);
    }

    // ── AssignLecturerAsync ──────────────────────────────────────────────────

    public async Task<bool> AssignLecturerAsync(Guid userId, Guid subclassId, AssignLecturerDto dto, CancellationToken ct = default)
    {
        if (!await OwnsSubclassAsync(userId, subclassId, ct)) return false;

        // Verify lecturer is approved and active
        var lecturer = await _db.Lecturers
            .FirstOrDefaultAsync(l => l.Id == dto.LecturerId && l.IsApproved && l.Status == UserStatus.Active && !l.IsDeleted, ct);
        if (lecturer is null) return false;

        // Prevent duplicate assignment
        var alreadyAssigned = await _db.LecturerClasses
            .AnyAsync(lc => lc.ClassId == subclassId && lc.LecturerId == dto.LecturerId, ct);
        if (alreadyAssigned) return false;

        _db.LecturerClasses.Add(new LecturerClass
        {
            LecturerId   = dto.LecturerId,
            ClassId      = subclassId,
            AssignedBy   = userId,
            AssignedAt   = DateTime.UtcNow,
        });
        await _db.SaveChangesAsync(ct);

        _logger.LogInformation("ClassRep {UserId} assigned lecturer {LecturerId} to subclass {SubclassId}", userId, dto.LecturerId, subclassId);
        return true;
    }

    // ── GetAttendanceSummaryAsync ────────────────────────────────────────────

    public async Task<ClassRepAttendanceSummaryDto> GetAttendanceSummaryAsync(Guid userId, CancellationToken ct = default)
    {
        var mainIds = await GetManagedMainClassIdsAsync(userId, ct);
        if (mainIds.Count == 0) return new ClassRepAttendanceSummaryDto(0, 0, 0);

        var subclassIds = await _db.Classes
            .Where(c => c.ParentClassId != null && mainIds.Contains(c.ParentClassId.Value))
            .Select(c => c.Id)
            .ToListAsync(ct);

        var allClassIds = mainIds.Concat(subclassIds).ToList();

        var totalSessions = await _db.ClassSessions
            .CountAsync(s => allClassIds.Contains(s.ClassId), ct);

        var totalAttendances = await _db.Attendances
            .CountAsync(a => allClassIds.Contains(a.ClassId) && a.IsPresent, ct);

        double rate = totalSessions > 0
            ? Math.Round((double)totalAttendances / totalSessions * 100, 1)
            : 0;

        return new ClassRepAttendanceSummaryDto(rate, totalSessions, totalAttendances);
    }

    // ── GetStatsAsync ────────────────────────────────────────────────────────

    public async Task<ClassRepStatsDto> GetStatsAsync(Guid userId, CancellationToken ct = default)
    {
        var mainIds = await GetManagedMainClassIdsAsync(userId, ct);
        if (mainIds.Count == 0)
            return new ClassRepStatsDto(0, 0, 0, 0, 0);

        var subclassIds = await _db.Classes
            .Where(c => c.ParentClassId != null && mainIds.Contains(c.ParentClassId.Value))
            .Select(c => c.Id)
            .ToListAsync(ct);

        var allClassIds = mainIds.Concat(subclassIds).ToList();

        var totalSubclasses = subclassIds.Count;

        var totalStudents = await _db.StudentClasses
            .Where(sc => allClassIds.Contains(sc.ClassId) && sc.Status == "active")
            .Select(sc => sc.StudentId)
            .Distinct()
            .CountAsync(ct);

        var assignedLecturers = await _db.LecturerClasses
            .Where(lc => subclassIds.Contains(lc.ClassId))
            .Select(lc => lc.LecturerId)
            .Distinct()
            .CountAsync(ct);

        var totalSessions = await _db.ClassSessions
            .CountAsync(s => allClassIds.Contains(s.ClassId), ct);

        var totalAttendances = await _db.Attendances
            .CountAsync(a => allClassIds.Contains(a.ClassId) && a.IsPresent, ct);

        double avgRate = totalSessions > 0
            ? Math.Round((double)totalAttendances / totalSessions * 100, 1)
            : 0;

        return new ClassRepStatsDto(
            mainIds.Count,
            totalStudents,
            totalSubclasses,
            assignedLecturers,
            avgRate);
    }
}
