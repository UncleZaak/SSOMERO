using System;
using System.Threading;
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
/// Unit tests for the <see cref = "DashboardPage"/> class.
/// </summary>
[TestClass]
public partial class DashboardPageTests
{
    /// <summary>
    /// Tests that the constructor successfully initializes the page with a valid view model
    /// and sets the BindingContext property to the provided view model.
    /// Input: Valid DashboardViewModel instance
    /// Expected: Page is created and BindingContext equals the provided view model
    /// </summary>
    [TestMethod]
    public void Constructor_WithValidViewModel_InitializesPageAndSetsBindingContext()
    {
        // Arrange
        var mockDashboardService = new Mock<IDashboardService>();
        var mockAuthService = new Mock<IAuthService>();
        var mockSessionService = new Mock<SessionService>();
        var viewModel = new DashboardViewModel(mockDashboardService.Object, mockAuthService.Object, mockSessionService.Object);
        // Act
        var page = new DashboardPage(viewModel);
        // Assert
        Assert.IsNotNull(page);
        Assert.AreSame(viewModel, page.BindingContext);
    }

    /// <summary>
    /// Helper class that inherits from DashboardPage to expose the protected OnDisappearing method for testing.
    /// This is necessary because OnDisappearing is a protected method and cannot be called directly from test code.
    /// </summary>
    private class TestablePageHelper : DashboardPage
    {
        public TestablePageHelper(DashboardViewModel vm) : base(vm)
        {
        }

        /// <summary>
        /// Exposes the protected OnDisappearing method for testing purposes.
        /// </summary>
        public void CallOnDisappearing()
        {
            OnDisappearing();
        }

        /// <summary>
        /// Disposes resources if needed.
        /// </summary>
        public void Dispose()
        {
            // Clean up any resources if necessary
        }
    }

    /// <summary>
    /// Tests that OnAppearing calls LoadAsync on the view model.
    /// Input: Valid DashboardPage with mocked view model
    /// Expected: LoadAsync is called exactly once on the view model
    /// </summary>
    [TestMethod]
    public async Task OnAppearing_WhenCalled_CallsLoadAsyncOnViewModel()
    {
        // Arrange
        var mockDashboardService = new Mock<IDashboardService>();
        var mockAuthService = new Mock<IAuthService>();
        var mockSessionService = new Mock<SessionService>();
        var mockViewModel = new Mock<DashboardViewModel>(mockDashboardService.Object, mockAuthService.Object, mockSessionService.Object);

        var tcs = new TaskCompletionSource<bool>();
        mockViewModel.Setup(vm => vm.LoadAsync(It.IsAny<bool>())).Returns(Task.CompletedTask).Callback(() => tcs.TrySetResult(true));

        var testablePage = new TestableOnAppearingHelper(mockViewModel.Object);

        // Act
        testablePage.CallOnAppearing();

        // Wait for async operations to complete
        await Task.WhenAny(tcs.Task, Task.Delay(5000));

        // Assert
        mockViewModel.Verify(vm => vm.LoadAsync(It.IsAny<bool>()), Times.Once());
    }

    /// <summary>
    /// Tests that OnAppearing executes without throwing exceptions under normal conditions.
    /// Input: Valid DashboardPage with properly configured view model
    /// Expected: Method completes successfully without throwing exceptions
    /// </summary>
    [TestMethod]
    public async Task OnAppearing_WhenCalled_DoesNotThrowException()
    {
        // Arrange
        var mockDashboardService = new Mock<IDashboardService>();
        var mockAuthService = new Mock<IAuthService>();
        var mockSessionService = new Mock<SessionService>();
        var mockViewModel = new Mock<DashboardViewModel>(mockDashboardService.Object, mockAuthService.Object, mockSessionService.Object);

        var tcs = new TaskCompletionSource<bool>();
        mockViewModel.Setup(vm => vm.LoadAsync(It.IsAny<bool>())).Returns(Task.CompletedTask).Callback(() => tcs.TrySetResult(true));

        var testablePage = new TestableOnAppearingHelper(mockViewModel.Object);

        // Act & Assert
        try
        {
            testablePage.CallOnAppearing();
            await Task.WhenAny(tcs.Task, Task.Delay(5000));
        }
        catch (Exception ex)
        {
            Assert.Fail($"OnAppearing should not throw exceptions. Exception: {ex.Message}");
        }
    }

    /// <summary>
    /// Tests that OnAppearing calls LoadAsync with default parameter (false).
    /// Input: Valid DashboardPage with mocked view model
    /// Expected: LoadAsync is called with forceRefresh parameter set to false
    /// </summary>
    [TestMethod]
    public async Task OnAppearing_WhenCalled_CallsLoadAsyncWithDefaultParameter()
    {
        // Arrange
        var mockDashboardService = new Mock<IDashboardService>();
        var mockAuthService = new Mock<IAuthService>();
        var mockSessionService = new Mock<SessionService>();
        var mockViewModel = new Mock<DashboardViewModel>(mockDashboardService.Object, mockAuthService.Object, mockSessionService.Object);

        var tcs = new TaskCompletionSource<bool>();
        mockViewModel.Setup(vm => vm.LoadAsync(It.IsAny<bool>())).Returns(Task.CompletedTask).Callback(() => tcs.TrySetResult(true));

        var testablePage = new TestableOnAppearingHelper(mockViewModel.Object);

        // Act
        testablePage.CallOnAppearing();

        // Wait for async operations to complete
        await Task.WhenAny(tcs.Task, Task.Delay(5000));

        // Assert
        mockViewModel.Verify(vm => vm.LoadAsync(false), Times.Once());
    }

    /// <summary>
    /// Tests that OnAppearing handles exceptions from LoadAsync gracefully.
    /// Input: DashboardPage with view model that throws exception in LoadAsync
    /// Expected: OnAppearing completes without propagating the exception
    /// </summary>
    [TestMethod]
    public async Task OnAppearing_WhenLoadAsyncThrowsException_HandlesExceptionGracefully()
    {
        // Arrange
        var mockDashboardService = new Mock<IDashboardService>();
        var mockAuthService = new Mock<IAuthService>();
        var mockSessionService = new Mock<SessionService>();
        var mockViewModel = new Mock<DashboardViewModel>(mockDashboardService.Object, mockAuthService.Object, mockSessionService.Object);

        var tcs = new TaskCompletionSource<bool>();
        mockViewModel.Setup(vm => vm.LoadAsync(It.IsAny<bool>()))
            .Returns(Task.FromException(new InvalidOperationException("Test exception")))
            .Callback(() => tcs.TrySetResult(true));

        var testablePage = new TestableOnAppearingHelper(mockViewModel.Object);

        // Act & Assert - OnAppearing is async void, so exceptions are handled by synchronization context
        try
        {
            testablePage.CallOnAppearing();
            await Task.WhenAny(tcs.Task, Task.Delay(5000));
            // If we reach here, the exception was handled appropriately
        }
        catch (Exception ex)
        {
            Assert.Fail($"OnAppearing should handle LoadAsync exceptions gracefully. Exception: {ex.Message}");
        }
    }

    /// <summary>
    /// Helper class that inherits from DashboardPage to expose the protected OnAppearing method for testing.
    /// This is necessary because OnAppearing is a protected method and cannot be called directly from test code.
    /// </summary>
    private class TestableOnAppearingHelper : DashboardPage
    {
        public TestableOnAppearingHelper(DashboardViewModel vm) : base(vm)
        {
        }

        /// <summary>
        /// Exposes the protected OnAppearing method for testing purposes.
        /// </summary>
        public void CallOnAppearing()
        {
            OnAppearing();
        }
    }
}




/// <summary>
/// Unit tests for the <see cref="DashboardPage"/> constructor.
/// </summary>
[TestClass]
public partial class DashboardPageConstructorTests
{
    /// <summary>
    /// Tests that the constructor successfully initializes the page with a valid view model
    /// and sets the BindingContext property to the provided view model.
    /// Input: Valid DashboardViewModel instance
    /// Expected: Page is created, BindingContext equals the provided view model, and page is not null
    /// </summary>
    [TestMethod]
    public void Constructor_WithValidViewModel_InitializesPageAndSetsBindingContext()
    {
        // Arrange
        var mockDashboardService = new Mock<IDashboardService>();
        var mockAuthService = new Mock<IAuthService>();
        var mockSessionService = new Mock<SessionService>();
        var viewModel = new DashboardViewModel(mockDashboardService.Object, mockAuthService.Object, mockSessionService.Object);

        // Act
        var page = new DashboardPage(viewModel);

        // Assert
        Assert.IsNotNull(page);
        Assert.AreSame(viewModel, page.BindingContext);
        Assert.IsInstanceOfType(page, typeof(DashboardPage));
        Assert.IsInstanceOfType(page, typeof(ContentPage));
    }

    /// <summary>
    /// Tests that the constructor can be called multiple times with different view models.
    /// Input: Two different DashboardViewModel instances
    /// Expected: Two separate DashboardPage instances are created with correct BindingContexts
    /// </summary>
    [TestMethod]
    public void Constructor_WithMultipleInstances_CreatesIndependentPages()
    {
        // Arrange
        var mockDashboardService1 = new Mock<IDashboardService>();
        var mockAuthService1 = new Mock<IAuthService>();
        var mockSessionService1 = new Mock<SessionService>();
        var viewModel1 = new DashboardViewModel(mockDashboardService1.Object, mockAuthService1.Object, mockSessionService1.Object);

        var mockDashboardService2 = new Mock<IDashboardService>();
        var mockAuthService2 = new Mock<IAuthService>();
        var mockSessionService2 = new Mock<SessionService>();
        var viewModel2 = new DashboardViewModel(mockDashboardService2.Object, mockAuthService2.Object, mockSessionService2.Object);

        // Act
        var page1 = new DashboardPage(viewModel1);
        var page2 = new DashboardPage(viewModel2);

        // Assert
        Assert.IsNotNull(page1);
        Assert.IsNotNull(page2);
        Assert.AreNotSame(page1, page2);
        Assert.AreSame(viewModel1, page1.BindingContext);
        Assert.AreSame(viewModel2, page2.BindingContext);
    }

}



/// <summary>
/// Unit tests for the <see cref="DashboardPage.OnDisappearing"/> method.
/// </summary>
[TestClass]
public partial class DashboardPageOnDisappearingTests
{
    /// <summary>
    /// Tests that OnDisappearing calls CancelPendingRequests on the view model.
    /// Input: Valid DashboardPage with mocked view model
    /// Expected: CancelPendingRequests is called exactly once on the view model
    /// </summary>
    [TestMethod]
    public void OnDisappearing_WhenCalled_CallsCancelPendingRequestsOnViewModel()
    {
        // Arrange
        var mockDashboardService = new Mock<IDashboardService>();
        var mockAuthService = new Mock<IAuthService>();
        var mockSessionService = new Mock<SessionService>();
        var mockViewModel = new Mock<DashboardViewModel>(mockDashboardService.Object, mockAuthService.Object, mockSessionService.Object);

        mockViewModel.Setup(vm => vm.CancelPendingRequests());

        var testablePage = new TestableOnDisappearingHelper(mockViewModel.Object);

        // Act
        testablePage.CallOnDisappearing();

        // Assert
        mockViewModel.Verify(vm => vm.CancelPendingRequests(), Times.Once());
    }

    /// <summary>
    /// Tests that OnDisappearing executes without throwing exceptions under normal conditions.
    /// Input: Valid DashboardPage with properly configured view model
    /// Expected: Method completes successfully without throwing exceptions
    /// </summary>
    [TestMethod]
    public void OnDisappearing_WhenCalled_DoesNotThrowException()
    {
        // Arrange
        var mockDashboardService = new Mock<IDashboardService>();
        var mockAuthService = new Mock<IAuthService>();
        var mockSessionService = new Mock<SessionService>();
        var mockViewModel = new Mock<DashboardViewModel>(mockDashboardService.Object, mockAuthService.Object, mockSessionService.Object);

        mockViewModel.Setup(vm => vm.CancelPendingRequests());

        var testablePage = new TestableOnDisappearingHelper(mockViewModel.Object);

        // Act & Assert
        try
        {
            testablePage.CallOnDisappearing();
            Assert.IsTrue(true, "OnDisappearing completed without throwing an exception.");
        }
        catch (Exception ex)
        {
            Assert.Fail($"OnDisappearing should not throw an exception, but threw: {ex.GetType().Name} - {ex.Message}");
        }
    }

    /// <summary>
    /// Helper class that inherits from DashboardPage to expose the protected OnDisappearing method for testing.
    /// This is necessary because OnDisappearing is a protected method and cannot be called directly from test code.
    /// </summary>
    private class TestableOnDisappearingHelper : DashboardPage
    {
        public TestableOnDisappearingHelper(DashboardViewModel vm) : base(vm)
        {
        }

        /// <summary>
        /// Exposes the protected OnDisappearing method for testing purposes.
        /// </summary>
        public void CallOnDisappearing()
        {
            OnDisappearing();
        }
    }
}



/// <summary>
/// Unit tests for the <see cref="DashboardPage.OnAppearing"/> method.
/// </summary>
[TestClass]
public partial class DashboardPageOnAppearingTests
{
    /// <summary>
    /// Tests that OnAppearing sets the page opacity to 0 before starting the fade animation.
    /// Input: Valid DashboardPage instance
    /// Expected: Opacity is set to 0 immediately after OnAppearing is called
    /// </summary>
    [TestMethod]
    public async Task OnAppearing_WhenCalled_SetsOpacityToZero()
    {
        // Arrange
        var mockDashboardService = new Mock<IDashboardService>();
        var mockAuthService = new Mock<IAuthService>();
        var mockSessionService = new Mock<SessionService>();
        var mockViewModel = new Mock<DashboardViewModel>(mockDashboardService.Object, mockAuthService.Object, mockSessionService.Object);

        var tcs = new TaskCompletionSource<bool>();
        mockViewModel.Setup(vm => vm.LoadAsync(It.IsAny<bool>())).Returns(Task.CompletedTask).Callback(() => tcs.TrySetResult(true));

        var testablePage = new TestableOnAppearingHelper(mockViewModel.Object);

        // Act
        testablePage.CallOnAppearing();

        // Assert - Opacity should be set to 0 synchronously
        Assert.AreEqual(0.0, testablePage.Opacity, "Opacity should be set to 0 at the start of OnAppearing");

        // Wait for async operations to complete
        await Task.WhenAny(tcs.Task, Task.Delay(5000));
    }

    /// <summary>
    /// Tests that OnAppearing calls LoadAsync exactly once even when called multiple times rapidly.
    /// Input: Multiple rapid calls to OnAppearing
    /// Expected: LoadAsync is called once per OnAppearing call
    /// </summary>
    [TestMethod]
    public async Task OnAppearing_WhenCalledMultipleTimes_CallsLoadAsyncMultipleTimes()
    {
        // Arrange
        var mockDashboardService = new Mock<IDashboardService>();
        var mockAuthService = new Mock<IAuthService>();
        var mockSessionService = new Mock<SessionService>();
        var mockViewModel = new Mock<DashboardViewModel>(mockDashboardService.Object, mockAuthService.Object, mockSessionService.Object);

        var callCount = 0;
        var tcs = new TaskCompletionSource<bool>();
        mockViewModel.Setup(vm => vm.LoadAsync(It.IsAny<bool>()))
            .Returns(Task.CompletedTask)
            .Callback(() =>
            {
                callCount++;
                if (callCount >= 3)
                {
                    tcs.TrySetResult(true);
                }
            });

        var testablePage = new TestableOnAppearingHelper(mockViewModel.Object);

        // Act
        testablePage.CallOnAppearing();
        testablePage.CallOnAppearing();
        testablePage.CallOnAppearing();

        // Wait for async operations to complete
        await Task.WhenAny(tcs.Task, Task.Delay(5000));

        // Assert
        mockViewModel.Verify(vm => vm.LoadAsync(It.IsAny<bool>()), Times.Exactly(3));
    }

    /// <summary>
    /// Tests that OnAppearing handles exceptions from LoadAsync without propagating them.
    /// Input: DashboardPage with LoadAsync throwing various exception types
    /// Expected: OnAppearing completes without throwing, regardless of exception type
    /// </summary>
    [TestMethod]
    [DataRow(typeof(InvalidOperationException))]
    [DataRow(typeof(ArgumentException))]
    [DataRow(typeof(NullReferenceException))]
    [DataRow(typeof(TimeoutException))]
    public async Task OnAppearing_WhenLoadAsyncThrowsVariousExceptions_DoesNotPropagate(Type exceptionType)
    {
        // Arrange
        var mockDashboardService = new Mock<IDashboardService>();
        var mockAuthService = new Mock<IAuthService>();
        var mockSessionService = new Mock<SessionService>();
        var mockViewModel = new Mock<DashboardViewModel>(mockDashboardService.Object, mockAuthService.Object, mockSessionService.Object);

        var exception = (Exception)Activator.CreateInstance(exceptionType, "Test exception")!;
        var tcs = new TaskCompletionSource<bool>();
        mockViewModel.Setup(vm => vm.LoadAsync(It.IsAny<bool>()))
            .Returns(Task.FromException(exception))
            .Callback(() => tcs.TrySetResult(true));

        var testablePage = new TestableOnAppearingHelper(mockViewModel.Object);

        // Act & Assert
        try
        {
            testablePage.CallOnAppearing();
            await Task.WhenAny(tcs.Task, Task.Delay(5000));
            // If we reach here without exception, the test passes
        }
        catch (Exception ex)
        {
            Assert.Fail($"OnAppearing should not propagate {exceptionType.Name}. Exception: {ex.Message}");
        }
    }

    /// <summary>
    /// Tests that OnAppearing completes successfully when LoadAsync completes asynchronously with delay.
    /// Input: DashboardPage with LoadAsync that takes time to complete
    /// Expected: OnAppearing waits for LoadAsync to complete and verifies it was called
    /// </summary>
    [TestMethod]
    public async Task OnAppearing_WhenLoadAsyncIsDelayed_WaitsForCompletion()
    {
        // Arrange
        var mockDashboardService = new Mock<IDashboardService>();
        var mockAuthService = new Mock<IAuthService>();
        var mockSessionService = new Mock<SessionService>();
        var mockViewModel = new Mock<DashboardViewModel>(mockDashboardService.Object, mockAuthService.Object, mockSessionService.Object);

        var tcs = new TaskCompletionSource<bool>();
        mockViewModel.Setup(vm => vm.LoadAsync(It.IsAny<bool>()))
            .Returns(async () =>
            {
                await Task.Delay(100);
                tcs.TrySetResult(true);
            });

        var testablePage = new TestableOnAppearingHelper(mockViewModel.Object);

        // Act
        testablePage.CallOnAppearing();

        // Wait for async operations to complete
        var completed = await Task.WhenAny(tcs.Task, Task.Delay(5000));

        // Assert
        Assert.AreSame(tcs.Task, completed, "LoadAsync should have completed within timeout");
        mockViewModel.Verify(vm => vm.LoadAsync(false), Times.Once());
    }

    /// <summary>
    /// Tests that OnAppearing invokes LoadAsync with the correct default parameter value of false.
    /// Input: DashboardPage with mocked view model
    /// Expected: LoadAsync is called with false (not forceRefresh)
    /// </summary>
    [TestMethod]
    public async Task OnAppearing_WhenCalled_InvokesLoadAsyncWithFalseParameter()
    {
        // Arrange
        var mockDashboardService = new Mock<IDashboardService>();
        var mockAuthService = new Mock<IAuthService>();
        var mockSessionService = new Mock<SessionService>();
        var mockViewModel = new Mock<DashboardViewModel>(mockDashboardService.Object, mockAuthService.Object, mockSessionService.Object);

        var tcs = new TaskCompletionSource<bool>();
        var capturedParameter = true; // Initialize to opposite of expected
        mockViewModel.Setup(vm => vm.LoadAsync(It.IsAny<bool>()))
            .Returns(Task.CompletedTask)
            .Callback<bool>(forceRefresh =>
            {
                capturedParameter = forceRefresh;
                tcs.TrySetResult(true);
            });

        var testablePage = new TestableOnAppearingHelper(mockViewModel.Object);

        // Act
        testablePage.CallOnAppearing();

        // Wait for async operations to complete
        await Task.WhenAny(tcs.Task, Task.Delay(5000));

        // Assert
        Assert.IsFalse(capturedParameter, "LoadAsync should be called with forceRefresh parameter set to false");
        mockViewModel.Verify(vm => vm.LoadAsync(false), Times.Once());
    }

    /// <summary>
    /// Helper class that inherits from DashboardPage to expose the protected OnAppearing method for testing.
    /// This is necessary because OnAppearing is a protected method and cannot be called directly from test code.
    /// </summary>
    private class TestableOnAppearingHelper : DashboardPage
    {
        public TestableOnAppearingHelper(DashboardViewModel vm) : base(vm)
        {
        }

        /// <summary>
        /// Exposes the protected OnAppearing method for testing purposes.
        /// </summary>
        public void CallOnAppearing()
        {
            OnAppearing();
        }
    }
}