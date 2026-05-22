using System;
using AcademicSystem.Domain.Common;
using System.Collections.Generic;

namespace AcademicSystem.Domain.Entities
{
    public class University : AuditableEntity
    {
        public string Name { get; set; } = null!;

        public string? Code { get; set; }

        // Navigation
        public ICollection<Programme> Programmes { get; set; } = new List<Programme>();

        public ICollection<User> Users { get; set; } = new List<User>();
    }
}
