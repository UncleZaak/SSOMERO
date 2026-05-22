using Microsoft.EntityFrameworkCore;
using Ssomero.Api.Data;
using Ssomero.Api.Entities;

namespace Ssomero.Api.Services;

/// <summary>
/// Handles automated class creation and student enrollment per the system rules:
/// - Main class: program + year + semester + academic_year
/// - Subclasses: generated from curriculum, linked via parent_class_id
/// - Auto-enroll students into main + all subclasses
/// </summary>
public class ClassService
{
    private readonly SsomeroDbContext _db;
    private readonly ILogger<ClassService> _logger;

    public ClassService(SsomeroDbContext db, ILogger<ClassService> logger)
    {
        _db = db;
        _logger = logger;
    }

    /// <summary>
    /// Finds or creates the main class for the given program/year/semester/academic year.
    /// Then ensures subclasses exist from curriculum and enrolls the student.
    /// </summary>
    public async Task EnrollStudentAsync(Guid studentId, Guid programId, int yearOfStudy, Guid semesterId, Guid academicYearId)
    {
        // 1. Find or create main class
        var mainClass = await _db.Classes
            .FirstOrDefaultAsync(c => c.ProgramId == programId
                                   && c.YearOfStudy == yearOfStudy
                                   && c.SemesterId == semesterId
                                   && c.AcademicYearId == academicYearId
                                   && c.ParentClassId == null);

        if (mainClass is null)
        {
            var program = await _db.Programs.FindAsync(programId);
            var semester = await _db.Semesters.FindAsync(semesterId);
            var ay = await _db.AcademicYears.FindAsync(academicYearId);

            mainClass = new Class
            {
                Id = Guid.NewGuid(),
                Name = $"{program?.Name} - Y{yearOfStudy} {semester?.Name} ({ay?.Name})",
                CourseCode = null,
                ParentClassId = null,
                ProgramId = programId,
                YearOfStudy = yearOfStudy,
                SemesterId = semesterId,
                AcademicYearId = academicYearId
            };
            _db.Classes.Add(mainClass);
            _logger.LogInformation("Created main class: {Name}", mainClass.Name);
        }

        // 2. Enroll in main class
        await EnsureEnrolledAsync(studentId, mainClass.Id);

        // 3. Find curriculum entries and create/find subclasses
        var curriculumEntries = await _db.Curricula
            .Where(c => c.ProgramId == programId
                     && c.YearOfStudy == yearOfStudy
                     && c.SemesterId == semesterId)
            .ToListAsync();

        foreach (var entry in curriculumEntries)
        {
            var subClass = await _db.Classes
                .FirstOrDefaultAsync(c => c.ParentClassId == mainClass.Id
                                       && c.CourseCode == entry.CourseCode);

            if (subClass is null)
            {
                subClass = new Class
                {
                    Id = Guid.NewGuid(),
                    Name = $"{entry.CourseCode} - {entry.CourseName}",
                    CourseCode = entry.CourseCode,
                    ParentClassId = mainClass.Id,
                    ProgramId = programId,
                    YearOfStudy = yearOfStudy,
                    SemesterId = semesterId,
                    AcademicYearId = academicYearId
                };
                _db.Classes.Add(subClass);
                _logger.LogInformation("Created subclass: {Name}", subClass.Name);
            }

            await EnsureEnrolledAsync(studentId, subClass.Id);
        }

        await _db.SaveChangesAsync();
    }

    private async Task EnsureEnrolledAsync(Guid studentId, Guid classId)
    {
        var exists = await _db.StudentClasses
            .AnyAsync(sc => sc.StudentId == studentId && sc.ClassId == classId);

        if (!exists)
        {
            _db.StudentClasses.Add(new StudentClass
            {
                StudentId = studentId,
                ClassId = classId,
                Role = "student",
                Status = "active"
            });
        }
    }
}
