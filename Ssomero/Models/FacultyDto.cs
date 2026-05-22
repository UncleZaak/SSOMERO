namespace Ssomero.Models;

public class FacultyDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string UniversityId { get; set; } = string.Empty;
    public string UniversityName { get; set; } = string.Empty;
    public int DepartmentsCount { get; set; }
    public string Status { get; set; } = "Active";

    /// <summary>Picker display label with parent context: "{Name} ({UniversityName})".</summary>
    public string DisplayLabel => string.IsNullOrEmpty(UniversityName)
        ? Name
        : $"{Name} ({UniversityName})";
}
