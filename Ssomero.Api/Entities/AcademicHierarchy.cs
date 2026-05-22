using System;
using System.ComponentModel.DataAnnotations;

namespace Ssomero.Api.Entities;

public class University
{
    public Guid Id { get; set; }

    [Required, MaxLength(300)]
    public string Name { get; set; } = string.Empty;

    public ICollection<Faculty> Faculties { get; set; } = [];
}

public class Faculty
{
    public Guid Id { get; set; }

    [Required, MaxLength(300)]
    public string Name { get; set; } = string.Empty;

    public Guid UniversityId { get; set; }
    public University University { get; set; } = null!;

    public ICollection<Department> Departments { get; set; } = [];
}

public class Department
{
    public Guid Id { get; set; }

    [Required, MaxLength(300)]
    public string Name { get; set; } = string.Empty;

    public Guid FacultyId { get; set; }
    public Faculty Faculty { get; set; } = null!;

    public ICollection<AcademicProgram> Programs { get; set; } = [];
}

public class AcademicProgram
{
    public Guid Id { get; set; }

    [Required, MaxLength(300)]
    public string Name { get; set; } = string.Empty;

    /// <summary>Duration in semesters (e.g. 8 for a 4-year programme).</summary>
    public int DurationSemesters { get; set; }

    public Guid DepartmentId { get; set; }
    public Department Department { get; set; } = null!;

    public ICollection<Curriculum> CurriculumEntries { get; set; } = [];
    public ICollection<Class> Classes { get; set; } = [];
}
