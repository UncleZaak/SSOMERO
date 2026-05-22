namespace Ssomero.Models;

public class LecturerRegisterDto
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string? Photo { get; set; }
    public string? StaffId { get; set; }
    public string VerificationToken { get; set; } = string.Empty;
}
