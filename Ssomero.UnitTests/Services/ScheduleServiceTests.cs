using System;
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
/// Unit tests for the <see cref="ScheduleService"/> class.
/// </summary>
[TestClass]
public class ScheduleServiceTests
{
    /// <summary>
    /// Tests that the constructor successfully creates an instance when provided with valid dependencies.
    /// Verifies that no exception is thrown during instantiation with valid IApiService and ILogger instances.
    /// </summary>
    [TestMethod]
    public void Constructor_WithValidDependencies_CreatesInstanceSuccessfully()
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<ScheduleService>>();

        // Act
        var service = new ScheduleService(mockApi.Object, mockLogger.Object);

        // Assert
        Assert.IsNotNull(service);
    }

    /// <summary>
    /// Tests that the constructor accepts a null IApiService parameter without throwing an exception.
    /// This test reveals that the constructor lacks null validation for the api parameter,
    /// which may lead to NullReferenceException when the service attempts to use _api field.
    /// </summary>
    [TestMethod]
    public void Constructor_WithNullApiService_DoesNotThrowException()
    {
        // Arrange
        IApiService? nullApi = null;
        var mockLogger = new Mock<ILogger<ScheduleService>>();

        // Act
        var service = new ScheduleService(nullApi!, mockLogger.Object);

        // Assert
        Assert.IsNotNull(service);
    }

    /// <summary>
    /// Tests that the constructor accepts a null ILogger parameter without throwing an exception.
    /// This test reveals that the constructor lacks null validation for the logger parameter,
    /// which may lead to NullReferenceException when the service attempts to use _logger field.
    /// </summary>
    [TestMethod]
    public void Constructor_WithNullLogger_DoesNotThrowException()
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        ILogger<ScheduleService>? nullLogger = null;

        // Act
        var service = new ScheduleService(mockApi.Object, nullLogger!);

        // Assert
        Assert.IsNotNull(service);
    }

    /// <summary>
    /// Tests that the constructor accepts both null parameters without throwing an exception.
    /// This test reveals that the constructor lacks any null validation,
    /// which may lead to NullReferenceException when the service attempts to use either field.
    /// </summary>
    [TestMethod]
    public void Constructor_WithBothParametersNull_DoesNotThrowException()
    {
        // Arrange
        IApiService? nullApi = null;
        ILogger<ScheduleService>? nullLogger = null;

        // Act
        var service = new ScheduleService(nullApi!, nullLogger!);

        // Assert
        Assert.IsNotNull(service);
    }

    /// <summary>
    /// Tests that GetSchedulesAsync returns deserialized schedules when the API response is successful and contains valid data.
    /// </summary>
    [TestMethod]
    public async Task GetSchedulesAsync_SuccessfulResponseWithData_ReturnsDeserializedSchedules()
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<ScheduleService>>();
        var service = new ScheduleService(mockApi.Object, mockLogger.Object);

        var expectedSchedules = new List<ScheduleDto>
        {
            new ScheduleDto
            {
                Id = "1",
                CourseId = "CS101",
                CourseName = "Introduction to Computer Science",
                LecturerName = "Dr. Smith",
                StartTime = new DateTime(2024, 1, 15, 9, 0, 0),
                EndTime = new DateTime(2024, 1, 15, 11, 0, 0),
                Venue = "Room 101",
                IsCancelled = false
            },
            new ScheduleDto
            {
                Id = "2",
                CourseId = "CS102",
                CourseName = "Data Structures",
                LecturerName = "Prof. Johnson",
                StartTime = new DateTime(2024, 1, 15, 13, 0, 0),
                EndTime = new DateTime(2024, 1, 15, 15, 0, 0),
                Venue = "Room 202",
                IsCancelled = false
            }
        };

        var jsonContent = JsonContent.Create(expectedSchedules);
        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = jsonContent
        };

        mockApi.Setup(x => x.GetAsync("schedules", It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        // Act
        var result = await service.GetSchedulesAsync();

        // Assert
        Assert.IsNotNull(result);
        var schedulesList = result.ToList();
        Assert.AreEqual(2, schedulesList.Count);
        Assert.AreEqual("1", schedulesList[0].Id);
        Assert.AreEqual("CS101", schedulesList[0].CourseId);
        Assert.AreEqual("2", schedulesList[1].Id);
        Assert.AreEqual("CS102", schedulesList[1].CourseId);
        mockApi.Verify(x => x.GetAsync("schedules", It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Tests that GetSchedulesAsync returns an empty collection when the API response is successful but contains no schedules.
    /// </summary>
    [TestMethod]
    public async Task GetSchedulesAsync_SuccessfulResponseWithEmptyCollection_ReturnsEmptyCollection()
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<ScheduleService>>();
        var service = new ScheduleService(mockApi.Object, mockLogger.Object);

        var emptySchedules = new List<ScheduleDto>();
        var jsonContent = JsonContent.Create(emptySchedules);
        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = jsonContent
        };

        mockApi.Setup(x => x.GetAsync("schedules", It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        // Act
        var result = await service.GetSchedulesAsync();

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(0, result.Count());
        mockApi.Verify(x => x.GetAsync("schedules", It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Tests that GetSchedulesAsync returns an empty collection when the API response is successful but the content deserializes to null.
    /// </summary>
    [TestMethod]
    public async Task GetSchedulesAsync_SuccessfulResponseWithNullContent_ReturnsEmptyCollection()
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<ScheduleService>>();
        var service = new ScheduleService(mockApi.Object, mockLogger.Object);

        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("null", System.Text.Encoding.UTF8, "application/json")
        };

        mockApi.Setup(x => x.GetAsync("schedules", It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        // Act
        var result = await service.GetSchedulesAsync();

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(0, result.Count());
        mockApi.Verify(x => x.GetAsync("schedules", It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Tests that GetSchedulesAsync returns an empty collection and logs a warning when the API response has a non-success status code.
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
    public async Task GetSchedulesAsync_NonSuccessStatusCode_ReturnsEmptyCollectionAndLogsWarning(HttpStatusCode statusCode)
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<ScheduleService>>();
        var service = new ScheduleService(mockApi.Object, mockLogger.Object);

        var response = new HttpResponseMessage(statusCode);

        mockApi.Setup(x => x.GetAsync("schedules", It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        // Act
        var result = await service.GetSchedulesAsync();

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(0, result.Count());
        mockApi.Verify(x => x.GetAsync("schedules", It.IsAny<CancellationToken>()), Times.Once);
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("GetSchedules returned")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    /// <summary>
    /// Tests that GetSchedulesAsync returns a single schedule when the API response contains exactly one schedule.
    /// </summary>
    [TestMethod]
    public async Task GetSchedulesAsync_SuccessfulResponseWithSingleSchedule_ReturnsSingleSchedule()
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<ScheduleService>>();
        var service = new ScheduleService(mockApi.Object, mockLogger.Object);

        var expectedSchedule = new ScheduleDto
        {
            Id = "single-1",
            CourseId = "CS201",
            CourseName = "Advanced Programming",
            LecturerName = "Dr. Williams",
            StartTime = new DateTime(2024, 1, 20, 10, 0, 0),
            EndTime = new DateTime(2024, 1, 20, 12, 0, 0),
            Venue = "Lab 1",
            IsCancelled = false
        };

        var jsonContent = JsonContent.Create(new List<ScheduleDto> { expectedSchedule });
        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = jsonContent
        };

        mockApi.Setup(x => x.GetAsync("schedules", It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        // Act
        var result = await service.GetSchedulesAsync();

        // Assert
        Assert.IsNotNull(result);
        var schedulesList = result.ToList();
        Assert.AreEqual(1, schedulesList.Count);
        Assert.AreEqual("single-1", schedulesList[0].Id);
        Assert.AreEqual("CS201", schedulesList[0].CourseId);
        mockApi.Verify(x => x.GetAsync("schedules", It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Tests that GetSchedulesAsync returns multiple schedules when the API response contains many schedules.
    /// </summary>
    [TestMethod]
    public async Task GetSchedulesAsync_SuccessfulResponseWithMultipleSchedules_ReturnsAllSchedules()
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<ScheduleService>>();
        var service = new ScheduleService(mockApi.Object, mockLogger.Object);

        var expectedSchedules = new List<ScheduleDto>();
        for (int i = 1; i <= 10; i++)
        {
            expectedSchedules.Add(new ScheduleDto
            {
                Id = $"schedule-{i}",
                CourseId = $"COURSE{i}",
                CourseName = $"Course {i}",
                LecturerName = $"Lecturer {i}",
                StartTime = new DateTime(2024, 1, i, 9, 0, 0),
                EndTime = new DateTime(2024, 1, i, 11, 0, 0),
                Venue = $"Room {i}",
                IsCancelled = false
            });
        }

        var jsonContent = JsonContent.Create(expectedSchedules);
        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = jsonContent
        };

        mockApi.Setup(x => x.GetAsync("schedules", It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        // Act
        var result = await service.GetSchedulesAsync();

        // Assert
        Assert.IsNotNull(result);
        var schedulesList = result.ToList();
        Assert.AreEqual(10, schedulesList.Count);
        Assert.AreEqual("schedule-1", schedulesList[0].Id);
        Assert.AreEqual("schedule-10", schedulesList[9].Id);
        mockApi.Verify(x => x.GetAsync("schedules", It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Tests that GetSchedulesAsync correctly handles schedules with various property values including edge cases.
    /// </summary>
    [TestMethod]
    public async Task GetSchedulesAsync_SchedulesWithEdgeCaseValues_ReturnsSchedulesWithAllProperties()
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<ScheduleService>>();
        var service = new ScheduleService(mockApi.Object, mockLogger.Object);

        var expectedSchedules = new List<ScheduleDto>
        {
            new ScheduleDto
            {
                Id = string.Empty,
                CourseId = string.Empty,
                CourseName = string.Empty,
                LecturerName = string.Empty,
                StartTime = DateTime.MinValue,
                EndTime = DateTime.MaxValue,
                Venue = string.Empty,
                IsCancelled = true
            },
            new ScheduleDto
            {
                Id = "very-long-id-" + new string('x', 1000),
                CourseId = "special-chars-!@#$%^&*()",
                CourseName = "Course with\nnewlines\tand\ttabs",
                LecturerName = "Lecturer with unicode: 中文",
                StartTime = new DateTime(2024, 12, 31, 23, 59, 59),
                EndTime = new DateTime(2025, 1, 1, 0, 0, 0),
                Venue = "   Venue with spaces   ",
                IsCancelled = false
            }
        };

        var jsonContent = JsonContent.Create(expectedSchedules);
        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = jsonContent
        };

        mockApi.Setup(x => x.GetAsync("schedules", It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        // Act
        var result = await service.GetSchedulesAsync();

        // Assert
        Assert.IsNotNull(result);
        var schedulesList = result.ToList();
        Assert.AreEqual(2, schedulesList.Count);
        Assert.AreEqual(string.Empty, schedulesList[0].Id);
        Assert.IsTrue(schedulesList[0].IsCancelled);
        Assert.IsTrue(schedulesList[1].Id.StartsWith("very-long-id-"));
        mockApi.Verify(x => x.GetAsync("schedules", It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Tests that CancelScheduleAsync returns true when the API returns a successful status code.
    /// </summary>
    /// <param name="statusCode">The HTTP status code to test.</param>
    [TestMethod]
    [DataRow(HttpStatusCode.OK)]
    [DataRow(HttpStatusCode.NoContent)]
    [DataRow(HttpStatusCode.Accepted)]
    public async Task CancelScheduleAsync_SuccessfulStatusCode_ReturnsTrue(HttpStatusCode statusCode)
    {
        // Arrange
        var scheduleId = "test-schedule-123";
        var mockApi = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<ScheduleService>>();
        var response = new HttpResponseMessage(statusCode);

        mockApi.Setup(x => x.DeleteAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        var service = new ScheduleService(mockApi.Object, mockLogger.Object);

        // Act
        var result = await service.CancelScheduleAsync(scheduleId);

        // Assert
        Assert.IsTrue(result);
        mockApi.Verify(x => x.DeleteAsync($"schedules/{scheduleId}", It.IsAny<CancellationToken>()), Times.Once);
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
    /// Tests that CancelScheduleAsync returns false and logs a warning when the API returns a non-successful status code.
    /// </summary>
    /// <param name="statusCode">The HTTP status code to test.</param>
    [TestMethod]
    [DataRow(HttpStatusCode.NotFound)]
    [DataRow(HttpStatusCode.InternalServerError)]
    [DataRow(HttpStatusCode.BadRequest)]
    [DataRow(HttpStatusCode.Unauthorized)]
    [DataRow(HttpStatusCode.Forbidden)]
    [DataRow(HttpStatusCode.ServiceUnavailable)]
    public async Task CancelScheduleAsync_NonSuccessfulStatusCode_ReturnsFalseAndLogsWarning(HttpStatusCode statusCode)
    {
        // Arrange
        var scheduleId = "test-schedule-123";
        var mockApi = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<ScheduleService>>();
        var response = new HttpResponseMessage(statusCode);

        mockApi.Setup(x => x.DeleteAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        var service = new ScheduleService(mockApi.Object, mockLogger.Object);

        // Act
        var result = await service.CancelScheduleAsync(scheduleId);

        // Assert
        Assert.IsFalse(result);
        mockApi.Verify(x => x.DeleteAsync($"schedules/{scheduleId}", It.IsAny<CancellationToken>()), Times.Once);
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("CancelSchedule returned") && v.ToString()!.Contains(statusCode.ToString())),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    /// <summary>
    /// Tests that CancelScheduleAsync constructs the correct URL with the provided scheduleId.
    /// </summary>
    [TestMethod]
    public async Task CancelScheduleAsync_ValidScheduleId_ConstructsCorrectUrl()
    {
        // Arrange
        var scheduleId = "my-schedule-456";
        var mockApi = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<ScheduleService>>();
        var response = new HttpResponseMessage(HttpStatusCode.OK);

        mockApi.Setup(x => x.DeleteAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        var service = new ScheduleService(mockApi.Object, mockLogger.Object);

        // Act
        await service.CancelScheduleAsync(scheduleId);

        // Assert
        mockApi.Verify(x => x.DeleteAsync("schedules/my-schedule-456", It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Tests that CancelScheduleAsync handles empty string scheduleId without throwing exception.
    /// </summary>
    [TestMethod]
    public async Task CancelScheduleAsync_EmptyScheduleId_CallsApiWithEmptyId()
    {
        // Arrange
        var scheduleId = string.Empty;
        var mockApi = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<ScheduleService>>();
        var response = new HttpResponseMessage(HttpStatusCode.OK);

        mockApi.Setup(x => x.DeleteAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        var service = new ScheduleService(mockApi.Object, mockLogger.Object);

        // Act
        var result = await service.CancelScheduleAsync(scheduleId);

        // Assert
        Assert.IsTrue(result);
        mockApi.Verify(x => x.DeleteAsync("schedules/", It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Tests that CancelScheduleAsync handles whitespace scheduleId without throwing exception.
    /// </summary>
    [TestMethod]
    public async Task CancelScheduleAsync_WhitespaceScheduleId_CallsApiWithWhitespace()
    {
        // Arrange
        var scheduleId = "   ";
        var mockApi = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<ScheduleService>>();
        var response = new HttpResponseMessage(HttpStatusCode.OK);

        mockApi.Setup(x => x.DeleteAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        var service = new ScheduleService(mockApi.Object, mockLogger.Object);

        // Act
        var result = await service.CancelScheduleAsync(scheduleId);

        // Assert
        Assert.IsTrue(result);
        mockApi.Verify(x => x.DeleteAsync("schedules/   ", It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Tests that CancelScheduleAsync handles scheduleId with special characters.
    /// </summary>
    /// <param name="scheduleId">The scheduleId with special characters to test.</param>
    [TestMethod]
    [DataRow("test/123")]
    [DataRow("test?id=1")]
    [DataRow("test#anchor")]
    [DataRow("test&param=value")]
    [DataRow("test schedule")]
    public async Task CancelScheduleAsync_ScheduleIdWithSpecialCharacters_CallsApiWithSpecialCharacters(string scheduleId)
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<ScheduleService>>();
        var response = new HttpResponseMessage(HttpStatusCode.OK);

        mockApi.Setup(x => x.DeleteAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        var service = new ScheduleService(mockApi.Object, mockLogger.Object);

        // Act
        var result = await service.CancelScheduleAsync(scheduleId);

        // Assert
        Assert.IsTrue(result);
        mockApi.Verify(x => x.DeleteAsync($"schedules/{scheduleId}", It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Tests that CancelScheduleAsync handles very long scheduleId without issues.
    /// </summary>
    [TestMethod]
    public async Task CancelScheduleAsync_VeryLongScheduleId_CallsApiSuccessfully()
    {
        // Arrange
        var scheduleId = new string('a', 10000);
        var mockApi = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<ScheduleService>>();
        var response = new HttpResponseMessage(HttpStatusCode.OK);

        mockApi.Setup(x => x.DeleteAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        var service = new ScheduleService(mockApi.Object, mockLogger.Object);

        // Act
        var result = await service.CancelScheduleAsync(scheduleId);

        // Assert
        Assert.IsTrue(result);
        mockApi.Verify(x => x.DeleteAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Tests that CancelScheduleAsync properly disposes HttpResponseMessage.
    /// Verifies that the response object is not left in an invalid state.
    /// </summary>
    [TestMethod]
    public async Task CancelScheduleAsync_MultipleCallsWithSameService_HandlesResponseCorrectly()
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<ScheduleService>>();
        var response1 = new HttpResponseMessage(HttpStatusCode.OK);
        var response2 = new HttpResponseMessage(HttpStatusCode.NotFound);

        mockApi.SetupSequence(x => x.DeleteAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(response1)
            .ReturnsAsync(response2);

        var service = new ScheduleService(mockApi.Object, mockLogger.Object);

        // Act
        var result1 = await service.CancelScheduleAsync("schedule1");
        var result2 = await service.CancelScheduleAsync("schedule2");

        // Assert
        Assert.IsTrue(result1);
        Assert.IsFalse(result2);
        mockApi.Verify(x => x.DeleteAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
    }

    /// <summary>
    /// Tests that CreateScheduleAsync returns false when the API call fails.
    /// Input: Valid ScheduleDto with HTTP 400 Bad Request response.
    /// Expected: Returns false and logs a warning with the status code.
    /// </summary>
    [TestMethod]
    public async Task CreateScheduleAsync_UnsuccessfulResponse_ReturnsFalseAndLogsWarning()
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<ScheduleService>>();
        var service = new ScheduleService(mockApi.Object, mockLogger.Object);

        var schedule = new ScheduleDto
        {
            Id = "1",
            CourseId = "CS101",
            CourseName = "Introduction to Computer Science",
            LecturerName = "Dr. Smith",
            StartTime = DateTime.Now,
            EndTime = DateTime.Now.AddHours(2),
            Venue = "Room 101"
        };

        var failureResponse = new HttpResponseMessage(HttpStatusCode.BadRequest);
        mockApi.Setup(x => x.PostAsync("schedules", It.IsAny<HttpContent>(), It.IsAny<CancellationToken>()))
               .ReturnsAsync(failureResponse);

        // Act
        var result = await service.CreateScheduleAsync(schedule);

        // Assert
        Assert.IsFalse(result);
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("CreateSchedule returned")),
                It.IsAny<Exception?>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    /// <summary>
    /// Tests that CreateScheduleAsync handles various HTTP status codes correctly.
    /// Input: Valid ScheduleDto with different HTTP status codes.
    /// Expected: Returns true for success codes, false for error codes, and logs warning only for failures.
    /// </summary>
    [TestMethod]
    [DataRow(HttpStatusCode.OK, true, DisplayName = "200 OK")]
    [DataRow(HttpStatusCode.Created, true, DisplayName = "201 Created")]
    [DataRow(HttpStatusCode.Accepted, true, DisplayName = "202 Accepted")]
    [DataRow(HttpStatusCode.NoContent, true, DisplayName = "204 No Content")]
    [DataRow(HttpStatusCode.BadRequest, false, DisplayName = "400 Bad Request")]
    [DataRow(HttpStatusCode.Unauthorized, false, DisplayName = "401 Unauthorized")]
    [DataRow(HttpStatusCode.Forbidden, false, DisplayName = "403 Forbidden")]
    [DataRow(HttpStatusCode.NotFound, false, DisplayName = "404 Not Found")]
    [DataRow(HttpStatusCode.InternalServerError, false, DisplayName = "500 Internal Server Error")]
    [DataRow(HttpStatusCode.ServiceUnavailable, false, DisplayName = "503 Service Unavailable")]
    public async Task CreateScheduleAsync_VariousStatusCodes_ReturnsExpectedResult(HttpStatusCode statusCode, bool expectedResult)
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<ScheduleService>>();
        var service = new ScheduleService(mockApi.Object, mockLogger.Object);

        var schedule = new ScheduleDto
        {
            Id = "test-id",
            CourseId = "CS101",
            CourseName = "Test Course"
        };

        var response = new HttpResponseMessage(statusCode);
        mockApi.Setup(x => x.PostAsync("schedules", It.IsAny<HttpContent>(), It.IsAny<CancellationToken>()))
               .ReturnsAsync(response);

        // Act
        var result = await service.CreateScheduleAsync(schedule);

        // Assert
        Assert.AreEqual(expectedResult, result);

        var expectedLogTimes = expectedResult ? Times.Never() : Times.Once();
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("CreateSchedule returned")),
                It.IsAny<Exception?>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            expectedLogTimes);
    }

    /// <summary>
    /// Tests that CreateScheduleAsync correctly passes the schedule data to the API.
    /// Input: Valid ScheduleDto with specific values.
    /// Expected: PostAsync is called with "schedules" endpoint and JSON content.
    /// </summary>
    [TestMethod]
    public async Task CreateScheduleAsync_ValidSchedule_CallsApiWithCorrectParameters()
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<ScheduleService>>();
        var service = new ScheduleService(mockApi.Object, mockLogger.Object);

        var schedule = new ScheduleDto
        {
            Id = "schedule-123",
            CourseId = "CS101",
            CourseName = "Advanced Programming",
            LecturerName = "Prof. Johnson",
            StartTime = new DateTime(2024, 1, 15, 10, 0, 0),
            EndTime = new DateTime(2024, 1, 15, 12, 0, 0),
            Venue = "Lab A1"
        };

        var successResponse = new HttpResponseMessage(HttpStatusCode.Created);
        mockApi.Setup(x => x.PostAsync("schedules", It.IsAny<HttpContent>(), It.IsAny<CancellationToken>()))
               .ReturnsAsync(successResponse);

        // Act
        var result = await service.CreateScheduleAsync(schedule);

        // Assert
        Assert.IsTrue(result);
        mockApi.Verify(
            x => x.PostAsync("schedules", It.IsAny<HttpContent>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    /// <summary>
    /// Tests that CreateScheduleAsync handles empty string properties in ScheduleDto.
    /// Input: ScheduleDto with empty string properties.
    /// Expected: Method executes without exception and returns based on API response.
    /// </summary>
    [TestMethod]
    public async Task CreateScheduleAsync_ScheduleWithEmptyStrings_ExecutesSuccessfully()
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<ScheduleService>>();
        var service = new ScheduleService(mockApi.Object, mockLogger.Object);

        var schedule = new ScheduleDto
        {
            Id = string.Empty,
            CourseId = string.Empty,
            CourseName = string.Empty,
            LecturerName = string.Empty,
            Venue = string.Empty,
            StartTime = DateTime.MinValue,
            EndTime = DateTime.MaxValue
        };

        var successResponse = new HttpResponseMessage(HttpStatusCode.OK);
        mockApi.Setup(x => x.PostAsync("schedules", It.IsAny<HttpContent>(), It.IsAny<CancellationToken>()))
               .ReturnsAsync(successResponse);

        // Act
        var result = await service.CreateScheduleAsync(schedule);

        // Assert
        Assert.IsTrue(result);
        mockApi.Verify(
            x => x.PostAsync("schedules", It.IsAny<HttpContent>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    /// <summary>
    /// Tests that CreateScheduleAsync handles very long string values in ScheduleDto.
    /// Input: ScheduleDto with very long string properties.
    /// Expected: Method executes without exception and returns based on API response.
    /// </summary>
    [TestMethod]
    public async Task CreateScheduleAsync_ScheduleWithLongStrings_ExecutesSuccessfully()
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<ScheduleService>>();
        var service = new ScheduleService(mockApi.Object, mockLogger.Object);

        var longString = new string('x', 10000);
        var schedule = new ScheduleDto
        {
            Id = longString,
            CourseId = longString,
            CourseName = longString,
            LecturerName = longString,
            Venue = longString,
            StartTime = DateTime.Now,
            EndTime = DateTime.Now.AddHours(1)
        };

        var successResponse = new HttpResponseMessage(HttpStatusCode.OK);
        mockApi.Setup(x => x.PostAsync("schedules", It.IsAny<HttpContent>(), It.IsAny<CancellationToken>()))
               .ReturnsAsync(successResponse);

        // Act
        var result = await service.CreateScheduleAsync(schedule);

        // Assert
        Assert.IsTrue(result);
        mockApi.Verify(
            x => x.PostAsync("schedules", It.IsAny<HttpContent>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    /// <summary>
    /// Tests that CreateScheduleAsync handles special characters in string properties.
    /// Input: ScheduleDto with special characters, unicode, and control characters.
    /// Expected: Method executes without exception and returns based on API response.
    /// </summary>
    [TestMethod]
    public async Task CreateScheduleAsync_ScheduleWithSpecialCharacters_ExecutesSuccessfully()
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<ScheduleService>>();
        var service = new ScheduleService(mockApi.Object, mockLogger.Object);

        var schedule = new ScheduleDto
        {
            Id = "id-with-special-!@#$%^&*()",
            CourseId = "course<>\"'&",
            CourseName = "Course with émojis 🎓📚",
            LecturerName = "Dr. O'Brien & Associates",
            Venue = "Room\t\n\r123",
            StartTime = DateTime.Now,
            EndTime = DateTime.Now.AddHours(1)
        };

        var successResponse = new HttpResponseMessage(HttpStatusCode.OK);
        mockApi.Setup(x => x.PostAsync("schedules", It.IsAny<HttpContent>(), It.IsAny<CancellationToken>()))
               .ReturnsAsync(successResponse);

        // Act
        var result = await service.CreateScheduleAsync(schedule);

        // Assert
        Assert.IsTrue(result);
        mockApi.Verify(
            x => x.PostAsync("schedules", It.IsAny<HttpContent>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    /// <summary>
    /// Tests that CreateScheduleAsync handles extreme DateTime boundary values.
    /// Input: ScheduleDto with DateTime.MinValue and DateTime.MaxValue.
    /// Expected: Method executes without exception and returns based on API response.
    /// </summary>
    [TestMethod]
    public async Task CreateScheduleAsync_ScheduleWithExtremeDateTimes_ExecutesSuccessfully()
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<ScheduleService>>();
        var service = new ScheduleService(mockApi.Object, mockLogger.Object);

        var schedule = new ScheduleDto
        {
            Id = "extreme-dates",
            CourseId = "CS999",
            CourseName = "Test Course",
            StartTime = DateTime.MinValue,
            EndTime = DateTime.MaxValue
        };

        var successResponse = new HttpResponseMessage(HttpStatusCode.OK);
        mockApi.Setup(x => x.PostAsync("schedules", It.IsAny<HttpContent>(), It.IsAny<CancellationToken>()))
               .ReturnsAsync(successResponse);

        // Act
        var result = await service.CreateScheduleAsync(schedule);

        // Assert
        Assert.IsTrue(result);
    }

    /// <summary>
    /// Tests that CreateScheduleAsync logs the correct status code in the warning message.
    /// Input: Valid ScheduleDto with HTTP 404 Not Found response.
    /// Expected: Warning is logged containing the status code 404.
    /// </summary>
    [TestMethod]
    public async Task CreateScheduleAsync_FailureResponse_LogsCorrectStatusCode()
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<ScheduleService>>();
        var service = new ScheduleService(mockApi.Object, mockLogger.Object);

        var schedule = new ScheduleDto
        {
            Id = "1",
            CourseId = "CS101"
        };

        var notFoundResponse = new HttpResponseMessage(HttpStatusCode.NotFound);
        mockApi.Setup(x => x.PostAsync("schedules", It.IsAny<HttpContent>(), It.IsAny<CancellationToken>()))
               .ReturnsAsync(notFoundResponse);

        // Act
        var result = await service.CreateScheduleAsync(schedule);

        // Assert
        Assert.IsFalse(result);
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("NotFound")),
                It.IsAny<Exception?>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    /// <summary>
    /// Tests that CreateScheduleAsync returns true and does not log a warning when the API returns a successful status code.
    /// Input: Valid ScheduleDto with HTTP 200 OK response.
    /// Expected: Returns true and logger is not called.
    /// </summary>
    [TestMethod]
    public async Task CreateScheduleAsync_SuccessfulResponse_ReturnsTrueAndDoesNotLog()
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<ScheduleService>>();
        var service = new ScheduleService(mockApi.Object, mockLogger.Object);

        var schedule = new ScheduleDto
        {
            Id = "schedule-1",
            CourseId = "CS101",
            CourseName = "Computer Science",
            LecturerName = "Dr. Smith",
            StartTime = DateTime.Now,
            EndTime = DateTime.Now.AddHours(1),
            Venue = "Room 101"
        };

        var successResponse = new HttpResponseMessage(HttpStatusCode.OK);
        mockApi.Setup(x => x.PostAsync("schedules", It.IsAny<HttpContent>(), It.IsAny<CancellationToken>()))
               .ReturnsAsync(successResponse);

        // Act
        var result = await service.CreateScheduleAsync(schedule);

        // Assert
        Assert.IsTrue(result);
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
    /// Tests that CreateScheduleAsync handles ScheduleDto with all default values successfully.
    /// Input: ScheduleDto with all properties set to default values.
    /// Expected: Method executes without exception and returns based on API response.
    /// </summary>
    [TestMethod]
    public async Task CreateScheduleAsync_ScheduleWithDefaultValues_ExecutesSuccessfully()
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<ScheduleService>>();
        var service = new ScheduleService(mockApi.Object, mockLogger.Object);

        var schedule = new ScheduleDto();

        var successResponse = new HttpResponseMessage(HttpStatusCode.Created);
        mockApi.Setup(x => x.PostAsync("schedules", It.IsAny<HttpContent>(), It.IsAny<CancellationToken>()))
               .ReturnsAsync(successResponse);

        // Act
        var result = await service.CreateScheduleAsync(schedule);

        // Assert
        Assert.IsTrue(result);
        mockApi.Verify(
            x => x.PostAsync("schedules", It.IsAny<HttpContent>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    /// <summary>
    /// Tests that CreateScheduleAsync verifies PostAsync is called exactly once per invocation.
    /// Input: Valid ScheduleDto.
    /// Expected: PostAsync is invoked exactly once with correct endpoint.
    /// </summary>
    [TestMethod]
    public async Task CreateScheduleAsync_ValidSchedule_CallsPostAsyncExactlyOnce()
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<ScheduleService>>();
        var service = new ScheduleService(mockApi.Object, mockLogger.Object);

        var schedule = new ScheduleDto
        {
            Id = "test-id",
            CourseId = "CS202",
            CourseName = "Data Structures"
        };

        var response = new HttpResponseMessage(HttpStatusCode.OK);
        mockApi.Setup(x => x.PostAsync("schedules", It.IsAny<HttpContent>(), It.IsAny<CancellationToken>()))
               .ReturnsAsync(response);

        // Act
        await service.CreateScheduleAsync(schedule);

        // Assert
        mockApi.Verify(
            x => x.PostAsync("schedules", It.IsAny<HttpContent>(), It.IsAny<CancellationToken>()),
            Times.Once);
        mockApi.VerifyNoOtherCalls();
    }

    /// <summary>
    /// Tests that CreateScheduleAsync handles multiple consecutive calls correctly.
    /// Input: Two valid ScheduleDto objects called sequentially.
    /// Expected: Both calls succeed and each triggers separate API calls.
    /// </summary>
    [TestMethod]
    public async Task CreateScheduleAsync_MultipleConsecutiveCalls_HandlesEachCallIndependently()
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<ScheduleService>>();
        var service = new ScheduleService(mockApi.Object, mockLogger.Object);

        var schedule1 = new ScheduleDto { Id = "1", CourseId = "CS101" };
        var schedule2 = new ScheduleDto { Id = "2", CourseId = "CS102" };

        var response1 = new HttpResponseMessage(HttpStatusCode.Created);
        var response2 = new HttpResponseMessage(HttpStatusCode.OK);

        mockApi.SetupSequence(x => x.PostAsync("schedules", It.IsAny<HttpContent>(), It.IsAny<CancellationToken>()))
               .ReturnsAsync(response1)
               .ReturnsAsync(response2);

        // Act
        var result1 = await service.CreateScheduleAsync(schedule1);
        var result2 = await service.CreateScheduleAsync(schedule2);

        // Assert
        Assert.IsTrue(result1);
        Assert.IsTrue(result2);
        mockApi.Verify(
            x => x.PostAsync("schedules", It.IsAny<HttpContent>(), It.IsAny<CancellationToken>()),
            Times.Exactly(2));
    }

    /// <summary>
    /// Tests that CreateScheduleAsync handles ScheduleDto with boundary DateTime values.
    /// Input: ScheduleDto with DateTime values at specific boundaries (today, specific times).
    /// Expected: Method executes without exception and returns based on API response.
    /// </summary>
    [TestMethod]
    public async Task CreateScheduleAsync_ScheduleWithBoundaryDateTimeValues_ExecutesSuccessfully()
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<ScheduleService>>();
        var service = new ScheduleService(mockApi.Object, mockLogger.Object);

        var schedule = new ScheduleDto
        {
            Id = "boundary-test",
            CourseId = "CS999",
            StartTime = new DateTime(2024, 1, 1, 0, 0, 0),
            EndTime = new DateTime(2024, 12, 31, 23, 59, 59)
        };

        var successResponse = new HttpResponseMessage(HttpStatusCode.OK);
        mockApi.Setup(x => x.PostAsync("schedules", It.IsAny<HttpContent>(), It.IsAny<CancellationToken>()))
               .ReturnsAsync(successResponse);

        // Act
        var result = await service.CreateScheduleAsync(schedule);

        // Assert
        Assert.IsTrue(result);
    }

    /// <summary>
    /// Tests that CreateScheduleAsync handles redirection status codes correctly.
    /// Input: Valid ScheduleDto with HTTP redirection status codes (3xx).
    /// Expected: Returns false for redirection codes as they are not success codes, and logs warning.
    /// </summary>
    [TestMethod]
    [DataRow(HttpStatusCode.MovedPermanently, DisplayName = "301 Moved Permanently")]
    [DataRow(HttpStatusCode.Found, DisplayName = "302 Found")]
    [DataRow(HttpStatusCode.TemporaryRedirect, DisplayName = "307 Temporary Redirect")]
    public async Task CreateScheduleAsync_RedirectionStatusCodes_ReturnsFalseAndLogsWarning(HttpStatusCode statusCode)
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<ScheduleService>>();
        var service = new ScheduleService(mockApi.Object, mockLogger.Object);

        var schedule = new ScheduleDto { Id = "redirect-test" };

        var response = new HttpResponseMessage(statusCode);
        mockApi.Setup(x => x.PostAsync("schedules", It.IsAny<HttpContent>(), It.IsAny<CancellationToken>()))
               .ReturnsAsync(response);

        // Act
        var result = await service.CreateScheduleAsync(schedule);

        // Assert
        Assert.IsFalse(result);
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("CreateSchedule returned")),
                It.IsAny<Exception?>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    /// <summary>
    /// Tests that CancelScheduleAsync handles unicode characters in scheduleId.
    /// Input: ScheduleId containing unicode characters from various scripts.
    /// Expected: Method executes successfully and passes unicode characters to the API.
    /// </summary>
    [TestMethod]
    public async Task CancelScheduleAsync_UnicodeScheduleId_CallsApiWithUnicode()
    {
        // Arrange
        var scheduleId = "テスト-测试-тест-🎉";
        var mockApi = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<ScheduleService>>();
        var response = new HttpResponseMessage(HttpStatusCode.OK);

        mockApi.Setup(x => x.DeleteAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        var service = new ScheduleService(mockApi.Object, mockLogger.Object);

        // Act
        var result = await service.CancelScheduleAsync(scheduleId);

        // Assert
        Assert.IsTrue(result);
        mockApi.Verify(x => x.DeleteAsync($"schedules/{scheduleId}", It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Tests that CancelScheduleAsync handles control characters in scheduleId.
    /// Input: ScheduleId containing control characters like newline and tab.
    /// Expected: Method executes successfully and passes control characters to the API.
    /// </summary>
    [TestMethod]
    public async Task CancelScheduleAsync_ControlCharactersInScheduleId_CallsApiWithControlCharacters()
    {
        // Arrange
        var scheduleId = "test\n\t\r";
        var mockApi = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<ScheduleService>>();
        var response = new HttpResponseMessage(HttpStatusCode.OK);

        mockApi.Setup(x => x.DeleteAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        var service = new ScheduleService(mockApi.Object, mockLogger.Object);

        // Act
        var result = await service.CancelScheduleAsync(scheduleId);

        // Assert
        Assert.IsTrue(result);
        mockApi.Verify(x => x.DeleteAsync($"schedules/{scheduleId}", It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Tests that CancelScheduleAsync handles additional 3xx redirection status codes correctly.
    /// Input: Valid scheduleId with redirection status codes.
    /// Expected: Returns false as redirection codes are not considered successful, and logs a warning.
    /// </summary>
    [TestMethod]
    [DataRow(HttpStatusCode.MovedPermanently)]
    [DataRow(HttpStatusCode.Found)]
    [DataRow(HttpStatusCode.SeeOther)]
    public async Task CancelScheduleAsync_RedirectionStatusCode_ReturnsFalseAndLogsWarning(HttpStatusCode statusCode)
    {
        // Arrange
        var scheduleId = "test-schedule-redirect";
        var mockApi = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<ScheduleService>>();
        var response = new HttpResponseMessage(statusCode);

        mockApi.Setup(x => x.DeleteAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        var service = new ScheduleService(mockApi.Object, mockLogger.Object);

        // Act
        var result = await service.CancelScheduleAsync(scheduleId);

        // Assert
        Assert.IsFalse(result);
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("CancelSchedule returned")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    /// <summary>
    /// Tests that CancelScheduleAsync logs the exact status code in the warning message.
    /// Input: Valid scheduleId with NotFound status code.
    /// Expected: Warning message contains "NotFound" status code.
    /// </summary>
    [TestMethod]
    public async Task CancelScheduleAsync_FailureResponse_LogsCorrectStatusCode()
    {
        // Arrange
        var scheduleId = "test-schedule-log";
        var mockApi = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<ScheduleService>>();
        var response = new HttpResponseMessage(HttpStatusCode.NotFound);

        mockApi.Setup(x => x.DeleteAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        var service = new ScheduleService(mockApi.Object, mockLogger.Object);

        // Act
        await service.CancelScheduleAsync(scheduleId);

        // Assert
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("NotFound")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    /// <summary>
    /// Tests that GetSchedulesAsync verifies the correct endpoint is called.
    /// Input: Valid API service mock.
    /// Expected: GetAsync is called with "schedules" endpoint exactly once.
    /// </summary>
    [TestMethod]
    public async Task GetSchedulesAsync_ValidCall_CallsCorrectEndpoint()
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<ScheduleService>>();
        var service = new ScheduleService(mockApi.Object, mockLogger.Object);

        var emptySchedules = new List<ScheduleDto>();
        var jsonContent = JsonContent.Create(emptySchedules);
        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = jsonContent
        };

        mockApi.Setup(x => x.GetAsync("schedules", It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        // Act
        await service.GetSchedulesAsync();

        // Assert
        mockApi.Verify(x => x.GetAsync("schedules", It.IsAny<CancellationToken>()), Times.Once);
        mockApi.VerifyNoOtherCalls();
    }

    /// <summary>
    /// Tests that GetSchedulesAsync does not log when the response is successful.
    /// Input: Successful HTTP response with schedules.
    /// Expected: No warning is logged.
    /// </summary>
    [TestMethod]
    public async Task GetSchedulesAsync_SuccessfulResponse_DoesNotLogWarning()
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<ScheduleService>>();
        var service = new ScheduleService(mockApi.Object, mockLogger.Object);

        var schedules = new List<ScheduleDto>
        {
            new ScheduleDto { Id = "1", CourseId = "CS101" }
        };
        var jsonContent = JsonContent.Create(schedules);
        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = jsonContent
        };

        mockApi.Setup(x => x.GetAsync("schedules", It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        // Act
        await service.GetSchedulesAsync();

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
    /// Tests that GetSchedulesAsync logs warning with correct status code for non-success responses.
    /// Input: HTTP response with specific non-success status code.
    /// Expected: Warning is logged containing the exact status code.
    /// </summary>
    [TestMethod]
    [DataRow(HttpStatusCode.BadRequest)]
    [DataRow(HttpStatusCode.NotFound)]
    [DataRow(HttpStatusCode.InternalServerError)]
    public async Task GetSchedulesAsync_NonSuccessResponse_LogsCorrectStatusCode(HttpStatusCode statusCode)
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<ScheduleService>>();
        var service = new ScheduleService(mockApi.Object, mockLogger.Object);

        var response = new HttpResponseMessage(statusCode);

        mockApi.Setup(x => x.GetAsync("schedules", It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        // Act
        var result = await service.GetSchedulesAsync();

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(0, result.Count());

        mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains(statusCode.ToString())),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    /// <summary>
    /// Tests that GetSchedulesAsync returns empty collection for 1xx informational status codes.
    /// Input: HTTP response with informational status codes (100-199).
    /// Expected: Returns empty collection and logs warning.
    /// </summary>
    [TestMethod]
    [DataRow(HttpStatusCode.Continue)]
    [DataRow(HttpStatusCode.SwitchingProtocols)]
    public async Task GetSchedulesAsync_InformationalStatusCode_ReturnsEmptyCollectionAndLogsWarning(HttpStatusCode statusCode)
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<ScheduleService>>();
        var service = new ScheduleService(mockApi.Object, mockLogger.Object);

        var response = new HttpResponseMessage(statusCode);

        mockApi.Setup(x => x.GetAsync("schedules", It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        // Act
        var result = await service.GetSchedulesAsync();

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(0, result.Count());

        mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("GetSchedules returned")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    /// <summary>
    /// Tests that GetSchedulesAsync returns empty collection for 3xx redirection status codes.
    /// Input: HTTP response with redirection status codes (300-399).
    /// Expected: Returns empty collection and logs warning.
    /// </summary>
    [TestMethod]
    [DataRow(HttpStatusCode.MovedPermanently)]
    [DataRow(HttpStatusCode.Found)]
    [DataRow(HttpStatusCode.RedirectMethod)]
    public async Task GetSchedulesAsync_RedirectionStatusCode_ReturnsEmptyCollectionAndLogsWarning(HttpStatusCode statusCode)
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<ScheduleService>>();
        var service = new ScheduleService(mockApi.Object, mockLogger.Object);

        var response = new HttpResponseMessage(statusCode);

        mockApi.Setup(x => x.GetAsync("schedules", It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        // Act
        var result = await service.GetSchedulesAsync();

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(0, result.Count());

        mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("GetSchedules returned")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }


    /// <summary>
    /// Tests that CancelScheduleAsync invokes DeleteAsync exactly once per call.
    /// Input: Valid scheduleId.
    /// Expected: DeleteAsync is called exactly once.
    /// </summary>
    [TestMethod]
    public async Task CancelScheduleAsync_ValidScheduleId_CallsDeleteAsyncExactlyOnce()
    {
        // Arrange
        var scheduleId = "test-schedule-123";
        var mockApi = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<ScheduleService>>();
        var response = new HttpResponseMessage(HttpStatusCode.OK);

        mockApi.Setup(x => x.DeleteAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        var service = new ScheduleService(mockApi.Object, mockLogger.Object);

        // Act
        await service.CancelScheduleAsync(scheduleId);

        // Assert
        mockApi.Verify(x => x.DeleteAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Tests that CreateScheduleAsync returns true when the API returns a successful status code.
    /// Input: Valid ScheduleDto with HTTP 200 OK response.
    /// Expected: Returns true and no warning is logged.
    /// </summary>
    [TestMethod]
    public async Task CreateScheduleAsync_SuccessfulResponse_ReturnsTrue()
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<ScheduleService>>();
        var service = new ScheduleService(mockApi.Object, mockLogger.Object);

        var schedule = new ScheduleDto
        {
            Id = "test-id",
            CourseId = "CS101",
            CourseName = "Test Course",
            LecturerName = "Dr. Test",
            StartTime = DateTime.Now,
            EndTime = DateTime.Now.AddHours(2),
            Venue = "Room 101"
        };

        var successResponse = new HttpResponseMessage(HttpStatusCode.OK);
        mockApi.Setup(x => x.PostAsync("schedules", It.IsAny<HttpContent>(), It.IsAny<CancellationToken>()))
               .ReturnsAsync(successResponse);

        // Act
        var result = await service.CreateScheduleAsync(schedule);

        // Assert
        Assert.IsTrue(result);
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
    /// Tests that CreateScheduleAsync handles various successful HTTP status codes correctly.
    /// Input: Valid ScheduleDto with different success status codes.
    /// Expected: Returns true for all success codes and no warnings are logged.
    /// </summary>
    [TestMethod]
    [DataRow(HttpStatusCode.OK, DisplayName = "200 OK")]
    [DataRow(HttpStatusCode.Created, DisplayName = "201 Created")]
    [DataRow(HttpStatusCode.Accepted, DisplayName = "202 Accepted")]
    [DataRow(HttpStatusCode.NoContent, DisplayName = "204 No Content")]
    public async Task CreateScheduleAsync_SuccessStatusCodes_ReturnsTrue(HttpStatusCode statusCode)
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<ScheduleService>>();
        var service = new ScheduleService(mockApi.Object, mockLogger.Object);

        var schedule = new ScheduleDto { Id = "test" };
        var response = new HttpResponseMessage(statusCode);
        mockApi.Setup(x => x.PostAsync("schedules", It.IsAny<HttpContent>(), It.IsAny<CancellationToken>()))
               .ReturnsAsync(response);

        // Act
        var result = await service.CreateScheduleAsync(schedule);

        // Assert
        Assert.IsTrue(result);
    }

    /// <summary>
    /// Tests that CreateScheduleAsync returns false and logs warning for client error status codes.
    /// Input: Valid ScheduleDto with client error status codes (4xx).
    /// Expected: Returns false and logs warning with status code.
    /// </summary>
    [TestMethod]
    [DataRow(HttpStatusCode.BadRequest, DisplayName = "400 Bad Request")]
    [DataRow(HttpStatusCode.Unauthorized, DisplayName = "401 Unauthorized")]
    [DataRow(HttpStatusCode.Forbidden, DisplayName = "403 Forbidden")]
    [DataRow(HttpStatusCode.NotFound, DisplayName = "404 Not Found")]
    [DataRow(HttpStatusCode.Conflict, DisplayName = "409 Conflict")]
    public async Task CreateScheduleAsync_ClientErrorStatusCodes_ReturnsFalseAndLogsWarning(HttpStatusCode statusCode)
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<ScheduleService>>();
        var service = new ScheduleService(mockApi.Object, mockLogger.Object);

        var schedule = new ScheduleDto { Id = "test" };
        var response = new HttpResponseMessage(statusCode);
        mockApi.Setup(x => x.PostAsync("schedules", It.IsAny<HttpContent>(), It.IsAny<CancellationToken>()))
               .ReturnsAsync(response);

        // Act
        var result = await service.CreateScheduleAsync(schedule);

        // Assert
        Assert.IsFalse(result);
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("CreateSchedule returned")),
                It.IsAny<Exception?>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    /// <summary>
    /// Tests that CreateScheduleAsync returns false and logs warning for server error status codes.
    /// Input: Valid ScheduleDto with server error status codes (5xx).
    /// Expected: Returns false and logs warning with status code.
    /// </summary>
    [TestMethod]
    [DataRow(HttpStatusCode.InternalServerError, DisplayName = "500 Internal Server Error")]
    [DataRow(HttpStatusCode.BadGateway, DisplayName = "502 Bad Gateway")]
    [DataRow(HttpStatusCode.ServiceUnavailable, DisplayName = "503 Service Unavailable")]
    [DataRow(HttpStatusCode.GatewayTimeout, DisplayName = "504 Gateway Timeout")]
    public async Task CreateScheduleAsync_ServerErrorStatusCodes_ReturnsFalseAndLogsWarning(HttpStatusCode statusCode)
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<ScheduleService>>();
        var service = new ScheduleService(mockApi.Object, mockLogger.Object);

        var schedule = new ScheduleDto { Id = "test" };
        var response = new HttpResponseMessage(statusCode);
        mockApi.Setup(x => x.PostAsync("schedules", It.IsAny<HttpContent>(), It.IsAny<CancellationToken>()))
               .ReturnsAsync(response);

        // Act
        var result = await service.CreateScheduleAsync(schedule);

        // Assert
        Assert.IsFalse(result);
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("CreateSchedule returned")),
                It.IsAny<Exception?>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    /// <summary>
    /// Tests that CreateScheduleAsync calls PostAsync with correct endpoint.
    /// Input: Valid ScheduleDto.
    /// Expected: PostAsync is called with "schedules" endpoint.
    /// </summary>
    [TestMethod]
    public async Task CreateScheduleAsync_ValidSchedule_CallsCorrectEndpoint()
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<ScheduleService>>();
        var service = new ScheduleService(mockApi.Object, mockLogger.Object);

        var schedule = new ScheduleDto { Id = "test-id" };
        var response = new HttpResponseMessage(HttpStatusCode.OK);
        mockApi.Setup(x => x.PostAsync("schedules", It.IsAny<HttpContent>(), It.IsAny<CancellationToken>()))
               .ReturnsAsync(response);

        // Act
        await service.CreateScheduleAsync(schedule);

        // Assert
        mockApi.Verify(
            x => x.PostAsync("schedules", It.IsAny<HttpContent>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    /// <summary>
    /// Tests that CreateScheduleAsync handles ScheduleDto with very long string values.
    /// Input: ScheduleDto with string properties containing 10000 characters.
    /// Expected: Method executes successfully and returns based on API response.
    /// </summary>
    [TestMethod]
    public async Task CreateScheduleAsync_ScheduleWithVeryLongStrings_ExecutesSuccessfully()
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<ScheduleService>>();
        var service = new ScheduleService(mockApi.Object, mockLogger.Object);

        var longString = new string('x', 10000);
        var schedule = new ScheduleDto
        {
            Id = longString,
            CourseId = longString,
            CourseName = longString,
            LecturerName = longString,
            Venue = longString
        };

        var response = new HttpResponseMessage(HttpStatusCode.OK);
        mockApi.Setup(x => x.PostAsync("schedules", It.IsAny<HttpContent>(), It.IsAny<CancellationToken>()))
               .ReturnsAsync(response);

        // Act
        var result = await service.CreateScheduleAsync(schedule);

        // Assert
        Assert.IsTrue(result);
    }

    /// <summary>
    /// Tests that CreateScheduleAsync handles ScheduleDto with whitespace-only string properties.
    /// Input: ScheduleDto with string properties containing only whitespace.
    /// Expected: Method executes successfully and returns based on API response.
    /// </summary>
    [TestMethod]
    public async Task CreateScheduleAsync_ScheduleWithWhitespaceStrings_ExecutesSuccessfully()
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<ScheduleService>>();
        var service = new ScheduleService(mockApi.Object, mockLogger.Object);

        var schedule = new ScheduleDto
        {
            Id = "   ",
            CourseId = "\t\t",
            CourseName = "  \n  ",
            LecturerName = "    ",
            Venue = "\r\n"
        };

        var response = new HttpResponseMessage(HttpStatusCode.OK);
        mockApi.Setup(x => x.PostAsync("schedules", It.IsAny<HttpContent>(), It.IsAny<CancellationToken>()))
               .ReturnsAsync(response);

        // Act
        var result = await service.CreateScheduleAsync(schedule);

        // Assert
        Assert.IsTrue(result);
    }

    /// <summary>
    /// Tests that CreateScheduleAsync handles ScheduleDto with extreme DateTime values.
    /// Input: ScheduleDto with DateTime.MinValue and DateTime.MaxValue.
    /// Expected: Method executes successfully and returns based on API response.
    /// </summary>
    [TestMethod]
    public async Task CreateScheduleAsync_ScheduleWithExtremeDateTimeValues_ExecutesSuccessfully()
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<ScheduleService>>();
        var service = new ScheduleService(mockApi.Object, mockLogger.Object);

        var schedule = new ScheduleDto
        {
            Id = "test",
            StartTime = DateTime.MinValue,
            EndTime = DateTime.MaxValue
        };

        var response = new HttpResponseMessage(HttpStatusCode.OK);
        mockApi.Setup(x => x.PostAsync("schedules", It.IsAny<HttpContent>(), It.IsAny<CancellationToken>()))
               .ReturnsAsync(response);

        // Act
        var result = await service.CreateScheduleAsync(schedule);

        // Assert
        Assert.IsTrue(result);
    }

    /// <summary>
    /// Tests that CreateScheduleAsync handles ScheduleDto with default DateTime value.
    /// Input: ScheduleDto with default DateTime (0001-01-01).
    /// Expected: Method executes successfully and returns based on API response.
    /// </summary>
    [TestMethod]
    public async Task CreateScheduleAsync_ScheduleWithDefaultDateTime_ExecutesSuccessfully()
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<ScheduleService>>();
        var service = new ScheduleService(mockApi.Object, mockLogger.Object);

        var schedule = new ScheduleDto
        {
            Id = "test",
            StartTime = default,
            EndTime = default
        };

        var response = new HttpResponseMessage(HttpStatusCode.OK);
        mockApi.Setup(x => x.PostAsync("schedules", It.IsAny<HttpContent>(), It.IsAny<CancellationToken>()))
               .ReturnsAsync(response);

        // Act
        var result = await service.CreateScheduleAsync(schedule);

        // Assert
        Assert.IsTrue(result);
    }

    /// <summary>
    /// Tests that CreateScheduleAsync logs the exact status code in warning message.
    /// Input: Valid ScheduleDto with specific non-success status codes.
    /// Expected: Warning message contains the exact status code.
    /// </summary>
    [TestMethod]
    [DataRow(HttpStatusCode.BadRequest)]
    [DataRow(HttpStatusCode.NotFound)]
    [DataRow(HttpStatusCode.InternalServerError)]
    public async Task CreateScheduleAsync_FailureResponse_LogsExactStatusCode(HttpStatusCode statusCode)
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<ScheduleService>>();
        var service = new ScheduleService(mockApi.Object, mockLogger.Object);

        var schedule = new ScheduleDto { Id = "test" };
        var response = new HttpResponseMessage(statusCode);
        mockApi.Setup(x => x.PostAsync("schedules", It.IsAny<HttpContent>(), It.IsAny<CancellationToken>()))
               .ReturnsAsync(response);

        // Act
        await service.CreateScheduleAsync(schedule);

        // Assert
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains(statusCode.ToString())),
                It.IsAny<Exception?>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    /// <summary>
    /// Tests that CreateScheduleAsync calls PostAsync exactly once per invocation.
    /// Input: Valid ScheduleDto.
    /// Expected: PostAsync is called exactly once.
    /// </summary>
    [TestMethod]
    public async Task CreateScheduleAsync_SingleCall_InvokesPostAsyncOnce()
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<ScheduleService>>();
        var service = new ScheduleService(mockApi.Object, mockLogger.Object);

        var schedule = new ScheduleDto { Id = "test" };
        var response = new HttpResponseMessage(HttpStatusCode.OK);
        mockApi.Setup(x => x.PostAsync("schedules", It.IsAny<HttpContent>(), It.IsAny<CancellationToken>()))
               .ReturnsAsync(response);

        // Act
        await service.CreateScheduleAsync(schedule);

        // Assert
        mockApi.Verify(
            x => x.PostAsync("schedules", It.IsAny<HttpContent>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    /// <summary>
    /// Tests that CreateScheduleAsync handles multiple consecutive calls independently.
    /// Input: Multiple valid ScheduleDto objects.
    /// Expected: Each call results in a separate API invocation and independent result.
    /// </summary>
    [TestMethod]
    public async Task CreateScheduleAsync_MultipleCalls_HandlesEachIndependently()
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<ScheduleService>>();
        var service = new ScheduleService(mockApi.Object, mockLogger.Object);

        var schedule1 = new ScheduleDto { Id = "test1" };
        var schedule2 = new ScheduleDto { Id = "test2" };

        var response1 = new HttpResponseMessage(HttpStatusCode.OK);
        var response2 = new HttpResponseMessage(HttpStatusCode.BadRequest);

        mockApi.SetupSequence(x => x.PostAsync("schedules", It.IsAny<HttpContent>(), It.IsAny<CancellationToken>()))
               .ReturnsAsync(response1)
               .ReturnsAsync(response2);

        // Act
        var result1 = await service.CreateScheduleAsync(schedule1);
        var result2 = await service.CreateScheduleAsync(schedule2);

        // Assert
        Assert.IsTrue(result1);
        Assert.IsFalse(result2);
        mockApi.Verify(
            x => x.PostAsync("schedules", It.IsAny<HttpContent>(), It.IsAny<CancellationToken>()),
            Times.Exactly(2));
    }

    /// <summary>
    /// Tests that CreateScheduleAsync handles ScheduleDto with all properties set to default values.
    /// Input: ScheduleDto with all default property values.
    /// Expected: Method executes successfully and returns based on API response.
    /// </summary>
    [TestMethod]
    public async Task CreateScheduleAsync_ScheduleWithAllDefaults_ExecutesSuccessfully()
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<ScheduleService>>();
        var service = new ScheduleService(mockApi.Object, mockLogger.Object);

        var schedule = new ScheduleDto();

        var response = new HttpResponseMessage(HttpStatusCode.OK);
        mockApi.Setup(x => x.PostAsync("schedules", It.IsAny<HttpContent>(), It.IsAny<CancellationToken>()))
               .ReturnsAsync(response);

        // Act
        var result = await service.CreateScheduleAsync(schedule);

        // Assert
        Assert.IsTrue(result);
    }

    /// <summary>
    /// Tests that GetSchedulesAsync calls GetAsync with default CancellationToken.
    /// Input: Valid mock setup.
    /// Expected: GetAsync is called with default CancellationToken parameter.
    /// </summary>
    [TestMethod]
    public async Task GetSchedulesAsync_ValidCall_PassesDefaultCancellationToken()
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<ScheduleService>>();
        var service = new ScheduleService(mockApi.Object, mockLogger.Object);

        var emptySchedules = new List<ScheduleDto>();
        var jsonContent = JsonContent.Create(emptySchedules);
        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = jsonContent
        };

        CancellationToken capturedToken = default;
        mockApi.Setup(x => x.GetAsync("schedules", It.IsAny<CancellationToken>()))
            .ReturnsAsync(response)
            .Callback<string, CancellationToken>((_, ct) => capturedToken = ct);

        // Act
        await service.GetSchedulesAsync();

        // Assert
        Assert.AreEqual(default(CancellationToken), capturedToken);
        mockApi.Verify(x => x.GetAsync("schedules", It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Tests that GetSchedulesAsync handles JSON array with mixed valid and default-valued schedules.
    /// Input: JSON containing schedules with some properties at default values.
    /// Expected: Returns all schedules including those with default values.
    /// </summary>
    [TestMethod]
    public async Task GetSchedulesAsync_JsonWithDefaultValues_ReturnsAllSchedules()
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<ScheduleService>>();
        var service = new ScheduleService(mockApi.Object, mockLogger.Object);

        var schedules = new List<ScheduleDto>
        {
            new ScheduleDto
            {
                Id = string.Empty,
                CourseId = string.Empty,
                CourseName = string.Empty,
                LecturerName = string.Empty,
                StartTime = default,
                EndTime = default,
                Venue = string.Empty,
                IsCancelled = default
            }
        };

        var jsonContent = JsonContent.Create(schedules);
        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = jsonContent
        };

        mockApi.Setup(x => x.GetAsync("schedules", It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        // Act
        var result = await service.GetSchedulesAsync();

        // Assert
        Assert.IsNotNull(result);
        var schedulesList = result.ToList();
        Assert.AreEqual(1, schedulesList.Count);
        Assert.AreEqual(string.Empty, schedulesList[0].Id);
        Assert.AreEqual(default(DateTime), schedulesList[0].StartTime);
    }
}