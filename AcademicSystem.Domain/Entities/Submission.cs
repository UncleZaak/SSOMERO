using System;
using AcademicSystem.Domain.Common;

namespace AcademicSystem.Domain.Entities
{
    public class Submission : AuditableEntity
    {
        public Guid AssessmentId { get; set; }

        public Guid StudentId { get; set; }

        public DateTime SubmittedAt { get; set; }

        public decimal? Score { get; set; }

        public string? Feedback { get; set; }

        public string FileName { get; set; } = null!;

        public string ContentType { get; set; } = null!;

        public long FileSize { get; set; }

        public string StoragePath { get; set; } = null!;

        // Navigation
        public Assessment Assessment { get; set; } = null!;

        public User Student { get; set; } = null!;
    }
}
