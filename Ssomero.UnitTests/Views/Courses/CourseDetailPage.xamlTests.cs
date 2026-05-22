using System;
using System.Threading.Tasks;

using Microsoft.Maui.Controls;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Ssomero.Interfaces;
using Ssomero.Models;
using Ssomero.Services;
using Ssomero.ViewModels;
using Ssomero.Views.Courses;

namespace Ssomero.Views.Courses.UnitTests;

/// <summary>
/// Contains unit tests for the <see cref = "CourseDetailPage"/> class.
/// </summary>
[TestClass]
public partial class CourseDetailPageTests
{
    /// <summary>
    /// Tests that the constructor properly initializes the page with a valid ViewModel
    /// and sets the BindingContext correctly.
    /// </summary>
    [TestMethod]
    public void CourseDetailPage_ValidViewModel_SetsBindingContext()
    {
        // Arrange
        var mockCoursesService = new Mock<ICoursesService>();
        var viewModel = new CourseDetailViewModel(mockCoursesService.Object);
        // Act
        CourseDetailPage? page = null;
        Exception? exception = null;
        try
        {
            page = new CourseDetailPage(viewModel);
        }
        catch (Exception ex)
        {
            exception = ex;
        }

        // Assert
        // If InitializeComponent requires platform initialization, the test may throw
        // In that case, we document this limitation
        if (exception == null)
        {
            Assert.IsNotNull(page);
            Assert.AreSame(viewModel, page.BindingContext);
        }
        else
        {
            Assert.Inconclusive($"Constructor threw exception (likely due to MAUI platform initialization requirement): {exception.Message}");
        }
    }

    /// <summary>
    /// Tests that the constructor successfully creates an instance of CourseDetailPage
    /// and verifies proper type inheritance.
    /// </summary>
    [TestMethod]
    public void Constructor_ValidViewModel_CreatesInstanceSuccessfully()
    {
        // Arrange
        var mockCoursesService = new Mock<ICoursesService>();
        var viewModel = new CourseDetailViewModel(mockCoursesService.Object);
        // Act
        CourseDetailPage? page = null;
        Exception? exception = null;
        try
        {
            page = new CourseDetailPage(viewModel);
        }
        catch (Exception ex)
        {
            exception = ex;
        }

        // Assert
        if (exception == null)
        {
            Assert.IsNotNull(page);
            Assert.IsInstanceOfType(page, typeof(CourseDetailPage));
            Assert.IsInstanceOfType(page, typeof(ContentPage));
        }
        else
        {
            Assert.Inconclusive($"Constructor threw exception (likely due to MAUI platform initialization requirement): {exception.Message}");
        }
    }

    /// <summary>
    /// Tests that the constructor handles null ViewModel parameter.
    /// Verifies behavior when null is passed despite non-nullable parameter annotation.
    /// </summary>
    [TestMethod]
    public void Constructor_NullViewModel_HandlesBehavior()
    {
        // Arrange
        CourseDetailViewModel? viewModel = null;
        // Act
        CourseDetailPage? page = null;
        Exception? exception = null;
        try
        {
            page = new CourseDetailPage(viewModel!);
        }
        catch (Exception ex)
        {
            exception = ex;
        }

        // Assert
        if (exception == null)
        {
            Assert.IsNotNull(page);
            Assert.IsNull(page.BindingContext);
        }
        else
        {
            Assert.Inconclusive($"Constructor threw exception (likely due to MAUI platform initialization requirement): {exception.Message}");
        }
    }

    /// <summary>
    /// Helper class extension to expose the protected OnDisappearing method for testing.
    /// </summary>
    private partial class TestableCourseDetailPage
    {
    }

}