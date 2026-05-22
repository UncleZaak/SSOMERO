using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ssomero.Api.Data;

namespace Ssomero.Api.Services.Implementations;

/// <summary>
/// One-time diagnostic utility that detects existing duplicate academic records
/// that would violate the unique indexes added in the AddAcademicHierarchyUniqueIndexes migration.
/// Call ReportDuplicatesAsync() before applying the migration to identify data that needs cleaning.
/// This service NEVER deletes data.
/// </summary>
public class AcademicDuplicateAuditService
{
    private readonly SsomeroDbContext _db;
    private readonly ILogger<AcademicDuplicateAuditService> _logger;

    public AcademicDuplicateAuditService(SsomeroDbContext db, ILogger<AcademicDuplicateAuditService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<AcademicDuplicateReport> ReportDuplicatesAsync(CancellationToken ct = default)
    {
        var report = new AcademicDuplicateReport();

        // Universities: duplicate by Name
        var dupUniversities = await _db.Universities
            .GroupBy(u => u.Name)
            .Where(g => g.Count() > 1)
            .Select(g => new DuplicateEntry
            {
                Key = g.Key,
                Count = g.Count(),
                Ids = g.Select(u => u.Id.ToString()).ToList()
            })
            .ToListAsync(ct);

        report.DuplicateUniversities = dupUniversities;

        // Faculties: duplicate by (Name, UniversityId)
        var dupFaculties = await _db.Faculties
            .Include(f => f.University)
            .GroupBy(f => new { f.Name, f.UniversityId })
            .Where(g => g.Count() > 1)
            .Select(g => new DuplicateEntry
            {
                Key = $"{g.Key.Name} (UniversityId={g.Key.UniversityId})",
                Count = g.Count(),
                Ids = g.Select(f => f.Id.ToString()).ToList()
            })
            .ToListAsync(ct);

        report.DuplicateFaculties = dupFaculties;

        // Departments: duplicate by (Name, FacultyId)
        var dupDepartments = await _db.Departments
            .GroupBy(d => new { d.Name, d.FacultyId })
            .Where(g => g.Count() > 1)
            .Select(g => new DuplicateEntry
            {
                Key = $"{g.Key.Name} (FacultyId={g.Key.FacultyId})",
                Count = g.Count(),
                Ids = g.Select(d => d.Id.ToString()).ToList()
            })
            .ToListAsync(ct);

        report.DuplicateDepartments = dupDepartments;

        // Programs: duplicate by (Name, DepartmentId)
        var dupPrograms = await _db.Programs
            .GroupBy(p => new { p.Name, p.DepartmentId })
            .Where(g => g.Count() > 1)
            .Select(g => new DuplicateEntry
            {
                Key = $"{g.Key.Name} (DepartmentId={g.Key.DepartmentId})",
                Count = g.Count(),
                Ids = g.Select(p => p.Id.ToString()).ToList()
            })
            .ToListAsync(ct);

        report.DuplicatePrograms = dupPrograms;

        // Curriculum: duplicate by (CourseCode, ProgramId)
        var dupCurricula = await _db.Curricula
            .GroupBy(c => new { c.CourseCode, c.ProgramId })
            .Where(g => g.Count() > 1)
            .Select(g => new DuplicateEntry
            {
                Key = $"{g.Key.CourseCode} (ProgramId={g.Key.ProgramId})",
                Count = g.Count(),
                Ids = g.Select(c => c.Id.ToString()).ToList()
            })
            .ToListAsync(ct);

        report.DuplicateCurricula = dupCurricula;

        LogReport(report);
        return report;
    }

    private void LogReport(AcademicDuplicateReport report)
    {
        int total = report.DuplicateUniversities.Count + report.DuplicateFaculties.Count
            + report.DuplicateDepartments.Count + report.DuplicatePrograms.Count + report.DuplicateCurricula.Count;

        if (total == 0)
        {
            _logger.LogInformation("[AcademicDuplicateAudit] No duplicate academic records found. Safe to apply unique-index migration.");
            return;
        }

        _logger.LogWarning("[AcademicDuplicateAudit] Found {Total} duplicate group(s) that would violate new unique indexes.", total);

        foreach (var d in report.DuplicateUniversities)
            _logger.LogWarning("[AcademicDuplicateAudit] Duplicate University: '{Key}' — {Count} records [{Ids}]", d.Key, d.Count, string.Join(", ", d.Ids));

        foreach (var d in report.DuplicateFaculties)
            _logger.LogWarning("[AcademicDuplicateAudit] Duplicate Faculty: '{Key}' — {Count} records [{Ids}]", d.Key, d.Count, string.Join(", ", d.Ids));

        foreach (var d in report.DuplicateDepartments)
            _logger.LogWarning("[AcademicDuplicateAudit] Duplicate Department: '{Key}' — {Count} records [{Ids}]", d.Key, d.Count, string.Join(", ", d.Ids));

        foreach (var d in report.DuplicatePrograms)
            _logger.LogWarning("[AcademicDuplicateAudit] Duplicate Program: '{Key}' — {Count} records [{Ids}]", d.Key, d.Count, string.Join(", ", d.Ids));

        foreach (var d in report.DuplicateCurricula)
            _logger.LogWarning("[AcademicDuplicateAudit] Duplicate Curriculum: '{Key}' — {Count} records [{Ids}]", d.Key, d.Count, string.Join(", ", d.Ids));
    }
}

public class AcademicDuplicateReport
{
    public List<DuplicateEntry> DuplicateUniversities { get; set; } = [];
    public List<DuplicateEntry> DuplicateFaculties { get; set; } = [];
    public List<DuplicateEntry> DuplicateDepartments { get; set; } = [];
    public List<DuplicateEntry> DuplicatePrograms { get; set; } = [];
    public List<DuplicateEntry> DuplicateCurricula { get; set; } = [];

    public bool HasDuplicates =>
        DuplicateUniversities.Count > 0 || DuplicateFaculties.Count > 0 ||
        DuplicateDepartments.Count > 0 || DuplicatePrograms.Count > 0 || DuplicateCurricula.Count > 0;
}

public class DuplicateEntry
{
    public string Key { get; set; } = string.Empty;
    public int Count { get; set; }
    public List<string> Ids { get; set; } = [];
}
