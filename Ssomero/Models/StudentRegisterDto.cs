namespace Ssomero.Models;

public class StudentRegisterDto
{
    public string FirstName { get; set; } = string.Empty;
    public string SecondName { get; set; } = string.Empty;
    public string? OtherNames { get; set; }
    public string Dob { get; set; } = string.Empty; // "yyyy-MM-dd"
    public string Gender { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string? Photo { get; set; }
    public string VerificationToken { get; set; } = string.Empty;
    public string UniversityId { get; set; } = string.Empty;
    public string FacultyId { get; set; } = string.Empty;
    public string DepartmentId { get; set; } = string.Empty;
    public string ProgramId { get; set; } = string.Empty;
    public string EntrySchemeId { get; set; } = string.Empty;
    public string IntakeId { get; set; } = string.Empty;
    public string StudyModeId { get; set; } = string.Empty;
    public string AcademicYearId { get; set; } = string.Empty;
    public int YearOfStudy { get; set; }
    public string SemesterId { get; set; } = string.Empty;
}
