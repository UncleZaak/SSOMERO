using System;

namespace AcademicSystem.Application.DTOs.Students
{
    public class StudentDto
    {
        public Guid Id { get; set; }
        public string RegistrationNumber { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public Guid UniversityId { get; set; }
        public Guid ProgrammeId { get; set; }

        // UI-friendly fields (add only if frontend needs them)
        public string? FullName { get; set; }
        public DateTime? CreatedAt { get; set; }
    }
}
