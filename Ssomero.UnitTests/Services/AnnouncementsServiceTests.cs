using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
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
/// Unit tests for the AnnouncementsService class.
/// </summary>
[TestClass]
public class AnnouncementsServiceTests
{
    /// <summary>
    /// Tests that GetAnnouncementsAsync returns announcements when API returns success with valid JSON content.
    /// Input: API returns 200 OK with valid JSON containing announcements.
    /// Expected: Returns the deserialized list of announcements.
    /// </summary>
    [TestMethod]
    public async Task GetAnnouncementsAsync_SuccessWithValidData_ReturnsAnnouncements()
    {
        // Arrange
        Mock<IApiService> mockApiService = new Mock<IApiService>();
        Mock<ILogger<AnnouncementsService>> mockLogger = new Mock<ILogger<AnnouncementsService>>();

        List<AnnouncementDto> expectedAnnouncements = new List<AnnouncementDto>
        {
            new AnnouncementDto { Title = "Announcement 1", Body = "Body 1", Date = new DateTime(2024, 1, 1) },
            new AnnouncementDto { Title = "Announcement 2", Body = "Body 2", Date = new DateTime(2024, 1, 2) }
        };

        string jsonContent = JsonSerializer.Serialize(expectedAnnouncements);
        HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(jsonContent, System.Text.Encoding.UTF8, "application/json")
        };

        mockApiService.Setup(x => x.GetAsync("announcements", It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        AnnouncementsService service = new AnnouncementsService(mockApiService.Object, mockLogger.Object);

        // Act
        IEnumerable<AnnouncementDto> result = await service.GetAnnouncementsAsync();

        // Assert
        Assert.IsNotNull(result);
        List<AnnouncementDto> resultList = result.ToList();
        Assert.AreEqual(2, resultList.Count);
        Assert.AreEqual("Announcement 1", resultList[0].Title);
        Assert.AreEqual("Body 1", resultList[0].Body);
        Assert.AreEqual("Announcement 2", resultList[1].Title);
        Assert.AreEqual("Body 2", resultList[1].Body);
    }

    /// <summary>
    /// Tests that GetAnnouncementsAsync returns empty collection when API returns success with empty array.
    /// Input: API returns 200 OK with empty JSON array.
    /// Expected: Returns an empty collection.
    /// </summary>
    [TestMethod]
    public async Task GetAnnouncementsAsync_SuccessWithEmptyArray_ReturnsEmptyCollection()
    {
        // Arrange
        Mock<IApiService> mockApiService = new Mock<IApiService>();
        Mock<ILogger<AnnouncementsService>> mockLogger = new Mock<ILogger<AnnouncementsService>>();

        string jsonContent = "[]";
        HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(jsonContent, System.Text.Encoding.UTF8, "application/json")
        };

        mockApiService.Setup(x => x.GetAsync("announcements", It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        AnnouncementsService service = new AnnouncementsService(mockApiService.Object, mockLogger.Object);

        // Act
        IEnumerable<AnnouncementDto> result = await service.GetAnnouncementsAsync();

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(0, result.Count());
    }

    /// <summary>
    /// Tests that GetAnnouncementsAsync returns empty collection when API returns success but content deserializes to null.
    /// Input: API returns 200 OK with null JSON content.
    /// Expected: Returns an empty collection instead of null.
    /// </summary>
    [TestMethod]
    public async Task GetAnnouncementsAsync_SuccessWithNullContent_ReturnsEmptyCollection()
    {
        // Arrange
        Mock<IApiService> mockApiService = new Mock<IApiService>();
        Mock<ILogger<AnnouncementsService>> mockLogger = new Mock<ILogger<AnnouncementsService>>();

        string jsonContent = "null";
        HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(jsonContent, System.Text.Encoding.UTF8, "application/json")
        };

        mockApiService.Setup(x => x.GetAsync("announcements", It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        AnnouncementsService service = new AnnouncementsService(mockApiService.Object, mockLogger.Object);

        // Act
        IEnumerable<AnnouncementDto> result = await service.GetAnnouncementsAsync();

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(0, result.Count());
    }

    /// <summary>
    /// Tests that GetAnnouncementsAsync returns single announcement when API returns success with one item.
    /// Input: API returns 200 OK with JSON containing a single announcement.
    /// Expected: Returns a collection with one announcement.
    /// </summary>
    [TestMethod]
    public async Task GetAnnouncementsAsync_SuccessWithSingleAnnouncement_ReturnsSingleItem()
    {
        // Arrange
        Mock<IApiService> mockApiService = new Mock<IApiService>();
        Mock<ILogger<AnnouncementsService>> mockLogger = new Mock<ILogger<AnnouncementsService>>();

        List<AnnouncementDto> expectedAnnouncements = new List<AnnouncementDto>
        {
            new AnnouncementDto { Title = "Single", Body = "Single Body", Date = new DateTime(2024, 6, 15) }
        };

        string jsonContent = JsonSerializer.Serialize(expectedAnnouncements);
        HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(jsonContent, System.Text.Encoding.UTF8, "application/json")
        };

        mockApiService.Setup(x => x.GetAsync("announcements", It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        AnnouncementsService service = new AnnouncementsService(mockApiService.Object, mockLogger.Object);

        // Act
        IEnumerable<AnnouncementDto> result = await service.GetAnnouncementsAsync();

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(1, result.Count());
        Assert.AreEqual("Single", result.First().Title);
    }

    /// <summary>
    /// Tests that GetAnnouncementsAsync returns empty collection and logs warning when API returns bad request status.
    /// Input: API returns 400 Bad Request status code.
    /// Expected: Returns empty collection and logs warning with status code.
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
    public async Task GetAnnouncementsAsync_NonSuccessStatusCode_ReturnsEmptyCollectionAndLogsWarning(HttpStatusCode statusCode)
    {
        // Arrange
        Mock<IApiService> mockApiService = new Mock<IApiService>();
        Mock<ILogger<AnnouncementsService>> mockLogger = new Mock<ILogger<AnnouncementsService>>();

        HttpResponseMessage response = new HttpResponseMessage(statusCode);

        mockApiService.Setup(x => x.GetAsync("announcements", It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        AnnouncementsService service = new AnnouncementsService(mockApiService.Object, mockLogger.Object);

        // Act
        IEnumerable<AnnouncementDto> result = await service.GetAnnouncementsAsync();

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(0, result.Count());

        mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("GetAnnouncements returned") && v.ToString()!.Contains(statusCode.ToString())),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    /// <summary>
    /// Tests that GetAnnouncementsAsync returns announcements for various 2xx success status codes.
    /// Input: API returns various success status codes (201, 202, 204) with valid JSON.
    /// Expected: Returns the deserialized list of announcements.
    /// </summary>
    [TestMethod]
    [DataRow(HttpStatusCode.Created)]
    [DataRow(HttpStatusCode.Accepted)]
    [DataRow(HttpStatusCode.NoContent)]
    public async Task GetAnnouncementsAsync_Various2xxStatusCodes_ReturnsAnnouncements(HttpStatusCode statusCode)
    {
        // Arrange
        Mock<IApiService> mockApiService = new Mock<IApiService>();
        Mock<ILogger<AnnouncementsService>> mockLogger = new Mock<ILogger<AnnouncementsService>>();

        List<AnnouncementDto> expectedAnnouncements = new List<AnnouncementDto>
        {
            new AnnouncementDto { Title = "Test", Body = "Test Body", Date = new DateTime(2024, 3, 15) }
        };

        string jsonContent = JsonSerializer.Serialize(expectedAnnouncements);
        HttpResponseMessage response = new HttpResponseMessage(statusCode)
        {
            Content = new StringContent(jsonContent, System.Text.Encoding.UTF8, "application/json")
        };

        mockApiService.Setup(x => x.GetAsync("announcements", It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        AnnouncementsService service = new AnnouncementsService(mockApiService.Object, mockLogger.Object);

        // Act
        IEnumerable<AnnouncementDto> result = await service.GetAnnouncementsAsync();

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(1, result.Count());
    }

    /// <summary>
    /// Tests that GetAnnouncementsAsync does not log warning when API returns success status.
    /// Input: API returns 200 OK with valid data.
    /// Expected: No warning is logged.
    /// </summary>
    [TestMethod]
    public async Task GetAnnouncementsAsync_SuccessStatusCode_DoesNotLogWarning()
    {
        // Arrange
        Mock<IApiService> mockApiService = new Mock<IApiService>();
        Mock<ILogger<AnnouncementsService>> mockLogger = new Mock<ILogger<AnnouncementsService>>();

        List<AnnouncementDto> expectedAnnouncements = new List<AnnouncementDto>
        {
            new AnnouncementDto { Title = "Test", Body = "Body", Date = DateTime.Now }
        };

        string jsonContent = JsonSerializer.Serialize(expectedAnnouncements);
        HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(jsonContent, System.Text.Encoding.UTF8, "application/json")
        };

        mockApiService.Setup(x => x.GetAsync("announcements", It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        AnnouncementsService service = new AnnouncementsService(mockApiService.Object, mockLogger.Object);

        // Act
        IEnumerable<AnnouncementDto> result = await service.GetAnnouncementsAsync();

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
    /// Tests that GetAnnouncementsAsync calls API with correct endpoint path.
    /// Input: Method is called.
    /// Expected: API's GetAsync is called with "announcements" path.
    /// </summary>
    [TestMethod]
    public async Task GetAnnouncementsAsync_WhenCalled_CallsApiWithCorrectPath()
    {
        // Arrange
        Mock<IApiService> mockApiService = new Mock<IApiService>();
        Mock<ILogger<AnnouncementsService>> mockLogger = new Mock<ILogger<AnnouncementsService>>();

        string jsonContent = "[]";
        HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(jsonContent, System.Text.Encoding.UTF8, "application/json")
        };

        mockApiService.Setup(x => x.GetAsync("announcements", It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        AnnouncementsService service = new AnnouncementsService(mockApiService.Object, mockLogger.Object);

        // Act
        await service.GetAnnouncementsAsync();

        // Assert
        mockApiService.Verify(x => x.GetAsync("announcements", It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Tests that the constructor successfully creates an instance when provided with valid non-null dependencies.
    /// </summary>
    [TestMethod]
    public void Constructor_WithValidDependencies_CreatesInstance()
    {
        // Arrange
        Mock<IApiService> mockApiService = new Mock<IApiService>();
        Mock<ILogger<AnnouncementsService>> mockLogger = new Mock<ILogger<AnnouncementsService>>();

        // Act
        AnnouncementsService result = new AnnouncementsService(mockApiService.Object, mockLogger.Object);

        // Assert
        Assert.IsNotNull(result);
    }

    /// <summary>
    /// Tests that the constructor accepts a null api parameter without throwing an exception.
    /// Note: The constructor does not perform null validation, so null values are assigned to fields.
    /// </summary>
    [TestMethod]
    public void Constructor_WithNullApiService_DoesNotThrow()
    {
        // Arrange
        IApiService? nullApiService = null;
        Mock<ILogger<AnnouncementsService>> mockLogger = new Mock<ILogger<AnnouncementsService>>();

        // Act
        AnnouncementsService result = new AnnouncementsService(nullApiService!, mockLogger.Object);

        // Assert
        Assert.IsNotNull(result);
    }

    /// <summary>
    /// Tests that the constructor accepts a null logger parameter without throwing an exception.
    /// Note: The constructor does not perform null validation, so null values are assigned to fields.
    /// </summary>
    [TestMethod]
    public void Constructor_WithNullLogger_DoesNotThrow()
    {
        // Arrange
        Mock<IApiService> mockApiService = new Mock<IApiService>();
        ILogger<AnnouncementsService>? nullLogger = null;

        // Act
        AnnouncementsService result = new AnnouncementsService(mockApiService.Object, nullLogger!);

        // Assert
        Assert.IsNotNull(result);
    }

    /// <summary>
    /// Tests that the constructor accepts both null parameters without throwing an exception.
    /// Note: The constructor does not perform null validation, so null values are assigned to fields.
    /// </summary>
    [TestMethod]
    public void Constructor_WithBothParametersNull_DoesNotThrow()
    {
        // Arrange
        IApiService? nullApiService = null;
        ILogger<AnnouncementsService>? nullLogger = null;

        // Act
        AnnouncementsService result = new AnnouncementsService(nullApiService!, nullLogger!);

        // Assert
        Assert.IsNotNull(result);
    }

    /// <summary>
    /// Tests that GetAnnouncementsAsync returns empty collection when API returns redirect status codes.
    /// Input: API returns various 3xx redirect status codes.
    /// Expected: Returns empty collection and logs warning.
    /// </summary>
    [TestMethod]
    [DataRow(HttpStatusCode.MovedPermanently)]
    [DataRow(HttpStatusCode.Found)]
    [DataRow(HttpStatusCode.SeeOther)]
    [DataRow(HttpStatusCode.TemporaryRedirect)]
    public async Task GetAnnouncementsAsync_RedirectStatusCode_ReturnsEmptyCollectionAndLogsWarning(HttpStatusCode statusCode)
    {
        // Arrange
        Mock<IApiService> mockApiService = new Mock<IApiService>();
        Mock<ILogger<AnnouncementsService>> mockLogger = new Mock<ILogger<AnnouncementsService>>();

        HttpResponseMessage response = new HttpResponseMessage(statusCode);

        mockApiService.Setup(x => x.GetAsync("announcements", It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        AnnouncementsService service = new AnnouncementsService(mockApiService.Object, mockLogger.Object);

        // Act
        IEnumerable<AnnouncementDto> result = await service.GetAnnouncementsAsync();

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(0, result.Count());
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("GetAnnouncements returned")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    /// <summary>
    /// Tests that GetAnnouncementsAsync handles announcements with special characters and edge case dates.
    /// Input: API returns announcements with special characters in title/body and edge case dates.
    /// Expected: Returns the announcements preserving all special characters and dates.
    /// </summary>
    [TestMethod]
    public async Task GetAnnouncementsAsync_AnnouncementsWithSpecialCharacters_ReturnsAnnouncementsPreservingContent()
    {
        // Arrange
        Mock<IApiService> mockApiService = new Mock<IApiService>();
        Mock<ILogger<AnnouncementsService>> mockLogger = new Mock<ILogger<AnnouncementsService>>();

        List<AnnouncementDto> expectedAnnouncements = new List<AnnouncementDto>
        {
            new AnnouncementDto { Title = "Title with \"quotes\" and 'apostrophes'", Body = "Body with\nnewlines\tand\ttabs", Date = DateTime.MinValue },
            new AnnouncementDto { Title = "Title with <html>tags</html>", Body = "Body with émojis 🎉 and unicode ñ", Date = DateTime.MaxValue },
            new AnnouncementDto { Title = "", Body = "", Date = new DateTime(2024, 1, 1) }
        };

        string jsonContent = JsonSerializer.Serialize(expectedAnnouncements);
        HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(jsonContent, System.Text.Encoding.UTF8, "application/json")
        };

        mockApiService.Setup(x => x.GetAsync("announcements", It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        AnnouncementsService service = new AnnouncementsService(mockApiService.Object, mockLogger.Object);

        // Act
        IEnumerable<AnnouncementDto> result = await service.GetAnnouncementsAsync();

        // Assert
        Assert.IsNotNull(result);
        List<AnnouncementDto> resultList = result.ToList();
        Assert.AreEqual(3, resultList.Count);
        Assert.AreEqual("Title with \"quotes\" and 'apostrophes'", resultList[0].Title);
        Assert.AreEqual("Title with <html>tags</html>", resultList[1].Title);
        Assert.AreEqual("", resultList[2].Title);
    }

    /// <summary>
    /// Tests that GetAnnouncementsAsync handles large collection of announcements.
    /// Input: API returns a large number of announcements (1000 items).
    /// Expected: Returns all announcements without errors.
    /// </summary>
    [TestMethod]
    public async Task GetAnnouncementsAsync_LargeCollectionOfAnnouncements_ReturnsAllAnnouncements()
    {
        // Arrange
        Mock<IApiService> mockApiService = new Mock<IApiService>();
        Mock<ILogger<AnnouncementsService>> mockLogger = new Mock<ILogger<AnnouncementsService>>();

        List<AnnouncementDto> expectedAnnouncements = new List<AnnouncementDto>();
        for (int i = 0; i < 1000; i++)
        {
            expectedAnnouncements.Add(new AnnouncementDto
            {
                Title = $"Announcement {i}",
                Body = $"Body {i}",
                Date = new DateTime(2024, 1, 1).AddDays(i)
            });
        }

        string jsonContent = JsonSerializer.Serialize(expectedAnnouncements);
        HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(jsonContent, System.Text.Encoding.UTF8, "application/json")
        };

        mockApiService.Setup(x => x.GetAsync("announcements", It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        AnnouncementsService service = new AnnouncementsService(mockApiService.Object, mockLogger.Object);

        // Act
        IEnumerable<AnnouncementDto> result = await service.GetAnnouncementsAsync();

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(1000, result.Count());
    }
}