namespace Ssomero.Models;

public class DropoutRequestDto
{
    public string Id { get; set; } = string.Empty;
    public string StudentId { get; set; } = string.Empty;
    public string StudentName { get; set; } = string.Empty;
    public string CourseId { get; set; } = string.Empty;
    public string CourseName { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
    public DropoutStatus Status { get; set; }
    public DateTime RequestedAt { get; set; }
    public string? ApprovedByClassRepId { get; set; }
}

public enum DropoutStatus
{
    Pending,
    Approved,
    Rejected
}
