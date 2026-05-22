using System;
using System.ComponentModel.DataAnnotations;

namespace AcademicSystem.Application.DTOs.Courses
{
    public class CreateCourseDto
    {
        [Required]
        [StringLength(256)]
        public string Title { get; set; } = string.Empty;

        [StringLength(50)]
        public string? Code { get; set; }

        [Required]
        public Guid ProgrammeId { get; set; }

        [Range(0, 30)]
        public int Credits { get; set; }
    }
}
