using System;
using System.Threading.Tasks;

using Microsoft.Maui.Controls;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Ssomero.Interfaces;
using Ssomero.Models;
using Ssomero.Services;
using Ssomero.ViewModels;
using Ssomero.Views.Dashboard;

namespace Ssomero.Views.Dashboard.UnitTests;


/// <summary>
/// Unit tests for the <see cref = "AdminDashboardPage"/> class.
/// </summary>
[TestClass]
public partial class AdminDashboardPageTests
{
    /// <summary>
    /// Helper class to expose the protected OnAppearing method for testing.
    /// </summary>
    private class TestableAdminDashboardPage : AdminDashboardPage
    {
        public TestableAdminDashboardPage(DashboardViewModel vm) : base(vm)
        {
        }

        public new void OnAppearing()
        {
            base.OnAppearing();
        }
    }

    /// <summary>
    /// Tests that the constructor successfully initializes the page with a valid DashboardViewModel
    /// and sets the BindingContext property correctly.
    /// </summary>
    [TestMethod]
    public void Constructor_WithValidViewModel_SetsBindingContextCorrectly()
    {
        // Arrange
        var mockDashboardService = new Mock<IDashboardService>();
        var mockAuthService = new Mock<IAuthService>();
        var mockSessionService = new Mock<SessionService>();
        var viewModel = new DashboardViewModel(mockDashboardService.Object, mockAuthService.Object, mockSessionService.Object);
        // Act
        var page = new AdminDashboardPage(viewModel);
        // Assert
        Assert.IsNotNull(page);
        Assert.AreSame(viewModel, page.BindingContext);
    }


    /// <summary>
    /// Tests that the constructor successfully creates an instance of AdminDashboardPage.
    /// Verifies that the constructor executes without throwing exceptions and that
    /// the created instance is properly initialized and correctly typed.
    /// </summary>
    [TestMethod]
    public void Constructor_WhenCalled_CreatesInstanceSuccessfully()
    {
        // Arrange
        var mockDashboardService = new Mock<IDashboardService>();
        var mockAuthService = new Mock<IAuthService>();
        var mockSessionService = new Mock<SessionService>();
        var viewModel = new DashboardViewModel(mockDashboardService.Object, mockAuthService.Object, mockSessionService.Object);

        // Act
        var page = new AdminDashboardPage(viewModel);

        // Assert
        Assert.IsNotNull(page);
        Assert.IsInstanceOfType(page, typeof(AdminDashboardPage));
        Assert.IsInstanceOfType(page, typeof(ContentPage));
    }

    /// <summary>
    /// Helper class to expose the protected OnDisappearing method for testing.
    /// </summary>
    private class TestableAdminDashboardPageForDisappearing : AdminDashboardPage
    {
        public TestableAdminDashboardPageForDisappearing(DashboardViewModel vm) : base(vm)
        {
        }

        public new void OnDisappearing()
        {
            base.OnDisappearing();
        }
    }

    /// <summary>
    /// Tests that OnDisappearing calls CancelPendingRequests on the view model.
    /// This test verifies that cleanup logic is executed when the page disappears,
    /// ensuring that pending async operations are properly cancelled.
    /// </summary>
    [TestMethod]
    public void OnDisappearing_WhenCalled_CallsCancelPendingRequestsOnViewModel()
    {
        // Arrange
        var mockDashboardService = new Mock<IDashboardService>();
        var mockAuthService = new Mock<IAuthService>();
        var mockSessionService = new Mock<SessionService>();
        var mockViewModel = new Mock<DashboardViewModel>(mockDashboardService.Object, mockAuthService.Object, mockSessionService.Object);
        var page = new TestableAdminDashboardPageForDisappearing(mockViewModel.Object);

        // Act
        page.OnDisappearing();

        // Assert
        mockViewModel.Verify(vm => vm.CancelPendingRequests(), Times.Once);
    }


    /// <summary>
    /// Tests that the constructor handles a null DashboardViewModel parameter.
    /// Verifies that the constructor completes execution and sets BindingContext to null,
    /// even though passing null violates the non-nullable parameter contract.
    /// This tests runtime behavior when nullable reference type annotations are bypassed.
    /// </summary>
    [TestMethod]
    public void Constructor_WithNullViewModel_SetsBindingContextToNull()
    {
        // Arrange
        DashboardViewModel? viewModel = null;

        // Act
        var page = new AdminDashboardPage(viewModel!);

        // Assert
        Assert.IsNotNull(page);
        Assert.IsNull(page.BindingContext);
    }

    /// <summary>
    /// Helper class to expose the protected OnAppearing method for testing.
    /// </summary>
    private class TestableAdminDashboardPageForAppearing : AdminDashboardPage
    {
        public TestableAdminDashboardPageForAppearing(DashboardViewModel vm) : base(vm)
        {
        }

        public new void OnAppearing()
        {
            base.OnAppearing();
        }
    }

    /// <summary>
    /// Tests that OnAppearing calls LoadAsync on the view model.
    /// Verifies that when the page appears, it properly initializes by calling
    /// the view model's LoadAsync method to load dashboard data.
    /// </summary>
    [TestMethod]
    public async Task OnAppearing_WhenCalled_CallsLoadAsyncOnViewModel()
    {
        // Arrange
        var mockDashboardService = new Mock<IDashboardService>();
        var mockAuthService = new Mock<IAuthService>();
        var mockSessionService = new Mock<SessionService>();
        var mockViewModel = new Mock<DashboardViewModel>(mockDashboardService.Object, mockAuthService.Object, mockSessionService.Object);

        mockViewModel.Setup(vm => vm.LoadAsync(It.IsAny<bool>())).Returns(Task.CompletedTask);

        var page = new TestableAdminDashboardPageForAppearing(mockViewModel.Object);

        // Act
        page.OnAppearing();

        // Wait for async operations to complete
        await Task.Delay(500);

        // Assert
        mockViewModel.Verify(vm => vm.LoadAsync(It.IsAny<bool>()), Times.Once);
    }

    /// <summary>
    /// Tests that OnAppearing sets the opacity to zero initially.
    /// Verifies that the page's opacity is set to 0 when the page is appearing,
    /// preparing it for the fade-in animation.
    /// </summary>
    [TestMethod]
    public void OnAppearing_WhenCalled_SetsOpacityToZero()
    {
        // Arrange
        var mockDashboardService = new Mock<IDashboardService>();
        var mockAuthService = new Mock<IAuthService>();
        var mockSessionService = new Mock<SessionService>();
        var viewModel = new DashboardViewModel(mockDashboardService.Object, mockAuthService.Object, mockSessionService.Object);

        var page = new TestableAdminDashboardPageForAppearing(viewModel);

        // Act
        page.OnAppearing();

        // Assert
        Assert.AreEqual(0, page.Opacity);
    }

    /// <summary>
    /// Tests that OnAppearing executes without throwing exceptions.
    /// Verifies that the method completes successfully under normal conditions,
    /// ensuring all async operations and animations execute properly.
    /// </summary>
    [TestMethod]
    public async Task OnAppearing_WithValidViewModel_ExecutesWithoutException()
    {
        // Arrange
        var mockDashboardService = new Mock<IDashboardService>();
        var mockAuthService = new Mock<IAuthService>();
        var mockSessionService = new Mock<SessionService>();
        var mockViewModel = new Mock<DashboardViewModel>(mockDashboardService.Object, mockAuthService.Object, mockSessionService.Object);

        mockViewModel.Setup(vm => vm.LoadAsync(It.IsAny<bool>())).Returns(Task.CompletedTask);

        var page = new TestableAdminDashboardPageForAppearing(mockViewModel.Object);

        // Act & Assert
        try
        {
            page.OnAppearing();
            await Task.Delay(500);
            Assert.IsTrue(true);
        }
        catch (Exception ex)
        {
            Assert.Fail($"OnAppearing should not throw an exception, but threw: {ex.Message}");
        }
    }

    /// <summary>
    /// Tests that OnDisappearing calls CancelPendingRequests on the view model when called multiple times.
    /// This test verifies that each call to OnDisappearing properly invokes cleanup logic,
    /// ensuring that pending async operations are cancelled every time the page disappears.
    /// </summary>
    [TestMethod]
    public void OnDisappearing_WhenCalledMultipleTimes_CallsCancelPendingRequestsEachTime()
    {
        // Arrange
        var mockDashboardService = new Mock<IDashboardService>();
        var mockAuthService = new Mock<IAuthService>();
        var mockSessionService = new Mock<SessionService>();
        var mockViewModel = new Mock<DashboardViewModel>(mockDashboardService.Object, mockAuthService.Object, mockSessionService.Object);
        var page = new TestableAdminDashboardPageForDisappearing(mockViewModel.Object);

        // Act
        page.OnDisappearing();
        page.OnDisappearing();
        page.OnDisappearing();

        // Assert
        mockViewModel.Verify(vm => vm.CancelPendingRequests(), Times.Exactly(3));
    }

    /// <summary>
    /// Tests that OnDisappearing does not throw an exception when called.
    /// This test verifies that the cleanup logic executes successfully without errors,
    /// ensuring safe navigation away from the page.
    /// </summary>
    [TestMethod]
    public void OnDisappearing_WhenCalled_DoesNotThrowException()
    {
        // Arrange
        var mockDashboardService = new Mock<IDashboardService>();
        var mockAuthService = new Mock<IAuthService>();
        var mockSessionService = new Mock<SessionService>();
        var viewModel = new DashboardViewModel(mockDashboardService.Object, mockAuthService.Object, mockSessionService.Object);
        var page = new TestableAdminDashboardPageForDisappearing(viewModel);

        // Act & Assert
        try
        {
            page.OnDisappearing();
        }
        catch (Exception ex)
        {
            Assert.Fail($"OnDisappearing should not throw an exception, but threw: {ex.GetType().Name} - {ex.Message}");
        }
    }
}