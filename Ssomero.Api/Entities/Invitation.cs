using System;
using System.ComponentModel.DataAnnotations;

namespace Ssomero.Api.Entities;

public class Invitation
{
    public Guid Id { get; set; }

    public Guid? ClassId { get; set; }

    public Guid InviterId { get; set; }

    public Guid? InviteeStudentId { get; set; }

    [MaxLength(320)]
    public string? InviteeContact { get; set; }

    [Required, MaxLength(32)]
    public string Purpose { get; set; } = string.Empty;

    [Required, MaxLength(512)]
    public string TokenHash { get; set; } = string.Empty;

    [Required, MaxLength(64)]
    public string TokenKeyId { get; set; } = string.Empty;

    [Required, MaxLength(32)]
    public string Status { get; set; } = "Created";

    public DateTime CreatedAt { get; set; }

    public DateTime ExpiresAt { get; set; }

    public DateTime? ConsumedAt { get; set; }

    public int UsesCount { get; set; }

    public int MaxUses { get; set; } = 1;

    public bool SingleUse { get; set; } = true;

    public string? Metadata { get; set; }
}
