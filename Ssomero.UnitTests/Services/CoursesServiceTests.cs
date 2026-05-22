using System;
using System.Collections;
using System.Collections.Generic;
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
/// Unit tests for the <see cref="CoursesService"/> class.
/// </summary>
[TestClass]
public class CoursesServiceTests
{
    /// <summary>
    /// Tests that the constructor successfully creates an instance when valid dependencies are provided.
    /// </summary>
    [TestMethod]
    public void Constructor_WithValidDependencies_CreatesInstance()
    {
        // Arrange
        Mock<IApiService> mockApiService = new Mock<IApiService>();
        Mock<ILogger<CoursesService>> mockLogger = new Mock<ILogger<CoursesService>>();

        // Act
        CoursesService service = new CoursesService(mockApiService.Object, mockLogger.Object);

        // Assert
        Assert.IsNotNull(service);
    }

    /// <summary>
    /// Tests that the constructor accepts a null api parameter.
    /// Note: This documents the current behavior where no validation is performed.
    /// Ideally, the constructor should throw ArgumentNullException for null dependencies.
    /// </summary>
    [TestMethod]
    public void Constructor_WithNullApi_DoesNotThrow()
    {
        // Arrange
        Mock<ILogger<CoursesService>> mockLogger = new Mock<ILogger<CoursesService>>();

        // Act
        CoursesService service = new CoursesService(null!, mockLogger.Object);

        // Assert
        Assert.IsNotNull(service);
    }

    /// <summary>
    /// Tests that the constructor accepts a null logger parameter.
    /// Note: This documents the current behavior where no validation is performed.
    /// Ideally, the constructor should throw ArgumentNullException for null dependencies.
    /// </summary>
    [TestMethod]
    public void Constructor_WithNullLogger_DoesNotThrow()
    {
        // Arrange
        Mock<IApiService> mockApiService = new Mock<IApiService>();

        // Act
        CoursesService service = new CoursesService(mockApiService.Object, null!);

        // Assert
        Assert.IsNotNull(service);
    }

    /// <summary>
    /// Tests that the constructor accepts null for both parameters.
    /// Note: This documents the current behavior where no validation is performed.
    /// Ideally, the constructor should throw ArgumentNullException for null dependencies.
    /// </summary>
    [TestMethod]
    public void Constructor_WithBothParametersNull_DoesNotThrow()
    {
        // Act
        CoursesService service = new CoursesService(null!, null!);

        // Assert
        Assert.IsNotNull(service);
    }

    /// <summary>
    /// Tests that GetCourseAsync returns a valid CourseDto when the API request is successful.
    /// Input: Valid course ID with successful HTTP response (200 OK).
    /// Expected: Returns deserialized CourseDto object.
    /// </summary>
    [TestMethod]
    public async Task GetCourseAsync_SuccessfulResponse_ReturnsDeserializedCourseDto()
    {
        // Arrange
        string courseId = "course123";
        CourseDto expectedCourse = new CourseDto
        {
            Id = courseId,
            Name = "Test Course",
            Lecturer = "Test Lecturer",
            LecturerId = "lecturer123",
            Progress = 50,
            EnrolledStudents = 25,
            TotalSessions = 10,
            CompletedSessions = 5,
            ClassRepId = "rep123",
            ClassRepName = "Test Rep"
        };

        HttpResponseMessage successResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(expectedCourse)
        };

        Mock<IApiService> mockApiService = new Mock<IApiService>();
        mockApiService.Setup(x => x.GetAsync($"courses/{courseId}", It.IsAny<CancellationToken>()))
            .ReturnsAsync(successResponse);

        Mock<ILogger<CoursesService>> mockLogger = new Mock<ILogger<CoursesService>>();
        CoursesService service = new CoursesService(mockApiService.Object, mockLogger.Object);

        // Act
        CourseDto? result = await service.GetCourseAsync(courseId);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(expectedCourse.Id, result.Id);
        Assert.AreEqual(expectedCourse.Name, result.Name);
        Assert.AreEqual(expectedCourse.Lecturer, result.Lecturer);
        mockApiService.Verify(x => x.GetAsync($"courses/{courseId}", It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Tests that GetCourseAsync returns null and logs warning when API returns not found status.
    /// Input: Valid course ID with 404 Not Found HTTP response.
    /// Expected: Returns null and logs warning with course ID and status code.
    /// </summary>
    [TestMethod]
    [DataRow(HttpStatusCode.NotFound)]
    [DataRow(HttpStatusCode.BadRequest)]
    [DataRow(HttpStatusCode.InternalServerError)]
    [DataRow(HttpStatusCode.Unauthorized)]
    [DataRow(HttpStatusCode.Forbidden)]
    [DataRow(HttpStatusCode.ServiceUnavailable)]
    public async Task GetCourseAsync_UnsuccessfulResponse_ReturnsNullAndLogsWarning(HttpStatusCode statusCode)
    {
        // Arrange
        string courseId = "course123";
        HttpResponseMessage errorResponse = new HttpResponseMessage(statusCode);

        Mock<IApiService> mockApiService = new Mock<IApiService>();
        mockApiService.Setup(x => x.GetAsync($"courses/{courseId}", It.IsAny<CancellationToken>()))
            .ReturnsAsync(errorResponse);

        Mock<ILogger<CoursesService>> mockLogger = new Mock<ILogger<CoursesService>>();
        CoursesService service = new CoursesService(mockApiService.Object, mockLogger.Object);

        // Act
        CourseDto? result = await service.GetCourseAsync(courseId);

        // Assert
        Assert.IsNull(result);
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("GetCourse") && v.ToString()!.Contains(courseId)),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
        mockApiService.Verify(x => x.GetAsync($"courses/{courseId}", It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Tests that GetCourseAsync handles empty string course ID correctly.
    /// Input: Empty string as course ID.
    /// Expected: Makes API call with empty string in path.
    /// </summary>
    [TestMethod]
    public async Task GetCourseAsync_EmptyStringId_MakesApiCallWithEmptyString()
    {
        // Arrange
        string courseId = "";
        HttpResponseMessage successResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(new CourseDto { Id = "", Name = "Test" })
        };

        Mock<IApiService> mockApiService = new Mock<IApiService>();
        mockApiService.Setup(x => x.GetAsync($"courses/{courseId}", It.IsAny<CancellationToken>()))
            .ReturnsAsync(successResponse);

        Mock<ILogger<CoursesService>> mockLogger = new Mock<ILogger<CoursesService>>();
        CoursesService service = new CoursesService(mockApiService.Object, mockLogger.Object);

        // Act
        CourseDto? result = await service.GetCourseAsync(courseId);

        // Assert
        Assert.IsNotNull(result);
        mockApiService.Verify(x => x.GetAsync("courses/", It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Tests that GetCourseAsync handles whitespace-only course ID correctly.
    /// Input: Whitespace-only string as course ID.
    /// Expected: Makes API call with whitespace in path.
    /// </summary>
    [TestMethod]
    public async Task GetCourseAsync_WhitespaceId_MakesApiCallWithWhitespace()
    {
        // Arrange
        string courseId = "   ";
        HttpResponseMessage successResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(new CourseDto { Id = courseId, Name = "Test" })
        };

        Mock<IApiService> mockApiService = new Mock<IApiService>();
        mockApiService.Setup(x => x.GetAsync($"courses/{courseId}", It.IsAny<CancellationToken>()))
            .ReturnsAsync(successResponse);

        Mock<ILogger<CoursesService>> mockLogger = new Mock<ILogger<CoursesService>>();
        CoursesService service = new CoursesService(mockApiService.Object, mockLogger.Object);

        // Act
        CourseDto? result = await service.GetCourseAsync(courseId);

        // Assert
        Assert.IsNotNull(result);
        mockApiService.Verify(x => x.GetAsync($"courses/{courseId}", It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Tests that GetCourseAsync handles course IDs with special characters correctly.
    /// Input: Course ID containing special characters.
    /// Expected: Makes API call with special characters in path.
    /// </summary>
    [TestMethod]
    [DataRow("course-123")]
    [DataRow("course_123")]
    [DataRow("course@123")]
    [DataRow("course#123")]
    [DataRow("course!123")]
    [DataRow("course$123")]
    public async Task GetCourseAsync_SpecialCharactersInId_MakesApiCallWithSpecialCharacters(string courseId)
    {
        // Arrange
        HttpResponseMessage successResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(new CourseDto { Id = courseId, Name = "Test" })
        };

        Mock<IApiService> mockApiService = new Mock<IApiService>();
        mockApiService.Setup(x => x.GetAsync($"courses/{courseId}", It.IsAny<CancellationToken>()))
            .ReturnsAsync(successResponse);

        Mock<ILogger<CoursesService>> mockLogger = new Mock<ILogger<CoursesService>>();
        CoursesService service = new CoursesService(mockApiService.Object, mockLogger.Object);

        // Act
        CourseDto? result = await service.GetCourseAsync(courseId);

        // Assert
        Assert.IsNotNull(result);
        mockApiService.Verify(x => x.GetAsync($"courses/{courseId}", It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Tests that GetCourseAsync handles very long course ID correctly.
    /// Input: Very long string as course ID (1000 characters).
    /// Expected: Makes API call with full long string in path.
    /// </summary>
    [TestMethod]
    public async Task GetCourseAsync_VeryLongId_MakesApiCallWithLongString()
    {
        // Arrange
        string courseId = new string('a', 1000);
        HttpResponseMessage successResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(new CourseDto { Id = courseId, Name = "Test" })
        };

        Mock<IApiService> mockApiService = new Mock<IApiService>();
        mockApiService.Setup(x => x.GetAsync($"courses/{courseId}", It.IsAny<CancellationToken>()))
            .ReturnsAsync(successResponse);

        Mock<ILogger<CoursesService>> mockLogger = new Mock<ILogger<CoursesService>>();
        CoursesService service = new CoursesService(mockApiService.Object, mockLogger.Object);

        // Act
        CourseDto? result = await service.GetCourseAsync(courseId);

        // Assert
        Assert.IsNotNull(result);
        mockApiService.Verify(x => x.GetAsync($"courses/{courseId}", It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Tests that GetCourseAsync handles null response content correctly when successful status.
    /// Input: Successful response with null content that deserializes to null.
    /// Expected: Returns null without throwing exception.
    /// </summary>
    [TestMethod]
    public async Task GetCourseAsync_SuccessfulResponseWithNullContent_ReturnsNull()
    {
        // Arrange
        string courseId = "course123";
        HttpResponseMessage successResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create<CourseDto?>(null)
        };

        Mock<IApiService> mockApiService = new Mock<IApiService>();
        mockApiService.Setup(x => x.GetAsync($"courses/{courseId}", It.IsAny<CancellationToken>()))
            .ReturnsAsync(successResponse);

        Mock<ILogger<CoursesService>> mockLogger = new Mock<ILogger<CoursesService>>();
        CoursesService service = new CoursesService(mockApiService.Object, mockLogger.Object);

        // Act
        CourseDto? result = await service.GetCourseAsync(courseId);

        // Assert
        Assert.IsNull(result);
    }

    /// <summary>
    /// Tests that GetCourseAsync handles HTTP status 204 No Content correctly.
    /// Input: HTTP 204 No Content response.
    /// Expected: Returns null and logs warning since 204 is not considered success for this use case.
    /// </summary>
    [TestMethod]
    public async Task GetCourseAsync_NoContentResponse_ReturnsNullAndLogsWarning()
    {
        // Arrange
        string courseId = "course123";
        HttpResponseMessage noContentResponse = new HttpResponseMessage(HttpStatusCode.NoContent);

        Mock<IApiService> mockApiService = new Mock<IApiService>();
        mockApiService.Setup(x => x.GetAsync($"courses/{courseId}", It.IsAny<CancellationToken>()))
            .ReturnsAsync(noContentResponse);

        Mock<ILogger<CoursesService>> mockLogger = new Mock<ILogger<CoursesService>>();
        CoursesService service = new CoursesService(mockApiService.Object, mockLogger.Object);

        // Act
        CourseDto? result = await service.GetCourseAsync(courseId);

        // Assert
        Assert.IsNull(result);
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("GetCourse")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    /// <summary>
    /// Tests that GetCourseAsync correctly formats the API path with the course ID.
    /// Input: Specific course ID.
    /// Expected: API is called with "courses/{id}" path format.
    /// </summary>
    [TestMethod]
    public async Task GetCourseAsync_ValidId_CallsApiWithCorrectPath()
    {
        // Arrange
        string courseId = "test-course-456";
        string expectedPath = $"courses/{courseId}";
        HttpResponseMessage successResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(new CourseDto { Id = courseId })
        };

        Mock<IApiService> mockApiService = new Mock<IApiService>();
        mockApiService.Setup(x => x.GetAsync(expectedPath, It.IsAny<CancellationToken>()))
            .ReturnsAsync(successResponse);

        Mock<ILogger<CoursesService>> mockLogger = new Mock<ILogger<CoursesService>>();
        CoursesService service = new CoursesService(mockApiService.Object, mockLogger.Object);

        // Act
        await service.GetCourseAsync(courseId);

        // Assert
        mockApiService.Verify(x => x.GetAsync(expectedPath, It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Tests that GetCourseAsync logs correct information in warning message.
    /// Input: Course ID that results in error response.
    /// Expected: Warning log contains the course ID and status code.
    /// </summary>
    [TestMethod]
    public async Task GetCourseAsync_ErrorResponse_LogsCorrectWarningMessage()
    {
        // Arrange
        string courseId = "missing-course";
        HttpStatusCode expectedStatusCode = HttpStatusCode.NotFound;
        HttpResponseMessage errorResponse = new HttpResponseMessage(expectedStatusCode);

        Mock<IApiService> mockApiService = new Mock<IApiService>();
        mockApiService.Setup(x => x.GetAsync($"courses/{courseId}", It.IsAny<CancellationToken>()))
            .ReturnsAsync(errorResponse);

        Mock<ILogger<CoursesService>> mockLogger = new Mock<ILogger<CoursesService>>();
        CoursesService service = new CoursesService(mockApiService.Object, mockLogger.Object);

        // Act
        await service.GetCourseAsync(courseId);

        // Assert
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) =>
                    v.ToString()!.Contains(courseId) &&
                    v.ToString()!.Contains(expectedStatusCode.ToString())),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    /// <summary>
    /// Tests that GetCoursesAsync returns a collection of courses when the API responds with a successful status code and valid JSON content.
    /// </summary>
    [TestMethod]
    public async Task GetCoursesAsync_SuccessfulResponseWithCourses_ReturnsCourses()
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<CoursesService>>();
        var expectedCourses = new List<CourseDto>
        {
            new CourseDto { Id = "1", Name = "Course 1", Lecturer = "Lecturer 1", LecturerId = "L1", Progress = 50, EnrolledStudents = 10, TotalSessions = 20, CompletedSessions = 10 },
            new CourseDto { Id = "2", Name = "Course 2", Lecturer = "Lecturer 2", LecturerId = "L2", Progress = 75, EnrolledStudents = 15, TotalSessions = 30, CompletedSessions = 22 }
        };
        var json = JsonSerializer.Serialize(expectedCourses);
        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        };
        mockApi.Setup(x => x.GetAsync("courses", It.IsAny<CancellationToken>())).ReturnsAsync(httpResponse);
        var service = new CoursesService(mockApi.Object, mockLogger.Object);

        // Act
        var result = await service.GetCoursesAsync();

        // Assert
        Assert.IsNotNull(result);
        var resultList = result.ToList();
        Assert.AreEqual(2, resultList.Count);
        Assert.AreEqual("1", resultList[0].Id);
        Assert.AreEqual("Course 1", resultList[0].Name);
        Assert.AreEqual("2", resultList[1].Id);
        Assert.AreEqual("Course 2", resultList[1].Name);
        mockApi.Verify(x => x.GetAsync("courses", It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Tests that GetCoursesAsync returns an empty collection when the API responds with a successful status code but the JSON content deserializes to null.
    /// </summary>
    [TestMethod]
    public async Task GetCoursesAsync_SuccessfulResponseWithNullContent_ReturnsEmptyCollection()
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<CoursesService>>();
        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("null", Encoding.UTF8, "application/json")
        };
        mockApi.Setup(x => x.GetAsync("courses", It.IsAny<CancellationToken>())).ReturnsAsync(httpResponse);
        var service = new CoursesService(mockApi.Object, mockLogger.Object);

        // Act
        var result = await service.GetCoursesAsync();

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(0, result.Count());
        mockApi.Verify(x => x.GetAsync("courses", It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Tests that GetCoursesAsync returns an empty collection when the API responds with a successful status code and an empty array.
    /// </summary>
    [TestMethod]
    public async Task GetCoursesAsync_SuccessfulResponseWithEmptyArray_ReturnsEmptyCollection()
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<CoursesService>>();
        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("[]", Encoding.UTF8, "application/json")
        };
        mockApi.Setup(x => x.GetAsync("courses", It.IsAny<CancellationToken>())).ReturnsAsync(httpResponse);
        var service = new CoursesService(mockApi.Object, mockLogger.Object);

        // Act
        var result = await service.GetCoursesAsync();

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(0, result.Count());
        mockApi.Verify(x => x.GetAsync("courses", It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Tests that GetCoursesAsync returns a single course when the API responds with a successful status code and a single course in the JSON content.
    /// </summary>
    [TestMethod]
    public async Task GetCoursesAsync_SuccessfulResponseWithSingleCourse_ReturnsSingleCourse()
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<CoursesService>>();
        var expectedCourse = new List<CourseDto>
        {
            new CourseDto { Id = "100", Name = "Single Course", Lecturer = "Dr. Smith", LecturerId = "LS100", Progress = 100, EnrolledStudents = 5, TotalSessions = 10, CompletedSessions = 10, ClassRepId = "CR1", ClassRepName = "Rep Name" }
        };
        var json = JsonSerializer.Serialize(expectedCourse);
        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        };
        mockApi.Setup(x => x.GetAsync("courses", It.IsAny<CancellationToken>())).ReturnsAsync(httpResponse);
        var service = new CoursesService(mockApi.Object, mockLogger.Object);

        // Act
        var result = await service.GetCoursesAsync();

        // Assert
        Assert.IsNotNull(result);
        var resultList = result.ToList();
        Assert.AreEqual(1, resultList.Count);
        Assert.AreEqual("100", resultList[0].Id);
        Assert.AreEqual("Single Course", resultList[0].Name);
        mockApi.Verify(x => x.GetAsync("courses", It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Tests that GetCoursesAsync returns an empty collection and logs a warning when the API responds with various non-success status codes.
    /// </summary>
    /// <param name="statusCode">The HTTP status code to test.</param>
    [TestMethod]
    [DataRow(HttpStatusCode.BadRequest)]
    [DataRow(HttpStatusCode.Unauthorized)]
    [DataRow(HttpStatusCode.Forbidden)]
    [DataRow(HttpStatusCode.NotFound)]
    [DataRow(HttpStatusCode.InternalServerError)]
    [DataRow(HttpStatusCode.BadGateway)]
    [DataRow(HttpStatusCode.ServiceUnavailable)]
    [DataRow(HttpStatusCode.GatewayTimeout)]
    public async Task GetCoursesAsync_NonSuccessStatusCode_ReturnsEmptyCollectionAndLogsWarning(HttpStatusCode statusCode)
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<CoursesService>>();
        var httpResponse = new HttpResponseMessage(statusCode);
        mockApi.Setup(x => x.GetAsync("courses", It.IsAny<CancellationToken>())).ReturnsAsync(httpResponse);
        var service = new CoursesService(mockApi.Object, mockLogger.Object);

        // Act
        var result = await service.GetCoursesAsync();

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(0, result.Count());
        mockApi.Verify(x => x.GetAsync("courses", It.IsAny<CancellationToken>()), Times.Once);
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("GetCourses returned")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    /// <summary>
    /// Tests that GetCoursesAsync returns an empty collection when the API responds with a 204 No Content status code.
    /// </summary>
    [TestMethod]
    public async Task GetCoursesAsync_NoContentStatusCode_ReturnsEmptyCollectionAndLogsWarning()
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<CoursesService>>();
        var httpResponse = new HttpResponseMessage(HttpStatusCode.NoContent);
        mockApi.Setup(x => x.GetAsync("courses", It.IsAny<CancellationToken>())).ReturnsAsync(httpResponse);
        var service = new CoursesService(mockApi.Object, mockLogger.Object);

        // Act
        var result = await service.GetCoursesAsync();

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(0, result.Count());
        mockApi.Verify(x => x.GetAsync("courses", It.IsAny<CancellationToken>()), Times.Once);
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("GetCourses returned")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    /// <summary>
    /// Tests that GetCoursesAsync returns an empty collection when the API responds with a redirect status code (3xx).
    /// </summary>
    [TestMethod]
    [DataRow(HttpStatusCode.MovedPermanently)]
    [DataRow(HttpStatusCode.Found)]
    [DataRow(HttpStatusCode.TemporaryRedirect)]
    public async Task GetCoursesAsync_RedirectStatusCode_ReturnsEmptyCollectionAndLogsWarning(HttpStatusCode statusCode)
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<CoursesService>>();
        var httpResponse = new HttpResponseMessage(statusCode);
        mockApi.Setup(x => x.GetAsync("courses", It.IsAny<CancellationToken>())).ReturnsAsync(httpResponse);
        var service = new CoursesService(mockApi.Object, mockLogger.Object);

        // Act
        var result = await service.GetCoursesAsync();

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(0, result.Count());
        mockApi.Verify(x => x.GetAsync("courses", It.IsAny<CancellationToken>()), Times.Once);
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("GetCourses returned")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    /// <summary>
    /// Tests that GetCoursesAsync returns courses with various data values including edge cases like zero enrolled students and maximum progress.
    /// </summary>
    [TestMethod]
    public async Task GetCoursesAsync_CoursesWithEdgeCaseValues_ReturnsCoursesCorrectly()
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<CoursesService>>();
        var expectedCourses = new List<CourseDto>
        {
            new CourseDto { Id = "", Name = "", Lecturer = "", LecturerId = "", Progress = 0, EnrolledStudents = 0, TotalSessions = 0, CompletedSessions = 0 },
            new CourseDto { Id = "max", Name = "Maximum Values Course", Lecturer = "Prof X", LecturerId = "PX", Progress = 100, EnrolledStudents = int.MaxValue, TotalSessions = int.MaxValue, CompletedSessions = int.MaxValue },
            new CourseDto { Id = "special-chars", Name = "Course with Special!@#$%^&*() Characters", Lecturer = "Dr. O'Brien", LecturerId = "L-123", Progress = 50, EnrolledStudents = 20, TotalSessions = 40, CompletedSessions = 20, ClassRepId = null, ClassRepName = null }
        };
        var json = JsonSerializer.Serialize(expectedCourses);
        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        };
        mockApi.Setup(x => x.GetAsync("courses", It.IsAny<CancellationToken>())).ReturnsAsync(httpResponse);
        var service = new CoursesService(mockApi.Object, mockLogger.Object);

        // Act
        var result = await service.GetCoursesAsync();

        // Assert
        Assert.IsNotNull(result);
        var resultList = result.ToList();
        Assert.AreEqual(3, resultList.Count);
        Assert.AreEqual("", resultList[0].Id);
        Assert.AreEqual(0, resultList[0].Progress);
        Assert.AreEqual("max", resultList[1].Id);
        Assert.AreEqual(int.MaxValue, resultList[1].EnrolledStudents);
        Assert.AreEqual("special-chars", resultList[2].Id);
        Assert.IsNull(resultList[2].ClassRepId);
        mockApi.Verify(x => x.GetAsync("courses", It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Tests that GetCoursesAsync correctly handles courses with long string values.
    /// </summary>
    [TestMethod]
    public async Task GetCoursesAsync_CoursesWithVeryLongStrings_ReturnsCoursesCorrectly()
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<CoursesService>>();
        var longString = new string('A', 10000);
        var expectedCourses = new List<CourseDto>
        {
            new CourseDto { Id = longString, Name = longString, Lecturer = longString, LecturerId = longString, Progress = 50, EnrolledStudents = 100, TotalSessions = 200, CompletedSessions = 100 }
        };
        var json = JsonSerializer.Serialize(expectedCourses);
        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        };
        mockApi.Setup(x => x.GetAsync("courses", It.IsAny<CancellationToken>())).ReturnsAsync(httpResponse);
        var service = new CoursesService(mockApi.Object, mockLogger.Object);

        // Act
        var result = await service.GetCoursesAsync();

        // Assert
        Assert.IsNotNull(result);
        var resultList = result.ToList();
        Assert.AreEqual(1, resultList.Count);
        Assert.AreEqual(longString, resultList[0].Id);
        Assert.AreEqual(longString, resultList[0].Name);
        mockApi.Verify(x => x.GetAsync("courses", It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Tests that GetCoursesAsync correctly handles a large collection of courses.
    /// </summary>
    [TestMethod]
    public async Task GetCoursesAsync_LargeCollectionOfCourses_ReturnsAllCourses()
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<CoursesService>>();
        var expectedCourses = new List<CourseDto>();
        for (int i = 0; i < 1000; i++)
        {
            expectedCourses.Add(new CourseDto
            {
                Id = $"course-{i}",
                Name = $"Course {i}",
                Lecturer = $"Lecturer {i}",
                LecturerId = $"L{i}",
                Progress = i % 101,
                EnrolledStudents = i,
                TotalSessions = i * 2,
                CompletedSessions = i
            });
        }
        var json = JsonSerializer.Serialize(expectedCourses);
        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        };
        mockApi.Setup(x => x.GetAsync("courses", It.IsAny<CancellationToken>())).ReturnsAsync(httpResponse);
        var service = new CoursesService(mockApi.Object, mockLogger.Object);

        // Act
        var result = await service.GetCoursesAsync();

        // Assert
        Assert.IsNotNull(result);
        var resultList = result.ToList();
        Assert.AreEqual(1000, resultList.Count);
        Assert.AreEqual("course-0", resultList[0].Id);
        Assert.AreEqual("course-999", resultList[999].Id);
        mockApi.Verify(x => x.GetAsync("courses", It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Tests that GetCoursesAsync calls the API with the correct endpoint path.
    /// Input: Valid mock setup.
    /// Expected: API is called with "courses" as the path parameter.
    /// </summary>
    [TestMethod]
    public async Task GetCoursesAsync_WhenCalled_CallsApiWithCorrectPath()
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<CoursesService>>();
        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("[]", Encoding.UTF8, "application/json")
        };
        mockApi.Setup(x => x.GetAsync("courses", It.IsAny<CancellationToken>())).ReturnsAsync(httpResponse);
        var service = new CoursesService(mockApi.Object, mockLogger.Object);

        // Act
        await service.GetCoursesAsync();

        // Assert
        mockApi.Verify(x => x.GetAsync("courses", It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Tests that GetCourseAsync handles null course ID correctly.
    /// Input: Null course ID.
    /// Expected: Makes API call with null interpolated into path.
    /// </summary>
    [TestMethod]
    public async Task GetCourseAsync_NullId_MakesApiCallWithNull()
    {
        // Arrange
        string? courseId = null;
        HttpResponseMessage successResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(new CourseDto { Id = "test", Name = "Test" })
        };

        Mock<IApiService> mockApiService = new Mock<IApiService>();
        mockApiService.Setup(x => x.GetAsync($"courses/{courseId}", It.IsAny<CancellationToken>()))
            .ReturnsAsync(successResponse);

        Mock<ILogger<CoursesService>> mockLogger = new Mock<ILogger<CoursesService>>();
        CoursesService service = new CoursesService(mockApiService.Object, mockLogger.Object);

        // Act
        CourseDto? result = await service.GetCourseAsync(courseId!);

        // Assert
        Assert.IsNotNull(result);
        mockApiService.Verify(x => x.GetAsync("courses/", It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Tests that GetCourseAsync handles whitespace-only course ID correctly.
    /// Input: Whitespace-only strings as course ID.
    /// Expected: Makes API call with whitespace in path.
    /// </summary>
    [TestMethod]
    [DataRow("   ")]
    [DataRow("\t")]
    [DataRow("\n")]
    [DataRow(" \t\n ")]
    public async Task GetCourseAsync_WhitespaceId_MakesApiCallWithWhitespace(string courseId)
    {
        // Arrange
        HttpResponseMessage successResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(new CourseDto { Id = courseId, Name = "Test" })
        };

        Mock<IApiService> mockApiService = new Mock<IApiService>();
        mockApiService.Setup(x => x.GetAsync($"courses/{courseId}", It.IsAny<CancellationToken>()))
            .ReturnsAsync(successResponse);

        Mock<ILogger<CoursesService>> mockLogger = new Mock<ILogger<CoursesService>>();
        CoursesService service = new CoursesService(mockApiService.Object, mockLogger.Object);

        // Act
        CourseDto? result = await service.GetCourseAsync(courseId);

        // Assert
        Assert.IsNotNull(result);
        mockApiService.Verify(x => x.GetAsync($"courses/{courseId}", It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Tests that GetCourseAsync handles HTTP status 204 No Content correctly.
    /// Input: HTTP 204 No Content response.
    /// Expected: Returns null and logs warning since 204 is not considered success (IsSuccessStatusCode is true but edge case).
    /// </summary>
    [TestMethod]
    public async Task GetCourseAsync_NoContentResponse_HandlesCorrectly()
    {
        // Arrange
        string courseId = "course123";
        HttpResponseMessage noContentResponse = new HttpResponseMessage(HttpStatusCode.NoContent);

        Mock<IApiService> mockApiService = new Mock<IApiService>();
        mockApiService.Setup(x => x.GetAsync($"courses/{courseId}", It.IsAny<CancellationToken>()))
            .ReturnsAsync(noContentResponse);

        Mock<ILogger<CoursesService>> mockLogger = new Mock<ILogger<CoursesService>>();
        CoursesService service = new CoursesService(mockApiService.Object, mockLogger.Object);

        // Act
        CourseDto? result = await service.GetCourseAsync(courseId);

        // Assert
        Assert.IsNull(result);
        mockApiService.Verify(x => x.GetAsync($"courses/{courseId}", It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Tests that GetCourseAsync handles redirect status codes correctly.
    /// Input: HTTP redirect status codes (3xx).
    /// Expected: Returns null and logs warning since redirect is not considered success.
    /// </summary>
    [TestMethod]
    [DataRow(HttpStatusCode.MovedPermanently)]
    [DataRow(HttpStatusCode.Found)]
    [DataRow(HttpStatusCode.TemporaryRedirect)]
    public async Task GetCourseAsync_RedirectResponse_ReturnsNullAndLogsWarning(HttpStatusCode statusCode)
    {
        // Arrange
        string courseId = "redirect-course";
        HttpResponseMessage redirectResponse = new HttpResponseMessage(statusCode);

        Mock<IApiService> mockApiService = new Mock<IApiService>();
        mockApiService.Setup(x => x.GetAsync($"courses/{courseId}", It.IsAny<CancellationToken>()))
            .ReturnsAsync(redirectResponse);

        Mock<ILogger<CoursesService>> mockLogger = new Mock<ILogger<CoursesService>>();
        CoursesService service = new CoursesService(mockApiService.Object, mockLogger.Object);

        // Act
        CourseDto? result = await service.GetCourseAsync(courseId);

        // Assert
        Assert.IsNull(result);
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("GetCourse") && v.ToString()!.Contains(courseId)),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    /// <summary>
    /// Tests that GetCoursesAsync returns a collection of courses when the API responds with a successful status code and valid JSON content.
    /// Input: Successful HTTP response with multiple courses.
    /// Expected: Returns deserialized collection of CourseDto objects.
    /// </summary>
    [TestMethod]
    public async Task GetCoursesAsync_SuccessfulResponseWithMultipleCourses_ReturnsCourses()
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<CoursesService>>();
        var expectedCourses = new List<CourseDto>
        {
            new CourseDto { Id = "1", Name = "Course 1", Lecturer = "Lecturer 1", LecturerId = "L1", Progress = 50, EnrolledStudents = 10, TotalSessions = 20, CompletedSessions = 10 },
            new CourseDto { Id = "2", Name = "Course 2", Lecturer = "Lecturer 2", LecturerId = "L2", Progress = 75, EnrolledStudents = 15, TotalSessions = 30, CompletedSessions = 22 }
        };
        var json = JsonSerializer.Serialize(expectedCourses);
        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        };
        mockApi.Setup(x => x.GetAsync("courses", It.IsAny<CancellationToken>())).ReturnsAsync(httpResponse);
        var service = new CoursesService(mockApi.Object, mockLogger.Object);

        // Act
        var result = await service.GetCoursesAsync();

        // Assert
        Assert.IsNotNull(result);
        var resultList = result.ToList();
        Assert.AreEqual(2, resultList.Count);
        Assert.AreEqual("1", resultList[0].Id);
        Assert.AreEqual("Course 1", resultList[0].Name);
        Assert.AreEqual("2", resultList[1].Id);
        Assert.AreEqual("Course 2", resultList[1].Name);
        mockApi.Verify(x => x.GetAsync("courses", It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Tests that GetCoursesAsync handles courses with special characters in string fields.
    /// Input: Courses with special characters, Unicode, and control characters.
    /// Expected: Returns courses with special characters correctly deserialized.
    /// </summary>
    [TestMethod]
    public async Task GetCoursesAsync_CoursesWithSpecialCharacters_ReturnsCoursesCorrectly()
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<CoursesService>>();
        var expectedCourses = new List<CourseDto>
        {
            new CourseDto { Id = "course-@#$%", Name = "Course with símböls & <html>", Lecturer = "Dr. O'Brien", LecturerId = "L-123_ABC", Progress = 50, EnrolledStudents = 10, TotalSessions = 20, CompletedSessions = 10 },
            new CourseDto { Id = "课程ID", Name = "コース名", Lecturer = "Преподаватель", LecturerId = "מזהה", Progress = 75, EnrolledStudents = 15, TotalSessions = 30, CompletedSessions = 22 }
        };
        var json = JsonSerializer.Serialize(expectedCourses);
        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        };
        mockApi.Setup(x => x.GetAsync("courses", It.IsAny<CancellationToken>())).ReturnsAsync(httpResponse);
        var service = new CoursesService(mockApi.Object, mockLogger.Object);

        // Act
        var result = await service.GetCoursesAsync();

        // Assert
        Assert.IsNotNull(result);
        var resultList = result.ToList();
        Assert.AreEqual(2, resultList.Count);
        Assert.AreEqual("course-@#$%", resultList[0].Id);
        Assert.AreEqual("课程ID", resultList[1].Id);
        mockApi.Verify(x => x.GetAsync("courses", It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Tests that GetCoursesAsync handles courses with negative values in numeric fields.
    /// Input: Courses with negative progress, enrolled students, sessions.
    /// Expected: Returns courses with negative values correctly deserialized.
    /// </summary>
    [TestMethod]
    public async Task GetCoursesAsync_CoursesWithNegativeValues_ReturnsCoursesCorrectly()
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<CoursesService>>();
        var expectedCourses = new List<CourseDto>
        {
            new CourseDto { Id = "1", Name = "Negative Course", Lecturer = "Lecturer", LecturerId = "L1", Progress = -1, EnrolledStudents = -100, TotalSessions = -50, CompletedSessions = -25 }
        };
        var json = JsonSerializer.Serialize(expectedCourses);
        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        };
        mockApi.Setup(x => x.GetAsync("courses", It.IsAny<CancellationToken>())).ReturnsAsync(httpResponse);
        var service = new CoursesService(mockApi.Object, mockLogger.Object);

        // Act
        var result = await service.GetCoursesAsync();

        // Assert
        Assert.IsNotNull(result);
        var resultList = result.ToList();
        Assert.AreEqual(1, resultList.Count);
        Assert.AreEqual(-1, resultList[0].Progress);
        Assert.AreEqual(-100, resultList[0].EnrolledStudents);
        mockApi.Verify(x => x.GetAsync("courses", It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Tests that GetCoursesAsync handles courses with minimum integer values.
    /// Input: Courses with int.MinValue in numeric fields.
    /// Expected: Returns courses with minimum values correctly deserialized.
    /// </summary>
    [TestMethod]
    public async Task GetCoursesAsync_CoursesWithMinIntegerValues_ReturnsCoursesCorrectly()
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<CoursesService>>();
        var expectedCourses = new List<CourseDto>
        {
            new CourseDto { Id = "1", Name = "Min Values", Lecturer = "Lecturer", LecturerId = "L1", Progress = int.MinValue, EnrolledStudents = int.MinValue, TotalSessions = int.MinValue, CompletedSessions = int.MinValue }
        };
        var json = JsonSerializer.Serialize(expectedCourses);
        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        };
        mockApi.Setup(x => x.GetAsync("courses", It.IsAny<CancellationToken>())).ReturnsAsync(httpResponse);
        var service = new CoursesService(mockApi.Object, mockLogger.Object);

        // Act
        var result = await service.GetCoursesAsync();

        // Assert
        Assert.IsNotNull(result);
        var resultList = result.ToList();
        Assert.AreEqual(1, resultList.Count);
        Assert.AreEqual(int.MinValue, resultList[0].Progress);
        Assert.AreEqual(int.MinValue, resultList[0].EnrolledStudents);
        mockApi.Verify(x => x.GetAsync("courses", It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Tests that GetCoursesAsync handles other 2xx success status codes correctly.
    /// Input: HTTP 201 Created and 202 Accepted responses with valid content.
    /// Expected: Returns deserialized courses for all 2xx success codes.
    /// </summary>
    /// <param name="statusCode">The 2xx HTTP status code to test.</param>
    [TestMethod]
    [DataRow(HttpStatusCode.Created)]
    [DataRow(HttpStatusCode.Accepted)]
    public async Task GetCoursesAsync_Other2xxSuccessCodes_ReturnsCourses(HttpStatusCode statusCode)
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<CoursesService>>();
        var expectedCourses = new List<CourseDto>
        {
            new CourseDto { Id = "1", Name = "Course 1", Lecturer = "Lecturer 1", LecturerId = "L1", Progress = 50, EnrolledStudents = 10, TotalSessions = 20, CompletedSessions = 10 }
        };
        var json = JsonSerializer.Serialize(expectedCourses);
        var httpResponse = new HttpResponseMessage(statusCode)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        };
        mockApi.Setup(x => x.GetAsync("courses", It.IsAny<CancellationToken>())).ReturnsAsync(httpResponse);
        var service = new CoursesService(mockApi.Object, mockLogger.Object);

        // Act
        var result = await service.GetCoursesAsync();

        // Assert
        Assert.IsNotNull(result);
        var resultList = result.ToList();
        Assert.AreEqual(1, resultList.Count);
        Assert.AreEqual("1", resultList[0].Id);
        mockApi.Verify(x => x.GetAsync("courses", It.IsAny<CancellationToken>()), Times.Once);
    }
}