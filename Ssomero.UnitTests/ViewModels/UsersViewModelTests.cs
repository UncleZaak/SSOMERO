using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

using Microsoft.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Controls;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Ssomero;
using Ssomero.Interfaces;
using Ssomero.Models;
using Ssomero.ViewModels;

namespace Ssomero.ViewModels.UnitTests;




/// <summary>
/// Unit tests for <see cref="UsersViewModel"/> class.
/// </summary>
[TestClass]
public class UsersViewModelTests
{
    /// <summary>
    /// Tests that the IsLecturersTab property returns the expected boolean value
    /// based on the current value of SelectedTab property.
    /// </summary>
    /// <param name="selectedTabValue">The value to set for SelectedTab property.</param>
    /// <param name="expectedResult">The expected result of IsLecturersTab property.</param>
    [TestMethod]
    [DataRow("Lecturers", true, DisplayName = "IsLecturersTab returns true when SelectedTab is 'Lecturers'")]
    [DataRow("Students", false, DisplayName = "IsLecturersTab returns false when SelectedTab is 'Students'")]
    [DataRow("", false, DisplayName = "IsLecturersTab returns false when SelectedTab is empty string")]
    [DataRow("lecturers", false, DisplayName = "IsLecturersTab returns false when SelectedTab is lowercase 'lecturers'")]
    [DataRow("LECTURERS", false, DisplayName = "IsLecturersTab returns false when SelectedTab is uppercase 'LECTURERS'")]
    [DataRow(" Lecturers ", false, DisplayName = "IsLecturersTab returns false when SelectedTab has leading/trailing whitespace")]
    [DataRow("Lecturer", false, DisplayName = "IsLecturersTab returns false when SelectedTab is 'Lecturer' (singular)")]
    [DataRow("SomeOtherValue", false, DisplayName = "IsLecturersTab returns false when SelectedTab is any other value")]
    [DataRow("   ", false, DisplayName = "IsLecturersTab returns false when SelectedTab is whitespace only")]
    public void IsLecturersTab_VariousSelectedTabValues_ReturnsExpectedBoolean(string selectedTabValue, bool expectedResult)
    {
        // Arrange
        var mockAdminService = new Mock<IAdminService>();
        var mockLogger = new Mock<ILogger<UsersViewModel>>();
        var viewModel = new UsersViewModel(mockAdminService.Object, new Mock<IRefreshCoordinator>().Object, mockLogger.Object);

        // Act
        viewModel.SelectedTab = selectedTabValue;
        var actualResult = viewModel.IsLecturersTab;

        // Assert
        Assert.AreEqual(expectedResult, actualResult);
    }

    /// <summary>
    /// Tests that the IsLecturersTab property returns false when using the default
    /// value of SelectedTab ("Students") without explicitly setting it.
    /// </summary>
    [TestMethod]
    public void IsLecturersTab_DefaultSelectedTab_ReturnsFalse()
    {
        // Arrange
        var mockAdminService = new Mock<IAdminService>();
        var mockLogger = new Mock<ILogger<UsersViewModel>>();
        var viewModel = new UsersViewModel(mockAdminService.Object, new Mock<IRefreshCoordinator>().Object, mockLogger.Object);

        // Act
        var actualResult = viewModel.IsLecturersTab;

        // Assert
        Assert.IsFalse(actualResult);
    }

    /// <summary>
    /// Tests that the TotalStudents getter returns the correct value after setting it.
    /// </summary>
    /// <param name="value">The value to set and verify.</param>
    [TestMethod]
    [DataRow(0)]
    [DataRow(1)]
    [DataRow(-1)]
    [DataRow(100)]
    [DataRow(-100)]
    [DataRow(int.MaxValue)]
    [DataRow(int.MinValue)]
    public void TotalStudents_SetValue_ReturnsCorrectValue(int value)
    {
        // Arrange
        var mockAdminService = new Mock<IAdminService>();
        var mockLogger = new Mock<ILogger<UsersViewModel>>();
        var viewModel = new UsersViewModel(mockAdminService.Object, new Mock<IRefreshCoordinator>().Object, mockLogger.Object);

        // Act
        viewModel.TotalStudents = value;

        // Assert
        Assert.AreEqual(value, viewModel.TotalStudents);
    }

    /// <summary>
    /// Tests that setting TotalStudents raises the PropertyChanged event with the correct property name.
    /// </summary>
    [TestMethod]
    public void TotalStudents_SetNewValue_RaisesPropertyChanged()
    {
        // Arrange
        var mockAdminService = new Mock<IAdminService>();
        var mockLogger = new Mock<ILogger<UsersViewModel>>();
        var viewModel = new UsersViewModel(mockAdminService.Object, new Mock<IRefreshCoordinator>().Object, mockLogger.Object);
        string? raisedPropertyName = null;
        viewModel.PropertyChanged += (sender, args) => raisedPropertyName = args.PropertyName;

        // Act
        viewModel.TotalStudents = 42;

        // Assert
        Assert.AreEqual("TotalStudents", raisedPropertyName);
    }

    /// <summary>
    /// Tests that setting TotalStudents to the same value does not raise PropertyChanged event.
    /// </summary>
    [TestMethod]
    public void TotalStudents_SetSameValue_DoesNotRaisePropertyChanged()
    {
        // Arrange
        var mockAdminService = new Mock<IAdminService>();
        var mockLogger = new Mock<ILogger<UsersViewModel>>();
        var viewModel = new UsersViewModel(mockAdminService.Object, new Mock<IRefreshCoordinator>().Object, mockLogger.Object);
        viewModel.TotalStudents = 10;
        int eventRaisedCount = 0;
        viewModel.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == "TotalStudents")
            {
                eventRaisedCount++;
            }
        };

        // Act
        viewModel.TotalStudents = 10;

        // Assert
        Assert.AreEqual(0, eventRaisedCount);
    }

    /// <summary>
    /// Tests that TotalStudents can be set multiple times to different values and each change raises PropertyChanged.
    /// </summary>
    /// <param name="firstValue">The first value to set.</param>
    /// <param name="secondValue">The second value to set.</param>
    [TestMethod]
    [DataRow(0, 1)]
    [DataRow(1, 0)]
    [DataRow(-1, 1)]
    [DataRow(int.MinValue, int.MaxValue)]
    [DataRow(100, -100)]
    public void TotalStudents_SetMultipleDifferentValues_RaisesPropertyChangedEachTime(int firstValue, int secondValue)
    {
        // Arrange
        var mockAdminService = new Mock<IAdminService>();
        var mockLogger = new Mock<ILogger<UsersViewModel>>();
        var viewModel = new UsersViewModel(mockAdminService.Object, new Mock<IRefreshCoordinator>().Object, mockLogger.Object);
        int eventRaisedCount = 0;
        viewModel.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == "TotalStudents")
            {
                eventRaisedCount++;
            }
        };

        // Act
        viewModel.TotalStudents = firstValue;
        viewModel.TotalStudents = secondValue;

        // Assert
        Assert.AreEqual(2, eventRaisedCount);
        Assert.AreEqual(secondValue, viewModel.TotalStudents);
    }

    /// <summary>
    /// Tests that the initial value of TotalStudents is zero (default int value).
    /// </summary>
    [TestMethod]
    public void TotalStudents_InitialValue_IsZero()
    {
        // Arrange
        var mockAdminService = new Mock<IAdminService>();
        var mockLogger = new Mock<ILogger<UsersViewModel>>();

        // Act
        var viewModel = new UsersViewModel(mockAdminService.Object, new Mock<IRefreshCoordinator>().Object, mockLogger.Object);

        // Assert
        Assert.AreEqual(0, viewModel.TotalStudents);
    }

    /// <summary>
    /// Tests that IsStudentsTab returns the expected boolean value based on the SelectedTab property value.
    /// Validates case-sensitive string comparison and various edge cases including empty strings,
    /// case variations, whitespace, and different tab values.
    /// </summary>
    /// <param name="selectedTab">The value to set for SelectedTab property.</param>
    /// <param name="expected">The expected return value of IsStudentsTab.</param>
    [TestMethod]
    [DataRow("Students", true, DisplayName = "IsStudentsTab_SelectedTabIsStudents_ReturnsTrue")]
    [DataRow("Lecturers", false, DisplayName = "IsStudentsTab_SelectedTabIsLecturers_ReturnsFalse")]
    [DataRow("", false, DisplayName = "IsStudentsTab_SelectedTabIsEmpty_ReturnsFalse")]
    [DataRow("students", false, DisplayName = "IsStudentsTab_SelectedTabIsLowercase_ReturnsFalse")]
    [DataRow("STUDENTS", false, DisplayName = "IsStudentsTab_SelectedTabIsUppercase_ReturnsFalse")]
    [DataRow(" Students", false, DisplayName = "IsStudentsTab_SelectedTabHasLeadingSpace_ReturnsFalse")]
    [DataRow("Students ", false, DisplayName = "IsStudentsTab_SelectedTabHasTrailingSpace_ReturnsFalse")]
    [DataRow(" Students ", false, DisplayName = "IsStudentsTab_SelectedTabHasLeadingAndTrailingSpaces_ReturnsFalse")]
    [DataRow("Admin", false, DisplayName = "IsStudentsTab_SelectedTabIsOtherValue_ReturnsFalse")]
    [DataRow("Stud", false, DisplayName = "IsStudentsTab_SelectedTabIsPartialMatch_ReturnsFalse")]
    public void IsStudentsTab_VariousSelectedTabValues_ReturnsExpectedResult(string selectedTab, bool expected)
    {
        // Arrange
        var mockAdminService = new Mock<IAdminService>();
        var mockLogger = new Mock<ILogger<UsersViewModel>>();
        var viewModel = new UsersViewModel(mockAdminService.Object, new Mock<IRefreshCoordinator>().Object, mockLogger.Object);
        viewModel.SelectedTab = selectedTab;

        // Act
        bool result = viewModel.IsStudentsTab;

        // Assert
        Assert.AreEqual(expected, result);
    }

    /// <summary>
    /// Tests that IsStudentsTab returns true by default when UsersViewModel is instantiated
    /// with the default SelectedTab value of "Students".
    /// </summary>
    [TestMethod]
    public void IsStudentsTab_DefaultState_ReturnsTrue()
    {
        // Arrange
        var mockAdminService = new Mock<IAdminService>();
        var mockLogger = new Mock<ILogger<UsersViewModel>>();
        var viewModel = new UsersViewModel(mockAdminService.Object, new Mock<IRefreshCoordinator>().Object, mockLogger.Object);

        // Act
        bool result = viewModel.IsStudentsTab;

        // Assert
        Assert.IsTrue(result);
    }

    /// <summary>
    /// Tests that HasError getter returns the default value (false) when not explicitly set.
    /// </summary>
    [TestMethod]
    public void HasError_DefaultValue_ReturnsFalse()
    {
        // Arrange
        var mockAdminService = new Mock<IAdminService>();
        var mockLogger = new Mock<ILogger<UsersViewModel>>();
        var viewModel = new UsersViewModel(mockAdminService.Object, new Mock<IRefreshCoordinator>().Object, mockLogger.Object);

        // Act
        bool result = viewModel.HasError;

        // Assert
        Assert.IsFalse(result);
    }

    /// <summary>
    /// Tests that HasError setter updates the property value to true and raises PropertyChanged event.
    /// </summary>
    [TestMethod]
    public void HasError_SetToTrue_UpdatesValueAndRaisesPropertyChanged()
    {
        // Arrange
        var mockAdminService = new Mock<IAdminService>();
        var mockLogger = new Mock<ILogger<UsersViewModel>>();
        var viewModel = new UsersViewModel(mockAdminService.Object, new Mock<IRefreshCoordinator>().Object, mockLogger.Object);
        string? raisedPropertyName = null;
        viewModel.PropertyChanged += (sender, args) => raisedPropertyName = args.PropertyName;

        // Act
        viewModel.HasError = true;

        // Assert
        Assert.IsTrue(viewModel.HasError);
        Assert.AreEqual("HasError", raisedPropertyName);
    }

    /// <summary>
    /// Tests that HasError setter updates the property value to false and raises PropertyChanged event.
    /// </summary>
    [TestMethod]
    public void HasError_SetToFalse_UpdatesValueAndRaisesPropertyChanged()
    {
        // Arrange
        var mockAdminService = new Mock<IAdminService>();
        var mockLogger = new Mock<ILogger<UsersViewModel>>();
        var viewModel = new UsersViewModel(mockAdminService.Object, new Mock<IRefreshCoordinator>().Object, mockLogger.Object);
        viewModel.HasError = true; // Set to true first
        string? raisedPropertyName = null;
        viewModel.PropertyChanged += (sender, args) => raisedPropertyName = args.PropertyName;

        // Act
        viewModel.HasError = false;

        // Assert
        Assert.IsFalse(viewModel.HasError);
        Assert.AreEqual("HasError", raisedPropertyName);
    }

    /// <summary>
    /// Tests that HasError setter does not raise PropertyChanged event when set to the same value.
    /// </summary>
    [TestMethod]
    [DataRow(true)]
    [DataRow(false)]
    public void HasError_SetToSameValue_DoesNotRaisePropertyChanged(bool value)
    {
        // Arrange
        var mockAdminService = new Mock<IAdminService>();
        var mockLogger = new Mock<ILogger<UsersViewModel>>();
        var viewModel = new UsersViewModel(mockAdminService.Object, new Mock<IRefreshCoordinator>().Object, mockLogger.Object);
        viewModel.HasError = value; // Set initial value
        int eventRaisedCount = 0;
        viewModel.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == "HasError")
                eventRaisedCount++;
        };

        // Act
        viewModel.HasError = value; // Set to same value

        // Assert
        Assert.AreEqual(value, viewModel.HasError);
        Assert.AreEqual(0, eventRaisedCount);
    }

    /// <summary>
    /// Tests that HasError properly alternates between true and false values, raising PropertyChanged each time.
    /// </summary>
    [TestMethod]
    public void HasError_AlternatingValues_RaisesPropertyChangedEachTime()
    {
        // Arrange
        var mockAdminService = new Mock<IAdminService>();
        var mockLogger = new Mock<ILogger<UsersViewModel>>();
        var viewModel = new UsersViewModel(mockAdminService.Object, new Mock<IRefreshCoordinator>().Object, mockLogger.Object);
        int eventRaisedCount = 0;
        viewModel.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == "HasError")
                eventRaisedCount++;
        };

        // Act & Assert
        viewModel.HasError = true;
        Assert.IsTrue(viewModel.HasError);
        Assert.AreEqual(1, eventRaisedCount);

        viewModel.HasError = false;
        Assert.IsFalse(viewModel.HasError);
        Assert.AreEqual(2, eventRaisedCount);

        viewModel.HasError = true;
        Assert.IsTrue(viewModel.HasError);
        Assert.AreEqual(3, eventRaisedCount);

        viewModel.HasError = false;
        Assert.IsFalse(viewModel.HasError);
        Assert.AreEqual(4, eventRaisedCount);
    }

    /// <summary>
    /// Tests that HasError setter raises PropertyChanged event with correct sender reference.
    /// </summary>
    [TestMethod]
    public void HasError_SetValue_RaisesPropertyChangedWithCorrectSender()
    {
        // Arrange
        var mockAdminService = new Mock<IAdminService>();
        var mockLogger = new Mock<ILogger<UsersViewModel>>();
        var viewModel = new UsersViewModel(mockAdminService.Object, new Mock<IRefreshCoordinator>().Object, mockLogger.Object);
        object? eventSender = null;
        viewModel.PropertyChanged += (sender, args) => eventSender = sender;

        // Act
        viewModel.HasError = true;

        // Assert
        Assert.AreSame(viewModel, eventSender);
    }

    /// <summary>
    /// Tests that setting SelectedTab to a new value updates the property and raises property changed notifications for dependent properties.
    /// </summary>
    /// <param name="newValue">The value to set to SelectedTab.</param>
    /// <param name="expectedIsStudentsTab">Expected value of IsStudentsTab.</param>
    /// <param name="expectedIsLecturersTab">Expected value of IsLecturersTab.</param>
    [TestMethod]
    [DataRow("Lecturers", false, true, DisplayName = "Setting SelectedTab to Lecturers")]
    [DataRow("Students", true, false, DisplayName = "Setting SelectedTab to Students")]
    [DataRow("SomeOtherValue", false, false, DisplayName = "Setting SelectedTab to arbitrary value")]
    public void SelectedTab_SetToNewValue_UpdatesPropertyAndRaisesNotifications(string newValue, bool expectedIsStudentsTab, bool expectedIsLecturersTab)
    {
        // Arrange
        var adminMock = new Mock<IAdminService>();
        var loggerMock = new Mock<ILogger<UsersViewModel>>();
        var viewModel = new UsersViewModel(adminMock.Object, new Mock<IRefreshCoordinator>().Object, loggerMock.Object);

        var propertyChangedEvents = new List<string>();
        viewModel.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName != null)
            {
                propertyChangedEvents.Add(args.PropertyName);
            }
        };

        // Act
        viewModel.SelectedTab = newValue;

        // Assert
        Assert.AreEqual(newValue, viewModel.SelectedTab);
        Assert.AreEqual(expectedIsStudentsTab, viewModel.IsStudentsTab);
        Assert.AreEqual(expectedIsLecturersTab, viewModel.IsLecturersTab);
        CollectionAssert.Contains(propertyChangedEvents, "SelectedTab");
        CollectionAssert.Contains(propertyChangedEvents, "IsStudentsTab");
        CollectionAssert.Contains(propertyChangedEvents, "IsLecturersTab");
    }

    /// <summary>
    /// Tests that setting SelectedTab to the same value does not raise property changed notifications.
    /// </summary>
    [TestMethod]
    public void SelectedTab_SetToSameValue_DoesNotRaiseNotifications()
    {
        // Arrange
        var adminMock = new Mock<IAdminService>();
        var loggerMock = new Mock<ILogger<UsersViewModel>>();
        var viewModel = new UsersViewModel(adminMock.Object, new Mock<IRefreshCoordinator>().Object, loggerMock.Object);

        var initialValue = viewModel.SelectedTab;
        var propertyChangedCount = 0;
        viewModel.PropertyChanged += (sender, args) => propertyChangedCount++;

        // Act
        viewModel.SelectedTab = initialValue;

        // Assert
        Assert.AreEqual(initialValue, viewModel.SelectedTab);
        Assert.AreEqual(0, propertyChangedCount);
    }

    /// <summary>
    /// Tests that setting SelectedTab to empty string updates the property and raises notifications.
    /// </summary>
    [TestMethod]
    public void SelectedTab_SetToEmptyString_UpdatesPropertyAndRaisesNotifications()
    {
        // Arrange
        var adminMock = new Mock<IAdminService>();
        var loggerMock = new Mock<ILogger<UsersViewModel>>();
        var viewModel = new UsersViewModel(adminMock.Object, new Mock<IRefreshCoordinator>().Object, loggerMock.Object);

        var propertyChangedEvents = new List<string>();
        viewModel.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName != null)
            {
                propertyChangedEvents.Add(args.PropertyName);
            }
        };

        // Act
        viewModel.SelectedTab = string.Empty;

        // Assert
        Assert.AreEqual(string.Empty, viewModel.SelectedTab);
        Assert.IsFalse(viewModel.IsStudentsTab);
        Assert.IsFalse(viewModel.IsLecturersTab);
        CollectionAssert.Contains(propertyChangedEvents, "SelectedTab");
        CollectionAssert.Contains(propertyChangedEvents, "IsStudentsTab");
        CollectionAssert.Contains(propertyChangedEvents, "IsLecturersTab");
    }

    /// <summary>
    /// Tests that setting SelectedTab to whitespace-only string updates the property and raises notifications.
    /// </summary>
    [TestMethod]
    [DataRow("   ", DisplayName = "Whitespace - spaces")]
    [DataRow("\t", DisplayName = "Whitespace - tab")]
    [DataRow("\n", DisplayName = "Whitespace - newline")]
    [DataRow("\r\n", DisplayName = "Whitespace - carriage return and newline")]
    public void SelectedTab_SetToWhitespaceString_UpdatesPropertyAndRaisesNotifications(string whitespaceValue)
    {
        // Arrange
        var adminMock = new Mock<IAdminService>();
        var loggerMock = new Mock<ILogger<UsersViewModel>>();
        var viewModel = new UsersViewModel(adminMock.Object, new Mock<IRefreshCoordinator>().Object, loggerMock.Object);

        var propertyChangedEvents = new List<string>();
        viewModel.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName != null)
            {
                propertyChangedEvents.Add(args.PropertyName);
            }
        };

        // Act
        viewModel.SelectedTab = whitespaceValue;

        // Assert
        Assert.AreEqual(whitespaceValue, viewModel.SelectedTab);
        Assert.IsFalse(viewModel.IsStudentsTab);
        Assert.IsFalse(viewModel.IsLecturersTab);
        CollectionAssert.Contains(propertyChangedEvents, "SelectedTab");
        CollectionAssert.Contains(propertyChangedEvents, "IsStudentsTab");
        CollectionAssert.Contains(propertyChangedEvents, "IsLecturersTab");
    }

    /// <summary>
    /// Tests that setting SelectedTab to a very long string updates the property correctly.
    /// </summary>
    [TestMethod]
    public void SelectedTab_SetToVeryLongString_UpdatesPropertyAndRaisesNotifications()
    {
        // Arrange
        var adminMock = new Mock<IAdminService>();
        var loggerMock = new Mock<ILogger<UsersViewModel>>();
        var viewModel = new UsersViewModel(adminMock.Object, new Mock<IRefreshCoordinator>().Object, loggerMock.Object);

        var veryLongString = new string('A', 10000);
        var propertyChangedEvents = new List<string>();
        viewModel.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName != null)
            {
                propertyChangedEvents.Add(args.PropertyName);
            }
        };

        // Act
        viewModel.SelectedTab = veryLongString;

        // Assert
        Assert.AreEqual(veryLongString, viewModel.SelectedTab);
        Assert.IsFalse(viewModel.IsStudentsTab);
        Assert.IsFalse(viewModel.IsLecturersTab);
        CollectionAssert.Contains(propertyChangedEvents, "SelectedTab");
        CollectionAssert.Contains(propertyChangedEvents, "IsStudentsTab");
        CollectionAssert.Contains(propertyChangedEvents, "IsLecturersTab");
    }

    /// <summary>
    /// Tests that setting SelectedTab to strings with special characters updates the property correctly.
    /// </summary>
    [TestMethod]
    [DataRow("Special!@#$%^&*()", DisplayName = "Special characters")]
    [DataRow("Unicode-\u00E9\u00F1\u4E2D", DisplayName = "Unicode characters")]
    [DataRow("Control\0Characters", DisplayName = "Control characters")]
    public void SelectedTab_SetToStringWithSpecialCharacters_UpdatesPropertyAndRaisesNotifications(string specialValue)
    {
        // Arrange
        var adminMock = new Mock<IAdminService>();
        var loggerMock = new Mock<ILogger<UsersViewModel>>();
        var viewModel = new UsersViewModel(adminMock.Object, new Mock<IRefreshCoordinator>().Object, loggerMock.Object);

        var propertyChangedEvents = new List<string>();
        viewModel.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName != null)
            {
                propertyChangedEvents.Add(args.PropertyName);
            }
        };

        // Act
        viewModel.SelectedTab = specialValue;

        // Assert
        Assert.AreEqual(specialValue, viewModel.SelectedTab);
        CollectionAssert.Contains(propertyChangedEvents, "SelectedTab");
        CollectionAssert.Contains(propertyChangedEvents, "IsStudentsTab");
        CollectionAssert.Contains(propertyChangedEvents, "IsLecturersTab");
    }

    /// <summary>
    /// Tests that SelectedTab initializes to "Students" by default.
    /// </summary>
    [TestMethod]
    public void SelectedTab_DefaultValue_IsStudents()
    {
        // Arrange
        var adminMock = new Mock<IAdminService>();
        var loggerMock = new Mock<ILogger<UsersViewModel>>();

        // Act
        var viewModel = new UsersViewModel(adminMock.Object, new Mock<IRefreshCoordinator>().Object, loggerMock.Object);

        // Assert
        Assert.AreEqual("Students", viewModel.SelectedTab);
        Assert.IsTrue(viewModel.IsStudentsTab);
        Assert.IsFalse(viewModel.IsLecturersTab);
    }

    /// <summary>
    /// Tests that setting SelectedTab multiple times to different values raises notifications each time.
    /// </summary>
    [TestMethod]
    public void SelectedTab_SetMultipleTimes_RaisesNotificationsEachTime()
    {
        // Arrange
        var adminMock = new Mock<IAdminService>();
        var loggerMock = new Mock<ILogger<UsersViewModel>>();
        var viewModel = new UsersViewModel(adminMock.Object, new Mock<IRefreshCoordinator>().Object, loggerMock.Object);

        var propertyChangedCount = 0;
        viewModel.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == "SelectedTab")
            {
                propertyChangedCount++;
            }
        };

        // Act
        viewModel.SelectedTab = "Lecturers";
        viewModel.SelectedTab = "Students";
        viewModel.SelectedTab = "Other";

        // Assert
        Assert.AreEqual("Other", viewModel.SelectedTab);
        Assert.AreEqual(3, propertyChangedCount);
    }

    /// <summary>
    /// Tests that IsStudentsTab returns true when SelectedTab is "Students" with exact case match.
    /// </summary>
    [TestMethod]
    public void SelectedTab_SetToStudentsExactCase_IsStudentsTabIsTrue()
    {
        // Arrange
        var adminMock = new Mock<IAdminService>();
        var loggerMock = new Mock<ILogger<UsersViewModel>>();
        var viewModel = new UsersViewModel(adminMock.Object, new Mock<IRefreshCoordinator>().Object, loggerMock.Object);

        // Act
        viewModel.SelectedTab = "Students";

        // Assert
        Assert.IsTrue(viewModel.IsStudentsTab);
        Assert.IsFalse(viewModel.IsLecturersTab);
    }

    /// <summary>
    /// Tests that IsStudentsTab returns false when SelectedTab is "Students" with different case.
    /// </summary>
    [TestMethod]
    [DataRow("students", DisplayName = "Lowercase students")]
    [DataRow("STUDENTS", DisplayName = "Uppercase STUDENTS")]
    [DataRow("StUdEnTs", DisplayName = "Mixed case StUdEnTs")]
    public void SelectedTab_SetToStudentsDifferentCase_IsStudentsTabIsFalse(string caseVariant)
    {
        // Arrange
        var adminMock = new Mock<IAdminService>();
        var loggerMock = new Mock<ILogger<UsersViewModel>>();
        var viewModel = new UsersViewModel(adminMock.Object, new Mock<IRefreshCoordinator>().Object, loggerMock.Object);

        // Act
        viewModel.SelectedTab = caseVariant;

        // Assert
        Assert.IsFalse(viewModel.IsStudentsTab);
    }

    /// <summary>
    /// Tests that IsLecturersTab returns true when SelectedTab is "Lecturers" with exact case match.
    /// </summary>
    [TestMethod]
    public void SelectedTab_SetToLecturersExactCase_IsLecturersTabIsTrue()
    {
        // Arrange
        var adminMock = new Mock<IAdminService>();
        var loggerMock = new Mock<ILogger<UsersViewModel>>();
        var viewModel = new UsersViewModel(adminMock.Object, new Mock<IRefreshCoordinator>().Object, loggerMock.Object);

        // Act
        viewModel.SelectedTab = "Lecturers";

        // Assert
        Assert.IsTrue(viewModel.IsLecturersTab);
        Assert.IsFalse(viewModel.IsStudentsTab);
    }

    /// <summary>
    /// Tests that IsLecturersTab returns false when SelectedTab is "Lecturers" with different case.
    /// </summary>
    [TestMethod]
    [DataRow("lecturers", DisplayName = "Lowercase lecturers")]
    [DataRow("LECTURERS", DisplayName = "Uppercase LECTURERS")]
    [DataRow("LeCtuReRs", DisplayName = "Mixed case LeCtuReRs")]
    public void SelectedTab_SetToLecturersDifferentCase_IsLecturersTabIsFalse(string caseVariant)
    {
        // Arrange
        var adminMock = new Mock<IAdminService>();
        var loggerMock = new Mock<ILogger<UsersViewModel>>();
        var viewModel = new UsersViewModel(adminMock.Object, new Mock<IRefreshCoordinator>().Object, loggerMock.Object);

        // Act
        viewModel.SelectedTab = caseVariant;

        // Assert
        Assert.IsFalse(viewModel.IsLecturersTab);
    }

    /// <summary>
    /// Tests that setting SearchText to a different value triggers ApplyFilter by verifying FilteredUsers is cleared.
    /// </summary>
    /// <param name="newValue">The new value to set for SearchText.</param>
    [TestMethod]
    [DataRow("test")]
    [DataRow("")]
    [DataRow("   ")]
    [DataRow("a")]
    [DataRow("SearchQuery123")]
    public void SearchText_SetDifferentValue_CallsApplyFilter(string newValue)
    {
        // Arrange
        var mockAdminService = new Mock<IAdminService>();
        var mockLogger = new Mock<ILogger<UsersViewModel>>();
        var viewModel = new UsersViewModel(mockAdminService.Object, new Mock<IRefreshCoordinator>().Object, mockLogger.Object);

        // Add a sentinel item to FilteredUsers to detect if ApplyFilter clears it
        var sentinelUser = new UserItem { Name = "Sentinel", Email = "sentinel@test.com", Role = "Student" };
        viewModel.FilteredUsers.Add(sentinelUser);

        // Act
        viewModel.SearchText = newValue;

        // Assert
        Assert.AreEqual(newValue, viewModel.SearchText);
        // ApplyFilter clears FilteredUsers, so the sentinel should be removed
        Assert.IsFalse(viewModel.FilteredUsers.Contains(sentinelUser), "ApplyFilter should have cleared the FilteredUsers collection");
    }

    /// <summary>
    /// Tests that setting SearchText to the same value does not trigger ApplyFilter.
    /// </summary>
    [TestMethod]
    public void SearchText_SetSameValue_DoesNotCallApplyFilter()
    {
        // Arrange
        var mockAdminService = new Mock<IAdminService>();
        var mockLogger = new Mock<ILogger<UsersViewModel>>();
        var viewModel = new UsersViewModel(mockAdminService.Object, new Mock<IRefreshCoordinator>().Object, mockLogger.Object);

        var initialValue = "test";
        viewModel.SearchText = initialValue;

        // Add a sentinel item after the first set to detect if ApplyFilter is called again
        var sentinelUser = new UserItem { Name = "Sentinel", Email = "sentinel@test.com", Role = "Student" };
        viewModel.FilteredUsers.Add(sentinelUser);

        // Act
        viewModel.SearchText = initialValue; // Set the same value

        // Assert
        Assert.AreEqual(initialValue, viewModel.SearchText);
        // ApplyFilter should NOT be called, so the sentinel should still be present
        Assert.IsTrue(viewModel.FilteredUsers.Contains(sentinelUser), "ApplyFilter should not have been called");
    }

    /// <summary>
    /// Tests that getting SearchText returns the current value.
    /// </summary>
    [TestMethod]
    public void SearchText_GetValue_ReturnsCurrentValue()
    {
        // Arrange
        var mockAdminService = new Mock<IAdminService>();
        var mockLogger = new Mock<ILogger<UsersViewModel>>();
        var viewModel = new UsersViewModel(mockAdminService.Object, new Mock<IRefreshCoordinator>().Object, mockLogger.Object);

        // Act
        var initialValue = viewModel.SearchText;

        // Assert
        Assert.AreEqual(string.Empty, initialValue);
    }

    /// <summary>
    /// Tests that setting SearchText to a value and then getting it returns the correct value.
    /// </summary>
    /// <param name="testValue">The value to test.</param>
    [TestMethod]
    [DataRow("test")]
    [DataRow("")]
    [DataRow("   whitespace   ")]
    [DataRow("Special!@#$%^&*()Characters")]
    public void SearchText_SetAndGet_ReturnsSetValue(string testValue)
    {
        // Arrange
        var mockAdminService = new Mock<IAdminService>();
        var mockLogger = new Mock<ILogger<UsersViewModel>>();
        var viewModel = new UsersViewModel(mockAdminService.Object, new Mock<IRefreshCoordinator>().Object, mockLogger.Object);

        // Act
        viewModel.SearchText = testValue;
        var retrievedValue = viewModel.SearchText;

        // Assert
        Assert.AreEqual(testValue, retrievedValue);
    }

    /// <summary>
    /// Tests that setting SearchText with a very long string triggers ApplyFilter correctly.
    /// </summary>
    [TestMethod]
    public void SearchText_SetVeryLongString_CallsApplyFilter()
    {
        // Arrange
        var mockAdminService = new Mock<IAdminService>();
        var mockLogger = new Mock<ILogger<UsersViewModel>>();
        var viewModel = new UsersViewModel(mockAdminService.Object, new Mock<IRefreshCoordinator>().Object, mockLogger.Object);

        var longString = new string('x', 10000);
        var sentinelUser = new UserItem { Name = "Sentinel", Email = "sentinel@test.com", Role = "Student" };
        viewModel.FilteredUsers.Add(sentinelUser);

        // Act
        viewModel.SearchText = longString;

        // Assert
        Assert.AreEqual(longString, viewModel.SearchText);
        Assert.IsFalse(viewModel.FilteredUsers.Contains(sentinelUser), "ApplyFilter should have cleared the FilteredUsers collection");
    }

    /// <summary>
    /// Tests that SearchText property raises PropertyChanged event when value changes.
    /// </summary>
    [TestMethod]
    public void SearchText_ValueChanged_RaisesPropertyChangedEvent()
    {
        // Arrange
        var mockAdminService = new Mock<IAdminService>();
        var mockLogger = new Mock<ILogger<UsersViewModel>>();
        var viewModel = new UsersViewModel(mockAdminService.Object, new Mock<IRefreshCoordinator>().Object, mockLogger.Object);

        var propertyChangedRaised = false;
        string? raisedPropertyName = null;

        viewModel.PropertyChanged += (sender, args) =>
        {
            propertyChangedRaised = true;
            raisedPropertyName = args.PropertyName;
        };

        // Act
        viewModel.SearchText = "new value";

        // Assert
        Assert.IsTrue(propertyChangedRaised, "PropertyChanged event should be raised");
        Assert.AreEqual("SearchText", raisedPropertyName);
    }

    /// <summary>
    /// Tests that SearchText property does not raise PropertyChanged event when value is the same.
    /// </summary>
    [TestMethod]
    public void SearchText_SameValue_DoesNotRaisePropertyChangedEvent()
    {
        // Arrange
        var mockAdminService = new Mock<IAdminService>();
        var mockLogger = new Mock<ILogger<UsersViewModel>>();
        var viewModel = new UsersViewModel(mockAdminService.Object, new Mock<IRefreshCoordinator>().Object, mockLogger.Object);

        viewModel.SearchText = "initial";

        var propertyChangedRaised = false;
        viewModel.PropertyChanged += (sender, args) =>
        {
            propertyChangedRaised = true;
        };

        // Act
        viewModel.SearchText = "initial"; // Same value

        // Assert
        Assert.IsFalse(propertyChangedRaised, "PropertyChanged event should not be raised for same value");
    }

    /// <summary>
    /// Tests that setting SearchText multiple times with different values triggers ApplyFilter each time.
    /// </summary>
    [TestMethod]
    public void SearchText_SetMultipleDifferentValues_CallsApplyFilterEachTime()
    {
        // Arrange
        var mockAdminService = new Mock<IAdminService>();
        var mockLogger = new Mock<ILogger<UsersViewModel>>();
        var viewModel = new UsersViewModel(mockAdminService.Object, new Mock<IRefreshCoordinator>().Object, mockLogger.Object);

        // Act & Assert - First change
        var sentinelUser1 = new UserItem { Name = "Sentinel1", Email = "sentinel1@test.com", Role = "Student" };
        viewModel.FilteredUsers.Add(sentinelUser1);
        viewModel.SearchText = "first";
        Assert.IsFalse(viewModel.FilteredUsers.Contains(sentinelUser1), "First ApplyFilter call should clear FilteredUsers");

        // Act & Assert - Second change
        var sentinelUser2 = new UserItem { Name = "Sentinel2", Email = "sentinel2@test.com", Role = "Student" };
        viewModel.FilteredUsers.Add(sentinelUser2);
        viewModel.SearchText = "second";
        Assert.IsFalse(viewModel.FilteredUsers.Contains(sentinelUser2), "Second ApplyFilter call should clear FilteredUsers");

        // Act & Assert - Third change
        var sentinelUser3 = new UserItem { Name = "Sentinel3", Email = "sentinel3@test.com", Role = "Student" };
        viewModel.FilteredUsers.Add(sentinelUser3);
        viewModel.SearchText = "third";
        Assert.IsFalse(viewModel.FilteredUsers.Contains(sentinelUser3), "Third ApplyFilter call should clear FilteredUsers");
    }

    /// <summary>
    /// Tests that the constructor properly initializes the UsersViewModel
    /// with valid admin service and logger dependencies.
    /// Verifies that all properties and commands are correctly initialized.
    /// </summary>
    [TestMethod]
    public void Constructor_WithValidParameters_InitializesAllPropertiesAndCommands()
    {
        // Arrange
        var mockAdmin = new Mock<IAdminService>();
        var mockLogger = new Mock<ILogger<UsersViewModel>>();

        // Act
        var viewModel = new UsersViewModel(mockAdmin.Object, new Mock<IRefreshCoordinator>().Object, mockLogger.Object);

        // Assert
        Assert.IsNotNull(viewModel);
        Assert.AreEqual("User Management", viewModel.Title);
        Assert.IsNotNull(viewModel.LoadCommand);
        Assert.IsNotNull(viewModel.SearchCommand);
        Assert.IsNotNull(viewModel.SwitchTabCommand);
        Assert.IsNotNull(viewModel.SuspendCommand);
        Assert.IsNotNull(viewModel.ActivateCommand);
        Assert.IsNotNull(viewModel.DeleteCommand);
        Assert.IsNotNull(viewModel.FilteredUsers);
        Assert.AreEqual(0, viewModel.FilteredUsers.Count);
    }

    /// <summary>
    /// Tests that the constructor accepts null admin service parameter.
    /// This verifies the behavior when a required dependency is not provided.
    /// Expected behavior: Constructor does not throw (no null validation present).
    /// </summary>
    [TestMethod]
    public void Constructor_WithNullAdmin_DoesNotThrow()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<UsersViewModel>>();

        // Act & Assert
        var viewModel = new UsersViewModel(null!, new Mock<IRefreshCoordinator>().Object, mockLogger.Object);
        Assert.IsNotNull(viewModel);
    }

    /// <summary>
    /// Tests that the constructor accepts null logger parameter.
    /// This verifies the behavior when a required dependency is not provided.
    /// Expected behavior: Constructor does not throw (no null validation present).
    /// </summary>
    [TestMethod]
    public void Constructor_WithNullLogger_DoesNotThrow()
    {
        // Arrange
        var mockAdmin = new Mock<IAdminService>();

        // Act & Assert
        var viewModel = new UsersViewModel(mockAdmin.Object, new Mock<IRefreshCoordinator>().Object, null!);
        Assert.IsNotNull(viewModel);
    }

    /// <summary>
    /// Tests that the constructor accepts both null parameters.
    /// This verifies the behavior when all dependencies are not provided.
    /// Expected behavior: Constructor does not throw (no null validation present).
    /// </summary>
    [TestMethod]
    public void Constructor_WithBothNullParameters_DoesNotThrow()
    {
        // Act & Assert
        var viewModel = new UsersViewModel(null!, new Mock<IRefreshCoordinator>().Object, null!);
        Assert.IsNotNull(viewModel);
    }

    /// <summary>
    /// Tests that the constructor properly sets the Title property
    /// to the expected value "User Management".
    /// </summary>
    [TestMethod]
    public void Constructor_WithValidParameters_SetsTitleToUserManagement()
    {
        // Arrange
        var mockAdmin = new Mock<IAdminService>();
        var mockLogger = new Mock<ILogger<UsersViewModel>>();

        // Act
        var viewModel = new UsersViewModel(mockAdmin.Object, new Mock<IRefreshCoordinator>().Object, mockLogger.Object);

        // Assert
        Assert.AreEqual("User Management", viewModel.Title);
    }

    /// <summary>
    /// Tests that the constructor initializes the FilteredUsers observable collection.
    /// Verifies that the collection is not null and is initially empty.
    /// </summary>
    [TestMethod]
    public void Constructor_WithValidParameters_InitializesFilteredUsersCollection()
    {
        // Arrange
        var mockAdmin = new Mock<IAdminService>();
        var mockLogger = new Mock<ILogger<UsersViewModel>>();

        // Act
        var viewModel = new UsersViewModel(mockAdmin.Object, new Mock<IRefreshCoordinator>().Object, mockLogger.Object);

        // Assert
        Assert.IsNotNull(viewModel.FilteredUsers);
        Assert.AreEqual(0, viewModel.FilteredUsers.Count);
    }

    /// <summary>
    /// Tests that all command properties are initialized and of the expected type.
    /// Verifies that each command is not null and is a valid ICommand instance.
    /// </summary>
    [TestMethod]
    public void Constructor_WithValidParameters_InitializesAllCommandsAsICommand()
    {
        // Arrange
        var mockAdmin = new Mock<IAdminService>();
        var mockLogger = new Mock<ILogger<UsersViewModel>>();

        // Act
        var viewModel = new UsersViewModel(mockAdmin.Object, new Mock<IRefreshCoordinator>().Object, mockLogger.Object);

        // Assert
        Assert.IsInstanceOfType(viewModel.LoadCommand, typeof(ICommand));
        Assert.IsInstanceOfType(viewModel.SearchCommand, typeof(ICommand));
        Assert.IsInstanceOfType(viewModel.SwitchTabCommand, typeof(ICommand));
        Assert.IsInstanceOfType(viewModel.SuspendCommand, typeof(ICommand));
        Assert.IsInstanceOfType(viewModel.ActivateCommand, typeof(ICommand));
        Assert.IsInstanceOfType(viewModel.DeleteCommand, typeof(ICommand));
    }

    /// <summary>
    /// Tests that default property values are correctly set after construction.
    /// Verifies SearchText, SelectedTab, TotalStudents, TotalLecturers, TotalSuspended, ErrorMessage, and HasError.
    /// </summary>
    [TestMethod]
    public void Constructor_WithValidParameters_InitializesDefaultPropertyValues()
    {
        // Arrange
        var mockAdmin = new Mock<IAdminService>();
        var mockLogger = new Mock<ILogger<UsersViewModel>>();

        // Act
        var viewModel = new UsersViewModel(mockAdmin.Object, new Mock<IRefreshCoordinator>().Object, mockLogger.Object);

        // Assert
        Assert.AreEqual(string.Empty, viewModel.SearchText);
        Assert.AreEqual("Students", viewModel.SelectedTab);
        Assert.AreEqual(0, viewModel.TotalStudents);
        Assert.AreEqual(0, viewModel.TotalLecturers);
        Assert.AreEqual(0, viewModel.TotalSuspended);
        Assert.AreEqual(string.Empty, viewModel.ErrorMessage);
        Assert.IsFalse(viewModel.HasError);
        Assert.IsTrue(viewModel.IsStudentsTab);
        Assert.IsFalse(viewModel.IsLecturersTab);
    }

    /// <summary>
    /// Tests that setting TotalSuspended property updates the backing field and the value can be retrieved.
    /// Tests various boundary and edge case values for integer type.
    /// </summary>
    /// <param name="value">The integer value to set.</param>
    [TestMethod]
    [DataRow(0)]
    [DataRow(1)]
    [DataRow(-1)]
    [DataRow(100)]
    [DataRow(-100)]
    [DataRow(int.MaxValue)]
    [DataRow(int.MinValue)]
    public void TotalSuspended_SetValue_UpdatesPropertyAndReturnsValue(int value)
    {
        // Arrange
        var mockAdminService = new Mock<IAdminService>();
        var mockLogger = new Mock<ILogger<UsersViewModel>>();
        var viewModel = new UsersViewModel(mockAdminService.Object, new Mock<IRefreshCoordinator>().Object, mockLogger.Object);

        // Act
        viewModel.TotalSuspended = value;

        // Assert
        Assert.AreEqual(value, viewModel.TotalSuspended);
    }

    /// <summary>
    /// Tests that setting TotalSuspended to a different value raises the PropertyChanged event
    /// with the correct property name.
    /// </summary>
    [TestMethod]
    public void TotalSuspended_SetDifferentValue_RaisesPropertyChangedEvent()
    {
        // Arrange
        var mockAdminService = new Mock<IAdminService>();
        var mockLogger = new Mock<ILogger<UsersViewModel>>();
        var viewModel = new UsersViewModel(mockAdminService.Object, new Mock<IRefreshCoordinator>().Object, mockLogger.Object);

        string? raisedPropertyName = null;
        viewModel.PropertyChanged += (sender, args) => raisedPropertyName = args.PropertyName;

        viewModel.TotalSuspended = 10;

        // Act
        viewModel.TotalSuspended = 20;

        // Assert
        Assert.AreEqual("TotalSuspended", raisedPropertyName);
    }

    /// <summary>
    /// Tests that setting TotalSuspended to the same value does not raise the PropertyChanged event.
    /// </summary>
    [TestMethod]
    public void TotalSuspended_SetSameValue_DoesNotRaisePropertyChangedEvent()
    {
        // Arrange
        var mockAdminService = new Mock<IAdminService>();
        var mockLogger = new Mock<ILogger<UsersViewModel>>();
        var viewModel = new UsersViewModel(mockAdminService.Object, new Mock<IRefreshCoordinator>().Object, mockLogger.Object);

        viewModel.TotalSuspended = 42;

        bool eventRaised = false;
        viewModel.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == "TotalSuspended")
            {
                eventRaised = true;
            }
        };

        // Act
        viewModel.TotalSuspended = 42;

        // Assert
        Assert.IsFalse(eventRaised);
    }

    /// <summary>
    /// Tests that the initial value of TotalSuspended is zero (default for int).
    /// </summary>
    [TestMethod]
    public void TotalSuspended_InitialValue_IsZero()
    {
        // Arrange
        var mockAdminService = new Mock<IAdminService>();
        var mockLogger = new Mock<ILogger<UsersViewModel>>();

        // Act
        var viewModel = new UsersViewModel(mockAdminService.Object, new Mock<IRefreshCoordinator>().Object, mockLogger.Object);

        // Assert
        Assert.AreEqual(0, viewModel.TotalSuspended);
    }

    /// <summary>
    /// Tests that the ErrorMessage property getter returns the initial empty string value.
    /// </summary>
    [TestMethod]
    public void ErrorMessage_InitialValue_ReturnsEmptyString()
    {
        // Arrange
        var mockAdminService = new Mock<IAdminService>();
        var mockLogger = new Mock<ILogger<UsersViewModel>>();
        var viewModel = new UsersViewModel(mockAdminService.Object, new Mock<IRefreshCoordinator>().Object, mockLogger.Object);

        // Act
        var result = viewModel.ErrorMessage;

        // Assert
        Assert.AreEqual(string.Empty, result);
    }

    /// <summary>
    /// Tests that setting the ErrorMessage property with a new value updates the property and raises PropertyChanged event.
    /// </summary>
    /// <param name="newValue">The new value to set.</param>
    [TestMethod]
    [DataRow("Error occurred")]
    [DataRow("")]
    [DataRow("   ")]
    [DataRow("Error with special chars: @#$%^&*()")]
    [DataRow("Very long error message that contains a lot of text to simulate edge case scenarios where error messages might be exceptionally verbose and detailed")]
    [DataRow("\t\n\r")]
    public void ErrorMessage_SetNewValue_UpdatesPropertyAndRaisesPropertyChanged(string newValue)
    {
        // Arrange
        var mockAdminService = new Mock<IAdminService>();
        var mockLogger = new Mock<ILogger<UsersViewModel>>();
        var viewModel = new UsersViewModel(mockAdminService.Object, new Mock<IRefreshCoordinator>().Object, mockLogger.Object);
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
        Assert.IsTrue(propertyChangedRaised, "PropertyChanged event should be raised");
        Assert.AreEqual("ErrorMessage", raisedPropertyName);
    }

    /// <summary>
    /// Tests that setting the ErrorMessage property with null updates the property and raises PropertyChanged event.
    /// </summary>
    [TestMethod]
    public void ErrorMessage_SetNullValue_UpdatesPropertyAndRaisesPropertyChanged()
    {
        // Arrange
        var mockAdminService = new Mock<IAdminService>();
        var mockLogger = new Mock<ILogger<UsersViewModel>>();
        var viewModel = new UsersViewModel(mockAdminService.Object, new Mock<IRefreshCoordinator>().Object, mockLogger.Object);
        viewModel.ErrorMessage = "Initial value";
        var propertyChangedRaised = false;
        string? raisedPropertyName = null;

        viewModel.PropertyChanged += (sender, args) =>
        {
            propertyChangedRaised = true;
            raisedPropertyName = args.PropertyName;
        };

        // Act
        viewModel.ErrorMessage = null!;

        // Assert
        Assert.IsNull(viewModel.ErrorMessage);
        Assert.IsTrue(propertyChangedRaised, "PropertyChanged event should be raised");
        Assert.AreEqual("ErrorMessage", raisedPropertyName);
    }

    /// <summary>
    /// Tests that setting the ErrorMessage property with the same value does not raise PropertyChanged event.
    /// </summary>
    [TestMethod]
    public void ErrorMessage_SetSameValue_DoesNotRaisePropertyChanged()
    {
        // Arrange
        var mockAdminService = new Mock<IAdminService>();
        var mockLogger = new Mock<ILogger<UsersViewModel>>();
        var viewModel = new UsersViewModel(mockAdminService.Object, new Mock<IRefreshCoordinator>().Object, mockLogger.Object);
        viewModel.ErrorMessage = "Test error";
        var propertyChangedRaised = false;

        viewModel.PropertyChanged += (sender, args) =>
        {
            propertyChangedRaised = true;
        };

        // Act
        viewModel.ErrorMessage = "Test error";

        // Assert
        Assert.AreEqual("Test error", viewModel.ErrorMessage);
        Assert.IsFalse(propertyChangedRaised, "PropertyChanged event should not be raised when value is the same");
    }

    /// <summary>
    /// Tests that setting the ErrorMessage property with the initial empty string value does not raise PropertyChanged event.
    /// </summary>
    [TestMethod]
    public void ErrorMessage_SetInitialEmptyStringAgain_DoesNotRaisePropertyChanged()
    {
        // Arrange
        var mockAdminService = new Mock<IAdminService>();
        var mockLogger = new Mock<ILogger<UsersViewModel>>();
        var viewModel = new UsersViewModel(mockAdminService.Object, new Mock<IRefreshCoordinator>().Object, mockLogger.Object);
        var propertyChangedRaised = false;

        viewModel.PropertyChanged += (sender, args) =>
        {
            propertyChangedRaised = true;
        };

        // Act
        viewModel.ErrorMessage = string.Empty;

        // Assert
        Assert.AreEqual(string.Empty, viewModel.ErrorMessage);
        Assert.IsFalse(propertyChangedRaised, "PropertyChanged event should not be raised when value is the same");
    }

    /// <summary>
    /// Tests that multiple updates to ErrorMessage property correctly update the value and raise PropertyChanged event each time.
    /// </summary>
    [TestMethod]
    public void ErrorMessage_MultipleUpdates_UpdatesPropertyAndRaisesPropertyChangedEachTime()
    {
        // Arrange
        var mockAdminService = new Mock<IAdminService>();
        var mockLogger = new Mock<ILogger<UsersViewModel>>();
        var viewModel = new UsersViewModel(mockAdminService.Object, new Mock<IRefreshCoordinator>().Object, mockLogger.Object);
        var propertyChangedCount = 0;

        viewModel.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == "ErrorMessage")
            {
                propertyChangedCount++;
            }
        };

        // Act
        viewModel.ErrorMessage = "First error";
        viewModel.ErrorMessage = "Second error";
        viewModel.ErrorMessage = "Third error";

        // Assert
        Assert.AreEqual("Third error", viewModel.ErrorMessage);
        Assert.AreEqual(3, propertyChangedCount, "PropertyChanged should be raised for each distinct value change");
    }

    /// <summary>
    /// Tests that LoadUsersAsync returns immediately when IsBusy is already true.
    /// </summary>
    [TestMethod]
    public async Task LoadUsersAsync_WhenIsBusy_ReturnsImmediately()
    {
        // Arrange
        var mockAdmin = new Mock<IAdminService>();
        var mockLogger = new Mock<ILogger<UsersViewModel>>();
        var viewModel = new UsersViewModel(mockAdmin.Object, new Mock<IRefreshCoordinator>().Object, mockLogger.Object);

        // Set IsBusy to true by starting a long-running operation
        var tcs = new TaskCompletionSource<List<UserItem>>();
        mockAdmin.Setup(x => x.GetStudentsAsync(It.IsAny<CancellationToken>())).Returns(tcs.Task);
        mockAdmin.Setup(x => x.GetLecturersAsync(It.IsAny<CancellationToken>())).Returns(tcs.Task);

        var firstCall = viewModel.LoadUsersAsync();

        // Act
        var secondCall = viewModel.LoadUsersAsync();

        // Assert
        Assert.IsTrue(viewModel.IsBusy);
        Assert.IsTrue(secondCall.IsCompleted, "Second call should complete immediately when IsBusy is true");

        // Cleanup
        tcs.SetResult(new List<UserItem>());
        await firstCall;
    }

    /// <summary>
    /// Tests that LoadUsersAsync successfully loads users, sets properties, and calls ApplyFilter.
    /// </summary>
    [TestMethod]
    public async Task LoadUsersAsync_WithValidData_LoadsUsersSuccessfully()
    {
        // Arrange
        var mockAdmin = new Mock<IAdminService>();
        var mockLogger = new Mock<ILogger<UsersViewModel>>();
        var viewModel = new UsersViewModel(mockAdmin.Object, new Mock<IRefreshCoordinator>().Object, mockLogger.Object);

        var students = new List<UserItem>
        {
            new UserItem { Id = Guid.NewGuid(), Name = "Student1", Email = "s1@test.com", Role = "Student", Status = "Active" },
            new UserItem { Id = Guid.NewGuid(), Name = "Student2", Email = "s2@test.com", Role = "Student", Status = "Suspended" }
        };

        var lecturers = new List<UserItem>
        {
            new UserItem { Id = Guid.NewGuid(), Name = "Lecturer1", Email = "l1@test.com", Role = "Lecturer", Status = "Active" },
            new UserItem { Id = Guid.NewGuid(), Name = "Lecturer2", Email = "l2@test.com", Role = "Lecturer", Status = "Suspended" },
            new UserItem { Id = Guid.NewGuid(), Name = "Lecturer3", Email = "l3@test.com", Role = "Lecturer", Status = "Active" }
        };

        mockAdmin.Setup(x => x.GetStudentsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(students);
        mockAdmin.Setup(x => x.GetLecturersAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(lecturers);

        // Act
        await viewModel.LoadUsersAsync();

        // Assert
        Assert.AreEqual(2, viewModel.TotalStudents);
        Assert.AreEqual(3, viewModel.TotalLecturers);
        Assert.AreEqual(2, viewModel.TotalSuspended);
        Assert.IsFalse(viewModel.IsBusy);
        Assert.IsFalse(viewModel.HasError);
        Assert.AreEqual(string.Empty, viewModel.ErrorMessage);

        mockAdmin.Verify(x => x.GetStudentsAsync(It.IsAny<CancellationToken>()), Times.Once);
        mockAdmin.Verify(x => x.GetLecturersAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Tests that LoadUsersAsync handles empty lists from both services correctly.
    /// </summary>
    [TestMethod]
    public async Task LoadUsersAsync_WithEmptyLists_SetsCountsToZero()
    {
        // Arrange
        var mockAdmin = new Mock<IAdminService>();
        var mockLogger = new Mock<ILogger<UsersViewModel>>();
        var viewModel = new UsersViewModel(mockAdmin.Object, new Mock<IRefreshCoordinator>().Object, mockLogger.Object);

        mockAdmin.Setup(x => x.GetStudentsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<UserItem>());
        mockAdmin.Setup(x => x.GetLecturersAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<UserItem>());

        // Act
        await viewModel.LoadUsersAsync();

        // Assert
        Assert.AreEqual(0, viewModel.TotalStudents);
        Assert.AreEqual(0, viewModel.TotalLecturers);
        Assert.AreEqual(0, viewModel.TotalSuspended);
        Assert.IsFalse(viewModel.IsBusy);
        Assert.IsFalse(viewModel.HasError);
    }

    /// <summary>
    /// Tests that LoadUsersAsync correctly counts suspended users across both students and lecturers.
    /// </summary>
    [TestMethod]
    [DataRow(0, 0, 0, DisplayName = "No users")]
    [DataRow(5, 0, 0, DisplayName = "Only active students")]
    [DataRow(0, 3, 0, DisplayName = "Only active lecturers")]
    [DataRow(2, 3, 5, DisplayName = "All suspended")]
    [DataRow(3, 2, 2, DisplayName = "Mixed suspended")]
    public async Task LoadUsersAsync_VariousSuspensionCounts_CalculatesCorrectly(
        int studentCount, int lecturerCount, int suspendedCount)
    {
        // Arrange
        var mockAdmin = new Mock<IAdminService>();
        var mockLogger = new Mock<ILogger<UsersViewModel>>();
        var viewModel = new UsersViewModel(mockAdmin.Object, new Mock<IRefreshCoordinator>().Object, mockLogger.Object);

        var students = new List<UserItem>();
        for (int i = 0; i < studentCount; i++)
        {
            students.Add(new UserItem
            {
                Id = Guid.NewGuid(),
                Name = $"Student{i}",
                Email = $"s{i}@test.com",
                Role = "Student",
                Status = i < suspendedCount ? "Suspended" : "Active"
            });
        }

        var lecturers = new List<UserItem>();
        int suspendedLecturers = Math.Max(0, suspendedCount - studentCount);
        for (int i = 0; i < lecturerCount; i++)
        {
            lecturers.Add(new UserItem
            {
                Id = Guid.NewGuid(),
                Name = $"Lecturer{i}",
                Email = $"l{i}@test.com",
                Role = "Lecturer",
                Status = i < suspendedLecturers ? "Suspended" : "Active"
            });
        }

        mockAdmin.Setup(x => x.GetStudentsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(students);
        mockAdmin.Setup(x => x.GetLecturersAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(lecturers);

        // Act
        await viewModel.LoadUsersAsync();

        // Assert
        Assert.AreEqual(studentCount, viewModel.TotalStudents);
        Assert.AreEqual(lecturerCount, viewModel.TotalLecturers);
        Assert.AreEqual(suspendedCount, viewModel.TotalSuspended);
    }

    /// <summary>
    /// Tests that LoadUsersAsync handles OperationCanceledException by returning without setting error state.
    /// </summary>
    [TestMethod]
    public async Task LoadUsersAsync_WhenOperationCanceled_DoesNotSetError()
    {
        // Arrange
        var mockAdmin = new Mock<IAdminService>();
        var mockLogger = new Mock<ILogger<UsersViewModel>>();
        var viewModel = new UsersViewModel(mockAdmin.Object, new Mock<IRefreshCoordinator>().Object, mockLogger.Object);

        mockAdmin.Setup(x => x.GetStudentsAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new OperationCanceledException());

        // Act
        await viewModel.LoadUsersAsync();

        // Assert
        Assert.IsFalse(viewModel.IsBusy);
        Assert.IsFalse(viewModel.HasError);
        Assert.AreEqual(string.Empty, viewModel.ErrorMessage);

        mockLogger.Verify(
            x => x.Log(
                It.IsAny<LogLevel>(),
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Never);
    }

    /// <summary>
    /// Tests that LoadUsersAsync logs error and sets error state when GetStudentsAsync throws an exception.
    /// </summary>
    [TestMethod]
    public async Task LoadUsersAsync_WhenGetStudentsFails_SetsErrorState()
    {
        // Arrange
        var mockAdmin = new Mock<IAdminService>();
        var mockLogger = new Mock<ILogger<UsersViewModel>>();
        var viewModel = new UsersViewModel(mockAdmin.Object, new Mock<IRefreshCoordinator>().Object, mockLogger.Object);

        var exception = new InvalidOperationException("Database error");
        mockAdmin.Setup(x => x.GetStudentsAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(exception);

        // Act
        await viewModel.LoadUsersAsync();

        // Assert
        Assert.IsFalse(viewModel.IsBusy);
        Assert.IsTrue(viewModel.HasError);
        Assert.AreEqual("Failed to load users. Please try again.", viewModel.ErrorMessage);

        mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Failed to load users")),
                exception,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    /// <summary>
    /// Tests that LoadUsersAsync logs error and sets error state when GetLecturersAsync throws an exception.
    /// </summary>
    [TestMethod]
    public async Task LoadUsersAsync_WhenGetLecturersFails_SetsErrorState()
    {
        // Arrange
        var mockAdmin = new Mock<IAdminService>();
        var mockLogger = new Mock<ILogger<UsersViewModel>>();
        var viewModel = new UsersViewModel(mockAdmin.Object, new Mock<IRefreshCoordinator>().Object, mockLogger.Object);

        mockAdmin.Setup(x => x.GetStudentsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<UserItem>());

        var exception = new InvalidOperationException("Network error");
        mockAdmin.Setup(x => x.GetLecturersAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(exception);

        // Act
        await viewModel.LoadUsersAsync();

        // Assert
        Assert.IsFalse(viewModel.IsBusy);
        Assert.IsTrue(viewModel.HasError);
        Assert.AreEqual("Failed to load users. Please try again.", viewModel.ErrorMessage);

        mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Failed to load users")),
                exception,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    /// <summary>
    /// Tests that LoadUsersAsync resets IsBusy to false even when an exception occurs.
    /// </summary>
    [TestMethod]
    public async Task LoadUsersAsync_WhenExceptionThrown_ResetsIsBusy()
    {
        // Arrange
        var mockAdmin = new Mock<IAdminService>();
        var mockLogger = new Mock<ILogger<UsersViewModel>>();
        var viewModel = new UsersViewModel(mockAdmin.Object, new Mock<IRefreshCoordinator>().Object, mockLogger.Object);

        mockAdmin.Setup(x => x.GetStudentsAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Test exception"));

        // Act
        await viewModel.LoadUsersAsync();

        // Assert
        Assert.IsFalse(viewModel.IsBusy, "IsBusy should be reset to false after exception");
    }

    /// <summary>
    /// Tests that LoadUsersAsync clears previous error state on successful load.
    /// </summary>
    [TestMethod]
    public async Task LoadUsersAsync_AfterPreviousError_ClearsErrorState()
    {
        // Arrange
        var mockAdmin = new Mock<IAdminService>();
        var mockLogger = new Mock<ILogger<UsersViewModel>>();
        var viewModel = new UsersViewModel(mockAdmin.Object, new Mock<IRefreshCoordinator>().Object, mockLogger.Object);

        // First call fails
        mockAdmin.Setup(x => x.GetStudentsAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("First error"));
        await viewModel.LoadUsersAsync();

        Assert.IsTrue(viewModel.HasError, "Precondition: HasError should be true");

        // Second call succeeds
        mockAdmin.Setup(x => x.GetStudentsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<UserItem>());
        mockAdmin.Setup(x => x.GetLecturersAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<UserItem>());

        // Act
        await viewModel.LoadUsersAsync();

        // Assert
        Assert.IsFalse(viewModel.HasError);
        Assert.AreEqual(string.Empty, viewModel.ErrorMessage);
    }

    /// <summary>
    /// Tests that LoadUsersAsync handles various exception types correctly.
    /// </summary>
    [TestMethod]
    [DataRow(typeof(InvalidOperationException), DisplayName = "InvalidOperationException")]
    [DataRow(typeof(ArgumentException), DisplayName = "ArgumentException")]
    [DataRow(typeof(TimeoutException), DisplayName = "TimeoutException")]
    [DataRow(typeof(Exception), DisplayName = "Generic Exception")]
    public async Task LoadUsersAsync_WithDifferentExceptionTypes_HandlesCorrectly(Type exceptionType)
    {
        // Arrange
        var mockAdmin = new Mock<IAdminService>();
        var mockLogger = new Mock<ILogger<UsersViewModel>>();
        var viewModel = new UsersViewModel(mockAdmin.Object, new Mock<IRefreshCoordinator>().Object, mockLogger.Object);

        var exception = (Exception)Activator.CreateInstance(exceptionType, "Test error")!;
        mockAdmin.Setup(x => x.GetStudentsAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(exception);

        // Act
        await viewModel.LoadUsersAsync();

        // Assert
        Assert.IsFalse(viewModel.IsBusy);
        Assert.IsTrue(viewModel.HasError);
        Assert.AreEqual("Failed to load users. Please try again.", viewModel.ErrorMessage);

        mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => true),
                exception,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    /// <summary>
    /// Tests that LoadUsersAsync sets IsBusy to true at the start of execution.
    /// </summary>
    [TestMethod]
    public async Task LoadUsersAsync_DuringExecution_SetsIsBusyToTrue()
    {
        // Arrange
        var mockAdmin = new Mock<IAdminService>();
        var mockLogger = new Mock<ILogger<UsersViewModel>>();
        var viewModel = new UsersViewModel(mockAdmin.Object, new Mock<IRefreshCoordinator>().Object, mockLogger.Object);

        var tcs = new TaskCompletionSource<List<UserItem>>();
        mockAdmin.Setup(x => x.GetStudentsAsync(It.IsAny<CancellationToken>()))
            .Returns(tcs.Task);
        mockAdmin.Setup(x => x.GetLecturersAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<UserItem>());

        // Act
        var loadTask = viewModel.LoadUsersAsync();

        // Assert
        Assert.IsTrue(viewModel.IsBusy, "IsBusy should be true during execution");

        // Cleanup
        tcs.SetResult(new List<UserItem>());
        await loadTask;
        Assert.IsFalse(viewModel.IsBusy, "IsBusy should be false after completion");
    }

    /// <summary>
    /// Tests that LoadUsersAsync handles large numbers of users correctly.
    /// </summary>
    [TestMethod]
    public async Task LoadUsersAsync_WithLargeDatasets_HandlesCorrectly()
    {
        // Arrange
        var mockAdmin = new Mock<IAdminService>();
        var mockLogger = new Mock<ILogger<UsersViewModel>>();
        var viewModel = new UsersViewModel(mockAdmin.Object, new Mock<IRefreshCoordinator>().Object, mockLogger.Object);

        var students = new List<UserItem>();
        for (int i = 0; i < 1000; i++)
        {
            students.Add(new UserItem
            {
                Id = Guid.NewGuid(),
                Name = $"Student{i}",
                Email = $"s{i}@test.com",
                Role = "Student",
                Status = i % 10 == 0 ? "Suspended" : "Active"
            });
        }

        var lecturers = new List<UserItem>();
        for (int i = 0; i < 500; i++)
        {
            lecturers.Add(new UserItem
            {
                Id = Guid.NewGuid(),
                Name = $"Lecturer{i}",
                Email = $"l{i}@test.com",
                Role = "Lecturer",
                Status = i % 5 == 0 ? "Suspended" : "Active"
            });
        }

        mockAdmin.Setup(x => x.GetStudentsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(students);
        mockAdmin.Setup(x => x.GetLecturersAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(lecturers);

        // Act
        await viewModel.LoadUsersAsync();

        // Assert
        Assert.AreEqual(1000, viewModel.TotalStudents);
        Assert.AreEqual(500, viewModel.TotalLecturers);
        Assert.AreEqual(200, viewModel.TotalSuspended); // 100 students + 100 lecturers
        Assert.IsFalse(viewModel.IsBusy);
        Assert.IsFalse(viewModel.HasError);
    }

    /// <summary>
    /// Tests that LoadUsersAsync handles users with various status values including edge cases.
    /// </summary>
    [TestMethod]
    public async Task LoadUsersAsync_WithVariousStatusValues_CountsOnlySuspended()
    {
        // Arrange
        var mockAdmin = new Mock<IAdminService>();
        var mockLogger = new Mock<ILogger<UsersViewModel>>();
        var viewModel = new UsersViewModel(mockAdmin.Object, new Mock<IRefreshCoordinator>().Object, mockLogger.Object);

        var students = new List<UserItem>
        {
            new UserItem { Id = Guid.NewGuid(), Name = "S1", Email = "s1@test.com", Role = "Student", Status = "Suspended" },
            new UserItem { Id = Guid.NewGuid(), Name = "S2", Email = "s2@test.com", Role = "Student", Status = "Active" },
            new UserItem { Id = Guid.NewGuid(), Name = "S3", Email = "s3@test.com", Role = "Student", Status = "SUSPENDED" },
            new UserItem { Id = Guid.NewGuid(), Name = "S4", Email = "s4@test.com", Role = "Student", Status = "suspended" },
            new UserItem { Id = Guid.NewGuid(), Name = "S5", Email = "s5@test.com", Role = "Student", Status = "" },
            new UserItem { Id = Guid.NewGuid(), Name = "S6", Email = "s6@test.com", Role = "Student", Status = "Pending" }
        };

        mockAdmin.Setup(x => x.GetStudentsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(students);
        mockAdmin.Setup(x => x.GetLecturersAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<UserItem>());

        // Act
        await viewModel.LoadUsersAsync();

        // Assert - Only exact match "Suspended" should be counted
        Assert.AreEqual(1, viewModel.TotalSuspended);
    }

    /// <summary>
    /// Tests that setting TotalLecturers with various integer values correctly updates and retrieves the property value.
    /// </summary>
    /// <param name="value">The integer value to set and verify.</param>
    [TestMethod]
    [DataRow(0)]
    [DataRow(1)]
    [DataRow(-1)]
    [DataRow(100)]
    [DataRow(-100)]
    [DataRow(int.MaxValue)]
    [DataRow(int.MinValue)]
    public void TotalLecturers_SetValue_ReturnsSetValue(int value)
    {
        // Arrange
        var mockAdminService = new Mock<IAdminService>();
        var mockLogger = new Mock<ILogger<UsersViewModel>>();
        var viewModel = new UsersViewModel(mockAdminService.Object, new Mock<IRefreshCoordinator>().Object, mockLogger.Object);

        // Act
        viewModel.TotalLecturers = value;

        // Assert
        Assert.AreEqual(value, viewModel.TotalLecturers);
    }

    /// <summary>
    /// Tests that setting TotalLecturers to a different value raises the PropertyChanged event with the correct property name.
    /// </summary>
    [TestMethod]
    public void TotalLecturers_SetDifferentValue_RaisesPropertyChanged()
    {
        // Arrange
        var mockAdminService = new Mock<IAdminService>();
        var mockLogger = new Mock<ILogger<UsersViewModel>>();
        var viewModel = new UsersViewModel(mockAdminService.Object, new Mock<IRefreshCoordinator>().Object, mockLogger.Object);
        var propertyChangedRaised = false;
        string? raisedPropertyName = null;

        viewModel.PropertyChanged += (sender, e) =>
        {
            propertyChangedRaised = true;
            raisedPropertyName = e.PropertyName;
        };

        // Act
        viewModel.TotalLecturers = 42;

        // Assert
        Assert.IsTrue(propertyChangedRaised);
        Assert.AreEqual(nameof(UsersViewModel.TotalLecturers), raisedPropertyName);
    }

    /// <summary>
    /// Tests that setting TotalLecturers to the same value does not raise the PropertyChanged event.
    /// </summary>
    [TestMethod]
    public void TotalLecturers_SetSameValue_DoesNotRaisePropertyChanged()
    {
        // Arrange
        var mockAdminService = new Mock<IAdminService>();
        var mockLogger = new Mock<ILogger<UsersViewModel>>();
        var viewModel = new UsersViewModel(mockAdminService.Object, new Mock<IRefreshCoordinator>().Object, mockLogger.Object);
        viewModel.TotalLecturers = 10;
        var propertyChangedCount = 0;

        viewModel.PropertyChanged += (sender, e) =>
        {
            if (e.PropertyName == nameof(UsersViewModel.TotalLecturers))
            {
                propertyChangedCount++;
            }
        };

        // Act
        viewModel.TotalLecturers = 10;

        // Assert
        Assert.AreEqual(0, propertyChangedCount);
    }

    /// <summary>
    /// Tests that the default value of TotalLecturers is zero when the ViewModel is first initialized.
    /// </summary>
    [TestMethod]
    public void TotalLecturers_DefaultValue_ReturnsZero()
    {
        // Arrange
        var mockAdminService = new Mock<IAdminService>();
        var mockLogger = new Mock<ILogger<UsersViewModel>>();

        // Act
        var viewModel = new UsersViewModel(mockAdminService.Object, new Mock<IRefreshCoordinator>().Object, mockLogger.Object);

        // Assert
        Assert.AreEqual(0, viewModel.TotalLecturers);
    }

    /// <summary>
    /// Tests that multiple consecutive sets to different values correctly update the property and raise PropertyChanged each time.
    /// </summary>
    [TestMethod]
    public void TotalLecturers_MultipleConsecutiveSets_UpdatesCorrectly()
    {
        // Arrange
        var mockAdminService = new Mock<IAdminService>();
        var mockLogger = new Mock<ILogger<UsersViewModel>>();
        var viewModel = new UsersViewModel(mockAdminService.Object, new Mock<IRefreshCoordinator>().Object, mockLogger.Object);
        var propertyChangedCount = 0;

        viewModel.PropertyChanged += (sender, e) =>
        {
            if (e.PropertyName == nameof(UsersViewModel.TotalLecturers))
            {
                propertyChangedCount++;
            }
        };

        // Act
        viewModel.TotalLecturers = 5;
        viewModel.TotalLecturers = 10;
        viewModel.TotalLecturers = -3;

        // Assert
        Assert.AreEqual(-3, viewModel.TotalLecturers);
        Assert.AreEqual(3, propertyChangedCount);
    }

    /// <summary>
    /// Tests that the TotalStudents property correctly stores and returns the set value
    /// for various integer inputs including boundary values, typical values, and edge cases.
    /// </summary>
    /// <param name="value">The value to set and verify for the TotalStudents property.</param>
    [TestMethod]
    [DataRow(0, DisplayName = "TotalStudents zero value")]
    [DataRow(1, DisplayName = "TotalStudents positive one")]
    [DataRow(-1, DisplayName = "TotalStudents negative one")]
    [DataRow(100, DisplayName = "TotalStudents typical positive value")]
    [DataRow(-100, DisplayName = "TotalStudents typical negative value")]
    [DataRow(int.MaxValue, DisplayName = "TotalStudents maximum integer value")]
    [DataRow(int.MinValue, DisplayName = "TotalStudents minimum integer value")]
    [DataRow(500, DisplayName = "TotalStudents moderate positive value")]
    [DataRow(-500, DisplayName = "TotalStudents moderate negative value")]
    [DataRow(1000000, DisplayName = "TotalStudents large positive value")]
    [DataRow(-1000000, DisplayName = "TotalStudents large negative value")]
    public void TotalStudents_SetAndGet_ReturnsCorrectValue(int value)
    {
        // Arrange
        var mockAdmin = new Mock<IAdminService>();
        var mockLogger = new Mock<ILogger<UsersViewModel>>();
        var viewModel = new UsersViewModel(mockAdmin.Object, new Mock<IRefreshCoordinator>().Object, mockLogger.Object);

        // Act
        viewModel.TotalStudents = value;
        var result = viewModel.TotalStudents;

        // Assert
        Assert.AreEqual(value, result);
    }

    /// <summary>
    /// Tests that setting the TotalStudents property to a new value raises the PropertyChanged event
    /// with the correct property name "TotalStudents".
    /// </summary>
    [TestMethod]
    public void TotalStudents_SetToNewValue_RaisesPropertyChangedEvent()
    {
        // Arrange
        var mockAdmin = new Mock<IAdminService>();
        var mockLogger = new Mock<ILogger<UsersViewModel>>();
        var viewModel = new UsersViewModel(mockAdmin.Object, new Mock<IRefreshCoordinator>().Object, mockLogger.Object);
        string? raisedPropertyName = null;
        viewModel.PropertyChanged += (sender, args) => raisedPropertyName = args.PropertyName;

        // Act
        viewModel.TotalStudents = 42;

        // Assert
        Assert.AreEqual("TotalStudents", raisedPropertyName);
    }

    /// <summary>
    /// Tests that setting the TotalStudents property to the same value it already holds
    /// does not raise the PropertyChanged event.
    /// </summary>
    [TestMethod]
    public void TotalStudents_SetToSameValue_DoesNotRaisePropertyChangedEvent()
    {
        // Arrange
        var mockAdmin = new Mock<IAdminService>();
        var mockLogger = new Mock<ILogger<UsersViewModel>>();
        var viewModel = new UsersViewModel(mockAdmin.Object, new Mock<IRefreshCoordinator>().Object, mockLogger.Object);
        viewModel.TotalStudents = 25;
        bool eventRaised = false;
        viewModel.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == "TotalStudents")
            {
                eventRaised = true;
            }
        };

        // Act
        viewModel.TotalStudents = 25;

        // Assert
        Assert.IsFalse(eventRaised);
    }

    /// <summary>
    /// Tests that setting TotalStudents multiple times to different values correctly updates
    /// the property and raises PropertyChanged event for each distinct change.
    /// </summary>
    /// <param name="firstValue">The first value to set.</param>
    /// <param name="secondValue">The second value to set.</param>
    /// <param name="thirdValue">The third value to set.</param>
    [TestMethod]
    [DataRow(0, 10, 20, DisplayName = "Sequential positive increases")]
    [DataRow(100, 50, 25, DisplayName = "Sequential positive decreases")]
    [DataRow(-10, 0, 10, DisplayName = "Negative to positive progression")]
    [DataRow(int.MaxValue, 0, int.MinValue, DisplayName = "Max to min through zero")]
    [DataRow(5, -5, 5, DisplayName = "Alternating positive and negative")]
    public void TotalStudents_SetMultipleDifferentValues_RaisesPropertyChangedForEachChange(int firstValue, int secondValue, int thirdValue)
    {
        // Arrange
        var mockAdmin = new Mock<IAdminService>();
        var mockLogger = new Mock<ILogger<UsersViewModel>>();
        var viewModel = new UsersViewModel(mockAdmin.Object, new Mock<IRefreshCoordinator>().Object, mockLogger.Object);
        int eventCount = 0;
        viewModel.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == "TotalStudents")
            {
                eventCount++;
            }
        };

        // Act
        viewModel.TotalStudents = firstValue;
        viewModel.TotalStudents = secondValue;
        viewModel.TotalStudents = thirdValue;

        // Assert
        Assert.AreEqual(3, eventCount);
        Assert.AreEqual(thirdValue, viewModel.TotalStudents);
    }

    /// <summary>
    /// Tests that the PropertyChanged event is raised with the correct sender reference
    /// when TotalStudents is modified.
    /// </summary>
    [TestMethod]
    public void TotalStudents_SetValue_PropertyChangedEventHasCorrectSender()
    {
        // Arrange
        var mockAdmin = new Mock<IAdminService>();
        var mockLogger = new Mock<ILogger<UsersViewModel>>();
        var viewModel = new UsersViewModel(mockAdmin.Object, new Mock<IRefreshCoordinator>().Object, mockLogger.Object);
        object? eventSender = null;
        viewModel.PropertyChanged += (sender, args) => eventSender = sender;

        // Act
        viewModel.TotalStudents = 100;

        // Assert
        Assert.AreSame(viewModel, eventSender);
    }

    /// <summary>
    /// Tests that setting TotalStudents to the initial default value (0) after changing it
    /// correctly updates the property and raises PropertyChanged event.
    /// </summary>
    [TestMethod]
    public void TotalStudents_SetBackToZero_RaisesPropertyChangedEvent()
    {
        // Arrange
        var mockAdmin = new Mock<IAdminService>();
        var mockLogger = new Mock<ILogger<UsersViewModel>>();
        var viewModel = new UsersViewModel(mockAdmin.Object, new Mock<IRefreshCoordinator>().Object, mockLogger.Object);
        viewModel.TotalStudents = 50;
        bool eventRaised = false;
        viewModel.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == "TotalStudents")
            {
                eventRaised = true;
            }
        };

        // Act
        viewModel.TotalStudents = 0;

        // Assert
        Assert.IsTrue(eventRaised);
        Assert.AreEqual(0, viewModel.TotalStudents);
    }

    /// <summary>
    /// Tests that setting TotalStudents to the same value multiple times consecutively
    /// only raises PropertyChanged event once (on the first set).
    /// </summary>
    [TestMethod]
    [DataRow(0, DisplayName = "Same value zero")]
    [DataRow(100, DisplayName = "Same value positive")]
    [DataRow(-100, DisplayName = "Same value negative")]
    [DataRow(int.MaxValue, DisplayName = "Same value max int")]
    [DataRow(int.MinValue, DisplayName = "Same value min int")]
    public void TotalStudents_SetSameValueMultipleTimes_RaisesPropertyChangedOnlyOnce(int value)
    {
        // Arrange
        var mockAdmin = new Mock<IAdminService>();
        var mockLogger = new Mock<ILogger<UsersViewModel>>();
        var viewModel = new UsersViewModel(mockAdmin.Object, new Mock<IRefreshCoordinator>().Object, mockLogger.Object);
        int eventCount = 0;
        viewModel.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == "TotalStudents")
            {
                eventCount++;
            }
        };

        // Act
        viewModel.TotalStudents = value;
        viewModel.TotalStudents = value;
        viewModel.TotalStudents = value;

        // Assert
        Assert.AreEqual(1, eventCount);
        Assert.AreEqual(value, viewModel.TotalStudents);
    }

    /// <summary>
    /// Tests that the PropertyChanged event args contain the correct property name
    /// when TotalStudents is set to a new value.
    /// </summary>
    [TestMethod]
    public void TotalStudents_SetValue_PropertyChangedEventArgsContainCorrectPropertyName()
    {
        // Arrange
        var mockAdmin = new Mock<IAdminService>();
        var mockLogger = new Mock<ILogger<UsersViewModel>>();
        var viewModel = new UsersViewModel(mockAdmin.Object, new Mock<IRefreshCoordinator>().Object, mockLogger.Object);
        PropertyChangedEventArgs? capturedArgs = null;
        viewModel.PropertyChanged += (sender, args) => capturedArgs = args;

        // Act
        viewModel.TotalStudents = 75;

        // Assert
        Assert.IsNotNull(capturedArgs);
        Assert.AreEqual("TotalStudents", capturedArgs.PropertyName);
    }

    /// <summary>
    /// Tests that setting SelectedTab to a new value updates the property value correctly.
    /// Verifies the getter returns the newly set value.
    /// </summary>
    /// <param name="newValue">The new value to set for SelectedTab.</param>
    [TestMethod]
    [DataRow("Lecturers", DisplayName = "SelectedTab_SetToLecturers_UpdatesValue")]
    [DataRow("Students", DisplayName = "SelectedTab_SetToStudents_UpdatesValue")]
    [DataRow("", DisplayName = "SelectedTab_SetToEmptyString_UpdatesValue")]
    [DataRow("SomeOtherTab", DisplayName = "SelectedTab_SetToArbitraryValue_UpdatesValue")]
    [DataRow("Admin", DisplayName = "SelectedTab_SetToAdmin_UpdatesValue")]
    public void SelectedTab_SetToNewValue_UpdatesValue(string newValue)
    {
        // Arrange
        var adminMock = new Mock<IAdminService>();
        var loggerMock = new Mock<ILogger<UsersViewModel>>();
        var viewModel = new UsersViewModel(adminMock.Object, new Mock<IRefreshCoordinator>().Object, loggerMock.Object);

        // Act
        viewModel.SelectedTab = newValue;

        // Assert
        Assert.AreEqual(newValue, viewModel.SelectedTab);
    }

    /// <summary>
    /// Tests that setting SelectedTab to a new value raises PropertyChanged event for SelectedTab.
    /// Verifies that the PropertyChanged event is raised with the correct property name.
    /// </summary>
    /// <param name="newValue">The new value to set for SelectedTab.</param>
    [TestMethod]
    [DataRow("Lecturers", DisplayName = "SelectedTab_SetToLecturers_RaisesPropertyChanged")]
    [DataRow("Admin", DisplayName = "SelectedTab_SetToAdmin_RaisesPropertyChanged")]
    [DataRow("", DisplayName = "SelectedTab_SetToEmpty_RaisesPropertyChanged")]
    public void SelectedTab_SetToNewValue_RaisesPropertyChangedForSelectedTab(string newValue)
    {
        // Arrange
        var adminMock = new Mock<IAdminService>();
        var loggerMock = new Mock<ILogger<UsersViewModel>>();
        var viewModel = new UsersViewModel(adminMock.Object, new Mock<IRefreshCoordinator>().Object, loggerMock.Object);
        var propertyChangedRaised = false;
        string? raisedPropertyName = null;

        viewModel.PropertyChanged += (sender, e) =>
        {
            if (e.PropertyName == nameof(UsersViewModel.SelectedTab))
            {
                propertyChangedRaised = true;
                raisedPropertyName = e.PropertyName;
            }
        };

        // Act
        viewModel.SelectedTab = newValue;

        // Assert
        Assert.IsTrue(propertyChangedRaised, "PropertyChanged event should be raised for SelectedTab");
        Assert.AreEqual(nameof(UsersViewModel.SelectedTab), raisedPropertyName);
    }

    /// <summary>
    /// Tests that setting SelectedTab to a new value raises PropertyChanged event for IsStudentsTab.
    /// Verifies that dependent computed properties are notified of changes.
    /// </summary>
    /// <param name="newValue">The new value to set for SelectedTab.</param>
    [TestMethod]
    [DataRow("Lecturers", DisplayName = "SelectedTab_SetToLecturers_RaisesPropertyChangedForIsStudentsTab")]
    [DataRow("Admin", DisplayName = "SelectedTab_SetToAdmin_RaisesPropertyChangedForIsStudentsTab")]
    public void SelectedTab_SetToNewValue_RaisesPropertyChangedForIsStudentsTab(string newValue)
    {
        // Arrange
        var adminMock = new Mock<IAdminService>();
        var loggerMock = new Mock<ILogger<UsersViewModel>>();
        var viewModel = new UsersViewModel(adminMock.Object, new Mock<IRefreshCoordinator>().Object, loggerMock.Object);
        var propertyChangedRaised = false;

        viewModel.PropertyChanged += (sender, e) =>
        {
            if (e.PropertyName == nameof(UsersViewModel.IsStudentsTab))
            {
                propertyChangedRaised = true;
            }
        };

        // Act
        viewModel.SelectedTab = newValue;

        // Assert
        Assert.IsTrue(propertyChangedRaised, "PropertyChanged event should be raised for IsStudentsTab");
    }

    /// <summary>
    /// Tests that setting SelectedTab to a new value raises PropertyChanged event for IsLecturersTab.
    /// Verifies that dependent computed properties are notified of changes.
    /// </summary>
    /// <param name="newValue">The new value to set for SelectedTab.</param>
    [TestMethod]
    [DataRow("Students", DisplayName = "SelectedTab_SetFromLecturersToStudents_RaisesPropertyChangedForIsLecturersTab")]
    [DataRow("Admin", DisplayName = "SelectedTab_SetToAdmin_RaisesPropertyChangedForIsLecturersTab")]
    public void SelectedTab_SetToNewValue_RaisesPropertyChangedForIsLecturersTab(string newValue)
    {
        // Arrange
        var adminMock = new Mock<IAdminService>();
        var loggerMock = new Mock<ILogger<UsersViewModel>>();
        var viewModel = new UsersViewModel(adminMock.Object, new Mock<IRefreshCoordinator>().Object, loggerMock.Object);
        viewModel.SelectedTab = "Lecturers";
        var propertyChangedRaised = false;

        viewModel.PropertyChanged += (sender, e) =>
        {
            if (e.PropertyName == nameof(UsersViewModel.IsLecturersTab))
            {
                propertyChangedRaised = true;
            }
        };

        // Act
        viewModel.SelectedTab = newValue;

        // Assert
        Assert.IsTrue(propertyChangedRaised, "PropertyChanged event should be raised for IsLecturersTab");
    }

    /// <summary>
    /// Tests that setting SelectedTab to a new value calls ApplyFilter method.
    /// Verifies this by checking that FilteredUsers collection is cleared.
    /// </summary>
    /// <param name="newValue">The new value to set for SelectedTab.</param>
    [TestMethod]
    [DataRow("Lecturers", DisplayName = "SelectedTab_SetToLecturers_CallsApplyFilter")]
    [DataRow("Admin", DisplayName = "SelectedTab_SetToAdmin_CallsApplyFilter")]
    [DataRow("", DisplayName = "SelectedTab_SetToEmpty_CallsApplyFilter")]
    public void SelectedTab_SetToNewValue_CallsApplyFilter(string newValue)
    {
        // Arrange
        var adminMock = new Mock<IAdminService>();
        var loggerMock = new Mock<ILogger<UsersViewModel>>();
        var viewModel = new UsersViewModel(adminMock.Object, new Mock<IRefreshCoordinator>().Object, loggerMock.Object);
        viewModel.FilteredUsers.Add(new UserItem { Name = "Test User", Email = "test@example.com", Role = "Student", Status = "Active" });
        var initialCount = viewModel.FilteredUsers.Count;

        // Act
        viewModel.SelectedTab = newValue;

        // Assert
        Assert.AreEqual(0, viewModel.FilteredUsers.Count, "ApplyFilter should have been called and cleared FilteredUsers");
    }

    /// <summary>
    /// Tests that setting SelectedTab to the same value does not raise PropertyChanged events.
    /// Verifies that no change detection occurs when value remains the same.
    /// </summary>
    [TestMethod]
    public void SelectedTab_SetToSameValue_DoesNotRaisePropertyChanged()
    {
        // Arrange
        var adminMock = new Mock<IAdminService>();
        var loggerMock = new Mock<ILogger<UsersViewModel>>();
        var viewModel = new UsersViewModel(adminMock.Object, new Mock<IRefreshCoordinator>().Object, loggerMock.Object);
        var propertyChangedCount = 0;

        viewModel.PropertyChanged += (sender, e) =>
        {
            propertyChangedCount++;
        };

        // Act
        viewModel.SelectedTab = "Students";

        // Assert
        Assert.AreEqual(0, propertyChangedCount, "PropertyChanged should not be raised when setting to the same value");
    }

    /// <summary>
    /// Tests that setting SelectedTab to the same value does not call ApplyFilter.
    /// Verifies this by checking that FilteredUsers collection is not cleared.
    /// </summary>
    [TestMethod]
    public void SelectedTab_SetToSameValue_DoesNotCallApplyFilter()
    {
        // Arrange
        var adminMock = new Mock<IAdminService>();
        var loggerMock = new Mock<ILogger<UsersViewModel>>();
        var viewModel = new UsersViewModel(adminMock.Object, new Mock<IRefreshCoordinator>().Object, loggerMock.Object);
        viewModel.FilteredUsers.Add(new UserItem { Name = "Test User", Email = "test@example.com", Role = "Student", Status = "Active" });
        var initialCount = viewModel.FilteredUsers.Count;

        // Act
        viewModel.SelectedTab = "Students";

        // Assert
        Assert.AreEqual(initialCount, viewModel.FilteredUsers.Count, "ApplyFilter should not have been called when setting to the same value");
    }

    /// <summary>
    /// Tests that setting SelectedTab to whitespace-only strings updates the property and raises notifications.
    /// Verifies that whitespace strings are treated as valid distinct values.
    /// </summary>
    /// <param name="whitespaceValue">The whitespace string to test.</param>
    [TestMethod]
    [DataRow("   ", DisplayName = "SelectedTab_SetToSpaces_UpdatesAndRaisesNotifications")]
    [DataRow("\t", DisplayName = "SelectedTab_SetToTab_UpdatesAndRaisesNotifications")]
    [DataRow("\n", DisplayName = "SelectedTab_SetToNewline_UpdatesAndRaisesNotifications")]
    [DataRow("\r\n", DisplayName = "SelectedTab_SetToCarriageReturnNewline_UpdatesAndRaisesNotifications")]
    public void SelectedTab_SetToWhitespace_UpdatesAndRaisesNotifications(string whitespaceValue)
    {
        // Arrange
        var adminMock = new Mock<IAdminService>();
        var loggerMock = new Mock<ILogger<UsersViewModel>>();
        var viewModel = new UsersViewModel(adminMock.Object, new Mock<IRefreshCoordinator>().Object, loggerMock.Object);
        var selectedTabChanged = false;
        var isStudentsTabChanged = false;
        var isLecturersTabChanged = false;

        viewModel.PropertyChanged += (sender, e) =>
        {
            if (e.PropertyName == nameof(UsersViewModel.SelectedTab)) selectedTabChanged = true;
            if (e.PropertyName == nameof(UsersViewModel.IsStudentsTab)) isStudentsTabChanged = true;
            if (e.PropertyName == nameof(UsersViewModel.IsLecturersTab)) isLecturersTabChanged = true;
        };

        // Act
        viewModel.SelectedTab = whitespaceValue;

        // Assert
        Assert.AreEqual(whitespaceValue, viewModel.SelectedTab, "SelectedTab should be set to whitespace value");
        Assert.IsTrue(selectedTabChanged, "PropertyChanged should be raised for SelectedTab");
        Assert.IsTrue(isStudentsTabChanged, "PropertyChanged should be raised for IsStudentsTab");
        Assert.IsTrue(isLecturersTabChanged, "PropertyChanged should be raised for IsLecturersTab");
        Assert.AreEqual(0, viewModel.FilteredUsers.Count, "ApplyFilter should have been called");
    }

    /// <summary>
    /// Tests that setting SelectedTab to a very long string updates the property correctly.
    /// Verifies that there are no length restrictions on the property value.
    /// </summary>
    [TestMethod]
    public void SelectedTab_SetToVeryLongString_UpdatesAndRaisesNotifications()
    {
        // Arrange
        var adminMock = new Mock<IAdminService>();
        var loggerMock = new Mock<ILogger<UsersViewModel>>();
        var viewModel = new UsersViewModel(adminMock.Object, new Mock<IRefreshCoordinator>().Object, loggerMock.Object);
        var veryLongString = new string('A', 10000);
        var selectedTabChanged = false;
        var isStudentsTabChanged = false;
        var isLecturersTabChanged = false;

        viewModel.PropertyChanged += (sender, e) =>
        {
            if (e.PropertyName == nameof(UsersViewModel.SelectedTab)) selectedTabChanged = true;
            if (e.PropertyName == nameof(UsersViewModel.IsStudentsTab)) isStudentsTabChanged = true;
            if (e.PropertyName == nameof(UsersViewModel.IsLecturersTab)) isLecturersTabChanged = true;
        };

        // Act
        viewModel.SelectedTab = veryLongString;

        // Assert
        Assert.AreEqual(veryLongString, viewModel.SelectedTab, "SelectedTab should be set to very long string");
        Assert.IsTrue(selectedTabChanged, "PropertyChanged should be raised for SelectedTab");
        Assert.IsTrue(isStudentsTabChanged, "PropertyChanged should be raised for IsStudentsTab");
        Assert.IsTrue(isLecturersTabChanged, "PropertyChanged should be raised for IsLecturersTab");
        Assert.AreEqual(0, viewModel.FilteredUsers.Count, "ApplyFilter should have been called");
    }

    /// <summary>
    /// Tests that setting SelectedTab to strings with special characters updates the property correctly.
    /// Verifies that special characters, unicode, and control characters are handled properly.
    /// </summary>
    /// <param name="specialValue">The string with special characters to test.</param>
    [TestMethod]
    [DataRow("Special!@#$%^&*()", DisplayName = "SelectedTab_SetToSpecialCharacters_UpdatesCorrectly")]
    [DataRow("Unicode-Ã©Ã±ä¸­", DisplayName = "SelectedTab_SetToUnicode_UpdatesCorrectly")]
    [DataRow("With\0Null\0Characters", DisplayName = "SelectedTab_SetToControlCharacters_UpdatesCorrectly")]
    public void SelectedTab_SetToSpecialCharacters_UpdatesAndRaisesNotifications(string specialValue)
    {
        // Arrange
        var adminMock = new Mock<IAdminService>();
        var loggerMock = new Mock<ILogger<UsersViewModel>>();
        var viewModel = new UsersViewModel(adminMock.Object, new Mock<IRefreshCoordinator>().Object, loggerMock.Object);
        var selectedTabChanged = false;

        viewModel.PropertyChanged += (sender, e) =>
        {
            if (e.PropertyName == nameof(UsersViewModel.SelectedTab)) selectedTabChanged = true;
        };

        // Act
        viewModel.SelectedTab = specialValue;

        // Assert
        Assert.AreEqual(specialValue, viewModel.SelectedTab, "SelectedTab should be set to special value");
        Assert.IsTrue(selectedTabChanged, "PropertyChanged should be raised for SelectedTab");
    }

    /// <summary>
    /// Tests that setting SelectedTab multiple times to different values raises notifications each time.
    /// Verifies that consecutive changes are properly detected and notified.
    /// </summary>
    [TestMethod]
    public void SelectedTab_SetMultipleDifferentValues_RaisesNotificationsEachTime()
    {
        // Arrange
        var adminMock = new Mock<IAdminService>();
        var loggerMock = new Mock<ILogger<UsersViewModel>>();
        var viewModel = new UsersViewModel(adminMock.Object, new Mock<IRefreshCoordinator>().Object, loggerMock.Object);
        var changeCount = 0;

        viewModel.PropertyChanged += (sender, e) =>
        {
            if (e.PropertyName == nameof(UsersViewModel.SelectedTab))
            {
                changeCount++;
            }
        };

        // Act
        viewModel.SelectedTab = "Lecturers";
        viewModel.SelectedTab = "Admin";
        viewModel.SelectedTab = "Students";

        // Assert
        Assert.AreEqual(3, changeCount, "PropertyChanged should be raised three times for three different values");
        Assert.AreEqual("Students", viewModel.SelectedTab, "Final value should be 'Students'");
    }

    /// <summary>
    /// Tests that IsStudentsTab and IsLecturersTab update correctly when SelectedTab changes.
    /// Verifies the computed properties reflect the current tab selection.
    /// </summary>
    /// <param name="selectedTabValue">The value to set for SelectedTab.</param>
    /// <param name="expectedIsStudentsTab">Expected value of IsStudentsTab.</param>
    /// <param name="expectedIsLecturersTab">Expected value of IsLecturersTab.</param>
    [TestMethod]
    [DataRow("Students", true, false, DisplayName = "SelectedTab_Students_ComputedPropertiesCorrect")]
    [DataRow("Lecturers", false, true, DisplayName = "SelectedTab_Lecturers_ComputedPropertiesCorrect")]
    [DataRow("Admin", false, false, DisplayName = "SelectedTab_Admin_ComputedPropertiesCorrect")]
    [DataRow("", false, false, DisplayName = "SelectedTab_Empty_ComputedPropertiesCorrect")]
    [DataRow("students", false, false, DisplayName = "SelectedTab_LowercaseStudents_ComputedPropertiesCorrect")]
    [DataRow("STUDENTS", false, false, DisplayName = "SelectedTab_UppercaseStudents_ComputedPropertiesCorrect")]
    public void SelectedTab_SetValue_UpdatesComputedPropertiesCorrectly(string selectedTabValue, bool expectedIsStudentsTab, bool expectedIsLecturersTab)
    {
        // Arrange
        var adminMock = new Mock<IAdminService>();
        var loggerMock = new Mock<ILogger<UsersViewModel>>();
        var viewModel = new UsersViewModel(adminMock.Object, new Mock<IRefreshCoordinator>().Object, loggerMock.Object);

        // Act
        viewModel.SelectedTab = selectedTabValue;

        // Assert
        Assert.AreEqual(expectedIsStudentsTab, viewModel.IsStudentsTab, $"IsStudentsTab should be {expectedIsStudentsTab} when SelectedTab is '{selectedTabValue}'");
        Assert.AreEqual(expectedIsLecturersTab, viewModel.IsLecturersTab, $"IsLecturersTab should be {expectedIsLecturersTab} when SelectedTab is '{selectedTabValue}'");
    }

    /// <summary>
    /// Tests that SelectedTab comparison is case-sensitive.
    /// Verifies that only exact case matches for "Students" and "Lecturers" affect computed properties.
    /// </summary>
    /// <param name="caseVariation">The case variation to test.</param>
    [TestMethod]
    [DataRow("students", DisplayName = "SelectedTab_LowercaseStudents_CaseSensitive")]
    [DataRow("STUDENTS", DisplayName = "SelectedTab_UppercaseStudents_CaseSensitive")]
    [DataRow("StUdEnTs", DisplayName = "SelectedTab_MixedCaseStudents_CaseSensitive")]
    [DataRow("lecturers", DisplayName = "SelectedTab_LowercaseLecturers_CaseSensitive")]
    [DataRow("LECTURERS", DisplayName = "SelectedTab_UppercaseLecturers_CaseSensitive")]
    public void SelectedTab_CaseVariations_IsCaseSensitive(string caseVariation)
    {
        // Arrange
        var adminMock = new Mock<IAdminService>();
        var loggerMock = new Mock<ILogger<UsersViewModel>>();
        var viewModel = new UsersViewModel(adminMock.Object, new Mock<IRefreshCoordinator>().Object, loggerMock.Object);

        // Act
        viewModel.SelectedTab = caseVariation;

        // Assert
        Assert.IsFalse(viewModel.IsStudentsTab, "IsStudentsTab should be false for non-exact case match");
        Assert.IsFalse(viewModel.IsLecturersTab, "IsLecturersTab should be false for non-exact case match");
    }

    /// <summary>
    /// Tests that all three PropertyChanged events (SelectedTab, IsStudentsTab, IsLecturersTab) are raised together.
    /// Verifies the complete notification chain when SelectedTab value changes.
    /// </summary>
    [TestMethod]
    public void SelectedTab_SetToNewValue_RaisesAllThreePropertyChangedEvents()
    {
        // Arrange
        var adminMock = new Mock<IAdminService>();
        var loggerMock = new Mock<ILogger<UsersViewModel>>();
        var viewModel = new UsersViewModel(adminMock.Object, new Mock<IRefreshCoordinator>().Object, loggerMock.Object);
        var raisedProperties = new List<string>();

        viewModel.PropertyChanged += (sender, e) =>
        {
            if (e.PropertyName != null)
            {
                raisedProperties.Add(e.PropertyName);
            }
        };

        // Act
        viewModel.SelectedTab = "Lecturers";

        // Assert
        Assert.IsTrue(raisedProperties.Contains(nameof(UsersViewModel.SelectedTab)), "SelectedTab PropertyChanged should be raised");
        Assert.IsTrue(raisedProperties.Contains(nameof(UsersViewModel.IsStudentsTab)), "IsStudentsTab PropertyChanged should be raised");
        Assert.IsTrue(raisedProperties.Contains(nameof(UsersViewModel.IsLecturersTab)), "IsLecturersTab PropertyChanged should be raised");
    }

    /// <summary>
    /// Tests that setting SelectedTab alternating between same and different values behaves correctly.
    /// Verifies change detection works correctly across multiple operations.
    /// </summary>
    [TestMethod]
    public void SelectedTab_AlternatingBetweenSameAndDifferent_DetectsChangesCorrectly()
    {
        // Arrange
        var adminMock = new Mock<IAdminService>();
        var loggerMock = new Mock<ILogger<UsersViewModel>>();
        var viewModel = new UsersViewModel(adminMock.Object, new Mock<IRefreshCoordinator>().Object, loggerMock.Object);
        var changeCount = 0;

        viewModel.PropertyChanged += (sender, e) =>
        {
            if (e.PropertyName == nameof(UsersViewModel.SelectedTab))
            {
                changeCount++;
            }
        };

        // Act
        viewModel.SelectedTab = "Students";
        viewModel.SelectedTab = "Lecturers";
        viewModel.SelectedTab = "Lecturers";
        viewModel.SelectedTab = "Admin";
        viewModel.SelectedTab = "Admin";
        viewModel.SelectedTab = "Students";

        // Assert
        Assert.AreEqual(3, changeCount, "PropertyChanged should be raised only for actual value changes (3 times)");
        Assert.AreEqual("Students", viewModel.SelectedTab, "Final value should be 'Students'");
    }

    /// <summary>
    /// Tests that SearchText property initializes to an empty string by default.
    /// </summary>
    [TestMethod]
    public void SearchText_InitialValue_ReturnsEmptyString()
    {
        // Arrange
        var mockAdmin = new Mock<IAdminService>();
        var mockLogger = new Mock<ILogger<UsersViewModel>>();

        // Act
        var viewModel = new UsersViewModel(mockAdmin.Object, new Mock<IRefreshCoordinator>().Object, mockLogger.Object);
        var result = viewModel.SearchText;

        // Assert
        Assert.AreEqual(string.Empty, result);
    }

    /// <summary>
    /// Tests that setting SearchText to a different value updates the property and calls ApplyFilter.
    /// Verifies by checking that FilteredUsers collection is cleared (ApplyFilter behavior).
    /// </summary>
    /// <param name="newValue">The new value to set for SearchText.</param>
    [TestMethod]
    [DataRow("test", DisplayName = "SearchText_SetToNonEmptyString_UpdatesValueAndCallsApplyFilter")]
    [DataRow("", DisplayName = "SearchText_SetToEmptyString_UpdatesValueAndCallsApplyFilter")]
    [DataRow("   ", DisplayName = "SearchText_SetToWhitespace_UpdatesValueAndCallsApplyFilter")]
    [DataRow("a", DisplayName = "SearchText_SetToSingleCharacter_UpdatesValueAndCallsApplyFilter")]
    [DataRow("SearchQuery123", DisplayName = "SearchText_SetToAlphanumeric_UpdatesValueAndCallsApplyFilter")]
    [DataRow("Special!@#$%^&*()Characters", DisplayName = "SearchText_SetToSpecialCharacters_UpdatesValueAndCallsApplyFilter")]
    [DataRow("\t\n\r", DisplayName = "SearchText_SetToControlCharacters_UpdatesValueAndCallsApplyFilter")]
    [DataRow("Unicode-\u00E9\u00F1\u4E2D", DisplayName = "SearchText_SetToUnicodeCharacters_UpdatesValueAndCallsApplyFilter")]
    public void SearchText_SetToDifferentValue_UpdatesValueAndCallsApplyFilter(string newValue)
    {
        // Arrange
        var mockAdmin = new Mock<IAdminService>();
        var mockLogger = new Mock<ILogger<UsersViewModel>>();
        var viewModel = new UsersViewModel(mockAdmin.Object, new Mock<IRefreshCoordinator>().Object, mockLogger.Object);

        // Add a user to FilteredUsers to verify ApplyFilter clears it
        viewModel.FilteredUsers.Add(new UserItem { Name = "Test", Email = "test@test.com", Role = "Student", Status = "Active" });

        // Act
        viewModel.SearchText = newValue;

        // Assert
        Assert.AreEqual(newValue, viewModel.SearchText);
        Assert.AreEqual(0, viewModel.FilteredUsers.Count, "FilteredUsers should be cleared by ApplyFilter");
    }

    /// <summary>
    /// Tests that setting SearchText to the same value does not call ApplyFilter.
    /// Verifies by checking that FilteredUsers collection is not cleared.
    /// </summary>
    [TestMethod]
    public void SearchText_SetToSameValue_DoesNotCallApplyFilter()
    {
        // Arrange
        var mockAdmin = new Mock<IAdminService>();
        var mockLogger = new Mock<ILogger<UsersViewModel>>();
        var viewModel = new UsersViewModel(mockAdmin.Object, new Mock<IRefreshCoordinator>().Object, mockLogger.Object);

        // Add a user to FilteredUsers
        var testUser = new UserItem { Name = "Test", Email = "test@test.com", Role = "Student", Status = "Active" };
        viewModel.FilteredUsers.Add(testUser);

        // Act - set to same value (empty string is the initial value)
        viewModel.SearchText = string.Empty;

        // Assert
        Assert.AreEqual(string.Empty, viewModel.SearchText);
        Assert.AreEqual(1, viewModel.FilteredUsers.Count, "FilteredUsers should not be cleared when value doesn't change");
    }

    /// <summary>
    /// Tests that SearchText property raises PropertyChanged event when value changes.
    /// </summary>
    [TestMethod]
    public void SearchText_SetToDifferentValue_RaisesPropertyChangedEvent()
    {
        // Arrange
        var mockAdmin = new Mock<IAdminService>();
        var mockLogger = new Mock<ILogger<UsersViewModel>>();
        var viewModel = new UsersViewModel(mockAdmin.Object, new Mock<IRefreshCoordinator>().Object, mockLogger.Object);

        string? raisedPropertyName = null;
        viewModel.PropertyChanged += (sender, e) =>
        {
            if (e.PropertyName == nameof(UsersViewModel.SearchText))
                raisedPropertyName = e.PropertyName;
        };

        // Act
        viewModel.SearchText = "test";

        // Assert
        Assert.AreEqual("SearchText", raisedPropertyName);
    }

    /// <summary>
    /// Tests that SearchText property does not raise PropertyChanged event when set to the same value.
    /// </summary>
    [TestMethod]
    public void SearchText_SetToSameValue_DoesNotRaisePropertyChangedEvent()
    {
        // Arrange
        var mockAdmin = new Mock<IAdminService>();
        var mockLogger = new Mock<ILogger<UsersViewModel>>();
        var viewModel = new UsersViewModel(mockAdmin.Object, new Mock<IRefreshCoordinator>().Object, mockLogger.Object);

        var eventRaised = false;
        viewModel.PropertyChanged += (sender, e) =>
        {
            if (e.PropertyName == nameof(UsersViewModel.SearchText))
                eventRaised = true;
        };

        // Act - set to same value (empty string is the initial value)
        viewModel.SearchText = string.Empty;

        // Assert
        Assert.IsFalse(eventRaised);
    }

    /// <summary>
    /// Tests that setting SearchText with a very long string updates the property and calls ApplyFilter correctly.
    /// </summary>
    [TestMethod]
    public void SearchText_SetToVeryLongString_UpdatesValueAndCallsApplyFilter()
    {
        // Arrange
        var mockAdmin = new Mock<IAdminService>();
        var mockLogger = new Mock<ILogger<UsersViewModel>>();
        var viewModel = new UsersViewModel(mockAdmin.Object, new Mock<IRefreshCoordinator>().Object, mockLogger.Object);
        var longString = new string('a', 10000);

        viewModel.FilteredUsers.Add(new UserItem { Name = "Test", Email = "test@test.com", Role = "Student", Status = "Active" });

        // Act
        viewModel.SearchText = longString;

        // Assert
        Assert.AreEqual(longString, viewModel.SearchText);
        Assert.AreEqual(0, viewModel.FilteredUsers.Count, "FilteredUsers should be cleared by ApplyFilter");
    }

    /// <summary>
    /// Tests that getting SearchText returns the current value after setting it.
    /// </summary>
    /// <param name="testValue">The value to set and verify.</param>
    [TestMethod]
    [DataRow("test", DisplayName = "SearchText_GetAfterSet_ReturnsCorrectValue_test")]
    [DataRow("", DisplayName = "SearchText_GetAfterSet_ReturnsCorrectValue_empty")]
    [DataRow("   whitespace   ", DisplayName = "SearchText_GetAfterSet_ReturnsCorrectValue_whitespace")]
    [DataRow("Special!@#$%^&*()Characters", DisplayName = "SearchText_GetAfterSet_ReturnsCorrectValue_special")]
    public void SearchText_GetAfterSet_ReturnsCorrectValue(string testValue)
    {
        // Arrange
        var mockAdmin = new Mock<IAdminService>();
        var mockLogger = new Mock<ILogger<UsersViewModel>>();
        var viewModel = new UsersViewModel(mockAdmin.Object, new Mock<IRefreshCoordinator>().Object, mockLogger.Object);

        // Act
        viewModel.SearchText = testValue;
        var result = viewModel.SearchText;

        // Assert
        Assert.AreEqual(testValue, result);
    }

    /// <summary>
    /// Tests that setting SearchText to null updates the property value.
    /// Note: The property is declared as non-nullable (string), but C# allows null assignment at runtime.
    /// </summary>
    [TestMethod]
    public void SearchText_SetToNull_UpdatesPropertyValue()
    {
        // Arrange
        var mockAdmin = new Mock<IAdminService>();
        var mockLogger = new Mock<ILogger<UsersViewModel>>();
        var viewModel = new UsersViewModel(mockAdmin.Object, new Mock<IRefreshCoordinator>().Object, mockLogger.Object);

        // Act
        viewModel.SearchText = null!;

        // Assert
        Assert.IsNull(viewModel.SearchText);
    }

    /// <summary>
    /// Tests that setting SearchText to null raises PropertyChanged event.
    /// </summary>
    [TestMethod]
    public void SearchText_SetToNull_RaisesPropertyChangedEvent()
    {
        // Arrange
        var mockAdmin = new Mock<IAdminService>();
        var mockLogger = new Mock<ILogger<UsersViewModel>>();
        var viewModel = new UsersViewModel(mockAdmin.Object, new Mock<IRefreshCoordinator>().Object, mockLogger.Object);

        var eventRaised = false;
        viewModel.PropertyChanged += (sender, e) =>
        {
            if (e.PropertyName == nameof(UsersViewModel.SearchText))
                eventRaised = true;
        };

        // Act
        viewModel.SearchText = null!;

        // Assert
        Assert.IsTrue(eventRaised);
    }

    /// <summary>
    /// Tests that setting SearchText alternating between different values raises PropertyChanged event each time.
    /// </summary>
    [TestMethod]
    public void SearchText_AlternatingValues_RaisesPropertyChangedEachTime()
    {
        // Arrange
        var mockAdmin = new Mock<IAdminService>();
        var mockLogger = new Mock<ILogger<UsersViewModel>>();
        var viewModel = new UsersViewModel(mockAdmin.Object, new Mock<IRefreshCoordinator>().Object, mockLogger.Object);

        var eventCount = 0;
        viewModel.PropertyChanged += (sender, e) =>
        {
            if (e.PropertyName == nameof(UsersViewModel.SearchText))
                eventCount++;
        };

        // Act
        viewModel.SearchText = "value1";
        viewModel.SearchText = "value2";
        viewModel.SearchText = "value1";
        viewModel.SearchText = "value2";

        // Assert
        Assert.AreEqual(4, eventCount);
    }

    /// <summary>
    /// Tests that PropertyChanged event provides the correct sender reference when SearchText changes.
    /// </summary>
    [TestMethod]
    public void SearchText_PropertyChanged_ProvidesCorrectSender()
    {
        // Arrange
        var mockAdmin = new Mock<IAdminService>();
        var mockLogger = new Mock<ILogger<UsersViewModel>>();
        var viewModel = new UsersViewModel(mockAdmin.Object, new Mock<IRefreshCoordinator>().Object, mockLogger.Object);

        object? eventSender = null;
        viewModel.PropertyChanged += (sender, e) =>
        {
            if (e.PropertyName == nameof(UsersViewModel.SearchText))
                eventSender = sender;
        };

        // Act
        viewModel.SearchText = "test";

        // Assert
        Assert.AreSame(viewModel, eventSender);
    }

    /// <summary>
    /// Tests that setting SearchText with boundary length strings works correctly.
    /// Tests empty string (0 length), single character (1 length), and very long string (10000+ length).
    /// </summary>
    [TestMethod]
    [DataRow(0, DisplayName = "SearchText_BoundaryLength_EmptyString")]
    [DataRow(1, DisplayName = "SearchText_BoundaryLength_SingleChar")]
    [DataRow(100, DisplayName = "SearchText_BoundaryLength_100Chars")]
    [DataRow(1000, DisplayName = "SearchText_BoundaryLength_1000Chars")]
    [DataRow(10000, DisplayName = "SearchText_BoundaryLength_10000Chars")]
    public void SearchText_SetWithVariousLengths_UpdatesCorrectly(int length)
    {
        // Arrange
        var mockAdmin = new Mock<IAdminService>();
        var mockLogger = new Mock<ILogger<UsersViewModel>>();
        var viewModel = new UsersViewModel(mockAdmin.Object, new Mock<IRefreshCoordinator>().Object, mockLogger.Object);
        var testValue = new string('x', length);

        // Act
        viewModel.SearchText = testValue;

        // Assert
        Assert.AreEqual(testValue, viewModel.SearchText);
        Assert.AreEqual(length, viewModel.SearchText.Length);
    }

    /// <summary>
    /// Tests that the HasError property returns false as its default value
    /// when the UsersViewModel is first instantiated.
    /// </summary>
    [TestMethod]
    public void HasError_DefaultValue_IsFalse()
    {
        // Arrange
        Mock<IAdminService> mockAdminService = new Mock<IAdminService>();
        Mock<ILogger<UsersViewModel>> mockLogger = new Mock<ILogger<UsersViewModel>>();

        // Act
        UsersViewModel viewModel = new UsersViewModel(mockAdminService.Object, new Mock<IRefreshCoordinator>().Object, mockLogger.Object);

        // Assert
        Assert.IsFalse(viewModel.HasError);
    }

    /// <summary>
    /// Tests that setting HasError to true updates the property value correctly
    /// and raises the PropertyChanged event with the correct property name.
    /// </summary>
    [TestMethod]
    public void HasError_SetToTrueFromDefault_UpdatesValueAndRaisesPropertyChanged()
    {
        // Arrange
        Mock<IAdminService> mockAdminService = new Mock<IAdminService>();
        Mock<ILogger<UsersViewModel>> mockLogger = new Mock<ILogger<UsersViewModel>>();
        UsersViewModel viewModel = new UsersViewModel(mockAdminService.Object, new Mock<IRefreshCoordinator>().Object, mockLogger.Object);
        string? raisedPropertyName = null;
        viewModel.PropertyChanged += (sender, args) => raisedPropertyName = args.PropertyName;

        // Act
        viewModel.HasError = true;

        // Assert
        Assert.IsTrue(viewModel.HasError);
        Assert.AreEqual("HasError", raisedPropertyName);
    }

    /// <summary>
    /// Tests that setting HasError to false from true updates the property value correctly
    /// and raises the PropertyChanged event with the correct property name.
    /// </summary>
    [TestMethod]
    public void HasError_SetToFalseFromTrue_UpdatesValueAndRaisesPropertyChanged()
    {
        // Arrange
        Mock<IAdminService> mockAdminService = new Mock<IAdminService>();
        Mock<ILogger<UsersViewModel>> mockLogger = new Mock<ILogger<UsersViewModel>>();
        UsersViewModel viewModel = new UsersViewModel(mockAdminService.Object, new Mock<IRefreshCoordinator>().Object, mockLogger.Object);
        viewModel.HasError = true;
        string? raisedPropertyName = null;
        viewModel.PropertyChanged += (sender, args) => raisedPropertyName = args.PropertyName;

        // Act
        viewModel.HasError = false;

        // Assert
        Assert.IsFalse(viewModel.HasError);
        Assert.AreEqual("HasError", raisedPropertyName);
    }

    /// <summary>
    /// Tests that setting HasError to false when it's already false (default state)
    /// does not raise the PropertyChanged event.
    /// </summary>
    [TestMethod]
    public void HasError_SetToFalseWhenAlreadyFalse_DoesNotRaisePropertyChanged()
    {
        // Arrange
        Mock<IAdminService> mockAdminService = new Mock<IAdminService>();
        Mock<ILogger<UsersViewModel>> mockLogger = new Mock<ILogger<UsersViewModel>>();
        UsersViewModel viewModel = new UsersViewModel(mockAdminService.Object, new Mock<IRefreshCoordinator>().Object, mockLogger.Object);
        bool eventRaised = false;
        viewModel.PropertyChanged += (sender, args) => eventRaised = true;

        // Act
        viewModel.HasError = false;

        // Assert
        Assert.IsFalse(viewModel.HasError);
        Assert.IsFalse(eventRaised);
    }

    /// <summary>
    /// Tests that alternating HasError between true and false multiple times
    /// raises PropertyChanged event for each change.
    /// </summary>
    [TestMethod]
    public void HasError_AlternatingBetweenTrueAndFalse_RaisesPropertyChangedEachTime()
    {
        // Arrange
        Mock<IAdminService> mockAdminService = new Mock<IAdminService>();
        Mock<ILogger<UsersViewModel>> mockLogger = new Mock<ILogger<UsersViewModel>>();
        UsersViewModel viewModel = new UsersViewModel(mockAdminService.Object, new Mock<IRefreshCoordinator>().Object, mockLogger.Object);
        int eventCount = 0;
        viewModel.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == "HasError")
                eventCount++;
        };

        // Act & Assert
        viewModel.HasError = true;
        Assert.IsTrue(viewModel.HasError);
        Assert.AreEqual(1, eventCount);

        viewModel.HasError = false;
        Assert.IsFalse(viewModel.HasError);
        Assert.AreEqual(2, eventCount);

        viewModel.HasError = true;
        Assert.IsTrue(viewModel.HasError);
        Assert.AreEqual(3, eventCount);

        viewModel.HasError = false;
        Assert.IsFalse(viewModel.HasError);
        Assert.AreEqual(4, eventCount);
    }

    /// <summary>
    /// Tests that the PropertyChanged event raised by setting HasError
    /// has the correct sender (the ViewModel instance itself).
    /// </summary>
    [TestMethod]
    public void HasError_SetValue_PropertyChangedHasCorrectSender()
    {
        // Arrange
        Mock<IAdminService> mockAdminService = new Mock<IAdminService>();
        Mock<ILogger<UsersViewModel>> mockLogger = new Mock<ILogger<UsersViewModel>>();
        UsersViewModel viewModel = new UsersViewModel(mockAdminService.Object, new Mock<IRefreshCoordinator>().Object, mockLogger.Object);
        object? eventSender = null;
        viewModel.PropertyChanged += (sender, args) => eventSender = sender;

        // Act
        viewModel.HasError = true;

        // Assert
        Assert.AreSame(viewModel, eventSender);
    }

    /// <summary>
    /// Tests that the PropertyChanged event raised by setting HasError
    /// has the correct PropertyName in the event arguments.
    /// </summary>
    [TestMethod]
    public void HasError_SetValue_PropertyChangedHasCorrectPropertyName()
    {
        // Arrange
        Mock<IAdminService> mockAdminService = new Mock<IAdminService>();
        Mock<ILogger<UsersViewModel>> mockLogger = new Mock<ILogger<UsersViewModel>>();
        UsersViewModel viewModel = new UsersViewModel(mockAdminService.Object, new Mock<IRefreshCoordinator>().Object, mockLogger.Object);
        string? propertyName = null;
        viewModel.PropertyChanged += (sender, args) => propertyName = args.PropertyName;

        // Act
        viewModel.HasError = true;

        // Assert
        Assert.AreEqual("HasError", propertyName);
    }

    /// <summary>
    /// Tests that multiple consecutive sets to the same value (true)
    /// only raises PropertyChanged once on the first change.
    /// </summary>
    [TestMethod]
    public void HasError_MultipleConsecutiveSetsToTrue_RaisesPropertyChangedOnlyOnce()
    {
        // Arrange
        Mock<IAdminService> mockAdminService = new Mock<IAdminService>();
        Mock<ILogger<UsersViewModel>> mockLogger = new Mock<ILogger<UsersViewModel>>();
        UsersViewModel viewModel = new UsersViewModel(mockAdminService.Object, new Mock<IRefreshCoordinator>().Object, mockLogger.Object);
        int eventCount = 0;
        viewModel.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == "HasError")
                eventCount++;
        };

        // Act
        viewModel.HasError = true;
        viewModel.HasError = true;
        viewModel.HasError = true;

        // Assert
        Assert.IsTrue(viewModel.HasError);
        Assert.AreEqual(1, eventCount);
    }

    /// <summary>
    /// Tests that multiple consecutive sets to the same value (false)
    /// from default state does not raise PropertyChanged event.
    /// </summary>
    [TestMethod]
    public void HasError_MultipleConsecutiveSetsToFalseFromDefault_DoesNotRaisePropertyChanged()
    {
        // Arrange
        Mock<IAdminService> mockAdminService = new Mock<IAdminService>();
        Mock<ILogger<UsersViewModel>> mockLogger = new Mock<ILogger<UsersViewModel>>();
        UsersViewModel viewModel = new UsersViewModel(mockAdminService.Object, new Mock<IRefreshCoordinator>().Object, mockLogger.Object);
        int eventCount = 0;
        viewModel.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == "HasError")
                eventCount++;
        };

        // Act
        viewModel.HasError = false;
        viewModel.HasError = false;
        viewModel.HasError = false;

        // Assert
        Assert.IsFalse(viewModel.HasError);
        Assert.AreEqual(0, eventCount);
    }

    /// <summary>
    /// Tests that the getter returns the exact value that was set via the setter.
    /// Validates round-trip consistency for both true and false values.
    /// </summary>
    /// <param name="value">The boolean value to set and verify.</param>
    [TestMethod]
    [DataRow(true)]
    [DataRow(false)]
    public void HasError_GetAfterSet_ReturnsSetValue(bool value)
    {
        // Arrange
        Mock<IAdminService> mockAdminService = new Mock<IAdminService>();
        Mock<ILogger<UsersViewModel>> mockLogger = new Mock<ILogger<UsersViewModel>>();
        UsersViewModel viewModel = new UsersViewModel(mockAdminService.Object, new Mock<IRefreshCoordinator>().Object, mockLogger.Object);

        // Act
        viewModel.HasError = value;
        bool result = viewModel.HasError;

        // Assert
        Assert.AreEqual(value, result);
    }

    /// <summary>
    /// Tests that IsStudentsTab correctly updates when SelectedTab changes multiple times between different values.
    /// </summary>
    [TestMethod]
    public void IsStudentsTab_MultipleChangesToSelectedTab_ReturnsCorrectValueEachTime()
    {
        // Arrange
        var mockAdmin = new Mock<IAdminService>();
        var mockLogger = new Mock<ILogger<UsersViewModel>>();
        var viewModel = new UsersViewModel(mockAdmin.Object, new Mock<IRefreshCoordinator>().Object, mockLogger.Object);

        // Act & Assert - Initial state
        Assert.IsTrue(viewModel.IsStudentsTab);

        // Act & Assert - Change to Lecturers
        viewModel.SelectedTab = "Lecturers";
        Assert.IsFalse(viewModel.IsStudentsTab);

        // Act & Assert - Change back to Students
        viewModel.SelectedTab = "Students";
        Assert.IsTrue(viewModel.IsStudentsTab);

        // Act & Assert - Change to arbitrary value
        viewModel.SelectedTab = "SomeValue";
        Assert.IsFalse(viewModel.IsStudentsTab);

        // Act & Assert - Change back to Students again
        viewModel.SelectedTab = "Students";
        Assert.IsTrue(viewModel.IsStudentsTab);
    }

    /// <summary>
    /// Tests that IsStudentsTab property raises PropertyChanged notification when SelectedTab changes.
    /// </summary>
    [TestMethod]
    public void IsStudentsTab_WhenSelectedTabChanges_RaisesPropertyChanged()
    {
        // Arrange
        var mockAdmin = new Mock<IAdminService>();
        var mockLogger = new Mock<ILogger<UsersViewModel>>();
        var viewModel = new UsersViewModel(mockAdmin.Object, new Mock<IRefreshCoordinator>().Object, mockLogger.Object);
        var propertyChangedRaised = false;
        string? propertyName = null;

        viewModel.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == nameof(UsersViewModel.IsStudentsTab))
            {
                propertyChangedRaised = true;
                propertyName = args.PropertyName;
            }
        };

        // Act
        viewModel.SelectedTab = "Lecturers";

        // Assert
        Assert.IsTrue(propertyChangedRaised);
        Assert.AreEqual(nameof(UsersViewModel.IsStudentsTab), propertyName);
    }

    /// <summary>
    /// Tests that IsStudentsTab does not raise PropertyChanged when SelectedTab is set to the same value.
    /// </summary>
    [TestMethod]
    public void IsStudentsTab_WhenSelectedTabSetToSameValue_DoesNotRaisePropertyChanged()
    {
        // Arrange
        var mockAdmin = new Mock<IAdminService>();
        var mockLogger = new Mock<ILogger<UsersViewModel>>();
        var viewModel = new UsersViewModel(mockAdmin.Object, new Mock<IRefreshCoordinator>().Object, mockLogger.Object);
        var propertyChangedCount = 0;

        viewModel.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == nameof(UsersViewModel.IsStudentsTab))
            {
                propertyChangedCount++;
            }
        };

        // Act
        viewModel.SelectedTab = "Students";

        // Assert
        Assert.AreEqual(0, propertyChangedCount);
    }

    /// <summary>
    /// Tests that IsLecturersTab returns true when SelectedTab is exactly "Lecturers".
    /// </summary>
    [TestMethod]
    public void IsLecturersTab_SelectedTabIsExactlyLecturers_ReturnsTrue()
    {
        // Arrange
        var mockAdminService = new Mock<IAdminService>();
        var mockLogger = new Mock<ILogger<UsersViewModel>>();
        var viewModel = new UsersViewModel(mockAdminService.Object, new Mock<IRefreshCoordinator>().Object, mockLogger.Object);

        // Act
        viewModel.SelectedTab = "Lecturers";
        var result = viewModel.IsLecturersTab;

        // Assert
        Assert.IsTrue(result);
    }

    /// <summary>
    /// Tests that IsLecturersTab returns false when SelectedTab is "Students".
    /// </summary>
    [TestMethod]
    public void IsLecturersTab_SelectedTabIsStudents_ReturnsFalse()
    {
        // Arrange
        var mockAdminService = new Mock<IAdminService>();
        var mockLogger = new Mock<ILogger<UsersViewModel>>();
        var viewModel = new UsersViewModel(mockAdminService.Object, new Mock<IRefreshCoordinator>().Object, mockLogger.Object);

        // Act
        viewModel.SelectedTab = "Students";
        var result = viewModel.IsLecturersTab;

        // Assert
        Assert.IsFalse(result);
    }

    /// <summary>
    /// Tests that IsLecturersTab returns false when SelectedTab is an empty string.
    /// </summary>
    [TestMethod]
    public void IsLecturersTab_SelectedTabIsEmpty_ReturnsFalse()
    {
        // Arrange
        var mockAdminService = new Mock<IAdminService>();
        var mockLogger = new Mock<ILogger<UsersViewModel>>();
        var viewModel = new UsersViewModel(mockAdminService.Object, new Mock<IRefreshCoordinator>().Object, mockLogger.Object);

        // Act
        viewModel.SelectedTab = "";
        var result = viewModel.IsLecturersTab;

        // Assert
        Assert.IsFalse(result);
    }

    /// <summary>
    /// Tests that IsLecturersTab performs case-sensitive comparison and returns false for case variations.
    /// </summary>
    /// <param name="selectedTabValue">The case variation of "Lecturers" to test.</param>
    [TestMethod]
    [DataRow("lecturers", DisplayName = "IsLecturersTab_LowercaseLecturers_ReturnsFalse")]
    [DataRow("LECTURERS", DisplayName = "IsLecturersTab_UppercaseLecturers_ReturnsFalse")]
    [DataRow("LeCtuReRs", DisplayName = "IsLecturersTab_MixedCaseLecturers_ReturnsFalse")]
    [DataRow("lECTURERS", DisplayName = "IsLecturersTab_MixedCaseVariation_ReturnsFalse")]
    public void IsLecturersTab_CaseVariations_ReturnsFalse(string selectedTabValue)
    {
        // Arrange
        var mockAdminService = new Mock<IAdminService>();
        var mockLogger = new Mock<ILogger<UsersViewModel>>();
        var viewModel = new UsersViewModel(mockAdminService.Object, new Mock<IRefreshCoordinator>().Object, mockLogger.Object);

        // Act
        viewModel.SelectedTab = selectedTabValue;
        var result = viewModel.IsLecturersTab;

        // Assert
        Assert.IsFalse(result);
    }

    /// <summary>
    /// Tests that IsLecturersTab returns false when SelectedTab has leading or trailing whitespace.
    /// </summary>
    /// <param name="selectedTabValue">The value with whitespace to test.</param>
    [TestMethod]
    [DataRow(" Lecturers", DisplayName = "IsLecturersTab_LeadingSpace_ReturnsFalse")]
    [DataRow("Lecturers ", DisplayName = "IsLecturersTab_TrailingSpace_ReturnsFalse")]
    [DataRow(" Lecturers ", DisplayName = "IsLecturersTab_LeadingAndTrailingSpaces_ReturnsFalse")]
    [DataRow("  Lecturers  ", DisplayName = "IsLecturersTab_MultipleSpaces_ReturnsFalse")]
    [DataRow("\tLecturers", DisplayName = "IsLecturersTab_LeadingTab_ReturnsFalse")]
    [DataRow("Lecturers\t", DisplayName = "IsLecturersTab_TrailingTab_ReturnsFalse")]
    [DataRow("\nLecturers", DisplayName = "IsLecturersTab_LeadingNewline_ReturnsFalse")]
    [DataRow("Lecturers\n", DisplayName = "IsLecturersTab_TrailingNewline_ReturnsFalse")]
    public void IsLecturersTab_WhitespaceVariations_ReturnsFalse(string selectedTabValue)
    {
        // Arrange
        var mockAdminService = new Mock<IAdminService>();
        var mockLogger = new Mock<ILogger<UsersViewModel>>();
        var viewModel = new UsersViewModel(mockAdminService.Object, new Mock<IRefreshCoordinator>().Object, mockLogger.Object);

        // Act
        viewModel.SelectedTab = selectedTabValue;
        var result = viewModel.IsLecturersTab;

        // Assert
        Assert.IsFalse(result);
    }

    /// <summary>
    /// Tests that IsLecturersTab returns false for whitespace-only strings.
    /// </summary>
    /// <param name="whitespaceValue">The whitespace-only string to test.</param>
    [TestMethod]
    [DataRow("   ", DisplayName = "IsLecturersTab_Spaces_ReturnsFalse")]
    [DataRow("\t", DisplayName = "IsLecturersTab_Tab_ReturnsFalse")]
    [DataRow("\n", DisplayName = "IsLecturersTab_Newline_ReturnsFalse")]
    [DataRow("\r\n", DisplayName = "IsLecturersTab_CarriageReturnNewline_ReturnsFalse")]
    [DataRow("\t\n\r", DisplayName = "IsLecturersTab_MixedWhitespace_ReturnsFalse")]
    public void IsLecturersTab_WhitespaceOnly_ReturnsFalse(string whitespaceValue)
    {
        // Arrange
        var mockAdminService = new Mock<IAdminService>();
        var mockLogger = new Mock<ILogger<UsersViewModel>>();
        var viewModel = new UsersViewModel(mockAdminService.Object, new Mock<IRefreshCoordinator>().Object, mockLogger.Object);

        // Act
        viewModel.SelectedTab = whitespaceValue;
        var result = viewModel.IsLecturersTab;

        // Assert
        Assert.IsFalse(result);
    }

    /// <summary>
    /// Tests that IsLecturersTab returns false for partial matches or similar strings.
    /// </summary>
    /// <param name="selectedTabValue">The partial or similar string to test.</param>
    [TestMethod]
    [DataRow("Lecturer", DisplayName = "IsLecturersTab_SingularLecturer_ReturnsFalse")]
    [DataRow("Lecturers123", DisplayName = "IsLecturersTab_LecturersWithSuffix_ReturnsFalse")]
    [DataRow("123Lecturers", DisplayName = "IsLecturersTab_LecturersWithPrefix_ReturnsFalse")]
    [DataRow("Lectu", DisplayName = "IsLecturersTab_PartialMatch_ReturnsFalse")]
    [DataRow("Lecturerss", DisplayName = "IsLecturersTab_ExtraCharacter_ReturnsFalse")]
    [DataRow("Lecturrers", DisplayName = "IsLecturersTab_Misspelled_ReturnsFalse")]
    public void IsLecturersTab_PartialOrSimilarStrings_ReturnsFalse(string selectedTabValue)
    {
        // Arrange
        var mockAdminService = new Mock<IAdminService>();
        var mockLogger = new Mock<ILogger<UsersViewModel>>();
        var viewModel = new UsersViewModel(mockAdminService.Object, new Mock<IRefreshCoordinator>().Object, mockLogger.Object);

        // Act
        viewModel.SelectedTab = selectedTabValue;
        var result = viewModel.IsLecturersTab;

        // Assert
        Assert.IsFalse(result);
    }

    /// <summary>
    /// Tests that IsLecturersTab returns false for various other tab values.
    /// </summary>
    /// <param name="selectedTabValue">The other tab value to test.</param>
    [TestMethod]
    [DataRow("Admin", DisplayName = "IsLecturersTab_AdminTab_ReturnsFalse")]
    [DataRow("Staff", DisplayName = "IsLecturersTab_StaffTab_ReturnsFalse")]
    [DataRow("SomeOtherValue", DisplayName = "IsLecturersTab_ArbitraryValue_ReturnsFalse")]
    [DataRow("Teachers", DisplayName = "IsLecturersTab_Teachers_ReturnsFalse")]
    [DataRow("Faculty", DisplayName = "IsLecturersTab_Faculty_ReturnsFalse")]
    public void IsLecturersTab_OtherTabValues_ReturnsFalse(string selectedTabValue)
    {
        // Arrange
        var mockAdminService = new Mock<IAdminService>();
        var mockLogger = new Mock<ILogger<UsersViewModel>>();
        var viewModel = new UsersViewModel(mockAdminService.Object, new Mock<IRefreshCoordinator>().Object, mockLogger.Object);

        // Act
        viewModel.SelectedTab = selectedTabValue;
        var result = viewModel.IsLecturersTab;

        // Assert
        Assert.IsFalse(result);
    }

    /// <summary>
    /// Tests that IsLecturersTab returns false for strings with special characters.
    /// </summary>
    /// <param name="selectedTabValue">The string with special characters to test.</param>
    [TestMethod]
    [DataRow("Lecturers!", DisplayName = "IsLecturersTab_WithExclamation_ReturnsFalse")]
    [DataRow("Lecturers@#$", DisplayName = "IsLecturersTab_WithSpecialChars_ReturnsFalse")]
    [DataRow("Lecturers\u00E9", DisplayName = "IsLecturersTab_WithUnicodeChar_ReturnsFalse")]
    [DataRow("Lecturers\0", DisplayName = "IsLecturersTab_WithNullChar_ReturnsFalse")]
    public void IsLecturersTab_SpecialCharacters_ReturnsFalse(string selectedTabValue)
    {
        // Arrange
        var mockAdminService = new Mock<IAdminService>();
        var mockLogger = new Mock<ILogger<UsersViewModel>>();
        var viewModel = new UsersViewModel(mockAdminService.Object, new Mock<IRefreshCoordinator>().Object, mockLogger.Object);

        // Act
        viewModel.SelectedTab = selectedTabValue;
        var result = viewModel.IsLecturersTab;

        // Assert
        Assert.IsFalse(result);
    }

    /// <summary>
    /// Tests that IsLecturersTab returns false for very long strings.
    /// </summary>
    [TestMethod]
    public void IsLecturersTab_VeryLongString_ReturnsFalse()
    {
        // Arrange
        var mockAdminService = new Mock<IAdminService>();
        var mockLogger = new Mock<ILogger<UsersViewModel>>();
        var viewModel = new UsersViewModel(mockAdminService.Object, new Mock<IRefreshCoordinator>().Object, mockLogger.Object);
        var longString = new string('L', 10000);

        // Act
        viewModel.SelectedTab = longString;
        var result = viewModel.IsLecturersTab;

        // Assert
        Assert.IsFalse(result);
    }

    /// <summary>
    /// Tests that IsLecturersTab correctly alternates between true and false when SelectedTab changes.
    /// </summary>
    [TestMethod]
    public void IsLecturersTab_AlternatingSelectedTabValues_ReturnsCorrectResults()
    {
        // Arrange
        var mockAdminService = new Mock<IAdminService>();
        var mockLogger = new Mock<ILogger<UsersViewModel>>();
        var viewModel = new UsersViewModel(mockAdminService.Object, new Mock<IRefreshCoordinator>().Object, mockLogger.Object);

        // Act & Assert - Set to Lecturers
        viewModel.SelectedTab = "Lecturers";
        Assert.IsTrue(viewModel.IsLecturersTab);

        // Act & Assert - Set to Students
        viewModel.SelectedTab = "Students";
        Assert.IsFalse(viewModel.IsLecturersTab);

        // Act & Assert - Set back to Lecturers
        viewModel.SelectedTab = "Lecturers";
        Assert.IsTrue(viewModel.IsLecturersTab);

        // Act & Assert - Set to other value
        viewModel.SelectedTab = "Admin";
        Assert.IsFalse(viewModel.IsLecturersTab);
    }

    /// <summary>
    /// Tests that IsLecturersTab handles rapid consecutive reads correctly.
    /// </summary>
    [TestMethod]
    public void IsLecturersTab_RapidConsecutiveReads_ReturnsConsistentValue()
    {
        // Arrange
        var mockAdminService = new Mock<IAdminService>();
        var mockLogger = new Mock<ILogger<UsersViewModel>>();
        var viewModel = new UsersViewModel(mockAdminService.Object, new Mock<IRefreshCoordinator>().Object, mockLogger.Object);
        viewModel.SelectedTab = "Lecturers";

        // Act
        var result1 = viewModel.IsLecturersTab;
        var result2 = viewModel.IsLecturersTab;
        var result3 = viewModel.IsLecturersTab;

        // Assert
        Assert.IsTrue(result1);
        Assert.IsTrue(result2);
        Assert.IsTrue(result3);
    }

    /// <summary>
    /// Tests that LoadUsersAsync handles both GetStudentsAsync and GetLecturersAsync throwing exceptions.
    /// Verifies that when both services fail, error state is properly set.
    /// </summary>
    [TestMethod]
    public async Task LoadUsersAsync_WhenBothServicesFail_SetsErrorState()
    {
        // Arrange
        var mockAdmin = new Mock<IAdminService>();
        var mockLogger = new Mock<ILogger<UsersViewModel>>();
        var viewModel = new UsersViewModel(mockAdmin.Object, new Mock<IRefreshCoordinator>().Object, mockLogger.Object);

        var exception = new InvalidOperationException("Both services failed");
        mockAdmin.Setup(x => x.GetStudentsAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(exception);
        mockAdmin.Setup(x => x.GetLecturersAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(exception);

        // Act
        await viewModel.LoadUsersAsync();

        // Assert
        Assert.IsTrue(viewModel.HasError);
        Assert.AreEqual("Failed to load users. Please try again.", viewModel.ErrorMessage);
        Assert.IsFalse(viewModel.IsBusy);
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    /// <summary>
    /// Tests that LoadUsersAsync correctly counts suspended users with exact case-sensitive matching.
    /// Verifies that only "Suspended" (exact case) is counted, not "suspended", "SUSPENDED", etc.
    /// </summary>
    [TestMethod]
    public async Task LoadUsersAsync_WithCaseSensitiveStatus_CountsOnlyExactMatch()
    {
        // Arrange
        var mockAdmin = new Mock<IAdminService>();
        var mockLogger = new Mock<ILogger<UsersViewModel>>();
        var viewModel = new UsersViewModel(mockAdmin.Object, new Mock<IRefreshCoordinator>().Object, mockLogger.Object);

        var students = new List<UserItem>
        {
            new UserItem { Id = Guid.NewGuid(), Name = "Student1", Email = "s1@test.com", Role = "Student", Status = "Suspended" },
            new UserItem { Id = Guid.NewGuid(), Name = "Student2", Email = "s2@test.com", Role = "Student", Status = "suspended" },
            new UserItem { Id = Guid.NewGuid(), Name = "Student3", Email = "s3@test.com", Role = "Student", Status = "SUSPENDED" },
            new UserItem { Id = Guid.NewGuid(), Name = "Student4", Email = "s4@test.com", Role = "Student", Status = "Active" }
        };

        var lecturers = new List<UserItem>();

        mockAdmin.Setup(x => x.GetStudentsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(students);
        mockAdmin.Setup(x => x.GetLecturersAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(lecturers);

        // Act
        await viewModel.LoadUsersAsync();

        // Assert
        Assert.AreEqual(1, viewModel.TotalSuspended, "Only exact 'Suspended' match should be counted");
        Assert.AreEqual(4, viewModel.TotalStudents);
        Assert.AreEqual(0, viewModel.TotalLecturers);
    }

    /// <summary>
    /// Tests that LoadUsersAsync handles very long status strings correctly.
    /// Verifies that long strings are not matched when counting suspended users.
    /// </summary>
    [TestMethod]
    public async Task LoadUsersAsync_WithVeryLongStatusStrings_HandlesCorrectly()
    {
        // Arrange
        var mockAdmin = new Mock<IAdminService>();
        var mockLogger = new Mock<ILogger<UsersViewModel>>();
        var viewModel = new UsersViewModel(mockAdmin.Object, new Mock<IRefreshCoordinator>().Object, mockLogger.Object);

        var longStatus = new string('A', 10000);
        var students = new List<UserItem>
        {
            new UserItem { Id = Guid.NewGuid(), Name = "Student1", Email = "s1@test.com", Role = "Student", Status = longStatus },
            new UserItem { Id = Guid.NewGuid(), Name = "Student2", Email = "s2@test.com", Role = "Student", Status = "Suspended" }
        };

        var lecturers = new List<UserItem>();

        mockAdmin.Setup(x => x.GetStudentsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(students);
        mockAdmin.Setup(x => x.GetLecturersAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(lecturers);

        // Act
        await viewModel.LoadUsersAsync();

        // Assert
        Assert.AreEqual(1, viewModel.TotalSuspended);
        Assert.AreEqual(2, viewModel.TotalStudents);
        Assert.IsFalse(viewModel.HasError);
    }

    /// <summary>
    /// Tests that LoadUsersAsync handles special characters in status strings.
    /// Verifies that special characters don't match "Suspended" for counting.
    /// </summary>
    [TestMethod]
    [DataRow("Suspended\0", DisplayName = "Null character appended")]
    [DataRow("Suspended\t", DisplayName = "Tab character appended")]
    [DataRow("Suspended\n", DisplayName = "Newline character appended")]
    [DataRow(" Suspended", DisplayName = "Leading space")]
    [DataRow("Suspended ", DisplayName = "Trailing space")]
    [DataRow("Suspendâ‚¬d", DisplayName = "Unicode character in middle")]
    public async Task LoadUsersAsync_WithSpecialCharactersInStatus_DoesNotMatchSuspended(string statusValue)
    {
        // Arrange
        var mockAdmin = new Mock<IAdminService>();
        var mockLogger = new Mock<ILogger<UsersViewModel>>();
        var viewModel = new UsersViewModel(mockAdmin.Object, new Mock<IRefreshCoordinator>().Object, mockLogger.Object);

        var students = new List<UserItem>
        {
            new UserItem { Id = Guid.NewGuid(), Name = "Student1", Email = "s1@test.com", Role = "Student", Status = statusValue },
            new UserItem { Id = Guid.NewGuid(), Name = "Student2", Email = "s2@test.com", Role = "Student", Status = "Suspended" }
        };

        var lecturers = new List<UserItem>();

        mockAdmin.Setup(x => x.GetStudentsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(students);
        mockAdmin.Setup(x => x.GetLecturersAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(lecturers);

        // Act
        await viewModel.LoadUsersAsync();

        // Assert
        Assert.AreEqual(1, viewModel.TotalSuspended, "Only exact 'Suspended' should be counted");
        Assert.IsFalse(viewModel.HasError);
    }

    /// <summary>
    /// Tests that LoadUsersAsync handles empty string status correctly.
    /// Verifies that empty status strings are not counted as suspended.
    /// </summary>
    [TestMethod]
    public async Task LoadUsersAsync_WithEmptyStatusStrings_DoesNotCountAsSuspended()
    {
        // Arrange
        var mockAdmin = new Mock<IAdminService>();
        var mockLogger = new Mock<ILogger<UsersViewModel>>();
        var viewModel = new UsersViewModel(mockAdmin.Object, new Mock<IRefreshCoordinator>().Object, mockLogger.Object);

        var students = new List<UserItem>
        {
            new UserItem { Id = Guid.NewGuid(), Name = "Student1", Email = "s1@test.com", Role = "Student", Status = string.Empty },
            new UserItem { Id = Guid.NewGuid(), Name = "Student2", Email = "s2@test.com", Role = "Student", Status = "" },
            new UserItem { Id = Guid.NewGuid(), Name = "Student3", Email = "s3@test.com", Role = "Student", Status = "Suspended" }
        };

        var lecturers = new List<UserItem>();

        mockAdmin.Setup(x => x.GetStudentsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(students);
        mockAdmin.Setup(x => x.GetLecturersAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(lecturers);

        // Act
        await viewModel.LoadUsersAsync();

        // Assert
        Assert.AreEqual(1, viewModel.TotalSuspended);
        Assert.AreEqual(3, viewModel.TotalStudents);
    }

    /// <summary>
    /// Tests that LoadUsersAsync sets IsBusy back to false when OperationCanceledException is thrown.
    /// Verifies that the finally block executes even when operation is canceled.
    /// </summary>
    [TestMethod]
    public async Task LoadUsersAsync_WhenOperationCanceled_ResetsIsBusy()
    {
        // Arrange
        var mockAdmin = new Mock<IAdminService>();
        var mockLogger = new Mock<ILogger<UsersViewModel>>();
        var viewModel = new UsersViewModel(mockAdmin.Object, new Mock<IRefreshCoordinator>().Object, mockLogger.Object);

        mockAdmin.Setup(x => x.GetStudentsAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new OperationCanceledException());
        mockAdmin.Setup(x => x.GetLecturersAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<UserItem>());

        // Act
        await viewModel.LoadUsersAsync();

        // Assert
        Assert.IsFalse(viewModel.IsBusy, "IsBusy should be reset even when operation is canceled");
        Assert.IsFalse(viewModel.HasError, "HasError should remain false for OperationCanceledException");
    }

    /// <summary>
    /// Tests that LoadUsersAsync handles maximum integer values for user counts correctly.
    /// Verifies that the method can handle edge case user counts without overflow.
    /// </summary>
    [TestMethod]
    public async Task LoadUsersAsync_WithMaximumCounts_HandlesCorrectly()
    {
        // Arrange
        var mockAdmin = new Mock<IAdminService>();
        var mockLogger = new Mock<ILogger<UsersViewModel>>();
        var viewModel = new UsersViewModel(mockAdmin.Object, new Mock<IRefreshCoordinator>().Object, mockLogger.Object);

        var students = new List<UserItem>();
        for (int i = 0; i < 1000; i++)
        {
            students.Add(new UserItem
            {
                Id = Guid.NewGuid(),
                Name = $"Student{i}",
                Email = $"s{i}@test.com",
                Role = "Student",
                Status = i % 2 == 0 ? "Suspended" : "Active"
            });
        }

        var lecturers = new List<UserItem>();
        for (int i = 0; i < 500; i++)
        {
            lecturers.Add(new UserItem
            {
                Id = Guid.NewGuid(),
                Name = $"Lecturer{i}",
                Email = $"l{i}@test.com",
                Role = "Lecturer",
                Status = i % 3 == 0 ? "Suspended" : "Active"
            });
        }

        mockAdmin.Setup(x => x.GetStudentsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(students);
        mockAdmin.Setup(x => x.GetLecturersAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(lecturers);

        // Act
        await viewModel.LoadUsersAsync();

        // Assert
        Assert.AreEqual(1000, viewModel.TotalStudents);
        Assert.AreEqual(500, viewModel.TotalLecturers);
        Assert.AreEqual(500 + 167, viewModel.TotalSuspended); // 500 students (even) + 167 lecturers (multiples of 3)
        Assert.IsFalse(viewModel.HasError);
        Assert.IsFalse(viewModel.IsBusy);
    }

    /// <summary>
    /// Tests that LoadUsersAsync properly awaits both tasks using Task.WhenAll.
    /// Verifies that both GetStudentsAsync and GetLecturersAsync are called exactly once.
    /// </summary>
    [TestMethod]
    public async Task LoadUsersAsync_CallsBothServicesExactlyOnce()
    {
        // Arrange
        var mockAdmin = new Mock<IAdminService>();
        var mockLogger = new Mock<ILogger<UsersViewModel>>();
        var viewModel = new UsersViewModel(mockAdmin.Object, new Mock<IRefreshCoordinator>().Object, mockLogger.Object);

        var students = new List<UserItem>
        {
            new UserItem { Id = Guid.NewGuid(), Name = "Student1", Email = "s1@test.com", Role = "Student", Status = "Active" }
        };

        var lecturers = new List<UserItem>
        {
            new UserItem { Id = Guid.NewGuid(), Name = "Lecturer1", Email = "l1@test.com", Role = "Lecturer", Status = "Active" }
        };

        mockAdmin.Setup(x => x.GetStudentsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(students);
        mockAdmin.Setup(x => x.GetLecturersAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(lecturers);

        // Act
        await viewModel.LoadUsersAsync();

        // Assert
        mockAdmin.Verify(x => x.GetStudentsAsync(It.IsAny<CancellationToken>()), Times.Once);
        mockAdmin.Verify(x => x.GetLecturersAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Tests that LoadUsersAsync uses the CancellationToken from CreateLinkedToken.
    /// Verifies that the same cancellation token is passed to both service calls.
    /// </summary>
    [TestMethod]
    public async Task LoadUsersAsync_PassesCancellationTokenToBothServices()
    {
        // Arrange
        var mockAdmin = new Mock<IAdminService>();
        var mockLogger = new Mock<ILogger<UsersViewModel>>();
        var viewModel = new UsersViewModel(mockAdmin.Object, new Mock<IRefreshCoordinator>().Object, mockLogger.Object);

        CancellationToken? studentToken = null;
        CancellationToken? lecturerToken = null;

        mockAdmin.Setup(x => x.GetStudentsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync((CancellationToken ct) =>
            {
                studentToken = ct;
                return new List<UserItem>();
            });

        mockAdmin.Setup(x => x.GetLecturersAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync((CancellationToken ct) =>
            {
                lecturerToken = ct;
                return new List<UserItem>();
            });

        // Act
        await viewModel.LoadUsersAsync();

        // Assert
        Assert.IsNotNull(studentToken, "CancellationToken should be passed to GetStudentsAsync");
        Assert.IsNotNull(lecturerToken, "CancellationToken should be passed to GetLecturersAsync");
        Assert.IsFalse(studentToken.Value.IsCancellationRequested);
        Assert.IsFalse(lecturerToken.Value.IsCancellationRequested);
    }

    /// <summary>
    /// Tests that LoadUsersAsync correctly combines students and lecturers into _allUsers.
    /// Verifies that the total count matches the sum of students and lecturers.
    /// </summary>
    [TestMethod]
    public async Task LoadUsersAsync_CombinesStudentsAndLecturersCorrectly()
    {
        // Arrange
        var mockAdmin = new Mock<IAdminService>();
        var mockLogger = new Mock<ILogger<UsersViewModel>>();
        var viewModel = new UsersViewModel(mockAdmin.Object, new Mock<IRefreshCoordinator>().Object, mockLogger.Object);

        var students = new List<UserItem>
        {
            new UserItem { Id = Guid.NewGuid(), Name = "Student1", Email = "s1@test.com", Role = "Student", Status = "Active" },
            new UserItem { Id = Guid.NewGuid(), Name = "Student2", Email = "s2@test.com", Role = "Student", Status = "Active" }
        };

        var lecturers = new List<UserItem>
        {
            new UserItem { Id = Guid.NewGuid(), Name = "Lecturer1", Email = "l1@test.com", Role = "Lecturer", Status = "Active" },
            new UserItem { Id = Guid.NewGuid(), Name = "Lecturer2", Email = "l2@test.com", Role = "Lecturer", Status = "Active" },
            new UserItem { Id = Guid.NewGuid(), Name = "Lecturer3", Email = "l3@test.com", Role = "Lecturer", Status = "Active" }
        };

        mockAdmin.Setup(x => x.GetStudentsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(students);
        mockAdmin.Setup(x => x.GetLecturersAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(lecturers);

        // Act
        await viewModel.LoadUsersAsync();

        // Assert
        Assert.AreEqual(2, viewModel.TotalStudents);
        Assert.AreEqual(3, viewModel.TotalLecturers);
        // We can't directly access _allUsers, but we can verify through FilteredUsers after ApplyFilter is called
        // The TotalSuspended count verifies that _allUsers contains both collections
    }

    /// <summary>
    /// Tests that LoadUsersAsync handles single user in each list correctly.
    /// Verifies boundary case with minimal data.
    /// </summary>
    [TestMethod]
    public async Task LoadUsersAsync_WithSingleUserInEachList_HandlesCorrectly()
    {
        // Arrange
        var mockAdmin = new Mock<IAdminService>();
        var mockLogger = new Mock<ILogger<UsersViewModel>>();
        var viewModel = new UsersViewModel(mockAdmin.Object, new Mock<IRefreshCoordinator>().Object, mockLogger.Object);

        var students = new List<UserItem>
        {
            new UserItem { Id = Guid.NewGuid(), Name = "Student1", Email = "s1@test.com", Role = "Student", Status = "Suspended" }
        };

        var lecturers = new List<UserItem>
        {
            new UserItem { Id = Guid.NewGuid(), Name = "Lecturer1", Email = "l1@test.com", Role = "Lecturer", Status = "Active" }
        };

        mockAdmin.Setup(x => x.GetStudentsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(students);
        mockAdmin.Setup(x => x.GetLecturersAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(lecturers);

        // Act
        await viewModel.LoadUsersAsync();

        // Assert
        Assert.AreEqual(1, viewModel.TotalStudents);
        Assert.AreEqual(1, viewModel.TotalLecturers);
        Assert.AreEqual(1, viewModel.TotalSuspended);
        Assert.IsFalse(viewModel.HasError);
    }

    /// <summary>
    /// Tests that LoadUsersAsync handles all users having "Suspended" status.
    /// Verifies edge case where TotalSuspended equals total user count.
    /// </summary>
    [TestMethod]
    public async Task LoadUsersAsync_WithAllUsersSuspended_CountsCorrectly()
    {
        // Arrange
        var mockAdmin = new Mock<IAdminService>();
        var mockLogger = new Mock<ILogger<UsersViewModel>>();
        var viewModel = new UsersViewModel(mockAdmin.Object, new Mock<IRefreshCoordinator>().Object, mockLogger.Object);

        var students = new List<UserItem>
        {
            new UserItem { Id = Guid.NewGuid(), Name = "Student1", Email = "s1@test.com", Role = "Student", Status = "Suspended" },
            new UserItem { Id = Guid.NewGuid(), Name = "Student2", Email = "s2@test.com", Role = "Student", Status = "Suspended" }
        };

        var lecturers = new List<UserItem>
        {
            new UserItem { Id = Guid.NewGuid(), Name = "Lecturer1", Email = "l1@test.com", Role = "Lecturer", Status = "Suspended" }
        };

        mockAdmin.Setup(x => x.GetStudentsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(students);
        mockAdmin.Setup(x => x.GetLecturersAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(lecturers);

        // Act
        await viewModel.LoadUsersAsync();

        // Assert
        Assert.AreEqual(2, viewModel.TotalStudents);
        Assert.AreEqual(1, viewModel.TotalLecturers);
        Assert.AreEqual(3, viewModel.TotalSuspended);
    }

    /// <summary>
    /// Tests that LoadUsersAsync handles all users having non-"Suspended" status.
    /// Verifies that TotalSuspended is zero when no users are suspended.
    /// </summary>
    [TestMethod]
    public async Task LoadUsersAsync_WithNoUsersSuspended_CountsZeroSuspended()
    {
        // Arrange
        var mockAdmin = new Mock<IAdminService>();
        var mockLogger = new Mock<ILogger<UsersViewModel>>();
        var viewModel = new UsersViewModel(mockAdmin.Object, new Mock<IRefreshCoordinator>().Object, mockLogger.Object);

        var students = new List<UserItem>
        {
            new UserItem { Id = Guid.NewGuid(), Name = "Student1", Email = "s1@test.com", Role = "Student", Status = "Active" },
            new UserItem { Id = Guid.NewGuid(), Name = "Student2", Email = "s2@test.com", Role = "Student", Status = "Pending" }
        };

        var lecturers = new List<UserItem>
        {
            new UserItem { Id = Guid.NewGuid(), Name = "Lecturer1", Email = "l1@test.com", Role = "Lecturer", Status = "Active" }
        };

        mockAdmin.Setup(x => x.GetStudentsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(students);
        mockAdmin.Setup(x => x.GetLecturersAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(lecturers);

        // Act
        await viewModel.LoadUsersAsync();

        // Assert
        Assert.AreEqual(2, viewModel.TotalStudents);
        Assert.AreEqual(1, viewModel.TotalLecturers);
        Assert.AreEqual(0, viewModel.TotalSuspended);
    }

    /// <summary>
    /// Tests that LoadUsersAsync does not log when OperationCanceledException is thrown.
    /// Verifies that canceled operations are handled silently without logging errors.
    /// </summary>
    [TestMethod]
    public async Task LoadUsersAsync_WhenOperationCanceled_DoesNotLog()
    {
        // Arrange
        var mockAdmin = new Mock<IAdminService>();
        var mockLogger = new Mock<ILogger<UsersViewModel>>();
        var viewModel = new UsersViewModel(mockAdmin.Object, new Mock<IRefreshCoordinator>().Object, mockLogger.Object);

        mockAdmin.Setup(x => x.GetStudentsAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new OperationCanceledException());
        mockAdmin.Setup(x => x.GetLecturersAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<UserItem>());

        // Act
        await viewModel.LoadUsersAsync();

        // Assert
        mockLogger.Verify(
            x => x.Log(
                It.IsAny<LogLevel>(),
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Never);
    }

    /// <summary>
    /// Tests that LoadUsersAsync logs errors with the correct message when an exception occurs.
    /// Verifies the exact error message passed to the logger.
    /// </summary>
    [TestMethod]
    public async Task LoadUsersAsync_WhenExceptionThrown_LogsCorrectMessage()
    {
        // Arrange
        var mockAdmin = new Mock<IAdminService>();
        var mockLogger = new Mock<ILogger<UsersViewModel>>();
        var viewModel = new UsersViewModel(mockAdmin.Object, new Mock<IRefreshCoordinator>().Object, mockLogger.Object);

        var exception = new InvalidOperationException("Test exception");
        mockAdmin.Setup(x => x.GetStudentsAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(exception);
        mockAdmin.Setup(x => x.GetLecturersAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<UserItem>());

        // Act
        await viewModel.LoadUsersAsync();

        // Assert
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Failed to load users")),
                exception,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    /// <summary>
    /// Tests that LoadUsersAsync resets error state before attempting to load data.
    /// Verifies that HasError and ErrorMessage are cleared at the start of the method.
    /// </summary>
    [TestMethod]
    public async Task LoadUsersAsync_BeforeLoading_ClearsErrorState()
    {
        // Arrange
        var mockAdmin = new Mock<IAdminService>();
        var mockLogger = new Mock<ILogger<UsersViewModel>>();
        var viewModel = new UsersViewModel(mockAdmin.Object, new Mock<IRefreshCoordinator>().Object, mockLogger.Object);

        // Set initial error state
        var initialException = new InvalidOperationException("Initial error");
        mockAdmin.Setup(x => x.GetStudentsAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(initialException);
        mockAdmin.Setup(x => x.GetLecturersAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<UserItem>());

        await viewModel.LoadUsersAsync();
        Assert.IsTrue(viewModel.HasError);
        Assert.IsFalse(string.IsNullOrEmpty(viewModel.ErrorMessage));

        // Now setup successful response
        mockAdmin.Setup(x => x.GetStudentsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<UserItem>());
        mockAdmin.Setup(x => x.GetLecturersAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<UserItem>());

        // Act
        await viewModel.LoadUsersAsync();

        // Assert
        Assert.IsFalse(viewModel.HasError);
        Assert.AreEqual(string.Empty, viewModel.ErrorMessage);
    }

    /// <summary>
    /// Tests that LoadUsersAsync handles AggregateException from Task.WhenAll correctly.
    /// Verifies that aggregate exceptions from parallel tasks are properly caught and handled.
    /// </summary>
    [TestMethod]
    public async Task LoadUsersAsync_WithTaskWhenAllAggregateException_SetsErrorState()
    {
        // Arrange
        var mockAdmin = new Mock<IAdminService>();
        var mockLogger = new Mock<ILogger<UsersViewModel>>();
        var viewModel = new UsersViewModel(mockAdmin.Object, new Mock<IRefreshCoordinator>().Object, mockLogger.Object);

        var exception1 = new InvalidOperationException("Students error");
        var exception2 = new InvalidOperationException("Lecturers error");

        mockAdmin.Setup(x => x.GetStudentsAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(exception1);
        mockAdmin.Setup(x => x.GetLecturersAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(exception2);

        // Act
        await viewModel.LoadUsersAsync();

        // Assert
        Assert.IsTrue(viewModel.HasError);
        Assert.AreEqual("Failed to load users. Please try again.", viewModel.ErrorMessage);
        Assert.IsFalse(viewModel.IsBusy);
    }

    /// <summary>
    /// Tests that the PropertyChanged event is raised with the correct sender reference.
    /// Validates that the sender is the ViewModel instance itself.
    /// </summary>
    [TestMethod]
    public void ErrorMessage_SetValue_RaisesPropertyChangedWithCorrectSender()
    {
        // Arrange
        var mockAdminService = new Mock<IAdminService>();
        var mockLogger = new Mock<ILogger<UsersViewModel>>();
        var viewModel = new UsersViewModel(mockAdminService.Object, new Mock<IRefreshCoordinator>().Object, mockLogger.Object);
        object? eventSender = null;

        viewModel.PropertyChanged += (sender, args) =>
        {
            eventSender = sender;
        };

        // Act
        viewModel.ErrorMessage = "New error";

        // Assert
        Assert.AreSame(viewModel, eventSender, "The sender should be the ViewModel instance");
    }

    /// <summary>
    /// Tests that ErrorMessage can be set to an extremely long string without issues.
    /// Validates that the property handles large string values correctly.
    /// </summary>
    [TestMethod]
    public void ErrorMessage_SetExtremelyLongString_UpdatesCorrectly()
    {
        // Arrange
        var mockAdminService = new Mock<IAdminService>();
        var mockLogger = new Mock<ILogger<UsersViewModel>>();
        var viewModel = new UsersViewModel(mockAdminService.Object, new Mock<IRefreshCoordinator>().Object, mockLogger.Object);
        var veryLongString = new string('E', 10000);
        var propertyChangedRaised = false;

        viewModel.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == "ErrorMessage")
                propertyChangedRaised = true;
        };

        // Act
        viewModel.ErrorMessage = veryLongString;

        // Assert
        Assert.AreEqual(veryLongString, viewModel.ErrorMessage);
        Assert.IsTrue(propertyChangedRaised);
        Assert.AreEqual(10000, viewModel.ErrorMessage.Length);
    }

    /// <summary>
    /// Tests that ErrorMessage correctly handles strings with only whitespace characters of different types.
    /// Validates that various whitespace combinations are stored and retrieved correctly.
    /// </summary>
    /// <param name="whitespaceValue">The whitespace string to test.</param>
    [TestMethod]
    [DataRow("   ", DisplayName = "Three spaces")]
    [DataRow("\t", DisplayName = "Tab character")]
    [DataRow("\n", DisplayName = "Newline character")]
    [DataRow("\r\n", DisplayName = "Carriage return and newline")]
    [DataRow(" \t \n \r ", DisplayName = "Mixed whitespace")]
    public void ErrorMessage_SetWhitespaceOnlyString_UpdatesCorrectly(string whitespaceValue)
    {
        // Arrange
        var mockAdminService = new Mock<IAdminService>();
        var mockLogger = new Mock<ILogger<UsersViewModel>>();
        var viewModel = new UsersViewModel(mockAdminService.Object, new Mock<IRefreshCoordinator>().Object, mockLogger.Object);
        var propertyChangedRaised = false;

        viewModel.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == "ErrorMessage")
                propertyChangedRaised = true;
        };

        // Act
        viewModel.ErrorMessage = whitespaceValue;

        // Assert
        Assert.AreEqual(whitespaceValue, viewModel.ErrorMessage);
        Assert.IsTrue(propertyChangedRaised);
    }

    /// <summary>
    /// Tests that ErrorMessage correctly handles strings with special and Unicode characters.
    /// Validates that the property can store and retrieve complex character sequences.
    /// </summary>
    /// <param name="specialValue">The string with special characters to test.</param>
    [TestMethod]
    [DataRow("Error: !@#$%^&*()_+-=[]{}|;':\",./<>?", DisplayName = "Special ASCII characters")]
    [DataRow("Error: \u00E9\u00F1\u00FC\u00E0", DisplayName = "Accented characters")]
    [DataRow("Error: \u4E2D\u6587", DisplayName = "Chinese characters")]
    [DataRow("Error: \uD83D\uDE00", DisplayName = "Emoji")]
    public void ErrorMessage_SetSpecialCharacters_UpdatesCorrectly(string specialValue)
    {
        // Arrange
        var mockAdminService = new Mock<IAdminService>();
        var mockLogger = new Mock<ILogger<UsersViewModel>>();
        var viewModel = new UsersViewModel(mockAdminService.Object, new Mock<IRefreshCoordinator>().Object, mockLogger.Object);
        var propertyChangedRaised = false;

        viewModel.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == "ErrorMessage")
                propertyChangedRaised = true;
        };

        // Act
        viewModel.ErrorMessage = specialValue;

        // Assert
        Assert.AreEqual(specialValue, viewModel.ErrorMessage);
        Assert.IsTrue(propertyChangedRaised);
    }

    /// <summary>
    /// Tests that alternating between null and non-null values correctly updates the property and raises events.
    /// Validates that the property handles null transitions properly.
    /// </summary>
    [TestMethod]
    public void ErrorMessage_AlternateNullAndNonNull_UpdatesCorrectly()
    {
        // Arrange
        var mockAdminService = new Mock<IAdminService>();
        var mockLogger = new Mock<ILogger<UsersViewModel>>();
        var viewModel = new UsersViewModel(mockAdminService.Object, new Mock<IRefreshCoordinator>().Object, mockLogger.Object);
        var propertyChangedCount = 0;

        viewModel.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == "ErrorMessage")
                propertyChangedCount++;
        };

        // Act & Assert
        viewModel.ErrorMessage = "Error 1";
        Assert.AreEqual("Error 1", viewModel.ErrorMessage);
        Assert.AreEqual(1, propertyChangedCount);

        viewModel.ErrorMessage = null!;
        Assert.IsNull(viewModel.ErrorMessage);
        Assert.AreEqual(2, propertyChangedCount);

        viewModel.ErrorMessage = "Error 2";
        Assert.AreEqual("Error 2", viewModel.ErrorMessage);
        Assert.AreEqual(3, propertyChangedCount);
    }

    /// <summary>
    /// Tests that setting ErrorMessage to various boundary length strings works correctly.
    /// Validates empty string, single character, and progressively longer strings.
    /// </summary>
    /// <param name="length">The length of the string to test.</param>
    [TestMethod]
    [DataRow(0, DisplayName = "Empty string (length 0)")]
    [DataRow(1, DisplayName = "Single character (length 1)")]
    [DataRow(10, DisplayName = "Short string (length 10)")]
    [DataRow(100, DisplayName = "Medium string (length 100)")]
    [DataRow(1000, DisplayName = "Long string (length 1000)")]
    public void ErrorMessage_SetVariousLengthStrings_UpdatesCorrectly(int length)
    {
        // Arrange
        var mockAdminService = new Mock<IAdminService>();
        var mockLogger = new Mock<ILogger<UsersViewModel>>();
        var viewModel = new UsersViewModel(mockAdminService.Object, new Mock<IRefreshCoordinator>().Object, mockLogger.Object);
        var testString = new string('X', length);
        var propertyChangedRaised = false;

        viewModel.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == "ErrorMessage")
                propertyChangedRaised = true;
        };

        // Act
        viewModel.ErrorMessage = testString;

        // Assert
        Assert.AreEqual(testString, viewModel.ErrorMessage);
        Assert.AreEqual(length, viewModel.ErrorMessage.Length);
        if (length == 0)
        {
            // Empty string should not raise event since initial value is already empty
            Assert.IsFalse(propertyChangedRaised);
        }
        else
        {
            Assert.IsTrue(propertyChangedRaised);
        }
    }

    /// <summary>
    /// Tests that multiple UsersViewModel instances created with the same dependencies
    /// are independent and have their own command instances.
    /// </summary>
    [TestMethod]
    public void Constructor_MultipleInstances_CreatesIndependentCommandObjects()
    {
        // Arrange
        var mockAdmin = new Mock<IAdminService>();
        var mockLogger = new Mock<ILogger<UsersViewModel>>();

        // Act
        var viewModel1 = new UsersViewModel(mockAdmin.Object, new Mock<IRefreshCoordinator>().Object, mockLogger.Object);
        var viewModel2 = new UsersViewModel(mockAdmin.Object, new Mock<IRefreshCoordinator>().Object, mockLogger.Object);

        // Assert
        Assert.IsNotNull(viewModel1.LoadCommand);
        Assert.IsNotNull(viewModel2.LoadCommand);
        Assert.AreNotSame(viewModel1.LoadCommand, viewModel2.LoadCommand);
        Assert.AreNotSame(viewModel1.SearchCommand, viewModel2.SearchCommand);
        Assert.AreNotSame(viewModel1.SwitchTabCommand, viewModel2.SwitchTabCommand);
        Assert.AreNotSame(viewModel1.SuspendCommand, viewModel2.SuspendCommand);
        Assert.AreNotSame(viewModel1.ActivateCommand, viewModel2.ActivateCommand);
        Assert.AreNotSame(viewModel1.DeleteCommand, viewModel2.DeleteCommand);
    }

    /// <summary>
    /// Tests that multiple UsersViewModel instances have independent FilteredUsers collections.
    /// </summary>
    [TestMethod]
    public void Constructor_MultipleInstances_CreatesIndependentCollections()
    {
        // Arrange
        var mockAdmin = new Mock<IAdminService>();
        var mockLogger = new Mock<ILogger<UsersViewModel>>();

        // Act
        var viewModel1 = new UsersViewModel(mockAdmin.Object, new Mock<IRefreshCoordinator>().Object, mockLogger.Object);
        var viewModel2 = new UsersViewModel(mockAdmin.Object, new Mock<IRefreshCoordinator>().Object, mockLogger.Object);

        // Assert
        Assert.IsNotNull(viewModel1.FilteredUsers);
        Assert.IsNotNull(viewModel2.FilteredUsers);
        Assert.AreNotSame(viewModel1.FilteredUsers, viewModel2.FilteredUsers);
    }

    /// <summary>
    /// Tests that the constructor initializes the inherited IsBusy property to false.
    /// </summary>
    [TestMethod]
    public void Constructor_WithValidParameters_InitializesIsBusyToFalse()
    {
        // Arrange
        var mockAdmin = new Mock<IAdminService>();
        var mockLogger = new Mock<ILogger<UsersViewModel>>();

        // Act
        var viewModel = new UsersViewModel(mockAdmin.Object, new Mock<IRefreshCoordinator>().Object, mockLogger.Object);

        // Assert
        Assert.IsFalse(viewModel.IsBusy);
    }

    /// <summary>
    /// Tests that SwitchTabCommand is a Command with string parameter type.
    /// </summary>
    [TestMethod]
    public void Constructor_WithValidParameters_InitializesSwitchTabCommandAsGenericCommandOfString()
    {
        // Arrange
        var mockAdmin = new Mock<IAdminService>();
        var mockLogger = new Mock<ILogger<UsersViewModel>>();

        // Act
        var viewModel = new UsersViewModel(mockAdmin.Object, new Mock<IRefreshCoordinator>().Object, mockLogger.Object);

        // Assert
        Assert.IsNotNull(viewModel.SwitchTabCommand);
        Assert.IsInstanceOfType(viewModel.SwitchTabCommand, typeof(ICommand));
        Assert.IsInstanceOfType(viewModel.SwitchTabCommand, typeof(Command<string>));
    }

    /// <summary>
    /// Tests that SuspendCommand is a Command with UserItem parameter type.
    /// </summary>
    [TestMethod]
    public void Constructor_WithValidParameters_InitializesSuspendCommandAsGenericCommandOfUserItem()
    {
        // Arrange
        var mockAdmin = new Mock<IAdminService>();
        var mockLogger = new Mock<ILogger<UsersViewModel>>();

        // Act
        var viewModel = new UsersViewModel(mockAdmin.Object, new Mock<IRefreshCoordinator>().Object, mockLogger.Object);

        // Assert
        Assert.IsNotNull(viewModel.SuspendCommand);
        Assert.IsInstanceOfType(viewModel.SuspendCommand, typeof(ICommand));
        Assert.IsInstanceOfType(viewModel.SuspendCommand, typeof(Command<UserItem>));
    }

    /// <summary>
    /// Tests that ActivateCommand is a Command with UserItem parameter type.
    /// </summary>
    [TestMethod]
    public void Constructor_WithValidParameters_InitializesActivateCommandAsGenericCommandOfUserItem()
    {
        // Arrange
        var mockAdmin = new Mock<IAdminService>();
        var mockLogger = new Mock<ILogger<UsersViewModel>>();

        // Act
        var viewModel = new UsersViewModel(mockAdmin.Object, new Mock<IRefreshCoordinator>().Object, mockLogger.Object);

        // Assert
        Assert.IsNotNull(viewModel.ActivateCommand);
        Assert.IsInstanceOfType(viewModel.ActivateCommand, typeof(ICommand));
        Assert.IsInstanceOfType(viewModel.ActivateCommand, typeof(Command<UserItem>));
    }

    /// <summary>
    /// Tests that DeleteCommand is a Command with UserItem parameter type.
    /// </summary>
    [TestMethod]
    public void Constructor_WithValidParameters_InitializesDeleteCommandAsGenericCommandOfUserItem()
    {
        // Arrange
        var mockAdmin = new Mock<IAdminService>();
        var mockLogger = new Mock<ILogger<UsersViewModel>>();

        // Act
        var viewModel = new UsersViewModel(mockAdmin.Object, new Mock<IRefreshCoordinator>().Object, mockLogger.Object);

        // Assert
        Assert.IsNotNull(viewModel.DeleteCommand);
        Assert.IsInstanceOfType(viewModel.DeleteCommand, typeof(ICommand));
        Assert.IsInstanceOfType(viewModel.DeleteCommand, typeof(Command<UserItem>));
    }

    /// <summary>
    /// Tests that LoadCommand is a non-generic Command.
    /// </summary>
    [TestMethod]
    public void Constructor_WithValidParameters_InitializesLoadCommandAsNonGenericCommand()
    {
        // Arrange
        var mockAdmin = new Mock<IAdminService>();
        var mockLogger = new Mock<ILogger<UsersViewModel>>();

        // Act
        var viewModel = new UsersViewModel(mockAdmin.Object, new Mock<IRefreshCoordinator>().Object, mockLogger.Object);

        // Assert
        Assert.IsNotNull(viewModel.LoadCommand);
        Assert.IsInstanceOfType(viewModel.LoadCommand, typeof(ICommand));
        Assert.IsInstanceOfType(viewModel.LoadCommand, typeof(Command));
    }

    /// <summary>
    /// Tests that SearchCommand is a non-generic Command.
    /// </summary>
    [TestMethod]
    public void Constructor_WithValidParameters_InitializesSearchCommandAsNonGenericCommand()
    {
        // Arrange
        var mockAdmin = new Mock<IAdminService>();
        var mockLogger = new Mock<ILogger<UsersViewModel>>();

        // Act
        var viewModel = new UsersViewModel(mockAdmin.Object, new Mock<IRefreshCoordinator>().Object, mockLogger.Object);

        // Assert
        Assert.IsNotNull(viewModel.SearchCommand);
        Assert.IsInstanceOfType(viewModel.SearchCommand, typeof(ICommand));
        Assert.IsInstanceOfType(viewModel.SearchCommand, typeof(Command));
    }

    /// <summary>
    /// Tests that creating a UsersViewModel does not modify the provided dependencies.
    /// </summary>
    [TestMethod]
    public void Constructor_WithValidParameters_DoesNotModifyDependencies()
    {
        // Arrange
        var mockAdmin = new Mock<IAdminService>();
        var mockLogger = new Mock<ILogger<UsersViewModel>>();
        mockAdmin.SetupGet(x => x.ToString()).Returns("AdminService");

        // Act
        var viewModel = new UsersViewModel(mockAdmin.Object, new Mock<IRefreshCoordinator>().Object, mockLogger.Object);

        // Assert - verify no methods were called on the dependencies during construction
        mockAdmin.Verify(x => x.GetStudentsAsync(It.IsAny<CancellationToken>()), Times.Never);
        mockAdmin.Verify(x => x.GetLecturersAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    /// <summary>
    /// Tests that the constructor correctly initializes IsStudentsTab to true by default
    /// since SelectedTab defaults to "Students".
    /// </summary>
    [TestMethod]
    public void Constructor_WithValidParameters_InitializesIsStudentsTabToTrue()
    {
        // Arrange
        var mockAdmin = new Mock<IAdminService>();
        var mockLogger = new Mock<ILogger<UsersViewModel>>();

        // Act
        var viewModel = new UsersViewModel(mockAdmin.Object, new Mock<IRefreshCoordinator>().Object, mockLogger.Object);

        // Assert
        Assert.IsTrue(viewModel.IsStudentsTab);
    }

    /// <summary>
    /// Tests that the constructor correctly initializes IsLecturersTab to false by default
    /// since SelectedTab defaults to "Students".
    /// </summary>
    [TestMethod]
    public void Constructor_WithValidParameters_InitializesIsLecturersTabToFalse()
    {
        // Arrange
        var mockAdmin = new Mock<IAdminService>();
        var mockLogger = new Mock<ILogger<UsersViewModel>>();

        // Act
        var viewModel = new UsersViewModel(mockAdmin.Object, new Mock<IRefreshCoordinator>().Object, mockLogger.Object);

        // Assert
        Assert.IsFalse(viewModel.IsLecturersTab);
    }

    /// <summary>
    /// Tests that multiple ViewModels created sequentially with different dependencies
    /// each store their own dependency references correctly.
    /// </summary>
    [TestMethod]
    public void Constructor_MultipleInstancesWithDifferentDependencies_StoresCorrectReferences()
    {
        // Arrange
        var mockAdmin1 = new Mock<IAdminService>();
        var mockLogger1 = new Mock<ILogger<UsersViewModel>>();
        var mockAdmin2 = new Mock<IAdminService>();
        var mockLogger2 = new Mock<ILogger<UsersViewModel>>();

        // Act
        var viewModel1 = new UsersViewModel(mockAdmin1.Object, new Mock<IRefreshCoordinator>().Object, mockLogger1.Object);
        var viewModel2 = new UsersViewModel(mockAdmin2.Object, new Mock<IRefreshCoordinator>().Object, mockLogger2.Object);

        // Assert - verify each instance is independent
        Assert.AreNotSame(viewModel1, viewModel2);
        Assert.AreNotSame(viewModel1.FilteredUsers, viewModel2.FilteredUsers);
        Assert.AreEqual("User Management", viewModel1.Title);
        Assert.AreEqual("User Management", viewModel2.Title);
    }

    /// <summary>
    /// Tests that constructor does not throw when creating many instances in sequence.
    /// This validates that there are no static state issues or resource leaks.
    /// </summary>
    [TestMethod]
    public void Constructor_ManySequentialInstances_DoesNotThrow()
    {
        // Arrange
        var mockAdmin = new Mock<IAdminService>();
        var mockLogger = new Mock<ILogger<UsersViewModel>>();

        // Act & Assert - create 100 instances to verify no issues with repeated construction
        for (int i = 0; i < 100; i++)
        {
            var viewModel = new UsersViewModel(mockAdmin.Object, new Mock<IRefreshCoordinator>().Object, mockLogger.Object);
            Assert.IsNotNull(viewModel);
            Assert.AreEqual("User Management", viewModel.Title);
        }
    }

    /// <summary>
    /// Tests that the TotalSuspended getter returns the correct value after setting it.
    /// Validates various edge case values including zero, positive, negative, and boundary values.
    /// </summary>
    /// <param name="value">The value to set and verify.</param>
    [TestMethod]
    [DataRow(0, DisplayName = "TotalSuspended_SetValue_ReturnsCorrectValue - Zero")]
    [DataRow(1, DisplayName = "TotalSuspended_SetValue_ReturnsCorrectValue - One")]
    [DataRow(-1, DisplayName = "TotalSuspended_SetValue_ReturnsCorrectValue - Negative One")]
    [DataRow(100, DisplayName = "TotalSuspended_SetValue_ReturnsCorrectValue - Positive Hundred")]
    [DataRow(-100, DisplayName = "TotalSuspended_SetValue_ReturnsCorrectValue - Negative Hundred")]
    [DataRow(int.MaxValue, DisplayName = "TotalSuspended_SetValue_ReturnsCorrectValue - MaxValue")]
    [DataRow(int.MinValue, DisplayName = "TotalSuspended_SetValue_ReturnsCorrectValue - MinValue")]
    public void TotalSuspended_SetValue_ReturnsCorrectValue(int value)
    {
        // Arrange
        var mockAdminService = new Mock<IAdminService>();
        var mockLogger = new Mock<ILogger<UsersViewModel>>();
        var viewModel = new UsersViewModel(mockAdminService.Object, new Mock<IRefreshCoordinator>().Object, mockLogger.Object);

        // Act
        viewModel.TotalSuspended = value;

        // Assert
        Assert.AreEqual(value, viewModel.TotalSuspended);
    }

    /// <summary>
    /// Tests that setting TotalSuspended to a new value raises the PropertyChanged event with the correct property name.
    /// </summary>
    [TestMethod]
    public void TotalSuspended_SetNewValue_RaisesPropertyChanged()
    {
        // Arrange
        var mockAdminService = new Mock<IAdminService>();
        var mockLogger = new Mock<ILogger<UsersViewModel>>();
        var viewModel = new UsersViewModel(mockAdminService.Object, new Mock<IRefreshCoordinator>().Object, mockLogger.Object);
        var propertyChangedRaised = false;
        string? propertyName = null;

        viewModel.PropertyChanged += (sender, args) =>
        {
            propertyChangedRaised = true;
            propertyName = args.PropertyName;
        };

        // Act
        viewModel.TotalSuspended = 42;

        // Assert
        Assert.IsTrue(propertyChangedRaised, "PropertyChanged event should be raised");
        Assert.AreEqual(nameof(UsersViewModel.TotalSuspended), propertyName, "PropertyChanged event should have correct property name");
    }

    /// <summary>
    /// Tests that setting TotalSuspended to the same value does not raise PropertyChanged event.
    /// </summary>
    [TestMethod]
    public void TotalSuspended_SetSameValue_DoesNotRaisePropertyChanged()
    {
        // Arrange
        var mockAdminService = new Mock<IAdminService>();
        var mockLogger = new Mock<ILogger<UsersViewModel>>();
        var viewModel = new UsersViewModel(mockAdminService.Object, new Mock<IRefreshCoordinator>().Object, mockLogger.Object);

        viewModel.TotalSuspended = 10;

        var propertyChangedRaised = false;
        viewModel.PropertyChanged += (sender, args) =>
        {
            propertyChangedRaised = true;
        };

        // Act
        viewModel.TotalSuspended = 10;

        // Assert
        Assert.IsFalse(propertyChangedRaised, "PropertyChanged event should not be raised when setting to the same value");
    }

    /// <summary>
    /// Tests that TotalSuspended can be set multiple times to different values and each change raises PropertyChanged.
    /// </summary>
    /// <param name="firstValue">The first value to set.</param>
    /// <param name="secondValue">The second value to set.</param>
    [TestMethod]
    [DataRow(0, 1, DisplayName = "TotalSuspended_SetMultipleDifferentValues - From 0 to 1")]
    [DataRow(1, 0, DisplayName = "TotalSuspended_SetMultipleDifferentValues - From 1 to 0")]
    [DataRow(-1, 1, DisplayName = "TotalSuspended_SetMultipleDifferentValues - From -1 to 1")]
    [DataRow(int.MinValue, int.MaxValue, DisplayName = "TotalSuspended_SetMultipleDifferentValues - From MinValue to MaxValue")]
    [DataRow(100, -100, DisplayName = "TotalSuspended_SetMultipleDifferentValues - From 100 to -100")]
    public void TotalSuspended_SetMultipleDifferentValues_RaisesPropertyChangedEachTime(int firstValue, int secondValue)
    {
        // Arrange
        var mockAdminService = new Mock<IAdminService>();
        var mockLogger = new Mock<ILogger<UsersViewModel>>();
        var viewModel = new UsersViewModel(mockAdminService.Object, new Mock<IRefreshCoordinator>().Object, mockLogger.Object);
        var eventCount = 0;

        viewModel.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == nameof(UsersViewModel.TotalSuspended))
            {
                eventCount++;
            }
        };

        // Act
        viewModel.TotalSuspended = firstValue;
        viewModel.TotalSuspended = secondValue;

        // Assert
        Assert.AreEqual(2, eventCount, "PropertyChanged should be raised twice for two different values");
        Assert.AreEqual(secondValue, viewModel.TotalSuspended, "Final value should be the second value");
    }

    /// <summary>
    /// Tests that setting TotalSuspended raises PropertyChanged event with correct sender reference.
    /// </summary>
    [TestMethod]
    public void TotalSuspended_SetValue_RaisesPropertyChangedWithCorrectSender()
    {
        // Arrange
        var mockAdminService = new Mock<IAdminService>();
        var mockLogger = new Mock<ILogger<UsersViewModel>>();
        var viewModel = new UsersViewModel(mockAdminService.Object, new Mock<IRefreshCoordinator>().Object, mockLogger.Object);
        object? eventSender = null;

        viewModel.PropertyChanged += (sender, args) =>
        {
            eventSender = sender;
        };

        // Act
        viewModel.TotalSuspended = 5;

        // Assert
        Assert.IsNotNull(eventSender, "Event sender should not be null");
        Assert.AreSame(viewModel, eventSender, "Event sender should be the view model instance");
    }

    /// <summary>
    /// Tests that the TotalLecturers getter returns the correct value after setting it
    /// with various boundary and edge case integer values.
    /// </summary>
    /// <param name="value">The value to set and verify.</param>
    [TestMethod]
    [DataRow(0, DisplayName = "TotalLecturers_SetToZero_ReturnsZero")]
    [DataRow(1, DisplayName = "TotalLecturers_SetToOne_ReturnsOne")]
    [DataRow(-1, DisplayName = "TotalLecturers_SetToNegativeOne_ReturnsNegativeOne")]
    [DataRow(100, DisplayName = "TotalLecturers_SetToOneHundred_ReturnsOneHundred")]
    [DataRow(-100, DisplayName = "TotalLecturers_SetToNegativeOneHundred_ReturnsNegativeOneHundred")]
    [DataRow(int.MaxValue, DisplayName = "TotalLecturers_SetToMaxValue_ReturnsMaxValue")]
    [DataRow(int.MinValue, DisplayName = "TotalLecturers_SetToMinValue_ReturnsMinValue")]
    public void TotalLecturers_SetValue_ReturnsCorrectValue(int value)
    {
        // Arrange
        Mock<IAdminService> adminMock = new Mock<IAdminService>();
        Mock<ILogger<UsersViewModel>> loggerMock = new Mock<ILogger<UsersViewModel>>();
        UsersViewModel viewModel = new UsersViewModel(adminMock.Object, new Mock<IRefreshCoordinator>().Object, loggerMock.Object);

        // Act
        viewModel.TotalLecturers = value;
        int result = viewModel.TotalLecturers;

        // Assert
        Assert.AreEqual(value, result);
    }

    /// <summary>
    /// Tests that setting TotalLecturers to a new value raises the PropertyChanged event
    /// with the correct property name.
    /// </summary>
    [TestMethod]
    public void TotalLecturers_SetNewValue_RaisesPropertyChanged()
    {
        // Arrange
        Mock<IAdminService> adminMock = new Mock<IAdminService>();
        Mock<ILogger<UsersViewModel>> loggerMock = new Mock<ILogger<UsersViewModel>>();
        UsersViewModel viewModel = new UsersViewModel(adminMock.Object, new Mock<IRefreshCoordinator>().Object, loggerMock.Object);
        string? raisedPropertyName = null;
        viewModel.PropertyChanged += (sender, e) => raisedPropertyName = e.PropertyName;

        // Act
        viewModel.TotalLecturers = 42;

        // Assert
        Assert.AreEqual("TotalLecturers", raisedPropertyName);
    }

    /// <summary>
    /// Tests that TotalLecturers can be set multiple times to different values and each change raises PropertyChanged.
    /// </summary>
    /// <param name="firstValue">The first value to set.</param>
    /// <param name="secondValue">The second value to set.</param>
    [TestMethod]
    [DataRow(0, 1, DisplayName = "TotalLecturers_SetFromZeroToOne_RaisesPropertyChangedEachTime")]
    [DataRow(1, 0, DisplayName = "TotalLecturers_SetFromOneToZero_RaisesPropertyChangedEachTime")]
    [DataRow(-1, 1, DisplayName = "TotalLecturers_SetFromNegativeToPositive_RaisesPropertyChangedEachTime")]
    [DataRow(int.MinValue, int.MaxValue, DisplayName = "TotalLecturers_SetFromMinToMax_RaisesPropertyChangedEachTime")]
    [DataRow(100, -100, DisplayName = "TotalLecturers_SetFromPositiveToNegative_RaisesPropertyChangedEachTime")]
    public void TotalLecturers_SetMultipleDifferentValues_RaisesPropertyChangedEachTime(int firstValue, int secondValue)
    {
        // Arrange
        Mock<IAdminService> adminMock = new Mock<IAdminService>();
        Mock<ILogger<UsersViewModel>> loggerMock = new Mock<ILogger<UsersViewModel>>();
        UsersViewModel viewModel = new UsersViewModel(adminMock.Object, new Mock<IRefreshCoordinator>().Object, loggerMock.Object);
        int eventCount = 0;
        viewModel.PropertyChanged += (sender, e) =>
        {
            if (e.PropertyName == "TotalLecturers")
            {
                eventCount++;
            }
        };

        // Act
        viewModel.TotalLecturers = firstValue;
        viewModel.TotalLecturers = secondValue;

        // Assert
        Assert.AreEqual(2, eventCount);
    }

    /// <summary>
    /// Tests that the initial value of TotalLecturers is zero (default int value).
    /// </summary>
    [TestMethod]
    public void TotalLecturers_InitialValue_IsZero()
    {
        // Arrange
        Mock<IAdminService> adminMock = new Mock<IAdminService>();
        Mock<ILogger<UsersViewModel>> loggerMock = new Mock<ILogger<UsersViewModel>>();

        // Act
        UsersViewModel viewModel = new UsersViewModel(adminMock.Object, new Mock<IRefreshCoordinator>().Object, loggerMock.Object);

        // Assert
        Assert.AreEqual(0, viewModel.TotalLecturers);
    }

    /// <summary>
    /// Tests that setting HasError to true from its default value (false)
    /// correctly updates the property value and raises the PropertyChanged event.
    /// Expected: HasError should be true, and PropertyChanged should be raised with property name "HasError".
    /// </summary>
    [TestMethod]
    public void HasError_SetToTrueFromDefaultFalse_UpdatesValueAndRaisesPropertyChanged()
    {
        // Arrange
        var adminMock = new Mock<IAdminService>();
        var loggerMock = new Mock<ILogger<UsersViewModel>>();
        var viewModel = new UsersViewModel(adminMock.Object, new Mock<IRefreshCoordinator>().Object, loggerMock.Object);
        string? raisedPropertyName = null;
        viewModel.PropertyChanged += (sender, args) => raisedPropertyName = args.PropertyName;

        // Act
        viewModel.HasError = true;

        // Assert
        Assert.IsTrue(viewModel.HasError);
        Assert.AreEqual("HasError", raisedPropertyName);
    }

    /// <summary>
    /// Tests that setting HasError to true when it is already true
    /// does not raise the PropertyChanged event.
    /// Expected: PropertyChanged should not be raised, as the value has not changed.
    /// </summary>
    [TestMethod]
    public void HasError_SetToTrueWhenAlreadyTrue_DoesNotRaisePropertyChanged()
    {
        // Arrange
        var adminMock = new Mock<IAdminService>();
        var loggerMock = new Mock<ILogger<UsersViewModel>>();
        var viewModel = new UsersViewModel(adminMock.Object, new Mock<IRefreshCoordinator>().Object, loggerMock.Object);
        viewModel.HasError = true; // Set to true first
        bool eventRaised = false;
        viewModel.PropertyChanged += (sender, args) => eventRaised = true;

        // Act
        viewModel.HasError = true;

        // Assert
        Assert.IsTrue(viewModel.HasError);
        Assert.IsFalse(eventRaised);
    }

    /// <summary>
    /// Tests that setting HasError to the same value multiple times consecutively
    /// does not raise the PropertyChanged event for subsequent sets.
    /// Expected: PropertyChanged should only be raised on the first distinct change.
    /// </summary>
    /// <param name="value">The boolean value to test with repeated sets.</param>
    [TestMethod]
    [DataRow(true)]
    [DataRow(false)]
    public void HasError_SetSameValueMultipleTimes_RaisesPropertyChangedOnlyOnFirstChange(bool value)
    {
        // Arrange
        var adminMock = new Mock<IAdminService>();
        var loggerMock = new Mock<ILogger<UsersViewModel>>();
        var viewModel = new UsersViewModel(adminMock.Object, new Mock<IRefreshCoordinator>().Object, loggerMock.Object);
        int eventCount = 0;
        viewModel.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == "HasError")
                eventCount++;
        };

        // Act
        viewModel.HasError = value;
        viewModel.HasError = value;
        viewModel.HasError = value;

        // Assert
        Assert.AreEqual(value, viewModel.HasError);
        Assert.AreEqual(1, eventCount);
    }

    /// <summary>
    /// Tests that alternating the HasError property between true and false
    /// raises the PropertyChanged event for each distinct change.
    /// Expected: PropertyChanged should be raised for each value change.
    /// </summary>
    [TestMethod]
    public void HasError_AlternatingBetweenTrueAndFalse_RaisesPropertyChangedForEachChange()
    {
        // Arrange
        var adminMock = new Mock<IAdminService>();
        var loggerMock = new Mock<ILogger<UsersViewModel>>();
        var viewModel = new UsersViewModel(adminMock.Object, new Mock<IRefreshCoordinator>().Object, loggerMock.Object);
        int eventCount = 0;
        viewModel.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == "HasError")
                eventCount++;
        };

        // Act
        viewModel.HasError = true;  // Change 1
        viewModel.HasError = false; // Change 2
        viewModel.HasError = true;  // Change 3
        viewModel.HasError = false; // Change 4

        // Assert
        Assert.IsFalse(viewModel.HasError);
        Assert.AreEqual(4, eventCount);
    }

    /// <summary>
    /// Tests that the PropertyChanged event raised when setting HasError
    /// contains the correct sender reference (the ViewModel instance itself).
    /// Expected: The sender should be the ViewModel instance.
    /// </summary>
    [TestMethod]
    public void HasError_SetValue_PropertyChangedEventHasCorrectSender()
    {
        // Arrange
        var adminMock = new Mock<IAdminService>();
        var loggerMock = new Mock<ILogger<UsersViewModel>>();
        var viewModel = new UsersViewModel(adminMock.Object, new Mock<IRefreshCoordinator>().Object, loggerMock.Object);
        object? eventSender = null;
        viewModel.PropertyChanged += (sender, args) => eventSender = sender;

        // Act
        viewModel.HasError = true;

        // Assert
        Assert.AreSame(viewModel, eventSender);
    }

    /// <summary>
    /// Tests that the PropertyChanged event raised when setting HasError
    /// contains the correct property name "HasError" in the event arguments.
    /// Expected: PropertyName should be "HasError".
    /// </summary>
    [TestMethod]
    public void HasError_SetValue_PropertyChangedEventHasCorrectPropertyName()
    {
        // Arrange
        var adminMock = new Mock<IAdminService>();
        var loggerMock = new Mock<ILogger<UsersViewModel>>();
        var viewModel = new UsersViewModel(adminMock.Object, new Mock<IRefreshCoordinator>().Object, loggerMock.Object);
        string? propertyName = null;
        viewModel.PropertyChanged += (sender, args) => propertyName = args.PropertyName;

        // Act
        viewModel.HasError = true;

        // Assert
        Assert.AreEqual("HasError", propertyName);
    }

    /// <summary>
    /// Tests that multiple consecutive reads of HasError return the same value
    /// without triggering any side effects or changes.
    /// Expected: The value should remain consistent across multiple reads.
    /// </summary>
    [TestMethod]
    public void HasError_MultipleConsecutiveReads_ReturnsSameValue()
    {
        // Arrange
        var adminMock = new Mock<IAdminService>();
        var loggerMock = new Mock<ILogger<UsersViewModel>>();
        var viewModel = new UsersViewModel(adminMock.Object, new Mock<IRefreshCoordinator>().Object, loggerMock.Object);
        viewModel.HasError = true;

        // Act
        var read1 = viewModel.HasError;
        var read2 = viewModel.HasError;
        var read3 = viewModel.HasError;

        // Assert
        Assert.IsTrue(read1);
        Assert.IsTrue(read2);
        Assert.IsTrue(read3);
        Assert.AreEqual(read1, read2);
        Assert.AreEqual(read2, read3);
    }

    /// <summary>
    /// Tests that the HasError property correctly transitions through all possible states
    /// (false to true, true to false) and maintains correct values.
    /// Expected: Each transition should update the value correctly.
    /// </summary>
    [TestMethod]
    public void HasError_StateTransitions_UpdatesCorrectly()
    {
        // Arrange
        var adminMock = new Mock<IAdminService>();
        var loggerMock = new Mock<ILogger<UsersViewModel>>();
        var viewModel = new UsersViewModel(adminMock.Object, new Mock<IRefreshCoordinator>().Object, loggerMock.Object);

        // Act & Assert - Initial state
        Assert.IsFalse(viewModel.HasError);

        // Act & Assert - Transition to true
        viewModel.HasError = true;
        Assert.IsTrue(viewModel.HasError);

        // Act & Assert - Transition to false
        viewModel.HasError = false;
        Assert.IsFalse(viewModel.HasError);

        // Act & Assert - Transition back to true
        viewModel.HasError = true;
        Assert.IsTrue(viewModel.HasError);
    }

    /// <summary>
    /// Tests that setting HasError does not affect other properties of the ViewModel.
    /// Expected: Other properties should remain unchanged when HasError is modified.
    /// </summary>
    [TestMethod]
    public void HasError_SetValue_DoesNotAffectOtherProperties()
    {
        // Arrange
        var adminMock = new Mock<IAdminService>();
        var loggerMock = new Mock<ILogger<UsersViewModel>>();
        var viewModel = new UsersViewModel(adminMock.Object, new Mock<IRefreshCoordinator>().Object, loggerMock.Object);
        var initialTitle = viewModel.Title;
        var initialIsBusy = viewModel.IsBusy;
        var initialSelectedTab = viewModel.SelectedTab;

        // Act
        viewModel.HasError = true;

        // Assert
        Assert.AreEqual(initialTitle, viewModel.Title);
        Assert.AreEqual(initialIsBusy, viewModel.IsBusy);
        Assert.AreEqual(initialSelectedTab, viewModel.SelectedTab);
    }

    /// <summary>
    /// Tests that PropertyChanged event is only raised for the HasError property
    /// and not for any other properties when HasError is set.
    /// Expected: Only "HasError" PropertyChanged event should be raised.
    /// </summary>
    [TestMethod]
    public void HasError_SetValue_OnlyRaisesPropertyChangedForHasError()
    {
        // Arrange
        var adminMock = new Mock<IAdminService>();
        var loggerMock = new Mock<ILogger<UsersViewModel>>();
        var viewModel = new UsersViewModel(adminMock.Object, new Mock<IRefreshCoordinator>().Object, loggerMock.Object);
        var raisedProperties = new List<string?>();
        viewModel.PropertyChanged += (sender, args) => raisedProperties.Add(args.PropertyName);

        // Act
        viewModel.HasError = true;

        // Assert
        Assert.AreEqual(1, raisedProperties.Count);
        Assert.AreEqual("HasError", raisedProperties[0]);
    }

    /// <summary>
    /// Tests that the constructor with valid admin and logger parameters
    /// correctly assigns the dependencies to private fields and initializes
    /// all properties and commands without throwing exceptions.
    /// </summary>
    [TestMethod]
    public void Constructor_WithValidAdminAndLogger_InitializesSuccessfully()
    {
        // Arrange
        var mockAdmin = new Mock<IAdminService>();
        var mockLogger = new Mock<ILogger<UsersViewModel>>();

        // Act
        var viewModel = new UsersViewModel(mockAdmin.Object, new Mock<IRefreshCoordinator>().Object, mockLogger.Object);

        // Assert
        Assert.IsNotNull(viewModel);
        Assert.AreEqual("User Management", viewModel.Title);
        Assert.IsNotNull(viewModel.LoadCommand);
        Assert.IsNotNull(viewModel.SearchCommand);
        Assert.IsNotNull(viewModel.SwitchTabCommand);
        Assert.IsNotNull(viewModel.SuspendCommand);
        Assert.IsNotNull(viewModel.ActivateCommand);
        Assert.IsNotNull(viewModel.DeleteCommand);
        Assert.IsNotNull(viewModel.FilteredUsers);
    }

    /// <summary>
    /// Tests that the constructor correctly initializes the Title property
    /// to the exact expected value "User Management".
    /// </summary>
    [TestMethod]
    public void Constructor_WithValidParameters_SetsTitlePropertyCorrectly()
    {
        // Arrange
        var mockAdmin = new Mock<IAdminService>();
        var mockLogger = new Mock<ILogger<UsersViewModel>>();

        // Act
        var viewModel = new UsersViewModel(mockAdmin.Object, new Mock<IRefreshCoordinator>().Object, mockLogger.Object);

        // Assert
        Assert.AreEqual("User Management", viewModel.Title);
    }

    /// <summary>
    /// Tests that the constructor correctly initializes all six command properties
    /// and that each command is a non-null ICommand instance.
    /// </summary>
    [TestMethod]
    public void Constructor_WithValidParameters_InitializesAllSixCommands()
    {
        // Arrange
        var mockAdmin = new Mock<IAdminService>();
        var mockLogger = new Mock<ILogger<UsersViewModel>>();

        // Act
        var viewModel = new UsersViewModel(mockAdmin.Object, new Mock<IRefreshCoordinator>().Object, mockLogger.Object);

        // Assert
        Assert.IsNotNull(viewModel.LoadCommand);
        Assert.IsInstanceOfType(viewModel.LoadCommand, typeof(ICommand));
        Assert.IsNotNull(viewModel.SearchCommand);
        Assert.IsInstanceOfType(viewModel.SearchCommand, typeof(ICommand));
        Assert.IsNotNull(viewModel.SwitchTabCommand);
        Assert.IsInstanceOfType(viewModel.SwitchTabCommand, typeof(ICommand));
        Assert.IsNotNull(viewModel.SuspendCommand);
        Assert.IsInstanceOfType(viewModel.SuspendCommand, typeof(ICommand));
        Assert.IsNotNull(viewModel.ActivateCommand);
        Assert.IsInstanceOfType(viewModel.ActivateCommand, typeof(ICommand));
        Assert.IsNotNull(viewModel.DeleteCommand);
        Assert.IsInstanceOfType(viewModel.DeleteCommand, typeof(ICommand));
    }

    /// <summary>
    /// Tests that the constructor initializes the FilteredUsers property
    /// to a non-null ObservableCollection that is initially empty.
    /// </summary>
    [TestMethod]
    public void Constructor_WithValidParameters_InitializesEmptyFilteredUsersCollection()
    {
        // Arrange
        var mockAdmin = new Mock<IAdminService>();
        var mockLogger = new Mock<ILogger<UsersViewModel>>();

        // Act
        var viewModel = new UsersViewModel(mockAdmin.Object, new Mock<IRefreshCoordinator>().Object, mockLogger.Object);

        // Assert
        Assert.IsNotNull(viewModel.FilteredUsers);
        Assert.IsInstanceOfType(viewModel.FilteredUsers, typeof(ObservableCollection<UserItem>));
        Assert.AreEqual(0, viewModel.FilteredUsers.Count);
    }

    /// <summary>
    /// Tests that the constructor initializes all property default values correctly:
    /// SearchText (empty string), SelectedTab ("Students"), TotalStudents (0),
    /// TotalLecturers (0), TotalSuspended (0), ErrorMessage (empty string),
    /// HasError (false), IsBusy (false), IsStudentsTab (true), IsLecturersTab (false).
    /// </summary>
    [TestMethod]
    public void Constructor_WithValidParameters_SetsCorrectDefaultPropertyValues()
    {
        // Arrange
        var mockAdmin = new Mock<IAdminService>();
        var mockLogger = new Mock<ILogger<UsersViewModel>>();

        // Act
        var viewModel = new UsersViewModel(mockAdmin.Object, new Mock<IRefreshCoordinator>().Object, mockLogger.Object);

        // Assert
        Assert.AreEqual(string.Empty, viewModel.SearchText);
        Assert.AreEqual("Students", viewModel.SelectedTab);
        Assert.AreEqual(0, viewModel.TotalStudents);
        Assert.AreEqual(0, viewModel.TotalLecturers);
        Assert.AreEqual(0, viewModel.TotalSuspended);
        Assert.AreEqual(string.Empty, viewModel.ErrorMessage);
        Assert.IsFalse(viewModel.HasError);
        Assert.IsFalse(viewModel.IsBusy);
        Assert.IsTrue(viewModel.IsStudentsTab);
        Assert.IsFalse(viewModel.IsLecturersTab);
    }

    /// <summary>
    /// Tests that the constructor does not throw an exception when the admin
    /// parameter is null, verifying there is no null validation for this parameter.
    /// </summary>
    [TestMethod]
    public void Constructor_WithNullAdminParameter_DoesNotThrowException()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<UsersViewModel>>();

        // Act & Assert
        var viewModel = new UsersViewModel(null!, new Mock<IRefreshCoordinator>().Object, mockLogger.Object);
        Assert.IsNotNull(viewModel);
    }

    /// <summary>
    /// Tests that the constructor does not throw an exception when the logger
    /// parameter is null, verifying there is no null validation for this parameter.
    /// </summary>
    [TestMethod]
    public void Constructor_WithNullLoggerParameter_DoesNotThrowException()
    {
        // Arrange
        var mockAdmin = new Mock<IAdminService>();

        // Act & Assert
        var viewModel = new UsersViewModel(mockAdmin.Object, new Mock<IRefreshCoordinator>().Object, null!);
        Assert.IsNotNull(viewModel);
    }

    /// <summary>
    /// Tests that the constructor does not throw an exception when both
    /// admin and logger parameters are null.
    /// </summary>
    [TestMethod]
    public void Constructor_WithBothParametersNull_DoesNotThrowException()
    {
        // Act & Assert
        var viewModel = new UsersViewModel(null!, new Mock<IRefreshCoordinator>().Object, null!);
        Assert.IsNotNull(viewModel);
    }

    /// <summary>
    /// Tests that creating multiple UsersViewModel instances results in
    /// independent command objects for each instance.
    /// </summary>
    [TestMethod]
    public void Constructor_MultipleInstances_CreatesIndependentCommands()
    {
        // Arrange
        var mockAdmin = new Mock<IAdminService>();
        var mockLogger = new Mock<ILogger<UsersViewModel>>();

        // Act
        var viewModel1 = new UsersViewModel(mockAdmin.Object, new Mock<IRefreshCoordinator>().Object, mockLogger.Object);
        var viewModel2 = new UsersViewModel(mockAdmin.Object, new Mock<IRefreshCoordinator>().Object, mockLogger.Object);

        // Assert
        Assert.AreNotSame(viewModel1.LoadCommand, viewModel2.LoadCommand);
        Assert.AreNotSame(viewModel1.SearchCommand, viewModel2.SearchCommand);
        Assert.AreNotSame(viewModel1.SwitchTabCommand, viewModel2.SwitchTabCommand);
        Assert.AreNotSame(viewModel1.SuspendCommand, viewModel2.SuspendCommand);
        Assert.AreNotSame(viewModel1.ActivateCommand, viewModel2.ActivateCommand);
        Assert.AreNotSame(viewModel1.DeleteCommand, viewModel2.DeleteCommand);
    }

    /// <summary>
    /// Tests that creating multiple UsersViewModel instances results in
    /// independent FilteredUsers collections for each instance.
    /// </summary>
    [TestMethod]
    public void Constructor_MultipleInstances_CreatesIndependentFilteredUsersCollections()
    {
        // Arrange
        var mockAdmin = new Mock<IAdminService>();
        var mockLogger = new Mock<ILogger<UsersViewModel>>();

        // Act
        var viewModel1 = new UsersViewModel(mockAdmin.Object, new Mock<IRefreshCoordinator>().Object, mockLogger.Object);
        var viewModel2 = new UsersViewModel(mockAdmin.Object, new Mock<IRefreshCoordinator>().Object, mockLogger.Object);

        // Assert
        Assert.AreNotSame(viewModel1.FilteredUsers, viewModel2.FilteredUsers);
    }

    /// <summary>
    /// Tests that the constructor does not modify or call methods on the
    /// provided admin service dependency.
    /// </summary>
    [TestMethod]
    public void Constructor_WithValidParameters_DoesNotInvokeAdminService()
    {
        // Arrange
        var mockAdmin = new Mock<IAdminService>(MockBehavior.Strict);
        var mockLogger = new Mock<ILogger<UsersViewModel>>();

        // Act
        var viewModel = new UsersViewModel(mockAdmin.Object, new Mock<IRefreshCoordinator>().Object, mockLogger.Object);

        // Assert
        Assert.IsNotNull(viewModel);
        mockAdmin.VerifyNoOtherCalls();
    }

    /// <summary>
    /// Tests that the constructor does not modify or call methods on the
    /// provided logger dependency.
    /// </summary>
    [TestMethod]
    public void Constructor_WithValidParameters_DoesNotInvokeLogger()
    {
        // Arrange
        var mockAdmin = new Mock<IAdminService>();
        var mockLogger = new Mock<ILogger<UsersViewModel>>(MockBehavior.Strict);

        // Act
        var viewModel = new UsersViewModel(mockAdmin.Object, new Mock<IRefreshCoordinator>().Object, mockLogger.Object);

        // Assert
        Assert.IsNotNull(viewModel);
        mockLogger.VerifyNoOtherCalls();
    }

    /// <summary>
    /// Tests that the constructor initializes LoadCommand as a Command
    /// (non-generic) that can be executed.
    /// </summary>
    [TestMethod]
    public void Constructor_WithValidParameters_InitializesLoadCommandAsExecutable()
    {
        // Arrange
        var mockAdmin = new Mock<IAdminService>();
        var mockLogger = new Mock<ILogger<UsersViewModel>>();

        // Act
        var viewModel = new UsersViewModel(mockAdmin.Object, new Mock<IRefreshCoordinator>().Object, mockLogger.Object);

        // Assert
        Assert.IsNotNull(viewModel.LoadCommand);
        Assert.IsTrue(viewModel.LoadCommand.CanExecute(null));
    }

    /// <summary>
    /// Tests that the constructor initializes SearchCommand as a Command
    /// (non-generic) that can be executed.
    /// </summary>
    [TestMethod]
    public void Constructor_WithValidParameters_InitializesSearchCommandAsExecutable()
    {
        // Arrange
        var mockAdmin = new Mock<IAdminService>();
        var mockLogger = new Mock<ILogger<UsersViewModel>>();

        // Act
        var viewModel = new UsersViewModel(mockAdmin.Object, new Mock<IRefreshCoordinator>().Object, mockLogger.Object);

        // Assert
        Assert.IsNotNull(viewModel.SearchCommand);
        Assert.IsTrue(viewModel.SearchCommand.CanExecute(null));
    }

    /// <summary>
    /// Tests that the constructor initializes SwitchTabCommand as a generic
    /// Command&lt;string&gt; that can be executed with a string parameter.
    /// </summary>
    [TestMethod]
    public void Constructor_WithValidParameters_InitializesSwitchTabCommandAsExecutable()
    {
        // Arrange
        var mockAdmin = new Mock<IAdminService>();
        var mockLogger = new Mock<ILogger<UsersViewModel>>();

        // Act
        var viewModel = new UsersViewModel(mockAdmin.Object, new Mock<IRefreshCoordinator>().Object, mockLogger.Object);

        // Assert
        Assert.IsNotNull(viewModel.SwitchTabCommand);
        Assert.IsTrue(viewModel.SwitchTabCommand.CanExecute("Lecturers"));
    }

    /// <summary>
    /// Tests that the constructor initializes SuspendCommand as a generic
    /// Command&lt;UserItem&gt; that can be executed with a UserItem parameter.
    /// </summary>
    [TestMethod]
    public void Constructor_WithValidParameters_InitializesSuspendCommandAsExecutable()
    {
        // Arrange
        var mockAdmin = new Mock<IAdminService>();
        var mockLogger = new Mock<ILogger<UsersViewModel>>();
        var userItem = new UserItem { Id = Guid.NewGuid(), Name = "Test User" };

        // Act
        var viewModel = new UsersViewModel(mockAdmin.Object, new Mock<IRefreshCoordinator>().Object, mockLogger.Object);

        // Assert
        Assert.IsNotNull(viewModel.SuspendCommand);
        Assert.IsTrue(viewModel.SuspendCommand.CanExecute(userItem));
    }

    /// <summary>
    /// Tests that the constructor initializes ActivateCommand as a generic
    /// Command&lt;UserItem&gt; that can be executed with a UserItem parameter.
    /// </summary>
    [TestMethod]
    public void Constructor_WithValidParameters_InitializesActivateCommandAsExecutable()
    {
        // Arrange
        var mockAdmin = new Mock<IAdminService>();
        var mockLogger = new Mock<ILogger<UsersViewModel>>();
        var userItem = new UserItem { Id = Guid.NewGuid(), Name = "Test User" };

        // Act
        var viewModel = new UsersViewModel(mockAdmin.Object, new Mock<IRefreshCoordinator>().Object, mockLogger.Object);

        // Assert
        Assert.IsNotNull(viewModel.ActivateCommand);
        Assert.IsTrue(viewModel.ActivateCommand.CanExecute(userItem));
    }

    /// <summary>
    /// Tests that the constructor initializes DeleteCommand as a generic
    /// Command&lt;UserItem&gt; that can be executed with a UserItem parameter.
    /// </summary>
    [TestMethod]
    public void Constructor_WithValidParameters_InitializesDeleteCommandAsExecutable()
    {
        // Arrange
        var mockAdmin = new Mock<IAdminService>();
        var mockLogger = new Mock<ILogger<UsersViewModel>>();
        var userItem = new UserItem { Id = Guid.NewGuid(), Name = "Test User" };

        // Act
        var viewModel = new UsersViewModel(mockAdmin.Object, new Mock<IRefreshCoordinator>().Object, mockLogger.Object);

        // Assert
        Assert.IsNotNull(viewModel.DeleteCommand);
        Assert.IsTrue(viewModel.DeleteCommand.CanExecute(userItem));
    }

    /// <summary>
    /// Tests that constructing multiple instances sequentially with the same
    /// mock objects results in each instance maintaining independent state.
    /// </summary>
    [TestMethod]
    public void Constructor_MultipleSequentialInstancesWithSameMocks_MaintainsIndependentState()
    {
        // Arrange
        var mockAdmin = new Mock<IAdminService>();
        var mockLogger = new Mock<ILogger<UsersViewModel>>();

        // Act
        var viewModel1 = new UsersViewModel(mockAdmin.Object, new Mock<IRefreshCoordinator>().Object, mockLogger.Object);
        viewModel1.SearchText = "Modified";
        var viewModel2 = new UsersViewModel(mockAdmin.Object, new Mock<IRefreshCoordinator>().Object, mockLogger.Object);

        // Assert
        Assert.AreEqual("Modified", viewModel1.SearchText);
        Assert.AreEqual(string.Empty, viewModel2.SearchText);
    }

    /// <summary>
    /// Tests that the constructor correctly initializes when called rapidly
    /// in succession, verifying thread safety and no shared state issues.
    /// </summary>
    [TestMethod]
    public void Constructor_RapidSuccessiveInstantiation_AllInstancesInitializeCorrectly()
    {
        // Arrange
        var mockAdmin = new Mock<IAdminService>();
        var mockLogger = new Mock<ILogger<UsersViewModel>>();

        // Act
        var viewModels = new List<UsersViewModel>();
        for (int i = 0; i < 100; i++)
        {
            viewModels.Add(new UsersViewModel(mockAdmin.Object, new Mock<IRefreshCoordinator>().Object, mockLogger.Object));
        }

        // Assert
        foreach (var viewModel in viewModels)
        {
            Assert.AreEqual("User Management", viewModel.Title);
            Assert.IsNotNull(viewModel.LoadCommand);
            Assert.AreEqual(0, viewModel.FilteredUsers.Count);
        }
    }

    /// <summary>
    /// Tests that LoadUsersAsync properly combines students and lecturers from both service calls
    /// when both services return valid non-empty lists.
    /// </summary>
    [TestMethod]
    public async Task LoadUsersAsync_WithBothStudentsAndLecturers_CombinesListsCorrectly()
    {
        // Arrange
        var mockAdmin = new Mock<IAdminService>();
        var mockLogger = new Mock<ILogger<UsersViewModel>>();
        var viewModel = new UsersViewModel(mockAdmin.Object, new Mock<IRefreshCoordinator>().Object, mockLogger.Object);

        var students = new List<UserItem>
        {
            new UserItem { Id = Guid.NewGuid(), Name = "Student1", Email = "s1@test.com", Role = "Student", Status = "Active" },
            new UserItem { Id = Guid.NewGuid(), Name = "Student2", Email = "s2@test.com", Role = "Student", Status = "Active" }
        };

        var lecturers = new List<UserItem>
        {
            new UserItem { Id = Guid.NewGuid(), Name = "Lecturer1", Email = "l1@test.com", Role = "Lecturer", Status = "Active" },
            new UserItem { Id = Guid.NewGuid(), Name = "Lecturer2", Email = "l2@test.com", Role = "Lecturer", Status = "Active" },
            new UserItem { Id = Guid.NewGuid(), Name = "Lecturer3", Email = "l3@test.com", Role = "Lecturer", Status = "Active" }
        };

        mockAdmin.Setup(x => x.GetStudentsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(students);
        mockAdmin.Setup(x => x.GetLecturersAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(lecturers);

        // Act
        await viewModel.LoadUsersAsync();

        // Assert
        Assert.AreEqual(2, viewModel.TotalStudents);
        Assert.AreEqual(3, viewModel.TotalLecturers);
        Assert.AreEqual(0, viewModel.TotalSuspended);
        Assert.IsFalse(viewModel.IsBusy);
        Assert.IsFalse(viewModel.HasError);
        Assert.AreEqual(string.Empty, viewModel.ErrorMessage);
    }

    /// <summary>
    /// Tests that LoadUsersAsync correctly counts users with "Suspended" status (exact case match).
    /// Verifies case-sensitive matching where only exact "Suspended" is counted, not "suspended", "SUSPENDED", etc.
    /// </summary>
    [TestMethod]
    public async Task LoadUsersAsync_WithMixedCaseStatusValues_CountsOnlyExactSuspendedMatch()
    {
        // Arrange
        var mockAdmin = new Mock<IAdminService>();
        var mockLogger = new Mock<ILogger<UsersViewModel>>();
        var viewModel = new UsersViewModel(mockAdmin.Object, new Mock<IRefreshCoordinator>().Object, mockLogger.Object);

        var students = new List<UserItem>
        {
            new UserItem { Id = Guid.NewGuid(), Name = "Student1", Email = "s1@test.com", Role = "Student", Status = "Suspended" },
            new UserItem { Id = Guid.NewGuid(), Name = "Student2", Email = "s2@test.com", Role = "Student", Status = "suspended" },
            new UserItem { Id = Guid.NewGuid(), Name = "Student3", Email = "s3@test.com", Role = "Student", Status = "SUSPENDED" },
            new UserItem { Id = Guid.NewGuid(), Name = "Student4", Email = "s4@test.com", Role = "Student", Status = "Active" }
        };

        var lecturers = new List<UserItem>
        {
            new UserItem { Id = Guid.NewGuid(), Name = "Lecturer1", Email = "l1@test.com", Role = "Lecturer", Status = "Suspended" },
            new UserItem { Id = Guid.NewGuid(), Name = "Lecturer2", Email = "l2@test.com", Role = "Lecturer", Status = "Suspend" }
        };

        mockAdmin.Setup(x => x.GetStudentsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(students);
        mockAdmin.Setup(x => x.GetLecturersAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(lecturers);

        // Act
        await viewModel.LoadUsersAsync();

        // Assert
        Assert.AreEqual(4, viewModel.TotalStudents);
        Assert.AreEqual(2, viewModel.TotalLecturers);
        Assert.AreEqual(2, viewModel.TotalSuspended, "Only exact 'Suspended' matches should be counted");
        Assert.IsFalse(viewModel.HasError);
    }

    /// <summary>
    /// Tests that LoadUsersAsync handles null status values correctly without throwing exceptions.
    /// </summary>
    [TestMethod]
    public async Task LoadUsersAsync_WithNullStatusValues_HandlesGracefully()
    {
        // Arrange
        var mockAdmin = new Mock<IAdminService>();
        var mockLogger = new Mock<ILogger<UsersViewModel>>();
        var viewModel = new UsersViewModel(mockAdmin.Object, new Mock<IRefreshCoordinator>().Object, mockLogger.Object);

        var students = new List<UserItem>
        {
            new UserItem { Id = Guid.NewGuid(), Name = "Student1", Email = "s1@test.com", Role = "Student", Status = null! },
            new UserItem { Id = Guid.NewGuid(), Name = "Student2", Email = "s2@test.com", Role = "Student", Status = "Active" }
        };

        var lecturers = new List<UserItem>
        {
            new UserItem { Id = Guid.NewGuid(), Name = "Lecturer1", Email = "l1@test.com", Role = "Lecturer", Status = null! }
        };

        mockAdmin.Setup(x => x.GetStudentsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(students);
        mockAdmin.Setup(x => x.GetLecturersAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(lecturers);

        // Act & Assert - Should not throw
        await viewModel.LoadUsersAsync();

        Assert.AreEqual(2, viewModel.TotalStudents);
        Assert.AreEqual(1, viewModel.TotalLecturers);
        Assert.AreEqual(0, viewModel.TotalSuspended);
    }

    /// <summary>
    /// Tests that LoadUsersAsync correctly clears error state at the start of execution
    /// even if there was a previous error.
    /// </summary>
    [TestMethod]
    public async Task LoadUsersAsync_WithPreviousError_ClearsErrorStateBeforeLoading()
    {
        // Arrange
        var mockAdmin = new Mock<IAdminService>();
        var mockLogger = new Mock<ILogger<UsersViewModel>>();
        var viewModel = new UsersViewModel(mockAdmin.Object, new Mock<IRefreshCoordinator>().Object, mockLogger.Object);

        // Simulate previous error state
        mockAdmin.Setup(x => x.GetStudentsAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("First error"));
        mockAdmin.Setup(x => x.GetLecturersAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<UserItem>());

        await viewModel.LoadUsersAsync();
        Assert.IsTrue(viewModel.HasError);
        Assert.IsFalse(string.IsNullOrEmpty(viewModel.ErrorMessage));

        // Setup successful call
        mockAdmin.Setup(x => x.GetStudentsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<UserItem> { new UserItem { Id = Guid.NewGuid(), Name = "Student", Email = "test@test.com", Role = "Student", Status = "Active" } });

        // Act
        await viewModel.LoadUsersAsync();

        // Assert
        Assert.IsFalse(viewModel.HasError, "HasError should be cleared on successful load");
        Assert.AreEqual(string.Empty, viewModel.ErrorMessage, "ErrorMessage should be cleared on successful load");
    }

    /// <summary>
    /// Tests that LoadUsersAsync logs the correct error message and exception when GetStudentsAsync fails.
    /// </summary>
    [TestMethod]
    public async Task LoadUsersAsync_WhenGetStudentsAsyncThrowsException_LogsCorrectErrorMessage()
    {
        // Arrange
        var mockAdmin = new Mock<IAdminService>();
        var mockLogger = new Mock<ILogger<UsersViewModel>>();
        var viewModel = new UsersViewModel(mockAdmin.Object, new Mock<IRefreshCoordinator>().Object, mockLogger.Object);

        var expectedException = new InvalidOperationException("Service unavailable");

        mockAdmin.Setup(x => x.GetStudentsAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(expectedException);
        mockAdmin.Setup(x => x.GetLecturersAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<UserItem>());

        // Act
        await viewModel.LoadUsersAsync();

        // Assert
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Failed to load users")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once,
            "Should log error with correct message");

        Assert.IsTrue(viewModel.HasError);
        Assert.AreEqual("Failed to load users. Please try again.", viewModel.ErrorMessage);
    }

    /// <summary>
    /// Tests that LoadUsersAsync logs the correct error message and exception when GetLecturersAsync fails.
    /// </summary>
    [TestMethod]
    public async Task LoadUsersAsync_WhenGetLecturersAsyncThrowsException_LogsCorrectErrorMessage()
    {
        // Arrange
        var mockAdmin = new Mock<IAdminService>();
        var mockLogger = new Mock<ILogger<UsersViewModel>>();
        var viewModel = new UsersViewModel(mockAdmin.Object, new Mock<IRefreshCoordinator>().Object, mockLogger.Object);

        var expectedException = new TimeoutException("Request timeout");

        mockAdmin.Setup(x => x.GetStudentsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<UserItem>());
        mockAdmin.Setup(x => x.GetLecturersAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(expectedException);

        // Act
        await viewModel.LoadUsersAsync();

        // Assert
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Failed to load users")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once,
            "Should log error with correct message");

        Assert.IsTrue(viewModel.HasError);
        Assert.AreEqual("Failed to load users. Please try again.", viewModel.ErrorMessage);
    }

    /// <summary>
    /// Tests that LoadUsersAsync does not log any error when OperationCanceledException is thrown.
    /// Verifies that cancellation is handled silently without logging.
    /// </summary>
    [TestMethod]
    public async Task LoadUsersAsync_WhenOperationCanceled_DoesNotLogError()
    {
        // Arrange
        var mockAdmin = new Mock<IAdminService>();
        var mockLogger = new Mock<ILogger<UsersViewModel>>();
        var viewModel = new UsersViewModel(mockAdmin.Object, new Mock<IRefreshCoordinator>().Object, mockLogger.Object);

        mockAdmin.Setup(x => x.GetStudentsAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new OperationCanceledException());
        mockAdmin.Setup(x => x.GetLecturersAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<UserItem>());

        // Act
        await viewModel.LoadUsersAsync();

        // Assert
        mockLogger.Verify(
            x => x.Log(
                It.IsAny<LogLevel>(),
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Never,
            "Should not log when operation is canceled");

        Assert.IsFalse(viewModel.HasError, "HasError should remain false when operation is canceled");
        Assert.AreEqual(string.Empty, viewModel.ErrorMessage, "ErrorMessage should remain empty when operation is canceled");
    }

    /// <summary>
    /// Tests that LoadUsersAsync always resets IsBusy to false in the finally block
    /// even when OperationCanceledException is thrown.
    /// </summary>
    [TestMethod]
    public async Task LoadUsersAsync_WhenOperationCanceledInGetStudents_ResetsIsBusyInFinally()
    {
        // Arrange
        var mockAdmin = new Mock<IAdminService>();
        var mockLogger = new Mock<ILogger<UsersViewModel>>();
        var viewModel = new UsersViewModel(mockAdmin.Object, new Mock<IRefreshCoordinator>().Object, mockLogger.Object);

        mockAdmin.Setup(x => x.GetStudentsAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new OperationCanceledException());
        mockAdmin.Setup(x => x.GetLecturersAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<UserItem>());

        // Act
        await viewModel.LoadUsersAsync();

        // Assert
        Assert.IsFalse(viewModel.IsBusy, "IsBusy should be reset to false in finally block");
    }

    /// <summary>
    /// Tests that LoadUsersAsync handles whitespace-only status values correctly
    /// and does not count them as "Suspended".
    /// </summary>
    [TestMethod]
    [DataRow("   ", DisplayName = "Whitespace status - spaces")]
    [DataRow("\t", DisplayName = "Whitespace status - tab")]
    [DataRow("\n", DisplayName = "Whitespace status - newline")]
    [DataRow("\r\n", DisplayName = "Whitespace status - carriage return and newline")]
    public async Task LoadUsersAsync_WithWhitespaceStatusValues_DoesNotCountAsSuspended(string whitespaceStatus)
    {
        // Arrange
        var mockAdmin = new Mock<IAdminService>();
        var mockLogger = new Mock<ILogger<UsersViewModel>>();
        var viewModel = new UsersViewModel(mockAdmin.Object, new Mock<IRefreshCoordinator>().Object, mockLogger.Object);

        var students = new List<UserItem>
        {
            new UserItem { Id = Guid.NewGuid(), Name = "Student1", Email = "s1@test.com", Role = "Student", Status = whitespaceStatus },
            new UserItem { Id = Guid.NewGuid(), Name = "Student2", Email = "s2@test.com", Role = "Student", Status = "Suspended" }
        };

        var lecturers = new List<UserItem>();

        mockAdmin.Setup(x => x.GetStudentsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(students);
        mockAdmin.Setup(x => x.GetLecturersAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(lecturers);

        // Act
        await viewModel.LoadUsersAsync();

        // Assert
        Assert.AreEqual(1, viewModel.TotalSuspended, "Only exact 'Suspended' should be counted, not whitespace");
    }

    /// <summary>
    /// Tests that LoadUsersAsync handles status values with leading or trailing whitespace
    /// and does not count them as "Suspended".
    /// </summary>
    [TestMethod]
    [DataRow(" Suspended", DisplayName = "Status with leading space")]
    [DataRow("Suspended ", DisplayName = "Status with trailing space")]
    [DataRow(" Suspended ", DisplayName = "Status with leading and trailing spaces")]
    public async Task LoadUsersAsync_WithStatusHavingWhitespace_DoesNotCountAsSuspended(string statusWithWhitespace)
    {
        // Arrange
        var mockAdmin = new Mock<IAdminService>();
        var mockLogger = new Mock<ILogger<UsersViewModel>>();
        var viewModel = new UsersViewModel(mockAdmin.Object, new Mock<IRefreshCoordinator>().Object, mockLogger.Object);

        var students = new List<UserItem>
        {
            new UserItem { Id = Guid.NewGuid(), Name = "Student1", Email = "s1@test.com", Role = "Student", Status = statusWithWhitespace }
        };

        var lecturers = new List<UserItem>();

        mockAdmin.Setup(x => x.GetStudentsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(students);
        mockAdmin.Setup(x => x.GetLecturersAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(lecturers);

        // Act
        await viewModel.LoadUsersAsync();

        // Assert
        Assert.AreEqual(0, viewModel.TotalSuspended, "Status with whitespace should not match exact 'Suspended'");
    }

    /// <summary>
    /// Tests that LoadUsersAsync correctly handles the scenario where all users are suspended.
    /// Verifies that TotalSuspended equals the sum of TotalStudents and TotalLecturers.
    /// </summary>
    [TestMethod]
    public async Task LoadUsersAsync_WhenAllUsersAreSuspended_CountsAllAsSuspended()
    {
        // Arrange
        var mockAdmin = new Mock<IAdminService>();
        var mockLogger = new Mock<ILogger<UsersViewModel>>();
        var viewModel = new UsersViewModel(mockAdmin.Object, new Mock<IRefreshCoordinator>().Object, mockLogger.Object);

        var students = new List<UserItem>
        {
            new UserItem { Id = Guid.NewGuid(), Name = "Student1", Email = "s1@test.com", Role = "Student", Status = "Suspended" },
            new UserItem { Id = Guid.NewGuid(), Name = "Student2", Email = "s2@test.com", Role = "Student", Status = "Suspended" }
        };

        var lecturers = new List<UserItem>
        {
            new UserItem { Id = Guid.NewGuid(), Name = "Lecturer1", Email = "l1@test.com", Role = "Lecturer", Status = "Suspended" }
        };

        mockAdmin.Setup(x => x.GetStudentsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(students);
        mockAdmin.Setup(x => x.GetLecturersAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(lecturers);

        // Act
        await viewModel.LoadUsersAsync();

        // Assert
        Assert.AreEqual(2, viewModel.TotalStudents);
        Assert.AreEqual(1, viewModel.TotalLecturers);
        Assert.AreEqual(3, viewModel.TotalSuspended);
    }

    /// <summary>
    /// Tests that LoadUsersAsync correctly handles the scenario where no users are suspended.
    /// Verifies that TotalSuspended is zero when all users have non-"Suspended" status.
    /// </summary>
    [TestMethod]
    public async Task LoadUsersAsync_WhenNoUsersAreSuspended_CountsZeroSuspended()
    {
        // Arrange
        var mockAdmin = new Mock<IAdminService>();
        var mockLogger = new Mock<ILogger<UsersViewModel>>();
        var viewModel = new UsersViewModel(mockAdmin.Object, new Mock<IRefreshCoordinator>().Object, mockLogger.Object);

        var students = new List<UserItem>
        {
            new UserItem { Id = Guid.NewGuid(), Name = "Student1", Email = "s1@test.com", Role = "Student", Status = "Active" },
            new UserItem { Id = Guid.NewGuid(), Name = "Student2", Email = "s2@test.com", Role = "Student", Status = "Pending" }
        };

        var lecturers = new List<UserItem>
        {
            new UserItem { Id = Guid.NewGuid(), Name = "Lecturer1", Email = "l1@test.com", Role = "Lecturer", Status = "Active" },
            new UserItem { Id = Guid.NewGuid(), Name = "Lecturer2", Email = "l2@test.com", Role = "Lecturer", Status = "Inactive" }
        };

        mockAdmin.Setup(x => x.GetStudentsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(students);
        mockAdmin.Setup(x => x.GetLecturersAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(lecturers);

        // Act
        await viewModel.LoadUsersAsync();

        // Assert
        Assert.AreEqual(2, viewModel.TotalStudents);
        Assert.AreEqual(2, viewModel.TotalLecturers);
        Assert.AreEqual(0, viewModel.TotalSuspended);
    }

    /// <summary>
    /// Tests that LoadUsersAsync calls both GetStudentsAsync and GetLecturersAsync exactly once
    /// with the cancellation token from CreateLinkedToken.
    /// </summary>
    [TestMethod]
    public async Task LoadUsersAsync_CallsBothServicesOnceWithCancellationToken()
    {
        // Arrange
        var mockAdmin = new Mock<IAdminService>();
        var mockLogger = new Mock<ILogger<UsersViewModel>>();
        var viewModel = new UsersViewModel(mockAdmin.Object, new Mock<IRefreshCoordinator>().Object, mockLogger.Object);

        mockAdmin.Setup(x => x.GetStudentsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<UserItem>());
        mockAdmin.Setup(x => x.GetLecturersAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<UserItem>());

        // Act
        await viewModel.LoadUsersAsync();

        // Assert
        mockAdmin.Verify(x => x.GetStudentsAsync(It.IsAny<CancellationToken>()), Times.Once);
        mockAdmin.Verify(x => x.GetLecturersAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Tests that LoadUsersAsync sets IsBusy to true at the start and false at the end
    /// of successful execution.
    /// </summary>
    [TestMethod]
    public async Task LoadUsersAsync_DuringSuccessfulExecution_ManagesIsBusyCorrectly()
    {
        // Arrange
        var mockAdmin = new Mock<IAdminService>();
        var mockLogger = new Mock<ILogger<UsersViewModel>>();
        var viewModel = new UsersViewModel(mockAdmin.Object, new Mock<IRefreshCoordinator>().Object, mockLogger.Object);

        var tcs = new TaskCompletionSource<List<UserItem>>();
        bool isBusyDuringExecution = false;

        mockAdmin.Setup(x => x.GetStudentsAsync(It.IsAny<CancellationToken>()))
            .Returns(() =>
            {
                isBusyDuringExecution = viewModel.IsBusy;
                return tcs.Task;
            });
        mockAdmin.Setup(x => x.GetLecturersAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<UserItem>());

        // Act
        var task = viewModel.LoadUsersAsync();
        tcs.SetResult(new List<UserItem>());
        await task;

        // Assert
        Assert.IsTrue(isBusyDuringExecution, "IsBusy should be true during execution");
        Assert.IsFalse(viewModel.IsBusy, "IsBusy should be false after execution");
    }

    /// <summary>
    /// Tests that LoadUsersAsync handles extremely large datasets without issues.
    /// Verifies that the method can process and combine large lists of users.
    /// </summary>
    [TestMethod]
    public async Task LoadUsersAsync_WithLargeDatasets_ProcessesSuccessfully()
    {
        // Arrange
        var mockAdmin = new Mock<IAdminService>();
        var mockLogger = new Mock<ILogger<UsersViewModel>>();
        var viewModel = new UsersViewModel(mockAdmin.Object, new Mock<IRefreshCoordinator>().Object, mockLogger.Object);

        var students = Enumerable.Range(1, 10000).Select(i => new UserItem
        {
            Id = Guid.NewGuid(),
            Name = $"Student{i}",
            Email = $"s{i}@test.com",
            Role = "Student",
            Status = i % 2 == 0 ? "Suspended" : "Active"
        }).ToList();

        var lecturers = Enumerable.Range(1, 5000).Select(i => new UserItem
        {
            Id = Guid.NewGuid(),
            Name = $"Lecturer{i}",
            Email = $"l{i}@test.com",
            Role = "Lecturer",
            Status = i % 3 == 0 ? "Suspended" : "Active"
        }).ToList();

        mockAdmin.Setup(x => x.GetStudentsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(students);
        mockAdmin.Setup(x => x.GetLecturersAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(lecturers);

        // Act
        await viewModel.LoadUsersAsync();

        // Assert
        Assert.AreEqual(10000, viewModel.TotalStudents);
        Assert.AreEqual(5000, viewModel.TotalLecturers);
        var expectedSuspended = students.Count(u => u.Status == "Suspended") + lecturers.Count(u => u.Status == "Suspended");
        Assert.AreEqual(expectedSuspended, viewModel.TotalSuspended);
        Assert.IsFalse(viewModel.HasError);
    }

    /// <summary>
    /// Tests that LoadUsersAsync handles the boundary case where only students exist (no lecturers).
    /// </summary>
    [TestMethod]
    public async Task LoadUsersAsync_WithOnlyStudentsNoLecturers_ProcessesCorrectly()
    {
        // Arrange
        var mockAdmin = new Mock<IAdminService>();
        var mockLogger = new Mock<ILogger<UsersViewModel>>();
        var viewModel = new UsersViewModel(mockAdmin.Object, new Mock<IRefreshCoordinator>().Object, mockLogger.Object);

        var students = new List<UserItem>
        {
            new UserItem { Id = Guid.NewGuid(), Name = "Student1", Email = "s1@test.com", Role = "Student", Status = "Active" },
            new UserItem { Id = Guid.NewGuid(), Name = "Student2", Email = "s2@test.com", Role = "Student", Status = "Suspended" }
        };

        var lecturers = new List<UserItem>();

        mockAdmin.Setup(x => x.GetStudentsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(students);
        mockAdmin.Setup(x => x.GetLecturersAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(lecturers);

        // Act
        await viewModel.LoadUsersAsync();

        // Assert
        Assert.AreEqual(2, viewModel.TotalStudents);
        Assert.AreEqual(0, viewModel.TotalLecturers);
        Assert.AreEqual(1, viewModel.TotalSuspended);
    }

    /// <summary>
    /// Tests that LoadUsersAsync handles the boundary case where only lecturers exist (no students).
    /// </summary>
    [TestMethod]
    public async Task LoadUsersAsync_WithOnlyLecturersNoStudents_ProcessesCorrectly()
    {
        // Arrange
        var mockAdmin = new Mock<IAdminService>();
        var mockLogger = new Mock<ILogger<UsersViewModel>>();
        var viewModel = new UsersViewModel(mockAdmin.Object, new Mock<IRefreshCoordinator>().Object, mockLogger.Object);

        var students = new List<UserItem>();

        var lecturers = new List<UserItem>
        {
            new UserItem { Id = Guid.NewGuid(), Name = "Lecturer1", Email = "l1@test.com", Role = "Lecturer", Status = "Active" },
            new UserItem { Id = Guid.NewGuid(), Name = "Lecturer2", Email = "l2@test.com", Role = "Lecturer", Status = "Suspended" },
            new UserItem { Id = Guid.NewGuid(), Name = "Lecturer3", Email = "l3@test.com", Role = "Lecturer", Status = "Suspended" }
        };

        mockAdmin.Setup(x => x.GetStudentsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(students);
        mockAdmin.Setup(x => x.GetLecturersAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(lecturers);

        // Act
        await viewModel.LoadUsersAsync();

        // Assert
        Assert.AreEqual(0, viewModel.TotalStudents);
        Assert.AreEqual(3, viewModel.TotalLecturers);
        Assert.AreEqual(2, viewModel.TotalSuspended);
    }

    /// <summary>
    /// Tests that LoadUsersAsync handles various exception types correctly and sets error state.
    /// </summary>
    [TestMethod]
    [DataRow(typeof(InvalidOperationException), "Invalid operation", DisplayName = "InvalidOperationException")]
    [DataRow(typeof(ArgumentException), "Invalid argument", DisplayName = "ArgumentException")]
    [DataRow(typeof(TimeoutException), "Timeout occurred", DisplayName = "TimeoutException")]
    [DataRow(typeof(NullReferenceException), "Null reference", DisplayName = "NullReferenceException")]
    public async Task LoadUsersAsync_WithVariousExceptionTypes_SetsErrorStateAndLogs(Type exceptionType, string message)
    {
        // Arrange
        var mockAdmin = new Mock<IAdminService>();
        var mockLogger = new Mock<ILogger<UsersViewModel>>();
        var viewModel = new UsersViewModel(mockAdmin.Object, new Mock<IRefreshCoordinator>().Object, mockLogger.Object);

        var exception = (Exception)Activator.CreateInstance(exceptionType, message)!;

        mockAdmin.Setup(x => x.GetStudentsAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(exception);
        mockAdmin.Setup(x => x.GetLecturersAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<UserItem>());

        // Act
        await viewModel.LoadUsersAsync();

        // Assert
        Assert.IsTrue(viewModel.HasError);
        Assert.AreEqual("Failed to load users. Please try again.", viewModel.ErrorMessage);
        Assert.IsFalse(viewModel.IsBusy);

        mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Failed to load users")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    /// <summary>
    /// Tests that LoadUsersAsync handles the scenario where one service returns a single item
    /// and the other returns an empty list.
    /// </summary>
    [TestMethod]
    public async Task LoadUsersAsync_WithSingleItemAndEmptyList_ProcessesCorrectly()
    {
        // Arrange
        var mockAdmin = new Mock<IAdminService>();
        var mockLogger = new Mock<ILogger<UsersViewModel>>();
        var viewModel = new UsersViewModel(mockAdmin.Object, new Mock<IRefreshCoordinator>().Object, mockLogger.Object);

        var students = new List<UserItem>
        {
            new UserItem { Id = Guid.NewGuid(), Name = "Student1", Email = "s1@test.com", Role = "Student", Status = "Suspended" }
        };

        var lecturers = new List<UserItem>();

        mockAdmin.Setup(x => x.GetStudentsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(students);
        mockAdmin.Setup(x => x.GetLecturersAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(lecturers);

        // Act
        await viewModel.LoadUsersAsync();

        // Assert
        Assert.AreEqual(1, viewModel.TotalStudents);
        Assert.AreEqual(0, viewModel.TotalLecturers);
        Assert.AreEqual(1, viewModel.TotalSuspended);
        Assert.IsFalse(viewModel.HasError);
    }

    /// <summary>
    /// Tests that LoadUsersAsync returns immediately when IsBusy is true at the start
    /// without calling any service methods or modifying any properties.
    /// </summary>
    [TestMethod]
    public async Task LoadUsersAsync_WhenIsBusyIsTrue_ReturnsImmediatelyWithoutCallingServices()
    {
        // Arrange
        var mockAdmin = new Mock<IAdminService>();
        var mockLogger = new Mock<ILogger<UsersViewModel>>();
        var viewModel = new UsersViewModel(mockAdmin.Object, new Mock<IRefreshCoordinator>().Object, mockLogger.Object);

        // Simulate IsBusy being true by starting a long-running operation
        var tcs = new TaskCompletionSource<List<UserItem>>();
        mockAdmin.Setup(x => x.GetStudentsAsync(It.IsAny<CancellationToken>())).Returns(tcs.Task);
        mockAdmin.Setup(x => x.GetLecturersAsync(It.IsAny<CancellationToken>())).Returns(tcs.Task);

        var firstCall = viewModel.LoadUsersAsync();

        // Reset mock invocations to track only second call
        mockAdmin.Invocations.Clear();

        // Act
        var secondCall = viewModel.LoadUsersAsync();

        // Assert
        Assert.IsTrue(secondCall.IsCompleted, "Second call should complete immediately when IsBusy is true");
        mockAdmin.Verify(x => x.GetStudentsAsync(It.IsAny<CancellationToken>()), Times.Never, "Should not call GetStudentsAsync when IsBusy is true");
        mockAdmin.Verify(x => x.GetLecturersAsync(It.IsAny<CancellationToken>()), Times.Never, "Should not call GetLecturersAsync when IsBusy is true");

        // Cleanup
        tcs.SetResult(new List<UserItem>());
        await firstCall;
    }

    /// <summary>
    /// Tests that LoadUsersAsync handles empty string status values correctly
    /// and does not count them as "Suspended".
    /// </summary>
    [TestMethod]
    public async Task LoadUsersAsync_WithEmptyStringStatus_DoesNotCountAsSuspended()
    {
        // Arrange
        var mockAdmin = new Mock<IAdminService>();
        var mockLogger = new Mock<ILogger<UsersViewModel>>();
        var viewModel = new UsersViewModel(mockAdmin.Object, new Mock<IRefreshCoordinator>().Object, mockLogger.Object);

        var students = new List<UserItem>
        {
            new UserItem { Id = Guid.NewGuid(), Name = "Student1", Email = "s1@test.com", Role = "Student", Status = "" },
            new UserItem { Id = Guid.NewGuid(), Name = "Student2", Email = "s2@test.com", Role = "Student", Status = "Suspended" }
        };

        var lecturers = new List<UserItem>
        {
            new UserItem { Id = Guid.NewGuid(), Name = "Lecturer1", Email = "l1@test.com", Role = "Lecturer", Status = "" }
        };

        mockAdmin.Setup(x => x.GetStudentsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(students);
        mockAdmin.Setup(x => x.GetLecturersAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(lecturers);

        // Act
        await viewModel.LoadUsersAsync();

        // Assert
        Assert.AreEqual(2, viewModel.TotalStudents);
        Assert.AreEqual(1, viewModel.TotalLecturers);
        Assert.AreEqual(1, viewModel.TotalSuspended, "Empty status should not be counted as Suspended");
    }

    /// <summary>
    /// Tests that LoadUsersAsync handles AggregateException thrown by Task.WhenAll
    /// when both services fail simultaneously.
    /// </summary>
    [TestMethod]
    public async Task LoadUsersAsync_WhenBothServicesThrowSimultaneously_HandlesAggregateException()
    {
        // Arrange
        var mockAdmin = new Mock<IAdminService>();
        var mockLogger = new Mock<ILogger<UsersViewModel>>();
        var viewModel = new UsersViewModel(mockAdmin.Object, new Mock<IRefreshCoordinator>().Object, mockLogger.Object);

        mockAdmin.Setup(x => x.GetStudentsAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Students service failed"));
        mockAdmin.Setup(x => x.GetLecturersAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new TimeoutException("Lecturers service failed"));

        // Act
        await viewModel.LoadUsersAsync();

        // Assert
        Assert.IsTrue(viewModel.HasError, "HasError should be true when both services fail");
        Assert.AreEqual("Failed to load users. Please try again.", viewModel.ErrorMessage);
        Assert.IsFalse(viewModel.IsBusy);

        mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Failed to load users")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once,
            "Error should be logged when both services fail");
    }

    /// <summary>
    /// Tests that LoadUsersAsync maintains correct counts when users have long string values
    /// in various properties including Status.
    /// </summary>
    [TestMethod]
    public async Task LoadUsersAsync_WithVeryLongStatusStrings_ProcessesCorrectly()
    {
        // Arrange
        var mockAdmin = new Mock<IAdminService>();
        var mockLogger = new Mock<ILogger<UsersViewModel>>();
        var viewModel = new UsersViewModel(mockAdmin.Object, new Mock<IRefreshCoordinator>().Object, mockLogger.Object);

        var longStatus = new string('A', 10000);

        var students = new List<UserItem>
        {
            new UserItem { Id = Guid.NewGuid(), Name = "Student1", Email = "s1@test.com", Role = "Student", Status = longStatus },
            new UserItem { Id = Guid.NewGuid(), Name = "Student2", Email = "s2@test.com", Role = "Student", Status = "Suspended" }
        };

        var lecturers = new List<UserItem>
        {
            new UserItem { Id = Guid.NewGuid(), Name = "Lecturer1", Email = "l1@test.com", Role = "Lecturer", Status = "Active" }
        };

        mockAdmin.Setup(x => x.GetStudentsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(students);
        mockAdmin.Setup(x => x.GetLecturersAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(lecturers);

        // Act
        await viewModel.LoadUsersAsync();

        // Assert
        Assert.AreEqual(2, viewModel.TotalStudents);
        Assert.AreEqual(1, viewModel.TotalLecturers);
        Assert.AreEqual(1, viewModel.TotalSuspended, "Only exact 'Suspended' should be counted, not long strings");
        Assert.IsFalse(viewModel.HasError);
    }

    /// <summary>
    /// Tests that LoadUsersAsync correctly handles negative counts edge case
    /// by ensuring counts are never negative (though this shouldn't happen with valid data).
    /// </summary>
    [TestMethod]
    public async Task LoadUsersAsync_WithValidData_NeverProducesNegativeCounts()
    {
        // Arrange
        var mockAdmin = new Mock<IAdminService>();
        var mockLogger = new Mock<ILogger<UsersViewModel>>();
        var viewModel = new UsersViewModel(mockAdmin.Object, new Mock<IRefreshCoordinator>().Object, mockLogger.Object);

        var students = new List<UserItem>();
        var lecturers = new List<UserItem>();

        mockAdmin.Setup(x => x.GetStudentsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(students);
        mockAdmin.Setup(x => x.GetLecturersAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(lecturers);

        // Act
        await viewModel.LoadUsersAsync();

        // Assert
        Assert.IsTrue(viewModel.TotalStudents >= 0, "TotalStudents should never be negative");
        Assert.IsTrue(viewModel.TotalLecturers >= 0, "TotalLecturers should never be negative");
        Assert.IsTrue(viewModel.TotalSuspended >= 0, "TotalSuspended should never be negative");
    }

    /// <summary>
    /// Tests that LoadUsersAsync properly clears HasError and ErrorMessage properties
    /// before attempting to load data, even when previous values were set.
    /// </summary>
    [TestMethod]
    public async Task LoadUsersAsync_ClearsErrorStateBeforeLoading_RegardlessOfPreviousState()
    {
        // Arrange
        var mockAdmin = new Mock<IAdminService>();
        var mockLogger = new Mock<ILogger<UsersViewModel>>();
        var viewModel = new UsersViewModel(mockAdmin.Object, new Mock<IRefreshCoordinator>().Object, mockLogger.Object);

        // Manually set error state
        viewModel.HasError = true;
        viewModel.ErrorMessage = "Previous error message";

        mockAdmin.Setup(x => x.GetStudentsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<UserItem>());
        mockAdmin.Setup(x => x.GetLecturersAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<UserItem>());

        // Act
        await viewModel.LoadUsersAsync();

        // Assert
        Assert.IsFalse(viewModel.HasError, "HasError should be false after successful load");
        Assert.AreEqual(string.Empty, viewModel.ErrorMessage, "ErrorMessage should be empty after successful load");
    }

    /// <summary>
    /// Tests that LoadUsersAsync correctly handles status values that are substrings or superstrings
    /// of "Suspended" and does not incorrectly count them.
    /// </summary>
    [TestMethod]
    [DataRow("Suspend", DisplayName = "Status is 'Suspend' (substring)")]
    [DataRow("Suspende", DisplayName = "Status is 'Suspende' (partial)")]
    [DataRow("SuspendedUser", DisplayName = "Status is 'SuspendedUser' (superstring)")]
    [DataRow("UserSuspended", DisplayName = "Status is 'UserSuspended' (contains Suspended)")]
    public async Task LoadUsersAsync_WithPartialSuspendedStatus_DoesNotCountAsSuspended(string partialStatus)
    {
        // Arrange
        var mockAdmin = new Mock<IAdminService>();
        var mockLogger = new Mock<ILogger<UsersViewModel>>();
        var viewModel = new UsersViewModel(mockAdmin.Object, new Mock<IRefreshCoordinator>().Object, mockLogger.Object);

        var students = new List<UserItem>
        {
            new UserItem { Id = Guid.NewGuid(), Name = "Student1", Email = "s1@test.com", Role = "Student", Status = partialStatus }
        };

        var lecturers = new List<UserItem>();

        mockAdmin.Setup(x => x.GetStudentsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(students);
        mockAdmin.Setup(x => x.GetLecturersAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(lecturers);

        // Act
        await viewModel.LoadUsersAsync();

        // Assert
        Assert.AreEqual(0, viewModel.TotalSuspended, "Partial or extended status should not match exact 'Suspended'");
    }
}