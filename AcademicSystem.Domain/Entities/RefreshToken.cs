using System;
using AcademicSystem.Domain.Common;

namespace AcademicSystem.Domain.Entities
{
    public class RefreshToken : AuditableEntity
    {
        public Guid UserId { get; set; }

        public string TokenHash { get; set; } = null!;

        public DateTime ExpiresAt { get; set; }

        public string? CreatedByIp { get; set; }

        public DateTime? RevokedAt { get; set; }

        public string? ReplacedByTokenHash { get; set; }

        public bool IsRevoked { get; set; }

        // Navigation
        public User User { get; set; } = null!;
    }
}
