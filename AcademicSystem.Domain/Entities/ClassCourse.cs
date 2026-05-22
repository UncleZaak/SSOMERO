using System;
using AcademicSystem.Domain.Common;
using System.Collections.Generic;

namespace AcademicSystem.Domain.Entities
{
    public class ClassCourse : AuditableEntity
    {
        public Guid ClassId { get; set; }

        public Guid CourseId { get; set; }

        public Guid? LecturerId { get; set; }

        // Navigation
        public AcademicClass Class { get; set; } = null!;

        public Course Course { get; set; } = null!;

        public User? Lecturer { get; set; }

        public ICollection<Assessment> Assessments { get; set; } = new List<Assessment>();
    }
}
