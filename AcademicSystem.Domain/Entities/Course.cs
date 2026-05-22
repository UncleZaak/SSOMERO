using System;
using AcademicSystem.Domain.Common;
using System.Collections.Generic;

namespace AcademicSystem.Domain.Entities
{
    public class Course : AuditableEntity
    {
        public Guid ProgrammeId { get; set; }

        public string Code { get; set; } = null!;

        public string Title { get; set; } = null!;

        public string? Description { get; set; }

        public int? Credits { get; set; }

        public Programme Programme { get; set; } = null!;

        public ICollection<ClassCourse> ClassCourses { get; set; } = new List<ClassCourse>();

        public ICollection<Assessment> Assessments { get; set; } = new List<Assessment>();
    }
}
