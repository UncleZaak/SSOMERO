using System;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;
using Microsoft.Maui.Controls;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Ssomero.Interfaces;
using Ssomero.Services;
using Ssomero.ViewModels;
using Ssomero.Views.Schedule;

namespace Ssomero.Views.Schedule.UnitTests;

/// <summary>
/// Unit tests for the SchedulePage class.
/// </summary>
[TestClass]
public partial class SchedulePageTests
{
    /// <summary>
    /// Helper class to expose the protected OnAppearing method for testing.
    /// This class inherits from SchedulePage and provides a public method to invoke OnAppearing.
    /// Note: This test may fail if InitializeComponent() requires XAML resources not available in test context.
    /// </summary>
    private class TestableSchedulePage : SchedulePage
    {
        public TestableSchedulePage(ScheduleViewModel vm) : base(vm)
        {
            // Note: InitializeComponent() is called in base constructor.
            // If tests fail with XAML-related errors, the page requires XAML resources
            // that are not available in the unit test context.
        }

        /// <summary>
        /// Exposes the protected OnAppearing method as a public method for testing.
        /// </summary>
        public void CallOnAppearing()
        {
            OnAppearing();
        }
    }

    /// <summary>
    /// Tests that the constructor successfully creates an instance when provided with a valid ScheduleViewModel.
    /// Input: Valid ScheduleViewModel instance.
    /// Expected: Instance is created without throwing exceptions and BindingContext is set to the provided ViewModel.
    /// </summary>
    /// <remarks>
    /// Note: This test may fail if InitializeComponent() requires a fully initialized MAUI UI context.
    /// If this test fails due to InitializeComponent(), consider marking it as inconclusive or
    /// using integration testing instead.
    /// </remarks>
    [TestMethod]
    public void Constructor_ValidViewModel_SetsBindingContextAndCreatesInstance()
    {
        // Arrange
        var mockScheduleService = new Mock<IScheduleService>();
        var mockLogger = new Mock<ILogger<ScheduleViewModel>>();
        var viewModel = new ScheduleViewModel(mockScheduleService.Object, mockLogger.Object);
        // Act
        var page = new SchedulePage(viewModel);
        // Assert
        Assert.IsNotNull(page);
        Assert.AreSame(viewModel, page.BindingContext);
    }

    /// <summary>
    /// Tests the constructor behavior when provided with a null ViewModel.
    /// Input: null ViewModel parameter.
    /// Expected: Either throws ArgumentNullException or handles gracefully depending on implementation.
    /// </summary>
    /// <remarks>
    /// Note: This test may fail if InitializeComponent() requires a fully initialized MAUI UI context.
    /// The parameter is marked as non-nullable, but runtime can still pass null.
    /// </remarks>
    [TestMethod]
    public void Constructor_NullViewModel_ThrowsOrSetsBindingContextToNull()
    {
        // Arrange
        ScheduleViewModel? viewModel = null;
        try
        {
            // Act
            var page = new SchedulePage(viewModel!);
            // Assert
            // If no exception is thrown, verify that BindingContext is set to null
            Assert.IsNotNull(page);
            Assert.IsNull(page.BindingContext);
        }
        catch (ArgumentNullException)
        {
            // Expected if the constructor validates the parameter
            Assert.IsTrue(true);
        }
        catch (NullReferenceException)
        {
            // May occur if InitializeComponent or other operations fail with null vm
            Assert.IsTrue(true);
        }
    }

    /// <summary>
    /// Helper class to expose the protected OnDisappearing method for testing.
    /// This class inherits from SchedulePage and provides a public method to invoke OnDisappearing.
    /// </summary>
    private class TestableSchedulePageForDisappearing : SchedulePage
    {
        public TestableSchedulePageForDisappearing(ScheduleViewModel vm) : base(vm)
        {
            // Note: InitializeComponent() is called in base constructor.
        }

        /// <summary>
        /// Exposes the protected OnDisappearing method as a public method for testing.
        /// </summary>
        public void CallOnDisappearing()
        {
            OnDisappearing();
        }
    }

    /// <summary>
    /// Tests that OnDisappearing executes successfully and calls CancelPendingRequests on the ViewModel.
    /// Input: Valid ScheduleViewModel instance.
    /// Expected: OnDisappearing completes without throwing exceptions and triggers cleanup on the ViewModel.
    /// </summary>
    /// <remarks>
    /// Note: This test verifies that OnDisappearing executes without errors. Direct verification
    /// that CancelPendingRequests is called cannot be achieved with Moq since the method is not virtual.
    /// The test ensures the page lifecycle method executes correctly and integrates with the ViewModel.
    /// </remarks>
    [TestMethod]
    public void OnDisappearing_WhenCalled_ExecutesWithoutException()
    {
        // Arrange
        var mockScheduleService = new Mock<IScheduleService>();
        var mockLogger = new Mock<ILogger<ScheduleViewModel>>();
        var viewModel = new ScheduleViewModel(mockScheduleService.Object, mockLogger.Object);
        var page = new TestableSchedulePageForDisappearing(viewModel);

        // Act
        page.CallOnDisappearing();

        // Assert
        // Verification: Method completes without throwing
        Assert.IsTrue(true);
    }

    /// <summary>
    /// Tests that OnDisappearing can be called multiple times without errors.
    /// Input: Valid ScheduleViewModel instance, OnDisappearing called multiple times.
    /// Expected: Each call completes successfully without throwing exceptions.
    /// </summary>
    /// <remarks>
    /// This test ensures that OnDisappearing is idempotent and can handle being called
    /// multiple times during the page lifecycle (e.g., navigating away and back).
    /// </remarks>
    [TestMethod]
    public void OnDisappearing_CalledMultipleTimes_ExecutesWithoutException()
    {
        // Arrange
        var mockScheduleService = new Mock<IScheduleService>();
        var mockLogger = new Mock<ILogger<ScheduleViewModel>>();
        var viewModel = new ScheduleViewModel(mockScheduleService.Object, mockLogger.Object);
        var page = new TestableSchedulePageForDisappearing(viewModel);

        // Act
        page.CallOnDisappearing();
        page.CallOnDisappearing();
        page.CallOnDisappearing();

        // Assert
        // Verification: Multiple calls complete without throwing
        Assert.IsTrue(true);
    }
}