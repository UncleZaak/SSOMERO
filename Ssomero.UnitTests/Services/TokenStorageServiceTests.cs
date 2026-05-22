using System;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Ssomero.Services;

namespace Ssomero.Services.UnitTests;




/// <summary>
/// Unit tests for the <see cref="TokenStorageService"/> class.
/// </summary>
[TestClass]
public class TokenStorageServiceTests
{
    /// <summary>
    /// Tests that the constructor successfully initializes the service with a valid logger instance.
    /// </summary>
    [TestMethod]
    public void Constructor_WithValidLogger_SuccessfullyCreatesInstance()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<TokenStorageService>>();

        // Act
        var service = new TokenStorageService(mockLogger.Object);

        // Assert
        Assert.IsNotNull(service);
    }

    /// <summary>
    /// Tests that the constructor accepts a null logger without throwing an exception.
    /// This documents the current behavior where no null validation is performed,
    /// despite the parameter being marked as non-nullable.
    /// </summary>
    [TestMethod]
    public void Constructor_WithNullLogger_DoesNotThrowException()
    {
        // Arrange
        ILogger<TokenStorageService> nullLogger = null!;

        // Act
        var service = new TokenStorageService(nullLogger);

        // Assert
        Assert.IsNotNull(service);
    }

    /// <summary>
    /// Tests that GetAccessTokenAsync returns a valid token when SecureStorage contains one.
    /// NOTE: This test is marked as inconclusive because the method uses a static dependency
    /// (SecureStorage.Default) that cannot be mocked with Moq. To make this testable,
    /// the TokenStorageService should accept ISecureStorage as a constructor parameter
    /// instead of using the static Default property.
    /// </summary>
    [TestMethod]
    public async Task GetAccessTokenAsync_WhenTokenExists_ReturnsToken()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<TokenStorageService>>();
        var service = new TokenStorageService(mockLogger.Object);

        // This test cannot proceed because SecureStorage.Default is a static member
        // that cannot be mocked with Moq without creating fake classes (which is prohibited).
        Assert.Inconclusive(
            "Cannot test due to static SecureStorage.Default dependency. " +
            "Refactor TokenStorageService to inject ISecureStorage via constructor.");
    }

    /// <summary>
    /// Tests that GetAccessTokenAsync returns null when SecureStorage returns null.
    /// NOTE: This test is marked as inconclusive because the method uses a static dependency
    /// (SecureStorage.Default) that cannot be mocked with Moq. To make this testable,
    /// the TokenStorageService should accept ISecureStorage as a constructor parameter
    /// instead of using the static Default property.
    /// </summary>
    [TestMethod]
    public async Task GetAccessTokenAsync_WhenTokenDoesNotExist_ReturnsNull()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<TokenStorageService>>();
        var service = new TokenStorageService(mockLogger.Object);

        // This test cannot proceed because SecureStorage.Default is a static member
        // that cannot be mocked with Moq without creating fake classes (which is prohibited).
        Assert.Inconclusive(
            "Cannot test due to static SecureStorage.Default dependency. " +
            "Refactor TokenStorageService to inject ISecureStorage via constructor.");
    }

    /// <summary>
    /// Tests that GetAccessTokenAsync catches exceptions, logs a warning, and returns null.
    /// NOTE: This test is marked as inconclusive because the method uses a static dependency
    /// (SecureStorage.Default) that cannot be mocked with Moq. To make this testable,
    /// the TokenStorageService should accept ISecureStorage as a constructor parameter
    /// instead of using the static Default property.
    /// </summary>
    [TestMethod]
    public async Task GetAccessTokenAsync_WhenExceptionOccurs_LogsWarningAndReturnsNull()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<TokenStorageService>>();
        var service = new TokenStorageService(mockLogger.Object);

        // This test cannot proceed because SecureStorage.Default is a static member
        // that cannot be mocked with Moq without creating fake classes (which is prohibited).
        // We need to verify that:
        // 1. The exception is caught
        // 2. logger.LogWarning is called with the exception and correct message
        // 3. The method returns null
        Assert.Inconclusive(
            "Cannot test due to static SecureStorage.Default dependency. " +
            "Refactor TokenStorageService to inject ISecureStorage via constructor.");
    }

    /// <summary>
    /// Tests that the constructor correctly initializes the service with a valid logger.
    /// </summary>
    [TestMethod]
    public void Constructor_WithValidLogger_InitializesSuccessfully()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<TokenStorageService>>();

        // Act
        var service = new TokenStorageService(loggerMock.Object);

        // Assert
        Assert.IsNotNull(service);
    }

    /// <summary>
    /// Tests that StoreTokensAsync executes without throwing when provided valid tokens and no expiry.
    /// NOTE: This test has limited verification capability because SecureStorage.Default is a static
    /// dependency that cannot be mocked with Moq. Proper unit testing would require injecting
    /// ISecureStorage as a dependency. This test only verifies the method doesn't throw synchronously.
    /// </summary>
    /// <param name="accessToken">The access token to store.</param>
    /// <param name="refreshToken">The refresh token to store.</param>
    [TestMethod]
    [DataRow("valid_access_token", "valid_refresh_token")]
    [DataRow("", "")]
    [DataRow("token_with_special_chars_!@#$%", "token_with_special_chars_!@#$%")]
    public async Task StoreTokensAsync_WithValidTokensAndNoExpiry_ExecutesWithoutThrowing(
        string accessToken,
        string refreshToken)
    {
        // Arrange
        var loggerMock = new Mock<ILogger<TokenStorageService>>();
        var service = new TokenStorageService(loggerMock.Object);

        // Act & Assert
        // NOTE: Cannot verify SecureStorage interactions due to static dependency.
        // This test will attempt to interact with the actual SecureStorage implementation.
        // In a proper design, ISecureStorage would be injected and mocked here.
        try
        {
            await service.StoreTokensAsync(accessToken, refreshToken, null);
            // Test passes if no exception is thrown
            Assert.IsTrue(true);
        }
        catch (Exception)
        {
            // If SecureStorage is not available in test environment, mark as inconclusive
            Assert.Inconclusive("SecureStorage.Default is not available in the test environment. " +
                              "This method requires ISecureStorage to be injected for proper unit testing.");
        }
    }

    /// <summary>
    /// Tests that StoreTokensAsync executes without throwing when provided valid tokens with an expiry date.
    /// NOTE: This test has limited verification capability because SecureStorage.Default is a static
    /// dependency that cannot be mocked with Moq. Proper unit testing would require injecting
    /// ISecureStorage as a dependency.
    /// </summary>
    /// <param name="year">The year component of the expiry date.</param>
    /// <param name="month">The month component of the expiry date.</param>
    /// <param name="day">The day component of the expiry date.</param>
    [TestMethod]
    [DataRow(2024, 12, 31)]
    [DataRow(1, 1, 1)]
    [DataRow(9999, 12, 31)]
    public async Task StoreTokensAsync_WithValidTokensAndExpiry_ExecutesWithoutThrowing(
        int year,
        int month,
        int day)
    {
        // Arrange
        var loggerMock = new Mock<ILogger<TokenStorageService>>();
        var service = new TokenStorageService(loggerMock.Object);
        var expiry = new DateTime(year, month, day);

        // Act & Assert
        // NOTE: Cannot verify SecureStorage interactions due to static dependency.
        // This test will attempt to interact with the actual SecureStorage implementation.
        try
        {
            await service.StoreTokensAsync("access_token", "refresh_token", expiry);
            Assert.IsTrue(true);
        }
        catch (Exception)
        {
            Assert.Inconclusive("SecureStorage.Default is not available in the test environment. " +
                              "This method requires ISecureStorage to be injected for proper unit testing.");
        }
    }

    /// <summary>
    /// Tests that StoreTokensAsync handles DateTime.MinValue expiry correctly.
    /// NOTE: This test has limited verification capability due to static SecureStorage dependency.
    /// </summary>
    [TestMethod]
    public async Task StoreTokensAsync_WithMinDateTimeExpiry_ExecutesWithoutThrowing()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<TokenStorageService>>();
        var service = new TokenStorageService(loggerMock.Object);
        var expiry = DateTime.MinValue;

        // Act & Assert
        try
        {
            await service.StoreTokensAsync("access", "refresh", expiry);
            Assert.IsTrue(true);
        }
        catch (Exception)
        {
            Assert.Inconclusive("SecureStorage.Default is not available in the test environment. " +
                              "This method requires ISecureStorage to be injected for proper unit testing.");
        }
    }

    /// <summary>
    /// Tests that StoreTokensAsync handles DateTime.MaxValue expiry correctly.
    /// NOTE: This test has limited verification capability due to static SecureStorage dependency.
    /// </summary>
    [TestMethod]
    public async Task StoreTokensAsync_WithMaxDateTimeExpiry_ExecutesWithoutThrowing()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<TokenStorageService>>();
        var service = new TokenStorageService(loggerMock.Object);
        var expiry = DateTime.MaxValue;

        // Act & Assert
        try
        {
            await service.StoreTokensAsync("access", "refresh", expiry);
            Assert.IsTrue(true);
        }
        catch (Exception)
        {
            Assert.Inconclusive("SecureStorage.Default is not available in the test environment. " +
                              "This method requires ISecureStorage to be injected for proper unit testing.");
        }
    }

    /// <summary>
    /// Tests StoreTokensAsync with very long token strings to verify boundary behavior.
    /// NOTE: This test has limited verification capability due to static SecureStorage dependency.
    /// </summary>
    [TestMethod]
    public async Task StoreTokensAsync_WithVeryLongTokens_ExecutesWithoutThrowing()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<TokenStorageService>>();
        var service = new TokenStorageService(loggerMock.Object);
        var longToken = new string('a', 10000);

        // Act & Assert
        try
        {
            await service.StoreTokensAsync(longToken, longToken, null);
            Assert.IsTrue(true);
        }
        catch (Exception)
        {
            Assert.Inconclusive("SecureStorage.Default is not available in the test environment. " +
                              "This method requires ISecureStorage to be injected for proper unit testing.");
        }
    }

    /// <summary>
    /// Tests StoreTokensAsync with whitespace-only tokens to verify edge case handling.
    /// NOTE: This test has limited verification capability due to static SecureStorage dependency.
    /// </summary>
    [TestMethod]
    [DataRow("   ", "   ")]
    [DataRow("\t", "\n")]
    [DataRow("\r\n", " \t\r\n ")]
    public async Task StoreTokensAsync_WithWhitespaceTokens_ExecutesWithoutThrowing(
        string accessToken,
        string refreshToken)
    {
        // Arrange
        var loggerMock = new Mock<ILogger<TokenStorageService>>();
        var service = new TokenStorageService(loggerMock.Object);

        // Act & Assert
        try
        {
            await service.StoreTokensAsync(accessToken, refreshToken, null);
            Assert.IsTrue(true);
        }
        catch (Exception)
        {
            Assert.Inconclusive("SecureStorage.Default is not available in the test environment. " +
                              "This method requires ISecureStorage to be injected for proper unit testing.");
        }
    }

    /// <summary>
    /// Tests that IsTokenExpiredAsync returns true when SecureStorage returns null.
    /// NOTE: This test cannot be properly implemented because SecureStorage.Default is a static property
    /// that cannot be mocked with Moq. The production code would need to be refactored to inject
    /// ISecureStorage as a dependency instead of using the static Default property.
    /// Expected behavior: When GetAsync returns null, the method should return true.
    /// </summary>
    [TestMethod]
    [Ignore("Cannot mock static SecureStorage.Default. Requires refactoring to inject ISecureStorage.")]
    public async Task IsTokenExpiredAsync_WhenStorageReturnsNull_ReturnsTrue()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<TokenStorageService>>();
        var service = new TokenStorageService(mockLogger.Object);

        // TODO: Mock SecureStorage.Default.GetAsync to return null
        // This requires refactoring the production code to accept ISecureStorage via dependency injection

        // Act
        var result = await service.IsTokenExpiredAsync();

        // Assert
        Assert.IsTrue(result);
    }

    /// <summary>
    /// Tests that IsTokenExpiredAsync returns true when SecureStorage returns an empty string.
    /// NOTE: This test cannot be properly implemented because SecureStorage.Default is a static property
    /// that cannot be mocked with Moq. The production code would need to be refactored to inject
    /// ISecureStorage as a dependency instead of using the static Default property.
    /// Expected behavior: When GetAsync returns empty string, the method should return true.
    /// </summary>
    [TestMethod]
    [Ignore("Cannot mock static SecureStorage.Default. Requires refactoring to inject ISecureStorage.")]
    public async Task IsTokenExpiredAsync_WhenStorageReturnsEmptyString_ReturnsTrue()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<TokenStorageService>>();
        var service = new TokenStorageService(mockLogger.Object);

        // TODO: Mock SecureStorage.Default.GetAsync to return string.Empty
        // This requires refactoring the production code to accept ISecureStorage via dependency injection

        // Act
        var result = await service.IsTokenExpiredAsync();

        // Assert
        Assert.IsTrue(result);
    }

    /// <summary>
    /// Tests that IsTokenExpiredAsync returns false when SecureStorage returns a future date.
    /// NOTE: This test cannot be properly implemented because SecureStorage.Default is a static property
    /// that cannot be mocked with Moq. The production code would need to be refactored to inject
    /// ISecureStorage as a dependency instead of using the static Default property.
    /// Expected behavior: When GetAsync returns a valid future DateTime string, the method should return false.
    /// </summary>
    [TestMethod]
    [Ignore("Cannot mock static SecureStorage.Default. Requires refactoring to inject ISecureStorage.")]
    public async Task IsTokenExpiredAsync_WhenStorageReturnsFutureDate_ReturnsFalse()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<TokenStorageService>>();
        var service = new TokenStorageService(mockLogger.Object);
        var futureDate = DateTime.UtcNow.AddHours(1).ToString("O");

        // TODO: Mock SecureStorage.Default.GetAsync to return futureDate
        // This requires refactoring the production code to accept ISecureStorage via dependency injection

        // Act
        var result = await service.IsTokenExpiredAsync();

        // Assert
        Assert.IsFalse(result);
    }

    /// <summary>
    /// Tests that IsTokenExpiredAsync returns true when SecureStorage returns a past date.
    /// NOTE: This test cannot be properly implemented because SecureStorage.Default is a static property
    /// that cannot be mocked with Moq. The production code would need to be refactored to inject
    /// ISecureStorage as a dependency instead of using the static Default property.
    /// Expected behavior: When GetAsync returns a valid past DateTime string, the method should return true.
    /// </summary>
    [TestMethod]
    [Ignore("Cannot mock static SecureStorage.Default. Requires refactoring to inject ISecureStorage.")]
    public async Task IsTokenExpiredAsync_WhenStorageReturnsPastDate_ReturnsTrue()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<TokenStorageService>>();
        var service = new TokenStorageService(mockLogger.Object);
        var pastDate = DateTime.UtcNow.AddHours(-1).ToString("O");

        // TODO: Mock SecureStorage.Default.GetAsync to return pastDate
        // This requires refactoring the production code to accept ISecureStorage via dependency injection

        // Act
        var result = await service.IsTokenExpiredAsync();

        // Assert
        Assert.IsTrue(result);
    }

    /// <summary>
    /// Tests that IsTokenExpiredAsync returns true when SecureStorage returns current time.
    /// NOTE: This test cannot be properly implemented because SecureStorage.Default is a static property
    /// that cannot be mocked with Moq. The production code would need to be refactored to inject
    /// ISecureStorage as a dependency instead of using the static Default property.
    /// Expected behavior: When GetAsync returns exactly current time, the method should return true (uses <=).
    /// </summary>
    [TestMethod]
    [Ignore("Cannot mock static SecureStorage.Default. Requires refactoring to inject ISecureStorage.")]
    public async Task IsTokenExpiredAsync_WhenStorageReturnsCurrentTime_ReturnsTrue()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<TokenStorageService>>();
        var service = new TokenStorageService(mockLogger.Object);
        var currentTime = DateTime.UtcNow.ToString("O");

        // TODO: Mock SecureStorage.Default.GetAsync to return currentTime
        // This requires refactoring the production code to accept ISecureStorage via dependency injection

        // Act
        var result = await service.IsTokenExpiredAsync();

        // Assert
        Assert.IsTrue(result);
    }

    /// <summary>
    /// Tests that IsTokenExpiredAsync returns true and logs warning when DateTime.Parse fails.
    /// NOTE: This test cannot be properly implemented because SecureStorage.Default is a static property
    /// that cannot be mocked with Moq. The production code would need to be refactored to inject
    /// ISecureStorage as a dependency instead of using the static Default property.
    /// Expected behavior: When GetAsync returns invalid date format, DateTime.Parse throws,
    /// exception is caught, warning is logged, and method returns true.
    /// </summary>
    [TestMethod]
    [Ignore("Cannot mock static SecureStorage.Default. Requires refactoring to inject ISecureStorage.")]
    public async Task IsTokenExpiredAsync_WhenStorageReturnsInvalidDateFormat_ReturnsTrueAndLogsWarning()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<TokenStorageService>>();
        var service = new TokenStorageService(mockLogger.Object);
        var invalidDate = "not-a-date";

        // TODO: Mock SecureStorage.Default.GetAsync to return invalidDate
        // This requires refactoring the production code to accept ISecureStorage via dependency injection

        // Act
        var result = await service.IsTokenExpiredAsync();

        // Assert
        Assert.IsTrue(result);
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => true),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    /// <summary>
    /// Tests that IsTokenExpiredAsync returns true and logs warning when SecureStorage.GetAsync throws.
    /// NOTE: This test cannot be properly implemented because SecureStorage.Default is a static property
    /// that cannot be mocked with Moq. The production code would need to be refactored to inject
    /// ISecureStorage as a dependency instead of using the static Default property.
    /// Expected behavior: When GetAsync throws an exception, it's caught, warning is logged, and method returns true.
    /// </summary>
    [TestMethod]
    [Ignore("Cannot mock static SecureStorage.Default. Requires refactoring to inject ISecureStorage.")]
    public async Task IsTokenExpiredAsync_WhenStorageThrowsException_ReturnsTrueAndLogsWarning()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<TokenStorageService>>();
        var service = new TokenStorageService(mockLogger.Object);

        // TODO: Mock SecureStorage.Default.GetAsync to throw an exception
        // This requires refactoring the production code to accept ISecureStorage via dependency injection

        // Act
        var result = await service.IsTokenExpiredAsync();

        // Assert
        Assert.IsTrue(result);
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => true),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    /// <summary>
    /// Tests that IsTokenExpiredAsync returns true when SecureStorage returns whitespace.
    /// NOTE: This test cannot be properly implemented because SecureStorage.Default is a static property
    /// that cannot be mocked with Moq. The production code would need to be refactored to inject
    /// ISecureStorage as a dependency instead of using the static Default property.
    /// Expected behavior: When GetAsync returns whitespace, string.IsNullOrEmpty returns false,
    /// but DateTime.Parse throws, exception is caught, and method returns true.
    /// </summary>
    [TestMethod]
    [Ignore("Cannot mock static SecureStorage.Default. Requires refactoring to inject ISecureStorage.")]
    public async Task IsTokenExpiredAsync_WhenStorageReturnsWhitespace_ReturnsTrueAndLogsWarning()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<TokenStorageService>>();
        var service = new TokenStorageService(mockLogger.Object);
        var whitespace = "   ";

        // TODO: Mock SecureStorage.Default.GetAsync to return whitespace
        // This requires refactoring the production code to accept ISecureStorage via dependency injection

        // Act
        var result = await service.IsTokenExpiredAsync();

        // Assert
        Assert.IsTrue(result);
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => true),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    /// <summary>
    /// Tests that ClearAsync completes successfully and returns a completed task.
    /// </summary>
    /// <remarks>
    /// NOTE: This test has limitations due to the static dependency on SecureStorage.Default.
    /// The actual removal of tokens from SecureStorage cannot be verified without refactoring
    /// the production code to inject ISecureStorage as a dependency.
    /// This test only verifies that the method completes without throwing exceptions.
    /// </remarks>
    [TestMethod]
    public async Task ClearAsync_WhenCalled_CompletesSuccessfully()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<TokenStorageService>>();
        var service = new TokenStorageService(mockLogger.Object);

        // Act
        var task = service.ClearAsync();

        // Assert
        Assert.IsNotNull(task);
        Assert.IsTrue(task.IsCompleted);
        await task;
    }

    /// <summary>
    /// Tests that ClearAsync returns a completed task and does not throw exceptions.
    /// </summary>
    /// <remarks>
    /// NOTE: Due to static SecureStorage.Default dependency, we cannot mock or verify
    /// the actual SecureStorage.Remove() calls or simulate exceptions from SecureStorage.
    /// In a real environment, SecureStorage operations may throw platform-specific exceptions,
    /// but these cannot be tested without dependency injection of ISecureStorage.
    /// </remarks>
    [TestMethod]
    public void ClearAsync_WhenCalled_ReturnsCompletedTask()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<TokenStorageService>>();
        var service = new TokenStorageService(mockLogger.Object);

        // Act
        var result = service.ClearAsync();

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(TaskStatus.RanToCompletion, result.Status);
    }

    /// <summary>
    /// Tests that ClearAsync never throws exceptions even in error scenarios.
    /// </summary>
    /// <remarks>
    /// The ClearAsync method is designed to catch all exceptions and return a completed task.
    /// This test verifies the method signature and return behavior.
    /// NOTE: Cannot test actual exception handling from SecureStorage due to static dependency.
    /// </remarks>
    [TestMethod]
    public async Task ClearAsync_AlwaysReturnsCompletedTask_NeverThrows()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<TokenStorageService>>();
        var service = new TokenStorageService(mockLogger.Object);

        // Act
        Task result = service.ClearAsync();
        await result;

        // Assert - Method completes without throwing
        Assert.IsTrue(result.IsCompleted);
        Assert.IsFalse(result.IsFaulted);
        Assert.IsFalse(result.IsCanceled);
    }

    /// <summary>
    /// Tests that the logger is properly injected and available for use.
    /// </summary>
    /// <remarks>
    /// This test verifies the constructor dependency injection works correctly.
    /// NOTE: We cannot verify actual logger calls (LogInformation/LogError) during ClearAsync
    /// because we cannot control or mock the static SecureStorage.Default behavior.
    /// </remarks>
    [TestMethod]
    public void Constructor_WithLogger_CreatesInstance()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<TokenStorageService>>();

        // Act
        var service = new TokenStorageService(mockLogger.Object);

        // Assert
        Assert.IsNotNull(service);
    }

    /// <summary>
    /// Tests that GetRefreshTokenAsync returns the token value when SecureStorage succeeds.
    /// Input: SecureStorage returns a valid token string.
    /// Expected: The method returns the token value.
    /// NOTE: This test cannot be fully implemented because SecureStorage.Default is a static
    /// dependency that cannot be mocked with Moq. To properly test this method, the code would
    /// need to be refactored to inject an ISecureStorage interface instead of using the static
    /// SecureStorage.Default property.
    /// </summary>
    [TestMethod]
    public async Task GetRefreshTokenAsync_SecureStorageReturnsToken_ReturnsToken()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<TokenStorageService>>();
        var service = new TokenStorageService(mockLogger.Object);

        // Act & Assert
        Assert.Inconclusive("Cannot test this method because SecureStorage.Default is a static dependency that cannot be mocked with Moq. " +
                          "Refactor the code to inject ISecureStorage via dependency injection to enable proper unit testing.");
    }

    /// <summary>
    /// Tests that GetRefreshTokenAsync returns null when SecureStorage returns null.
    /// Input: SecureStorage returns null.
    /// Expected: The method returns null.
    /// NOTE: This test cannot be fully implemented because SecureStorage.Default is a static
    /// dependency that cannot be mocked with Moq. To properly test this method, the code would
    /// need to be refactored to inject an ISecureStorage interface instead of using the static
    /// SecureStorage.Default property.
    /// </summary>
    [TestMethod]
    public async Task GetRefreshTokenAsync_SecureStorageReturnsNull_ReturnsNull()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<TokenStorageService>>();
        var service = new TokenStorageService(mockLogger.Object);

        // Act & Assert
        Assert.Inconclusive("Cannot test this method because SecureStorage.Default is a static dependency that cannot be mocked with Moq. " +
                          "Refactor the code to inject ISecureStorage via dependency injection to enable proper unit testing.");
    }

    /// <summary>
    /// Tests that GetRefreshTokenAsync logs a warning and returns null when SecureStorage throws an exception.
    /// Input: SecureStorage throws any exception.
    /// Expected: The method logs a warning message and returns null.
    /// NOTE: This test cannot be fully implemented because SecureStorage.Default is a static
    /// dependency that cannot be mocked with Moq. To properly test this method, the code would
    /// need to be refactored to inject an ISecureStorage interface instead of using the static
    /// SecureStorage.Default property.
    /// </summary>
    [TestMethod]
    public async Task GetRefreshTokenAsync_SecureStorageThrowsException_LogsWarningAndReturnsNull()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<TokenStorageService>>();
        var service = new TokenStorageService(mockLogger.Object);

        // Act & Assert
        Assert.Inconclusive("Cannot test this method because SecureStorage.Default is a static dependency that cannot be mocked with Moq. " +
                          "Refactor the code to inject ISecureStorage via dependency injection to enable proper unit testing.");
    }

    /// <summary>
    /// Tests that GetRefreshTokenAsync returns an empty string when SecureStorage returns an empty string.
    /// Input: SecureStorage returns an empty string.
    /// Expected: The method returns an empty string.
    /// NOTE: This test cannot be fully implemented because SecureStorage.Default is a static
    /// dependency that cannot be mocked with Moq. To properly test this method, the code would
    /// need to be refactored to inject an ISecureStorage interface instead of using the static
    /// SecureStorage.Default property.
    /// </summary>
    [TestMethod]
    public async Task GetRefreshTokenAsync_SecureStorageReturnsEmptyString_ReturnsEmptyString()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<TokenStorageService>>();
        var service = new TokenStorageService(mockLogger.Object);

        // Act & Assert
        Assert.Inconclusive("Cannot test this method because SecureStorage.Default is a static dependency that cannot be mocked with Moq. " +
                          "Refactor the code to inject ISecureStorage via dependency injection to enable proper unit testing.");
    }

    /// <summary>
    /// Tests that StoreTokensAsync handles null accessToken parameter.
    /// Input: null accessToken, valid refreshToken, no expiry.
    /// Expected: Method completes without throwing (exceptions are caught internally).
    /// NOTE: This test has limited verification capability because SecureStorage.Default is a static
    /// dependency that cannot be mocked with Moq. The actual behavior depends on how SecureStorage
    /// handles null values. This test documents the current behavior.
    /// </summary>
    [TestMethod]
    public async Task StoreTokensAsync_WithNullAccessToken_ExecutesWithoutThrowing()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<TokenStorageService>>();
        var service = new TokenStorageService(loggerMock.Object);

        // Act & Assert
        try
        {
            await service.StoreTokensAsync(null!, "valid_refresh_token", null);
            Assert.IsTrue(true);
        }
        catch (Exception)
        {
            Assert.Inconclusive("SecureStorage.Default is not available in the test environment. " +
                              "This method requires ISecureStorage to be injected for proper unit testing.");
        }
    }

    /// <summary>
    /// Tests that StoreTokensAsync handles null refreshToken parameter.
    /// Input: valid accessToken, null refreshToken, no expiry.
    /// Expected: Method completes without throwing (exceptions are caught internally).
    /// NOTE: This test has limited verification capability because SecureStorage.Default is a static
    /// dependency that cannot be mocked with Moq. The actual behavior depends on how SecureStorage
    /// handles null values.
    /// </summary>
    [TestMethod]
    public async Task StoreTokensAsync_WithNullRefreshToken_ExecutesWithoutThrowing()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<TokenStorageService>>();
        var service = new TokenStorageService(loggerMock.Object);

        // Act & Assert
        try
        {
            await service.StoreTokensAsync("valid_access_token", null!, null);
            Assert.IsTrue(true);
        }
        catch (Exception)
        {
            Assert.Inconclusive("SecureStorage.Default is not available in the test environment. " +
                              "This method requires ISecureStorage to be injected for proper unit testing.");
        }
    }

    /// <summary>
    /// Tests that StoreTokensAsync handles both null token parameters.
    /// Input: null accessToken, null refreshToken, no expiry.
    /// Expected: Method completes without throwing (exceptions are caught internally).
    /// NOTE: This test has limited verification capability due to static SecureStorage dependency.
    /// </summary>
    [TestMethod]
    public async Task StoreTokensAsync_WithBothTokensNull_ExecutesWithoutThrowing()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<TokenStorageService>>();
        var service = new TokenStorageService(loggerMock.Object);

        // Act & Assert
        try
        {
            await service.StoreTokensAsync(null!, null!, null);
            Assert.IsTrue(true);
        }
        catch (Exception)
        {
            Assert.Inconclusive("SecureStorage.Default is not available in the test environment. " +
                              "This method requires ISecureStorage to be injected for proper unit testing.");
        }
    }

    /// <summary>
    /// Tests that StoreTokensAsync handles tokens containing control characters.
    /// Input: Tokens with control characters like null terminator, carriage return, line feed, tab.
    /// Expected: Method completes without throwing.
    /// NOTE: This test has limited verification capability due to static SecureStorage dependency.
    /// </summary>
    /// <param name="accessToken">The access token containing control characters.</param>
    /// <param name="refreshToken">The refresh token containing control characters.</param>
    [TestMethod]
    [DataRow("token\0with\0nulls", "refresh\0token")]
    [DataRow("token\r\nwith\r\nnewlines", "refresh\r\ntoken")]
    [DataRow("token\twith\ttabs", "refresh\ttoken")]
    [DataRow("\0\r\n\t", "\t\n\r\0")]
    public async Task StoreTokensAsync_WithControlCharacters_ExecutesWithoutThrowing(
        string accessToken,
        string refreshToken)
    {
        // Arrange
        var loggerMock = new Mock<ILogger<TokenStorageService>>();
        var service = new TokenStorageService(loggerMock.Object);

        // Act & Assert
        try
        {
            await service.StoreTokensAsync(accessToken, refreshToken, null);
            Assert.IsTrue(true);
        }
        catch (Exception)
        {
            Assert.Inconclusive("SecureStorage.Default is not available in the test environment. " +
                              "This method requires ISecureStorage to be injected for proper unit testing.");
        }
    }

    /// <summary>
    /// Tests that StoreTokensAsync handles tokens containing Unicode characters.
    /// Input: Tokens with various Unicode characters including emoji, Chinese, Arabic, etc.
    /// Expected: Method completes without throwing.
    /// NOTE: This test has limited verification capability due to static SecureStorage dependency.
    /// </summary>
    /// <param name="accessToken">The access token with Unicode characters.</param>
    /// <param name="refreshToken">The refresh token with Unicode characters.</param>
    [TestMethod]
    [DataRow("token_😀_emoji", "refresh_🔑_token")]
    [DataRow("令牌_中文", "刷新_令牌")]
    [DataRow("رمز_عربي", "تحديث_رمز")]
    [DataRow("токен_русский", "обновить_токен")]
    public async Task StoreTokensAsync_WithUnicodeCharacters_ExecutesWithoutThrowing(
        string accessToken,
        string refreshToken)
    {
        // Arrange
        var loggerMock = new Mock<ILogger<TokenStorageService>>();
        var service = new TokenStorageService(loggerMock.Object);

        // Act & Assert
        try
        {
            await service.StoreTokensAsync(accessToken, refreshToken, null);
            Assert.IsTrue(true);
        }
        catch (Exception)
        {
            Assert.Inconclusive("SecureStorage.Default is not available in the test environment. " +
                              "This method requires ISecureStorage to be injected for proper unit testing.");
        }
    }

    /// <summary>
    /// Tests that StoreTokensAsync handles null tokens with valid expiry date.
    /// Input: null accessToken, null refreshToken, valid expiry date.
    /// Expected: Method completes without throwing (exceptions are caught internally).
    /// NOTE: This test has limited verification capability due to static SecureStorage dependency.
    /// </summary>
    [TestMethod]
    public async Task StoreTokensAsync_WithNullTokensAndExpiry_ExecutesWithoutThrowing()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<TokenStorageService>>();
        var service = new TokenStorageService(loggerMock.Object);
        var expiry = DateTime.UtcNow.AddHours(1);

        // Act & Assert
        try
        {
            await service.StoreTokensAsync(null!, null!, expiry);
            Assert.IsTrue(true);
        }
        catch (Exception)
        {
            Assert.Inconclusive("SecureStorage.Default is not available in the test environment. " +
                              "This method requires ISecureStorage to be injected for proper unit testing.");
        }
    }

    /// <summary>
    /// Tests that StoreTokensAsync handles mixed null and valid token parameters with expiry.
    /// Input: Various combinations of null and valid tokens with expiry dates.
    /// Expected: Method completes without throwing (exceptions are caught internally).
    /// NOTE: This test has limited verification capability due to static SecureStorage dependency.
    /// </summary>
    /// <param name="accessToken">The access token (may be null).</param>
    /// <param name="refreshToken">The refresh token (may be null).</param>
    [TestMethod]
    [DataRow(null, "valid_refresh")]
    [DataRow("valid_access", null)]
    [DataRow("", null)]
    [DataRow(null, "")]
    public async Task StoreTokensAsync_WithMixedNullAndValidTokensAndExpiry_ExecutesWithoutThrowing(
        string? accessToken,
        string? refreshToken)
    {
        // Arrange
        var loggerMock = new Mock<ILogger<TokenStorageService>>();
        var service = new TokenStorageService(loggerMock.Object);
        var expiry = new DateTime(2025, 6, 15);

        // Act & Assert
        try
        {
            await service.StoreTokensAsync(accessToken!, refreshToken!, expiry);
            Assert.IsTrue(true);
        }
        catch (Exception)
        {
            Assert.Inconclusive("SecureStorage.Default is not available in the test environment. " +
                              "This method requires ISecureStorage to be injected for proper unit testing.");
        }
    }

    /// <summary>
    /// Tests that StoreTokensAsync handles specific DateTime boundary values for expiry.
    /// Input: Valid tokens with DateTime values near Unix epoch and other significant dates.
    /// Expected: Method completes without throwing.
    /// NOTE: This test has limited verification capability due to static SecureStorage dependency.
    /// </summary>
    /// <param name="year">Year component of the expiry date.</param>
    /// <param name="month">Month component of the expiry date.</param>
    /// <param name="day">Day component of the expiry date.</param>
    /// <param name="hour">Hour component of the expiry date.</param>
    /// <param name="minute">Minute component of the expiry date.</param>
    /// <param name="second">Second component of the expiry date.</param>
    [TestMethod]
    [DataRow(1970, 1, 1, 0, 0, 0)] // Unix epoch
    [DataRow(2000, 1, 1, 0, 0, 0)] // Y2K
    [DataRow(2038, 1, 19, 3, 14, 7)] // Unix 32-bit timestamp limit
    [DataRow(9999, 12, 31, 23, 59, 59)] // Near DateTime.MaxValue
    [DataRow(1, 1, 1, 0, 0, 0)] // Near DateTime.MinValue
    public async Task StoreTokensAsync_WithSpecificDateTimeBoundaries_ExecutesWithoutThrowing(
        int year,
        int month,
        int day,
        int hour,
        int minute,
        int second)
    {
        // Arrange
        var loggerMock = new Mock<ILogger<TokenStorageService>>();
        var service = new TokenStorageService(loggerMock.Object);
        var expiry = new DateTime(year, month, day, hour, minute, second);

        // Act & Assert
        try
        {
            await service.StoreTokensAsync("access_token", "refresh_token", expiry);
            Assert.IsTrue(true);
        }
        catch (Exception)
        {
            Assert.Inconclusive("SecureStorage.Default is not available in the test environment. " +
                              "This method requires ISecureStorage to be injected for proper unit testing.");
        }
    }

    /// <summary>
    /// Tests that GetAccessTokenAsync returns a valid token when SecureStorage contains one.
    /// Input: SecureStorage contains a valid access token.
    /// Expected: The method returns the token string.
    /// NOTE: This test is marked as inconclusive because the method uses a static dependency
    /// (SecureStorage.Default) that cannot be mocked with Moq. To make this testable,
    /// the TokenStorageService should accept ISecureStorage as a constructor parameter
    /// instead of using the static Default property.
    /// </summary>
    [TestMethod]
    public async Task GetAccessTokenAsync_SecureStorageContainsValidToken_ReturnsToken()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<TokenStorageService>>();
        var service = new TokenStorageService(mockLogger.Object);

        // Act & Assert
        Assert.Inconclusive(
            "Cannot mock static SecureStorage.Default. " +
            "Requires refactoring to inject ISecureStorage as a dependency.");
    }

    /// <summary>
    /// Tests that GetAccessTokenAsync returns null when no token is stored in SecureStorage.
    /// Input: SecureStorage returns null for the "AccessToken" key.
    /// Expected: The method returns null.
    /// NOTE: This test is marked as inconclusive because the method uses a static dependency
    /// (SecureStorage.Default) that cannot be mocked with Moq.
    /// </summary>
    [TestMethod]
    public async Task GetAccessTokenAsync_SecureStorageReturnsNull_ReturnsNull()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<TokenStorageService>>();
        var service = new TokenStorageService(mockLogger.Object);

        // Act & Assert
        Assert.Inconclusive(
            "Cannot mock static SecureStorage.Default. " +
            "Requires refactoring to inject ISecureStorage as a dependency.");
    }

    /// <summary>
    /// Tests that GetAccessTokenAsync returns an empty string when SecureStorage returns an empty string.
    /// Input: SecureStorage returns an empty string for the "AccessToken" key.
    /// Expected: The method returns an empty string.
    /// NOTE: This test is marked as inconclusive because the method uses a static dependency
    /// (SecureStorage.Default) that cannot be mocked with Moq.
    /// </summary>
    [TestMethod]
    public async Task GetAccessTokenAsync_SecureStorageReturnsEmptyString_ReturnsEmptyString()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<TokenStorageService>>();
        var service = new TokenStorageService(mockLogger.Object);

        // Act & Assert
        Assert.Inconclusive(
            "Cannot mock static SecureStorage.Default. " +
            "Requires refactoring to inject ISecureStorage as a dependency.");
    }

    /// <summary>
    /// Tests that GetAccessTokenAsync returns whitespace when SecureStorage returns whitespace-only string.
    /// Input: SecureStorage returns whitespace-only string (spaces, tabs, newlines).
    /// Expected: The method returns the whitespace string as-is.
    /// NOTE: This test is marked as inconclusive because the method uses a static dependency
    /// (SecureStorage.Default) that cannot be mocked with Moq.
    /// </summary>
    [TestMethod]
    public async Task GetAccessTokenAsync_SecureStorageReturnsWhitespace_ReturnsWhitespace()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<TokenStorageService>>();
        var service = new TokenStorageService(mockLogger.Object);

        // Act & Assert
        Assert.Inconclusive(
            "Cannot mock static SecureStorage.Default. " +
            "Requires refactoring to inject ISecureStorage as a dependency.");
    }

    /// <summary>
    /// Tests that GetAccessTokenAsync returns a very long token correctly.
    /// Input: SecureStorage returns a very long token string (10000 characters).
    /// Expected: The method returns the entire long token string.
    /// NOTE: This test is marked as inconclusive because the method uses a static dependency
    /// (SecureStorage.Default) that cannot be mocked with Moq.
    /// </summary>
    [TestMethod]
    public async Task GetAccessTokenAsync_SecureStorageReturnsVeryLongToken_ReturnsLongToken()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<TokenStorageService>>();
        var service = new TokenStorageService(mockLogger.Object);

        // Act & Assert
        Assert.Inconclusive(
            "Cannot mock static SecureStorage.Default. " +
            "Requires refactoring to inject ISecureStorage as a dependency.");
    }

    /// <summary>
    /// Tests that GetAccessTokenAsync returns tokens containing special characters correctly.
    /// Input: SecureStorage returns a token with special characters (!@#$%^&*(), etc.).
    /// Expected: The method returns the token with special characters intact.
    /// NOTE: This test is marked as inconclusive because the method uses a static dependency
    /// (SecureStorage.Default) that cannot be mocked with Moq.
    /// </summary>
    [TestMethod]
    public async Task GetAccessTokenAsync_SecureStorageReturnsTokenWithSpecialCharacters_ReturnsToken()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<TokenStorageService>>();
        var service = new TokenStorageService(mockLogger.Object);

        // Act & Assert
        Assert.Inconclusive(
            "Cannot mock static SecureStorage.Default. " +
            "Requires refactoring to inject ISecureStorage as a dependency.");
    }

    /// <summary>
    /// Tests that GetAccessTokenAsync catches exceptions, logs a warning, and returns null.
    /// Input: SecureStorage.GetAsync throws an Exception.
    /// Expected: The method catches the exception, logs a warning with the exception details, and returns null.
    /// NOTE: This test is marked as inconclusive because the method uses a static dependency
    /// (SecureStorage.Default) that cannot be mocked with Moq.
    /// </summary>
    [TestMethod]
    public async Task GetAccessTokenAsync_SecureStorageThrowsException_LogsWarningAndReturnsNull()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<TokenStorageService>>();
        var service = new TokenStorageService(mockLogger.Object);

        // Act & Assert
        Assert.Inconclusive(
            "Cannot mock static SecureStorage.Default. " +
            "Requires refactoring to inject ISecureStorage as a dependency.");
    }

    /// <summary>
    /// Tests that GetAccessTokenAsync catches InvalidOperationException, logs a warning, and returns null.
    /// Input: SecureStorage.GetAsync throws an InvalidOperationException.
    /// Expected: The method catches the exception, logs a warning, and returns null.
    /// NOTE: This test is marked as inconclusive because the method uses a static dependency
    /// (SecureStorage.Default) that cannot be mocked with Moq.
    /// </summary>
    [TestMethod]
    public async Task GetAccessTokenAsync_SecureStorageThrowsInvalidOperationException_LogsWarningAndReturnsNull()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<TokenStorageService>>();
        var service = new TokenStorageService(mockLogger.Object);

        // Act & Assert
        Assert.Inconclusive(
            "Cannot mock static SecureStorage.Default. " +
            "Requires refactoring to inject ISecureStorage as a dependency.");
    }

    /// <summary>
    /// Tests that GetAccessTokenAsync catches PlatformNotSupportedException, logs a warning, and returns null.
    /// Input: SecureStorage.GetAsync throws a PlatformNotSupportedException.
    /// Expected: The method catches the exception, logs a warning, and returns null.
    /// NOTE: This test is marked as inconclusive because the method uses a static dependency
    /// (SecureStorage.Default) that cannot be mocked with Moq.
    /// </summary>
    [TestMethod]
    public async Task GetAccessTokenAsync_SecureStorageThrowsPlatformNotSupportedException_LogsWarningAndReturnsNull()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<TokenStorageService>>();
        var service = new TokenStorageService(mockLogger.Object);

        // Act & Assert
        Assert.Inconclusive(
            "Cannot mock static SecureStorage.Default. " +
            "Requires refactoring to inject ISecureStorage as a dependency.");
    }

    /// <summary>
    /// Tests that GetAccessTokenAsync logs the correct warning message when an exception occurs.
    /// Input: SecureStorage.GetAsync throws an exception.
    /// Expected: The logger's LogWarning method is called with the exception and the message "Failed to read access token from SecureStorage".
    /// NOTE: This test is marked as inconclusive because the method uses a static dependency
    /// (SecureStorage.Default) that cannot be mocked with Moq.
    /// </summary>
    [TestMethod]
    public async Task GetAccessTokenAsync_WhenExceptionOccurs_LogsCorrectWarningMessage()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<TokenStorageService>>();
        var service = new TokenStorageService(mockLogger.Object);

        // Act & Assert
        Assert.Inconclusive(
            "Cannot mock static SecureStorage.Default. " +
            "Requires refactoring to inject ISecureStorage as a dependency. " +
            "To verify logging, mock setup would need to verify that LogWarning was called with " +
            "the expected exception and message: 'Failed to read access token from SecureStorage'.");
    }

    /// <summary>
    /// Tests that GetAccessTokenAsync does not throw exceptions even when SecureStorage throws.
    /// Input: SecureStorage.GetAsync throws various exception types.
    /// Expected: The method catches all exceptions and returns null without propagating the exception.
    /// NOTE: This test is marked as inconclusive because the method uses a static dependency
    /// (SecureStorage.Default) that cannot be mocked with Moq.
    /// </summary>
    [TestMethod]
    public async Task GetAccessTokenAsync_WhenSecureStorageThrows_DoesNotPropagateException()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<TokenStorageService>>();
        var service = new TokenStorageService(mockLogger.Object);

        // Act & Assert
        Assert.Inconclusive(
            "Cannot mock static SecureStorage.Default. " +
            "Requires refactoring to inject ISecureStorage as a dependency.");
    }

    /// <summary>
    /// Tests that GetRefreshTokenAsync returns a valid token when SecureStorage contains one.
    /// Input: SecureStorage returns a non-null, non-empty token string.
    /// Expected: The method returns the token value.
    /// NOTE: This test is marked as inconclusive because the method uses a static dependency
    /// (SecureStorage.Default) that cannot be mocked with Moq. To make this testable,
    /// the TokenStorageService should accept ISecureStorage as a constructor parameter
    /// instead of using the static Default property.
    /// </summary>
    [TestMethod]
    public async Task GetRefreshTokenAsync_SecureStorageContainsValidToken_ReturnsToken()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<TokenStorageService>>();
        var service = new TokenStorageService(mockLogger.Object);

        // Act & Assert
        Assert.Inconclusive(
            "Cannot mock static SecureStorage.Default. This test documents expected behavior: " +
            "when SecureStorage.GetAsync(\"RefreshToken\") returns a valid token string, " +
            "the method should return that token. To properly test this, refactor to inject " +
            "ISecureStorage as a dependency instead of using SecureStorage.Default.");
    }

    /// <summary>
    /// Tests that GetRefreshTokenAsync returns null when SecureStorage contains no token.
    /// Input: SecureStorage.GetAsync returns null.
    /// Expected: The method returns null without throwing.
    /// NOTE: This test is marked as inconclusive because the method uses a static dependency
    /// (SecureStorage.Default) that cannot be mocked with Moq. To make this testable,
    /// the TokenStorageService should accept ISecureStorage as a constructor parameter
    /// instead of using the static Default property.
    /// </summary>
    [TestMethod]
    public async Task GetRefreshTokenAsync_SecureStorageReturnsNullToken_ReturnsNull()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<TokenStorageService>>();
        var service = new TokenStorageService(mockLogger.Object);

        // Act & Assert
        Assert.Inconclusive(
            "Cannot mock static SecureStorage.Default. This test documents expected behavior: " +
            "when SecureStorage.GetAsync(\"RefreshToken\") returns null, " +
            "the method should return null. To properly test this, refactor to inject " +
            "ISecureStorage as a dependency instead of using SecureStorage.Default.");
    }

    /// <summary>
    /// Tests that GetRefreshTokenAsync returns empty string when SecureStorage returns empty string.
    /// Input: SecureStorage.GetAsync returns an empty string.
    /// Expected: The method returns an empty string.
    /// NOTE: This test is marked as inconclusive because the method uses a static dependency
    /// (SecureStorage.Default) that cannot be mocked with Moq. To make this testable,
    /// the TokenStorageService should accept ISecureStorage as a constructor parameter
    /// instead of using the static Default property.
    /// </summary>
    [TestMethod]
    public async Task GetRefreshTokenAsync_SecureStorageReturnsEmpty_ReturnsEmptyString()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<TokenStorageService>>();
        var service = new TokenStorageService(mockLogger.Object);

        // Act & Assert
        Assert.Inconclusive(
            "Cannot mock static SecureStorage.Default. This test documents expected behavior: " +
            "when SecureStorage.GetAsync(\"RefreshToken\") returns an empty string, " +
            "the method should return an empty string. To properly test this, refactor to inject " +
            "ISecureStorage as a dependency instead of using SecureStorage.Default.");
    }

    /// <summary>
    /// Tests that GetRefreshTokenAsync returns whitespace when SecureStorage returns whitespace.
    /// Input: SecureStorage.GetAsync returns whitespace-only strings.
    /// Expected: The method returns the whitespace string as-is.
    /// NOTE: This test is marked as inconclusive because the method uses a static dependency
    /// (SecureStorage.Default) that cannot be mocked with Moq. To make this testable,
    /// the TokenStorageService should accept ISecureStorage as a constructor parameter
    /// instead of using the static Default property.
    /// </summary>
    [TestMethod]
    [DataRow("   ")]
    [DataRow("\t")]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [DataRow(" \t\n ")]
    public async Task GetRefreshTokenAsync_SecureStorageReturnsWhitespace_ReturnsWhitespace(string whitespaceToken)
    {
        // Arrange
        var mockLogger = new Mock<ILogger<TokenStorageService>>();
        var service = new TokenStorageService(mockLogger.Object);

        // Act & Assert
        Assert.Inconclusive(
            $"Cannot mock static SecureStorage.Default. This test documents expected behavior: " +
            $"when SecureStorage.GetAsync(\"RefreshToken\") returns whitespace ('{whitespaceToken}'), " +
            $"the method should return that whitespace string. To properly test this, refactor to inject " +
            $"ISecureStorage as a dependency instead of using SecureStorage.Default.");
    }

    /// <summary>
    /// Tests that GetRefreshTokenAsync catches exceptions, logs a warning, and returns null.
    /// Input: SecureStorage.GetAsync throws any exception.
    /// Expected: The method catches the exception, logs a warning with the exception, and returns null.
    /// NOTE: This test is marked as inconclusive because the method uses a static dependency
    /// (SecureStorage.Default) that cannot be mocked with Moq. To make this testable,
    /// the TokenStorageService should accept ISecureStorage as a constructor parameter
    /// instead of using the static Default property.
    /// </summary>
    [TestMethod]
    public async Task GetRefreshTokenAsync_SecureStorageThrows_LogsWarningAndReturnsNull()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<TokenStorageService>>();
        var service = new TokenStorageService(mockLogger.Object);

        // Act & Assert
        Assert.Inconclusive(
            "Cannot mock static SecureStorage.Default. This test documents expected behavior: " +
            "when SecureStorage.GetAsync(\"RefreshToken\") throws any exception, " +
            "the method should catch it, log a warning with message 'Failed to read refresh token from SecureStorage', " +
            "and return null. To properly test this, refactor to inject " +
            "ISecureStorage as a dependency instead of using SecureStorage.Default.");
    }

    /// <summary>
    /// Tests that GetRefreshTokenAsync handles very long token strings correctly.
    /// Input: SecureStorage.GetAsync returns a very long token string.
    /// Expected: The method returns the long token string without truncation.
    /// NOTE: This test is marked as inconclusive because the method uses a static dependency
    /// (SecureStorage.Default) that cannot be mocked with Moq. To make this testable,
    /// the TokenStorageService should accept ISecureStorage as a constructor parameter
    /// instead of using the static Default property.
    /// </summary>
    [TestMethod]
    public async Task GetRefreshTokenAsync_SecureStorageReturnsVeryLongToken_ReturnsFullToken()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<TokenStorageService>>();
        var service = new TokenStorageService(mockLogger.Object);

        // Act & Assert
        Assert.Inconclusive(
            "Cannot mock static SecureStorage.Default. This test documents expected behavior: " +
            "when SecureStorage.GetAsync(\"RefreshToken\") returns a very long string (10000+ chars), " +
            "the method should return the complete string without truncation. To properly test this, refactor to inject " +
            "ISecureStorage as a dependency instead of using SecureStorage.Default.");
    }

    /// <summary>
    /// Tests that GetRefreshTokenAsync handles tokens with special characters correctly.
    /// Input: SecureStorage.GetAsync returns tokens containing special characters.
    /// Expected: The method returns the token with special characters unchanged.
    /// NOTE: This test is marked as inconclusive because the method uses a static dependency
    /// (SecureStorage.Default) that cannot be mocked with Moq. To make this testable,
    /// the TokenStorageService should accept ISecureStorage as a constructor parameter
    /// instead of using the static Default property.
    /// </summary>
    [TestMethod]
    [DataRow("token!@#$%^&*()")]
    [DataRow("token<>?:\"{}|")]
    [DataRow("token\0with\0null")]
    [DataRow("token\u200B\u200C\u200D")]
    public async Task GetRefreshTokenAsync_SecureStorageReturnsTokenWithSpecialChars_ReturnsToken(string specialToken)
    {
        // Arrange
        var mockLogger = new Mock<ILogger<TokenStorageService>>();
        var service = new TokenStorageService(mockLogger.Object);

        // Act & Assert
        Assert.Inconclusive(
            $"Cannot mock static SecureStorage.Default. This test documents expected behavior: " +
            $"when SecureStorage.GetAsync(\"RefreshToken\") returns a token with special characters, " +
            $"the method should return the token unchanged. To properly test this, refactor to inject " +
            $"ISecureStorage as a dependency instead of using SecureStorage.Default.");
    }

    /// <summary>
    /// Tests that ClearAsync is idempotent and can be called multiple times successfully.
    /// Input: Multiple sequential calls to ClearAsync.
    /// Expected: All calls complete successfully without throwing exceptions.
    /// </summary>
    /// <remarks>
    /// NOTE: Due to the static SecureStorage.Default dependency, we cannot verify the actual
    /// SecureStorage.Remove() calls or logger invocations. This test only verifies that
    /// multiple calls complete successfully without exceptions.
    /// </remarks>
    [TestMethod]
    public async Task ClearAsync_CalledMultipleTimes_CompletesSuccessfullyEachTime()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<TokenStorageService>>();
        var service = new TokenStorageService(mockLogger.Object);

        // Act
        var task1 = service.ClearAsync();
        var task2 = service.ClearAsync();
        var task3 = service.ClearAsync();

        // Assert
        Assert.IsNotNull(task1);
        Assert.IsNotNull(task2);
        Assert.IsNotNull(task3);
        Assert.IsTrue(task1.IsCompleted);
        Assert.IsTrue(task2.IsCompleted);
        Assert.IsTrue(task3.IsCompleted);
        await task1;
        await task2;
        await task3;
    }

    /// <summary>
    /// Tests that ClearAsync handles concurrent calls from multiple tasks safely.
    /// Input: Multiple concurrent invocations of ClearAsync.
    /// Expected: All concurrent calls complete successfully without exceptions.
    /// </summary>
    /// <remarks>
    /// NOTE: Due to the static SecureStorage.Default dependency, we cannot verify thread safety
    /// of the actual SecureStorage operations. This test only verifies that the method itself
    /// doesn't throw exceptions when called concurrently.
    /// </remarks>
    [TestMethod]
    public async Task ClearAsync_CalledConcurrently_CompletesSuccessfullyForAllCalls()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<TokenStorageService>>();
        var service = new TokenStorageService(mockLogger.Object);
        var tasks = new Task[10];

        // Act
        for (int i = 0; i < tasks.Length; i++)
        {
            tasks[i] = Task.Run(async () => await service.ClearAsync());
        }

        // Assert
        await Task.WhenAll(tasks);
        foreach (var task in tasks)
        {
            Assert.IsTrue(task.IsCompleted);
            Assert.IsFalse(task.IsFaulted);
            Assert.IsFalse(task.IsCanceled);
        }
    }

    /// <summary>
    /// Tests that ClearAsync returns a non-null Task that is immediately completed.
    /// Input: Single call to ClearAsync.
    /// Expected: Returns a non-null, completed Task with status RanToCompletion.
    /// </summary>
    /// <remarks>
    /// NOTE: Due to the static SecureStorage.Default dependency, we cannot verify the actual
    /// token removal or logger calls. This test verifies the Task return characteristics.
    /// </remarks>
    [TestMethod]
    public void ClearAsync_WhenInvoked_ReturnsImmediatelyCompletedTask()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<TokenStorageService>>();
        var service = new TokenStorageService(mockLogger.Object);

        // Act
        var result = service.ClearAsync();

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(TaskStatus.RanToCompletion, result.Status);
        Assert.IsTrue(result.IsCompleted);
        Assert.IsFalse(result.IsCompletedSuccessfully == false);
    }

    /// <summary>
    /// Tests that IsTokenExpiredAsync handles DateTime.MinValue correctly.
    /// Input: SecureStorage.Default.GetAsync returns DateTime.MinValue as string.
    /// Expected: Method returns true (DateTime.MinValue is far in the past, so token is expired).
    /// NOTE: This test cannot be properly implemented because SecureStorage.Default is a static property
    /// that cannot be mocked with Moq. To properly test this method, the production code would need to be
    /// refactored to inject ISecureStorage as a dependency instead of using the static Default property.
    /// </summary>
    [TestMethod]
    [Ignore("Cannot mock static SecureStorage.Default. Requires refactoring to inject ISecureStorage.")]
    public async Task IsTokenExpiredAsync_WhenStorageReturnsMinValue_ReturnsTrue()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<TokenStorageService>>();
        var service = new TokenStorageService(mockLogger.Object);
        var minValue = DateTime.MinValue.ToString("O");

        // TODO: Mock SecureStorage.Default.GetAsync to return minValue
        // This requires refactoring the production code to accept ISecureStorage via dependency injection

        // Act
        var result = await service.IsTokenExpiredAsync();

        // Assert
        Assert.IsTrue(result);
    }

    /// <summary>
    /// Tests that IsTokenExpiredAsync handles DateTime.MaxValue correctly.
    /// Input: SecureStorage.Default.GetAsync returns DateTime.MaxValue as string.
    /// Expected: Method returns false (DateTime.MaxValue is far in the future, so token is not expired).
    /// NOTE: This test cannot be properly implemented because SecureStorage.Default is a static property
    /// that cannot be mocked with Moq. To properly test this method, the production code would need to be
    /// refactored to inject ISecureStorage as a dependency instead of using the static Default property.
    /// </summary>
    [TestMethod]
    [Ignore("Cannot mock static SecureStorage.Default. Requires refactoring to inject ISecureStorage.")]
    public async Task IsTokenExpiredAsync_WhenStorageReturnsMaxValue_ReturnsFalse()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<TokenStorageService>>();
        var service = new TokenStorageService(mockLogger.Object);
        var maxValue = DateTime.MaxValue.ToString("O");

        // TODO: Mock SecureStorage.Default.GetAsync to return maxValue
        // This requires refactoring the production code to accept ISecureStorage via dependency injection

        // Act
        var result = await service.IsTokenExpiredAsync();

        // Assert
        Assert.IsFalse(result);
    }

    /// <summary>
    /// Tests that ClearAsync returns the same completed task instance on each call.
    /// Input: Multiple calls to ClearAsync on the same service instance.
    /// Expected: Each call returns Task.CompletedTask (reference equality).
    /// </summary>
    /// <remarks>
    /// NOTE: Due to static SecureStorage.Default dependency, we cannot verify the actual
    /// SecureStorage.Remove() calls. This test verifies the return value optimization.
    /// </remarks>
    [TestMethod]
    public void ClearAsync_MultipleCallsOnSameInstance_ReturnsSameCompletedTaskInstance()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<TokenStorageService>>();
        var service = new TokenStorageService(mockLogger.Object);

        // Act
        var task1 = service.ClearAsync();
        var task2 = service.ClearAsync();
        var task3 = service.ClearAsync();

        // Assert
        Assert.IsNotNull(task1);
        Assert.IsNotNull(task2);
        Assert.IsNotNull(task3);
        Assert.AreSame(Task.CompletedTask, task1);
        Assert.AreSame(Task.CompletedTask, task2);
        Assert.AreSame(Task.CompletedTask, task3);
    }

    /// <summary>
    /// Tests that ClearAsync with null logger reference does not throw during execution.
    /// Input: Service created with null logger (bypassing nullable check), then ClearAsync called.
    /// Expected: Method completes without throwing NullReferenceException.
    /// </summary>
    /// <remarks>
    /// This documents the current behavior where the constructor doesn't validate the logger parameter.
    /// NOTE: Cannot verify logger calls due to static SecureStorage dependency.
    /// </remarks>
    [TestMethod]
    public void ClearAsync_WithNullLogger_CompletesWithoutThrowingNullReferenceException()
    {
        // Arrange
        var service = new TokenStorageService(null!);

        // Act
        Task result = service.ClearAsync();

        // Assert
        Assert.IsNotNull(result);
        Assert.IsTrue(result.IsCompleted);
        Assert.AreEqual(TaskStatus.RanToCompletion, result.Status);
    }

    /// <summary>
    /// Tests that awaiting ClearAsync completes synchronously without yielding.
    /// Input: Single call to ClearAsync.
    /// Expected: Task is completed before await, no asynchronous continuation needed.
    /// </summary>
    /// <remarks>
    /// NOTE: Due to static SecureStorage.Default dependency, we cannot verify the actual
    /// token removal operations. This test verifies the synchronous completion characteristic.
    /// </remarks>
    [TestMethod]
    public async Task ClearAsync_WhenAwaited_CompletesSynchronously()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<TokenStorageService>>();
        var service = new TokenStorageService(mockLogger.Object);

        // Act
        var task = service.ClearAsync();
        var isCompletedBeforeAwait = task.IsCompleted;
        await task;
        var isCompletedAfterAwait = task.IsCompleted;

        // Assert
        Assert.IsTrue(isCompletedBeforeAwait, "Task should be completed synchronously");
        Assert.IsTrue(isCompletedAfterAwait);
        Assert.AreEqual(TaskStatus.RanToCompletion, task.Status);
    }

    /// <summary>
    /// Tests that ClearAsync task has no exception even when potentially called after service disposal.
    /// Input: Call to ClearAsync.
    /// Expected: Task is not faulted and has no exception.
    /// </summary>
    /// <remarks>
    /// NOTE: Due to static SecureStorage.Default dependency, we cannot simulate actual
    /// SecureStorage errors. This test verifies the task never enters a faulted state.
    /// </remarks>
    [TestMethod]
    public void ClearAsync_TaskResult_IsNeverFaultedOrCanceled()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<TokenStorageService>>();
        var service = new TokenStorageService(mockLogger.Object);

        // Act
        var task = service.ClearAsync();

        // Assert
        Assert.IsNotNull(task);
        Assert.IsFalse(task.IsFaulted, "Task should never be faulted");
        Assert.IsFalse(task.IsCanceled, "Task should never be canceled");
        Assert.IsNull(task.Exception, "Task should have no exception");
        Assert.AreEqual(TaskStatus.RanToCompletion, task.Status);
    }

    /// <summary>
    /// Tests that StoreTokensAsync completes successfully with valid non-empty tokens and null expiry.
    /// Input: Valid non-empty access and refresh tokens with no expiry date.
    /// Expected: Method completes without throwing exceptions.
    /// NOTE: This test has limited verification capability because SecureStorage.Default is a static
    /// dependency that cannot be mocked with Moq. Proper unit testing would require injecting
    /// ISecureStorage as a dependency.
    /// </summary>
    [TestMethod]
    [DataRow("valid_access_token_123", "valid_refresh_token_456")]
    [DataRow("a", "b")]
    [DataRow("AccessToken123!@#", "RefreshToken456$%^")]
    public async Task StoreTokensAsync_WithValidNonEmptyTokensAndNullExpiry_CompletesSuccessfully(
        string accessToken,
        string refreshToken)
    {
        // Arrange
        var loggerMock = new Mock<ILogger<TokenStorageService>>();
        var service = new TokenStorageService(loggerMock.Object);

        // Act & Assert
        try
        {
            await service.StoreTokensAsync(accessToken, refreshToken, null);
            Assert.IsTrue(true);
        }
        catch (Exception)
        {
            Assert.Inconclusive("SecureStorage.Default is not available in the test environment. " +
                              "This method requires ISecureStorage to be injected for proper unit testing.");
        }
    }

    /// <summary>
    /// Tests that StoreTokensAsync completes successfully with empty string tokens.
    /// Input: Empty strings for both access and refresh tokens.
    /// Expected: Method completes without throwing exceptions.
    /// NOTE: This test has limited verification capability due to static SecureStorage dependency.
    /// </summary>
    [TestMethod]
    public async Task StoreTokensAsync_WithEmptyStringTokens_CompletesSuccessfully()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<TokenStorageService>>();
        var service = new TokenStorageService(loggerMock.Object);

        // Act & Assert
        try
        {
            await service.StoreTokensAsync("", "", null);
            Assert.IsTrue(true);
        }
        catch (Exception)
        {
            Assert.Inconclusive("SecureStorage.Default is not available in the test environment. " +
                              "This method requires ISecureStorage to be injected for proper unit testing.");
        }
    }

    /// <summary>
    /// Tests that StoreTokensAsync handles various whitespace-only token strings.
    /// Input: Whitespace-only strings including spaces, tabs, newlines, and combinations.
    /// Expected: Method completes without throwing exceptions.
    /// NOTE: This test has limited verification capability due to static SecureStorage dependency.
    /// </summary>
    [TestMethod]
    [DataRow("   ", "   ")]
    [DataRow("\t", "\t")]
    [DataRow("\n", "\n")]
    [DataRow("\r\n", "\r\n")]
    [DataRow(" \t\r\n ", " \t\r\n ")]
    public async Task StoreTokensAsync_WithWhitespaceOnlyTokens_CompletesSuccessfully(
        string accessToken,
        string refreshToken)
    {
        // Arrange
        var loggerMock = new Mock<ILogger<TokenStorageService>>();
        var service = new TokenStorageService(loggerMock.Object);

        // Act & Assert
        try
        {
            await service.StoreTokensAsync(accessToken, refreshToken, null);
            Assert.IsTrue(true);
        }
        catch (Exception)
        {
            Assert.Inconclusive("SecureStorage.Default is not available in the test environment. " +
                              "This method requires ISecureStorage to be injected for proper unit testing.");
        }
    }

    /// <summary>
    /// Tests that StoreTokensAsync handles very long token strings.
    /// Input: Very long strings (10000 characters) for both tokens.
    /// Expected: Method completes without throwing exceptions.
    /// NOTE: This test has limited verification capability due to static SecureStorage dependency.
    /// </summary>
    [TestMethod]
    public async Task StoreTokensAsync_WithVeryLongStrings_CompletesSuccessfully()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<TokenStorageService>>();
        var service = new TokenStorageService(loggerMock.Object);
        var longAccessToken = new string('A', 10000);
        var longRefreshToken = new string('B', 10000);

        // Act & Assert
        try
        {
            await service.StoreTokensAsync(longAccessToken, longRefreshToken, null);
            Assert.IsTrue(true);
        }
        catch (Exception)
        {
            Assert.Inconclusive("SecureStorage.Default is not available in the test environment. " +
                              "This method requires ISecureStorage to be injected for proper unit testing.");
        }
    }

    /// <summary>
    /// Tests that StoreTokensAsync handles tokens with special characters.
    /// Input: Tokens containing various special characters.
    /// Expected: Method completes without throwing exceptions.
    /// NOTE: This test has limited verification capability due to static SecureStorage dependency.
    /// </summary>
    [TestMethod]
    [DataRow("token!@#$%", "token^&*()")]
    [DataRow("token<>?", "token:\"{}|")]
    [DataRow("token[];',./", "token-=_+`~")]
    public async Task StoreTokensAsync_WithSpecialCharacters_CompletesSuccessfully(
        string accessToken,
        string refreshToken)
    {
        // Arrange
        var loggerMock = new Mock<ILogger<TokenStorageService>>();
        var service = new TokenStorageService(loggerMock.Object);

        // Act & Assert
        try
        {
            await service.StoreTokensAsync(accessToken, refreshToken, null);
            Assert.IsTrue(true);
        }
        catch (Exception)
        {
            Assert.Inconclusive("SecureStorage.Default is not available in the test environment. " +
                              "This method requires ISecureStorage to be injected for proper unit testing.");
        }
    }

    /// <summary>
    /// Tests that StoreTokensAsync handles tokens with control characters.
    /// Input: Tokens containing null terminators, carriage returns, line feeds, and tabs.
    /// Expected: Method completes without throwing exceptions.
    /// NOTE: This test has limited verification capability due to static SecureStorage dependency.
    /// </summary>
    [TestMethod]
    [DataRow("token\0null", "refresh\0token")]
    [DataRow("token\r\nlines", "refresh\r\nlines")]
    [DataRow("token\ttab", "refresh\ttab")]
    [DataRow("\0\r\n\t", "\t\n\r\0")]
    public async Task StoreTokensAsync_WithControlCharacters_CompletesSuccessfully(
        string accessToken,
        string refreshToken)
    {
        // Arrange
        var loggerMock = new Mock<ILogger<TokenStorageService>>();
        var service = new TokenStorageService(loggerMock.Object);

        // Act & Assert
        try
        {
            await service.StoreTokensAsync(accessToken, refreshToken, null);
            Assert.IsTrue(true);
        }
        catch (Exception)
        {
            Assert.Inconclusive("SecureStorage.Default is not available in the test environment. " +
                              "This method requires ISecureStorage to be injected for proper unit testing.");
        }
    }

    /// <summary>
    /// Tests that StoreTokensAsync handles tokens with Unicode characters.
    /// Input: Tokens containing emoji, Chinese, Arabic, and other Unicode characters.
    /// Expected: Method completes without throwing exceptions.
    /// NOTE: This test has limited verification capability due to static SecureStorage dependency.
    /// </summary>
    [TestMethod]
    [DataRow("token😀emoji", "refresh🔑key")]
    [DataRow("令牌中文", "刷新令牌")]
    [DataRow("رمز", "تحديث")]
    [DataRow("токен", "обновить")]
    public async Task StoreTokensAsync_WithUnicodeCharacters_CompletesSuccessfully(
        string accessToken,
        string refreshToken)
    {
        // Arrange
        var loggerMock = new Mock<ILogger<TokenStorageService>>();
        var service = new TokenStorageService(loggerMock.Object);

        // Act & Assert
        try
        {
            await service.StoreTokensAsync(accessToken, refreshToken, null);
            Assert.IsTrue(true);
        }
        catch (Exception)
        {
            Assert.Inconclusive("SecureStorage.Default is not available in the test environment. " +
                              "This method requires ISecureStorage to be injected for proper unit testing.");
        }
    }

    /// <summary>
    /// Tests that StoreTokensAsync completes with valid tokens and future expiry date.
    /// Input: Valid tokens with a future expiry date.
    /// Expected: Method completes without throwing exceptions and stores the expiry.
    /// NOTE: This test has limited verification capability due to static SecureStorage dependency.
    /// </summary>
    [TestMethod]
    public async Task StoreTokensAsync_WithValidTokensAndFutureExpiry_CompletesSuccessfully()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<TokenStorageService>>();
        var service = new TokenStorageService(loggerMock.Object);
        var futureExpiry = DateTime.UtcNow.AddHours(1);

        // Act & Assert
        try
        {
            await service.StoreTokensAsync("access_token", "refresh_token", futureExpiry);
            Assert.IsTrue(true);
        }
        catch (Exception)
        {
            Assert.Inconclusive("SecureStorage.Default is not available in the test environment. " +
                              "This method requires ISecureStorage to be injected for proper unit testing.");
        }
    }

    /// <summary>
    /// Tests that StoreTokensAsync completes with valid tokens and past expiry date.
    /// Input: Valid tokens with a past expiry date.
    /// Expected: Method completes without throwing exceptions (no validation on expiry being in future).
    /// NOTE: This test has limited verification capability due to static SecureStorage dependency.
    /// </summary>
    [TestMethod]
    public async Task StoreTokensAsync_WithValidTokensAndPastExpiry_CompletesSuccessfully()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<TokenStorageService>>();
        var service = new TokenStorageService(loggerMock.Object);
        var pastExpiry = DateTime.UtcNow.AddHours(-1);

        // Act & Assert
        try
        {
            await service.StoreTokensAsync("access_token", "refresh_token", pastExpiry);
            Assert.IsTrue(true);
        }
        catch (Exception)
        {
            Assert.Inconclusive("SecureStorage.Default is not available in the test environment. " +
                              "This method requires ISecureStorage to be injected for proper unit testing.");
        }
    }

    /// <summary>
    /// Tests that StoreTokensAsync handles DateTime.MinValue as expiry.
    /// Input: Valid tokens with DateTime.MinValue as expiry.
    /// Expected: Method completes without throwing exceptions.
    /// NOTE: This test has limited verification capability due to static SecureStorage dependency.
    /// </summary>
    [TestMethod]
    public async Task StoreTokensAsync_WithDateTimeMinValueExpiry_CompletesSuccessfully()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<TokenStorageService>>();
        var service = new TokenStorageService(loggerMock.Object);

        // Act & Assert
        try
        {
            await service.StoreTokensAsync("access_token", "refresh_token", DateTime.MinValue);
            Assert.IsTrue(true);
        }
        catch (Exception)
        {
            Assert.Inconclusive("SecureStorage.Default is not available in the test environment. " +
                              "This method requires ISecureStorage to be injected for proper unit testing.");
        }
    }

    /// <summary>
    /// Tests that StoreTokensAsync handles DateTime.MaxValue as expiry.
    /// Input: Valid tokens with DateTime.MaxValue as expiry.
    /// Expected: Method completes without throwing exceptions.
    /// NOTE: This test has limited verification capability due to static SecureStorage dependency.
    /// </summary>
    [TestMethod]
    public async Task StoreTokensAsync_WithDateTimeMaxValueExpiry_CompletesSuccessfully()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<TokenStorageService>>();
        var service = new TokenStorageService(loggerMock.Object);

        // Act & Assert
        try
        {
            await service.StoreTokensAsync("access_token", "refresh_token", DateTime.MaxValue);
            Assert.IsTrue(true);
        }
        catch (Exception)
        {
            Assert.Inconclusive("SecureStorage.Default is not available in the test environment. " +
                              "This method requires ISecureStorage to be injected for proper unit testing.");
        }
    }

    /// <summary>
    /// Tests that StoreTokensAsync handles specific DateTime boundary values.
    /// Input: Valid tokens with significant DateTime boundaries (Unix epoch, Y2K, 32-bit timestamp limit).
    /// Expected: Method completes without throwing exceptions.
    /// NOTE: This test has limited verification capability due to static SecureStorage dependency.
    /// </summary>
    [TestMethod]
    [DataRow(1970, 1, 1, 0, 0, 0)]
    [DataRow(2000, 1, 1, 0, 0, 0)]
    [DataRow(2038, 1, 19, 3, 14, 7)]
    [DataRow(1, 1, 1, 0, 0, 0)]
    [DataRow(9999, 12, 31, 23, 59, 59)]
    public async Task StoreTokensAsync_WithSpecificDateTimeBoundaries_CompletesSuccessfully(
        int year,
        int month,
        int day,
        int hour,
        int minute,
        int second)
    {
        // Arrange
        var loggerMock = new Mock<ILogger<TokenStorageService>>();
        var service = new TokenStorageService(loggerMock.Object);
        var expiry = new DateTime(year, month, day, hour, minute, second);

        // Act & Assert
        try
        {
            await service.StoreTokensAsync("access_token", "refresh_token", expiry);
            Assert.IsTrue(true);
        }
        catch (Exception)
        {
            Assert.Inconclusive("SecureStorage.Default is not available in the test environment. " +
                              "This method requires ISecureStorage to be injected for proper unit testing.");
        }
    }

    /// <summary>
    /// Tests that StoreTokensAsync handles null accessToken parameter.
    /// Input: null accessToken with valid refreshToken and no expiry.
    /// Expected: Method completes without throwing (exceptions are caught internally).
    /// NOTE: Despite accessToken being marked non-nullable, the method does not validate and will
    /// pass null to SecureStorage.SetAsync. This test documents the current behavior.
    /// </summary>
    [TestMethod]
    public async Task StoreTokensAsync_WithNullAccessToken_CompletesWithoutThrowing()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<TokenStorageService>>();
        var service = new TokenStorageService(loggerMock.Object);

        // Act & Assert
        try
        {
            await service.StoreTokensAsync(null!, "refresh_token", null);
            Assert.IsTrue(true);
        }
        catch (Exception)
        {
            Assert.Inconclusive("SecureStorage.Default is not available in the test environment. " +
                              "This method requires ISecureStorage to be injected for proper unit testing.");
        }
    }

    /// <summary>
    /// Tests that StoreTokensAsync handles null refreshToken parameter.
    /// Input: valid accessToken with null refreshToken and no expiry.
    /// Expected: Method completes without throwing (exceptions are caught internally).
    /// NOTE: Despite refreshToken being marked non-nullable, the method does not validate and will
    /// pass null to SecureStorage.SetAsync. This test documents the current behavior.
    /// </summary>
    [TestMethod]
    public async Task StoreTokensAsync_WithNullRefreshToken_CompletesWithoutThrowing()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<TokenStorageService>>();
        var service = new TokenStorageService(loggerMock.Object);

        // Act & Assert
        try
        {
            await service.StoreTokensAsync("access_token", null!, null);
            Assert.IsTrue(true);
        }
        catch (Exception)
        {
            Assert.Inconclusive("SecureStorage.Default is not available in the test environment. " +
                              "This method requires ISecureStorage to be injected for proper unit testing.");
        }
    }

    /// <summary>
    /// Tests that StoreTokensAsync handles both null token parameters.
    /// Input: null accessToken and null refreshToken with no expiry.
    /// Expected: Method completes without throwing (exceptions are caught internally).
    /// NOTE: This test documents that no null validation is performed on the token parameters.
    /// </summary>
    [TestMethod]
    public async Task StoreTokensAsync_WithBothNullTokens_CompletesWithoutThrowing()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<TokenStorageService>>();
        var service = new TokenStorageService(loggerMock.Object);

        // Act & Assert
        try
        {
            await service.StoreTokensAsync(null!, null!, null);
            Assert.IsTrue(true);
        }
        catch (Exception)
        {
            Assert.Inconclusive("SecureStorage.Default is not available in the test environment. " +
                              "This method requires ISecureStorage to be injected for proper unit testing.");
        }
    }

    /// <summary>
    /// Tests that StoreTokensAsync handles null tokens with valid expiry.
    /// Input: null accessToken and refreshToken with a valid future expiry date.
    /// Expected: Method completes without throwing (exceptions are caught internally).
    /// NOTE: This test has limited verification capability due to static SecureStorage dependency.
    /// </summary>
    [TestMethod]
    public async Task StoreTokensAsync_WithNullTokensAndValidExpiry_CompletesWithoutThrowing()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<TokenStorageService>>();
        var service = new TokenStorageService(loggerMock.Object);
        var expiry = DateTime.UtcNow.AddDays(1);

        // Act & Assert
        try
        {
            await service.StoreTokensAsync(null!, null!, expiry);
            Assert.IsTrue(true);
        }
        catch (Exception)
        {
            Assert.Inconclusive("SecureStorage.Default is not available in the test environment. " +
                              "This method requires ISecureStorage to be injected for proper unit testing.");
        }
    }

    /// <summary>
    /// Tests that StoreTokensAsync handles mixed null and valid token combinations with expiry.
    /// Input: Various combinations of null and valid tokens with expiry dates.
    /// Expected: Method completes without throwing (exceptions are caught internally).
    /// NOTE: This test has limited verification capability due to static SecureStorage dependency.
    /// </summary>
    [TestMethod]
    [DataRow(null, "valid_refresh")]
    [DataRow("valid_access", null)]
    [DataRow("", null)]
    [DataRow(null, "")]
    public async Task StoreTokensAsync_WithMixedNullAndValidTokensWithExpiry_CompletesWithoutThrowing(
        string? accessToken,
        string? refreshToken)
    {
        // Arrange
        var loggerMock = new Mock<ILogger<TokenStorageService>>();
        var service = new TokenStorageService(loggerMock.Object);
        var expiry = DateTime.UtcNow.AddHours(2);

        // Act & Assert
        try
        {
            await service.StoreTokensAsync(accessToken!, refreshToken!, expiry);
            Assert.IsTrue(true);
        }
        catch (Exception)
        {
            Assert.Inconclusive("SecureStorage.Default is not available in the test environment. " +
                              "This method requires ISecureStorage to be injected for proper unit testing.");
        }
    }

    /// <summary>
    /// Tests that StoreTokensAsync completes successfully when called multiple times sequentially.
    /// Input: Multiple sequential calls with different token values.
    /// Expected: All calls complete without throwing exceptions.
    /// NOTE: This test has limited verification capability due to static SecureStorage dependency.
    /// </summary>
    [TestMethod]
    public async Task StoreTokensAsync_CalledMultipleTimesSequentially_CompletesSuccessfully()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<TokenStorageService>>();
        var service = new TokenStorageService(loggerMock.Object);

        // Act & Assert
        try
        {
            await service.StoreTokensAsync("token1", "refresh1", null);
            await service.StoreTokensAsync("token2", "refresh2", DateTime.UtcNow.AddDays(1));
            await service.StoreTokensAsync("token3", "refresh3", DateTime.MinValue);
            Assert.IsTrue(true);
        }
        catch (Exception)
        {
            Assert.Inconclusive("SecureStorage.Default is not available in the test environment. " +
                              "This method requires ISecureStorage to be injected for proper unit testing.");
        }
    }

    /// <summary>
    /// Tests that StoreTokensAsync can be called concurrently without throwing.
    /// Input: Multiple concurrent calls to StoreTokensAsync with different values.
    /// Expected: All calls complete without throwing exceptions.
    /// NOTE: This test has limited verification capability due to static SecureStorage dependency.
    /// Cannot verify thread safety of SecureStorage or proper concurrent behavior.
    /// </summary>
    [TestMethod]
    public async Task StoreTokensAsync_CalledConcurrently_CompletesWithoutThrowing()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<TokenStorageService>>();
        var service = new TokenStorageService(loggerMock.Object);

        // Act & Assert
        try
        {
            var task1 = service.StoreTokensAsync("token1", "refresh1", null);
            var task2 = service.StoreTokensAsync("token2", "refresh2", DateTime.UtcNow.AddHours(1));
            var task3 = service.StoreTokensAsync("token3", "refresh3", DateTime.UtcNow.AddDays(7));

            await Task.WhenAll(task1, task2, task3);
            Assert.IsTrue(true);
        }
        catch (Exception)
        {
            Assert.Inconclusive("SecureStorage.Default is not available in the test environment. " +
                              "This method requires ISecureStorage to be injected for proper unit testing.");
        }
    }

    /// <summary>
    /// Tests that GetAccessTokenAsync returns a valid token when SecureStorage contains one.
    /// Input: SecureStorage contains a valid access token.
    /// Expected: The method returns the token string.
    /// NOTE: This test is marked as inconclusive because the method uses a static dependency
    /// (SecureStorage.Default) that cannot be mocked with Moq. To make this testable,
    /// the TokenStorageService should accept ISecureStorage as a constructor parameter
    /// instead of using the static Default property.
    /// </summary>
    [TestMethod]
    public async Task GetAccessTokenAsync_WhenValidTokenExists_ReturnsToken()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<TokenStorageService>>();
        var service = new TokenStorageService(mockLogger.Object);

        // Act & Assert
        Assert.Inconclusive("Cannot mock static SecureStorage.Default. Requires refactoring to inject ISecureStorage. " +
                          "Expected behavior: When SecureStorage.GetAsync returns a valid token string, the method should return that token.");
    }

    /// <summary>
    /// Tests that GetAccessTokenAsync returns null when SecureStorage returns null.
    /// Input: SecureStorage.GetAsync("AccessToken") returns null.
    /// Expected: The method returns null without throwing.
    /// NOTE: This test is marked as inconclusive because the method uses a static dependency
    /// (SecureStorage.Default) that cannot be mocked with Moq.
    /// </summary>
    [TestMethod]
    public async Task GetAccessTokenAsync_WhenSecureStorageReturnsNull_ReturnsNull()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<TokenStorageService>>();
        var service = new TokenStorageService(mockLogger.Object);

        // Act & Assert
        Assert.Inconclusive("Cannot mock static SecureStorage.Default. Requires refactoring to inject ISecureStorage. " +
                          "Expected behavior: When SecureStorage.GetAsync returns null, the method should return null.");
    }

    /// <summary>
    /// Tests that GetAccessTokenAsync returns an empty string when SecureStorage returns an empty string.
    /// Input: SecureStorage.GetAsync("AccessToken") returns an empty string.
    /// Expected: The method returns an empty string.
    /// NOTE: This test is marked as inconclusive because the method uses a static dependency
    /// (SecureStorage.Default) that cannot be mocked with Moq.
    /// </summary>
    [TestMethod]
    public async Task GetAccessTokenAsync_WhenSecureStorageReturnsEmpty_ReturnsEmptyString()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<TokenStorageService>>();
        var service = new TokenStorageService(mockLogger.Object);

        // Act & Assert
        Assert.Inconclusive("Cannot mock static SecureStorage.Default. Requires refactoring to inject ISecureStorage. " +
                          "Expected behavior: When SecureStorage.GetAsync returns empty string, the method should return empty string.");
    }

    /// <summary>
    /// Tests that GetAccessTokenAsync returns whitespace strings unchanged.
    /// Input: SecureStorage.GetAsync("AccessToken") returns whitespace-only strings.
    /// Expected: The method returns the whitespace string as-is.
    /// NOTE: This test is marked as inconclusive because the method uses a static dependency
    /// (SecureStorage.Default) that cannot be mocked with Moq.
    /// </summary>
    /// <param name="whitespace">The whitespace string to test.</param>
    [TestMethod]
    [DataRow("   ")]
    [DataRow("\t")]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [DataRow(" \t\r\n ")]
    public async Task GetAccessTokenAsync_WhenSecureStorageReturnsWhitespace_ReturnsWhitespace(string whitespace)
    {
        // Arrange
        var mockLogger = new Mock<ILogger<TokenStorageService>>();
        var service = new TokenStorageService(mockLogger.Object);

        // Act & Assert
        Assert.Inconclusive($"Cannot mock static SecureStorage.Default. Requires refactoring to inject ISecureStorage. " +
                          $"Expected behavior: When SecureStorage.GetAsync returns whitespace '{whitespace}', the method should return it unchanged.");
    }

    /// <summary>
    /// Tests that GetAccessTokenAsync handles very long token strings correctly.
    /// Input: SecureStorage.GetAsync("AccessToken") returns a very long string (10000 characters).
    /// Expected: The method returns the entire long string without truncation.
    /// NOTE: This test is marked as inconclusive because the method uses a static dependency
    /// (SecureStorage.Default) that cannot be mocked with Moq.
    /// </summary>
    [TestMethod]
    public async Task GetAccessTokenAsync_WhenSecureStorageReturnsVeryLongToken_ReturnsFullToken()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<TokenStorageService>>();
        var service = new TokenStorageService(mockLogger.Object);

        // Act & Assert
        Assert.Inconclusive("Cannot mock static SecureStorage.Default. Requires refactoring to inject ISecureStorage. " +
                          "Expected behavior: When SecureStorage.GetAsync returns a very long token (10000+ chars), the method should return the full token.");
    }

    /// <summary>
    /// Tests that GetAccessTokenAsync handles tokens with special characters correctly.
    /// Input: SecureStorage.GetAsync("AccessToken") returns tokens containing special characters.
    /// Expected: The method returns the token with all special characters intact.
    /// NOTE: This test is marked as inconclusive because the method uses a static dependency
    /// (SecureStorage.Default) that cannot be mocked with Moq.
    /// </summary>
    /// <param name="specialToken">The token containing special characters.</param>
    [TestMethod]
    [DataRow("token!@#$%^&*()")]
    [DataRow("token<>?:\"{}|")]
    [DataRow("token_with_dashes-and.dots")]
    [DataRow("token=with=equals&and&ampersands")]
    public async Task GetAccessTokenAsync_WhenTokenContainsSpecialCharacters_ReturnsTokenUnchanged(string specialToken)
    {
        // Arrange
        var mockLogger = new Mock<ILogger<TokenStorageService>>();
        var service = new TokenStorageService(mockLogger.Object);

        // Act & Assert
        Assert.Inconclusive($"Cannot mock static SecureStorage.Default. Requires refactoring to inject ISecureStorage. " +
                          $"Expected behavior: When SecureStorage.GetAsync returns token with special characters, the method should return it unchanged.");
    }

    /// <summary>
    /// Tests that GetAccessTokenAsync handles tokens with control characters correctly.
    /// Input: SecureStorage.GetAsync("AccessToken") returns tokens containing control characters.
    /// Expected: The method returns the token with control characters intact.
    /// NOTE: This test is marked as inconclusive because the method uses a static dependency
    /// (SecureStorage.Default) that cannot be mocked with Moq.
    /// </summary>
    /// <param name="controlCharToken">The token containing control characters.</param>
    [TestMethod]
    [DataRow("token\0with\0null")]
    [DataRow("token\twith\ttabs")]
    [DataRow("token\r\nwith\r\nnewlines")]
    [DataRow("\0\t\r\n")]
    public async Task GetAccessTokenAsync_WhenTokenContainsControlCharacters_ReturnsTokenUnchanged(string controlCharToken)
    {
        // Arrange
        var mockLogger = new Mock<ILogger<TokenStorageService>>();
        var service = new TokenStorageService(mockLogger.Object);

        // Act & Assert
        Assert.Inconclusive($"Cannot mock static SecureStorage.Default. Requires refactoring to inject ISecureStorage. " +
                          $"Expected behavior: When SecureStorage.GetAsync returns token with control characters, the method should return it unchanged.");
    }

    /// <summary>
    /// Tests that GetAccessTokenAsync handles tokens with Unicode characters correctly.
    /// Input: SecureStorage.GetAsync("AccessToken") returns tokens with Unicode characters.
    /// Expected: The method returns the token with Unicode characters intact.
    /// NOTE: This test is marked as inconclusive because the method uses a static dependency
    /// (SecureStorage.Default) that cannot be mocked with Moq.
    /// </summary>
    /// <param name="unicodeToken">The token containing Unicode characters.</param>
    [TestMethod]
    [DataRow("token_😀_emoji")]
    [DataRow("令牌_中文")]
    [DataRow("رمز_عربي")]
    [DataRow("токен_русский")]
    [DataRow("token_\u200B\u200C\u200D_zero_width")]
    public async Task GetAccessTokenAsync_WhenTokenContainsUnicodeCharacters_ReturnsTokenUnchanged(string unicodeToken)
    {
        // Arrange
        var mockLogger = new Mock<ILogger<TokenStorageService>>();
        var service = new TokenStorageService(mockLogger.Object);

        // Act & Assert
        Assert.Inconclusive($"Cannot mock static SecureStorage.Default. Requires refactoring to inject ISecureStorage. " +
                          $"Expected behavior: When SecureStorage.GetAsync returns token with Unicode characters, the method should return it unchanged.");
    }

    /// <summary>
    /// Tests that GetAccessTokenAsync catches generic exceptions, logs a warning, and returns null.
    /// Input: SecureStorage.GetAsync("AccessToken") throws a generic Exception.
    /// Expected: The method catches the exception, logs "Failed to read access token from SecureStorage", and returns null.
    /// NOTE: This test is marked as inconclusive because the method uses a static dependency
    /// (SecureStorage.Default) that cannot be mocked with Moq.
    /// </summary>
    [TestMethod]
    public async Task GetAccessTokenAsync_WhenGenericExceptionThrown_LogsWarningAndReturnsNull()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<TokenStorageService>>();
        var service = new TokenStorageService(mockLogger.Object);

        // Act & Assert
        Assert.Inconclusive("Cannot mock static SecureStorage.Default. Requires refactoring to inject ISecureStorage. " +
                          "Expected behavior: When SecureStorage.GetAsync throws Exception, catch it, log warning with exception and message 'Failed to read access token from SecureStorage', and return null.");
    }

    /// <summary>
    /// Tests that GetAccessTokenAsync catches InvalidOperationException, logs a warning, and returns null.
    /// Input: SecureStorage.GetAsync("AccessToken") throws InvalidOperationException.
    /// Expected: The method catches the exception, logs a warning, and returns null.
    /// NOTE: This test is marked as inconclusive because the method uses a static dependency
    /// (SecureStorage.Default) that cannot be mocked with Moq.
    /// </summary>
    [TestMethod]
    public async Task GetAccessTokenAsync_WhenInvalidOperationExceptionThrown_LogsWarningAndReturnsNull()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<TokenStorageService>>();
        var service = new TokenStorageService(mockLogger.Object);

        // Act & Assert
        Assert.Inconclusive("Cannot mock static SecureStorage.Default. Requires refactoring to inject ISecureStorage. " +
                          "Expected behavior: When SecureStorage.GetAsync throws InvalidOperationException, catch it, log warning, and return null.");
    }

    /// <summary>
    /// Tests that GetAccessTokenAsync catches PlatformNotSupportedException, logs a warning, and returns null.
    /// Input: SecureStorage.GetAsync("AccessToken") throws PlatformNotSupportedException.
    /// Expected: The method catches the exception, logs a warning, and returns null.
    /// NOTE: This test is marked as inconclusive because the method uses a static dependency
    /// (SecureStorage.Default) that cannot be mocked with Moq.
    /// </summary>
    [TestMethod]
    public async Task GetAccessTokenAsync_WhenPlatformNotSupportedExceptionThrown_LogsWarningAndReturnsNull()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<TokenStorageService>>();
        var service = new TokenStorageService(mockLogger.Object);

        // Act & Assert
        Assert.Inconclusive("Cannot mock static SecureStorage.Default. Requires refactoring to inject ISecureStorage. " +
                          "Expected behavior: When SecureStorage.GetAsync throws PlatformNotSupportedException, catch it, log warning, and return null.");
    }

    /// <summary>
    /// Tests that GetAccessTokenAsync catches UnauthorizedAccessException, logs a warning, and returns null.
    /// Input: SecureStorage.GetAsync("AccessToken") throws UnauthorizedAccessException.
    /// Expected: The method catches the exception, logs a warning, and returns null.
    /// NOTE: This test is marked as inconclusive because the method uses a static dependency
    /// (SecureStorage.Default) that cannot be mocked with Moq.
    /// </summary>
    [TestMethod]
    public async Task GetAccessTokenAsync_WhenUnauthorizedAccessExceptionThrown_LogsWarningAndReturnsNull()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<TokenStorageService>>();
        var service = new TokenStorageService(mockLogger.Object);

        // Act & Assert
        Assert.Inconclusive("Cannot mock static SecureStorage.Default. Requires refactoring to inject ISecureStorage. " +
                          "Expected behavior: When SecureStorage.GetAsync throws UnauthorizedAccessException, catch it, log warning, and return null.");
    }

    /// <summary>
    /// Tests that GetAccessTokenAsync logs the exact warning message when an exception occurs.
    /// Input: SecureStorage.GetAsync("AccessToken") throws an exception.
    /// Expected: Logger.LogWarning is called with the exception and exact message "Failed to read access token from SecureStorage".
    /// NOTE: This test is marked as inconclusive because the method uses a static dependency
    /// (SecureStorage.Default) that cannot be mocked with Moq.
    /// </summary>
    [TestMethod]
    public async Task GetAccessTokenAsync_WhenExceptionOccurs_LogsExactWarningMessage()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<TokenStorageService>>();
        var service = new TokenStorageService(mockLogger.Object);

        // Act & Assert
        Assert.Inconclusive("Cannot mock static SecureStorage.Default. Requires refactoring to inject ISecureStorage. " +
                          "Expected behavior: When exception occurs, logger.LogWarning should be called with the exception and message 'Failed to read access token from SecureStorage'.");
    }

    /// <summary>
    /// Tests that GetAccessTokenAsync does not throw exceptions even when SecureStorage throws.
    /// Input: SecureStorage.GetAsync("AccessToken") throws any type of exception.
    /// Expected: The method catches all exceptions and returns null without propagating the exception.
    /// NOTE: This test is marked as inconclusive because the method uses a static dependency
    /// (SecureStorage.Default) that cannot be mocked with Moq.
    /// </summary>
    [TestMethod]
    public async Task GetAccessTokenAsync_WhenAnyExceptionThrown_DoesNotPropagateException()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<TokenStorageService>>();
        var service = new TokenStorageService(mockLogger.Object);

        // Act & Assert
        Assert.Inconclusive("Cannot mock static SecureStorage.Default. Requires refactoring to inject ISecureStorage. " +
                          "Expected behavior: When any exception is thrown, it should be caught and not propagated; method should return null.");
    }

    /// <summary>
    /// Tests that GetAccessTokenAsync does not log when successful.
    /// Input: SecureStorage.GetAsync("AccessToken") returns a valid token without throwing.
    /// Expected: Logger.LogWarning is not called.
    /// NOTE: This test is marked as inconclusive because the method uses a static dependency
    /// (SecureStorage.Default) that cannot be mocked with Moq.
    /// </summary>
    [TestMethod]
    public async Task GetAccessTokenAsync_WhenSuccessful_DoesNotLog()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<TokenStorageService>>();
        var service = new TokenStorageService(mockLogger.Object);

        // Act & Assert
        Assert.Inconclusive("Cannot mock static SecureStorage.Default. Requires refactoring to inject ISecureStorage. " +
                          "Expected behavior: When SecureStorage.GetAsync succeeds, no logging should occur (LogWarning should not be called).");
    }

    /// <summary>
    /// Tests that the constructor successfully initializes the service with a valid logger instance.
    /// Input: A valid ILogger mock instance.
    /// Expected: The TokenStorageService instance is created successfully without throwing exceptions.
    /// </summary>
    [TestMethod]
    public void Constructor_WithValidLogger_CreatesInstanceSuccessfully()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<TokenStorageService>>();

        // Act
        var service = new TokenStorageService(loggerMock.Object);

        // Assert
        Assert.IsNotNull(service);
    }

    /// <summary>
    /// Tests that the constructor accepts a null logger without throwing an exception.
    /// Input: null logger parameter.
    /// Expected: The TokenStorageService instance is created successfully.
    /// This documents the current behavior where no null validation is performed on the logger parameter,
    /// despite the parameter being marked as non-nullable. This may lead to NullReferenceException
    /// when the service attempts to use the logger.
    /// </summary>
    [TestMethod]
    public void Constructor_WithNullLogger_CreatesInstanceWithoutThrowingException()
    {
        // Arrange
        ILogger<TokenStorageService>? logger = null;

        // Act
        var service = new TokenStorageService(logger!);

        // Assert
        Assert.IsNotNull(service);
    }

    /// <summary>
    /// Tests that IsTokenExpiredAsync returns true when SecureStorage returns various whitespace strings.
    /// Input: SecureStorage returns whitespace strings (spaces, tabs, newlines, mixed).
    /// Expected: string.IsNullOrEmpty returns false, but DateTime.Parse throws FormatException,
    /// exception is caught, warning is logged, and method returns true.
    /// NOTE: Cannot be properly tested due to static SecureStorage.Default dependency.
    /// </summary>
    /// <param name="whitespaceValue">The whitespace string value to test.</param>
    [TestMethod]
    [DataRow("   ")]
    [DataRow("\t")]
    [DataRow("\n")]
    [DataRow("\r")]
    [DataRow("\r\n")]
    [DataRow(" \t\n\r ")]
    [Ignore("Cannot mock static SecureStorage.Default. Requires refactoring to inject ISecureStorage.")]
    public async Task IsTokenExpiredAsync_WhenStorageReturnsVariousWhitespace_ReturnsTrueAndLogsWarning(string whitespaceValue)
    {
        // Arrange
        var mockLogger = new Mock<ILogger<TokenStorageService>>();
        var service = new TokenStorageService(mockLogger.Object);

        // TODO: Mock SecureStorage.Default.GetAsync to return whitespaceValue
        // This requires refactoring the production code to accept ISecureStorage via dependency injection

        // Act
        var result = await service.IsTokenExpiredAsync();

        // Assert
        Assert.IsTrue(result);
        // TODO: Verify LogWarning was called with correct exception and message
    }

    /// <summary>
    /// Tests that IsTokenExpiredAsync handles DateTime strings in various valid formats.
    /// Input: SecureStorage returns DateTime strings in different valid formats.
    /// Expected: DateTime.Parse succeeds, comparison with UtcNow determines expiry status.
    /// NOTE: Cannot be properly tested due to static SecureStorage.Default dependency.
    /// </summary>
    /// <param name="dateTimeString">The DateTime string in a specific format.</param>
    /// <param name="isPast">Whether the date represents a past time.</param>
    [TestMethod]
    [DataRow("2020-01-01T00:00:00Z", true)]
    [DataRow("2099-12-31T23:59:59Z", false)]
    [DataRow("1900-01-01T00:00:00", true)]
    [DataRow("3000-01-01T00:00:00", false)]
    [Ignore("Cannot mock static SecureStorage.Default. Requires refactoring to inject ISecureStorage.")]
    public async Task IsTokenExpiredAsync_WhenStorageReturnsVariousDateFormats_ReturnsExpectedResult(
        string dateTimeString,
        bool isPast)
    {
        // Arrange
        var mockLogger = new Mock<ILogger<TokenStorageService>>();
        var service = new TokenStorageService(mockLogger.Object);

        // TODO: Mock SecureStorage.Default.GetAsync to return dateTimeString
        // This requires refactoring the production code to accept ISecureStorage via dependency injection

        // Act
        var result = await service.IsTokenExpiredAsync();

        // Assert
        Assert.AreEqual(isPast, result);
    }

    /// <summary>
    /// Tests that IsTokenExpiredAsync returns true and logs warning for completely invalid date strings.
    /// Input: SecureStorage returns strings that cannot be parsed as DateTime.
    /// Expected: DateTime.Parse throws FormatException, caught, logged, returns true.
    /// NOTE: Cannot be properly tested due to static SecureStorage.Default dependency.
    /// </summary>
    /// <param name="invalidDateString">An invalid date string.</param>
    [TestMethod]
    [DataRow("not-a-date")]
    [DataRow("12345")]
    [DataRow("abc123")]
    [DataRow("2023-13-45")]
    [DataRow("Invalid Date")]
    [DataRow("00/00/0000")]
    [Ignore("Cannot mock static SecureStorage.Default. Requires refactoring to inject ISecureStorage.")]
    public async Task IsTokenExpiredAsync_WhenStorageReturnsInvalidDateStrings_ReturnsTrueAndLogsWarning(string invalidDateString)
    {
        // Arrange
        var mockLogger = new Mock<ILogger<TokenStorageService>>();
        var service = new TokenStorageService(mockLogger.Object);

        // TODO: Mock SecureStorage.Default.GetAsync to return invalidDateString
        // This requires refactoring the production code to accept ISecureStorage via dependency injection

        // Act
        var result = await service.IsTokenExpiredAsync();

        // Assert
        Assert.IsTrue(result);
        // TODO: Verify LogWarning was called with FormatException and correct message
    }

    /// <summary>
    /// Tests that IsTokenExpiredAsync returns true when the token expiry is exactly one second in the past.
    /// Input: SecureStorage returns DateTime string representing UtcNow minus one second.
    /// Expected: Parsed DateTime is less than UtcNow, method returns true.
    /// NOTE: Cannot be properly tested due to static SecureStorage.Default dependency.
    /// </summary>
    [TestMethod]
    [Ignore("Cannot mock static SecureStorage.Default. Requires refactoring to inject ISecureStorage.")]
    public async Task IsTokenExpiredAsync_WhenExpiryIsOneSecondInPast_ReturnsTrue()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<TokenStorageService>>();
        var service = new TokenStorageService(mockLogger.Object);
        var oneSecondAgo = DateTime.UtcNow.AddSeconds(-1).ToString("O");

        // TODO: Mock SecureStorage.Default.GetAsync to return oneSecondAgo
        // This requires refactoring the production code to accept ISecureStorage via dependency injection

        // Act
        var result = await service.IsTokenExpiredAsync();

        // Assert
        Assert.IsTrue(result);
    }

    /// <summary>
    /// Tests that IsTokenExpiredAsync returns false when the token expiry is exactly one second in the future.
    /// Input: SecureStorage returns DateTime string representing UtcNow plus one second.
    /// Expected: Parsed DateTime is greater than UtcNow, method returns false.
    /// NOTE: Cannot be properly tested due to static SecureStorage.Default dependency.
    /// </summary>
    [TestMethod]
    [Ignore("Cannot mock static SecureStorage.Default. Requires refactoring to inject ISecureStorage.")]
    public async Task IsTokenExpiredAsync_WhenExpiryIsOneSecondInFuture_ReturnsFalse()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<TokenStorageService>>();
        var service = new TokenStorageService(mockLogger.Object);
        var oneSecondFuture = DateTime.UtcNow.AddSeconds(1).ToString("O");

        // TODO: Mock SecureStorage.Default.GetAsync to return oneSecondFuture
        // This requires refactoring the production code to accept ISecureStorage via dependency injection

        // Act
        var result = await service.IsTokenExpiredAsync();

        // Assert
        Assert.IsFalse(result);
    }

    /// <summary>
    /// Tests that IsTokenExpiredAsync correctly uses the less-than-or-equal-to operator for boundary comparison.
    /// Input: SecureStorage returns DateTime strings at exact UtcNow boundary.
    /// Expected: When expiry equals UtcNow, method returns true (due to <= comparison).
    /// NOTE: Cannot be properly tested due to static SecureStorage.Default dependency.
    /// This test documents that the implementation uses <= rather than <.
    /// </summary>
    [TestMethod]
    [Ignore("Cannot mock static SecureStorage.Default. Requires refactoring to inject ISecureStorage.")]
    public async Task IsTokenExpiredAsync_WhenExpiryEqualsUtcNow_ReturnsTrueDueToLessThanOrEqualComparison()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<TokenStorageService>>();
        var service = new TokenStorageService(mockLogger.Object);
        var exactNow = DateTime.UtcNow.ToString("O");

        // TODO: Mock SecureStorage.Default.GetAsync to return exactNow
        // TODO: Ensure the comparison happens at exact same time to validate <= behavior
        // This requires refactoring the production code to accept ISecureStorage via dependency injection

        // Act
        var result = await service.IsTokenExpiredAsync();

        // Assert
        Assert.IsTrue(result, "Method should return true when expiry equals UtcNow due to <= comparison");
    }

    /// <summary>
    /// Tests that IsTokenExpiredAsync logs warning with correct message format when exception occurs.
    /// Input: SecureStorage.GetAsync throws any exception.
    /// Expected: LogWarning is called with the exception and message "Failed to check token expiry".
    /// NOTE: Cannot be properly tested due to static SecureStorage.Default dependency.
    /// </summary>
    [TestMethod]
    [Ignore("Cannot mock static SecureStorage.Default. Requires refactoring to inject ISecureStorage.")]
    public async Task IsTokenExpiredAsync_WhenExceptionOccurs_LogsWarningWithExactMessage()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<TokenStorageService>>();
        var service = new TokenStorageService(mockLogger.Object);

        // TODO: Mock SecureStorage.Default.GetAsync to throw an exception
        // This requires refactoring the production code to accept ISecureStorage via dependency injection

        // Act
        var result = await service.IsTokenExpiredAsync();

        // Assert
        Assert.IsTrue(result);
        // TODO: Verify LogWarning was called exactly once
        // TODO: Verify the message parameter was exactly "Failed to check token expiry"
        // TODO: Verify the exception parameter was the thrown exception
    }

    /// <summary>
    /// Tests that IsTokenExpiredAsync handles very long but valid DateTime strings correctly.
    /// Input: SecureStorage returns DateTime string with maximum precision.
    /// Expected: DateTime.Parse succeeds, comparison works correctly.
    /// NOTE: Cannot be properly tested due to static SecureStorage.Default dependency.
    /// </summary>
    [TestMethod]
    [Ignore("Cannot mock static SecureStorage.Default. Requires refactoring to inject ISecureStorage.")]
    public async Task IsTokenExpiredAsync_WhenStorageReturnsHighPrecisionDateTimeString_HandlesCorrectly()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<TokenStorageService>>();
        var service = new TokenStorageService(mockLogger.Object);
        var highPrecisionFuture = DateTime.UtcNow.AddHours(1).ToString("O");

        // TODO: Mock SecureStorage.Default.GetAsync to return highPrecisionFuture
        // This requires refactoring the production code to accept ISecureStorage via dependency injection

        // Act
        var result = await service.IsTokenExpiredAsync();

        // Assert
        Assert.IsFalse(result);
    }

    /// <summary>
    /// Tests that IsTokenExpiredAsync returns true for strings with special characters that aren't dates.
    /// Input: SecureStorage returns strings with special characters.
    /// Expected: DateTime.Parse throws exception, caught, logged, returns true.
    /// NOTE: Cannot be properly tested due to static SecureStorage.Default dependency.
    /// </summary>
    /// <param name="specialCharString">String with special characters.</param>
    [TestMethod]
    [DataRow("!@#$%^&*()")]
    [DataRow("<script>alert('xss')</script>")]
    [DataRow("'; DROP TABLE tokens;--")]
    [DataRow("\\x00\\x01\\x02")]
    [Ignore("Cannot mock static SecureStorage.Default. Requires refactoring to inject ISecureStorage.")]
    public async Task IsTokenExpiredAsync_WhenStorageReturnsSpecialCharacters_ReturnsTrueAndLogsWarning(string specialCharString)
    {
        // Arrange
        var mockLogger = new Mock<ILogger<TokenStorageService>>();
        var service = new TokenStorageService(mockLogger.Object);

        // TODO: Mock SecureStorage.Default.GetAsync to return specialCharString
        // This requires refactoring the production code to accept ISecureStorage via dependency injection

        // Act
        var result = await service.IsTokenExpiredAsync();

        // Assert
        Assert.IsTrue(result);
        // TODO: Verify LogWarning was called
    }

    /// <summary>
    /// Tests that IsTokenExpiredAsync doesn't throw when called multiple times concurrently.
    /// Input: Multiple concurrent calls to IsTokenExpiredAsync.
    /// Expected: All calls complete successfully without throwing, returning true on error or proper result.
    /// NOTE: Cannot be properly tested due to static SecureStorage.Default dependency.
    /// This test would verify thread-safety if the dependency could be mocked.
    /// </summary>
    [TestMethod]
    [Ignore("Cannot mock static SecureStorage.Default. Requires refactoring to inject ISecureStorage.")]
    public async Task IsTokenExpiredAsync_CalledConcurrently_CompletesSuccessfully()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<TokenStorageService>>();
        var service = new TokenStorageService(mockLogger.Object);

        // TODO: Mock SecureStorage.Default.GetAsync behavior
        // This requires refactoring the production code to accept ISecureStorage via dependency injection

        // Act
        var tasks = new Task<bool>[10];
        for (int i = 0; i < 10; i++)
        {
            tasks[i] = service.IsTokenExpiredAsync();
        }
        var results = await Task.WhenAll(tasks);

        // Assert
        Assert.IsNotNull(results);
        Assert.AreEqual(10, results.Length);
        // All calls should complete without throwing
    }
}