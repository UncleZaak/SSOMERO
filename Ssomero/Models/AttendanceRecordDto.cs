namespace Ssomero.Models;

public class AttendanceRecordDto
{
    public Guid Id { get; set; }
    public Guid ClassId { get; set; }
    public Guid? SessionId { get; set; }
    public string CourseName { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public bool IsPresent { get; set; }
    public DateTime? SubmittedAt { get; set; }

    public string StatusLabel  => IsPresent ? "Present ✅" : "Absent ❌";
    public string StatusColor  => IsPresent ? "#10B981" : "#EF4444";
    public string DateLabel    => Date.ToString("EEE, MMM dd yyyy");
}
