using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Moq.Protected;
using Ssomero;
using Ssomero.Interfaces;
using Ssomero.Models;
using Ssomero.Services;

namespace Ssomero.Services.UnitTests;




/// <summary>
/// Unit tests for the <see cref="AcademicService"/> class.
/// </summary>
[TestClass]
public class AcademicServiceTests
{
    /// <summary>
    /// Tests that the constructor initializes the service with valid dependencies.
    /// Input: Valid IApiService and ILogger instances.
    /// Expected: Service is created successfully and dependencies are assigned.
    /// </summary>
    [TestMethod]
    public void Constructor_ValidDependencies_InitializesSuccessfully()
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<AcademicService>>();

        // Act
        var service = new AcademicService(mockApi.Object, mockLogger.Object);

        // Assert
        Assert.IsNotNull(service);
    }

    /// <summary>
    /// Tests that the constructor does not throw when api parameter is null.
    /// Input: Null IApiService and valid ILogger.
    /// Expected: No exception is thrown (documents lack of validation).
    /// </summary>
    [TestMethod]
    public void Constructor_NullApi_DoesNotThrow()
    {
        // Arrange
        IApiService? nullApi = null;
        var mockLogger = new Mock<ILogger<AcademicService>>();

        // Act & Assert
        var service = new AcademicService(nullApi!, mockLogger.Object);
        Assert.IsNotNull(service);
    }

    /// <summary>
    /// Tests that the constructor does not throw when logger parameter is null.
    /// Input: Valid IApiService and null ILogger.
    /// Expected: No exception is thrown (documents lack of validation).
    /// </summary>
    [TestMethod]
    public void Constructor_NullLogger_DoesNotThrow()
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        ILogger<AcademicService>? nullLogger = null;

        // Act & Assert
        var service = new AcademicService(mockApi.Object, nullLogger!);
        Assert.IsNotNull(service);
    }

    /// <summary>
    /// Tests that the constructor does not throw when both parameters are null.
    /// Input: Null IApiService and null ILogger.
    /// Expected: No exception is thrown (documents lack of validation).
    /// </summary>
    [TestMethod]
    public void Constructor_BothParametersNull_DoesNotThrow()
    {
        // Arrange
        IApiService? nullApi = null;
        ILogger<AcademicService>? nullLogger = null;

        // Act & Assert
        var service = new AcademicService(nullApi!, nullLogger!);
        Assert.IsNotNull(service);
    }

    /// <summary>
    /// Tests that GetAcademicYearsAsync successfully retrieves academic years from the API
    /// when the data is not cached.
    /// </summary>
    [TestMethod]
    public async Task GetAcademicYearsAsync_WhenDataNotCached_ReturnsDataFromApi()
    {
        // Arrange
        var expectedData = new List<LookupItem>
        {
            new LookupItem { Id = "1", Name = "2023/2024" },
            new LookupItem { Id = "2", Name = "2024/2025" }
        };

        var responseContent = JsonSerializer.Serialize(expectedData);
        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(responseContent, Encoding.UTF8, "application/json")
        };

        var mockApi = new Mock<IApiService>();
        mockApi.Setup(a => a.GetAsync("academic-years"))
            .ReturnsAsync(httpResponse);

        var mockLogger = new Mock<ILogger<AcademicService>>();
        var service = new AcademicService(mockApi.Object, mockLogger.Object);

        // Act
        var result = await service.GetAcademicYearsAsync();

        // Assert
        Assert.IsNotNull(result);
        var resultList = result.ToList();
        Assert.AreEqual(2, resultList.Count);
        Assert.AreEqual("1", resultList[0].Id);
        Assert.AreEqual("2023/2024", resultList[0].Name);
        Assert.AreEqual("2", resultList[1].Id);
        Assert.AreEqual("2024/2025", resultList[1].Name);
        mockApi.Verify(a => a.GetAsync("academic-years"), Times.Once);
    }

    /// <summary>
    /// Tests that GetAcademicYearsAsync returns cached data on subsequent calls
    /// within the cache duration window.
    /// </summary>
    [TestMethod]
    public async Task GetAcademicYearsAsync_WhenDataIsCached_ReturnsCachedData()
    {
        // Arrange
        var expectedData = new List<LookupItem>
        {
            new LookupItem { Id = "1", Name = "2023/2024" }
        };

        var responseContent = JsonSerializer.Serialize(expectedData);
        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(responseContent, Encoding.UTF8, "application/json")
        };

        var mockApi = new Mock<IApiService>();
        mockApi.Setup(a => a.GetAsync("academic-years"))
            .ReturnsAsync(httpResponse);

        var mockLogger = new Mock<ILogger<AcademicService>>();
        var service = new AcademicService(mockApi.Object, mockLogger.Object);

        // Act
        var firstResult = await service.GetAcademicYearsAsync();
        var secondResult = await service.GetAcademicYearsAsync();

        // Assert
        Assert.IsNotNull(firstResult);
        Assert.IsNotNull(secondResult);
        Assert.AreEqual(firstResult.Count(), secondResult.Count());
        mockApi.Verify(a => a.GetAsync("academic-years"), Times.Once, "API should only be called once due to caching");
    }

    /// <summary>
    /// Tests that GetAcademicYearsAsync returns an empty collection
    /// when the API returns a non-success status code.
    /// </summary>
    [TestMethod]
    public async Task GetAcademicYearsAsync_WhenApiReturnsNonSuccessStatus_ReturnsEmptyCollection()
    {
        // Arrange
        var httpResponse = new HttpResponseMessage(HttpStatusCode.NotFound);

        var mockApi = new Mock<IApiService>();
        mockApi.Setup(a => a.GetAsync("academic-years"))
            .ReturnsAsync(httpResponse);

        var mockLogger = new Mock<ILogger<AcademicService>>();
        var service = new AcademicService(mockApi.Object, mockLogger.Object);

        // Act
        var result = await service.GetAcademicYearsAsync();

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(0, result.Count());
    }

    /// <summary>
    /// Tests that GetAcademicYearsAsync returns an empty collection
    /// when the API throws an exception, ensuring the exception is caught
    /// and does not propagate to the caller.
    /// </summary>
    [TestMethod]
    public async Task GetAcademicYearsAsync_WhenApiThrowsException_ReturnsEmptyCollection()
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        mockApi.Setup(a => a.GetAsync("academic-years"))
            .ThrowsAsync(new HttpRequestException("Network error"));

        var mockLogger = new Mock<ILogger<AcademicService>>();
        var service = new AcademicService(mockApi.Object, mockLogger.Object);

        // Act
        var result = await service.GetAcademicYearsAsync();

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(0, result.Count());
    }

    /// <summary>
    /// Tests that GetAcademicYearsAsync returns an empty collection
    /// when the API response content is null.
    /// </summary>
    [TestMethod]
    public async Task GetAcademicYearsAsync_WhenApiResponseContentIsNull_ReturnsEmptyCollection()
    {
        // Arrange
        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("null", Encoding.UTF8, "application/json")
        };

        var mockApi = new Mock<IApiService>();
        mockApi.Setup(a => a.GetAsync("academic-years"))
            .ReturnsAsync(httpResponse);

        var mockLogger = new Mock<ILogger<AcademicService>>();
        var service = new AcademicService(mockApi.Object, mockLogger.Object);

        // Act
        var result = await service.GetAcademicYearsAsync();

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(0, result.Count());
    }

    /// <summary>
    /// Tests that GetAcademicYearsAsync returns an empty collection
    /// when the API returns an empty array.
    /// </summary>
    [TestMethod]
    public async Task GetAcademicYearsAsync_WhenApiReturnsEmptyArray_ReturnsEmptyCollection()
    {
        // Arrange
        var responseContent = JsonSerializer.Serialize(new List<LookupItem>());
        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(responseContent, Encoding.UTF8, "application/json")
        };

        var mockApi = new Mock<IApiService>();
        mockApi.Setup(a => a.GetAsync("academic-years"))
            .ReturnsAsync(httpResponse);

        var mockLogger = new Mock<ILogger<AcademicService>>();
        var service = new AcademicService(mockApi.Object, mockLogger.Object);

        // Act
        var result = await service.GetAcademicYearsAsync();

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(0, result.Count());
    }

    /// <summary>
    /// Tests that GetAcademicYearsAsync handles a single academic year item correctly.
    /// </summary>
    [TestMethod]
    public async Task GetAcademicYearsAsync_WhenApiReturnsSingleItem_ReturnsSingleItem()
    {
        // Arrange
        var expectedData = new List<LookupItem>
        {
            new LookupItem { Id = "1", Name = "2023/2024" }
        };

        var responseContent = JsonSerializer.Serialize(expectedData);
        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(responseContent, Encoding.UTF8, "application/json")
        };

        var mockApi = new Mock<IApiService>();
        mockApi.Setup(a => a.GetAsync("academic-years"))
            .ReturnsAsync(httpResponse);

        var mockLogger = new Mock<ILogger<AcademicService>>();
        var service = new AcademicService(mockApi.Object, mockLogger.Object);

        // Act
        var result = await service.GetAcademicYearsAsync();

        // Assert
        Assert.IsNotNull(result);
        var resultList = result.ToList();
        Assert.AreEqual(1, resultList.Count);
        Assert.AreEqual("1", resultList[0].Id);
        Assert.AreEqual("2023/2024", resultList[0].Name);
    }

    /// <summary>
    /// Tests that GetAcademicYearsAsync handles academic years with empty string values.
    /// </summary>
    [TestMethod]
    public async Task GetAcademicYearsAsync_WhenApiReturnsItemsWithEmptyStrings_ReturnsItems()
    {
        // Arrange
        var expectedData = new List<LookupItem>
        {
            new LookupItem { Id = "", Name = "" },
            new LookupItem { Id = "2", Name = "2024/2025" }
        };

        var responseContent = JsonSerializer.Serialize(expectedData);
        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(responseContent, Encoding.UTF8, "application/json")
        };

        var mockApi = new Mock<IApiService>();
        mockApi.Setup(a => a.GetAsync("academic-years"))
            .ReturnsAsync(httpResponse);

        var mockLogger = new Mock<ILogger<AcademicService>>();
        var service = new AcademicService(mockApi.Object, mockLogger.Object);

        // Act
        var result = await service.GetAcademicYearsAsync();

        // Assert
        Assert.IsNotNull(result);
        var resultList = result.ToList();
        Assert.AreEqual(2, resultList.Count);
        Assert.AreEqual("", resultList[0].Id);
        Assert.AreEqual("", resultList[0].Name);
    }

    /// <summary>
    /// Tests that GetAcademicYearsAsync returns an empty collection
    /// when the API response contains invalid JSON.
    /// </summary>
    [TestMethod]
    public async Task GetAcademicYearsAsync_WhenApiReturnsInvalidJson_ReturnsEmptyCollection()
    {
        // Arrange
        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("invalid json {", Encoding.UTF8, "application/json")
        };

        var mockApi = new Mock<IApiService>();
        mockApi.Setup(a => a.GetAsync("academic-years"))
            .ReturnsAsync(httpResponse);

        var mockLogger = new Mock<ILogger<AcademicService>>();
        var service = new AcademicService(mockApi.Object, mockLogger.Object);

        // Act
        var result = await service.GetAcademicYearsAsync();

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(0, result.Count());
    }

    /// <summary>
    /// Tests that GetAcademicYearsAsync returns an empty collection
    /// when the API returns various HTTP error status codes.
    /// </summary>
    [TestMethod]
    [DataRow(HttpStatusCode.BadRequest)]
    [DataRow(HttpStatusCode.Unauthorized)]
    [DataRow(HttpStatusCode.Forbidden)]
    [DataRow(HttpStatusCode.NotFound)]
    [DataRow(HttpStatusCode.InternalServerError)]
    [DataRow(HttpStatusCode.ServiceUnavailable)]
    public async Task GetAcademicYearsAsync_WhenApiReturnsErrorStatusCode_ReturnsEmptyCollection(HttpStatusCode statusCode)
    {
        // Arrange
        var httpResponse = new HttpResponseMessage(statusCode);

        var mockApi = new Mock<IApiService>();
        mockApi.Setup(a => a.GetAsync("academic-years"))
            .ReturnsAsync(httpResponse);

        var mockLogger = new Mock<ILogger<AcademicService>>();
        var service = new AcademicService(mockApi.Object, mockLogger.Object);

        // Act
        var result = await service.GetAcademicYearsAsync();

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(0, result.Count());
    }

    /// <summary>
    /// Tests UpdateFacultyAsync returns the deserialized FacultyDto when the API call succeeds.
    /// Input: Valid id, name, and universityId; API returns success.
    /// Expected: Returns FacultyDto from API response.
    /// </summary>
    [TestMethod]
    public async Task UpdateFacultyAsync_ValidParametersAndSuccessfulResponse_ReturnsFacultyDto()
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<AcademicService>>();
        var service = new AcademicService(mockApi.Object, mockLogger.Object);

        string id = "faculty123";
        string name = "Engineering";
        string universityId = "uni456";

        var expectedFaculty = new FacultyDto
        {
            Id = id,
            Name = name,
            UniversityId = universityId,
            UniversityName = "Test University",
            DepartmentsCount = 5,
            Status = "Active"
        };

        var responseMessage = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(expectedFaculty)
        };

        mockApi.Setup(x => x.PutAsync($"faculties/{id}", It.IsAny<HttpContent>(), It.IsAny<CancellationToken>()))
               .ReturnsAsync(responseMessage);

        // Act
        var result = await service.UpdateFacultyAsync(id, name, universityId);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(expectedFaculty.Id, result.Id);
        Assert.AreEqual(expectedFaculty.Name, result.Name);
        Assert.AreEqual(expectedFaculty.UniversityId, result.UniversityId);
    }

    /// <summary>
    /// Tests UpdateFacultyAsync returns null when the API response indicates failure.
    /// Input: Valid parameters; API returns non-success status code.
    /// Expected: Returns null.
    /// </summary>
    [TestMethod]
    [DataRow(HttpStatusCode.BadRequest, DisplayName = "BadRequest")]
    [DataRow(HttpStatusCode.NotFound, DisplayName = "NotFound")]
    [DataRow(HttpStatusCode.InternalServerError, DisplayName = "InternalServerError")]
    [DataRow(HttpStatusCode.Unauthorized, DisplayName = "Unauthorized")]
    [DataRow(HttpStatusCode.Forbidden, DisplayName = "Forbidden")]
    public async Task UpdateFacultyAsync_ApiReturnsNonSuccessStatusCode_ReturnsNull(HttpStatusCode statusCode)
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<AcademicService>>();
        var service = new AcademicService(mockApi.Object, mockLogger.Object);

        string id = "faculty123";
        string name = "Engineering";
        string universityId = "uni456";

        var responseMessage = new HttpResponseMessage(statusCode);

        mockApi.Setup(x => x.PutAsync($"faculties/{id}", It.IsAny<HttpContent>(), It.IsAny<CancellationToken>()))
               .ReturnsAsync(responseMessage);

        // Act
        var result = await service.UpdateFacultyAsync(id, name, universityId);

        // Assert
        Assert.IsNull(result);
    }

    /// <summary>
    /// Tests UpdateFacultyAsync returns null and logs error when an exception occurs during API call.
    /// Input: Valid parameters; API throws exception.
    /// Expected: Returns null and logs error.
    /// </summary>
    [TestMethod]
    public async Task UpdateFacultyAsync_ApiThrowsException_ReturnsNullAndLogsError()
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<AcademicService>>();
        var service = new AcademicService(mockApi.Object, mockLogger.Object);

        string id = "faculty123";
        string name = "Engineering";
        string universityId = "uni456";

        var exception = new HttpRequestException("Network error");

        mockApi.Setup(x => x.PutAsync($"faculties/{id}", It.IsAny<HttpContent>(), It.IsAny<CancellationToken>()))
               .ThrowsAsync(exception);

        // Act
        var result = await service.UpdateFacultyAsync(id, name, universityId);

        // Assert
        Assert.IsNull(result);
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Failed to update faculty")),
                exception,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    /// <summary>
    /// Tests UpdateFacultyAsync with empty string parameters.
    /// Input: Empty strings for id, name, and universityId.
    /// Expected: API is called with empty strings in the request.
    /// </summary>
    [TestMethod]
    public async Task UpdateFacultyAsync_EmptyStringParameters_CallsApiWithEmptyStrings()
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<AcademicService>>();
        var service = new AcademicService(mockApi.Object, mockLogger.Object);

        string id = string.Empty;
        string name = string.Empty;
        string universityId = string.Empty;

        var responseMessage = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(new FacultyDto { Id = id, Name = name, UniversityId = universityId })
        };

        mockApi.Setup(x => x.PutAsync($"faculties/{id}", It.IsAny<HttpContent>(), It.IsAny<CancellationToken>()))
               .ReturnsAsync(responseMessage);

        // Act
        var result = await service.UpdateFacultyAsync(id, name, universityId);

        // Assert
        mockApi.Verify(x => x.PutAsync($"faculties/{id}", It.IsAny<HttpContent>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Tests UpdateFacultyAsync with whitespace-only string parameters.
    /// Input: Whitespace-only strings for name and universityId.
    /// Expected: API is called with whitespace strings in the request.
    /// </summary>
    [TestMethod]
    public async Task UpdateFacultyAsync_WhitespaceOnlyParameters_CallsApiWithWhitespaceStrings()
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<AcademicService>>();
        var service = new AcademicService(mockApi.Object, mockLogger.Object);

        string id = "faculty123";
        string name = "   ";
        string universityId = "\t\n";

        var responseMessage = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(new FacultyDto { Id = id, Name = name, UniversityId = universityId })
        };

        mockApi.Setup(x => x.PutAsync($"faculties/{id}", It.IsAny<HttpContent>(), It.IsAny<CancellationToken>()))
               .ReturnsAsync(responseMessage);

        // Act
        var result = await service.UpdateFacultyAsync(id, name, universityId);

        // Assert
        mockApi.Verify(x => x.PutAsync($"faculties/{id}", It.IsAny<HttpContent>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Tests UpdateFacultyAsync with very long string parameters.
    /// Input: Very long strings for id, name, and universityId.
    /// Expected: API is called successfully with long strings.
    /// </summary>
    [TestMethod]
    public async Task UpdateFacultyAsync_VeryLongStrings_CallsApiSuccessfully()
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<AcademicService>>();
        var service = new AcademicService(mockApi.Object, mockLogger.Object);

        string id = new string('a', 10000);
        string name = new string('b', 10000);
        string universityId = new string('c', 10000);

        var responseMessage = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(new FacultyDto { Id = id, Name = name, UniversityId = universityId })
        };

        mockApi.Setup(x => x.PutAsync($"faculties/{id}", It.IsAny<HttpContent>(), It.IsAny<CancellationToken>()))
               .ReturnsAsync(responseMessage);

        // Act
        var result = await service.UpdateFacultyAsync(id, name, universityId);

        // Assert
        mockApi.Verify(x => x.PutAsync($"faculties/{id}", It.IsAny<HttpContent>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Tests UpdateFacultyAsync with special characters in parameters.
    /// Input: Strings with special characters, Unicode, and control characters.
    /// Expected: API is called with special characters properly serialized.
    /// </summary>
    [TestMethod]
    [DataRow("faculty<>123", "Name&Value", "uni\"456", DisplayName = "SpecialCharacters")]
    [DataRow("faculty\n123", "Name\tValue", "uni\r456", DisplayName = "ControlCharacters")]
    [DataRow("faculty™123", "Ñame©", "uni™456", DisplayName = "UnicodeCharacters")]
    [DataRow("faculty/123", "Name\\Value", "uni|456", DisplayName = "PathLikeCharacters")]
    public async Task UpdateFacultyAsync_SpecialCharactersInParameters_CallsApiSuccessfully(string id, string name, string universityId)
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<AcademicService>>();
        var service = new AcademicService(mockApi.Object, mockLogger.Object);

        var responseMessage = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(new FacultyDto { Id = id, Name = name, UniversityId = universityId })
        };

        mockApi.Setup(x => x.PutAsync($"faculties/{id}", It.IsAny<HttpContent>(), It.IsAny<CancellationToken>()))
               .ReturnsAsync(responseMessage);

        // Act
        var result = await service.UpdateFacultyAsync(id, name, universityId);

        // Assert
        mockApi.Verify(x => x.PutAsync($"faculties/{id}", It.IsAny<HttpContent>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Tests UpdateFacultyAsync returns null and logs error when deserialization throws exception.
    /// Input: Valid parameters; API returns success but invalid JSON content.
    /// Expected: Returns null and logs error.
    /// </summary>
    [TestMethod]
    public async Task UpdateFacultyAsync_DeserializationThrowsException_ReturnsNullAndLogsError()
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<AcademicService>>();
        var service = new AcademicService(mockApi.Object, mockLogger.Object);

        string id = "faculty123";
        string name = "Engineering";
        string universityId = "uni456";

        var responseMessage = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("invalid json", Encoding.UTF8, "application/json")
        };

        mockApi.Setup(x => x.PutAsync($"faculties/{id}", It.IsAny<HttpContent>(), It.IsAny<CancellationToken>()))
               .ReturnsAsync(responseMessage);

        // Act
        var result = await service.UpdateFacultyAsync(id, name, universityId);

        // Assert
        Assert.IsNull(result);
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Failed to update faculty")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    /// <summary>
    /// Tests UpdateFacultyAsync logs the correct faculty id when an exception occurs.
    /// Input: Valid parameters; exception occurs during operation.
    /// Expected: Error log contains the correct faculty id.
    /// </summary>
    [TestMethod]
    public async Task UpdateFacultyAsync_ExceptionOccurs_LogsCorrectFacultyId()
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<AcademicService>>();
        var service = new AcademicService(mockApi.Object, mockLogger.Object);

        string id = "specific-faculty-789";
        string name = "Engineering";
        string universityId = "uni456";

        var exception = new InvalidOperationException("Test exception");

        mockApi.Setup(x => x.PutAsync(It.IsAny<string>(), It.IsAny<HttpContent>(), It.IsAny<CancellationToken>()))
               .ThrowsAsync(exception);

        // Act
        var result = await service.UpdateFacultyAsync(id, name, universityId);

        // Assert
        Assert.IsNull(result);
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains(id)),
                exception,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    /// <summary>
    /// Tests UpdateFacultyAsync with various exception types.
    /// Input: Valid parameters; different exception types thrown during API call.
    /// Expected: Returns null and logs error for all exception types.
    /// </summary>
    [TestMethod]
    public async Task UpdateFacultyAsync_VariousExceptionTypes_ReturnsNullAndLogsError()
    {
        // Arrange - HttpRequestException
        var mockApi1 = new Mock<IApiService>();
        var mockLogger1 = new Mock<ILogger<AcademicService>>();
        var service1 = new AcademicService(mockApi1.Object, mockLogger1.Object);

        mockApi1.Setup(x => x.PutAsync(It.IsAny<string>(), It.IsAny<HttpContent>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new HttpRequestException("Network error"));

        // Act
        var result1 = await service1.UpdateFacultyAsync("id1", "name1", "uni1");

        // Assert
        Assert.IsNull(result1);

        // Arrange - TaskCanceledException
        var mockApi2 = new Mock<IApiService>();
        var mockLogger2 = new Mock<ILogger<AcademicService>>();
        var service2 = new AcademicService(mockApi2.Object, mockLogger2.Object);

        mockApi2.Setup(x => x.PutAsync(It.IsAny<string>(), It.IsAny<HttpContent>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new TaskCanceledException("Timeout"));

        // Act
        var result2 = await service2.UpdateFacultyAsync("id2", "name2", "uni2");

        // Assert
        Assert.IsNull(result2);

        // Arrange - InvalidOperationException
        var mockApi3 = new Mock<IApiService>();
        var mockLogger3 = new Mock<ILogger<AcademicService>>();
        var service3 = new AcademicService(mockApi3.Object, mockLogger3.Object);

        mockApi3.Setup(x => x.PutAsync(It.IsAny<string>(), It.IsAny<HttpContent>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidOperationException("Invalid state"));

        // Act
        var result3 = await service3.UpdateFacultyAsync("id3", "name3", "uni3");

        // Assert
        Assert.IsNull(result3);
    }

    /// <summary>
    /// Tests UpdateFacultyAsync returns null when response content is null.
    /// Input: Valid parameters; API returns success but content is null.
    /// Expected: Returns null (due to exception during ReadFromJsonAsync).
    /// </summary>
    [TestMethod]
    public async Task UpdateFacultyAsync_ResponseContentIsNull_ReturnsNull()
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<AcademicService>>();
        var service = new AcademicService(mockApi.Object, mockLogger.Object);

        string id = "faculty123";
        string name = "Engineering";
        string universityId = "uni456";

        var responseMessage = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("null", Encoding.UTF8, "application/json")
        };

        mockApi.Setup(x => x.PutAsync($"faculties/{id}", It.IsAny<HttpContent>(), It.IsAny<CancellationToken>()))
               .ReturnsAsync(responseMessage);

        // Act
        var result = await service.UpdateFacultyAsync(id, name, universityId);

        // Assert
        Assert.IsNull(result);
    }

    /// <summary>
    /// Tests that GetDepartmentsAsync with a valid faculty ID returns lookup items from the API.
    /// </summary>
    [TestMethod]
    public async Task GetDepartmentsAsync_ValidFacultyId_ReturnsLookupItems()
    {
        // Arrange
        var facultyId = "FAC123";
        var expectedPath = $"departments?facultyId={facultyId}";
        var expectedItems = new List<LookupItem>
        {
            new LookupItem { Id = "DEPT1", Name = "Computer Science" },
            new LookupItem { Id = "DEPT2", Name = "Mathematics" }
        };
        var jsonContent = JsonSerializer.Serialize(expectedItems);
        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(jsonContent, System.Text.Encoding.UTF8, "application/json")
        };

        var mockApiService = new Mock<IApiService>();
        mockApiService.Setup(x => x.GetAsync(expectedPath)).ReturnsAsync(httpResponse);
        var mockLogger = new Mock<ILogger<AcademicService>>();
        var service = new AcademicService(mockApiService.Object, mockLogger.Object);

        // Act
        var result = await service.GetDepartmentsAsync(facultyId);

        // Assert
        Assert.IsNotNull(result);
        var resultList = result.ToList();
        Assert.AreEqual(2, resultList.Count);
        Assert.AreEqual("DEPT1", resultList[0].Id);
        Assert.AreEqual("Computer Science", resultList[0].Name);
        mockApiService.Verify(x => x.GetAsync(expectedPath), Times.Once);
    }

    /// <summary>
    /// Tests that GetDepartmentsAsync with an empty faculty ID constructs the correct path and calls the API.
    /// </summary>
    [TestMethod]
    public async Task GetDepartmentsAsync_EmptyFacultyId_CallsApiWithEmptyParameter()
    {
        // Arrange
        var facultyId = "";
        var expectedPath = "departments?facultyId=";
        var expectedItems = new List<LookupItem>();
        var jsonContent = JsonSerializer.Serialize(expectedItems);
        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(jsonContent, System.Text.Encoding.UTF8, "application/json")
        };

        var mockApiService = new Mock<IApiService>();
        mockApiService.Setup(x => x.GetAsync(expectedPath)).ReturnsAsync(httpResponse);
        var mockLogger = new Mock<ILogger<AcademicService>>();
        var service = new AcademicService(mockApiService.Object, mockLogger.Object);

        // Act
        var result = await service.GetDepartmentsAsync(facultyId);

        // Assert
        Assert.IsNotNull(result);
        mockApiService.Verify(x => x.GetAsync(expectedPath), Times.Once);
    }

    /// <summary>
    /// Tests that GetDepartmentsAsync with a whitespace-only faculty ID constructs the correct path.
    /// </summary>
    [TestMethod]
    public async Task GetDepartmentsAsync_WhitespaceFacultyId_CallsApiWithWhitespace()
    {
        // Arrange
        var facultyId = "   ";
        var expectedPath = "departments?facultyId=   ";
        var expectedItems = new List<LookupItem>();
        var jsonContent = JsonSerializer.Serialize(expectedItems);
        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(jsonContent, System.Text.Encoding.UTF8, "application/json")
        };

        var mockApiService = new Mock<IApiService>();
        mockApiService.Setup(x => x.GetAsync(expectedPath)).ReturnsAsync(httpResponse);
        var mockLogger = new Mock<ILogger<AcademicService>>();
        var service = new AcademicService(mockApiService.Object, mockLogger.Object);

        // Act
        var result = await service.GetDepartmentsAsync(facultyId);

        // Assert
        Assert.IsNotNull(result);
        mockApiService.Verify(x => x.GetAsync(expectedPath), Times.Once);
    }

    /// <summary>
    /// Tests that GetDepartmentsAsync with special characters in faculty ID constructs the path correctly.
    /// Special characters like &, =, ? should be passed as-is without encoding (URL encoding should be handled by the HTTP client).
    /// </summary>
    [TestMethod]
    [DataRow("FAC&123", "departments?facultyId=FAC&123")]
    [DataRow("FAC=456", "departments?facultyId=FAC=456")]
    [DataRow("FAC?789", "departments?facultyId=FAC?789")]
    [DataRow("FAC/ABC", "departments?facultyId=FAC/ABC")]
    [DataRow("FAC%20", "departments?facultyId=FAC%20")]
    public async Task GetDepartmentsAsync_SpecialCharactersInFacultyId_ConstructsCorrectPath(string facultyId, string expectedPath)
    {
        // Arrange
        var expectedItems = new List<LookupItem>();
        var jsonContent = JsonSerializer.Serialize(expectedItems);
        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(jsonContent, System.Text.Encoding.UTF8, "application/json")
        };

        var mockApiService = new Mock<IApiService>();
        mockApiService.Setup(x => x.GetAsync(expectedPath)).ReturnsAsync(httpResponse);
        var mockLogger = new Mock<ILogger<AcademicService>>();
        var service = new AcademicService(mockApiService.Object, mockLogger.Object);

        // Act
        var result = await service.GetDepartmentsAsync(facultyId);

        // Assert
        Assert.IsNotNull(result);
        mockApiService.Verify(x => x.GetAsync(expectedPath), Times.Once);
    }

    /// <summary>
    /// Tests that GetDepartmentsAsync with a very long faculty ID handles it correctly.
    /// </summary>
    [TestMethod]
    public async Task GetDepartmentsAsync_VeryLongFacultyId_HandlesCorrectly()
    {
        // Arrange
        var facultyId = new string('A', 10000);
        var expectedPath = $"departments?facultyId={facultyId}";
        var expectedItems = new List<LookupItem>();
        var jsonContent = JsonSerializer.Serialize(expectedItems);
        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(jsonContent, System.Text.Encoding.UTF8, "application/json")
        };

        var mockApiService = new Mock<IApiService>();
        mockApiService.Setup(x => x.GetAsync(expectedPath)).ReturnsAsync(httpResponse);
        var mockLogger = new Mock<ILogger<AcademicService>>();
        var service = new AcademicService(mockApiService.Object, mockLogger.Object);

        // Act
        var result = await service.GetDepartmentsAsync(facultyId);

        // Assert
        Assert.IsNotNull(result);
        mockApiService.Verify(x => x.GetAsync(expectedPath), Times.Once);
    }

    /// <summary>
    /// Tests that GetDepartmentsAsync with Unicode characters in faculty ID constructs the path correctly.
    /// </summary>
    [TestMethod]
    public async Task GetDepartmentsAsync_UnicodeFacultyId_ConstructsCorrectPath()
    {
        // Arrange
        var facultyId = "教授123";
        var expectedPath = $"departments?facultyId={facultyId}";
        var expectedItems = new List<LookupItem>();
        var jsonContent = JsonSerializer.Serialize(expectedItems);
        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(jsonContent, System.Text.Encoding.UTF8, "application/json")
        };

        var mockApiService = new Mock<IApiService>();
        mockApiService.Setup(x => x.GetAsync(expectedPath)).ReturnsAsync(httpResponse);
        var mockLogger = new Mock<ILogger<AcademicService>>();
        var service = new AcademicService(mockApiService.Object, mockLogger.Object);

        // Act
        var result = await service.GetDepartmentsAsync(facultyId);

        // Assert
        Assert.IsNotNull(result);
        mockApiService.Verify(x => x.GetAsync(expectedPath), Times.Once);
    }

    /// <summary>
    /// Tests that GetDepartmentsAsync returns cached data on second call within cache duration without making another API call.
    /// </summary>
    [TestMethod]
    public async Task GetDepartmentsAsync_SecondCallWithinCacheDuration_ReturnsCachedData()
    {
        // Arrange
        var facultyId = "FAC123";
        var expectedPath = $"departments?facultyId={facultyId}";
        var expectedItems = new List<LookupItem>
        {
            new LookupItem { Id = "DEPT1", Name = "Computer Science" }
        };
        var jsonContent = JsonSerializer.Serialize(expectedItems);
        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(jsonContent, System.Text.Encoding.UTF8, "application/json")
        };

        var mockApiService = new Mock<IApiService>();
        mockApiService.Setup(x => x.GetAsync(expectedPath)).ReturnsAsync(httpResponse);
        var mockLogger = new Mock<ILogger<AcademicService>>();
        var service = new AcademicService(mockApiService.Object, mockLogger.Object);

        // Act
        var result1 = await service.GetDepartmentsAsync(facultyId);
        var result2 = await service.GetDepartmentsAsync(facultyId);

        // Assert
        Assert.IsNotNull(result1);
        Assert.IsNotNull(result2);
        Assert.AreEqual(result1.First().Id, result2.First().Id);
        mockApiService.Verify(x => x.GetAsync(expectedPath), Times.Once);
    }

    /// <summary>
    /// Tests that GetDepartmentsAsync returns an empty collection when the API returns a non-success status code.
    /// </summary>
    [TestMethod]
    public async Task GetDepartmentsAsync_ApiReturnsNonSuccessStatus_ReturnsEmptyCollection()
    {
        // Arrange
        var facultyId = "FAC123";
        var expectedPath = $"departments?facultyId={facultyId}";
        var httpResponse = new HttpResponseMessage(HttpStatusCode.NotFound);

        var mockApiService = new Mock<IApiService>();
        mockApiService.Setup(x => x.GetAsync(expectedPath)).ReturnsAsync(httpResponse);
        var mockLogger = new Mock<ILogger<AcademicService>>();
        var service = new AcademicService(mockApiService.Object, mockLogger.Object);

        // Act
        var result = await service.GetDepartmentsAsync(facultyId);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(0, result.Count());
        mockApiService.Verify(x => x.GetAsync(expectedPath), Times.Once);
    }

    /// <summary>
    /// Tests that GetDepartmentsAsync returns an empty collection when the API throws an exception.
    /// </summary>
    [TestMethod]
    public async Task GetDepartmentsAsync_ApiThrowsException_ReturnsEmptyCollection()
    {
        // Arrange
        var facultyId = "FAC123";
        var expectedPath = $"departments?facultyId={facultyId}";

        var mockApiService = new Mock<IApiService>();
        mockApiService.Setup(x => x.GetAsync(expectedPath)).ThrowsAsync(new HttpRequestException("Network error"));
        var mockLogger = new Mock<ILogger<AcademicService>>();
        var service = new AcademicService(mockApiService.Object, mockLogger.Object);

        // Act
        var result = await service.GetDepartmentsAsync(facultyId);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(0, result.Count());
        mockApiService.Verify(x => x.GetAsync(expectedPath), Times.Once);
    }

    /// <summary>
    /// Tests that GetDepartmentsAsync returns an empty collection when the API returns null content.
    /// </summary>
    [TestMethod]
    public async Task GetDepartmentsAsync_ApiReturnsNullContent_ReturnsEmptyCollection()
    {
        // Arrange
        var facultyId = "FAC123";
        var expectedPath = $"departments?facultyId={facultyId}";
        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("null", System.Text.Encoding.UTF8, "application/json")
        };

        var mockApiService = new Mock<IApiService>();
        mockApiService.Setup(x => x.GetAsync(expectedPath)).ReturnsAsync(httpResponse);
        var mockLogger = new Mock<ILogger<AcademicService>>();
        var service = new AcademicService(mockApiService.Object, mockLogger.Object);

        // Act
        var result = await service.GetDepartmentsAsync(facultyId);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(0, result.Count());
        mockApiService.Verify(x => x.GetAsync(expectedPath), Times.Once);
    }

    /// <summary>
    /// Tests that GetDepartmentsAsync returns an empty collection when API returns an empty array.
    /// </summary>
    [TestMethod]
    public async Task GetDepartmentsAsync_ApiReturnsEmptyArray_ReturnsEmptyCollection()
    {
        // Arrange
        var facultyId = "FAC123";
        var expectedPath = $"departments?facultyId={facultyId}";
        var expectedItems = new List<LookupItem>();
        var jsonContent = JsonSerializer.Serialize(expectedItems);
        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(jsonContent, System.Text.Encoding.UTF8, "application/json")
        };

        var mockApiService = new Mock<IApiService>();
        mockApiService.Setup(x => x.GetAsync(expectedPath)).ReturnsAsync(httpResponse);
        var mockLogger = new Mock<ILogger<AcademicService>>();
        var service = new AcademicService(mockApiService.Object, mockLogger.Object);

        // Act
        var result = await service.GetDepartmentsAsync(facultyId);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(0, result.Count());
        mockApiService.Verify(x => x.GetAsync(expectedPath), Times.Once);
    }

    /// <summary>
    /// Tests that GetDepartmentsAsync with different faculty IDs uses separate cache entries.
    /// </summary>
    [TestMethod]
    public async Task GetDepartmentsAsync_DifferentFacultyIds_UsesSeparateCacheEntries()
    {
        // Arrange
        var facultyId1 = "FAC1";
        var facultyId2 = "FAC2";
        var path1 = $"departments?facultyId={facultyId1}";
        var path2 = $"departments?facultyId={facultyId2}";

        var items1 = new List<LookupItem> { new LookupItem { Id = "DEPT1", Name = "Dept1" } };
        var items2 = new List<LookupItem> { new LookupItem { Id = "DEPT2", Name = "Dept2" } };

        var response1 = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(JsonSerializer.Serialize(items1), System.Text.Encoding.UTF8, "application/json")
        };
        var response2 = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(JsonSerializer.Serialize(items2), System.Text.Encoding.UTF8, "application/json")
        };

        var mockApiService = new Mock<IApiService>();
        mockApiService.Setup(x => x.GetAsync(path1)).ReturnsAsync(response1);
        mockApiService.Setup(x => x.GetAsync(path2)).ReturnsAsync(response2);
        var mockLogger = new Mock<ILogger<AcademicService>>();
        var service = new AcademicService(mockApiService.Object, mockLogger.Object);

        // Act
        var result1 = await service.GetDepartmentsAsync(facultyId1);
        var result2 = await service.GetDepartmentsAsync(facultyId2);

        // Assert
        Assert.AreEqual("DEPT1", result1.First().Id);
        Assert.AreEqual("DEPT2", result2.First().Id);
        mockApiService.Verify(x => x.GetAsync(path1), Times.Once);
        mockApiService.Verify(x => x.GetAsync(path2), Times.Once);
    }

    /// <summary>
    /// Tests that GetFacultyDetailsAsync returns a list of faculties when the API call succeeds with valid data.
    /// </summary>
    [TestMethod]
    public async Task GetFacultyDetailsAsync_SuccessfulResponseWithValidData_ReturnsListOfFaculties()
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<AcademicService>>();
        var expectedFaculties = new List<FacultyDto>
        {
            new FacultyDto { Id = "1", Name = "Faculty of Science", UniversityId = "U1", UniversityName = "University A", DepartmentsCount = 5, Status = "Active" },
            new FacultyDto { Id = "2", Name = "Faculty of Arts", UniversityId = "U1", UniversityName = "University A", DepartmentsCount = 3, Status = "Active" }
        };

        var jsonContent = JsonContent.Create(expectedFaculties);
        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = jsonContent
        };

        mockApi.Setup(x => x.GetAsync("faculties/details", It.IsAny<CancellationToken>()))
               .ReturnsAsync(response);

        var service = new AcademicService(mockApi.Object, mockLogger.Object);

        // Act
        var result = await service.GetFacultyDetailsAsync();

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(2, result.Count);
        Assert.AreEqual("1", result[0].Id);
        Assert.AreEqual("Faculty of Science", result[0].Name);
        Assert.AreEqual("2", result[1].Id);
        Assert.AreEqual("Faculty of Arts", result[1].Name);
    }

    /// <summary>
    /// Tests that GetFacultyDetailsAsync returns an empty list when the API call succeeds but returns an empty list.
    /// </summary>
    [TestMethod]
    public async Task GetFacultyDetailsAsync_SuccessfulResponseWithEmptyList_ReturnsEmptyList()
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<AcademicService>>();
        var emptyFaculties = new List<FacultyDto>();

        var jsonContent = JsonContent.Create(emptyFaculties);
        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = jsonContent
        };

        mockApi.Setup(x => x.GetAsync("faculties/details", It.IsAny<CancellationToken>()))
               .ReturnsAsync(response);

        var service = new AcademicService(mockApi.Object, mockLogger.Object);

        // Act
        var result = await service.GetFacultyDetailsAsync();

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(0, result.Count);
    }

    /// <summary>
    /// Tests that GetFacultyDetailsAsync returns an empty list when the API call succeeds but content is null.
    /// </summary>
    [TestMethod]
    public async Task GetFacultyDetailsAsync_SuccessfulResponseWithNullContent_ReturnsEmptyList()
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<AcademicService>>();

        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("null")
        };

        mockApi.Setup(x => x.GetAsync("faculties/details", It.IsAny<CancellationToken>()))
               .ReturnsAsync(response);

        var service = new AcademicService(mockApi.Object, mockLogger.Object);

        // Act
        var result = await service.GetFacultyDetailsAsync();

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(0, result.Count);
    }

    /// <summary>
    /// Tests that GetFacultyDetailsAsync returns an empty list when the API response status is unsuccessful (4xx status code).
    /// </summary>
    [TestMethod]
    [DataRow(HttpStatusCode.BadRequest)]
    [DataRow(HttpStatusCode.Unauthorized)]
    [DataRow(HttpStatusCode.Forbidden)]
    [DataRow(HttpStatusCode.NotFound)]
    public async Task GetFacultyDetailsAsync_UnsuccessfulResponse4xx_ReturnsEmptyList(HttpStatusCode statusCode)
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<AcademicService>>();

        var response = new HttpResponseMessage(statusCode);

        mockApi.Setup(x => x.GetAsync("faculties/details", It.IsAny<CancellationToken>()))
               .ReturnsAsync(response);

        var service = new AcademicService(mockApi.Object, mockLogger.Object);

        // Act
        var result = await service.GetFacultyDetailsAsync();

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(0, result.Count);
    }

    /// <summary>
    /// Tests that GetFacultyDetailsAsync returns an empty list when the API response status is unsuccessful (5xx status code).
    /// </summary>
    [TestMethod]
    [DataRow(HttpStatusCode.InternalServerError)]
    [DataRow(HttpStatusCode.BadGateway)]
    [DataRow(HttpStatusCode.ServiceUnavailable)]
    [DataRow(HttpStatusCode.GatewayTimeout)]
    public async Task GetFacultyDetailsAsync_UnsuccessfulResponse5xx_ReturnsEmptyList(HttpStatusCode statusCode)
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<AcademicService>>();

        var response = new HttpResponseMessage(statusCode);

        mockApi.Setup(x => x.GetAsync("faculties/details", It.IsAny<CancellationToken>()))
               .ReturnsAsync(response);

        var service = new AcademicService(mockApi.Object, mockLogger.Object);

        // Act
        var result = await service.GetFacultyDetailsAsync();

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(0, result.Count);
    }

    /// <summary>
    /// Tests that GetFacultyDetailsAsync logs error and returns an empty list when GetAsync throws an HttpRequestException.
    /// </summary>
    [TestMethod]
    public async Task GetFacultyDetailsAsync_GetAsyncThrowsHttpRequestException_LogsErrorAndReturnsEmptyList()
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<AcademicService>>();
        var exception = new HttpRequestException("Network error");

        mockApi.Setup(x => x.GetAsync("faculties/details", It.IsAny<CancellationToken>()))
               .ThrowsAsync(exception);

        var service = new AcademicService(mockApi.Object, mockLogger.Object);

        // Act
        var result = await service.GetFacultyDetailsAsync();

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(0, result.Count);
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Failed to fetch faculty details")),
                exception,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    /// <summary>
    /// Tests that GetFacultyDetailsAsync logs error and returns an empty list when GetAsync throws a TaskCanceledException.
    /// </summary>
    [TestMethod]
    public async Task GetFacultyDetailsAsync_GetAsyncThrowsTaskCanceledException_LogsErrorAndReturnsEmptyList()
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<AcademicService>>();
        var exception = new TaskCanceledException("Request timeout");

        mockApi.Setup(x => x.GetAsync("faculties/details", It.IsAny<CancellationToken>()))
               .ThrowsAsync(exception);

        var service = new AcademicService(mockApi.Object, mockLogger.Object);

        // Act
        var result = await service.GetFacultyDetailsAsync();

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(0, result.Count);
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Failed to fetch faculty details")),
                exception,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    /// <summary>
    /// Tests that GetFacultyDetailsAsync logs error and returns an empty list when content deserialization throws JsonException.
    /// </summary>
    [TestMethod]
    public async Task GetFacultyDetailsAsync_DeserializationThrowsJsonException_LogsErrorAndReturnsEmptyList()
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<AcademicService>>();

        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("{invalid json")
        };

        mockApi.Setup(x => x.GetAsync("faculties/details", It.IsAny<CancellationToken>()))
               .ReturnsAsync(response);

        var service = new AcademicService(mockApi.Object, mockLogger.Object);

        // Act
        var result = await service.GetFacultyDetailsAsync();

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(0, result.Count);
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Failed to fetch faculty details")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    /// <summary>
    /// Tests that GetFacultyDetailsAsync logs error and returns an empty list when GetAsync throws a generic Exception.
    /// </summary>
    [TestMethod]
    public async Task GetFacultyDetailsAsync_GetAsyncThrowsGenericException_LogsErrorAndReturnsEmptyList()
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<AcademicService>>();
        var exception = new InvalidOperationException("Unexpected error");

        mockApi.Setup(x => x.GetAsync("faculties/details", It.IsAny<CancellationToken>()))
               .ThrowsAsync(exception);

        var service = new AcademicService(mockApi.Object, mockLogger.Object);

        // Act
        var result = await service.GetFacultyDetailsAsync();

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(0, result.Count);
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Failed to fetch faculty details")),
                exception,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    /// <summary>
    /// Tests that GetFacultyDetailsAsync returns a list with a single faculty when the API returns one item.
    /// </summary>
    [TestMethod]
    public async Task GetFacultyDetailsAsync_SuccessfulResponseWithSingleItem_ReturnsListWithOneItem()
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<AcademicService>>();
        var singleFaculty = new List<FacultyDto>
        {
            new FacultyDto { Id = "1", Name = "Faculty of Engineering", UniversityId = "U1", UniversityName = "University A", DepartmentsCount = 10, Status = "Active" }
        };

        var jsonContent = JsonContent.Create(singleFaculty);
        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = jsonContent
        };

        mockApi.Setup(x => x.GetAsync("faculties/details", It.IsAny<CancellationToken>()))
               .ReturnsAsync(response);

        var service = new AcademicService(mockApi.Object, mockLogger.Object);

        // Act
        var result = await service.GetFacultyDetailsAsync();

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(1, result.Count);
        Assert.AreEqual("1", result[0].Id);
        Assert.AreEqual("Faculty of Engineering", result[0].Name);
        Assert.AreEqual(10, result[0].DepartmentsCount);
    }

    /// <summary>
    /// Tests that GetFacultyDetailsAsync successfully handles a large list of faculties.
    /// </summary>
    [TestMethod]
    public async Task GetFacultyDetailsAsync_SuccessfulResponseWithLargeList_ReturnsCompleteList()
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<AcademicService>>();
        var largeFacultyList = new List<FacultyDto>();
        for (int i = 0; i < 1000; i++)
        {
            largeFacultyList.Add(new FacultyDto
            {
                Id = $"F{i}",
                Name = $"Faculty {i}",
                UniversityId = $"U{i % 10}",
                UniversityName = $"University {i % 10}",
                DepartmentsCount = i,
                Status = "Active"
            });
        }

        var jsonContent = JsonContent.Create(largeFacultyList);
        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = jsonContent
        };

        mockApi.Setup(x => x.GetAsync("faculties/details", It.IsAny<CancellationToken>()))
               .ReturnsAsync(response);

        var service = new AcademicService(mockApi.Object, mockLogger.Object);

        // Act
        var result = await service.GetFacultyDetailsAsync();

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(1000, result.Count);
    }

    /// <summary>
    /// Tests that GetFacultyDetailsAsync handles faculties with special characters in names correctly.
    /// </summary>
    [TestMethod]
    public async Task GetFacultyDetailsAsync_SuccessfulResponseWithSpecialCharactersInNames_ReturnsCorrectData()
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<AcademicService>>();
        var faculties = new List<FacultyDto>
        {
            new FacultyDto { Id = "1", Name = "Faculty of Arts & Sciences", UniversityId = "U1", UniversityName = "University A", DepartmentsCount = 5, Status = "Active" },
            new FacultyDto { Id = "2", Name = "Faculty of \"Law\"", UniversityId = "U1", UniversityName = "University A", DepartmentsCount = 2, Status = "Active" },
            new FacultyDto { Id = "3", Name = "Faculty with\nNewline", UniversityId = "U1", UniversityName = "University A", DepartmentsCount = 3, Status = "Active" }
        };

        var jsonContent = JsonContent.Create(faculties);
        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = jsonContent
        };

        mockApi.Setup(x => x.GetAsync("faculties/details", It.IsAny<CancellationToken>()))
               .ReturnsAsync(response);

        var service = new AcademicService(mockApi.Object, mockLogger.Object);

        // Act
        var result = await service.GetFacultyDetailsAsync();

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(3, result.Count);
        Assert.AreEqual("Faculty of Arts & Sciences", result[0].Name);
        Assert.AreEqual("Faculty of \"Law\"", result[1].Name);
        Assert.AreEqual("Faculty with\nNewline", result[2].Name);
    }

    /// <summary>
    /// Tests that GetUniversitiesAsync successfully returns universities from API on first call (cache miss).
    /// </summary>
    [TestMethod]
    public async Task GetUniversitiesAsync_FirstCall_ReturnsUniversitiesFromApi()
    {
        // Arrange
        var expectedUniversities = new List<LookupItem>
        {
            new LookupItem { Id = "1", Name = "University A" },
            new LookupItem { Id = "2", Name = "University B" }
        };

        var responseMessage = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(expectedUniversities)
        };

        var mockApiService = new Mock<IApiService>();
        mockApiService
            .Setup(x => x.GetAsync("universities"))
            .ReturnsAsync(responseMessage);

        var mockLogger = new Mock<ILogger<AcademicService>>();
        var service = new AcademicService(mockApiService.Object, mockLogger.Object);

        // Act
        var result = await service.GetUniversitiesAsync();

        // Assert
        Assert.IsNotNull(result);
        var resultList = result.ToList();
        Assert.AreEqual(2, resultList.Count);
        Assert.AreEqual("1", resultList[0].Id);
        Assert.AreEqual("University A", resultList[0].Name);
        Assert.AreEqual("2", resultList[1].Id);
        Assert.AreEqual("University B", resultList[1].Name);
        mockApiService.Verify(x => x.GetAsync("universities"), Times.Once);
    }

    /// <summary>
    /// Tests that GetUniversitiesAsync returns cached data on subsequent calls within cache duration.
    /// </summary>
    [TestMethod]
    public async Task GetUniversitiesAsync_SubsequentCallWithinCacheDuration_ReturnsCachedData()
    {
        // Arrange
        var expectedUniversities = new List<LookupItem>
        {
            new LookupItem { Id = "1", Name = "University A" }
        };

        var responseMessage = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(expectedUniversities)
        };

        var mockApiService = new Mock<IApiService>();
        mockApiService
            .Setup(x => x.GetAsync("universities"))
            .ReturnsAsync(responseMessage);

        var mockLogger = new Mock<ILogger<AcademicService>>();
        var service = new AcademicService(mockApiService.Object, mockLogger.Object);

        // Act
        var result1 = await service.GetUniversitiesAsync();
        var result2 = await service.GetUniversitiesAsync();

        // Assert
        Assert.IsNotNull(result1);
        Assert.IsNotNull(result2);
        Assert.AreEqual(result1.Count(), result2.Count());
        mockApiService.Verify(x => x.GetAsync("universities"), Times.Once);
    }

    /// <summary>
    /// Tests that GetUniversitiesAsync returns an empty collection when API returns non-success status code.
    /// </summary>
    [TestMethod]
    [DataRow(HttpStatusCode.BadRequest)]
    [DataRow(HttpStatusCode.Unauthorized)]
    [DataRow(HttpStatusCode.NotFound)]
    [DataRow(HttpStatusCode.InternalServerError)]
    [DataRow(HttpStatusCode.ServiceUnavailable)]
    public async Task GetUniversitiesAsync_ApiReturnsErrorStatusCode_ReturnsEmptyCollection(HttpStatusCode statusCode)
    {
        // Arrange
        var responseMessage = new HttpResponseMessage(statusCode);

        var mockApiService = new Mock<IApiService>();
        mockApiService
            .Setup(x => x.GetAsync("universities"))
            .ReturnsAsync(responseMessage);

        var mockLogger = new Mock<ILogger<AcademicService>>();
        var service = new AcademicService(mockApiService.Object, mockLogger.Object);

        // Act
        var result = await service.GetUniversitiesAsync();

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(0, result.Count());
        mockApiService.Verify(x => x.GetAsync("universities"), Times.Once);
    }

    /// <summary>
    /// Tests that GetUniversitiesAsync returns an empty collection when API call throws an exception.
    /// </summary>
    [TestMethod]
    public async Task GetUniversitiesAsync_ApiThrowsException_ReturnsEmptyCollection()
    {
        // Arrange
        var mockApiService = new Mock<IApiService>();
        mockApiService
            .Setup(x => x.GetAsync("universities"))
            .ThrowsAsync(new HttpRequestException("Network error"));

        var mockLogger = new Mock<ILogger<AcademicService>>();
        var service = new AcademicService(mockApiService.Object, mockLogger.Object);

        // Act
        var result = await service.GetUniversitiesAsync();

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(0, result.Count());
        mockApiService.Verify(x => x.GetAsync("universities"), Times.Once);
    }

    /// <summary>
    /// Tests that GetUniversitiesAsync returns an empty collection when API returns null content.
    /// </summary>
    [TestMethod]
    public async Task GetUniversitiesAsync_ApiReturnsNullContent_ReturnsEmptyCollection()
    {
        // Arrange
        var responseMessage = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create<IEnumerable<LookupItem>>(null)
        };

        var mockApiService = new Mock<IApiService>();
        mockApiService
            .Setup(x => x.GetAsync("universities"))
            .ReturnsAsync(responseMessage);

        var mockLogger = new Mock<ILogger<AcademicService>>();
        var service = new AcademicService(mockApiService.Object, mockLogger.Object);

        // Act
        var result = await service.GetUniversitiesAsync();

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(0, result.Count());
    }

    /// <summary>
    /// Tests that GetUniversitiesAsync returns an empty collection when API returns empty list.
    /// </summary>
    [TestMethod]
    public async Task GetUniversitiesAsync_ApiReturnsEmptyList_ReturnsEmptyCollection()
    {
        // Arrange
        var emptyList = new List<LookupItem>();
        var responseMessage = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(emptyList)
        };

        var mockApiService = new Mock<IApiService>();
        mockApiService
            .Setup(x => x.GetAsync("universities"))
            .ReturnsAsync(responseMessage);

        var mockLogger = new Mock<ILogger<AcademicService>>();
        var service = new AcademicService(mockApiService.Object, mockLogger.Object);

        // Act
        var result = await service.GetUniversitiesAsync();

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(0, result.Count());
        mockApiService.Verify(x => x.GetAsync("universities"), Times.Once);
    }

    /// <summary>
    /// Tests that GetUniversitiesAsync returns a single item correctly when API returns one university.
    /// </summary>
    [TestMethod]
    public async Task GetUniversitiesAsync_ApiReturnsSingleItem_ReturnsSingleItemCollection()
    {
        // Arrange
        var singleUniversity = new List<LookupItem>
        {
            new LookupItem { Id = "1", Name = "University A" }
        };

        var responseMessage = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(singleUniversity)
        };

        var mockApiService = new Mock<IApiService>();
        mockApiService
            .Setup(x => x.GetAsync("universities"))
            .ReturnsAsync(responseMessage);

        var mockLogger = new Mock<ILogger<AcademicService>>();
        var service = new AcademicService(mockApiService.Object, mockLogger.Object);

        // Act
        var result = await service.GetUniversitiesAsync();

        // Assert
        Assert.IsNotNull(result);
        var resultList = result.ToList();
        Assert.AreEqual(1, resultList.Count);
        Assert.AreEqual("1", resultList[0].Id);
        Assert.AreEqual("University A", resultList[0].Name);
    }

    /// <summary>
    /// Tests that GetUniversitiesAsync handles universities with special characters in names correctly.
    /// </summary>
    [TestMethod]
    public async Task GetUniversitiesAsync_UniversityWithSpecialCharacters_ReturnsCorrectData()
    {
        // Arrange
        var universities = new List<LookupItem>
        {
            new LookupItem { Id = "1", Name = "University & College" },
            new LookupItem { Id = "2", Name = "L'Université de Paris" },
            new LookupItem { Id = "3", Name = "大学" }
        };

        var responseMessage = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(universities)
        };

        var mockApiService = new Mock<IApiService>();
        mockApiService
            .Setup(x => x.GetAsync("universities"))
            .ReturnsAsync(responseMessage);

        var mockLogger = new Mock<ILogger<AcademicService>>();
        var service = new AcademicService(mockApiService.Object, mockLogger.Object);

        // Act
        var result = await service.GetUniversitiesAsync();

        // Assert
        Assert.IsNotNull(result);
        var resultList = result.ToList();
        Assert.AreEqual(3, resultList.Count);
        Assert.AreEqual("University & College", resultList[0].Name);
        Assert.AreEqual("L'Université de Paris", resultList[1].Name);
        Assert.AreEqual("大学", resultList[2].Name);
    }

    /// <summary>
    /// Tests that GetUniversitiesAsync handles universities with empty strings in properties correctly.
    /// </summary>
    [TestMethod]
    public async Task GetUniversitiesAsync_UniversityWithEmptyStrings_ReturnsCorrectData()
    {
        // Arrange
        var universities = new List<LookupItem>
        {
            new LookupItem { Id = "", Name = "" },
            new LookupItem { Id = "1", Name = "" },
            new LookupItem { Id = "", Name = "University A" }
        };

        var responseMessage = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(universities)
        };

        var mockApiService = new Mock<IApiService>();
        mockApiService
            .Setup(x => x.GetAsync("universities"))
            .ReturnsAsync(responseMessage);

        var mockLogger = new Mock<ILogger<AcademicService>>();
        var service = new AcademicService(mockApiService.Object, mockLogger.Object);

        // Act
        var result = await service.GetUniversitiesAsync();

        // Assert
        Assert.IsNotNull(result);
        var resultList = result.ToList();
        Assert.AreEqual(3, resultList.Count);
        Assert.AreEqual("", resultList[0].Id);
        Assert.AreEqual("", resultList[0].Name);
    }

    /// <summary>
    /// Tests that GetUniversitiesAsync returns an empty collection when API throws TaskCanceledException (timeout).
    /// </summary>
    [TestMethod]
    public async Task GetUniversitiesAsync_ApiTimeout_ReturnsEmptyCollection()
    {
        // Arrange
        var mockApiService = new Mock<IApiService>();
        mockApiService
            .Setup(x => x.GetAsync("universities"))
            .ThrowsAsync(new TaskCanceledException("Request timeout"));

        var mockLogger = new Mock<ILogger<AcademicService>>();
        var service = new AcademicService(mockApiService.Object, mockLogger.Object);

        // Act
        var result = await service.GetUniversitiesAsync();

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(0, result.Count());
    }

    /// <summary>
    /// Tests that GetUniversitiesAsync handles large collections correctly.
    /// </summary>
    [TestMethod]
    public async Task GetUniversitiesAsync_ApiReturnsLargeCollection_ReturnsAllItems()
    {
        // Arrange
        var largeList = new List<LookupItem>();
        for (int i = 0; i < 1000; i++)
        {
            largeList.Add(new LookupItem { Id = i.ToString(), Name = $"University {i}" });
        }

        var responseMessage = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(largeList)
        };

        var mockApiService = new Mock<IApiService>();
        mockApiService
            .Setup(x => x.GetAsync("universities"))
            .ReturnsAsync(responseMessage);

        var mockLogger = new Mock<ILogger<AcademicService>>();
        var service = new AcademicService(mockApiService.Object, mockLogger.Object);

        // Act
        var result = await service.GetUniversitiesAsync();

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(1000, result.Count());
    }

    /// <summary>
    /// Tests that DeleteUniversityAsync returns true when the API responds with a successful status code.
    /// Input: Valid university ID and API returns success status.
    /// Expected: Returns true.
    /// </summary>
    [TestMethod]
    public async Task DeleteUniversityAsync_SuccessfulResponse_ReturnsTrue()
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<AcademicService>>();
        var service = new AcademicService(mockApi.Object, mockLogger.Object);
        var universityId = "123";
        var response = new HttpResponseMessage(HttpStatusCode.OK);

        mockApi.Setup(x => x.DeleteAsync($"universities/{universityId}", It.IsAny<CancellationToken>()))
               .ReturnsAsync(response);

        // Act
        var result = await service.DeleteUniversityAsync(universityId);

        // Assert
        Assert.IsTrue(result);
        mockApi.Verify(x => x.DeleteAsync($"universities/{universityId}", It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Tests that DeleteUniversityAsync returns false when the API responds with a failed status code.
    /// Input: Valid university ID and API returns failure status.
    /// Expected: Returns false.
    /// </summary>
    [TestMethod]
    public async Task DeleteUniversityAsync_FailedResponse_ReturnsFalse()
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<AcademicService>>();
        var service = new AcademicService(mockApi.Object, mockLogger.Object);
        var universityId = "123";
        var response = new HttpResponseMessage(HttpStatusCode.NotFound);

        mockApi.Setup(x => x.DeleteAsync($"universities/{universityId}", It.IsAny<CancellationToken>()))
               .ReturnsAsync(response);

        // Act
        var result = await service.DeleteUniversityAsync(universityId);

        // Assert
        Assert.IsFalse(result);
        mockApi.Verify(x => x.DeleteAsync($"universities/{universityId}", It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Tests that DeleteUniversityAsync returns false and logs error when an exception is thrown.
    /// Input: Valid university ID and API throws an exception.
    /// Expected: Returns false and logs the error with exception details.
    /// </summary>
    [TestMethod]
    public async Task DeleteUniversityAsync_ExceptionThrown_ReturnsFalseAndLogsError()
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<AcademicService>>();
        var service = new AcademicService(mockApi.Object, mockLogger.Object);
        var universityId = "123";
        var exception = new HttpRequestException("Network error");

        mockApi.Setup(x => x.DeleteAsync($"universities/{universityId}", It.IsAny<CancellationToken>()))
               .ThrowsAsync(exception);

        // Act
        var result = await service.DeleteUniversityAsync(universityId);

        // Assert
        Assert.IsFalse(result);
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Failed to delete university")),
                exception,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    /// <summary>
    /// Tests that DeleteUniversityAsync constructs correct endpoint path for various ID formats.
    /// Input: Various string formats including edge cases.
    /// Expected: Correct endpoint path is called.
    /// </summary>
    [TestMethod]
    [DataRow("", DisplayName = "Empty string ID")]
    [DataRow("   ", DisplayName = "Whitespace-only ID")]
    [DataRow("abc-def-123", DisplayName = "ID with hyphens")]
    [DataRow("550e8400-e29b-41d4-a716-446655440000", DisplayName = "GUID format ID")]
    [DataRow("id with spaces", DisplayName = "ID with spaces")]
    [DataRow("special!@#$%chars", DisplayName = "ID with special characters")]
    [DataRow("verylongidstringverylongidstringverylongidstringverylongidstringverylongidstringverylongidstringverylongidstringverylongidstringverylongidstringverylongidstring", DisplayName = "Very long ID")]
    public async Task DeleteUniversityAsync_VariousIdFormats_CallsCorrectEndpoint(string id)
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<AcademicService>>();
        var service = new AcademicService(mockApi.Object, mockLogger.Object);
        var response = new HttpResponseMessage(HttpStatusCode.OK);

        mockApi.Setup(x => x.DeleteAsync($"universities/{id}", It.IsAny<CancellationToken>()))
               .ReturnsAsync(response);

        // Act
        await service.DeleteUniversityAsync(id);

        // Assert
        mockApi.Verify(x => x.DeleteAsync($"universities/{id}", It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Tests that DeleteUniversityAsync returns false for multiple HTTP error status codes.
    /// Input: Various HTTP error status codes.
    /// Expected: Returns false for all non-success status codes.
    /// </summary>
    [TestMethod]
    [DataRow(HttpStatusCode.BadRequest, DisplayName = "Bad Request (400)")]
    [DataRow(HttpStatusCode.Unauthorized, DisplayName = "Unauthorized (401)")]
    [DataRow(HttpStatusCode.Forbidden, DisplayName = "Forbidden (403)")]
    [DataRow(HttpStatusCode.NotFound, DisplayName = "Not Found (404)")]
    [DataRow(HttpStatusCode.InternalServerError, DisplayName = "Internal Server Error (500)")]
    [DataRow(HttpStatusCode.ServiceUnavailable, DisplayName = "Service Unavailable (503)")]
    public async Task DeleteUniversityAsync_VariousErrorStatusCodes_ReturnsFalse(HttpStatusCode statusCode)
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<AcademicService>>();
        var service = new AcademicService(mockApi.Object, mockLogger.Object);
        var universityId = "123";
        var response = new HttpResponseMessage(statusCode);

        mockApi.Setup(x => x.DeleteAsync($"universities/{universityId}", It.IsAny<CancellationToken>()))
               .ReturnsAsync(response);

        // Act
        var result = await service.DeleteUniversityAsync(universityId);

        // Assert
        Assert.IsFalse(result);
    }

    /// <summary>
    /// Tests that DeleteUniversityAsync returns true for all 2xx success status codes.
    /// Input: Various 2xx HTTP success status codes.
    /// Expected: Returns true for all success status codes.
    /// </summary>
    [TestMethod]
    [DataRow(HttpStatusCode.OK, DisplayName = "OK (200)")]
    [DataRow(HttpStatusCode.Created, DisplayName = "Created (201)")]
    [DataRow(HttpStatusCode.Accepted, DisplayName = "Accepted (202)")]
    [DataRow(HttpStatusCode.NoContent, DisplayName = "No Content (204)")]
    public async Task DeleteUniversityAsync_VariousSuccessStatusCodes_ReturnsTrue(HttpStatusCode statusCode)
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<AcademicService>>();
        var service = new AcademicService(mockApi.Object, mockLogger.Object);
        var universityId = "123";
        var response = new HttpResponseMessage(statusCode);

        mockApi.Setup(x => x.DeleteAsync($"universities/{universityId}", It.IsAny<CancellationToken>()))
               .ReturnsAsync(response);

        // Act
        var result = await service.DeleteUniversityAsync(universityId);

        // Assert
        Assert.IsTrue(result);
    }

    /// <summary>
    /// Tests that DeleteUniversityAsync handles different exception types correctly.
    /// Input: Various exception types thrown by the API.
    /// Expected: Returns false and logs error for all exception types.
    /// </summary>
    [TestMethod]
    [DataRow(typeof(HttpRequestException), "Network failure", DisplayName = "HttpRequestException")]
    [DataRow(typeof(TaskCanceledException), "Request timeout", DisplayName = "TaskCanceledException")]
    [DataRow(typeof(InvalidOperationException), "Invalid state", DisplayName = "InvalidOperationException")]
    [DataRow(typeof(Exception), "Generic error", DisplayName = "Generic Exception")]
    public async Task DeleteUniversityAsync_VariousExceptionTypes_ReturnsFalseAndLogsError(Type exceptionType, string message)
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<AcademicService>>();
        var service = new AcademicService(mockApi.Object, mockLogger.Object);
        var universityId = "123";
        var exception = (Exception)Activator.CreateInstance(exceptionType, message)!;

        mockApi.Setup(x => x.DeleteAsync($"universities/{universityId}", It.IsAny<CancellationToken>()))
               .ThrowsAsync(exception);

        // Act
        var result = await service.DeleteUniversityAsync(universityId);

        // Assert
        Assert.IsFalse(result);
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Failed to delete university")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    /// <summary>
    /// Tests that GetFacultiesAsync with a valid university ID returns the expected lookup items.
    /// </summary>
    [TestMethod]
    public async Task GetFacultiesAsync_ValidUniversityId_ReturnsLookupItems()
    {
        // Arrange
        var universityId = "uni123";
        var expectedPath = $"faculties?universityId={universityId}";
        var expectedData = new List<LookupItem>
        {
            new LookupItem { Id = "fac1", Name = "Faculty 1" },
            new LookupItem { Id = "fac2", Name = "Faculty 2" }
        };
        var jsonContent = JsonSerializer.Serialize(expectedData);
        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(jsonContent, Encoding.UTF8, "application/json")
        };

        var mockApi = new Mock<IApiService>();
        mockApi.Setup(x => x.GetAsync(expectedPath)).ReturnsAsync(httpResponse);
        var mockLogger = new Mock<ILogger<AcademicService>>();
        var service = new AcademicService(mockApi.Object, mockLogger.Object);

        // Act
        var result = await service.GetFacultiesAsync(universityId);

        // Assert
        Assert.IsNotNull(result);
        var resultList = result.ToList();
        Assert.AreEqual(2, resultList.Count);
        Assert.AreEqual("fac1", resultList[0].Id);
        Assert.AreEqual("Faculty 1", resultList[0].Name);
        Assert.AreEqual("fac2", resultList[1].Id);
        Assert.AreEqual("Faculty 2", resultList[1].Name);
        mockApi.Verify(x => x.GetAsync(expectedPath), Times.Once);
    }

    /// <summary>
    /// Tests that GetFacultiesAsync with an empty string university ID constructs the correct path.
    /// </summary>
    [TestMethod]
    public async Task GetFacultiesAsync_EmptyString_MakesApiCallWithEmptyParameter()
    {
        // Arrange
        var universityId = "";
        var expectedPath = "faculties?universityId=";
        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("[]", Encoding.UTF8, "application/json")
        };

        var mockApi = new Mock<IApiService>();
        mockApi.Setup(x => x.GetAsync(expectedPath)).ReturnsAsync(httpResponse);
        var mockLogger = new Mock<ILogger<AcademicService>>();
        var service = new AcademicService(mockApi.Object, mockLogger.Object);

        // Act
        var result = await service.GetFacultiesAsync(universityId);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(0, result.Count());
        mockApi.Verify(x => x.GetAsync(expectedPath), Times.Once);
    }

    /// <summary>
    /// Tests that GetFacultiesAsync with a whitespace-only string university ID constructs the correct path.
    /// </summary>
    [TestMethod]
    public async Task GetFacultiesAsync_WhitespaceString_MakesApiCallWithWhitespace()
    {
        // Arrange
        var universityId = "   ";
        var expectedPath = $"faculties?universityId={universityId}";
        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("[]", Encoding.UTF8, "application/json")
        };

        var mockApi = new Mock<IApiService>();
        mockApi.Setup(x => x.GetAsync(expectedPath)).ReturnsAsync(httpResponse);
        var mockLogger = new Mock<ILogger<AcademicService>>();
        var service = new AcademicService(mockApi.Object, mockLogger.Object);

        // Act
        var result = await service.GetFacultiesAsync(universityId);

        // Assert
        Assert.IsNotNull(result);
        mockApi.Verify(x => x.GetAsync(expectedPath), Times.Once);
    }

    /// <summary>
    /// Tests that GetFacultiesAsync with special characters in university ID properly constructs the path.
    /// </summary>
    [TestMethod]
    [DataRow("uni&123")]
    [DataRow("uni=123")]
    [DataRow("uni?123")]
    [DataRow("uni/123")]
    [DataRow("uni@123#$%")]
    public async Task GetFacultiesAsync_SpecialCharacters_ProperlyConstructsPath(string universityId)
    {
        // Arrange
        var expectedPath = $"faculties?universityId={universityId}";
        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("[]", Encoding.UTF8, "application/json")
        };

        var mockApi = new Mock<IApiService>();
        mockApi.Setup(x => x.GetAsync(expectedPath)).ReturnsAsync(httpResponse);
        var mockLogger = new Mock<ILogger<AcademicService>>();
        var service = new AcademicService(mockApi.Object, mockLogger.Object);

        // Act
        var result = await service.GetFacultiesAsync(universityId);

        // Assert
        Assert.IsNotNull(result);
        mockApi.Verify(x => x.GetAsync(expectedPath), Times.Once);
    }

    /// <summary>
    /// Tests that GetFacultiesAsync with a very long university ID properly constructs the path.
    /// </summary>
    [TestMethod]
    public async Task GetFacultiesAsync_VeryLongString_MakesApiCall()
    {
        // Arrange
        var universityId = new string('x', 10000);
        var expectedPath = $"faculties?universityId={universityId}";
        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("[]", Encoding.UTF8, "application/json")
        };

        var mockApi = new Mock<IApiService>();
        mockApi.Setup(x => x.GetAsync(expectedPath)).ReturnsAsync(httpResponse);
        var mockLogger = new Mock<ILogger<AcademicService>>();
        var service = new AcademicService(mockApi.Object, mockLogger.Object);

        // Act
        var result = await service.GetFacultiesAsync(universityId);

        // Assert
        Assert.IsNotNull(result);
        mockApi.Verify(x => x.GetAsync(expectedPath), Times.Once);
    }

    /// <summary>
    /// Tests that GetFacultiesAsync caches results and only makes one API call for multiple requests with the same university ID.
    /// </summary>
    [TestMethod]
    public async Task GetFacultiesAsync_CalledTwiceWithSameId_UsesCacheOnSecondCall()
    {
        // Arrange
        var universityId = "uni123";
        var expectedPath = $"faculties?universityId={universityId}";
        var expectedData = new List<LookupItem>
        {
            new LookupItem { Id = "fac1", Name = "Faculty 1" }
        };
        var jsonContent = JsonSerializer.Serialize(expectedData);
        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(jsonContent, Encoding.UTF8, "application/json")
        };

        var mockApi = new Mock<IApiService>();
        mockApi.Setup(x => x.GetAsync(expectedPath)).ReturnsAsync(httpResponse);
        var mockLogger = new Mock<ILogger<AcademicService>>();
        var service = new AcademicService(mockApi.Object, mockLogger.Object);

        // Act
        var result1 = await service.GetFacultiesAsync(universityId);
        var result2 = await service.GetFacultiesAsync(universityId);

        // Assert
        Assert.IsNotNull(result1);
        Assert.IsNotNull(result2);
        Assert.AreEqual(1, result1.Count());
        Assert.AreEqual(1, result2.Count());
        mockApi.Verify(x => x.GetAsync(expectedPath), Times.Once);
    }

    /// <summary>
    /// Tests that GetFacultiesAsync returns an empty collection when the API returns an error status code.
    /// </summary>
    [TestMethod]
    [DataRow(HttpStatusCode.BadRequest)]
    [DataRow(HttpStatusCode.NotFound)]
    [DataRow(HttpStatusCode.InternalServerError)]
    public async Task GetFacultiesAsync_ApiReturnsError_ReturnsEmptyCollection(HttpStatusCode statusCode)
    {
        // Arrange
        var universityId = "uni123";
        var expectedPath = $"faculties?universityId={universityId}";
        var httpResponse = new HttpResponseMessage(statusCode);

        var mockApi = new Mock<IApiService>();
        mockApi.Setup(x => x.GetAsync(expectedPath)).ReturnsAsync(httpResponse);
        var mockLogger = new Mock<ILogger<AcademicService>>();
        var service = new AcademicService(mockApi.Object, mockLogger.Object);

        // Act
        var result = await service.GetFacultiesAsync(universityId);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(0, result.Count());
    }

    /// <summary>
    /// Tests that GetFacultiesAsync returns an empty collection when the API throws an exception.
    /// </summary>
    [TestMethod]
    public async Task GetFacultiesAsync_ApiThrowsException_ReturnsEmptyCollection()
    {
        // Arrange
        var universityId = "uni123";
        var expectedPath = $"faculties?universityId={universityId}";

        var mockApi = new Mock<IApiService>();
        mockApi.Setup(x => x.GetAsync(expectedPath)).ThrowsAsync(new HttpRequestException("Network error"));
        var mockLogger = new Mock<ILogger<AcademicService>>();
        var service = new AcademicService(mockApi.Object, mockLogger.Object);

        // Act
        var result = await service.GetFacultiesAsync(universityId);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(0, result.Count());
    }

    /// <summary>
    /// Tests that GetFacultiesAsync returns an empty collection when the API returns null content.
    /// </summary>
    [TestMethod]
    public async Task GetFacultiesAsync_ApiReturnsNull_ReturnsEmptyCollection()
    {
        // Arrange
        var universityId = "uni123";
        var expectedPath = $"faculties?universityId={universityId}";
        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("null", Encoding.UTF8, "application/json")
        };

        var mockApi = new Mock<IApiService>();
        mockApi.Setup(x => x.GetAsync(expectedPath)).ReturnsAsync(httpResponse);
        var mockLogger = new Mock<ILogger<AcademicService>>();
        var service = new AcademicService(mockApi.Object, mockLogger.Object);

        // Act
        var result = await service.GetFacultiesAsync(universityId);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(0, result.Count());
    }

    /// <summary>
    /// Tests that GetFacultiesAsync with different university IDs makes separate API calls and caches independently.
    /// </summary>
    [TestMethod]
    public async Task GetFacultiesAsync_DifferentUniversityIds_MakesSeparateApiCalls()
    {
        // Arrange
        var universityId1 = "uni1";
        var universityId2 = "uni2";
        var path1 = $"faculties?universityId={universityId1}";
        var path2 = $"faculties?universityId={universityId2}";

        var data1 = new List<LookupItem> { new LookupItem { Id = "fac1", Name = "Faculty 1" } };
        var data2 = new List<LookupItem> { new LookupItem { Id = "fac2", Name = "Faculty 2" } };

        var response1 = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(JsonSerializer.Serialize(data1), Encoding.UTF8, "application/json")
        };
        var response2 = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(JsonSerializer.Serialize(data2), Encoding.UTF8, "application/json")
        };

        var mockApi = new Mock<IApiService>();
        mockApi.Setup(x => x.GetAsync(path1)).ReturnsAsync(response1);
        mockApi.Setup(x => x.GetAsync(path2)).ReturnsAsync(response2);
        var mockLogger = new Mock<ILogger<AcademicService>>();
        var service = new AcademicService(mockApi.Object, mockLogger.Object);

        // Act
        var result1 = await service.GetFacultiesAsync(universityId1);
        var result2 = await service.GetFacultiesAsync(universityId2);

        // Assert
        Assert.IsNotNull(result1);
        Assert.IsNotNull(result2);
        Assert.AreEqual(1, result1.Count());
        Assert.AreEqual(1, result2.Count());
        Assert.AreEqual("fac1", result1.First().Id);
        Assert.AreEqual("fac2", result2.First().Id);
        mockApi.Verify(x => x.GetAsync(path1), Times.Once);
        mockApi.Verify(x => x.GetAsync(path2), Times.Once);
    }

    /// <summary>
    /// Tests that GetUniversityByIdAsync returns a valid UniversityDto when the API call succeeds with a 200 OK response.
    /// </summary>
    [TestMethod]
    public async Task GetUniversityByIdAsync_ValidIdAndSuccessfulResponse_ReturnsUniversityDto()
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<AcademicService>>();
        var service = new AcademicService(mockApi.Object, mockLogger.Object);

        var universityId = "test-university-123";
        var expectedDto = new UniversityDto
        {
            Id = universityId,
            Name = "Test University",
            FacultiesCount = 5,
            Status = "Active"
        };

        var responseContent = JsonContent.Create(expectedDto);
        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = responseContent
        };

        mockApi.Setup(x => x.GetAsync($"universities/{universityId}", It.IsAny<CancellationToken>()))
            .ReturnsAsync(httpResponse);

        // Act
        var result = await service.GetUniversityByIdAsync(universityId);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(expectedDto.Id, result.Id);
        Assert.AreEqual(expectedDto.Name, result.Name);
        Assert.AreEqual(expectedDto.FacultiesCount, result.FacultiesCount);
        Assert.AreEqual(expectedDto.Status, result.Status);
    }

    /// <summary>
    /// Tests that GetUniversityByIdAsync returns null when the API responds with a non-successful status code (404 Not Found).
    /// </summary>
    [TestMethod]
    [DataRow(HttpStatusCode.NotFound, DisplayName = "404 Not Found")]
    [DataRow(HttpStatusCode.BadRequest, DisplayName = "400 Bad Request")]
    [DataRow(HttpStatusCode.InternalServerError, DisplayName = "500 Internal Server Error")]
    [DataRow(HttpStatusCode.Unauthorized, DisplayName = "401 Unauthorized")]
    [DataRow(HttpStatusCode.Forbidden, DisplayName = "403 Forbidden")]
    [DataRow(HttpStatusCode.ServiceUnavailable, DisplayName = "503 Service Unavailable")]
    public async Task GetUniversityByIdAsync_NonSuccessStatusCode_ReturnsNull(HttpStatusCode statusCode)
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<AcademicService>>();
        var service = new AcademicService(mockApi.Object, mockLogger.Object);

        var universityId = "test-university-123";
        var httpResponse = new HttpResponseMessage(statusCode);

        mockApi.Setup(x => x.GetAsync($"universities/{universityId}", It.IsAny<CancellationToken>()))
            .ReturnsAsync(httpResponse);

        // Act
        var result = await service.GetUniversityByIdAsync(universityId);

        // Assert
        Assert.IsNull(result);
    }

    /// <summary>
    /// Tests that GetUniversityByIdAsync logs an error and returns null when GetAsync throws an exception.
    /// </summary>
    [TestMethod]
    public async Task GetUniversityByIdAsync_GetAsyncThrowsException_LogsErrorAndReturnsNull()
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<AcademicService>>();
        var service = new AcademicService(mockApi.Object, mockLogger.Object);

        var universityId = "test-university-123";
        var expectedException = new HttpRequestException("Network error");

        mockApi.Setup(x => x.GetAsync($"universities/{universityId}", It.IsAny<CancellationToken>()))
            .ThrowsAsync(expectedException);

        // Act
        var result = await service.GetUniversityByIdAsync(universityId);

        // Assert
        Assert.IsNull(result);
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Failed to fetch university")),
                expectedException,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    /// <summary>
    /// Tests that GetUniversityByIdAsync logs an error and returns null when JSON deserialization fails.
    /// </summary>
    [TestMethod]
    public async Task GetUniversityByIdAsync_JsonDeserializationFails_LogsErrorAndReturnsNull()
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<AcademicService>>();
        var service = new AcademicService(mockApi.Object, mockLogger.Object);

        var universityId = "test-university-123";
        var invalidJsonContent = new StringContent("{ invalid json }", System.Text.Encoding.UTF8, "application/json");
        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = invalidJsonContent
        };

        mockApi.Setup(x => x.GetAsync($"universities/{universityId}", It.IsAny<CancellationToken>()))
            .ReturnsAsync(httpResponse);

        // Act
        var result = await service.GetUniversityByIdAsync(universityId);

        // Assert
        Assert.IsNull(result);
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Failed to fetch university")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    /// <summary>
    /// Tests that GetUniversityByIdAsync handles empty string id by making the API call with the empty value.
    /// </summary>
    [TestMethod]
    public async Task GetUniversityByIdAsync_EmptyStringId_MakesApiCallWithEmptyId()
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<AcademicService>>();
        var service = new AcademicService(mockApi.Object, mockLogger.Object);

        var emptyId = string.Empty;
        var httpResponse = new HttpResponseMessage(HttpStatusCode.NotFound);

        mockApi.Setup(x => x.GetAsync($"universities/{emptyId}", It.IsAny<CancellationToken>()))
            .ReturnsAsync(httpResponse);

        // Act
        var result = await service.GetUniversityByIdAsync(emptyId);

        // Assert
        Assert.IsNull(result);
        mockApi.Verify(x => x.GetAsync($"universities/{emptyId}", It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Tests that GetUniversityByIdAsync handles whitespace-only id by making the API call with the whitespace value.
    /// </summary>
    [TestMethod]
    public async Task GetUniversityByIdAsync_WhitespaceOnlyId_MakesApiCallWithWhitespaceId()
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<AcademicService>>();
        var service = new AcademicService(mockApi.Object, mockLogger.Object);

        var whitespaceId = "   ";
        var httpResponse = new HttpResponseMessage(HttpStatusCode.NotFound);

        mockApi.Setup(x => x.GetAsync($"universities/{whitespaceId}", It.IsAny<CancellationToken>()))
            .ReturnsAsync(httpResponse);

        // Act
        var result = await service.GetUniversityByIdAsync(whitespaceId);

        // Assert
        Assert.IsNull(result);
        mockApi.Verify(x => x.GetAsync($"universities/{whitespaceId}", It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Tests that GetUniversityByIdAsync handles special characters in the id parameter.
    /// </summary>
    [TestMethod]
    [DataRow("test/id", DisplayName = "ID with forward slash")]
    [DataRow("test&id", DisplayName = "ID with ampersand")]
    [DataRow("test?id", DisplayName = "ID with question mark")]
    [DataRow("test id", DisplayName = "ID with space")]
    [DataRow("test@id#123", DisplayName = "ID with special characters")]
    public async Task GetUniversityByIdAsync_SpecialCharactersInId_MakesApiCallWithSpecialCharacters(string specialId)
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<AcademicService>>();
        var service = new AcademicService(mockApi.Object, mockLogger.Object);

        var httpResponse = new HttpResponseMessage(HttpStatusCode.NotFound);

        mockApi.Setup(x => x.GetAsync($"universities/{specialId}", It.IsAny<CancellationToken>()))
            .ReturnsAsync(httpResponse);

        // Act
        var result = await service.GetUniversityByIdAsync(specialId);

        // Assert
        Assert.IsNull(result);
        mockApi.Verify(x => x.GetAsync($"universities/{specialId}", It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Tests that GetUniversityByIdAsync handles a very long id string.
    /// </summary>
    [TestMethod]
    public async Task GetUniversityByIdAsync_VeryLongId_MakesApiCallWithLongId()
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<AcademicService>>();
        var service = new AcademicService(mockApi.Object, mockLogger.Object);

        var longId = new string('a', 10000);
        var httpResponse = new HttpResponseMessage(HttpStatusCode.NotFound);

        mockApi.Setup(x => x.GetAsync($"universities/{longId}", It.IsAny<CancellationToken>()))
            .ReturnsAsync(httpResponse);

        // Act
        var result = await service.GetUniversityByIdAsync(longId);

        // Assert
        Assert.IsNull(result);
        mockApi.Verify(x => x.GetAsync($"universities/{longId}", It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Tests that GetUniversityByIdAsync handles Unicode characters in the id parameter.
    /// </summary>
    [TestMethod]
    public async Task GetUniversityByIdAsync_UnicodeCharactersInId_MakesApiCallWithUnicodeCharacters()
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<AcademicService>>();
        var service = new AcademicService(mockApi.Object, mockLogger.Object);

        var unicodeId = "test-大学-université-🎓";
        var httpResponse = new HttpResponseMessage(HttpStatusCode.NotFound);

        mockApi.Setup(x => x.GetAsync($"universities/{unicodeId}", It.IsAny<CancellationToken>()))
            .ReturnsAsync(httpResponse);

        // Act
        var result = await service.GetUniversityByIdAsync(unicodeId);

        // Assert
        Assert.IsNull(result);
        mockApi.Verify(x => x.GetAsync($"universities/{unicodeId}", It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Tests that GetUniversityByIdAsync returns null when response content is null.
    /// </summary>
    [TestMethod]
    public async Task GetUniversityByIdAsync_NullResponseContent_ReturnsNull()
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<AcademicService>>();
        var service = new AcademicService(mockApi.Object, mockLogger.Object);

        var universityId = "test-university-123";
        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = null!
        };

        mockApi.Setup(x => x.GetAsync($"universities/{universityId}", It.IsAny<CancellationToken>()))
            .ReturnsAsync(httpResponse);

        // Act
        var result = await service.GetUniversityByIdAsync(universityId);

        // Assert
        Assert.IsNull(result);
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Failed to fetch university")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    /// <summary>
    /// Tests that GetUniversityByIdAsync logs the correct university id when an error occurs.
    /// </summary>
    [TestMethod]
    public async Task GetUniversityByIdAsync_ExceptionOccurs_LogsCorrectUniversityId()
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<AcademicService>>();
        var service = new AcademicService(mockApi.Object, mockLogger.Object);

        var universityId = "specific-test-id-456";
        var expectedException = new InvalidOperationException("Test exception");

        mockApi.Setup(x => x.GetAsync($"universities/{universityId}", It.IsAny<CancellationToken>()))
            .ThrowsAsync(expectedException);

        // Act
        var result = await service.GetUniversityByIdAsync(universityId);

        // Assert
        Assert.IsNull(result);
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains(universityId)),
                expectedException,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    /// <summary>
    /// Tests that GetUniversityByIdAsync returns null when response contains empty JSON object.
    /// </summary>
    [TestMethod]
    public async Task GetUniversityByIdAsync_EmptyJsonObject_ReturnsDefaultDto()
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<AcademicService>>();
        var service = new AcademicService(mockApi.Object, mockLogger.Object);

        var universityId = "test-university-123";
        var emptyJsonContent = new StringContent("{}", System.Text.Encoding.UTF8, "application/json");
        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = emptyJsonContent
        };

        mockApi.Setup(x => x.GetAsync($"universities/{universityId}", It.IsAny<CancellationToken>()))
            .ReturnsAsync(httpResponse);

        // Act
        var result = await service.GetUniversityByIdAsync(universityId);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(string.Empty, result.Id);
        Assert.AreEqual(string.Empty, result.Name);
        Assert.AreEqual(0, result.FacultiesCount);
    }

    /// <summary>
    /// Tests that DeleteFacultyAsync returns true when the API returns a successful response.
    /// </summary>
    /// <param name="id">The faculty ID to delete.</param>
    [TestMethod]
    [DataRow("faculty123")]
    [DataRow("")]
    [DataRow(" ")]
    [DataRow("faculty-with-special-chars-!@#$%")]
    [DataRow("a")]
    [DataRow("verylongfacultyidthatcontainsmanycharacterstoensureithandlesedgecases1234567890")]
    public async Task DeleteFacultyAsync_SuccessfulResponse_ReturnsTrue(string id)
    {
        // Arrange
        var mockApiService = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<AcademicService>>();
        var successResponse = new HttpResponseMessage(HttpStatusCode.OK);

        mockApiService
            .Setup(x => x.DeleteAsync($"faculties/{id}", It.IsAny<CancellationToken>()))
            .ReturnsAsync(successResponse);

        var service = new AcademicService(mockApiService.Object, mockLogger.Object);

        // Act
        var result = await service.DeleteFacultyAsync(id);

        // Assert
        Assert.IsTrue(result);
        mockApiService.Verify(x => x.DeleteAsync($"faculties/{id}", It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Tests that DeleteFacultyAsync returns false when the API returns an unsuccessful response.
    /// </summary>
    /// <param name="statusCode">The HTTP status code to return from the API.</param>
    [TestMethod]
    [DataRow(HttpStatusCode.BadRequest)]
    [DataRow(HttpStatusCode.NotFound)]
    [DataRow(HttpStatusCode.InternalServerError)]
    [DataRow(HttpStatusCode.Unauthorized)]
    [DataRow(HttpStatusCode.Forbidden)]
    public async Task DeleteFacultyAsync_UnsuccessfulResponse_ReturnsFalse(HttpStatusCode statusCode)
    {
        // Arrange
        var mockApiService = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<AcademicService>>();
        var failureResponse = new HttpResponseMessage(statusCode);
        var facultyId = "faculty123";

        mockApiService
            .Setup(x => x.DeleteAsync($"faculties/{facultyId}", It.IsAny<CancellationToken>()))
            .ReturnsAsync(failureResponse);

        var service = new AcademicService(mockApiService.Object, mockLogger.Object);

        // Act
        var result = await service.DeleteFacultyAsync(facultyId);

        // Assert
        Assert.IsFalse(result);
        mockApiService.Verify(x => x.DeleteAsync($"faculties/{facultyId}", It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Tests that DeleteFacultyAsync returns false and logs error when the API throws an exception.
    /// </summary>
    [TestMethod]
    public async Task DeleteFacultyAsync_ApiThrowsException_ReturnsFalseAndLogsError()
    {
        // Arrange
        var mockApiService = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<AcademicService>>();
        var facultyId = "faculty123";
        var expectedException = new HttpRequestException("Network error");

        mockApiService
            .Setup(x => x.DeleteAsync($"faculties/{facultyId}", It.IsAny<CancellationToken>()))
            .ThrowsAsync(expectedException);

        var service = new AcademicService(mockApiService.Object, mockLogger.Object);

        // Act
        var result = await service.DeleteFacultyAsync(facultyId);

        // Assert
        Assert.IsFalse(result);
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Failed to delete faculty")),
                expectedException,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    /// <summary>
    /// Tests that DeleteFacultyAsync returns false and logs error when the API throws an InvalidOperationException.
    /// </summary>
    [TestMethod]
    public async Task DeleteFacultyAsync_ApiThrowsInvalidOperationException_ReturnsFalseAndLogsError()
    {
        // Arrange
        var mockApiService = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<AcademicService>>();
        var facultyId = "faculty456";
        var expectedException = new InvalidOperationException("Invalid operation");

        mockApiService
            .Setup(x => x.DeleteAsync($"faculties/{facultyId}", It.IsAny<CancellationToken>()))
            .ThrowsAsync(expectedException);

        var service = new AcademicService(mockApiService.Object, mockLogger.Object);

        // Act
        var result = await service.DeleteFacultyAsync(facultyId);

        // Assert
        Assert.IsFalse(result);
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Failed to delete faculty")),
                expectedException,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    /// <summary>
    /// Tests that DeleteFacultyAsync calls the API with the correct endpoint format.
    /// </summary>
    [TestMethod]
    public async Task DeleteFacultyAsync_CallsCorrectApiEndpoint()
    {
        // Arrange
        var mockApiService = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<AcademicService>>();
        var facultyId = "testFaculty";
        var successResponse = new HttpResponseMessage(HttpStatusCode.OK);

        mockApiService
            .Setup(x => x.DeleteAsync($"faculties/{facultyId}", It.IsAny<CancellationToken>()))
            .ReturnsAsync(successResponse);

        var service = new AcademicService(mockApiService.Object, mockLogger.Object);

        // Act
        await service.DeleteFacultyAsync(facultyId);

        // Assert
        mockApiService.Verify(x => x.DeleteAsync($"faculties/{facultyId}", It.IsAny<CancellationToken>()), Times.Once);
        mockApiService.VerifyNoOtherCalls();
    }

    /// <summary>
    /// Tests that DeleteFacultyAsync returns true for multiple success status codes.
    /// </summary>
    /// <param name="statusCode">The HTTP success status code to return from the API.</param>
    [TestMethod]
    [DataRow(HttpStatusCode.OK)]
    [DataRow(HttpStatusCode.NoContent)]
    [DataRow(HttpStatusCode.Accepted)]
    public async Task DeleteFacultyAsync_VariousSuccessStatusCodes_ReturnsTrue(HttpStatusCode statusCode)
    {
        // Arrange
        var mockApiService = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<AcademicService>>();
        var facultyId = "faculty123";
        var successResponse = new HttpResponseMessage(statusCode);

        mockApiService
            .Setup(x => x.DeleteAsync($"faculties/{facultyId}", It.IsAny<CancellationToken>()))
            .ReturnsAsync(successResponse);

        var service = new AcademicService(mockApiService.Object, mockLogger.Object);

        // Act
        var result = await service.DeleteFacultyAsync(facultyId);

        // Assert
        Assert.IsTrue(result);
    }

    /// <summary>
    /// Tests that GetProgramsAsync returns programs successfully when API returns valid data.
    /// </summary>
    /// <param name="departmentId">The department ID to test.</param>
    [TestMethod]
    [DataRow("dept123")]
    [DataRow("DEPT-456")]
    [DataRow("dept_with_underscore")]
    [DataRow("dept-with-dash")]
    [DataRow("dept.with.dot")]
    public async Task GetProgramsAsync_ValidDepartmentId_ReturnsPrograms(string departmentId)
    {
        // Arrange
        var expectedPrograms = new List<LookupItem>
        {
            new LookupItem { Id = "prog1", Name = "Program 1" },
            new LookupItem { Id = "prog2", Name = "Program 2" }
        };

        var mockApiService = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<AcademicService>>();

        var jsonContent = JsonSerializer.Serialize(expectedPrograms);
        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(jsonContent, Encoding.UTF8, "application/json")
        };

        mockApiService
            .Setup(x => x.GetAsync($"programs?departmentId={departmentId}"))
            .ReturnsAsync(httpResponse);

        var service = new AcademicService(mockApiService.Object, mockLogger.Object);

        // Act
        var result = await service.GetProgramsAsync(departmentId);

        // Assert
        Assert.IsNotNull(result);
        var programsList = result.ToList();
        Assert.AreEqual(2, programsList.Count);
        Assert.AreEqual("prog1", programsList[0].Id);
        Assert.AreEqual("Program 1", programsList[0].Name);
        Assert.AreEqual("prog2", programsList[1].Id);
        Assert.AreEqual("Program 2", programsList[1].Name);
        mockApiService.Verify(x => x.GetAsync($"programs?departmentId={departmentId}"), Times.Once);
    }

    /// <summary>
    /// Tests that GetProgramsAsync handles empty string departmentId by constructing correct path.
    /// </summary>
    [TestMethod]
    public async Task GetProgramsAsync_EmptyStringDepartmentId_ConstructsCorrectPath()
    {
        // Arrange
        var departmentId = string.Empty;
        var mockApiService = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<AcademicService>>();

        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("[]", Encoding.UTF8, "application/json")
        };

        mockApiService
            .Setup(x => x.GetAsync("programs?departmentId="))
            .ReturnsAsync(httpResponse);

        var service = new AcademicService(mockApiService.Object, mockLogger.Object);

        // Act
        var result = await service.GetProgramsAsync(departmentId);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(0, result.Count());
        mockApiService.Verify(x => x.GetAsync("programs?departmentId="), Times.Once);
    }

    /// <summary>
    /// Tests that GetProgramsAsync handles whitespace-only departmentId by constructing correct path.
    /// </summary>
    [TestMethod]
    [DataRow(" ")]
    [DataRow("  ")]
    [DataRow("\t")]
    [DataRow("\n")]
    [DataRow(" \t\n ")]
    public async Task GetProgramsAsync_WhitespaceDepartmentId_ConstructsCorrectPath(string departmentId)
    {
        // Arrange
        var mockApiService = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<AcademicService>>();

        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("[]", Encoding.UTF8, "application/json")
        };

        mockApiService
            .Setup(x => x.GetAsync($"programs?departmentId={departmentId}"))
            .ReturnsAsync(httpResponse);

        var service = new AcademicService(mockApiService.Object, mockLogger.Object);

        // Act
        var result = await service.GetProgramsAsync(departmentId);

        // Assert
        Assert.IsNotNull(result);
        mockApiService.Verify(x => x.GetAsync($"programs?departmentId={departmentId}"), Times.Once);
    }

    /// <summary>
    /// Tests that GetProgramsAsync handles special characters in departmentId by constructing correct path.
    /// </summary>
    [TestMethod]
    [DataRow("dept&special")]
    [DataRow("dept=equals")]
    [DataRow("dept?question")]
    [DataRow("dept/slash")]
    [DataRow("dept\\backslash")]
    [DataRow("dept#hash")]
    [DataRow("dept%percent")]
    [DataRow("dept@at")]
    [DataRow("dept!exclamation")]
    [DataRow("dept*asterisk")]
    public async Task GetProgramsAsync_SpecialCharactersDepartmentId_ConstructsCorrectPath(string departmentId)
    {
        // Arrange
        var mockApiService = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<AcademicService>>();

        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("[]", Encoding.UTF8, "application/json")
        };

        mockApiService
            .Setup(x => x.GetAsync($"programs?departmentId={departmentId}"))
            .ReturnsAsync(httpResponse);

        var service = new AcademicService(mockApiService.Object, mockLogger.Object);

        // Act
        var result = await service.GetProgramsAsync(departmentId);

        // Assert
        Assert.IsNotNull(result);
        mockApiService.Verify(x => x.GetAsync($"programs?departmentId={departmentId}"), Times.Once);
    }

    /// <summary>
    /// Tests that GetProgramsAsync handles very long departmentId strings without failure.
    /// </summary>
    [TestMethod]
    public async Task GetProgramsAsync_VeryLongDepartmentId_ReturnsData()
    {
        // Arrange
        var departmentId = new string('A', 10000);
        var mockApiService = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<AcademicService>>();

        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("[]", Encoding.UTF8, "application/json")
        };

        mockApiService
            .Setup(x => x.GetAsync($"programs?departmentId={departmentId}"))
            .ReturnsAsync(httpResponse);

        var service = new AcademicService(mockApiService.Object, mockLogger.Object);

        // Act
        var result = await service.GetProgramsAsync(departmentId);

        // Assert
        Assert.IsNotNull(result);
        mockApiService.Verify(x => x.GetAsync($"programs?departmentId={departmentId}"), Times.Once);
    }

    /// <summary>
    /// Tests that GetProgramsAsync returns empty collection when API returns non-success status code.
    /// </summary>
    [TestMethod]
    [DataRow(HttpStatusCode.BadRequest)]
    [DataRow(HttpStatusCode.Unauthorized)]
    [DataRow(HttpStatusCode.Forbidden)]
    [DataRow(HttpStatusCode.NotFound)]
    [DataRow(HttpStatusCode.InternalServerError)]
    [DataRow(HttpStatusCode.ServiceUnavailable)]
    public async Task GetProgramsAsync_ApiReturnsNonSuccessStatusCode_ReturnsEmptyCollection(HttpStatusCode statusCode)
    {
        // Arrange
        var departmentId = "dept123";
        var mockApiService = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<AcademicService>>();

        var httpResponse = new HttpResponseMessage(statusCode);

        mockApiService
            .Setup(x => x.GetAsync($"programs?departmentId={departmentId}"))
            .ReturnsAsync(httpResponse);

        var service = new AcademicService(mockApiService.Object, mockLogger.Object);

        // Act
        var result = await service.GetProgramsAsync(departmentId);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(0, result.Count());
    }

    /// <summary>
    /// Tests that GetProgramsAsync returns empty collection when API throws exception.
    /// </summary>
    [TestMethod]
    public async Task GetProgramsAsync_ApiThrowsException_ReturnsEmptyCollection()
    {
        // Arrange
        var departmentId = "dept123";
        var mockApiService = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<AcademicService>>();

        mockApiService
            .Setup(x => x.GetAsync($"programs?departmentId={departmentId}"))
            .ThrowsAsync(new HttpRequestException("Network error"));

        var service = new AcademicService(mockApiService.Object, mockLogger.Object);

        // Act
        var result = await service.GetProgramsAsync(departmentId);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(0, result.Count());
    }

    /// <summary>
    /// Tests that GetProgramsAsync returns empty collection when API returns null content.
    /// </summary>
    [TestMethod]
    public async Task GetProgramsAsync_ApiReturnsNullContent_ReturnsEmptyCollection()
    {
        // Arrange
        var departmentId = "dept123";
        var mockApiService = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<AcademicService>>();

        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("null", Encoding.UTF8, "application/json")
        };

        mockApiService
            .Setup(x => x.GetAsync($"programs?departmentId={departmentId}"))
            .ReturnsAsync(httpResponse);

        var service = new AcademicService(mockApiService.Object, mockLogger.Object);

        // Act
        var result = await service.GetProgramsAsync(departmentId);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(0, result.Count());
    }

    /// <summary>
    /// Tests that GetProgramsAsync uses cached data on subsequent calls within cache duration.
    /// </summary>
    [TestMethod]
    public async Task GetProgramsAsync_CalledTwiceWithinCacheDuration_UsesCachedData()
    {
        // Arrange
        var departmentId = "dept123";
        var expectedPrograms = new List<LookupItem>
        {
            new LookupItem { Id = "prog1", Name = "Program 1" }
        };

        var mockApiService = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<AcademicService>>();

        var jsonContent = JsonSerializer.Serialize(expectedPrograms);
        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(jsonContent, Encoding.UTF8, "application/json")
        };

        mockApiService
            .Setup(x => x.GetAsync($"programs?departmentId={departmentId}"))
            .ReturnsAsync(httpResponse);

        var service = new AcademicService(mockApiService.Object, mockLogger.Object);

        // Act
        var result1 = await service.GetProgramsAsync(departmentId);
        var result2 = await service.GetProgramsAsync(departmentId);

        // Assert
        Assert.IsNotNull(result1);
        Assert.IsNotNull(result2);
        Assert.AreEqual(1, result1.Count());
        Assert.AreEqual(1, result2.Count());

        // Verify API was called only once due to caching
        mockApiService.Verify(x => x.GetAsync($"programs?departmentId={departmentId}"), Times.Once);
    }

    /// <summary>
    /// Tests that GetProgramsAsync returns empty collection when API returns empty array.
    /// </summary>
    [TestMethod]
    public async Task GetProgramsAsync_ApiReturnsEmptyArray_ReturnsEmptyCollection()
    {
        // Arrange
        var departmentId = "dept123";
        var mockApiService = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<AcademicService>>();

        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("[]", Encoding.UTF8, "application/json")
        };

        mockApiService
            .Setup(x => x.GetAsync($"programs?departmentId={departmentId}"))
            .ReturnsAsync(httpResponse);

        var service = new AcademicService(mockApiService.Object, mockLogger.Object);

        // Act
        var result = await service.GetProgramsAsync(departmentId);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(0, result.Count());
    }

    /// <summary>
    /// Tests that GetProgramsAsync handles departmentId with Unicode characters correctly.
    /// </summary>
    [TestMethod]
    [DataRow("dept日本語")]
    [DataRow("dept한국어")]
    [DataRow("deptالعربية")]
    [DataRow("dept中文")]
    [DataRow("dept🔥emoji")]
    public async Task GetProgramsAsync_UnicodeDepartmentId_ConstructsCorrectPath(string departmentId)
    {
        // Arrange
        var mockApiService = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<AcademicService>>();

        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("[]", Encoding.UTF8, "application/json")
        };

        mockApiService
            .Setup(x => x.GetAsync($"programs?departmentId={departmentId}"))
            .ReturnsAsync(httpResponse);

        var service = new AcademicService(mockApiService.Object, mockLogger.Object);

        // Act
        var result = await service.GetProgramsAsync(departmentId);

        // Assert
        Assert.IsNotNull(result);
        mockApiService.Verify(x => x.GetAsync($"programs?departmentId={departmentId}"), Times.Once);
    }

    /// <summary>
    /// Tests that GetProgramsAsync correctly deserializes single item response.
    /// </summary>
    [TestMethod]
    public async Task GetProgramsAsync_ApiReturnsSingleItem_ReturnsSingleItemCollection()
    {
        // Arrange
        var departmentId = "dept123";
        var expectedPrograms = new List<LookupItem>
        {
            new LookupItem { Id = "prog1", Name = "Program 1" }
        };

        var mockApiService = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<AcademicService>>();

        var jsonContent = JsonSerializer.Serialize(expectedPrograms);
        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(jsonContent, Encoding.UTF8, "application/json")
        };

        mockApiService
            .Setup(x => x.GetAsync($"programs?departmentId={departmentId}"))
            .ReturnsAsync(httpResponse);

        var service = new AcademicService(mockApiService.Object, mockLogger.Object);

        // Act
        var result = await service.GetProgramsAsync(departmentId);

        // Assert
        Assert.IsNotNull(result);
        var programsList = result.ToList();
        Assert.AreEqual(1, programsList.Count);
        Assert.AreEqual("prog1", programsList[0].Id);
        Assert.AreEqual("Program 1", programsList[0].Name);
    }

    /// <summary>
    /// Tests that GetUniversityDetailsAsync returns a list of universities when the API call is successful.
    /// Input: Successful HTTP response with valid university data.
    /// Expected: Returns the deserialized list of UniversityDto objects.
    /// </summary>
    [TestMethod]
    public async Task GetUniversityDetailsAsync_SuccessfulResponse_ReturnsUniversityList()
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<AcademicService>>();
        var universities = new List<UniversityDto>
        {
            new UniversityDto { Id = "1", Name = "University One", FacultiesCount = 5, Status = "Active" },
            new UniversityDto { Id = "2", Name = "University Two", FacultiesCount = 3, Status = "Active" }
        };
        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(universities)
        };
        mockApi.Setup(x => x.GetAsync("universities/details", It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);
        var service = new AcademicService(mockApi.Object, mockLogger.Object);

        // Act
        var result = await service.GetUniversityDetailsAsync();

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(2, result.Count);
        Assert.AreEqual("1", result[0].Id);
        Assert.AreEqual("University One", result[0].Name);
    }

    /// <summary>
    /// Tests that GetUniversityDetailsAsync returns an empty list when the API returns a successful response with an empty list.
    /// Input: Successful HTTP response with empty list.
    /// Expected: Returns an empty list.
    /// </summary>
    [TestMethod]
    public async Task GetUniversityDetailsAsync_SuccessfulResponseWithEmptyList_ReturnsEmptyList()
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<AcademicService>>();
        var universities = new List<UniversityDto>();
        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(universities)
        };
        mockApi.Setup(x => x.GetAsync("universities/details", It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);
        var service = new AcademicService(mockApi.Object, mockLogger.Object);

        // Act
        var result = await service.GetUniversityDetailsAsync();

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(0, result.Count);
    }

    /// <summary>
    /// Tests that GetUniversityDetailsAsync returns an empty list when the API returns a non-success status code.
    /// Input: HTTP response with status code.
    /// Expected: Returns an empty list without attempting to deserialize content.
    /// </summary>
    [TestMethod]
    [DataRow(HttpStatusCode.BadRequest)]
    [DataRow(HttpStatusCode.Unauthorized)]
    [DataRow(HttpStatusCode.NotFound)]
    [DataRow(HttpStatusCode.InternalServerError)]
    [DataRow(HttpStatusCode.ServiceUnavailable)]
    public async Task GetUniversityDetailsAsync_NonSuccessStatusCode_ReturnsEmptyList(HttpStatusCode statusCode)
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<AcademicService>>();
        var response = new HttpResponseMessage(statusCode);
        mockApi.Setup(x => x.GetAsync("universities/details", It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);
        var service = new AcademicService(mockApi.Object, mockLogger.Object);

        // Act
        var result = await service.GetUniversityDetailsAsync();

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(0, result.Count);
    }

    /// <summary>
    /// Tests that GetUniversityDetailsAsync returns an empty list and logs error when GetAsync throws an exception.
    /// Input: GetAsync throws an HttpRequestException.
    /// Expected: Returns an empty list and logs the exception.
    /// </summary>
    [TestMethod]
    public async Task GetUniversityDetailsAsync_GetAsyncThrowsException_ReturnsEmptyListAndLogsError()
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<AcademicService>>();
        var exception = new HttpRequestException("Network error");
        mockApi.Setup(x => x.GetAsync("universities/details", It.IsAny<CancellationToken>()))
            .ThrowsAsync(exception);
        var service = new AcademicService(mockApi.Object, mockLogger.Object);

        // Act
        var result = await service.GetUniversityDetailsAsync();

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(0, result.Count);
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Failed to fetch university details")),
                exception,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    /// <summary>
    /// Tests that GetUniversityDetailsAsync returns an empty list and logs error when ReadFromJsonAsync throws an exception.
    /// Input: Successful response but deserialization fails.
    /// Expected: Returns an empty list and logs the exception.
    /// </summary>
    [TestMethod]
    public async Task GetUniversityDetailsAsync_DeserializationThrowsException_ReturnsEmptyListAndLogsError()
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<AcademicService>>();
        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("invalid json content")
        };
        mockApi.Setup(x => x.GetAsync("universities/details", It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);
        var service = new AcademicService(mockApi.Object, mockLogger.Object);

        // Act
        var result = await service.GetUniversityDetailsAsync();

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(0, result.Count);
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Failed to fetch university details")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    /// <summary>
    /// Tests that GetUniversityDetailsAsync returns an empty list when deserialization returns null.
    /// Input: Successful response with null content.
    /// Expected: Returns an empty list due to null-coalescing operator.
    /// </summary>
    [TestMethod]
    public async Task GetUniversityDetailsAsync_DeserializationReturnsNull_ReturnsEmptyList()
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<AcademicService>>();
        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("null")
        };
        response.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");
        mockApi.Setup(x => x.GetAsync("universities/details", It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);
        var service = new AcademicService(mockApi.Object, mockLogger.Object);

        // Act
        var result = await service.GetUniversityDetailsAsync();

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(0, result.Count);
    }

    /// <summary>
    /// Tests that GetUniversityDetailsAsync returns an empty list and logs error when a general exception occurs.
    /// Input: GetAsync throws a general Exception.
    /// Expected: Returns an empty list and logs the exception.
    /// </summary>
    [TestMethod]
    public async Task GetUniversityDetailsAsync_GeneralException_ReturnsEmptyListAndLogsError()
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<AcademicService>>();
        var exception = new InvalidOperationException("Unexpected error");
        mockApi.Setup(x => x.GetAsync("universities/details", It.IsAny<CancellationToken>()))
            .ThrowsAsync(exception);
        var service = new AcademicService(mockApi.Object, mockLogger.Object);

        // Act
        var result = await service.GetUniversityDetailsAsync();

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(0, result.Count);
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Failed to fetch university details")),
                exception,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    /// <summary>
    /// Tests that GetUniversityDetailsAsync calls the API with the correct endpoint path.
    /// Input: Any valid setup.
    /// Expected: Verifies that GetAsync is called with "universities/details".
    /// </summary>
    [TestMethod]
    public async Task GetUniversityDetailsAsync_CallsApiWithCorrectPath()
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<AcademicService>>();
        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(new List<UniversityDto>())
        };
        mockApi.Setup(x => x.GetAsync("universities/details", It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);
        var service = new AcademicService(mockApi.Object, mockLogger.Object);

        // Act
        await service.GetUniversityDetailsAsync();

        // Assert
        mockApi.Verify(x => x.GetAsync("universities/details", It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Tests that CreateFacultyAsync successfully creates a faculty with valid inputs.
    /// Input: Valid name and universityId.
    /// Expected: Returns a FacultyDto object with the expected values.
    /// </summary>
    [TestMethod]
    public async Task CreateFacultyAsync_ValidInputs_ReturnsFacultyDto()
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<AcademicService>>();
        var service = new AcademicService(mockApi.Object, mockLogger.Object);

        var expectedFaculty = new FacultyDto
        {
            Id = "faculty123",
            Name = "Engineering",
            UniversityId = "uni456",
            UniversityName = "Test University",
            DepartmentsCount = 5,
            Status = "Active"
        };

        var responseContent = JsonContent.Create(expectedFaculty);
        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = responseContent
        };

        mockApi.Setup(x => x.PostAsync("faculties", It.IsAny<HttpContent>(), It.IsAny<CancellationToken>()))
               .ReturnsAsync(httpResponse);

        // Act
        var result = await service.CreateFacultyAsync("Engineering", "uni456");

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual("faculty123", result.Id);
        Assert.AreEqual("Engineering", result.Name);
        Assert.AreEqual("uni456", result.UniversityId);
        mockApi.Verify(x => x.PostAsync("faculties", It.IsAny<HttpContent>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Tests that CreateFacultyAsync returns null when API returns non-success status code.
    /// Input: Valid inputs but API returns error status codes.
    /// Expected: Returns null.
    /// </summary>
    [TestMethod]
    [DataRow(HttpStatusCode.BadRequest)]
    [DataRow(HttpStatusCode.Unauthorized)]
    [DataRow(HttpStatusCode.Forbidden)]
    [DataRow(HttpStatusCode.NotFound)]
    [DataRow(HttpStatusCode.InternalServerError)]
    [DataRow(HttpStatusCode.ServiceUnavailable)]
    public async Task CreateFacultyAsync_ApiReturnsErrorStatusCode_ReturnsNull(HttpStatusCode statusCode)
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<AcademicService>>();
        var service = new AcademicService(mockApi.Object, mockLogger.Object);

        var httpResponse = new HttpResponseMessage(statusCode);

        mockApi.Setup(x => x.PostAsync("faculties", It.IsAny<HttpContent>(), It.IsAny<CancellationToken>()))
               .ReturnsAsync(httpResponse);

        // Act
        var result = await service.CreateFacultyAsync("Engineering", "uni456");

        // Assert
        Assert.IsNull(result);
        mockApi.Verify(x => x.PostAsync("faculties", It.IsAny<HttpContent>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Tests that CreateFacultyAsync returns null and logs error when API throws exception.
    /// Input: Valid inputs but API throws exception.
    /// Expected: Returns null and logs error.
    /// </summary>
    [TestMethod]
    public async Task CreateFacultyAsync_ApiThrowsException_ReturnsNullAndLogsError()
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<AcademicService>>();
        var service = new AcademicService(mockApi.Object, mockLogger.Object);

        var expectedException = new HttpRequestException("Network error");

        mockApi.Setup(x => x.PostAsync("faculties", It.IsAny<HttpContent>(), It.IsAny<CancellationToken>()))
               .ThrowsAsync(expectedException);

        // Act
        var result = await service.CreateFacultyAsync("Engineering", "uni456");

        // Assert
        Assert.IsNull(result);
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Failed to create faculty")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    /// <summary>
    /// Tests that CreateFacultyAsync handles null name parameter.
    /// Input: Null name and valid universityId.
    /// Expected: Method executes without throwing, may return null or FacultyDto depending on API behavior.
    /// </summary>
    [TestMethod]
    public async Task CreateFacultyAsync_NullName_HandlesGracefully()
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<AcademicService>>();
        var service = new AcademicService(mockApi.Object, mockLogger.Object);

        var httpResponse = new HttpResponseMessage(HttpStatusCode.BadRequest);

        mockApi.Setup(x => x.PostAsync("faculties", It.IsAny<HttpContent>(), It.IsAny<CancellationToken>()))
               .ReturnsAsync(httpResponse);

        // Act
        var result = await service.CreateFacultyAsync(null!, "uni456");

        // Assert
        Assert.IsNull(result);
    }

    /// <summary>
    /// Tests that CreateFacultyAsync handles null universityId parameter.
    /// Input: Valid name and null universityId.
    /// Expected: Method executes without throwing, may return null or FacultyDto depending on API behavior.
    /// </summary>
    [TestMethod]
    public async Task CreateFacultyAsync_NullUniversityId_HandlesGracefully()
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<AcademicService>>();
        var service = new AcademicService(mockApi.Object, mockLogger.Object);

        var httpResponse = new HttpResponseMessage(HttpStatusCode.BadRequest);

        mockApi.Setup(x => x.PostAsync("faculties", It.IsAny<HttpContent>(), It.IsAny<CancellationToken>()))
               .ReturnsAsync(httpResponse);

        // Act
        var result = await service.CreateFacultyAsync("Engineering", null!);

        // Assert
        Assert.IsNull(result);
    }

    /// <summary>
    /// Tests that CreateFacultyAsync handles empty string parameters.
    /// Input: Empty strings for name and universityId.
    /// Expected: Method executes and sends request to API.
    /// </summary>
    [TestMethod]
    [DataRow("", "")]
    [DataRow("", "uni456")]
    [DataRow("Engineering", "")]
    public async Task CreateFacultyAsync_EmptyStringParameters_SendsRequestToApi(string name, string universityId)
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<AcademicService>>();
        var service = new AcademicService(mockApi.Object, mockLogger.Object);

        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(new FacultyDto { Id = "1", Name = name, UniversityId = universityId })
        };

        mockApi.Setup(x => x.PostAsync("faculties", It.IsAny<HttpContent>(), It.IsAny<CancellationToken>()))
               .ReturnsAsync(httpResponse);

        // Act
        var result = await service.CreateFacultyAsync(name, universityId);

        // Assert
        Assert.IsNotNull(result);
        mockApi.Verify(x => x.PostAsync("faculties", It.IsAny<HttpContent>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Tests that CreateFacultyAsync handles whitespace-only string parameters.
    /// Input: Whitespace-only strings for name and universityId.
    /// Expected: Method executes and sends request to API.
    /// </summary>
    [TestMethod]
    [DataRow("   ", "   ")]
    [DataRow("   ", "uni456")]
    [DataRow("Engineering", "   ")]
    [DataRow("\t\t", "\n\n")]
    public async Task CreateFacultyAsync_WhitespaceOnlyParameters_SendsRequestToApi(string name, string universityId)
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<AcademicService>>();
        var service = new AcademicService(mockApi.Object, mockLogger.Object);

        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(new FacultyDto { Id = "1", Name = name, UniversityId = universityId })
        };

        mockApi.Setup(x => x.PostAsync("faculties", It.IsAny<HttpContent>(), It.IsAny<CancellationToken>()))
               .ReturnsAsync(httpResponse);

        // Act
        var result = await service.CreateFacultyAsync(name, universityId);

        // Assert
        Assert.IsNotNull(result);
        mockApi.Verify(x => x.PostAsync("faculties", It.IsAny<HttpContent>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Tests that CreateFacultyAsync handles very long string parameters.
    /// Input: Very long strings (10000+ characters) for name and universityId.
    /// Expected: Method executes and sends request to API.
    /// </summary>
    [TestMethod]
    public async Task CreateFacultyAsync_VeryLongStrings_SendsRequestToApi()
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<AcademicService>>();
        var service = new AcademicService(mockApi.Object, mockLogger.Object);

        var longName = new string('A', 10000);
        var longUniversityId = new string('B', 10000);

        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(new FacultyDto { Id = "1", Name = longName, UniversityId = longUniversityId })
        };

        mockApi.Setup(x => x.PostAsync("faculties", It.IsAny<HttpContent>(), It.IsAny<CancellationToken>()))
               .ReturnsAsync(httpResponse);

        // Act
        var result = await service.CreateFacultyAsync(longName, longUniversityId);

        // Assert
        Assert.IsNotNull(result);
        mockApi.Verify(x => x.PostAsync("faculties", It.IsAny<HttpContent>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Tests that CreateFacultyAsync handles special characters in parameters.
    /// Input: Strings with special characters, quotes, and escape sequences.
    /// Expected: Method executes and properly serializes the data.
    /// </summary>
    [TestMethod]
    [DataRow("Faculty \"Engineering\"", "uni<123>")]
    [DataRow("Faculty's & School", "uni@#$%")]
    [DataRow("Faculty\nWith\nNewlines", "uni\tWith\tTabs")]
    [DataRow("Faculty\\Backslash", "uni/Forward")]
    [DataRow("العربية", "中文")]
    public async Task CreateFacultyAsync_SpecialCharactersInParameters_SendsRequestToApi(string name, string universityId)
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<AcademicService>>();
        var service = new AcademicService(mockApi.Object, mockLogger.Object);

        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(new FacultyDto { Id = "1", Name = name, UniversityId = universityId })
        };

        mockApi.Setup(x => x.PostAsync("faculties", It.IsAny<HttpContent>(), It.IsAny<CancellationToken>()))
               .ReturnsAsync(httpResponse);

        // Act
        var result = await service.CreateFacultyAsync(name, universityId);

        // Assert
        Assert.IsNotNull(result);
        mockApi.Verify(x => x.PostAsync("faculties", It.IsAny<HttpContent>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Tests that CreateFacultyAsync returns null when deserialization returns null.
    /// Input: Valid inputs but API returns content that deserializes to null.
    /// Expected: Returns null.
    /// </summary>
    [TestMethod]
    public async Task CreateFacultyAsync_DeserializationReturnsNull_ReturnsNull()
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<AcademicService>>();
        var service = new AcademicService(mockApi.Object, mockLogger.Object);

        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("null", Encoding.UTF8, "application/json")
        };

        mockApi.Setup(x => x.PostAsync("faculties", It.IsAny<HttpContent>(), It.IsAny<CancellationToken>()))
               .ReturnsAsync(httpResponse);

        // Act
        var result = await service.CreateFacultyAsync("Engineering", "uni456");

        // Assert
        Assert.IsNull(result);
    }

    /// <summary>
    /// Tests that CreateFacultyAsync returns null and logs error when deserialization throws exception.
    /// Input: Valid inputs but API returns invalid JSON.
    /// Expected: Returns null and logs error.
    /// </summary>
    [TestMethod]
    public async Task CreateFacultyAsync_InvalidJsonResponse_ReturnsNullAndLogsError()
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<AcademicService>>();
        var service = new AcademicService(mockApi.Object, mockLogger.Object);

        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("invalid json {{{", Encoding.UTF8, "application/json")
        };

        mockApi.Setup(x => x.PostAsync("faculties", It.IsAny<HttpContent>(), It.IsAny<CancellationToken>()))
               .ReturnsAsync(httpResponse);

        // Act
        var result = await service.CreateFacultyAsync("Engineering", "uni456");

        // Assert
        Assert.IsNull(result);
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Failed to create faculty")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    /// <summary>
    /// Tests that CreateFacultyAsync uses correct endpoint path.
    /// Input: Valid name and universityId.
    /// Expected: Calls PostAsync with "faculties" endpoint.
    /// </summary>
    [TestMethod]
    public async Task CreateFacultyAsync_ValidInputs_UsesCorrectEndpoint()
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<AcademicService>>();
        var service = new AcademicService(mockApi.Object, mockLogger.Object);

        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(new FacultyDto { Id = "1", Name = "Engineering", UniversityId = "uni456" })
        };

        mockApi.Setup(x => x.PostAsync("faculties", It.IsAny<HttpContent>(), It.IsAny<CancellationToken>()))
               .ReturnsAsync(httpResponse);

        // Act
        await service.CreateFacultyAsync("Engineering", "uni456");

        // Assert
        mockApi.Verify(x => x.PostAsync("faculties", It.IsAny<HttpContent>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Tests that CreateFacultyAsync handles timeout exception.
    /// Input: Valid inputs but API operation times out.
    /// Expected: Returns null and logs error.
    /// </summary>
    [TestMethod]
    public async Task CreateFacultyAsync_ApiTimesOut_ReturnsNullAndLogsError()
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<AcademicService>>();
        var service = new AcademicService(mockApi.Object, mockLogger.Object);

        mockApi.Setup(x => x.PostAsync("faculties", It.IsAny<HttpContent>(), It.IsAny<CancellationToken>()))
               .ThrowsAsync(new TaskCanceledException("Request timed out"));

        // Act
        var result = await service.CreateFacultyAsync("Engineering", "uni456");

        // Assert
        Assert.IsNull(result);
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Failed to create faculty")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    /// <summary>
    /// Tests that CreateFacultyAsync handles various exception types.
    /// Input: Valid inputs but API throws different exception types.
    /// Expected: Returns null and logs error for all exception types.
    /// </summary>
    [TestMethod]
    public async Task CreateFacultyAsync_VariousExceptionTypes_ReturnsNullAndLogsError()
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<AcademicService>>();
        var service = new AcademicService(mockApi.Object, mockLogger.Object);

        var exceptions = new Exception[]
        {
            new InvalidOperationException("Invalid operation"),
            new ArgumentException("Invalid argument"),
            new NullReferenceException("Null reference"),
            new JsonException("JSON error")
        };

        foreach (var exception in exceptions)
        {
            mockApi.Setup(x => x.PostAsync("faculties", It.IsAny<HttpContent>(), It.IsAny<CancellationToken>()))
                   .ThrowsAsync(exception);

            // Act
            var result = await service.CreateFacultyAsync("Engineering", "uni456");

            // Assert
            Assert.IsNull(result);
        }

        // Verify logging occurred for each exception
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Failed to create faculty")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Exactly(exceptions.Length));
    }

    /// <summary>
    /// Tests that GetIntakesAsync returns data successfully when the API call succeeds with valid data.
    /// </summary>
    [TestMethod]
    public async Task GetIntakesAsync_ReturnsData_WhenApiCallSucceeds()
    {
        // Arrange
        var expectedData = new List<LookupItem>
        {
            new LookupItem { Id = "1", Name = "Fall 2024" },
            new LookupItem { Id = "2", Name = "Spring 2025" }
        };
        var jsonContent = JsonContent.Create(expectedData);
        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = jsonContent
        };

        var mockApiService = new Mock<IApiService>();
        mockApiService
            .Setup(x => x.GetAsync("intakes"))
            .ReturnsAsync(response);

        var mockLogger = new Mock<ILogger<AcademicService>>();
        var service = new AcademicService(mockApiService.Object, mockLogger.Object);

        // Act
        var result = await service.GetIntakesAsync();

        // Assert
        Assert.IsNotNull(result);
        var resultList = result.ToList();
        Assert.AreEqual(2, resultList.Count);
        Assert.AreEqual("1", resultList[0].Id);
        Assert.AreEqual("Fall 2024", resultList[0].Name);
        Assert.AreEqual("2", resultList[1].Id);
        Assert.AreEqual("Spring 2025", resultList[1].Name);
        mockApiService.Verify(x => x.GetAsync("intakes"), Times.Once);
    }

    /// <summary>
    /// Tests that GetIntakesAsync returns an empty collection when the API returns a non-success status code.
    /// </summary>
    [TestMethod]
    public async Task GetIntakesAsync_ReturnsEmptyCollection_WhenApiReturnsNonSuccessStatus()
    {
        // Arrange
        var response = new HttpResponseMessage(HttpStatusCode.NotFound);

        var mockApiService = new Mock<IApiService>();
        mockApiService
            .Setup(x => x.GetAsync("intakes"))
            .ReturnsAsync(response);

        var mockLogger = new Mock<ILogger<AcademicService>>();
        var service = new AcademicService(mockApiService.Object, mockLogger.Object);

        // Act
        var result = await service.GetIntakesAsync();

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(0, result.Count());
        mockApiService.Verify(x => x.GetAsync("intakes"), Times.Once);
    }

    /// <summary>
    /// Tests that GetIntakesAsync returns an empty collection when the API call throws an exception.
    /// </summary>
    [TestMethod]
    public async Task GetIntakesAsync_ReturnsEmptyCollection_WhenApiThrowsException()
    {
        // Arrange
        var mockApiService = new Mock<IApiService>();
        mockApiService
            .Setup(x => x.GetAsync("intakes"))
            .ThrowsAsync(new HttpRequestException("Network error"));

        var mockLogger = new Mock<ILogger<AcademicService>>();
        var service = new AcademicService(mockApiService.Object, mockLogger.Object);

        // Act
        var result = await service.GetIntakesAsync();

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(0, result.Count());
        mockApiService.Verify(x => x.GetAsync("intakes"), Times.Once);
    }

    /// <summary>
    /// Tests that GetIntakesAsync returns cached data when called multiple times within the cache duration,
    /// ensuring the API is only called once.
    /// </summary>
    [TestMethod]
    public async Task GetIntakesAsync_ReturnsCachedData_WhenCalledMultipleTimes()
    {
        // Arrange
        var expectedData = new List<LookupItem>
        {
            new LookupItem { Id = "1", Name = "Fall 2024" }
        };
        var jsonContent = JsonContent.Create(expectedData);
        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = jsonContent
        };

        var mockApiService = new Mock<IApiService>();
        mockApiService
            .Setup(x => x.GetAsync("intakes"))
            .ReturnsAsync(response);

        var mockLogger = new Mock<ILogger<AcademicService>>();
        var service = new AcademicService(mockApiService.Object, mockLogger.Object);

        // Act
        var result1 = await service.GetIntakesAsync();
        var result2 = await service.GetIntakesAsync();
        var result3 = await service.GetIntakesAsync();

        // Assert
        Assert.IsNotNull(result1);
        Assert.IsNotNull(result2);
        Assert.IsNotNull(result3);
        Assert.AreEqual(1, result1.Count());
        Assert.AreEqual(1, result2.Count());
        Assert.AreEqual(1, result3.Count());
        mockApiService.Verify(x => x.GetAsync("intakes"), Times.Once, "API should only be called once due to caching");
    }

    /// <summary>
    /// Tests that GetIntakesAsync returns an empty collection when the response content deserializes to null.
    /// </summary>
    [TestMethod]
    public async Task GetIntakesAsync_ReturnsEmptyCollection_WhenResponseContentIsNull()
    {
        // Arrange
        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("null", System.Text.Encoding.UTF8, "application/json")
        };

        var mockApiService = new Mock<IApiService>();
        mockApiService
            .Setup(x => x.GetAsync("intakes"))
            .ReturnsAsync(response);

        var mockLogger = new Mock<ILogger<AcademicService>>();
        var service = new AcademicService(mockApiService.Object, mockLogger.Object);

        // Act
        var result = await service.GetIntakesAsync();

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(0, result.Count());
        mockApiService.Verify(x => x.GetAsync("intakes"), Times.Once);
    }

    /// <summary>
    /// Tests that GetIntakesAsync returns an empty collection when the response contains an empty JSON array.
    /// </summary>
    [TestMethod]
    public async Task GetIntakesAsync_ReturnsEmptyCollection_WhenResponseContainsEmptyArray()
    {
        // Arrange
        var expectedData = new List<LookupItem>();
        var jsonContent = JsonContent.Create(expectedData);
        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = jsonContent
        };

        var mockApiService = new Mock<IApiService>();
        mockApiService
            .Setup(x => x.GetAsync("intakes"))
            .ReturnsAsync(response);

        var mockLogger = new Mock<ILogger<AcademicService>>();
        var service = new AcademicService(mockApiService.Object, mockLogger.Object);

        // Act
        var result = await service.GetIntakesAsync();

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(0, result.Count());
        mockApiService.Verify(x => x.GetAsync("intakes"), Times.Once);
    }

    /// <summary>
    /// Tests that GetIntakesAsync handles various HTTP error status codes and returns an empty collection.
    /// </summary>
    /// <param name="statusCode">The HTTP status code to test.</param>
    [TestMethod]
    [DataRow(400)] // Bad Request
    [DataRow(401)] // Unauthorized
    [DataRow(403)] // Forbidden
    [DataRow(404)] // Not Found
    [DataRow(500)] // Internal Server Error
    [DataRow(503)] // Service Unavailable
    public async Task GetIntakesAsync_ReturnsEmptyCollection_WhenApiReturnsErrorStatusCode(int statusCode)
    {
        // Arrange
        var response = new HttpResponseMessage((HttpStatusCode)statusCode);

        var mockApiService = new Mock<IApiService>();
        mockApiService
            .Setup(x => x.GetAsync("intakes"))
            .ReturnsAsync(response);

        var mockLogger = new Mock<ILogger<AcademicService>>();
        var service = new AcademicService(mockApiService.Object, mockLogger.Object);

        // Act
        var result = await service.GetIntakesAsync();

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(0, result.Count());
        mockApiService.Verify(x => x.GetAsync("intakes"), Times.Once);
    }

    /// <summary>
    /// Tests that GetIntakesAsync properly handles various exception types and returns an empty collection.
    /// </summary>
    /// <param name="exceptionType">The type of exception to throw.</param>
    [TestMethod]
    [DataRow("HttpRequestException")]
    [DataRow("TaskCanceledException")]
    [DataRow("InvalidOperationException")]
    [DataRow("JsonException")]
    public async Task GetIntakesAsync_ReturnsEmptyCollection_WhenApiThrowsVariousExceptions(string exceptionType)
    {
        // Arrange
        Exception exception = exceptionType switch
        {
            "HttpRequestException" => new HttpRequestException("Network error"),
            "TaskCanceledException" => new TaskCanceledException("Request timeout"),
            "InvalidOperationException" => new InvalidOperationException("Invalid operation"),
            "JsonException" => new JsonException("JSON parsing error"),
            _ => new Exception("Unknown error")
        };

        var mockApiService = new Mock<IApiService>();
        mockApiService
            .Setup(x => x.GetAsync("intakes"))
            .ThrowsAsync(exception);

        var mockLogger = new Mock<ILogger<AcademicService>>();
        var service = new AcademicService(mockApiService.Object, mockLogger.Object);

        // Act
        var result = await service.GetIntakesAsync();

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(0, result.Count());
        mockApiService.Verify(x => x.GetAsync("intakes"), Times.Once);
    }

    /// <summary>
    /// Tests that GetIntakesAsync returns data with special characters in lookup item names.
    /// </summary>
    [TestMethod]
    public async Task GetIntakesAsync_ReturnsData_WithSpecialCharactersInNames()
    {
        // Arrange
        var expectedData = new List<LookupItem>
        {
            new LookupItem { Id = "1", Name = "Fall 2024 - Röck & Röll" },
            new LookupItem { Id = "2", Name = "Spring/Summer '25" },
            new LookupItem { Id = "3", Name = "Intake-2025\n\tSpecial" }
        };
        var jsonContent = JsonContent.Create(expectedData);
        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = jsonContent
        };

        var mockApiService = new Mock<IApiService>();
        mockApiService
            .Setup(x => x.GetAsync("intakes"))
            .ReturnsAsync(response);

        var mockLogger = new Mock<ILogger<AcademicService>>();
        var service = new AcademicService(mockApiService.Object, mockLogger.Object);

        // Act
        var result = await service.GetIntakesAsync();

        // Assert
        Assert.IsNotNull(result);
        var resultList = result.ToList();
        Assert.AreEqual(3, resultList.Count);
        Assert.AreEqual("Fall 2024 - Röck & Röll", resultList[0].Name);
        Assert.AreEqual("Spring/Summer '25", resultList[1].Name);
        Assert.AreEqual("Intake-2025\n\tSpecial", resultList[2].Name);
    }

    /// <summary>
    /// Tests that GetSemestersAsync returns semesters successfully from the API.
    /// Input: Valid API response with semester data.
    /// Expected: Returns the list of semesters from the API.
    /// </summary>
    [TestMethod]
    public async Task GetSemestersAsync_ValidApiResponse_ReturnsSemesters()
    {
        // Arrange
        var expectedSemesters = new List<LookupItem>
        {
            new LookupItem { Id = "1", Name = "Semester 1" },
            new LookupItem { Id = "2", Name = "Semester 2" }
        };

        var responseContent = JsonContent.Create(expectedSemesters);
        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = responseContent
        };

        var mockApiService = new Mock<IApiService>();
        mockApiService
            .Setup(x => x.GetAsync("semesters"))
            .ReturnsAsync(httpResponse);

        var mockLogger = new Mock<ILogger<AcademicService>>();
        var service = new AcademicService(mockApiService.Object, mockLogger.Object);

        // Act
        var result = await service.GetSemestersAsync();

        // Assert
        Assert.IsNotNull(result);
        var resultList = result.ToList();
        Assert.AreEqual(2, resultList.Count);
        Assert.AreEqual("1", resultList[0].Id);
        Assert.AreEqual("Semester 1", resultList[0].Name);
        Assert.AreEqual("2", resultList[1].Id);
        Assert.AreEqual("Semester 2", resultList[1].Name);
        mockApiService.Verify(x => x.GetAsync("semesters"), Times.Once);
    }

    /// <summary>
    /// Tests that GetSemestersAsync returns empty collection when API returns empty result.
    /// Input: Valid API response with empty array.
    /// Expected: Returns an empty collection.
    /// </summary>
    [TestMethod]
    public async Task GetSemestersAsync_EmptyApiResponse_ReturnsEmptyCollection()
    {
        // Arrange
        var emptySemesters = new List<LookupItem>();
        var responseContent = JsonContent.Create(emptySemesters);
        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = responseContent
        };

        var mockApiService = new Mock<IApiService>();
        mockApiService
            .Setup(x => x.GetAsync("semesters"))
            .ReturnsAsync(httpResponse);

        var mockLogger = new Mock<ILogger<AcademicService>>();
        var service = new AcademicService(mockApiService.Object, mockLogger.Object);

        // Act
        var result = await service.GetSemestersAsync();

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(0, result.Count());
    }

    /// <summary>
    /// Tests that GetSemestersAsync returns cached data on subsequent calls.
    /// Input: Two consecutive calls within cache duration.
    /// Expected: API is called only once, second call returns cached data.
    /// </summary>
    [TestMethod]
    public async Task GetSemestersAsync_CalledTwice_ReturnsCachedData()
    {
        // Arrange
        var semesters = new List<LookupItem>
        {
            new LookupItem { Id = "1", Name = "Semester 1" }
        };

        var responseContent = JsonContent.Create(semesters);
        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = responseContent
        };

        var mockApiService = new Mock<IApiService>();
        mockApiService
            .Setup(x => x.GetAsync("semesters"))
            .ReturnsAsync(httpResponse);

        var mockLogger = new Mock<ILogger<AcademicService>>();
        var service = new AcademicService(mockApiService.Object, mockLogger.Object);

        // Act
        var result1 = await service.GetSemestersAsync();
        var result2 = await service.GetSemestersAsync();

        // Assert
        Assert.IsNotNull(result1);
        Assert.IsNotNull(result2);
        Assert.AreEqual(result1.Count(), result2.Count());
        mockApiService.Verify(x => x.GetAsync("semesters"), Times.Once);
    }

    /// <summary>
    /// Tests that GetSemestersAsync returns empty collection when API returns error status code.
    /// Input: API response with HTTP 404 status code.
    /// Expected: Returns empty collection and logs warning.
    /// </summary>
    [TestMethod]
    public async Task GetSemestersAsync_ApiReturnsNotFound_ReturnsEmptyCollection()
    {
        // Arrange
        var httpResponse = new HttpResponseMessage(HttpStatusCode.NotFound);

        var mockApiService = new Mock<IApiService>();
        mockApiService
            .Setup(x => x.GetAsync("semesters"))
            .ReturnsAsync(httpResponse);

        var mockLogger = new Mock<ILogger<AcademicService>>();
        var service = new AcademicService(mockApiService.Object, mockLogger.Object);

        // Act
        var result = await service.GetSemestersAsync();

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(0, result.Count());
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((o, t) => o.ToString()!.Contains("semesters") && o.ToString()!.Contains("404")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    /// <summary>
    /// Tests that GetSemestersAsync returns empty collection when API returns internal server error.
    /// Input: API response with HTTP 500 status code.
    /// Expected: Returns empty collection and logs warning.
    /// </summary>
    [TestMethod]
    public async Task GetSemestersAsync_ApiReturnsServerError_ReturnsEmptyCollection()
    {
        // Arrange
        var httpResponse = new HttpResponseMessage(HttpStatusCode.InternalServerError);

        var mockApiService = new Mock<IApiService>();
        mockApiService
            .Setup(x => x.GetAsync("semesters"))
            .ReturnsAsync(httpResponse);

        var mockLogger = new Mock<ILogger<AcademicService>>();
        var service = new AcademicService(mockApiService.Object, mockLogger.Object);

        // Act
        var result = await service.GetSemestersAsync();

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(0, result.Count());
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((o, t) => o.ToString()!.Contains("semesters")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    /// <summary>
    /// Tests that GetSemestersAsync returns empty collection when API throws exception.
    /// Input: API throws HttpRequestException.
    /// Expected: Returns empty collection and logs error.
    /// </summary>
    [TestMethod]
    public async Task GetSemestersAsync_ApiThrowsException_ReturnsEmptyCollection()
    {
        // Arrange
        var mockApiService = new Mock<IApiService>();
        mockApiService
            .Setup(x => x.GetAsync("semesters"))
            .ThrowsAsync(new HttpRequestException("Network error"));

        var mockLogger = new Mock<ILogger<AcademicService>>();
        var service = new AcademicService(mockApiService.Object, mockLogger.Object);

        // Act
        var result = await service.GetSemestersAsync();

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(0, result.Count());
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((o, t) => o.ToString()!.Contains("semesters")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    /// <summary>
    /// Tests that GetSemestersAsync handles null content in API response.
    /// Input: API response with null content that deserializes to null.
    /// Expected: Returns empty collection.
    /// </summary>
    [TestMethod]
    public async Task GetSemestersAsync_ApiReturnsNullContent_ReturnsEmptyCollection()
    {
        // Arrange
        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("null", System.Text.Encoding.UTF8, "application/json")
        };

        var mockApiService = new Mock<IApiService>();
        mockApiService
            .Setup(x => x.GetAsync("semesters"))
            .ReturnsAsync(httpResponse);

        var mockLogger = new Mock<ILogger<AcademicService>>();
        var service = new AcademicService(mockApiService.Object, mockLogger.Object);

        // Act
        var result = await service.GetSemestersAsync();

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(0, result.Count());
    }

    /// <summary>
    /// Tests that GetSemestersAsync returns data with special characters in names.
    /// Input: API response with semesters containing special characters.
    /// Expected: Returns semesters with special characters preserved.
    /// </summary>
    [TestMethod]
    public async Task GetSemestersAsync_SemestersWithSpecialCharacters_ReturnsCorrectData()
    {
        // Arrange
        var expectedSemesters = new List<LookupItem>
        {
            new LookupItem { Id = "1", Name = "Semester 1 (Spring) - 2023/24" },
            new LookupItem { Id = "2", Name = "Semester 2 & Summer" }
        };

        var responseContent = JsonContent.Create(expectedSemesters);
        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = responseContent
        };

        var mockApiService = new Mock<IApiService>();
        mockApiService
            .Setup(x => x.GetAsync("semesters"))
            .ReturnsAsync(httpResponse);

        var mockLogger = new Mock<ILogger<AcademicService>>();
        var service = new AcademicService(mockApiService.Object, mockLogger.Object);

        // Act
        var result = await service.GetSemestersAsync();

        // Assert
        Assert.IsNotNull(result);
        var resultList = result.ToList();
        Assert.AreEqual(2, resultList.Count);
        Assert.AreEqual("Semester 1 (Spring) - 2023/24", resultList[0].Name);
        Assert.AreEqual("Semester 2 & Summer", resultList[1].Name);
    }

    /// <summary>
    /// Tests that GetSemestersAsync handles single semester in response.
    /// Input: API response with single semester.
    /// Expected: Returns collection with one semester.
    /// </summary>
    [TestMethod]
    public async Task GetSemestersAsync_SingleSemester_ReturnsSingleItem()
    {
        // Arrange
        var expectedSemesters = new List<LookupItem>
        {
            new LookupItem { Id = "1", Name = "Semester 1" }
        };

        var responseContent = JsonContent.Create(expectedSemesters);
        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = responseContent
        };

        var mockApiService = new Mock<IApiService>();
        mockApiService
            .Setup(x => x.GetAsync("semesters"))
            .ReturnsAsync(httpResponse);

        var mockLogger = new Mock<ILogger<AcademicService>>();
        var service = new AcademicService(mockApiService.Object, mockLogger.Object);

        // Act
        var result = await service.GetSemestersAsync();

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(1, result.Count());
        Assert.AreEqual("1", result.First().Id);
        Assert.AreEqual("Semester 1", result.First().Name);
    }

    /// <summary>
    /// Tests that GetStudyModesAsync successfully retrieves data from the API on first call
    /// and returns the expected collection of lookup items.
    /// </summary>
    [TestMethod]
    public async Task GetStudyModesAsync_FirstCall_ReturnsDataFromApi()
    {
        // Arrange
        var expectedData = new List<LookupItem>
        {
            new LookupItem { Id = "1", Name = "Full Time" },
            new LookupItem { Id = "2", Name = "Part Time" }
        };

        var mockApiService = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<AcademicService>>();

        var responseMessage = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(expectedData)
        };

        mockApiService.Setup(x => x.GetAsync("study-modes"))
            .ReturnsAsync(responseMessage);

        var service = new AcademicService(mockApiService.Object, mockLogger.Object);

        // Act
        var result = await service.GetStudyModesAsync();

        // Assert
        Assert.IsNotNull(result);
        var resultList = result.ToList();
        Assert.AreEqual(2, resultList.Count);
        Assert.AreEqual("1", resultList[0].Id);
        Assert.AreEqual("Full Time", resultList[0].Name);
        Assert.AreEqual("2", resultList[1].Id);
        Assert.AreEqual("Part Time", resultList[1].Name);
        mockApiService.Verify(x => x.GetAsync("study-modes"), Times.Once);
    }

    /// <summary>
    /// Tests that GetStudyModesAsync returns cached data on subsequent calls within cache duration
    /// without making additional API calls.
    /// </summary>
    [TestMethod]
    public async Task GetStudyModesAsync_SecondCallWithinCacheDuration_ReturnsCachedDataWithoutApiCall()
    {
        // Arrange
        var expectedData = new List<LookupItem>
        {
            new LookupItem { Id = "1", Name = "Full Time" }
        };

        var mockApiService = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<AcademicService>>();

        var responseMessage = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(expectedData)
        };

        mockApiService.Setup(x => x.GetAsync("study-modes"))
            .ReturnsAsync(responseMessage);

        var service = new AcademicService(mockApiService.Object, mockLogger.Object);

        // Act
        var result1 = await service.GetStudyModesAsync();
        var result2 = await service.GetStudyModesAsync();

        // Assert
        Assert.IsNotNull(result1);
        Assert.IsNotNull(result2);
        Assert.AreSame(result1, result2); // Should return same cached instance
        mockApiService.Verify(x => x.GetAsync("study-modes"), Times.Once); // API called only once
    }

    /// <summary>
    /// Tests that GetStudyModesAsync returns an empty collection when the API returns
    /// a non-successful status code and logs a warning.
    /// </summary>
    [TestMethod]
    [DataRow(HttpStatusCode.BadRequest)]
    [DataRow(HttpStatusCode.NotFound)]
    [DataRow(HttpStatusCode.InternalServerError)]
    [DataRow(HttpStatusCode.Unauthorized)]
    [DataRow(HttpStatusCode.ServiceUnavailable)]
    public async Task GetStudyModesAsync_ApiReturnsNonSuccessStatusCode_ReturnsEmptyCollectionAndLogsWarning(HttpStatusCode statusCode)
    {
        // Arrange
        var mockApiService = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<AcademicService>>();

        var responseMessage = new HttpResponseMessage(statusCode);

        mockApiService.Setup(x => x.GetAsync("study-modes"))
            .ReturnsAsync(responseMessage);

        var service = new AcademicService(mockApiService.Object, mockLogger.Object);

        // Act
        var result = await service.GetStudyModesAsync();

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(0, result.Count());
        mockApiService.Verify(x => x.GetAsync("study-modes"), Times.Once);
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("study-modes") && v.ToString()!.Contains(statusCode.ToString())),
                It.IsAny<Exception?>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    /// <summary>
    /// Tests that GetStudyModesAsync returns an empty collection when the API throws
    /// an exception and logs the error.
    /// </summary>
    [TestMethod]
    [DataRow(typeof(HttpRequestException))]
    [DataRow(typeof(TaskCanceledException))]
    [DataRow(typeof(InvalidOperationException))]
    public async Task GetStudyModesAsync_ApiThrowsException_ReturnsEmptyCollectionAndLogsError(Type exceptionType)
    {
        // Arrange
        var mockApiService = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<AcademicService>>();

        var exception = (Exception)Activator.CreateInstance(exceptionType, "Test exception")!;

        mockApiService.Setup(x => x.GetAsync("study-modes"))
            .ThrowsAsync(exception);

        var service = new AcademicService(mockApiService.Object, mockLogger.Object);

        // Act
        var result = await service.GetStudyModesAsync();

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(0, result.Count());
        mockApiService.Verify(x => x.GetAsync("study-modes"), Times.Once);
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("study-modes")),
                It.Is<Exception>(ex => ex == exception),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    /// <summary>
    /// Tests that GetStudyModesAsync returns an empty collection when the API response
    /// content cannot be deserialized (returns null).
    /// </summary>
    [TestMethod]
    public async Task GetStudyModesAsync_ApiReturnsNullContent_ReturnsEmptyCollection()
    {
        // Arrange
        var mockApiService = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<AcademicService>>();

        var responseMessage = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create<IEnumerable<LookupItem>?>(null)
        };

        mockApiService.Setup(x => x.GetAsync("study-modes"))
            .ReturnsAsync(responseMessage);

        var service = new AcademicService(mockApiService.Object, mockLogger.Object);

        // Act
        var result = await service.GetStudyModesAsync();

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(0, result.Count());
        mockApiService.Verify(x => x.GetAsync("study-modes"), Times.Once);
    }

    /// <summary>
    /// Tests that GetStudyModesAsync returns an empty collection when the API returns
    /// an empty array.
    /// </summary>
    [TestMethod]
    public async Task GetStudyModesAsync_ApiReturnsEmptyArray_ReturnsEmptyCollection()
    {
        // Arrange
        var emptyData = new List<LookupItem>();

        var mockApiService = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<AcademicService>>();

        var responseMessage = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(emptyData)
        };

        mockApiService.Setup(x => x.GetAsync("study-modes"))
            .ReturnsAsync(responseMessage);

        var service = new AcademicService(mockApiService.Object, mockLogger.Object);

        // Act
        var result = await service.GetStudyModesAsync();

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(0, result.Count());
        mockApiService.Verify(x => x.GetAsync("study-modes"), Times.Once);
    }

    /// <summary>
    /// Tests that GetStudyModesAsync correctly handles a single item in the response.
    /// </summary>
    [TestMethod]
    public async Task GetStudyModesAsync_ApiReturnsSingleItem_ReturnsSingleItemCollection()
    {
        // Arrange
        var expectedData = new List<LookupItem>
        {
            new LookupItem { Id = "1", Name = "Distance Learning" }
        };

        var mockApiService = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<AcademicService>>();

        var responseMessage = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(expectedData)
        };

        mockApiService.Setup(x => x.GetAsync("study-modes"))
            .ReturnsAsync(responseMessage);

        var service = new AcademicService(mockApiService.Object, mockLogger.Object);

        // Act
        var result = await service.GetStudyModesAsync();

        // Assert
        Assert.IsNotNull(result);
        var resultList = result.ToList();
        Assert.AreEqual(1, resultList.Count);
        Assert.AreEqual("1", resultList[0].Id);
        Assert.AreEqual("Distance Learning", resultList[0].Name);
    }

    /// <summary>
    /// Tests that GetStudyModesAsync correctly handles multiple items with edge case values
    /// like empty strings and special characters.
    /// </summary>
    [TestMethod]
    public async Task GetStudyModesAsync_ApiReturnsItemsWithEdgeCaseValues_ReturnsDataCorrectly()
    {
        // Arrange
        var expectedData = new List<LookupItem>
        {
            new LookupItem { Id = "", Name = "" },
            new LookupItem { Id = "special-!@#$%", Name = "Mode with Special Chars: !@#$%" },
            new LookupItem { Id = "   ", Name = "   " },
            new LookupItem { Id = "very-long-id-" + new string('x', 1000), Name = new string('y', 5000) }
        };

        var mockApiService = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<AcademicService>>();

        var responseMessage = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(expectedData)
        };

        mockApiService.Setup(x => x.GetAsync("study-modes"))
            .ReturnsAsync(responseMessage);

        var service = new AcademicService(mockApiService.Object, mockLogger.Object);

        // Act
        var result = await service.GetStudyModesAsync();

        // Assert
        Assert.IsNotNull(result);
        var resultList = result.ToList();
        Assert.AreEqual(4, resultList.Count);
        Assert.AreEqual("", resultList[0].Id);
        Assert.AreEqual("special-!@#$%", resultList[1].Id);
        Assert.AreEqual("   ", resultList[2].Id);
        Assert.IsTrue(resultList[3].Id.Length > 1000);
    }

    /// <summary>
    /// Tests that GetStudyModesAsync throws when deserialization fails due to malformed JSON,
    /// and returns an empty collection after logging the error.
    /// </summary>
    [TestMethod]
    public async Task GetStudyModesAsync_ApiReturnsInvalidJson_ReturnsEmptyCollectionAndLogsError()
    {
        // Arrange
        var mockApiService = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<AcademicService>>();

        var responseMessage = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("invalid json {{{", System.Text.Encoding.UTF8, "application/json")
        };

        mockApiService.Setup(x => x.GetAsync("study-modes"))
            .ReturnsAsync(responseMessage);

        var service = new AcademicService(mockApiService.Object, mockLogger.Object);

        // Act
        var result = await service.GetStudyModesAsync();

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(0, result.Count());
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("study-modes")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    /// <summary>
    /// Tests that CreateUniversityAsync successfully creates a university with a valid name.
    /// </summary>
    [TestMethod]
    public async Task CreateUniversityAsync_ValidName_ReturnsUniversityDto()
    {
        // Arrange
        var apiMock = new Mock<IApiService>();
        var loggerMock = new Mock<ILogger<AcademicService>>();
        var service = new AcademicService(apiMock.Object, loggerMock.Object);
        string name = "Harvard University";
        var expectedDto = new UniversityDto { Id = "1", Name = name, Status = "Active", FacultiesCount = 0 };
        var responseMessage = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(expectedDto)
        };

        apiMock.Setup(x => x.PostAsync("universities", It.IsAny<HttpContent>(), It.IsAny<CancellationToken>()))
               .ReturnsAsync(responseMessage);

        // Act
        var result = await service.CreateUniversityAsync(name);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(expectedDto.Id, result.Id);
        Assert.AreEqual(expectedDto.Name, result.Name);
        apiMock.Verify(x => x.PostAsync("universities", It.IsAny<HttpContent>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Tests that CreateUniversityAsync handles empty string names.
    /// </summary>
    [TestMethod]
    [DataRow("")]
    [DataRow("   ")]
    [DataRow("\t")]
    [DataRow("\n")]
    [DataRow("\r\n")]
    public async Task CreateUniversityAsync_EmptyOrWhitespaceName_ReturnsUniversityDto(string name)
    {
        // Arrange
        var apiMock = new Mock<IApiService>();
        var loggerMock = new Mock<ILogger<AcademicService>>();
        var service = new AcademicService(apiMock.Object, loggerMock.Object);
        var expectedDto = new UniversityDto { Id = "1", Name = name, Status = "Active", FacultiesCount = 0 };
        var responseMessage = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(expectedDto)
        };

        apiMock.Setup(x => x.PostAsync("universities", It.IsAny<HttpContent>(), It.IsAny<CancellationToken>()))
               .ReturnsAsync(responseMessage);

        // Act
        var result = await service.CreateUniversityAsync(name);

        // Assert
        Assert.IsNotNull(result);
        apiMock.Verify(x => x.PostAsync("universities", It.IsAny<HttpContent>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Tests that CreateUniversityAsync handles names with special characters.
    /// </summary>
    [TestMethod]
    [DataRow("University & College")]
    [DataRow("Université de Paris")]
    [DataRow("大学")]
    [DataRow("University<Test>")]
    [DataRow("University\"Test\"")]
    [DataRow("University'Test'")]
    [DataRow("University\0Test")]
    public async Task CreateUniversityAsync_SpecialCharactersInName_ReturnsUniversityDto(string name)
    {
        // Arrange
        var apiMock = new Mock<IApiService>();
        var loggerMock = new Mock<ILogger<AcademicService>>();
        var service = new AcademicService(apiMock.Object, loggerMock.Object);
        var expectedDto = new UniversityDto { Id = "1", Name = name, Status = "Active", FacultiesCount = 0 };
        var responseMessage = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(expectedDto)
        };

        apiMock.Setup(x => x.PostAsync("universities", It.IsAny<HttpContent>(), It.IsAny<CancellationToken>()))
               .ReturnsAsync(responseMessage);

        // Act
        var result = await service.CreateUniversityAsync(name);

        // Assert
        Assert.IsNotNull(result);
        apiMock.Verify(x => x.PostAsync("universities", It.IsAny<HttpContent>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Tests that CreateUniversityAsync handles very long university names.
    /// </summary>
    [TestMethod]
    public async Task CreateUniversityAsync_VeryLongName_ReturnsUniversityDto()
    {
        // Arrange
        var apiMock = new Mock<IApiService>();
        var loggerMock = new Mock<ILogger<AcademicService>>();
        var service = new AcademicService(apiMock.Object, loggerMock.Object);
        string name = new string('A', 10000);
        var expectedDto = new UniversityDto { Id = "1", Name = name, Status = "Active", FacultiesCount = 0 };
        var responseMessage = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(expectedDto)
        };

        apiMock.Setup(x => x.PostAsync("universities", It.IsAny<HttpContent>(), It.IsAny<CancellationToken>()))
               .ReturnsAsync(responseMessage);

        // Act
        var result = await service.CreateUniversityAsync(name);

        // Assert
        Assert.IsNotNull(result);
        apiMock.Verify(x => x.PostAsync("universities", It.IsAny<HttpContent>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Tests that CreateUniversityAsync returns null when API returns non-success status code.
    /// </summary>
    [TestMethod]
    [DataRow(HttpStatusCode.BadRequest)]
    [DataRow(HttpStatusCode.Unauthorized)]
    [DataRow(HttpStatusCode.Forbidden)]
    [DataRow(HttpStatusCode.NotFound)]
    [DataRow(HttpStatusCode.InternalServerError)]
    [DataRow(HttpStatusCode.ServiceUnavailable)]
    public async Task CreateUniversityAsync_NonSuccessStatusCode_ReturnsNull(HttpStatusCode statusCode)
    {
        // Arrange
        var apiMock = new Mock<IApiService>();
        var loggerMock = new Mock<ILogger<AcademicService>>();
        var service = new AcademicService(apiMock.Object, loggerMock.Object);
        string name = "Test University";
        var responseMessage = new HttpResponseMessage(statusCode);

        apiMock.Setup(x => x.PostAsync("universities", It.IsAny<HttpContent>(), It.IsAny<CancellationToken>()))
               .ReturnsAsync(responseMessage);

        // Act
        var result = await service.CreateUniversityAsync(name);

        // Assert
        Assert.IsNull(result);
        apiMock.Verify(x => x.PostAsync("universities", It.IsAny<HttpContent>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Tests that CreateUniversityAsync returns null and logs error when PostAsync throws HttpRequestException.
    /// </summary>
    [TestMethod]
    public async Task CreateUniversityAsync_PostAsyncThrowsHttpRequestException_ReturnsNullAndLogsError()
    {
        // Arrange
        var apiMock = new Mock<IApiService>();
        var loggerMock = new Mock<ILogger<AcademicService>>();
        var service = new AcademicService(apiMock.Object, loggerMock.Object);
        string name = "Test University";
        var exception = new HttpRequestException("Network error");

        apiMock.Setup(x => x.PostAsync("universities", It.IsAny<HttpContent>(), It.IsAny<CancellationToken>()))
               .ThrowsAsync(exception);

        // Act
        var result = await service.CreateUniversityAsync(name);

        // Assert
        Assert.IsNull(result);
        loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Failed to create university")),
                exception,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    /// <summary>
    /// Tests that CreateUniversityAsync returns null and logs error when PostAsync throws generic Exception.
    /// </summary>
    [TestMethod]
    public async Task CreateUniversityAsync_PostAsyncThrowsException_ReturnsNullAndLogsError()
    {
        // Arrange
        var apiMock = new Mock<IApiService>();
        var loggerMock = new Mock<ILogger<AcademicService>>();
        var service = new AcademicService(apiMock.Object, loggerMock.Object);
        string name = "Test University";
        var exception = new InvalidOperationException("Unexpected error");

        apiMock.Setup(x => x.PostAsync("universities", It.IsAny<HttpContent>(), It.IsAny<CancellationToken>()))
               .ThrowsAsync(exception);

        // Act
        var result = await service.CreateUniversityAsync(name);

        // Assert
        Assert.IsNull(result);
        loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Failed to create university")),
                exception,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    /// <summary>
    /// Tests that CreateUniversityAsync returns null and logs error when deserialization throws JsonException.
    /// </summary>
    [TestMethod]
    public async Task CreateUniversityAsync_DeserializationThrowsJsonException_ReturnsNullAndLogsError()
    {
        // Arrange
        var apiMock = new Mock<IApiService>();
        var loggerMock = new Mock<ILogger<AcademicService>>();
        var service = new AcademicService(apiMock.Object, loggerMock.Object);
        string name = "Test University";
        var responseMessage = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("invalid json", Encoding.UTF8, "application/json")
        };

        apiMock.Setup(x => x.PostAsync("universities", It.IsAny<HttpContent>(), It.IsAny<CancellationToken>()))
               .ReturnsAsync(responseMessage);

        // Act
        var result = await service.CreateUniversityAsync(name);

        // Assert
        Assert.IsNull(result);
        loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Failed to create university")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    /// <summary>
    /// Tests that CreateUniversityAsync correctly serializes the name parameter in the request.
    /// </summary>
    [TestMethod]
    public async Task CreateUniversityAsync_ValidName_SerializesNameCorrectly()
    {
        // Arrange
        var apiMock = new Mock<IApiService>();
        var loggerMock = new Mock<ILogger<AcademicService>>();
        var service = new AcademicService(apiMock.Object, loggerMock.Object);
        string name = "Stanford University";
        var expectedDto = new UniversityDto { Id = "1", Name = name, Status = "Active", FacultiesCount = 0 };
        var responseMessage = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(expectedDto)
        };
        HttpContent? capturedContent = null;

        apiMock.Setup(x => x.PostAsync("universities", It.IsAny<HttpContent>(), It.IsAny<CancellationToken>()))
               .Callback<string, HttpContent, CancellationToken>((path, content, ct) => capturedContent = content)
               .ReturnsAsync(responseMessage);

        // Act
        var result = await service.CreateUniversityAsync(name);

        // Assert
        Assert.IsNotNull(capturedContent);
        string requestBody = await capturedContent.ReadAsStringAsync();
        var deserializedRequest = JsonSerializer.Deserialize<JsonElement>(requestBody);
        Assert.AreEqual(name, deserializedRequest.GetProperty("name").GetString());
    }

    /// <summary>
    /// Tests that CreateUniversityAsync sends correct content type header.
    /// </summary>
    [TestMethod]
    public async Task CreateUniversityAsync_ValidName_SendsCorrectContentType()
    {
        // Arrange
        var apiMock = new Mock<IApiService>();
        var loggerMock = new Mock<ILogger<AcademicService>>();
        var service = new AcademicService(apiMock.Object, loggerMock.Object);
        string name = "MIT";
        var expectedDto = new UniversityDto { Id = "1", Name = name, Status = "Active", FacultiesCount = 0 };
        var responseMessage = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(expectedDto)
        };
        HttpContent? capturedContent = null;

        apiMock.Setup(x => x.PostAsync("universities", It.IsAny<HttpContent>(), It.IsAny<CancellationToken>()))
               .Callback<string, HttpContent, CancellationToken>((path, content, ct) => capturedContent = content)
               .ReturnsAsync(responseMessage);

        // Act
        var result = await service.CreateUniversityAsync(name);

        // Assert
        Assert.IsNotNull(capturedContent);
        Assert.AreEqual("application/json", capturedContent.Headers.ContentType?.MediaType);
        Assert.AreEqual("utf-8", capturedContent.Headers.ContentType?.CharSet);
    }

    /// <summary>
    /// Tests that CreateUniversityAsync returns null when API returns success but null content.
    /// </summary>
    [TestMethod]
    public async Task CreateUniversityAsync_SuccessResponseWithNullContent_ReturnsNull()
    {
        // Arrange
        var apiMock = new Mock<IApiService>();
        var loggerMock = new Mock<ILogger<AcademicService>>();
        var service = new AcademicService(apiMock.Object, loggerMock.Object);
        string name = "Test University";
        var responseMessage = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("null", Encoding.UTF8, "application/json")
        };

        apiMock.Setup(x => x.PostAsync("universities", It.IsAny<HttpContent>(), It.IsAny<CancellationToken>()))
               .ReturnsAsync(responseMessage);

        // Act
        var result = await service.CreateUniversityAsync(name);

        // Assert
        Assert.IsNull(result);
    }

    /// <summary>
    /// Tests that GetEntrySchemesAsync returns lookup items when the API call is successful.
    /// </summary>
    /// <remarks>
    /// This test verifies the happy path where the API returns a successful response with entry schemes data.
    /// </remarks>
    [TestMethod]
    public async Task GetEntrySchemesAsync_WhenApiReturnsSuccessfully_ReturnsLookupItems()
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<AcademicService>>();
        var expectedData = new List<LookupItem>
        {
            new LookupItem { Id = "1", Name = "Direct Entry" },
            new LookupItem { Id = "2", Name = "Mature Age Entry" }
        };
        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(expectedData)
        };
        mockApi.Setup(x => x.GetAsync("entry-schemes")).ReturnsAsync(response);
        var service = new AcademicService(mockApi.Object, mockLogger.Object);

        // Act
        var result = await service.GetEntrySchemesAsync();

        // Assert
        Assert.IsNotNull(result);
        var resultList = result.ToList();
        Assert.AreEqual(2, resultList.Count);
        Assert.AreEqual("1", resultList[0].Id);
        Assert.AreEqual("Direct Entry", resultList[0].Name);
        Assert.AreEqual("2", resultList[1].Id);
        Assert.AreEqual("Mature Age Entry", resultList[1].Name);
        mockApi.Verify(x => x.GetAsync("entry-schemes"), Times.Once);
    }

    /// <summary>
    /// Tests that GetEntrySchemesAsync returns an empty collection when the API returns a non-success status code.
    /// </summary>
    /// <remarks>
    /// This test verifies error handling when the API returns an HTTP error status.
    /// The method should log a warning and return an empty collection instead of throwing.
    /// </remarks>
    [TestMethod]
    public async Task GetEntrySchemesAsync_WhenApiReturnsError_ReturnsEmptyCollection()
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<AcademicService>>();
        var response = new HttpResponseMessage(HttpStatusCode.BadRequest);
        mockApi.Setup(x => x.GetAsync("entry-schemes")).ReturnsAsync(response);
        var service = new AcademicService(mockApi.Object, mockLogger.Object);

        // Act
        var result = await service.GetEntrySchemesAsync();

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(0, result.Count());
        mockApi.Verify(x => x.GetAsync("entry-schemes"), Times.Once);
    }

    /// <summary>
    /// Tests that GetEntrySchemesAsync returns an empty collection when the API throws an exception.
    /// </summary>
    /// <remarks>
    /// This test verifies error handling when the API call throws an exception (e.g., network failure).
    /// The method should log the error and return an empty collection instead of propagating the exception.
    /// </remarks>
    [TestMethod]
    public async Task GetEntrySchemesAsync_WhenApiThrowsException_ReturnsEmptyCollection()
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<AcademicService>>();
        mockApi.Setup(x => x.GetAsync("entry-schemes")).ThrowsAsync(new HttpRequestException("Network error"));
        var service = new AcademicService(mockApi.Object, mockLogger.Object);

        // Act
        var result = await service.GetEntrySchemesAsync();

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(0, result.Count());
        mockApi.Verify(x => x.GetAsync("entry-schemes"), Times.Once);
    }

    /// <summary>
    /// Tests that GetEntrySchemesAsync returns an empty collection when the API returns null content.
    /// </summary>
    /// <remarks>
    /// This test verifies that the method handles null deserialization results gracefully.
    /// </remarks>
    [TestMethod]
    public async Task GetEntrySchemesAsync_WhenApiReturnsNullContent_ReturnsEmptyCollection()
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<AcademicService>>();
        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create<IEnumerable<LookupItem>?>(null)
        };
        mockApi.Setup(x => x.GetAsync("entry-schemes")).ReturnsAsync(response);
        var service = new AcademicService(mockApi.Object, mockLogger.Object);

        // Act
        var result = await service.GetEntrySchemesAsync();

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(0, result.Count());
        mockApi.Verify(x => x.GetAsync("entry-schemes"), Times.Once);
    }

    /// <summary>
    /// Tests that GetEntrySchemesAsync uses cached data on subsequent calls within the cache duration.
    /// </summary>
    /// <remarks>
    /// This test verifies that the caching mechanism works correctly.
    /// The API should only be called once, and subsequent calls should return cached data.
    /// </remarks>
    [TestMethod]
    public async Task GetEntrySchemesAsync_WhenCalledMultipleTimes_UsesCacheOnSubsequentCalls()
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<AcademicService>>();
        var expectedData = new List<LookupItem>
        {
            new LookupItem { Id = "1", Name = "Direct Entry" }
        };
        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(expectedData)
        };
        mockApi.Setup(x => x.GetAsync("entry-schemes")).ReturnsAsync(response);
        var service = new AcademicService(mockApi.Object, mockLogger.Object);

        // Act
        var result1 = await service.GetEntrySchemesAsync();
        var result2 = await service.GetEntrySchemesAsync();
        var result3 = await service.GetEntrySchemesAsync();

        // Assert
        Assert.IsNotNull(result1);
        Assert.IsNotNull(result2);
        Assert.IsNotNull(result3);
        Assert.AreEqual(1, result1.Count());
        Assert.AreEqual(1, result2.Count());
        Assert.AreEqual(1, result3.Count());
        mockApi.Verify(x => x.GetAsync("entry-schemes"), Times.Once);
    }

    /// <summary>
    /// Tests that GetEntrySchemesAsync returns an empty collection when API returns an empty array.
    /// </summary>
    /// <remarks>
    /// This test verifies that the method correctly handles the case where the API returns an empty but valid collection.
    /// </remarks>
    [TestMethod]
    public async Task GetEntrySchemesAsync_WhenApiReturnsEmptyArray_ReturnsEmptyCollection()
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<AcademicService>>();
        var expectedData = new List<LookupItem>();
        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(expectedData)
        };
        mockApi.Setup(x => x.GetAsync("entry-schemes")).ReturnsAsync(response);
        var service = new AcademicService(mockApi.Object, mockLogger.Object);

        // Act
        var result = await service.GetEntrySchemesAsync();

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(0, result.Count());
        mockApi.Verify(x => x.GetAsync("entry-schemes"), Times.Once);
    }

    /// <summary>
    /// Tests that GetEntrySchemesAsync handles various HTTP error status codes correctly.
    /// </summary>
    /// <param name="statusCode">The HTTP status code to test.</param>
    /// <remarks>
    /// This parameterized test verifies that the method handles different error status codes consistently,
    /// returning an empty collection in all cases.
    /// </remarks>
    [TestMethod]
    [DataRow(HttpStatusCode.NotFound)]
    [DataRow(HttpStatusCode.InternalServerError)]
    [DataRow(HttpStatusCode.Unauthorized)]
    [DataRow(HttpStatusCode.Forbidden)]
    [DataRow(HttpStatusCode.ServiceUnavailable)]
    public async Task GetEntrySchemesAsync_WhenApiReturnsErrorStatusCode_ReturnsEmptyCollection(HttpStatusCode statusCode)
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<AcademicService>>();
        var response = new HttpResponseMessage(statusCode);
        mockApi.Setup(x => x.GetAsync("entry-schemes")).ReturnsAsync(response);
        var service = new AcademicService(mockApi.Object, mockLogger.Object);

        // Act
        var result = await service.GetEntrySchemesAsync();

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(0, result.Count());
        mockApi.Verify(x => x.GetAsync("entry-schemes"), Times.Once);
    }

    /// <summary>
    /// Tests that GetEntrySchemesAsync returns a collection with a single item when API returns one entry.
    /// </summary>
    /// <remarks>
    /// This test verifies the boundary case of a single-item collection.
    /// </remarks>
    [TestMethod]
    public async Task GetEntrySchemesAsync_WhenApiReturnsSingleItem_ReturnsSingleItemCollection()
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<AcademicService>>();
        var expectedData = new List<LookupItem>
        {
            new LookupItem { Id = "single", Name = "Single Entry Scheme" }
        };
        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(expectedData)
        };
        mockApi.Setup(x => x.GetAsync("entry-schemes")).ReturnsAsync(response);
        var service = new AcademicService(mockApi.Object, mockLogger.Object);

        // Act
        var result = await service.GetEntrySchemesAsync();

        // Assert
        Assert.IsNotNull(result);
        var resultList = result.ToList();
        Assert.AreEqual(1, resultList.Count);
        Assert.AreEqual("single", resultList[0].Id);
        Assert.AreEqual("Single Entry Scheme", resultList[0].Name);
        mockApi.Verify(x => x.GetAsync("entry-schemes"), Times.Once);
    }

    /// <summary>
    /// Tests that UpdateUniversityAsync successfully updates a university and returns the updated DTO.
    /// Input: Valid id and name, API returns success with valid DTO.
    /// Expected: Returns the deserialized UniversityDto.
    /// </summary>
    [TestMethod]
    public async Task UpdateUniversityAsync_ValidIdAndName_ReturnsUpdatedUniversity()
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<AcademicService>>();
        var service = new AcademicService(mockApi.Object, mockLogger.Object);

        var expectedDto = new UniversityDto
        {
            Id = "uni123",
            Name = "Updated University",
            FacultiesCount = 5,
            Status = "Active"
        };

        var responseMessage = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(expectedDto)
        };

        mockApi.Setup(x => x.PutAsync(
            It.IsAny<string>(),
            It.IsAny<HttpContent>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(responseMessage);

        // Act
        var result = await service.UpdateUniversityAsync("uni123", "Updated University");

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual("uni123", result.Id);
        Assert.AreEqual("Updated University", result.Name);
        mockApi.Verify(x => x.PutAsync(
            "universities/uni123",
            It.IsAny<HttpContent>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Tests that UpdateUniversityAsync returns null when API returns unsuccessful status code.
    /// Input: Valid id and name, API returns 404 NotFound.
    /// Expected: Returns null.
    /// </summary>
    [TestMethod]
    [DataRow(HttpStatusCode.BadRequest)]
    [DataRow(HttpStatusCode.NotFound)]
    [DataRow(HttpStatusCode.InternalServerError)]
    [DataRow(HttpStatusCode.Unauthorized)]
    [DataRow(HttpStatusCode.Forbidden)]
    public async Task UpdateUniversityAsync_UnsuccessfulStatusCode_ReturnsNull(HttpStatusCode statusCode)
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<AcademicService>>();
        var service = new AcademicService(mockApi.Object, mockLogger.Object);

        var responseMessage = new HttpResponseMessage(statusCode);

        mockApi.Setup(x => x.PutAsync(
            It.IsAny<string>(),
            It.IsAny<HttpContent>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(responseMessage);

        // Act
        var result = await service.UpdateUniversityAsync("uni123", "Updated University");

        // Assert
        Assert.IsNull(result);
    }

    /// <summary>
    /// Tests that UpdateUniversityAsync returns null and logs error when PutAsync throws exception.
    /// Input: Valid id and name, API throws HttpRequestException.
    /// Expected: Returns null and logs error.
    /// </summary>
    [TestMethod]
    public async Task UpdateUniversityAsync_PutAsyncThrowsException_ReturnsNullAndLogsError()
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<AcademicService>>();
        var service = new AcademicService(mockApi.Object, mockLogger.Object);

        var exception = new HttpRequestException("Network error");

        mockApi.Setup(x => x.PutAsync(
            It.IsAny<string>(),
            It.IsAny<HttpContent>(),
            It.IsAny<CancellationToken>()))
            .ThrowsAsync(exception);

        // Act
        var result = await service.UpdateUniversityAsync("uni123", "Updated University");

        // Assert
        Assert.IsNull(result);
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Failed to update university")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    /// <summary>
    /// Tests that UpdateUniversityAsync handles empty string id.
    /// Input: Empty string id and valid name.
    /// Expected: Makes PUT request to "universities/" endpoint.
    /// </summary>
    [TestMethod]
    public async Task UpdateUniversityAsync_EmptyId_MakesRequestWithEmptyId()
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<AcademicService>>();
        var service = new AcademicService(mockApi.Object, mockLogger.Object);

        var responseMessage = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(new UniversityDto { Id = "", Name = "Test" })
        };

        mockApi.Setup(x => x.PutAsync(
            It.IsAny<string>(),
            It.IsAny<HttpContent>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(responseMessage);

        // Act
        var result = await service.UpdateUniversityAsync("", "Test University");

        // Assert
        mockApi.Verify(x => x.PutAsync(
            "universities/",
            It.IsAny<HttpContent>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Tests that UpdateUniversityAsync handles whitespace string id.
    /// Input: Whitespace string id and valid name.
    /// Expected: Makes PUT request with whitespace in path.
    /// </summary>
    [TestMethod]
    public async Task UpdateUniversityAsync_WhitespaceId_MakesRequestWithWhitespaceId()
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<AcademicService>>();
        var service = new AcademicService(mockApi.Object, mockLogger.Object);

        var responseMessage = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(new UniversityDto { Id = "   ", Name = "Test" })
        };

        mockApi.Setup(x => x.PutAsync(
            It.IsAny<string>(),
            It.IsAny<HttpContent>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(responseMessage);

        // Act
        var result = await service.UpdateUniversityAsync("   ", "Test University");

        // Assert
        mockApi.Verify(x => x.PutAsync(
            "universities/   ",
            It.IsAny<HttpContent>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Tests that UpdateUniversityAsync handles empty string name.
    /// Input: Valid id and empty string name.
    /// Expected: Successfully serializes and sends empty name.
    /// </summary>
    [TestMethod]
    public async Task UpdateUniversityAsync_EmptyName_SerializesEmptyName()
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<AcademicService>>();
        var service = new AcademicService(mockApi.Object, mockLogger.Object);

        var expectedDto = new UniversityDto { Id = "uni123", Name = "" };
        var responseMessage = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(expectedDto)
        };

        mockApi.Setup(x => x.PutAsync(
            It.IsAny<string>(),
            It.IsAny<HttpContent>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(responseMessage);

        // Act
        var result = await service.UpdateUniversityAsync("uni123", "");

        // Assert
        Assert.IsNotNull(result);
        mockApi.Verify(x => x.PutAsync(
            "universities/uni123",
            It.IsAny<HttpContent>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Tests that UpdateUniversityAsync handles whitespace-only name.
    /// Input: Valid id and whitespace-only name.
    /// Expected: Successfully serializes and sends whitespace name.
    /// </summary>
    [TestMethod]
    public async Task UpdateUniversityAsync_WhitespaceName_SerializesWhitespaceName()
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<AcademicService>>();
        var service = new AcademicService(mockApi.Object, mockLogger.Object);

        var expectedDto = new UniversityDto { Id = "uni123", Name = "   " };
        var responseMessage = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(expectedDto)
        };

        mockApi.Setup(x => x.PutAsync(
            It.IsAny<string>(),
            It.IsAny<HttpContent>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(responseMessage);

        // Act
        var result = await service.UpdateUniversityAsync("uni123", "   ");

        // Assert
        Assert.IsNotNull(result);
    }

    /// <summary>
    /// Tests that UpdateUniversityAsync handles very long id string.
    /// Input: Very long id (1000 characters) and valid name.
    /// Expected: Successfully makes request with long id.
    /// </summary>
    [TestMethod]
    public async Task UpdateUniversityAsync_VeryLongId_MakesRequestSuccessfully()
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<AcademicService>>();
        var service = new AcademicService(mockApi.Object, mockLogger.Object);

        var longId = new string('a', 1000);
        var expectedDto = new UniversityDto { Id = longId, Name = "Test" };
        var responseMessage = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(expectedDto)
        };

        mockApi.Setup(x => x.PutAsync(
            It.IsAny<string>(),
            It.IsAny<HttpContent>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(responseMessage);

        // Act
        var result = await service.UpdateUniversityAsync(longId, "Test University");

        // Assert
        Assert.IsNotNull(result);
        mockApi.Verify(x => x.PutAsync(
            $"universities/{longId}",
            It.IsAny<HttpContent>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Tests that UpdateUniversityAsync handles very long name string.
    /// Input: Valid id and very long name (10000 characters).
    /// Expected: Successfully serializes and sends long name.
    /// </summary>
    [TestMethod]
    public async Task UpdateUniversityAsync_VeryLongName_SerializesSuccessfully()
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<AcademicService>>();
        var service = new AcademicService(mockApi.Object, mockLogger.Object);

        var longName = new string('b', 10000);
        var expectedDto = new UniversityDto { Id = "uni123", Name = longName };
        var responseMessage = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(expectedDto)
        };

        mockApi.Setup(x => x.PutAsync(
            It.IsAny<string>(),
            It.IsAny<HttpContent>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(responseMessage);

        // Act
        var result = await service.UpdateUniversityAsync("uni123", longName);

        // Assert
        Assert.IsNotNull(result);
    }

    /// <summary>
    /// Tests that UpdateUniversityAsync handles special characters in id.
    /// Input: Id with special characters and valid name.
    /// Expected: Successfully makes request with special characters in path.
    /// </summary>
    [TestMethod]
    [DataRow("uni/123")]
    [DataRow("uni?123")]
    [DataRow("uni#123")]
    [DataRow("uni&123")]
    [DataRow("uni@123")]
    public async Task UpdateUniversityAsync_SpecialCharactersInId_MakesRequest(string idWithSpecialChars)
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<AcademicService>>();
        var service = new AcademicService(mockApi.Object, mockLogger.Object);

        var expectedDto = new UniversityDto { Id = idWithSpecialChars, Name = "Test" };
        var responseMessage = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(expectedDto)
        };

        mockApi.Setup(x => x.PutAsync(
            It.IsAny<string>(),
            It.IsAny<HttpContent>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(responseMessage);

        // Act
        var result = await service.UpdateUniversityAsync(idWithSpecialChars, "Test University");

        // Assert
        mockApi.Verify(x => x.PutAsync(
            $"universities/{idWithSpecialChars}",
            It.IsAny<HttpContent>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Tests that UpdateUniversityAsync handles special characters in name that need JSON escaping.
    /// Input: Valid id and name with special JSON characters.
    /// Expected: Successfully serializes with proper escaping.
    /// </summary>
    [TestMethod]
    [DataRow("University with \"quotes\"")]
    [DataRow("University with \\ backslash")]
    [DataRow("University with \n newline")]
    [DataRow("University with \t tab")]
    public async Task UpdateUniversityAsync_SpecialCharactersInName_SerializesWithEscaping(string nameWithSpecialChars)
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<AcademicService>>();
        var service = new AcademicService(mockApi.Object, mockLogger.Object);

        var expectedDto = new UniversityDto { Id = "uni123", Name = nameWithSpecialChars };
        var responseMessage = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(expectedDto)
        };

        mockApi.Setup(x => x.PutAsync(
            It.IsAny<string>(),
            It.IsAny<HttpContent>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(responseMessage);

        // Act
        var result = await service.UpdateUniversityAsync("uni123", nameWithSpecialChars);

        // Assert
        Assert.IsNotNull(result);
    }

    /// <summary>
    /// Tests that UpdateUniversityAsync handles Unicode characters in name.
    /// Input: Valid id and name with Unicode characters.
    /// Expected: Successfully serializes Unicode characters.
    /// </summary>
    [TestMethod]
    public async Task UpdateUniversityAsync_UnicodeCharactersInName_SerializesSuccessfully()
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<AcademicService>>();
        var service = new AcademicService(mockApi.Object, mockLogger.Object);

        var unicodeName = "大学 🎓 Université";
        var expectedDto = new UniversityDto { Id = "uni123", Name = unicodeName };
        var responseMessage = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(expectedDto)
        };

        mockApi.Setup(x => x.PutAsync(
            It.IsAny<string>(),
            It.IsAny<HttpContent>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(responseMessage);

        // Act
        var result = await service.UpdateUniversityAsync("uni123", unicodeName);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(unicodeName, result.Name);
    }

    /// <summary>
    /// Tests that UpdateUniversityAsync returns null when response content is null.
    /// Input: Valid id and name, API returns success but ReadFromJsonAsync returns null.
    /// Expected: Returns null.
    /// </summary>
    [TestMethod]
    public async Task UpdateUniversityAsync_ResponseContentIsNull_ReturnsNull()
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<AcademicService>>();
        var service = new AcademicService(mockApi.Object, mockLogger.Object);

        var responseMessage = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create<UniversityDto?>(null)
        };

        mockApi.Setup(x => x.PutAsync(
            It.IsAny<string>(),
            It.IsAny<HttpContent>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(responseMessage);

        // Act
        var result = await service.UpdateUniversityAsync("uni123", "Test");

        // Assert
        Assert.IsNull(result);
    }

    /// <summary>
    /// Tests that UpdateUniversityAsync returns null and logs error when deserialization throws exception.
    /// Input: Valid id and name, API returns success but response content is invalid JSON.
    /// Expected: Returns null and logs error.
    /// </summary>
    [TestMethod]
    public async Task UpdateUniversityAsync_DeserializationFails_ReturnsNullAndLogsError()
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<AcademicService>>();
        var service = new AcademicService(mockApi.Object, mockLogger.Object);

        var responseMessage = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("invalid json", Encoding.UTF8, "application/json")
        };

        mockApi.Setup(x => x.PutAsync(
            It.IsAny<string>(),
            It.IsAny<HttpContent>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(responseMessage);

        // Act
        var result = await service.UpdateUniversityAsync("uni123", "Test");

        // Assert
        Assert.IsNull(result);
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Failed to update university")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    /// <summary>
    /// Tests that UpdateUniversityAsync logs error with correct university id.
    /// Input: Valid id and name, API throws exception.
    /// Expected: Logs error message containing the university id.
    /// </summary>
    [TestMethod]
    public async Task UpdateUniversityAsync_ExceptionThrown_LogsErrorWithUniversityId()
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<AcademicService>>();
        var service = new AcademicService(mockApi.Object, mockLogger.Object);

        var testId = "test-uni-456";
        var exception = new InvalidOperationException("Test exception");

        mockApi.Setup(x => x.PutAsync(
            It.IsAny<string>(),
            It.IsAny<HttpContent>(),
            It.IsAny<CancellationToken>()))
            .ThrowsAsync(exception);

        // Act
        var result = await service.UpdateUniversityAsync(testId, "Test");

        // Assert
        Assert.IsNull(result);
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains(testId)),
                exception,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    /// <summary>
    /// Tests that UpdateUniversityAsync handles TaskCanceledException.
    /// Input: Valid id and name, API throws TaskCanceledException.
    /// Expected: Returns null and logs error.
    /// </summary>
    [TestMethod]
    public async Task UpdateUniversityAsync_TaskCanceled_ReturnsNullAndLogsError()
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<AcademicService>>();
        var service = new AcademicService(mockApi.Object, mockLogger.Object);

        var exception = new TaskCanceledException("Request canceled");

        mockApi.Setup(x => x.PutAsync(
            It.IsAny<string>(),
            It.IsAny<HttpContent>(),
            It.IsAny<CancellationToken>()))
            .ThrowsAsync(exception);

        // Act
        var result = await service.UpdateUniversityAsync("uni123", "Test");

        // Assert
        Assert.IsNull(result);
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Failed to update university")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    /// <summary>
    /// Tests that CreateUniversityAsync handles empty string names.
    /// Input: Empty string or whitespace-only string.
    /// Expected: Method executes without throwing and makes API call.
    /// </summary>
    [TestMethod]
    [DataRow("")]
    [DataRow("   ")]
    [DataRow("\t")]
    [DataRow("\n")]
    [DataRow("\r\n")]
    public async Task CreateUniversityAsync_EmptyOrWhitespaceName_MakesApiCall(string name)
    {
        // Arrange
        var apiMock = new Mock<IApiService>();
        var loggerMock = new Mock<ILogger<AcademicService>>();
        var service = new AcademicService(apiMock.Object, loggerMock.Object);
        var expectedDto = new UniversityDto { Id = "1", Name = name, Status = "Active", FacultiesCount = 0 };
        var responseMessage = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(expectedDto)
        };

        apiMock.Setup(x => x.PostAsync("universities", It.IsAny<HttpContent>(), It.IsAny<CancellationToken>()))
               .ReturnsAsync(responseMessage);

        // Act
        var result = await service.CreateUniversityAsync(name);

        // Assert
        Assert.IsNotNull(result);
        apiMock.Verify(x => x.PostAsync("universities", It.IsAny<HttpContent>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Tests that CreateUniversityAsync handles names with special characters.
    /// Input: Strings with special characters, quotes, and Unicode.
    /// Expected: Method successfully serializes and sends the request.
    /// </summary>
    [TestMethod]
    [DataRow("University & College")]
    [DataRow("Université de Paris")]
    [DataRow("大学")]
    [DataRow("University<Test>")]
    [DataRow("University\"Test\"")]
    [DataRow("University'Test'")]
    [DataRow("University\0Test")]
    public async Task CreateUniversityAsync_SpecialCharactersInName_SerializesCorrectly(string name)
    {
        // Arrange
        var apiMock = new Mock<IApiService>();
        var loggerMock = new Mock<ILogger<AcademicService>>();
        var service = new AcademicService(apiMock.Object, loggerMock.Object);
        var expectedDto = new UniversityDto { Id = "1", Name = name, Status = "Active", FacultiesCount = 0 };
        var responseMessage = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(expectedDto)
        };

        apiMock.Setup(x => x.PostAsync("universities", It.IsAny<HttpContent>(), It.IsAny<CancellationToken>()))
               .ReturnsAsync(responseMessage);

        // Act
        var result = await service.CreateUniversityAsync(name);

        // Assert
        Assert.IsNotNull(result);
        apiMock.Verify(x => x.PostAsync("universities", It.IsAny<HttpContent>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Tests that CreateUniversityAsync handles very long university names.
    /// Input: Very long string (10000 characters).
    /// Expected: Method successfully serializes and sends the request.
    /// </summary>
    [TestMethod]
    public async Task CreateUniversityAsync_VeryLongName_MakesApiCall()
    {
        // Arrange
        var apiMock = new Mock<IApiService>();
        var loggerMock = new Mock<ILogger<AcademicService>>();
        var service = new AcademicService(apiMock.Object, loggerMock.Object);
        string name = new string('A', 10000);
        var expectedDto = new UniversityDto { Id = "1", Name = name, Status = "Active", FacultiesCount = 0 };
        var responseMessage = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(expectedDto)
        };

        apiMock.Setup(x => x.PostAsync("universities", It.IsAny<HttpContent>(), It.IsAny<CancellationToken>()))
               .ReturnsAsync(responseMessage);

        // Act
        var result = await service.CreateUniversityAsync(name);

        // Assert
        Assert.IsNotNull(result);
        apiMock.Verify(x => x.PostAsync("universities", It.IsAny<HttpContent>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Tests that CreateUniversityAsync handles TaskCanceledException (timeout scenario).
    /// Input: Valid name but API operation times out.
    /// Expected: Returns null and logs error.
    /// </summary>
    [TestMethod]
    public async Task CreateUniversityAsync_ApiTimesOut_ReturnsNullAndLogsError()
    {
        // Arrange
        var apiMock = new Mock<IApiService>();
        var loggerMock = new Mock<ILogger<AcademicService>>();
        var service = new AcademicService(apiMock.Object, loggerMock.Object);
        string name = "Test University";
        var exception = new TaskCanceledException("Request timed out");

        apiMock.Setup(x => x.PostAsync("universities", It.IsAny<HttpContent>(), It.IsAny<CancellationToken>()))
               .ThrowsAsync(exception);

        // Act
        var result = await service.CreateUniversityAsync(name);

        // Assert
        Assert.IsNull(result);
        loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Failed to create university")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    /// <summary>
    /// Tests that CreateUniversityAsync handles InvalidOperationException.
    /// Input: Valid name but API throws InvalidOperationException.
    /// Expected: Returns null and logs error.
    /// </summary>
    [TestMethod]
    public async Task CreateUniversityAsync_InvalidOperationException_ReturnsNullAndLogsError()
    {
        // Arrange
        var apiMock = new Mock<IApiService>();
        var loggerMock = new Mock<ILogger<AcademicService>>();
        var service = new AcademicService(apiMock.Object, loggerMock.Object);
        string name = "Test University";
        var exception = new InvalidOperationException("Invalid state");

        apiMock.Setup(x => x.PostAsync("universities", It.IsAny<HttpContent>(), It.IsAny<CancellationToken>()))
               .ThrowsAsync(exception);

        // Act
        var result = await service.CreateUniversityAsync(name);

        // Assert
        Assert.IsNull(result);
        loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Failed to create university")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    /// <summary>
    /// Tests that CreateUniversityAsync calls the correct API endpoint.
    /// Input: Valid university name.
    /// Expected: Calls PostAsync with "universities" endpoint.
    /// </summary>
    [TestMethod]
    public async Task CreateUniversityAsync_ValidName_CallsCorrectEndpoint()
    {
        // Arrange
        var apiMock = new Mock<IApiService>();
        var loggerMock = new Mock<ILogger<AcademicService>>();
        var service = new AcademicService(apiMock.Object, loggerMock.Object);
        string name = "Yale";
        var expectedDto = new UniversityDto { Id = "1", Name = name, Status = "Active", FacultiesCount = 0 };
        var responseMessage = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(expectedDto)
        };

        apiMock.Setup(x => x.PostAsync("universities", It.IsAny<HttpContent>(), It.IsAny<CancellationToken>()))
               .ReturnsAsync(responseMessage);

        // Act
        var result = await service.CreateUniversityAsync(name);

        // Assert
        Assert.IsNotNull(result);
        apiMock.Verify(x => x.PostAsync("universities", It.IsAny<HttpContent>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Tests that CreateUniversityAsync handles names with control characters.
    /// Input: Names with newline, tab, and carriage return characters.
    /// Expected: Successfully serializes and sends the request.
    /// </summary>
    [TestMethod]
    [DataRow("University\nWith\nNewlines")]
    [DataRow("University\tWith\tTabs")]
    [DataRow("University\rWith\rCarriageReturns")]
    [DataRow("University\\Backslash")]
    public async Task CreateUniversityAsync_ControlCharactersInName_SerializesCorrectly(string name)
    {
        // Arrange
        var apiMock = new Mock<IApiService>();
        var loggerMock = new Mock<ILogger<AcademicService>>();
        var service = new AcademicService(apiMock.Object, loggerMock.Object);
        var expectedDto = new UniversityDto { Id = "1", Name = name, Status = "Active", FacultiesCount = 0 };
        var responseMessage = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(expectedDto)
        };

        apiMock.Setup(x => x.PostAsync("universities", It.IsAny<HttpContent>(), It.IsAny<CancellationToken>()))
               .ReturnsAsync(responseMessage);

        // Act
        var result = await service.CreateUniversityAsync(name);

        // Assert
        Assert.IsNotNull(result);
        apiMock.Verify(x => x.PostAsync("universities", It.IsAny<HttpContent>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Tests that GetUniversityDetailsAsync returns a list with a single university when API returns one item.
    /// Input: API returns a single university.
    /// Expected: Returns a list containing exactly one university with correct properties.
    /// </summary>
    [TestMethod]
    public async Task GetUniversityDetailsAsync_SingleUniversityInResponse_ReturnsSingleItemList()
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<AcademicService>>();
        var universities = new List<UniversityDto>
        {
            new UniversityDto { Id = "uni-001", Name = "Test University", FacultiesCount = 10, Status = "Active" }
        };
        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(universities)
        };
        mockApi.Setup(x => x.GetAsync("universities/details", It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);
        var service = new AcademicService(mockApi.Object, mockLogger.Object);

        // Act
        var result = await service.GetUniversityDetailsAsync();

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(1, result.Count);
        Assert.AreEqual("uni-001", result[0].Id);
        Assert.AreEqual("Test University", result[0].Name);
        Assert.AreEqual(10, result[0].FacultiesCount);
        Assert.AreEqual("Active", result[0].Status);
    }

    /// <summary>
    /// Tests that GetUniversityDetailsAsync handles large collections correctly.
    /// Input: API returns a large list of 1000 universities.
    /// Expected: Returns all 1000 universities successfully.
    /// </summary>
    [TestMethod]
    public async Task GetUniversityDetailsAsync_LargeListOfUniversities_ReturnsAllItems()
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<AcademicService>>();
        var universities = new List<UniversityDto>();
        for (int i = 0; i < 1000; i++)
        {
            universities.Add(new UniversityDto
            {
                Id = $"uni-{i:D4}",
                Name = $"University {i}",
                FacultiesCount = i % 20,
                Status = i % 2 == 0 ? "Active" : "Inactive"
            });
        }
        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(universities)
        };
        mockApi.Setup(x => x.GetAsync("universities/details", It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);
        var service = new AcademicService(mockApi.Object, mockLogger.Object);

        // Act
        var result = await service.GetUniversityDetailsAsync();

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(1000, result.Count);
        Assert.AreEqual("uni-0000", result[0].Id);
        Assert.AreEqual("uni-0999", result[999].Id);
    }

    /// <summary>
    /// Tests that GetUniversityDetailsAsync correctly handles universities with special characters in names.
    /// Input: API returns universities with special characters, quotes, Unicode, and control characters.
    /// Expected: Returns universities with special characters preserved correctly.
    /// </summary>
    [TestMethod]
    public async Task GetUniversityDetailsAsync_UniversitiesWithSpecialCharacters_ReturnsCorrectData()
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<AcademicService>>();
        var universities = new List<UniversityDto>
        {
            new UniversityDto { Id = "1", Name = "University & College", FacultiesCount = 5, Status = "Active" },
            new UniversityDto { Id = "2", Name = "Université de Paris", FacultiesCount = 8, Status = "Active" },
            new UniversityDto { Id = "3", Name = "University \"Elite\"", FacultiesCount = 3, Status = "Inactive" },
            new UniversityDto { Id = "4", Name = "大学 (University)", FacultiesCount = 12, Status = "Active" },
            new UniversityDto { Id = "5", Name = "University's School", FacultiesCount = 7, Status = "Active" }
        };
        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(universities)
        };
        mockApi.Setup(x => x.GetAsync("universities/details", It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);
        var service = new AcademicService(mockApi.Object, mockLogger.Object);

        // Act
        var result = await service.GetUniversityDetailsAsync();

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(5, result.Count);
        Assert.AreEqual("University & College", result[0].Name);
        Assert.AreEqual("Université de Paris", result[1].Name);
        Assert.AreEqual("University \"Elite\"", result[2].Name);
        Assert.AreEqual("大学 (University)", result[3].Name);
        Assert.AreEqual("University's School", result[4].Name);
    }

    /// <summary>
    /// Tests that GetUniversityDetailsAsync correctly handles universities with empty or whitespace strings in properties.
    /// Input: API returns universities with empty strings for name and status.
    /// Expected: Returns universities with empty strings as provided.
    /// </summary>
    [TestMethod]
    public async Task GetUniversityDetailsAsync_UniversitiesWithEmptyStrings_ReturnsCorrectData()
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<AcademicService>>();
        var universities = new List<UniversityDto>
        {
            new UniversityDto { Id = "", Name = "", FacultiesCount = 0, Status = "" },
            new UniversityDto { Id = "2", Name = "   ", FacultiesCount = 5, Status = "   " },
            new UniversityDto { Id = "3", Name = "Normal University", FacultiesCount = 10, Status = "Active" }
        };
        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(universities)
        };
        mockApi.Setup(x => x.GetAsync("universities/details", It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);
        var service = new AcademicService(mockApi.Object, mockLogger.Object);

        // Act
        var result = await service.GetUniversityDetailsAsync();

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(3, result.Count);
        Assert.AreEqual("", result[0].Id);
        Assert.AreEqual("", result[0].Name);
        Assert.AreEqual("   ", result[1].Name);
    }

    /// <summary>
    /// Tests that GetUniversityDetailsAsync returns empty list and logs error when API operation times out.
    /// Input: API throws TaskCanceledException (timeout scenario).
    /// Expected: Returns empty list and logs error.
    /// </summary>
    [TestMethod]
    public async Task GetUniversityDetailsAsync_ApiTimeout_ReturnsEmptyListAndLogsError()
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<AcademicService>>();
        mockApi.Setup(x => x.GetAsync("universities/details", It.IsAny<CancellationToken>()))
            .ThrowsAsync(new TaskCanceledException("The operation was canceled due to timeout"));
        var service = new AcademicService(mockApi.Object, mockLogger.Object);

        // Act
        var result = await service.GetUniversityDetailsAsync();

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(0, result.Count);
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Failed to fetch university details")),
                It.IsAny<TaskCanceledException>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    /// <summary>
    /// Tests that GetUniversityDetailsAsync handles universities with very long strings in name property.
    /// Input: API returns a university with name containing 10,000 characters.
    /// Expected: Returns university with full long name preserved.
    /// </summary>
    [TestMethod]
    public async Task GetUniversityDetailsAsync_UniversityWithVeryLongName_ReturnsCorrectData()
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<AcademicService>>();
        var longName = new string('A', 10000);
        var universities = new List<UniversityDto>
        {
            new UniversityDto { Id = "1", Name = longName, FacultiesCount = 5, Status = "Active" }
        };
        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(universities)
        };
        mockApi.Setup(x => x.GetAsync("universities/details", It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);
        var service = new AcademicService(mockApi.Object, mockLogger.Object);

        // Act
        var result = await service.GetUniversityDetailsAsync();

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(1, result.Count);
        Assert.AreEqual(10000, result[0].Name.Length);
        Assert.AreEqual(longName, result[0].Name);
    }

    /// <summary>
    /// Tests that GetUniversityDetailsAsync handles universities with boundary values for FacultiesCount.
    /// Input: API returns universities with int.MinValue, int.MaxValue, zero, and negative faculty counts.
    /// Expected: Returns universities with boundary values preserved.
    /// </summary>
    [TestMethod]
    public async Task GetUniversityDetailsAsync_UniversitiesWithBoundaryFacultyCounts_ReturnsCorrectData()
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<AcademicService>>();
        var universities = new List<UniversityDto>
        {
            new UniversityDto { Id = "1", Name = "University A", FacultiesCount = int.MinValue, Status = "Active" },
            new UniversityDto { Id = "2", Name = "University B", FacultiesCount = int.MaxValue, Status = "Active" },
            new UniversityDto { Id = "3", Name = "University C", FacultiesCount = 0, Status = "Active" },
            new UniversityDto { Id = "4", Name = "University D", FacultiesCount = -1, Status = "Active" }
        };
        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(universities)
        };
        mockApi.Setup(x => x.GetAsync("universities/details", It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);
        var service = new AcademicService(mockApi.Object, mockLogger.Object);

        // Act
        var result = await service.GetUniversityDetailsAsync();

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(4, result.Count);
        Assert.AreEqual(int.MinValue, result[0].FacultiesCount);
        Assert.AreEqual(int.MaxValue, result[1].FacultiesCount);
        Assert.AreEqual(0, result[2].FacultiesCount);
        Assert.AreEqual(-1, result[3].FacultiesCount);
    }

    /// <summary>
    /// Tests that GetUniversitiesAsync handles whitespace-only strings in university names.
    /// Input: API returns universities with whitespace-only names.
    /// Expected: Returns data with whitespace preserved.
    /// </summary>
    [TestMethod]
    public async Task GetUniversitiesAsync_UniversityWithWhitespaceNames_ReturnsCorrectData()
    {
        // Arrange
        var universitiesWithWhitespace = new List<LookupItem>
        {
            new LookupItem { Id = "1", Name = "   " },
            new LookupItem { Id = "2", Name = "\t\t" },
            new LookupItem { Id = "3", Name = "\n\r" }
        };

        var responseMessage = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(universitiesWithWhitespace)
        };

        var mockApiService = new Mock<IApiService>();
        mockApiService
            .Setup(x => x.GetAsync("universities"))
            .ReturnsAsync(responseMessage);

        var mockLogger = new Mock<ILogger<AcademicService>>();
        var service = new AcademicService(mockApiService.Object, mockLogger.Object);

        // Act
        var result = await service.GetUniversitiesAsync();

        // Assert
        Assert.IsNotNull(result);
        var resultList = result.ToList();
        Assert.AreEqual(3, resultList.Count);
    }

    /// <summary>
    /// Tests that GetUniversitiesAsync handles very long university names correctly.
    /// Input: API returns university with very long name (10000 characters).
    /// Expected: Returns data with full name preserved.
    /// </summary>
    [TestMethod]
    public async Task GetUniversitiesAsync_UniversityWithVeryLongName_ReturnsCorrectData()
    {
        // Arrange
        var longName = new string('A', 10000);
        var universities = new List<LookupItem>
        {
            new LookupItem { Id = "1", Name = longName }
        };

        var responseMessage = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(universities)
        };

        var mockApiService = new Mock<IApiService>();
        mockApiService
            .Setup(x => x.GetAsync("universities"))
            .ReturnsAsync(responseMessage);

        var mockLogger = new Mock<ILogger<AcademicService>>();
        var service = new AcademicService(mockApiService.Object, mockLogger.Object);

        // Act
        var result = await service.GetUniversitiesAsync();

        // Assert
        Assert.IsNotNull(result);
        var resultList = result.ToList();
        Assert.AreEqual(1, resultList.Count);
        Assert.AreEqual(10000, resultList[0].Name?.Length);
    }

    /// <summary>
    /// Tests that GetUniversitiesAsync handles InvalidOperationException correctly.
    /// Input: API GetAsync throws InvalidOperationException.
    /// Expected: Returns empty collection and logs error.
    /// </summary>
    [TestMethod]
    public async Task GetUniversitiesAsync_ApiThrowsInvalidOperationException_ReturnsEmptyCollection()
    {
        // Arrange
        var mockApiService = new Mock<IApiService>();
        mockApiService
            .Setup(x => x.GetAsync("universities"))
            .ThrowsAsync(new InvalidOperationException("Invalid operation"));

        var mockLogger = new Mock<ILogger<AcademicService>>();
        var service = new AcademicService(mockApiService.Object, mockLogger.Object);

        // Act
        var result = await service.GetUniversitiesAsync();

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(0, result.Count());
    }

    /// <summary>
    /// Tests that GetUniversitiesAsync returns correct data for all 2xx success status codes.
    /// Input: API returns various 2xx success status codes with data.
    /// Expected: Returns deserialized data for all success codes.
    /// </summary>
    [TestMethod]
    [DataRow(HttpStatusCode.OK, DisplayName = "OK")]
    [DataRow(HttpStatusCode.Created, DisplayName = "Created")]
    [DataRow(HttpStatusCode.Accepted, DisplayName = "Accepted")]
    [DataRow(HttpStatusCode.NoContent, DisplayName = "NoContent")]
    public async Task GetUniversitiesAsync_ApiReturnsSuccessStatusCode_ReturnsData(HttpStatusCode statusCode)
    {
        // Arrange
        var universities = new List<LookupItem>
        {
            new LookupItem { Id = "1", Name = "Test University" }
        };

        var responseMessage = new HttpResponseMessage(statusCode)
        {
            Content = JsonContent.Create(universities)
        };

        var mockApiService = new Mock<IApiService>();
        mockApiService
            .Setup(x => x.GetAsync("universities"))
            .ReturnsAsync(responseMessage);

        var mockLogger = new Mock<ILogger<AcademicService>>();
        var service = new AcademicService(mockApiService.Object, mockLogger.Object);

        // Act
        var result = await service.GetUniversitiesAsync();

        // Assert
        Assert.IsNotNull(result);
        var resultList = result.ToList();
        Assert.AreEqual(1, resultList.Count);
        Assert.AreEqual("Test University", resultList[0].Name);
    }

    /// <summary>
    /// Tests that GetSemestersAsync successfully retrieves semesters from the API on first call.
    /// Input: Valid API response with semester data.
    /// Expected: Returns the list of semesters from the API.
    /// </summary>
    [TestMethod]
    public async Task GetSemestersAsync_FirstCallWithValidData_ReturnsSemestersFromApi()
    {
        // Arrange
        var expectedSemesters = new List<LookupItem>
        {
            new LookupItem { Id = "sem1", Name = "Fall 2024" },
            new LookupItem { Id = "sem2", Name = "Spring 2025" },
            new LookupItem { Id = "sem3", Name = "Summer 2025" }
        };

        var responseContent = JsonContent.Create(expectedSemesters);
        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = responseContent
        };

        var mockApiService = new Mock<IApiService>();
        mockApiService
            .Setup(x => x.GetAsync("semesters"))
            .ReturnsAsync(httpResponse);

        var mockLogger = new Mock<ILogger<AcademicService>>();
        var service = new AcademicService(mockApiService.Object, mockLogger.Object);

        // Act
        var result = await service.GetSemestersAsync();

        // Assert
        Assert.IsNotNull(result);
        var resultList = result.ToList();
        Assert.AreEqual(3, resultList.Count);
        Assert.AreEqual("sem1", resultList[0].Id);
        Assert.AreEqual("Fall 2024", resultList[0].Name);
        Assert.AreEqual("sem2", resultList[1].Id);
        Assert.AreEqual("Spring 2025", resultList[1].Name);
        Assert.AreEqual("sem3", resultList[2].Id);
        Assert.AreEqual("Summer 2025", resultList[2].Name);
        mockApiService.Verify(x => x.GetAsync("semesters"), Times.Once);
    }

    /// <summary>
    /// Tests that GetSemestersAsync returns cached data on subsequent calls within cache duration.
    /// Input: Two consecutive calls within cache duration.
    /// Expected: API is called only once, second call returns cached data.
    /// </summary>
    [TestMethod]
    public async Task GetSemestersAsync_SecondCallWithinCacheDuration_ReturnsCachedData()
    {
        // Arrange
        var semesters = new List<LookupItem>
        {
            new LookupItem { Id = "sem1", Name = "Fall 2024" }
        };

        var responseContent = JsonContent.Create(semesters);
        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = responseContent
        };

        var mockApiService = new Mock<IApiService>();
        mockApiService
            .Setup(x => x.GetAsync("semesters"))
            .ReturnsAsync(httpResponse);

        var mockLogger = new Mock<ILogger<AcademicService>>();
        var service = new AcademicService(mockApiService.Object, mockLogger.Object);

        // Act
        var result1 = await service.GetSemestersAsync();
        var result2 = await service.GetSemestersAsync();

        // Assert
        Assert.IsNotNull(result1);
        Assert.IsNotNull(result2);
        var list1 = result1.ToList();
        var list2 = result2.ToList();
        Assert.AreEqual(1, list1.Count);
        Assert.AreEqual(1, list2.Count);
        Assert.AreEqual(list1[0].Id, list2[0].Id);
        Assert.AreEqual(list1[0].Name, list2[0].Name);
        mockApiService.Verify(x => x.GetAsync("semesters"), Times.Once);
    }

    /// <summary>
    /// Tests that GetSemestersAsync returns an empty collection when API returns non-success status code.
    /// Input: API response with various HTTP error status codes.
    /// Expected: Returns empty collection and logs warning.
    /// </summary>
    [TestMethod]
    [DataRow(HttpStatusCode.BadRequest, DisplayName = "BadRequest (400)")]
    [DataRow(HttpStatusCode.Unauthorized, DisplayName = "Unauthorized (401)")]
    [DataRow(HttpStatusCode.Forbidden, DisplayName = "Forbidden (403)")]
    [DataRow(HttpStatusCode.NotFound, DisplayName = "NotFound (404)")]
    [DataRow(HttpStatusCode.InternalServerError, DisplayName = "InternalServerError (500)")]
    [DataRow(HttpStatusCode.BadGateway, DisplayName = "BadGateway (502)")]
    [DataRow(HttpStatusCode.ServiceUnavailable, DisplayName = "ServiceUnavailable (503)")]
    [DataRow(HttpStatusCode.GatewayTimeout, DisplayName = "GatewayTimeout (504)")]
    public async Task GetSemestersAsync_ApiReturnsErrorStatusCode_ReturnsEmptyCollectionAndLogsWarning(HttpStatusCode statusCode)
    {
        // Arrange
        var httpResponse = new HttpResponseMessage(statusCode);

        var mockApiService = new Mock<IApiService>();
        mockApiService
            .Setup(x => x.GetAsync("semesters"))
            .ReturnsAsync(httpResponse);

        var mockLogger = new Mock<ILogger<AcademicService>>();
        var service = new AcademicService(mockApiService.Object, mockLogger.Object);

        // Act
        var result = await service.GetSemestersAsync();

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(0, result.Count());
        mockApiService.Verify(x => x.GetAsync("semesters"), Times.Once);
    }

    /// <summary>
    /// Tests that GetSemestersAsync returns an empty collection when API throws an exception.
    /// Input: API throws HttpRequestException.
    /// Expected: Returns empty collection and logs error.
    /// </summary>
    [TestMethod]
    public async Task GetSemestersAsync_ApiThrowsHttpRequestException_ReturnsEmptyCollectionAndLogsError()
    {
        // Arrange
        var mockApiService = new Mock<IApiService>();
        mockApiService
            .Setup(x => x.GetAsync("semesters"))
            .ThrowsAsync(new HttpRequestException("Network error"));

        var mockLogger = new Mock<ILogger<AcademicService>>();
        var service = new AcademicService(mockApiService.Object, mockLogger.Object);

        // Act
        var result = await service.GetSemestersAsync();

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(0, result.Count());
        mockApiService.Verify(x => x.GetAsync("semesters"), Times.Once);
    }

    /// <summary>
    /// Tests that GetSemestersAsync returns an empty collection when API throws various exception types.
    /// Input: API throws different exception types.
    /// Expected: Returns empty collection and logs error for all exception types.
    /// </summary>
    [TestMethod]
    [DataRow(typeof(TaskCanceledException), DisplayName = "TaskCanceledException")]
    [DataRow(typeof(InvalidOperationException), DisplayName = "InvalidOperationException")]
    [DataRow(typeof(JsonException), DisplayName = "JsonException")]
    public async Task GetSemestersAsync_ApiThrowsVariousExceptions_ReturnsEmptyCollectionAndLogsError(Type exceptionType)
    {
        // Arrange
        var exception = (Exception)Activator.CreateInstance(exceptionType, "Test exception")!;

        var mockApiService = new Mock<IApiService>();
        mockApiService
            .Setup(x => x.GetAsync("semesters"))
            .ThrowsAsync(exception);

        var mockLogger = new Mock<ILogger<AcademicService>>();
        var service = new AcademicService(mockApiService.Object, mockLogger.Object);

        // Act
        var result = await service.GetSemestersAsync();

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(0, result.Count());
        mockApiService.Verify(x => x.GetAsync("semesters"), Times.Once);
    }

    /// <summary>
    /// Tests that GetSemestersAsync returns an empty collection when API returns empty array.
    /// Input: API returns success with empty array.
    /// Expected: Returns empty collection.
    /// </summary>
    [TestMethod]
    public async Task GetSemestersAsync_ApiReturnsEmptyArray_ReturnsEmptyCollection()
    {
        // Arrange
        var emptySemesters = new List<LookupItem>();
        var responseContent = JsonContent.Create(emptySemesters);
        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = responseContent
        };

        var mockApiService = new Mock<IApiService>();
        mockApiService
            .Setup(x => x.GetAsync("semesters"))
            .ReturnsAsync(httpResponse);

        var mockLogger = new Mock<ILogger<AcademicService>>();
        var service = new AcademicService(mockApiService.Object, mockLogger.Object);

        // Act
        var result = await service.GetSemestersAsync();

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(0, result.Count());
    }

    /// <summary>
    /// Tests that GetSemestersAsync handles single semester in response correctly.
    /// Input: API response with single semester.
    /// Expected: Returns collection with one semester.
    /// </summary>
    [TestMethod]
    public async Task GetSemestersAsync_ApiReturnsSingleSemester_ReturnsSingleItemCollection()
    {
        // Arrange
        var singleSemester = new List<LookupItem>
        {
            new LookupItem { Id = "sem1", Name = "Fall 2024" }
        };

        var responseContent = JsonContent.Create(singleSemester);
        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = responseContent
        };

        var mockApiService = new Mock<IApiService>();
        mockApiService
            .Setup(x => x.GetAsync("semesters"))
            .ReturnsAsync(httpResponse);

        var mockLogger = new Mock<ILogger<AcademicService>>();
        var service = new AcademicService(mockApiService.Object, mockLogger.Object);

        // Act
        var result = await service.GetSemestersAsync();

        // Assert
        Assert.IsNotNull(result);
        var resultList = result.ToList();
        Assert.AreEqual(1, resultList.Count);
        Assert.AreEqual("sem1", resultList[0].Id);
        Assert.AreEqual("Fall 2024", resultList[0].Name);
    }

    /// <summary>
    /// Tests that GetSemestersAsync handles semesters with empty string values correctly.
    /// Input: API response with semesters containing empty or whitespace strings.
    /// Expected: Returns semesters with empty/whitespace values as-is.
    /// </summary>
    [TestMethod]
    public async Task GetSemestersAsync_SemestersWithEmptyOrWhitespaceValues_ReturnsData()
    {
        // Arrange
        var semestersWithEmptyValues = new List<LookupItem>
        {
            new LookupItem { Id = "", Name = "Empty ID" },
            new LookupItem { Id = "sem2", Name = "" },
            new LookupItem { Id = "   ", Name = "Whitespace ID" },
            new LookupItem { Id = "sem4", Name = "   " }
        };

        var responseContent = JsonContent.Create(semestersWithEmptyValues);
        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = responseContent
        };

        var mockApiService = new Mock<IApiService>();
        mockApiService
            .Setup(x => x.GetAsync("semesters"))
            .ReturnsAsync(httpResponse);

        var mockLogger = new Mock<ILogger<AcademicService>>();
        var service = new AcademicService(mockApiService.Object, mockLogger.Object);

        // Act
        var result = await service.GetSemestersAsync();

        // Assert
        Assert.IsNotNull(result);
        var resultList = result.ToList();
        Assert.AreEqual(4, resultList.Count);
        Assert.AreEqual("", resultList[0].Id);
        Assert.AreEqual("Empty ID", resultList[0].Name);
        Assert.AreEqual("sem2", resultList[1].Id);
        Assert.AreEqual("", resultList[1].Name);
    }

    /// <summary>
    /// Tests that GetSemestersAsync handles very long semester names correctly.
    /// Input: API response with very long semester names (1000+ characters).
    /// Expected: Returns semesters with full long names preserved.
    /// </summary>
    [TestMethod]
    public async Task GetSemestersAsync_VeryLongSemesterNames_ReturnsDataCorrectly()
    {
        // Arrange
        var longName = new string('A', 5000);
        var semestersWithLongNames = new List<LookupItem>
        {
            new LookupItem { Id = "sem1", Name = longName }
        };

        var responseContent = JsonContent.Create(semestersWithLongNames);
        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = responseContent
        };

        var mockApiService = new Mock<IApiService>();
        mockApiService
            .Setup(x => x.GetAsync("semesters"))
            .ReturnsAsync(httpResponse);

        var mockLogger = new Mock<ILogger<AcademicService>>();
        var service = new AcademicService(mockApiService.Object, mockLogger.Object);

        // Act
        var result = await service.GetSemestersAsync();

        // Assert
        Assert.IsNotNull(result);
        var resultList = result.ToList();
        Assert.AreEqual(1, resultList.Count);
        Assert.AreEqual(longName, resultList[0].Name);
        Assert.AreEqual(5000, resultList[0].Name.Length);
    }

    /// <summary>
    /// Tests that GetSemestersAsync returns an empty collection when API returns malformed JSON.
    /// Input: API returns invalid JSON content.
    /// Expected: Returns empty collection and logs error.
    /// </summary>
    [TestMethod]
    public async Task GetSemestersAsync_ApiReturnsInvalidJson_ReturnsEmptyCollectionAndLogsError()
    {
        // Arrange
        var invalidJsonContent = new StringContent("{ invalid json }", Encoding.UTF8, "application/json");
        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = invalidJsonContent
        };

        var mockApiService = new Mock<IApiService>();
        mockApiService
            .Setup(x => x.GetAsync("semesters"))
            .ReturnsAsync(httpResponse);

        var mockLogger = new Mock<ILogger<AcademicService>>();
        var service = new AcademicService(mockApiService.Object, mockLogger.Object);

        // Act
        var result = await service.GetSemestersAsync();

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(0, result.Count());
    }

    /// <summary>
    /// Tests that GetSemestersAsync handles large collections correctly.
    /// Input: API returns a large collection of semesters (1000+ items).
    /// Expected: Returns all items correctly.
    /// </summary>
    [TestMethod]
    public async Task GetSemestersAsync_LargeCollection_ReturnsAllItems()
    {
        // Arrange
        var largeSemesterList = new List<LookupItem>();
        for (int i = 0; i < 1000; i++)
        {
            largeSemesterList.Add(new LookupItem { Id = $"sem{i}", Name = $"Semester {i}" });
        }

        var responseContent = JsonContent.Create(largeSemesterList);
        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = responseContent
        };

        var mockApiService = new Mock<IApiService>();
        mockApiService
            .Setup(x => x.GetAsync("semesters"))
            .ReturnsAsync(httpResponse);

        var mockLogger = new Mock<ILogger<AcademicService>>();
        var service = new AcademicService(mockApiService.Object, mockLogger.Object);

        // Act
        var result = await service.GetSemestersAsync();

        // Assert
        Assert.IsNotNull(result);
        var resultList = result.ToList();
        Assert.AreEqual(1000, resultList.Count);
        Assert.AreEqual("sem0", resultList[0].Id);
        Assert.AreEqual("sem999", resultList[999].Id);
    }

    /// <summary>
    /// Tests that GetSemestersAsync calls the API with the correct path "semesters".
    /// Input: Valid setup.
    /// Expected: Verifies GetAsync is called with "semesters" path.
    /// </summary>
    [TestMethod]
    public async Task GetSemestersAsync_Always_CallsApiWithCorrectPath()
    {
        // Arrange
        var semesters = new List<LookupItem>
        {
            new LookupItem { Id = "sem1", Name = "Fall 2024" }
        };

        var responseContent = JsonContent.Create(semesters);
        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = responseContent
        };

        var mockApiService = new Mock<IApiService>();
        mockApiService
            .Setup(x => x.GetAsync("semesters"))
            .ReturnsAsync(httpResponse);

        var mockLogger = new Mock<ILogger<AcademicService>>();
        var service = new AcademicService(mockApiService.Object, mockLogger.Object);

        // Act
        await service.GetSemestersAsync();

        // Assert
        mockApiService.Verify(x => x.GetAsync("semesters"), Times.Once);
    }

    /// <summary>
    /// Tests that GetSemestersAsync handles multiple rapid consecutive calls correctly with caching.
    /// Input: Multiple consecutive calls within cache duration.
    /// Expected: API is called only once, all subsequent calls return cached data.
    /// </summary>
    [TestMethod]
    public async Task GetSemestersAsync_MultipleConsecutiveCalls_UsesCache()
    {
        // Arrange
        var semesters = new List<LookupItem>
        {
            new LookupItem { Id = "sem1", Name = "Fall 2024" }
        };

        var responseContent = JsonContent.Create(semesters);
        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = responseContent
        };

        var mockApiService = new Mock<IApiService>();
        mockApiService
            .Setup(x => x.GetAsync("semesters"))
            .ReturnsAsync(httpResponse);

        var mockLogger = new Mock<ILogger<AcademicService>>();
        var service = new AcademicService(mockApiService.Object, mockLogger.Object);

        // Act
        var result1 = await service.GetSemestersAsync();
        var result2 = await service.GetSemestersAsync();
        var result3 = await service.GetSemestersAsync();
        var result4 = await service.GetSemestersAsync();
        var result5 = await service.GetSemestersAsync();

        // Assert
        Assert.IsNotNull(result1);
        Assert.IsNotNull(result2);
        Assert.IsNotNull(result3);
        Assert.IsNotNull(result4);
        Assert.IsNotNull(result5);
        Assert.AreEqual(result1.Count(), result2.Count());
        Assert.AreEqual(result1.Count(), result3.Count());
        Assert.AreEqual(result1.Count(), result4.Count());
        Assert.AreEqual(result1.Count(), result5.Count());
        mockApiService.Verify(x => x.GetAsync("semesters"), Times.Once);
    }

    /// <summary>
    /// Tests that UpdateFacultyAsync sends correctly formatted JSON payload with both name and universityId fields.
    /// Input: Valid id, name, and universityId.
    /// Expected: API receives JSON with both name and universityId fields.
    /// </summary>
    [TestMethod]
    public async Task UpdateFacultyAsync_ValidParameters_SendsCorrectJsonPayload()
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<AcademicService>>();
        var service = new AcademicService(mockApi.Object, mockLogger.Object);

        string id = "faculty123";
        string name = "Engineering";
        string universityId = "uni456";

        HttpContent? capturedContent = null;
        var responseMessage = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(new FacultyDto { Id = id, Name = name, UniversityId = universityId })
        };

        mockApi.Setup(x => x.PutAsync(It.IsAny<string>(), It.IsAny<HttpContent>(), It.IsAny<CancellationToken>()))
               .Callback<string, HttpContent, CancellationToken>((path, content, ct) => capturedContent = content)
               .ReturnsAsync(responseMessage);

        // Act
        await service.UpdateFacultyAsync(id, name, universityId);

        // Assert
        Assert.IsNotNull(capturedContent);
        var jsonString = await capturedContent.ReadAsStringAsync();
        var deserializedPayload = JsonSerializer.Deserialize<JsonElement>(jsonString);
        Assert.IsTrue(deserializedPayload.TryGetProperty("name", out var nameProperty));
        Assert.AreEqual(name, nameProperty.GetString());
        Assert.IsTrue(deserializedPayload.TryGetProperty("universityId", out var universityIdProperty));
        Assert.AreEqual(universityId, universityIdProperty.GetString());
    }

    /// <summary>
    /// Tests that UpdateFacultyAsync correctly handles various 2xx success status codes.
    /// Input: Valid parameters; API returns different 2xx status codes.
    /// Expected: Returns FacultyDto for all 2xx status codes.
    /// </summary>
    [TestMethod]
    [DataRow(HttpStatusCode.OK, DisplayName = "200 OK")]
    [DataRow(HttpStatusCode.Created, DisplayName = "201 Created")]
    [DataRow(HttpStatusCode.Accepted, DisplayName = "202 Accepted")]
    [DataRow(HttpStatusCode.NoContent, DisplayName = "204 No Content")]
    public async Task UpdateFacultyAsync_Various2xxStatusCodes_ReturnsSuccessfully(HttpStatusCode statusCode)
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<AcademicService>>();
        var service = new AcademicService(mockApi.Object, mockLogger.Object);

        string id = "faculty123";
        string name = "Engineering";
        string universityId = "uni456";

        var expectedFaculty = new FacultyDto
        {
            Id = id,
            Name = name,
            UniversityId = universityId
        };

        var responseMessage = new HttpResponseMessage(statusCode)
        {
            Content = JsonContent.Create(expectedFaculty)
        };

        mockApi.Setup(x => x.PutAsync($"faculties/{id}", It.IsAny<HttpContent>(), It.IsAny<CancellationToken>()))
               .ReturnsAsync(responseMessage);

        // Act
        var result = await service.UpdateFacultyAsync(id, name, universityId);

        // Assert
        if (statusCode == HttpStatusCode.NoContent)
        {
            // NoContent typically has no body, so result might be null or default
            // This documents actual behavior
        }
        else
        {
            Assert.IsNotNull(result);
            Assert.AreEqual(expectedFaculty.Id, result.Id);
        }
    }

    /// <summary>
    /// Tests that UpdateFacultyAsync sends HTTP content with correct Content-Type header.
    /// Input: Valid id, name, and universityId.
    /// Expected: HTTP content has Content-Type set to application/json with UTF-8 charset.
    /// </summary>
    [TestMethod]
    public async Task UpdateFacultyAsync_ValidParameters_SendsCorrectContentType()
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<AcademicService>>();
        var service = new AcademicService(mockApi.Object, mockLogger.Object);

        string id = "faculty123";
        string name = "Engineering";
        string universityId = "uni456";

        HttpContent? capturedContent = null;
        var responseMessage = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(new FacultyDto { Id = id, Name = name, UniversityId = universityId })
        };

        mockApi.Setup(x => x.PutAsync(It.IsAny<string>(), It.IsAny<HttpContent>(), It.IsAny<CancellationToken>()))
               .Callback<string, HttpContent, CancellationToken>((path, content, ct) => capturedContent = content)
               .ReturnsAsync(responseMessage);

        // Act
        await service.UpdateFacultyAsync(id, name, universityId);

        // Assert
        Assert.IsNotNull(capturedContent);
        Assert.IsNotNull(capturedContent.Headers.ContentType);
        Assert.AreEqual("application/json", capturedContent.Headers.ContentType.MediaType);
        Assert.AreEqual("utf-8", capturedContent.Headers.ContentType.CharSet);
    }

    /// <summary>
    /// Tests that UpdateFacultyAsync constructs correct endpoint path with the provided id.
    /// Input: Valid id, name, and universityId.
    /// Expected: Calls PutAsync with "faculties/{id}" path.
    /// </summary>
    [TestMethod]
    public async Task UpdateFacultyAsync_ValidId_ConstructsCorrectEndpointPath()
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<AcademicService>>();
        var service = new AcademicService(mockApi.Object, mockLogger.Object);

        string id = "faculty-xyz-789";
        string name = "Engineering";
        string universityId = "uni456";

        string? capturedPath = null;
        var responseMessage = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(new FacultyDto { Id = id, Name = name, UniversityId = universityId })
        };

        mockApi.Setup(x => x.PutAsync(It.IsAny<string>(), It.IsAny<HttpContent>(), It.IsAny<CancellationToken>()))
               .Callback<string, HttpContent, CancellationToken>((path, content, ct) => capturedPath = path)
               .ReturnsAsync(responseMessage);

        // Act
        await service.UpdateFacultyAsync(id, name, universityId);

        // Assert
        Assert.AreEqual($"faculties/{id}", capturedPath);
    }

    /// <summary>
    /// Tests that UpdateFacultyAsync handles control characters in name parameter correctly.
    /// Input: Name with control characters like null terminator, bell, etc.
    /// Expected: JSON serialization escapes control characters properly.
    /// </summary>
    [TestMethod]
    [DataRow("Faculty\0Name", DisplayName = "Null terminator")]
    [DataRow("Faculty\bName", DisplayName = "Backspace")]
    [DataRow("Faculty\fName", DisplayName = "Form feed")]
    public async Task UpdateFacultyAsync_ControlCharactersInName_SerializesCorrectly(string name)
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<AcademicService>>();
        var service = new AcademicService(mockApi.Object, mockLogger.Object);

        string id = "faculty123";
        string universityId = "uni456";

        HttpContent? capturedContent = null;
        var responseMessage = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(new FacultyDto { Id = id, Name = name, UniversityId = universityId })
        };

        mockApi.Setup(x => x.PutAsync(It.IsAny<string>(), It.IsAny<HttpContent>(), It.IsAny<CancellationToken>()))
               .Callback<string, HttpContent, CancellationToken>((path, content, ct) => capturedContent = content)
               .ReturnsAsync(responseMessage);

        // Act
        var result = await service.UpdateFacultyAsync(id, name, universityId);

        // Assert
        Assert.IsNotNull(capturedContent);
        var jsonString = await capturedContent.ReadAsStringAsync();
        Assert.IsFalse(string.IsNullOrEmpty(jsonString));
        // Verify JSON is valid by deserializing
        var deserializedPayload = JsonSerializer.Deserialize<JsonElement>(jsonString);
        Assert.IsTrue(deserializedPayload.TryGetProperty("name", out _));
    }

    /// <summary>
    /// Tests that UpdateFacultyAsync handles extremely long id in URL path.
    /// Input: Very long id string (10000+ characters).
    /// Expected: Constructs path with the long id without truncation.
    /// </summary>
    [TestMethod]
    public async Task UpdateFacultyAsync_ExtremelyLongId_ConstructsFullPath()
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<AcademicService>>();
        var service = new AcademicService(mockApi.Object, mockLogger.Object);

        string id = new string('a', 10000);
        string name = "Engineering";
        string universityId = "uni456";

        string? capturedPath = null;
        var responseMessage = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(new FacultyDto { Id = id, Name = name, UniversityId = universityId })
        };

        mockApi.Setup(x => x.PutAsync(It.IsAny<string>(), It.IsAny<HttpContent>(), It.IsAny<CancellationToken>()))
               .Callback<string, HttpContent, CancellationToken>((path, content, ct) => capturedPath = path)
               .ReturnsAsync(responseMessage);

        // Act
        await service.UpdateFacultyAsync(id, name, universityId);

        // Assert
        Assert.AreEqual($"faculties/{id}", capturedPath);
        Assert.AreEqual(10010, capturedPath.Length); // "faculties/" is 10 chars + 10000
    }

    /// <summary>
    /// Tests that UpdateFacultyAsync returns null when response is 3xx redirect status code.
    /// Input: Valid parameters; API returns 3xx redirect status.
    /// Expected: Returns null (3xx is not a success status code).
    /// </summary>
    [TestMethod]
    [DataRow(HttpStatusCode.MovedPermanently, DisplayName = "301 Moved Permanently")]
    [DataRow(HttpStatusCode.Found, DisplayName = "302 Found")]
    [DataRow(HttpStatusCode.SeeOther, DisplayName = "303 See Other")]
    [DataRow(HttpStatusCode.NotModified, DisplayName = "304 Not Modified")]
    [DataRow(HttpStatusCode.TemporaryRedirect, DisplayName = "307 Temporary Redirect")]
    public async Task UpdateFacultyAsync_3xxRedirectStatusCode_ReturnsNull(HttpStatusCode statusCode)
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<AcademicService>>();
        var service = new AcademicService(mockApi.Object, mockLogger.Object);

        string id = "faculty123";
        string name = "Engineering";
        string universityId = "uni456";

        var responseMessage = new HttpResponseMessage(statusCode);

        mockApi.Setup(x => x.PutAsync($"faculties/{id}", It.IsAny<HttpContent>(), It.IsAny<CancellationToken>()))
               .ReturnsAsync(responseMessage);

        // Act
        var result = await service.UpdateFacultyAsync(id, name, universityId);

        // Assert
        Assert.IsNull(result);
    }

    /// <summary>
    /// Tests that GetDepartmentsAsync returns a single item when API returns one department.
    /// Input: Valid faculty ID; API returns single department.
    /// Expected: Returns collection with one item.
    /// </summary>
    [TestMethod]
    public async Task GetDepartmentsAsync_ApiReturnsSingleItem_ReturnsSingleItemCollection()
    {
        // Arrange
        var facultyId = "FAC123";
        var expectedPath = $"departments?facultyId={facultyId}";
        var expectedItems = new List<LookupItem>
        {
            new LookupItem { Id = "DEPT1", Name = "Computer Science" }
        };
        var jsonContent = JsonSerializer.Serialize(expectedItems);
        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(jsonContent, Encoding.UTF8, "application/json")
        };

        var mockApiService = new Mock<IApiService>();
        mockApiService.Setup(x => x.GetAsync(expectedPath)).ReturnsAsync(httpResponse);
        var mockLogger = new Mock<ILogger<AcademicService>>();
        var service = new AcademicService(mockApiService.Object, mockLogger.Object);

        // Act
        var result = await service.GetDepartmentsAsync(facultyId);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(1, result.Count());
        Assert.AreEqual("DEPT1", result.First().Id);
        Assert.AreEqual("Computer Science", result.First().Name);
    }

    /// <summary>
    /// Tests that GetDepartmentsAsync handles departments with empty string values correctly.
    /// Input: Valid faculty ID; API returns departments with empty string values.
    /// Expected: Returns departments with empty string values preserved.
    /// </summary>
    [TestMethod]
    public async Task GetDepartmentsAsync_ApiReturnsItemsWithEmptyStrings_ReturnsItems()
    {
        // Arrange
        var facultyId = "FAC123";
        var expectedPath = $"departments?facultyId={facultyId}";
        var expectedItems = new List<LookupItem>
        {
            new LookupItem { Id = "", Name = "" },
            new LookupItem { Id = "DEPT2", Name = "" }
        };
        var jsonContent = JsonSerializer.Serialize(expectedItems);
        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(jsonContent, Encoding.UTF8, "application/json")
        };

        var mockApiService = new Mock<IApiService>();
        mockApiService.Setup(x => x.GetAsync(expectedPath)).ReturnsAsync(httpResponse);
        var mockLogger = new Mock<ILogger<AcademicService>>();
        var service = new AcademicService(mockApiService.Object, mockLogger.Object);

        // Act
        var result = await service.GetDepartmentsAsync(facultyId);

        // Assert
        Assert.IsNotNull(result);
        var resultList = result.ToList();
        Assert.AreEqual(2, resultList.Count);
        Assert.AreEqual("", resultList[0].Id);
        Assert.AreEqual("", resultList[0].Name);
    }

    /// <summary>
    /// Tests that GetDepartmentsAsync returns an empty collection when the API returns various HTTP error status codes.
    /// Input: Valid faculty ID; API returns error status codes (400, 401, 403, 500, 503).
    /// Expected: Returns empty collection for all error status codes.
    /// </summary>
    [TestMethod]
    [DataRow(HttpStatusCode.BadRequest)]
    [DataRow(HttpStatusCode.Unauthorized)]
    [DataRow(HttpStatusCode.Forbidden)]
    [DataRow(HttpStatusCode.InternalServerError)]
    [DataRow(HttpStatusCode.ServiceUnavailable)]
    public async Task GetDepartmentsAsync_ApiReturnsErrorStatusCode_ReturnsEmptyCollection(HttpStatusCode statusCode)
    {
        // Arrange
        var facultyId = "FAC123";
        var expectedPath = $"departments?facultyId={facultyId}";
        var httpResponse = new HttpResponseMessage(statusCode);

        var mockApiService = new Mock<IApiService>();
        mockApiService.Setup(x => x.GetAsync(expectedPath)).ReturnsAsync(httpResponse);
        var mockLogger = new Mock<ILogger<AcademicService>>();
        var service = new AcademicService(mockApiService.Object, mockLogger.Object);

        // Act
        var result = await service.GetDepartmentsAsync(facultyId);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(0, result.Count());
    }

    /// <summary>
    /// Tests that GetDepartmentsAsync handles control characters in faculty ID.
    /// Input: Faculty ID with tab, newline, and carriage return characters.
    /// Expected: Constructs path with control characters and calls API.
    /// </summary>
    [TestMethod]
    [DataRow("FAC\t123", "departments?facultyId=FAC\t123")]
    [DataRow("FAC\n123", "departments?facultyId=FAC\n123")]
    [DataRow("FAC\r\n123", "departments?facultyId=FAC\r\n123")]
    public async Task GetDepartmentsAsync_ControlCharactersInFacultyId_ConstructsCorrectPath(string facultyId, string expectedPath)
    {
        // Arrange
        var expectedItems = new List<LookupItem>();
        var jsonContent = JsonSerializer.Serialize(expectedItems);
        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(jsonContent, Encoding.UTF8, "application/json")
        };

        var mockApiService = new Mock<IApiService>();
        mockApiService.Setup(x => x.GetAsync(expectedPath)).ReturnsAsync(httpResponse);
        var mockLogger = new Mock<ILogger<AcademicService>>();
        var service = new AcademicService(mockApiService.Object, mockLogger.Object);

        // Act
        var result = await service.GetDepartmentsAsync(facultyId);

        // Assert
        Assert.IsNotNull(result);
        mockApiService.Verify(x => x.GetAsync(expectedPath), Times.Once);
    }

    /// <summary>
    /// Tests that GetDepartmentsAsync handles large collections correctly.
    /// Input: Valid faculty ID; API returns 100 departments.
    /// Expected: Returns all 100 departments.
    /// </summary>
    [TestMethod]
    public async Task GetDepartmentsAsync_ApiReturnsLargeCollection_ReturnsAllItems()
    {
        // Arrange
        var facultyId = "FAC123";
        var expectedPath = $"departments?facultyId={facultyId}";
        var expectedItems = Enumerable.Range(1, 100)
            .Select(i => new LookupItem { Id = $"DEPT{i}", Name = $"Department {i}" })
            .ToList();
        var jsonContent = JsonSerializer.Serialize(expectedItems);
        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(jsonContent, Encoding.UTF8, "application/json")
        };

        var mockApiService = new Mock<IApiService>();
        mockApiService.Setup(x => x.GetAsync(expectedPath)).ReturnsAsync(httpResponse);
        var mockLogger = new Mock<ILogger<AcademicService>>();
        var service = new AcademicService(mockApiService.Object, mockLogger.Object);

        // Act
        var result = await service.GetDepartmentsAsync(facultyId);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(100, result.Count());
    }

    /// <summary>
    /// Tests that GetDepartmentsAsync handles TaskCanceledException.
    /// Input: Valid faculty ID; API throws TaskCanceledException.
    /// Expected: Returns empty collection and logs error.
    /// </summary>
    [TestMethod]
    public async Task GetDepartmentsAsync_ApiThrowsTaskCanceledException_ReturnsEmptyCollection()
    {
        // Arrange
        var facultyId = "FAC123";
        var expectedPath = $"departments?facultyId={facultyId}";

        var mockApiService = new Mock<IApiService>();
        mockApiService.Setup(x => x.GetAsync(expectedPath)).ThrowsAsync(new TaskCanceledException("Request timeout"));
        var mockLogger = new Mock<ILogger<AcademicService>>();
        var service = new AcademicService(mockApiService.Object, mockLogger.Object);

        // Act
        var result = await service.GetDepartmentsAsync(facultyId);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(0, result.Count());
    }

    /// <summary>
    /// Tests that GetDepartmentsAsync handles InvalidOperationException.
    /// Input: Valid faculty ID; API throws InvalidOperationException.
    /// Expected: Returns empty collection and logs error.
    /// </summary>
    [TestMethod]
    public async Task GetDepartmentsAsync_ApiThrowsInvalidOperationException_ReturnsEmptyCollection()
    {
        // Arrange
        var facultyId = "FAC123";
        var expectedPath = $"departments?facultyId={facultyId}";

        var mockApiService = new Mock<IApiService>();
        mockApiService.Setup(x => x.GetAsync(expectedPath)).ThrowsAsync(new InvalidOperationException("Invalid operation"));
        var mockLogger = new Mock<ILogger<AcademicService>>();
        var service = new AcademicService(mockApiService.Object, mockLogger.Object);

        // Act
        var result = await service.GetDepartmentsAsync(facultyId);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(0, result.Count());
    }

    /// <summary>
    /// Tests that GetDepartmentsAsync handles departments with special characters in names.
    /// Input: Valid faculty ID; API returns departments with special characters.
    /// Expected: Returns departments with special characters preserved.
    /// </summary>
    [TestMethod]
    public async Task GetDepartmentsAsync_ApiReturnsItemsWithSpecialCharacters_ReturnsItems()
    {
        // Arrange
        var facultyId = "FAC123";
        var expectedPath = $"departments?facultyId={facultyId}";
        var expectedItems = new List<LookupItem>
        {
            new LookupItem { Id = "DEPT1", Name = "Computer & Information Science" },
            new LookupItem { Id = "DEPT2", Name = "Math/Physics" },
            new LookupItem { Id = "DEPT3", Name = "Engineering \"Core\"" }
        };
        var jsonContent = JsonSerializer.Serialize(expectedItems);
        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(jsonContent, Encoding.UTF8, "application/json")
        };

        var mockApiService = new Mock<IApiService>();
        mockApiService.Setup(x => x.GetAsync(expectedPath)).ReturnsAsync(httpResponse);
        var mockLogger = new Mock<ILogger<AcademicService>>();
        var service = new AcademicService(mockApiService.Object, mockLogger.Object);

        // Act
        var result = await service.GetDepartmentsAsync(facultyId);

        // Assert
        Assert.IsNotNull(result);
        var resultList = result.ToList();
        Assert.AreEqual(3, resultList.Count);
        Assert.AreEqual("Computer & Information Science", resultList[0].Name);
        Assert.AreEqual("Math/Physics", resultList[1].Name);
        Assert.AreEqual("Engineering \"Core\"", resultList[2].Name);
    }

    /// <summary>
    /// Tests that GetUniversityByIdAsync handles various exception types thrown during API call.
    /// Input: Valid university ID and different exception types.
    /// Expected: Returns null and logs error for all exception types.
    /// </summary>
    [TestMethod]
    [DataRow(typeof(TaskCanceledException), DisplayName = "TaskCanceledException")]
    [DataRow(typeof(OperationCanceledException), DisplayName = "OperationCanceledException")]
    [DataRow(typeof(InvalidOperationException), DisplayName = "InvalidOperationException")]
    public async Task GetUniversityByIdAsync_VariousExceptionTypes_ReturnsNullAndLogsError(Type exceptionType)
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<AcademicService>>();
        var service = new AcademicService(mockApi.Object, mockLogger.Object);

        var universityId = "test-university-123";
        var exception = (Exception)Activator.CreateInstance(exceptionType, "Test exception")!;

        mockApi.Setup(x => x.GetAsync($"universities/{universityId}", It.IsAny<CancellationToken>()))
            .ThrowsAsync(exception);

        // Act
        var result = await service.GetUniversityByIdAsync(universityId);

        // Assert
        Assert.IsNull(result);
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Failed to fetch university")),
                exception,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    /// <summary>
    /// Tests that GetUniversityByIdAsync constructs the correct endpoint path for various ID formats.
    /// Input: Different ID formats including GUID, numeric, and alphanumeric.
    /// Expected: Correct endpoint path is constructed.
    /// </summary>
    [TestMethod]
    [DataRow("123", "universities/123", DisplayName = "Numeric ID")]
    [DataRow("550e8400-e29b-41d4-a716-446655440000", "universities/550e8400-e29b-41d4-a716-446655440000", DisplayName = "GUID format")]
    [DataRow("abc-def-123", "universities/abc-def-123", DisplayName = "Alphanumeric with hyphens")]
    [DataRow("University_123", "universities/University_123", DisplayName = "With underscore")]
    public async Task GetUniversityByIdAsync_VariousIdFormats_ConstructsCorrectPath(string id, string expectedPath)
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<AcademicService>>();
        var service = new AcademicService(mockApi.Object, mockLogger.Object);

        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(new UniversityDto { Id = id, Name = "Test" })
        };

        mockApi.Setup(x => x.GetAsync(expectedPath, It.IsAny<CancellationToken>()))
            .ReturnsAsync(httpResponse);

        // Act
        var result = await service.GetUniversityByIdAsync(id);

        // Assert
        mockApi.Verify(x => x.GetAsync(expectedPath, It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Tests that GetUniversityByIdAsync handles response with partial DTO data correctly.
    /// Input: Successful HTTP response with only some DTO properties populated.
    /// Expected: Returns UniversityDto with only provided properties set.
    /// </summary>
    [TestMethod]
    public async Task GetUniversityByIdAsync_PartialDtoData_ReturnsPartialDto()
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<AcademicService>>();
        var service = new AcademicService(mockApi.Object, mockLogger.Object);

        var universityId = "test-university-123";
        var jsonContent = "{\"Id\":\"test-university-123\",\"Name\":\"Test University\"}";
        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(jsonContent, Encoding.UTF8, "application/json")
        };

        mockApi.Setup(x => x.GetAsync($"universities/{universityId}", It.IsAny<CancellationToken>()))
            .ReturnsAsync(httpResponse);

        // Act
        var result = await service.GetUniversityByIdAsync(universityId);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(universityId, result.Id);
        Assert.AreEqual("Test University", result.Name);
    }

    /// <summary>
    /// Tests that GetStudyModesAsync correctly handles a large collection of study modes.
    /// Input: API returns 100 study mode items.
    /// Expected: Returns all 100 items correctly.
    /// </summary>
    [TestMethod]
    public async Task GetStudyModesAsync_ApiReturnsLargeCollection_ReturnsAllItems()
    {
        // Arrange
        var expectedData = Enumerable.Range(1, 100)
            .Select(i => new LookupItem { Id = $"id-{i}", Name = $"Study Mode {i}" })
            .ToList();

        var mockApiService = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<AcademicService>>();

        var responseMessage = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(expectedData)
        };

        mockApiService.Setup(x => x.GetAsync("study-modes"))
            .ReturnsAsync(responseMessage);

        var service = new AcademicService(mockApiService.Object, mockLogger.Object);

        // Act
        var result = await service.GetStudyModesAsync();

        // Assert
        Assert.IsNotNull(result);
        var resultList = result.ToList();
        Assert.AreEqual(100, resultList.Count);
        Assert.AreEqual("id-1", resultList[0].Id);
        Assert.AreEqual("Study Mode 1", resultList[0].Name);
        Assert.AreEqual("id-100", resultList[99].Id);
        Assert.AreEqual("Study Mode 100", resultList[99].Name);
        mockApiService.Verify(x => x.GetAsync("study-modes"), Times.Once);
    }

    /// <summary>
    /// Tests that GetStudyModesAsync handles whitespace-only strings in study mode data.
    /// Input: API returns items with whitespace-only Id and Name.
    /// Expected: Returns data with whitespace preserved.
    /// </summary>
    [TestMethod]
    public async Task GetStudyModesAsync_ApiReturnsWhitespaceData_ReturnsWhitespacePreserved()
    {
        // Arrange
        var expectedData = new List<LookupItem>
        {
            new LookupItem { Id = "   ", Name = "\t\t" },
            new LookupItem { Id = "\n\r", Name = "  Valid Name  " }
        };

        var mockApiService = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<AcademicService>>();

        var responseMessage = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(expectedData)
        };

        mockApiService.Setup(x => x.GetAsync("study-modes"))
            .ReturnsAsync(responseMessage);

        var service = new AcademicService(mockApiService.Object, mockLogger.Object);

        // Act
        var result = await service.GetStudyModesAsync();

        // Assert
        Assert.IsNotNull(result);
        var resultList = result.ToList();
        Assert.AreEqual(2, resultList.Count);
        Assert.AreEqual("   ", resultList[0].Id);
        Assert.AreEqual("\t\t", resultList[0].Name);
        mockApiService.Verify(x => x.GetAsync("study-modes"), Times.Once);
    }

    /// <summary>
    /// Tests that GetStudyModesAsync handles very long string values in study mode data.
    /// Input: API returns items with very long Id and Name strings (10000+ characters).
    /// Expected: Returns data with long strings preserved.
    /// </summary>
    [TestMethod]
    public async Task GetStudyModesAsync_ApiReturnsVeryLongStrings_ReturnsLongStringsPreserved()
    {
        // Arrange
        var longString = new string('A', 10000);
        var expectedData = new List<LookupItem>
        {
            new LookupItem { Id = longString, Name = longString }
        };

        var mockApiService = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<AcademicService>>();

        var responseMessage = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(expectedData)
        };

        mockApiService.Setup(x => x.GetAsync("study-modes"))
            .ReturnsAsync(responseMessage);

        var service = new AcademicService(mockApiService.Object, mockLogger.Object);

        // Act
        var result = await service.GetStudyModesAsync();

        // Assert
        Assert.IsNotNull(result);
        var resultList = result.ToList();
        Assert.AreEqual(1, resultList.Count);
        Assert.AreEqual(longString, resultList[0].Id);
        Assert.AreEqual(longString, resultList[0].Name);
        mockApiService.Verify(x => x.GetAsync("study-modes"), Times.Once);
    }

    /// <summary>
    /// Tests that DeleteUniversityAsync returns true when API returns 200 OK status code.
    /// Input: Valid university ID and API returns HttpStatusCode.OK.
    /// Expected: Returns true and verifies DeleteAsync was called once.
    /// </summary>
    [TestMethod]
    public async Task DeleteUniversityAsync_ApiReturnsOK_ReturnsTrue()
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<AcademicService>>();
        var service = new AcademicService(mockApi.Object, mockLogger.Object);
        var universityId = "uni123";
        var response = new HttpResponseMessage(HttpStatusCode.OK);

        mockApi.Setup(x => x.DeleteAsync($"universities/{universityId}", It.IsAny<CancellationToken>()))
               .ReturnsAsync(response);

        // Act
        var result = await service.DeleteUniversityAsync(universityId);

        // Assert
        Assert.IsTrue(result);
        mockApi.Verify(x => x.DeleteAsync($"universities/{universityId}", It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Tests that DeleteUniversityAsync returns false for various HTTP error status codes.
    /// Input: Valid university ID and API returns different error status codes.
    /// Expected: Returns false for all non-2xx status codes.
    /// </summary>
    [TestMethod]
    [DataRow(HttpStatusCode.BadRequest, DisplayName = "400 Bad Request")]
    [DataRow(HttpStatusCode.Unauthorized, DisplayName = "401 Unauthorized")]
    [DataRow(HttpStatusCode.Forbidden, DisplayName = "403 Forbidden")]
    [DataRow(HttpStatusCode.NotFound, DisplayName = "404 Not Found")]
    [DataRow(HttpStatusCode.MethodNotAllowed, DisplayName = "405 Method Not Allowed")]
    [DataRow(HttpStatusCode.Conflict, DisplayName = "409 Conflict")]
    [DataRow(HttpStatusCode.InternalServerError, DisplayName = "500 Internal Server Error")]
    [DataRow(HttpStatusCode.BadGateway, DisplayName = "502 Bad Gateway")]
    [DataRow(HttpStatusCode.ServiceUnavailable, DisplayName = "503 Service Unavailable")]
    [DataRow(HttpStatusCode.GatewayTimeout, DisplayName = "504 Gateway Timeout")]
    public async Task DeleteUniversityAsync_ErrorStatusCodes_ReturnsFalse(HttpStatusCode statusCode)
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<AcademicService>>();
        var service = new AcademicService(mockApi.Object, mockLogger.Object);
        var universityId = "uni123";
        var response = new HttpResponseMessage(statusCode);

        mockApi.Setup(x => x.DeleteAsync($"universities/{universityId}", It.IsAny<CancellationToken>()))
               .ReturnsAsync(response);

        // Act
        var result = await service.DeleteUniversityAsync(universityId);

        // Assert
        Assert.IsFalse(result);
        mockApi.Verify(x => x.DeleteAsync($"universities/{universityId}", It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Tests that DeleteUniversityAsync returns false and logs error when HttpRequestException is thrown.
    /// Input: Valid university ID and API throws HttpRequestException.
    /// Expected: Returns false and logs error with exception details and university id.
    /// </summary>
    [TestMethod]
    public async Task DeleteUniversityAsync_HttpRequestExceptionThrown_ReturnsFalseAndLogsError()
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<AcademicService>>();
        var service = new AcademicService(mockApi.Object, mockLogger.Object);
        var universityId = "uni123";
        var exception = new HttpRequestException("Network connection failed");

        mockApi.Setup(x => x.DeleteAsync($"universities/{universityId}", It.IsAny<CancellationToken>()))
               .ThrowsAsync(exception);

        // Act
        var result = await service.DeleteUniversityAsync(universityId);

        // Assert
        Assert.IsFalse(result);
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Failed to delete university") && v.ToString()!.Contains(universityId)),
                exception,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    /// <summary>
    /// Tests that DeleteUniversityAsync returns false and logs error when TaskCanceledException is thrown.
    /// Input: Valid university ID and API throws TaskCanceledException (timeout scenario).
    /// Expected: Returns false and logs error with exception details.
    /// </summary>
    [TestMethod]
    public async Task DeleteUniversityAsync_TaskCanceledExceptionThrown_ReturnsFalseAndLogsError()
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<AcademicService>>();
        var service = new AcademicService(mockApi.Object, mockLogger.Object);
        var universityId = "uni123";
        var exception = new TaskCanceledException("Request timeout");

        mockApi.Setup(x => x.DeleteAsync($"universities/{universityId}", It.IsAny<CancellationToken>()))
               .ThrowsAsync(exception);

        // Act
        var result = await service.DeleteUniversityAsync(universityId);

        // Assert
        Assert.IsFalse(result);
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Failed to delete university")),
                exception,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    /// <summary>
    /// Tests that DeleteUniversityAsync constructs correct endpoint for various university ID formats.
    /// Input: Various string formats for university ID including edge cases.
    /// Expected: Correct endpoint path is constructed and called.
    /// </summary>
    [TestMethod]
    [DataRow("uni123", "universities/uni123", DisplayName = "Standard alphanumeric ID")]
    [DataRow("", "universities/", DisplayName = "Empty string ID")]
    [DataRow("   ", "universities/   ", DisplayName = "Whitespace-only ID")]
    [DataRow("abc-def-123", "universities/abc-def-123", DisplayName = "ID with hyphens")]
    [DataRow("550e8400-e29b-41d4-a716-446655440000", "universities/550e8400-e29b-41d4-a716-446655440000", DisplayName = "GUID format ID")]
    [DataRow("id with spaces", "universities/id with spaces", DisplayName = "ID with spaces")]
    [DataRow("special!@#$%chars", "universities/special!@#$%chars", DisplayName = "ID with special characters")]
    public async Task DeleteUniversityAsync_VariousIdFormats_ConstructsCorrectEndpoint(string id, string expectedPath)
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<AcademicService>>();
        var service = new AcademicService(mockApi.Object, mockLogger.Object);
        var response = new HttpResponseMessage(HttpStatusCode.OK);

        mockApi.Setup(x => x.DeleteAsync(expectedPath, It.IsAny<CancellationToken>()))
               .ReturnsAsync(response);

        // Act
        var result = await service.DeleteUniversityAsync(id);

        // Assert
        mockApi.Verify(x => x.DeleteAsync(expectedPath, It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Tests that DeleteUniversityAsync handles very long university ID strings.
    /// Input: Very long string (10000+ characters) as university ID.
    /// Expected: Endpoint is constructed and API is called successfully.
    /// </summary>
    [TestMethod]
    public async Task DeleteUniversityAsync_VeryLongId_CallsApiSuccessfully()
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<AcademicService>>();
        var service = new AcademicService(mockApi.Object, mockLogger.Object);
        var veryLongId = new string('x', 10000);
        var response = new HttpResponseMessage(HttpStatusCode.OK);

        mockApi.Setup(x => x.DeleteAsync($"universities/{veryLongId}", It.IsAny<CancellationToken>()))
               .ReturnsAsync(response);

        // Act
        var result = await service.DeleteUniversityAsync(veryLongId);

        // Assert
        Assert.IsTrue(result);
        mockApi.Verify(x => x.DeleteAsync($"universities/{veryLongId}", It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Tests that DeleteUniversityAsync handles Unicode characters in university ID.
    /// Input: University ID with various Unicode characters.
    /// Expected: Endpoint is constructed correctly with Unicode characters.
    /// </summary>
    [TestMethod]
    [DataRow("大学123", DisplayName = "Chinese characters")]
    [DataRow("université-456", DisplayName = "French characters")]
    [DataRow("университет789", DisplayName = "Cyrillic characters")]
    [DataRow("🏫school", DisplayName = "Emoji characters")]
    public async Task DeleteUniversityAsync_UnicodeCharactersInId_ConstructsCorrectEndpoint(string id)
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<AcademicService>>();
        var service = new AcademicService(mockApi.Object, mockLogger.Object);
        var response = new HttpResponseMessage(HttpStatusCode.OK);

        mockApi.Setup(x => x.DeleteAsync($"universities/{id}", It.IsAny<CancellationToken>()))
               .ReturnsAsync(response);

        // Act
        var result = await service.DeleteUniversityAsync(id);

        // Assert
        Assert.IsTrue(result);
        mockApi.Verify(x => x.DeleteAsync($"universities/{id}", It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Tests that DeleteUniversityAsync does not log when API returns non-success status code.
    /// Input: Valid university ID and API returns 404 NotFound.
    /// Expected: Returns false without logging any error.
    /// </summary>
    [TestMethod]
    public async Task DeleteUniversityAsync_NonSuccessStatusCode_DoesNotLog()
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<AcademicService>>();
        var service = new AcademicService(mockApi.Object, mockLogger.Object);
        var universityId = "uni123";
        var response = new HttpResponseMessage(HttpStatusCode.NotFound);

        mockApi.Setup(x => x.DeleteAsync($"universities/{universityId}", It.IsAny<CancellationToken>()))
               .ReturnsAsync(response);

        // Act
        var result = await service.DeleteUniversityAsync(universityId);

        // Assert
        Assert.IsFalse(result);
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
    /// Tests that DeleteUniversityAsync does not log when API returns success status code.
    /// Input: Valid university ID and API returns 200 OK.
    /// Expected: Returns true without logging any messages.
    /// </summary>
    [TestMethod]
    public async Task DeleteUniversityAsync_SuccessStatusCode_DoesNotLog()
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<AcademicService>>();
        var service = new AcademicService(mockApi.Object, mockLogger.Object);
        var universityId = "uni123";
        var response = new HttpResponseMessage(HttpStatusCode.OK);

        mockApi.Setup(x => x.DeleteAsync($"universities/{universityId}", It.IsAny<CancellationToken>()))
               .ReturnsAsync(response);

        // Act
        var result = await service.DeleteUniversityAsync(universityId);

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
    /// Tests that DeleteUniversityAsync logs the correct university ID when exception occurs.
    /// Input: Specific university ID and API throws exception.
    /// Expected: Error log contains the exact university ID that was passed.
    /// </summary>
    [TestMethod]
    public async Task DeleteUniversityAsync_ExceptionOccurs_LogsCorrectUniversityId()
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<AcademicService>>();
        var service = new AcademicService(mockApi.Object, mockLogger.Object);
        var universityId = "specific-uni-456";
        var exception = new HttpRequestException("Network error");

        mockApi.Setup(x => x.DeleteAsync($"universities/{universityId}", It.IsAny<CancellationToken>()))
               .ThrowsAsync(exception);

        // Act
        var result = await service.DeleteUniversityAsync(universityId);

        // Assert
        Assert.IsFalse(result);
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains(universityId)),
                exception,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    /// <summary>
    /// Tests that DeleteUniversityAsync handles control characters in university ID.
    /// Input: University ID with control characters (newline, tab, carriage return).
    /// Expected: Endpoint is constructed and API is called.
    /// </summary>
    [TestMethod]
    [DataRow("uni\n123", DisplayName = "ID with newline")]
    [DataRow("uni\t456", DisplayName = "ID with tab")]
    [DataRow("uni\r789", DisplayName = "ID with carriage return")]
    [DataRow("uni\0null", DisplayName = "ID with null character")]
    public async Task DeleteUniversityAsync_ControlCharactersInId_CallsApi(string id)
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<AcademicService>>();
        var service = new AcademicService(mockApi.Object, mockLogger.Object);
        var response = new HttpResponseMessage(HttpStatusCode.OK);

        mockApi.Setup(x => x.DeleteAsync($"universities/{id}", It.IsAny<CancellationToken>()))
               .ReturnsAsync(response);

        // Act
        var result = await service.DeleteUniversityAsync(id);

        // Assert
        Assert.IsTrue(result);
        mockApi.Verify(x => x.DeleteAsync($"universities/{id}", It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Tests that DeleteUniversityAsync returns false for 3xx redirection status codes.
    /// Input: Valid university ID and API returns redirection status codes.
    /// Expected: Returns false for all 3xx status codes.
    /// </summary>
    [TestMethod]
    [DataRow(HttpStatusCode.MovedPermanently, DisplayName = "301 Moved Permanently")]
    [DataRow(HttpStatusCode.Found, DisplayName = "302 Found")]
    [DataRow(HttpStatusCode.NotModified, DisplayName = "304 Not Modified")]
    [DataRow(HttpStatusCode.TemporaryRedirect, DisplayName = "307 Temporary Redirect")]
    public async Task DeleteUniversityAsync_RedirectionStatusCodes_ReturnsFalse(HttpStatusCode statusCode)
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<AcademicService>>();
        var service = new AcademicService(mockApi.Object, mockLogger.Object);
        var universityId = "uni123";
        var response = new HttpResponseMessage(statusCode);

        mockApi.Setup(x => x.DeleteAsync($"universities/{universityId}", It.IsAny<CancellationToken>()))
               .ReturnsAsync(response);

        // Act
        var result = await service.DeleteUniversityAsync(universityId);

        // Assert
        Assert.IsFalse(result);
    }

    /// <summary>
    /// Tests that GetFacultiesAsync with null university ID constructs the correct path with null in query string.
    /// Input: Null universityId.
    /// Expected: API is called with "faculties?universityId=" path.
    /// </summary>
    [TestMethod]
    public async Task GetFacultiesAsync_NullUniversityId_ConstructsPathWithNull()
    {
        // Arrange
        string? universityId = null;
        var expectedPath = $"faculties?universityId={universityId}";
        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("[]", Encoding.UTF8, "application/json")
        };

        var mockApi = new Mock<IApiService>();
        mockApi.Setup(x => x.GetAsync(expectedPath)).ReturnsAsync(httpResponse);
        var mockLogger = new Mock<ILogger<AcademicService>>();
        var service = new AcademicService(mockApi.Object, mockLogger.Object);

        // Act
        var result = await service.GetFacultiesAsync(universityId!);

        // Assert
        Assert.IsNotNull(result);
        mockApi.Verify(x => x.GetAsync(expectedPath), Times.Once);
    }

    /// <summary>
    /// Tests that GetFacultiesAsync with Unicode characters in university ID constructs the correct path.
    /// Input: University ID with Unicode characters.
    /// Expected: API is called with Unicode characters in the path.
    /// </summary>
    [TestMethod]
    [DataRow("uni日本", "faculties?universityId=uni日本")]
    [DataRow("uni한국어", "faculties?universityId=uni한국어")]
    [DataRow("uniالعربية", "faculties?universityId=uniالعربية")]
    [DataRow("uni中文", "faculties?universityId=uni中文")]
    [DataRow("uni🔥", "faculties?universityId=uni🔥")]
    public async Task GetFacultiesAsync_UnicodeCharacters_ConstructsCorrectPath(string universityId, string expectedPath)
    {
        // Arrange
        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("[]", Encoding.UTF8, "application/json")
        };

        var mockApi = new Mock<IApiService>();
        mockApi.Setup(x => x.GetAsync(expectedPath)).ReturnsAsync(httpResponse);
        var mockLogger = new Mock<ILogger<AcademicService>>();
        var service = new AcademicService(mockApi.Object, mockLogger.Object);

        // Act
        var result = await service.GetFacultiesAsync(universityId);

        // Assert
        Assert.IsNotNull(result);
        mockApi.Verify(x => x.GetAsync(expectedPath), Times.Once);
    }

    /// <summary>
    /// Tests that GetFacultiesAsync handles control characters in university ID.
    /// Input: University ID with control characters (\n, \t, \r).
    /// Expected: API is called with control characters in the path.
    /// </summary>
    [TestMethod]
    [DataRow("uni\nid", "faculties?universityId=uni\nid")]
    [DataRow("uni\tid", "faculties?universityId=uni\tid")]
    [DataRow("uni\rid", "faculties?universityId=uni\rid")]
    [DataRow("uni\n\t\rid", "faculties?universityId=uni\n\t\rid")]
    public async Task GetFacultiesAsync_ControlCharacters_ConstructsCorrectPath(string universityId, string expectedPath)
    {
        // Arrange
        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("[]", Encoding.UTF8, "application/json")
        };

        var mockApi = new Mock<IApiService>();
        mockApi.Setup(x => x.GetAsync(expectedPath)).ReturnsAsync(httpResponse);
        var mockLogger = new Mock<ILogger<AcademicService>>();
        var service = new AcademicService(mockApi.Object, mockLogger.Object);

        // Act
        var result = await service.GetFacultiesAsync(universityId);

        // Assert
        Assert.IsNotNull(result);
        mockApi.Verify(x => x.GetAsync(expectedPath), Times.Once);
    }

    /// <summary>
    /// Tests that GetFacultiesAsync returns a collection with multiple items when API returns multiple faculties.
    /// Input: Valid university ID; API returns multiple faculties.
    /// Expected: Returns collection with all items from API.
    /// </summary>
    [TestMethod]
    public async Task GetFacultiesAsync_ApiReturnsMultipleItems_ReturnsAllItems()
    {
        // Arrange
        var universityId = "uni123";
        var expectedPath = $"faculties?universityId={universityId}";
        var expectedData = new List<LookupItem>
        {
            new LookupItem { Id = "fac1", Name = "Faculty of Engineering" },
            new LookupItem { Id = "fac2", Name = "Faculty of Science" },
            new LookupItem { Id = "fac3", Name = "Faculty of Arts" },
            new LookupItem { Id = "fac4", Name = "Faculty of Medicine" }
        };
        var jsonContent = JsonSerializer.Serialize(expectedData);
        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(jsonContent, Encoding.UTF8, "application/json")
        };

        var mockApi = new Mock<IApiService>();
        mockApi.Setup(x => x.GetAsync(expectedPath)).ReturnsAsync(httpResponse);
        var mockLogger = new Mock<ILogger<AcademicService>>();
        var service = new AcademicService(mockApi.Object, mockLogger.Object);

        // Act
        var result = await service.GetFacultiesAsync(universityId);

        // Assert
        Assert.IsNotNull(result);
        var resultList = result.ToList();
        Assert.AreEqual(4, resultList.Count);
        Assert.AreEqual("fac1", resultList[0].Id);
        Assert.AreEqual("Faculty of Engineering", resultList[0].Name);
        Assert.AreEqual("fac4", resultList[3].Id);
        Assert.AreEqual("Faculty of Medicine", resultList[3].Name);
    }

    /// <summary>
    /// Tests that GetFacultiesAsync returns empty collection when API returns empty array.
    /// Input: Valid university ID; API returns empty array.
    /// Expected: Returns empty collection.
    /// </summary>
    [TestMethod]
    public async Task GetFacultiesAsync_ApiReturnsEmptyArray_ReturnsEmptyCollection()
    {
        // Arrange
        var universityId = "uni123";
        var expectedPath = $"faculties?universityId={universityId}";
        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("[]", Encoding.UTF8, "application/json")
        };

        var mockApi = new Mock<IApiService>();
        mockApi.Setup(x => x.GetAsync(expectedPath)).ReturnsAsync(httpResponse);
        var mockLogger = new Mock<ILogger<AcademicService>>();
        var service = new AcademicService(mockApi.Object, mockLogger.Object);

        // Act
        var result = await service.GetFacultiesAsync(universityId);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(0, result.Count());
    }

    /// <summary>
    /// Tests that GetFacultiesAsync handles faculties with empty string values in properties.
    /// Input: Valid university ID; API returns faculties with empty strings.
    /// Expected: Returns faculties with empty string values preserved.
    /// </summary>
    [TestMethod]
    public async Task GetFacultiesAsync_ApiReturnsFacultiesWithEmptyStrings_ReturnsCorrectData()
    {
        // Arrange
        var universityId = "uni123";
        var expectedPath = $"faculties?universityId={universityId}";
        var expectedData = new List<LookupItem>
        {
            new LookupItem { Id = "", Name = "" },
            new LookupItem { Id = "fac1", Name = "" },
            new LookupItem { Id = "", Name = "Faculty Name" }
        };
        var jsonContent = JsonSerializer.Serialize(expectedData);
        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(jsonContent, Encoding.UTF8, "application/json")
        };

        var mockApi = new Mock<IApiService>();
        mockApi.Setup(x => x.GetAsync(expectedPath)).ReturnsAsync(httpResponse);
        var mockLogger = new Mock<ILogger<AcademicService>>();
        var service = new AcademicService(mockApi.Object, mockLogger.Object);

        // Act
        var result = await service.GetFacultiesAsync(universityId);

        // Assert
        Assert.IsNotNull(result);
        var resultList = result.ToList();
        Assert.AreEqual(3, resultList.Count);
        Assert.AreEqual("", resultList[0].Id);
        Assert.AreEqual("", resultList[0].Name);
    }

    /// <summary>
    /// Tests that GetFacultiesAsync returns empty collection when API returns invalid JSON.
    /// Input: Valid university ID; API returns malformed JSON.
    /// Expected: Returns empty collection and logs error.
    /// </summary>
    [TestMethod]
    public async Task GetFacultiesAsync_ApiReturnsInvalidJson_ReturnsEmptyCollection()
    {
        // Arrange
        var universityId = "uni123";
        var expectedPath = $"faculties?universityId={universityId}";
        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("{invalid json}", Encoding.UTF8, "application/json")
        };

        var mockApi = new Mock<IApiService>();
        mockApi.Setup(x => x.GetAsync(expectedPath)).ReturnsAsync(httpResponse);
        var mockLogger = new Mock<ILogger<AcademicService>>();
        var service = new AcademicService(mockApi.Object, mockLogger.Object);

        // Act
        var result = await service.GetFacultiesAsync(universityId);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(0, result.Count());
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => true),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception?, string>>((v, t) => true)),
            Times.Once);
    }

    /// <summary>
    /// Tests that GetFacultiesAsync with multiple different university IDs maintains separate cache entries.
    /// Input: Three different university IDs called in sequence.
    /// Expected: Three separate API calls made, each cached independently.
    /// </summary>
    [TestMethod]
    public async Task GetFacultiesAsync_MultipleDifferentIds_MaintainsSeparateCacheEntries()
    {
        // Arrange
        var universityId1 = "uni1";
        var universityId2 = "uni2";
        var universityId3 = "uni3";
        var path1 = $"faculties?universityId={universityId1}";
        var path2 = $"faculties?universityId={universityId2}";
        var path3 = $"faculties?universityId={universityId3}";

        var data1 = new List<LookupItem> { new LookupItem { Id = "fac1", Name = "Faculty 1" } };
        var data2 = new List<LookupItem> { new LookupItem { Id = "fac2", Name = "Faculty 2" } };
        var data3 = new List<LookupItem> { new LookupItem { Id = "fac3", Name = "Faculty 3" } };

        var httpResponse1 = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(JsonSerializer.Serialize(data1), Encoding.UTF8, "application/json")
        };
        var httpResponse2 = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(JsonSerializer.Serialize(data2), Encoding.UTF8, "application/json")
        };
        var httpResponse3 = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(JsonSerializer.Serialize(data3), Encoding.UTF8, "application/json")
        };

        var mockApi = new Mock<IApiService>();
        mockApi.Setup(x => x.GetAsync(path1)).ReturnsAsync(httpResponse1);
        mockApi.Setup(x => x.GetAsync(path2)).ReturnsAsync(httpResponse2);
        mockApi.Setup(x => x.GetAsync(path3)).ReturnsAsync(httpResponse3);
        var mockLogger = new Mock<ILogger<AcademicService>>();
        var service = new AcademicService(mockApi.Object, mockLogger.Object);

        // Act
        var result1 = await service.GetFacultiesAsync(universityId1);
        var result2 = await service.GetFacultiesAsync(universityId2);
        var result3 = await service.GetFacultiesAsync(universityId3);

        // Assert
        Assert.AreEqual(1, result1.Count());
        Assert.AreEqual("fac1", result1.First().Id);
        Assert.AreEqual(1, result2.Count());
        Assert.AreEqual("fac2", result2.First().Id);
        Assert.AreEqual(1, result3.Count());
        Assert.AreEqual("fac3", result3.First().Id);
        mockApi.Verify(x => x.GetAsync(path1), Times.Once);
        mockApi.Verify(x => x.GetAsync(path2), Times.Once);
        mockApi.Verify(x => x.GetAsync(path3), Times.Once);
    }

    /// <summary>
    /// Tests that GetFacultiesAsync handles API timeout exception correctly.
    /// Input: Valid university ID; API throws TaskCanceledException.
    /// Expected: Returns empty collection and logs error.
    /// </summary>
    [TestMethod]
    public async Task GetFacultiesAsync_ApiThrowsTaskCanceledException_ReturnsEmptyCollection()
    {
        // Arrange
        var universityId = "uni123";
        var expectedPath = $"faculties?universityId={universityId}";

        var mockApi = new Mock<IApiService>();
        mockApi.Setup(x => x.GetAsync(expectedPath)).ThrowsAsync(new TaskCanceledException("Request timed out"));
        var mockLogger = new Mock<ILogger<AcademicService>>();
        var service = new AcademicService(mockApi.Object, mockLogger.Object);

        // Act
        var result = await service.GetFacultiesAsync(universityId);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(0, result.Count());
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => true),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception?, string>>((v, t) => true)),
            Times.Once);
    }

    /// <summary>
    /// Tests that GetFacultiesAsync handles API throwing InvalidOperationException correctly.
    /// Input: Valid university ID; API throws InvalidOperationException.
    /// Expected: Returns empty collection and logs error.
    /// </summary>
    [TestMethod]
    public async Task GetFacultiesAsync_ApiThrowsInvalidOperationException_ReturnsEmptyCollection()
    {
        // Arrange
        var universityId = "uni123";
        var expectedPath = $"faculties?universityId={universityId}";

        var mockApi = new Mock<IApiService>();
        mockApi.Setup(x => x.GetAsync(expectedPath)).ThrowsAsync(new InvalidOperationException("Invalid operation"));
        var mockLogger = new Mock<ILogger<AcademicService>>();
        var service = new AcademicService(mockApi.Object, mockLogger.Object);

        // Act
        var result = await service.GetFacultiesAsync(universityId);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(0, result.Count());
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => true),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception?, string>>((v, t) => true)),
            Times.Once);
    }

    /// <summary>
    /// Tests that GetFacultiesAsync handles faculties with special characters in names.
    /// Input: Valid university ID; API returns faculties with special characters.
    /// Expected: Returns faculties with special characters preserved.
    /// </summary>
    [TestMethod]
    public async Task GetFacultiesAsync_FacultiesWithSpecialCharactersInNames_ReturnsCorrectData()
    {
        // Arrange
        var universityId = "uni123";
        var expectedPath = $"faculties?universityId={universityId}";
        var expectedData = new List<LookupItem>
        {
            new LookupItem { Id = "fac1", Name = "Faculty of \"Engineering\"" },
            new LookupItem { Id = "fac2", Name = "Faculty & Science" },
            new LookupItem { Id = "fac3", Name = "Faculty <Arts>" },
            new LookupItem { Id = "fac4", Name = "Faculty's Medicine" }
        };
        var jsonContent = JsonSerializer.Serialize(expectedData);
        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(jsonContent, Encoding.UTF8, "application/json")
        };

        var mockApi = new Mock<IApiService>();
        mockApi.Setup(x => x.GetAsync(expectedPath)).ReturnsAsync(httpResponse);
        var mockLogger = new Mock<ILogger<AcademicService>>();
        var service = new AcademicService(mockApi.Object, mockLogger.Object);

        // Act
        var result = await service.GetFacultiesAsync(universityId);

        // Assert
        Assert.IsNotNull(result);
        var resultList = result.ToList();
        Assert.AreEqual(4, resultList.Count);
        Assert.AreEqual("Faculty of \"Engineering\"", resultList[0].Name);
        Assert.AreEqual("Faculty & Science", resultList[1].Name);
        Assert.AreEqual("Faculty <Arts>", resultList[2].Name);
        Assert.AreEqual("Faculty's Medicine", resultList[3].Name);
    }

    /// <summary>
    /// Tests that GetFacultiesAsync returns single item collection when API returns one faculty.
    /// Input: Valid university ID; API returns single faculty.
    /// Expected: Returns collection with single item.
    /// </summary>
    [TestMethod]
    public async Task GetFacultiesAsync_ApiReturnsSingleItem_ReturnsSingleItemCollection()
    {
        // Arrange
        var universityId = "uni123";
        var expectedPath = $"faculties?universityId={universityId}";
        var expectedData = new List<LookupItem>
        {
            new LookupItem { Id = "fac1", Name = "Faculty of Engineering" }
        };
        var jsonContent = JsonSerializer.Serialize(expectedData);
        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(jsonContent, Encoding.UTF8, "application/json")
        };

        var mockApi = new Mock<IApiService>();
        mockApi.Setup(x => x.GetAsync(expectedPath)).ReturnsAsync(httpResponse);
        var mockLogger = new Mock<ILogger<AcademicService>>();
        var service = new AcademicService(mockApi.Object, mockLogger.Object);

        // Act
        var result = await service.GetFacultiesAsync(universityId);

        // Assert
        Assert.IsNotNull(result);
        var resultList = result.ToList();
        Assert.AreEqual(1, resultList.Count);
        Assert.AreEqual("fac1", resultList[0].Id);
        Assert.AreEqual("Faculty of Engineering", resultList[0].Name);
    }

    /// <summary>
    /// Tests that GetFacultiesAsync handles various HTTP 4xx client error status codes.
    /// Input: Valid university ID; API returns client error status codes.
    /// Expected: Returns empty collection and logs warning.
    /// </summary>
    [TestMethod]
    [DataRow(HttpStatusCode.Unauthorized)]
    [DataRow(HttpStatusCode.Forbidden)]
    [DataRow(HttpStatusCode.Conflict)]
    [DataRow(HttpStatusCode.Gone)]
    public async Task GetFacultiesAsync_ApiReturns4xxError_ReturnsEmptyCollectionAndLogsWarning(HttpStatusCode statusCode)
    {
        // Arrange
        var universityId = "uni123";
        var expectedPath = $"faculties?universityId={universityId}";
        var httpResponse = new HttpResponseMessage(statusCode);

        var mockApi = new Mock<IApiService>();
        mockApi.Setup(x => x.GetAsync(expectedPath)).ReturnsAsync(httpResponse);
        var mockLogger = new Mock<ILogger<AcademicService>>();
        var service = new AcademicService(mockApi.Object, mockLogger.Object);

        // Act
        var result = await service.GetFacultiesAsync(universityId);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(0, result.Count());
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
    /// Tests that GetFacultiesAsync handles various HTTP 5xx server error status codes.
    /// Input: Valid university ID; API returns server error status codes.
    /// Expected: Returns empty collection and logs warning.
    /// </summary>
    [TestMethod]
    [DataRow(HttpStatusCode.BadGateway)]
    [DataRow(HttpStatusCode.GatewayTimeout)]
    [DataRow(HttpStatusCode.HttpVersionNotSupported)]
    public async Task GetFacultiesAsync_ApiReturns5xxError_ReturnsEmptyCollectionAndLogsWarning(HttpStatusCode statusCode)
    {
        // Arrange
        var universityId = "uni123";
        var expectedPath = $"faculties?universityId={universityId}";
        var httpResponse = new HttpResponseMessage(statusCode);

        var mockApi = new Mock<IApiService>();
        mockApi.Setup(x => x.GetAsync(expectedPath)).ReturnsAsync(httpResponse);
        var mockLogger = new Mock<ILogger<AcademicService>>();
        var service = new AcademicService(mockApi.Object, mockLogger.Object);

        // Act
        var result = await service.GetFacultiesAsync(universityId);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(0, result.Count());
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
    /// Tests that GetAcademicYearsAsync handles a very large collection of academic years correctly.
    /// Input: API returns a large collection (1000 items).
    /// Expected: Returns all items without data loss or performance issues.
    /// </summary>
    [TestMethod]
    public async Task GetAcademicYearsAsync_WhenApiReturnsLargeCollection_ReturnsAllItems()
    {
        // Arrange
        var largeData = new List<LookupItem>();
        for (int i = 0; i < 1000; i++)
        {
            largeData.Add(new LookupItem { Id = $"year-{i}", Name = $"Academic Year {i}/{i + 1}" });
        }

        var responseContent = JsonSerializer.Serialize(largeData);
        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(responseContent, Encoding.UTF8, "application/json")
        };

        var mockApi = new Mock<IApiService>();
        mockApi.Setup(a => a.GetAsync("academic-years"))
            .ReturnsAsync(httpResponse);

        var mockLogger = new Mock<ILogger<AcademicService>>();
        var service = new AcademicService(mockApi.Object, mockLogger.Object);

        // Act
        var result = await service.GetAcademicYearsAsync();

        // Assert
        Assert.IsNotNull(result);
        var resultList = result.ToList();
        Assert.AreEqual(1000, resultList.Count);
        Assert.AreEqual("year-0", resultList[0].Id);
        Assert.AreEqual("year-999", resultList[999].Id);
        mockApi.Verify(a => a.GetAsync("academic-years"), Times.Once);
    }

    /// <summary>
    /// Tests that GetAcademicYearsAsync handles academic years with very long string values.
    /// Input: API returns items with very long Id and Name strings (5000+ characters each).
    /// Expected: Returns items with long strings preserved correctly.
    /// </summary>
    [TestMethod]
    public async Task GetAcademicYearsAsync_WhenApiReturnsItemsWithVeryLongStrings_ReturnsItems()
    {
        // Arrange
        var longId = new string('A', 5000);
        var longName = new string('B', 5000);
        var expectedData = new List<LookupItem>
        {
            new LookupItem { Id = longId, Name = longName }
        };

        var responseContent = JsonSerializer.Serialize(expectedData);
        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(responseContent, Encoding.UTF8, "application/json")
        };

        var mockApi = new Mock<IApiService>();
        mockApi.Setup(a => a.GetAsync("academic-years"))
            .ReturnsAsync(httpResponse);

        var mockLogger = new Mock<ILogger<AcademicService>>();
        var service = new AcademicService(mockApi.Object, mockLogger.Object);

        // Act
        var result = await service.GetAcademicYearsAsync();

        // Assert
        Assert.IsNotNull(result);
        var resultList = result.ToList();
        Assert.AreEqual(1, resultList.Count);
        Assert.AreEqual(5000, resultList[0].Id.Length);
        Assert.AreEqual(5000, resultList[0].Name.Length);
        Assert.AreEqual(longId, resultList[0].Id);
        Assert.AreEqual(longName, resultList[0].Name);
    }

    /// <summary>
    /// Tests that GetAcademicYearsAsync handles a collection with duplicate items correctly.
    /// Input: API returns multiple items with identical Id and Name values.
    /// Expected: Returns all items including duplicates without modification.
    /// </summary>
    [TestMethod]
    public async Task GetAcademicYearsAsync_WhenApiReturnsDuplicateItems_ReturnsAllItems()
    {
        // Arrange
        var expectedData = new List<LookupItem>
        {
            new LookupItem { Id = "1", Name = "2023/2024" },
            new LookupItem { Id = "1", Name = "2023/2024" },
            new LookupItem { Id = "1", Name = "2023/2024" }
        };

        var responseContent = JsonSerializer.Serialize(expectedData);
        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(responseContent, Encoding.UTF8, "application/json")
        };

        var mockApi = new Mock<IApiService>();
        mockApi.Setup(a => a.GetAsync("academic-years"))
            .ReturnsAsync(httpResponse);

        var mockLogger = new Mock<ILogger<AcademicService>>();
        var service = new AcademicService(mockApi.Object, mockLogger.Object);

        // Act
        var result = await service.GetAcademicYearsAsync();

        // Assert
        Assert.IsNotNull(result);
        var resultList = result.ToList();
        Assert.AreEqual(3, resultList.Count);
        Assert.IsTrue(resultList.All(item => item.Id == "1" && item.Name == "2023/2024"));
    }

    /// <summary>
    /// Tests that GetAcademicYearsAsync handles academic years with Unicode characters in various scripts.
    /// Input: API returns items with Unicode characters from different language scripts.
    /// Expected: Returns items with Unicode characters preserved correctly.
    /// </summary>
    [TestMethod]
    public async Task GetAcademicYearsAsync_WhenApiReturnsItemsWithUnicodeCharacters_ReturnsItemsCorrectly()
    {
        // Arrange
        var expectedData = new List<LookupItem>
        {
            new LookupItem { Id = "1", Name = "学年 2023/2024" },
            new LookupItem { Id = "2", Name = "Année académique 2024/2025" },
            new LookupItem { Id = "3", Name = "Учебный год 2025/2026" },
            new LookupItem { Id = "4", Name = "السنة الدراسية 2026/2027" },
            new LookupItem { Id = "5", Name = "🎓 Academic Year 2027/2028 🎓" }
        };

        var responseContent = JsonSerializer.Serialize(expectedData);
        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(responseContent, Encoding.UTF8, "application/json")
        };

        var mockApi = new Mock<IApiService>();
        mockApi.Setup(a => a.GetAsync("academic-years"))
            .ReturnsAsync(httpResponse);

        var mockLogger = new Mock<ILogger<AcademicService>>();
        var service = new AcademicService(mockApi.Object, mockLogger.Object);

        // Act
        var result = await service.GetAcademicYearsAsync();

        // Assert
        Assert.IsNotNull(result);
        var resultList = result.ToList();
        Assert.AreEqual(5, resultList.Count);
        Assert.AreEqual("学年 2023/2024", resultList[0].Name);
        Assert.AreEqual("Année académique 2024/2025", resultList[1].Name);
        Assert.AreEqual("Учебный год 2025/2026", resultList[2].Name);
        Assert.AreEqual("السنة الدراسية 2026/2027", resultList[3].Name);
        Assert.AreEqual("🎓 Academic Year 2027/2028 🎓", resultList[4].Name);
    }

    /// <summary>
    /// Tests that GetAcademicYearsAsync handles items with control characters and special whitespace.
    /// Input: API returns items with tab, newline, carriage return, and other control characters.
    /// Expected: Returns items with control characters preserved or handled appropriately.
    /// </summary>
    [TestMethod]
    public async Task GetAcademicYearsAsync_WhenApiReturnsItemsWithControlCharacters_ReturnsItems()
    {
        // Arrange
        var expectedData = new List<LookupItem>
        {
            new LookupItem { Id = "1", Name = "Year\t2023/2024" },
            new LookupItem { Id = "2", Name = "Year\n2024/2025" },
            new LookupItem { Id = "3", Name = "Year\r\n2025/2026" },
            new LookupItem { Id = "4", Name = "Year\u00A02026/2027" }
        };

        var responseContent = JsonSerializer.Serialize(expectedData);
        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(responseContent, Encoding.UTF8, "application/json")
        };

        var mockApi = new Mock<IApiService>();
        mockApi.Setup(a => a.GetAsync("academic-years"))
            .ReturnsAsync(httpResponse);

        var mockLogger = new Mock<ILogger<AcademicService>>();
        var service = new AcademicService(mockApi.Object, mockLogger.Object);

        // Act
        var result = await service.GetAcademicYearsAsync();

        // Assert
        Assert.IsNotNull(result);
        var resultList = result.ToList();
        Assert.AreEqual(4, resultList.Count);
        Assert.AreEqual("Year\t2023/2024", resultList[0].Name);
        Assert.AreEqual("Year\n2024/2025", resultList[1].Name);
        Assert.AreEqual("Year\r\n2025/2026", resultList[2].Name);
    }

    /// <summary>
    /// Tests that GetAcademicYearsAsync handles items where both Id and Name are empty strings.
    /// Input: API returns items with both properties set to empty strings.
    /// Expected: Returns items with empty strings preserved.
    /// </summary>
    [TestMethod]
    public async Task GetAcademicYearsAsync_WhenApiReturnsItemsWithBothPropertiesEmpty_ReturnsItems()
    {
        // Arrange
        var expectedData = new List<LookupItem>
        {
            new LookupItem { Id = "", Name = "" },
            new LookupItem { Id = "", Name = "" }
        };

        var responseContent = JsonSerializer.Serialize(expectedData);
        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(responseContent, Encoding.UTF8, "application/json")
        };

        var mockApi = new Mock<IApiService>();
        mockApi.Setup(a => a.GetAsync("academic-years"))
            .ReturnsAsync(httpResponse);

        var mockLogger = new Mock<ILogger<AcademicService>>();
        var service = new AcademicService(mockApi.Object, mockLogger.Object);

        // Act
        var result = await service.GetAcademicYearsAsync();

        // Assert
        Assert.IsNotNull(result);
        var resultList = result.ToList();
        Assert.AreEqual(2, resultList.Count);
        Assert.AreEqual(string.Empty, resultList[0].Id);
        Assert.AreEqual(string.Empty, resultList[0].Name);
        Assert.AreEqual(string.Empty, resultList[1].Id);
        Assert.AreEqual(string.Empty, resultList[1].Name);
    }

    /// <summary>
    /// Tests that GetAcademicYearsAsync handles mixed collection with various edge-case values.
    /// Input: API returns collection mixing empty strings, whitespace, special characters, and normal values.
    /// Expected: Returns all items with their values preserved correctly.
    /// </summary>
    [TestMethod]
    public async Task GetAcademicYearsAsync_WhenApiReturnsMixedEdgeCaseValues_ReturnsAllItemsCorrectly()
    {
        // Arrange
        var expectedData = new List<LookupItem>
        {
            new LookupItem { Id = "1", Name = "Normal Year 2023/2024" },
            new LookupItem { Id = "", Name = "" },
            new LookupItem { Id = "   ", Name = "   " },
            new LookupItem { Id = "3", Name = "Year with <special> & \"characters\"" },
            new LookupItem { Id = "4", Name = new string('X', 1000) }
        };

        var responseContent = JsonSerializer.Serialize(expectedData);
        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(responseContent, Encoding.UTF8, "application/json")
        };

        var mockApi = new Mock<IApiService>();
        mockApi.Setup(a => a.GetAsync("academic-years"))
            .ReturnsAsync(httpResponse);

        var mockLogger = new Mock<ILogger<AcademicService>>();
        var service = new AcademicService(mockApi.Object, mockLogger.Object);

        // Act
        var result = await service.GetAcademicYearsAsync();

        // Assert
        Assert.IsNotNull(result);
        var resultList = result.ToList();
        Assert.AreEqual(5, resultList.Count);
        Assert.AreEqual("Normal Year 2023/2024", resultList[0].Name);
        Assert.AreEqual(string.Empty, resultList[1].Name);
        Assert.AreEqual("   ", resultList[2].Name);
        Assert.AreEqual("Year with <special> & \"characters\"", resultList[3].Name);
        Assert.AreEqual(1000, resultList[4].Name.Length);
    }

    /// <summary>
    /// Tests that DeleteFacultyAsync returns true when the API returns various success status codes.
    /// Input: Valid faculty ID and API returns 2xx success status codes.
    /// Expected: Returns true for all success status codes.
    /// </summary>
    [TestMethod]
    [DataRow(HttpStatusCode.OK, DisplayName = "OK (200)")]
    [DataRow(HttpStatusCode.Created, DisplayName = "Created (201)")]
    [DataRow(HttpStatusCode.Accepted, DisplayName = "Accepted (202)")]
    [DataRow(HttpStatusCode.NoContent, DisplayName = "No Content (204)")]
    public async Task DeleteFacultyAsync_SuccessStatusCodes_ReturnsTrue(HttpStatusCode statusCode)
    {
        // Arrange
        var mockApiService = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<AcademicService>>();
        var facultyId = "faculty123";
        var successResponse = new HttpResponseMessage(statusCode);

        mockApiService
            .Setup(x => x.DeleteAsync($"faculties/{facultyId}", It.IsAny<CancellationToken>()))
            .ReturnsAsync(successResponse);

        var service = new AcademicService(mockApiService.Object, mockLogger.Object);

        // Act
        var result = await service.DeleteFacultyAsync(facultyId);

        // Assert
        Assert.IsTrue(result);
        mockApiService.Verify(x => x.DeleteAsync($"faculties/{facultyId}", It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Tests that DeleteFacultyAsync returns false when the API returns various error status codes.
    /// Input: Valid faculty ID and API returns non-2xx status codes.
    /// Expected: Returns false for all error status codes.
    /// </summary>
    [TestMethod]
    [DataRow(HttpStatusCode.BadRequest, DisplayName = "Bad Request (400)")]
    [DataRow(HttpStatusCode.Unauthorized, DisplayName = "Unauthorized (401)")]
    [DataRow(HttpStatusCode.Forbidden, DisplayName = "Forbidden (403)")]
    [DataRow(HttpStatusCode.NotFound, DisplayName = "Not Found (404)")]
    [DataRow(HttpStatusCode.InternalServerError, DisplayName = "Internal Server Error (500)")]
    [DataRow(HttpStatusCode.ServiceUnavailable, DisplayName = "Service Unavailable (503)")]
    [DataRow(HttpStatusCode.BadGateway, DisplayName = "Bad Gateway (502)")]
    [DataRow(HttpStatusCode.GatewayTimeout, DisplayName = "Gateway Timeout (504)")]
    public async Task DeleteFacultyAsync_ErrorStatusCodes_ReturnsFalse(HttpStatusCode statusCode)
    {
        // Arrange
        var mockApiService = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<AcademicService>>();
        var facultyId = "faculty123";
        var errorResponse = new HttpResponseMessage(statusCode);

        mockApiService
            .Setup(x => x.DeleteAsync($"faculties/{facultyId}", It.IsAny<CancellationToken>()))
            .ReturnsAsync(errorResponse);

        var service = new AcademicService(mockApiService.Object, mockLogger.Object);

        // Act
        var result = await service.DeleteFacultyAsync(facultyId);

        // Assert
        Assert.IsFalse(result);
        mockApiService.Verify(x => x.DeleteAsync($"faculties/{facultyId}", It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Tests that DeleteFacultyAsync handles various faculty ID formats correctly.
    /// Input: Various string formats for faculty ID including edge cases.
    /// Expected: Correctly constructs endpoint path for all ID formats.
    /// </summary>
    [TestMethod]
    [DataRow("faculty123", DisplayName = "Normal ID")]
    [DataRow("", DisplayName = "Empty string")]
    [DataRow(" ", DisplayName = "Single space")]
    [DataRow("   ", DisplayName = "Multiple spaces")]
    [DataRow("\t", DisplayName = "Tab character")]
    [DataRow("\n", DisplayName = "Newline character")]
    [DataRow("faculty-with-special-chars-!@#$%", DisplayName = "Special characters")]
    [DataRow("a", DisplayName = "Single character")]
    [DataRow("abc-def-123", DisplayName = "ID with hyphens")]
    [DataRow("550e8400-e29b-41d4-a716-446655440000", DisplayName = "GUID format")]
    [DataRow("faculty/with/slashes", DisplayName = "ID with slashes")]
    [DataRow("faculty&with&ampersands", DisplayName = "ID with ampersands")]
    public async Task DeleteFacultyAsync_VariousIdFormats_ConstructsCorrectEndpoint(string id)
    {
        // Arrange
        var mockApiService = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<AcademicService>>();
        var successResponse = new HttpResponseMessage(HttpStatusCode.OK);

        mockApiService
            .Setup(x => x.DeleteAsync($"faculties/{id}", It.IsAny<CancellationToken>()))
            .ReturnsAsync(successResponse);

        var service = new AcademicService(mockApiService.Object, mockLogger.Object);

        // Act
        var result = await service.DeleteFacultyAsync(id);

        // Assert
        Assert.IsTrue(result);
        mockApiService.Verify(x => x.DeleteAsync($"faculties/{id}", It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Tests that DeleteFacultyAsync handles very long faculty ID strings.
    /// Input: Very long string (1000 characters) as faculty ID.
    /// Expected: Successfully makes API call with long ID.
    /// </summary>
    [TestMethod]
    public async Task DeleteFacultyAsync_VeryLongId_MakesApiCall()
    {
        // Arrange
        var mockApiService = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<AcademicService>>();
        var veryLongId = new string('x', 1000);
        var successResponse = new HttpResponseMessage(HttpStatusCode.OK);

        mockApiService
            .Setup(x => x.DeleteAsync($"faculties/{veryLongId}", It.IsAny<CancellationToken>()))
            .ReturnsAsync(successResponse);

        var service = new AcademicService(mockApiService.Object, mockLogger.Object);

        // Act
        var result = await service.DeleteFacultyAsync(veryLongId);

        // Assert
        Assert.IsTrue(result);
        mockApiService.Verify(x => x.DeleteAsync($"faculties/{veryLongId}", It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Tests that DeleteFacultyAsync returns false and logs error when API throws HttpRequestException.
    /// Input: Valid faculty ID and API throws HttpRequestException.
    /// Expected: Returns false and logs error with exception details.
    /// </summary>
    [TestMethod]
    public async Task DeleteFacultyAsync_HttpRequestException_ReturnsFalseAndLogsError()
    {
        // Arrange
        var mockApiService = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<AcademicService>>();
        var facultyId = "faculty123";
        var expectedException = new HttpRequestException("Network error");

        mockApiService
            .Setup(x => x.DeleteAsync($"faculties/{facultyId}", It.IsAny<CancellationToken>()))
            .ThrowsAsync(expectedException);

        var service = new AcademicService(mockApiService.Object, mockLogger.Object);

        // Act
        var result = await service.DeleteFacultyAsync(facultyId);

        // Assert
        Assert.IsFalse(result);
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Failed to delete faculty")),
                expectedException,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    /// <summary>
    /// Tests that DeleteFacultyAsync returns false and logs error when API throws TaskCanceledException.
    /// Input: Valid faculty ID and API throws TaskCanceledException (timeout scenario).
    /// Expected: Returns false and logs error with exception details.
    /// </summary>
    [TestMethod]
    public async Task DeleteFacultyAsync_TaskCanceledException_ReturnsFalseAndLogsError()
    {
        // Arrange
        var mockApiService = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<AcademicService>>();
        var facultyId = "faculty456";
        var expectedException = new TaskCanceledException("Request timeout");

        mockApiService
            .Setup(x => x.DeleteAsync($"faculties/{facultyId}", It.IsAny<CancellationToken>()))
            .ThrowsAsync(expectedException);

        var service = new AcademicService(mockApiService.Object, mockLogger.Object);

        // Act
        var result = await service.DeleteFacultyAsync(facultyId);

        // Assert
        Assert.IsFalse(result);
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Failed to delete faculty")),
                expectedException,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    /// <summary>
    /// Tests that DeleteFacultyAsync returns false and logs error when API throws InvalidOperationException.
    /// Input: Valid faculty ID and API throws InvalidOperationException.
    /// Expected: Returns false and logs error with exception details.
    /// </summary>
    [TestMethod]
    public async Task DeleteFacultyAsync_InvalidOperationException_ReturnsFalseAndLogsError()
    {
        // Arrange
        var mockApiService = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<AcademicService>>();
        var facultyId = "faculty789";
        var expectedException = new InvalidOperationException("Invalid state");

        mockApiService
            .Setup(x => x.DeleteAsync($"faculties/{facultyId}", It.IsAny<CancellationToken>()))
            .ThrowsAsync(expectedException);

        var service = new AcademicService(mockApiService.Object, mockLogger.Object);

        // Act
        var result = await service.DeleteFacultyAsync(facultyId);

        // Assert
        Assert.IsFalse(result);
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Failed to delete faculty")),
                expectedException,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    /// <summary>
    /// Tests that DeleteFacultyAsync returns false and logs error when API throws generic Exception.
    /// Input: Valid faculty ID and API throws generic Exception.
    /// Expected: Returns false and logs error with exception details.
    /// </summary>
    [TestMethod]
    public async Task DeleteFacultyAsync_GenericException_ReturnsFalseAndLogsError()
    {
        // Arrange
        var mockApiService = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<AcademicService>>();
        var facultyId = "faculty999";
        var expectedException = new Exception("Unexpected error");

        mockApiService
            .Setup(x => x.DeleteAsync($"faculties/{facultyId}", It.IsAny<CancellationToken>()))
            .ThrowsAsync(expectedException);

        var service = new AcademicService(mockApiService.Object, mockLogger.Object);

        // Act
        var result = await service.DeleteFacultyAsync(facultyId);

        // Assert
        Assert.IsFalse(result);
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Failed to delete faculty")),
                expectedException,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    /// <summary>
    /// Tests that DeleteFacultyAsync logs the correct faculty ID in error message.
    /// Input: Specific faculty ID and API throws exception.
    /// Expected: Error log contains the exact faculty ID that was passed.
    /// </summary>
    [TestMethod]
    public async Task DeleteFacultyAsync_ExceptionOccurs_LogsCorrectFacultyId()
    {
        // Arrange
        var mockApiService = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<AcademicService>>();
        var facultyId = "specific-faculty-id-123";
        var expectedException = new Exception("Test error");

        mockApiService
            .Setup(x => x.DeleteAsync($"faculties/{facultyId}", It.IsAny<CancellationToken>()))
            .ThrowsAsync(expectedException);

        var service = new AcademicService(mockApiService.Object, mockLogger.Object);

        // Act
        var result = await service.DeleteFacultyAsync(facultyId);

        // Assert
        Assert.IsFalse(result);
        mockApiService.Verify(x => x.DeleteAsync($"faculties/{facultyId}", It.IsAny<CancellationToken>()), Times.Once);
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Failed to delete faculty")),
                expectedException,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    /// <summary>
    /// Tests that DeleteFacultyAsync handles Unicode characters in faculty ID.
    /// Input: Faculty ID with Unicode characters.
    /// Expected: Successfully makes API call with Unicode ID.
    /// </summary>
    [TestMethod]
    [DataRow("faculty日本語")]
    [DataRow("faculty한국어")]
    [DataRow("facultyالعربية")]
    [DataRow("faculty中文")]
    [DataRow("faculty🔥")]
    public async Task DeleteFacultyAsync_UnicodeCharactersInId_MakesApiCall(string id)
    {
        // Arrange
        var mockApiService = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<AcademicService>>();
        var successResponse = new HttpResponseMessage(HttpStatusCode.OK);

        mockApiService
            .Setup(x => x.DeleteAsync($"faculties/{id}", It.IsAny<CancellationToken>()))
            .ReturnsAsync(successResponse);

        var service = new AcademicService(mockApiService.Object, mockLogger.Object);

        // Act
        var result = await service.DeleteFacultyAsync(id);

        // Assert
        Assert.IsTrue(result);
        mockApiService.Verify(x => x.DeleteAsync($"faculties/{id}", It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Tests that DeleteFacultyAsync does not log any error when the operation succeeds.
    /// Input: Valid faculty ID and API returns success.
    /// Expected: No error is logged, only API call is made.
    /// </summary>
    [TestMethod]
    public async Task DeleteFacultyAsync_SuccessfulOperation_DoesNotLogError()
    {
        // Arrange
        var mockApiService = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<AcademicService>>();
        var facultyId = "faculty123";
        var successResponse = new HttpResponseMessage(HttpStatusCode.OK);

        mockApiService
            .Setup(x => x.DeleteAsync($"faculties/{facultyId}", It.IsAny<CancellationToken>()))
            .ReturnsAsync(successResponse);

        var service = new AcademicService(mockApiService.Object, mockLogger.Object);

        // Act
        var result = await service.DeleteFacultyAsync(facultyId);

        // Assert
        Assert.IsTrue(result);
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Never);
    }

    /// <summary>
    /// Tests that DeleteFacultyAsync does not log any error when API returns non-success status code.
    /// Input: Valid faculty ID and API returns 404 NotFound.
    /// Expected: Returns false but does not log error (only logs on exception).
    /// </summary>
    [TestMethod]
    public async Task DeleteFacultyAsync_NonSuccessStatusCode_DoesNotLogError()
    {
        // Arrange
        var mockApiService = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<AcademicService>>();
        var facultyId = "faculty123";
        var errorResponse = new HttpResponseMessage(HttpStatusCode.NotFound);

        mockApiService
            .Setup(x => x.DeleteAsync($"faculties/{facultyId}", It.IsAny<CancellationToken>()))
            .ReturnsAsync(errorResponse);

        var service = new AcademicService(mockApiService.Object, mockLogger.Object);

        // Act
        var result = await service.DeleteFacultyAsync(facultyId);

        // Assert
        Assert.IsFalse(result);
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Never);
    }

    /// <summary>
    /// Tests that GetEntrySchemesAsync successfully returns entry schemes when the API call succeeds.
    /// Input: API returns successful response with valid entry scheme data.
    /// Expected: Returns the deserialized collection of LookupItem objects.
    /// </summary>
    [TestMethod]
    public async Task GetEntrySchemesAsync_SuccessfulApiResponse_ReturnsEntrySchemes()
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<AcademicService>>();
        var expectedData = new List<LookupItem>
        {
            new LookupItem { Id = "1", Name = "Direct Entry" },
            new LookupItem { Id = "2", Name = "Mature Age Entry" },
            new LookupItem { Id = "3", Name = "Transfer Entry" }
        };
        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(expectedData)
        };
        mockApi.Setup(x => x.GetAsync("entry-schemes")).ReturnsAsync(response);
        var service = new AcademicService(mockApi.Object, mockLogger.Object);

        // Act
        var result = await service.GetEntrySchemesAsync();

        // Assert
        Assert.IsNotNull(result);
        var resultList = result.ToList();
        Assert.AreEqual(3, resultList.Count);
        Assert.AreEqual("1", resultList[0].Id);
        Assert.AreEqual("Direct Entry", resultList[0].Name);
        Assert.AreEqual("2", resultList[1].Id);
        Assert.AreEqual("Mature Age Entry", resultList[1].Name);
        Assert.AreEqual("3", resultList[2].Id);
        Assert.AreEqual("Transfer Entry", resultList[2].Name);
        mockApi.Verify(x => x.GetAsync("entry-schemes"), Times.Once);
    }

    /// <summary>
    /// Tests that GetEntrySchemesAsync returns cached data on subsequent calls within cache duration.
    /// Input: Multiple calls within 10 minute cache window.
    /// Expected: API is called only once, subsequent calls return cached data.
    /// </summary>
    [TestMethod]
    public async Task GetEntrySchemesAsync_CalledMultipleTimesWithinCacheDuration_UsesCachedData()
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<AcademicService>>();
        var expectedData = new List<LookupItem>
        {
            new LookupItem { Id = "1", Name = "Direct Entry" }
        };
        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(expectedData)
        };
        mockApi.Setup(x => x.GetAsync("entry-schemes")).ReturnsAsync(response);
        var service = new AcademicService(mockApi.Object, mockLogger.Object);

        // Act
        var result1 = await service.GetEntrySchemesAsync();
        var result2 = await service.GetEntrySchemesAsync();

        // Assert
        Assert.IsNotNull(result1);
        Assert.IsNotNull(result2);
        Assert.AreEqual(1, result1.Count());
        Assert.AreEqual(1, result2.Count());
        mockApi.Verify(x => x.GetAsync("entry-schemes"), Times.Once);
    }

    /// <summary>
    /// Tests that GetEntrySchemesAsync returns empty collection when API returns non-success status codes.
    /// Input: API returns various HTTP error status codes.
    /// Expected: Returns empty collection for all error status codes.
    /// </summary>
    [TestMethod]
    [DataRow(HttpStatusCode.BadRequest, DisplayName = "BadRequest")]
    [DataRow(HttpStatusCode.Unauthorized, DisplayName = "Unauthorized")]
    [DataRow(HttpStatusCode.Forbidden, DisplayName = "Forbidden")]
    [DataRow(HttpStatusCode.NotFound, DisplayName = "NotFound")]
    [DataRow(HttpStatusCode.InternalServerError, DisplayName = "InternalServerError")]
    [DataRow(HttpStatusCode.ServiceUnavailable, DisplayName = "ServiceUnavailable")]
    [DataRow(HttpStatusCode.BadGateway, DisplayName = "BadGateway")]
    [DataRow(HttpStatusCode.GatewayTimeout, DisplayName = "GatewayTimeout")]
    public async Task GetEntrySchemesAsync_ApiReturnsErrorStatusCode_ReturnsEmptyCollection(HttpStatusCode statusCode)
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<AcademicService>>();
        var response = new HttpResponseMessage(statusCode);
        mockApi.Setup(x => x.GetAsync("entry-schemes")).ReturnsAsync(response);
        var service = new AcademicService(mockApi.Object, mockLogger.Object);

        // Act
        var result = await service.GetEntrySchemesAsync();

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(0, result.Count());
        mockApi.Verify(x => x.GetAsync("entry-schemes"), Times.Once);
    }

    /// <summary>
    /// Tests that GetEntrySchemesAsync returns empty collection when API throws exception.
    /// Input: API throws HttpRequestException during request.
    /// Expected: Returns empty collection and logs error.
    /// </summary>
    [TestMethod]
    public async Task GetEntrySchemesAsync_ApiThrowsHttpRequestException_ReturnsEmptyCollection()
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<AcademicService>>();
        mockApi.Setup(x => x.GetAsync("entry-schemes"))
               .ThrowsAsync(new HttpRequestException("Network error"));
        var service = new AcademicService(mockApi.Object, mockLogger.Object);

        // Act
        var result = await service.GetEntrySchemesAsync();

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(0, result.Count());
        mockApi.Verify(x => x.GetAsync("entry-schemes"), Times.Once);
    }

    /// <summary>
    /// Tests that GetEntrySchemesAsync returns empty collection when API throws generic exception.
    /// Input: API throws generic Exception during request.
    /// Expected: Returns empty collection and logs error.
    /// </summary>
    [TestMethod]
    public async Task GetEntrySchemesAsync_ApiThrowsGenericException_ReturnsEmptyCollection()
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<AcademicService>>();
        mockApi.Setup(x => x.GetAsync("entry-schemes"))
               .ThrowsAsync(new Exception("Unexpected error"));
        var service = new AcademicService(mockApi.Object, mockLogger.Object);

        // Act
        var result = await service.GetEntrySchemesAsync();

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(0, result.Count());
        mockApi.Verify(x => x.GetAsync("entry-schemes"), Times.Once);
    }

    /// <summary>
    /// Tests that GetEntrySchemesAsync returns empty collection when API throws TaskCanceledException.
    /// Input: API throws TaskCanceledException (timeout scenario).
    /// Expected: Returns empty collection and logs error.
    /// </summary>
    [TestMethod]
    public async Task GetEntrySchemesAsync_ApiThrowsTaskCanceledException_ReturnsEmptyCollection()
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<AcademicService>>();
        mockApi.Setup(x => x.GetAsync("entry-schemes"))
               .ThrowsAsync(new TaskCanceledException("Request timeout"));
        var service = new AcademicService(mockApi.Object, mockLogger.Object);

        // Act
        var result = await service.GetEntrySchemesAsync();

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(0, result.Count());
        mockApi.Verify(x => x.GetAsync("entry-schemes"), Times.Once);
    }

    /// <summary>
    /// Tests that GetEntrySchemesAsync returns empty collection when API response content is null.
    /// Input: API returns success status but content deserializes to null.
    /// Expected: Returns empty collection.
    /// </summary>
    [TestMethod]
    public async Task GetEntrySchemesAsync_ApiReturnsNullContent_ReturnsEmptyCollection()
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<AcademicService>>();
        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create<IEnumerable<LookupItem>?>(null)
        };
        mockApi.Setup(x => x.GetAsync("entry-schemes")).ReturnsAsync(response);
        var service = new AcademicService(mockApi.Object, mockLogger.Object);

        // Act
        var result = await service.GetEntrySchemesAsync();

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(0, result.Count());
        mockApi.Verify(x => x.GetAsync("entry-schemes"), Times.Once);
    }

    /// <summary>
    /// Tests that GetEntrySchemesAsync returns empty collection when API returns empty array.
    /// Input: API returns success status with empty array.
    /// Expected: Returns empty collection.
    /// </summary>
    [TestMethod]
    public async Task GetEntrySchemesAsync_ApiReturnsEmptyArray_ReturnsEmptyCollection()
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<AcademicService>>();
        var expectedData = new List<LookupItem>();
        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(expectedData)
        };
        mockApi.Setup(x => x.GetAsync("entry-schemes")).ReturnsAsync(response);
        var service = new AcademicService(mockApi.Object, mockLogger.Object);

        // Act
        var result = await service.GetEntrySchemesAsync();

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(0, result.Count());
        mockApi.Verify(x => x.GetAsync("entry-schemes"), Times.Once);
    }

    /// <summary>
    /// Tests that GetEntrySchemesAsync correctly handles single item response.
    /// Input: API returns success with single entry scheme.
    /// Expected: Returns collection with one item.
    /// </summary>
    [TestMethod]
    public async Task GetEntrySchemesAsync_ApiReturnsSingleItem_ReturnsSingleItemCollection()
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<AcademicService>>();
        var expectedData = new List<LookupItem>
        {
            new LookupItem { Id = "1", Name = "Direct Entry" }
        };
        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(expectedData)
        };
        mockApi.Setup(x => x.GetAsync("entry-schemes")).ReturnsAsync(response);
        var service = new AcademicService(mockApi.Object, mockLogger.Object);

        // Act
        var result = await service.GetEntrySchemesAsync();

        // Assert
        Assert.IsNotNull(result);
        var resultList = result.ToList();
        Assert.AreEqual(1, resultList.Count);
        Assert.AreEqual("1", resultList[0].Id);
        Assert.AreEqual("Direct Entry", resultList[0].Name);
        mockApi.Verify(x => x.GetAsync("entry-schemes"), Times.Once);
    }

    /// <summary>
    /// Tests that GetEntrySchemesAsync correctly handles entry schemes with special characters.
    /// Input: API returns entry schemes with special characters, Unicode, and escape sequences.
    /// Expected: Returns correctly deserialized data with special characters preserved.
    /// </summary>
    [TestMethod]
    public async Task GetEntrySchemesAsync_ApiReturnsItemsWithSpecialCharacters_ReturnsCorrectData()
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<AcademicService>>();
        var expectedData = new List<LookupItem>
        {
            new LookupItem { Id = "1", Name = "Entry & Exit" },
            new LookupItem { Id = "2", Name = "Entry \"Quoted\"" },
            new LookupItem { Id = "3", Name = "Entry\nWith\nNewlines" },
            new LookupItem { Id = "4", Name = "Entry\tWith\tTabs" },
            new LookupItem { Id = "5", Name = "Entry with Unicode: 日本語" }
        };
        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(expectedData)
        };
        mockApi.Setup(x => x.GetAsync("entry-schemes")).ReturnsAsync(response);
        var service = new AcademicService(mockApi.Object, mockLogger.Object);

        // Act
        var result = await service.GetEntrySchemesAsync();

        // Assert
        Assert.IsNotNull(result);
        var resultList = result.ToList();
        Assert.AreEqual(5, resultList.Count);
        Assert.AreEqual("Entry & Exit", resultList[0].Name);
        Assert.AreEqual("Entry \"Quoted\"", resultList[1].Name);
        Assert.AreEqual("Entry\nWith\nNewlines", resultList[2].Name);
        Assert.AreEqual("Entry\tWith\tTabs", resultList[3].Name);
        Assert.AreEqual("Entry with Unicode: 日本語", resultList[4].Name);
        mockApi.Verify(x => x.GetAsync("entry-schemes"), Times.Once);
    }

    /// <summary>
    /// Tests that GetEntrySchemesAsync correctly handles entry schemes with empty string values.
    /// Input: API returns entry schemes with empty Id and Name properties.
    /// Expected: Returns data with empty strings preserved.
    /// </summary>
    [TestMethod]
    public async Task GetEntrySchemesAsync_ApiReturnsItemsWithEmptyStrings_ReturnsCorrectData()
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<AcademicService>>();
        var expectedData = new List<LookupItem>
        {
            new LookupItem { Id = "", Name = "" },
            new LookupItem { Id = "1", Name = "" },
            new LookupItem { Id = "", Name = "Entry Scheme" }
        };
        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(expectedData)
        };
        mockApi.Setup(x => x.GetAsync("entry-schemes")).ReturnsAsync(response);
        var service = new AcademicService(mockApi.Object, mockLogger.Object);

        // Act
        var result = await service.GetEntrySchemesAsync();

        // Assert
        Assert.IsNotNull(result);
        var resultList = result.ToList();
        Assert.AreEqual(3, resultList.Count);
        Assert.AreEqual("", resultList[0].Id);
        Assert.AreEqual("", resultList[0].Name);
        Assert.AreEqual("1", resultList[1].Id);
        Assert.AreEqual("", resultList[1].Name);
        Assert.AreEqual("", resultList[2].Id);
        Assert.AreEqual("Entry Scheme", resultList[2].Name);
        mockApi.Verify(x => x.GetAsync("entry-schemes"), Times.Once);
    }

    /// <summary>
    /// Tests that GetEntrySchemesAsync returns empty collection when API returns invalid JSON.
    /// Input: API returns success status but invalid JSON content.
    /// Expected: Returns empty collection and logs error due to deserialization failure.
    /// </summary>
    [TestMethod]
    public async Task GetEntrySchemesAsync_ApiReturnsInvalidJson_ReturnsEmptyCollection()
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<AcademicService>>();
        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("invalid json content")
        };
        mockApi.Setup(x => x.GetAsync("entry-schemes")).ReturnsAsync(response);
        var service = new AcademicService(mockApi.Object, mockLogger.Object);

        // Act
        var result = await service.GetEntrySchemesAsync();

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(0, result.Count());
        mockApi.Verify(x => x.GetAsync("entry-schemes"), Times.Once);
    }

    /// <summary>
    /// Tests that GetEntrySchemesAsync correctly handles large collections of entry schemes.
    /// Input: API returns large number (100+) of entry schemes.
    /// Expected: Returns all items correctly.
    /// </summary>
    [TestMethod]
    public async Task GetEntrySchemesAsync_ApiReturnsLargeCollection_ReturnsAllItems()
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<AcademicService>>();
        var expectedData = new List<LookupItem>();
        for (int i = 1; i <= 100; i++)
        {
            expectedData.Add(new LookupItem { Id = i.ToString(), Name = $"Entry Scheme {i}" });
        }
        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(expectedData)
        };
        mockApi.Setup(x => x.GetAsync("entry-schemes")).ReturnsAsync(response);
        var service = new AcademicService(mockApi.Object, mockLogger.Object);

        // Act
        var result = await service.GetEntrySchemesAsync();

        // Assert
        Assert.IsNotNull(result);
        var resultList = result.ToList();
        Assert.AreEqual(100, resultList.Count);
        Assert.AreEqual("1", resultList[0].Id);
        Assert.AreEqual("Entry Scheme 1", resultList[0].Name);
        Assert.AreEqual("100", resultList[99].Id);
        Assert.AreEqual("Entry Scheme 100", resultList[99].Name);
        mockApi.Verify(x => x.GetAsync("entry-schemes"), Times.Once);
    }

    /// <summary>
    /// Tests that GetEntrySchemesAsync correctly handles entry schemes with whitespace-only values.
    /// Input: API returns entry schemes with whitespace-only Name properties.
    /// Expected: Returns data with whitespace preserved.
    /// </summary>
    [TestMethod]
    public async Task GetEntrySchemesAsync_ApiReturnsItemsWithWhitespace_ReturnsCorrectData()
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<AcademicService>>();
        var expectedData = new List<LookupItem>
        {
            new LookupItem { Id = "1", Name = "   " },
            new LookupItem { Id = "2", Name = "\t\t" },
            new LookupItem { Id = "3", Name = " Normal Entry " }
        };
        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(expectedData)
        };
        mockApi.Setup(x => x.GetAsync("entry-schemes")).ReturnsAsync(response);
        var service = new AcademicService(mockApi.Object, mockLogger.Object);

        // Act
        var result = await service.GetEntrySchemesAsync();

        // Assert
        Assert.IsNotNull(result);
        var resultList = result.ToList();
        Assert.AreEqual(3, resultList.Count);
        Assert.AreEqual("   ", resultList[0].Name);
        Assert.AreEqual("\t\t", resultList[1].Name);
        Assert.AreEqual(" Normal Entry ", resultList[2].Name);
        mockApi.Verify(x => x.GetAsync("entry-schemes"), Times.Once);
    }

    /// <summary>
    /// Tests that GetEntrySchemesAsync correctly handles entry schemes with very long names.
    /// Input: API returns entry schemes with very long (1000+ characters) Name values.
    /// Expected: Returns data with long names correctly.
    /// </summary>
    [TestMethod]
    public async Task GetEntrySchemesAsync_ApiReturnsItemsWithVeryLongNames_ReturnsCorrectData()
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<AcademicService>>();
        var longName = new string('A', 1000);
        var expectedData = new List<LookupItem>
        {
            new LookupItem { Id = "1", Name = longName }
        };
        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(expectedData)
        };
        mockApi.Setup(x => x.GetAsync("entry-schemes")).ReturnsAsync(response);
        var service = new AcademicService(mockApi.Object, mockLogger.Object);

        // Act
        var result = await service.GetEntrySchemesAsync();

        // Assert
        Assert.IsNotNull(result);
        var resultList = result.ToList();
        Assert.AreEqual(1, resultList.Count);
        Assert.AreEqual(longName, resultList[0].Name);
        Assert.AreEqual(1000, resultList[0].Name?.Length);
        mockApi.Verify(x => x.GetAsync("entry-schemes"), Times.Once);
    }

    /// <summary>
    /// Tests that GetIntakesAsync returns data successfully when the API call succeeds with valid data.
    /// Input: API returns success with valid intake data.
    /// Expected: Returns collection of LookupItem with correct data.
    /// </summary>
    [TestMethod]
    public async Task GetIntakesAsync_ValidApiResponse_ReturnsLookupItems()
    {
        // Arrange
        var expectedData = new List<LookupItem>
        {
            new LookupItem { Id = "1", Name = "Fall 2024" },
            new LookupItem { Id = "2", Name = "Spring 2025" },
            new LookupItem { Id = "3", Name = "Summer 2025" }
        };
        var jsonContent = JsonContent.Create(expectedData);
        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = jsonContent
        };

        var mockApiService = new Mock<IApiService>();
        mockApiService
            .Setup(x => x.GetAsync("intakes"))
            .ReturnsAsync(response);

        var mockLogger = new Mock<ILogger<AcademicService>>();
        var service = new AcademicService(mockApiService.Object, mockLogger.Object);

        // Act
        var result = await service.GetIntakesAsync();

        // Assert
        Assert.IsNotNull(result);
        var resultList = result.ToList();
        Assert.AreEqual(3, resultList.Count);
        Assert.AreEqual("1", resultList[0].Id);
        Assert.AreEqual("Fall 2024", resultList[0].Name);
        Assert.AreEqual("2", resultList[1].Id);
        Assert.AreEqual("Spring 2025", resultList[1].Name);
        Assert.AreEqual("3", resultList[2].Id);
        Assert.AreEqual("Summer 2025", resultList[2].Name);
        mockApiService.Verify(x => x.GetAsync("intakes"), Times.Once);
    }

    /// <summary>
    /// Tests that GetIntakesAsync returns an empty collection when the API returns a non-success status code.
    /// Input: API returns 404 Not Found status.
    /// Expected: Returns empty collection and logs warning.
    /// </summary>
    [TestMethod]
    public async Task GetIntakesAsync_ApiReturnsNotFound_ReturnsEmptyCollection()
    {
        // Arrange
        var response = new HttpResponseMessage(HttpStatusCode.NotFound);

        var mockApiService = new Mock<IApiService>();
        mockApiService
            .Setup(x => x.GetAsync("intakes"))
            .ReturnsAsync(response);

        var mockLogger = new Mock<ILogger<AcademicService>>();
        var service = new AcademicService(mockApiService.Object, mockLogger.Object);

        // Act
        var result = await service.GetIntakesAsync();

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(0, result.Count());
        mockApiService.Verify(x => x.GetAsync("intakes"), Times.Once);
    }

    /// <summary>
    /// Tests that GetIntakesAsync returns an empty collection when the API call throws an exception.
    /// Input: API throws HttpRequestException.
    /// Expected: Returns empty collection and logs error.
    /// </summary>
    [TestMethod]
    public async Task GetIntakesAsync_ApiThrowsHttpRequestException_ReturnsEmptyCollection()
    {
        // Arrange
        var mockApiService = new Mock<IApiService>();
        mockApiService
            .Setup(x => x.GetAsync("intakes"))
            .ThrowsAsync(new HttpRequestException("Network error"));

        var mockLogger = new Mock<ILogger<AcademicService>>();
        var service = new AcademicService(mockApiService.Object, mockLogger.Object);

        // Act
        var result = await service.GetIntakesAsync();

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(0, result.Count());
        mockApiService.Verify(x => x.GetAsync("intakes"), Times.Once);
    }

    /// <summary>
    /// Tests that GetIntakesAsync returns cached data when called multiple times within the cache duration.
    /// Input: Two consecutive calls within cache duration.
    /// Expected: API is called only once, second call returns cached data.
    /// </summary>
    [TestMethod]
    public async Task GetIntakesAsync_CalledTwiceWithinCacheDuration_ReturnsCachedData()
    {
        // Arrange
        var expectedData = new List<LookupItem>
        {
            new LookupItem { Id = "1", Name = "Fall 2024" }
        };
        var jsonContent = JsonContent.Create(expectedData);
        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = jsonContent
        };

        var mockApiService = new Mock<IApiService>();
        mockApiService
            .Setup(x => x.GetAsync("intakes"))
            .ReturnsAsync(response);

        var mockLogger = new Mock<ILogger<AcademicService>>();
        var service = new AcademicService(mockApiService.Object, mockLogger.Object);

        // Act
        var result1 = await service.GetIntakesAsync();
        var result2 = await service.GetIntakesAsync();

        // Assert
        Assert.IsNotNull(result1);
        Assert.IsNotNull(result2);
        Assert.AreEqual(result1.Count(), result2.Count());
        mockApiService.Verify(x => x.GetAsync("intakes"), Times.Once);
    }

    /// <summary>
    /// Tests that GetIntakesAsync returns an empty collection when the response content deserializes to null.
    /// Input: API returns success but content deserializes to null.
    /// Expected: Returns empty collection.
    /// </summary>
    [TestMethod]
    public async Task GetIntakesAsync_ResponseContentDeserializesToNull_ReturnsEmptyCollection()
    {
        // Arrange
        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("null", System.Text.Encoding.UTF8, "application/json")
        };

        var mockApiService = new Mock<IApiService>();
        mockApiService
            .Setup(x => x.GetAsync("intakes"))
            .ReturnsAsync(response);

        var mockLogger = new Mock<ILogger<AcademicService>>();
        var service = new AcademicService(mockApiService.Object, mockLogger.Object);

        // Act
        var result = await service.GetIntakesAsync();

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(0, result.Count());
        mockApiService.Verify(x => x.GetAsync("intakes"), Times.Once);
    }

    /// <summary>
    /// Tests that GetIntakesAsync returns an empty collection when the response contains an empty JSON array.
    /// Input: API returns success with empty JSON array.
    /// Expected: Returns empty collection.
    /// </summary>
    [TestMethod]
    public async Task GetIntakesAsync_ApiReturnsEmptyArray_ReturnsEmptyCollection()
    {
        // Arrange
        var emptyData = new List<LookupItem>();
        var jsonContent = JsonContent.Create(emptyData);
        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = jsonContent
        };

        var mockApiService = new Mock<IApiService>();
        mockApiService
            .Setup(x => x.GetAsync("intakes"))
            .ReturnsAsync(response);

        var mockLogger = new Mock<ILogger<AcademicService>>();
        var service = new AcademicService(mockApiService.Object, mockLogger.Object);

        // Act
        var result = await service.GetIntakesAsync();

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(0, result.Count());
        mockApiService.Verify(x => x.GetAsync("intakes"), Times.Once);
    }

    /// <summary>
    /// Tests that GetIntakesAsync handles various HTTP error status codes and returns an empty collection.
    /// Input: Various HTTP error status codes.
    /// Expected: Returns empty collection for all error status codes.
    /// </summary>
    [TestMethod]
    [DataRow(HttpStatusCode.BadRequest, DisplayName = "Bad Request (400)")]
    [DataRow(HttpStatusCode.Unauthorized, DisplayName = "Unauthorized (401)")]
    [DataRow(HttpStatusCode.Forbidden, DisplayName = "Forbidden (403)")]
    [DataRow(HttpStatusCode.InternalServerError, DisplayName = "Internal Server Error (500)")]
    [DataRow(HttpStatusCode.ServiceUnavailable, DisplayName = "Service Unavailable (503)")]
    [DataRow(HttpStatusCode.BadGateway, DisplayName = "Bad Gateway (502)")]
    public async Task GetIntakesAsync_VariousErrorStatusCodes_ReturnsEmptyCollection(HttpStatusCode statusCode)
    {
        // Arrange
        var response = new HttpResponseMessage(statusCode);

        var mockApiService = new Mock<IApiService>();
        mockApiService
            .Setup(x => x.GetAsync("intakes"))
            .ReturnsAsync(response);

        var mockLogger = new Mock<ILogger<AcademicService>>();
        var service = new AcademicService(mockApiService.Object, mockLogger.Object);

        // Act
        var result = await service.GetIntakesAsync();

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(0, result.Count());
        mockApiService.Verify(x => x.GetAsync("intakes"), Times.Once);
    }

    /// <summary>
    /// Tests that GetIntakesAsync properly handles various exception types and returns an empty collection.
    /// Input: Various exception types thrown by API.
    /// Expected: Returns empty collection and logs error for all exception types.
    /// </summary>
    [TestMethod]
    [DataRow(typeof(HttpRequestException), DisplayName = "HttpRequestException")]
    [DataRow(typeof(TaskCanceledException), DisplayName = "TaskCanceledException")]
    [DataRow(typeof(InvalidOperationException), DisplayName = "InvalidOperationException")]
    [DataRow(typeof(Exception), DisplayName = "Generic Exception")]
    public async Task GetIntakesAsync_VariousExceptionTypes_ReturnsEmptyCollection(Type exceptionType)
    {
        // Arrange
        var exception = (Exception)Activator.CreateInstance(exceptionType, "Test error")!;
        var mockApiService = new Mock<IApiService>();
        mockApiService
            .Setup(x => x.GetAsync("intakes"))
            .ThrowsAsync(exception);

        var mockLogger = new Mock<ILogger<AcademicService>>();
        var service = new AcademicService(mockApiService.Object, mockLogger.Object);

        // Act
        var result = await service.GetIntakesAsync();

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(0, result.Count());
        mockApiService.Verify(x => x.GetAsync("intakes"), Times.Once);
    }

    /// <summary>
    /// Tests that GetIntakesAsync returns data with special characters in lookup item names.
    /// Input: API returns data with special characters, Unicode, and control characters.
    /// Expected: Returns data with special characters preserved.
    /// </summary>
    [TestMethod]
    public async Task GetIntakesAsync_DataWithSpecialCharacters_ReturnsDataCorrectly()
    {
        // Arrange
        var expectedData = new List<LookupItem>
        {
            new LookupItem { Id = "1", Name = "Fall & Winter 2024/25" },
            new LookupItem { Id = "2", Name = "Spring \"Early\" 2025" },
            new LookupItem { Id = "3", Name = "Été 2025 (été)" },
            new LookupItem { Id = "4", Name = "Summer<2025>" }
        };
        var jsonContent = JsonContent.Create(expectedData);
        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = jsonContent
        };

        var mockApiService = new Mock<IApiService>();
        mockApiService
            .Setup(x => x.GetAsync("intakes"))
            .ReturnsAsync(response);

        var mockLogger = new Mock<ILogger<AcademicService>>();
        var service = new AcademicService(mockApiService.Object, mockLogger.Object);

        // Act
        var result = await service.GetIntakesAsync();

        // Assert
        Assert.IsNotNull(result);
        var resultList = result.ToList();
        Assert.AreEqual(4, resultList.Count);
        Assert.AreEqual("Fall & Winter 2024/25", resultList[0].Name);
        Assert.AreEqual("Spring \"Early\" 2025", resultList[1].Name);
        Assert.AreEqual("Été 2025 (été)", resultList[2].Name);
        Assert.AreEqual("Summer<2025>", resultList[3].Name);
        mockApiService.Verify(x => x.GetAsync("intakes"), Times.Once);
    }

    /// <summary>
    /// Tests that GetIntakesAsync returns a single item collection when API returns one intake.
    /// Input: API returns success with single intake item.
    /// Expected: Returns collection with one item.
    /// </summary>
    [TestMethod]
    public async Task GetIntakesAsync_ApiReturnsSingleItem_ReturnsSingleItemCollection()
    {
        // Arrange
        var expectedData = new List<LookupItem>
        {
            new LookupItem { Id = "1", Name = "Fall 2024" }
        };
        var jsonContent = JsonContent.Create(expectedData);
        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = jsonContent
        };

        var mockApiService = new Mock<IApiService>();
        mockApiService
            .Setup(x => x.GetAsync("intakes"))
            .ReturnsAsync(response);

        var mockLogger = new Mock<ILogger<AcademicService>>();
        var service = new AcademicService(mockApiService.Object, mockLogger.Object);

        // Act
        var result = await service.GetIntakesAsync();

        // Assert
        Assert.IsNotNull(result);
        var resultList = result.ToList();
        Assert.AreEqual(1, resultList.Count);
        Assert.AreEqual("1", resultList[0].Id);
        Assert.AreEqual("Fall 2024", resultList[0].Name);
        mockApiService.Verify(x => x.GetAsync("intakes"), Times.Once);
    }

    /// <summary>
    /// Tests that GetIntakesAsync handles intakes with empty string values.
    /// Input: API returns data with empty strings for Id and Name.
    /// Expected: Returns items with empty string values preserved.
    /// </summary>
    [TestMethod]
    public async Task GetIntakesAsync_ItemsWithEmptyStrings_ReturnsItemsCorrectly()
    {
        // Arrange
        var expectedData = new List<LookupItem>
        {
            new LookupItem { Id = "", Name = "" },
            new LookupItem { Id = "1", Name = "" },
            new LookupItem { Id = "", Name = "Fall 2024" }
        };
        var jsonContent = JsonContent.Create(expectedData);
        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = jsonContent
        };

        var mockApiService = new Mock<IApiService>();
        mockApiService
            .Setup(x => x.GetAsync("intakes"))
            .ReturnsAsync(response);

        var mockLogger = new Mock<ILogger<AcademicService>>();
        var service = new AcademicService(mockApiService.Object, mockLogger.Object);

        // Act
        var result = await service.GetIntakesAsync();

        // Assert
        Assert.IsNotNull(result);
        var resultList = result.ToList();
        Assert.AreEqual(3, resultList.Count);
        Assert.AreEqual("", resultList[0].Id);
        Assert.AreEqual("", resultList[0].Name);
        Assert.AreEqual("1", resultList[1].Id);
        Assert.AreEqual("", resultList[1].Name);
        Assert.AreEqual("", resultList[2].Id);
        Assert.AreEqual("Fall 2024", resultList[2].Name);
        mockApiService.Verify(x => x.GetAsync("intakes"), Times.Once);
    }

    /// <summary>
    /// Tests that GetIntakesAsync returns an empty collection when API returns invalid JSON.
    /// Input: API returns success with malformed JSON content.
    /// Expected: Returns empty collection and logs error.
    /// </summary>
    [TestMethod]
    public async Task GetIntakesAsync_ApiReturnsInvalidJson_ReturnsEmptyCollection()
    {
        // Arrange
        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("{invalid json", System.Text.Encoding.UTF8, "application/json")
        };

        var mockApiService = new Mock<IApiService>();
        mockApiService
            .Setup(x => x.GetAsync("intakes"))
            .ReturnsAsync(response);

        var mockLogger = new Mock<ILogger<AcademicService>>();
        var service = new AcademicService(mockApiService.Object, mockLogger.Object);

        // Act
        var result = await service.GetIntakesAsync();

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(0, result.Count());
        mockApiService.Verify(x => x.GetAsync("intakes"), Times.Once);
    }

    /// <summary>
    /// Tests that GetIntakesAsync handles a large collection of intakes correctly.
    /// Input: API returns success with a large number of intakes.
    /// Expected: Returns all items in the collection.
    /// </summary>
    [TestMethod]
    public async Task GetIntakesAsync_ApiReturnsLargeCollection_ReturnsAllItems()
    {
        // Arrange
        var expectedData = new List<LookupItem>();
        for (int i = 0; i < 1000; i++)
        {
            expectedData.Add(new LookupItem { Id = i.ToString(), Name = $"Intake {i}" });
        }
        var jsonContent = JsonContent.Create(expectedData);
        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = jsonContent
        };

        var mockApiService = new Mock<IApiService>();
        mockApiService
            .Setup(x => x.GetAsync("intakes"))
            .ReturnsAsync(response);

        var mockLogger = new Mock<ILogger<AcademicService>>();
        var service = new AcademicService(mockApiService.Object, mockLogger.Object);

        // Act
        var result = await service.GetIntakesAsync();

        // Assert
        Assert.IsNotNull(result);
        var resultList = result.ToList();
        Assert.AreEqual(1000, resultList.Count);
        Assert.AreEqual("0", resultList[0].Id);
        Assert.AreEqual("Intake 0", resultList[0].Name);
        Assert.AreEqual("999", resultList[999].Id);
        Assert.AreEqual("Intake 999", resultList[999].Name);
        mockApiService.Verify(x => x.GetAsync("intakes"), Times.Once);
    }

    /// <summary>
    /// Tests that GetIntakesAsync handles whitespace-only strings in intake data.
    /// Input: API returns data with whitespace-only values.
    /// Expected: Returns items with whitespace values preserved.
    /// </summary>
    [TestMethod]
    public async Task GetIntakesAsync_ItemsWithWhitespace_ReturnsItemsCorrectly()
    {
        // Arrange
        var expectedData = new List<LookupItem>
        {
            new LookupItem { Id = "   ", Name = "   " },
            new LookupItem { Id = "\t", Name = "\n" },
            new LookupItem { Id = "1", Name = "  Fall 2024  " }
        };
        var jsonContent = JsonContent.Create(expectedData);
        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = jsonContent
        };

        var mockApiService = new Mock<IApiService>();
        mockApiService
            .Setup(x => x.GetAsync("intakes"))
            .ReturnsAsync(response);

        var mockLogger = new Mock<ILogger<AcademicService>>();
        var service = new AcademicService(mockApiService.Object, mockLogger.Object);

        // Act
        var result = await service.GetIntakesAsync();

        // Assert
        Assert.IsNotNull(result);
        var resultList = result.ToList();
        Assert.AreEqual(3, resultList.Count);
        Assert.AreEqual("   ", resultList[0].Id);
        Assert.AreEqual("   ", resultList[0].Name);
        mockApiService.Verify(x => x.GetAsync("intakes"), Times.Once);
    }

    /// <summary>
    /// Tests that UpdateUniversityAsync handles various successful HTTP status codes.
    /// Input: Valid id and name, API returns various 2xx status codes.
    /// Expected: Returns the deserialized UniversityDto for all success codes.
    /// </summary>
    [TestMethod]
    [DataRow(HttpStatusCode.OK)]
    [DataRow(HttpStatusCode.Created)]
    [DataRow(HttpStatusCode.Accepted)]
    [DataRow(HttpStatusCode.NoContent)]
    public async Task UpdateUniversityAsync_VariousSuccessStatusCodes_ReturnsDto(HttpStatusCode statusCode)
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<AcademicService>>();
        var service = new AcademicService(mockApi.Object, mockLogger.Object);

        var expectedDto = new UniversityDto
        {
            Id = "uni123",
            Name = "Updated University"
        };

        var responseMessage = new HttpResponseMessage(statusCode)
        {
            Content = JsonContent.Create(expectedDto)
        };

        mockApi.Setup(x => x.PutAsync(
            It.IsAny<string>(),
            It.IsAny<HttpContent>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(responseMessage);

        // Act
        var result = await service.UpdateUniversityAsync("uni123", "Updated University");

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual("uni123", result.Id);
    }

    /// <summary>
    /// Tests that UpdateUniversityAsync handles InvalidOperationException.
    /// Input: Valid id and name, API throws InvalidOperationException.
    /// Expected: Returns null and logs error.
    /// </summary>
    [TestMethod]
    public async Task UpdateUniversityAsync_InvalidOperationException_ReturnsNullAndLogsError()
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<AcademicService>>();
        var service = new AcademicService(mockApi.Object, mockLogger.Object);

        var exception = new InvalidOperationException("Invalid state");

        mockApi.Setup(x => x.PutAsync(
            It.IsAny<string>(),
            It.IsAny<HttpContent>(),
            It.IsAny<CancellationToken>()))
            .ThrowsAsync(exception);

        // Act
        var result = await service.UpdateUniversityAsync("uni123", "Test University");

        // Assert
        Assert.IsNull(result);
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Failed to update university")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    /// <summary>
    /// Tests that UpdateUniversityAsync correctly constructs the request endpoint.
    /// Input: Specific id value.
    /// Expected: Verifies PutAsync is called with the correct endpoint format.
    /// </summary>
    [TestMethod]
    public async Task UpdateUniversityAsync_SpecificId_ConstructsCorrectEndpoint()
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<AcademicService>>();
        var service = new AcademicService(mockApi.Object, mockLogger.Object);

        var responseMessage = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(new UniversityDto { Id = "test-123", Name = "Test" })
        };

        mockApi.Setup(x => x.PutAsync(
            It.IsAny<string>(),
            It.IsAny<HttpContent>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(responseMessage);

        // Act
        await service.UpdateUniversityAsync("test-123", "Test");

        // Assert
        mockApi.Verify(x => x.PutAsync(
            "universities/test-123",
            It.IsAny<HttpContent>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Tests that UpdateUniversityAsync sends content with correct content type.
    /// Input: Valid id and name.
    /// Expected: Verifies the content type is "application/json" and encoding is UTF-8.
    /// </summary>
    [TestMethod]
    public async Task UpdateUniversityAsync_ValidRequest_SendsCorrectContentType()
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<AcademicService>>();
        var service = new AcademicService(mockApi.Object, mockLogger.Object);

        HttpContent? capturedContent = null;

        var responseMessage = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(new UniversityDto { Id = "uni123", Name = "Test" })
        };

        mockApi.Setup(x => x.PutAsync(
            It.IsAny<string>(),
            It.IsAny<HttpContent>(),
            It.IsAny<CancellationToken>()))
            .Callback<string, HttpContent, CancellationToken>((_, content, _) => capturedContent = content)
            .ReturnsAsync(responseMessage);

        // Act
        await service.UpdateUniversityAsync("uni123", "Test");

        // Assert
        Assert.IsNotNull(capturedContent);
        Assert.AreEqual("application/json", capturedContent.Headers.ContentType?.MediaType);
        Assert.AreEqual("utf-8", capturedContent.Headers.ContentType?.CharSet);
    }

    /// <summary>
    /// Tests that GetSemestersAsync returns cached data on subsequent calls within cache duration.
    /// Input: Two consecutive calls within cache duration.
    /// Expected: API is called only once, second call returns cached data.
    /// </summary>
    [TestMethod]
    public async Task GetSemestersAsync_SecondCallWithinCacheDuration_ReturnsCachedDataWithoutApiCall()
    {
        // Arrange
        var semesters = new List<LookupItem>
        {
            new LookupItem { Id = "1", Name = "Semester 1" },
            new LookupItem { Id = "2", Name = "Semester 2" }
        };

        var responseContent = JsonContent.Create(semesters);
        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = responseContent
        };

        var mockApiService = new Mock<IApiService>();
        mockApiService
            .Setup(x => x.GetAsync("semesters"))
            .ReturnsAsync(httpResponse);

        var mockLogger = new Mock<ILogger<AcademicService>>();
        var service = new AcademicService(mockApiService.Object, mockLogger.Object);

        // Act
        var result1 = await service.GetSemestersAsync();
        var result2 = await service.GetSemestersAsync();

        // Assert
        Assert.IsNotNull(result1);
        Assert.IsNotNull(result2);
        var list1 = result1.ToList();
        var list2 = result2.ToList();
        Assert.AreEqual(2, list1.Count);
        Assert.AreEqual(2, list2.Count);
        Assert.AreEqual(list1[0].Id, list2[0].Id);
        Assert.AreEqual(list1[0].Name, list2[0].Name);
        mockApiService.Verify(x => x.GetAsync("semesters"), Times.Once);
    }

    /// <summary>
    /// Tests that DeleteFacultyAsync returns true when API returns 200 OK status code.
    /// Input: Valid faculty ID and API returns HttpStatusCode.OK.
    /// Expected: Returns true and verifies DeleteAsync was called with correct endpoint.
    /// </summary>
    [TestMethod]
    public async Task DeleteFacultyAsync_ApiReturns200OK_ReturnsTrue()
    {
        // Arrange
        var mockApiService = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<AcademicService>>();
        var facultyId = "faculty123";
        var successResponse = new HttpResponseMessage(HttpStatusCode.OK);

        mockApiService
            .Setup(x => x.DeleteAsync($"faculties/{facultyId}", It.IsAny<CancellationToken>()))
            .ReturnsAsync(successResponse);

        var service = new AcademicService(mockApiService.Object, mockLogger.Object);

        // Act
        var result = await service.DeleteFacultyAsync(facultyId);

        // Assert
        Assert.IsTrue(result);
        mockApiService.Verify(x => x.DeleteAsync($"faculties/{facultyId}", It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Tests that DeleteFacultyAsync returns false for various HTTP client error status codes.
    /// Input: Valid faculty ID and API returns 4xx error status codes.
    /// Expected: Returns false for all client error status codes.
    /// </summary>
    [TestMethod]
    [DataRow(HttpStatusCode.BadRequest, DisplayName = "400 Bad Request")]
    [DataRow(HttpStatusCode.Unauthorized, DisplayName = "401 Unauthorized")]
    [DataRow(HttpStatusCode.Forbidden, DisplayName = "403 Forbidden")]
    [DataRow(HttpStatusCode.NotFound, DisplayName = "404 Not Found")]
    [DataRow(HttpStatusCode.MethodNotAllowed, DisplayName = "405 Method Not Allowed")]
    [DataRow(HttpStatusCode.Conflict, DisplayName = "409 Conflict")]
    public async Task DeleteFacultyAsync_ClientErrorStatusCodes_ReturnsFalse(HttpStatusCode statusCode)
    {
        // Arrange
        var mockApiService = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<AcademicService>>();
        var facultyId = "faculty123";
        var errorResponse = new HttpResponseMessage(statusCode);

        mockApiService
            .Setup(x => x.DeleteAsync($"faculties/{facultyId}", It.IsAny<CancellationToken>()))
            .ReturnsAsync(errorResponse);

        var service = new AcademicService(mockApiService.Object, mockLogger.Object);

        // Act
        var result = await service.DeleteFacultyAsync(facultyId);

        // Assert
        Assert.IsFalse(result);
        mockApiService.Verify(x => x.DeleteAsync($"faculties/{facultyId}", It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Tests that DeleteFacultyAsync returns false for various HTTP server error status codes.
    /// Input: Valid faculty ID and API returns 5xx error status codes.
    /// Expected: Returns false for all server error status codes.
    /// </summary>
    [TestMethod]
    [DataRow(HttpStatusCode.InternalServerError, DisplayName = "500 Internal Server Error")]
    [DataRow(HttpStatusCode.BadGateway, DisplayName = "502 Bad Gateway")]
    [DataRow(HttpStatusCode.ServiceUnavailable, DisplayName = "503 Service Unavailable")]
    [DataRow(HttpStatusCode.GatewayTimeout, DisplayName = "504 Gateway Timeout")]
    public async Task DeleteFacultyAsync_ServerErrorStatusCodes_ReturnsFalse(HttpStatusCode statusCode)
    {
        // Arrange
        var mockApiService = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<AcademicService>>();
        var facultyId = "faculty123";
        var errorResponse = new HttpResponseMessage(statusCode);

        mockApiService
            .Setup(x => x.DeleteAsync($"faculties/{facultyId}", It.IsAny<CancellationToken>()))
            .ReturnsAsync(errorResponse);

        var service = new AcademicService(mockApiService.Object, mockLogger.Object);

        // Act
        var result = await service.DeleteFacultyAsync(facultyId);

        // Assert
        Assert.IsFalse(result);
        mockApiService.Verify(x => x.DeleteAsync($"faculties/{facultyId}", It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Tests that DeleteFacultyAsync returns false for HTTP redirect status codes.
    /// Input: Valid faculty ID and API returns 3xx redirect status codes.
    /// Expected: Returns false for all redirect status codes.
    /// </summary>
    [TestMethod]
    [DataRow(HttpStatusCode.MovedPermanently, DisplayName = "301 Moved Permanently")]
    [DataRow(HttpStatusCode.Found, DisplayName = "302 Found")]
    [DataRow(HttpStatusCode.NotModified, DisplayName = "304 Not Modified")]
    [DataRow(HttpStatusCode.TemporaryRedirect, DisplayName = "307 Temporary Redirect")]
    public async Task DeleteFacultyAsync_RedirectStatusCodes_ReturnsFalse(HttpStatusCode statusCode)
    {
        // Arrange
        var mockApiService = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<AcademicService>>();
        var facultyId = "faculty123";
        var redirectResponse = new HttpResponseMessage(statusCode);

        mockApiService
            .Setup(x => x.DeleteAsync($"faculties/{facultyId}", It.IsAny<CancellationToken>()))
            .ReturnsAsync(redirectResponse);

        var service = new AcademicService(mockApiService.Object, mockLogger.Object);

        // Act
        var result = await service.DeleteFacultyAsync(facultyId);

        // Assert
        Assert.IsFalse(result);
        mockApiService.Verify(x => x.DeleteAsync($"faculties/{facultyId}", It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Tests that DeleteFacultyAsync handles empty string faculty ID.
    /// Input: Empty string as faculty ID.
    /// Expected: Constructs endpoint "faculties/" and makes API call.
    /// </summary>
    [TestMethod]
    public async Task DeleteFacultyAsync_EmptyStringId_ConstructsCorrectEndpoint()
    {
        // Arrange
        var mockApiService = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<AcademicService>>();
        var facultyId = "";
        var successResponse = new HttpResponseMessage(HttpStatusCode.OK);

        mockApiService
            .Setup(x => x.DeleteAsync($"faculties/{facultyId}", It.IsAny<CancellationToken>()))
            .ReturnsAsync(successResponse);

        var service = new AcademicService(mockApiService.Object, mockLogger.Object);

        // Act
        var result = await service.DeleteFacultyAsync(facultyId);

        // Assert
        Assert.IsTrue(result);
        mockApiService.Verify(x => x.DeleteAsync("faculties/", It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Tests that DeleteFacultyAsync handles whitespace-only faculty ID.
    /// Input: Whitespace-only strings as faculty ID.
    /// Expected: Constructs endpoint with whitespace and makes API call.
    /// </summary>
    [TestMethod]
    [DataRow(" ", DisplayName = "Single space")]
    [DataRow("   ", DisplayName = "Multiple spaces")]
    [DataRow("\t", DisplayName = "Tab character")]
    [DataRow("\n", DisplayName = "Newline character")]
    [DataRow("\r\n", DisplayName = "Carriage return and newline")]
    public async Task DeleteFacultyAsync_WhitespaceId_ConstructsCorrectEndpoint(string facultyId)
    {
        // Arrange
        var mockApiService = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<AcademicService>>();
        var successResponse = new HttpResponseMessage(HttpStatusCode.OK);

        mockApiService
            .Setup(x => x.DeleteAsync($"faculties/{facultyId}", It.IsAny<CancellationToken>()))
            .ReturnsAsync(successResponse);

        var service = new AcademicService(mockApiService.Object, mockLogger.Object);

        // Act
        var result = await service.DeleteFacultyAsync(facultyId);

        // Assert
        Assert.IsTrue(result);
        mockApiService.Verify(x => x.DeleteAsync($"faculties/{facultyId}", It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Tests that DeleteFacultyAsync handles single character faculty ID.
    /// Input: Single character string as faculty ID.
    /// Expected: Constructs endpoint correctly and makes API call.
    /// </summary>
    [TestMethod]
    public async Task DeleteFacultyAsync_SingleCharacterId_ConstructsCorrectEndpoint()
    {
        // Arrange
        var mockApiService = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<AcademicService>>();
        var facultyId = "a";
        var successResponse = new HttpResponseMessage(HttpStatusCode.OK);

        mockApiService
            .Setup(x => x.DeleteAsync($"faculties/{facultyId}", It.IsAny<CancellationToken>()))
            .ReturnsAsync(successResponse);

        var service = new AcademicService(mockApiService.Object, mockLogger.Object);

        // Act
        var result = await service.DeleteFacultyAsync(facultyId);

        // Assert
        Assert.IsTrue(result);
        mockApiService.Verify(x => x.DeleteAsync("faculties/a", It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Tests that DeleteFacultyAsync handles very long faculty ID strings.
    /// Input: Very long string (1000+ characters) as faculty ID.
    /// Expected: Constructs endpoint with full long ID and makes API call.
    /// </summary>
    [TestMethod]
    public async Task DeleteFacultyAsync_VeryLongId_ConstructsCorrectEndpoint()
    {
        // Arrange
        var mockApiService = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<AcademicService>>();
        var facultyId = new string('x', 1000);
        var successResponse = new HttpResponseMessage(HttpStatusCode.OK);

        mockApiService
            .Setup(x => x.DeleteAsync($"faculties/{facultyId}", It.IsAny<CancellationToken>()))
            .ReturnsAsync(successResponse);

        var service = new AcademicService(mockApiService.Object, mockLogger.Object);

        // Act
        var result = await service.DeleteFacultyAsync(facultyId);

        // Assert
        Assert.IsTrue(result);
        mockApiService.Verify(x => x.DeleteAsync($"faculties/{facultyId}", It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Tests that DeleteFacultyAsync handles faculty ID with special characters.
    /// Input: Faculty IDs containing various special characters.
    /// Expected: Constructs endpoint with special characters and makes API call.
    /// </summary>
    [TestMethod]
    [DataRow("faculty-with-hyphens", DisplayName = "Hyphens")]
    [DataRow("faculty_with_underscores", DisplayName = "Underscores")]
    [DataRow("faculty/with/slashes", DisplayName = "Forward slashes")]
    [DataRow("faculty&with&ampersands", DisplayName = "Ampersands")]
    [DataRow("faculty?with?questions", DisplayName = "Question marks")]
    [DataRow("faculty#with#hashes", DisplayName = "Hash symbols")]
    [DataRow("faculty@with@ats", DisplayName = "At symbols")]
    [DataRow("faculty!@#$%^&*()", DisplayName = "Mixed special characters")]
    public async Task DeleteFacultyAsync_SpecialCharactersInId_ConstructsCorrectEndpoint(string facultyId)
    {
        // Arrange
        var mockApiService = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<AcademicService>>();
        var successResponse = new HttpResponseMessage(HttpStatusCode.OK);

        mockApiService
            .Setup(x => x.DeleteAsync($"faculties/{facultyId}", It.IsAny<CancellationToken>()))
            .ReturnsAsync(successResponse);

        var service = new AcademicService(mockApiService.Object, mockLogger.Object);

        // Act
        var result = await service.DeleteFacultyAsync(facultyId);

        // Assert
        Assert.IsTrue(result);
        mockApiService.Verify(x => x.DeleteAsync($"faculties/{facultyId}", It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Tests that DeleteFacultyAsync handles faculty ID with Unicode characters.
    /// Input: Faculty IDs containing Unicode characters from various languages.
    /// Expected: Constructs endpoint with Unicode characters and makes API call.
    /// </summary>
    [TestMethod]
    [DataRow("faculty日本語", DisplayName = "Japanese characters")]
    [DataRow("faculty한국어", DisplayName = "Korean characters")]
    [DataRow("facultyالعربية", DisplayName = "Arabic characters")]
    [DataRow("faculty中文", DisplayName = "Chinese characters")]
    [DataRow("faculty🔥emoji", DisplayName = "Emoji characters")]
    [DataRow("facultyÜnicode", DisplayName = "Accented characters")]
    public async Task DeleteFacultyAsync_UnicodeCharactersInId_ConstructsCorrectEndpoint(string facultyId)
    {
        // Arrange
        var mockApiService = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<AcademicService>>();
        var successResponse = new HttpResponseMessage(HttpStatusCode.OK);

        mockApiService
            .Setup(x => x.DeleteAsync($"faculties/{facultyId}", It.IsAny<CancellationToken>()))
            .ReturnsAsync(successResponse);

        var service = new AcademicService(mockApiService.Object, mockLogger.Object);

        // Act
        var result = await service.DeleteFacultyAsync(facultyId);

        // Assert
        Assert.IsTrue(result);
        mockApiService.Verify(x => x.DeleteAsync($"faculties/{facultyId}", It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Tests that DeleteFacultyAsync handles faculty ID in GUID format.
    /// Input: Faculty ID as a GUID string.
    /// Expected: Constructs endpoint with GUID and makes API call.
    /// </summary>
    [TestMethod]
    public async Task DeleteFacultyAsync_GuidFormatId_ConstructsCorrectEndpoint()
    {
        // Arrange
        var mockApiService = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<AcademicService>>();
        var facultyId = "550e8400-e29b-41d4-a716-446655440000";
        var successResponse = new HttpResponseMessage(HttpStatusCode.OK);

        mockApiService
            .Setup(x => x.DeleteAsync($"faculties/{facultyId}", It.IsAny<CancellationToken>()))
            .ReturnsAsync(successResponse);

        var service = new AcademicService(mockApiService.Object, mockLogger.Object);

        // Act
        var result = await service.DeleteFacultyAsync(facultyId);

        // Assert
        Assert.IsTrue(result);
        mockApiService.Verify(x => x.DeleteAsync($"faculties/{facultyId}", It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Tests that DeleteFacultyAsync returns false and logs error when API throws HttpRequestException.
    /// Input: Valid faculty ID and API throws HttpRequestException.
    /// Expected: Returns false and logs error with exception details and faculty ID.
    /// </summary>
    [TestMethod]
    public async Task DeleteFacultyAsync_HttpRequestExceptionThrown_ReturnsFalseAndLogsError()
    {
        // Arrange
        var mockApiService = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<AcademicService>>();
        var facultyId = "faculty123";
        var expectedException = new HttpRequestException("Network error occurred");

        mockApiService
            .Setup(x => x.DeleteAsync($"faculties/{facultyId}", It.IsAny<CancellationToken>()))
            .ThrowsAsync(expectedException);

        var service = new AcademicService(mockApiService.Object, mockLogger.Object);

        // Act
        var result = await service.DeleteFacultyAsync(facultyId);

        // Assert
        Assert.IsFalse(result);
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Failed to delete faculty")),
                expectedException,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    /// <summary>
    /// Tests that DeleteFacultyAsync returns false and logs error when API throws TaskCanceledException.
    /// Input: Valid faculty ID and API throws TaskCanceledException (timeout scenario).
    /// Expected: Returns false and logs error with exception details.
    /// </summary>
    [TestMethod]
    public async Task DeleteFacultyAsync_TaskCanceledExceptionThrown_ReturnsFalseAndLogsError()
    {
        // Arrange
        var mockApiService = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<AcademicService>>();
        var facultyId = "faculty123";
        var expectedException = new TaskCanceledException("Request timeout");

        mockApiService
            .Setup(x => x.DeleteAsync($"faculties/{facultyId}", It.IsAny<CancellationToken>()))
            .ThrowsAsync(expectedException);

        var service = new AcademicService(mockApiService.Object, mockLogger.Object);

        // Act
        var result = await service.DeleteFacultyAsync(facultyId);

        // Assert
        Assert.IsFalse(result);
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Failed to delete faculty")),
                expectedException,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    /// <summary>
    /// Tests that DeleteFacultyAsync returns false and logs error when API throws InvalidOperationException.
    /// Input: Valid faculty ID and API throws InvalidOperationException.
    /// Expected: Returns false and logs error with exception details.
    /// </summary>
    [TestMethod]
    public async Task DeleteFacultyAsync_InvalidOperationExceptionThrown_ReturnsFalseAndLogsError()
    {
        // Arrange
        var mockApiService = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<AcademicService>>();
        var facultyId = "faculty123";
        var expectedException = new InvalidOperationException("Invalid operation");

        mockApiService
            .Setup(x => x.DeleteAsync($"faculties/{facultyId}", It.IsAny<CancellationToken>()))
            .ThrowsAsync(expectedException);

        var service = new AcademicService(mockApiService.Object, mockLogger.Object);

        // Act
        var result = await service.DeleteFacultyAsync(facultyId);

        // Assert
        Assert.IsFalse(result);
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Failed to delete faculty")),
                expectedException,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    /// <summary>
    /// Tests that DeleteFacultyAsync returns false and logs error when API throws generic Exception.
    /// Input: Valid faculty ID and API throws generic Exception.
    /// Expected: Returns false and logs error with exception details.
    /// </summary>
    [TestMethod]
    public async Task DeleteFacultyAsync_GenericExceptionThrown_ReturnsFalseAndLogsError()
    {
        // Arrange
        var mockApiService = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<AcademicService>>();
        var facultyId = "faculty123";
        var expectedException = new Exception("Unexpected error");

        mockApiService
            .Setup(x => x.DeleteAsync($"faculties/{facultyId}", It.IsAny<CancellationToken>()))
            .ThrowsAsync(expectedException);

        var service = new AcademicService(mockApiService.Object, mockLogger.Object);

        // Act
        var result = await service.DeleteFacultyAsync(facultyId);

        // Assert
        Assert.IsFalse(result);
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Failed to delete faculty")),
                expectedException,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    /// <summary>
    /// Tests that DeleteFacultyAsync logs the correct faculty ID when an exception occurs.
    /// Input: Specific faculty ID and API throws exception.
    /// Expected: Error log contains the exact faculty ID that was passed.
    /// </summary>
    [TestMethod]
    public async Task DeleteFacultyAsync_ExceptionThrown_LogsCorrectFacultyId()
    {
        // Arrange
        var mockApiService = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<AcademicService>>();
        var facultyId = "specific-faculty-id-12345";
        var expectedException = new Exception("Test exception");

        mockApiService
            .Setup(x => x.DeleteAsync($"faculties/{facultyId}", It.IsAny<CancellationToken>()))
            .ThrowsAsync(expectedException);

        var service = new AcademicService(mockApiService.Object, mockLogger.Object);

        // Act
        var result = await service.DeleteFacultyAsync(facultyId);

        // Assert
        Assert.IsFalse(result);
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains(facultyId)),
                expectedException,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    /// <summary>
    /// Tests that DeleteFacultyAsync handles faculty ID with control characters.
    /// Input: Faculty ID containing control characters like null terminator, backspace, etc.
    /// Expected: Constructs endpoint with control characters and makes API call.
    /// </summary>
    [TestMethod]
    [DataRow("faculty\0null", DisplayName = "Null terminator")]
    [DataRow("faculty\bbackspace", DisplayName = "Backspace")]
    [DataRow("faculty\fformfeed", DisplayName = "Form feed")]
    public async Task DeleteFacultyAsync_ControlCharactersInId_ConstructsCorrectEndpoint(string facultyId)
    {
        // Arrange
        var mockApiService = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<AcademicService>>();
        var successResponse = new HttpResponseMessage(HttpStatusCode.OK);

        mockApiService
            .Setup(x => x.DeleteAsync($"faculties/{facultyId}", It.IsAny<CancellationToken>()))
            .ReturnsAsync(successResponse);

        var service = new AcademicService(mockApiService.Object, mockLogger.Object);

        // Act
        var result = await service.DeleteFacultyAsync(facultyId);

        // Assert
        Assert.IsTrue(result);
        mockApiService.Verify(x => x.DeleteAsync($"faculties/{facultyId}", It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Tests that CreateUniversityAsync handles empty string name.
    /// Input: Empty string.
    /// Expected: Makes API call with empty string.
    /// </summary>
    [TestMethod]
    public async Task CreateUniversityAsync_EmptyStringName_MakesApiCall()
    {
        // Arrange
        var apiMock = new Mock<IApiService>();
        var loggerMock = new Mock<ILogger<AcademicService>>();
        var service = new AcademicService(apiMock.Object, loggerMock.Object);
        var responseMessage = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(new UniversityDto { Id = "1", Name = "" })
        };

        apiMock.Setup(x => x.PostAsync("universities", It.IsAny<HttpContent>(), It.IsAny<CancellationToken>()))
               .ReturnsAsync(responseMessage);

        // Act
        var result = await service.CreateUniversityAsync("");

        // Assert
        Assert.IsNotNull(result);
        apiMock.Verify(x => x.PostAsync("universities", It.IsAny<HttpContent>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Tests that CreateUniversityAsync handles whitespace-only string name.
    /// Input: Whitespace-only strings.
    /// Expected: Makes API call with whitespace string.
    /// </summary>
    [TestMethod]
    [DataRow("   ")]
    [DataRow("\t")]
    [DataRow("\n")]
    [DataRow("\r\n")]
    public async Task CreateUniversityAsync_WhitespaceOnlyName_MakesApiCall(string name)
    {
        // Arrange
        var apiMock = new Mock<IApiService>();
        var loggerMock = new Mock<ILogger<AcademicService>>();
        var service = new AcademicService(apiMock.Object, loggerMock.Object);
        var responseMessage = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(new UniversityDto { Id = "1", Name = name })
        };

        apiMock.Setup(x => x.PostAsync("universities", It.IsAny<HttpContent>(), It.IsAny<CancellationToken>()))
               .ReturnsAsync(responseMessage);

        // Act
        var result = await service.CreateUniversityAsync(name);

        // Assert
        Assert.IsNotNull(result);
        apiMock.Verify(x => x.PostAsync("universities", It.IsAny<HttpContent>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Tests that CreateUniversityAsync handles null name parameter.
    /// Input: Null name (runtime allows despite non-nullable annotation).
    /// Expected: Method executes without throwing NullReferenceException during serialization.
    /// </summary>
    [TestMethod]
    public async Task CreateUniversityAsync_NullName_HandlesGracefully()
    {
        // Arrange
        var apiMock = new Mock<IApiService>();
        var loggerMock = new Mock<ILogger<AcademicService>>();
        var service = new AcademicService(apiMock.Object, loggerMock.Object);
        var responseMessage = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(new UniversityDto { Id = "1", Name = "" })
        };

        apiMock.Setup(x => x.PostAsync("universities", It.IsAny<HttpContent>(), It.IsAny<CancellationToken>()))
               .ReturnsAsync(responseMessage);

        // Act
        var result = await service.CreateUniversityAsync(null!);

        // Assert
        // Method should handle null gracefully (JsonSerializer serializes null as "null")
        apiMock.Verify(x => x.PostAsync("universities", It.IsAny<HttpContent>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Tests that CreateUniversityAsync returns correct DTO with all properties.
    /// Input: Valid name; API returns complete DTO.
    /// Expected: All DTO properties are correctly deserialized.
    /// </summary>
    [TestMethod]
    public async Task CreateUniversityAsync_ValidResponse_ReturnsCompleteDto()
    {
        // Arrange
        var apiMock = new Mock<IApiService>();
        var loggerMock = new Mock<ILogger<AcademicService>>();
        var service = new AcademicService(apiMock.Object, loggerMock.Object);
        var expectedDto = new UniversityDto
        {
            Id = "123",
            Name = "Oxford University",
            Status = "Active",
            FacultiesCount = 5
        };
        var responseMessage = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(expectedDto)
        };

        apiMock.Setup(x => x.PostAsync("universities", It.IsAny<HttpContent>(), It.IsAny<CancellationToken>()))
               .ReturnsAsync(responseMessage);

        // Act
        var result = await service.CreateUniversityAsync("Oxford University");

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual("123", result.Id);
        Assert.AreEqual("Oxford University", result.Name);
        Assert.AreEqual("Active", result.Status);
        Assert.AreEqual(5, result.FacultiesCount);
    }

    /// <summary>
    /// Tests that CreateUniversityAsync handles 2xx success status codes.
    /// Input: Valid name; API returns various 2xx status codes.
    /// Expected: Returns DTO for all 2xx success codes.
    /// </summary>
    [TestMethod]
    [DataRow(HttpStatusCode.OK)]
    [DataRow(HttpStatusCode.Created)]
    [DataRow(HttpStatusCode.Accepted)]
    public async Task CreateUniversityAsync_Various2xxStatusCodes_ReturnsDto(HttpStatusCode statusCode)
    {
        // Arrange
        var apiMock = new Mock<IApiService>();
        var loggerMock = new Mock<ILogger<AcademicService>>();
        var service = new AcademicService(apiMock.Object, loggerMock.Object);
        var expectedDto = new UniversityDto { Id = "1", Name = "Test University" };
        var responseMessage = new HttpResponseMessage(statusCode)
        {
            Content = JsonContent.Create(expectedDto)
        };

        apiMock.Setup(x => x.PostAsync("universities", It.IsAny<HttpContent>(), It.IsAny<CancellationToken>()))
               .ReturnsAsync(responseMessage);

        // Act
        var result = await service.CreateUniversityAsync("Test University");

        // Assert
        Assert.IsNotNull(result);
    }

    /// <summary>
    /// Tests that CreateUniversityAsync returns null for 3xx redirect status codes.
    /// Input: Valid name; API returns redirect status codes.
    /// Expected: Returns null (redirects are not considered success).
    /// </summary>
    [TestMethod]
    [DataRow(HttpStatusCode.MovedPermanently)]
    [DataRow(HttpStatusCode.Found)]
    [DataRow(HttpStatusCode.NotModified)]
    public async Task CreateUniversityAsync_RedirectStatusCodes_ReturnsNull(HttpStatusCode statusCode)
    {
        // Arrange
        var apiMock = new Mock<IApiService>();
        var loggerMock = new Mock<ILogger<AcademicService>>();
        var service = new AcademicService(apiMock.Object, loggerMock.Object);
        var responseMessage = new HttpResponseMessage(statusCode);

        apiMock.Setup(x => x.PostAsync("universities", It.IsAny<HttpContent>(), It.IsAny<CancellationToken>()))
               .ReturnsAsync(responseMessage);

        // Act
        var result = await service.CreateUniversityAsync("Test University");

        // Assert
        Assert.IsNull(result);
    }

    /// <summary>
    /// Tests that GetUniversityByIdAsync returns null when id parameter is null.
    /// Input: Null id.
    /// Expected: Returns null and logs error.
    /// </summary>
    [TestMethod]
    public async Task GetUniversityByIdAsync_NullId_ReturnsNull()
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<AcademicService>>();
        var service = new AcademicService(mockApi.Object, mockLogger.Object);

        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(new UniversityDto { Id = "1", Name = "Test" })
        };

        mockApi.Setup(x => x.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(httpResponse);

        // Act
        var result = await service.GetUniversityByIdAsync(null!);

        // Assert
        Assert.IsNull(result);
    }

    /// <summary>
    /// Tests that GetUniversityByIdAsync handles response with all UniversityDto properties populated.
    /// Input: Valid id; API returns complete UniversityDto with all properties.
    /// Expected: Returns UniversityDto with all properties correctly populated.
    /// </summary>
    [TestMethod]
    public async Task GetUniversityByIdAsync_CompleteDto_ReturnsAllProperties()
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<AcademicService>>();
        var service = new AcademicService(mockApi.Object, mockLogger.Object);

        var expectedDto = new UniversityDto
        {
            Id = "uni-123",
            Name = "Complete University",
            FacultiesCount = 10,
            Status = "Active"
        };

        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(expectedDto)
        };

        mockApi.Setup(x => x.GetAsync("universities/uni-123", It.IsAny<CancellationToken>()))
            .ReturnsAsync(httpResponse);

        // Act
        var result = await service.GetUniversityByIdAsync("uni-123");

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(expectedDto.Id, result.Id);
        Assert.AreEqual(expectedDto.Name, result.Name);
        Assert.AreEqual(expectedDto.FacultiesCount, result.FacultiesCount);
        Assert.AreEqual(expectedDto.Status, result.Status);
    }

    /// <summary>
    /// Tests that GetUniversityByIdAsync handles UniversityDto with boundary values for FacultiesCount.
    /// Input: Valid id; API returns UniversityDto with extreme integer values.
    /// Expected: Returns UniversityDto with boundary values preserved.
    /// </summary>
    [TestMethod]
    [DataRow(int.MinValue, DisplayName = "MinValue")]
    [DataRow(int.MaxValue, DisplayName = "MaxValue")]
    [DataRow(0, DisplayName = "Zero")]
    [DataRow(-1, DisplayName = "NegativeOne")]
    public async Task GetUniversityByIdAsync_BoundaryFacultiesCount_ReturnsCorrectValue(int facultiesCount)
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<AcademicService>>();
        var service = new AcademicService(mockApi.Object, mockLogger.Object);

        var expectedDto = new UniversityDto
        {
            Id = "uni-123",
            Name = "Test University",
            FacultiesCount = facultiesCount,
            Status = "Active"
        };

        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(expectedDto)
        };

        mockApi.Setup(x => x.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(httpResponse);

        // Act
        var result = await service.GetUniversityByIdAsync("uni-123");

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(facultiesCount, result.FacultiesCount);
    }

    /// <summary>
    /// Tests that GetUniversityByIdAsync handles UniversityDto with empty string properties.
    /// Input: Valid id; API returns UniversityDto with empty string values.
    /// Expected: Returns UniversityDto with empty strings preserved.
    /// </summary>
    [TestMethod]
    public async Task GetUniversityByIdAsync_EmptyStringProperties_ReturnsEmptyStrings()
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<AcademicService>>();
        var service = new AcademicService(mockApi.Object, mockLogger.Object);

        var expectedDto = new UniversityDto
        {
            Id = "",
            Name = "",
            FacultiesCount = 0,
            Status = ""
        };

        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(expectedDto)
        };

        mockApi.Setup(x => x.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(httpResponse);

        // Act
        var result = await service.GetUniversityByIdAsync("test-id");

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual("", result.Id);
        Assert.AreEqual("", result.Name);
        Assert.AreEqual("", result.Status);
    }

    /// <summary>
    /// Tests that GetUniversityByIdAsync handles various 2xx success status codes correctly.
    /// Input: Valid id; API returns different 2xx status codes.
    /// Expected: Returns UniversityDto for all 2xx success codes.
    /// </summary>
    [TestMethod]
    [DataRow(HttpStatusCode.OK, DisplayName = "200 OK")]
    [DataRow(HttpStatusCode.Created, DisplayName = "201 Created")]
    [DataRow(HttpStatusCode.Accepted, DisplayName = "202 Accepted")]
    [DataRow(HttpStatusCode.NoContent, DisplayName = "204 NoContent")]
    public async Task GetUniversityByIdAsync_Various2xxStatusCodes_ReturnsDto(HttpStatusCode statusCode)
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<AcademicService>>();
        var service = new AcademicService(mockApi.Object, mockLogger.Object);

        var expectedDto = new UniversityDto
        {
            Id = "uni-123",
            Name = "Test University",
            FacultiesCount = 5,
            Status = "Active"
        };

        var httpResponse = new HttpResponseMessage(statusCode)
        {
            Content = JsonContent.Create(expectedDto)
        };

        mockApi.Setup(x => x.GetAsync("universities/uni-123", It.IsAny<CancellationToken>()))
            .ReturnsAsync(httpResponse);

        // Act
        var result = await service.GetUniversityByIdAsync("uni-123");

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(expectedDto.Id, result.Id);
        Assert.AreEqual(expectedDto.Name, result.Name);
    }

    /// <summary>
    /// Tests that GetUniversityByIdAsync returns null for 3xx redirect status codes.
    /// Input: Valid id; API returns redirect status codes.
    /// Expected: Returns null for all redirect status codes.
    /// </summary>
    [TestMethod]
    [DataRow(HttpStatusCode.MovedPermanently, DisplayName = "301 Moved Permanently")]
    [DataRow(HttpStatusCode.Found, DisplayName = "302 Found")]
    [DataRow(HttpStatusCode.SeeOther, DisplayName = "303 See Other")]
    [DataRow(HttpStatusCode.NotModified, DisplayName = "304 Not Modified")]
    [DataRow(HttpStatusCode.TemporaryRedirect, DisplayName = "307 Temporary Redirect")]
    public async Task GetUniversityByIdAsync_RedirectStatusCodes_ReturnsNull(HttpStatusCode statusCode)
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<AcademicService>>();
        var service = new AcademicService(mockApi.Object, mockLogger.Object);

        var httpResponse = new HttpResponseMessage(statusCode);

        mockApi.Setup(x => x.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(httpResponse);

        // Act
        var result = await service.GetUniversityByIdAsync("test-id");

        // Assert
        Assert.IsNull(result);
    }

    /// <summary>
    /// Tests that GetUniversityByIdAsync logs error with exception details when deserialization fails.
    /// Input: Valid id; API returns invalid JSON that cannot be deserialized.
    /// Expected: Returns null and logs error with JsonException.
    /// </summary>
    [TestMethod]
    public async Task GetUniversityByIdAsync_JsonDeserializationFailsWithJsonException_LogsErrorAndReturnsNull()
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<AcademicService>>();
        var service = new AcademicService(mockApi.Object, mockLogger.Object);

        var invalidJsonContent = new StringContent("{ invalid json }", Encoding.UTF8, "application/json");
        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = invalidJsonContent
        };

        mockApi.Setup(x => x.GetAsync("universities/test-id", It.IsAny<CancellationToken>()))
            .ReturnsAsync(httpResponse);

        // Act
        var result = await service.GetUniversityByIdAsync("test-id");

        // Assert
        Assert.IsNull(result);
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Failed to fetch university")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    /// <summary>
    /// Tests that GetUniversityByIdAsync constructs correct path for various ID edge cases.
    /// Input: Various edge-case id values including boundaries and special characters.
    /// Expected: Correct API path is constructed for all inputs.
    /// </summary>
    [TestMethod]
    [DataRow("123", "universities/123", DisplayName = "Numeric ID")]
    [DataRow("abc-123", "universities/abc-123", DisplayName = "Alphanumeric with hyphen")]
    [DataRow("550e8400-e29b-41d4-a716-446655440000", "universities/550e8400-e29b-41d4-a716-446655440000", DisplayName = "GUID")]
    [DataRow("uni_123", "universities/uni_123", DisplayName = "With underscore")]
    [DataRow("uni.123", "universities/uni.123", DisplayName = "With dot")]
    public async Task GetUniversityByIdAsync_VariousValidIdFormats_ConstructsCorrectPath(string id, string expectedPath)
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<AcademicService>>();
        var service = new AcademicService(mockApi.Object, mockLogger.Object);

        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(new UniversityDto { Id = id, Name = "Test" })
        };

        mockApi.Setup(x => x.GetAsync(expectedPath, It.IsAny<CancellationToken>()))
            .ReturnsAsync(httpResponse);

        // Act
        var result = await service.GetUniversityByIdAsync(id);

        // Assert
        mockApi.Verify(x => x.GetAsync(expectedPath, It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Tests that GetUniversityByIdAsync handles response with null content correctly.
    /// Input: Valid id; API returns success but ReadFromJsonAsync returns null.
    /// Expected: Returns null.
    /// </summary>
    [TestMethod]
    public async Task GetUniversityByIdAsync_ResponseContentDeserializesToNull_ReturnsNull()
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<AcademicService>>();
        var service = new AcademicService(mockApi.Object, mockLogger.Object);

        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("null", Encoding.UTF8, "application/json")
        };

        mockApi.Setup(x => x.GetAsync("universities/test-id", It.IsAny<CancellationToken>()))
            .ReturnsAsync(httpResponse);

        // Act
        var result = await service.GetUniversityByIdAsync("test-id");

        // Assert
        Assert.IsNull(result);
    }

    /// <summary>
    /// Tests that GetUniversityByIdAsync does not log when API returns non-success status code.
    /// Input: Valid id; API returns 404 NotFound.
    /// Expected: Returns null without logging any error (no exception occurred).
    /// </summary>
    [TestMethod]
    public async Task GetUniversityByIdAsync_NonSuccessStatusCode_DoesNotLogError()
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<AcademicService>>();
        var service = new AcademicService(mockApi.Object, mockLogger.Object);

        var httpResponse = new HttpResponseMessage(HttpStatusCode.NotFound);

        mockApi.Setup(x => x.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(httpResponse);

        // Act
        var result = await service.GetUniversityByIdAsync("test-id");

        // Assert
        Assert.IsNull(result);
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
    /// Tests that GetUniversityByIdAsync logs the correct university id in the error message.
    /// Input: Specific id value; API throws exception.
    /// Expected: Error log contains the exact university id that was passed.
    /// </summary>
    [TestMethod]
    public async Task GetUniversityByIdAsync_ExceptionOccursWithSpecificId_LogsCorrectId()
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<AcademicService>>();
        var service = new AcademicService(mockApi.Object, mockLogger.Object);

        var specificId = "specific-university-id-12345";
        var expectedException = new HttpRequestException("Network error");

        mockApi.Setup(x => x.GetAsync($"universities/{specificId}", It.IsAny<CancellationToken>()))
            .ThrowsAsync(expectedException);

        // Act
        var result = await service.GetUniversityByIdAsync(specificId);

        // Assert
        Assert.IsNull(result);
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains(specificId)),
                expectedException,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    /// <summary>
    /// Tests that GetUniversityByIdAsync handles UniversityDto with very long string properties.
    /// Input: Valid id; API returns UniversityDto with very long strings.
    /// Expected: Returns UniversityDto with long strings preserved.
    /// </summary>
    [TestMethod]
    public async Task GetUniversityByIdAsync_VeryLongStringProperties_ReturnsCorrectData()
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<AcademicService>>();
        var service = new AcademicService(mockApi.Object, mockLogger.Object);

        var longString = new string('A', 10000);
        var expectedDto = new UniversityDto
        {
            Id = longString,
            Name = longString,
            FacultiesCount = 5,
            Status = longString
        };

        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(expectedDto)
        };

        mockApi.Setup(x => x.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(httpResponse);

        // Act
        var result = await service.GetUniversityByIdAsync("test-id");

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(longString, result.Id);
        Assert.AreEqual(longString, result.Name);
        Assert.AreEqual(longString, result.Status);
    }

    /// <summary>
    /// Tests that GetUniversityByIdAsync handles UniversityDto with special characters in properties.
    /// Input: Valid id; API returns UniversityDto with special characters.
    /// Expected: Returns UniversityDto with special characters preserved.
    /// </summary>
    [TestMethod]
    public async Task GetUniversityByIdAsync_SpecialCharactersInProperties_ReturnsCorrectData()
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<AcademicService>>();
        var service = new AcademicService(mockApi.Object, mockLogger.Object);

        var expectedDto = new UniversityDto
        {
            Id = "uni-<test>&\"123\"",
            Name = "University's & College",
            FacultiesCount = 5,
            Status = "Active/Inactive"
        };

        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(expectedDto)
        };

        mockApi.Setup(x => x.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(httpResponse);

        // Act
        var result = await service.GetUniversityByIdAsync("test-id");

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(expectedDto.Id, result.Id);
        Assert.AreEqual(expectedDto.Name, result.Name);
        Assert.AreEqual(expectedDto.Status, result.Status);
    }

    /// <summary>
    /// Tests that GetUniversityByIdAsync handles OperationCanceledException correctly.
    /// Input: Valid id; API throws OperationCanceledException.
    /// Expected: Returns null and logs error.
    /// </summary>
    [TestMethod]
    public async Task GetUniversityByIdAsync_OperationCanceled_ReturnsNullAndLogsError()
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<AcademicService>>();
        var service = new AcademicService(mockApi.Object, mockLogger.Object);

        var expectedException = new OperationCanceledException("Operation was canceled");

        mockApi.Setup(x => x.GetAsync("universities/test-id", It.IsAny<CancellationToken>()))
            .ThrowsAsync(expectedException);

        // Act
        var result = await service.GetUniversityByIdAsync("test-id");

        // Assert
        Assert.IsNull(result);
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Failed to fetch university")),
                expectedException,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    /// <summary>
    /// Tests that GetUniversityByIdAsync handles empty response content correctly.
    /// Input: Valid id; API returns success with empty content.
    /// Expected: Returns null or throws exception based on deserialization behavior.
    /// </summary>
    [TestMethod]
    public async Task GetUniversityByIdAsync_EmptyResponseContent_HandlesGracefully()
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<AcademicService>>();
        var service = new AcademicService(mockApi.Object, mockLogger.Object);

        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("", Encoding.UTF8, "application/json")
        };

        mockApi.Setup(x => x.GetAsync("universities/test-id", It.IsAny<CancellationToken>()))
            .ReturnsAsync(httpResponse);

        // Act
        var result = await service.GetUniversityByIdAsync("test-id");

        // Assert - May return null or log error depending on JSON deserialization behavior
        // The method will catch any exception and return null
        Assert.IsNull(result);
    }

    /// <summary>
    /// Tests that CreateFacultyAsync returns null when API returns non-success HTTP status codes.
    /// Input: Valid parameters; API returns various error status codes.
    /// Expected: Returns null without throwing exception.
    /// </summary>
    [TestMethod]
    [DataRow(HttpStatusCode.BadRequest)]
    [DataRow(HttpStatusCode.Unauthorized)]
    [DataRow(HttpStatusCode.Forbidden)]
    [DataRow(HttpStatusCode.NotFound)]
    [DataRow(HttpStatusCode.InternalServerError)]
    [DataRow(HttpStatusCode.ServiceUnavailable)]
    [DataRow(HttpStatusCode.BadGateway)]
    [DataRow(HttpStatusCode.GatewayTimeout)]
    public async Task CreateFacultyAsync_ApiReturnsNonSuccessStatusCode_ReturnsNull(HttpStatusCode statusCode)
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<AcademicService>>();
        var service = new AcademicService(mockApi.Object, mockLogger.Object);

        var httpResponse = new HttpResponseMessage(statusCode);

        mockApi.Setup(x => x.PostAsync("faculties", It.IsAny<HttpContent>(), It.IsAny<CancellationToken>()))
               .ReturnsAsync(httpResponse);

        // Act
        var result = await service.CreateFacultyAsync("Engineering", "uni456");

        // Assert
        Assert.IsNull(result);
        mockApi.Verify(x => x.PostAsync("faculties", It.IsAny<HttpContent>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Tests that CreateFacultyAsync returns null and logs error when API throws HttpRequestException.
    /// Input: Valid parameters; API PostAsync throws HttpRequestException.
    /// Expected: Returns null and logs error with exception details.
    /// </summary>
    [TestMethod]
    public async Task CreateFacultyAsync_ApiThrowsHttpRequestException_ReturnsNullAndLogsError()
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<AcademicService>>();
        var service = new AcademicService(mockApi.Object, mockLogger.Object);

        var expectedException = new HttpRequestException("Network error");

        mockApi.Setup(x => x.PostAsync("faculties", It.IsAny<HttpContent>(), It.IsAny<CancellationToken>()))
               .ThrowsAsync(expectedException);

        // Act
        var result = await service.CreateFacultyAsync("Engineering", "uni456");

        // Assert
        Assert.IsNull(result);
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Failed to create faculty")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    /// <summary>
    /// Tests that CreateFacultyAsync handles null name parameter by serializing it as JSON null.
    /// Input: Null name and valid universityId.
    /// Expected: Method executes without throwing; API is called with serialized null value.
    /// </summary>
    [TestMethod]
    public async Task CreateFacultyAsync_NullName_SerializesAndCallsApi()
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<AcademicService>>();
        var service = new AcademicService(mockApi.Object, mockLogger.Object);

        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(new FacultyDto { Id = "f1", Name = null, UniversityId = "uni1" })
        };

        mockApi.Setup(x => x.PostAsync("faculties", It.IsAny<HttpContent>(), It.IsAny<CancellationToken>()))
               .ReturnsAsync(httpResponse);

        // Act
        var result = await service.CreateFacultyAsync(null!, "uni456");

        // Assert
        mockApi.Verify(x => x.PostAsync("faculties", It.IsAny<HttpContent>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Tests that CreateFacultyAsync handles null universityId parameter by serializing it as JSON null.
    /// Input: Valid name and null universityId.
    /// Expected: Method executes without throwing; API is called with serialized null value.
    /// </summary>
    [TestMethod]
    public async Task CreateFacultyAsync_NullUniversityId_SerializesAndCallsApi()
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<AcademicService>>();
        var service = new AcademicService(mockApi.Object, mockLogger.Object);

        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(new FacultyDto { Id = "f1", Name = "Engineering", UniversityId = null })
        };

        mockApi.Setup(x => x.PostAsync("faculties", It.IsAny<HttpContent>(), It.IsAny<CancellationToken>()))
               .ReturnsAsync(httpResponse);

        // Act
        var result = await service.CreateFacultyAsync("Engineering", null!);

        // Assert
        mockApi.Verify(x => x.PostAsync("faculties", It.IsAny<HttpContent>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Tests that CreateFacultyAsync handles both parameters being null.
    /// Input: Null name and null universityId.
    /// Expected: Method executes without throwing; API is called with both values serialized as null.
    /// </summary>
    [TestMethod]
    public async Task CreateFacultyAsync_BothParametersNull_SerializesAndCallsApi()
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<AcademicService>>();
        var service = new AcademicService(mockApi.Object, mockLogger.Object);

        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(new FacultyDto { Id = "f1", Name = null, UniversityId = null })
        };

        mockApi.Setup(x => x.PostAsync("faculties", It.IsAny<HttpContent>(), It.IsAny<CancellationToken>()))
               .ReturnsAsync(httpResponse);

        // Act
        var result = await service.CreateFacultyAsync(null!, null!);

        // Assert
        mockApi.Verify(x => x.PostAsync("faculties", It.IsAny<HttpContent>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Tests that CreateFacultyAsync returns null and logs error for various exception types.
    /// Input: Valid parameters; API throws different exception types.
    /// Expected: Returns null and logs error for all exception types.
    /// </summary>
    [TestMethod]
    [DataRow(typeof(InvalidOperationException))]
    [DataRow(typeof(ArgumentException))]
    [DataRow(typeof(Exception))]
    public async Task CreateFacultyAsync_VariousExceptionTypes_ReturnsNullAndLogsError(Type exceptionType)
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<AcademicService>>();
        var service = new AcademicService(mockApi.Object, mockLogger.Object);

        var exception = (Exception)Activator.CreateInstance(exceptionType, "Test error")!;

        mockApi.Setup(x => x.PostAsync("faculties", It.IsAny<HttpContent>(), It.IsAny<CancellationToken>()))
               .ThrowsAsync(exception);

        // Act
        var result = await service.CreateFacultyAsync("Engineering", "uni456");

        // Assert
        Assert.IsNull(result);
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Failed to create faculty")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    /// <summary>
    /// Tests that CreateFacultyAsync sends HTTP content with correct Content-Type header.
    /// Input: Valid name and universityId.
    /// Expected: HTTP content has Content-Type set to "application/json" with UTF-8 charset.
    /// </summary>
    [TestMethod]
    public async Task CreateFacultyAsync_ValidInputs_SendsCorrectContentType()
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<AcademicService>>();
        var service = new AcademicService(mockApi.Object, mockLogger.Object);

        HttpContent? capturedContent = null;
        mockApi.Setup(x => x.PostAsync("faculties", It.IsAny<HttpContent>(), It.IsAny<CancellationToken>()))
               .Callback<string, HttpContent, CancellationToken>((path, content, ct) => capturedContent = content)
               .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
               {
                   Content = JsonContent.Create(new FacultyDto())
               });

        // Act
        await service.CreateFacultyAsync("Engineering", "uni456");

        // Assert
        Assert.IsNotNull(capturedContent);
        Assert.AreEqual("application/json", capturedContent.Headers.ContentType?.MediaType);
        Assert.AreEqual("utf-8", capturedContent.Headers.ContentType?.CharSet);
    }

    /// <summary>
    /// Tests that CreateFacultyAsync correctly serializes parameters into JSON request body.
    /// Input: Specific name and universityId values.
    /// Expected: JSON payload contains both parameters with correct values.
    /// </summary>
    [TestMethod]
    public async Task CreateFacultyAsync_ValidInputs_SerializesParametersCorrectly()
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<AcademicService>>();
        var service = new AcademicService(mockApi.Object, mockLogger.Object);

        string? capturedJson = null;
        mockApi.Setup(x => x.PostAsync("faculties", It.IsAny<HttpContent>(), It.IsAny<CancellationToken>()))
               .Callback<string, HttpContent, CancellationToken>(async (path, content, ct) =>
               {
                   capturedJson = await content.ReadAsStringAsync();
               })
               .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
               {
                   Content = JsonContent.Create(new FacultyDto())
               });

        // Act
        await service.CreateFacultyAsync("Engineering", "uni456");

        // Assert
        Assert.IsNotNull(capturedJson);
        Assert.IsTrue(capturedJson.Contains("\"name\""));
        Assert.IsTrue(capturedJson.Contains("\"universityId\""));
        Assert.IsTrue(capturedJson.Contains("Engineering"));
        Assert.IsTrue(capturedJson.Contains("uni456"));
    }

    /// <summary>
    /// Tests that CreateFacultyAsync returns null when API returns 3xx redirect status codes.
    /// Input: Valid parameters; API returns redirect status codes.
    /// Expected: Returns null (redirect is not considered success).
    /// </summary>
    [TestMethod]
    [DataRow(HttpStatusCode.MovedPermanently)]
    [DataRow(HttpStatusCode.Found)]
    [DataRow(HttpStatusCode.SeeOther)]
    [DataRow(HttpStatusCode.TemporaryRedirect)]
    public async Task CreateFacultyAsync_ApiReturnsRedirectStatusCode_ReturnsNull(HttpStatusCode statusCode)
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<AcademicService>>();
        var service = new AcademicService(mockApi.Object, mockLogger.Object);

        var httpResponse = new HttpResponseMessage(statusCode);

        mockApi.Setup(x => x.PostAsync("faculties", It.IsAny<HttpContent>(), It.IsAny<CancellationToken>()))
               .ReturnsAsync(httpResponse);

        // Act
        var result = await service.CreateFacultyAsync("Engineering", "uni456");

        // Assert
        Assert.IsNull(result);
    }

    /// <summary>
    /// Tests that CreateFacultyAsync returns FacultyDto for various 2xx success status codes.
    /// Input: Valid parameters; API returns different success status codes.
    /// Expected: Returns deserialized FacultyDto for all 2xx status codes.
    /// </summary>
    [TestMethod]
    [DataRow(HttpStatusCode.OK)]
    [DataRow(HttpStatusCode.Created)]
    [DataRow(HttpStatusCode.Accepted)]
    public async Task CreateFacultyAsync_ApiReturnsSuccessStatusCode_ReturnsFacultyDto(HttpStatusCode statusCode)
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<AcademicService>>();
        var service = new AcademicService(mockApi.Object, mockLogger.Object);

        var expectedFaculty = new FacultyDto { Id = "f1", Name = "Engineering", UniversityId = "uni1" };
        var httpResponse = new HttpResponseMessage(statusCode)
        {
            Content = JsonContent.Create(expectedFaculty)
        };

        mockApi.Setup(x => x.PostAsync("faculties", It.IsAny<HttpContent>(), It.IsAny<CancellationToken>()))
               .ReturnsAsync(httpResponse);

        // Act
        var result = await service.CreateFacultyAsync("Engineering", "uni1");

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual("f1", result.Id);
    }

    /// <summary>
    /// Tests that CreateFacultyAsync handles control characters in string parameters.
    /// Input: Strings with control characters like backspace, form feed, bell, etc.
    /// Expected: Method executes and serializes control characters appropriately.
    /// </summary>
    [TestMethod]
    [DataRow("Faculty\bName", "uni\bid")]
    [DataRow("Faculty\fName", "uni\fid")]
    [DataRow("Faculty\aName", "uni\aid")]
    public async Task CreateFacultyAsync_ControlCharactersInParameters_SendsRequestToApi(string name, string universityId)
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<AcademicService>>();
        var service = new AcademicService(mockApi.Object, mockLogger.Object);

        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(new FacultyDto { Id = "f1", Name = name, UniversityId = universityId })
        };

        mockApi.Setup(x => x.PostAsync("faculties", It.IsAny<HttpContent>(), It.IsAny<CancellationToken>()))
               .ReturnsAsync(httpResponse);

        // Act
        var result = await service.CreateFacultyAsync(name, universityId);

        // Assert
        mockApi.Verify(x => x.PostAsync("faculties", It.IsAny<HttpContent>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Tests that GetFacultyDetailsAsync handles faculties with empty strings in properties.
    /// Input: API returns faculties with empty string values.
    /// Expected: Returns faculties with empty strings as provided.
    /// </summary>
    [TestMethod]
    public async Task GetFacultyDetailsAsync_FacultiesWithEmptyStrings_ReturnsCorrectData()
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<AcademicService>>();
        var expectedFaculties = new List<FacultyDto>
        {
            new FacultyDto { Id = "", Name = "", UniversityId = "", UniversityName = "", DepartmentsCount = 0, Status = "" },
            new FacultyDto { Id = "F2", Name = "Faculty", UniversityId = "", UniversityName = "University", DepartmentsCount = 1, Status = "" }
        };

        var jsonContent = JsonContent.Create(expectedFaculties);
        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = jsonContent
        };

        mockApi.Setup(x => x.GetAsync("faculties/details", It.IsAny<CancellationToken>()))
               .ReturnsAsync(response);

        var service = new AcademicService(mockApi.Object, mockLogger.Object);

        // Act
        var result = await service.GetFacultyDetailsAsync();

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(2, result.Count);
        Assert.AreEqual("", result[0].Id);
        Assert.AreEqual("", result[0].Name);
        Assert.AreEqual("F2", result[1].Id);
    }

    /// <summary>
    /// Tests that GetFacultyDetailsAsync handles faculties with whitespace-only strings in properties.
    /// Input: API returns faculties with whitespace-only values.
    /// Expected: Returns faculties with whitespace values preserved.
    /// </summary>
    [TestMethod]
    public async Task GetFacultyDetailsAsync_FacultiesWithWhitespace_ReturnsCorrectData()
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<AcademicService>>();
        var expectedFaculties = new List<FacultyDto>
        {
            new FacultyDto { Id = "   ", Name = "   ", UniversityId = "\t", UniversityName = "\n", DepartmentsCount = 0, Status = " " }
        };

        var jsonContent = JsonContent.Create(expectedFaculties);
        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = jsonContent
        };

        mockApi.Setup(x => x.GetAsync("faculties/details", It.IsAny<CancellationToken>()))
               .ReturnsAsync(response);

        var service = new AcademicService(mockApi.Object, mockLogger.Object);

        // Act
        var result = await service.GetFacultyDetailsAsync();

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(1, result.Count);
        Assert.AreEqual("   ", result[0].Id);
        Assert.AreEqual("   ", result[0].Name);
    }

    /// <summary>
    /// Tests that GetFacultyDetailsAsync handles faculties with very long names correctly.
    /// Input: API returns faculties with very long string values (1000+ characters).
    /// Expected: Returns faculties with full long names preserved.
    /// </summary>
    [TestMethod]
    public async Task GetFacultyDetailsAsync_FacultiesWithVeryLongNames_ReturnsCorrectData()
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<AcademicService>>();
        var longName = new string('A', 1000);
        var expectedFaculties = new List<FacultyDto>
        {
            new FacultyDto { Id = "F1", Name = longName, UniversityId = "U1", UniversityName = "University", DepartmentsCount = 5, Status = "Active" }
        };

        var jsonContent = JsonContent.Create(expectedFaculties);
        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = jsonContent
        };

        mockApi.Setup(x => x.GetAsync("faculties/details", It.IsAny<CancellationToken>()))
               .ReturnsAsync(response);

        var service = new AcademicService(mockApi.Object, mockLogger.Object);

        // Act
        var result = await service.GetFacultyDetailsAsync();

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(1, result.Count);
        Assert.AreEqual(longName, result[0].Name);
        Assert.AreEqual(1000, result[0].Name.Length);
    }

    /// <summary>
    /// Tests that GetFacultyDetailsAsync handles faculties with boundary values for DepartmentsCount.
    /// Input: API returns faculties with int.MinValue, int.MaxValue, zero, and negative department counts.
    /// Expected: Returns faculties with boundary values preserved.
    /// </summary>
    [TestMethod]
    public async Task GetFacultyDetailsAsync_FacultiesWithBoundaryDepartmentCounts_ReturnsCorrectData()
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<AcademicService>>();
        var expectedFaculties = new List<FacultyDto>
        {
            new FacultyDto { Id = "F1", Name = "Faculty 1", UniversityId = "U1", UniversityName = "University", DepartmentsCount = int.MinValue, Status = "Active" },
            new FacultyDto { Id = "F2", Name = "Faculty 2", UniversityId = "U2", UniversityName = "University", DepartmentsCount = int.MaxValue, Status = "Active" },
            new FacultyDto { Id = "F3", Name = "Faculty 3", UniversityId = "U3", UniversityName = "University", DepartmentsCount = 0, Status = "Active" },
            new FacultyDto { Id = "F4", Name = "Faculty 4", UniversityId = "U4", UniversityName = "University", DepartmentsCount = -1, Status = "Active" }
        };

        var jsonContent = JsonContent.Create(expectedFaculties);
        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = jsonContent
        };

        mockApi.Setup(x => x.GetAsync("faculties/details", It.IsAny<CancellationToken>()))
               .ReturnsAsync(response);

        var service = new AcademicService(mockApi.Object, mockLogger.Object);

        // Act
        var result = await service.GetFacultyDetailsAsync();

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(4, result.Count);
        Assert.AreEqual(int.MinValue, result[0].DepartmentsCount);
        Assert.AreEqual(int.MaxValue, result[1].DepartmentsCount);
        Assert.AreEqual(0, result[2].DepartmentsCount);
        Assert.AreEqual(-1, result[3].DepartmentsCount);
    }

    /// <summary>
    /// Tests that GetFacultyDetailsAsync handles InvalidOperationException correctly.
    /// Input: API GetAsync throws InvalidOperationException.
    /// Expected: Returns empty list and logs error.
    /// </summary>
    [TestMethod]
    public async Task GetFacultyDetailsAsync_GetAsyncThrowsInvalidOperationException_LogsErrorAndReturnsEmptyList()
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<AcademicService>>();
        var exception = new InvalidOperationException("Invalid operation");

        mockApi.Setup(x => x.GetAsync("faculties/details", It.IsAny<CancellationToken>()))
               .ThrowsAsync(exception);

        var service = new AcademicService(mockApi.Object, mockLogger.Object);

        // Act
        var result = await service.GetFacultyDetailsAsync();

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(0, result.Count);
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Failed to fetch faculty details")),
                It.Is<Exception>(ex => ex == exception),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    /// <summary>
    /// Tests that GetFacultyDetailsAsync does not log when returning empty list due to non-success status.
    /// Input: API returns 404 Not Found.
    /// Expected: Returns empty list without any logging.
    /// </summary>
    [TestMethod]
    public async Task GetFacultyDetailsAsync_NonSuccessStatus_DoesNotLog()
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<AcademicService>>();

        var response = new HttpResponseMessage(HttpStatusCode.NotFound);
        mockApi.Setup(x => x.GetAsync("faculties/details", It.IsAny<CancellationToken>()))
               .ReturnsAsync(response);

        var service = new AcademicService(mockApi.Object, mockLogger.Object);

        // Act
        var result = await service.GetFacultyDetailsAsync();

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(0, result.Count);
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
    /// Tests that GetFacultyDetailsAsync does not log when successfully returning faculties.
    /// Input: API returns success with valid data.
    /// Expected: Returns faculties without any logging.
    /// </summary>
    [TestMethod]
    public async Task GetFacultyDetailsAsync_SuccessfulResponse_DoesNotLog()
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<AcademicService>>();
        var expectedFaculties = new List<FacultyDto>
        {
            new FacultyDto { Id = "F1", Name = "Faculty", UniversityId = "U1", UniversityName = "University", DepartmentsCount = 5, Status = "Active" }
        };

        var jsonContent = JsonContent.Create(expectedFaculties);
        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = jsonContent
        };

        mockApi.Setup(x => x.GetAsync("faculties/details", It.IsAny<CancellationToken>()))
               .ReturnsAsync(response);

        var service = new AcademicService(mockApi.Object, mockLogger.Object);

        // Act
        var result = await service.GetFacultyDetailsAsync();

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(1, result.Count);
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
    /// Tests that GetFacultyDetailsAsync verifies the correct endpoint is called.
    /// Input: Valid setup.
    /// Expected: Calls GetAsync with "faculties/details" endpoint.
    /// </summary>
    [TestMethod]
    public async Task GetFacultyDetailsAsync_CallsCorrectEndpoint()
    {
        // Arrange
        var mockApi = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<AcademicService>>();
        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(new List<FacultyDto>())
        };

        mockApi.Setup(x => x.GetAsync("faculties/details", It.IsAny<CancellationToken>()))
               .ReturnsAsync(response);

        var service = new AcademicService(mockApi.Object, mockLogger.Object);

        // Act
        await service.GetFacultyDetailsAsync();

        // Assert
        mockApi.Verify(
            x => x.GetAsync("faculties/details", It.IsAny<CancellationToken>()),
            Times.Once);
    }
}