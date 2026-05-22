using System;
using System.ComponentModel.DataAnnotations;

namespace Ssomero.Api.Entities;

public class Class
{
    public Guid Id { get; set; }

    [Required, MaxLength(300)]
    public string Name { get; set; } = string.Empty;

    /// <summary>Null for main class, set for subclasses (course unit classes).</summary>
    [MaxLength(50)]
    public string? CourseCode { get; set; }

    /// <summary>Self-referencing FK: null for main class, points to main class for subclasses.</summary>
    public Guid? ParentClassId { get; set; }
    public Class? ParentClass { get; set; }

    public Guid ProgramId { get; set; }
    public AcademicProgram Program { get; set; } = null!;

    public int YearOfStudy { get; set; }

    public Guid SemesterId { get; set; }
    public Semester Semester { get; set; } = null!;

    public Guid AcademicYearId { get; set; }
    public AcademicYear AcademicYear { get; set; } = null!;

    public Guid? CreatedBy { get; set; }

    // Navigation
    public ICollection<Class> SubClasses { get; set; } = [];
    public ICollection<StudentClass> StudentClasses { get; set; } = [];
    public ICollection<LecturerClass> LecturerClasses { get; set; } = [];
    public ICollection<ClassSession> Sessions { get; set; } = [];
}
