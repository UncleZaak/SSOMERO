using System;
using AcademicSystem.Domain.Common;
using AcademicSystem.Domain.Enums;

namespace AcademicSystem.Domain.Entities
{
    public class Student : AuditableEntity
    {
        private Student()
        {
        }

        public Student(string email, Guid universityId, Guid programmeId, Guid academicYearId, Guid semesterId)
            : base()
        {
            if (email is null) throw new ArgumentNullException(nameof(email));
            var trimmed = email.Trim();
            Email = trimmed;
            NormalizedEmail = trimmed.ToLowerInvariant();
            UniversityId = universityId;
            ProgrammeId = programmeId;
            AcademicYearId = academicYearId;
            SemesterId = semesterId;
        }

        public string Email { get; private set; } = null!;

        // Normalized (lowercase, trimmed) email used for uniqueness checks and indexing
        public string NormalizedEmail { get; private set; } = null!;

        // Password storage is handled by Infrastructure (Identity) — here we keep a placeholder for hashed password if needed.
        public string? PasswordHash { get; private set; }

        public Guid UniversityId { get; private set; }
        public Guid ProgrammeId { get; private set; }
        public Guid AcademicYearId { get; private set; }
        public Guid SemesterId { get; private set; }

        public UserRole Role { get; private set; } = UserRole.Student;

        public void SetPasswordHash(string hashed)
        {
            PasswordHash = hashed;
        }
    }
}
