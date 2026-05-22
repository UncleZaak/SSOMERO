using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime;
using System.Runtime.CompilerServices;
using System.Threading;

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Ssomero;
using Ssomero.ViewModels;

namespace Ssomero.ViewModels.UnitTests;




/// <summary>
/// Unit tests for the BaseViewModel class.
/// </summary>
[TestClass]
public class BaseViewModelTests
{
    /// <summary>
    /// Tests that the Title property returns an empty string as its initial value.
    /// </summary>
    [TestMethod]
    public void Title_InitialValue_ReturnsEmptyString()
    {
        // Arrange
        var viewModel = new BaseViewModel();

        // Act
        var result = viewModel.Title;

        // Assert
        Assert.AreEqual(string.Empty, result);
    }

    /// <summary>
    /// Tests that setting a new value to the Title property updates the value and raises PropertyChanged event.
    /// Tests various string values including normal strings, empty strings, whitespace, and special characters.
    /// </summary>
    /// <param name="newValue">The value to set to the Title property.</param>
    [TestMethod]
    [DataRow("Test Title")]
    [DataRow("")]
    [DataRow("   ")]
    [DataRow("Title with special chars: !@#$%^&*()")]
    [DataRow("Very long title with many characters: Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat.")]
    [DataRow("\t\n\r")]
    [DataRow("Title\nWith\nNewlines")]
    [DataRow("Title\tWith\tTabs")]
    public void Title_SetNewValue_UpdatesValueAndRaisesPropertyChanged(string newValue)
    {
        // Arrange
        var viewModel = new BaseViewModel();
        var propertyChangedRaised = false;
        string? raisedPropertyName = null;
        viewModel.PropertyChanged += (sender, args) =>
        {
            propertyChangedRaised = true;
            raisedPropertyName = args.PropertyName;
        };

        // Act
        viewModel.Title = newValue;

        // Assert
        Assert.AreEqual(newValue, viewModel.Title);
        Assert.IsTrue(propertyChangedRaised, "PropertyChanged event should be raised");
        Assert.AreEqual("Title", raisedPropertyName);
    }

    /// <summary>
    /// Tests that setting the same value to the Title property does not raise PropertyChanged event.
    /// </summary>
    [TestMethod]
    public void Title_SetSameValue_DoesNotRaisePropertyChanged()
    {
        // Arrange
        var viewModel = new BaseViewModel();
        viewModel.Title = "Initial Value";
        var propertyChangedRaised = false;
        viewModel.PropertyChanged += (sender, args) =>
        {
            propertyChangedRaised = true;
        };

        // Act
        viewModel.Title = "Initial Value";

        // Assert
        Assert.AreEqual("Initial Value", viewModel.Title);
        Assert.IsFalse(propertyChangedRaised, "PropertyChanged event should not be raised when setting the same value");
    }

    /// <summary>
    /// Tests that setting the same empty string value does not raise PropertyChanged event.
    /// </summary>
    [TestMethod]
    public void Title_SetSameEmptyValue_DoesNotRaisePropertyChanged()
    {
        // Arrange
        var viewModel = new BaseViewModel();
        var propertyChangedRaised = false;
        viewModel.PropertyChanged += (sender, args) =>
        {
            propertyChangedRaised = true;
        };

        // Act
        viewModel.Title = string.Empty;

        // Assert
        Assert.AreEqual(string.Empty, viewModel.Title);
        Assert.IsFalse(propertyChangedRaised, "PropertyChanged event should not be raised when setting the same empty value");
    }

    /// <summary>
    /// Tests that setting different values sequentially raises PropertyChanged event for each change.
    /// </summary>
    [TestMethod]
    public void Title_SetDifferentValuesSequentially_RaisesPropertyChangedForEachChange()
    {
        // Arrange
        var viewModel = new BaseViewModel();
        var propertyChangedCount = 0;
        viewModel.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == "Title")
            {
                propertyChangedCount++;
            }
        };

        // Act
        viewModel.Title = "First";
        viewModel.Title = "Second";
        viewModel.Title = "Third";

        // Assert
        Assert.AreEqual("Third", viewModel.Title);
        Assert.AreEqual(3, propertyChangedCount, "PropertyChanged should be raised for each different value");
    }

    /// <summary>
    /// Tests that PropertyChanged event arguments contain the correct property name "Title".
    /// </summary>
    [TestMethod]
    public void Title_PropertyChangedEventArgs_HasCorrectPropertyName()
    {
        // Arrange
        var viewModel = new BaseViewModel();
        PropertyChangedEventArgs? eventArgs = null;
        viewModel.PropertyChanged += (sender, args) =>
        {
            eventArgs = args;
        };

        // Act
        viewModel.Title = "Test";

        // Assert
        Assert.IsNotNull(eventArgs);
        Assert.AreEqual("Title", eventArgs.PropertyName);
    }

    /// <summary>
    /// Tests that the PropertyChanged event sender is the BaseViewModel instance itself.
    /// </summary>
    [TestMethod]
    public void Title_PropertyChangedEvent_SenderIsViewModel()
    {
        // Arrange
        var viewModel = new BaseViewModel();
        object? eventSender = null;
        viewModel.PropertyChanged += (sender, args) =>
        {
            eventSender = sender;
        };

        // Act
        viewModel.Title = "Test";

        // Assert
        Assert.IsNotNull(eventSender);
        Assert.AreSame(viewModel, eventSender);
    }

    /// <summary>
    /// Tests that setting null to the Title property updates the value and raises PropertyChanged event.
    /// Although Title is non-nullable in the property signature, the underlying SetProperty can handle null values.
    /// </summary>
    [TestMethod]
    public void Title_SetNull_UpdatesValueAndRaisesPropertyChanged()
    {
        // Arrange
        var viewModel = new BaseViewModel();
        viewModel.Title = "Initial Value";
        var propertyChangedRaised = false;
        viewModel.PropertyChanged += (sender, args) =>
        {
            propertyChangedRaised = true;
        };

        // Act
        viewModel.Title = null!;

        // Assert
        Assert.IsNull(viewModel.Title);
        Assert.IsTrue(propertyChangedRaised, "PropertyChanged event should be raised");
    }

    /// <summary>
    /// Tests that setting null twice does not raise PropertyChanged event the second time.
    /// </summary>
    [TestMethod]
    public void Title_SetNullTwice_DoesNotRaisePropertyChangedSecondTime()
    {
        // Arrange
        var viewModel = new BaseViewModel();
        viewModel.Title = null!;
        var propertyChangedRaised = false;
        viewModel.PropertyChanged += (sender, args) =>
        {
            propertyChangedRaised = true;
        };

        // Act
        viewModel.Title = null!;

        // Assert
        Assert.IsNull(viewModel.Title);
        Assert.IsFalse(propertyChangedRaised, "PropertyChanged event should not be raised when setting null twice");
    }

    /// <summary>
    /// Tests that setting IsBusy to a different value updates the property and raises PropertyChanged event.
    /// Input: New boolean value different from current value.
    /// Expected: Property value is updated and PropertyChanged event is raised with correct property name.
    /// </summary>
    /// <param name="initialValue">The initial value of IsBusy.</param>
    /// <param name="newValue">The new value to set.</param>
    [TestMethod]
    [DataRow(false, true)]
    [DataRow(true, false)]
    public void IsBusy_SetToDifferentValue_UpdatesValueAndRaisesPropertyChanged(bool initialValue, bool newValue)
    {
        // Arrange
        var viewModel = new BaseViewModel();
        viewModel.IsBusy = initialValue;
        var propertyChangedRaised = false;
        string? raisedPropertyName = null;
        viewModel.PropertyChanged += (sender, args) =>
        {
            propertyChangedRaised = true;
            raisedPropertyName = args.PropertyName;
        };

        // Act
        viewModel.IsBusy = newValue;

        // Assert
        Assert.AreEqual(newValue, viewModel.IsBusy);
        Assert.IsTrue(propertyChangedRaised);
        Assert.AreEqual("IsBusy", raisedPropertyName);
    }

    /// <summary>
    /// Tests that setting IsBusy to the same value does not raise PropertyChanged event.
    /// Input: Boolean value equal to current value.
    /// Expected: Property value remains unchanged and PropertyChanged event is not raised.
    /// </summary>
    /// <param name="value">The value to set (same as current).</param>
    [TestMethod]
    [DataRow(true)]
    [DataRow(false)]
    public void IsBusy_SetToSameValue_DoesNotRaisePropertyChanged(bool value)
    {
        // Arrange
        var viewModel = new BaseViewModel();
        viewModel.IsBusy = value;
        var propertyChangedRaised = false;
        viewModel.PropertyChanged += (sender, args) =>
        {
            propertyChangedRaised = true;
        };

        // Act
        viewModel.IsBusy = value;

        // Assert
        Assert.AreEqual(value, viewModel.IsBusy);
        Assert.IsFalse(propertyChangedRaised);
    }

    /// <summary>
    /// Tests that getting IsBusy returns the correct current value.
    /// Input: Setting IsBusy to a specific value.
    /// Expected: Getting IsBusy returns the same value.
    /// </summary>
    /// <param name="expectedValue">The value to set and retrieve.</param>
    [TestMethod]
    [DataRow(true)]
    [DataRow(false)]
    public void IsBusy_Get_ReturnsCurrentValue(bool expectedValue)
    {
        // Arrange
        var viewModel = new BaseViewModel();

        // Act
        viewModel.IsBusy = expectedValue;
        var actualValue = viewModel.IsBusy;

        // Assert
        Assert.AreEqual(expectedValue, actualValue);
    }

    /// <summary>
    /// Tests that IsBusy has a default initial value of false.
    /// Input: Newly created BaseViewModel instance.
    /// Expected: IsBusy returns false.
    /// </summary>
    [TestMethod]
    public void IsBusy_InitialValue_IsFalse()
    {
        // Arrange & Act
        var viewModel = new BaseViewModel();

        // Assert
        Assert.IsFalse(viewModel.IsBusy);
    }

    /// <summary>
    /// Tests that setting IsBusy multiple times with alternating values raises PropertyChanged event each time.
    /// Input: Multiple alternating boolean values.
    /// Expected: PropertyChanged event is raised for each value change.
    /// </summary>
    [TestMethod]
    public void IsBusy_SetMultipleTimesWithDifferentValues_RaisesPropertyChangedEachTime()
    {
        // Arrange
        var viewModel = new BaseViewModel();
        var propertyChangedCount = 0;
        viewModel.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == "IsBusy")
            {
                propertyChangedCount++;
            }
        };

        // Act
        viewModel.IsBusy = true;
        viewModel.IsBusy = false;
        viewModel.IsBusy = true;

        // Assert
        Assert.AreEqual(3, propertyChangedCount);
        Assert.IsTrue(viewModel.IsBusy);
    }

    /// <summary>
    /// Tests that PropertyChanged event sender is the BaseViewModel instance.
    /// Input: Setting IsBusy to a new value.
    /// Expected: PropertyChanged event sender is the BaseViewModel instance.
    /// </summary>
    [TestMethod]
    public void IsBusy_SetValue_PropertyChangedSenderIsViewModel()
    {
        // Arrange
        var viewModel = new BaseViewModel();
        object? eventSender = null;
        viewModel.PropertyChanged += (sender, args) =>
        {
            eventSender = sender;
        };

        // Act
        viewModel.IsBusy = true;

        // Assert
        Assert.AreSame(viewModel, eventSender);
    }

    /// <summary>
    /// Tests that CancelPendingRequests does not throw when called on a newly created instance
    /// with no CancellationTokenSource initialized.
    /// Expected: No exception is thrown.
    /// </summary>
    [TestMethod]
    public void CancelPendingRequests_WhenCtsIsNull_DoesNotThrow()
    {
        // Arrange
        var viewModel = new BaseViewModel();

        // Act & Assert
        viewModel.CancelPendingRequests();
    }

    /// <summary>
    /// Helper class to expose protected members for testing.
    /// </summary>
    private class TestableBaseViewModel : BaseViewModel
    {
        public CancellationToken TestCreateLinkedToken()
        {
            return CreateLinkedToken();
        }
    }



    /// <summary>
    /// Tests that CancelPendingRequests can be called multiple times without throwing an exception.
    /// Input: Multiple consecutive calls to CancelPendingRequests.
    /// Expected: No exception is thrown.
    /// </summary>
    [TestMethod]
    public void CancelPendingRequests_CalledMultipleTimes_DoesNotThrow()
    {
        // Arrange
        var viewModel = new BaseViewModel();

        // Act & Assert
        viewModel.CancelPendingRequests();
        viewModel.CancelPendingRequests();
        viewModel.CancelPendingRequests();
    }

    /// <summary>
    /// Tests that RaisePropertyChanged does not throw when PropertyChanged has no subscribers.
    /// Input: Valid property name with no event subscribers.
    /// Expected: No exception is thrown.
    /// </summary>
    [TestMethod]
    public void RaisePropertyChanged_NoSubscribers_DoesNotThrow()
    {
        // Arrange
        var viewModel = new TestableBaseViewModelForRaisePropertyChanged();

        // Act & Assert
        viewModel.TestRaisePropertyChanged("TestProperty");
    }

    /// <summary>
    /// Tests that RaisePropertyChanged invokes PropertyChanged event with correct property name.
    /// Input: Various valid property names.
    /// Expected: PropertyChanged event is raised with the specified property name.
    /// </summary>
    /// <param name="propertyName">The property name to test.</param>
    [TestMethod]
    [DataRow("Title")]
    [DataRow("IsBusy")]
    [DataRow("SomeProperty")]
    [DataRow("PropertyWithNumbers123")]
    [DataRow("_propertyWithUnderscore")]
    public void RaisePropertyChanged_ValidPropertyName_RaisesEventWithCorrectPropertyName(string propertyName)
    {
        // Arrange
        var viewModel = new TestableBaseViewModelForRaisePropertyChanged();
        string? raisedPropertyName = null;
        viewModel.PropertyChanged += (sender, args) => raisedPropertyName = args.PropertyName;

        // Act
        viewModel.TestRaisePropertyChanged(propertyName);

        // Assert
        Assert.AreEqual(propertyName, raisedPropertyName);
    }

    /// <summary>
    /// Tests that RaisePropertyChanged raises PropertyChanged event with empty string.
    /// Input: Empty string as property name.
    /// Expected: PropertyChanged event is raised with empty string.
    /// </summary>
    [TestMethod]
    public void RaisePropertyChanged_EmptyString_RaisesEventWithEmptyString()
    {
        // Arrange
        var viewModel = new TestableBaseViewModelForRaisePropertyChanged();
        string? raisedPropertyName = null;
        viewModel.PropertyChanged += (sender, args) => raisedPropertyName = args.PropertyName;

        // Act
        viewModel.TestRaisePropertyChanged(string.Empty);

        // Assert
        Assert.AreEqual(string.Empty, raisedPropertyName);
    }

    /// <summary>
    /// Tests that RaisePropertyChanged handles whitespace-only property names.
    /// Input: Whitespace-only strings.
    /// Expected: PropertyChanged event is raised with the whitespace string.
    /// </summary>
    /// <param name="propertyName">The whitespace property name to test.</param>
    [TestMethod]
    [DataRow("   ")]
    [DataRow("\t")]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [DataRow(" \t\n ")]
    public void RaisePropertyChanged_WhitespacePropertyName_RaisesEventWithWhitespace(string propertyName)
    {
        // Arrange
        var viewModel = new TestableBaseViewModelForRaisePropertyChanged();
        string? raisedPropertyName = null;
        viewModel.PropertyChanged += (sender, args) => raisedPropertyName = args.PropertyName;

        // Act
        viewModel.TestRaisePropertyChanged(propertyName);

        // Assert
        Assert.AreEqual(propertyName, raisedPropertyName);
    }

    /// <summary>
    /// Tests that RaisePropertyChanged handles property names with special characters.
    /// Input: Property names containing special characters.
    /// Expected: PropertyChanged event is raised with the special character string.
    /// </summary>
    /// <param name="propertyName">The property name with special characters.</param>
    [TestMethod]
    [DataRow("Property!@#$%")]
    [DataRow("Property-With-Dashes")]
    [DataRow("Property.With.Dots")]
    [DataRow("Property[With]Brackets")]
    [DataRow("Property(With)Parentheses")]
    [DataRow("Property<With>AngleBrackets")]
    public void RaisePropertyChanged_SpecialCharacters_RaisesEventWithSpecialCharacters(string propertyName)
    {
        // Arrange
        var viewModel = new TestableBaseViewModelForRaisePropertyChanged();
        string? raisedPropertyName = null;
        viewModel.PropertyChanged += (sender, args) => raisedPropertyName = args.PropertyName;

        // Act
        viewModel.TestRaisePropertyChanged(propertyName);

        // Assert
        Assert.AreEqual(propertyName, raisedPropertyName);
    }

    /// <summary>
    /// Tests that RaisePropertyChanged handles very long property names.
    /// Input: Very long string as property name.
    /// Expected: PropertyChanged event is raised with the long string.
    /// </summary>
    [TestMethod]
    public void RaisePropertyChanged_VeryLongPropertyName_RaisesEventWithLongString()
    {
        // Arrange
        var viewModel = new TestableBaseViewModelForRaisePropertyChanged();
        var longPropertyName = new string('A', 10000);
        string? raisedPropertyName = null;
        viewModel.PropertyChanged += (sender, args) => raisedPropertyName = args.PropertyName;

        // Act
        viewModel.TestRaisePropertyChanged(longPropertyName);

        // Assert
        Assert.AreEqual(longPropertyName, raisedPropertyName);
    }

    /// <summary>
    /// Tests that RaisePropertyChanged invokes all subscribed event handlers.
    /// Input: Multiple event subscribers.
    /// Expected: All subscribers are invoked exactly once.
    /// </summary>
    [TestMethod]
    public void RaisePropertyChanged_MultipleSubscribers_InvokesAllSubscribers()
    {
        // Arrange
        var viewModel = new TestableBaseViewModelForRaisePropertyChanged();
        int firstHandlerInvoked = 0;
        int secondHandlerInvoked = 0;
        int thirdHandlerInvoked = 0;

        viewModel.PropertyChanged += (sender, args) => firstHandlerInvoked++;
        viewModel.PropertyChanged += (sender, args) => secondHandlerInvoked++;
        viewModel.PropertyChanged += (sender, args) => thirdHandlerInvoked++;

        // Act
        viewModel.TestRaisePropertyChanged("TestProperty");

        // Assert
        Assert.AreEqual(1, firstHandlerInvoked);
        Assert.AreEqual(1, secondHandlerInvoked);
        Assert.AreEqual(1, thirdHandlerInvoked);
    }

    /// <summary>
    /// Tests that RaisePropertyChanged passes the correct sender (the view model instance) to event handlers.
    /// Input: Valid property name with event subscriber.
    /// Expected: Event sender is the view model instance.
    /// </summary>
    [TestMethod]
    public void RaisePropertyChanged_WithSubscriber_SenderIsViewModel()
    {
        // Arrange
        var viewModel = new TestableBaseViewModelForRaisePropertyChanged();
        object? eventSender = null;
        viewModel.PropertyChanged += (sender, args) => eventSender = sender;

        // Act
        viewModel.TestRaisePropertyChanged("TestProperty");

        // Assert
        Assert.AreSame(viewModel, eventSender);
    }

    /// <summary>
    /// Tests that RaisePropertyChanged creates PropertyChangedEventArgs with correct property name.
    /// Input: Valid property name.
    /// Expected: EventArgs.PropertyName matches the input property name.
    /// </summary>
    [TestMethod]
    public void RaisePropertyChanged_WithSubscriber_EventArgsContainsCorrectPropertyName()
    {
        // Arrange
        var viewModel = new TestableBaseViewModelForRaisePropertyChanged();
        PropertyChangedEventArgs? eventArgs = null;
        viewModel.PropertyChanged += (sender, args) => eventArgs = args;
        var expectedPropertyName = "MyProperty";

        // Act
        viewModel.TestRaisePropertyChanged(expectedPropertyName);

        // Assert
        Assert.IsNotNull(eventArgs);
        Assert.AreEqual(expectedPropertyName, eventArgs.PropertyName);
    }

    /// <summary>
    /// Tests that RaisePropertyChanged invokes event handler exactly once per call.
    /// Input: Single event subscriber, method called once.
    /// Expected: Event handler is invoked exactly once.
    /// </summary>
    [TestMethod]
    public void RaisePropertyChanged_SingleSubscriber_InvokesHandlerOnce()
    {
        // Arrange
        var viewModel = new TestableBaseViewModelForRaisePropertyChanged();
        int handlerInvokeCount = 0;
        viewModel.PropertyChanged += (sender, args) => handlerInvokeCount++;

        // Act
        viewModel.TestRaisePropertyChanged("TestProperty");

        // Assert
        Assert.AreEqual(1, handlerInvokeCount);
    }

    /// <summary>
    /// Tests that calling RaisePropertyChanged multiple times invokes event handler each time.
    /// Input: Multiple sequential calls with different property names.
    /// Expected: Event handler is invoked for each call with corresponding property names.
    /// </summary>
    [TestMethod]
    public void RaisePropertyChanged_CalledMultipleTimes_InvokesHandlerEachTime()
    {
        // Arrange
        var viewModel = new TestableBaseViewModelForRaisePropertyChanged();
        var raisedPropertyNames = new List<string?>();
        viewModel.PropertyChanged += (sender, args) => raisedPropertyNames.Add(args.PropertyName);

        // Act
        viewModel.TestRaisePropertyChanged("Property1");
        viewModel.TestRaisePropertyChanged("Property2");
        viewModel.TestRaisePropertyChanged("Property3");

        // Assert
        Assert.AreEqual(3, raisedPropertyNames.Count);
        Assert.AreEqual("Property1", raisedPropertyNames[0]);
        Assert.AreEqual("Property2", raisedPropertyNames[1]);
        Assert.AreEqual("Property3", raisedPropertyNames[2]);
    }

    /// <summary>
    /// Tests that RaisePropertyChanged with null property name raises event with null.
    /// Input: Null property name.
    /// Expected: PropertyChanged event is raised with null property name.
    /// </summary>
    [TestMethod]
    public void RaisePropertyChanged_NullPropertyName_RaisesEventWithNull()
    {
        // Arrange
        var viewModel = new TestableBaseViewModelForRaisePropertyChanged();
        string? raisedPropertyName = "NotNull";
        bool eventRaised = false;
        viewModel.PropertyChanged += (sender, args) =>
        {
            raisedPropertyName = args.PropertyName;
            eventRaised = true;
        };

        // Act
        viewModel.TestRaisePropertyChanged(null!);

        // Assert
        Assert.IsTrue(eventRaised);
        Assert.IsNull(raisedPropertyName);
    }

    /// <summary>
    /// Helper class to expose protected RaisePropertyChanged method for testing.
    /// </summary>
    private class TestableBaseViewModelForRaisePropertyChanged : BaseViewModel
    {
        public void TestRaisePropertyChanged(string propertyName)
        {
            RaisePropertyChanged(propertyName);
        }
    }

}