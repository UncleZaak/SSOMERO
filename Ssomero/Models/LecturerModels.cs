namespace Ssomero.Models;

public class LecturerClassDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? CourseCode { get; set; }
    public int EnrolledStudents { get; set; }
    public int TotalSessions { get; set; }
}

public class SessionSummaryDto
{
    public Guid Id { get; set; }
    public string DayOfWeek { get; set; } = string.Empty;
    public string StartTime { get; set; } = string.Empty;
    public string EndTime { get; set; } = string.Empty;
    public string? Location { get; set; }
    public bool IsActive { get; set; }

    public string DisplayTime => $"{DayOfWeek}  {StartTime}–{EndTime}";
    public string LocationLabel => string.IsNullOrWhiteSpace(Location) ? "No location set" : Location;
}

public class LecturerClassDetailDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? CourseCode { get; set; }
    public int EnrolledStudents { get; set; }
    public List<SessionSummaryDto> Sessions { get; set; } = [];
}

public class LecturerStudentDto
{
    public Guid Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
}

public class SessionAttendanceDto
{
    public Guid AttendanceId { get; set; }
    public Guid StudentId { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public bool IsPresent { get; set; }
    public DateTime? SubmittedAt { get; set; }
}

public class LecturerMaterialDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? FileUrl { get; set; }
    public DateTime CreatedAt { get; set; }

    public string CreatedAtLabel => CreatedAt.ToLocalTime().ToString("dd MMM yyyy, HH:mm");
    public bool HasUrl => !string.IsNullOrWhiteSpace(FileUrl);
}
