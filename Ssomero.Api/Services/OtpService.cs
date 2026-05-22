using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ssomero.Api.Data;
using Ssomero.Api.Entities;

namespace Ssomero.Api.Services;

public class OtpService
{
    private readonly SsomeroDbContext _db;
    private readonly EmailService _email;
    private readonly ILogger<OtpService> _logger;

    public OtpService(SsomeroDbContext db, EmailService email, ILogger<OtpService> logger)
    {
        _db = db;
        _email = email;
        _logger = logger;
    }

    public async Task<string> GenerateOtpAsync(string email)
    {
        var normalizedEmail = email.ToLowerInvariant();

        // Invalidate all previous OTPs for this email
        var oldOtps = await _db.Otps
            .Where(o => o.Email == normalizedEmail && !o.IsUsed)
            .ToListAsync();
        foreach (var old in oldOtps)
            old.IsUsed = true;

        // Persist invalidation of old OTPs first
        if (oldOtps.Count > 0)
            await _db.SaveChangesAsync();

        // Cryptographically secure 6-digit code
        var code = RandomNumberGenerator.GetInt32(100000, 1000000).ToString();

        // Send OTP via email BEFORE saving the record.
        // If delivery fails, no orphaned OTP is left in the database.
        await _email.SendOtpEmailAsync(normalizedEmail, code);

        var otp = new Otp
        {
            Id = Guid.NewGuid(),
            Email = normalizedEmail,
            OtpCode = BCrypt.Net.BCrypt.HashPassword(code),
            ExpiresAt = DateTime.UtcNow.AddMinutes(10),
            IsUsed = false
        };
        _db.Otps.Add(otp);
        await _db.SaveChangesAsync();

        _logger.LogInformation("OTP generated and sent for {Email}", normalizedEmail);
        return code;
    }

    /// <summary>
    /// Verifies the OTP. On success, generates and stores a short-lived verification token
    /// that must be presented during registration.
    /// </summary>
    public async Task<string?> VerifyOtpAsync(string email, string code)
    {
        var normalizedEmail = email.ToLowerInvariant();

        // Get all unexpired, unused OTPs for this email (should be at most 1 after cleanup)
        var candidates = await _db.Otps
            .Where(o => o.Email == normalizedEmail
                     && !o.IsUsed
                     && o.ExpiresAt > DateTime.UtcNow)
            .OrderByDescending(o => o.ExpiresAt)
            .Take(5)
            .ToListAsync();

        foreach (var otp in candidates)
        {
            if (BCrypt.Net.BCrypt.Verify(code, otp.OtpCode))
            {
                // Generate a cryptographic verification token
                var tokenBytes = RandomNumberGenerator.GetBytes(32);
                var verificationToken = Convert.ToBase64String(tokenBytes);

                otp.IsUsed = true;
                otp.VerifiedAt = DateTime.UtcNow;
                otp.VerificationToken = BCrypt.Net.BCrypt.HashPassword(verificationToken);
                otp.VerificationTokenExpiresAt = DateTime.UtcNow.AddMinutes(15);
                await _db.SaveChangesAsync();
                return verificationToken;
            }
        }

        return null;
    }

    /// <summary>
    /// Validates that the verification token is valid for the given email.
    /// Consumed on use (single-use token).
    /// </summary>
    public async Task<bool> ValidateVerificationTokenAsync(string email, string token)
    {
        var normalizedEmail = email.ToLowerInvariant();

        // Token is hashed, so we must load candidates and verify with BCrypt
        var candidates = await _db.Otps
            .Where(o => o.Email == normalizedEmail
                     && o.VerificationToken != null
                     && o.VerificationTokenExpiresAt != null
                     && o.VerificationTokenExpiresAt > DateTime.UtcNow)
            .ToListAsync();

        foreach (var otp in candidates)
        {
            if (BCrypt.Net.BCrypt.Verify(token, otp.VerificationToken))
            {
                // Consume the token so it cannot be reused
                otp.VerificationToken = null;
                otp.VerificationTokenExpiresAt = null;
                await _db.SaveChangesAsync();
                return true;
            }
        }

        return false;
    }
}
