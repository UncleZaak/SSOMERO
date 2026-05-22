using System;

namespace AcademicSystem.Application.DTOs.Enrollments
{
    public class EnrollmentDto
    {
        public Guid Id { get; set; }
        public Guid StudentId { get; set; }
        public Guid ClassId { get; set; }
        public DateTime EnrolledAt { get; set; }
    }
}
