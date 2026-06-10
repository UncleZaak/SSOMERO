using System;
using System.ComponentModel.DataAnnotations;

namespace Ssomero.Api.Entities;

public class InvitationAudit
{
    public Guid Id { get; set; }

    public Guid InvitationId { get; set; }

    [Required, MaxLength(32)]
    public string EventType { get; set; } = string.Empty;

    public Guid? ActorId { get; set; }

    public DateTime Timestamp { get; set; }

    public string? Details { get; set; }

    public string? CorrelationId { get; set; }
}
