namespace Ssomero.Models;

public class ClassDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? CourseCode { get; set; }
    public string? ParentClassId { get; set; }
    public int EnrolledStudents { get; set; }
    public string? LecturerName { get; set; }

    public ClassDto() { }
    public ClassDto(string id, string name, string? courseCode, string? parentClassId, int enrolledStudents, string? lecturerName)
    {
        Id = id;
        Name = name;
        CourseCode = courseCode;
        ParentClassId = parentClassId;
        EnrolledStudents = enrolledStudents;
        LecturerName = lecturerName;
    }

    // Convenience for bindings expecting Id as Guid
    public Guid IdAsGuid => Guid.TryParse(Id, out var g) ? g : Guid.Empty;
}
