using System;
using System.ComponentModel.DataAnnotations;

namespace Ssomero.Api.Entities;

public class AcademicProfile
{
    public Guid Id { get; set; }

    public Guid StudentId { get; set; }
    public Student Student { get; set; } = null!;

    public Guid UniversityId { get; set; }
    public University University { get; set; } = null!;

    public Guid FacultyId { get; set; }
    public Faculty Faculty { get; set; } = null!;

    public Guid DepartmentId { get; set; }
    public Department Department { get; set; } = null!;

    public Guid ProgramId { get; set; }
    public AcademicProgram Program { get; set; } = null!;

    public Guid EntrySchemeId { get; set; }
    public EntryScheme EntryScheme { get; set; } = null!;

    public Guid IntakeId { get; set; }
    public Intake Intake { get; set; } = null!;

    public Guid StudyModeId { get; set; }
    public StudyMode StudyMode { get; set; } = null!;

    public Guid AcademicYearId { get; set; }
    public AcademicYear AcademicYear { get; set; } = null!;

    public int YearOfStudy { get; set; }

    public Guid SemesterId { get; set; }
    public Semester Semester { get; set; } = null!;

    // Optimistic-concurrency token.
    public byte[] RowVersion { get; set; } = [];
}
