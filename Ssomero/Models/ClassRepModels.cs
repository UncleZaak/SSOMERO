namespace Ssomero.Models;

public class ClassRepMyClassModel
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string ProgramName { get; set; } = string.Empty;
    public int StudentCount { get; set; }
    public int SubclassCount { get; set; }
    public int LecturerCount { get; set; }

    public string StudentCountLabel    => $"{StudentCount} student{(StudentCount == 1 ? "" : "s")}";
    public string SubclassCountLabel   => $"{SubclassCount} subclass{(SubclassCount == 1 ? "" : "es")}";
    public string LecturerCountLabel   => $"{LecturerCount} lecturer{(LecturerCount == 1 ? "" : "s")}";
    public string DisplayName          => string.IsNullOrWhiteSpace(Name) ? "My Class" : Name;
}

public class ClassRepSubclassModel
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int StudentCount { get; set; }
    public int LecturerCount { get; set; }
    public DateTime CreatedAt { get; set; }

    public string StudentCountLabel  => $"{StudentCount} student{(StudentCount == 1 ? "" : "s")}";
    public string LecturerCountLabel => $"{LecturerCount} lecturer{(LecturerCount == 1 ? "" : "s")}";
    public string DisplayName        => string.IsNullOrWhiteSpace(Name) ? "Subclass" : Name;
}

public class CreateSubclassRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
}

public class RenameSubclassRequest
{
    public string Name { get; set; } = string.Empty;
}

public class ClassRepStudentModel
{
    public Guid Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;

    public string DisplayName => string.IsNullOrWhiteSpace(FullName) ? Email : FullName;
    public string Initials    => FullName.Length > 0 ? string.Concat(FullName.Split(' ').Where(p => p.Length > 0).Take(2).Select(p => p[0].ToString().ToUpper())) : "?";
}

public class ClassRepLecturerModel
{
    public Guid Id { get; set; }
    public string? StaffId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;

    public string DisplayName    => string.IsNullOrWhiteSpace(FullName) ? Email : FullName;
    public string StaffIdDisplay => string.IsNullOrWhiteSpace(StaffId) ? "—" : StaffId;
}

public class AssignLecturerRequest
{
    public Guid LecturerId { get; set; }
}

public class ClassRepAttendanceSummaryModel
{
    public double AverageAttendanceRate { get; set; }
    public int TotalSessions { get; set; }
    public int TotalAttendances { get; set; }

    public string AttendanceRateLabel => $"{AverageAttendanceRate:F1}%";
    public double AttendanceRateProgress => Math.Clamp(AverageAttendanceRate / 100.0, 0, 1);
    public string RateColor => AverageAttendanceRate >= 75 ? "#22C55E" : AverageAttendanceRate >= 50 ? "#F59E0B" : "#EF4444";
}

public class ClassRepStatsModel
{
    public int ManagedClasses { get; set; }
    public int TotalStudents { get; set; }
    public int TotalSubclasses { get; set; }
    public int AssignedLecturers { get; set; }
    public double AverageAttendanceRate { get; set; }

    public string AttendanceRateLabel => $"{AverageAttendanceRate:F1}%";
}

// ── Announcements ─────────────────────────────────────────────────────────────

public class ClassAnnouncementModel
{
    public Guid Id { get; set; }
    public Guid ClassId { get; set; }
    public string ClassName { get; set; } = string.Empty;
    public Guid CreatedBy { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }

    public string TimeAgo
    {
        get
        {
            var diff = DateTime.UtcNow - CreatedAt;
            return diff.TotalMinutes < 1   ? "just now"
                 : diff.TotalHours   < 1   ? $"{(int)diff.TotalMinutes}m ago"
                 : diff.TotalDays    < 1   ? $"{(int)diff.TotalHours}h ago"
                 : diff.TotalDays    < 7   ? $"{(int)diff.TotalDays}d ago"
                 : CreatedAt.ToString("dd MMM yyyy");
        }
    }
}

public class CreateClassAnnouncementRequest
{
    public Guid ClassId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}

// ── Analytics ─────────────────────────────────────────────────────────────────

public class TrendPointModel
{
    public string Label { get; set; } = string.Empty;
    public double Value { get; set; }
}

public class ClassRepAnalyticsModel
{
    public int TotalStudents { get; set; }
    public int TotalSubclasses { get; set; }
    public int AssignedLecturers { get; set; }
    public double AverageAttendanceRate { get; set; }
    public List<TrendPointModel> AttendanceTrend { get; set; } = [];
    public List<TrendPointModel> StudentGrowthTrend { get; set; } = [];

    public string AttendanceRateLabel    => $"{AverageAttendanceRate:F1}%";
    public double AttendanceRateProgress => Math.Clamp(AverageAttendanceRate / 100.0, 0, 1);
    public string RateColor              => AverageAttendanceRate >= 75 ? "#22C55E"
                                         : AverageAttendanceRate >= 50 ? "#F59E0B" : "#EF4444";
}
