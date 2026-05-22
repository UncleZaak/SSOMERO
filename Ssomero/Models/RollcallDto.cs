namespace Ssomero.Models;

public class RollcallDto
{
    public string Id { get; set; } = string.Empty;
    public string ScheduleId { get; set; } = string.Empty;
    public string StudentId { get; set; } = string.Empty;
    public string StudentName { get; set; } = string.Empty;
    public string CourseName { get; set; } = string.Empty;
    public string SelfieUrl { get; set; } = string.Empty;
    public DateTime SubmittedAt { get; set; }
    public RollcallStatus Status { get; set; }
    public string? ApprovedByClassRepId { get; set; }
    public string? ApprovedByLecturerId { get; set; }
}

public enum RollcallStatus
{
    Pending,
    ApprovedByClassRep,
    ApprovedByLecturer,
    Rejected
}
