using System;
using System.ComponentModel.DataAnnotations;

namespace Ssomero.Api.Entities;

public class StudentClass
{
    public Guid StudentId { get; set; }
    public Student Student { get; set; } = null!;

    public Guid ClassId { get; set; }
    public Class Class { get; set; } = null!;

    [Required, MaxLength(30)]
    public string Role { get; set; } = "student"; // "student" | "class_rep"

    [Required, MaxLength(20)]
    public string Status { get; set; } = "active"; // "active" | "dropped"
}

public class LecturerClass
{
    public Guid LecturerId { get; set; }
    public Lecturer Lecturer { get; set; } = null!;

    public Guid ClassId { get; set; }
    public Class Class { get; set; } = null!;

    public Guid? AssignedBy { get; set; }
    public DateTime AssignedAt { get; set; } = DateTime.UtcNow;
}
