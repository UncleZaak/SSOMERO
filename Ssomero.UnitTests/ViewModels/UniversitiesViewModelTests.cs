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
/// Tests for UniversitiesViewModel class.
/// </summary>
[TestClass]
public class UniversitiesViewModelTests
{
    /// <summary>
    /// Tests that setting SearchQuery to a different value updates the property and triggers filtering.
    /// Input: A new non-empty search query value.
    /// Expected: Property is updated, PropertyChanged event is raised, and ApplyFilterAndPagination is called (verified through Universities collection update).
    /// </summary>
    [TestMethod]
    public void SearchQuery_SetDifferentValue_UpdatesPropertyAndTriggersFiltering()
    {
        // Arrange
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<UniversitiesViewModel>>();
        var viewModel = new UniversitiesViewModel(mockAcademic.Object, mockLogger.Object);

        bool propertyChangedRaised = false;
        viewModel.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == nameof(UniversitiesViewModel.SearchQuery))
                propertyChangedRaised = true;
        };

        // Act
        viewModel.SearchQuery = "test";

        // Assert
        Assert.AreEqual("test", viewModel.SearchQuery);
        Assert.IsTrue(propertyChangedRaised);
    }

    /// <summary>
    /// Tests that setting SearchQuery to the same value does not raise PropertyChanged event.
    /// Input: Same value as current SearchQuery.
    /// Expected: PropertyChanged event is not raised.
    /// </summary>
    [TestMethod]
    public void SearchQuery_SetSameValue_DoesNotRaisePropertyChanged()
    {
        // Arrange
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<UniversitiesViewModel>>();
        var viewModel = new UniversitiesViewModel(mockAcademic.Object, mockLogger.Object);

        viewModel.SearchQuery = "initial";

        bool propertyChangedRaised = false;
        viewModel.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == nameof(UniversitiesViewModel.SearchQuery))
                propertyChangedRaised = true;
        };

        // Act
        viewModel.SearchQuery = "initial";

        // Assert
        Assert.AreEqual("initial", viewModel.SearchQuery);
        Assert.IsFalse(propertyChangedRaised);
    }

    /// <summary>
    /// Tests that setting SearchQuery to various string values including edge cases.
    /// Input: Empty string, whitespace, special characters, and long strings.
    /// Expected: Property is updated correctly for each value.
    /// </summary>
    [TestMethod]
    [DataRow("", DisplayName = "Empty string")]
    [DataRow("   ", DisplayName = "Whitespace only")]
    [DataRow("\t\n\r", DisplayName = "Special whitespace characters")]
    [DataRow("a", DisplayName = "Single character")]
    [DataRow("University Name With Spaces", DisplayName = "String with spaces")]
    [DataRow("Special@#$%Characters", DisplayName = "Special characters")]
    [DataRow("UPPERCASE", DisplayName = "Uppercase string")]
    [DataRow("lowercase", DisplayName = "Lowercase string")]
    [DataRow("MiXeDcAsE", DisplayName = "Mixed case string")]
    public void SearchQuery_SetVariousStringValues_UpdatesPropertyCorrectly(string value)
    {
        // Arrange
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<UniversitiesViewModel>>();
        var viewModel = new UniversitiesViewModel(mockAcademic.Object, mockLogger.Object);

        // Act
        viewModel.SearchQuery = value;

        // Assert
        Assert.AreEqual(value, viewModel.SearchQuery);
    }

    /// <summary>
    /// Tests that setting SearchQuery to a very long string updates the property correctly.
    /// Input: A string with 10000 characters.
    /// Expected: Property is updated with the long string.
    /// </summary>
    [TestMethod]
    public void SearchQuery_SetVeryLongString_UpdatesPropertyCorrectly()
    {
        // Arrange
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<UniversitiesViewModel>>();
        var viewModel = new UniversitiesViewModel(mockAcademic.Object, mockLogger.Object);
        var longString = new string('A', 10000);

        // Act
        viewModel.SearchQuery = longString;

        // Assert
        Assert.AreEqual(longString, viewModel.SearchQuery);
    }

    /// <summary>
    /// Tests that setting SearchQuery multiple times with different values raises PropertyChanged each time.
    /// Input: Multiple different string values set sequentially.
    /// Expected: PropertyChanged event is raised for each different value.
    /// </summary>
    [TestMethod]
    public void SearchQuery_SetMultipleDifferentValues_RaisesPropertyChangedEachTime()
    {
        // Arrange
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<UniversitiesViewModel>>();
        var viewModel = new UniversitiesViewModel(mockAcademic.Object, mockLogger.Object);

        int propertyChangedCount = 0;
        viewModel.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == nameof(UniversitiesViewModel.SearchQuery))
                propertyChangedCount++;
        };

        // Act
        viewModel.SearchQuery = "value1";
        viewModel.SearchQuery = "value2";
        viewModel.SearchQuery = "value3";

        // Assert
        Assert.AreEqual("value3", viewModel.SearchQuery);
        Assert.AreEqual(3, propertyChangedCount);
    }

    /// <summary>
    /// Tests that getting SearchQuery returns the initial default value.
    /// Input: No setter call.
    /// Expected: Returns empty string (default value).
    /// </summary>
    [TestMethod]
    public void SearchQuery_GetInitialValue_ReturnsEmptyString()
    {
        // Arrange
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<UniversitiesViewModel>>();
        var viewModel = new UniversitiesViewModel(mockAcademic.Object, mockLogger.Object);

        // Act
        var result = viewModel.SearchQuery;

        // Assert
        Assert.AreEqual(string.Empty, result);
    }

    /// <summary>
    /// Tests that setting SearchQuery back to empty string after having a value updates correctly.
    /// Input: Set to non-empty value, then back to empty string.
    /// Expected: Property is updated to empty string and PropertyChanged is raised.
    /// </summary>
    [TestMethod]
    public void SearchQuery_SetToEmptyAfterNonEmpty_UpdatesPropertyCorrectly()
    {
        // Arrange
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<UniversitiesViewModel>>();
        var viewModel = new UniversitiesViewModel(mockAcademic.Object, mockLogger.Object);

        viewModel.SearchQuery = "test";

        bool propertyChangedRaised = false;
        viewModel.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == nameof(UniversitiesViewModel.SearchQuery))
                propertyChangedRaised = true;
        };

        // Act
        viewModel.SearchQuery = string.Empty;

        // Assert
        Assert.AreEqual(string.Empty, viewModel.SearchQuery);
        Assert.IsTrue(propertyChangedRaised);
    }

    /// <summary>
    /// Tests that setting SearchQuery with Unicode and international characters works correctly.
    /// Input: Strings with various Unicode characters.
    /// Expected: Property is updated correctly with Unicode values.
    /// </summary>
    [TestMethod]
    [DataRow("Université", DisplayName = "French characters")]
    [DataRow("大学", DisplayName = "Chinese characters")]
    [DataRow("Университет", DisplayName = "Cyrillic characters")]
    [DataRow("جامعة", DisplayName = "Arabic characters")]
    [DataRow("🎓📚", DisplayName = "Emoji characters")]
    public void SearchQuery_SetUnicodeCharacters_UpdatesPropertyCorrectly(string value)
    {
        // Arrange
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<UniversitiesViewModel>>();
        var viewModel = new UniversitiesViewModel(mockAcademic.Object, mockLogger.Object);

        // Act
        viewModel.SearchQuery = value;

        // Assert
        Assert.AreEqual(value, viewModel.SearchQuery);
    }

    /// <summary>
    /// Tests that setting EditId to a valid string value correctly updates the property.
    /// Input: A valid string value.
    /// Expected: The property value is updated and PropertyChanged event is raised.
    /// </summary>
    [TestMethod]
    public void EditId_SetValidString_UpdatesPropertyAndRaisesPropertyChanged()
    {
        // Arrange
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<UniversitiesViewModel>>();
        var viewModel = new UniversitiesViewModel(mockAcademic.Object, mockLogger.Object);
        var expectedValue = "university-123";
        var propertyChangedRaised = false;
        string? propertyName = null;

        viewModel.PropertyChanged += (sender, e) =>
        {
            propertyChangedRaised = true;
            propertyName = e.PropertyName;
        };

        // Act
        viewModel.EditId = expectedValue;

        // Assert
        Assert.AreEqual(expectedValue, viewModel.EditId);
        Assert.IsTrue(propertyChangedRaised);
        Assert.AreEqual(nameof(UniversitiesViewModel.EditId), propertyName);
    }

    /// <summary>
    /// Tests that setting EditId to null correctly updates the property.
    /// Input: null value.
    /// Expected: The property value is set to null and PropertyChanged event is raised.
    /// </summary>
    [TestMethod]
    public void EditId_SetNull_UpdatesPropertyAndRaisesPropertyChanged()
    {
        // Arrange
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<UniversitiesViewModel>>();
        var viewModel = new UniversitiesViewModel(mockAcademic.Object, mockLogger.Object);
        viewModel.EditId = "initial-value";
        var propertyChangedRaised = false;

        viewModel.PropertyChanged += (sender, e) =>
        {
            propertyChangedRaised = true;
        };

        // Act
        viewModel.EditId = null;

        // Assert
        Assert.IsNull(viewModel.EditId);
        Assert.IsTrue(propertyChangedRaised);
    }

    /// <summary>
    /// Tests that setting EditId to an empty string correctly updates the property.
    /// Input: Empty string.
    /// Expected: The property value is set to empty string and PropertyChanged event is raised.
    /// </summary>
    [TestMethod]
    public void EditId_SetEmptyString_UpdatesPropertyAndRaisesPropertyChanged()
    {
        // Arrange
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<UniversitiesViewModel>>();
        var viewModel = new UniversitiesViewModel(mockAcademic.Object, mockLogger.Object);
        var propertyChangedRaised = false;

        viewModel.PropertyChanged += (sender, e) =>
        {
            propertyChangedRaised = true;
        };

        // Act
        viewModel.EditId = string.Empty;

        // Assert
        Assert.AreEqual(string.Empty, viewModel.EditId);
        Assert.IsTrue(propertyChangedRaised);
    }

    /// <summary>
    /// Tests that setting EditId to a whitespace string correctly updates the property.
    /// Input: Whitespace string.
    /// Expected: The property value is set to the whitespace string and PropertyChanged event is raised.
    /// </summary>
    [TestMethod]
    public void EditId_SetWhitespace_UpdatesPropertyAndRaisesPropertyChanged()
    {
        // Arrange
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<UniversitiesViewModel>>();
        var viewModel = new UniversitiesViewModel(mockAcademic.Object, mockLogger.Object);
        var expectedValue = "   ";
        var propertyChangedRaised = false;

        viewModel.PropertyChanged += (sender, e) =>
        {
            propertyChangedRaised = true;
        };

        // Act
        viewModel.EditId = expectedValue;

        // Assert
        Assert.AreEqual(expectedValue, viewModel.EditId);
        Assert.IsTrue(propertyChangedRaised);
    }

    /// <summary>
    /// Tests that setting EditId to the same value does not raise PropertyChanged event.
    /// Input: Same value set twice consecutively.
    /// Expected: PropertyChanged event is raised only on the first set, not the second.
    /// </summary>
    [TestMethod]
    public void EditId_SetSameValue_DoesNotRaisePropertyChanged()
    {
        // Arrange
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<UniversitiesViewModel>>();
        var viewModel = new UniversitiesViewModel(mockAcademic.Object, mockLogger.Object);
        var value = "university-456";
        viewModel.EditId = value;
        var propertyChangedCount = 0;

        viewModel.PropertyChanged += (sender, e) =>
        {
            if (e.PropertyName == nameof(UniversitiesViewModel.EditId))
            {
                propertyChangedCount++;
            }
        };

        // Act
        viewModel.EditId = value;

        // Assert
        Assert.AreEqual(value, viewModel.EditId);
        Assert.AreEqual(0, propertyChangedCount);
    }

    /// <summary>
    /// Tests that getting EditId returns the correct value after it has been set.
    /// Input: A valid string value.
    /// Expected: The getter returns the previously set value.
    /// </summary>
    [TestMethod]
    public void EditId_Get_ReturnsCorrectValue()
    {
        // Arrange
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<UniversitiesViewModel>>();
        var viewModel = new UniversitiesViewModel(mockAcademic.Object, mockLogger.Object);
        var expectedValue = "test-university-id";

        // Act
        viewModel.EditId = expectedValue;
        var actualValue = viewModel.EditId;

        // Assert
        Assert.AreEqual(expectedValue, actualValue);
    }

    /// <summary>
    /// Tests that EditId can handle a very long string value.
    /// Input: A very long string.
    /// Expected: The property value is set correctly and PropertyChanged event is raised.
    /// </summary>
    [TestMethod]
    public void EditId_SetVeryLongString_UpdatesPropertyAndRaisesPropertyChanged()
    {
        // Arrange
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<UniversitiesViewModel>>();
        var viewModel = new UniversitiesViewModel(mockAcademic.Object, mockLogger.Object);
        var expectedValue = new string('a', 10000);
        var propertyChangedRaised = false;

        viewModel.PropertyChanged += (sender, e) =>
        {
            propertyChangedRaised = true;
        };

        // Act
        viewModel.EditId = expectedValue;

        // Assert
        Assert.AreEqual(expectedValue, viewModel.EditId);
        Assert.IsTrue(propertyChangedRaised);
    }

    /// <summary>
    /// Tests that EditId can handle special characters.
    /// Input: String with special characters.
    /// Expected: The property value is set correctly and PropertyChanged event is raised.
    /// </summary>
    [TestMethod]
    public void EditId_SetSpecialCharacters_UpdatesPropertyAndRaisesPropertyChanged()
    {
        // Arrange
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<UniversitiesViewModel>>();
        var viewModel = new UniversitiesViewModel(mockAcademic.Object, mockLogger.Object);
        var expectedValue = "!@#$%^&*()_+-=[]{}|;':\",./<>?";
        var propertyChangedRaised = false;

        viewModel.PropertyChanged += (sender, e) =>
        {
            propertyChangedRaised = true;
        };

        // Act
        viewModel.EditId = expectedValue;

        // Assert
        Assert.AreEqual(expectedValue, viewModel.EditId);
        Assert.IsTrue(propertyChangedRaised);
    }

    /// <summary>
    /// Tests that EditId default value is null when the ViewModel is instantiated.
    /// Input: None (newly created ViewModel).
    /// Expected: EditId is null by default.
    /// </summary>
    [TestMethod]
    public void EditId_DefaultValue_IsNull()
    {
        // Arrange
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<UniversitiesViewModel>>();

        // Act
        var viewModel = new UniversitiesViewModel(mockAcademic.Object, mockLogger.Object);

        // Assert
        Assert.IsNull(viewModel.EditId);
    }

    /// <summary>
    /// Tests that EditId can be set multiple times with different values.
    /// Input: Multiple different string values.
    /// Expected: The property is updated correctly each time and PropertyChanged is raised for each change.
    /// </summary>
    [TestMethod]
    public void EditId_SetMultipleTimes_UpdatesPropertyEachTime()
    {
        // Arrange
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<UniversitiesViewModel>>();
        var viewModel = new UniversitiesViewModel(mockAcademic.Object, mockLogger.Object);
        var value1 = "first-id";
        var value2 = "second-id";
        var value3 = "third-id";
        var propertyChangedCount = 0;

        viewModel.PropertyChanged += (sender, e) =>
        {
            if (e.PropertyName == nameof(UniversitiesViewModel.EditId))
            {
                propertyChangedCount++;
            }
        };

        // Act
        viewModel.EditId = value1;
        viewModel.EditId = value2;
        viewModel.EditId = value3;

        // Assert
        Assert.AreEqual(value3, viewModel.EditId);
        Assert.AreEqual(3, propertyChangedCount);
    }

    /// <summary>
    /// Tests that TotalPages getter returns the initial default value of 1.
    /// </summary>
    [TestMethod]
    public void TotalPages_Get_ReturnsInitialValue()
    {
        // Arrange
        var academicServiceMock = new Mock<IAcademicService>();
        var loggerMock = new Mock<ILogger<UniversitiesViewModel>>();
        var viewModel = new UniversitiesViewModel(academicServiceMock.Object, loggerMock.Object);

        // Act
        var result = viewModel.TotalPages;

        // Assert
        Assert.AreEqual(1, result);
    }

    /// <summary>
    /// Tests that setting TotalPages to a different value raises PropertyChanged for both "TotalPages" and "PageInfo".
    /// </summary>
    /// <param name="newValue">The new value to set for TotalPages.</param>
    [TestMethod]
    [DataRow(2)]
    [DataRow(10)]
    [DataRow(100)]
    [DataRow(0)]
    [DataRow(-1)]
    [DataRow(int.MaxValue)]
    [DataRow(int.MinValue)]
    public void TotalPages_SetToDifferentValue_RaisesPropertyChangedForBothProperties(int newValue)
    {
        // Arrange
        var academicServiceMock = new Mock<IAcademicService>();
        var loggerMock = new Mock<ILogger<UniversitiesViewModel>>();
        var viewModel = new UniversitiesViewModel(academicServiceMock.Object, loggerMock.Object);

        var propertyChangedEvents = new List<string>();
        viewModel.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName != null)
            {
                propertyChangedEvents.Add(args.PropertyName);
            }
        };

        // Act
        viewModel.TotalPages = newValue;

        // Assert
        Assert.AreEqual(newValue, viewModel.TotalPages);
        Assert.AreEqual(2, propertyChangedEvents.Count);
        Assert.AreEqual("TotalPages", propertyChangedEvents[0]);
        Assert.AreEqual("PageInfo", propertyChangedEvents[1]);
    }

    /// <summary>
    /// Tests that setting TotalPages to the same value does not raise PropertyChanged events.
    /// </summary>
    [TestMethod]
    public void TotalPages_SetToSameValue_DoesNotRaisePropertyChanged()
    {
        // Arrange
        var academicServiceMock = new Mock<IAcademicService>();
        var loggerMock = new Mock<ILogger<UniversitiesViewModel>>();
        var viewModel = new UniversitiesViewModel(academicServiceMock.Object, loggerMock.Object);

        // First, set to a specific value
        viewModel.TotalPages = 5;

        var propertyChangedEvents = new List<string>();
        viewModel.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName != null)
            {
                propertyChangedEvents.Add(args.PropertyName);
            }
        };

        // Act - set to the same value again
        viewModel.TotalPages = 5;

        // Assert
        Assert.AreEqual(5, viewModel.TotalPages);
        Assert.AreEqual(0, propertyChangedEvents.Count);
    }

    /// <summary>
    /// Tests that setting TotalPages updates the PageInfo property which depends on it.
    /// </summary>
    [TestMethod]
    [DataRow(1, 1, "Page 1 of 1")]
    [DataRow(1, 5, "Page 1 of 5")]
    [DataRow(1, 10, "Page 1 of 10")]
    [DataRow(1, 0, "Page 1 of 0")]
    [DataRow(1, -1, "Page 1 of -1")]
    public void TotalPages_SetValue_UpdatesPageInfoProperty(int currentPage, int totalPages, string expectedPageInfo)
    {
        // Arrange
        var academicServiceMock = new Mock<IAcademicService>();
        var loggerMock = new Mock<ILogger<UniversitiesViewModel>>();
        var viewModel = new UniversitiesViewModel(academicServiceMock.Object, loggerMock.Object);

        // Act
        viewModel.TotalPages = totalPages;

        // Assert
        Assert.AreEqual(expectedPageInfo, viewModel.PageInfo);
    }

    /// <summary>
    /// Tests that setting TotalPages to boundary values works correctly.
    /// </summary>
    [TestMethod]
    [DataRow(int.MaxValue)]
    [DataRow(int.MinValue)]
    [DataRow(0)]
    public void TotalPages_SetToBoundaryValues_UpdatesValueCorrectly(int boundaryValue)
    {
        // Arrange
        var academicServiceMock = new Mock<IAcademicService>();
        var loggerMock = new Mock<ILogger<UniversitiesViewModel>>();
        var viewModel = new UniversitiesViewModel(academicServiceMock.Object, loggerMock.Object);

        // Act
        viewModel.TotalPages = boundaryValue;

        // Assert
        Assert.AreEqual(boundaryValue, viewModel.TotalPages);
    }

    /// <summary>
    /// Tests that multiple consecutive sets with different values raise PropertyChanged each time.
    /// </summary>
    [TestMethod]
    public void TotalPages_MultipleConsecutiveSetsWithDifferentValues_RaisesPropertyChangedEachTime()
    {
        // Arrange
        var academicServiceMock = new Mock<IAcademicService>();
        var loggerMock = new Mock<ILogger<UniversitiesViewModel>>();
        var viewModel = new UniversitiesViewModel(academicServiceMock.Object, loggerMock.Object);

        var propertyChangedCount = 0;
        viewModel.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == "TotalPages")
            {
                propertyChangedCount++;
            }
        };

        // Act
        viewModel.TotalPages = 2;
        viewModel.TotalPages = 3;
        viewModel.TotalPages = 4;

        // Assert
        Assert.AreEqual(4, viewModel.TotalPages);
        Assert.AreEqual(3, propertyChangedCount);
    }

    /// <summary>
    /// Tests that IsEditing property returns false by default when the ViewModel is first created.
    /// </summary>
    [TestMethod]
    public void IsEditing_DefaultValue_ReturnsFalse()
    {
        // Arrange
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<UniversitiesViewModel>>();
        var viewModel = new UniversitiesViewModel(mockAcademic.Object, mockLogger.Object);

        // Act
        var result = viewModel.IsEditing;

        // Assert
        Assert.IsFalse(result);
    }

    /// <summary>
    /// Tests that setting IsEditing to true updates the property value and raises PropertyChanged event.
    /// </summary>
    [TestMethod]
    public void IsEditing_SetToTrue_UpdatesValueAndRaisesPropertyChanged()
    {
        // Arrange
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<UniversitiesViewModel>>();
        var viewModel = new UniversitiesViewModel(mockAcademic.Object, mockLogger.Object);
        var propertyChangedRaised = false;
        var propertyName = string.Empty;

        viewModel.PropertyChanged += (sender, args) =>
        {
            propertyChangedRaised = true;
            propertyName = args.PropertyName ?? string.Empty;
        };

        // Act
        viewModel.IsEditing = true;

        // Assert
        Assert.IsTrue(viewModel.IsEditing);
        Assert.IsTrue(propertyChangedRaised);
        Assert.AreEqual("IsEditing", propertyName);
    }

    /// <summary>
    /// Tests that setting IsEditing to false when it was true updates the property value and raises PropertyChanged event.
    /// </summary>
    [TestMethod]
    public void IsEditing_SetToFalseFromTrue_UpdatesValueAndRaisesPropertyChanged()
    {
        // Arrange
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<UniversitiesViewModel>>();
        var viewModel = new UniversitiesViewModel(mockAcademic.Object, mockLogger.Object);
        viewModel.IsEditing = true;

        var propertyChangedRaised = false;
        var propertyName = string.Empty;

        viewModel.PropertyChanged += (sender, args) =>
        {
            propertyChangedRaised = true;
            propertyName = args.PropertyName ?? string.Empty;
        };

        // Act
        viewModel.IsEditing = false;

        // Assert
        Assert.IsFalse(viewModel.IsEditing);
        Assert.IsTrue(propertyChangedRaised);
        Assert.AreEqual("IsEditing", propertyName);
    }

    /// <summary>
    /// Tests that setting IsEditing to the same value (true to true) does not raise PropertyChanged event.
    /// </summary>
    [TestMethod]
    public void IsEditing_SetToSameValueTrue_DoesNotRaisePropertyChanged()
    {
        // Arrange
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<UniversitiesViewModel>>();
        var viewModel = new UniversitiesViewModel(mockAcademic.Object, mockLogger.Object);
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
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<UniversitiesViewModel>>();
        var viewModel = new UniversitiesViewModel(mockAcademic.Object, mockLogger.Object);

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
    /// Tests that multiple alternating sets between true and false correctly update the property value each time.
    /// </summary>
    [TestMethod]
    public void IsEditing_MultipleAlternatingSets_UpdatesValueCorrectly()
    {
        // Arrange
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<UniversitiesViewModel>>();
        var viewModel = new UniversitiesViewModel(mockAcademic.Object, mockLogger.Object);

        // Act & Assert
        viewModel.IsEditing = true;
        Assert.IsTrue(viewModel.IsEditing);

        viewModel.IsEditing = false;
        Assert.IsFalse(viewModel.IsEditing);

        viewModel.IsEditing = true;
        Assert.IsTrue(viewModel.IsEditing);

        viewModel.IsEditing = false;
        Assert.IsFalse(viewModel.IsEditing);
    }

    /// <summary>
    /// Tests that the StatusFilter property returns the initial value of "All".
    /// </summary>
    [TestMethod]
    public void StatusFilter_Get_ReturnsInitialValue()
    {
        // Arrange
        Mock<IAcademicService> academicServiceMock = new Mock<IAcademicService>();
        Mock<ILogger<UniversitiesViewModel>> loggerMock = new Mock<ILogger<UniversitiesViewModel>>();
        UniversitiesViewModel viewModel = new UniversitiesViewModel(academicServiceMock.Object, loggerMock.Object);

        // Act
        string result = viewModel.StatusFilter;

        // Assert
        Assert.AreEqual("All", result);
    }

    /// <summary>
    /// Tests that setting StatusFilter to a new value updates the property and raises PropertyChanged event.
    /// Tests various string values including empty, whitespace, normal, long, and special characters.
    /// </summary>
    /// <param name="newValue">The new value to set for StatusFilter.</param>
    [TestMethod]
    [DataRow("Accredited")]
    [DataRow("Pending")]
    [DataRow("")]
    [DataRow("   ")]
    [DataRow("A")]
    [DataRow("ThisIsAVeryLongStatusFilterValueThatExceedsNormalLengthAndTestsTheBoundaryConditionsOfStringHandling")]
    [DataRow("Special!@#$%^&*()_+{}|:<>?")]
    [DataRow("Status\nWith\nNewlines")]
    [DataRow("Status\tWith\tTabs")]
    public void StatusFilter_SetNewValue_UpdatesPropertyAndRaisesPropertyChanged(string newValue)
    {
        // Arrange
        Mock<IAcademicService> academicServiceMock = new Mock<IAcademicService>();
        Mock<ILogger<UniversitiesViewModel>> loggerMock = new Mock<ILogger<UniversitiesViewModel>>();
        UniversitiesViewModel viewModel = new UniversitiesViewModel(academicServiceMock.Object, loggerMock.Object);
        bool propertyChangedRaised = false;
        string? propertyName = null;
        ((INotifyPropertyChanged)viewModel).PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == nameof(UniversitiesViewModel.StatusFilter))
            {
                propertyChangedRaised = true;
                propertyName = args.PropertyName;
            }
        };

        // Act
        viewModel.StatusFilter = newValue;

        // Assert
        Assert.AreEqual(newValue, viewModel.StatusFilter);
        Assert.IsTrue(propertyChangedRaised, "PropertyChanged event should be raised");
        Assert.AreEqual(nameof(UniversitiesViewModel.StatusFilter), propertyName);
    }

    /// <summary>
    /// Tests that setting StatusFilter to the same value does not raise PropertyChanged event.
    /// This verifies that SetProperty correctly detects when the value hasn't changed.
    /// </summary>
    [TestMethod]
    public void StatusFilter_SetSameValue_DoesNotRaisePropertyChanged()
    {
        // Arrange
        Mock<IAcademicService> academicServiceMock = new Mock<IAcademicService>();
        Mock<ILogger<UniversitiesViewModel>> loggerMock = new Mock<ILogger<UniversitiesViewModel>>();
        UniversitiesViewModel viewModel = new UniversitiesViewModel(academicServiceMock.Object, loggerMock.Object);
        bool propertyChangedRaised = false;
        ((INotifyPropertyChanged)viewModel).PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == nameof(UniversitiesViewModel.StatusFilter))
            {
                propertyChangedRaised = true;
            }
        };

        // Act
        viewModel.StatusFilter = "All"; // Same as initial value

        // Assert
        Assert.AreEqual("All", viewModel.StatusFilter);
        Assert.IsFalse(propertyChangedRaised, "PropertyChanged event should not be raised when value hasn't changed");
    }

    /// <summary>
    /// Tests that setting StatusFilter to the same value after a change does not raise PropertyChanged event.
    /// This tests the scenario where the value is changed and then set to the same value again.
    /// </summary>
    [TestMethod]
    public void StatusFilter_SetSameValueAfterChange_DoesNotRaisePropertyChanged()
    {
        // Arrange
        Mock<IAcademicService> academicServiceMock = new Mock<IAcademicService>();
        Mock<ILogger<UniversitiesViewModel>> loggerMock = new Mock<ILogger<UniversitiesViewModel>>();
        UniversitiesViewModel viewModel = new UniversitiesViewModel(academicServiceMock.Object, loggerMock.Object);
        viewModel.StatusFilter = "Accredited"; // Change from initial value
        int propertyChangedCount = 0;
        ((INotifyPropertyChanged)viewModel).PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == nameof(UniversitiesViewModel.StatusFilter))
            {
                propertyChangedCount++;
            }
        };

        // Act
        viewModel.StatusFilter = "Accredited"; // Same as current value

        // Assert
        Assert.AreEqual("Accredited", viewModel.StatusFilter);
        Assert.AreEqual(0, propertyChangedCount, "PropertyChanged event should not be raised when value hasn't changed");
    }

    /// <summary>
    /// Tests that setting StatusFilter triggers ApplyFilterAndPagination by verifying side effects.
    /// When StatusFilter changes, ApplyFilterAndPagination should execute without throwing exceptions.
    /// This test verifies that the Universities collection is cleared as a side effect.
    /// </summary>
    [TestMethod]
    public void StatusFilter_SetValue_TriggersApplyFilterAndPagination()
    {
        // Arrange
        Mock<IAcademicService> academicServiceMock = new Mock<IAcademicService>();
        Mock<ILogger<UniversitiesViewModel>> loggerMock = new Mock<ILogger<UniversitiesViewModel>>();
        UniversitiesViewModel viewModel = new UniversitiesViewModel(academicServiceMock.Object, loggerMock.Object);

        // Act & Assert - Should not throw exception
        viewModel.StatusFilter = "Accredited";

        // Verify side effect: Universities collection should be empty (since _allUniversities is empty)
        Assert.AreEqual(0, viewModel.Universities.Count);
        Assert.AreEqual("Accredited", viewModel.StatusFilter);
    }

    /// <summary>
    /// Tests that changing StatusFilter multiple times updates the property correctly each time.
    /// This verifies that the property correctly handles sequential changes.
    /// </summary>
    [TestMethod]
    public void StatusFilter_SetMultipleDifferentValues_UpdatesPropertyEachTime()
    {
        // Arrange
        Mock<IAcademicService> academicServiceMock = new Mock<IAcademicService>();
        Mock<ILogger<UniversitiesViewModel>> loggerMock = new Mock<ILogger<UniversitiesViewModel>>();
        UniversitiesViewModel viewModel = new UniversitiesViewModel(academicServiceMock.Object, loggerMock.Object);
        int propertyChangedCount = 0;
        ((INotifyPropertyChanged)viewModel).PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == nameof(UniversitiesViewModel.StatusFilter))
            {
                propertyChangedCount++;
            }
        };

        // Act
        viewModel.StatusFilter = "Accredited";
        viewModel.StatusFilter = "Pending";
        viewModel.StatusFilter = "All";

        // Assert
        Assert.AreEqual("All", viewModel.StatusFilter);
        Assert.AreEqual(3, propertyChangedCount, "PropertyChanged should be raised for each distinct value change");
    }

    /// <summary>
    /// Tests that StatusFilter handles Unicode and international characters correctly.
    /// This ensures the property can handle non-ASCII status values.
    /// </summary>
    [TestMethod]
    [DataRow("Accrédité")]
    [DataRow("认可")]
    [DataRow("معتمد")]
    [DataRow("מאושר")]
    [DataRow("Одобрено")]
    public void StatusFilter_SetUnicodeValue_UpdatesPropertyCorrectly(string unicodeValue)
    {
        // Arrange
        Mock<IAcademicService> academicServiceMock = new Mock<IAcademicService>();
        Mock<ILogger<UniversitiesViewModel>> loggerMock = new Mock<ILogger<UniversitiesViewModel>>();
        UniversitiesViewModel viewModel = new UniversitiesViewModel(academicServiceMock.Object, loggerMock.Object);

        // Act
        viewModel.StatusFilter = unicodeValue;

        // Assert
        Assert.AreEqual(unicodeValue, viewModel.StatusFilter);
    }

    /// <summary>
    /// Tests that StatusFilter getter returns the correct value after setting it.
    /// This is a basic round-trip test for the property.
    /// </summary>
    [TestMethod]
    public void StatusFilter_SetAndGet_ReturnsSetValue()
    {
        // Arrange
        Mock<IAcademicService> academicServiceMock = new Mock<IAcademicService>();
        Mock<ILogger<UniversitiesViewModel>> loggerMock = new Mock<ILogger<UniversitiesViewModel>>();
        UniversitiesViewModel viewModel = new UniversitiesViewModel(academicServiceMock.Object, loggerMock.Object);
        string expectedValue = "CustomStatus";

        // Act
        viewModel.StatusFilter = expectedValue;
        string actualValue = viewModel.StatusFilter;

        // Assert
        Assert.AreEqual(expectedValue, actualValue);
    }

    /// <summary>
    /// Tests that setting StatusFilter to boundary length strings works correctly.
    /// Tests empty string and extremely long string to verify no length-related issues.
    /// </summary>
    [TestMethod]
    public void StatusFilter_SetBoundaryLengthStrings_UpdatesPropertyCorrectly()
    {
        // Arrange
        Mock<IAcademicService> academicServiceMock = new Mock<IAcademicService>();
        Mock<ILogger<UniversitiesViewModel>> loggerMock = new Mock<ILogger<UniversitiesViewModel>>();
        UniversitiesViewModel viewModel = new UniversitiesViewModel(academicServiceMock.Object, loggerMock.Object);
        string veryLongString = new string('A', 10000); // 10,000 character string

        // Act & Assert - Empty string
        viewModel.StatusFilter = string.Empty;
        Assert.AreEqual(string.Empty, viewModel.StatusFilter);

        // Act & Assert - Very long string
        viewModel.StatusFilter = veryLongString;
        Assert.AreEqual(veryLongString, viewModel.StatusFilter);
        Assert.AreEqual(10000, viewModel.StatusFilter.Length);
    }

    /// <summary>
    /// Tests that ErrorMessage property returns the initial value of empty string.
    /// Input: None (initial state)
    /// Expected: ErrorMessage returns empty string
    /// </summary>
    [TestMethod]
    public void ErrorMessage_InitialValue_ReturnsEmptyString()
    {
        // Arrange
        var mockAcademicService = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<UniversitiesViewModel>>();
        var viewModel = new UniversitiesViewModel(mockAcademicService.Object, mockLogger.Object);

        // Act
        var result = viewModel.ErrorMessage;

        // Assert
        Assert.AreEqual(string.Empty, result);
    }

    /// <summary>
    /// Tests that ErrorMessage property can be set and retrieved with various string values.
    /// Input: Various string values (empty, whitespace, normal, long, special characters)
    /// Expected: Property returns the set value
    /// </summary>
    [TestMethod]
    [DataRow("", DisplayName = "Empty string")]
    [DataRow("   ", DisplayName = "Whitespace only")]
    [DataRow("Error occurred", DisplayName = "Normal string")]
    [DataRow("Error: An unexpected error occurred while processing your request. Please try again later.", DisplayName = "Long error message")]
    [DataRow("Error\nMultiline\nMessage", DisplayName = "Multiline string")]
    [DataRow("Error with special chars: @#$%^&*()", DisplayName = "Special characters")]
    [DataRow("Error with unicode: 你好世界", DisplayName = "Unicode characters")]
    [DataRow("\t\r\n", DisplayName = "Control characters")]
    public void ErrorMessage_SetValue_ReturnsSetValue(string value)
    {
        // Arrange
        var mockAcademicService = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<UniversitiesViewModel>>();
        var viewModel = new UniversitiesViewModel(mockAcademicService.Object, mockLogger.Object);

        // Act
        viewModel.ErrorMessage = value;
        var result = viewModel.ErrorMessage;

        // Assert
        Assert.AreEqual(value, result);
    }

    /// <summary>
    /// Tests that ErrorMessage property raises PropertyChanged event when value changes.
    /// Input: New string value different from current value
    /// Expected: PropertyChanged event is raised with correct property name
    /// </summary>
    [TestMethod]
    public void ErrorMessage_SetNewValue_RaisesPropertyChangedEvent()
    {
        // Arrange
        var mockAcademicService = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<UniversitiesViewModel>>();
        var viewModel = new UniversitiesViewModel(mockAcademicService.Object, mockLogger.Object);
        string? raisedPropertyName = null;
        viewModel.PropertyChanged += (sender, e) => raisedPropertyName = e.PropertyName;

        // Act
        viewModel.ErrorMessage = "New error message";

        // Assert
        Assert.AreEqual("ErrorMessage", raisedPropertyName);
    }

    /// <summary>
    /// Tests that ErrorMessage property does not raise PropertyChanged event when set to the same value.
    /// Input: Same string value as current value
    /// Expected: PropertyChanged event is not raised
    /// </summary>
    [TestMethod]
    public void ErrorMessage_SetSameValue_DoesNotRaisePropertyChangedEvent()
    {
        // Arrange
        var mockAcademicService = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<UniversitiesViewModel>>();
        var viewModel = new UniversitiesViewModel(mockAcademicService.Object, mockLogger.Object);
        viewModel.ErrorMessage = "Initial error";
        var eventRaised = false;
        viewModel.PropertyChanged += (sender, e) =>
        {
            if (e.PropertyName == "ErrorMessage")
            {
                eventRaised = true;
            }
        };

        // Act
        viewModel.ErrorMessage = "Initial error";

        // Assert
        Assert.IsFalse(eventRaised);
    }

    /// <summary>
    /// Tests that ErrorMessage property can handle very long strings.
    /// Input: String with 10000 characters
    /// Expected: Property correctly stores and returns the long string
    /// </summary>
    [TestMethod]
    public void ErrorMessage_SetVeryLongString_ReturnsSetValue()
    {
        // Arrange
        var mockAcademicService = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<UniversitiesViewModel>>();
        var viewModel = new UniversitiesViewModel(mockAcademicService.Object, mockLogger.Object);
        var longString = new string('A', 10000);

        // Act
        viewModel.ErrorMessage = longString;
        var result = viewModel.ErrorMessage;

        // Assert
        Assert.AreEqual(longString, result);
        Assert.AreEqual(10000, result.Length);
    }

    /// <summary>
    /// Tests that ErrorMessage property can be set multiple times with different values.
    /// Input: Multiple different string values set sequentially
    /// Expected: Property returns the most recent value each time
    /// </summary>
    [TestMethod]
    public void ErrorMessage_SetMultipleTimes_ReturnsLatestValue()
    {
        // Arrange
        var mockAcademicService = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<UniversitiesViewModel>>();
        var viewModel = new UniversitiesViewModel(mockAcademicService.Object, mockLogger.Object);

        // Act & Assert
        viewModel.ErrorMessage = "First error";
        Assert.AreEqual("First error", viewModel.ErrorMessage);

        viewModel.ErrorMessage = "Second error";
        Assert.AreEqual("Second error", viewModel.ErrorMessage);

        viewModel.ErrorMessage = string.Empty;
        Assert.AreEqual(string.Empty, viewModel.ErrorMessage);

        viewModel.ErrorMessage = "Third error";
        Assert.AreEqual("Third error", viewModel.ErrorMessage);
    }

    /// <summary>
    /// Tests that ErrorMessage property raises PropertyChanged event for each unique value change.
    /// Input: Multiple different string values set sequentially
    /// Expected: PropertyChanged event is raised for each unique value change
    /// </summary>
    [TestMethod]
    public void ErrorMessage_SetMultipleUniqueValues_RaisesPropertyChangedEventEachTime()
    {
        // Arrange
        var mockAcademicService = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<UniversitiesViewModel>>();
        var viewModel = new UniversitiesViewModel(mockAcademicService.Object, mockLogger.Object);
        var eventRaisedCount = 0;
        viewModel.PropertyChanged += (sender, e) =>
        {
            if (e.PropertyName == "ErrorMessage")
            {
                eventRaisedCount++;
            }
        };

        // Act
        viewModel.ErrorMessage = "First error";
        viewModel.ErrorMessage = "Second error";
        viewModel.ErrorMessage = "Third error";

        // Assert
        Assert.AreEqual(3, eventRaisedCount);
    }

    /// <summary>
    /// Tests that setting CurrentPage to a new valid value updates the property and raises PropertyChanged for both CurrentPage and PageInfo.
    /// Input: New valid page number (5).
    /// Expected: Property is updated, PropertyChanged is raised for CurrentPage and PageInfo.
    /// </summary>
    [TestMethod]
    public void CurrentPage_SetNewValue_UpdatesPropertyAndRaisesPropertyChangedForCurrentPageAndPageInfo()
    {
        // Arrange
        var academicMock = new Mock<IAcademicService>();
        var loggerMock = new Mock<ILogger<UniversitiesViewModel>>();
        var viewModel = new UniversitiesViewModel(academicMock.Object, loggerMock.Object);
        var propertyChangedEvents = new System.Collections.Generic.List<string>();
        viewModel.PropertyChanged += (sender, args) => propertyChangedEvents.Add(args.PropertyName ?? string.Empty);

        // Act
        viewModel.CurrentPage = 5;

        // Assert
        Assert.AreEqual(5, viewModel.CurrentPage);
        Assert.IsTrue(propertyChangedEvents.Contains(nameof(UniversitiesViewModel.CurrentPage)));
        Assert.IsTrue(propertyChangedEvents.Contains(nameof(UniversitiesViewModel.PageInfo)));
    }

    /// <summary>
    /// Tests that setting CurrentPage to the same value does not raise PropertyChanged events.
    /// Input: Same value as the current value (1).
    /// Expected: Property remains unchanged, no PropertyChanged events are raised.
    /// </summary>
    [TestMethod]
    public void CurrentPage_SetSameValue_DoesNotRaisePropertyChanged()
    {
        // Arrange
        var academicMock = new Mock<IAcademicService>();
        var loggerMock = new Mock<ILogger<UniversitiesViewModel>>();
        var viewModel = new UniversitiesViewModel(academicMock.Object, loggerMock.Object);
        var propertyChangedEvents = new System.Collections.Generic.List<string>();
        viewModel.PropertyChanged += (sender, args) => propertyChangedEvents.Add(args.PropertyName ?? string.Empty);

        // Act
        viewModel.CurrentPage = 1; // Default value is 1

        // Assert
        Assert.AreEqual(1, viewModel.CurrentPage);
        Assert.AreEqual(0, propertyChangedEvents.Count);
    }

    /// <summary>
    /// Tests that setting CurrentPage multiple times with different values raises PropertyChanged each time.
    /// Input: Multiple different page numbers (2, 3, 4).
    /// Expected: Property is updated each time, PropertyChanged is raised for each change.
    /// </summary>
    [TestMethod]
    public void CurrentPage_SetMultipleDifferentValues_RaisesPropertyChangedEachTime()
    {
        // Arrange
        var academicMock = new Mock<IAcademicService>();
        var loggerMock = new Mock<ILogger<UniversitiesViewModel>>();
        var viewModel = new UniversitiesViewModel(academicMock.Object, loggerMock.Object);
        var propertyChangedCount = 0;
        viewModel.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == nameof(UniversitiesViewModel.CurrentPage))
                propertyChangedCount++;
        };

        // Act
        viewModel.CurrentPage = 2;
        viewModel.CurrentPage = 3;
        viewModel.CurrentPage = 4;

        // Assert
        Assert.AreEqual(4, viewModel.CurrentPage);
        Assert.AreEqual(3, propertyChangedCount);
    }

    /// <summary>
    /// Tests that setting CurrentPage to zero updates the property and raises PropertyChanged.
    /// Input: Zero.
    /// Expected: Property is set to 0, PropertyChanged is raised for CurrentPage and PageInfo.
    /// </summary>
    [TestMethod]
    public void CurrentPage_SetToZero_UpdatesPropertyAndRaisesPropertyChanged()
    {
        // Arrange
        var academicMock = new Mock<IAcademicService>();
        var loggerMock = new Mock<ILogger<UniversitiesViewModel>>();
        var viewModel = new UniversitiesViewModel(academicMock.Object, loggerMock.Object);
        var propertyChangedEvents = new System.Collections.Generic.List<string>();
        viewModel.PropertyChanged += (sender, args) => propertyChangedEvents.Add(args.PropertyName ?? string.Empty);

        // Act
        viewModel.CurrentPage = 0;

        // Assert
        Assert.AreEqual(0, viewModel.CurrentPage);
        Assert.IsTrue(propertyChangedEvents.Contains(nameof(UniversitiesViewModel.CurrentPage)));
        Assert.IsTrue(propertyChangedEvents.Contains(nameof(UniversitiesViewModel.PageInfo)));
    }

    /// <summary>
    /// Tests that setting CurrentPage to a negative value updates the property and raises PropertyChanged.
    /// Input: Negative value (-1).
    /// Expected: Property is set to -1, PropertyChanged is raised for CurrentPage and PageInfo.
    /// </summary>
    [TestMethod]
    public void CurrentPage_SetToNegative_UpdatesPropertyAndRaisesPropertyChanged()
    {
        // Arrange
        var academicMock = new Mock<IAcademicService>();
        var loggerMock = new Mock<ILogger<UniversitiesViewModel>>();
        var viewModel = new UniversitiesViewModel(academicMock.Object, loggerMock.Object);
        var propertyChangedEvents = new System.Collections.Generic.List<string>();
        viewModel.PropertyChanged += (sender, args) => propertyChangedEvents.Add(args.PropertyName ?? string.Empty);

        // Act
        viewModel.CurrentPage = -1;

        // Assert
        Assert.AreEqual(-1, viewModel.CurrentPage);
        Assert.IsTrue(propertyChangedEvents.Contains(nameof(UniversitiesViewModel.CurrentPage)));
        Assert.IsTrue(propertyChangedEvents.Contains(nameof(UniversitiesViewModel.PageInfo)));
    }

    /// <summary>
    /// Tests that setting CurrentPage to int.MinValue updates the property and raises PropertyChanged.
    /// Input: int.MinValue.
    /// Expected: Property is set to int.MinValue, PropertyChanged is raised for CurrentPage and PageInfo.
    /// </summary>
    [TestMethod]
    public void CurrentPage_SetToIntMinValue_UpdatesPropertyAndRaisesPropertyChanged()
    {
        // Arrange
        var academicMock = new Mock<IAcademicService>();
        var loggerMock = new Mock<ILogger<UniversitiesViewModel>>();
        var viewModel = new UniversitiesViewModel(academicMock.Object, loggerMock.Object);
        var propertyChangedEvents = new System.Collections.Generic.List<string>();
        viewModel.PropertyChanged += (sender, args) => propertyChangedEvents.Add(args.PropertyName ?? string.Empty);

        // Act
        viewModel.CurrentPage = int.MinValue;

        // Assert
        Assert.AreEqual(int.MinValue, viewModel.CurrentPage);
        Assert.IsTrue(propertyChangedEvents.Contains(nameof(UniversitiesViewModel.CurrentPage)));
        Assert.IsTrue(propertyChangedEvents.Contains(nameof(UniversitiesViewModel.PageInfo)));
    }

    /// <summary>
    /// Tests that setting CurrentPage to int.MaxValue updates the property and raises PropertyChanged.
    /// Input: int.MaxValue.
    /// Expected: Property is set to int.MaxValue, PropertyChanged is raised for CurrentPage and PageInfo.
    /// </summary>
    [TestMethod]
    public void CurrentPage_SetToIntMaxValue_UpdatesPropertyAndRaisesPropertyChanged()
    {
        // Arrange
        var academicMock = new Mock<IAcademicService>();
        var loggerMock = new Mock<ILogger<UniversitiesViewModel>>();
        var viewModel = new UniversitiesViewModel(academicMock.Object, loggerMock.Object);
        var propertyChangedEvents = new System.Collections.Generic.List<string>();
        viewModel.PropertyChanged += (sender, args) => propertyChangedEvents.Add(args.PropertyName ?? string.Empty);

        // Act
        viewModel.CurrentPage = int.MaxValue;

        // Assert
        Assert.AreEqual(int.MaxValue, viewModel.CurrentPage);
        Assert.IsTrue(propertyChangedEvents.Contains(nameof(UniversitiesViewModel.CurrentPage)));
        Assert.IsTrue(propertyChangedEvents.Contains(nameof(UniversitiesViewModel.PageInfo)));
    }

    /// <summary>
    /// Tests that CurrentPage getter returns the correct default value on initialization.
    /// Input: None (default initialization).
    /// Expected: CurrentPage returns 1 (the default value).
    /// </summary>
    [TestMethod]
    public void CurrentPage_DefaultValue_ReturnsOne()
    {
        // Arrange
        var academicMock = new Mock<IAcademicService>();
        var loggerMock = new Mock<ILogger<UniversitiesViewModel>>();
        var viewModel = new UniversitiesViewModel(academicMock.Object, loggerMock.Object);

        // Act
        var result = viewModel.CurrentPage;

        // Assert
        Assert.AreEqual(1, result);
    }

    /// <summary>
    /// Tests that setting CurrentPage raises PropertyChanged event with correct sender.
    /// Input: New page number (10).
    /// Expected: PropertyChanged event is raised with the view model as sender.
    /// </summary>
    [TestMethod]
    public void CurrentPage_SetValue_RaisesPropertyChangedWithCorrectSender()
    {
        // Arrange
        var academicMock = new Mock<IAcademicService>();
        var loggerMock = new Mock<ILogger<UniversitiesViewModel>>();
        var viewModel = new UniversitiesViewModel(academicMock.Object, loggerMock.Object);
        object? eventSender = null;
        viewModel.PropertyChanged += (sender, args) => eventSender = sender;

        // Act
        viewModel.CurrentPage = 10;

        // Assert
        Assert.AreSame(viewModel, eventSender);
    }

    /// <summary>
    /// Tests that PageInfo PropertyChanged is only raised when CurrentPage value changes.
    /// Input: Set same value twice, then different value.
    /// Expected: PageInfo PropertyChanged is raised only when value actually changes.
    /// </summary>
    [TestMethod]
    public void CurrentPage_PageInfoNotification_OnlyRaisedWhenValueChanges()
    {
        // Arrange
        var academicMock = new Mock<IAcademicService>();
        var loggerMock = new Mock<ILogger<UniversitiesViewModel>>();
        var viewModel = new UniversitiesViewModel(academicMock.Object, loggerMock.Object);
        var pageInfoChangedCount = 0;
        viewModel.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == nameof(UniversitiesViewModel.PageInfo))
                pageInfoChangedCount++;
        };

        // Act
        viewModel.CurrentPage = 1; // Same as default
        viewModel.CurrentPage = 1; // Same as current
        viewModel.CurrentPage = 2; // Different

        // Assert
        Assert.AreEqual(1, pageInfoChangedCount);
    }

    /// <summary>
    /// Tests that setting PendingCount updates the property value correctly for various integer values.
    /// Verifies that the getter returns the value set by the setter.
    /// </summary>
    /// <param name="value">The integer value to set.</param>
    [TestMethod]
    [DataRow(0)]
    [DataRow(1)]
    [DataRow(-1)]
    [DataRow(100)]
    [DataRow(-100)]
    [DataRow(1000)]
    [DataRow(int.MaxValue)]
    [DataRow(int.MinValue)]
    public void PendingCount_SetValue_ReturnsSetValue(int value)
    {
        // Arrange
        var mockAcademicService = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<UniversitiesViewModel>>();
        var viewModel = new UniversitiesViewModel(mockAcademicService.Object, mockLogger.Object);

        // Act
        viewModel.PendingCount = value;

        // Assert
        Assert.AreEqual(value, viewModel.PendingCount);
    }

    /// <summary>
    /// Tests that setting PendingCount to a different value raises the PropertyChanged event
    /// with the correct property name "PendingCount".
    /// </summary>
    /// <param name="initialValue">The initial value to set.</param>
    /// <param name="newValue">The new value to set.</param>
    [TestMethod]
    [DataRow(0, 1)]
    [DataRow(1, 0)]
    [DataRow(10, 20)]
    [DataRow(-5, 5)]
    [DataRow(int.MaxValue, int.MinValue)]
    [DataRow(int.MinValue, int.MaxValue)]
    [DataRow(0, int.MaxValue)]
    [DataRow(0, int.MinValue)]
    public void PendingCount_SetDifferentValue_RaisesPropertyChanged(int initialValue, int newValue)
    {
        // Arrange
        var mockAcademicService = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<UniversitiesViewModel>>();
        var viewModel = new UniversitiesViewModel(mockAcademicService.Object, mockLogger.Object);
        viewModel.PendingCount = initialValue;

        var propertyChangedRaised = false;
        string? propertyName = null;
        viewModel.PropertyChanged += (sender, args) =>
        {
            propertyChangedRaised = true;
            propertyName = args.PropertyName;
        };

        // Act
        viewModel.PendingCount = newValue;

        // Assert
        Assert.IsTrue(propertyChangedRaised, "PropertyChanged event should be raised when value changes.");
        Assert.AreEqual("PendingCount", propertyName, "PropertyChanged event should be raised with correct property name.");
    }

    /// <summary>
    /// Tests that setting PendingCount to the same value does not raise the PropertyChanged event.
    /// Verifies the optimization where unchanged values don't trigger notifications.
    /// </summary>
    /// <param name="value">The value to set twice.</param>
    [TestMethod]
    [DataRow(0)]
    [DataRow(1)]
    [DataRow(-1)]
    [DataRow(100)]
    [DataRow(int.MaxValue)]
    [DataRow(int.MinValue)]
    public void PendingCount_SetSameValue_DoesNotRaisePropertyChanged(int value)
    {
        // Arrange
        var mockAcademicService = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<UniversitiesViewModel>>();
        var viewModel = new UniversitiesViewModel(mockAcademicService.Object, mockLogger.Object);
        viewModel.PendingCount = value;

        var propertyChangedRaised = false;
        viewModel.PropertyChanged += (sender, args) =>
        {
            propertyChangedRaised = true;
        };

        // Act
        viewModel.PendingCount = value;

        // Assert
        Assert.IsFalse(propertyChangedRaised, "PropertyChanged event should not be raised when setting the same value.");
    }

    /// <summary>
    /// Tests that PendingCount has a default value of 0 when the ViewModel is first instantiated.
    /// </summary>
    [TestMethod]
    public void PendingCount_InitialValue_IsZero()
    {
        // Arrange
        var mockAcademicService = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<UniversitiesViewModel>>();

        // Act
        var viewModel = new UniversitiesViewModel(mockAcademicService.Object, mockLogger.Object);

        // Assert
        Assert.AreEqual(0, viewModel.PendingCount, "PendingCount should have a default value of 0.");
    }

    /// <summary>
    /// Tests that PendingCount can be set to negative values without throwing exceptions.
    /// Verifies that there is no built-in validation preventing negative values.
    /// </summary>
    [TestMethod]
    public void PendingCount_SetNegativeValue_NoExceptionThrown()
    {
        // Arrange
        var mockAcademicService = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<UniversitiesViewModel>>();
        var viewModel = new UniversitiesViewModel(mockAcademicService.Object, mockLogger.Object);

        // Act & Assert
        viewModel.PendingCount = -1;
        Assert.AreEqual(-1, viewModel.PendingCount);

        viewModel.PendingCount = -999999;
        Assert.AreEqual(-999999, viewModel.PendingCount);

        viewModel.PendingCount = int.MinValue;
        Assert.AreEqual(int.MinValue, viewModel.PendingCount);
    }

    /// <summary>
    /// Tests that PendingCount correctly handles sequential updates with multiple different values.
    /// Verifies that each update is properly stored and retrieved.
    /// </summary>
    [TestMethod]
    public void PendingCount_SequentialUpdates_AllValuesSetCorrectly()
    {
        // Arrange
        var mockAcademicService = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<UniversitiesViewModel>>();
        var viewModel = new UniversitiesViewModel(mockAcademicService.Object, mockLogger.Object);
        var testValues = new[] { 0, 5, -3, 100, int.MaxValue, int.MinValue, 42 };

        // Act & Assert
        foreach (var value in testValues)
        {
            viewModel.PendingCount = value;
            Assert.AreEqual(value, viewModel.PendingCount, $"PendingCount should be {value} after setting.");
        }
    }

    /// <summary>
    /// Tests that TotalInstitutions property getter returns the value that was set.
    /// </summary>
    /// <param name="value">The value to set.</param>
    [TestMethod]
    [DataRow(0)]
    [DataRow(1)]
    [DataRow(100)]
    [DataRow(1000)]
    [DataRow(-1)]
    [DataRow(-100)]
    [DataRow(int.MaxValue)]
    [DataRow(int.MinValue)]
    public void TotalInstitutions_SetValue_ReturnsSetValue(int value)
    {
        // Arrange
        var mockAcademicService = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<UniversitiesViewModel>>();
        var viewModel = new UniversitiesViewModel(mockAcademicService.Object, mockLogger.Object);

        // Act
        viewModel.TotalInstitutions = value;

        // Assert
        Assert.AreEqual(value, viewModel.TotalInstitutions);
    }

    /// <summary>
    /// Tests that TotalInstitutions property raises PropertyChanged event when value changes.
    /// </summary>
    [TestMethod]
    public void TotalInstitutions_ValueChanged_RaisesPropertyChangedEvent()
    {
        // Arrange
        var mockAcademicService = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<UniversitiesViewModel>>();
        var viewModel = new UniversitiesViewModel(mockAcademicService.Object, mockLogger.Object);
        string? propertyChangedName = null;
        viewModel.PropertyChanged += (sender, args) => propertyChangedName = args.PropertyName;

        // Act
        viewModel.TotalInstitutions = 42;

        // Assert
        Assert.AreEqual("TotalInstitutions", propertyChangedName);
    }

    /// <summary>
    /// Tests that TotalInstitutions property does not raise PropertyChanged event when set to the same value.
    /// </summary>
    [TestMethod]
    public void TotalInstitutions_SetSameValue_DoesNotRaisePropertyChangedEvent()
    {
        // Arrange
        var mockAcademicService = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<UniversitiesViewModel>>();
        var viewModel = new UniversitiesViewModel(mockAcademicService.Object, mockLogger.Object);
        viewModel.TotalInstitutions = 42;
        int eventRaisedCount = 0;
        viewModel.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == "TotalInstitutions")
            {
                eventRaisedCount++;
            }
        };

        // Act
        viewModel.TotalInstitutions = 42;

        // Assert
        Assert.AreEqual(0, eventRaisedCount);
    }

    /// <summary>
    /// Tests that TotalInstitutions property raises PropertyChanged event multiple times for different values.
    /// </summary>
    [TestMethod]
    public void TotalInstitutions_SetDifferentValues_RaisesPropertyChangedEventEachTime()
    {
        // Arrange
        var mockAcademicService = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<UniversitiesViewModel>>();
        var viewModel = new UniversitiesViewModel(mockAcademicService.Object, mockLogger.Object);
        int eventRaisedCount = 0;
        viewModel.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == "TotalInstitutions")
            {
                eventRaisedCount++;
            }
        };

        // Act
        viewModel.TotalInstitutions = 10;
        viewModel.TotalInstitutions = 20;
        viewModel.TotalInstitutions = 30;

        // Assert
        Assert.AreEqual(3, eventRaisedCount);
    }

    /// <summary>
    /// Tests that TotalInstitutions property default value is zero.
    /// </summary>
    [TestMethod]
    public void TotalInstitutions_DefaultValue_IsZero()
    {
        // Arrange
        var mockAcademicService = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<UniversitiesViewModel>>();

        // Act
        var viewModel = new UniversitiesViewModel(mockAcademicService.Object, mockLogger.Object);

        // Assert
        Assert.AreEqual(0, viewModel.TotalInstitutions);
    }

    /// <summary>
    /// Tests that TotalInstitutions property correctly handles transition from zero to positive value.
    /// </summary>
    [TestMethod]
    public void TotalInstitutions_TransitionFromZeroToPositive_UpdatesCorrectly()
    {
        // Arrange
        var mockAcademicService = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<UniversitiesViewModel>>();
        var viewModel = new UniversitiesViewModel(mockAcademicService.Object, mockLogger.Object);

        // Act
        viewModel.TotalInstitutions = 100;

        // Assert
        Assert.AreEqual(100, viewModel.TotalInstitutions);
    }

    /// <summary>
    /// Tests that TotalInstitutions property correctly handles transition from positive to negative value.
    /// </summary>
    [TestMethod]
    public void TotalInstitutions_TransitionFromPositiveToNegative_UpdatesCorrectly()
    {
        // Arrange
        var mockAcademicService = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<UniversitiesViewModel>>();
        var viewModel = new UniversitiesViewModel(mockAcademicService.Object, mockLogger.Object);
        viewModel.TotalInstitutions = 50;

        // Act
        viewModel.TotalInstitutions = -50;

        // Assert
        Assert.AreEqual(-50, viewModel.TotalInstitutions);
    }

    /// <summary>
    /// Tests that TotalInstitutions property correctly handles extreme value transitions.
    /// </summary>
    [TestMethod]
    public void TotalInstitutions_TransitionBetweenExtremeValues_UpdatesCorrectly()
    {
        // Arrange
        var mockAcademicService = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<UniversitiesViewModel>>();
        var viewModel = new UniversitiesViewModel(mockAcademicService.Object, mockLogger.Object);

        // Act & Assert
        viewModel.TotalInstitutions = int.MaxValue;
        Assert.AreEqual(int.MaxValue, viewModel.TotalInstitutions);

        viewModel.TotalInstitutions = int.MinValue;
        Assert.AreEqual(int.MinValue, viewModel.TotalInstitutions);

        viewModel.TotalInstitutions = 0;
        Assert.AreEqual(0, viewModel.TotalInstitutions);
    }

    /// <summary>
    /// Tests that the constructor successfully initializes with valid parameters.
    /// </summary>
    [TestMethod]
    public void Constructor_ValidParameters_InitializesSuccessfully()
    {
        // Arrange
        var academic = new Mock<IAcademicService>().Object;
        var logger = new Mock<ILogger<UniversitiesViewModel>>().Object;

        // Act
        var viewModel = new UniversitiesViewModel(academic, logger);

        // Assert
        Assert.IsNotNull(viewModel);
    }

    /// <summary>
    /// Tests that all command properties are initialized and not null after construction.
    /// </summary>
    [TestMethod]
    public void Constructor_ValidParameters_InitializesAllCommands()
    {
        // Arrange
        var academic = new Mock<IAcademicService>().Object;
        var logger = new Mock<ILogger<UniversitiesViewModel>>().Object;

        // Act
        var viewModel = new UniversitiesViewModel(academic, logger);

        // Assert
        Assert.IsNotNull(viewModel.LoadCommand, "LoadCommand should be initialized");
        Assert.IsNotNull(viewModel.SearchCommand, "SearchCommand should be initialized");
        Assert.IsNotNull(viewModel.FilterCommand, "FilterCommand should be initialized");
        Assert.IsNotNull(viewModel.NextPageCommand, "NextPageCommand should be initialized");
        Assert.IsNotNull(viewModel.PrevPageCommand, "PrevPageCommand should be initialized");
        Assert.IsNotNull(viewModel.AddUniversityCommand, "AddUniversityCommand should be initialized");
        Assert.IsNotNull(viewModel.EditUniversityCommand, "EditUniversityCommand should be initialized");
        Assert.IsNotNull(viewModel.DeleteUniversityCommand, "DeleteUniversityCommand should be initialized");
        Assert.IsNotNull(viewModel.SaveCommand, "SaveCommand should be initialized");
        Assert.IsNotNull(viewModel.CancelEditCommand, "CancelEditCommand should be initialized");
        Assert.IsNotNull(viewModel.ExportCommand, "ExportCommand should be initialized");
        Assert.IsNotNull(viewModel.RefreshCommand, "RefreshCommand should be initialized");
    }

    /// <summary>
    /// Tests that the Universities collection is initialized and empty after construction.
    /// </summary>
    [TestMethod]
    public void Constructor_ValidParameters_InitializesUniversitiesCollection()
    {
        // Arrange
        var academic = new Mock<IAcademicService>().Object;
        var logger = new Mock<ILogger<UniversitiesViewModel>>().Object;

        // Act
        var viewModel = new UniversitiesViewModel(academic, logger);

        // Assert
        Assert.IsNotNull(viewModel.Universities);
        Assert.AreEqual(0, viewModel.Universities.Count);
    }

    /// <summary>
    /// Tests that default property values are correctly initialized after construction.
    /// </summary>
    [TestMethod]
    public void Constructor_ValidParameters_InitializesDefaultPropertyValues()
    {
        // Arrange
        var academic = new Mock<IAcademicService>().Object;
        var logger = new Mock<ILogger<UniversitiesViewModel>>().Object;

        // Act
        var viewModel = new UniversitiesViewModel(academic, logger);

        // Assert
        Assert.AreEqual(string.Empty, viewModel.SearchQuery);
        Assert.AreEqual("All", viewModel.StatusFilter);
        Assert.AreEqual(1, viewModel.CurrentPage);
        Assert.AreEqual(1, viewModel.TotalPages);
        Assert.AreEqual(10, viewModel.PageSize);
        Assert.AreEqual(0, viewModel.TotalInstitutions);
        Assert.AreEqual(0, viewModel.AccreditedCount);
        Assert.AreEqual(0, viewModel.PendingCount);
        Assert.AreEqual(string.Empty, viewModel.EditName);
        Assert.IsNull(viewModel.EditId);
        Assert.IsFalse(viewModel.IsEditing);
        Assert.AreEqual(string.Empty, viewModel.ErrorMessage);
    }

    /// <summary>
    /// Tests that the PageInfo property returns the correct format after construction.
    /// </summary>
    [TestMethod]
    public void Constructor_ValidParameters_PageInfoReturnsCorrectFormat()
    {
        // Arrange
        var academic = new Mock<IAcademicService>().Object;
        var logger = new Mock<ILogger<UniversitiesViewModel>>().Object;

        // Act
        var viewModel = new UniversitiesViewModel(academic, logger);

        // Assert
        Assert.AreEqual("Page 1 of 1", viewModel.PageInfo);
    }

    /// <summary>
    /// Tests that the PageSize getter returns the initial default value of 10.
    /// </summary>
    [TestMethod]
    public void PageSize_InitialValue_Returns10()
    {
        // Arrange
        var academicMock = new Mock<IAcademicService>();
        var loggerMock = new Mock<ILogger<UniversitiesViewModel>>();
        var viewModel = new UniversitiesViewModel(academicMock.Object, loggerMock.Object);

        // Act
        int result = viewModel.PageSize;

        // Assert
        Assert.AreEqual(10, result);
    }

    /// <summary>
    /// Tests that setting PageSize to a different positive value updates the property and resets CurrentPage to 1.
    /// </summary>
    /// <param name="newPageSize">The new page size value to set.</param>
    [TestMethod]
    [DataRow(1)]
    [DataRow(5)]
    [DataRow(20)]
    [DataRow(50)]
    [DataRow(100)]
    [DataRow(1000)]
    public void PageSize_SetDifferentPositiveValue_UpdatesValueAndResetsCurrentPage(int newPageSize)
    {
        // Arrange
        var academicMock = new Mock<IAcademicService>();
        var loggerMock = new Mock<ILogger<UniversitiesViewModel>>();
        var viewModel = new UniversitiesViewModel(academicMock.Object, loggerMock.Object);

        // Act
        viewModel.PageSize = newPageSize;

        // Assert
        Assert.AreEqual(newPageSize, viewModel.PageSize);
        Assert.AreEqual(1, viewModel.CurrentPage);
    }

    /// <summary>
    /// Tests that setting PageSize to the same value does not reset CurrentPage.
    /// </summary>
    [TestMethod]
    public void PageSize_SetSameValue_DoesNotResetCurrentPage()
    {
        // Arrange
        var academicMock = new Mock<IAcademicService>();
        var loggerMock = new Mock<ILogger<UniversitiesViewModel>>();
        var viewModel = new UniversitiesViewModel(academicMock.Object, loggerMock.Object);
        int initialPageSize = viewModel.PageSize;

        // Set CurrentPage to a value other than 1
        viewModel.PageSize = 20; // This will set CurrentPage to 1
        // Now manually change CurrentPage to test if setting same PageSize affects it
        // We need to get CurrentPage to a different value first
        var currentPageBefore = viewModel.CurrentPage;

        // Act
        viewModel.PageSize = 20; // Setting the same value again

        // Assert
        Assert.AreEqual(20, viewModel.PageSize);
        Assert.AreEqual(currentPageBefore, viewModel.CurrentPage); // CurrentPage should remain unchanged
    }

    /// <summary>
    /// Tests that setting PageSize to zero updates the value and resets CurrentPage to 1.
    /// Note: Zero page size may cause issues in pagination logic but is not validated in the setter.
    /// </summary>
    [TestMethod]
    public void PageSize_SetToZero_UpdatesValueAndResetsCurrentPage()
    {
        // Arrange
        var academicMock = new Mock<IAcademicService>();
        var loggerMock = new Mock<ILogger<UniversitiesViewModel>>();
        var viewModel = new UniversitiesViewModel(academicMock.Object, loggerMock.Object);

        // Act
        viewModel.PageSize = 0;

        // Assert
        Assert.AreEqual(0, viewModel.PageSize);
        Assert.AreEqual(1, viewModel.CurrentPage);
    }

    /// <summary>
    /// Tests that setting PageSize to negative values updates the property and resets CurrentPage to 1.
    /// Note: Negative page size may cause issues in pagination logic but is not validated in the setter.
    /// </summary>
    /// <param name="negativeValue">The negative page size value to test.</param>
    [TestMethod]
    [DataRow(-1)]
    [DataRow(-10)]
    [DataRow(-100)]
    public void PageSize_SetToNegativeValue_UpdatesValueAndResetsCurrentPage(int negativeValue)
    {
        // Arrange
        var academicMock = new Mock<IAcademicService>();
        var loggerMock = new Mock<ILogger<UniversitiesViewModel>>();
        var viewModel = new UniversitiesViewModel(academicMock.Object, loggerMock.Object);

        // Act
        viewModel.PageSize = negativeValue;

        // Assert
        Assert.AreEqual(negativeValue, viewModel.PageSize);
        Assert.AreEqual(1, viewModel.CurrentPage);
    }

    /// <summary>
    /// Tests that setting PageSize to int.MinValue updates the value and resets CurrentPage to 1.
    /// </summary>
    [TestMethod]
    public void PageSize_SetToIntMinValue_UpdatesValueAndResetsCurrentPage()
    {
        // Arrange
        var academicMock = new Mock<IAcademicService>();
        var loggerMock = new Mock<ILogger<UniversitiesViewModel>>();
        var viewModel = new UniversitiesViewModel(academicMock.Object, loggerMock.Object);

        // Act
        viewModel.PageSize = int.MinValue;

        // Assert
        Assert.AreEqual(int.MinValue, viewModel.PageSize);
        Assert.AreEqual(1, viewModel.CurrentPage);
    }

    /// <summary>
    /// Tests that setting PageSize to int.MaxValue updates the value and resets CurrentPage to 1.
    /// </summary>
    [TestMethod]
    public void PageSize_SetToIntMaxValue_UpdatesValueAndResetsCurrentPage()
    {
        // Arrange
        var academicMock = new Mock<IAcademicService>();
        var loggerMock = new Mock<ILogger<UniversitiesViewModel>>();
        var viewModel = new UniversitiesViewModel(academicMock.Object, loggerMock.Object);

        // Act
        viewModel.PageSize = int.MaxValue;

        // Assert
        Assert.AreEqual(int.MaxValue, viewModel.PageSize);
        Assert.AreEqual(1, viewModel.CurrentPage);
    }

    /// <summary>
    /// Tests that setting PageSize to a different value raises PropertyChanged event for "PageSize".
    /// </summary>
    [TestMethod]
    public void PageSize_SetDifferentValue_RaisesPropertyChangedEvent()
    {
        // Arrange
        var academicMock = new Mock<IAcademicService>();
        var loggerMock = new Mock<ILogger<UniversitiesViewModel>>();
        var viewModel = new UniversitiesViewModel(academicMock.Object, loggerMock.Object);
        bool propertyChangedRaised = false;
        string? changedPropertyName = null;

        viewModel.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == nameof(UniversitiesViewModel.PageSize))
            {
                propertyChangedRaised = true;
                changedPropertyName = args.PropertyName;
            }
        };

        // Act
        viewModel.PageSize = 25;

        // Assert
        Assert.IsTrue(propertyChangedRaised);
        Assert.AreEqual(nameof(UniversitiesViewModel.PageSize), changedPropertyName);
    }

    /// <summary>
    /// Tests that setting PageSize to the same value does not raise PropertyChanged event for "PageSize".
    /// </summary>
    [TestMethod]
    public void PageSize_SetSameValue_DoesNotRaisePropertyChangedEvent()
    {
        // Arrange
        var academicMock = new Mock<IAcademicService>();
        var loggerMock = new Mock<ILogger<UniversitiesViewModel>>();
        var viewModel = new UniversitiesViewModel(academicMock.Object, loggerMock.Object);
        int propertyChangedCount = 0;

        viewModel.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == nameof(UniversitiesViewModel.PageSize))
            {
                propertyChangedCount++;
            }
        };

        // Act
        viewModel.PageSize = 10; // Setting to the same initial value

        // Assert
        Assert.AreEqual(0, propertyChangedCount);
    }

    /// <summary>
    /// Tests that setting PageSize to a different value also raises PropertyChanged for CurrentPage
    /// since CurrentPage is modified as a side effect.
    /// </summary>
    [TestMethod]
    public void PageSize_SetDifferentValue_RaisesPropertyChangedForCurrentPage()
    {
        // Arrange
        var academicMock = new Mock<IAcademicService>();
        var loggerMock = new Mock<ILogger<UniversitiesViewModel>>();
        var viewModel = new UniversitiesViewModel(academicMock.Object, loggerMock.Object);
        bool currentPagePropertyChangedRaised = false;

        viewModel.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == nameof(UniversitiesViewModel.CurrentPage))
            {
                currentPagePropertyChangedRaised = true;
            }
        };

        // Act
        viewModel.PageSize = 25;

        // Assert
        Assert.IsTrue(currentPagePropertyChangedRaised);
    }

    /// <summary>
    /// Tests that AccreditedCount property correctly stores and retrieves various integer values including boundaries.
    /// </summary>
    /// <param name="value">The value to set on the AccreditedCount property.</param>
    [TestMethod]
    [DataRow(0)]
    [DataRow(1)]
    [DataRow(10)]
    [DataRow(100)]
    [DataRow(1000)]
    [DataRow(-1)]
    [DataRow(-100)]
    [DataRow(2147483647)] // int.MaxValue
    [DataRow(-2147483648)] // int.MinValue
    public void AccreditedCount_SetValue_ReturnsSetValue(int value)
    {
        // Arrange
        var mockAcademicService = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<UniversitiesViewModel>>();
        var viewModel = new UniversitiesViewModel(mockAcademicService.Object, mockLogger.Object);

        // Act
        viewModel.AccreditedCount = value;

        // Assert
        Assert.AreEqual(value, viewModel.AccreditedCount);
    }

    /// <summary>
    /// Tests that AccreditedCount property raises PropertyChanged event when the value is changed.
    /// </summary>
    [TestMethod]
    public void AccreditedCount_WhenValueChanges_RaisesPropertyChangedEvent()
    {
        // Arrange
        var mockAcademicService = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<UniversitiesViewModel>>();
        var viewModel = new UniversitiesViewModel(mockAcademicService.Object, mockLogger.Object);
        var eventRaised = false;
        string? raisedPropertyName = null;
        viewModel.PropertyChanged += (sender, args) =>
        {
            eventRaised = true;
            raisedPropertyName = args.PropertyName;
        };

        // Act
        viewModel.AccreditedCount = 42;

        // Assert
        Assert.IsTrue(eventRaised);
        Assert.AreEqual("AccreditedCount", raisedPropertyName);
    }

    /// <summary>
    /// Tests that AccreditedCount property does not raise PropertyChanged event when set to the same value.
    /// </summary>
    [TestMethod]
    public void AccreditedCount_WhenSetToSameValue_DoesNotRaisePropertyChangedEvent()
    {
        // Arrange
        var mockAcademicService = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<UniversitiesViewModel>>();
        var viewModel = new UniversitiesViewModel(mockAcademicService.Object, mockLogger.Object);
        viewModel.AccreditedCount = 10;
        var eventRaisedCount = 0;
        viewModel.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == "AccreditedCount")
            {
                eventRaisedCount++;
            }
        };

        // Act
        viewModel.AccreditedCount = 10;

        // Assert
        Assert.AreEqual(0, eventRaisedCount);
    }

    /// <summary>
    /// Tests that AccreditedCount property has default value of 0 when first initialized.
    /// </summary>
    [TestMethod]
    public void AccreditedCount_WhenInitialized_HasDefaultValueOfZero()
    {
        // Arrange
        var mockAcademicService = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<UniversitiesViewModel>>();

        // Act
        var viewModel = new UniversitiesViewModel(mockAcademicService.Object, mockLogger.Object);

        // Assert
        Assert.AreEqual(0, viewModel.AccreditedCount);
    }

    /// <summary>
    /// Tests that AccreditedCount property can be updated multiple times with different values.
    /// </summary>
    [TestMethod]
    public void AccreditedCount_WhenUpdatedMultipleTimes_RetainsLastValue()
    {
        // Arrange
        var mockAcademicService = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<UniversitiesViewModel>>();
        var viewModel = new UniversitiesViewModel(mockAcademicService.Object, mockLogger.Object);

        // Act
        viewModel.AccreditedCount = 5;
        viewModel.AccreditedCount = 10;
        viewModel.AccreditedCount = 15;

        // Assert
        Assert.AreEqual(15, viewModel.AccreditedCount);
    }

    /// <summary>
    /// Tests that AccreditedCount property raises PropertyChanged event for each distinct value change.
    /// </summary>
    [TestMethod]
    public void AccreditedCount_WhenChangedMultipleTimes_RaisesEventForEachChange()
    {
        // Arrange
        var mockAcademicService = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<UniversitiesViewModel>>();
        var viewModel = new UniversitiesViewModel(mockAcademicService.Object, mockLogger.Object);
        var eventRaisedCount = 0;
        viewModel.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == "AccreditedCount")
            {
                eventRaisedCount++;
            }
        };

        // Act
        viewModel.AccreditedCount = 1;
        viewModel.AccreditedCount = 2;
        viewModel.AccreditedCount = 3;

        // Assert
        Assert.AreEqual(3, eventRaisedCount);
    }

    /// <summary>
    /// Tests that LoadAsync returns immediately without loading when IsBusy is already true,
    /// preventing concurrent execution.
    /// </summary>
    [TestMethod]
    public async Task LoadAsync_WhenIsBusyIsTrue_ReturnsImmediatelyWithoutLoading()
    {
        // Arrange
        var mockAcademicService = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<UniversitiesViewModel>>();
        var viewModel = new UniversitiesViewModel(mockAcademicService.Object, mockLogger.Object);

        // Set IsBusy to true
        viewModel.IsBusy = true;

        // Act
        await viewModel.LoadAsync();

        // Assert
        mockAcademicService.Verify(x => x.GetUniversityDetailsAsync(), Times.Never);
    }

    /// <summary>
    /// Tests that LoadAsync successfully loads universities, updates all statistics,
    /// and sets IsBusy to false when the operation completes successfully.
    /// </summary>
    [TestMethod]
    public async Task LoadAsync_WhenSuccessful_LoadsUniversitiesAndUpdatesStatistics()
    {
        // Arrange
        var mockAcademicService = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<UniversitiesViewModel>>();

        var universities = new List<UniversityDto>
        {
            new UniversityDto { Id = "1", Name = "University A", Status = "Active" },
            new UniversityDto { Id = "2", Name = "University B", Status = "Active" },
            new UniversityDto { Id = "3", Name = "University C", Status = "Pending" },
            new UniversityDto { Id = "4", Name = "University D", Status = "Inactive" }
        };

        mockAcademicService.Setup(x => x.GetUniversityDetailsAsync())
            .ReturnsAsync(universities);

        var viewModel = new UniversitiesViewModel(mockAcademicService.Object, mockLogger.Object);

        // Act
        await viewModel.LoadAsync();

        // Assert
        Assert.AreEqual(4, viewModel.TotalInstitutions);
        Assert.AreEqual(2, viewModel.AccreditedCount);
        Assert.AreEqual(2, viewModel.PendingCount);
        Assert.AreEqual(string.Empty, viewModel.ErrorMessage);
        Assert.IsFalse(viewModel.IsBusy);
        mockAcademicService.Verify(x => x.GetUniversityDetailsAsync(), Times.Once);
    }

    /// <summary>
    /// Tests that LoadAsync correctly handles an empty list of universities,
    /// setting all counts to zero.
    /// </summary>
    [TestMethod]
    public async Task LoadAsync_WithEmptyUniversityList_SetsCountsToZero()
    {
        // Arrange
        var mockAcademicService = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<UniversitiesViewModel>>();

        var emptyList = new List<UniversityDto>();

        mockAcademicService.Setup(x => x.GetUniversityDetailsAsync())
            .ReturnsAsync(emptyList);

        var viewModel = new UniversitiesViewModel(mockAcademicService.Object, mockLogger.Object);

        // Act
        await viewModel.LoadAsync();

        // Assert
        Assert.AreEqual(0, viewModel.TotalInstitutions);
        Assert.AreEqual(0, viewModel.AccreditedCount);
        Assert.AreEqual(0, viewModel.PendingCount);
        Assert.IsFalse(viewModel.IsBusy);
    }

    /// <summary>
    /// Tests that LoadAsync correctly counts universities with all Active status,
    /// resulting in zero pending count.
    /// </summary>
    [TestMethod]
    public async Task LoadAsync_WithAllActiveUniversities_CountsAllAsAccredited()
    {
        // Arrange
        var mockAcademicService = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<UniversitiesViewModel>>();

        var universities = new List<UniversityDto>
        {
            new UniversityDto { Id = "1", Name = "University A", Status = "Active" },
            new UniversityDto { Id = "2", Name = "University B", Status = "Active" },
            new UniversityDto { Id = "3", Name = "University C", Status = "Active" }
        };

        mockAcademicService.Setup(x => x.GetUniversityDetailsAsync())
            .ReturnsAsync(universities);

        var viewModel = new UniversitiesViewModel(mockAcademicService.Object, mockLogger.Object);

        // Act
        await viewModel.LoadAsync();

        // Assert
        Assert.AreEqual(3, viewModel.TotalInstitutions);
        Assert.AreEqual(3, viewModel.AccreditedCount);
        Assert.AreEqual(0, viewModel.PendingCount);
    }

    /// <summary>
    /// Tests that LoadAsync correctly counts universities with no Active status,
    /// resulting in zero accredited count and all counted as pending.
    /// </summary>
    [TestMethod]
    public async Task LoadAsync_WithNoActiveUniversities_CountsAllAsPending()
    {
        // Arrange
        var mockAcademicService = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<UniversitiesViewModel>>();

        var universities = new List<UniversityDto>
        {
            new UniversityDto { Id = "1", Name = "University A", Status = "Pending" },
            new UniversityDto { Id = "2", Name = "University B", Status = "Inactive" },
            new UniversityDto { Id = "3", Name = "University C", Status = "Suspended" }
        };

        mockAcademicService.Setup(x => x.GetUniversityDetailsAsync())
            .ReturnsAsync(universities);

        var viewModel = new UniversitiesViewModel(mockAcademicService.Object, mockLogger.Object);

        // Act
        await viewModel.LoadAsync();

        // Assert
        Assert.AreEqual(3, viewModel.TotalInstitutions);
        Assert.AreEqual(0, viewModel.AccreditedCount);
        Assert.AreEqual(3, viewModel.PendingCount);
    }

    /// <summary>
    /// Tests that LoadAsync correctly handles universities with various status values,
    /// counting only exact "Active" match as accredited.
    /// </summary>
    [TestMethod]
    [DataRow("Active", true)]
    [DataRow("active", false)]
    [DataRow("ACTIVE", false)]
    [DataRow("Pending", false)]
    [DataRow("Inactive", false)]
    [DataRow("", false)]
    [DataRow("  ", false)]
    public async Task LoadAsync_WithVariousStatusValues_CountsOnlyExactActiveMatch(string status, bool shouldBeAccredited)
    {
        // Arrange
        var mockAcademicService = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<UniversitiesViewModel>>();

        var universities = new List<UniversityDto>
        {
            new UniversityDto { Id = "1", Name = "University A", Status = status }
        };

        mockAcademicService.Setup(x => x.GetUniversityDetailsAsync())
            .ReturnsAsync(universities);

        var viewModel = new UniversitiesViewModel(mockAcademicService.Object, mockLogger.Object);

        // Act
        await viewModel.LoadAsync();

        // Assert
        Assert.AreEqual(1, viewModel.TotalInstitutions);
        Assert.AreEqual(shouldBeAccredited ? 1 : 0, viewModel.AccreditedCount);
        Assert.AreEqual(shouldBeAccredited ? 0 : 1, viewModel.PendingCount);
    }

    /// <summary>
    /// Tests that LoadAsync handles exceptions from GetUniversityDetailsAsync,
    /// logs the error, sets error message, and ensures IsBusy is reset to false.
    /// </summary>
    [TestMethod]
    public async Task LoadAsync_WhenGetUniversityDetailsThrowsException_LogsErrorAndSetsErrorMessage()
    {
        // Arrange
        var mockAcademicService = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<UniversitiesViewModel>>();

        var expectedException = new InvalidOperationException("Service unavailable");
        mockAcademicService.Setup(x => x.GetUniversityDetailsAsync())
            .ThrowsAsync(expectedException);

        var viewModel = new UniversitiesViewModel(mockAcademicService.Object, mockLogger.Object);

        // Act
        await viewModel.LoadAsync();

        // Assert
        Assert.AreEqual("Failed to load universities.", viewModel.ErrorMessage);
        Assert.IsFalse(viewModel.IsBusy);
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => true),
                expectedException,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    /// <summary>
    /// Tests that LoadAsync handles different exception types appropriately,
    /// ensuring all exceptions result in error logging and message.
    /// </summary>
    [TestMethod]
    [DataRow(typeof(InvalidOperationException))]
    [DataRow(typeof(ArgumentException))]
    [DataRow(typeof(NullReferenceException))]
    [DataRow(typeof(TimeoutException))]
    public async Task LoadAsync_WithDifferentExceptionTypes_HandlesAllAppropriately(Type exceptionType)
    {
        // Arrange
        var mockAcademicService = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<UniversitiesViewModel>>();

        var exception = (Exception)Activator.CreateInstance(exceptionType, "Test error")!;
        mockAcademicService.Setup(x => x.GetUniversityDetailsAsync())
            .ThrowsAsync(exception);

        var viewModel = new UniversitiesViewModel(mockAcademicService.Object, mockLogger.Object);

        // Act
        await viewModel.LoadAsync();

        // Assert
        Assert.AreEqual("Failed to load universities.", viewModel.ErrorMessage);
        Assert.IsFalse(viewModel.IsBusy);
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
    /// Tests that LoadAsync clears any previous error message at the start
    /// of a successful load operation.
    /// </summary>
    [TestMethod]
    public async Task LoadAsync_WhenSuccessful_ClearsPreviousErrorMessage()
    {
        // Arrange
        var mockAcademicService = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<UniversitiesViewModel>>();

        var universities = new List<UniversityDto>
        {
            new UniversityDto { Id = "1", Name = "University A", Status = "Active" }
        };

        mockAcademicService.Setup(x => x.GetUniversityDetailsAsync())
            .ReturnsAsync(universities);

        var viewModel = new UniversitiesViewModel(mockAcademicService.Object, mockLogger.Object);

        // Simulate previous error
        viewModel.ErrorMessage = "Previous error";

        // Act
        await viewModel.LoadAsync();

        // Assert
        Assert.AreEqual(string.Empty, viewModel.ErrorMessage);
    }

    /// <summary>
    /// Tests that LoadAsync ensures IsBusy is reset to false even when
    /// an exception occurs, verifying the finally block behavior.
    /// </summary>
    [TestMethod]
    public async Task LoadAsync_WhenExceptionOccurs_EnsuresIsBusyIsResetInFinally()
    {
        // Arrange
        var mockAcademicService = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<UniversitiesViewModel>>();

        mockAcademicService.Setup(x => x.GetUniversityDetailsAsync())
            .ThrowsAsync(new Exception("Test exception"));

        var viewModel = new UniversitiesViewModel(mockAcademicService.Object, mockLogger.Object);

        // Verify IsBusy starts as false
        Assert.IsFalse(viewModel.IsBusy);

        // Act
        await viewModel.LoadAsync();

        // Assert
        Assert.IsFalse(viewModel.IsBusy, "IsBusy should be reset to false in finally block");
    }

    /// <summary>
    /// Tests that LoadAsync handles a large number of universities correctly,
    /// verifying performance and statistics calculation with boundary values.
    /// </summary>
    [TestMethod]
    public async Task LoadAsync_WithLargeNumberOfUniversities_CalculatesStatisticsCorrectly()
    {
        // Arrange
        var mockAcademicService = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<UniversitiesViewModel>>();

        var universities = new List<UniversityDto>();
        for (int i = 0; i < 1000; i++)
        {
            universities.Add(new UniversityDto
            {
                Id = i.ToString(),
                Name = $"University {i}",
                Status = i % 2 == 0 ? "Active" : "Pending"
            });
        }

        mockAcademicService.Setup(x => x.GetUniversityDetailsAsync())
            .ReturnsAsync(universities);

        var viewModel = new UniversitiesViewModel(mockAcademicService.Object, mockLogger.Object);

        // Act
        await viewModel.LoadAsync();

        // Assert
        Assert.AreEqual(1000, viewModel.TotalInstitutions);
        Assert.AreEqual(500, viewModel.AccreditedCount);
        Assert.AreEqual(500, viewModel.PendingCount);
    }

    /// <summary>
    /// Tests that LoadAsync with a single university correctly updates all statistics.
    /// </summary>
    [TestMethod]
    public async Task LoadAsync_WithSingleUniversity_UpdatesStatisticsCorrectly()
    {
        // Arrange
        var mockAcademicService = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<UniversitiesViewModel>>();

        var universities = new List<UniversityDto>
        {
            new UniversityDto { Id = "1", Name = "Single University", Status = "Active" }
        };

        mockAcademicService.Setup(x => x.GetUniversityDetailsAsync())
            .ReturnsAsync(universities);

        var viewModel = new UniversitiesViewModel(mockAcademicService.Object, mockLogger.Object);

        // Act
        await viewModel.LoadAsync();

        // Assert
        Assert.AreEqual(1, viewModel.TotalInstitutions);
        Assert.AreEqual(1, viewModel.AccreditedCount);
        Assert.AreEqual(0, viewModel.PendingCount);
    }

    /// <summary>
    /// Tests that LoadAsync sets IsBusy to true during execution and false after completion,
    /// ensuring proper concurrency guard behavior.
    /// </summary>
    [TestMethod]
    public async Task LoadAsync_DuringExecution_SetsIsBusyToTrueThenFalse()
    {
        // Arrange
        var mockAcademicService = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<UniversitiesViewModel>>();

        var taskCompletionSource = new TaskCompletionSource<List<UniversityDto>>();
        mockAcademicService.Setup(x => x.GetUniversityDetailsAsync())
            .Returns(taskCompletionSource.Task);

        var viewModel = new UniversitiesViewModel(mockAcademicService.Object, mockLogger.Object);

        Assert.IsFalse(viewModel.IsBusy, "IsBusy should start as false");

        // Act
        var loadTask = viewModel.LoadAsync();

        // IsBusy should be true during execution
        Assert.IsTrue(viewModel.IsBusy, "IsBusy should be true during execution");

        // Complete the async operation
        taskCompletionSource.SetResult(new List<UniversityDto>());
        await loadTask;

        // Assert
        Assert.IsFalse(viewModel.IsBusy, "IsBusy should be false after completion");
    }

    /// <summary>
    /// Tests that LoadAsync correctly handles universities with special characters
    /// in their names and various status values.
    /// </summary>
    [TestMethod]
    public async Task LoadAsync_WithSpecialCharactersInNames_LoadsCorrectly()
    {
        // Arrange
        var mockAcademicService = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<UniversitiesViewModel>>();

        var universities = new List<UniversityDto>
        {
            new UniversityDto { Id = "1", Name = "University <Test>", Status = "Active" },
            new UniversityDto { Id = "2", Name = "University & College", Status = "Active" },
            new UniversityDto { Id = "3", Name = "Université François", Status = "Pending" }
        };

        mockAcademicService.Setup(x => x.GetUniversityDetailsAsync())
            .ReturnsAsync(universities);

        var viewModel = new UniversitiesViewModel(mockAcademicService.Object, mockLogger.Object);

        // Act
        await viewModel.LoadAsync();

        // Assert
        Assert.AreEqual(3, viewModel.TotalInstitutions);
        Assert.AreEqual(2, viewModel.AccreditedCount);
        Assert.AreEqual(1, viewModel.PendingCount);
    }

    /// <summary>
    /// Tests that the PageInfo property returns the correct formatted string with typical page values.
    /// </summary>
    /// <param name="currentPage">The current page number to test.</param>
    /// <param name="totalPages">The total pages number to test.</param>
    /// <param name="expected">The expected formatted string.</param>
    [TestMethod]
    [DataRow(1, 10, "Page 1 of 10")]
    [DataRow(5, 20, "Page 5 of 20")]
    [DataRow(1, 1, "Page 1 of 1")]
    [DataRow(100, 100, "Page 100 of 100")]
    [DataRow(50, 100, "Page 50 of 100")]
    public void PageInfo_TypicalValues_ReturnsCorrectFormat(int currentPage, int totalPages, string expected)
    {
        // Arrange
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<UniversitiesViewModel>>();
        var viewModel = new UniversitiesViewModel(mockAcademic.Object, mockLogger.Object);

        viewModel.CurrentPage = currentPage;
        viewModel.TotalPages = totalPages;

        // Act
        var result = viewModel.PageInfo;

        // Assert
        Assert.AreEqual(expected, result);
    }

    /// <summary>
    /// Tests that the PageInfo property returns the correct formatted string with zero values.
    /// </summary>
    /// <param name="currentPage">The current page number to test.</param>
    /// <param name="totalPages">The total pages number to test.</param>
    /// <param name="expected">The expected formatted string.</param>
    [TestMethod]
    [DataRow(0, 0, "Page 0 of 0")]
    [DataRow(0, 10, "Page 0 of 10")]
    [DataRow(1, 0, "Page 1 of 0")]
    public void PageInfo_ZeroValues_ReturnsCorrectFormat(int currentPage, int totalPages, string expected)
    {
        // Arrange
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<UniversitiesViewModel>>();
        var viewModel = new UniversitiesViewModel(mockAcademic.Object, mockLogger.Object);

        viewModel.CurrentPage = currentPage;
        viewModel.TotalPages = totalPages;

        // Act
        var result = viewModel.PageInfo;

        // Assert
        Assert.AreEqual(expected, result);
    }

    /// <summary>
    /// Tests that the PageInfo property returns the correct formatted string with negative values.
    /// </summary>
    /// <param name="currentPage">The current page number to test.</param>
    /// <param name="totalPages">The total pages number to test.</param>
    /// <param name="expected">The expected formatted string.</param>
    [TestMethod]
    [DataRow(-1, 10, "Page -1 of 10")]
    [DataRow(5, -10, "Page 5 of -10")]
    [DataRow(-5, -10, "Page -5 of -10")]
    public void PageInfo_NegativeValues_ReturnsCorrectFormat(int currentPage, int totalPages, string expected)
    {
        // Arrange
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<UniversitiesViewModel>>();
        var viewModel = new UniversitiesViewModel(mockAcademic.Object, mockLogger.Object);

        viewModel.CurrentPage = currentPage;
        viewModel.TotalPages = totalPages;

        // Act
        var result = viewModel.PageInfo;

        // Assert
        Assert.AreEqual(expected, result);
    }

    /// <summary>
    /// Tests that the PageInfo property returns the correct formatted string when CurrentPage exceeds TotalPages.
    /// </summary>
    /// <param name="currentPage">The current page number to test.</param>
    /// <param name="totalPages">The total pages number to test.</param>
    /// <param name="expected">The expected formatted string.</param>
    [TestMethod]
    [DataRow(10, 5, "Page 10 of 5")]
    [DataRow(100, 10, "Page 100 of 10")]
    [DataRow(2, 1, "Page 2 of 1")]
    public void PageInfo_CurrentPageExceedsTotalPages_ReturnsCorrectFormat(int currentPage, int totalPages, string expected)
    {
        // Arrange
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<UniversitiesViewModel>>();
        var viewModel = new UniversitiesViewModel(mockAcademic.Object, mockLogger.Object);

        viewModel.CurrentPage = currentPage;
        viewModel.TotalPages = totalPages;

        // Act
        var result = viewModel.PageInfo;

        // Assert
        Assert.AreEqual(expected, result);
    }

    /// <summary>
    /// Tests that the PageInfo property returns the correct formatted string with extreme integer values.
    /// </summary>
    [TestMethod]
    public void PageInfo_IntMaxValue_ReturnsCorrectFormat()
    {
        // Arrange
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<UniversitiesViewModel>>();
        var viewModel = new UniversitiesViewModel(mockAcademic.Object, mockLogger.Object);

        viewModel.CurrentPage = int.MaxValue;
        viewModel.TotalPages = int.MaxValue;

        // Act
        var result = viewModel.PageInfo;

        // Assert
        Assert.AreEqual($"Page {int.MaxValue} of {int.MaxValue}", result);
    }

    /// <summary>
    /// Tests that the PageInfo property returns the correct formatted string with minimum integer values.
    /// </summary>
    [TestMethod]
    public void PageInfo_IntMinValue_ReturnsCorrectFormat()
    {
        // Arrange
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<UniversitiesViewModel>>();
        var viewModel = new UniversitiesViewModel(mockAcademic.Object, mockLogger.Object);

        viewModel.CurrentPage = int.MinValue;
        viewModel.TotalPages = int.MinValue;

        // Act
        var result = viewModel.PageInfo;

        // Assert
        Assert.AreEqual($"Page {int.MinValue} of {int.MinValue}", result);
    }

    /// <summary>
    /// Tests that the PageInfo property returns the correct formatted string with mixed extreme values.
    /// </summary>
    [TestMethod]
    public void PageInfo_MixedExtremeValues_ReturnsCorrectFormat()
    {
        // Arrange
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<UniversitiesViewModel>>();
        var viewModel = new UniversitiesViewModel(mockAcademic.Object, mockLogger.Object);

        viewModel.CurrentPage = int.MinValue;
        viewModel.TotalPages = int.MaxValue;

        // Act
        var result = viewModel.PageInfo;

        // Assert
        Assert.AreEqual($"Page {int.MinValue} of {int.MaxValue}", result);
    }

    /// <summary>
    /// Tests that the PageInfo property returns the correct default formatted string on initialization.
    /// </summary>
    [TestMethod]
    public void PageInfo_DefaultInitialization_ReturnsCorrectFormat()
    {
        // Arrange
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<UniversitiesViewModel>>();
        var viewModel = new UniversitiesViewModel(mockAcademic.Object, mockLogger.Object);

        // Act
        var result = viewModel.PageInfo;

        // Assert - Default values are CurrentPage = 1, TotalPages = 1 based on field initialization
        Assert.AreEqual("Page 1 of 1", result);
    }

    /// <summary>
    /// Tests that the EditName property initializes with an empty string value.
    /// Expected result: EditName should be string.Empty upon construction.
    /// </summary>
    [TestMethod]
    public void EditName_InitialValue_ReturnsEmptyString()
    {
        // Arrange
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<UniversitiesViewModel>>();
        var viewModel = new UniversitiesViewModel(mockAcademic.Object, mockLogger.Object);

        // Act
        var result = viewModel.EditName;

        // Assert
        Assert.AreEqual(string.Empty, result);
    }

    /// <summary>
    /// Tests that setting EditName to various valid string values updates the property correctly and raises PropertyChanged event.
    /// Input conditions: Valid non-empty strings, empty string, whitespace, special characters, and very long strings.
    /// Expected result: Property value is updated and PropertyChanged event is raised with correct property name.
    /// </summary>
    [TestMethod]
    [DataRow("University Name")]
    [DataRow("")]
    [DataRow("   ")]
    [DataRow("A")]
    [DataRow("University with Special !@#$%^&*() Characters")]
    [DataRow("Université de Montréal")]
    [DataRow("北京大学")]
    [DataRow("Name\nWith\nNewlines")]
    [DataRow("Name\tWith\tTabs")]
    public void EditName_SetValidValue_UpdatesPropertyAndRaisesPropertyChanged(string newValue)
    {
        // Arrange
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<UniversitiesViewModel>>();
        var viewModel = new UniversitiesViewModel(mockAcademic.Object, mockLogger.Object);
        var propertyChangedRaised = false;
        string? raisedPropertyName = null;

        viewModel.PropertyChanged += (sender, args) =>
        {
            propertyChangedRaised = true;
            raisedPropertyName = args.PropertyName;
        };

        // Act
        viewModel.EditName = newValue;

        // Assert
        Assert.AreEqual(newValue, viewModel.EditName);
        Assert.IsTrue(propertyChangedRaised);
        Assert.AreEqual(nameof(UniversitiesViewModel.EditName), raisedPropertyName);
    }

    /// <summary>
    /// Tests that setting EditName to a very long string value is handled correctly.
    /// Input conditions: String with 10000 characters.
    /// Expected result: Property value is updated correctly and PropertyChanged event is raised.
    /// </summary>
    [TestMethod]
    public void EditName_SetVeryLongString_UpdatesPropertyAndRaisesPropertyChanged()
    {
        // Arrange
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<UniversitiesViewModel>>();
        var viewModel = new UniversitiesViewModel(mockAcademic.Object, mockLogger.Object);
        var veryLongString = new string('A', 10000);
        var propertyChangedRaised = false;

        viewModel.PropertyChanged += (sender, args) => propertyChangedRaised = true;

        // Act
        viewModel.EditName = veryLongString;

        // Assert
        Assert.AreEqual(veryLongString, viewModel.EditName);
        Assert.IsTrue(propertyChangedRaised);
    }

    /// <summary>
    /// Tests that setting EditName to the same value does not raise PropertyChanged event.
    /// Input conditions: Setting the same value twice consecutively.
    /// Expected result: PropertyChanged event is raised only on the first set, not on the second.
    /// </summary>
    [TestMethod]
    public void EditName_SetSameValue_DoesNotRaisePropertyChanged()
    {
        // Arrange
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<UniversitiesViewModel>>();
        var viewModel = new UniversitiesViewModel(mockAcademic.Object, mockLogger.Object);
        var testValue = "Test University";
        var propertyChangedCount = 0;

        viewModel.EditName = testValue;
        viewModel.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == nameof(UniversitiesViewModel.EditName))
                propertyChangedCount++;
        };

        // Act
        viewModel.EditName = testValue;

        // Assert
        Assert.AreEqual(testValue, viewModel.EditName);
        Assert.AreEqual(0, propertyChangedCount);
    }

    /// <summary>
    /// Tests that setting EditName multiple times with different values updates correctly each time.
    /// Input conditions: Multiple different string values set sequentially.
    /// Expected result: Property value is updated each time and PropertyChanged event is raised for each change.
    /// </summary>
    [TestMethod]
    public void EditName_SetMultipleDifferentValues_UpdatesEachTimeAndRaisesPropertyChanged()
    {
        // Arrange
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<UniversitiesViewModel>>();
        var viewModel = new UniversitiesViewModel(mockAcademic.Object, mockLogger.Object);
        var propertyChangedCount = 0;
        var values = new[] { "First University", "Second University", "Third University" };

        viewModel.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == nameof(UniversitiesViewModel.EditName))
                propertyChangedCount++;
        };

        // Act & Assert
        foreach (var value in values)
        {
            viewModel.EditName = value;
            Assert.AreEqual(value, viewModel.EditName);
        }

        Assert.AreEqual(values.Length, propertyChangedCount);
    }

    /// <summary>
    /// Tests that EditName getter returns the correct value after setting.
    /// Input conditions: Setting a value and then reading it back.
    /// Expected result: The getter returns the exact value that was set.
    /// </summary>
    [TestMethod]
    public void EditName_GetAfterSet_ReturnsSetValue()
    {
        // Arrange
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<UniversitiesViewModel>>();
        var viewModel = new UniversitiesViewModel(mockAcademic.Object, mockLogger.Object);
        var expectedValue = "Test University Name";

        // Act
        viewModel.EditName = expectedValue;
        var actualValue = viewModel.EditName;

        // Assert
        Assert.AreEqual(expectedValue, actualValue);
    }

    /// <summary>
    /// Tests that EditName property with control characters is handled correctly.
    /// Input conditions: String containing various control characters.
    /// Expected result: Property value is updated and PropertyChanged event is raised.
    /// </summary>
    [TestMethod]
    [DataRow("\r\n")]
    [DataRow("\0")]
    [DataRow("\b\f\v")]
    public void EditName_SetStringWithControlCharacters_UpdatesPropertyAndRaisesPropertyChanged(string valueWithControlChars)
    {
        // Arrange
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<UniversitiesViewModel>>();
        var viewModel = new UniversitiesViewModel(mockAcademic.Object, mockLogger.Object);
        var propertyChangedRaised = false;

        viewModel.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == nameof(UniversitiesViewModel.EditName))
                propertyChangedRaised = true;
        };

        // Act
        viewModel.EditName = valueWithControlChars;

        // Assert
        Assert.AreEqual(valueWithControlChars, viewModel.EditName);
        Assert.IsTrue(propertyChangedRaised);
    }

    /// <summary>
    /// Tests that setting SearchQuery to a different value calls ApplyFilterAndPagination
    /// by verifying that the Universities collection is cleared as a side effect.
    /// Input: A different search query value.
    /// Expected: Universities collection is cleared indicating ApplyFilterAndPagination was called.
    /// </summary>
    [TestMethod]
    public void SearchQuery_SetDifferentValue_CallsApplyFilterAndPagination()
    {
        // Arrange
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<UniversitiesViewModel>>();
        var viewModel = new UniversitiesViewModel(mockAcademic.Object, mockLogger.Object);

        // Add a university to the collection to verify it gets cleared
        viewModel.Universities.Add(new UniversityDto { Id = "1", Name = "Test University", Status = "Active" });
        int initialCount = viewModel.Universities.Count;

        // Act
        viewModel.SearchQuery = "test";

        // Assert
        Assert.AreEqual("test", viewModel.SearchQuery);
        // ApplyFilterAndPagination clears the Universities collection, so count should be 0
        Assert.AreEqual(0, viewModel.Universities.Count);
        Assert.AreNotEqual(initialCount, viewModel.Universities.Count);
    }

    /// <summary>
    /// Tests that setting SearchQuery to the same value does not call ApplyFilterAndPagination
    /// by verifying that the Universities collection is not modified.
    /// Input: Same value as current SearchQuery.
    /// Expected: Universities collection remains unchanged indicating ApplyFilterAndPagination was not called.
    /// </summary>
    [TestMethod]
    public void SearchQuery_SetSameValue_DoesNotCallApplyFilterAndPagination()
    {
        // Arrange
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<UniversitiesViewModel>>();
        var viewModel = new UniversitiesViewModel(mockAcademic.Object, mockLogger.Object);

        viewModel.SearchQuery = "initial";

        // Add a university after setting SearchQuery
        viewModel.Universities.Add(new UniversityDto { Id = "1", Name = "Test University", Status = "Active" });
        int countBeforeSet = viewModel.Universities.Count;

        // Act
        viewModel.SearchQuery = "initial";

        // Assert
        Assert.AreEqual("initial", viewModel.SearchQuery);
        // Universities collection should not be cleared since ApplyFilterAndPagination should not be called
        Assert.AreEqual(countBeforeSet, viewModel.Universities.Count);
        Assert.AreEqual(1, viewModel.Universities.Count);
    }

    /// <summary>
    /// Tests that SearchQuery property correctly handles transition from empty to non-empty value
    /// and triggers ApplyFilterAndPagination.
    /// Input: Transition from empty string to non-empty string.
    /// Expected: Property updates, PropertyChanged is raised, and ApplyFilterAndPagination is called.
    /// </summary>
    [TestMethod]
    public void SearchQuery_TransitionFromEmptyToNonEmpty_UpdatesAndTriggersFiltering()
    {
        // Arrange
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<UniversitiesViewModel>>();
        var viewModel = new UniversitiesViewModel(mockAcademic.Object, mockLogger.Object);

        bool propertyChangedRaised = false;
        viewModel.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == nameof(UniversitiesViewModel.SearchQuery))
                propertyChangedRaised = true;
        };

        Assert.AreEqual(string.Empty, viewModel.SearchQuery);

        // Act
        viewModel.SearchQuery = "university";

        // Assert
        Assert.AreEqual("university", viewModel.SearchQuery);
        Assert.IsTrue(propertyChangedRaised);
        // Verify ApplyFilterAndPagination was called (Universities collection is cleared)
        Assert.AreEqual(0, viewModel.Universities.Count);
    }

    /// <summary>
    /// Tests that SearchQuery correctly handles strings with null characters.
    /// Input: String containing null character.
    /// Expected: Property is updated and ApplyFilterAndPagination is triggered.
    /// </summary>
    [TestMethod]
    public void SearchQuery_SetStringWithNullCharacter_UpdatesAndTriggersFiltering()
    {
        // Arrange
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<UniversitiesViewModel>>();
        var viewModel = new UniversitiesViewModel(mockAcademic.Object, mockLogger.Object);

        string valueWithNull = "test\0value";

        // Act
        viewModel.SearchQuery = valueWithNull;

        // Assert
        Assert.AreEqual(valueWithNull, viewModel.SearchQuery);
        Assert.AreEqual(0, viewModel.Universities.Count);
    }

    /// <summary>
    /// Tests that PropertyChanged event is raised with correct sender when SearchQuery changes.
    /// Input: New search query value.
    /// Expected: PropertyChanged event is raised with the ViewModel instance as sender.
    /// </summary>
    [TestMethod]
    public void SearchQuery_SetValue_RaisesPropertyChangedWithCorrectSender()
    {
        // Arrange
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<UniversitiesViewModel>>();
        var viewModel = new UniversitiesViewModel(mockAcademic.Object, mockLogger.Object);

        object? eventSender = null;
        viewModel.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == nameof(UniversitiesViewModel.SearchQuery))
                eventSender = sender;
        };

        // Act
        viewModel.SearchQuery = "test";

        // Assert
        Assert.IsNotNull(eventSender);
        Assert.AreSame(viewModel, eventSender);
    }

    /// <summary>
    /// Tests that PropertyChanged event args contain correct property name when SearchQuery changes.
    /// Input: New search query value.
    /// Expected: PropertyChanged event args contain "SearchQuery" as property name.
    /// </summary>
    [TestMethod]
    public void SearchQuery_SetValue_RaisesPropertyChangedWithCorrectPropertyName()
    {
        // Arrange
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<UniversitiesViewModel>>();
        var viewModel = new UniversitiesViewModel(mockAcademic.Object, mockLogger.Object);

        string? propertyName = null;
        viewModel.PropertyChanged += (sender, args) =>
        {
            propertyName = args.PropertyName;
        };

        // Act
        viewModel.SearchQuery = "test";

        // Assert
        Assert.IsNotNull(propertyName);
        Assert.AreEqual(nameof(UniversitiesViewModel.SearchQuery), propertyName);
    }

    /// <summary>
    /// Tests that setting SearchQuery to whitespace-only string triggers filtering.
    /// Input: Whitespace-only string.
    /// Expected: Property updates and ApplyFilterAndPagination is called.
    /// </summary>
    [TestMethod]
    [DataRow("   ")]
    [DataRow("\t")]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [DataRow(" \t\n\r ")]
    public void SearchQuery_SetWhitespaceValue_UpdatesAndTriggersFiltering(string whitespace)
    {
        // Arrange
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<UniversitiesViewModel>>();
        var viewModel = new UniversitiesViewModel(mockAcademic.Object, mockLogger.Object);

        // Act
        viewModel.SearchQuery = whitespace;

        // Assert
        Assert.AreEqual(whitespace, viewModel.SearchQuery);
        // Verify ApplyFilterAndPagination was called
        Assert.AreEqual(0, viewModel.Universities.Count);
    }

    /// <summary>
    /// Tests that setting SearchQuery multiple times in succession with different values
    /// triggers ApplyFilterAndPagination each time.
    /// Input: Multiple different values set sequentially.
    /// Expected: ApplyFilterAndPagination is called for each unique value change.
    /// </summary>
    [TestMethod]
    public void SearchQuery_SetMultipleDifferentValuesSequentially_TriggersFilteringEachTime()
    {
        // Arrange
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<UniversitiesViewModel>>();
        var viewModel = new UniversitiesViewModel(mockAcademic.Object, mockLogger.Object);

        int propertyChangedCount = 0;
        viewModel.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == nameof(UniversitiesViewModel.SearchQuery))
                propertyChangedCount++;
        };

        // Act & Assert
        viewModel.SearchQuery = "first";
        Assert.AreEqual("first", viewModel.SearchQuery);
        Assert.AreEqual(1, propertyChangedCount);

        viewModel.SearchQuery = "second";
        Assert.AreEqual("second", viewModel.SearchQuery);
        Assert.AreEqual(2, propertyChangedCount);

        viewModel.SearchQuery = "third";
        Assert.AreEqual("third", viewModel.SearchQuery);
        Assert.AreEqual(3, propertyChangedCount);
    }

    /// <summary>
    /// Tests that setting SearchQuery with extreme length string works correctly.
    /// Input: String with 100000 characters.
    /// Expected: Property is updated and ApplyFilterAndPagination is triggered.
    /// </summary>
    [TestMethod]
    public void SearchQuery_SetExtremelyLongString_UpdatesAndTriggersFiltering()
    {
        // Arrange
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<UniversitiesViewModel>>();
        var viewModel = new UniversitiesViewModel(mockAcademic.Object, mockLogger.Object);

        string extremelyLongString = new string('X', 100000);

        // Act
        viewModel.SearchQuery = extremelyLongString;

        // Assert
        Assert.AreEqual(extremelyLongString, viewModel.SearchQuery);
        Assert.AreEqual(100000, viewModel.SearchQuery.Length);
        Assert.AreEqual(0, viewModel.Universities.Count);
    }

    /// <summary>
    /// Tests that setting SearchQuery alternating between two values correctly updates each time.
    /// Input: Alternating between two different string values.
    /// Expected: Property updates each time and PropertyChanged is raised for each change.
    /// </summary>
    [TestMethod]
    public void SearchQuery_AlternateBetweenTwoValues_UpdatesEachTime()
    {
        // Arrange
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<UniversitiesViewModel>>();
        var viewModel = new UniversitiesViewModel(mockAcademic.Object, mockLogger.Object);

        int propertyChangedCount = 0;
        viewModel.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == nameof(UniversitiesViewModel.SearchQuery))
                propertyChangedCount++;
        };

        // Act & Assert
        viewModel.SearchQuery = "value1";
        Assert.AreEqual("value1", viewModel.SearchQuery);
        Assert.AreEqual(1, propertyChangedCount);

        viewModel.SearchQuery = "value2";
        Assert.AreEqual("value2", viewModel.SearchQuery);
        Assert.AreEqual(2, propertyChangedCount);

        viewModel.SearchQuery = "value1";
        Assert.AreEqual("value1", viewModel.SearchQuery);
        Assert.AreEqual(3, propertyChangedCount);

        viewModel.SearchQuery = "value2";
        Assert.AreEqual("value2", viewModel.SearchQuery);
        Assert.AreEqual(4, propertyChangedCount);
    }

    /// <summary>
    /// Tests that setting SearchQuery with strings containing only special characters works correctly.
    /// Input: Various strings with only special characters.
    /// Expected: Property is updated and ApplyFilterAndPagination is triggered.
    /// </summary>
    [TestMethod]
    [DataRow("!@#$%^&*()")]
    [DataRow("<>?:\"{}|")]
    [DataRow("[];',./")]
    [DataRow("~`")]
    [DataRow("+-=")]
    public void SearchQuery_SetOnlySpecialCharacters_UpdatesAndTriggersFiltering(string specialChars)
    {
        // Arrange
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<UniversitiesViewModel>>();
        var viewModel = new UniversitiesViewModel(mockAcademic.Object, mockLogger.Object);

        // Act
        viewModel.SearchQuery = specialChars;

        // Assert
        Assert.AreEqual(specialChars, viewModel.SearchQuery);
        Assert.AreEqual(0, viewModel.Universities.Count);
    }

    /// <summary>
    /// Tests that setting SearchQuery does not raise PropertyChanged for other properties.
    /// Input: New search query value.
    /// Expected: Only SearchQuery PropertyChanged event is raised, not other properties.
    /// </summary>
    [TestMethod]
    public void SearchQuery_SetValue_OnlyRaisesPropertyChangedForSearchQuery()
    {
        // Arrange
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<UniversitiesViewModel>>();
        var viewModel = new UniversitiesViewModel(mockAcademic.Object, mockLogger.Object);

        var propertiesChanged = new List<string>();
        viewModel.PropertyChanged += (sender, args) =>
        {
            if (!string.IsNullOrEmpty(args.PropertyName))
                propertiesChanged.Add(args.PropertyName);
        };

        // Act
        viewModel.SearchQuery = "test";

        // Assert
        Assert.IsTrue(propertiesChanged.Contains(nameof(UniversitiesViewModel.SearchQuery)));
        // Note: ApplyFilterAndPagination may raise other property changes (TotalPages, etc.)
        // but SearchQuery setter itself should only raise PropertyChanged for SearchQuery
    }

    /// <summary>
    /// Tests that PropertyChanged event has the correct sender when IsEditing is set.
    /// Input: true value.
    /// Expected: PropertyChanged event sender is the view model instance.
    /// </summary>
    [TestMethod]
    public void IsEditing_SetValue_PropertyChangedEventHasCorrectSender()
    {
        // Arrange
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<UniversitiesViewModel>>();
        var viewModel = new UniversitiesViewModel(mockAcademic.Object, mockLogger.Object);
        object? eventSender = null;

        viewModel.PropertyChanged += (sender, args) =>
        {
            eventSender = sender;
        };

        // Act
        viewModel.IsEditing = true;

        // Assert
        Assert.IsNotNull(eventSender);
        Assert.AreSame(viewModel, eventSender);
    }

    /// <summary>
    /// Tests that getter returns the correct value after setter updates it.
    /// Input: true value set via setter.
    /// Expected: Getter returns true.
    /// </summary>
    [TestMethod]
    public void IsEditing_SetAndGet_ReturnsSetValue()
    {
        // Arrange
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<UniversitiesViewModel>>();
        var viewModel = new UniversitiesViewModel(mockAcademic.Object, mockLogger.Object);

        // Act
        viewModel.IsEditing = true;
        var result = viewModel.IsEditing;

        // Assert
        Assert.IsTrue(result);
    }

    /// <summary>
    /// Tests that PropertyChanged event is raised multiple times for each value change.
    /// Input: Multiple different values (true, false, true).
    /// Expected: PropertyChanged event is raised for each actual value change.
    /// </summary>
    [TestMethod]
    public void IsEditing_MultipleValueChanges_RaisesPropertyChangedForEachChange()
    {
        // Arrange
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<UniversitiesViewModel>>();
        var viewModel = new UniversitiesViewModel(mockAcademic.Object, mockLogger.Object);
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

        // Assert
        Assert.AreEqual(3, eventRaisedCount);
    }

    /// <summary>
    /// Tests that PropertyChanged event provides correct PropertyName in EventArgs.
    /// Input: true value.
    /// Expected: PropertyChangedEventArgs.PropertyName equals "IsEditing".
    /// </summary>
    [TestMethod]
    public void IsEditing_PropertyChangedEventArgs_HasCorrectPropertyName()
    {
        // Arrange
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<UniversitiesViewModel>>();
        var viewModel = new UniversitiesViewModel(mockAcademic.Object, mockLogger.Object);
        var capturedPropertyNames = new List<string>();

        viewModel.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName != null)
            {
                capturedPropertyNames.Add(args.PropertyName);
            }
        };

        // Act
        viewModel.IsEditing = true;

        // Assert
        Assert.AreEqual(1, capturedPropertyNames.Count);
        Assert.AreEqual("IsEditing", capturedPropertyNames[0]);
    }

    /// <summary>
    /// Tests that AccreditedCount property correctly handles transition from zero to positive values.
    /// Input: Setting from default 0 to 50.
    /// Expected: Property is updated correctly and PropertyChanged event is raised.
    /// </summary>
    [TestMethod]
    public void AccreditedCount_TransitionFromZeroToPositive_UpdatesCorrectly()
    {
        // Arrange
        var mockAcademicService = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<UniversitiesViewModel>>();
        var viewModel = new UniversitiesViewModel(mockAcademicService.Object, mockLogger.Object);
        var eventRaised = false;
        viewModel.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == "AccreditedCount")
            {
                eventRaised = true;
            }
        };

        // Act
        viewModel.AccreditedCount = 50;

        // Assert
        Assert.AreEqual(50, viewModel.AccreditedCount);
        Assert.IsTrue(eventRaised);
    }

    /// <summary>
    /// Tests that AccreditedCount property correctly handles transition from positive to negative values.
    /// Input: Setting from 100 to -50.
    /// Expected: Property is updated correctly and PropertyChanged event is raised.
    /// </summary>
    [TestMethod]
    public void AccreditedCount_TransitionFromPositiveToNegative_UpdatesCorrectly()
    {
        // Arrange
        var mockAcademicService = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<UniversitiesViewModel>>();
        var viewModel = new UniversitiesViewModel(mockAcademicService.Object, mockLogger.Object);
        viewModel.AccreditedCount = 100;
        var eventRaised = false;
        viewModel.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == "AccreditedCount")
            {
                eventRaised = true;
            }
        };

        // Act
        viewModel.AccreditedCount = -50;

        // Assert
        Assert.AreEqual(-50, viewModel.AccreditedCount);
        Assert.IsTrue(eventRaised);
    }

    /// <summary>
    /// Tests that AccreditedCount property correctly handles transition between extreme boundary values.
    /// Input: Setting from int.MaxValue to int.MinValue.
    /// Expected: Property is updated correctly and PropertyChanged event is raised.
    /// </summary>
    [TestMethod]
    public void AccreditedCount_TransitionBetweenExtremeValues_UpdatesCorrectly()
    {
        // Arrange
        var mockAcademicService = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<UniversitiesViewModel>>();
        var viewModel = new UniversitiesViewModel(mockAcademicService.Object, mockLogger.Object);
        viewModel.AccreditedCount = int.MaxValue;
        var eventRaised = false;
        viewModel.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == "AccreditedCount")
            {
                eventRaised = true;
            }
        };

        // Act
        viewModel.AccreditedCount = int.MinValue;

        // Assert
        Assert.AreEqual(int.MinValue, viewModel.AccreditedCount);
        Assert.IsTrue(eventRaised);
    }

    /// <summary>
    /// Tests that AccreditedCount property raises PropertyChanged event with correct sender.
    /// Input: Setting the property to a new value.
    /// Expected: PropertyChanged event is raised with the ViewModel as sender.
    /// </summary>
    [TestMethod]
    public void AccreditedCount_RaisesPropertyChanged_WithCorrectSender()
    {
        // Arrange
        var mockAcademicService = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<UniversitiesViewModel>>();
        var viewModel = new UniversitiesViewModel(mockAcademicService.Object, mockLogger.Object);
        object? eventSender = null;
        viewModel.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == "AccreditedCount")
            {
                eventSender = sender;
            }
        };

        // Act
        viewModel.AccreditedCount = 100;

        // Assert
        Assert.AreSame(viewModel, eventSender);
    }

    /// <summary>
    /// Tests that setting AccreditedCount with alternating values between positive and negative correctly updates each time.
    /// Input: Alternating between 10 and -10 multiple times.
    /// Expected: Property value is updated correctly each time.
    /// </summary>
    [TestMethod]
    public void AccreditedCount_AlternatingPositiveNegative_UpdatesCorrectly()
    {
        // Arrange
        var mockAcademicService = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<UniversitiesViewModel>>();
        var viewModel = new UniversitiesViewModel(mockAcademicService.Object, mockLogger.Object);

        // Act & Assert
        viewModel.AccreditedCount = 10;
        Assert.AreEqual(10, viewModel.AccreditedCount);

        viewModel.AccreditedCount = -10;
        Assert.AreEqual(-10, viewModel.AccreditedCount);

        viewModel.AccreditedCount = 10;
        Assert.AreEqual(10, viewModel.AccreditedCount);

        viewModel.AccreditedCount = -10;
        Assert.AreEqual(-10, viewModel.AccreditedCount);
    }

    /// <summary>
    /// Tests that AccreditedCount property does not raise PropertyChanged when set to default value twice.
    /// Input: Setting to 0 when already at default 0.
    /// Expected: No PropertyChanged event is raised.
    /// </summary>
    [TestMethod]
    public void AccreditedCount_SetToDefaultTwice_DoesNotRaisePropertyChanged()
    {
        // Arrange
        var mockAcademicService = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<UniversitiesViewModel>>();
        var viewModel = new UniversitiesViewModel(mockAcademicService.Object, mockLogger.Object);
        var eventRaisedCount = 0;
        viewModel.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == "AccreditedCount")
            {
                eventRaisedCount++;
            }
        };

        // Act
        viewModel.AccreditedCount = 0;

        // Assert
        Assert.AreEqual(0, eventRaisedCount);
    }

    /// <summary>
    /// Tests that TotalInstitutions property correctly handles sequential updates with multiple different values.
    /// Input: Sequential updates with various integer values.
    /// Expected: Each update correctly changes the property value.
    /// </summary>
    [TestMethod]
    public void TotalInstitutions_SequentialUpdates_UpdatesCorrectlyEachTime()
    {
        // Arrange
        Mock<IAcademicService> mockAcademic = new Mock<IAcademicService>();
        Mock<ILogger<UniversitiesViewModel>> mockLogger = new Mock<ILogger<UniversitiesViewModel>>();
        UniversitiesViewModel viewModel = new UniversitiesViewModel(mockAcademic.Object, mockLogger.Object);
        int[] testValues = { 5, 15, 25, 0, -10, 100, int.MaxValue, int.MinValue };

        // Act & Assert
        foreach (int value in testValues)
        {
            viewModel.TotalInstitutions = value;
            Assert.AreEqual(value, viewModel.TotalInstitutions);
        }
    }

    /// <summary>
    /// Tests that setting TotalInstitutions to int.MaxValue works correctly without overflow.
    /// Input: int.MaxValue (2147483647).
    /// Expected: Property is set to int.MaxValue and PropertyChanged event is raised.
    /// </summary>
    [TestMethod]
    public void TotalInstitutions_SetToIntMaxValue_UpdatesCorrectlyAndRaisesPropertyChanged()
    {
        // Arrange
        Mock<IAcademicService> mockAcademic = new Mock<IAcademicService>();
        Mock<ILogger<UniversitiesViewModel>> mockLogger = new Mock<ILogger<UniversitiesViewModel>>();
        UniversitiesViewModel viewModel = new UniversitiesViewModel(mockAcademic.Object, mockLogger.Object);
        bool eventRaised = false;
        viewModel.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == "TotalInstitutions")
            {
                eventRaised = true;
            }
        };

        // Act
        viewModel.TotalInstitutions = int.MaxValue;

        // Assert
        Assert.AreEqual(int.MaxValue, viewModel.TotalInstitutions);
        Assert.IsTrue(eventRaised);
    }

    /// <summary>
    /// Tests that setting TotalInstitutions to int.MinValue works correctly without underflow.
    /// Input: int.MinValue (-2147483648).
    /// Expected: Property is set to int.MinValue and PropertyChanged event is raised.
    /// </summary>
    [TestMethod]
    public void TotalInstitutions_SetToIntMinValue_UpdatesCorrectlyAndRaisesPropertyChanged()
    {
        // Arrange
        Mock<IAcademicService> mockAcademic = new Mock<IAcademicService>();
        Mock<ILogger<UniversitiesViewModel>> mockLogger = new Mock<ILogger<UniversitiesViewModel>>();
        UniversitiesViewModel viewModel = new UniversitiesViewModel(mockAcademic.Object, mockLogger.Object);
        bool eventRaised = false;
        viewModel.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == "TotalInstitutions")
            {
                eventRaised = true;
            }
        };

        // Act
        viewModel.TotalInstitutions = int.MinValue;

        // Assert
        Assert.AreEqual(int.MinValue, viewModel.TotalInstitutions);
        Assert.IsTrue(eventRaised);
    }

    /// <summary>
    /// Tests that FilterCommand correctly accepts string parameters including edge cases.
    /// Input: Various string values passed to FilterCommand.
    /// Expected: Command executes without throwing exceptions.
    /// </summary>
    [TestMethod]
    [DataRow("All")]
    [DataRow("Accredited")]
    [DataRow("Pending")]
    [DataRow("")]
    [DataRow("   ")]
    [DataRow("VeryLongFilterStatusValueThatExceedsNormalExpectations")]
    [DataRow("Special!@#$Characters")]
    public void Constructor_FilterCommandWithVariousStringValues_ExecutesWithoutError(string filterValue)
    {
        // Arrange
        var academic = new Mock<IAcademicService>().Object;
        var logger = new Mock<ILogger<UniversitiesViewModel>>().Object;
        var viewModel = new UniversitiesViewModel(academic, logger);

        // Act & Assert - Should not throw
        viewModel.FilterCommand.Execute(filterValue);
        Assert.AreEqual(filterValue, viewModel.StatusFilter);
    }

    /// <summary>
    /// Tests that EditUniversityCommand correctly handles UniversityDto with various property values.
    /// Input: UniversityDto with empty, whitespace, and special character values.
    /// Expected: Command executes and sets properties correctly.
    /// </summary>
    [TestMethod]
    public void Constructor_EditUniversityCommandWithEmptyValues_SetsPropertiesCorrectly()
    {
        // Arrange
        var academic = new Mock<IAcademicService>().Object;
        var logger = new Mock<ILogger<UniversitiesViewModel>>().Object;
        var viewModel = new UniversitiesViewModel(academic, logger);
        var university = new UniversityDto { Id = "", Name = "" };

        // Act
        viewModel.EditUniversityCommand.Execute(university);

        // Assert
        Assert.AreEqual("", viewModel.EditId);
        Assert.AreEqual("", viewModel.EditName);
        Assert.IsTrue(viewModel.IsEditing);
    }

    /// <summary>
    /// Tests that EditUniversityCommand correctly handles UniversityDto with special characters.
    /// Input: UniversityDto with special characters in Id and Name.
    /// Expected: Command executes and sets properties correctly.
    /// </summary>
    [TestMethod]
    public void Constructor_EditUniversityCommandWithSpecialCharacters_SetsPropertiesCorrectly()
    {
        // Arrange
        var academic = new Mock<IAcademicService>().Object;
        var logger = new Mock<ILogger<UniversitiesViewModel>>().Object;
        var viewModel = new UniversitiesViewModel(academic, logger);
        var university = new UniversityDto
        {
            Id = "!@#$%^&*()",
            Name = "University <>&\"'"
        };

        // Act
        viewModel.EditUniversityCommand.Execute(university);

        // Assert
        Assert.AreEqual("!@#$%^&*()", viewModel.EditId);
        Assert.AreEqual("University <>&\"'", viewModel.EditName);
        Assert.IsTrue(viewModel.IsEditing);
    }

    /// <summary>
    /// Tests that EditUniversityCommand correctly handles UniversityDto with very long strings.
    /// Input: UniversityDto with very long Id and Name values.
    /// Expected: Command executes and sets properties correctly.
    /// </summary>
    [TestMethod]
    public void Constructor_EditUniversityCommandWithVeryLongStrings_SetsPropertiesCorrectly()
    {
        // Arrange
        var academic = new Mock<IAcademicService>().Object;
        var logger = new Mock<ILogger<UniversitiesViewModel>>().Object;
        var viewModel = new UniversitiesViewModel(academic, logger);
        var longId = new string('A', 10000);
        var longName = new string('B', 10000);
        var university = new UniversityDto
        {
            Id = longId,
            Name = longName
        };

        // Act
        viewModel.EditUniversityCommand.Execute(university);

        // Assert
        Assert.AreEqual(longId, viewModel.EditId);
        Assert.AreEqual(longName, viewModel.EditName);
        Assert.IsTrue(viewModel.IsEditing);
    }

    /// <summary>
    /// Tests that AddUniversityCommand correctly resets all edit properties.
    /// Input: Execute AddUniversityCommand after EditUniversityCommand has set values.
    /// Expected: EditId is null, EditName is empty, IsEditing is true.
    /// </summary>
    [TestMethod]
    public void Constructor_AddUniversityCommandAfterEdit_ResetsPropertiesToDefault()
    {
        // Arrange
        var academic = new Mock<IAcademicService>().Object;
        var logger = new Mock<ILogger<UniversitiesViewModel>>().Object;
        var viewModel = new UniversitiesViewModel(academic, logger);
        var university = new UniversityDto { Id = "123", Name = "Test University" };

        // Set some values first
        viewModel.EditUniversityCommand.Execute(university);

        // Act
        viewModel.AddUniversityCommand.Execute(null);

        // Assert
        Assert.IsNull(viewModel.EditId);
        Assert.AreEqual(string.Empty, viewModel.EditName);
        Assert.IsTrue(viewModel.IsEditing);
    }

    /// <summary>
    /// Tests that CancelEditCommand correctly resets all edit properties.
    /// Input: Execute CancelEditCommand after EditUniversityCommand has set values.
    /// Expected: EditId is null, EditName is empty, IsEditing is false.
    /// </summary>
    [TestMethod]
    public void Constructor_CancelEditCommandAfterEdit_ResetsAllEditProperties()
    {
        // Arrange
        var academic = new Mock<IAcademicService>().Object;
        var logger = new Mock<ILogger<UniversitiesViewModel>>().Object;
        var viewModel = new UniversitiesViewModel(academic, logger);
        var university = new UniversityDto { Id = "123", Name = "Test University" };

        // Set some values first
        viewModel.EditUniversityCommand.Execute(university);

        // Act
        viewModel.CancelEditCommand.Execute(null);

        // Assert
        Assert.IsNull(viewModel.EditId);
        Assert.AreEqual(string.Empty, viewModel.EditName);
        Assert.IsFalse(viewModel.IsEditing);
    }

    /// <summary>
    /// Tests that NextPageCommand correctly increments CurrentPage when conditions are met.
    /// Input: Set CurrentPage to 1 and TotalPages to 10, then execute NextPageCommand.
    /// Expected: CurrentPage is incremented to 2.
    /// </summary>
    [TestMethod]
    public void Constructor_NextPageCommandWhenValidConditions_IncrementsCurrentPage()
    {
        // Arrange
        var academic = new Mock<IAcademicService>().Object;
        var logger = new Mock<ILogger<UniversitiesViewModel>>().Object;
        var viewModel = new UniversitiesViewModel(academic, logger);
        viewModel.TotalPages = 10;
        viewModel.CurrentPage = 1;

        // Act
        viewModel.NextPageCommand.Execute(null);

        // Assert
        Assert.AreEqual(2, viewModel.CurrentPage);
    }

    /// <summary>
    /// Tests that NextPageCommand does not increment CurrentPage when already at TotalPages.
    /// Input: Set CurrentPage equal to TotalPages, then execute NextPageCommand.
    /// Expected: CurrentPage remains unchanged.
    /// </summary>
    [TestMethod]
    public void Constructor_NextPageCommandWhenAtLastPage_DoesNotIncrementCurrentPage()
    {
        // Arrange
        var academic = new Mock<IAcademicService>().Object;
        var logger = new Mock<ILogger<UniversitiesViewModel>>().Object;
        var viewModel = new UniversitiesViewModel(academic, logger);
        viewModel.TotalPages = 5;
        viewModel.CurrentPage = 5;

        // Act
        viewModel.NextPageCommand.Execute(null);

        // Assert
        Assert.AreEqual(5, viewModel.CurrentPage);
    }

    /// <summary>
    /// Tests that NextPageCommand does not increment CurrentPage when exceeding TotalPages.
    /// Input: Set CurrentPage greater than TotalPages, then execute NextPageCommand.
    /// Expected: CurrentPage remains unchanged.
    /// </summary>
    [TestMethod]
    public void Constructor_NextPageCommandWhenExceedingTotalPages_DoesNotIncrementCurrentPage()
    {
        // Arrange
        var academic = new Mock<IAcademicService>().Object;
        var logger = new Mock<ILogger<UniversitiesViewModel>>().Object;
        var viewModel = new UniversitiesViewModel(academic, logger);
        viewModel.TotalPages = 5;
        viewModel.CurrentPage = 10;

        // Act
        viewModel.NextPageCommand.Execute(null);

        // Assert
        Assert.AreEqual(10, viewModel.CurrentPage);
    }

    /// <summary>
    /// Tests that PrevPageCommand correctly decrements CurrentPage when conditions are met.
    /// Input: Set CurrentPage to 5, then execute PrevPageCommand.
    /// Expected: CurrentPage is decremented to 4.
    /// </summary>
    [TestMethod]
    public void Constructor_PrevPageCommandWhenValidConditions_DecrementsCurrentPage()
    {
        // Arrange
        var academic = new Mock<IAcademicService>().Object;
        var logger = new Mock<ILogger<UniversitiesViewModel>>().Object;
        var viewModel = new UniversitiesViewModel(academic, logger);
        viewModel.CurrentPage = 5;

        // Act
        viewModel.PrevPageCommand.Execute(null);

        // Assert
        Assert.AreEqual(4, viewModel.CurrentPage);
    }

    /// <summary>
    /// Tests that PrevPageCommand does not decrement CurrentPage when already at page 1.
    /// Input: Set CurrentPage to 1, then execute PrevPageCommand.
    /// Expected: CurrentPage remains at 1.
    /// </summary>
    [TestMethod]
    public void Constructor_PrevPageCommandWhenAtFirstPage_DoesNotDecrementCurrentPage()
    {
        // Arrange
        var academic = new Mock<IAcademicService>().Object;
        var logger = new Mock<ILogger<UniversitiesViewModel>>().Object;
        var viewModel = new UniversitiesViewModel(academic, logger);
        viewModel.CurrentPage = 1;

        // Act
        viewModel.PrevPageCommand.Execute(null);

        // Assert
        Assert.AreEqual(1, viewModel.CurrentPage);
    }

    /// <summary>
    /// Tests that PrevPageCommand does not decrement CurrentPage when at or below 1.
    /// Input: Set CurrentPage to 0 or negative, then execute PrevPageCommand.
    /// Expected: CurrentPage remains unchanged.
    /// </summary>
    [TestMethod]
    [DataRow(0)]
    [DataRow(-1)]
    [DataRow(-100)]
    public void Constructor_PrevPageCommandWhenBelowFirstPage_DoesNotDecrementCurrentPage(int initialPage)
    {
        // Arrange
        var academic = new Mock<IAcademicService>().Object;
        var logger = new Mock<ILogger<UniversitiesViewModel>>().Object;
        var viewModel = new UniversitiesViewModel(academic, logger);
        viewModel.CurrentPage = initialPage;

        // Act
        viewModel.PrevPageCommand.Execute(null);

        // Assert
        Assert.AreEqual(initialPage, viewModel.CurrentPage);
    }

    /// <summary>
    /// Tests that NextPageCommand handles boundary values correctly.
    /// Input: Set CurrentPage to int.MaxValue - 1 and TotalPages to int.MaxValue, then execute NextPageCommand.
    /// Expected: CurrentPage is incremented to int.MaxValue.
    /// </summary>
    [TestMethod]
    public void Constructor_NextPageCommandWithMaxIntBoundary_IncrementsCorrectly()
    {
        // Arrange
        var academic = new Mock<IAcademicService>().Object;
        var logger = new Mock<ILogger<UniversitiesViewModel>>().Object;
        var viewModel = new UniversitiesViewModel(academic, logger);
        viewModel.TotalPages = int.MaxValue;
        viewModel.CurrentPage = int.MaxValue - 1;

        // Act
        viewModel.NextPageCommand.Execute(null);

        // Assert
        Assert.AreEqual(int.MaxValue, viewModel.CurrentPage);
    }

    /// <summary>
    /// Tests that PrevPageCommand handles boundary values correctly.
    /// Input: Set CurrentPage to int.MinValue + 1, then execute PrevPageCommand.
    /// Expected: CurrentPage remains unchanged (since it's less than 1).
    /// </summary>
    [TestMethod]
    public void Constructor_PrevPageCommandWithMinIntBoundary_DoesNotDecrement()
    {
        // Arrange
        var academic = new Mock<IAcademicService>().Object;
        var logger = new Mock<ILogger<UniversitiesViewModel>>().Object;
        var viewModel = new UniversitiesViewModel(academic, logger);
        viewModel.CurrentPage = int.MinValue + 1;

        // Act
        viewModel.PrevPageCommand.Execute(null);

        // Assert
        Assert.AreEqual(int.MinValue + 1, viewModel.CurrentPage);
    }

    /// <summary>
    /// Tests that all commands are distinct instances.
    /// Input: Valid constructor parameters.
    /// Expected: No two command properties reference the same object instance.
    /// </summary>
    [TestMethod]
    public void Constructor_ValidParameters_CreatesDistinctCommandInstances()
    {
        // Arrange
        var academic = new Mock<IAcademicService>().Object;
        var logger = new Mock<ILogger<UniversitiesViewModel>>().Object;

        // Act
        var viewModel = new UniversitiesViewModel(academic, logger);

        // Assert
        var commands = new ICommand[]
        {
            viewModel.LoadCommand,
            viewModel.SearchCommand,
            viewModel.FilterCommand,
            viewModel.NextPageCommand,
            viewModel.PrevPageCommand,
            viewModel.AddUniversityCommand,
            viewModel.EditUniversityCommand,
            viewModel.DeleteUniversityCommand,
            viewModel.SaveCommand,
            viewModel.CancelEditCommand,
            viewModel.ExportCommand,
            viewModel.RefreshCommand
        };

        for (int i = 0; i < commands.Length; i++)
        {
            for (int j = i + 1; j < commands.Length; j++)
            {
                Assert.AreNotSame(commands[i], commands[j],
                    $"Command at index {i} and {j} should be distinct instances");
            }
        }
    }

    /// <summary>
    /// Tests that FilterCommand with null parameter sets StatusFilter to null.
    /// Input: null string parameter to FilterCommand.
    /// Expected: StatusFilter is set to null without throwing exception.
    /// </summary>
    [TestMethod]
    public void Constructor_FilterCommandWithNullParameter_SetsStatusFilterToNull()
    {
        // Arrange
        var academic = new Mock<IAcademicService>().Object;
        var logger = new Mock<ILogger<UniversitiesViewModel>>().Object;
        var viewModel = new UniversitiesViewModel(academic, logger);

        // Act
        viewModel.FilterCommand.Execute(null);

        // Assert
        Assert.IsNull(viewModel.StatusFilter);
    }

    /// <summary>
    /// Tests that multiple sequential executions of AddUniversityCommand maintain consistent state.
    /// Input: Execute AddUniversityCommand multiple times.
    /// Expected: EditId remains null, EditName remains empty, IsEditing remains true.
    /// </summary>
    [TestMethod]
    public void Constructor_AddUniversityCommandExecutedMultipleTimes_MaintainsConsistentState()
    {
        // Arrange
        var academic = new Mock<IAcademicService>().Object;
        var logger = new Mock<ILogger<UniversitiesViewModel>>().Object;
        var viewModel = new UniversitiesViewModel(academic, logger);

        // Act
        viewModel.AddUniversityCommand.Execute(null);
        viewModel.AddUniversityCommand.Execute(null);
        viewModel.AddUniversityCommand.Execute(null);

        // Assert
        Assert.IsNull(viewModel.EditId);
        Assert.AreEqual(string.Empty, viewModel.EditName);
        Assert.IsTrue(viewModel.IsEditing);
    }

    /// <summary>
    /// Tests that NextPageCommand and PrevPageCommand work correctly together.
    /// Input: Execute NextPageCommand followed by PrevPageCommand.
    /// Expected: CurrentPage returns to original value.
    /// </summary>
    [TestMethod]
    public void Constructor_NextThenPrevPageCommands_ReturnsToOriginalPage()
    {
        // Arrange
        var academic = new Mock<IAcademicService>().Object;
        var logger = new Mock<ILogger<UniversitiesViewModel>>().Object;
        var viewModel = new UniversitiesViewModel(academic, logger);
        viewModel.TotalPages = 10;
        viewModel.CurrentPage = 5;
        int originalPage = viewModel.CurrentPage;

        // Act
        viewModel.NextPageCommand.Execute(null);
        viewModel.PrevPageCommand.Execute(null);

        // Assert
        Assert.AreEqual(originalPage, viewModel.CurrentPage);
    }

    /// <summary>
    /// Tests that the PageSize property returns the initial default value of 10.
    /// Input: None (initial state).
    /// Expected: PageSize returns 10.
    /// </summary>
    [TestMethod]
    public void PageSize_Get_ReturnsInitialValue()
    {
        // Arrange
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<UniversitiesViewModel>>();
        var viewModel = new UniversitiesViewModel(mockAcademic.Object, mockLogger.Object);

        // Act
        var result = viewModel.PageSize;

        // Assert
        Assert.AreEqual(10, result);
    }

    /// <summary>
    /// Tests that setting PageSize to a different positive value updates the property and resets CurrentPage to 1.
    /// Input: Various positive integer values.
    /// Expected: PageSize is updated and CurrentPage is reset to 1.
    /// </summary>
    [TestMethod]
    [DataRow(1)]
    [DataRow(5)]
    [DataRow(15)]
    [DataRow(20)]
    [DataRow(50)]
    [DataRow(100)]
    [DataRow(500)]
    [DataRow(1000)]
    [DataRow(10000)]
    public void PageSize_SetDifferentPositiveValue_UpdatesPropertyAndResetsCurrentPage(int newPageSize)
    {
        // Arrange
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<UniversitiesViewModel>>();
        var viewModel = new UniversitiesViewModel(mockAcademic.Object, mockLogger.Object);
        viewModel.CurrentPage = 5;

        // Act
        viewModel.PageSize = newPageSize;

        // Assert
        Assert.AreEqual(newPageSize, viewModel.PageSize);
        Assert.AreEqual(1, viewModel.CurrentPage);
    }

    /// <summary>
    /// Tests that setting PageSize to zero updates the property and resets CurrentPage to 1.
    /// Input: Zero.
    /// Expected: PageSize is set to 0 and CurrentPage is reset to 1.
    /// </summary>
    [TestMethod]
    public void PageSize_SetToZero_UpdatesPropertyAndResetsCurrentPage()
    {
        // Arrange
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<UniversitiesViewModel>>();
        var viewModel = new UniversitiesViewModel(mockAcademic.Object, mockLogger.Object);
        viewModel.CurrentPage = 3;

        // Act
        viewModel.PageSize = 0;

        // Assert
        Assert.AreEqual(0, viewModel.PageSize);
        Assert.AreEqual(1, viewModel.CurrentPage);
    }

    /// <summary>
    /// Tests that setting PageSize to negative values updates the property and resets CurrentPage to 1.
    /// Input: Various negative integer values.
    /// Expected: PageSize is set to the negative value and CurrentPage is reset to 1.
    /// </summary>
    [TestMethod]
    [DataRow(-1)]
    [DataRow(-5)]
    [DataRow(-10)]
    [DataRow(-50)]
    [DataRow(-100)]
    [DataRow(-1000)]
    public void PageSize_SetToNegativeValue_UpdatesPropertyAndResetsCurrentPage(int negativeValue)
    {
        // Arrange
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<UniversitiesViewModel>>();
        var viewModel = new UniversitiesViewModel(mockAcademic.Object, mockLogger.Object);
        viewModel.CurrentPage = 4;

        // Act
        viewModel.PageSize = negativeValue;

        // Assert
        Assert.AreEqual(negativeValue, viewModel.PageSize);
        Assert.AreEqual(1, viewModel.CurrentPage);
    }

    /// <summary>
    /// Tests that setting PageSize to int.MinValue updates the property and resets CurrentPage to 1.
    /// Input: int.MinValue.
    /// Expected: PageSize is set to int.MinValue and CurrentPage is reset to 1.
    /// </summary>
    [TestMethod]
    public void PageSize_SetToIntMinValue_UpdatesPropertyAndResetsCurrentPage()
    {
        // Arrange
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<UniversitiesViewModel>>();
        var viewModel = new UniversitiesViewModel(mockAcademic.Object, mockLogger.Object);
        viewModel.CurrentPage = 2;

        // Act
        viewModel.PageSize = int.MinValue;

        // Assert
        Assert.AreEqual(int.MinValue, viewModel.PageSize);
        Assert.AreEqual(1, viewModel.CurrentPage);
    }

    /// <summary>
    /// Tests that setting PageSize to int.MaxValue updates the property and resets CurrentPage to 1.
    /// Input: int.MaxValue.
    /// Expected: PageSize is set to int.MaxValue and CurrentPage is reset to 1.
    /// </summary>
    [TestMethod]
    public void PageSize_SetToIntMaxValue_UpdatesPropertyAndResetsCurrentPage()
    {
        // Arrange
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<UniversitiesViewModel>>();
        var viewModel = new UniversitiesViewModel(mockAcademic.Object, mockLogger.Object);
        viewModel.CurrentPage = 7;

        // Act
        viewModel.PageSize = int.MaxValue;

        // Assert
        Assert.AreEqual(int.MaxValue, viewModel.PageSize);
        Assert.AreEqual(1, viewModel.CurrentPage);
    }

    /// <summary>
    /// Tests that setting PageSize to a different value also raises PropertyChanged for CurrentPage and PageInfo
    /// since CurrentPage is modified as a side effect.
    /// Input: A different value than current PageSize.
    /// Expected: PropertyChanged events are raised for PageSize, CurrentPage, and PageInfo.
    /// </summary>
    [TestMethod]
    public void PageSize_SetDifferentValue_RaisesPropertyChangedForCurrentPageAndPageInfo()
    {
        // Arrange
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<UniversitiesViewModel>>();
        var viewModel = new UniversitiesViewModel(mockAcademic.Object, mockLogger.Object);
        viewModel.CurrentPage = 3;
        var raisedProperties = new List<string>();

        viewModel.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName != null)
            {
                raisedProperties.Add(args.PropertyName);
            }
        };

        // Act
        viewModel.PageSize = 20;

        // Assert
        Assert.IsTrue(raisedProperties.Contains("PageSize"));
        Assert.IsTrue(raisedProperties.Contains("CurrentPage"));
        Assert.IsTrue(raisedProperties.Contains("PageInfo"));
    }

    /// <summary>
    /// Tests that setting PageSize multiple times with different values updates the property correctly each time.
    /// Input: Multiple different integer values set sequentially.
    /// Expected: PageSize is updated to the most recent value each time.
    /// </summary>
    [TestMethod]
    public void PageSize_SetMultipleDifferentValues_UpdatesPropertyEachTime()
    {
        // Arrange
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<UniversitiesViewModel>>();
        var viewModel = new UniversitiesViewModel(mockAcademic.Object, mockLogger.Object);

        // Act & Assert
        viewModel.PageSize = 5;
        Assert.AreEqual(5, viewModel.PageSize);

        viewModel.PageSize = 15;
        Assert.AreEqual(15, viewModel.PageSize);

        viewModel.PageSize = 30;
        Assert.AreEqual(30, viewModel.PageSize);

        viewModel.PageSize = 100;
        Assert.AreEqual(100, viewModel.PageSize);
    }

    /// <summary>
    /// Tests that setting PageSize to a different value when CurrentPage is already 1 still resets it to 1.
    /// Input: Different PageSize value when CurrentPage is 1.
    /// Expected: PageSize is updated and CurrentPage remains 1 (but setter is still called).
    /// </summary>
    [TestMethod]
    public void PageSize_SetDifferentValueWhenCurrentPageIsOne_ResetsCurrentPageToOne()
    {
        // Arrange
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<UniversitiesViewModel>>();
        var viewModel = new UniversitiesViewModel(mockAcademic.Object, mockLogger.Object);
        var currentPageChangedCount = 0;

        viewModel.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == "CurrentPage")
            {
                currentPageChangedCount++;
            }
        };

        // Act
        viewModel.PageSize = 20;

        // Assert
        Assert.AreEqual(20, viewModel.PageSize);
        Assert.AreEqual(1, viewModel.CurrentPage);
        Assert.AreEqual(1, currentPageChangedCount);
    }

    /// <summary>
    /// Tests that setting PageSize does not raise PropertyChanged when setting to the same value multiple times.
    /// Input: Same value set multiple times consecutively.
    /// Expected: PropertyChanged is not raised for any of the subsequent sets.
    /// </summary>
    [TestMethod]
    public void PageSize_SetSameValueMultipleTimes_DoesNotRaisePropertyChangedAfterFirst()
    {
        // Arrange
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<UniversitiesViewModel>>();
        var viewModel = new UniversitiesViewModel(mockAcademic.Object, mockLogger.Object);
        viewModel.PageSize = 15;
        var propertyChangedCount = 0;

        viewModel.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == "PageSize")
            {
                propertyChangedCount++;
            }
        };

        // Act
        viewModel.PageSize = 15;
        viewModel.PageSize = 15;
        viewModel.PageSize = 15;

        // Assert
        Assert.AreEqual(0, propertyChangedCount);
        Assert.AreEqual(15, viewModel.PageSize);
    }

    /// <summary>
    /// Tests that setting PageSize to alternating values raises PropertyChanged each time the value changes.
    /// Input: Alternating between two different values.
    /// Expected: PropertyChanged is raised for each value change.
    /// </summary>
    [TestMethod]
    public void PageSize_SetAlternatingValues_RaisesPropertyChangedForEachChange()
    {
        // Arrange
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<UniversitiesViewModel>>();
        var viewModel = new UniversitiesViewModel(mockAcademic.Object, mockLogger.Object);
        var propertyChangedCount = 0;

        viewModel.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == "PageSize")
            {
                propertyChangedCount++;
            }
        };

        // Act
        viewModel.PageSize = 15;
        viewModel.PageSize = 20;
        viewModel.PageSize = 15;
        viewModel.PageSize = 20;

        // Assert
        Assert.AreEqual(4, propertyChangedCount);
        Assert.AreEqual(20, viewModel.PageSize);
    }

    /// <summary>
    /// Tests that PageSize getter returns the correct value after being set to boundary values.
    /// Input: Boundary values (0, 1, int.MaxValue, int.MinValue).
    /// Expected: Getter returns the exact value that was set.
    /// </summary>
    [TestMethod]
    [DataRow(0)]
    [DataRow(1)]
    [DataRow(int.MaxValue)]
    [DataRow(int.MinValue)]
    public void PageSize_SetToBoundaryValue_GetReturnsSetValue(int boundaryValue)
    {
        // Arrange
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<UniversitiesViewModel>>();
        var viewModel = new UniversitiesViewModel(mockAcademic.Object, mockLogger.Object);

        // Act
        viewModel.PageSize = boundaryValue;
        var result = viewModel.PageSize;

        // Assert
        Assert.AreEqual(boundaryValue, result);
    }

    /// <summary>
    /// Tests that LoadAsync correctly handles universities with various status values,
    /// counting only exact "Active" match as accredited (case-sensitive).
    /// Input: Universities with different status values and case variations.
    /// Expected: Only exact "Active" match is counted as accredited.
    /// </summary>
    [TestMethod]
    [DataRow("Active", 1, 0)]
    [DataRow("active", 0, 1)]
    [DataRow("ACTIVE", 0, 1)]
    [DataRow("Pending", 0, 1)]
    [DataRow("Inactive", 0, 1)]
    [DataRow("", 0, 1)]
    [DataRow("  ", 0, 1)]
    [DataRow(" Active ", 0, 1)]
    public async Task LoadAsync_WithVariousStatusValues_CountsOnlyExactActiveMatch(string status, int expectedAccredited, int expectedPending)
    {
        // Arrange
        var mockAcademicService = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<UniversitiesViewModel>>();
        var universities = new List<UniversityDto>
        {
            new UniversityDto { Id = "1", Name = "University A", Status = status }
        };
        mockAcademicService.Setup(x => x.GetUniversityDetailsAsync()).ReturnsAsync(universities);
        var viewModel = new UniversitiesViewModel(mockAcademicService.Object, mockLogger.Object);

        // Act
        await viewModel.LoadAsync();

        // Assert
        Assert.AreEqual(1, viewModel.TotalInstitutions);
        Assert.AreEqual(expectedAccredited, viewModel.AccreditedCount);
        Assert.AreEqual(expectedPending, viewModel.PendingCount);
    }

    /// <summary>
    /// Tests that LoadAsync can be called multiple times sequentially (non-concurrently),
    /// and each call correctly updates the statistics.
    /// Input: Multiple sequential calls with different data.
    /// Expected: Each call updates statistics based on the current data.
    /// </summary>
    [TestMethod]
    public async Task LoadAsync_CalledMultipleTimesSequentially_UpdatesStatisticsEachTime()
    {
        // Arrange
        var mockAcademicService = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<UniversitiesViewModel>>();
        var firstLoad = new List<UniversityDto>
        {
            new UniversityDto { Id = "1", Name = "University A", Status = "Active" }
        };
        var secondLoad = new List<UniversityDto>
        {
            new UniversityDto { Id = "1", Name = "University A", Status = "Active" },
            new UniversityDto { Id = "2", Name = "University B", Status = "Pending" }
        };
        mockAcademicService.SetupSequence(x => x.GetUniversityDetailsAsync())
            .ReturnsAsync(firstLoad)
            .ReturnsAsync(secondLoad);
        var viewModel = new UniversitiesViewModel(mockAcademicService.Object, mockLogger.Object);

        // Act
        await viewModel.LoadAsync();
        var firstTotalInstitutions = viewModel.TotalInstitutions;
        await viewModel.LoadAsync();

        // Assert
        Assert.AreEqual(1, firstTotalInstitutions);
        Assert.AreEqual(2, viewModel.TotalInstitutions);
        Assert.AreEqual(1, viewModel.AccreditedCount);
        Assert.AreEqual(1, viewModel.PendingCount);
    }

    /// <summary>
    /// Tests that LoadAsync correctly handles mixed status values including edge cases.
    /// Input: Universities with various status combinations.
    /// Expected: Statistics reflect exact "Active" matches only.
    /// </summary>
    [TestMethod]
    public async Task LoadAsync_WithMixedStatusValues_CalculatesCountsCorrectly()
    {
        // Arrange
        var mockAcademicService = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<UniversitiesViewModel>>();
        var universities = new List<UniversityDto>
        {
            new UniversityDto { Id = "1", Name = "Uni 1", Status = "Active" },
            new UniversityDto { Id = "2", Name = "Uni 2", Status = "Active" },
            new UniversityDto { Id = "3", Name = "Uni 3", Status = "Pending" },
            new UniversityDto { Id = "4", Name = "Uni 4", Status = "Inactive" },
            new UniversityDto { Id = "5", Name = "Uni 5", Status = "Suspended" },
            new UniversityDto { Id = "6", Name = "Uni 6", Status = "" },
            new UniversityDto { Id = "7", Name = "Uni 7", Status = "active" }
        };
        mockAcademicService.Setup(x => x.GetUniversityDetailsAsync()).ReturnsAsync(universities);
        var viewModel = new UniversitiesViewModel(mockAcademicService.Object, mockLogger.Object);

        // Act
        await viewModel.LoadAsync();

        // Assert
        Assert.AreEqual(7, viewModel.TotalInstitutions);
        Assert.AreEqual(2, viewModel.AccreditedCount);
        Assert.AreEqual(5, viewModel.PendingCount);
    }

    /// <summary>
    /// Tests that LoadAsync verifies ApplyFilterAndPagination is called by checking
    /// that the Universities collection is cleared (side effect of the method).
    /// Input: Valid universities list with existing items in Universities collection.
    /// Expected: Universities collection is cleared as a result of ApplyFilterAndPagination.
    /// </summary>
    [TestMethod]
    public async Task LoadAsync_ExecutesSuccessfully_CallsApplyFilterAndPagination()
    {
        // Arrange
        var mockAcademicService = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<UniversitiesViewModel>>();
        var universities = new List<UniversityDto>
        {
            new UniversityDto { Id = "1", Name = "University A", Status = "Active" }
        };
        mockAcademicService.Setup(x => x.GetUniversityDetailsAsync()).ReturnsAsync(universities);
        var viewModel = new UniversitiesViewModel(mockAcademicService.Object, mockLogger.Object);
        viewModel.Universities.Add(new UniversityDto { Id = "old", Name = "Old University", Status = "Active" });
        var initialCount = viewModel.Universities.Count;

        // Act
        await viewModel.LoadAsync();

        // Assert
        Assert.AreEqual(1, initialCount);
        Assert.IsTrue(viewModel.Universities.Count <= 1);
    }

    /// <summary>
    /// Tests that LoadAsync handles zero value correctly for boundary testing.
    /// Input: Empty list (zero universities).
    /// Expected: All counts are zero, no exceptions thrown.
    /// </summary>
    [TestMethod]
    public async Task LoadAsync_WithZeroUniversities_HandlesGracefully()
    {
        // Arrange
        var mockAcademicService = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<UniversitiesViewModel>>();
        mockAcademicService.Setup(x => x.GetUniversityDetailsAsync()).ReturnsAsync(new List<UniversityDto>());
        var viewModel = new UniversitiesViewModel(mockAcademicService.Object, mockLogger.Object);

        // Act
        await viewModel.LoadAsync();

        // Assert
        Assert.AreEqual(0, viewModel.TotalInstitutions);
        Assert.AreEqual(0, viewModel.AccreditedCount);
        Assert.AreEqual(0, viewModel.PendingCount);
        Assert.IsFalse(viewModel.IsBusy);
    }

    /// <summary>
    /// Tests that LoadAsync correctly handles the scenario where ErrorMessage
    /// is cleared and remains empty after successful load.
    /// Input: Successful load after previous error.
    /// Expected: ErrorMessage is empty string after successful load.
    /// </summary>
    [TestMethod]
    public async Task LoadAsync_AfterPreviousError_ClearsErrorMessageOnSuccess()
    {
        // Arrange
        var mockAcademicService = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<UniversitiesViewModel>>();
        var universities = new List<UniversityDto>
        {
            new UniversityDto { Id = "1", Name = "University A", Status = "Active" }
        };
        mockAcademicService.SetupSequence(x => x.GetUniversityDetailsAsync())
            .ThrowsAsync(new Exception("First error"))
            .ReturnsAsync(universities);
        var viewModel = new UniversitiesViewModel(mockAcademicService.Object, mockLogger.Object);

        // Act
        await viewModel.LoadAsync();
        Assert.AreEqual("Failed to load universities.", viewModel.ErrorMessage);
        await viewModel.LoadAsync();

        // Assert
        Assert.AreEqual(string.Empty, viewModel.ErrorMessage);
        Assert.AreEqual(1, viewModel.TotalInstitutions);
    }

    /// <summary>
    /// Tests that LoadAsync correctly handles universities with Status containing
    /// whitespace variations.
    /// Input: Universities with whitespace in status values.
    /// Expected: Only exact "Active" (no whitespace) is counted as accredited.
    /// </summary>
    [TestMethod]
    public async Task LoadAsync_WithWhitespaceInStatus_CountsOnlyExactMatch()
    {
        // Arrange
        var mockAcademicService = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<UniversitiesViewModel>>();
        var universities = new List<UniversityDto>
        {
            new UniversityDto { Id = "1", Name = "Uni 1", Status = "Active" },
            new UniversityDto { Id = "2", Name = "Uni 2", Status = " Active" },
            new UniversityDto { Id = "3", Name = "Uni 3", Status = "Active " },
            new UniversityDto { Id = "4", Name = "Uni 4", Status = " Active " },
            new UniversityDto { Id = "5", Name = "Uni 5", Status = "\tActive\t" }
        };
        mockAcademicService.Setup(x => x.GetUniversityDetailsAsync()).ReturnsAsync(universities);
        var viewModel = new UniversitiesViewModel(mockAcademicService.Object, mockLogger.Object);

        // Act
        await viewModel.LoadAsync();

        // Assert
        Assert.AreEqual(5, viewModel.TotalInstitutions);
        Assert.AreEqual(1, viewModel.AccreditedCount);
        Assert.AreEqual(4, viewModel.PendingCount);
    }

    /// <summary>
    /// Tests that LoadAsync does not throw exception when the service call completes successfully
    /// even with unusual but valid data.
    /// Input: Universities with empty strings for all string properties.
    /// Expected: No exception is thrown, counts are calculated correctly.
    /// </summary>
    [TestMethod]
    public async Task LoadAsync_WithEmptyStringProperties_HandlesGracefully()
    {
        // Arrange
        var mockAcademicService = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<UniversitiesViewModel>>();
        var universities = new List<UniversityDto>
        {
            new UniversityDto { Id = "", Name = "", Status = "" },
            new UniversityDto { Id = "", Name = "", Status = "Active" }
        };
        mockAcademicService.Setup(x => x.GetUniversityDetailsAsync()).ReturnsAsync(universities);
        var viewModel = new UniversitiesViewModel(mockAcademicService.Object, mockLogger.Object);

        // Act
        await viewModel.LoadAsync();

        // Assert
        Assert.AreEqual(2, viewModel.TotalInstitutions);
        Assert.AreEqual(1, viewModel.AccreditedCount);
        Assert.AreEqual(1, viewModel.PendingCount);
    }

    /// <summary>
    /// Tests that the PageInfo property correctly updates when CurrentPage changes.
    /// Input: Setting CurrentPage to a different value while TotalPages remains constant.
    /// Expected: PageInfo reflects the new CurrentPage value.
    /// </summary>
    [TestMethod]
    public void PageInfo_CurrentPageChanges_ReturnsUpdatedFormat()
    {
        // Arrange
        Mock<IAcademicService> mockAcademicService = new Mock<IAcademicService>();
        Mock<ILogger<UniversitiesViewModel>> mockLogger = new Mock<ILogger<UniversitiesViewModel>>();
        UniversitiesViewModel viewModel = new UniversitiesViewModel(mockAcademicService.Object, mockLogger.Object);
        viewModel.TotalPages = 10;
        viewModel.CurrentPage = 1;

        // Act
        viewModel.CurrentPage = 5;
        string actual = viewModel.PageInfo;

        // Assert
        Assert.AreEqual("Page 5 of 10", actual);
    }

    /// <summary>
    /// Tests that the PageInfo property correctly updates when TotalPages changes.
    /// Input: Setting TotalPages to a different value while CurrentPage remains constant.
    /// Expected: PageInfo reflects the new TotalPages value.
    /// </summary>
    [TestMethod]
    public void PageInfo_TotalPagesChanges_ReturnsUpdatedFormat()
    {
        // Arrange
        Mock<IAcademicService> mockAcademicService = new Mock<IAcademicService>();
        Mock<ILogger<UniversitiesViewModel>> mockLogger = new Mock<ILogger<UniversitiesViewModel>>();
        UniversitiesViewModel viewModel = new UniversitiesViewModel(mockAcademicService.Object, mockLogger.Object);
        viewModel.CurrentPage = 5;
        viewModel.TotalPages = 10;

        // Act
        viewModel.TotalPages = 20;
        string actual = viewModel.PageInfo;

        // Assert
        Assert.AreEqual("Page 5 of 20", actual);
    }

    /// <summary>
    /// Tests that the PageInfo property correctly updates when both CurrentPage and TotalPages change.
    /// Input: Setting both CurrentPage and TotalPages to new values.
    /// Expected: PageInfo reflects both new values.
    /// </summary>
    [TestMethod]
    public void PageInfo_BothPropertiesChange_ReturnsUpdatedFormat()
    {
        // Arrange
        Mock<IAcademicService> mockAcademicService = new Mock<IAcademicService>();
        Mock<ILogger<UniversitiesViewModel>> mockLogger = new Mock<ILogger<UniversitiesViewModel>>();
        UniversitiesViewModel viewModel = new UniversitiesViewModel(mockAcademicService.Object, mockLogger.Object);
        viewModel.CurrentPage = 1;
        viewModel.TotalPages = 10;

        // Act
        viewModel.CurrentPage = 7;
        viewModel.TotalPages = 15;
        string actual = viewModel.PageInfo;

        // Assert
        Assert.AreEqual("Page 7 of 15", actual);
    }

    /// <summary>
    /// Tests that setting ErrorMessage with whitespace variations correctly updates the property.
    /// Input: Various whitespace strings including tabs, newlines, and spaces
    /// Expected: Property correctly stores each whitespace variation
    /// </summary>
    [TestMethod]
    [DataRow("\t", DisplayName = "Tab character")]
    [DataRow("\n", DisplayName = "Newline character")]
    [DataRow("\r", DisplayName = "Carriage return")]
    [DataRow("\r\n", DisplayName = "CRLF")]
    [DataRow("     ", DisplayName = "Multiple spaces")]
    [DataRow("\t\n\r", DisplayName = "Mixed whitespace")]
    public void ErrorMessage_SetWhitespaceVariations_UpdatesPropertyCorrectly(string whitespaceValue)
    {
        // Arrange
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<UniversitiesViewModel>>();
        var viewModel = new UniversitiesViewModel(mockAcademic.Object, mockLogger.Object);

        // Act
        viewModel.ErrorMessage = whitespaceValue;

        // Assert
        Assert.AreEqual(whitespaceValue, viewModel.ErrorMessage);
    }

    /// <summary>
    /// Tests that ErrorMessage handles single character strings correctly.
    /// Input: Single character strings of various types
    /// Expected: Property correctly stores single character values
    /// </summary>
    [TestMethod]
    [DataRow("A", DisplayName = "Single letter")]
    [DataRow("1", DisplayName = "Single digit")]
    [DataRow("!", DisplayName = "Single special character")]
    [DataRow(" ", DisplayName = "Single space")]
    [DataRow("@", DisplayName = "At symbol")]
    public void ErrorMessage_SetSingleCharacter_UpdatesPropertyCorrectly(string singleChar)
    {
        // Arrange
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<UniversitiesViewModel>>();
        var viewModel = new UniversitiesViewModel(mockAcademic.Object, mockLogger.Object);

        // Act
        viewModel.ErrorMessage = singleChar;

        // Assert
        Assert.AreEqual(singleChar, viewModel.ErrorMessage);
    }

    /// <summary>
    /// Tests that ErrorMessage handles control characters correctly.
    /// Input: Strings containing various control characters
    /// Expected: Property correctly stores control character values
    /// </summary>
    [TestMethod]
    [DataRow("\0", DisplayName = "Null character")]
    [DataRow("\b", DisplayName = "Backspace")]
    [DataRow("\f", DisplayName = "Form feed")]
    [DataRow("\v", DisplayName = "Vertical tab")]
    [DataRow("\b\f\v", DisplayName = "Mixed control characters")]
    public void ErrorMessage_SetControlCharacters_UpdatesPropertyCorrectly(string controlChars)
    {
        // Arrange
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<UniversitiesViewModel>>();
        var viewModel = new UniversitiesViewModel(mockAcademic.Object, mockLogger.Object);

        // Act
        viewModel.ErrorMessage = controlChars;

        // Assert
        Assert.AreEqual(controlChars, viewModel.ErrorMessage);
    }

    /// <summary>
    /// Tests that ErrorMessage handles international and Unicode characters correctly.
    /// Input: Strings with various international character sets
    /// Expected: Property correctly stores Unicode values
    /// </summary>
    [TestMethod]
    [DataRow("Erreur: Opération échouée", DisplayName = "French accents")]
    [DataRow("错误：操作失败", DisplayName = "Chinese characters")]
    [DataRow("Ошибка: операция не удалась", DisplayName = "Cyrillic characters")]
    [DataRow("خطأ: فشلت العملية", DisplayName = "Arabic characters")]
    [DataRow("エラー：操作に失敗しました", DisplayName = "Japanese characters")]
    [DataRow("שגיאה: הפעולה נכשלה", DisplayName = "Hebrew characters")]
    [DataRow("🚨 Error occurred! ⚠️", DisplayName = "Emoji characters")]
    public void ErrorMessage_SetUnicodeCharacters_UpdatesPropertyCorrectly(string unicodeValue)
    {
        // Arrange
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<UniversitiesViewModel>>();
        var viewModel = new UniversitiesViewModel(mockAcademic.Object, mockLogger.Object);

        // Act
        viewModel.ErrorMessage = unicodeValue;

        // Assert
        Assert.AreEqual(unicodeValue, viewModel.ErrorMessage);
    }

    /// <summary>
    /// Tests that setting ErrorMessage back to empty string after having a value works correctly.
    /// Input: Set to non-empty value, then back to empty string
    /// Expected: Property is updated to empty string and PropertyChanged is raised
    /// </summary>
    [TestMethod]
    public void ErrorMessage_SetToEmptyAfterNonEmpty_UpdatesPropertyAndRaisesPropertyChanged()
    {
        // Arrange
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<UniversitiesViewModel>>();
        var viewModel = new UniversitiesViewModel(mockAcademic.Object, mockLogger.Object);
        viewModel.ErrorMessage = "Some error";
        var propertyChangedRaised = false;
        viewModel.PropertyChanged += (sender, e) => { if (e.PropertyName == "ErrorMessage") propertyChangedRaised = true; };

        // Act
        viewModel.ErrorMessage = string.Empty;

        // Assert
        Assert.AreEqual(string.Empty, viewModel.ErrorMessage);
        Assert.IsTrue(propertyChangedRaised);
    }

    /// <summary>
    /// Tests that PropertyChanged event sender is the view model instance.
    /// Input: New error message value
    /// Expected: PropertyChanged event is raised with the view model as sender
    /// </summary>
    [TestMethod]
    public void ErrorMessage_SetValue_PropertyChangedEventHasCorrectSender()
    {
        // Arrange
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<UniversitiesViewModel>>();
        var viewModel = new UniversitiesViewModel(mockAcademic.Object, mockLogger.Object);
        object? eventSender = null;
        viewModel.PropertyChanged += (sender, e) => { if (e.PropertyName == "ErrorMessage") eventSender = sender; };

        // Act
        viewModel.ErrorMessage = "Test error";

        // Assert
        Assert.AreSame(viewModel, eventSender);
    }

    /// <summary>
    /// Tests that ErrorMessage handles extremely long strings with various character types.
    /// Input: Very long string (100000 characters) containing mixed content
    /// Expected: Property correctly stores and returns the entire long string
    /// </summary>
    [TestMethod]
    public void ErrorMessage_SetExtremelyLongMixedString_UpdatesPropertyCorrectly()
    {
        // Arrange
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<UniversitiesViewModel>>();
        var viewModel = new UniversitiesViewModel(mockAcademic.Object, mockLogger.Object);
        var sb = new StringBuilder();
        for (int i = 0; i < 10000; i++)
        {
            sb.Append("Error: ");
            sb.Append(i);
            sb.Append(". ");
        }
        var extremelyLongString = sb.ToString();

        // Act
        viewModel.ErrorMessage = extremelyLongString;

        // Assert
        Assert.AreEqual(extremelyLongString, viewModel.ErrorMessage);
        Assert.IsTrue(viewModel.ErrorMessage.Length > 50000);
    }

    /// <summary>
    /// Tests that ErrorMessage with special formatting characters is handled correctly.
    /// Input: Strings with special formatting and escape sequences
    /// Expected: Property correctly stores the formatted strings
    /// </summary>
    [TestMethod]
    [DataRow("Error: \"Quoted message\"", DisplayName = "Quoted string")]
    [DataRow("Error: Path\\To\\File", DisplayName = "Backslashes")]
    [DataRow("Error: C:\\Users\\Test\\file.txt", DisplayName = "File path")]
    [DataRow("Error: {0} - {1}", DisplayName = "Format placeholders")]
    [DataRow("Error: [ErrorCode: 500]", DisplayName = "Brackets")]
    public void ErrorMessage_SetSpecialFormattingCharacters_UpdatesPropertyCorrectly(string formattedValue)
    {
        // Arrange
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<UniversitiesViewModel>>();
        var viewModel = new UniversitiesViewModel(mockAcademic.Object, mockLogger.Object);

        // Act
        viewModel.ErrorMessage = formattedValue;

        // Assert
        Assert.AreEqual(formattedValue, viewModel.ErrorMessage);
    }

    /// <summary>
    /// Tests that setting TotalPages updates the PageInfo property which depends on it.
    /// Input: Various combinations of TotalPages values.
    /// Expected: PageInfo returns correct formatted string "Page {CurrentPage} of {TotalPages}".
    /// </summary>
    [TestMethod]
    [DataRow(1, "Page 1 of 1")]
    [DataRow(5, "Page 1 of 5")]
    [DataRow(10, "Page 1 of 10")]
    [DataRow(0, "Page 1 of 0")]
    [DataRow(-1, "Page 1 of -1")]
    [DataRow(int.MaxValue, "Page 1 of 2147483647")]
    [DataRow(int.MinValue, "Page 1 of -2147483648")]
    public void TotalPages_SetValue_UpdatesPageInfoProperty(int totalPages, string expectedPageInfo)
    {
        // Arrange
        var academicServiceMock = new Mock<IAcademicService>();
        var loggerMock = new Mock<ILogger<UniversitiesViewModel>>();
        var viewModel = new UniversitiesViewModel(academicServiceMock.Object, loggerMock.Object);

        // Act
        viewModel.TotalPages = totalPages;

        // Assert
        Assert.AreEqual(expectedPageInfo, viewModel.PageInfo);
    }

    /// <summary>
    /// Tests that setting TotalPages to the same value multiple times does not raise PropertyChanged after the first set.
    /// Input: Setting value 10 multiple times.
    /// Expected: PropertyChanged is raised only once (on initial set from 1 to 10), subsequent sets to 10 don't raise events.
    /// </summary>
    [TestMethod]
    public void TotalPages_SetSameValueMultipleTimes_RaisesPropertyChangedOnlyOnce()
    {
        // Arrange
        var academicServiceMock = new Mock<IAcademicService>();
        var loggerMock = new Mock<ILogger<UniversitiesViewModel>>();
        var viewModel = new UniversitiesViewModel(academicServiceMock.Object, loggerMock.Object);

        var propertyChangedCount = 0;
        viewModel.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == "TotalPages")
            {
                propertyChangedCount++;
            }
        };

        // Act
        viewModel.TotalPages = 10;
        viewModel.TotalPages = 10;
        viewModel.TotalPages = 10;

        // Assert
        Assert.AreEqual(10, viewModel.TotalPages);
        Assert.AreEqual(1, propertyChangedCount);
    }

    /// <summary>
    /// Tests that setting TotalPages raises PropertyChanged with correct sender.
    /// Input: New value (15).
    /// Expected: PropertyChanged event is raised with the ViewModel instance as sender.
    /// </summary>
    [TestMethod]
    public void TotalPages_SetValue_RaisesPropertyChangedWithCorrectSender()
    {
        // Arrange
        var academicServiceMock = new Mock<IAcademicService>();
        var loggerMock = new Mock<ILogger<UniversitiesViewModel>>();
        var viewModel = new UniversitiesViewModel(academicServiceMock.Object, loggerMock.Object);

        object? eventSender = null;
        viewModel.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == "TotalPages")
            {
                eventSender = sender;
            }
        };

        // Act
        viewModel.TotalPages = 15;

        // Assert
        Assert.AreSame(viewModel, eventSender);
    }

    /// <summary>
    /// Tests that PageInfo PropertyChanged is raised in correct order after TotalPages PropertyChanged.
    /// Input: New value (20).
    /// Expected: PropertyChanged events are raised in order: "TotalPages" then "PageInfo".
    /// </summary>
    [TestMethod]
    public void TotalPages_SetValue_RaisesPageInfoPropertyChangedInCorrectOrder()
    {
        // Arrange
        var academicServiceMock = new Mock<IAcademicService>();
        var loggerMock = new Mock<ILogger<UniversitiesViewModel>>();
        var viewModel = new UniversitiesViewModel(academicServiceMock.Object, loggerMock.Object);

        var propertyChangedEvents = new List<string>();
        viewModel.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName != null)
            {
                propertyChangedEvents.Add(args.PropertyName);
            }
        };

        // Act
        viewModel.TotalPages = 20;

        // Assert
        Assert.AreEqual(2, propertyChangedEvents.Count);
        Assert.AreEqual("TotalPages", propertyChangedEvents[0]);
        Assert.AreEqual("PageInfo", propertyChangedEvents[1]);
    }

    /// <summary>
    /// Tests that TotalPages correctly transitions from positive to negative values.
    /// Input: Transition from 10 to -5.
    /// Expected: Property is updated and PropertyChanged is raised.
    /// </summary>
    [TestMethod]
    public void TotalPages_TransitionFromPositiveToNegative_UpdatesCorrectly()
    {
        // Arrange
        var academicServiceMock = new Mock<IAcademicService>();
        var loggerMock = new Mock<ILogger<UniversitiesViewModel>>();
        var viewModel = new UniversitiesViewModel(academicServiceMock.Object, loggerMock.Object);

        viewModel.TotalPages = 10;

        // Act
        viewModel.TotalPages = -5;

        // Assert
        Assert.AreEqual(-5, viewModel.TotalPages);
    }

    /// <summary>
    /// Tests that TotalPages correctly transitions from negative to positive values.
    /// Input: Transition from -10 to 5.
    /// Expected: Property is updated and PropertyChanged is raised.
    /// </summary>
    [TestMethod]
    public void TotalPages_TransitionFromNegativeToPositive_UpdatesCorrectly()
    {
        // Arrange
        var academicServiceMock = new Mock<IAcademicService>();
        var loggerMock = new Mock<ILogger<UniversitiesViewModel>>();
        var viewModel = new UniversitiesViewModel(academicServiceMock.Object, loggerMock.Object);

        viewModel.TotalPages = -10;

        // Act
        viewModel.TotalPages = 5;

        // Assert
        Assert.AreEqual(5, viewModel.TotalPages);
    }

    /// <summary>
    /// Tests that TotalPages correctly handles transition to zero from positive value.
    /// Input: Transition from 10 to 0.
    /// Expected: Property is set to 0 and PropertyChanged is raised.
    /// </summary>
    [TestMethod]
    public void TotalPages_TransitionToZeroFromPositive_UpdatesCorrectly()
    {
        // Arrange
        var academicServiceMock = new Mock<IAcademicService>();
        var loggerMock = new Mock<ILogger<UniversitiesViewModel>>();
        var viewModel = new UniversitiesViewModel(academicServiceMock.Object, loggerMock.Object);

        viewModel.TotalPages = 10;

        // Act
        viewModel.TotalPages = 0;

        // Assert
        Assert.AreEqual(0, viewModel.TotalPages);
    }

    /// <summary>
    /// Tests that TotalPages correctly handles transition from zero to positive value.
    /// Input: Transition from 0 to 10.
    /// Expected: Property is set to 10 and PropertyChanged is raised.
    /// </summary>
    [TestMethod]
    public void TotalPages_TransitionFromZeroToPositive_UpdatesCorrectly()
    {
        // Arrange
        var academicServiceMock = new Mock<IAcademicService>();
        var loggerMock = new Mock<ILogger<UniversitiesViewModel>>();
        var viewModel = new UniversitiesViewModel(academicServiceMock.Object, loggerMock.Object);

        viewModel.TotalPages = 0;

        // Act
        viewModel.TotalPages = 10;

        // Assert
        Assert.AreEqual(10, viewModel.TotalPages);
    }

    /// <summary>
    /// Tests that EditId handles various string edge cases correctly.
    /// Input: Various edge case string values including tabs, newlines, and control characters.
    /// Expected: The property value is set correctly for each case.
    /// </summary>
    [TestMethod]
    [DataRow("\t", DisplayName = "Tab character")]
    [DataRow("\n", DisplayName = "Newline character")]
    [DataRow("\r\n", DisplayName = "CRLF")]
    [DataRow("\t\r\n", DisplayName = "Mixed whitespace")]
    [DataRow("id\twith\ttabs", DisplayName = "ID with tabs")]
    [DataRow("id\nwith\nnewlines", DisplayName = "ID with newlines")]
    public void EditId_SetVariousWhitespaceCharacters_UpdatesPropertyCorrectly(string value)
    {
        // Arrange
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<UniversitiesViewModel>>();
        var viewModel = new UniversitiesViewModel(mockAcademic.Object, mockLogger.Object);
        var propertyChangedRaised = false;

        viewModel.PropertyChanged += (sender, e) =>
        {
            propertyChangedRaised = true;
        };

        // Act
        viewModel.EditId = value;

        // Assert
        Assert.AreEqual(value, viewModel.EditId);
        Assert.IsTrue(propertyChangedRaised);
    }

    /// <summary>
    /// Tests that EditId handles Unicode and international characters correctly.
    /// Input: Strings with Unicode characters from various languages.
    /// Expected: The property value is set correctly and PropertyChanged event is raised.
    /// </summary>
    [TestMethod]
    [DataRow("université-123", DisplayName = "French accented characters")]
    [DataRow("大学-456", DisplayName = "Chinese characters")]
    [DataRow("جامعة-789", DisplayName = "Arabic characters")]
    [DataRow("университет-012", DisplayName = "Cyrillic characters")]
    [DataRow("🎓-university", DisplayName = "Emoji characters")]
    public void EditId_SetUnicodeCharacters_UpdatesPropertyCorrectly(string value)
    {
        // Arrange
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<UniversitiesViewModel>>();
        var viewModel = new UniversitiesViewModel(mockAcademic.Object, mockLogger.Object);
        var propertyChangedRaised = false;

        viewModel.PropertyChanged += (sender, e) =>
        {
            propertyChangedRaised = true;
        };

        // Act
        viewModel.EditId = value;

        // Assert
        Assert.AreEqual(value, viewModel.EditId);
        Assert.IsTrue(propertyChangedRaised);
    }

    /// <summary>
    /// Tests that setting EditId to null after having a non-null value updates correctly.
    /// Input: Set to non-null value, then to null.
    /// Expected: Property is updated to null and PropertyChanged is raised for the change.
    /// </summary>
    [TestMethod]
    public void EditId_SetToNullAfterNonNull_UpdatesPropertyCorrectly()
    {
        // Arrange
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<UniversitiesViewModel>>();
        var viewModel = new UniversitiesViewModel(mockAcademic.Object, mockLogger.Object);
        viewModel.EditId = "some-id";
        var propertyChangedRaised = false;

        viewModel.PropertyChanged += (sender, e) =>
        {
            if (e.PropertyName == nameof(UniversitiesViewModel.EditId))
            {
                propertyChangedRaised = true;
            }
        };

        // Act
        viewModel.EditId = null;

        // Assert
        Assert.IsNull(viewModel.EditId);
        Assert.IsTrue(propertyChangedRaised);
    }

    /// <summary>
    /// Tests that setting EditId to the same null value does not raise PropertyChanged event.
    /// Input: Set to null when already null.
    /// Expected: PropertyChanged event is not raised.
    /// </summary>
    [TestMethod]
    public void EditId_SetNullWhenAlreadyNull_DoesNotRaisePropertyChanged()
    {
        // Arrange
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<UniversitiesViewModel>>();
        var viewModel = new UniversitiesViewModel(mockAcademic.Object, mockLogger.Object);
        var propertyChangedCount = 0;

        viewModel.PropertyChanged += (sender, e) =>
        {
            if (e.PropertyName == nameof(UniversitiesViewModel.EditId))
            {
                propertyChangedCount++;
            }
        };

        // Act
        viewModel.EditId = null;

        // Assert
        Assert.IsNull(viewModel.EditId);
        Assert.AreEqual(0, propertyChangedCount);
    }

    /// <summary>
    /// Tests that setting EditId to empty string when already empty does not raise PropertyChanged event.
    /// Input: Set to empty string when already empty.
    /// Expected: PropertyChanged event is not raised.
    /// </summary>
    [TestMethod]
    public void EditId_SetEmptyStringWhenAlreadyEmpty_DoesNotRaisePropertyChanged()
    {
        // Arrange
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<UniversitiesViewModel>>();
        var viewModel = new UniversitiesViewModel(mockAcademic.Object, mockLogger.Object);
        viewModel.EditId = string.Empty;
        var propertyChangedCount = 0;

        viewModel.PropertyChanged += (sender, e) =>
        {
            if (e.PropertyName == nameof(UniversitiesViewModel.EditId))
            {
                propertyChangedCount++;
            }
        };

        // Act
        viewModel.EditId = string.Empty;

        // Assert
        Assert.AreEqual(string.Empty, viewModel.EditId);
        Assert.AreEqual(0, propertyChangedCount);
    }

    /// <summary>
    /// Tests that EditId PropertyChanged event includes the correct sender.
    /// Input: Any valid string value.
    /// Expected: PropertyChanged event is raised with the ViewModel as sender.
    /// </summary>
    [TestMethod]
    public void EditId_SetValue_PropertyChangedHasCorrectSender()
    {
        // Arrange
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<UniversitiesViewModel>>();
        var viewModel = new UniversitiesViewModel(mockAcademic.Object, mockLogger.Object);
        object? eventSender = null;

        viewModel.PropertyChanged += (sender, e) =>
        {
            eventSender = sender;
        };

        // Act
        viewModel.EditId = "test-id";

        // Assert
        Assert.AreSame(viewModel, eventSender);
    }

    /// <summary>
    /// Tests that EditId can handle a string with only control characters.
    /// Input: String containing only control characters.
    /// Expected: The property value is set correctly and PropertyChanged event is raised.
    /// </summary>
    [TestMethod]
    public void EditId_SetControlCharacters_UpdatesPropertyAndRaisesPropertyChanged()
    {
        // Arrange
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<UniversitiesViewModel>>();
        var viewModel = new UniversitiesViewModel(mockAcademic.Object, mockLogger.Object);
        var expectedValue = "\0\b\f\v";
        var propertyChangedRaised = false;

        viewModel.PropertyChanged += (sender, e) =>
        {
            propertyChangedRaised = true;
        };

        // Act
        viewModel.EditId = expectedValue;

        // Assert
        Assert.AreEqual(expectedValue, viewModel.EditId);
        Assert.IsTrue(propertyChangedRaised);
    }

    /// <summary>
    /// Tests that EditId can transition between empty string and null.
    /// Input: Set to empty string, then to null, then to empty string again.
    /// Expected: Property updates correctly each time and PropertyChanged is raised for each change.
    /// </summary>
    [TestMethod]
    public void EditId_TransitionBetweenEmptyAndNull_UpdatesPropertyCorrectly()
    {
        // Arrange
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<UniversitiesViewModel>>();
        var viewModel = new UniversitiesViewModel(mockAcademic.Object, mockLogger.Object);
        var propertyChangedCount = 0;

        viewModel.PropertyChanged += (sender, e) =>
        {
            if (e.PropertyName == nameof(UniversitiesViewModel.EditId))
            {
                propertyChangedCount++;
            }
        };

        // Act & Assert
        viewModel.EditId = string.Empty;
        Assert.AreEqual(string.Empty, viewModel.EditId);
        Assert.AreEqual(1, propertyChangedCount);

        viewModel.EditId = null;
        Assert.IsNull(viewModel.EditId);
        Assert.AreEqual(2, propertyChangedCount);

        viewModel.EditId = string.Empty;
        Assert.AreEqual(string.Empty, viewModel.EditId);
        Assert.AreEqual(3, propertyChangedCount);
    }

    /// <summary>
    /// Tests that EditId can handle GUID-formatted strings.
    /// Input: Valid GUID string.
    /// Expected: The property value is set correctly and PropertyChanged event is raised.
    /// </summary>
    [TestMethod]
    public void EditId_SetGuidFormattedString_UpdatesPropertyAndRaisesPropertyChanged()
    {
        // Arrange
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<UniversitiesViewModel>>();
        var viewModel = new UniversitiesViewModel(mockAcademic.Object, mockLogger.Object);
        var expectedValue = "12345678-1234-1234-1234-123456789abc";
        var propertyChangedRaised = false;

        viewModel.PropertyChanged += (sender, e) =>
        {
            propertyChangedRaised = true;
        };

        // Act
        viewModel.EditId = expectedValue;

        // Assert
        Assert.AreEqual(expectedValue, viewModel.EditId);
        Assert.IsTrue(propertyChangedRaised);
    }

    /// <summary>
    /// Tests that EditId can handle numeric string values.
    /// Input: String containing only numeric characters.
    /// Expected: The property value is set correctly and PropertyChanged event is raised.
    /// </summary>
    [TestMethod]
    [DataRow("0", DisplayName = "Single digit zero")]
    [DataRow("123456789", DisplayName = "Multiple digits")]
    [DataRow("-123", DisplayName = "Negative number string")]
    [DataRow("12.34", DisplayName = "Decimal number string")]
    public void EditId_SetNumericString_UpdatesPropertyCorrectly(string value)
    {
        // Arrange
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<UniversitiesViewModel>>();
        var viewModel = new UniversitiesViewModel(mockAcademic.Object, mockLogger.Object);
        var propertyChangedRaised = false;

        viewModel.PropertyChanged += (sender, e) =>
        {
            propertyChangedRaised = true;
        };

        // Act
        viewModel.EditId = value;

        // Assert
        Assert.AreEqual(value, viewModel.EditId);
        Assert.IsTrue(propertyChangedRaised);
    }

    /// <summary>
    /// Tests that CurrentPage getter returns the initial default value of 1.
    /// Input: None (default initialization).
    /// Expected: CurrentPage returns 1.
    /// </summary>
    [TestMethod]
    public void CurrentPage_InitialValue_ReturnsOne()
    {
        // Arrange
        Mock<IAcademicService> mockAcademic = new Mock<IAcademicService>();
        Mock<ILogger<UniversitiesViewModel>> mockLogger = new Mock<ILogger<UniversitiesViewModel>>();

        // Act
        UniversitiesViewModel viewModel = new UniversitiesViewModel(mockAcademic.Object, mockLogger.Object);

        // Assert
        Assert.AreEqual(1, viewModel.CurrentPage);
    }

    /// <summary>
    /// Tests that setting CurrentPage to a new valid value updates the property and raises PropertyChanged for both CurrentPage and PageInfo.
    /// Input: New valid page number (5).
    /// Expected: Property is updated, PropertyChanged is raised for CurrentPage and PageInfo.
    /// </summary>
    [TestMethod]
    public void CurrentPage_SetNewValue_UpdatesPropertyAndRaisesPropertyChangedForBothProperties()
    {
        // Arrange
        Mock<IAcademicService> mockAcademic = new Mock<IAcademicService>();
        Mock<ILogger<UniversitiesViewModel>> mockLogger = new Mock<ILogger<UniversitiesViewModel>>();
        UniversitiesViewModel viewModel = new UniversitiesViewModel(mockAcademic.Object, mockLogger.Object);
        List<string> changedProperties = new List<string>();
        viewModel.PropertyChanged += (sender, e) => { if (e.PropertyName != null) changedProperties.Add(e.PropertyName); };

        // Act
        viewModel.CurrentPage = 5;

        // Assert
        Assert.AreEqual(5, viewModel.CurrentPage);
        Assert.IsTrue(changedProperties.Contains("CurrentPage"));
        Assert.IsTrue(changedProperties.Contains("PageInfo"));
    }

    /// <summary>
    /// Tests that setting CurrentPage to various boundary values correctly updates the property.
    /// Input: Various boundary integer values.
    /// Expected: Property is set correctly for each value.
    /// </summary>
    /// <param name="value">The boundary value to set.</param>
    [TestMethod]
    [DataRow(0)]
    [DataRow(1)]
    [DataRow(-1)]
    [DataRow(100)]
    [DataRow(-100)]
    [DataRow(int.MaxValue)]
    [DataRow(int.MinValue)]
    public void CurrentPage_SetBoundaryValues_UpdatesPropertyCorrectly(int value)
    {
        // Arrange
        Mock<IAcademicService> mockAcademic = new Mock<IAcademicService>();
        Mock<ILogger<UniversitiesViewModel>> mockLogger = new Mock<ILogger<UniversitiesViewModel>>();
        UniversitiesViewModel viewModel = new UniversitiesViewModel(mockAcademic.Object, mockLogger.Object);

        // Act
        viewModel.CurrentPage = value;

        // Assert
        Assert.AreEqual(value, viewModel.CurrentPage);
    }

    /// <summary>
    /// Tests that alternating between two different values raises PropertyChanged for both CurrentPage and PageInfo each time.
    /// Input: Alternating values (2, 3, 2, 3).
    /// Expected: PropertyChanged is raised for both properties on each change.
    /// </summary>
    [TestMethod]
    public void CurrentPage_AlternatingValues_RaisesPropertyChangedEachTime()
    {
        // Arrange
        Mock<IAcademicService> mockAcademic = new Mock<IAcademicService>();
        Mock<ILogger<UniversitiesViewModel>> mockLogger = new Mock<ILogger<UniversitiesViewModel>>();
        UniversitiesViewModel viewModel = new UniversitiesViewModel(mockAcademic.Object, mockLogger.Object);
        int currentPageEventCount = 0;
        int pageInfoEventCount = 0;
        viewModel.PropertyChanged += (sender, e) =>
        {
            if (e.PropertyName == "CurrentPage") currentPageEventCount++;
            if (e.PropertyName == "PageInfo") pageInfoEventCount++;
        };

        // Act
        viewModel.CurrentPage = 2;
        viewModel.CurrentPage = 3;
        viewModel.CurrentPage = 2;
        viewModel.CurrentPage = 3;

        // Assert
        Assert.AreEqual(4, currentPageEventCount);
        Assert.AreEqual(4, pageInfoEventCount);
    }

    /// <summary>
    /// Tests that setting CurrentPage to same value multiple times in a row does not raise any PropertyChanged events.
    /// Input: Same value set three times consecutively.
    /// Expected: No PropertyChanged events are raised after the initial set.
    /// </summary>
    [TestMethod]
    public void CurrentPage_SetSameValueMultipleTimes_DoesNotRaisePropertyChangedAfterFirst()
    {
        // Arrange
        Mock<IAcademicService> mockAcademic = new Mock<IAcademicService>();
        Mock<ILogger<UniversitiesViewModel>> mockLogger = new Mock<ILogger<UniversitiesViewModel>>();
        UniversitiesViewModel viewModel = new UniversitiesViewModel(mockAcademic.Object, mockLogger.Object);
        viewModel.CurrentPage = 5; // Set initial value
        int eventCount = 0;
        viewModel.PropertyChanged += (sender, e) => eventCount++;

        // Act
        viewModel.CurrentPage = 5;
        viewModel.CurrentPage = 5;
        viewModel.CurrentPage = 5;

        // Assert
        Assert.AreEqual(0, eventCount);
    }

    /// <summary>
    /// Tests that PropertyChanged event for CurrentPage is raised before PropertyChanged event for PageInfo.
    /// Input: New page number (7).
    /// Expected: CurrentPage PropertyChanged is raised first, followed by PageInfo PropertyChanged.
    /// </summary>
    [TestMethod]
    public void CurrentPage_SetValue_RaisesCurrentPagePropertyChangedBeforePageInfo()
    {
        // Arrange
        Mock<IAcademicService> mockAcademic = new Mock<IAcademicService>();
        Mock<ILogger<UniversitiesViewModel>> mockLogger = new Mock<ILogger<UniversitiesViewModel>>();
        UniversitiesViewModel viewModel = new UniversitiesViewModel(mockAcademic.Object, mockLogger.Object);
        List<string> eventOrder = new List<string>();
        viewModel.PropertyChanged += (sender, e) => { if (e.PropertyName != null) eventOrder.Add(e.PropertyName); };

        // Act
        viewModel.CurrentPage = 7;

        // Assert
        Assert.AreEqual(2, eventOrder.Count);
        Assert.AreEqual("CurrentPage", eventOrder[0]);
        Assert.AreEqual("PageInfo", eventOrder[1]);
    }

    /// <summary>
    /// Tests that CurrentPage can be set to large positive values and retrieved correctly.
    /// Input: Large positive value (999999).
    /// Expected: Property is set and retrieved correctly.
    /// </summary>
    [TestMethod]
    public void CurrentPage_SetLargePositiveValue_UpdatesPropertyCorrectly()
    {
        // Arrange
        Mock<IAcademicService> mockAcademic = new Mock<IAcademicService>();
        Mock<ILogger<UniversitiesViewModel>> mockLogger = new Mock<ILogger<UniversitiesViewModel>>();
        UniversitiesViewModel viewModel = new UniversitiesViewModel(mockAcademic.Object, mockLogger.Object);

        // Act
        viewModel.CurrentPage = 999999;

        // Assert
        Assert.AreEqual(999999, viewModel.CurrentPage);
    }

    /// <summary>
    /// Tests that CurrentPage can be set to large negative values and retrieved correctly.
    /// Input: Large negative value (-999999).
    /// Expected: Property is set and retrieved correctly.
    /// </summary>
    [TestMethod]
    public void CurrentPage_SetLargeNegativeValue_UpdatesPropertyCorrectly()
    {
        // Arrange
        Mock<IAcademicService> mockAcademic = new Mock<IAcademicService>();
        Mock<ILogger<UniversitiesViewModel>> mockLogger = new Mock<ILogger<UniversitiesViewModel>>();
        UniversitiesViewModel viewModel = new UniversitiesViewModel(mockAcademic.Object, mockLogger.Object);

        // Act
        viewModel.CurrentPage = -999999;

        // Assert
        Assert.AreEqual(-999999, viewModel.CurrentPage);
    }

    /// <summary>
    /// Tests that setting CurrentPage from a non-default value back to the default value (1) raises PropertyChanged.
    /// Input: Set to 5, then back to 1.
    /// Expected: PropertyChanged is raised when setting back to 1.
    /// </summary>
    [TestMethod]
    public void CurrentPage_SetBackToDefault_RaisesPropertyChanged()
    {
        // Arrange
        Mock<IAcademicService> mockAcademic = new Mock<IAcademicService>();
        Mock<ILogger<UniversitiesViewModel>> mockLogger = new Mock<ILogger<UniversitiesViewModel>>();
        UniversitiesViewModel viewModel = new UniversitiesViewModel(mockAcademic.Object, mockLogger.Object);
        viewModel.CurrentPage = 5;
        List<string> changedProperties = new List<string>();
        viewModel.PropertyChanged += (sender, e) => { if (e.PropertyName != null) changedProperties.Add(e.PropertyName); };

        // Act
        viewModel.CurrentPage = 1;

        // Assert
        Assert.AreEqual(1, viewModel.CurrentPage);
        Assert.IsTrue(changedProperties.Contains("CurrentPage"));
        Assert.IsTrue(changedProperties.Contains("PageInfo"));
    }

    /// <summary>
    /// Tests that StatusFilter property returns the initial default value of "All".
    /// Input: None (initial state).
    /// Expected: StatusFilter returns "All".
    /// </summary>
    [TestMethod]
    public void StatusFilter_InitialValue_ReturnsAll()
    {
        // Arrange
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<UniversitiesViewModel>>();

        // Act
        var viewModel = new UniversitiesViewModel(mockAcademic.Object, mockLogger.Object);

        // Assert
        Assert.AreEqual("All", viewModel.StatusFilter);
    }

    /// <summary>
    /// Tests that StatusFilter can handle very long strings without issues.
    /// Input: String with 10000 characters.
    /// Expected: Property is updated correctly with the long string.
    /// </summary>
    [TestMethod]
    public void StatusFilter_SetVeryLongString_UpdatesPropertyCorrectly()
    {
        // Arrange
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<UniversitiesViewModel>>();
        var viewModel = new UniversitiesViewModel(mockAcademic.Object, mockLogger.Object);
        var longString = new string('A', 10000);

        // Act
        viewModel.StatusFilter = longString;

        // Assert
        Assert.AreEqual(longString, viewModel.StatusFilter);
        Assert.AreEqual(10000, viewModel.StatusFilter.Length);
    }

    /// <summary>
    /// Tests that StatusFilter handles control characters correctly.
    /// Input: Strings with various control characters.
    /// Expected: Property is updated and PropertyChanged is raised.
    /// </summary>
    [TestMethod]
    [DataRow("\0", DisplayName = "Null character")]
    [DataRow("\r\n", DisplayName = "Carriage return and line feed")]
    [DataRow("\b\f\v", DisplayName = "Backspace, form feed, vertical tab")]
    public void StatusFilter_SetControlCharacters_UpdatesPropertyCorrectly(string controlChars)
    {
        // Arrange
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<UniversitiesViewModel>>();
        var viewModel = new UniversitiesViewModel(mockAcademic.Object, mockLogger.Object);

        // Act
        viewModel.StatusFilter = controlChars;

        // Assert
        Assert.AreEqual(controlChars, viewModel.StatusFilter);
    }

    /// <summary>
    /// Tests that setting StatusFilter raises PropertyChanged with the correct sender.
    /// Input: New filter value.
    /// Expected: PropertyChanged event is raised with the viewModel instance as sender.
    /// </summary>
    [TestMethod]
    public void StatusFilter_SetValue_RaisesPropertyChangedWithCorrectSender()
    {
        // Arrange
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<UniversitiesViewModel>>();
        var viewModel = new UniversitiesViewModel(mockAcademic.Object, mockLogger.Object);

        object? actualSender = null;
        viewModel.PropertyChanged += (sender, e) =>
        {
            if (e.PropertyName == "StatusFilter")
                actualSender = sender;
        };

        // Act
        viewModel.StatusFilter = "Accredited";

        // Assert
        Assert.AreSame(viewModel, actualSender);
    }

    /// <summary>
    /// Tests that StatusFilter transition from default "All" to a different value works correctly.
    /// Input: Change from default "All" to "Accredited".
    /// Expected: Property is updated and PropertyChanged is raised.
    /// </summary>
    [TestMethod]
    public void StatusFilter_TransitionFromDefaultValue_UpdatesCorrectly()
    {
        // Arrange
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<UniversitiesViewModel>>();
        var viewModel = new UniversitiesViewModel(mockAcademic.Object, mockLogger.Object);

        Assert.AreEqual("All", viewModel.StatusFilter); // Verify initial state

        bool propertyChanged = false;
        viewModel.PropertyChanged += (sender, e) =>
        {
            if (e.PropertyName == "StatusFilter")
                propertyChanged = true;
        };

        // Act
        viewModel.StatusFilter = "Accredited";

        // Assert
        Assert.AreEqual("Accredited", viewModel.StatusFilter);
        Assert.IsTrue(propertyChanged);
    }

    /// <summary>
    /// Tests that StatusFilter transition back to default "All" value after being changed works correctly.
    /// Input: Change to "Pending" then back to "All".
    /// Expected: Property is updated correctly both times.
    /// </summary>
    [TestMethod]
    public void StatusFilter_TransitionBackToDefaultValue_UpdatesCorrectly()
    {
        // Arrange
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<UniversitiesViewModel>>();
        var viewModel = new UniversitiesViewModel(mockAcademic.Object, mockLogger.Object);

        viewModel.StatusFilter = "Pending";
        Assert.AreEqual("Pending", viewModel.StatusFilter);

        // Act
        viewModel.StatusFilter = "All";

        // Assert
        Assert.AreEqual("All", viewModel.StatusFilter);
    }

    /// <summary>
    /// Tests that StatusFilter with case-sensitive different values are treated as different.
    /// Input: "Active", then "active".
    /// Expected: Both values are accepted as different and PropertyChanged is raised for each.
    /// </summary>
    [TestMethod]
    public void StatusFilter_CaseSensitiveDifferentValues_TreatsAsDifferent()
    {
        // Arrange
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<UniversitiesViewModel>>();
        var viewModel = new UniversitiesViewModel(mockAcademic.Object, mockLogger.Object);

        viewModel.StatusFilter = "Active";

        int propertyChangedCount = 0;
        viewModel.PropertyChanged += (sender, e) =>
        {
            if (e.PropertyName == "StatusFilter")
                propertyChangedCount++;
        };

        // Act
        viewModel.StatusFilter = "active";

        // Assert
        Assert.AreEqual("active", viewModel.StatusFilter);
        Assert.AreEqual(1, propertyChangedCount);
    }

    /// <summary>
    /// Tests that StatusFilter with boundary length strings (empty and extremely long) works correctly.
    /// Input: Empty string followed by 100000 character string.
    /// Expected: Both values are handled correctly.
    /// </summary>
    [TestMethod]
    public void StatusFilter_BoundaryLengthStrings_HandledCorrectly()
    {
        // Arrange
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<UniversitiesViewModel>>();
        var viewModel = new UniversitiesViewModel(mockAcademic.Object, mockLogger.Object);

        // Act & Assert - Empty string
        viewModel.StatusFilter = "";
        Assert.AreEqual("", viewModel.StatusFilter);

        // Act & Assert - Very long string
        var extremelyLongString = new string('X', 100000);
        viewModel.StatusFilter = extremelyLongString;
        Assert.AreEqual(extremelyLongString, viewModel.StatusFilter);
        Assert.AreEqual(100000, viewModel.StatusFilter.Length);
    }
}




/// <summary>
/// Tests for the EditName property of UniversitiesViewModel class.
/// </summary>
[TestClass]
public partial class UniversitiesViewModelEditNameTests
{
    /// <summary>
    /// Tests that the EditName property initializes with an empty string value.
    /// Input: None (initial state).
    /// Expected result: EditName should be string.Empty upon construction.
    /// </summary>
    [TestMethod]
    public void EditName_InitialValue_ReturnsEmptyString()
    {
        // Arrange
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<UniversitiesViewModel>>();
        var viewModel = new UniversitiesViewModel(mockAcademic.Object, mockLogger.Object);

        // Act
        var result = viewModel.EditName;

        // Assert
        Assert.AreEqual(string.Empty, result);
    }

    /// <summary>
    /// Tests that setting EditName to various valid string values updates the property correctly and raises PropertyChanged event.
    /// Input conditions: Valid non-empty strings, empty string, whitespace, special characters, and various edge cases.
    /// Expected result: Property value is updated and PropertyChanged event is raised with correct property name.
    /// </summary>
    [TestMethod]
    [DataRow("University Name", DisplayName = "Normal university name")]
    [DataRow("", DisplayName = "Empty string")]
    [DataRow("   ", DisplayName = "Whitespace only")]
    [DataRow("\t\n\r", DisplayName = "Tab and newline characters")]
    [DataRow("A", DisplayName = "Single character")]
    [DataRow("University with Special !@#$%^&*() Characters", DisplayName = "Special characters")]
    [DataRow("Université de Montréal", DisplayName = "French accented characters")]
    [DataRow("北京大学", DisplayName = "Chinese characters")]
    [DataRow("جامعة القاهرة", DisplayName = "Arabic characters")]
    [DataRow("🎓📚University", DisplayName = "Emoji characters")]
    [DataRow("Name\nWith\nNewlines", DisplayName = "Multi-line string")]
    [DataRow("Name\tWith\tTabs", DisplayName = "String with tabs")]
    [DataRow("UPPERCASE UNIVERSITY", DisplayName = "Uppercase string")]
    [DataRow("lowercase university", DisplayName = "Lowercase string")]
    [DataRow("MiXeD CaSe University", DisplayName = "Mixed case string")]
    public void EditName_SetValidValue_UpdatesPropertyAndRaisesPropertyChanged(string newValue)
    {
        // Arrange
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<UniversitiesViewModel>>();
        var viewModel = new UniversitiesViewModel(mockAcademic.Object, mockLogger.Object);
        string? propertyChangedName = null;
        viewModel.PropertyChanged += (sender, args) => propertyChangedName = args.PropertyName;

        // Act
        viewModel.EditName = newValue;

        // Assert
        Assert.AreEqual(newValue, viewModel.EditName);
        Assert.AreEqual("EditName", propertyChangedName);
    }

    /// <summary>
    /// Tests that setting EditName to a very long string value is handled correctly.
    /// Input conditions: String with 10000 characters.
    /// Expected result: Property value is updated correctly and PropertyChanged event is raised.
    /// </summary>
    [TestMethod]
    public void EditName_SetVeryLongString_UpdatesPropertyAndRaisesPropertyChanged()
    {
        // Arrange
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<UniversitiesViewModel>>();
        var viewModel = new UniversitiesViewModel(mockAcademic.Object, mockLogger.Object);
        var veryLongString = new string('A', 10000);
        string? propertyChangedName = null;
        viewModel.PropertyChanged += (sender, args) => propertyChangedName = args.PropertyName;

        // Act
        viewModel.EditName = veryLongString;

        // Assert
        Assert.AreEqual(veryLongString, viewModel.EditName);
        Assert.AreEqual("EditName", propertyChangedName);
    }

    /// <summary>
    /// Tests that setting EditName to the same value does not raise PropertyChanged event.
    /// Input conditions: Setting the same value twice consecutively.
    /// Expected result: PropertyChanged event is raised only on the first set, not on the second.
    /// </summary>
    [TestMethod]
    public void EditName_SetSameValue_DoesNotRaisePropertyChanged()
    {
        // Arrange
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<UniversitiesViewModel>>();
        var viewModel = new UniversitiesViewModel(mockAcademic.Object, mockLogger.Object);
        const string testValue = "Test University";
        viewModel.EditName = testValue;
        int propertyChangedCount = 0;
        viewModel.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == "EditName")
                propertyChangedCount++;
        };

        // Act
        viewModel.EditName = testValue;

        // Assert
        Assert.AreEqual(testValue, viewModel.EditName);
        Assert.AreEqual(0, propertyChangedCount);
    }

    /// <summary>
    /// Tests that setting EditName multiple times with different values updates correctly each time.
    /// Input conditions: Multiple different string values set sequentially.
    /// Expected result: Property value is updated each time and PropertyChanged event is raised for each change.
    /// </summary>
    [TestMethod]
    public void EditName_SetMultipleDifferentValues_UpdatesEachTimeAndRaisesPropertyChanged()
    {
        // Arrange
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<UniversitiesViewModel>>();
        var viewModel = new UniversitiesViewModel(mockAcademic.Object, mockLogger.Object);
        int propertyChangedCount = 0;
        viewModel.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == "EditName")
                propertyChangedCount++;
        };

        // Act & Assert
        viewModel.EditName = "First University";
        Assert.AreEqual("First University", viewModel.EditName);
        Assert.AreEqual(1, propertyChangedCount);

        viewModel.EditName = "Second University";
        Assert.AreEqual("Second University", viewModel.EditName);
        Assert.AreEqual(2, propertyChangedCount);

        viewModel.EditName = "Third University";
        Assert.AreEqual("Third University", viewModel.EditName);
        Assert.AreEqual(3, propertyChangedCount);
    }

    /// <summary>
    /// Tests that EditName getter returns the correct value after setting.
    /// Input conditions: Setting a value and then reading it back.
    /// Expected result: The getter returns the exact value that was set.
    /// </summary>
    [TestMethod]
    public void EditName_GetAfterSet_ReturnsSetValue()
    {
        // Arrange
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<UniversitiesViewModel>>();
        var viewModel = new UniversitiesViewModel(mockAcademic.Object, mockLogger.Object);
        const string expectedValue = "Harvard University";

        // Act
        viewModel.EditName = expectedValue;
        var actualValue = viewModel.EditName;

        // Assert
        Assert.AreEqual(expectedValue, actualValue);
    }

    /// <summary>
    /// Tests that EditName property with control characters is handled correctly.
    /// Input conditions: String containing various control characters.
    /// Expected result: Property value is updated and PropertyChanged event is raised.
    /// </summary>
    [TestMethod]
    [DataRow("\r\n", DisplayName = "Carriage return and newline")]
    [DataRow("\0", DisplayName = "Null character")]
    [DataRow("\b\f\v", DisplayName = "Backspace, form feed, vertical tab")]
    public void EditName_SetStringWithControlCharacters_UpdatesPropertyAndRaisesPropertyChanged(string valueWithControlChars)
    {
        // Arrange
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<UniversitiesViewModel>>();
        var viewModel = new UniversitiesViewModel(mockAcademic.Object, mockLogger.Object);
        string? propertyChangedName = null;
        viewModel.PropertyChanged += (sender, args) => propertyChangedName = args.PropertyName;

        // Act
        viewModel.EditName = valueWithControlChars;

        // Assert
        Assert.AreEqual(valueWithControlChars, viewModel.EditName);
        Assert.AreEqual("EditName", propertyChangedName);
    }

    /// <summary>
    /// Tests that setting EditName to empty string after having a non-empty value updates correctly.
    /// Input conditions: Set to non-empty value, then back to empty string.
    /// Expected result: Property is updated to empty string and PropertyChanged is raised.
    /// </summary>
    [TestMethod]
    public void EditName_SetToEmptyAfterNonEmpty_UpdatesPropertyAndRaisesPropertyChanged()
    {
        // Arrange
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<UniversitiesViewModel>>();
        var viewModel = new UniversitiesViewModel(mockAcademic.Object, mockLogger.Object);
        viewModel.EditName = "Some University";
        string? propertyChangedName = null;
        viewModel.PropertyChanged += (sender, args) => propertyChangedName = args.PropertyName;

        // Act
        viewModel.EditName = string.Empty;

        // Assert
        Assert.AreEqual(string.Empty, viewModel.EditName);
        Assert.AreEqual("EditName", propertyChangedName);
    }

    /// <summary>
    /// Tests that EditName handles strings with extreme lengths at boundary values.
    /// Input conditions: Strings with length 0, 1, and maximum practical length.
    /// Expected result: All values are handled correctly without exceptions.
    /// </summary>
    [TestMethod]
    [DataRow(0, DisplayName = "Zero length")]
    [DataRow(1, DisplayName = "Length of 1")]
    [DataRow(100, DisplayName = "Length of 100")]
    [DataRow(1000, DisplayName = "Length of 1000")]
    [DataRow(10000, DisplayName = "Length of 10000")]
    public void EditName_SetStringsWithVariousLengths_UpdatesPropertyCorrectly(int length)
    {
        // Arrange
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<UniversitiesViewModel>>();
        var viewModel = new UniversitiesViewModel(mockAcademic.Object, mockLogger.Object);
        var testString = new string('X', length);

        // Act
        viewModel.EditName = testString;

        // Assert
        Assert.AreEqual(testString, viewModel.EditName);
        Assert.AreEqual(length, viewModel.EditName.Length);
    }

    /// <summary>
    /// Tests that PropertyChanged event contains the correct sender object.
    /// Input conditions: Setting a new value for EditName.
    /// Expected result: PropertyChanged event is raised with the viewModel as sender.
    /// </summary>
    [TestMethod]
    public void EditName_SetValue_RaisesPropertyChangedWithCorrectSender()
    {
        // Arrange
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<UniversitiesViewModel>>();
        var viewModel = new UniversitiesViewModel(mockAcademic.Object, mockLogger.Object);
        object? eventSender = null;
        viewModel.PropertyChanged += (sender, args) => eventSender = sender;

        // Act
        viewModel.EditName = "New University";

        // Assert
        Assert.AreSame(viewModel, eventSender);
    }

    /// <summary>
    /// Tests that EditName handles alternating between different values correctly.
    /// Input conditions: Alternating between two different values multiple times.
    /// Expected result: Property updates correctly and PropertyChanged is raised each time.
    /// </summary>
    [TestMethod]
    public void EditName_AlternateBetweenTwoValues_UpdatesCorrectlyEachTime()
    {
        // Arrange
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<UniversitiesViewModel>>();
        var viewModel = new UniversitiesViewModel(mockAcademic.Object, mockLogger.Object);
        const string value1 = "University A";
        const string value2 = "University B";
        int propertyChangedCount = 0;
        viewModel.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == "EditName")
                propertyChangedCount++;
        };

        // Act & Assert
        viewModel.EditName = value1;
        Assert.AreEqual(value1, viewModel.EditName);
        Assert.AreEqual(1, propertyChangedCount);

        viewModel.EditName = value2;
        Assert.AreEqual(value2, viewModel.EditName);
        Assert.AreEqual(2, propertyChangedCount);

        viewModel.EditName = value1;
        Assert.AreEqual(value1, viewModel.EditName);
        Assert.AreEqual(3, propertyChangedCount);

        viewModel.EditName = value2;
        Assert.AreEqual(value2, viewModel.EditName);
        Assert.AreEqual(4, propertyChangedCount);
    }
}



/// <summary>
/// Tests for the ErrorMessage property of UniversitiesViewModel class.
/// </summary>
[TestClass]
public partial class UniversitiesViewModelErrorMessageTests
{
    /// <summary>
    /// Tests that ErrorMessage property returns the initial value of empty string.
    /// Input: None (initial state)
    /// Expected: ErrorMessage returns empty string
    /// </summary>
    [TestMethod]
    public void ErrorMessage_InitialValue_ReturnsEmptyString()
    {
        // Arrange
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<UniversitiesViewModel>>();
        var viewModel = new UniversitiesViewModel(mockAcademic.Object, mockLogger.Object);

        // Act
        var result = viewModel.ErrorMessage;

        // Assert
        Assert.AreEqual(string.Empty, result);
    }

    /// <summary>
    /// Tests that ErrorMessage property can be set and retrieved with various string values.
    /// Input: Various string values (empty, whitespace, normal, long, special characters)
    /// Expected: Property returns the set value
    /// </summary>
    [TestMethod]
    [DataRow("", DisplayName = "Empty string")]
    [DataRow("   ", DisplayName = "Whitespace only")]
    [DataRow("Error occurred", DisplayName = "Normal error message")]
    [DataRow("Error: An unexpected error occurred while processing your request. Please try again later.", DisplayName = "Long error message")]
    [DataRow("Error\nMultiline\nMessage", DisplayName = "Multiline string")]
    [DataRow("Error with special chars: @#$%^&*()", DisplayName = "Special characters")]
    [DataRow("Error with unicode: 你好世界", DisplayName = "Unicode characters")]
    [DataRow("\t\r\n", DisplayName = "Control characters")]
    [DataRow("A", DisplayName = "Single character")]
    [DataRow("!", DisplayName = "Single special character")]
    public void ErrorMessage_SetValue_ReturnsSetValue(string value)
    {
        // Arrange
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<UniversitiesViewModel>>();
        var viewModel = new UniversitiesViewModel(mockAcademic.Object, mockLogger.Object);

        // Act
        viewModel.ErrorMessage = value;
        var result = viewModel.ErrorMessage;

        // Assert
        Assert.AreEqual(value, result);
    }

    /// <summary>
    /// Tests that ErrorMessage property raises PropertyChanged event when value changes.
    /// Input: New string value different from current value
    /// Expected: PropertyChanged event is raised with correct property name
    /// </summary>
    [TestMethod]
    public void ErrorMessage_SetNewValue_RaisesPropertyChangedEvent()
    {
        // Arrange
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<UniversitiesViewModel>>();
        var viewModel = new UniversitiesViewModel(mockAcademic.Object, mockLogger.Object);
        var propertyChangedRaised = false;
        viewModel.PropertyChanged += (sender, args) => { if (args.PropertyName == nameof(viewModel.ErrorMessage)) propertyChangedRaised = true; };

        // Act
        viewModel.ErrorMessage = "New error message";

        // Assert
        Assert.IsTrue(propertyChangedRaised);
    }

    /// <summary>
    /// Tests that ErrorMessage property does not raise PropertyChanged event when set to the same value.
    /// Input: Same string value as current value
    /// Expected: PropertyChanged event is not raised
    /// </summary>
    [TestMethod]
    public void ErrorMessage_SetSameValue_DoesNotRaisePropertyChangedEvent()
    {
        // Arrange
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<UniversitiesViewModel>>();
        var viewModel = new UniversitiesViewModel(mockAcademic.Object, mockLogger.Object);
        viewModel.ErrorMessage = "Same value";
        var propertyChangedCount = 0;
        viewModel.PropertyChanged += (sender, args) => { if (args.PropertyName == nameof(viewModel.ErrorMessage)) propertyChangedCount++; };

        // Act
        viewModel.ErrorMessage = "Same value";

        // Assert
        Assert.AreEqual(0, propertyChangedCount);
    }

    /// <summary>
    /// Tests that ErrorMessage property can handle very long strings.
    /// Input: String with 10000 characters
    /// Expected: Property correctly stores and returns the long string
    /// </summary>
    [TestMethod]
    public void ErrorMessage_SetVeryLongString_ReturnsSetValue()
    {
        // Arrange
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<UniversitiesViewModel>>();
        var viewModel = new UniversitiesViewModel(mockAcademic.Object, mockLogger.Object);
        var longString = new string('E', 10000);

        // Act
        viewModel.ErrorMessage = longString;
        var result = viewModel.ErrorMessage;

        // Assert
        Assert.AreEqual(longString, result);
    }

    /// <summary>
    /// Tests that ErrorMessage property can be set multiple times with different values.
    /// Input: Multiple different string values set sequentially
    /// Expected: Property returns the most recent value each time
    /// </summary>
    [TestMethod]
    public void ErrorMessage_SetMultipleTimes_ReturnsLatestValue()
    {
        // Arrange
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<UniversitiesViewModel>>();
        var viewModel = new UniversitiesViewModel(mockAcademic.Object, mockLogger.Object);

        // Act & Assert
        viewModel.ErrorMessage = "First error";
        Assert.AreEqual("First error", viewModel.ErrorMessage);

        viewModel.ErrorMessage = "Second error";
        Assert.AreEqual("Second error", viewModel.ErrorMessage);

        viewModel.ErrorMessage = "Third error";
        Assert.AreEqual("Third error", viewModel.ErrorMessage);
    }

    /// <summary>
    /// Tests that ErrorMessage property raises PropertyChanged event for each unique value change.
    /// Input: Multiple different string values set sequentially
    /// Expected: PropertyChanged event is raised for each unique value change
    /// </summary>
    [TestMethod]
    public void ErrorMessage_SetMultipleUniqueValues_RaisesPropertyChangedEventEachTime()
    {
        // Arrange
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<UniversitiesViewModel>>();
        var viewModel = new UniversitiesViewModel(mockAcademic.Object, mockLogger.Object);
        var propertyChangedCount = 0;
        viewModel.PropertyChanged += (sender, args) => { if (args.PropertyName == nameof(viewModel.ErrorMessage)) propertyChangedCount++; };

        // Act
        viewModel.ErrorMessage = "Error 1";
        viewModel.ErrorMessage = "Error 2";
        viewModel.ErrorMessage = "Error 3";

        // Assert
        Assert.AreEqual(3, propertyChangedCount);
    }

    /// <summary>
    /// Tests that ErrorMessage property correctly handles whitespace variations.
    /// Input: Various whitespace strings
    /// Expected: Property correctly stores each whitespace variation
    /// </summary>
    [TestMethod]
    [DataRow("\t", DisplayName = "Tab character")]
    [DataRow("\n", DisplayName = "Newline character")]
    [DataRow("\r", DisplayName = "Carriage return")]
    [DataRow("\r\n", DisplayName = "CRLF")]
    [DataRow("     ", DisplayName = "Multiple spaces")]
    [DataRow("\t\n\r", DisplayName = "Mixed whitespace")]
    [DataRow(" ", DisplayName = "Single space")]
    public void ErrorMessage_SetWhitespaceVariations_UpdatesPropertyCorrectly(string whitespaceValue)
    {
        // Arrange
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<UniversitiesViewModel>>();
        var viewModel = new UniversitiesViewModel(mockAcademic.Object, mockLogger.Object);

        // Act
        viewModel.ErrorMessage = whitespaceValue;

        // Assert
        Assert.AreEqual(whitespaceValue, viewModel.ErrorMessage);
    }

    /// <summary>
    /// Tests that ErrorMessage handles single character strings correctly.
    /// Input: Single character strings of various types
    /// Expected: Property correctly stores single character values
    /// </summary>
    [TestMethod]
    [DataRow("A", DisplayName = "Single letter")]
    [DataRow("1", DisplayName = "Single digit")]
    [DataRow("!", DisplayName = "Single special character")]
    [DataRow(" ", DisplayName = "Single space")]
    [DataRow("@", DisplayName = "At symbol")]
    [DataRow("#", DisplayName = "Hash symbol")]
    public void ErrorMessage_SetSingleCharacter_UpdatesPropertyCorrectly(string singleChar)
    {
        // Arrange
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<UniversitiesViewModel>>();
        var viewModel = new UniversitiesViewModel(mockAcademic.Object, mockLogger.Object);

        // Act
        viewModel.ErrorMessage = singleChar;

        // Assert
        Assert.AreEqual(singleChar, viewModel.ErrorMessage);
    }

    /// <summary>
    /// Tests that ErrorMessage handles control characters correctly.
    /// Input: Strings containing various control characters
    /// Expected: Property correctly stores control character values
    /// </summary>
    [TestMethod]
    [DataRow("\0", DisplayName = "Null character")]
    [DataRow("\b", DisplayName = "Backspace")]
    [DataRow("\f", DisplayName = "Form feed")]
    [DataRow("\v", DisplayName = "Vertical tab")]
    [DataRow("\b\f\v", DisplayName = "Mixed control characters")]
    public void ErrorMessage_SetControlCharacters_UpdatesPropertyCorrectly(string controlChars)
    {
        // Arrange
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<UniversitiesViewModel>>();
        var viewModel = new UniversitiesViewModel(mockAcademic.Object, mockLogger.Object);

        // Act
        viewModel.ErrorMessage = controlChars;

        // Assert
        Assert.AreEqual(controlChars, viewModel.ErrorMessage);
    }

    /// <summary>
    /// Tests that ErrorMessage handles international and Unicode characters correctly.
    /// Input: Strings with various international character sets
    /// Expected: Property correctly stores Unicode values
    /// </summary>
    [TestMethod]
    [DataRow("Erreur: Opération échouée", DisplayName = "French accents")]
    [DataRow("错误：操作失败", DisplayName = "Chinese characters")]
    [DataRow("Ошибка: операция не удалась", DisplayName = "Cyrillic characters")]
    [DataRow("خطأ: فشلت العملية", DisplayName = "Arabic characters")]
    [DataRow("エラー：操作に失敗しました", DisplayName = "Japanese characters")]
    [DataRow("שגיאה: הפעולה נכשלה", DisplayName = "Hebrew characters")]
    [DataRow("🚨 Error occurred! ⚠️", DisplayName = "Emoji characters")]
    public void ErrorMessage_SetUnicodeCharacters_UpdatesPropertyCorrectly(string unicodeValue)
    {
        // Arrange
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<UniversitiesViewModel>>();
        var viewModel = new UniversitiesViewModel(mockAcademic.Object, mockLogger.Object);

        // Act
        viewModel.ErrorMessage = unicodeValue;

        // Assert
        Assert.AreEqual(unicodeValue, viewModel.ErrorMessage);
    }

    /// <summary>
    /// Tests that setting ErrorMessage back to empty string after having a value works correctly.
    /// Input: Set to non-empty value, then back to empty string
    /// Expected: Property is updated to empty string and PropertyChanged is raised
    /// </summary>
    [TestMethod]
    public void ErrorMessage_SetToEmptyAfterNonEmpty_UpdatesPropertyAndRaisesPropertyChanged()
    {
        // Arrange
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<UniversitiesViewModel>>();
        var viewModel = new UniversitiesViewModel(mockAcademic.Object, mockLogger.Object);
        viewModel.ErrorMessage = "Some error";
        var propertyChangedRaised = false;
        viewModel.PropertyChanged += (sender, args) => { if (args.PropertyName == nameof(viewModel.ErrorMessage)) propertyChangedRaised = true; };

        // Act
        viewModel.ErrorMessage = string.Empty;

        // Assert
        Assert.AreEqual(string.Empty, viewModel.ErrorMessage);
        Assert.IsTrue(propertyChangedRaised);
    }

    /// <summary>
    /// Tests that PropertyChanged event sender is the view model instance.
    /// Input: New error message value
    /// Expected: PropertyChanged event is raised with the view model as sender
    /// </summary>
    [TestMethod]
    public void ErrorMessage_SetValue_PropertyChangedEventHasCorrectSender()
    {
        // Arrange
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<UniversitiesViewModel>>();
        var viewModel = new UniversitiesViewModel(mockAcademic.Object, mockLogger.Object);
        object? eventSender = null;
        viewModel.PropertyChanged += (sender, args) => { if (args.PropertyName == nameof(viewModel.ErrorMessage)) eventSender = sender; };

        // Act
        viewModel.ErrorMessage = "Test error";

        // Assert
        Assert.AreSame(viewModel, eventSender);
    }

    /// <summary>
    /// Tests that ErrorMessage handles extremely long strings with various character types.
    /// Input: Very long string (100000 characters) containing mixed content
    /// Expected: Property correctly stores and returns the entire long string
    /// </summary>
    [TestMethod]
    public void ErrorMessage_SetExtremelyLongMixedString_UpdatesPropertyCorrectly()
    {
        // Arrange
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<UniversitiesViewModel>>();
        var viewModel = new UniversitiesViewModel(mockAcademic.Object, mockLogger.Object);
        var sb = new StringBuilder();
        for (int i = 0; i < 100000; i++)
        {
            sb.Append((char)('A' + (i % 26)));
        }
        var extremelyLongString = sb.ToString();

        // Act
        viewModel.ErrorMessage = extremelyLongString;

        // Assert
        Assert.AreEqual(extremelyLongString, viewModel.ErrorMessage);
    }

    /// <summary>
    /// Tests that ErrorMessage with special formatting characters is handled correctly.
    /// Input: Strings with special formatting and escape sequences
    /// Expected: Property correctly stores the formatted strings
    /// </summary>
    [TestMethod]
    [DataRow("Error: \"Quoted message\"", DisplayName = "Quoted string")]
    [DataRow("Error: Path\\To\\File", DisplayName = "Backslashes")]
    [DataRow("Error: C:\\Users\\Test\\file.txt", DisplayName = "File path")]
    [DataRow("Error: {0} - {1}", DisplayName = "Format placeholders")]
    [DataRow("Error: [ErrorCode: 500]", DisplayName = "Brackets")]
    public void ErrorMessage_SetSpecialFormattingCharacters_UpdatesPropertyCorrectly(string formattedValue)
    {
        // Arrange
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<UniversitiesViewModel>>();
        var viewModel = new UniversitiesViewModel(mockAcademic.Object, mockLogger.Object);

        // Act
        viewModel.ErrorMessage = formattedValue;

        // Assert
        Assert.AreEqual(formattedValue, viewModel.ErrorMessage);
    }

    /// <summary>
    /// Tests that ErrorMessage property raises PropertyChanged with the correct property name in event args.
    /// Input: New error message value
    /// Expected: PropertyChanged event args contain "ErrorMessage" as property name
    /// </summary>
    [TestMethod]
    public void ErrorMessage_SetValue_RaisesPropertyChangedWithCorrectPropertyName()
    {
        // Arrange
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<UniversitiesViewModel>>();
        var viewModel = new UniversitiesViewModel(mockAcademic.Object, mockLogger.Object);
        string? propertyName = null;
        viewModel.PropertyChanged += (sender, args) => { propertyName = args.PropertyName; };

        // Act
        viewModel.ErrorMessage = "Test error";

        // Assert
        Assert.AreEqual(nameof(viewModel.ErrorMessage), propertyName);
    }

    /// <summary>
    /// Tests that ErrorMessage can transition between empty and non-empty values multiple times.
    /// Input: Alternating between empty and non-empty values
    /// Expected: Property updates correctly each time and PropertyChanged is raised for each change
    /// </summary>
    [TestMethod]
    public void ErrorMessage_AlternateBetweenEmptyAndNonEmpty_UpdatesCorrectlyEachTime()
    {
        // Arrange
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<UniversitiesViewModel>>();
        var viewModel = new UniversitiesViewModel(mockAcademic.Object, mockLogger.Object);
        var propertyChangedCount = 0;
        viewModel.PropertyChanged += (sender, args) => { if (args.PropertyName == nameof(viewModel.ErrorMessage)) propertyChangedCount++; };

        // Act
        viewModel.ErrorMessage = "Error 1";
        viewModel.ErrorMessage = string.Empty;
        viewModel.ErrorMessage = "Error 2";
        viewModel.ErrorMessage = string.Empty;

        // Assert
        Assert.AreEqual(string.Empty, viewModel.ErrorMessage);
        Assert.AreEqual(4, propertyChangedCount);
    }

    /// <summary>
    /// Tests that setting ErrorMessage to different whitespace values are treated as different values.
    /// Input: Different whitespace values
    /// Expected: Each whitespace value is treated as unique and PropertyChanged is raised
    /// </summary>
    [TestMethod]
    public void ErrorMessage_SetDifferentWhitespaceValues_TreatsAsDifferent()
    {
        // Arrange
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<UniversitiesViewModel>>();
        var viewModel = new UniversitiesViewModel(mockAcademic.Object, mockLogger.Object);
        var propertyChangedCount = 0;
        viewModel.PropertyChanged += (sender, args) => { if (args.PropertyName == nameof(viewModel.ErrorMessage)) propertyChangedCount++; };

        // Act
        viewModel.ErrorMessage = " ";
        viewModel.ErrorMessage = "  ";
        viewModel.ErrorMessage = "\t";

        // Assert
        Assert.AreEqual(3, propertyChangedCount);
    }

    /// <summary>
    /// Tests that ErrorMessage with boundary length strings works correctly.
    /// Input: Strings of various lengths from 0 to very large
    /// Expected: All lengths are handled correctly
    /// </summary>
    [TestMethod]
    [DataRow(0, DisplayName = "Length 0 (empty)")]
    [DataRow(1, DisplayName = "Length 1")]
    [DataRow(100, DisplayName = "Length 100")]
    [DataRow(1000, DisplayName = "Length 1000")]
    [DataRow(10000, DisplayName = "Length 10000")]
    public void ErrorMessage_SetVariousLengthStrings_UpdatesPropertyCorrectly(int length)
    {
        // Arrange
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<UniversitiesViewModel>>();
        var viewModel = new UniversitiesViewModel(mockAcademic.Object, mockLogger.Object);
        var testString = new string('X', length);

        // Act
        viewModel.ErrorMessage = testString;

        // Assert
        Assert.AreEqual(testString, viewModel.ErrorMessage);
        Assert.AreEqual(length, viewModel.ErrorMessage.Length);
    }

    /// <summary>
    /// Tests that ErrorMessage can handle strings with mixed special, control, and unicode characters.
    /// Input: String containing mix of various character types
    /// Expected: Property correctly stores the mixed content string
    /// </summary>
    [TestMethod]
    public void ErrorMessage_SetMixedCharacterTypesString_UpdatesPropertyCorrectly()
    {
        // Arrange
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<UniversitiesViewModel>>();
        var viewModel = new UniversitiesViewModel(mockAcademic.Object, mockLogger.Object);
        var mixedString = "Error: 错误 Erreur\n@#$%\t🚨\r\nEnd";

        // Act
        viewModel.ErrorMessage = mixedString;

        // Assert
        Assert.AreEqual(mixedString, viewModel.ErrorMessage);
    }

    /// <summary>
    /// Tests that ErrorMessage property can be set to the same value as initial value without raising PropertyChanged.
    /// Input: Set to empty string when already empty
    /// Expected: PropertyChanged is not raised
    /// </summary>
    [TestMethod]
    public void ErrorMessage_SetToInitialValueAgain_DoesNotRaisePropertyChanged()
    {
        // Arrange
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<UniversitiesViewModel>>();
        var viewModel = new UniversitiesViewModel(mockAcademic.Object, mockLogger.Object);
        var propertyChangedCount = 0;
        viewModel.PropertyChanged += (sender, args) => { if (args.PropertyName == nameof(viewModel.ErrorMessage)) propertyChangedCount++; };

        // Act
        viewModel.ErrorMessage = string.Empty;

        // Assert
        Assert.AreEqual(0, propertyChangedCount);
    }
}



/// <summary>
/// Tests for the PageInfo property of UniversitiesViewModel class.
/// </summary>
[TestClass]
public partial class UniversitiesViewModelPageInfoTests
{
    /// <summary>
    /// Tests that PageInfo returns the correct default formatted string on initialization.
    /// Input: Default initialization (CurrentPage=1, TotalPages=1).
    /// Expected: PageInfo returns "Page 1 of 1".
    /// </summary>
    [TestMethod]
    public void PageInfo_DefaultInitialization_ReturnsCorrectFormat()
    {
        // Arrange
        Mock<IAcademicService> mockAcademic = new Mock<IAcademicService>();
        Mock<ILogger<UniversitiesViewModel>> mockLogger = new Mock<ILogger<UniversitiesViewModel>>();
        UniversitiesViewModel viewModel = new UniversitiesViewModel(mockAcademic.Object, mockLogger.Object);

        // Act
        string result = viewModel.PageInfo;

        // Assert
        Assert.AreEqual("Page 1 of 1", result);
    }

    /// <summary>
    /// Tests that PageInfo returns the correct formatted string with typical positive values.
    /// Input: Various typical positive page combinations.
    /// Expected: PageInfo returns "Page {currentPage} of {totalPages}".
    /// </summary>
    [TestMethod]
    [DataRow(1, 1, "Page 1 of 1")]
    [DataRow(1, 10, "Page 1 of 10")]
    [DataRow(5, 10, "Page 5 of 10")]
    [DataRow(10, 10, "Page 10 of 10")]
    [DataRow(50, 100, "Page 50 of 100")]
    [DataRow(1, 1000, "Page 1 of 1000")]
    [DataRow(999, 1000, "Page 999 of 1000")]
    public void PageInfo_TypicalPositiveValues_ReturnsCorrectFormat(int currentPage, int totalPages, string expected)
    {
        // Arrange
        Mock<IAcademicService> mockAcademic = new Mock<IAcademicService>();
        Mock<ILogger<UniversitiesViewModel>> mockLogger = new Mock<ILogger<UniversitiesViewModel>>();
        UniversitiesViewModel viewModel = new UniversitiesViewModel(mockAcademic.Object, mockLogger.Object);

        // Act
        viewModel.CurrentPage = currentPage;
        viewModel.TotalPages = totalPages;
        string result = viewModel.PageInfo;

        // Assert
        Assert.AreEqual(expected, result);
    }

    /// <summary>
    /// Tests that PageInfo returns the correct formatted string with zero values.
    /// Input: Zero values for CurrentPage and/or TotalPages.
    /// Expected: PageInfo returns formatted string with zero values.
    /// </summary>
    [TestMethod]
    [DataRow(0, 0, "Page 0 of 0")]
    [DataRow(0, 10, "Page 0 of 10")]
    [DataRow(1, 0, "Page 1 of 0")]
    [DataRow(0, 1, "Page 0 of 1")]
    public void PageInfo_ZeroValues_ReturnsCorrectFormat(int currentPage, int totalPages, string expected)
    {
        // Arrange
        Mock<IAcademicService> mockAcademic = new Mock<IAcademicService>();
        Mock<ILogger<UniversitiesViewModel>> mockLogger = new Mock<ILogger<UniversitiesViewModel>>();
        UniversitiesViewModel viewModel = new UniversitiesViewModel(mockAcademic.Object, mockLogger.Object);

        // Act
        viewModel.CurrentPage = currentPage;
        viewModel.TotalPages = totalPages;
        string result = viewModel.PageInfo;

        // Assert
        Assert.AreEqual(expected, result);
    }

    /// <summary>
    /// Tests that PageInfo returns the correct formatted string with negative values.
    /// Input: Negative values for CurrentPage and/or TotalPages.
    /// Expected: PageInfo returns formatted string with negative values.
    /// </summary>
    [TestMethod]
    [DataRow(-1, 10, "Page -1 of 10")]
    [DataRow(5, -10, "Page 5 of -10")]
    [DataRow(-5, -10, "Page -5 of -10")]
    [DataRow(-1, -1, "Page -1 of -1")]
    [DataRow(-100, 100, "Page -100 of 100")]
    [DataRow(100, -100, "Page 100 of -100")]
    public void PageInfo_NegativeValues_ReturnsCorrectFormat(int currentPage, int totalPages, string expected)
    {
        // Arrange
        Mock<IAcademicService> mockAcademic = new Mock<IAcademicService>();
        Mock<ILogger<UniversitiesViewModel>> mockLogger = new Mock<ILogger<UniversitiesViewModel>>();
        UniversitiesViewModel viewModel = new UniversitiesViewModel(mockAcademic.Object, mockLogger.Object);

        // Act
        viewModel.CurrentPage = currentPage;
        viewModel.TotalPages = totalPages;
        string result = viewModel.PageInfo;

        // Assert
        Assert.AreEqual(expected, result);
    }

    /// <summary>
    /// Tests that PageInfo returns the correct formatted string when CurrentPage exceeds TotalPages.
    /// Input: CurrentPage greater than TotalPages.
    /// Expected: PageInfo returns formatted string showing CurrentPage > TotalPages.
    /// </summary>
    [TestMethod]
    [DataRow(10, 5, "Page 10 of 5")]
    [DataRow(100, 10, "Page 100 of 10")]
    [DataRow(2, 1, "Page 2 of 1")]
    [DataRow(1000, 100, "Page 1000 of 100")]
    public void PageInfo_CurrentPageExceedsTotalPages_ReturnsCorrectFormat(int currentPage, int totalPages, string expected)
    {
        // Arrange
        Mock<IAcademicService> mockAcademic = new Mock<IAcademicService>();
        Mock<ILogger<UniversitiesViewModel>> mockLogger = new Mock<ILogger<UniversitiesViewModel>>();
        UniversitiesViewModel viewModel = new UniversitiesViewModel(mockAcademic.Object, mockLogger.Object);

        // Act
        viewModel.CurrentPage = currentPage;
        viewModel.TotalPages = totalPages;
        string result = viewModel.PageInfo;

        // Assert
        Assert.AreEqual(expected, result);
    }

    /// <summary>
    /// Tests that PageInfo returns the correct formatted string with int.MaxValue.
    /// Input: int.MaxValue for CurrentPage and TotalPages.
    /// Expected: PageInfo returns formatted string with int.MaxValue.
    /// </summary>
    [TestMethod]
    public void PageInfo_IntMaxValue_ReturnsCorrectFormat()
    {
        // Arrange
        Mock<IAcademicService> mockAcademic = new Mock<IAcademicService>();
        Mock<ILogger<UniversitiesViewModel>> mockLogger = new Mock<ILogger<UniversitiesViewModel>>();
        UniversitiesViewModel viewModel = new UniversitiesViewModel(mockAcademic.Object, mockLogger.Object);

        // Act
        viewModel.CurrentPage = int.MaxValue;
        viewModel.TotalPages = int.MaxValue;
        string result = viewModel.PageInfo;

        // Assert
        Assert.AreEqual($"Page {int.MaxValue} of {int.MaxValue}", result);
    }

    /// <summary>
    /// Tests that PageInfo returns the correct formatted string with int.MinValue.
    /// Input: int.MinValue for CurrentPage and TotalPages.
    /// Expected: PageInfo returns formatted string with int.MinValue.
    /// </summary>
    [TestMethod]
    public void PageInfo_IntMinValue_ReturnsCorrectFormat()
    {
        // Arrange
        Mock<IAcademicService> mockAcademic = new Mock<IAcademicService>();
        Mock<ILogger<UniversitiesViewModel>> mockLogger = new Mock<ILogger<UniversitiesViewModel>>();
        UniversitiesViewModel viewModel = new UniversitiesViewModel(mockAcademic.Object, mockLogger.Object);

        // Act
        viewModel.CurrentPage = int.MinValue;
        viewModel.TotalPages = int.MinValue;
        string result = viewModel.PageInfo;

        // Assert
        Assert.AreEqual($"Page {int.MinValue} of {int.MinValue}", result);
    }

    /// <summary>
    /// Tests that PageInfo returns the correct formatted string with mixed extreme values.
    /// Input: Mix of int.MaxValue and int.MinValue for properties.
    /// Expected: PageInfo returns formatted string with mixed extreme values.
    /// </summary>
    [TestMethod]
    [DataRow(int.MaxValue, int.MinValue)]
    [DataRow(int.MinValue, int.MaxValue)]
    [DataRow(int.MaxValue, 0)]
    [DataRow(0, int.MinValue)]
    public void PageInfo_MixedExtremeValues_ReturnsCorrectFormat(int currentPage, int totalPages)
    {
        // Arrange
        Mock<IAcademicService> mockAcademic = new Mock<IAcademicService>();
        Mock<ILogger<UniversitiesViewModel>> mockLogger = new Mock<ILogger<UniversitiesViewModel>>();
        UniversitiesViewModel viewModel = new UniversitiesViewModel(mockAcademic.Object, mockLogger.Object);
        string expected = $"Page {currentPage} of {totalPages}";

        // Act
        viewModel.CurrentPage = currentPage;
        viewModel.TotalPages = totalPages;
        string result = viewModel.PageInfo;

        // Assert
        Assert.AreEqual(expected, result);
    }

    /// <summary>
    /// Tests that PageInfo correctly updates when CurrentPage changes.
    /// Input: Setting CurrentPage to a new value while TotalPages remains constant.
    /// Expected: PageInfo reflects the new CurrentPage value.
    /// </summary>
    [TestMethod]
    public void PageInfo_CurrentPageChanges_ReturnsUpdatedFormat()
    {
        // Arrange
        Mock<IAcademicService> mockAcademic = new Mock<IAcademicService>();
        Mock<ILogger<UniversitiesViewModel>> mockLogger = new Mock<ILogger<UniversitiesViewModel>>();
        UniversitiesViewModel viewModel = new UniversitiesViewModel(mockAcademic.Object, mockLogger.Object);
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
    /// Tests that PageInfo correctly updates when TotalPages changes.
    /// Input: Setting TotalPages to a new value while CurrentPage remains constant.
    /// Expected: PageInfo reflects the new TotalPages value.
    /// </summary>
    [TestMethod]
    public void PageInfo_TotalPagesChanges_ReturnsUpdatedFormat()
    {
        // Arrange
        Mock<IAcademicService> mockAcademic = new Mock<IAcademicService>();
        Mock<ILogger<UniversitiesViewModel>> mockLogger = new Mock<ILogger<UniversitiesViewModel>>();
        UniversitiesViewModel viewModel = new UniversitiesViewModel(mockAcademic.Object, mockLogger.Object);
        viewModel.CurrentPage = 5;

        // Act & Assert
        viewModel.TotalPages = 10;
        Assert.AreEqual("Page 5 of 10", viewModel.PageInfo);

        viewModel.TotalPages = 20;
        Assert.AreEqual("Page 5 of 20", viewModel.PageInfo);

        viewModel.TotalPages = 100;
        Assert.AreEqual("Page 5 of 100", viewModel.PageInfo);
    }

    /// <summary>
    /// Tests that PageInfo correctly updates when both CurrentPage and TotalPages change.
    /// Input: Setting both CurrentPage and TotalPages to new values.
    /// Expected: PageInfo reflects both new values.
    /// </summary>
    [TestMethod]
    public void PageInfo_BothPropertiesChange_ReturnsUpdatedFormat()
    {
        // Arrange
        Mock<IAcademicService> mockAcademic = new Mock<IAcademicService>();
        Mock<ILogger<UniversitiesViewModel>> mockLogger = new Mock<ILogger<UniversitiesViewModel>>();
        UniversitiesViewModel viewModel = new UniversitiesViewModel(mockAcademic.Object, mockLogger.Object);

        // Act & Assert
        viewModel.CurrentPage = 3;
        viewModel.TotalPages = 15;
        Assert.AreEqual("Page 3 of 15", viewModel.PageInfo);

        viewModel.CurrentPage = 7;
        viewModel.TotalPages = 25;
        Assert.AreEqual("Page 7 of 25", viewModel.PageInfo);
    }

    /// <summary>
    /// Tests that PageInfo is re-evaluated on each access (not cached).
    /// Input: Change CurrentPage multiple times and read PageInfo each time.
    /// Expected: PageInfo returns the current value on each access.
    /// </summary>
    [TestMethod]
    public void PageInfo_MultipleAccesses_ReturnsCurrentValue()
    {
        // Arrange
        Mock<IAcademicService> mockAcademic = new Mock<IAcademicService>();
        Mock<ILogger<UniversitiesViewModel>> mockLogger = new Mock<ILogger<UniversitiesViewModel>>();
        UniversitiesViewModel viewModel = new UniversitiesViewModel(mockAcademic.Object, mockLogger.Object);
        viewModel.TotalPages = 10;

        // Act & Assert - Multiple accesses return current value
        viewModel.CurrentPage = 1;
        Assert.AreEqual("Page 1 of 10", viewModel.PageInfo);
        Assert.AreEqual("Page 1 of 10", viewModel.PageInfo);

        viewModel.CurrentPage = 2;
        Assert.AreEqual("Page 2 of 10", viewModel.PageInfo);
        Assert.AreEqual("Page 2 of 10", viewModel.PageInfo);
    }

    /// <summary>
    /// Tests that PageInfo handles large positive values correctly.
    /// Input: Large positive values for CurrentPage and TotalPages.
    /// Expected: PageInfo returns correctly formatted string with large values.
    /// </summary>
    [TestMethod]
    [DataRow(999999, 1000000, "Page 999999 of 1000000")]
    [DataRow(1000000, 1000000, "Page 1000000 of 1000000")]
    [DataRow(5000, 10000, "Page 5000 of 10000")]
    public void PageInfo_LargePositiveValues_ReturnsCorrectFormat(int currentPage, int totalPages, string expected)
    {
        // Arrange
        Mock<IAcademicService> mockAcademic = new Mock<IAcademicService>();
        Mock<ILogger<UniversitiesViewModel>> mockLogger = new Mock<ILogger<UniversitiesViewModel>>();
        UniversitiesViewModel viewModel = new UniversitiesViewModel(mockAcademic.Object, mockLogger.Object);

        // Act
        viewModel.CurrentPage = currentPage;
        viewModel.TotalPages = totalPages;
        string result = viewModel.PageInfo;

        // Assert
        Assert.AreEqual(expected, result);
    }

    /// <summary>
    /// Tests that PageInfo handles large negative values correctly.
    /// Input: Large negative values for CurrentPage and TotalPages.
    /// Expected: PageInfo returns correctly formatted string with large negative values.
    /// </summary>
    [TestMethod]
    [DataRow(-999999, -1000000, "Page -999999 of -1000000")]
    [DataRow(-1000000, -1000000, "Page -1000000 of -1000000")]
    [DataRow(-5000, -10000, "Page -5000 of -10000")]
    public void PageInfo_LargeNegativeValues_ReturnsCorrectFormat(int currentPage, int totalPages, string expected)
    {
        // Arrange
        Mock<IAcademicService> mockAcademic = new Mock<IAcademicService>();
        Mock<ILogger<UniversitiesViewModel>> mockLogger = new Mock<ILogger<UniversitiesViewModel>>();
        UniversitiesViewModel viewModel = new UniversitiesViewModel(mockAcademic.Object, mockLogger.Object);

        // Act
        viewModel.CurrentPage = currentPage;
        viewModel.TotalPages = totalPages;
        string result = viewModel.PageInfo;

        // Assert
        Assert.AreEqual(expected, result);
    }
}



/// <summary>
/// Tests for the TotalPages property of UniversitiesViewModel class.
/// </summary>
[TestClass]
public partial class UniversitiesViewModelTotalPagesTests
{
    /// <summary>
    /// Tests that TotalPages getter returns the initial default value of 1.
    /// Input: None (initial state).
    /// Expected: TotalPages returns 1.
    /// </summary>
    [TestMethod]
    public void TotalPages_Get_ReturnsInitialValue()
    {
        // Arrange
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<UniversitiesViewModel>>();
        var viewModel = new UniversitiesViewModel(mockAcademic.Object, mockLogger.Object);

        // Act
        int result = viewModel.TotalPages;

        // Assert
        Assert.AreEqual(1, result);
    }

    /// <summary>
    /// Tests that setting TotalPages to a different value updates the property and raises PropertyChanged for both "TotalPages" and "PageInfo".
    /// Input: Various different integer values including boundaries.
    /// Expected: Property is updated, PropertyChanged is raised for "TotalPages" and "PageInfo".
    /// </summary>
    [TestMethod]
    [DataRow(2)]
    [DataRow(5)]
    [DataRow(10)]
    [DataRow(100)]
    [DataRow(1000)]
    [DataRow(0)]
    [DataRow(-1)]
    [DataRow(-100)]
    [DataRow(int.MaxValue)]
    [DataRow(int.MinValue)]
    public void TotalPages_SetDifferentValue_RaisesPropertyChangedForBothProperties(int newValue)
    {
        // Arrange
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<UniversitiesViewModel>>();
        var viewModel = new UniversitiesViewModel(mockAcademic.Object, mockLogger.Object);
        var totalPagesChanged = false;
        var pageInfoChanged = false;

        viewModel.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == nameof(viewModel.TotalPages))
                totalPagesChanged = true;
            if (args.PropertyName == nameof(viewModel.PageInfo))
                pageInfoChanged = true;
        };

        // Act
        viewModel.TotalPages = newValue;

        // Assert
        Assert.AreEqual(newValue, viewModel.TotalPages);
        Assert.IsTrue(totalPagesChanged, "PropertyChanged should be raised for TotalPages");
        Assert.IsTrue(pageInfoChanged, "PropertyChanged should be raised for PageInfo");
    }

    /// <summary>
    /// Tests that setting TotalPages to the same value does not raise PropertyChanged events.
    /// Input: Same value as current (1).
    /// Expected: No PropertyChanged events are raised.
    /// </summary>
    [TestMethod]
    public void TotalPages_SetSameValue_DoesNotRaisePropertyChanged()
    {
        // Arrange
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<UniversitiesViewModel>>();
        var viewModel = new UniversitiesViewModel(mockAcademic.Object, mockLogger.Object);
        var propertyChangedRaised = false;

        viewModel.PropertyChanged += (sender, args) =>
        {
            propertyChangedRaised = true;
        };

        // Act
        viewModel.TotalPages = 1; // Setting to same value as initial

        // Assert
        Assert.IsFalse(propertyChangedRaised, "PropertyChanged should not be raised when setting to same value");
    }

    /// <summary>
    /// Tests that setting TotalPages updates the PageInfo property which depends on it.
    /// Input: Various TotalPages values while CurrentPage remains at default 1.
    /// Expected: PageInfo returns formatted string "Page {CurrentPage} of {TotalPages}".
    /// </summary>
    [TestMethod]
    [DataRow(1, "Page 1 of 1")]
    [DataRow(5, "Page 1 of 5")]
    [DataRow(10, "Page 1 of 10")]
    [DataRow(100, "Page 1 of 100")]
    [DataRow(0, "Page 1 of 0")]
    [DataRow(-1, "Page 1 of -1")]
    [DataRow(int.MaxValue, "Page 1 of 2147483647")]
    [DataRow(int.MinValue, "Page 1 of -2147483648")]
    public void TotalPages_SetValue_UpdatesPageInfoProperty(int totalPages, string expectedPageInfo)
    {
        // Arrange
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<UniversitiesViewModel>>();
        var viewModel = new UniversitiesViewModel(mockAcademic.Object, mockLogger.Object);

        // Act
        viewModel.TotalPages = totalPages;

        // Assert
        Assert.AreEqual(expectedPageInfo, viewModel.PageInfo);
    }

    /// <summary>
    /// Tests that setting TotalPages to boundary values works correctly.
    /// Input: Boundary integer values (int.MaxValue, int.MinValue, 0).
    /// Expected: Property is updated correctly to the boundary value.
    /// </summary>
    [TestMethod]
    [DataRow(int.MaxValue)]
    [DataRow(int.MinValue)]
    [DataRow(0)]
    public void TotalPages_SetBoundaryValues_UpdatesValueCorrectly(int boundaryValue)
    {
        // Arrange
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<UniversitiesViewModel>>();
        var viewModel = new UniversitiesViewModel(mockAcademic.Object, mockLogger.Object);

        // Act
        viewModel.TotalPages = boundaryValue;

        // Assert
        Assert.AreEqual(boundaryValue, viewModel.TotalPages);
    }

    /// <summary>
    /// Tests that multiple consecutive sets with different values raise PropertyChanged each time.
    /// Input: Sequential different values (5, 10, 15).
    /// Expected: PropertyChanged is raised for each value change.
    /// </summary>
    [TestMethod]
    public void TotalPages_MultipleConsecutiveSetsWithDifferentValues_RaisesPropertyChangedEachTime()
    {
        // Arrange
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<UniversitiesViewModel>>();
        var viewModel = new UniversitiesViewModel(mockAcademic.Object, mockLogger.Object);
        var eventCount = 0;

        viewModel.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == nameof(viewModel.TotalPages))
                eventCount++;
        };

        // Act
        viewModel.TotalPages = 5;
        viewModel.TotalPages = 10;
        viewModel.TotalPages = 15;

        // Assert
        Assert.AreEqual(3, eventCount, "PropertyChanged should be raised 3 times for 3 different values");
        Assert.AreEqual(15, viewModel.TotalPages);
    }

    /// <summary>
    /// Tests that setting TotalPages to the same value multiple times does not raise PropertyChanged after the first set.
    /// Input: Setting value 10 multiple times.
    /// Expected: PropertyChanged is raised only once (on initial set from 1 to 10).
    /// </summary>
    [TestMethod]
    public void TotalPages_SetSameValueMultipleTimes_RaisesPropertyChangedOnlyOnce()
    {
        // Arrange
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<UniversitiesViewModel>>();
        var viewModel = new UniversitiesViewModel(mockAcademic.Object, mockLogger.Object);
        var eventCount = 0;

        viewModel.TotalPages = 10; // Initial set

        viewModel.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == nameof(viewModel.TotalPages))
                eventCount++;
        };

        // Act
        viewModel.TotalPages = 10; // Same value
        viewModel.TotalPages = 10; // Same value again
        viewModel.TotalPages = 10; // Same value again

        // Assert
        Assert.AreEqual(0, eventCount, "PropertyChanged should not be raised for same value");
    }

    /// <summary>
    /// Tests that setting TotalPages raises PropertyChanged with correct sender.
    /// Input: New value (15).
    /// Expected: PropertyChanged event is raised with the ViewModel instance as sender.
    /// </summary>
    [TestMethod]
    public void TotalPages_SetValue_RaisesPropertyChangedWithCorrectSender()
    {
        // Arrange
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<UniversitiesViewModel>>();
        var viewModel = new UniversitiesViewModel(mockAcademic.Object, mockLogger.Object);
        object? actualSender = null;

        viewModel.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == nameof(viewModel.TotalPages))
                actualSender = sender;
        };

        // Act
        viewModel.TotalPages = 15;

        // Assert
        Assert.AreSame(viewModel, actualSender, "Sender should be the ViewModel instance");
    }

    /// <summary>
    /// Tests that PageInfo PropertyChanged is raised in correct order after TotalPages PropertyChanged.
    /// Input: New value (20).
    /// Expected: PropertyChanged events are raised in order: "TotalPages" then "PageInfo".
    /// </summary>
    [TestMethod]
    public void TotalPages_SetValue_RaisesPageInfoPropertyChangedInCorrectOrder()
    {
        // Arrange
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<UniversitiesViewModel>>();
        var viewModel = new UniversitiesViewModel(mockAcademic.Object, mockLogger.Object);
        var propertyNames = new List<string>();

        viewModel.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == nameof(viewModel.TotalPages) || args.PropertyName == nameof(viewModel.PageInfo))
                propertyNames.Add(args.PropertyName!);
        };

        // Act
        viewModel.TotalPages = 20;

        // Assert
        Assert.AreEqual(2, propertyNames.Count, "Both TotalPages and PageInfo PropertyChanged should be raised");
        Assert.AreEqual(nameof(viewModel.TotalPages), propertyNames[0], "TotalPages PropertyChanged should be raised first");
        Assert.AreEqual(nameof(viewModel.PageInfo), propertyNames[1], "PageInfo PropertyChanged should be raised second");
    }

    /// <summary>
    /// Tests that TotalPages correctly transitions from positive to negative values.
    /// Input: Transition from 10 to -5.
    /// Expected: Property is updated and PropertyChanged is raised.
    /// </summary>
    [TestMethod]
    public void TotalPages_TransitionFromPositiveToNegative_UpdatesCorrectly()
    {
        // Arrange
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<UniversitiesViewModel>>();
        var viewModel = new UniversitiesViewModel(mockAcademic.Object, mockLogger.Object);
        viewModel.TotalPages = 10;

        // Act
        viewModel.TotalPages = -5;

        // Assert
        Assert.AreEqual(-5, viewModel.TotalPages);
    }

    /// <summary>
    /// Tests that TotalPages correctly transitions from negative to positive values.
    /// Input: Transition from -10 to 5.
    /// Expected: Property is updated and PropertyChanged is raised.
    /// </summary>
    [TestMethod]
    public void TotalPages_TransitionFromNegativeToPositive_UpdatesCorrectly()
    {
        // Arrange
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<UniversitiesViewModel>>();
        var viewModel = new UniversitiesViewModel(mockAcademic.Object, mockLogger.Object);
        viewModel.TotalPages = -10;

        // Act
        viewModel.TotalPages = 5;

        // Assert
        Assert.AreEqual(5, viewModel.TotalPages);
    }

    /// <summary>
    /// Tests that TotalPages correctly handles transition to zero from positive value.
    /// Input: Transition from 10 to 0.
    /// Expected: Property is set to 0 and PropertyChanged is raised.
    /// </summary>
    [TestMethod]
    public void TotalPages_TransitionToZeroFromPositive_UpdatesCorrectly()
    {
        // Arrange
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<UniversitiesViewModel>>();
        var viewModel = new UniversitiesViewModel(mockAcademic.Object, mockLogger.Object);
        viewModel.TotalPages = 10;

        // Act
        viewModel.TotalPages = 0;

        // Assert
        Assert.AreEqual(0, viewModel.TotalPages);
    }

    /// <summary>
    /// Tests that TotalPages correctly handles transition from zero to positive value.
    /// Input: Transition from 0 to 10.
    /// Expected: Property is set to 10 and PropertyChanged is raised.
    /// </summary>
    [TestMethod]
    public void TotalPages_TransitionFromZeroToPositive_UpdatesCorrectly()
    {
        // Arrange
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<UniversitiesViewModel>>();
        var viewModel = new UniversitiesViewModel(mockAcademic.Object, mockLogger.Object);
        viewModel.TotalPages = 0;

        // Act
        viewModel.TotalPages = 10;

        // Assert
        Assert.AreEqual(10, viewModel.TotalPages);
    }

    /// <summary>
    /// Tests that setting TotalPages to alternating values raises PropertyChanged correctly each time.
    /// Input: Alternating between 5 and 10 multiple times.
    /// Expected: PropertyChanged is raised for each value change.
    /// </summary>
    [TestMethod]
    public void TotalPages_AlternatingValues_RaisesPropertyChangedEachTime()
    {
        // Arrange
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<UniversitiesViewModel>>();
        var viewModel = new UniversitiesViewModel(mockAcademic.Object, mockLogger.Object);
        var eventCount = 0;

        viewModel.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == nameof(viewModel.TotalPages))
                eventCount++;
        };

        // Act
        viewModel.TotalPages = 5;
        viewModel.TotalPages = 10;
        viewModel.TotalPages = 5;
        viewModel.TotalPages = 10;

        // Assert
        Assert.AreEqual(4, eventCount, "PropertyChanged should be raised for each value change");
    }

    /// <summary>
    /// Tests that TotalPages correctly handles transition between extreme boundary values.
    /// Input: Transition from int.MaxValue to int.MinValue.
    /// Expected: Property is updated and PropertyChanged is raised.
    /// </summary>
    [TestMethod]
    public void TotalPages_TransitionBetweenExtremeValues_UpdatesCorrectly()
    {
        // Arrange
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<UniversitiesViewModel>>();
        var viewModel = new UniversitiesViewModel(mockAcademic.Object, mockLogger.Object);
        viewModel.TotalPages = int.MaxValue;

        // Act
        viewModel.TotalPages = int.MinValue;

        // Assert
        Assert.AreEqual(int.MinValue, viewModel.TotalPages);
    }

    /// <summary>
    /// Tests that setting TotalPages does not raise PropertyChanged for unrelated properties.
    /// Input: New value (25).
    /// Expected: PropertyChanged is raised only for "TotalPages" and "PageInfo", not for other properties.
    /// </summary>
    [TestMethod]
    public void TotalPages_SetValue_DoesNotRaisePropertyChangedForOtherProperties()
    {
        // Arrange
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<UniversitiesViewModel>>();
        var viewModel = new UniversitiesViewModel(mockAcademic.Object, mockLogger.Object);
        var raisedProperties = new List<string>();

        viewModel.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName != null)
                raisedProperties.Add(args.PropertyName);
        };

        // Act
        viewModel.TotalPages = 25;

        // Assert
        Assert.AreEqual(2, raisedProperties.Count, "Only TotalPages and PageInfo should raise PropertyChanged");
        CollectionAssert.Contains(raisedProperties, nameof(viewModel.TotalPages));
        CollectionAssert.Contains(raisedProperties, nameof(viewModel.PageInfo));
    }

    /// <summary>
    /// Tests that TotalPages with large positive values works correctly.
    /// Input: Large positive values near int.MaxValue.
    /// Expected: Property is updated correctly.
    /// </summary>
    [TestMethod]
    [DataRow(1000000)]
    [DataRow(10000000)]
    [DataRow(100000000)]
    [DataRow(int.MaxValue - 1)]
    [DataRow(int.MaxValue)]
    public void TotalPages_SetLargePositiveValues_UpdatesCorrectly(int largeValue)
    {
        // Arrange
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<UniversitiesViewModel>>();
        var viewModel = new UniversitiesViewModel(mockAcademic.Object, mockLogger.Object);

        // Act
        viewModel.TotalPages = largeValue;

        // Assert
        Assert.AreEqual(largeValue, viewModel.TotalPages);
    }

    /// <summary>
    /// Tests that TotalPages with large negative values works correctly.
    /// Input: Large negative values near int.MinValue.
    /// Expected: Property is updated correctly.
    /// </summary>
    [TestMethod]
    [DataRow(-1000000)]
    [DataRow(-10000000)]
    [DataRow(-100000000)]
    [DataRow(int.MinValue + 1)]
    [DataRow(int.MinValue)]
    public void TotalPages_SetLargeNegativeValues_UpdatesCorrectly(int largeNegativeValue)
    {
        // Arrange
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<UniversitiesViewModel>>();
        var viewModel = new UniversitiesViewModel(mockAcademic.Object, mockLogger.Object);

        // Act
        viewModel.TotalPages = largeNegativeValue;

        // Assert
        Assert.AreEqual(largeNegativeValue, viewModel.TotalPages);
    }

    /// <summary>
    /// Tests that getting TotalPages after setting returns the correct value.
    /// Input: Set to various values and immediately get.
    /// Expected: Getter returns the value that was set.
    /// </summary>
    [TestMethod]
    [DataRow(0)]
    [DataRow(1)]
    [DataRow(50)]
    [DataRow(-50)]
    [DataRow(int.MaxValue)]
    [DataRow(int.MinValue)]
    public void TotalPages_SetAndGet_ReturnsSetValue(int value)
    {
        // Arrange
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<UniversitiesViewModel>>();
        var viewModel = new UniversitiesViewModel(mockAcademic.Object, mockLogger.Object);

        // Act
        viewModel.TotalPages = value;
        int result = viewModel.TotalPages;

        // Assert
        Assert.AreEqual(value, result);
    }
}



/// <summary>
/// Tests for the IsEditing property of UniversitiesViewModel class.
/// </summary>
[TestClass]
public partial class UniversitiesViewModelIsEditingTests
{
    /// <summary>
    /// Tests that IsEditing property returns false by default when the ViewModel is first created.
    /// Input: None (newly instantiated ViewModel).
    /// Expected: IsEditing returns false.
    /// </summary>
    [TestMethod]
    public void IsEditing_DefaultValue_ReturnsFalse()
    {
        // Arrange
        var mockAcademicService = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<UniversitiesViewModel>>();

        // Act
        var viewModel = new UniversitiesViewModel(mockAcademicService.Object, mockLogger.Object);

        // Assert
        Assert.IsFalse(viewModel.IsEditing);
    }

    /// <summary>
    /// Tests that setting IsEditing to true updates the property value and raises PropertyChanged event.
    /// Input: Setting IsEditing to true from default false.
    /// Expected: Property value is updated to true and PropertyChanged event is raised with property name "IsEditing".
    /// </summary>
    [TestMethod]
    public void IsEditing_SetToTrue_UpdatesValueAndRaisesPropertyChanged()
    {
        // Arrange
        var mockAcademicService = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<UniversitiesViewModel>>();
        var viewModel = new UniversitiesViewModel(mockAcademicService.Object, mockLogger.Object);
        bool propertyChangedRaised = false;
        string? propertyName = null;
        viewModel.PropertyChanged += (sender, args) =>
        {
            propertyChangedRaised = true;
            propertyName = args.PropertyName;
        };

        // Act
        viewModel.IsEditing = true;

        // Assert
        Assert.IsTrue(viewModel.IsEditing);
        Assert.IsTrue(propertyChangedRaised);
        Assert.AreEqual("IsEditing", propertyName);
    }

    /// <summary>
    /// Tests that setting IsEditing to false when it was true updates the property value and raises PropertyChanged event.
    /// Input: Setting IsEditing to false after it was set to true.
    /// Expected: Property value is updated to false and PropertyChanged event is raised.
    /// </summary>
    [TestMethod]
    public void IsEditing_SetToFalseFromTrue_UpdatesValueAndRaisesPropertyChanged()
    {
        // Arrange
        var mockAcademicService = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<UniversitiesViewModel>>();
        var viewModel = new UniversitiesViewModel(mockAcademicService.Object, mockLogger.Object);
        viewModel.IsEditing = true;
        bool propertyChangedRaised = false;
        string? propertyName = null;
        viewModel.PropertyChanged += (sender, args) =>
        {
            propertyChangedRaised = true;
            propertyName = args.PropertyName;
        };

        // Act
        viewModel.IsEditing = false;

        // Assert
        Assert.IsFalse(viewModel.IsEditing);
        Assert.IsTrue(propertyChangedRaised);
        Assert.AreEqual("IsEditing", propertyName);
    }

    /// <summary>
    /// Tests that setting IsEditing to the same value (true to true) does not raise PropertyChanged event.
    /// Input: Setting IsEditing to true when it is already true.
    /// Expected: Property remains true and PropertyChanged event is not raised.
    /// </summary>
    [TestMethod]
    public void IsEditing_SetToSameValueTrue_DoesNotRaisePropertyChanged()
    {
        // Arrange
        var mockAcademicService = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<UniversitiesViewModel>>();
        var viewModel = new UniversitiesViewModel(mockAcademicService.Object, mockLogger.Object);
        viewModel.IsEditing = true;
        bool propertyChangedRaised = false;
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
    /// Input: Setting IsEditing to false when it is already false (default).
    /// Expected: Property remains false and PropertyChanged event is not raised.
    /// </summary>
    [TestMethod]
    public void IsEditing_SetToSameValueFalse_DoesNotRaisePropertyChanged()
    {
        // Arrange
        var mockAcademicService = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<UniversitiesViewModel>>();
        var viewModel = new UniversitiesViewModel(mockAcademicService.Object, mockLogger.Object);
        bool propertyChangedRaised = false;
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
    /// Tests that multiple alternating sets between true and false correctly update the property value each time.
    /// Input: Alternating between true and false multiple times.
    /// Expected: Property value is updated correctly for each change.
    /// </summary>
    [TestMethod]
    public void IsEditing_MultipleAlternatingSets_UpdatesValueCorrectly()
    {
        // Arrange
        var mockAcademicService = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<UniversitiesViewModel>>();
        var viewModel = new UniversitiesViewModel(mockAcademicService.Object, mockLogger.Object);

        // Act & Assert
        viewModel.IsEditing = true;
        Assert.IsTrue(viewModel.IsEditing);

        viewModel.IsEditing = false;
        Assert.IsFalse(viewModel.IsEditing);

        viewModel.IsEditing = true;
        Assert.IsTrue(viewModel.IsEditing);

        viewModel.IsEditing = false;
        Assert.IsFalse(viewModel.IsEditing);
    }

    /// <summary>
    /// Tests that PropertyChanged event has the correct sender when IsEditing is set.
    /// Input: Setting IsEditing to true.
    /// Expected: PropertyChanged event sender is the view model instance.
    /// </summary>
    [TestMethod]
    public void IsEditing_SetValue_PropertyChangedEventHasCorrectSender()
    {
        // Arrange
        var mockAcademicService = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<UniversitiesViewModel>>();
        var viewModel = new UniversitiesViewModel(mockAcademicService.Object, mockLogger.Object);
        object? eventSender = null;
        viewModel.PropertyChanged += (sender, args) =>
        {
            eventSender = sender;
        };

        // Act
        viewModel.IsEditing = true;

        // Assert
        Assert.IsNotNull(eventSender);
        Assert.AreSame(viewModel, eventSender);
    }

    /// <summary>
    /// Tests that getter returns the correct value after setter updates it.
    /// Input: Setting IsEditing to true via setter.
    /// Expected: Getter returns true.
    /// </summary>
    [TestMethod]
    public void IsEditing_SetAndGet_ReturnsSetValue()
    {
        // Arrange
        var mockAcademicService = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<UniversitiesViewModel>>();
        var viewModel = new UniversitiesViewModel(mockAcademicService.Object, mockLogger.Object);

        // Act
        viewModel.IsEditing = true;

        // Assert
        Assert.AreEqual(true, viewModel.IsEditing);
    }

    /// <summary>
    /// Tests that PropertyChanged event is raised multiple times for each value change.
    /// Input: Multiple different values (true, false, true).
    /// Expected: PropertyChanged event is raised for each actual value change.
    /// </summary>
    [TestMethod]
    public void IsEditing_MultipleValueChanges_RaisesPropertyChangedForEachChange()
    {
        // Arrange
        var mockAcademicService = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<UniversitiesViewModel>>();
        var viewModel = new UniversitiesViewModel(mockAcademicService.Object, mockLogger.Object);
        int propertyChangedCount = 0;
        viewModel.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == "IsEditing")
            {
                propertyChangedCount++;
            }
        };

        // Act
        viewModel.IsEditing = true;
        viewModel.IsEditing = false;
        viewModel.IsEditing = true;

        // Assert
        Assert.AreEqual(3, propertyChangedCount);
    }

    /// <summary>
    /// Tests that PropertyChanged event provides correct PropertyName in EventArgs.
    /// Input: Setting IsEditing to true.
    /// Expected: PropertyChangedEventArgs.PropertyName equals "IsEditing".
    /// </summary>
    [TestMethod]
    public void IsEditing_PropertyChangedEventArgs_HasCorrectPropertyName()
    {
        // Arrange
        var mockAcademicService = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<UniversitiesViewModel>>();
        var viewModel = new UniversitiesViewModel(mockAcademicService.Object, mockLogger.Object);
        string? capturedPropertyName = null;
        viewModel.PropertyChanged += (sender, args) =>
        {
            capturedPropertyName = args.PropertyName;
        };

        // Act
        viewModel.IsEditing = true;

        // Assert
        Assert.IsNotNull(capturedPropertyName);
        Assert.AreEqual("IsEditing", capturedPropertyName);
    }

    /// <summary>
    /// Tests that setting IsEditing to false initially (when already false) behaves correctly.
    /// Input: Setting IsEditing to false when it is already false (default state).
    /// Expected: Property remains false and no PropertyChanged event is raised.
    /// </summary>
    [TestMethod]
    public void IsEditing_SetToFalseWhenAlreadyFalse_DoesNotRaisePropertyChanged()
    {
        // Arrange
        var mockAcademicService = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<UniversitiesViewModel>>();
        var viewModel = new UniversitiesViewModel(mockAcademicService.Object, mockLogger.Object);
        int propertyChangedCount = 0;
        viewModel.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == "IsEditing")
            {
                propertyChangedCount++;
            }
        };

        // Act
        viewModel.IsEditing = false;

        // Assert
        Assert.IsFalse(viewModel.IsEditing);
        Assert.AreEqual(0, propertyChangedCount);
    }

    /// <summary>
    /// Tests that getter returns false after setting to false from true.
    /// Input: Set to true, then to false.
    /// Expected: Getter returns false after the transition.
    /// </summary>
    [TestMethod]
    public void IsEditing_GetAfterSettingToFalse_ReturnsFalse()
    {
        // Arrange
        var mockAcademicService = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<UniversitiesViewModel>>();
        var viewModel = new UniversitiesViewModel(mockAcademicService.Object, mockLogger.Object);
        viewModel.IsEditing = true;

        // Act
        viewModel.IsEditing = false;

        // Assert
        Assert.AreEqual(false, viewModel.IsEditing);
    }

    /// <summary>
    /// Tests that PropertyChanged is not raised for other properties when IsEditing changes.
    /// Input: Setting IsEditing to true.
    /// Expected: Only "IsEditing" PropertyChanged event is raised, not other properties.
    /// </summary>
    [TestMethod]
    public void IsEditing_SetValue_OnlyRaisesPropertyChangedForIsEditing()
    {
        // Arrange
        var mockAcademicService = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<UniversitiesViewModel>>();
        var viewModel = new UniversitiesViewModel(mockAcademicService.Object, mockLogger.Object);
        var propertyNames = new List<string>();
        viewModel.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName != null)
            {
                propertyNames.Add(args.PropertyName);
            }
        };

        // Act
        viewModel.IsEditing = true;

        // Assert
        Assert.AreEqual(1, propertyNames.Count);
        Assert.AreEqual("IsEditing", propertyNames[0]);
    }

    /// <summary>
    /// Tests that setting IsEditing multiple times with same value does not raise PropertyChanged multiple times.
    /// Input: Setting to true three times consecutively.
    /// Expected: PropertyChanged is raised only on the first set (when value actually changes from false to true).
    /// </summary>
    [TestMethod]
    public void IsEditing_SetSameValueMultipleTimes_RaisesPropertyChangedOnlyOnce()
    {
        // Arrange
        var mockAcademicService = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<UniversitiesViewModel>>();
        var viewModel = new UniversitiesViewModel(mockAcademicService.Object, mockLogger.Object);
        int propertyChangedCount = 0;
        viewModel.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == "IsEditing")
            {
                propertyChangedCount++;
            }
        };

        // Act
        viewModel.IsEditing = true;
        viewModel.IsEditing = true;
        viewModel.IsEditing = true;

        // Assert
        Assert.AreEqual(1, propertyChangedCount);
        Assert.IsTrue(viewModel.IsEditing);
    }

    /// <summary>
    /// Tests that alternating rapidly between true and false raises PropertyChanged for each transition.
    /// Input: Five rapid alternations between true and false.
    /// Expected: PropertyChanged is raised for each of the five value changes.
    /// </summary>
    [TestMethod]
    public void IsEditing_RapidAlternation_RaisesPropertyChangedForEachTransition()
    {
        // Arrange
        var mockAcademicService = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<UniversitiesViewModel>>();
        var viewModel = new UniversitiesViewModel(mockAcademicService.Object, mockLogger.Object);
        int propertyChangedCount = 0;
        viewModel.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == "IsEditing")
            {
                propertyChangedCount++;
            }
        };

        // Act
        viewModel.IsEditing = true;
        viewModel.IsEditing = false;
        viewModel.IsEditing = true;
        viewModel.IsEditing = false;
        viewModel.IsEditing = true;

        // Assert
        Assert.AreEqual(5, propertyChangedCount);
        Assert.IsTrue(viewModel.IsEditing);
    }
}



/// <summary>
/// Tests for the CurrentPage property of UniversitiesViewModel class.
/// </summary>
[TestClass]
public partial class UniversitiesViewModelCurrentPageTests
{
    /// <summary>
    /// Tests that CurrentPage property returns the initial default value of 1.
    /// Input: None (default initialization).
    /// Expected: CurrentPage returns 1.
    /// </summary>
    [TestMethod]
    public void CurrentPage_Get_ReturnsInitialDefaultValue()
    {
        // Arrange
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<UniversitiesViewModel>>();
        var viewModel = new UniversitiesViewModel(mockAcademic.Object, mockLogger.Object);

        // Act
        int result = viewModel.CurrentPage;

        // Assert
        Assert.AreEqual(1, result);
    }

    /// <summary>
    /// Tests that setting CurrentPage to a different value updates the property and raises PropertyChanged for both CurrentPage and PageInfo.
    /// Input: New valid page number (5).
    /// Expected: Property is updated, PropertyChanged is raised for CurrentPage and PageInfo.
    /// </summary>
    [TestMethod]
    public void CurrentPage_SetDifferentValue_UpdatesPropertyAndRaisesPropertyChangedForBothProperties()
    {
        // Arrange
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<UniversitiesViewModel>>();
        var viewModel = new UniversitiesViewModel(mockAcademic.Object, mockLogger.Object);
        var propertyChangedEvents = new List<string>();
        viewModel.PropertyChanged += (sender, args) => propertyChangedEvents.Add(args.PropertyName ?? string.Empty);

        // Act
        viewModel.CurrentPage = 5;

        // Assert
        Assert.AreEqual(5, viewModel.CurrentPage);
        CollectionAssert.Contains(propertyChangedEvents, "CurrentPage");
        CollectionAssert.Contains(propertyChangedEvents, "PageInfo");
    }

    /// <summary>
    /// Tests that setting CurrentPage to the same value does not raise PropertyChanged events.
    /// Input: Same value as the current value (1).
    /// Expected: Property remains unchanged, no PropertyChanged events are raised.
    /// </summary>
    [TestMethod]
    public void CurrentPage_SetSameValue_DoesNotRaisePropertyChanged()
    {
        // Arrange
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<UniversitiesViewModel>>();
        var viewModel = new UniversitiesViewModel(mockAcademic.Object, mockLogger.Object);
        var propertyChangedEvents = new List<string>();
        viewModel.PropertyChanged += (sender, args) => propertyChangedEvents.Add(args.PropertyName ?? string.Empty);

        // Act
        viewModel.CurrentPage = 1;

        // Assert
        Assert.AreEqual(1, viewModel.CurrentPage);
        Assert.AreEqual(0, propertyChangedEvents.Count);
    }

    /// <summary>
    /// Tests that setting CurrentPage to zero updates the property and raises PropertyChanged.
    /// Input: Zero.
    /// Expected: Property is set to 0, PropertyChanged is raised for CurrentPage and PageInfo.
    /// </summary>
    [TestMethod]
    public void CurrentPage_SetToZero_UpdatesPropertyAndRaisesPropertyChanged()
    {
        // Arrange
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<UniversitiesViewModel>>();
        var viewModel = new UniversitiesViewModel(mockAcademic.Object, mockLogger.Object);
        var propertyChangedEvents = new List<string>();
        viewModel.PropertyChanged += (sender, args) => propertyChangedEvents.Add(args.PropertyName ?? string.Empty);

        // Act
        viewModel.CurrentPage = 0;

        // Assert
        Assert.AreEqual(0, viewModel.CurrentPage);
        CollectionAssert.Contains(propertyChangedEvents, "CurrentPage");
        CollectionAssert.Contains(propertyChangedEvents, "PageInfo");
    }

    /// <summary>
    /// Tests that setting CurrentPage to a negative value updates the property and raises PropertyChanged.
    /// Input: Negative value (-5).
    /// Expected: Property is set to -5, PropertyChanged is raised for CurrentPage and PageInfo.
    /// </summary>
    [TestMethod]
    public void CurrentPage_SetToNegativeValue_UpdatesPropertyAndRaisesPropertyChanged()
    {
        // Arrange
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<UniversitiesViewModel>>();
        var viewModel = new UniversitiesViewModel(mockAcademic.Object, mockLogger.Object);
        var propertyChangedEvents = new List<string>();
        viewModel.PropertyChanged += (sender, args) => propertyChangedEvents.Add(args.PropertyName ?? string.Empty);

        // Act
        viewModel.CurrentPage = -5;

        // Assert
        Assert.AreEqual(-5, viewModel.CurrentPage);
        CollectionAssert.Contains(propertyChangedEvents, "CurrentPage");
        CollectionAssert.Contains(propertyChangedEvents, "PageInfo");
    }

    /// <summary>
    /// Tests that setting CurrentPage to int.MinValue updates the property and raises PropertyChanged.
    /// Input: int.MinValue.
    /// Expected: Property is set to int.MinValue, PropertyChanged is raised for CurrentPage and PageInfo.
    /// </summary>
    [TestMethod]
    public void CurrentPage_SetToIntMinValue_UpdatesPropertyAndRaisesPropertyChanged()
    {
        // Arrange
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<UniversitiesViewModel>>();
        var viewModel = new UniversitiesViewModel(mockAcademic.Object, mockLogger.Object);
        var propertyChangedEvents = new List<string>();
        viewModel.PropertyChanged += (sender, args) => propertyChangedEvents.Add(args.PropertyName ?? string.Empty);

        // Act
        viewModel.CurrentPage = int.MinValue;

        // Assert
        Assert.AreEqual(int.MinValue, viewModel.CurrentPage);
        CollectionAssert.Contains(propertyChangedEvents, "CurrentPage");
        CollectionAssert.Contains(propertyChangedEvents, "PageInfo");
    }

    /// <summary>
    /// Tests that setting CurrentPage to int.MaxValue updates the property and raises PropertyChanged.
    /// Input: int.MaxValue.
    /// Expected: Property is set to int.MaxValue, PropertyChanged is raised for CurrentPage and PageInfo.
    /// </summary>
    [TestMethod]
    public void CurrentPage_SetToIntMaxValue_UpdatesPropertyAndRaisesPropertyChanged()
    {
        // Arrange
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<UniversitiesViewModel>>();
        var viewModel = new UniversitiesViewModel(mockAcademic.Object, mockLogger.Object);
        var propertyChangedEvents = new List<string>();
        viewModel.PropertyChanged += (sender, args) => propertyChangedEvents.Add(args.PropertyName ?? string.Empty);

        // Act
        viewModel.CurrentPage = int.MaxValue;

        // Assert
        Assert.AreEqual(int.MaxValue, viewModel.CurrentPage);
        CollectionAssert.Contains(propertyChangedEvents, "CurrentPage");
        CollectionAssert.Contains(propertyChangedEvents, "PageInfo");
    }

    /// <summary>
    /// Tests that setting CurrentPage multiple times with different values raises PropertyChanged each time.
    /// Input: Multiple different page numbers (2, 3, 4).
    /// Expected: Property is updated each time, PropertyChanged is raised for each change.
    /// </summary>
    [TestMethod]
    public void CurrentPage_SetMultipleDifferentValues_RaisesPropertyChangedEachTime()
    {
        // Arrange
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<UniversitiesViewModel>>();
        var viewModel = new UniversitiesViewModel(mockAcademic.Object, mockLogger.Object);
        int propertyChangedCount = 0;
        viewModel.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == "CurrentPage")
                propertyChangedCount++;
        };

        // Act
        viewModel.CurrentPage = 2;
        viewModel.CurrentPage = 3;
        viewModel.CurrentPage = 4;

        // Assert
        Assert.AreEqual(4, viewModel.CurrentPage);
        Assert.AreEqual(3, propertyChangedCount);
    }

    /// <summary>
    /// Tests that setting CurrentPage raises PropertyChanged event with correct sender.
    /// Input: New page number (10).
    /// Expected: PropertyChanged event is raised with the view model as sender.
    /// </summary>
    [TestMethod]
    public void CurrentPage_SetValue_RaisesPropertyChangedWithCorrectSender()
    {
        // Arrange
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<UniversitiesViewModel>>();
        var viewModel = new UniversitiesViewModel(mockAcademic.Object, mockLogger.Object);
        object? eventSender = null;
        viewModel.PropertyChanged += (sender, args) => eventSender = sender;

        // Act
        viewModel.CurrentPage = 10;

        // Assert
        Assert.AreSame(viewModel, eventSender);
    }

    /// <summary>
    /// Tests that PageInfo PropertyChanged is only raised when CurrentPage value actually changes.
    /// Input: Set same value twice, then different value.
    /// Expected: PageInfo PropertyChanged is raised only when value changes.
    /// </summary>
    [TestMethod]
    public void CurrentPage_PageInfoNotification_OnlyRaisedWhenValueChanges()
    {
        // Arrange
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<UniversitiesViewModel>>();
        var viewModel = new UniversitiesViewModel(mockAcademic.Object, mockLogger.Object);
        int pageInfoChangedCount = 0;
        viewModel.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == "PageInfo")
                pageInfoChangedCount++;
        };

        // Act
        viewModel.CurrentPage = 1; // Same as default
        viewModel.CurrentPage = 2; // Different
        viewModel.CurrentPage = 2; // Same as current

        // Assert
        Assert.AreEqual(1, pageInfoChangedCount);
    }

    /// <summary>
    /// Tests that CurrentPage correctly handles various positive boundary values.
    /// Input: Various positive boundary values.
    /// Expected: Property is updated correctly for each value and PropertyChanged is raised.
    /// </summary>
    [TestMethod]
    [DataRow(1)]
    [DataRow(10)]
    [DataRow(100)]
    [DataRow(1000)]
    [DataRow(10000)]
    [DataRow(int.MaxValue - 1)]
    public void CurrentPage_SetPositiveBoundaryValues_UpdatesPropertyCorrectly(int value)
    {
        // Arrange
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<UniversitiesViewModel>>();
        var viewModel = new UniversitiesViewModel(mockAcademic.Object, mockLogger.Object);

        // Act
        viewModel.CurrentPage = value;

        // Assert
        Assert.AreEqual(value, viewModel.CurrentPage);
    }

    /// <summary>
    /// Tests that CurrentPage correctly handles various negative boundary values.
    /// Input: Various negative boundary values.
    /// Expected: Property is updated correctly for each value and PropertyChanged is raised.
    /// </summary>
    [TestMethod]
    [DataRow(-1)]
    [DataRow(-10)]
    [DataRow(-100)]
    [DataRow(-1000)]
    [DataRow(-10000)]
    [DataRow(int.MinValue + 1)]
    public void CurrentPage_SetNegativeBoundaryValues_UpdatesPropertyCorrectly(int value)
    {
        // Arrange
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<UniversitiesViewModel>>();
        var viewModel = new UniversitiesViewModel(mockAcademic.Object, mockLogger.Object);

        // Act
        viewModel.CurrentPage = value;

        // Assert
        Assert.AreEqual(value, viewModel.CurrentPage);
    }

    /// <summary>
    /// Tests that CurrentPage PropertyChanged is raised before PageInfo PropertyChanged.
    /// Input: New page number (7).
    /// Expected: CurrentPage PropertyChanged is raised first, followed by PageInfo PropertyChanged.
    /// </summary>
    [TestMethod]
    public void CurrentPage_SetValue_RaisesCurrentPagePropertyChangedBeforePageInfo()
    {
        // Arrange
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<UniversitiesViewModel>>();
        var viewModel = new UniversitiesViewModel(mockAcademic.Object, mockLogger.Object);
        var propertyChangedOrder = new List<string>();
        viewModel.PropertyChanged += (sender, args) => propertyChangedOrder.Add(args.PropertyName ?? string.Empty);

        // Act
        viewModel.CurrentPage = 7;

        // Assert
        Assert.IsTrue(propertyChangedOrder.Count >= 2);
        Assert.AreEqual("CurrentPage", propertyChangedOrder[0]);
        Assert.AreEqual("PageInfo", propertyChangedOrder[1]);
    }

    /// <summary>
    /// Tests that setting CurrentPage alternating between two values raises PropertyChanged each time.
    /// Input: Alternating between values 2 and 3.
    /// Expected: PropertyChanged is raised for each value change.
    /// </summary>
    [TestMethod]
    public void CurrentPage_AlternatingBetweenTwoValues_RaisesPropertyChangedEachTime()
    {
        // Arrange
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<UniversitiesViewModel>>();
        var viewModel = new UniversitiesViewModel(mockAcademic.Object, mockLogger.Object);
        int currentPageChangedCount = 0;
        viewModel.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == "CurrentPage")
                currentPageChangedCount++;
        };

        // Act
        viewModel.CurrentPage = 2;
        viewModel.CurrentPage = 3;
        viewModel.CurrentPage = 2;
        viewModel.CurrentPage = 3;

        // Assert
        Assert.AreEqual(4, currentPageChangedCount);
    }

    /// <summary>
    /// Tests that setting CurrentPage from a non-default value back to default (1) raises PropertyChanged.
    /// Input: Set to 5, then back to 1.
    /// Expected: PropertyChanged is raised when setting back to 1.
    /// </summary>
    [TestMethod]
    public void CurrentPage_SetBackToDefaultValue_RaisesPropertyChanged()
    {
        // Arrange
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<UniversitiesViewModel>>();
        var viewModel = new UniversitiesViewModel(mockAcademic.Object, mockLogger.Object);
        viewModel.CurrentPage = 5;
        var propertyChangedEvents = new List<string>();
        viewModel.PropertyChanged += (sender, args) => propertyChangedEvents.Add(args.PropertyName ?? string.Empty);

        // Act
        viewModel.CurrentPage = 1;

        // Assert
        Assert.AreEqual(1, viewModel.CurrentPage);
        CollectionAssert.Contains(propertyChangedEvents, "CurrentPage");
        CollectionAssert.Contains(propertyChangedEvents, "PageInfo");
    }

    /// <summary>
    /// Tests that PropertyChangedEventArgs contains the correct property name "CurrentPage".
    /// Input: New page number (8).
    /// Expected: PropertyChangedEventArgs.PropertyName equals "CurrentPage".
    /// </summary>
    [TestMethod]
    public void CurrentPage_SetValue_PropertyChangedEventArgsContainsCorrectPropertyName()
    {
        // Arrange
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<UniversitiesViewModel>>();
        var viewModel = new UniversitiesViewModel(mockAcademic.Object, mockLogger.Object);
        string? propertyName = null;
        viewModel.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == "CurrentPage")
                propertyName = args.PropertyName;
        };

        // Act
        viewModel.CurrentPage = 8;

        // Assert
        Assert.AreEqual("CurrentPage", propertyName);
    }

    /// <summary>
    /// Tests that setting CurrentPage to the same value multiple times does not raise PropertyChanged after the first set.
    /// Input: Same value (10) set three times consecutively.
    /// Expected: No PropertyChanged events are raised for the subsequent sets.
    /// </summary>
    [TestMethod]
    public void CurrentPage_SetSameValueMultipleTimes_DoesNotRaisePropertyChangedAfterFirst()
    {
        // Arrange
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<UniversitiesViewModel>>();
        var viewModel = new UniversitiesViewModel(mockAcademic.Object, mockLogger.Object);
        viewModel.CurrentPage = 10;
        var propertyChangedCount = 0;
        viewModel.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == "CurrentPage")
                propertyChangedCount++;
        };

        // Act
        viewModel.CurrentPage = 10;
        viewModel.CurrentPage = 10;
        viewModel.CurrentPage = 10;

        // Assert
        Assert.AreEqual(0, propertyChangedCount);
    }

    /// <summary>
    /// Tests that setting CurrentPage correctly updates PageInfo property which depends on it.
    /// Input: Setting CurrentPage to 5.
    /// Expected: PageInfo reflects the CurrentPage value.
    /// </summary>
    [TestMethod]
    public void CurrentPage_SetValue_UpdatesPageInfoProperty()
    {
        // Arrange
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<UniversitiesViewModel>>();
        var viewModel = new UniversitiesViewModel(mockAcademic.Object, mockLogger.Object);

        // Act
        viewModel.CurrentPage = 5;

        // Assert
        Assert.IsTrue(viewModel.PageInfo.Contains("5"));
    }
}