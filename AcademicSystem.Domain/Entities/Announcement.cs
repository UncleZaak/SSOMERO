using System;
using AcademicSystem.Domain.Common;

namespace AcademicSystem.Domain.Entities
{
    public class Announcement : AuditableEntity
    {
        public Guid ClassId { get; set; }

        public Guid PostedByUserId { get; set; }

        public string Title { get; set; } = null!;

        public string Content { get; set; } = null!;

        public DateTime PostedAt { get; set; }

        // Navigation
        public AcademicClass Class { get; set; } = null!;

        public User PostedBy { get; set; } = null!;
    }
}
