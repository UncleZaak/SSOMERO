using System.ComponentModel.DataAnnotations;

namespace Ssomero.Api.Entities;

/// <summary>
/// Tracks a password-reset flow: OTP request → OTP verification → password change.
/// OtpHash and ResetTokenHash are NEVER stored in plaintext.
/// </summary>
public class PasswordResetRequest
{
    public Guid Id { get; set; }

    /// <summary>Normalized (lowercase) email of the requester.</summary>
    [Required, MaxLength(200)]
    public string Email { get; set; } = string.Empty;

    /// <summary>BCrypt hash of the 6-digit OTP. Never stored in plaintext.</summary>
    [Required]
    public string OtpHash { get; set; } = string.Empty;

    /// <summary>
    /// BCrypt hash of the short-lived reset token issued after OTP verification.
    /// Null until the OTP has been successfully verified.
    /// </summary>
    public string? ResetTokenHash { get; set; }

    public DateTime ExpiresAt { get; set; }

    /// <summary>Number of failed OTP verification attempts for this request.</summary>
    public int Attempts { get; set; }

    /// <summary>True once the password has been reset (or the request has been consumed).</summary>
    public bool IsUsed { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
