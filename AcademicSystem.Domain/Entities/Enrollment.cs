using System;
using AcademicSystem.Domain.Common;

namespace AcademicSystem.Domain.Entities
{
    public class Enrollment : AuditableEntity
    {
        public Guid StudentId { get; set; }

        public Guid ClassId { get; set; }

        public DateTime EnrolledAt { get; set; }

        public int Status { get; set; }

        // Navigation
        public User Student { get; set; } = null!;

        public AcademicClass Class { get; set; } = null!;
    }
}
