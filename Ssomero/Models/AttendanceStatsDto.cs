namespace Ssomero.Models;

public class AttendanceStatsDto
{
    public string CourseId { get; set; } = string.Empty;
    public string CourseName { get; set; } = string.Empty;
    public int TotalSessions { get; set; }
    public int AttendedSessions { get; set; }
    public double AttendancePercent => TotalSessions > 0 ? (double)AttendedSessions / TotalSessions * 100 : 0;

    /// <summary>Class-wide average attendance percentage returned by the API.</summary>
    public double ClassAvgPercent { get; set; }

    /// <summary>Difference between student's attendance and the class average.</summary>
    public double VsClassAverage => ClassAvgPercent > 0 ? AttendancePercent - ClassAvgPercent : 0;

    /// <summary>Human-readable comparison label, e.g. "+8% above class avg".</summary>
    public string ComparisonLabel
    {
        get
        {
            if (ClassAvgPercent <= 0) return string.Empty;
            var diff = Math.Abs(VsClassAverage);
            return VsClassAverage >= 0
                ? $"+{diff:F0}% above class avg"
                : $"-{diff:F0}% below class avg";
        }
    }
}

public class StudentAttendanceReportDto
{
    public string StudentId { get; set; } = string.Empty;
    public string StudentName { get; set; } = string.Empty;
    public List<AttendanceStatsDto> CourseStats { get; set; } = [];
    public double OverallPercent { get; set; }
}

public class LecturerAttendanceDto
{
    public string LecturerId { get; set; } = string.Empty;
    public string LecturerName { get; set; } = string.Empty;
    public string CourseId { get; set; } = string.Empty;
    public int ScheduledSessions { get; set; }
    public int AttendedSessions { get; set; }
}

public class RegisterDto
{
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string StudentId { get; set; } = string.Empty;
    public string Role { get; set; } = "Student";
}
