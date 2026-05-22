using System;
using System.ComponentModel.DataAnnotations;

namespace Ssomero.Api.Entities;

public class Otp
{
    public Guid Id { get; set; }

    [Required, MaxLength(200)]
    public string Email { get; set; } = string.Empty;

    [Required, MaxLength(200)]
    public string OtpCode { get; set; } = string.Empty; // Now stores bcrypt hash

    public DateTime ExpiresAt { get; set; }
    public bool IsUsed { get; set; }
    public DateTime? VerifiedAt { get; set; }

    [MaxLength(200)]
    public string? VerificationToken { get; set; }
    public DateTime? VerificationTokenExpiresAt { get; set; }
}
