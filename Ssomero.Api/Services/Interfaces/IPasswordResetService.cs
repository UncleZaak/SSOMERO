namespace Ssomero.Api.Services.Interfaces;

public interface IPasswordResetService
{
    /// <summary>
    /// Sends a password-reset OTP to the address if it belongs to any user type.
    /// Always returns without revealing whether the account exists.
    /// </summary>
    Task SendResetOtpAsync(string email, CancellationToken ct = default);

    /// <summary>
    /// Verifies the reset OTP.  On success returns a plaintext reset token to present
    /// in the next step.  Returns null when the OTP is invalid, expired, or exhausted.
    /// </summary>
    Task<string?> VerifyResetOtpAsync(string email, string otp, CancellationToken ct = default);

    /// <summary>
    /// Resets the password for the account identified by <paramref name="email"/>.
    /// The <paramref name="resetToken"/> must be the value returned by
    /// <see cref="VerifyResetOtpAsync"/>.  Returns false on invalid/expired token.
    /// </summary>
    Task<bool> ResetPasswordAsync(string email, string resetToken, string newPassword, CancellationToken ct = default);
}
