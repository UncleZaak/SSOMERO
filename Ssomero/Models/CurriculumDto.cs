namespace Ssomero.Models;

public class CurriculumDto
{
    public string Id { get; set; } = string.Empty;
    public string ProgramId { get; set; } = string.Empty;
    public string ProgramName { get; set; } = string.Empty;
    public string DepartmentName { get; set; } = string.Empty;
    public string FacultyName { get; set; } = string.Empty;
    public string UniversityName { get; set; } = string.Empty;
    public string CourseCode { get; set; } = string.Empty;
    public string CourseName { get; set; } = string.Empty;
    public int YearOfStudy { get; set; }
    public string SemesterId { get; set; } = string.Empty;
    public string SemesterName { get; set; } = string.Empty;
    public string Status { get; set; } = "Active";

    public string DisplayTitle => $"{CourseCode} — {CourseName}";
    public string DisplaySubtitle => $"Year {YearOfStudy} · {SemesterName} · {ProgramName}";

    /// <summary>Picker display label: "{CourseCode} - {CourseName}".</summary>
    public string DisplayLabel => $"{CourseCode} - {CourseName}";
}
