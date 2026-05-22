using System;

namespace AcademicSystem.Application.DTOs.Auth
{
    public class RefreshTokenDto
    {
        public Guid Id { get; set; }
        public string Token { get; set; } = string.Empty;
        public DateTime Expires { get; set; }
    }
}
