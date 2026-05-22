using System;
using System.ComponentModel.DataAnnotations;

namespace Ssomero.Api.Entities;

public class Curriculum
{
    public Guid Id { get; set; }

    public Guid ProgramId { get; set; }
    public AcademicProgram Program { get; set; } = null!;

    public int YearOfStudy { get; set; }

    public Guid SemesterId { get; set; }
    public Semester Semester { get; set; } = null!;

    [Required, MaxLength(50)]
    public string CourseCode { get; set; } = string.Empty;

    [Required, MaxLength(300)]
    public string CourseName { get; set; } = string.Empty;
}
