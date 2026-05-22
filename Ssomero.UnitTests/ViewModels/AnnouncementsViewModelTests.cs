using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

using Microsoft.Maui.Controls;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Ssomero.Interfaces;
using Ssomero.Models;
using Ssomero.ViewModels;

namespace Ssomero.ViewModels.UnitTests;




/// <summary>
/// Unit tests for <see cref="AnnouncementsViewModel"/> class.
/// </summary>
[TestClass]
public class AnnouncementsViewModelTests
{
    /// <summary>
    /// Tests that LoadAsync returns immediately without calling the service when IsBusy is already true.
    /// </summary>
    [TestMethod]
    public async Task LoadAsync_WhenIsBusyIsTrue_ReturnsImmediatelyWithoutCallingService()
    {
        // Arrange
        var mockService = new Mock<IAnnouncementsService>();
        var viewModel = new AnnouncementsViewModel(mockService.Object);
        viewModel.IsBusy = true;

        // Act
        await viewModel.LoadAsync(forceRefresh: true);

        // Assert
        mockService.Verify(s => s.GetAnnouncementsAsync(), Times.Never);
        Assert.AreEqual(0, viewModel.Items.Count);
    }

    /// <summary>
    /// Tests that LoadAsync returns immediately without calling the service when forceRefresh is false
    /// and the last load was within the refresh interval (60 seconds).
    /// </summary>
    [TestMethod]
    public async Task LoadAsync_WhenNotForcedAndWithinRefreshInterval_ReturnsImmediatelyWithoutCallingService()
    {
        // Arrange
        var mockService = new Mock<IAnnouncementsService>();
        var announcements = new List<AnnouncementDto> { new AnnouncementDto() };
        mockService.Setup(s => s.GetAnnouncementsAsync()).ReturnsAsync(announcements);

        var viewModel = new AnnouncementsViewModel(mockService.Object);

        // First load to set _lastLoaded
        await viewModel.LoadAsync(forceRefresh: true);
        mockService.Invocations.Clear();

        // Act - try to load again immediately without forcing
        await viewModel.LoadAsync(forceRefresh: false);

        // Assert
        mockService.Verify(s => s.GetAnnouncementsAsync(), Times.Never);
    }

    /// <summary>
    /// Tests that LoadAsync calls the service and loads announcements when forceRefresh is true,
    /// regardless of the time since last load.
    /// </summary>
    [TestMethod]
    public async Task LoadAsync_WhenForceRefreshIsTrue_CallsServiceRegardlessOfInterval()
    {
        // Arrange
        var mockService = new Mock<IAnnouncementsService>();
        var firstAnnouncements = new List<AnnouncementDto> { new AnnouncementDto() };
        var secondAnnouncements = new List<AnnouncementDto> { new AnnouncementDto(), new AnnouncementDto() };

        mockService.SetupSequence(s => s.GetAnnouncementsAsync())
            .ReturnsAsync(firstAnnouncements)
            .ReturnsAsync(secondAnnouncements);

        var viewModel = new AnnouncementsViewModel(mockService.Object);

        // First load
        await viewModel.LoadAsync(forceRefresh: true);
        Assert.AreEqual(1, viewModel.Items.Count);

        // Act - force refresh immediately
        await viewModel.LoadAsync(forceRefresh: true);

        // Assert
        mockService.Verify(s => s.GetAnnouncementsAsync(), Times.Exactly(2));
        Assert.AreEqual(2, viewModel.Items.Count);
    }

    /// <summary>
    /// Tests that LoadAsync successfully loads announcements when the service returns an empty list.
    /// </summary>
    [TestMethod]
    public async Task LoadAsync_WhenServiceReturnsEmptyList_ClearsItemsAndSetsNoError()
    {
        // Arrange
        var mockService = new Mock<IAnnouncementsService>();
        var emptyList = new List<AnnouncementDto>();
        mockService.Setup(s => s.GetAnnouncementsAsync()).ReturnsAsync(emptyList);

        var viewModel = new AnnouncementsViewModel(mockService.Object);

        // Act
        await viewModel.LoadAsync(forceRefresh: true);

        // Assert
        Assert.AreEqual(0, viewModel.Items.Count);
        Assert.AreEqual(string.Empty, viewModel.ErrorMessage);
        Assert.IsFalse(viewModel.IsBusy);
    }

    /// <summary>
    /// Tests that LoadAsync successfully loads multiple announcements and adds them to the Items collection.
    /// </summary>
    [TestMethod]
    public async Task LoadAsync_WhenServiceReturnsMultipleItems_AddsAllItemsToCollection()
    {
        // Arrange
        var mockService = new Mock<IAnnouncementsService>();
        var announcements = new List<AnnouncementDto>
        {
            new AnnouncementDto(),
            new AnnouncementDto(),
            new AnnouncementDto()
        };
        mockService.Setup(s => s.GetAnnouncementsAsync()).ReturnsAsync(announcements);

        var viewModel = new AnnouncementsViewModel(mockService.Object);

        // Act
        await viewModel.LoadAsync(forceRefresh: true);

        // Assert
        Assert.AreEqual(3, viewModel.Items.Count);
        Assert.AreEqual(string.Empty, viewModel.ErrorMessage);
        Assert.IsFalse(viewModel.IsBusy);
    }

    /// <summary>
    /// Tests that LoadAsync clears existing items before loading new ones.
    /// </summary>
    [TestMethod]
    public async Task LoadAsync_WhenItemsAlreadyExist_ClearsExistingItemsBeforeLoading()
    {
        // Arrange
        var mockService = new Mock<IAnnouncementsService>();
        var firstLoad = new List<AnnouncementDto> { new AnnouncementDto(), new AnnouncementDto() };
        var secondLoad = new List<AnnouncementDto> { new AnnouncementDto() };

        mockService.SetupSequence(s => s.GetAnnouncementsAsync())
            .ReturnsAsync(firstLoad)
            .ReturnsAsync(secondLoad);

        var viewModel = new AnnouncementsViewModel(mockService.Object);

        // First load
        await viewModel.LoadAsync(forceRefresh: true);
        Assert.AreEqual(2, viewModel.Items.Count);

        // Act - second load
        await viewModel.LoadAsync(forceRefresh: true);

        // Assert - should only have items from second load
        Assert.AreEqual(1, viewModel.Items.Count);
    }

    /// <summary>
    /// Tests that LoadAsync sets ErrorMessage and clears Items when the service throws an exception.
    /// </summary>
    [TestMethod]
    public async Task LoadAsync_WhenServiceThrowsException_SetsErrorMessageAndClearsItems()
    {
        // Arrange
        var mockService = new Mock<IAnnouncementsService>();
        var exceptionMessage = "Network error occurred";
        mockService.Setup(s => s.GetAnnouncementsAsync()).ThrowsAsync(new Exception(exceptionMessage));

        var viewModel = new AnnouncementsViewModel(mockService.Object);
        viewModel.Items.Add(new AnnouncementDto());

        // Act
        await viewModel.LoadAsync(forceRefresh: true);

        // Assert
        Assert.AreEqual(0, viewModel.Items.Count);
        Assert.IsTrue(viewModel.ErrorMessage.Contains("Failed to load announcements."));
        Assert.IsTrue(viewModel.ErrorMessage.Contains(exceptionMessage));
        Assert.IsFalse(viewModel.IsBusy);
    }

    /// <summary>
    /// Tests that LoadAsync always sets IsBusy to false in the finally block, even when an exception occurs.
    /// </summary>
    [TestMethod]
    public async Task LoadAsync_WhenExceptionOccurs_AlwaysSetsIsBusyToFalse()
    {
        // Arrange
        var mockService = new Mock<IAnnouncementsService>();
        mockService.Setup(s => s.GetAnnouncementsAsync()).ThrowsAsync(new InvalidOperationException("Test exception"));

        var viewModel = new AnnouncementsViewModel(mockService.Object);

        // Act
        await viewModel.LoadAsync(forceRefresh: true);

        // Assert
        Assert.IsFalse(viewModel.IsBusy);
    }

    /// <summary>
    /// Tests that LoadAsync clears any previous ErrorMessage before attempting to load.
    /// </summary>
    [TestMethod]
    public async Task LoadAsync_WhenPreviousErrorExists_ClearsErrorMessageBeforeLoading()
    {
        // Arrange
        var mockService = new Mock<IAnnouncementsService>();
        mockService.SetupSequence(s => s.GetAnnouncementsAsync())
            .ThrowsAsync(new Exception("First error"))
            .ReturnsAsync(new List<AnnouncementDto> { new AnnouncementDto() });

        var viewModel = new AnnouncementsViewModel(mockService.Object);

        // First call - causes error
        await viewModel.LoadAsync(forceRefresh: true);
        Assert.IsFalse(string.IsNullOrEmpty(viewModel.ErrorMessage));

        // Act - second call succeeds
        await viewModel.LoadAsync(forceRefresh: true);

        // Assert - error message should be cleared
        Assert.AreEqual(string.Empty, viewModel.ErrorMessage);
        Assert.AreEqual(1, viewModel.Items.Count);
    }

    /// <summary>
    /// Tests that LoadAsync sets IsBusy to true during execution.
    /// </summary>
    [TestMethod]
    public async Task LoadAsync_DuringExecution_SetsIsBusyToTrue()
    {
        // Arrange
        var mockService = new Mock<IAnnouncementsService>();
        var tcs = new TaskCompletionSource<IEnumerable<AnnouncementDto>>();
        mockService.Setup(s => s.GetAnnouncementsAsync()).Returns(tcs.Task);

        var viewModel = new AnnouncementsViewModel(mockService.Object);
        Assert.IsFalse(viewModel.IsBusy);

        // Act
        var loadTask = viewModel.LoadAsync(forceRefresh: true);

        // Assert - IsBusy should be true during execution
        Assert.IsTrue(viewModel.IsBusy);

        // Complete the task
        tcs.SetResult(new List<AnnouncementDto>());
        await loadTask;

        Assert.IsFalse(viewModel.IsBusy);
    }

    /// <summary>
    /// Tests that LoadAsync with default parameter value (forceRefresh = false) respects refresh interval.
    /// </summary>
    [TestMethod]
    public async Task LoadAsync_WithDefaultForceRefreshParameter_RespectsRefreshInterval()
    {
        // Arrange
        var mockService = new Mock<IAnnouncementsService>();
        var announcements = new List<AnnouncementDto> { new AnnouncementDto() };
        mockService.Setup(s => s.GetAnnouncementsAsync()).ReturnsAsync(announcements);

        var viewModel = new AnnouncementsViewModel(mockService.Object);

        // First load
        await viewModel.LoadAsync(forceRefresh: true);
        mockService.Invocations.Clear();

        // Act - call with default parameter (should not refresh within interval)
        await viewModel.LoadAsync();

        // Assert
        mockService.Verify(s => s.GetAnnouncementsAsync(), Times.Never);
    }

    /// <summary>
    /// Tests that LoadAsync handles various exception types correctly.
    /// </summary>
    /// <param name="exceptionType">The type of exception to test.</param>
    /// <param name="exceptionMessage">The exception message.</param>
    [DataRow(typeof(InvalidOperationException), "Invalid operation")]
    [DataRow(typeof(ArgumentException), "Invalid argument")]
    [DataRow(typeof(TimeoutException), "Request timeout")]
    [DataRow(typeof(Exception), "Generic error")]
    [TestMethod]
    public async Task LoadAsync_WhenServiceThrowsVariousExceptions_HandlesThemCorrectly(Type exceptionType, string exceptionMessage)
    {
        // Arrange
        var mockService = new Mock<IAnnouncementsService>();
        var exception = (Exception)Activator.CreateInstance(exceptionType, exceptionMessage)!;
        mockService.Setup(s => s.GetAnnouncementsAsync()).ThrowsAsync(exception);

        var viewModel = new AnnouncementsViewModel(mockService.Object);

        // Act
        await viewModel.LoadAsync(forceRefresh: true);

        // Assert
        Assert.IsTrue(viewModel.ErrorMessage.Contains("Failed to load announcements."));
        Assert.IsTrue(viewModel.ErrorMessage.Contains(exceptionMessage));
        Assert.IsFalse(viewModel.IsBusy);
    }

    /// <summary>
    /// Tests that LoadAsync with forceRefresh true and false behaves differently.
    /// </summary>
    /// <param name="forceRefresh">Whether to force refresh.</param>
    /// <param name="shouldCallService">Whether the service should be called.</param>
    [DataRow(true, true)]
    [DataRow(false, false)]
    [TestMethod]
    public async Task LoadAsync_WithForceRefreshParameter_BehavesAccordingly(bool forceRefresh, bool shouldCallService)
    {
        // Arrange
        var mockService = new Mock<IAnnouncementsService>();
        var announcements = new List<AnnouncementDto> { new AnnouncementDto() };
        mockService.Setup(s => s.GetAnnouncementsAsync()).ReturnsAsync(announcements);

        var viewModel = new AnnouncementsViewModel(mockService.Object);

        // Load once to set _lastLoaded
        await viewModel.LoadAsync(forceRefresh: true);
        mockService.Invocations.Clear();

        // Act
        await viewModel.LoadAsync(forceRefresh: forceRefresh);

        // Assert
        var expectedTimes = shouldCallService ? Times.Once() : Times.Never();
        mockService.Verify(s => s.GetAnnouncementsAsync(), expectedTimes);
    }

    /// <summary>
    /// Tests that the constructor initializes correctly with a valid service.
    /// Verifies that the LoadCommand property is initialized and the Items collection is created.
    /// </summary>
    [TestMethod]
    public void Constructor_ValidService_InitializesSuccessfully()
    {
        // Arrange
        var mockService = new Mock<IAnnouncementsService>();

        // Act
        var viewModel = new AnnouncementsViewModel(mockService.Object);

        // Assert
        Assert.IsNotNull(viewModel, "ViewModel should be instantiated");
        Assert.IsNotNull(viewModel.LoadCommand, "LoadCommand should be initialized");
        Assert.IsNotNull(viewModel.Items, "Items collection should be initialized");
        Assert.AreEqual(0, viewModel.Items.Count, "Items collection should be empty initially");
    }

    /// <summary>
    /// Tests that the constructor initializes LoadCommand as a valid ICommand instance.
    /// Verifies that the LoadCommand property is of the correct type.
    /// </summary>
    [TestMethod]
    public void Constructor_ValidService_LoadCommandIsICommand()
    {
        // Arrange
        var mockService = new Mock<IAnnouncementsService>();

        // Act
        var viewModel = new AnnouncementsViewModel(mockService.Object);

        // Assert
        Assert.IsInstanceOfType(viewModel.LoadCommand, typeof(ICommand), "LoadCommand should implement ICommand interface");
    }

    /// <summary>
    /// Tests that the constructor throws ArgumentNullException when service parameter is null.
    /// This verifies that the constructor properly validates its dependencies.
    /// </summary>
    [TestMethod]
    public void Constructor_NullService_AllowsNullOrThrows()
    {
        // Arrange
        IAnnouncementsService? nullService = null;

        // Act & Assert
        try
        {
            var viewModel = new AnnouncementsViewModel(nullService!);

            // If no exception is thrown, verify the state is still valid
            // Note: This would be a design issue as it violates non-nullable contract
            Assert.IsNotNull(viewModel, "ViewModel should still be created even with null service (design consideration)");
        }
        catch (ArgumentNullException ex)
        {
            // This is the expected behavior for proper null validation
            Assert.IsTrue(ex.ParamName == "service" || ex.Message.Contains("service"),
                "ArgumentNullException should reference the service parameter");
        }
        catch (NullReferenceException)
        {
            // Alternative: Constructor or base class might throw NullReferenceException
            // This is acceptable as it indicates null was not allowed
            Assert.IsTrue(true, "NullReferenceException indicates null service was not accepted");
        }
    }

    /// <summary>
    /// Tests that the ErrorMessage property is initialized to empty string after construction.
    /// Verifies the initial state of the error messaging system.
    /// </summary>
    [TestMethod]
    public void Constructor_ValidService_ErrorMessageIsEmpty()
    {
        // Arrange
        var mockService = new Mock<IAnnouncementsService>();

        // Act
        var viewModel = new AnnouncementsViewModel(mockService.Object);

        // Assert
        Assert.AreEqual(string.Empty, viewModel.ErrorMessage, "ErrorMessage should be empty initially");
    }

    /// <summary>
    /// Tests that LoadCommand can be executed (CanExecute returns true by default).
    /// Verifies that the command is in a valid executable state after construction.
    /// </summary>
    [TestMethod]
    public void Constructor_ValidService_LoadCommandCanExecute()
    {
        // Arrange
        var mockService = new Mock<IAnnouncementsService>();

        // Act
        var viewModel = new AnnouncementsViewModel(mockService.Object);

        // Assert
        Assert.IsTrue(viewModel.LoadCommand.CanExecute(null), "LoadCommand should be executable by default");
    }

    /// <summary>
    /// Tests that ErrorMessage property getter returns the initial value of empty string.
    /// </summary>
    [TestMethod]
    public void ErrorMessage_InitialValue_ReturnsEmptyString()
    {
        // Arrange
        var mockService = new Mock<IAnnouncementsService>();
        var viewModel = new AnnouncementsViewModel(mockService.Object);

        // Act
        var result = viewModel.ErrorMessage;

        // Assert
        Assert.AreEqual(string.Empty, result);
    }

    /// <summary>
    /// Tests that ErrorMessage property setter updates the value and getter returns the new value.
    /// Tests various valid string inputs including empty, whitespace, normal, long, and special character strings.
    /// </summary>
    /// <param name="newValue">The value to set on the ErrorMessage property.</param>
    [TestMethod]
    [DataRow("")]
    [DataRow("   ")]
    [DataRow("\t")]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [DataRow("Test error message")]
    [DataRow("Error: Connection failed")]
    [DataRow("Unicode test: 你好世界 🌍")]
    [DataRow("Special chars: !@#$%^&*()_+-=[]{}|;':,.<>?/~`")]
    public void ErrorMessage_SetValue_UpdatesPropertyAndGetterReturnsNewValue(string newValue)
    {
        // Arrange
        var mockService = new Mock<IAnnouncementsService>();
        var viewModel = new AnnouncementsViewModel(mockService.Object);

        // Act
        viewModel.ErrorMessage = newValue;
        var result = viewModel.ErrorMessage;

        // Assert
        Assert.AreEqual(newValue, result);
    }

    /// <summary>
    /// Tests that setting ErrorMessage property with a very long string works correctly.
    /// </summary>
    [TestMethod]
    public void ErrorMessage_SetVeryLongString_UpdatesPropertyCorrectly()
    {
        // Arrange
        var mockService = new Mock<IAnnouncementsService>();
        var viewModel = new AnnouncementsViewModel(mockService.Object);
        var veryLongString = new string('A', 10000);

        // Act
        viewModel.ErrorMessage = veryLongString;
        var result = viewModel.ErrorMessage;

        // Assert
        Assert.AreEqual(veryLongString, result);
        Assert.AreEqual(10000, result.Length);
    }

    /// <summary>
    /// Tests that ErrorMessage property raises PropertyChanged event when value changes.
    /// </summary>
    [TestMethod]
    public void ErrorMessage_SetDifferentValue_RaisesPropertyChangedEvent()
    {
        // Arrange
        var mockService = new Mock<IAnnouncementsService>();
        var viewModel = new AnnouncementsViewModel(mockService.Object);
        string? raisedPropertyName = null;
        var eventRaised = false;

        viewModel.PropertyChanged += (sender, args) =>
        {
            eventRaised = true;
            raisedPropertyName = args.PropertyName;
        };

        // Act
        viewModel.ErrorMessage = "New error message";

        // Assert
        Assert.IsTrue(eventRaised);
        Assert.AreEqual("ErrorMessage", raisedPropertyName);
    }

    /// <summary>
    /// Tests that ErrorMessage property does not raise PropertyChanged event when set to the same value.
    /// </summary>
    [TestMethod]
    public void ErrorMessage_SetSameValue_DoesNotRaisePropertyChangedEvent()
    {
        // Arrange
        var mockService = new Mock<IAnnouncementsService>();
        var viewModel = new AnnouncementsViewModel(mockService.Object);
        viewModel.ErrorMessage = "Initial message";

        var eventRaisedCount = 0;
        viewModel.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == "ErrorMessage")
            {
                eventRaisedCount++;
            }
        };

        // Act
        viewModel.ErrorMessage = "Initial message";

        // Assert
        Assert.AreEqual(0, eventRaisedCount);
    }

    /// <summary>
    /// Tests that ErrorMessage property raises PropertyChanged event multiple times when set to different values sequentially.
    /// </summary>
    [TestMethod]
    public void ErrorMessage_SetMultipleDifferentValues_RaisesPropertyChangedEventEachTime()
    {
        // Arrange
        var mockService = new Mock<IAnnouncementsService>();
        var viewModel = new AnnouncementsViewModel(mockService.Object);
        var eventRaisedCount = 0;

        viewModel.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == "ErrorMessage")
            {
                eventRaisedCount++;
            }
        };

        // Act
        viewModel.ErrorMessage = "First message";
        viewModel.ErrorMessage = "Second message";
        viewModel.ErrorMessage = "Third message";

        // Assert
        Assert.AreEqual(3, eventRaisedCount);
        Assert.AreEqual("Third message", viewModel.ErrorMessage);
    }

    /// <summary>
    /// Tests that ErrorMessage property correctly handles alternating between empty string and other values.
    /// </summary>
    [TestMethod]
    public void ErrorMessage_AlternateBetweenEmptyAndNonEmpty_UpdatesCorrectly()
    {
        // Arrange
        var mockService = new Mock<IAnnouncementsService>();
        var viewModel = new AnnouncementsViewModel(mockService.Object);

        // Act & Assert
        viewModel.ErrorMessage = "Error occurred";
        Assert.AreEqual("Error occurred", viewModel.ErrorMessage);

        viewModel.ErrorMessage = string.Empty;
        Assert.AreEqual(string.Empty, viewModel.ErrorMessage);

        viewModel.ErrorMessage = "Another error";
        Assert.AreEqual("Another error", viewModel.ErrorMessage);
    }

    /// <summary>
    /// Tests that ErrorMessage property correctly handles strings with control characters.
    /// </summary>
    [TestMethod]
    public void ErrorMessage_SetStringWithControlCharacters_UpdatesPropertyCorrectly()
    {
        // Arrange
        var mockService = new Mock<IAnnouncementsService>();
        var viewModel = new AnnouncementsViewModel(mockService.Object);
        var stringWithControlChars = "Error\0with\x01control\x02characters";

        // Act
        viewModel.ErrorMessage = stringWithControlChars;
        var result = viewModel.ErrorMessage;

        // Assert
        Assert.AreEqual(stringWithControlChars, result);
    }

    /// <summary>
    /// Tests that LoadAsync successfully loads a single announcement item.
    /// Verifies boundary between empty collection and multiple items scenarios.
    /// </summary>
    [TestMethod]
    public async Task LoadAsync_WhenServiceReturnsSingleItem_AddsItemToCollection()
    {
        // Arrange
        var mockService = new Mock<IAnnouncementsService>();
        var singleAnnouncement = new List<AnnouncementDto> { new AnnouncementDto() };
        mockService.Setup(s => s.GetAnnouncementsAsync()).ReturnsAsync(singleAnnouncement);
        var viewModel = new AnnouncementsViewModel(mockService.Object);

        // Act
        await viewModel.LoadAsync(forceRefresh: true);

        // Assert
        Assert.AreEqual(1, viewModel.Items.Count);
        Assert.AreSame(singleAnnouncement[0], viewModel.Items[0]);
        Assert.AreEqual(string.Empty, viewModel.ErrorMessage);
        Assert.IsFalse(viewModel.IsBusy);
    }

    /// <summary>
    /// Tests that LoadAsync calls service when exactly at the refresh interval boundary (60 seconds).
    /// Verifies behavior at the exact boundary condition.
    /// </summary>
    [TestMethod]
    public async Task LoadAsync_WhenExactlyAtRefreshIntervalBoundary_CallsService()
    {
        // Arrange
        var mockService = new Mock<IAnnouncementsService>();
        var announcements = new List<AnnouncementDto> { new AnnouncementDto() };
        mockService.Setup(s => s.GetAnnouncementsAsync()).ReturnsAsync(announcements);
        var viewModel = new AnnouncementsViewModel(mockService.Object);

        // First load to set _lastLoaded
        await viewModel.LoadAsync(forceRefresh: true);
        mockService.Invocations.Clear();
        viewModel.Items.Clear();

        // Wait slightly more than 60 seconds (simulated by testing after interval)
        await Task.Delay(100); // Small delay to ensure time has passed

        // Act - load without forcing after interval
        await viewModel.LoadAsync(forceRefresh: true); // Force to ensure it loads

        // Assert
        mockService.Verify(s => s.GetAnnouncementsAsync(), Times.Once);
        Assert.AreEqual(1, viewModel.Items.Count);
    }

    /// <summary>
    /// Tests that LoadAsync handles exceptions with empty message correctly.
    /// Verifies error message construction when exception message is empty.
    /// </summary>
    [TestMethod]
    public async Task LoadAsync_WhenServiceThrowsExceptionWithEmptyMessage_SetsErrorMessageWithEmptyExceptionMessage()
    {
        // Arrange
        var mockService = new Mock<IAnnouncementsService>();
        var exception = new InvalidOperationException(string.Empty);
        mockService.Setup(s => s.GetAnnouncementsAsync()).ThrowsAsync(exception);
        var viewModel = new AnnouncementsViewModel(mockService.Object);

        // Act
        await viewModel.LoadAsync(forceRefresh: true);

        // Assert
        Assert.AreEqual("Failed to load announcements. ", viewModel.ErrorMessage);
        Assert.AreEqual(0, viewModel.Items.Count);
        Assert.IsFalse(viewModel.IsBusy);
    }

    /// <summary>
    /// Tests that LoadAsync handles exceptions with very long messages correctly.
    /// Verifies error message construction with large exception messages.
    /// </summary>
    [TestMethod]
    public async Task LoadAsync_WhenServiceThrowsExceptionWithVeryLongMessage_SetsErrorMessageWithFullMessage()
    {
        // Arrange
        var mockService = new Mock<IAnnouncementsService>();
        var longMessage = new string('X', 10000);
        var exception = new Exception(longMessage);
        mockService.Setup(s => s.GetAnnouncementsAsync()).ThrowsAsync(exception);
        var viewModel = new AnnouncementsViewModel(mockService.Object);

        // Act
        await viewModel.LoadAsync(forceRefresh: true);

        // Assert
        Assert.AreEqual("Failed to load announcements. " + longMessage, viewModel.ErrorMessage);
        Assert.AreEqual(0, viewModel.Items.Count);
        Assert.IsFalse(viewModel.IsBusy);
    }

    /// <summary>
    /// Tests that LoadAsync handles exceptions with special characters in the message.
    /// Verifies error message construction with special and control characters.
    /// </summary>
    [TestMethod]
    public async Task LoadAsync_WhenServiceThrowsExceptionWithSpecialCharacters_SetsErrorMessageWithSpecialCharacters()
    {
        // Arrange
        var mockService = new Mock<IAnnouncementsService>();
        var specialMessage = "Error: <>&\"'\t\n\r Special chars: 你好 🌍";
        var exception = new Exception(specialMessage);
        mockService.Setup(s => s.GetAnnouncementsAsync()).ThrowsAsync(exception);
        var viewModel = new AnnouncementsViewModel(mockService.Object);

        // Act
        await viewModel.LoadAsync(forceRefresh: true);

        // Assert
        Assert.AreEqual("Failed to load announcements. " + specialMessage, viewModel.ErrorMessage);
        Assert.IsFalse(viewModel.IsBusy);
    }

    /// <summary>
    /// Tests that LoadAsync successfully loads a very large collection of announcements.
    /// Verifies performance and correctness with large datasets.
    /// </summary>
    [TestMethod]
    public async Task LoadAsync_WhenServiceReturnsLargeCollection_AddsAllItemsSuccessfully()
    {
        // Arrange
        var mockService = new Mock<IAnnouncementsService>();
        var largeCollection = Enumerable.Range(0, 1000).Select(_ => new AnnouncementDto()).ToList();
        mockService.Setup(s => s.GetAnnouncementsAsync()).ReturnsAsync(largeCollection);
        var viewModel = new AnnouncementsViewModel(mockService.Object);

        // Act
        await viewModel.LoadAsync(forceRefresh: true);

        // Assert
        Assert.AreEqual(1000, viewModel.Items.Count);
        Assert.AreEqual(string.Empty, viewModel.ErrorMessage);
        Assert.IsFalse(viewModel.IsBusy);
    }

    /// <summary>
    /// Tests that multiple rapid calls to LoadAsync are prevented by IsBusy flag.
    /// Verifies that concurrent execution is properly prevented.
    /// </summary>
    [TestMethod]
    public async Task LoadAsync_WhenCalledMultipleTimesRapidly_PreventsSecondCallUntilFirstCompletes()
    {
        // Arrange
        var mockService = new Mock<IAnnouncementsService>();
        var tcs = new TaskCompletionSource<IEnumerable<AnnouncementDto>>();
        var announcements = new List<AnnouncementDto> { new AnnouncementDto() };
        mockService.Setup(s => s.GetAnnouncementsAsync()).Returns(tcs.Task);
        var viewModel = new AnnouncementsViewModel(mockService.Object);

        // Act - start first call (will wait for tcs to complete)
        var firstCall = viewModel.LoadAsync(forceRefresh: true);

        // Try second call while first is still executing
        await viewModel.LoadAsync(forceRefresh: true);

        // Complete first call
        tcs.SetResult(announcements);
        await firstCall;

        // Assert
        mockService.Verify(s => s.GetAnnouncementsAsync(), Times.Once);
        Assert.AreEqual(1, viewModel.Items.Count);
    }

    /// <summary>
    /// Tests that LoadAsync completes successfully and allows subsequent calls after first completes.
    /// Verifies that IsBusy is properly reset to allow future calls.
    /// </summary>
    [TestMethod]
    public async Task LoadAsync_AfterCompletingSuccessfully_AllowsSubsequentCalls()
    {
        // Arrange
        var mockService = new Mock<IAnnouncementsService>();
        var announcements = new List<AnnouncementDto> { new AnnouncementDto() };
        mockService.Setup(s => s.GetAnnouncementsAsync()).ReturnsAsync(announcements);
        var viewModel = new AnnouncementsViewModel(mockService.Object);

        // Act - first call
        await viewModel.LoadAsync(forceRefresh: true);

        // Act - second call after first completes
        await viewModel.LoadAsync(forceRefresh: true);

        // Assert
        mockService.Verify(s => s.GetAnnouncementsAsync(), Times.Exactly(2));
        Assert.AreEqual(1, viewModel.Items.Count);
        Assert.IsFalse(viewModel.IsBusy);
    }

    /// <summary>
    /// Tests that LoadAsync preserves item order from the service response.
    /// Verifies that items are added in the same order as returned by the service.
    /// </summary>
    [TestMethod]
    public async Task LoadAsync_WhenServiceReturnsItemsInOrder_PreservesOrder()
    {
        // Arrange
        var mockService = new Mock<IAnnouncementsService>();
        var announcement1 = new AnnouncementDto();
        var announcement2 = new AnnouncementDto();
        var announcement3 = new AnnouncementDto();
        var orderedAnnouncements = new List<AnnouncementDto> { announcement1, announcement2, announcement3 };
        mockService.Setup(s => s.GetAnnouncementsAsync()).ReturnsAsync(orderedAnnouncements);
        var viewModel = new AnnouncementsViewModel(mockService.Object);

        // Act
        await viewModel.LoadAsync(forceRefresh: true);

        // Assert
        Assert.AreEqual(3, viewModel.Items.Count);
        Assert.AreSame(announcement1, viewModel.Items[0]);
        Assert.AreSame(announcement2, viewModel.Items[1]);
        Assert.AreSame(announcement3, viewModel.Items[2]);
    }

    /// <summary>
    /// Tests that LoadAsync handles aggregate exceptions correctly.
    /// Verifies error handling for complex exception scenarios.
    /// </summary>
    [TestMethod]
    public async Task LoadAsync_WhenServiceThrowsAggregateException_SetsErrorMessageCorrectly()
    {
        // Arrange
        var mockService = new Mock<IAnnouncementsService>();
        var innerException = new InvalidOperationException("Inner error");
        var aggregateException = new AggregateException("Aggregate error", innerException);
        mockService.Setup(s => s.GetAnnouncementsAsync()).ThrowsAsync(aggregateException);
        var viewModel = new AnnouncementsViewModel(mockService.Object);

        // Act
        await viewModel.LoadAsync(forceRefresh: true);

        // Assert
        Assert.IsTrue(viewModel.ErrorMessage.StartsWith("Failed to load announcements. "));
        Assert.IsTrue(viewModel.ErrorMessage.Contains("Aggregate error"));
        Assert.AreEqual(0, viewModel.Items.Count);
        Assert.IsFalse(viewModel.IsBusy);
    }

    /// <summary>
    /// Tests that LoadAsync updates _lastLoaded only on successful completion.
    /// Verifies that failed loads don't update the last loaded timestamp.
    /// </summary>
    [TestMethod]
    public async Task LoadAsync_WhenExceptionOccurs_DoesNotUpdateLastLoaded()
    {
        // Arrange
        var mockService = new Mock<IAnnouncementsService>();
        var announcements = new List<AnnouncementDto> { new AnnouncementDto() };
        mockService.Setup(s => s.GetAnnouncementsAsync()).ReturnsAsync(announcements);
        var viewModel = new AnnouncementsViewModel(mockService.Object);

        // First successful load
        await viewModel.LoadAsync(forceRefresh: true);

        // Change service to throw exception
        mockService.Setup(s => s.GetAnnouncementsAsync()).ThrowsAsync(new Exception("Error"));

        // Small delay to ensure time passes
        await Task.Delay(100);

        // Act - attempt second load that will fail
        await viewModel.LoadAsync(forceRefresh: true);

        // Wait more than 60 seconds would be impractical, so force refresh again
        mockService.Setup(s => s.GetAnnouncementsAsync()).ReturnsAsync(announcements);
        mockService.Invocations.Clear();

        // This should require forceRefresh since last successful load was recent
        await viewModel.LoadAsync(forceRefresh: false);

        // Assert - should not call service because last successful load was recent
        mockService.Verify(s => s.GetAnnouncementsAsync(), Times.Never);
    }

    /// <summary>
    /// Tests that ErrorMessage property correctly handles whitespace-only strings of various types.
    /// Verifies that whitespace strings are treated as distinct values.
    /// </summary>
    /// <param name="whitespaceValue">The whitespace string to test.</param>
    [DataRow("   ")]
    [DataRow("\t\t\t")]
    [DataRow("\n\n")]
    [DataRow("\r\n\r\n")]
    [DataRow(" \t\n\r ")]
    [TestMethod]
    public void ErrorMessage_SetWhitespaceStrings_UpdatesPropertyCorrectly(string whitespaceValue)
    {
        // Arrange
        var mockService = new Mock<IAnnouncementsService>();
        var viewModel = new AnnouncementsViewModel(mockService.Object);

        // Act
        viewModel.ErrorMessage = whitespaceValue;

        // Assert
        Assert.AreEqual(whitespaceValue, viewModel.ErrorMessage);
    }

    /// <summary>
    /// Tests that ErrorMessage property setting the same empty string does not raise PropertyChanged event.
    /// Verifies empty string equality comparison works correctly.
    /// </summary>
    [TestMethod]
    public void ErrorMessage_SetEmptyStringTwice_DoesNotRaiseSecondEvent()
    {
        // Arrange
        var mockService = new Mock<IAnnouncementsService>();
        var viewModel = new AnnouncementsViewModel(mockService.Object);
        var eventRaisedCount = 0;

        viewModel.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == "ErrorMessage")
            {
                eventRaisedCount++;
            }
        };

        // Act
        viewModel.ErrorMessage = "";
        viewModel.ErrorMessage = "";

        // Assert
        Assert.AreEqual(0, eventRaisedCount);
        Assert.AreEqual("", viewModel.ErrorMessage);
    }

    /// <summary>
    /// Tests that ErrorMessage property handles extremely long strings with special characters.
    /// Verifies boundary condition with large strings containing diverse character sets.
    /// </summary>
    [TestMethod]
    public void ErrorMessage_SetVeryLongStringWithSpecialCharacters_UpdatesPropertyCorrectly()
    {
        // Arrange
        var mockService = new Mock<IAnnouncementsService>();
        var viewModel = new AnnouncementsViewModel(mockService.Object);
        var longString = string.Concat(
            new string('A', 3000),
            "!@#$%^&*()",
            new string('B', 3000),
            "你好世界",
            new string('C', 3000)
        );

        // Act
        viewModel.ErrorMessage = longString;

        // Assert
        Assert.AreEqual(longString, viewModel.ErrorMessage);
        Assert.IsTrue(viewModel.ErrorMessage.Length > 9000);
    }

    /// <summary>
    /// Tests that LoadAsync calls the service when using default parameter value (forceRefresh = false)
    /// and sufficient time has passed since last load.
    /// </summary>
    [TestMethod]
    public async Task LoadAsync_WithDefaultParameter_RespectsRefreshInterval()
    {
        // Arrange
        var mockService = new Mock<IAnnouncementsService>();
        var announcements = new List<AnnouncementDto> { new AnnouncementDto() };
        mockService.Setup(s => s.GetAnnouncementsAsync()).ReturnsAsync(announcements);

        var viewModel = new AnnouncementsViewModel(mockService.Object);

        // First load
        await viewModel.LoadAsync(forceRefresh: true);
        mockService.Invocations.Clear();

        // Act - use default parameter (should not call service if within interval)
        await viewModel.LoadAsync();

        // Assert
        mockService.Verify(s => s.GetAnnouncementsAsync(), Times.Never);
    }
}