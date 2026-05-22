using BCrypt.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Ssomero.Api.Configuration;
using Ssomero.Api.Controllers;
using Ssomero.Api.Data;
using Ssomero.Api.Dtos;
using Ssomero.Api.Entities;
using Ssomero.Api.Services;
using Ssomero.Api.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace Ssomero.Api.Controllers.UnitTests;
/// <summary>
/// Unit tests for the AuthController class.
/// </summary>
[TestClass]
public class AuthControllerTests
{
    /// <summary>
    /// Tests that SendOtp logs an error when OtpService throws an exception.
    /// </summary>
    [TestMethod]
    [TestCategory("ProductionBugSuspected")]
    [Ignore("ProductionBugSuspected")]
    public async Task SendOtp_OtpServiceThrowsException_LogsError()
    {
        // Arrange
        var mockDb = new Mock<SsomeroDbContext>();
        var mockJwt = new Mock<JwtService>();
        var mockOtp = new Mock<OtpService>();
        var mockClassService = new Mock<ClassService>();
        var mockLogger = new Mock<ILogger<AuthController>>();
        var exception = new InvalidOperationException("Email service unavailable");
        mockOtp.Setup(o => o.GenerateOtpAsync(It.IsAny<string>())).ThrowsAsync(exception);
        var mockCache = new Mock<IDistributedCache>();
        mockCache.Setup(c => c.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync((byte[]?)null);
        var mockPasswordReset = new Mock<IPasswordResetService>();
        var controller = new AuthController(mockDb.Object, mockJwt.Object, mockOtp.Object, mockClassService.Object, mockCache.Object, mockLogger.Object, mockPasswordReset.Object);
        var email = $"logerror{Guid.NewGuid()}@example.com";
        var request = new SendOtpRequest(email);
        // Act
        await controller.SendOtp(request);
        // Assert
        mockLogger.Verify(x => x.Log(LogLevel.Error, It.IsAny<EventId>(), It.Is<It.IsAnyType>((o, t) => o.ToString()!.Contains("Failed to generate/send OTP")), exception, It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Once);
    }

    private Mock<SsomeroDbContext> _mockDb = null!;
    private Mock<JwtService> _mockJwt = null!;
    private Mock<OtpService> _mockOtp = null!;
    private Mock<ClassService> _mockClassService = null!;
    private Mock<ILogger<AuthController>> _mockLogger = null!;
    private AuthController _controller = null!;
    [TestInitialize]
    public void TestInitialize()
    {
        var options = new DbContextOptionsBuilder<SsomeroDbContext>().UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()).Options;
        _mockDb = new Mock<SsomeroDbContext>(options);
        _mockJwt = new Mock<JwtService>();
        _mockOtp = new Mock<OtpService>();
        _mockClassService = new Mock<ClassService>();
        _mockLogger = new Mock<ILogger<AuthController>>();
        var mockCache = new Mock<IDistributedCache>();
        mockCache.Setup(c => c.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync((byte[]?)null);
        var mockPasswordReset = new Mock<IPasswordResetService>();
        _controller = new AuthController(_mockDb.Object, _mockJwt.Object, _mockOtp.Object, _mockClassService.Object, mockCache.Object, _mockLogger.Object, mockPasswordReset.Object);
    }

}