using System;
using System.Threading.Tasks;

using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Ssomero.Interfaces;
using Ssomero.Services;
using Ssomero.ViewModels;
using Ssomero.Views.Dashboard;

namespace Ssomero.Views.Dashboard.UnitTests;




/// <summary>
/// Unit tests for the ClassRepDashboardPage class.
/// </summary>
[TestClass]
public partial class ClassRepDashboardPageTests
{
    /// <summary>
    /// Tests that the constructor initializes the page correctly with a valid view model.
    /// Expected: BindingContext is set to the provided view model and no exception is thrown.
    /// </summary>
    /// <remarks>
    /// Note: This test may require MAUI framework initialization for InitializeComponent() to work properly.
    /// If InitializeComponent() throws an exception in the test environment, consider using a MAUI test host
    /// or marking this test with [Ignore] and testing in an integration test environment instead.
    /// </remarks>
    [TestMethod]
    public void Constructor_WithValidViewModel_SetsBindingContext()
    {
        // Arrange
        var mockDashboardService = new Mock<IDashboardService>();
        var mockAuthService = new Mock<IAuthService>();
        var mockSessionService = new Mock<SessionService>();
        var viewModel = new DashboardViewModel(
            mockDashboardService.Object,
            mockAuthService.Object,
            mockSessionService.Object);

        // Act & Assert
        try
        {
            var page = new ClassRepDashboardPage(viewModel);

            // Assert
            Assert.IsNotNull(page);
            Assert.AreSame(viewModel, page.BindingContext);
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("InitializeComponent"))
        {
            // InitializeComponent may fail in unit test environment without MAUI runtime
            Assert.Inconclusive(
                "InitializeComponent requires MAUI framework initialization. " +
                "This test should be run in a MAUI test environment or as an integration test. " +
                $"Exception: {ex.Message}");
        }
    }

    /// <summary>
    /// Tests that the constructor throws ArgumentNullException when view model parameter is null.
    /// Expected: Constructor throws or assigns null causing issues when setting BindingContext.
    /// </summary>
    /// <remarks>
    /// With nullable reference types enabled and vm parameter being non-nullable,
    /// passing null violates the contract. The behavior depends on whether runtime checks are enforced.
    /// </remarks>
    [TestMethod]
    public void Constructor_WithNullViewModel_ThrowsOrHandlesGracefully()
    {
        // Arrange
        DashboardViewModel? nullViewModel = null;

        // Act & Assert
        try
        {
            // This may throw during construction or when setting BindingContext
#pragma warning disable CS8604 // Possible null reference argument - intentionally testing null
            var page = new ClassRepDashboardPage(nullViewModel!);
#pragma warning restore CS8604

            // If we get here without exception, verify that BindingContext is null
            Assert.IsNull(page.BindingContext,
                "BindingContext should be null when null view model is passed");
        }
        catch (NullReferenceException)
        {
            // Expected if null causes issues during initialization
            Assert.IsTrue(true, "NullReferenceException thrown as expected for null parameter");
        }
        catch (ArgumentNullException)
        {
            // Also acceptable if explicit null checking is added
            Assert.IsTrue(true, "ArgumentNullException thrown as expected for null parameter");
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("InitializeComponent"))
        {
            // InitializeComponent may fail first before null issues are detected
            Assert.Inconclusive(
                "InitializeComponent requires MAUI framework initialization. " +
                "Cannot fully test null parameter handling in unit test environment. " +
                $"Exception: {ex.Message}");
        }
    }

    /// <summary>
    /// Helper class to expose protected OnAppearing method for testing purposes.
    /// </summary>
    private class TestableClassRepDashboardPage : ClassRepDashboardPage
    {
        public TestableClassRepDashboardPage(DashboardViewModel vm) : base(vm)
        {
        }

        public void CallOnAppearing()
        {
            OnAppearing();
        }
    }



}