using System;
using System.ComponentModel.DataAnnotations;

namespace Ssomero.Api.Entities;

public class EntryScheme
{
    public Guid Id { get; set; }

    [Required, MaxLength(200)]
    public string Name { get; set; } = string.Empty; // e.g. "Direct Entry", "Mature Entry"
}

public class Intake
{
    public Guid Id { get; set; }

    [Required, MaxLength(100)]
    public string Name { get; set; } = string.Empty; // e.g. "August 2024"
}

public class StudyMode
{
    public Guid Id { get; set; }

    [Required, MaxLength(100)]
    public string Name { get; set; } = string.Empty; // e.g. "Day", "Evening", "Weekend"
}

public class AcademicYear
{
    public Guid Id { get; set; }

    [Required, MaxLength(50)]
    public string Name { get; set; } = string.Empty; // e.g. "2024/2025"
}

public class Semester
{
    public Guid Id { get; set; }

    [Required, MaxLength(100)]
    public string Name { get; set; } = string.Empty; // e.g. "Semester 1"

    public int Number { get; set; } // 1, 2
}
