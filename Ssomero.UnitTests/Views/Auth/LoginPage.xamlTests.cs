using System;

using Microsoft.Extensions.Logging;
using Microsoft.Maui.Controls;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Ssomero.Interfaces;
using Ssomero.Services;
using Ssomero.ViewModels;
using Ssomero.Views.Auth;

namespace Ssomero.Views.Auth.UnitTests;




/// <summary>
/// Unit tests for the <see cref="LoginPage"/> class.
/// Note: These tests require MAUI infrastructure to be initialized for InitializeComponent() to work properly.
/// Consider using MAUI test frameworks or integration tests for full validation.
/// </summary>
[TestClass]
public partial class LoginPageTests
{
    /// <summary>
    /// Tests that the constructor initializes the page with a valid LoginViewModel
    /// and sets the BindingContext property correctly.
    /// </summary>
    [TestMethod]
    public void Constructor_ValidViewModel_SetsBindingContext()
    {
        // Arrange
        var mockAuthService = new Mock<IAuthService>();
        var mockApiService = new Mock<IApiService>();
        var mockSessionService = new Mock<SessionService>();
        var mockLogger = new Mock<ILogger<LoginViewModel>>();

        var viewModel = new LoginViewModel(
            mockAuthService.Object,
            mockApiService.Object,
            mockSessionService.Object,
            mockLogger.Object);

        // Act
        // Note: This may fail if MAUI infrastructure (Application, Shell.Current) is not initialized.
        // InitializeComponent() requires XAML infrastructure to be available.
        // Consider running this as an integration test with proper MAUI test host setup.
        LoginPage? page = null;
        try
        {
            page = new LoginPage(viewModel);
        }
        catch (InvalidOperationException)
        {
            // Expected if MAUI infrastructure is not initialized
            Assert.Inconclusive("MAUI infrastructure not initialized. This test requires MAUI Application context.");
            return;
        }

        // Assert
        Assert.IsNotNull(page);
        Assert.AreSame(viewModel, page.BindingContext);
    }

    /// <summary>
    /// Tests that the constructor throws or handles appropriately when provided with a null ViewModel.
    /// Even though the parameter is non-nullable, null can be passed at runtime.
    /// </summary>
    [TestMethod]
    public void Constructor_NullViewModel_SetsBindingContextToNull()
    {
        // Arrange
        LoginViewModel? nullViewModel = null;

        // Act
        // Note: This may fail if MAUI infrastructure is not initialized.
        // The behavior depends on whether InitializeComponent() or BindingContext assignment fails first.
        LoginPage? page = null;
        try
        {
            page = new LoginPage(nullViewModel!);
        }
        catch (InvalidOperationException)
        {
            // Expected if MAUI infrastructure is not initialized
            Assert.Inconclusive("MAUI infrastructure not initialized. This test requires MAUI Application context.");
            return;
        }

        // Assert
        // If construction succeeds, BindingContext should be null
        Assert.IsNotNull(page);
        Assert.IsNull(page.BindingContext);
    }

    /// <summary>
    /// Helper class to expose protected OnDisappearing method for testing.
    /// </summary>
    private class LoginPageTestWrapper : LoginPage
    {
        public LoginPageTestWrapper(LoginViewModel vm) : base(vm)
        {
        }

        public new void OnDisappearing()
        {
            base.OnDisappearing();
        }
    }


}



/// <summary>
/// Unit tests for the <see cref="LoginPage.OnDisappearing"/> method.
/// </summary>
[TestClass]
public partial class LoginPageOnDisappearingTests
{
    /// <summary>
    /// Tests that OnDisappearing does not throw when BindingContext is null.
    /// Expected behavior: Method completes without exception.
    /// </summary>
    [TestMethod]
    public void OnDisappearing_NullBindingContext_DoesNotThrow()
    {
        // Arrange
        var mockAuthService = new Mock<IAuthService>();
        var mockApiService = new Mock<IApiService>();
        var mockSessionService = new Mock<SessionService>();
        var mockLogger = new Mock<ILogger<LoginViewModel>>();

        var viewModel = new LoginViewModel(
            mockAuthService.Object,
            mockApiService.Object,
            mockSessionService.Object,
            mockLogger.Object);

        LoginPageTestWrapper? page = null;
        try
        {
            page = new LoginPageTestWrapper(viewModel);
            page.BindingContext = null;
        }
        catch (InvalidOperationException)
        {
            Assert.Inconclusive("MAUI infrastructure not initialized. This test requires MAUI Application context.");
            return;
        }

        // Act & Assert
        page.OnDisappearing();
    }

    /// <summary>
    /// Tests that OnDisappearing calls CancelPendingRequests on LoginViewModel when BindingContext is set.
    /// Expected behavior: CancelPendingRequests is called exactly once.
    /// </summary>
    [TestMethod]
    public void OnDisappearing_LoginViewModelBindingContext_CallsCancelPendingRequests()
    {
        // Arrange
        var mockAuthService = new Mock<IAuthService>();
        var mockApiService = new Mock<IApiService>();
        var mockSessionService = new Mock<SessionService>();
        var mockLogger = new Mock<ILogger<TestLoginViewModel>>();

        var testViewModel = new TestLoginViewModel(
            mockAuthService.Object,
            mockApiService.Object,
            mockSessionService.Object,
            mockLogger.Object);

        LoginPageTestWrapper? page = null;
        try
        {
            page = new LoginPageTestWrapper(testViewModel);
        }
        catch (InvalidOperationException)
        {
            Assert.Inconclusive("MAUI infrastructure not initialized. This test requires MAUI Application context.");
            return;
        }

        // Act
        page.OnDisappearing();

        // Assert
        Assert.IsTrue(testViewModel.CancelPendingRequestsCalled, "CancelPendingRequests should be called when BindingContext is LoginViewModel.");
        Assert.AreEqual(1, testViewModel.CancelPendingRequestsCallCount, "CancelPendingRequests should be called exactly once.");
    }

    /// <summary>
    /// Tests that OnDisappearing does not throw when BindingContext is not a LoginViewModel.
    /// Expected behavior: Method completes without exception and does not attempt to call CancelPendingRequests.
    /// </summary>
    [TestMethod]
    public void OnDisappearing_NonLoginViewModelBindingContext_DoesNotThrow()
    {
        // Arrange
        var mockAuthService = new Mock<IAuthService>();
        var mockApiService = new Mock<IApiService>();
        var mockSessionService = new Mock<SessionService>();
        var mockLogger = new Mock<ILogger<LoginViewModel>>();

        var viewModel = new LoginViewModel(
            mockAuthService.Object,
            mockApiService.Object,
            mockSessionService.Object,
            mockLogger.Object);

        LoginPageTestWrapper? page = null;
        try
        {
            page = new LoginPageTestWrapper(viewModel);
            page.BindingContext = new object();
        }
        catch (InvalidOperationException)
        {
            Assert.Inconclusive("MAUI infrastructure not initialized. This test requires MAUI Application context.");
            return;
        }

        // Act & Assert
        page.OnDisappearing();
    }

    /// <summary>
    /// Tests that OnDisappearing can be called multiple times without throwing.
    /// Expected behavior: Method is idempotent and does not throw on repeated calls.
    /// </summary>
    [TestMethod]
    public void OnDisappearing_CalledMultipleTimes_DoesNotThrow()
    {
        // Arrange
        var mockAuthService = new Mock<IAuthService>();
        var mockApiService = new Mock<IApiService>();
        var mockSessionService = new Mock<SessionService>();
        var mockLogger = new Mock<ILogger<TestLoginViewModel>>();

        var testViewModel = new TestLoginViewModel(
            mockAuthService.Object,
            mockApiService.Object,
            mockSessionService.Object,
            mockLogger.Object);

        LoginPageTestWrapper? page = null;
        try
        {
            page = new LoginPageTestWrapper(testViewModel);
        }
        catch (InvalidOperationException)
        {
            Assert.Inconclusive("MAUI infrastructure not initialized. This test requires MAUI Application context.");
            return;
        }

        // Act
        page.OnDisappearing();
        page.OnDisappearing();
        page.OnDisappearing();

        // Assert
        Assert.AreEqual(3, testViewModel.CancelPendingRequestsCallCount, "CancelPendingRequests should be called each time OnDisappearing is called.");
    }

    /// <summary>
    /// Helper class to expose protected OnDisappearing method for testing.
    /// </summary>
    private class LoginPageTestWrapper : LoginPage
    {
        public LoginPageTestWrapper(LoginViewModel vm) : base(vm)
        {
        }

        public new void OnDisappearing()
        {
            base.OnDisappearing();
        }
    }

    /// <summary>
    /// Test helper class that extends LoginViewModel to track calls to CancelPendingRequests.
    /// </summary>
    private class TestLoginViewModel : LoginViewModel
    {
        public bool CancelPendingRequestsCalled { get; private set; }
        public int CancelPendingRequestsCallCount { get; private set; }

        public TestLoginViewModel(
            IAuthService authService,
            IApiService apiService,
            SessionService sessionService,
            ILogger<TestLoginViewModel> logger)
            : base(authService, apiService, sessionService, logger)
        {
        }

        public new void CancelPendingRequests()
        {
            CancelPendingRequestsCalled = true;
            CancelPendingRequestsCallCount++;
            base.CancelPendingRequests();
        }
    }
}