using System;
using System.Collections.Generic;

using System.Collections.Generic;
using AcademicSystem.Domain.Common;

namespace AcademicSystem.Domain.Entities
{
    public class User : AuditableEntity
    {
        public string Email { get; set; } = null!;

        public string NormalizedEmail { get; set; } = null!;

        public string? PasswordHash { get; set; }

        public int Role { get; set; }

        public Guid? UniversityId { get; set; }

        // Navigation properties
        public University? University { get; set; }

        public ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();

        public ICollection<Submission> Submissions { get; set; } = new List<Submission>();

        public ICollection<Announcement> Announcements { get; set; } = new List<Announcement>();

        public ICollection<ClassCourse> ClassCoursesTaught { get; set; } = new List<ClassCourse>();

        public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
    }
}
