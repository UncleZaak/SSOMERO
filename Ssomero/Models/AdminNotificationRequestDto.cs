namespace Ssomero.Models;

public class AdminNotificationRequestDto
{
    public string Title { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public string? TargetRole { get; set; }
    public string? TargetClassId { get; set; }
}

public class AdminNotificationDto
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public string? TargetRole { get; set; }
    public DateTime SentAt { get; set; }
    public int RecipientsCount { get; set; }
    public string SentBy { get; set; } = string.Empty;

    public string DisplayTarget => string.IsNullOrEmpty(TargetRole) ? "All Users" : TargetRole;
    public string DisplayDate => SentAt.ToString("MMM dd, yyyy HH:mm");
}

public class AdminAttendanceSummaryDto
{
    public string ClassId { get; set; } = string.Empty;
    public string ClassName { get; set; } = string.Empty;
    public string LecturerName { get; set; } = string.Empty;
    public int TotalSessions { get; set; }
    public int TotalStudents { get; set; }
    public double AverageAttendanceRate { get; set; }

    public string AttendanceDisplay => $"{AverageAttendanceRate:F1}%";
    public Color AttendanceColor => AverageAttendanceRate >= 75
        ? Color.FromArgb("#388E3C")
        : AverageAttendanceRate >= 50
            ? Color.FromArgb("#F57C00")
            : Color.FromArgb("#D32F2F");
}
