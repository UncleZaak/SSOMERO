using System;

namespace AcademicSystem.Application.DTOs.Courses
{
    public class CourseDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Code { get; set; }
        public Guid ProgrammeId { get; set; }
        public int Credits { get; set; }
    }
}
