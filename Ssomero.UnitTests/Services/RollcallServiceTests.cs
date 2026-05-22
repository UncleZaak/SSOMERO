using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
/// Unit tests for the RollcallService class.
/// </summary>
[TestClass]
public class RollcallServiceTests
{
    /// <summary>
    /// Tests that GetPendingApprovalsAsync returns rollcall items when API responds with success status code and valid data.
    /// Input: Successful HTTP response (200 OK) with valid RollcallDto collection.
    /// Expected: Returns the deserialized collection of RollcallDto items.
    /// </summary>
    [TestMethod]
    public async Task GetPendingApprovalsAsync_SuccessfulResponseWithValidData_ReturnsRollcallDtos()
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<RollcallService>>();
        var service = new RollcallService(mockApi.Object, mockLogger.Object);

        var expectedRollcalls = new List<RollcallDto>
        {
            new RollcallDto
            {
                Id = "1",
                ScheduleId = "schedule1",
                StudentId = "student1",
                StudentName = "John Doe",
                CourseName = "Math 101",
                SelfieUrl = "https://example.com/selfie1.jpg",
                SubmittedAt = DateTime.UtcNow,
                Status = RollcallStatus.Pending
            },
            new RollcallDto
            {
                Id = "2",
                ScheduleId = "schedule2",
                StudentId = "student2",
                StudentName = "Jane Smith",
                CourseName = "Science 201",
                SelfieUrl = "https://example.com/selfie2.jpg",
                SubmittedAt = DateTime.UtcNow,
                Status = RollcallStatus.Pending
            }
        };

        var responseMessage = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(expectedRollcalls)
        };

        mockApi.Setup(x => x.GetAsync("rollcall/pending", It.IsAny<CancellationToken>()))
            .ReturnsAsync(responseMessage);

        // Act
        var result = await service.GetPendingApprovalsAsync();

        // Assert
        Assert.IsNotNull(result);
        var rollcallList = result.ToList();
        Assert.AreEqual(2, rollcallList.Count);
        Assert.AreEqual("1", rollcallList[0].Id);
        Assert.AreEqual("John Doe", rollcallList[0].StudentName);
        Assert.AreEqual("2", rollcallList[1].Id);
        Assert.AreEqual("Jane Smith", rollcallList[1].StudentName);
    }

    /// <summary>
    /// Tests that GetPendingApprovalsAsync returns empty collection when API responds with success status code but null content.
    /// Input: Successful HTTP response (200 OK) with null content.
    /// Expected: Returns an empty collection.
    /// </summary>
    [TestMethod]
    public async Task GetPendingApprovalsAsync_SuccessfulResponseWithNullContent_ReturnsEmptyCollection()
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<RollcallService>>();
        var service = new RollcallService(mockApi.Object, mockLogger.Object);

        var responseMessage = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create<IEnumerable<RollcallDto>?>(null)
        };

        mockApi.Setup(x => x.GetAsync("rollcall/pending", It.IsAny<CancellationToken>()))
            .ReturnsAsync(responseMessage);

        // Act
        var result = await service.GetPendingApprovalsAsync();

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(0, result.Count());
    }

    /// <summary>
    /// Tests that GetPendingApprovalsAsync returns empty collection when API responds with success status code and empty array.
    /// Input: Successful HTTP response (200 OK) with empty RollcallDto array.
    /// Expected: Returns an empty collection.
    /// </summary>
    [TestMethod]
    public async Task GetPendingApprovalsAsync_SuccessfulResponseWithEmptyArray_ReturnsEmptyCollection()
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<RollcallService>>();
        var service = new RollcallService(mockApi.Object, mockLogger.Object);

        var emptyRollcalls = new List<RollcallDto>();
        var responseMessage = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(emptyRollcalls)
        };

        mockApi.Setup(x => x.GetAsync("rollcall/pending", It.IsAny<CancellationToken>()))
            .ReturnsAsync(responseMessage);

        // Act
        var result = await service.GetPendingApprovalsAsync();

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(0, result.Count());
    }

    /// <summary>
    /// Tests that GetPendingApprovalsAsync logs warning and returns empty collection when API responds with non-success status code.
    /// Input: Various non-success HTTP status codes (BadRequest, NotFound, InternalServerError, Unauthorized, Forbidden).
    /// Expected: Logs warning with status code and returns an empty collection.
    /// </summary>
    [TestMethod]
    [DataRow(HttpStatusCode.BadRequest)]
    [DataRow(HttpStatusCode.NotFound)]
    [DataRow(HttpStatusCode.InternalServerError)]
    [DataRow(HttpStatusCode.Unauthorized)]
    [DataRow(HttpStatusCode.Forbidden)]
    [DataRow(HttpStatusCode.ServiceUnavailable)]
    public async Task GetPendingApprovalsAsync_NonSuccessStatusCode_LogsWarningAndReturnsEmptyCollection(HttpStatusCode statusCode)
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<RollcallService>>();
        var service = new RollcallService(mockApi.Object, mockLogger.Object);

        var responseMessage = new HttpResponseMessage(statusCode);

        mockApi.Setup(x => x.GetAsync("rollcall/pending", It.IsAny<CancellationToken>()))
            .ReturnsAsync(responseMessage);

        // Act
        var result = await service.GetPendingApprovalsAsync();

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(0, result.Count());

        mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("GetPendingApprovals returned")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    /// <summary>
    /// Tests that GetPendingApprovalsAsync returns single item collection when API responds with one rollcall.
    /// Input: Successful HTTP response (200 OK) with single RollcallDto item.
    /// Expected: Returns a collection containing one RollcallDto item.
    /// </summary>
    [TestMethod]
    public async Task GetPendingApprovalsAsync_SuccessfulResponseWithSingleItem_ReturnsSingleItemCollection()
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<RollcallService>>();
        var service = new RollcallService(mockApi.Object, mockLogger.Object);

        var singleRollcall = new List<RollcallDto>
        {
            new RollcallDto
            {
                Id = "test-id",
                ScheduleId = "schedule-id",
                StudentId = "student-id",
                StudentName = "Test Student",
                CourseName = "Test Course",
                SelfieUrl = "https://test.com/selfie.jpg",
                SubmittedAt = new DateTime(2024, 1, 15, 10, 30, 0),
                Status = RollcallStatus.Pending
            }
        };

        var responseMessage = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(singleRollcall)
        };

        mockApi.Setup(x => x.GetAsync("rollcall/pending", It.IsAny<CancellationToken>()))
            .ReturnsAsync(responseMessage);

        // Act
        var result = await service.GetPendingApprovalsAsync();

        // Assert
        Assert.IsNotNull(result);
        var rollcallList = result.ToList();
        Assert.AreEqual(1, rollcallList.Count);
        Assert.AreEqual("test-id", rollcallList[0].Id);
        Assert.AreEqual("Test Student", rollcallList[0].StudentName);
    }

    /// <summary>
    /// Tests that GetPendingApprovalsAsync does not log when API responds with success status code.
    /// Input: Successful HTTP response (200 OK) with valid data.
    /// Expected: No warning is logged.
    /// </summary>
    [TestMethod]
    public async Task GetPendingApprovalsAsync_SuccessfulResponse_DoesNotLogWarning()
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<RollcallService>>();
        var service = new RollcallService(mockApi.Object, mockLogger.Object);

        var rollcalls = new List<RollcallDto>
        {
            new RollcallDto
            {
                Id = "1",
                ScheduleId = "schedule1",
                StudentId = "student1",
                StudentName = "Student",
                CourseName = "Course",
                SelfieUrl = "https://example.com/selfie.jpg",
                SubmittedAt = DateTime.UtcNow,
                Status = RollcallStatus.Pending
            }
        };

        var responseMessage = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(rollcalls)
        };

        mockApi.Setup(x => x.GetAsync("rollcall/pending", It.IsAny<CancellationToken>()))
            .ReturnsAsync(responseMessage);

        // Act
        var result = await service.GetPendingApprovalsAsync();

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
    /// Tests that the constructor successfully initializes the service when valid non-null dependencies are provided.
    /// Input: Valid mock instances for api and logger.
    /// Expected: Constructor completes successfully and creates a valid instance.
    /// </summary>
    [TestMethod]
    public void Constructor_WithValidDependencies_CreatesInstance()
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<RollcallService>>();

        // Act
        var service = new RollcallService(mockApi.Object, mockLogger.Object);

        // Assert
        Assert.IsNotNull(service);
    }

    /// <summary>
    /// Tests that the constructor accepts a null api parameter without throwing an exception.
    /// Input: Null api, valid logger.
    /// Expected: Constructor completes without throwing, documenting that no validation is performed.
    /// </summary>
    [TestMethod]
    public void Constructor_WithNullApi_DoesNotThrow()
    {
        // Arrange
        IApiService? nullApi = null;
        var mockLogger = new Mock<ILogger<RollcallService>>();

        // Act
        var service = new RollcallService(nullApi!, mockLogger.Object);

        // Assert
        Assert.IsNotNull(service);
    }

    /// <summary>
    /// Tests that the constructor accepts a null logger parameter without throwing an exception.
    /// Input: Valid api, null logger.
    /// Expected: Constructor completes without throwing, documenting that no validation is performed.
    /// </summary>
    [TestMethod]
    public void Constructor_WithNullLogger_DoesNotThrow()
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        ILogger<RollcallService>? nullLogger = null;

        // Act
        var service = new RollcallService(mockApi.Object, nullLogger!);

        // Assert
        Assert.IsNotNull(service);
    }

    /// <summary>
    /// Tests that the constructor accepts null values for both parameters without throwing an exception.
    /// Input: Null api and null logger.
    /// Expected: Constructor completes without throwing, documenting that no validation is performed.
    /// </summary>
    [TestMethod]
    public void Constructor_WithBothParametersNull_DoesNotThrow()
    {
        // Arrange
        IApiService? nullApi = null;
        ILogger<RollcallService>? nullLogger = null;

        // Act
        var service = new RollcallService(nullApi!, nullLogger!);

        // Assert
        Assert.IsNotNull(service);
    }

    /// <summary>
    /// Tests that GetMyRollcallsAsync returns an empty collection when the API call succeeds but returns empty data.
    /// Input: Successful HTTP response with empty JSON array.
    /// Expected: Returns an empty collection.
    /// </summary>
    [TestMethod]
    public async Task GetMyRollcallsAsync_SuccessfulResponseWithEmptyData_ReturnsEmptyCollection()
    {
        // Arrange
        Mock<IApiService> mockApiService = new Mock<IApiService>();
        Mock<ILogger<RollcallService>> mockLogger = new Mock<ILogger<RollcallService>>();

        string jsonContent = "[]";
        HttpResponseMessage successResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(jsonContent, Encoding.UTF8, "application/json")
        };

        mockApiService
            .Setup(x => x.GetAsync("rollcall/my", It.IsAny<CancellationToken>()))
            .ReturnsAsync(successResponse);

        RollcallService service = new RollcallService(mockApiService.Object, mockLogger.Object);

        // Act
        IEnumerable<RollcallDto> result = await service.GetMyRollcallsAsync();

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(0, result.Count());
        mockApiService.Verify(x => x.GetAsync("rollcall/my", It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Tests that GetMyRollcallsAsync returns an empty collection when deserialization returns null.
    /// Input: Successful HTTP response with null content.
    /// Expected: Returns an empty collection due to null-coalescing operator.
    /// </summary>
    [TestMethod]
    public async Task GetMyRollcallsAsync_SuccessfulResponseWithNullContent_ReturnsEmptyCollection()
    {
        // Arrange
        Mock<IApiService> mockApiService = new Mock<IApiService>();
        Mock<ILogger<RollcallService>> mockLogger = new Mock<ILogger<RollcallService>>();

        string jsonContent = "null";
        HttpResponseMessage successResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(jsonContent, Encoding.UTF8, "application/json")
        };

        mockApiService
            .Setup(x => x.GetAsync("rollcall/my", It.IsAny<CancellationToken>()))
            .ReturnsAsync(successResponse);

        RollcallService service = new RollcallService(mockApiService.Object, mockLogger.Object);

        // Act
        IEnumerable<RollcallDto> result = await service.GetMyRollcallsAsync();

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(0, result.Count());
        mockApiService.Verify(x => x.GetAsync("rollcall/my", It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Tests that GetMyRollcallsAsync returns an empty collection and logs a warning when the API call fails.
    /// Input: HTTP response with various non-success status codes (400, 401, 404, 500, 503).
    /// Expected: Returns an empty collection and logs a warning with the status code.
    /// </summary>
    [TestMethod]
    [DataRow(HttpStatusCode.BadRequest)]
    [DataRow(HttpStatusCode.Unauthorized)]
    [DataRow(HttpStatusCode.Forbidden)]
    [DataRow(HttpStatusCode.NotFound)]
    [DataRow(HttpStatusCode.InternalServerError)]
    [DataRow(HttpStatusCode.ServiceUnavailable)]
    [DataRow(HttpStatusCode.GatewayTimeout)]
    public async Task GetMyRollcallsAsync_UnsuccessfulResponse_ReturnsEmptyCollectionAndLogsWarning(HttpStatusCode statusCode)
    {
        // Arrange
        Mock<IApiService> mockApiService = new Mock<IApiService>();
        Mock<ILogger<RollcallService>> mockLogger = new Mock<ILogger<RollcallService>>();

        HttpResponseMessage failureResponse = new HttpResponseMessage(statusCode);

        mockApiService
            .Setup(x => x.GetAsync("rollcall/my", It.IsAny<CancellationToken>()))
            .ReturnsAsync(failureResponse);

        RollcallService service = new RollcallService(mockApiService.Object, mockLogger.Object);

        // Act
        IEnumerable<RollcallDto> result = await service.GetMyRollcallsAsync();

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(0, result.Count());
        mockApiService.Verify(x => x.GetAsync("rollcall/my", It.IsAny<CancellationToken>()), Times.Once);
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("GetMyRollcalls returned")),
                It.IsAny<Exception?>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    /// <summary>
    /// Tests that GetMyRollcallsAsync verifies the correct endpoint is called.
    /// Input: Successful HTTP response.
    /// Expected: Calls IApiService.GetAsync with "rollcall/my" endpoint exactly once.
    /// </summary>
    [TestMethod]
    public async Task GetMyRollcallsAsync_Always_CallsCorrectEndpoint()
    {
        // Arrange
        Mock<IApiService> mockApiService = new Mock<IApiService>();
        Mock<ILogger<RollcallService>> mockLogger = new Mock<ILogger<RollcallService>>();

        string jsonContent = "[]";
        HttpResponseMessage successResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(jsonContent, Encoding.UTF8, "application/json")
        };

        mockApiService
            .Setup(x => x.GetAsync("rollcall/my", It.IsAny<CancellationToken>()))
            .ReturnsAsync(successResponse);

        RollcallService service = new RollcallService(mockApiService.Object, mockLogger.Object);

        // Act
        await service.GetMyRollcallsAsync();

        // Assert
        mockApiService.Verify(x => x.GetAsync("rollcall/my", It.IsAny<CancellationToken>()), Times.Once);
        mockApiService.Verify(x => x.GetAsync(It.Is<string>(s => s != "rollcall/my"), It.IsAny<CancellationToken>()), Times.Never);
    }

    /// <summary>
    /// Tests that GetMyRollcallsAsync returns a single rollcall when the API call succeeds with one item.
    /// Input: Successful HTTP response with a single RollcallDto in JSON array.
    /// Expected: Returns a collection containing one RollcallDto object.
    /// </summary>
    [TestMethod]
    public async Task GetMyRollcallsAsync_SuccessfulResponseWithSingleItem_ReturnsSingleRollcall()
    {
        // Arrange
        Mock<IApiService> mockApiService = new Mock<IApiService>();
        Mock<ILogger<RollcallService>> mockLogger = new Mock<ILogger<RollcallService>>();

        List<RollcallDto> expectedRollcalls = new List<RollcallDto>
        {
            new RollcallDto
            {
                Id = "single",
                ScheduleId = "schedule1",
                StudentId = "student1",
                StudentName = "Single Student",
                CourseName = "Course 101",
                SelfieUrl = "https://example.com/selfie.jpg",
                SubmittedAt = new DateTime(2024, 1, 15, 10, 30, 0, DateTimeKind.Utc),
                Status = RollcallStatus.Pending,
                ApprovedByClassRepId = null,
                ApprovedByLecturerId = null
            }
        };

        string jsonContent = JsonSerializer.Serialize(expectedRollcalls);
        HttpResponseMessage successResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(jsonContent, Encoding.UTF8, "application/json")
        };

        mockApiService
            .Setup(x => x.GetAsync("rollcall/my", It.IsAny<CancellationToken>()))
            .ReturnsAsync(successResponse);

        RollcallService service = new RollcallService(mockApiService.Object, mockLogger.Object);

        // Act
        IEnumerable<RollcallDto> result = await service.GetMyRollcallsAsync();

        // Assert
        Assert.IsNotNull(result);
        List<RollcallDto> resultList = result.ToList();
        Assert.AreEqual(1, resultList.Count);
        Assert.AreEqual("single", resultList[0].Id);
        Assert.AreEqual("Single Student", resultList[0].StudentName);
    }

    /// <summary>
    /// Tests that GetMyRollcallsAsync does not log when the API call succeeds.
    /// Input: Successful HTTP response with valid data.
    /// Expected: Logger is not called.
    /// </summary>
    [TestMethod]
    public async Task GetMyRollcallsAsync_SuccessfulResponse_DoesNotLog()
    {
        // Arrange
        Mock<IApiService> mockApiService = new Mock<IApiService>();
        Mock<ILogger<RollcallService>> mockLogger = new Mock<ILogger<RollcallService>>();

        string jsonContent = "[]";
        HttpResponseMessage successResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(jsonContent, Encoding.UTF8, "application/json")
        };

        mockApiService
            .Setup(x => x.GetAsync("rollcall/my", It.IsAny<CancellationToken>()))
            .ReturnsAsync(successResponse);

        RollcallService service = new RollcallService(mockApiService.Object, mockLogger.Object);

        // Act
        await service.GetMyRollcallsAsync();

        // Assert
        mockLogger.Verify(
            x => x.Log(
                It.IsAny<LogLevel>(),
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception?>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Never);
    }

    /// <summary>
    /// Tests that GetMyRollcallsAsync handles various success status codes correctly.
    /// Input: HTTP responses with different 2xx status codes.
    /// Expected: Returns the deserialized data without logging warnings.
    /// </summary>
    [TestMethod]
    [DataRow(HttpStatusCode.OK)]
    [DataRow(HttpStatusCode.Created)]
    [DataRow(HttpStatusCode.Accepted)]
    [DataRow(HttpStatusCode.NoContent)]
    public async Task GetMyRollcallsAsync_VariousSuccessStatusCodes_ReturnsDataWithoutLogging(HttpStatusCode statusCode)
    {
        // Arrange
        Mock<IApiService> mockApiService = new Mock<IApiService>();
        Mock<ILogger<RollcallService>> mockLogger = new Mock<ILogger<RollcallService>>();

        string jsonContent = "[]";
        HttpResponseMessage successResponse = new HttpResponseMessage(statusCode)
        {
            Content = new StringContent(jsonContent, Encoding.UTF8, "application/json")
        };

        mockApiService
            .Setup(x => x.GetAsync("rollcall/my", It.IsAny<CancellationToken>()))
            .ReturnsAsync(successResponse);

        RollcallService service = new RollcallService(mockApiService.Object, mockLogger.Object);

        // Act
        IEnumerable<RollcallDto> result = await service.GetMyRollcallsAsync();

        // Assert
        Assert.IsNotNull(result);
        mockLogger.Verify(
            x => x.Log(
                It.IsAny<LogLevel>(),
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception?>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Never);
    }

    /// <summary>
    /// Tests that ApproveRollcallAsync returns true when the API returns a successful HTTP status code (200 OK).
    /// </summary>
    [TestMethod]
    public async Task ApproveRollcallAsync_SuccessfulResponse_ReturnsTrue()
    {
        // Arrange
        string rollcallId = "test-rollcall-123";
        Mock<IApiService> mockApiService = new Mock<IApiService>();
        Mock<ILogger<RollcallService>> mockLogger = new Mock<ILogger<RollcallService>>();

        HttpResponseMessage successResponse = new HttpResponseMessage(HttpStatusCode.OK);
        mockApiService
            .Setup(api => api.PostAsync($"rollcall/{rollcallId}/approve", It.IsAny<HttpContent>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(successResponse);

        RollcallService service = new RollcallService(mockApiService.Object, mockLogger.Object);

        // Act
        bool result = await service.ApproveRollcallAsync(rollcallId);

        // Assert
        Assert.IsTrue(result);
        mockApiService.Verify(api => api.PostAsync($"rollcall/{rollcallId}/approve", It.IsAny<HttpContent>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Tests that ApproveRollcallAsync returns false when the API returns a client error status code (400 Bad Request).
    /// </summary>
    [TestMethod]
    public async Task ApproveRollcallAsync_ClientErrorResponse_ReturnsFalse()
    {
        // Arrange
        string rollcallId = "test-rollcall-123";
        Mock<IApiService> mockApiService = new Mock<IApiService>();
        Mock<ILogger<RollcallService>> mockLogger = new Mock<ILogger<RollcallService>>();

        HttpResponseMessage errorResponse = new HttpResponseMessage(HttpStatusCode.BadRequest);
        mockApiService
            .Setup(api => api.PostAsync($"rollcall/{rollcallId}/approve", It.IsAny<HttpContent>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(errorResponse);

        RollcallService service = new RollcallService(mockApiService.Object, mockLogger.Object);

        // Act
        bool result = await service.ApproveRollcallAsync(rollcallId);

        // Assert
        Assert.IsFalse(result);
    }

    /// <summary>
    /// Tests that ApproveRollcallAsync returns false when the API returns a server error status code (500 Internal Server Error).
    /// </summary>
    [TestMethod]
    public async Task ApproveRollcallAsync_ServerErrorResponse_ReturnsFalse()
    {
        // Arrange
        string rollcallId = "test-rollcall-123";
        Mock<IApiService> mockApiService = new Mock<IApiService>();
        Mock<ILogger<RollcallService>> mockLogger = new Mock<ILogger<RollcallService>>();

        HttpResponseMessage errorResponse = new HttpResponseMessage(HttpStatusCode.InternalServerError);
        mockApiService
            .Setup(api => api.PostAsync($"rollcall/{rollcallId}/approve", It.IsAny<HttpContent>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(errorResponse);

        RollcallService service = new RollcallService(mockApiService.Object, mockLogger.Object);

        // Act
        bool result = await service.ApproveRollcallAsync(rollcallId);

        // Assert
        Assert.IsFalse(result);
    }

    /// <summary>
    /// Tests that ApproveRollcallAsync returns the correct result for various HTTP status codes.
    /// Verifies that 2xx codes return true and all other codes return false.
    /// </summary>
    /// <param name="statusCode">The HTTP status code to test.</param>
    /// <param name="expectedResult">The expected boolean result.</param>
    [TestMethod]
    [DataRow(HttpStatusCode.OK, true, DisplayName = "200 OK returns true")]
    [DataRow(HttpStatusCode.Created, true, DisplayName = "201 Created returns true")]
    [DataRow(HttpStatusCode.Accepted, true, DisplayName = "202 Accepted returns true")]
    [DataRow(HttpStatusCode.NoContent, true, DisplayName = "204 NoContent returns true")]
    [DataRow(HttpStatusCode.BadRequest, false, DisplayName = "400 BadRequest returns false")]
    [DataRow(HttpStatusCode.Unauthorized, false, DisplayName = "401 Unauthorized returns false")]
    [DataRow(HttpStatusCode.Forbidden, false, DisplayName = "403 Forbidden returns false")]
    [DataRow(HttpStatusCode.NotFound, false, DisplayName = "404 NotFound returns false")]
    [DataRow(HttpStatusCode.InternalServerError, false, DisplayName = "500 InternalServerError returns false")]
    [DataRow(HttpStatusCode.BadGateway, false, DisplayName = "502 BadGateway returns false")]
    [DataRow(HttpStatusCode.ServiceUnavailable, false, DisplayName = "503 ServiceUnavailable returns false")]
    public async Task ApproveRollcallAsync_VariousStatusCodes_ReturnsExpectedResult(HttpStatusCode statusCode, bool expectedResult)
    {
        // Arrange
        string rollcallId = "test-rollcall-123";
        Mock<IApiService> mockApiService = new Mock<IApiService>();
        Mock<ILogger<RollcallService>> mockLogger = new Mock<ILogger<RollcallService>>();

        HttpResponseMessage response = new HttpResponseMessage(statusCode);
        mockApiService
            .Setup(api => api.PostAsync($"rollcall/{rollcallId}/approve", It.IsAny<HttpContent>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        RollcallService service = new RollcallService(mockApiService.Object, mockLogger.Object);

        // Act
        bool result = await service.ApproveRollcallAsync(rollcallId);

        // Assert
        Assert.AreEqual(expectedResult, result);
    }

    /// <summary>
    /// Tests that ApproveRollcallAsync constructs the correct API endpoint URL with the provided rollcallId.
    /// Verifies the endpoint format is "rollcall/{rollcallId}/approve".
    /// </summary>
    [TestMethod]
    public async Task ApproveRollcallAsync_ValidRollcallId_ConstructsCorrectEndpoint()
    {
        // Arrange
        string rollcallId = "rollcall-456";
        string expectedEndpoint = $"rollcall/{rollcallId}/approve";
        Mock<IApiService> mockApiService = new Mock<IApiService>();
        Mock<ILogger<RollcallService>> mockLogger = new Mock<ILogger<RollcallService>>();

        HttpResponseMessage successResponse = new HttpResponseMessage(HttpStatusCode.OK);
        mockApiService
            .Setup(api => api.PostAsync(expectedEndpoint, It.IsAny<HttpContent>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(successResponse);

        RollcallService service = new RollcallService(mockApiService.Object, mockLogger.Object);

        // Act
        bool result = await service.ApproveRollcallAsync(rollcallId);

        // Assert
        Assert.IsTrue(result);
        mockApiService.Verify(api => api.PostAsync(expectedEndpoint, It.IsAny<HttpContent>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Tests that ApproveRollcallAsync handles empty string rollcallId by constructing an endpoint with empty segment.
    /// Verifies the method still executes but with potentially malformed endpoint "rollcall//approve".
    /// </summary>
    [TestMethod]
    public async Task ApproveRollcallAsync_EmptyStringRollcallId_ConstructsEndpointWithEmptySegment()
    {
        // Arrange
        string rollcallId = "";
        string expectedEndpoint = "rollcall//approve";
        Mock<IApiService> mockApiService = new Mock<IApiService>();
        Mock<ILogger<RollcallService>> mockLogger = new Mock<ILogger<RollcallService>>();

        HttpResponseMessage successResponse = new HttpResponseMessage(HttpStatusCode.OK);
        mockApiService
            .Setup(api => api.PostAsync(expectedEndpoint, It.IsAny<HttpContent>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(successResponse);

        RollcallService service = new RollcallService(mockApiService.Object, mockLogger.Object);

        // Act
        bool result = await service.ApproveRollcallAsync(rollcallId);

        // Assert
        Assert.IsTrue(result);
        mockApiService.Verify(api => api.PostAsync(expectedEndpoint, It.IsAny<HttpContent>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Tests that ApproveRollcallAsync handles whitespace-only rollcallId by including it in the endpoint.
    /// Verifies the method executes with the whitespace preserved in the URL.
    /// </summary>
    [TestMethod]
    public async Task ApproveRollcallAsync_WhitespaceRollcallId_IncludesWhitespaceInEndpoint()
    {
        // Arrange
        string rollcallId = "   ";
        string expectedEndpoint = $"rollcall/{rollcallId}/approve";
        Mock<IApiService> mockApiService = new Mock<IApiService>();
        Mock<ILogger<RollcallService>> mockLogger = new Mock<ILogger<RollcallService>>();

        HttpResponseMessage successResponse = new HttpResponseMessage(HttpStatusCode.OK);
        mockApiService
            .Setup(api => api.PostAsync(expectedEndpoint, It.IsAny<HttpContent>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(successResponse);

        RollcallService service = new RollcallService(mockApiService.Object, mockLogger.Object);

        // Act
        bool result = await service.ApproveRollcallAsync(rollcallId);

        // Assert
        Assert.IsTrue(result);
    }

    /// <summary>
    /// Tests that ApproveRollcallAsync handles rollcallId with special characters.
    /// Verifies the method includes special characters directly in the endpoint URL.
    /// </summary>
    [TestMethod]
    [DataRow("rollcall/123", DisplayName = "RollcallId with forward slash")]
    [DataRow("rollcall?param=value", DisplayName = "RollcallId with query string characters")]
    [DataRow("rollcall#anchor", DisplayName = "RollcallId with hash character")]
    [DataRow("rollcall@123", DisplayName = "RollcallId with @ symbol")]
    [DataRow("rollcall 123", DisplayName = "RollcallId with space")]
    public async Task ApproveRollcallAsync_RollcallIdWithSpecialCharacters_IncludesInEndpoint(string rollcallId)
    {
        // Arrange
        string expectedEndpoint = $"rollcall/{rollcallId}/approve";
        Mock<IApiService> mockApiService = new Mock<IApiService>();
        Mock<ILogger<RollcallService>> mockLogger = new Mock<ILogger<RollcallService>>();

        HttpResponseMessage successResponse = new HttpResponseMessage(HttpStatusCode.OK);
        mockApiService
            .Setup(api => api.PostAsync(expectedEndpoint, It.IsAny<HttpContent>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(successResponse);

        RollcallService service = new RollcallService(mockApiService.Object, mockLogger.Object);

        // Act
        bool result = await service.ApproveRollcallAsync(rollcallId);

        // Assert
        Assert.IsTrue(result);
        mockApiService.Verify(api => api.PostAsync(expectedEndpoint, It.IsAny<HttpContent>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Tests that ApproveRollcallAsync handles a very long rollcallId string.
    /// Verifies the method can process extremely long identifiers without error.
    /// </summary>
    [TestMethod]
    public async Task ApproveRollcallAsync_VeryLongRollcallId_ExecutesSuccessfully()
    {
        // Arrange
        string rollcallId = new string('a', 10000);
        string expectedEndpoint = $"rollcall/{rollcallId}/approve";
        Mock<IApiService> mockApiService = new Mock<IApiService>();
        Mock<ILogger<RollcallService>> mockLogger = new Mock<ILogger<RollcallService>>();

        HttpResponseMessage successResponse = new HttpResponseMessage(HttpStatusCode.OK);
        mockApiService
            .Setup(api => api.PostAsync(expectedEndpoint, It.IsAny<HttpContent>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(successResponse);

        RollcallService service = new RollcallService(mockApiService.Object, mockLogger.Object);

        // Act
        bool result = await service.ApproveRollcallAsync(rollcallId);

        // Assert
        Assert.IsTrue(result);
    }

    /// <summary>
    /// Tests that ApproveRollcallAsync sends empty JSON object as content in the POST request.
    /// Verifies that the API is called with JsonContent containing an empty anonymous object.
    /// </summary>
    [TestMethod]
    public async Task ApproveRollcallAsync_ValidRequest_SendsEmptyJsonContent()
    {
        // Arrange
        string rollcallId = "test-rollcall-123";
        Mock<IApiService> mockApiService = new Mock<IApiService>();
        Mock<ILogger<RollcallService>> mockLogger = new Mock<ILogger<RollcallService>>();

        HttpResponseMessage successResponse = new HttpResponseMessage(HttpStatusCode.OK);
        HttpContent? capturedContent = null;

        mockApiService
            .Setup(api => api.PostAsync(It.IsAny<string>(), It.IsAny<HttpContent>(), It.IsAny<CancellationToken>()))
            .Callback<string, HttpContent, CancellationToken>((path, content, ct) => capturedContent = content)
            .ReturnsAsync(successResponse);

        RollcallService service = new RollcallService(mockApiService.Object, mockLogger.Object);

        // Act
        bool result = await service.ApproveRollcallAsync(rollcallId);

        // Assert
        Assert.IsTrue(result);
        Assert.IsNotNull(capturedContent);
        Assert.IsInstanceOfType(capturedContent, typeof(JsonContent));
    }

    /// <summary>
    /// Tests that SubmitRollcallAsync returns true when the API returns a success status code.
    /// </summary>
    [TestMethod]
    public async Task SubmitRollcallAsync_SuccessResponse_ReturnsTrue()
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<RollcallService>>();
        var service = new RollcallService(mockApi.Object, mockLogger.Object);

        var scheduleId = "schedule123";
        var fileName = "selfie.jpg";
        using var stream = new MemoryStream();

        var successResponse = new HttpResponseMessage(HttpStatusCode.OK);
        mockApi.Setup(x => x.PostAsync("rollcall/submit", It.IsAny<MultipartFormDataContent>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(successResponse);

        // Act
        var result = await service.SubmitRollcallAsync(scheduleId, stream, fileName);

        // Assert
        Assert.IsTrue(result);
        mockApi.Verify(x => x.PostAsync("rollcall/submit", It.IsAny<MultipartFormDataContent>(), It.IsAny<CancellationToken>()), Times.Once);
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
    /// Tests that SubmitRollcallAsync returns false and logs a warning when the API returns a failure status code.
    /// </summary>
    /// <param name="statusCode">The HTTP status code to test.</param>
    [TestMethod]
    [DataRow(HttpStatusCode.BadRequest)]
    [DataRow(HttpStatusCode.Unauthorized)]
    [DataRow(HttpStatusCode.Forbidden)]
    [DataRow(HttpStatusCode.NotFound)]
    [DataRow(HttpStatusCode.InternalServerError)]
    [DataRow(HttpStatusCode.ServiceUnavailable)]
    public async Task SubmitRollcallAsync_FailureResponse_ReturnsFalseAndLogsWarning(HttpStatusCode statusCode)
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<RollcallService>>();
        var service = new RollcallService(mockApi.Object, mockLogger.Object);

        var scheduleId = "schedule123";
        var fileName = "selfie.jpg";
        using var stream = new MemoryStream();

        var failureResponse = new HttpResponseMessage(statusCode);
        mockApi.Setup(x => x.PostAsync("rollcall/submit", It.IsAny<MultipartFormDataContent>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(failureResponse);

        // Act
        var result = await service.SubmitRollcallAsync(scheduleId, stream, fileName);

        // Assert
        Assert.IsFalse(result);
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("SubmitRollcall returned")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    /// <summary>
    /// Tests that SubmitRollcallAsync handles empty scheduleId parameter.
    /// </summary>
    [TestMethod]
    public async Task SubmitRollcallAsync_EmptyScheduleId_CallsApiWithEmptyString()
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<RollcallService>>();
        var service = new RollcallService(mockApi.Object, mockLogger.Object);

        var scheduleId = string.Empty;
        var fileName = "selfie.jpg";
        using var stream = new MemoryStream();

        var successResponse = new HttpResponseMessage(HttpStatusCode.OK);
        mockApi.Setup(x => x.PostAsync("rollcall/submit", It.IsAny<MultipartFormDataContent>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(successResponse);

        // Act
        var result = await service.SubmitRollcallAsync(scheduleId, stream, fileName);

        // Assert
        Assert.IsTrue(result);
        mockApi.Verify(x => x.PostAsync("rollcall/submit", It.IsAny<MultipartFormDataContent>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Tests that SubmitRollcallAsync handles whitespace-only scheduleId parameter.
    /// </summary>
    [TestMethod]
    public async Task SubmitRollcallAsync_WhitespaceScheduleId_CallsApiWithWhitespace()
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<RollcallService>>();
        var service = new RollcallService(mockApi.Object, mockLogger.Object);

        var scheduleId = "   ";
        var fileName = "selfie.jpg";
        using var stream = new MemoryStream();

        var successResponse = new HttpResponseMessage(HttpStatusCode.OK);
        mockApi.Setup(x => x.PostAsync("rollcall/submit", It.IsAny<MultipartFormDataContent>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(successResponse);

        // Act
        var result = await service.SubmitRollcallAsync(scheduleId, stream, fileName);

        // Assert
        Assert.IsTrue(result);
        mockApi.Verify(x => x.PostAsync("rollcall/submit", It.IsAny<MultipartFormDataContent>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Tests that SubmitRollcallAsync handles scheduleId with special characters.
    /// </summary>
    [TestMethod]
    public async Task SubmitRollcallAsync_ScheduleIdWithSpecialCharacters_CallsApiSuccessfully()
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<RollcallService>>();
        var service = new RollcallService(mockApi.Object, mockLogger.Object);

        var scheduleId = "schedule-123_test@#$%";
        var fileName = "selfie.jpg";
        using var stream = new MemoryStream();

        var successResponse = new HttpResponseMessage(HttpStatusCode.OK);
        mockApi.Setup(x => x.PostAsync("rollcall/submit", It.IsAny<MultipartFormDataContent>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(successResponse);

        // Act
        var result = await service.SubmitRollcallAsync(scheduleId, stream, fileName);

        // Assert
        Assert.IsTrue(result);
    }

    /// <summary>
    /// Tests that SubmitRollcallAsync handles very long scheduleId parameter.
    /// </summary>
    [TestMethod]
    public async Task SubmitRollcallAsync_VeryLongScheduleId_CallsApiSuccessfully()
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<RollcallService>>();
        var service = new RollcallService(mockApi.Object, mockLogger.Object);

        var scheduleId = new string('a', 10000);
        var fileName = "selfie.jpg";
        using var stream = new MemoryStream();

        var successResponse = new HttpResponseMessage(HttpStatusCode.OK);
        mockApi.Setup(x => x.PostAsync("rollcall/submit", It.IsAny<MultipartFormDataContent>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(successResponse);

        // Act
        var result = await service.SubmitRollcallAsync(scheduleId, stream, fileName);

        // Assert
        Assert.IsTrue(result);
    }

    /// <summary>
    /// Tests that SubmitRollcallAsync handles empty fileName parameter.
    /// </summary>
    [TestMethod]
    public async Task SubmitRollcallAsync_EmptyFileName_CallsApiWithEmptyString()
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<RollcallService>>();
        var service = new RollcallService(mockApi.Object, mockLogger.Object);

        var scheduleId = "schedule123";
        var fileName = string.Empty;
        using var stream = new MemoryStream();

        var successResponse = new HttpResponseMessage(HttpStatusCode.OK);
        mockApi.Setup(x => x.PostAsync("rollcall/submit", It.IsAny<MultipartFormDataContent>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(successResponse);

        // Act
        var result = await service.SubmitRollcallAsync(scheduleId, stream, fileName);

        // Assert
        Assert.IsTrue(result);
    }

    /// <summary>
    /// Tests that SubmitRollcallAsync handles fileName with special characters and path components.
    /// </summary>
    [TestMethod]
    public async Task SubmitRollcallAsync_FileNameWithSpecialCharacters_CallsApiSuccessfully()
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<RollcallService>>();
        var service = new RollcallService(mockApi.Object, mockLogger.Object);

        var scheduleId = "schedule123";
        var fileName = "my_selfie-2024@test.jpg";
        using var stream = new MemoryStream();

        var successResponse = new HttpResponseMessage(HttpStatusCode.OK);
        mockApi.Setup(x => x.PostAsync("rollcall/submit", It.IsAny<MultipartFormDataContent>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(successResponse);

        // Act
        var result = await service.SubmitRollcallAsync(scheduleId, stream, fileName);

        // Assert
        Assert.IsTrue(result);
    }

    /// <summary>
    /// Tests that SubmitRollcallAsync handles very long fileName parameter.
    /// </summary>
    [TestMethod]
    public async Task SubmitRollcallAsync_VeryLongFileName_CallsApiSuccessfully()
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<RollcallService>>();
        var service = new RollcallService(mockApi.Object, mockLogger.Object);

        var scheduleId = "schedule123";
        var fileName = new string('a', 5000) + ".jpg";
        using var stream = new MemoryStream();

        var successResponse = new HttpResponseMessage(HttpStatusCode.OK);
        mockApi.Setup(x => x.PostAsync("rollcall/submit", It.IsAny<MultipartFormDataContent>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(successResponse);

        // Act
        var result = await service.SubmitRollcallAsync(scheduleId, stream, fileName);

        // Assert
        Assert.IsTrue(result);
    }

    /// <summary>
    /// Tests that SubmitRollcallAsync handles an empty stream.
    /// </summary>
    [TestMethod]
    public async Task SubmitRollcallAsync_EmptyStream_CallsApiSuccessfully()
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<RollcallService>>();
        var service = new RollcallService(mockApi.Object, mockLogger.Object);

        var scheduleId = "schedule123";
        var fileName = "selfie.jpg";
        using var stream = new MemoryStream();

        var successResponse = new HttpResponseMessage(HttpStatusCode.OK);
        mockApi.Setup(x => x.PostAsync("rollcall/submit", It.IsAny<MultipartFormDataContent>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(successResponse);

        // Act
        var result = await service.SubmitRollcallAsync(scheduleId, stream, fileName);

        // Assert
        Assert.IsTrue(result);
    }

    /// <summary>
    /// Tests that SubmitRollcallAsync handles a stream with data.
    /// </summary>
    [TestMethod]
    public async Task SubmitRollcallAsync_StreamWithData_CallsApiSuccessfully()
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<RollcallService>>();
        var service = new RollcallService(mockApi.Object, mockLogger.Object);

        var scheduleId = "schedule123";
        var fileName = "selfie.jpg";
        using var stream = new MemoryStream(new byte[] { 1, 2, 3, 4, 5 });

        var successResponse = new HttpResponseMessage(HttpStatusCode.OK);
        mockApi.Setup(x => x.PostAsync("rollcall/submit", It.IsAny<MultipartFormDataContent>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(successResponse);

        // Act
        var result = await service.SubmitRollcallAsync(scheduleId, stream, fileName);

        // Assert
        Assert.IsTrue(result);
    }

    /// <summary>
    /// Tests that SubmitRollcallAsync returns true for all 2xx success status codes.
    /// </summary>
    /// <param name="statusCode">The HTTP status code to test.</param>
    [TestMethod]
    [DataRow(HttpStatusCode.OK)]
    [DataRow(HttpStatusCode.Created)]
    [DataRow(HttpStatusCode.Accepted)]
    [DataRow(HttpStatusCode.NoContent)]
    public async Task SubmitRollcallAsync_SuccessStatusCodes_ReturnsTrue(HttpStatusCode statusCode)
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<RollcallService>>();
        var service = new RollcallService(mockApi.Object, mockLogger.Object);

        var scheduleId = "schedule123";
        var fileName = "selfie.jpg";
        using var stream = new MemoryStream();

        var response = new HttpResponseMessage(statusCode);
        mockApi.Setup(x => x.PostAsync("rollcall/submit", It.IsAny<MultipartFormDataContent>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        // Act
        var result = await service.SubmitRollcallAsync(scheduleId, stream, fileName);

        // Assert
        Assert.IsTrue(result);
    }

    /// <summary>
    /// Tests that SubmitRollcallAsync calls the correct API endpoint.
    /// </summary>
    [TestMethod]
    public async Task SubmitRollcallAsync_ValidInputs_CallsCorrectEndpoint()
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<RollcallService>>();
        var service = new RollcallService(mockApi.Object, mockLogger.Object);

        var scheduleId = "schedule123";
        var fileName = "selfie.jpg";
        using var stream = new MemoryStream();

        var successResponse = new HttpResponseMessage(HttpStatusCode.OK);
        mockApi.Setup(x => x.PostAsync("rollcall/submit", It.IsAny<MultipartFormDataContent>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(successResponse);

        // Act
        await service.SubmitRollcallAsync(scheduleId, stream, fileName);

        // Assert
        mockApi.Verify(x => x.PostAsync("rollcall/submit", It.IsAny<MultipartFormDataContent>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Tests that SubmitRollcallAsync logs the correct status code on failure.
    /// </summary>
    [TestMethod]
    public async Task SubmitRollcallAsync_FailureResponse_LogsCorrectStatusCode()
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<RollcallService>>();
        var service = new RollcallService(mockApi.Object, mockLogger.Object);

        var scheduleId = "schedule123";
        var fileName = "selfie.jpg";
        using var stream = new MemoryStream();

        var expectedStatusCode = HttpStatusCode.BadRequest;
        var failureResponse = new HttpResponseMessage(expectedStatusCode);
        mockApi.Setup(x => x.PostAsync("rollcall/submit", It.IsAny<MultipartFormDataContent>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(failureResponse);

        // Act
        await service.SubmitRollcallAsync(scheduleId, stream, fileName);

        // Assert
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains(expectedStatusCode.ToString())),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    /// <summary>
    /// Tests that RejectRollcallAsync returns true when the API call succeeds.
    /// Input: Valid rollcallId with successful API response (status code 200).
    /// Expected: Method returns true.
    /// </summary>
    [TestMethod]
    public async Task RejectRollcallAsync_ValidRollcallIdWithSuccessResponse_ReturnsTrue()
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<RollcallService>>();
        var service = new RollcallService(mockApi.Object, mockLogger.Object);
        string rollcallId = "rollcall-123";
        var successResponse = new HttpResponseMessage(HttpStatusCode.OK);

        mockApi.Setup(x => x.PostAsync(
                $"rollcall/{rollcallId}/reject",
                It.IsAny<HttpContent>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(successResponse);

        // Act
        bool result = await service.RejectRollcallAsync(rollcallId);

        // Assert
        Assert.IsTrue(result);
        mockApi.Verify(x => x.PostAsync(
            $"rollcall/{rollcallId}/reject",
            It.IsAny<HttpContent>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Tests that RejectRollcallAsync returns false when the API call fails.
    /// Input: Valid rollcallId with various failure status codes.
    /// Expected: Method returns false for all failure status codes.
    /// </summary>
    [TestMethod]
    [DataRow(HttpStatusCode.BadRequest)]
    [DataRow(HttpStatusCode.NotFound)]
    [DataRow(HttpStatusCode.InternalServerError)]
    [DataRow(HttpStatusCode.Unauthorized)]
    [DataRow(HttpStatusCode.Forbidden)]
    [DataRow(HttpStatusCode.ServiceUnavailable)]
    public async Task RejectRollcallAsync_ValidRollcallIdWithFailureResponse_ReturnsFalse(HttpStatusCode statusCode)
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<RollcallService>>();
        var service = new RollcallService(mockApi.Object, mockLogger.Object);
        string rollcallId = "rollcall-456";
        var failureResponse = new HttpResponseMessage(statusCode);

        mockApi.Setup(x => x.PostAsync(
                It.IsAny<string>(),
                It.IsAny<HttpContent>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(failureResponse);

        // Act
        bool result = await service.RejectRollcallAsync(rollcallId);

        // Assert
        Assert.IsFalse(result);
    }

    /// <summary>
    /// Tests that RejectRollcallAsync handles null rollcallId.
    /// Input: null rollcallId.
    /// Expected: Method constructs URL with empty string (URL will be "rollcall//reject").
    /// </summary>
    [TestMethod]
    public async Task RejectRollcallAsync_NullRollcallId_CallsApiWithEmptyString()
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<RollcallService>>();
        var service = new RollcallService(mockApi.Object, mockLogger.Object);
        string? rollcallId = null;
        var successResponse = new HttpResponseMessage(HttpStatusCode.OK);

        mockApi.Setup(x => x.PostAsync(
                It.IsAny<string>(),
                It.IsAny<HttpContent>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(successResponse);

        // Act
        bool result = await service.RejectRollcallAsync(rollcallId!);

        // Assert
        Assert.IsTrue(result);
        mockApi.Verify(x => x.PostAsync(
            "rollcall//reject",
            It.IsAny<HttpContent>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Tests that RejectRollcallAsync handles empty string rollcallId.
    /// Input: Empty string rollcallId.
    /// Expected: Method constructs URL with empty string (URL will be "rollcall//reject").
    /// </summary>
    [TestMethod]
    public async Task RejectRollcallAsync_EmptyStringRollcallId_CallsApiWithEmptyString()
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<RollcallService>>();
        var service = new RollcallService(mockApi.Object, mockLogger.Object);
        string rollcallId = string.Empty;
        var successResponse = new HttpResponseMessage(HttpStatusCode.OK);

        mockApi.Setup(x => x.PostAsync(
                "rollcall//reject",
                It.IsAny<HttpContent>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(successResponse);

        // Act
        bool result = await service.RejectRollcallAsync(rollcallId);

        // Assert
        Assert.IsTrue(result);
        mockApi.Verify(x => x.PostAsync(
            "rollcall//reject",
            It.IsAny<HttpContent>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Tests that RejectRollcallAsync handles whitespace-only rollcallId.
    /// Input: Whitespace string rollcallId (spaces, tabs, newlines).
    /// Expected: Method constructs URL with whitespace characters.
    /// </summary>
    [TestMethod]
    [DataRow("   ")]
    [DataRow("\t")]
    [DataRow("\n")]
    [DataRow(" \t\n ")]
    public async Task RejectRollcallAsync_WhitespaceRollcallId_CallsApiWithWhitespace(string rollcallId)
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<RollcallService>>();
        var service = new RollcallService(mockApi.Object, mockLogger.Object);
        var successResponse = new HttpResponseMessage(HttpStatusCode.OK);

        mockApi.Setup(x => x.PostAsync(
                It.IsAny<string>(),
                It.IsAny<HttpContent>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(successResponse);

        // Act
        bool result = await service.RejectRollcallAsync(rollcallId);

        // Assert
        Assert.IsTrue(result);
        mockApi.Verify(x => x.PostAsync(
            $"rollcall/{rollcallId}/reject",
            It.IsAny<HttpContent>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Tests that RejectRollcallAsync handles rollcallId with special characters.
    /// Input: RollcallId containing special URL characters.
    /// Expected: Method constructs URL with special characters (no encoding performed by method).
    /// </summary>
    [TestMethod]
    [DataRow("rollcall/123")]
    [DataRow("rollcall?query=1")]
    [DataRow("rollcall&param=value")]
    [DataRow("rollcall#anchor")]
    [DataRow("rollcall with spaces")]
    [DataRow("rollcall%20encoded")]
    public async Task RejectRollcallAsync_RollcallIdWithSpecialCharacters_CallsApiWithSpecialCharacters(string rollcallId)
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<RollcallService>>();
        var service = new RollcallService(mockApi.Object, mockLogger.Object);
        var successResponse = new HttpResponseMessage(HttpStatusCode.OK);

        mockApi.Setup(x => x.PostAsync(
                It.IsAny<string>(),
                It.IsAny<HttpContent>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(successResponse);

        // Act
        bool result = await service.RejectRollcallAsync(rollcallId);

        // Assert
        Assert.IsTrue(result);
        mockApi.Verify(x => x.PostAsync(
            $"rollcall/{rollcallId}/reject",
            It.IsAny<HttpContent>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Tests that RejectRollcallAsync handles very long rollcallId strings.
    /// Input: Very long string (1000 characters).
    /// Expected: Method successfully constructs URL and calls API.
    /// </summary>
    [TestMethod]
    public async Task RejectRollcallAsync_VeryLongRollcallId_CallsApiSuccessfully()
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<RollcallService>>();
        var service = new RollcallService(mockApi.Object, mockLogger.Object);
        string rollcallId = new string('a', 1000);
        var successResponse = new HttpResponseMessage(HttpStatusCode.OK);

        mockApi.Setup(x => x.PostAsync(
                It.IsAny<string>(),
                It.IsAny<HttpContent>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(successResponse);

        // Act
        bool result = await service.RejectRollcallAsync(rollcallId);

        // Assert
        Assert.IsTrue(result);
        mockApi.Verify(x => x.PostAsync(
            $"rollcall/{rollcallId}/reject",
            It.IsAny<HttpContent>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Tests that RejectRollcallAsync handles Unicode characters in rollcallId.
    /// Input: RollcallId containing Unicode characters.
    /// Expected: Method constructs URL with Unicode characters.
    /// </summary>
    [TestMethod]
    [DataRow("rollcall-日本語")]
    [DataRow("rollcall-עברית")]
    [DataRow("rollcall-العربية")]
    [DataRow("rollcall-🎉emoji")]
    public async Task RejectRollcallAsync_RollcallIdWithUnicodeCharacters_CallsApiWithUnicode(string rollcallId)
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<RollcallService>>();
        var service = new RollcallService(mockApi.Object, mockLogger.Object);
        var successResponse = new HttpResponseMessage(HttpStatusCode.OK);

        mockApi.Setup(x => x.PostAsync(
                It.IsAny<string>(),
                It.IsAny<HttpContent>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(successResponse);

        // Act
        bool result = await service.RejectRollcallAsync(rollcallId);

        // Assert
        Assert.IsTrue(result);
        mockApi.Verify(x => x.PostAsync(
            $"rollcall/{rollcallId}/reject",
            It.IsAny<HttpContent>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Tests that RejectRollcallAsync returns true for all 2xx success status codes.
    /// Input: Valid rollcallId with various success status codes (200, 201, 202, 204).
    /// Expected: Method returns true for all 2xx status codes.
    /// </summary>
    [TestMethod]
    [DataRow(HttpStatusCode.OK)]
    [DataRow(HttpStatusCode.Created)]
    [DataRow(HttpStatusCode.Accepted)]
    [DataRow(HttpStatusCode.NoContent)]
    public async Task RejectRollcallAsync_SuccessStatusCodes_ReturnsTrue(HttpStatusCode statusCode)
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<RollcallService>>();
        var service = new RollcallService(mockApi.Object, mockLogger.Object);
        string rollcallId = "rollcall-success";
        var response = new HttpResponseMessage(statusCode);

        mockApi.Setup(x => x.PostAsync(
                It.IsAny<string>(),
                It.IsAny<HttpContent>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        // Act
        bool result = await service.RejectRollcallAsync(rollcallId);

        // Assert
        Assert.IsTrue(result);
    }

    /// <summary>
    /// Tests that RejectRollcallAsync verifies the correct content is sent to the API.
    /// Input: Valid rollcallId.
    /// Expected: PostAsync is called with JsonContent containing an empty object.
    /// </summary>
    [TestMethod]
    public async Task RejectRollcallAsync_ValidRollcallId_SendsEmptyJsonContent()
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<RollcallService>>();
        var service = new RollcallService(mockApi.Object, mockLogger.Object);
        string rollcallId = "rollcall-content-test";
        var successResponse = new HttpResponseMessage(HttpStatusCode.OK);
        HttpContent? capturedContent = null;

        mockApi.Setup(x => x.PostAsync(
                It.IsAny<string>(),
                It.IsAny<HttpContent>(),
                It.IsAny<CancellationToken>()))
            .Callback<string, HttpContent, CancellationToken>((path, content, ct) => capturedContent = content)
            .ReturnsAsync(successResponse);

        // Act
        bool result = await service.RejectRollcallAsync(rollcallId);

        // Assert
        Assert.IsTrue(result);
        Assert.IsNotNull(capturedContent);
        Assert.IsInstanceOfType(capturedContent, typeof(JsonContent));
    }

    /// <summary>
    /// Tests that RejectRollcallAsync returns true when the API call succeeds with a 200 OK response.
    /// Input: Valid rollcallId with successful API response.
    /// Expected: Method returns true.
    /// </summary>
    [TestMethod]
    public async Task RejectRollcallAsync_SuccessfulApiResponse_ReturnsTrue()
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<RollcallService>>();
        var service = new RollcallService(mockApi.Object, mockLogger.Object);
        string rollcallId = "test-rollcall-123";
        var successResponse = new HttpResponseMessage(HttpStatusCode.OK);

        mockApi.Setup(x => x.PostAsync(
                $"rollcall/{rollcallId}/reject",
                It.IsAny<HttpContent>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(successResponse);

        // Act
        bool result = await service.RejectRollcallAsync(rollcallId);

        // Assert
        Assert.IsTrue(result);
    }

    /// <summary>
    /// Tests that RejectRollcallAsync returns false for various HTTP failure status codes.
    /// Input: Valid rollcallId with failure status codes.
    /// Expected: Method returns false for all failure status codes.
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
    public async Task RejectRollcallAsync_FailureStatusCodes_ReturnsFalse(HttpStatusCode statusCode)
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<RollcallService>>();
        var service = new RollcallService(mockApi.Object, mockLogger.Object);
        string rollcallId = "test-rollcall-456";
        var failureResponse = new HttpResponseMessage(statusCode);

        mockApi.Setup(x => x.PostAsync(
                It.IsAny<string>(),
                It.IsAny<HttpContent>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(failureResponse);

        // Act
        bool result = await service.RejectRollcallAsync(rollcallId);

        // Assert
        Assert.IsFalse(result);
    }

    /// <summary>
    /// Tests that RejectRollcallAsync constructs the correct API endpoint URL.
    /// Input: Valid rollcallId.
    /// Expected: PostAsync is called with correct endpoint format "rollcall/{rollcallId}/reject".
    /// </summary>
    [TestMethod]
    public async Task RejectRollcallAsync_ValidRollcallId_CallsCorrectEndpoint()
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<RollcallService>>();
        var service = new RollcallService(mockApi.Object, mockLogger.Object);
        string rollcallId = "specific-id-123";
        var successResponse = new HttpResponseMessage(HttpStatusCode.OK);

        mockApi.Setup(x => x.PostAsync(
                It.IsAny<string>(),
                It.IsAny<HttpContent>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(successResponse);

        // Act
        await service.RejectRollcallAsync(rollcallId);

        // Assert
        mockApi.Verify(x => x.PostAsync(
            $"rollcall/{rollcallId}/reject",
            It.IsAny<HttpContent>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Tests that RejectRollcallAsync handles empty string rollcallId.
    /// Input: Empty string rollcallId.
    /// Expected: Constructs URL with empty segment "rollcall//reject".
    /// </summary>
    [TestMethod]
    public async Task RejectRollcallAsync_EmptyStringRollcallId_ConstructsUrlWithEmptySegment()
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<RollcallService>>();
        var service = new RollcallService(mockApi.Object, mockLogger.Object);
        string rollcallId = string.Empty;
        var successResponse = new HttpResponseMessage(HttpStatusCode.OK);

        mockApi.Setup(x => x.PostAsync(
                It.IsAny<string>(),
                It.IsAny<HttpContent>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(successResponse);

        // Act
        await service.RejectRollcallAsync(rollcallId);

        // Assert
        mockApi.Verify(x => x.PostAsync(
            "rollcall//reject",
            It.IsAny<HttpContent>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Tests that RejectRollcallAsync handles whitespace-only rollcallId.
    /// Input: Whitespace strings (spaces, tabs, newlines).
    /// Expected: Constructs URL with whitespace characters.
    /// </summary>
    [TestMethod]
    [DataRow("   ")]
    [DataRow("\t")]
    [DataRow("\n")]
    [DataRow(" \t\n ")]
    public async Task RejectRollcallAsync_WhitespaceRollcallId_IncludesWhitespaceInUrl(string rollcallId)
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<RollcallService>>();
        var service = new RollcallService(mockApi.Object, mockLogger.Object);
        var successResponse = new HttpResponseMessage(HttpStatusCode.OK);

        mockApi.Setup(x => x.PostAsync(
                It.IsAny<string>(),
                It.IsAny<HttpContent>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(successResponse);

        // Act
        await service.RejectRollcallAsync(rollcallId);

        // Assert
        mockApi.Verify(x => x.PostAsync(
            $"rollcall/{rollcallId}/reject",
            It.IsAny<HttpContent>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Tests that RejectRollcallAsync handles rollcallId with special URL characters.
    /// Input: RollcallId containing special characters that may affect URLs.
    /// Expected: Constructs URL with special characters included.
    /// </summary>
    [TestMethod]
    [DataRow("rollcall/123")]
    [DataRow("rollcall?query=1")]
    [DataRow("rollcall&param=value")]
    [DataRow("rollcall#anchor")]
    [DataRow("rollcall with spaces")]
    [DataRow("rollcall%20encoded")]
    public async Task RejectRollcallAsync_RollcallIdWithSpecialCharacters_IncludesInUrl(string rollcallId)
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<RollcallService>>();
        var service = new RollcallService(mockApi.Object, mockLogger.Object);
        var successResponse = new HttpResponseMessage(HttpStatusCode.OK);

        mockApi.Setup(x => x.PostAsync(
                It.IsAny<string>(),
                It.IsAny<HttpContent>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(successResponse);

        // Act
        await service.RejectRollcallAsync(rollcallId);

        // Assert
        mockApi.Verify(x => x.PostAsync(
            $"rollcall/{rollcallId}/reject",
            It.IsAny<HttpContent>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Tests that RejectRollcallAsync handles very long rollcallId strings.
    /// Input: Very long string (1000 characters).
    /// Expected: Method successfully constructs URL and completes.
    /// </summary>
    [TestMethod]
    public async Task RejectRollcallAsync_VeryLongRollcallId_ExecutesSuccessfully()
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<RollcallService>>();
        var service = new RollcallService(mockApi.Object, mockLogger.Object);
        string rollcallId = new string('x', 1000);
        var successResponse = new HttpResponseMessage(HttpStatusCode.OK);

        mockApi.Setup(x => x.PostAsync(
                It.IsAny<string>(),
                It.IsAny<HttpContent>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(successResponse);

        // Act
        bool result = await service.RejectRollcallAsync(rollcallId);

        // Assert
        Assert.IsTrue(result);
    }

    /// <summary>
    /// Tests that RejectRollcallAsync handles Unicode characters in rollcallId.
    /// Input: RollcallId containing Unicode characters.
    /// Expected: Constructs URL with Unicode characters.
    /// </summary>
    [TestMethod]
    [DataRow("rollcall-日本語")]
    [DataRow("rollcall-עברית")]
    [DataRow("rollcall-العربية")]
    [DataRow("rollcall-🎉emoji")]
    public async Task RejectRollcallAsync_RollcallIdWithUnicode_IncludesUnicodeInUrl(string rollcallId)
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<RollcallService>>();
        var service = new RollcallService(mockApi.Object, mockLogger.Object);
        var successResponse = new HttpResponseMessage(HttpStatusCode.OK);

        mockApi.Setup(x => x.PostAsync(
                It.IsAny<string>(),
                It.IsAny<HttpContent>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(successResponse);

        // Act
        await service.RejectRollcallAsync(rollcallId);

        // Assert
        mockApi.Verify(x => x.PostAsync(
            $"rollcall/{rollcallId}/reject",
            It.IsAny<HttpContent>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Tests that ApproveRollcallAsync handles null rollcallId.
    /// Input: null rollcallId (even though parameter is non-nullable).
    /// Expected: Method constructs URL with empty string resulting in "rollcall//approve".
    /// </summary>
    [TestMethod]
    public async Task ApproveRollcallAsync_NullRollcallId_ConstructsEndpointWithEmptySegment()
    {
        // Arrange
        string rollcallId = null!;
        Mock<IApiService> mockApiService = new Mock<IApiService>();
        Mock<ILogger<RollcallService>> mockLogger = new Mock<ILogger<RollcallService>>();

        HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.OK);
        mockApiService
            .Setup(api => api.PostAsync("rollcall//approve", It.IsAny<HttpContent>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        RollcallService service = new RollcallService(mockApiService.Object, mockLogger.Object);

        // Act
        bool result = await service.ApproveRollcallAsync(rollcallId);

        // Assert
        Assert.IsTrue(result);
        mockApiService.Verify(api => api.PostAsync("rollcall//approve", It.IsAny<HttpContent>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Tests that SubmitRollcallAsync does not log when the API returns a success status code.
    /// Input: Valid parameters with successful API response.
    /// Expected: No warning is logged.
    /// </summary>
    [TestMethod]
    public async Task SubmitRollcallAsync_SuccessResponse_DoesNotLogWarning()
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<RollcallService>>();
        var service = new RollcallService(mockApi.Object, mockLogger.Object);

        var scheduleId = "schedule123";
        var fileName = "selfie.jpg";
        using var stream = new MemoryStream();

        var successResponse = new HttpResponseMessage(HttpStatusCode.OK);
        mockApi.Setup(x => x.PostAsync("rollcall/submit", It.IsAny<MultipartFormDataContent>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(successResponse);

        // Act
        await service.SubmitRollcallAsync(scheduleId, stream, fileName);

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
    /// Tests that SubmitRollcallAsync handles whitespace-only fileName parameter.
    /// Input: Whitespace string for fileName.
    /// Expected: Calls API successfully with whitespace preserved.
    /// </summary>
    [TestMethod]
    public async Task SubmitRollcallAsync_WhitespaceFileName_CallsApiWithWhitespace()
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<RollcallService>>();
        var service = new RollcallService(mockApi.Object, mockLogger.Object);

        var scheduleId = "schedule123";
        var fileName = "   ";
        using var stream = new MemoryStream();

        var successResponse = new HttpResponseMessage(HttpStatusCode.OK);
        mockApi.Setup(x => x.PostAsync("rollcall/submit", It.IsAny<MultipartFormDataContent>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(successResponse);

        // Act
        var result = await service.SubmitRollcallAsync(scheduleId, stream, fileName);

        // Assert
        Assert.IsTrue(result);
    }

    /// <summary>
    /// Tests that SubmitRollcallAsync handles a large stream.
    /// Input: MemoryStream containing 1MB of data.
    /// Expected: Calls API successfully.
    /// </summary>
    [TestMethod]
    public async Task SubmitRollcallAsync_LargeStream_CallsApiSuccessfully()
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<RollcallService>>();
        var service = new RollcallService(mockApi.Object, mockLogger.Object);

        var scheduleId = "schedule123";
        var fileName = "selfie.jpg";
        var data = new byte[1024 * 1024]; // 1MB
        using var stream = new MemoryStream(data);

        var successResponse = new HttpResponseMessage(HttpStatusCode.OK);
        mockApi.Setup(x => x.PostAsync("rollcall/submit", It.IsAny<MultipartFormDataContent>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(successResponse);

        // Act
        var result = await service.SubmitRollcallAsync(scheduleId, stream, fileName);

        // Assert
        Assert.IsTrue(result);
    }

    /// <summary>
    /// Tests that GetPendingApprovalsAsync returns true for all 2xx success status codes.
    /// Input: Various 2xx success status codes.
    /// Expected: Returns deserialized data for all 2xx codes without logging warnings.
    /// </summary>
    [TestMethod]
    [DataRow(HttpStatusCode.OK)]
    [DataRow(HttpStatusCode.Created)]
    [DataRow(HttpStatusCode.Accepted)]
    [DataRow(HttpStatusCode.NoContent)]
    [DataRow(HttpStatusCode.PartialContent)]
    public async Task GetPendingApprovalsAsync_Various2xxStatusCodes_ReturnsData(HttpStatusCode statusCode)
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<RollcallService>>();
        var service = new RollcallService(mockApi.Object, mockLogger.Object);

        var expectedRollcalls = new List<RollcallDto>
        {
            new RollcallDto
            {
                Id = "test-id",
                ScheduleId = "schedule-id",
                StudentId = "student-id",
                StudentName = "Test Student",
                CourseName = "Test Course",
                SelfieUrl = "https://example.com/selfie.jpg",
                SubmittedAt = DateTime.UtcNow,
                Status = RollcallStatus.Pending
            }
        };

        var responseMessage = new HttpResponseMessage(statusCode)
        {
            Content = JsonContent.Create(expectedRollcalls)
        };

        mockApi.Setup(x => x.GetAsync("rollcall/pending", It.IsAny<CancellationToken>()))
            .ReturnsAsync(responseMessage);

        // Act
        var result = await service.GetPendingApprovalsAsync();

        // Assert
        Assert.IsNotNull(result);
        var rollcallList = result.ToList();
        Assert.AreEqual(1, rollcallList.Count);
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
    /// Tests that GetPendingApprovalsAsync handles 3xx redirect status codes as non-success.
    /// Input: Various 3xx redirect status codes.
    /// Expected: Logs warning and returns empty collection.
    /// </summary>
    [TestMethod]
    [DataRow(HttpStatusCode.MultipleChoices)]
    [DataRow(HttpStatusCode.MovedPermanently)]
    [DataRow(HttpStatusCode.Found)]
    [DataRow(HttpStatusCode.NotModified)]
    public async Task GetPendingApprovalsAsync_3xxRedirectStatusCodes_LogsWarningAndReturnsEmpty(HttpStatusCode statusCode)
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<RollcallService>>();
        var service = new RollcallService(mockApi.Object, mockLogger.Object);

        var responseMessage = new HttpResponseMessage(statusCode);

        mockApi.Setup(x => x.GetAsync("rollcall/pending", It.IsAny<CancellationToken>()))
            .ReturnsAsync(responseMessage);

        // Act
        var result = await service.GetPendingApprovalsAsync();

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(0, result.Count());
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("GetPendingApprovals returned")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    /// <summary>
    /// Tests that GetPendingApprovalsAsync handles boundary status codes correctly.
    /// Input: Boundary status codes between success and failure ranges.
    /// Expected: 299 (last 2xx) succeeds, 300 (first 3xx) fails.
    /// </summary>
    [TestMethod]
    [DataRow((HttpStatusCode)299, true)]
    [DataRow((HttpStatusCode)300, false)]
    [DataRow((HttpStatusCode)199, false)]
    [DataRow((HttpStatusCode)200, true)]
    public async Task GetPendingApprovalsAsync_BoundaryStatusCodes_HandlesCorrectly(HttpStatusCode statusCode, bool shouldSucceed)
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<RollcallService>>();
        var service = new RollcallService(mockApi.Object, mockLogger.Object);

        var expectedRollcalls = new List<RollcallDto>
        {
            new RollcallDto { Id = "test-id" }
        };

        var responseMessage = new HttpResponseMessage(statusCode)
        {
            Content = JsonContent.Create(expectedRollcalls)
        };

        mockApi.Setup(x => x.GetAsync("rollcall/pending", It.IsAny<CancellationToken>()))
            .ReturnsAsync(responseMessage);

        // Act
        var result = await service.GetPendingApprovalsAsync();

        // Assert
        Assert.IsNotNull(result);
        if (shouldSucceed)
        {
            Assert.AreEqual(1, result.Count());
        }
        else
        {
            Assert.AreEqual(0, result.Count());
            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }
    }

    /// <summary>
    /// Tests that GetPendingApprovalsAsync correctly calls the API endpoint.
    /// Input: Valid request.
    /// Expected: Calls GetAsync with "rollcall/pending" endpoint exactly once.
    /// </summary>
    [TestMethod]
    public async Task GetPendingApprovalsAsync_ValidRequest_CallsCorrectEndpoint()
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<RollcallService>>();
        var service = new RollcallService(mockApi.Object, mockLogger.Object);

        var responseMessage = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(new List<RollcallDto>())
        };

        mockApi.Setup(x => x.GetAsync("rollcall/pending", It.IsAny<CancellationToken>()))
            .ReturnsAsync(responseMessage);

        // Act
        await service.GetPendingApprovalsAsync();

        // Assert
        mockApi.Verify(x => x.GetAsync("rollcall/pending", It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Tests that GetPendingApprovalsAsync handles large collections correctly.
    /// Input: Successful HTTP response with large RollcallDto collection.
    /// Expected: Returns all items in the collection.
    /// </summary>
    [TestMethod]
    public async Task GetPendingApprovalsAsync_LargeCollection_ReturnsAllItems()
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<RollcallService>>();
        var service = new RollcallService(mockApi.Object, mockLogger.Object);

        var expectedRollcalls = new List<RollcallDto>();
        for (int i = 0; i < 1000; i++)
        {
            expectedRollcalls.Add(new RollcallDto
            {
                Id = $"id-{i}",
                ScheduleId = $"schedule-{i}",
                StudentId = $"student-{i}",
                StudentName = $"Student {i}",
                CourseName = $"Course {i}",
                SelfieUrl = $"https://example.com/selfie{i}.jpg",
                SubmittedAt = DateTime.UtcNow,
                Status = RollcallStatus.Pending
            });
        }

        var responseMessage = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(expectedRollcalls)
        };

        mockApi.Setup(x => x.GetAsync("rollcall/pending", It.IsAny<CancellationToken>()))
            .ReturnsAsync(responseMessage);

        // Act
        var result = await service.GetPendingApprovalsAsync();

        // Assert
        Assert.IsNotNull(result);
        var rollcallList = result.ToList();
        Assert.AreEqual(1000, rollcallList.Count);
        Assert.AreEqual("id-0", rollcallList[0].Id);
        Assert.AreEqual("id-999", rollcallList[999].Id);
    }

    /// <summary>
    /// Tests that GetPendingApprovalsAsync verifies the correct warning message format.
    /// Input: Non-success status code.
    /// Expected: Logs warning with specific message format including status code.
    /// </summary>
    [TestMethod]
    public async Task GetPendingApprovalsAsync_NonSuccessResponse_LogsCorrectMessageFormat()
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<RollcallService>>();
        var service = new RollcallService(mockApi.Object, mockLogger.Object);

        var responseMessage = new HttpResponseMessage(HttpStatusCode.InternalServerError);

        mockApi.Setup(x => x.GetAsync("rollcall/pending", It.IsAny<CancellationToken>()))
            .ReturnsAsync(responseMessage);

        // Act
        var result = await service.GetPendingApprovalsAsync();

        // Assert
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) =>
                    v.ToString()!.Contains("GetPendingApprovals returned") &&
                    v.ToString()!.Contains("InternalServerError")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    /// <summary>
    /// Tests that GetMyRollcallsAsync returns multiple rollcall items when the API call succeeds with multiple items.
    /// Input: Successful HTTP response (200 OK) with multiple RollcallDto objects in JSON array.
    /// Expected: Returns a collection containing multiple RollcallDto objects with correct data.
    /// </summary>
    [TestMethod]
    public async Task GetMyRollcallsAsync_SuccessfulResponseWithMultipleItems_ReturnsMultipleRollcalls()
    {
        // Arrange
        Mock<IApiService> mockApiService = new Mock<IApiService>();
        Mock<ILogger<RollcallService>> mockLogger = new Mock<ILogger<RollcallService>>();

        string jsonContent = @"[
            {""id"":""rollcall-1"",""scheduleId"":""schedule-1"",""userId"":""user-1"",""status"":""Pending"",""submittedAt"":""2024-01-01T10:00:00Z""},
            {""id"":""rollcall-2"",""scheduleId"":""schedule-2"",""userId"":""user-2"",""status"":""Approved"",""submittedAt"":""2024-01-02T11:00:00Z""},
            {""id"":""rollcall-3"",""scheduleId"":""schedule-3"",""userId"":""user-3"",""status"":""Rejected"",""submittedAt"":""2024-01-03T12:00:00Z""}
        ]";
        HttpResponseMessage successResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(jsonContent, Encoding.UTF8, "application/json")
        };

        mockApiService
            .Setup(x => x.GetAsync("rollcall/my", It.IsAny<CancellationToken>()))
            .ReturnsAsync(successResponse);

        RollcallService service = new RollcallService(mockApiService.Object, mockLogger.Object);

        // Act
        IEnumerable<RollcallDto> result = await service.GetMyRollcallsAsync();

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(3, result.Count());
        Assert.AreEqual("rollcall-1", result.ElementAt(0).Id);
        Assert.AreEqual("rollcall-2", result.ElementAt(1).Id);
        Assert.AreEqual("rollcall-3", result.ElementAt(2).Id);
        mockApiService.Verify(x => x.GetAsync("rollcall/my", It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Tests that GetMyRollcallsAsync handles 3xx redirect status codes by returning empty collection and logging warning.
    /// Input: HTTP response with redirect status codes (301, 302, 304).
    /// Expected: Returns an empty collection and logs a warning with the status code.
    /// </summary>
    [TestMethod]
    [DataRow(HttpStatusCode.MovedPermanently)]
    [DataRow(HttpStatusCode.Found)]
    [DataRow(HttpStatusCode.NotModified)]
    [DataRow(HttpStatusCode.TemporaryRedirect)]
    public async Task GetMyRollcallsAsync_RedirectStatusCode_ReturnsEmptyCollectionAndLogsWarning(HttpStatusCode statusCode)
    {
        // Arrange
        Mock<IApiService> mockApiService = new Mock<IApiService>();
        Mock<ILogger<RollcallService>> mockLogger = new Mock<ILogger<RollcallService>>();

        HttpResponseMessage redirectResponse = new HttpResponseMessage(statusCode);

        mockApiService
            .Setup(x => x.GetAsync("rollcall/my", It.IsAny<CancellationToken>()))
            .ReturnsAsync(redirectResponse);

        RollcallService service = new RollcallService(mockApiService.Object, mockLogger.Object);

        // Act
        IEnumerable<RollcallDto> result = await service.GetMyRollcallsAsync();

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(0, result.Count());
        mockApiService.Verify(x => x.GetAsync("rollcall/my", It.IsAny<CancellationToken>()), Times.Once);
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("GetMyRollcalls returned")),
                It.IsAny<Exception?>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    /// <summary>
    /// Tests that GetMyRollcallsAsync handles informational 1xx status codes by returning empty collection and logging warning.
    /// Input: HTTP response with informational status codes (100, 101, 102).
    /// Expected: Returns an empty collection and logs a warning with the status code.
    /// </summary>
    [TestMethod]
    [DataRow(HttpStatusCode.Continue)]
    [DataRow(HttpStatusCode.SwitchingProtocols)]
    public async Task GetMyRollcallsAsync_InformationalStatusCode_ReturnsEmptyCollectionAndLogsWarning(HttpStatusCode statusCode)
    {
        // Arrange
        Mock<IApiService> mockApiService = new Mock<IApiService>();
        Mock<ILogger<RollcallService>> mockLogger = new Mock<ILogger<RollcallService>>();

        HttpResponseMessage informationalResponse = new HttpResponseMessage(statusCode);

        mockApiService
            .Setup(x => x.GetAsync("rollcall/my", It.IsAny<CancellationToken>()))
            .ReturnsAsync(informationalResponse);

        RollcallService service = new RollcallService(mockApiService.Object, mockLogger.Object);

        // Act
        IEnumerable<RollcallDto> result = await service.GetMyRollcallsAsync();

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(0, result.Count());
        mockApiService.Verify(x => x.GetAsync("rollcall/my", It.IsAny<CancellationToken>()), Times.Once);
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("GetMyRollcalls returned")),
                It.IsAny<Exception?>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    /// <summary>
    /// Tests that GetMyRollcallsAsync verifies the API is called exactly once regardless of outcome.
    /// Input: Both successful and unsuccessful HTTP responses.
    /// Expected: IApiService.GetAsync is called exactly once in all scenarios.
    /// </summary>
    [TestMethod]
    [DataRow(HttpStatusCode.OK)]
    [DataRow(HttpStatusCode.BadRequest)]
    [DataRow(HttpStatusCode.InternalServerError)]
    public async Task GetMyRollcallsAsync_AnyStatusCode_CallsApiExactlyOnce(HttpStatusCode statusCode)
    {
        // Arrange
        Mock<IApiService> mockApiService = new Mock<IApiService>();
        Mock<ILogger<RollcallService>> mockLogger = new Mock<ILogger<RollcallService>>();

        string jsonContent = statusCode == HttpStatusCode.OK ? "[]" : "";
        HttpResponseMessage response = new HttpResponseMessage(statusCode)
        {
            Content = new StringContent(jsonContent, Encoding.UTF8, "application/json")
        };

        mockApiService
            .Setup(x => x.GetAsync("rollcall/my", It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        RollcallService service = new RollcallService(mockApiService.Object, mockLogger.Object);

        // Act
        IEnumerable<RollcallDto> result = await service.GetMyRollcallsAsync();

        // Assert
        mockApiService.Verify(x => x.GetAsync("rollcall/my", It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Tests that GetMyRollcallsAsync logs the exact status code value in the warning message.
    /// Input: HTTP response with specific non-success status code.
    /// Expected: Logger is called with message containing the exact status code.
    /// </summary>
    [TestMethod]
    public async Task GetMyRollcallsAsync_NonSuccessStatusCode_LogsExactStatusCode()
    {
        // Arrange
        Mock<IApiService> mockApiService = new Mock<IApiService>();
        Mock<ILogger<RollcallService>> mockLogger = new Mock<ILogger<RollcallService>>();

        HttpStatusCode expectedStatusCode = HttpStatusCode.NotFound;
        HttpResponseMessage failureResponse = new HttpResponseMessage(expectedStatusCode);

        mockApiService
            .Setup(x => x.GetAsync("rollcall/my", It.IsAny<CancellationToken>()))
            .ReturnsAsync(failureResponse);

        RollcallService service = new RollcallService(mockApiService.Object, mockLogger.Object);

        // Act
        IEnumerable<RollcallDto> result = await service.GetMyRollcallsAsync();

        // Assert
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("GetMyRollcalls returned") && v.ToString()!.Contains(expectedStatusCode.ToString())),
                It.IsAny<Exception?>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    /// <summary>
    /// Tests that GetMyRollcallsAsync returns collection that can be enumerated multiple times.
    /// Input: Successful HTTP response with valid data.
    /// Expected: Returns a collection that can be enumerated multiple times with consistent results.
    /// </summary>
    [TestMethod]
    public async Task GetMyRollcallsAsync_SuccessfulResponse_ReturnsReEnumerableCollection()
    {
        // Arrange
        Mock<IApiService> mockApiService = new Mock<IApiService>();
        Mock<ILogger<RollcallService>> mockLogger = new Mock<ILogger<RollcallService>>();

        string jsonContent = @"[{""id"":""test-1""},{""id"":""test-2""}]";
        HttpResponseMessage successResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(jsonContent, Encoding.UTF8, "application/json")
        };

        mockApiService
            .Setup(x => x.GetAsync("rollcall/my", It.IsAny<CancellationToken>()))
            .ReturnsAsync(successResponse);

        RollcallService service = new RollcallService(mockApiService.Object, mockLogger.Object);

        // Act
        IEnumerable<RollcallDto> result = await service.GetMyRollcallsAsync();

        // Assert
        int firstCount = result.Count();
        int secondCount = result.Count();
        Assert.AreEqual(2, firstCount);
        Assert.AreEqual(2, secondCount);
        Assert.AreEqual(firstCount, secondCount);
    }

    /// <summary>
    /// Tests that RejectRollcallAsync returns true when API responds with OK status.
    /// Input: Valid rollcallId with 200 OK response.
    /// Expected: Returns true.
    /// </summary>
    [TestMethod]
    public async Task RejectRollcallAsync_OkResponse_ReturnsTrue()
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<RollcallService>>();
        var service = new RollcallService(mockApi.Object, mockLogger.Object);
        string rollcallId = "test-id-123";
        var response = new HttpResponseMessage(HttpStatusCode.OK);

        mockApi.Setup(x => x.PostAsync(
                It.IsAny<string>(),
                It.IsAny<HttpContent>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        // Act
        bool result = await service.RejectRollcallAsync(rollcallId);

        // Assert
        Assert.IsTrue(result);
    }

    /// <summary>
    /// Tests that RejectRollcallAsync returns false for various client and server error status codes.
    /// Input: Valid rollcallId with non-2xx status codes.
    /// Expected: Returns false for all error status codes.
    /// </summary>
    [TestMethod]
    [DataRow(HttpStatusCode.BadRequest, DisplayName = "400 BadRequest")]
    [DataRow(HttpStatusCode.Unauthorized, DisplayName = "401 Unauthorized")]
    [DataRow(HttpStatusCode.Forbidden, DisplayName = "403 Forbidden")]
    [DataRow(HttpStatusCode.NotFound, DisplayName = "404 NotFound")]
    [DataRow(HttpStatusCode.InternalServerError, DisplayName = "500 InternalServerError")]
    [DataRow(HttpStatusCode.BadGateway, DisplayName = "502 BadGateway")]
    [DataRow(HttpStatusCode.ServiceUnavailable, DisplayName = "503 ServiceUnavailable")]
    [DataRow(HttpStatusCode.GatewayTimeout, DisplayName = "504 GatewayTimeout")]
    public async Task RejectRollcallAsync_ErrorStatusCodes_ReturnsFalse(HttpStatusCode statusCode)
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<RollcallService>>();
        var service = new RollcallService(mockApi.Object, mockLogger.Object);
        string rollcallId = "error-test-id";
        var response = new HttpResponseMessage(statusCode);

        mockApi.Setup(x => x.PostAsync(
                It.IsAny<string>(),
                It.IsAny<HttpContent>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        // Act
        bool result = await service.RejectRollcallAsync(rollcallId);

        // Assert
        Assert.IsFalse(result);
    }

    /// <summary>
    /// Tests that RejectRollcallAsync returns true for all 2xx success status codes.
    /// Input: Valid rollcallId with various 2xx status codes.
    /// Expected: Returns true for all 2xx status codes.
    /// </summary>
    [TestMethod]
    [DataRow(HttpStatusCode.OK, DisplayName = "200 OK")]
    [DataRow(HttpStatusCode.Created, DisplayName = "201 Created")]
    [DataRow(HttpStatusCode.Accepted, DisplayName = "202 Accepted")]
    [DataRow(HttpStatusCode.NoContent, DisplayName = "204 NoContent")]
    public async Task RejectRollcallAsync_Success2xxStatusCodes_ReturnsTrue(HttpStatusCode statusCode)
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<RollcallService>>();
        var service = new RollcallService(mockApi.Object, mockLogger.Object);
        string rollcallId = "success-test-id";
        var response = new HttpResponseMessage(statusCode);

        mockApi.Setup(x => x.PostAsync(
                It.IsAny<string>(),
                It.IsAny<HttpContent>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        // Act
        bool result = await service.RejectRollcallAsync(rollcallId);

        // Assert
        Assert.IsTrue(result);
    }

    /// <summary>
    /// Tests that RejectRollcallAsync calls PostAsync with the correct endpoint format.
    /// Input: Valid rollcallId.
    /// Expected: Calls PostAsync with "rollcall/{rollcallId}/reject" endpoint exactly once.
    /// </summary>
    [TestMethod]
    public async Task RejectRollcallAsync_ValidRollcallId_CallsCorrectEndpointFormat()
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<RollcallService>>();
        var service = new RollcallService(mockApi.Object, mockLogger.Object);
        string rollcallId = "endpoint-test-123";
        var response = new HttpResponseMessage(HttpStatusCode.OK);

        mockApi.Setup(x => x.PostAsync(
                It.IsAny<string>(),
                It.IsAny<HttpContent>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        // Act
        await service.RejectRollcallAsync(rollcallId);

        // Assert
        mockApi.Verify(x => x.PostAsync(
            $"rollcall/{rollcallId}/reject",
            It.IsAny<HttpContent>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Tests that RejectRollcallAsync handles empty string rollcallId by constructing endpoint with empty segment.
    /// Input: Empty string rollcallId.
    /// Expected: Constructs "rollcall//reject" endpoint and executes successfully.
    /// </summary>
    [TestMethod]
    public async Task RejectRollcallAsync_EmptyStringRollcallId_ConstructsEndpointWithEmptySegment()
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<RollcallService>>();
        var service = new RollcallService(mockApi.Object, mockLogger.Object);
        string rollcallId = string.Empty;
        var response = new HttpResponseMessage(HttpStatusCode.OK);

        mockApi.Setup(x => x.PostAsync(
                It.IsAny<string>(),
                It.IsAny<HttpContent>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        // Act
        bool result = await service.RejectRollcallAsync(rollcallId);

        // Assert
        Assert.IsTrue(result);
        mockApi.Verify(x => x.PostAsync(
            "rollcall//reject",
            It.IsAny<HttpContent>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Tests that RejectRollcallAsync handles whitespace-only rollcallId strings.
    /// Input: Various whitespace strings (spaces, tabs, newlines).
    /// Expected: Includes whitespace in the endpoint URL.
    /// </summary>
    [TestMethod]
    [DataRow("   ", DisplayName = "Three spaces")]
    [DataRow("\t", DisplayName = "Tab character")]
    [DataRow("\n", DisplayName = "Newline character")]
    [DataRow(" \t\n ", DisplayName = "Mixed whitespace")]
    public async Task RejectRollcallAsync_WhitespaceRollcallId_IncludesWhitespaceInEndpoint(string rollcallId)
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<RollcallService>>();
        var service = new RollcallService(mockApi.Object, mockLogger.Object);
        var response = new HttpResponseMessage(HttpStatusCode.OK);

        mockApi.Setup(x => x.PostAsync(
                It.IsAny<string>(),
                It.IsAny<HttpContent>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        // Act
        bool result = await service.RejectRollcallAsync(rollcallId);

        // Assert
        Assert.IsTrue(result);
        mockApi.Verify(x => x.PostAsync(
            $"rollcall/{rollcallId}/reject",
            It.IsAny<HttpContent>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Tests that RejectRollcallAsync handles rollcallId containing special URL characters.
    /// Input: RollcallId with special characters that could affect URL construction.
    /// Expected: Includes special characters in the endpoint without encoding.
    /// </summary>
    [TestMethod]
    [DataRow("id/with/slashes", DisplayName = "Forward slashes")]
    [DataRow("id?query=value", DisplayName = "Query string characters")]
    [DataRow("id&param=test", DisplayName = "Ampersand")]
    [DataRow("id#anchor", DisplayName = "Hash/anchor")]
    [DataRow("id with spaces", DisplayName = "Spaces")]
    [DataRow("id%20encoded", DisplayName = "Percent encoding")]
    public async Task RejectRollcallAsync_RollcallIdWithSpecialCharacters_IncludesInEndpoint(string rollcallId)
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<RollcallService>>();
        var service = new RollcallService(mockApi.Object, mockLogger.Object);
        var response = new HttpResponseMessage(HttpStatusCode.OK);

        mockApi.Setup(x => x.PostAsync(
                It.IsAny<string>(),
                It.IsAny<HttpContent>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        // Act
        bool result = await service.RejectRollcallAsync(rollcallId);

        // Assert
        Assert.IsTrue(result);
        mockApi.Verify(x => x.PostAsync(
            $"rollcall/{rollcallId}/reject",
            It.IsAny<HttpContent>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Tests that RejectRollcallAsync handles Unicode characters in rollcallId.
    /// Input: RollcallId containing various Unicode characters.
    /// Expected: Includes Unicode characters in the endpoint.
    /// </summary>
    [TestMethod]
    [DataRow("id-日本語", DisplayName = "Japanese characters")]
    [DataRow("id-עברית", DisplayName = "Hebrew characters")]
    [DataRow("id-العربية", DisplayName = "Arabic characters")]
    [DataRow("id-🎉emoji", DisplayName = "Emoji characters")]
    public async Task RejectRollcallAsync_RollcallIdWithUnicode_IncludesUnicodeInEndpoint(string rollcallId)
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<RollcallService>>();
        var service = new RollcallService(mockApi.Object, mockLogger.Object);
        var response = new HttpResponseMessage(HttpStatusCode.OK);

        mockApi.Setup(x => x.PostAsync(
                It.IsAny<string>(),
                It.IsAny<HttpContent>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        // Act
        bool result = await service.RejectRollcallAsync(rollcallId);

        // Assert
        Assert.IsTrue(result);
        mockApi.Verify(x => x.PostAsync(
            $"rollcall/{rollcallId}/reject",
            It.IsAny<HttpContent>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Tests that RejectRollcallAsync returns false for 3xx redirect status codes.
    /// Input: Valid rollcallId with redirect status codes.
    /// Expected: Returns false for all redirect status codes.
    /// </summary>
    [TestMethod]
    [DataRow(HttpStatusCode.MovedPermanently, DisplayName = "301 MovedPermanently")]
    [DataRow(HttpStatusCode.Found, DisplayName = "302 Found")]
    [DataRow(HttpStatusCode.NotModified, DisplayName = "304 NotModified")]
    [DataRow(HttpStatusCode.TemporaryRedirect, DisplayName = "307 TemporaryRedirect")]
    public async Task RejectRollcallAsync_RedirectStatusCodes_ReturnsFalse(HttpStatusCode statusCode)
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<RollcallService>>();
        var service = new RollcallService(mockApi.Object, mockLogger.Object);
        string rollcallId = "redirect-test";
        var response = new HttpResponseMessage(statusCode);

        mockApi.Setup(x => x.PostAsync(
                It.IsAny<string>(),
                It.IsAny<HttpContent>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        // Act
        bool result = await service.RejectRollcallAsync(rollcallId);

        // Assert
        Assert.IsFalse(result);
    }

    /// <summary>
    /// Tests that RejectRollcallAsync handles boundary status codes correctly.
    /// Input: Status codes at the boundary between success and non-success ranges.
    /// Expected: 199 returns false, 200 returns true, 299 returns true, 300 returns false.
    /// </summary>
    [TestMethod]
    [DataRow((HttpStatusCode)199, false, DisplayName = "199 (just below 2xx) returns false")]
    [DataRow((HttpStatusCode)200, true, DisplayName = "200 (first 2xx) returns true")]
    [DataRow((HttpStatusCode)299, true, DisplayName = "299 (last 2xx) returns true")]
    [DataRow((HttpStatusCode)300, false, DisplayName = "300 (first 3xx) returns false")]
    public async Task RejectRollcallAsync_BoundaryStatusCodes_ReturnsExpectedResult(HttpStatusCode statusCode, bool expectedResult)
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<RollcallService>>();
        var service = new RollcallService(mockApi.Object, mockLogger.Object);
        string rollcallId = "boundary-test";
        var response = new HttpResponseMessage(statusCode);

        mockApi.Setup(x => x.PostAsync(
                It.IsAny<string>(),
                It.IsAny<HttpContent>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        // Act
        bool result = await service.RejectRollcallAsync(rollcallId);

        // Assert
        Assert.AreEqual(expectedResult, result);
    }

    /// <summary>
    /// Tests that RejectRollcallAsync verifies PostAsync is called exactly once.
    /// Input: Valid rollcallId with success response.
    /// Expected: PostAsync is invoked exactly once.
    /// </summary>
    [TestMethod]
    public async Task RejectRollcallAsync_ValidRequest_CallsPostAsyncOnce()
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<RollcallService>>();
        var service = new RollcallService(mockApi.Object, mockLogger.Object);
        string rollcallId = "once-test";
        var response = new HttpResponseMessage(HttpStatusCode.OK);

        mockApi.Setup(x => x.PostAsync(
                It.IsAny<string>(),
                It.IsAny<HttpContent>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        // Act
        await service.RejectRollcallAsync(rollcallId);

        // Assert
        mockApi.Verify(x => x.PostAsync(
            It.IsAny<string>(),
            It.IsAny<HttpContent>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Tests that RejectRollcallAsync does not log when successful.
    /// Input: Valid rollcallId with success response.
    /// Expected: Logger is not invoked.
    /// </summary>
    [TestMethod]
    public async Task RejectRollcallAsync_SuccessResponse_DoesNotLog()
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<RollcallService>>();
        var service = new RollcallService(mockApi.Object, mockLogger.Object);
        string rollcallId = "no-log-success";
        var response = new HttpResponseMessage(HttpStatusCode.OK);

        mockApi.Setup(x => x.PostAsync(
                It.IsAny<string>(),
                It.IsAny<HttpContent>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        // Act
        await service.RejectRollcallAsync(rollcallId);

        // Assert
        mockLogger.Verify(
            x => x.Log(
                It.IsAny<LogLevel>(),
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Never);
    }

    /// <summary>
    /// Tests that RejectRollcallAsync does not log when failing.
    /// Input: Valid rollcallId with failure response.
    /// Expected: Logger is not invoked.
    /// </summary>
    [TestMethod]
    public async Task RejectRollcallAsync_FailureResponse_DoesNotLog()
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<RollcallService>>();
        var service = new RollcallService(mockApi.Object, mockLogger.Object);
        string rollcallId = "no-log-failure";
        var response = new HttpResponseMessage(HttpStatusCode.BadRequest);

        mockApi.Setup(x => x.PostAsync(
                It.IsAny<string>(),
                It.IsAny<HttpContent>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        // Act
        await service.RejectRollcallAsync(rollcallId);

        // Assert
        mockLogger.Verify(
            x => x.Log(
                It.IsAny<LogLevel>(),
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Never);
    }

    /// <summary>
    /// Tests that ApproveRollcallAsync returns true for all 2xx success status codes.
    /// Input: Valid rollcallId with various 2xx HTTP success status codes.
    /// Expected: Returns true for all 2xx status codes (200, 201, 202, 204, 206, 299).
    /// </summary>
    [TestMethod]
    [DataRow(HttpStatusCode.OK, DisplayName = "200 OK")]
    [DataRow(HttpStatusCode.Created, DisplayName = "201 Created")]
    [DataRow(HttpStatusCode.Accepted, DisplayName = "202 Accepted")]
    [DataRow(HttpStatusCode.NoContent, DisplayName = "204 NoContent")]
    [DataRow(HttpStatusCode.PartialContent, DisplayName = "206 PartialContent")]
    [DataRow((HttpStatusCode)299, DisplayName = "299 (boundary 2xx)")]
    public async Task ApproveRollcallAsync_Success2xxStatusCodes_ReturnsTrue(HttpStatusCode statusCode)
    {
        // Arrange
        string rollcallId = "test-rollcall-123";
        Mock<IApiService> mockApiService = new Mock<IApiService>();
        Mock<ILogger<RollcallService>> mockLogger = new Mock<ILogger<RollcallService>>();

        HttpResponseMessage response = new HttpResponseMessage(statusCode);
        mockApiService
            .Setup(api => api.PostAsync($"rollcall/{rollcallId}/approve", It.IsAny<HttpContent>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        RollcallService service = new RollcallService(mockApiService.Object, mockLogger.Object);

        // Act
        bool result = await service.ApproveRollcallAsync(rollcallId);

        // Assert
        Assert.IsTrue(result);
    }

    /// <summary>
    /// Tests that ApproveRollcallAsync returns false for 4xx client error status codes.
    /// Input: Valid rollcallId with various 4xx HTTP client error status codes.
    /// Expected: Returns false for all 4xx status codes.
    /// </summary>
    [TestMethod]
    [DataRow(HttpStatusCode.BadRequest, DisplayName = "400 BadRequest")]
    [DataRow(HttpStatusCode.Unauthorized, DisplayName = "401 Unauthorized")]
    [DataRow(HttpStatusCode.Forbidden, DisplayName = "403 Forbidden")]
    [DataRow(HttpStatusCode.NotFound, DisplayName = "404 NotFound")]
    [DataRow(HttpStatusCode.MethodNotAllowed, DisplayName = "405 MethodNotAllowed")]
    [DataRow(HttpStatusCode.Conflict, DisplayName = "409 Conflict")]
    [DataRow(HttpStatusCode.Gone, DisplayName = "410 Gone")]
    public async Task ApproveRollcallAsync_ClientError4xxStatusCodes_ReturnsFalse(HttpStatusCode statusCode)
    {
        // Arrange
        string rollcallId = "test-rollcall-456";
        Mock<IApiService> mockApiService = new Mock<IApiService>();
        Mock<ILogger<RollcallService>> mockLogger = new Mock<ILogger<RollcallService>>();

        HttpResponseMessage response = new HttpResponseMessage(statusCode);
        mockApiService
            .Setup(api => api.PostAsync($"rollcall/{rollcallId}/approve", It.IsAny<HttpContent>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        RollcallService service = new RollcallService(mockApiService.Object, mockLogger.Object);

        // Act
        bool result = await service.ApproveRollcallAsync(rollcallId);

        // Assert
        Assert.IsFalse(result);
    }

    /// <summary>
    /// Tests that ApproveRollcallAsync returns false for 5xx server error status codes.
    /// Input: Valid rollcallId with various 5xx HTTP server error status codes.
    /// Expected: Returns false for all 5xx status codes.
    /// </summary>
    [TestMethod]
    [DataRow(HttpStatusCode.InternalServerError, DisplayName = "500 InternalServerError")]
    [DataRow(HttpStatusCode.NotImplemented, DisplayName = "501 NotImplemented")]
    [DataRow(HttpStatusCode.BadGateway, DisplayName = "502 BadGateway")]
    [DataRow(HttpStatusCode.ServiceUnavailable, DisplayName = "503 ServiceUnavailable")]
    [DataRow(HttpStatusCode.GatewayTimeout, DisplayName = "504 GatewayTimeout")]
    public async Task ApproveRollcallAsync_ServerError5xxStatusCodes_ReturnsFalse(HttpStatusCode statusCode)
    {
        // Arrange
        string rollcallId = "test-rollcall-789";
        Mock<IApiService> mockApiService = new Mock<IApiService>();
        Mock<ILogger<RollcallService>> mockLogger = new Mock<ILogger<RollcallService>>();

        HttpResponseMessage response = new HttpResponseMessage(statusCode);
        mockApiService
            .Setup(api => api.PostAsync($"rollcall/{rollcallId}/approve", It.IsAny<HttpContent>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        RollcallService service = new RollcallService(mockApiService.Object, mockLogger.Object);

        // Act
        bool result = await service.ApproveRollcallAsync(rollcallId);

        // Assert
        Assert.IsFalse(result);
    }

    /// <summary>
    /// Tests that ApproveRollcallAsync returns false for 3xx redirect status codes.
    /// Input: Valid rollcallId with various 3xx HTTP redirect status codes.
    /// Expected: Returns false for all 3xx status codes.
    /// </summary>
    [TestMethod]
    [DataRow(HttpStatusCode.MultipleChoices, DisplayName = "300 MultipleChoices")]
    [DataRow(HttpStatusCode.MovedPermanently, DisplayName = "301 MovedPermanently")]
    [DataRow(HttpStatusCode.Found, DisplayName = "302 Found")]
    [DataRow(HttpStatusCode.NotModified, DisplayName = "304 NotModified")]
    [DataRow(HttpStatusCode.TemporaryRedirect, DisplayName = "307 TemporaryRedirect")]
    public async Task ApproveRollcallAsync_Redirect3xxStatusCodes_ReturnsFalse(HttpStatusCode statusCode)
    {
        // Arrange
        string rollcallId = "test-rollcall-redirect";
        Mock<IApiService> mockApiService = new Mock<IApiService>();
        Mock<ILogger<RollcallService>> mockLogger = new Mock<ILogger<RollcallService>>();

        HttpResponseMessage response = new HttpResponseMessage(statusCode);
        mockApiService
            .Setup(api => api.PostAsync($"rollcall/{rollcallId}/approve", It.IsAny<HttpContent>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        RollcallService service = new RollcallService(mockApiService.Object, mockLogger.Object);

        // Act
        bool result = await service.ApproveRollcallAsync(rollcallId);

        // Assert
        Assert.IsFalse(result);
    }

    /// <summary>
    /// Tests that ApproveRollcallAsync returns false for 1xx informational status codes.
    /// Input: Valid rollcallId with 1xx HTTP informational status codes.
    /// Expected: Returns false for all 1xx status codes.
    /// </summary>
    [TestMethod]
    [DataRow(HttpStatusCode.Continue, DisplayName = "100 Continue")]
    [DataRow(HttpStatusCode.SwitchingProtocols, DisplayName = "101 SwitchingProtocols")]
    public async Task ApproveRollcallAsync_Informational1xxStatusCodes_ReturnsFalse(HttpStatusCode statusCode)
    {
        // Arrange
        string rollcallId = "test-rollcall-informational";
        Mock<IApiService> mockApiService = new Mock<IApiService>();
        Mock<ILogger<RollcallService>> mockLogger = new Mock<ILogger<RollcallService>>();

        HttpResponseMessage response = new HttpResponseMessage(statusCode);
        mockApiService
            .Setup(api => api.PostAsync($"rollcall/{rollcallId}/approve", It.IsAny<HttpContent>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        RollcallService service = new RollcallService(mockApiService.Object, mockLogger.Object);

        // Act
        bool result = await service.ApproveRollcallAsync(rollcallId);

        // Assert
        Assert.IsFalse(result);
    }

    /// <summary>
    /// Tests that ApproveRollcallAsync handles null rollcallId by using it in the endpoint URL.
    /// Input: null rollcallId (even though parameter is non-nullable).
    /// Expected: Method executes and constructs endpoint with null in the URL.
    /// </summary>
    [TestMethod]
    public async Task ApproveRollcallAsync_NullRollcallId_ExecutesSuccessfully()
    {
        // Arrange
        string? rollcallId = null;
        Mock<IApiService> mockApiService = new Mock<IApiService>();
        Mock<ILogger<RollcallService>> mockLogger = new Mock<ILogger<RollcallService>>();

        HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.OK);
        mockApiService
            .Setup(api => api.PostAsync(It.IsAny<string>(), It.IsAny<HttpContent>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        RollcallService service = new RollcallService(mockApiService.Object, mockLogger.Object);

        // Act
        bool result = await service.ApproveRollcallAsync(rollcallId!);

        // Assert
        Assert.IsTrue(result);
        mockApiService.Verify(api => api.PostAsync(It.IsAny<string>(), It.IsAny<HttpContent>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Tests that ApproveRollcallAsync handles empty string rollcallId.
    /// Input: Empty string rollcallId.
    /// Expected: Method constructs endpoint with empty segment "rollcall//approve".
    /// </summary>
    [TestMethod]
    public async Task ApproveRollcallAsync_EmptyStringRollcallId_ConstructsEndpointCorrectly()
    {
        // Arrange
        string rollcallId = string.Empty;
        Mock<IApiService> mockApiService = new Mock<IApiService>();
        Mock<ILogger<RollcallService>> mockLogger = new Mock<ILogger<RollcallService>>();

        HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.OK);
        mockApiService
            .Setup(api => api.PostAsync("rollcall//approve", It.IsAny<HttpContent>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        RollcallService service = new RollcallService(mockApiService.Object, mockLogger.Object);

        // Act
        bool result = await service.ApproveRollcallAsync(rollcallId);

        // Assert
        Assert.IsTrue(result);
        mockApiService.Verify(api => api.PostAsync("rollcall//approve", It.IsAny<HttpContent>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Tests that ApproveRollcallAsync handles whitespace-only rollcallId.
    /// Input: Whitespace-only rollcallId (spaces, tabs, newlines).
    /// Expected: Method includes whitespace in the endpoint URL.
    /// </summary>
    [TestMethod]
    [DataRow("   ", DisplayName = "Three spaces")]
    [DataRow("\t", DisplayName = "Tab character")]
    [DataRow("\n", DisplayName = "Newline character")]
    [DataRow(" \t\n ", DisplayName = "Mixed whitespace")]
    public async Task ApproveRollcallAsync_WhitespaceRollcallId_IncludesInEndpoint(string rollcallId)
    {
        // Arrange
        Mock<IApiService> mockApiService = new Mock<IApiService>();
        Mock<ILogger<RollcallService>> mockLogger = new Mock<ILogger<RollcallService>>();

        HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.OK);
        mockApiService
            .Setup(api => api.PostAsync($"rollcall/{rollcallId}/approve", It.IsAny<HttpContent>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        RollcallService service = new RollcallService(mockApiService.Object, mockLogger.Object);

        // Act
        bool result = await service.ApproveRollcallAsync(rollcallId);

        // Assert
        Assert.IsTrue(result);
    }

    /// <summary>
    /// Tests that ApproveRollcallAsync handles rollcallId with special URL characters.
    /// Input: RollcallId containing special characters that might affect URL construction.
    /// Expected: Method includes special characters in the endpoint URL without encoding.
    /// </summary>
    [TestMethod]
    [DataRow("rollcall/123", DisplayName = "Forward slash")]
    [DataRow("rollcall?query=1", DisplayName = "Question mark and equals")]
    [DataRow("rollcall#anchor", DisplayName = "Hash character")]
    [DataRow("rollcall&param=value", DisplayName = "Ampersand and equals")]
    [DataRow("rollcall with spaces", DisplayName = "Spaces")]
    [DataRow("rollcall%20encoded", DisplayName = "Percent encoding")]
    public async Task ApproveRollcallAsync_SpecialCharactersInRollcallId_IncludesInEndpoint(string rollcallId)
    {
        // Arrange
        Mock<IApiService> mockApiService = new Mock<IApiService>();
        Mock<ILogger<RollcallService>> mockLogger = new Mock<ILogger<RollcallService>>();

        HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.OK);
        mockApiService
            .Setup(api => api.PostAsync($"rollcall/{rollcallId}/approve", It.IsAny<HttpContent>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        RollcallService service = new RollcallService(mockApiService.Object, mockLogger.Object);

        // Act
        bool result = await service.ApproveRollcallAsync(rollcallId);

        // Assert
        Assert.IsTrue(result);
    }

    /// <summary>
    /// Tests that ApproveRollcallAsync handles Unicode characters in rollcallId.
    /// Input: RollcallId containing various Unicode characters.
    /// Expected: Method includes Unicode characters in the endpoint URL.
    /// </summary>
    [TestMethod]
    [DataRow("rollcall-日本語", DisplayName = "Japanese characters")]
    [DataRow("rollcall-עברית", DisplayName = "Hebrew characters")]
    [DataRow("rollcall-العربية", DisplayName = "Arabic characters")]
    [DataRow("rollcall-🎉emoji", DisplayName = "Emoji characters")]
    public async Task ApproveRollcallAsync_UnicodeRollcallId_IncludesInEndpoint(string rollcallId)
    {
        // Arrange
        Mock<IApiService> mockApiService = new Mock<IApiService>();
        Mock<ILogger<RollcallService>> mockLogger = new Mock<ILogger<RollcallService>>();

        HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.OK);
        mockApiService
            .Setup(api => api.PostAsync($"rollcall/{rollcallId}/approve", It.IsAny<HttpContent>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        RollcallService service = new RollcallService(mockApiService.Object, mockLogger.Object);

        // Act
        bool result = await service.ApproveRollcallAsync(rollcallId);

        // Assert
        Assert.IsTrue(result);
    }

    /// <summary>
    /// Tests that ApproveRollcallAsync sends JsonContent with empty object.
    /// Input: Valid rollcallId.
    /// Expected: PostAsync is called with JsonContent containing empty object.
    /// </summary>
    [TestMethod]
    public async Task ApproveRollcallAsync_ValidRequest_SendsJsonContentWithEmptyObject()
    {
        // Arrange
        string rollcallId = "test-rollcall-content";
        Mock<IApiService> mockApiService = new Mock<IApiService>();
        Mock<ILogger<RollcallService>> mockLogger = new Mock<ILogger<RollcallService>>();

        HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.OK);
        mockApiService
            .Setup(api => api.PostAsync(It.IsAny<string>(), It.IsAny<HttpContent>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        RollcallService service = new RollcallService(mockApiService.Object, mockLogger.Object);

        // Act
        bool result = await service.ApproveRollcallAsync(rollcallId);

        // Assert
        Assert.IsTrue(result);
        mockApiService.Verify(api => api.PostAsync(
            It.IsAny<string>(),
            It.Is<HttpContent>(content => content is JsonContent),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Tests that ApproveRollcallAsync calls the API exactly once.
    /// Input: Valid rollcallId.
    /// Expected: PostAsync is invoked exactly once.
    /// </summary>
    [TestMethod]
    public async Task ApproveRollcallAsync_ValidRequest_CallsApiExactlyOnce()
    {
        // Arrange
        string rollcallId = "test-rollcall-verify";
        Mock<IApiService> mockApiService = new Mock<IApiService>();
        Mock<ILogger<RollcallService>> mockLogger = new Mock<ILogger<RollcallService>>();

        HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.OK);
        mockApiService
            .Setup(api => api.PostAsync(It.IsAny<string>(), It.IsAny<HttpContent>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        RollcallService service = new RollcallService(mockApiService.Object, mockLogger.Object);

        // Act
        bool result = await service.ApproveRollcallAsync(rollcallId);

        // Assert
        Assert.IsTrue(result);
        mockApiService.Verify(api => api.PostAsync(It.IsAny<string>(), It.IsAny<HttpContent>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Tests that ApproveRollcallAsync handles boundary status codes at 2xx/3xx transition.
    /// Input: Status codes at the boundary between success and redirect (299, 300).
    /// Expected: 299 returns true (last 2xx), 300 returns false (first 3xx).
    /// </summary>
    [TestMethod]
    [DataRow((HttpStatusCode)199, false, DisplayName = "199 (before 2xx range)")]
    [DataRow((HttpStatusCode)200, true, DisplayName = "200 (first 2xx)")]
    [DataRow((HttpStatusCode)299, true, DisplayName = "299 (last 2xx)")]
    [DataRow((HttpStatusCode)300, false, DisplayName = "300 (first 3xx)")]
    public async Task ApproveRollcallAsync_BoundaryStatusCodes_ReturnsExpectedResult(HttpStatusCode statusCode, bool expectedResult)
    {
        // Arrange
        string rollcallId = "test-rollcall-boundary";
        Mock<IApiService> mockApiService = new Mock<IApiService>();
        Mock<ILogger<RollcallService>> mockLogger = new Mock<ILogger<RollcallService>>();

        HttpResponseMessage response = new HttpResponseMessage(statusCode);
        mockApiService
            .Setup(api => api.PostAsync($"rollcall/{rollcallId}/approve", It.IsAny<HttpContent>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        RollcallService service = new RollcallService(mockApiService.Object, mockLogger.Object);

        // Act
        bool result = await service.ApproveRollcallAsync(rollcallId);

        // Assert
        Assert.AreEqual(expectedResult, result);
    }

    /// <summary>
    /// Tests that SubmitRollcallAsync returns true when the API returns a 200 OK status code.
    /// Input: Valid parameters with successful API response (200 OK).
    /// Expected: Returns true and does not log a warning.
    /// </summary>
    [TestMethod]
    public async Task SubmitRollcallAsync_SuccessResponseOK_ReturnsTrue()
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<RollcallService>>();
        var service = new RollcallService(mockApi.Object, mockLogger.Object);

        var scheduleId = "schedule123";
        var fileName = "selfie.jpg";
        using var stream = new MemoryStream();

        var successResponse = new HttpResponseMessage(HttpStatusCode.OK);
        mockApi.Setup(x => x.PostAsync("rollcall/submit", It.IsAny<MultipartFormDataContent>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(successResponse);

        // Act
        var result = await service.SubmitRollcallAsync(scheduleId, stream, fileName);

        // Assert
        Assert.IsTrue(result);
    }

    /// <summary>
    /// Tests that SubmitRollcallAsync returns true for all 2xx success status codes.
    /// Input: Valid parameters with various 2xx HTTP status codes.
    /// Expected: Returns true for all 2xx status codes.
    /// </summary>
    [TestMethod]
    [DataRow(HttpStatusCode.OK)]
    [DataRow(HttpStatusCode.Created)]
    [DataRow(HttpStatusCode.Accepted)]
    [DataRow(HttpStatusCode.NoContent)]
    [DataRow(HttpStatusCode.PartialContent)]
    public async Task SubmitRollcallAsync_Various2xxStatusCodes_ReturnsTrue(HttpStatusCode statusCode)
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<RollcallService>>();
        var service = new RollcallService(mockApi.Object, mockLogger.Object);

        var scheduleId = "schedule123";
        var fileName = "selfie.jpg";
        using var stream = new MemoryStream();

        var successResponse = new HttpResponseMessage(statusCode);
        mockApi.Setup(x => x.PostAsync("rollcall/submit", It.IsAny<MultipartFormDataContent>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(successResponse);

        // Act
        var result = await service.SubmitRollcallAsync(scheduleId, stream, fileName);

        // Assert
        Assert.IsTrue(result);
    }

    /// <summary>
    /// Tests that SubmitRollcallAsync returns false and logs a warning for 4xx client error status codes.
    /// Input: Valid parameters with 4xx HTTP status codes.
    /// Expected: Returns false and logs warning with status code.
    /// </summary>
    [TestMethod]
    [DataRow(HttpStatusCode.BadRequest)]
    [DataRow(HttpStatusCode.Unauthorized)]
    [DataRow(HttpStatusCode.Forbidden)]
    [DataRow(HttpStatusCode.NotFound)]
    [DataRow(HttpStatusCode.MethodNotAllowed)]
    [DataRow(HttpStatusCode.Conflict)]
    [DataRow(HttpStatusCode.Gone)]
    public async Task SubmitRollcallAsync_4xxClientErrors_ReturnsFalseAndLogsWarning(HttpStatusCode statusCode)
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<RollcallService>>();
        var service = new RollcallService(mockApi.Object, mockLogger.Object);

        var scheduleId = "schedule123";
        var fileName = "selfie.jpg";
        using var stream = new MemoryStream();

        var failureResponse = new HttpResponseMessage(statusCode);
        mockApi.Setup(x => x.PostAsync("rollcall/submit", It.IsAny<MultipartFormDataContent>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(failureResponse);

        // Act
        var result = await service.SubmitRollcallAsync(scheduleId, stream, fileName);

        // Assert
        Assert.IsFalse(result);
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("SubmitRollcall returned")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    /// <summary>
    /// Tests that SubmitRollcallAsync returns false and logs a warning for 5xx server error status codes.
    /// Input: Valid parameters with 5xx HTTP status codes.
    /// Expected: Returns false and logs warning with status code.
    /// </summary>
    [TestMethod]
    [DataRow(HttpStatusCode.InternalServerError)]
    [DataRow(HttpStatusCode.NotImplemented)]
    [DataRow(HttpStatusCode.BadGateway)]
    [DataRow(HttpStatusCode.ServiceUnavailable)]
    [DataRow(HttpStatusCode.GatewayTimeout)]
    public async Task SubmitRollcallAsync_5xxServerErrors_ReturnsFalseAndLogsWarning(HttpStatusCode statusCode)
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<RollcallService>>();
        var service = new RollcallService(mockApi.Object, mockLogger.Object);

        var scheduleId = "schedule123";
        var fileName = "selfie.jpg";
        using var stream = new MemoryStream();

        var failureResponse = new HttpResponseMessage(statusCode);
        mockApi.Setup(x => x.PostAsync("rollcall/submit", It.IsAny<MultipartFormDataContent>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(failureResponse);

        // Act
        var result = await service.SubmitRollcallAsync(scheduleId, stream, fileName);

        // Assert
        Assert.IsFalse(result);
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("SubmitRollcall returned") && v.ToString()!.Contains(statusCode.ToString())),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    /// <summary>
    /// Tests that SubmitRollcallAsync returns false and logs a warning for 3xx redirect status codes.
    /// Input: Valid parameters with 3xx HTTP status codes.
    /// Expected: Returns false and logs warning with status code.
    /// </summary>
    [TestMethod]
    [DataRow(HttpStatusCode.MovedPermanently)]
    [DataRow(HttpStatusCode.Found)]
    [DataRow(HttpStatusCode.NotModified)]
    [DataRow(HttpStatusCode.TemporaryRedirect)]
    public async Task SubmitRollcallAsync_3xxRedirects_ReturnsFalseAndLogsWarning(HttpStatusCode statusCode)
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<RollcallService>>();
        var service = new RollcallService(mockApi.Object, mockLogger.Object);

        var scheduleId = "schedule123";
        var fileName = "selfie.jpg";
        using var stream = new MemoryStream();

        var redirectResponse = new HttpResponseMessage(statusCode);
        mockApi.Setup(x => x.PostAsync("rollcall/submit", It.IsAny<MultipartFormDataContent>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(redirectResponse);

        // Act
        var result = await service.SubmitRollcallAsync(scheduleId, stream, fileName);

        // Assert
        Assert.IsFalse(result);
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("SubmitRollcall returned")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    /// <summary>
    /// Tests that SubmitRollcallAsync handles empty scheduleId parameter.
    /// Input: Empty string for scheduleId.
    /// Expected: Calls API successfully and returns based on API response.
    /// </summary>
    [TestMethod]
    public async Task SubmitRollcallAsync_EmptyScheduleId_CallsApiSuccessfully()
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<RollcallService>>();
        var service = new RollcallService(mockApi.Object, mockLogger.Object);

        var scheduleId = string.Empty;
        var fileName = "selfie.jpg";
        using var stream = new MemoryStream();

        var successResponse = new HttpResponseMessage(HttpStatusCode.OK);
        mockApi.Setup(x => x.PostAsync("rollcall/submit", It.IsAny<MultipartFormDataContent>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(successResponse);

        // Act
        var result = await service.SubmitRollcallAsync(scheduleId, stream, fileName);

        // Assert
        Assert.IsTrue(result);
        mockApi.Verify(x => x.PostAsync("rollcall/submit", It.IsAny<MultipartFormDataContent>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Tests that SubmitRollcallAsync handles whitespace-only scheduleId parameter.
    /// Input: Whitespace string for scheduleId.
    /// Expected: Calls API successfully and returns based on API response.
    /// </summary>
    [TestMethod]
    [DataRow("   ")]
    [DataRow("\t")]
    [DataRow("\n")]
    [DataRow(" \t\n ")]
    public async Task SubmitRollcallAsync_WhitespaceScheduleId_CallsApiSuccessfully(string scheduleId)
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<RollcallService>>();
        var service = new RollcallService(mockApi.Object, mockLogger.Object);

        var fileName = "selfie.jpg";
        using var stream = new MemoryStream();

        var successResponse = new HttpResponseMessage(HttpStatusCode.OK);
        mockApi.Setup(x => x.PostAsync("rollcall/submit", It.IsAny<MultipartFormDataContent>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(successResponse);

        // Act
        var result = await service.SubmitRollcallAsync(scheduleId, stream, fileName);

        // Assert
        Assert.IsTrue(result);
        mockApi.Verify(x => x.PostAsync("rollcall/submit", It.IsAny<MultipartFormDataContent>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Tests that SubmitRollcallAsync handles scheduleId with special URL characters.
    /// Input: ScheduleId containing special characters like /, ?, &, #, etc.
    /// Expected: Calls API successfully without URL encoding.
    /// </summary>
    [TestMethod]
    [DataRow("schedule/123")]
    [DataRow("schedule?query=1")]
    [DataRow("schedule&param=value")]
    [DataRow("schedule#anchor")]
    [DataRow("schedule with spaces")]
    [DataRow("schedule%20encoded")]
    public async Task SubmitRollcallAsync_ScheduleIdWithSpecialCharacters_CallsApiSuccessfully(string scheduleId)
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<RollcallService>>();
        var service = new RollcallService(mockApi.Object, mockLogger.Object);

        var fileName = "selfie.jpg";
        using var stream = new MemoryStream();

        var successResponse = new HttpResponseMessage(HttpStatusCode.OK);
        mockApi.Setup(x => x.PostAsync("rollcall/submit", It.IsAny<MultipartFormDataContent>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(successResponse);

        // Act
        var result = await service.SubmitRollcallAsync(scheduleId, stream, fileName);

        // Assert
        Assert.IsTrue(result);
        mockApi.Verify(x => x.PostAsync("rollcall/submit", It.IsAny<MultipartFormDataContent>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Tests that SubmitRollcallAsync handles scheduleId with Unicode characters.
    /// Input: ScheduleId containing Unicode characters from various languages.
    /// Expected: Calls API successfully with Unicode characters.
    /// </summary>
    [TestMethod]
    [DataRow("schedule-日本語")]
    [DataRow("schedule-עברית")]
    [DataRow("schedule-العربية")]
    [DataRow("schedule-🎉emoji")]
    public async Task SubmitRollcallAsync_ScheduleIdWithUnicode_CallsApiSuccessfully(string scheduleId)
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<RollcallService>>();
        var service = new RollcallService(mockApi.Object, mockLogger.Object);

        var fileName = "selfie.jpg";
        using var stream = new MemoryStream();

        var successResponse = new HttpResponseMessage(HttpStatusCode.OK);
        mockApi.Setup(x => x.PostAsync("rollcall/submit", It.IsAny<MultipartFormDataContent>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(successResponse);

        // Act
        var result = await service.SubmitRollcallAsync(scheduleId, stream, fileName);

        // Assert
        Assert.IsTrue(result);
        mockApi.Verify(x => x.PostAsync("rollcall/submit", It.IsAny<MultipartFormDataContent>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Tests that SubmitRollcallAsync handles empty fileName parameter.
    /// Input: Empty string for fileName.
    /// Expected: Calls API successfully and returns based on API response.
    /// </summary>
    [TestMethod]
    public async Task SubmitRollcallAsync_EmptyFileName_CallsApiSuccessfully()
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<RollcallService>>();
        var service = new RollcallService(mockApi.Object, mockLogger.Object);

        var scheduleId = "schedule123";
        var fileName = string.Empty;
        using var stream = new MemoryStream();

        var successResponse = new HttpResponseMessage(HttpStatusCode.OK);
        mockApi.Setup(x => x.PostAsync("rollcall/submit", It.IsAny<MultipartFormDataContent>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(successResponse);

        // Act
        var result = await service.SubmitRollcallAsync(scheduleId, stream, fileName);

        // Assert
        Assert.IsTrue(result);
        mockApi.Verify(x => x.PostAsync("rollcall/submit", It.IsAny<MultipartFormDataContent>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Tests that SubmitRollcallAsync handles whitespace-only fileName parameter.
    /// Input: Whitespace string for fileName.
    /// Expected: Calls API successfully and returns based on API response.
    /// </summary>
    [TestMethod]
    [DataRow("   ")]
    [DataRow("\t")]
    [DataRow("\n")]
    public async Task SubmitRollcallAsync_WhitespaceFileName_CallsApiSuccessfully(string fileName)
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<RollcallService>>();
        var service = new RollcallService(mockApi.Object, mockLogger.Object);

        var scheduleId = "schedule123";
        using var stream = new MemoryStream();

        var successResponse = new HttpResponseMessage(HttpStatusCode.OK);
        mockApi.Setup(x => x.PostAsync("rollcall/submit", It.IsAny<MultipartFormDataContent>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(successResponse);

        // Act
        var result = await service.SubmitRollcallAsync(scheduleId, stream, fileName);

        // Assert
        Assert.IsTrue(result);
        mockApi.Verify(x => x.PostAsync("rollcall/submit", It.IsAny<MultipartFormDataContent>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Tests that SubmitRollcallAsync handles fileName with special characters and path components.
    /// Input: FileName containing path separators, special characters, and extensions.
    /// Expected: Calls API successfully with special characters.
    /// </summary>
    [TestMethod]
    [DataRow(@"C:\path\to\file.jpg")]
    [DataRow("../relative/path.jpg")]
    [DataRow("file name with spaces.jpg")]
    [DataRow("file@#$%^&().jpg")]
    [DataRow("file|pipe<>quote\".jpg")]
    public async Task SubmitRollcallAsync_FileNameWithSpecialCharacters_CallsApiSuccessfully(string fileName)
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<RollcallService>>();
        var service = new RollcallService(mockApi.Object, mockLogger.Object);

        var scheduleId = "schedule123";
        using var stream = new MemoryStream();

        var successResponse = new HttpResponseMessage(HttpStatusCode.OK);
        mockApi.Setup(x => x.PostAsync("rollcall/submit", It.IsAny<MultipartFormDataContent>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(successResponse);

        // Act
        var result = await service.SubmitRollcallAsync(scheduleId, stream, fileName);

        // Assert
        Assert.IsTrue(result);
        mockApi.Verify(x => x.PostAsync("rollcall/submit", It.IsAny<MultipartFormDataContent>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Tests that SubmitRollcallAsync handles fileName with Unicode characters.
    /// Input: FileName containing Unicode characters from various languages.
    /// Expected: Calls API successfully with Unicode characters.
    /// </summary>
    [TestMethod]
    [DataRow("selfie-日本語.jpg")]
    [DataRow("selfie-עברית.jpg")]
    [DataRow("selfie-العربية.jpg")]
    [DataRow("selfie-🎉emoji.jpg")]
    public async Task SubmitRollcallAsync_FileNameWithUnicode_CallsApiSuccessfully(string fileName)
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<RollcallService>>();
        var service = new RollcallService(mockApi.Object, mockLogger.Object);

        var scheduleId = "schedule123";
        using var stream = new MemoryStream();

        var successResponse = new HttpResponseMessage(HttpStatusCode.OK);
        mockApi.Setup(x => x.PostAsync("rollcall/submit", It.IsAny<MultipartFormDataContent>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(successResponse);

        // Act
        var result = await service.SubmitRollcallAsync(scheduleId, stream, fileName);

        // Assert
        Assert.IsTrue(result);
        mockApi.Verify(x => x.PostAsync("rollcall/submit", It.IsAny<MultipartFormDataContent>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Tests that SubmitRollcallAsync handles a stream positioned at non-zero offset.
    /// Input: MemoryStream with data, positioned in the middle.
    /// Expected: Calls API successfully using the stream from its current position.
    /// </summary>
    [TestMethod]
    public async Task SubmitRollcallAsync_StreamAtNonZeroPosition_CallsApiSuccessfully()
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<RollcallService>>();
        var service = new RollcallService(mockApi.Object, mockLogger.Object);

        var scheduleId = "schedule123";
        var fileName = "selfie.jpg";
        var data = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
        using var stream = new MemoryStream(data);
        stream.Position = 5;

        var successResponse = new HttpResponseMessage(HttpStatusCode.OK);
        mockApi.Setup(x => x.PostAsync("rollcall/submit", It.IsAny<MultipartFormDataContent>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(successResponse);

        // Act
        var result = await service.SubmitRollcallAsync(scheduleId, stream, fileName);

        // Assert
        Assert.IsTrue(result);
        Assert.AreEqual(5, stream.Position);
        mockApi.Verify(x => x.PostAsync("rollcall/submit", It.IsAny<MultipartFormDataContent>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Tests that SubmitRollcallAsync sends MultipartFormDataContent to the API.
    /// Input: Valid parameters.
    /// Expected: PostAsync is called with MultipartFormDataContent.
    /// </summary>
    [TestMethod]
    public async Task SubmitRollcallAsync_ValidInputs_SendsMultipartFormDataContent()
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<RollcallService>>();
        var service = new RollcallService(mockApi.Object, mockLogger.Object);

        var scheduleId = "schedule123";
        var fileName = "selfie.jpg";
        using var stream = new MemoryStream();

        var successResponse = new HttpResponseMessage(HttpStatusCode.OK);
        mockApi.Setup(x => x.PostAsync(It.IsAny<string>(), It.IsAny<MultipartFormDataContent>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(successResponse);

        // Act
        await service.SubmitRollcallAsync(scheduleId, stream, fileName);

        // Assert
        mockApi.Verify(x => x.PostAsync("rollcall/submit", It.IsAny<MultipartFormDataContent>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Tests that SubmitRollcallAsync handles boundary HTTP status codes correctly.
    /// Input: Boundary status codes between success and failure ranges.
    /// Expected: Status codes 199 and 300 return false; 200 and 299 return true.
    /// </summary>
    [TestMethod]
    [DataRow((HttpStatusCode)199, false)]
    [DataRow(HttpStatusCode.OK, true)]
    [DataRow((HttpStatusCode)299, true)]
    [DataRow((HttpStatusCode)300, false)]
    public async Task SubmitRollcallAsync_BoundaryStatusCodes_ReturnsExpectedResult(HttpStatusCode statusCode, bool expectedResult)
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<RollcallService>>();
        var service = new RollcallService(mockApi.Object, mockLogger.Object);

        var scheduleId = "schedule123";
        var fileName = "selfie.jpg";
        using var stream = new MemoryStream();

        var response = new HttpResponseMessage(statusCode);
        mockApi.Setup(x => x.PostAsync("rollcall/submit", It.IsAny<MultipartFormDataContent>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        // Act
        var result = await service.SubmitRollcallAsync(scheduleId, stream, fileName);

        // Assert
        Assert.AreEqual(expectedResult, result);
    }

    /// <summary>
    /// Tests that SubmitRollcallAsync handles combined edge case inputs.
    /// Input: Empty scheduleId, empty fileName, and empty stream.
    /// Expected: Calls API successfully and returns based on API response.
    /// </summary>
    [TestMethod]
    public async Task SubmitRollcallAsync_AllEmptyInputs_CallsApiSuccessfully()
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<RollcallService>>();
        var service = new RollcallService(mockApi.Object, mockLogger.Object);

        var scheduleId = string.Empty;
        var fileName = string.Empty;
        using var stream = new MemoryStream();

        var successResponse = new HttpResponseMessage(HttpStatusCode.OK);
        mockApi.Setup(x => x.PostAsync("rollcall/submit", It.IsAny<MultipartFormDataContent>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(successResponse);

        // Act
        var result = await service.SubmitRollcallAsync(scheduleId, stream, fileName);

        // Assert
        Assert.IsTrue(result);
        mockApi.Verify(x => x.PostAsync("rollcall/submit", It.IsAny<MultipartFormDataContent>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Tests that SubmitRollcallAsync handles combined edge case inputs with special characters.
    /// Input: ScheduleId and fileName with special characters, stream with data.
    /// Expected: Calls API successfully and returns based on API response.
    /// </summary>
    [TestMethod]
    public async Task SubmitRollcallAsync_AllSpecialCharacterInputs_CallsApiSuccessfully()
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<RollcallService>>();
        var service = new RollcallService(mockApi.Object, mockLogger.Object);

        var scheduleId = "schedule/123?query=1&param=value";
        var fileName = "file name with spaces & special chars.jpg";
        var data = new byte[] { 0xFF, 0xD8, 0xFF, 0xE0 };
        using var stream = new MemoryStream(data);

        var successResponse = new HttpResponseMessage(HttpStatusCode.OK);
        mockApi.Setup(x => x.PostAsync("rollcall/submit", It.IsAny<MultipartFormDataContent>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(successResponse);

        // Act
        var result = await service.SubmitRollcallAsync(scheduleId, stream, fileName);

        // Assert
        Assert.IsTrue(result);
        mockApi.Verify(x => x.PostAsync("rollcall/submit", It.IsAny<MultipartFormDataContent>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Tests that SubmitRollcallAsync logs warning with LogLevel.Warning.
    /// Input: Valid parameters with failure response.
    /// Expected: Logger is called with LogLevel.Warning.
    /// </summary>
    [TestMethod]
    public async Task SubmitRollcallAsync_FailureResponse_LogsWithWarningLevel()
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<RollcallService>>();
        var service = new RollcallService(mockApi.Object, mockLogger.Object);

        var scheduleId = "schedule123";
        var fileName = "selfie.jpg";
        using var stream = new MemoryStream();

        var failureResponse = new HttpResponseMessage(HttpStatusCode.BadRequest);
        mockApi.Setup(x => x.PostAsync("rollcall/submit", It.IsAny<MultipartFormDataContent>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(failureResponse);

        // Act
        await service.SubmitRollcallAsync(scheduleId, stream, fileName);

        // Assert
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    /// <summary>
    /// Tests that SubmitRollcallAsync logs exactly once when the API returns a failure response.
    /// Input: Valid parameters with failure response.
    /// Expected: Logger is called exactly once.
    /// </summary>
    [TestMethod]
    public async Task SubmitRollcallAsync_FailureResponse_LogsExactlyOnce()
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<RollcallService>>();
        var service = new RollcallService(mockApi.Object, mockLogger.Object);

        var scheduleId = "schedule123";
        var fileName = "selfie.jpg";
        using var stream = new MemoryStream();

        var failureResponse = new HttpResponseMessage(HttpStatusCode.InternalServerError);
        mockApi.Setup(x => x.PostAsync("rollcall/submit", It.IsAny<MultipartFormDataContent>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(failureResponse);

        // Act
        await service.SubmitRollcallAsync(scheduleId, stream, fileName);

        // Assert
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    /// <summary>
    /// Tests that SubmitRollcallAsync calls PostAsync exactly once for any status code.
    /// Input: Various status codes.
    /// Expected: PostAsync is called exactly once regardless of status code.
    /// </summary>
    [TestMethod]
    [DataRow(HttpStatusCode.OK)]
    [DataRow(HttpStatusCode.BadRequest)]
    [DataRow(HttpStatusCode.InternalServerError)]
    public async Task SubmitRollcallAsync_AnyStatusCode_CallsPostAsyncOnce(HttpStatusCode statusCode)
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<RollcallService>>();
        var service = new RollcallService(mockApi.Object, mockLogger.Object);

        var scheduleId = "schedule123";
        var fileName = "selfie.jpg";
        using var stream = new MemoryStream();

        var response = new HttpResponseMessage(statusCode);
        mockApi.Setup(x => x.PostAsync("rollcall/submit", It.IsAny<MultipartFormDataContent>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        // Act
        await service.SubmitRollcallAsync(scheduleId, stream, fileName);

        // Assert
        mockApi.Verify(x => x.PostAsync("rollcall/submit", It.IsAny<MultipartFormDataContent>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}