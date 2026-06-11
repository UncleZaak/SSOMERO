using System;
using System.ComponentModel.DataAnnotations;

namespace Ssomero.Api.Entities;

public class InvitationDelivery
{
    public Guid Id { get; set; }

    public Guid InvitationId { get; set; }

    [Required, MaxLength(320)]
    public string Recipient { get; set; } = string.Empty;

    [Required, MaxLength(200)]
    public string Subject { get; set; } = string.Empty;

    public string? BodyHtml { get; set; }

    [Required, MaxLength(32)]
    public string Status { get; set; } = "Queued"; // Queued, Processing, Sent, Failed, Retried, Expired

    public int RetryCount { get; set; }

    public DateTime QueuedAt { get; set; }

    public DateTime? ProcessingAt { get; set; }

    public DateTime? SentAt { get; set; }

    public DateTime? FailedAt { get; set; }

    public string? FailureReason { get; set; }

    public string? Metadata { get; set; }
}
