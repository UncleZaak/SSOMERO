using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;
using Ssomero.Api.Data;
using Ssomero.Api.Entities;
using Ssomero.Api.Services.Interfaces;

namespace Ssomero.Api.Services.Implementations;

public sealed class PasswordResetService : IPasswordResetService
{
    private const int OtpExpiryMinutes = 5;
    private const int ResetTokenExpiryMinutes = 10;
    private const int MaxOtpAttempts = 5;
    private const int CooldownSeconds = 60;

    private readonly SsomeroDbContext _db;
    private readonly EmailService _email;
    private readonly ILogger<PasswordResetService> _logger;

    public PasswordResetService(
        SsomeroDbContext db,
        EmailService email,
        ILogger<PasswordResetService> logger)
    {
        _db = db;
        _email = email;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task SendResetOtpAsync(string email, CancellationToken ct = default)
    {
        var normalized = email.ToLowerInvariant();

        // Enforce cooldown: reject if an unused, non-expired request was created within
        // the last CooldownSeconds to prevent OTP flooding.
        var cooldownThreshold = DateTime.UtcNow.AddSeconds(-CooldownSeconds);
        var recent = await _db.PasswordResetRequests
            .Where(r => r.Email == normalized && !r.IsUsed && r.CreatedAt > cooldownThreshold)
            .AnyAsync(ct);

        if (recent)
        {
            // Still return success — never leak timing information about account existence.
            _logger.LogWarning("Password reset OTP cooldown active for {Email}", normalized);
            return;
        }

        // Silently check whether the email belongs to any user type.
        var userExists =
            await _db.Students.IgnoreQueryFilters().AnyAsync(s => s.Email == normalized && !s.IsDeleted, ct) ||
            await _db.Lecturers.IgnoreQueryFilters().AnyAsync(l => l.Email == normalized && !l.IsDeleted, ct) ||
            await _db.Admins.IgnoreQueryFilters().AnyAsync(a => a.Email == normalized && !a.IsDeleted, ct);

        if (!userExists)
        {
            // Do NOT reveal that the account does not exist.
            _logger.LogInformation("Password reset requested for unknown email (suppressed): {Email}", normalized);
            return;
        }

        // Cryptographically secure 6-digit OTP.
        var otp = RandomNumberGenerator.GetInt32(100_000, 1_000_000).ToString();
        var otpHash = BCrypt.Net.BCrypt.HashPassword(otp);

        var request = new PasswordResetRequest
        {
            Id = Guid.NewGuid(),
            Email = normalized,
            OtpHash = otpHash,
            ExpiresAt = DateTime.UtcNow.AddMinutes(OtpExpiryMinutes),
            CreatedAt = DateTime.UtcNow
        };

        _db.PasswordResetRequests.Add(request);
        await _db.SaveChangesAsync(ct);

        try
        {
            await _email.SendEmailAsync(
                normalized,
                "Your password reset code",
                $"Your password reset code is: {otp}\n\nIt expires in {OtpExpiryMinutes} minutes.\n\nIf you did not request this, please ignore this email.");
        }
        catch (Exception ex)
        {
            // Remove the stored request so the user can retry immediately.
            _db.PasswordResetRequests.Remove(request);
            await _db.SaveChangesAsync(ct);
            _logger.LogError(ex, "Failed to send password reset email to {Email}", normalized);
            throw;
        }

        _logger.LogInformation("Password reset OTP sent for {Email}", normalized);
    }

    /// <inheritdoc/>
    public async Task<string?> VerifyResetOtpAsync(string email, string otp, CancellationToken ct = default)
    {
        var normalized = email.ToLowerInvariant();

        var request = await _db.PasswordResetRequests
            .Where(r => r.Email == normalized && !r.IsUsed)
            .OrderByDescending(r => r.CreatedAt)
            .FirstOrDefaultAsync(ct);

        if (request is null)
        {
            _logger.LogWarning("Password reset OTP verification failed — no pending request for {Email}", normalized);
            return null;
        }

        if (request.ExpiresAt <= DateTime.UtcNow)
        {
            _logger.LogWarning("Password reset OTP expired for {Email}", normalized);
            return null;
        }

        if (request.Attempts >= MaxOtpAttempts)
        {
            _logger.LogWarning("Password reset OTP max attempts exceeded for {Email}", normalized);
            return null;
        }

        if (!BCrypt.Net.BCrypt.Verify(otp, request.OtpHash))
        {
            request.Attempts++;
            await _db.SaveChangesAsync(ct);
            _logger.LogWarning(
                "Invalid reset OTP for {Email} — attempt {Attempt}/{Max}",
                normalized, request.Attempts, MaxOtpAttempts);
            return null;
        }

        // OTP is valid — issue a short-lived reset token.
        var tokenBytes = RandomNumberGenerator.GetBytes(32);
        var plainToken = Convert.ToBase64String(tokenBytes);

        request.ResetTokenHash = BCrypt.Net.BCrypt.HashPassword(plainToken);
        // Extend window so the user has time to submit the new password.
        request.ExpiresAt = DateTime.UtcNow.AddMinutes(ResetTokenExpiryMinutes);
        // Mark the OTP itself as consumed (the reset token is a separate credential).
        request.OtpHash = string.Empty;

        await _db.SaveChangesAsync(ct);

        _logger.LogInformation("Password reset OTP verified for {Email}", normalized);
        return plainToken;
    }

    /// <inheritdoc/>
    public async Task<bool> ResetPasswordAsync(
        string email, string resetToken, string newPassword, CancellationToken ct = default)
    {
        var normalized = email.ToLowerInvariant();

        var request = await _db.PasswordResetRequests
            .Where(r => r.Email == normalized && !r.IsUsed && r.ResetTokenHash != null)
            .OrderByDescending(r => r.CreatedAt)
            .FirstOrDefaultAsync(ct);

        if (request is null || request.ExpiresAt <= DateTime.UtcNow)
        {
            _logger.LogWarning("Password reset token invalid or expired for {Email}", normalized);
            return false;
        }

        if (!BCrypt.Net.BCrypt.Verify(resetToken, request.ResetTokenHash))
        {
            _logger.LogWarning("Password reset token mismatch for {Email}", normalized);
            return false;
        }

        var newHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
        var updated = false;

        var student = await _db.Students.IgnoreQueryFilters()
            .FirstOrDefaultAsync(s => s.Email == normalized && !s.IsDeleted, ct);
        if (student is not null)
        {
            student.PasswordHash = newHash;
            updated = true;
        }

        if (!updated)
        {
            var lecturer = await _db.Lecturers.IgnoreQueryFilters()
                .FirstOrDefaultAsync(l => l.Email == normalized && !l.IsDeleted, ct);
            if (lecturer is not null)
            {
                lecturer.PasswordHash = newHash;
                updated = true;
            }
        }

        if (!updated)
        {
            var admin = await _db.Admins.IgnoreQueryFilters()
                .FirstOrDefaultAsync(a => a.Email == normalized && !a.IsDeleted, ct);
            if (admin is not null)
            {
                admin.PasswordHash = newHash;
                updated = true;
            }
        }

        if (!updated)
        {
            _logger.LogWarning("Password reset attempted for non-existent account: {Email}", normalized);
            return false;
        }

        // Consume the reset request so it cannot be replayed.
        request.IsUsed = true;
        await _db.SaveChangesAsync(ct);

        _logger.LogInformation("Password reset completed for {Email}", normalized);
        return true;
    }
}
