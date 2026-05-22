using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Ssomero.Interfaces;
using Ssomero.Models;
using Ssomero.Services;

namespace Ssomero.Services.UnitTests;




/// <summary>
/// Unit tests for the <see cref="AdminService"/> class.
/// </summary>
[TestClass]
public class AdminServiceTests
{
    /// <summary>
    /// Tests that ActivateLecturerAsync returns true when the API call succeeds.
    /// </summary>
    [TestMethod]
    public async Task ActivateLecturerAsync_SuccessfulApiResponse_ReturnsTrue()
    {
        // Arrange
        var lecturerId = Guid.NewGuid();
        var apiMock = new Mock<IApiService>();
        var loggerMock = new Mock<ILogger<AdminService>>();
        var httpResponse = new HttpResponseMessage(System.Net.HttpStatusCode.OK);

        apiMock.Setup(x => x.PostAsync(
            $"api/admin/lecturers/{lecturerId}/activate",
            It.IsAny<StringContent>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(httpResponse);

        var service = new AdminService(apiMock.Object, loggerMock.Object);

        // Act
        var result = await service.ActivateLecturerAsync(lecturerId);

        // Assert
        Assert.IsTrue(result);
        apiMock.Verify(x => x.PostAsync(
            $"api/admin/lecturers/{lecturerId}/activate",
            It.IsAny<StringContent>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Tests that ActivateLecturerAsync returns false when the API call fails.
    /// </summary>
    [TestMethod]
    public async Task ActivateLecturerAsync_UnsuccessfulApiResponse_ReturnsFalse()
    {
        // Arrange
        var lecturerId = Guid.NewGuid();
        var apiMock = new Mock<IApiService>();
        var loggerMock = new Mock<ILogger<AdminService>>();
        var httpResponse = new HttpResponseMessage(System.Net.HttpStatusCode.BadRequest);

        apiMock.Setup(x => x.PostAsync(
            $"api/admin/lecturers/{lecturerId}/activate",
            It.IsAny<StringContent>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(httpResponse);

        var service = new AdminService(apiMock.Object, loggerMock.Object);

        // Act
        var result = await service.ActivateLecturerAsync(lecturerId);

        // Assert
        Assert.IsFalse(result);
    }

    /// <summary>
    /// Tests that ActivateLecturerAsync returns false and logs error when the API call throws an exception.
    /// </summary>
    [TestMethod]
    public async Task ActivateLecturerAsync_ApiThrowsException_ReturnsFalseAndLogsError()
    {
        // Arrange
        var lecturerId = Guid.NewGuid();
        var apiMock = new Mock<IApiService>();
        var loggerMock = new Mock<ILogger<AdminService>>();
        var exception = new HttpRequestException("Network error");

        apiMock.Setup(x => x.PostAsync(
            $"api/admin/lecturers/{lecturerId}/activate",
            It.IsAny<StringContent>(),
            It.IsAny<CancellationToken>()))
            .ThrowsAsync(exception);

        var service = new AdminService(apiMock.Object, loggerMock.Object);

        // Act
        var result = await service.ActivateLecturerAsync(lecturerId);

        // Assert
        Assert.IsFalse(result);
        loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains($"api/admin/lecturers/{lecturerId}/activate")),
                exception,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    /// <summary>
    /// Tests that ActivateLecturerAsync passes the cancellation token to the API call.
    /// </summary>
    [TestMethod]
    public async Task ActivateLecturerAsync_WithCancellationToken_PassesTokenToApiCall()
    {
        // Arrange
        var lecturerId = Guid.NewGuid();
        var cancellationTokenSource = new CancellationTokenSource();
        var cancellationToken = cancellationTokenSource.Token;
        var apiMock = new Mock<IApiService>();
        var loggerMock = new Mock<ILogger<AdminService>>();
        var httpResponse = new HttpResponseMessage(System.Net.HttpStatusCode.OK);

        apiMock.Setup(x => x.PostAsync(
            It.IsAny<string>(),
            It.IsAny<StringContent>(),
            cancellationToken))
            .ReturnsAsync(httpResponse);

        var service = new AdminService(apiMock.Object, loggerMock.Object);

        // Act
        var result = await service.ActivateLecturerAsync(lecturerId, cancellationToken);

        // Assert
        Assert.IsTrue(result);
        apiMock.Verify(x => x.PostAsync(
            $"api/admin/lecturers/{lecturerId}/activate",
            It.IsAny<StringContent>(),
            cancellationToken), Times.Once);
    }

    /// <summary>
    /// Tests that ActivateLecturerAsync works correctly with Guid.Empty.
    /// </summary>
    [TestMethod]
    public async Task ActivateLecturerAsync_WithEmptyGuid_CallsApiWithEmptyGuidPath()
    {
        // Arrange
        var lecturerId = Guid.Empty;
        var apiMock = new Mock<IApiService>();
        var loggerMock = new Mock<ILogger<AdminService>>();
        var httpResponse = new HttpResponseMessage(System.Net.HttpStatusCode.OK);

        apiMock.Setup(x => x.PostAsync(
            $"api/admin/lecturers/{lecturerId}/activate",
            It.IsAny<StringContent>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(httpResponse);

        var service = new AdminService(apiMock.Object, loggerMock.Object);

        // Act
        var result = await service.ActivateLecturerAsync(lecturerId);

        // Assert
        Assert.IsTrue(result);
        apiMock.Verify(x => x.PostAsync(
            $"api/admin/lecturers/00000000-0000-0000-0000-000000000000/activate",
            It.IsAny<StringContent>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Tests that ActivateLecturerAsync constructs the correct API path with the provided lecturer ID.
    /// </summary>
    [TestMethod]
    [DataRow("a1b2c3d4-e5f6-4789-a012-b3c4d5e6f789")]
    [DataRow("12345678-1234-1234-1234-123456789012")]
    [DataRow("ffffffff-ffff-ffff-ffff-ffffffffffff")]
    public async Task ActivateLecturerAsync_WithVariousGuids_ConstructsCorrectPath(string guidString)
    {
        // Arrange
        var lecturerId = Guid.Parse(guidString);
        var apiMock = new Mock<IApiService>();
        var loggerMock = new Mock<ILogger<AdminService>>();
        var httpResponse = new HttpResponseMessage(System.Net.HttpStatusCode.OK);

        apiMock.Setup(x => x.PostAsync(
            It.IsAny<string>(),
            It.IsAny<StringContent>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(httpResponse);

        var service = new AdminService(apiMock.Object, loggerMock.Object);

        // Act
        var result = await service.ActivateLecturerAsync(lecturerId);

        // Assert
        Assert.IsTrue(result);
        apiMock.Verify(x => x.PostAsync(
            $"api/admin/lecturers/{lecturerId}/activate",
            It.IsAny<StringContent>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Tests that ActivateLecturerAsync handles a canceled token gracefully.
    /// </summary>
    [TestMethod]
    public async Task ActivateLecturerAsync_WithCanceledToken_ReturnsFalse()
    {
        // Arrange
        var lecturerId = Guid.NewGuid();
        var cancellationTokenSource = new CancellationTokenSource();
        cancellationTokenSource.Cancel();
        var cancellationToken = cancellationTokenSource.Token;
        var apiMock = new Mock<IApiService>();
        var loggerMock = new Mock<ILogger<AdminService>>();

        apiMock.Setup(x => x.PostAsync(
            It.IsAny<string>(),
            It.IsAny<StringContent>(),
            cancellationToken))
            .ThrowsAsync(new OperationCanceledException(cancellationToken));

        var service = new AdminService(apiMock.Object, loggerMock.Object);

        // Act
        var result = await service.ActivateLecturerAsync(lecturerId, cancellationToken);

        // Assert
        Assert.IsFalse(result);
    }

    /// <summary>
    /// Tests that ActivateLecturerAsync uses default cancellation token when none is provided.
    /// </summary>
    [TestMethod]
    public async Task ActivateLecturerAsync_WithoutCancellationToken_UsesDefault()
    {
        // Arrange
        var lecturerId = Guid.NewGuid();
        var apiMock = new Mock<IApiService>();
        var loggerMock = new Mock<ILogger<AdminService>>();
        var httpResponse = new HttpResponseMessage(System.Net.HttpStatusCode.OK);

        apiMock.Setup(x => x.PostAsync(
            It.IsAny<string>(),
            It.IsAny<StringContent>(),
            default(CancellationToken)))
            .ReturnsAsync(httpResponse);

        var service = new AdminService(apiMock.Object, loggerMock.Object);

        // Act
        var result = await service.ActivateLecturerAsync(lecturerId);

        // Assert
        Assert.IsTrue(result);
        apiMock.Verify(x => x.PostAsync(
            $"api/admin/lecturers/{lecturerId}/activate",
            It.IsAny<StringContent>(),
            default(CancellationToken)), Times.Once);
    }

    /// <summary>
    /// Tests that the constructor successfully initializes the service with valid dependencies.
    /// Input: Valid IApiService and ILogger mocks.
    /// Expected: AdminService instance is created without throwing an exception.
    /// </summary>
    [TestMethod]
    public void AdminService_ValidDependencies_CreatesInstance()
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<AdminService>>();

        // Act
        var service = new AdminService(mockApi.Object, mockLogger.Object);

        // Assert
        Assert.IsNotNull(service);
    }

    /// <summary>
    /// Tests the constructor behavior when the api parameter is null.
    /// Input: Null IApiService, valid ILogger.
    /// Expected: Constructor accepts null (no explicit validation in code).
    /// </summary>
    [TestMethod]
    public void AdminService_NullApi_AcceptsNull()
    {
        // Arrange
        IApiService? nullApi = null;
        var mockLogger = new Mock<ILogger<AdminService>>();

        // Act
        var service = new AdminService(nullApi!, mockLogger.Object);

        // Assert
        Assert.IsNotNull(service);
    }

    /// <summary>
    /// Tests the constructor behavior when the logger parameter is null.
    /// Input: Valid IApiService, null ILogger.
    /// Expected: Constructor accepts null (no explicit validation in code).
    /// </summary>
    [TestMethod]
    public void AdminService_NullLogger_AcceptsNull()
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        ILogger<AdminService>? nullLogger = null;

        // Act
        var service = new AdminService(mockApi.Object, nullLogger!);

        // Assert
        Assert.IsNotNull(service);
    }

    /// <summary>
    /// Tests the constructor behavior when both parameters are null.
    /// Input: Null IApiService and null ILogger.
    /// Expected: Constructor accepts both nulls (no explicit validation in code).
    /// </summary>
    [TestMethod]
    public void AdminService_BothParametersNull_AcceptsNull()
    {
        // Arrange
        IApiService? nullApi = null;
        ILogger<AdminService>? nullLogger = null;

        // Act
        var service = new AdminService(nullApi!, nullLogger!);

        // Assert
        Assert.IsNotNull(service);
    }

    /// <summary>
    /// Tests that DeleteLecturerAsync returns true when the API call succeeds with a valid Guid.
    /// </summary>
    [TestMethod]
    public async Task DeleteLecturerAsync_ValidGuidAndSuccessfulApiCall_ReturnsTrue()
    {
        // Arrange
        var validId = Guid.NewGuid();
        var mockApiService = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<AdminService>>();
        var expectedPath = $"api/admin/lecturers/{validId}/delete";
        var successResponse = new HttpResponseMessage(HttpStatusCode.OK);

        mockApiService
            .Setup(api => api.PostAsync(
                expectedPath,
                It.Is<StringContent>(c => c.Headers.ContentType != null && c.Headers.ContentType.MediaType == "application/json"),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(successResponse);

        var service = new AdminService(mockApiService.Object, mockLogger.Object);

        // Act
        var result = await service.DeleteLecturerAsync(validId);

        // Assert
        Assert.IsTrue(result);
        mockApiService.Verify(api => api.PostAsync(
            expectedPath,
            It.Is<StringContent>(c => c.Headers.ContentType != null && c.Headers.ContentType.MediaType == "application/json"),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Tests that DeleteLecturerAsync returns true when the API call succeeds with an explicit CancellationToken.
    /// </summary>
    [TestMethod]
    public async Task DeleteLecturerAsync_ValidGuidWithExplicitCancellationToken_ReturnsTrue()
    {
        // Arrange
        var validId = Guid.NewGuid();
        var cancellationToken = new CancellationToken();
        var mockApiService = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<AdminService>>();
        var expectedPath = $"api/admin/lecturers/{validId}/delete";
        var successResponse = new HttpResponseMessage(HttpStatusCode.OK);

        mockApiService
            .Setup(api => api.PostAsync(
                expectedPath,
                It.Is<StringContent>(c => c.Headers.ContentType != null && c.Headers.ContentType.MediaType == "application/json"),
                cancellationToken))
            .ReturnsAsync(successResponse);

        var service = new AdminService(mockApiService.Object, mockLogger.Object);

        // Act
        var result = await service.DeleteLecturerAsync(validId, cancellationToken);

        // Assert
        Assert.IsTrue(result);
    }

    /// <summary>
    /// Tests that DeleteLecturerAsync returns false when the API call fails with a non-success status code.
    /// </summary>
    [TestMethod]
    public async Task DeleteLecturerAsync_ApiReturnsFailureStatusCode_ReturnsFalse()
    {
        // Arrange
        var validId = Guid.NewGuid();
        var mockApiService = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<AdminService>>();
        var expectedPath = $"api/admin/lecturers/{validId}/delete";
        var failureResponse = new HttpResponseMessage(HttpStatusCode.InternalServerError);

        mockApiService
            .Setup(api => api.PostAsync(
                expectedPath,
                It.Is<StringContent>(c => c.Headers.ContentType != null && c.Headers.ContentType.MediaType == "application/json"),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(failureResponse);

        var service = new AdminService(mockApiService.Object, mockLogger.Object);

        // Act
        var result = await service.DeleteLecturerAsync(validId);

        // Assert
        Assert.IsFalse(result);
    }

    /// <summary>
    /// Tests that DeleteLecturerAsync handles Guid.Empty correctly and constructs the proper path.
    /// </summary>
    [TestMethod]
    public async Task DeleteLecturerAsync_EmptyGuid_CallsApiWithEmptyGuidInPath()
    {
        // Arrange
        var emptyId = Guid.Empty;
        var mockApiService = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<AdminService>>();
        var expectedPath = $"api/admin/lecturers/{emptyId}/delete";
        var successResponse = new HttpResponseMessage(HttpStatusCode.OK);

        mockApiService
            .Setup(api => api.PostAsync(
                expectedPath,
                It.Is<StringContent>(c => c.Headers.ContentType != null && c.Headers.ContentType.MediaType == "application/json"),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(successResponse);

        var service = new AdminService(mockApiService.Object, mockLogger.Object);

        // Act
        var result = await service.DeleteLecturerAsync(emptyId);

        // Assert
        Assert.IsTrue(result);
        mockApiService.Verify(api => api.PostAsync(
            expectedPath,
            It.Is<StringContent>(c => c.Headers.ContentType != null && c.Headers.ContentType.MediaType == "application/json"),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Tests that DeleteLecturerAsync returns false when the API throws an exception.
    /// </summary>
    [TestMethod]
    public async Task DeleteLecturerAsync_ApiThrowsException_ReturnsFalse()
    {
        // Arrange
        var validId = Guid.NewGuid();
        var mockApiService = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<AdminService>>();
        var expectedPath = $"api/admin/lecturers/{validId}/delete";
        var expectedException = new HttpRequestException("Network error");

        mockApiService
            .Setup(api => api.PostAsync(
                expectedPath,
                It.Is<StringContent>(c => c.Headers.ContentType != null && c.Headers.ContentType.MediaType == "application/json"),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(expectedException);

        var service = new AdminService(mockApiService.Object, mockLogger.Object);

        // Act
        var result = await service.DeleteLecturerAsync(validId);

        // Assert
        Assert.IsFalse(result);
    }

    /// <summary>
    /// Tests that DeleteLecturerAsync returns false when the operation is cancelled.
    /// </summary>
    [TestMethod]
    public async Task DeleteLecturerAsync_OperationCancelled_ReturnsFalse()
    {
        // Arrange
        var validId = Guid.NewGuid();
        var cancellationTokenSource = new CancellationTokenSource();
        cancellationTokenSource.Cancel();
        var cancellationToken = cancellationTokenSource.Token;
        var mockApiService = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<AdminService>>();
        var expectedPath = $"api/admin/lecturers/{validId}/delete";

        mockApiService
            .Setup(api => api.PostAsync(
                expectedPath,
                It.Is<StringContent>(c => c.Headers.ContentType != null && c.Headers.ContentType.MediaType == "application/json"),
                cancellationToken))
            .ThrowsAsync(new OperationCanceledException());

        var service = new AdminService(mockApiService.Object, mockLogger.Object);

        // Act
        var result = await service.DeleteLecturerAsync(validId, cancellationToken);

        // Assert
        Assert.IsFalse(result);
    }

    /// <summary>
    /// Tests that DeleteLecturerAsync returns false for various HTTP error status codes.
    /// </summary>
    /// <param name="statusCode">The HTTP status code to test.</param>
    [TestMethod]
    [DataRow((int)HttpStatusCode.BadRequest)]
    [DataRow((int)HttpStatusCode.Unauthorized)]
    [DataRow((int)HttpStatusCode.Forbidden)]
    [DataRow((int)HttpStatusCode.NotFound)]
    [DataRow((int)HttpStatusCode.InternalServerError)]
    [DataRow((int)HttpStatusCode.ServiceUnavailable)]
    public async Task DeleteLecturerAsync_VariousHttpErrorStatusCodes_ReturnsFalse(int statusCode)
    {
        // Arrange
        var validId = Guid.NewGuid();
        var mockApiService = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<AdminService>>();
        var expectedPath = $"api/admin/lecturers/{validId}/delete";
        var errorResponse = new HttpResponseMessage((HttpStatusCode)statusCode);

        mockApiService
            .Setup(api => api.PostAsync(
                expectedPath,
                It.Is<StringContent>(c => c.Headers.ContentType != null && c.Headers.ContentType.MediaType == "application/json"),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(errorResponse);

        var service = new AdminService(mockApiService.Object, mockLogger.Object);

        // Act
        var result = await service.DeleteLecturerAsync(validId);

        // Assert
        Assert.IsFalse(result);
    }

    /// <summary>
    /// Tests that GetStudentsAsync returns a list of students when the API call is successful with multiple students.
    /// </summary>
    [TestMethod]
    public async Task GetStudentsAsync_SuccessfulResponseWithMultipleStudents_ReturnsListOfStudents()
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<AdminService>>();

        var expectedStudents = new List<UserItem>
        {
            new UserItem
            {
                Id = Guid.NewGuid(),
                Name = "John Doe",
                Email = "john@example.com",
                Role = "Student",
                Status = "Active",
                Program = "Computer Science",
                IsApproved = true,
                CreatedAt = DateTime.UtcNow
            },
            new UserItem
            {
                Id = Guid.NewGuid(),
                Name = "Jane Smith",
                Email = "jane@example.com",
                Role = "Student",
                Status = "Active",
                Program = "Engineering",
                IsApproved = true,
                CreatedAt = DateTime.UtcNow
            }
        };

        var jsonContent = JsonSerializer.Serialize(expectedStudents);
        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(jsonContent, Encoding.UTF8, "application/json")
        };

        mockApi.Setup(api => api.GetAsync("api/admin/students", It.IsAny<CancellationToken>()))
            .ReturnsAsync(httpResponse);

        var service = new AdminService(mockApi.Object, mockLogger.Object);

        // Act
        var result = await service.GetStudentsAsync();

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(2, result.Count);
        Assert.AreEqual(expectedStudents[0].Name, result[0].Name);
        Assert.AreEqual(expectedStudents[1].Email, result[1].Email);
    }

    /// <summary>
    /// Tests that GetStudentsAsync returns an empty list when the API returns an empty array.
    /// </summary>
    [TestMethod]
    public async Task GetStudentsAsync_SuccessfulResponseWithEmptyArray_ReturnsEmptyList()
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<AdminService>>();

        var jsonContent = JsonSerializer.Serialize(new List<UserItem>());
        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(jsonContent, Encoding.UTF8, "application/json")
        };

        mockApi.Setup(api => api.GetAsync("api/admin/students", It.IsAny<CancellationToken>()))
            .ReturnsAsync(httpResponse);

        var service = new AdminService(mockApi.Object, mockLogger.Object);

        // Act
        var result = await service.GetStudentsAsync();

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(0, result.Count);
    }

    /// <summary>
    /// Tests that GetStudentsAsync returns an empty list when the API returns null JSON value.
    /// </summary>
    [TestMethod]
    public async Task GetStudentsAsync_NullJsonValue_ReturnsEmptyList()
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<AdminService>>();

        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("null", Encoding.UTF8, "application/json")
        };

        mockApi.Setup(api => api.GetAsync("api/admin/students", It.IsAny<CancellationToken>()))
            .ReturnsAsync(httpResponse);

        var service = new AdminService(mockApi.Object, mockLogger.Object);

        // Act
        var result = await service.GetStudentsAsync();

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(0, result.Count);
    }

    /// <summary>
    /// Tests that GetStudentsAsync returns a single-item list when the API returns one student.
    /// </summary>
    [TestMethod]
    public async Task GetStudentsAsync_SuccessfulResponseWithSingleStudent_ReturnsSingleItemList()
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<AdminService>>();

        var expectedStudent = new UserItem
        {
            Id = Guid.NewGuid(),
            Name = "Single Student",
            Email = "single@example.com",
            Role = "Student",
            Status = "Active",
            Program = "Mathematics",
            IsApproved = false,
            CreatedAt = DateTime.UtcNow
        };

        var jsonContent = JsonSerializer.Serialize(new List<UserItem> { expectedStudent });
        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(jsonContent, Encoding.UTF8, "application/json")
        };

        mockApi.Setup(api => api.GetAsync("api/admin/students", It.IsAny<CancellationToken>()))
            .ReturnsAsync(httpResponse);

        var service = new AdminService(mockApi.Object, mockLogger.Object);

        // Act
        var result = await service.GetStudentsAsync();

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(1, result.Count);
        Assert.AreEqual(expectedStudent.Name, result[0].Name);
    }

    /// <summary>
    /// Tests that GetStudentsAsync passes the cancellation token to the API call.
    /// </summary>
    [TestMethod]
    public async Task GetStudentsAsync_WithCancellationToken_PassesTokenToApiCall()
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<AdminService>>();

        var jsonContent = JsonSerializer.Serialize(new List<UserItem>());
        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(jsonContent, Encoding.UTF8, "application/json")
        };

        var cts = new CancellationTokenSource();
        var cancellationToken = cts.Token;

        mockApi.Setup(api => api.GetAsync("api/admin/students", cancellationToken))
            .ReturnsAsync(httpResponse);

        var service = new AdminService(mockApi.Object, mockLogger.Object);

        // Act
        await service.GetStudentsAsync(cancellationToken);

        // Assert
        mockApi.Verify(api => api.GetAsync("api/admin/students", cancellationToken), Times.Once);
    }

    /// <summary>
    /// Tests that GetStudentsAsync correctly deserializes students with various property values including nulls.
    /// </summary>
    [TestMethod]
    public async Task GetStudentsAsync_StudentsWithNullableProperties_DeserializesCorrectly()
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<AdminService>>();

        var expectedStudents = new List<UserItem>
        {
            new UserItem
            {
                Id = Guid.Empty,
                Name = string.Empty,
                Email = string.Empty,
                Role = string.Empty,
                Status = "Suspended",
                Program = null,
                StaffId = null,
                IsApproved = false,
                CreatedAt = DateTime.MinValue
            }
        };

        var jsonContent = JsonSerializer.Serialize(expectedStudents);
        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(jsonContent, Encoding.UTF8, "application/json")
        };

        mockApi.Setup(api => api.GetAsync("api/admin/students", It.IsAny<CancellationToken>()))
            .ReturnsAsync(httpResponse);

        var service = new AdminService(mockApi.Object, mockLogger.Object);

        // Act
        var result = await service.GetStudentsAsync();

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(1, result.Count);
        Assert.AreEqual(Guid.Empty, result[0].Id);
        Assert.IsNull(result[0].Program);
        Assert.IsNull(result[0].StaffId);
    }

    /// <summary>
    /// Tests that GetStudentsAsync calls the correct API endpoint.
    /// </summary>
    [TestMethod]
    public async Task GetStudentsAsync_Always_CallsCorrectEndpoint()
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<AdminService>>();

        var jsonContent = JsonSerializer.Serialize(new List<UserItem>());
        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(jsonContent, Encoding.UTF8, "application/json")
        };

        mockApi.Setup(api => api.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(httpResponse);

        var service = new AdminService(mockApi.Object, mockLogger.Object);

        // Act
        await service.GetStudentsAsync();

        // Assert
        mockApi.Verify(api => api.GetAsync("api/admin/students", It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Tests that GetLecturersAsync returns a populated list of lecturers when the API call succeeds with valid data.
    /// </summary>
    [TestMethod]
    public async Task GetLecturersAsync_SuccessfulResponseWithData_ReturnsListOfLecturers()
    {
        // Arrange
        var expectedLecturers = new List<UserItem>
        {
            new UserItem
            {
                Id = Guid.NewGuid(),
                Name = "John Doe",
                Email = "john.doe@example.com",
                Role = "Lecturer",
                Status = "Active",
                StaffId = "STAFF001",
                IsApproved = true,
                CreatedAt = DateTime.UtcNow
            },
            new UserItem
            {
                Id = Guid.NewGuid(),
                Name = "Jane Smith",
                Email = "jane.smith@example.com",
                Role = "Lecturer",
                Status = "Suspended",
                StaffId = "STAFF002",
                IsApproved = true,
                CreatedAt = DateTime.UtcNow.AddDays(-10)
            }
        };
        var jsonContent = JsonSerializer.Serialize(expectedLecturers);
        var httpContent = new StringContent(jsonContent, Encoding.UTF8, "application/json");
        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = httpContent
        };
        var mockApiService = new Mock<IApiService>();
        mockApiService.Setup(x => x.GetAsync("api/admin/lecturers", It.IsAny<CancellationToken>()))
            .ReturnsAsync(httpResponse);
        var mockLogger = new Mock<ILogger<AdminService>>();
        var adminService = new AdminService(mockApiService.Object, mockLogger.Object);

        // Act
        var result = await adminService.GetLecturersAsync();

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(2, result.Count);
        Assert.AreEqual(expectedLecturers[0].Id, result[0].Id);
        Assert.AreEqual(expectedLecturers[0].Name, result[0].Name);
        Assert.AreEqual(expectedLecturers[1].Id, result[1].Id);
        Assert.AreEqual(expectedLecturers[1].Name, result[1].Name);
        mockApiService.Verify(x => x.GetAsync("api/admin/lecturers", It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Tests that GetLecturersAsync returns an empty list when the API call succeeds but returns an empty array.
    /// </summary>
    [TestMethod]
    public async Task GetLecturersAsync_SuccessfulResponseWithEmptyArray_ReturnsEmptyList()
    {
        // Arrange
        var jsonContent = JsonSerializer.Serialize(new List<UserItem>());
        var httpContent = new StringContent(jsonContent, Encoding.UTF8, "application/json");
        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = httpContent
        };
        var mockApiService = new Mock<IApiService>();
        mockApiService.Setup(x => x.GetAsync("api/admin/lecturers", It.IsAny<CancellationToken>()))
            .ReturnsAsync(httpResponse);
        var mockLogger = new Mock<ILogger<AdminService>>();
        var adminService = new AdminService(mockApiService.Object, mockLogger.Object);

        // Act
        var result = await adminService.GetLecturersAsync();

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(0, result.Count);
        mockApiService.Verify(x => x.GetAsync("api/admin/lecturers", It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Tests that GetLecturersAsync returns an empty list when the API response content deserializes to null.
    /// </summary>
    [TestMethod]
    public async Task GetLecturersAsync_ResponseContentDeserializesToNull_ReturnsEmptyList()
    {
        // Arrange
        var jsonContent = "null";
        var httpContent = new StringContent(jsonContent, Encoding.UTF8, "application/json");
        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = httpContent
        };
        var mockApiService = new Mock<IApiService>();
        mockApiService.Setup(x => x.GetAsync("api/admin/lecturers", It.IsAny<CancellationToken>()))
            .ReturnsAsync(httpResponse);
        var mockLogger = new Mock<ILogger<AdminService>>();
        var adminService = new AdminService(mockApiService.Object, mockLogger.Object);

        // Act
        var result = await adminService.GetLecturersAsync();

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(0, result.Count);
        mockApiService.Verify(x => x.GetAsync("api/admin/lecturers", It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Tests that GetLecturersAsync passes the provided CancellationToken to the API service.
    /// </summary>
    [TestMethod]
    public async Task GetLecturersAsync_WithCancellationToken_PassesTokenToApiService()
    {
        // Arrange
        var cts = new CancellationTokenSource();
        var ct = cts.Token;
        var jsonContent = JsonSerializer.Serialize(new List<UserItem>());
        var httpContent = new StringContent(jsonContent, Encoding.UTF8, "application/json");
        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = httpContent
        };
        var mockApiService = new Mock<IApiService>();
        mockApiService.Setup(x => x.GetAsync("api/admin/lecturers", ct))
            .ReturnsAsync(httpResponse);
        var mockLogger = new Mock<ILogger<AdminService>>();
        var adminService = new AdminService(mockApiService.Object, mockLogger.Object);

        // Act
        var result = await adminService.GetLecturersAsync(ct);

        // Assert
        Assert.IsNotNull(result);
        mockApiService.Verify(x => x.GetAsync("api/admin/lecturers", ct), Times.Once);
    }

    /// <summary>
    /// Tests that GetLecturersAsync returns a list with a single lecturer when the API response contains one item.
    /// </summary>
    [TestMethod]
    public async Task GetLecturersAsync_SingleLecturerInResponse_ReturnsListWithOneItem()
    {
        // Arrange
        var expectedLecturer = new UserItem
        {
            Id = Guid.NewGuid(),
            Name = "Single Lecturer",
            Email = "single@example.com",
            Role = "Lecturer",
            Status = "Active",
            StaffId = "STAFF999",
            IsApproved = true,
            CreatedAt = DateTime.UtcNow
        };
        var jsonContent = JsonSerializer.Serialize(new List<UserItem> { expectedLecturer });
        var httpContent = new StringContent(jsonContent, Encoding.UTF8, "application/json");
        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = httpContent
        };
        var mockApiService = new Mock<IApiService>();
        mockApiService.Setup(x => x.GetAsync("api/admin/lecturers", It.IsAny<CancellationToken>()))
            .ReturnsAsync(httpResponse);
        var mockLogger = new Mock<ILogger<AdminService>>();
        var adminService = new AdminService(mockApiService.Object, mockLogger.Object);

        // Act
        var result = await adminService.GetLecturersAsync();

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(1, result.Count);
        Assert.AreEqual(expectedLecturer.Id, result[0].Id);
        Assert.AreEqual(expectedLecturer.Name, result[0].Name);
        Assert.AreEqual(expectedLecturer.Email, result[0].Email);
        mockApiService.Verify(x => x.GetAsync("api/admin/lecturers", It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Tests that GetLecturersAsync correctly deserializes lecturers with various property values including nullable fields.
    /// </summary>
    [TestMethod]
    public async Task GetLecturersAsync_LecturersWithVariousPropertyValues_DeserializesCorrectly()
    {
        // Arrange
        var expectedLecturers = new List<UserItem>
        {
            new UserItem
            {
                Id = Guid.Empty,
                Name = string.Empty,
                Email = string.Empty,
                Role = "Lecturer",
                Status = "Active",
                StaffId = null,
                Program = null,
                IsApproved = false,
                CreatedAt = DateTime.MinValue
            },
            new UserItem
            {
                Id = Guid.Parse("ffffffff-ffff-ffff-ffff-ffffffffffff"),
                Name = "Very Long Name With Special Characters @#$%^&*()",
                Email = "test+special@example.co.uk",
                Role = "Admin",
                Status = "Suspended",
                StaffId = "STAFF-123-456",
                Program = "Computer Science",
                IsApproved = true,
                CreatedAt = DateTime.MaxValue
            }
        };
        var jsonContent = JsonSerializer.Serialize(expectedLecturers);
        var httpContent = new StringContent(jsonContent, Encoding.UTF8, "application/json");
        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = httpContent
        };
        var mockApiService = new Mock<IApiService>();
        mockApiService.Setup(x => x.GetAsync("api/admin/lecturers", It.IsAny<CancellationToken>()))
            .ReturnsAsync(httpResponse);
        var mockLogger = new Mock<ILogger<AdminService>>();
        var adminService = new AdminService(mockApiService.Object, mockLogger.Object);

        // Act
        var result = await adminService.GetLecturersAsync();

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(2, result.Count);
        Assert.AreEqual(Guid.Empty, result[0].Id);
        Assert.AreEqual(string.Empty, result[0].Name);
        Assert.IsNull(result[0].StaffId);
        Assert.AreEqual(Guid.Parse("ffffffff-ffff-ffff-ffff-ffffffffffff"), result[1].Id);
        Assert.AreEqual("Very Long Name With Special Characters @#$%^&*()", result[1].Name);
        Assert.AreEqual("STAFF-123-456", result[1].StaffId);
        mockApiService.Verify(x => x.GetAsync("api/admin/lecturers", It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Tests that SuspendStudentAsync returns true when the API call succeeds.
    /// Input: Valid student ID and successful API response.
    /// Expected: Method returns true.
    /// </summary>
    [TestMethod]
    public async Task SuspendStudentAsync_SuccessfulApiResponse_ReturnsTrue()
    {
        // Arrange
        var studentId = Guid.NewGuid();
        var mockApiService = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<AdminService>>();
        var successResponse = new HttpResponseMessage(HttpStatusCode.OK);

        mockApiService
            .Setup(x => x.PostAsync(
                $"api/admin/students/{studentId}/suspend",
                It.IsAny<StringContent>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(successResponse);

        var service = new AdminService(mockApiService.Object, mockLogger.Object);

        // Act
        var result = await service.SuspendStudentAsync(studentId);

        // Assert
        Assert.IsTrue(result);
        mockApiService.Verify(
            x => x.PostAsync(
                $"api/admin/students/{studentId}/suspend",
                It.IsAny<StringContent>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    /// <summary>
    /// Tests that SuspendStudentAsync returns false when the API call fails.
    /// Input: Valid student ID and failed API response (4xx or 5xx status code).
    /// Expected: Method returns false.
    /// </summary>
    [TestMethod]
    [DataRow(HttpStatusCode.BadRequest)]
    [DataRow(HttpStatusCode.NotFound)]
    [DataRow(HttpStatusCode.InternalServerError)]
    [DataRow(HttpStatusCode.Unauthorized)]
    public async Task SuspendStudentAsync_FailedApiResponse_ReturnsFalse(HttpStatusCode statusCode)
    {
        // Arrange
        var studentId = Guid.NewGuid();
        var mockApiService = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<AdminService>>();
        var failedResponse = new HttpResponseMessage(statusCode);

        mockApiService
            .Setup(x => x.PostAsync(
                It.IsAny<string>(),
                It.IsAny<StringContent>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(failedResponse);

        var service = new AdminService(mockApiService.Object, mockLogger.Object);

        // Act
        var result = await service.SuspendStudentAsync(studentId);

        // Assert
        Assert.IsFalse(result);
    }

    /// <summary>
    /// Tests that SuspendStudentAsync handles Guid.Empty correctly.
    /// Input: Empty Guid (Guid.Empty).
    /// Expected: Method calls PostAsync with correct path including empty Guid.
    /// </summary>
    [TestMethod]
    public async Task SuspendStudentAsync_EmptyGuid_CallsPostAsyncWithCorrectPath()
    {
        // Arrange
        var studentId = Guid.Empty;
        var mockApiService = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<AdminService>>();
        var successResponse = new HttpResponseMessage(HttpStatusCode.OK);

        mockApiService
            .Setup(x => x.PostAsync(
                $"api/admin/students/{studentId}/suspend",
                It.IsAny<StringContent>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(successResponse);

        var service = new AdminService(mockApiService.Object, mockLogger.Object);

        // Act
        var result = await service.SuspendStudentAsync(studentId);

        // Assert
        Assert.IsTrue(result);
        mockApiService.Verify(
            x => x.PostAsync(
                $"api/admin/students/{Guid.Empty}/suspend",
                It.IsAny<StringContent>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    /// <summary>
    /// Tests that SuspendStudentAsync properly passes the cancellation token to the API service.
    /// Input: Valid student ID and a custom cancellation token.
    /// Expected: PostAsync is called with the provided cancellation token.
    /// </summary>
    [TestMethod]
    public async Task SuspendStudentAsync_WithCancellationToken_PassesCancellationTokenToApi()
    {
        // Arrange
        var studentId = Guid.NewGuid();
        var cancellationTokenSource = new CancellationTokenSource();
        var cancellationToken = cancellationTokenSource.Token;
        var mockApiService = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<AdminService>>();
        var successResponse = new HttpResponseMessage(HttpStatusCode.OK);

        mockApiService
            .Setup(x => x.PostAsync(
                It.IsAny<string>(),
                It.IsAny<StringContent>(),
                cancellationToken))
            .ReturnsAsync(successResponse);

        var service = new AdminService(mockApiService.Object, mockLogger.Object);

        // Act
        var result = await service.SuspendStudentAsync(studentId, cancellationToken);

        // Assert
        Assert.IsTrue(result);
        mockApiService.Verify(
            x => x.PostAsync(
                It.IsAny<string>(),
                It.IsAny<StringContent>(),
                cancellationToken),
            Times.Once);
    }

    /// <summary>
    /// Tests that SuspendStudentAsync returns false and logs error when PostAsync throws an exception.
    /// Input: Valid student ID, but PostAsync throws an exception.
    /// Expected: Method returns false and exception is logged.
    /// </summary>
    [TestMethod]
    public async Task SuspendStudentAsync_PostAsyncThrowsException_ReturnsFalseAndLogsError()
    {
        // Arrange
        var studentId = Guid.NewGuid();
        var expectedException = new HttpRequestException("Network error");
        var mockApiService = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<AdminService>>();

        mockApiService
            .Setup(x => x.PostAsync(
                It.IsAny<string>(),
                It.IsAny<StringContent>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(expectedException);

        var service = new AdminService(mockApiService.Object, mockLogger.Object);

        // Act
        var result = await service.SuspendStudentAsync(studentId);

        // Assert
        Assert.IsFalse(result);
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains($"api/admin/students/{studentId}/suspend")),
                expectedException,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    /// <summary>
    /// Tests that SuspendStudentAsync uses default cancellation token when none is provided.
    /// Input: Valid student ID without explicit cancellation token.
    /// Expected: Method calls PostAsync successfully with default cancellation token.
    /// </summary>
    [TestMethod]
    public async Task SuspendStudentAsync_NoCancellationToken_UsesDefaultToken()
    {
        // Arrange
        var studentId = Guid.NewGuid();
        var mockApiService = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<AdminService>>();
        var successResponse = new HttpResponseMessage(HttpStatusCode.OK);

        mockApiService
            .Setup(x => x.PostAsync(
                It.IsAny<string>(),
                It.IsAny<StringContent>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(successResponse);

        var service = new AdminService(mockApiService.Object, mockLogger.Object);

        // Act
        var result = await service.SuspendStudentAsync(studentId);

        // Assert
        Assert.IsTrue(result);
        mockApiService.Verify(
            x => x.PostAsync(
                It.IsAny<string>(),
                It.IsAny<StringContent>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    /// <summary>
    /// Tests that SuspendStudentAsync properly formats the API path with different Guid values.
    /// Input: Various Guid values.
    /// Expected: PostAsync is called with correctly formatted path for each Guid.
    /// </summary>
    [TestMethod]
    public async Task SuspendStudentAsync_VariousGuids_FormatsPathCorrectly()
    {
        // Arrange
        var testGuid1 = new Guid("12345678-1234-1234-1234-123456789abc");
        var testGuid2 = new Guid("ffffffff-ffff-ffff-ffff-ffffffffffff");
        var mockApiService = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<AdminService>>();
        var successResponse = new HttpResponseMessage(HttpStatusCode.OK);

        mockApiService
            .Setup(x => x.PostAsync(
                It.IsAny<string>(),
                It.IsAny<StringContent>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(successResponse);

        var service = new AdminService(mockApiService.Object, mockLogger.Object);

        // Act
        await service.SuspendStudentAsync(testGuid1);
        await service.SuspendStudentAsync(testGuid2);

        // Assert
        mockApiService.Verify(
            x => x.PostAsync(
                $"api/admin/students/{testGuid1}/suspend",
                It.IsAny<StringContent>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
        mockApiService.Verify(
            x => x.PostAsync(
                $"api/admin/students/{testGuid2}/suspend",
                It.IsAny<StringContent>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    /// <summary>
    /// Tests that SuspendStudentAsync handles task cancellation properly.
    /// Input: Valid student ID with already cancelled cancellation token.
    /// Expected: OperationCanceledException is thrown or method returns false.
    /// </summary>
    [TestMethod]
    public async Task SuspendStudentAsync_CancelledToken_HandlesOperationCanceled()
    {
        // Arrange
        var studentId = Guid.NewGuid();
        var cancellationTokenSource = new CancellationTokenSource();
        cancellationTokenSource.Cancel();
        var mockApiService = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<AdminService>>();

        mockApiService
            .Setup(x => x.PostAsync(
                It.IsAny<string>(),
                It.IsAny<StringContent>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new OperationCanceledException());

        var service = new AdminService(mockApiService.Object, mockLogger.Object);

        // Act
        var result = await service.SuspendStudentAsync(studentId, cancellationTokenSource.Token);

        // Assert
        Assert.IsFalse(result);
    }

    /// <summary>
    /// Tests that DeleteStudentAsync returns true when the API call succeeds.
    /// Input: Valid Guid and default cancellation token.
    /// Expected: Returns true when the response has a success status code.
    /// </summary>
    [TestMethod]
    public async Task DeleteStudentAsync_ValidIdWithSuccessResponse_ReturnsTrue()
    {
        // Arrange
        var studentId = Guid.NewGuid();
        var mockApi = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<AdminService>>();
        var successResponse = new HttpResponseMessage(HttpStatusCode.OK);

        mockApi.Setup(x => x.PostAsync(
            $"api/admin/students/{studentId}/delete",
            It.IsAny<HttpContent>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(successResponse);

        var service = new AdminService(mockApi.Object, mockLogger.Object);

        // Act
        var result = await service.DeleteStudentAsync(studentId);

        // Assert
        Assert.IsTrue(result);
        mockApi.Verify(x => x.PostAsync(
            $"api/admin/students/{studentId}/delete",
            It.IsAny<HttpContent>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Tests that DeleteStudentAsync returns false when the API call fails.
    /// Input: Valid Guid with a failure response from the API.
    /// Expected: Returns false when the response has a failure status code.
    /// </summary>
    [TestMethod]
    public async Task DeleteStudentAsync_ValidIdWithFailureResponse_ReturnsFalse()
    {
        // Arrange
        var studentId = Guid.NewGuid();
        var mockApi = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<AdminService>>();
        var failureResponse = new HttpResponseMessage(HttpStatusCode.BadRequest);

        mockApi.Setup(x => x.PostAsync(
            $"api/admin/students/{studentId}/delete",
            It.IsAny<HttpContent>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(failureResponse);

        var service = new AdminService(mockApi.Object, mockLogger.Object);

        // Act
        var result = await service.DeleteStudentAsync(studentId);

        // Assert
        Assert.IsFalse(result);
    }

    /// <summary>
    /// Tests that DeleteStudentAsync returns false when an exception is thrown.
    /// Input: Valid Guid with an exception thrown during the API call.
    /// Expected: Returns false and logs the error.
    /// </summary>
    [TestMethod]
    public async Task DeleteStudentAsync_ApiThrowsException_ReturnsFalseAndLogsError()
    {
        // Arrange
        var studentId = Guid.NewGuid();
        var mockApi = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<AdminService>>();
        var expectedException = new HttpRequestException("Network error");

        mockApi.Setup(x => x.PostAsync(
            It.IsAny<string>(),
            It.IsAny<HttpContent>(),
            It.IsAny<CancellationToken>()))
            .ThrowsAsync(expectedException);

        var service = new AdminService(mockApi.Object, mockLogger.Object);

        // Act
        var result = await service.DeleteStudentAsync(studentId);

        // Assert
        Assert.IsFalse(result);
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains($"api/admin/students/{studentId}/delete")),
                expectedException,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    /// <summary>
    /// Tests that DeleteStudentAsync works correctly with Guid.Empty.
    /// Input: Guid.Empty and default cancellation token.
    /// Expected: Returns true when the response has a success status code, verifies correct path construction.
    /// </summary>
    [TestMethod]
    public async Task DeleteStudentAsync_EmptyGuid_ConstructsCorrectPathAndReturnsTrue()
    {
        // Arrange
        var studentId = Guid.Empty;
        var mockApi = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<AdminService>>();
        var successResponse = new HttpResponseMessage(HttpStatusCode.OK);

        mockApi.Setup(x => x.PostAsync(
            $"api/admin/students/{studentId}/delete",
            It.IsAny<HttpContent>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(successResponse);

        var service = new AdminService(mockApi.Object, mockLogger.Object);

        // Act
        var result = await service.DeleteStudentAsync(studentId);

        // Assert
        Assert.IsTrue(result);
        mockApi.Verify(x => x.PostAsync(
            "api/admin/students/00000000-0000-0000-0000-000000000000/delete",
            It.IsAny<HttpContent>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Tests that DeleteStudentAsync passes the cancellation token correctly.
    /// Input: Valid Guid and a specific cancellation token.
    /// Expected: The cancellation token is passed through to the API call.
    /// </summary>
    [TestMethod]
    public async Task DeleteStudentAsync_WithCancellationToken_PassesTokenToApi()
    {
        // Arrange
        var studentId = Guid.NewGuid();
        var cts = new CancellationTokenSource();
        var mockApi = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<AdminService>>();
        var successResponse = new HttpResponseMessage(HttpStatusCode.OK);

        mockApi.Setup(x => x.PostAsync(
            It.IsAny<string>(),
            It.IsAny<HttpContent>(),
            cts.Token))
            .ReturnsAsync(successResponse);

        var service = new AdminService(mockApi.Object, mockLogger.Object);

        // Act
        var result = await service.DeleteStudentAsync(studentId, cts.Token);

        // Assert
        Assert.IsTrue(result);
        mockApi.Verify(x => x.PostAsync(
            It.IsAny<string>(),
            It.IsAny<HttpContent>(),
            cts.Token), Times.Once);
    }

    /// <summary>
    /// Tests that DeleteStudentAsync handles operation cancellation correctly.
    /// Input: Valid Guid and a cancelled cancellation token.
    /// Expected: Returns false when operation is cancelled (exception is caught).
    /// </summary>
    [TestMethod]
    public async Task DeleteStudentAsync_WithCancelledToken_ReturnsFalseAndLogsError()
    {
        // Arrange
        var studentId = Guid.NewGuid();
        var cts = new CancellationTokenSource();
        cts.Cancel();
        var mockApi = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<AdminService>>();

        mockApi.Setup(x => x.PostAsync(
            It.IsAny<string>(),
            It.IsAny<HttpContent>(),
            cts.Token))
            .ThrowsAsync(new OperationCanceledException());

        var service = new AdminService(mockApi.Object, mockLogger.Object);

        // Act
        var result = await service.DeleteStudentAsync(studentId, cts.Token);

        // Assert
        Assert.IsFalse(result);
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => true),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    /// <summary>
    /// Tests that DeleteStudentAsync constructs the correct API path for various Guid values.
    /// Input: Multiple different Guid values.
    /// Expected: The correct path is constructed for each Guid.
    /// </summary>
    [TestMethod]
    [DataRow("a1b2c3d4-e5f6-4789-abcd-ef0123456789")]
    [DataRow("00000000-0000-0000-0000-000000000001")]
    [DataRow("ffffffff-ffff-ffff-ffff-ffffffffffff")]
    public async Task DeleteStudentAsync_VariousGuids_ConstructsCorrectPath(string guidString)
    {
        // Arrange
        var studentId = Guid.Parse(guidString);
        var expectedPath = $"api/admin/students/{studentId}/delete";
        var mockApi = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<AdminService>>();
        var successResponse = new HttpResponseMessage(HttpStatusCode.OK);

        mockApi.Setup(x => x.PostAsync(
            expectedPath,
            It.IsAny<HttpContent>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(successResponse);

        var service = new AdminService(mockApi.Object, mockLogger.Object);

        // Act
        var result = await service.DeleteStudentAsync(studentId);

        // Assert
        Assert.IsTrue(result);
        mockApi.Verify(x => x.PostAsync(
            expectedPath,
            It.IsAny<HttpContent>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Tests that DeleteStudentAsync returns false for various HTTP error status codes.
    /// Input: Valid Guid with different HTTP error status codes.
    /// Expected: Returns false for all error status codes.
    /// </summary>
    [TestMethod]
    [DataRow(HttpStatusCode.NotFound)]
    [DataRow(HttpStatusCode.InternalServerError)]
    [DataRow(HttpStatusCode.Unauthorized)]
    [DataRow(HttpStatusCode.Forbidden)]
    [DataRow(HttpStatusCode.BadGateway)]
    public async Task DeleteStudentAsync_VariousErrorStatusCodes_ReturnsFalse(HttpStatusCode statusCode)
    {
        // Arrange
        var studentId = Guid.NewGuid();
        var mockApi = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<AdminService>>();
        var errorResponse = new HttpResponseMessage(statusCode);

        mockApi.Setup(x => x.PostAsync(
            It.IsAny<string>(),
            It.IsAny<HttpContent>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(errorResponse);

        var service = new AdminService(mockApi.Object, mockLogger.Object);

        // Act
        var result = await service.DeleteStudentAsync(studentId);

        // Assert
        Assert.IsFalse(result);
    }

    /// <summary>
    /// Tests that SuspendLecturerAsync returns true when the API responds with a success status code.
    /// </summary>
    [TestMethod]
    public async Task SuspendLecturerAsync_ValidIdWithSuccessResponse_ReturnsTrue()
    {
        // Arrange
        var lecturerId = Guid.NewGuid();
        var apiMock = new Mock<IApiService>();
        var loggerMock = new Mock<ILogger<AdminService>>();
        var successResponse = new HttpResponseMessage(HttpStatusCode.OK);

        apiMock
            .Setup(x => x.PostAsync(
                $"api/admin/lecturers/{lecturerId}/suspend",
                It.IsAny<StringContent>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(successResponse);

        var service = new AdminService(apiMock.Object, loggerMock.Object);

        // Act
        var result = await service.SuspendLecturerAsync(lecturerId);

        // Assert
        Assert.IsTrue(result);
        apiMock.Verify(x => x.PostAsync(
            $"api/admin/lecturers/{lecturerId}/suspend",
            It.IsAny<StringContent>(),
            It.IsAny<CancellationToken>()),
            Times.Once);
    }

    /// <summary>
    /// Tests that SuspendLecturerAsync returns false when the API responds with a failure status code.
    /// </summary>
    [TestMethod]
    public async Task SuspendLecturerAsync_ValidIdWithFailureResponse_ReturnsFalse()
    {
        // Arrange
        var lecturerId = Guid.NewGuid();
        var apiMock = new Mock<IApiService>();
        var loggerMock = new Mock<ILogger<AdminService>>();
        var failureResponse = new HttpResponseMessage(HttpStatusCode.BadRequest);

        apiMock
            .Setup(x => x.PostAsync(
                $"api/admin/lecturers/{lecturerId}/suspend",
                It.IsAny<StringContent>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(failureResponse);

        var service = new AdminService(apiMock.Object, loggerMock.Object);

        // Act
        var result = await service.SuspendLecturerAsync(lecturerId);

        // Assert
        Assert.IsFalse(result);
    }

    /// <summary>
    /// Tests that SuspendLecturerAsync returns false and logs an error when the API throws an exception.
    /// </summary>
    [TestMethod]
    public async Task SuspendLecturerAsync_ApiThrowsException_ReturnsFalseAndLogsError()
    {
        // Arrange
        var lecturerId = Guid.NewGuid();
        var apiMock = new Mock<IApiService>();
        var loggerMock = new Mock<ILogger<AdminService>>();
        var expectedException = new HttpRequestException("Network error");

        apiMock
            .Setup(x => x.PostAsync(
                $"api/admin/lecturers/{lecturerId}/suspend",
                It.IsAny<StringContent>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(expectedException);

        var service = new AdminService(apiMock.Object, loggerMock.Object);

        // Act
        var result = await service.SuspendLecturerAsync(lecturerId);

        // Assert
        Assert.IsFalse(result);
        loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => true),
                expectedException,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    /// <summary>
    /// Tests that SuspendLecturerAsync works correctly with Guid.Empty and returns true on success.
    /// </summary>
    [TestMethod]
    public async Task SuspendLecturerAsync_EmptyGuidWithSuccessResponse_ReturnsTrue()
    {
        // Arrange
        var lecturerId = Guid.Empty;
        var apiMock = new Mock<IApiService>();
        var loggerMock = new Mock<ILogger<AdminService>>();
        var successResponse = new HttpResponseMessage(HttpStatusCode.OK);

        apiMock
            .Setup(x => x.PostAsync(
                $"api/admin/lecturers/{lecturerId}/suspend",
                It.IsAny<StringContent>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(successResponse);

        var service = new AdminService(apiMock.Object, loggerMock.Object);

        // Act
        var result = await service.SuspendLecturerAsync(lecturerId);

        // Assert
        Assert.IsTrue(result);
    }

    /// <summary>
    /// Tests that SuspendLecturerAsync passes the cancellation token correctly to the API service.
    /// </summary>
    [TestMethod]
    public async Task SuspendLecturerAsync_WithCancellationToken_PassesTokenToApi()
    {
        // Arrange
        var lecturerId = Guid.NewGuid();
        var cancellationTokenSource = new CancellationTokenSource();
        var cancellationToken = cancellationTokenSource.Token;
        var apiMock = new Mock<IApiService>();
        var loggerMock = new Mock<ILogger<AdminService>>();
        var successResponse = new HttpResponseMessage(HttpStatusCode.OK);

        apiMock
            .Setup(x => x.PostAsync(
                $"api/admin/lecturers/{lecturerId}/suspend",
                It.IsAny<StringContent>(),
                cancellationToken))
            .ReturnsAsync(successResponse);

        var service = new AdminService(apiMock.Object, loggerMock.Object);

        // Act
        var result = await service.SuspendLecturerAsync(lecturerId, cancellationToken);

        // Assert
        Assert.IsTrue(result);
        apiMock.Verify(x => x.PostAsync(
            $"api/admin/lecturers/{lecturerId}/suspend",
            It.IsAny<StringContent>(),
            cancellationToken),
            Times.Once);
    }

    /// <summary>
    /// Tests that SuspendLecturerAsync handles OperationCanceledException when cancellation is requested.
    /// </summary>
    [TestMethod]
    public async Task SuspendLecturerAsync_WithCancelledToken_ReturnsFalseAndLogsError()
    {
        // Arrange
        var lecturerId = Guid.NewGuid();
        var cancellationTokenSource = new CancellationTokenSource();
        cancellationTokenSource.Cancel();
        var cancellationToken = cancellationTokenSource.Token;
        var apiMock = new Mock<IApiService>();
        var loggerMock = new Mock<ILogger<AdminService>>();

        apiMock
            .Setup(x => x.PostAsync(
                $"api/admin/lecturers/{lecturerId}/suspend",
                It.IsAny<StringContent>(),
                cancellationToken))
            .ThrowsAsync(new OperationCanceledException());

        var service = new AdminService(apiMock.Object, loggerMock.Object);

        // Act
        var result = await service.SuspendLecturerAsync(lecturerId, cancellationToken);

        // Assert
        Assert.IsFalse(result);
    }

    /// <summary>
    /// Tests that SuspendLecturerAsync constructs the correct API path with various Guid formats.
    /// </summary>
    /// <param name="guidString">The GUID string to test.</param>
    /// <param name="expectedInPath">Expected GUID representation in the path.</param>
    [TestMethod]
    [DataRow("00000000-0000-0000-0000-000000000001", "00000000-0000-0000-0000-000000000001")]
    [DataRow("ffffffff-ffff-ffff-ffff-ffffffffffff", "ffffffff-ffff-ffff-ffff-ffffffffffff")]
    [DataRow("12345678-1234-1234-1234-123456789abc", "12345678-1234-1234-1234-123456789abc")]
    public async Task SuspendLecturerAsync_VariousGuids_ConstructsCorrectPath(string guidString, string expectedInPath)
    {
        // Arrange
        var lecturerId = Guid.Parse(guidString);
        var apiMock = new Mock<IApiService>();
        var loggerMock = new Mock<ILogger<AdminService>>();
        var successResponse = new HttpResponseMessage(HttpStatusCode.OK);
        var expectedPath = $"api/admin/lecturers/{expectedInPath}/suspend";

        apiMock
            .Setup(x => x.PostAsync(
                expectedPath,
                It.IsAny<StringContent>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(successResponse);

        var service = new AdminService(apiMock.Object, loggerMock.Object);

        // Act
        var result = await service.SuspendLecturerAsync(lecturerId);

        // Assert
        Assert.IsTrue(result);
        apiMock.Verify(x => x.PostAsync(
            expectedPath,
            It.IsAny<StringContent>(),
            It.IsAny<CancellationToken>()),
            Times.Once);
    }

    /// <summary>
    /// Tests that SuspendLecturerAsync handles different HTTP error status codes correctly.
    /// </summary>
    /// <param name="statusCode">The HTTP status code to test.</param>
    /// <param name="expectedResult">The expected boolean result.</param>
    [TestMethod]
    [DataRow(200, true)]
    [DataRow(201, true)]
    [DataRow(204, true)]
    [DataRow(400, false)]
    [DataRow(401, false)]
    [DataRow(403, false)]
    [DataRow(404, false)]
    [DataRow(500, false)]
    [DataRow(503, false)]
    public async Task SuspendLecturerAsync_VariousHttpStatusCodes_ReturnsExpectedResult(int statusCode, bool expectedResult)
    {
        // Arrange
        var lecturerId = Guid.NewGuid();
        var apiMock = new Mock<IApiService>();
        var loggerMock = new Mock<ILogger<AdminService>>();
        var response = new HttpResponseMessage((HttpStatusCode)statusCode);

        apiMock
            .Setup(x => x.PostAsync(
                $"api/admin/lecturers/{lecturerId}/suspend",
                It.IsAny<StringContent>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        var service = new AdminService(apiMock.Object, loggerMock.Object);

        // Act
        var result = await service.SuspendLecturerAsync(lecturerId);

        // Assert
        Assert.AreEqual(expectedResult, result);
    }

    /// <summary>
    /// Tests that ActivateStudentAsync returns true when the API call succeeds.
    /// Input: Valid Guid and default CancellationToken.
    /// Expected: Returns true when API response indicates success.
    /// </summary>
    [TestMethod]
    public async Task ActivateStudentAsync_ValidIdAndSuccessResponse_ReturnsTrue()
    {
        // Arrange
        var studentId = Guid.NewGuid();
        var mockApi = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<AdminService>>();
        var mockResponse = new Mock<HttpResponseMessage>();
        mockResponse.Setup(r => r.IsSuccessStatusCode).Returns(true);

        mockApi.Setup(a => a.PostAsync(
            It.Is<string>(path => path == $"api/admin/students/{studentId}/activate"),
            It.IsAny<HttpContent>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockResponse.Object);

        var service = new AdminService(mockApi.Object, mockLogger.Object);

        // Act
        var result = await service.ActivateStudentAsync(studentId);

        // Assert
        Assert.IsTrue(result);
        mockApi.Verify(a => a.PostAsync(
            $"api/admin/students/{studentId}/activate",
            It.IsAny<HttpContent>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Tests that ActivateStudentAsync returns false when the API call fails.
    /// Input: Valid Guid and default CancellationToken with failing API response.
    /// Expected: Returns false when API response indicates failure.
    /// </summary>
    [TestMethod]
    public async Task ActivateStudentAsync_ValidIdAndFailureResponse_ReturnsFalse()
    {
        // Arrange
        var studentId = Guid.NewGuid();
        var mockApi = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<AdminService>>();
        var mockResponse = new Mock<HttpResponseMessage>();
        mockResponse.Setup(r => r.IsSuccessStatusCode).Returns(false);

        mockApi.Setup(a => a.PostAsync(
            It.IsAny<string>(),
            It.IsAny<HttpContent>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockResponse.Object);

        var service = new AdminService(mockApi.Object, mockLogger.Object);

        // Act
        var result = await service.ActivateStudentAsync(studentId);

        // Assert
        Assert.IsFalse(result);
    }

    /// <summary>
    /// Tests that ActivateStudentAsync handles exceptions and returns false.
    /// Input: Valid Guid with API throwing an exception.
    /// Expected: Returns false and logs the error.
    /// </summary>
    [TestMethod]
    public async Task ActivateStudentAsync_ApiThrowsException_ReturnsFalseAndLogsError()
    {
        // Arrange
        var studentId = Guid.NewGuid();
        var exception = new HttpRequestException("Network error");
        var mockApi = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<AdminService>>();

        mockApi.Setup(a => a.PostAsync(
            It.IsAny<string>(),
            It.IsAny<HttpContent>(),
            It.IsAny<CancellationToken>()))
            .ThrowsAsync(exception);

        var service = new AdminService(mockApi.Object, mockLogger.Object);

        // Act
        var result = await service.ActivateStudentAsync(studentId);

        // Assert
        Assert.IsFalse(result);
        mockLogger.Verify(
            l => l.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains($"api/admin/students/{studentId}/activate")),
                exception,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    /// <summary>
    /// Tests that ActivateStudentAsync properly passes the cancellation token to the API.
    /// Input: Valid Guid and a cancellation token.
    /// Expected: Cancellation token is passed through to the API call.
    /// </summary>
    [TestMethod]
    public async Task ActivateStudentAsync_WithCancellationToken_PassesTokenToApi()
    {
        // Arrange
        var studentId = Guid.NewGuid();
        var cts = new CancellationTokenSource();
        var cancellationToken = cts.Token;
        var mockApi = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<AdminService>>();
        var mockResponse = new Mock<HttpResponseMessage>();
        mockResponse.Setup(r => r.IsSuccessStatusCode).Returns(true);

        mockApi.Setup(a => a.PostAsync(
            It.IsAny<string>(),
            It.IsAny<HttpContent>(),
            cancellationToken))
            .ReturnsAsync(mockResponse.Object);

        var service = new AdminService(mockApi.Object, mockLogger.Object);

        // Act
        var result = await service.ActivateStudentAsync(studentId, cancellationToken);

        // Assert
        Assert.IsTrue(result);
        mockApi.Verify(a => a.PostAsync(
            It.IsAny<string>(),
            It.IsAny<HttpContent>(),
            cancellationToken), Times.Once);
    }

    /// <summary>
    /// Tests that ActivateStudentAsync correctly formats the path with Guid.Empty.
    /// Input: Guid.Empty (all zeros).
    /// Expected: Path is correctly formatted with the empty Guid value.
    /// </summary>
    [TestMethod]
    public async Task ActivateStudentAsync_WithEmptyGuid_FormatsPathCorrectly()
    {
        // Arrange
        var studentId = Guid.Empty;
        var expectedPath = $"api/admin/students/{Guid.Empty}/activate";
        var mockApi = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<AdminService>>();
        var mockResponse = new Mock<HttpResponseMessage>();
        mockResponse.Setup(r => r.IsSuccessStatusCode).Returns(true);

        mockApi.Setup(a => a.PostAsync(
            expectedPath,
            It.IsAny<HttpContent>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockResponse.Object);

        var service = new AdminService(mockApi.Object, mockLogger.Object);

        // Act
        var result = await service.ActivateStudentAsync(studentId);

        // Assert
        Assert.IsTrue(result);
        mockApi.Verify(a => a.PostAsync(
            expectedPath,
            It.IsAny<HttpContent>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Tests that ActivateStudentAsync handles cancellation properly.
    /// Input: Cancelled CancellationToken.
    /// Expected: Cancellation is propagated and caught, returning false.
    /// </summary>
    [TestMethod]
    public async Task ActivateStudentAsync_WithCancelledToken_ReturnsFalse()
    {
        // Arrange
        var studentId = Guid.NewGuid();
        var cts = new CancellationTokenSource();
        cts.Cancel();
        var cancellationToken = cts.Token;
        var mockApi = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<AdminService>>();

        mockApi.Setup(a => a.PostAsync(
            It.IsAny<string>(),
            It.IsAny<HttpContent>(),
            cancellationToken))
            .ThrowsAsync(new OperationCanceledException());

        var service = new AdminService(mockApi.Object, mockLogger.Object);

        // Act
        var result = await service.ActivateStudentAsync(studentId, cancellationToken);

        // Assert
        Assert.IsFalse(result);
    }

    /// <summary>
    /// Tests that ActivateStudentAsync constructs the correct API path format.
    /// Input: Multiple different Guid values.
    /// Expected: Each Guid is correctly formatted in the path "api/admin/students/{id}/activate".
    /// </summary>
    [TestMethod]
    [DataRow("00000000-0000-0000-0000-000000000000")]
    [DataRow("12345678-1234-1234-1234-123456789abc")]
    [DataRow("ffffffff-ffff-ffff-ffff-ffffffffffff")]
    public async Task ActivateStudentAsync_VariousGuids_ConstructsCorrectPath(string guidString)
    {
        // Arrange
        var studentId = Guid.Parse(guidString);
        var expectedPath = $"api/admin/students/{studentId}/activate";
        var mockApi = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<AdminService>>();
        var mockResponse = new Mock<HttpResponseMessage>();
        mockResponse.Setup(r => r.IsSuccessStatusCode).Returns(true);

        mockApi.Setup(a => a.PostAsync(
            expectedPath,
            It.IsAny<HttpContent>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockResponse.Object);

        var service = new AdminService(mockApi.Object, mockLogger.Object);

        // Act
        var result = await service.ActivateStudentAsync(studentId);

        // Assert
        Assert.IsTrue(result);
        mockApi.Verify(a => a.PostAsync(
            expectedPath,
            It.IsAny<HttpContent>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Tests that ActivateLecturerAsync returns false for various HTTP error status codes.
    /// Input: Valid Guid, various HTTP error status codes.
    /// Expected: Returns false for all error codes.
    /// </summary>
    [TestMethod]
    [DataRow(HttpStatusCode.NotFound)]
    [DataRow(HttpStatusCode.InternalServerError)]
    [DataRow(HttpStatusCode.Unauthorized)]
    [DataRow(HttpStatusCode.Forbidden)]
    [DataRow(HttpStatusCode.BadGateway)]
    [DataRow(HttpStatusCode.ServiceUnavailable)]
    public async Task ActivateLecturerAsync_VariousHttpErrorStatusCodes_ReturnsFalse(HttpStatusCode statusCode)
    {
        // Arrange
        var lecturerId = Guid.NewGuid();
        var apiMock = new Mock<IApiService>();
        var loggerMock = new Mock<ILogger<AdminService>>();
        var httpResponse = new HttpResponseMessage(statusCode);

        apiMock.Setup(x => x.PostAsync(
            $"api/admin/lecturers/{lecturerId}/activate",
            It.IsAny<StringContent>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(httpResponse);

        var service = new AdminService(apiMock.Object, loggerMock.Object);

        // Act
        var result = await service.ActivateLecturerAsync(lecturerId);

        // Assert
        Assert.IsFalse(result);
    }

    /// <summary>
    /// Tests that ActivateLecturerAsync returns true for various HTTP success status codes.
    /// Input: Valid Guid, various HTTP success status codes.
    /// Expected: Returns true for all success codes.
    /// </summary>
    [TestMethod]
    [DataRow(HttpStatusCode.OK)]
    [DataRow(HttpStatusCode.Created)]
    [DataRow(HttpStatusCode.Accepted)]
    [DataRow(HttpStatusCode.NoContent)]
    public async Task ActivateLecturerAsync_VariousHttpSuccessStatusCodes_ReturnsTrue(HttpStatusCode statusCode)
    {
        // Arrange
        var lecturerId = Guid.NewGuid();
        var apiMock = new Mock<IApiService>();
        var loggerMock = new Mock<ILogger<AdminService>>();
        var httpResponse = new HttpResponseMessage(statusCode);

        apiMock.Setup(x => x.PostAsync(
            $"api/admin/lecturers/{lecturerId}/activate",
            It.IsAny<StringContent>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(httpResponse);

        var service = new AdminService(apiMock.Object, loggerMock.Object);

        // Act
        var result = await service.ActivateLecturerAsync(lecturerId);

        // Assert
        Assert.IsTrue(result);
    }

    /// <summary>
    /// Tests that DeleteLecturerAsync returns true when the API call succeeds with a valid random Guid.
    /// Input: Valid random Guid.
    /// Expected: Returns true when HTTP response has success status code.
    /// </summary>
    [TestMethod]
    public async Task DeleteLecturerAsync_ValidRandomGuid_ReturnsTrue()
    {
        // Arrange
        var lecturerId = Guid.NewGuid();
        var mockApiService = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<AdminService>>();
        var expectedPath = $"api/admin/lecturers/{lecturerId}/delete";
        var successResponse = new HttpResponseMessage(HttpStatusCode.OK);

        mockApiService
            .Setup(api => api.PostAsync(
                expectedPath,
                It.Is<StringContent>(c => c.Headers.ContentType != null && c.Headers.ContentType.MediaType == "application/json"),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(successResponse);

        var service = new AdminService(mockApiService.Object, mockLogger.Object);

        // Act
        var result = await service.DeleteLecturerAsync(lecturerId);

        // Assert
        Assert.IsTrue(result);
        mockApiService.Verify(api => api.PostAsync(
            expectedPath,
            It.Is<StringContent>(c => c.Headers.ContentType != null && c.Headers.ContentType.MediaType == "application/json"),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Tests that DeleteLecturerAsync returns false when the API returns a non-success HTTP status code.
    /// Input: Valid Guid with API returning BadRequest status code.
    /// Expected: Returns false.
    /// </summary>
    [TestMethod]
    public async Task DeleteLecturerAsync_ApiBadRequest_ReturnsFalse()
    {
        // Arrange
        var lecturerId = Guid.NewGuid();
        var mockApiService = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<AdminService>>();
        var expectedPath = $"api/admin/lecturers/{lecturerId}/delete";
        var failureResponse = new HttpResponseMessage(HttpStatusCode.BadRequest);

        mockApiService
            .Setup(api => api.PostAsync(
                expectedPath,
                It.Is<StringContent>(c => c.Headers.ContentType != null && c.Headers.ContentType.MediaType == "application/json"),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(failureResponse);

        var service = new AdminService(mockApiService.Object, mockLogger.Object);

        // Act
        var result = await service.DeleteLecturerAsync(lecturerId);

        // Assert
        Assert.IsFalse(result);
    }

    /// <summary>
    /// Tests that DeleteLecturerAsync correctly handles Guid.Empty.
    /// Input: Guid.Empty.
    /// Expected: Calls API with correct path containing empty Guid and returns true on success.
    /// </summary>
    [TestMethod]
    public async Task DeleteLecturerAsync_EmptyGuid_CallsApiWithCorrectPath()
    {
        // Arrange
        var emptyGuid = Guid.Empty;
        var mockApiService = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<AdminService>>();
        var expectedPath = $"api/admin/lecturers/{emptyGuid}/delete";
        var successResponse = new HttpResponseMessage(HttpStatusCode.OK);

        mockApiService
            .Setup(api => api.PostAsync(
                expectedPath,
                It.Is<StringContent>(c => c.Headers.ContentType != null && c.Headers.ContentType.MediaType == "application/json"),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(successResponse);

        var service = new AdminService(mockApiService.Object, mockLogger.Object);

        // Act
        var result = await service.DeleteLecturerAsync(emptyGuid);

        // Assert
        Assert.IsTrue(result);
        mockApiService.Verify(api => api.PostAsync(
            expectedPath,
            It.Is<StringContent>(c => c.Headers.ContentType != null && c.Headers.ContentType.MediaType == "application/json"),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Tests that DeleteLecturerAsync correctly passes an explicit CancellationToken to the API call.
    /// Input: Valid Guid and explicit CancellationToken.
    /// Expected: CancellationToken is passed to PostAsync and returns true on success.
    /// </summary>
    [TestMethod]
    public async Task DeleteLecturerAsync_WithExplicitCancellationToken_PassesTokenCorrectly()
    {
        // Arrange
        var lecturerId = Guid.NewGuid();
        var cancellationToken = new CancellationToken();
        var mockApiService = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<AdminService>>();
        var expectedPath = $"api/admin/lecturers/{lecturerId}/delete";
        var successResponse = new HttpResponseMessage(HttpStatusCode.OK);

        mockApiService
            .Setup(api => api.PostAsync(
                expectedPath,
                It.Is<StringContent>(c => c.Headers.ContentType != null && c.Headers.ContentType.MediaType == "application/json"),
                cancellationToken))
            .ReturnsAsync(successResponse);

        var service = new AdminService(mockApiService.Object, mockLogger.Object);

        // Act
        var result = await service.DeleteLecturerAsync(lecturerId, cancellationToken);

        // Assert
        Assert.IsTrue(result);
    }

    /// <summary>
    /// Tests that DeleteLecturerAsync returns false when the API throws an exception.
    /// Input: Valid Guid with API throwing HttpRequestException.
    /// Expected: Returns false and logs error.
    /// </summary>
    [TestMethod]
    public async Task DeleteLecturerAsync_ApiThrowsHttpRequestException_ReturnsFalse()
    {
        // Arrange
        var lecturerId = Guid.NewGuid();
        var mockApiService = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<AdminService>>();
        var expectedPath = $"api/admin/lecturers/{lecturerId}/delete";
        var exception = new HttpRequestException("Network error");

        mockApiService
            .Setup(api => api.PostAsync(
                expectedPath,
                It.Is<StringContent>(c => c.Headers.ContentType != null && c.Headers.ContentType.MediaType == "application/json"),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(exception);

        var service = new AdminService(mockApiService.Object, mockLogger.Object);

        // Act
        var result = await service.DeleteLecturerAsync(lecturerId);

        // Assert
        Assert.IsFalse(result);
    }

    /// <summary>
    /// Tests that DeleteLecturerAsync returns false when operation is cancelled.
    /// Input: Valid Guid with already cancelled CancellationToken.
    /// Expected: Returns false and logs error.
    /// </summary>
    [TestMethod]
    public async Task DeleteLecturerAsync_WithCancelledToken_ReturnsFalse()
    {
        // Arrange
        var lecturerId = Guid.NewGuid();
        var mockApiService = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<AdminService>>();
        var expectedPath = $"api/admin/lecturers/{lecturerId}/delete";
        var cts = new CancellationTokenSource();
        cts.Cancel();

        mockApiService
            .Setup(api => api.PostAsync(
                expectedPath,
                It.Is<StringContent>(c => c.Headers.ContentType != null && c.Headers.ContentType.MediaType == "application/json"),
                cts.Token))
            .ThrowsAsync(new OperationCanceledException());

        var service = new AdminService(mockApiService.Object, mockLogger.Object);

        // Act
        var result = await service.DeleteLecturerAsync(lecturerId, cts.Token);

        // Assert
        Assert.IsFalse(result);
    }

    /// <summary>
    /// Tests that DeleteLecturerAsync returns false for various HTTP error status codes.
    /// Input: Valid Guid with various HTTP error status codes from API.
    /// Expected: Returns false for all error status codes.
    /// </summary>
    /// <param name="statusCode">The HTTP status code to test.</param>
    [TestMethod]
    [DataRow((int)HttpStatusCode.BadRequest)]
    [DataRow((int)HttpStatusCode.Unauthorized)]
    [DataRow((int)HttpStatusCode.Forbidden)]
    [DataRow((int)HttpStatusCode.NotFound)]
    [DataRow((int)HttpStatusCode.InternalServerError)]
    [DataRow((int)HttpStatusCode.BadGateway)]
    [DataRow((int)HttpStatusCode.ServiceUnavailable)]
    [DataRow((int)HttpStatusCode.GatewayTimeout)]
    public async Task DeleteLecturerAsync_VariousErrorStatusCodes_ReturnsFalse(int statusCode)
    {
        // Arrange
        var lecturerId = Guid.NewGuid();
        var mockApiService = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<AdminService>>();
        var expectedPath = $"api/admin/lecturers/{lecturerId}/delete";
        var failureResponse = new HttpResponseMessage((HttpStatusCode)statusCode);

        mockApiService
            .Setup(api => api.PostAsync(
                expectedPath,
                It.Is<StringContent>(c => c.Headers.ContentType != null && c.Headers.ContentType.MediaType == "application/json"),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(failureResponse);

        var service = new AdminService(mockApiService.Object, mockLogger.Object);

        // Act
        var result = await service.DeleteLecturerAsync(lecturerId);

        // Assert
        Assert.IsFalse(result);
    }

    /// <summary>
    /// Tests that DeleteLecturerAsync returns true for various HTTP success status codes.
    /// Input: Valid Guid with various HTTP success status codes from API.
    /// Expected: Returns true for all success status codes.
    /// </summary>
    /// <param name="statusCode">The HTTP success status code to test.</param>
    [TestMethod]
    [DataRow((int)HttpStatusCode.OK)]
    [DataRow((int)HttpStatusCode.Created)]
    [DataRow((int)HttpStatusCode.Accepted)]
    [DataRow((int)HttpStatusCode.NoContent)]
    public async Task DeleteLecturerAsync_VariousSuccessStatusCodes_ReturnsTrue(int statusCode)
    {
        // Arrange
        var lecturerId = Guid.NewGuid();
        var mockApiService = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<AdminService>>();
        var expectedPath = $"api/admin/lecturers/{lecturerId}/delete";
        var successResponse = new HttpResponseMessage((HttpStatusCode)statusCode);

        mockApiService
            .Setup(api => api.PostAsync(
                expectedPath,
                It.Is<StringContent>(c => c.Headers.ContentType != null && c.Headers.ContentType.MediaType == "application/json"),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(successResponse);

        var service = new AdminService(mockApiService.Object, mockLogger.Object);

        // Act
        var result = await service.DeleteLecturerAsync(lecturerId);

        // Assert
        Assert.IsTrue(result);
    }

    /// <summary>
    /// Tests that DeleteLecturerAsync constructs the correct path for various Guid values.
    /// Input: Various specific Guid values.
    /// Expected: Correct API path is constructed for each Guid.
    /// </summary>
    /// <param name="guidString">The GUID string to test.</param>
    [TestMethod]
    [DataRow("00000000-0000-0000-0000-000000000001")]
    [DataRow("ffffffff-ffff-ffff-ffff-ffffffffffff")]
    [DataRow("12345678-1234-5678-1234-567812345678")]
    [DataRow("a1b2c3d4-e5f6-7890-abcd-ef1234567890")]
    public async Task DeleteLecturerAsync_VariousGuids_ConstructsCorrectPath(string guidString)
    {
        // Arrange
        var lecturerId = Guid.Parse(guidString);
        var mockApiService = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<AdminService>>();
        var expectedPath = $"api/admin/lecturers/{lecturerId}/delete";
        var successResponse = new HttpResponseMessage(HttpStatusCode.OK);

        mockApiService
            .Setup(api => api.PostAsync(
                expectedPath,
                It.Is<StringContent>(c => c.Headers.ContentType != null && c.Headers.ContentType.MediaType == "application/json"),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(successResponse);

        var service = new AdminService(mockApiService.Object, mockLogger.Object);

        // Act
        var result = await service.DeleteLecturerAsync(lecturerId);

        // Assert
        Assert.IsTrue(result);
        mockApiService.Verify(api => api.PostAsync(
            expectedPath,
            It.Is<StringContent>(c => c.Headers.ContentType != null && c.Headers.ContentType.MediaType == "application/json"),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Tests that DeleteLecturerAsync uses default CancellationToken when none is provided.
    /// Input: Valid Guid without explicit CancellationToken.
    /// Expected: Method executes successfully with default token.
    /// </summary>
    [TestMethod]
    public async Task DeleteLecturerAsync_WithoutExplicitToken_UsesDefaultToken()
    {
        // Arrange
        var lecturerId = Guid.NewGuid();
        var mockApiService = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<AdminService>>();
        var expectedPath = $"api/admin/lecturers/{lecturerId}/delete";
        var successResponse = new HttpResponseMessage(HttpStatusCode.OK);

        mockApiService
            .Setup(api => api.PostAsync(
                expectedPath,
                It.Is<StringContent>(c => c.Headers.ContentType != null && c.Headers.ContentType.MediaType == "application/json"),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(successResponse);

        var service = new AdminService(mockApiService.Object, mockLogger.Object);

        // Act
        var result = await service.DeleteLecturerAsync(lecturerId);

        // Assert
        Assert.IsTrue(result);
        mockApiService.Verify(api => api.PostAsync(
            It.IsAny<string>(),
            It.IsAny<StringContent>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Tests that DeleteLecturerAsync returns false when the API throws a generic Exception.
    /// Input: Valid Guid with API throwing a generic Exception.
    /// Expected: Returns false and exception is caught and logged.
    /// </summary>
    [TestMethod]
    public async Task DeleteLecturerAsync_ApiThrowsGenericException_ReturnsFalse()
    {
        // Arrange
        var lecturerId = Guid.NewGuid();
        var mockApiService = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<AdminService>>();
        var expectedPath = $"api/admin/lecturers/{lecturerId}/delete";
        var exception = new Exception("Unexpected error");

        mockApiService
            .Setup(api => api.PostAsync(
                expectedPath,
                It.Is<StringContent>(c => c.Headers.ContentType != null && c.Headers.ContentType.MediaType == "application/json"),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(exception);

        var service = new AdminService(mockApiService.Object, mockLogger.Object);

        // Act
        var result = await service.DeleteLecturerAsync(lecturerId);

        // Assert
        Assert.IsFalse(result);
    }

    /// <summary>
    /// Tests that DeleteLecturerAsync returns false when the API throws a TaskCanceledException.
    /// Input: Valid Guid with API throwing TaskCanceledException.
    /// Expected: Returns false and exception is caught and logged.
    /// </summary>
    [TestMethod]
    public async Task DeleteLecturerAsync_ApiThrowsTaskCanceledException_ReturnsFalse()
    {
        // Arrange
        var lecturerId = Guid.NewGuid();
        var mockApiService = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<AdminService>>();
        var expectedPath = $"api/admin/lecturers/{lecturerId}/delete";
        var exception = new TaskCanceledException("Request timeout");

        mockApiService
            .Setup(api => api.PostAsync(
                expectedPath,
                It.Is<StringContent>(c => c.Headers.ContentType != null && c.Headers.ContentType.MediaType == "application/json"),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(exception);

        var service = new AdminService(mockApiService.Object, mockLogger.Object);

        // Act
        var result = await service.DeleteLecturerAsync(lecturerId);

        // Assert
        Assert.IsFalse(result);
    }

    /// <summary>
    /// Tests that SuspendLecturerAsync returns false when the API responds with a failure status code.
    /// Input: Valid lecturer ID with various failure HTTP status codes.
    /// Expected: Returns false for non-success status codes.
    /// </summary>
    [TestMethod]
    [DataRow((int)HttpStatusCode.BadRequest)]
    [DataRow((int)HttpStatusCode.Unauthorized)]
    [DataRow((int)HttpStatusCode.Forbidden)]
    [DataRow((int)HttpStatusCode.NotFound)]
    [DataRow((int)HttpStatusCode.InternalServerError)]
    [DataRow((int)HttpStatusCode.BadGateway)]
    [DataRow((int)HttpStatusCode.ServiceUnavailable)]
    public async Task SuspendLecturerAsync_VariousFailureStatusCodes_ReturnsFalse(int statusCode)
    {
        // Arrange
        var lecturerId = Guid.NewGuid();
        var apiMock = new Mock<IApiService>();
        var loggerMock = new Mock<ILogger<AdminService>>();
        var failureResponse = new HttpResponseMessage((HttpStatusCode)statusCode);

        apiMock
            .Setup(x => x.PostAsync(
                It.IsAny<string>(),
                It.IsAny<StringContent>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(failureResponse);

        var service = new AdminService(apiMock.Object, loggerMock.Object);

        // Act
        var result = await service.SuspendLecturerAsync(lecturerId);

        // Assert
        Assert.IsFalse(result);
    }

    /// <summary>
    /// Tests that SuspendLecturerAsync returns false and logs an error when the API throws an exception.
    /// Input: Valid lecturer ID with API throwing HttpRequestException.
    /// Expected: Returns false and logs the exception.
    /// </summary>
    [TestMethod]
    public async Task SuspendLecturerAsync_ApiThrowsHttpRequestException_ReturnsFalseAndLogsError()
    {
        // Arrange
        var lecturerId = Guid.NewGuid();
        var apiMock = new Mock<IApiService>();
        var loggerMock = new Mock<ILogger<AdminService>>();
        var expectedException = new HttpRequestException("Network error");

        apiMock
            .Setup(x => x.PostAsync(
                $"api/admin/lecturers/{lecturerId}/suspend",
                It.IsAny<StringContent>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(expectedException);

        var service = new AdminService(apiMock.Object, loggerMock.Object);

        // Act
        var result = await service.SuspendLecturerAsync(lecturerId);

        // Assert
        Assert.IsFalse(result);
        loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => true),
                expectedException,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    /// <summary>
    /// Tests that SuspendLecturerAsync works correctly with Guid.Empty.
    /// Input: Guid.Empty (all zeros).
    /// Expected: Constructs path with empty GUID and returns true on success.
    /// </summary>
    [TestMethod]
    public async Task SuspendLecturerAsync_EmptyGuid_ConstructsCorrectPathAndReturnsTrue()
    {
        // Arrange
        var lecturerId = Guid.Empty;
        var apiMock = new Mock<IApiService>();
        var loggerMock = new Mock<ILogger<AdminService>>();
        var successResponse = new HttpResponseMessage(HttpStatusCode.OK);

        apiMock
            .Setup(x => x.PostAsync(
                $"api/admin/lecturers/{Guid.Empty}/suspend",
                It.IsAny<StringContent>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(successResponse);

        var service = new AdminService(apiMock.Object, loggerMock.Object);

        // Act
        var result = await service.SuspendLecturerAsync(lecturerId);

        // Assert
        Assert.IsTrue(result);
        apiMock.Verify(x => x.PostAsync(
            $"api/admin/lecturers/{Guid.Empty}/suspend",
            It.IsAny<StringContent>(),
            It.IsAny<CancellationToken>()),
            Times.Once);
    }

    /// <summary>
    /// Tests that SuspendLecturerAsync handles OperationCanceledException when cancellation is requested.
    /// Input: Valid lecturer ID with an already cancelled token.
    /// Expected: Returns false when operation is cancelled.
    /// </summary>
    [TestMethod]
    public async Task SuspendLecturerAsync_WithCancelledToken_ReturnsFalse()
    {
        // Arrange
        var lecturerId = Guid.NewGuid();
        var cancellationTokenSource = new CancellationTokenSource();
        cancellationTokenSource.Cancel();
        var apiMock = new Mock<IApiService>();
        var loggerMock = new Mock<ILogger<AdminService>>();

        apiMock
            .Setup(x => x.PostAsync(
                It.IsAny<string>(),
                It.IsAny<StringContent>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new OperationCanceledException());

        var service = new AdminService(apiMock.Object, loggerMock.Object);

        // Act
        var result = await service.SuspendLecturerAsync(lecturerId, cancellationTokenSource.Token);

        // Assert
        Assert.IsFalse(result);
    }

    /// <summary>
    /// Tests that SuspendLecturerAsync constructs the correct API path with various Guid values.
    /// Input: Different Guid values.
    /// Expected: Each Guid is correctly formatted in the path "api/admin/lecturers/{id}/suspend".
    /// </summary>
    [TestMethod]
    [DataRow("00000000-0000-0000-0000-000000000001")]
    [DataRow("12345678-1234-1234-1234-123456789abc")]
    [DataRow("ffffffff-ffff-ffff-ffff-ffffffffffff")]
    [DataRow("a1b2c3d4-e5f6-7890-abcd-ef0123456789")]
    public async Task SuspendLecturerAsync_VariousGuids_ConstructsCorrectPath(string guidString)
    {
        // Arrange
        var lecturerId = Guid.Parse(guidString);
        var apiMock = new Mock<IApiService>();
        var loggerMock = new Mock<ILogger<AdminService>>();
        var successResponse = new HttpResponseMessage(HttpStatusCode.OK);

        apiMock
            .Setup(x => x.PostAsync(
                $"api/admin/lecturers/{lecturerId}/suspend",
                It.IsAny<StringContent>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(successResponse);

        var service = new AdminService(apiMock.Object, loggerMock.Object);

        // Act
        var result = await service.SuspendLecturerAsync(lecturerId);

        // Assert
        Assert.IsTrue(result);
        apiMock.Verify(x => x.PostAsync(
            $"api/admin/lecturers/{lecturerId}/suspend",
            It.IsAny<StringContent>(),
            It.IsAny<CancellationToken>()),
            Times.Once);
    }

    /// <summary>
    /// Tests that SuspendLecturerAsync uses default cancellation token when none is provided.
    /// Input: Valid lecturer ID without explicit cancellation token.
    /// Expected: Method calls PostAsync successfully with default cancellation token.
    /// </summary>
    [TestMethod]
    public async Task SuspendLecturerAsync_NoCancellationToken_UsesDefaultToken()
    {
        // Arrange
        var lecturerId = Guid.NewGuid();
        var apiMock = new Mock<IApiService>();
        var loggerMock = new Mock<ILogger<AdminService>>();
        var successResponse = new HttpResponseMessage(HttpStatusCode.OK);

        apiMock
            .Setup(x => x.PostAsync(
                It.IsAny<string>(),
                It.IsAny<StringContent>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(successResponse);

        var service = new AdminService(apiMock.Object, loggerMock.Object);

        // Act
        var result = await service.SuspendLecturerAsync(lecturerId);

        // Assert
        Assert.IsTrue(result);
        apiMock.Verify(x => x.PostAsync(
            $"api/admin/lecturers/{lecturerId}/suspend",
            It.IsAny<StringContent>(),
            It.IsAny<CancellationToken>()),
            Times.Once);
    }

    /// <summary>
    /// Tests that SuspendLecturerAsync handles different HTTP success status codes correctly.
    /// Input: Valid lecturer ID with various success status codes.
    /// Expected: Returns true for all 2xx status codes.
    /// </summary>
    [TestMethod]
    [DataRow((int)HttpStatusCode.OK)]
    [DataRow((int)HttpStatusCode.Created)]
    [DataRow((int)HttpStatusCode.Accepted)]
    [DataRow((int)HttpStatusCode.NoContent)]
    public async Task SuspendLecturerAsync_VariousSuccessStatusCodes_ReturnsTrue(int statusCode)
    {
        // Arrange
        var lecturerId = Guid.NewGuid();
        var apiMock = new Mock<IApiService>();
        var loggerMock = new Mock<ILogger<AdminService>>();
        var successResponse = new HttpResponseMessage((HttpStatusCode)statusCode);

        apiMock
            .Setup(x => x.PostAsync(
                It.IsAny<string>(),
                It.IsAny<StringContent>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(successResponse);

        var service = new AdminService(apiMock.Object, loggerMock.Object);

        // Act
        var result = await service.SuspendLecturerAsync(lecturerId);

        // Assert
        Assert.IsTrue(result);
    }

    /// <summary>
    /// Tests that SuspendStudentAsync returns true for various success status codes.
    /// Input: Valid student ID with different 2xx status codes.
    /// Expected: Method returns true for all success status codes.
    /// </summary>
    [TestMethod]
    [DataRow(HttpStatusCode.OK)]
    [DataRow(HttpStatusCode.Created)]
    [DataRow(HttpStatusCode.Accepted)]
    [DataRow(HttpStatusCode.NoContent)]
    public async Task SuspendStudentAsync_VariousSuccessStatusCodes_ReturnsTrue(HttpStatusCode statusCode)
    {
        // Arrange
        var studentId = Guid.NewGuid();
        var mockApiService = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<AdminService>>();
        var successResponse = new HttpResponseMessage(statusCode);

        mockApiService
            .Setup(x => x.PostAsync(
                It.IsAny<string>(),
                It.IsAny<StringContent>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(successResponse);

        var service = new AdminService(mockApiService.Object, mockLogger.Object);

        // Act
        var result = await service.SuspendStudentAsync(studentId);

        // Assert
        Assert.IsTrue(result);
    }

    /// <summary>
    /// Tests that SuspendStudentAsync constructs the correct API path format.
    /// Input: Valid student ID.
    /// Expected: API path follows the format "api/admin/students/{id}/suspend".
    /// </summary>
    [TestMethod]
    public async Task SuspendStudentAsync_ValidId_ConstructsCorrectApiPath()
    {
        // Arrange
        var studentId = Guid.NewGuid();
        var expectedPath = $"api/admin/students/{studentId}/suspend";
        var mockApiService = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<AdminService>>();
        var successResponse = new HttpResponseMessage(HttpStatusCode.OK);

        mockApiService
            .Setup(x => x.PostAsync(
                expectedPath,
                It.IsAny<StringContent>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(successResponse);

        var service = new AdminService(mockApiService.Object, mockLogger.Object);

        // Act
        var result = await service.SuspendStudentAsync(studentId);

        // Assert
        Assert.IsTrue(result);
        mockApiService.Verify(
            x => x.PostAsync(
                expectedPath,
                It.IsAny<StringContent>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    /// <summary>
    /// Tests that SuspendStudentAsync passes an empty JSON StringContent to the API.
    /// Input: Valid student ID.
    /// Expected: PostAsync is called with a StringContent having empty JSON body.
    /// </summary>
    [TestMethod]
    public async Task SuspendStudentAsync_ValidId_PassesEmptyJsonContent()
    {
        // Arrange
        var studentId = Guid.NewGuid();
        var mockApiService = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<AdminService>>();
        var successResponse = new HttpResponseMessage(HttpStatusCode.OK);
        StringContent? capturedContent = null;

        mockApiService
            .Setup(x => x.PostAsync(
                It.IsAny<string>(),
                It.IsAny<StringContent>(),
                It.IsAny<CancellationToken>()))
            .Callback<string, StringContent, CancellationToken>((path, content, ct) => capturedContent = content)
            .ReturnsAsync(successResponse);

        var service = new AdminService(mockApiService.Object, mockLogger.Object);

        // Act
        var result = await service.SuspendStudentAsync(studentId);

        // Assert
        Assert.IsTrue(result);
        Assert.IsNotNull(capturedContent);
    }

    /// <summary>
    /// Tests that SuspendStudentAsync handles TaskCanceledException properly.
    /// Input: Valid student ID with TaskCanceledException thrown by API.
    /// Expected: Method returns false and logs the error.
    /// </summary>
    [TestMethod]
    public async Task SuspendStudentAsync_TaskCanceledException_ReturnsFalseAndLogsError()
    {
        // Arrange
        var studentId = Guid.NewGuid();
        var mockApiService = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<AdminService>>();
        var expectedException = new TaskCanceledException("Request was cancelled");

        mockApiService
            .Setup(x => x.PostAsync(
                It.IsAny<string>(),
                It.IsAny<StringContent>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(expectedException);

        var service = new AdminService(mockApiService.Object, mockLogger.Object);

        // Act
        var result = await service.SuspendStudentAsync(studentId);

        // Assert
        Assert.IsFalse(result);
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => true),
                expectedException,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    /// <summary>
    /// Tests that GetStudentsAsync returns empty list when the API returns an empty JSON object array with whitespace.
    /// Input: API returns JSON with whitespace and empty array.
    /// Expected: Returns empty list.
    /// </summary>
    [TestMethod]
    public async Task GetStudentsAsync_EmptyArrayWithWhitespace_ReturnsEmptyList()
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<AdminService>>();

        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("  [  ]  ", Encoding.UTF8, "application/json")
        };

        mockApi.Setup(api => api.GetAsync("api/admin/students", It.IsAny<CancellationToken>()))
            .ReturnsAsync(httpResponse);

        var service = new AdminService(mockApi.Object, mockLogger.Object);

        // Act
        var result = await service.GetStudentsAsync();

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(0, result.Count);
    }

    /// <summary>
    /// Tests that GetStudentsAsync handles a large number of students correctly.
    /// Input: API returns a list with 1000 students.
    /// Expected: Returns list with 1000 students.
    /// </summary>
    [TestMethod]
    public async Task GetStudentsAsync_LargeNumberOfStudents_ReturnsCompleteList()
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<AdminService>>();

        var expectedStudents = new List<UserItem>();
        for (int i = 0; i < 1000; i++)
        {
            expectedStudents.Add(new UserItem
            {
                Id = Guid.NewGuid(),
                Name = $"Student {i}",
                Email = $"student{i}@example.com",
                Role = "Student",
                Status = "Active",
                Program = "Engineering",
                IsApproved = true,
                CreatedAt = DateTime.UtcNow
            });
        }

        var jsonContent = JsonSerializer.Serialize(expectedStudents);
        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(jsonContent, Encoding.UTF8, "application/json")
        };

        mockApi.Setup(api => api.GetAsync("api/admin/students", It.IsAny<CancellationToken>()))
            .ReturnsAsync(httpResponse);

        var service = new AdminService(mockApi.Object, mockLogger.Object);

        // Act
        var result = await service.GetStudentsAsync();

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(1000, result.Count);
    }

    /// <summary>
    /// Tests that GetStudentsAsync uses default cancellation token when none is provided.
    /// Input: No cancellation token provided (uses default).
    /// Expected: Method completes successfully with default token.
    /// </summary>
    [TestMethod]
    public async Task GetStudentsAsync_WithoutCancellationToken_UsesDefaultToken()
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<AdminService>>();

        var expectedStudents = new List<UserItem>
        {
            new UserItem
            {
                Id = Guid.NewGuid(),
                Name = "Test Student",
                Email = "test@example.com",
                Role = "Student",
                Status = "Active",
                Program = "Computer Science",
                IsApproved = true,
                CreatedAt = DateTime.UtcNow
            }
        };

        var jsonContent = JsonSerializer.Serialize(expectedStudents);
        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(jsonContent, Encoding.UTF8, "application/json")
        };

        mockApi.Setup(api => api.GetAsync("api/admin/students", It.IsAny<CancellationToken>()))
            .ReturnsAsync(httpResponse);

        var service = new AdminService(mockApi.Object, mockLogger.Object);

        // Act
        var result = await service.GetStudentsAsync();

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(1, result.Count);
        mockApi.Verify(api => api.GetAsync("api/admin/students", It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Tests that GetStudentsAsync handles students with special characters in string properties.
    /// Input: API returns students with special characters, unicode, and control characters.
    /// Expected: Deserializes correctly and returns students with special characters.
    /// </summary>
    [TestMethod]
    public async Task GetStudentsAsync_StudentsWithSpecialCharacters_DeserializesCorrectly()
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<AdminService>>();

        var expectedStudents = new List<UserItem>
        {
            new UserItem
            {
                Id = Guid.NewGuid(),
                Name = "José María O'Brien",
                Email = "josé.maría@example.com",
                Role = "Student",
                Status = "Active",
                Program = "Ingénierie électrique",
                IsApproved = true,
                CreatedAt = DateTime.UtcNow
            },
            new UserItem
            {
                Id = Guid.NewGuid(),
                Name = "李明 (Li Ming)",
                Email = "li.ming@example.com",
                Role = "Student",
                Status = "Active",
                Program = "计算机科学",
                IsApproved = false,
                CreatedAt = DateTime.UtcNow
            }
        };

        var jsonContent = JsonSerializer.Serialize(expectedStudents);
        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(jsonContent, Encoding.UTF8, "application/json")
        };

        mockApi.Setup(api => api.GetAsync("api/admin/students", It.IsAny<CancellationToken>()))
            .ReturnsAsync(httpResponse);

        var service = new AdminService(mockApi.Object, mockLogger.Object);

        // Act
        var result = await service.GetStudentsAsync();

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(2, result.Count);
        Assert.AreEqual("José María O'Brien", result[0].Name);
        Assert.AreEqual("李明 (Li Ming)", result[1].Name);
    }

    /// <summary>
    /// Tests that GetStudentsAsync correctly handles HTTP 204 No Content response.
    /// Input: API returns 204 No Content with empty body.
    /// Expected: Returns empty list.
    /// </summary>
    [TestMethod]
    public async Task GetStudentsAsync_NoContentStatusCode_ReturnsEmptyList()
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<AdminService>>();

        var httpResponse = new HttpResponseMessage(HttpStatusCode.NoContent)
        {
            Content = new StringContent("", Encoding.UTF8, "application/json")
        };

        mockApi.Setup(api => api.GetAsync("api/admin/students", It.IsAny<CancellationToken>()))
            .ReturnsAsync(httpResponse);

        var service = new AdminService(mockApi.Object, mockLogger.Object);

        // Act
        var result = await service.GetStudentsAsync();

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(0, result.Count);
    }

    /// <summary>
    /// Tests that GetLecturersAsync calls the correct API endpoint with the exact path.
    /// Input: Valid request.
    /// Expected: GetAsync is called with "api/admin/lecturers" path exactly.
    /// </summary>
    [TestMethod]
    public async Task GetLecturersAsync_Always_CallsCorrectEndpoint()
    {
        // Arrange
        var jsonContent = JsonSerializer.Serialize(new List<UserItem>());
        var httpContent = new StringContent(jsonContent, Encoding.UTF8, "application/json");
        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = httpContent
        };
        var mockApiService = new Mock<IApiService>();
        mockApiService.Setup(x => x.GetAsync("api/admin/lecturers", It.IsAny<CancellationToken>()))
            .ReturnsAsync(httpResponse);
        var mockLogger = new Mock<ILogger<AdminService>>();
        var adminService = new AdminService(mockApiService.Object, mockLogger.Object);

        // Act
        await adminService.GetLecturersAsync();

        // Assert
        mockApiService.Verify(x => x.GetAsync("api/admin/lecturers", It.IsAny<CancellationToken>()), Times.Once);
        mockApiService.VerifyNoOtherCalls();
    }

    /// <summary>
    /// Tests that GetLecturersAsync uses default CancellationToken when none is provided.
    /// Input: No cancellation token provided (uses default).
    /// Expected: Method completes successfully and GetAsync is called with default token.
    /// </summary>
    [TestMethod]
    public async Task GetLecturersAsync_NoTokenProvided_UsesDefaultCancellationToken()
    {
        // Arrange
        var jsonContent = JsonSerializer.Serialize(new List<UserItem>());
        var httpContent = new StringContent(jsonContent, Encoding.UTF8, "application/json");
        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = httpContent
        };
        var mockApiService = new Mock<IApiService>();
        mockApiService.Setup(x => x.GetAsync("api/admin/lecturers", It.IsAny<CancellationToken>()))
            .ReturnsAsync(httpResponse);
        var mockLogger = new Mock<ILogger<AdminService>>();
        var adminService = new AdminService(mockApiService.Object, mockLogger.Object);

        // Act
        var result = await adminService.GetLecturersAsync();

        // Assert
        Assert.IsNotNull(result);
        mockApiService.Verify(x => x.GetAsync("api/admin/lecturers", It.IsAny<CancellationToken>()), Times.Once);
    }


    /// <summary>
    /// Tests that DeleteLecturerAsync returns true when the API call succeeds with a valid random Guid.
    /// Input: Valid random Guid with successful API response (200 OK).
    /// Expected: Returns true.
    /// </summary>
    [TestMethod]
    public async Task DeleteLecturerAsync_ValidRandomGuidWithSuccessResponse_ReturnsTrue()
    {
        // Arrange
        var lecturerId = Guid.NewGuid();
        var mockApiService = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<AdminService>>();
        var expectedPath = $"api/admin/lecturers/{lecturerId}/delete";
        var successResponse = new HttpResponseMessage(HttpStatusCode.OK);

        mockApiService
            .Setup(api => api.PostAsync(
                expectedPath,
                It.Is<StringContent>(c => c.Headers.ContentType != null && c.Headers.ContentType.MediaType == "application/json"),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(successResponse);

        var service = new AdminService(mockApiService.Object, mockLogger.Object);

        // Act
        var result = await service.DeleteLecturerAsync(lecturerId);

        // Assert
        Assert.IsTrue(result);
        mockApiService.Verify(
            api => api.PostAsync(
                expectedPath,
                It.Is<StringContent>(c => c.Headers.ContentType != null && c.Headers.ContentType.MediaType == "application/json"),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    /// <summary>
    /// Tests that DeleteLecturerAsync returns true when called with an explicit CancellationToken.
    /// Input: Valid Guid with explicit CancellationToken and successful API response.
    /// Expected: Returns true and passes the token to the API call.
    /// </summary>
    [TestMethod]
    public async Task DeleteLecturerAsync_WithExplicitCancellationToken_PassesTokenAndReturnsTrue()
    {
        // Arrange
        var lecturerId = Guid.NewGuid();
        var cancellationToken = new CancellationToken();
        var mockApiService = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<AdminService>>();
        var expectedPath = $"api/admin/lecturers/{lecturerId}/delete";
        var successResponse = new HttpResponseMessage(HttpStatusCode.OK);

        mockApiService
            .Setup(api => api.PostAsync(
                expectedPath,
                It.Is<StringContent>(c => c.Headers.ContentType != null && c.Headers.ContentType.MediaType == "application/json"),
                cancellationToken))
            .ReturnsAsync(successResponse);

        var service = new AdminService(mockApiService.Object, mockLogger.Object);

        // Act
        var result = await service.DeleteLecturerAsync(lecturerId, cancellationToken);

        // Assert
        Assert.IsTrue(result);
        mockApiService.Verify(
            api => api.PostAsync(
                expectedPath,
                It.Is<StringContent>(c => c.Headers.ContentType != null && c.Headers.ContentType.MediaType == "application/json"),
                cancellationToken),
            Times.Once);
    }

    /// <summary>
    /// Tests that DeleteLecturerAsync handles Guid.Empty correctly and constructs the proper path.
    /// Input: Guid.Empty with successful API response.
    /// Expected: Returns true and calls API with correct path containing empty Guid.
    /// </summary>
    [TestMethod]
    public async Task DeleteLecturerAsync_WithEmptyGuid_ConstructsCorrectPathAndReturnsTrue()
    {
        // Arrange
        var emptyId = Guid.Empty;
        var mockApiService = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<AdminService>>();
        var expectedPath = $"api/admin/lecturers/{emptyId}/delete";
        var successResponse = new HttpResponseMessage(HttpStatusCode.OK);

        mockApiService
            .Setup(api => api.PostAsync(
                expectedPath,
                It.Is<StringContent>(c => c.Headers.ContentType != null && c.Headers.ContentType.MediaType == "application/json"),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(successResponse);

        var service = new AdminService(mockApiService.Object, mockLogger.Object);

        // Act
        var result = await service.DeleteLecturerAsync(emptyId);

        // Assert
        Assert.IsTrue(result);
        mockApiService.Verify(
            api => api.PostAsync(
                expectedPath,
                It.IsAny<StringContent>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    /// <summary>
    /// Tests that DeleteLecturerAsync returns false when the API throws an HttpRequestException.
    /// Input: Valid Guid with API throwing HttpRequestException.
    /// Expected: Returns false and logs the error.
    /// </summary>
    [TestMethod]
    public async Task DeleteLecturerAsync_ApiThrowsHttpRequestException_ReturnsFalseAndLogsError()
    {
        // Arrange
        var lecturerId = Guid.NewGuid();
        var mockApiService = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<AdminService>>();
        var expectedException = new HttpRequestException("Network error");

        mockApiService
            .Setup(api => api.PostAsync(
                It.IsAny<string>(),
                It.IsAny<StringContent>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(expectedException);

        var service = new AdminService(mockApiService.Object, mockLogger.Object);

        // Act
        var result = await service.DeleteLecturerAsync(lecturerId);

        // Assert
        Assert.IsFalse(result);
        mockLogger.Verify(
            logger => logger.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => true),
                expectedException,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    /// <summary>
    /// Tests that DeleteLecturerAsync returns false when operation is cancelled.
    /// Input: Valid Guid with already cancelled CancellationToken.
    /// Expected: Returns false and logs the error.
    /// </summary>
    [TestMethod]
    public async Task DeleteLecturerAsync_WithCancelledToken_ReturnsFalseAndLogsError()
    {
        // Arrange
        var lecturerId = Guid.NewGuid();
        var mockApiService = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<AdminService>>();
        var cts = new CancellationTokenSource();
        cts.Cancel();
        var cancelledToken = cts.Token;

        mockApiService
            .Setup(api => api.PostAsync(
                It.IsAny<string>(),
                It.IsAny<StringContent>(),
                cancelledToken))
            .ThrowsAsync(new OperationCanceledException());

        var service = new AdminService(mockApiService.Object, mockLogger.Object);

        // Act
        var result = await service.DeleteLecturerAsync(lecturerId, cancelledToken);

        // Assert
        Assert.IsFalse(result);
    }

    /// <summary>
    /// Tests that DeleteLecturerAsync returns false when the API throws a generic Exception.
    /// Input: Valid Guid with API throwing a generic Exception.
    /// Expected: Returns false and exception is caught and logged.
    /// </summary>
    [TestMethod]
    public async Task DeleteLecturerAsync_ApiThrowsGenericException_ReturnsFalseAndLogsError()
    {
        // Arrange
        var lecturerId = Guid.NewGuid();
        var mockApiService = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<AdminService>>();
        var expectedException = new Exception("Unexpected error");

        mockApiService
            .Setup(api => api.PostAsync(
                It.IsAny<string>(),
                It.IsAny<StringContent>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(expectedException);

        var service = new AdminService(mockApiService.Object, mockLogger.Object);

        // Act
        var result = await service.DeleteLecturerAsync(lecturerId);

        // Assert
        Assert.IsFalse(result);
        mockLogger.Verify(
            logger => logger.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => true),
                expectedException,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    /// <summary>
    /// Tests that DeleteLecturerAsync returns false when the API throws a TaskCanceledException.
    /// Input: Valid Guid with API throwing TaskCanceledException.
    /// Expected: Returns false and exception is caught and logged.
    /// </summary>
    [TestMethod]
    public async Task DeleteLecturerAsync_ApiThrowsTaskCanceledException_ReturnsFalseAndLogsError()
    {
        // Arrange
        var lecturerId = Guid.NewGuid();
        var mockApiService = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<AdminService>>();
        var expectedException = new TaskCanceledException("Task was cancelled");

        mockApiService
            .Setup(api => api.PostAsync(
                It.IsAny<string>(),
                It.IsAny<StringContent>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(expectedException);

        var service = new AdminService(mockApiService.Object, mockLogger.Object);

        // Act
        var result = await service.DeleteLecturerAsync(lecturerId);

        // Assert
        Assert.IsFalse(result);
        mockLogger.Verify(
            logger => logger.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => true),
                expectedException,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    /// <summary>
    /// Tests that SuspendLecturerAsync passes the cancellation token correctly to the API service.
    /// Input: Valid lecturer ID with explicit cancellation token.
    /// Expected: The cancellation token is passed through to PostAsync.
    /// </summary>
    [TestMethod]
    public async Task SuspendLecturerAsync_WithExplicitCancellationToken_PassesTokenToApi()
    {
        // Arrange
        var lecturerId = Guid.NewGuid();
        var cancellationTokenSource = new CancellationTokenSource();
        var cancellationToken = cancellationTokenSource.Token;
        var apiMock = new Mock<IApiService>();
        var loggerMock = new Mock<ILogger<AdminService>>();
        var response = new HttpResponseMessage(HttpStatusCode.OK);

        apiMock
            .Setup(x => x.PostAsync(
                It.IsAny<string>(),
                It.IsAny<StringContent>(),
                cancellationToken))
            .ReturnsAsync(response);

        var service = new AdminService(apiMock.Object, loggerMock.Object);

        // Act
        var result = await service.SuspendLecturerAsync(lecturerId, cancellationToken);

        // Assert
        Assert.IsTrue(result);
        apiMock.Verify(x => x.PostAsync(
            $"api/admin/lecturers/{lecturerId}/suspend",
            It.IsAny<StringContent>(),
            cancellationToken),
            Times.Once);
    }

    /// <summary>
    /// Tests that SuspendLecturerAsync uses default cancellation token when none is provided.
    /// Input: Valid lecturer ID without explicit cancellation token.
    /// Expected: Method calls PostAsync successfully with default cancellation token.
    /// </summary>
    [TestMethod]
    public async Task SuspendLecturerAsync_WithoutExplicitCancellationToken_UsesDefaultToken()
    {
        // Arrange
        var lecturerId = Guid.NewGuid();
        var apiMock = new Mock<IApiService>();
        var loggerMock = new Mock<ILogger<AdminService>>();
        var response = new HttpResponseMessage(HttpStatusCode.OK);

        apiMock
            .Setup(x => x.PostAsync(
                It.IsAny<string>(),
                It.IsAny<StringContent>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        var service = new AdminService(apiMock.Object, loggerMock.Object);

        // Act
        var result = await service.SuspendLecturerAsync(lecturerId);

        // Assert
        Assert.IsTrue(result);
        apiMock.Verify(x => x.PostAsync(
            $"api/admin/lecturers/{lecturerId}/suspend",
            It.IsAny<StringContent>(),
            It.IsAny<CancellationToken>()),
            Times.Once);
    }

    /// <summary>
    /// Tests that SuspendLecturerAsync returns false and logs error when PostAsync throws TaskCanceledException.
    /// Input: Valid lecturer ID with API throwing TaskCanceledException.
    /// Expected: Returns false and logs the exception.
    /// </summary>
    [TestMethod]
    public async Task SuspendLecturerAsync_ApiThrowsTaskCanceledException_ReturnsFalseAndLogsError()
    {
        // Arrange
        var lecturerId = Guid.NewGuid();
        var apiMock = new Mock<IApiService>();
        var loggerMock = new Mock<ILogger<AdminService>>();
        var expectedException = new TaskCanceledException("Request timeout");

        apiMock
            .Setup(x => x.PostAsync(
                It.IsAny<string>(),
                It.IsAny<StringContent>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(expectedException);

        var service = new AdminService(apiMock.Object, loggerMock.Object);

        // Act
        var result = await service.SuspendLecturerAsync(lecturerId);

        // Assert
        Assert.IsFalse(result);
        loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => true),
                expectedException,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    /// <summary>
    /// Tests that SuspendLecturerAsync returns false and logs error when PostAsync throws generic Exception.
    /// Input: Valid lecturer ID with API throwing generic Exception.
    /// Expected: Returns false and logs the exception.
    /// </summary>
    [TestMethod]
    public async Task SuspendLecturerAsync_ApiThrowsGenericException_ReturnsFalseAndLogsError()
    {
        // Arrange
        var lecturerId = Guid.NewGuid();
        var apiMock = new Mock<IApiService>();
        var loggerMock = new Mock<ILogger<AdminService>>();
        var expectedException = new Exception("Unexpected error");

        apiMock
            .Setup(x => x.PostAsync(
                It.IsAny<string>(),
                It.IsAny<StringContent>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(expectedException);

        var service = new AdminService(apiMock.Object, loggerMock.Object);

        // Act
        var result = await service.SuspendLecturerAsync(lecturerId);

        // Assert
        Assert.IsFalse(result);
        loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => true),
                expectedException,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    /// <summary>
    /// Tests that SuspendLecturerAsync verifies the StringContent passed to PostAsync has correct properties.
    /// Input: Valid lecturer ID.
    /// Expected: PostAsync is called with StringContent having empty JSON body, UTF-8 encoding, and application/json media type.
    /// </summary>
    [TestMethod]
    public async Task SuspendLecturerAsync_ValidId_PassesCorrectStringContentToApi()
    {
        // Arrange
        var lecturerId = Guid.NewGuid();
        var apiMock = new Mock<IApiService>();
        var loggerMock = new Mock<ILogger<AdminService>>();
        var response = new HttpResponseMessage(HttpStatusCode.OK);
        StringContent? capturedContent = null;

        apiMock
            .Setup(x => x.PostAsync(
                It.IsAny<string>(),
                It.IsAny<StringContent>(),
                It.IsAny<CancellationToken>()))
            .Callback<string, StringContent, CancellationToken>((path, content, ct) => capturedContent = content)
            .ReturnsAsync(response);

        var service = new AdminService(apiMock.Object, loggerMock.Object);

        // Act
        var result = await service.SuspendLecturerAsync(lecturerId);

        // Assert
        Assert.IsTrue(result);
        Assert.IsNotNull(capturedContent);
        Assert.AreEqual("application/json", capturedContent.Headers.ContentType?.MediaType);
        Assert.AreEqual("utf-8", capturedContent.Headers.ContentType?.CharSet);
    }

    /// <summary>
    /// Tests that ActivateStudentAsync returns false for various HTTP error status codes.
    /// Input: Valid Guid, various HTTP error status codes.
    /// Expected: Returns false for all error codes.
    /// </summary>
    [TestMethod]
    [DataRow(HttpStatusCode.BadRequest)]
    [DataRow(HttpStatusCode.Unauthorized)]
    [DataRow(HttpStatusCode.Forbidden)]
    [DataRow(HttpStatusCode.NotFound)]
    [DataRow(HttpStatusCode.InternalServerError)]
    [DataRow(HttpStatusCode.BadGateway)]
    [DataRow(HttpStatusCode.ServiceUnavailable)]
    [DataRow(HttpStatusCode.GatewayTimeout)]
    public async Task ActivateStudentAsync_VariousHttpErrorStatusCodes_ReturnsFalse(HttpStatusCode statusCode)
    {
        // Arrange
        var studentId = Guid.NewGuid();
        var mockApi = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<AdminService>>();
        var mockResponse = new Mock<HttpResponseMessage>();
        mockResponse.Setup(r => r.IsSuccessStatusCode).Returns(false);
        mockResponse.Setup(r => r.StatusCode).Returns(statusCode);

        mockApi.Setup(a => a.PostAsync(
            It.IsAny<string>(),
            It.IsAny<HttpContent>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockResponse.Object);

        var service = new AdminService(mockApi.Object, mockLogger.Object);

        // Act
        var result = await service.ActivateStudentAsync(studentId);

        // Assert
        Assert.IsFalse(result);
    }

    /// <summary>
    /// Tests that ActivateStudentAsync returns true for various HTTP success status codes.
    /// Input: Valid Guid, various HTTP success status codes.
    /// Expected: Returns true for all success codes.
    /// </summary>
    [TestMethod]
    [DataRow(HttpStatusCode.OK)]
    [DataRow(HttpStatusCode.Created)]
    [DataRow(HttpStatusCode.Accepted)]
    [DataRow(HttpStatusCode.NoContent)]
    public async Task ActivateStudentAsync_VariousHttpSuccessStatusCodes_ReturnsTrue(HttpStatusCode statusCode)
    {
        // Arrange
        var studentId = Guid.NewGuid();
        var mockApi = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<AdminService>>();
        var mockResponse = new Mock<HttpResponseMessage>();
        mockResponse.Setup(r => r.IsSuccessStatusCode).Returns(true);
        mockResponse.Setup(r => r.StatusCode).Returns(statusCode);

        mockApi.Setup(a => a.PostAsync(
            It.IsAny<string>(),
            It.IsAny<HttpContent>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockResponse.Object);

        var service = new AdminService(mockApi.Object, mockLogger.Object);

        // Act
        var result = await service.ActivateStudentAsync(studentId);

        // Assert
        Assert.IsTrue(result);
    }

    /// <summary>
    /// Tests that ActivateStudentAsync uses default cancellation token when none is provided.
    /// Input: Valid Guid without explicit cancellation token.
    /// Expected: Method executes successfully with default token.
    /// </summary>
    [TestMethod]
    public async Task ActivateStudentAsync_WithoutExplicitToken_UsesDefaultToken()
    {
        // Arrange
        var studentId = Guid.NewGuid();
        var mockApi = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<AdminService>>();
        var mockResponse = new Mock<HttpResponseMessage>();
        mockResponse.Setup(r => r.IsSuccessStatusCode).Returns(true);

        mockApi.Setup(a => a.PostAsync(
            It.IsAny<string>(),
            It.IsAny<HttpContent>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockResponse.Object);

        var service = new AdminService(mockApi.Object, mockLogger.Object);

        // Act
        var result = await service.ActivateStudentAsync(studentId);

        // Assert
        Assert.IsTrue(result);
        mockApi.Verify(a => a.PostAsync(
            It.IsAny<string>(),
            It.IsAny<HttpContent>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Tests that ActivateStudentAsync returns false when the API throws a TaskCanceledException.
    /// Input: Valid Guid with API throwing TaskCanceledException.
    /// Expected: Returns false and logs the error.
    /// </summary>
    [TestMethod]
    public async Task ActivateStudentAsync_ApiThrowsTaskCanceledException_ReturnsFalseAndLogsError()
    {
        // Arrange
        var studentId = Guid.NewGuid();
        var exception = new TaskCanceledException("Request timed out");
        var mockApi = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<AdminService>>();

        mockApi.Setup(a => a.PostAsync(
            It.IsAny<string>(),
            It.IsAny<HttpContent>(),
            It.IsAny<CancellationToken>()))
            .ThrowsAsync(exception);

        var service = new AdminService(mockApi.Object, mockLogger.Object);

        // Act
        var result = await service.ActivateStudentAsync(studentId);

        // Assert
        Assert.IsFalse(result);
        mockLogger.Verify(
            l => l.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains($"api/admin/students/{studentId}/activate")),
                exception,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    /// <summary>
    /// Tests that ActivateStudentAsync returns false when the API throws a generic Exception.
    /// Input: Valid Guid with API throwing a generic Exception.
    /// Expected: Returns false and logs the error.
    /// </summary>
    [TestMethod]
    public async Task ActivateStudentAsync_ApiThrowsGenericException_ReturnsFalseAndLogsError()
    {
        // Arrange
        var studentId = Guid.NewGuid();
        var exception = new Exception("Unexpected error");
        var mockApi = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<AdminService>>();

        mockApi.Setup(a => a.PostAsync(
            It.IsAny<string>(),
            It.IsAny<HttpContent>(),
            It.IsAny<CancellationToken>()))
            .ThrowsAsync(exception);

        var service = new AdminService(mockApi.Object, mockLogger.Object);

        // Act
        var result = await service.ActivateStudentAsync(studentId);

        // Assert
        Assert.IsFalse(result);
        mockLogger.Verify(
            l => l.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains($"api/admin/students/{studentId}/activate")),
                exception,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    /// <summary>
    /// Tests that ActivateStudentAsync passes empty JSON content with correct encoding to the API.
    /// Input: Valid Guid.
    /// Expected: PostAsync is called with StringContent having empty JSON body, UTF8 encoding, and application/json media type.
    /// </summary>
    [TestMethod]
    public async Task ActivateStudentAsync_ValidId_PassesEmptyJsonContentWithCorrectEncoding()
    {
        // Arrange
        var studentId = Guid.NewGuid();
        var mockApi = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<AdminService>>();
        var mockResponse = new Mock<HttpResponseMessage>();
        mockResponse.Setup(r => r.IsSuccessStatusCode).Returns(true);
        HttpContent? capturedContent = null;

        mockApi.Setup(a => a.PostAsync(
            It.IsAny<string>(),
            It.IsAny<HttpContent>(),
            It.IsAny<CancellationToken>()))
            .Callback<string, HttpContent, CancellationToken>((path, content, ct) => capturedContent = content)
            .ReturnsAsync(mockResponse.Object);

        var service = new AdminService(mockApi.Object, mockLogger.Object);

        // Act
        var result = await service.ActivateStudentAsync(studentId);

        // Assert
        Assert.IsTrue(result);
        Assert.IsNotNull(capturedContent);
        Assert.IsInstanceOfType(capturedContent, typeof(StringContent));
        var stringContent = capturedContent as StringContent;
        Assert.IsNotNull(stringContent);
        var contentString = await stringContent!.ReadAsStringAsync();
        Assert.AreEqual("", contentString);
        Assert.AreEqual("application/json", stringContent.Headers.ContentType?.MediaType);
    }

    /// <summary>
    /// Tests that ActivateStudentAsync correctly formats the API path with maximum Guid value.
    /// Input: Maximum Guid value (all F's).
    /// Expected: Path is correctly formatted with the maximum Guid value.
    /// </summary>
    [TestMethod]
    public async Task ActivateStudentAsync_MaximumGuidValue_FormatsPathCorrectly()
    {
        // Arrange
        var studentId = new Guid("ffffffff-ffff-ffff-ffff-ffffffffffff");
        var expectedPath = $"api/admin/students/{studentId}/activate";
        var mockApi = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<AdminService>>();
        var mockResponse = new Mock<HttpResponseMessage>();
        mockResponse.Setup(r => r.IsSuccessStatusCode).Returns(true);

        mockApi.Setup(a => a.PostAsync(
            expectedPath,
            It.IsAny<HttpContent>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockResponse.Object);

        var service = new AdminService(mockApi.Object, mockLogger.Object);

        // Act
        var result = await service.ActivateStudentAsync(studentId);

        // Assert
        Assert.IsTrue(result);
        mockApi.Verify(a => a.PostAsync(
            expectedPath,
            It.IsAny<HttpContent>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Tests that SuspendStudentAsync returns true when the API call succeeds with HTTP 200 OK.
    /// Input: Valid student ID and successful API response (200 OK).
    /// Expected: Method returns true and PostAsync is called once with correct parameters.
    /// </summary>
    [TestMethod]
    public async Task SuspendStudentAsync_ValidIdWithOkResponse_ReturnsTrue()
    {
        // Arrange
        var studentId = Guid.NewGuid();
        var mockApiService = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<AdminService>>();
        var successResponse = new HttpResponseMessage(HttpStatusCode.OK);

        mockApiService
            .Setup(x => x.PostAsync(
                $"api/admin/students/{studentId}/suspend",
                It.IsAny<StringContent>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(successResponse);

        var service = new AdminService(mockApiService.Object, mockLogger.Object);

        // Act
        var result = await service.SuspendStudentAsync(studentId);

        // Assert
        Assert.IsTrue(result);
        mockApiService.Verify(
            x => x.PostAsync(
                $"api/admin/students/{studentId}/suspend",
                It.IsAny<StringContent>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    /// <summary>
    /// Tests that SuspendStudentAsync returns false for various HTTP client and server error status codes.
    /// Input: Valid student ID with different HTTP error status codes.
    /// Expected: Method returns false for all error status codes.
    /// </summary>
    [TestMethod]
    [DataRow(HttpStatusCode.BadRequest)]
    [DataRow(HttpStatusCode.Unauthorized)]
    [DataRow(HttpStatusCode.Forbidden)]
    [DataRow(HttpStatusCode.NotFound)]
    [DataRow(HttpStatusCode.MethodNotAllowed)]
    [DataRow(HttpStatusCode.Conflict)]
    [DataRow(HttpStatusCode.InternalServerError)]
    [DataRow(HttpStatusCode.BadGateway)]
    [DataRow(HttpStatusCode.ServiceUnavailable)]
    [DataRow(HttpStatusCode.GatewayTimeout)]
    public async Task SuspendStudentAsync_VariousErrorStatusCodes_ReturnsFalse(HttpStatusCode statusCode)
    {
        // Arrange
        var studentId = Guid.NewGuid();
        var mockApiService = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<AdminService>>();
        var errorResponse = new HttpResponseMessage(statusCode);

        mockApiService
            .Setup(x => x.PostAsync(
                It.IsAny<string>(),
                It.IsAny<StringContent>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(errorResponse);

        var service = new AdminService(mockApiService.Object, mockLogger.Object);

        // Act
        var result = await service.SuspendStudentAsync(studentId);

        // Assert
        Assert.IsFalse(result);
    }

    /// <summary>
    /// Tests that SuspendStudentAsync correctly handles Guid.Empty.
    /// Input: Guid.Empty as student ID.
    /// Expected: Method calls PostAsync with correct path containing empty Guid and returns true on success.
    /// </summary>
    [TestMethod]
    public async Task SuspendStudentAsync_WithEmptyGuid_CallsApiWithCorrectPath()
    {
        // Arrange
        var studentId = Guid.Empty;
        var mockApiService = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<AdminService>>();
        var successResponse = new HttpResponseMessage(HttpStatusCode.OK);

        mockApiService
            .Setup(x => x.PostAsync(
                $"api/admin/students/{Guid.Empty}/suspend",
                It.IsAny<StringContent>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(successResponse);

        var service = new AdminService(mockApiService.Object, mockLogger.Object);

        // Act
        var result = await service.SuspendStudentAsync(studentId);

        // Assert
        Assert.IsTrue(result);
        mockApiService.Verify(
            x => x.PostAsync(
                $"api/admin/students/{Guid.Empty}/suspend",
                It.IsAny<StringContent>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    /// <summary>
    /// Tests that SuspendStudentAsync properly passes an explicit cancellation token to the API service.
    /// Input: Valid student ID with an explicit cancellation token.
    /// Expected: The provided cancellation token is passed to PostAsync.
    /// </summary>
    [TestMethod]
    public async Task SuspendStudentAsync_WithExplicitCancellationToken_PassesTokenToApi()
    {
        // Arrange
        var studentId = Guid.NewGuid();
        var cancellationTokenSource = new CancellationTokenSource();
        var cancellationToken = cancellationTokenSource.Token;
        var mockApiService = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<AdminService>>();
        var successResponse = new HttpResponseMessage(HttpStatusCode.OK);

        mockApiService
            .Setup(x => x.PostAsync(
                It.IsAny<string>(),
                It.IsAny<StringContent>(),
                cancellationToken))
            .ReturnsAsync(successResponse);

        var service = new AdminService(mockApiService.Object, mockLogger.Object);

        // Act
        var result = await service.SuspendStudentAsync(studentId, cancellationToken);

        // Assert
        Assert.IsTrue(result);
        mockApiService.Verify(
            x => x.PostAsync(
                $"api/admin/students/{studentId}/suspend",
                It.IsAny<StringContent>(),
                cancellationToken),
            Times.Once);
    }

    /// <summary>
    /// Tests that SuspendStudentAsync returns false and logs error when PostAsync throws HttpRequestException.
    /// Input: Valid student ID with API throwing HttpRequestException.
    /// Expected: Method returns false and exception is logged with correct path.
    /// </summary>
    [TestMethod]
    public async Task SuspendStudentAsync_ApiThrowsHttpRequestException_ReturnsFalseAndLogsError()
    {
        // Arrange
        var studentId = Guid.NewGuid();
        var expectedException = new HttpRequestException("Network error");
        var mockApiService = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<AdminService>>();

        mockApiService
            .Setup(x => x.PostAsync(
                It.IsAny<string>(),
                It.IsAny<StringContent>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(expectedException);

        var service = new AdminService(mockApiService.Object, mockLogger.Object);

        // Act
        var result = await service.SuspendStudentAsync(studentId);

        // Assert
        Assert.IsFalse(result);
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains($"api/admin/students/{studentId}/suspend")),
                expectedException,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    /// <summary>
    /// Tests that SuspendStudentAsync uses default cancellation token when none is provided.
    /// Input: Valid student ID without explicit cancellation token.
    /// Expected: Method executes successfully with default token.
    /// </summary>
    [TestMethod]
    public async Task SuspendStudentAsync_WithoutExplicitToken_UsesDefaultCancellationToken()
    {
        // Arrange
        var studentId = Guid.NewGuid();
        var mockApiService = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<AdminService>>();
        var successResponse = new HttpResponseMessage(HttpStatusCode.OK);

        mockApiService
            .Setup(x => x.PostAsync(
                It.IsAny<string>(),
                It.IsAny<StringContent>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(successResponse);

        var service = new AdminService(mockApiService.Object, mockLogger.Object);

        // Act
        var result = await service.SuspendStudentAsync(studentId);

        // Assert
        Assert.IsTrue(result);
        mockApiService.Verify(
            x => x.PostAsync(
                It.IsAny<string>(),
                It.IsAny<StringContent>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    /// <summary>
    /// Tests that SuspendStudentAsync constructs correct API paths for various Guid values.
    /// Input: Different Guid values.
    /// Expected: Each Guid is correctly formatted in the path "api/admin/students/{id}/suspend".
    /// </summary>
    [TestMethod]
    [DataRow("00000000-0000-0000-0000-000000000001")]
    [DataRow("ffffffff-ffff-ffff-ffff-ffffffffffff")]
    [DataRow("12345678-1234-1234-1234-123456789abc")]
    [DataRow("a1b2c3d4-e5f6-7890-abcd-ef0123456789")]
    public async Task SuspendStudentAsync_DifferentGuids_ConstructsCorrectApiPath(string guidString)
    {
        // Arrange
        var studentId = Guid.Parse(guidString);
        var mockApiService = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<AdminService>>();
        var successResponse = new HttpResponseMessage(HttpStatusCode.OK);

        mockApiService
            .Setup(x => x.PostAsync(
                $"api/admin/students/{studentId}/suspend",
                It.IsAny<StringContent>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(successResponse);

        var service = new AdminService(mockApiService.Object, mockLogger.Object);

        // Act
        var result = await service.SuspendStudentAsync(studentId);

        // Assert
        Assert.IsTrue(result);
        mockApiService.Verify(
            x => x.PostAsync(
                $"api/admin/students/{studentId}/suspend",
                It.IsAny<StringContent>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    /// <summary>
    /// Tests that SuspendStudentAsync handles OperationCanceledException when cancellation is requested.
    /// Input: Valid student ID with already cancelled cancellation token.
    /// Expected: Returns false when operation is cancelled.
    /// </summary>
    [TestMethod]
    public async Task SuspendStudentAsync_WithCancelledToken_ReturnsFalse()
    {
        // Arrange
        var studentId = Guid.NewGuid();
        var cancellationTokenSource = new CancellationTokenSource();
        cancellationTokenSource.Cancel();
        var mockApiService = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<AdminService>>();

        mockApiService
            .Setup(x => x.PostAsync(
                It.IsAny<string>(),
                It.IsAny<StringContent>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new OperationCanceledException());

        var service = new AdminService(mockApiService.Object, mockLogger.Object);

        // Act
        var result = await service.SuspendStudentAsync(studentId, cancellationTokenSource.Token);

        // Assert
        Assert.IsFalse(result);
    }

    /// <summary>
    /// Tests that SuspendStudentAsync verifies the exact API path format.
    /// Input: Valid student ID.
    /// Expected: API path follows the exact format "api/admin/students/{id}/suspend".
    /// </summary>
    [TestMethod]
    public async Task SuspendStudentAsync_Always_UsesCorrectApiPathFormat()
    {
        // Arrange
        var studentId = Guid.NewGuid();
        var expectedPath = $"api/admin/students/{studentId}/suspend";
        var mockApiService = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<AdminService>>();
        var successResponse = new HttpResponseMessage(HttpStatusCode.OK);

        mockApiService
            .Setup(x => x.PostAsync(
                expectedPath,
                It.IsAny<StringContent>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(successResponse);

        var service = new AdminService(mockApiService.Object, mockLogger.Object);

        // Act
        var result = await service.SuspendStudentAsync(studentId);

        // Assert
        Assert.IsTrue(result);
        mockApiService.Verify(
            x => x.PostAsync(
                expectedPath,
                It.IsAny<StringContent>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    /// <summary>
    /// Tests that SuspendStudentAsync passes empty JSON content to the API.
    /// Input: Valid student ID.
    /// Expected: PostAsync is called with StringContent having empty JSON body, UTF-8 encoding, and application/json media type.
    /// </summary>
    [TestMethod]
    public async Task SuspendStudentAsync_Always_PassesEmptyJsonContent()
    {
        // Arrange
        var studentId = Guid.NewGuid();
        var mockApiService = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<AdminService>>();
        var successResponse = new HttpResponseMessage(HttpStatusCode.OK);
        StringContent? capturedContent = null;

        mockApiService
            .Setup(x => x.PostAsync(
                It.IsAny<string>(),
                It.IsAny<StringContent>(),
                It.IsAny<CancellationToken>()))
            .Callback<string, StringContent, CancellationToken>((path, content, ct) => capturedContent = content)
            .ReturnsAsync(successResponse);

        var service = new AdminService(mockApiService.Object, mockLogger.Object);

        // Act
        var result = await service.SuspendStudentAsync(studentId);

        // Assert
        Assert.IsTrue(result);
        Assert.IsNotNull(capturedContent);
        var contentString = await capturedContent.ReadAsStringAsync();
        Assert.AreEqual("", contentString);
        Assert.AreEqual("application/json", capturedContent.Headers.ContentType?.MediaType);
    }

    /// <summary>
    /// Tests that SuspendStudentAsync handles TaskCanceledException properly.
    /// Input: Valid student ID with TaskCanceledException thrown by API.
    /// Expected: Method returns false and logs the error.
    /// </summary>
    [TestMethod]
    public async Task SuspendStudentAsync_ApiThrowsTaskCanceledException_ReturnsFalseAndLogsError()
    {
        // Arrange
        var studentId = Guid.NewGuid();
        var expectedException = new TaskCanceledException("Task was cancelled");
        var mockApiService = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<AdminService>>();

        mockApiService
            .Setup(x => x.PostAsync(
                It.IsAny<string>(),
                It.IsAny<StringContent>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(expectedException);

        var service = new AdminService(mockApiService.Object, mockLogger.Object);

        // Act
        var result = await service.SuspendStudentAsync(studentId);

        // Assert
        Assert.IsFalse(result);
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => true),
                expectedException,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    /// <summary>
    /// Tests that SuspendStudentAsync handles generic Exception properly.
    /// Input: Valid student ID with generic Exception thrown by API.
    /// Expected: Method returns false and logs the error with correct path.
    /// </summary>
    [TestMethod]
    public async Task SuspendStudentAsync_ApiThrowsGenericException_ReturnsFalseAndLogsError()
    {
        // Arrange
        var studentId = Guid.NewGuid();
        var expectedException = new InvalidOperationException("Unexpected error");
        var mockApiService = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<AdminService>>();

        mockApiService
            .Setup(x => x.PostAsync(
                It.IsAny<string>(),
                It.IsAny<StringContent>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(expectedException);

        var service = new AdminService(mockApiService.Object, mockLogger.Object);

        // Act
        var result = await service.SuspendStudentAsync(studentId);

        // Assert
        Assert.IsFalse(result);
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains($"api/admin/students/{studentId}/suspend")),
                expectedException,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    /// <summary>
    /// Tests that SuspendStudentAsync returns false for 3xx redirect status codes.
    /// Input: Valid student ID with various redirect status codes.
    /// Expected: Method returns false for redirect status codes (not considered success).
    /// </summary>
    [TestMethod]
    [DataRow(HttpStatusCode.MovedPermanently)]
    [DataRow(HttpStatusCode.Found)]
    [DataRow(HttpStatusCode.SeeOther)]
    [DataRow(HttpStatusCode.TemporaryRedirect)]
    public async Task SuspendStudentAsync_RedirectStatusCodes_ReturnsFalse(HttpStatusCode statusCode)
    {
        // Arrange
        var studentId = Guid.NewGuid();
        var mockApiService = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<AdminService>>();
        var redirectResponse = new HttpResponseMessage(statusCode);

        mockApiService
            .Setup(x => x.PostAsync(
                It.IsAny<string>(),
                It.IsAny<StringContent>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(redirectResponse);

        var service = new AdminService(mockApiService.Object, mockLogger.Object);

        // Act
        var result = await service.SuspendStudentAsync(studentId);

        // Assert
        Assert.IsFalse(result);
    }
}