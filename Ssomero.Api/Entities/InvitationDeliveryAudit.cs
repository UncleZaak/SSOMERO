using System;
using System.ComponentModel.DataAnnotations;

namespace Ssomero.Api.Entities;

public class InvitationDeliveryAudit
{
    public Guid Id { get; set; }

    public Guid DeliveryId { get; set; }

    [Required, MaxLength(32)]
    public string EventType { get; set; } = string.Empty; // Enqueued, ProcessingStarted, Sent, Failed, Retried, DeadLettered

    public DateTime Timestamp { get; set; }

    public string? Details { get; set; }

    public string? CorrelationId { get; set; }
}
