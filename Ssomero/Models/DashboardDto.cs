using System.Collections.Generic;
using Ssomero.Models;

namespace Ssomero.Models;

public class DashboardDto
{
    public int ActiveCourses { get; set; }
    public int UpcomingAssignments { get; set; }
    public double AttendancePercent { get; set; }
    public List<AnnouncementDto> RecentAnnouncements { get; set; } = [];

    // Student
    public List<ClassDto>? MyClasses { get; set; }

    // Lecturer
    public List<ClassDto>? TeachingClasses { get; set; }

    // ClassRep
    public List<ClassDto>? ManagedClasses { get; set; }

    // Admin summary
    public int? TotalStudents { get; set; }
    public int? TotalLecturers { get; set; }
    public int? TotalPrograms { get; set; }
}
