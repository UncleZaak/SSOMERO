using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Ssomero.Api.Configuration;
using Ssomero.Api.Data;
using Ssomero.Api.Entities;
using Ssomero.Api.Services;
using Ssomero.Api.Services.Implementations;
using System;
using System.Threading.Tasks;

namespace Ssomero.Api.Services.UnitTests;

/// <summary>
/// Controllable stand-in for EmailService. Records calls without touching SMTP.
/// </summary>
internal sealed class FakeEmailService : EmailService
{
    public int SendCount { get; private set; }

    public FakeEmailService()
        : base(
            Options.Create(new EmailSettings
            {
                SmtpServer = "localhost",
                Port = 25,
                SenderEmail = "test@test.com",
                SenderName = "Test",
                Password = "pass",
                EnableSsl = false,
                TimeoutMs = 1000
            }),
            new Mock<ILogger<EmailService>>().Object)
    { }

    public override Task SendEmailAsync(string toEmail, string subject, string body)
    {
        SendCount++;
        return Task.CompletedTask;
    }
}

[TestClass]
public class PasswordResetServiceTests
{
    private static SsomeroDbContext CreateDb()
    {
        var opts = new DbContextOptionsBuilder<SsomeroDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new SsomeroDbContext(opts);
    }

    private static PasswordResetService CreateService(SsomeroDbContext db, FakeEmailService? email = null)
    {
        email ??= new FakeEmailService();
        var logger = new Mock<ILogger<PasswordResetService>>().Object;
        return new PasswordResetService(db, email, logger);
    }

    // ── SendResetOtpAsync ──────────────────────────────────────────────────────

    [TestMethod]
    public async Task SendResetOtp_UnknownEmail_ReturnsGenericSuccessWithoutThrowing()
    {
        using var db = CreateDb();
        var email = new FakeEmailService();
        var svc = CreateService(db, email);

        await svc.SendResetOtpAsync("nobody@unknown.com");

        Assert.AreEqual(0, email.SendCount, "No email must be sent for an unknown address");
    }

    [TestMethod]
    public async Task SendResetOtp_KnownStudent_SendsEmailAndStoresHashedOtp()
    {
        using var db = CreateDb();
        db.Students.Add(new Student
        {
            Id = Guid.NewGuid(),
            Email = "student@uni.ac.ug",
            FirstName = "Alice", SecondName = "B",
            Gender = "F", Phone = "0700000000",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("OldPass1!"),
            IsVerified = true
        });
        await db.SaveChangesAsync();

        var email = new FakeEmailService();
        var svc = CreateService(db, email);
        await svc.SendResetOtpAsync("student@uni.ac.ug");

        Assert.AreEqual(1, email.SendCount, "Exactly one email must be sent for a known student");

        var request = await db.PasswordResetRequests.FirstOrDefaultAsync();
        Assert.IsNotNull(request, "A PasswordResetRequest should have been stored");
        Assert.AreEqual("student@uni.ac.ug", request.Email);
        Assert.IsFalse(string.IsNullOrEmpty(request.OtpHash), "OTP must be hashed, not empty");
        Assert.AreNotEqual("123456", request.OtpHash, "OTP must not be stored in plaintext");
    }

    [TestMethod]
    public async Task SendResetOtp_CooldownActive_DoesNotSendSecondEmail()
    {
        using var db = CreateDb();
        db.Students.Add(new Student
        {
            Id = Guid.NewGuid(),
            Email = "cooldown@uni.ac.ug",
            FirstName = "Bob", SecondName = "C",
            Gender = "M", Phone = "0700000001",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("OldPass1!"),
            IsVerified = true
        });
        db.PasswordResetRequests.Add(new PasswordResetRequest
        {
            Id = Guid.NewGuid(),
            Email = "cooldown@uni.ac.ug",
            OtpHash = BCrypt.Net.BCrypt.HashPassword("111111"),
            ExpiresAt = DateTime.UtcNow.AddMinutes(5),
            CreatedAt = DateTime.UtcNow.AddSeconds(-10)
        });
        await db.SaveChangesAsync();

        var email = new FakeEmailService();
        var svc = CreateService(db, email);
        await svc.SendResetOtpAsync("cooldown@uni.ac.ug");

        Assert.AreEqual(0, email.SendCount, "No second email should be sent during cooldown");
    }

    // ── VerifyResetOtpAsync ───────────────────────────────────────────────────

    [TestMethod]
    public async Task VerifyResetOtp_ValidOtp_ReturnsResetTokenAndClearsOtpHash()
    {
        const string otp = "654321";
        using var db = CreateDb();
        db.PasswordResetRequests.Add(new PasswordResetRequest
        {
            Id = Guid.NewGuid(),
            Email = "verify@uni.ac.ug",
            OtpHash = BCrypt.Net.BCrypt.HashPassword(otp),
            ExpiresAt = DateTime.UtcNow.AddMinutes(5),
            CreatedAt = DateTime.UtcNow
        });
        await db.SaveChangesAsync();

        var svc = CreateService(db);
        var resetToken = await svc.VerifyResetOtpAsync("verify@uni.ac.ug", otp);

        Assert.IsNotNull(resetToken, "A reset token should be returned on success");
        Assert.IsFalse(string.IsNullOrEmpty(resetToken));

        var request = await db.PasswordResetRequests.FirstAsync();
        Assert.IsNotNull(request.ResetTokenHash, "ResetTokenHash must be stored");
        Assert.AreEqual(string.Empty, request.OtpHash, "OtpHash must be cleared after use");
    }

    [TestMethod]
    public async Task VerifyResetOtp_WrongOtp_ReturnsNullAndIncrementsAttempts()
    {
        using var db = CreateDb();
        db.PasswordResetRequests.Add(new PasswordResetRequest
        {
            Id = Guid.NewGuid(),
            Email = "wrong@uni.ac.ug",
            OtpHash = BCrypt.Net.BCrypt.HashPassword("999999"),
            ExpiresAt = DateTime.UtcNow.AddMinutes(5),
            CreatedAt = DateTime.UtcNow
        });
        await db.SaveChangesAsync();

        var svc = CreateService(db);
        var result = await svc.VerifyResetOtpAsync("wrong@uni.ac.ug", "000000");

        Assert.IsNull(result);
        var req = await db.PasswordResetRequests.FirstAsync();
        Assert.AreEqual(1, req.Attempts, "Attempts must increment on failure");
    }

    [TestMethod]
    public async Task VerifyResetOtp_ExpiredOtp_ReturnsNull()
    {
        const string otp = "123456";
        using var db = CreateDb();
        db.PasswordResetRequests.Add(new PasswordResetRequest
        {
            Id = Guid.NewGuid(),
            Email = "expired@uni.ac.ug",
            OtpHash = BCrypt.Net.BCrypt.HashPassword(otp),
            ExpiresAt = DateTime.UtcNow.AddMinutes(-1),
            CreatedAt = DateTime.UtcNow.AddMinutes(-6)
        });
        await db.SaveChangesAsync();

        var svc = CreateService(db);
        var result = await svc.VerifyResetOtpAsync("expired@uni.ac.ug", otp);

        Assert.IsNull(result, "Expired OTP must be rejected");
    }

    [TestMethod]
    public async Task VerifyResetOtp_MaxAttemptsExceeded_ReturnsNull()
    {
        const string otp = "777777";
        using var db = CreateDb();
        db.PasswordResetRequests.Add(new PasswordResetRequest
        {
            Id = Guid.NewGuid(),
            Email = "maxattempts@uni.ac.ug",
            OtpHash = BCrypt.Net.BCrypt.HashPassword(otp),
            ExpiresAt = DateTime.UtcNow.AddMinutes(5),
            Attempts = 5,
            CreatedAt = DateTime.UtcNow
        });
        await db.SaveChangesAsync();

        var svc = CreateService(db);
        var result = await svc.VerifyResetOtpAsync("maxattempts@uni.ac.ug", otp);

        Assert.IsNull(result, "OTP with max attempts must be rejected");
    }

    [TestMethod]
    public async Task VerifyResetOtp_UsedRequest_ReturnsNull()
    {
        const string otp = "888888";
        using var db = CreateDb();
        db.PasswordResetRequests.Add(new PasswordResetRequest
        {
            Id = Guid.NewGuid(),
            Email = "used@uni.ac.ug",
            OtpHash = BCrypt.Net.BCrypt.HashPassword(otp),
            ExpiresAt = DateTime.UtcNow.AddMinutes(5),
            IsUsed = true,
            CreatedAt = DateTime.UtcNow
        });
        await db.SaveChangesAsync();

        var svc = CreateService(db);
        var result = await svc.VerifyResetOtpAsync("used@uni.ac.ug", otp);

        Assert.IsNull(result, "Used OTP must be rejected");
    }

    // ── ResetPasswordAsync ────────────────────────────────────────────────────

    [TestMethod]
    public async Task ResetPassword_ValidToken_UpdatesStudentPasswordAndConsumesRequest()
    {
        using var db = CreateDb();
        var studentId = Guid.NewGuid();
        db.Students.Add(new Student
        {
            Id = studentId,
            Email = "reset@uni.ac.ug",
            FirstName = "Carol", SecondName = "D",
            Gender = "F", Phone = "0700000002",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("OldPass1!"),
            IsVerified = true
        });
        var plainToken = Convert.ToBase64String(System.Security.Cryptography.RandomNumberGenerator.GetBytes(32));
        db.PasswordResetRequests.Add(new PasswordResetRequest
        {
            Id = Guid.NewGuid(),
            Email = "reset@uni.ac.ug",
            OtpHash = string.Empty,
            ResetTokenHash = BCrypt.Net.BCrypt.HashPassword(plainToken),
            ExpiresAt = DateTime.UtcNow.AddMinutes(10),
            CreatedAt = DateTime.UtcNow
        });
        await db.SaveChangesAsync();

        var svc = CreateService(db);
        var success = await svc.ResetPasswordAsync("reset@uni.ac.ug", plainToken, "NewPass1!");

        Assert.IsTrue(success);
        var student = await db.Students.IgnoreQueryFilters().FirstAsync(s => s.Id == studentId);
        Assert.IsTrue(BCrypt.Net.BCrypt.Verify("NewPass1!", student.PasswordHash),
            "Password should be updated to the new value");
        var req = await db.PasswordResetRequests.FirstAsync();
        Assert.IsTrue(req.IsUsed, "Reset request must be marked as used");
    }

    [TestMethod]
    public async Task ResetPassword_WrongToken_ReturnsFalse()
    {
        using var db = CreateDb();
        db.Students.Add(new Student
        {
            Id = Guid.NewGuid(),
            Email = "badtoken@uni.ac.ug",
            FirstName = "Eve", SecondName = "F",
            Gender = "F", Phone = "0700000003",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("OldPass1!"),
            IsVerified = true
        });
        var realToken = Convert.ToBase64String(System.Security.Cryptography.RandomNumberGenerator.GetBytes(32));
        db.PasswordResetRequests.Add(new PasswordResetRequest
        {
            Id = Guid.NewGuid(),
            Email = "badtoken@uni.ac.ug",
            OtpHash = string.Empty,
            ResetTokenHash = BCrypt.Net.BCrypt.HashPassword(realToken),
            ExpiresAt = DateTime.UtcNow.AddMinutes(10),
            CreatedAt = DateTime.UtcNow
        });
        await db.SaveChangesAsync();

        var svc = CreateService(db);
        var success = await svc.ResetPasswordAsync("badtoken@uni.ac.ug", "wrong-token", "NewPass1!");

        Assert.IsFalse(success, "Wrong reset token must be rejected");
    }

    [TestMethod]
    public async Task ResetPassword_ExpiredToken_ReturnsFalse()
    {
        using var db = CreateDb();
        var token = Convert.ToBase64String(System.Security.Cryptography.RandomNumberGenerator.GetBytes(32));
        db.PasswordResetRequests.Add(new PasswordResetRequest
        {
            Id = Guid.NewGuid(),
            Email = "expiredtoken@uni.ac.ug",
            OtpHash = string.Empty,
            ResetTokenHash = BCrypt.Net.BCrypt.HashPassword(token),
            ExpiresAt = DateTime.UtcNow.AddMinutes(-1),
            CreatedAt = DateTime.UtcNow.AddMinutes(-15)
        });
        await db.SaveChangesAsync();

        var svc = CreateService(db);
        var success = await svc.ResetPasswordAsync("expiredtoken@uni.ac.ug", token, "NewPass1!");

        Assert.IsFalse(success, "Expired reset token must be rejected");
    }

    [TestMethod]
    public async Task ResetPassword_UnknownEmail_ReturnsFalse()
    {
        using var db = CreateDb();
        var svc = CreateService(db);

        var success = await svc.ResetPasswordAsync("ghost@uni.ac.ug", "any-token", "NewPass1!");

        Assert.IsFalse(success, "Reset for non-existent account must return false");
    }

    [TestMethod]
    public async Task ResetPassword_StrongPassword_IsAccepted()
    {
        using var db = CreateDb();
        var token = Convert.ToBase64String(System.Security.Cryptography.RandomNumberGenerator.GetBytes(32));
        db.Students.Add(new Student
        {
            Id = Guid.NewGuid(),
            Email = "strength@uni.ac.ug",
            FirstName = "G", SecondName = "H",
            Gender = "M", Phone = "0700000004",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("OldPass1!"),
            IsVerified = true
        });
        db.PasswordResetRequests.Add(new PasswordResetRequest
        {
            Id = Guid.NewGuid(),
            Email = "strength@uni.ac.ug",
            OtpHash = string.Empty,
            ResetTokenHash = BCrypt.Net.BCrypt.HashPassword(token),
            ExpiresAt = DateTime.UtcNow.AddMinutes(10),
            CreatedAt = DateTime.UtcNow
        });
        await db.SaveChangesAsync();

        var svc = CreateService(db);
        var success = await svc.ResetPasswordAsync("strength@uni.ac.ug", token, "Str0ng!Pass");

        Assert.IsTrue(success, "Strong password meeting all criteria must be accepted");
    }
}
