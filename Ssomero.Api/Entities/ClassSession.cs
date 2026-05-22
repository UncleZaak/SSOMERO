using System.ComponentModel.DataAnnotations;

namespace Ssomero.Api.Entities;

/// <summary>
/// A recurring timetable slot for a class (e.g., Monday 08:00–09:30 in Room 201).
/// </summary>
public class ClassSession
{
    public Guid Id { get; set; }

    public Guid ClassId { get; set; }
    public Class Class { get; set; } = null!;

    public DayOfWeek DayOfWeek { get; set; }

    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }

    [MaxLength(200)]
    public string? Location { get; set; }

    /// <summary>Optional GPS latitude of the classroom. When set, attendance GPS is validated.</summary>
    public double? Latitude { get; set; }

    /// <summary>Optional GPS longitude of the classroom. When set, attendance GPS is validated.</summary>
    public double? Longitude { get; set; }

    /// <summary>Allowed radius in metres. Defaults to 200 m when coordinates are present.</summary>
    public double AllowedRadiusMetres { get; set; } = 200;

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation back to attendance records for this session
    public ICollection<Attendance> Attendances { get; set; } = [];
}
