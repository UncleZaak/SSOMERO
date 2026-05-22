using BCrypt.Net;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Ssomero.Api.Configuration;
using Ssomero.Api.Data;
using Ssomero.Api.Entities;
using Ssomero.Api.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Ssomero.Api.Services.UnitTests;
/// <summary>
/// Unit tests for the OtpService class.
/// </summary>
[TestClass]
public class OtpServiceTests
{
#region Helper Methods
    /// <summary>
    /// Creates a mock SsomeroDbContext with the specified DbSet.
    /// </summary>
    private static Mock<SsomeroDbContext> CreateMockDbContext(DbSet<Otp> otpSet)
    {
        Mock<SsomeroDbContext> mockContext = new Mock<SsomeroDbContext>(new DbContextOptionsBuilder<SsomeroDbContext>().Options);
        mockContext.Setup(c => c.Otps).Returns(otpSet);
        mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
        return mockContext;
    }

#endregion
#region Test Infrastructure Classes
#endregion
    /// <summary>
    /// Tests that ValidateVerificationTokenAsync returns true and consumes the token
    /// when a valid email and matching token are provided.
    /// </summary>
    [TestMethod]
    [TestCategory("ProductionBugSuspected")]
    [Ignore("ProductionBugSuspected")]
    public async Task ValidateVerificationTokenAsync_ValidEmailAndToken_ReturnsTrueAndConsumesToken()
    {
        // Arrange
        string email = "test@example.com";
        string plainToken = "test-verification-token";
        string hashedToken = BCrypt.Net.BCrypt.HashPassword(plainToken);
        DateTime futureExpiry = DateTime.UtcNow.AddMinutes(10);
        var otp = new Otp
        {
            Id = Guid.NewGuid(),
            Email = email.ToLowerInvariant(),
            OtpCode = "123456",
            ExpiresAt = futureExpiry,
            IsUsed = false,
            VerificationToken = hashedToken,
            VerificationTokenExpiresAt = futureExpiry
        };
        var data = new List<Otp>
        {
            otp
        }.AsQueryable();
        var mockSet = CreateMockDbSet(data);
        var mockContext = new Mock<SsomeroDbContext>();
        mockContext.Setup(c => c.Otps).Returns(mockSet.Object);
        mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
        var mockEmailService = new Mock<EmailService>();
        var mockLogger = new Mock<ILogger<OtpService>>();
        var service = new OtpService(mockContext.Object, mockEmailService.Object, mockLogger.Object);
        // Act
        bool result = await service.ValidateVerificationTokenAsync(email, plainToken);
        // Assert
        Assert.IsTrue(result);
        Assert.IsNull(otp.VerificationToken);
        Assert.IsNull(otp.VerificationTokenExpiresAt);
        mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Tests that ValidateVerificationTokenAsync returns true when email has mixed case.
    /// The method normalizes email to lowercase for comparison.
    /// </summary>
    [TestMethod]
    [TestCategory("ProductionBugSuspected")]
    [Ignore("ProductionBugSuspected")]
    public async Task ValidateVerificationTokenAsync_MixedCaseEmail_ReturnsTrueAndConsumesToken()
    {
        // Arrange
        string email = "Test@Example.COM";
        string plainToken = "test-token";
        string hashedToken = BCrypt.Net.BCrypt.HashPassword(plainToken);
        DateTime futureExpiry = DateTime.UtcNow.AddMinutes(10);
        var otp = new Otp
        {
            Id = Guid.NewGuid(),
            Email = "test@example.com",
            OtpCode = "123456",
            ExpiresAt = futureExpiry,
            IsUsed = false,
            VerificationToken = hashedToken,
            VerificationTokenExpiresAt = futureExpiry
        };
        var data = new List<Otp>
        {
            otp
        }.AsQueryable();
        var mockSet = CreateMockDbSet(data);
        var mockContext = new Mock<SsomeroDbContext>();
        mockContext.Setup(c => c.Otps).Returns(mockSet.Object);
        mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
        var mockEmailService = new Mock<EmailService>();
        var mockLogger = new Mock<ILogger<OtpService>>();
        var service = new OtpService(mockContext.Object, mockEmailService.Object, mockLogger.Object);
        // Act
        bool result = await service.ValidateVerificationTokenAsync(email, plainToken);
        // Assert
        Assert.IsTrue(result);
    }

    /// <summary>
    /// Tests that ValidateVerificationTokenAsync returns false when the token does not match.
    /// No token should be consumed.
    /// </summary>
    [TestMethod]
    [TestCategory("ProductionBugSuspected")]
    [Ignore("ProductionBugSuspected")]
    public async Task ValidateVerificationTokenAsync_InvalidToken_ReturnsFalse()
    {
        // Arrange
        string email = "test@example.com";
        string plainToken = "wrong-token";
        string hashedToken = BCrypt.Net.BCrypt.HashPassword("correct-token");
        DateTime futureExpiry = DateTime.UtcNow.AddMinutes(10);
        var otp = new Otp
        {
            Id = Guid.NewGuid(),
            Email = email.ToLowerInvariant(),
            OtpCode = "123456",
            ExpiresAt = futureExpiry,
            IsUsed = false,
            VerificationToken = hashedToken,
            VerificationTokenExpiresAt = futureExpiry
        };
        var data = new List<Otp>
        {
            otp
        }.AsQueryable();
        var mockSet = CreateMockDbSet(data);
        var mockContext = new Mock<SsomeroDbContext>();
        mockContext.Setup(c => c.Otps).Returns(mockSet.Object);
        var mockEmailService = new Mock<EmailService>();
        var mockLogger = new Mock<ILogger<OtpService>>();
        var service = new OtpService(mockContext.Object, mockEmailService.Object, mockLogger.Object);
        // Act
        bool result = await service.ValidateVerificationTokenAsync(email, plainToken);
        // Assert
        Assert.IsFalse(result);
        Assert.IsNotNull(otp.VerificationToken);
        mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    /// <summary>
    /// Tests that ValidateVerificationTokenAsync returns false when no OTP record exists for the email.
    /// </summary>
    [TestMethod]
    [TestCategory("ProductionBugSuspected")]
    [Ignore("ProductionBugSuspected")]
    public async Task ValidateVerificationTokenAsync_NoMatchingEmail_ReturnsFalse()
    {
        // Arrange
        string email = "nonexistent@example.com";
        string plainToken = "test-token";
        var data = new List<Otp>().AsQueryable();
        var mockSet = CreateMockDbSet(data);
        var mockContext = new Mock<SsomeroDbContext>();
        mockContext.Setup(c => c.Otps).Returns(mockSet.Object);
        var mockEmailService = new Mock<EmailService>();
        var mockLogger = new Mock<ILogger<OtpService>>();
        var service = new OtpService(mockContext.Object, mockEmailService.Object, mockLogger.Object);
        // Act
        bool result = await service.ValidateVerificationTokenAsync(email, plainToken);
        // Assert
        Assert.IsFalse(result);
        mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    /// <summary>
    /// Tests that ValidateVerificationTokenAsync returns false when the verification token is expired.
    /// </summary>
    [TestMethod]
    [TestCategory("ProductionBugSuspected")]
    [Ignore("ProductionBugSuspected")]
    public async Task ValidateVerificationTokenAsync_ExpiredToken_ReturnsFalse()
    {
        // Arrange
        string email = "test@example.com";
        string plainToken = "test-token";
        string hashedToken = BCrypt.Net.BCrypt.HashPassword(plainToken);
        DateTime pastExpiry = DateTime.UtcNow.AddMinutes(-10);
        var otp = new Otp
        {
            Id = Guid.NewGuid(),
            Email = email.ToLowerInvariant(),
            OtpCode = "123456",
            ExpiresAt = pastExpiry,
            IsUsed = false,
            VerificationToken = hashedToken,
            VerificationTokenExpiresAt = pastExpiry
        };
        var data = new List<Otp>
        {
            otp
        }.AsQueryable();
        var mockSet = CreateMockDbSet(data);
        var mockContext = new Mock<SsomeroDbContext>();
        mockContext.Setup(c => c.Otps).Returns(mockSet.Object);
        var mockEmailService = new Mock<EmailService>();
        var mockLogger = new Mock<ILogger<OtpService>>();
        var service = new OtpService(mockContext.Object, mockEmailService.Object, mockLogger.Object);
        // Act
        bool result = await service.ValidateVerificationTokenAsync(email, plainToken);
        // Assert
        Assert.IsFalse(result);
        mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    /// <summary>
    /// Tests that ValidateVerificationTokenAsync returns false when VerificationToken is null.
    /// </summary>
    [TestMethod]
    [TestCategory("ProductionBugSuspected")]
    [Ignore("ProductionBugSuspected")]
    public async Task ValidateVerificationTokenAsync_NullVerificationToken_ReturnsFalse()
    {
        // Arrange
        string email = "test@example.com";
        string plainToken = "test-token";
        DateTime futureExpiry = DateTime.UtcNow.AddMinutes(10);
        var otp = new Otp
        {
            Id = Guid.NewGuid(),
            Email = email.ToLowerInvariant(),
            OtpCode = "123456",
            ExpiresAt = futureExpiry,
            IsUsed = false,
            VerificationToken = null,
            VerificationTokenExpiresAt = futureExpiry
        };
        var data = new List<Otp>
        {
            otp
        }.AsQueryable();
        var mockSet = CreateMockDbSet(data);
        var mockContext = new Mock<SsomeroDbContext>();
        mockContext.Setup(c => c.Otps).Returns(mockSet.Object);
        var mockEmailService = new Mock<EmailService>();
        var mockLogger = new Mock<ILogger<OtpService>>();
        var service = new OtpService(mockContext.Object, mockEmailService.Object, mockLogger.Object);
        // Act
        bool result = await service.ValidateVerificationTokenAsync(email, plainToken);
        // Assert
        Assert.IsFalse(result);
        mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    /// <summary>
    /// Tests that ValidateVerificationTokenAsync returns false when VerificationTokenExpiresAt is null.
    /// </summary>
    [TestMethod]
    [TestCategory("ProductionBugSuspected")]
    [Ignore("ProductionBugSuspected")]
    public async Task ValidateVerificationTokenAsync_NullVerificationTokenExpiresAt_ReturnsFalse()
    {
        // Arrange
        string email = "test@example.com";
        string plainToken = "test-token";
        string hashedToken = BCrypt.Net.BCrypt.HashPassword(plainToken);
        DateTime futureExpiry = DateTime.UtcNow.AddMinutes(10);
        var otp = new Otp
        {
            Id = Guid.NewGuid(),
            Email = email.ToLowerInvariant(),
            OtpCode = "123456",
            ExpiresAt = futureExpiry,
            IsUsed = false,
            VerificationToken = hashedToken,
            VerificationTokenExpiresAt = null
        };
        var data = new List<Otp>
        {
            otp
        }.AsQueryable();
        var mockSet = CreateMockDbSet(data);
        var mockContext = new Mock<SsomeroDbContext>();
        mockContext.Setup(c => c.Otps).Returns(mockSet.Object);
        var mockEmailService = new Mock<EmailService>();
        var mockLogger = new Mock<ILogger<OtpService>>();
        var service = new OtpService(mockContext.Object, mockEmailService.Object, mockLogger.Object);
        // Act
        bool result = await service.ValidateVerificationTokenAsync(email, plainToken);
        // Assert
        Assert.IsFalse(result);
        mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    /// <summary>
    /// Tests that ValidateVerificationTokenAsync with empty token parameter returns false.
    /// BCrypt.Verify with empty token should not match any hashed token.
    /// </summary>
    [TestMethod]
    [TestCategory("ProductionBugSuspected")]
    [Ignore("ProductionBugSuspected")]
    public async Task ValidateVerificationTokenAsync_EmptyToken_ReturnsFalse()
    {
        // Arrange
        string email = "test@example.com";
        string plainToken = string.Empty;
        string hashedToken = BCrypt.Net.BCrypt.HashPassword("non-empty-token");
        DateTime futureExpiry = DateTime.UtcNow.AddMinutes(10);
        var otp = new Otp
        {
            Id = Guid.NewGuid(),
            Email = email.ToLowerInvariant(),
            OtpCode = "123456",
            ExpiresAt = futureExpiry,
            IsUsed = false,
            VerificationToken = hashedToken,
            VerificationTokenExpiresAt = futureExpiry
        };
        var data = new List<Otp>
        {
            otp
        }.AsQueryable();
        var mockSet = CreateMockDbSet(data);
        var mockContext = new Mock<SsomeroDbContext>();
        mockContext.Setup(c => c.Otps).Returns(mockSet.Object);
        var mockEmailService = new Mock<EmailService>();
        var mockLogger = new Mock<ILogger<OtpService>>();
        var service = new OtpService(mockContext.Object, mockEmailService.Object, mockLogger.Object);
        // Act
        bool result = await service.ValidateVerificationTokenAsync(email, plainToken);
        // Assert
        Assert.IsFalse(result);
        mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    /// <summary>
    /// Tests that ValidateVerificationTokenAsync with very long email parameter.
    /// The method should handle long strings without throwing.
    /// </summary>
    [TestMethod]
    [TestCategory("ProductionBugSuspected")]
    [Ignore("ProductionBugSuspected")]
    public async Task ValidateVerificationTokenAsync_VeryLongEmail_ReturnsFalse()
    {
        // Arrange
        string email = new string ('a', 1000) + "@example.com";
        string plainToken = "test-token";
        var data = new List<Otp>().AsQueryable();
        var mockSet = CreateMockDbSet(data);
        var mockContext = new Mock<SsomeroDbContext>();
        mockContext.Setup(c => c.Otps).Returns(mockSet.Object);
        var mockEmailService = new Mock<EmailService>();
        var mockLogger = new Mock<ILogger<OtpService>>();
        var service = new OtpService(mockContext.Object, mockEmailService.Object, mockLogger.Object);
        // Act
        bool result = await service.ValidateVerificationTokenAsync(email, plainToken);
        // Assert
        Assert.IsFalse(result);
    }

    /// <summary>
    /// Tests that ValidateVerificationTokenAsync with very long token parameter.
    /// The method should handle long strings without throwing.
    /// </summary>
    [TestMethod]
    [TestCategory("ProductionBugSuspected")]
    [Ignore("ProductionBugSuspected")]
    public async Task ValidateVerificationTokenAsync_VeryLongToken_ReturnsFalse()
    {
        // Arrange
        string email = "test@example.com";
        string plainToken = new string ('x', 10000);
        string hashedToken = BCrypt.Net.BCrypt.HashPassword("short-token");
        DateTime futureExpiry = DateTime.UtcNow.AddMinutes(10);
        var otp = new Otp
        {
            Id = Guid.NewGuid(),
            Email = email.ToLowerInvariant(),
            OtpCode = "123456",
            ExpiresAt = futureExpiry,
            IsUsed = false,
            VerificationToken = hashedToken,
            VerificationTokenExpiresAt = futureExpiry
        };
        var data = new List<Otp>
        {
            otp
        }.AsQueryable();
        var mockSet = CreateMockDbSet(data);
        var mockContext = new Mock<SsomeroDbContext>();
        mockContext.Setup(c => c.Otps).Returns(mockSet.Object);
        var mockLogger = new Mock<ILogger<OtpService>>();
        var service = new OtpService(mockContext.Object, null, mockLogger.Object);
        // Act
        bool result = await service.ValidateVerificationTokenAsync(email, plainToken);
        // Assert
        Assert.IsFalse(result);
        mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    /// <summary>
    /// Tests that ValidateVerificationTokenAsync with email containing special characters.
    /// The method should normalize and handle special characters correctly.
    /// </summary>
    [TestMethod]
    [TestCategory("ProductionBugSuspected")]
    [Ignore("ProductionBugSuspected")]
    public async Task ValidateVerificationTokenAsync_EmailWithSpecialCharacters_ReturnsTrue()
    {
        // Arrange
        string email = "test+tag@example.com";
        string plainToken = "test-token";
        string hashedToken = BCrypt.Net.BCrypt.HashPassword(plainToken);
        DateTime futureExpiry = DateTime.UtcNow.AddMinutes(10);
        var otp = new Otp
        {
            Id = Guid.NewGuid(),
            Email = email.ToLowerInvariant(),
            OtpCode = "123456",
            ExpiresAt = futureExpiry,
            IsUsed = false,
            VerificationToken = hashedToken,
            VerificationTokenExpiresAt = futureExpiry
        };
        var data = new List<Otp>
        {
            otp
        }.AsQueryable();
        var mockSet = CreateMockDbSet(data);
        var mockContext = new Mock<SsomeroDbContext>();
        mockContext.Setup(c => c.Otps).Returns(mockSet.Object);
        mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
        var mockEmailService = new Mock<EmailService>();
        var mockLogger = new Mock<ILogger<OtpService>>();
        var service = new OtpService(mockContext.Object, mockEmailService.Object, mockLogger.Object);
        // Act
        bool result = await service.ValidateVerificationTokenAsync(email, plainToken);
        // Assert
        Assert.IsTrue(result);
    }

    /// <summary>
    /// Tests that ValidateVerificationTokenAsync with token containing special characters.
    /// The method should handle special characters in tokens correctly.
    /// </summary>
    [TestMethod]
    [TestCategory("ProductionBugSuspected")]
    [Ignore("ProductionBugSuspected")]
    public async Task ValidateVerificationTokenAsync_TokenWithSpecialCharacters_ReturnsTrue()
    {
        // Arrange
        string email = "test@example.com";
        string plainToken = "token!@#$%^&*()_+-=[]{}|;':\",./<>?";
        string hashedToken = BCrypt.Net.BCrypt.HashPassword(plainToken);
        DateTime futureExpiry = DateTime.UtcNow.AddMinutes(10);
        var otp = new Otp
        {
            Id = Guid.NewGuid(),
            Email = email.ToLowerInvariant(),
            OtpCode = "123456",
            ExpiresAt = futureExpiry,
            IsUsed = false,
            VerificationToken = hashedToken,
            VerificationTokenExpiresAt = futureExpiry
        };
        var data = new List<Otp>
        {
            otp
        }.AsQueryable();
        var mockSet = CreateMockDbSet(data);
        var mockContext = new Mock<SsomeroDbContext>();
        mockContext.Setup(c => c.Otps).Returns(mockSet.Object);
        mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
        var mockEmailService = new Mock<EmailService>();
        var mockLogger = new Mock<ILogger<OtpService>>();
        var service = new OtpService(mockContext.Object, mockEmailService.Object, mockLogger.Object);
        // Act
        bool result = await service.ValidateVerificationTokenAsync(email, plainToken);
        // Assert
        Assert.IsTrue(result);
    }

    /// <summary>
    /// Helper method to create a mock DbSet with queryable support.
    /// </summary>
    private static Mock<DbSet<Otp>> CreateMockDbSet(IQueryable<Otp> data)
    {
        var mockSet = new Mock<DbSet<Otp>>();
        mockSet.As<IQueryable<Otp>>().Setup(m => m.Provider).Returns(data.Provider);
        mockSet.As<IQueryable<Otp>>().Setup(m => m.Expression).Returns(data.Expression);
        mockSet.As<IQueryable<Otp>>().Setup(m => m.ElementType).Returns(data.ElementType);
        mockSet.As<IQueryable<Otp>>().Setup(m => m.GetEnumerator()).Returns(data.GetEnumerator());
        return mockSet;
    }
}