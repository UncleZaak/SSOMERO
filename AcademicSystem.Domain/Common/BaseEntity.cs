using System;

namespace AcademicSystem.Domain.Common
{
    /// <summary>
    /// Base entity with an Id, concurrency token (RowVersion) and soft-delete flag.
    /// Persistence mapping (EF Core) must be implemented in the Infrastructure layer.
    /// </summary>
    public abstract class BaseEntity
    {
        protected BaseEntity()
        {
            Id = Guid.NewGuid();
        }

        public Guid Id { get; protected set; }

        /// <summary>
        /// Concurrency token conceptually equivalent to SQL rowversion. Kept as a byte[] here
        /// so infrastructure can map it to a database-specific concurrency token.
        /// </summary>
        public byte[]? RowVersion { get; set; }

        /// <summary>
        /// Soft-delete marker. Use SoftDelete() to mark an entity logically deleted.
        /// </summary>
        public bool IsDeleted { get; private set; }

        public void SoftDelete()
        {
            IsDeleted = true;
        }

        public void Restore()
        {
            IsDeleted = false;
        }
    }
}
