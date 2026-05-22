using System;
using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Ssomero.Interfaces;
using Ssomero.Models;
using Ssomero.ViewModels;

namespace Ssomero.ViewModels.UnitTests;




/// <summary>
/// Unit tests for the <see cref="CourseDetailViewModel"/> class.
/// </summary>
[TestClass]
public class CourseDetailViewModelTests
{
    /// <summary>
    /// Tests that the constructor successfully creates an instance when provided with a valid ICoursesService.
    /// Verifies that the instance is created without throwing an exception.
    /// </summary>
    [TestMethod]
    public void Constructor_WithValidCoursesService_CreatesInstance()
    {
        // Arrange
        Mock<ICoursesService> mockCoursesService = new Mock<ICoursesService>();

        // Act
        CourseDetailViewModel viewModel = new CourseDetailViewModel(mockCoursesService.Object);

        // Assert
        Assert.IsNotNull(viewModel);
        Assert.IsInstanceOfType(viewModel, typeof(CourseDetailViewModel));
    }

    /// <summary>
    /// Tests that the constructor handles a null ICoursesService parameter.
    /// Since the parameter is non-nullable but has no explicit null check,
    /// this test documents the current behavior and will catch any future changes
    /// that add validation (e.g., ArgumentNullException).
    /// </summary>
    [TestMethod]
    public void Constructor_WithNullCoursesService_DoesNotThrow()
    {
        // Arrange
        ICoursesService? nullCoursesService = null;

        // Act
        CourseDetailViewModel viewModel = new CourseDetailViewModel(nullCoursesService!);

        // Assert
        Assert.IsNotNull(viewModel);
    }

    /// <summary>
    /// Tests that the Course property returns a non-null CourseDto instance upon initialization.
    /// This verifies that the property is properly initialized with a default CourseDto object.
    /// </summary>
    [TestMethod]
    public void Course_InitialState_ReturnsNonNullCourseDto()
    {
        // Arrange
        var mockCoursesService = new Mock<ICoursesService>();
        var viewModel = new CourseDetailViewModel(mockCoursesService.Object);

        // Act
        var result = viewModel.Course;

        // Assert
        Assert.IsNotNull(result);
        Assert.IsInstanceOfType(result, typeof(CourseDto));
    }

    /// <summary>
    /// Tests that the Course property getter returns the same instance on multiple calls.
    /// This verifies that the property returns the backing field reference and doesn't create
    /// new instances on each access.
    /// </summary>
    [TestMethod]
    public void Course_MultipleGets_ReturnsSameInstance()
    {
        // Arrange
        var mockCoursesService = new Mock<ICoursesService>();
        var viewModel = new CourseDetailViewModel(mockCoursesService.Object);

        // Act
        var firstAccess = viewModel.Course;
        var secondAccess = viewModel.Course;

        // Assert
        Assert.AreSame(firstAccess, secondAccess);
    }

    /// <summary>
    /// Tests that the Course property returns a CourseDto with default property values
    /// upon initialization. This verifies the initial state of the CourseDto object.
    /// </summary>
    [TestMethod]
    public void Course_InitialState_HasDefaultValues()
    {
        // Arrange
        var mockCoursesService = new Mock<ICoursesService>();
        var viewModel = new CourseDetailViewModel(mockCoursesService.Object);

        // Act
        var result = viewModel.Course;

        // Assert
        Assert.AreEqual(string.Empty, result.Id);
        Assert.AreEqual(string.Empty, result.Name);
        Assert.AreEqual(string.Empty, result.Lecturer);
        Assert.AreEqual(string.Empty, result.LecturerId);
        Assert.AreEqual(0, result.Progress);
        Assert.AreEqual(0, result.EnrolledStudents);
        Assert.AreEqual(0, result.TotalSessions);
        Assert.AreEqual(0, result.CompletedSessions);
        Assert.IsNull(result.ClassRepId);
        Assert.IsNull(result.ClassRepName);
    }

    /// <summary>
    /// Tests that LoadAsync returns early without calling GetCourseAsync when id is null.
    /// </summary>
    [TestMethod]
    public async Task LoadAsync_NullId_ReturnsEarlyWithoutCallingService()
    {
        // Arrange
        var mockCoursesService = new Mock<ICoursesService>();
        var viewModel = new CourseDetailViewModel(mockCoursesService.Object);

        // Act
        await viewModel.LoadAsync(null!);

        // Assert
        mockCoursesService.Verify(x => x.GetCourseAsync(It.IsAny<string>()), Times.Never);
    }

    /// <summary>
    /// Tests that LoadAsync returns early without calling GetCourseAsync when id is an empty string.
    /// </summary>
    [TestMethod]
    public async Task LoadAsync_EmptyId_ReturnsEarlyWithoutCallingService()
    {
        // Arrange
        var mockCoursesService = new Mock<ICoursesService>();
        var viewModel = new CourseDetailViewModel(mockCoursesService.Object);

        // Act
        await viewModel.LoadAsync(string.Empty);

        // Assert
        mockCoursesService.Verify(x => x.GetCourseAsync(It.IsAny<string>()), Times.Never);
    }

    /// <summary>
    /// Tests that LoadAsync sets Course and Title properties when GetCourseAsync returns a valid CourseDto.
    /// </summary>
    [TestMethod]
    public async Task LoadAsync_ValidIdAndCourseReturned_SetsCourseAndTitleProperties()
    {
        // Arrange
        var mockCoursesService = new Mock<ICoursesService>();
        var expectedCourse = new CourseDto
        {
            Id = "course123",
            Name = "Test Course",
            Lecturer = "Test Lecturer",
            LecturerId = "lecturer123",
            Progress = 50,
            EnrolledStudents = 30,
            TotalSessions = 10,
            CompletedSessions = 5
        };
        mockCoursesService.Setup(x => x.GetCourseAsync("course123")).ReturnsAsync(expectedCourse);
        var viewModel = new CourseDetailViewModel(mockCoursesService.Object);

        // Act
        await viewModel.LoadAsync("course123");

        // Assert
        Assert.AreSame(expectedCourse, viewModel.Course);
        Assert.AreEqual("Test Course", viewModel.Title);
        mockCoursesService.Verify(x => x.GetCourseAsync("course123"), Times.Once);
    }

    /// <summary>
    /// Tests that LoadAsync sets Course to a new CourseDto and Title to empty string when GetCourseAsync returns null.
    /// </summary>
    [TestMethod]
    public async Task LoadAsync_ValidIdButNullCourseReturned_SetsNewCourseDtoAndEmptyTitle()
    {
        // Arrange
        var mockCoursesService = new Mock<ICoursesService>();
        mockCoursesService.Setup(x => x.GetCourseAsync("course123")).ReturnsAsync((CourseDto?)null);
        var viewModel = new CourseDetailViewModel(mockCoursesService.Object);

        // Act
        await viewModel.LoadAsync("course123");

        // Assert
        Assert.IsNotNull(viewModel.Course);
        Assert.AreEqual(string.Empty, viewModel.Course.Name);
        Assert.AreEqual(string.Empty, viewModel.Title);
        mockCoursesService.Verify(x => x.GetCourseAsync("course123"), Times.Once);
    }

    /// <summary>
    /// Tests that LoadAsync processes whitespace-only strings as valid input and calls GetCourseAsync.
    /// IsNullOrEmpty does not treat whitespace-only strings as empty.
    /// </summary>
    /// <param name="whitespaceId">The whitespace string to test.</param>
    [TestMethod]
    [DataRow(" ")]
    [DataRow("  ")]
    [DataRow("\t")]
    [DataRow("\n")]
    [DataRow("\r\n")]
    public async Task LoadAsync_WhitespaceId_CallsServiceWithWhitespaceString(string whitespaceId)
    {
        // Arrange
        var mockCoursesService = new Mock<ICoursesService>();
        var course = new CourseDto { Id = "test", Name = "Test Name" };
        mockCoursesService.Setup(x => x.GetCourseAsync(whitespaceId)).ReturnsAsync(course);
        var viewModel = new CourseDetailViewModel(mockCoursesService.Object);

        // Act
        await viewModel.LoadAsync(whitespaceId);

        // Assert
        mockCoursesService.Verify(x => x.GetCourseAsync(whitespaceId), Times.Once);
    }

    /// <summary>
    /// Tests that LoadAsync handles special characters in the id parameter correctly.
    /// </summary>
    [TestMethod]
    [DataRow("course@123")]
    [DataRow("course#test")]
    [DataRow("course$%^&*")]
    [DataRow("课程123")]
    [DataRow("course\u0000test")]
    public async Task LoadAsync_IdWithSpecialCharacters_CallsServiceWithSpecialCharacterId(string specialId)
    {
        // Arrange
        var mockCoursesService = new Mock<ICoursesService>();
        var course = new CourseDto { Id = specialId, Name = "Special Course" };
        mockCoursesService.Setup(x => x.GetCourseAsync(specialId)).ReturnsAsync(course);
        var viewModel = new CourseDetailViewModel(mockCoursesService.Object);

        // Act
        await viewModel.LoadAsync(specialId);

        // Assert
        Assert.AreEqual(course, viewModel.Course);
        Assert.AreEqual("Special Course", viewModel.Title);
        mockCoursesService.Verify(x => x.GetCourseAsync(specialId), Times.Once);
    }

    /// <summary>
    /// Tests that LoadAsync handles very long id strings correctly.
    /// </summary>
    [TestMethod]
    public async Task LoadAsync_VeryLongId_CallsServiceWithLongString()
    {
        // Arrange
        var mockCoursesService = new Mock<ICoursesService>();
        var longId = new string('a', 10000);
        var course = new CourseDto { Id = longId, Name = "Long ID Course" };
        mockCoursesService.Setup(x => x.GetCourseAsync(longId)).ReturnsAsync(course);
        var viewModel = new CourseDetailViewModel(mockCoursesService.Object);

        // Act
        await viewModel.LoadAsync(longId);

        // Assert
        Assert.AreEqual(course, viewModel.Course);
        Assert.AreEqual("Long ID Course", viewModel.Title);
        mockCoursesService.Verify(x => x.GetCourseAsync(longId), Times.Once);
    }

    /// <summary>
    /// Tests that LoadAsync correctly sets Title property when Course.Name has special values.
    /// </summary>
    [TestMethod]
    [DataRow("")]
    [DataRow("Course with emoji 🎓")]
    [DataRow("Course\nWith\nNewlines")]
    public async Task LoadAsync_CourseNameWithSpecialValues_SetsTitleCorrectly(string courseName)
    {
        // Arrange
        var mockCoursesService = new Mock<ICoursesService>();
        var course = new CourseDto { Id = "course123", Name = courseName };
        mockCoursesService.Setup(x => x.GetCourseAsync("course123")).ReturnsAsync(course);
        var viewModel = new CourseDetailViewModel(mockCoursesService.Object);

        // Act
        await viewModel.LoadAsync("course123");

        // Assert
        Assert.AreEqual(courseName, viewModel.Title);
    }

    /// <summary>
    /// Tests that LoadAsync sets all CourseDto properties correctly when a full course object is returned.
    /// </summary>
    [TestMethod]
    public async Task LoadAsync_CompleteCourseDtoReturned_SetsAllPropertiesCorrectly()
    {
        // Arrange
        var mockCoursesService = new Mock<ICoursesService>();
        var expectedCourse = new CourseDto
        {
            Id = "course789",
            Name = "Complete Course",
            Lecturer = "Dr. Smith",
            LecturerId = "lecturer456",
            Progress = 75,
            EnrolledStudents = 100,
            TotalSessions = 20,
            CompletedSessions = 15,
            ClassRepId = "rep123",
            ClassRepName = "John Doe"
        };
        mockCoursesService.Setup(x => x.GetCourseAsync("course789")).ReturnsAsync(expectedCourse);
        var viewModel = new CourseDetailViewModel(mockCoursesService.Object);

        // Act
        await viewModel.LoadAsync("course789");

        // Assert
        Assert.AreSame(expectedCourse, viewModel.Course);
        Assert.AreEqual("course789", viewModel.Course.Id);
        Assert.AreEqual("Complete Course", viewModel.Course.Name);
        Assert.AreEqual("Dr. Smith", viewModel.Course.Lecturer);
        Assert.AreEqual("lecturer456", viewModel.Course.LecturerId);
        Assert.AreEqual(75, viewModel.Course.Progress);
        Assert.AreEqual(100, viewModel.Course.EnrolledStudents);
        Assert.AreEqual(20, viewModel.Course.TotalSessions);
        Assert.AreEqual(15, viewModel.Course.CompletedSessions);
        Assert.AreEqual("rep123", viewModel.Course.ClassRepId);
        Assert.AreEqual("John Doe", viewModel.Course.ClassRepName);
        Assert.AreEqual("Complete Course", viewModel.Title);
    }

    /// <summary>
    /// Tests that LoadAsync handles CourseDto with nullable properties set to null correctly.
    /// </summary>
    [TestMethod]
    public async Task LoadAsync_CourseDtoWithNullablePropertiesNull_HandlesNullsCorrectly()
    {
        // Arrange
        var mockCoursesService = new Mock<ICoursesService>();
        var course = new CourseDto
        {
            Id = "course999",
            Name = "Course Without Class Rep",
            Lecturer = "Prof. Jones",
            LecturerId = "lecturer789",
            Progress = 0,
            EnrolledStudents = 50,
            TotalSessions = 15,
            CompletedSessions = 0,
            ClassRepId = null,
            ClassRepName = null
        };
        mockCoursesService.Setup(x => x.GetCourseAsync("course999")).ReturnsAsync(course);
        var viewModel = new CourseDetailViewModel(mockCoursesService.Object);

        // Act
        await viewModel.LoadAsync("course999");

        // Assert
        Assert.AreSame(course, viewModel.Course);
        Assert.IsNull(viewModel.Course.ClassRepId);
        Assert.IsNull(viewModel.Course.ClassRepName);
        Assert.AreEqual("Course Without Class Rep", viewModel.Title);
    }

    /// <summary>
    /// Tests that LoadAsync replaces the previous Course when called multiple times with different IDs.
    /// Each call should update the Course property to the newly retrieved CourseDto.
    /// </summary>
    [TestMethod]
    public async Task LoadAsync_CalledMultipleTimesWithDifferentIds_ReplacesCourseProperly()
    {
        // Arrange
        var mockCoursesService = new Mock<ICoursesService>();
        var firstCourse = new CourseDto { Id = "course1", Name = "First Course" };
        var secondCourse = new CourseDto { Id = "course2", Name = "Second Course" };

        mockCoursesService.Setup(x => x.GetCourseAsync("course1")).ReturnsAsync(firstCourse);
        mockCoursesService.Setup(x => x.GetCourseAsync("course2")).ReturnsAsync(secondCourse);

        var viewModel = new CourseDetailViewModel(mockCoursesService.Object);

        // Act
        await viewModel.LoadAsync("course1");
        var firstResult = viewModel.Course;
        var firstTitle = viewModel.Title;

        await viewModel.LoadAsync("course2");
        var secondResult = viewModel.Course;
        var secondTitle = viewModel.Title;

        // Assert
        Assert.AreSame(firstCourse, firstResult);
        Assert.AreEqual("First Course", firstTitle);
        Assert.AreSame(secondCourse, secondResult);
        Assert.AreEqual("Second Course", secondTitle);
        mockCoursesService.Verify(x => x.GetCourseAsync("course1"), Times.Once);
        mockCoursesService.Verify(x => x.GetCourseAsync("course2"), Times.Once);
    }

    /// <summary>
    /// Tests that LoadAsync properly transitions from a null course result to a valid course result.
    /// This verifies that Course and Title are updated correctly even after being set to default values.
    /// </summary>
    [TestMethod]
    public async Task LoadAsync_TransitionFromNullToValidCourse_UpdatesCourseAndTitle()
    {
        // Arrange
        var mockCoursesService = new Mock<ICoursesService>();
        var validCourse = new CourseDto { Id = "course123", Name = "Valid Course" };

        mockCoursesService.Setup(x => x.GetCourseAsync("null-course")).ReturnsAsync((CourseDto?)null);
        mockCoursesService.Setup(x => x.GetCourseAsync("valid-course")).ReturnsAsync(validCourse);

        var viewModel = new CourseDetailViewModel(mockCoursesService.Object);

        // Act
        await viewModel.LoadAsync("null-course");
        var firstCourse = viewModel.Course;
        var firstTitle = viewModel.Title;

        await viewModel.LoadAsync("valid-course");
        var secondCourse = viewModel.Course;
        var secondTitle = viewModel.Title;

        // Assert
        Assert.IsNotNull(firstCourse);
        Assert.AreEqual(string.Empty, firstTitle);
        Assert.AreSame(validCourse, secondCourse);
        Assert.AreEqual("Valid Course", secondTitle);
    }

    /// <summary>
    /// Tests that LoadAsync does not modify Course or Title when id is null or empty.
    /// The properties should retain their previous values when the method returns early.
    /// </summary>
    [TestMethod]
    [DataRow(null)]
    [DataRow("")]
    public async Task LoadAsync_NullOrEmptyId_DoesNotModifyCourseOrTitle(string? invalidId)
    {
        // Arrange
        var mockCoursesService = new Mock<ICoursesService>();
        var initialCourse = new CourseDto { Id = "initial", Name = "Initial Course" };

        mockCoursesService.Setup(x => x.GetCourseAsync("initial")).ReturnsAsync(initialCourse);

        var viewModel = new CourseDetailViewModel(mockCoursesService.Object);
        await viewModel.LoadAsync("initial");

        var courseBeforeInvalidCall = viewModel.Course;
        var titleBeforeInvalidCall = viewModel.Title;

        // Act
        await viewModel.LoadAsync(invalidId!);

        // Assert
        Assert.AreSame(courseBeforeInvalidCall, viewModel.Course);
        Assert.AreEqual(titleBeforeInvalidCall, viewModel.Title);
        mockCoursesService.Verify(x => x.GetCourseAsync("initial"), Times.Once);
        mockCoursesService.Verify(x => x.GetCourseAsync(It.IsAny<string>()), Times.Once);
    }

    /// <summary>
    /// Tests that LoadAsync calls GetCourseAsync exactly once for a valid ID.
    /// This ensures no duplicate or redundant service calls are made.
    /// </summary>
    [TestMethod]
    public async Task LoadAsync_ValidId_CallsGetCourseAsyncExactlyOnce()
    {
        // Arrange
        var mockCoursesService = new Mock<ICoursesService>();
        var course = new CourseDto { Id = "course123", Name = "Test Course" };
        mockCoursesService.Setup(x => x.GetCourseAsync("course123")).ReturnsAsync(course);
        var viewModel = new CourseDetailViewModel(mockCoursesService.Object);

        // Act
        await viewModel.LoadAsync("course123");

        // Assert
        mockCoursesService.Verify(x => x.GetCourseAsync("course123"), Times.Once);
        mockCoursesService.Verify(x => x.GetCourseAsync(It.IsAny<string>()), Times.Once);
    }
}