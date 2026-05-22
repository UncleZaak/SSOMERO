namespace Ssomero.Models;

public class CourseDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Lecturer { get; set; } = string.Empty;
    public string LecturerId { get; set; } = string.Empty;
    public int Progress { get; set; }
    public int EnrolledStudents { get; set; }
    public int TotalSessions { get; set; }
    public int CompletedSessions { get; set; }
    public string? ClassRepId { get; set; }
    public string? ClassRepName { get; set; }
}