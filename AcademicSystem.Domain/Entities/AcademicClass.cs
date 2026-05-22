using System;
using AcademicSystem.Domain.Common;
using System.Collections.Generic;

namespace AcademicSystem.Domain.Entities
{
    public class AcademicClass : AuditableEntity
    {
        public Guid ProgrammeId { get; set; }

        public string Name { get; set; } = null!;

        public string AcademicYear { get; set; } = null!;

        public string Semester { get; set; } = null!;

        // Navigation
        public Programme Programme { get; set; } = null!;

        public ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();

        public ICollection<ClassCourse> ClassCourses { get; set; } = new List<ClassCourse>();

        public ICollection<Announcement> Announcements { get; set; } = new List<Announcement>();
    }
}
