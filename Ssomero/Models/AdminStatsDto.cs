namespace Ssomero.Models;

public class AdminStatsDto
{
    public int TotalStudents { get; set; }
    public int TotalLecturers { get; set; }
    public int TotalPrograms { get; set; }
    public int TotalClasses { get; set; }
    public int TotalUniversities { get; set; }
    public int TotalFaculties { get; set; }
    public int TotalDepartments { get; set; }
    public int ActiveStudents { get; set; }
    public int SuspendedStudents { get; set; }
    public int PendingLecturers { get; set; }
    public double AverageAttendanceRate { get; set; }
}
