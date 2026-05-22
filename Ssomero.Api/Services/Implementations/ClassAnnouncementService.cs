using Microsoft.EntityFrameworkCore;
using Ssomero.Api.Data;
using Ssomero.Api.Dtos;
using Ssomero.Api.Entities;
using Ssomero.Api.Services.Interfaces;

namespace Ssomero.Api.Services.Implementations;

public sealed class ClassAnnouncementService : IClassAnnouncementService
{
    private readonly SsomeroDbContext _db;
    private readonly ILogger<ClassAnnouncementService> _logger;

    public ClassAnnouncementService(SsomeroDbContext db, ILogger<ClassAnnouncementService> logger)
    {
        _db     = db;
        _logger = logger;
    }

    // ── Ownership helper ─────────────────────────────────────────────────────

    /// <summary>
    /// Returns all main class IDs (and their subclass IDs) that userId manages as class rep.
    /// </summary>
    private async Task<List<Guid>> GetManagedClassIdsAsync(Guid userId, CancellationToken ct)
    {
        var mainIds = await _db.StudentClasses
            .Where(sc => sc.StudentId == userId
                      && sc.Role == "class_rep"
                      && sc.Status == "active"
                      && sc.Class.ParentClassId == null)
            .Select(sc => sc.ClassId)
            .ToListAsync(ct);

        if (mainIds.Count == 0) return [];

        var subIds = await _db.Classes
            .Where(c => c.ParentClassId != null && mainIds.Contains(c.ParentClassId!.Value))
            .Select(c => c.Id)
            .ToListAsync(ct);

        return [.. mainIds, .. subIds];
    }

    // ── GetAnnouncementsAsync ────────────────────────────────────────────────

    public async Task<IReadOnlyList<ClassAnnouncementDto>> GetAnnouncementsAsync(
        Guid userId, CancellationToken ct = default)
    {
        var classIds = await GetManagedClassIdsAsync(userId, ct);
        if (classIds.Count == 0) return [];

        return await _db.ClassAnnouncements
            .Include(a => a.Class)
            .Where(a => classIds.Contains(a.ClassId))
            .OrderByDescending(a => a.CreatedAt)
            .Select(a => new ClassAnnouncementDto(
                a.Id, a.ClassId, a.Class.Name,
                a.CreatedBy, a.Title, a.Body, a.CreatedAt))
            .ToListAsync(ct);
    }

    // ── CreateAnnouncementAsync ──────────────────────────────────────────────

    public async Task<ClassAnnouncementDto> CreateAnnouncementAsync(
        Guid userId, CreateClassAnnouncementDto dto, CancellationToken ct = default)
    {
        var managedIds = await GetManagedClassIdsAsync(userId, ct);
        if (!managedIds.Contains(dto.ClassId))
            throw new InvalidOperationException("Access denied: class is not managed by you.");

        var className = await _db.Classes
            .Where(c => c.Id == dto.ClassId)
            .Select(c => c.Name)
            .FirstOrDefaultAsync(ct) ?? string.Empty;

        var entity = new ClassAnnouncement
        {
            Id        = Guid.NewGuid(),
            ClassId   = dto.ClassId,
            CreatedBy = userId,
            Title     = dto.Title.Trim(),
            Body      = dto.Message.Trim(),
            CreatedAt = DateTime.UtcNow,
        };

        _db.ClassAnnouncements.Add(entity);
        await _db.SaveChangesAsync(ct);

        _logger.LogInformation("ClassRep {UserId} created announcement {AnnouncementId} for class {ClassId}",
            userId, entity.Id, dto.ClassId);

        return new ClassAnnouncementDto(
            entity.Id, entity.ClassId, className,
            entity.CreatedBy, entity.Title, entity.Body, entity.CreatedAt);
    }

    // ── DeleteAnnouncementAsync ──────────────────────────────────────────────

    public async Task<bool> DeleteAnnouncementAsync(
        Guid userId, Guid announcementId, CancellationToken ct = default)
    {
        var entity = await _db.ClassAnnouncements
            .FirstOrDefaultAsync(a => a.Id == announcementId, ct);

        if (entity is null) return false;
        if (entity.CreatedBy != userId) return false;  // only creator may delete

        entity.IsDeleted  = true;
        entity.DeletedAt  = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);

        _logger.LogInformation("ClassRep {UserId} deleted announcement {AnnouncementId}", userId, announcementId);
        return true;
    }

    // ── GetAnalyticsAsync ────────────────────────────────────────────────────

    public async Task<ClassRepAnalyticsDto> GetAnalyticsAsync(
        Guid userId, CancellationToken ct = default)
    {
        var mainIds = await _db.StudentClasses
            .Where(sc => sc.StudentId == userId
                      && sc.Role == "class_rep"
                      && sc.Status == "active"
                      && sc.Class.ParentClassId == null)
            .Select(sc => sc.ClassId)
            .ToListAsync(ct);

        if (mainIds.Count == 0)
            return new ClassRepAnalyticsDto(0, 0, 0, 0.0, [], []);

        // Sub-class IDs
        var subIds = await _db.Classes
            .Where(c => c.ParentClassId != null && mainIds.Contains(c.ParentClassId!.Value))
            .Select(c => c.Id)
            .ToListAsync(ct);

        var allClassIds = new List<Guid>([.. mainIds, .. subIds]);

        // Total students (distinct, active) across main classes
        var totalStudents = await _db.StudentClasses
            .Where(sc => mainIds.Contains(sc.ClassId) && sc.Status == "active")
            .Select(sc => sc.StudentId)
            .Distinct()
            .CountAsync(ct);

        var totalSubclasses = subIds.Count;

        var assignedLecturers = await _db.LecturerClasses
            .Where(lc => allClassIds.Contains(lc.ClassId))
            .Select(lc => lc.LecturerId)
            .Distinct()
            .CountAsync(ct);

        // Average attendance across all subclasses
        double avgAttendance = 0;
        var attendanceData = await _db.Attendances
            .Where(a => allClassIds.Contains(a.ClassId))
            .GroupBy(a => a.ClassId)
            .Select(g => new { Total = g.Count(), Present = g.Count(a => a.IsPresent) })
            .ToListAsync(ct);

        if (attendanceData.Count > 0)
        {
            int totalPresent = attendanceData.Sum(g => g.Present);
            int totalRecs    = attendanceData.Sum(g => g.Total);
            avgAttendance = totalRecs > 0 ? (double)totalPresent / totalRecs * 100.0 : 0;
        }

        // Attendance trend: last 8 weeks
        var now        = DateTime.UtcNow;
        var trendStart = now.AddDays(-56); // 8 weeks

        var rawAttendance = await _db.Attendances
            .Where(a => allClassIds.Contains(a.ClassId) && a.Date >= trendStart)
            .Select(a => new { a.Date, a.IsPresent })
            .ToListAsync(ct);

        var attendanceTrend = Enumerable.Range(0, 8)
            .Select(w =>
            {
                var weekStart = trendStart.AddDays(w * 7);
                var weekEnd   = weekStart.AddDays(7);
                var week      = rawAttendance.Where(a => a.Date >= weekStart && a.Date < weekEnd).ToList();
                double rate   = week.Count > 0 ? (double)week.Count(a => a.IsPresent) / week.Count * 100.0 : 0;
                return new TrendPointDto($"W{w + 1}", Math.Round(rate, 1));
            })
            .ToList();

        // Student growth trend: enrolments in last 8 weeks (approximated via StudentClass, no CreatedAt on entity — use constant growth)
        // Since StudentClass has no timestamp, we generate a flat line from current total as a safe placeholder.
        // Real implementations should add a CreatedAt column to StudentClass for accurate trend data.
        var studentGrowthTrend = Enumerable.Range(0, 8)
            .Select(w => new TrendPointDto($"W{w + 1}", totalStudents))
            .ToList();

        return new ClassRepAnalyticsDto(
            totalStudents,
            totalSubclasses,
            assignedLecturers,
            Math.Round(avgAttendance, 1),
            attendanceTrend,
            studentGrowthTrend);
    }
}
