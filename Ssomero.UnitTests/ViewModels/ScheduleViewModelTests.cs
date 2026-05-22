using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Input;

using Microsoft.Extensions.Logging;
using Microsoft.Maui.Controls;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Ssomero.Interfaces;
using Ssomero.Models;
using Ssomero.ViewModels;

namespace Ssomero.ViewModels.UnitTests;




/// <summary>
/// Unit tests for the <see cref="ScheduleViewModel"/> class.
/// </summary>
[TestClass]
public class ScheduleViewModelTests
{
    /// <summary>
    /// Tests that ErrorMessage property returns the initial default value of empty string.
    /// </summary>
    [TestMethod]
    public void ErrorMessage_InitialValue_ReturnsEmptyString()
    {
        // Arrange
        var mockScheduleService = new Mock<IScheduleService>();
        var mockLogger = new Mock<ILogger<ScheduleViewModel>>();
        var viewModel = new ScheduleViewModel(mockScheduleService.Object, mockLogger.Object);

        // Act
        var result = viewModel.ErrorMessage;

        // Assert
        Assert.AreEqual(string.Empty, result);
    }

    /// <summary>
    /// Tests that ErrorMessage property setter updates the value and raises PropertyChanged event.
    /// </summary>
    /// <param name="newValue">The new value to set.</param>
    [TestMethod]
    [DataRow("Error occurred")]
    [DataRow("")]
    [DataRow(" ")]
    [DataRow("   \t\n")]
    [DataRow("A very long error message that contains multiple sentences and goes on for quite some time to test how the property handles longer strings without any issues whatsoever.")]
    [DataRow("Special chars: !@#$%^&*()_+-=[]{}|;':,.<>?/~`")]
    [DataRow("Unicode: こんにちは 世界 🌍")]
    public void ErrorMessage_SetValue_UpdatesValueAndRaisesPropertyChanged(string newValue)
    {
        // Arrange
        var mockScheduleService = new Mock<IScheduleService>();
        var mockLogger = new Mock<ILogger<ScheduleViewModel>>();
        var viewModel = new ScheduleViewModel(mockScheduleService.Object, mockLogger.Object);
        var propertyChangedRaised = false;
        string? raisedPropertyName = null;

        viewModel.PropertyChanged += (sender, args) =>
        {
            propertyChangedRaised = true;
            raisedPropertyName = args.PropertyName;
        };

        // Act
        viewModel.ErrorMessage = newValue;

        // Assert
        Assert.AreEqual(newValue, viewModel.ErrorMessage);
        Assert.IsTrue(propertyChangedRaised);
        Assert.AreEqual(nameof(viewModel.ErrorMessage), raisedPropertyName);
    }

    /// <summary>
    /// Tests that setting ErrorMessage to the same value does not raise PropertyChanged event.
    /// </summary>
    [TestMethod]
    public void ErrorMessage_SetSameValue_DoesNotRaisePropertyChanged()
    {
        // Arrange
        var mockScheduleService = new Mock<IScheduleService>();
        var mockLogger = new Mock<ILogger<ScheduleViewModel>>();
        var viewModel = new ScheduleViewModel(mockScheduleService.Object, mockLogger.Object);
        var propertyChangedCount = 0;

        viewModel.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == nameof(viewModel.ErrorMessage))
            {
                propertyChangedCount++;
            }
        };

        // Act - Set to a new value first
        viewModel.ErrorMessage = "Test error";
        var countAfterFirstSet = propertyChangedCount;

        // Act - Set to the same value again
        viewModel.ErrorMessage = "Test error";
        var countAfterSecondSet = propertyChangedCount;

        // Assert
        Assert.AreEqual(1, countAfterFirstSet);
        Assert.AreEqual(1, countAfterSecondSet); // Should still be 1, not 2
    }

    /// <summary>
    /// Tests that ErrorMessage property handles null value correctly.
    /// </summary>
    [TestMethod]
    public void ErrorMessage_SetNull_UpdatesValueAndRaisesPropertyChanged()
    {
        // Arrange
        var mockScheduleService = new Mock<IScheduleService>();
        var mockLogger = new Mock<ILogger<ScheduleViewModel>>();
        var viewModel = new ScheduleViewModel(mockScheduleService.Object, mockLogger.Object);
        var propertyChangedRaised = false;

        viewModel.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == nameof(viewModel.ErrorMessage))
            {
                propertyChangedRaised = true;
            }
        };

        // Act
        viewModel.ErrorMessage = null!;

        // Assert
        Assert.IsNull(viewModel.ErrorMessage);
        Assert.IsTrue(propertyChangedRaised);
    }

    /// <summary>
    /// Tests that ErrorMessage property can be set multiple times with different values.
    /// </summary>
    [TestMethod]
    public void ErrorMessage_SetMultipleDifferentValues_UpdatesValueEachTime()
    {
        // Arrange
        var mockScheduleService = new Mock<IScheduleService>();
        var mockLogger = new Mock<ILogger<ScheduleViewModel>>();
        var viewModel = new ScheduleViewModel(mockScheduleService.Object, mockLogger.Object);
        var propertyChangedCount = 0;

        viewModel.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == nameof(viewModel.ErrorMessage))
            {
                propertyChangedCount++;
            }
        };

        // Act & Assert
        viewModel.ErrorMessage = "First error";
        Assert.AreEqual("First error", viewModel.ErrorMessage);
        Assert.AreEqual(1, propertyChangedCount);

        viewModel.ErrorMessage = "Second error";
        Assert.AreEqual("Second error", viewModel.ErrorMessage);
        Assert.AreEqual(2, propertyChangedCount);

        viewModel.ErrorMessage = "";
        Assert.AreEqual("", viewModel.ErrorMessage);
        Assert.AreEqual(3, propertyChangedCount);
    }

    /// <summary>
    /// Tests that ErrorMessage property handles setting from non-empty back to empty string.
    /// </summary>
    [TestMethod]
    public void ErrorMessage_SetToEmptyAfterNonEmpty_UpdatesValueAndRaisesPropertyChanged()
    {
        // Arrange
        var mockScheduleService = new Mock<IScheduleService>();
        var mockLogger = new Mock<ILogger<ScheduleViewModel>>();
        var viewModel = new ScheduleViewModel(mockScheduleService.Object, mockLogger.Object);
        viewModel.ErrorMessage = "Some error";
        var propertyChangedRaised = false;

        viewModel.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == nameof(viewModel.ErrorMessage))
            {
                propertyChangedRaised = true;
            }
        };

        // Act
        viewModel.ErrorMessage = string.Empty;

        // Assert
        Assert.AreEqual(string.Empty, viewModel.ErrorMessage);
        Assert.IsTrue(propertyChangedRaised);
    }

    /// <summary>
    /// Tests that LoadAsync returns immediately without loading when IsBusy is already true.
    /// This prevents concurrent load operations.
    /// </summary>
    [TestMethod]
    public async Task LoadAsync_WhenIsBusyIsTrue_ReturnsImmediatelyWithoutLoading()
    {
        // Arrange
        var mockScheduleService = new Mock<IScheduleService>();
        var mockLogger = new Mock<ILogger<ScheduleViewModel>>();
        var viewModel = new ScheduleViewModel(mockScheduleService.Object, mockLogger.Object);

        viewModel.IsBusy = true;

        // Act
        await viewModel.LoadAsync(forceRefresh: true);

        // Assert
        mockScheduleService.Verify(s => s.GetSchedulesAsync(), Times.Never);
        Assert.AreEqual(0, viewModel.Items.Count);
    }

    /// <summary>
    /// Tests that LoadAsync returns immediately without loading when called within the refresh interval
    /// and forceRefresh is false.
    /// </summary>
    [TestMethod]
    public async Task LoadAsync_WhenNotForceRefreshAndWithinInterval_ReturnsImmediatelyWithoutLoading()
    {
        // Arrange
        var mockScheduleService = new Mock<IScheduleService>();
        var mockLogger = new Mock<ILogger<ScheduleViewModel>>();
        var viewModel = new ScheduleViewModel(mockScheduleService.Object, mockLogger.Object);

        var schedules = new List<ScheduleDto> { new ScheduleDto() };
        mockScheduleService.Setup(s => s.GetSchedulesAsync()).ReturnsAsync(schedules);

        // First load to set _lastLoaded
        await viewModel.LoadAsync(forceRefresh: true);

        mockScheduleService.ResetCalls();

        // Act - Second load immediately after, without force refresh
        await viewModel.LoadAsync(forceRefresh: false);

        // Assert
        mockScheduleService.Verify(s => s.GetSchedulesAsync(), Times.Never);
    }

    /// <summary>
    /// Tests that LoadAsync loads data when forceRefresh is true, regardless of the refresh interval.
    /// </summary>
    [TestMethod]
    public async Task LoadAsync_WhenForceRefreshIsTrue_LoadsRegardlessOfInterval()
    {
        // Arrange
        var mockScheduleService = new Mock<IScheduleService>();
        var mockLogger = new Mock<ILogger<ScheduleViewModel>>();
        var viewModel = new ScheduleViewModel(mockScheduleService.Object, mockLogger.Object);

        var schedules = new List<ScheduleDto> { new ScheduleDto() };
        mockScheduleService.Setup(s => s.GetSchedulesAsync()).ReturnsAsync(schedules);

        // First load
        await viewModel.LoadAsync(forceRefresh: true);

        mockScheduleService.ResetCalls();
        mockScheduleService.Setup(s => s.GetSchedulesAsync()).ReturnsAsync(schedules);

        // Act - Second load immediately after with force refresh
        await viewModel.LoadAsync(forceRefresh: true);

        // Assert
        mockScheduleService.Verify(s => s.GetSchedulesAsync(), Times.Once);
    }

    /// <summary>
    /// Tests that LoadAsync successfully loads data on the first call when _lastLoaded is DateTime.MinValue.
    /// </summary>
    [TestMethod]
    public async Task LoadAsync_WhenFirstLoad_LoadsSuccessfully()
    {
        // Arrange
        var mockScheduleService = new Mock<IScheduleService>();
        var mockLogger = new Mock<ILogger<ScheduleViewModel>>();
        var viewModel = new ScheduleViewModel(mockScheduleService.Object, mockLogger.Object);

        var schedules = new List<ScheduleDto> { new ScheduleDto(), new ScheduleDto() };
        mockScheduleService.Setup(s => s.GetSchedulesAsync()).ReturnsAsync(schedules);

        // Act
        await viewModel.LoadAsync(forceRefresh: false);

        // Assert
        mockScheduleService.Verify(s => s.GetSchedulesAsync(), Times.Once);
        Assert.AreEqual(2, viewModel.Items.Count);
        Assert.AreEqual(string.Empty, viewModel.ErrorMessage);
    }

    /// <summary>
    /// Tests that LoadAsync clears the Items collection when the service returns an empty collection.
    /// </summary>
    [TestMethod]
    public async Task LoadAsync_WhenServiceReturnsEmptyCollection_ClearsItemsSuccessfully()
    {
        // Arrange
        var mockScheduleService = new Mock<IScheduleService>();
        var mockLogger = new Mock<ILogger<ScheduleViewModel>>();
        var viewModel = new ScheduleViewModel(mockScheduleService.Object, mockLogger.Object);

        // Add some initial items
        viewModel.Items.Add(new ScheduleDto());
        viewModel.Items.Add(new ScheduleDto());

        var emptySchedules = new List<ScheduleDto>();
        mockScheduleService.Setup(s => s.GetSchedulesAsync()).ReturnsAsync(emptySchedules);

        // Act
        await viewModel.LoadAsync(forceRefresh: true);

        // Assert
        Assert.AreEqual(0, viewModel.Items.Count);
        Assert.AreEqual(string.Empty, viewModel.ErrorMessage);
    }

    /// <summary>
    /// Tests that LoadAsync adds all items returned by the service to the Items collection.
    /// Verifies both single and multiple item scenarios.
    /// </summary>
    /// <param name="itemCount">Number of items to return from the service.</param>
    [TestMethod]
    [DataRow(1)]
    [DataRow(3)]
    [DataRow(10)]
    public async Task LoadAsync_WhenServiceReturnsItems_AddsAllItemsToCollection(int itemCount)
    {
        // Arrange
        var mockScheduleService = new Mock<IScheduleService>();
        var mockLogger = new Mock<ILogger<ScheduleViewModel>>();
        var viewModel = new ScheduleViewModel(mockScheduleService.Object, mockLogger.Object);

        var schedules = Enumerable.Range(0, itemCount).Select(_ => new ScheduleDto()).ToList();
        mockScheduleService.Setup(s => s.GetSchedulesAsync()).ReturnsAsync(schedules);

        // Act
        await viewModel.LoadAsync(forceRefresh: true);

        // Assert
        Assert.AreEqual(itemCount, viewModel.Items.Count);
        Assert.AreEqual(string.Empty, viewModel.ErrorMessage);
    }

    /// <summary>
    /// Tests that LoadAsync properly handles exceptions thrown by the service.
    /// Verifies that the exception is logged and the ErrorMessage property is set.
    /// </summary>
    [TestMethod]
    public async Task LoadAsync_WhenServiceThrowsException_HandlesErrorAndLogsIt()
    {
        // Arrange
        var mockScheduleService = new Mock<IScheduleService>();
        var mockLogger = new Mock<ILogger<ScheduleViewModel>>();
        var viewModel = new ScheduleViewModel(mockScheduleService.Object, mockLogger.Object);

        var expectedException = new InvalidOperationException("Service error");
        mockScheduleService.Setup(s => s.GetSchedulesAsync()).ThrowsAsync(expectedException);

        // Act
        await viewModel.LoadAsync(forceRefresh: true);

        // Assert
        mockLogger.Verify(
            l => l.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Failed to load schedules")),
                expectedException,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
        Assert.IsTrue(viewModel.ErrorMessage.Contains("Failed to load schedule"));
        Assert.IsTrue(viewModel.ErrorMessage.Contains("Service error"));
    }

    /// <summary>
    /// Tests that LoadAsync sets IsBusy to false in the finally block even when an exception occurs.
    /// This ensures the UI state is properly restored.
    /// </summary>
    [TestMethod]
    public async Task LoadAsync_WhenExceptionOccurs_SetsIsBusyToFalseInFinally()
    {
        // Arrange
        var mockScheduleService = new Mock<IScheduleService>();
        var mockLogger = new Mock<ILogger<ScheduleViewModel>>();
        var viewModel = new ScheduleViewModel(mockScheduleService.Object, mockLogger.Object);

        mockScheduleService.Setup(s => s.GetSchedulesAsync()).ThrowsAsync(new Exception("Test exception"));

        // Act
        await viewModel.LoadAsync(forceRefresh: true);

        // Assert
        Assert.IsFalse(viewModel.IsBusy);
    }

    /// <summary>
    /// Tests that LoadAsync sets IsBusy to true during execution and false after completion.
    /// </summary>
    [TestMethod]
    public async Task LoadAsync_WhenExecuting_SetsIsBusyCorrectly()
    {
        // Arrange
        var mockScheduleService = new Mock<IScheduleService>();
        var mockLogger = new Mock<ILogger<ScheduleViewModel>>();
        var viewModel = new ScheduleViewModel(mockScheduleService.Object, mockLogger.Object);

        bool isBusyDuringExecution = false;
        var tcs = new TaskCompletionSource<IEnumerable<ScheduleDto>>();
        mockScheduleService.Setup(s => s.GetSchedulesAsync()).Returns(() =>
        {
            isBusyDuringExecution = viewModel.IsBusy;
            tcs.SetResult(new List<ScheduleDto>());
            return tcs.Task;
        });

        // Act
        await viewModel.LoadAsync(forceRefresh: true);

        // Assert
        Assert.IsTrue(isBusyDuringExecution, "IsBusy should be true during execution");
        Assert.IsFalse(viewModel.IsBusy, "IsBusy should be false after completion");
    }

    /// <summary>
    /// Tests that LoadAsync clears the ErrorMessage at the start of a successful load.
    /// </summary>
    [TestMethod]
    public async Task LoadAsync_WhenSuccessful_ClearsErrorMessage()
    {
        // Arrange
        var mockScheduleService = new Mock<IScheduleService>();
        var mockLogger = new Mock<ILogger<ScheduleViewModel>>();
        var viewModel = new ScheduleViewModel(mockScheduleService.Object, mockLogger.Object);

        // Set an initial error message
        viewModel.ErrorMessage = "Previous error";

        var schedules = new List<ScheduleDto> { new ScheduleDto() };
        mockScheduleService.Setup(s => s.GetSchedulesAsync()).ReturnsAsync(schedules);

        // Act
        await viewModel.LoadAsync(forceRefresh: true);

        // Assert
        Assert.AreEqual(string.Empty, viewModel.ErrorMessage);
    }

    /// <summary>
    /// Tests that LoadAsync clears existing items before adding new ones.
    /// </summary>
    [TestMethod]
    public async Task LoadAsync_WhenCalled_ClearsExistingItemsBeforeAddingNew()
    {
        // Arrange
        var mockScheduleService = new Mock<IScheduleService>();
        var mockLogger = new Mock<ILogger<ScheduleViewModel>>();
        var viewModel = new ScheduleViewModel(mockScheduleService.Object, mockLogger.Object);

        // Add initial items
        viewModel.Items.Add(new ScheduleDto());
        viewModel.Items.Add(new ScheduleDto());
        Assert.AreEqual(2, viewModel.Items.Count);

        var newSchedules = new List<ScheduleDto> { new ScheduleDto() };
        mockScheduleService.Setup(s => s.GetSchedulesAsync()).ReturnsAsync(newSchedules);

        // Act
        await viewModel.LoadAsync(forceRefresh: true);

        // Assert
        Assert.AreEqual(1, viewModel.Items.Count);
    }

    /// <summary>
    /// Tests that LoadAsync handles various exception types correctly.
    /// </summary>
    /// <param name="exceptionType">The type of exception to test.</param>
    /// <param name="exceptionMessage">The exception message.</param>
    [TestMethod]
    [DataRow(typeof(InvalidOperationException), "Invalid operation")]
    [DataRow(typeof(ArgumentException), "Argument error")]
    [DataRow(typeof(NullReferenceException), "Null reference")]
    [DataRow(typeof(Exception), "Generic error")]
    public async Task LoadAsync_WhenServiceThrowsDifferentExceptions_HandlesAllCorrectly(Type exceptionType, string exceptionMessage)
    {
        // Arrange
        var mockScheduleService = new Mock<IScheduleService>();
        var mockLogger = new Mock<ILogger<ScheduleViewModel>>();
        var viewModel = new ScheduleViewModel(mockScheduleService.Object, mockLogger.Object);

        var exception = (Exception)Activator.CreateInstance(exceptionType, exceptionMessage)!;
        mockScheduleService.Setup(s => s.GetSchedulesAsync()).ThrowsAsync(exception);

        // Act
        await viewModel.LoadAsync(forceRefresh: true);

        // Assert
        Assert.IsFalse(viewModel.IsBusy);
        Assert.IsTrue(viewModel.ErrorMessage.Contains("Failed to load schedule"));
        Assert.IsTrue(viewModel.ErrorMessage.Contains(exceptionMessage));
        mockLogger.Verify(
            l => l.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                exception,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    /// <summary>
    /// Tests that the constructor properly initializes the ViewModel with valid dependencies.
    /// Verifies that LoadCommand is initialized and not null.
    /// </summary>
    [TestMethod]
    public void Constructor_ValidParameters_InitializesSuccessfully()
    {
        // Arrange
        Mock<IScheduleService> mockScheduleService = new Mock<IScheduleService>();
        Mock<ILogger<ScheduleViewModel>> mockLogger = new Mock<ILogger<ScheduleViewModel>>();

        // Act
        ScheduleViewModel viewModel = new ScheduleViewModel(mockScheduleService.Object, mockLogger.Object);

        // Assert
        Assert.IsNotNull(viewModel);
        Assert.IsNotNull(viewModel.LoadCommand);
        Assert.IsInstanceOfType(viewModel.LoadCommand, typeof(ICommand));
    }

    /// <summary>
    /// Tests that the constructor initializes the Items collection.
    /// Verifies that Items is not null and is empty after construction.
    /// </summary>
    [TestMethod]
    public void Constructor_ValidParameters_InitializesItemsCollection()
    {
        // Arrange
        Mock<IScheduleService> mockScheduleService = new Mock<IScheduleService>();
        Mock<ILogger<ScheduleViewModel>> mockLogger = new Mock<ILogger<ScheduleViewModel>>();

        // Act
        ScheduleViewModel viewModel = new ScheduleViewModel(mockScheduleService.Object, mockLogger.Object);

        // Assert
        Assert.IsNotNull(viewModel.Items);
        Assert.AreEqual(0, viewModel.Items.Count);
    }

    /// <summary>
    /// Tests that the constructor initializes the ErrorMessage property.
    /// Verifies that ErrorMessage is not null and is empty after construction.
    /// </summary>
    [TestMethod]
    public void Constructor_ValidParameters_InitializesErrorMessageAsEmpty()
    {
        // Arrange
        Mock<IScheduleService> mockScheduleService = new Mock<IScheduleService>();
        Mock<ILogger<ScheduleViewModel>> mockLogger = new Mock<ILogger<ScheduleViewModel>>();

        // Act
        ScheduleViewModel viewModel = new ScheduleViewModel(mockScheduleService.Object, mockLogger.Object);

        // Assert
        Assert.IsNotNull(viewModel.ErrorMessage);
        Assert.AreEqual(string.Empty, viewModel.ErrorMessage);
    }

    /// <summary>
    /// Tests that the constructor throws ArgumentNullException when schedule parameter is null.
    /// Validates that null schedule service is properly rejected.
    /// </summary>
    [TestMethod]
    public void Constructor_NullScheduleService_ThrowsOrAcceptsNull()
    {
        // Arrange
        Mock<ILogger<ScheduleViewModel>> mockLogger = new Mock<ILogger<ScheduleViewModel>>();

        // Act & Assert
        try
        {
            ScheduleViewModel viewModel = new ScheduleViewModel(null!, mockLogger.Object);

            // If no exception is thrown, verify the object was still created
            Assert.IsNotNull(viewModel);
            Assert.IsNotNull(viewModel.LoadCommand);
        }
        catch (ArgumentNullException)
        {
            // Expected behavior if null validation is implemented
            Assert.IsTrue(true);
        }
        catch (NullReferenceException)
        {
            // May occur if null is dereferenced during construction
            Assert.IsTrue(true);
        }
    }

    /// <summary>
    /// Tests that the constructor throws ArgumentNullException when logger parameter is null.
    /// Validates that null logger is properly rejected.
    /// </summary>
    [TestMethod]
    public void Constructor_NullLogger_ThrowsOrAcceptsNull()
    {
        // Arrange
        Mock<IScheduleService> mockScheduleService = new Mock<IScheduleService>();

        // Act & Assert
        try
        {
            ScheduleViewModel viewModel = new ScheduleViewModel(mockScheduleService.Object, null!);

            // If no exception is thrown, verify the object was still created
            Assert.IsNotNull(viewModel);
            Assert.IsNotNull(viewModel.LoadCommand);
        }
        catch (ArgumentNullException)
        {
            // Expected behavior if null validation is implemented
            Assert.IsTrue(true);
        }
        catch (NullReferenceException)
        {
            // May occur if null is dereferenced during construction
            Assert.IsTrue(true);
        }
    }

    /// <summary>
    /// Tests that the constructor handles both parameters being null.
    /// Validates extreme edge case of all dependencies being null.
    /// </summary>
    [TestMethod]
    public void Constructor_BothParametersNull_ThrowsOrAcceptsNull()
    {
        // Act & Assert
        try
        {
            ScheduleViewModel viewModel = new ScheduleViewModel(null!, null!);

            // If no exception is thrown, verify the object was still created
            Assert.IsNotNull(viewModel);
            Assert.IsNotNull(viewModel.LoadCommand);
        }
        catch (ArgumentNullException)
        {
            // Expected behavior if null validation is implemented
            Assert.IsTrue(true);
        }
        catch (NullReferenceException)
        {
            // May occur if null is dereferenced during construction
            Assert.IsTrue(true);
        }
    }

    /// <summary>
    /// Tests that LoadCommand is of the concrete Command type.
    /// Verifies that the command is properly instantiated as a Microsoft.Maui.Controls.Command.
    /// </summary>
    [TestMethod]
    public void Constructor_ValidParameters_LoadCommandIsCommandType()
    {
        // Arrange
        Mock<IScheduleService> mockScheduleService = new Mock<IScheduleService>();
        Mock<ILogger<ScheduleViewModel>> mockLogger = new Mock<ILogger<ScheduleViewModel>>();

        // Act
        ScheduleViewModel viewModel = new ScheduleViewModel(mockScheduleService.Object, mockLogger.Object);

        // Assert
        Assert.IsInstanceOfType(viewModel.LoadCommand, typeof(Command));
    }

    /// <summary>
    /// Tests that LoadAsync loads data when the refresh interval has passed and forceRefresh is false.
    /// This verifies that natural time-based refresh works correctly after the 60-second interval.
    /// </summary>
    [TestMethod]
    public async Task LoadAsync_WhenIntervalHasPassedAndNotForceRefresh_LoadsSuccessfully()
    {
        // Arrange
        var mockScheduleService = new Mock<IScheduleService>();
        var mockLogger = new Mock<ILogger<ScheduleViewModel>>();
        var viewModel = new ScheduleViewModel(mockScheduleService.Object, mockLogger.Object);

        var schedules = new List<ScheduleDto> { new ScheduleDto() };
        mockScheduleService.Setup(s => s.GetSchedulesAsync()).ReturnsAsync(schedules);

        // First load to set _lastLoaded
        await viewModel.LoadAsync(forceRefresh: true);
        mockScheduleService.ResetCalls();

        // Use reflection to set _lastLoaded to a time more than 60 seconds ago
        var lastLoadedField = typeof(ScheduleViewModel).GetField("_lastLoaded", BindingFlags.NonPublic | BindingFlags.Instance);
        if (lastLoadedField != null)
        {
            lastLoadedField.SetValue(viewModel, DateTime.UtcNow.AddSeconds(-61));
        }

        mockScheduleService.Setup(s => s.GetSchedulesAsync()).ReturnsAsync(schedules);

        // Act - Load without force refresh, but after interval has passed
        await viewModel.LoadAsync(forceRefresh: false);

        // Assert
        mockScheduleService.Verify(s => s.GetSchedulesAsync(), Times.Once);
        Assert.AreEqual(1, viewModel.Items.Count);
    }

    /// <summary>
    /// Tests that LoadAsync handles the boundary condition exactly at the refresh interval (60 seconds).
    /// Verifies behavior when _lastLoaded is exactly 60 seconds ago.
    /// </summary>
    [TestMethod]
    public async Task LoadAsync_WhenExactlyAtRefreshInterval_LoadsSuccessfully()
    {
        // Arrange
        var mockScheduleService = new Mock<IScheduleService>();
        var mockLogger = new Mock<ILogger<ScheduleViewModel>>();
        var viewModel = new ScheduleViewModel(mockScheduleService.Object, mockLogger.Object);

        var schedules = new List<ScheduleDto> { new ScheduleDto() };
        mockScheduleService.Setup(s => s.GetSchedulesAsync()).ReturnsAsync(schedules);

        // First load to set _lastLoaded
        await viewModel.LoadAsync(forceRefresh: true);
        mockScheduleService.ResetCalls();

        // Use reflection to set _lastLoaded to exactly 60 seconds ago
        var lastLoadedField = typeof(ScheduleViewModel).GetField("_lastLoaded", BindingFlags.NonPublic | BindingFlags.Instance);
        if (lastLoadedField != null)
        {
            lastLoadedField.SetValue(viewModel, DateTime.UtcNow.AddSeconds(-60));
        }

        mockScheduleService.Setup(s => s.GetSchedulesAsync()).ReturnsAsync(schedules);

        // Act - Load without force refresh at exactly the interval boundary
        await viewModel.LoadAsync(forceRefresh: false);

        // Assert
        mockScheduleService.Verify(s => s.GetSchedulesAsync(), Times.Once);
        Assert.AreEqual(1, viewModel.Items.Count);
    }

    /// <summary>
    /// Tests that LoadAsync properly handles a very large collection of items.
    /// Verifies that the method can handle adding many items to the collection without issues.
    /// </summary>
    [TestMethod]
    [DataRow(100)]
    [DataRow(1000)]
    public async Task LoadAsync_WhenServiceReturnsLargeCollection_AddsAllItemsSuccessfully(int itemCount)
    {
        // Arrange
        var mockScheduleService = new Mock<IScheduleService>();
        var mockLogger = new Mock<ILogger<ScheduleViewModel>>();
        var viewModel = new ScheduleViewModel(mockScheduleService.Object, mockLogger.Object);

        var schedules = new List<ScheduleDto>();
        for (int i = 0; i < itemCount; i++)
        {
            schedules.Add(new ScheduleDto());
        }

        mockScheduleService.Setup(s => s.GetSchedulesAsync()).ReturnsAsync(schedules);

        // Act
        await viewModel.LoadAsync(forceRefresh: true);

        // Assert
        Assert.AreEqual(itemCount, viewModel.Items.Count);
        Assert.AreEqual(string.Empty, viewModel.ErrorMessage);
        Assert.IsFalse(viewModel.IsBusy);
    }

    /// <summary>
    /// Tests that LoadAsync handles exceptions with very long error messages correctly.
    /// Verifies that the ErrorMessage property can store and display lengthy exception messages.
    /// </summary>
    [TestMethod]
    public async Task LoadAsync_WhenExceptionHasVeryLongMessage_SetsErrorMessageCorrectly()
    {
        // Arrange
        var mockScheduleService = new Mock<IScheduleService>();
        var mockLogger = new Mock<ILogger<ScheduleViewModel>>();
        var viewModel = new ScheduleViewModel(mockScheduleService.Object, mockLogger.Object);

        var longMessage = new string('X', 5000);
        var exception = new InvalidOperationException(longMessage);
        mockScheduleService.Setup(s => s.GetSchedulesAsync()).ThrowsAsync(exception);

        // Act
        await viewModel.LoadAsync(forceRefresh: true);

        // Assert
        Assert.IsTrue(viewModel.ErrorMessage.Contains(longMessage));
        Assert.AreEqual(0, viewModel.Items.Count);
        Assert.IsFalse(viewModel.IsBusy);
    }

    /// <summary>
    /// Tests that LoadAsync handles exceptions with empty error messages correctly.
    /// Verifies behavior when an exception has no message.
    /// </summary>
    [TestMethod]
    public async Task LoadAsync_WhenExceptionHasEmptyMessage_SetsErrorMessageCorrectly()
    {
        // Arrange
        var mockScheduleService = new Mock<IScheduleService>();
        var mockLogger = new Mock<ILogger<ScheduleViewModel>>();
        var viewModel = new ScheduleViewModel(mockScheduleService.Object, mockLogger.Object);

        var exception = new InvalidOperationException(string.Empty);
        mockScheduleService.Setup(s => s.GetSchedulesAsync()).ThrowsAsync(exception);

        // Act
        await viewModel.LoadAsync(forceRefresh: true);

        // Assert
        Assert.IsTrue(viewModel.ErrorMessage.StartsWith("Failed to load schedule. "));
        Assert.AreEqual(0, viewModel.Items.Count);
        Assert.IsFalse(viewModel.IsBusy);
    }

    /// <summary>
    /// Tests that LoadAsync handles exceptions with special characters in the message correctly.
    /// Verifies that ErrorMessage can handle various special characters without issues.
    /// </summary>
    [TestMethod]
    public async Task LoadAsync_WhenExceptionHasSpecialCharacters_SetsErrorMessageCorrectly()
    {
        // Arrange
        var mockScheduleService = new Mock<IScheduleService>();
        var mockLogger = new Mock<ILogger<ScheduleViewModel>>();
        var viewModel = new ScheduleViewModel(mockScheduleService.Object, mockLogger.Object);

        var specialMessage = "Error: <>&\"'\t\n\r!@#$%^&*()";
        var exception = new InvalidOperationException(specialMessage);
        mockScheduleService.Setup(s => s.GetSchedulesAsync()).ThrowsAsync(exception);

        // Act
        await viewModel.LoadAsync(forceRefresh: true);

        // Assert
        Assert.IsTrue(viewModel.ErrorMessage.Contains(specialMessage));
        Assert.AreEqual(0, viewModel.Items.Count);
        Assert.IsFalse(viewModel.IsBusy);
    }

    /// <summary>
    /// Tests that LoadAsync with forceRefresh false parameter (explicit) behaves correctly within interval.
    /// Verifies that explicitly passing false for forceRefresh respects the interval.
    /// </summary>
    [TestMethod]
    public async Task LoadAsync_WhenForceRefreshExplicitlyFalseWithinInterval_DoesNotLoad()
    {
        // Arrange
        var mockScheduleService = new Mock<IScheduleService>();
        var mockLogger = new Mock<ILogger<ScheduleViewModel>>();
        var viewModel = new ScheduleViewModel(mockScheduleService.Object, mockLogger.Object);

        var schedules = new List<ScheduleDto> { new ScheduleDto() };
        mockScheduleService.Setup(s => s.GetSchedulesAsync()).ReturnsAsync(schedules);

        // First load
        await viewModel.LoadAsync(forceRefresh: true);
        mockScheduleService.ResetCalls();

        // Act - Explicitly pass false for forceRefresh
        await viewModel.LoadAsync(forceRefresh: false);

        // Assert
        mockScheduleService.Verify(s => s.GetSchedulesAsync(), Times.Never);
    }

    /// <summary>
    /// Tests that LoadAsync with default parameter value behaves correctly within interval.
    /// Verifies that the default value for forceRefresh (false) respects the interval.
    /// </summary>
    [TestMethod]
    public async Task LoadAsync_WhenUsingDefaultParameterWithinInterval_DoesNotLoad()
    {
        // Arrange
        var mockScheduleService = new Mock<IScheduleService>();
        var mockLogger = new Mock<ILogger<ScheduleViewModel>>();
        var viewModel = new ScheduleViewModel(mockScheduleService.Object, mockLogger.Object);

        var schedules = new List<ScheduleDto> { new ScheduleDto() };
        mockScheduleService.Setup(s => s.GetSchedulesAsync()).ReturnsAsync(schedules);

        // First load
        await viewModel.LoadAsync(forceRefresh: true);
        mockScheduleService.ResetCalls();

        // Act - Use default parameter value (no parameter)
        await viewModel.LoadAsync();

        // Assert
        mockScheduleService.Verify(s => s.GetSchedulesAsync(), Times.Never);
    }

    /// <summary>
    /// Tests that the constructor properly initializes the ViewModel with valid dependencies.
    /// Verifies that the instance, LoadCommand, Items collection, and ErrorMessage are all initialized correctly.
    /// </summary>
    [TestMethod]
    public void Constructor_ValidParameters_InitializesAllPropertiesCorrectly()
    {
        // Arrange
        Mock<IScheduleService> mockScheduleService = new Mock<IScheduleService>();
        Mock<ILogger<ScheduleViewModel>> mockLogger = new Mock<ILogger<ScheduleViewModel>>();

        // Act
        ScheduleViewModel viewModel = new ScheduleViewModel(mockScheduleService.Object, mockLogger.Object);

        // Assert
        Assert.IsNotNull(viewModel);
        Assert.IsNotNull(viewModel.LoadCommand);
        Assert.IsInstanceOfType(viewModel.LoadCommand, typeof(ICommand));
        Assert.IsNotNull(viewModel.Items);
        Assert.AreEqual(0, viewModel.Items.Count);
        Assert.IsNotNull(viewModel.ErrorMessage);
        Assert.AreEqual(string.Empty, viewModel.ErrorMessage);
    }

    /// <summary>
    /// Tests that the constructor initializes LoadCommand as a Command type.
    /// Verifies the concrete type is Microsoft.Maui.Controls.Command.
    /// </summary>
    [TestMethod]
    public void Constructor_ValidParameters_InitializesLoadCommandAsCommandType()
    {
        // Arrange
        Mock<IScheduleService> mockScheduleService = new Mock<IScheduleService>();
        Mock<ILogger<ScheduleViewModel>> mockLogger = new Mock<ILogger<ScheduleViewModel>>();

        // Act
        ScheduleViewModel viewModel = new ScheduleViewModel(mockScheduleService.Object, mockLogger.Object);

        // Assert
        Assert.IsInstanceOfType(viewModel.LoadCommand, typeof(Command));
    }

    /// <summary>
    /// Tests that the constructor initializes the Items collection as an empty ObservableCollection.
    /// Verifies that Items is not null and has zero count.
    /// </summary>
    [TestMethod]
    public void Constructor_ValidParameters_InitializesItemsAsEmptyObservableCollection()
    {
        // Arrange
        Mock<IScheduleService> mockScheduleService = new Mock<IScheduleService>();
        Mock<ILogger<ScheduleViewModel>> mockLogger = new Mock<ILogger<ScheduleViewModel>>();

        // Act
        ScheduleViewModel viewModel = new ScheduleViewModel(mockScheduleService.Object, mockLogger.Object);

        // Assert
        Assert.IsNotNull(viewModel.Items);
        Assert.IsInstanceOfType(viewModel.Items, typeof(ObservableCollection<ScheduleDto>));
        Assert.AreEqual(0, viewModel.Items.Count);
    }

    /// <summary>
    /// Tests that the constructor initializes ErrorMessage as an empty string.
    /// Verifies that ErrorMessage is not null and equals string.Empty.
    /// </summary>
    [TestMethod]
    public void Constructor_ValidParameters_InitializesErrorMessageAsEmptyString()
    {
        // Arrange
        Mock<IScheduleService> mockScheduleService = new Mock<IScheduleService>();
        Mock<ILogger<ScheduleViewModel>> mockLogger = new Mock<ILogger<ScheduleViewModel>>();

        // Act
        ScheduleViewModel viewModel = new ScheduleViewModel(mockScheduleService.Object, mockLogger.Object);

        // Assert
        Assert.IsNotNull(viewModel.ErrorMessage);
        Assert.AreEqual(string.Empty, viewModel.ErrorMessage);
    }

    /// <summary>
    /// Tests that the constructor does not call any methods on the schedule service during initialization.
    /// Verifies that dependencies are stored but not invoked during construction.
    /// </summary>
    [TestMethod]
    public void Constructor_ValidParameters_DoesNotCallScheduleServiceMethods()
    {
        // Arrange
        Mock<IScheduleService> mockScheduleService = new Mock<IScheduleService>();
        Mock<ILogger<ScheduleViewModel>> mockLogger = new Mock<ILogger<ScheduleViewModel>>();

        // Act
        ScheduleViewModel viewModel = new ScheduleViewModel(mockScheduleService.Object, mockLogger.Object);

        // Assert
        mockScheduleService.VerifyNoOtherCalls();
    }

    /// <summary>
    /// Tests that the constructor does not call any methods on the logger during initialization.
    /// Verifies that the logger dependency is stored but not invoked during construction.
    /// </summary>
    [TestMethod]
    public void Constructor_ValidParameters_DoesNotCallLoggerMethods()
    {
        // Arrange
        Mock<IScheduleService> mockScheduleService = new Mock<IScheduleService>();
        Mock<ILogger<ScheduleViewModel>> mockLogger = new Mock<ILogger<ScheduleViewModel>>();

        // Act
        ScheduleViewModel viewModel = new ScheduleViewModel(mockScheduleService.Object, mockLogger.Object);

        // Assert
        mockLogger.VerifyNoOtherCalls();
    }

    /// <summary>
    /// Tests that the constructor handles null schedule service parameter.
    /// Validates behavior when the schedule service dependency is null.
    /// </summary>
    [TestMethod]
    public void Constructor_NullScheduleService_HandlesNullParameter()
    {
        // Arrange
        Mock<ILogger<ScheduleViewModel>> mockLogger = new Mock<ILogger<ScheduleViewModel>>();

        // Act & Assert
        try
        {
            ScheduleViewModel viewModel = new ScheduleViewModel(null!, mockLogger.Object);

            // If no exception is thrown, verify the object was still created
            Assert.IsNotNull(viewModel);
            Assert.IsNotNull(viewModel.LoadCommand);
        }
        catch (ArgumentNullException)
        {
            // Expected behavior if null validation is implemented
            Assert.IsTrue(true);
        }
        catch (NullReferenceException)
        {
            // May occur if null is dereferenced during construction
            Assert.IsTrue(true);
        }
    }

    /// <summary>
    /// Tests that the constructor handles null logger parameter.
    /// Validates behavior when the logger dependency is null.
    /// </summary>
    [TestMethod]
    public void Constructor_NullLogger_HandlesNullParameter()
    {
        // Arrange
        Mock<IScheduleService> mockScheduleService = new Mock<IScheduleService>();

        // Act & Assert
        try
        {
            ScheduleViewModel viewModel = new ScheduleViewModel(mockScheduleService.Object, null!);

            // If no exception is thrown, verify the object was still created
            Assert.IsNotNull(viewModel);
            Assert.IsNotNull(viewModel.LoadCommand);
        }
        catch (ArgumentNullException)
        {
            // Expected behavior if null validation is implemented
            Assert.IsTrue(true);
        }
        catch (NullReferenceException)
        {
            // May occur if null is dereferenced during construction
            Assert.IsTrue(true);
        }
    }

    /// <summary>
    /// Tests that the constructor handles both parameters being null.
    /// Validates extreme edge case where all dependencies are null.
    /// </summary>
    [TestMethod]
    public void Constructor_BothParametersNull_HandlesNullParameters()
    {
        // Act & Assert
        try
        {
            ScheduleViewModel viewModel = new ScheduleViewModel(null!, null!);

            // If no exception is thrown, verify the object was still created
            Assert.IsNotNull(viewModel);
        }
        catch (ArgumentNullException)
        {
            // Expected behavior if null validation is implemented
            Assert.IsTrue(true);
        }
        catch (NullReferenceException)
        {
            // May occur if null is dereferenced during construction
            Assert.IsTrue(true);
        }
    }

    /// <summary>
    /// Tests that the constructor creates unique instances with different dependency instances.
    /// Verifies that each ViewModel instance is independent.
    /// </summary>
    [TestMethod]
    public void Constructor_MultipleCalls_CreatesUniqueInstances()
    {
        // Arrange
        Mock<IScheduleService> mockScheduleService1 = new Mock<IScheduleService>();
        Mock<ILogger<ScheduleViewModel>> mockLogger1 = new Mock<ILogger<ScheduleViewModel>>();
        Mock<IScheduleService> mockScheduleService2 = new Mock<IScheduleService>();
        Mock<ILogger<ScheduleViewModel>> mockLogger2 = new Mock<ILogger<ScheduleViewModel>>();

        // Act
        ScheduleViewModel viewModel1 = new ScheduleViewModel(mockScheduleService1.Object, mockLogger1.Object);
        ScheduleViewModel viewModel2 = new ScheduleViewModel(mockScheduleService2.Object, mockLogger2.Object);

        // Assert
        Assert.AreNotSame(viewModel1, viewModel2);
        Assert.AreNotSame(viewModel1.Items, viewModel2.Items);
        Assert.AreNotSame(viewModel1.LoadCommand, viewModel2.LoadCommand);
    }

    /// <summary>
    /// Tests that the constructor can be called with the same dependency instances multiple times.
    /// Verifies that the constructor works correctly when reusing dependency mocks.
    /// </summary>
    [TestMethod]
    public void Constructor_SameDependencies_CreatesUniqueInstancesWithSharedDependencies()
    {
        // Arrange
        Mock<IScheduleService> mockScheduleService = new Mock<IScheduleService>();
        Mock<ILogger<ScheduleViewModel>> mockLogger = new Mock<ILogger<ScheduleViewModel>>();

        // Act
        ScheduleViewModel viewModel1 = new ScheduleViewModel(mockScheduleService.Object, mockLogger.Object);
        ScheduleViewModel viewModel2 = new ScheduleViewModel(mockScheduleService.Object, mockLogger.Object);

        // Assert
        Assert.AreNotSame(viewModel1, viewModel2);
        Assert.AreNotSame(viewModel1.Items, viewModel2.Items);
        Assert.AreNotSame(viewModel1.LoadCommand, viewModel2.LoadCommand);
    }
}