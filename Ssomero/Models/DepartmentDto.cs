namespace Ssomero.Models;

public class DepartmentDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string FacultyId { get; set; } = string.Empty;
    public string FacultyName { get; set; } = string.Empty;
    public string UniversityId { get; set; } = string.Empty;
    public string UniversityName { get; set; } = string.Empty;
    public int ProgramsCount { get; set; }
    public string Status { get; set; } = "Active";

    /// <summary>Disambiguated display name when duplicate department names exist across faculties.</summary>
    public string DisplayName => string.IsNullOrEmpty(UniversityName)
        ? Name
        : $"{Name} ({UniversityName})";

    /// <summary>Picker display label with full hierarchy context: "{Name} ({FacultyName} - {UniversityName})".</summary>
    public string DisplayLabel => string.IsNullOrEmpty(FacultyName)
        ? Name
        : string.IsNullOrEmpty(UniversityName)
            ? $"{Name} ({FacultyName})"
            : $"{Name} ({FacultyName} - {UniversityName})";
}
