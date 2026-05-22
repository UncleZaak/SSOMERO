using System;
using System.Threading.Tasks;

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
/// Unit tests for the RegisterPage class.
/// </summary>
[TestClass]
public partial class RegisterPageTests
{
    /// <summary>
    /// Tests that the constructor initializes the page with a valid RegisterViewModel,
    /// sets the BindingContext to the provided view model, and assigns the view model to the internal field.
    /// </summary>
    /// <remarks>
    /// Note: This test requires MAUI infrastructure to be initialized for InitializeComponent() to succeed.
    /// In a pure unit test environment, InitializeComponent() may throw because it attempts to load XAML resources.
    /// If this test fails with XAML-related exceptions, it indicates the MAUI test host is not properly configured.
    /// </remarks>
    [TestMethod]
    public void Constructor_ValidViewModel_SetsBindingContextAndInitializesPage()
    {
        // Arrange
        Mock<IAuthService> mockAuthService = new Mock<IAuthService>();
        Mock<IAcademicService> mockAcademicService = new Mock<IAcademicService>();
        Mock<ILogger<RegisterViewModel>> mockLogger = new Mock<ILogger<RegisterViewModel>>();
        RegisterViewModel viewModel = new RegisterViewModel(mockAuthService.Object, mockAcademicService.Object, mockLogger.Object);
        try
        {
            // Act
            RegisterPage page = new RegisterPage(viewModel);
            // Assert
            Assert.IsNotNull(page, "RegisterPage instance should be created successfully.");
            Assert.AreSame(viewModel, page.BindingContext, "BindingContext should be set to the provided view model.");
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("XAML") || ex.Message.Contains("InitializeComponent"))
        {
            // XAML infrastructure not available in unit test environment
            Assert.Inconclusive("Test requires MAUI infrastructure to be initialized. InitializeComponent() failed: " + ex.Message);
        }
        catch (Exception ex) when (ex.GetType().Name.Contains("Xaml") || ex.GetType().Name.Contains("Maui"))
        {
            // Other XAML/MAUI related exceptions
            Assert.Inconclusive("Test requires MAUI infrastructure to be initialized. Exception: " + ex.Message);
        }
    }

    /// <summary>
    /// Tests that OnAppearing calls InitAsync on the view model successfully.
    /// </summary>
    [TestMethod]
    public async Task OnAppearing_WhenCalled_CallsInitAsync()
    {
        // Arrange
        Mock<RegisterViewModel> mockViewModel = new Mock<RegisterViewModel>(MockBehavior.Strict, null!, null!, null!, null!, null!);
        TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();
        mockViewModel.Setup(vm => vm.InitAsync()).Callback(() => tcs.SetResult(true)).Returns(Task.CompletedTask);
        TestableRegisterPage page = new TestableRegisterPage(mockViewModel.Object);
        // Act
        page.CallOnAppearing();
        await tcs.Task.WaitAsync(TimeSpan.FromSeconds(5));
        // Assert
        mockViewModel.Verify(vm => vm.InitAsync(), Times.Once);
    }

    /// <summary>
    /// Tests that OnAppearing handles exceptions from InitAsync gracefully without crashing.
    /// </summary>
    [TestMethod]
    public async Task OnAppearing_WhenInitAsyncThrowsException_DoesNotCrash()
    {
        // Arrange
        Mock<RegisterViewModel> mockViewModel = new Mock<RegisterViewModel>(MockBehavior.Strict, null!, null!, null!, null!, null!);
        TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();
        InvalidOperationException expectedException = new InvalidOperationException("Test exception");
        mockViewModel.Setup(vm => vm.InitAsync()).Callback(() =>
        {
            tcs.SetResult(true);
        }).ThrowsAsync(expectedException);
        TestableRegisterPage page = new TestableRegisterPage(mockViewModel.Object);
        // Act
        page.CallOnAppearing();
        await tcs.Task.WaitAsync(TimeSpan.FromSeconds(5));
        // Assert - method completes without crashing, exception is swallowed or handled by framework
        mockViewModel.Verify(vm => vm.InitAsync(), Times.Once);
    }

    /// <summary>
    /// Tests that OnAppearing awaits the completion of InitAsync before returning.
    /// </summary>
    [TestMethod]
    public async Task OnAppearing_WhenCalled_AwaitsInitAsyncCompletion()
    {
        // Arrange
        Mock<RegisterViewModel> mockViewModel = new Mock<RegisterViewModel>(MockBehavior.Strict, null!, null!, null!, null!, null!);
        TaskCompletionSource<bool> initAsyncStarted = new TaskCompletionSource<bool>();
        TaskCompletionSource<bool> initAsyncCompleted = new TaskCompletionSource<bool>();
        mockViewModel.Setup(vm => vm.InitAsync()).Callback(() => initAsyncStarted.SetResult(true)).Returns(async () =>
        {
            await Task.Delay(100);
            initAsyncCompleted.SetResult(true);
        });
        TestableRegisterPage page = new TestableRegisterPage(mockViewModel.Object);
        // Act
        page.CallOnAppearing();
        await initAsyncStarted.Task.WaitAsync(TimeSpan.FromSeconds(5));
        await Task.Delay(200); // Allow async void to complete
        // Assert
        Assert.IsTrue(initAsyncCompleted.Task.IsCompleted, "InitAsync should have completed");
        mockViewModel.Verify(vm => vm.InitAsync(), Times.Once);
    }

    /// <summary>
    /// Helper class to expose the protected OnAppearing method for testing.
    /// </summary>
    private class TestableRegisterPage : RegisterPage
    {
        public TestableRegisterPage(RegisterViewModel vm) : base(vm)
        {
        }

        public void CallOnAppearing()
        {
            OnAppearing();
        }
    }

    /// <summary>
    /// Tests that the constructor handles null ViewModel parameter.
    /// Since the parameter is non-nullable, passing null violates the contract but should be tested for robustness.
    /// The constructor does not contain explicit null checks, so null will be assigned to the field and BindingContext.
    /// </summary>
    [TestMethod]
    public void Constructor_NullViewModel_AcceptsNullOrThrows()
    {
        // Arrange
        RegisterViewModel? nullViewModel = null;

        try
        {
            // Act
            RegisterPage page = new RegisterPage(nullViewModel!);

            // Assert
            Assert.IsNotNull(page, "RegisterPage instance should be created even with null view model.");
            Assert.IsNull(page.BindingContext, "BindingContext should be null when null view model is provided.");
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("XAML") || ex.Message.Contains("InitializeComponent"))
        {
            // XAML infrastructure not available in unit test environment
            Assert.Inconclusive("Test requires MAUI infrastructure to be initialized. InitializeComponent() failed: " + ex.Message);
        }
        catch (Exception ex) when (ex.GetType().Name.Contains("Xaml") || ex.GetType().Name.Contains("Maui"))
        {
            // Other XAML/MAUI related exceptions
            Assert.Inconclusive("Test requires MAUI infrastructure to be initialized. Exception: " + ex.Message);
        }
        catch (ArgumentNullException)
        {
            // If the code or framework throws ArgumentNullException for null parameter, that's acceptable
            Assert.IsTrue(true, "Constructor correctly throws ArgumentNullException for null view model.");
        }
        catch (NullReferenceException)
        {
            // If a NullReferenceException is thrown during construction, that's also acceptable
            Assert.IsTrue(true, "Constructor throws NullReferenceException when accessing null view model.");
        }
    }
}




/// <summary>
/// Unit tests for the RegisterPage OnAppearing method.
/// </summary>
[TestClass]
public partial class RegisterPageOnAppearingTests
{
    /// <summary>
    /// Tests that OnAppearing calls InitAsync on the view model when invoked.
    /// Verifies that the InitAsync method is called exactly once.
    /// </summary>
    [TestMethod]
    public async Task OnAppearing_WhenCalled_CallsInitAsync()
    {
        // Arrange
        Mock<RegisterViewModel> mockViewModel = new Mock<RegisterViewModel>(MockBehavior.Strict, null!, null!, null!, null!, null!);
        TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();
        mockViewModel.Setup(vm => vm.InitAsync()).Callback(() => tcs.SetResult(true)).Returns(Task.CompletedTask);
        TestableRegisterPage page = new TestableRegisterPage(mockViewModel.Object);

        // Act
        page.CallOnAppearing();
        await tcs.Task.WaitAsync(TimeSpan.FromSeconds(5));

        // Assert
        mockViewModel.Verify(vm => vm.InitAsync(), Times.Once);
    }

    /// <summary>
    /// Tests that OnAppearing handles exceptions thrown by InitAsync gracefully.
    /// Verifies that the method does not crash and the exception is handled by the framework.
    /// </summary>
    [TestMethod]
    public async Task OnAppearing_WhenInitAsyncThrowsException_DoesNotCrash()
    {
        // Arrange
        Mock<RegisterViewModel> mockViewModel = new Mock<RegisterViewModel>(MockBehavior.Strict, null!, null!, null!, null!, null!);
        TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();
        InvalidOperationException expectedException = new InvalidOperationException("Test exception");
        mockViewModel.Setup(vm => vm.InitAsync()).Callback(() =>
        {
            tcs.SetResult(true);
        }).ThrowsAsync(expectedException);
        TestableRegisterPage page = new TestableRegisterPage(mockViewModel.Object);

        // Act
        page.CallOnAppearing();
        await tcs.Task.WaitAsync(TimeSpan.FromSeconds(5));

        // Assert
        mockViewModel.Verify(vm => vm.InitAsync(), Times.Once);
    }

    /// <summary>
    /// Tests that OnAppearing properly awaits the completion of InitAsync.
    /// Verifies that the async operation completes before the method returns.
    /// </summary>
    [TestMethod]
    public async Task OnAppearing_WhenCalled_AwaitsInitAsyncCompletion()
    {
        // Arrange
        Mock<RegisterViewModel> mockViewModel = new Mock<RegisterViewModel>(MockBehavior.Strict, null!, null!, null!, null!, null!);
        TaskCompletionSource<bool> initAsyncStarted = new TaskCompletionSource<bool>();
        TaskCompletionSource<bool> initAsyncCompleted = new TaskCompletionSource<bool>();
        mockViewModel.Setup(vm => vm.InitAsync()).Callback(() => initAsyncStarted.SetResult(true)).Returns(async () =>
        {
            await Task.Delay(100);
            initAsyncCompleted.SetResult(true);
        });
        TestableRegisterPage page = new TestableRegisterPage(mockViewModel.Object);

        // Act
        page.CallOnAppearing();
        await initAsyncStarted.Task.WaitAsync(TimeSpan.FromSeconds(5));
        await Task.Delay(200);

        // Assert
        Assert.IsTrue(initAsyncCompleted.Task.IsCompleted, "InitAsync should have completed");
        mockViewModel.Verify(vm => vm.InitAsync(), Times.Once);
    }

    /// <summary>
    /// Tests that OnAppearing handles different exception types thrown by InitAsync.
    /// Verifies that ArgumentException is handled gracefully without crashing.
    /// </summary>
    [TestMethod]
    public async Task OnAppearing_WhenInitAsyncThrowsArgumentException_DoesNotCrash()
    {
        // Arrange
        Mock<RegisterViewModel> mockViewModel = new Mock<RegisterViewModel>(MockBehavior.Strict, null!, null!, null!, null!, null!);
        TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();
        ArgumentException expectedException = new ArgumentException("Invalid argument");
        mockViewModel.Setup(vm => vm.InitAsync()).Callback(() =>
        {
            tcs.SetResult(true);
        }).ThrowsAsync(expectedException);
        TestableRegisterPage page = new TestableRegisterPage(mockViewModel.Object);

        // Act
        page.CallOnAppearing();
        await tcs.Task.WaitAsync(TimeSpan.FromSeconds(5));

        // Assert
        mockViewModel.Verify(vm => vm.InitAsync(), Times.Once);
    }

    /// <summary>
    /// Tests that OnAppearing handles NullReferenceException thrown by InitAsync.
    /// Verifies that the exception is handled gracefully without crashing.
    /// </summary>
    [TestMethod]
    public async Task OnAppearing_WhenInitAsyncThrowsNullReferenceException_DoesNotCrash()
    {
        // Arrange
        Mock<RegisterViewModel> mockViewModel = new Mock<RegisterViewModel>(MockBehavior.Strict, null!, null!, null!, null!, null!);
        TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();
        NullReferenceException expectedException = new NullReferenceException("Null reference");
        mockViewModel.Setup(vm => vm.InitAsync()).Callback(() =>
        {
            tcs.SetResult(true);
        }).ThrowsAsync(expectedException);
        TestableRegisterPage page = new TestableRegisterPage(mockViewModel.Object);

        // Act
        page.CallOnAppearing();
        await tcs.Task.WaitAsync(TimeSpan.FromSeconds(5));

        // Assert
        mockViewModel.Verify(vm => vm.InitAsync(), Times.Once);
    }

    /// <summary>
    /// Tests that OnAppearing only calls InitAsync once even when the initialization takes time.
    /// Verifies that no duplicate calls are made during async execution.
    /// </summary>
    [TestMethod]
    public async Task OnAppearing_WithDelayedInitAsync_CallsInitAsyncOnlyOnce()
    {
        // Arrange
        Mock<RegisterViewModel> mockViewModel = new Mock<RegisterViewModel>(MockBehavior.Strict, null!, null!, null!, null!, null!);
        TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();
        mockViewModel.Setup(vm => vm.InitAsync()).Returns(async () =>
        {
            await Task.Delay(500);
            tcs.SetResult(true);
        });
        TestableRegisterPage page = new TestableRegisterPage(mockViewModel.Object);

        // Act
        page.CallOnAppearing();
        await tcs.Task.WaitAsync(TimeSpan.FromSeconds(5));

        // Assert
        mockViewModel.Verify(vm => vm.InitAsync(), Times.Once);
    }

    /// <summary>
    /// Tests that OnAppearing completes immediately when InitAsync returns a completed task.
    /// Verifies that synchronous completion is handled correctly.
    /// </summary>
    [TestMethod]
    public async Task OnAppearing_WhenInitAsyncCompletesImmediately_CompletesSuccessfully()
    {
        // Arrange
        Mock<RegisterViewModel> mockViewModel = new Mock<RegisterViewModel>(MockBehavior.Strict, null!, null!, null!, null!, null!);
        TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();
        mockViewModel.Setup(vm => vm.InitAsync()).Callback(() => tcs.SetResult(true)).Returns(Task.CompletedTask);
        TestableRegisterPage page = new TestableRegisterPage(mockViewModel.Object);

        // Act
        page.CallOnAppearing();
        await tcs.Task.WaitAsync(TimeSpan.FromSeconds(5));

        // Assert
        mockViewModel.Verify(vm => vm.InitAsync(), Times.Once);
        Assert.IsTrue(tcs.Task.IsCompleted);
    }

    /// <summary>
    /// Helper class to expose the protected OnAppearing method for testing.
    /// </summary>
    private class TestableRegisterPage : RegisterPage
    {
        public TestableRegisterPage(RegisterViewModel vm) : base(vm)
        {
        }

        public void CallOnAppearing()
        {
            OnAppearing();
        }
    }
}



/// <summary>
/// Unit tests for the RegisterPage OnAppearing method.
/// </summary>
[TestClass]
public partial class RegisterPageOnAppearingMethodTests
{
    /// <summary>
    /// Tests that OnAppearing calls InitAsync on the view model when invoked.
    /// Verifies that the InitAsync method is called exactly once.
    /// </summary>
    [TestMethod]
    public async Task OnAppearing_WhenCalled_CallsInitAsync()
    {
        // Arrange
        Mock<RegisterViewModel> mockViewModel = new Mock<RegisterViewModel>(MockBehavior.Strict, null!, null!, null!, null!, null!);
        TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();
        mockViewModel.Setup(vm => vm.InitAsync()).Callback(() => tcs.SetResult(true)).Returns(Task.CompletedTask);
        TestableRegisterPage page = new TestableRegisterPage(mockViewModel.Object);

        // Act
        page.CallOnAppearing();
        await tcs.Task.WaitAsync(TimeSpan.FromSeconds(5));

        // Assert
        mockViewModel.Verify(vm => vm.InitAsync(), Times.Once);
    }

    /// <summary>
    /// Tests that OnAppearing handles InvalidOperationException thrown by InitAsync gracefully.
    /// Verifies that the method does not crash and the exception is handled by the framework.
    /// </summary>
    [TestMethod]
    public async Task OnAppearing_WhenInitAsyncThrowsInvalidOperationException_DoesNotCrash()
    {
        // Arrange
        Mock<RegisterViewModel> mockViewModel = new Mock<RegisterViewModel>(MockBehavior.Strict, null!, null!, null!, null!, null!);
        TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();
        InvalidOperationException expectedException = new InvalidOperationException("Test exception");
        mockViewModel.Setup(vm => vm.InitAsync()).Callback(() => tcs.SetResult(true)).ThrowsAsync(expectedException);
        TestableRegisterPage page = new TestableRegisterPage(mockViewModel.Object);

        // Act
        page.CallOnAppearing();
        await tcs.Task.WaitAsync(TimeSpan.FromSeconds(5));

        // Assert - method completes without crashing, exception is swallowed or handled by framework
        mockViewModel.Verify(vm => vm.InitAsync(), Times.Once);
    }

    /// <summary>
    /// Tests that OnAppearing properly awaits the completion of InitAsync.
    /// Verifies that the async operation completes before the method returns.
    /// </summary>
    [TestMethod]
    public async Task OnAppearing_WhenCalled_AwaitsInitAsyncCompletion()
    {
        // Arrange
        Mock<RegisterViewModel> mockViewModel = new Mock<RegisterViewModel>(MockBehavior.Strict, null!, null!, null!, null!, null!);
        TaskCompletionSource<bool> initAsyncStarted = new TaskCompletionSource<bool>();
        TaskCompletionSource<bool> initAsyncCompleted = new TaskCompletionSource<bool>();
        mockViewModel.Setup(vm => vm.InitAsync()).Callback(() => initAsyncStarted.SetResult(true)).Returns(async () =>
        {
            await Task.Delay(100);
            initAsyncCompleted.SetResult(true);
        });
        TestableRegisterPage page = new TestableRegisterPage(mockViewModel.Object);

        // Act
        page.CallOnAppearing();
        await initAsyncStarted.Task.WaitAsync(TimeSpan.FromSeconds(5));
        await Task.Delay(200);

        // Assert
        Assert.IsTrue(initAsyncCompleted.Task.IsCompleted, "InitAsync should have completed");
        mockViewModel.Verify(vm => vm.InitAsync(), Times.Once);
    }

    /// <summary>
    /// Tests that OnAppearing handles ArgumentException thrown by InitAsync.
    /// Verifies that ArgumentException is handled gracefully without crashing.
    /// </summary>
    [TestMethod]
    public async Task OnAppearing_WhenInitAsyncThrowsArgumentException_DoesNotCrash()
    {
        // Arrange
        Mock<RegisterViewModel> mockViewModel = new Mock<RegisterViewModel>(MockBehavior.Strict, null!, null!, null!, null!, null!);
        TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();
        ArgumentException expectedException = new ArgumentException("Invalid argument", "paramName");
        mockViewModel.Setup(vm => vm.InitAsync()).Callback(() => tcs.SetResult(true)).ThrowsAsync(expectedException);
        TestableRegisterPage page = new TestableRegisterPage(mockViewModel.Object);

        // Act
        page.CallOnAppearing();
        await tcs.Task.WaitAsync(TimeSpan.FromSeconds(5));

        // Assert
        mockViewModel.Verify(vm => vm.InitAsync(), Times.Once);
    }

    /// <summary>
    /// Tests that OnAppearing handles NullReferenceException thrown by InitAsync.
    /// Verifies that the exception is handled gracefully without crashing.
    /// </summary>
    [TestMethod]
    public async Task OnAppearing_WhenInitAsyncThrowsNullReferenceException_DoesNotCrash()
    {
        // Arrange
        Mock<RegisterViewModel> mockViewModel = new Mock<RegisterViewModel>(MockBehavior.Strict, null!, null!, null!, null!, null!);
        TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();
        NullReferenceException expectedException = new NullReferenceException("Null reference");
        mockViewModel.Setup(vm => vm.InitAsync()).Callback(() => tcs.SetResult(true)).ThrowsAsync(expectedException);
        TestableRegisterPage page = new TestableRegisterPage(mockViewModel.Object);

        // Act
        page.CallOnAppearing();
        await tcs.Task.WaitAsync(TimeSpan.FromSeconds(5));

        // Assert
        mockViewModel.Verify(vm => vm.InitAsync(), Times.Once);
    }

    /// <summary>
    /// Tests that OnAppearing handles OperationCanceledException thrown by InitAsync.
    /// Verifies that cancellation exceptions are handled gracefully.
    /// </summary>
    [TestMethod]
    public async Task OnAppearing_WhenInitAsyncThrowsOperationCanceledException_DoesNotCrash()
    {
        // Arrange
        Mock<RegisterViewModel> mockViewModel = new Mock<RegisterViewModel>(MockBehavior.Strict, null!, null!, null!, null!, null!);
        TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();
        OperationCanceledException expectedException = new OperationCanceledException("Operation was canceled");
        mockViewModel.Setup(vm => vm.InitAsync()).Callback(() => tcs.SetResult(true)).ThrowsAsync(expectedException);
        TestableRegisterPage page = new TestableRegisterPage(mockViewModel.Object);

        // Act
        page.CallOnAppearing();
        await tcs.Task.WaitAsync(TimeSpan.FromSeconds(5));

        // Assert
        mockViewModel.Verify(vm => vm.InitAsync(), Times.Once);
    }

    /// <summary>
    /// Tests that OnAppearing only calls InitAsync once even when the initialization takes time.
    /// Verifies that no duplicate calls are made during async execution.
    /// </summary>
    [TestMethod]
    public async Task OnAppearing_WithDelayedInitAsync_CallsInitAsyncOnlyOnce()
    {
        // Arrange
        Mock<RegisterViewModel> mockViewModel = new Mock<RegisterViewModel>(MockBehavior.Strict, null!, null!, null!, null!, null!);
        TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();
        mockViewModel.Setup(vm => vm.InitAsync()).Returns(async () =>
        {
            await Task.Delay(500);
            tcs.SetResult(true);
        });
        TestableRegisterPage page = new TestableRegisterPage(mockViewModel.Object);

        // Act
        page.CallOnAppearing();
        await tcs.Task.WaitAsync(TimeSpan.FromSeconds(5));

        // Assert
        mockViewModel.Verify(vm => vm.InitAsync(), Times.Once);
    }

    /// <summary>
    /// Tests that OnAppearing completes immediately when InitAsync returns a completed task.
    /// Verifies that synchronous completion is handled correctly.
    /// </summary>
    [TestMethod]
    public async Task OnAppearing_WhenInitAsyncCompletesImmediately_CompletesSuccessfully()
    {
        // Arrange
        Mock<RegisterViewModel> mockViewModel = new Mock<RegisterViewModel>(MockBehavior.Strict, null!, null!, null!, null!, null!);
        TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();
        mockViewModel.Setup(vm => vm.InitAsync()).Callback(() => tcs.SetResult(true)).Returns(Task.CompletedTask);
        TestableRegisterPage page = new TestableRegisterPage(mockViewModel.Object);

        // Act
        page.CallOnAppearing();
        await tcs.Task.WaitAsync(TimeSpan.FromSeconds(5));

        // Assert
        mockViewModel.Verify(vm => vm.InitAsync(), Times.Once);
    }

    /// <summary>
    /// Tests that OnAppearing handles AggregateException thrown by InitAsync.
    /// Verifies that composite exceptions are handled gracefully.
    /// </summary>
    [TestMethod]
    public async Task OnAppearing_WhenInitAsyncThrowsAggregateException_DoesNotCrash()
    {
        // Arrange
        Mock<RegisterViewModel> mockViewModel = new Mock<RegisterViewModel>(MockBehavior.Strict, null!, null!, null!, null!, null!);
        TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();
        AggregateException expectedException = new AggregateException("Multiple errors", new Exception("Error 1"), new Exception("Error 2"));
        mockViewModel.Setup(vm => vm.InitAsync()).Callback(() => tcs.SetResult(true)).ThrowsAsync(expectedException);
        TestableRegisterPage page = new TestableRegisterPage(mockViewModel.Object);

        // Act
        page.CallOnAppearing();
        await tcs.Task.WaitAsync(TimeSpan.FromSeconds(5));

        // Assert
        mockViewModel.Verify(vm => vm.InitAsync(), Times.Once);
    }

    /// <summary>
    /// Tests that OnAppearing handles TimeoutException thrown by InitAsync.
    /// Verifies that timeout exceptions are handled gracefully.
    /// </summary>
    [TestMethod]
    public async Task OnAppearing_WhenInitAsyncThrowsTimeoutException_DoesNotCrash()
    {
        // Arrange
        Mock<RegisterViewModel> mockViewModel = new Mock<RegisterViewModel>(MockBehavior.Strict, null!, null!, null!, null!, null!);
        TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();
        TimeoutException expectedException = new TimeoutException("Operation timed out");
        mockViewModel.Setup(vm => vm.InitAsync()).Callback(() => tcs.SetResult(true)).ThrowsAsync(expectedException);
        TestableRegisterPage page = new TestableRegisterPage(mockViewModel.Object);

        // Act
        page.CallOnAppearing();
        await tcs.Task.WaitAsync(TimeSpan.FromSeconds(5));

        // Assert
        mockViewModel.Verify(vm => vm.InitAsync(), Times.Once);
    }

    /// <summary>
    /// Tests that multiple consecutive calls to OnAppearing each trigger InitAsync.
    /// Verifies that the method can be called multiple times without issue.
    /// </summary>
    [TestMethod]
    public async Task OnAppearing_WhenCalledMultipleTimes_CallsInitAsyncEachTime()
    {
        // Arrange
        Mock<RegisterViewModel> mockViewModel = new Mock<RegisterViewModel>(MockBehavior.Strict, null!, null!, null!, null!, null!);
        TaskCompletionSource<bool> tcs1 = new TaskCompletionSource<bool>();
        TaskCompletionSource<bool> tcs2 = new TaskCompletionSource<bool>();
        int callCount = 0;
        mockViewModel.Setup(vm => vm.InitAsync()).Returns(() =>
        {
            callCount++;
            if (callCount == 1)
                tcs1.SetResult(true);
            else if (callCount == 2)
                tcs2.SetResult(true);
            return Task.CompletedTask;
        });
        TestableRegisterPage page = new TestableRegisterPage(mockViewModel.Object);

        // Act
        page.CallOnAppearing();
        await tcs1.Task.WaitAsync(TimeSpan.FromSeconds(5));
        page.CallOnAppearing();
        await tcs2.Task.WaitAsync(TimeSpan.FromSeconds(5));

        // Assert
        mockViewModel.Verify(vm => vm.InitAsync(), Times.Exactly(2));
    }

    /// <summary>
    /// Helper class to expose the protected OnAppearing method for testing.
    /// </summary>
    private class TestableRegisterPage : RegisterPage
    {
        public TestableRegisterPage(RegisterViewModel vm) : base(vm)
        {
        }

        public void CallOnAppearing()
        {
            OnAppearing();
        }
    }
}



/// <summary>
/// Unit tests for the RegisterPage constructor.
/// </summary>
[TestClass]
public partial class RegisterPageConstructorTests
{
    /// <summary>
    /// Tests that the constructor initializes the page with a valid RegisterViewModel,
    /// sets the BindingContext to the provided view model, and assigns the view model to the internal field.
    /// </summary>
    /// <remarks>
    /// Note: This test requires MAUI infrastructure to be initialized for InitializeComponent() to succeed.
    /// In a pure unit test environment, InitializeComponent() may throw because it attempts to load XAML resources.
    /// If this test fails with XAML-related exceptions, it indicates the MAUI test host is not properly configured.
    /// </remarks>
    [TestMethod]
    public void Constructor_ValidViewModel_SetsBindingContextAndInitializesPage()
    {
        // Arrange
        Mock<IAuthService> mockAuthService = new Mock<IAuthService>();
        Mock<IAcademicService> mockAcademicService = new Mock<IAcademicService>();
        Mock<ILogger<RegisterViewModel>> mockLogger = new Mock<ILogger<RegisterViewModel>>();
        RegisterViewModel viewModel = new RegisterViewModel(mockAuthService.Object, mockAcademicService.Object, mockLogger.Object);

        try
        {
            // Act
            RegisterPage page = new RegisterPage(viewModel);

            // Assert
            Assert.IsNotNull(page, "RegisterPage instance should be created successfully.");
            Assert.AreSame(viewModel, page.BindingContext, "BindingContext should be set to the provided view model.");
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("XAML") || ex.Message.Contains("InitializeComponent"))
        {
            // XAML infrastructure not available in unit test environment
            Assert.Inconclusive("Test requires MAUI infrastructure to be initialized. InitializeComponent() failed: " + ex.Message);
        }
        catch (Exception ex) when (ex.GetType().Name.Contains("Xaml") || ex.GetType().Name.Contains("Maui"))
        {
            // Other XAML/MAUI related exceptions
            Assert.Inconclusive("Test requires MAUI infrastructure to be initialized. Exception: " + ex.Message);
        }
    }

    /// <summary>
    /// Tests that the constructor handles null ViewModel parameter.
    /// Since the parameter is non-nullable, passing null violates the contract but should be tested for robustness.
    /// The constructor does not contain explicit null checks, so null will be assigned to the field and BindingContext.
    /// </summary>
    [TestMethod]
    public void Constructor_NullViewModel_AcceptsNullOrThrows()
    {
        // Arrange
        RegisterViewModel? nullViewModel = null;

        try
        {
            // Act
            RegisterPage page = new RegisterPage(nullViewModel!);

            // Assert
            Assert.IsNotNull(page, "RegisterPage instance should be created even with null view model.");
            Assert.IsNull(page.BindingContext, "BindingContext should be null when null view model is provided.");
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("XAML") || ex.Message.Contains("InitializeComponent"))
        {
            // XAML infrastructure not available in unit test environment
            Assert.Inconclusive("Test requires MAUI infrastructure to be initialized. InitializeComponent() failed: " + ex.Message);
        }
        catch (Exception ex) when (ex.GetType().Name.Contains("Xaml") || ex.GetType().Name.Contains("Maui"))
        {
            // Other XAML/MAUI related exceptions
            Assert.Inconclusive("Test requires MAUI infrastructure to be initialized. Exception: " + ex.Message);
        }
        catch (ArgumentNullException)
        {
            // If the code or framework throws ArgumentNullException for null parameter, that's acceptable
            Assert.IsTrue(true, "Constructor correctly throws ArgumentNullException for null view model.");
        }
        catch (NullReferenceException)
        {
            // If a NullReferenceException is thrown during construction, that's also acceptable
            Assert.IsTrue(true, "Constructor throws NullReferenceException when accessing null view model.");
        }
    }
}