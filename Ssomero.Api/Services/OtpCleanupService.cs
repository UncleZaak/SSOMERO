using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Ssomero.Api.Data;

namespace Ssomero.Api.Services;

public class OtpCleanupService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<OtpCleanupService> _logger;
    private readonly TimeSpan _interval = TimeSpan.FromMinutes(15);

    public OtpCleanupService(IServiceScopeFactory scopeFactory, ILogger<OtpCleanupService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("OTP cleanup service started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await CleanupAsync(stoppingToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogError(ex, "Error during OTP cleanup");
            }

            await Task.Delay(_interval, stoppingToken);
        }
    }

    private async Task CleanupAsync(CancellationToken ct)
    {
        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SsomeroDbContext>();

        var now = DateTime.UtcNow;

        // Remove stale OTPs but preserve used records that still have an
        // active (non-expired) verification token — those are needed for
        // the registration step that follows OTP verification.
        var staleOtps = await db.Otps
            .Where(o =>
                (o.IsUsed && (o.VerificationToken == null || o.VerificationTokenExpiresAt == null || o.VerificationTokenExpiresAt < now))
                || o.ExpiresAt < now)
            .ToListAsync(ct);

        if (staleOtps.Count > 0)
        {
            db.Otps.RemoveRange(staleOtps);
            await db.SaveChangesAsync(ct);
            _logger.LogInformation("OTP cleanup removed {Count} stale record(s)", staleOtps.Count);
        }

        // Remove password-reset requests that are expired and at least 24 h old,
        // keeping a short audit trail without growing the table indefinitely.
        var staleResets = await db.PasswordResetRequests
            .Where(r => r.IsUsed || r.ExpiresAt < now.AddHours(-24))
            .ToListAsync(ct);

        if (staleResets.Count > 0)
        {
            db.PasswordResetRequests.RemoveRange(staleResets);
            await db.SaveChangesAsync(ct);
            _logger.LogInformation("Password-reset cleanup removed {Count} stale record(s)", staleResets.Count);
        }
    }
}
