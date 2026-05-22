using System;

namespace AcademicSystem.Application.DTOs.Enrollments
{
    public class UpdateEnrollmentDto
    {
        public Guid StudentId { get; set; }
        public Guid ClassId { get; set; }
    }
}
