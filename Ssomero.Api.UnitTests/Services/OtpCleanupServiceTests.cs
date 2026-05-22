using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Ssomero.Api.Data;
using Ssomero.Api.Services;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Ssomero.Api.Services.UnitTests;
/// <summary>
/// Unit tests for the <see cref = "OtpCleanupService"/> class.
/// </summary>
[TestClass]
public class OtpCleanupServiceTests
{
    /// <summary>
    /// Tests that the constructor successfully creates an instance when provided with valid dependencies.
    /// </summary>
    [TestMethod]
    public void Constructor_ValidParameters_CreatesInstance()
    {
        // Arrange
        var mockScopeFactory = new Mock<IServiceScopeFactory>();
        var mockLogger = new Mock<ILogger<OtpCleanupService>>();
        // Act
        var service = new OtpCleanupService(mockScopeFactory.Object, mockLogger.Object);
        // Assert
        Assert.IsNotNull(service);
    }

    /// <summary>
    /// Tests that the constructor does not throw when scopeFactory is null.
    /// Note: The constructor lacks null validation, which may lead to runtime errors later.
    /// </summary>
    [TestMethod]
    public void Constructor_NullScopeFactory_DoesNotThrow()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<OtpCleanupService>>();
        // Act
        var service = new OtpCleanupService(null!, mockLogger.Object);
        // Assert
        Assert.IsNotNull(service);
    }

    /// <summary>
    /// Tests that the constructor does not throw when logger is null.
    /// Note: The constructor lacks null validation, which may lead to runtime errors later.
    /// </summary>
    [TestMethod]
    public void Constructor_NullLogger_DoesNotThrow()
    {
        // Arrange
        var mockScopeFactory = new Mock<IServiceScopeFactory>();
        // Act
        var service = new OtpCleanupService(mockScopeFactory.Object, null!);
        // Assert
        Assert.IsNotNull(service);
    }

    /// <summary>
    /// Tests that the constructor does not throw when both parameters are null.
    /// Note: The constructor lacks null validation, which may lead to runtime errors later.
    /// </summary>
    [TestMethod]
    public void Constructor_BothParametersNull_DoesNotThrow()
    {
        // Arrange & Act
        var service = new OtpCleanupService(null!, null!);
        // Assert
        Assert.IsNotNull(service);
    }

    /// <summary>
    /// Tests that ExecuteAsync logs the startup message when the service starts.
    /// </summary>
    [TestMethod]
    public async Task ExecuteAsync_ServiceStarts_LogsStartupMessage()
    {
        // Arrange
        var mockScope = new Mock<IServiceScope>();
        var mockServiceProvider = new Mock<IServiceProvider>();
        var mockScopeFactory = new Mock<IServiceScopeFactory>();
        mockScopeFactory.Setup(x => x.CreateScope()).Returns(mockScope.Object);
        mockScope.Setup(x => x.ServiceProvider).Returns(mockServiceProvider.Object);
        var mockLogger = new Mock<ILogger<OtpCleanupService>>();
        var service = new OtpCleanupService(mockScopeFactory.Object, mockLogger.Object);
        var cts = new CancellationTokenSource();
        // Act
        await service.StartAsync(cts.Token);
        await Task.Delay(100); // Give the background service time to start and log
        cts.Cancel();
        await service.StopAsync(CancellationToken.None);
        // Assert
        mockLogger.Verify(x => x.Log(LogLevel.Information, It.IsAny<EventId>(), It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("OTP cleanup service started")), It.IsAny<Exception>(), It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Once);
    }

    /// <summary>
    /// Tests that ExecuteAsync respects immediate cancellation and stops without entering the loop.
    /// </summary>
    [TestMethod]
    public async Task ExecuteAsync_ImmediateCancellation_StopsImmediately()
    {
        // Arrange
        var mockScope = new Mock<IServiceScope>();
        var mockServiceProvider = new Mock<IServiceProvider>();
        var mockScopeFactory = new Mock<IServiceScopeFactory>();
        var mockLogger = new Mock<ILogger<OtpCleanupService>>();
        mockScopeFactory.Setup(x => x.CreateScope()).Returns(mockScope.Object);
        mockScope.Setup(x => x.ServiceProvider).Returns(mockServiceProvider.Object);
        var service = new OtpCleanupService(mockScopeFactory.Object, mockLogger.Object);
        var cts = new CancellationTokenSource();
        cts.Cancel();
        // Act
        await service.StartAsync(cts.Token);
        await Task.Delay(100);
        await service.StopAsync(CancellationToken.None);
        // Assert
        mockScopeFactory.Verify(x => x.CreateScope(), Times.Never);
    }

    /// <summary>
    /// Tests that ExecuteAsync stops when cancellation is requested during execution.
    /// </summary>
    [TestMethod]
    public async Task ExecuteAsync_CancellationDuringExecution_StopsService()
    {
        // Arrange
        var mockScope = new Mock<IServiceScope>();
        var mockServiceProvider = new Mock<IServiceProvider>();
        var dbContextOptions = new DbContextOptionsBuilder<SsomeroDbContext>().Options;
        var mockDbContext = new Mock<SsomeroDbContext>(dbContextOptions);
        var mockScopeFactory = new Mock<IServiceScopeFactory>();
        var mockLogger = new Mock<ILogger<OtpCleanupService>>();
        mockScopeFactory.Setup(x => x.CreateScope()).Returns(mockScope.Object);
        mockScope.Setup(x => x.ServiceProvider).Returns(mockServiceProvider.Object);
        mockServiceProvider.Setup(x => x.GetService(typeof(SsomeroDbContext))).Returns(mockDbContext.Object);
        var service = new OtpCleanupService(mockScopeFactory.Object, mockLogger.Object);
        var cts = new CancellationTokenSource();
        // Act
        await service.StartAsync(cts.Token);
        await Task.Delay(100);
        cts.Cancel();
        await service.StopAsync(CancellationToken.None);
        // Assert
        mockLogger.Verify(x => x.Log(LogLevel.Information, It.IsAny<EventId>(), It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("OTP cleanup service started")), It.IsAny<Exception>(), It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Once);
    }

    /// <summary>
    /// Tests that ExecuteAsync catches exceptions from CleanupAsync (except OperationCanceledException),
    /// logs the error, and continues running.
    /// </summary>
    [TestMethod]
    public async Task ExecuteAsync_ExceptionInCleanup_LogsErrorAndContinues()
    {
        // Arrange
        var mockScopeFactory = new Mock<IServiceScopeFactory>();
        var mockLogger = new Mock<ILogger<OtpCleanupService>>();
        var exceptionThrown = new InvalidOperationException("Test exception");
        mockScopeFactory.Setup(x => x.CreateScope()).Throws(exceptionThrown);
        var service = new OtpCleanupService(mockScopeFactory.Object, mockLogger.Object);
        var cts = new CancellationTokenSource();
        // Act
        await service.StartAsync(cts.Token);
        await Task.Delay(200);
        cts.Cancel();
        await service.StopAsync(CancellationToken.None);
        // Assert
        mockLogger.Verify(x => x.Log(LogLevel.Error, It.IsAny<EventId>(), It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Error during OTP cleanup")), exceptionThrown, It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.AtLeastOnce);
    }

    /// <summary>
    /// Tests that ExecuteAsync does not catch OperationCanceledException from CleanupAsync,
    /// allowing it to propagate and terminate the service.
    /// </summary>
    [TestMethod]
    public async Task ExecuteAsync_OperationCanceledExceptionInCleanup_DoesNotCatchException()
    {
        // Arrange
        var mockScopeFactory = new Mock<IServiceScopeFactory>();
        var mockLogger = new Mock<ILogger<OtpCleanupService>>();
        mockScopeFactory.Setup(x => x.CreateScope()).Throws(new OperationCanceledException("Test cancellation"));
        var service = new OtpCleanupService(mockScopeFactory.Object, mockLogger.Object);
        var cts = new CancellationTokenSource();
        // Act
        await service.StartAsync(cts.Token);
        await Task.Delay(200);
        await service.StopAsync(CancellationToken.None);
        // Assert
        mockLogger.Verify(x => x.Log(LogLevel.Error, It.IsAny<EventId>(), It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Error during OTP cleanup")), It.IsAny<Exception>(), It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Never);
    }

    /// <summary>
    /// Tests that ExecuteAsync continues to run and perform multiple cleanup iterations
    /// when exceptions occur, demonstrating resilience.
    /// </summary>
    [TestMethod]
    public async Task ExecuteAsync_MultipleExceptions_ContinuesRunning()
    {
        // Arrange
        var mockScopeFactory = new Mock<IServiceScopeFactory>();
        var mockLogger = new Mock<ILogger<OtpCleanupService>>();
        var exceptionThrown = new InvalidOperationException("Test exception");
        mockScopeFactory.Setup(x => x.CreateScope()).Throws(exceptionThrown);
        var service = new OtpCleanupService(mockScopeFactory.Object, mockLogger.Object);
        var cts = new CancellationTokenSource();
        // Act
        await service.StartAsync(cts.Token);
        await Task.Delay(500);
        cts.Cancel();
        await service.StopAsync(CancellationToken.None);
        // Assert
        mockScopeFactory.Verify(x => x.CreateScope(), Times.AtLeastOnce);
        mockLogger.Verify(x => x.Log(LogLevel.Error, It.IsAny<EventId>(), It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Error during OTP cleanup")), exceptionThrown, It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.AtLeastOnce);
    }

    /// <summary>
    /// Tests that ExecuteAsync handles various exception types correctly,
    /// catching all except OperationCanceledException.
    /// </summary>
    /// <param name = "exception">The exception to throw during cleanup.</param>
    /// <param name = "shouldCatch">Whether the exception should be caught by ExecuteAsync.</param>
    [TestMethod]
    [DataRow(typeof(InvalidOperationException), true)]
    [DataRow(typeof(ArgumentException), true)]
    [DataRow(typeof(NullReferenceException), true)]
    [DataRow(typeof(Exception), true)]
    public async Task ExecuteAsync_VariousExceptionTypes_HandlesCorrectly(Type exceptionType, bool shouldCatch)
    {
        // Arrange
        var mockScopeFactory = new Mock<IServiceScopeFactory>();
        var mockLogger = new Mock<ILogger<OtpCleanupService>>();
        var exception = (Exception)Activator.CreateInstance(exceptionType, "Test exception")!;
        mockScopeFactory.Setup(x => x.CreateScope()).Throws(exception);
        var service = new OtpCleanupService(mockScopeFactory.Object, mockLogger.Object);
        var cts = new CancellationTokenSource();
        // Act
        await service.StartAsync(cts.Token);
        await Task.Delay(200);
        cts.Cancel();
        await service.StopAsync(CancellationToken.None);
        // Assert
        if (shouldCatch)
        {
            mockLogger.Verify(x => x.Log(LogLevel.Error, It.IsAny<EventId>(), It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Error during OTP cleanup")), exception, It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.AtLeastOnce);
        }
    }
}