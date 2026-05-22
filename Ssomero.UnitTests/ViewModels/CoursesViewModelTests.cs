using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

using Microsoft.Maui.ApplicationModel.DataTransfer;
using Microsoft.Maui.Controls;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Ssomero.Interfaces;
using Ssomero.Models;
using Ssomero.ViewModels;

namespace Ssomero.ViewModels.UnitTests;


/// <summary>
/// Unit tests for the CoursesViewModel class.
/// </summary>
[TestClass]
public class CoursesViewModelTests
{
    /// <summary>
    /// Tests that setting ErrorMessage property updates the value correctly for various string inputs.
    /// </summary>
    /// <param name = "testValue">The string value to set on ErrorMessage property.</param>
    [TestMethod]
    [DataRow("Error occurred")]
    [DataRow("")]
    [DataRow("   ")]
    [DataRow("Error with special characters: !@#$%^&*()")]
    [DataRow("Very long error message that contains many characters to test the behavior with lengthy strings that might occur in real-world scenarios where error messages can be quite detailed and extensive")]
    public void ErrorMessage_SetValue_UpdatesProperty(string testValue)
    {
        // Arrange
        var mockCoursesService = new Mock<ICoursesService>();
        var viewModel = new CoursesViewModel(mockCoursesService.Object);
        // Act
        viewModel.ErrorMessage = testValue;
        // Assert
        Assert.AreEqual(testValue, viewModel.ErrorMessage);
    }

    /// <summary>
    /// Tests that setting ErrorMessage property raises PropertyChanged event with correct property name.
    /// </summary>
    [TestMethod]
    public void ErrorMessage_SetNewValue_RaisesPropertyChangedEvent()
    {
        // Arrange
        var mockCoursesService = new Mock<ICoursesService>();
        var viewModel = new CoursesViewModel(mockCoursesService.Object);
        string? raisedPropertyName = null;
        viewModel.PropertyChanged += (sender, args) => raisedPropertyName = args.PropertyName;
        // Act
        viewModel.ErrorMessage = "Test error";
        // Assert
        Assert.IsNotNull(raisedPropertyName);
        Assert.AreEqual("ErrorMessage", raisedPropertyName);
    }

    /// <summary>
    /// Tests that setting ErrorMessage to the same value does not raise PropertyChanged event.
    /// </summary>
    [TestMethod]
    public void ErrorMessage_SetSameValue_DoesNotRaisePropertyChangedEvent()
    {
        // Arrange
        var mockCoursesService = new Mock<ICoursesService>();
        var viewModel = new CoursesViewModel(mockCoursesService.Object);
        viewModel.ErrorMessage = "Initial error";
        var eventRaisedCount = 0;
        viewModel.PropertyChanged += (sender, args) => eventRaisedCount++;
        // Act
        viewModel.ErrorMessage = "Initial error";
        // Assert
        Assert.AreEqual(0, eventRaisedCount);
    }

    /// <summary>
    /// Tests that getting ErrorMessage returns the initial default value (empty string).
    /// </summary>
    [TestMethod]
    public void ErrorMessage_GetInitialValue_ReturnsEmptyString()
    {
        // Arrange
        var mockCoursesService = new Mock<ICoursesService>();
        var viewModel = new CoursesViewModel(mockCoursesService.Object);
        // Act
        var result = viewModel.ErrorMessage;
        // Assert
        Assert.AreEqual(string.Empty, result);
    }

    /// <summary>
    /// Tests that setting ErrorMessage multiple times with different values raises PropertyChanged for each change.
    /// </summary>
    [TestMethod]
    public void ErrorMessage_SetMultipleDifferentValues_RaisesPropertyChangedForEach()
    {
        // Arrange
        var mockCoursesService = new Mock<ICoursesService>();
        var viewModel = new CoursesViewModel(mockCoursesService.Object);
        var eventRaisedCount = 0;
        viewModel.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == "ErrorMessage")
            {
                eventRaisedCount++;
            }
        };
        // Act
        viewModel.ErrorMessage = "Error 1";
        viewModel.ErrorMessage = "Error 2";
        viewModel.ErrorMessage = "Error 3";
        // Assert
        Assert.AreEqual(3, eventRaisedCount);
        Assert.AreEqual("Error 3", viewModel.ErrorMessage);
    }

    /// <summary>
    /// Tests that setting ErrorMessage from non-empty to empty string raises PropertyChanged event.
    /// </summary>
    [TestMethod]
    public void ErrorMessage_SetFromNonEmptyToEmpty_RaisesPropertyChangedEvent()
    {
        // Arrange
        var mockCoursesService = new Mock<ICoursesService>();
        var viewModel = new CoursesViewModel(mockCoursesService.Object);
        viewModel.ErrorMessage = "Some error";
        var eventRaised = false;
        viewModel.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == "ErrorMessage")
            {
                eventRaised = true;
            }
        };
        // Act
        viewModel.ErrorMessage = string.Empty;
        // Assert
        Assert.IsTrue(eventRaised);
        Assert.AreEqual(string.Empty, viewModel.ErrorMessage);
    }

    /// <summary>
    /// Tests that setting ErrorMessage with whitespace-only string correctly updates the value.
    /// </summary>
    [TestMethod]
    public void ErrorMessage_SetWhitespaceOnly_UpdatesPropertyCorrectly()
    {
        // Arrange
        var mockCoursesService = new Mock<ICoursesService>();
        var viewModel = new CoursesViewModel(mockCoursesService.Object);
        var whitespaceValue = "   \t\n   ";
        // Act
        viewModel.ErrorMessage = whitespaceValue;
        // Assert
        Assert.AreEqual(whitespaceValue, viewModel.ErrorMessage);
    }

    /// <summary>
    /// Tests that the constructor initializes the view model with a valid courses service
    /// without throwing any exceptions and properly initializes all commands.
    /// </summary>
    [TestMethod]
    public void CoursesViewModel_ValidCoursesService_InitializesSuccessfully()
    {
        // Arrange
        Mock<ICoursesService> mockCoursesService = new Mock<ICoursesService>();
        // Act
        CoursesViewModel viewModel = new CoursesViewModel(mockCoursesService.Object);
        // Assert
        Assert.IsNotNull(viewModel);
        Assert.IsNotNull(viewModel.LoadCommand);
        Assert.IsNotNull(viewModel.OpenCourseCommand);
        Assert.IsNotNull(viewModel.Items);
        Assert.AreEqual(0, viewModel.Items.Count);
    }

    /// <summary>
    /// Tests that the LoadCommand property is properly initialized as an ICommand instance
    /// when the constructor is called with a valid courses service.
    /// </summary>
    [TestMethod]
    public void CoursesViewModel_ValidCoursesService_LoadCommandIsICommand()
    {
        // Arrange
        Mock<ICoursesService> mockCoursesService = new Mock<ICoursesService>();
        // Act
        CoursesViewModel viewModel = new CoursesViewModel(mockCoursesService.Object);
        // Assert
        Assert.IsInstanceOfType(viewModel.LoadCommand, typeof(ICommand));
    }

    /// <summary>
    /// Tests that the OpenCourseCommand property is properly initialized as an ICommand instance
    /// when the constructor is called with a valid courses service.
    /// </summary>
    [TestMethod]
    public void CoursesViewModel_ValidCoursesService_OpenCourseCommandIsICommand()
    {
        // Arrange
        Mock<ICoursesService> mockCoursesService = new Mock<ICoursesService>();
        // Act
        CoursesViewModel viewModel = new CoursesViewModel(mockCoursesService.Object);
        // Assert
        Assert.IsInstanceOfType(viewModel.OpenCourseCommand, typeof(ICommand));
    }

    /// <summary>
    /// Tests that the constructor accepts a null courses service parameter.
    /// This verifies the behavior when nullability contract is violated,
    /// as the constructor does not perform explicit null validation.
    /// </summary>
    [TestMethod]
    public void CoursesViewModel_NullCoursesService_DoesNotThrowException()
    {
        // Arrange
        ICoursesService? nullService = null;
        // Act & Assert - Constructor should not throw for null input
        CoursesViewModel viewModel = new CoursesViewModel(nullService!);
        Assert.IsNotNull(viewModel);
        Assert.IsNotNull(viewModel.LoadCommand);
        Assert.IsNotNull(viewModel.OpenCourseCommand);
    }

    /// <summary>
    /// Tests that Items collection is initialized and empty after constructor execution
    /// with a valid courses service.
    /// </summary>
    [TestMethod]
    public void CoursesViewModel_ValidCoursesService_ItemsCollectionIsEmptyAndNotNull()
    {
        // Arrange
        Mock<ICoursesService> mockCoursesService = new Mock<ICoursesService>();
        // Act
        CoursesViewModel viewModel = new CoursesViewModel(mockCoursesService.Object);
        // Assert
        Assert.IsNotNull(viewModel.Items);
        Assert.AreEqual(0, viewModel.Items.Count);
    }

    /// <summary>
    /// Tests that ErrorMessage property is initialized with an empty string after constructor execution
    /// with a valid courses service.
    /// </summary>
    [TestMethod]
    public void CoursesViewModel_ValidCoursesService_ErrorMessageIsEmptyString()
    {
        // Arrange
        Mock<ICoursesService> mockCoursesService = new Mock<ICoursesService>();
        // Act
        CoursesViewModel viewModel = new CoursesViewModel(mockCoursesService.Object);
        // Assert
        Assert.AreEqual(string.Empty, viewModel.ErrorMessage);
    }

    /// <summary>
    /// Tests that LoadAsync returns immediately without loading when IsBusy is already true.
    /// </summary>
    [TestMethod]
    public async Task LoadAsync_WhenIsBusyIsTrue_ReturnsImmediatelyWithoutLoading()
    {
        // Arrange
        var mockCoursesService = new Mock<ICoursesService>();
        var viewModel = new CoursesViewModel(mockCoursesService.Object);
        viewModel.IsBusy = true;
        // Act
        await viewModel.LoadAsync(forceRefresh: false);
        // Assert
        mockCoursesService.Verify(s => s.GetCoursesAsync(), Times.Never);
        Assert.AreEqual(0, viewModel.Items.Count);
    }

    /// <summary>
    /// Tests that LoadAsync clears ErrorMessage at the start of execution.
    /// </summary>
    [TestMethod]
    public async Task LoadAsync_ClearsErrorMessageAtStart()
    {
        // Arrange
        var mockCoursesService = new Mock<ICoursesService>();
        mockCoursesService.SetupSequence(s => s.GetCoursesAsync()).ThrowsAsync(new Exception("First error")).ReturnsAsync(new List<CourseDto>());
        var viewModel = new CoursesViewModel(mockCoursesService.Object);
        // Act - first call should set error message
        await viewModel.LoadAsync(forceRefresh: true);
        Assert.IsFalse(string.IsNullOrEmpty(viewModel.ErrorMessage));
        // Act - second call should clear error message
        await viewModel.LoadAsync(forceRefresh: true);
        // Assert
        Assert.AreEqual(string.Empty, viewModel.ErrorMessage);
    }

    /// <summary>
    /// Tests that LoadAsync catches exceptions and sets appropriate error message.
    /// </summary>
    [TestMethod]
    [DataRow("Network error", DisplayName = "Network error")]
    [DataRow("Service unavailable", DisplayName = "Service unavailable")]
    [DataRow("", DisplayName = "Empty exception message")]
    public async Task LoadAsync_WhenServiceThrowsException_SetsErrorMessage(string exceptionMessage)
    {
        // Arrange
        var mockCoursesService = new Mock<ICoursesService>();
        var exception = new Exception(exceptionMessage);
        mockCoursesService.Setup(s => s.GetCoursesAsync()).ThrowsAsync(exception);
        var viewModel = new CoursesViewModel(mockCoursesService.Object);
        // Act
        await viewModel.LoadAsync(forceRefresh: true);
        // Assert
        Assert.AreEqual($"Failed to load courses. {exceptionMessage}", viewModel.ErrorMessage);
        Assert.AreEqual(0, viewModel.Items.Count);
    }

    /// <summary>
    /// Tests that LoadAsync sets IsBusy to false even when an exception occurs.
    /// </summary>
    [TestMethod]
    public async Task LoadAsync_WhenExceptionOccurs_SetsIsBusyToFalseInFinally()
    {
        // Arrange
        var mockCoursesService = new Mock<ICoursesService>();
        mockCoursesService.Setup(s => s.GetCoursesAsync()).ThrowsAsync(new Exception("Test error"));
        var viewModel = new CoursesViewModel(mockCoursesService.Object);
        // Act
        await viewModel.LoadAsync(forceRefresh: true);
        // Assert
        Assert.IsFalse(viewModel.IsBusy);
    }

    /// <summary>
    /// Tests that LoadAsync does not throw exception when service throws, handling it gracefully.
    /// </summary>
    [TestMethod]
    public async Task LoadAsync_WhenServiceThrowsException_DoesNotPropagateException()
    {
        // Arrange
        var mockCoursesService = new Mock<ICoursesService>();
        mockCoursesService.Setup(s => s.GetCoursesAsync()).ThrowsAsync(new InvalidOperationException("Service error"));
        var viewModel = new CoursesViewModel(mockCoursesService.Object);
        // Act & Assert - should not throw
        await viewModel.LoadAsync(forceRefresh: true);
        Assert.IsFalse(string.IsNullOrEmpty(viewModel.ErrorMessage));
    }

    /// <summary>
    /// Tests that LoadAsync handles null reference in exception message gracefully.
    /// </summary>
    [TestMethod]
    public async Task LoadAsync_WhenExceptionMessageIsNull_HandlesGracefully()
    {
        // Arrange
        var mockCoursesService = new Mock<ICoursesService>();
        var exception = new Exception();
        mockCoursesService.Setup(s => s.GetCoursesAsync()).ThrowsAsync(exception);
        var viewModel = new CoursesViewModel(mockCoursesService.Object);
        // Act
        await viewModel.LoadAsync(forceRefresh: true);
        // Assert
        Assert.IsTrue(viewModel.ErrorMessage.StartsWith("Failed to load courses."));
    }


    /// <summary>
    /// Tests that setting ErrorMessage from empty string to non-empty string raises PropertyChanged event.
    /// </summary>
    [TestMethod]
    public void ErrorMessage_SetFromEmptyToNonEmpty_RaisesPropertyChangedEvent()
    {
        // Arrange
        var mockCoursesService = new Mock<ICoursesService>();
        var viewModel = new CoursesViewModel(mockCoursesService.Object);
        string? raisedPropertyName = null;
        viewModel.PropertyChanged += (sender, args) => raisedPropertyName = args.PropertyName;

        // Act
        viewModel.ErrorMessage = "New error";

        // Assert
        Assert.IsNotNull(raisedPropertyName);
        Assert.AreEqual("ErrorMessage", raisedPropertyName);
        Assert.AreEqual("New error", viewModel.ErrorMessage);
    }

    /// <summary>
    /// Tests that ErrorMessage getter returns the same value that was set.
    /// </summary>
    [TestMethod]
    public void ErrorMessage_GetAfterSet_ReturnsSameValue()
    {
        // Arrange
        var mockCoursesService = new Mock<ICoursesService>();
        var viewModel = new CoursesViewModel(mockCoursesService.Object);
        var expectedValue = "Specific error message";

        // Act
        viewModel.ErrorMessage = expectedValue;
        var actualValue = viewModel.ErrorMessage;

        // Assert
        Assert.AreEqual(expectedValue, actualValue);
    }

    /// <summary>
    /// Tests that PropertyChanged event includes the sender as the viewModel instance.
    /// </summary>
    [TestMethod]
    public void ErrorMessage_SetValue_PropertyChangedEventSenderIsViewModel()
    {
        // Arrange
        var mockCoursesService = new Mock<ICoursesService>();
        var viewModel = new CoursesViewModel(mockCoursesService.Object);
        object? eventSender = null;
        viewModel.PropertyChanged += (sender, args) => eventSender = sender;

        // Act
        viewModel.ErrorMessage = "Test error";

        // Assert
        Assert.IsNotNull(eventSender);
        Assert.AreSame(viewModel, eventSender);
    }

    /// <summary>
    /// Tests that setting ErrorMessage to same empty string multiple times does not raise PropertyChanged.
    /// </summary>
    [TestMethod]
    public void ErrorMessage_SetEmptyStringMultipleTimes_DoesNotRaisePropertyChanged()
    {
        // Arrange
        var mockCoursesService = new Mock<ICoursesService>();
        var viewModel = new CoursesViewModel(mockCoursesService.Object);
        var eventRaisedCount = 0;
        viewModel.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == "ErrorMessage")
            {
                eventRaisedCount++;
            }
        };

        // Act
        viewModel.ErrorMessage = string.Empty;
        viewModel.ErrorMessage = string.Empty;
        viewModel.ErrorMessage = string.Empty;

        // Assert
        Assert.AreEqual(0, eventRaisedCount);
    }

    /// <summary>
    /// Tests that alternating between two different values raises PropertyChanged for each change.
    /// </summary>
    [TestMethod]
    public void ErrorMessage_AlternateBetweenTwoValues_RaisesPropertyChangedForEachChange()
    {
        // Arrange
        var mockCoursesService = new Mock<ICoursesService>();
        var viewModel = new CoursesViewModel(mockCoursesService.Object);
        var eventRaisedCount = 0;
        viewModel.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == "ErrorMessage")
            {
                eventRaisedCount++;
            }
        };

        // Act
        viewModel.ErrorMessage = "Error A";
        viewModel.ErrorMessage = "Error B";
        viewModel.ErrorMessage = "Error A";
        viewModel.ErrorMessage = "Error B";

        // Assert
        Assert.AreEqual(4, eventRaisedCount);
        Assert.AreEqual("Error B", viewModel.ErrorMessage);
    }

    /// <summary>
    /// Tests that LoadAsync with empty course list clears Items and completes successfully.
    /// </summary>
    [TestMethod]
    public async Task LoadAsync_EmptyCourseList_ClearsItemsSuccessfully()
    {
        // Arrange
        var mockCoursesService = new Mock<ICoursesService>();
        mockCoursesService.Setup(s => s.GetCoursesAsync()).ReturnsAsync(new List<CourseDto>());
        var viewModel = new CoursesViewModel(mockCoursesService.Object);

        // Act
        await viewModel.LoadAsync(forceRefresh: true);

        // Assert
        Assert.AreEqual(0, viewModel.Items.Count);
        Assert.AreEqual(string.Empty, viewModel.ErrorMessage);
        Assert.IsFalse(viewModel.IsBusy);
    }

    /// <summary>
    /// Tests that LoadAsync sets IsBusy to true during execution and false after completion.
    /// </summary>
    [TestMethod]
    public async Task LoadAsync_DuringExecution_SetsIsBusyCorrectly()
    {
        // Arrange
        var mockCoursesService = new Mock<ICoursesService>();
        bool isBusyDuringExecution = false;
        mockCoursesService.Setup(s => s.GetCoursesAsync())
            .ReturnsAsync(() =>
            {
                return new List<CourseDto>();
            })
            .Callback(() => isBusyDuringExecution = true);
        var viewModel = new CoursesViewModel(mockCoursesService.Object);

        // Act
        var loadTask = viewModel.LoadAsync(forceRefresh: true);
        await loadTask;

        // Assert
        Assert.IsFalse(viewModel.IsBusy);
    }

    /// <summary>
    /// Tests that LoadAsync with various exception types sets appropriate error messages.
    /// </summary>
    /// <param name="exceptionMessage">The exception message to test.</param>
    [TestMethod]
    [DataRow("Connection timeout", DisplayName = "Connection timeout")]
    [DataRow("Invalid credentials", DisplayName = "Invalid credentials")]
    [DataRow("Database error", DisplayName = "Database error")]
    [DataRow("!@#$%^&*()", DisplayName = "Special characters")]
    [DataRow("Very long error message that contains extensive details about what went wrong in the system during the operation that failed unexpectedly", DisplayName = "Very long message")]
    public async Task LoadAsync_ServiceThrowsExceptionWithMessage_SetsCorrectErrorMessage(string exceptionMessage)
    {
        // Arrange
        var mockCoursesService = new Mock<ICoursesService>();
        mockCoursesService.Setup(s => s.GetCoursesAsync()).ThrowsAsync(new Exception(exceptionMessage));
        var viewModel = new CoursesViewModel(mockCoursesService.Object);

        // Act
        await viewModel.LoadAsync(forceRefresh: true);

        // Assert
        Assert.AreEqual($"Failed to load courses. {exceptionMessage}", viewModel.ErrorMessage);
        Assert.IsFalse(viewModel.IsBusy);
    }

    /// <summary>
    /// Tests that LoadAsync with different exception types handles them appropriately.
    /// </summary>
    [TestMethod]
    public async Task LoadAsync_ServiceThrowsInvalidOperationException_HandlesGracefully()
    {
        // Arrange
        var mockCoursesService = new Mock<ICoursesService>();
        var exception = new InvalidOperationException("Invalid operation occurred");
        mockCoursesService.Setup(s => s.GetCoursesAsync()).ThrowsAsync(exception);
        var viewModel = new CoursesViewModel(mockCoursesService.Object);

        // Act
        await viewModel.LoadAsync(forceRefresh: true);

        // Assert
        Assert.AreEqual("Failed to load courses. Invalid operation occurred", viewModel.ErrorMessage);
        Assert.AreEqual(0, viewModel.Items.Count);
        Assert.IsFalse(viewModel.IsBusy);
    }

    /// <summary>
    /// Tests that LoadAsync with ArgumentNullException handles it appropriately.
    /// </summary>
    [TestMethod]
    public async Task LoadAsync_ServiceThrowsArgumentNullException_HandlesGracefully()
    {
        // Arrange
        var mockCoursesService = new Mock<ICoursesService>();
        var exception = new ArgumentNullException("parameter", "Parameter cannot be null");
        mockCoursesService.Setup(s => s.GetCoursesAsync()).ThrowsAsync(exception);
        var viewModel = new CoursesViewModel(mockCoursesService.Object);

        // Act
        await viewModel.LoadAsync(forceRefresh: true);

        // Assert
        Assert.IsTrue(viewModel.ErrorMessage.StartsWith("Failed to load courses."));
        Assert.IsFalse(viewModel.IsBusy);
    }

    /// <summary>
    /// Tests that LoadAsync maintains Items empty when exception occurs.
    /// </summary>
    [TestMethod]
    public async Task LoadAsync_ExceptionDuringLoad_KeepsItemsEmpty()
    {
        // Arrange
        var mockCoursesService = new Mock<ICoursesService>();
        mockCoursesService.Setup(s => s.GetCoursesAsync()).ThrowsAsync(new Exception("Error"));
        var viewModel = new CoursesViewModel(mockCoursesService.Object);

        // Act
        await viewModel.LoadAsync(forceRefresh: true);

        // Assert
        Assert.AreEqual(0, viewModel.Items.Count);
    }

    /// <summary>
    /// Tests that LoadAsync with whitespace-only exception message handles it correctly.
    /// </summary>
    [TestMethod]
    public async Task LoadAsync_ExceptionWithWhitespaceMessage_SetsErrorMessageCorrectly()
    {
        // Arrange
        var mockCoursesService = new Mock<ICoursesService>();
        mockCoursesService.Setup(s => s.GetCoursesAsync()).ThrowsAsync(new Exception("   "));
        var viewModel = new CoursesViewModel(mockCoursesService.Object);

        // Act
        await viewModel.LoadAsync(forceRefresh: true);

        // Assert
        Assert.AreEqual("Failed to load courses.    ", viewModel.ErrorMessage);
    }

    /// <summary>
    /// Tests that LoadAsync does not call service when forceRefresh is false and last load was recent.
    /// </summary>
    [TestMethod]
    public async Task LoadAsync_ForceRefreshFalseRecentLoad_DoesNotCallService()
    {
        // Arrange
        var mockCoursesService = new Mock<ICoursesService>();
        mockCoursesService.Setup(s => s.GetCoursesAsync()).ReturnsAsync(new List<CourseDto>());
        var viewModel = new CoursesViewModel(mockCoursesService.Object);

        // Act - Initial load
        await viewModel.LoadAsync(forceRefresh: true);
        mockCoursesService.Verify(s => s.GetCoursesAsync(), Times.Once);

        // Act - Attempted reload within refresh interval
        await viewModel.LoadAsync(forceRefresh: false);

        // Assert
        mockCoursesService.Verify(s => s.GetCoursesAsync(), Times.Once);
    }


    /// <summary>
    /// Tests that setting ErrorMessage with newline characters correctly updates the value.
    /// </summary>
    [TestMethod]
    [DataRow("\n", DisplayName = "Single newline")]
    [DataRow("\r\n", DisplayName = "Windows newline")]
    [DataRow("Line1\nLine2", DisplayName = "Multi-line with LF")]
    [DataRow("Line1\r\nLine2\r\nLine3", DisplayName = "Multi-line with CRLF")]
    public void ErrorMessage_SetValueWithNewlines_UpdatesProperty(string testValue)
    {
        // Arrange
        var mockCoursesService = new Mock<ICoursesService>();
        var viewModel = new CoursesViewModel(mockCoursesService.Object);

        // Act
        viewModel.ErrorMessage = testValue;

        // Assert
        Assert.AreEqual(testValue, viewModel.ErrorMessage);
    }

    /// <summary>
    /// Tests that setting ErrorMessage with tab characters correctly updates the value.
    /// </summary>
    [TestMethod]
    [DataRow("\t", DisplayName = "Single tab")]
    [DataRow("\t\t\t", DisplayName = "Multiple tabs")]
    [DataRow("Column1\tColumn2", DisplayName = "Tab-separated values")]
    [DataRow("  \t  \t  ", DisplayName = "Mixed spaces and tabs")]
    public void ErrorMessage_SetValueWithTabs_UpdatesProperty(string testValue)
    {
        // Arrange
        var mockCoursesService = new Mock<ICoursesService>();
        var viewModel = new CoursesViewModel(mockCoursesService.Object);

        // Act
        viewModel.ErrorMessage = testValue;

        // Assert
        Assert.AreEqual(testValue, viewModel.ErrorMessage);
    }

    /// <summary>
    /// Tests that setting ErrorMessage with Unicode characters correctly updates the value.
    /// </summary>
    [TestMethod]
    [DataRow("Error: 你好", DisplayName = "Chinese characters")]
    [DataRow("Erreur: café", DisplayName = "Accented characters")]
    [DataRow("Error 😀🎉", DisplayName = "Emoji characters")]
    [DataRow("Ошибка", DisplayName = "Cyrillic characters")]
    [DataRow("エラー", DisplayName = "Japanese characters")]
    public void ErrorMessage_SetValueWithUnicode_UpdatesProperty(string testValue)
    {
        // Arrange
        var mockCoursesService = new Mock<ICoursesService>();
        var viewModel = new CoursesViewModel(mockCoursesService.Object);

        // Act
        viewModel.ErrorMessage = testValue;

        // Assert
        Assert.AreEqual(testValue, viewModel.ErrorMessage);
    }

    /// <summary>
    /// Tests that setting ErrorMessage with control characters correctly updates the value.
    /// </summary>
    [TestMethod]
    [DataRow("\0", DisplayName = "Null character")]
    [DataRow("Error\0Message", DisplayName = "Embedded null character")]
    [DataRow("\b\f\v", DisplayName = "Backspace, form feed, vertical tab")]
    public void ErrorMessage_SetValueWithControlCharacters_UpdatesProperty(string testValue)
    {
        // Arrange
        var mockCoursesService = new Mock<ICoursesService>();
        var viewModel = new CoursesViewModel(mockCoursesService.Object);

        // Act
        viewModel.ErrorMessage = testValue;

        // Assert
        Assert.AreEqual(testValue, viewModel.ErrorMessage);
    }

    /// <summary>
    /// Tests that setting ErrorMessage with newline raises PropertyChanged event.
    /// </summary>
    [TestMethod]
    public void ErrorMessage_SetValueWithNewline_RaisesPropertyChangedEvent()
    {
        // Arrange
        var mockCoursesService = new Mock<ICoursesService>();
        var viewModel = new CoursesViewModel(mockCoursesService.Object);
        string? raisedPropertyName = null;
        viewModel.PropertyChanged += (sender, args) => raisedPropertyName = args.PropertyName;

        // Act
        viewModel.ErrorMessage = "Line1\nLine2";

        // Assert
        Assert.AreEqual("ErrorMessage", raisedPropertyName);
    }

    /// <summary>
    /// Tests that setting ErrorMessage with Unicode raises PropertyChanged event.
    /// </summary>
    [TestMethod]
    public void ErrorMessage_SetValueWithUnicode_RaisesPropertyChangedEvent()
    {
        // Arrange
        var mockCoursesService = new Mock<ICoursesService>();
        var viewModel = new CoursesViewModel(mockCoursesService.Object);
        string? raisedPropertyName = null;
        viewModel.PropertyChanged += (sender, args) => raisedPropertyName = args.PropertyName;

        // Act
        viewModel.ErrorMessage = "Error 😀";

        // Assert
        Assert.AreEqual("ErrorMessage", raisedPropertyName);
    }

    /// <summary>
    /// Tests that setting ErrorMessage with mixed control characters correctly updates and preserves the value.
    /// </summary>
    [TestMethod]
    public void ErrorMessage_SetValueWithMixedControlCharacters_UpdatesAndPreservesValue()
    {
        // Arrange
        var mockCoursesService = new Mock<ICoursesService>();
        var viewModel = new CoursesViewModel(mockCoursesService.Object);
        string testValue = "Error:\nDetails:\tCode\0End";

        // Act
        viewModel.ErrorMessage = testValue;

        // Assert
        Assert.AreEqual(testValue, viewModel.ErrorMessage);
    }
}