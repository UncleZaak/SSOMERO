namespace Ssomero.Models;

public class StudyGroupDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string CourseId { get; set; } = string.Empty;
    public string CourseName { get; set; } = string.Empty;
    public int MemberCount { get; set; }
    public string CreatedByLecturerId { get; set; } = string.Empty;
}
