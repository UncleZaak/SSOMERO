using System;

namespace AcademicSystem.Domain.Common
{
    /// <summary>
    /// Marker for domain events. Keep as simple POCO.
    /// Infrastructure can subscribe and dispatch these events after SaveChanges.
    /// </summary>
    public abstract class DomainEvent
    {
        public DateTime OccurredOn { get; } = DateTime.UtcNow;
    }
}
