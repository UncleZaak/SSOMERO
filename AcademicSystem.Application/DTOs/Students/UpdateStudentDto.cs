using System;
using System.ComponentModel.DataAnnotations;

namespace AcademicSystem.Application.DTOs.Students
{
    public class UpdateStudentDto
    {
        [Required]
        [StringLength(50)]
        public string RegistrationNumber { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public Guid UniversityId { get; set; }

        [Required]
        public Guid ProgrammeId { get; set; }
    }
}
