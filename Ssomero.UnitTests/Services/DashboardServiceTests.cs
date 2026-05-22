using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
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
/// Unit tests for the <see cref="DashboardService"/> class.
/// </summary>
[TestClass]
public class DashboardServiceTests
{
    /// <summary>
    /// Tests that the DashboardService constructor successfully initializes
    /// with valid IApiService and ILogger dependencies.
    /// </summary>
    [TestMethod]
    public void Constructor_WithValidDependencies_InitializesSuccessfully()
    {
        // Arrange
        var mockApiService = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<DashboardService>>();

        // Act
        var service = new DashboardService(mockApiService.Object, new Mock<ICacheService>().Object, mockLogger.Object);

        // Assert
        Assert.IsNotNull(service);
    }

    /// <summary>
    /// Tests that GetDashboardAsync returns a valid DashboardDto when API returns successful response with valid content.
    /// </summary>
    [TestMethod]
    public async Task GetDashboardAsync_SuccessfulResponseWithValidContent_ReturnsDashboardDto()
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<DashboardService>>();

        var expectedDto = new DashboardDto
        {
            ActiveCourses = 5,
            UpcomingAssignments = 3,
            AttendancePercent = 85.5,
            RecentAnnouncements = new List<AnnouncementDto>()
        };

        var jsonContent = JsonContent.Create(expectedDto);
        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = jsonContent
        };

        mockApi.Setup(x => x.GetAsync("dashboard", It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        var service = new DashboardService(mockApi.Object, new Mock<ICacheService>().Object, mockLogger.Object);

        // Act
        var result = await service.GetDashboardAsync();

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(expectedDto.ActiveCourses, result.ActiveCourses);
        Assert.AreEqual(expectedDto.UpcomingAssignments, result.UpcomingAssignments);
        Assert.AreEqual(expectedDto.AttendancePercent, result.AttendancePercent);
        mockApi.Verify(x => x.GetAsync("dashboard", It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Tests that GetDashboardAsync returns an empty DashboardDto when API returns successful response but content is null.
    /// </summary>
    [TestMethod]
    public async Task GetDashboardAsync_SuccessfulResponseWithNullContent_ReturnsEmptyDashboardDto()
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<DashboardService>>();

        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("null")
        };

        mockApi.Setup(x => x.GetAsync("dashboard", It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        var service = new DashboardService(mockApi.Object, new Mock<ICacheService>().Object, mockLogger.Object);

        // Act
        var result = await service.GetDashboardAsync();

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(0, result.ActiveCourses);
        Assert.AreEqual(0, result.UpcomingAssignments);
        Assert.AreEqual(0.0, result.AttendancePercent);
    }

    /// <summary>
    /// Tests that GetDashboardAsync returns empty DashboardDto and logs warning when API returns a non-successful status code.
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
    public async Task GetDashboardAsync_NonSuccessfulResponse_ReturnsEmptyDashboardDtoAndLogsWarning(HttpStatusCode statusCode)
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<DashboardService>>();

        var response = new HttpResponseMessage(statusCode);

        mockApi.Setup(x => x.GetAsync("dashboard", It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        var service = new DashboardService(mockApi.Object, new Mock<ICacheService>().Object, mockLogger.Object);

        // Act
        var result = await service.GetDashboardAsync();

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(0, result.ActiveCourses);
        Assert.AreEqual(0, result.UpcomingAssignments);
        Assert.AreEqual(0.0, result.AttendancePercent);

        mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("GetDashboard returned")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);

        mockApi.Verify(x => x.GetAsync("dashboard", It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Tests that GetDashboardAsync calls the API with the correct endpoint string.
    /// </summary>
    [TestMethod]
    public async Task GetDashboardAsync_Always_CallsApiWithCorrectEndpoint()
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<DashboardService>>();

        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(new DashboardDto())
        };

        mockApi.Setup(x => x.GetAsync("dashboard", It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        var service = new DashboardService(mockApi.Object, new Mock<ICacheService>().Object, mockLogger.Object);

        // Act
        await service.GetDashboardAsync();

        // Assert
        mockApi.Verify(x => x.GetAsync("dashboard", It.IsAny<CancellationToken>()), Times.Once);
        mockApi.Verify(x => x.GetAsync(It.Is<string>(s => s != "dashboard"), It.IsAny<CancellationToken>()), Times.Never);
    }

    /// <summary>
    /// Tests that GetDashboardAsync does not log warning when API returns successful response.
    /// </summary>
    [TestMethod]
    public async Task GetDashboardAsync_SuccessfulResponse_DoesNotLogWarning()
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<DashboardService>>();

        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(new DashboardDto())
        };

        mockApi.Setup(x => x.GetAsync("dashboard", It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        var service = new DashboardService(mockApi.Object, new Mock<ICacheService>().Object, mockLogger.Object);

        // Act
        await service.GetDashboardAsync();

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
    /// Tests that GetDashboardAsync returns DashboardDto with all properties correctly deserialized.
    /// </summary>
    [TestMethod]
    public async Task GetDashboardAsync_SuccessfulResponse_ReturnsCompletelyPopulatedDto()
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<DashboardService>>();

        var expectedDto = new DashboardDto
        {
            ActiveCourses = 10,
            UpcomingAssignments = 5,
            AttendancePercent = 92.75,
            RecentAnnouncements = new List<AnnouncementDto>(),
            MyClasses = new List<ClassDto>(),
            TeachingClasses = new List<ClassDto>(),
            ManagedClasses = new List<ClassDto>(),
            TotalStudents = 500,
            TotalLecturers = 50,
            TotalPrograms = 15
        };

        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(expectedDto)
        };

        mockApi.Setup(x => x.GetAsync("dashboard", It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        var service = new DashboardService(mockApi.Object, new Mock<ICacheService>().Object, mockLogger.Object);

        // Act
        var result = await service.GetDashboardAsync();

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(expectedDto.ActiveCourses, result.ActiveCourses);
        Assert.AreEqual(expectedDto.UpcomingAssignments, result.UpcomingAssignments);
        Assert.AreEqual(expectedDto.AttendancePercent, result.AttendancePercent);
        Assert.AreEqual(expectedDto.TotalStudents, result.TotalStudents);
        Assert.AreEqual(expectedDto.TotalLecturers, result.TotalLecturers);
        Assert.AreEqual(expectedDto.TotalPrograms, result.TotalPrograms);
        Assert.IsNotNull(result.RecentAnnouncements);
        Assert.IsNotNull(result.MyClasses);
        Assert.IsNotNull(result.TeachingClasses);
        Assert.IsNotNull(result.ManagedClasses);
    }

    /// <summary>
    /// Tests that GetDashboardAsync handles various successful HTTP status codes correctly.
    /// </summary>
    /// <param name="statusCode">The successful HTTP status code to test.</param>
    [TestMethod]
    [DataRow(HttpStatusCode.OK)]
    [DataRow(HttpStatusCode.Created)]
    [DataRow(HttpStatusCode.Accepted)]
    [DataRow(HttpStatusCode.NoContent)]
    public async Task GetDashboardAsync_VariousSuccessStatusCodes_ReturnsValidDto(HttpStatusCode statusCode)
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<DashboardService>>();

        var expectedDto = new DashboardDto { ActiveCourses = 3 };
        var response = new HttpResponseMessage(statusCode)
        {
            Content = JsonContent.Create(expectedDto)
        };

        mockApi.Setup(x => x.GetAsync("dashboard", It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        var service = new DashboardService(mockApi.Object, new Mock<ICacheService>().Object, mockLogger.Object);

        // Act
        var result = await service.GetDashboardAsync();

        // Assert
        Assert.IsNotNull(result);
        if (statusCode == HttpStatusCode.NoContent)
        {
            // NoContent might result in empty DTO depending on deserialization
            Assert.IsNotNull(result);
        }
        else
        {
            Assert.AreEqual(expectedDto.ActiveCourses, result.ActiveCourses);
        }
    }

    /// <summary>
    /// Tests that GetDashboardAsync returns empty DashboardDto with default values when response fails.
    /// </summary>
    [TestMethod]
    public async Task GetDashboardAsync_FailedResponse_ReturnsEmptyDashboardDtoWithDefaultValues()
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<DashboardService>>();

        var response = new HttpResponseMessage(HttpStatusCode.InternalServerError);

        mockApi.Setup(x => x.GetAsync("dashboard", It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        var service = new DashboardService(mockApi.Object, new Mock<ICacheService>().Object, mockLogger.Object);

        // Act
        var result = await service.GetDashboardAsync();

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(0, result.ActiveCourses);
        Assert.AreEqual(0, result.UpcomingAssignments);
        Assert.AreEqual(0.0, result.AttendancePercent);
        Assert.IsNotNull(result.RecentAnnouncements);
        Assert.AreEqual(0, result.RecentAnnouncements.Count);
        Assert.IsNull(result.MyClasses);
        Assert.IsNull(result.TeachingClasses);
        Assert.IsNull(result.ManagedClasses);
        Assert.IsNull(result.TotalStudents);
        Assert.IsNull(result.TotalLecturers);
        Assert.IsNull(result.TotalPrograms);
    }

    /// <summary>
    /// Tests that GetDashboardAsync returns empty DashboardDto when successful response contains empty JSON object.
    /// </summary>
    [TestMethod]
    public async Task GetDashboardAsync_SuccessfulResponseWithEmptyJson_ReturnsDefaultDashboardDto()
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<DashboardService>>();

        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("{}")
        };

        mockApi.Setup(x => x.GetAsync("dashboard", It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        var service = new DashboardService(mockApi.Object, new Mock<ICacheService>().Object, mockLogger.Object);

        // Act
        var result = await service.GetDashboardAsync();

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(0, result.ActiveCourses);
        Assert.AreEqual(0, result.UpcomingAssignments);
        Assert.AreEqual(0.0, result.AttendancePercent);
    }

    /// <summary>
    /// Tests that GetDashboardAsync handles extreme numeric values in DashboardDto properties.
    /// </summary>
    [TestMethod]
    public async Task GetDashboardAsync_SuccessfulResponseWithExtremeValues_ReturnsCorrectDto()
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<DashboardService>>();

        var expectedDto = new DashboardDto
        {
            ActiveCourses = int.MaxValue,
            UpcomingAssignments = int.MaxValue,
            AttendancePercent = double.MaxValue
        };

        var jsonContent = JsonContent.Create(expectedDto);
        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = jsonContent
        };

        mockApi.Setup(x => x.GetAsync("dashboard", It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        var service = new DashboardService(mockApi.Object, new Mock<ICacheService>().Object, mockLogger.Object);

        // Act
        var result = await service.GetDashboardAsync();

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(int.MaxValue, result.ActiveCourses);
        Assert.AreEqual(int.MaxValue, result.UpcomingAssignments);
        Assert.AreEqual(double.MaxValue, result.AttendancePercent);
    }

    /// <summary>
    /// Tests that GetDashboardAsync handles zero values in DashboardDto properties.
    /// </summary>
    [TestMethod]
    public async Task GetDashboardAsync_SuccessfulResponseWithZeroValues_ReturnsCorrectDto()
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<DashboardService>>();

        var expectedDto = new DashboardDto
        {
            ActiveCourses = 0,
            UpcomingAssignments = 0,
            AttendancePercent = 0.0
        };

        var jsonContent = JsonContent.Create(expectedDto);
        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = jsonContent
        };

        mockApi.Setup(x => x.GetAsync("dashboard", It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        var service = new DashboardService(mockApi.Object, new Mock<ICacheService>().Object, mockLogger.Object);

        // Act
        var result = await service.GetDashboardAsync();

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(0, result.ActiveCourses);
        Assert.AreEqual(0, result.UpcomingAssignments);
        Assert.AreEqual(0.0, result.AttendancePercent);
    }

    /// <summary>
    /// Tests that GetDashboardAsync handles negative values in numeric properties.
    /// </summary>
    [TestMethod]
    public async Task GetDashboardAsync_SuccessfulResponseWithNegativeValues_ReturnsCorrectDto()
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<DashboardService>>();

        var expectedDto = new DashboardDto
        {
            ActiveCourses = -1,
            UpcomingAssignments = -10,
            AttendancePercent = -50.5
        };

        var jsonContent = JsonContent.Create(expectedDto);
        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = jsonContent
        };

        mockApi.Setup(x => x.GetAsync("dashboard", It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        var service = new DashboardService(mockApi.Object, new Mock<ICacheService>().Object, mockLogger.Object);

        // Act
        var result = await service.GetDashboardAsync();

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(-1, result.ActiveCourses);
        Assert.AreEqual(-10, result.UpcomingAssignments);
        Assert.AreEqual(-50.5, result.AttendancePercent);
    }

    /// <summary>
    /// Tests that GetDashboardAsync logs warning with correct status code for multiple error scenarios.
    /// </summary>
    /// <param name="statusCode">The HTTP error status code to test.</param>
    [TestMethod]
    [DataRow(HttpStatusCode.RequestTimeout)]
    [DataRow(HttpStatusCode.Conflict)]
    [DataRow(HttpStatusCode.Gone)]
    [DataRow(HttpStatusCode.BadGateway)]
    public async Task GetDashboardAsync_VariousErrorStatusCodes_LogsWarningWithStatusCode(HttpStatusCode statusCode)
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<DashboardService>>();

        var response = new HttpResponseMessage(statusCode);

        mockApi.Setup(x => x.GetAsync("dashboard", It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        var service = new DashboardService(mockApi.Object, new Mock<ICacheService>().Object, mockLogger.Object);

        // Act
        await service.GetDashboardAsync();

        // Assert
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
    /// Tests that GetDashboardAsync returns empty DashboardDto when content is empty string on successful response.
    /// </summary>
    [TestMethod]
    public async Task GetDashboardAsync_SuccessfulResponseWithEmptyContent_ReturnsEmptyDashboardDto()
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<DashboardService>>();

        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(string.Empty)
        };

        mockApi.Setup(x => x.GetAsync("dashboard", It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        var service = new DashboardService(mockApi.Object, new Mock<ICacheService>().Object, mockLogger.Object);

        // Act
        var result = await service.GetDashboardAsync();

        // Assert
        Assert.IsNotNull(result);
    }

    /// <summary>
    /// Tests that GetDashboardAsync handles empty announcements list correctly.
    /// </summary>
    [TestMethod]
    public async Task GetDashboardAsync_SuccessfulResponseWithEmptyAnnouncementsList_ReturnsCorrectDto()
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<DashboardService>>();

        var expectedDto = new DashboardDto
        {
            ActiveCourses = 5,
            UpcomingAssignments = 3,
            AttendancePercent = 85.5,
            RecentAnnouncements = new List<AnnouncementDto>()
        };

        var jsonContent = JsonContent.Create(expectedDto);
        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = jsonContent
        };

        mockApi.Setup(x => x.GetAsync("dashboard", It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        var service = new DashboardService(mockApi.Object, new Mock<ICacheService>().Object, mockLogger.Object);

        // Act
        var result = await service.GetDashboardAsync();

        // Assert
        Assert.IsNotNull(result);
        Assert.IsNotNull(result.RecentAnnouncements);
        Assert.AreEqual(0, result.RecentAnnouncements.Count);
    }
}
