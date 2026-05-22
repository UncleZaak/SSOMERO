using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Moq.Protected;
using Ssomero.Interfaces;
using Ssomero.Models;
using Ssomero.Services;

namespace Ssomero.Services.UnitTests;




/// <summary>
/// Unit tests for the <see cref="ApiService"/> class.
/// </summary>
[TestClass]
public class ApiServiceTests
{
    /// <summary>
    /// Tests that the constructor accepts a null TokenStorageService parameter.
    /// Verifies the constructor completes without throwing during construction,
    /// even though this violates the non-nullable contract.
    /// </summary>
    [TestMethod]
    public void Constructor_WithNullTokenStorage_DoesNotThrowDuringConstruction()
    {
        // Arrange
        HttpClient client = new HttpClient();
        TokenStorageService? tokenStorage = null;
        Mock<ILogger<ApiService>> loggerMock = new Mock<ILogger<ApiService>>();

        // Act
        ApiService service = new ApiService(client, tokenStorage!, loggerMock.Object);

        // Assert
        Assert.IsNotNull(service);
    }

    /// <summary>
    /// Tests that the constructor accepts all null parameters.
    /// Verifies the constructor completes without throwing during construction,
    /// even though this violates all non-nullable contracts.
    /// </summary>
    [TestMethod]
    public void Constructor_WithAllNullParameters_DoesNotThrowDuringConstruction()
    {
        // Arrange
        HttpClient? client = null;
        TokenStorageService? tokenStorage = null;
        ILogger<ApiService>? logger = null;

        // Act
        ApiService service = new ApiService(client!, tokenStorage!, logger!);

        // Assert
        Assert.IsNotNull(service);
    }

    /// <summary>
    /// Tests that DeleteAsync successfully sends a DELETE request and returns the response for a valid path.
    /// Input: Valid path "/api/resource"
    /// Expected: HttpResponseMessage with StatusCode OK is returned.
    /// </summary>
    [TestMethod]
    public async Task DeleteAsync_ValidPath_ReturnsSuccessResponse()
    {
        // Arrange
        var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
        mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req => req.Method == HttpMethod.Delete),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("{}")
            });

        var httpClient = new HttpClient(mockHttpMessageHandler.Object)
        {
            BaseAddress = new Uri("https://api.example.com")
        };

        var mockTokenStorage = new Mock<TokenStorageService>();
        mockTokenStorage.Setup(x => x.GetAccessTokenAsync()).ReturnsAsync("valid-token");

        var mockLogger = new Mock<ILogger<ApiService>>();
        var apiService = new ApiService(httpClient, mockTokenStorage.Object, mockLogger.Object);

        // Act
        var response = await apiService.DeleteAsync("/api/resource");

        // Assert
        Assert.IsNotNull(response);
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
    }

    /// <summary>
    /// Tests that DeleteAsync handles empty path string correctly.
    /// Input: Empty string path
    /// Expected: Request is sent with empty path.
    /// </summary>
    [TestMethod]
    [DataRow("")]
    [DataRow("   ")]
    public async Task DeleteAsync_EmptyOrWhitespacePath_SendsRequest(string path)
    {
        // Arrange
        var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
        mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage { StatusCode = HttpStatusCode.OK });

        var httpClient = new HttpClient(mockHttpMessageHandler.Object)
        {
            BaseAddress = new Uri("https://api.example.com")
        };

        var mockTokenStorage = new Mock<TokenStorageService>();
        mockTokenStorage.Setup(x => x.GetAccessTokenAsync()).ReturnsAsync(string.Empty);

        var mockLogger = new Mock<ILogger<ApiService>>();
        var apiService = new ApiService(httpClient, mockTokenStorage.Object, mockLogger.Object);

        // Act
        var response = await apiService.DeleteAsync(path);

        // Assert
        Assert.IsNotNull(response);
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
    }

    /// <summary>
    /// Tests that DeleteAsync handles paths with special characters correctly.
    /// Input: Paths with various special characters
    /// Expected: Request is sent successfully.
    /// </summary>
    [TestMethod]
    [DataRow("/api/resource?id=123&sort=asc")]
    [DataRow("/api/resource/with spaces")]
    [DataRow("/api/resource/with/special/chars/@#$%")]
    [DataRow("/api/very/long/path/segment/that/continues/for/many/characters/to/test/boundary/conditions/and/ensure/proper/handling")]
    public async Task DeleteAsync_PathWithSpecialCharacters_SendsRequest(string path)
    {
        // Arrange
        var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
        mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage { StatusCode = HttpStatusCode.OK });

        var httpClient = new HttpClient(mockHttpMessageHandler.Object)
        {
            BaseAddress = new Uri("https://api.example.com")
        };

        var mockTokenStorage = new Mock<TokenStorageService>();
        mockTokenStorage.Setup(x => x.GetAccessTokenAsync()).ReturnsAsync("token");

        var mockLogger = new Mock<ILogger<ApiService>>();
        var apiService = new ApiService(httpClient, mockTokenStorage.Object, mockLogger.Object);

        // Act
        var response = await apiService.DeleteAsync(path);

        // Assert
        Assert.IsNotNull(response);
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
    }

    /// <summary>
    /// Tests that DeleteAsync returns non-401 error responses directly without retry logic.
    /// Input: Various HTTP error status codes
    /// Expected: Response with the error status code is returned without retry.
    /// </summary>
    [TestMethod]
    [DataRow(HttpStatusCode.NotFound)]
    [DataRow(HttpStatusCode.BadRequest)]
    [DataRow(HttpStatusCode.InternalServerError)]
    [DataRow(HttpStatusCode.Forbidden)]
    [DataRow(HttpStatusCode.ServiceUnavailable)]
    public async Task DeleteAsync_NonUnauthorizedErrorResponse_ReturnsResponseDirectly(HttpStatusCode statusCode)
    {
        // Arrange
        var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
        mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage { StatusCode = statusCode });

        var httpClient = new HttpClient(mockHttpMessageHandler.Object)
        {
            BaseAddress = new Uri("https://api.example.com")
        };

        var mockTokenStorage = new Mock<TokenStorageService>();
        mockTokenStorage.Setup(x => x.GetAccessTokenAsync()).ReturnsAsync("token");

        var mockLogger = new Mock<ILogger<ApiService>>();
        var apiService = new ApiService(httpClient, mockTokenStorage.Object, mockLogger.Object);

        // Act
        var response = await apiService.DeleteAsync("/api/resource");

        // Assert
        Assert.IsNotNull(response);
        Assert.AreEqual(statusCode, response.StatusCode);
    }

    /// <summary>
    /// Tests that DeleteAsync uses default cancellation token when none is provided.
    /// Input: No cancellation token (uses default)
    /// Expected: Request is sent successfully with default token.
    /// </summary>
    [TestMethod]
    public async Task DeleteAsync_DefaultCancellationToken_SendsRequestSuccessfully()
    {
        // Arrange
        var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
        mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage { StatusCode = HttpStatusCode.NoContent });

        var httpClient = new HttpClient(mockHttpMessageHandler.Object)
        {
            BaseAddress = new Uri("https://api.example.com")
        };

        var mockTokenStorage = new Mock<TokenStorageService>();
        mockTokenStorage.Setup(x => x.GetAccessTokenAsync()).ReturnsAsync("token");

        var mockLogger = new Mock<ILogger<ApiService>>();
        var apiService = new ApiService(httpClient, mockTokenStorage.Object, mockLogger.Object);

        // Act
        var response = await apiService.DeleteAsync("/api/resource");

        // Assert
        Assert.IsNotNull(response);
        Assert.AreEqual(HttpStatusCode.NoContent, response.StatusCode);
    }

    /// <summary>
    /// Tests that DeleteAsync sends DELETE method specifically (not other HTTP methods).
    /// Input: Valid path
    /// Expected: HttpMethod.Delete is used in the request.
    /// </summary>
    [TestMethod]
    public async Task DeleteAsync_ValidRequest_UsesDeleteHttpMethod()
    {
        // Arrange
        HttpRequestMessage? capturedRequest = null;
        var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
        mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .Callback<HttpRequestMessage, CancellationToken>((req, ct) => capturedRequest = req)
            .ReturnsAsync(new HttpResponseMessage { StatusCode = HttpStatusCode.OK });

        var httpClient = new HttpClient(mockHttpMessageHandler.Object)
        {
            BaseAddress = new Uri("https://api.example.com")
        };

        var mockTokenStorage = new Mock<TokenStorageService>();
        mockTokenStorage.Setup(x => x.GetAccessTokenAsync()).ReturnsAsync("token");

        var mockLogger = new Mock<ILogger<ApiService>>();
        var apiService = new ApiService(httpClient, mockTokenStorage.Object, mockLogger.Object);

        // Act
        await apiService.DeleteAsync("/api/resource");

        // Assert
        Assert.IsNotNull(capturedRequest);
        Assert.AreEqual(HttpMethod.Delete, capturedRequest.Method);
    }

    /// <summary>
    /// Tests that DeleteAsync does not include content in the DELETE request.
    /// Input: Valid path
    /// Expected: Request has no content body.
    /// </summary>
    [TestMethod]
    public async Task DeleteAsync_ValidRequest_HasNoContent()
    {
        // Arrange
        HttpRequestMessage? capturedRequest = null;
        var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
        mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .Callback<HttpRequestMessage, CancellationToken>((req, ct) => capturedRequest = req)
            .ReturnsAsync(new HttpResponseMessage { StatusCode = HttpStatusCode.OK });

        var httpClient = new HttpClient(mockHttpMessageHandler.Object)
        {
            BaseAddress = new Uri("https://api.example.com")
        };

        var mockTokenStorage = new Mock<TokenStorageService>();
        mockTokenStorage.Setup(x => x.GetAccessTokenAsync()).ReturnsAsync("token");

        var mockLogger = new Mock<ILogger<ApiService>>();
        var apiService = new ApiService(httpClient, mockTokenStorage.Object, mockLogger.Object);

        // Act
        await apiService.DeleteAsync("/api/resource");

        // Assert
        Assert.IsNotNull(capturedRequest);
        Assert.IsNull(capturedRequest.Content);
    }

    /// <summary>
    /// Tests that GetAsync successfully sends a GET request with a valid path and returns the response.
    /// Input: Valid path "api/test", default cancellation token
    /// Expected: Returns HttpResponseMessage with OK status
    /// </summary>
    [TestMethod]
    public async Task GetAsync_ValidPath_ReturnsSuccessResponse()
    {
        // Arrange
        var path = "api/test";
        var expectedResponse = new HttpResponseMessage(HttpStatusCode.OK);
        var mockHandler = new Mock<HttpMessageHandler>();
        mockHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req => req.Method == HttpMethod.Get && req.RequestUri != null && req.RequestUri.ToString().Contains(path)),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(expectedResponse);
        var httpClient = new HttpClient(mockHandler.Object) { BaseAddress = new Uri("https://api.example.com/") };
        var mockTokenStorage = new Mock<TokenStorageService>();
        mockTokenStorage.Setup(ts => ts.GetAccessTokenAsync()).ReturnsAsync(string.Empty);
        var mockLogger = new Mock<ILogger<ApiService>>();
        var apiService = new ApiService(httpClient, mockTokenStorage.Object, mockLogger.Object);

        // Act
        var result = await apiService.GetAsync(path);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(HttpStatusCode.OK, result.StatusCode);
        mockHandler.Protected().Verify(
            "SendAsync",
            Times.Once(),
            ItExpr.Is<HttpRequestMessage>(req => req.Method == HttpMethod.Get),
            ItExpr.IsAny<CancellationToken>());
    }

    /// <summary>
    /// Tests that GetAsync handles an empty string path.
    /// Input: Empty string path
    /// Expected: Request is sent with empty path
    /// </summary>
    [TestMethod]
    public async Task GetAsync_EmptyPath_SendsRequestWithEmptyPath()
    {
        // Arrange
        var path = "";
        var expectedResponse = new HttpResponseMessage(HttpStatusCode.OK);
        var mockHandler = new Mock<HttpMessageHandler>();
        mockHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(expectedResponse);
        var httpClient = new HttpClient(mockHandler.Object) { BaseAddress = new Uri("https://api.example.com/") };
        var mockTokenStorage = new Mock<TokenStorageService>();
        mockTokenStorage.Setup(ts => ts.GetAccessTokenAsync()).ReturnsAsync(string.Empty);
        var mockLogger = new Mock<ILogger<ApiService>>();
        var apiService = new ApiService(httpClient, mockTokenStorage.Object, mockLogger.Object);

        // Act
        var result = await apiService.GetAsync(path);

        // Assert
        Assert.IsNotNull(result);
        mockHandler.Protected().Verify(
            "SendAsync",
            Times.Once(),
            ItExpr.IsAny<HttpRequestMessage>(),
            ItExpr.IsAny<CancellationToken>());
    }

    /// <summary>
    /// Tests that GetAsync handles a whitespace-only path.
    /// Input: Whitespace-only string path
    /// Expected: Request is sent with whitespace path
    /// </summary>
    [TestMethod]
    public async Task GetAsync_WhitespacePath_SendsRequestWithWhitespacePath()
    {
        // Arrange
        var path = "   ";
        var expectedResponse = new HttpResponseMessage(HttpStatusCode.OK);
        var mockHandler = new Mock<HttpMessageHandler>();
        mockHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(expectedResponse);
        var httpClient = new HttpClient(mockHandler.Object) { BaseAddress = new Uri("https://api.example.com/") };
        var mockTokenStorage = new Mock<TokenStorageService>();
        mockTokenStorage.Setup(ts => ts.GetAccessTokenAsync()).ReturnsAsync(string.Empty);
        var mockLogger = new Mock<ILogger<ApiService>>();
        var apiService = new ApiService(httpClient, mockTokenStorage.Object, mockLogger.Object);

        // Act
        var result = await apiService.GetAsync(path);

        // Assert
        Assert.IsNotNull(result);
        mockHandler.Protected().Verify(
            "SendAsync",
            Times.Once(),
            ItExpr.IsAny<HttpRequestMessage>(),
            ItExpr.IsAny<CancellationToken>());
    }

    /// <summary>
    /// Tests that GetAsync handles a very long path string.
    /// Input: Very long path (1000+ characters)
    /// Expected: Request is sent successfully with the long path
    /// </summary>
    [TestMethod]
    public async Task GetAsync_VeryLongPath_SendsRequestSuccessfully()
    {
        // Arrange
        var path = new string('a', 1000);
        var expectedResponse = new HttpResponseMessage(HttpStatusCode.OK);
        var mockHandler = new Mock<HttpMessageHandler>();
        mockHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(expectedResponse);
        var httpClient = new HttpClient(mockHandler.Object) { BaseAddress = new Uri("https://api.example.com/") };
        var mockTokenStorage = new Mock<TokenStorageService>();
        mockTokenStorage.Setup(ts => ts.GetAccessTokenAsync()).ReturnsAsync(string.Empty);
        var mockLogger = new Mock<ILogger<ApiService>>();
        var apiService = new ApiService(httpClient, mockTokenStorage.Object, mockLogger.Object);

        // Act
        var result = await apiService.GetAsync(path);

        // Assert
        Assert.IsNotNull(result);
        mockHandler.Protected().Verify(
            "SendAsync",
            Times.Once(),
            ItExpr.IsAny<HttpRequestMessage>(),
            ItExpr.IsAny<CancellationToken>());
    }

    /// <summary>
    /// Tests that GetAsync handles a path with special characters.
    /// Input: Path with special characters (?, &, =, %)
    /// Expected: Request is sent successfully with special characters
    /// </summary>
    [TestMethod]
    public async Task GetAsync_PathWithSpecialCharacters_SendsRequestSuccessfully()
    {
        // Arrange
        var path = "api/test?param1=value1&param2=value%202";
        var expectedResponse = new HttpResponseMessage(HttpStatusCode.OK);
        var mockHandler = new Mock<HttpMessageHandler>();
        mockHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(expectedResponse);
        var httpClient = new HttpClient(mockHandler.Object) { BaseAddress = new Uri("https://api.example.com/") };
        var mockTokenStorage = new Mock<TokenStorageService>();
        mockTokenStorage.Setup(ts => ts.GetAccessTokenAsync()).ReturnsAsync(string.Empty);
        var mockLogger = new Mock<ILogger<ApiService>>();
        var apiService = new ApiService(httpClient, mockTokenStorage.Object, mockLogger.Object);

        // Act
        var result = await apiService.GetAsync(path);

        // Assert
        Assert.IsNotNull(result);
        mockHandler.Protected().Verify(
            "SendAsync",
            Times.Once(),
            ItExpr.IsAny<HttpRequestMessage>(),
            ItExpr.IsAny<CancellationToken>());
    }

    /// <summary>
    /// Tests that GetAsync passes the default cancellation token when not provided.
    /// Input: Valid path, no cancellation token provided
    /// Expected: Request is sent successfully with default token
    /// </summary>
    [TestMethod]
    public async Task GetAsync_NoTokenProvided_UsesDefaultToken()
    {
        // Arrange
        var path = "api/test";
        var expectedResponse = new HttpResponseMessage(HttpStatusCode.OK);
        var mockHandler = new Mock<HttpMessageHandler>();
        mockHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.Is<CancellationToken>(ct => ct == CancellationToken.None))
            .ReturnsAsync(expectedResponse);
        var httpClient = new HttpClient(mockHandler.Object) { BaseAddress = new Uri("https://api.example.com/") };
        var mockTokenStorage = new Mock<TokenStorageService>();
        mockTokenStorage.Setup(ts => ts.GetAccessTokenAsync()).ReturnsAsync(string.Empty);
        var mockLogger = new Mock<ILogger<ApiService>>();
        var apiService = new ApiService(httpClient, mockTokenStorage.Object, mockLogger.Object);

        // Act
        var result = await apiService.GetAsync(path);

        // Assert
        Assert.IsNotNull(result);
        mockHandler.Protected().Verify(
            "SendAsync",
            Times.Once(),
            ItExpr.IsAny<HttpRequestMessage>(),
            ItExpr.Is<CancellationToken>(ct => ct == CancellationToken.None));
    }

    /// <summary>
    /// Tests that GetAsync returns various HTTP status codes correctly.
    /// Input: Valid path, various status codes (404, 500, 201)
    /// Expected: Returns response with the appropriate status code
    /// </summary>
    [TestMethod]
    [DataRow(HttpStatusCode.NotFound)]
    [DataRow(HttpStatusCode.InternalServerError)]
    [DataRow(HttpStatusCode.Created)]
    [DataRow(HttpStatusCode.NoContent)]
    [DataRow(HttpStatusCode.BadRequest)]
    public async Task GetAsync_VariousStatusCodes_ReturnsCorrectStatusCode(HttpStatusCode statusCode)
    {
        // Arrange
        var path = "api/test";
        var expectedResponse = new HttpResponseMessage(statusCode);
        var mockHandler = new Mock<HttpMessageHandler>();
        mockHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(expectedResponse);
        var httpClient = new HttpClient(mockHandler.Object) { BaseAddress = new Uri("https://api.example.com/") };
        var mockTokenStorage = new Mock<TokenStorageService>();
        mockTokenStorage.Setup(ts => ts.GetAccessTokenAsync()).ReturnsAsync(string.Empty);
        var mockLogger = new Mock<ILogger<ApiService>>();
        var apiService = new ApiService(httpClient, mockTokenStorage.Object, mockLogger.Object);

        // Act
        var result = await apiService.GetAsync(path);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(statusCode, result.StatusCode);
    }

    /// <summary>
    /// Tests that GetAsync uses HttpMethod.Get for the request.
    /// Input: Valid path
    /// Expected: Request is sent with GET method
    /// </summary>
    [TestMethod]
    public async Task GetAsync_ValidPath_UsesGetHttpMethod()
    {
        // Arrange
        var path = "api/test";
        var expectedResponse = new HttpResponseMessage(HttpStatusCode.OK);
        HttpRequestMessage? capturedRequest = null;
        var mockHandler = new Mock<HttpMessageHandler>();
        mockHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .Callback<HttpRequestMessage, CancellationToken>((req, ct) => capturedRequest = req)
            .ReturnsAsync(expectedResponse);
        var httpClient = new HttpClient(mockHandler.Object) { BaseAddress = new Uri("https://api.example.com/") };
        var mockTokenStorage = new Mock<TokenStorageService>();
        mockTokenStorage.Setup(ts => ts.GetAccessTokenAsync()).ReturnsAsync(string.Empty);
        var mockLogger = new Mock<ILogger<ApiService>>();
        var apiService = new ApiService(httpClient, mockTokenStorage.Object, mockLogger.Object);

        // Act
        await apiService.GetAsync(path);

        // Assert
        Assert.IsNotNull(capturedRequest);
        Assert.AreEqual(HttpMethod.Get, capturedRequest.Method);
    }

    /// <summary>
    /// Tests that GetAsync does not include content in the request.
    /// Input: Valid path
    /// Expected: Request Content is null
    /// </summary>
    [TestMethod]
    public async Task GetAsync_ValidPath_RequestContentIsNull()
    {
        // Arrange
        var path = "api/test";
        var expectedResponse = new HttpResponseMessage(HttpStatusCode.OK);
        HttpRequestMessage? capturedRequest = null;
        var mockHandler = new Mock<HttpMessageHandler>();
        mockHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .Callback<HttpRequestMessage, CancellationToken>((req, ct) => capturedRequest = req)
            .ReturnsAsync(expectedResponse);
        var httpClient = new HttpClient(mockHandler.Object) { BaseAddress = new Uri("https://api.example.com/") };
        var mockTokenStorage = new Mock<TokenStorageService>();
        mockTokenStorage.Setup(ts => ts.GetAccessTokenAsync()).ReturnsAsync(string.Empty);
        var mockLogger = new Mock<ILogger<ApiService>>();
        var apiService = new ApiService(httpClient, mockTokenStorage.Object, mockLogger.Object);

        // Act
        await apiService.GetAsync(path);

        // Assert
        Assert.IsNotNull(capturedRequest);
        Assert.IsNull(capturedRequest.Content);
    }

    /// <summary>
    /// Tests that CheckHealthAsync returns true when the health endpoint responds with a success status code.
    /// </summary>
    [TestMethod]
    public async Task CheckHealthAsync_SuccessStatusCode_ReturnsTrue()
    {
        // Arrange
        var mockHandler = new Mock<HttpMessageHandler>();
        mockHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req => req.Method == HttpMethod.Get && req.RequestUri!.ToString().EndsWith("health")),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK
            });

        var httpClient = new HttpClient(mockHandler.Object)
        {
            BaseAddress = new Uri("http://localhost/")
        };

        var mockTokenStorage = new Mock<TokenStorageService>(Mock.Of<ILogger<TokenStorageService>>());
        var mockLogger = new Mock<ILogger<ApiService>>();
        var apiService = new ApiService(httpClient, mockTokenStorage.Object, mockLogger.Object);

        // Act
        bool result = await apiService.CheckHealthAsync();

        // Assert
        Assert.IsTrue(result);
    }

    /// <summary>
    /// Tests that CheckHealthAsync returns false when the health endpoint responds with a non-success status code.
    /// </summary>
    /// <param name="statusCode">The HTTP status code to test.</param>
    [TestMethod]
    [DataRow(HttpStatusCode.BadRequest)]
    [DataRow(HttpStatusCode.Unauthorized)]
    [DataRow(HttpStatusCode.Forbidden)]
    [DataRow(HttpStatusCode.NotFound)]
    [DataRow(HttpStatusCode.InternalServerError)]
    [DataRow(HttpStatusCode.ServiceUnavailable)]
    [DataRow(HttpStatusCode.BadGateway)]
    public async Task CheckHealthAsync_NonSuccessStatusCode_ReturnsFalse(HttpStatusCode statusCode)
    {
        // Arrange
        var mockHandler = new Mock<HttpMessageHandler>();
        mockHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req => req.Method == HttpMethod.Get && req.RequestUri!.ToString().EndsWith("health")),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = statusCode
            });

        var httpClient = new HttpClient(mockHandler.Object)
        {
            BaseAddress = new Uri("http://localhost/")
        };

        var mockTokenStorage = new Mock<TokenStorageService>(Mock.Of<ILogger<TokenStorageService>>());
        var mockLogger = new Mock<ILogger<ApiService>>();
        var apiService = new ApiService(httpClient, mockTokenStorage.Object, mockLogger.Object);

        // Act
        bool result = await apiService.CheckHealthAsync();

        // Assert
        Assert.IsFalse(result);
    }

    /// <summary>
    /// Tests that CheckHealthAsync returns false when a TaskCanceledException is thrown due to timeout.
    /// </summary>
    [TestMethod]
    public async Task CheckHealthAsync_TaskCanceledException_ReturnsFalse()
    {
        // Arrange
        var mockHandler = new Mock<HttpMessageHandler>();
        mockHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new TaskCanceledException("The operation was canceled."));

        var httpClient = new HttpClient(mockHandler.Object)
        {
            BaseAddress = new Uri("http://localhost/")
        };

        var mockTokenStorage = new Mock<TokenStorageService>(Mock.Of<ILogger<TokenStorageService>>());
        var mockLogger = new Mock<ILogger<ApiService>>();
        var apiService = new ApiService(httpClient, mockTokenStorage.Object, mockLogger.Object);

        // Act
        bool result = await apiService.CheckHealthAsync();

        // Assert
        Assert.IsFalse(result);
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => true),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception?, string>>((v, t) => true)),
            Times.Once);
    }

    /// <summary>
    /// Tests that CheckHealthAsync returns false when an HttpRequestException is thrown.
    /// </summary>
    [TestMethod]
    public async Task CheckHealthAsync_HttpRequestException_ReturnsFalse()
    {
        // Arrange
        var mockHandler = new Mock<HttpMessageHandler>();
        mockHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new HttpRequestException("Network error"));

        var httpClient = new HttpClient(mockHandler.Object)
        {
            BaseAddress = new Uri("http://localhost/")
        };

        var mockTokenStorage = new Mock<TokenStorageService>(Mock.Of<ILogger<TokenStorageService>>());
        var mockLogger = new Mock<ILogger<ApiService>>();
        var apiService = new ApiService(httpClient, mockTokenStorage.Object, mockLogger.Object);

        // Act
        bool result = await apiService.CheckHealthAsync();

        // Assert
        Assert.IsFalse(result);
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => true),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception?, string>>((v, t) => true)),
            Times.Once);
    }

    /// <summary>
    /// Tests that CheckHealthAsync returns false when a generic exception is thrown.
    /// </summary>
    [TestMethod]
    public async Task CheckHealthAsync_GenericException_ReturnsFalse()
    {
        // Arrange
        var mockHandler = new Mock<HttpMessageHandler>();
        mockHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new InvalidOperationException("Unexpected error"));

        var httpClient = new HttpClient(mockHandler.Object)
        {
            BaseAddress = new Uri("http://localhost/")
        };

        var mockTokenStorage = new Mock<TokenStorageService>(Mock.Of<ILogger<TokenStorageService>>());
        var mockLogger = new Mock<ILogger<ApiService>>();
        var apiService = new ApiService(httpClient, mockTokenStorage.Object, mockLogger.Object);

        // Act
        bool result = await apiService.CheckHealthAsync();

        // Assert
        Assert.IsFalse(result);
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => true),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception?, string>>((v, t) => true)),
            Times.Once);
    }

    /// <summary>
    /// Tests that CheckHealthAsync returns false when the cancellation token is already cancelled.
    /// </summary>
    [TestMethod]
    public async Task CheckHealthAsync_CancelledToken_ReturnsFalse()
    {
        // Arrange
        var mockHandler = new Mock<HttpMessageHandler>();
        mockHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new OperationCanceledException());

        var httpClient = new HttpClient(mockHandler.Object)
        {
            BaseAddress = new Uri("http://localhost/")
        };

        var mockTokenStorage = new Mock<TokenStorageService>(Mock.Of<ILogger<TokenStorageService>>());
        var mockLogger = new Mock<ILogger<ApiService>>();
        var apiService = new ApiService(httpClient, mockTokenStorage.Object, mockLogger.Object);

        var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act
        bool result = await apiService.CheckHealthAsync(cts.Token);

        // Assert
        Assert.IsFalse(result);
    }

    /// <summary>
    /// Tests that CheckHealthAsync logs a warning when an exception occurs during the health check.
    /// </summary>
    [TestMethod]
    public async Task CheckHealthAsync_ExceptionOccurs_LogsWarning()
    {
        // Arrange
        var expectedException = new HttpRequestException("Connection failed");
        var mockHandler = new Mock<HttpMessageHandler>();
        mockHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(expectedException);

        var httpClient = new HttpClient(mockHandler.Object)
        {
            BaseAddress = new Uri("http://localhost/")
        };

        var mockTokenStorage = new Mock<TokenStorageService>(Mock.Of<ILogger<TokenStorageService>>());
        var mockLogger = new Mock<ILogger<ApiService>>();
        var apiService = new ApiService(httpClient, mockTokenStorage.Object, mockLogger.Object);

        // Act
        bool result = await apiService.CheckHealthAsync();

        // Assert
        Assert.IsFalse(result);
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Health check failed")),
                It.Is<Exception>(ex => ex == expectedException),
                It.Is<Func<It.IsAnyType, Exception?, string>>((v, t) => true)),
            Times.Once);
    }

    /// <summary>
    /// Tests that CheckHealthAsync uses the correct timeout value of 5 seconds.
    /// </summary>
    [TestMethod]
    public async Task CheckHealthAsync_DefaultTimeout_Uses5Seconds()
    {
        // Arrange
        var tcs = new TaskCompletionSource<HttpResponseMessage>();
        var mockHandler = new Mock<HttpMessageHandler>();
        mockHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .Returns(async (HttpRequestMessage request, CancellationToken ct) =>
            {
                // Delay longer than 5 seconds to trigger timeout
                await Task.Delay(TimeSpan.FromSeconds(10), ct);
                return new HttpResponseMessage(HttpStatusCode.OK);
            });

        var httpClient = new HttpClient(mockHandler.Object)
        {
            BaseAddress = new Uri("http://localhost/")
        };

        var mockTokenStorage = new Mock<TokenStorageService>(Mock.Of<ILogger<TokenStorageService>>());
        var mockLogger = new Mock<ILogger<ApiService>>();
        var apiService = new ApiService(httpClient, mockTokenStorage.Object, mockLogger.Object);

        // Act
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        bool result = await apiService.CheckHealthAsync();
        stopwatch.Stop();

        // Assert
        Assert.IsFalse(result);
        // Verify the timeout happens around 5 seconds (with some tolerance for execution time)
        Assert.IsTrue(stopwatch.Elapsed.TotalSeconds >= 5.0 && stopwatch.Elapsed.TotalSeconds < 6.5,
            $"Expected timeout around 5 seconds, but was {stopwatch.Elapsed.TotalSeconds} seconds");
    }

    /// <summary>
    /// Tests that CheckHealthAsync properly handles multiple success status codes.
    /// </summary>
    /// <param name="statusCode">The HTTP success status code to test.</param>
    [TestMethod]
    [DataRow(HttpStatusCode.OK)]
    [DataRow(HttpStatusCode.Created)]
    [DataRow(HttpStatusCode.Accepted)]
    [DataRow(HttpStatusCode.NoContent)]
    public async Task CheckHealthAsync_VariousSuccessCodes_ReturnsTrue(HttpStatusCode statusCode)
    {
        // Arrange
        var mockHandler = new Mock<HttpMessageHandler>();
        mockHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = statusCode
            });

        var httpClient = new HttpClient(mockHandler.Object)
        {
            BaseAddress = new Uri("http://localhost/")
        };

        var mockTokenStorage = new Mock<TokenStorageService>(Mock.Of<ILogger<TokenStorageService>>());
        var mockLogger = new Mock<ILogger<ApiService>>();
        var apiService = new ApiService(httpClient, mockTokenStorage.Object, mockLogger.Object);

        // Act
        bool result = await apiService.CheckHealthAsync();

        // Assert
        Assert.IsTrue(result);
    }

    /// <summary>
    /// Tests that PutAsync sends a PUT request with valid path and content and returns the response successfully.
    /// Input: Valid path string, valid HttpContent, and default CancellationToken.
    /// Expected: HttpResponseMessage is returned with status OK, and HTTP method is PUT.
    /// </summary>
    [TestMethod]
    public async Task PutAsync_ValidPathAndContent_ReturnsSuccessResponse()
    {
        // Arrange
        var path = "api/resource/123";
        var content = new StringContent("{\"name\":\"test\"}", System.Text.Encoding.UTF8, "application/json");
        var expectedResponse = new HttpResponseMessage(HttpStatusCode.OK);

        var mockHandler = new Mock<HttpMessageHandler>();
        mockHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method == HttpMethod.Put &&
                    req.RequestUri!.ToString().Contains(path)),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(expectedResponse);

        var httpClient = new HttpClient(mockHandler.Object) { BaseAddress = new Uri("https://api.example.com/") };
        var mockTokenStorage = new Mock<TokenStorageService>();
        mockTokenStorage.Setup(x => x.GetAccessTokenAsync()).ReturnsAsync((string?)null);
        var mockLogger = new Mock<ILogger<ApiService>>();
        var apiService = new ApiService(httpClient, mockTokenStorage.Object, mockLogger.Object);

        // Act
        var result = await apiService.PutAsync(path, content);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(HttpStatusCode.OK, result.StatusCode);
        mockHandler.Protected().Verify(
            "SendAsync",
            Times.AtLeastOnce(),
            ItExpr.Is<HttpRequestMessage>(req => req.Method == HttpMethod.Put),
            ItExpr.IsAny<CancellationToken>());
    }

    /// <summary>
    /// Tests that PutAsync handles empty string path.
    /// Input: Empty string path, valid content, and default CancellationToken.
    /// Expected: Request is sent with empty path and response is returned.
    /// </summary>
    [TestMethod]
    public async Task PutAsync_EmptyPath_SendsRequestSuccessfully()
    {
        // Arrange
        var path = string.Empty;
        var content = new StringContent("{\"name\":\"test\"}");
        var expectedResponse = new HttpResponseMessage(HttpStatusCode.OK);

        var mockHandler = new Mock<HttpMessageHandler>();
        mockHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req => req.Method == HttpMethod.Put),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(expectedResponse);

        var httpClient = new HttpClient(mockHandler.Object) { BaseAddress = new Uri("https://api.example.com/") };
        var mockTokenStorage = new Mock<TokenStorageService>();
        mockTokenStorage.Setup(x => x.GetAccessTokenAsync()).ReturnsAsync((string?)null);
        var mockLogger = new Mock<ILogger<ApiService>>();
        var apiService = new ApiService(httpClient, mockTokenStorage.Object, mockLogger.Object);

        // Act
        var result = await apiService.PutAsync(path, content);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(HttpStatusCode.OK, result.StatusCode);
    }

    /// <summary>
    /// Tests that PutAsync handles whitespace-only path.
    /// Input: Whitespace-only string path, valid content, and default CancellationToken.
    /// Expected: Request is sent with whitespace path and response is returned.
    /// </summary>
    [TestMethod]
    public async Task PutAsync_WhitespacePath_SendsRequestSuccessfully()
    {
        // Arrange
        var path = "   ";
        var content = new StringContent("{\"name\":\"test\"}");
        var expectedResponse = new HttpResponseMessage(HttpStatusCode.OK);

        var mockHandler = new Mock<HttpMessageHandler>();
        mockHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req => req.Method == HttpMethod.Put),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(expectedResponse);

        var httpClient = new HttpClient(mockHandler.Object) { BaseAddress = new Uri("https://api.example.com/") };
        var mockTokenStorage = new Mock<TokenStorageService>();
        mockTokenStorage.Setup(x => x.GetAccessTokenAsync()).ReturnsAsync((string?)null);
        var mockLogger = new Mock<ILogger<ApiService>>();
        var apiService = new ApiService(httpClient, mockTokenStorage.Object, mockLogger.Object);

        // Act
        var result = await apiService.PutAsync(path, content);

        // Assert
        Assert.IsNotNull(result);
    }

    /// <summary>
    /// Tests that PutAsync handles path with special characters.
    /// Input: Path string with special characters, valid content, and default CancellationToken.
    /// Expected: Request is sent with special character path and response is returned.
    /// </summary>
    [TestMethod]
    public async Task PutAsync_PathWithSpecialCharacters_SendsRequestSuccessfully()
    {
        // Arrange
        var path = "api/resource/test@#$%^&*()";
        var content = new StringContent("{\"name\":\"test\"}");
        var expectedResponse = new HttpResponseMessage(HttpStatusCode.OK);

        var mockHandler = new Mock<HttpMessageHandler>();
        mockHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(expectedResponse);

        var httpClient = new HttpClient(mockHandler.Object) { BaseAddress = new Uri("https://api.example.com/") };
        var mockTokenStorage = new Mock<TokenStorageService>();
        mockTokenStorage.Setup(x => x.GetAccessTokenAsync()).ReturnsAsync((string?)null);
        var mockLogger = new Mock<ILogger<ApiService>>();
        var apiService = new ApiService(httpClient, mockTokenStorage.Object, mockLogger.Object);

        // Act
        var result = await apiService.PutAsync(path, content);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(HttpStatusCode.OK, result.StatusCode);
    }

    /// <summary>
    /// Tests that PutAsync handles very long path strings.
    /// Input: Very long path string, valid content, and default CancellationToken.
    /// Expected: Request is sent with long path and response is returned.
    /// </summary>
    [TestMethod]
    public async Task PutAsync_VeryLongPath_SendsRequestSuccessfully()
    {
        // Arrange
        var path = new string('a', 10000);
        var content = new StringContent("{\"name\":\"test\"}");
        var expectedResponse = new HttpResponseMessage(HttpStatusCode.OK);

        var mockHandler = new Mock<HttpMessageHandler>();
        mockHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(expectedResponse);

        var httpClient = new HttpClient(mockHandler.Object) { BaseAddress = new Uri("https://api.example.com/") };
        var mockTokenStorage = new Mock<TokenStorageService>();
        mockTokenStorage.Setup(x => x.GetAccessTokenAsync()).ReturnsAsync((string?)null);
        var mockLogger = new Mock<ILogger<ApiService>>();
        var apiService = new ApiService(httpClient, mockTokenStorage.Object, mockLogger.Object);

        // Act
        var result = await apiService.PutAsync(path, content);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(HttpStatusCode.OK, result.StatusCode);
    }

    /// <summary>
    /// Tests that PutAsync returns different HTTP status codes correctly.
    /// Input: Valid path, content, and default CancellationToken with various response status codes.
    /// Expected: The status code from the response is correctly returned.
    /// </summary>
    [TestMethod]
    [DataRow(HttpStatusCode.OK)]
    [DataRow(HttpStatusCode.Created)]
    [DataRow(HttpStatusCode.NoContent)]
    [DataRow(HttpStatusCode.BadRequest)]
    [DataRow(HttpStatusCode.NotFound)]
    [DataRow(HttpStatusCode.InternalServerError)]
    public async Task PutAsync_VariousStatusCodes_ReturnsCorrectStatusCode(HttpStatusCode statusCode)
    {
        // Arrange
        var path = "api/resource/123";
        var content = new StringContent("{\"name\":\"test\"}");
        var expectedResponse = new HttpResponseMessage(statusCode);

        var mockHandler = new Mock<HttpMessageHandler>();
        mockHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(expectedResponse);

        var httpClient = new HttpClient(mockHandler.Object) { BaseAddress = new Uri("https://api.example.com/") };
        var mockTokenStorage = new Mock<TokenStorageService>();
        mockTokenStorage.Setup(x => x.GetAccessTokenAsync()).ReturnsAsync((string?)null);
        var mockLogger = new Mock<ILogger<ApiService>>();
        var apiService = new ApiService(httpClient, mockTokenStorage.Object, mockLogger.Object);

        // Act
        var result = await apiService.PutAsync(path, content);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(statusCode, result.StatusCode);
    }

    /// <summary>
    /// Tests that PutAsync sends the correct HTTP method (PUT).
    /// Input: Valid path, content, and default CancellationToken.
    /// Expected: The HTTP request uses PUT method.
    /// </summary>
    [TestMethod]
    public async Task PutAsync_ValidRequest_UsesPutHttpMethod()
    {
        // Arrange
        var path = "api/resource/123";
        var content = new StringContent("{\"name\":\"test\"}");
        var expectedResponse = new HttpResponseMessage(HttpStatusCode.OK);
        HttpMethod? capturedMethod = null;

        var mockHandler = new Mock<HttpMessageHandler>();
        mockHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .Callback<HttpRequestMessage, CancellationToken>((req, ct) => capturedMethod = req.Method)
            .ReturnsAsync(expectedResponse);

        var httpClient = new HttpClient(mockHandler.Object) { BaseAddress = new Uri("https://api.example.com/") };
        var mockTokenStorage = new Mock<TokenStorageService>();
        mockTokenStorage.Setup(x => x.GetAccessTokenAsync()).ReturnsAsync((string?)null);
        var mockLogger = new Mock<ILogger<ApiService>>();
        var apiService = new ApiService(httpClient, mockTokenStorage.Object, mockLogger.Object);

        // Act
        await apiService.PutAsync(path, content);

        // Assert
        Assert.IsNotNull(capturedMethod);
        Assert.AreEqual(HttpMethod.Put, capturedMethod);
    }

    /// <summary>
    /// Tests that PutAsync properly includes the content in the request.
    /// Input: Valid path, HttpContent with JSON data, and default CancellationToken.
    /// Expected: The request contains the provided content.
    /// </summary>
    [TestMethod]
    public async Task PutAsync_WithContent_IncludesContentInRequest()
    {
        // Arrange
        var path = "api/resource/123";
        var expectedBody = "{\"name\":\"test\",\"value\":42}";
        var content = new StringContent(expectedBody, System.Text.Encoding.UTF8, "application/json");
        var expectedResponse = new HttpResponseMessage(HttpStatusCode.OK);
        HttpContent? capturedContent = null;

        var mockHandler = new Mock<HttpMessageHandler>();
        mockHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .Callback<HttpRequestMessage, CancellationToken>((req, ct) => capturedContent = req.Content)
            .ReturnsAsync(expectedResponse);

        var httpClient = new HttpClient(mockHandler.Object) { BaseAddress = new Uri("https://api.example.com/") };
        var mockTokenStorage = new Mock<TokenStorageService>();
        mockTokenStorage.Setup(x => x.GetAccessTokenAsync()).ReturnsAsync((string?)null);
        var mockLogger = new Mock<ILogger<ApiService>>();
        var apiService = new ApiService(httpClient, mockTokenStorage.Object, mockLogger.Object);

        // Act
        await apiService.PutAsync(path, content);

        // Assert
        Assert.IsNotNull(capturedContent);
        var actualBody = await capturedContent.ReadAsStringAsync();
        Assert.AreEqual(expectedBody, actualBody);
    }

    /// <summary>
    /// Tests that PutAsync properly passes the path to the request.
    /// Input: Specific path string, valid content, and default CancellationToken.
    /// Expected: The request URI contains the provided path.
    /// </summary>
    [TestMethod]
    public async Task PutAsync_WithPath_IncludesPathInRequest()
    {
        // Arrange
        var path = "api/users/456/profile";
        var content = new StringContent("{\"name\":\"test\"}");
        var expectedResponse = new HttpResponseMessage(HttpStatusCode.OK);
        Uri? capturedUri = null;

        var mockHandler = new Mock<HttpMessageHandler>();
        mockHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .Callback<HttpRequestMessage, CancellationToken>((req, ct) => capturedUri = req.RequestUri)
            .ReturnsAsync(expectedResponse);

        var httpClient = new HttpClient(mockHandler.Object) { BaseAddress = new Uri("https://api.example.com/") };
        var mockTokenStorage = new Mock<TokenStorageService>();
        mockTokenStorage.Setup(x => x.GetAccessTokenAsync()).ReturnsAsync((string?)null);
        var mockLogger = new Mock<ILogger<ApiService>>();
        var apiService = new ApiService(httpClient, mockTokenStorage.Object, mockLogger.Object);

        // Act
        await apiService.PutAsync(path, content);

        // Assert
        Assert.IsNotNull(capturedUri);
        Assert.IsTrue(capturedUri.ToString().Contains(path));
    }

    /// <summary>
    /// Tests that PostAsync with valid path and content sends a POST request and returns the response.
    /// </summary>
    [TestMethod]
    public async Task PostAsync_ValidPathAndContent_SendsPostRequestAndReturnsResponse()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<ApiService>>();
        var mockTokenStorage = new Mock<TokenStorageService>();
        mockTokenStorage.Setup(x => x.GetAccessTokenAsync()).ReturnsAsync("test-token");

        var expectedResponse = new HttpResponseMessage(HttpStatusCode.OK);
        var handler = new TestHttpMessageHandler(req =>
        {
            Assert.AreEqual(HttpMethod.Post, req.Method);
            Assert.AreEqual("api/test", req.RequestUri?.ToString());
            return Task.FromResult(expectedResponse);
        });
        var httpClient = new HttpClient(handler) { BaseAddress = new Uri("https://test.com/") };

        var apiService = new ApiService(httpClient, mockTokenStorage.Object, mockLogger.Object);
        var content = new StringContent("test content");

        // Act
        var result = await apiService.PostAsync("api/test", content, CancellationToken.None);

        // Assert
        Assert.AreEqual(expectedResponse, result);
        Assert.AreEqual(HttpStatusCode.OK, result.StatusCode);
    }

    /// <summary>
    /// Tests that PostAsync with null content sends a POST request without content.
    /// </summary>
    [TestMethod]
    public async Task PostAsync_NullContent_SendsPostRequestWithoutContent()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<ApiService>>();
        var mockTokenStorage = new Mock<TokenStorageService>();
        mockTokenStorage.Setup(x => x.GetAccessTokenAsync()).ReturnsAsync("test-token");

        var expectedResponse = new HttpResponseMessage(HttpStatusCode.OK);
        var handler = new TestHttpMessageHandler(req =>
        {
            Assert.AreEqual(HttpMethod.Post, req.Method);
            Assert.IsNull(req.Content);
            return Task.FromResult(expectedResponse);
        });
        var httpClient = new HttpClient(handler) { BaseAddress = new Uri("https://test.com/") };

        var apiService = new ApiService(httpClient, mockTokenStorage.Object, mockLogger.Object);

        // Act
        var result = await apiService.PostAsync("api/test", null!);

        // Assert
        Assert.AreEqual(expectedResponse, result);
    }

    /// <summary>
    /// Tests that PostAsync with an empty path sends a POST request with empty path.
    /// </summary>
    [TestMethod]
    public async Task PostAsync_EmptyPath_SendsPostRequest()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<ApiService>>();
        var mockTokenStorage = new Mock<TokenStorageService>();
        mockTokenStorage.Setup(x => x.GetAccessTokenAsync()).ReturnsAsync("test-token");

        var expectedResponse = new HttpResponseMessage(HttpStatusCode.OK);
        var handler = new TestHttpMessageHandler(req =>
        {
            Assert.AreEqual(HttpMethod.Post, req.Method);
            return Task.FromResult(expectedResponse);
        });
        var httpClient = new HttpClient(handler) { BaseAddress = new Uri("https://test.com/") };

        var apiService = new ApiService(httpClient, mockTokenStorage.Object, mockLogger.Object);
        var content = new StringContent("test");

        // Act
        var result = await apiService.PostAsync("", content);

        // Assert
        Assert.AreEqual(expectedResponse, result);
    }

    /// <summary>
    /// Tests that PostAsync with whitespace-only path sends a POST request with whitespace path.
    /// </summary>
    [TestMethod]
    public async Task PostAsync_WhitespacePath_SendsPostRequest()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<ApiService>>();
        var mockTokenStorage = new Mock<TokenStorageService>();
        mockTokenStorage.Setup(x => x.GetAccessTokenAsync()).ReturnsAsync("test-token");

        var expectedResponse = new HttpResponseMessage(HttpStatusCode.OK);
        var handler = new TestHttpMessageHandler(req =>
        {
            Assert.AreEqual(HttpMethod.Post, req.Method);
            return Task.FromResult(expectedResponse);
        });
        var httpClient = new HttpClient(handler) { BaseAddress = new Uri("https://test.com/") };

        var apiService = new ApiService(httpClient, mockTokenStorage.Object, mockLogger.Object);
        var content = new StringContent("test");

        // Act
        var result = await apiService.PostAsync("   ", content);

        // Assert
        Assert.AreEqual(expectedResponse, result);
    }

    /// <summary>
    /// Tests that PostAsync with special characters in path sends a POST request successfully.
    /// </summary>
    [TestMethod]
    [DataRow("api/test?param=value")]
    [DataRow("api/test/with spaces")]
    [DataRow("api/test#fragment")]
    [DataRow("api/test&special=chars")]
    public async Task PostAsync_PathWithSpecialCharacters_SendsPostRequest(string path)
    {
        // Arrange
        var mockLogger = new Mock<ILogger<ApiService>>();
        var mockTokenStorage = new Mock<TokenStorageService>();
        mockTokenStorage.Setup(x => x.GetAccessTokenAsync()).ReturnsAsync("test-token");

        var expectedResponse = new HttpResponseMessage(HttpStatusCode.OK);
        var handler = new TestHttpMessageHandler(req =>
        {
            Assert.AreEqual(HttpMethod.Post, req.Method);
            return Task.FromResult(expectedResponse);
        });
        var httpClient = new HttpClient(handler) { BaseAddress = new Uri("https://test.com/") };

        var apiService = new ApiService(httpClient, mockTokenStorage.Object, mockLogger.Object);
        var content = new StringContent("test");

        // Act
        var result = await apiService.PostAsync(path, content);

        // Assert
        Assert.AreEqual(expectedResponse, result);
    }

    /// <summary>
    /// Tests that PostAsync with default cancellation token works correctly.
    /// </summary>
    [TestMethod]
    public async Task PostAsync_DefaultCancellationToken_SendsPostRequest()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<ApiService>>();
        var mockTokenStorage = new Mock<TokenStorageService>();
        mockTokenStorage.Setup(x => x.GetAccessTokenAsync()).ReturnsAsync("test-token");

        var expectedResponse = new HttpResponseMessage(HttpStatusCode.OK);
        var handler = new TestHttpMessageHandler(req =>
        {
            Assert.AreEqual(HttpMethod.Post, req.Method);
            return Task.FromResult(expectedResponse);
        });
        var httpClient = new HttpClient(handler) { BaseAddress = new Uri("https://test.com/") };

        var apiService = new ApiService(httpClient, mockTokenStorage.Object, mockLogger.Object);
        var content = new StringContent("test");

        // Act
        var result = await apiService.PostAsync("api/test", content);

        // Assert
        Assert.AreEqual(expectedResponse, result);
    }

    /// <summary>
    /// Tests that PostAsync with very long path sends a POST request successfully.
    /// </summary>
    [TestMethod]
    public async Task PostAsync_VeryLongPath_SendsPostRequest()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<ApiService>>();
        var mockTokenStorage = new Mock<TokenStorageService>();
        mockTokenStorage.Setup(x => x.GetAccessTokenAsync()).ReturnsAsync("test-token");

        var expectedResponse = new HttpResponseMessage(HttpStatusCode.OK);
        var handler = new TestHttpMessageHandler(req =>
        {
            Assert.AreEqual(HttpMethod.Post, req.Method);
            return Task.FromResult(expectedResponse);
        });
        var httpClient = new HttpClient(handler) { BaseAddress = new Uri("https://test.com/") };

        var apiService = new ApiService(httpClient, mockTokenStorage.Object, mockLogger.Object);
        var content = new StringContent("test");
        var longPath = "api/" + new string('a', 2000);

        // Act
        var result = await apiService.PostAsync(longPath, content);

        // Assert
        Assert.AreEqual(expectedResponse, result);
    }

    /// <summary>
    /// Tests that PostAsync returns different HTTP status codes correctly.
    /// </summary>
    [TestMethod]
    [DataRow(HttpStatusCode.OK)]
    [DataRow(HttpStatusCode.Created)]
    [DataRow(HttpStatusCode.BadRequest)]
    [DataRow(HttpStatusCode.NotFound)]
    [DataRow(HttpStatusCode.InternalServerError)]
    public async Task PostAsync_VariousStatusCodes_ReturnsCorrectResponse(HttpStatusCode statusCode)
    {
        // Arrange
        var mockLogger = new Mock<ILogger<ApiService>>();
        var mockTokenStorage = new Mock<TokenStorageService>();
        mockTokenStorage.Setup(x => x.GetAccessTokenAsync()).ReturnsAsync("test-token");

        var expectedResponse = new HttpResponseMessage(statusCode);
        var handler = new TestHttpMessageHandler(req => Task.FromResult(expectedResponse));
        var httpClient = new HttpClient(handler) { BaseAddress = new Uri("https://test.com/") };

        var apiService = new ApiService(httpClient, mockTokenStorage.Object, mockLogger.Object);
        var content = new StringContent("test");

        // Act
        var result = await apiService.PostAsync("api/test", content);

        // Assert
        Assert.AreEqual(statusCode, result.StatusCode);
    }

    /// <summary>
    /// Tests that PostAsync without authentication token sends a POST request without Authorization header.
    /// </summary>
    [TestMethod]
    public async Task PostAsync_NoAuthToken_SendsPostRequestWithoutAuthorizationHeader()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<ApiService>>();
        var mockTokenStorage = new Mock<TokenStorageService>();
        mockTokenStorage.Setup(x => x.GetAccessTokenAsync()).ReturnsAsync((string?)null);

        var expectedResponse = new HttpResponseMessage(HttpStatusCode.OK);
        var handler = new TestHttpMessageHandler(req =>
        {
            Assert.AreEqual(HttpMethod.Post, req.Method);
            Assert.IsNull(req.Headers.Authorization);
            return Task.FromResult(expectedResponse);
        });
        var httpClient = new HttpClient(handler) { BaseAddress = new Uri("https://test.com/") };

        var apiService = new ApiService(httpClient, mockTokenStorage.Object, mockLogger.Object);
        var content = new StringContent("test");

        // Act
        var result = await apiService.PostAsync("api/test", content);

        // Assert
        Assert.AreEqual(expectedResponse, result);
    }

    /// <summary>
    /// Helper class to mock HttpMessageHandler for testing HttpClient behavior.
    /// </summary>
    private class TestHttpMessageHandler : HttpMessageHandler
    {
        private readonly Func<HttpRequestMessage, Task<HttpResponseMessage>> _handler;

        public TestHttpMessageHandler(Func<HttpRequestMessage, Task<HttpResponseMessage>> handler)
        {
            _handler = handler;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return _handler(request);
        }
    }

    /// <summary>
    /// Tests that CheckHealthAsync uses the default cancellation token when none is provided.
    /// Input: No cancellation token provided (uses default).
    /// Expected: Request completes successfully without cancellation.
    /// </summary>
    [TestMethod]
    public async Task CheckHealthAsync_DefaultCancellationToken_SendsRequestSuccessfully()
    {
        // Arrange
        var mockHandler = new Mock<HttpMessageHandler>();
        mockHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req => req.Method == HttpMethod.Get && req.RequestUri!.ToString().EndsWith("health")),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK
            });

        var httpClient = new HttpClient(mockHandler.Object)
        {
            BaseAddress = new Uri("http://localhost/")
        };

        var mockTokenStorage = new Mock<TokenStorageService>(Mock.Of<ILogger<TokenStorageService>>());
        var mockLogger = new Mock<ILogger<ApiService>>();
        var apiService = new ApiService(httpClient, mockTokenStorage.Object, mockLogger.Object);

        // Act
        bool result = await apiService.CheckHealthAsync();

        // Assert
        Assert.IsTrue(result);
        mockHandler.Protected().Verify(
            "SendAsync",
            Times.Once(),
            ItExpr.IsAny<HttpRequestMessage>(),
            ItExpr.IsAny<CancellationToken>());
    }

    /// <summary>
    /// Tests that CheckHealthAsync sends a GET request to the "health" endpoint.
    /// Input: Valid health check request.
    /// Expected: GET request is sent to "health" endpoint.
    /// </summary>
    [TestMethod]
    public async Task CheckHealthAsync_ValidRequest_SendsGetRequestToHealthEndpoint()
    {
        // Arrange
        HttpRequestMessage? capturedRequest = null;
        var mockHandler = new Mock<HttpMessageHandler>();
        mockHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .Callback<HttpRequestMessage, CancellationToken>((req, ct) => capturedRequest = req)
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK
            });

        var httpClient = new HttpClient(mockHandler.Object)
        {
            BaseAddress = new Uri("http://localhost/")
        };

        var mockTokenStorage = new Mock<TokenStorageService>(Mock.Of<ILogger<TokenStorageService>>());
        var mockLogger = new Mock<ILogger<ApiService>>();
        var apiService = new ApiService(httpClient, mockTokenStorage.Object, mockLogger.Object);

        // Act
        await apiService.CheckHealthAsync();

        // Assert
        Assert.IsNotNull(capturedRequest);
        Assert.AreEqual(HttpMethod.Get, capturedRequest.Method);
        Assert.IsTrue(capturedRequest.RequestUri!.ToString().EndsWith("health"));
    }

    /// <summary>
    /// Tests that CheckHealthAsync does not throw exceptions and always returns a boolean result.
    /// Input: Various exception scenarios.
    /// Expected: Method never throws, always returns false on exception.
    /// </summary>
    [TestMethod]
    public async Task CheckHealthAsync_AnyException_NeverThrows()
    {
        // Arrange
        var mockHandler = new Mock<HttpMessageHandler>();
        mockHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new Exception("Critical error"));

        var httpClient = new HttpClient(mockHandler.Object)
        {
            BaseAddress = new Uri("http://localhost/")
        };

        var mockTokenStorage = new Mock<TokenStorageService>(Mock.Of<ILogger<TokenStorageService>>());
        var mockLogger = new Mock<ILogger<ApiService>>();
        var apiService = new ApiService(httpClient, mockTokenStorage.Object, mockLogger.Object);

        // Act
        bool result = await apiService.CheckHealthAsync();

        // Assert
        Assert.IsFalse(result);
    }

    /// <summary>
    /// Tests that CheckHealthAsync properly disposes the timeout cancellation token source.
    /// Input: Valid health check request.
    /// Expected: Request completes without resource leaks.
    /// </summary>
    [TestMethod]
    public async Task CheckHealthAsync_ValidRequest_DisposesTimeoutCancellationTokenSource()
    {
        // Arrange
        var mockHandler = new Mock<HttpMessageHandler>();
        mockHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK
            });

        var httpClient = new HttpClient(mockHandler.Object)
        {
            BaseAddress = new Uri("http://localhost/")
        };

        var mockTokenStorage = new Mock<TokenStorageService>(Mock.Of<ILogger<TokenStorageService>>());
        var mockLogger = new Mock<ILogger<ApiService>>();
        var apiService = new ApiService(httpClient, mockTokenStorage.Object, mockLogger.Object);

        // Act
        bool result = await apiService.CheckHealthAsync();

        // Assert
        Assert.IsTrue(result);
    }

    /// <summary>
    /// Tests that CheckHealthAsync returns false for 3xx redirection status codes.
    /// Input: Health endpoint returns redirection status codes.
    /// Expected: Returns false (redirections are not considered healthy).
    /// </summary>
    [TestMethod]
    [DataRow(HttpStatusCode.MovedPermanently)]
    [DataRow(HttpStatusCode.Found)]
    [DataRow(HttpStatusCode.SeeOther)]
    [DataRow(HttpStatusCode.NotModified)]
    [DataRow(HttpStatusCode.TemporaryRedirect)]
    public async Task CheckHealthAsync_RedirectionStatusCode_ReturnsFalse(HttpStatusCode statusCode)
    {
        // Arrange
        var mockHandler = new Mock<HttpMessageHandler>();
        mockHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req => req.Method == HttpMethod.Get && req.RequestUri!.ToString().EndsWith("health")),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = statusCode
            });

        var httpClient = new HttpClient(mockHandler.Object)
        {
            BaseAddress = new Uri("http://localhost/")
        };

        var mockTokenStorage = new Mock<TokenStorageService>(Mock.Of<ILogger<TokenStorageService>>());
        var mockLogger = new Mock<ILogger<ApiService>>();
        var apiService = new ApiService(httpClient, mockTokenStorage.Object, mockLogger.Object);

        // Act
        bool result = await apiService.CheckHealthAsync();

        // Assert
        Assert.IsFalse(result);
    }

    /// <summary>
    /// Tests that the constructor successfully initializes an ApiService instance with valid parameters.
    /// Input: Valid HttpClient, TokenStorageService, and ILogger instances.
    /// Expected: Constructor completes successfully without throwing any exceptions.
    /// </summary>
    [TestMethod]
    public void Constructor_WithValidParameters_CreatesInstanceSuccessfully()
    {
        // Arrange
        var httpClient = new HttpClient
        {
            BaseAddress = new Uri("http://localhost/")
        };
        var mockTokenStorage = new Mock<TokenStorageService>(Mock.Of<ILogger<TokenStorageService>>());
        var mockLogger = new Mock<ILogger<ApiService>>();

        // Act
        var apiService = new ApiService(httpClient, mockTokenStorage.Object, mockLogger.Object);

        // Assert
        Assert.IsNotNull(apiService);
    }

    /// <summary>
    /// Tests that the constructor accepts a null HttpClient parameter.
    /// Input: Null HttpClient, valid TokenStorageService and ILogger.
    /// Expected: Constructor completes without throwing during construction,
    /// even though this violates the non-nullable contract.
    /// </summary>
    [TestMethod]
    public void Constructor_WithNullHttpClient_DoesNotThrowDuringConstruction()
    {
        // Arrange
        HttpClient? httpClient = null;
        var mockTokenStorage = new Mock<TokenStorageService>(Mock.Of<ILogger<TokenStorageService>>());
        var mockLogger = new Mock<ILogger<ApiService>>();

        // Act
        var apiService = new ApiService(httpClient!, mockTokenStorage.Object, mockLogger.Object);

        // Assert
        Assert.IsNotNull(apiService);
    }

    /// <summary>
    /// Tests that the constructor accepts a null ILogger parameter.
    /// Input: Valid HttpClient and TokenStorageService, null ILogger.
    /// Expected: Constructor completes without throwing during construction,
    /// even though this violates the non-nullable contract.
    /// </summary>
    [TestMethod]
    public void Constructor_WithNullLogger_DoesNotThrowDuringConstruction()
    {
        // Arrange
        var httpClient = new HttpClient
        {
            BaseAddress = new Uri("http://localhost/")
        };
        var mockTokenStorage = new Mock<TokenStorageService>(Mock.Of<ILogger<TokenStorageService>>());
        ILogger<ApiService>? logger = null;

        // Act
        var apiService = new ApiService(httpClient, mockTokenStorage.Object, logger!);

        // Assert
        Assert.IsNotNull(apiService);
    }

    /// <summary>
    /// Tests that the constructor accepts null HttpClient and null ILogger parameters.
    /// Input: Null HttpClient, valid TokenStorageService, null ILogger.
    /// Expected: Constructor completes without throwing during construction.
    /// </summary>
    [TestMethod]
    public void Constructor_WithNullHttpClientAndNullLogger_DoesNotThrowDuringConstruction()
    {
        // Arrange
        HttpClient? httpClient = null;
        var mockTokenStorage = new Mock<TokenStorageService>(Mock.Of<ILogger<TokenStorageService>>());
        ILogger<ApiService>? logger = null;

        // Act
        var apiService = new ApiService(httpClient!, mockTokenStorage.Object, logger!);

        // Assert
        Assert.IsNotNull(apiService);
    }

    /// <summary>
    /// Tests that the constructor accepts null HttpClient and null TokenStorageService parameters.
    /// Input: Null HttpClient, null TokenStorageService, valid ILogger.
    /// Expected: Constructor completes without throwing during construction.
    /// </summary>
    [TestMethod]
    public void Constructor_WithNullHttpClientAndNullTokenStorage_DoesNotThrowDuringConstruction()
    {
        // Arrange
        HttpClient? httpClient = null;
        TokenStorageService? tokenStorage = null;
        var mockLogger = new Mock<ILogger<ApiService>>();

        // Act
        var apiService = new ApiService(httpClient!, tokenStorage!, mockLogger.Object);

        // Assert
        Assert.IsNotNull(apiService);
    }

    /// <summary>
    /// Tests that the constructor accepts null TokenStorageService and null ILogger parameters.
    /// Input: Valid HttpClient, null TokenStorageService, null ILogger.
    /// Expected: Constructor completes without throwing during construction.
    /// </summary>
    [TestMethod]
    public void Constructor_WithNullTokenStorageAndNullLogger_DoesNotThrowDuringConstruction()
    {
        // Arrange
        var httpClient = new HttpClient
        {
            BaseAddress = new Uri("http://localhost/")
        };
        TokenStorageService? tokenStorage = null;
        ILogger<ApiService>? logger = null;

        // Act
        var apiService = new ApiService(httpClient, tokenStorage!, logger!);

        // Assert
        Assert.IsNotNull(apiService);
    }

    /// <summary>
    /// Tests that PutAsync respects a custom CancellationToken and passes it to the underlying request.
    /// Input: Valid path, content, and a custom CancellationToken.
    /// Expected: Request is sent successfully with the custom token.
    /// </summary>
    [TestMethod]
    public async Task PutAsync_CustomCancellationToken_PassesTokenToRequest()
    {
        // Arrange
        var path = "api/resource";
        var content = new StringContent("{\"name\":\"test\"}");
        var expectedResponse = new HttpResponseMessage(HttpStatusCode.OK);
        var cts = new CancellationTokenSource();
        var customToken = cts.Token;

        var mockHandler = new Mock<HttpMessageHandler>();
        mockHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(expectedResponse);

        var httpClient = new HttpClient(mockHandler.Object) { BaseAddress = new Uri("https://api.example.com/") };
        var mockTokenStorage = new Mock<TokenStorageService>();
        mockTokenStorage.Setup(x => x.GetAccessTokenAsync()).ReturnsAsync((string?)null);
        var mockLogger = new Mock<ILogger<ApiService>>();
        var apiService = new ApiService(httpClient, mockTokenStorage.Object, mockLogger.Object);

        // Act
        var result = await apiService.PutAsync(path, content, customToken);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(HttpStatusCode.OK, result.StatusCode);
    }

    /// <summary>
    /// Tests that PutAsync handles different content types correctly.
    /// Input: Valid path with FormUrlEncodedContent and default CancellationToken.
    /// Expected: Request is sent successfully with form content.
    /// </summary>
    [TestMethod]
    public async Task PutAsync_FormUrlEncodedContent_SendsRequestSuccessfully()
    {
        // Arrange
        var path = "api/resource";
        var formData = new FormUrlEncodedContent(new[]
        {
            new System.Collections.Generic.KeyValuePair<string, string>("key", "value")
        });
        var expectedResponse = new HttpResponseMessage(HttpStatusCode.OK);

        var mockHandler = new Mock<HttpMessageHandler>();
        mockHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(expectedResponse);

        var httpClient = new HttpClient(mockHandler.Object) { BaseAddress = new Uri("https://api.example.com/") };
        var mockTokenStorage = new Mock<TokenStorageService>();
        mockTokenStorage.Setup(x => x.GetAccessTokenAsync()).ReturnsAsync((string?)null);
        var mockLogger = new Mock<ILogger<ApiService>>();
        var apiService = new ApiService(httpClient, mockTokenStorage.Object, mockLogger.Object);

        // Act
        var result = await apiService.PutAsync(path, formData);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(HttpStatusCode.OK, result.StatusCode);
    }

    /// <summary>
    /// Tests that PostAsync uses POST HTTP method specifically.
    /// Input: Valid path, valid content, default cancellation token
    /// Expected: The HTTP request uses POST method
    /// </summary>
    [TestMethod]
    public async Task PostAsync_ValidRequest_UsesPostHttpMethod()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<ApiService>>();
        var mockTokenStorage = new Mock<TokenStorageService>();
        mockTokenStorage.Setup(x => x.GetAccessTokenAsync()).ReturnsAsync("test-token");

        HttpMethod? capturedMethod = null;
        var expectedResponse = new HttpResponseMessage(HttpStatusCode.OK);
        var handler = new TestHttpMessageHandler(req =>
        {
            capturedMethod = req.Method;
            return Task.FromResult(expectedResponse);
        });
        var httpClient = new HttpClient(handler) { BaseAddress = new Uri("https://test.com/") };

        var apiService = new ApiService(httpClient, mockTokenStorage.Object, mockLogger.Object);
        var content = new StringContent("test");

        // Act
        await apiService.PostAsync("api/test", content);

        // Assert
        Assert.AreEqual(HttpMethod.Post, capturedMethod);
    }

    /// <summary>
    /// Tests that PostAsync properly includes the content in the request.
    /// Input: Valid path, StringContent with specific text, default cancellation token
    /// Expected: The request contains the provided content
    /// </summary>
    [TestMethod]
    public async Task PostAsync_WithContent_IncludesContentInRequest()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<ApiService>>();
        var mockTokenStorage = new Mock<TokenStorageService>();
        mockTokenStorage.Setup(x => x.GetAccessTokenAsync()).ReturnsAsync("test-token");

        HttpContent? capturedContent = null;
        var expectedResponse = new HttpResponseMessage(HttpStatusCode.OK);
        var handler = new TestHttpMessageHandler(req =>
        {
            capturedContent = req.Content;
            return Task.FromResult(expectedResponse);
        });
        var httpClient = new HttpClient(handler) { BaseAddress = new Uri("https://test.com/") };

        var apiService = new ApiService(httpClient, mockTokenStorage.Object, mockLogger.Object);
        var content = new StringContent("test content");

        // Act
        await apiService.PostAsync("api/test", content);

        // Assert
        Assert.IsNotNull(capturedContent);
    }

    /// <summary>
    /// Tests that PostAsync properly passes the path to the request.
    /// Input: Specific path string "api/resource/123", valid content, default cancellation token
    /// Expected: The request URI contains the provided path
    /// </summary>
    [TestMethod]
    public async Task PostAsync_WithPath_IncludesPathInRequest()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<ApiService>>();
        var mockTokenStorage = new Mock<TokenStorageService>();
        mockTokenStorage.Setup(x => x.GetAccessTokenAsync()).ReturnsAsync("test-token");

        string? capturedPath = null;
        var expectedResponse = new HttpResponseMessage(HttpStatusCode.OK);
        var handler = new TestHttpMessageHandler(req =>
        {
            capturedPath = req.RequestUri?.ToString();
            return Task.FromResult(expectedResponse);
        });
        var httpClient = new HttpClient(handler) { BaseAddress = new Uri("https://test.com/") };

        var apiService = new ApiService(httpClient, mockTokenStorage.Object, mockLogger.Object);
        var content = new StringContent("test");

        // Act
        await apiService.PostAsync("api/resource/123", content);

        // Assert
        Assert.AreEqual("api/resource/123", capturedPath);
    }

    /// <summary>
    /// Tests that DeleteAsync handles null path parameter.
    /// Input: Null path string
    /// Expected: Request is processed (runtime allows null despite non-nullable annotation).
    /// </summary>
    [TestMethod]
    public async Task DeleteAsync_NullPath_ProcessesRequest()
    {
        // Arrange
        var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
        mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage { StatusCode = HttpStatusCode.OK });

        var httpClient = new HttpClient(mockHttpMessageHandler.Object)
        {
            BaseAddress = new Uri("https://api.example.com")
        };

        var mockTokenStorage = new Mock<TokenStorageService>();
        mockTokenStorage.Setup(x => x.GetAccessTokenAsync()).ReturnsAsync("token");

        var mockLogger = new Mock<ILogger<ApiService>>();
        var apiService = new ApiService(httpClient, mockTokenStorage.Object, mockLogger.Object);

        // Act
        var response = await apiService.DeleteAsync(null!);

        // Assert
        Assert.IsNotNull(response);
    }

    /// <summary>
    /// Tests that DeleteAsync returns various success HTTP status codes correctly.
    /// Input: Valid path, various success status codes
    /// Expected: Response with the success status code is returned.
    /// </summary>
    [TestMethod]
    [DataRow(HttpStatusCode.OK)]
    [DataRow(HttpStatusCode.Accepted)]
    [DataRow(HttpStatusCode.NoContent)]
    [DataRow(HttpStatusCode.Created)]
    public async Task DeleteAsync_VariousSuccessStatusCodes_ReturnsCorrectStatusCode(HttpStatusCode statusCode)
    {
        // Arrange
        var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
        mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage { StatusCode = statusCode });

        var httpClient = new HttpClient(mockHttpMessageHandler.Object)
        {
            BaseAddress = new Uri("https://api.example.com")
        };

        var mockTokenStorage = new Mock<TokenStorageService>();
        mockTokenStorage.Setup(x => x.GetAccessTokenAsync()).ReturnsAsync("valid-token");

        var mockLogger = new Mock<ILogger<ApiService>>();
        var apiService = new ApiService(httpClient, mockTokenStorage.Object, mockLogger.Object);

        // Act
        var response = await apiService.DeleteAsync("/api/resource");

        // Assert
        Assert.IsNotNull(response);
        Assert.AreEqual(statusCode, response.StatusCode);
    }

    /// <summary>
    /// Tests that GetAsync handles paths with Unicode characters.
    /// Input: Path with Unicode/emoji characters
    /// Expected: Request is sent successfully
    /// </summary>
    [TestMethod]
    public async Task GetAsync_PathWithUnicodeCharacters_SendsRequestSuccessfully()
    {
        // Arrange
        var path = "api/测试/データ/🔥";
        var expectedResponse = new HttpResponseMessage(HttpStatusCode.OK);
        var mockHandler = new Mock<HttpMessageHandler>();
        mockHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(expectedResponse);
        var httpClient = new HttpClient(mockHandler.Object) { BaseAddress = new Uri("https://api.example.com/") };
        var mockTokenStorage = new Mock<TokenStorageService>();
        mockTokenStorage.Setup(ts => ts.GetAccessTokenAsync()).ReturnsAsync(string.Empty);
        var mockLogger = new Mock<ILogger<ApiService>>();
        var apiService = new ApiService(httpClient, mockTokenStorage.Object, mockLogger.Object);

        // Act
        var result = await apiService.GetAsync(path);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(HttpStatusCode.OK, result.StatusCode);
    }

    /// <summary>
    /// Tests that GetAsync properly passes an explicit cancellation token.
    /// Input: Valid path with an explicit (non-cancelled) cancellation token
    /// Expected: Request is sent successfully with the provided token
    /// </summary>
    [TestMethod]
    public async Task GetAsync_ExplicitCancellationToken_UseProvidedToken()
    {
        // Arrange
        var path = "api/test";
        var expectedResponse = new HttpResponseMessage(HttpStatusCode.OK);
        var mockHandler = new Mock<HttpMessageHandler>();
        mockHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(expectedResponse);
        var httpClient = new HttpClient(mockHandler.Object) { BaseAddress = new Uri("https://api.example.com/") };
        var mockTokenStorage = new Mock<TokenStorageService>();
        mockTokenStorage.Setup(ts => ts.GetAccessTokenAsync()).ReturnsAsync(string.Empty);
        var mockLogger = new Mock<ILogger<ApiService>>();
        var apiService = new ApiService(httpClient, mockTokenStorage.Object, mockLogger.Object);
        var cts = new CancellationTokenSource();

        // Act
        var result = await apiService.GetAsync(path, cts.Token);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(HttpStatusCode.OK, result.StatusCode);
        mockHandler.Protected().Verify(
            "SendAsync",
            Times.Once(),
            ItExpr.IsAny<HttpRequestMessage>(),
            ItExpr.IsAny<CancellationToken>());
    }

    /// <summary>
    /// Tests that GetAsync handles null path parameter at runtime.
    /// Input: Null path string (runtime behavior despite non-nullable annotation)
    /// Expected: Request is processed (runtime allows null despite non-nullable annotation).
    /// </summary>
    [TestMethod]
    public async Task GetAsync_NullPath_ProcessesRequest()
    {
        // Arrange
        string path = null!;
        var expectedResponse = new HttpResponseMessage(HttpStatusCode.OK);
        var mockHandler = new Mock<HttpMessageHandler>();
        mockHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(expectedResponse);
        var httpClient = new HttpClient(mockHandler.Object) { BaseAddress = new Uri("https://api.example.com/") };
        var mockTokenStorage = new Mock<TokenStorageService>();
        mockTokenStorage.Setup(ts => ts.GetAccessTokenAsync()).ReturnsAsync(string.Empty);
        var mockLogger = new Mock<ILogger<ApiService>>();
        var apiService = new ApiService(httpClient, mockTokenStorage.Object, mockLogger.Object);

        // Act
        var result = await apiService.GetAsync(path);

        // Assert
        Assert.IsNotNull(result);
        mockHandler.Protected().Verify(
            "SendAsync",
            Times.Once(),
            ItExpr.IsAny<HttpRequestMessage>(),
            ItExpr.IsAny<CancellationToken>());
    }

    /// <summary>
    /// Tests that GetAsync handles paths with control characters.
    /// Input: Paths containing control characters (newline, tab, null character)
    /// Expected: Request is sent successfully with control characters in path.
    /// </summary>
    [TestMethod]
    [DataRow("api/test\n/path")]
    [DataRow("api/test\t/path")]
    [DataRow("api/test\r/path")]
    [DataRow("api/test\0/path")]
    public async Task GetAsync_PathWithControlCharacters_SendsRequestSuccessfully(string path)
    {
        // Arrange
        var expectedResponse = new HttpResponseMessage(HttpStatusCode.OK);
        var mockHandler = new Mock<HttpMessageHandler>();
        mockHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(expectedResponse);
        var httpClient = new HttpClient(mockHandler.Object) { BaseAddress = new Uri("https://api.example.com/") };
        var mockTokenStorage = new Mock<TokenStorageService>();
        mockTokenStorage.Setup(ts => ts.GetAccessTokenAsync()).ReturnsAsync(string.Empty);
        var mockLogger = new Mock<ILogger<ApiService>>();
        var apiService = new ApiService(httpClient, mockTokenStorage.Object, mockLogger.Object);

        // Act
        var result = await apiService.GetAsync(path);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(HttpStatusCode.OK, result.StatusCode);
        mockHandler.Protected().Verify(
            "SendAsync",
            Times.Once(),
            ItExpr.IsAny<HttpRequestMessage>(),
            ItExpr.IsAny<CancellationToken>());
    }

    /// <summary>
    /// Tests that GetAsync handles boundary HTTP status codes correctly.
    /// Input: Valid path with boundary status codes (100, 599, etc.)
    /// Expected: Returns response with the specified status code.
    /// </summary>
    [TestMethod]
    [DataRow((HttpStatusCode)100)]
    [DataRow((HttpStatusCode)199)]
    [DataRow((HttpStatusCode)599)]
    public async Task GetAsync_BoundaryStatusCodes_ReturnsCorrectStatusCode(HttpStatusCode statusCode)
    {
        // Arrange
        var path = "api/test";
        var expectedResponse = new HttpResponseMessage(statusCode);
        var mockHandler = new Mock<HttpMessageHandler>();
        mockHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(expectedResponse);
        var httpClient = new HttpClient(mockHandler.Object) { BaseAddress = new Uri("https://api.example.com/") };
        var mockTokenStorage = new Mock<TokenStorageService>();
        mockTokenStorage.Setup(ts => ts.GetAccessTokenAsync()).ReturnsAsync(string.Empty);
        var mockLogger = new Mock<ILogger<ApiService>>();
        var apiService = new ApiService(httpClient, mockTokenStorage.Object, mockLogger.Object);

        // Act
        var result = await apiService.GetAsync(path);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(statusCode, result.StatusCode);
        mockHandler.Protected().Verify(
            "SendAsync",
            Times.Once(),
            ItExpr.IsAny<HttpRequestMessage>(),
            ItExpr.IsAny<CancellationToken>());
    }

    /// <summary>
    /// Tests that GetAsync passes no content to the underlying SendWithRefreshAsync method.
    /// Input: Valid path and cancellation token
    /// Expected: Request is sent with GET method and null content.
    /// </summary>
    [TestMethod]
    public async Task GetAsync_AnyPath_SendsRequestWithNoContent()
    {
        // Arrange
        var path = "api/test";
        HttpRequestMessage? capturedRequest = null;
        var expectedResponse = new HttpResponseMessage(HttpStatusCode.OK);
        var mockHandler = new Mock<HttpMessageHandler>();
        mockHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .Callback<HttpRequestMessage, CancellationToken>((req, ct) => capturedRequest = req)
            .ReturnsAsync(expectedResponse);
        var httpClient = new HttpClient(mockHandler.Object) { BaseAddress = new Uri("https://api.example.com/") };
        var mockTokenStorage = new Mock<TokenStorageService>();
        mockTokenStorage.Setup(ts => ts.GetAccessTokenAsync()).ReturnsAsync(string.Empty);
        var mockLogger = new Mock<ILogger<ApiService>>();
        var apiService = new ApiService(httpClient, mockTokenStorage.Object, mockLogger.Object);

        // Act
        var result = await apiService.GetAsync(path);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsNotNull(capturedRequest);
        Assert.AreEqual(HttpMethod.Get, capturedRequest.Method);
        Assert.IsNull(capturedRequest.Content);
    }

    /// <summary>
    /// Tests that GetAsync handles paths with maximum length strings.
    /// Input: Extremely long path string (10000+ characters)
    /// Expected: Request is sent successfully with the extremely long path.
    /// </summary>
    [TestMethod]
    public async Task GetAsync_ExtremelyLongPath_SendsRequestSuccessfully()
    {
        // Arrange
        var path = new string('a', 10000);
        var expectedResponse = new HttpResponseMessage(HttpStatusCode.OK);
        var mockHandler = new Mock<HttpMessageHandler>();
        mockHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(expectedResponse);
        var httpClient = new HttpClient(mockHandler.Object) { BaseAddress = new Uri("https://api.example.com/") };
        var mockTokenStorage = new Mock<TokenStorageService>();
        mockTokenStorage.Setup(ts => ts.GetAccessTokenAsync()).ReturnsAsync(string.Empty);
        var mockLogger = new Mock<ILogger<ApiService>>();
        var apiService = new ApiService(httpClient, mockTokenStorage.Object, mockLogger.Object);

        // Act
        var result = await apiService.GetAsync(path);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(HttpStatusCode.OK, result.StatusCode);
        mockHandler.Protected().Verify(
            "SendAsync",
            Times.Once(),
            ItExpr.IsAny<HttpRequestMessage>(),
            ItExpr.IsAny<CancellationToken>());
    }

    /// <summary>
    /// Tests that PutAsync handles null path parameter.
    /// Input: Null path string, valid content
    /// Expected: Request is processed (runtime allows null despite non-nullable annotation).
    /// </summary>
    [TestMethod]
    public async Task PutAsync_NullPath_ProcessesRequest()
    {
        // Arrange
        string path = null!;
        var content = new StringContent("{\"name\":\"test\"}");
        var expectedResponse = new HttpResponseMessage(HttpStatusCode.OK);

        var mockHandler = new Mock<HttpMessageHandler>();
        mockHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req => req.Method == HttpMethod.Put),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(expectedResponse);

        var httpClient = new HttpClient(mockHandler.Object) { BaseAddress = new Uri("https://api.example.com/") };
        var mockTokenStorage = new Mock<TokenStorageService>();
        mockTokenStorage.Setup(x => x.GetAccessTokenAsync()).ReturnsAsync((string?)null);
        var mockLogger = new Mock<ILogger<ApiService>>();
        var apiService = new ApiService(httpClient, mockTokenStorage.Object, mockLogger.Object);

        // Act
        var result = await apiService.PutAsync(path, content);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(HttpStatusCode.OK, result.StatusCode);
    }

    /// <summary>
    /// Tests that PutAsync handles null content parameter.
    /// Input: Valid path, null content
    /// Expected: Request is sent without content body.
    /// </summary>
    [TestMethod]
    public async Task PutAsync_NullContent_SendsRequestWithoutContent()
    {
        // Arrange
        var path = "api/resource/123";
        HttpContent content = null!;
        var expectedResponse = new HttpResponseMessage(HttpStatusCode.OK);

        var mockHandler = new Mock<HttpMessageHandler>();
        mockHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req => req.Method == HttpMethod.Put && req.Content == null),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(expectedResponse);

        var httpClient = new HttpClient(mockHandler.Object) { BaseAddress = new Uri("https://api.example.com/") };
        var mockTokenStorage = new Mock<TokenStorageService>();
        mockTokenStorage.Setup(x => x.GetAccessTokenAsync()).ReturnsAsync((string?)null);
        var mockLogger = new Mock<ILogger<ApiService>>();
        var apiService = new ApiService(httpClient, mockTokenStorage.Object, mockLogger.Object);

        // Act
        var result = await apiService.PutAsync(path, content);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(HttpStatusCode.OK, result.StatusCode);
    }

    /// <summary>
    /// Tests that PutAsync handles ByteArrayContent correctly.
    /// Input: Valid path, ByteArrayContent
    /// Expected: Request is sent successfully with byte array content.
    /// </summary>
    [TestMethod]
    public async Task PutAsync_ByteArrayContent_SendsRequestSuccessfully()
    {
        // Arrange
        var path = "api/resource/upload";
        var byteArray = new byte[] { 0x01, 0x02, 0x03, 0x04, 0x05 };
        var content = new ByteArrayContent(byteArray);
        var expectedResponse = new HttpResponseMessage(HttpStatusCode.OK);

        var mockHandler = new Mock<HttpMessageHandler>();
        mockHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req => req.Method == HttpMethod.Put && req.Content != null),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(expectedResponse);

        var httpClient = new HttpClient(mockHandler.Object) { BaseAddress = new Uri("https://api.example.com/") };
        var mockTokenStorage = new Mock<TokenStorageService>();
        mockTokenStorage.Setup(x => x.GetAccessTokenAsync()).ReturnsAsync((string?)null);
        var mockLogger = new Mock<ILogger<ApiService>>();
        var apiService = new ApiService(httpClient, mockTokenStorage.Object, mockLogger.Object);

        // Act
        var result = await apiService.PutAsync(path, content);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(HttpStatusCode.OK, result.StatusCode);
    }

    /// <summary>
    /// Tests that PutAsync handles StreamContent correctly.
    /// Input: Valid path, StreamContent
    /// Expected: Request is sent successfully with stream content.
    /// </summary>
    [TestMethod]
    public async Task PutAsync_StreamContent_SendsRequestSuccessfully()
    {
        // Arrange
        var path = "api/resource/stream";
        var stream = new System.IO.MemoryStream(System.Text.Encoding.UTF8.GetBytes("stream data"));
        var content = new StreamContent(stream);
        var expectedResponse = new HttpResponseMessage(HttpStatusCode.OK);

        var mockHandler = new Mock<HttpMessageHandler>();
        mockHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req => req.Method == HttpMethod.Put && req.Content != null),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(expectedResponse);

        var httpClient = new HttpClient(mockHandler.Object) { BaseAddress = new Uri("https://api.example.com/") };
        var mockTokenStorage = new Mock<TokenStorageService>();
        mockTokenStorage.Setup(x => x.GetAccessTokenAsync()).ReturnsAsync((string?)null);
        var mockLogger = new Mock<ILogger<ApiService>>();
        var apiService = new ApiService(httpClient, mockTokenStorage.Object, mockLogger.Object);

        // Act
        var result = await apiService.PutAsync(path, content);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(HttpStatusCode.OK, result.StatusCode);
    }

    /// <summary>
    /// Tests that PutAsync handles MultipartFormDataContent correctly.
    /// Input: Valid path, MultipartFormDataContent
    /// Expected: Request is sent successfully with multipart form data.
    /// </summary>
    [TestMethod]
    public async Task PutAsync_MultipartFormDataContent_SendsRequestSuccessfully()
    {
        // Arrange
        var path = "api/resource/multipart";
        var content = new MultipartFormDataContent
        {
            { new StringContent("value1"), "field1" },
            { new StringContent("value2"), "field2" }
        };
        var expectedResponse = new HttpResponseMessage(HttpStatusCode.OK);

        var mockHandler = new Mock<HttpMessageHandler>();
        mockHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req => req.Method == HttpMethod.Put && req.Content != null),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(expectedResponse);

        var httpClient = new HttpClient(mockHandler.Object) { BaseAddress = new Uri("https://api.example.com/") };
        var mockTokenStorage = new Mock<TokenStorageService>();
        mockTokenStorage.Setup(x => x.GetAccessTokenAsync()).ReturnsAsync((string?)null);
        var mockLogger = new Mock<ILogger<ApiService>>();
        var apiService = new ApiService(httpClient, mockTokenStorage.Object, mockLogger.Object);

        // Act
        var result = await apiService.PutAsync(path, content);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(HttpStatusCode.OK, result.StatusCode);
    }

    /// <summary>
    /// Tests that PutAsync handles content with custom headers.
    /// Input: Valid path, content with custom headers
    /// Expected: Request is sent successfully with custom headers.
    /// </summary>
    [TestMethod]
    public async Task PutAsync_ContentWithCustomHeaders_SendsRequestSuccessfully()
    {
        // Arrange
        var path = "api/resource/123";
        var content = new StringContent("{\"name\":\"test\"}");
        content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
        content.Headers.Add("X-Custom-Header", "CustomValue");
        var expectedResponse = new HttpResponseMessage(HttpStatusCode.OK);

        var mockHandler = new Mock<HttpMessageHandler>();
        mockHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req => req.Method == HttpMethod.Put && req.Content != null),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(expectedResponse);

        var httpClient = new HttpClient(mockHandler.Object) { BaseAddress = new Uri("https://api.example.com/") };
        var mockTokenStorage = new Mock<TokenStorageService>();
        mockTokenStorage.Setup(x => x.GetAccessTokenAsync()).ReturnsAsync((string?)null);
        var mockLogger = new Mock<ILogger<ApiService>>();
        var apiService = new ApiService(httpClient, mockTokenStorage.Object, mockLogger.Object);

        // Act
        var result = await apiService.PutAsync(path, content);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(HttpStatusCode.OK, result.StatusCode);
    }

    /// <summary>
    /// Tests that PutAsync handles path with control characters.
    /// Input: Path with control characters (\r, \n, \t)
    /// Expected: Request is sent successfully.
    /// </summary>
    [TestMethod]
    [DataRow("api/resource\r\nwith\tcontrols")]
    [DataRow("api\nresource")]
    [DataRow("api\tresource")]
    public async Task PutAsync_PathWithControlCharacters_SendsRequestSuccessfully(string path)
    {
        // Arrange
        var content = new StringContent("{\"name\":\"test\"}");
        var expectedResponse = new HttpResponseMessage(HttpStatusCode.OK);

        var mockHandler = new Mock<HttpMessageHandler>();
        mockHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req => req.Method == HttpMethod.Put),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(expectedResponse);

        var httpClient = new HttpClient(mockHandler.Object) { BaseAddress = new Uri("https://api.example.com/") };
        var mockTokenStorage = new Mock<TokenStorageService>();
        mockTokenStorage.Setup(x => x.GetAccessTokenAsync()).ReturnsAsync((string?)null);
        var mockLogger = new Mock<ILogger<ApiService>>();
        var apiService = new ApiService(httpClient, mockTokenStorage.Object, mockLogger.Object);

        // Act
        var result = await apiService.PutAsync(path, content);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(HttpStatusCode.OK, result.StatusCode);
    }

    /// <summary>
    /// Tests that PutAsync handles path with Unicode characters.
    /// Input: Path with Unicode/emoji characters
    /// Expected: Request is sent successfully.
    /// </summary>
    [TestMethod]
    public async Task PutAsync_PathWithUnicodeCharacters_SendsRequestSuccessfully()
    {
        // Arrange
        var path = "api/resource/你好/世界/😀";
        var content = new StringContent("{\"name\":\"test\"}");
        var expectedResponse = new HttpResponseMessage(HttpStatusCode.OK);

        var mockHandler = new Mock<HttpMessageHandler>();
        mockHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req => req.Method == HttpMethod.Put),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(expectedResponse);

        var httpClient = new HttpClient(mockHandler.Object) { BaseAddress = new Uri("https://api.example.com/") };
        var mockTokenStorage = new Mock<TokenStorageService>();
        mockTokenStorage.Setup(x => x.GetAccessTokenAsync()).ReturnsAsync((string?)null);
        var mockLogger = new Mock<ILogger<ApiService>>();
        var apiService = new ApiService(httpClient, mockTokenStorage.Object, mockLogger.Object);

        // Act
        var result = await apiService.PutAsync(path, content);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(HttpStatusCode.OK, result.StatusCode);
    }

    /// <summary>
    /// Tests that PutAsync returns various success HTTP status codes correctly.
    /// Input: Valid path and content, various success status codes
    /// Expected: Response with the success status code is returned.
    /// </summary>
    [TestMethod]
    [DataRow(HttpStatusCode.OK)]
    [DataRow(HttpStatusCode.Accepted)]
    [DataRow(HttpStatusCode.NoContent)]
    [DataRow(HttpStatusCode.Created)]
    public async Task PutAsync_VariousSuccessStatusCodes_ReturnsCorrectStatusCode(HttpStatusCode statusCode)
    {
        // Arrange
        var path = "api/resource/123";
        var content = new StringContent("{\"name\":\"test\"}");
        var expectedResponse = new HttpResponseMessage(statusCode);

        var mockHandler = new Mock<HttpMessageHandler>();
        mockHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req => req.Method == HttpMethod.Put),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(expectedResponse);

        var httpClient = new HttpClient(mockHandler.Object) { BaseAddress = new Uri("https://api.example.com/") };
        var mockTokenStorage = new Mock<TokenStorageService>();
        mockTokenStorage.Setup(x => x.GetAccessTokenAsync()).ReturnsAsync((string?)null);
        var mockLogger = new Mock<ILogger<ApiService>>();
        var apiService = new ApiService(httpClient, mockTokenStorage.Object, mockLogger.Object);

        // Act
        var result = await apiService.PutAsync(path, content);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(statusCode, result.StatusCode);
    }

    /// <summary>
    /// Tests that DeleteAsync handles very long path strings.
    /// Input: Path with 1000+ characters
    /// Expected: Request is sent successfully with the long path.
    /// </summary>
    [TestMethod]
    public async Task DeleteAsync_VeryLongPath_SendsRequestSuccessfully()
    {
        // Arrange
        var longPath = "/api/very/long/path/" + new string('x', 1000);
        var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
        mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage { StatusCode = HttpStatusCode.OK });

        var httpClient = new HttpClient(mockHttpMessageHandler.Object)
        {
            BaseAddress = new Uri("https://api.example.com")
        };

        var mockTokenStorage = new Mock<TokenStorageService>();
        mockTokenStorage.Setup(x => x.GetAccessTokenAsync()).ReturnsAsync("token");

        var mockLogger = new Mock<ILogger<ApiService>>();
        var apiService = new ApiService(httpClient, mockTokenStorage.Object, mockLogger.Object);

        // Act
        var response = await apiService.DeleteAsync(longPath);

        // Assert
        Assert.IsNotNull(response);
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
    }

    /// <summary>
    /// Tests that DeleteAsync handles paths with Unicode and emoji characters.
    /// Input: Path with Unicode/emoji characters
    /// Expected: Request is sent successfully.
    /// </summary>
    [TestMethod]
    public async Task DeleteAsync_PathWithUnicodeCharacters_SendsRequestSuccessfully()
    {
        // Arrange
        var unicodePath = "/api/resource/测试/🚀/данные";
        var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
        mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage { StatusCode = HttpStatusCode.OK });

        var httpClient = new HttpClient(mockHttpMessageHandler.Object)
        {
            BaseAddress = new Uri("https://api.example.com")
        };

        var mockTokenStorage = new Mock<TokenStorageService>();
        mockTokenStorage.Setup(x => x.GetAccessTokenAsync()).ReturnsAsync("token");

        var mockLogger = new Mock<ILogger<ApiService>>();
        var apiService = new ApiService(httpClient, mockTokenStorage.Object, mockLogger.Object);

        // Act
        var response = await apiService.DeleteAsync(unicodePath);

        // Assert
        Assert.IsNotNull(response);
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
    }

    /// <summary>
    /// Tests that DeleteAsync properly passes an explicit cancellation token.
    /// Input: Valid path with an explicit (non-cancelled) cancellation token
    /// Expected: Request is sent successfully with the provided token.
    /// </summary>
    [TestMethod]
    public async Task DeleteAsync_ExplicitCancellationToken_UsesProvidedToken()
    {
        // Arrange
        var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
        mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req => req.Method == HttpMethod.Delete),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage { StatusCode = HttpStatusCode.OK });

        var httpClient = new HttpClient(mockHttpMessageHandler.Object)
        {
            BaseAddress = new Uri("https://api.example.com")
        };

        var mockTokenStorage = new Mock<TokenStorageService>();
        mockTokenStorage.Setup(x => x.GetAccessTokenAsync()).ReturnsAsync("token");

        var mockLogger = new Mock<ILogger<ApiService>>();
        var apiService = new ApiService(httpClient, mockTokenStorage.Object, mockLogger.Object);

        var cts = new CancellationTokenSource();

        // Act
        var response = await apiService.DeleteAsync("/api/resource", cts.Token);

        // Assert
        Assert.IsNotNull(response);
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
    }

    /// <summary>
    /// Tests that DeleteAsync properly includes the path in the request URI.
    /// Input: Specific path string
    /// Expected: The request URI contains the provided path.
    /// </summary>
    [TestMethod]
    public async Task DeleteAsync_WithPath_IncludesPathInRequestUri()
    {
        // Arrange
        var expectedPath = "/api/resource/123";
        Uri? capturedUri = null;
        var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
        mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync((HttpRequestMessage req, CancellationToken ct) =>
            {
                capturedUri = req.RequestUri;
                return new HttpResponseMessage { StatusCode = HttpStatusCode.OK };
            });

        var httpClient = new HttpClient(mockHttpMessageHandler.Object)
        {
            BaseAddress = new Uri("https://api.example.com")
        };

        var mockTokenStorage = new Mock<TokenStorageService>();
        mockTokenStorage.Setup(x => x.GetAccessTokenAsync()).ReturnsAsync("token");

        var mockLogger = new Mock<ILogger<ApiService>>();
        var apiService = new ApiService(httpClient, mockTokenStorage.Object, mockLogger.Object);

        // Act
        await apiService.DeleteAsync(expectedPath);

        // Assert
        Assert.IsNotNull(capturedUri);
        Assert.IsTrue(capturedUri.ToString().Contains(expectedPath));
    }

    /// <summary>
    /// Tests that CheckHealthAsync returns false for 1xx informational status codes.
    /// Input: Health endpoint returns informational status codes (100, 101, 102).
    /// Expected: Returns false (informational codes are not considered healthy).
    /// </summary>
    [TestMethod]
    [DataRow(HttpStatusCode.Continue)]
    [DataRow(HttpStatusCode.SwitchingProtocols)]
    public async Task CheckHealthAsync_InformationalStatusCode_ReturnsFalse(HttpStatusCode statusCode)
    {
        // Arrange
        var mockHandler = new Mock<HttpMessageHandler>();
        mockHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req => req.Method == HttpMethod.Get && req.RequestUri!.ToString().EndsWith("health")),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = statusCode
            });

        var httpClient = new HttpClient(mockHandler.Object)
        {
            BaseAddress = new Uri("http://localhost/")
        };

        var mockTokenStorage = new Mock<TokenStorageService>(Mock.Of<ILogger<TokenStorageService>>());
        var mockLogger = new Mock<ILogger<ApiService>>();
        var apiService = new ApiService(httpClient, mockTokenStorage.Object, mockLogger.Object);

        // Act
        bool result = await apiService.CheckHealthAsync();

        // Assert
        Assert.IsFalse(result);
    }

    /// <summary>
    /// Tests that CheckHealthAsync returns false when an OperationCanceledException is thrown.
    /// Input: GetAsync throws OperationCanceledException.
    /// Expected: Returns false and logs warning.
    /// </summary>
    [TestMethod]
    public async Task CheckHealthAsync_OperationCanceledException_ReturnsFalse()
    {
        // Arrange
        var mockHandler = new Mock<HttpMessageHandler>();
        mockHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new OperationCanceledException("Operation was cancelled."));

        var httpClient = new HttpClient(mockHandler.Object)
        {
            BaseAddress = new Uri("http://localhost/")
        };

        var mockTokenStorage = new Mock<TokenStorageService>(Mock.Of<ILogger<TokenStorageService>>());
        var mockLogger = new Mock<ILogger<ApiService>>();
        var apiService = new ApiService(httpClient, mockTokenStorage.Object, mockLogger.Object);

        // Act
        bool result = await apiService.CheckHealthAsync();

        // Assert
        Assert.IsFalse(result);
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => true),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception?, string>>((v, t) => true)),
            Times.Once);
    }

    /// <summary>
    /// Tests that CheckHealthAsync handles multiple rapid successive calls correctly.
    /// Input: Multiple concurrent calls to CheckHealthAsync.
    /// Expected: All calls complete successfully and return correct results.
    /// </summary>
    [TestMethod]
    public async Task CheckHealthAsync_MultipleConcurrentCalls_AllReturnCorrectResults()
    {
        // Arrange
        var mockHandler = new Mock<HttpMessageHandler>();
        mockHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK
            });

        var httpClient = new HttpClient(mockHandler.Object)
        {
            BaseAddress = new Uri("http://localhost/")
        };

        var mockTokenStorage = new Mock<TokenStorageService>(Mock.Of<ILogger<TokenStorageService>>());
        var mockLogger = new Mock<ILogger<ApiService>>();
        var apiService = new ApiService(httpClient, mockTokenStorage.Object, mockLogger.Object);

        // Act
        var tasks = new Task<bool>[10];
        for (int i = 0; i < 10; i++)
        {
            tasks[i] = apiService.CheckHealthAsync();
        }

        bool[] results = await Task.WhenAll(tasks);

        // Assert
        foreach (bool result in results)
        {
            Assert.IsTrue(result);
        }
    }

    /// <summary>
    /// Tests that CheckHealthAsync does not throw when response IsSuccessStatusCode throws an exception.
    /// Input: Response with unusual state causing IsSuccessStatusCode to potentially fail.
    /// Expected: Method handles gracefully and returns false.
    /// </summary>
    [TestMethod]
    public async Task CheckHealthAsync_InvalidOperationException_ReturnsFalse()
    {
        // Arrange
        var mockHandler = new Mock<HttpMessageHandler>();
        mockHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new InvalidOperationException("Invalid operation during health check."));

        var httpClient = new HttpClient(mockHandler.Object)
        {
            BaseAddress = new Uri("http://localhost/")
        };

        var mockTokenStorage = new Mock<TokenStorageService>(Mock.Of<ILogger<TokenStorageService>>());
        var mockLogger = new Mock<ILogger<ApiService>>();
        var apiService = new ApiService(httpClient, mockTokenStorage.Object, mockLogger.Object);

        // Act
        bool result = await apiService.CheckHealthAsync();

        // Assert
        Assert.IsFalse(result);
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => true),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception?, string>>((v, t) => true)),
            Times.Once);
    }

    /// <summary>
    /// Tests that CheckHealthAsync handles ObjectDisposedException correctly.
    /// Input: GetAsync throws ObjectDisposedException (e.g., HttpClient was disposed).
    /// Expected: Returns false and logs warning.
    /// </summary>
    [TestMethod]
    public async Task CheckHealthAsync_ObjectDisposedException_ReturnsFalse()
    {
        // Arrange
        var mockHandler = new Mock<HttpMessageHandler>();
        mockHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new ObjectDisposedException("HttpClient"));

        var httpClient = new HttpClient(mockHandler.Object)
        {
            BaseAddress = new Uri("http://localhost/")
        };

        var mockTokenStorage = new Mock<TokenStorageService>(Mock.Of<ILogger<TokenStorageService>>());
        var mockLogger = new Mock<ILogger<ApiService>>();
        var apiService = new ApiService(httpClient, mockTokenStorage.Object, mockLogger.Object);

        // Act
        bool result = await apiService.CheckHealthAsync();

        // Assert
        Assert.IsFalse(result);
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => true),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception?, string>>((v, t) => true)),
            Times.Once);
    }

    /// <summary>
    /// Tests that PostAsync respects an explicit (non-default) CancellationToken and passes it to the underlying request.
    /// Input: Valid path, content, and an explicit CancellationToken.
    /// Expected: Request is sent successfully with the explicit token.
    /// </summary>
    [TestMethod]
    public async Task PostAsync_ExplicitCancellationToken_PassesTokenToRequest()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<ApiService>>();
        var mockTokenStorage = new Mock<TokenStorageService>();
        mockTokenStorage.Setup(x => x.GetAccessTokenAsync()).ReturnsAsync("test-token");

        var cts = new CancellationTokenSource();
        var expectedResponse = new HttpResponseMessage(HttpStatusCode.OK);
        var handler = new TestHttpMessageHandler(req =>
        {
            Assert.AreEqual(HttpMethod.Post, req.Method);
            return Task.FromResult(expectedResponse);
        });
        var httpClient = new HttpClient(handler) { BaseAddress = new Uri("https://test.com/") };

        var apiService = new ApiService(httpClient, mockTokenStorage.Object, mockLogger.Object);
        var content = new StringContent("test content");

        // Act
        var result = await apiService.PostAsync("api/test", content, cts.Token);

        // Assert
        Assert.AreEqual(expectedResponse, result);
        Assert.AreEqual(HttpStatusCode.OK, result.StatusCode);
    }

    /// <summary>
    /// Tests that PostAsync handles ByteArrayContent correctly.
    /// Input: Valid path with ByteArrayContent and default CancellationToken.
    /// Expected: Request is sent successfully with byte array content.
    /// </summary>
    [TestMethod]
    public async Task PostAsync_ByteArrayContent_SendsRequestSuccessfully()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<ApiService>>();
        var mockTokenStorage = new Mock<TokenStorageService>();
        mockTokenStorage.Setup(x => x.GetAccessTokenAsync()).ReturnsAsync("test-token");

        var expectedResponse = new HttpResponseMessage(HttpStatusCode.Created);
        var handler = new TestHttpMessageHandler(req =>
        {
            Assert.AreEqual(HttpMethod.Post, req.Method);
            Assert.IsNotNull(req.Content);
            return Task.FromResult(expectedResponse);
        });
        var httpClient = new HttpClient(handler) { BaseAddress = new Uri("https://test.com/") };

        var apiService = new ApiService(httpClient, mockTokenStorage.Object, mockLogger.Object);
        var byteContent = new ByteArrayContent(new byte[] { 1, 2, 3, 4, 5 });

        // Act
        var result = await apiService.PostAsync("api/upload", byteContent);

        // Assert
        Assert.AreEqual(expectedResponse, result);
        Assert.AreEqual(HttpStatusCode.Created, result.StatusCode);
    }

    /// <summary>
    /// Tests that PostAsync handles MultipartFormDataContent correctly.
    /// Input: Valid path with MultipartFormDataContent and default CancellationToken.
    /// Expected: Request is sent successfully with multipart form data.
    /// </summary>
    [TestMethod]
    public async Task PostAsync_MultipartFormDataContent_SendsRequestSuccessfully()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<ApiService>>();
        var mockTokenStorage = new Mock<TokenStorageService>();
        mockTokenStorage.Setup(x => x.GetAccessTokenAsync()).ReturnsAsync("test-token");

        var expectedResponse = new HttpResponseMessage(HttpStatusCode.OK);
        var handler = new TestHttpMessageHandler(req =>
        {
            Assert.AreEqual(HttpMethod.Post, req.Method);
            Assert.IsNotNull(req.Content);
            return Task.FromResult(expectedResponse);
        });
        var httpClient = new HttpClient(handler) { BaseAddress = new Uri("https://test.com/") };

        var apiService = new ApiService(httpClient, mockTokenStorage.Object, mockLogger.Object);
        var multipartContent = new MultipartFormDataContent();
        multipartContent.Add(new StringContent("value1"), "field1");
        multipartContent.Add(new StringContent("value2"), "field2");

        // Act
        var result = await apiService.PostAsync("api/form", multipartContent);

        // Assert
        Assert.AreEqual(expectedResponse, result);
        Assert.AreEqual(HttpStatusCode.OK, result.StatusCode);
    }

    /// <summary>
    /// Tests that PostAsync handles null path parameter.
    /// Input: Null path string.
    /// Expected: Request is processed (runtime allows null despite non-nullable annotation).
    /// </summary>
    [TestMethod]
    public async Task PostAsync_NullPath_ProcessesRequest()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<ApiService>>();
        var mockTokenStorage = new Mock<TokenStorageService>();
        mockTokenStorage.Setup(x => x.GetAccessTokenAsync()).ReturnsAsync("test-token");

        var expectedResponse = new HttpResponseMessage(HttpStatusCode.OK);
        var handler = new TestHttpMessageHandler(req =>
        {
            Assert.AreEqual(HttpMethod.Post, req.Method);
            return Task.FromResult(expectedResponse);
        });
        var httpClient = new HttpClient(handler) { BaseAddress = new Uri("https://test.com/") };

        var apiService = new ApiService(httpClient, mockTokenStorage.Object, mockLogger.Object);
        var content = new StringContent("test");

        // Act
        var result = await apiService.PostAsync(null!, content);

        // Assert
        Assert.AreEqual(expectedResponse, result);
    }

    /// <summary>
    /// Tests that PostAsync handles paths with Unicode and emoji characters.
    /// Input: Path with Unicode/emoji characters.
    /// Expected: Request is sent successfully.
    /// </summary>
    [TestMethod]
    public async Task PostAsync_PathWithUnicodeCharacters_SendsRequestSuccessfully()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<ApiService>>();
        var mockTokenStorage = new Mock<TokenStorageService>();
        mockTokenStorage.Setup(x => x.GetAccessTokenAsync()).ReturnsAsync("test-token");

        var path = "api/资源/🔥/тест";
        var expectedResponse = new HttpResponseMessage(HttpStatusCode.OK);
        var handler = new TestHttpMessageHandler(req =>
        {
            Assert.AreEqual(HttpMethod.Post, req.Method);
            return Task.FromResult(expectedResponse);
        });
        var httpClient = new HttpClient(handler) { BaseAddress = new Uri("https://test.com/") };

        var apiService = new ApiService(httpClient, mockTokenStorage.Object, mockLogger.Object);
        var content = new StringContent("test");

        // Act
        var result = await apiService.PostAsync(path, content);

        // Assert
        Assert.AreEqual(expectedResponse, result);
        Assert.AreEqual(HttpStatusCode.OK, result.StatusCode);
    }

    /// <summary>
    /// Tests that PostAsync handles StreamContent correctly.
    /// Input: Valid path with StreamContent and default CancellationToken.
    /// Expected: Request is sent successfully with stream content.
    /// </summary>
    [TestMethod]
    public async Task PostAsync_StreamContent_SendsRequestSuccessfully()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<ApiService>>();
        var mockTokenStorage = new Mock<TokenStorageService>();
        mockTokenStorage.Setup(x => x.GetAccessTokenAsync()).ReturnsAsync("test-token");

        var expectedResponse = new HttpResponseMessage(HttpStatusCode.Accepted);
        var handler = new TestHttpMessageHandler(req =>
        {
            Assert.AreEqual(HttpMethod.Post, req.Method);
            Assert.IsNotNull(req.Content);
            return Task.FromResult(expectedResponse);
        });
        var httpClient = new HttpClient(handler) { BaseAddress = new Uri("https://test.com/") };

        var apiService = new ApiService(httpClient, mockTokenStorage.Object, mockLogger.Object);
        var stream = new System.IO.MemoryStream(new byte[] { 10, 20, 30, 40 });
        var streamContent = new StreamContent(stream);

        // Act
        var result = await apiService.PostAsync("api/stream", streamContent);

        // Assert
        Assert.AreEqual(expectedResponse, result);
        Assert.AreEqual(HttpStatusCode.Accepted, result.StatusCode);
    }

    /// <summary>
    /// Tests that PostAsync handles FormUrlEncodedContent correctly.
    /// Input: Valid path with FormUrlEncodedContent and default CancellationToken.
    /// Expected: Request is sent successfully with form data.
    /// </summary>
    [TestMethod]
    public async Task PostAsync_FormUrlEncodedContent_SendsRequestSuccessfully()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<ApiService>>();
        var mockTokenStorage = new Mock<TokenStorageService>();
        mockTokenStorage.Setup(x => x.GetAccessTokenAsync()).ReturnsAsync("test-token");

        var expectedResponse = new HttpResponseMessage(HttpStatusCode.OK);
        var handler = new TestHttpMessageHandler(req =>
        {
            Assert.AreEqual(HttpMethod.Post, req.Method);
            Assert.IsNotNull(req.Content);
            return Task.FromResult(expectedResponse);
        });
        var httpClient = new HttpClient(handler) { BaseAddress = new Uri("https://test.com/") };

        var apiService = new ApiService(httpClient, mockTokenStorage.Object, mockLogger.Object);
        var formContent = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("key1", "value1"),
            new KeyValuePair<string, string>("key2", "value2")
        });

        // Act
        var result = await apiService.PostAsync("api/form-data", formContent);

        // Assert
        Assert.AreEqual(expectedResponse, result);
        Assert.AreEqual(HttpStatusCode.OK, result.StatusCode);
    }

    /// <summary>
    /// Tests that PostAsync handles relative paths correctly.
    /// Input: Relative path without leading slash.
    /// Expected: Request is sent successfully with relative path.
    /// </summary>
    [TestMethod]
    [DataRow("api/resource")]
    [DataRow("resource")]
    [DataRow("a/b/c/d/e/f")]
    public async Task PostAsync_RelativePath_SendsRequestSuccessfully(string path)
    {
        // Arrange
        var mockLogger = new Mock<ILogger<ApiService>>();
        var mockTokenStorage = new Mock<TokenStorageService>();
        mockTokenStorage.Setup(x => x.GetAccessTokenAsync()).ReturnsAsync("test-token");

        var expectedResponse = new HttpResponseMessage(HttpStatusCode.OK);
        var handler = new TestHttpMessageHandler(req =>
        {
            Assert.AreEqual(HttpMethod.Post, req.Method);
            return Task.FromResult(expectedResponse);
        });
        var httpClient = new HttpClient(handler) { BaseAddress = new Uri("https://test.com/") };

        var apiService = new ApiService(httpClient, mockTokenStorage.Object, mockLogger.Object);
        var content = new StringContent("test");

        // Act
        var result = await apiService.PostAsync(path, content);

        // Assert
        Assert.AreEqual(expectedResponse, result);
    }

    /// <summary>
    /// Tests that PostAsync handles absolute paths correctly.
    /// Input: Absolute path with leading slash.
    /// Expected: Request is sent successfully with absolute path.
    /// </summary>
    [TestMethod]
    [DataRow("/api/resource")]
    [DataRow("/resource")]
    [DataRow("/a/b/c")]
    public async Task PostAsync_AbsolutePath_SendsRequestSuccessfully(string path)
    {
        // Arrange
        var mockLogger = new Mock<ILogger<ApiService>>();
        var mockTokenStorage = new Mock<TokenStorageService>();
        mockTokenStorage.Setup(x => x.GetAccessTokenAsync()).ReturnsAsync("test-token");

        var expectedResponse = new HttpResponseMessage(HttpStatusCode.OK);
        var handler = new TestHttpMessageHandler(req =>
        {
            Assert.AreEqual(HttpMethod.Post, req.Method);
            return Task.FromResult(expectedResponse);
        });
        var httpClient = new HttpClient(handler) { BaseAddress = new Uri("https://test.com/") };

        var apiService = new ApiService(httpClient, mockTokenStorage.Object, mockLogger.Object);
        var content = new StringContent("test");

        // Act
        var result = await apiService.PostAsync(path, content);

        // Assert
        Assert.AreEqual(expectedResponse, result);
    }
}