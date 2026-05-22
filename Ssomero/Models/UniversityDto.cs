namespace Ssomero.Models;

public class UniversityDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int FacultiesCount { get; set; }
    public string Status { get; set; } = "Active";
}
