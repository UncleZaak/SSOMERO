using System;

namespace AcademicSystem.Domain.Common
{
    public abstract class AuditableEntity : BaseEntity
    {
        protected AuditableEntity() : base()
        {
        }

        public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
        public Guid? CreatedBy { get; private set; }

        public DateTime? UpdatedAt { get; private set; }
        public Guid? UpdatedBy { get; private set; }

        public void SetCreated(Guid? createdBy)
        {
            CreatedAt = DateTime.UtcNow;
            CreatedBy = createdBy;
        }

        public void SetUpdated(Guid? updatedBy)
        {
            UpdatedAt = DateTime.UtcNow;
            UpdatedBy = updatedBy;
        }
    }
}
