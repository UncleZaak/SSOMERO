namespace Ssomero.Models;

public class AttendanceDto
{
    public string CourseId { get; set; } = string.Empty;
    public int Present { get; set; }
    public int Total { get; set; }
}