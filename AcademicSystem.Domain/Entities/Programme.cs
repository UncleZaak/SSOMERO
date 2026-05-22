using System;
using AcademicSystem.Domain.Common;
using System.Collections.Generic;

namespace AcademicSystem.Domain.Entities
{
    public class Programme : AuditableEntity
    {
        public Guid UniversityId { get; set; }

        public string Name { get; set; } = null!;

        public string? Code { get; set; }

        // Navigation
        public University University { get; set; } = null!;

        public ICollection<AcademicClass> AcademicClasses { get; set; } = new List<AcademicClass>();

        public ICollection<Course> Courses { get; set; } = new List<Course>();
    }
}
