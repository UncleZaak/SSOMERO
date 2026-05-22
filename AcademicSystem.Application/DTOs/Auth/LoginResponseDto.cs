namespace AcademicSystem.Application.DTOs.Auth
{
    public class LoginResponseDto : AuthResponseDto
    {
        public string Email { get; set; } = string.Empty;
    }
}