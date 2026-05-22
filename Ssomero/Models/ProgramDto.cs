namespace Ssomero.Models;

public class ProgramDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string DepartmentId { get; set; } = string.Empty;
    public string DepartmentName { get; set; } = string.Empty;
    public string FacultyId { get; set; } = string.Empty;
    public string FacultyName { get; set; } = string.Empty;
    public string UniversityId { get; set; } = string.Empty;
    public string UniversityName { get; set; } = string.Empty;
    public int DurationSemesters { get; set; }
    public int CurriculumCount { get; set; }
    public string Status { get; set; } = "Active";

    /// <summary>Disambiguated display name when duplicate program names exist across departments.</summary>
    public string DisplayName => string.IsNullOrEmpty(DepartmentName)
        ? Name
        : $"{Name} ({DepartmentName})";

    /// <summary>Picker display label: "{Name} ({DepartmentName})".</summary>
    public string DisplayLabel => DisplayName;
}
