using System;

namespace AcademicSystem.Application.DTOs.Enrollments
{
    public class CreateEnrollmentDto
    {
        public Guid StudentId { get; set; }
        public Guid ClassId { get; set; }
    }
}
