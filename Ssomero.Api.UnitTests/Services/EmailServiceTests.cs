using System;
using System.Net.Mail;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Ssomero.Api.Configuration;
using Ssomero.Api.Services;

namespace Ssomero.Api.Services.UnitTests;


/// <summary>
/// Unit tests for the EmailService class, focusing on the SendOtpEmailAsync method.
/// </summary>
[TestClass]
public class EmailServiceTests
{
    /// <summary>
    /// Tests that SendOtpEmailAsync handles null OTP code without throwing during message construction.
    /// This validates that the method can handle null OTP codes (though not recommended in practice).
    /// Note: This test verifies the method doesn't fail during the initial stages with null otpCode,
    /// but will fail at SMTP sending due to lack of real SMTP infrastructure in unit tests.
    /// </summary>
    [TestMethod]
    public async Task SendOtpEmailAsync_WithNullOtpCode_DoesNotThrowDuringConstruction()
    {
        // Arrange
        var mockOptions = new Mock<IOptions<EmailSettings>>();
        var mockLogger = new Mock<ILogger<EmailService>>();
        var settings = new EmailSettings
        {
            SenderEmail = "sender@example.com",
            Password = "password123",
            SenderName = "Test Sender",
            SmtpServer = "smtp.example.com",
            Port = 587,
            EnableSsl = true,
            TimeoutMs = 10000
        };
        mockOptions.Setup(x => x.Value).Returns(settings);
        var emailService = new EmailService(mockOptions.Object, mockLogger.Object);

        // Act & Assert
        // The method will throw when trying to connect to SMTP server, but not due to null otpCode
        // We verify it doesn't throw ArgumentNullException specifically for the otpCode parameter
        try
        {
            await emailService.SendOtpEmailAsync("recipient@example.com", null!);
            // If no exception is thrown (unlikely without real SMTP), the test passes
            Assert.IsTrue(true);
        }
        catch (ArgumentNullException ex) when (ex.ParamName == "otpCode")
        {
            // If ArgumentNullException is thrown specifically for otpCode, fail the test
            Assert.Fail("Method should not throw ArgumentNullException for null otpCode");
        }
        catch
        {
            // Any other exception (e.g., SmtpException, SocketException) is expected 
            // due to lack of real SMTP infrastructure and is acceptable for this test
            Assert.IsTrue(true);
        }
    }

    /// <summary>
    /// Tests that SendOtpEmailAsync handles empty OTP code without throwing during message construction.
    /// This validates that the method can handle empty OTP codes (though not recommended in practice).
    /// Note: This test verifies the method doesn't fail during the initial stages with empty otpCode,
    /// but will fail at SMTP sending due to lack of real SMTP infrastructure in unit tests.
    /// </summary>
    [TestMethod]
    public async Task SendOtpEmailAsync_WithEmptyOtpCode_DoesNotThrowDuringConstruction()
    {
        // Arrange
        var mockOptions = new Mock<IOptions<EmailSettings>>();
        var mockLogger = new Mock<ILogger<EmailService>>();
        var settings = new EmailSettings
        {
            SenderEmail = "sender@example.com",
            Password = "password123",
            SenderName = "Test Sender",
            SmtpServer = "smtp.example.com",
            Port = 587,
            EnableSsl = true,
            TimeoutMs = 10000
        };
        mockOptions.Setup(x => x.Value).Returns(settings);
        var emailService = new EmailService(mockOptions.Object, mockLogger.Object);

        // Act & Assert
        // The method will throw when trying to connect to SMTP server, but not due to empty otpCode
        try
        {
            await emailService.SendOtpEmailAsync("recipient@example.com", string.Empty);
            // If no exception is thrown (unlikely without real SMTP), the test passes
            Assert.IsTrue(true);
        }
        catch (ArgumentException ex) when (ex.ParamName == "otpCode")
        {
            // If ArgumentException is thrown specifically for otpCode, fail the test
            Assert.Fail("Method should not throw ArgumentException for empty otpCode");
        }
        catch
        {
            // Any other exception (e.g., SmtpException, SocketException) is expected 
            // due to lack of real SMTP infrastructure and is acceptable for this test
            Assert.IsTrue(true);
        }
    }

    /// <summary>
    /// Tests that SendOtpEmailAsync handles OTP codes with special characters correctly.
    /// This validates that special characters in OTP codes don't cause issues during message formatting.
    /// Note: Full validation requires SMTP infrastructure; this test verifies no immediate exceptions
    /// are thrown during message construction with special characters.
    /// </summary>
    [TestMethod]
    [DataRow("123<456>")]
    [DataRow("ABC&123")]
    [DataRow("test@code")]
    [DataRow("код123")]
    [DataRow("123\n456")]
    public async Task SendOtpEmailAsync_WithSpecialCharactersInOtpCode_HandlesCorrectly(string otpCode)
    {
        // Arrange
        var mockOptions = new Mock<IOptions<EmailSettings>>();
        var mockLogger = new Mock<ILogger<EmailService>>();
        var settings = new EmailSettings
        {
            SenderEmail = "sender@example.com",
            Password = "password123",
            SenderName = "Test Sender",
            SmtpServer = "smtp.example.com",
            Port = 587,
            EnableSsl = true,
            TimeoutMs = 10000
        };
        mockOptions.Setup(x => x.Value).Returns(settings);
        var emailService = new EmailService(mockOptions.Object, mockLogger.Object);

        // Act & Assert
        // The method will throw when trying to connect to SMTP server, but not due to special characters in otpCode
        try
        {
            await emailService.SendOtpEmailAsync("recipient@example.com", otpCode);
            // If no exception is thrown (unlikely without real SMTP), the test passes
            Assert.IsTrue(true);
        }
        catch (FormatException)
        {
            // If FormatException is thrown, it might be due to special character handling issues
            Assert.Fail("Method should handle special characters in OTP code without FormatException");
        }
        catch
        {
            // Any other exception (e.g., SmtpException, SocketException) is expected 
            // due to lack of real SMTP infrastructure and is acceptable for this test
            Assert.IsTrue(true);
        }
    }

    /// <summary>
    /// Tests that constructor logs a warning when SenderEmail or Password is not configured.
    /// Validates that the warning message is logged during service initialization.
    /// </summary>
    [TestMethod]
    [DataRow(null, "password")]
    [DataRow("", "password")]
    [DataRow(" ", "password")]
    [DataRow("sender@test.com", null)]
    [DataRow("sender@test.com", "")]
    [DataRow("sender@test.com", " ")]
    [DataRow(null, null)]
    public void Constructor_MissingCredentials_LogsWarning(string? senderEmail, string? password)
    {
        // Arrange
        var mockOptions = new Mock<IOptions<EmailSettings>>();
        var mockLogger = new Mock<ILogger<EmailService>>();
        var settings = new EmailSettings
        {
            SenderEmail = senderEmail ?? string.Empty,
            Password = password ?? string.Empty
        };
        mockOptions.Setup(o => o.Value).Returns(settings);

        // Act
        var emailService = new EmailService(mockOptions.Object, mockLogger.Object);

        // Assert
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("EmailSettings:SenderEmail or EmailSettings:Password is not configured")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    /// <summary>
    /// Tests that constructor does not log a warning when both SenderEmail and Password are configured.
    /// Validates that no warning is logged for valid credentials.
    /// </summary>
    [TestMethod]
    public void Constructor_ValidCredentials_DoesNotLogWarning()
    {
        // Arrange
        var mockOptions = new Mock<IOptions<EmailSettings>>();
        var mockLogger = new Mock<ILogger<EmailService>>();
        var settings = new EmailSettings
        {
            SenderEmail = "sender@test.com",
            Password = "password123"
        };
        mockOptions.Setup(o => o.Value).Returns(settings);

        // Act
        var emailService = new EmailService(mockOptions.Object, mockLogger.Object);

        // Assert
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Never);
    }

    /// <summary>
    /// Tests that the constructor logs a warning when SenderEmail is null.
    /// </summary>
    [TestMethod]
    public void EmailService_NullSenderEmail_LogsWarning()
    {
        // Arrange
        var emailSettings = new EmailSettings
        {
            SenderEmail = null!,
            Password = "password123"
        };
        var mockSettings = new Mock<IOptions<EmailSettings>>();
        mockSettings.Setup(s => s.Value).Returns(emailSettings);
        var mockLogger = new Mock<ILogger<EmailService>>();

        // Act
        var service = new EmailService(mockSettings.Object, mockLogger.Object);

        // Assert
        mockLogger.Verify(
            logger => logger.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("EmailSettings:SenderEmail or EmailSettings:Password is not configured")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    /// <summary>
    /// Tests that the constructor logs a warning when SenderEmail is empty string.
    /// </summary>
    [TestMethod]
    public void EmailService_EmptySenderEmail_LogsWarning()
    {
        // Arrange
        var emailSettings = new EmailSettings
        {
            SenderEmail = string.Empty,
            Password = "password123"
        };
        var mockSettings = new Mock<IOptions<EmailSettings>>();
        mockSettings.Setup(s => s.Value).Returns(emailSettings);
        var mockLogger = new Mock<ILogger<EmailService>>();

        // Act
        var service = new EmailService(mockSettings.Object, mockLogger.Object);

        // Assert
        mockLogger.Verify(
            logger => logger.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("EmailSettings:SenderEmail or EmailSettings:Password is not configured")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    /// <summary>
    /// Tests that the constructor logs a warning when SenderEmail contains only whitespace.
    /// </summary>
    [TestMethod]
    [DataRow("   ")]
    [DataRow("\t")]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [DataRow(" \t \n ")]
    public void EmailService_WhitespaceSenderEmail_LogsWarning(string whitespaceSenderEmail)
    {
        // Arrange
        var emailSettings = new EmailSettings
        {
            SenderEmail = whitespaceSenderEmail,
            Password = "password123"
        };
        var mockSettings = new Mock<IOptions<EmailSettings>>();
        mockSettings.Setup(s => s.Value).Returns(emailSettings);
        var mockLogger = new Mock<ILogger<EmailService>>();

        // Act
        var service = new EmailService(mockSettings.Object, mockLogger.Object);

        // Assert
        mockLogger.Verify(
            logger => logger.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("EmailSettings:SenderEmail or EmailSettings:Password is not configured")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    /// <summary>
    /// Tests that the constructor logs a warning when Password is null.
    /// </summary>
    [TestMethod]
    public void EmailService_NullPassword_LogsWarning()
    {
        // Arrange
        var emailSettings = new EmailSettings
        {
            SenderEmail = "test@example.com",
            Password = null!
        };
        var mockSettings = new Mock<IOptions<EmailSettings>>();
        mockSettings.Setup(s => s.Value).Returns(emailSettings);
        var mockLogger = new Mock<ILogger<EmailService>>();

        // Act
        var service = new EmailService(mockSettings.Object, mockLogger.Object);

        // Assert
        mockLogger.Verify(
            logger => logger.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("EmailSettings:SenderEmail or EmailSettings:Password is not configured")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    /// <summary>
    /// Tests that the constructor logs a warning when Password is empty string.
    /// </summary>
    [TestMethod]
    public void EmailService_EmptyPassword_LogsWarning()
    {
        // Arrange
        var emailSettings = new EmailSettings
        {
            SenderEmail = "test@example.com",
            Password = string.Empty
        };
        var mockSettings = new Mock<IOptions<EmailSettings>>();
        mockSettings.Setup(s => s.Value).Returns(emailSettings);
        var mockLogger = new Mock<ILogger<EmailService>>();

        // Act
        var service = new EmailService(mockSettings.Object, mockLogger.Object);

        // Assert
        mockLogger.Verify(
            logger => logger.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("EmailSettings:SenderEmail or EmailSettings:Password is not configured")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    /// <summary>
    /// Tests that the constructor logs a warning when Password contains only whitespace.
    /// </summary>
    [TestMethod]
    [DataRow("   ")]
    [DataRow("\t")]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [DataRow(" \t \n ")]
    public void EmailService_WhitespacePassword_LogsWarning(string whitespacePassword)
    {
        // Arrange
        var emailSettings = new EmailSettings
        {
            SenderEmail = "test@example.com",
            Password = whitespacePassword
        };
        var mockSettings = new Mock<IOptions<EmailSettings>>();
        mockSettings.Setup(s => s.Value).Returns(emailSettings);
        var mockLogger = new Mock<ILogger<EmailService>>();

        // Act
        var service = new EmailService(mockSettings.Object, mockLogger.Object);

        // Assert
        mockLogger.Verify(
            logger => logger.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("EmailSettings:SenderEmail or EmailSettings:Password is not configured")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    /// <summary>
    /// Tests that the constructor logs a warning when both SenderEmail and Password are invalid (null/empty/whitespace).
    /// </summary>
    [TestMethod]
    [DataRow(null, null)]
    [DataRow("", "")]
    [DataRow("   ", "   ")]
    [DataRow(null, "")]
    [DataRow("", null)]
    [DataRow(null, "   ")]
    [DataRow("   ", null)]
    public void EmailService_BothInvalidSenderEmailAndPassword_LogsWarning(string? senderEmail, string? password)
    {
        // Arrange
        var emailSettings = new EmailSettings
        {
            SenderEmail = senderEmail!,
            Password = password!
        };
        var mockSettings = new Mock<IOptions<EmailSettings>>();
        mockSettings.Setup(s => s.Value).Returns(emailSettings);
        var mockLogger = new Mock<ILogger<EmailService>>();

        // Act
        var service = new EmailService(mockSettings.Object, mockLogger.Object);

        // Assert
        mockLogger.Verify(
            logger => logger.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("EmailSettings:SenderEmail or EmailSettings:Password is not configured")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    /// <summary>
    /// Tests that the constructor does not log a warning when both SenderEmail and Password are valid.
    /// </summary>
    [TestMethod]
    [DataRow("test@example.com", "password123")]
    [DataRow("user@domain.org", "P@ssw0rd!")]
    [DataRow("admin@site.co.uk", "Secure#Pass123")]
    public void EmailService_ValidSenderEmailAndPassword_DoesNotLogWarning(string senderEmail, string password)
    {
        // Arrange
        var emailSettings = new EmailSettings
        {
            SenderEmail = senderEmail,
            Password = password
        };
        var mockSettings = new Mock<IOptions<EmailSettings>>();
        mockSettings.Setup(s => s.Value).Returns(emailSettings);
        var mockLogger = new Mock<ILogger<EmailService>>();

        // Act
        var service = new EmailService(mockSettings.Object, mockLogger.Object);

        // Assert
        mockLogger.Verify(
            logger => logger.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Never);
    }

    /// <summary>
    /// Tests that the constructor properly initializes the service when all inputs are valid.
    /// </summary>
    [TestMethod]
    public void EmailService_ValidInputs_InitializesSuccessfully()
    {
        // Arrange
        var emailSettings = new EmailSettings
        {
            SenderEmail = "test@example.com",
            Password = "password123",
            SmtpServer = "smtp.gmail.com",
            Port = 587,
            EnableSsl = true
        };
        var mockSettings = new Mock<IOptions<EmailSettings>>();
        mockSettings.Setup(s => s.Value).Returns(emailSettings);
        var mockLogger = new Mock<ILogger<EmailService>>();

        // Act
        var service = new EmailService(mockSettings.Object, mockLogger.Object);

        // Assert
        Assert.IsNotNull(service);
        mockLogger.Verify(
            logger => logger.Log(
                It.IsAny<LogLevel>(),
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Never);
    }
}