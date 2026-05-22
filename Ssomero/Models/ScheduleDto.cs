namespace Ssomero.Models;

public class ScheduleDto
{
    public string Id { get; set; } = string.Empty;
    public string CourseId { get; set; } = string.Empty;
    public string CourseName { get; set; } = string.Empty;
    public string LecturerName { get; set; } = string.Empty;
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public string Venue { get; set; } = string.Empty;
    public ScheduleType Type { get; set; }
    public SessionKind SessionKind { get; set; }
    public bool IsCancelled { get; set; }
}

public enum ScheduleType
{
    Class,
    Test,
    Exam
}

public enum SessionKind
{
    Physical,
    Online
}
