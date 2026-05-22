using Hangfire;
using Microsoft.EntityFrameworkCore;
using Ssomero.Api.Data;
using Ssomero.Api.Services;

namespace Ssomero.Api.BackgroundJobs;

public class EmailJobs
{
    private readonly SsomeroDbContext _db;
    private readonly EmailService _emailService;
    private readonly ILogger<EmailJobs> _logger;

    public EmailJobs(SsomeroDbContext db, EmailService emailService, ILogger<EmailJobs> logger)
    {
        _db = db;
        _emailService = emailService;
        _logger = logger;
    }

    /// <summary>Recurring: cleans up expired OTPs. Schedule via Hangfire recurring job manager.</summary>
    [AutomaticRetry(Attempts = 2)]
    public async Task CleanupExpiredOtpsAsync()
    {
        var now = DateTime.UtcNow;

        var stale = await _db.Otps
            .Where(o =>
                (o.IsUsed && (o.VerificationToken == null || o.VerificationTokenExpiresAt == null || o.VerificationTokenExpiresAt < now))
                || o.ExpiresAt < now)
            .ToListAsync();

        if (stale.Count > 0)
        {
            _db.Otps.RemoveRange(stale);
            await _db.SaveChangesAsync();
            _logger.LogInformation("[EmailJobs] Cleaned up {Count} expired OTP(s)", stale.Count);
        }
    }

    /// <summary>Send a transactional email via Hangfire enqueue.</summary>
    [AutomaticRetry(Attempts = 3)]
    public async Task SendEmailAsync(string to, string subject, string body)
    {
        try
        {
            await _emailService.SendEmailAsync(to, subject, body);
            _logger.LogInformation("[EmailJobs] Email sent to {To}", to);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[EmailJobs] Failed to send email to {To}", to);
            throw;
        }
    }

    /// <summary>Recurring: send welcome email to newly verified but not-yet-enrolled students.</summary>
    [AutomaticRetry(Attempts = 2)]
    public async Task SendWelcomeEmailsAsync()
    {
        var newStudents = await _db.Students
            .Where(s => s.IsVerified && s.CreatedAt >= DateTime.UtcNow.AddHours(-24))
            .Select(s => new { s.Email, s.FirstName })
            .ToListAsync();

        foreach (var student in newStudents)
        {
            await _emailService.SendEmailAsync(
                student.Email,
                "Welcome to Ssomero!",
                $"Hi {student.FirstName}, welcome aboard. Your account is active and ready to use.");
        }

        _logger.LogInformation("[EmailJobs] Sent welcome emails to {Count} student(s)", newStudents.Count);
    }
}
