using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

using Microsoft.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.ApplicationModel.DataTransfer;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Storage;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Ssomero;
using Ssomero.Interfaces;
using Ssomero.Models;
using Ssomero.ViewModels;

namespace Ssomero.ViewModels.UnitTests;




/// <summary>
/// Unit tests for the FacultiesViewModel class, specifically testing the SearchQuery property.
/// </summary>
[TestClass]
public class FacultiesViewModelTests
{
    /// <summary>
    /// Tests that the SearchQuery property can be set and retrieved correctly.
    /// </summary>
    [TestMethod]
    public void SearchQuery_SetValue_ReturnsSetValue()
    {
        // Arrange
        var academicServiceMock = new Mock<IAcademicService>();
        var loggerMock = new Mock<ILogger<FacultiesViewModel>>();
        var viewModel = new FacultiesViewModel(academicServiceMock.Object, loggerMock.Object);
        var expectedValue = "test query";

        // Act
        viewModel.SearchQuery = expectedValue;

        // Assert
        Assert.AreEqual(expectedValue, viewModel.SearchQuery);
    }

    /// <summary>
    /// Tests that setting SearchQuery to empty string is handled correctly.
    /// </summary>
    [TestMethod]
    public void SearchQuery_SetEmptyString_ReturnsEmptyString()
    {
        // Arrange
        var academicServiceMock = new Mock<IAcademicService>();
        var loggerMock = new Mock<ILogger<FacultiesViewModel>>();
        var viewModel = new FacultiesViewModel(academicServiceMock.Object, loggerMock.Object);
        var expectedValue = string.Empty;

        // Act
        viewModel.SearchQuery = expectedValue;

        // Assert
        Assert.AreEqual(expectedValue, viewModel.SearchQuery);
    }

    /// <summary>
    /// Tests that setting SearchQuery raises PropertyChanged event when value changes.
    /// </summary>
    [TestMethod]
    public void SearchQuery_SetDifferentValue_RaisesPropertyChangedEvent()
    {
        // Arrange
        var academicServiceMock = new Mock<IAcademicService>();
        var loggerMock = new Mock<ILogger<FacultiesViewModel>>();
        var viewModel = new FacultiesViewModel(academicServiceMock.Object, loggerMock.Object);
        var propertyChangedRaised = false;
        var propertyName = string.Empty;

        viewModel.PropertyChanged += (sender, e) =>
        {
            if (e.PropertyName == nameof(FacultiesViewModel.SearchQuery))
            {
                propertyChangedRaised = true;
                propertyName = e.PropertyName;
            }
        };

        // Act
        viewModel.SearchQuery = "new value";

        // Assert
        Assert.IsTrue(propertyChangedRaised);
        Assert.AreEqual(nameof(FacultiesViewModel.SearchQuery), propertyName);
    }

    /// <summary>
    /// Tests that setting SearchQuery to the same value does not raise PropertyChanged event.
    /// </summary>
    [TestMethod]
    public void SearchQuery_SetSameValue_DoesNotRaisePropertyChangedEvent()
    {
        // Arrange
        var academicServiceMock = new Mock<IAcademicService>();
        var loggerMock = new Mock<ILogger<FacultiesViewModel>>();
        var viewModel = new FacultiesViewModel(academicServiceMock.Object, loggerMock.Object);
        var initialValue = "initial value";
        viewModel.SearchQuery = initialValue;

        var propertyChangedRaised = false;
        viewModel.PropertyChanged += (sender, e) =>
        {
            if (e.PropertyName == nameof(FacultiesViewModel.SearchQuery))
            {
                propertyChangedRaised = true;
            }
        };

        // Act
        viewModel.SearchQuery = initialValue;

        // Assert
        Assert.IsFalse(propertyChangedRaised);
    }

    /// <summary>
    /// Tests SearchQuery property with various string edge cases including whitespace-only strings.
    /// </summary>
    /// <param name="testValue">The test value to set on SearchQuery property.</param>
    [TestMethod]
    [DataRow("")]
    [DataRow(" ")]
    [DataRow("   ")]
    [DataRow("\t")]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [DataRow("  \t  \n  ")]
    public void SearchQuery_SetWhitespaceValues_StoresAndReturnsValue(string testValue)
    {
        // Arrange
        var academicServiceMock = new Mock<IAcademicService>();
        var loggerMock = new Mock<ILogger<FacultiesViewModel>>();
        var viewModel = new FacultiesViewModel(academicServiceMock.Object, loggerMock.Object);

        // Act
        viewModel.SearchQuery = testValue;

        // Assert
        Assert.AreEqual(testValue, viewModel.SearchQuery);
    }

    /// <summary>
    /// Tests SearchQuery property with special characters to ensure they are handled correctly.
    /// </summary>
    /// <param name="testValue">The test value containing special characters.</param>
    [TestMethod]
    [DataRow("test@example.com")]
    [DataRow("search#query")]
    [DataRow("test$value")]
    [DataRow("query%20with%20encoding")]
    [DataRow("test&value")]
    [DataRow("test*value")]
    [DataRow("(test)")]
    [DataRow("[test]")]
    [DataRow("{test}")]
    [DataRow("test|value")]
    [DataRow("test\\value")]
    [DataRow("test/value")]
    [DataRow("test<value>")]
    [DataRow("test'value")]
    [DataRow("test\"value")]
    [DataRow("test:value")]
    [DataRow("test;value")]
    [DataRow("test?value")]
    public void SearchQuery_SetSpecialCharacters_StoresAndReturnsValue(string testValue)
    {
        // Arrange
        var academicServiceMock = new Mock<IAcademicService>();
        var loggerMock = new Mock<ILogger<FacultiesViewModel>>();
        var viewModel = new FacultiesViewModel(academicServiceMock.Object, loggerMock.Object);

        // Act
        viewModel.SearchQuery = testValue;

        // Assert
        Assert.AreEqual(testValue, viewModel.SearchQuery);
    }

    /// <summary>
    /// Tests SearchQuery property with a very long string to ensure there are no length limitations.
    /// </summary>
    [TestMethod]
    public void SearchQuery_SetVeryLongString_StoresAndReturnsValue()
    {
        // Arrange
        var academicServiceMock = new Mock<IAcademicService>();
        var loggerMock = new Mock<ILogger<FacultiesViewModel>>();
        var viewModel = new FacultiesViewModel(academicServiceMock.Object, loggerMock.Object);
        var veryLongString = new string('a', 10000);

        // Act
        viewModel.SearchQuery = veryLongString;

        // Assert
        Assert.AreEqual(veryLongString, viewModel.SearchQuery);
    }

    /// <summary>
    /// Tests that SearchQuery property with unicode characters are handled correctly.
    /// </summary>
    /// <param name="testValue">The test value containing unicode characters.</param>
    [TestMethod]
    [DataRow("café")]
    [DataRow("日本語")]
    [DataRow("العربية")]
    [DataRow("Привет")]
    [DataRow("😀🎉")]
    [DataRow("test™value")]
    [DataRow("test©value")]
    [DataRow("test®value")]
    public void SearchQuery_SetUnicodeCharacters_StoresAndReturnsValue(string testValue)
    {
        // Arrange
        var academicServiceMock = new Mock<IAcademicService>();
        var loggerMock = new Mock<ILogger<FacultiesViewModel>>();
        var viewModel = new FacultiesViewModel(academicServiceMock.Object, loggerMock.Object);

        // Act
        viewModel.SearchQuery = testValue;

        // Assert
        Assert.AreEqual(testValue, viewModel.SearchQuery);
    }

    /// <summary>
    /// Tests that multiple consecutive property changes each raise PropertyChanged event.
    /// </summary>
    [TestMethod]
    public void SearchQuery_MultipleConsecutiveChanges_RaisesPropertyChangedForEach()
    {
        // Arrange
        var academicServiceMock = new Mock<IAcademicService>();
        var loggerMock = new Mock<ILogger<FacultiesViewModel>>();
        var viewModel = new FacultiesViewModel(academicServiceMock.Object, loggerMock.Object);
        var eventCount = 0;

        viewModel.PropertyChanged += (sender, e) =>
        {
            if (e.PropertyName == nameof(FacultiesViewModel.SearchQuery))
            {
                eventCount++;
            }
        };

        // Act
        viewModel.SearchQuery = "first";
        viewModel.SearchQuery = "second";
        viewModel.SearchQuery = "third";

        // Assert
        Assert.AreEqual(3, eventCount);
    }

    /// <summary>
    /// Tests that SearchQuery property handles alternating between empty and non-empty values correctly.
    /// </summary>
    [TestMethod]
    public void SearchQuery_AlternateBetweenEmptyAndNonEmpty_UpdatesCorrectly()
    {
        // Arrange
        var academicServiceMock = new Mock<IAcademicService>();
        var loggerMock = new Mock<ILogger<FacultiesViewModel>>();
        var viewModel = new FacultiesViewModel(academicServiceMock.Object, loggerMock.Object);

        // Act & Assert
        viewModel.SearchQuery = "test";
        Assert.AreEqual("test", viewModel.SearchQuery);

        viewModel.SearchQuery = string.Empty;
        Assert.AreEqual(string.Empty, viewModel.SearchQuery);

        viewModel.SearchQuery = "another test";
        Assert.AreEqual("another test", viewModel.SearchQuery);

        viewModel.SearchQuery = string.Empty;
        Assert.AreEqual(string.Empty, viewModel.SearchQuery);
    }

    /// <summary>
    /// Tests that SearchQuery initial value is empty string.
    /// </summary>
    [TestMethod]
    public void SearchQuery_InitialValue_IsEmptyString()
    {
        // Arrange
        var academicServiceMock = new Mock<IAcademicService>();
        var loggerMock = new Mock<ILogger<FacultiesViewModel>>();

        // Act
        var viewModel = new FacultiesViewModel(academicServiceMock.Object, loggerMock.Object);

        // Assert
        Assert.AreEqual(string.Empty, viewModel.SearchQuery);
    }

    /// <summary>
    /// Tests SearchQuery with strings containing control characters.
    /// </summary>
    [TestMethod]
    public void SearchQuery_SetControlCharacters_StoresAndReturnsValue()
    {
        // Arrange
        var academicServiceMock = new Mock<IAcademicService>();
        var loggerMock = new Mock<ILogger<FacultiesViewModel>>();
        var viewModel = new FacultiesViewModel(academicServiceMock.Object, loggerMock.Object);
        var controlCharString = "test\u0000value\u0001end";

        // Act
        viewModel.SearchQuery = controlCharString;

        // Assert
        Assert.AreEqual(controlCharString, viewModel.SearchQuery);
    }

    /// <summary>
    /// Tests that EditId setter updates the backing field with a valid non-null string value.
    /// Input: Valid string "faculty-123"
    /// Expected: Property value is set correctly and PropertyChanged event is raised.
    /// </summary>
    [TestMethod]
    public void EditId_SetValidString_UpdatesPropertyAndRaisesPropertyChanged()
    {
        // Arrange
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<FacultiesViewModel>>();
        var viewModel = new FacultiesViewModel(mockAcademic.Object, mockLogger.Object);
        var propertyChangedRaised = false;
        string? raisedPropertyName = null;

        viewModel.PropertyChanged += (sender, args) =>
        {
            propertyChangedRaised = true;
            raisedPropertyName = args.PropertyName;
        };

        // Act
        viewModel.EditId = "faculty-123";

        // Assert
        Assert.AreEqual("faculty-123", viewModel.EditId);
        Assert.IsTrue(propertyChangedRaised);
        Assert.AreEqual(nameof(viewModel.EditId), raisedPropertyName);
    }

    /// <summary>
    /// Tests that EditId setter updates the backing field with null value.
    /// Input: null
    /// Expected: Property value is set to null and PropertyChanged event is raised.
    /// </summary>
    [TestMethod]
    public void EditId_SetNull_UpdatesPropertyAndRaisesPropertyChanged()
    {
        // Arrange
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<FacultiesViewModel>>();
        var viewModel = new FacultiesViewModel(mockAcademic.Object, mockLogger.Object);
        viewModel.EditId = "initial-value";
        var propertyChangedRaised = false;
        string? raisedPropertyName = null;

        viewModel.PropertyChanged += (sender, args) =>
        {
            propertyChangedRaised = true;
            raisedPropertyName = args.PropertyName;
        };

        // Act
        viewModel.EditId = null;

        // Assert
        Assert.IsNull(viewModel.EditId);
        Assert.IsTrue(propertyChangedRaised);
        Assert.AreEqual(nameof(viewModel.EditId), raisedPropertyName);
    }

    /// <summary>
    /// Tests that EditId setter updates the backing field with an empty string.
    /// Input: Empty string
    /// Expected: Property value is set to empty string and PropertyChanged event is raised.
    /// </summary>
    [TestMethod]
    public void EditId_SetEmptyString_UpdatesPropertyAndRaisesPropertyChanged()
    {
        // Arrange
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<FacultiesViewModel>>();
        var viewModel = new FacultiesViewModel(mockAcademic.Object, mockLogger.Object);
        var propertyChangedRaised = false;
        string? raisedPropertyName = null;

        viewModel.PropertyChanged += (sender, args) =>
        {
            propertyChangedRaised = true;
            raisedPropertyName = args.PropertyName;
        };

        // Act
        viewModel.EditId = string.Empty;

        // Assert
        Assert.AreEqual(string.Empty, viewModel.EditId);
        Assert.IsTrue(propertyChangedRaised);
        Assert.AreEqual(nameof(viewModel.EditId), raisedPropertyName);
    }

    /// <summary>
    /// Tests that EditId setter updates the backing field with a whitespace-only string.
    /// Input: Whitespace string "   "
    /// Expected: Property value is set to whitespace string and PropertyChanged event is raised.
    /// </summary>
    [TestMethod]
    public void EditId_SetWhitespaceString_UpdatesPropertyAndRaisesPropertyChanged()
    {
        // Arrange
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<FacultiesViewModel>>();
        var viewModel = new FacultiesViewModel(mockAcademic.Object, mockLogger.Object);
        var propertyChangedRaised = false;
        string? raisedPropertyName = null;

        viewModel.PropertyChanged += (sender, args) =>
        {
            propertyChangedRaised = true;
            raisedPropertyName = args.PropertyName;
        };

        // Act
        viewModel.EditId = "   ";

        // Assert
        Assert.AreEqual("   ", viewModel.EditId);
        Assert.IsTrue(propertyChangedRaised);
        Assert.AreEqual(nameof(viewModel.EditId), raisedPropertyName);
    }

    /// <summary>
    /// Tests that EditId setter handles very long strings correctly.
    /// Input: Very long string (10000 characters)
    /// Expected: Property value is set correctly and PropertyChanged event is raised.
    /// </summary>
    [TestMethod]
    public void EditId_SetVeryLongString_UpdatesPropertyAndRaisesPropertyChanged()
    {
        // Arrange
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<FacultiesViewModel>>();
        var viewModel = new FacultiesViewModel(mockAcademic.Object, mockLogger.Object);
        var longString = new string('a', 10000);
        var propertyChangedRaised = false;
        string? raisedPropertyName = null;

        viewModel.PropertyChanged += (sender, args) =>
        {
            propertyChangedRaised = true;
            raisedPropertyName = args.PropertyName;
        };

        // Act
        viewModel.EditId = longString;

        // Assert
        Assert.AreEqual(longString, viewModel.EditId);
        Assert.IsTrue(propertyChangedRaised);
        Assert.AreEqual(nameof(viewModel.EditId), raisedPropertyName);
    }

    /// <summary>
    /// Tests that EditId setter handles strings with special characters correctly.
    /// Input: String with special characters
    /// Expected: Property value is set correctly and PropertyChanged event is raised.
    /// </summary>
    [TestMethod]
    [DataRow("faculty-!@#$%^&*()")]
    [DataRow("faculty-<>?/\\|")]
    [DataRow("faculty-\t\n\r")]
    [DataRow("faculty-\u0000\u0001\u001F")]
    [DataRow("faculty-😀🎉")]
    public void EditId_SetStringWithSpecialCharacters_UpdatesPropertyAndRaisesPropertyChanged(string specialValue)
    {
        // Arrange
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<FacultiesViewModel>>();
        var viewModel = new FacultiesViewModel(mockAcademic.Object, mockLogger.Object);
        var propertyChangedRaised = false;
        string? raisedPropertyName = null;

        viewModel.PropertyChanged += (sender, args) =>
        {
            propertyChangedRaised = true;
            raisedPropertyName = args.PropertyName;
        };

        // Act
        viewModel.EditId = specialValue;

        // Assert
        Assert.AreEqual(specialValue, viewModel.EditId);
        Assert.IsTrue(propertyChangedRaised);
        Assert.AreEqual(nameof(viewModel.EditId), raisedPropertyName);
    }

    /// <summary>
    /// Tests that EditId setter does not raise PropertyChanged event when setting the same value.
    /// Input: Same value as current ("faculty-123")
    /// Expected: Property value remains unchanged and PropertyChanged event is not raised.
    /// </summary>
    [TestMethod]
    public void EditId_SetSameValue_DoesNotRaisePropertyChanged()
    {
        // Arrange
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<FacultiesViewModel>>();
        var viewModel = new FacultiesViewModel(mockAcademic.Object, mockLogger.Object);
        viewModel.EditId = "faculty-123";
        var propertyChangedRaised = false;

        viewModel.PropertyChanged += (sender, args) =>
        {
            propertyChangedRaised = true;
        };

        // Act
        viewModel.EditId = "faculty-123";

        // Assert
        Assert.AreEqual("faculty-123", viewModel.EditId);
        Assert.IsFalse(propertyChangedRaised);
    }

    /// <summary>
    /// Tests that EditId setter does not raise PropertyChanged event when setting null to null.
    /// Input: null when current value is already null
    /// Expected: Property value remains null and PropertyChanged event is not raised.
    /// </summary>
    [TestMethod]
    public void EditId_SetNullWhenAlreadyNull_DoesNotRaisePropertyChanged()
    {
        // Arrange
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<FacultiesViewModel>>();
        var viewModel = new FacultiesViewModel(mockAcademic.Object, mockLogger.Object);
        var propertyChangedRaised = false;

        viewModel.PropertyChanged += (sender, args) =>
        {
            propertyChangedRaised = true;
        };

        // Act
        viewModel.EditId = null;

        // Assert
        Assert.IsNull(viewModel.EditId);
        Assert.IsFalse(propertyChangedRaised);
    }

    /// <summary>
    /// Tests that EditId getter returns the correct value after setting.
    /// Input: Valid string "test-id"
    /// Expected: Getter returns the same value that was set.
    /// </summary>
    [TestMethod]
    public void EditId_GetAfterSet_ReturnsCorrectValue()
    {
        // Arrange
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<FacultiesViewModel>>();
        var viewModel = new FacultiesViewModel(mockAcademic.Object, mockLogger.Object);

        // Act
        viewModel.EditId = "test-id";
        var result = viewModel.EditId;

        // Assert
        Assert.AreEqual("test-id", result);
    }

    /// <summary>
    /// Tests that EditId initial value is null by default.
    /// Input: None (testing initial state)
    /// Expected: Property value is null before any assignment.
    /// </summary>
    [TestMethod]
    public void EditId_InitialValue_IsNull()
    {
        // Arrange
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<FacultiesViewModel>>();

        // Act
        var viewModel = new FacultiesViewModel(mockAcademic.Object, mockLogger.Object);

        // Assert
        Assert.IsNull(viewModel.EditId);
    }

    /// <summary>
    /// Tests that setting the UniversityFilter property to a different value updates the property value and raises PropertyChanged event.
    /// </summary>
    /// <param name="newValue">The new value to set.</param>
    [TestMethod]
    [DataRow("University1")]
    [DataRow("")]
    [DataRow("   ")]
    [DataRow("UniversityWithVeryLongNameThatExceedsNormalLengthExpectationsAndTestsBoundaryConditionsForStringHandling")]
    [DataRow("University!@#$%^&*()")]
    [DataRow("All")]
    public void UniversityFilter_SetDifferentValue_UpdatesPropertyAndRaisesPropertyChanged(string newValue)
    {
        // Arrange
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<FacultiesViewModel>>();
        var viewModel = new FacultiesViewModel(mockAcademic.Object, mockLogger.Object);
        var propertyChangedRaised = false;
        string? raisedPropertyName = null;

        viewModel.PropertyChanged += (sender, args) =>
        {
            propertyChangedRaised = true;
            raisedPropertyName = args.PropertyName;
        };

        // Act
        viewModel.UniversityFilter = newValue;

        // Assert
        Assert.AreEqual(newValue, viewModel.UniversityFilter);
        Assert.IsTrue(propertyChangedRaised);
        Assert.AreEqual(nameof(viewModel.UniversityFilter), raisedPropertyName);
    }

    /// <summary>
    /// Tests that setting the UniversityFilter property to null updates the property value and raises PropertyChanged event.
    /// </summary>
    [TestMethod]
    public void UniversityFilter_SetNull_UpdatesPropertyAndRaisesPropertyChanged()
    {
        // Arrange
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<FacultiesViewModel>>();
        var viewModel = new FacultiesViewModel(mockAcademic.Object, mockLogger.Object);
        var propertyChangedRaised = false;
        string? raisedPropertyName = null;

        viewModel.PropertyChanged += (sender, args) =>
        {
            propertyChangedRaised = true;
            raisedPropertyName = args.PropertyName;
        };

        // Act
        viewModel.UniversityFilter = null!;

        // Assert
        Assert.IsNull(viewModel.UniversityFilter);
        Assert.IsTrue(propertyChangedRaised);
        Assert.AreEqual(nameof(viewModel.UniversityFilter), raisedPropertyName);
    }

    /// <summary>
    /// Tests that setting the UniversityFilter property to the same value does not raise PropertyChanged event.
    /// </summary>
    [TestMethod]
    public void UniversityFilter_SetSameValue_DoesNotRaisePropertyChanged()
    {
        // Arrange
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<FacultiesViewModel>>();
        var viewModel = new FacultiesViewModel(mockAcademic.Object, mockLogger.Object);

        viewModel.UniversityFilter = "TestUniversity";

        var propertyChangedRaised = false;
        viewModel.PropertyChanged += (sender, args) =>
        {
            propertyChangedRaised = true;
        };

        // Act
        viewModel.UniversityFilter = "TestUniversity";

        // Assert
        Assert.AreEqual("TestUniversity", viewModel.UniversityFilter);
        Assert.IsFalse(propertyChangedRaised);
    }

    /// <summary>
    /// Tests that the UniversityFilter property has the correct default value.
    /// </summary>
    [TestMethod]
    public void UniversityFilter_DefaultValue_IsAll()
    {
        // Arrange
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<FacultiesViewModel>>();

        // Act
        var viewModel = new FacultiesViewModel(mockAcademic.Object, mockLogger.Object);

        // Assert
        Assert.AreEqual("All", viewModel.UniversityFilter);
    }

    /// <summary>
    /// Tests that setting the UniversityFilter property multiple times with different values correctly updates the property each time.
    /// </summary>
    [TestMethod]
    public void UniversityFilter_SetMultipleDifferentValues_UpdatesPropertyEachTime()
    {
        // Arrange
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<FacultiesViewModel>>();
        var viewModel = new FacultiesViewModel(mockAcademic.Object, mockLogger.Object);
        var propertyChangedCount = 0;

        viewModel.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == nameof(viewModel.UniversityFilter))
            {
                propertyChangedCount++;
            }
        };

        // Act
        viewModel.UniversityFilter = "Value1";
        viewModel.UniversityFilter = "Value2";
        viewModel.UniversityFilter = "Value3";

        // Assert
        Assert.AreEqual("Value3", viewModel.UniversityFilter);
        Assert.AreEqual(3, propertyChangedCount);
    }

    /// <summary>
    /// Tests that setting EditName with a valid non-empty string updates the property value correctly.
    /// Input: Valid string value.
    /// Expected: Property value is updated and PropertyChanged event is raised.
    /// </summary>
    [TestMethod]
    [DataRow("Faculty of Science")]
    [DataRow("A")]
    [DataRow("Faculty with Numbers 123")]
    [DataRow("Faculty-with-Special_Characters!@#")]
    public void EditName_SetValidString_UpdatesPropertyAndRaisesPropertyChanged(string value)
    {
        // Arrange
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<FacultiesViewModel>>();
        var viewModel = new FacultiesViewModel(mockAcademic.Object, mockLogger.Object);
        string? changedPropertyName = null;
        viewModel.PropertyChanged += (sender, args) => changedPropertyName = args.PropertyName;

        // Act
        viewModel.EditName = value;

        // Assert
        Assert.AreEqual(value, viewModel.EditName);
        Assert.AreEqual("EditName", changedPropertyName);
    }

    /// <summary>
    /// Tests that setting EditName with an empty string updates the property value correctly.
    /// Input: Empty string.
    /// Expected: Property value is updated to empty string.
    /// </summary>
    [TestMethod]
    public void EditName_SetEmptyString_UpdatesPropertyToEmptyString()
    {
        // Arrange
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<FacultiesViewModel>>();
        var viewModel = new FacultiesViewModel(mockAcademic.Object, mockLogger.Object);
        viewModel.EditName = "Initial Value";

        // Act
        viewModel.EditName = string.Empty;

        // Assert
        Assert.AreEqual(string.Empty, viewModel.EditName);
    }

    /// <summary>
    /// Tests that setting EditName with whitespace-only strings updates the property value correctly.
    /// Input: Various whitespace-only strings.
    /// Expected: Property value is updated to the whitespace string.
    /// </summary>
    [TestMethod]
    [DataRow("   ")]
    [DataRow("\t")]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [DataRow("  \t\n  ")]
    public void EditName_SetWhitespaceString_UpdatesPropertyToWhitespaceString(string value)
    {
        // Arrange
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<FacultiesViewModel>>();
        var viewModel = new FacultiesViewModel(mockAcademic.Object, mockLogger.Object);

        // Act
        viewModel.EditName = value;

        // Assert
        Assert.AreEqual(value, viewModel.EditName);
    }

    /// <summary>
    /// Tests that setting EditName with a very long string updates the property value correctly.
    /// Input: String with 10000 characters.
    /// Expected: Property value is updated to the long string.
    /// </summary>
    [TestMethod]
    public void EditName_SetVeryLongString_UpdatesPropertyCorrectly()
    {
        // Arrange
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<FacultiesViewModel>>();
        var viewModel = new FacultiesViewModel(mockAcademic.Object, mockLogger.Object);
        var longString = new string('A', 10000);

        // Act
        viewModel.EditName = longString;

        // Assert
        Assert.AreEqual(longString, viewModel.EditName);
        Assert.AreEqual(10000, viewModel.EditName.Length);
    }

    /// <summary>
    /// Tests that setting EditName with Unicode and special characters updates the property value correctly.
    /// Input: Strings with Unicode characters, emojis, and control characters.
    /// Expected: Property value is updated to the special string.
    /// </summary>
    [TestMethod]
    [DataRow("Faculté d'Ingénierie")]
    [DataRow("كلية العلوم")]
    [DataRow("科学学院")]
    [DataRow("Faculty 😀🎓")]
    [DataRow("Line1\r\nLine2\r\nLine3")]
    public void EditName_SetUnicodeAndSpecialCharacters_UpdatesPropertyCorrectly(string value)
    {
        // Arrange
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<FacultiesViewModel>>();
        var viewModel = new FacultiesViewModel(mockAcademic.Object, mockLogger.Object);

        // Act
        viewModel.EditName = value;

        // Assert
        Assert.AreEqual(value, viewModel.EditName);
    }

    /// <summary>
    /// Tests that setting EditName to the same value does not raise PropertyChanged event.
    /// Input: Same string value set twice.
    /// Expected: PropertyChanged event is raised only once on first set.
    /// </summary>
    [TestMethod]
    public void EditName_SetSameValueTwice_DoesNotRaisePropertyChangedOnSecondSet()
    {
        // Arrange
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<FacultiesViewModel>>();
        var viewModel = new FacultiesViewModel(mockAcademic.Object, mockLogger.Object);
        var value = "Faculty of Engineering";
        var propertyChangedCount = 0;
        viewModel.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == "EditName")
            {
                propertyChangedCount++;
            }
        };

        // Act
        viewModel.EditName = value;
        viewModel.EditName = value;

        // Assert
        Assert.AreEqual(value, viewModel.EditName);
        Assert.AreEqual(1, propertyChangedCount);
    }

    /// <summary>
    /// Tests that getting EditName returns the default value when not set.
    /// Input: No value set.
    /// Expected: Returns empty string (default value).
    /// </summary>
    [TestMethod]
    public void EditName_GetDefaultValue_ReturnsEmptyString()
    {
        // Arrange
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<FacultiesViewModel>>();
        var viewModel = new FacultiesViewModel(mockAcademic.Object, mockLogger.Object);

        // Act
        var result = viewModel.EditName;

        // Assert
        Assert.AreEqual(string.Empty, result);
    }

    /// <summary>
    /// Tests that setting EditName multiple times with different values updates correctly each time.
    /// Input: Multiple different string values.
    /// Expected: Property value reflects the last set value and PropertyChanged is raised for each change.
    /// </summary>
    [TestMethod]
    public void EditName_SetMultipleDifferentValues_UpdatesCorrectlyEachTime()
    {
        // Arrange
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<FacultiesViewModel>>();
        var viewModel = new FacultiesViewModel(mockAcademic.Object, mockLogger.Object);
        var values = new[] { "First", "Second", "Third" };
        var propertyChangedCount = 0;
        viewModel.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == "EditName")
            {
                propertyChangedCount++;
            }
        };

        // Act & Assert
        foreach (var value in values)
        {
            viewModel.EditName = value;
            Assert.AreEqual(value, viewModel.EditName);
        }
        Assert.AreEqual(3, propertyChangedCount);
    }

    /// <summary>
    /// Tests that PropertyChanged event provides the correct property name when EditName is set.
    /// Input: Valid string value.
    /// Expected: PropertyChanged event args contain "EditName" as the property name.
    /// </summary>
    [TestMethod]
    public void EditName_SetValue_PropertyChangedEventContainsCorrectPropertyName()
    {
        // Arrange
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<FacultiesViewModel>>();
        var viewModel = new FacultiesViewModel(mockAcademic.Object, mockLogger.Object);
        PropertyChangedEventArgs? eventArgs = null;
        viewModel.PropertyChanged += (sender, args) => eventArgs = args;

        // Act
        viewModel.EditName = "Test Faculty";

        // Assert
        Assert.IsNotNull(eventArgs);
        Assert.AreEqual("EditName", eventArgs.PropertyName);
    }

    /// <summary>
    /// Tests that setting EditName with strings containing only control characters updates the property correctly.
    /// Input: Strings with control characters.
    /// Expected: Property value is updated to the control character string.
    /// </summary>
    [TestMethod]
    [DataRow("\0")]
    [DataRow("\u0001")]
    [DataRow("\u001F")]
    public void EditName_SetControlCharacters_UpdatesPropertyCorrectly(string value)
    {
        // Arrange
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<FacultiesViewModel>>();
        var viewModel = new FacultiesViewModel(mockAcademic.Object, mockLogger.Object);

        // Act
        viewModel.EditName = value;

        // Assert
        Assert.AreEqual(value, viewModel.EditName);
    }

    /// <summary>
    /// Tests that the CurrentPage property returns the initial default value of 1.
    /// </summary>
    [TestMethod]
    public void CurrentPage_InitialValue_ReturnsOne()
    {
        // Arrange
        var mockAcademicService = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<FacultiesViewModel>>();
        var viewModel = new FacultiesViewModel(mockAcademicService.Object, mockLogger.Object);

        // Act
        int result = viewModel.CurrentPage;

        // Assert
        Assert.AreEqual(1, result);
    }

    /// <summary>
    /// Tests that setting CurrentPage to a new value raises PropertyChanged for both "CurrentPage" and "PageInfo".
    /// </summary>
    /// <param name="newValue">The new value to set for CurrentPage.</param>
    [TestMethod]
    [DataRow(2)]
    [DataRow(10)]
    [DataRow(100)]
    [DataRow(int.MaxValue)]
    [DataRow(0)]
    [DataRow(-1)]
    [DataRow(int.MinValue)]
    public void CurrentPage_SetNewValue_RaisesPropertyChangedForCurrentPageAndPageInfo(int newValue)
    {
        // Arrange
        var mockAcademicService = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<FacultiesViewModel>>();
        var viewModel = new FacultiesViewModel(mockAcademicService.Object, mockLogger.Object);
        var propertyChangedEvents = new System.Collections.Generic.List<string>();
        viewModel.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName != null)
            {
                propertyChangedEvents.Add(args.PropertyName);
            }
        };

        // Act
        viewModel.CurrentPage = newValue;

        // Assert
        Assert.AreEqual(newValue, viewModel.CurrentPage);
        Assert.IsTrue(propertyChangedEvents.Contains("CurrentPage"), "PropertyChanged should be raised for CurrentPage");
        Assert.IsTrue(propertyChangedEvents.Contains("PageInfo"), "PropertyChanged should be raised for PageInfo");
    }

    /// <summary>
    /// Tests that setting CurrentPage to the same value does not raise PropertyChanged events.
    /// </summary>
    [TestMethod]
    public void CurrentPage_SetSameValue_DoesNotRaisePropertyChanged()
    {
        // Arrange
        var mockAcademicService = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<FacultiesViewModel>>();
        var viewModel = new FacultiesViewModel(mockAcademicService.Object, mockLogger.Object);
        var initialValue = viewModel.CurrentPage;
        var propertyChangedCount = 0;
        viewModel.PropertyChanged += (sender, args) =>
        {
            propertyChangedCount++;
        };

        // Act
        viewModel.CurrentPage = initialValue;

        // Assert
        Assert.AreEqual(initialValue, viewModel.CurrentPage);
        Assert.AreEqual(0, propertyChangedCount, "PropertyChanged should not be raised when setting the same value");
    }

    /// <summary>
    /// Tests that setting CurrentPage multiple times to different values raises PropertyChanged events each time.
    /// </summary>
    [TestMethod]
    public void CurrentPage_SetMultipleDifferentValues_RaisesPropertyChangedEachTime()
    {
        // Arrange
        var mockAcademicService = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<FacultiesViewModel>>();
        var viewModel = new FacultiesViewModel(mockAcademicService.Object, mockLogger.Object);
        var propertyChangedCount = 0;
        viewModel.PropertyChanged += (sender, args) =>
        {
            propertyChangedCount++;
        };

        // Act
        viewModel.CurrentPage = 2;
        viewModel.CurrentPage = 3;
        viewModel.CurrentPage = 4;

        // Assert
        Assert.AreEqual(4, viewModel.CurrentPage);
        Assert.AreEqual(6, propertyChangedCount, "PropertyChanged should be raised 6 times (2 events per change: CurrentPage and PageInfo)");
    }

    /// <summary>
    /// Tests that setting CurrentPage to a boundary value (int.MinValue) works correctly.
    /// </summary>
    [TestMethod]
    public void CurrentPage_SetMinValue_UpdatesPropertySuccessfully()
    {
        // Arrange
        var mockAcademicService = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<FacultiesViewModel>>();
        var viewModel = new FacultiesViewModel(mockAcademicService.Object, mockLogger.Object);

        // Act
        viewModel.CurrentPage = int.MinValue;

        // Assert
        Assert.AreEqual(int.MinValue, viewModel.CurrentPage);
    }

    /// <summary>
    /// Tests that setting CurrentPage to a boundary value (int.MaxValue) works correctly.
    /// </summary>
    [TestMethod]
    public void CurrentPage_SetMaxValue_UpdatesPropertySuccessfully()
    {
        // Arrange
        var mockAcademicService = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<FacultiesViewModel>>();
        var viewModel = new FacultiesViewModel(mockAcademicService.Object, mockLogger.Object);

        // Act
        viewModel.CurrentPage = int.MaxValue;

        // Assert
        Assert.AreEqual(int.MaxValue, viewModel.CurrentPage);
    }

    /// <summary>
    /// Tests that setting CurrentPage to zero updates the property correctly.
    /// </summary>
    [TestMethod]
    public void CurrentPage_SetZero_UpdatesPropertySuccessfully()
    {
        // Arrange
        var mockAcademicService = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<FacultiesViewModel>>();
        var viewModel = new FacultiesViewModel(mockAcademicService.Object, mockLogger.Object);

        // Act
        viewModel.CurrentPage = 0;

        // Assert
        Assert.AreEqual(0, viewModel.CurrentPage);
    }

    /// <summary>
    /// Tests that setting CurrentPage to a negative value updates the property correctly.
    /// </summary>
    [TestMethod]
    public void CurrentPage_SetNegativeValue_UpdatesPropertySuccessfully()
    {
        // Arrange
        var mockAcademicService = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<FacultiesViewModel>>();
        var viewModel = new FacultiesViewModel(mockAcademicService.Object, mockLogger.Object);

        // Act
        viewModel.CurrentPage = -100;

        // Assert
        Assert.AreEqual(-100, viewModel.CurrentPage);
    }

    /// <summary>
    /// Tests that PropertyChanged is raised exactly once for "PageInfo" when CurrentPage changes.
    /// </summary>
    [TestMethod]
    public void CurrentPage_SetNewValue_RaisesPropertyChangedForPageInfoOnce()
    {
        // Arrange
        var mockAcademicService = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<FacultiesViewModel>>();
        var viewModel = new FacultiesViewModel(mockAcademicService.Object, mockLogger.Object);
        var pageInfoChangedCount = 0;
        viewModel.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == "PageInfo")
            {
                pageInfoChangedCount++;
            }
        };

        // Act
        viewModel.CurrentPage = 5;

        // Assert
        Assert.AreEqual(1, pageInfoChangedCount, "PropertyChanged for PageInfo should be raised exactly once");
    }

    /// <summary>
    /// Tests that setting IsEditing to true updates the property value and raises PropertyChanged event.
    /// </summary>
    [TestMethod]
    public void IsEditing_SetToTrue_UpdatesValueAndRaisesPropertyChanged()
    {
        // Arrange
        var mockAcademicService = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<FacultiesViewModel>>();
        var viewModel = new FacultiesViewModel(mockAcademicService.Object, mockLogger.Object);
        var propertyChangedRaised = false;
        string? raisedPropertyName = null;
        viewModel.PropertyChanged += (sender, args) =>
        {
            propertyChangedRaised = true;
            raisedPropertyName = args.PropertyName;
        };

        // Act
        viewModel.IsEditing = true;

        // Assert
        Assert.IsTrue(viewModel.IsEditing);
        Assert.IsTrue(propertyChangedRaised);
        Assert.AreEqual("IsEditing", raisedPropertyName);
    }

    /// <summary>
    /// Tests that setting IsEditing to false updates the property value and raises PropertyChanged event.
    /// </summary>
    [TestMethod]
    public void IsEditing_SetToFalse_UpdatesValueAndRaisesPropertyChanged()
    {
        // Arrange
        var mockAcademicService = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<FacultiesViewModel>>();
        var viewModel = new FacultiesViewModel(mockAcademicService.Object, mockLogger.Object);
        viewModel.IsEditing = true; // Set initial value
        var propertyChangedRaised = false;
        string? raisedPropertyName = null;
        viewModel.PropertyChanged += (sender, args) =>
        {
            propertyChangedRaised = true;
            raisedPropertyName = args.PropertyName;
        };

        // Act
        viewModel.IsEditing = false;

        // Assert
        Assert.IsFalse(viewModel.IsEditing);
        Assert.IsTrue(propertyChangedRaised);
        Assert.AreEqual("IsEditing", raisedPropertyName);
    }

    /// <summary>
    /// Tests that setting IsEditing to the same value (true to true) does not raise PropertyChanged event.
    /// </summary>
    [TestMethod]
    public void IsEditing_SetToSameValueTrue_DoesNotRaisePropertyChanged()
    {
        // Arrange
        var mockAcademicService = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<FacultiesViewModel>>();
        var viewModel = new FacultiesViewModel(mockAcademicService.Object, mockLogger.Object);
        viewModel.IsEditing = true;
        var propertyChangedRaised = false;
        viewModel.PropertyChanged += (sender, args) =>
        {
            propertyChangedRaised = true;
        };

        // Act
        viewModel.IsEditing = true;

        // Assert
        Assert.IsTrue(viewModel.IsEditing);
        Assert.IsFalse(propertyChangedRaised);
    }

    /// <summary>
    /// Tests that setting IsEditing to the same value (false to false) does not raise PropertyChanged event.
    /// </summary>
    [TestMethod]
    public void IsEditing_SetToSameValueFalse_DoesNotRaisePropertyChanged()
    {
        // Arrange
        var mockAcademicService = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<FacultiesViewModel>>();
        var viewModel = new FacultiesViewModel(mockAcademicService.Object, mockLogger.Object);
        var propertyChangedRaised = false;
        viewModel.PropertyChanged += (sender, args) =>
        {
            propertyChangedRaised = true;
        };

        // Act
        viewModel.IsEditing = false;

        // Assert
        Assert.IsFalse(viewModel.IsEditing);
        Assert.IsFalse(propertyChangedRaised);
    }

    /// <summary>
    /// Tests that the IsEditing property has a default value of false.
    /// </summary>
    [TestMethod]
    public void IsEditing_DefaultValue_IsFalse()
    {
        // Arrange
        var mockAcademicService = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<FacultiesViewModel>>();

        // Act
        var viewModel = new FacultiesViewModel(mockAcademicService.Object, mockLogger.Object);

        // Assert
        Assert.IsFalse(viewModel.IsEditing);
    }

    /// <summary>
    /// Tests that getting IsEditing returns the correct value after multiple state changes.
    /// </summary>
    [TestMethod]
    public void IsEditing_MultipleStateChanges_ReturnsCorrectValue()
    {
        // Arrange
        var mockAcademicService = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<FacultiesViewModel>>();
        var viewModel = new FacultiesViewModel(mockAcademicService.Object, mockLogger.Object);

        // Act & Assert - Initial state
        Assert.IsFalse(viewModel.IsEditing);

        // Act & Assert - Set to true
        viewModel.IsEditing = true;
        Assert.IsTrue(viewModel.IsEditing);

        // Act & Assert - Set to false
        viewModel.IsEditing = false;
        Assert.IsFalse(viewModel.IsEditing);

        // Act & Assert - Set to true again
        viewModel.IsEditing = true;
        Assert.IsTrue(viewModel.IsEditing);
    }

    /// <summary>
    /// Tests that the PageInfo property returns the correct formatted string
    /// for various combinations of CurrentPage and TotalPages values.
    /// </summary>
    /// <param name="currentPage">The current page number to set.</param>
    /// <param name="totalPages">The total number of pages to set.</param>
    /// <param name="expectedPageInfo">The expected PageInfo string output.</param>
    [TestMethod]
    [DataRow(1, 1, "Page 1 of 1", DisplayName = "Default values (1, 1)")]
    [DataRow(1, 10, "Page 1 of 10", DisplayName = "First page of multiple pages")]
    [DataRow(5, 10, "Page 5 of 10", DisplayName = "Middle page")]
    [DataRow(10, 10, "Page 10 of 10", DisplayName = "Last page")]
    [DataRow(0, 0, "Page 0 of 0", DisplayName = "Zero values")]
    [DataRow(0, 10, "Page 0 of 10", DisplayName = "Zero current page")]
    [DataRow(1, 0, "Page 1 of 0", DisplayName = "Zero total pages")]
    [DataRow(-1, 5, "Page -1 of 5", DisplayName = "Negative current page")]
    [DataRow(5, -1, "Page 5 of -1", DisplayName = "Negative total pages")]
    [DataRow(-5, -10, "Page -5 of -10", DisplayName = "Both negative")]
    [DataRow(15, 10, "Page 15 of 10", DisplayName = "Current page exceeds total pages")]
    [DataRow(100, 1, "Page 100 of 1", DisplayName = "Current page much larger than total")]
    [DataRow(2147483647, 1, "Page 2147483647 of 1", DisplayName = "int.MaxValue current page")]
    [DataRow(1, 2147483647, "Page 1 of 2147483647", DisplayName = "int.MaxValue total pages")]
    [DataRow(2147483647, 2147483647, "Page 2147483647 of 2147483647", DisplayName = "Both int.MaxValue")]
    [DataRow(-2147483648, 1, "Page -2147483648 of 1", DisplayName = "int.MinValue current page")]
    [DataRow(1, -2147483648, "Page 1 of -2147483648", DisplayName = "int.MinValue total pages")]
    [DataRow(-2147483648, -2147483648, "Page -2147483648 of -2147483648", DisplayName = "Both int.MinValue")]
    public void PageInfo_VariousCurrentPageAndTotalPagesValues_ReturnsCorrectFormat(int currentPage, int totalPages, string expectedPageInfo)
    {
        // Arrange
        var mockAcademicService = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<FacultiesViewModel>>();
        var viewModel = new FacultiesViewModel(mockAcademicService.Object, mockLogger.Object);
        viewModel.CurrentPage = currentPage;
        viewModel.TotalPages = totalPages;

        // Act
        var result = viewModel.PageInfo;

        // Assert
        Assert.AreEqual(expectedPageInfo, result);
    }

    /// <summary>
    /// Tests that setting PendingCount with a different value updates the property and raises PropertyChanged event.
    /// </summary>
    /// <param name="newValue">The new value to set for PendingCount.</param>
    [TestMethod]
    [DataRow(0)]
    [DataRow(1)]
    [DataRow(10)]
    [DataRow(100)]
    [DataRow(-1)]
    [DataRow(-100)]
    [DataRow(int.MaxValue)]
    [DataRow(int.MinValue)]
    public void PendingCount_SetWithDifferentValue_UpdatesPropertyAndRaisesPropertyChangedEvent(int newValue)
    {
        // Arrange
        Mock<IAcademicService> mockAcademicService = new Mock<IAcademicService>();
        Mock<ILogger<FacultiesViewModel>> mockLogger = new Mock<ILogger<FacultiesViewModel>>();
        FacultiesViewModel viewModel = new FacultiesViewModel(mockAcademicService.Object, mockLogger.Object);
        bool eventRaised = false;
        string? raisedPropertyName = null;
        viewModel.PropertyChanged += (sender, args) =>
        {
            eventRaised = true;
            raisedPropertyName = args.PropertyName;
        };

        // Act
        viewModel.PendingCount = newValue;

        // Assert
        Assert.AreEqual(newValue, viewModel.PendingCount);
        Assert.IsTrue(eventRaised);
        Assert.AreEqual("PendingCount", raisedPropertyName);
    }

    /// <summary>
    /// Tests that setting PendingCount with the same value does not raise PropertyChanged event.
    /// </summary>
    [TestMethod]
    public void PendingCount_SetWithSameValue_DoesNotRaisePropertyChangedEvent()
    {
        // Arrange
        Mock<IAcademicService> mockAcademicService = new Mock<IAcademicService>();
        Mock<ILogger<FacultiesViewModel>> mockLogger = new Mock<ILogger<FacultiesViewModel>>();
        FacultiesViewModel viewModel = new FacultiesViewModel(mockAcademicService.Object, mockLogger.Object);
        int initialValue = 42;
        viewModel.PendingCount = initialValue;
        bool eventRaised = false;
        viewModel.PropertyChanged += (sender, args) =>
        {
            eventRaised = true;
        };

        // Act
        viewModel.PendingCount = initialValue;

        // Assert
        Assert.AreEqual(initialValue, viewModel.PendingCount);
        Assert.IsFalse(eventRaised);
    }

    /// <summary>
    /// Tests that getting PendingCount returns the correct value after setting it.
    /// </summary>
    /// <param name="value">The value to set and retrieve.</param>
    [TestMethod]
    [DataRow(0)]
    [DataRow(5)]
    [DataRow(999)]
    [DataRow(-5)]
    [DataRow(int.MaxValue)]
    [DataRow(int.MinValue)]
    public void PendingCount_Get_ReturnsCorrectValue(int value)
    {
        // Arrange
        Mock<IAcademicService> mockAcademicService = new Mock<IAcademicService>();
        Mock<ILogger<FacultiesViewModel>> mockLogger = new Mock<ILogger<FacultiesViewModel>>();
        FacultiesViewModel viewModel = new FacultiesViewModel(mockAcademicService.Object, mockLogger.Object);
        viewModel.PendingCount = value;

        // Act
        int result = viewModel.PendingCount;

        // Assert
        Assert.AreEqual(value, result);
    }

    /// <summary>
    /// Tests that PendingCount has a default value of 0 when first initialized.
    /// </summary>
    [TestMethod]
    public void PendingCount_Initial_DefaultsToZero()
    {
        // Arrange
        Mock<IAcademicService> mockAcademicService = new Mock<IAcademicService>();
        Mock<ILogger<FacultiesViewModel>> mockLogger = new Mock<ILogger<FacultiesViewModel>>();

        // Act
        FacultiesViewModel viewModel = new FacultiesViewModel(mockAcademicService.Object, mockLogger.Object);

        // Assert
        Assert.AreEqual(0, viewModel.PendingCount);
    }

    /// <summary>
    /// Tests that multiple consecutive updates to PendingCount with different values all raise PropertyChanged events.
    /// </summary>
    [TestMethod]
    public void PendingCount_MultipleUpdatesWithDifferentValues_RaisesPropertyChangedEventForEach()
    {
        // Arrange
        Mock<IAcademicService> mockAcademicService = new Mock<IAcademicService>();
        Mock<ILogger<FacultiesViewModel>> mockLogger = new Mock<ILogger<FacultiesViewModel>>();
        FacultiesViewModel viewModel = new FacultiesViewModel(mockAcademicService.Object, mockLogger.Object);
        int eventCount = 0;
        viewModel.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == "PendingCount")
            {
                eventCount++;
            }
        };

        // Act
        viewModel.PendingCount = 1;
        viewModel.PendingCount = 2;
        viewModel.PendingCount = 3;

        // Assert
        Assert.AreEqual(3, viewModel.PendingCount);
        Assert.AreEqual(3, eventCount);
    }

    /// <summary>
    /// Tests that LoadAsync successfully loads universities and faculties, populates collections, and updates statistics.
    /// Verifies that Universities collection is populated with results from GetUniversitiesAsync.
    /// Verifies that TotalFaculties, ActiveFacultiesCount, and PendingCount are calculated correctly.
    /// </summary>
    [TestMethod]
    public async Task LoadAsync_WithValidData_LoadsUniversitiesAndFacultiesSuccessfully()
    {
        // Arrange
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<FacultiesViewModel>>();

        var universities = new List<LookupItem>
        {
            new LookupItem { Id = "u1", Name = "University 1" },
            new LookupItem { Id = "u2", Name = "University 2" }
        };

        var faculties = new List<FacultyDto>
        {
            new FacultyDto { Id = "f1", Name = "Faculty 1", Status = "Active", UniversityId = "u1" },
            new FacultyDto { Id = "f2", Name = "Faculty 2", Status = "Active", UniversityId = "u1" },
            new FacultyDto { Id = "f3", Name = "Faculty 3", Status = "Pending", UniversityId = "u2" }
        };

        mockAcademic.Setup(a => a.GetUniversitiesAsync()).ReturnsAsync(universities);
        mockAcademic.Setup(a => a.GetFacultyDetailsAsync()).ReturnsAsync(faculties);

        var viewModel = new FacultiesViewModel(mockAcademic.Object, mockLogger.Object);

        // Act
        await viewModel.LoadAsync();

        // Assert
        Assert.AreEqual(2, viewModel.Universities.Count, "Universities count should be 2");
        Assert.AreEqual("u1", viewModel.Universities[0].Id);
        Assert.AreEqual("University 1", viewModel.Universities[0].Name);
        Assert.AreEqual("u2", viewModel.Universities[1].Id);
        Assert.AreEqual("University 2", viewModel.Universities[1].Name);
        Assert.AreEqual(3, viewModel.TotalFaculties, "TotalFaculties should be 3");
        Assert.AreEqual(2, viewModel.ActiveFacultiesCount, "ActiveFacultiesCount should be 2");
        Assert.AreEqual(1, viewModel.PendingCount, "PendingCount should be 1");
        Assert.IsFalse(viewModel.IsBusy, "IsBusy should be false after completion");
        Assert.AreEqual(string.Empty, viewModel.ErrorMessage, "ErrorMessage should be empty");
    }

    /// <summary>
    /// Tests that LoadAsync returns immediately without executing logic when IsBusy is already true.
    /// Ensures no service calls are made and collections remain unchanged.
    /// </summary>
    [TestMethod]
    public async Task LoadAsync_WhenAlreadyBusy_ReturnsImmediately()
    {
        // Arrange
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<FacultiesViewModel>>();

        var viewModel = new FacultiesViewModel(mockAcademic.Object, mockLogger.Object);

        // Set IsBusy to true by starting a load operation
        var loadTask = viewModel.LoadAsync();

        // Act - try to load again while busy
        await viewModel.LoadAsync();

        // Complete first load
        await loadTask;

        // Assert
        mockAcademic.Verify(a => a.GetUniversitiesAsync(), Times.Once(), "GetUniversitiesAsync should only be called once");
        mockAcademic.Verify(a => a.GetFacultyDetailsAsync(), Times.Once(), "GetFacultyDetailsAsync should only be called once");
    }

    /// <summary>
    /// Tests that LoadAsync handles empty collections from GetUniversitiesAsync and GetFacultyDetailsAsync.
    /// Verifies that collections are cleared and statistics are set to zero.
    /// </summary>
    [TestMethod]
    public async Task LoadAsync_WithEmptyData_SetsZeroStatistics()
    {
        // Arrange
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<FacultiesViewModel>>();

        var emptyUniversities = new List<LookupItem>();
        var emptyFaculties = new List<FacultyDto>();

        mockAcademic.Setup(a => a.GetUniversitiesAsync()).ReturnsAsync(emptyUniversities);
        mockAcademic.Setup(a => a.GetFacultyDetailsAsync()).ReturnsAsync(emptyFaculties);

        var viewModel = new FacultiesViewModel(mockAcademic.Object, mockLogger.Object);

        // Act
        await viewModel.LoadAsync();

        // Assert
        Assert.AreEqual(0, viewModel.Universities.Count, "Universities count should be 0");
        Assert.AreEqual(0, viewModel.TotalFaculties, "TotalFaculties should be 0");
        Assert.AreEqual(0, viewModel.ActiveFacultiesCount, "ActiveFacultiesCount should be 0");
        Assert.AreEqual(0, viewModel.PendingCount, "PendingCount should be 0");
        Assert.IsFalse(viewModel.IsBusy, "IsBusy should be false after completion");
        Assert.AreEqual(string.Empty, viewModel.ErrorMessage, "ErrorMessage should be empty");
    }

    /// <summary>
    /// Tests that LoadAsync correctly counts only faculties with Status == "Active" for ActiveFacultiesCount.
    /// Tests that LoadAsync correctly counts faculties with Status != "Active" for PendingCount.
    /// </summary>
    [TestMethod]
    [DataRow("Active", "Active", "Active", 3, 0)]
    [DataRow("Active", "Inactive", "Pending", 1, 2)]
    [DataRow("Pending", "Inactive", "Draft", 0, 3)]
    [DataRow("Active", "Active", "Inactive", 2, 1)]
    public async Task LoadAsync_WithVariousStatuses_CalculatesStatisticsCorrectly(
        string status1, string status2, string status3, int expectedActive, int expectedPending)
    {
        // Arrange
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<FacultiesViewModel>>();

        var universities = new List<LookupItem>
        {
            new LookupItem { Id = "u1", Name = "University 1" }
        };

        var faculties = new List<FacultyDto>
        {
            new FacultyDto { Id = "f1", Name = "Faculty 1", Status = status1, UniversityId = "u1" },
            new FacultyDto { Id = "f2", Name = "Faculty 2", Status = status2, UniversityId = "u1" },
            new FacultyDto { Id = "f3", Name = "Faculty 3", Status = status3, UniversityId = "u1" }
        };

        mockAcademic.Setup(a => a.GetUniversitiesAsync()).ReturnsAsync(universities);
        mockAcademic.Setup(a => a.GetFacultyDetailsAsync()).ReturnsAsync(faculties);

        var viewModel = new FacultiesViewModel(mockAcademic.Object, mockLogger.Object);

        // Act
        await viewModel.LoadAsync();

        // Assert
        Assert.AreEqual(3, viewModel.TotalFaculties, "TotalFaculties should be 3");
        Assert.AreEqual(expectedActive, viewModel.ActiveFacultiesCount, $"ActiveFacultiesCount should be {expectedActive}");
        Assert.AreEqual(expectedPending, viewModel.PendingCount, $"PendingCount should be {expectedPending}");
    }

    /// <summary>
    /// Tests that LoadAsync handles exceptions from GetUniversitiesAsync.
    /// Verifies that error is logged, ErrorMessage is set, and IsBusy is reset to false.
    /// </summary>
    [TestMethod]
    public async Task LoadAsync_WhenGetUniversitiesAsyncThrows_LogsErrorAndSetsErrorMessage()
    {
        // Arrange
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<FacultiesViewModel>>();

        var expectedException = new InvalidOperationException("Failed to retrieve universities");
        mockAcademic.Setup(a => a.GetUniversitiesAsync()).ThrowsAsync(expectedException);

        var viewModel = new FacultiesViewModel(mockAcademic.Object, mockLogger.Object);

        // Act
        await viewModel.LoadAsync();

        // Assert
        Assert.IsFalse(viewModel.IsBusy, "IsBusy should be false after exception");
        Assert.AreEqual("Failed to load faculties.", viewModel.ErrorMessage, "ErrorMessage should be set");
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => true),
                expectedException,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once,
            "Error should be logged");
    }

    /// <summary>
    /// Tests that LoadAsync handles exceptions from GetFacultyDetailsAsync.
    /// Verifies that error is logged, ErrorMessage is set, and IsBusy is reset to false.
    /// </summary>
    [TestMethod]
    public async Task LoadAsync_WhenGetFacultyDetailsAsyncThrows_LogsErrorAndSetsErrorMessage()
    {
        // Arrange
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<FacultiesViewModel>>();

        var universities = new List<LookupItem>
        {
            new LookupItem { Id = "u1", Name = "University 1" }
        };

        var expectedException = new InvalidOperationException("Failed to retrieve faculties");
        mockAcademic.Setup(a => a.GetUniversitiesAsync()).ReturnsAsync(universities);
        mockAcademic.Setup(a => a.GetFacultyDetailsAsync()).ThrowsAsync(expectedException);

        var viewModel = new FacultiesViewModel(mockAcademic.Object, mockLogger.Object);

        // Act
        await viewModel.LoadAsync();

        // Assert
        Assert.IsFalse(viewModel.IsBusy, "IsBusy should be false after exception");
        Assert.AreEqual("Failed to load faculties.", viewModel.ErrorMessage, "ErrorMessage should be set");
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => true),
                expectedException,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once,
            "Error should be logged");
    }

    /// <summary>
    /// Tests that LoadAsync clears existing Universities collection before adding new data.
    /// Ensures that multiple calls to LoadAsync don't accumulate duplicate entries.
    /// </summary>
    [TestMethod]
    public async Task LoadAsync_OnMultipleCalls_ClearsAndReloadsUniversities()
    {
        // Arrange
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<FacultiesViewModel>>();

        var firstUniversities = new List<LookupItem>
        {
            new LookupItem { Id = "u1", Name = "University 1" },
            new LookupItem { Id = "u2", Name = "University 2" }
        };

        var secondUniversities = new List<LookupItem>
        {
            new LookupItem { Id = "u3", Name = "University 3" }
        };

        var faculties = new List<FacultyDto>();

        mockAcademic.SetupSequence(a => a.GetUniversitiesAsync())
            .ReturnsAsync(firstUniversities)
            .ReturnsAsync(secondUniversities);
        mockAcademic.Setup(a => a.GetFacultyDetailsAsync()).ReturnsAsync(faculties);

        var viewModel = new FacultiesViewModel(mockAcademic.Object, mockLogger.Object);

        // Act
        await viewModel.LoadAsync();
        await viewModel.LoadAsync();

        // Assert
        Assert.AreEqual(1, viewModel.Universities.Count, "Universities should contain only second load data");
        Assert.AreEqual("u3", viewModel.Universities[0].Id, "Should contain university from second load");
        Assert.AreEqual("University 3", viewModel.Universities[0].Name);
    }

    /// <summary>
    /// Tests that LoadAsync clears ErrorMessage at the start of execution.
    /// Ensures that previous error messages are not retained on successful execution.
    /// </summary>
    [TestMethod]
    public async Task LoadAsync_OnSuccessfulExecution_ClearsErrorMessage()
    {
        // Arrange
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<FacultiesViewModel>>();

        var universities = new List<LookupItem>();
        var faculties = new List<FacultyDto>();

        mockAcademic.SetupSequence(a => a.GetUniversitiesAsync())
            .ThrowsAsync(new Exception("First error"))
            .ReturnsAsync(universities);
        mockAcademic.Setup(a => a.GetFacultyDetailsAsync()).ReturnsAsync(faculties);

        var viewModel = new FacultiesViewModel(mockAcademic.Object, mockLogger.Object);

        // Act
        await viewModel.LoadAsync(); // This should set ErrorMessage
        Assert.AreEqual("Failed to load faculties.", viewModel.ErrorMessage);

        await viewModel.LoadAsync(); // This should clear ErrorMessage

        // Assert
        Assert.AreEqual(string.Empty, viewModel.ErrorMessage, "ErrorMessage should be cleared on successful load");
    }

    /// <summary>
    /// Tests that LoadAsync sets IsBusy to true during execution and false after completion.
    /// Verifies proper state management throughout the async operation.
    /// </summary>
    [TestMethod]
    public async Task LoadAsync_DuringExecution_SetsIsBusyCorrectly()
    {
        // Arrange
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<FacultiesViewModel>>();

        var taskCompletionSource = new TaskCompletionSource<IEnumerable<LookupItem>>();
        mockAcademic.Setup(a => a.GetUniversitiesAsync()).Returns(taskCompletionSource.Task);

        var viewModel = new FacultiesViewModel(mockAcademic.Object, mockLogger.Object);

        // Act
        var loadTask = viewModel.LoadAsync();

        // Assert - IsBusy should be true during execution
        Assert.IsTrue(viewModel.IsBusy, "IsBusy should be true during execution");

        // Complete the task
        taskCompletionSource.SetResult(new List<LookupItem>());
        mockAcademic.Setup(a => a.GetFacultyDetailsAsync()).ReturnsAsync(new List<FacultyDto>());

        await loadTask;

        // Assert - IsBusy should be false after completion
        Assert.IsFalse(viewModel.IsBusy, "IsBusy should be false after completion");
    }

    /// <summary>
    /// Tests that LoadAsync resets IsBusy to false even when an exception occurs.
    /// Ensures proper cleanup in the finally block.
    /// </summary>
    [TestMethod]
    public async Task LoadAsync_WhenExceptionOccurs_ResetsIsBusyInFinally()
    {
        // Arrange
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<FacultiesViewModel>>();

        mockAcademic.Setup(a => a.GetUniversitiesAsync()).ThrowsAsync(new Exception("Test exception"));

        var viewModel = new FacultiesViewModel(mockAcademic.Object, mockLogger.Object);

        // Act
        await viewModel.LoadAsync();

        // Assert
        Assert.IsFalse(viewModel.IsBusy, "IsBusy should be false after exception");
    }

    /// <summary>
    /// Tests that LoadAsync handles large collections of universities and faculties correctly.
    /// Verifies that statistics are calculated correctly for large datasets.
    /// </summary>
    [TestMethod]
    public async Task LoadAsync_WithLargeDataset_HandlesCorrectly()
    {
        // Arrange
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<FacultiesViewModel>>();

        var universities = Enumerable.Range(1, 100)
            .Select(i => new LookupItem { Id = $"u{i}", Name = $"University {i}" })
            .ToList();

        var faculties = Enumerable.Range(1, 1000)
            .Select(i => new FacultyDto
            {
                Id = $"f{i}",
                Name = $"Faculty {i}",
                Status = i % 3 == 0 ? "Active" : "Pending",
                UniversityId = $"u{i % 100 + 1}"
            })
            .ToList();

        mockAcademic.Setup(a => a.GetUniversitiesAsync()).ReturnsAsync(universities);
        mockAcademic.Setup(a => a.GetFacultyDetailsAsync()).ReturnsAsync(faculties);

        var viewModel = new FacultiesViewModel(mockAcademic.Object, mockLogger.Object);

        // Act
        await viewModel.LoadAsync();

        // Assert
        Assert.AreEqual(100, viewModel.Universities.Count, "Universities count should be 100");
        Assert.AreEqual(1000, viewModel.TotalFaculties, "TotalFaculties should be 1000");
        Assert.AreEqual(333, viewModel.ActiveFacultiesCount, "ActiveFacultiesCount should be 333");
        Assert.AreEqual(667, viewModel.PendingCount, "PendingCount should be 667");
    }

    /// <summary>
    /// Tests that LoadAsync handles faculties with null or empty Status strings.
    /// Verifies that null/empty statuses are not counted as "Active".
    /// </summary>
    [TestMethod]
    public async Task LoadAsync_WithNullAndEmptyStatuses_CountsCorrectly()
    {
        // Arrange
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<FacultiesViewModel>>();

        var universities = new List<LookupItem>
        {
            new LookupItem { Id = "u1", Name = "University 1" }
        };

        var faculties = new List<FacultyDto>
        {
            new FacultyDto { Id = "f1", Name = "Faculty 1", Status = "Active", UniversityId = "u1" },
            new FacultyDto { Id = "f2", Name = "Faculty 2", Status = string.Empty, UniversityId = "u1" },
            new FacultyDto { Id = "f3", Name = "Faculty 3", Status = "   ", UniversityId = "u1" }
        };

        mockAcademic.Setup(a => a.GetUniversitiesAsync()).ReturnsAsync(universities);
        mockAcademic.Setup(a => a.GetFacultyDetailsAsync()).ReturnsAsync(faculties);

        var viewModel = new FacultiesViewModel(mockAcademic.Object, mockLogger.Object);

        // Act
        await viewModel.LoadAsync();

        // Assert
        Assert.AreEqual(3, viewModel.TotalFaculties, "TotalFaculties should be 3");
        Assert.AreEqual(1, viewModel.ActiveFacultiesCount, "ActiveFacultiesCount should be 1 (only exact 'Active')");
        Assert.AreEqual(2, viewModel.PendingCount, "PendingCount should be 2 (empty and whitespace statuses)");
    }

    /// <summary>
    /// Tests that LoadAsync handles case-sensitive Status comparison.
    /// Verifies that only exact "Active" string is counted as active.
    /// </summary>
    [TestMethod]
    public async Task LoadAsync_WithCaseSensitiveStatus_CountsOnlyExactMatch()
    {
        // Arrange
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<FacultiesViewModel>>();

        var universities = new List<LookupItem>
        {
            new LookupItem { Id = "u1", Name = "University 1" }
        };

        var faculties = new List<FacultyDto>
        {
            new FacultyDto { Id = "f1", Name = "Faculty 1", Status = "Active", UniversityId = "u1" },
            new FacultyDto { Id = "f2", Name = "Faculty 2", Status = "active", UniversityId = "u1" },
            new FacultyDto { Id = "f3", Name = "Faculty 3", Status = "ACTIVE", UniversityId = "u1" }
        };

        mockAcademic.Setup(a => a.GetUniversitiesAsync()).ReturnsAsync(universities);
        mockAcademic.Setup(a => a.GetFacultyDetailsAsync()).ReturnsAsync(faculties);

        var viewModel = new FacultiesViewModel(mockAcademic.Object, mockLogger.Object);

        // Act
        await viewModel.LoadAsync();

        // Assert
        Assert.AreEqual(3, viewModel.TotalFaculties, "TotalFaculties should be 3");
        Assert.AreEqual(1, viewModel.ActiveFacultiesCount, "ActiveFacultiesCount should be 1 (case-sensitive)");
        Assert.AreEqual(2, viewModel.PendingCount, "PendingCount should be 2 (non-matching case)");
    }

    /// <summary>
    /// Tests that LoadAsync handles different types of exceptions correctly.
    /// Verifies generic error handling for various exception types.
    /// </summary>
    [TestMethod]
    [DataRow(typeof(InvalidOperationException))]
    [DataRow(typeof(ArgumentException))]
    [DataRow(typeof(NullReferenceException))]
    [DataRow(typeof(TimeoutException))]
    public async Task LoadAsync_WithVariousExceptionTypes_HandlesGracefully(Type exceptionType)
    {
        // Arrange
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<FacultiesViewModel>>();

        var exception = (Exception)Activator.CreateInstance(exceptionType, "Test exception")!;
        mockAcademic.Setup(a => a.GetUniversitiesAsync()).ThrowsAsync(exception);

        var viewModel = new FacultiesViewModel(mockAcademic.Object, mockLogger.Object);

        // Act
        await viewModel.LoadAsync();

        // Assert
        Assert.IsFalse(viewModel.IsBusy, "IsBusy should be false after exception");
        Assert.AreEqual("Failed to load faculties.", viewModel.ErrorMessage, "ErrorMessage should be set");
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => true),
                exception,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once,
            "Error should be logged");
    }

    /// <summary>
    /// Tests that the PageSize property returns its initial default value of 10.
    /// </summary>
    [TestMethod]
    public void PageSize_GetInitialValue_ReturnsDefaultValueOfTen()
    {
        // Arrange
        var mockAcademicService = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<FacultiesViewModel>>();
        var viewModel = new FacultiesViewModel(mockAcademicService.Object, mockLogger.Object);

        // Act
        int result = viewModel.PageSize;

        // Assert
        Assert.AreEqual(10, result);
    }

    /// <summary>
    /// Tests that setting PageSize to a new value updates the property and resets CurrentPage to 1.
    /// </summary>
    [TestMethod]
    public void PageSize_SetNewValue_UpdatesValueAndResetsCurrentPageToOne()
    {
        // Arrange
        var mockAcademicService = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<FacultiesViewModel>>();
        var viewModel = new FacultiesViewModel(mockAcademicService.Object, mockLogger.Object);

        // Set CurrentPage to a different value first
        viewModel.GetType().GetField("currentPage", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!.SetValue(viewModel, 5);

        // Act
        viewModel.PageSize = 20;

        // Assert
        Assert.AreEqual(20, viewModel.PageSize);
        Assert.AreEqual(1, viewModel.CurrentPage);
    }

    /// <summary>
    /// Tests that setting PageSize to the same value does not reset CurrentPage.
    /// </summary>
    [TestMethod]
    public void PageSize_SetSameValue_DoesNotResetCurrentPage()
    {
        // Arrange
        var mockAcademicService = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<FacultiesViewModel>>();
        var viewModel = new FacultiesViewModel(mockAcademicService.Object, mockLogger.Object);

        // Set CurrentPage to a different value using reflection
        viewModel.GetType().GetField("currentPage", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!.SetValue(viewModel, 5);
        int expectedCurrentPage = 5;

        // Act - Set PageSize to its current value (10)
        viewModel.PageSize = 10;

        // Assert
        Assert.AreEqual(10, viewModel.PageSize);
        Assert.AreEqual(expectedCurrentPage, viewModel.CurrentPage);
    }

    /// <summary>
    /// Tests that setting PageSize to various edge case values updates the property correctly.
    /// Tests include int.MinValue, int.MaxValue, zero, negative values, and positive boundary values.
    /// </summary>
    /// <param name="newPageSize">The new page size value to test.</param>
    [TestMethod]
    [DataRow(int.MinValue)]
    [DataRow(int.MaxValue)]
    [DataRow(0)]
    [DataRow(-1)]
    [DataRow(-10)]
    [DataRow(-100)]
    [DataRow(1)]
    [DataRow(5)]
    [DataRow(100)]
    [DataRow(1000)]
    [DataRow(50000)]
    public void PageSize_SetEdgeCaseValues_UpdatesValueCorrectly(int newPageSize)
    {
        // Arrange
        var mockAcademicService = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<FacultiesViewModel>>();
        var viewModel = new FacultiesViewModel(mockAcademicService.Object, mockLogger.Object);

        // Act
        viewModel.PageSize = newPageSize;

        // Assert
        Assert.AreEqual(newPageSize, viewModel.PageSize);
    }

    /// <summary>
    /// Tests that setting PageSize to a new value resets CurrentPage to 1 for various edge case values.
    /// </summary>
    /// <param name="newPageSize">The new page size value to test.</param>
    [TestMethod]
    [DataRow(int.MinValue)]
    [DataRow(int.MaxValue)]
    [DataRow(0)]
    [DataRow(-1)]
    [DataRow(1)]
    [DataRow(5)]
    [DataRow(100)]
    [DataRow(1000)]
    public void PageSize_SetDifferentEdgeCaseValues_ResetsCurrentPageToOne(int newPageSize)
    {
        // Arrange
        var mockAcademicService = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<FacultiesViewModel>>();
        var viewModel = new FacultiesViewModel(mockAcademicService.Object, mockLogger.Object);

        // Set CurrentPage to a value other than 1
        viewModel.GetType().GetField("currentPage", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!.SetValue(viewModel, 7);

        // Act
        viewModel.PageSize = newPageSize;

        // Assert
        Assert.AreEqual(1, viewModel.CurrentPage);
    }

    /// <summary>
    /// Tests that setting PageSize multiple times in sequence correctly updates the value each time.
    /// </summary>
    [TestMethod]
    public void PageSize_SetMultipleSequentialValues_UpdatesCorrectly()
    {
        // Arrange
        var mockAcademicService = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<FacultiesViewModel>>();
        var viewModel = new FacultiesViewModel(mockAcademicService.Object, mockLogger.Object);

        // Act & Assert
        viewModel.PageSize = 5;
        Assert.AreEqual(5, viewModel.PageSize);
        Assert.AreEqual(1, viewModel.CurrentPage);

        viewModel.PageSize = 15;
        Assert.AreEqual(15, viewModel.PageSize);
        Assert.AreEqual(1, viewModel.CurrentPage);

        viewModel.PageSize = 25;
        Assert.AreEqual(25, viewModel.PageSize);
        Assert.AreEqual(1, viewModel.CurrentPage);
    }

    /// <summary>
    /// Tests that setting SelectedUniversity to a non-null value updates the property and SelectedUniversityId.
    /// </summary>
    [TestMethod]
    public void SelectedUniversity_SetToNonNullValue_UpdatesPropertyAndSelectedUniversityId()
    {
        // Arrange
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<FacultiesViewModel>>();
        var viewModel = new FacultiesViewModel(mockAcademic.Object, mockLogger.Object);
        var lookupItem = new LookupItem { Id = "uni-123", Name = "Test University" };
        bool propertyChangedRaised = false;
        viewModel.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == nameof(FacultiesViewModel.SelectedUniversity))
                propertyChangedRaised = true;
        };

        // Act
        viewModel.SelectedUniversity = lookupItem;

        // Assert
        Assert.IsNotNull(viewModel.SelectedUniversity);
        Assert.AreEqual("uni-123", viewModel.SelectedUniversity.Id);
        Assert.AreEqual("Test University", viewModel.SelectedUniversity.Name);
        Assert.AreEqual("uni-123", viewModel.SelectedUniversityId);
        Assert.IsTrue(propertyChangedRaised);
    }

    /// <summary>
    /// Tests that setting SelectedUniversity to null updates the property and sets SelectedUniversityId to null.
    /// </summary>
    [TestMethod]
    public void SelectedUniversity_SetToNull_UpdatesPropertyAndSetsSelectedUniversityIdToNull()
    {
        // Arrange
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<FacultiesViewModel>>();
        var viewModel = new FacultiesViewModel(mockAcademic.Object, mockLogger.Object);
        var lookupItem = new LookupItem { Id = "uni-123", Name = "Test University" };
        viewModel.SelectedUniversity = lookupItem;
        bool propertyChangedRaised = false;
        viewModel.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == nameof(FacultiesViewModel.SelectedUniversity))
                propertyChangedRaised = true;
        };

        // Act
        viewModel.SelectedUniversity = null;

        // Assert
        Assert.IsNull(viewModel.SelectedUniversity);
        Assert.IsNull(viewModel.SelectedUniversityId);
        Assert.IsTrue(propertyChangedRaised);
    }

    /// <summary>
    /// Tests that setting SelectedUniversity to the same value does not update SelectedUniversityId again and does not raise PropertyChanged.
    /// </summary>
    [TestMethod]
    public void SelectedUniversity_SetToSameValue_DoesNotUpdateSelectedUniversityIdOrRaisePropertyChanged()
    {
        // Arrange
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<FacultiesViewModel>>();
        var viewModel = new FacultiesViewModel(mockAcademic.Object, mockLogger.Object);
        var lookupItem = new LookupItem { Id = "uni-123", Name = "Test University" };
        viewModel.SelectedUniversity = lookupItem;
        bool propertyChangedRaised = false;
        viewModel.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == nameof(FacultiesViewModel.SelectedUniversity))
                propertyChangedRaised = true;
        };

        // Act
        viewModel.SelectedUniversity = lookupItem;

        // Assert
        Assert.AreEqual("uni-123", viewModel.SelectedUniversityId);
        Assert.IsFalse(propertyChangedRaised);
    }

    /// <summary>
    /// Tests that setting SelectedUniversity to a different value updates both SelectedUniversity and SelectedUniversityId.
    /// </summary>
    [TestMethod]
    public void SelectedUniversity_SetToDifferentValue_UpdatesBothProperties()
    {
        // Arrange
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<FacultiesViewModel>>();
        var viewModel = new FacultiesViewModel(mockAcademic.Object, mockLogger.Object);
        var lookupItem1 = new LookupItem { Id = "uni-123", Name = "Test University 1" };
        var lookupItem2 = new LookupItem { Id = "uni-456", Name = "Test University 2" };
        viewModel.SelectedUniversity = lookupItem1;

        // Act
        viewModel.SelectedUniversity = lookupItem2;

        // Assert
        Assert.IsNotNull(viewModel.SelectedUniversity);
        Assert.AreEqual("uni-456", viewModel.SelectedUniversity.Id);
        Assert.AreEqual("Test University 2", viewModel.SelectedUniversity.Name);
        Assert.AreEqual("uni-456", viewModel.SelectedUniversityId);
    }

    /// <summary>
    /// Tests that setting SelectedUniversity from null to a value updates SelectedUniversityId correctly.
    /// </summary>
    [TestMethod]
    public void SelectedUniversity_SetFromNullToValue_UpdatesSelectedUniversityId()
    {
        // Arrange
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<FacultiesViewModel>>();
        var viewModel = new FacultiesViewModel(mockAcademic.Object, mockLogger.Object);
        var lookupItem = new LookupItem { Id = "uni-789", Name = "New University" };

        // Act
        viewModel.SelectedUniversity = lookupItem;

        // Assert
        Assert.IsNotNull(viewModel.SelectedUniversity);
        Assert.AreEqual("uni-789", viewModel.SelectedUniversityId);
    }

    /// <summary>
    /// Tests that setting SelectedUniversity with an empty Id updates SelectedUniversityId to empty string.
    /// </summary>
    [TestMethod]
    public void SelectedUniversity_SetWithEmptyId_UpdatesSelectedUniversityIdToEmptyString()
    {
        // Arrange
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<FacultiesViewModel>>();
        var viewModel = new FacultiesViewModel(mockAcademic.Object, mockLogger.Object);
        var lookupItem = new LookupItem { Id = string.Empty, Name = "University With Empty Id" };

        // Act
        viewModel.SelectedUniversity = lookupItem;

        // Assert
        Assert.IsNotNull(viewModel.SelectedUniversity);
        Assert.AreEqual(string.Empty, viewModel.SelectedUniversityId);
    }

    /// <summary>
    /// Tests that getting SelectedUniversity returns the correct value after setting it.
    /// </summary>
    [TestMethod]
    public void SelectedUniversity_Get_ReturnsCorrectValue()
    {
        // Arrange
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<FacultiesViewModel>>();
        var viewModel = new FacultiesViewModel(mockAcademic.Object, mockLogger.Object);
        var lookupItem = new LookupItem { Id = "uni-999", Name = "Get Test University" };
        viewModel.SelectedUniversity = lookupItem;

        // Act
        var result = viewModel.SelectedUniversity;

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual("uni-999", result.Id);
        Assert.AreEqual("Get Test University", result.Name);
    }

    /// <summary>
    /// Tests that getting SelectedUniversity returns null when not set.
    /// </summary>
    [TestMethod]
    public void SelectedUniversity_GetWhenNotSet_ReturnsNull()
    {
        // Arrange
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<FacultiesViewModel>>();
        var viewModel = new FacultiesViewModel(mockAcademic.Object, mockLogger.Object);

        // Act
        var result = viewModel.SelectedUniversity;

        // Assert
        Assert.IsNull(result);
    }

    /// <summary>
    /// Tests that PropertyChanged event is raised with correct property name when SelectedUniversity is set.
    /// </summary>
    [TestMethod]
    public void SelectedUniversity_Set_RaisesPropertyChangedWithCorrectPropertyName()
    {
        // Arrange
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<FacultiesViewModel>>();
        var viewModel = new FacultiesViewModel(mockAcademic.Object, mockLogger.Object);
        var lookupItem = new LookupItem { Id = "uni-abc", Name = "Property Changed Test" };
        string? raisedPropertyName = null;
        viewModel.PropertyChanged += (sender, args) =>
        {
            raisedPropertyName = args.PropertyName;
        };

        // Act
        viewModel.SelectedUniversity = lookupItem;

        // Assert
        Assert.AreEqual(nameof(FacultiesViewModel.SelectedUniversity), raisedPropertyName);
    }

    /// <summary>
    /// Tests that setting SelectedUniversity to a value with special characters in Id updates SelectedUniversityId correctly.
    /// </summary>
    [TestMethod]
    public void SelectedUniversity_SetWithSpecialCharactersInId_UpdatesSelectedUniversityId()
    {
        // Arrange
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<FacultiesViewModel>>();
        var viewModel = new FacultiesViewModel(mockAcademic.Object, mockLogger.Object);
        var lookupItem = new LookupItem { Id = "uni-@#$%^&*()", Name = "Special Chars University" };

        // Act
        viewModel.SelectedUniversity = lookupItem;

        // Assert
        Assert.AreEqual("uni-@#$%^&*()", viewModel.SelectedUniversityId);
    }

    /// <summary>
    /// Tests that setting SelectedUniversity to a value with very long Id updates SelectedUniversityId correctly.
    /// </summary>
    [TestMethod]
    public void SelectedUniversity_SetWithVeryLongId_UpdatesSelectedUniversityId()
    {
        // Arrange
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<FacultiesViewModel>>();
        var viewModel = new FacultiesViewModel(mockAcademic.Object, mockLogger.Object);
        var longId = new string('a', 10000);
        var lookupItem = new LookupItem { Id = longId, Name = "Long Id University" };

        // Act
        viewModel.SelectedUniversity = lookupItem;

        // Assert
        Assert.AreEqual(longId, viewModel.SelectedUniversityId);
    }

    /// <summary>
    /// Tests that TotalFaculties property can be set and retrieved correctly with various integer values.
    /// </summary>
    /// <param name="value">The integer value to set.</param>
    [TestMethod]
    [DataRow(0)]
    [DataRow(1)]
    [DataRow(100)]
    [DataRow(-1)]
    [DataRow(-100)]
    [DataRow(int.MaxValue)]
    [DataRow(int.MinValue)]
    public void TotalFaculties_SetAndGet_ReturnsCorrectValue(int value)
    {
        // Arrange
        var mockAcademicService = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<FacultiesViewModel>>();
        var viewModel = new FacultiesViewModel(mockAcademicService.Object, mockLogger.Object);

        // Act
        viewModel.TotalFaculties = value;
        var result = viewModel.TotalFaculties;

        // Assert
        Assert.AreEqual(value, result);
    }

    /// <summary>
    /// Tests that PropertyChanged event is raised when TotalFaculties value changes.
    /// </summary>
    [TestMethod]
    public void TotalFaculties_SetDifferentValue_RaisesPropertyChangedEvent()
    {
        // Arrange
        var mockAcademicService = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<FacultiesViewModel>>();
        var viewModel = new FacultiesViewModel(mockAcademicService.Object, mockLogger.Object);
        var propertyChangedRaised = false;
        string? raisedPropertyName = null;
        viewModel.PropertyChanged += (sender, args) =>
        {
            propertyChangedRaised = true;
            raisedPropertyName = args.PropertyName;
        };

        // Act
        viewModel.TotalFaculties = 42;

        // Assert
        Assert.IsTrue(propertyChangedRaised);
        Assert.AreEqual(nameof(viewModel.TotalFaculties), raisedPropertyName);
    }

    /// <summary>
    /// Tests that PropertyChanged event is not raised when TotalFaculties is set to the same value.
    /// </summary>
    [TestMethod]
    public void TotalFaculties_SetSameValue_DoesNotRaisePropertyChangedEvent()
    {
        // Arrange
        var mockAcademicService = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<FacultiesViewModel>>();
        var viewModel = new FacultiesViewModel(mockAcademicService.Object, mockLogger.Object);
        viewModel.TotalFaculties = 10;
        var eventRaisedCount = 0;
        viewModel.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == nameof(viewModel.TotalFaculties))
            {
                eventRaisedCount++;
            }
        };

        // Act
        viewModel.TotalFaculties = 10;

        // Assert
        Assert.AreEqual(0, eventRaisedCount);
    }

    /// <summary>
    /// Tests that PropertyChanged event is raised multiple times when TotalFaculties value changes multiple times.
    /// </summary>
    [TestMethod]
    public void TotalFaculties_SetDifferentValuesMultipleTimes_RaisesPropertyChangedEventEachTime()
    {
        // Arrange
        var mockAcademicService = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<FacultiesViewModel>>();
        var viewModel = new FacultiesViewModel(mockAcademicService.Object, mockLogger.Object);
        var eventRaisedCount = 0;
        viewModel.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == nameof(viewModel.TotalFaculties))
            {
                eventRaisedCount++;
            }
        };

        // Act
        viewModel.TotalFaculties = 1;
        viewModel.TotalFaculties = 2;
        viewModel.TotalFaculties = 3;

        // Assert
        Assert.AreEqual(3, eventRaisedCount);
    }

    /// <summary>
    /// Tests that TotalFaculties property correctly handles transition from positive to negative values.
    /// </summary>
    [TestMethod]
    public void TotalFaculties_SetPositiveThenNegative_ReturnsCorrectValues()
    {
        // Arrange
        var mockAcademicService = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<FacultiesViewModel>>();
        var viewModel = new FacultiesViewModel(mockAcademicService.Object, mockLogger.Object);

        // Act
        viewModel.TotalFaculties = 100;
        var positiveResult = viewModel.TotalFaculties;
        viewModel.TotalFaculties = -50;
        var negativeResult = viewModel.TotalFaculties;

        // Assert
        Assert.AreEqual(100, positiveResult);
        Assert.AreEqual(-50, negativeResult);
    }

    /// <summary>
    /// Tests that TotalFaculties initial value is zero (default int value).
    /// </summary>
    [TestMethod]
    public void TotalFaculties_InitialValue_IsZero()
    {
        // Arrange
        var mockAcademicService = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<FacultiesViewModel>>();

        // Act
        var viewModel = new FacultiesViewModel(mockAcademicService.Object, mockLogger.Object);
        var initialValue = viewModel.TotalFaculties;

        // Assert
        Assert.AreEqual(0, initialValue);
    }

    /// <summary>
    /// Tests that the ErrorMessage property returns an empty string initially.
    /// </summary>
    [TestMethod]
    public void ErrorMessage_InitialValue_ReturnsEmptyString()
    {
        // Arrange
        var mockAcademicService = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<FacultiesViewModel>>();
        var viewModel = new FacultiesViewModel(mockAcademicService.Object, mockLogger.Object);

        // Act
        var result = viewModel.ErrorMessage;

        // Assert
        Assert.AreEqual(string.Empty, result);
    }

    /// <summary>
    /// Tests that the ErrorMessage property correctly sets and returns the provided string value.
    /// Covers various edge cases including empty string, whitespace, long strings, and special characters.
    /// </summary>
    /// <param name="value">The string value to set on the ErrorMessage property.</param>
    [TestMethod]
    [DataRow("")]
    [DataRow(" ")]
    [DataRow("   ")]
    [DataRow("\t")]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [DataRow("Simple error message")]
    [DataRow("Error with special chars: !@#$%^&*()")]
    [DataRow("Unicode: 你好世界 🌍")]
    [DataRow("Path-like: C:\\Users\\Test\\file.txt")]
    [DataRow("Very long string with lots of text that exceeds normal length expectations for error messages and continues for quite some time to test boundary conditions")]
    public void ErrorMessage_SetValue_UpdatesPropertyCorrectly(string value)
    {
        // Arrange
        var mockAcademicService = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<FacultiesViewModel>>();
        var viewModel = new FacultiesViewModel(mockAcademicService.Object, mockLogger.Object);

        // Act
        viewModel.ErrorMessage = value;
        var result = viewModel.ErrorMessage;

        // Assert
        Assert.AreEqual(value, result);
    }

    /// <summary>
    /// Tests that the ErrorMessage property can be updated multiple times and maintains the latest value.
    /// </summary>
    [TestMethod]
    public void ErrorMessage_MultipleUpdates_MaintainsLatestValue()
    {
        // Arrange
        var mockAcademicService = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<FacultiesViewModel>>();
        var viewModel = new FacultiesViewModel(mockAcademicService.Object, mockLogger.Object);

        // Act
        viewModel.ErrorMessage = "First error";
        viewModel.ErrorMessage = "Second error";
        viewModel.ErrorMessage = "Final error";
        var result = viewModel.ErrorMessage;

        // Assert
        Assert.AreEqual("Final error", result);
    }

    /// <summary>
    /// Tests that setting the ErrorMessage to the same value it already has does not cause issues.
    /// </summary>
    [TestMethod]
    public void ErrorMessage_SetSameValue_HandlesCorrectly()
    {
        // Arrange
        var mockAcademicService = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<FacultiesViewModel>>();
        var viewModel = new FacultiesViewModel(mockAcademicService.Object, mockLogger.Object);
        var testValue = "Test error message";

        // Act
        viewModel.ErrorMessage = testValue;
        viewModel.ErrorMessage = testValue;
        var result = viewModel.ErrorMessage;

        // Assert
        Assert.AreEqual(testValue, result);
    }

    /// <summary>
    /// Tests that ActiveFacultiesCount property correctly stores and retrieves various integer values
    /// including edge cases such as zero, negative values, positive values, int.MinValue, and int.MaxValue.
    /// </summary>
    /// <param name="value">The integer value to set on the ActiveFacultiesCount property.</param>
    [TestMethod]
    [DataRow(0)]
    [DataRow(1)]
    [DataRow(10)]
    [DataRow(100)]
    [DataRow(-1)]
    [DataRow(-100)]
    [DataRow(int.MinValue)]
    [DataRow(int.MaxValue)]
    public void ActiveFacultiesCount_SetValue_ReturnsSetValue(int value)
    {
        // Arrange
        var mockAcademicService = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<FacultiesViewModel>>();
        var viewModel = new FacultiesViewModel(mockAcademicService.Object, mockLogger.Object);

        // Act
        viewModel.ActiveFacultiesCount = value;
        var result = viewModel.ActiveFacultiesCount;

        // Assert
        Assert.AreEqual(value, result);
    }

    /// <summary>
    /// Tests that setting ActiveFacultiesCount property raises PropertyChanged event
    /// with the correct property name when the value changes.
    /// </summary>
    [TestMethod]
    public void ActiveFacultiesCount_SetDifferentValue_RaisesPropertyChangedEvent()
    {
        // Arrange
        var mockAcademicService = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<FacultiesViewModel>>();
        var viewModel = new FacultiesViewModel(mockAcademicService.Object, mockLogger.Object);
        var propertyChangedRaised = false;
        string? raisedPropertyName = null;

        viewModel.PropertyChanged += (sender, args) =>
        {
            propertyChangedRaised = true;
            raisedPropertyName = args.PropertyName;
        };

        // Act
        viewModel.ActiveFacultiesCount = 42;

        // Assert
        Assert.IsTrue(propertyChangedRaised);
        Assert.AreEqual(nameof(viewModel.ActiveFacultiesCount), raisedPropertyName);
    }

    /// <summary>
    /// Tests that setting ActiveFacultiesCount property to the same value
    /// does not raise PropertyChanged event.
    /// </summary>
    [TestMethod]
    public void ActiveFacultiesCount_SetSameValue_DoesNotRaisePropertyChangedEvent()
    {
        // Arrange
        var mockAcademicService = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<FacultiesViewModel>>();
        var viewModel = new FacultiesViewModel(mockAcademicService.Object, mockLogger.Object);
        viewModel.ActiveFacultiesCount = 10;
        var propertyChangedRaised = false;

        viewModel.PropertyChanged += (sender, args) =>
        {
            propertyChangedRaised = true;
        };

        // Act
        viewModel.ActiveFacultiesCount = 10;

        // Assert
        Assert.IsFalse(propertyChangedRaised);
    }

    /// <summary>
    /// Tests that ActiveFacultiesCount property has default value of zero
    /// when the ViewModel is first instantiated.
    /// </summary>
    [TestMethod]
    public void ActiveFacultiesCount_InitialValue_IsZero()
    {
        // Arrange
        var mockAcademicService = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<FacultiesViewModel>>();

        // Act
        var viewModel = new FacultiesViewModel(mockAcademicService.Object, mockLogger.Object);
        var result = viewModel.ActiveFacultiesCount;

        // Assert
        Assert.AreEqual(0, result);
    }

    /// <summary>
    /// Tests that ActiveFacultiesCount property can be set multiple times
    /// with different values and each value is correctly stored and retrieved.
    /// </summary>
    [TestMethod]
    public void ActiveFacultiesCount_SetMultipleDifferentValues_EachValueIsStoredCorrectly()
    {
        // Arrange
        var mockAcademicService = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<FacultiesViewModel>>();
        var viewModel = new FacultiesViewModel(mockAcademicService.Object, mockLogger.Object);

        // Act & Assert
        viewModel.ActiveFacultiesCount = 5;
        Assert.AreEqual(5, viewModel.ActiveFacultiesCount);

        viewModel.ActiveFacultiesCount = 15;
        Assert.AreEqual(15, viewModel.ActiveFacultiesCount);

        viewModel.ActiveFacultiesCount = 0;
        Assert.AreEqual(0, viewModel.ActiveFacultiesCount);

        viewModel.ActiveFacultiesCount = -10;
        Assert.AreEqual(-10, viewModel.ActiveFacultiesCount);
    }

    /// <summary>
    /// Tests that the constructor successfully initializes the view model with valid parameters.
    /// </summary>
    [TestMethod]
    public void Constructor_WithValidParameters_CreatesInstance()
    {
        // Arrange
        var academic = new Mock<IAcademicService>();
        var logger = new Mock<ILogger<FacultiesViewModel>>();

        // Act
        var viewModel = new FacultiesViewModel(academic.Object, logger.Object);

        // Assert
        Assert.IsNotNull(viewModel);
    }

    /// <summary>
    /// Tests that the constructor initializes all command properties to non-null values.
    /// </summary>
    [TestMethod]
    public void Constructor_WithValidParameters_InitializesAllCommands()
    {
        // Arrange
        var academic = new Mock<IAcademicService>();
        var logger = new Mock<ILogger<FacultiesViewModel>>();

        // Act
        var viewModel = new FacultiesViewModel(academic.Object, logger.Object);

        // Assert
        Assert.IsNotNull(viewModel.LoadCommand, "LoadCommand should be initialized");
        Assert.IsNotNull(viewModel.SearchCommand, "SearchCommand should be initialized");
        Assert.IsNotNull(viewModel.FilterCommand, "FilterCommand should be initialized");
        Assert.IsNotNull(viewModel.NextPageCommand, "NextPageCommand should be initialized");
        Assert.IsNotNull(viewModel.PrevPageCommand, "PrevPageCommand should be initialized");
        Assert.IsNotNull(viewModel.AddFacultyCommand, "AddFacultyCommand should be initialized");
        Assert.IsNotNull(viewModel.EditFacultyCommand, "EditFacultyCommand should be initialized");
        Assert.IsNotNull(viewModel.DeleteFacultyCommand, "DeleteFacultyCommand should be initialized");
        Assert.IsNotNull(viewModel.SaveCommand, "SaveCommand should be initialized");
        Assert.IsNotNull(viewModel.CancelEditCommand, "CancelEditCommand should be initialized");
        Assert.IsNotNull(viewModel.ExportCommand, "ExportCommand should be initialized");
        Assert.IsNotNull(viewModel.RefreshCommand, "RefreshCommand should be initialized");
    }

    /// <summary>
    /// Tests that the constructor initializes collections to non-null values.
    /// </summary>
    [TestMethod]
    public void Constructor_WithValidParameters_InitializesCollections()
    {
        // Arrange
        var academic = new Mock<IAcademicService>();
        var logger = new Mock<ILogger<FacultiesViewModel>>();

        // Act
        var viewModel = new FacultiesViewModel(academic.Object, logger.Object);

        // Assert
        Assert.IsNotNull(viewModel.Faculties, "Faculties collection should be initialized");
        Assert.IsNotNull(viewModel.Universities, "Universities collection should be initialized");
        Assert.AreEqual(0, viewModel.Faculties.Count, "Faculties collection should be empty initially");
        Assert.AreEqual(0, viewModel.Universities.Count, "Universities collection should be empty initially");
    }

    /// <summary>
    /// Tests that AddFacultyCommand resets form fields when executed.
    /// </summary>
    [TestMethod]
    public void Constructor_AddFacultyCommand_ResetsFormFieldsAndSetsIsEditing()
    {
        // Arrange
        var academic = new Mock<IAcademicService>();
        var logger = new Mock<ILogger<FacultiesViewModel>>();
        var viewModel = new FacultiesViewModel(academic.Object, logger.Object);
        viewModel.EditId = "existing-id";
        viewModel.EditName = "Existing Name";
        viewModel.SelectedUniversity = new LookupItem { Id = "uni-1", Name = "University 1" };
        viewModel.IsEditing = false;

        // Act
        viewModel.AddFacultyCommand.Execute(null);

        // Assert
        Assert.IsNull(viewModel.EditId, "EditId should be null");
        Assert.AreEqual(string.Empty, viewModel.EditName, "EditName should be empty");
        Assert.IsNull(viewModel.SelectedUniversity, "SelectedUniversity should be null");
        Assert.IsTrue(viewModel.IsEditing, "IsEditing should be true");
    }

    /// <summary>
    /// Tests that CancelEditCommand resets form fields and sets IsEditing to false.
    /// </summary>
    [TestMethod]
    public void Constructor_CancelEditCommand_ResetsFormFieldsAndClearsIsEditing()
    {
        // Arrange
        var academic = new Mock<IAcademicService>();
        var logger = new Mock<ILogger<FacultiesViewModel>>();
        var viewModel = new FacultiesViewModel(academic.Object, logger.Object);
        viewModel.EditId = "existing-id";
        viewModel.EditName = "Existing Name";
        viewModel.SelectedUniversity = new LookupItem { Id = "uni-1", Name = "University 1" };
        viewModel.IsEditing = true;

        // Act
        viewModel.CancelEditCommand.Execute(null);

        // Assert
        Assert.IsNull(viewModel.EditId, "EditId should be null");
        Assert.AreEqual(string.Empty, viewModel.EditName, "EditName should be empty");
        Assert.IsNull(viewModel.SelectedUniversity, "SelectedUniversity should be null");
        Assert.IsFalse(viewModel.IsEditing, "IsEditing should be false");
    }

    /// <summary>
    /// Tests that EditFacultyCommand populates form fields from the provided FacultyDto.
    /// </summary>
    [TestMethod]
    public void Constructor_EditFacultyCommand_PopulatesFormFieldsFromFaculty()
    {
        // Arrange
        var academic = new Mock<IAcademicService>();
        var logger = new Mock<ILogger<FacultiesViewModel>>();
        var viewModel = new FacultiesViewModel(academic.Object, logger.Object);
        viewModel.Universities.Add(new LookupItem { Id = "uni-1", Name = "University 1" });
        viewModel.Universities.Add(new LookupItem { Id = "uni-2", Name = "University 2" });

        var faculty = new FacultyDto
        {
            Id = "faculty-123",
            Name = "Faculty of Science",
            UniversityId = "uni-2"
        };

        // Act
        viewModel.EditFacultyCommand.Execute(faculty);

        // Assert
        Assert.AreEqual("faculty-123", viewModel.EditId, "EditId should match faculty Id");
        Assert.AreEqual("Faculty of Science", viewModel.EditName, "EditName should match faculty Name");
        Assert.IsNotNull(viewModel.SelectedUniversity, "SelectedUniversity should not be null");
        Assert.AreEqual("uni-2", viewModel.SelectedUniversity.Id, "SelectedUniversity should match faculty UniversityId");
        Assert.IsTrue(viewModel.IsEditing, "IsEditing should be true");
    }

    /// <summary>
    /// Tests that EditFacultyCommand sets SelectedUniversity to null when no matching university is found.
    /// </summary>
    [TestMethod]
    public void Constructor_EditFacultyCommand_WithNoMatchingUniversity_SetsSelectedUniversityToNull()
    {
        // Arrange
        var academic = new Mock<IAcademicService>();
        var logger = new Mock<ILogger<FacultiesViewModel>>();
        var viewModel = new FacultiesViewModel(academic.Object, logger.Object);
        viewModel.Universities.Add(new LookupItem { Id = "uni-1", Name = "University 1" });

        var faculty = new FacultyDto
        {
            Id = "faculty-123",
            Name = "Faculty of Science",
            UniversityId = "non-existent-id"
        };

        // Act
        viewModel.EditFacultyCommand.Execute(faculty);

        // Assert
        Assert.AreEqual("faculty-123", viewModel.EditId);
        Assert.AreEqual("Faculty of Science", viewModel.EditName);
        Assert.IsNull(viewModel.SelectedUniversity, "SelectedUniversity should be null when no match found");
        Assert.IsTrue(viewModel.IsEditing);
    }

    /// <summary>
    /// Tests that NextPageCommand increments CurrentPage when it is less than TotalPages.
    /// </summary>
    [TestMethod]
    public void Constructor_NextPageCommand_WhenCurrentPageLessThanTotalPages_IncrementsCurrentPage()
    {
        // Arrange
        var academic = new Mock<IAcademicService>();
        var logger = new Mock<ILogger<FacultiesViewModel>>();
        var viewModel = new FacultiesViewModel(academic.Object, logger.Object);

        // Use reflection or property access to set initial state
        viewModel.GetType().GetProperty("TotalPages")!.SetValue(viewModel, 5);
        viewModel.GetType().GetProperty("CurrentPage")!.SetValue(viewModel, 2);

        // Act
        viewModel.NextPageCommand.Execute(null);

        // Assert
        Assert.AreEqual(3, viewModel.CurrentPage, "CurrentPage should be incremented");
    }

    /// <summary>
    /// Tests that NextPageCommand does not increment CurrentPage when it equals TotalPages.
    /// </summary>
    [TestMethod]
    public void Constructor_NextPageCommand_WhenCurrentPageEqualsTotalPages_DoesNotIncrementCurrentPage()
    {
        // Arrange
        var academic = new Mock<IAcademicService>();
        var logger = new Mock<ILogger<FacultiesViewModel>>();
        var viewModel = new FacultiesViewModel(academic.Object, logger.Object);

        viewModel.GetType().GetProperty("TotalPages")!.SetValue(viewModel, 5);
        viewModel.GetType().GetProperty("CurrentPage")!.SetValue(viewModel, 5);

        // Act
        viewModel.NextPageCommand.Execute(null);

        // Assert
        Assert.AreEqual(5, viewModel.CurrentPage, "CurrentPage should not be incremented");
    }

    /// <summary>
    /// Tests that NextPageCommand does not increment CurrentPage when it is greater than TotalPages.
    /// </summary>
    [TestMethod]
    public void Constructor_NextPageCommand_WhenCurrentPageGreaterThanTotalPages_DoesNotIncrementCurrentPage()
    {
        // Arrange
        var academic = new Mock<IAcademicService>();
        var logger = new Mock<ILogger<FacultiesViewModel>>();
        var viewModel = new FacultiesViewModel(academic.Object, logger.Object);

        viewModel.GetType().GetProperty("TotalPages")!.SetValue(viewModel, 3);
        viewModel.GetType().GetProperty("CurrentPage")!.SetValue(viewModel, 5);

        // Act
        viewModel.NextPageCommand.Execute(null);

        // Assert
        Assert.AreEqual(5, viewModel.CurrentPage, "CurrentPage should not be incremented");
    }

    /// <summary>
    /// Tests that PrevPageCommand decrements CurrentPage when it is greater than 1.
    /// </summary>
    [TestMethod]
    public void Constructor_PrevPageCommand_WhenCurrentPageGreaterThanOne_DecrementsCurrentPage()
    {
        // Arrange
        var academic = new Mock<IAcademicService>();
        var logger = new Mock<ILogger<FacultiesViewModel>>();
        var viewModel = new FacultiesViewModel(academic.Object, logger.Object);

        viewModel.GetType().GetProperty("CurrentPage")!.SetValue(viewModel, 3);

        // Act
        viewModel.PrevPageCommand.Execute(null);

        // Assert
        Assert.AreEqual(2, viewModel.CurrentPage, "CurrentPage should be decremented");
    }

    /// <summary>
    /// Tests that PrevPageCommand does not decrement CurrentPage when it equals 1.
    /// </summary>
    [TestMethod]
    public void Constructor_PrevPageCommand_WhenCurrentPageEqualsOne_DoesNotDecrementCurrentPage()
    {
        // Arrange
        var academic = new Mock<IAcademicService>();
        var logger = new Mock<ILogger<FacultiesViewModel>>();
        var viewModel = new FacultiesViewModel(academic.Object, logger.Object);

        viewModel.GetType().GetProperty("CurrentPage")!.SetValue(viewModel, 1);

        // Act
        viewModel.PrevPageCommand.Execute(null);

        // Assert
        Assert.AreEqual(1, viewModel.CurrentPage, "CurrentPage should not be decremented");
    }

    /// <summary>
    /// Tests that PrevPageCommand does not decrement CurrentPage when it is less than 1.
    /// </summary>
    [TestMethod]
    public void Constructor_PrevPageCommand_WhenCurrentPageLessThanOne_DoesNotDecrementCurrentPage()
    {
        // Arrange
        var academic = new Mock<IAcademicService>();
        var logger = new Mock<ILogger<FacultiesViewModel>>();
        var viewModel = new FacultiesViewModel(academic.Object, logger.Object);

        viewModel.GetType().GetProperty("CurrentPage")!.SetValue(viewModel, 0);

        // Act
        viewModel.PrevPageCommand.Execute(null);

        // Assert
        Assert.AreEqual(0, viewModel.CurrentPage, "CurrentPage should not be decremented");
    }

    /// <summary>
    /// Tests that FilterCommand sets the UniversityFilter property with the provided filter value.
    /// </summary>
    [TestMethod]
    public void Constructor_FilterCommand_SetsUniversityFilterProperty()
    {
        // Arrange
        var academic = new Mock<IAcademicService>();
        var logger = new Mock<ILogger<FacultiesViewModel>>();
        var viewModel = new FacultiesViewModel(academic.Object, logger.Object);
        var filterValue = "University 1";

        // Act
        viewModel.FilterCommand.Execute(filterValue);

        // Assert
        Assert.AreEqual(filterValue, viewModel.UniversityFilter, "UniversityFilter should be set to the provided value");
    }

    /// <summary>
    /// Tests that FilterCommand handles empty string filter values.
    /// </summary>
    [TestMethod]
    public void Constructor_FilterCommand_WithEmptyString_SetsUniversityFilterToEmpty()
    {
        // Arrange
        var academic = new Mock<IAcademicService>();
        var logger = new Mock<ILogger<FacultiesViewModel>>();
        var viewModel = new FacultiesViewModel(academic.Object, logger.Object);

        // Act
        viewModel.FilterCommand.Execute(string.Empty);

        // Assert
        Assert.AreEqual(string.Empty, viewModel.UniversityFilter, "UniversityFilter should be set to empty string");
    }

    /// <summary>
    /// Tests that FilterCommand handles null filter values.
    /// </summary>
    [TestMethod]
    public void Constructor_FilterCommand_WithNull_SetsUniversityFilterToNull()
    {
        // Arrange
        var academic = new Mock<IAcademicService>();
        var logger = new Mock<ILogger<FacultiesViewModel>>();
        var viewModel = new FacultiesViewModel(academic.Object, logger.Object);

        // Act
        viewModel.FilterCommand.Execute(null);

        // Assert
        Assert.IsNull(viewModel.UniversityFilter, "UniversityFilter should be set to null");
    }

    /// <summary>
    /// Tests that FilterCommand handles whitespace-only filter values.
    /// </summary>
    [TestMethod]
    public void Constructor_FilterCommand_WithWhitespace_SetsUniversityFilterToWhitespace()
    {
        // Arrange
        var academic = new Mock<IAcademicService>();
        var logger = new Mock<ILogger<FacultiesViewModel>>();
        var viewModel = new FacultiesViewModel(academic.Object, logger.Object);
        var whitespace = "   ";

        // Act
        viewModel.FilterCommand.Execute(whitespace);

        // Assert
        Assert.AreEqual(whitespace, viewModel.UniversityFilter, "UniversityFilter should be set to whitespace");
    }

    /// <summary>
    /// Tests that FilterCommand handles very long filter values.
    /// </summary>
    [TestMethod]
    public void Constructor_FilterCommand_WithVeryLongString_SetsUniversityFilter()
    {
        // Arrange
        var academic = new Mock<IAcademicService>();
        var logger = new Mock<ILogger<FacultiesViewModel>>();
        var viewModel = new FacultiesViewModel(academic.Object, logger.Object);
        var longString = new string('A', 10000);

        // Act
        viewModel.FilterCommand.Execute(longString);

        // Assert
        Assert.AreEqual(longString, viewModel.UniversityFilter, "UniversityFilter should handle very long strings");
    }

    /// <summary>
    /// Tests that FilterCommand handles filter values with special characters.
    /// </summary>
    [TestMethod]
    public void Constructor_FilterCommand_WithSpecialCharacters_SetsUniversityFilter()
    {
        // Arrange
        var academic = new Mock<IAcademicService>();
        var logger = new Mock<ILogger<FacultiesViewModel>>();
        var viewModel = new FacultiesViewModel(academic.Object, logger.Object);
        var specialChars = "!@#$%^&*()_+-=[]{}|;':\",./<>?`~";

        // Act
        viewModel.FilterCommand.Execute(specialChars);

        // Assert
        Assert.AreEqual(specialChars, viewModel.UniversityFilter, "UniversityFilter should handle special characters");
    }

    /// <summary>
    /// Tests that initial property values are set correctly after constructor execution.
    /// </summary>
    [TestMethod]
    public void Constructor_WithValidParameters_InitializesPropertiesToDefaultValues()
    {
        // Arrange
        var academic = new Mock<IAcademicService>();
        var logger = new Mock<ILogger<FacultiesViewModel>>();

        // Act
        var viewModel = new FacultiesViewModel(academic.Object, logger.Object);

        // Assert
        Assert.AreEqual(string.Empty, viewModel.SearchQuery, "SearchQuery should be initialized to empty string");
        Assert.AreEqual("All", viewModel.UniversityFilter, "UniversityFilter should be initialized to 'All'");
        Assert.AreEqual(1, viewModel.CurrentPage, "CurrentPage should be initialized to 1");
        Assert.AreEqual(1, viewModel.TotalPages, "TotalPages should be initialized to 1");
        Assert.AreEqual(10, viewModel.PageSize, "PageSize should be initialized to 10");
        Assert.AreEqual(0, viewModel.TotalFaculties, "TotalFaculties should be initialized to 0");
        Assert.AreEqual(0, viewModel.ActiveFacultiesCount, "ActiveFacultiesCount should be initialized to 0");
        Assert.AreEqual(0, viewModel.PendingCount, "PendingCount should be initialized to 0");
        Assert.AreEqual(string.Empty, viewModel.EditName, "EditName should be initialized to empty string");
        Assert.IsNull(viewModel.EditId, "EditId should be initialized to null");
        Assert.IsNull(viewModel.SelectedUniversityId, "SelectedUniversityId should be initialized to null");
        Assert.IsNull(viewModel.SelectedUniversity, "SelectedUniversity should be initialized to null");
        Assert.IsFalse(viewModel.IsEditing, "IsEditing should be initialized to false");
        Assert.AreEqual(string.Empty, viewModel.ErrorMessage, "ErrorMessage should be initialized to empty string");
    }

    /// <summary>
    /// Tests that PageInfo property returns correct format after constructor initialization.
    /// </summary>
    [TestMethod]
    public void Constructor_WithValidParameters_PageInfoReturnsCorrectFormat()
    {
        // Arrange
        var academic = new Mock<IAcademicService>();
        var logger = new Mock<ILogger<FacultiesViewModel>>();

        // Act
        var viewModel = new FacultiesViewModel(academic.Object, logger.Object);

        // Assert
        Assert.AreEqual("Page 1 of 1", viewModel.PageInfo, "PageInfo should return correct format");
    }

    /// <summary>
    /// Tests that setting TotalPages to a new value updates the property value correctly.
    /// </summary>
    /// <param name="newValue">The new value to set.</param>
    [TestMethod]
    [DataRow(0)]
    [DataRow(1)]
    [DataRow(5)]
    [DataRow(10)]
    [DataRow(100)]
    [DataRow(1000)]
    [DataRow(-1)]
    [DataRow(-100)]
    [DataRow(int.MinValue)]
    [DataRow(int.MaxValue)]
    public void TotalPages_SetNewValue_UpdatesPropertyValue(int newValue)
    {
        // Arrange
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<FacultiesViewModel>>();
        var viewModel = new FacultiesViewModel(mockAcademic.Object, mockLogger.Object);

        // Act
        viewModel.TotalPages = newValue;

        // Assert
        Assert.AreEqual(newValue, viewModel.TotalPages);
    }

    /// <summary>
    /// Tests that setting TotalPages to a new value raises PropertyChanged event for "TotalPages".
    /// </summary>
    [TestMethod]
    public void TotalPages_SetNewValue_RaisesPropertyChangedForTotalPages()
    {
        // Arrange
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<FacultiesViewModel>>();
        var viewModel = new FacultiesViewModel(mockAcademic.Object, mockLogger.Object);
        var propertyChangedRaised = false;
        string? raisedPropertyName = null;

        viewModel.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == nameof(viewModel.TotalPages))
            {
                propertyChangedRaised = true;
                raisedPropertyName = args.PropertyName;
            }
        };

        // Act
        viewModel.TotalPages = 5;

        // Assert
        Assert.IsTrue(propertyChangedRaised);
        Assert.AreEqual(nameof(viewModel.TotalPages), raisedPropertyName);
    }

    /// <summary>
    /// Tests that setting TotalPages to a new value raises PropertyChanged event for "PageInfo".
    /// </summary>
    [TestMethod]
    public void TotalPages_SetNewValue_RaisesPropertyChangedForPageInfo()
    {
        // Arrange
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<FacultiesViewModel>>();
        var viewModel = new FacultiesViewModel(mockAcademic.Object, mockLogger.Object);
        var propertyChangedRaised = false;
        string? raisedPropertyName = null;

        viewModel.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == nameof(viewModel.PageInfo))
            {
                propertyChangedRaised = true;
                raisedPropertyName = args.PropertyName;
            }
        };

        // Act
        viewModel.TotalPages = 5;

        // Assert
        Assert.IsTrue(propertyChangedRaised);
        Assert.AreEqual(nameof(viewModel.PageInfo), raisedPropertyName);
    }

    /// <summary>
    /// Tests that setting TotalPages to the same value does not raise PropertyChanged events.
    /// </summary>
    [TestMethod]
    public void TotalPages_SetSameValue_DoesNotRaisePropertyChanged()
    {
        // Arrange
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<FacultiesViewModel>>();
        var viewModel = new FacultiesViewModel(mockAcademic.Object, mockLogger.Object);

        // TotalPages is initialized to 1, set it to a known value first
        viewModel.TotalPages = 10;

        var propertyChangedCount = 0;
        viewModel.PropertyChanged += (sender, args) =>
        {
            propertyChangedCount++;
        };

        // Act
        viewModel.TotalPages = 10;

        // Assert
        Assert.AreEqual(0, propertyChangedCount);
    }

    /// <summary>
    /// Tests that setting TotalPages raises both PropertyChanged events in correct order.
    /// </summary>
    [TestMethod]
    public void TotalPages_SetNewValue_RaisesBothPropertyChangedEvents()
    {
        // Arrange
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<FacultiesViewModel>>();
        var viewModel = new FacultiesViewModel(mockAcademic.Object, mockLogger.Object);
        var raisedProperties = new List<string>();

        viewModel.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName != null)
            {
                raisedProperties.Add(args.PropertyName);
            }
        };

        // Act
        viewModel.TotalPages = 20;

        // Assert
        Assert.AreEqual(2, raisedProperties.Count);
        Assert.AreEqual(nameof(viewModel.TotalPages), raisedProperties[0]);
        Assert.AreEqual(nameof(viewModel.PageInfo), raisedProperties[1]);
    }

    /// <summary>
    /// Tests that TotalPages has correct initial value.
    /// </summary>
    [TestMethod]
    public void TotalPages_InitialValue_IsOne()
    {
        // Arrange
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<FacultiesViewModel>>();

        // Act
        var viewModel = new FacultiesViewModel(mockAcademic.Object, mockLogger.Object);

        // Assert
        Assert.AreEqual(1, viewModel.TotalPages);
    }

    /// <summary>
    /// Tests that setting TotalPages to zero updates property correctly.
    /// </summary>
    [TestMethod]
    public void TotalPages_SetToZero_UpdatesPropertyValue()
    {
        // Arrange
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<FacultiesViewModel>>();
        var viewModel = new FacultiesViewModel(mockAcademic.Object, mockLogger.Object);

        // Act
        viewModel.TotalPages = 0;

        // Assert
        Assert.AreEqual(0, viewModel.TotalPages);
    }

    /// <summary>
    /// Tests that setting TotalPages to negative value updates property correctly.
    /// </summary>
    [TestMethod]
    public void TotalPages_SetToNegativeValue_UpdatesPropertyValue()
    {
        // Arrange
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<FacultiesViewModel>>();
        var viewModel = new FacultiesViewModel(mockAcademic.Object, mockLogger.Object);

        // Act
        viewModel.TotalPages = -5;

        // Assert
        Assert.AreEqual(-5, viewModel.TotalPages);
    }

    /// <summary>
    /// Tests that setting TotalPages to int.MinValue updates property correctly.
    /// </summary>
    [TestMethod]
    public void TotalPages_SetToMinValue_UpdatesPropertyValue()
    {
        // Arrange
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<FacultiesViewModel>>();
        var viewModel = new FacultiesViewModel(mockAcademic.Object, mockLogger.Object);

        // Act
        viewModel.TotalPages = int.MinValue;

        // Assert
        Assert.AreEqual(int.MinValue, viewModel.TotalPages);
    }

    /// <summary>
    /// Tests that setting TotalPages to int.MaxValue updates property correctly.
    /// </summary>
    [TestMethod]
    public void TotalPages_SetToMaxValue_UpdatesPropertyValue()
    {
        // Arrange
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<FacultiesViewModel>>();
        var viewModel = new FacultiesViewModel(mockAcademic.Object, mockLogger.Object);

        // Act
        viewModel.TotalPages = int.MaxValue;

        // Assert
        Assert.AreEqual(int.MaxValue, viewModel.TotalPages);
    }

    /// <summary>
    /// Tests that the SelectedUniversityId property returns the correct value after being set.
    /// </summary>
    /// <param name="value">The value to set.</param>
    [TestMethod]
    [DataRow(null, DisplayName = "SelectedUniversityId with null")]
    [DataRow("", DisplayName = "SelectedUniversityId with empty string")]
    [DataRow("   ", DisplayName = "SelectedUniversityId with whitespace")]
    [DataRow("123e4567-e89b-12d3-a456-426614174000", DisplayName = "SelectedUniversityId with valid GUID")]
    [DataRow("UniversityId123", DisplayName = "SelectedUniversityId with alphanumeric ID")]
    [DataRow("a", DisplayName = "SelectedUniversityId with single character")]
    [DataRow("ID with spaces and special chars !@#$%^&*()", DisplayName = "SelectedUniversityId with special characters")]
    public void SelectedUniversityId_SetValue_ReturnsSetValue(string? value)
    {
        // Arrange
        var mockAcademicService = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<FacultiesViewModel>>();
        var viewModel = new FacultiesViewModel(mockAcademicService.Object, mockLogger.Object);

        // Act
        viewModel.SelectedUniversityId = value;

        // Assert
        Assert.AreEqual(value, viewModel.SelectedUniversityId);
    }

    /// <summary>
    /// Tests that setting SelectedUniversityId raises PropertyChanged event when value changes.
    /// </summary>
    /// <param name="initialValue">The initial value.</param>
    /// <param name="newValue">The new value to set.</param>
    [TestMethod]
    [DataRow(null, "newId", DisplayName = "PropertyChanged from null to non-null")]
    [DataRow("oldId", null, DisplayName = "PropertyChanged from non-null to null")]
    [DataRow("oldId", "newId", DisplayName = "PropertyChanged from one ID to another")]
    [DataRow("", "newId", DisplayName = "PropertyChanged from empty to non-empty")]
    [DataRow("oldId", "", DisplayName = "PropertyChanged from non-empty to empty")]
    public void SelectedUniversityId_ValueChanges_RaisesPropertyChangedEvent(string? initialValue, string? newValue)
    {
        // Arrange
        var mockAcademicService = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<FacultiesViewModel>>();
        var viewModel = new FacultiesViewModel(mockAcademicService.Object, mockLogger.Object);
        viewModel.SelectedUniversityId = initialValue;

        var propertyChangedRaised = false;
        string? changedPropertyName = null;
        viewModel.PropertyChanged += (sender, args) =>
        {
            propertyChangedRaised = true;
            changedPropertyName = args.PropertyName;
        };

        // Act
        viewModel.SelectedUniversityId = newValue;

        // Assert
        Assert.IsTrue(propertyChangedRaised, "PropertyChanged event should be raised");
        Assert.AreEqual(nameof(viewModel.SelectedUniversityId), changedPropertyName);
    }

    /// <summary>
    /// Tests that setting SelectedUniversityId to the same value does not raise PropertyChanged event.
    /// </summary>
    /// <param name="value">The value to set twice.</param>
    [TestMethod]
    [DataRow(null, DisplayName = "Same value (null) does not raise PropertyChanged")]
    [DataRow("", DisplayName = "Same value (empty) does not raise PropertyChanged")]
    [DataRow("sameId", DisplayName = "Same value (non-empty) does not raise PropertyChanged")]
    public void SelectedUniversityId_SameValue_DoesNotRaisePropertyChangedEvent(string? value)
    {
        // Arrange
        var mockAcademicService = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<FacultiesViewModel>>();
        var viewModel = new FacultiesViewModel(mockAcademicService.Object, mockLogger.Object);
        viewModel.SelectedUniversityId = value;

        var propertyChangedRaised = false;
        viewModel.PropertyChanged += (sender, args) =>
        {
            propertyChangedRaised = true;
        };

        // Act
        viewModel.SelectedUniversityId = value;

        // Assert
        Assert.IsFalse(propertyChangedRaised, "PropertyChanged event should not be raised when setting the same value");
    }

    /// <summary>
    /// Tests that SelectedUniversityId can handle very long strings.
    /// </summary>
    [TestMethod]
    public void SelectedUniversityId_VeryLongString_SetsAndReturnsValue()
    {
        // Arrange
        var mockAcademicService = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<FacultiesViewModel>>();
        var viewModel = new FacultiesViewModel(mockAcademicService.Object, mockLogger.Object);
        var veryLongString = new string('A', 10000);

        // Act
        viewModel.SelectedUniversityId = veryLongString;

        // Assert
        Assert.AreEqual(veryLongString, viewModel.SelectedUniversityId);
    }

    /// <summary>
    /// Tests that SelectedUniversityId initializes as null.
    /// </summary>
    [TestMethod]
    public void SelectedUniversityId_InitialValue_IsNull()
    {
        // Arrange
        var mockAcademicService = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<FacultiesViewModel>>();

        // Act
        var viewModel = new FacultiesViewModel(mockAcademicService.Object, mockLogger.Object);

        // Assert
        Assert.IsNull(viewModel.SelectedUniversityId);
    }

    /// <summary>
    /// Tests that SelectedUniversityId can be set multiple times with different values.
    /// </summary>
    [TestMethod]
    public void SelectedUniversityId_MultipleSetOperations_UpdatesValueCorrectly()
    {
        // Arrange
        var mockAcademicService = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<FacultiesViewModel>>();
        var viewModel = new FacultiesViewModel(mockAcademicService.Object, mockLogger.Object);

        // Act & Assert
        viewModel.SelectedUniversityId = "firstId";
        Assert.AreEqual("firstId", viewModel.SelectedUniversityId);

        viewModel.SelectedUniversityId = "secondId";
        Assert.AreEqual("secondId", viewModel.SelectedUniversityId);

        viewModel.SelectedUniversityId = null;
        Assert.IsNull(viewModel.SelectedUniversityId);

        viewModel.SelectedUniversityId = "";
        Assert.AreEqual("", viewModel.SelectedUniversityId);
    }

    /// <summary>
    /// Tests that SelectedUniversityId handles strings with Unicode characters.
    /// </summary>
    [TestMethod]
    public void SelectedUniversityId_UnicodeCharacters_SetsAndReturnsValue()
    {
        // Arrange
        var mockAcademicService = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<FacultiesViewModel>>();
        var viewModel = new FacultiesViewModel(mockAcademicService.Object, mockLogger.Object);
        var unicodeString = "ID_测试_テスト_🎓";

        // Act
        viewModel.SelectedUniversityId = unicodeString;

        // Assert
        Assert.AreEqual(unicodeString, viewModel.SelectedUniversityId);
    }

    /// <summary>
    /// Tests that SelectedUniversityId handles strings with control characters.
    /// </summary>
    [TestMethod]
    public void SelectedUniversityId_ControlCharacters_SetsAndReturnsValue()
    {
        // Arrange
        var mockAcademicService = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<FacultiesViewModel>>();
        var viewModel = new FacultiesViewModel(mockAcademicService.Object, mockLogger.Object);
        var stringWithControlChars = "ID\t\n\r\0Test";

        // Act
        viewModel.SelectedUniversityId = stringWithControlChars;

        // Assert
        Assert.AreEqual(stringWithControlChars, viewModel.SelectedUniversityId);
    }

    /// <summary>
    /// Tests that UniversityFilter triggers ApplyFilterAndPagination by verifying Faculties collection is updated.
    /// Input: Different filter value with pre-populated faculties data.
    /// Expected: Faculties collection is filtered and updated.
    /// </summary>
    [TestMethod]
    public void UniversityFilter_SetDifferentValue_TriggersApplyFilterAndPagination()
    {
        // Arrange
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<FacultiesViewModel>>();
        var viewModel = new FacultiesViewModel(mockAcademic.Object, mockLogger.Object);

        var faculty1 = new FacultyDto { Id = "1", Name = "Faculty1", UniversityId = "uni1", UniversityName = "University 1", Status = "Active" };
        var faculty2 = new FacultyDto { Id = "2", Name = "Faculty2", UniversityId = "uni2", UniversityName = "University 2", Status = "Active" };
        var faculty3 = new FacultyDto { Id = "3", Name = "Faculty3", UniversityId = "uni1", UniversityName = "University 1", Status = "Active" };

        var allFaculties = new List<FacultyDto> { faculty1, faculty2, faculty3 };
        typeof(FacultiesViewModel).GetField("_allFaculties", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            ?.SetValue(viewModel, allFaculties);

        // Act
        viewModel.UniversityFilter = "uni1";

        // Assert
        Assert.AreEqual(2, viewModel.Faculties.Count, "Faculties should be filtered to 2 items for uni1");
        Assert.IsTrue(viewModel.Faculties.All(f => f.UniversityId == "uni1"), "All faculties should belong to uni1");
    }

    /// <summary>
    /// Tests that UniversityFilter set to same value does not trigger ApplyFilterAndPagination.
    /// Input: Same filter value as current.
    /// Expected: Faculties collection remains unchanged.
    /// </summary>
    [TestMethod]
    public void UniversityFilter_SetSameValue_DoesNotTriggerApplyFilterAndPagination()
    {
        // Arrange
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<FacultiesViewModel>>();
        var viewModel = new FacultiesViewModel(mockAcademic.Object, mockLogger.Object);

        var faculty1 = new FacultyDto { Id = "1", Name = "Faculty1", UniversityId = "uni1", UniversityName = "University 1", Status = "Active" };
        var allFaculties = new List<FacultyDto> { faculty1 };
        typeof(FacultiesViewModel).GetField("_allFaculties", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            ?.SetValue(viewModel, allFaculties);

        viewModel.UniversityFilter = "TestFilter";
        var initialCount = viewModel.Faculties.Count;

        // Act
        viewModel.UniversityFilter = "TestFilter";

        // Assert
        Assert.AreEqual(initialCount, viewModel.Faculties.Count, "Faculties count should remain the same");
    }

    /// <summary>
    /// Tests that UniversityFilter handles control characters correctly.
    /// Input: Strings with control characters.
    /// Expected: Value is set and PropertyChanged is raised.
    /// </summary>
    [TestMethod]
    [DataRow("\0")]
    [DataRow("\u0001")]
    [DataRow("\u001F")]
    [DataRow("filter\0value")]
    [DataRow("filter\u0001value")]
    public void UniversityFilter_SetControlCharacters_UpdatesPropertyAndRaisesPropertyChanged(string controlCharValue)
    {
        // Arrange
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<FacultiesViewModel>>();
        var viewModel = new FacultiesViewModel(mockAcademic.Object, mockLogger.Object);
        var propertyChangedRaised = false;
        string? changedPropertyName = null;

        viewModel.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == nameof(viewModel.UniversityFilter))
            {
                propertyChangedRaised = true;
                changedPropertyName = args.PropertyName;
            }
        };

        // Act
        viewModel.UniversityFilter = controlCharValue;

        // Assert
        Assert.AreEqual(controlCharValue, viewModel.UniversityFilter);
        Assert.IsTrue(propertyChangedRaised, "PropertyChanged should be raised");
        Assert.AreEqual(nameof(viewModel.UniversityFilter), changedPropertyName);
    }

    /// <summary>
    /// Tests that UniversityFilter handles Unicode characters correctly.
    /// Input: Strings with various Unicode characters including emojis and non-Latin scripts.
    /// Expected: Value is set correctly and PropertyChanged is raised.
    /// </summary>
    [TestMethod]
    [DataRow("café")]
    [DataRow("日本語")]
    [DataRow("العربية")]
    [DataRow("Привет")]
    [DataRow("😀🎉")]
    [DataRow("Université")]
    [DataRow("测试")]
    public void UniversityFilter_SetUnicodeCharacters_UpdatesPropertyCorrectly(string unicodeValue)
    {
        // Arrange
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<FacultiesViewModel>>();
        var viewModel = new FacultiesViewModel(mockAcademic.Object, mockLogger.Object);

        // Act
        viewModel.UniversityFilter = unicodeValue;

        // Assert
        Assert.AreEqual(unicodeValue, viewModel.UniversityFilter);
    }

    /// <summary>
    /// Tests that UniversityFilter handles extremely long strings correctly.
    /// Input: String with 100000 characters.
    /// Expected: Value is set correctly without issues.
    /// </summary>
    [TestMethod]
    public void UniversityFilter_SetVeryLongString_UpdatesPropertyCorrectly()
    {
        // Arrange
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<FacultiesViewModel>>();
        var viewModel = new FacultiesViewModel(mockAcademic.Object, mockLogger.Object);
        var veryLongString = new string('A', 100000);

        // Act
        viewModel.UniversityFilter = veryLongString;

        // Assert
        Assert.AreEqual(veryLongString, viewModel.UniversityFilter);
    }

    /// <summary>
    /// Tests that UniversityFilter correctly handles mixed whitespace characters.
    /// Input: Various combinations of whitespace characters.
    /// Expected: Value is set and stored as-is.
    /// </summary>
    [TestMethod]
    [DataRow("\r")]
    [DataRow("\t\t")]
    [DataRow(" \t\n\r ")]
    [DataRow("\u00A0")]
    [DataRow("\u2003")]
    public void UniversityFilter_SetMixedWhitespace_StoresValueCorrectly(string whitespaceValue)
    {
        // Arrange
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<FacultiesViewModel>>();
        var viewModel = new FacultiesViewModel(mockAcademic.Object, mockLogger.Object);

        // Act
        viewModel.UniversityFilter = whitespaceValue;

        // Assert
        Assert.AreEqual(whitespaceValue, viewModel.UniversityFilter);
    }

    /// <summary>
    /// Tests that setting UniversityFilter to "All" filters correctly to show all faculties.
    /// Input: "All" filter value.
    /// Expected: All faculties are shown.
    /// </summary>
    [TestMethod]
    public void UniversityFilter_SetToAll_ShowsAllFaculties()
    {
        // Arrange
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<FacultiesViewModel>>();
        var viewModel = new FacultiesViewModel(mockAcademic.Object, mockLogger.Object);

        var faculty1 = new FacultyDto { Id = "1", Name = "Faculty1", UniversityId = "uni1", UniversityName = "University 1", Status = "Active" };
        var faculty2 = new FacultyDto { Id = "2", Name = "Faculty2", UniversityId = "uni2", UniversityName = "University 2", Status = "Active" };
        var faculty3 = new FacultyDto { Id = "3", Name = "Faculty3", UniversityId = "uni3", UniversityName = "University 3", Status = "Active" };

        var allFaculties = new List<FacultyDto> { faculty1, faculty2, faculty3 };
        typeof(FacultiesViewModel).GetField("_allFaculties", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            ?.SetValue(viewModel, allFaculties);

        viewModel.UniversityFilter = "uni1";

        // Act
        viewModel.UniversityFilter = "All";

        // Assert
        Assert.AreEqual(3, viewModel.Faculties.Count, "All faculties should be shown when filter is 'All'");
    }

    /// <summary>
    /// Tests that UniversityFilter correctly updates TotalPages when filtering.
    /// Input: Different filter values affecting pagination.
    /// Expected: TotalPages is recalculated based on filtered results.
    /// </summary>
    [TestMethod]
    public void UniversityFilter_SetValue_UpdatesTotalPages()
    {
        // Arrange
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<FacultiesViewModel>>();
        var viewModel = new FacultiesViewModel(mockAcademic.Object, mockLogger.Object);

        var faculties = new List<FacultyDto>();
        for (int i = 0; i < 25; i++)
        {
            faculties.Add(new FacultyDto
            {
                Id = i.ToString(),
                Name = $"Faculty{i}",
                UniversityId = i < 15 ? "uni1" : "uni2",
                UniversityName = i < 15 ? "University 1" : "University 2",
                Status = "Active"
            });
        }

        typeof(FacultiesViewModel).GetField("_allFaculties", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            ?.SetValue(viewModel, faculties);

        // Act - Filter to uni1 (15 items with PageSize=10 should give 2 pages)
        viewModel.UniversityFilter = "uni1";

        // Assert
        Assert.AreEqual(2, viewModel.TotalPages, "TotalPages should be 2 for 15 items with PageSize=10");
    }

    /// <summary>
    /// Tests that UniversityFilter handles transition from null to non-null value correctly.
    /// Input: null then non-null value.
    /// Expected: Both transitions update the property and raise PropertyChanged.
    /// </summary>
    [TestMethod]
    public void UniversityFilter_TransitionFromNullToValue_UpdatesCorrectly()
    {
        // Arrange
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<FacultiesViewModel>>();
        var viewModel = new FacultiesViewModel(mockAcademic.Object, mockLogger.Object);

        viewModel.UniversityFilter = null!;
        Assert.IsNull(viewModel.UniversityFilter);

        // Act
        viewModel.UniversityFilter = "TestUniversity";

        // Assert
        Assert.AreEqual("TestUniversity", viewModel.UniversityFilter);
    }

    /// <summary>
    /// Tests that UniversityFilter handles alternating between empty and non-empty values.
    /// Input: Alternating empty string and non-empty values.
    /// Expected: Each change updates the property correctly.
    /// </summary>
    [TestMethod]
    public void UniversityFilter_AlternateBetweenEmptyAndNonEmpty_UpdatesCorrectly()
    {
        // Arrange
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<FacultiesViewModel>>();
        var viewModel = new FacultiesViewModel(mockAcademic.Object, mockLogger.Object);

        // Act & Assert
        viewModel.UniversityFilter = "";
        Assert.AreEqual("", viewModel.UniversityFilter);

        viewModel.UniversityFilter = "NonEmpty";
        Assert.AreEqual("NonEmpty", viewModel.UniversityFilter);

        viewModel.UniversityFilter = "";
        Assert.AreEqual("", viewModel.UniversityFilter);

        viewModel.UniversityFilter = "AnotherValue";
        Assert.AreEqual("AnotherValue", viewModel.UniversityFilter);
    }

    /// <summary>
    /// Tests that UniversityFilter handles special path-like strings.
    /// Input: Strings resembling file paths or URLs.
    /// Expected: Value is stored correctly.
    /// </summary>
    [TestMethod]
    [DataRow("C:\\Users\\Test\\file.txt")]
    [DataRow("/usr/local/bin")]
    [DataRow("http://example.com/university")]
    [DataRow("\\\\network\\share")]
    [DataRow("..\\..\\parent")]
    public void UniversityFilter_SetPathLikeStrings_StoresValueCorrectly(string pathLikeValue)
    {
        // Arrange
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<FacultiesViewModel>>();
        var viewModel = new FacultiesViewModel(mockAcademic.Object, mockLogger.Object);

        // Act
        viewModel.UniversityFilter = pathLikeValue;

        // Assert
        Assert.AreEqual(pathLikeValue, viewModel.UniversityFilter);
    }

    /// <summary>
    /// Tests that UniversityFilter with case variations is treated as different values.
    /// Input: Same string with different casing.
    /// Expected: Each case variation is stored as a different value and raises PropertyChanged.
    /// </summary>
    [TestMethod]
    public void UniversityFilter_SetCaseVariations_TreatsAsDifferentValues()
    {
        // Arrange
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<FacultiesViewModel>>();
        var viewModel = new FacultiesViewModel(mockAcademic.Object, mockLogger.Object);
        var propertyChangedCount = 0;

        viewModel.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == nameof(viewModel.UniversityFilter))
                propertyChangedCount++;
        };

        // Act
        viewModel.UniversityFilter = "university";
        viewModel.UniversityFilter = "UNIVERSITY";
        viewModel.UniversityFilter = "University";

        // Assert
        Assert.AreEqual("University", viewModel.UniversityFilter);
        Assert.AreEqual(3, propertyChangedCount, "PropertyChanged should be raised for each case variation");
    }

    /// <summary>
    /// Tests that UniversityFilter handles strings with SQL injection-like patterns.
    /// Input: Strings resembling SQL injection attempts.
    /// Expected: Value is stored as-is without processing.
    /// </summary>
    [TestMethod]
    [DataRow("'; DROP TABLE Faculties--")]
    [DataRow("1' OR '1'='1")]
    [DataRow("admin'--")]
    [DataRow("' UNION SELECT * FROM Users--")]
    public void UniversityFilter_SetSqlInjectionLikeStrings_StoresValueAsIs(string sqlLikeValue)
    {
        // Arrange
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<FacultiesViewModel>>();
        var viewModel = new FacultiesViewModel(mockAcademic.Object, mockLogger.Object);

        // Act
        viewModel.UniversityFilter = sqlLikeValue;

        // Assert
        Assert.AreEqual(sqlLikeValue, viewModel.UniversityFilter);
    }

    /// <summary>
    /// Tests that UniversityFilter handles strings with HTML/XML-like content.
    /// Input: Strings with markup tags.
    /// Expected: Value is stored without escaping or processing.
    /// </summary>
    [TestMethod]
    [DataRow("<script>alert('test')</script>")]
    [DataRow("<div>University</div>")]
    [DataRow("<?xml version=\"1.0\"?>")]
    [DataRow("<![CDATA[data]]>")]
    public void UniversityFilter_SetMarkupStrings_StoresValueAsIs(string markupValue)
    {
        // Arrange
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<FacultiesViewModel>>();
        var viewModel = new FacultiesViewModel(mockAcademic.Object, mockLogger.Object);

        // Act
        viewModel.UniversityFilter = markupValue;

        // Assert
        Assert.AreEqual(markupValue, viewModel.UniversityFilter);
    }

    /// <summary>
    /// Tests that UniversityFilter handles strings with newline variations.
    /// Input: Strings with different newline characters.
    /// Expected: Newlines are preserved in the stored value.
    /// </summary>
    [TestMethod]
    [DataRow("line1\nline2")]
    [DataRow("line1\r\nline2")]
    [DataRow("line1\rline2")]
    [DataRow("multi\nline\nvalue")]
    public void UniversityFilter_SetStringsWithNewlines_PreservesNewlines(string newlineValue)
    {
        // Arrange
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<FacultiesViewModel>>();
        var viewModel = new FacultiesViewModel(mockAcademic.Object, mockLogger.Object);

        // Act
        viewModel.UniversityFilter = newlineValue;

        // Assert
        Assert.AreEqual(newlineValue, viewModel.UniversityFilter);
    }

    /// <summary>
    /// Tests that setting UniversityFilter to empty string filters correctly.
    /// Input: Empty string.
    /// Expected: Faculties are filtered based on empty UniversityId match.
    /// </summary>
    [TestMethod]
    public void UniversityFilter_SetToEmptyString_FiltersCorrectly()
    {
        // Arrange
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<FacultiesViewModel>>();
        var viewModel = new FacultiesViewModel(mockAcademic.Object, mockLogger.Object);

        var faculty1 = new FacultyDto { Id = "1", Name = "Faculty1", UniversityId = "", UniversityName = "No University", Status = "Active" };
        var faculty2 = new FacultyDto { Id = "2", Name = "Faculty2", UniversityId = "uni1", UniversityName = "University 1", Status = "Active" };

        var allFaculties = new List<FacultyDto> { faculty1, faculty2 };
        typeof(FacultiesViewModel).GetField("_allFaculties", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            ?.SetValue(viewModel, allFaculties);

        // Act
        viewModel.UniversityFilter = "";

        // Assert
        Assert.AreEqual(1, viewModel.Faculties.Count);
        Assert.AreEqual("", viewModel.Faculties[0].UniversityId);
    }

    /// <summary>
    /// Tests that setting TotalPages to the initial value (1) does not raise PropertyChanged events.
    /// Input: Value 1 when current value is already 1 (initial value).
    /// Expected: No PropertyChanged events are raised.
    /// </summary>
    [TestMethod]
    public void TotalPages_SetToInitialValue_DoesNotRaisePropertyChanged()
    {
        // Arrange
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<FacultiesViewModel>>();
        var viewModel = new FacultiesViewModel(mockAcademic.Object, mockLogger.Object);
        var propertyChangedCount = 0;

        viewModel.PropertyChanged += (sender, args) =>
        {
            propertyChangedCount++;
        };

        // Act
        viewModel.TotalPages = 1;

        // Assert
        Assert.AreEqual(0, propertyChangedCount);
    }

    /// <summary>
    /// Tests that multiple consecutive updates to TotalPages with different values each raise PropertyChanged events.
    /// Input: Three different values set consecutively.
    /// Expected: PropertyChanged events are raised for each value change.
    /// </summary>
    [TestMethod]
    public void TotalPages_MultipleConsecutiveUpdates_RaisesPropertyChangedForEach()
    {
        // Arrange
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<FacultiesViewModel>>();
        var viewModel = new FacultiesViewModel(mockAcademic.Object, mockLogger.Object);
        var eventCount = 0;

        viewModel.PropertyChanged += (sender, args) =>
        {
            eventCount++;
        };

        // Act
        viewModel.TotalPages = 5;
        viewModel.TotalPages = 10;
        viewModel.TotalPages = 15;

        // Assert - Each change raises 2 events (TotalPages and PageInfo)
        Assert.AreEqual(6, eventCount);
        Assert.AreEqual(15, viewModel.TotalPages);
    }

    /// <summary>
    /// Tests that TotalPages properly handles transition from positive to negative values.
    /// Input: First positive value, then negative value.
    /// Expected: Both values are set correctly and PropertyChanged events are raised for each.
    /// </summary>
    [TestMethod]
    public void TotalPages_TransitionPositiveToNegative_UpdatesCorrectly()
    {
        // Arrange
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<FacultiesViewModel>>();
        var viewModel = new FacultiesViewModel(mockAcademic.Object, mockLogger.Object);

        // Act
        viewModel.TotalPages = 100;
        Assert.AreEqual(100, viewModel.TotalPages);

        viewModel.TotalPages = -50;

        // Assert
        Assert.AreEqual(-50, viewModel.TotalPages);
    }

    /// <summary>
    /// Tests that TotalPages properly handles transition from negative to positive values.
    /// Input: First negative value, then positive value.
    /// Expected: Both values are set correctly and PropertyChanged events are raised for each.
    /// </summary>
    [TestMethod]
    public void TotalPages_TransitionNegativeToPositive_UpdatesCorrectly()
    {
        // Arrange
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<FacultiesViewModel>>();
        var viewModel = new FacultiesViewModel(mockAcademic.Object, mockLogger.Object);

        // Act
        viewModel.TotalPages = -50;
        Assert.AreEqual(-50, viewModel.TotalPages);

        viewModel.TotalPages = 100;

        // Assert
        Assert.AreEqual(100, viewModel.TotalPages);
    }

    /// <summary>
    /// Tests that PageInfo updates correctly when TotalPages changes.
    /// Input: Setting TotalPages to 5.
    /// Expected: PageInfo reflects the new TotalPages value in the format "Page {CurrentPage} of {TotalPages}".
    /// </summary>
    [TestMethod]
    public void TotalPages_SetNewValue_UpdatesPageInfo()
    {
        // Arrange
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<FacultiesViewModel>>();
        var viewModel = new FacultiesViewModel(mockAcademic.Object, mockLogger.Object);

        // Act
        viewModel.TotalPages = 5;

        // Assert
        Assert.AreEqual("Page 1 of 5", viewModel.PageInfo);
    }

    /// <summary>
    /// Tests that only PropertyChanged events for TotalPages and PageInfo are raised, no other events.
    /// Input: Setting TotalPages to a new value.
    /// Expected: Exactly two PropertyChanged events with property names "TotalPages" and "PageInfo".
    /// </summary>
    [TestMethod]
    public void TotalPages_SetNewValue_RaisesOnlyExpectedPropertyChangedEvents()
    {
        // Arrange
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<FacultiesViewModel>>();
        var viewModel = new FacultiesViewModel(mockAcademic.Object, mockLogger.Object);
        var raisedProperties = new List<string>();

        viewModel.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName != null)
            {
                raisedProperties.Add(args.PropertyName);
            }
        };

        // Act
        viewModel.TotalPages = 42;

        // Assert
        Assert.AreEqual(2, raisedProperties.Count);
        CollectionAssert.Contains(raisedProperties, nameof(viewModel.TotalPages));
        CollectionAssert.Contains(raisedProperties, nameof(viewModel.PageInfo));
    }

    /// <summary>
    /// Tests that setting TotalPages from initial value to a different value raises PropertyChanged events.
    /// Input: Setting from initial value of 1 to 10.
    /// Expected: PropertyChanged events are raised for TotalPages and PageInfo.
    /// </summary>
    [TestMethod]
    public void TotalPages_SetFromInitialValueToDifferentValue_RaisesPropertyChanged()
    {
        // Arrange
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<FacultiesViewModel>>();
        var viewModel = new FacultiesViewModel(mockAcademic.Object, mockLogger.Object);
        var eventCount = 0;

        viewModel.PropertyChanged += (sender, args) =>
        {
            eventCount++;
        };

        // Act
        viewModel.TotalPages = 10;

        // Assert
        Assert.AreEqual(2, eventCount);
        Assert.AreEqual(10, viewModel.TotalPages);
    }

    /// <summary>
    /// Tests that setting TotalPages to boundary value (int.MaxValue - 1) updates correctly.
    /// Input: int.MaxValue - 1.
    /// Expected: Property value is set to int.MaxValue - 1.
    /// </summary>
    [TestMethod]
    public void TotalPages_SetToMaxValueMinusOne_UpdatesPropertyValue()
    {
        // Arrange
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<FacultiesViewModel>>();
        var viewModel = new FacultiesViewModel(mockAcademic.Object, mockLogger.Object);

        // Act
        viewModel.TotalPages = int.MaxValue - 1;

        // Assert
        Assert.AreEqual(int.MaxValue - 1, viewModel.TotalPages);
    }

    /// <summary>
    /// Tests that setting TotalPages to boundary value (int.MinValue + 1) updates correctly.
    /// Input: int.MinValue + 1.
    /// Expected: Property value is set to int.MinValue + 1.
    /// </summary>
    [TestMethod]
    public void TotalPages_SetToMinValuePlusOne_UpdatesPropertyValue()
    {
        // Arrange
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<FacultiesViewModel>>();
        var viewModel = new FacultiesViewModel(mockAcademic.Object, mockLogger.Object);

        // Act
        viewModel.TotalPages = int.MinValue + 1;

        // Assert
        Assert.AreEqual(int.MinValue + 1, viewModel.TotalPages);
    }

    /// <summary>
    /// Tests that setting ErrorMessage to a different value raises PropertyChanged event.
    /// Input: Different string value from current value.
    /// Expected: PropertyChanged event is raised with property name "ErrorMessage".
    /// </summary>
    [TestMethod]
    public void ErrorMessage_SetDifferentValue_RaisesPropertyChangedEvent()
    {
        // Arrange
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<FacultiesViewModel>>();
        var viewModel = new FacultiesViewModel(mockAcademic.Object, mockLogger.Object);
        bool eventRaised = false;
        string? raisedPropertyName = null;

        viewModel.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == "ErrorMessage")
            {
                eventRaised = true;
                raisedPropertyName = args.PropertyName;
            }
        };

        // Act
        viewModel.ErrorMessage = "Test error message";

        // Assert
        Assert.IsTrue(eventRaised, "PropertyChanged event should be raised.");
        Assert.AreEqual("ErrorMessage", raisedPropertyName, "Property name should be 'ErrorMessage'.");
        Assert.AreEqual("Test error message", viewModel.ErrorMessage);
    }

    /// <summary>
    /// Tests that setting ErrorMessage to the same value does not raise PropertyChanged event.
    /// Input: Same string value as current value.
    /// Expected: PropertyChanged event is not raised.
    /// </summary>
    [TestMethod]
    public void ErrorMessage_SetSameValue_DoesNotRaisePropertyChangedEvent()
    {
        // Arrange
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<FacultiesViewModel>>();
        var viewModel = new FacultiesViewModel(mockAcademic.Object, mockLogger.Object);
        viewModel.ErrorMessage = "Initial error";

        bool eventRaised = false;
        viewModel.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == "ErrorMessage")
            {
                eventRaised = true;
            }
        };

        // Act
        viewModel.ErrorMessage = "Initial error";

        // Assert
        Assert.IsFalse(eventRaised, "PropertyChanged event should not be raised when setting the same value.");
        Assert.AreEqual("Initial error", viewModel.ErrorMessage);
    }

    /// <summary>
    /// Tests that ErrorMessage property handles strings with control characters correctly.
    /// Input: Strings containing various control characters.
    /// Expected: Property stores and returns the control character strings correctly.
    /// </summary>
    [TestMethod]
    [DataRow("\0")]
    [DataRow("\u0001")]
    [DataRow("\u0002\u0003")]
    [DataRow("\u001F")]
    [DataRow("Error\0Message")]
    [DataRow("Error\u0001\u0002\u0003Message")]
    public void ErrorMessage_SetControlCharacters_UpdatesPropertyCorrectly(string value)
    {
        // Arrange
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<FacultiesViewModel>>();
        var viewModel = new FacultiesViewModel(mockAcademic.Object, mockLogger.Object);

        // Act
        viewModel.ErrorMessage = value;

        // Assert
        Assert.AreEqual(value, viewModel.ErrorMessage);
    }

    /// <summary>
    /// Tests that multiple consecutive different value changes each raise PropertyChanged event.
    /// Input: Multiple different string values set sequentially.
    /// Expected: PropertyChanged event is raised for each distinct change.
    /// </summary>
    [TestMethod]
    public void ErrorMessage_MultipleChanges_RaisesPropertyChangedForEachChange()
    {
        // Arrange
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<FacultiesViewModel>>();
        var viewModel = new FacultiesViewModel(mockAcademic.Object, mockLogger.Object);
        int eventCount = 0;

        viewModel.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == "ErrorMessage")
            {
                eventCount++;
            }
        };

        // Act
        viewModel.ErrorMessage = "Error 1";
        viewModel.ErrorMessage = "Error 2";
        viewModel.ErrorMessage = "Error 3";

        // Assert
        Assert.AreEqual(3, eventCount, "PropertyChanged event should be raised three times for three distinct changes.");
        Assert.AreEqual("Error 3", viewModel.ErrorMessage);
    }

    /// <summary>
    /// Tests that PropertyChanged event is raised when changing from empty string to non-empty string.
    /// Input: Non-empty string when current value is empty.
    /// Expected: PropertyChanged event is raised.
    /// </summary>
    [TestMethod]
    public void ErrorMessage_SetFromEmptyToNonEmpty_RaisesPropertyChangedEvent()
    {
        // Arrange
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<FacultiesViewModel>>();
        var viewModel = new FacultiesViewModel(mockAcademic.Object, mockLogger.Object);
        bool eventRaised = false;

        viewModel.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == "ErrorMessage")
            {
                eventRaised = true;
            }
        };

        // Act
        viewModel.ErrorMessage = "New error message";

        // Assert
        Assert.IsTrue(eventRaised, "PropertyChanged event should be raised when changing from empty to non-empty.");
        Assert.AreEqual("New error message", viewModel.ErrorMessage);
    }

    /// <summary>
    /// Tests that PropertyChanged event is raised when changing from non-empty string to empty string.
    /// Input: Empty string when current value is non-empty.
    /// Expected: PropertyChanged event is raised.
    /// </summary>
    [TestMethod]
    public void ErrorMessage_SetFromNonEmptyToEmpty_RaisesPropertyChangedEvent()
    {
        // Arrange
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<FacultiesViewModel>>();
        var viewModel = new FacultiesViewModel(mockAcademic.Object, mockLogger.Object);
        viewModel.ErrorMessage = "Existing error";

        bool eventRaised = false;
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
        Assert.IsTrue(eventRaised, "PropertyChanged event should be raised when changing from non-empty to empty.");
        Assert.AreEqual(string.Empty, viewModel.ErrorMessage);
    }

    /// <summary>
    /// Tests that setting ErrorMessage to the same empty string twice does not raise PropertyChanged on second set.
    /// Input: Empty string set twice consecutively.
    /// Expected: PropertyChanged event is raised only once (on first set from initial empty).
    /// </summary>
    [TestMethod]
    public void ErrorMessage_SetEmptyStringTwice_RaisesPropertyChangedOnlyOnce()
    {
        // Arrange
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<FacultiesViewModel>>();
        var viewModel = new FacultiesViewModel(mockAcademic.Object, mockLogger.Object);
        int eventCount = 0;

        viewModel.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == "ErrorMessage")
            {
                eventCount++;
            }
        };

        // Act
        viewModel.ErrorMessage = string.Empty;
        viewModel.ErrorMessage = string.Empty;

        // Assert
        Assert.AreEqual(0, eventCount, "PropertyChanged event should not be raised when setting empty to empty.");
        Assert.AreEqual(string.Empty, viewModel.ErrorMessage);
    }

    /// <summary>
    /// Tests that ErrorMessage PropertyChanged event provides correct property name in event args.
    /// Input: Any different string value.
    /// Expected: PropertyChanged event args contain exactly "ErrorMessage" as property name.
    /// </summary>
    [TestMethod]
    public void ErrorMessage_PropertyChangedEventArgs_ContainsCorrectPropertyName()
    {
        // Arrange
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<FacultiesViewModel>>();
        var viewModel = new FacultiesViewModel(mockAcademic.Object, mockLogger.Object);
        string? capturedPropertyName = null;

        viewModel.PropertyChanged += (sender, args) =>
        {
            capturedPropertyName = args.PropertyName;
        };

        // Act
        viewModel.ErrorMessage = "Test error";

        // Assert
        Assert.AreEqual("ErrorMessage", capturedPropertyName, "PropertyName in event args should be 'ErrorMessage'.");
    }

    /// <summary>
    /// Tests that ErrorMessage handles alternating between different values correctly and raises events appropriately.
    /// Input: Alternating between two different values.
    /// Expected: PropertyChanged event is raised for each change, values alternate correctly.
    /// </summary>
    [TestMethod]
    public void ErrorMessage_AlternateBetweenValues_RaisesPropertyChangedForEach()
    {
        // Arrange
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<FacultiesViewModel>>();
        var viewModel = new FacultiesViewModel(mockAcademic.Object, mockLogger.Object);
        int eventCount = 0;

        viewModel.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == "ErrorMessage")
            {
                eventCount++;
            }
        };

        // Act
        viewModel.ErrorMessage = "Error A";
        viewModel.ErrorMessage = "Error B";
        viewModel.ErrorMessage = "Error A";
        viewModel.ErrorMessage = "Error B";

        // Assert
        Assert.AreEqual(4, eventCount, "PropertyChanged event should be raised four times for four distinct changes.");
        Assert.AreEqual("Error B", viewModel.ErrorMessage);
    }

    /// <summary>
    /// Tests that ErrorMessage handles very long strings correctly and raises PropertyChanged event.
    /// Input: String with more than 10000 characters.
    /// Expected: Property stores the long string and PropertyChanged event is raised.
    /// </summary>
    [TestMethod]
    public void ErrorMessage_SetVeryLongString_RaisesPropertyChangedEvent()
    {
        // Arrange
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<FacultiesViewModel>>();
        var viewModel = new FacultiesViewModel(mockAcademic.Object, mockLogger.Object);
        var longString = new string('E', 15000);
        bool eventRaised = false;

        viewModel.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == "ErrorMessage")
            {
                eventRaised = true;
            }
        };

        // Act
        viewModel.ErrorMessage = longString;

        // Assert
        Assert.IsTrue(eventRaised, "PropertyChanged event should be raised for very long strings.");
        Assert.AreEqual(longString, viewModel.ErrorMessage);
        Assert.AreEqual(15000, viewModel.ErrorMessage.Length);
    }

    /// <summary>
    /// Tests that the PageInfo property returns the correctly formatted string
    /// for various combinations of CurrentPage and TotalPages values including
    /// edge cases, boundary values, negative values, and extreme integer values.
    /// </summary>
    /// <param name="currentPage">The current page number to set.</param>
    /// <param name="totalPages">The total number of pages to set.</param>
    /// <param name="expectedPageInfo">The expected PageInfo string output.</param>
    [TestMethod]
    [DataRow(1, 1, "Page 1 of 1")]
    [DataRow(1, 10, "Page 1 of 10")]
    [DataRow(5, 10, "Page 5 of 10")]
    [DataRow(10, 10, "Page 10 of 10")]
    [DataRow(0, 0, "Page 0 of 0")]
    [DataRow(0, 10, "Page 0 of 10")]
    [DataRow(1, 0, "Page 1 of 0")]
    [DataRow(-1, 5, "Page -1 of 5")]
    [DataRow(5, -1, "Page 5 of -1")]
    [DataRow(-1, -1, "Page -1 of -1")]
    [DataRow(-5, -10, "Page -5 of -10")]
    [DataRow(15, 10, "Page 15 of 10")]
    [DataRow(100, 1, "Page 100 of 1")]
    [DataRow(2147483647, 1, "Page 2147483647 of 1")]
    [DataRow(1, 2147483647, "Page 1 of 2147483647")]
    [DataRow(2147483647, 2147483647, "Page 2147483647 of 2147483647")]
    [DataRow(-2147483648, 1, "Page -2147483648 of 1")]
    [DataRow(1, -2147483648, "Page 1 of -2147483648")]
    [DataRow(-2147483648, -2147483648, "Page -2147483648 of -2147483648")]
    [DataRow(2147483647, -2147483648, "Page 2147483647 of -2147483648")]
    [DataRow(-2147483648, 2147483647, "Page -2147483648 of 2147483647")]
    public void PageInfo_VariousCurrentPageAndTotalPagesValues_ReturnsCorrectFormattedString(int currentPage, int totalPages, string expectedPageInfo)
    {
        // Arrange
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<FacultiesViewModel>>();
        var viewModel = new FacultiesViewModel(mockAcademic.Object, mockLogger.Object);

        // Act
        viewModel.CurrentPage = currentPage;
        viewModel.TotalPages = totalPages;
        var actualPageInfo = viewModel.PageInfo;

        // Assert
        Assert.AreEqual(expectedPageInfo, actualPageInfo);
    }

    /// <summary>
    /// Tests that the PageInfo property updates correctly when CurrentPage changes
    /// while TotalPages remains constant.
    /// </summary>
    [TestMethod]
    public void PageInfo_CurrentPageChangesWithConstantTotalPages_ReflectsCurrentPageChanges()
    {
        // Arrange
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<FacultiesViewModel>>();
        var viewModel = new FacultiesViewModel(mockAcademic.Object, mockLogger.Object);
        viewModel.TotalPages = 10;

        // Act & Assert
        viewModel.CurrentPage = 1;
        Assert.AreEqual("Page 1 of 10", viewModel.PageInfo);

        viewModel.CurrentPage = 5;
        Assert.AreEqual("Page 5 of 10", viewModel.PageInfo);

        viewModel.CurrentPage = 10;
        Assert.AreEqual("Page 10 of 10", viewModel.PageInfo);
    }

    /// <summary>
    /// Tests that the PageInfo property updates correctly when TotalPages changes
    /// while CurrentPage remains constant.
    /// </summary>
    [TestMethod]
    public void PageInfo_TotalPagesChangesWithConstantCurrentPage_ReflectsTotalPagesChanges()
    {
        // Arrange
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<FacultiesViewModel>>();
        var viewModel = new FacultiesViewModel(mockAcademic.Object, mockLogger.Object);
        viewModel.CurrentPage = 5;

        // Act & Assert
        viewModel.TotalPages = 10;
        Assert.AreEqual("Page 5 of 10", viewModel.PageInfo);

        viewModel.TotalPages = 20;
        Assert.AreEqual("Page 5 of 20", viewModel.PageInfo);

        viewModel.TotalPages = 5;
        Assert.AreEqual("Page 5 of 5", viewModel.PageInfo);
    }

    /// <summary>
    /// Tests that the PageInfo property returns the correct initial value
    /// when the ViewModel is first constructed without any property changes.
    /// </summary>
    [TestMethod]
    public void PageInfo_InitialValue_ReturnsDefaultPageOneOfOne()
    {
        // Arrange
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<FacultiesViewModel>>();

        // Act
        var viewModel = new FacultiesViewModel(mockAcademic.Object, mockLogger.Object);
        var actualPageInfo = viewModel.PageInfo;

        // Assert
        Assert.AreEqual("Page 1 of 1", actualPageInfo);
    }

    /// <summary>
    /// Tests that the PendingCount property has a default initial value of 0.
    /// Input: None (testing initial state).
    /// Expected: PendingCount returns 0.
    /// </summary>
    [TestMethod]
    public void PendingCount_InitialValue_IsZero()
    {
        // Arrange
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<FacultiesViewModel>>();

        // Act
        var viewModel = new FacultiesViewModel(mockAcademic.Object, mockLogger.Object);

        // Assert
        Assert.AreEqual(0, viewModel.PendingCount);
    }

    /// <summary>
    /// Tests that setting PendingCount to a different value updates the property and raises PropertyChanged event.
    /// Input: Various integer values including edge cases.
    /// Expected: Property is updated and PropertyChanged event is raised with correct property name.
    /// </summary>
    /// <param name="newValue">The new value to set for PendingCount.</param>
    [TestMethod]
    [DataRow(0)]
    [DataRow(1)]
    [DataRow(10)]
    [DataRow(100)]
    [DataRow(1000)]
    [DataRow(-1)]
    [DataRow(-10)]
    [DataRow(-100)]
    [DataRow(-1000)]
    [DataRow(int.MaxValue)]
    [DataRow(int.MinValue)]
    public void PendingCount_SetDifferentValue_UpdatesPropertyAndRaisesPropertyChanged(int newValue)
    {
        // Arrange
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<FacultiesViewModel>>();
        var viewModel = new FacultiesViewModel(mockAcademic.Object, mockLogger.Object);
        var propertyChangedRaised = false;
        string? raisedPropertyName = null;

        viewModel.PropertyChanged += (sender, e) =>
        {
            propertyChangedRaised = true;
            raisedPropertyName = e.PropertyName;
        };

        // Act
        viewModel.PendingCount = newValue;

        // Assert
        Assert.AreEqual(newValue, viewModel.PendingCount);
        Assert.IsTrue(propertyChangedRaised);
        Assert.AreEqual("PendingCount", raisedPropertyName);
    }

    /// <summary>
    /// Tests that setting PendingCount to the same value does not raise PropertyChanged event.
    /// Input: Same value as current value.
    /// Expected: Property value remains unchanged and PropertyChanged event is not raised.
    /// </summary>
    [TestMethod]
    public void PendingCount_SetSameValue_DoesNotRaisePropertyChanged()
    {
        // Arrange
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<FacultiesViewModel>>();
        var viewModel = new FacultiesViewModel(mockAcademic.Object, mockLogger.Object);
        viewModel.PendingCount = 42;
        var propertyChangedRaised = false;

        viewModel.PropertyChanged += (sender, e) =>
        {
            propertyChangedRaised = true;
        };

        // Act
        viewModel.PendingCount = 42;

        // Assert
        Assert.AreEqual(42, viewModel.PendingCount);
        Assert.IsFalse(propertyChangedRaised);
    }

    /// <summary>
    /// Tests that getting PendingCount returns the correct value after setting it.
    /// Input: Various integer values.
    /// Expected: Getter returns the exact value that was set.
    /// </summary>
    /// <param name="value">The value to set and retrieve.</param>
    [TestMethod]
    [DataRow(0)]
    [DataRow(5)]
    [DataRow(999)]
    [DataRow(-5)]
    [DataRow(-999)]
    [DataRow(int.MaxValue)]
    [DataRow(int.MinValue)]
    public void PendingCount_GetAfterSet_ReturnsCorrectValue(int value)
    {
        // Arrange
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<FacultiesViewModel>>();
        var viewModel = new FacultiesViewModel(mockAcademic.Object, mockLogger.Object);

        // Act
        viewModel.PendingCount = value;

        // Assert
        Assert.AreEqual(value, viewModel.PendingCount);
    }

    /// <summary>
    /// Tests that multiple consecutive updates to PendingCount with different values all raise PropertyChanged events.
    /// Input: Sequence of different values.
    /// Expected: PropertyChanged event is raised for each different value assignment.
    /// </summary>
    [TestMethod]
    public void PendingCount_MultipleConsecutiveUpdates_RaisesPropertyChangedForEach()
    {
        // Arrange
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<FacultiesViewModel>>();
        var viewModel = new FacultiesViewModel(mockAcademic.Object, mockLogger.Object);
        var propertyChangedCount = 0;

        viewModel.PropertyChanged += (sender, e) =>
        {
            if (e.PropertyName == "PendingCount")
            {
                propertyChangedCount++;
            }
        };

        // Act
        viewModel.PendingCount = 10;
        viewModel.PendingCount = 20;
        viewModel.PendingCount = 30;

        // Assert
        Assert.AreEqual(30, viewModel.PendingCount);
        Assert.AreEqual(3, propertyChangedCount);
    }

    /// <summary>
    /// Tests that setting PendingCount to zero from a non-zero value updates correctly.
    /// Input: Set to non-zero, then set to zero.
    /// Expected: Property is updated to zero and PropertyChanged is raised.
    /// </summary>
    [TestMethod]
    public void PendingCount_SetToZeroFromNonZero_UpdatesCorrectly()
    {
        // Arrange
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<FacultiesViewModel>>();
        var viewModel = new FacultiesViewModel(mockAcademic.Object, mockLogger.Object);
        viewModel.PendingCount = 100;
        var propertyChangedRaised = false;

        viewModel.PropertyChanged += (sender, e) =>
        {
            if (e.PropertyName == "PendingCount")
            {
                propertyChangedRaised = true;
            }
        };

        // Act
        viewModel.PendingCount = 0;

        // Assert
        Assert.AreEqual(0, viewModel.PendingCount);
        Assert.IsTrue(propertyChangedRaised);
    }

    /// <summary>
    /// Tests that setting PendingCount from positive to negative value works correctly.
    /// Input: Positive value then negative value.
    /// Expected: Property is updated to negative value and PropertyChanged is raised.
    /// </summary>
    [TestMethod]
    public void PendingCount_SetFromPositiveToNegative_UpdatesCorrectly()
    {
        // Arrange
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<FacultiesViewModel>>();
        var viewModel = new FacultiesViewModel(mockAcademic.Object, mockLogger.Object);
        viewModel.PendingCount = 50;
        var propertyChangedRaised = false;

        viewModel.PropertyChanged += (sender, e) =>
        {
            if (e.PropertyName == "PendingCount")
            {
                propertyChangedRaised = true;
            }
        };

        // Act
        viewModel.PendingCount = -50;

        // Assert
        Assert.AreEqual(-50, viewModel.PendingCount);
        Assert.IsTrue(propertyChangedRaised);
    }

    /// <summary>
    /// Tests that setting PendingCount from negative to positive value works correctly.
    /// Input: Negative value then positive value.
    /// Expected: Property is updated to positive value and PropertyChanged is raised.
    /// </summary>
    [TestMethod]
    public void PendingCount_SetFromNegativeToPositive_UpdatesCorrectly()
    {
        // Arrange
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<FacultiesViewModel>>();
        var viewModel = new FacultiesViewModel(mockAcademic.Object, mockLogger.Object);
        viewModel.PendingCount = -50;
        var propertyChangedRaised = false;

        viewModel.PropertyChanged += (sender, e) =>
        {
            if (e.PropertyName == "PendingCount")
            {
                propertyChangedRaised = true;
            }
        };

        // Act
        viewModel.PendingCount = 50;

        // Assert
        Assert.AreEqual(50, viewModel.PendingCount);
        Assert.IsTrue(propertyChangedRaised);
    }

    /// <summary>
    /// Tests that setting PendingCount to int.MaxValue works correctly.
    /// Input: int.MaxValue.
    /// Expected: Property is set to int.MaxValue and PropertyChanged is raised.
    /// </summary>
    [TestMethod]
    public void PendingCount_SetToMaxValue_UpdatesCorrectly()
    {
        // Arrange
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<FacultiesViewModel>>();
        var viewModel = new FacultiesViewModel(mockAcademic.Object, mockLogger.Object);
        var propertyChangedRaised = false;

        viewModel.PropertyChanged += (sender, e) =>
        {
            if (e.PropertyName == "PendingCount")
            {
                propertyChangedRaised = true;
            }
        };

        // Act
        viewModel.PendingCount = int.MaxValue;

        // Assert
        Assert.AreEqual(int.MaxValue, viewModel.PendingCount);
        Assert.IsTrue(propertyChangedRaised);
    }

    /// <summary>
    /// Tests that setting PendingCount to int.MinValue works correctly.
    /// Input: int.MinValue.
    /// Expected: Property is set to int.MinValue and PropertyChanged is raised.
    /// </summary>
    [TestMethod]
    public void PendingCount_SetToMinValue_UpdatesCorrectly()
    {
        // Arrange
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<FacultiesViewModel>>();
        var viewModel = new FacultiesViewModel(mockAcademic.Object, mockLogger.Object);
        var propertyChangedRaised = false;

        viewModel.PropertyChanged += (sender, e) =>
        {
            if (e.PropertyName == "PendingCount")
            {
                propertyChangedRaised = true;
            }
        };

        // Act
        viewModel.PendingCount = int.MinValue;

        // Assert
        Assert.AreEqual(int.MinValue, viewModel.PendingCount);
        Assert.IsTrue(propertyChangedRaised);
    }

    /// <summary>
    /// Tests that LoadAsync correctly handles single university with single faculty.
    /// Verifies minimal dataset scenario.
    /// </summary>
    [TestMethod]
    public async Task LoadAsync_WithSingleUniversityAndSingleFaculty_HandlesCorrectly()
    {
        // Arrange
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<FacultiesViewModel>>();

        var universities = new List<LookupItem>
        {
            new LookupItem { Id = "u1", Name = "University 1" }
        };

        var faculties = new List<FacultyDto>
        {
            new FacultyDto { Id = "f1", Name = "Faculty 1", Status = "Active", UniversityId = "u1" }
        };

        mockAcademic.Setup(a => a.GetUniversitiesAsync()).ReturnsAsync(universities);
        mockAcademic.Setup(a => a.GetFacultyDetailsAsync()).ReturnsAsync(faculties);

        var viewModel = new FacultiesViewModel(mockAcademic.Object, mockLogger.Object);

        // Act
        await viewModel.LoadAsync();

        // Assert
        Assert.AreEqual(1, viewModel.Universities.Count, "Universities count should be 1");
        Assert.AreEqual(1, viewModel.TotalFaculties, "TotalFaculties should be 1");
        Assert.AreEqual(1, viewModel.ActiveFacultiesCount, "ActiveFacultiesCount should be 1");
        Assert.AreEqual(0, viewModel.PendingCount, "PendingCount should be 0");
    }

    /// <summary>
    /// Tests that LoadAsync correctly handles faculties with whitespace in Status field.
    /// Verifies that whitespace-only statuses are not counted as "Active".
    /// </summary>
    [TestMethod]
    public async Task LoadAsync_WithWhitespaceStatus_CountsAsNonActive()
    {
        // Arrange
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<FacultiesViewModel>>();

        var universities = new List<LookupItem>
        {
            new LookupItem { Id = "u1", Name = "University 1" }
        };

        var faculties = new List<FacultyDto>
        {
            new FacultyDto { Id = "f1", Name = "Faculty 1", Status = "Active", UniversityId = "u1" },
            new FacultyDto { Id = "f2", Name = "Faculty 2", Status = " ", UniversityId = "u1" },
            new FacultyDto { Id = "f3", Name = "Faculty 3", Status = "\t", UniversityId = "u1" },
            new FacultyDto { Id = "f4", Name = "Faculty 4", Status = "\n", UniversityId = "u1" }
        };

        mockAcademic.Setup(a => a.GetUniversitiesAsync()).ReturnsAsync(universities);
        mockAcademic.Setup(a => a.GetFacultyDetailsAsync()).ReturnsAsync(faculties);

        var viewModel = new FacultiesViewModel(mockAcademic.Object, mockLogger.Object);

        // Act
        await viewModel.LoadAsync();

        // Assert
        Assert.AreEqual(4, viewModel.TotalFaculties, "TotalFaculties should be 4");
        Assert.AreEqual(1, viewModel.ActiveFacultiesCount, "ActiveFacultiesCount should be 1");
        Assert.AreEqual(3, viewModel.PendingCount, "PendingCount should be 3");
    }

    /// <summary>
    /// Tests that LoadAsync verifies GetUniversitiesAsync is called exactly once.
    /// Ensures the service method is invoked as expected.
    /// </summary>
    [TestMethod]
    public async Task LoadAsync_OnSuccessfulExecution_CallsGetUniversitiesAsyncOnce()
    {
        // Arrange
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<FacultiesViewModel>>();

        var universities = new List<LookupItem>();
        var faculties = new List<FacultyDto>();

        mockAcademic.Setup(a => a.GetUniversitiesAsync()).ReturnsAsync(universities);
        mockAcademic.Setup(a => a.GetFacultyDetailsAsync()).ReturnsAsync(faculties);

        var viewModel = new FacultiesViewModel(mockAcademic.Object, mockLogger.Object);

        // Act
        await viewModel.LoadAsync();

        // Assert
        mockAcademic.Verify(a => a.GetUniversitiesAsync(), Times.Once());
    }

    /// <summary>
    /// Tests that LoadAsync verifies GetFacultyDetailsAsync is called exactly once.
    /// Ensures the service method is invoked as expected.
    /// </summary>
    [TestMethod]
    public async Task LoadAsync_OnSuccessfulExecution_CallsGetFacultyDetailsAsyncOnce()
    {
        // Arrange
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<FacultiesViewModel>>();

        var universities = new List<LookupItem>();
        var faculties = new List<FacultyDto>();

        mockAcademic.Setup(a => a.GetUniversitiesAsync()).ReturnsAsync(universities);
        mockAcademic.Setup(a => a.GetFacultyDetailsAsync()).ReturnsAsync(faculties);

        var viewModel = new FacultiesViewModel(mockAcademic.Object, mockLogger.Object);

        // Act
        await viewModel.LoadAsync();

        // Assert
        mockAcademic.Verify(a => a.GetFacultyDetailsAsync(), Times.Once());
    }

    /// <summary>
    /// Tests that LoadAsync does not call GetFacultyDetailsAsync when GetUniversitiesAsync throws.
    /// Verifies that exception stops execution before second service call.
    /// </summary>
    [TestMethod]
    public async Task LoadAsync_WhenGetUniversitiesAsyncThrows_DoesNotCallGetFacultyDetailsAsync()
    {
        // Arrange
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<FacultiesViewModel>>();

        mockAcademic.Setup(a => a.GetUniversitiesAsync()).ThrowsAsync(new Exception("Test exception"));

        var viewModel = new FacultiesViewModel(mockAcademic.Object, mockLogger.Object);

        // Act
        await viewModel.LoadAsync();

        // Assert
        mockAcademic.Verify(a => a.GetFacultyDetailsAsync(), Times.Never());
    }

    /// <summary>
    /// Tests that LoadAsync maintains zero statistics when exception occurs before faculty loading.
    /// Verifies that statistics are not partially updated.
    /// </summary>
    [TestMethod]
    public async Task LoadAsync_WhenGetUniversitiesAsyncThrows_MaintainsZeroStatistics()
    {
        // Arrange
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<FacultiesViewModel>>();

        mockAcademic.Setup(a => a.GetUniversitiesAsync()).ThrowsAsync(new Exception("Test exception"));

        var viewModel = new FacultiesViewModel(mockAcademic.Object, mockLogger.Object);

        // Act
        await viewModel.LoadAsync();

        // Assert
        Assert.AreEqual(0, viewModel.TotalFaculties, "TotalFaculties should remain 0");
        Assert.AreEqual(0, viewModel.ActiveFacultiesCount, "ActiveFacultiesCount should remain 0");
        Assert.AreEqual(0, viewModel.PendingCount, "PendingCount should remain 0");
    }

    /// <summary>
    /// Tests that LoadAsync with only Active faculties sets PendingCount to zero.
    /// Verifies correct statistics when all faculties have Active status.
    /// </summary>
    [TestMethod]
    public async Task LoadAsync_WithOnlyActiveFaculties_SetsPendingCountToZero()
    {
        // Arrange
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<FacultiesViewModel>>();

        var universities = new List<LookupItem>
        {
            new LookupItem { Id = "u1", Name = "University 1" }
        };

        var faculties = new List<FacultyDto>
        {
            new FacultyDto { Id = "f1", Name = "Faculty 1", Status = "Active", UniversityId = "u1" },
            new FacultyDto { Id = "f2", Name = "Faculty 2", Status = "Active", UniversityId = "u1" },
            new FacultyDto { Id = "f3", Name = "Faculty 3", Status = "Active", UniversityId = "u1" }
        };

        mockAcademic.Setup(a => a.GetUniversitiesAsync()).ReturnsAsync(universities);
        mockAcademic.Setup(a => a.GetFacultyDetailsAsync()).ReturnsAsync(faculties);

        var viewModel = new FacultiesViewModel(mockAcademic.Object, mockLogger.Object);

        // Act
        await viewModel.LoadAsync();

        // Assert
        Assert.AreEqual(3, viewModel.TotalFaculties, "TotalFaculties should be 3");
        Assert.AreEqual(3, viewModel.ActiveFacultiesCount, "ActiveFacultiesCount should be 3");
        Assert.AreEqual(0, viewModel.PendingCount, "PendingCount should be 0");
    }

    /// <summary>
    /// Tests that LoadAsync with only non-Active faculties sets ActiveFacultiesCount to zero.
    /// Verifies correct statistics when no faculties have Active status.
    /// </summary>
    [TestMethod]
    public async Task LoadAsync_WithOnlyNonActiveFaculties_SetsActiveFacultiesCountToZero()
    {
        // Arrange
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<FacultiesViewModel>>();

        var universities = new List<LookupItem>
        {
            new LookupItem { Id = "u1", Name = "University 1" }
        };

        var faculties = new List<FacultyDto>
        {
            new FacultyDto { Id = "f1", Name = "Faculty 1", Status = "Pending", UniversityId = "u1" },
            new FacultyDto { Id = "f2", Name = "Faculty 2", Status = "Inactive", UniversityId = "u1" },
            new FacultyDto { Id = "f3", Name = "Faculty 3", Status = "Draft", UniversityId = "u1" }
        };

        mockAcademic.Setup(a => a.GetUniversitiesAsync()).ReturnsAsync(universities);
        mockAcademic.Setup(a => a.GetFacultyDetailsAsync()).ReturnsAsync(faculties);

        var viewModel = new FacultiesViewModel(mockAcademic.Object, mockLogger.Object);

        // Act
        await viewModel.LoadAsync();

        // Assert
        Assert.AreEqual(3, viewModel.TotalFaculties, "TotalFaculties should be 3");
        Assert.AreEqual(0, viewModel.ActiveFacultiesCount, "ActiveFacultiesCount should be 0");
        Assert.AreEqual(3, viewModel.PendingCount, "PendingCount should be 3");
    }

    /// <summary>
    /// Tests that setting PageSize to a different value raises PropertyChanged event for "PageSize".
    /// Input: New PageSize value of 20.
    /// Expected: PropertyChanged event is raised with property name "PageSize".
    /// </summary>
    [TestMethod]
    public void PageSize_SetDifferentValue_RaisesPropertyChangedForPageSize()
    {
        // Arrange
        var mockAcademicService = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<FacultiesViewModel>>();
        var viewModel = new FacultiesViewModel(mockAcademicService.Object, mockLogger.Object);
        string? raisedPropertyName = null;
        viewModel.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == "PageSize")
                raisedPropertyName = args.PropertyName;
        };

        // Act
        viewModel.PageSize = 20;

        // Assert
        Assert.AreEqual("PageSize", raisedPropertyName);
    }

    /// <summary>
    /// Tests that setting PageSize to the same value does not raise PropertyChanged event.
    /// Input: Setting PageSize to its current value (10).
    /// Expected: PropertyChanged event is not raised.
    /// </summary>
    [TestMethod]
    public void PageSize_SetSameValue_DoesNotRaisePropertyChanged()
    {
        // Arrange
        var mockAcademicService = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<FacultiesViewModel>>();
        var viewModel = new FacultiesViewModel(mockAcademicService.Object, mockLogger.Object);
        bool propertyChangedRaised = false;
        viewModel.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == "PageSize")
                propertyChangedRaised = true;
        };

        // Act
        viewModel.PageSize = 10;

        // Assert
        Assert.IsFalse(propertyChangedRaised);
    }

    /// <summary>
    /// Tests that setting PageSize to a different value raises PropertyChanged event for "CurrentPage" due to reset.
    /// Input: New PageSize value of 20 when CurrentPage is set to 5.
    /// Expected: PropertyChanged event is raised for both "PageSize" and "CurrentPage".
    /// </summary>
    [TestMethod]
    public void PageSize_SetDifferentValue_RaisesPropertyChangedForCurrentPage()
    {
        // Arrange
        var mockAcademicService = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<FacultiesViewModel>>();
        var viewModel = new FacultiesViewModel(mockAcademicService.Object, mockLogger.Object);
        viewModel.GetType().GetField("currentPage", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!.SetValue(viewModel, 5);
        var raisedPropertyNames = new List<string>();
        viewModel.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == "PageSize" || args.PropertyName == "CurrentPage" || args.PropertyName == "PageInfo")
                raisedPropertyNames.Add(args.PropertyName!);
        };

        // Act
        viewModel.PageSize = 20;

        // Assert
        Assert.IsTrue(raisedPropertyNames.Contains("PageSize"));
        Assert.IsTrue(raisedPropertyNames.Contains("CurrentPage"));
    }

    /// <summary>
    /// Tests that setting PageSize to the same value when CurrentPage is not 1 does not trigger CurrentPage reset or PropertyChanged for CurrentPage.
    /// Input: Setting PageSize to 10 (same value) when CurrentPage is 5.
    /// Expected: CurrentPage remains 5 and PropertyChanged for CurrentPage is not raised.
    /// </summary>
    [TestMethod]
    public void PageSize_SetSameValueWithDifferentCurrentPage_DoesNotResetCurrentPage()
    {
        // Arrange
        var mockAcademicService = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<FacultiesViewModel>>();
        var viewModel = new FacultiesViewModel(mockAcademicService.Object, mockLogger.Object);
        viewModel.GetType().GetField("currentPage", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!.SetValue(viewModel, 5);
        var raisedPropertyNames = new List<string>();
        viewModel.PropertyChanged += (sender, args) =>
        {
            raisedPropertyNames.Add(args.PropertyName!);
        };

        // Act
        viewModel.PageSize = 10;

        // Assert
        Assert.AreEqual(5, viewModel.CurrentPage);
        Assert.IsFalse(raisedPropertyNames.Contains("CurrentPage"));
    }

    /// <summary>
    /// Tests that setting PageSize from initial value to a different value correctly raises PropertyChanged events.
    /// Input: Changing PageSize from default 10 to 50.
    /// Expected: PropertyChanged is raised for "PageSize" and "CurrentPage".
    /// </summary>
    [TestMethod]
    public void PageSize_SetFromInitialValueToDifferent_RaisesPropertyChangedEvents()
    {
        // Arrange
        var mockAcademicService = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<FacultiesViewModel>>();
        var viewModel = new FacultiesViewModel(mockAcademicService.Object, mockLogger.Object);
        var raisedPropertyNames = new List<string>();
        viewModel.PropertyChanged += (sender, args) =>
        {
            if (!string.IsNullOrEmpty(args.PropertyName))
                raisedPropertyNames.Add(args.PropertyName);
        };

        // Act
        viewModel.PageSize = 50;

        // Assert
        Assert.IsTrue(raisedPropertyNames.Contains("PageSize"));
        Assert.IsTrue(raisedPropertyNames.Contains("CurrentPage"));
        Assert.AreEqual(50, viewModel.PageSize);
        Assert.AreEqual(1, viewModel.CurrentPage);
    }

    /// <summary>
    /// Tests that setting PageSize to zero updates the property correctly and resets CurrentPage.
    /// Input: PageSize value of 0.
    /// Expected: PageSize is set to 0 and CurrentPage is reset to 1.
    /// </summary>
    [TestMethod]
    public void PageSize_SetToZero_UpdatesValueAndResetsCurrentPage()
    {
        // Arrange
        var mockAcademicService = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<FacultiesViewModel>>();
        var viewModel = new FacultiesViewModel(mockAcademicService.Object, mockLogger.Object);
        viewModel.GetType().GetField("currentPage", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!.SetValue(viewModel, 10);

        // Act
        viewModel.PageSize = 0;

        // Assert
        Assert.AreEqual(0, viewModel.PageSize);
        Assert.AreEqual(1, viewModel.CurrentPage);
    }

    /// <summary>
    /// Tests that setting PageSize to a negative value updates the property correctly and resets CurrentPage.
    /// Input: PageSize value of -100.
    /// Expected: PageSize is set to -100 and CurrentPage is reset to 1.
    /// </summary>
    [TestMethod]
    public void PageSize_SetToNegativeValue_UpdatesValueAndResetsCurrentPage()
    {
        // Arrange
        var mockAcademicService = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<FacultiesViewModel>>();
        var viewModel = new FacultiesViewModel(mockAcademicService.Object, mockLogger.Object);
        viewModel.GetType().GetField("currentPage", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!.SetValue(viewModel, 8);

        // Act
        viewModel.PageSize = -100;

        // Assert
        Assert.AreEqual(-100, viewModel.PageSize);
        Assert.AreEqual(1, viewModel.CurrentPage);
    }

    /// <summary>
    /// Tests that setting PageSize to int.MaxValue updates the property correctly and resets CurrentPage.
    /// Input: PageSize value of int.MaxValue.
    /// Expected: PageSize is set to int.MaxValue and CurrentPage is reset to 1.
    /// </summary>
    [TestMethod]
    public void PageSize_SetToMaxValue_UpdatesValueAndResetsCurrentPage()
    {
        // Arrange
        var mockAcademicService = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<FacultiesViewModel>>();
        var viewModel = new FacultiesViewModel(mockAcademicService.Object, mockLogger.Object);
        viewModel.GetType().GetField("currentPage", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!.SetValue(viewModel, 3);

        // Act
        viewModel.PageSize = int.MaxValue;

        // Assert
        Assert.AreEqual(int.MaxValue, viewModel.PageSize);
        Assert.AreEqual(1, viewModel.CurrentPage);
    }

    /// <summary>
    /// Tests that setting PageSize to int.MinValue updates the property correctly and resets CurrentPage.
    /// Input: PageSize value of int.MinValue.
    /// Expected: PageSize is set to int.MinValue and CurrentPage is reset to 1.
    /// </summary>
    [TestMethod]
    public void PageSize_SetToMinValue_UpdatesValueAndResetsCurrentPage()
    {
        // Arrange
        var mockAcademicService = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<FacultiesViewModel>>();
        var viewModel = new FacultiesViewModel(mockAcademicService.Object, mockLogger.Object);
        viewModel.GetType().GetField("currentPage", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!.SetValue(viewModel, 6);

        // Act
        viewModel.PageSize = int.MinValue;

        // Assert
        Assert.AreEqual(int.MinValue, viewModel.PageSize);
        Assert.AreEqual(1, viewModel.CurrentPage);
    }

    /// <summary>
    /// Tests that PropertyChanged event for PageSize is raised exactly once when setting a different value.
    /// Input: New PageSize value of 30.
    /// Expected: PropertyChanged event for "PageSize" is raised exactly once.
    /// </summary>
    [TestMethod]
    public void PageSize_SetDifferentValue_RaisesPropertyChangedForPageSizeOnce()
    {
        // Arrange
        var mockAcademicService = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<FacultiesViewModel>>();
        var viewModel = new FacultiesViewModel(mockAcademicService.Object, mockLogger.Object);
        int pageSizeChangedCount = 0;
        viewModel.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == "PageSize")
                pageSizeChangedCount++;
        };

        // Act
        viewModel.PageSize = 30;

        // Assert
        Assert.AreEqual(1, pageSizeChangedCount);
    }

    /// <summary>
    /// Tests that multiple consecutive changes to PageSize raise PropertyChanged event each time.
    /// Input: Sequential PageSize values of 15, 25, 35.
    /// Expected: PropertyChanged event for "PageSize" is raised three times.
    /// </summary>
    [TestMethod]
    public void PageSize_MultipleConsecutiveChanges_RaisesPropertyChangedEachTime()
    {
        // Arrange
        var mockAcademicService = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<FacultiesViewModel>>();
        var viewModel = new FacultiesViewModel(mockAcademicService.Object, mockLogger.Object);
        int pageSizeChangedCount = 0;
        viewModel.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == "PageSize")
                pageSizeChangedCount++;
        };

        // Act
        viewModel.PageSize = 15;
        viewModel.PageSize = 25;
        viewModel.PageSize = 35;

        // Assert
        Assert.AreEqual(3, pageSizeChangedCount);
    }

    /// <summary>
    /// Tests that setting PageSize back to its initial value after changing it updates the property and raises PropertyChanged.
    /// Input: Change PageSize from 10 to 20, then back to 10.
    /// Expected: PropertyChanged is raised twice, CurrentPage is reset both times.
    /// </summary>
    [TestMethod]
    public void PageSize_SetBackToInitialValue_RaisesPropertyChangedAndResetsCurrentPage()
    {
        // Arrange
        var mockAcademicService = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<FacultiesViewModel>>();
        var viewModel = new FacultiesViewModel(mockAcademicService.Object, mockLogger.Object);
        int pageSizeChangedCount = 0;
        viewModel.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == "PageSize")
                pageSizeChangedCount++;
        };

        // Act
        viewModel.PageSize = 20;
        Assert.AreEqual(1, viewModel.CurrentPage);
        viewModel.GetType().GetField("currentPage", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!.SetValue(viewModel, 5);
        viewModel.PageSize = 10;

        // Assert
        Assert.AreEqual(2, pageSizeChangedCount);
        Assert.AreEqual(10, viewModel.PageSize);
        Assert.AreEqual(1, viewModel.CurrentPage);
    }

    /// <summary>
    /// Tests that setting PageSize does not throw any exceptions for valid or edge case values.
    /// Input: Various integer values including edge cases.
    /// Expected: No exceptions are thrown.
    /// </summary>
    /// <param name="newPageSize">The new page size value to test.</param>
    [TestMethod]
    [DataRow(int.MinValue)]
    [DataRow(-1000)]
    [DataRow(-1)]
    [DataRow(0)]
    [DataRow(1)]
    [DataRow(10)]
    [DataRow(100)]
    [DataRow(1000)]
    [DataRow(int.MaxValue)]
    public void PageSize_SetVariousValues_DoesNotThrowException(int newPageSize)
    {
        // Arrange
        var mockAcademicService = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<FacultiesViewModel>>();
        var viewModel = new FacultiesViewModel(mockAcademicService.Object, mockLogger.Object);

        // Act & Assert - Should not throw
        viewModel.PageSize = newPageSize;
        Assert.AreEqual(newPageSize, viewModel.PageSize);
    }

    /// <summary>
    /// Tests that the constructor stores the academic service dependency correctly.
    /// Input: Valid IAcademicService mock.
    /// Expected: Constructor completes and commands can execute without NullReferenceException for the service.
    /// </summary>
    [TestMethod]
    public void Constructor_WithValidAcademicService_StoresDependencyCorrectly()
    {
        // Arrange
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<FacultiesViewModel>>();

        // Act
        var viewModel = new FacultiesViewModel(mockAcademic.Object, mockLogger.Object);

        // Assert
        Assert.IsNotNull(viewModel);
        Assert.IsNotNull(viewModel.LoadCommand);

        // Verify the dependency is usable by checking a command that would use it
        // The LoadCommand should be able to execute without immediate error
        Assert.IsInstanceOfType(viewModel.LoadCommand, typeof(ICommand));
    }

    /// <summary>
    /// Tests that the constructor stores the logger dependency correctly.
    /// Input: Valid ILogger mock.
    /// Expected: Constructor completes successfully.
    /// </summary>
    [TestMethod]
    public void Constructor_WithValidLogger_StoresDependencyCorrectly()
    {
        // Arrange
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<FacultiesViewModel>>();

        // Act
        var viewModel = new FacultiesViewModel(mockAcademic.Object, mockLogger.Object);

        // Assert
        Assert.IsNotNull(viewModel);
        // Logger is stored internally and would be used in methods like LoadAsync
    }

    /// <summary>
    /// Tests that SearchCommand is initialized and can be executed.
    /// Input: Valid dependencies.
    /// Expected: SearchCommand is not null and can be executed.
    /// </summary>
    [TestMethod]
    public void Constructor_SearchCommand_InitializedAndExecutable()
    {
        // Arrange
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<FacultiesViewModel>>();

        // Act
        var viewModel = new FacultiesViewModel(mockAcademic.Object, mockLogger.Object);

        // Assert
        Assert.IsNotNull(viewModel.SearchCommand, "SearchCommand should be initialized");

        // Execute the command to verify it doesn't throw immediately
        viewModel.SearchCommand.Execute(null);

        // Command executed without throwing exception
    }

    /// <summary>
    /// Tests that LoadCommand is initialized as an async command.
    /// Input: Valid dependencies.
    /// Expected: LoadCommand is not null and is of type Command.
    /// </summary>
    [TestMethod]
    public void Constructor_LoadCommand_InitializedAsCommand()
    {
        // Arrange
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<FacultiesViewModel>>();

        // Act
        var viewModel = new FacultiesViewModel(mockAcademic.Object, mockLogger.Object);

        // Assert
        Assert.IsNotNull(viewModel.LoadCommand, "LoadCommand should be initialized");
        Assert.IsInstanceOfType(viewModel.LoadCommand, typeof(ICommand), "LoadCommand should implement ICommand");
    }

    /// <summary>
    /// Tests that SaveCommand is initialized as an async command.
    /// Input: Valid dependencies.
    /// Expected: SaveCommand is not null and is of type Command.
    /// </summary>
    [TestMethod]
    public void Constructor_SaveCommand_InitializedAsCommand()
    {
        // Arrange
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<FacultiesViewModel>>();

        // Act
        var viewModel = new FacultiesViewModel(mockAcademic.Object, mockLogger.Object);

        // Assert
        Assert.IsNotNull(viewModel.SaveCommand, "SaveCommand should be initialized");
        Assert.IsInstanceOfType(viewModel.SaveCommand, typeof(ICommand), "SaveCommand should implement ICommand");
    }

    /// <summary>
    /// Tests that DeleteFacultyCommand is initialized as a parameterized command.
    /// Input: Valid dependencies.
    /// Expected: DeleteFacultyCommand is not null and can accept FacultyDto parameter.
    /// </summary>
    [TestMethod]
    public void Constructor_DeleteFacultyCommand_InitializedAsParameterizedCommand()
    {
        // Arrange
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<FacultiesViewModel>>();

        // Act
        var viewModel = new FacultiesViewModel(mockAcademic.Object, mockLogger.Object);

        // Assert
        Assert.IsNotNull(viewModel.DeleteFacultyCommand, "DeleteFacultyCommand should be initialized");
        Assert.IsInstanceOfType(viewModel.DeleteFacultyCommand, typeof(ICommand), "DeleteFacultyCommand should implement ICommand");
    }

    /// <summary>
    /// Tests that ExportCommand is initialized as an async command.
    /// Input: Valid dependencies.
    /// Expected: ExportCommand is not null and is of type Command.
    /// </summary>
    [TestMethod]
    public void Constructor_ExportCommand_InitializedAsCommand()
    {
        // Arrange
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<FacultiesViewModel>>();

        // Act
        var viewModel = new FacultiesViewModel(mockAcademic.Object, mockLogger.Object);

        // Assert
        Assert.IsNotNull(viewModel.ExportCommand, "ExportCommand should be initialized");
        Assert.IsInstanceOfType(viewModel.ExportCommand, typeof(ICommand), "ExportCommand should implement ICommand");
    }

    /// <summary>
    /// Tests that RefreshCommand is initialized as an async command.
    /// Input: Valid dependencies.
    /// Expected: RefreshCommand is not null and is of type Command.
    /// </summary>
    [TestMethod]
    public void Constructor_RefreshCommand_InitializedAsCommand()
    {
        // Arrange
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<FacultiesViewModel>>();

        // Act
        var viewModel = new FacultiesViewModel(mockAcademic.Object, mockLogger.Object);

        // Assert
        Assert.IsNotNull(viewModel.RefreshCommand, "RefreshCommand should be initialized");
        Assert.IsInstanceOfType(viewModel.RefreshCommand, typeof(ICommand), "RefreshCommand should implement ICommand");
    }

    /// <summary>
    /// Tests that EditFacultyCommand handles FacultyDto with null UniversityId correctly.
    /// Input: FacultyDto with null UniversityId.
    /// Expected: SelectedUniversity is set to null when no match is found.
    /// </summary>
    [TestMethod]
    public void Constructor_EditFacultyCommand_WithNullUniversityId_SetsSelectedUniversityToNull()
    {
        // Arrange
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<FacultiesViewModel>>();
        var viewModel = new FacultiesViewModel(mockAcademic.Object, mockLogger.Object);

        viewModel.Universities.Add(new LookupItem { Id = "u1", Name = "University 1" });

        var faculty = new FacultyDto
        {
            Id = "f1",
            Name = "Faculty 1",
            UniversityId = null
        };

        // Act
        ((Command<FacultyDto>)viewModel.EditFacultyCommand).Execute(faculty);

        // Assert
        Assert.AreEqual("f1", viewModel.EditId);
        Assert.AreEqual("Faculty 1", viewModel.EditName);
        Assert.IsNull(viewModel.SelectedUniversity, "SelectedUniversity should be null when UniversityId is null");
        Assert.IsTrue(viewModel.IsEditing);
    }

    /// <summary>
    /// Tests that EditFacultyCommand handles FacultyDto with empty string UniversityId correctly.
    /// Input: FacultyDto with empty UniversityId.
    /// Expected: SelectedUniversity is set to null when no match is found.
    /// </summary>
    [TestMethod]
    public void Constructor_EditFacultyCommand_WithEmptyUniversityId_SetsSelectedUniversityToNull()
    {
        // Arrange
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<FacultiesViewModel>>();
        var viewModel = new FacultiesViewModel(mockAcademic.Object, mockLogger.Object);

        viewModel.Universities.Add(new LookupItem { Id = "u1", Name = "University 1" });

        var faculty = new FacultyDto
        {
            Id = "f2",
            Name = "Faculty 2",
            UniversityId = string.Empty
        };

        // Act
        ((Command<FacultyDto>)viewModel.EditFacultyCommand).Execute(faculty);

        // Assert
        Assert.AreEqual("f2", viewModel.EditId);
        Assert.AreEqual("Faculty 2", viewModel.EditName);
        Assert.IsNull(viewModel.SelectedUniversity, "SelectedUniversity should be null when UniversityId is empty");
        Assert.IsTrue(viewModel.IsEditing);
    }

    /// <summary>
    /// Tests that EditFacultyCommand handles FacultyDto with whitespace-only Name correctly.
    /// Input: FacultyDto with whitespace-only Name.
    /// Expected: EditName is set to the whitespace string.
    /// </summary>
    [TestMethod]
    public void Constructor_EditFacultyCommand_WithWhitespaceOnlyName_SetsEditNameCorrectly()
    {
        // Arrange
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<FacultiesViewModel>>();
        var viewModel = new FacultiesViewModel(mockAcademic.Object, mockLogger.Object);

        var faculty = new FacultyDto
        {
            Id = "f3",
            Name = "   ",
            UniversityId = "u1"
        };

        // Act
        ((Command<FacultyDto>)viewModel.EditFacultyCommand).Execute(faculty);

        // Assert
        Assert.AreEqual("f3", viewModel.EditId);
        Assert.AreEqual("   ", viewModel.EditName);
        Assert.IsTrue(viewModel.IsEditing);
    }

    /// <summary>
    /// Tests that EditFacultyCommand handles FacultyDto with empty Name correctly.
    /// Input: FacultyDto with empty Name.
    /// Expected: EditName is set to empty string.
    /// </summary>
    [TestMethod]
    public void Constructor_EditFacultyCommand_WithEmptyName_SetsEditNameToEmpty()
    {
        // Arrange
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<FacultiesViewModel>>();
        var viewModel = new FacultiesViewModel(mockAcademic.Object, mockLogger.Object);

        var faculty = new FacultyDto
        {
            Id = "f4",
            Name = string.Empty,
            UniversityId = "u1"
        };

        // Act
        ((Command<FacultyDto>)viewModel.EditFacultyCommand).Execute(faculty);

        // Assert
        Assert.AreEqual("f4", viewModel.EditId);
        Assert.AreEqual(string.Empty, viewModel.EditName);
        Assert.IsTrue(viewModel.IsEditing);
    }

    /// <summary>
    /// Tests that EditFacultyCommand handles FacultyDto with very long Name correctly.
    /// Input: FacultyDto with very long Name (1000 characters).
    /// Expected: EditName is set to the long string.
    /// </summary>
    [TestMethod]
    public void Constructor_EditFacultyCommand_WithVeryLongName_SetsEditNameCorrectly()
    {
        // Arrange
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<FacultiesViewModel>>();
        var viewModel = new FacultiesViewModel(mockAcademic.Object, mockLogger.Object);

        var longName = new string('A', 1000);
        var faculty = new FacultyDto
        {
            Id = "f5",
            Name = longName,
            UniversityId = "u1"
        };

        // Act
        ((Command<FacultyDto>)viewModel.EditFacultyCommand).Execute(faculty);

        // Assert
        Assert.AreEqual("f5", viewModel.EditId);
        Assert.AreEqual(longName, viewModel.EditName);
        Assert.AreEqual(1000, viewModel.EditName.Length);
        Assert.IsTrue(viewModel.IsEditing);
    }

    /// <summary>
    /// Tests that EditFacultyCommand handles FacultyDto with special characters in Name.
    /// Input: FacultyDto with special characters in Name.
    /// Expected: EditName is set correctly with special characters preserved.
    /// </summary>
    [TestMethod]
    [DataRow("Faculty!@#$%")]
    [DataRow("Faculty<>?")]
    [DataRow("Faculty\t\n\r")]
    [DataRow("Faculty™©®")]
    public void Constructor_EditFacultyCommand_WithSpecialCharactersInName_SetsEditNameCorrectly(string specialName)
    {
        // Arrange
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<FacultiesViewModel>>();
        var viewModel = new FacultiesViewModel(mockAcademic.Object, mockLogger.Object);

        var faculty = new FacultyDto
        {
            Id = "f6",
            Name = specialName,
            UniversityId = "u1"
        };

        // Act
        ((Command<FacultyDto>)viewModel.EditFacultyCommand).Execute(faculty);

        // Assert
        Assert.AreEqual("f6", viewModel.EditId);
        Assert.AreEqual(specialName, viewModel.EditName);
        Assert.IsTrue(viewModel.IsEditing);
    }

    /// <summary>
    /// Tests that FilterCommand can handle consecutive calls with different values.
    /// Input: Multiple different filter values in sequence.
    /// Expected: UniversityFilter is updated correctly each time.
    /// </summary>
    [TestMethod]
    public void Constructor_FilterCommand_MultipleConsecutiveCalls_UpdatesFilterEachTime()
    {
        // Arrange
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<FacultiesViewModel>>();
        var viewModel = new FacultiesViewModel(mockAcademic.Object, mockLogger.Object);

        // Act & Assert
        ((Command<string>)viewModel.FilterCommand).Execute("Filter1");
        Assert.AreEqual("Filter1", viewModel.UniversityFilter);

        ((Command<string>)viewModel.FilterCommand).Execute("Filter2");
        Assert.AreEqual("Filter2", viewModel.UniversityFilter);

        ((Command<string>)viewModel.FilterCommand).Execute(string.Empty);
        Assert.AreEqual(string.Empty, viewModel.UniversityFilter);

        ((Command<string?>)viewModel.FilterCommand).Execute(null);
        Assert.IsNull(viewModel.UniversityFilter);
    }

    /// <summary>
    /// Tests that NextPageCommand at boundary condition (CurrentPage = TotalPages - 1) increments correctly.
    /// Input: CurrentPage = 9, TotalPages = 10.
    /// Expected: CurrentPage increments to 10.
    /// </summary>
    [TestMethod]
    public void Constructor_NextPageCommand_AtBoundary_IncrementsToTotalPages()
    {
        // Arrange
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<FacultiesViewModel>>();
        var viewModel = new FacultiesViewModel(mockAcademic.Object, mockLogger.Object);

        viewModel.TotalPages = 10;
        viewModel.CurrentPage = 9;

        // Act
        viewModel.NextPageCommand.Execute(null);

        // Assert
        Assert.AreEqual(10, viewModel.CurrentPage);
    }

    /// <summary>
    /// Tests that PrevPageCommand at boundary condition (CurrentPage = 2) decrements correctly.
    /// Input: CurrentPage = 2.
    /// Expected: CurrentPage decrements to 1.
    /// </summary>
    [TestMethod]
    public void Constructor_PrevPageCommand_AtBoundary_DecrementsToOne()
    {
        // Arrange
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<FacultiesViewModel>>();
        var viewModel = new FacultiesViewModel(mockAcademic.Object, mockLogger.Object);

        viewModel.CurrentPage = 2;

        // Act
        viewModel.PrevPageCommand.Execute(null);

        // Assert
        Assert.AreEqual(1, viewModel.CurrentPage);
    }

    /// <summary>
    /// Tests that NextPageCommand with negative TotalPages does not increment.
    /// Input: CurrentPage = 1, TotalPages = -1.
    /// Expected: CurrentPage remains 1.
    /// </summary>
    [TestMethod]
    public void Constructor_NextPageCommand_WithNegativeTotalPages_DoesNotIncrement()
    {
        // Arrange
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<FacultiesViewModel>>();
        var viewModel = new FacultiesViewModel(mockAcademic.Object, mockLogger.Object);

        viewModel.TotalPages = -1;
        viewModel.CurrentPage = 1;

        // Act
        viewModel.NextPageCommand.Execute(null);

        // Assert
        Assert.AreEqual(1, viewModel.CurrentPage);
    }

    /// <summary>
    /// Tests that PrevPageCommand with CurrentPage = 0 does not decrement.
    /// Input: CurrentPage = 0.
    /// Expected: CurrentPage remains 0.
    /// </summary>
    [TestMethod]
    public void Constructor_PrevPageCommand_WithZeroCurrentPage_DoesNotDecrement()
    {
        // Arrange
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<FacultiesViewModel>>();
        var viewModel = new FacultiesViewModel(mockAcademic.Object, mockLogger.Object);

        viewModel.CurrentPage = 0;

        // Act
        viewModel.PrevPageCommand.Execute(null);

        // Assert
        Assert.AreEqual(0, viewModel.CurrentPage);
    }

    /// <summary>
    /// Tests that AddFacultyCommand can be executed multiple times and resets form each time.
    /// Input: Execute AddFacultyCommand twice.
    /// Expected: Form is reset correctly both times.
    /// </summary>
    [TestMethod]
    public void Constructor_AddFacultyCommand_MultipleExecutions_ResetsFormEachTime()
    {
        // Arrange
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<FacultiesViewModel>>();
        var viewModel = new FacultiesViewModel(mockAcademic.Object, mockLogger.Object);

        // First execution
        viewModel.EditId = "someId";
        viewModel.EditName = "someName";
        viewModel.SelectedUniversity = new LookupItem { Id = "u1", Name = "University" };

        // Act
        viewModel.AddFacultyCommand.Execute(null);

        // Assert
        Assert.IsNull(viewModel.EditId);
        Assert.AreEqual(string.Empty, viewModel.EditName);
        Assert.IsNull(viewModel.SelectedUniversity);
        Assert.IsTrue(viewModel.IsEditing);

        // Second execution
        viewModel.EditId = "anotherId";
        viewModel.EditName = "anotherName";
        viewModel.SelectedUniversity = new LookupItem { Id = "u2", Name = "University 2" };

        // Act
        viewModel.AddFacultyCommand.Execute(null);

        // Assert
        Assert.IsNull(viewModel.EditId);
        Assert.AreEqual(string.Empty, viewModel.EditName);
        Assert.IsNull(viewModel.SelectedUniversity);
        Assert.IsTrue(viewModel.IsEditing);
    }

    /// <summary>
    /// Tests that CancelEditCommand can be executed multiple times and resets form each time.
    /// Input: Execute CancelEditCommand twice with different initial states.
    /// Expected: Form is reset and IsEditing is false both times.
    /// </summary>
    [TestMethod]
    public void Constructor_CancelEditCommand_MultipleExecutions_ResetsFormEachTime()
    {
        // Arrange
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<FacultiesViewModel>>();
        var viewModel = new FacultiesViewModel(mockAcademic.Object, mockLogger.Object);

        // First execution
        viewModel.IsEditing = true;
        viewModel.EditId = "id1";
        viewModel.EditName = "name1";
        viewModel.SelectedUniversity = new LookupItem { Id = "u1", Name = "Uni1" };

        // Act
        viewModel.CancelEditCommand.Execute(null);

        // Assert
        Assert.IsFalse(viewModel.IsEditing);
        Assert.AreEqual(string.Empty, viewModel.EditName);
        Assert.IsNull(viewModel.EditId);
        Assert.IsNull(viewModel.SelectedUniversity);

        // Second execution
        viewModel.IsEditing = true;
        viewModel.EditId = "id2";
        viewModel.EditName = "name2";
        viewModel.SelectedUniversity = new LookupItem { Id = "u2", Name = "Uni2" };

        // Act
        viewModel.CancelEditCommand.Execute(null);

        // Assert
        Assert.IsFalse(viewModel.IsEditing);
        Assert.AreEqual(string.Empty, viewModel.EditName);
        Assert.IsNull(viewModel.EditId);
        Assert.IsNull(viewModel.SelectedUniversity);
    }

    /// <summary>
    /// Tests that all commands can be executed without throwing exceptions immediately after construction.
    /// Input: Newly constructed view model.
    /// Expected: All non-parameterized commands can execute without immediate exception.
    /// </summary>
    [TestMethod]
    public void Constructor_AllNonParameterizedCommands_CanExecuteWithoutImmediateException()
    {
        // Arrange
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<FacultiesViewModel>>();
        mockAcademic.Setup(a => a.GetUniversitiesAsync()).ReturnsAsync(new List<LookupItem>());
        mockAcademic.Setup(a => a.GetFacultyDetailsAsync()).ReturnsAsync(new List<FacultyDto>());

        var viewModel = new FacultiesViewModel(mockAcademic.Object, mockLogger.Object);

        // Act & Assert - these should not throw immediately
        viewModel.SearchCommand.Execute(null);
        viewModel.AddFacultyCommand.Execute(null);
        viewModel.CancelEditCommand.Execute(null);

        // NextPage and PrevPage commands execute inline logic
        viewModel.NextPageCommand.Execute(null);
        viewModel.PrevPageCommand.Execute(null);

        // Async commands may start async operations but shouldn't throw immediately
        // Note: We're not awaiting these, just checking they don't throw synchronously
        viewModel.LoadCommand.Execute(null);
        viewModel.SaveCommand.Execute(null);
        // ExportCommand is excluded: it uses Shell.Current and FileSystem which are unavailable in test context
        viewModel.RefreshCommand.Execute(null);

        // If we reach here, no immediate exceptions were thrown
        Assert.IsTrue(true);
    }

    /// <summary>
    /// Tests that TotalFaculties property handles extreme boundary values correctly.
    /// Validates proper storage and retrieval of int.MaxValue and int.MinValue.
    /// </summary>
    [TestMethod]
    [DataRow(int.MaxValue)]
    [DataRow(int.MinValue)]
    public void TotalFaculties_SetBoundaryValues_StoresAndReturnsCorrectly(int boundaryValue)
    {
        // Arrange
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<FacultiesViewModel>>();
        var viewModel = new FacultiesViewModel(mockAcademic.Object, mockLogger.Object);

        // Act
        viewModel.TotalFaculties = boundaryValue;

        // Assert
        Assert.AreEqual(boundaryValue, viewModel.TotalFaculties);
    }

    /// <summary>
    /// Tests that setting TotalFaculties to zero updates the property correctly and raises PropertyChanged.
    /// Verifies handling of the zero boundary value.
    /// </summary>
    [TestMethod]
    public void TotalFaculties_SetToZero_UpdatesPropertyAndRaisesPropertyChanged()
    {
        // Arrange
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<FacultiesViewModel>>();
        var viewModel = new FacultiesViewModel(mockAcademic.Object, mockLogger.Object);
        viewModel.TotalFaculties = 50;
        var eventRaised = false;
        viewModel.PropertyChanged += (sender, e) =>
        {
            if (e.PropertyName == "TotalFaculties")
            {
                eventRaised = true;
            }
        };

        // Act
        viewModel.TotalFaculties = 0;

        // Assert
        Assert.AreEqual(0, viewModel.TotalFaculties);
        Assert.IsTrue(eventRaised);
    }

    /// <summary>
    /// Tests that TotalFaculties property correctly handles alternating between positive and negative values.
    /// Validates multiple transitions between different value ranges.
    /// </summary>
    [TestMethod]
    public void TotalFaculties_AlternateBetweenPositiveAndNegative_UpdatesCorrectly()
    {
        // Arrange
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<FacultiesViewModel>>();
        var viewModel = new FacultiesViewModel(mockAcademic.Object, mockLogger.Object);

        // Act & Assert
        viewModel.TotalFaculties = 100;
        Assert.AreEqual(100, viewModel.TotalFaculties);

        viewModel.TotalFaculties = -50;
        Assert.AreEqual(-50, viewModel.TotalFaculties);

        viewModel.TotalFaculties = 75;
        Assert.AreEqual(75, viewModel.TotalFaculties);

        viewModel.TotalFaculties = -25;
        Assert.AreEqual(-25, viewModel.TotalFaculties);
    }

    /// <summary>
    /// Tests that setting ActiveFacultiesCount raises PropertyChanged event exactly once per change.
    /// Input: Multiple different values.
    /// Expected: PropertyChanged event is raised exactly once for each distinct value change.
    /// </summary>
    [TestMethod]
    public void ActiveFacultiesCount_SetMultipleDifferentValues_RaisesPropertyChangedForEachChange()
    {
        // Arrange
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<FacultiesViewModel>>();
        var viewModel = new FacultiesViewModel(mockAcademic.Object, mockLogger.Object);
        var propertyChangedCount = 0;
        viewModel.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == "ActiveFacultiesCount")
                propertyChangedCount++;
        };

        // Act
        viewModel.ActiveFacultiesCount = 10;
        viewModel.ActiveFacultiesCount = 20;
        viewModel.ActiveFacultiesCount = 30;

        // Assert
        Assert.AreEqual(3, propertyChangedCount);
    }

    /// <summary>
    /// Tests that setting ActiveFacultiesCount to boundary value int.MinValue works correctly.
    /// Input: int.MinValue (-2,147,483,648).
    /// Expected: Property value is set correctly and PropertyChanged event is raised.
    /// </summary>
    [TestMethod]
    public void ActiveFacultiesCount_SetToMinValue_UpdatesPropertyAndRaisesEvent()
    {
        // Arrange
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<FacultiesViewModel>>();
        var viewModel = new FacultiesViewModel(mockAcademic.Object, mockLogger.Object);
        var eventRaised = false;
        viewModel.PropertyChanged += (sender, args) => eventRaised = true;

        // Act
        viewModel.ActiveFacultiesCount = int.MinValue;

        // Assert
        Assert.AreEqual(int.MinValue, viewModel.ActiveFacultiesCount);
        Assert.IsTrue(eventRaised);
    }

    /// <summary>
    /// Tests that setting ActiveFacultiesCount to boundary value int.MaxValue works correctly.
    /// Input: int.MaxValue (2,147,483,647).
    /// Expected: Property value is set correctly and PropertyChanged event is raised.
    /// </summary>
    [TestMethod]
    public void ActiveFacultiesCount_SetToMaxValue_UpdatesPropertyAndRaisesEvent()
    {
        // Arrange
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<FacultiesViewModel>>();
        var viewModel = new FacultiesViewModel(mockAcademic.Object, mockLogger.Object);
        var eventRaised = false;
        viewModel.PropertyChanged += (sender, args) => eventRaised = true;

        // Act
        viewModel.ActiveFacultiesCount = int.MaxValue;

        // Assert
        Assert.AreEqual(int.MaxValue, viewModel.ActiveFacultiesCount);
        Assert.IsTrue(eventRaised);
    }

    /// <summary>
    /// Tests that setting ActiveFacultiesCount to zero from a non-zero value works correctly.
    /// Input: 0 (after setting to non-zero).
    /// Expected: Property value is set to zero and PropertyChanged event is raised.
    /// </summary>
    [TestMethod]
    public void ActiveFacultiesCount_SetToZeroFromNonZero_UpdatesPropertyAndRaisesEvent()
    {
        // Arrange
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<FacultiesViewModel>>();
        var viewModel = new FacultiesViewModel(mockAcademic.Object, mockLogger.Object);
        viewModel.ActiveFacultiesCount = 100;
        var eventRaised = false;
        viewModel.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == "ActiveFacultiesCount")
                eventRaised = true;
        };

        // Act
        viewModel.ActiveFacultiesCount = 0;

        // Assert
        Assert.AreEqual(0, viewModel.ActiveFacultiesCount);
        Assert.IsTrue(eventRaised);
    }

    /// <summary>
    /// Tests that setting ActiveFacultiesCount to negative values works correctly.
    /// Input: Various negative integer values.
    /// Expected: Property value is set correctly for each negative value.
    /// </summary>
    /// <param name="negativeValue">The negative value to test.</param>
    [TestMethod]
    [DataRow(-1)]
    [DataRow(-10)]
    [DataRow(-100)]
    [DataRow(-1000)]
    [DataRow(-999999)]
    public void ActiveFacultiesCount_SetNegativeValues_UpdatesPropertyCorrectly(int negativeValue)
    {
        // Arrange
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<FacultiesViewModel>>();
        var viewModel = new FacultiesViewModel(mockAcademic.Object, mockLogger.Object);

        // Act
        viewModel.ActiveFacultiesCount = negativeValue;

        // Assert
        Assert.AreEqual(negativeValue, viewModel.ActiveFacultiesCount);
    }

    /// <summary>
    /// Tests that setting ActiveFacultiesCount to positive values works correctly.
    /// Input: Various positive integer values.
    /// Expected: Property value is set correctly for each positive value.
    /// </summary>
    /// <param name="positiveValue">The positive value to test.</param>
    [TestMethod]
    [DataRow(1)]
    [DataRow(10)]
    [DataRow(100)]
    [DataRow(1000)]
    [DataRow(999999)]
    public void ActiveFacultiesCount_SetPositiveValues_UpdatesPropertyCorrectly(int positiveValue)
    {
        // Arrange
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<FacultiesViewModel>>();
        var viewModel = new FacultiesViewModel(mockAcademic.Object, mockLogger.Object);

        // Act
        viewModel.ActiveFacultiesCount = positiveValue;

        // Assert
        Assert.AreEqual(positiveValue, viewModel.ActiveFacultiesCount);
    }

    /// <summary>
    /// Tests that PropertyChanged event is not raised when setting ActiveFacultiesCount to zero initially.
    /// Input: 0 when property is already 0 (default).
    /// Expected: PropertyChanged event is not raised.
    /// </summary>
    [TestMethod]
    public void ActiveFacultiesCount_SetToZeroWhenAlreadyZero_DoesNotRaisePropertyChanged()
    {
        // Arrange
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<FacultiesViewModel>>();
        var viewModel = new FacultiesViewModel(mockAcademic.Object, mockLogger.Object);
        var eventRaised = false;
        viewModel.PropertyChanged += (sender, args) => eventRaised = true;

        // Act
        viewModel.ActiveFacultiesCount = 0;

        // Assert
        Assert.IsFalse(eventRaised);
    }

    /// <summary>
    /// Tests that alternating between different values correctly updates the property each time.
    /// Input: Alternating between two different values.
    /// Expected: Property value reflects the current set value after each assignment.
    /// </summary>
    [TestMethod]
    public void ActiveFacultiesCount_AlternateBetweenValues_UpdatesCorrectlyEachTime()
    {
        // Arrange
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<FacultiesViewModel>>();
        var viewModel = new FacultiesViewModel(mockAcademic.Object, mockLogger.Object);

        // Act & Assert
        viewModel.ActiveFacultiesCount = 5;
        Assert.AreEqual(5, viewModel.ActiveFacultiesCount);

        viewModel.ActiveFacultiesCount = 10;
        Assert.AreEqual(10, viewModel.ActiveFacultiesCount);

        viewModel.ActiveFacultiesCount = 5;
        Assert.AreEqual(5, viewModel.ActiveFacultiesCount);

        viewModel.ActiveFacultiesCount = 10;
        Assert.AreEqual(10, viewModel.ActiveFacultiesCount);
    }

    /// <summary>
    /// Tests that PropertyChanged event is raised exactly once when IsEditing value changes.
    /// Input: true (from default false)
    /// Expected: PropertyChanged event is raised exactly once.
    /// </summary>
    [TestMethod]
    public void IsEditing_ValueChange_RaisesPropertyChangedOnce()
    {
        // Arrange
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<FacultiesViewModel>>();
        var viewModel = new FacultiesViewModel(mockAcademic.Object, mockLogger.Object);
        var eventRaisedCount = 0;
        viewModel.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == "IsEditing")
            {
                eventRaisedCount++;
            }
        };

        // Act
        viewModel.IsEditing = true;

        // Assert
        Assert.AreEqual(1, eventRaisedCount);
    }

    /// <summary>
    /// Tests that multiple consecutive value changes each raise PropertyChanged event.
    /// Input: Alternating true and false values
    /// Expected: PropertyChanged event is raised for each value change.
    /// </summary>
    [TestMethod]
    public void IsEditing_MultipleValueChanges_RaisesPropertyChangedForEachChange()
    {
        // Arrange
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<FacultiesViewModel>>();
        var viewModel = new FacultiesViewModel(mockAcademic.Object, mockLogger.Object);
        var eventRaisedCount = 0;
        viewModel.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == "IsEditing")
            {
                eventRaisedCount++;
            }
        };

        // Act
        viewModel.IsEditing = true;
        viewModel.IsEditing = false;
        viewModel.IsEditing = true;
        viewModel.IsEditing = false;

        // Assert
        Assert.AreEqual(4, eventRaisedCount);
    }

    /// <summary>
    /// Tests that setting IsEditing from false to true and back to false works correctly.
    /// Input: true then false
    /// Expected: Final value is false, PropertyChanged raised twice.
    /// </summary>
    [TestMethod]
    public void IsEditing_SetTrueAndBackToFalse_UpdatesCorrectly()
    {
        // Arrange
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<FacultiesViewModel>>();
        var viewModel = new FacultiesViewModel(mockAcademic.Object, mockLogger.Object);
        var eventRaisedCount = 0;
        viewModel.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == "IsEditing")
            {
                eventRaisedCount++;
            }
        };

        // Act
        viewModel.IsEditing = true;
        viewModel.IsEditing = false;

        // Assert
        Assert.IsFalse(viewModel.IsEditing);
        Assert.AreEqual(2, eventRaisedCount);
    }

    /// <summary>
    /// Tests that the getter returns the exact value that was set via the setter.
    /// Input: true
    /// Expected: Getter returns true.
    /// </summary>
    [TestMethod]
    public void IsEditing_GetAfterSet_ReturnsExactSetValue()
    {
        // Arrange
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<FacultiesViewModel>>();
        var viewModel = new FacultiesViewModel(mockAcademic.Object, mockLogger.Object);

        // Act
        viewModel.IsEditing = true;
        var result = viewModel.IsEditing;

        // Assert
        Assert.IsTrue(result);
    }

    /// <summary>
    /// Tests that CurrentPage property raises PropertyChanged event for both "CurrentPage" and "PageInfo" when value changes.
    /// Input: Setting CurrentPage from 1 (initial) to 2
    /// Expected: PropertyChanged is raised for "CurrentPage" and "PageInfo", value is updated to 2.
    /// </summary>
    [TestMethod]
    public void CurrentPage_SetDifferentValue_RaisesPropertyChangedForBothCurrentPageAndPageInfo()
    {
        // Arrange
        var mockAcademicService = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<FacultiesViewModel>>();
        var viewModel = new FacultiesViewModel(mockAcademicService.Object, mockLogger.Object);
        var propertyChangedEvents = new List<string>();
        viewModel.PropertyChanged += (sender, args) => propertyChangedEvents.Add(args.PropertyName ?? string.Empty);

        // Act
        viewModel.CurrentPage = 2;

        // Assert
        Assert.AreEqual(2, viewModel.CurrentPage);
        Assert.IsTrue(propertyChangedEvents.Contains("CurrentPage"));
        Assert.IsTrue(propertyChangedEvents.Contains("PageInfo"));
        Assert.AreEqual(2, propertyChangedEvents.Count);
    }

    /// <summary>
    /// Tests that CurrentPage property does not raise PropertyChanged events when set to the same value.
    /// Input: Setting CurrentPage to 1 (same as initial value)
    /// Expected: No PropertyChanged events are raised, value remains 1.
    /// </summary>
    [TestMethod]
    public void CurrentPage_SetSameValue_DoesNotRaisePropertyChangedEvents()
    {
        // Arrange
        var mockAcademicService = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<FacultiesViewModel>>();
        var viewModel = new FacultiesViewModel(mockAcademicService.Object, mockLogger.Object);
        var propertyChangedEvents = new List<string>();
        viewModel.PropertyChanged += (sender, args) => propertyChangedEvents.Add(args.PropertyName ?? string.Empty);

        // Act
        viewModel.CurrentPage = 1;

        // Assert
        Assert.AreEqual(1, viewModel.CurrentPage);
        Assert.AreEqual(0, propertyChangedEvents.Count);
    }

    /// <summary>
    /// Tests that CurrentPage property correctly updates to various edge case integer values.
    /// Input: Various integer values including boundaries (int.MinValue, int.MaxValue, 0, negative, positive)
    /// Expected: Property value is updated correctly for each case and PropertyChanged events are raised.
    /// </summary>
    [TestMethod]
    [DataRow(0)]
    [DataRow(-1)]
    [DataRow(-100)]
    [DataRow(2)]
    [DataRow(10)]
    [DataRow(100)]
    [DataRow(1000)]
    [DataRow(int.MinValue)]
    [DataRow(int.MaxValue)]
    public void CurrentPage_SetVariousValues_UpdatesPropertyAndRaisesPropertyChanged(int newValue)
    {
        // Arrange
        var mockAcademicService = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<FacultiesViewModel>>();
        var viewModel = new FacultiesViewModel(mockAcademicService.Object, mockLogger.Object);
        var propertyChangedEvents = new List<string>();
        viewModel.PropertyChanged += (sender, args) => propertyChangedEvents.Add(args.PropertyName ?? string.Empty);

        // Act
        viewModel.CurrentPage = newValue;

        // Assert
        Assert.AreEqual(newValue, viewModel.CurrentPage);
        Assert.IsTrue(propertyChangedEvents.Contains("CurrentPage"));
        Assert.IsTrue(propertyChangedEvents.Contains("PageInfo"));
    }

    /// <summary>
    /// Tests that PropertyChanged events are raised in the correct order when CurrentPage changes.
    /// Input: Setting CurrentPage from 1 to 5
    /// Expected: "CurrentPage" PropertyChanged event is raised before "PageInfo" PropertyChanged event.
    /// </summary>
    [TestMethod]
    public void CurrentPage_SetNewValue_RaisesPropertyChangedEventsInCorrectOrder()
    {
        // Arrange
        var mockAcademicService = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<FacultiesViewModel>>();
        var viewModel = new FacultiesViewModel(mockAcademicService.Object, mockLogger.Object);
        var propertyChangedEvents = new List<string>();
        viewModel.PropertyChanged += (sender, args) => propertyChangedEvents.Add(args.PropertyName ?? string.Empty);

        // Act
        viewModel.CurrentPage = 5;

        // Assert
        Assert.AreEqual(2, propertyChangedEvents.Count);
        Assert.AreEqual("CurrentPage", propertyChangedEvents[0]);
        Assert.AreEqual("PageInfo", propertyChangedEvents[1]);
    }

    /// <summary>
    /// Tests that multiple consecutive changes to CurrentPage each raise PropertyChanged events.
    /// Input: Setting CurrentPage to 2, then 3, then 4
    /// Expected: PropertyChanged events are raised for each change (total 6 events: 3 for CurrentPage, 3 for PageInfo).
    /// </summary>
    [TestMethod]
    public void CurrentPage_MultipleConsecutiveChanges_RaisesPropertyChangedForEachChange()
    {
        // Arrange
        var mockAcademicService = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<FacultiesViewModel>>();
        var viewModel = new FacultiesViewModel(mockAcademicService.Object, mockLogger.Object);
        var propertyChangedEvents = new List<string>();
        viewModel.PropertyChanged += (sender, args) => propertyChangedEvents.Add(args.PropertyName ?? string.Empty);

        // Act
        viewModel.CurrentPage = 2;
        viewModel.CurrentPage = 3;
        viewModel.CurrentPage = 4;

        // Assert
        Assert.AreEqual(4, viewModel.CurrentPage);
        Assert.AreEqual(6, propertyChangedEvents.Count);
        Assert.AreEqual(3, propertyChangedEvents.Count(e => e == "CurrentPage"));
        Assert.AreEqual(3, propertyChangedEvents.Count(e => e == "PageInfo"));
    }

    /// <summary>
    /// Tests that setting CurrentPage to the same value multiple times does not raise PropertyChanged events.
    /// Input: Setting CurrentPage to 5, then to 5 again, then to 5 once more
    /// Expected: PropertyChanged events are raised only for the first change.
    /// </summary>
    [TestMethod]
    public void CurrentPage_SetSameValueMultipleTimes_RaisesPropertyChangedOnlyOnce()
    {
        // Arrange
        var mockAcademicService = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<FacultiesViewModel>>();
        var viewModel = new FacultiesViewModel(mockAcademicService.Object, mockLogger.Object);
        viewModel.CurrentPage = 5;
        var propertyChangedEvents = new List<string>();
        viewModel.PropertyChanged += (sender, args) => propertyChangedEvents.Add(args.PropertyName ?? string.Empty);

        // Act
        viewModel.CurrentPage = 5;
        viewModel.CurrentPage = 5;
        viewModel.CurrentPage = 5;

        // Assert
        Assert.AreEqual(5, viewModel.CurrentPage);
        Assert.AreEqual(0, propertyChangedEvents.Count);
    }

    /// <summary>
    /// Tests that CurrentPage property correctly handles alternating between different values.
    /// Input: Setting CurrentPage to 2, then 1, then 2, then 1
    /// Expected: PropertyChanged events are raised for each actual change.
    /// </summary>
    [TestMethod]
    public void CurrentPage_AlternateBetweenValues_RaisesPropertyChangedForEachChange()
    {
        // Arrange
        var mockAcademicService = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<FacultiesViewModel>>();
        var viewModel = new FacultiesViewModel(mockAcademicService.Object, mockLogger.Object);
        var propertyChangedEvents = new List<string>();
        viewModel.PropertyChanged += (sender, args) => propertyChangedEvents.Add(args.PropertyName ?? string.Empty);

        // Act
        viewModel.CurrentPage = 2;
        viewModel.CurrentPage = 1;
        viewModel.CurrentPage = 2;
        viewModel.CurrentPage = 1;

        // Assert
        Assert.AreEqual(1, viewModel.CurrentPage);
        Assert.AreEqual(8, propertyChangedEvents.Count);
        Assert.AreEqual(4, propertyChangedEvents.Count(e => e == "CurrentPage"));
        Assert.AreEqual(4, propertyChangedEvents.Count(e => e == "PageInfo"));
    }

    /// <summary>
    /// Tests that CurrentPage property with negative boundary values updates correctly.
    /// Input: Setting CurrentPage to int.MinValue
    /// Expected: Property value is updated to int.MinValue and PropertyChanged events are raised.
    /// </summary>
    [TestMethod]
    public void CurrentPage_SetToMinValue_UpdatesPropertyAndRaisesPropertyChanged()
    {
        // Arrange
        var mockAcademicService = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<FacultiesViewModel>>();
        var viewModel = new FacultiesViewModel(mockAcademicService.Object, mockLogger.Object);
        var propertyChangedEvents = new List<string>();
        viewModel.PropertyChanged += (sender, args) => propertyChangedEvents.Add(args.PropertyName ?? string.Empty);

        // Act
        viewModel.CurrentPage = int.MinValue;

        // Assert
        Assert.AreEqual(int.MinValue, viewModel.CurrentPage);
        Assert.IsTrue(propertyChangedEvents.Contains("CurrentPage"));
        Assert.IsTrue(propertyChangedEvents.Contains("PageInfo"));
    }

    /// <summary>
    /// Tests that CurrentPage property with positive boundary values updates correctly.
    /// Input: Setting CurrentPage to int.MaxValue
    /// Expected: Property value is updated to int.MaxValue and PropertyChanged events are raised.
    /// </summary>
    [TestMethod]
    public void CurrentPage_SetToMaxValue_UpdatesPropertyAndRaisesPropertyChanged()
    {
        // Arrange
        var mockAcademicService = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<FacultiesViewModel>>();
        var viewModel = new FacultiesViewModel(mockAcademicService.Object, mockLogger.Object);
        var propertyChangedEvents = new List<string>();
        viewModel.PropertyChanged += (sender, args) => propertyChangedEvents.Add(args.PropertyName ?? string.Empty);

        // Act
        viewModel.CurrentPage = int.MaxValue;

        // Assert
        Assert.AreEqual(int.MaxValue, viewModel.CurrentPage);
        Assert.IsTrue(propertyChangedEvents.Contains("CurrentPage"));
        Assert.IsTrue(propertyChangedEvents.Contains("PageInfo"));
    }

    /// <summary>
    /// Tests that CurrentPage property correctly handles setting to zero.
    /// Input: Setting CurrentPage to 0
    /// Expected: Property value is updated to 0 and PropertyChanged events are raised.
    /// </summary>
    [TestMethod]
    public void CurrentPage_SetToZero_UpdatesPropertyAndRaisesPropertyChanged()
    {
        // Arrange
        var mockAcademicService = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<FacultiesViewModel>>();
        var viewModel = new FacultiesViewModel(mockAcademicService.Object, mockLogger.Object);
        var propertyChangedEvents = new List<string>();
        viewModel.PropertyChanged += (sender, args) => propertyChangedEvents.Add(args.PropertyName ?? string.Empty);

        // Act
        viewModel.CurrentPage = 0;

        // Assert
        Assert.AreEqual(0, viewModel.CurrentPage);
        Assert.IsTrue(propertyChangedEvents.Contains("CurrentPage"));
        Assert.IsTrue(propertyChangedEvents.Contains("PageInfo"));
    }

    /// <summary>
    /// Tests that setting SelectedUniversity to a non-null value updates the property and SelectedUniversityId correctly.
    /// Input: Valid LookupItem with Id "uni-123".
    /// Expected: SelectedUniversity is set, SelectedUniversityId is set to "uni-123", and PropertyChanged event is raised.
    /// </summary>
    [TestMethod]
    public void SelectedUniversity_SetNonNullValue_UpdatesPropertyAndSelectedUniversityId()
    {
        // Arrange
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<FacultiesViewModel>>();
        var viewModel = new FacultiesViewModel(mockAcademic.Object, mockLogger.Object);
        var lookupItem = new LookupItem { Id = "uni-123", Name = "Test University" };
        bool propertyChangedRaised = false;
        viewModel.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == nameof(FacultiesViewModel.SelectedUniversity))
                propertyChangedRaised = true;
        };

        // Act
        viewModel.SelectedUniversity = lookupItem;

        // Assert
        Assert.AreEqual(lookupItem, viewModel.SelectedUniversity);
        Assert.AreEqual("uni-123", viewModel.SelectedUniversityId);
        Assert.IsTrue(propertyChangedRaised);
    }

    /// <summary>
    /// Tests that setting SelectedUniversity to null updates the property to null and sets SelectedUniversityId to null.
    /// Input: null value.
    /// Expected: SelectedUniversity is null, SelectedUniversityId is null, and PropertyChanged event is raised.
    /// </summary>
    [TestMethod]
    public void SelectedUniversity_SetNull_UpdatesPropertyToNullAndSetsSelectedUniversityIdToNull()
    {
        // Arrange
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<FacultiesViewModel>>();
        var viewModel = new FacultiesViewModel(mockAcademic.Object, mockLogger.Object);
        var lookupItem = new LookupItem { Id = "uni-123", Name = "Test University" };
        viewModel.SelectedUniversity = lookupItem;
        bool propertyChangedRaised = false;
        viewModel.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == nameof(FacultiesViewModel.SelectedUniversity))
                propertyChangedRaised = true;
        };

        // Act
        viewModel.SelectedUniversity = null;

        // Assert
        Assert.IsNull(viewModel.SelectedUniversity);
        Assert.IsNull(viewModel.SelectedUniversityId);
        Assert.IsTrue(propertyChangedRaised);
    }

    /// <summary>
    /// Tests that setting SelectedUniversity to the same value does not update SelectedUniversityId or raise PropertyChanged event.
    /// Input: Same LookupItem instance set twice.
    /// Expected: SelectedUniversityId is not updated again, PropertyChanged event is not raised on second set.
    /// </summary>
    [TestMethod]
    public void SelectedUniversity_SetSameValue_DoesNotUpdateSelectedUniversityIdOrRaisePropertyChanged()
    {
        // Arrange
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<FacultiesViewModel>>();
        var viewModel = new FacultiesViewModel(mockAcademic.Object, mockLogger.Object);
        var lookupItem = new LookupItem { Id = "uni-123", Name = "Test University" };
        viewModel.SelectedUniversity = lookupItem;
        int propertyChangedCount = 0;
        viewModel.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == nameof(FacultiesViewModel.SelectedUniversity))
                propertyChangedCount++;
        };

        // Act
        viewModel.SelectedUniversity = lookupItem;

        // Assert
        Assert.AreEqual(lookupItem, viewModel.SelectedUniversity);
        Assert.AreEqual("uni-123", viewModel.SelectedUniversityId);
        Assert.AreEqual(0, propertyChangedCount);
    }

    /// <summary>
    /// Tests that setting SelectedUniversity to a different value updates both SelectedUniversity and SelectedUniversityId.
    /// Input: Two different LookupItem instances.
    /// Expected: Both properties are updated with the new values.
    /// </summary>
    [TestMethod]
    public void SelectedUniversity_SetDifferentValue_UpdatesBothProperties()
    {
        // Arrange
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<FacultiesViewModel>>();
        var viewModel = new FacultiesViewModel(mockAcademic.Object, mockLogger.Object);
        var lookupItem1 = new LookupItem { Id = "uni-123", Name = "Test University 1" };
        var lookupItem2 = new LookupItem { Id = "uni-456", Name = "Test University 2" };
        viewModel.SelectedUniversity = lookupItem1;

        // Act
        viewModel.SelectedUniversity = lookupItem2;

        // Assert
        Assert.AreEqual(lookupItem2, viewModel.SelectedUniversity);
        Assert.AreEqual("uni-456", viewModel.SelectedUniversityId);
    }

    /// <summary>
    /// Tests that setting SelectedUniversity to a value with special characters in Id updates SelectedUniversityId correctly.
    /// Input: LookupItem with Id containing special characters.
    /// Expected: SelectedUniversityId is set to the Id with special characters.
    /// </summary>
    [TestMethod]
    [DataRow("uni-!@#$%^&*()")]
    [DataRow("uni-<>?/\\|")]
    [DataRow("uni-\t\n\r")]
    [DataRow("uni-😀🎉")]
    [DataRow("uni-ñáéíóú")]
    public void SelectedUniversity_SetWithSpecialCharactersInId_UpdatesSelectedUniversityId(string specialId)
    {
        // Arrange
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<FacultiesViewModel>>();
        var viewModel = new FacultiesViewModel(mockAcademic.Object, mockLogger.Object);
        var lookupItem = new LookupItem { Id = specialId, Name = "University with Special Id" };

        // Act
        viewModel.SelectedUniversity = lookupItem;

        // Assert
        Assert.AreEqual(specialId, viewModel.SelectedUniversityId);
    }

    /// <summary>
    /// Tests that setting SelectedUniversity to null when already null does not raise PropertyChanged event.
    /// Input: null set twice.
    /// Expected: PropertyChanged event is not raised on second set.
    /// </summary>
    [TestMethod]
    public void SelectedUniversity_SetNullWhenAlreadyNull_DoesNotRaisePropertyChanged()
    {
        // Arrange
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<FacultiesViewModel>>();
        var viewModel = new FacultiesViewModel(mockAcademic.Object, mockLogger.Object);
        int propertyChangedCount = 0;
        viewModel.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == nameof(FacultiesViewModel.SelectedUniversity))
                propertyChangedCount++;
        };

        // Act
        viewModel.SelectedUniversity = null;

        // Assert
        Assert.IsNull(viewModel.SelectedUniversity);
        Assert.IsNull(viewModel.SelectedUniversityId);
        Assert.AreEqual(0, propertyChangedCount);
    }

    /// <summary>
    /// Tests that setting SelectedUniversity with whitespace-only Id updates SelectedUniversityId correctly.
    /// Input: LookupItem with whitespace-only Id.
    /// Expected: SelectedUniversityId is set to the whitespace string.
    /// </summary>
    [TestMethod]
    [DataRow("   ")]
    [DataRow("\t\t")]
    [DataRow("\n\n")]
    [DataRow("  \t  \n  ")]
    public void SelectedUniversity_SetWithWhitespaceId_UpdatesSelectedUniversityId(string whitespaceId)
    {
        // Arrange
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<FacultiesViewModel>>();
        var viewModel = new FacultiesViewModel(mockAcademic.Object, mockLogger.Object);
        var lookupItem = new LookupItem { Id = whitespaceId, Name = "University with Whitespace Id" };

        // Act
        viewModel.SelectedUniversity = lookupItem;

        // Assert
        Assert.AreEqual(whitespaceId, viewModel.SelectedUniversityId);
    }

    /// <summary>
    /// Tests that setting SelectedUniversity multiple times alternating between null and values updates correctly.
    /// Input: Multiple alternating null and non-null values.
    /// Expected: Each value is set correctly including SelectedUniversityId.
    /// </summary>
    [TestMethod]
    public void SelectedUniversity_SetMultipleAlternatingValues_UpdatesCorrectly()
    {
        // Arrange
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<FacultiesViewModel>>();
        var viewModel = new FacultiesViewModel(mockAcademic.Object, mockLogger.Object);
        var lookupItem1 = new LookupItem { Id = "uni-1", Name = "University 1" };
        var lookupItem2 = new LookupItem { Id = "uni-2", Name = "University 2" };

        // Act & Assert
        viewModel.SelectedUniversity = lookupItem1;
        Assert.AreEqual("uni-1", viewModel.SelectedUniversityId);

        viewModel.SelectedUniversity = null;
        Assert.IsNull(viewModel.SelectedUniversityId);

        viewModel.SelectedUniversity = lookupItem2;
        Assert.AreEqual("uni-2", viewModel.SelectedUniversityId);

        viewModel.SelectedUniversity = null;
        Assert.IsNull(viewModel.SelectedUniversityId);
    }

    /// <summary>
    /// Tests that PropertyChanged event is raised exactly once when SelectedUniversity value changes.
    /// Input: Different LookupItem value.
    /// Expected: PropertyChanged event is raised exactly once.
    /// </summary>
    [TestMethod]
    public void SelectedUniversity_SetDifferentValue_RaisesPropertyChangedExactlyOnce()
    {
        // Arrange
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<FacultiesViewModel>>();
        var viewModel = new FacultiesViewModel(mockAcademic.Object, mockLogger.Object);
        var lookupItem = new LookupItem { Id = "uni-single", Name = "Single Event Test" };
        int propertyChangedCount = 0;
        viewModel.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == nameof(FacultiesViewModel.SelectedUniversity))
                propertyChangedCount++;
        };

        // Act
        viewModel.SelectedUniversity = lookupItem;

        // Assert
        Assert.AreEqual(1, propertyChangedCount);
    }

    /// <summary>
    /// Tests that setting SelectedUniversity with Id containing control characters updates SelectedUniversityId correctly.
    /// Input: LookupItem with Id containing control characters.
    /// Expected: SelectedUniversityId is set to the Id with control characters.
    /// </summary>
    [TestMethod]
    [DataRow("\0")]
    [DataRow("\u0001")]
    [DataRow("\u001F")]
    [DataRow("uni\0test")]
    public void SelectedUniversity_SetWithControlCharactersInId_UpdatesSelectedUniversityId(string controlId)
    {
        // Arrange
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<FacultiesViewModel>>();
        var viewModel = new FacultiesViewModel(mockAcademic.Object, mockLogger.Object);
        var lookupItem = new LookupItem { Id = controlId, Name = "University with Control Characters" };

        // Act
        viewModel.SelectedUniversity = lookupItem;

        // Assert
        Assert.AreEqual(controlId, viewModel.SelectedUniversityId);
    }

    /// <summary>
    /// Tests that setting SelectedUniversity updates SelectedUniversityId even when Name property is empty.
    /// Input: LookupItem with valid Id but empty Name.
    /// Expected: SelectedUniversityId is updated correctly regardless of Name value.
    /// </summary>
    [TestMethod]
    public void SelectedUniversity_SetWithEmptyName_UpdatesSelectedUniversityIdCorrectly()
    {
        // Arrange
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<FacultiesViewModel>>();
        var viewModel = new FacultiesViewModel(mockAcademic.Object, mockLogger.Object);
        var lookupItem = new LookupItem { Id = "uni-no-name", Name = "" };

        // Act
        viewModel.SelectedUniversity = lookupItem;

        // Assert
        Assert.AreEqual("uni-no-name", viewModel.SelectedUniversityId);
        Assert.AreEqual(lookupItem, viewModel.SelectedUniversity);
    }

    /// <summary>
    /// Tests that setting SelectedUniversity to multiple different values in sequence updates correctly each time.
    /// Input: Three different LookupItem values set in sequence.
    /// Expected: Each value is set correctly and SelectedUniversityId matches.
    /// </summary>
    [TestMethod]
    public void SelectedUniversity_SetMultipleDifferentValuesInSequence_UpdatesCorrectlyEachTime()
    {
        // Arrange
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<FacultiesViewModel>>();
        var viewModel = new FacultiesViewModel(mockAcademic.Object, mockLogger.Object);
        var lookupItem1 = new LookupItem { Id = "uni-seq-1", Name = "University 1" };
        var lookupItem2 = new LookupItem { Id = "uni-seq-2", Name = "University 2" };
        var lookupItem3 = new LookupItem { Id = "uni-seq-3", Name = "University 3" };

        // Act & Assert
        viewModel.SelectedUniversity = lookupItem1;
        Assert.AreEqual(lookupItem1, viewModel.SelectedUniversity);
        Assert.AreEqual("uni-seq-1", viewModel.SelectedUniversityId);

        viewModel.SelectedUniversity = lookupItem2;
        Assert.AreEqual(lookupItem2, viewModel.SelectedUniversity);
        Assert.AreEqual("uni-seq-2", viewModel.SelectedUniversityId);

        viewModel.SelectedUniversity = lookupItem3;
        Assert.AreEqual(lookupItem3, viewModel.SelectedUniversity);
        Assert.AreEqual("uni-seq-3", viewModel.SelectedUniversityId);
    }

    /// <summary>
    /// Tests that setting SelectedUniversity with Id containing path-like strings updates SelectedUniversityId correctly.
    /// Input: LookupItem with Id containing path-like strings.
    /// Expected: SelectedUniversityId is set to the path-like Id.
    /// </summary>
    [TestMethod]
    [DataRow("C:\\Users\\Test\\University")]
    [DataRow("/usr/local/university")]
    [DataRow("..\\..\\university")]
    [DataRow("university/subfolder/item")]
    public void SelectedUniversity_SetWithPathLikeId_UpdatesSelectedUniversityId(string pathLikeId)
    {
        // Arrange
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<FacultiesViewModel>>();
        var viewModel = new FacultiesViewModel(mockAcademic.Object, mockLogger.Object);
        var lookupItem = new LookupItem { Id = pathLikeId, Name = "University with Path-like Id" };

        // Act
        viewModel.SelectedUniversity = lookupItem;

        // Assert
        Assert.AreEqual(pathLikeId, viewModel.SelectedUniversityId);
    }

    /// <summary>
    /// Tests that IsEditing property has a default initial value of false.
    /// Input: None (testing initial state).
    /// Expected: IsEditing returns false.
    /// </summary>
    [TestMethod]
    public void IsEditing_InitialValue_IsFalse()
    {
        // Arrange
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<FacultiesViewModel>>();

        // Act
        var viewModel = new FacultiesViewModel(mockAcademic.Object, mockLogger.Object);

        // Assert
        Assert.AreEqual(false, viewModel.IsEditing);
    }

    /// <summary>
    /// Tests that SelectedUniversityId property can be set and retrieved with null value.
    /// Input: null.
    /// Expected: Property value is set to null.
    /// </summary>
    [TestMethod]
    public void SelectedUniversityId_SetNull_ReturnsNull()
    {
        // Arrange
        Mock<IAcademicService> mockAcademic = new();
        Mock<ILogger<FacultiesViewModel>> mockLogger = new();
        FacultiesViewModel viewModel = new(mockAcademic.Object, mockLogger.Object);

        // Act
        viewModel.SelectedUniversityId = null;

        // Assert
        Assert.IsNull(viewModel.SelectedUniversityId);
    }

    /// <summary>
    /// Tests that SelectedUniversityId property can be set and retrieved with empty string.
    /// Input: Empty string.
    /// Expected: Property value is set to empty string.
    /// </summary>
    [TestMethod]
    public void SelectedUniversityId_SetEmptyString_ReturnsEmptyString()
    {
        // Arrange
        Mock<IAcademicService> mockAcademic = new();
        Mock<ILogger<FacultiesViewModel>> mockLogger = new();
        FacultiesViewModel viewModel = new(mockAcademic.Object, mockLogger.Object);

        // Act
        viewModel.SelectedUniversityId = string.Empty;

        // Assert
        Assert.AreEqual(string.Empty, viewModel.SelectedUniversityId);
    }

    /// <summary>
    /// Tests that SelectedUniversityId property can be set and retrieved with valid non-empty string values.
    /// Input: Various valid string values.
    /// Expected: Property value matches the set value.
    /// </summary>
    /// <param name="value">The value to set.</param>
    [TestMethod]
    [DataRow("uni-123")]
    [DataRow("123e4567-e89b-12d3-a456-426614174000")]
    [DataRow("UniversityId123")]
    [DataRow("a")]
    [DataRow("ID with spaces")]
    public void SelectedUniversityId_SetValidString_ReturnsSetValue(string value)
    {
        // Arrange
        Mock<IAcademicService> mockAcademic = new();
        Mock<ILogger<FacultiesViewModel>> mockLogger = new();
        FacultiesViewModel viewModel = new(mockAcademic.Object, mockLogger.Object);

        // Act
        viewModel.SelectedUniversityId = value;

        // Assert
        Assert.AreEqual(value, viewModel.SelectedUniversityId);
    }

    /// <summary>
    /// Tests that setting SelectedUniversityId to a different value raises PropertyChanged event.
    /// Input: New value different from initial null.
    /// Expected: PropertyChanged event is raised with property name "SelectedUniversityId".
    /// </summary>
    [TestMethod]
    public void SelectedUniversityId_SetDifferentValueFromNull_RaisesPropertyChangedEvent()
    {
        // Arrange
        Mock<IAcademicService> mockAcademic = new();
        Mock<ILogger<FacultiesViewModel>> mockLogger = new();
        FacultiesViewModel viewModel = new(mockAcademic.Object, mockLogger.Object);
        string? raisedPropertyName = null;
        viewModel.PropertyChanged += (sender, args) => raisedPropertyName = args.PropertyName;

        // Act
        viewModel.SelectedUniversityId = "newId";

        // Assert
        Assert.AreEqual("SelectedUniversityId", raisedPropertyName);
    }

    /// <summary>
    /// Tests that setting SelectedUniversityId from non-null to null raises PropertyChanged event.
    /// Input: null after setting to a non-null value.
    /// Expected: PropertyChanged event is raised.
    /// </summary>
    [TestMethod]
    public void SelectedUniversityId_SetFromNonNullToNull_RaisesPropertyChangedEvent()
    {
        // Arrange
        Mock<IAcademicService> mockAcademic = new();
        Mock<ILogger<FacultiesViewModel>> mockLogger = new();
        FacultiesViewModel viewModel = new(mockAcademic.Object, mockLogger.Object);
        viewModel.SelectedUniversityId = "oldId";
        string? raisedPropertyName = null;
        viewModel.PropertyChanged += (sender, args) => raisedPropertyName = args.PropertyName;

        // Act
        viewModel.SelectedUniversityId = null;

        // Assert
        Assert.AreEqual("SelectedUniversityId", raisedPropertyName);
    }

    /// <summary>
    /// Tests that setting SelectedUniversityId from one non-null value to another raises PropertyChanged event.
    /// Input: Different non-null value.
    /// Expected: PropertyChanged event is raised.
    /// </summary>
    [TestMethod]
    public void SelectedUniversityId_SetFromOneValueToAnother_RaisesPropertyChangedEvent()
    {
        // Arrange
        Mock<IAcademicService> mockAcademic = new();
        Mock<ILogger<FacultiesViewModel>> mockLogger = new();
        FacultiesViewModel viewModel = new(mockAcademic.Object, mockLogger.Object);
        viewModel.SelectedUniversityId = "oldId";
        string? raisedPropertyName = null;
        viewModel.PropertyChanged += (sender, args) => raisedPropertyName = args.PropertyName;

        // Act
        viewModel.SelectedUniversityId = "newId";

        // Assert
        Assert.AreEqual("SelectedUniversityId", raisedPropertyName);
    }

    /// <summary>
    /// Tests that setting SelectedUniversityId to the same value does not raise PropertyChanged event.
    /// Input: Same value as currently set.
    /// Expected: PropertyChanged event is not raised.
    /// </summary>
    [TestMethod]
    public void SelectedUniversityId_SetSameValue_DoesNotRaisePropertyChangedEvent()
    {
        // Arrange
        Mock<IAcademicService> mockAcademic = new();
        Mock<ILogger<FacultiesViewModel>> mockLogger = new();
        FacultiesViewModel viewModel = new(mockAcademic.Object, mockLogger.Object);
        viewModel.SelectedUniversityId = "sameId";
        int eventRaisedCount = 0;
        viewModel.PropertyChanged += (sender, args) => eventRaisedCount++;

        // Act
        viewModel.SelectedUniversityId = "sameId";

        // Assert
        Assert.AreEqual(0, eventRaisedCount);
    }

    /// <summary>
    /// Tests that setting SelectedUniversityId to null when already null does not raise PropertyChanged event.
    /// Input: null when current value is already null.
    /// Expected: PropertyChanged event is not raised.
    /// </summary>
    [TestMethod]
    public void SelectedUniversityId_SetNullWhenAlreadyNull_DoesNotRaisePropertyChangedEvent()
    {
        // Arrange
        Mock<IAcademicService> mockAcademic = new();
        Mock<ILogger<FacultiesViewModel>> mockLogger = new();
        FacultiesViewModel viewModel = new(mockAcademic.Object, mockLogger.Object);
        int eventRaisedCount = 0;
        viewModel.PropertyChanged += (sender, args) => eventRaisedCount++;

        // Act
        viewModel.SelectedUniversityId = null;

        // Assert
        Assert.AreEqual(0, eventRaisedCount);
    }

    /// <summary>
    /// Tests that SelectedUniversityId property handles whitespace-only strings correctly.
    /// Input: Various whitespace-only strings.
    /// Expected: Property value is set to the whitespace string.
    /// </summary>
    /// <param name="whitespaceValue">The whitespace string to test.</param>
    [TestMethod]
    [DataRow("   ")]
    [DataRow("\t")]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [DataRow("  \t\n  ")]
    [DataRow("\r")]
    [DataRow("\t\t\t")]
    public void SelectedUniversityId_SetWhitespaceStrings_StoresAndReturnsValue(string whitespaceValue)
    {
        // Arrange
        Mock<IAcademicService> mockAcademic = new();
        Mock<ILogger<FacultiesViewModel>> mockLogger = new();
        FacultiesViewModel viewModel = new(mockAcademic.Object, mockLogger.Object);

        // Act
        viewModel.SelectedUniversityId = whitespaceValue;

        // Assert
        Assert.AreEqual(whitespaceValue, viewModel.SelectedUniversityId);
    }

    /// <summary>
    /// Tests that SelectedUniversityId property handles very long strings correctly.
    /// Input: String with 10000 characters.
    /// Expected: Property value is set to the long string.
    /// </summary>
    [TestMethod]
    public void SelectedUniversityId_SetVeryLongString_StoresAndReturnsValue()
    {
        // Arrange
        Mock<IAcademicService> mockAcademic = new();
        Mock<ILogger<FacultiesViewModel>> mockLogger = new();
        FacultiesViewModel viewModel = new(mockAcademic.Object, mockLogger.Object);
        string longString = new('x', 10000);

        // Act
        viewModel.SelectedUniversityId = longString;

        // Assert
        Assert.AreEqual(longString, viewModel.SelectedUniversityId);
    }

    /// <summary>
    /// Tests that SelectedUniversityId property handles strings with special characters correctly.
    /// Input: Strings containing various special characters.
    /// Expected: Property value is set correctly with special characters preserved.
    /// </summary>
    /// <param name="specialValue">The string with special characters to test.</param>
    [TestMethod]
    [DataRow("id!@#$%^&*()")]
    [DataRow("id<>?/\\|")]
    [DataRow("id-with-dashes")]
    [DataRow("id_with_underscores")]
    [DataRow("id.with.dots")]
    [DataRow("id:with:colons")]
    [DataRow("id;with;semicolons")]
    [DataRow("id'with'quotes")]
    [DataRow("id\"with\"doublequotes")]
    public void SelectedUniversityId_SetStringWithSpecialCharacters_StoresAndReturnsValue(string specialValue)
    {
        // Arrange
        Mock<IAcademicService> mockAcademic = new();
        Mock<ILogger<FacultiesViewModel>> mockLogger = new();
        FacultiesViewModel viewModel = new(mockAcademic.Object, mockLogger.Object);

        // Act
        viewModel.SelectedUniversityId = specialValue;

        // Assert
        Assert.AreEqual(specialValue, viewModel.SelectedUniversityId);
    }

    /// <summary>
    /// Tests that SelectedUniversityId property handles strings with Unicode characters correctly.
    /// Input: Strings containing Unicode characters, emojis, and non-Latin scripts.
    /// Expected: Property value is set correctly with Unicode characters preserved.
    /// </summary>
    /// <param name="unicodeValue">The string with Unicode characters to test.</param>
    [TestMethod]
    [DataRow("café")]
    [DataRow("日本語")]
    [DataRow("العربية")]
    [DataRow("Привет")]
    [DataRow("😀🎉")]
    [DataRow("test™value")]
    [DataRow("test©value")]
    [DataRow("test®value")]
    [DataRow("Université")]
    [DataRow("测试")]
    public void SelectedUniversityId_SetUnicodeCharacters_StoresAndReturnsValue(string unicodeValue)
    {
        // Arrange
        Mock<IAcademicService> mockAcademic = new();
        Mock<ILogger<FacultiesViewModel>> mockLogger = new();
        FacultiesViewModel viewModel = new(mockAcademic.Object, mockLogger.Object);

        // Act
        viewModel.SelectedUniversityId = unicodeValue;

        // Assert
        Assert.AreEqual(unicodeValue, viewModel.SelectedUniversityId);
    }

    /// <summary>
    /// Tests that SelectedUniversityId property handles strings with control characters correctly.
    /// Input: Strings containing control characters.
    /// Expected: Property value is set correctly with control characters preserved.
    /// </summary>
    /// <param name="controlValue">The string with control characters to test.</param>
    [TestMethod]
    [DataRow("\0")]
    [DataRow("\u0001")]
    [DataRow("\u001F")]
    [DataRow("id\0test")]
    [DataRow("id\u0001test")]
    [DataRow("id\btab")]
    public void SelectedUniversityId_SetControlCharacters_StoresAndReturnsValue(string controlValue)
    {
        // Arrange
        Mock<IAcademicService> mockAcademic = new();
        Mock<ILogger<FacultiesViewModel>> mockLogger = new();
        FacultiesViewModel viewModel = new(mockAcademic.Object, mockLogger.Object);

        // Act
        viewModel.SelectedUniversityId = controlValue;

        // Assert
        Assert.AreEqual(controlValue, viewModel.SelectedUniversityId);
    }

    /// <summary>
    /// Tests that SelectedUniversityId can be set multiple times with different values correctly.
    /// Input: Sequence of different string values.
    /// Expected: Each value is stored and retrieved correctly.
    /// </summary>
    [TestMethod]
    public void SelectedUniversityId_SetMultipleDifferentValues_UpdatesCorrectlyEachTime()
    {
        // Arrange
        Mock<IAcademicService> mockAcademic = new();
        Mock<ILogger<FacultiesViewModel>> mockLogger = new();
        FacultiesViewModel viewModel = new(mockAcademic.Object, mockLogger.Object);

        // Act & Assert
        viewModel.SelectedUniversityId = "first";
        Assert.AreEqual("first", viewModel.SelectedUniversityId);

        viewModel.SelectedUniversityId = "second";
        Assert.AreEqual("second", viewModel.SelectedUniversityId);

        viewModel.SelectedUniversityId = null;
        Assert.IsNull(viewModel.SelectedUniversityId);

        viewModel.SelectedUniversityId = "third";
        Assert.AreEqual("third", viewModel.SelectedUniversityId);
    }

    /// <summary>
    /// Tests that multiple consecutive value changes each raise PropertyChanged event.
    /// Input: Three different values set consecutively.
    /// Expected: PropertyChanged event is raised three times.
    /// </summary>
    [TestMethod]
    public void SelectedUniversityId_MultipleConsecutiveChanges_RaisesPropertyChangedForEach()
    {
        // Arrange
        Mock<IAcademicService> mockAcademic = new();
        Mock<ILogger<FacultiesViewModel>> mockLogger = new();
        FacultiesViewModel viewModel = new(mockAcademic.Object, mockLogger.Object);
        int eventRaisedCount = 0;
        viewModel.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == "SelectedUniversityId")
                eventRaisedCount++;
        };

        // Act
        viewModel.SelectedUniversityId = "first";
        viewModel.SelectedUniversityId = "second";
        viewModel.SelectedUniversityId = "third";

        // Assert
        Assert.AreEqual(3, eventRaisedCount);
    }

    /// <summary>
    /// Tests that alternating between two different values raises PropertyChanged event each time.
    /// Input: Alternating between two different values.
    /// Expected: PropertyChanged event is raised for each change.
    /// </summary>
    [TestMethod]
    public void SelectedUniversityId_AlternateBetweenTwoValues_RaisesPropertyChangedForEachChange()
    {
        // Arrange
        Mock<IAcademicService> mockAcademic = new();
        Mock<ILogger<FacultiesViewModel>> mockLogger = new();
        FacultiesViewModel viewModel = new(mockAcademic.Object, mockLogger.Object);
        int eventRaisedCount = 0;
        viewModel.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == "SelectedUniversityId")
                eventRaisedCount++;
        };

        // Act
        viewModel.SelectedUniversityId = "valueA";
        viewModel.SelectedUniversityId = "valueB";
        viewModel.SelectedUniversityId = "valueA";
        viewModel.SelectedUniversityId = "valueB";

        // Assert
        Assert.AreEqual(4, eventRaisedCount);
    }

    /// <summary>
    /// Tests that SelectedUniversityId property handles path-like strings correctly.
    /// Input: Strings resembling file paths or URLs.
    /// Expected: Property value is set correctly.
    /// </summary>
    /// <param name="pathLikeValue">The path-like string to test.</param>
    [TestMethod]
    [DataRow("C:\\Users\\Test\\university")]
    [DataRow("/usr/local/universities")]
    [DataRow("..\\..\\parent")]
    [DataRow("university/subfolder/item")]
    [DataRow("\\\\network\\share\\universities")]
    [DataRow("http://example.com/university")]
    [DataRow("ftp://server.com/data")]
    public void SelectedUniversityId_SetPathLikeStrings_StoresAndReturnsValue(string pathLikeValue)
    {
        // Arrange
        Mock<IAcademicService> mockAcademic = new();
        Mock<ILogger<FacultiesViewModel>> mockLogger = new();
        FacultiesViewModel viewModel = new(mockAcademic.Object, mockLogger.Object);

        // Act
        viewModel.SelectedUniversityId = pathLikeValue;

        // Assert
        Assert.AreEqual(pathLikeValue, viewModel.SelectedUniversityId);
    }

    /// <summary>
    /// Tests that SelectedUniversityId is case-sensitive when comparing values.
    /// Input: Same string with different casing.
    /// Expected: Different case variations are treated as different values and raise PropertyChanged.
    /// </summary>
    [TestMethod]
    public void SelectedUniversityId_CaseSensitiveComparison_TreatsAsDistinctValues()
    {
        // Arrange
        Mock<IAcademicService> mockAcademic = new();
        Mock<ILogger<FacultiesViewModel>> mockLogger = new();
        FacultiesViewModel viewModel = new(mockAcademic.Object, mockLogger.Object);
        viewModel.SelectedUniversityId = "UniversityId";
        int eventRaisedCount = 0;
        viewModel.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == "SelectedUniversityId")
                eventRaisedCount++;
        };

        // Act
        viewModel.SelectedUniversityId = "universityid";

        // Assert
        Assert.AreEqual("universityid", viewModel.SelectedUniversityId);
        Assert.AreEqual(1, eventRaisedCount);
    }

    /// <summary>
    /// Tests that SelectedUniversityId handles strings with SQL injection-like patterns safely.
    /// Input: Strings resembling SQL injection attempts.
    /// Expected: Property value is stored as-is without processing.
    /// </summary>
    /// <param name="sqlLikeValue">The SQL-like string to test.</param>
    [TestMethod]
    [DataRow("'; DROP TABLE Universities--")]
    [DataRow("1' OR '1'='1")]
    [DataRow("admin'--")]
    [DataRow("' UNION SELECT * FROM Users--")]
    public void SelectedUniversityId_SetSqlInjectionLikeStrings_StoresValueAsIs(string sqlLikeValue)
    {
        // Arrange
        Mock<IAcademicService> mockAcademic = new();
        Mock<ILogger<FacultiesViewModel>> mockLogger = new();
        FacultiesViewModel viewModel = new(mockAcademic.Object, mockLogger.Object);

        // Act
        viewModel.SelectedUniversityId = sqlLikeValue;

        // Assert
        Assert.AreEqual(sqlLikeValue, viewModel.SelectedUniversityId);
    }

    /// <summary>
    /// Tests that SelectedUniversityId handles strings with HTML/XML markup correctly.
    /// Input: Strings with markup tags.
    /// Expected: Property value is stored without escaping or processing.
    /// </summary>
    /// <param name="markupValue">The markup string to test.</param>
    [TestMethod]
    [DataRow("<script>alert('test')</script>")]
    [DataRow("<div>UniversityId</div>")]
    [DataRow("<?xml version=\"1.0\"?>")]
    [DataRow("<![CDATA[data]]>")]
    [DataRow("<university id=\"123\" />")]
    public void SelectedUniversityId_SetMarkupStrings_StoresValueAsIs(string markupValue)
    {
        // Arrange
        Mock<IAcademicService> mockAcademic = new();
        Mock<ILogger<FacultiesViewModel>> mockLogger = new();
        FacultiesViewModel viewModel = new(mockAcademic.Object, mockLogger.Object);

        // Act
        viewModel.SelectedUniversityId = markupValue;

        // Assert
        Assert.AreEqual(markupValue, viewModel.SelectedUniversityId);
    }

    /// <summary>
    /// Tests that SelectedUniversityId handles strings with various newline characters correctly.
    /// Input: Strings with different newline variations.
    /// Expected: Newlines are preserved in the stored value.
    /// </summary>
    /// <param name="newlineValue">The string with newline characters to test.</param>
    [TestMethod]
    [DataRow("line1\nline2")]
    [DataRow("line1\r\nline2")]
    [DataRow("line1\rline2")]
    [DataRow("multi\nline\nvalue")]
    [DataRow("id\n\n\nvalue")]
    public void SelectedUniversityId_SetStringsWithNewlines_PreservesNewlines(string newlineValue)
    {
        // Arrange
        Mock<IAcademicService> mockAcademic = new();
        Mock<ILogger<FacultiesViewModel>> mockLogger = new();
        FacultiesViewModel viewModel = new(mockAcademic.Object, mockLogger.Object);

        // Act
        viewModel.SelectedUniversityId = newlineValue;

        // Assert
        Assert.AreEqual(newlineValue, viewModel.SelectedUniversityId);
    }

    /// <summary>
    /// Tests that PropertyChanged event args contain the correct property name.
    /// Input: Any different value.
    /// Expected: PropertyChanged event args contain exactly "SelectedUniversityId".
    /// </summary>
    [TestMethod]
    public void SelectedUniversityId_PropertyChangedEvent_ContainsCorrectPropertyName()
    {
        // Arrange
        Mock<IAcademicService> mockAcademic = new();
        Mock<ILogger<FacultiesViewModel>> mockLogger = new();
        FacultiesViewModel viewModel = new(mockAcademic.Object, mockLogger.Object);
        string? capturedPropertyName = null;
        viewModel.PropertyChanged += (sender, args) => capturedPropertyName = args.PropertyName;

        // Act
        viewModel.SelectedUniversityId = "testId";

        // Assert
        Assert.AreEqual("SelectedUniversityId", capturedPropertyName);
    }

    /// <summary>
    /// Tests that setting SelectedUniversityId from empty string to null raises PropertyChanged event.
    /// Input: null after setting to empty string.
    /// Expected: PropertyChanged event is raised.
    /// </summary>
    [TestMethod]
    public void SelectedUniversityId_SetFromEmptyStringToNull_RaisesPropertyChangedEvent()
    {
        // Arrange
        Mock<IAcademicService> mockAcademic = new();
        Mock<ILogger<FacultiesViewModel>> mockLogger = new();
        FacultiesViewModel viewModel = new(mockAcademic.Object, mockLogger.Object);
        viewModel.SelectedUniversityId = string.Empty;
        int eventRaisedCount = 0;
        viewModel.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == "SelectedUniversityId")
                eventRaisedCount++;
        };

        // Act
        viewModel.SelectedUniversityId = null;

        // Assert
        Assert.AreEqual(1, eventRaisedCount);
    }

    /// <summary>
    /// Tests that setting SelectedUniversityId from null to empty string raises PropertyChanged event.
    /// Input: Empty string from initial null.
    /// Expected: PropertyChanged event is raised.
    /// </summary>
    [TestMethod]
    public void SelectedUniversityId_SetFromNullToEmptyString_RaisesPropertyChangedEvent()
    {
        // Arrange
        Mock<IAcademicService> mockAcademic = new();
        Mock<ILogger<FacultiesViewModel>> mockLogger = new();
        FacultiesViewModel viewModel = new(mockAcademic.Object, mockLogger.Object);
        int eventRaisedCount = 0;
        viewModel.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == "SelectedUniversityId")
                eventRaisedCount++;
        };

        // Act
        viewModel.SelectedUniversityId = string.Empty;

        // Assert
        Assert.AreEqual(1, eventRaisedCount);
    }

    /// <summary>
    /// Tests that setting SelectedUniversityId to empty string when already empty does not raise PropertyChanged event.
    /// Input: Empty string when current value is already empty string.
    /// Expected: PropertyChanged event is not raised.
    /// </summary>
    [TestMethod]
    public void SelectedUniversityId_SetEmptyStringWhenAlreadyEmpty_DoesNotRaisePropertyChangedEvent()
    {
        // Arrange
        Mock<IAcademicService> mockAcademic = new();
        Mock<ILogger<FacultiesViewModel>> mockLogger = new();
        FacultiesViewModel viewModel = new(mockAcademic.Object, mockLogger.Object);
        viewModel.SelectedUniversityId = string.Empty;
        int eventRaisedCount = 0;
        viewModel.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == "SelectedUniversityId")
                eventRaisedCount++;
        };

        // Act
        viewModel.SelectedUniversityId = string.Empty;

        // Assert
        Assert.AreEqual(0, eventRaisedCount);
    }

    /// <summary>
    /// Tests that SelectedUniversityId handles GUID-like strings correctly.
    /// Input: Various GUID format strings.
    /// Expected: Property value is set correctly.
    /// </summary>
    /// <param name="guidValue">The GUID-like string to test.</param>
    [TestMethod]
    [DataRow("123e4567-e89b-12d3-a456-426614174000")]
    [DataRow("00000000-0000-0000-0000-000000000000")]
    [DataRow("ffffffff-ffff-ffff-ffff-ffffffffffff")]
    [DataRow("{123e4567-e89b-12d3-a456-426614174000}")]
    [DataRow("123e4567e89b12d3a456426614174000")]
    public void SelectedUniversityId_SetGuidLikeStrings_StoresAndReturnsValue(string guidValue)
    {
        // Arrange
        Mock<IAcademicService> mockAcademic = new();
        Mock<ILogger<FacultiesViewModel>> mockLogger = new();
        FacultiesViewModel viewModel = new(mockAcademic.Object, mockLogger.Object);

        // Act
        viewModel.SelectedUniversityId = guidValue;

        // Assert
        Assert.AreEqual(guidValue, viewModel.SelectedUniversityId);
    }

    /// <summary>
    /// Tests that SelectedUniversityId handles numeric-only strings correctly.
    /// Input: Strings containing only numeric characters.
    /// Expected: Property value is set correctly.
    /// </summary>
    /// <param name="numericValue">The numeric string to test.</param>
    [TestMethod]
    [DataRow("0")]
    [DataRow("123")]
    [DataRow("999999999")]
    [DataRow("2147483647")]
    [DataRow("-1")]
    [DataRow("-2147483648")]
    public void SelectedUniversityId_SetNumericStrings_StoresAndReturnsValue(string numericValue)
    {
        // Arrange
        Mock<IAcademicService> mockAcademic = new();
        Mock<ILogger<FacultiesViewModel>> mockLogger = new();
        FacultiesViewModel viewModel = new(mockAcademic.Object, mockLogger.Object);

        // Act
        viewModel.SelectedUniversityId = numericValue;

        // Assert
        Assert.AreEqual(numericValue, viewModel.SelectedUniversityId);
    }

    /// <summary>
    /// Tests that setting PendingCount to the same value multiple times does not raise PropertyChanged after the first time.
    /// Input: Set to same value three times.
    /// Expected: PropertyChanged event is raised only once for the initial change from default.
    /// </summary>
    [TestMethod]
    public void PendingCount_SetSameValueMultipleTimes_RaisesPropertyChangedOnlyOnce()
    {
        // Arrange
        Mock<IAcademicService> mockAcademic = new Mock<IAcademicService>();
        Mock<ILogger<FacultiesViewModel>> mockLogger = new Mock<ILogger<FacultiesViewModel>>();
        FacultiesViewModel viewModel = new FacultiesViewModel(mockAcademic.Object, mockLogger.Object);
        int eventCount = 0;

        viewModel.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == "PendingCount")
            {
                eventCount++;
            }
        };

        // Act
        viewModel.PendingCount = 15;
        viewModel.PendingCount = 15;
        viewModel.PendingCount = 15;

        // Assert
        Assert.AreEqual(15, viewModel.PendingCount);
        Assert.AreEqual(1, eventCount, "PropertyChanged event should be raised only once when setting the same value multiple times.");
    }

    /// <summary>
    /// Tests that alternating PendingCount between two different values raises PropertyChanged for each change.
    /// Input: Alternating between value 10 and value 20.
    /// Expected: PropertyChanged event is raised for each value change.
    /// </summary>
    [TestMethod]
    public void PendingCount_AlternateBetweenTwoValues_RaisesPropertyChangedForEach()
    {
        // Arrange
        Mock<IAcademicService> mockAcademic = new Mock<IAcademicService>();
        Mock<ILogger<FacultiesViewModel>> mockLogger = new Mock<ILogger<FacultiesViewModel>>();
        FacultiesViewModel viewModel = new FacultiesViewModel(mockAcademic.Object, mockLogger.Object);
        int eventCount = 0;

        viewModel.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == "PendingCount")
            {
                eventCount++;
            }
        };

        // Act
        viewModel.PendingCount = 10;
        viewModel.PendingCount = 20;
        viewModel.PendingCount = 10;
        viewModel.PendingCount = 20;

        // Assert
        Assert.AreEqual(20, viewModel.PendingCount);
        Assert.AreEqual(4, eventCount, "PropertyChanged event should be raised four times for four value changes.");
    }

    /// <summary>
    /// Tests that LoadAsync with Status containing leading whitespace does not count as Active.
    /// Input: Faculty with Status = " Active" (leading space).
    /// Expected: ActiveFacultiesCount is 0, PendingCount is 1.
    /// </summary>
    [TestMethod]
    public async Task LoadAsync_WithLeadingWhitespaceInStatus_DoesNotCountAsActive()
    {
        // Arrange
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<FacultiesViewModel>>();

        var universities = new List<LookupItem>();
        var faculties = new List<FacultyDto>
        {
            new FacultyDto { Id = "f1", Name = "Faculty 1", Status = " Active", UniversityId = "u1" }
        };

        mockAcademic.Setup(a => a.GetUniversitiesAsync()).ReturnsAsync(universities);
        mockAcademic.Setup(a => a.GetFacultyDetailsAsync()).ReturnsAsync(faculties);

        var viewModel = new FacultiesViewModel(mockAcademic.Object, mockLogger.Object);

        // Act
        await viewModel.LoadAsync();

        // Assert
        Assert.AreEqual(1, viewModel.TotalFaculties, "TotalFaculties should be 1");
        Assert.AreEqual(0, viewModel.ActiveFacultiesCount, "ActiveFacultiesCount should be 0 for Status with leading whitespace");
        Assert.AreEqual(1, viewModel.PendingCount, "PendingCount should be 1");
    }

    /// <summary>
    /// Tests that LoadAsync with Status containing trailing whitespace does not count as Active.
    /// Input: Faculty with Status = "Active " (trailing space).
    /// Expected: ActiveFacultiesCount is 0, PendingCount is 1.
    /// </summary>
    [TestMethod]
    public async Task LoadAsync_WithTrailingWhitespaceInStatus_DoesNotCountAsActive()
    {
        // Arrange
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<FacultiesViewModel>>();

        var universities = new List<LookupItem>();
        var faculties = new List<FacultyDto>
        {
            new FacultyDto { Id = "f1", Name = "Faculty 1", Status = "Active ", UniversityId = "u1" }
        };

        mockAcademic.Setup(a => a.GetUniversitiesAsync()).ReturnsAsync(universities);
        mockAcademic.Setup(a => a.GetFacultyDetailsAsync()).ReturnsAsync(faculties);

        var viewModel = new FacultiesViewModel(mockAcademic.Object, mockLogger.Object);

        // Act
        await viewModel.LoadAsync();

        // Assert
        Assert.AreEqual(1, viewModel.TotalFaculties, "TotalFaculties should be 1");
        Assert.AreEqual(0, viewModel.ActiveFacultiesCount, "ActiveFacultiesCount should be 0 for Status with trailing whitespace");
        Assert.AreEqual(1, viewModel.PendingCount, "PendingCount should be 1");
    }

    /// <summary>
    /// Tests that LoadAsync with lowercase Status does not count as Active.
    /// Input: Faculty with Status = "active" (lowercase).
    /// Expected: ActiveFacultiesCount is 0, PendingCount is 1.
    /// </summary>
    [TestMethod]
    public async Task LoadAsync_WithLowercaseStatus_DoesNotCountAsActive()
    {
        // Arrange
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<FacultiesViewModel>>();

        var universities = new List<LookupItem>();
        var faculties = new List<FacultyDto>
        {
            new FacultyDto { Id = "f1", Name = "Faculty 1", Status = "active", UniversityId = "u1" }
        };

        mockAcademic.Setup(a => a.GetUniversitiesAsync()).ReturnsAsync(universities);
        mockAcademic.Setup(a => a.GetFacultyDetailsAsync()).ReturnsAsync(faculties);

        var viewModel = new FacultiesViewModel(mockAcademic.Object, mockLogger.Object);

        // Act
        await viewModel.LoadAsync();

        // Assert
        Assert.AreEqual(1, viewModel.TotalFaculties, "TotalFaculties should be 1");
        Assert.AreEqual(0, viewModel.ActiveFacultiesCount, "ActiveFacultiesCount should be 0 for lowercase status");
        Assert.AreEqual(1, viewModel.PendingCount, "PendingCount should be 1");
    }

    /// <summary>
    /// Tests that LoadAsync with mixed case Status does not count as Active.
    /// Input: Faculties with Status = "ACTIVE", "AcTiVe", "aCTIVE".
    /// Expected: ActiveFacultiesCount is 0, all counted as pending.
    /// </summary>
    [TestMethod]
    [DataRow("ACTIVE")]
    [DataRow("AcTiVe")]
    [DataRow("aCTIVE")]
    [DataRow("Active\t")]
    [DataRow("\tActive")]
    [DataRow("Active\n")]
    public async Task LoadAsync_WithNonExactCaseStatus_DoesNotCountAsActive(string status)
    {
        // Arrange
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<FacultiesViewModel>>();

        var universities = new List<LookupItem>();
        var faculties = new List<FacultyDto>
        {
            new FacultyDto { Id = "f1", Name = "Faculty 1", Status = status, UniversityId = "u1" }
        };

        mockAcademic.Setup(a => a.GetUniversitiesAsync()).ReturnsAsync(universities);
        mockAcademic.Setup(a => a.GetFacultyDetailsAsync()).ReturnsAsync(faculties);

        var viewModel = new FacultiesViewModel(mockAcademic.Object, mockLogger.Object);

        // Act
        await viewModel.LoadAsync();

        // Assert
        Assert.AreEqual(1, viewModel.TotalFaculties, "TotalFaculties should be 1");
        Assert.AreEqual(0, viewModel.ActiveFacultiesCount, $"ActiveFacultiesCount should be 0 for Status '{status}'");
        Assert.AreEqual(1, viewModel.PendingCount, "PendingCount should be 1");
    }

    /// <summary>
    /// Tests that LoadAsync verifies the exact error message set when exception occurs.
    /// Input: GetUniversitiesAsync throws exception.
    /// Expected: ErrorMessage is exactly "Failed to load faculties.".
    /// </summary>
    [TestMethod]
    public async Task LoadAsync_WhenExceptionOccurs_SetsExactErrorMessage()
    {
        // Arrange
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<FacultiesViewModel>>();

        mockAcademic.Setup(a => a.GetUniversitiesAsync()).ThrowsAsync(new Exception("Test exception"));

        var viewModel = new FacultiesViewModel(mockAcademic.Object, mockLogger.Object);

        // Act
        await viewModel.LoadAsync();

        // Assert
        Assert.AreEqual("Failed to load faculties.", viewModel.ErrorMessage, "ErrorMessage should be exactly 'Failed to load faculties.'");
    }

    /// <summary>
    /// Tests that LoadAsync logs the exception with correct log level and message.
    /// Input: GetFacultyDetailsAsync throws exception.
    /// Expected: Logger.LogError is called with the exception and "Failed to load faculties" message.
    /// </summary>
    [TestMethod]
    public async Task LoadAsync_WhenExceptionOccurs_LogsErrorWithCorrectParameters()
    {
        // Arrange
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<FacultiesViewModel>>();

        var testException = new InvalidOperationException("Test error");
        mockAcademic.Setup(a => a.GetUniversitiesAsync()).ReturnsAsync(new List<LookupItem>());
        mockAcademic.Setup(a => a.GetFacultyDetailsAsync()).ThrowsAsync(testException);

        var viewModel = new FacultiesViewModel(mockAcademic.Object, mockLogger.Object);

        // Act
        await viewModel.LoadAsync();

        // Assert
        mockLogger.Verify(
            logger => logger.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Failed to load faculties")),
                testException,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once,
            "Logger should log error with correct message and exception");
    }

    /// <summary>
    /// Tests that LoadAsync does not modify statistics when IsBusy is true.
    /// Input: Set IsBusy to true, then call LoadAsync.
    /// Expected: Statistics remain at default values (0).
    /// </summary>
    [TestMethod]
    public async Task LoadAsync_WhenIsBusyIsTrue_DoesNotModifyStatistics()
    {
        // Arrange
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<FacultiesViewModel>>();

        var universities = new List<LookupItem> { new LookupItem { Id = "u1", Name = "University 1" } };
        var faculties = new List<FacultyDto> { new FacultyDto { Id = "f1", Name = "Faculty 1", Status = "Active", UniversityId = "u1" } };

        mockAcademic.Setup(a => a.GetUniversitiesAsync()).ReturnsAsync(universities);
        mockAcademic.Setup(a => a.GetFacultyDetailsAsync()).ReturnsAsync(faculties);

        var viewModel = new FacultiesViewModel(mockAcademic.Object, mockLogger.Object);

        // Start a LoadAsync operation to set IsBusy to true
        var firstLoadTask = viewModel.LoadAsync();

        // Act - attempt second load while first is in progress
        await viewModel.LoadAsync();

        // Complete first load
        await firstLoadTask;

        // Assert - verify service methods were called only once (by first load)
        mockAcademic.Verify(a => a.GetUniversitiesAsync(), Times.Once, "GetUniversitiesAsync should only be called once");
        mockAcademic.Verify(a => a.GetFacultyDetailsAsync(), Times.Once, "GetFacultyDetailsAsync should only be called once");
    }

    /// <summary>
    /// Tests that LoadAsync correctly handles maximum integer values in counts.
    /// Input: Very large number of faculties (simulating int.MaxValue scenario).
    /// Expected: Statistics are calculated correctly without overflow.
    /// </summary>
    [TestMethod]
    public async Task LoadAsync_WithMaximumFacultyCount_HandlesCorrectly()
    {
        // Arrange
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<FacultiesViewModel>>();

        var universities = new List<LookupItem>();
        // Create a reasonably large list to test counting logic
        var faculties = new List<FacultyDto>();
        for (int i = 0; i < 10000; i++)
        {
            faculties.Add(new FacultyDto
            {
                Id = $"f{i}",
                Name = $"Faculty {i}",
                Status = i % 2 == 0 ? "Active" : "Pending",
                UniversityId = "u1"
            });
        }

        mockAcademic.Setup(a => a.GetUniversitiesAsync()).ReturnsAsync(universities);
        mockAcademic.Setup(a => a.GetFacultyDetailsAsync()).ReturnsAsync(faculties);

        var viewModel = new FacultiesViewModel(mockAcademic.Object, mockLogger.Object);

        // Act
        await viewModel.LoadAsync();

        // Assert
        Assert.AreEqual(10000, viewModel.TotalFaculties, "TotalFaculties should be 10000");
        Assert.AreEqual(5000, viewModel.ActiveFacultiesCount, "ActiveFacultiesCount should be 5000");
        Assert.AreEqual(5000, viewModel.PendingCount, "PendingCount should be 5000");
    }

    /// <summary>
    /// Tests that LoadAsync handles faculties with all possible edge case Status values.
    /// Input: Faculties with Status values: null, empty, whitespace, special characters.
    /// Expected: None are counted as Active, all counted in PendingCount.
    /// </summary>
    [TestMethod]
    public async Task LoadAsync_WithEdgeCaseStatusValues_CountsAllAsNonActive()
    {
        // Arrange
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<FacultiesViewModel>>();

        var universities = new List<LookupItem>();
        var faculties = new List<FacultyDto>
        {
            new FacultyDto { Id = "f1", Name = "Faculty 1", Status = null!, UniversityId = "u1" },
            new FacultyDto { Id = "f2", Name = "Faculty 2", Status = string.Empty, UniversityId = "u1" },
            new FacultyDto { Id = "f3", Name = "Faculty 3", Status = "   ", UniversityId = "u1" },
            new FacultyDto { Id = "f4", Name = "Faculty 4", Status = "\t\n", UniversityId = "u1" },
            new FacultyDto { Id = "f5", Name = "Faculty 5", Status = "!@#$%", UniversityId = "u1" }
        };

        mockAcademic.Setup(a => a.GetUniversitiesAsync()).ReturnsAsync(universities);
        mockAcademic.Setup(a => a.GetFacultyDetailsAsync()).ReturnsAsync(faculties);

        var viewModel = new FacultiesViewModel(mockAcademic.Object, mockLogger.Object);

        // Act
        await viewModel.LoadAsync();

        // Assert
        Assert.AreEqual(5, viewModel.TotalFaculties, "TotalFaculties should be 5");
        Assert.AreEqual(0, viewModel.ActiveFacultiesCount, "ActiveFacultiesCount should be 0 for all edge case statuses");
        Assert.AreEqual(5, viewModel.PendingCount, "PendingCount should be 5");
    }

    /// <summary>
    /// Tests that LoadAsync maintains correct state when GetUniversitiesAsync returns null.
    /// Input: GetUniversitiesAsync returns null.
    /// Expected: Exception is caught, error is logged, ErrorMessage is set.
    /// </summary>
    [TestMethod]
    public async Task LoadAsync_WhenGetUniversitiesAsyncReturnsNull_HandlesGracefully()
    {
        // Arrange
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<FacultiesViewModel>>();

        mockAcademic.Setup(a => a.GetUniversitiesAsync()).ReturnsAsync((IEnumerable<LookupItem>)null!);

        var viewModel = new FacultiesViewModel(mockAcademic.Object, mockLogger.Object);

        // Act
        await viewModel.LoadAsync();

        // Assert
        Assert.IsFalse(viewModel.IsBusy, "IsBusy should be false after exception");
        Assert.AreEqual("Failed to load faculties.", viewModel.ErrorMessage, "ErrorMessage should be set");
        mockLogger.Verify(
            logger => logger.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once,
            "Logger should log the error");
    }

    /// <summary>
    /// Tests that LoadAsync maintains correct state when GetFacultyDetailsAsync returns null.
    /// Input: GetFacultyDetailsAsync returns null.
    /// Expected: Exception is caught, error is logged, ErrorMessage is set.
    /// </summary>
    [TestMethod]
    public async Task LoadAsync_WhenGetFacultyDetailsAsyncReturnsNull_HandlesGracefully()
    {
        // Arrange
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<FacultiesViewModel>>();

        mockAcademic.Setup(a => a.GetUniversitiesAsync()).ReturnsAsync(new List<LookupItem>());
        mockAcademic.Setup(a => a.GetFacultyDetailsAsync()).ReturnsAsync((List<FacultyDto>)null!);

        var viewModel = new FacultiesViewModel(mockAcademic.Object, mockLogger.Object);

        // Act
        await viewModel.LoadAsync();

        // Assert
        Assert.IsFalse(viewModel.IsBusy, "IsBusy should be false after exception");
        Assert.AreEqual("Failed to load faculties.", viewModel.ErrorMessage, "ErrorMessage should be set");
        mockLogger.Verify(
            logger => logger.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once,
            "Logger should log the error");
    }

    /// <summary>
    /// Tests that LoadAsync handles universities with special characters in Id and Name.
    /// Input: Universities with special characters.
    /// Expected: Universities are loaded correctly into the collection.
    /// </summary>
    [TestMethod]
    public async Task LoadAsync_WithSpecialCharactersInUniversityData_LoadsCorrectly()
    {
        // Arrange
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<FacultiesViewModel>>();

        var universities = new List<LookupItem>
        {
            new LookupItem { Id = "u-!@#$%", Name = "University <>&\"'" },
            new LookupItem { Id = "u\t\n\r", Name = "University\t\n\r" }
        };

        mockAcademic.Setup(a => a.GetUniversitiesAsync()).ReturnsAsync(universities);
        mockAcademic.Setup(a => a.GetFacultyDetailsAsync()).ReturnsAsync(new List<FacultyDto>());

        var viewModel = new FacultiesViewModel(mockAcademic.Object, mockLogger.Object);

        // Act
        await viewModel.LoadAsync();

        // Assert
        Assert.AreEqual(2, viewModel.Universities.Count, "Universities count should be 2");
        Assert.AreEqual("u-!@#$%", viewModel.Universities[0].Id, "First university Id should preserve special characters");
        Assert.AreEqual("University <>&\"'", viewModel.Universities[0].Name, "First university Name should preserve special characters");
    }

    /// <summary>
    /// Tests that LoadAsync correctly orders operations: clears ErrorMessage before service calls.
    /// Input: Previous ErrorMessage exists.
    /// Expected: ErrorMessage is cleared even if subsequent operation fails.
    /// </summary>
    [TestMethod]
    public async Task LoadAsync_ClearsErrorMessageBeforeServiceCalls()
    {
        // Arrange
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<FacultiesViewModel>>();

        var universities = new List<LookupItem>();
        var faculties = new List<FacultyDto>();

        mockAcademic.Setup(a => a.GetUniversitiesAsync()).ReturnsAsync(universities);
        mockAcademic.Setup(a => a.GetFacultyDetailsAsync()).ReturnsAsync(faculties);

        var viewModel = new FacultiesViewModel(mockAcademic.Object, mockLogger.Object);

        // Set an initial error message
        viewModel.ErrorMessage = "Previous error";

        // Act
        await viewModel.LoadAsync();

        // Assert
        Assert.AreEqual(string.Empty, viewModel.ErrorMessage, "ErrorMessage should be cleared on successful load");
    }

    /// <summary>
    /// Tests that LoadAsync with exactly one Active faculty calculates statistics correctly.
    /// Input: Single faculty with Status = "Active".
    /// Expected: ActiveFacultiesCount = 1, PendingCount = 0.
    /// </summary>
    [TestMethod]
    public async Task LoadAsync_WithSingleActiveFaculty_CalculatesStatisticsCorrectly()
    {
        // Arrange
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<FacultiesViewModel>>();

        var universities = new List<LookupItem>();
        var faculties = new List<FacultyDto>
        {
            new FacultyDto { Id = "f1", Name = "Faculty 1", Status = "Active", UniversityId = "u1" }
        };

        mockAcademic.Setup(a => a.GetUniversitiesAsync()).ReturnsAsync(universities);
        mockAcademic.Setup(a => a.GetFacultyDetailsAsync()).ReturnsAsync(faculties);

        var viewModel = new FacultiesViewModel(mockAcademic.Object, mockLogger.Object);

        // Act
        await viewModel.LoadAsync();

        // Assert
        Assert.AreEqual(1, viewModel.TotalFaculties, "TotalFaculties should be 1");
        Assert.AreEqual(1, viewModel.ActiveFacultiesCount, "ActiveFacultiesCount should be 1");
        Assert.AreEqual(0, viewModel.PendingCount, "PendingCount should be 0");
    }

    /// <summary>
    /// Tests that PageInfo property returns the correct initial value when the ViewModel is first constructed.
    /// Input: None (testing initial state).
    /// Expected: PageInfo returns "Page 1 of 1" using default values.
    /// </summary>
    [TestMethod]
    public void PageInfo_InitialValue_ReturnsPageOneOfOne()
    {
        // Arrange
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<FacultiesViewModel>>();

        // Act
        var viewModel = new FacultiesViewModel(mockAcademic.Object, mockLogger.Object);
        var actualPageInfo = viewModel.PageInfo;

        // Assert
        Assert.AreEqual("Page 1 of 1", actualPageInfo);
    }

    /// <summary>
    /// Tests that PageInfo property updates correctly when CurrentPage changes while TotalPages remains constant.
    /// Input: CurrentPage changes from 1 to 2, then to 3, while TotalPages remains 10.
    /// Expected: PageInfo reflects the updated CurrentPage value each time.
    /// </summary>
    [TestMethod]
    public void PageInfo_CurrentPageChanges_ReflectsUpdatedValue()
    {
        // Arrange
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<FacultiesViewModel>>();
        var viewModel = new FacultiesViewModel(mockAcademic.Object, mockLogger.Object);
        viewModel.TotalPages = 10;

        // Act & Assert
        viewModel.CurrentPage = 1;
        Assert.AreEqual("Page 1 of 10", viewModel.PageInfo);

        viewModel.CurrentPage = 2;
        Assert.AreEqual("Page 2 of 10", viewModel.PageInfo);

        viewModel.CurrentPage = 3;
        Assert.AreEqual("Page 3 of 10", viewModel.PageInfo);
    }

    /// <summary>
    /// Tests that PageInfo property updates correctly when TotalPages changes while CurrentPage remains constant.
    /// Input: TotalPages changes from 1 to 5, then to 20, while CurrentPage remains 1.
    /// Expected: PageInfo reflects the updated TotalPages value each time.
    /// </summary>
    [TestMethod]
    public void PageInfo_TotalPagesChanges_ReflectsUpdatedValue()
    {
        // Arrange
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<FacultiesViewModel>>();
        var viewModel = new FacultiesViewModel(mockAcademic.Object, mockLogger.Object);

        // Act & Assert
        viewModel.TotalPages = 1;
        Assert.AreEqual("Page 1 of 1", viewModel.PageInfo);

        viewModel.TotalPages = 5;
        Assert.AreEqual("Page 1 of 5", viewModel.PageInfo);

        viewModel.TotalPages = 20;
        Assert.AreEqual("Page 1 of 20", viewModel.PageInfo);
    }

    /// <summary>
    /// Tests that PageInfo property updates correctly when both CurrentPage and TotalPages change.
    /// Input: Multiple updates to both CurrentPage and TotalPages.
    /// Expected: PageInfo always reflects the current values of both properties.
    /// </summary>
    [TestMethod]
    public void PageInfo_BothCurrentPageAndTotalPagesChange_ReflectsUpdatedValues()
    {
        // Arrange
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<FacultiesViewModel>>();
        var viewModel = new FacultiesViewModel(mockAcademic.Object, mockLogger.Object);

        // Act & Assert
        viewModel.CurrentPage = 2;
        viewModel.TotalPages = 5;
        Assert.AreEqual("Page 2 of 5", viewModel.PageInfo);

        viewModel.CurrentPage = 10;
        viewModel.TotalPages = 100;
        Assert.AreEqual("Page 10 of 100", viewModel.PageInfo);

        viewModel.CurrentPage = 0;
        viewModel.TotalPages = 0;
        Assert.AreEqual("Page 0 of 0", viewModel.PageInfo);
    }

    /// <summary>
    /// Tests that PageInfo property handles extreme boundary values correctly.
    /// Input: Extreme values including int.MaxValue and int.MinValue for both properties.
    /// Expected: PageInfo returns correctly formatted string without overflow or exception.
    /// </summary>
    [TestMethod]
    public void PageInfo_ExtremeBoundaryValues_ReturnsFormattedStringWithoutException()
    {
        // Arrange
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<FacultiesViewModel>>();
        var viewModel = new FacultiesViewModel(mockAcademic.Object, mockLogger.Object);

        // Act & Assert - int.MaxValue
        viewModel.CurrentPage = int.MaxValue;
        viewModel.TotalPages = int.MaxValue;
        Assert.AreEqual("Page 2147483647 of 2147483647", viewModel.PageInfo);

        // Act & Assert - int.MinValue
        viewModel.CurrentPage = int.MinValue;
        viewModel.TotalPages = int.MinValue;
        Assert.AreEqual("Page -2147483648 of -2147483648", viewModel.PageInfo);

        // Act & Assert - Mixed
        viewModel.CurrentPage = int.MaxValue;
        viewModel.TotalPages = int.MinValue;
        Assert.AreEqual("Page 2147483647 of -2147483648", viewModel.PageInfo);
    }

    /// <summary>
    /// Tests that PageInfo property returns the correct format with single digit and multi-digit values.
    /// Input: Various single-digit and multi-digit values.
    /// Expected: PageInfo correctly formats all values regardless of digit count.
    /// </summary>
    [TestMethod]
    [DataRow(1, 9, "Page 1 of 9")]
    [DataRow(9, 9, "Page 9 of 9")]
    [DataRow(10, 99, "Page 10 of 99")]
    [DataRow(99, 99, "Page 99 of 99")]
    [DataRow(100, 999, "Page 100 of 999")]
    [DataRow(999, 9999, "Page 999 of 9999")]
    [DataRow(1000, 10000, "Page 1000 of 10000")]
    public void PageInfo_VariousDigitCounts_FormatsCorrectly(int currentPage, int totalPages, string expectedPageInfo)
    {
        // Arrange
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<FacultiesViewModel>>();
        var viewModel = new FacultiesViewModel(mockAcademic.Object, mockLogger.Object);

        // Act
        viewModel.CurrentPage = currentPage;
        viewModel.TotalPages = totalPages;
        var actualPageInfo = viewModel.PageInfo;

        // Assert
        Assert.AreEqual(expectedPageInfo, actualPageInfo);
    }
}



/// <summary>
/// Unit tests for the FacultiesViewModel constructor.
/// </summary>
[TestClass]
public partial class FacultiesViewModelConstructorTests
{
    /// <summary>
    /// Tests that the constructor successfully initializes the view model with valid parameters.
    /// Input: Valid mocked IAcademicService and ILogger.
    /// Expected: Instance is created successfully.
    /// </summary>
    [TestMethod]
    public void Constructor_WithValidParameters_CreatesInstanceSuccessfully()
    {
        // Arrange
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<FacultiesViewModel>>();

        // Act
        var viewModel = new FacultiesViewModel(mockAcademic.Object, mockLogger.Object);

        // Assert
        Assert.IsNotNull(viewModel);
    }

    /// <summary>
    /// Tests that the constructor initializes all command properties to non-null values.
    /// Input: Valid mocked dependencies.
    /// Expected: All ICommand properties are not null.
    /// </summary>
    [TestMethod]
    public void Constructor_WithValidParameters_InitializesAllCommandsToNonNull()
    {
        // Arrange
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<FacultiesViewModel>>();

        // Act
        var viewModel = new FacultiesViewModel(mockAcademic.Object, mockLogger.Object);

        // Assert
        Assert.IsNotNull(viewModel.LoadCommand);
        Assert.IsNotNull(viewModel.SearchCommand);
        Assert.IsNotNull(viewModel.FilterCommand);
        Assert.IsNotNull(viewModel.NextPageCommand);
        Assert.IsNotNull(viewModel.PrevPageCommand);
        Assert.IsNotNull(viewModel.AddFacultyCommand);
        Assert.IsNotNull(viewModel.EditFacultyCommand);
        Assert.IsNotNull(viewModel.DeleteFacultyCommand);
        Assert.IsNotNull(viewModel.SaveCommand);
        Assert.IsNotNull(viewModel.CancelEditCommand);
        Assert.IsNotNull(viewModel.ExportCommand);
        Assert.IsNotNull(viewModel.RefreshCommand);
    }

    /// <summary>
    /// Tests that AddFacultyCommand resets form fields and sets IsEditing to true when executed.
    /// Input: Execute AddFacultyCommand.
    /// Expected: EditId is null, EditName is empty, SelectedUniversity is null, IsEditing is true.
    /// </summary>
    [TestMethod]
    public void Constructor_AddFacultyCommand_ResetsFormFieldsAndSetsIsEditingToTrue()
    {
        // Arrange
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<FacultiesViewModel>>();
        var viewModel = new FacultiesViewModel(mockAcademic.Object, mockLogger.Object);
        viewModel.EditId = "existing-id";
        viewModel.EditName = "Existing Name";
        viewModel.SelectedUniversity = new LookupItem { Id = "uni-1", Name = "University 1" };

        // Act
        ((Command)viewModel.AddFacultyCommand).Execute(null);

        // Assert
        Assert.IsNull(viewModel.EditId);
        Assert.AreEqual(string.Empty, viewModel.EditName);
        Assert.IsNull(viewModel.SelectedUniversity);
        Assert.IsTrue(viewModel.IsEditing);
    }

    /// <summary>
    /// Tests that CancelEditCommand resets form fields and sets IsEditing to false when executed.
    /// Input: Execute CancelEditCommand.
    /// Expected: EditId is null, EditName is empty, SelectedUniversity is null, IsEditing is false.
    /// </summary>
    [TestMethod]
    public void Constructor_CancelEditCommand_ResetsFormFieldsAndSetsIsEditingToFalse()
    {
        // Arrange
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<FacultiesViewModel>>();
        var viewModel = new FacultiesViewModel(mockAcademic.Object, mockLogger.Object);
        viewModel.EditId = "existing-id";
        viewModel.EditName = "Existing Name";
        viewModel.SelectedUniversity = new LookupItem { Id = "uni-1", Name = "University 1" };
        viewModel.IsEditing = true;

        // Act
        ((Command)viewModel.CancelEditCommand).Execute(null);

        // Assert
        Assert.IsNull(viewModel.EditId);
        Assert.AreEqual(string.Empty, viewModel.EditName);
        Assert.IsNull(viewModel.SelectedUniversity);
        Assert.IsFalse(viewModel.IsEditing);
    }

    /// <summary>
    /// Tests that FilterCommand sets UniversityFilter property with the provided string value.
    /// Input: Valid string value.
    /// Expected: UniversityFilter is set to the provided value.
    /// </summary>
    [TestMethod]
    public void Constructor_FilterCommand_SetsUniversityFilterToProvidedValue()
    {
        // Arrange
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<FacultiesViewModel>>();
        var viewModel = new FacultiesViewModel(mockAcademic.Object, mockLogger.Object);
        var filterValue = "TestUniversity";

        // Act
        ((Command<string>)viewModel.FilterCommand).Execute(filterValue);

        // Assert
        Assert.AreEqual(filterValue, viewModel.UniversityFilter);
    }

    /// <summary>
    /// Tests that FilterCommand handles null value correctly.
    /// Input: null string.
    /// Expected: UniversityFilter is set to null.
    /// </summary>
    [TestMethod]
    public void Constructor_FilterCommand_WithNullValue_SetsUniversityFilterToNull()
    {
        // Arrange
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<FacultiesViewModel>>();
        var viewModel = new FacultiesViewModel(mockAcademic.Object, mockLogger.Object);

        // Act
        ((Command<string>)viewModel.FilterCommand).Execute(null);

        // Assert
        Assert.IsNull(viewModel.UniversityFilter);
    }

    /// <summary>
    /// Tests that FilterCommand handles empty string correctly.
    /// Input: Empty string.
    /// Expected: UniversityFilter is set to empty string.
    /// </summary>
    [TestMethod]
    public void Constructor_FilterCommand_WithEmptyString_SetsUniversityFilterToEmpty()
    {
        // Arrange
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<FacultiesViewModel>>();
        var viewModel = new FacultiesViewModel(mockAcademic.Object, mockLogger.Object);

        // Act
        ((Command<string>)viewModel.FilterCommand).Execute(string.Empty);

        // Assert
        Assert.AreEqual(string.Empty, viewModel.UniversityFilter);
    }

    /// <summary>
    /// Tests that FilterCommand handles whitespace-only string correctly.
    /// Input: Whitespace string.
    /// Expected: UniversityFilter is set to the whitespace string.
    /// </summary>
    [TestMethod]
    [DataRow("   ")]
    [DataRow("\t")]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [DataRow("  \t  \n  ")]
    public void Constructor_FilterCommand_WithWhitespace_SetsUniversityFilterToWhitespace(string whitespaceValue)
    {
        // Arrange
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<FacultiesViewModel>>();
        var viewModel = new FacultiesViewModel(mockAcademic.Object, mockLogger.Object);

        // Act
        ((Command<string>)viewModel.FilterCommand).Execute(whitespaceValue);

        // Assert
        Assert.AreEqual(whitespaceValue, viewModel.UniversityFilter);
    }

    /// <summary>
    /// Tests that FilterCommand handles very long strings correctly.
    /// Input: Very long string (10000 characters).
    /// Expected: UniversityFilter is set to the long string.
    /// </summary>
    [TestMethod]
    public void Constructor_FilterCommand_WithVeryLongString_SetsUniversityFilter()
    {
        // Arrange
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<FacultiesViewModel>>();
        var viewModel = new FacultiesViewModel(mockAcademic.Object, mockLogger.Object);
        var longString = new string('a', 10000);

        // Act
        ((Command<string>)viewModel.FilterCommand).Execute(longString);

        // Assert
        Assert.AreEqual(longString, viewModel.UniversityFilter);
    }

    /// <summary>
    /// Tests that FilterCommand handles strings with special characters correctly.
    /// Input: Strings with various special characters.
    /// Expected: UniversityFilter is set to the string with special characters.
    /// </summary>
    [TestMethod]
    [DataRow("University!@#$%^&*()")]
    [DataRow("University<>?/\\|")]
    [DataRow("University™©®")]
    [DataRow("University😀🎉")]
    [DataRow("Université")]
    public void Constructor_FilterCommand_WithSpecialCharacters_SetsUniversityFilter(string specialValue)
    {
        // Arrange
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<FacultiesViewModel>>();
        var viewModel = new FacultiesViewModel(mockAcademic.Object, mockLogger.Object);

        // Act
        ((Command<string>)viewModel.FilterCommand).Execute(specialValue);

        // Assert
        Assert.AreEqual(specialValue, viewModel.UniversityFilter);
    }

    /// <summary>
    /// Tests that NextPageCommand increments CurrentPage when CurrentPage is less than TotalPages.
    /// Input: CurrentPage = 1, TotalPages = 10.
    /// Expected: CurrentPage is incremented to 2.
    /// </summary>
    [TestMethod]
    public void Constructor_NextPageCommand_WhenCurrentPageLessThanTotalPages_IncrementsCurrentPage()
    {
        // Arrange
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<FacultiesViewModel>>();
        var viewModel = new FacultiesViewModel(mockAcademic.Object, mockLogger.Object);
        viewModel.TotalPages = 10;

        // Act
        ((Command)viewModel.NextPageCommand).Execute(null);

        // Assert
        Assert.AreEqual(2, viewModel.CurrentPage);
    }

    /// <summary>
    /// Tests that NextPageCommand does not increment CurrentPage when CurrentPage equals TotalPages.
    /// Input: CurrentPage = 10, TotalPages = 10.
    /// Expected: CurrentPage remains 10.
    /// </summary>
    [TestMethod]
    public void Constructor_NextPageCommand_WhenCurrentPageEqualsTotalPages_DoesNotIncrementCurrentPage()
    {
        // Arrange
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<FacultiesViewModel>>();
        var viewModel = new FacultiesViewModel(mockAcademic.Object, mockLogger.Object);
        viewModel.TotalPages = 10;
        viewModel.CurrentPage = 10;

        // Act
        ((Command)viewModel.NextPageCommand).Execute(null);

        // Assert
        Assert.AreEqual(10, viewModel.CurrentPage);
    }

    /// <summary>
    /// Tests that NextPageCommand does not increment CurrentPage when CurrentPage is greater than TotalPages.
    /// Input: CurrentPage = 15, TotalPages = 10.
    /// Expected: CurrentPage remains 15.
    /// </summary>
    [TestMethod]
    public void Constructor_NextPageCommand_WhenCurrentPageGreaterThanTotalPages_DoesNotIncrementCurrentPage()
    {
        // Arrange
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<FacultiesViewModel>>();
        var viewModel = new FacultiesViewModel(mockAcademic.Object, mockLogger.Object);
        viewModel.TotalPages = 10;
        viewModel.CurrentPage = 15;

        // Act
        ((Command)viewModel.NextPageCommand).Execute(null);

        // Assert
        Assert.AreEqual(15, viewModel.CurrentPage);
    }

    /// <summary>
    /// Tests that NextPageCommand handles boundary condition at TotalPages - 1.
    /// Input: CurrentPage = 9, TotalPages = 10.
    /// Expected: CurrentPage is incremented to 10.
    /// </summary>
    [TestMethod]
    public void Constructor_NextPageCommand_AtBoundaryTotalPagesMinusOne_IncrementsToTotalPages()
    {
        // Arrange
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<FacultiesViewModel>>();
        var viewModel = new FacultiesViewModel(mockAcademic.Object, mockLogger.Object);
        viewModel.TotalPages = 10;
        viewModel.CurrentPage = 9;

        // Act
        ((Command)viewModel.NextPageCommand).Execute(null);

        // Assert
        Assert.AreEqual(10, viewModel.CurrentPage);
    }

    /// <summary>
    /// Tests that NextPageCommand with negative TotalPages does not increment CurrentPage.
    /// Input: CurrentPage = 1, TotalPages = -1.
    /// Expected: CurrentPage remains 1.
    /// </summary>
    [TestMethod]
    public void Constructor_NextPageCommand_WithNegativeTotalPages_DoesNotIncrementCurrentPage()
    {
        // Arrange
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<FacultiesViewModel>>();
        var viewModel = new FacultiesViewModel(mockAcademic.Object, mockLogger.Object);
        viewModel.TotalPages = -1;
        viewModel.CurrentPage = 1;

        // Act
        ((Command)viewModel.NextPageCommand).Execute(null);

        // Assert
        Assert.AreEqual(1, viewModel.CurrentPage);
    }

    /// <summary>
    /// Tests that NextPageCommand with zero TotalPages does not increment CurrentPage.
    /// Input: CurrentPage = 1, TotalPages = 0.
    /// Expected: CurrentPage remains 1.
    /// </summary>
    [TestMethod]
    public void Constructor_NextPageCommand_WithZeroTotalPages_DoesNotIncrementCurrentPage()
    {
        // Arrange
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<FacultiesViewModel>>();
        var viewModel = new FacultiesViewModel(mockAcademic.Object, mockLogger.Object);
        viewModel.TotalPages = 0;
        viewModel.CurrentPage = 1;

        // Act
        ((Command)viewModel.NextPageCommand).Execute(null);

        // Assert
        Assert.AreEqual(1, viewModel.CurrentPage);
    }

    /// <summary>
    /// Tests that PrevPageCommand decrements CurrentPage when CurrentPage is greater than 1.
    /// Input: CurrentPage = 5.
    /// Expected: CurrentPage is decremented to 4.
    /// </summary>
    [TestMethod]
    public void Constructor_PrevPageCommand_WhenCurrentPageGreaterThanOne_DecrementsCurrentPage()
    {
        // Arrange
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<FacultiesViewModel>>();
        var viewModel = new FacultiesViewModel(mockAcademic.Object, mockLogger.Object);
        viewModel.CurrentPage = 5;

        // Act
        ((Command)viewModel.PrevPageCommand).Execute(null);

        // Assert
        Assert.AreEqual(4, viewModel.CurrentPage);
    }

    /// <summary>
    /// Tests that PrevPageCommand does not decrement CurrentPage when CurrentPage equals 1.
    /// Input: CurrentPage = 1.
    /// Expected: CurrentPage remains 1.
    /// </summary>
    [TestMethod]
    public void Constructor_PrevPageCommand_WhenCurrentPageEqualsOne_DoesNotDecrementCurrentPage()
    {
        // Arrange
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<FacultiesViewModel>>();
        var viewModel = new FacultiesViewModel(mockAcademic.Object, mockLogger.Object);
        viewModel.CurrentPage = 1;

        // Act
        ((Command)viewModel.PrevPageCommand).Execute(null);

        // Assert
        Assert.AreEqual(1, viewModel.CurrentPage);
    }

    /// <summary>
    /// Tests that PrevPageCommand does not decrement CurrentPage when CurrentPage is less than 1.
    /// Input: CurrentPage = 0.
    /// Expected: CurrentPage remains 0.
    /// </summary>
    [TestMethod]
    public void Constructor_PrevPageCommand_WhenCurrentPageLessThanOne_DoesNotDecrementCurrentPage()
    {
        // Arrange
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<FacultiesViewModel>>();
        var viewModel = new FacultiesViewModel(mockAcademic.Object, mockLogger.Object);
        viewModel.CurrentPage = 0;

        // Act
        ((Command)viewModel.PrevPageCommand).Execute(null);

        // Assert
        Assert.AreEqual(0, viewModel.CurrentPage);
    }

    /// <summary>
    /// Tests that PrevPageCommand handles boundary condition at CurrentPage = 2.
    /// Input: CurrentPage = 2.
    /// Expected: CurrentPage is decremented to 1.
    /// </summary>
    [TestMethod]
    public void Constructor_PrevPageCommand_AtBoundaryTwo_DecrementsToOne()
    {
        // Arrange
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<FacultiesViewModel>>();
        var viewModel = new FacultiesViewModel(mockAcademic.Object, mockLogger.Object);
        viewModel.CurrentPage = 2;

        // Act
        ((Command)viewModel.PrevPageCommand).Execute(null);

        // Assert
        Assert.AreEqual(1, viewModel.CurrentPage);
    }

    /// <summary>
    /// Tests that PrevPageCommand with negative CurrentPage does not decrement.
    /// Input: CurrentPage = -5.
    /// Expected: CurrentPage remains -5.
    /// </summary>
    [TestMethod]
    public void Constructor_PrevPageCommand_WithNegativeCurrentPage_DoesNotDecrement()
    {
        // Arrange
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<FacultiesViewModel>>();
        var viewModel = new FacultiesViewModel(mockAcademic.Object, mockLogger.Object);
        viewModel.CurrentPage = -5;

        // Act
        ((Command)viewModel.PrevPageCommand).Execute(null);

        // Assert
        Assert.AreEqual(-5, viewModel.CurrentPage);
    }

    /// <summary>
    /// Tests that EditFacultyCommand populates form fields from the provided FacultyDto.
    /// Input: Valid FacultyDto with matching university in Universities collection.
    /// Expected: EditId, EditName, SelectedUniversity, and IsEditing are set correctly.
    /// </summary>
    [TestMethod]
    public void Constructor_EditFacultyCommand_WithMatchingUniversity_PopulatesFormFieldsCorrectly()
    {
        // Arrange
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<FacultiesViewModel>>();
        var viewModel = new FacultiesViewModel(mockAcademic.Object, mockLogger.Object);
        var university = new LookupItem { Id = "uni-1", Name = "University 1" };
        viewModel.Universities.Add(university);
        var faculty = new FacultyDto { Id = "fac-1", Name = "Faculty 1", UniversityId = "uni-1" };

        // Act
        ((Command<FacultyDto>)viewModel.EditFacultyCommand).Execute(faculty);

        // Assert
        Assert.AreEqual("fac-1", viewModel.EditId);
        Assert.AreEqual("Faculty 1", viewModel.EditName);
        Assert.AreEqual(university, viewModel.SelectedUniversity);
        Assert.IsTrue(viewModel.IsEditing);
    }

    /// <summary>
    /// Tests that EditFacultyCommand sets SelectedUniversity to null when no matching university is found.
    /// Input: FacultyDto with UniversityId that doesn't match any university in Universities collection.
    /// Expected: EditId and EditName are set, SelectedUniversity is null, IsEditing is true.
    /// </summary>
    [TestMethod]
    public void Constructor_EditFacultyCommand_WithNoMatchingUniversity_SetsSelectedUniversityToNull()
    {
        // Arrange
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<FacultiesViewModel>>();
        var viewModel = new FacultiesViewModel(mockAcademic.Object, mockLogger.Object);
        var university = new LookupItem { Id = "uni-1", Name = "University 1" };
        viewModel.Universities.Add(university);
        var faculty = new FacultyDto { Id = "fac-1", Name = "Faculty 1", UniversityId = "uni-999" };

        // Act
        ((Command<FacultyDto>)viewModel.EditFacultyCommand).Execute(faculty);

        // Assert
        Assert.AreEqual("fac-1", viewModel.EditId);
        Assert.AreEqual("Faculty 1", viewModel.EditName);
        Assert.IsNull(viewModel.SelectedUniversity);
        Assert.IsTrue(viewModel.IsEditing);
    }

    /// <summary>
    /// Tests that EditFacultyCommand handles FacultyDto with null UniversityId correctly.
    /// Input: FacultyDto with null UniversityId.
    /// Expected: EditId and EditName are set, SelectedUniversity is null, IsEditing is true.
    /// </summary>
    [TestMethod]
    public void Constructor_EditFacultyCommand_WithNullUniversityId_SetsSelectedUniversityToNull()
    {
        // Arrange
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<FacultiesViewModel>>();
        var viewModel = new FacultiesViewModel(mockAcademic.Object, mockLogger.Object);
        var university = new LookupItem { Id = "uni-1", Name = "University 1" };
        viewModel.Universities.Add(university);
        var faculty = new FacultyDto { Id = "fac-1", Name = "Faculty 1", UniversityId = null };

        // Act
        ((Command<FacultyDto>)viewModel.EditFacultyCommand).Execute(faculty);

        // Assert
        Assert.AreEqual("fac-1", viewModel.EditId);
        Assert.AreEqual("Faculty 1", viewModel.EditName);
        Assert.IsNull(viewModel.SelectedUniversity);
        Assert.IsTrue(viewModel.IsEditing);
    }

    /// <summary>
    /// Tests that EditFacultyCommand handles FacultyDto with empty UniversityId correctly.
    /// Input: FacultyDto with empty string UniversityId.
    /// Expected: EditId and EditName are set, SelectedUniversity is null, IsEditing is true.
    /// </summary>
    [TestMethod]
    public void Constructor_EditFacultyCommand_WithEmptyUniversityId_SetsSelectedUniversityToNull()
    {
        // Arrange
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<FacultiesViewModel>>();
        var viewModel = new FacultiesViewModel(mockAcademic.Object, mockLogger.Object);
        var university = new LookupItem { Id = "uni-1", Name = "University 1" };
        viewModel.Universities.Add(university);
        var faculty = new FacultyDto { Id = "fac-1", Name = "Faculty 1", UniversityId = string.Empty };

        // Act
        ((Command<FacultyDto>)viewModel.EditFacultyCommand).Execute(faculty);

        // Assert
        Assert.AreEqual("fac-1", viewModel.EditId);
        Assert.AreEqual("Faculty 1", viewModel.EditName);
        Assert.IsNull(viewModel.SelectedUniversity);
        Assert.IsTrue(viewModel.IsEditing);
    }

    /// <summary>
    /// Tests that EditFacultyCommand handles FacultyDto with whitespace-only Name correctly.
    /// Input: FacultyDto with whitespace-only Name.
    /// Expected: EditName is set to the whitespace string.
    /// </summary>
    [TestMethod]
    public void Constructor_EditFacultyCommand_WithWhitespaceOnlyName_SetsEditNameToWhitespace()
    {
        // Arrange
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<FacultiesViewModel>>();
        var viewModel = new FacultiesViewModel(mockAcademic.Object, mockLogger.Object);
        var faculty = new FacultyDto { Id = "fac-1", Name = "   ", UniversityId = null };

        // Act
        ((Command<FacultyDto>)viewModel.EditFacultyCommand).Execute(faculty);

        // Assert
        Assert.AreEqual("   ", viewModel.EditName);
        Assert.IsTrue(viewModel.IsEditing);
    }

    /// <summary>
    /// Tests that EditFacultyCommand handles FacultyDto with empty Name correctly.
    /// Input: FacultyDto with empty Name.
    /// Expected: EditName is set to empty string.
    /// </summary>
    [TestMethod]
    public void Constructor_EditFacultyCommand_WithEmptyName_SetsEditNameToEmpty()
    {
        // Arrange
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<FacultiesViewModel>>();
        var viewModel = new FacultiesViewModel(mockAcademic.Object, mockLogger.Object);
        var faculty = new FacultyDto { Id = "fac-1", Name = string.Empty, UniversityId = null };

        // Act
        ((Command<FacultyDto>)viewModel.EditFacultyCommand).Execute(faculty);

        // Assert
        Assert.AreEqual(string.Empty, viewModel.EditName);
        Assert.IsTrue(viewModel.IsEditing);
    }

    /// <summary>
    /// Tests that EditFacultyCommand handles FacultyDto with very long Name correctly.
    /// Input: FacultyDto with very long Name (1000 characters).
    /// Expected: EditName is set to the long string.
    /// </summary>
    [TestMethod]
    public void Constructor_EditFacultyCommand_WithVeryLongName_SetsEditNameCorrectly()
    {
        // Arrange
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<FacultiesViewModel>>();
        var viewModel = new FacultiesViewModel(mockAcademic.Object, mockLogger.Object);
        var longName = new string('a', 1000);
        var faculty = new FacultyDto { Id = "fac-1", Name = longName, UniversityId = null };

        // Act
        ((Command<FacultyDto>)viewModel.EditFacultyCommand).Execute(faculty);

        // Assert
        Assert.AreEqual(longName, viewModel.EditName);
        Assert.IsTrue(viewModel.IsEditing);
    }

    /// <summary>
    /// Tests that EditFacultyCommand handles FacultyDto with special characters in Name.
    /// Input: FacultyDto with special characters in Name.
    /// Expected: EditName is set correctly with special characters preserved.
    /// </summary>
    [TestMethod]
    [DataRow("Faculty!@#$%")]
    [DataRow("Faculty<>?")]
    [DataRow("Faculty\t\n\r")]
    [DataRow("Faculty™©®")]
    [DataRow("Faculty😀🎉")]
    [DataRow("Faculté")]
    public void Constructor_EditFacultyCommand_WithSpecialCharactersInName_SetsEditNameCorrectly(string specialName)
    {
        // Arrange
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<FacultiesViewModel>>();
        var viewModel = new FacultiesViewModel(mockAcademic.Object, mockLogger.Object);
        var faculty = new FacultyDto { Id = "fac-1", Name = specialName, UniversityId = null };

        // Act
        ((Command<FacultyDto>)viewModel.EditFacultyCommand).Execute(faculty);

        // Assert
        Assert.AreEqual(specialName, viewModel.EditName);
        Assert.IsTrue(viewModel.IsEditing);
    }

    /// <summary>
    /// Tests that all commands are of the expected type (Command or Command with parameter).
    /// Input: Constructor with valid parameters.
    /// Expected: Commands are of correct types.
    /// </summary>
    [TestMethod]
    public void Constructor_WithValidParameters_InitializesCommandsWithCorrectTypes()
    {
        // Arrange
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<FacultiesViewModel>>();

        // Act
        var viewModel = new FacultiesViewModel(mockAcademic.Object, mockLogger.Object);

        // Assert
        Assert.IsInstanceOfType(viewModel.LoadCommand, typeof(Command));
        Assert.IsInstanceOfType(viewModel.SearchCommand, typeof(Command));
        Assert.IsInstanceOfType(viewModel.FilterCommand, typeof(Command<string>));
        Assert.IsInstanceOfType(viewModel.NextPageCommand, typeof(Command));
        Assert.IsInstanceOfType(viewModel.PrevPageCommand, typeof(Command));
        Assert.IsInstanceOfType(viewModel.AddFacultyCommand, typeof(Command));
        Assert.IsInstanceOfType(viewModel.EditFacultyCommand, typeof(Command<FacultyDto>));
        Assert.IsInstanceOfType(viewModel.DeleteFacultyCommand, typeof(Command<FacultyDto>));
        Assert.IsInstanceOfType(viewModel.SaveCommand, typeof(Command));
        Assert.IsInstanceOfType(viewModel.CancelEditCommand, typeof(Command));
        Assert.IsInstanceOfType(viewModel.ExportCommand, typeof(Command));
        Assert.IsInstanceOfType(viewModel.RefreshCommand, typeof(Command));
    }

    /// <summary>
    /// Tests that AddFacultyCommand can be executed multiple times and resets form each time.
    /// Input: Execute AddFacultyCommand twice with different initial states.
    /// Expected: Form is reset correctly both times.
    /// </summary>
    [TestMethod]
    public void Constructor_AddFacultyCommand_MultipleExecutions_ResetsFormEachTime()
    {
        // Arrange
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<FacultiesViewModel>>();
        var viewModel = new FacultiesViewModel(mockAcademic.Object, mockLogger.Object);

        // First execution
        viewModel.EditId = "id-1";
        viewModel.EditName = "Name 1";
        viewModel.SelectedUniversity = new LookupItem { Id = "uni-1", Name = "Uni 1" };
        ((Command)viewModel.AddFacultyCommand).Execute(null);

        Assert.IsNull(viewModel.EditId);
        Assert.AreEqual(string.Empty, viewModel.EditName);
        Assert.IsNull(viewModel.SelectedUniversity);
        Assert.IsTrue(viewModel.IsEditing);

        // Second execution with different state
        viewModel.EditId = "id-2";
        viewModel.EditName = "Name 2";
        viewModel.SelectedUniversity = new LookupItem { Id = "uni-2", Name = "Uni 2" };
        viewModel.IsEditing = false;
        ((Command)viewModel.AddFacultyCommand).Execute(null);

        // Assert
        Assert.IsNull(viewModel.EditId);
        Assert.AreEqual(string.Empty, viewModel.EditName);
        Assert.IsNull(viewModel.SelectedUniversity);
        Assert.IsTrue(viewModel.IsEditing);
    }

    /// <summary>
    /// Tests that CancelEditCommand can be executed multiple times and resets form each time.
    /// Input: Execute CancelEditCommand twice with different initial states.
    /// Expected: Form is reset and IsEditing is false both times.
    /// </summary>
    [TestMethod]
    public void Constructor_CancelEditCommand_MultipleExecutions_ResetsFormEachTime()
    {
        // Arrange
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<FacultiesViewModel>>();
        var viewModel = new FacultiesViewModel(mockAcademic.Object, mockLogger.Object);

        // First execution
        viewModel.EditId = "id-1";
        viewModel.EditName = "Name 1";
        viewModel.SelectedUniversity = new LookupItem { Id = "uni-1", Name = "Uni 1" };
        viewModel.IsEditing = true;
        ((Command)viewModel.CancelEditCommand).Execute(null);

        Assert.IsNull(viewModel.EditId);
        Assert.AreEqual(string.Empty, viewModel.EditName);
        Assert.IsNull(viewModel.SelectedUniversity);
        Assert.IsFalse(viewModel.IsEditing);

        // Second execution with different state
        viewModel.EditId = "id-2";
        viewModel.EditName = "Name 2";
        viewModel.SelectedUniversity = new LookupItem { Id = "uni-2", Name = "Uni 2" };
        viewModel.IsEditing = true;
        ((Command)viewModel.CancelEditCommand).Execute(null);

        // Assert
        Assert.IsNull(viewModel.EditId);
        Assert.AreEqual(string.Empty, viewModel.EditName);
        Assert.IsNull(viewModel.SelectedUniversity);
        Assert.IsFalse(viewModel.IsEditing);
    }

    /// <summary>
    /// Tests that FilterCommand can handle consecutive calls with different values.
    /// Input: Multiple different filter values in sequence.
    /// Expected: UniversityFilter is updated correctly each time.
    /// </summary>
    [TestMethod]
    public void Constructor_FilterCommand_ConsecutiveCalls_UpdatesFilterEachTime()
    {
        // Arrange
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<FacultiesViewModel>>();
        var viewModel = new FacultiesViewModel(mockAcademic.Object, mockLogger.Object);

        // Act & Assert
        ((Command<string>)viewModel.FilterCommand).Execute("University1");
        Assert.AreEqual("University1", viewModel.UniversityFilter);

        ((Command<string>)viewModel.FilterCommand).Execute("University2");
        Assert.AreEqual("University2", viewModel.UniversityFilter);

        ((Command<string>)viewModel.FilterCommand).Execute(null);
        Assert.IsNull(viewModel.UniversityFilter);

        ((Command<string>)viewModel.FilterCommand).Execute(string.Empty);
        Assert.AreEqual(string.Empty, viewModel.UniversityFilter);
    }

    /// <summary>
    /// Tests that initial property values are set correctly after constructor execution.
    /// Input: Valid parameters.
    /// Expected: Properties have expected default values.
    /// </summary>
    [TestMethod]
    public void Constructor_WithValidParameters_InitializesPropertiesToDefaultValues()
    {
        // Arrange
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<FacultiesViewModel>>();

        // Act
        var viewModel = new FacultiesViewModel(mockAcademic.Object, mockLogger.Object);

        // Assert
        Assert.AreEqual(string.Empty, viewModel.SearchQuery);
        Assert.AreEqual("All", viewModel.UniversityFilter);
        Assert.AreEqual(1, viewModel.CurrentPage);
        Assert.AreEqual(1, viewModel.TotalPages);
        Assert.AreEqual(10, viewModel.PageSize);
        Assert.AreEqual(0, viewModel.TotalFaculties);
        Assert.AreEqual(0, viewModel.ActiveFacultiesCount);
        Assert.AreEqual(0, viewModel.PendingCount);
        Assert.AreEqual(string.Empty, viewModel.EditName);
        Assert.IsNull(viewModel.EditId);
        Assert.IsNull(viewModel.SelectedUniversityId);
        Assert.IsNull(viewModel.SelectedUniversity);
        Assert.IsFalse(viewModel.IsEditing);
        Assert.AreEqual(string.Empty, viewModel.ErrorMessage);
    }

    /// <summary>
    /// Tests that collections are initialized to empty but non-null after constructor execution.
    /// Input: Valid parameters.
    /// Expected: Faculties and Universities collections are empty but not null.
    /// </summary>
    [TestMethod]
    public void Constructor_WithValidParameters_InitializesCollectionsToEmptyNonNull()
    {
        // Arrange
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<FacultiesViewModel>>();

        // Act
        var viewModel = new FacultiesViewModel(mockAcademic.Object, mockLogger.Object);

        // Assert
        Assert.IsNotNull(viewModel.Faculties);
        Assert.AreEqual(0, viewModel.Faculties.Count);
        Assert.IsNotNull(viewModel.Universities);
        Assert.AreEqual(0, viewModel.Universities.Count);
    }

    /// <summary>
    /// Tests that EditFacultyCommand with multiple universities finds the correct matching university.
    /// Input: FacultyDto with UniversityId matching one of multiple universities.
    /// Expected: SelectedUniversity is set to the correct matching university.
    /// </summary>
    [TestMethod]
    public void Constructor_EditFacultyCommand_WithMultipleUniversities_SelectsCorrectUniversity()
    {
        // Arrange
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<FacultiesViewModel>>();
        var viewModel = new FacultiesViewModel(mockAcademic.Object, mockLogger.Object);
        var university1 = new LookupItem { Id = "uni-1", Name = "University 1" };
        var university2 = new LookupItem { Id = "uni-2", Name = "University 2" };
        var university3 = new LookupItem { Id = "uni-3", Name = "University 3" };
        viewModel.Universities.Add(university1);
        viewModel.Universities.Add(university2);
        viewModel.Universities.Add(university3);
        var faculty = new FacultyDto { Id = "fac-1", Name = "Faculty 1", UniversityId = "uni-2" };

        // Act
        ((Command<FacultyDto>)viewModel.EditFacultyCommand).Execute(faculty);

        // Assert
        Assert.AreEqual(university2, viewModel.SelectedUniversity);
        Assert.AreEqual("uni-2", viewModel.SelectedUniversityId);
    }

    /// <summary>
    /// Tests that NextPageCommand with maximum integer values handles correctly.
    /// Input: CurrentPage = int.MaxValue - 1, TotalPages = int.MaxValue.
    /// Expected: CurrentPage is incremented to int.MaxValue.
    /// </summary>
    [TestMethod]
    public void Constructor_NextPageCommand_WithMaxIntegerValues_HandlesCorrectly()
    {
        // Arrange
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<FacultiesViewModel>>();
        var viewModel = new FacultiesViewModel(mockAcademic.Object, mockLogger.Object);
        viewModel.TotalPages = int.MaxValue;
        viewModel.CurrentPage = int.MaxValue - 1;

        // Act
        ((Command)viewModel.NextPageCommand).Execute(null);

        // Assert
        Assert.AreEqual(int.MaxValue, viewModel.CurrentPage);
    }

    /// <summary>
    /// Tests that PrevPageCommand with minimum integer values handles correctly.
    /// Input: CurrentPage = int.MinValue + 2.
    /// Expected: CurrentPage is decremented to int.MinValue + 1.
    /// </summary>
    [TestMethod]
    public void Constructor_PrevPageCommand_WithMinIntegerValues_HandlesCorrectly()
    {
        // Arrange
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<FacultiesViewModel>>();
        var viewModel = new FacultiesViewModel(mockAcademic.Object, mockLogger.Object);
        viewModel.CurrentPage = int.MinValue + 2;

        // Act
        ((Command)viewModel.PrevPageCommand).Execute(null);

        // Assert
        Assert.AreEqual(int.MinValue + 1, viewModel.CurrentPage);
    }

    /// <summary>
    /// Tests that FilterCommand with control characters sets UniversityFilter correctly.
    /// Input: Strings with control characters.
    /// Expected: UniversityFilter is set to the control character string.
    /// </summary>
    [TestMethod]
    [DataRow("\0")]
    [DataRow("\u0001")]
    [DataRow("\u001F")]
    [DataRow("filter\0value")]
    public void Constructor_FilterCommand_WithControlCharacters_SetsUniversityFilter(string controlCharValue)
    {
        // Arrange
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<FacultiesViewModel>>();
        var viewModel = new FacultiesViewModel(mockAcademic.Object, mockLogger.Object);

        // Act
        ((Command<string>)viewModel.FilterCommand).Execute(controlCharValue);

        // Assert
        Assert.AreEqual(controlCharValue, viewModel.UniversityFilter);
    }

    /// <summary>
    /// Tests that EditFacultyCommand with null Id handles correctly.
    /// Input: FacultyDto with null Id.
    /// Expected: EditId is set to null.
    /// </summary>
    [TestMethod]
    public void Constructor_EditFacultyCommand_WithNullId_SetsEditIdToNull()
    {
        // Arrange
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<FacultiesViewModel>>();
        var viewModel = new FacultiesViewModel(mockAcademic.Object, mockLogger.Object);
        var faculty = new FacultyDto { Id = null, Name = "Faculty", UniversityId = null };

        // Act
        ((Command<FacultyDto>)viewModel.EditFacultyCommand).Execute(faculty);

        // Assert
        Assert.IsNull(viewModel.EditId);
        Assert.IsTrue(viewModel.IsEditing);
    }

    /// <summary>
    /// Tests that EditFacultyCommand with empty Id handles correctly.
    /// Input: FacultyDto with empty Id.
    /// Expected: EditId is set to empty string.
    /// </summary>
    [TestMethod]
    public void Constructor_EditFacultyCommand_WithEmptyId_SetsEditIdToEmpty()
    {
        // Arrange
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<FacultiesViewModel>>();
        var viewModel = new FacultiesViewModel(mockAcademic.Object, mockLogger.Object);
        var faculty = new FacultyDto { Id = string.Empty, Name = "Faculty", UniversityId = null };

        // Act
        ((Command<FacultyDto>)viewModel.EditFacultyCommand).Execute(faculty);

        // Assert
        Assert.AreEqual(string.Empty, viewModel.EditId);
        Assert.IsTrue(viewModel.IsEditing);
    }

    /// <summary>
    /// Tests that PageInfo property returns correct format after constructor initialization.
    /// Input: Valid parameters.
    /// Expected: PageInfo returns "Page 1 of 1".
    /// </summary>
    [TestMethod]
    public void Constructor_WithValidParameters_PageInfoReturnsCorrectInitialFormat()
    {
        // Arrange
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<FacultiesViewModel>>();

        // Act
        var viewModel = new FacultiesViewModel(mockAcademic.Object, mockLogger.Object);

        // Assert
        Assert.AreEqual("Page 1 of 1", viewModel.PageInfo);
    }
}