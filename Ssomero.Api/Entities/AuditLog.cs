using System.ComponentModel.DataAnnotations;

namespace Ssomero.Api.Entities;

public class AuditLog
{
    public Guid Id { get; set; }

    [Required, MaxLength(100)]
    public string Action { get; set; } = string.Empty;

    [Required, MaxLength(100)]
    public string EntityName { get; set; } = string.Empty;

    [MaxLength(50)]
    public string? EntityId { get; set; }

    public Guid? UserId { get; set; }

    [MaxLength(200)]
    public string? UserEmail { get; set; }

    [MaxLength(50)]
    public string? UserRole { get; set; }

    public string? OldValues { get; set; }

    public string? NewValues { get; set; }

    [MaxLength(50)]
    public string? IpAddress { get; set; }

    [MaxLength(500)]
    public string? UserAgent { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
