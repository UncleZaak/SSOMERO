using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Storage;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Ssomero;
using Ssomero.Interfaces;
using Ssomero.Models;
using Ssomero.Services;

namespace Ssomero.Services.UnitTests;




/// <summary>
/// Unit tests for the AuthService class.
/// </summary>
[TestClass]
public class AuthServiceTests
{
    /// <summary>
    /// Tests that RegisterStudentAsync returns true when the API call is successful.
    /// </summary>
    [TestMethod]
    public async Task RegisterStudentAsync_SuccessfulApiResponse_ReturnsTrue()
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockTokenStorage = new Mock<TokenStorageService>();
        var mockSession = new Mock<SessionService>();
        var mockLogger = new Mock<ILogger<AuthService>>();

        var dto = new StudentRegisterDto
        {
            Email = "test@example.com",
            FirstName = "John",
            SecondName = "Doe"
        };

        var successResponse = new HttpResponseMessage(HttpStatusCode.OK);
        mockApi.Setup(x => x.PostAsync("auth/register", It.IsAny<HttpContent>(), It.IsAny<CancellationToken>()))
               .ReturnsAsync(successResponse);

        var authService = new AuthService(mockApi.Object, mockTokenStorage.Object, mockSession.Object, null!, Mock.Of<ICacheService>(), mockLogger.Object);

        // Act
        var result = await authService.RegisterStudentAsync(dto);

        // Assert
        Assert.IsTrue(result);
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Student registration for")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
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
    /// Tests that RegisterStudentAsync returns false when the API call fails with various status codes.
    /// </summary>
    /// <param name="statusCode">The HTTP status code returned by the API.</param>
    [TestMethod]
    [DataRow(400)] // Bad Request
    [DataRow(401)] // Unauthorized
    [DataRow(403)] // Forbidden
    [DataRow(404)] // Not Found
    [DataRow(500)] // Internal Server Error
    [DataRow(503)] // Service Unavailable
    public async Task RegisterStudentAsync_FailedApiResponse_ReturnsFalse(int statusCode)
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockTokenStorage = new Mock<TokenStorageService>();
        var mockSession = new Mock<SessionService>();
        var mockLogger = new Mock<ILogger<AuthService>>();

        var dto = new StudentRegisterDto
        {
            Email = "test@example.com",
            FirstName = "John",
            SecondName = "Doe"
        };

        var failureResponse = new HttpResponseMessage((HttpStatusCode)statusCode);
        mockApi.Setup(x => x.PostAsync("auth/register", It.IsAny<HttpContent>(), It.IsAny<CancellationToken>()))
               .ReturnsAsync(failureResponse);

        var authService = new AuthService(mockApi.Object, mockTokenStorage.Object, mockSession.Object, null!, Mock.Of<ICacheService>(), mockLogger.Object);

        // Act
        var result = await authService.RegisterStudentAsync(dto);

        // Assert
        Assert.IsFalse(result);
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Student registration for")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Student registration failed")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    /// <summary>
    /// Tests that RegisterStudentAsync correctly calls the API with the provided DTO.
    /// </summary>
    [TestMethod]
    public async Task RegisterStudentAsync_ValidDto_CallsApiWithCorrectParameters()
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockTokenStorage = new Mock<TokenStorageService>();
        var mockSession = new Mock<SessionService>();
        var mockLogger = new Mock<ILogger<AuthService>>();

        var dto = new StudentRegisterDto
        {
            Email = "student@university.edu",
            FirstName = "Jane",
            SecondName = "Smith",
            Password = "SecurePass123!",
            UniversityId = "UNI001",
            YearOfStudy = 2
        };

        var successResponse = new HttpResponseMessage(HttpStatusCode.Created);
        mockApi.Setup(x => x.PostAsync("auth/register", It.IsAny<HttpContent>(), It.IsAny<CancellationToken>()))
               .ReturnsAsync(successResponse);

        var authService = new AuthService(mockApi.Object, mockTokenStorage.Object, mockSession.Object, null!, Mock.Of<ICacheService>(), mockLogger.Object);

        // Act
        var result = await authService.RegisterStudentAsync(dto);

        // Assert
        Assert.IsTrue(result);
        mockApi.Verify(
            x => x.PostAsync(
                "auth/register",
                It.IsAny<JsonContent>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    /// <summary>
    /// Tests that RegisterStudentAsync handles DTO with empty email string.
    /// </summary>
    [TestMethod]
    public async Task RegisterStudentAsync_EmptyEmail_LogsAndProcessesNormally()
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockTokenStorage = new Mock<TokenStorageService>();
        var mockSession = new Mock<SessionService>();
        var mockLogger = new Mock<ILogger<AuthService>>();

        var dto = new StudentRegisterDto
        {
            Email = string.Empty,
            FirstName = "John",
            SecondName = "Doe"
        };

        var successResponse = new HttpResponseMessage(HttpStatusCode.OK);
        mockApi.Setup(x => x.PostAsync("auth/register", It.IsAny<HttpContent>(), It.IsAny<CancellationToken>()))
               .ReturnsAsync(successResponse);

        var authService = new AuthService(mockApi.Object, mockTokenStorage.Object, mockSession.Object, null!, Mock.Of<ICacheService>(), mockLogger.Object);

        // Act
        var result = await authService.RegisterStudentAsync(dto);

        // Assert
        Assert.IsTrue(result);
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    /// <summary>
    /// Tests that RegisterStudentAsync handles DTO with special characters in email.
    /// </summary>
    [TestMethod]
    [DataRow("test+tag@example.com")]
    [DataRow("user.name@sub.domain.com")]
    [DataRow("test@example-domain.co.uk")]
    [DataRow("user_123@example.com")]
    public async Task RegisterStudentAsync_EmailWithSpecialCharacters_ProcessesSuccessfully(string email)
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockTokenStorage = new Mock<TokenStorageService>();
        var mockSession = new Mock<SessionService>();
        var mockLogger = new Mock<ILogger<AuthService>>();

        var dto = new StudentRegisterDto
        {
            Email = email,
            FirstName = "Test",
            SecondName = "User"
        };

        var successResponse = new HttpResponseMessage(HttpStatusCode.OK);
        mockApi.Setup(x => x.PostAsync("auth/register", It.IsAny<HttpContent>(), It.IsAny<CancellationToken>()))
               .ReturnsAsync(successResponse);

        var authService = new AuthService(mockApi.Object, mockTokenStorage.Object, mockSession.Object, null!, Mock.Of<ICacheService>(), mockLogger.Object);

        // Act
        var result = await authService.RegisterStudentAsync(dto);

        // Assert
        Assert.IsTrue(result);
    }

    /// <summary>
    /// Tests that RegisterStudentAsync handles DTO with very long email string.
    /// </summary>
    [TestMethod]
    public async Task RegisterStudentAsync_VeryLongEmail_ProcessesSuccessfully()
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockTokenStorage = new Mock<TokenStorageService>();
        var mockSession = new Mock<SessionService>();
        var mockLogger = new Mock<ILogger<AuthService>>();

        var longEmail = new string('a', 250) + "@example.com";
        var dto = new StudentRegisterDto
        {
            Email = longEmail,
            FirstName = "Test",
            SecondName = "User"
        };

        var successResponse = new HttpResponseMessage(HttpStatusCode.OK);
        mockApi.Setup(x => x.PostAsync("auth/register", It.IsAny<HttpContent>(), It.IsAny<CancellationToken>()))
               .ReturnsAsync(successResponse);

        var authService = new AuthService(mockApi.Object, mockTokenStorage.Object, mockSession.Object, null!, Mock.Of<ICacheService>(), mockLogger.Object);

        // Act
        var result = await authService.RegisterStudentAsync(dto);

        // Assert
        Assert.IsTrue(result);
    }

    /// <summary>
    /// Tests that RegisterStudentAsync handles DTO with whitespace-only email.
    /// </summary>
    [TestMethod]
    public async Task RegisterStudentAsync_WhitespaceEmail_ProcessesSuccessfully()
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockTokenStorage = new Mock<TokenStorageService>();
        var mockSession = new Mock<SessionService>();
        var mockLogger = new Mock<ILogger<AuthService>>();

        var dto = new StudentRegisterDto
        {
            Email = "   ",
            FirstName = "Test",
            SecondName = "User"
        };

        var successResponse = new HttpResponseMessage(HttpStatusCode.OK);
        mockApi.Setup(x => x.PostAsync("auth/register", It.IsAny<HttpContent>(), It.IsAny<CancellationToken>()))
               .ReturnsAsync(successResponse);

        var authService = new AuthService(mockApi.Object, mockTokenStorage.Object, mockSession.Object, null!, Mock.Of<ICacheService>(), mockLogger.Object);

        // Act
        var result = await authService.RegisterStudentAsync(dto);

        // Assert
        Assert.IsTrue(result);
    }

    /// <summary>
    /// Tests that RegisterStudentAsync handles boundary value for YearOfStudy (minimum integer).
    /// </summary>
    [TestMethod]
    public async Task RegisterStudentAsync_YearOfStudyMinValue_ProcessesSuccessfully()
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockTokenStorage = new Mock<TokenStorageService>();
        var mockSession = new Mock<SessionService>();
        var mockLogger = new Mock<ILogger<AuthService>>();

        var dto = new StudentRegisterDto
        {
            Email = "test@example.com",
            YearOfStudy = int.MinValue
        };

        var successResponse = new HttpResponseMessage(HttpStatusCode.OK);
        mockApi.Setup(x => x.PostAsync("auth/register", It.IsAny<HttpContent>(), It.IsAny<CancellationToken>()))
               .ReturnsAsync(successResponse);

        var authService = new AuthService(mockApi.Object, mockTokenStorage.Object, mockSession.Object, null!, Mock.Of<ICacheService>(), mockLogger.Object);

        // Act
        var result = await authService.RegisterStudentAsync(dto);

        // Assert
        Assert.IsTrue(result);
    }

    /// <summary>
    /// Tests that RegisterStudentAsync handles boundary value for YearOfStudy (maximum integer).
    /// </summary>
    [TestMethod]
    public async Task RegisterStudentAsync_YearOfStudyMaxValue_ProcessesSuccessfully()
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockTokenStorage = new Mock<TokenStorageService>();
        var mockSession = new Mock<SessionService>();
        var mockLogger = new Mock<ILogger<AuthService>>();

        var dto = new StudentRegisterDto
        {
            Email = "test@example.com",
            YearOfStudy = int.MaxValue
        };

        var successResponse = new HttpResponseMessage(HttpStatusCode.OK);
        mockApi.Setup(x => x.PostAsync("auth/register", It.IsAny<HttpContent>(), It.IsAny<CancellationToken>()))
               .ReturnsAsync(successResponse);

        var authService = new AuthService(mockApi.Object, mockTokenStorage.Object, mockSession.Object, null!, Mock.Of<ICacheService>(), mockLogger.Object);

        // Act
        var result = await authService.RegisterStudentAsync(dto);

        // Assert
        Assert.IsTrue(result);
    }

    /// <summary>
    /// Tests that RegisterStudentAsync handles negative YearOfStudy value.
    /// </summary>
    [TestMethod]
    public async Task RegisterStudentAsync_NegativeYearOfStudy_ProcessesSuccessfully()
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockTokenStorage = new Mock<TokenStorageService>();
        var mockSession = new Mock<SessionService>();
        var mockLogger = new Mock<ILogger<AuthService>>();

        var dto = new StudentRegisterDto
        {
            Email = "test@example.com",
            YearOfStudy = -1
        };

        var successResponse = new HttpResponseMessage(HttpStatusCode.OK);
        mockApi.Setup(x => x.PostAsync("auth/register", It.IsAny<HttpContent>(), It.IsAny<CancellationToken>()))
               .ReturnsAsync(successResponse);

        var authService = new AuthService(mockApi.Object, mockTokenStorage.Object, mockSession.Object, null!, Mock.Of<ICacheService>(), mockLogger.Object);

        // Act
        var result = await authService.RegisterStudentAsync(dto);

        // Assert
        Assert.IsTrue(result);
    }

    /// <summary>
    /// Tests that RegisterStudentAsync handles zero YearOfStudy value.
    /// </summary>
    [TestMethod]
    public async Task RegisterStudentAsync_ZeroYearOfStudy_ProcessesSuccessfully()
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockTokenStorage = new Mock<TokenStorageService>();
        var mockSession = new Mock<SessionService>();
        var mockLogger = new Mock<ILogger<AuthService>>();

        var dto = new StudentRegisterDto
        {
            Email = "test@example.com",
            YearOfStudy = 0
        };

        var successResponse = new HttpResponseMessage(HttpStatusCode.OK);
        mockApi.Setup(x => x.PostAsync("auth/register", It.IsAny<HttpContent>(), It.IsAny<CancellationToken>()))
               .ReturnsAsync(successResponse);

        var authService = new AuthService(mockApi.Object, mockTokenStorage.Object, mockSession.Object, null!, Mock.Of<ICacheService>(), mockLogger.Object);

        // Act
        var result = await authService.RegisterStudentAsync(dto);

        // Assert
        Assert.IsTrue(result);
    }

    /// <summary>
    /// Tests that the AuthService constructor successfully creates an instance
    /// when provided with all valid, non-null dependencies.
    /// </summary>
    [TestMethod]
    public void Constructor_ValidParameters_CreatesInstance()
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockTokenStorageLogger = new Mock<ILogger<TokenStorageService>>();
        var tokenStorage = new TokenStorageService(mockTokenStorageLogger.Object);
        var session = new SessionService();
        var mockLogger = new Mock<ILogger<AuthService>>();

        // Act
        var authService = new AuthService(mockApi.Object, tokenStorage, session, null!, Mock.Of<ICacheService>(), mockLogger.Object);

        // Assert
        Assert.IsNotNull(authService);
    }

    /// <summary>
    /// Tests that the AuthService constructor does not throw an exception
    /// when the api parameter is null.
    /// </summary>
    [TestMethod]
    public void Constructor_NullApi_DoesNotThrow()
    {
        // Arrange
        var mockTokenStorageLogger = new Mock<ILogger<TokenStorageService>>();
        var tokenStorage = new TokenStorageService(mockTokenStorageLogger.Object);
        var session = new SessionService();
        var mockLogger = new Mock<ILogger<AuthService>>();

        // Act
        var authService = new AuthService(null!, tokenStorage, session, null!, Mock.Of<ICacheService>(), mockLogger.Object);

        // Assert
        Assert.IsNotNull(authService);
    }

    /// <summary>
    /// Tests that the AuthService constructor does not throw an exception
    /// when the tokenStorage parameter is null.
    /// </summary>
    [TestMethod]
    public void Constructor_NullTokenStorage_DoesNotThrow()
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var session = new SessionService();
        var mockLogger = new Mock<ILogger<AuthService>>();

        // Act
        var authService = new AuthService(mockApi.Object, null!, session, null!, Mock.Of<ICacheService>(), mockLogger.Object);

        // Assert
        Assert.IsNotNull(authService);
    }

    /// <summary>
    /// Tests that the AuthService constructor does not throw an exception
    /// when the session parameter is null.
    /// </summary>
    [TestMethod]
    public void Constructor_NullSession_DoesNotThrow()
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockTokenStorageLogger = new Mock<ILogger<TokenStorageService>>();
        var tokenStorage = new TokenStorageService(mockTokenStorageLogger.Object);
        var mockLogger = new Mock<ILogger<AuthService>>();

        // Act
        var authService = new AuthService(mockApi.Object, tokenStorage, null!, null!, Mock.Of<ICacheService>(), mockLogger.Object);

        // Assert
        Assert.IsNotNull(authService);
    }

    /// <summary>
    /// Tests that the AuthService constructor does not throw an exception
    /// when the logger parameter is null.
    /// </summary>
    [TestMethod]
    public void Constructor_NullLogger_DoesNotThrow()
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockTokenStorageLogger = new Mock<ILogger<TokenStorageService>>();
        var tokenStorage = new TokenStorageService(mockTokenStorageLogger.Object);
        var session = new SessionService();

        // Act
        var authService = new AuthService(mockApi.Object, tokenStorage, session, null!, Mock.Of<ICacheService>(), null!);

        // Assert
        Assert.IsNotNull(authService);
    }

    /// <summary>
    /// Tests that the AuthService constructor does not throw an exception
    /// when all parameters are null.
    /// </summary>
    [TestMethod]
    public void Constructor_AllParametersNull_DoesNotThrow()
    {
        // Arrange & Act
        var authService = new AuthService(null!, null!, null!, null!, Mock.Of<ICacheService>(), null!);

        // Assert
        Assert.IsNotNull(authService);
    }

    /// <summary>
    /// Tests that LoginAsync successfully logs in with valid credentials,
    /// stores tokens, and returns the AuthResponseDto.
    /// </summary>
    [TestMethod]
    public async Task LoginAsync_ValidCredentials_ReturnsAuthResponseDtoAndStoresTokens()
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockTokenStorage = new Mock<TokenStorageService>();
        var mockSession = new Mock<SessionService>();
        var mockLogger = new Mock<ILogger<AuthService>>();

        var authService = new AuthService(mockApi.Object, mockTokenStorage.Object, mockSession.Object, null!, Mock.Of<ICacheService>(), mockLogger.Object);

        var email = "test@example.com";
        var password = "Password123!";
        var expectedDto = new AuthResponseDto
        {
            AccessToken = "access_token_value",
            RefreshToken = "refresh_token_value",
            ExpiresAt = DateTime.UtcNow.AddHours(1)
        };

        var responseContent = JsonContent.Create(expectedDto);
        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = responseContent
        };

        mockApi.Setup(x => x.PostAsync("auth/login", It.IsAny<HttpContent>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(httpResponse);

        mockTokenStorage.Setup(x => x.StoreTokensAsync(
            expectedDto.AccessToken,
            expectedDto.RefreshToken,
            expectedDto.ExpiresAt))
            .Returns(Task.CompletedTask);

        // Act
        var result = await authService.LoginAsync(email, password);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(expectedDto.AccessToken, result.AccessToken);
        Assert.AreEqual(expectedDto.RefreshToken, result.RefreshToken);
        Assert.AreEqual(expectedDto.ExpiresAt, result.ExpiresAt);

        mockTokenStorage.Verify(x => x.StoreTokensAsync(
            expectedDto.AccessToken,
            expectedDto.RefreshToken,
            expectedDto.ExpiresAt), Times.Once);
    }

    /// <summary>
    /// Tests that LoginAsync returns null and does not store tokens
    /// when the API returns a successful response but ReadFromJsonAsync returns null.
    /// </summary>
    [TestMethod]
    public async Task LoginAsync_SuccessfulResponseButNullDto_ReturnsNullAndDoesNotStoreTokens()
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockTokenStorage = new Mock<TokenStorageService>();
        var mockSession = new Mock<SessionService>();
        var mockLogger = new Mock<ILogger<AuthService>>();

        var authService = new AuthService(mockApi.Object, mockTokenStorage.Object, mockSession.Object, null!, Mock.Of<ICacheService>(), mockLogger.Object);

        var email = "test@example.com";
        var password = "password123";

        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("null", Encoding.UTF8, "application/json")
        };

        mockApi.Setup(x => x.PostAsync("auth/login", It.IsAny<HttpContent>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(httpResponse);

        // Act
        var result = await authService.LoginAsync(email, password);

        // Assert
        Assert.IsNull(result);

        mockTokenStorage.Verify(x => x.StoreTokensAsync(
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<DateTime?>()), Times.Never);
    }

    /// <summary>
    /// Tests that LoginAsync handles very long email and password strings correctly.
    /// The method should process the request and pass the strings to the API.
    /// </summary>
    [TestMethod]
    public async Task LoginAsync_VeryLongEmailAndPassword_ProcessesRequest()
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockTokenStorage = new Mock<TokenStorageService>();
        var mockSession = new Mock<SessionService>();
        var mockLogger = new Mock<ILogger<AuthService>>();

        var authService = new AuthService(mockApi.Object, mockTokenStorage.Object, mockSession.Object, null!, Mock.Of<ICacheService>(), mockLogger.Object);

        var email = new string('a', 1000) + "@example.com";
        var password = new string('p', 10000);

        var expectedDto = new AuthResponseDto
        {
            AccessToken = "access_token",
            RefreshToken = "refresh_token",
            ExpiresAt = DateTime.UtcNow.AddHours(1)
        };

        var responseContent = JsonContent.Create(expectedDto);
        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = responseContent
        };

        mockApi.Setup(x => x.PostAsync("auth/login", It.IsAny<HttpContent>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(httpResponse);

        mockTokenStorage.Setup(x => x.StoreTokensAsync(
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<DateTime?>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await authService.LoginAsync(email, password);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(expectedDto.AccessToken, result.AccessToken);
    }

    /// <summary>
    /// Tests that LoginAsync handles special characters in email and password correctly.
    /// The method should process the request with special characters in parameters.
    /// </summary>
    [TestMethod]
    public async Task LoginAsync_SpecialCharactersInEmailAndPassword_ProcessesRequest()
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockTokenStorage = new Mock<TokenStorageService>();
        var mockSession = new Mock<SessionService>();
        var mockLogger = new Mock<ILogger<AuthService>>();

        var authService = new AuthService(mockApi.Object, mockTokenStorage.Object, mockSession.Object, null!, Mock.Of<ICacheService>(), mockLogger.Object);

        var email = "test+tag@example.com";
        var password = "P@$$w0rd!#%&*()";

        var expectedDto = new AuthResponseDto
        {
            AccessToken = "access_token",
            RefreshToken = "refresh_token",
            ExpiresAt = DateTime.UtcNow.AddHours(1)
        };

        var responseContent = JsonContent.Create(expectedDto);
        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = responseContent
        };

        mockApi.Setup(x => x.PostAsync("auth/login", It.IsAny<HttpContent>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(httpResponse);

        mockTokenStorage.Setup(x => x.StoreTokensAsync(
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<DateTime?>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await authService.LoginAsync(email, password);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(expectedDto.AccessToken, result.AccessToken);
    }

    /// <summary>
    /// Tests that RegisterAsync returns true when API returns a successful status code.
    /// </summary>
    /// <param name="statusCode">The HTTP status code to test.</param>
    [DataRow(HttpStatusCode.OK)]
    [DataRow(HttpStatusCode.Created)]
    [DataRow(HttpStatusCode.Accepted)]
    [DataRow(HttpStatusCode.NoContent)]
    [TestMethod]
    public async Task RegisterAsync_SuccessfulResponse_ReturnsTrue(HttpStatusCode statusCode)
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockTokenStorage = new Mock<TokenStorageService>();
        var mockSession = new Mock<SessionService>();
        var mockLogger = new Mock<ILogger<AuthService>>();
        var dto = new RegisterDto
        {
            FullName = "Test User",
            Email = "test@example.com",
            Password = "Password123",
            PhoneNumber = "1234567890",
            StudentId = "STU001",
            Role = "Student"
        };
        var response = new HttpResponseMessage(statusCode);
        mockApi.Setup(x => x.PostAsync("auth/register", It.IsAny<HttpContent>(), It.IsAny<CancellationToken>()))
               .ReturnsAsync(response);
        var authService = new AuthService(mockApi.Object, mockTokenStorage.Object, mockSession.Object, null!, Mock.Of<ICacheService>(), mockLogger.Object);

        // Act
        var result = await authService.RegisterAsync(dto);

        // Assert
        Assert.IsTrue(result);
        mockApi.Verify(x => x.PostAsync("auth/register", It.IsAny<HttpContent>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Tests that RegisterAsync returns false when API returns a client error status code.
    /// </summary>
    /// <param name="statusCode">The HTTP status code to test.</param>
    [DataRow(HttpStatusCode.BadRequest)]
    [DataRow(HttpStatusCode.Unauthorized)]
    [DataRow(HttpStatusCode.Forbidden)]
    [DataRow(HttpStatusCode.NotFound)]
    [DataRow(HttpStatusCode.Conflict)]
    [TestMethod]
    public async Task RegisterAsync_ClientErrorResponse_ReturnsFalse(HttpStatusCode statusCode)
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockTokenStorage = new Mock<TokenStorageService>();
        var mockSession = new Mock<SessionService>();
        var mockLogger = new Mock<ILogger<AuthService>>();
        var dto = new RegisterDto
        {
            FullName = "Test User",
            Email = "test@example.com",
            Password = "Password123",
            PhoneNumber = "1234567890",
            StudentId = "STU001",
            Role = "Student"
        };
        var response = new HttpResponseMessage(statusCode);
        mockApi.Setup(x => x.PostAsync("auth/register", It.IsAny<HttpContent>(), It.IsAny<CancellationToken>()))
               .ReturnsAsync(response);
        var authService = new AuthService(mockApi.Object, mockTokenStorage.Object, mockSession.Object, null!, Mock.Of<ICacheService>(), mockLogger.Object);

        // Act
        var result = await authService.RegisterAsync(dto);

        // Assert
        Assert.IsFalse(result);
        mockApi.Verify(x => x.PostAsync("auth/register", It.IsAny<HttpContent>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Tests that RegisterAsync returns false when API returns a server error status code.
    /// </summary>
    /// <param name="statusCode">The HTTP status code to test.</param>
    [DataRow(HttpStatusCode.InternalServerError)]
    [DataRow(HttpStatusCode.BadGateway)]
    [DataRow(HttpStatusCode.ServiceUnavailable)]
    [DataRow(HttpStatusCode.GatewayTimeout)]
    [TestMethod]
    public async Task RegisterAsync_ServerErrorResponse_ReturnsFalse(HttpStatusCode statusCode)
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockTokenStorage = new Mock<TokenStorageService>();
        var mockSession = new Mock<SessionService>();
        var mockLogger = new Mock<ILogger<AuthService>>();
        var dto = new RegisterDto
        {
            FullName = "Test User",
            Email = "test@example.com",
            Password = "Password123",
            PhoneNumber = "1234567890",
            StudentId = "STU001",
            Role = "Student"
        };
        var response = new HttpResponseMessage(statusCode);
        mockApi.Setup(x => x.PostAsync("auth/register", It.IsAny<HttpContent>(), It.IsAny<CancellationToken>()))
               .ReturnsAsync(response);
        var authService = new AuthService(mockApi.Object, mockTokenStorage.Object, mockSession.Object, null!, Mock.Of<ICacheService>(), mockLogger.Object);

        // Act
        var result = await authService.RegisterAsync(dto);

        // Assert
        Assert.IsFalse(result);
        mockApi.Verify(x => x.PostAsync("auth/register", It.IsAny<HttpContent>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Tests that RegisterAsync handles dto with null email without throwing exception.
    /// </summary>
    [TestMethod]
    public async Task RegisterAsync_DtoWithNullEmail_CompletesSuccessfully()
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockTokenStorage = new Mock<TokenStorageService>();
        var mockSession = new Mock<SessionService>();
        var mockLogger = new Mock<ILogger<AuthService>>();
        var dto = new RegisterDto
        {
            FullName = "Test User",
            Email = null!,
            Password = "Password123",
            PhoneNumber = "1234567890",
            StudentId = "STU001",
            Role = "Student"
        };
        var response = new HttpResponseMessage(HttpStatusCode.OK);
        mockApi.Setup(x => x.PostAsync("auth/register", It.IsAny<HttpContent>(), It.IsAny<CancellationToken>()))
               .ReturnsAsync(response);
        var authService = new AuthService(mockApi.Object, mockTokenStorage.Object, mockSession.Object, null!, Mock.Of<ICacheService>(), mockLogger.Object);

        // Act
        var result = await authService.RegisterAsync(dto);

        // Assert
        Assert.IsTrue(result);
    }

    /// <summary>
    /// Tests that RegisterAsync handles dto with empty email without throwing exception.
    /// </summary>
    [TestMethod]
    public async Task RegisterAsync_DtoWithEmptyEmail_CompletesSuccessfully()
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockTokenStorage = new Mock<TokenStorageService>();
        var mockSession = new Mock<SessionService>();
        var mockLogger = new Mock<ILogger<AuthService>>();
        var dto = new RegisterDto
        {
            FullName = "Test User",
            Email = string.Empty,
            Password = "Password123",
            PhoneNumber = "1234567890",
            StudentId = "STU001",
            Role = "Student"
        };
        var response = new HttpResponseMessage(HttpStatusCode.OK);
        mockApi.Setup(x => x.PostAsync("auth/register", It.IsAny<HttpContent>(), It.IsAny<CancellationToken>()))
               .ReturnsAsync(response);
        var authService = new AuthService(mockApi.Object, mockTokenStorage.Object, mockSession.Object, null!, Mock.Of<ICacheService>(), mockLogger.Object);

        // Act
        var result = await authService.RegisterAsync(dto);

        // Assert
        Assert.IsTrue(result);
    }

    /// <summary>
    /// Tests that RegisterAsync handles dto with whitespace-only email without throwing exception.
    /// </summary>
    [TestMethod]
    public async Task RegisterAsync_DtoWithWhitespaceEmail_CompletesSuccessfully()
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockTokenStorage = new Mock<TokenStorageService>();
        var mockSession = new Mock<SessionService>();
        var mockLogger = new Mock<ILogger<AuthService>>();
        var dto = new RegisterDto
        {
            FullName = "Test User",
            Email = "   ",
            Password = "Password123",
            PhoneNumber = "1234567890",
            StudentId = "STU001",
            Role = "Student"
        };
        var response = new HttpResponseMessage(HttpStatusCode.OK);
        mockApi.Setup(x => x.PostAsync("auth/register", It.IsAny<HttpContent>(), It.IsAny<CancellationToken>()))
               .ReturnsAsync(response);
        var authService = new AuthService(mockApi.Object, mockTokenStorage.Object, mockSession.Object, null!, Mock.Of<ICacheService>(), mockLogger.Object);

        // Act
        var result = await authService.RegisterAsync(dto);

        // Assert
        Assert.IsTrue(result);
    }

    /// <summary>
    /// Tests that RegisterAsync handles dto with very long email without throwing exception.
    /// </summary>
    [TestMethod]
    public async Task RegisterAsync_DtoWithVeryLongEmail_CompletesSuccessfully()
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockTokenStorage = new Mock<TokenStorageService>();
        var mockSession = new Mock<SessionService>();
        var mockLogger = new Mock<ILogger<AuthService>>();
        var dto = new RegisterDto
        {
            FullName = "Test User",
            Email = new string('a', 10000) + "@example.com",
            Password = "Password123",
            PhoneNumber = "1234567890",
            StudentId = "STU001",
            Role = "Student"
        };
        var response = new HttpResponseMessage(HttpStatusCode.OK);
        mockApi.Setup(x => x.PostAsync("auth/register", It.IsAny<HttpContent>(), It.IsAny<CancellationToken>()))
               .ReturnsAsync(response);
        var authService = new AuthService(mockApi.Object, mockTokenStorage.Object, mockSession.Object, null!, Mock.Of<ICacheService>(), mockLogger.Object);

        // Act
        var result = await authService.RegisterAsync(dto);

        // Assert
        Assert.IsTrue(result);
    }

    /// <summary>
    /// Tests that RegisterAsync handles dto with special characters in email without throwing exception.
    /// </summary>
    [TestMethod]
    public async Task RegisterAsync_DtoWithSpecialCharactersInEmail_CompletesSuccessfully()
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockTokenStorage = new Mock<TokenStorageService>();
        var mockSession = new Mock<SessionService>();
        var mockLogger = new Mock<ILogger<AuthService>>();
        var dto = new RegisterDto
        {
            FullName = "Test User",
            Email = "test+special!#$%&'*+-/=?^_`{|}~@example.com",
            Password = "Password123",
            PhoneNumber = "1234567890",
            StudentId = "STU001",
            Role = "Student"
        };
        var response = new HttpResponseMessage(HttpStatusCode.OK);
        mockApi.Setup(x => x.PostAsync("auth/register", It.IsAny<HttpContent>(), It.IsAny<CancellationToken>()))
               .ReturnsAsync(response);
        var authService = new AuthService(mockApi.Object, mockTokenStorage.Object, mockSession.Object, null!, Mock.Of<ICacheService>(), mockLogger.Object);

        // Act
        var result = await authService.RegisterAsync(dto);

        // Assert
        Assert.IsTrue(result);
    }

    /// <summary>
    /// Tests that RegisterAsync calls PostAsync with correct endpoint and content.
    /// </summary>
    [TestMethod]
    public async Task RegisterAsync_ValidDto_CallsPostAsyncWithCorrectEndpoint()
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockTokenStorage = new Mock<TokenStorageService>();
        var mockSession = new Mock<SessionService>();
        var mockLogger = new Mock<ILogger<AuthService>>();
        var dto = new RegisterDto
        {
            FullName = "Test User",
            Email = "test@example.com",
            Password = "Password123",
            PhoneNumber = "1234567890",
            StudentId = "STU001",
            Role = "Student"
        };
        var response = new HttpResponseMessage(HttpStatusCode.OK);
        mockApi.Setup(x => x.PostAsync("auth/register", It.IsAny<HttpContent>(), It.IsAny<CancellationToken>()))
               .ReturnsAsync(response);
        var authService = new AuthService(mockApi.Object, mockTokenStorage.Object, mockSession.Object, null!, Mock.Of<ICacheService>(), mockLogger.Object);

        // Act
        await authService.RegisterAsync(dto);

        // Assert
        mockApi.Verify(x => x.PostAsync("auth/register", It.IsAny<HttpContent>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Tests that RegisterAsync handles dto with all empty properties successfully.
    /// </summary>
    [TestMethod]
    public async Task RegisterAsync_DtoWithAllEmptyProperties_CompletesSuccessfully()
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockTokenStorage = new Mock<TokenStorageService>();
        var mockSession = new Mock<SessionService>();
        var mockLogger = new Mock<ILogger<AuthService>>();
        var dto = new RegisterDto
        {
            FullName = string.Empty,
            Email = string.Empty,
            Password = string.Empty,
            PhoneNumber = string.Empty,
            StudentId = string.Empty,
            Role = string.Empty
        };
        var response = new HttpResponseMessage(HttpStatusCode.OK);
        mockApi.Setup(x => x.PostAsync("auth/register", It.IsAny<HttpContent>(), It.IsAny<CancellationToken>()))
               .ReturnsAsync(response);
        var authService = new AuthService(mockApi.Object, mockTokenStorage.Object, mockSession.Object, null!, Mock.Of<ICacheService>(), mockLogger.Object);

        // Act
        var result = await authService.RegisterAsync(dto);

        // Assert
        Assert.IsTrue(result);
    }

    /// <summary>
    /// Tests that VerifyOtpAsync returns the verification token when the API responds with a success status code
    /// and valid VerifyOtpResponseDto content.
    /// </summary>
    [TestMethod]
    public async Task VerifyOtpAsync_ValidCredentialsAndSuccessResponse_ReturnsVerificationToken()
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockTokenStorage = new Mock<TokenStorageService>();
        var mockSession = new Mock<SessionService>();
        var mockLogger = new Mock<ILogger<AuthService>>();
        var authService = new AuthService(mockApi.Object, mockTokenStorage.Object, mockSession.Object, null!, Mock.Of<ICacheService>(), mockLogger.Object);

        string email = "test@example.com";
        string otpCode = "123456";
        string expectedToken = "verification-token-12345";

        var responseDto = new VerifyOtpResponseDto
        {
            Message = "Success",
            VerificationToken = expectedToken
        };
        var jsonContent = JsonContent.Create(responseDto);
        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = jsonContent
        };

        mockApi.Setup(x => x.PostAsync(
            "auth/verify-otp",
            It.IsAny<HttpContent>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(httpResponse);

        // Act
        var result = await authService.VerifyOtpAsync(email, otpCode);

        // Assert
        Assert.AreEqual(expectedToken, result);
        mockApi.Verify(x => x.PostAsync(
            "auth/verify-otp",
            It.IsAny<HttpContent>(),
            It.IsAny<CancellationToken>()),
            Times.Once);
    }

    /// <summary>
    /// Tests that VerifyOtpAsync returns null when the API responds with various non-success status codes.
    /// </summary>
    /// <param name="statusCode">The HTTP status code to test.</param>
    [TestMethod]
    [DataRow(HttpStatusCode.BadRequest)]
    [DataRow(HttpStatusCode.Unauthorized)]
    [DataRow(HttpStatusCode.Forbidden)]
    [DataRow(HttpStatusCode.NotFound)]
    [DataRow(HttpStatusCode.InternalServerError)]
    [DataRow(HttpStatusCode.ServiceUnavailable)]
    [DataRow(HttpStatusCode.GatewayTimeout)]
    public async Task VerifyOtpAsync_NonSuccessStatusCode_ReturnsNull(HttpStatusCode statusCode)
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockTokenStorage = new Mock<TokenStorageService>();
        var mockSession = new Mock<SessionService>();
        var mockLogger = new Mock<ILogger<AuthService>>();
        var authService = new AuthService(mockApi.Object, mockTokenStorage.Object, mockSession.Object, null!, Mock.Of<ICacheService>(), mockLogger.Object);

        string email = "test@example.com";
        string otpCode = "123456";

        var httpResponse = new HttpResponseMessage(statusCode);

        mockApi.Setup(x => x.PostAsync(
            "auth/verify-otp",
            It.IsAny<HttpContent>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(httpResponse);

        // Act
        var result = await authService.VerifyOtpAsync(email, otpCode);

        // Assert
        Assert.IsNull(result);
    }

    /// <summary>
    /// Tests that VerifyOtpAsync returns null when the API response content cannot be deserialized
    /// (e.g., null or invalid JSON).
    /// </summary>
    [TestMethod]
    public async Task VerifyOtpAsync_InvalidJsonContent_ReturnsNull()
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockTokenStorage = new Mock<TokenStorageService>();
        var mockSession = new Mock<SessionService>();
        var mockLogger = new Mock<ILogger<AuthService>>();
        var authService = new AuthService(mockApi.Object, mockTokenStorage.Object, mockSession.Object, null!, Mock.Of<ICacheService>(), mockLogger.Object);

        string email = "test@example.com";
        string otpCode = "123456";

        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("null", Encoding.UTF8, "application/json")
        };

        mockApi.Setup(x => x.PostAsync(
            "auth/verify-otp",
            It.IsAny<HttpContent>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(httpResponse);

        // Act
        var result = await authService.VerifyOtpAsync(email, otpCode);

        // Assert
        Assert.IsNull(result);
    }

    /// <summary>
    /// Tests that VerifyOtpAsync returns an empty string when the API responds successfully
    /// but the VerificationToken property is empty.
    /// </summary>
    [TestMethod]
    public async Task VerifyOtpAsync_SuccessResponseWithEmptyVerificationToken_ReturnsEmptyString()
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockTokenStorage = new Mock<TokenStorageService>();
        var mockSession = new Mock<SessionService>();
        var mockLogger = new Mock<ILogger<AuthService>>();
        var authService = new AuthService(mockApi.Object, mockTokenStorage.Object, mockSession.Object, null!, Mock.Of<ICacheService>(), mockLogger.Object);

        string email = "test@example.com";
        string otpCode = "123456";

        var responseDto = new VerifyOtpResponseDto
        {
            Message = "Success",
            VerificationToken = string.Empty
        };
        var jsonContent = JsonContent.Create(responseDto);
        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = jsonContent
        };

        mockApi.Setup(x => x.PostAsync(
            "auth/verify-otp",
            It.IsAny<HttpContent>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(httpResponse);

        // Act
        var result = await authService.VerifyOtpAsync(email, otpCode);

        // Assert
        Assert.AreEqual(string.Empty, result);
    }

    /// <summary>
    /// Tests that VerifyOtpAsync handles empty string inputs for email and otpCode
    /// by calling the API and processing the response.
    /// </summary>
    /// <param name="email">The email value to test.</param>
    /// <param name="otpCode">The OTP code value to test.</param>
    [TestMethod]
    [DataRow("", "123456")]
    [DataRow("test@example.com", "")]
    [DataRow("", "")]
    public async Task VerifyOtpAsync_EmptyStringInputs_CallsApiAndProcessesResponse(string email, string otpCode)
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockTokenStorage = new Mock<TokenStorageService>();
        var mockSession = new Mock<SessionService>();
        var mockLogger = new Mock<ILogger<AuthService>>();
        var authService = new AuthService(mockApi.Object, mockTokenStorage.Object, mockSession.Object, null!, Mock.Of<ICacheService>(), mockLogger.Object);

        string expectedToken = "token-xyz";
        var responseDto = new VerifyOtpResponseDto
        {
            Message = "Success",
            VerificationToken = expectedToken
        };
        var jsonContent = JsonContent.Create(responseDto);
        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = jsonContent
        };

        mockApi.Setup(x => x.PostAsync(
            "auth/verify-otp",
            It.IsAny<HttpContent>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(httpResponse);

        // Act
        var result = await authService.VerifyOtpAsync(email, otpCode);

        // Assert
        Assert.AreEqual(expectedToken, result);
        mockApi.Verify(x => x.PostAsync(
            "auth/verify-otp",
            It.IsAny<HttpContent>(),
            It.IsAny<CancellationToken>()),
            Times.Once);
    }

    /// <summary>
    /// Tests that VerifyOtpAsync handles whitespace-only string inputs
    /// by calling the API and processing the response.
    /// </summary>
    /// <param name="email">The email value to test.</param>
    /// <param name="otpCode">The OTP code value to test.</param>
    [TestMethod]
    [DataRow("   ", "123456")]
    [DataRow("test@example.com", "   ")]
    [DataRow("   ", "   ")]
    [DataRow("\t", "\n")]
    public async Task VerifyOtpAsync_WhitespaceInputs_CallsApiAndProcessesResponse(string email, string otpCode)
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockTokenStorage = new Mock<TokenStorageService>();
        var mockSession = new Mock<SessionService>();
        var mockLogger = new Mock<ILogger<AuthService>>();
        var authService = new AuthService(mockApi.Object, mockTokenStorage.Object, mockSession.Object, null!, Mock.Of<ICacheService>(), mockLogger.Object);

        string expectedToken = "token-abc";
        var responseDto = new VerifyOtpResponseDto
        {
            Message = "Success",
            VerificationToken = expectedToken
        };
        var jsonContent = JsonContent.Create(responseDto);
        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = jsonContent
        };

        mockApi.Setup(x => x.PostAsync(
            "auth/verify-otp",
            It.IsAny<HttpContent>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(httpResponse);

        // Act
        var result = await authService.VerifyOtpAsync(email, otpCode);

        // Assert
        Assert.AreEqual(expectedToken, result);
    }

    /// <summary>
    /// Tests that VerifyOtpAsync handles very long string inputs
    /// by calling the API and processing the response.
    /// </summary>
    [TestMethod]
    public async Task VerifyOtpAsync_VeryLongInputs_CallsApiAndProcessesResponse()
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockTokenStorage = new Mock<TokenStorageService>();
        var mockSession = new Mock<SessionService>();
        var mockLogger = new Mock<ILogger<AuthService>>();
        var authService = new AuthService(mockApi.Object, mockTokenStorage.Object, mockSession.Object, null!, Mock.Of<ICacheService>(), mockLogger.Object);

        string email = new string('a', 10000) + "@example.com";
        string otpCode = new string('1', 10000);
        string expectedToken = "token-long";

        var responseDto = new VerifyOtpResponseDto
        {
            Message = "Success",
            VerificationToken = expectedToken
        };
        var jsonContent = JsonContent.Create(responseDto);
        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = jsonContent
        };

        mockApi.Setup(x => x.PostAsync(
            "auth/verify-otp",
            It.IsAny<HttpContent>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(httpResponse);

        // Act
        var result = await authService.VerifyOtpAsync(email, otpCode);

        // Assert
        Assert.AreEqual(expectedToken, result);
    }

    /// <summary>
    /// Tests that VerifyOtpAsync handles special characters in email and otpCode
    /// by calling the API and processing the response.
    /// </summary>
    /// <param name="email">The email value to test.</param>
    /// <param name="otpCode">The OTP code value to test.</param>
    [TestMethod]
    [DataRow("test+special@example.com", "ABC123")]
    [DataRow("test@sub.domain.example.com", "!@#$%^")]
    [DataRow("user<script>@test.com", "123<>456")]
    [DataRow("Ã§â€Â¨Ã¦Ë†Â·@example.com", "Ã¥Â¯â€ Ã§Â Â123")]
    public async Task VerifyOtpAsync_SpecialCharactersInInputs_CallsApiAndProcessesResponse(string email, string otpCode)
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockTokenStorage = new Mock<TokenStorageService>();
        var mockSession = new Mock<SessionService>();
        var mockLogger = new Mock<ILogger<AuthService>>();
        var authService = new AuthService(mockApi.Object, mockTokenStorage.Object, mockSession.Object, null!, Mock.Of<ICacheService>(), mockLogger.Object);

        string expectedToken = "token-special";
        var responseDto = new VerifyOtpResponseDto
        {
            Message = "Success",
            VerificationToken = expectedToken
        };
        var jsonContent = JsonContent.Create(responseDto);
        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = jsonContent
        };

        mockApi.Setup(x => x.PostAsync(
            "auth/verify-otp",
            It.IsAny<HttpContent>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(httpResponse);

        // Act
        var result = await authService.VerifyOtpAsync(email, otpCode);

        // Assert
        Assert.AreEqual(expectedToken, result);
    }

    /// <summary>
    /// Tests that VerifyOtpAsync returns null when response content is empty.
    /// </summary>
    [TestMethod]
    public async Task VerifyOtpAsync_EmptyContent_ReturnsNull()
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockTokenStorage = new Mock<TokenStorageService>();
        var mockSession = new Mock<SessionService>();
        var mockLogger = new Mock<ILogger<AuthService>>();
        var authService = new AuthService(mockApi.Object, mockTokenStorage.Object, mockSession.Object, null!, Mock.Of<ICacheService>(), mockLogger.Object);

        string email = "test@example.com";
        string otpCode = "123456";

        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(string.Empty, Encoding.UTF8, "application/json")
        };

        mockApi.Setup(x => x.PostAsync(
            "auth/verify-otp",
            It.IsAny<HttpContent>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(httpResponse);

        // Act
        var result = await authService.VerifyOtpAsync(email, otpCode);

        // Assert
        Assert.IsNull(result);
    }

    /// <summary>
    /// Tests that VerifyOtpAsync returns the verification token even when it contains special characters
    /// or unusual formatting.
    /// </summary>
    /// <param name="verificationToken">The verification token to test.</param>
    [TestMethod]
    [DataRow("")]
    [DataRow("simple-token")]
    [DataRow("token-with-special-chars-!@#$%")]
    [DataRow("very-long-token-" + "abcdefghijklmnopqrstuvwxyz0123456789")]
    [DataRow("token\nwith\nnewlines")]
    [DataRow("token\twith\ttabs")]
    public async Task VerifyOtpAsync_VariousTokenFormats_ReturnsToken(string verificationToken)
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockTokenStorage = new Mock<TokenStorageService>();
        var mockSession = new Mock<SessionService>();
        var mockLogger = new Mock<ILogger<AuthService>>();
        var authService = new AuthService(mockApi.Object, mockTokenStorage.Object, mockSession.Object, null!, Mock.Of<ICacheService>(), mockLogger.Object);

        string email = "test@example.com";
        string otpCode = "123456";

        var responseDto = new VerifyOtpResponseDto
        {
            Message = "Success",
            VerificationToken = verificationToken
        };
        var jsonContent = JsonContent.Create(responseDto);
        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = jsonContent
        };

        mockApi.Setup(x => x.PostAsync(
            "auth/verify-otp",
            It.IsAny<HttpContent>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(httpResponse);

        // Act
        var result = await authService.VerifyOtpAsync(email, otpCode);

        // Assert
        Assert.AreEqual(verificationToken, result);
    }

    /// <summary>
    /// Tests that TryRefreshTokenAsync returns false when GetRefreshTokenAsync returns null.
    /// </summary>
    [TestMethod]
    public async Task TryRefreshTokenAsync_RefreshTokenIsNull_ReturnsFalse()
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockTokenStorage = new Mock<TokenStorageService>();
        var mockSession = new Mock<SessionService>();
        var mockLogger = new Mock<ILogger<AuthService>>();

        mockTokenStorage.Setup(x => x.GetRefreshTokenAsync()).ReturnsAsync((string?)null);

        var authService = new AuthService(mockApi.Object, mockTokenStorage.Object, mockSession.Object, null!, Mock.Of<ICacheService>(), mockLogger.Object);

        // Act
        var result = await authService.TryRefreshTokenAsync();

        // Assert
        Assert.IsFalse(result);
        mockApi.Verify(x => x.PostAsync(It.IsAny<string>(), It.IsAny<HttpContent>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    /// <summary>
    /// Tests that TryRefreshTokenAsync returns false when GetRefreshTokenAsync returns an empty string.
    /// </summary>
    [TestMethod]
    public async Task TryRefreshTokenAsync_RefreshTokenIsEmpty_ReturnsFalse()
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockTokenStorage = new Mock<TokenStorageService>();
        var mockSession = new Mock<SessionService>();
        var mockLogger = new Mock<ILogger<AuthService>>();

        mockTokenStorage.Setup(x => x.GetRefreshTokenAsync()).ReturnsAsync(string.Empty);

        var authService = new AuthService(mockApi.Object, mockTokenStorage.Object, mockSession.Object, null!, Mock.Of<ICacheService>(), mockLogger.Object);

        // Act
        var result = await authService.TryRefreshTokenAsync();

        // Assert
        Assert.IsFalse(result);
        mockApi.Verify(x => x.PostAsync(It.IsAny<string>(), It.IsAny<HttpContent>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    /// <summary>
    /// Tests that TryRefreshTokenAsync returns false when GetRefreshTokenAsync returns whitespace.
    /// </summary>
    [TestMethod]
    [DataRow("   ")]
    [DataRow("\t")]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [DataRow(" \t\n ")]
    public async Task TryRefreshTokenAsync_RefreshTokenIsWhitespace_ReturnsFalse(string whitespaceToken)
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockTokenStorage = new Mock<TokenStorageService>();
        var mockSession = new Mock<SessionService>();
        var mockLogger = new Mock<ILogger<AuthService>>();

        mockTokenStorage.Setup(x => x.GetRefreshTokenAsync()).ReturnsAsync(whitespaceToken);

        var authService = new AuthService(mockApi.Object, mockTokenStorage.Object, mockSession.Object, null!, Mock.Of<ICacheService>(), mockLogger.Object);

        // Act
        var result = await authService.TryRefreshTokenAsync();

        // Assert
        Assert.IsFalse(result);
        mockApi.Verify(x => x.PostAsync(It.IsAny<string>(), It.IsAny<HttpContent>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    /// <summary>
    /// Tests that TryRefreshTokenAsync returns false and logs warning when API returns non-success status code.
    /// </summary>
    [TestMethod]
    [DataRow(HttpStatusCode.Unauthorized)]
    [DataRow(HttpStatusCode.BadRequest)]
    [DataRow(HttpStatusCode.InternalServerError)]
    [DataRow(HttpStatusCode.Forbidden)]
    [DataRow(HttpStatusCode.NotFound)]
    [DataRow(HttpStatusCode.ServiceUnavailable)]
    public async Task TryRefreshTokenAsync_ApiReturnsNonSuccessStatusCode_ReturnsFalseAndLogsWarning(HttpStatusCode statusCode)
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockTokenStorage = new Mock<TokenStorageService>();
        var mockSession = new Mock<SessionService>();
        var mockLogger = new Mock<ILogger<AuthService>>();

        var refreshToken = "valid-refresh-token";
        mockTokenStorage.Setup(x => x.GetRefreshTokenAsync()).ReturnsAsync(refreshToken);

        var response = new HttpResponseMessage(statusCode);
        mockApi.Setup(x => x.PostAsync("auth/refresh", It.IsAny<HttpContent>(), It.IsAny<CancellationToken>()))
               .ReturnsAsync(response);

        var authService = new AuthService(mockApi.Object, mockTokenStorage.Object, mockSession.Object, null!, Mock.Of<ICacheService>(), mockLogger.Object);

        // Act
        var result = await authService.TryRefreshTokenAsync();

        // Assert
        Assert.IsFalse(result);
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Token refresh returned")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
        mockTokenStorage.Verify(x => x.StoreTokensAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTime?>()), Times.Never);
    }

    /// <summary>
    /// Tests that TryRefreshTokenAsync returns false when response content deserializes to null.
    /// </summary>
    [TestMethod]
    public async Task TryRefreshTokenAsync_ResponseContentDeserializesToNull_ReturnsFalse()
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockTokenStorage = new Mock<TokenStorageService>();
        var mockSession = new Mock<SessionService>();
        var mockLogger = new Mock<ILogger<AuthService>>();

        var refreshToken = "valid-refresh-token";
        mockTokenStorage.Setup(x => x.GetRefreshTokenAsync()).ReturnsAsync(refreshToken);

        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("null", Encoding.UTF8, "application/json")
        };
        mockApi.Setup(x => x.PostAsync("auth/refresh", It.IsAny<HttpContent>(), It.IsAny<CancellationToken>()))
               .ReturnsAsync(response);

        var authService = new AuthService(mockApi.Object, mockTokenStorage.Object, mockSession.Object, null!, Mock.Of<ICacheService>(), mockLogger.Object);

        // Act
        var result = await authService.TryRefreshTokenAsync();

        // Assert
        Assert.IsFalse(result);
        mockTokenStorage.Verify(x => x.StoreTokensAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTime?>()), Times.Never);
    }

    /// <summary>
    /// Tests that TryRefreshTokenAsync returns true and stores tokens when refresh is successful.
    /// </summary>
    [TestMethod]
    public async Task TryRefreshTokenAsync_SuccessfulRefresh_ReturnsTrueAndStoresTokens()
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockTokenStorage = new Mock<TokenStorageService>();
        var mockSession = new Mock<SessionService>();
        var mockLogger = new Mock<ILogger<AuthService>>();

        var refreshToken = "valid-refresh-token";
        var newAccessToken = "new-access-token";
        var newRefreshToken = "new-refresh-token";
        var expiresAt = DateTime.UtcNow.AddHours(1);

        mockTokenStorage.Setup(x => x.GetRefreshTokenAsync()).ReturnsAsync(refreshToken);

        var dto = new AuthResponseDto
        {
            AccessToken = newAccessToken,
            RefreshToken = newRefreshToken,
            ExpiresAt = expiresAt
        };

        var jsonContent = JsonSerializer.Serialize(dto);
        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(jsonContent, Encoding.UTF8, "application/json")
        };
        mockApi.Setup(x => x.PostAsync("auth/refresh", It.IsAny<HttpContent>(), It.IsAny<CancellationToken>()))
               .ReturnsAsync(response);

        mockTokenStorage.Setup(x => x.StoreTokensAsync(newAccessToken, newRefreshToken, expiresAt))
                       .Returns(Task.CompletedTask);

        var authService = new AuthService(mockApi.Object, mockTokenStorage.Object, mockSession.Object, null!, Mock.Of<ICacheService>(), mockLogger.Object);

        // Act
        var result = await authService.TryRefreshTokenAsync();

        // Assert
        Assert.IsTrue(result);
        mockTokenStorage.Verify(x => x.StoreTokensAsync(newAccessToken, newRefreshToken, expiresAt), Times.Once);
        mockApi.Verify(x => x.PostAsync("auth/refresh", It.IsAny<HttpContent>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Tests that TryRefreshTokenAsync handles special characters in refresh token correctly.
    /// </summary>
    [TestMethod]
    [DataRow("token-with-special-chars!@#$%^&*()")]
    [DataRow("token.with.dots")]
    [DataRow("token_with_underscores")]
    [DataRow("token+with+plus")]
    [DataRow("very-long-token-" + "abcdefghijklmnopqrstuvwxyz0123456789" + "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789")]
    public async Task TryRefreshTokenAsync_RefreshTokenWithSpecialCharacters_ProcessesCorrectly(string refreshToken)
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockTokenStorage = new Mock<TokenStorageService>();
        var mockSession = new Mock<SessionService>();
        var mockLogger = new Mock<ILogger<AuthService>>();

        mockTokenStorage.Setup(x => x.GetRefreshTokenAsync()).ReturnsAsync(refreshToken);

        var dto = new AuthResponseDto
        {
            AccessToken = "new-access-token",
            RefreshToken = "new-refresh-token",
            ExpiresAt = DateTime.UtcNow.AddHours(1)
        };

        var jsonContent = JsonSerializer.Serialize(dto);
        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(jsonContent, Encoding.UTF8, "application/json")
        };
        mockApi.Setup(x => x.PostAsync("auth/refresh", It.IsAny<HttpContent>(), It.IsAny<CancellationToken>()))
               .ReturnsAsync(response);

        mockTokenStorage.Setup(x => x.StoreTokensAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTime?>()))
                       .Returns(Task.CompletedTask);

        var authService = new AuthService(mockApi.Object, mockTokenStorage.Object, mockSession.Object, null!, Mock.Of<ICacheService>(), mockLogger.Object);

        // Act
        var result = await authService.TryRefreshTokenAsync();

        // Assert
        Assert.IsTrue(result);
        mockApi.Verify(x => x.PostAsync("auth/refresh", It.IsAny<HttpContent>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Tests that TryRefreshTokenAsync handles DateTime.MinValue in ExpiresAt correctly.
    /// </summary>
    [TestMethod]
    public async Task TryRefreshTokenAsync_ExpiresAtIsMinValue_StoresTokensCorrectly()
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockTokenStorage = new Mock<TokenStorageService>();
        var mockSession = new Mock<SessionService>();
        var mockLogger = new Mock<ILogger<AuthService>>();

        var refreshToken = "valid-refresh-token";
        mockTokenStorage.Setup(x => x.GetRefreshTokenAsync()).ReturnsAsync(refreshToken);

        var dto = new AuthResponseDto
        {
            AccessToken = "new-access-token",
            RefreshToken = "new-refresh-token",
            ExpiresAt = DateTime.MinValue
        };

        var jsonContent = JsonSerializer.Serialize(dto);
        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(jsonContent, Encoding.UTF8, "application/json")
        };
        mockApi.Setup(x => x.PostAsync("auth/refresh", It.IsAny<HttpContent>(), It.IsAny<CancellationToken>()))
               .ReturnsAsync(response);

        mockTokenStorage.Setup(x => x.StoreTokensAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTime?>()))
                       .Returns(Task.CompletedTask);

        var authService = new AuthService(mockApi.Object, mockTokenStorage.Object, mockSession.Object, null!, Mock.Of<ICacheService>(), mockLogger.Object);

        // Act
        var result = await authService.TryRefreshTokenAsync();

        // Assert
        Assert.IsTrue(result);
        mockTokenStorage.Verify(x => x.StoreTokensAsync("new-access-token", "new-refresh-token", DateTime.MinValue), Times.Once);
    }

    /// <summary>
    /// Tests that TryRefreshTokenAsync handles DateTime.MaxValue in ExpiresAt correctly.
    /// </summary>
    [TestMethod]
    public async Task TryRefreshTokenAsync_ExpiresAtIsMaxValue_StoresTokensCorrectly()
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockTokenStorage = new Mock<TokenStorageService>();
        var mockSession = new Mock<SessionService>();
        var mockLogger = new Mock<ILogger<AuthService>>();

        var refreshToken = "valid-refresh-token";
        mockTokenStorage.Setup(x => x.GetRefreshTokenAsync()).ReturnsAsync(refreshToken);

        var dto = new AuthResponseDto
        {
            AccessToken = "new-access-token",
            RefreshToken = "new-refresh-token",
            ExpiresAt = DateTime.MaxValue
        };

        var jsonContent = JsonSerializer.Serialize(dto);
        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(jsonContent, Encoding.UTF8, "application/json")
        };
        mockApi.Setup(x => x.PostAsync("auth/refresh", It.IsAny<HttpContent>(), It.IsAny<CancellationToken>()))
               .ReturnsAsync(response);

        mockTokenStorage.Setup(x => x.StoreTokensAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTime?>()))
                       .Returns(Task.CompletedTask);

        var authService = new AuthService(mockApi.Object, mockTokenStorage.Object, mockSession.Object, null!, Mock.Of<ICacheService>(), mockLogger.Object);

        // Act
        var result = await authService.TryRefreshTokenAsync();

        // Assert
        Assert.IsTrue(result);
        mockTokenStorage.Verify(x => x.StoreTokensAsync("new-access-token", "new-refresh-token", DateTime.MaxValue), Times.Once);
    }

    /// <summary>
    /// Tests that TryRefreshTokenAsync handles empty string tokens in response correctly.
    /// </summary>
    [TestMethod]
    public async Task TryRefreshTokenAsync_ResponseContainsEmptyTokens_StoresEmptyTokens()
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockTokenStorage = new Mock<TokenStorageService>();
        var mockSession = new Mock<SessionService>();
        var mockLogger = new Mock<ILogger<AuthService>>();

        var refreshToken = "valid-refresh-token";
        mockTokenStorage.Setup(x => x.GetRefreshTokenAsync()).ReturnsAsync(refreshToken);

        var dto = new AuthResponseDto
        {
            AccessToken = string.Empty,
            RefreshToken = string.Empty,
            ExpiresAt = DateTime.UtcNow
        };

        var jsonContent = JsonSerializer.Serialize(dto);
        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(jsonContent, Encoding.UTF8, "application/json")
        };
        mockApi.Setup(x => x.PostAsync("auth/refresh", It.IsAny<HttpContent>(), It.IsAny<CancellationToken>()))
               .ReturnsAsync(response);

        mockTokenStorage.Setup(x => x.StoreTokensAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTime?>()))
                       .Returns(Task.CompletedTask);

        var authService = new AuthService(mockApi.Object, mockTokenStorage.Object, mockSession.Object, null!, Mock.Of<ICacheService>(), mockLogger.Object);

        // Act
        var result = await authService.TryRefreshTokenAsync();

        // Assert
        Assert.IsTrue(result);
        mockTokenStorage.Verify(x => x.StoreTokensAsync(string.Empty, string.Empty, It.IsAny<DateTime?>()), Times.Once);
    }

    /// <summary>
    /// Tests that LogoutAsync logs an information message, clears the session, and clears token storage successfully.
    /// Note: This test cannot verify SecureStorage.Remove("user_role") or Shell.Current.GoToAsync("//LoginPage")
    /// as they are static dependencies that cannot be mocked with Moq. A refactoring to inject these dependencies
    /// as abstractions would be required for full test coverage.
    /// </summary>
    [TestMethod]
    public async Task LogoutAsync_ValidCall_LogsInformationAndClearsSessionAndTokenStorage()
    {
        // Arrange
        var mockApiService = new Mock<Interfaces.IApiService>();
        var mockTokenStorage = new Mock<TokenStorageService>(MockBehavior.Loose, Mock.Of<ILogger<TokenStorageService>>());
        var mockSession = new Mock<SessionService>(MockBehavior.Loose);
        var mockLogger = new Mock<ILogger<AuthService>>();

        mockTokenStorage.Setup(ts => ts.ClearAsync()).Returns(Task.CompletedTask);
        mockSession.Setup(s => s.Clear());

        var authService = new AuthService(
            mockApiService.Object,
            mockTokenStorage.Object,
            mockSession.Object,
            new PollingService(Mock.Of<IRefreshCoordinator>(), Mock.Of<ILogger<PollingService>>()),
            Mock.Of<ICacheService>(),
            mockLogger.Object);

        // Act
        // Note: This will throw because Shell.Current.GoToAsync cannot be mocked.
        // In a real scenario, this would require refactoring to inject navigation as a dependency.
        try
        {
            await authService.LogoutAsync();
        }
        catch (NullReferenceException)
        {
            // Expected: Shell.Current is null in unit test context
        }
        catch (InvalidOperationException)
        {
            // Expected: Shell.Current might throw InvalidOperationException
        }

        // Assert
        mockLogger.Verify(
            l => l.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("User logging out")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once,
            "Logger should log information message about user logging out");

        mockSession.Verify(s => s.Clear(), Times.Once, "Session should be cleared");
        mockTokenStorage.Verify(ts => ts.ClearAsync(), Times.Once, "Token storage should be cleared");
    }

    /// <summary>
    /// Tests that SendOtpAsync returns true when the API call is successful with a valid email.
    /// Input: Valid email address.
    /// Expected: Returns true, no warning is logged.
    /// </summary>
    [TestMethod]
    public async Task SendOtpAsync_ValidEmailAndSuccessfulResponse_ReturnsTrue()
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockTokenStorage = new Mock<TokenStorageService>();
        var mockSession = new Mock<SessionService>();
        var mockLogger = new Mock<ILogger<AuthService>>();

        var successResponse = new HttpResponseMessage(HttpStatusCode.OK);
        mockApi.Setup(x => x.PostAsync(
            "auth/send-otp",
            It.IsAny<HttpContent>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(successResponse);

        var authService = new AuthService(mockApi.Object, mockTokenStorage.Object, mockSession.Object, null!, Mock.Of<ICacheService>(), mockLogger.Object);
        string email = "user@example.com";

        // Act
        bool result = await authService.SendOtpAsync(email);

        // Assert
        Assert.IsTrue(result);
        mockApi.Verify(x => x.PostAsync(
            "auth/send-otp",
            It.IsAny<HttpContent>(),
            It.IsAny<CancellationToken>()), Times.Once);
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
    /// Tests that SendOtpAsync returns false and logs a warning when the API call fails.
    /// Input: Valid email with various HTTP error status codes.
    /// Expected: Returns false, warning is logged with the status code.
    /// </summary>
    [TestMethod]
    [DataRow(HttpStatusCode.BadRequest, DisplayName = "BadRequest (400)")]
    [DataRow(HttpStatusCode.Unauthorized, DisplayName = "Unauthorized (401)")]
    [DataRow(HttpStatusCode.Forbidden, DisplayName = "Forbidden (403)")]
    [DataRow(HttpStatusCode.NotFound, DisplayName = "NotFound (404)")]
    [DataRow(HttpStatusCode.InternalServerError, DisplayName = "InternalServerError (500)")]
    [DataRow(HttpStatusCode.BadGateway, DisplayName = "BadGateway (502)")]
    [DataRow(HttpStatusCode.ServiceUnavailable, DisplayName = "ServiceUnavailable (503)")]
    public async Task SendOtpAsync_ApiReturnsErrorStatusCode_ReturnsFalseAndLogsWarning(HttpStatusCode statusCode)
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockTokenStorage = new Mock<TokenStorageService>();
        var mockSession = new Mock<SessionService>();
        var mockLogger = new Mock<ILogger<AuthService>>();

        var errorResponse = new HttpResponseMessage(statusCode);
        mockApi.Setup(x => x.PostAsync(
            "auth/send-otp",
            It.IsAny<HttpContent>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(errorResponse);

        var authService = new AuthService(mockApi.Object, mockTokenStorage.Object, mockSession.Object, null!, Mock.Of<ICacheService>(), mockLogger.Object);
        string email = "user@example.com";

        // Act
        bool result = await authService.SendOtpAsync(email);

        // Assert
        Assert.IsFalse(result);
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("send-otp failed")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    /// <summary>
    /// Tests that SendOtpAsync handles empty string email.
    /// Input: Empty string.
    /// Expected: Calls API with empty string, returns result based on API response.
    /// </summary>
    [TestMethod]
    public async Task SendOtpAsync_EmptyStringEmail_CallsApiAndReturnsResult()
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockTokenStorage = new Mock<TokenStorageService>();
        var mockSession = new Mock<SessionService>();
        var mockLogger = new Mock<ILogger<AuthService>>();

        var successResponse = new HttpResponseMessage(HttpStatusCode.OK);
        mockApi.Setup(x => x.PostAsync(
            "auth/send-otp",
            It.IsAny<HttpContent>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(successResponse);

        var authService = new AuthService(mockApi.Object, mockTokenStorage.Object, mockSession.Object, null!, Mock.Of<ICacheService>(), mockLogger.Object);
        string email = "";

        // Act
        bool result = await authService.SendOtpAsync(email);

        // Assert
        Assert.IsTrue(result);
        mockApi.Verify(x => x.PostAsync(
            "auth/send-otp",
            It.IsAny<HttpContent>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Tests that SendOtpAsync handles whitespace-only email string.
    /// Input: Whitespace-only string.
    /// Expected: Calls API with whitespace string, returns result based on API response.
    /// </summary>
    [TestMethod]
    public async Task SendOtpAsync_WhitespaceEmail_CallsApiAndReturnsResult()
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockTokenStorage = new Mock<TokenStorageService>();
        var mockSession = new Mock<SessionService>();
        var mockLogger = new Mock<ILogger<AuthService>>();

        var errorResponse = new HttpResponseMessage(HttpStatusCode.BadRequest);
        mockApi.Setup(x => x.PostAsync(
            "auth/send-otp",
            It.IsAny<HttpContent>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(errorResponse);

        var authService = new AuthService(mockApi.Object, mockTokenStorage.Object, mockSession.Object, null!, Mock.Of<ICacheService>(), mockLogger.Object);
        string email = "   ";

        // Act
        bool result = await authService.SendOtpAsync(email);

        // Assert
        Assert.IsFalse(result);
        mockApi.Verify(x => x.PostAsync(
            "auth/send-otp",
            It.IsAny<HttpContent>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Tests that SendOtpAsync handles very long email strings.
    /// Input: Very long string (1000 characters).
    /// Expected: Calls API with long string, returns result based on API response.
    /// </summary>
    [TestMethod]
    public async Task SendOtpAsync_VeryLongEmail_CallsApiAndReturnsResult()
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockTokenStorage = new Mock<TokenStorageService>();
        var mockSession = new Mock<SessionService>();
        var mockLogger = new Mock<ILogger<AuthService>>();

        var successResponse = new HttpResponseMessage(HttpStatusCode.OK);
        mockApi.Setup(x => x.PostAsync(
            "auth/send-otp",
            It.IsAny<HttpContent>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(successResponse);

        var authService = new AuthService(mockApi.Object, mockTokenStorage.Object, mockSession.Object, null!, Mock.Of<ICacheService>(), mockLogger.Object);
        string email = new string('a', 1000) + "@example.com";

        // Act
        bool result = await authService.SendOtpAsync(email);

        // Assert
        Assert.IsTrue(result);
        mockApi.Verify(x => x.PostAsync(
            "auth/send-otp",
            It.IsAny<HttpContent>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Tests that SendOtpAsync handles emails with special characters.
    /// Input: Email addresses with special characters and invalid formats.
    /// Expected: Calls API with the provided string (no validation), returns result based on API response.
    /// </summary>
    [TestMethod]
    [DataRow("user+tag@example.com", DisplayName = "Email with plus sign")]
    [DataRow("user@sub.domain.example.com", DisplayName = "Email with subdomain")]
    [DataRow("invalid-email", DisplayName = "Invalid email format without @")]
    [DataRow("@example.com", DisplayName = "Email without local part")]
    [DataRow("user@", DisplayName = "Email without domain")]
    [DataRow("user<script>@example.com", DisplayName = "Email with HTML tags")]
    [DataRow("user\n@example.com", DisplayName = "Email with newline character")]
    public async Task SendOtpAsync_SpecialCharactersInEmail_CallsApiAndReturnsResult(string email)
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockTokenStorage = new Mock<TokenStorageService>();
        var mockSession = new Mock<SessionService>();
        var mockLogger = new Mock<ILogger<AuthService>>();

        var successResponse = new HttpResponseMessage(HttpStatusCode.OK);
        mockApi.Setup(x => x.PostAsync(
            "auth/send-otp",
            It.IsAny<HttpContent>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(successResponse);

        var authService = new AuthService(mockApi.Object, mockTokenStorage.Object, mockSession.Object, null!, Mock.Of<ICacheService>(), mockLogger.Object);

        // Act
        bool result = await authService.SendOtpAsync(email);

        // Assert
        Assert.IsTrue(result);
        mockApi.Verify(x => x.PostAsync(
            "auth/send-otp",
            It.IsAny<HttpContent>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Tests that SendOtpAsync correctly returns true for various success status codes.
    /// Input: Valid email with different successful HTTP status codes.
    /// Expected: Returns true, no warning is logged.
    /// </summary>
    [TestMethod]
    [DataRow(HttpStatusCode.OK, DisplayName = "OK (200)")]
    [DataRow(HttpStatusCode.Created, DisplayName = "Created (201)")]
    [DataRow(HttpStatusCode.Accepted, DisplayName = "Accepted (202)")]
    [DataRow(HttpStatusCode.NoContent, DisplayName = "NoContent (204)")]
    public async Task SendOtpAsync_VariousSuccessStatusCodes_ReturnsTrue(HttpStatusCode statusCode)
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockTokenStorage = new Mock<TokenStorageService>();
        var mockSession = new Mock<SessionService>();
        var mockLogger = new Mock<ILogger<AuthService>>();

        var successResponse = new HttpResponseMessage(statusCode);
        mockApi.Setup(x => x.PostAsync(
            "auth/send-otp",
            It.IsAny<HttpContent>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(successResponse);

        var authService = new AuthService(mockApi.Object, mockTokenStorage.Object, mockSession.Object, null!, Mock.Of<ICacheService>(), mockLogger.Object);
        string email = "user@example.com";

        // Act
        bool result = await authService.SendOtpAsync(email);

        // Assert
        Assert.IsTrue(result);
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
    /// Tests that LoginAsync handles empty email string.
    /// Input: Empty email, valid password, successful API response.
    /// Expected: Processes request and returns AuthResponseDto.
    /// </summary>
    [TestMethod]
    public async Task LoginAsync_EmptyEmail_ProcessesRequest()
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockTokenStorage = new Mock<TokenStorageService>();
        var mockSession = new Mock<SessionService>();
        var mockLogger = new Mock<ILogger<AuthService>>();

        var authService = new AuthService(mockApi.Object, mockTokenStorage.Object, mockSession.Object, null!, Mock.Of<ICacheService>(), mockLogger.Object);

        var email = "";
        var password = "password123";
        var expectedDto = new AuthResponseDto
        {
            AccessToken = "access_token",
            RefreshToken = "refresh_token",
            ExpiresAt = DateTime.UtcNow.AddHours(1)
        };

        var responseContent = JsonContent.Create(expectedDto);
        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = responseContent
        };

        mockApi.Setup(x => x.PostAsync("auth/login", It.IsAny<HttpContent>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(httpResponse);

        mockTokenStorage.Setup(x => x.StoreTokensAsync(
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<DateTime?>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await authService.LoginAsync(email, password);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(expectedDto.AccessToken, result.AccessToken);
    }

    /// <summary>
    /// Tests that LoginAsync handles empty password string.
    /// Input: Valid email, empty password, successful API response.
    /// Expected: Processes request and returns AuthResponseDto.
    /// </summary>
    [TestMethod]
    public async Task LoginAsync_EmptyPassword_ProcessesRequest()
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockTokenStorage = new Mock<TokenStorageService>();
        var mockSession = new Mock<SessionService>();
        var mockLogger = new Mock<ILogger<AuthService>>();

        var authService = new AuthService(mockApi.Object, mockTokenStorage.Object, mockSession.Object, null!, Mock.Of<ICacheService>(), mockLogger.Object);

        var email = "test@example.com";
        var password = "";
        var expectedDto = new AuthResponseDto
        {
            AccessToken = "access_token",
            RefreshToken = "refresh_token",
            ExpiresAt = DateTime.UtcNow.AddHours(1)
        };

        var responseContent = JsonContent.Create(expectedDto);
        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = responseContent
        };

        mockApi.Setup(x => x.PostAsync("auth/login", It.IsAny<HttpContent>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(httpResponse);

        mockTokenStorage.Setup(x => x.StoreTokensAsync(
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<DateTime?>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await authService.LoginAsync(email, password);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(expectedDto.AccessToken, result.AccessToken);
    }

    /// <summary>
    /// Tests that LoginAsync handles whitespace-only email string.
    /// Input: Whitespace email, valid password, successful API response.
    /// Expected: Processes request and returns AuthResponseDto.
    /// </summary>
    [TestMethod]
    [DataRow("   ")]
    [DataRow("\t")]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [DataRow(" \t\n ")]
    public async Task LoginAsync_WhitespaceEmail_ProcessesRequest(string email)
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockTokenStorage = new Mock<TokenStorageService>();
        var mockSession = new Mock<SessionService>();
        var mockLogger = new Mock<ILogger<AuthService>>();

        var authService = new AuthService(mockApi.Object, mockTokenStorage.Object, mockSession.Object, null!, Mock.Of<ICacheService>(), mockLogger.Object);

        var password = "password123";
        var expectedDto = new AuthResponseDto
        {
            AccessToken = "access_token",
            RefreshToken = "refresh_token",
            ExpiresAt = DateTime.UtcNow.AddHours(1)
        };

        var responseContent = JsonContent.Create(expectedDto);
        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = responseContent
        };

        mockApi.Setup(x => x.PostAsync("auth/login", It.IsAny<HttpContent>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(httpResponse);

        mockTokenStorage.Setup(x => x.StoreTokensAsync(
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<DateTime?>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await authService.LoginAsync(email, password);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(expectedDto.AccessToken, result.AccessToken);
    }

    /// <summary>
    /// Tests that LoginAsync handles whitespace-only password string.
    /// Input: Valid email, whitespace password, successful API response.
    /// Expected: Processes request and returns AuthResponseDto.
    /// </summary>
    [TestMethod]
    [DataRow("   ")]
    [DataRow("\t")]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [DataRow(" \t\n ")]
    public async Task LoginAsync_WhitespacePassword_ProcessesRequest(string password)
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockTokenStorage = new Mock<TokenStorageService>();
        var mockSession = new Mock<SessionService>();
        var mockLogger = new Mock<ILogger<AuthService>>();

        var authService = new AuthService(mockApi.Object, mockTokenStorage.Object, mockSession.Object, null!, Mock.Of<ICacheService>(), mockLogger.Object);

        var email = "test@example.com";
        var expectedDto = new AuthResponseDto
        {
            AccessToken = "access_token",
            RefreshToken = "refresh_token",
            ExpiresAt = DateTime.UtcNow.AddHours(1)
        };

        var responseContent = JsonContent.Create(expectedDto);
        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = responseContent
        };

        mockApi.Setup(x => x.PostAsync("auth/login", It.IsAny<HttpContent>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(httpResponse);

        mockTokenStorage.Setup(x => x.StoreTokensAsync(
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<DateTime?>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await authService.LoginAsync(email, password);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(expectedDto.AccessToken, result.AccessToken);
    }

    /// <summary>
    /// Tests that LoginAsync handles both empty email and password.
    /// Input: Empty email and password, successful API response.
    /// Expected: Processes request and returns AuthResponseDto.
    /// </summary>
    [TestMethod]
    public async Task LoginAsync_EmptyEmailAndPassword_ProcessesRequest()
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockTokenStorage = new Mock<TokenStorageService>();
        var mockSession = new Mock<SessionService>();
        var mockLogger = new Mock<ILogger<AuthService>>();

        var authService = new AuthService(mockApi.Object, mockTokenStorage.Object, mockSession.Object, null!, Mock.Of<ICacheService>(), mockLogger.Object);

        var email = "";
        var password = "";
        var expectedDto = new AuthResponseDto
        {
            AccessToken = "access_token",
            RefreshToken = "refresh_token",
            ExpiresAt = DateTime.UtcNow.AddHours(1)
        };

        var responseContent = JsonContent.Create(expectedDto);
        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = responseContent
        };

        mockApi.Setup(x => x.PostAsync("auth/login", It.IsAny<HttpContent>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(httpResponse);

        mockTokenStorage.Setup(x => x.StoreTokensAsync(
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<DateTime?>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await authService.LoginAsync(email, password);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(expectedDto.AccessToken, result.AccessToken);
    }

    /// <summary>
    /// Tests that LogoutAsync calls dependencies in the correct order before encountering static Shell.Current.
    /// Input: All mockable dependencies are configured successfully.
    /// Expected: Logger is called first, then Session.Clear, then TokenStorage.ClearAsync in sequence.
    /// </summary>
    [TestMethod]
    public async Task LogoutAsync_ValidCall_CallsDependenciesInCorrectOrder()
    {
        // Arrange
        var mockApiService = new Mock<IApiService>();
        var mockTokenStorage = new Mock<TokenStorageService>(MockBehavior.Loose, Mock.Of<ILogger<TokenStorageService>>());
        var mockSession = new Mock<SessionService>(MockBehavior.Loose);
        var mockLogger = new Mock<ILogger<AuthService>>();

        var callOrder = new System.Collections.Generic.List<string>();

        mockLogger.Setup(l => l.Log(
                It.IsAny<LogLevel>(),
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()))
            .Callback(() => callOrder.Add("Logger"));

        mockSession.Setup(s => s.Clear())
            .Callback(() => callOrder.Add("Session.Clear"));

        mockTokenStorage.Setup(ts => ts.ClearAsync())
            .Callback(() => callOrder.Add("TokenStorage.ClearAsync"))
            .Returns(Task.CompletedTask);

        var authService = new AuthService(
            mockApiService.Object,
            mockTokenStorage.Object,
            mockSession.Object,
            new PollingService(Mock.Of<IRefreshCoordinator>(), Mock.Of<ILogger<PollingService>>()),
            Mock.Of<ICacheService>(),
            mockLogger.Object);

        // Act
        try
        {
            await authService.LogoutAsync();
        }
        catch (NullReferenceException)
        {
            // Expected: Shell.Current is null in unit test context
        }
        catch (InvalidOperationException)
        {
            // Expected: Shell.Current might throw InvalidOperationException
        }

        // Assert
        Assert.AreEqual(3, callOrder.Count, "All three operations should be called");
        Assert.AreEqual("Logger", callOrder[0], "Logger should be called first");
        Assert.AreEqual("Session.Clear", callOrder[1], "Session.Clear should be called second");
        Assert.AreEqual("TokenStorage.ClearAsync", callOrder[2], "TokenStorage.ClearAsync should be called third");
    }

    /// <summary>
    /// Tests that TryRefreshTokenAsync does not call PostAsync when refresh token is null.
    /// Input: GetRefreshTokenAsync returns null.
    /// Expected: Returns false, PostAsync is never called.
    /// </summary>
    [TestMethod]
    public async Task TryRefreshTokenAsync_RefreshTokenIsNull_DoesNotCallPostAsync()
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockTokenStorage = new Mock<TokenStorageService>();
        var mockSession = new Mock<SessionService>();
        var mockLogger = new Mock<ILogger<AuthService>>();

        mockTokenStorage.Setup(x => x.GetRefreshTokenAsync()).ReturnsAsync((string?)null);

        var authService = new AuthService(mockApi.Object, mockTokenStorage.Object, mockSession.Object, null!, Mock.Of<ICacheService>(), mockLogger.Object);

        // Act
        var result = await authService.TryRefreshTokenAsync();

        // Assert
        Assert.IsFalse(result);
        mockApi.Verify(x => x.PostAsync(It.IsAny<string>(), It.IsAny<HttpContent>(), It.IsAny<CancellationToken>()), Times.Never);
        mockTokenStorage.Verify(x => x.StoreTokensAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTime?>()), Times.Never);
    }

    /// <summary>
    /// Tests that TryRefreshTokenAsync does not call PostAsync when refresh token is empty.
    /// Input: GetRefreshTokenAsync returns empty string.
    /// Expected: Returns false, PostAsync is never called.
    /// </summary>
    [TestMethod]
    public async Task TryRefreshTokenAsync_RefreshTokenIsEmpty_DoesNotCallPostAsync()
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockTokenStorage = new Mock<TokenStorageService>();
        var mockSession = new Mock<SessionService>();
        var mockLogger = new Mock<ILogger<AuthService>>();

        mockTokenStorage.Setup(x => x.GetRefreshTokenAsync()).ReturnsAsync(string.Empty);

        var authService = new AuthService(mockApi.Object, mockTokenStorage.Object, mockSession.Object, null!, Mock.Of<ICacheService>(), mockLogger.Object);

        // Act
        var result = await authService.TryRefreshTokenAsync();

        // Assert
        Assert.IsFalse(result);
        mockApi.Verify(x => x.PostAsync(It.IsAny<string>(), It.IsAny<HttpContent>(), It.IsAny<CancellationToken>()), Times.Never);
        mockTokenStorage.Verify(x => x.StoreTokensAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTime?>()), Times.Never);
    }

    /// <summary>
    /// Tests that TryRefreshTokenAsync calls PostAsync with correct endpoint.
    /// Input: Valid refresh token.
    /// Expected: PostAsync is called with "auth/refresh" endpoint.
    /// </summary>
    [TestMethod]
    public async Task TryRefreshTokenAsync_ValidRefreshToken_CallsPostAsyncWithCorrectEndpoint()
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockTokenStorage = new Mock<TokenStorageService>();
        var mockSession = new Mock<SessionService>();
        var mockLogger = new Mock<ILogger<AuthService>>();

        var refreshToken = "valid-refresh-token";
        mockTokenStorage.Setup(x => x.GetRefreshTokenAsync()).ReturnsAsync(refreshToken);

        var responseDto = new AuthResponseDto
        {
            AccessToken = "new-access-token",
            RefreshToken = "new-refresh-token",
            ExpiresAt = DateTime.UtcNow.AddHours(1)
        };

        var responseMessage = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(responseDto)
        };

        mockApi.Setup(x => x.PostAsync("auth/refresh", It.IsAny<HttpContent>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(responseMessage);

        var authService = new AuthService(mockApi.Object, mockTokenStorage.Object, mockSession.Object, null!, Mock.Of<ICacheService>(), mockLogger.Object);

        // Act
        var result = await authService.TryRefreshTokenAsync();

        // Assert
        Assert.IsTrue(result);
        mockApi.Verify(x => x.PostAsync("auth/refresh", It.IsAny<HttpContent>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Tests that TryRefreshTokenAsync handles null ExpiresAt in response.
    /// Input: Response DTO with default DateTime value for ExpiresAt.
    /// Expected: Returns true, stores tokens with default DateTime.
    /// </summary>
    [TestMethod]
    public async Task TryRefreshTokenAsync_ResponseWithDefaultExpiresAt_StoresTokensSuccessfully()
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockTokenStorage = new Mock<TokenStorageService>();
        var mockSession = new Mock<SessionService>();
        var mockLogger = new Mock<ILogger<AuthService>>();

        var refreshToken = "valid-refresh-token";
        mockTokenStorage.Setup(x => x.GetRefreshTokenAsync()).ReturnsAsync(refreshToken);

        var responseDto = new AuthResponseDto
        {
            AccessToken = "new-access-token",
            RefreshToken = "new-refresh-token",
            ExpiresAt = default(DateTime)
        };

        var responseMessage = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(responseDto)
        };

        mockApi.Setup(x => x.PostAsync(It.IsAny<string>(), It.IsAny<HttpContent>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(responseMessage);

        var authService = new AuthService(mockApi.Object, mockTokenStorage.Object, mockSession.Object, null!, Mock.Of<ICacheService>(), mockLogger.Object);

        // Act
        var result = await authService.TryRefreshTokenAsync();

        // Assert
        Assert.IsTrue(result);
        mockTokenStorage.Verify(x => x.StoreTokensAsync("new-access-token", "new-refresh-token", default(DateTime)), Times.Once);
    }

    /// <summary>
    /// Tests that TryRefreshTokenAsync does not store tokens when response is not successful.
    /// Input: API returns non-success status code.
    /// Expected: Returns false, StoreTokensAsync is never called.
    /// </summary>
    [TestMethod]
    [DataRow(HttpStatusCode.BadRequest)]
    [DataRow(HttpStatusCode.Unauthorized)]
    [DataRow(HttpStatusCode.InternalServerError)]
    public async Task TryRefreshTokenAsync_NonSuccessResponse_DoesNotStoreTokens(HttpStatusCode statusCode)
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockTokenStorage = new Mock<TokenStorageService>();
        var mockSession = new Mock<SessionService>();
        var mockLogger = new Mock<ILogger<AuthService>>();

        var refreshToken = "valid-refresh-token";
        mockTokenStorage.Setup(x => x.GetRefreshTokenAsync()).ReturnsAsync(refreshToken);

        var responseMessage = new HttpResponseMessage(statusCode);

        mockApi.Setup(x => x.PostAsync(It.IsAny<string>(), It.IsAny<HttpContent>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(responseMessage);

        var authService = new AuthService(mockApi.Object, mockTokenStorage.Object, mockSession.Object, null!, Mock.Of<ICacheService>(), mockLogger.Object);

        // Act
        var result = await authService.TryRefreshTokenAsync();

        // Assert
        Assert.IsFalse(result);
        mockTokenStorage.Verify(x => x.StoreTokensAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTime?>()), Times.Never);
    }

    /// <summary>
    /// Tests that TryRefreshTokenAsync does not store tokens when response content is null.
    /// Input: Successful HTTP response but deserialization returns null.
    /// Expected: Returns false, StoreTokensAsync is never called.
    /// </summary>
    [TestMethod]
    public async Task TryRefreshTokenAsync_NullResponseDto_DoesNotStoreTokens()
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockTokenStorage = new Mock<TokenStorageService>();
        var mockSession = new Mock<SessionService>();
        var mockLogger = new Mock<ILogger<AuthService>>();

        var refreshToken = "valid-refresh-token";
        mockTokenStorage.Setup(x => x.GetRefreshTokenAsync()).ReturnsAsync(refreshToken);

        var responseMessage = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("null", Encoding.UTF8, "application/json")
        };

        mockApi.Setup(x => x.PostAsync(It.IsAny<string>(), It.IsAny<HttpContent>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(responseMessage);

        var authService = new AuthService(mockApi.Object, mockTokenStorage.Object, mockSession.Object, null!, Mock.Of<ICacheService>(), mockLogger.Object);

        // Act
        var result = await authService.TryRefreshTokenAsync();

        // Assert
        Assert.IsFalse(result);
        mockTokenStorage.Verify(x => x.StoreTokensAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTime?>()), Times.Never);
    }

    /// <summary>
    /// Tests that TryRefreshTokenAsync logs warning with correct status code on failure.
    /// Input: API returns various non-success status codes.
    /// Expected: Warning is logged with the exact status code.
    /// </summary>
    [TestMethod]
    [DataRow(HttpStatusCode.BadRequest)]
    [DataRow(HttpStatusCode.Forbidden)]
    [DataRow(HttpStatusCode.NotFound)]
    public async Task TryRefreshTokenAsync_NonSuccessResponse_LogsWarningWithStatusCode(HttpStatusCode statusCode)
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockTokenStorage = new Mock<TokenStorageService>();
        var mockSession = new Mock<SessionService>();
        var mockLogger = new Mock<ILogger<AuthService>>();

        var refreshToken = "valid-refresh-token";
        mockTokenStorage.Setup(x => x.GetRefreshTokenAsync()).ReturnsAsync(refreshToken);

        var responseMessage = new HttpResponseMessage(statusCode);

        mockApi.Setup(x => x.PostAsync(It.IsAny<string>(), It.IsAny<HttpContent>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(responseMessage);

        var authService = new AuthService(mockApi.Object, mockTokenStorage.Object, mockSession.Object, null!, Mock.Of<ICacheService>(), mockLogger.Object);

        // Act
        var result = await authService.TryRefreshTokenAsync();

        // Assert
        Assert.IsFalse(result);
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Token refresh returned")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    /// <summary>
    /// Tests that TryRefreshTokenAsync stores exact token values from response.
    /// Input: Response with specific access and refresh token values.
    /// Expected: StoreTokensAsync is called with exact values from response.
    /// </summary>
    [TestMethod]
    public async Task TryRefreshTokenAsync_SuccessfulResponse_StoresExactTokenValues()
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockTokenStorage = new Mock<TokenStorageService>();
        var mockSession = new Mock<SessionService>();
        var mockLogger = new Mock<ILogger<AuthService>>();

        var refreshToken = "old-refresh-token";
        mockTokenStorage.Setup(x => x.GetRefreshTokenAsync()).ReturnsAsync(refreshToken);

        var expectedAccessToken = "specific-access-token-12345";
        var expectedRefreshToken = "specific-refresh-token-67890";
        var expectedExpiresAt = new DateTime(2025, 6, 15, 10, 30, 0);

        var responseDto = new AuthResponseDto
        {
            AccessToken = expectedAccessToken,
            RefreshToken = expectedRefreshToken,
            ExpiresAt = expectedExpiresAt
        };

        var responseMessage = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(responseDto)
        };

        mockApi.Setup(x => x.PostAsync(It.IsAny<string>(), It.IsAny<HttpContent>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(responseMessage);

        var authService = new AuthService(mockApi.Object, mockTokenStorage.Object, mockSession.Object, null!, Mock.Of<ICacheService>(), mockLogger.Object);

        // Act
        var result = await authService.TryRefreshTokenAsync();

        // Assert
        Assert.IsTrue(result);
        mockTokenStorage.Verify(x => x.StoreTokensAsync(expectedAccessToken, expectedRefreshToken, expectedExpiresAt), Times.Once);
    }

    /// <summary>
    /// Tests that TryRefreshTokenAsync handles response with whitespace-only tokens.
    /// Input: Response DTO with whitespace-only AccessToken and RefreshToken.
    /// Expected: Returns true, stores whitespace tokens.
    /// </summary>
    [TestMethod]
    public async Task TryRefreshTokenAsync_ResponseWithWhitespaceTokens_StoresWhitespaceTokens()
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockTokenStorage = new Mock<TokenStorageService>();
        var mockSession = new Mock<SessionService>();
        var mockLogger = new Mock<ILogger<AuthService>>();

        var refreshToken = "valid-refresh-token";
        mockTokenStorage.Setup(x => x.GetRefreshTokenAsync()).ReturnsAsync(refreshToken);

        var responseDto = new AuthResponseDto
        {
            AccessToken = "   ",
            RefreshToken = "\t\n",
            ExpiresAt = DateTime.UtcNow.AddHours(1)
        };

        var responseMessage = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(responseDto)
        };

        mockApi.Setup(x => x.PostAsync(It.IsAny<string>(), It.IsAny<HttpContent>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(responseMessage);

        var authService = new AuthService(mockApi.Object, mockTokenStorage.Object, mockSession.Object, null!, Mock.Of<ICacheService>(), mockLogger.Object);

        // Act
        var result = await authService.TryRefreshTokenAsync();

        // Assert
        Assert.IsTrue(result);
        mockTokenStorage.Verify(x => x.StoreTokensAsync("   ", "\t\n", It.IsAny<DateTime>()), Times.Once);
    }

    /// <summary>
    /// Tests that TryRefreshTokenAsync handles response with very long token strings.
    /// Input: Response DTO with very long AccessToken and RefreshToken strings.
    /// Expected: Returns true, stores long tokens successfully.
    /// </summary>
    [TestMethod]
    public async Task TryRefreshTokenAsync_ResponseWithVeryLongTokens_StoresLongTokens()
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockTokenStorage = new Mock<TokenStorageService>();
        var mockSession = new Mock<SessionService>();
        var mockLogger = new Mock<ILogger<AuthService>>();

        var refreshToken = "valid-refresh-token";
        mockTokenStorage.Setup(x => x.GetRefreshTokenAsync()).ReturnsAsync(refreshToken);

        var longAccessToken = new string('A', 10000);
        var longRefreshToken = new string('R', 10000);

        var responseDto = new AuthResponseDto
        {
            AccessToken = longAccessToken,
            RefreshToken = longRefreshToken,
            ExpiresAt = DateTime.UtcNow.AddHours(1)
        };

        var responseMessage = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(responseDto)
        };

        mockApi.Setup(x => x.PostAsync(It.IsAny<string>(), It.IsAny<HttpContent>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(responseMessage);

        var authService = new AuthService(mockApi.Object, mockTokenStorage.Object, mockSession.Object, null!, Mock.Of<ICacheService>(), mockLogger.Object);

        // Act
        var result = await authService.TryRefreshTokenAsync();

        // Assert
        Assert.IsTrue(result);
        mockTokenStorage.Verify(x => x.StoreTokensAsync(longAccessToken, longRefreshToken, It.IsAny<DateTime>()), Times.Once);
    }

    /// <summary>
    /// Tests that TryRefreshTokenAsync handles response with special characters in tokens.
    /// Input: Response DTO with special characters in AccessToken and RefreshToken.
    /// Expected: Returns true, stores tokens with special characters.
    /// </summary>
    [TestMethod]
    [DataRow("token!@#$%^&*()", "refresh!@#$%^&*()")]
    [DataRow("token+with+plus", "refresh-with-dash")]
    [DataRow("token.with.dots", "refresh_with_underscores")]
    public async Task TryRefreshTokenAsync_ResponseWithSpecialCharactersInTokens_StoresSpecialCharacterTokens(string accessToken, string refreshToken)
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockTokenStorage = new Mock<TokenStorageService>();
        var mockSession = new Mock<SessionService>();
        var mockLogger = new Mock<ILogger<AuthService>>();

        var oldRefreshToken = "old-refresh-token";
        mockTokenStorage.Setup(x => x.GetRefreshTokenAsync()).ReturnsAsync(oldRefreshToken);

        var responseDto = new AuthResponseDto
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            ExpiresAt = DateTime.UtcNow.AddHours(1)
        };

        var responseMessage = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(responseDto)
        };

        mockApi.Setup(x => x.PostAsync(It.IsAny<string>(), It.IsAny<HttpContent>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(responseMessage);

        var authService = new AuthService(mockApi.Object, mockTokenStorage.Object, mockSession.Object, null!, Mock.Of<ICacheService>(), mockLogger.Object);

        // Act
        var result = await authService.TryRefreshTokenAsync();

        // Assert
        Assert.IsTrue(result);
        mockTokenStorage.Verify(x => x.StoreTokensAsync(accessToken, refreshToken, It.IsAny<DateTime>()), Times.Once);
    }

    /// <summary>
    /// Tests that TryRefreshTokenAsync does not log warning on successful response.
    /// Input: API returns successful status code.
    /// Expected: No warning is logged.
    /// </summary>
    [TestMethod]
    public async Task TryRefreshTokenAsync_SuccessfulResponse_DoesNotLogWarning()
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockTokenStorage = new Mock<TokenStorageService>();
        var mockSession = new Mock<SessionService>();
        var mockLogger = new Mock<ILogger<AuthService>>();

        var refreshToken = "valid-refresh-token";
        mockTokenStorage.Setup(x => x.GetRefreshTokenAsync()).ReturnsAsync(refreshToken);

        var responseDto = new AuthResponseDto
        {
            AccessToken = "new-access-token",
            RefreshToken = "new-refresh-token",
            ExpiresAt = DateTime.UtcNow.AddHours(1)
        };

        var responseMessage = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(responseDto)
        };

        mockApi.Setup(x => x.PostAsync(It.IsAny<string>(), It.IsAny<HttpContent>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(responseMessage);

        var authService = new AuthService(mockApi.Object, mockTokenStorage.Object, mockSession.Object, null!, Mock.Of<ICacheService>(), mockLogger.Object);

        // Act
        var result = await authService.TryRefreshTokenAsync();

        // Assert
        Assert.IsTrue(result);
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
    /// Tests that TryRefreshTokenAsync handles various successful HTTP status codes.
    /// Input: API returns different 2xx status codes.
    /// Expected: Returns true for all successful status codes.
    /// </summary>
    [TestMethod]
    [DataRow(HttpStatusCode.OK)]
    [DataRow(HttpStatusCode.Created)]
    [DataRow(HttpStatusCode.Accepted)]
    [DataRow(HttpStatusCode.NoContent)]
    public async Task TryRefreshTokenAsync_VariousSuccessStatusCodes_ReturnsTrue(HttpStatusCode statusCode)
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockTokenStorage = new Mock<TokenStorageService>();
        var mockSession = new Mock<SessionService>();
        var mockLogger = new Mock<ILogger<AuthService>>();

        var refreshToken = "valid-refresh-token";
        mockTokenStorage.Setup(x => x.GetRefreshTokenAsync()).ReturnsAsync(refreshToken);

        var responseDto = new AuthResponseDto
        {
            AccessToken = "new-access-token",
            RefreshToken = "new-refresh-token",
            ExpiresAt = DateTime.UtcNow.AddHours(1)
        };

        var responseMessage = new HttpResponseMessage(statusCode)
        {
            Content = JsonContent.Create(responseDto)
        };

        mockApi.Setup(x => x.PostAsync(It.IsAny<string>(), It.IsAny<HttpContent>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(responseMessage);

        var authService = new AuthService(mockApi.Object, mockTokenStorage.Object, mockSession.Object, null!, Mock.Of<ICacheService>(), mockLogger.Object);

        // Act
        var result = await authService.TryRefreshTokenAsync();

        // Assert
        Assert.IsTrue(result);
    }

    /// <summary>
    /// Tests that VerifyOtpAsync returns null when the API response content deserializes to null.
    /// Input: Valid email and OTP code with successful response but null deserialization.
    /// Expected: Returns null.
    /// </summary>
    [TestMethod]
    public async Task VerifyOtpAsync_DeserializationReturnsNull_ReturnsNull()
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockTokenStorage = new Mock<TokenStorageService>();
        var mockSession = new Mock<SessionService>();
        var mockLogger = new Mock<ILogger<AuthService>>();
        var authService = new AuthService(mockApi.Object, mockTokenStorage.Object, mockSession.Object, null!, Mock.Of<ICacheService>(), mockLogger.Object);

        string email = "test@example.com";
        string otpCode = "123456";

        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("null", Encoding.UTF8, "application/json")
        };

        mockApi.Setup(x => x.PostAsync(
            "auth/verify-otp",
            It.IsAny<HttpContent>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(httpResponse);

        // Act
        var result = await authService.VerifyOtpAsync(email, otpCode);

        // Assert
        Assert.IsNull(result);
    }

    /// <summary>
    /// Tests that VerifyOtpAsync returns null when VerifyOtpResponseDto has null VerificationToken.
    /// Input: Valid email and OTP code with successful response but null token.
    /// Expected: Returns null.
    /// </summary>
    [TestMethod]
    public async Task VerifyOtpAsync_ResponseWithNullVerificationToken_ReturnsNull()
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockTokenStorage = new Mock<TokenStorageService>();
        var mockSession = new Mock<SessionService>();
        var mockLogger = new Mock<ILogger<AuthService>>();
        var authService = new AuthService(mockApi.Object, mockTokenStorage.Object, mockSession.Object, null!, Mock.Of<ICacheService>(), mockLogger.Object);

        string email = "test@example.com";
        string otpCode = "123456";

        var responseDto = new VerifyOtpResponseDto
        {
            Message = "Success",
            VerificationToken = null
        };
        var jsonContent = JsonContent.Create(responseDto);
        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = jsonContent
        };

        mockApi.Setup(x => x.PostAsync(
            "auth/verify-otp",
            It.IsAny<HttpContent>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(httpResponse);

        // Act
        var result = await authService.VerifyOtpAsync(email, otpCode);

        // Assert
        Assert.IsNull(result);
    }

    /// <summary>
    /// Tests that VerifyOtpAsync returns empty string when VerifyOtpResponseDto has empty VerificationToken.
    /// Input: Valid email and OTP code with successful response but empty token.
    /// Expected: Returns empty string.
    /// </summary>
    [TestMethod]
    public async Task VerifyOtpAsync_ResponseWithEmptyVerificationToken_ReturnsEmptyString()
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockTokenStorage = new Mock<TokenStorageService>();
        var mockSession = new Mock<SessionService>();
        var mockLogger = new Mock<ILogger<AuthService>>();
        var authService = new AuthService(mockApi.Object, mockTokenStorage.Object, mockSession.Object, null!, Mock.Of<ICacheService>(), mockLogger.Object);

        string email = "test@example.com";
        string otpCode = "123456";

        var responseDto = new VerifyOtpResponseDto
        {
            Message = "Success",
            VerificationToken = string.Empty
        };
        var jsonContent = JsonContent.Create(responseDto);
        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = jsonContent
        };

        mockApi.Setup(x => x.PostAsync(
            "auth/verify-otp",
            It.IsAny<HttpContent>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(httpResponse);

        // Act
        var result = await authService.VerifyOtpAsync(email, otpCode);

        // Assert
        Assert.AreEqual(string.Empty, result);
    }

    /// <summary>
    /// Tests that VerifyOtpAsync handles malformed JSON response content.
    /// Input: Valid email and OTP code with malformed JSON response.
    /// Expected: Returns null due to deserialization failure.
    /// </summary>
    [TestMethod]
    public async Task VerifyOtpAsync_MalformedJsonContent_ReturnsNull()
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockTokenStorage = new Mock<TokenStorageService>();
        var mockSession = new Mock<SessionService>();
        var mockLogger = new Mock<ILogger<AuthService>>();
        var authService = new AuthService(mockApi.Object, mockTokenStorage.Object, mockSession.Object, null!, Mock.Of<ICacheService>(), mockLogger.Object);

        string email = "test@example.com";
        string otpCode = "123456";

        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("{invalid json}", Encoding.UTF8, "application/json")
        };

        mockApi.Setup(x => x.PostAsync(
            "auth/verify-otp",
            It.IsAny<HttpContent>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(httpResponse);

        // Act
        var result = await authService.VerifyOtpAsync(email, otpCode);

        // Assert
        Assert.IsNull(result);
    }

    /// <summary>
    /// Tests that VerifyOtpAsync returns verification token for various successful HTTP status codes.
    /// Input: Valid email and OTP code with different success status codes.
    /// Expected: Returns verification token for all success status codes.
    /// </summary>
    /// <param name="statusCode">The HTTP status code to test.</param>
    [TestMethod]
    [DataRow(HttpStatusCode.OK)]
    [DataRow(HttpStatusCode.Created)]
    [DataRow(HttpStatusCode.Accepted)]
    [DataRow(HttpStatusCode.NoContent)]
    public async Task VerifyOtpAsync_VariousSuccessStatusCodes_ReturnsVerificationToken(HttpStatusCode statusCode)
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockTokenStorage = new Mock<TokenStorageService>();
        var mockSession = new Mock<SessionService>();
        var mockLogger = new Mock<ILogger<AuthService>>();
        var authService = new AuthService(mockApi.Object, mockTokenStorage.Object, mockSession.Object, null!, Mock.Of<ICacheService>(), mockLogger.Object);

        string email = "test@example.com";
        string otpCode = "123456";
        string expectedToken = "verification-token-12345";

        var responseDto = new VerifyOtpResponseDto
        {
            Message = "Success",
            VerificationToken = expectedToken
        };
        var jsonContent = JsonContent.Create(responseDto);
        var httpResponse = new HttpResponseMessage(statusCode)
        {
            Content = jsonContent
        };

        mockApi.Setup(x => x.PostAsync(
            "auth/verify-otp",
            It.IsAny<HttpContent>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(httpResponse);

        // Act
        var result = await authService.VerifyOtpAsync(email, otpCode);

        // Assert
        Assert.AreEqual(expectedToken, result);
    }

    /// <summary>
    /// Tests that VerifyOtpAsync returns very long verification tokens correctly.
    /// Input: Valid email and OTP code with response containing very long verification token.
    /// Expected: Returns the entire long token without truncation.
    /// </summary>
    [TestMethod]
    public async Task VerifyOtpAsync_VeryLongVerificationToken_ReturnsFullToken()
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockTokenStorage = new Mock<TokenStorageService>();
        var mockSession = new Mock<SessionService>();
        var mockLogger = new Mock<ILogger<AuthService>>();
        var authService = new AuthService(mockApi.Object, mockTokenStorage.Object, mockSession.Object, null!, Mock.Of<ICacheService>(), mockLogger.Object);

        string email = "test@example.com";
        string otpCode = "123456";
        string expectedToken = new string('a', 5000) + "-token-" + new string('b', 5000);

        var responseDto = new VerifyOtpResponseDto
        {
            Message = "Success",
            VerificationToken = expectedToken
        };
        var jsonContent = JsonContent.Create(responseDto);
        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = jsonContent
        };

        mockApi.Setup(x => x.PostAsync(
            "auth/verify-otp",
            It.IsAny<HttpContent>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(httpResponse);

        // Act
        var result = await authService.VerifyOtpAsync(email, otpCode);

        // Assert
        Assert.AreEqual(expectedToken, result);
        Assert.AreEqual(expectedToken.Length, result?.Length);
    }

    /// <summary>
    /// Tests that RegisterStudentAsync returns true when the API returns various success status codes.
    /// Input: Valid DTO with different successful HTTP status codes.
    /// Expected: Returns true for all success status codes.
    /// </summary>
    /// <param name="statusCode">The HTTP status code returned by the API.</param>
    [TestMethod]
    [DataRow(200)] // OK
    [DataRow(201)] // Created
    [DataRow(202)] // Accepted
    [DataRow(204)] // NoContent
    public async Task RegisterStudentAsync_VariousSuccessStatusCodes_ReturnsTrue(int statusCode)
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockTokenStorage = new Mock<TokenStorageService>();
        var mockSession = new Mock<SessionService>();
        var mockLogger = new Mock<ILogger<AuthService>>();

        var dto = new StudentRegisterDto
        {
            Email = "test@example.com",
            FirstName = "Jane",
            SecondName = "Smith"
        };

        var successResponse = new HttpResponseMessage((HttpStatusCode)statusCode);
        mockApi.Setup(x => x.PostAsync("auth/register", It.IsAny<HttpContent>(), It.IsAny<CancellationToken>()))
               .ReturnsAsync(successResponse);

        var authService = new AuthService(mockApi.Object, mockTokenStorage.Object, mockSession.Object, null!, Mock.Of<ICacheService>(), mockLogger.Object);

        // Act
        var result = await authService.RegisterStudentAsync(dto);

        // Assert
        Assert.IsTrue(result);
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
    /// Tests that RegisterStudentAsync handles DTO with nullable properties set to null.
    /// Input: DTO with OtherNames and Photo set to null.
    /// Expected: Processes successfully without errors.
    /// </summary>
    [TestMethod]
    public async Task RegisterStudentAsync_NullablePropertiesSetToNull_ProcessesSuccessfully()
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockTokenStorage = new Mock<TokenStorageService>();
        var mockSession = new Mock<SessionService>();
        var mockLogger = new Mock<ILogger<AuthService>>();

        var dto = new StudentRegisterDto
        {
            Email = "test@example.com",
            FirstName = "John",
            SecondName = "Doe",
            OtherNames = null,
            Photo = null
        };

        var successResponse = new HttpResponseMessage(HttpStatusCode.OK);
        mockApi.Setup(x => x.PostAsync("auth/register", It.IsAny<HttpContent>(), It.IsAny<CancellationToken>()))
               .ReturnsAsync(successResponse);

        var authService = new AuthService(mockApi.Object, mockTokenStorage.Object, mockSession.Object, null!, Mock.Of<ICacheService>(), mockLogger.Object);

        // Act
        var result = await authService.RegisterStudentAsync(dto);

        // Assert
        Assert.IsTrue(result);
    }

    /// <summary>
    /// Tests that RegisterStudentAsync handles DTO with all string properties as empty strings.
    /// Input: DTO with all non-nullable string properties set to empty strings.
    /// Expected: Processes successfully without errors.
    /// </summary>
    [TestMethod]
    public async Task RegisterStudentAsync_AllEmptyStringProperties_ProcessesSuccessfully()
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockTokenStorage = new Mock<TokenStorageService>();
        var mockSession = new Mock<SessionService>();
        var mockLogger = new Mock<ILogger<AuthService>>();

        var dto = new StudentRegisterDto
        {
            FirstName = "",
            SecondName = "",
            Dob = "",
            Gender = "",
            Phone = "",
            Email = "",
            Password = "",
            VerificationToken = "",
            UniversityId = "",
            FacultyId = "",
            DepartmentId = "",
            ProgramId = "",
            EntrySchemeId = "",
            IntakeId = "",
            StudyModeId = "",
            AcademicYearId = "",
            SemesterId = ""
        };

        var successResponse = new HttpResponseMessage(HttpStatusCode.OK);
        mockApi.Setup(x => x.PostAsync("auth/register", It.IsAny<HttpContent>(), It.IsAny<CancellationToken>()))
               .ReturnsAsync(successResponse);

        var authService = new AuthService(mockApi.Object, mockTokenStorage.Object, mockSession.Object, null!, Mock.Of<ICacheService>(), mockLogger.Object);

        // Act
        var result = await authService.RegisterStudentAsync(dto);

        // Assert
        Assert.IsTrue(result);
    }

    /// <summary>
    /// Tests that RegisterStudentAsync handles DTO with very long strings in all properties.
    /// Input: DTO with very long strings (1000+ characters) in all string properties.
    /// Expected: Processes successfully without errors.
    /// </summary>
    [TestMethod]
    public async Task RegisterStudentAsync_VeryLongStringsInAllProperties_ProcessesSuccessfully()
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockTokenStorage = new Mock<TokenStorageService>();
        var mockSession = new Mock<SessionService>();
        var mockLogger = new Mock<ILogger<AuthService>>();

        var longString = new string('x', 1000);
        var dto = new StudentRegisterDto
        {
            FirstName = longString,
            SecondName = longString,
            OtherNames = longString,
            Dob = longString,
            Gender = longString,
            Phone = longString,
            Email = longString,
            Password = longString,
            Photo = longString,
            VerificationToken = longString,
            UniversityId = longString,
            FacultyId = longString,
            DepartmentId = longString,
            ProgramId = longString,
            EntrySchemeId = longString,
            IntakeId = longString,
            StudyModeId = longString,
            AcademicYearId = longString,
            SemesterId = longString
        };

        var successResponse = new HttpResponseMessage(HttpStatusCode.OK);
        mockApi.Setup(x => x.PostAsync("auth/register", It.IsAny<HttpContent>(), It.IsAny<CancellationToken>()))
               .ReturnsAsync(successResponse);

        var authService = new AuthService(mockApi.Object, mockTokenStorage.Object, mockSession.Object, null!, Mock.Of<ICacheService>(), mockLogger.Object);

        // Act
        var result = await authService.RegisterStudentAsync(dto);

        // Assert
        Assert.IsTrue(result);
    }

    /// <summary>
    /// Tests that RegisterStudentAsync handles DTO with special characters in string properties.
    /// Input: DTO with special characters including HTML, SQL injection patterns, and Unicode.
    /// Expected: Processes successfully without errors.
    /// </summary>
    [TestMethod]
    public async Task RegisterStudentAsync_SpecialCharactersInProperties_ProcessesSuccessfully()
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockTokenStorage = new Mock<TokenStorageService>();
        var mockSession = new Mock<SessionService>();
        var mockLogger = new Mock<ILogger<AuthService>>();

        var dto = new StudentRegisterDto
        {
            Email = "test@example.com",
            FirstName = "<script>alert('test')</script>",
            SecondName = "'; DROP TABLE Students; --",
            OtherNames = "Ã¦Âµâ€¹Ã¨Â¯â€¢Ã§â€Â¨Ã¦Ë†Â·",
            Phone = "+1-555-123-4567"
        };

        var successResponse = new HttpResponseMessage(HttpStatusCode.OK);
        mockApi.Setup(x => x.PostAsync("auth/register", It.IsAny<HttpContent>(), It.IsAny<CancellationToken>()))
               .ReturnsAsync(successResponse);

        var authService = new AuthService(mockApi.Object, mockTokenStorage.Object, mockSession.Object, null!, Mock.Of<ICacheService>(), mockLogger.Object);

        // Act
        var result = await authService.RegisterStudentAsync(dto);

        // Assert
        Assert.IsTrue(result);
    }

    /// <summary>
    /// Tests that RegisterStudentAsync logs the correct email in the information message.
    /// Input: DTO with specific email address.
    /// Expected: Information log contains the exact email from the DTO.
    /// </summary>
    [TestMethod]
    public async Task RegisterStudentAsync_ValidDto_LogsCorrectEmail()
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockTokenStorage = new Mock<TokenStorageService>();
        var mockSession = new Mock<SessionService>();
        var mockLogger = new Mock<ILogger<AuthService>>();

        var testEmail = "specific.test@example.com";
        var dto = new StudentRegisterDto
        {
            Email = testEmail,
            FirstName = "John",
            SecondName = "Doe"
        };

        var successResponse = new HttpResponseMessage(HttpStatusCode.OK);
        mockApi.Setup(x => x.PostAsync("auth/register", It.IsAny<HttpContent>(), It.IsAny<CancellationToken>()))
               .ReturnsAsync(successResponse);

        var authService = new AuthService(mockApi.Object, mockTokenStorage.Object, mockSession.Object, null!, Mock.Of<ICacheService>(), mockLogger.Object);

        // Act
        var result = await authService.RegisterStudentAsync(dto);

        // Assert
        Assert.IsTrue(result);
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Student registration for") && v.ToString()!.Contains(testEmail)),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    /// <summary>
    /// Tests that RegisterStudentAsync logs the correct status code in the warning message when API fails.
    /// Input: DTO with failed API response and specific status code.
    /// Expected: Warning log contains the exact status code from the response.
    /// </summary>
    [TestMethod]
    public async Task RegisterStudentAsync_FailedResponse_LogsCorrectStatusCode()
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockTokenStorage = new Mock<TokenStorageService>();
        var mockSession = new Mock<SessionService>();
        var mockLogger = new Mock<ILogger<AuthService>>();

        var dto = new StudentRegisterDto
        {
            Email = "test@example.com",
            FirstName = "John",
            SecondName = "Doe"
        };

        var failureResponse = new HttpResponseMessage(HttpStatusCode.BadRequest);
        mockApi.Setup(x => x.PostAsync("auth/register", It.IsAny<HttpContent>(), It.IsAny<CancellationToken>()))
               .ReturnsAsync(failureResponse);

        var authService = new AuthService(mockApi.Object, mockTokenStorage.Object, mockSession.Object, null!, Mock.Of<ICacheService>(), mockLogger.Object);

        // Act
        var result = await authService.RegisterStudentAsync(dto);

        // Assert
        Assert.IsFalse(result);
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Student registration failed") && v.ToString()!.Contains("400")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    /// <summary>
    /// Tests that TryRefreshTokenAsync handles a mix of null and valid token properties in response.
    /// Input: Successful API response with null AccessToken but valid RefreshToken.
    /// Expected: Returns true and stores tokens with null AccessToken.
    /// </summary>
    [TestMethod]
    public async Task TryRefreshTokenAsync_ResponseWithNullAccessTokenButValidRefreshToken_StoresTokensWithNullAccessToken()
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockTokenStorage = new Mock<TokenStorageService>();
        var mockSession = new Mock<SessionService>();
        var mockLogger = new Mock<ILogger<AuthService>>();

        mockTokenStorage.Setup(x => x.GetRefreshTokenAsync()).ReturnsAsync("valid-refresh-token");

        var responseDto = new AuthResponseDto
        {
            AccessToken = null!,
            RefreshToken = "new-refresh-token",
            ExpiresAt = DateTime.UtcNow.AddHours(1)
        };

        var responseMessage = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(responseDto)
        };

        mockApi.Setup(x => x.PostAsync("auth/refresh", It.IsAny<HttpContent>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(responseMessage);

        mockTokenStorage.Setup(x => x.StoreTokensAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTime?>()))
            .Returns(Task.CompletedTask);

        var authService = new AuthService(mockApi.Object, mockTokenStorage.Object, mockSession.Object, null!, Mock.Of<ICacheService>(), mockLogger.Object);

        // Act
        var result = await authService.TryRefreshTokenAsync();

        // Assert
        Assert.IsTrue(result);
        mockTokenStorage.Verify(x => x.StoreTokensAsync(null!, "new-refresh-token", responseDto.ExpiresAt), Times.Once);
    }

    /// <summary>
    /// Tests that TryRefreshTokenAsync handles a response with null RefreshToken but valid AccessToken.
    /// Input: Successful API response with valid AccessToken but null RefreshToken.
    /// Expected: Returns true and stores tokens with null RefreshToken.
    /// </summary>
    [TestMethod]
    public async Task TryRefreshTokenAsync_ResponseWithValidAccessTokenButNullRefreshToken_StoresTokensWithNullRefreshToken()
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockTokenStorage = new Mock<TokenStorageService>();
        var mockSession = new Mock<SessionService>();
        var mockLogger = new Mock<ILogger<AuthService>>();

        mockTokenStorage.Setup(x => x.GetRefreshTokenAsync()).ReturnsAsync("valid-refresh-token");

        var responseDto = new AuthResponseDto
        {
            AccessToken = "new-access-token",
            RefreshToken = null!,
            ExpiresAt = DateTime.UtcNow.AddHours(1)
        };

        var responseMessage = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(responseDto)
        };

        mockApi.Setup(x => x.PostAsync("auth/refresh", It.IsAny<HttpContent>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(responseMessage);

        mockTokenStorage.Setup(x => x.StoreTokensAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTime?>()))
            .Returns(Task.CompletedTask);

        var authService = new AuthService(mockApi.Object, mockTokenStorage.Object, mockSession.Object, null!, Mock.Of<ICacheService>(), mockLogger.Object);

        // Act
        var result = await authService.TryRefreshTokenAsync();

        // Assert
        Assert.IsTrue(result);
        mockTokenStorage.Verify(x => x.StoreTokensAsync("new-access-token", null!, responseDto.ExpiresAt), Times.Once);
    }

    /// <summary>
    /// Tests that TryRefreshTokenAsync handles a response with both null AccessToken and RefreshToken.
    /// Input: Successful API response with both tokens null.
    /// Expected: Returns true and stores null tokens.
    /// </summary>
    [TestMethod]
    public async Task TryRefreshTokenAsync_ResponseWithBothTokensNull_StoresNullTokens()
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockTokenStorage = new Mock<TokenStorageService>();
        var mockSession = new Mock<SessionService>();
        var mockLogger = new Mock<ILogger<AuthService>>();

        mockTokenStorage.Setup(x => x.GetRefreshTokenAsync()).ReturnsAsync("valid-refresh-token");

        var responseDto = new AuthResponseDto
        {
            AccessToken = null!,
            RefreshToken = null!,
            ExpiresAt = DateTime.UtcNow.AddHours(1)
        };

        var responseMessage = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(responseDto)
        };

        mockApi.Setup(x => x.PostAsync("auth/refresh", It.IsAny<HttpContent>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(responseMessage);

        mockTokenStorage.Setup(x => x.StoreTokensAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTime?>()))
            .Returns(Task.CompletedTask);

        var authService = new AuthService(mockApi.Object, mockTokenStorage.Object, mockSession.Object, null!, Mock.Of<ICacheService>(), mockLogger.Object);

        // Act
        var result = await authService.TryRefreshTokenAsync();

        // Assert
        Assert.IsTrue(result);
        mockTokenStorage.Verify(x => x.StoreTokensAsync(null!, null!, responseDto.ExpiresAt), Times.Once);
    }

    /// <summary>
    /// Tests that TryRefreshTokenAsync correctly passes default CancellationToken to PostAsync.
    /// Input: Valid refresh token and successful API response.
    /// Expected: PostAsync is called with CancellationToken.None (default value).
    /// </summary>
    [TestMethod]
    public async Task TryRefreshTokenAsync_ValidRefreshToken_PassesDefaultCancellationToken()
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockTokenStorage = new Mock<TokenStorageService>();
        var mockSession = new Mock<SessionService>();
        var mockLogger = new Mock<ILogger<AuthService>>();

        mockTokenStorage.Setup(x => x.GetRefreshTokenAsync()).ReturnsAsync("valid-refresh-token");

        var responseDto = new AuthResponseDto
        {
            AccessToken = "new-access-token",
            RefreshToken = "new-refresh-token",
            ExpiresAt = DateTime.UtcNow.AddHours(1)
        };

        var responseMessage = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(responseDto)
        };

        mockApi.Setup(x => x.PostAsync("auth/refresh", It.IsAny<HttpContent>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(responseMessage);

        mockTokenStorage.Setup(x => x.StoreTokensAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTime?>()))
            .Returns(Task.CompletedTask);

        var authService = new AuthService(mockApi.Object, mockTokenStorage.Object, mockSession.Object, null!, Mock.Of<ICacheService>(), mockLogger.Object);

        // Act
        var result = await authService.TryRefreshTokenAsync();

        // Assert
        Assert.IsTrue(result);
        mockApi.Verify(x => x.PostAsync("auth/refresh", It.IsAny<HttpContent>(), default(CancellationToken)), Times.Once);
    }

    /// <summary>
    /// Tests that TryRefreshTokenAsync handles a refresh token that is exactly at the boundary of being empty.
    /// Input: Single space character as refresh token.
    /// Expected: Returns false as string.IsNullOrEmpty will be false but it's whitespace-only.
    /// Note: The method uses string.IsNullOrEmpty, not IsNullOrWhiteSpace, so single space passes initial check.
    /// </summary>
    [TestMethod]
    public async Task TryRefreshTokenAsync_RefreshTokenIsSingleSpace_ProcessesAsValidToken()
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockTokenStorage = new Mock<TokenStorageService>();
        var mockSession = new Mock<SessionService>();
        var mockLogger = new Mock<ILogger<AuthService>>();

        mockTokenStorage.Setup(x => x.GetRefreshTokenAsync()).ReturnsAsync(" ");

        var responseDto = new AuthResponseDto
        {
            AccessToken = "new-access-token",
            RefreshToken = "new-refresh-token",
            ExpiresAt = DateTime.UtcNow.AddHours(1)
        };

        var responseMessage = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(responseDto)
        };

        mockApi.Setup(x => x.PostAsync("auth/refresh", It.IsAny<HttpContent>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(responseMessage);

        mockTokenStorage.Setup(x => x.StoreTokensAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTime?>()))
            .Returns(Task.CompletedTask);

        var authService = new AuthService(mockApi.Object, mockTokenStorage.Object, mockSession.Object, null!, Mock.Of<ICacheService>(), mockLogger.Object);

        // Act
        var result = await authService.TryRefreshTokenAsync();

        // Assert
        Assert.IsTrue(result);
        mockApi.Verify(x => x.PostAsync("auth/refresh", It.IsAny<HttpContent>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Tests that TryRefreshTokenAsync handles redirect status codes (3xx) correctly.
    /// Input: API returns redirect status codes.
    /// Expected: Returns false and logs warning as IsSuccessStatusCode is false for 3xx codes.
    /// </summary>
    [TestMethod]
    [DataRow(HttpStatusCode.MovedPermanently)]
    [DataRow(HttpStatusCode.Found)]
    [DataRow(HttpStatusCode.SeeOther)]
    [DataRow(HttpStatusCode.TemporaryRedirect)]
    public async Task TryRefreshTokenAsync_RedirectStatusCodes_ReturnsFalseAndLogsWarning(HttpStatusCode statusCode)
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockTokenStorage = new Mock<TokenStorageService>();
        var mockSession = new Mock<SessionService>();
        var mockLogger = new Mock<ILogger<AuthService>>();

        mockTokenStorage.Setup(x => x.GetRefreshTokenAsync()).ReturnsAsync("valid-refresh-token");

        var responseMessage = new HttpResponseMessage(statusCode);

        mockApi.Setup(x => x.PostAsync("auth/refresh", It.IsAny<HttpContent>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(responseMessage);

        var authService = new AuthService(mockApi.Object, mockTokenStorage.Object, mockSession.Object, null!, Mock.Of<ICacheService>(), mockLogger.Object);

        // Act
        var result = await authService.TryRefreshTokenAsync();

        // Assert
        Assert.IsFalse(result);
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Token refresh returned")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    /// <summary>
    /// Tests that TryRefreshTokenAsync handles a future date far beyond typical token expiration.
    /// Input: Response with ExpiresAt set to 100 years in the future.
    /// Expected: Returns true and stores the far-future expiration date.
    /// </summary>
    [TestMethod]
    public async Task TryRefreshTokenAsync_ExpiresAtIsFarFuture_StoresTokensCorrectly()
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockTokenStorage = new Mock<TokenStorageService>();
        var mockSession = new Mock<SessionService>();
        var mockLogger = new Mock<ILogger<AuthService>>();

        mockTokenStorage.Setup(x => x.GetRefreshTokenAsync()).ReturnsAsync("valid-refresh-token");

        var farFutureDate = DateTime.UtcNow.AddYears(100);
        var responseDto = new AuthResponseDto
        {
            AccessToken = "new-access-token",
            RefreshToken = "new-refresh-token",
            ExpiresAt = farFutureDate
        };

        var responseMessage = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(responseDto)
        };

        mockApi.Setup(x => x.PostAsync("auth/refresh", It.IsAny<HttpContent>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(responseMessage);

        mockTokenStorage.Setup(x => x.StoreTokensAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTime?>()))
            .Returns(Task.CompletedTask);

        var authService = new AuthService(mockApi.Object, mockTokenStorage.Object, mockSession.Object, null!, Mock.Of<ICacheService>(), mockLogger.Object);

        // Act
        var result = await authService.TryRefreshTokenAsync();

        // Assert
        Assert.IsTrue(result);
        mockTokenStorage.Verify(x => x.StoreTokensAsync("new-access-token", "new-refresh-token", farFutureDate), Times.Once);
    }

    /// <summary>
    /// Tests that TryRefreshTokenAsync handles a past expiration date.
    /// Input: Response with ExpiresAt set to a date in the past.
    /// Expected: Returns true and stores the past date (no validation in the method).
    /// </summary>
    [TestMethod]
    public async Task TryRefreshTokenAsync_ExpiresAtIsInPast_StoresTokensWithPastDate()
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockTokenStorage = new Mock<TokenStorageService>();
        var mockSession = new Mock<SessionService>();
        var mockLogger = new Mock<ILogger<AuthService>>();

        mockTokenStorage.Setup(x => x.GetRefreshTokenAsync()).ReturnsAsync("valid-refresh-token");

        var pastDate = DateTime.UtcNow.AddYears(-10);
        var responseDto = new AuthResponseDto
        {
            AccessToken = "new-access-token",
            RefreshToken = "new-refresh-token",
            ExpiresAt = pastDate
        };

        var responseMessage = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(responseDto)
        };

        mockApi.Setup(x => x.PostAsync("auth/refresh", It.IsAny<HttpContent>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(responseMessage);

        mockTokenStorage.Setup(x => x.StoreTokensAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTime?>()))
            .Returns(Task.CompletedTask);

        var authService = new AuthService(mockApi.Object, mockTokenStorage.Object, mockSession.Object, null!, Mock.Of<ICacheService>(), mockLogger.Object);

        // Act
        var result = await authService.TryRefreshTokenAsync();

        // Assert
        Assert.IsTrue(result);
        mockTokenStorage.Verify(x => x.StoreTokensAsync("new-access-token", "new-refresh-token", pastDate), Times.Once);
    }

    /// <summary>
    /// Tests that LogoutAsync calls Logger with exact message "User logging out".
    /// Input: Valid dependencies configured.
    /// Expected: Logger.LogInformation is called with exact message.
    /// </summary>
    [TestMethod]
    public async Task LogoutAsync_ValidCall_LogsExactMessage()
    {
        // Arrange
        var mockApiService = new Mock<IApiService>();
        var mockTokenStorage = new Mock<TokenStorageService>(MockBehavior.Loose, Mock.Of<ILogger<TokenStorageService>>());
        var mockSession = new Mock<SessionService>(MockBehavior.Loose);
        var mockLogger = new Mock<ILogger<AuthService>>();

        mockTokenStorage.Setup(ts => ts.ClearAsync()).Returns(Task.CompletedTask);
        mockSession.Setup(s => s.Clear());

        var authService = new AuthService(
            mockApiService.Object,
            mockTokenStorage.Object,
            mockSession.Object,
            new PollingService(Mock.Of<IRefreshCoordinator>(), Mock.Of<ILogger<PollingService>>()),
            Mock.Of<ICacheService>(),
            mockLogger.Object);

        // Act
        try
        {
            await authService.LogoutAsync();
        }
        catch (NullReferenceException)
        {
            // Expected: Shell.Current is null in unit test context
        }
        catch (InvalidOperationException)
        {
            // Expected: Shell.Current might throw InvalidOperationException
        }

        // Assert
        mockLogger.Verify(
            l => l.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString() == "User logging out"),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once,
            "Logger should log exact message 'User logging out'");
    }

    /// <summary>
    /// Tests that LogoutAsync calls Session.Clear exactly once.
    /// Input: All dependencies configured successfully.
    /// Expected: Session.Clear is called exactly once.
    /// </summary>
    [TestMethod]
    public async Task LogoutAsync_ValidCall_CallsSessionClearExactlyOnce()
    {
        // Arrange
        var mockApiService = new Mock<IApiService>();
        var mockTokenStorage = new Mock<TokenStorageService>(MockBehavior.Loose, Mock.Of<ILogger<TokenStorageService>>());
        var mockSession = new Mock<SessionService>(MockBehavior.Loose);
        var mockLogger = new Mock<ILogger<AuthService>>();

        var clearCallCount = 0;
        mockSession.Setup(s => s.Clear()).Callback(() => clearCallCount++);
        mockTokenStorage.Setup(ts => ts.ClearAsync()).Returns(Task.CompletedTask);

        var authService = new AuthService(
            mockApiService.Object,
            mockTokenStorage.Object,
            mockSession.Object,
            new PollingService(Mock.Of<IRefreshCoordinator>(), Mock.Of<ILogger<PollingService>>()),
            Mock.Of<ICacheService>(),
            mockLogger.Object);

        // Act
        try
        {
            await authService.LogoutAsync();
        }
        catch (NullReferenceException)
        {
            // Expected: Shell.Current is null in unit test context
        }
        catch (InvalidOperationException)
        {
            // Expected: Shell.Current might throw InvalidOperationException
        }

        // Assert
        Assert.AreEqual(1, clearCallCount, "Session.Clear should be called exactly once");
        mockSession.Verify(s => s.Clear(), Times.Once);
    }

    /// <summary>
    /// Tests that LogoutAsync calls TokenStorage.ClearAsync exactly once.
    /// Input: All dependencies configured successfully.
    /// Expected: TokenStorage.ClearAsync is called exactly once.
    /// </summary>
    [TestMethod]
    public async Task LogoutAsync_ValidCall_CallsTokenStorageClearAsyncExactlyOnce()
    {
        // Arrange
        var mockApiService = new Mock<IApiService>();
        var mockTokenStorage = new Mock<TokenStorageService>(MockBehavior.Loose, Mock.Of<ILogger<TokenStorageService>>());
        var mockSession = new Mock<SessionService>(MockBehavior.Loose);
        var mockLogger = new Mock<ILogger<AuthService>>();

        var clearAsyncCallCount = 0;
        mockSession.Setup(s => s.Clear());
        mockTokenStorage.Setup(ts => ts.ClearAsync())
            .Callback(() => clearAsyncCallCount++)
            .Returns(Task.CompletedTask);

        var authService = new AuthService(
            mockApiService.Object,
            mockTokenStorage.Object,
            mockSession.Object,
            new PollingService(Mock.Of<IRefreshCoordinator>(), Mock.Of<ILogger<PollingService>>()),
            Mock.Of<ICacheService>(),
            mockLogger.Object);

        // Act
        try
        {
            await authService.LogoutAsync();
        }
        catch (NullReferenceException)
        {
            // Expected: Shell.Current is null in unit test context
        }
        catch (InvalidOperationException)
        {
            // Expected: Shell.Current might throw InvalidOperationException
        }

        // Assert
        Assert.AreEqual(1, clearAsyncCallCount, "TokenStorage.ClearAsync should be called exactly once");
        mockTokenStorage.Verify(ts => ts.ClearAsync(), Times.Once);
    }

    /// <summary>
    /// Tests that LogoutAsync handles null logger gracefully by not throwing on log call.
    /// Input: Null logger.
    /// Expected: Method attempts to execute without null reference exception from logger.
    /// Note: This test verifies that the method can handle a null logger dependency,
    /// though in production this should be avoided through proper dependency injection.
    /// </summary>
    [TestMethod]
    public async Task LogoutAsync_NullLogger_HandlesGracefully()
    {
        // Arrange
        var mockApiService = new Mock<IApiService>();
        var mockTokenStorage = new Mock<TokenStorageService>(MockBehavior.Loose, Mock.Of<ILogger<TokenStorageService>>());
        var mockSession = new Mock<SessionService>(MockBehavior.Loose);

        mockSession.Setup(s => s.Clear());
        mockTokenStorage.Setup(ts => ts.ClearAsync()).Returns(Task.CompletedTask);

        var authService = new AuthService(
            mockApiService.Object,
            mockTokenStorage.Object,
            mockSession.Object,
            new PollingService(Mock.Of<IRefreshCoordinator>(), Mock.Of<ILogger<PollingService>>()),
            Mock.Of<ICacheService>(),
            null!);

        // Act & Assert
        try
        {
            await authService.LogoutAsync();
            // If we reach here without NullReferenceException from logger, test passes
            // We expect exception from Shell.Current instead
            Assert.Fail("Expected exception from Shell.Current");
        }
        catch (NullReferenceException ex)
        {
            // Expected: Shell.Current is null, not from logger
            Assert.IsTrue(
                !ex.StackTrace!.Contains("LogInformation"),
                "Exception should be from Shell.Current, not from logger");
        }
        catch (InvalidOperationException)
        {
            // Expected: Shell.Current might throw InvalidOperationException
        }

        mockSession.Verify(s => s.Clear(), Times.Once, "Session.Clear should still be called");
        mockTokenStorage.Verify(ts => ts.ClearAsync(), Times.Once, "TokenStorage.ClearAsync should still be called");
    }

    /// <summary>
    /// Tests that RegisterAsync logs information with the correct email before making the API call.
    /// Input: Valid RegisterDto with specific email.
    /// Expected: Information log is called with the email from DTO before API call.
    /// </summary>
    [TestMethod]
    public async Task RegisterAsync_ValidDto_LogsInformationWithEmail()
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockTokenStorage = new Mock<TokenStorageService>();
        var mockSession = new Mock<SessionService>();
        var mockLogger = new Mock<ILogger<AuthService>>();
        var dto = new RegisterDto
        {
            FullName = "Test User",
            Email = "specific.test@example.com",
            Password = "Password123",
            PhoneNumber = "1234567890",
            StudentId = "STU001",
            Role = "Student"
        };
        var response = new HttpResponseMessage(HttpStatusCode.OK);
        mockApi.Setup(x => x.PostAsync("auth/register", It.IsAny<HttpContent>(), It.IsAny<CancellationToken>()))
               .ReturnsAsync(response);
        var authService = new AuthService(mockApi.Object, mockTokenStorage.Object, mockSession.Object, null!, Mock.Of<ICacheService>(), mockLogger.Object);

        // Act
        var result = await authService.RegisterAsync(dto);

        // Assert
        Assert.IsTrue(result);
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("specific.test@example.com")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    /// <summary>
    /// Tests that RegisterAsync logs warning with the correct status code when API call fails.
    /// Input: Valid RegisterDto with various failure status codes.
    /// Expected: Warning log is called with the exact status code from the response.
    /// </summary>
    [TestMethod]
    [DataRow(HttpStatusCode.BadRequest)]
    [DataRow(HttpStatusCode.Unauthorized)]
    [DataRow(HttpStatusCode.InternalServerError)]
    public async Task RegisterAsync_FailedResponse_LogsWarningWithStatusCode(HttpStatusCode statusCode)
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockTokenStorage = new Mock<TokenStorageService>();
        var mockSession = new Mock<SessionService>();
        var mockLogger = new Mock<ILogger<AuthService>>();
        var dto = new RegisterDto
        {
            FullName = "Test User",
            Email = "test@example.com",
            Password = "Password123",
            PhoneNumber = "1234567890",
            StudentId = "STU001",
            Role = "Student"
        };
        var response = new HttpResponseMessage(statusCode);
        mockApi.Setup(x => x.PostAsync("auth/register", It.IsAny<HttpContent>(), It.IsAny<CancellationToken>()))
               .ReturnsAsync(response);
        var authService = new AuthService(mockApi.Object, mockTokenStorage.Object, mockSession.Object, null!, Mock.Of<ICacheService>(), mockLogger.Object);

        // Act
        var result = await authService.RegisterAsync(dto);

        // Assert
        Assert.IsFalse(result);
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains(((int)statusCode).ToString())),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    /// <summary>
    /// Tests that RegisterAsync does not log warning when API call succeeds.
    /// Input: Valid RegisterDto with successful API response.
    /// Expected: Warning log is never called, only information log is called.
    /// </summary>
    [TestMethod]
    public async Task RegisterAsync_SuccessfulResponse_DoesNotLogWarning()
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockTokenStorage = new Mock<TokenStorageService>();
        var mockSession = new Mock<SessionService>();
        var mockLogger = new Mock<ILogger<AuthService>>();
        var dto = new RegisterDto
        {
            FullName = "Test User",
            Email = "test@example.com",
            Password = "Password123",
            PhoneNumber = "1234567890",
            StudentId = "STU001",
            Role = "Student"
        };
        var response = new HttpResponseMessage(HttpStatusCode.OK);
        mockApi.Setup(x => x.PostAsync("auth/register", It.IsAny<HttpContent>(), It.IsAny<CancellationToken>()))
               .ReturnsAsync(response);
        var authService = new AuthService(mockApi.Object, mockTokenStorage.Object, mockSession.Object, null!, Mock.Of<ICacheService>(), mockLogger.Object);

        // Act
        var result = await authService.RegisterAsync(dto);

        // Assert
        Assert.IsTrue(result);
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
    /// Tests that RegisterAsync handles redirection status codes as non-success.
    /// Input: API returns redirection status codes (3xx).
    /// Expected: Returns false and logs warning.
    /// </summary>
    [TestMethod]
    [DataRow(HttpStatusCode.MovedPermanently)]
    [DataRow(HttpStatusCode.Found)]
    [DataRow(HttpStatusCode.SeeOther)]
    [DataRow(HttpStatusCode.NotModified)]
    [DataRow(HttpStatusCode.TemporaryRedirect)]
    public async Task RegisterAsync_RedirectionResponse_ReturnsFalse(HttpStatusCode statusCode)
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockTokenStorage = new Mock<TokenStorageService>();
        var mockSession = new Mock<SessionService>();
        var mockLogger = new Mock<ILogger<AuthService>>();
        var dto = new RegisterDto
        {
            FullName = "Test User",
            Email = "test@example.com",
            Password = "Password123",
            PhoneNumber = "1234567890",
            StudentId = "STU001",
            Role = "Student"
        };
        var response = new HttpResponseMessage(statusCode);
        mockApi.Setup(x => x.PostAsync("auth/register", It.IsAny<HttpContent>(), It.IsAny<CancellationToken>()))
               .ReturnsAsync(response);
        var authService = new AuthService(mockApi.Object, mockTokenStorage.Object, mockSession.Object, null!, Mock.Of<ICacheService>(), mockLogger.Object);

        // Act
        var result = await authService.RegisterAsync(dto);

        // Assert
        Assert.IsFalse(result);
    }

    /// <summary>
    /// Tests that RegisterAsync handles boundary HTTP status code values.
    /// Input: API returns edge case status codes (100, 599).
    /// Expected: Returns false for non-2xx codes.
    /// </summary>
    [TestMethod]
    [DataRow(100)] // Continue - Informational
    [DataRow(599)] // Non-standard high value
    public async Task RegisterAsync_BoundaryStatusCodes_ReturnsFalse(int statusCode)
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockTokenStorage = new Mock<TokenStorageService>();
        var mockSession = new Mock<SessionService>();
        var mockLogger = new Mock<ILogger<AuthService>>();
        var dto = new RegisterDto
        {
            FullName = "Test User",
            Email = "test@example.com",
            Password = "Password123",
            PhoneNumber = "1234567890",
            StudentId = "STU001",
            Role = "Student"
        };
        var response = new HttpResponseMessage((HttpStatusCode)statusCode);
        mockApi.Setup(x => x.PostAsync("auth/register", It.IsAny<HttpContent>(), It.IsAny<CancellationToken>()))
               .ReturnsAsync(response);
        var authService = new AuthService(mockApi.Object, mockTokenStorage.Object, mockSession.Object, null!, Mock.Of<ICacheService>(), mockLogger.Object);

        // Act
        var result = await authService.RegisterAsync(dto);

        // Assert
        Assert.IsFalse(result);
    }

    /// <summary>
    /// Tests that RegisterAsync handles all properties with null values correctly.
    /// Input: RegisterDto with all properties explicitly set to null (where allowed by compiler).
    /// Expected: Completes successfully, as properties have default empty string values.
    /// </summary>
    [TestMethod]
    public async Task RegisterAsync_DtoWithDefaultValues_CompletesSuccessfully()
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockTokenStorage = new Mock<TokenStorageService>();
        var mockSession = new Mock<SessionService>();
        var mockLogger = new Mock<ILogger<AuthService>>();
        var dto = new RegisterDto();
        var response = new HttpResponseMessage(HttpStatusCode.OK);
        mockApi.Setup(x => x.PostAsync("auth/register", It.IsAny<HttpContent>(), It.IsAny<CancellationToken>()))
               .ReturnsAsync(response);
        var authService = new AuthService(mockApi.Object, mockTokenStorage.Object, mockSession.Object, null!, Mock.Of<ICacheService>(), mockLogger.Object);

        // Act
        var result = await authService.RegisterAsync(dto);

        // Assert
        Assert.IsTrue(result);
    }

    /// <summary>
    /// Tests that RegisterAsync handles DTO with all maximum length strings.
    /// Input: RegisterDto with all properties set to very long strings (10000 characters).
    /// Expected: Processes successfully without errors.
    /// </summary>
    [TestMethod]
    public async Task RegisterAsync_DtoWithAllVeryLongProperties_CompletesSuccessfully()
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockTokenStorage = new Mock<TokenStorageService>();
        var mockSession = new Mock<SessionService>();
        var mockLogger = new Mock<ILogger<AuthService>>();
        var longString = new string('x', 10000);
        var dto = new RegisterDto
        {
            FullName = longString,
            Email = longString,
            Password = longString,
            PhoneNumber = longString,
            StudentId = longString,
            Role = longString
        };
        var response = new HttpResponseMessage(HttpStatusCode.OK);
        mockApi.Setup(x => x.PostAsync("auth/register", It.IsAny<HttpContent>(), It.IsAny<CancellationToken>()))
               .ReturnsAsync(response);
        var authService = new AuthService(mockApi.Object, mockTokenStorage.Object, mockSession.Object, null!, Mock.Of<ICacheService>(), mockLogger.Object);

        // Act
        var result = await authService.RegisterAsync(dto);

        // Assert
        Assert.IsTrue(result);
    }

    /// <summary>
    /// Tests that RegisterAsync handles DTO with Unicode and special characters in all properties.
    /// Input: RegisterDto with Unicode characters, emojis, and special characters.
    /// Expected: Processes successfully without errors.
    /// </summary>
    [TestMethod]
    public async Task RegisterAsync_DtoWithUnicodeCharacters_CompletesSuccessfully()
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockTokenStorage = new Mock<TokenStorageService>();
        var mockSession = new Mock<SessionService>();
        var mockLogger = new Mock<ILogger<AuthService>>();
        var dto = new RegisterDto
        {
            FullName = "Ã§â€Â¨Ã¦Ë†Â·Ã¥ÂÂ Ã°Å¸â€˜Â¤",
            Email = "Ã¦Âµâ€¹Ã¨Â¯â€¢@Ã¤Â¾â€¹Ã¥Â­Â.com Ã°Å¸â€œÂ§",
            Password = "Ã¥Â¯â€ Ã§Â Â123!@#",
            PhoneNumber = "Ã°Å¸â€œÅ¾+1234567890",
            StudentId = "Ã¥Â­Â¦Ã§â€Å¸ID-001",
            Role = "Ã¨Â§â€™Ã¨â€°Â²"
        };
        var response = new HttpResponseMessage(HttpStatusCode.OK);
        mockApi.Setup(x => x.PostAsync("auth/register", It.IsAny<HttpContent>(), It.IsAny<CancellationToken>()))
               .ReturnsAsync(response);
        var authService = new AuthService(mockApi.Object, mockTokenStorage.Object, mockSession.Object, null!, Mock.Of<ICacheService>(), mockLogger.Object);

        // Act
        var result = await authService.RegisterAsync(dto);

        // Assert
        Assert.IsTrue(result);
    }

    /// <summary>
    /// Tests that RegisterAsync verifies PostAsync is called exactly once.
    /// Input: Valid RegisterDto.
    /// Expected: PostAsync is called exactly once, regardless of result.
    /// </summary>
    [TestMethod]
    [DataRow(HttpStatusCode.OK, true)]
    [DataRow(HttpStatusCode.BadRequest, false)]
    public async Task RegisterAsync_AnyResponse_CallsPostAsyncExactlyOnce(HttpStatusCode statusCode, bool expectedResult)
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockTokenStorage = new Mock<TokenStorageService>();
        var mockSession = new Mock<SessionService>();
        var mockLogger = new Mock<ILogger<AuthService>>();
        var dto = new RegisterDto
        {
            FullName = "Test User",
            Email = "test@example.com",
            Password = "Password123",
            PhoneNumber = "1234567890",
            StudentId = "STU001",
            Role = "Student"
        };
        var response = new HttpResponseMessage(statusCode);
        mockApi.Setup(x => x.PostAsync("auth/register", It.IsAny<HttpContent>(), It.IsAny<CancellationToken>()))
               .ReturnsAsync(response);
        var authService = new AuthService(mockApi.Object, mockTokenStorage.Object, mockSession.Object, null!, Mock.Of<ICacheService>(), mockLogger.Object);

        // Act
        var result = await authService.RegisterAsync(dto);

        // Assert
        Assert.AreEqual(expectedResult, result);
        mockApi.Verify(x => x.PostAsync("auth/register", It.IsAny<HttpContent>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Tests that LoginAsync logs warning with correct status code and body when API returns error.
    /// Input: API returns error with specific status code and body content.
    /// Expected: Warning is logged with the status code and body content.
    /// </summary>
    [TestMethod]
    public async Task LoginAsync_ApiReturnsError_LogsWarningWithStatusCodeAndBody()
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockTokenStorage = new Mock<TokenStorageService>();
        var mockSession = new Mock<SessionService>();
        var mockLogger = new Mock<ILogger<AuthService>>();

        var authService = new AuthService(mockApi.Object, mockTokenStorage.Object, mockSession.Object, null!, Mock.Of<ICacheService>(), mockLogger.Object);

        var email = "test@example.com";
        var password = "wrongpassword";
        var responseBody = "{\"error\":\"Account locked\"}";

        var httpResponse = new HttpResponseMessage(HttpStatusCode.Forbidden)
        {
            Content = new StringContent(responseBody, Encoding.UTF8, "application/json")
        };

        mockApi.Setup(x => x.PostAsync("auth/login", It.IsAny<HttpContent>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(httpResponse);

        // Act
        try
        {
            await authService.LoginAsync(email, password);
        }
        catch (HttpRequestException)
        {
            // Expected exception
        }

        // Assert
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Login failed") && v.ToString()!.Contains("403")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    /// <summary>
    /// Tests that LoginAsync logs information message with email when login attempt starts.
    /// Input: Valid email and password.
    /// Expected: Information log contains the email address.
    /// </summary>
    [TestMethod]
    public async Task LoginAsync_LoginAttempt_LogsInformationWithEmail()
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockTokenStorage = new Mock<TokenStorageService>();
        var mockSession = new Mock<SessionService>();
        var mockLogger = new Mock<ILogger<AuthService>>();

        var authService = new AuthService(mockApi.Object, mockTokenStorage.Object, mockSession.Object, null!, Mock.Of<ICacheService>(), mockLogger.Object);

        var email = "specific@example.com";
        var password = "Password123!";

        var expectedDto = new AuthResponseDto
        {
            AccessToken = "access_token",
            RefreshToken = "refresh_token",
            ExpiresAt = DateTime.UtcNow.AddHours(1)
        };

        var responseContent = JsonContent.Create(expectedDto);
        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = responseContent
        };

        mockApi.Setup(x => x.PostAsync("auth/login", It.IsAny<HttpContent>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(httpResponse);

        mockTokenStorage.Setup(x => x.StoreTokensAsync(
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<DateTime?>()))
            .Returns(Task.CompletedTask);

        // Act
        await authService.LoginAsync(email, password);

        // Assert
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Login attempt") && v.ToString()!.Contains(email)),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    /// <summary>
    /// Tests that LoginAsync does not log success information when DTO is null.
    /// Input: API returns success but deserialization returns null DTO.
    /// Expected: Success information log is not called.
    /// </summary>
    [TestMethod]
    public async Task LoginAsync_SuccessButNullDto_DoesNotLogSuccessInformation()
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockTokenStorage = new Mock<TokenStorageService>();
        var mockSession = new Mock<SessionService>();
        var mockLogger = new Mock<ILogger<AuthService>>();

        var authService = new AuthService(mockApi.Object, mockTokenStorage.Object, mockSession.Object, null!, Mock.Of<ICacheService>(), mockLogger.Object);

        var email = "test@example.com";
        var password = "password123";

        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("null", Encoding.UTF8, "application/json")
        };

        mockApi.Setup(x => x.PostAsync("auth/login", It.IsAny<HttpContent>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(httpResponse);

        // Act
        await authService.LoginAsync(email, password);

        // Assert
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Login successful")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Never);
    }

    /// <summary>
    /// Tests that LoginAsync stores tokens with exact values from response DTO.
    /// Input: API returns success with specific token values.
    /// Expected: StoreTokensAsync is called with exact AccessToken, RefreshToken, and ExpiresAt values.
    /// </summary>
    [TestMethod]
    public async Task LoginAsync_SuccessfulLogin_StoresExactTokenValues()
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockTokenStorage = new Mock<TokenStorageService>();
        var mockSession = new Mock<SessionService>();
        var mockLogger = new Mock<ILogger<AuthService>>();

        var authService = new AuthService(mockApi.Object, mockTokenStorage.Object, mockSession.Object, null!, Mock.Of<ICacheService>(), mockLogger.Object);

        var email = "test@example.com";
        var password = "Password123!";
        var expectedAccessToken = "specific_access_token_12345";
        var expectedRefreshToken = "specific_refresh_token_67890";
        var expectedExpiresAt = new DateTime(2025, 12, 31, 23, 59, 59, DateTimeKind.Utc);

        var expectedDto = new AuthResponseDto
        {
            AccessToken = expectedAccessToken,
            RefreshToken = expectedRefreshToken,
            ExpiresAt = expectedExpiresAt
        };

        var responseContent = JsonContent.Create(expectedDto);
        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = responseContent
        };

        mockApi.Setup(x => x.PostAsync("auth/login", It.IsAny<HttpContent>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(httpResponse);

        mockTokenStorage.Setup(x => x.StoreTokensAsync(
            expectedAccessToken,
            expectedRefreshToken,
            expectedExpiresAt))
            .Returns(Task.CompletedTask);

        // Act
        await authService.LoginAsync(email, password);

        // Assert
        mockTokenStorage.Verify(x => x.StoreTokensAsync(
            expectedAccessToken,
            expectedRefreshToken,
            expectedExpiresAt), Times.Once);
    }

    /// <summary>
    /// Tests that LoginAsync handles DTO with empty token strings.
    /// Input: API returns success but DTO has empty AccessToken and RefreshToken.
    /// Expected: Empty tokens are stored successfully.
    /// </summary>
    [TestMethod]
    public async Task LoginAsync_DtoWithEmptyTokens_StoresEmptyTokens()
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockTokenStorage = new Mock<TokenStorageService>();
        var mockSession = new Mock<SessionService>();
        var mockLogger = new Mock<ILogger<AuthService>>();

        var authService = new AuthService(mockApi.Object, mockTokenStorage.Object, mockSession.Object, null!, Mock.Of<ICacheService>(), mockLogger.Object);

        var email = "test@example.com";
        var password = "Password123!";

        var expectedDto = new AuthResponseDto
        {
            AccessToken = "",
            RefreshToken = "",
            ExpiresAt = DateTime.UtcNow
        };

        var responseContent = JsonContent.Create(expectedDto);
        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = responseContent
        };

        mockApi.Setup(x => x.PostAsync("auth/login", It.IsAny<HttpContent>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(httpResponse);

        mockTokenStorage.Setup(x => x.StoreTokensAsync(
            "",
            "",
            It.IsAny<DateTime?>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await authService.LoginAsync(email, password);

        // Assert
        Assert.IsNotNull(result);
        mockTokenStorage.Verify(x => x.StoreTokensAsync("", "", It.IsAny<DateTime?>()), Times.Once);
    }

    /// <summary>
    /// Tests that LoginAsync handles DTO with DateTime.MinValue for ExpiresAt.
    /// Input: API returns success with ExpiresAt set to DateTime.MinValue.
    /// Expected: Tokens are stored with DateTime.MinValue for expiration.
    /// </summary>
    [TestMethod]
    public async Task LoginAsync_DtoWithMinDateTimeExpiresAt_StoresTokensCorrectly()
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockTokenStorage = new Mock<TokenStorageService>();
        var mockSession = new Mock<SessionService>();
        var mockLogger = new Mock<ILogger<AuthService>>();

        var authService = new AuthService(mockApi.Object, mockTokenStorage.Object, mockSession.Object, null!, Mock.Of<ICacheService>(), mockLogger.Object);

        var email = "test@example.com";
        var password = "Password123!";

        var expectedDto = new AuthResponseDto
        {
            AccessToken = "access_token",
            RefreshToken = "refresh_token",
            ExpiresAt = DateTime.MinValue
        };

        var responseContent = JsonContent.Create(expectedDto);
        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = responseContent
        };

        mockApi.Setup(x => x.PostAsync("auth/login", It.IsAny<HttpContent>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(httpResponse);

        mockTokenStorage.Setup(x => x.StoreTokensAsync(
            It.IsAny<string>(),
            It.IsAny<string>(),
            DateTime.MinValue))
            .Returns(Task.CompletedTask);

        // Act
        var result = await authService.LoginAsync(email, password);

        // Assert
        Assert.IsNotNull(result);
        mockTokenStorage.Verify(x => x.StoreTokensAsync(
            "access_token",
            "refresh_token",
            DateTime.MinValue), Times.Once);
    }

    /// <summary>
    /// Tests that LoginAsync handles DTO with DateTime.MaxValue for ExpiresAt.
    /// Input: API returns success with ExpiresAt set to DateTime.MaxValue.
    /// Expected: Tokens are stored with DateTime.MaxValue for expiration.
    /// </summary>
    [TestMethod]
    public async Task LoginAsync_DtoWithMaxDateTimeExpiresAt_StoresTokensCorrectly()
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockTokenStorage = new Mock<TokenStorageService>();
        var mockSession = new Mock<SessionService>();
        var mockLogger = new Mock<ILogger<AuthService>>();

        var authService = new AuthService(mockApi.Object, mockTokenStorage.Object, mockSession.Object, null!, Mock.Of<ICacheService>(), mockLogger.Object);

        var email = "test@example.com";
        var password = "Password123!";

        var expectedDto = new AuthResponseDto
        {
            AccessToken = "access_token",
            RefreshToken = "refresh_token",
            ExpiresAt = DateTime.MaxValue
        };

        var responseContent = JsonContent.Create(expectedDto);
        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = responseContent
        };

        mockApi.Setup(x => x.PostAsync("auth/login", It.IsAny<HttpContent>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(httpResponse);

        mockTokenStorage.Setup(x => x.StoreTokensAsync(
            It.IsAny<string>(),
            It.IsAny<string>(),
            DateTime.MaxValue))
            .Returns(Task.CompletedTask);

        // Act
        var result = await authService.LoginAsync(email, password);

        // Assert
        Assert.IsNotNull(result);
        mockTokenStorage.Verify(x => x.StoreTokensAsync(
            "access_token",
            "refresh_token",
            DateTime.MaxValue), Times.Once);
    }

    /// <summary>
    /// Tests that the AuthService constructor does not throw an exception
    /// when api and tokenStorage parameters are null.
    /// Input: Null api and tokenStorage, valid session and logger.
    /// Expected: Instance is created without throwing exceptions.
    /// </summary>
    [TestMethod]
    public void Constructor_NullApiAndTokenStorage_DoesNotThrow()
    {
        // Arrange
        var mockSession = new Mock<SessionService>();
        var mockLogger = new Mock<ILogger<AuthService>>();

        // Act
        var authService = new AuthService(null!, null!, mockSession.Object, null!, Mock.Of<ICacheService>(), mockLogger.Object);

        // Assert
        Assert.IsNotNull(authService);
    }

    /// <summary>
    /// Tests that the AuthService constructor does not throw an exception
    /// when api and session parameters are null.
    /// Input: Null api and session, valid tokenStorage and logger.
    /// Expected: Instance is created without throwing exceptions.
    /// </summary>
    [TestMethod]
    public void Constructor_NullApiAndSession_DoesNotThrow()
    {
        // Arrange
        var mockTokenStorage = new Mock<TokenStorageService>();
        var mockLogger = new Mock<ILogger<AuthService>>();

        // Act
        var authService = new AuthService(null!, mockTokenStorage.Object, null!, null!, Mock.Of<ICacheService>(), mockLogger.Object);

        // Assert
        Assert.IsNotNull(authService);
    }

    /// <summary>
    /// Tests that the AuthService constructor does not throw an exception
    /// when api and logger parameters are null.
    /// Input: Null api and logger, valid tokenStorage and session.
    /// Expected: Instance is created without throwing exceptions.
    /// </summary>
    [TestMethod]
    public void Constructor_NullApiAndLogger_DoesNotThrow()
    {
        // Arrange
        var mockTokenStorage = new Mock<TokenStorageService>();
        var mockSession = new Mock<SessionService>();

        // Act
        var authService = new AuthService(null!, mockTokenStorage.Object, mockSession.Object, null!, Mock.Of<ICacheService>(), null!);

        // Assert
        Assert.IsNotNull(authService);
    }

    /// <summary>
    /// Tests that the AuthService constructor does not throw an exception
    /// when tokenStorage and session parameters are null.
    /// Input: Null tokenStorage and session, valid api and logger.
    /// Expected: Instance is created without throwing exceptions.
    /// </summary>
    [TestMethod]
    public void Constructor_NullTokenStorageAndSession_DoesNotThrow()
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<AuthService>>();

        // Act
        var authService = new AuthService(mockApi.Object, null!, null!, null!, Mock.Of<ICacheService>(), mockLogger.Object);

        // Assert
        Assert.IsNotNull(authService);
    }

    /// <summary>
    /// Tests that the AuthService constructor does not throw an exception
    /// when tokenStorage and logger parameters are null.
    /// Input: Null tokenStorage and logger, valid api and session.
    /// Expected: Instance is created without throwing exceptions.
    /// </summary>
    [TestMethod]
    public void Constructor_NullTokenStorageAndLogger_DoesNotThrow()
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockSession = new Mock<SessionService>();

        // Act
        var authService = new AuthService(mockApi.Object, null!, mockSession.Object, null!, Mock.Of<ICacheService>(), null!);

        // Assert
        Assert.IsNotNull(authService);
    }

    /// <summary>
    /// Tests that the AuthService constructor does not throw an exception
    /// when session and logger parameters are null.
    /// Input: Null session and logger, valid api and tokenStorage.
    /// Expected: Instance is created without throwing exceptions.
    /// </summary>
    [TestMethod]
    public void Constructor_NullSessionAndLogger_DoesNotThrow()
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockTokenStorage = new Mock<TokenStorageService>();

        // Act
        var authService = new AuthService(mockApi.Object, mockTokenStorage.Object, null!, null!, Mock.Of<ICacheService>(), null!);

        // Assert
        Assert.IsNotNull(authService);
    }

    /// <summary>
    /// Tests that the AuthService constructor does not throw an exception
    /// when api, tokenStorage, and session parameters are null.
    /// Input: Null api, tokenStorage, and session, valid logger.
    /// Expected: Instance is created without throwing exceptions.
    /// </summary>
    [TestMethod]
    public void Constructor_NullApiTokenStorageAndSession_DoesNotThrow()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<AuthService>>();

        // Act
        var authService = new AuthService(null!, null!, null!, null!, Mock.Of<ICacheService>(), mockLogger.Object);

        // Assert
        Assert.IsNotNull(authService);
    }

    /// <summary>
    /// Tests that the AuthService constructor does not throw an exception
    /// when api, tokenStorage, and logger parameters are null.
    /// Input: Null api, tokenStorage, and logger, valid session.
    /// Expected: Instance is created without throwing exceptions.
    /// </summary>
    [TestMethod]
    public void Constructor_NullApiTokenStorageAndLogger_DoesNotThrow()
    {
        // Arrange
        var mockSession = new Mock<SessionService>();

        // Act
        var authService = new AuthService(null!, null!, mockSession.Object, null!, Mock.Of<ICacheService>(), null!);

        // Assert
        Assert.IsNotNull(authService);
    }

    /// <summary>
    /// Tests that the AuthService constructor does not throw an exception
    /// when api, session, and logger parameters are null.
    /// Input: Null api, session, and logger, valid tokenStorage.
    /// Expected: Instance is created without throwing exceptions.
    /// </summary>
    [TestMethod]
    public void Constructor_NullApiSessionAndLogger_DoesNotThrow()
    {
        // Arrange
        var mockTokenStorage = new Mock<TokenStorageService>();

        // Act
        var authService = new AuthService(null!, mockTokenStorage.Object, null!, null!, Mock.Of<ICacheService>(), null!);

        // Assert
        Assert.IsNotNull(authService);
    }

    /// <summary>
    /// Tests that the AuthService constructor does not throw an exception
    /// when tokenStorage, session, and logger parameters are null.
    /// Input: Null tokenStorage, session, and logger, valid api.
    /// Expected: Instance is created without throwing exceptions.
    /// </summary>
    [TestMethod]
    public void Constructor_NullTokenStorageSessionAndLogger_DoesNotThrow()
    {
        // Arrange
        var mockApi = new Mock<IApiService>();

        // Act
        var authService = new AuthService(mockApi.Object, null!, null!, null!, Mock.Of<ICacheService>(), null!);

        // Assert
        Assert.IsNotNull(authService);
    }

    /// <summary>
    /// Tests that VerifyOtpAsync calls PostAsync with the correct endpoint.
    /// Input: Valid email and OTP code.
    /// Expected: PostAsync is called with "auth/verify-otp" endpoint.
    /// </summary>
    [TestMethod]
    public async Task VerifyOtpAsync_ValidInputs_CallsCorrectEndpoint()
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockTokenStorage = new Mock<TokenStorageService>();
        var mockSession = new Mock<SessionService>();
        var mockLogger = new Mock<ILogger<AuthService>>();
        var authService = new AuthService(mockApi.Object, mockTokenStorage.Object, mockSession.Object, null!, Mock.Of<ICacheService>(), mockLogger.Object);

        string email = "test@example.com";
        string otpCode = "123456";

        var responseDto = new VerifyOtpResponseDto
        {
            Message = "Success",
            VerificationToken = "test-token"
        };
        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(responseDto)
        };

        mockApi.Setup(x => x.PostAsync(
            It.IsAny<string>(),
            It.IsAny<HttpContent>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(httpResponse);

        // Act
        await authService.VerifyOtpAsync(email, otpCode);

        // Assert
        mockApi.Verify(x => x.PostAsync(
            "auth/verify-otp",
            It.IsAny<HttpContent>(),
            It.IsAny<CancellationToken>()),
            Times.Once);
    }

    /// <summary>
    /// Tests that VerifyOtpAsync returns null when response has null VerificationToken property.
    /// Input: Valid email and OTP code with response containing null VerificationToken.
    /// Expected: Returns null.
    /// </summary>
    [TestMethod]
    public async Task VerifyOtpAsync_ResponseWithNullToken_ReturnsNull()
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockTokenStorage = new Mock<TokenStorageService>();
        var mockSession = new Mock<SessionService>();
        var mockLogger = new Mock<ILogger<AuthService>>();
        var authService = new AuthService(mockApi.Object, mockTokenStorage.Object, mockSession.Object, null!, Mock.Of<ICacheService>(), mockLogger.Object);

        string email = "test@example.com";
        string otpCode = "123456";

        var responseDto = new VerifyOtpResponseDto
        {
            Message = "Success",
            VerificationToken = null!
        };
        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(responseDto)
        };

        mockApi.Setup(x => x.PostAsync(
            "auth/verify-otp",
            It.IsAny<HttpContent>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(httpResponse);

        // Act
        var result = await authService.VerifyOtpAsync(email, otpCode);

        // Assert
        Assert.IsNull(result);
    }

    /// <summary>
    /// Tests that VerifyOtpAsync returns whitespace token when response contains whitespace-only VerificationToken.
    /// Input: Valid email and OTP code with response containing whitespace-only token.
    /// Expected: Returns the whitespace string.
    /// </summary>
    [TestMethod]
    [DataRow("   ")]
    [DataRow("\t")]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [DataRow(" \t\n ")]
    public async Task VerifyOtpAsync_ResponseWithWhitespaceToken_ReturnsWhitespace(string whitespaceToken)
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockTokenStorage = new Mock<TokenStorageService>();
        var mockSession = new Mock<SessionService>();
        var mockLogger = new Mock<ILogger<AuthService>>();
        var authService = new AuthService(mockApi.Object, mockTokenStorage.Object, mockSession.Object, null!, Mock.Of<ICacheService>(), mockLogger.Object);

        string email = "test@example.com";
        string otpCode = "123456";

        var responseDto = new VerifyOtpResponseDto
        {
            Message = "Success",
            VerificationToken = whitespaceToken
        };
        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(responseDto)
        };

        mockApi.Setup(x => x.PostAsync(
            "auth/verify-otp",
            It.IsAny<HttpContent>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(httpResponse);

        // Act
        var result = await authService.VerifyOtpAsync(email, otpCode);

        // Assert
        Assert.AreEqual(whitespaceToken, result);
    }

    /// <summary>
    /// Tests that VerifyOtpAsync handles redirect status codes by returning null.
    /// Input: Valid email and OTP code with redirect status codes.
    /// Expected: Returns null for all redirect codes.
    /// </summary>
    [TestMethod]
    [DataRow(HttpStatusCode.MovedPermanently)]
    [DataRow(HttpStatusCode.Found)]
    [DataRow(HttpStatusCode.SeeOther)]
    [DataRow(HttpStatusCode.TemporaryRedirect)]
    [DataRow(HttpStatusCode.PermanentRedirect)]
    public async Task VerifyOtpAsync_RedirectStatusCodes_ReturnsNull(HttpStatusCode statusCode)
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockTokenStorage = new Mock<TokenStorageService>();
        var mockSession = new Mock<SessionService>();
        var mockLogger = new Mock<ILogger<AuthService>>();
        var authService = new AuthService(mockApi.Object, mockTokenStorage.Object, mockSession.Object, null!, Mock.Of<ICacheService>(), mockLogger.Object);

        string email = "test@example.com";
        string otpCode = "123456";

        var httpResponse = new HttpResponseMessage(statusCode);

        mockApi.Setup(x => x.PostAsync(
            "auth/verify-otp",
            It.IsAny<HttpContent>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(httpResponse);

        // Act
        var result = await authService.VerifyOtpAsync(email, otpCode);

        // Assert
        Assert.IsNull(result);
    }

    /// <summary>
    /// Tests that VerifyOtpAsync handles numeric-only OTP codes correctly.
    /// Input: Email and numeric OTP codes of various lengths.
    /// Expected: Processes successfully and returns token.
    /// </summary>
    [TestMethod]
    [DataRow("0")]
    [DataRow("123456")]
    [DataRow("000000")]
    [DataRow("999999")]
    [DataRow("1234567890")]
    public async Task VerifyOtpAsync_NumericOtpCodes_ProcessesSuccessfully(string otpCode)
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockTokenStorage = new Mock<TokenStorageService>();
        var mockSession = new Mock<SessionService>();
        var mockLogger = new Mock<ILogger<AuthService>>();
        var authService = new AuthService(mockApi.Object, mockTokenStorage.Object, mockSession.Object, null!, Mock.Of<ICacheService>(), mockLogger.Object);

        string email = "test@example.com";
        string expectedToken = "verification-token";

        var responseDto = new VerifyOtpResponseDto
        {
            Message = "Success",
            VerificationToken = expectedToken
        };
        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(responseDto)
        };

        mockApi.Setup(x => x.PostAsync(
            "auth/verify-otp",
            It.IsAny<HttpContent>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(httpResponse);

        // Act
        var result = await authService.VerifyOtpAsync(email, otpCode);

        // Assert
        Assert.AreEqual(expectedToken, result);
    }

    /// <summary>
    /// Tests that VerifyOtpAsync handles alphanumeric OTP codes correctly.
    /// Input: Email and alphanumeric OTP codes.
    /// Expected: Processes successfully and returns token.
    /// </summary>
    [TestMethod]
    [DataRow("ABC123")]
    [DataRow("a1b2c3")]
    [DataRow("XYZ789")]
    [DataRow("MixedCase123")]
    public async Task VerifyOtpAsync_AlphanumericOtpCodes_ProcessesSuccessfully(string otpCode)
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockTokenStorage = new Mock<TokenStorageService>();
        var mockSession = new Mock<SessionService>();
        var mockLogger = new Mock<ILogger<AuthService>>();
        var authService = new AuthService(mockApi.Object, mockTokenStorage.Object, mockSession.Object, null!, Mock.Of<ICacheService>(), mockLogger.Object);

        string email = "test@example.com";
        string expectedToken = "verification-token";

        var responseDto = new VerifyOtpResponseDto
        {
            Message = "Success",
            VerificationToken = expectedToken
        };
        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(responseDto)
        };

        mockApi.Setup(x => x.PostAsync(
            "auth/verify-otp",
            It.IsAny<HttpContent>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(httpResponse);

        // Act
        var result = await authService.VerifyOtpAsync(email, otpCode);

        // Assert
        Assert.AreEqual(expectedToken, result);
    }

    /// <summary>
    /// Tests that VerifyOtpAsync processes successfully when email contains control characters.
    /// Input: Email with control characters, valid OTP code.
    /// Expected: Calls API and returns token based on response.
    /// </summary>
    [TestMethod]
    [DataRow("test\0@example.com")]
    [DataRow("test\r\n@example.com")]
    [DataRow("test\b@example.com")]
    public async Task VerifyOtpAsync_EmailWithControlCharacters_ProcessesSuccessfully(string email)
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockTokenStorage = new Mock<TokenStorageService>();
        var mockSession = new Mock<SessionService>();
        var mockLogger = new Mock<ILogger<AuthService>>();
        var authService = new AuthService(mockApi.Object, mockTokenStorage.Object, mockSession.Object, null!, Mock.Of<ICacheService>(), mockLogger.Object);

        string otpCode = "123456";
        string expectedToken = "verification-token";

        var responseDto = new VerifyOtpResponseDto
        {
            Message = "Success",
            VerificationToken = expectedToken
        };
        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(responseDto)
        };

        mockApi.Setup(x => x.PostAsync(
            "auth/verify-otp",
            It.IsAny<HttpContent>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(httpResponse);

        // Act
        var result = await authService.VerifyOtpAsync(email, otpCode);

        // Assert
        Assert.AreEqual(expectedToken, result);
    }

    /// <summary>
    /// Tests that VerifyOtpAsync returns token with newlines when response contains newlines in VerificationToken.
    /// Input: Valid email and OTP code with response containing token with newlines.
    /// Expected: Returns the token including newline characters.
    /// </summary>
    [TestMethod]
    public async Task VerifyOtpAsync_ResponseWithNewlinesInToken_ReturnsTokenWithNewlines()
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockTokenStorage = new Mock<TokenStorageService>();
        var mockSession = new Mock<SessionService>();
        var mockLogger = new Mock<ILogger<AuthService>>();
        var authService = new AuthService(mockApi.Object, mockTokenStorage.Object, mockSession.Object, null!, Mock.Of<ICacheService>(), mockLogger.Object);

        string email = "test@example.com";
        string otpCode = "123456";
        string tokenWithNewlines = "line1\nline2\r\nline3";

        var responseDto = new VerifyOtpResponseDto
        {
            Message = "Success",
            VerificationToken = tokenWithNewlines
        };
        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(responseDto)
        };

        mockApi.Setup(x => x.PostAsync(
            "auth/verify-otp",
            It.IsAny<HttpContent>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(httpResponse);

        // Act
        var result = await authService.VerifyOtpAsync(email, otpCode);

        // Assert
        Assert.AreEqual(tokenWithNewlines, result);
    }

    /// <summary>
    /// Tests that VerifyOtpAsync returns token with Unicode characters when response contains them.
    /// Input: Valid email and OTP code with response containing Unicode token.
    /// Expected: Returns the token including Unicode characters.
    /// </summary>
    [TestMethod]
    [DataRow("token-Ã¤Â¸Â­Ã¦â€“â€¡-Ã¦Âµâ€¹Ã¨Â¯â€¢")]
    [DataRow("token-Ã—Â¢Ã—â€˜Ã—Â¨Ã—â„¢Ã—Âª-test")]
    [DataRow("token-Ã°Å¸â€â€™-secure")]
    [DataRow("token-Ãƒâ€˜oÃƒÂ±o-123")]
    public async Task VerifyOtpAsync_ResponseWithUnicodeToken_ReturnsUnicodeToken(string unicodeToken)
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockTokenStorage = new Mock<TokenStorageService>();
        var mockSession = new Mock<SessionService>();
        var mockLogger = new Mock<ILogger<AuthService>>();
        var authService = new AuthService(mockApi.Object, mockTokenStorage.Object, mockSession.Object, null!, Mock.Of<ICacheService>(), mockLogger.Object);

        string email = "test@example.com";
        string otpCode = "123456";

        var responseDto = new VerifyOtpResponseDto
        {
            Message = "Success",
            VerificationToken = unicodeToken
        };
        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(responseDto)
        };

        mockApi.Setup(x => x.PostAsync(
            "auth/verify-otp",
            It.IsAny<HttpContent>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(httpResponse);

        // Act
        var result = await authService.VerifyOtpAsync(email, otpCode);

        // Assert
        Assert.AreEqual(unicodeToken, result);
    }

    /// <summary>
    /// Tests that VerifyOtpAsync handles extremely large OTP codes correctly.
    /// Input: Email with OTP code of 10000 characters.
    /// Expected: Processes successfully and returns token.
    /// </summary>
    [TestMethod]
    public async Task VerifyOtpAsync_ExtremelyLargeOtpCode_ProcessesSuccessfully()
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockTokenStorage = new Mock<TokenStorageService>();
        var mockSession = new Mock<SessionService>();
        var mockLogger = new Mock<ILogger<AuthService>>();
        var authService = new AuthService(mockApi.Object, mockTokenStorage.Object, mockSession.Object, null!, Mock.Of<ICacheService>(), mockLogger.Object);

        string email = "test@example.com";
        string otpCode = new string('1', 10000);
        string expectedToken = "verification-token";

        var responseDto = new VerifyOtpResponseDto
        {
            Message = "Success",
            VerificationToken = expectedToken
        };
        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(responseDto)
        };

        mockApi.Setup(x => x.PostAsync(
            "auth/verify-otp",
            It.IsAny<HttpContent>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(httpResponse);

        // Act
        var result = await authService.VerifyOtpAsync(email, otpCode);

        // Assert
        Assert.AreEqual(expectedToken, result);
    }

    /// <summary>
    /// Tests that VerifyOtpAsync processes requests with both empty email and empty OTP code.
    /// Input: Empty string for both email and otpCode.
    /// Expected: Calls API and returns token based on response.
    /// </summary>
    [TestMethod]
    public async Task VerifyOtpAsync_BothEmptyInputs_ProcessesSuccessfully()
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockTokenStorage = new Mock<TokenStorageService>();
        var mockSession = new Mock<SessionService>();
        var mockLogger = new Mock<ILogger<AuthService>>();
        var authService = new AuthService(mockApi.Object, mockTokenStorage.Object, mockSession.Object, null!, Mock.Of<ICacheService>(), mockLogger.Object);

        string email = "";
        string otpCode = "";
        string expectedToken = "verification-token";

        var responseDto = new VerifyOtpResponseDto
        {
            Message = "Success",
            VerificationToken = expectedToken
        };
        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(responseDto)
        };

        mockApi.Setup(x => x.PostAsync(
            "auth/verify-otp",
            It.IsAny<HttpContent>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(httpResponse);

        // Act
        var result = await authService.VerifyOtpAsync(email, otpCode);

        // Assert
        Assert.AreEqual(expectedToken, result);
    }

    /// <summary>
    /// Tests that VerifyOtpAsync returns null when response has multiple client error status codes.
    /// Input: Valid email and OTP code with various client error codes.
    /// Expected: Returns null for all client error codes.
    /// </summary>
    [TestMethod]
    [DataRow(HttpStatusCode.PaymentRequired)]
    [DataRow(HttpStatusCode.MethodNotAllowed)]
    [DataRow(HttpStatusCode.NotAcceptable)]
    [DataRow(HttpStatusCode.ProxyAuthenticationRequired)]
    [DataRow(HttpStatusCode.RequestTimeout)]
    [DataRow(HttpStatusCode.Gone)]
    [DataRow(HttpStatusCode.LengthRequired)]
    [DataRow(HttpStatusCode.PreconditionFailed)]
    [DataRow(HttpStatusCode.RequestEntityTooLarge)]
    [DataRow(HttpStatusCode.RequestUriTooLong)]
    [DataRow(HttpStatusCode.UnsupportedMediaType)]
    [DataRow(HttpStatusCode.RequestedRangeNotSatisfiable)]
    [DataRow(HttpStatusCode.ExpectationFailed)]
    [DataRow(HttpStatusCode.UpgradeRequired)]
    public async Task VerifyOtpAsync_VariousClientErrorCodes_ReturnsNull(HttpStatusCode statusCode)
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockTokenStorage = new Mock<TokenStorageService>();
        var mockSession = new Mock<SessionService>();
        var mockLogger = new Mock<ILogger<AuthService>>();
        var authService = new AuthService(mockApi.Object, mockTokenStorage.Object, mockSession.Object, null!, Mock.Of<ICacheService>(), mockLogger.Object);

        string email = "test@example.com";
        string otpCode = "123456";

        var httpResponse = new HttpResponseMessage(statusCode);

        mockApi.Setup(x => x.PostAsync(
            "auth/verify-otp",
            It.IsAny<HttpContent>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(httpResponse);

        // Act
        var result = await authService.VerifyOtpAsync(email, otpCode);

        // Assert
        Assert.IsNull(result);
    }

    /// <summary>
    /// Tests that VerifyOtpAsync returns null when response has various server error status codes.
    /// Input: Valid email and OTP code with various server error codes.
    /// Expected: Returns null for all server error codes.
    /// </summary>
    [TestMethod]
    [DataRow(HttpStatusCode.NotImplemented)]
    [DataRow(HttpStatusCode.HttpVersionNotSupported)]
    public async Task VerifyOtpAsync_VariousServerErrorCodes_ReturnsNull(HttpStatusCode statusCode)
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockTokenStorage = new Mock<TokenStorageService>();
        var mockSession = new Mock<SessionService>();
        var mockLogger = new Mock<ILogger<AuthService>>();
        var authService = new AuthService(mockApi.Object, mockTokenStorage.Object, mockSession.Object, null!, Mock.Of<ICacheService>(), mockLogger.Object);

        string email = "test@example.com";
        string otpCode = "123456";

        var httpResponse = new HttpResponseMessage(statusCode);

        mockApi.Setup(x => x.PostAsync(
            "auth/verify-otp",
            It.IsAny<HttpContent>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(httpResponse);

        // Act
        var result = await authService.VerifyOtpAsync(email, otpCode);

        // Assert
        Assert.IsNull(result);
    }

    /// <summary>
    /// Tests that RegisterStudentAsync handles 3xx redirect status codes as non-success.
    /// Input: Valid DTO with API returning redirect status codes.
    /// Expected: Returns false and logs warning with the redirect status code.
    /// </summary>
    [TestMethod]
    [DataRow(HttpStatusCode.MovedPermanently)] // 301
    [DataRow(HttpStatusCode.Found)] // 302
    [DataRow(HttpStatusCode.SeeOther)] // 303
    [DataRow(HttpStatusCode.TemporaryRedirect)] // 307
    [DataRow(308)] // Permanent Redirect
    public async Task RegisterStudentAsync_RedirectStatusCode_ReturnsFalseAndLogsWarning(HttpStatusCode statusCode)
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockTokenStorage = new Mock<TokenStorageService>();
        var mockSession = new Mock<SessionService>();
        var mockLogger = new Mock<ILogger<AuthService>>();

        var dto = new StudentRegisterDto
        {
            Email = "test@example.com",
            FirstName = "John",
            SecondName = "Doe"
        };

        var redirectResponse = new HttpResponseMessage(statusCode);
        mockApi.Setup(x => x.PostAsync("auth/register", It.IsAny<HttpContent>(), It.IsAny<CancellationToken>()))
               .ReturnsAsync(redirectResponse);

        var authService = new AuthService(mockApi.Object, mockTokenStorage.Object, mockSession.Object, null!, Mock.Of<ICacheService>(), mockLogger.Object);

        // Act
        var result = await authService.RegisterStudentAsync(dto);

        // Assert
        Assert.IsFalse(result);
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Student registration failed")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    /// <summary>
    /// Tests that RegisterStudentAsync handles DTO with whitespace-only string properties.
    /// Input: DTO with all non-nullable string properties set to whitespace.
    /// Expected: Processes successfully and calls API with whitespace values.
    /// </summary>
    [TestMethod]
    public async Task RegisterStudentAsync_WhitespaceOnlyStrings_ProcessesSuccessfully()
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockTokenStorage = new Mock<TokenStorageService>();
        var mockSession = new Mock<SessionService>();
        var mockLogger = new Mock<ILogger<AuthService>>();

        var dto = new StudentRegisterDto
        {
            FirstName = "   ",
            SecondName = "\t",
            Email = " \n ",
            Password = "  \r\n  ",
            UniversityId = "   ",
            YearOfStudy = 1
        };

        var successResponse = new HttpResponseMessage(HttpStatusCode.OK);
        mockApi.Setup(x => x.PostAsync("auth/register", It.IsAny<HttpContent>(), It.IsAny<CancellationToken>()))
               .ReturnsAsync(successResponse);

        var authService = new AuthService(mockApi.Object, mockTokenStorage.Object, mockSession.Object, null!, Mock.Of<ICacheService>(), mockLogger.Object);

        // Act
        var result = await authService.RegisterStudentAsync(dto);

        // Assert
        Assert.IsTrue(result);
        mockApi.Verify(
            x => x.PostAsync("auth/register", It.IsAny<JsonContent>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    /// <summary>
    /// Tests that RegisterStudentAsync handles DTO with control characters in string properties.
    /// Input: DTO with control characters (null char, backspace, etc.) in string properties.
    /// Expected: Processes successfully without errors.
    /// </summary>
    [TestMethod]
    public async Task RegisterStudentAsync_ControlCharactersInStrings_ProcessesSuccessfully()
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockTokenStorage = new Mock<TokenStorageService>();
        var mockSession = new Mock<SessionService>();
        var mockLogger = new Mock<ILogger<AuthService>>();

        var dto = new StudentRegisterDto
        {
            Email = "test\0@example.com",
            FirstName = "John\b",
            SecondName = "Doe\u0007",
            Password = "Pass\u001bword",
            UniversityId = "UNI\u0001",
            YearOfStudy = 2
        };

        var successResponse = new HttpResponseMessage(HttpStatusCode.Created);
        mockApi.Setup(x => x.PostAsync("auth/register", It.IsAny<HttpContent>(), It.IsAny<CancellationToken>()))
               .ReturnsAsync(successResponse);

        var authService = new AuthService(mockApi.Object, mockTokenStorage.Object, mockSession.Object, null!, Mock.Of<ICacheService>(), mockLogger.Object);

        // Act
        var result = await authService.RegisterStudentAsync(dto);

        // Assert
        Assert.IsTrue(result);
    }

    /// <summary>
    /// Tests that RegisterStudentAsync handles DTO with maximum length strings in multiple properties.
    /// Input: DTO with extremely long strings (10000+ characters) in multiple properties.
    /// Expected: Processes successfully without errors or truncation.
    /// </summary>
    [TestMethod]
    public async Task RegisterStudentAsync_MaximumLengthStringsInMultipleProperties_ProcessesSuccessfully()
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockTokenStorage = new Mock<TokenStorageService>();
        var mockSession = new Mock<SessionService>();
        var mockLogger = new Mock<ILogger<AuthService>>();

        var veryLongString = new string('A', 10000);
        var dto = new StudentRegisterDto
        {
            Email = veryLongString + "@example.com",
            FirstName = veryLongString,
            SecondName = veryLongString,
            Password = veryLongString,
            UniversityId = veryLongString,
            FacultyId = veryLongString,
            DepartmentId = veryLongString,
            YearOfStudy = 1
        };

        var successResponse = new HttpResponseMessage(HttpStatusCode.OK);
        mockApi.Setup(x => x.PostAsync("auth/register", It.IsAny<HttpContent>(), It.IsAny<CancellationToken>()))
               .ReturnsAsync(successResponse);

        var authService = new AuthService(mockApi.Object, mockTokenStorage.Object, mockSession.Object, null!, Mock.Of<ICacheService>(), mockLogger.Object);

        // Act
        var result = await authService.RegisterStudentAsync(dto);

        // Assert
        Assert.IsTrue(result);
        mockApi.Verify(
            x => x.PostAsync("auth/register", It.IsAny<JsonContent>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    /// <summary>
    /// Tests that RegisterStudentAsync handles DTO with Unicode characters including emojis.
    /// Input: DTO with various Unicode characters, emojis, and multi-byte characters.
    /// Expected: Processes successfully and preserves Unicode characters.
    /// </summary>
    [TestMethod]
    public async Task RegisterStudentAsync_UnicodeAndEmojisInProperties_ProcessesSuccessfully()
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockTokenStorage = new Mock<TokenStorageService>();
        var mockSession = new Mock<SessionService>();
        var mockLogger = new Mock<ILogger<AuthService>>();

        var dto = new StudentRegisterDto
        {
            Email = "Ã§â€Â¨Ã¦Ë†Â·Ã°Å¸â€œÂ§@Ã¤Â¾â€¹Ã£ÂË†.com",
            FirstName = "JosÃƒÂ©",
            SecondName = "FranÃƒÂ§ois",
            OtherNames = "MÃƒÂ¼ller Ã°Å¸Å½â€œ",
            Password = "Ã£Æ’â€˜Ã£â€šÂ¹Ã£Æ’Â¯Ã£Æ’Â¼Ã£Æ’â€°123",
            UniversityId = "Ã¥Â¤Â§Ã¥Â­Â¦001",
            YearOfStudy = 3
        };

        var successResponse = new HttpResponseMessage(HttpStatusCode.OK);
        mockApi.Setup(x => x.PostAsync("auth/register", It.IsAny<HttpContent>(), It.IsAny<CancellationToken>()))
               .ReturnsAsync(successResponse);

        var authService = new AuthService(mockApi.Object, mockTokenStorage.Object, mockSession.Object, null!, Mock.Of<ICacheService>(), mockLogger.Object);

        // Act
        var result = await authService.RegisterStudentAsync(dto);

        // Assert
        Assert.IsTrue(result);
    }

    /// <summary>
    /// Tests that RegisterStudentAsync handles various 1xx informational status codes as non-success.
    /// Input: Valid DTO with API returning informational status codes.
    /// Expected: Returns false and logs warning (informational codes are not success).
    /// </summary>
    [TestMethod]
    [DataRow(HttpStatusCode.Continue)] // 100
    [DataRow(HttpStatusCode.SwitchingProtocols)] // 101
    [DataRow(102)] // Processing
    public async Task RegisterStudentAsync_InformationalStatusCode_ReturnsFalseAndLogsWarning(HttpStatusCode statusCode)
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockTokenStorage = new Mock<TokenStorageService>();
        var mockSession = new Mock<SessionService>();
        var mockLogger = new Mock<ILogger<AuthService>>();

        var dto = new StudentRegisterDto
        {
            Email = "test@example.com",
            FirstName = "John",
            SecondName = "Doe"
        };

        var informationalResponse = new HttpResponseMessage(statusCode);
        mockApi.Setup(x => x.PostAsync("auth/register", It.IsAny<HttpContent>(), It.IsAny<CancellationToken>()))
               .ReturnsAsync(informationalResponse);

        var authService = new AuthService(mockApi.Object, mockTokenStorage.Object, mockSession.Object, null!, Mock.Of<ICacheService>(), mockLogger.Object);

        // Act
        var result = await authService.RegisterStudentAsync(dto);

        // Assert
        Assert.IsFalse(result);
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Student registration failed")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    /// <summary>
    /// Tests that RegisterStudentAsync handles DTO with mixed case and boundary YearOfStudy values.
    /// Input: DTO with YearOfStudy set to positive boundary values.
    /// Expected: Processes successfully with any integer value.
    /// </summary>
    [TestMethod]
    [DataRow(1)]
    [DataRow(10)]
    [DataRow(100)]
    [DataRow(1000)]
    public async Task RegisterStudentAsync_PositiveBoundaryYearOfStudy_ProcessesSuccessfully(int yearOfStudy)
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockTokenStorage = new Mock<TokenStorageService>();
        var mockSession = new Mock<SessionService>();
        var mockLogger = new Mock<ILogger<AuthService>>();

        var dto = new StudentRegisterDto
        {
            Email = "student@example.com",
            FirstName = "Jane",
            SecondName = "Smith",
            YearOfStudy = yearOfStudy
        };

        var successResponse = new HttpResponseMessage(HttpStatusCode.OK);
        mockApi.Setup(x => x.PostAsync("auth/register", It.IsAny<HttpContent>(), It.IsAny<CancellationToken>()))
               .ReturnsAsync(successResponse);

        var authService = new AuthService(mockApi.Object, mockTokenStorage.Object, mockSession.Object, null!, Mock.Of<ICacheService>(), mockLogger.Object);

        // Act
        var result = await authService.RegisterStudentAsync(dto);

        // Assert
        Assert.IsTrue(result);
    }

    /// <summary>
    /// Tests that RegisterStudentAsync does not throw when both nullable properties are null simultaneously.
    /// Input: DTO with both OtherNames and Photo set to null.
    /// Expected: Processes successfully without NullReferenceException.
    /// </summary>
    [TestMethod]
    public async Task RegisterStudentAsync_BothNullablePropertiesNull_ProcessesSuccessfully()
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockTokenStorage = new Mock<TokenStorageService>();
        var mockSession = new Mock<SessionService>();
        var mockLogger = new Mock<ILogger<AuthService>>();

        var dto = new StudentRegisterDto
        {
            Email = "test@example.com",
            FirstName = "John",
            SecondName = "Doe",
            OtherNames = null,
            Photo = null,
            YearOfStudy = 1
        };

        var successResponse = new HttpResponseMessage(HttpStatusCode.OK);
        mockApi.Setup(x => x.PostAsync("auth/register", It.IsAny<HttpContent>(), It.IsAny<CancellationToken>()))
               .ReturnsAsync(successResponse);

        var authService = new AuthService(mockApi.Object, mockTokenStorage.Object, mockSession.Object, null!, Mock.Of<ICacheService>(), mockLogger.Object);

        // Act
        var result = await authService.RegisterStudentAsync(dto);

        // Assert
        Assert.IsTrue(result);
    }

    /// <summary>
    /// Tests that RegisterStudentAsync logs information before making the API call.
    /// Input: Valid DTO.
    /// Expected: Information is logged before PostAsync is invoked.
    /// </summary>
    [TestMethod]
    public async Task RegisterStudentAsync_ValidDto_LogsInformationBeforeApiCall()
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockTokenStorage = new Mock<TokenStorageService>();
        var mockSession = new Mock<SessionService>();
        var mockLogger = new Mock<ILogger<AuthService>>();
        var callSequence = new System.Collections.Generic.List<string>();

        var dto = new StudentRegisterDto
        {
            Email = "sequence@example.com",
            FirstName = "Test",
            SecondName = "User"
        };

        mockLogger.Setup(x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()))
            .Callback(() => callSequence.Add("Log"));

        var successResponse = new HttpResponseMessage(HttpStatusCode.OK);
        mockApi.Setup(x => x.PostAsync("auth/register", It.IsAny<HttpContent>(), It.IsAny<CancellationToken>()))
               .Callback(() => callSequence.Add("PostAsync"))
               .ReturnsAsync(successResponse);

        var authService = new AuthService(mockApi.Object, mockTokenStorage.Object, mockSession.Object, null!, Mock.Of<ICacheService>(), mockLogger.Object);

        // Act
        var result = await authService.RegisterStudentAsync(dto);

        // Assert
        Assert.IsTrue(result);
        Assert.AreEqual(2, callSequence.Count);
        Assert.AreEqual("Log", callSequence[0]);
        Assert.AreEqual("PostAsync", callSequence[1]);
    }

    /// <summary>
    /// Tests that RegisterStudentAsync does not log warning when API call succeeds.
    /// Input: Valid DTO with successful API response.
    /// Expected: Warning log is never called.
    /// </summary>
    [TestMethod]
    public async Task RegisterStudentAsync_SuccessfulResponse_DoesNotLogWarning()
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockTokenStorage = new Mock<TokenStorageService>();
        var mockSession = new Mock<SessionService>();
        var mockLogger = new Mock<ILogger<AuthService>>();

        var dto = new StudentRegisterDto
        {
            Email = "success@example.com",
            FirstName = "Success",
            SecondName = "Test"
        };

        var successResponse = new HttpResponseMessage(HttpStatusCode.OK);
        mockApi.Setup(x => x.PostAsync("auth/register", It.IsAny<HttpContent>(), It.IsAny<CancellationToken>()))
               .ReturnsAsync(successResponse);

        var authService = new AuthService(mockApi.Object, mockTokenStorage.Object, mockSession.Object, null!, Mock.Of<ICacheService>(), mockLogger.Object);

        // Act
        var result = await authService.RegisterStudentAsync(dto);

        // Assert
        Assert.IsTrue(result);
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
    /// Tests that SendOtpAsync calls PostAsync with the correct endpoint.
    /// Input: Valid email address.
    /// Expected: PostAsync is called with "auth/send-otp" endpoint exactly.
    /// </summary>
    [TestMethod]
    public async Task SendOtpAsync_ValidEmail_CallsPostAsyncWithCorrectEndpoint()
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockTokenStorage = new Mock<TokenStorageService>();
        var mockSession = new Mock<SessionService>();
        var mockLogger = new Mock<ILogger<AuthService>>();

        var successResponse = new HttpResponseMessage(HttpStatusCode.OK);
        string? capturedEndpoint = null;

        mockApi.Setup(x => x.PostAsync(
            It.IsAny<string>(),
            It.IsAny<HttpContent>(),
            It.IsAny<CancellationToken>()))
            .Callback<string, HttpContent, CancellationToken>((endpoint, content, ct) => capturedEndpoint = endpoint)
            .ReturnsAsync(successResponse);

        var authService = new AuthService(mockApi.Object, mockTokenStorage.Object, mockSession.Object, null!, Mock.Of<ICacheService>(), mockLogger.Object);
        string email = "user@example.com";

        // Act
        bool result = await authService.SendOtpAsync(email);

        // Assert
        Assert.IsTrue(result);
        Assert.AreEqual("auth/send-otp", capturedEndpoint);
    }

    /// <summary>
    /// Tests that SendOtpAsync handles email with Unicode characters correctly.
    /// Input: Email addresses with various Unicode characters (Chinese, Arabic, emoji).
    /// Expected: Calls API with the provided Unicode string, returns result based on API response.
    /// </summary>
    [TestMethod]
    [DataRow("Ã§â€Â¨Ã¦Ë†Â·@example.com", DisplayName = "Email with Chinese characters")]
    [DataRow("Ã™â€¦Ã˜Â³Ã˜ÂªÃ˜Â®Ã˜Â¯Ã™â€¦@example.com", DisplayName = "Email with Arabic characters")]
    [DataRow("userÃ°Å¸Ëœâ‚¬@example.com", DisplayName = "Email with emoji")]
    [DataRow("Ãƒâ€˜oÃƒÂ±o@example.com", DisplayName = "Email with accented characters")]
    [DataRow("Ãâ€™ÃÂ»ÃÂ°ÃÂ´ÃÂ¸ÃÂ¼ÃÂ¸Ã‘â‚¬@example.com", DisplayName = "Email with Cyrillic characters")]
    public async Task SendOtpAsync_UnicodeCharactersInEmail_CallsApiAndReturnsResult(string email)
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockTokenStorage = new Mock<TokenStorageService>();
        var mockSession = new Mock<SessionService>();
        var mockLogger = new Mock<ILogger<AuthService>>();

        var successResponse = new HttpResponseMessage(HttpStatusCode.OK);
        mockApi.Setup(x => x.PostAsync(
            "auth/send-otp",
            It.IsAny<HttpContent>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(successResponse);

        var authService = new AuthService(mockApi.Object, mockTokenStorage.Object, mockSession.Object, null!, Mock.Of<ICacheService>(), mockLogger.Object);

        // Act
        bool result = await authService.SendOtpAsync(email);

        // Assert
        Assert.IsTrue(result);
        mockApi.Verify(x => x.PostAsync(
            "auth/send-otp",
            It.IsAny<HttpContent>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Tests that SendOtpAsync handles email with control characters correctly.
    /// Input: Email addresses with control characters (null byte, tab, carriage return).
    /// Expected: Calls API with the provided string containing control characters, returns result based on API response.
    /// </summary>
    [TestMethod]
    [DataRow("user\0@example.com", DisplayName = "Email with null byte")]
    [DataRow("user\r@example.com", DisplayName = "Email with carriage return")]
    [DataRow("user\t@example.com", DisplayName = "Email with tab character")]
    [DataRow("user\b@example.com", DisplayName = "Email with backspace character")]
    public async Task SendOtpAsync_ControlCharactersInEmail_CallsApiAndReturnsResult(string email)
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockTokenStorage = new Mock<TokenStorageService>();
        var mockSession = new Mock<SessionService>();
        var mockLogger = new Mock<ILogger<AuthService>>();

        var successResponse = new HttpResponseMessage(HttpStatusCode.OK);
        mockApi.Setup(x => x.PostAsync(
            "auth/send-otp",
            It.IsAny<HttpContent>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(successResponse);

        var authService = new AuthService(mockApi.Object, mockTokenStorage.Object, mockSession.Object, null!, Mock.Of<ICacheService>(), mockLogger.Object);

        // Act
        bool result = await authService.SendOtpAsync(email);

        // Assert
        Assert.IsTrue(result);
        mockApi.Verify(x => x.PostAsync(
            "auth/send-otp",
            It.IsAny<HttpContent>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Tests that SendOtpAsync passes default CancellationToken to PostAsync.
    /// Input: Valid email address.
    /// Expected: PostAsync is called with CancellationToken.None (default).
    /// </summary>
    [TestMethod]
    public async Task SendOtpAsync_ValidEmail_PassesDefaultCancellationToken()
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockTokenStorage = new Mock<TokenStorageService>();
        var mockSession = new Mock<SessionService>();
        var mockLogger = new Mock<ILogger<AuthService>>();

        var successResponse = new HttpResponseMessage(HttpStatusCode.OK);
        CancellationToken capturedToken = default;

        mockApi.Setup(x => x.PostAsync(
            It.IsAny<string>(),
            It.IsAny<HttpContent>(),
            It.IsAny<CancellationToken>()))
            .Callback<string, HttpContent, CancellationToken>((endpoint, content, ct) => capturedToken = ct)
            .ReturnsAsync(successResponse);

        var authService = new AuthService(mockApi.Object, mockTokenStorage.Object, mockSession.Object, null!, Mock.Of<ICacheService>(), mockLogger.Object);
        string email = "user@example.com";

        // Act
        bool result = await authService.SendOtpAsync(email);

        // Assert
        Assert.IsTrue(result);
        Assert.AreEqual(CancellationToken.None, capturedToken);
    }

    /// <summary>
    /// Tests that SendOtpAsync does not log warning when API call succeeds.
    /// Input: Valid email with successful response.
    /// Expected: Returns true, no warning log entry is created.
    /// </summary>
    [TestMethod]
    public async Task SendOtpAsync_SuccessfulResponse_DoesNotLogWarning()
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockTokenStorage = new Mock<TokenStorageService>();
        var mockSession = new Mock<SessionService>();
        var mockLogger = new Mock<ILogger<AuthService>>();

        var successResponse = new HttpResponseMessage(HttpStatusCode.OK);
        mockApi.Setup(x => x.PostAsync(
            "auth/send-otp",
            It.IsAny<HttpContent>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(successResponse);

        var authService = new AuthService(mockApi.Object, mockTokenStorage.Object, mockSession.Object, null!, Mock.Of<ICacheService>(), mockLogger.Object);
        string email = "user@example.com";

        // Act
        bool result = await authService.SendOtpAsync(email);

        // Assert
        Assert.IsTrue(result);
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
    /// Tests that SendOtpAsync logs warning message containing exact status code.
    /// Input: Valid email with failed response.
    /// Expected: Returns false, warning log contains "send-otp failed" and the exact status code.
    /// </summary>
    [TestMethod]
    [DataRow(HttpStatusCode.BadRequest, DisplayName = "BadRequest (400)")]
    [DataRow(HttpStatusCode.InternalServerError, DisplayName = "InternalServerError (500)")]
    public async Task SendOtpAsync_FailedResponse_LogsWarningWithExactStatusCode(HttpStatusCode statusCode)
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockTokenStorage = new Mock<TokenStorageService>();
        var mockSession = new Mock<SessionService>();
        var mockLogger = new Mock<ILogger<AuthService>>();

        var errorResponse = new HttpResponseMessage(statusCode);
        mockApi.Setup(x => x.PostAsync(
            "auth/send-otp",
            It.IsAny<HttpContent>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(errorResponse);

        var authService = new AuthService(mockApi.Object, mockTokenStorage.Object, mockSession.Object, null!, Mock.Of<ICacheService>(), mockLogger.Object);
        string email = "user@example.com";

        // Act
        bool result = await authService.SendOtpAsync(email);

        // Assert
        Assert.IsFalse(result);
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("send-otp failed") && v.ToString()!.Contains(statusCode.ToString())),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    /// <summary>
    /// Tests that SendOtpAsync handles HttpResponseMessage with all 1xx informational status codes.
    /// Input: Valid email with 1xx status codes.
    /// Expected: Returns false (not success), logs warning.
    /// </summary>
    [TestMethod]
    [DataRow(HttpStatusCode.Continue, DisplayName = "Continue (100)")]
    [DataRow(HttpStatusCode.SwitchingProtocols, DisplayName = "SwitchingProtocols (101)")]
    public async Task SendOtpAsync_InformationalStatusCodes_ReturnsFalseAndLogsWarning(HttpStatusCode statusCode)
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockTokenStorage = new Mock<TokenStorageService>();
        var mockSession = new Mock<SessionService>();
        var mockLogger = new Mock<ILogger<AuthService>>();

        var informationalResponse = new HttpResponseMessage(statusCode);
        mockApi.Setup(x => x.PostAsync(
            "auth/send-otp",
            It.IsAny<HttpContent>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(informationalResponse);

        var authService = new AuthService(mockApi.Object, mockTokenStorage.Object, mockSession.Object, null!, Mock.Of<ICacheService>(), mockLogger.Object);
        string email = "user@example.com";

        // Act
        bool result = await authService.SendOtpAsync(email);

        // Assert
        Assert.IsFalse(result);
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("send-otp failed")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    /// <summary>
    /// Tests that SendOtpAsync handles HttpResponseMessage with 3xx redirection status codes.
    /// Input: Valid email with 3xx status codes.
    /// Expected: Returns false (not success), logs warning.
    /// </summary>
    [TestMethod]
    [DataRow(HttpStatusCode.MovedPermanently, DisplayName = "MovedPermanently (301)")]
    [DataRow(HttpStatusCode.Found, DisplayName = "Found (302)")]
    [DataRow(HttpStatusCode.NotModified, DisplayName = "NotModified (304)")]
    [DataRow(HttpStatusCode.TemporaryRedirect, DisplayName = "TemporaryRedirect (307)")]
    public async Task SendOtpAsync_RedirectionStatusCodes_ReturnsFalseAndLogsWarning(HttpStatusCode statusCode)
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockTokenStorage = new Mock<TokenStorageService>();
        var mockSession = new Mock<SessionService>();
        var mockLogger = new Mock<ILogger<AuthService>>();

        var redirectionResponse = new HttpResponseMessage(statusCode);
        mockApi.Setup(x => x.PostAsync(
            "auth/send-otp",
            It.IsAny<HttpContent>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(redirectionResponse);

        var authService = new AuthService(mockApi.Object, mockTokenStorage.Object, mockSession.Object, null!, Mock.Of<ICacheService>(), mockLogger.Object);
        string email = "user@example.com";

        // Act
        bool result = await authService.SendOtpAsync(email);

        // Assert
        Assert.IsFalse(result);
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("send-otp failed")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    /// <summary>
    /// Tests that SendOtpAsync calls PostAsync exactly once.
    /// Input: Valid email address.
    /// Expected: PostAsync is called exactly once, no retries.
    /// </summary>
    [TestMethod]
    public async Task SendOtpAsync_ValidEmail_CallsPostAsyncExactlyOnce()
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockTokenStorage = new Mock<TokenStorageService>();
        var mockSession = new Mock<SessionService>();
        var mockLogger = new Mock<ILogger<AuthService>>();

        var successResponse = new HttpResponseMessage(HttpStatusCode.OK);
        mockApi.Setup(x => x.PostAsync(
            "auth/send-otp",
            It.IsAny<HttpContent>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(successResponse);

        var authService = new AuthService(mockApi.Object, mockTokenStorage.Object, mockSession.Object, null!, Mock.Of<ICacheService>(), mockLogger.Object);
        string email = "user@example.com";

        // Act
        bool result = await authService.SendOtpAsync(email);

        // Assert
        Assert.IsTrue(result);
        mockApi.Verify(x => x.PostAsync(
            It.IsAny<string>(),
            It.IsAny<HttpContent>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Tests that SendOtpAsync handles email with maximum string length correctly.
    /// Input: Email string at maximum practical length boundary.
    /// Expected: Calls API with the entire long string, returns result based on API response.
    /// </summary>
    [TestMethod]
    public async Task SendOtpAsync_MaximumLengthEmail_CallsApiAndReturnsResult()
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockTokenStorage = new Mock<TokenStorageService>();
        var mockSession = new Mock<SessionService>();
        var mockLogger = new Mock<ILogger<AuthService>>();

        var successResponse = new HttpResponseMessage(HttpStatusCode.OK);
        mockApi.Setup(x => x.PostAsync(
            "auth/send-otp",
            It.IsAny<HttpContent>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(successResponse);

        var authService = new AuthService(mockApi.Object, mockTokenStorage.Object, mockSession.Object, null!, Mock.Of<ICacheService>(), mockLogger.Object);
        string email = new string('a', 10000) + "@example.com";

        // Act
        bool result = await authService.SendOtpAsync(email);

        // Assert
        Assert.IsTrue(result);
        mockApi.Verify(x => x.PostAsync(
            "auth/send-otp",
            It.IsAny<HttpContent>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Tests that SendOtpAsync returns false for client error status codes beyond common ones.
    /// Input: Valid email with various 4xx client error status codes.
    /// Expected: Returns false, logs warning.
    /// </summary>
    [TestMethod]
    [DataRow(HttpStatusCode.MethodNotAllowed, DisplayName = "MethodNotAllowed (405)")]
    [DataRow(HttpStatusCode.NotAcceptable, DisplayName = "NotAcceptable (406)")]
    [DataRow(HttpStatusCode.RequestTimeout, DisplayName = "RequestTimeout (408)")]
    [DataRow(HttpStatusCode.Gone, DisplayName = "Gone (410)")]
    [DataRow(HttpStatusCode.PreconditionFailed, DisplayName = "PreconditionFailed (412)")]
    [DataRow(HttpStatusCode.UnsupportedMediaType, DisplayName = "UnsupportedMediaType (415)")]
    [DataRow((HttpStatusCode)429, DisplayName = "TooManyRequests (429)")]
    public async Task SendOtpAsync_VariousClientErrorStatusCodes_ReturnsFalseAndLogsWarning(HttpStatusCode statusCode)
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockTokenStorage = new Mock<TokenStorageService>();
        var mockSession = new Mock<SessionService>();
        var mockLogger = new Mock<ILogger<AuthService>>();

        var errorResponse = new HttpResponseMessage(statusCode);
        mockApi.Setup(x => x.PostAsync(
            "auth/send-otp",
            It.IsAny<HttpContent>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(errorResponse);

        var authService = new AuthService(mockApi.Object, mockTokenStorage.Object, mockSession.Object, null!, Mock.Of<ICacheService>(), mockLogger.Object);
        string email = "user@example.com";

        // Act
        bool result = await authService.SendOtpAsync(email);

        // Assert
        Assert.IsFalse(result);
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("send-otp failed")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    /// <summary>
    /// Tests that SendOtpAsync returns false for server error status codes beyond common ones.
    /// Input: Valid email with various 5xx server error status codes.
    /// Expected: Returns false, logs warning.
    /// </summary>
    [TestMethod]
    [DataRow(HttpStatusCode.NotImplemented, DisplayName = "NotImplemented (501)")]
    [DataRow(HttpStatusCode.GatewayTimeout, DisplayName = "GatewayTimeout (504)")]
    [DataRow(HttpStatusCode.HttpVersionNotSupported, DisplayName = "HttpVersionNotSupported (505)")]
    public async Task SendOtpAsync_VariousServerErrorStatusCodes_ReturnsFalseAndLogsWarning(HttpStatusCode statusCode)
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockTokenStorage = new Mock<TokenStorageService>();
        var mockSession = new Mock<SessionService>();
        var mockLogger = new Mock<ILogger<AuthService>>();

        var errorResponse = new HttpResponseMessage(statusCode);
        mockApi.Setup(x => x.PostAsync(
            "auth/send-otp",
            It.IsAny<HttpContent>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(errorResponse);

        var authService = new AuthService(mockApi.Object, mockTokenStorage.Object, mockSession.Object, null!, Mock.Of<ICacheService>(), mockLogger.Object);
        string email = "user@example.com";

        // Act
        bool result = await authService.SendOtpAsync(email);

        // Assert
        Assert.IsFalse(result);
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("send-otp failed")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
}