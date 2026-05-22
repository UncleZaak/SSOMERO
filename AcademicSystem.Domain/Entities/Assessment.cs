using System;
using AcademicSystem.Domain.Common;
using System.Collections.Generic;

namespace AcademicSystem.Domain.Entities
{
    public class Assessment : AuditableEntity
    {
        public Guid ClassCourseId { get; set; }

        public string Title { get; set; } = null!;

        public string? Description { get; set; }

        public DateTime? DueDate { get; set; }

        public decimal MaxScore { get; set; }

        // Navigation
        public ClassCourse ClassCourse { get; set; } = null!;

        public ICollection<Submission> Submissions { get; set; } = new List<Submission>();
    }
}
