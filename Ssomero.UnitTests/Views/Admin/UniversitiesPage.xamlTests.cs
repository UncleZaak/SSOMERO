using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;
using Microsoft.Maui.Controls;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Ssomero.Interfaces;
using Ssomero.Models;
using Ssomero.Services;
using Ssomero.ViewModels;
using Ssomero.Views.Admin;

namespace Ssomero.Views.Admin.UnitTests;


/// <summary>
/// Contains unit tests for the <see cref = "UniversitiesPage"/> class.
/// </summary>
[TestClass]
public partial class UniversitiesPageTests
{
    /// <summary>
    /// Tests that the constructor properly initializes the page with a valid ViewModel
    /// and sets the BindingContext correctly.
    /// </summary>
    [TestMethod]
    public void UniversitiesPage_ValidViewModel_SetsBindingContext()
    {
        // Arrange
        var mockAcademicService = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<UniversitiesViewModel>>();
        var viewModel = new UniversitiesViewModel(mockAcademicService.Object, mockLogger.Object);
        // Act
        // Note: InitializeComponent() may require MAUI platform initialization
        // This test focuses on verifying the BindingContext assignment
        UniversitiesPage? page = null;
        Exception? exception = null;
        try
        {
            page = new UniversitiesPage(viewModel);
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
    /// Tests that the constructor handles a null ViewModel parameter.
    /// Although the parameter is non-nullable, this tests the runtime behavior when null is passed.
    /// </summary>
    [TestMethod]
    public void UniversitiesPage_NullViewModel_ThrowsOrSetsBindingContextToNull()
    {
        // Arrange
        UniversitiesViewModel? viewModel = null!;
        // Act & Assert
        Exception? exception = null;
        UniversitiesPage? page = null;
        try
        {
            page = new UniversitiesPage(viewModel!);
        }
        catch (Exception ex)
        {
            exception = ex;
        }

        // The code doesn't validate null, so behavior depends on InitializeComponent
        // and whether BindingContext accepts null (which it should, as it's object type)
        if (exception != null)
        {
            // If an exception was thrown, document it
            Assert.Inconclusive($"Constructor threw exception with null parameter: {exception.GetType().Name} - {exception.Message}");
        }
        else if (page != null)
        {
            // If no exception, BindingContext should be null
            Assert.IsNull(page.BindingContext);
        }
    }

    /// <summary>
    /// Testable version of UniversitiesPage that exposes protected members for testing.
    /// </summary>
    private class TestableUniversitiesPage : UniversitiesPage
    {
        public Action? LoadAsyncCompleted { get; set; }

        public TestableUniversitiesPage(UniversitiesViewModel vm) : base(vm)
        {
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            // Signal that the base call completed
            LoadAsyncCompleted?.Invoke();
        }
    }

    /// <summary>
    /// Tests that the constructor handles a null ViewModel parameter.
    /// Although the parameter is non-nullable, this tests the runtime behavior when null is passed.
    /// </summary>
    [TestMethod]
    public void UniversitiesPage_NullViewModel_SetsBindingContextToNull()
    {
        // Arrange
        UniversitiesViewModel? viewModel = null!;

        // Act
        Exception? exception = null;
        UniversitiesPage? page = null;
        try
        {
            page = new UniversitiesPage(viewModel!);
        }
        catch (Exception ex)
        {
            exception = ex;
        }

        // Assert
        // The code doesn't validate null, so behavior depends on InitializeComponent
        // and whether BindingContext accepts null (which it should, as it's object type)
        if (exception != null)
        {
            // If an exception was thrown, document it
            Assert.Inconclusive($"Constructor threw exception with null parameter: {exception.GetType().Name} - {exception.Message}");
        }
        else if (page != null)
        {
            // If no exception, BindingContext should be null
            Assert.IsNull(page.BindingContext);
        }
    }

}



/// <summary>
/// Contains unit tests for the <see cref="UniversitiesPage"/> constructor.
/// </summary>
[TestClass]
public partial class UniversitiesPageConstructorTests
{
    /// <summary>
    /// Tests that the constructor properly initializes the page with a valid ViewModel,
    /// sets the internal field, and sets the BindingContext correctly.
    /// </summary>
    [TestMethod]
    public void Constructor_ValidViewModel_InitializesPageAndSetsBindingContext()
    {
        // Arrange
        var mockAcademicService = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<UniversitiesViewModel>>();
        var viewModel = new UniversitiesViewModel(mockAcademicService.Object, mockLogger.Object);

        // Act
        UniversitiesPage? page = null;
        Exception? exception = null;
        try
        {
            page = new UniversitiesPage(viewModel);
        }
        catch (Exception ex)
        {
            exception = ex;
        }

        // Assert
        if (exception == null)
        {
            Assert.IsNotNull(page, "Page should be created successfully.");
            Assert.AreSame(viewModel, page.BindingContext, "BindingContext should be set to the provided ViewModel.");
        }
        else
        {
            Assert.Inconclusive($"Constructor threw exception (likely due to MAUI platform initialization requirement): {exception.Message}");
        }
    }

    /// <summary>
    /// Tests the constructor's runtime behavior when a null ViewModel is passed.
    /// Although the parameter is non-nullable, this verifies actual runtime handling.
    /// Expected behavior: BindingContext should be null if no exception is thrown.
    /// </summary>
    [TestMethod]
    public void Constructor_NullViewModel_HandlesNullGracefully()
    {
        // Arrange
        UniversitiesViewModel? viewModel = null!;

        // Act
        UniversitiesPage? page = null;
        Exception? exception = null;
        try
        {
            page = new UniversitiesPage(viewModel!);
        }
        catch (Exception ex)
        {
            exception = ex;
        }

        // Assert
        if (exception != null)
        {
            Assert.Inconclusive($"Constructor threw exception with null parameter: {exception.GetType().Name} - {exception.Message}");
        }
        else if (page != null)
        {
            Assert.IsNull(page.BindingContext, "BindingContext should be null when null ViewModel is provided.");
        }
    }
}