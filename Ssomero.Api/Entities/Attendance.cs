using System.ComponentModel.DataAnnotations;

namespace Ssomero.Api.Entities;

public class Attendance
{
    public Guid Id { get; set; }

    public Guid StudentId { get; set; }

    public Guid ClassId { get; set; }

    /// <summary>Links to the specific timetable slot this attendance is for.</summary>
    public Guid? SessionId { get; set; }

    public DateTime Date { get; set; }

    public bool IsPresent { get; set; }

    /// <summary>UTC timestamp when the student submitted the request.</summary>
    public DateTime? SubmittedAt { get; set; }

    /// <summary>GPS latitude captured on device at submission time.</summary>
    public double? Latitude { get; set; }

    /// <summary>GPS longitude captured on device at submission time.</summary>
    public double? Longitude { get; set; }

    [MaxLength(500)]
    public string? Notes { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public Student Student { get; set; } = null!;
    public Class Class { get; set; } = null!;
    public ClassSession? Session { get; set; }
}
