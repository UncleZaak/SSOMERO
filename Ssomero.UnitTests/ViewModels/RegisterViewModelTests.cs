using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

using CommunityToolkit.Mvvm;
using CommunityToolkit.Mvvm.Input;
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
/// Unit tests for the RegisterViewModel class.
/// </summary>
[TestClass]
public class RegisterViewModelTests
{
    /// <summary>
    /// Tests that IsStep3 returns true when CurrentStep is set to 3.
    /// </summary>
    [TestMethod]
    public void IsStep3_WhenCurrentStepIs3_ReturnsTrue()
    {
        // Arrange
        var mockAuth = new Mock<IAuthService>();
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<RegisterViewModel>>();
        var viewModel = new RegisterViewModel(mockAuth.Object, mockAcademic.Object, mockLogger.Object);

        viewModel.CurrentStep = 3;

        // Act
        bool result = viewModel.IsStep3;

        // Assert
        Assert.IsTrue(result);
    }

    /// <summary>
    /// Tests that IsStep3 returns false when CurrentStep is not equal to 3.
    /// Tests various edge cases including boundary values, negative numbers, and typical step values.
    /// </summary>
    /// <param name="currentStep">The value to set for CurrentStep property.</param>
    [TestMethod]
    [DataRow(1)]
    [DataRow(2)]
    [DataRow(4)]
    [DataRow(0)]
    [DataRow(-1)]
    [DataRow(int.MinValue)]
    [DataRow(int.MaxValue)]
    public void IsStep3_WhenCurrentStepIsNot3_ReturnsFalse(int currentStep)
    {
        // Arrange
        var mockAuth = new Mock<IAuthService>();
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<RegisterViewModel>>();
        var viewModel = new RegisterViewModel(mockAuth.Object, mockAcademic.Object, mockLogger.Object);

        viewModel.CurrentStep = currentStep;

        // Act
        bool result = viewModel.IsStep3;

        // Assert
        Assert.IsFalse(result);
    }

    /// <summary>
    /// Tests that the Gender property returns an empty string as its initial value.
    /// </summary>
    [TestMethod]
    public void Gender_GetInitialValue_ReturnsEmptyString()
    {
        // Arrange
        var authMock = new Mock<IAuthService>();
        var academicMock = new Mock<IAcademicService>();
        var loggerMock = new Mock<ILogger<RegisterViewModel>>();
        var viewModel = new RegisterViewModel(authMock.Object, academicMock.Object, loggerMock.Object);

        // Act
        var result = viewModel.Gender;

        // Assert
        Assert.AreEqual(string.Empty, result);
    }

    /// <summary>
    /// Tests that setting the Gender property with various valid string values updates the property correctly.
    /// Tests normal values, empty strings, whitespace, special characters, and very long strings.
    /// </summary>
    /// <param name="value">The value to set on the Gender property.</param>
    [TestMethod]
    [DataRow("Male")]
    [DataRow("Female")]
    [DataRow("")]
    [DataRow(" ")]
    [DataRow("   \t\n")]
    [DataRow("Non-binary")]
    [DataRow("Other")]
    [DataRow("!@#$%^&*()")]
    [DataRow("Gender with special chars: é, ñ, ü")]
    [DataRow("abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789")]
    public void Gender_SetValidValue_UpdatesProperty(string value)
    {
        // Arrange
        var authMock = new Mock<IAuthService>();
        var academicMock = new Mock<IAcademicService>();
        var loggerMock = new Mock<ILogger<RegisterViewModel>>();
        var viewModel = new RegisterViewModel(authMock.Object, academicMock.Object, loggerMock.Object);

        // Act
        viewModel.Gender = value;

        // Assert
        Assert.AreEqual(value, viewModel.Gender);
    }

    /// <summary>
    /// Tests that setting the Gender property raises the PropertyChanged event with the correct property name.
    /// </summary>
    [TestMethod]
    public void Gender_SetValue_RaisesPropertyChangedEvent()
    {
        // Arrange
        var authMock = new Mock<IAuthService>();
        var academicMock = new Mock<IAcademicService>();
        var loggerMock = new Mock<ILogger<RegisterViewModel>>();
        var viewModel = new RegisterViewModel(authMock.Object, academicMock.Object, loggerMock.Object);
        string? raisedPropertyName = null;
        viewModel.PropertyChanged += (sender, args) => raisedPropertyName = args.PropertyName;

        // Act
        viewModel.Gender = "Male";

        // Assert
        Assert.AreEqual("Gender", raisedPropertyName);
    }

    /// <summary>
    /// Tests that setting the Gender property to the same value does not raise PropertyChanged event.
    /// This verifies that SetProperty correctly checks for value equality before raising the event.
    /// </summary>
    [TestMethod]
    public void Gender_SetSameValue_DoesNotRaisePropertyChangedEvent()
    {
        // Arrange
        var authMock = new Mock<IAuthService>();
        var academicMock = new Mock<IAcademicService>();
        var loggerMock = new Mock<ILogger<RegisterViewModel>>();
        var viewModel = new RegisterViewModel(authMock.Object, academicMock.Object, loggerMock.Object);
        viewModel.Gender = "Female";
        int eventRaisedCount = 0;
        viewModel.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == "Gender")
            {
                eventRaisedCount++;
            }
        };

        // Act
        viewModel.Gender = "Female";

        // Assert
        Assert.AreEqual(0, eventRaisedCount);
    }

    /// <summary>
    /// Tests that setting the Gender property multiple times with different values correctly updates the property each time.
    /// </summary>
    [TestMethod]
    public void Gender_SetMultipleDifferentValues_UpdatesPropertyEachTime()
    {
        // Arrange
        var authMock = new Mock<IAuthService>();
        var academicMock = new Mock<IAcademicService>();
        var loggerMock = new Mock<ILogger<RegisterViewModel>>();
        var viewModel = new RegisterViewModel(authMock.Object, academicMock.Object, loggerMock.Object);

        // Act & Assert
        viewModel.Gender = "Male";
        Assert.AreEqual("Male", viewModel.Gender);

        viewModel.Gender = "Female";
        Assert.AreEqual("Female", viewModel.Gender);

        viewModel.Gender = "Other";
        Assert.AreEqual("Other", viewModel.Gender);

        viewModel.Gender = "";
        Assert.AreEqual("", viewModel.Gender);
    }

    /// <summary>
    /// Tests that setting the Gender property with control characters is handled correctly.
    /// </summary>
    [TestMethod]
    public void Gender_SetValueWithControlCharacters_UpdatesProperty()
    {
        // Arrange
        var authMock = new Mock<IAuthService>();
        var academicMock = new Mock<IAcademicService>();
        var loggerMock = new Mock<ILogger<RegisterViewModel>>();
        var viewModel = new RegisterViewModel(authMock.Object, academicMock.Object, loggerMock.Object);
        var valueWithControlChars = "Gender\0\r\n\t";

        // Act
        viewModel.Gender = valueWithControlChars;

        // Assert
        Assert.AreEqual(valueWithControlChars, viewModel.Gender);
    }

    /// <summary>
    /// Tests that setting the Gender property with Unicode characters is handled correctly.
    /// </summary>
    [TestMethod]
    public void Gender_SetValueWithUnicodeCharacters_UpdatesProperty()
    {
        // Arrange
        var authMock = new Mock<IAuthService>();
        var academicMock = new Mock<IAcademicService>();
        var loggerMock = new Mock<ILogger<RegisterViewModel>>();
        var viewModel = new RegisterViewModel(authMock.Object, academicMock.Object, loggerMock.Object);
        var unicodeValue = "性别 🚻 ♂️ ♀️";

        // Act
        viewModel.Gender = unicodeValue;

        // Assert
        Assert.AreEqual(unicodeValue, viewModel.Gender);
    }

    /// <summary>
    /// Tests that setting SelectedFaculty to a non-null value updates the property,
    /// clears dependent collections, sets SelectedDepartment to null, and triggers LoadDepartmentsAsync.
    /// </summary>
    [TestMethod]
    public void SelectedFaculty_SetToNonNullValue_UpdatesPropertyAndClearsDependentCollections()
    {
        // Arrange
        var mockAuth = new Mock<IAuthService>();
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<RegisterViewModel>>();
        var viewModel = new RegisterViewModel(mockAuth.Object, mockAcademic.Object, mockLogger.Object);

        var faculty = new LookupItem { Id = "faculty-1", Name = "Faculty of Science" };
        var department1 = new LookupItem { Id = "dept-1", Name = "Department 1" };
        var program1 = new LookupItem { Id = "prog-1", Name = "Program 1" };

        viewModel.Departments.Add(department1);
        viewModel.Programs.Add(program1);
        viewModel.SelectedDepartment = new LookupItem { Id = "dept-2", Name = "Department 2" };

        // Act
        viewModel.SelectedFaculty = faculty;

        // Assert
        Assert.AreEqual(faculty, viewModel.SelectedFaculty);
        Assert.IsNull(viewModel.SelectedDepartment);
        Assert.AreEqual(0, viewModel.Departments.Count);
        Assert.AreEqual(0, viewModel.Programs.Count);
    }

    /// <summary>
    /// Tests that setting SelectedFaculty to null updates the property,
    /// clears dependent collections, sets SelectedDepartment to null, but does not trigger LoadDepartmentsAsync.
    /// </summary>
    [TestMethod]
    public void SelectedFaculty_SetToNull_UpdatesPropertyAndClearsDependentCollectionsWithoutLoadingDepartments()
    {
        // Arrange
        var mockAuth = new Mock<IAuthService>();
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<RegisterViewModel>>();
        var viewModel = new RegisterViewModel(mockAuth.Object, mockAcademic.Object, mockLogger.Object);

        var faculty = new LookupItem { Id = "faculty-1", Name = "Faculty of Science" };
        var department1 = new LookupItem { Id = "dept-1", Name = "Department 1" };
        var program1 = new LookupItem { Id = "prog-1", Name = "Program 1" };

        viewModel.SelectedFaculty = faculty;
        viewModel.Departments.Add(department1);
        viewModel.Programs.Add(program1);
        viewModel.SelectedDepartment = new LookupItem { Id = "dept-2", Name = "Department 2" };

        // Act
        viewModel.SelectedFaculty = null;

        // Assert
        Assert.IsNull(viewModel.SelectedFaculty);
        Assert.IsNull(viewModel.SelectedDepartment);
        Assert.AreEqual(0, viewModel.Departments.Count);
        Assert.AreEqual(0, viewModel.Programs.Count);
    }

    /// <summary>
    /// Tests that setting SelectedFaculty to the same value does not trigger side effects
    /// like clearing collections or resetting SelectedDepartment.
    /// </summary>
    [TestMethod]
    public void SelectedFaculty_SetToSameValue_DoesNotTriggerSideEffects()
    {
        // Arrange
        var mockAuth = new Mock<IAuthService>();
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<RegisterViewModel>>();
        var viewModel = new RegisterViewModel(mockAuth.Object, mockAcademic.Object, mockLogger.Object);

        var faculty = new LookupItem { Id = "faculty-1", Name = "Faculty of Science" };
        var department1 = new LookupItem { Id = "dept-1", Name = "Department 1" };
        var program1 = new LookupItem { Id = "prog-1", Name = "Program 1" };
        var selectedDept = new LookupItem { Id = "dept-2", Name = "Department 2" };

        viewModel.SelectedFaculty = faculty;
        viewModel.Departments.Add(department1);
        viewModel.Programs.Add(program1);
        viewModel.SelectedDepartment = selectedDept;

        // Act
        viewModel.SelectedFaculty = faculty;

        // Assert
        Assert.AreEqual(faculty, viewModel.SelectedFaculty);
        Assert.AreEqual(selectedDept, viewModel.SelectedDepartment);
        Assert.AreEqual(1, viewModel.Departments.Count);
        Assert.AreEqual(1, viewModel.Programs.Count);
    }

    /// <summary>
    /// Tests that setting SelectedFaculty raises PropertyChanged event.
    /// </summary>
    [TestMethod]
    public void SelectedFaculty_SetToNewValue_RaisesPropertyChangedEvent()
    {
        // Arrange
        var mockAuth = new Mock<IAuthService>();
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<RegisterViewModel>>();
        var viewModel = new RegisterViewModel(mockAuth.Object, mockAcademic.Object, mockLogger.Object);

        var faculty = new LookupItem { Id = "faculty-1", Name = "Faculty of Science" };
        var propertyChangedRaised = false;
        string? changedPropertyName = null;

        ((INotifyPropertyChanged)viewModel).PropertyChanged += (sender, e) =>
        {
            if (e.PropertyName == nameof(viewModel.SelectedFaculty))
            {
                propertyChangedRaised = true;
                changedPropertyName = e.PropertyName;
            }
        };

        // Act
        viewModel.SelectedFaculty = faculty;

        // Assert
        Assert.IsTrue(propertyChangedRaised);
        Assert.AreEqual(nameof(viewModel.SelectedFaculty), changedPropertyName);
    }

    /// <summary>
    /// Tests that setting SelectedFaculty to a different non-null value updates the property
    /// and triggers all side effects (clears collections, resets SelectedDepartment, calls LoadDepartmentsAsync).
    /// </summary>
    [TestMethod]
    public void SelectedFaculty_SetToDifferentNonNullValue_UpdatesPropertyAndTriggersSideEffects()
    {
        // Arrange
        var mockAuth = new Mock<IAuthService>();
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<RegisterViewModel>>();
        var viewModel = new RegisterViewModel(mockAuth.Object, mockAcademic.Object, mockLogger.Object);

        var faculty1 = new LookupItem { Id = "faculty-1", Name = "Faculty of Science" };
        var faculty2 = new LookupItem { Id = "faculty-2", Name = "Faculty of Arts" };
        var department1 = new LookupItem { Id = "dept-1", Name = "Department 1" };
        var program1 = new LookupItem { Id = "prog-1", Name = "Program 1" };

        viewModel.SelectedFaculty = faculty1;
        viewModel.Departments.Add(department1);
        viewModel.Programs.Add(program1);
        viewModel.SelectedDepartment = new LookupItem { Id = "dept-2", Name = "Department 2" };

        // Act
        viewModel.SelectedFaculty = faculty2;

        // Assert
        Assert.AreEqual(faculty2, viewModel.SelectedFaculty);
        Assert.IsNull(viewModel.SelectedDepartment);
        Assert.AreEqual(0, viewModel.Departments.Count);
        Assert.AreEqual(0, viewModel.Programs.Count);
    }

    /// <summary>
    /// Tests that setting SelectedFaculty with an empty Id string still triggers LoadDepartmentsAsync.
    /// </summary>
    [TestMethod]
    public void SelectedFaculty_SetWithEmptyId_StillTriggersLoadDepartments()
    {
        // Arrange
        var mockAuth = new Mock<IAuthService>();
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<RegisterViewModel>>();
        var viewModel = new RegisterViewModel(mockAuth.Object, mockAcademic.Object, mockLogger.Object);

        var facultyWithEmptyId = new LookupItem { Id = string.Empty, Name = "Faculty" };

        // Act
        viewModel.SelectedFaculty = facultyWithEmptyId;

        // Assert
        Assert.AreEqual(facultyWithEmptyId, viewModel.SelectedFaculty);
        Assert.IsNull(viewModel.SelectedDepartment);
        Assert.AreEqual(0, viewModel.Departments.Count);
        Assert.AreEqual(0, viewModel.Programs.Count);
    }

    /// <summary>
    /// Tests that setting SelectedFaculty from null to non-null value triggers all side effects.
    /// </summary>
    [TestMethod]
    public void SelectedFaculty_SetFromNullToNonNull_TriggersSideEffects()
    {
        // Arrange
        var mockAuth = new Mock<IAuthService>();
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<RegisterViewModel>>();
        var viewModel = new RegisterViewModel(mockAuth.Object, mockAcademic.Object, mockLogger.Object);

        var faculty = new LookupItem { Id = "faculty-1", Name = "Faculty of Science" };
        var department1 = new LookupItem { Id = "dept-1", Name = "Department 1" };

        viewModel.Departments.Add(department1);

        // Act
        viewModel.SelectedFaculty = faculty;

        // Assert
        Assert.AreEqual(faculty, viewModel.SelectedFaculty);
        Assert.IsNull(viewModel.SelectedDepartment);
        Assert.AreEqual(0, viewModel.Departments.Count);
    }

    /// <summary>
    /// Tests that setting SelectedFaculty from non-null to null triggers side effects but not LoadDepartmentsAsync.
    /// </summary>
    [TestMethod]
    public void SelectedFaculty_SetFromNonNullToNull_TriggersSideEffectsWithoutLoad()
    {
        // Arrange
        var mockAuth = new Mock<IAuthService>();
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<RegisterViewModel>>();
        var viewModel = new RegisterViewModel(mockAuth.Object, mockAcademic.Object, mockLogger.Object);

        var faculty = new LookupItem { Id = "faculty-1", Name = "Faculty of Science" };
        var department1 = new LookupItem { Id = "dept-1", Name = "Department 1" };
        var program1 = new LookupItem { Id = "prog-1", Name = "Program 1" };

        viewModel.SelectedFaculty = faculty;
        viewModel.Departments.Add(department1);
        viewModel.Programs.Add(program1);
        viewModel.SelectedDepartment = department1;

        // Act
        viewModel.SelectedFaculty = null;

        // Assert
        Assert.IsNull(viewModel.SelectedFaculty);
        Assert.IsNull(viewModel.SelectedDepartment);
        Assert.AreEqual(0, viewModel.Departments.Count);
        Assert.AreEqual(0, viewModel.Programs.Count);
    }

    /// <summary>
    /// Tests that setting SelectedFaculty to null when already null does not trigger side effects.
    /// </summary>
    [TestMethod]
    public void SelectedFaculty_SetToNullWhenAlreadyNull_DoesNotTriggerSideEffects()
    {
        // Arrange
        var mockAuth = new Mock<IAuthService>();
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<RegisterViewModel>>();
        var viewModel = new RegisterViewModel(mockAuth.Object, mockAcademic.Object, mockLogger.Object);

        var department1 = new LookupItem { Id = "dept-1", Name = "Department 1" };
        var program1 = new LookupItem { Id = "prog-1", Name = "Program 1" };

        viewModel.Departments.Add(department1);
        viewModel.Programs.Add(program1);

        // Act
        viewModel.SelectedFaculty = null;

        // Assert
        Assert.IsNull(viewModel.SelectedFaculty);
        Assert.AreEqual(1, viewModel.Departments.Count);
        Assert.AreEqual(1, viewModel.Programs.Count);
    }

    /// <summary>
    /// Tests that setting SelectedFaculty clears multiple items from Departments and Programs collections.
    /// </summary>
    [TestMethod]
    public void SelectedFaculty_SetToNewValue_ClearsMultipleItemsFromCollections()
    {
        // Arrange
        var mockAuth = new Mock<IAuthService>();
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<RegisterViewModel>>();
        var viewModel = new RegisterViewModel(mockAuth.Object, mockAcademic.Object, mockLogger.Object);

        var faculty = new LookupItem { Id = "faculty-1", Name = "Faculty of Science" };

        viewModel.Departments.Add(new LookupItem { Id = "dept-1", Name = "Department 1" });
        viewModel.Departments.Add(new LookupItem { Id = "dept-2", Name = "Department 2" });
        viewModel.Departments.Add(new LookupItem { Id = "dept-3", Name = "Department 3" });

        viewModel.Programs.Add(new LookupItem { Id = "prog-1", Name = "Program 1" });
        viewModel.Programs.Add(new LookupItem { Id = "prog-2", Name = "Program 2" });

        // Act
        viewModel.SelectedFaculty = faculty;

        // Assert
        Assert.AreEqual(faculty, viewModel.SelectedFaculty);
        Assert.AreEqual(0, viewModel.Departments.Count);
        Assert.AreEqual(0, viewModel.Programs.Count);
    }

    /// <summary>
    /// Tests that SelectedSemester property can be set to a valid LookupItem and retrieves the correct value.
    /// </summary>
    [TestMethod]
    public void SelectedSemester_SetValidLookupItem_ReturnsCorrectValue()
    {
        // Arrange
        var authMock = new Mock<IAuthService>();
        var academicMock = new Mock<IAcademicService>();
        var loggerMock = new Mock<ILogger<RegisterViewModel>>();
        var viewModel = new RegisterViewModel(authMock.Object, academicMock.Object, loggerMock.Object);
        var expectedLookupItem = new LookupItem { Id = "sem1", Name = "Semester 1" };

        // Act
        viewModel.SelectedSemester = expectedLookupItem;

        // Assert
        Assert.AreEqual(expectedLookupItem, viewModel.SelectedSemester);
    }

    /// <summary>
    /// Tests that SelectedSemester property can be set to null and retrieves null.
    /// </summary>
    [TestMethod]
    public void SelectedSemester_SetNull_ReturnsNull()
    {
        // Arrange
        var authMock = new Mock<IAuthService>();
        var academicMock = new Mock<IAcademicService>();
        var loggerMock = new Mock<ILogger<RegisterViewModel>>();
        var viewModel = new RegisterViewModel(authMock.Object, academicMock.Object, loggerMock.Object);
        viewModel.SelectedSemester = new LookupItem { Id = "sem1", Name = "Semester 1" };

        // Act
        viewModel.SelectedSemester = null;

        // Assert
        Assert.IsNull(viewModel.SelectedSemester);
    }

    /// <summary>
    /// Tests that SelectedSemester raises PropertyChanged event when set to a new value.
    /// </summary>
    [TestMethod]
    public void SelectedSemester_SetNewValue_RaisesPropertyChangedEvent()
    {
        // Arrange
        var authMock = new Mock<IAuthService>();
        var academicMock = new Mock<IAcademicService>();
        var loggerMock = new Mock<ILogger<RegisterViewModel>>();
        var viewModel = new RegisterViewModel(authMock.Object, academicMock.Object, loggerMock.Object);
        var propertyChangedRaised = false;
        string? raisedPropertyName = null;
        viewModel.PropertyChanged += (sender, args) =>
        {
            propertyChangedRaised = true;
            raisedPropertyName = args.PropertyName;
        };
        var lookupItem = new LookupItem { Id = "sem1", Name = "Semester 1" };

        // Act
        viewModel.SelectedSemester = lookupItem;

        // Assert
        Assert.IsTrue(propertyChangedRaised);
        Assert.AreEqual("SelectedSemester", raisedPropertyName);
    }

    /// <summary>
    /// Tests that SelectedSemester does not raise PropertyChanged event when set to the same value.
    /// </summary>
    [TestMethod]
    public void SelectedSemester_SetSameValue_DoesNotRaisePropertyChangedEvent()
    {
        // Arrange
        var authMock = new Mock<IAuthService>();
        var academicMock = new Mock<IAcademicService>();
        var loggerMock = new Mock<ILogger<RegisterViewModel>>();
        var viewModel = new RegisterViewModel(authMock.Object, academicMock.Object, loggerMock.Object);
        var lookupItem = new LookupItem { Id = "sem1", Name = "Semester 1" };
        viewModel.SelectedSemester = lookupItem;
        var propertyChangedCount = 0;
        viewModel.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == "SelectedSemester")
                propertyChangedCount++;
        };

        // Act
        viewModel.SelectedSemester = lookupItem;

        // Assert
        Assert.AreEqual(0, propertyChangedCount);
    }

    /// <summary>
    /// Tests that SelectedSemester initializes to null by default.
    /// </summary>
    [TestMethod]
    public void SelectedSemester_InitialValue_IsNull()
    {
        // Arrange
        var authMock = new Mock<IAuthService>();
        var academicMock = new Mock<IAcademicService>();
        var loggerMock = new Mock<ILogger<RegisterViewModel>>();

        // Act
        var viewModel = new RegisterViewModel(authMock.Object, academicMock.Object, loggerMock.Object);

        // Assert
        Assert.IsNull(viewModel.SelectedSemester);
    }

    /// <summary>
    /// Tests that SelectedSemester can be set to different values sequentially and raises PropertyChanged for each change.
    /// </summary>
    [TestMethod]
    public void SelectedSemester_SetDifferentValuesSequentially_RaisesPropertyChangedForEachChange()
    {
        // Arrange
        var authMock = new Mock<IAuthService>();
        var academicMock = new Mock<IAcademicService>();
        var loggerMock = new Mock<ILogger<RegisterViewModel>>();
        var viewModel = new RegisterViewModel(authMock.Object, academicMock.Object, loggerMock.Object);
        var propertyChangedCount = 0;
        viewModel.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == "SelectedSemester")
                propertyChangedCount++;
        };
        var lookupItem1 = new LookupItem { Id = "sem1", Name = "Semester 1" };
        var lookupItem2 = new LookupItem { Id = "sem2", Name = "Semester 2" };

        // Act
        viewModel.SelectedSemester = lookupItem1;
        viewModel.SelectedSemester = lookupItem2;

        // Assert
        Assert.AreEqual(2, propertyChangedCount);
        Assert.AreEqual(lookupItem2, viewModel.SelectedSemester);
    }

    /// <summary>
    /// Tests that SelectedSemester can be set from null to non-null to null and raises PropertyChanged for each transition.
    /// </summary>
    [TestMethod]
    public void SelectedSemester_SetFromNullToNonNullToNull_RaisesPropertyChangedForEachTransition()
    {
        // Arrange
        var authMock = new Mock<IAuthService>();
        var academicMock = new Mock<IAcademicService>();
        var loggerMock = new Mock<ILogger<RegisterViewModel>>();
        var viewModel = new RegisterViewModel(authMock.Object, academicMock.Object, loggerMock.Object);
        var propertyChangedCount = 0;
        viewModel.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == "SelectedSemester")
                propertyChangedCount++;
        };
        var lookupItem = new LookupItem { Id = "sem1", Name = "Semester 1" };

        // Act
        viewModel.SelectedSemester = lookupItem;
        viewModel.SelectedSemester = null;

        // Assert
        Assert.AreEqual(2, propertyChangedCount);
        Assert.IsNull(viewModel.SelectedSemester);
    }

    /// <summary>
    /// Tests that SelectedSemester handles LookupItem with empty strings for Id and Name.
    /// </summary>
    [TestMethod]
    public void SelectedSemester_SetLookupItemWithEmptyStrings_StoresAndRetrievesCorrectly()
    {
        // Arrange
        var authMock = new Mock<IAuthService>();
        var academicMock = new Mock<IAcademicService>();
        var loggerMock = new Mock<ILogger<RegisterViewModel>>();
        var viewModel = new RegisterViewModel(authMock.Object, academicMock.Object, loggerMock.Object);
        var lookupItem = new LookupItem { Id = string.Empty, Name = string.Empty };

        // Act
        viewModel.SelectedSemester = lookupItem;

        // Assert
        Assert.AreEqual(lookupItem, viewModel.SelectedSemester);
        Assert.AreEqual(string.Empty, viewModel.SelectedSemester?.Id);
        Assert.AreEqual(string.Empty, viewModel.SelectedSemester?.Name);
    }

    /// <summary>
    /// Tests that setting SelectedSemester to null when already null does not raise PropertyChanged event.
    /// </summary>
    [TestMethod]
    public void SelectedSemester_SetNullWhenAlreadyNull_DoesNotRaisePropertyChangedEvent()
    {
        // Arrange
        var authMock = new Mock<IAuthService>();
        var academicMock = new Mock<IAcademicService>();
        var loggerMock = new Mock<ILogger<RegisterViewModel>>();
        var viewModel = new RegisterViewModel(authMock.Object, academicMock.Object, loggerMock.Object);
        var propertyChangedCount = 0;
        viewModel.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == "SelectedSemester")
                propertyChangedCount++;
        };

        // Act
        viewModel.SelectedSemester = null;

        // Assert
        Assert.AreEqual(0, propertyChangedCount);
    }

    /// <summary>
    /// Tests that IsStep1 returns true when CurrentStep is 1, and false for all other integer values.
    /// </summary>
    /// <param name="currentStepValue">The value to set for CurrentStep.</param>
    /// <param name="expectedIsStep1">The expected return value of IsStep1.</param>
    [TestMethod]
    [DataRow(1, true, DisplayName = "CurrentStep equals 1 returns true")]
    [DataRow(0, false, DisplayName = "CurrentStep equals 0 returns false")]
    [DataRow(2, false, DisplayName = "CurrentStep equals 2 returns false")]
    [DataRow(3, false, DisplayName = "CurrentStep equals 3 returns false")]
    [DataRow(-1, false, DisplayName = "CurrentStep equals -1 returns false")]
    [DataRow(int.MinValue, false, DisplayName = "CurrentStep equals int.MinValue returns false")]
    [DataRow(int.MaxValue, false, DisplayName = "CurrentStep equals int.MaxValue returns false")]
    public void IsStep1_VariousCurrentStepValues_ReturnsExpectedBooleanValue(int currentStepValue, bool expectedIsStep1)
    {
        // Arrange
        var mockAuthService = new Mock<IAuthService>();
        var mockAcademicService = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<RegisterViewModel>>();

        var viewModel = new RegisterViewModel(
            mockAuthService.Object,
            mockAcademicService.Object,
            mockLogger.Object);

        viewModel.CurrentStep = currentStepValue;

        // Act
        bool result = viewModel.IsStep1;

        // Assert
        Assert.AreEqual(expectedIsStep1, result);
    }

    /// <summary>
    /// Tests that the Password property can be set and retrieved with a normal value.
    /// </summary>
    /// <param name="password">The password value to test.</param>
    [TestMethod]
    [DataRow("password123")]
    [DataRow("P@ssw0rd!")]
    [DataRow("SimplePassword")]
    [DataRow("12345678")]
    public void Password_SetValidValue_ReturnsSetValue(string password)
    {
        // Arrange
        var mockAuth = new Mock<IAuthService>();
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<RegisterViewModel>>();
        var viewModel = new RegisterViewModel(mockAuth.Object, mockAcademic.Object, mockLogger.Object);

        // Act
        viewModel.Password = password;

        // Assert
        Assert.AreEqual(password, viewModel.Password);
    }

    /// <summary>
    /// Tests that the Password property can be set to an empty string.
    /// </summary>
    [TestMethod]
    public void Password_SetEmptyString_ReturnsEmptyString()
    {
        // Arrange
        var mockAuth = new Mock<IAuthService>();
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<RegisterViewModel>>();
        var viewModel = new RegisterViewModel(mockAuth.Object, mockAcademic.Object, mockLogger.Object);

        // Act
        viewModel.Password = string.Empty;

        // Assert
        Assert.AreEqual(string.Empty, viewModel.Password);
    }

    /// <summary>
    /// Tests that the Password property can be set to a whitespace-only string.
    /// </summary>
    /// <param name="whitespace">The whitespace string to test.</param>
    [TestMethod]
    [DataRow("   ")]
    [DataRow("\t")]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [DataRow("  \t  \n  ")]
    public void Password_SetWhitespaceString_ReturnsWhitespaceString(string whitespace)
    {
        // Arrange
        var mockAuth = new Mock<IAuthService>();
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<RegisterViewModel>>();
        var viewModel = new RegisterViewModel(mockAuth.Object, mockAcademic.Object, mockLogger.Object);

        // Act
        viewModel.Password = whitespace;

        // Assert
        Assert.AreEqual(whitespace, viewModel.Password);
    }

    /// <summary>
    /// Tests that the Password property can be set to a very long string.
    /// </summary>
    [TestMethod]
    public void Password_SetVeryLongString_ReturnsVeryLongString()
    {
        // Arrange
        var mockAuth = new Mock<IAuthService>();
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<RegisterViewModel>>();
        var viewModel = new RegisterViewModel(mockAuth.Object, mockAcademic.Object, mockLogger.Object);
        var longPassword = new string('a', 10000);

        // Act
        viewModel.Password = longPassword;

        // Assert
        Assert.AreEqual(longPassword, viewModel.Password);
    }

    /// <summary>
    /// Tests that the Password property can be set with strings containing special characters.
    /// </summary>
    /// <param name="specialPassword">The password with special characters to test.</param>
    [TestMethod]
    [DataRow("P@$$w0rd!#%")]
    [DataRow("パスワード")]
    [DataRow("пароль")]
    [DataRow("🔒🔑password")]
    [DataRow("<script>alert('xss')</script>")]
    [DataRow("password\0withNull")]
    [DataRow("'OR'1'='1")]
    public void Password_SetStringWithSpecialCharacters_ReturnsStringWithSpecialCharacters(string specialPassword)
    {
        // Arrange
        var mockAuth = new Mock<IAuthService>();
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<RegisterViewModel>>();
        var viewModel = new RegisterViewModel(mockAuth.Object, mockAcademic.Object, mockLogger.Object);

        // Act
        viewModel.Password = specialPassword;

        // Assert
        Assert.AreEqual(specialPassword, viewModel.Password);
    }

    /// <summary>
    /// Tests that the Password property raises PropertyChanged event when value changes.
    /// </summary>
    [TestMethod]
    public void Password_SetNewValue_RaisesPropertyChangedEvent()
    {
        // Arrange
        var mockAuth = new Mock<IAuthService>();
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<RegisterViewModel>>();
        var viewModel = new RegisterViewModel(mockAuth.Object, mockAcademic.Object, mockLogger.Object);
        var propertyChangedRaised = false;
        string? changedPropertyName = null;

        ((INotifyPropertyChanged)viewModel).PropertyChanged += (sender, e) =>
        {
            propertyChangedRaised = true;
            changedPropertyName = e.PropertyName;
        };

        // Act
        viewModel.Password = "newPassword";

        // Assert
        Assert.IsTrue(propertyChangedRaised);
        Assert.AreEqual("Password", changedPropertyName);
    }

    /// <summary>
    /// Tests that the Password property does not raise PropertyChanged event when set to the same value.
    /// </summary>
    [TestMethod]
    public void Password_SetSameValue_DoesNotRaisePropertyChangedEvent()
    {
        // Arrange
        var mockAuth = new Mock<IAuthService>();
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<RegisterViewModel>>();
        var viewModel = new RegisterViewModel(mockAuth.Object, mockAcademic.Object, mockLogger.Object);
        viewModel.Password = "password123";

        var propertyChangedCount = 0;
        ((INotifyPropertyChanged)viewModel).PropertyChanged += (sender, e) =>
        {
            if (e.PropertyName == "Password")
            {
                propertyChangedCount++;
            }
        };

        // Act
        viewModel.Password = "password123";

        // Assert
        Assert.AreEqual(0, propertyChangedCount);
    }

    /// <summary>
    /// Tests that the Password property initializes to an empty string by default.
    /// </summary>
    [TestMethod]
    public void Password_DefaultValue_ReturnsEmptyString()
    {
        // Arrange
        var mockAuth = new Mock<IAuthService>();
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<RegisterViewModel>>();

        // Act
        var viewModel = new RegisterViewModel(mockAuth.Object, mockAcademic.Object, mockLogger.Object);

        // Assert
        Assert.AreEqual(string.Empty, viewModel.Password);
    }

    /// <summary>
    /// Tests that the Password property can be set multiple times with different values.
    /// </summary>
    [TestMethod]
    public void Password_SetMultipleTimes_ReturnsLastSetValue()
    {
        // Arrange
        var mockAuth = new Mock<IAuthService>();
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<RegisterViewModel>>();
        var viewModel = new RegisterViewModel(mockAuth.Object, mockAcademic.Object, mockLogger.Object);

        // Act
        viewModel.Password = "firstPassword";
        viewModel.Password = "secondPassword";
        viewModel.Password = "thirdPassword";

        // Assert
        Assert.AreEqual("thirdPassword", viewModel.Password);
    }

    /// <summary>
    /// Tests that SelectedIntake returns null by default after construction.
    /// </summary>
    [TestMethod]
    public void SelectedIntake_DefaultValue_ReturnsNull()
    {
        // Arrange
        var mockAuth = new Mock<IAuthService>();
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<RegisterViewModel>>();
        var viewModel = new RegisterViewModel(mockAuth.Object, mockAcademic.Object, mockLogger.Object);

        // Act
        var result = viewModel.SelectedIntake;

        // Assert
        Assert.IsNull(result);
    }

    /// <summary>
    /// Tests that SelectedIntake can be set to a valid LookupItem and retrieves the same value.
    /// </summary>
    [TestMethod]
    public void SelectedIntake_SetValidLookupItem_ReturnsSetValue()
    {
        // Arrange
        var mockAuth = new Mock<IAuthService>();
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<RegisterViewModel>>();
        var viewModel = new RegisterViewModel(mockAuth.Object, mockAcademic.Object, mockLogger.Object);
        var lookupItem = new LookupItem { Id = "intake1", Name = "Intake 1" };

        // Act
        viewModel.SelectedIntake = lookupItem;
        var result = viewModel.SelectedIntake;

        // Assert
        Assert.AreSame(lookupItem, result);
    }

    /// <summary>
    /// Tests that SelectedIntake can be set to null.
    /// </summary>
    [TestMethod]
    public void SelectedIntake_SetNull_ReturnsNull()
    {
        // Arrange
        var mockAuth = new Mock<IAuthService>();
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<RegisterViewModel>>();
        var viewModel = new RegisterViewModel(mockAuth.Object, mockAcademic.Object, mockLogger.Object);
        var lookupItem = new LookupItem { Id = "intake1", Name = "Intake 1" };
        viewModel.SelectedIntake = lookupItem;

        // Act
        viewModel.SelectedIntake = null;
        var result = viewModel.SelectedIntake;

        // Assert
        Assert.IsNull(result);
    }

    /// <summary>
    /// Tests that setting SelectedIntake to a new value raises PropertyChanged event with correct property name.
    /// </summary>
    [TestMethod]
    public void SelectedIntake_SetDifferentValue_RaisesPropertyChanged()
    {
        // Arrange
        var mockAuth = new Mock<IAuthService>();
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<RegisterViewModel>>();
        var viewModel = new RegisterViewModel(mockAuth.Object, mockAcademic.Object, mockLogger.Object);
        var lookupItem = new LookupItem { Id = "intake1", Name = "Intake 1" };
        string? raisedPropertyName = null;
        ((INotifyPropertyChanged)viewModel).PropertyChanged += (sender, args) => raisedPropertyName = args.PropertyName;

        // Act
        viewModel.SelectedIntake = lookupItem;

        // Assert
        Assert.AreEqual("SelectedIntake", raisedPropertyName);
    }

    /// <summary>
    /// Tests that setting SelectedIntake to the same value does not raise PropertyChanged event.
    /// </summary>
    [TestMethod]
    public void SelectedIntake_SetSameValue_DoesNotRaisePropertyChanged()
    {
        // Arrange
        var mockAuth = new Mock<IAuthService>();
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<RegisterViewModel>>();
        var viewModel = new RegisterViewModel(mockAuth.Object, mockAcademic.Object, mockLogger.Object);
        var lookupItem = new LookupItem { Id = "intake1", Name = "Intake 1" };
        viewModel.SelectedIntake = lookupItem;
        int propertyChangedCount = 0;
        ((INotifyPropertyChanged)viewModel).PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == "SelectedIntake")
                propertyChangedCount++;
        };

        // Act
        viewModel.SelectedIntake = lookupItem;

        // Assert
        Assert.AreEqual(0, propertyChangedCount);
    }

    /// <summary>
    /// Tests that setting SelectedIntake to null when already null does not raise PropertyChanged event.
    /// </summary>
    [TestMethod]
    public void SelectedIntake_SetNullWhenAlreadyNull_DoesNotRaisePropertyChanged()
    {
        // Arrange
        var mockAuth = new Mock<IAuthService>();
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<RegisterViewModel>>();
        var viewModel = new RegisterViewModel(mockAuth.Object, mockAcademic.Object, mockLogger.Object);
        int propertyChangedCount = 0;
        ((INotifyPropertyChanged)viewModel).PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == "SelectedIntake")
                propertyChangedCount++;
        };

        // Act
        viewModel.SelectedIntake = null;

        // Assert
        Assert.AreEqual(0, propertyChangedCount);
    }

    /// <summary>
    /// Tests that setting SelectedIntake to different values sequentially raises PropertyChanged for each change.
    /// </summary>
    [TestMethod]
    public void SelectedIntake_SetDifferentValuesSequentially_RaisesPropertyChangedForEach()
    {
        // Arrange
        var mockAuth = new Mock<IAuthService>();
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<RegisterViewModel>>();
        var viewModel = new RegisterViewModel(mockAuth.Object, mockAcademic.Object, mockLogger.Object);
        var lookupItem1 = new LookupItem { Id = "intake1", Name = "Intake 1" };
        var lookupItem2 = new LookupItem { Id = "intake2", Name = "Intake 2" };
        int propertyChangedCount = 0;
        ((INotifyPropertyChanged)viewModel).PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == "SelectedIntake")
                propertyChangedCount++;
        };

        // Act
        viewModel.SelectedIntake = lookupItem1;
        viewModel.SelectedIntake = lookupItem2;

        // Assert
        Assert.AreEqual(2, propertyChangedCount);
        Assert.AreSame(lookupItem2, viewModel.SelectedIntake);
    }

    /// <summary>
    /// Tests that setting SelectedIntake from null to a valid LookupItem raises PropertyChanged.
    /// </summary>
    [TestMethod]
    public void SelectedIntake_SetFromNullToValue_RaisesPropertyChanged()
    {
        // Arrange
        var mockAuth = new Mock<IAuthService>();
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<RegisterViewModel>>();
        var viewModel = new RegisterViewModel(mockAuth.Object, mockAcademic.Object, mockLogger.Object);
        var lookupItem = new LookupItem { Id = "intake1", Name = "Intake 1" };
        int propertyChangedCount = 0;
        ((INotifyPropertyChanged)viewModel).PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == "SelectedIntake")
                propertyChangedCount++;
        };

        // Act
        viewModel.SelectedIntake = lookupItem;

        // Assert
        Assert.AreEqual(1, propertyChangedCount);
        Assert.AreSame(lookupItem, viewModel.SelectedIntake);
    }

    /// <summary>
    /// Tests that setting SelectedIntake from a valid LookupItem to null raises PropertyChanged.
    /// </summary>
    [TestMethod]
    public void SelectedIntake_SetFromValueToNull_RaisesPropertyChanged()
    {
        // Arrange
        var mockAuth = new Mock<IAuthService>();
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<RegisterViewModel>>();
        var viewModel = new RegisterViewModel(mockAuth.Object, mockAcademic.Object, mockLogger.Object);
        var lookupItem = new LookupItem { Id = "intake1", Name = "Intake 1" };
        viewModel.SelectedIntake = lookupItem;
        int propertyChangedCount = 0;
        ((INotifyPropertyChanged)viewModel).PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == "SelectedIntake")
                propertyChangedCount++;
        };

        // Act
        viewModel.SelectedIntake = null;

        // Assert
        Assert.AreEqual(1, propertyChangedCount);
        Assert.IsNull(viewModel.SelectedIntake);
    }

    /// <summary>
    /// Tests that setting SelectedIntake with LookupItems having different property values but same reference equality.
    /// </summary>
    [TestMethod]
    public void SelectedIntake_SetLookupItemWithEmptyProperties_StoresAndReturnsCorrectly()
    {
        // Arrange
        var mockAuth = new Mock<IAuthService>();
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<RegisterViewModel>>();
        var viewModel = new RegisterViewModel(mockAuth.Object, mockAcademic.Object, mockLogger.Object);
        var lookupItem = new LookupItem { Id = string.Empty, Name = string.Empty };

        // Act
        viewModel.SelectedIntake = lookupItem;
        var result = viewModel.SelectedIntake;

        // Assert
        Assert.AreSame(lookupItem, result);
        Assert.AreEqual(string.Empty, result.Id);
        Assert.AreEqual(string.Empty, result.Name);
    }

    /// <summary>
    /// Tests that GoToStep3 sets CurrentStep to 3.
    /// </summary>
    [TestMethod]
    public void GoToStep3_WhenCalled_SetsCurrentStepTo3()
    {
        // Arrange
        var authServiceMock = new Mock<IAuthService>();
        var academicServiceMock = new Mock<IAcademicService>();
        var loggerMock = new Mock<ILogger<RegisterViewModel>>();
        var viewModel = new RegisterViewModel(authServiceMock.Object, academicServiceMock.Object, loggerMock.Object);

        // Act
        viewModel.GoToStep3();

        // Assert
        Assert.AreEqual(3, viewModel.CurrentStep);
    }

    /// <summary>
    /// Tests that GoToStep3 sets IsStep3 to true.
    /// </summary>
    [TestMethod]
    public void GoToStep3_WhenCalled_SetsIsStep3ToTrue()
    {
        // Arrange
        var authServiceMock = new Mock<IAuthService>();
        var academicServiceMock = new Mock<IAcademicService>();
        var loggerMock = new Mock<ILogger<RegisterViewModel>>();
        var viewModel = new RegisterViewModel(authServiceMock.Object, academicServiceMock.Object, loggerMock.Object);

        // Act
        viewModel.GoToStep3();

        // Assert
        Assert.IsTrue(viewModel.IsStep3);
    }

    /// <summary>
    /// Tests that GoToStep3 sets IsStep1 to false.
    /// </summary>
    [TestMethod]
    public void GoToStep3_WhenCalled_SetsIsStep1ToFalse()
    {
        // Arrange
        var authServiceMock = new Mock<IAuthService>();
        var academicServiceMock = new Mock<IAcademicService>();
        var loggerMock = new Mock<ILogger<RegisterViewModel>>();
        var viewModel = new RegisterViewModel(authServiceMock.Object, academicServiceMock.Object, loggerMock.Object);

        // Act
        viewModel.GoToStep3();

        // Assert
        Assert.IsFalse(viewModel.IsStep1);
    }

    /// <summary>
    /// Tests that GoToStep3 sets IsStep2 to false.
    /// </summary>
    [TestMethod]
    public void GoToStep3_WhenCalled_SetsIsStep2ToFalse()
    {
        // Arrange
        var authServiceMock = new Mock<IAuthService>();
        var academicServiceMock = new Mock<IAcademicService>();
        var loggerMock = new Mock<ILogger<RegisterViewModel>>();
        var viewModel = new RegisterViewModel(authServiceMock.Object, academicServiceMock.Object, loggerMock.Object);

        // Act
        viewModel.GoToStep3();

        // Assert
        Assert.IsFalse(viewModel.IsStep2);
    }

    /// <summary>
    /// Tests that GoToStep3 works correctly from different initial states.
    /// </summary>
    /// <param name="initialStep">The initial step value before calling GoToStep3.</param>
    [TestMethod]
    [DataRow(1)]
    [DataRow(2)]
    [DataRow(3)]
    [DataRow(0)]
    [DataRow(-1)]
    [DataRow(4)]
    [DataRow(100)]
    [DataRow(int.MinValue)]
    [DataRow(int.MaxValue)]
    public void GoToStep3_FromVariousInitialSteps_SetsCurrentStepTo3(int initialStep)
    {
        // Arrange
        var authServiceMock = new Mock<IAuthService>();
        var academicServiceMock = new Mock<IAcademicService>();
        var loggerMock = new Mock<ILogger<RegisterViewModel>>();
        var viewModel = new RegisterViewModel(authServiceMock.Object, academicServiceMock.Object, loggerMock.Object);
        viewModel.CurrentStep = initialStep;

        // Act
        viewModel.GoToStep3();

        // Assert
        Assert.AreEqual(3, viewModel.CurrentStep);
    }

    /// <summary>
    /// Tests that GoToStep3 raises PropertyChanged events for IsStep1, IsStep2, and IsStep3.
    /// </summary>
    [TestMethod]
    public void GoToStep3_WhenCalled_RaisesPropertyChangedForStepProperties()
    {
        // Arrange
        var authServiceMock = new Mock<IAuthService>();
        var academicServiceMock = new Mock<IAcademicService>();
        var loggerMock = new Mock<ILogger<RegisterViewModel>>();
        var viewModel = new RegisterViewModel(authServiceMock.Object, academicServiceMock.Object, loggerMock.Object);

        var propertyChangedEvents = new System.Collections.Generic.List<string>();
        ((INotifyPropertyChanged)viewModel).PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName != null)
            {
                propertyChangedEvents.Add(args.PropertyName);
            }
        };

        // Act
        viewModel.GoToStep3();

        // Assert
        Assert.IsTrue(propertyChangedEvents.Contains("IsStep1"), "PropertyChanged for IsStep1 was not raised.");
        Assert.IsTrue(propertyChangedEvents.Contains("IsStep2"), "PropertyChanged for IsStep2 was not raised.");
        Assert.IsTrue(propertyChangedEvents.Contains("IsStep3"), "PropertyChanged for IsStep3 was not raised.");
    }

    /// <summary>
    /// Tests that GoToStep3 raises PropertyChanged event for CurrentStep through the property setter.
    /// </summary>
    [TestMethod]
    public void GoToStep3_WhenCalled_RaisesPropertyChangedForCurrentStep()
    {
        // Arrange
        var authServiceMock = new Mock<IAuthService>();
        var academicServiceMock = new Mock<IAcademicService>();
        var loggerMock = new Mock<ILogger<RegisterViewModel>>();
        var viewModel = new RegisterViewModel(authServiceMock.Object, academicServiceMock.Object, loggerMock.Object);

        var propertyChangedEvents = new System.Collections.Generic.List<string>();
        ((INotifyPropertyChanged)viewModel).PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName != null)
            {
                propertyChangedEvents.Add(args.PropertyName);
            }
        };

        // Act
        viewModel.GoToStep3();

        // Assert
        Assert.IsTrue(propertyChangedEvents.Contains("CurrentStep"), "PropertyChanged for CurrentStep was not raised.");
    }

    /// <summary>
    /// Tests that calling GoToStep3 multiple times remains idempotent.
    /// </summary>
    [TestMethod]
    public void GoToStep3_CalledMultipleTimes_RemainsIdempotent()
    {
        // Arrange
        var authServiceMock = new Mock<IAuthService>();
        var academicServiceMock = new Mock<IAcademicService>();
        var loggerMock = new Mock<ILogger<RegisterViewModel>>();
        var viewModel = new RegisterViewModel(authServiceMock.Object, academicServiceMock.Object, loggerMock.Object);

        // Act
        viewModel.GoToStep3();
        viewModel.GoToStep3();
        viewModel.GoToStep3();

        // Assert
        Assert.AreEqual(3, viewModel.CurrentStep);
        Assert.IsTrue(viewModel.IsStep3);
        Assert.IsFalse(viewModel.IsStep1);
        Assert.IsFalse(viewModel.IsStep2);
    }
    private Mock<IAuthService> _authServiceMock = null!;
    private Mock<IAcademicService> _academicServiceMock = null!;
    private Mock<ILogger<RegisterViewModel>> _loggerMock = null!;

    [TestInitialize]
    public void TestInitialize()
    {
        _authServiceMock = new Mock<IAuthService>();
        _academicServiceMock = new Mock<IAcademicService>();
        _loggerMock = new Mock<ILogger<RegisterViewModel>>();
    }

    /// <summary>
    /// Tests that CurrentStep property is initialized to 1 when a new RegisterViewModel instance is created.
    /// </summary>
    [TestMethod]
    public void CurrentStep_WhenInstanceCreated_ShouldBeInitializedToOne()
    {
        // Arrange & Act
        var viewModel = new RegisterViewModel(_authServiceMock.Object, _academicServiceMock.Object, _loggerMock.Object);

        // Assert
        Assert.AreEqual(1, viewModel.CurrentStep);
    }

    /// <summary>
    /// Tests that setting CurrentStep to a new valid value updates the property correctly.
    /// Input: Various valid integer values.
    /// Expected: Property value is updated to the new value.
    /// </summary>
    [TestMethod]
    [DataRow(0)]
    [DataRow(1)]
    [DataRow(2)]
    [DataRow(3)]
    [DataRow(5)]
    [DataRow(10)]
    [DataRow(100)]
    [DataRow(-1)]
    [DataRow(-100)]
    public void CurrentStep_SetToValidValue_ShouldUpdateProperty(int newValue)
    {
        // Arrange
        var viewModel = new RegisterViewModel(_authServiceMock.Object, _academicServiceMock.Object, _loggerMock.Object);

        // Act
        viewModel.CurrentStep = newValue;

        // Assert
        Assert.AreEqual(newValue, viewModel.CurrentStep);
    }

    /// <summary>
    /// Tests that setting CurrentStep to extreme boundary values (int.MinValue and int.MaxValue) updates the property correctly.
    /// Input: int.MinValue and int.MaxValue.
    /// Expected: Property value is updated to the extreme value.
    /// </summary>
    [TestMethod]
    [DataRow(int.MinValue)]
    [DataRow(int.MaxValue)]
    public void CurrentStep_SetToExtremeValue_ShouldUpdateProperty(int extremeValue)
    {
        // Arrange
        var viewModel = new RegisterViewModel(_authServiceMock.Object, _academicServiceMock.Object, _loggerMock.Object);

        // Act
        viewModel.CurrentStep = extremeValue;

        // Assert
        Assert.AreEqual(extremeValue, viewModel.CurrentStep);
    }

    /// <summary>
    /// Tests that setting CurrentStep to a different value raises the PropertyChanged event with the correct property name.
    /// Input: A new value different from the current value.
    /// Expected: PropertyChanged event is raised with PropertyName = "CurrentStep".
    /// </summary>
    [TestMethod]
    public void CurrentStep_SetToDifferentValue_ShouldRaisePropertyChangedEvent()
    {
        // Arrange
        var viewModel = new RegisterViewModel(_authServiceMock.Object, _academicServiceMock.Object, _loggerMock.Object);
        string? raisedPropertyName = null;
        viewModel.PropertyChanged += (sender, args) => raisedPropertyName = args.PropertyName;

        // Act
        viewModel.CurrentStep = 2;

        // Assert
        Assert.AreEqual("CurrentStep", raisedPropertyName);
    }

    /// <summary>
    /// Tests that setting CurrentStep to the same value does not raise the PropertyChanged event.
    /// Input: The same value as the current value.
    /// Expected: PropertyChanged event is not raised.
    /// </summary>
    [TestMethod]
    public void CurrentStep_SetToSameValue_ShouldNotRaisePropertyChangedEvent()
    {
        // Arrange
        var viewModel = new RegisterViewModel(_authServiceMock.Object, _academicServiceMock.Object, _loggerMock.Object);
        bool eventRaised = false;
        viewModel.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == "CurrentStep")
            {
                eventRaised = true;
            }
        };

        // Act
        viewModel.CurrentStep = 1; // Setting to initial value

        // Assert
        Assert.IsFalse(eventRaised);
    }

    /// <summary>
    /// Tests that setting CurrentStep multiple times to different values updates the property each time and raises PropertyChanged event.
    /// Input: Sequential different values.
    /// Expected: Property is updated each time and PropertyChanged is raised for each change.
    /// </summary>
    [TestMethod]
    public void CurrentStep_SetMultipleTimes_ShouldUpdateAndRaiseEventEachTime()
    {
        // Arrange
        var viewModel = new RegisterViewModel(_authServiceMock.Object, _academicServiceMock.Object, _loggerMock.Object);
        int eventRaisedCount = 0;
        viewModel.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == "CurrentStep")
            {
                eventRaisedCount++;
            }
        };

        // Act
        viewModel.CurrentStep = 2;
        viewModel.CurrentStep = 3;
        viewModel.CurrentStep = 1;
        viewModel.CurrentStep = 5;

        // Assert
        Assert.AreEqual(5, viewModel.CurrentStep);
        Assert.AreEqual(4, eventRaisedCount);
    }

    /// <summary>
    /// Tests that setting CurrentStep to the same value multiple times only raises PropertyChanged once (on first set from initial value).
    /// Input: Same value set multiple times.
    /// Expected: PropertyChanged is raised only when value actually changes.
    /// </summary>
    [TestMethod]
    public void CurrentStep_SetToSameValueMultipleTimes_ShouldRaiseEventOnlyOnChange()
    {
        // Arrange
        var viewModel = new RegisterViewModel(_authServiceMock.Object, _academicServiceMock.Object, _loggerMock.Object);
        int eventRaisedCount = 0;
        viewModel.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == "CurrentStep")
            {
                eventRaisedCount++;
            }
        };

        // Act
        viewModel.CurrentStep = 2; // First change from 1 to 2
        viewModel.CurrentStep = 2; // No change
        viewModel.CurrentStep = 2; // No change
        viewModel.CurrentStep = 2; // No change

        // Assert
        Assert.AreEqual(2, viewModel.CurrentStep);
        Assert.AreEqual(1, eventRaisedCount);
    }

    /// <summary>
    /// Tests that setting CurrentStep to zero updates the property correctly.
    /// Input: 0.
    /// Expected: Property value is updated to 0.
    /// </summary>
    [TestMethod]
    public void CurrentStep_SetToZero_ShouldUpdateProperty()
    {
        // Arrange
        var viewModel = new RegisterViewModel(_authServiceMock.Object, _academicServiceMock.Object, _loggerMock.Object);

        // Act
        viewModel.CurrentStep = 0;

        // Assert
        Assert.AreEqual(0, viewModel.CurrentStep);
    }

    /// <summary>
    /// Tests that setting CurrentStep to negative values updates the property correctly.
    /// Input: Various negative integer values.
    /// Expected: Property value is updated to the negative value.
    /// </summary>
    [TestMethod]
    [DataRow(-1)]
    [DataRow(-5)]
    [DataRow(-1000)]
    [DataRow(int.MinValue)]
    public void CurrentStep_SetToNegativeValue_ShouldUpdateProperty(int negativeValue)
    {
        // Arrange
        var viewModel = new RegisterViewModel(_authServiceMock.Object, _academicServiceMock.Object, _loggerMock.Object);

        // Act
        viewModel.CurrentStep = negativeValue;

        // Assert
        Assert.AreEqual(negativeValue, viewModel.CurrentStep);
    }

    /// <summary>
    /// Tests that getting CurrentStep returns the correct value after multiple sets.
    /// Input: Sequential value changes.
    /// Expected: Get returns the most recently set value.
    /// </summary>
    [TestMethod]
    public void CurrentStep_GetAfterMultipleSets_ShouldReturnLastSetValue()
    {
        // Arrange
        var viewModel = new RegisterViewModel(_authServiceMock.Object, _academicServiceMock.Object, _loggerMock.Object);

        // Act
        viewModel.CurrentStep = 10;
        viewModel.CurrentStep = 20;
        viewModel.CurrentStep = 30;
        var result = viewModel.CurrentStep;

        // Assert
        Assert.AreEqual(30, result);
    }

    /// <summary>
    /// Tests that setting the Phone property to a valid phone number stores the value correctly.
    /// </summary>
    [TestMethod]
    public void Phone_SetValidValue_StoresValue()
    {
        // Arrange
        var authMock = new Mock<IAuthService>();
        var academicMock = new Mock<IAcademicService>();
        var loggerMock = new Mock<ILogger<RegisterViewModel>>();
        var viewModel = new RegisterViewModel(authMock.Object, academicMock.Object, loggerMock.Object);
        var expectedPhone = "+256700123456";

        // Act
        viewModel.Phone = expectedPhone;

        // Assert
        Assert.AreEqual(expectedPhone, viewModel.Phone);
    }

    /// <summary>
    /// Tests that setting the Phone property raises PropertyChanged event with correct property name.
    /// </summary>
    [TestMethod]
    public void Phone_SetValue_RaisesPropertyChangedEvent()
    {
        // Arrange
        var authMock = new Mock<IAuthService>();
        var academicMock = new Mock<IAcademicService>();
        var loggerMock = new Mock<ILogger<RegisterViewModel>>();
        var viewModel = new RegisterViewModel(authMock.Object, academicMock.Object, loggerMock.Object);
        string? raisedPropertyName = null;
        viewModel.PropertyChanged += (sender, args) => raisedPropertyName = args.PropertyName;
        var newPhone = "0700123456";

        // Act
        viewModel.Phone = newPhone;

        // Assert
        Assert.AreEqual("Phone", raisedPropertyName);
    }

    /// <summary>
    /// Tests that setting the Phone property to the same value does not raise PropertyChanged event.
    /// </summary>
    [TestMethod]
    public void Phone_SetSameValue_DoesNotRaisePropertyChangedEvent()
    {
        // Arrange
        var authMock = new Mock<IAuthService>();
        var academicMock = new Mock<IAcademicService>();
        var loggerMock = new Mock<ILogger<RegisterViewModel>>();
        var viewModel = new RegisterViewModel(authMock.Object, academicMock.Object, loggerMock.Object);
        var phone = "0700123456";
        viewModel.Phone = phone;
        int eventRaisedCount = 0;
        viewModel.PropertyChanged += (sender, args) => eventRaisedCount++;

        // Act
        viewModel.Phone = phone;

        // Assert
        Assert.AreEqual(0, eventRaisedCount);
    }

    /// <summary>
    /// Tests that setting the Phone property to an empty string stores the value correctly.
    /// </summary>
    [TestMethod]
    public void Phone_SetEmptyString_StoresValue()
    {
        // Arrange
        var authMock = new Mock<IAuthService>();
        var academicMock = new Mock<IAcademicService>();
        var loggerMock = new Mock<ILogger<RegisterViewModel>>();
        var viewModel = new RegisterViewModel(authMock.Object, academicMock.Object, loggerMock.Object);
        viewModel.Phone = "initial";

        // Act
        viewModel.Phone = string.Empty;

        // Assert
        Assert.AreEqual(string.Empty, viewModel.Phone);
    }

    /// <summary>
    /// Tests that the Phone property handles whitespace-only strings correctly.
    /// </summary>
    /// <param name="whitespaceValue">The whitespace string to test.</param>
    [TestMethod]
    [DataRow("   ")]
    [DataRow(" ")]
    [DataRow("\t")]
    [DataRow("\n")]
    [DataRow("\r\n")]
    public void Phone_SetWhitespaceString_StoresValue(string whitespaceValue)
    {
        // Arrange
        var authMock = new Mock<IAuthService>();
        var academicMock = new Mock<IAcademicService>();
        var loggerMock = new Mock<ILogger<RegisterViewModel>>();
        var viewModel = new RegisterViewModel(authMock.Object, academicMock.Object, loggerMock.Object);

        // Act
        viewModel.Phone = whitespaceValue;

        // Assert
        Assert.AreEqual(whitespaceValue, viewModel.Phone);
    }

    /// <summary>
    /// Tests that the Phone property handles special characters commonly used in phone numbers.
    /// </summary>
    /// <param name="phoneWithSpecialChars">Phone number with special characters.</param>
    [TestMethod]
    [DataRow("+1 (555) 123-4567")]
    [DataRow("+44-20-1234-5678")]
    [DataRow("(555) 123-4567")]
    [DataRow("555.123.4567")]
    [DataRow("+256 700 123 456")]
    public void Phone_SetValueWithSpecialCharacters_StoresValue(string phoneWithSpecialChars)
    {
        // Arrange
        var authMock = new Mock<IAuthService>();
        var academicMock = new Mock<IAcademicService>();
        var loggerMock = new Mock<ILogger<RegisterViewModel>>();
        var viewModel = new RegisterViewModel(authMock.Object, academicMock.Object, loggerMock.Object);

        // Act
        viewModel.Phone = phoneWithSpecialChars;

        // Assert
        Assert.AreEqual(phoneWithSpecialChars, viewModel.Phone);
    }

    /// <summary>
    /// Tests that the Phone property handles very long strings correctly.
    /// </summary>
    [TestMethod]
    public void Phone_SetVeryLongString_StoresValue()
    {
        // Arrange
        var authMock = new Mock<IAuthService>();
        var academicMock = new Mock<IAcademicService>();
        var loggerMock = new Mock<ILogger<RegisterViewModel>>();
        var viewModel = new RegisterViewModel(authMock.Object, academicMock.Object, loggerMock.Object);
        var veryLongPhone = new string('1', 10000);

        // Act
        viewModel.Phone = veryLongPhone;

        // Assert
        Assert.AreEqual(veryLongPhone, viewModel.Phone);
    }

    /// <summary>
    /// Tests that the Phone property handles strings with control characters.
    /// </summary>
    [TestMethod]
    public void Phone_SetStringWithControlCharacters_StoresValue()
    {
        // Arrange
        var authMock = new Mock<IAuthService>();
        var academicMock = new Mock<IAcademicService>();
        var loggerMock = new Mock<ILogger<RegisterViewModel>>();
        var viewModel = new RegisterViewModel(authMock.Object, academicMock.Object, loggerMock.Object);
        var phoneWithControlChars = "123\0456\u0001789";

        // Act
        viewModel.Phone = phoneWithControlChars;

        // Assert
        Assert.AreEqual(phoneWithControlChars, viewModel.Phone);
    }

    /// <summary>
    /// Tests that the Phone property handles null values by throwing ArgumentNullException or storing null based on nullability.
    /// Since Phone is non-nullable string, null assignment should be avoided but testing defensive behavior.
    /// </summary>
    [TestMethod]
    public void Phone_SetNull_HandlesNull()
    {
        // Arrange
        var authMock = new Mock<IAuthService>();
        var academicMock = new Mock<IAcademicService>();
        var loggerMock = new Mock<ILogger<RegisterViewModel>>();
        var viewModel = new RegisterViewModel(authMock.Object, academicMock.Object, loggerMock.Object);

        // Act
        viewModel.Phone = null!;

        // Assert
        Assert.IsNull(viewModel.Phone);
    }

    /// <summary>
    /// Tests that the Phone property default value is an empty string.
    /// </summary>
    [TestMethod]
    public void Phone_DefaultValue_IsEmptyString()
    {
        // Arrange
        var authMock = new Mock<IAuthService>();
        var academicMock = new Mock<IAcademicService>();
        var loggerMock = new Mock<ILogger<RegisterViewModel>>();

        // Act
        var viewModel = new RegisterViewModel(authMock.Object, academicMock.Object, loggerMock.Object);

        // Assert
        Assert.AreEqual(string.Empty, viewModel.Phone);
    }

    /// <summary>
    /// Tests that the Phone property can be updated multiple times and each change raises PropertyChanged event.
    /// </summary>
    [TestMethod]
    public void Phone_SetMultipleDifferentValues_RaisesPropertyChangedForEach()
    {
        // Arrange
        var authMock = new Mock<IAuthService>();
        var academicMock = new Mock<IAcademicService>();
        var loggerMock = new Mock<ILogger<RegisterViewModel>>();
        var viewModel = new RegisterViewModel(authMock.Object, academicMock.Object, loggerMock.Object);
        int eventRaisedCount = 0;
        viewModel.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == "Phone")
                eventRaisedCount++;
        };

        // Act
        viewModel.Phone = "0700123456";
        viewModel.Phone = "0701234567";
        viewModel.Phone = "0702345678";

        // Assert
        Assert.AreEqual(3, eventRaisedCount);
    }

    /// <summary>
    /// Tests that the Phone property handles strings with invalid or unusual characters.
    /// </summary>
    /// <param name="invalidPhone">Phone number with invalid characters.</param>
    [TestMethod]
    [DataRow("abc123")]
    [DataRow("!@#$%^&*()")]
    [DataRow("phone number")]
    [DataRow("123-abc-4567")]
    [DataRow("📱1234567890")]
    public void Phone_SetStringWithInvalidCharacters_StoresValue(string invalidPhone)
    {
        // Arrange
        var authMock = new Mock<IAuthService>();
        var academicMock = new Mock<IAcademicService>();
        var loggerMock = new Mock<ILogger<RegisterViewModel>>();
        var viewModel = new RegisterViewModel(authMock.Object, academicMock.Object, loggerMock.Object);

        // Act
        viewModel.Phone = invalidPhone;

        // Assert
        Assert.AreEqual(invalidPhone, viewModel.Phone);
    }
    private Mock<IAuthService> _mockAuthService = null!;
    private Mock<IAcademicService> _mockAcademicService = null!;
    private Mock<ILogger<RegisterViewModel>> _mockLogger = null!;
    private RegisterViewModel _viewModel = null!;

    /// <summary>
    /// Tests that setting SelectedDepartment to a non-null value from null clears Programs,
    /// sets SelectedProgram to null, and triggers LoadProgramsAsync.
    /// </summary>
    [TestMethod]
    public void SelectedDepartment_SetNonNullFromNull_ClearsProgramsAndSetsSelectedProgramToNull()
    {
        // Arrange
        var department = new LookupItem { Id = "dept123", Name = "Computer Science" };
        _viewModel.Programs.Add(new LookupItem { Id = "prog1", Name = "Program 1" });
        _viewModel.SelectedProgram = new LookupItem { Id = "prog1", Name = "Program 1" };

        // Act
        _viewModel.SelectedDepartment = department;

        // Assert
        Assert.AreEqual(department, _viewModel.SelectedDepartment);
        Assert.IsNull(_viewModel.SelectedProgram);
        Assert.AreEqual(0, _viewModel.Programs.Count);
    }

    /// <summary>
    /// Tests that setting SelectedDepartment to null from a non-null value clears Programs
    /// and sets SelectedProgram to null without calling LoadProgramsAsync.
    /// </summary>
    [TestMethod]
    public void SelectedDepartment_SetNullFromNonNull_ClearsProgramsAndSetsSelectedProgramToNull()
    {
        // Arrange
        var department = new LookupItem { Id = "dept123", Name = "Computer Science" };
        _viewModel.SelectedDepartment = department;
        _viewModel.Programs.Add(new LookupItem { Id = "prog1", Name = "Program 1" });
        _viewModel.SelectedProgram = new LookupItem { Id = "prog1", Name = "Program 1" };

        // Act
        _viewModel.SelectedDepartment = null;

        // Assert
        Assert.IsNull(_viewModel.SelectedDepartment);
        Assert.IsNull(_viewModel.SelectedProgram);
        Assert.AreEqual(0, _viewModel.Programs.Count);
    }

    /// <summary>
    /// Tests that setting SelectedDepartment to the same value does not trigger cascading updates.
    /// </summary>
    [TestMethod]
    public void SelectedDepartment_SetSameValue_DoesNotTriggerCascadingUpdates()
    {
        // Arrange
        var department = new LookupItem { Id = "dept123", Name = "Computer Science" };
        _viewModel.SelectedDepartment = department;
        var selectedProgram = new LookupItem { Id = "prog1", Name = "Program 1" };
        _viewModel.Programs.Add(selectedProgram);
        _viewModel.SelectedProgram = selectedProgram;
        var initialProgramCount = _viewModel.Programs.Count;

        // Act
        _viewModel.SelectedDepartment = department;

        // Assert
        Assert.AreEqual(department, _viewModel.SelectedDepartment);
        Assert.AreEqual(selectedProgram, _viewModel.SelectedProgram);
        Assert.AreEqual(initialProgramCount, _viewModel.Programs.Count);
    }

    /// <summary>
    /// Tests that setting SelectedDepartment to a different non-null value clears Programs,
    /// sets SelectedProgram to null, and triggers LoadProgramsAsync with the new department Id.
    /// </summary>
    [TestMethod]
    public void SelectedDepartment_SetDifferentNonNullValue_ClearsProgramsAndTriggersLoad()
    {
        // Arrange
        var department1 = new LookupItem { Id = "dept123", Name = "Computer Science" };
        var department2 = new LookupItem { Id = "dept456", Name = "Engineering" };
        _viewModel.SelectedDepartment = department1;
        _viewModel.Programs.Add(new LookupItem { Id = "prog1", Name = "Program 1" });
        _viewModel.SelectedProgram = new LookupItem { Id = "prog1", Name = "Program 1" };

        // Act
        _viewModel.SelectedDepartment = department2;

        // Assert
        Assert.AreEqual(department2, _viewModel.SelectedDepartment);
        Assert.IsNull(_viewModel.SelectedProgram);
        Assert.AreEqual(0, _viewModel.Programs.Count);
    }

    /// <summary>
    /// Tests that setting SelectedDepartment to null when already null does not trigger cascading updates.
    /// </summary>
    [TestMethod]
    public void SelectedDepartment_SetNullWhenAlreadyNull_DoesNotTriggerCascadingUpdates()
    {
        // Arrange
        _viewModel.SelectedDepartment = null;
        var selectedProgram = new LookupItem { Id = "prog1", Name = "Program 1" };
        _viewModel.Programs.Add(selectedProgram);
        _viewModel.SelectedProgram = selectedProgram;
        var initialProgramCount = _viewModel.Programs.Count;

        // Act
        _viewModel.SelectedDepartment = null;

        // Assert
        Assert.IsNull(_viewModel.SelectedDepartment);
        Assert.AreEqual(selectedProgram, _viewModel.SelectedProgram);
        Assert.AreEqual(initialProgramCount, _viewModel.Programs.Count);
    }

    /// <summary>
    /// Tests that setting SelectedDepartment with an empty Id string still triggers the cascade
    /// and attempts to call LoadProgramsAsync with the empty Id.
    /// </summary>
    [TestMethod]
    public void SelectedDepartment_SetDepartmentWithEmptyId_ClearsProgramsAndTriggersLoad()
    {
        // Arrange
        var departmentWithEmptyId = new LookupItem { Id = string.Empty, Name = "Department" };
        _viewModel.Programs.Add(new LookupItem { Id = "prog1", Name = "Program 1" });
        _viewModel.SelectedProgram = new LookupItem { Id = "prog1", Name = "Program 1" };

        // Act
        _viewModel.SelectedDepartment = departmentWithEmptyId;

        // Assert
        Assert.AreEqual(departmentWithEmptyId, _viewModel.SelectedDepartment);
        Assert.IsNull(_viewModel.SelectedProgram);
        Assert.AreEqual(0, _viewModel.Programs.Count);
    }

    /// <summary>
    /// Tests that Programs collection is cleared when SelectedDepartment changes,
    /// even if the collection has multiple items.
    /// </summary>
    [TestMethod]
    public void SelectedDepartment_SetWithMultipleProgramsInCollection_ClearsAllPrograms()
    {
        // Arrange
        var department = new LookupItem { Id = "dept123", Name = "Computer Science" };
        _viewModel.Programs.Add(new LookupItem { Id = "prog1", Name = "Program 1" });
        _viewModel.Programs.Add(new LookupItem { Id = "prog2", Name = "Program 2" });
        _viewModel.Programs.Add(new LookupItem { Id = "prog3", Name = "Program 3" });

        // Act
        _viewModel.SelectedDepartment = department;

        // Assert
        Assert.AreEqual(0, _viewModel.Programs.Count);
    }

    /// <summary>
    /// Tests that setting SelectedDepartment to a department with special characters in Id
    /// handles the scenario correctly.
    /// </summary>
    [TestMethod]
    public void SelectedDepartment_SetDepartmentWithSpecialCharactersInId_HandlesCorrectly()
    {
        // Arrange
        var departmentWithSpecialId = new LookupItem { Id = "dept-123_$@!", Name = "Special Department" };
        _viewModel.Programs.Add(new LookupItem { Id = "prog1", Name = "Program 1" });

        // Act
        _viewModel.SelectedDepartment = departmentWithSpecialId;

        // Assert
        Assert.AreEqual(departmentWithSpecialId, _viewModel.SelectedDepartment);
        Assert.IsNull(_viewModel.SelectedProgram);
        Assert.AreEqual(0, _viewModel.Programs.Count);
    }

    /// <summary>
    /// Tests that ErrorMessage property returns the value that was set.
    /// </summary>
    /// <param name="testValue">The value to set and verify</param>
    [TestMethod]
    [DataRow("")]
    [DataRow("An error occurred")]
    [DataRow("Invalid email format")]
    [DataRow(" ")]
    [DataRow("   ")]
    [DataRow("\t")]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [DataRow("Error with special chars: !@#$%^&*()")]
    [DataRow("Error with unicode: 你好世界")]
    [DataRow("Error\x00with\x01control\x02chars")]
    public void ErrorMessage_SetValue_ReturnsSetValue(string testValue)
    {
        // Arrange
        var mockAuthService = new Mock<IAuthService>();
        var mockAcademicService = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<RegisterViewModel>>();
        var viewModel = new RegisterViewModel(
            mockAuthService.Object,
            mockAcademicService.Object,
            mockLogger.Object);

        // Act
        viewModel.ErrorMessage = testValue;
        var result = viewModel.ErrorMessage;

        // Assert
        Assert.AreEqual(testValue, result);
    }

    /// <summary>
    /// Tests that ErrorMessage property handles very long strings correctly.
    /// </summary>
    [TestMethod]
    public void ErrorMessage_SetVeryLongString_ReturnsSetValue()
    {
        // Arrange
        var mockAuthService = new Mock<IAuthService>();
        var mockAcademicService = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<RegisterViewModel>>();
        var viewModel = new RegisterViewModel(
            mockAuthService.Object,
            mockAcademicService.Object,
            mockLogger.Object);
        var veryLongString = new string('A', 10000);

        // Act
        viewModel.ErrorMessage = veryLongString;
        var result = viewModel.ErrorMessage;

        // Assert
        Assert.AreEqual(veryLongString, result);
        Assert.AreEqual(10000, result.Length);
    }

    /// <summary>
    /// Tests that ErrorMessage property handles null value (runtime scenario).
    /// </summary>
    [TestMethod]
    public void ErrorMessage_SetNull_ReturnsNull()
    {
        // Arrange
        var mockAuthService = new Mock<IAuthService>();
        var mockAcademicService = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<RegisterViewModel>>();
        var viewModel = new RegisterViewModel(
            mockAuthService.Object,
            mockAcademicService.Object,
            mockLogger.Object);

        // Act
        viewModel.ErrorMessage = null!;
        var result = viewModel.ErrorMessage;

        // Assert
        Assert.IsNull(result);
    }

    /// <summary>
    /// Tests that ErrorMessage property has initial value of empty string.
    /// </summary>
    [TestMethod]
    public void ErrorMessage_InitialValue_IsEmptyString()
    {
        // Arrange
        var mockAuthService = new Mock<IAuthService>();
        var mockAcademicService = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<RegisterViewModel>>();

        // Act
        var viewModel = new RegisterViewModel(
            mockAuthService.Object,
            mockAcademicService.Object,
            mockLogger.Object);

        // Assert
        Assert.AreEqual(string.Empty, viewModel.ErrorMessage);
    }

    /// <summary>
    /// Tests that ErrorMessage property can be set multiple times with different values.
    /// </summary>
    [TestMethod]
    public void ErrorMessage_SetMultipleTimes_ReturnsLastSetValue()
    {
        // Arrange
        var mockAuthService = new Mock<IAuthService>();
        var mockAcademicService = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<RegisterViewModel>>();
        var viewModel = new RegisterViewModel(
            mockAuthService.Object,
            mockAcademicService.Object,
            mockLogger.Object);

        // Act & Assert
        viewModel.ErrorMessage = "First error";
        Assert.AreEqual("First error", viewModel.ErrorMessage);

        viewModel.ErrorMessage = "Second error";
        Assert.AreEqual("Second error", viewModel.ErrorMessage);

        viewModel.ErrorMessage = "";
        Assert.AreEqual("", viewModel.ErrorMessage);

        viewModel.ErrorMessage = "Third error";
        Assert.AreEqual("Third error", viewModel.ErrorMessage);
    }

    /// <summary>
    /// Tests that ErrorMessage property handles setting the same value twice.
    /// Verifies that SetProperty behavior works correctly when value doesn't change.
    /// </summary>
    [TestMethod]
    public void ErrorMessage_SetSameValueTwice_ReturnsCorrectValue()
    {
        // Arrange
        var mockAuthService = new Mock<IAuthService>();
        var mockAcademicService = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<RegisterViewModel>>();
        var viewModel = new RegisterViewModel(
            mockAuthService.Object,
            mockAcademicService.Object,
            mockLogger.Object);
        var testValue = "Same error message";

        // Act
        viewModel.ErrorMessage = testValue;
        viewModel.ErrorMessage = testValue;
        var result = viewModel.ErrorMessage;

        // Assert
        Assert.AreEqual(testValue, result);
    }

    /// <summary>
    /// Tests that ErrorMessage property handles string with maximum length boundary.
    /// </summary>
    [TestMethod]
    public void ErrorMessage_SetStringWithMaxLength_ReturnsSetValue()
    {
        // Arrange
        var mockAuthService = new Mock<IAuthService>();
        var mockAcademicService = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<RegisterViewModel>>();
        var viewModel = new RegisterViewModel(
            mockAuthService.Object,
            mockAcademicService.Object,
            mockLogger.Object);
        var maxLengthString = new string('X', 100000);

        // Act
        viewModel.ErrorMessage = maxLengthString;
        var result = viewModel.ErrorMessage;

        // Assert
        Assert.AreEqual(maxLengthString, result);
        Assert.AreEqual(100000, result.Length);
    }

    /// <summary>
    /// Tests that ErrorMessage property handles strings with mixed whitespace characters.
    /// </summary>
    [TestMethod]
    public void ErrorMessage_SetMixedWhitespace_ReturnsSetValue()
    {
        // Arrange
        var mockAuthService = new Mock<IAuthService>();
        var mockAcademicService = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<RegisterViewModel>>();
        var viewModel = new RegisterViewModel(
            mockAuthService.Object,
            mockAcademicService.Object,
            mockLogger.Object);
        var mixedWhitespace = " \t\n\r  Error  \t\n\r ";

        // Act
        viewModel.ErrorMessage = mixedWhitespace;
        var result = viewModel.ErrorMessage;

        // Assert
        Assert.AreEqual(mixedWhitespace, result);
    }

    /// <summary>
    /// Tests that the constructor successfully initializes the RegisterViewModel with valid dependencies.
    /// Verifies that all three command properties (SendOtpCommand, VerifyOtpCommand, RegisterCommand) are properly initialized and not null.
    /// </summary>
    [TestMethod]
    public void Constructor_ValidDependencies_InitializesSuccessfully()
    {
        // Arrange
        var mockAuthService = new Mock<IAuthService>();
        var mockAcademicService = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<RegisterViewModel>>();

        // Act
        var viewModel = new RegisterViewModel(
            mockAuthService.Object,
            mockAcademicService.Object,
            mockLogger.Object);

        // Assert
        Assert.IsNotNull(viewModel);
        Assert.IsNotNull(viewModel.SendOtpCommand);
        Assert.IsNotNull(viewModel.VerifyOtpCommand);
        Assert.IsNotNull(viewModel.RegisterCommand);
    }

    /// <summary>
    /// Tests that the constructor properly initializes all command properties as IAsyncRelayCommand instances.
    /// Verifies that SendOtpCommand, VerifyOtpCommand, and RegisterCommand are correctly typed and instantiated.
    /// </summary>
    [TestMethod]
    public void Constructor_ValidDependencies_InitializesAllCommands()
    {
        // Arrange
        var mockAuthService = new Mock<IAuthService>();
        var mockAcademicService = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<RegisterViewModel>>();

        // Act
        var viewModel = new RegisterViewModel(
            mockAuthService.Object,
            mockAcademicService.Object,
            mockLogger.Object);

        // Assert
        Assert.IsInstanceOfType(viewModel.SendOtpCommand, typeof(IAsyncRelayCommand));
        Assert.IsInstanceOfType(viewModel.VerifyOtpCommand, typeof(IAsyncRelayCommand));
        Assert.IsInstanceOfType(viewModel.RegisterCommand, typeof(IAsyncRelayCommand));
    }

    /// <summary>
    /// Tests that setting IsOtpSent updates the property value and raises PropertyChanged event.
    /// Input: Initial value (false or true), new value (true or false).
    /// Expected: Property value is updated and PropertyChanged event is raised with correct property name when value changes.
    /// </summary>
    /// <param name="initialValue">The initial value to set the property to before testing.</param>
    /// <param name="newValue">The new value to set the property to.</param>
    /// <param name="shouldRaisePropertyChanged">Whether PropertyChanged event should be raised.</param>
    [TestMethod]
    [DataRow(false, true, true, DisplayName = "Set IsOtpSent from false to true")]
    [DataRow(true, false, true, DisplayName = "Set IsOtpSent from true to false")]
    [DataRow(false, false, false, DisplayName = "Set IsOtpSent from false to false (no change)")]
    [DataRow(true, true, false, DisplayName = "Set IsOtpSent from true to true (no change)")]
    public void IsOtpSent_SetValue_UpdatesPropertyAndRaisesPropertyChangedWhenValueChanges(bool initialValue, bool newValue, bool shouldRaisePropertyChanged)
    {
        // Arrange
        var authServiceMock = new Mock<IAuthService>();
        var academicServiceMock = new Mock<IAcademicService>();
        var loggerMock = new Mock<ILogger<RegisterViewModel>>();

        var viewModel = new RegisterViewModel(authServiceMock.Object, academicServiceMock.Object, loggerMock.Object);

        // Set initial value
        viewModel.IsOtpSent = initialValue;

        string? raisedPropertyName = null;
        int propertyChangedCount = 0;
        viewModel.PropertyChanged += (sender, args) =>
        {
            raisedPropertyName = args.PropertyName;
            propertyChangedCount++;
        };

        // Act
        viewModel.IsOtpSent = newValue;

        // Assert
        Assert.AreEqual(newValue, viewModel.IsOtpSent);

        if (shouldRaisePropertyChanged)
        {
            Assert.AreEqual(1, propertyChangedCount);
            Assert.AreEqual(nameof(viewModel.IsOtpSent), raisedPropertyName);
        }
        else
        {
            Assert.AreEqual(0, propertyChangedCount);
        }
    }

    /// <summary>
    /// Tests that getting IsOtpSent returns the correct value.
    /// Input: Set property to true or false.
    /// Expected: Getter returns the same value that was set.
    /// </summary>
    /// <param name="expectedValue">The value to set and retrieve.</param>
    [TestMethod]
    [DataRow(true, DisplayName = "Get IsOtpSent returns true")]
    [DataRow(false, DisplayName = "Get IsOtpSent returns false")]
    public void IsOtpSent_GetValue_ReturnsCorrectValue(bool expectedValue)
    {
        // Arrange
        var authServiceMock = new Mock<IAuthService>();
        var academicServiceMock = new Mock<IAcademicService>();
        var loggerMock = new Mock<ILogger<RegisterViewModel>>();

        var viewModel = new RegisterViewModel(authServiceMock.Object, academicServiceMock.Object, loggerMock.Object);

        // Act
        viewModel.IsOtpSent = expectedValue;
        var actualValue = viewModel.IsOtpSent;

        // Assert
        Assert.AreEqual(expectedValue, actualValue);
    }

    /// <summary>
    /// Tests that IsOtpSent has default value of false when RegisterViewModel is first instantiated.
    /// Input: None (new instance).
    /// Expected: IsOtpSent is false by default.
    /// </summary>
    [TestMethod]
    public void IsOtpSent_InitialValue_IsFalse()
    {
        // Arrange
        var authServiceMock = new Mock<IAuthService>();
        var academicServiceMock = new Mock<IAcademicService>();
        var loggerMock = new Mock<ILogger<RegisterViewModel>>();

        // Act
        var viewModel = new RegisterViewModel(authServiceMock.Object, academicServiceMock.Object, loggerMock.Object);

        // Assert
        Assert.IsFalse(viewModel.IsOtpSent);
    }

    /// <summary>
    /// Tests that setting IsOtpSent multiple times with different values raises PropertyChanged correctly each time.
    /// Input: Multiple alternating true/false values.
    /// Expected: PropertyChanged is raised for each value change.
    /// </summary>
    [TestMethod]
    public void IsOtpSent_SetMultipleDifferentValues_RaisesPropertyChangedEachTime()
    {
        // Arrange
        var authServiceMock = new Mock<IAuthService>();
        var academicServiceMock = new Mock<IAcademicService>();
        var loggerMock = new Mock<ILogger<RegisterViewModel>>();

        var viewModel = new RegisterViewModel(authServiceMock.Object, academicServiceMock.Object, loggerMock.Object);

        int propertyChangedCount = 0;
        viewModel.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == nameof(viewModel.IsOtpSent))
            {
                propertyChangedCount++;
            }
        };

        // Act & Assert
        viewModel.IsOtpSent = true;
        Assert.AreEqual(1, propertyChangedCount);
        Assert.IsTrue(viewModel.IsOtpSent);

        viewModel.IsOtpSent = false;
        Assert.AreEqual(2, propertyChangedCount);
        Assert.IsFalse(viewModel.IsOtpSent);

        viewModel.IsOtpSent = true;
        Assert.AreEqual(3, propertyChangedCount);
        Assert.IsTrue(viewModel.IsOtpSent);
    }

    /// <summary>
    /// Tests that the SelectedAcademicYear property returns null by default.
    /// </summary>
    [TestMethod]
    public void SelectedAcademicYear_DefaultValue_ReturnsNull()
    {
        // Arrange
        var mockAuth = new Mock<IAuthService>();
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<RegisterViewModel>>();
        var viewModel = new RegisterViewModel(mockAuth.Object, mockAcademic.Object, mockLogger.Object);

        // Act
        var result = viewModel.SelectedAcademicYear;

        // Assert
        Assert.IsNull(result);
    }

    /// <summary>
    /// Tests that setting SelectedAcademicYear to a valid LookupItem stores the value correctly.
    /// </summary>
    [TestMethod]
    public void SelectedAcademicYear_SetValidLookupItem_StoresValue()
    {
        // Arrange
        var mockAuth = new Mock<IAuthService>();
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<RegisterViewModel>>();
        var viewModel = new RegisterViewModel(mockAuth.Object, mockAcademic.Object, mockLogger.Object);
        var lookupItem = new LookupItem { Id = "2024", Name = "Academic Year 2024" };

        // Act
        viewModel.SelectedAcademicYear = lookupItem;

        // Assert
        Assert.AreEqual(lookupItem, viewModel.SelectedAcademicYear);
        Assert.AreEqual("2024", viewModel.SelectedAcademicYear?.Id);
        Assert.AreEqual("Academic Year 2024", viewModel.SelectedAcademicYear?.Name);
    }

    /// <summary>
    /// Tests that setting SelectedAcademicYear to null stores null correctly.
    /// </summary>
    [TestMethod]
    public void SelectedAcademicYear_SetToNull_StoresNull()
    {
        // Arrange
        var mockAuth = new Mock<IAuthService>();
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<RegisterViewModel>>();
        var viewModel = new RegisterViewModel(mockAuth.Object, mockAcademic.Object, mockLogger.Object);
        var lookupItem = new LookupItem { Id = "2024", Name = "Academic Year 2024" };
        viewModel.SelectedAcademicYear = lookupItem;

        // Act
        viewModel.SelectedAcademicYear = null;

        // Assert
        Assert.IsNull(viewModel.SelectedAcademicYear);
    }

    /// <summary>
    /// Tests that PropertyChanged event is raised when SelectedAcademicYear is set to a different value.
    /// </summary>
    [TestMethod]
    public void SelectedAcademicYear_SetToDifferentValue_RaisesPropertyChanged()
    {
        // Arrange
        var mockAuth = new Mock<IAuthService>();
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<RegisterViewModel>>();
        var viewModel = new RegisterViewModel(mockAuth.Object, mockAcademic.Object, mockLogger.Object);
        var lookupItem = new LookupItem { Id = "2024", Name = "Academic Year 2024" };
        string? raisedPropertyName = null;
        viewModel.PropertyChanged += (sender, args) => raisedPropertyName = args.PropertyName;

        // Act
        viewModel.SelectedAcademicYear = lookupItem;

        // Assert
        Assert.AreEqual("SelectedAcademicYear", raisedPropertyName);
    }

    /// <summary>
    /// Tests that PropertyChanged event is not raised when SelectedAcademicYear is set to the same value.
    /// </summary>
    [TestMethod]
    public void SelectedAcademicYear_SetToSameValue_DoesNotRaisePropertyChanged()
    {
        // Arrange
        var mockAuth = new Mock<IAuthService>();
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<RegisterViewModel>>();
        var viewModel = new RegisterViewModel(mockAuth.Object, mockAcademic.Object, mockLogger.Object);
        var lookupItem = new LookupItem { Id = "2024", Name = "Academic Year 2024" };
        viewModel.SelectedAcademicYear = lookupItem;
        int eventRaisedCount = 0;
        viewModel.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == "SelectedAcademicYear")
                eventRaisedCount++;
        };

        // Act
        viewModel.SelectedAcademicYear = lookupItem;

        // Assert
        Assert.AreEqual(0, eventRaisedCount);
    }

    /// <summary>
    /// Tests that PropertyChanged event is raised when transitioning from null to a valid value.
    /// </summary>
    [TestMethod]
    public void SelectedAcademicYear_SetFromNullToValue_RaisesPropertyChanged()
    {
        // Arrange
        var mockAuth = new Mock<IAuthService>();
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<RegisterViewModel>>();
        var viewModel = new RegisterViewModel(mockAuth.Object, mockAcademic.Object, mockLogger.Object);
        var lookupItem = new LookupItem { Id = "2024", Name = "Academic Year 2024" };
        string? raisedPropertyName = null;
        viewModel.PropertyChanged += (sender, args) => raisedPropertyName = args.PropertyName;

        // Act
        viewModel.SelectedAcademicYear = lookupItem;

        // Assert
        Assert.AreEqual("SelectedAcademicYear", raisedPropertyName);
        Assert.IsNotNull(viewModel.SelectedAcademicYear);
    }

    /// <summary>
    /// Tests that PropertyChanged event is raised when transitioning from a value to null.
    /// </summary>
    [TestMethod]
    public void SelectedAcademicYear_SetFromValueToNull_RaisesPropertyChanged()
    {
        // Arrange
        var mockAuth = new Mock<IAuthService>();
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<RegisterViewModel>>();
        var viewModel = new RegisterViewModel(mockAuth.Object, mockAcademic.Object, mockLogger.Object);
        var lookupItem = new LookupItem { Id = "2024", Name = "Academic Year 2024" };
        viewModel.SelectedAcademicYear = lookupItem;
        string? raisedPropertyName = null;
        viewModel.PropertyChanged += (sender, args) => raisedPropertyName = args.PropertyName;

        // Act
        viewModel.SelectedAcademicYear = null;

        // Assert
        Assert.AreEqual("SelectedAcademicYear", raisedPropertyName);
        Assert.IsNull(viewModel.SelectedAcademicYear);
    }

    /// <summary>
    /// Tests that setting SelectedAcademicYear to a LookupItem with empty strings stores the value correctly.
    /// </summary>
    [TestMethod]
    public void SelectedAcademicYear_SetWithEmptyStrings_StoresValue()
    {
        // Arrange
        var mockAuth = new Mock<IAuthService>();
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<RegisterViewModel>>();
        var viewModel = new RegisterViewModel(mockAuth.Object, mockAcademic.Object, mockLogger.Object);
        var lookupItem = new LookupItem { Id = string.Empty, Name = string.Empty };

        // Act
        viewModel.SelectedAcademicYear = lookupItem;

        // Assert
        Assert.AreEqual(lookupItem, viewModel.SelectedAcademicYear);
        Assert.AreEqual(string.Empty, viewModel.SelectedAcademicYear?.Id);
        Assert.AreEqual(string.Empty, viewModel.SelectedAcademicYear?.Name);
    }

    /// <summary>
    /// Tests that setting SelectedAcademicYear to different LookupItem instances with same content raises PropertyChanged.
    /// </summary>
    [TestMethod]
    public void SelectedAcademicYear_SetToDifferentInstanceSameContent_RaisesPropertyChanged()
    {
        // Arrange
        var mockAuth = new Mock<IAuthService>();
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<RegisterViewModel>>();
        var viewModel = new RegisterViewModel(mockAuth.Object, mockAcademic.Object, mockLogger.Object);
        var lookupItem1 = new LookupItem { Id = "2024", Name = "Academic Year 2024" };
        var lookupItem2 = new LookupItem { Id = "2024", Name = "Academic Year 2024" };
        viewModel.SelectedAcademicYear = lookupItem1;
        string? raisedPropertyName = null;
        viewModel.PropertyChanged += (sender, args) => raisedPropertyName = args.PropertyName;

        // Act
        viewModel.SelectedAcademicYear = lookupItem2;

        // Assert
        Assert.AreEqual("SelectedAcademicYear", raisedPropertyName);
        Assert.AreEqual(lookupItem2, viewModel.SelectedAcademicYear);
    }

    /// <summary>
    /// Tests that setting SelectedAcademicYear multiple times with different values correctly updates the stored value.
    /// Validates input: null, LookupItem with Id="2023", LookupItem with Id="2024", null again.
    /// Expected: Each change updates the property and the final value is null.
    /// </summary>
    [TestMethod]
    public void SelectedAcademicYear_SetMultipleTimes_StoresLatestValue()
    {
        // Arrange
        var mockAuth = new Mock<IAuthService>();
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<RegisterViewModel>>();
        var viewModel = new RegisterViewModel(mockAuth.Object, mockAcademic.Object, mockLogger.Object);
        var lookupItem1 = new LookupItem { Id = "2023", Name = "Academic Year 2023" };
        var lookupItem2 = new LookupItem { Id = "2024", Name = "Academic Year 2024" };

        // Act & Assert
        viewModel.SelectedAcademicYear = lookupItem1;
        Assert.AreEqual("2023", viewModel.SelectedAcademicYear?.Id);

        viewModel.SelectedAcademicYear = lookupItem2;
        Assert.AreEqual("2024", viewModel.SelectedAcademicYear?.Id);

        viewModel.SelectedAcademicYear = null;
        Assert.IsNull(viewModel.SelectedAcademicYear);
    }

    /// <summary>
    /// Tests that setting SelectedAcademicYear when null is set again does not raise PropertyChanged.
    /// </summary>
    [TestMethod]
    public void SelectedAcademicYear_SetNullWhenAlreadyNull_DoesNotRaisePropertyChanged()
    {
        // Arrange
        var mockAuth = new Mock<IAuthService>();
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<RegisterViewModel>>();
        var viewModel = new RegisterViewModel(mockAuth.Object, mockAcademic.Object, mockLogger.Object);
        int eventRaisedCount = 0;
        viewModel.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == "SelectedAcademicYear")
                eventRaisedCount++;
        };

        // Act
        viewModel.SelectedAcademicYear = null;

        // Assert
        Assert.AreEqual(0, eventRaisedCount);
        Assert.IsNull(viewModel.SelectedAcademicYear);
    }

    /// <summary>
    /// Tests that IsStep2 returns the correct boolean value based on CurrentStep.
    /// Verifies that IsStep2 is true when CurrentStep is 2, and false for all other values.
    /// </summary>
    /// <param name="currentStep">The value to set for CurrentStep property.</param>
    /// <param name="expectedIsStep2">The expected return value of IsStep2 property.</param>
    [TestMethod]
    [DataRow(2, true, DisplayName = "IsStep2_WhenCurrentStepIs2_ReturnsTrue")]
    [DataRow(1, false, DisplayName = "IsStep2_WhenCurrentStepIs1_ReturnsFalse")]
    [DataRow(3, false, DisplayName = "IsStep2_WhenCurrentStepIs3_ReturnsFalse")]
    [DataRow(0, false, DisplayName = "IsStep2_WhenCurrentStepIs0_ReturnsFalse")]
    [DataRow(-1, false, DisplayName = "IsStep2_WhenCurrentStepIsNegative_ReturnsFalse")]
    [DataRow(int.MinValue, false, DisplayName = "IsStep2_WhenCurrentStepIsMinValue_ReturnsFalse")]
    [DataRow(int.MaxValue, false, DisplayName = "IsStep2_WhenCurrentStepIsMaxValue_ReturnsFalse")]
    [DataRow(100, false, DisplayName = "IsStep2_WhenCurrentStepIsLargeValue_ReturnsFalse")]
    [DataRow(-100, false, DisplayName = "IsStep2_WhenCurrentStepIsLargeNegative_ReturnsFalse")]
    public void IsStep2_VariousCurrentStepValues_ReturnsExpectedResult(int currentStep, bool expectedIsStep2)
    {
        // Arrange
        var mockAuthService = new Mock<IAuthService>();
        var mockAcademicService = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<RegisterViewModel>>();

        var viewModel = new RegisterViewModel(
            mockAuthService.Object,
            mockAcademicService.Object,
            mockLogger.Object);

        viewModel.CurrentStep = currentStep;

        // Act
        bool actualIsStep2 = viewModel.IsStep2;

        // Assert
        Assert.AreEqual(expectedIsStep2, actualIsStep2);
    }

    /// <summary>
    /// Tests that the Email property returns the default value (empty string) when first accessed.
    /// </summary>
    [TestMethod]
    public void Email_DefaultValue_ReturnsEmptyString()
    {
        // Arrange
        var authMock = new Mock<IAuthService>();
        var academicMock = new Mock<IAcademicService>();
        var loggerMock = new Mock<ILogger<RegisterViewModel>>();
        var viewModel = new RegisterViewModel(authMock.Object, academicMock.Object, loggerMock.Object);

        // Act
        var result = viewModel.Email;

        // Assert
        Assert.AreEqual(string.Empty, result);
    }

    /// <summary>
    /// Tests that setting the Email property updates the value correctly and raises PropertyChanged event.
    /// Tests multiple valid email strings including typical formats, special characters, and varying lengths.
    /// </summary>
    /// <param name="email">The email value to set</param>
    [TestMethod]
    [DataRow("test@example.com")]
    [DataRow("user.name+tag@example.co.uk")]
    [DataRow("a@b.c")]
    [DataRow("very.long.email.address.with.many.dots@subdomain.example.domain.com")]
    [DataRow("email@with-dash.com")]
    [DataRow("email_with_underscore@example.com")]
    [DataRow("123456@example.com")]
    [DataRow("email@123.456.789.012")]
    public void Email_SetValidValue_UpdatesValueAndRaisesPropertyChanged(string email)
    {
        // Arrange
        var authMock = new Mock<IAuthService>();
        var academicMock = new Mock<IAcademicService>();
        var loggerMock = new Mock<ILogger<RegisterViewModel>>();
        var viewModel = new RegisterViewModel(authMock.Object, academicMock.Object, loggerMock.Object);
        var propertyChangedRaised = false;
        string? changedPropertyName = null;
        viewModel.PropertyChanged += (sender, args) =>
        {
            propertyChangedRaised = true;
            changedPropertyName = args.PropertyName;
        };

        // Act
        viewModel.Email = email;

        // Assert
        Assert.AreEqual(email, viewModel.Email);
        Assert.IsTrue(propertyChangedRaised);
        Assert.AreEqual("Email", changedPropertyName);
    }

    /// <summary>
    /// Tests that setting the Email property to an empty string updates the value correctly.
    /// </summary>
    [TestMethod]
    public void Email_SetEmptyString_UpdatesValue()
    {
        // Arrange
        var authMock = new Mock<IAuthService>();
        var academicMock = new Mock<IAcademicService>();
        var loggerMock = new Mock<ILogger<RegisterViewModel>>();
        var viewModel = new RegisterViewModel(authMock.Object, academicMock.Object, loggerMock.Object);
        viewModel.Email = "test@example.com";

        // Act
        viewModel.Email = string.Empty;

        // Assert
        Assert.AreEqual(string.Empty, viewModel.Email);
    }

    /// <summary>
    /// Tests that setting the Email property to whitespace-only strings updates the value.
    /// Tests various whitespace patterns including spaces, tabs, and newlines.
    /// </summary>
    /// <param name="whitespace">The whitespace string to set</param>
    [TestMethod]
    [DataRow(" ")]
    [DataRow("   ")]
    [DataRow("\t")]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [DataRow("  \t  \n  ")]
    public void Email_SetWhitespaceString_UpdatesValue(string whitespace)
    {
        // Arrange
        var authMock = new Mock<IAuthService>();
        var academicMock = new Mock<IAcademicService>();
        var loggerMock = new Mock<ILogger<RegisterViewModel>>();
        var viewModel = new RegisterViewModel(authMock.Object, academicMock.Object, loggerMock.Object);

        // Act
        viewModel.Email = whitespace;

        // Assert
        Assert.AreEqual(whitespace, viewModel.Email);
    }

    /// <summary>
    /// Tests that setting the Email property to a very long string updates the value correctly.
    /// </summary>
    [TestMethod]
    public void Email_SetVeryLongString_UpdatesValue()
    {
        // Arrange
        var authMock = new Mock<IAuthService>();
        var academicMock = new Mock<IAcademicService>();
        var loggerMock = new Mock<ILogger<RegisterViewModel>>();
        var viewModel = new RegisterViewModel(authMock.Object, academicMock.Object, loggerMock.Object);
        var veryLongEmail = new string('a', 1000) + "@example.com";

        // Act
        viewModel.Email = veryLongEmail;

        // Assert
        Assert.AreEqual(veryLongEmail, viewModel.Email);
    }

    /// <summary>
    /// Tests that setting the Email property to strings with special characters updates the value.
    /// Tests control characters, Unicode, and other special character patterns.
    /// </summary>
    /// <param name="specialString">The string with special characters to set</param>
    [TestMethod]
    [DataRow("email@example.com\u0000")]
    [DataRow("email@例え.jp")]
    [DataRow("email@😀.com")]
    [DataRow("email@example.com\r")]
    [DataRow("email@example.com\u001F")]
    [DataRow("email<script>@example.com")]
    [DataRow("email@[192.168.1.1]")]
    public void Email_SetStringWithSpecialCharacters_UpdatesValue(string specialString)
    {
        // Arrange
        var authMock = new Mock<IAuthService>();
        var academicMock = new Mock<IAcademicService>();
        var loggerMock = new Mock<ILogger<RegisterViewModel>>();
        var viewModel = new RegisterViewModel(authMock.Object, academicMock.Object, loggerMock.Object);

        // Act
        viewModel.Email = specialString;

        // Assert
        Assert.AreEqual(specialString, viewModel.Email);
    }

    /// <summary>
    /// Tests that setting the Email property to the same value does not raise PropertyChanged event.
    /// This validates that the SetProperty method correctly detects duplicate values.
    /// </summary>
    [TestMethod]
    public void Email_SetSameValue_DoesNotRaisePropertyChanged()
    {
        // Arrange
        var authMock = new Mock<IAuthService>();
        var academicMock = new Mock<IAcademicService>();
        var loggerMock = new Mock<ILogger<RegisterViewModel>>();
        var viewModel = new RegisterViewModel(authMock.Object, academicMock.Object, loggerMock.Object);
        var email = "test@example.com";
        viewModel.Email = email;
        var propertyChangedRaised = false;
        viewModel.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == "Email")
            {
                propertyChangedRaised = true;
            }
        };

        // Act
        viewModel.Email = email;

        // Assert
        Assert.IsFalse(propertyChangedRaised);
        Assert.AreEqual(email, viewModel.Email);
    }

    /// <summary>
    /// Tests that setting the Email property multiple times with different values correctly updates
    /// the value and raises PropertyChanged event for each change.
    /// </summary>
    [TestMethod]
    public void Email_SetMultipleDifferentValues_UpdatesValueAndRaisesPropertyChangedEachTime()
    {
        // Arrange
        var authMock = new Mock<IAuthService>();
        var academicMock = new Mock<IAcademicService>();
        var loggerMock = new Mock<ILogger<RegisterViewModel>>();
        var viewModel = new RegisterViewModel(authMock.Object, academicMock.Object, loggerMock.Object);
        var propertyChangedCount = 0;
        viewModel.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == "Email")
            {
                propertyChangedCount++;
            }
        };

        // Act
        viewModel.Email = "first@example.com";
        viewModel.Email = "second@example.com";
        viewModel.Email = "third@example.com";

        // Assert
        Assert.AreEqual("third@example.com", viewModel.Email);
        Assert.AreEqual(3, propertyChangedCount);
    }

    /// <summary>
    /// Tests that setting Email to empty string when it's already empty does not raise PropertyChanged.
    /// </summary>
    [TestMethod]
    public void Email_SetEmptyWhenAlreadyEmpty_DoesNotRaisePropertyChanged()
    {
        // Arrange
        var authMock = new Mock<IAuthService>();
        var academicMock = new Mock<IAcademicService>();
        var loggerMock = new Mock<ILogger<RegisterViewModel>>();
        var viewModel = new RegisterViewModel(authMock.Object, academicMock.Object, loggerMock.Object);
        var propertyChangedRaised = false;
        viewModel.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == "Email")
            {
                propertyChangedRaised = true;
            }
        };

        // Act
        viewModel.Email = string.Empty;

        // Assert
        Assert.IsFalse(propertyChangedRaised);
        Assert.AreEqual(string.Empty, viewModel.Email);
    }

    /// <summary>
    /// Tests that setting SelectedProgram to a valid LookupItem updates the property value.
    /// Input: Valid LookupItem instance.
    /// Expected: Property returns the set value.
    /// </summary>
    [TestMethod]
    public void SelectedProgram_SetValidLookupItem_ReturnsSetValue()
    {
        // Arrange
        var mockAuth = new Mock<IAuthService>();
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<RegisterViewModel>>();
        var viewModel = new RegisterViewModel(mockAuth.Object, mockAcademic.Object, mockLogger.Object);
        var lookupItem = new LookupItem { Id = "123", Name = "Test Program" };

        // Act
        viewModel.SelectedProgram = lookupItem;

        // Assert
        Assert.AreEqual(lookupItem, viewModel.SelectedProgram);
    }

    /// <summary>
    /// Tests that setting SelectedProgram to null updates the property value.
    /// Input: Null value.
    /// Expected: Property returns null.
    /// </summary>
    [TestMethod]
    public void SelectedProgram_SetNull_ReturnsNull()
    {
        // Arrange
        var mockAuth = new Mock<IAuthService>();
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<RegisterViewModel>>();
        var viewModel = new RegisterViewModel(mockAuth.Object, mockAcademic.Object, mockLogger.Object);

        // Act
        viewModel.SelectedProgram = null;

        // Assert
        Assert.IsNull(viewModel.SelectedProgram);
    }

    /// <summary>
    /// Tests that setting SelectedProgram raises PropertyChanged event with correct property name.
    /// Input: Valid LookupItem instance.
    /// Expected: PropertyChanged event is raised with "SelectedProgram" as property name.
    /// </summary>
    [TestMethod]
    public void SelectedProgram_SetValue_RaisesPropertyChangedEvent()
    {
        // Arrange
        var mockAuth = new Mock<IAuthService>();
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<RegisterViewModel>>();
        var viewModel = new RegisterViewModel(mockAuth.Object, mockAcademic.Object, mockLogger.Object);
        var lookupItem = new LookupItem { Id = "456", Name = "Another Program" };
        string? raisedPropertyName = null;
        viewModel.PropertyChanged += (sender, args) => raisedPropertyName = args.PropertyName;

        // Act
        viewModel.SelectedProgram = lookupItem;

        // Assert
        Assert.AreEqual("SelectedProgram", raisedPropertyName);
    }

    /// <summary>
    /// Tests that setting SelectedProgram to the same value does not raise PropertyChanged event.
    /// Input: Same LookupItem set twice.
    /// Expected: PropertyChanged event is raised only once (on first set).
    /// </summary>
    [TestMethod]
    public void SelectedProgram_SetSameValueTwice_RaisesPropertyChangedOnlyOnce()
    {
        // Arrange
        var mockAuth = new Mock<IAuthService>();
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<RegisterViewModel>>();
        var viewModel = new RegisterViewModel(mockAuth.Object, mockAcademic.Object, mockLogger.Object);
        var lookupItem = new LookupItem { Id = "789", Name = "Same Program" };
        int eventRaisedCount = 0;
        viewModel.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == "SelectedProgram")
                eventRaisedCount++;
        };

        // Act
        viewModel.SelectedProgram = lookupItem;
        viewModel.SelectedProgram = lookupItem;

        // Assert
        Assert.AreEqual(1, eventRaisedCount);
    }

    /// <summary>
    /// Tests that setting SelectedProgram to different values raises PropertyChanged event each time.
    /// Input: Two different LookupItem instances.
    /// Expected: PropertyChanged event is raised twice.
    /// </summary>
    [TestMethod]
    public void SelectedProgram_SetDifferentValues_RaisesPropertyChangedForEachChange()
    {
        // Arrange
        var mockAuth = new Mock<IAuthService>();
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<RegisterViewModel>>();
        var viewModel = new RegisterViewModel(mockAuth.Object, mockAcademic.Object, mockLogger.Object);
        var lookupItem1 = new LookupItem { Id = "001", Name = "Program 1" };
        var lookupItem2 = new LookupItem { Id = "002", Name = "Program 2" };
        int eventRaisedCount = 0;
        viewModel.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == "SelectedProgram")
                eventRaisedCount++;
        };

        // Act
        viewModel.SelectedProgram = lookupItem1;
        viewModel.SelectedProgram = lookupItem2;

        // Assert
        Assert.AreEqual(2, eventRaisedCount);
        Assert.AreEqual(lookupItem2, viewModel.SelectedProgram);
    }

    /// <summary>
    /// Tests that SelectedProgram initial value is null.
    /// Input: None (initial state).
    /// Expected: Property returns null before any value is set.
    /// </summary>
    [TestMethod]
    public void SelectedProgram_InitialValue_IsNull()
    {
        // Arrange
        var mockAuth = new Mock<IAuthService>();
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<RegisterViewModel>>();

        // Act
        var viewModel = new RegisterViewModel(mockAuth.Object, mockAcademic.Object, mockLogger.Object);

        // Assert
        Assert.IsNull(viewModel.SelectedProgram);
    }

    /// <summary>
    /// Tests that setting SelectedProgram with empty string values works correctly.
    /// Input: LookupItem with empty Id and Name.
    /// Expected: Property returns the LookupItem with empty strings.
    /// </summary>
    [TestMethod]
    public void SelectedProgram_SetLookupItemWithEmptyStrings_ReturnsSetValue()
    {
        // Arrange
        var mockAuth = new Mock<IAuthService>();
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<RegisterViewModel>>();
        var viewModel = new RegisterViewModel(mockAuth.Object, mockAcademic.Object, mockLogger.Object);
        var lookupItem = new LookupItem { Id = string.Empty, Name = string.Empty };

        // Act
        viewModel.SelectedProgram = lookupItem;

        // Assert
        Assert.AreEqual(lookupItem, viewModel.SelectedProgram);
        Assert.AreEqual(string.Empty, viewModel.SelectedProgram.Id);
        Assert.AreEqual(string.Empty, viewModel.SelectedProgram.Name);
    }

    /// <summary>
    /// Tests that setting SelectedProgram from non-null to null raises PropertyChanged event.
    /// Input: First non-null LookupItem, then null.
    /// Expected: PropertyChanged event is raised for both changes.
    /// </summary>
    [TestMethod]
    public void SelectedProgram_SetToNullAfterValue_RaisesPropertyChangedEvent()
    {
        // Arrange
        var mockAuth = new Mock<IAuthService>();
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<RegisterViewModel>>();
        var viewModel = new RegisterViewModel(mockAuth.Object, mockAcademic.Object, mockLogger.Object);
        var lookupItem = new LookupItem { Id = "999", Name = "Temp Program" };
        int eventRaisedCount = 0;
        viewModel.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == "SelectedProgram")
                eventRaisedCount++;
        };

        // Act
        viewModel.SelectedProgram = lookupItem;
        viewModel.SelectedProgram = null;

        // Assert
        Assert.AreEqual(2, eventRaisedCount);
        Assert.IsNull(viewModel.SelectedProgram);
    }

    /// <summary>
    /// Tests that setting SelectedProgram to null twice does not raise PropertyChanged event second time.
    /// Input: Null set twice consecutively.
    /// Expected: PropertyChanged event is raised only once.
    /// </summary>
    [TestMethod]
    public void SelectedProgram_SetNullTwice_RaisesPropertyChangedOnlyOnce()
    {
        // Arrange
        var mockAuth = new Mock<IAuthService>();
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<RegisterViewModel>>();
        var viewModel = new RegisterViewModel(mockAuth.Object, mockAcademic.Object, mockLogger.Object);
        int eventRaisedCount = 0;
        viewModel.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == "SelectedProgram")
                eventRaisedCount++;
        };

        // Act
        viewModel.SelectedProgram = null;
        viewModel.SelectedProgram = null;

        // Assert
        Assert.AreEqual(1, eventRaisedCount);
        Assert.IsNull(viewModel.SelectedProgram);
    }

    /// <summary>
    /// Tests that InitAsync successfully loads all lookup data from the academic service
    /// on the first call when lookups have not been loaded yet.
    /// Verifies that all six academic service methods are called and their data is loaded
    /// into the corresponding observable collections.
    /// </summary>
    [TestMethod]
    public async Task InitAsync_FirstCall_LoadsAllLookups()
    {
        // Arrange
        var mockAuthService = new Mock<IAuthService>();
        var mockAcademicService = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<RegisterViewModel>>();

        var universities = new List<LookupItem> { new LookupItem { Id = "u1", Name = "University 1" } };
        var entrySchemes = new List<LookupItem> { new LookupItem { Id = "e1", Name = "Entry Scheme 1" } };
        var intakes = new List<LookupItem> { new LookupItem { Id = "i1", Name = "Intake 1" } };
        var studyModes = new List<LookupItem> { new LookupItem { Id = "s1", Name = "Study Mode 1" } };
        var academicYears = new List<LookupItem> { new LookupItem { Id = "a1", Name = "Academic Year 1" } };
        var semesters = new List<LookupItem> { new LookupItem { Id = "sem1", Name = "Semester 1" } };

        mockAcademicService.Setup(x => x.GetUniversitiesAsync()).ReturnsAsync(universities);
        mockAcademicService.Setup(x => x.GetEntrySchemesAsync()).ReturnsAsync(entrySchemes);
        mockAcademicService.Setup(x => x.GetIntakesAsync()).ReturnsAsync(intakes);
        mockAcademicService.Setup(x => x.GetStudyModesAsync()).ReturnsAsync(studyModes);
        mockAcademicService.Setup(x => x.GetAcademicYearsAsync()).ReturnsAsync(academicYears);
        mockAcademicService.Setup(x => x.GetSemestersAsync()).ReturnsAsync(semesters);

        var viewModel = new RegisterViewModel(mockAuthService.Object, mockAcademicService.Object, mockLogger.Object);

        // Act
        await viewModel.InitAsync();

        // Assert
        mockAcademicService.Verify(x => x.GetUniversitiesAsync(), Times.Once);
        mockAcademicService.Verify(x => x.GetEntrySchemesAsync(), Times.Once);
        mockAcademicService.Verify(x => x.GetIntakesAsync(), Times.Once);
        mockAcademicService.Verify(x => x.GetStudyModesAsync(), Times.Once);
        mockAcademicService.Verify(x => x.GetAcademicYearsAsync(), Times.Once);
        mockAcademicService.Verify(x => x.GetSemestersAsync(), Times.Once);

        Assert.AreEqual(1, viewModel.Universities.Count);
        Assert.AreEqual("u1", viewModel.Universities[0].Id);
        Assert.AreEqual(1, viewModel.EntrySchemes.Count);
        Assert.AreEqual("e1", viewModel.EntrySchemes[0].Id);
        Assert.AreEqual(1, viewModel.Intakes.Count);
        Assert.AreEqual("i1", viewModel.Intakes[0].Id);
        Assert.AreEqual(1, viewModel.StudyModes.Count);
        Assert.AreEqual("s1", viewModel.StudyModes[0].Id);
        Assert.AreEqual(1, viewModel.AcademicYears.Count);
        Assert.AreEqual("a1", viewModel.AcademicYears[0].Id);
        Assert.AreEqual(1, viewModel.Semesters.Count);
        Assert.AreEqual("sem1", viewModel.Semesters[0].Id);
    }

    /// <summary>
    /// Tests that InitAsync returns immediately on subsequent calls without reloading
    /// lookup data, demonstrating proper caching behavior.
    /// Verifies that academic service methods are only called once across multiple InitAsync calls.
    /// </summary>
    [TestMethod]
    public async Task InitAsync_SecondCall_DoesNotReloadLookups()
    {
        // Arrange
        var mockAuthService = new Mock<IAuthService>();
        var mockAcademicService = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<RegisterViewModel>>();

        var universities = new List<LookupItem> { new LookupItem { Id = "u1", Name = "University 1" } };
        var entrySchemes = new List<LookupItem> { new LookupItem { Id = "e1", Name = "Entry Scheme 1" } };
        var intakes = new List<LookupItem> { new LookupItem { Id = "i1", Name = "Intake 1" } };
        var studyModes = new List<LookupItem> { new LookupItem { Id = "s1", Name = "Study Mode 1" } };
        var academicYears = new List<LookupItem> { new LookupItem { Id = "a1", Name = "Academic Year 1" } };
        var semesters = new List<LookupItem> { new LookupItem { Id = "sem1", Name = "Semester 1" } };

        mockAcademicService.Setup(x => x.GetUniversitiesAsync()).ReturnsAsync(universities);
        mockAcademicService.Setup(x => x.GetEntrySchemesAsync()).ReturnsAsync(entrySchemes);
        mockAcademicService.Setup(x => x.GetIntakesAsync()).ReturnsAsync(intakes);
        mockAcademicService.Setup(x => x.GetStudyModesAsync()).ReturnsAsync(studyModes);
        mockAcademicService.Setup(x => x.GetAcademicYearsAsync()).ReturnsAsync(academicYears);
        mockAcademicService.Setup(x => x.GetSemestersAsync()).ReturnsAsync(semesters);

        var viewModel = new RegisterViewModel(mockAuthService.Object, mockAcademicService.Object, mockLogger.Object);

        // Act
        await viewModel.InitAsync();
        await viewModel.InitAsync();
        await viewModel.InitAsync();

        // Assert
        mockAcademicService.Verify(x => x.GetUniversitiesAsync(), Times.Once);
        mockAcademicService.Verify(x => x.GetEntrySchemesAsync(), Times.Once);
        mockAcademicService.Verify(x => x.GetIntakesAsync(), Times.Once);
        mockAcademicService.Verify(x => x.GetStudyModesAsync(), Times.Once);
        mockAcademicService.Verify(x => x.GetAcademicYearsAsync(), Times.Once);
        mockAcademicService.Verify(x => x.GetSemestersAsync(), Times.Once);
    }

    /// <summary>
    /// Tests that InitAsync correctly handles empty collections returned from the academic service.
    /// Verifies that observable collections remain empty when no lookup data is available.
    /// </summary>
    [TestMethod]
    public async Task InitAsync_EmptyCollections_LoadsEmptyLookups()
    {
        // Arrange
        var mockAuthService = new Mock<IAuthService>();
        var mockAcademicService = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<RegisterViewModel>>();

        var emptyList = new List<LookupItem>();

        mockAcademicService.Setup(x => x.GetUniversitiesAsync()).ReturnsAsync(emptyList);
        mockAcademicService.Setup(x => x.GetEntrySchemesAsync()).ReturnsAsync(emptyList);
        mockAcademicService.Setup(x => x.GetIntakesAsync()).ReturnsAsync(emptyList);
        mockAcademicService.Setup(x => x.GetStudyModesAsync()).ReturnsAsync(emptyList);
        mockAcademicService.Setup(x => x.GetAcademicYearsAsync()).ReturnsAsync(emptyList);
        mockAcademicService.Setup(x => x.GetSemestersAsync()).ReturnsAsync(emptyList);

        var viewModel = new RegisterViewModel(mockAuthService.Object, mockAcademicService.Object, mockLogger.Object);

        // Act
        await viewModel.InitAsync();

        // Assert
        Assert.AreEqual(0, viewModel.Universities.Count);
        Assert.AreEqual(0, viewModel.EntrySchemes.Count);
        Assert.AreEqual(0, viewModel.Intakes.Count);
        Assert.AreEqual(0, viewModel.StudyModes.Count);
        Assert.AreEqual(0, viewModel.AcademicYears.Count);
        Assert.AreEqual(0, viewModel.Semesters.Count);
    }

    /// <summary>
    /// Tests that InitAsync correctly loads multiple items when the academic service
    /// returns collections with more than one element.
    /// Verifies that all items are properly loaded into the observable collections.
    /// </summary>
    [TestMethod]
    public async Task InitAsync_MultipleItems_LoadsAllItems()
    {
        // Arrange
        var mockAuthService = new Mock<IAuthService>();
        var mockAcademicService = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<RegisterViewModel>>();

        var universities = new List<LookupItem>
        {
            new LookupItem { Id = "u1", Name = "University 1" },
            new LookupItem { Id = "u2", Name = "University 2" },
            new LookupItem { Id = "u3", Name = "University 3" }
        };
        var entrySchemes = new List<LookupItem>
        {
            new LookupItem { Id = "e1", Name = "Entry Scheme 1" },
            new LookupItem { Id = "e2", Name = "Entry Scheme 2" }
        };

        mockAcademicService.Setup(x => x.GetUniversitiesAsync()).ReturnsAsync(universities);
        mockAcademicService.Setup(x => x.GetEntrySchemesAsync()).ReturnsAsync(entrySchemes);
        mockAcademicService.Setup(x => x.GetIntakesAsync()).ReturnsAsync(new List<LookupItem>());
        mockAcademicService.Setup(x => x.GetStudyModesAsync()).ReturnsAsync(new List<LookupItem>());
        mockAcademicService.Setup(x => x.GetAcademicYearsAsync()).ReturnsAsync(new List<LookupItem>());
        mockAcademicService.Setup(x => x.GetSemestersAsync()).ReturnsAsync(new List<LookupItem>());

        var viewModel = new RegisterViewModel(mockAuthService.Object, mockAcademicService.Object, mockLogger.Object);

        // Act
        await viewModel.InitAsync();

        // Assert
        Assert.AreEqual(3, viewModel.Universities.Count);
        Assert.AreEqual("u1", viewModel.Universities[0].Id);
        Assert.AreEqual("u2", viewModel.Universities[1].Id);
        Assert.AreEqual("u3", viewModel.Universities[2].Id);
        Assert.AreEqual(2, viewModel.EntrySchemes.Count);
        Assert.AreEqual("e1", viewModel.EntrySchemes[0].Id);
        Assert.AreEqual("e2", viewModel.EntrySchemes[1].Id);
    }

    /// <summary>
    /// Tests that InitAsync clears existing items in collections before loading new data.
    /// Verifies that previously added items are removed when InitAsync is called on a fresh instance
    /// and data is loaded.
    /// </summary>
    [TestMethod]
    public async Task InitAsync_CollectionsHaveExistingItems_ClearsAndLoadsNew()
    {
        // Arrange
        var mockAuthService = new Mock<IAuthService>();
        var mockAcademicService = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<RegisterViewModel>>();

        var universities = new List<LookupItem> { new LookupItem { Id = "u1", Name = "University 1" } };

        mockAcademicService.Setup(x => x.GetUniversitiesAsync()).ReturnsAsync(universities);
        mockAcademicService.Setup(x => x.GetEntrySchemesAsync()).ReturnsAsync(new List<LookupItem>());
        mockAcademicService.Setup(x => x.GetIntakesAsync()).ReturnsAsync(new List<LookupItem>());
        mockAcademicService.Setup(x => x.GetStudyModesAsync()).ReturnsAsync(new List<LookupItem>());
        mockAcademicService.Setup(x => x.GetAcademicYearsAsync()).ReturnsAsync(new List<LookupItem>());
        mockAcademicService.Setup(x => x.GetSemestersAsync()).ReturnsAsync(new List<LookupItem>());

        var viewModel = new RegisterViewModel(mockAuthService.Object, mockAcademicService.Object, mockLogger.Object);

        // Add an item manually before InitAsync
        viewModel.Universities.Add(new LookupItem { Id = "existing", Name = "Existing University" });

        // Act
        await viewModel.InitAsync();

        // Assert
        Assert.AreEqual(1, viewModel.Universities.Count);
        Assert.AreEqual("u1", viewModel.Universities[0].Id);
        Assert.AreNotEqual("existing", viewModel.Universities[0].Id);
    }

    /// <summary>
    /// Tests that SecondName property returns the initial default value of empty string.
    /// </summary>
    [TestMethod]
    public void SecondName_InitialValue_ReturnsEmptyString()
    {
        // Arrange
        var authMock = new Mock<IAuthService>();
        var academicMock = new Mock<IAcademicService>();
        var loggerMock = new Mock<ILogger<RegisterViewModel>>();
        var viewModel = new RegisterViewModel(authMock.Object, academicMock.Object, loggerMock.Object);

        // Act
        var result = viewModel.SecondName;

        // Assert
        Assert.AreEqual(string.Empty, result);
    }

    /// <summary>
    /// Tests that SecondName property setter updates the value correctly for various valid string inputs.
    /// Tests empty string, whitespace, normal names, special characters, and very long strings.
    /// </summary>
    /// <param name="value">The value to set on the SecondName property.</param>
    [TestMethod]
    [DataRow("")]
    [DataRow(" ")]
    [DataRow("\t")]
    [DataRow("\n")]
    [DataRow("Doe")]
    [DataRow("Smith")]
    [DataRow("O'Brien")]
    [DataRow("José")]
    [DataRow("李")]
    [DataRow("Van Der Berg")]
    [DataRow("   ")]
    [DataRow("A")]
    public void SecondName_SetValue_UpdatesPropertyCorrectly(string value)
    {
        // Arrange
        var authMock = new Mock<IAuthService>();
        var academicMock = new Mock<IAcademicService>();
        var loggerMock = new Mock<ILogger<RegisterViewModel>>();
        var viewModel = new RegisterViewModel(authMock.Object, academicMock.Object, loggerMock.Object);

        // Act
        viewModel.SecondName = value;

        // Assert
        Assert.AreEqual(value, viewModel.SecondName);
    }

    /// <summary>
    /// Tests that SecondName property setter correctly handles and stores very long string values.
    /// </summary>
    [TestMethod]
    public void SecondName_SetVeryLongString_UpdatesPropertyCorrectly()
    {
        // Arrange
        var authMock = new Mock<IAuthService>();
        var academicMock = new Mock<IAcademicService>();
        var loggerMock = new Mock<ILogger<RegisterViewModel>>();
        var viewModel = new RegisterViewModel(authMock.Object, academicMock.Object, loggerMock.Object);
        var veryLongString = new string('A', 10000);

        // Act
        viewModel.SecondName = veryLongString;

        // Assert
        Assert.AreEqual(veryLongString, viewModel.SecondName);
    }

    /// <summary>
    /// Tests that SecondName property raises PropertyChanged event when value changes.
    /// </summary>
    [TestMethod]
    public void SecondName_SetNewValue_RaisesPropertyChangedEvent()
    {
        // Arrange
        var authMock = new Mock<IAuthService>();
        var academicMock = new Mock<IAcademicService>();
        var loggerMock = new Mock<ILogger<RegisterViewModel>>();
        var viewModel = new RegisterViewModel(authMock.Object, academicMock.Object, loggerMock.Object);
        var propertyChangedRaised = false;
        string? raisedPropertyName = null;

        viewModel.PropertyChanged += (sender, args) =>
        {
            propertyChangedRaised = true;
            raisedPropertyName = args.PropertyName;
        };

        // Act
        viewModel.SecondName = "Doe";

        // Assert
        Assert.IsTrue(propertyChangedRaised);
        Assert.AreEqual("SecondName", raisedPropertyName);
    }

    /// <summary>
    /// Tests that SecondName property does not raise PropertyChanged event when the same value is set.
    /// This verifies the equality check in SetProperty method.
    /// </summary>
    [TestMethod]
    public void SecondName_SetSameValue_DoesNotRaisePropertyChangedEvent()
    {
        // Arrange
        var authMock = new Mock<IAuthService>();
        var academicMock = new Mock<IAcademicService>();
        var loggerMock = new Mock<ILogger<RegisterViewModel>>();
        var viewModel = new RegisterViewModel(authMock.Object, academicMock.Object, loggerMock.Object);
        viewModel.SecondName = "Doe";

        var propertyChangedRaised = false;
        viewModel.PropertyChanged += (sender, args) =>
        {
            propertyChangedRaised = true;
        };

        // Act
        viewModel.SecondName = "Doe";

        // Assert
        Assert.IsFalse(propertyChangedRaised);
    }

    /// <summary>
    /// Tests that SecondName property can be set multiple times with different values,
    /// and PropertyChanged event is raised for each different value.
    /// </summary>
    [TestMethod]
    public void SecondName_SetMultipleDifferentValues_RaisesPropertyChangedEventEachTime()
    {
        // Arrange
        var authMock = new Mock<IAuthService>();
        var academicMock = new Mock<IAcademicService>();
        var loggerMock = new Mock<ILogger<RegisterViewModel>>();
        var viewModel = new RegisterViewModel(authMock.Object, academicMock.Object, loggerMock.Object);
        var eventCount = 0;

        viewModel.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == "SecondName")
            {
                eventCount++;
            }
        };

        // Act
        viewModel.SecondName = "Doe";
        viewModel.SecondName = "Smith";
        viewModel.SecondName = "Johnson";

        // Assert
        Assert.AreEqual(3, eventCount);
        Assert.AreEqual("Johnson", viewModel.SecondName);
    }

    /// <summary>
    /// Tests that SecondName property correctly handles strings with control characters.
    /// </summary>
    [TestMethod]
    [DataRow("\r")]
    [DataRow("\r\n")]
    [DataRow("\u0000")]
    [DataRow("Name\u0007WithControlChars")]
    public void SecondName_SetValueWithControlCharacters_UpdatesPropertyCorrectly(string value)
    {
        // Arrange
        var authMock = new Mock<IAuthService>();
        var academicMock = new Mock<IAcademicService>();
        var loggerMock = new Mock<ILogger<RegisterViewModel>>();
        var viewModel = new RegisterViewModel(authMock.Object, academicMock.Object, loggerMock.Object);

        // Act
        viewModel.SecondName = value;

        // Assert
        Assert.AreEqual(value, viewModel.SecondName);
    }

    /// <summary>
    /// Tests that SecondName property correctly handles strings with various Unicode characters
    /// including emojis and special symbols.
    /// </summary>
    [TestMethod]
    [DataRow("Müller")]
    [DataRow("Sánchez")]
    [DataRow("Østerberg")]
    [DataRow("Žigić")]
    [DataRow("中文")]
    [DataRow("日本語")]
    [DataRow("한글")]
    [DataRow("😀")]
    public void SecondName_SetValueWithUnicodeCharacters_UpdatesPropertyCorrectly(string value)
    {
        // Arrange
        var authMock = new Mock<IAuthService>();
        var academicMock = new Mock<IAcademicService>();
        var loggerMock = new Mock<ILogger<RegisterViewModel>>();
        var viewModel = new RegisterViewModel(authMock.Object, academicMock.Object, loggerMock.Object);

        // Act
        viewModel.SecondName = value;

        // Assert
        Assert.AreEqual(value, viewModel.SecondName);
    }

    /// <summary>
    /// Creates a new instance of RegisterViewModel with mocked dependencies.
    /// </summary>
    private RegisterViewModel CreateViewModel()
    {
        _mockAuthService = new Mock<IAuthService>();
        _mockAcademicService = new Mock<IAcademicService>();
        _mockLogger = new Mock<ILogger<RegisterViewModel>>();

        return new RegisterViewModel(_mockAuthService.Object, _mockAcademicService.Object, _mockLogger.Object);
    }

    /// <summary>
    /// Tests that OtpCode property returns the initial default value of empty string.
    /// </summary>
    [TestMethod]
    public void OtpCode_InitialValue_ReturnsEmptyString()
    {
        // Arrange
        var viewModel = CreateViewModel();

        // Act
        var result = viewModel.OtpCode;

        // Assert
        Assert.AreEqual(string.Empty, result);
    }

    /// <summary>
    /// Tests that OtpCode property setter correctly sets various string values and raises PropertyChanged event.
    /// </summary>
    /// <param name="value">The value to set on the OtpCode property.</param>
    [TestMethod]
    [DataRow("123456")]
    [DataRow("000000")]
    [DataRow("ABCDEF")]
    [DataRow("")]
    [DataRow(" ")]
    [DataRow("   ")]
    [DataRow("\t")]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [DataRow("a")]
    [DataRow("12345678901234567890123456789012345678901234567890")]
    [DataRow("!@#$%^&*()")]
    [DataRow("αβγδε")]
    [DataRow("😀😁😂")]
    [DataRow("Test\0Code")]
    public void OtpCode_SetValue_UpdatesPropertyAndRaisesPropertyChanged(string value)
    {
        // Arrange
        var viewModel = CreateViewModel();
        string? raisedPropertyName = null;
        var eventRaised = false;

        viewModel.PropertyChanged += (sender, args) =>
        {
            raisedPropertyName = args.PropertyName;
            eventRaised = true;
        };

        // Act
        viewModel.OtpCode = value;

        // Assert
        Assert.AreEqual(value, viewModel.OtpCode);
        Assert.IsTrue(eventRaised);
        Assert.AreEqual(nameof(RegisterViewModel.OtpCode), raisedPropertyName);
    }

    /// <summary>
    /// Tests that setting OtpCode to the same value does not raise PropertyChanged event.
    /// </summary>
    [TestMethod]
    public void OtpCode_SetSameValue_DoesNotRaisePropertyChanged()
    {
        // Arrange
        var viewModel = CreateViewModel();
        var initialValue = "123456";
        viewModel.OtpCode = initialValue;

        var eventRaisedCount = 0;
        viewModel.PropertyChanged += (sender, args) =>
        {
            eventRaisedCount++;
        };

        // Act
        viewModel.OtpCode = initialValue;

        // Assert
        Assert.AreEqual(initialValue, viewModel.OtpCode);
        Assert.AreEqual(0, eventRaisedCount);
    }

    /// <summary>
    /// Tests that OtpCode property can be updated multiple times with different values.
    /// </summary>
    [TestMethod]
    public void OtpCode_SetMultipleDifferentValues_UpdatesPropertyEachTime()
    {
        // Arrange
        var viewModel = CreateViewModel();
        var values = new[] { "111111", "222222", "333333", "444444" };
        var eventRaisedCount = 0;

        viewModel.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == nameof(RegisterViewModel.OtpCode))
            {
                eventRaisedCount++;
            }
        };

        // Act & Assert
        foreach (var value in values)
        {
            viewModel.OtpCode = value;
            Assert.AreEqual(value, viewModel.OtpCode);
        }

        Assert.AreEqual(values.Length, eventRaisedCount);
    }

    /// <summary>
    /// Tests that OtpCode property handles extremely long strings correctly.
    /// </summary>
    [TestMethod]
    public void OtpCode_SetVeryLongString_UpdatesPropertyCorrectly()
    {
        // Arrange
        var viewModel = CreateViewModel();
        var longString = new string('X', 10000);
        var eventRaised = false;

        viewModel.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == nameof(RegisterViewModel.OtpCode))
            {
                eventRaised = true;
            }
        };

        // Act
        viewModel.OtpCode = longString;

        // Assert
        Assert.AreEqual(longString, viewModel.OtpCode);
        Assert.IsTrue(eventRaised);
        Assert.AreEqual(10000, viewModel.OtpCode.Length);
    }

    /// <summary>
    /// Tests that setting OtpCode from non-empty to empty string raises PropertyChanged event.
    /// </summary>
    [TestMethod]
    public void OtpCode_SetToEmptyAfterNonEmpty_RaisesPropertyChanged()
    {
        // Arrange
        var viewModel = CreateViewModel();
        viewModel.OtpCode = "123456";

        var eventRaised = false;
        string? raisedPropertyName = null;

        viewModel.PropertyChanged += (sender, args) =>
        {
            raisedPropertyName = args.PropertyName;
            eventRaised = true;
        };

        // Act
        viewModel.OtpCode = string.Empty;

        // Assert
        Assert.AreEqual(string.Empty, viewModel.OtpCode);
        Assert.IsTrue(eventRaised);
        Assert.AreEqual(nameof(RegisterViewModel.OtpCode), raisedPropertyName);
    }

    /// <summary>
    /// Tests that OtpCode property correctly handles strings with various whitespace characters.
    /// </summary>
    [TestMethod]
    [DataRow(" \t\n\r ")]
    [DataRow("  123456  ")]
    [DataRow("\t123\t456\t")]
    public void OtpCode_SetWhitespaceVariations_UpdatesPropertyCorrectly(string value)
    {
        // Arrange
        var viewModel = CreateViewModel();
        var eventRaised = false;

        viewModel.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == nameof(RegisterViewModel.OtpCode))
            {
                eventRaised = true;
            }
        };

        // Act
        viewModel.OtpCode = value;

        // Assert
        Assert.AreEqual(value, viewModel.OtpCode);
        Assert.IsTrue(eventRaised);
    }

    /// <summary>
    /// Tests that setting SelectedEntryScheme to a non-null value updates the property and raises PropertyChanged event.
    /// Input: A valid LookupItem instance.
    /// Expected: Property is set and PropertyChanged event is raised with the correct property name.
    /// </summary>
    [TestMethod]
    public void SelectedEntryScheme_SetNonNullValue_UpdatesPropertyAndRaisesPropertyChanged()
    {
        // Arrange
        var viewModel = new RegisterViewModel(_mockAuthService.Object, _mockAcademicService.Object, _mockLogger.Object);
        var newLookupItem = new LookupItem { Id = "1", Name = "Direct Entry" };
        string? raisedPropertyName = null;
        viewModel.PropertyChanged += (sender, args) => raisedPropertyName = args.PropertyName;

        // Act
        viewModel.SelectedEntryScheme = newLookupItem;

        // Assert
        Assert.AreEqual(newLookupItem, viewModel.SelectedEntryScheme);
        Assert.AreEqual("SelectedEntryScheme", raisedPropertyName);
    }

    /// <summary>
    /// Tests that setting SelectedEntryScheme to null updates the property and raises PropertyChanged event.
    /// Input: Null value.
    /// Expected: Property is set to null and PropertyChanged event is raised.
    /// </summary>
    [TestMethod]
    public void SelectedEntryScheme_SetNullValue_UpdatesPropertyAndRaisesPropertyChanged()
    {
        // Arrange
        var viewModel = new RegisterViewModel(_mockAuthService.Object, _mockAcademicService.Object, _mockLogger.Object);
        viewModel.SelectedEntryScheme = new LookupItem { Id = "1", Name = "Direct Entry" };
        string? raisedPropertyName = null;
        viewModel.PropertyChanged += (sender, args) => raisedPropertyName = args.PropertyName;

        // Act
        viewModel.SelectedEntryScheme = null;

        // Assert
        Assert.IsNull(viewModel.SelectedEntryScheme);
        Assert.AreEqual("SelectedEntryScheme", raisedPropertyName);
    }

    /// <summary>
    /// Tests that setting SelectedEntryScheme to the same reference twice only raises PropertyChanged once.
    /// Input: Same LookupItem reference set twice.
    /// Expected: PropertyChanged is raised only on the first set operation.
    /// </summary>
    [TestMethod]
    public void SelectedEntryScheme_SetSameValueTwice_RaisesPropertyChangedOnlyOnce()
    {
        // Arrange
        var viewModel = new RegisterViewModel(_mockAuthService.Object, _mockAcademicService.Object, _mockLogger.Object);
        var lookupItem = new LookupItem { Id = "1", Name = "Direct Entry" };
        var eventRaisedCount = 0;
        viewModel.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == "SelectedEntryScheme")
            {
                eventRaisedCount++;
            }
        };

        // Act
        viewModel.SelectedEntryScheme = lookupItem;
        viewModel.SelectedEntryScheme = lookupItem;

        // Assert
        Assert.AreEqual(lookupItem, viewModel.SelectedEntryScheme);
        Assert.AreEqual(1, eventRaisedCount);
    }

    /// <summary>
    /// Tests that getting SelectedEntryScheme returns the correct value.
    /// Input: Property is set to a specific LookupItem.
    /// Expected: Getter returns the same LookupItem instance.
    /// </summary>
    [TestMethod]
    public void SelectedEntryScheme_GetValue_ReturnsCorrectValue()
    {
        // Arrange
        var viewModel = new RegisterViewModel(_mockAuthService.Object, _mockAcademicService.Object, _mockLogger.Object);
        var expectedLookupItem = new LookupItem { Id = "2", Name = "Diploma Entry" };
        viewModel.SelectedEntryScheme = expectedLookupItem;

        // Act
        var actualValue = viewModel.SelectedEntryScheme;

        // Assert
        Assert.AreEqual(expectedLookupItem, actualValue);
    }

    /// <summary>
    /// Tests that SelectedEntryScheme is initially null.
    /// Input: Newly created ViewModel.
    /// Expected: SelectedEntryScheme property is null.
    /// </summary>
    [TestMethod]
    public void SelectedEntryScheme_InitialValue_IsNull()
    {
        // Arrange & Act
        var viewModel = new RegisterViewModel(_mockAuthService.Object, _mockAcademicService.Object, _mockLogger.Object);

        // Assert
        Assert.IsNull(viewModel.SelectedEntryScheme);
    }

    /// <summary>
    /// Tests that setting SelectedEntryScheme to different LookupItem instances raises PropertyChanged for each change.
    /// Input: Multiple different LookupItem instances.
    /// Expected: PropertyChanged event is raised for each distinct instance.
    /// </summary>
    [TestMethod]
    public void SelectedEntryScheme_SetDifferentInstances_RaisesPropertyChangedForEachChange()
    {
        // Arrange
        var viewModel = new RegisterViewModel(_mockAuthService.Object, _mockAcademicService.Object, _mockLogger.Object);
        var firstItem = new LookupItem { Id = "1", Name = "Direct Entry" };
        var secondItem = new LookupItem { Id = "2", Name = "Diploma Entry" };
        var eventRaisedCount = 0;
        viewModel.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == "SelectedEntryScheme")
            {
                eventRaisedCount++;
            }
        };

        // Act
        viewModel.SelectedEntryScheme = firstItem;
        viewModel.SelectedEntryScheme = secondItem;

        // Assert
        Assert.AreEqual(secondItem, viewModel.SelectedEntryScheme);
        Assert.AreEqual(2, eventRaisedCount);
    }

    /// <summary>
    /// Tests that setting SelectedEntryScheme to null when already null does not raise PropertyChanged event.
    /// Input: Null value set twice consecutively.
    /// Expected: PropertyChanged is not raised on the second set.
    /// </summary>
    [TestMethod]
    public void SelectedEntryScheme_SetNullWhenAlreadyNull_DoesNotRaisePropertyChanged()
    {
        // Arrange
        var viewModel = new RegisterViewModel(_mockAuthService.Object, _mockAcademicService.Object, _mockLogger.Object);
        var eventRaisedCount = 0;
        viewModel.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == "SelectedEntryScheme")
            {
                eventRaisedCount++;
            }
        };

        // Act
        viewModel.SelectedEntryScheme = null;
        viewModel.SelectedEntryScheme = null;

        // Assert
        Assert.IsNull(viewModel.SelectedEntryScheme);
        Assert.AreEqual(0, eventRaisedCount);
    }

    /// <summary>
    /// Tests that setting SelectedEntryScheme with different instances having identical property values raises PropertyChanged.
    /// Input: Two different LookupItem instances with same Id and Name.
    /// Expected: PropertyChanged is raised because instances are different (reference equality).
    /// </summary>
    [TestMethod]
    public void SelectedEntryScheme_SetDifferentInstancesWithSameValues_RaisesPropertyChanged()
    {
        // Arrange
        var viewModel = new RegisterViewModel(_mockAuthService.Object, _mockAcademicService.Object, _mockLogger.Object);
        var firstItem = new LookupItem { Id = "1", Name = "Direct Entry" };
        var secondItem = new LookupItem { Id = "1", Name = "Direct Entry" };
        viewModel.SelectedEntryScheme = firstItem;
        var eventRaisedCount = 0;
        viewModel.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == "SelectedEntryScheme")
            {
                eventRaisedCount++;
            }
        };

        // Act
        viewModel.SelectedEntryScheme = secondItem;

        // Assert
        Assert.AreEqual(secondItem, viewModel.SelectedEntryScheme);
        Assert.AreEqual(1, eventRaisedCount);
    }

    /// <summary>
    /// Tests that setting SelectedEntryScheme with various LookupItem states (empty strings, special characters) works correctly.
    /// Input: LookupItem with empty strings and special characters.
    /// Expected: Property is set correctly and PropertyChanged is raised.
    /// </summary>
    [TestMethod]
    [DataRow("", "")]
    [DataRow("123", "")]
    [DataRow("", "Test Name")]
    [DataRow("id-with-special-chars!@#", "Name with spaces and symbols $%^")]
    [DataRow("very-long-id-" + "abcdefghijklmnopqrstuvwxyz", "Very Long Name That Contains Many Characters")]
    public void SelectedEntryScheme_SetWithVariousLookupItemValues_UpdatesPropertyCorrectly(string id, string name)
    {
        // Arrange
        var viewModel = new RegisterViewModel(_mockAuthService.Object, _mockAcademicService.Object, _mockLogger.Object);
        var lookupItem = new LookupItem { Id = id, Name = name };
        string? raisedPropertyName = null;
        viewModel.PropertyChanged += (sender, args) => raisedPropertyName = args.PropertyName;

        // Act
        viewModel.SelectedEntryScheme = lookupItem;

        // Assert
        Assert.AreEqual(lookupItem, viewModel.SelectedEntryScheme);
        Assert.AreEqual(id, viewModel.SelectedEntryScheme.Id);
        Assert.AreEqual(name, viewModel.SelectedEntryScheme.Name);
        Assert.AreEqual("SelectedEntryScheme", raisedPropertyName);
    }

    /// <summary>
    /// Tests that OnPropertyChanged raises PropertyChanged event with valid property name.
    /// Input: Valid property name string.
    /// Expected: PropertyChanged event is raised with the specified property name.
    /// </summary>
    [TestMethod]
    [DataRow("FirstName")]
    [DataRow("LastName")]
    [DataRow("Email")]
    [DataRow("")]
    [DataRow(" ")]
    [DataRow("   ")]
    [DataRow("PropertyWithVeryLongNameThatExceedsNormalLengthToTestEdgeCaseScenarioForPropertyChangedEventHandlingInViewModelImplementation")]
    [DataRow("Property!@#$%^&*()")]
    [DataRow("Property\nWith\nNewLines")]
    [DataRow("Property\tWith\tTabs")]
    public void OnPropertyChanged_ValidString_RaisesPropertyChangedEvent(string propertyName)
    {
        // Arrange
        var authMock = new Mock<IAuthService>();
        var academicMock = new Mock<IAcademicService>();
        var loggerMock = new Mock<ILogger<RegisterViewModel>>();

        var viewModel = new TestableRegisterViewModel(authMock.Object, academicMock.Object, loggerMock.Object);

        string? raisedPropertyName = null;
        viewModel.PropertyChanged += (sender, e) =>
        {
            raisedPropertyName = e.PropertyName;
        };

        // Act
        viewModel.ExposedOnPropertyChanged(propertyName);

        // Assert
        Assert.AreEqual(propertyName, raisedPropertyName);
    }

    /// <summary>
    /// Tests that OnPropertyChanged handles null parameter correctly.
    /// Input: null string.
    /// Expected: PropertyChanged event is raised with null property name (or exception if not supported).
    /// </summary>
    [TestMethod]
    public void OnPropertyChanged_NullString_RaisesPropertyChangedEventWithNull()
    {
        // Arrange
        var authMock = new Mock<IAuthService>();
        var academicMock = new Mock<IAcademicService>();
        var loggerMock = new Mock<ILogger<RegisterViewModel>>();

        var viewModel = new TestableRegisterViewModel(authMock.Object, academicMock.Object, loggerMock.Object);

        string? raisedPropertyName = "not null";
        viewModel.PropertyChanged += (sender, e) =>
        {
            raisedPropertyName = e.PropertyName;
        };

        // Act
        viewModel.ExposedOnPropertyChanged(null!);

        // Assert
        Assert.IsNull(raisedPropertyName);
    }

    /// <summary>
    /// Tests that OnPropertyChanged raises event with correct sender.
    /// Input: Valid property name.
    /// Expected: PropertyChanged event is raised with the view model instance as sender.
    /// </summary>
    [TestMethod]
    public void OnPropertyChanged_ValidString_RaisesEventWithCorrectSender()
    {
        // Arrange
        var authMock = new Mock<IAuthService>();
        var academicMock = new Mock<IAcademicService>();
        var loggerMock = new Mock<ILogger<RegisterViewModel>>();

        var viewModel = new TestableRegisterViewModel(authMock.Object, academicMock.Object, loggerMock.Object);

        object? eventSender = null;
        viewModel.PropertyChanged += (sender, e) =>
        {
            eventSender = sender;
        };

        // Act
        viewModel.ExposedOnPropertyChanged("TestProperty");

        // Assert
        Assert.AreSame(viewModel, eventSender);
    }

    /// <summary>
    /// Tests that OnPropertyChanged does not raise event when no subscribers are attached.
    /// Input: Valid property name with no event subscribers.
    /// Expected: No exception is thrown, method completes successfully.
    /// </summary>
    [TestMethod]
    public void OnPropertyChanged_NoSubscribers_DoesNotThrowException()
    {
        // Arrange
        var authMock = new Mock<IAuthService>();
        var academicMock = new Mock<IAcademicService>();
        var loggerMock = new Mock<ILogger<RegisterViewModel>>();

        var viewModel = new TestableRegisterViewModel(authMock.Object, academicMock.Object, loggerMock.Object);

        // Act & Assert - should not throw
        viewModel.ExposedOnPropertyChanged("TestProperty");
    }

    /// <summary>
    /// Tests that OnPropertyChanged can be called multiple times with different property names.
    /// Input: Multiple property names in sequence.
    /// Expected: PropertyChanged event is raised for each call with the correct property name.
    /// </summary>
    [TestMethod]
    public void OnPropertyChanged_MultipleCallsWithDifferentNames_RaisesEventForEach()
    {
        // Arrange
        var authMock = new Mock<IAuthService>();
        var academicMock = new Mock<IAcademicService>();
        var loggerMock = new Mock<ILogger<RegisterViewModel>>();

        var viewModel = new TestableRegisterViewModel(authMock.Object, academicMock.Object, loggerMock.Object);

        var raisedPropertyNames = new System.Collections.Generic.List<string?>();
        viewModel.PropertyChanged += (sender, e) =>
        {
            raisedPropertyNames.Add(e.PropertyName);
        };

        // Act
        viewModel.ExposedOnPropertyChanged("Property1");
        viewModel.ExposedOnPropertyChanged("Property2");
        viewModel.ExposedOnPropertyChanged("Property3");

        // Assert
        Assert.AreEqual(3, raisedPropertyNames.Count);
        Assert.AreEqual("Property1", raisedPropertyNames[0]);
        Assert.AreEqual("Property2", raisedPropertyNames[1]);
        Assert.AreEqual("Property3", raisedPropertyNames[2]);
    }

    /// <summary>
    /// Helper class that exposes the protected OnPropertyChanged method for testing.
    /// </summary>
    private class TestableRegisterViewModel : RegisterViewModel
    {
        public TestableRegisterViewModel(IAuthService auth, IAcademicService academic, ILogger<RegisterViewModel> logger)
            : base(auth, academic, logger)
        {
        }

        public void ExposedOnPropertyChanged(string name)
        {
            OnPropertyChanged(name);
        }
    }

    /// <summary>
    /// Tests that setting OtherNames property with a valid string value updates the property and raises PropertyChanged event.
    /// Input: Valid non-empty string.
    /// Expected: Property value is updated and PropertyChanged event is raised with correct property name.
    /// </summary>
    [TestMethod]
    [DataRow("John")]
    [DataRow("Mary Jane")]
    [DataRow("O'Connor")]
    [DataRow("François")]
    [DataRow("李明")]
    public void OtherNames_SetValidValue_UpdatesPropertyAndRaisesPropertyChanged(string value)
    {
        // Arrange
        var mockAuth = new Mock<IAuthService>();
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<RegisterViewModel>>();
        var viewModel = new RegisterViewModel(mockAuth.Object, mockAcademic.Object, mockLogger.Object);

        string? raisedPropertyName = null;
        viewModel.PropertyChanged += (sender, args) => raisedPropertyName = args.PropertyName;

        // Act
        viewModel.OtherNames = value;

        // Assert
        Assert.AreEqual(value, viewModel.OtherNames);
        Assert.AreEqual(nameof(RegisterViewModel.OtherNames), raisedPropertyName);
    }

    /// <summary>
    /// Tests that setting OtherNames property with an empty string updates the property and raises PropertyChanged event.
    /// Input: Empty string.
    /// Expected: Property value is set to empty string and PropertyChanged event is raised.
    /// </summary>
    [TestMethod]
    public void OtherNames_SetEmptyString_UpdatesPropertyAndRaisesPropertyChanged()
    {
        // Arrange
        var mockAuth = new Mock<IAuthService>();
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<RegisterViewModel>>();
        var viewModel = new RegisterViewModel(mockAuth.Object, mockAcademic.Object, mockLogger.Object);
        viewModel.OtherNames = "Initial Value";

        string? raisedPropertyName = null;
        viewModel.PropertyChanged += (sender, args) => raisedPropertyName = args.PropertyName;

        // Act
        viewModel.OtherNames = string.Empty;

        // Assert
        Assert.AreEqual(string.Empty, viewModel.OtherNames);
        Assert.AreEqual(nameof(RegisterViewModel.OtherNames), raisedPropertyName);
    }

    /// <summary>
    /// Tests that setting OtherNames property with whitespace-only strings updates the property and raises PropertyChanged event.
    /// Input: Whitespace-only strings (spaces, tabs, newlines).
    /// Expected: Property value is updated and PropertyChanged event is raised.
    /// </summary>
    [TestMethod]
    [DataRow("   ")]
    [DataRow("\t")]
    [DataRow("\n")]
    [DataRow(" \t\n ")]
    public void OtherNames_SetWhitespaceString_UpdatesPropertyAndRaisesPropertyChanged(string value)
    {
        // Arrange
        var mockAuth = new Mock<IAuthService>();
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<RegisterViewModel>>();
        var viewModel = new RegisterViewModel(mockAuth.Object, mockAcademic.Object, mockLogger.Object);

        string? raisedPropertyName = null;
        viewModel.PropertyChanged += (sender, args) => raisedPropertyName = args.PropertyName;

        // Act
        viewModel.OtherNames = value;

        // Assert
        Assert.AreEqual(value, viewModel.OtherNames);
        Assert.AreEqual(nameof(RegisterViewModel.OtherNames), raisedPropertyName);
    }

    /// <summary>
    /// Tests that setting OtherNames property with a very long string updates the property correctly.
    /// Input: String with 10000 characters.
    /// Expected: Property value is updated and PropertyChanged event is raised.
    /// </summary>
    [TestMethod]
    public void OtherNames_SetVeryLongString_UpdatesPropertyAndRaisesPropertyChanged()
    {
        // Arrange
        var mockAuth = new Mock<IAuthService>();
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<RegisterViewModel>>();
        var viewModel = new RegisterViewModel(mockAuth.Object, mockAcademic.Object, mockLogger.Object);
        var veryLongString = new string('A', 10000);

        string? raisedPropertyName = null;
        viewModel.PropertyChanged += (sender, args) => raisedPropertyName = args.PropertyName;

        // Act
        viewModel.OtherNames = veryLongString;

        // Assert
        Assert.AreEqual(veryLongString, viewModel.OtherNames);
        Assert.AreEqual(nameof(RegisterViewModel.OtherNames), raisedPropertyName);
    }

    /// <summary>
    /// Tests that setting OtherNames property with strings containing special characters updates the property correctly.
    /// Input: Strings with special characters (control characters, unicode, symbols).
    /// Expected: Property value is updated and PropertyChanged event is raised.
    /// </summary>
    [TestMethod]
    [DataRow("Name\u0000WithNull")]
    [DataRow("Name\u0001WithControlChar")]
    [DataRow("Name@#$%^&*()")]
    [DataRow("Name\r\nWithNewlines")]
    [DataRow("Name😀WithEmoji")]
    public void OtherNames_SetStringWithSpecialCharacters_UpdatesPropertyAndRaisesPropertyChanged(string value)
    {
        // Arrange
        var mockAuth = new Mock<IAuthService>();
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<RegisterViewModel>>();
        var viewModel = new RegisterViewModel(mockAuth.Object, mockAcademic.Object, mockLogger.Object);

        string? raisedPropertyName = null;
        viewModel.PropertyChanged += (sender, args) => raisedPropertyName = args.PropertyName;

        // Act
        viewModel.OtherNames = value;

        // Assert
        Assert.AreEqual(value, viewModel.OtherNames);
        Assert.AreEqual(nameof(RegisterViewModel.OtherNames), raisedPropertyName);
    }

    /// <summary>
    /// Tests that setting OtherNames property to the same value twice does not raise PropertyChanged event on the second set.
    /// Input: Same string value set twice.
    /// Expected: PropertyChanged event is raised only once (on the first set).
    /// </summary>
    [TestMethod]
    public void OtherNames_SetSameValueTwice_RaisesPropertyChangedOnlyOnce()
    {
        // Arrange
        var mockAuth = new Mock<IAuthService>();
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<RegisterViewModel>>();
        var viewModel = new RegisterViewModel(mockAuth.Object, mockAcademic.Object, mockLogger.Object);
        var testValue = "TestName";

        int eventRaisedCount = 0;
        viewModel.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == nameof(RegisterViewModel.OtherNames))
            {
                eventRaisedCount++;
            }
        };

        // Act
        viewModel.OtherNames = testValue;
        viewModel.OtherNames = testValue;

        // Assert
        Assert.AreEqual(testValue, viewModel.OtherNames);
        Assert.AreEqual(1, eventRaisedCount);
    }

    /// <summary>
    /// Tests that OtherNames property has the default value of empty string after construction.
    /// Input: None (testing initial state).
    /// Expected: OtherNames property returns empty string.
    /// </summary>
    [TestMethod]
    public void OtherNames_InitialValue_IsEmptyString()
    {
        // Arrange
        var mockAuth = new Mock<IAuthService>();
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<RegisterViewModel>>();

        // Act
        var viewModel = new RegisterViewModel(mockAuth.Object, mockAcademic.Object, mockLogger.Object);

        // Assert
        Assert.AreEqual(string.Empty, viewModel.OtherNames);
    }

    /// <summary>
    /// Tests that setting OtherNames property multiple times with different values updates correctly each time.
    /// Input: Multiple different string values set sequentially.
    /// Expected: Property value is updated correctly for each set and PropertyChanged event is raised for each change.
    /// </summary>
    [TestMethod]
    public void OtherNames_SetMultipleDifferentValues_UpdatesCorrectlyEachTime()
    {
        // Arrange
        var mockAuth = new Mock<IAuthService>();
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<RegisterViewModel>>();
        var viewModel = new RegisterViewModel(mockAuth.Object, mockAcademic.Object, mockLogger.Object);

        var values = new[] { "First", "Second", "Third", string.Empty, "Fourth" };
        int eventRaisedCount = 0;
        viewModel.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == nameof(RegisterViewModel.OtherNames))
            {
                eventRaisedCount++;
            }
        };

        // Act & Assert
        foreach (var value in values)
        {
            viewModel.OtherNames = value;
            Assert.AreEqual(value, viewModel.OtherNames);
        }

        Assert.AreEqual(values.Length, eventRaisedCount);
    }

    /// <summary>
    /// Tests that the Dob property initializes to approximately 18 years ago from the current date.
    /// Validates that the default value is set correctly upon instantiation.
    /// </summary>
    [TestMethod]
    public void Dob_DefaultValue_ShouldBeApproximately18YearsAgo()
    {
        // Arrange
        var authMock = new Mock<IAuthService>();
        var academicMock = new Mock<IAcademicService>();
        var loggerMock = new Mock<ILogger<RegisterViewModel>>();

        var beforeInstantiation = DateTime.Now.AddYears(-18);

        // Act
        var viewModel = new RegisterViewModel(authMock.Object, academicMock.Object, loggerMock.Object);
        var afterInstantiation = DateTime.Now.AddYears(-18);

        // Assert
        Assert.IsTrue(viewModel.Dob >= beforeInstantiation && viewModel.Dob <= afterInstantiation,
            $"Expected Dob to be approximately 18 years ago. Actual: {viewModel.Dob}");
    }

    /// <summary>
    /// Tests that the Dob property getter returns the value that was set.
    /// Input: Various valid DateTime values including boundaries and normal dates.
    /// Expected: The getter should return exactly what was set.
    /// </summary>
    [TestMethod]
    [DataRow("0001-01-01T00:00:00.0000000")]  // DateTime.MinValue
    [DataRow("9999-12-31T23:59:59.9999999")]  // DateTime.MaxValue
    [DataRow("2000-01-01T00:00:00.0000000")]  // Y2K
    [DataRow("1990-06-15T12:30:45.1234567")]  // Specific date with time
    [DataRow("2024-02-29T00:00:00.0000000")]  // Leap year date
    [DataRow("2023-12-31T23:59:59.9999999")]  // End of year
    [DataRow("2025-01-01T00:00:00.0000000")]  // Future date
    public void Dob_SetAndGet_ShouldReturnSetValue(string dateTimeString)
    {
        // Arrange
        var authMock = new Mock<IAuthService>();
        var academicMock = new Mock<IAcademicService>();
        var loggerMock = new Mock<ILogger<RegisterViewModel>>();
        var viewModel = new RegisterViewModel(authMock.Object, academicMock.Object, loggerMock.Object);
        var testDate = DateTime.Parse(dateTimeString);

        // Act
        viewModel.Dob = testDate;
        var result = viewModel.Dob;

        // Assert
        Assert.AreEqual(testDate, result);
    }

    /// <summary>
    /// Tests that setting the Dob property raises the PropertyChanged event.
    /// Input: A new DateTime value.
    /// Expected: PropertyChanged event should be raised with the correct property name.
    /// </summary>
    [TestMethod]
    public void Dob_SetValue_ShouldRaisePropertyChangedEvent()
    {
        // Arrange
        var authMock = new Mock<IAuthService>();
        var academicMock = new Mock<IAcademicService>();
        var loggerMock = new Mock<ILogger<RegisterViewModel>>();
        var viewModel = new RegisterViewModel(authMock.Object, academicMock.Object, loggerMock.Object);
        var newDate = new DateTime(1995, 5, 15);
        var propertyChangedRaised = false;
        string? raisedPropertyName = null;

        ((INotifyPropertyChanged)viewModel).PropertyChanged += (sender, args) =>
        {
            propertyChangedRaised = true;
            raisedPropertyName = args.PropertyName;
        };

        // Act
        viewModel.Dob = newDate;

        // Assert
        Assert.IsTrue(propertyChangedRaised, "PropertyChanged event was not raised.");
        Assert.AreEqual("Dob", raisedPropertyName, "PropertyChanged event was raised with incorrect property name.");
    }

    /// <summary>
    /// Tests that setting the Dob property to the same value does not raise the PropertyChanged event.
    /// Input: Setting the property to its current value.
    /// Expected: PropertyChanged event should not be raised.
    /// </summary>
    [TestMethod]
    public void Dob_SetSameValue_ShouldNotRaisePropertyChangedEvent()
    {
        // Arrange
        var authMock = new Mock<IAuthService>();
        var academicMock = new Mock<IAcademicService>();
        var loggerMock = new Mock<ILogger<RegisterViewModel>>();
        var viewModel = new RegisterViewModel(authMock.Object, academicMock.Object, loggerMock.Object);
        var testDate = new DateTime(2000, 1, 1);
        viewModel.Dob = testDate;

        var propertyChangedRaised = false;
        ((INotifyPropertyChanged)viewModel).PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == "Dob")
            {
                propertyChangedRaised = true;
            }
        };

        // Act
        viewModel.Dob = testDate; // Set to same value

        // Assert
        Assert.IsFalse(propertyChangedRaised, "PropertyChanged event should not be raised when setting the same value.");
    }

    /// <summary>
    /// Tests that the Dob property can handle DateTime.MinValue boundary.
    /// Input: DateTime.MinValue (0001-01-01 00:00:00).
    /// Expected: The property should store and return DateTime.MinValue without error.
    /// </summary>
    [TestMethod]
    public void Dob_SetMinValue_ShouldHandleMinValueBoundary()
    {
        // Arrange
        var authMock = new Mock<IAuthService>();
        var academicMock = new Mock<IAcademicService>();
        var loggerMock = new Mock<ILogger<RegisterViewModel>>();
        var viewModel = new RegisterViewModel(authMock.Object, academicMock.Object, loggerMock.Object);

        // Act
        viewModel.Dob = DateTime.MinValue;
        var result = viewModel.Dob;

        // Assert
        Assert.AreEqual(DateTime.MinValue, result);
    }

    /// <summary>
    /// Tests that the Dob property can handle DateTime.MaxValue boundary.
    /// Input: DateTime.MaxValue (9999-12-31 23:59:59.9999999).
    /// Expected: The property should store and return DateTime.MaxValue without error.
    /// </summary>
    [TestMethod]
    public void Dob_SetMaxValue_ShouldHandleMaxValueBoundary()
    {
        // Arrange
        var authMock = new Mock<IAuthService>();
        var academicMock = new Mock<IAcademicService>();
        var loggerMock = new Mock<ILogger<RegisterViewModel>>();
        var viewModel = new RegisterViewModel(authMock.Object, academicMock.Object, loggerMock.Object);

        // Act
        viewModel.Dob = DateTime.MaxValue;
        var result = viewModel.Dob;

        // Assert
        Assert.AreEqual(DateTime.MaxValue, result);
    }

    /// <summary>
    /// Tests that the Dob property can handle default DateTime value.
    /// Input: default(DateTime) which equals DateTime.MinValue.
    /// Expected: The property should store and return the default value without error.
    /// </summary>
    [TestMethod]
    public void Dob_SetDefaultValue_ShouldHandleDefaultDateTime()
    {
        // Arrange
        var authMock = new Mock<IAuthService>();
        var academicMock = new Mock<IAcademicService>();
        var loggerMock = new Mock<ILogger<RegisterViewModel>>();
        var viewModel = new RegisterViewModel(authMock.Object, academicMock.Object, loggerMock.Object);

        // Act
        viewModel.Dob = default(DateTime);
        var result = viewModel.Dob;

        // Assert
        Assert.AreEqual(default(DateTime), result);
        Assert.AreEqual(DateTime.MinValue, result);
    }

    /// <summary>
    /// Tests that multiple consecutive sets of different values to the Dob property work correctly.
    /// Input: Multiple different DateTime values set consecutively.
    /// Expected: Each set should update the property correctly and raise PropertyChanged.
    /// </summary>
    [TestMethod]
    public void Dob_MultipleConsecutiveSets_ShouldUpdateCorrectly()
    {
        // Arrange
        var authMock = new Mock<IAuthService>();
        var academicMock = new Mock<IAcademicService>();
        var loggerMock = new Mock<ILogger<RegisterViewModel>>();
        var viewModel = new RegisterViewModel(authMock.Object, academicMock.Object, loggerMock.Object);

        var date1 = new DateTime(1990, 1, 1);
        var date2 = new DateTime(2000, 12, 31);
        var date3 = DateTime.MinValue;
        var date4 = DateTime.MaxValue;

        // Act & Assert
        viewModel.Dob = date1;
        Assert.AreEqual(date1, viewModel.Dob);

        viewModel.Dob = date2;
        Assert.AreEqual(date2, viewModel.Dob);

        viewModel.Dob = date3;
        Assert.AreEqual(date3, viewModel.Dob);

        viewModel.Dob = date4;
        Assert.AreEqual(date4, viewModel.Dob);
    }

    /// <summary>
    /// Tests that setting SelectedUniversity to a non-null value when it was previously null
    /// clears the collections, sets SelectedFaculty to null, and triggers LoadFacultiesAsync.
    /// </summary>
    [TestMethod]
    public void SelectedUniversity_SetNonNullValueFromNull_ClearsCollectionsAndLoadsFactulties()
    {
        // Arrange
        var authMock = new Mock<IAuthService>();
        var academicMock = new Mock<IAcademicService>();
        var loggerMock = new Mock<ILogger<RegisterViewModel>>();
        var viewModel = new RegisterViewModel(authMock.Object, academicMock.Object, loggerMock.Object);

        // Add some items to collections to verify they get cleared
        viewModel.Faculties.Add(new LookupItem { Id = "1", Name = "Faculty1" });
        viewModel.Departments.Add(new LookupItem { Id = "2", Name = "Dept1" });
        viewModel.Programs.Add(new LookupItem { Id = "3", Name = "Program1" });

        var university = new LookupItem { Id = "uni1", Name = "University1" };

        // Act
        viewModel.SelectedUniversity = university;

        // Allow async operation to start
        Thread.Sleep(50);

        // Assert
        Assert.AreEqual(university, viewModel.SelectedUniversity);
        Assert.IsNull(viewModel.SelectedFaculty);
        Assert.AreEqual(0, viewModel.Faculties.Count);
        Assert.AreEqual(0, viewModel.Departments.Count);
        Assert.AreEqual(0, viewModel.Programs.Count);
    }

    /// <summary>
    /// Tests that setting SelectedUniversity to null clears the collections,
    /// sets SelectedFaculty to null, and does not trigger LoadFacultiesAsync.
    /// </summary>
    [TestMethod]
    public void SelectedUniversity_SetNullValue_ClearsCollectionsWithoutLoadingFaculties()
    {
        // Arrange
        var authMock = new Mock<IAuthService>();
        var academicMock = new Mock<IAcademicService>();
        var loggerMock = new Mock<ILogger<RegisterViewModel>>();
        var viewModel = new RegisterViewModel(authMock.Object, academicMock.Object, loggerMock.Object);

        // Set initial value
        viewModel.SelectedUniversity = new LookupItem { Id = "uni1", Name = "University1" };

        // Add some items to collections
        viewModel.Faculties.Add(new LookupItem { Id = "1", Name = "Faculty1" });
        viewModel.Departments.Add(new LookupItem { Id = "2", Name = "Dept1" });
        viewModel.Programs.Add(new LookupItem { Id = "3", Name = "Program1" });

        // Act
        viewModel.SelectedUniversity = null;

        // Assert
        Assert.IsNull(viewModel.SelectedUniversity);
        Assert.IsNull(viewModel.SelectedFaculty);
        Assert.AreEqual(0, viewModel.Faculties.Count);
        Assert.AreEqual(0, viewModel.Departments.Count);
        Assert.AreEqual(0, viewModel.Programs.Count);
    }

    /// <summary>
    /// Tests that setting SelectedUniversity to the same value does not trigger
    /// any side effects (collections remain unchanged, no LoadFacultiesAsync call).
    /// </summary>
    [TestMethod]
    public void SelectedUniversity_SetSameValue_DoesNotTriggerSideEffects()
    {
        // Arrange
        var authMock = new Mock<IAuthService>();
        var academicMock = new Mock<IAcademicService>();
        var loggerMock = new Mock<ILogger<RegisterViewModel>>();
        var viewModel = new RegisterViewModel(authMock.Object, academicMock.Object, loggerMock.Object);

        var university = new LookupItem { Id = "uni1", Name = "University1" };
        viewModel.SelectedUniversity = university;

        // Add items to collections after initial set
        viewModel.Faculties.Add(new LookupItem { Id = "1", Name = "Faculty1" });
        viewModel.Departments.Add(new LookupItem { Id = "2", Name = "Dept1" });
        viewModel.Programs.Add(new LookupItem { Id = "3", Name = "Program1" });

        var initialFacultiesCount = viewModel.Faculties.Count;
        var initialDepartmentsCount = viewModel.Departments.Count;
        var initialProgramsCount = viewModel.Programs.Count;

        // Act
        viewModel.SelectedUniversity = university; // Set same value

        // Assert
        Assert.AreEqual(university, viewModel.SelectedUniversity);
        Assert.AreEqual(initialFacultiesCount, viewModel.Faculties.Count);
        Assert.AreEqual(initialDepartmentsCount, viewModel.Departments.Count);
        Assert.AreEqual(initialProgramsCount, viewModel.Programs.Count);
    }

    /// <summary>
    /// Tests that setting SelectedUniversity to a different non-null value
    /// clears the collections, sets SelectedFaculty to null, and triggers LoadFacultiesAsync with the new Id.
    /// </summary>
    [TestMethod]
    public void SelectedUniversity_SetDifferentNonNullValue_ClearsCollectionsAndLoadsNewFaculties()
    {
        // Arrange
        var authMock = new Mock<IAuthService>();
        var academicMock = new Mock<IAcademicService>();
        var loggerMock = new Mock<ILogger<RegisterViewModel>>();
        var viewModel = new RegisterViewModel(authMock.Object, academicMock.Object, loggerMock.Object);

        var university1 = new LookupItem { Id = "uni1", Name = "University1" };
        var university2 = new LookupItem { Id = "uni2", Name = "University2" };

        viewModel.SelectedUniversity = university1;

        // Add items to collections
        viewModel.Faculties.Add(new LookupItem { Id = "1", Name = "Faculty1" });
        viewModel.Departments.Add(new LookupItem { Id = "2", Name = "Dept1" });
        viewModel.Programs.Add(new LookupItem { Id = "3", Name = "Program1" });

        // Act
        viewModel.SelectedUniversity = university2;

        // Allow async operation to start
        Thread.Sleep(50);

        // Assert
        Assert.AreEqual(university2, viewModel.SelectedUniversity);
        Assert.IsNull(viewModel.SelectedFaculty);
        Assert.AreEqual(0, viewModel.Faculties.Count);
        Assert.AreEqual(0, viewModel.Departments.Count);
        Assert.AreEqual(0, viewModel.Programs.Count);
    }

    /// <summary>
    /// Tests that setting SelectedUniversity to null when it is already null
    /// does not trigger any side effects.
    /// </summary>
    [TestMethod]
    public void SelectedUniversity_SetNullWhenAlreadyNull_DoesNotTriggerSideEffects()
    {
        // Arrange
        var authMock = new Mock<IAuthService>();
        var academicMock = new Mock<IAcademicService>();
        var loggerMock = new Mock<ILogger<RegisterViewModel>>();
        var viewModel = new RegisterViewModel(authMock.Object, academicMock.Object, loggerMock.Object);

        // Add items to collections
        viewModel.Faculties.Add(new LookupItem { Id = "1", Name = "Faculty1" });
        viewModel.Departments.Add(new LookupItem { Id = "2", Name = "Dept1" });
        viewModel.Programs.Add(new LookupItem { Id = "3", Name = "Program1" });

        var initialFacultiesCount = viewModel.Faculties.Count;
        var initialDepartmentsCount = viewModel.Departments.Count;
        var initialProgramsCount = viewModel.Programs.Count;

        // Act
        viewModel.SelectedUniversity = null; // Set null when already null

        // Assert
        Assert.IsNull(viewModel.SelectedUniversity);
        Assert.AreEqual(initialFacultiesCount, viewModel.Faculties.Count);
        Assert.AreEqual(initialDepartmentsCount, viewModel.Departments.Count);
        Assert.AreEqual(initialProgramsCount, viewModel.Programs.Count);
    }

    /// <summary>
    /// Tests that setting SelectedUniversity with an empty Id string still triggers
    /// the collections clearing and LoadFacultiesAsync call.
    /// </summary>
    [TestMethod]
    public void SelectedUniversity_SetValueWithEmptyId_ClearsCollectionsAndTriggersLoad()
    {
        // Arrange
        var authMock = new Mock<IAuthService>();
        var academicMock = new Mock<IAcademicService>();
        var loggerMock = new Mock<ILogger<RegisterViewModel>>();
        var viewModel = new RegisterViewModel(authMock.Object, academicMock.Object, loggerMock.Object);

        // Add items to collections
        viewModel.Faculties.Add(new LookupItem { Id = "1", Name = "Faculty1" });
        viewModel.Departments.Add(new LookupItem { Id = "2", Name = "Dept1" });
        viewModel.Programs.Add(new LookupItem { Id = "3", Name = "Program1" });

        var universityWithEmptyId = new LookupItem { Id = string.Empty, Name = "University" };

        // Act
        viewModel.SelectedUniversity = universityWithEmptyId;

        // Allow async operation to start
        Thread.Sleep(50);

        // Assert
        Assert.AreEqual(universityWithEmptyId, viewModel.SelectedUniversity);
        Assert.IsNull(viewModel.SelectedFaculty);
        Assert.AreEqual(0, viewModel.Faculties.Count);
        Assert.AreEqual(0, viewModel.Departments.Count);
        Assert.AreEqual(0, viewModel.Programs.Count);
    }

    /// <summary>
    /// Tests that when SelectedFaculty is already set, setting a new SelectedUniversity
    /// resets SelectedFaculty to null.
    /// </summary>
    [TestMethod]
    public void SelectedUniversity_SetNewValue_ResetsSelectedFacultyToNull()
    {
        // Arrange
        var authMock = new Mock<IAuthService>();
        var academicMock = new Mock<IAcademicService>();
        var loggerMock = new Mock<ILogger<RegisterViewModel>>();
        var viewModel = new RegisterViewModel(authMock.Object, academicMock.Object, loggerMock.Object);

        var university1 = new LookupItem { Id = "uni1", Name = "University1" };
        var university2 = new LookupItem { Id = "uni2", Name = "University2" };
        var faculty = new LookupItem { Id = "fac1", Name = "Faculty1" };

        viewModel.SelectedUniversity = university1;
        viewModel.Faculties.Add(faculty);
        viewModel.SelectedFaculty = faculty;

        // Act
        viewModel.SelectedUniversity = university2;

        // Assert
        Assert.IsNull(viewModel.SelectedFaculty);
    }

    /// <summary>
    /// Tests that setting SelectedUniversity clears all three dependent collections:
    /// Faculties, Departments, and Programs.
    /// </summary>
    [TestMethod]
    public void SelectedUniversity_SetValue_ClearsAllDependentCollections()
    {
        // Arrange
        var authMock = new Mock<IAuthService>();
        var academicMock = new Mock<IAcademicService>();
        var loggerMock = new Mock<ILogger<RegisterViewModel>>();
        var viewModel = new RegisterViewModel(authMock.Object, academicMock.Object, loggerMock.Object);

        // Add multiple items to all collections
        for (int i = 0; i < 5; i++)
        {
            viewModel.Faculties.Add(new LookupItem { Id = $"fac{i}", Name = $"Faculty{i}" });
            viewModel.Departments.Add(new LookupItem { Id = $"dept{i}", Name = $"Department{i}" });
            viewModel.Programs.Add(new LookupItem { Id = $"prog{i}", Name = $"Program{i}" });
        }

        var university = new LookupItem { Id = "uni1", Name = "University1" };

        // Act
        viewModel.SelectedUniversity = university;

        // Assert
        Assert.AreEqual(0, viewModel.Faculties.Count, "Faculties collection should be cleared");
        Assert.AreEqual(0, viewModel.Departments.Count, "Departments collection should be cleared");
        Assert.AreEqual(0, viewModel.Programs.Count, "Programs collection should be cleared");
    }

    /// <summary>
    /// Tests that YearOfStudy property returns the correct value after being set.
    /// </summary>
    /// <param name="value">The value to set.</param>
    [TestMethod]
    [DataRow(1)]
    [DataRow(2)]
    [DataRow(5)]
    [DataRow(7)]
    [DataRow(0)]
    [DataRow(-1)]
    [DataRow(-100)]
    [DataRow(100)]
    [DataRow(int.MinValue)]
    [DataRow(int.MaxValue)]
    public void YearOfStudy_SetValue_ReturnsSetValue(int value)
    {
        // Arrange
        var mockAuth = new Mock<IAuthService>();
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<Microsoft.Extensions.Logging.ILogger<RegisterViewModel>>();
        var viewModel = new RegisterViewModel(mockAuth.Object, mockAcademic.Object, mockLogger.Object);

        // Act
        viewModel.YearOfStudy = value;

        // Assert
        Assert.AreEqual(value, viewModel.YearOfStudy);
    }

    /// <summary>
    /// Tests that YearOfStudy property has the correct default value of 1.
    /// </summary>
    [TestMethod]
    public void YearOfStudy_DefaultValue_ReturnsOne()
    {
        // Arrange
        var mockAuth = new Mock<IAuthService>();
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<Microsoft.Extensions.Logging.ILogger<RegisterViewModel>>();

        // Act
        var viewModel = new RegisterViewModel(mockAuth.Object, mockAcademic.Object, mockLogger.Object);

        // Assert
        Assert.AreEqual(1, viewModel.YearOfStudy);
    }

    /// <summary>
    /// Tests that setting YearOfStudy property raises PropertyChanged event with correct property name.
    /// </summary>
    /// <param name="value">The value to set.</param>
    [TestMethod]
    [DataRow(2)]
    [DataRow(5)]
    [DataRow(0)]
    [DataRow(-1)]
    [DataRow(int.MinValue)]
    [DataRow(int.MaxValue)]
    public void YearOfStudy_SetValue_RaisesPropertyChangedEvent(int value)
    {
        // Arrange
        var mockAuth = new Mock<IAuthService>();
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<Microsoft.Extensions.Logging.ILogger<RegisterViewModel>>();
        var viewModel = new RegisterViewModel(mockAuth.Object, mockAcademic.Object, mockLogger.Object);
        string? propertyName = null;
        viewModel.PropertyChanged += (sender, args) => propertyName = args.PropertyName;

        // Act
        viewModel.YearOfStudy = value;

        // Assert
        Assert.AreEqual(nameof(viewModel.YearOfStudy), propertyName);
    }

    /// <summary>
    /// Tests that setting YearOfStudy to the same value does not raise PropertyChanged event.
    /// </summary>
    [TestMethod]
    public void YearOfStudy_SetSameValue_DoesNotRaisePropertyChangedEvent()
    {
        // Arrange
        var mockAuth = new Mock<IAuthService>();
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<Microsoft.Extensions.Logging.ILogger<RegisterViewModel>>();
        var viewModel = new RegisterViewModel(mockAuth.Object, mockAcademic.Object, mockLogger.Object);
        viewModel.YearOfStudy = 3;
        int eventRaisedCount = 0;
        viewModel.PropertyChanged += (sender, args) => eventRaisedCount++;

        // Act
        viewModel.YearOfStudy = 3;

        // Assert
        Assert.AreEqual(0, eventRaisedCount);
    }

    /// <summary>
    /// Tests that setting YearOfStudy multiple times with different values raises PropertyChanged event each time.
    /// </summary>
    [TestMethod]
    public void YearOfStudy_SetDifferentValuesMultipleTimes_RaisesPropertyChangedEventEachTime()
    {
        // Arrange
        var mockAuth = new Mock<IAuthService>();
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<Microsoft.Extensions.Logging.ILogger<RegisterViewModel>>();
        var viewModel = new RegisterViewModel(mockAuth.Object, mockAcademic.Object, mockLogger.Object);
        int eventRaisedCount = 0;
        viewModel.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == nameof(viewModel.YearOfStudy))
            {
                eventRaisedCount++;
            }
        };

        // Act
        viewModel.YearOfStudy = 2;
        viewModel.YearOfStudy = 3;
        viewModel.YearOfStudy = 4;

        // Assert
        Assert.AreEqual(3, eventRaisedCount);
    }

    /// <summary>
    /// Tests that FirstName property returns the initial value of empty string.
    /// </summary>
    [TestMethod]
    public void FirstName_InitialValue_ReturnsEmptyString()
    {
        // Arrange
        var authMock = new Mock<IAuthService>();
        var academicMock = new Mock<IAcademicService>();
        var loggerMock = new Mock<ILogger<RegisterViewModel>>();
        var viewModel = new RegisterViewModel(authMock.Object, academicMock.Object, loggerMock.Object);

        // Act
        var result = viewModel.FirstName;

        // Assert
        Assert.AreEqual(string.Empty, result);
    }

    /// <summary>
    /// Tests that FirstName property correctly sets and returns the provided value.
    /// </summary>
    /// <param name="value">The string value to set.</param>
    [TestMethod]
    [DataRow("John")]
    [DataRow("Mary")]
    [DataRow("")]
    [DataRow(" ")]
    [DataRow("  ")]
    [DataRow("\t")]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [DataRow("A")]
    [DataRow("FirstNameWithVeryLongStringThatExceedsNormalLengthToTestBoundaryConditionsAndMemoryHandling")]
    [DataRow("名前")]
    [DataRow("José")]
    [DataRow("O'Brien")]
    [DataRow("Jean-Paul")]
    [DataRow("Name\nWith\nNewlines")]
    [DataRow("Name\tWith\tTabs")]
    [DataRow("!@#$%^&*()")]
    public void FirstName_SetValue_ReturnsSetValue(string value)
    {
        // Arrange
        var authMock = new Mock<IAuthService>();
        var academicMock = new Mock<IAcademicService>();
        var loggerMock = new Mock<ILogger<RegisterViewModel>>();
        var viewModel = new RegisterViewModel(authMock.Object, academicMock.Object, loggerMock.Object);

        // Act
        viewModel.FirstName = value;

        // Assert
        Assert.AreEqual(value, viewModel.FirstName);
    }

    /// <summary>
    /// Tests that FirstName property raises PropertyChanged event when value is changed.
    /// </summary>
    [TestMethod]
    public void FirstName_SetNewValue_RaisesPropertyChangedEvent()
    {
        // Arrange
        var authMock = new Mock<IAuthService>();
        var academicMock = new Mock<IAcademicService>();
        var loggerMock = new Mock<ILogger<RegisterViewModel>>();
        var viewModel = new RegisterViewModel(authMock.Object, academicMock.Object, loggerMock.Object);
        string? raisedPropertyName = null;
        viewModel.PropertyChanged += (sender, args) => raisedPropertyName = args.PropertyName;

        // Act
        viewModel.FirstName = "NewValue";

        // Assert
        Assert.AreEqual("FirstName", raisedPropertyName);
    }

    /// <summary>
    /// Tests that FirstName property does not raise PropertyChanged event when value is set to the same value.
    /// </summary>
    [TestMethod]
    public void FirstName_SetSameValue_DoesNotRaisePropertyChangedEvent()
    {
        // Arrange
        var authMock = new Mock<IAuthService>();
        var academicMock = new Mock<IAcademicService>();
        var loggerMock = new Mock<ILogger<RegisterViewModel>>();
        var viewModel = new RegisterViewModel(authMock.Object, academicMock.Object, loggerMock.Object);
        viewModel.FirstName = "TestValue";
        int eventRaisedCount = 0;
        viewModel.PropertyChanged += (sender, args) => eventRaisedCount++;

        // Act
        viewModel.FirstName = "TestValue";

        // Assert
        Assert.AreEqual(0, eventRaisedCount);
    }

    /// <summary>
    /// Tests that FirstName property raises PropertyChanged event multiple times when value is changed multiple times with different values.
    /// </summary>
    [TestMethod]
    public void FirstName_SetMultipleDifferentValues_RaisesPropertyChangedEventMultipleTimes()
    {
        // Arrange
        var authMock = new Mock<IAuthService>();
        var academicMock = new Mock<IAcademicService>();
        var loggerMock = new Mock<ILogger<RegisterViewModel>>();
        var viewModel = new RegisterViewModel(authMock.Object, academicMock.Object, loggerMock.Object);
        int eventRaisedCount = 0;
        viewModel.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == "FirstName")
                eventRaisedCount++;
        };

        // Act
        viewModel.FirstName = "Value1";
        viewModel.FirstName = "Value2";
        viewModel.FirstName = "Value3";

        // Assert
        Assert.AreEqual(3, eventRaisedCount);
    }

    /// <summary>
    /// Tests that FirstName property correctly updates from empty string to a non-empty value.
    /// </summary>
    [TestMethod]
    public void FirstName_SetFromEmptyToNonEmpty_UpdatesCorrectly()
    {
        // Arrange
        var authMock = new Mock<IAuthService>();
        var academicMock = new Mock<IAcademicService>();
        var loggerMock = new Mock<ILogger<RegisterViewModel>>();
        var viewModel = new RegisterViewModel(authMock.Object, academicMock.Object, loggerMock.Object);
        Assert.AreEqual(string.Empty, viewModel.FirstName);

        // Act
        viewModel.FirstName = "NewName";

        // Assert
        Assert.AreEqual("NewName", viewModel.FirstName);
    }

    /// <summary>
    /// Tests that FirstName property correctly updates from a non-empty value back to empty string.
    /// </summary>
    [TestMethod]
    public void FirstName_SetFromNonEmptyToEmpty_UpdatesCorrectly()
    {
        // Arrange
        var authMock = new Mock<IAuthService>();
        var academicMock = new Mock<IAcademicService>();
        var loggerMock = new Mock<ILogger<RegisterViewModel>>();
        var viewModel = new RegisterViewModel(authMock.Object, academicMock.Object, loggerMock.Object);
        viewModel.FirstName = "SomeName";

        // Act
        viewModel.FirstName = string.Empty;

        // Assert
        Assert.AreEqual(string.Empty, viewModel.FirstName);
    }

    /// <summary>
    /// Tests that the IsOtpVerified property can be set and retrieved correctly,
    /// and raises PropertyChanged event when the value changes.
    /// </summary>
    /// <param name="initialValue">The initial value to set.</param>
    /// <param name="newValue">The new value to set.</param>
    /// <param name="shouldRaiseEvent">Whether PropertyChanged event should be raised.</param>
    [TestMethod]
    [DataRow(false, true, true, DisplayName = "Setting from false to true raises PropertyChanged")]
    [DataRow(true, false, true, DisplayName = "Setting from true to false raises PropertyChanged")]
    [DataRow(false, false, false, DisplayName = "Setting same value false does not raise PropertyChanged")]
    [DataRow(true, true, false, DisplayName = "Setting same value true does not raise PropertyChanged")]
    public void IsOtpVerified_SetValue_RaisesPropertyChangedWhenValueChanges(bool initialValue, bool newValue, bool shouldRaiseEvent)
    {
        // Arrange
        var mockAuth = new Mock<IAuthService>();
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<RegisterViewModel>>();
        var viewModel = new RegisterViewModel(mockAuth.Object, mockAcademic.Object, mockLogger.Object);

        viewModel.IsOtpVerified = initialValue;

        string? raisedPropertyName = null;
        int eventRaisedCount = 0;
        viewModel.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == nameof(RegisterViewModel.IsOtpVerified))
            {
                raisedPropertyName = args.PropertyName;
                eventRaisedCount++;
            }
        };

        // Act
        viewModel.IsOtpVerified = newValue;

        // Assert
        Assert.AreEqual(newValue, viewModel.IsOtpVerified);

        if (shouldRaiseEvent)
        {
            Assert.AreEqual(1, eventRaisedCount, "PropertyChanged event should be raised exactly once");
            Assert.AreEqual(nameof(RegisterViewModel.IsOtpVerified), raisedPropertyName);
        }
        else
        {
            Assert.AreEqual(0, eventRaisedCount, "PropertyChanged event should not be raised when value doesn't change");
        }
    }

    /// <summary>
    /// Tests that the IsOtpVerified property has the correct default value.
    /// </summary>
    [TestMethod]
    public void IsOtpVerified_DefaultValue_IsFalse()
    {
        // Arrange
        var mockAuth = new Mock<IAuthService>();
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<RegisterViewModel>>();

        // Act
        var viewModel = new RegisterViewModel(mockAuth.Object, mockAcademic.Object, mockLogger.Object);

        // Assert
        Assert.IsFalse(viewModel.IsOtpVerified);
    }

    /// <summary>
    /// Tests that multiple consecutive sets to the same value only raise PropertyChanged once.
    /// </summary>
    [TestMethod]
    public void IsOtpVerified_SetSameValueMultipleTimes_DoesNotRaisePropertyChanged()
    {
        // Arrange
        var mockAuth = new Mock<IAuthService>();
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<RegisterViewModel>>();
        var viewModel = new RegisterViewModel(mockAuth.Object, mockAcademic.Object, mockLogger.Object);

        viewModel.IsOtpVerified = true;

        int eventRaisedCount = 0;
        viewModel.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == nameof(RegisterViewModel.IsOtpVerified))
            {
                eventRaisedCount++;
            }
        };

        // Act
        viewModel.IsOtpVerified = true;
        viewModel.IsOtpVerified = true;
        viewModel.IsOtpVerified = true;

        // Assert
        Assert.AreEqual(0, eventRaisedCount, "PropertyChanged should not be raised when setting the same value multiple times");
        Assert.IsTrue(viewModel.IsOtpVerified);
    }

    /// <summary>
    /// Tests that alternating between true and false values raises PropertyChanged for each change.
    /// </summary>
    [TestMethod]
    public void IsOtpVerified_AlternatingValues_RaisesPropertyChangedForEachChange()
    {
        // Arrange
        var mockAuth = new Mock<IAuthService>();
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<RegisterViewModel>>();
        var viewModel = new RegisterViewModel(mockAuth.Object, mockAcademic.Object, mockLogger.Object);

        int eventRaisedCount = 0;
        viewModel.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == nameof(RegisterViewModel.IsOtpVerified))
            {
                eventRaisedCount++;
            }
        };

        // Act
        viewModel.IsOtpVerified = true;
        viewModel.IsOtpVerified = false;
        viewModel.IsOtpVerified = true;
        viewModel.IsOtpVerified = false;

        // Assert
        Assert.AreEqual(4, eventRaisedCount, "PropertyChanged should be raised for each value change");
        Assert.IsFalse(viewModel.IsOtpVerified);
    }

    /// <summary>
    /// Tests that setting SelectedStudyMode with a valid LookupItem value updates the property correctly.
    /// Verifies that the property value is stored and PropertyChanged event is raised.
    /// </summary>
    [TestMethod]
    public void SelectedStudyMode_SetValidValue_StoresValueAndRaisesPropertyChanged()
    {
        // Arrange
        var mockAuth = new Mock<IAuthService>();
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<RegisterViewModel>>();
        var viewModel = new RegisterViewModel(mockAuth.Object, mockAcademic.Object, mockLogger.Object);

        var lookupItem = new LookupItem { Id = "1", Name = "Full-Time" };
        string? raisedPropertyName = null;
        viewModel.PropertyChanged += (sender, args) => raisedPropertyName = args.PropertyName;

        // Act
        viewModel.SelectedStudyMode = lookupItem;

        // Assert
        Assert.IsNotNull(viewModel.SelectedStudyMode);
        Assert.AreEqual("1", viewModel.SelectedStudyMode.Id);
        Assert.AreEqual("Full-Time", viewModel.SelectedStudyMode.Name);
        Assert.AreEqual(nameof(viewModel.SelectedStudyMode), raisedPropertyName);
    }

    /// <summary>
    /// Tests that setting SelectedStudyMode to null updates the property and raises PropertyChanged.
    /// Verifies that null values are handled correctly for this nullable property.
    /// </summary>
    [TestMethod]
    public void SelectedStudyMode_SetNull_StoresNullAndRaisesPropertyChanged()
    {
        // Arrange
        var mockAuth = new Mock<IAuthService>();
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<RegisterViewModel>>();
        var viewModel = new RegisterViewModel(mockAuth.Object, mockAcademic.Object, mockLogger.Object);

        viewModel.SelectedStudyMode = new LookupItem { Id = "1", Name = "Full-Time" };
        string? raisedPropertyName = null;
        viewModel.PropertyChanged += (sender, args) => raisedPropertyName = args.PropertyName;

        // Act
        viewModel.SelectedStudyMode = null;

        // Assert
        Assert.IsNull(viewModel.SelectedStudyMode);
        Assert.AreEqual(nameof(viewModel.SelectedStudyMode), raisedPropertyName);
    }

    /// <summary>
    /// Tests that setting SelectedStudyMode to the same value does not raise PropertyChanged event.
    /// Verifies the optimization in SetProperty that prevents unnecessary notifications.
    /// </summary>
    [TestMethod]
    public void SelectedStudyMode_SetSameValue_DoesNotRaisePropertyChanged()
    {
        // Arrange
        var mockAuth = new Mock<IAuthService>();
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<RegisterViewModel>>();
        var viewModel = new RegisterViewModel(mockAuth.Object, mockAcademic.Object, mockLogger.Object);

        var lookupItem = new LookupItem { Id = "1", Name = "Full-Time" };
        viewModel.SelectedStudyMode = lookupItem;

        int eventRaisedCount = 0;
        viewModel.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == nameof(viewModel.SelectedStudyMode))
            {
                eventRaisedCount++;
            }
        };

        // Act
        viewModel.SelectedStudyMode = lookupItem;

        // Assert
        Assert.AreEqual(0, eventRaisedCount);
        Assert.AreEqual(lookupItem, viewModel.SelectedStudyMode);
    }

    /// <summary>
    /// Tests that setting SelectedStudyMode to different LookupItem instances raises PropertyChanged each time.
    /// Verifies that different instances are recognized as different values.
    /// </summary>
    [TestMethod]
    public void SelectedStudyMode_SetDifferentValues_RaisesPropertyChangedEachTime()
    {
        // Arrange
        var mockAuth = new Mock<IAuthService>();
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<RegisterViewModel>>();
        var viewModel = new RegisterViewModel(mockAuth.Object, mockAcademic.Object, mockLogger.Object);

        var firstItem = new LookupItem { Id = "1", Name = "Full-Time" };
        var secondItem = new LookupItem { Id = "2", Name = "Part-Time" };

        int eventRaisedCount = 0;
        viewModel.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == nameof(viewModel.SelectedStudyMode))
            {
                eventRaisedCount++;
            }
        };

        // Act
        viewModel.SelectedStudyMode = firstItem;
        viewModel.SelectedStudyMode = secondItem;

        // Assert
        Assert.AreEqual(2, eventRaisedCount);
        Assert.AreEqual(secondItem, viewModel.SelectedStudyMode);
        Assert.AreEqual("2", viewModel.SelectedStudyMode.Id);
        Assert.AreEqual("Part-Time", viewModel.SelectedStudyMode.Name);
    }

    /// <summary>
    /// Tests that getting SelectedStudyMode returns the default value (null) when not explicitly set.
    /// Verifies the initial state of the property.
    /// </summary>
    [TestMethod]
    public void SelectedStudyMode_GetDefaultValue_ReturnsNull()
    {
        // Arrange
        var mockAuth = new Mock<IAuthService>();
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<RegisterViewModel>>();
        var viewModel = new RegisterViewModel(mockAuth.Object, mockAcademic.Object, mockLogger.Object);

        // Act
        var result = viewModel.SelectedStudyMode;

        // Assert
        Assert.IsNull(result);
    }

    /// <summary>
    /// Tests that setting SelectedStudyMode with empty string properties works correctly.
    /// Verifies edge case handling for LookupItem with empty Id and Name.
    /// </summary>
    [TestMethod]
    public void SelectedStudyMode_SetValueWithEmptyStrings_StoresValueAndRaisesPropertyChanged()
    {
        // Arrange
        var mockAuth = new Mock<IAuthService>();
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<RegisterViewModel>>();
        var viewModel = new RegisterViewModel(mockAuth.Object, mockAcademic.Object, mockLogger.Object);

        var lookupItem = new LookupItem { Id = string.Empty, Name = string.Empty };
        string? raisedPropertyName = null;
        viewModel.PropertyChanged += (sender, args) => raisedPropertyName = args.PropertyName;

        // Act
        viewModel.SelectedStudyMode = lookupItem;

        // Assert
        Assert.IsNotNull(viewModel.SelectedStudyMode);
        Assert.AreEqual(string.Empty, viewModel.SelectedStudyMode.Id);
        Assert.AreEqual(string.Empty, viewModel.SelectedStudyMode.Name);
        Assert.AreEqual(nameof(viewModel.SelectedStudyMode), raisedPropertyName);
    }

    /// <summary>
    /// Tests setting SelectedStudyMode with LookupItem containing special characters in Id and Name.
    /// Verifies that special characters are handled correctly.
    /// </summary>
    [TestMethod]
    public void SelectedStudyMode_SetValueWithSpecialCharacters_StoresValueCorrectly()
    {
        // Arrange
        var mockAuth = new Mock<IAuthService>();
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<RegisterViewModel>>();
        var viewModel = new RegisterViewModel(mockAuth.Object, mockAcademic.Object, mockLogger.Object);

        var lookupItem = new LookupItem { Id = "!@#$%^&*()", Name = "Study Mode with 特殊字符 and symbols" };

        // Act
        viewModel.SelectedStudyMode = lookupItem;

        // Assert
        Assert.IsNotNull(viewModel.SelectedStudyMode);
        Assert.AreEqual("!@#$%^&*()", viewModel.SelectedStudyMode.Id);
        Assert.AreEqual("Study Mode with 特殊字符 and symbols", viewModel.SelectedStudyMode.Name);
    }

    /// <summary>
    /// Tests that CurrentStep property is initialized to 1 when a new RegisterViewModel instance is created.
    /// Input: None (default constructor behavior).
    /// Expected: CurrentStep returns 1.
    /// </summary>
    [TestMethod]
    public void CurrentStep_InitialValue_ReturnsOne()
    {
        // Arrange
        var authMock = new Mock<IAuthService>();
        var academicMock = new Mock<IAcademicService>();
        var loggerMock = new Mock<ILogger<RegisterViewModel>>();

        // Act
        var viewModel = new RegisterViewModel(authMock.Object, academicMock.Object, loggerMock.Object);

        // Assert
        Assert.AreEqual(1, viewModel.CurrentStep);
    }

    /// <summary>
    /// Tests that setting CurrentStep to various valid integer values updates the property correctly.
    /// Input: Valid integer values including 0, positive, negative, and typical step values.
    /// Expected: Property value is updated to the new value.
    /// </summary>
    /// <param name="newValue">The value to set for CurrentStep.</param>
    [TestMethod]
    [DataRow(0)]
    [DataRow(1)]
    [DataRow(2)]
    [DataRow(3)]
    [DataRow(4)]
    [DataRow(5)]
    [DataRow(10)]
    [DataRow(-1)]
    [DataRow(-5)]
    [DataRow(100)]
    [DataRow(-100)]
    public void CurrentStep_SetValidValue_UpdatesProperty(int newValue)
    {
        // Arrange
        var authMock = new Mock<IAuthService>();
        var academicMock = new Mock<IAcademicService>();
        var loggerMock = new Mock<ILogger<RegisterViewModel>>();
        var viewModel = new RegisterViewModel(authMock.Object, academicMock.Object, loggerMock.Object);

        // Act
        viewModel.CurrentStep = newValue;

        // Assert
        Assert.AreEqual(newValue, viewModel.CurrentStep);
    }

    /// <summary>
    /// Tests that setting CurrentStep to extreme boundary values (int.MinValue and int.MaxValue) updates the property correctly.
    /// Input: int.MinValue and int.MaxValue.
    /// Expected: Property value is updated to the extreme value.
    /// </summary>
    /// <param name="extremeValue">The extreme boundary value to set.</param>
    [TestMethod]
    [DataRow(int.MinValue)]
    [DataRow(int.MaxValue)]
    public void CurrentStep_SetBoundaryValue_UpdatesProperty(int extremeValue)
    {
        // Arrange
        var authMock = new Mock<IAuthService>();
        var academicMock = new Mock<IAcademicService>();
        var loggerMock = new Mock<ILogger<RegisterViewModel>>();
        var viewModel = new RegisterViewModel(authMock.Object, academicMock.Object, loggerMock.Object);

        // Act
        viewModel.CurrentStep = extremeValue;

        // Assert
        Assert.AreEqual(extremeValue, viewModel.CurrentStep);
    }

    /// <summary>
    /// Tests that setting CurrentStep to a different value raises the PropertyChanged event with the correct property name.
    /// Input: A new value different from the current value.
    /// Expected: PropertyChanged event is raised with PropertyName = "CurrentStep".
    /// </summary>
    [TestMethod]
    public void CurrentStep_SetDifferentValue_RaisesPropertyChangedEvent()
    {
        // Arrange
        var authMock = new Mock<IAuthService>();
        var academicMock = new Mock<IAcademicService>();
        var loggerMock = new Mock<ILogger<RegisterViewModel>>();
        var viewModel = new RegisterViewModel(authMock.Object, academicMock.Object, loggerMock.Object);
        string? raisedPropertyName = null;
        viewModel.PropertyChanged += (sender, e) => raisedPropertyName = e.PropertyName;

        // Act
        viewModel.CurrentStep = 2;

        // Assert
        Assert.AreEqual("CurrentStep", raisedPropertyName);
    }

    /// <summary>
    /// Tests that setting CurrentStep to the same value does not raise the PropertyChanged event.
    /// Input: The same value as the current value.
    /// Expected: PropertyChanged event is not raised.
    /// </summary>
    [TestMethod]
    public void CurrentStep_SetSameValue_DoesNotRaisePropertyChangedEvent()
    {
        // Arrange
        var authMock = new Mock<IAuthService>();
        var academicMock = new Mock<IAcademicService>();
        var loggerMock = new Mock<ILogger<RegisterViewModel>>();
        var viewModel = new RegisterViewModel(authMock.Object, academicMock.Object, loggerMock.Object);
        viewModel.CurrentStep = 2;
        int eventCount = 0;
        viewModel.PropertyChanged += (sender, e) => eventCount++;

        // Act
        viewModel.CurrentStep = 2;

        // Assert
        Assert.AreEqual(0, eventCount);
    }

    /// <summary>
    /// Tests that PropertyChanged event has the correct sender when CurrentStep is changed.
    /// Input: A new value different from current.
    /// Expected: PropertyChanged event sender is the viewModel instance.
    /// </summary>
    [TestMethod]
    public void CurrentStep_SetValue_RaisesPropertyChangedEventWithCorrectSender()
    {
        // Arrange
        var authMock = new Mock<IAuthService>();
        var academicMock = new Mock<IAcademicService>();
        var loggerMock = new Mock<ILogger<RegisterViewModel>>();
        var viewModel = new RegisterViewModel(authMock.Object, academicMock.Object, loggerMock.Object);
        object? eventSender = null;
        viewModel.PropertyChanged += (sender, e) => eventSender = sender;

        // Act
        viewModel.CurrentStep = 2;

        // Assert
        Assert.AreSame(viewModel, eventSender);
    }

    /// <summary>
    /// Tests that setting CurrentStep multiple times to different values updates the property each time and raises PropertyChanged event.
    /// Input: Sequential different values.
    /// Expected: Property is updated each time and PropertyChanged is raised for each change.
    /// </summary>
    [TestMethod]
    public void CurrentStep_SetMultipleDifferentValues_UpdatesAndRaisesEventEachTime()
    {
        // Arrange
        var authMock = new Mock<IAuthService>();
        var academicMock = new Mock<IAcademicService>();
        var loggerMock = new Mock<ILogger<RegisterViewModel>>();
        var viewModel = new RegisterViewModel(authMock.Object, academicMock.Object, loggerMock.Object);
        int eventCount = 0;
        viewModel.PropertyChanged += (sender, e) => { if (e.PropertyName == "CurrentStep") eventCount++; };

        // Act
        viewModel.CurrentStep = 2;
        viewModel.CurrentStep = 3;
        viewModel.CurrentStep = 1;

        // Assert
        Assert.AreEqual(1, viewModel.CurrentStep);
        Assert.AreEqual(3, eventCount);
    }

    /// <summary>
    /// Tests that IsStep1 returns true when CurrentStep is 1 and false for all other values.
    /// Input: Various CurrentStep values.
    /// Expected: IsStep1 is true only when CurrentStep equals 1.
    /// </summary>
    /// <param name="currentStepValue">The value to set for CurrentStep.</param>
    /// <param name="expectedIsStep1">The expected return value of IsStep1.</param>
    [TestMethod]
    [DataRow(1, true)]
    [DataRow(0, false)]
    [DataRow(2, false)]
    [DataRow(3, false)]
    [DataRow(-1, false)]
    [DataRow(int.MinValue, false)]
    [DataRow(int.MaxValue, false)]
    public void CurrentStep_SetValue_UpdatesIsStep1Correctly(int currentStepValue, bool expectedIsStep1)
    {
        // Arrange
        var authMock = new Mock<IAuthService>();
        var academicMock = new Mock<IAcademicService>();
        var loggerMock = new Mock<ILogger<RegisterViewModel>>();
        var viewModel = new RegisterViewModel(authMock.Object, academicMock.Object, loggerMock.Object);

        // Act
        viewModel.CurrentStep = currentStepValue;

        // Assert
        Assert.AreEqual(expectedIsStep1, viewModel.IsStep1);
    }

    /// <summary>
    /// Tests that IsStep2 returns true when CurrentStep is 2 and false for all other values.
    /// Input: Various CurrentStep values.
    /// Expected: IsStep2 is true only when CurrentStep equals 2.
    /// </summary>
    /// <param name="currentStepValue">The value to set for CurrentStep.</param>
    /// <param name="expectedIsStep2">The expected return value of IsStep2.</param>
    [TestMethod]
    [DataRow(2, true)]
    [DataRow(0, false)]
    [DataRow(1, false)]
    [DataRow(3, false)]
    [DataRow(-1, false)]
    [DataRow(int.MinValue, false)]
    [DataRow(int.MaxValue, false)]
    public void CurrentStep_SetValue_UpdatesIsStep2Correctly(int currentStepValue, bool expectedIsStep2)
    {
        // Arrange
        var authMock = new Mock<IAuthService>();
        var academicMock = new Mock<IAcademicService>();
        var loggerMock = new Mock<ILogger<RegisterViewModel>>();
        var viewModel = new RegisterViewModel(authMock.Object, academicMock.Object, loggerMock.Object);

        // Act
        viewModel.CurrentStep = currentStepValue;

        // Assert
        Assert.AreEqual(expectedIsStep2, viewModel.IsStep2);
    }

    /// <summary>
    /// Tests that IsStep3 returns true when CurrentStep is 3 and false for all other values.
    /// Input: Various CurrentStep values.
    /// Expected: IsStep3 is true only when CurrentStep equals 3.
    /// </summary>
    /// <param name="currentStepValue">The value to set for CurrentStep.</param>
    /// <param name="expectedIsStep3">The expected return value of IsStep3.</param>
    [TestMethod]
    [DataRow(3, true)]
    [DataRow(0, false)]
    [DataRow(1, false)]
    [DataRow(2, false)]
    [DataRow(-1, false)]
    [DataRow(int.MinValue, false)]
    [DataRow(int.MaxValue, false)]
    public void CurrentStep_SetValue_UpdatesIsStep3Correctly(int currentStepValue, bool expectedIsStep3)
    {
        // Arrange
        var authMock = new Mock<IAuthService>();
        var academicMock = new Mock<IAcademicService>();
        var loggerMock = new Mock<ILogger<RegisterViewModel>>();
        var viewModel = new RegisterViewModel(authMock.Object, academicMock.Object, loggerMock.Object);

        // Act
        viewModel.CurrentStep = currentStepValue;

        // Assert
        Assert.AreEqual(expectedIsStep3, viewModel.IsStep3);
    }

    /// <summary>
    /// Tests that all step indicator properties (IsStep1, IsStep2, IsStep3) update correctly when CurrentStep changes.
    /// Input: Setting CurrentStep to 1, 2, and 3 sequentially.
    /// Expected: Only the corresponding IsStepN property is true for each value.
    /// </summary>
    [TestMethod]
    public void CurrentStep_SetToSequentialSteps_UpdatesAllStepIndicatorsCorrectly()
    {
        // Arrange
        var authMock = new Mock<IAuthService>();
        var academicMock = new Mock<IAcademicService>();
        var loggerMock = new Mock<ILogger<RegisterViewModel>>();
        var viewModel = new RegisterViewModel(authMock.Object, academicMock.Object, loggerMock.Object);

        // Act & Assert - Step 1
        viewModel.CurrentStep = 1;
        Assert.IsTrue(viewModel.IsStep1);
        Assert.IsFalse(viewModel.IsStep2);
        Assert.IsFalse(viewModel.IsStep3);

        // Act & Assert - Step 2
        viewModel.CurrentStep = 2;
        Assert.IsFalse(viewModel.IsStep1);
        Assert.IsTrue(viewModel.IsStep2);
        Assert.IsFalse(viewModel.IsStep3);

        // Act & Assert - Step 3
        viewModel.CurrentStep = 3;
        Assert.IsFalse(viewModel.IsStep1);
        Assert.IsFalse(viewModel.IsStep2);
        Assert.IsTrue(viewModel.IsStep3);
    }

    /// <summary>
    /// Tests that setting CurrentStep to zero correctly updates the property and all step indicators are false.
    /// Input: 0.
    /// Expected: CurrentStep is 0 and all IsStepN properties are false.
    /// </summary>
    [TestMethod]
    public void CurrentStep_SetToZero_UpdatesPropertyAndAllStepIndicatorsFalse()
    {
        // Arrange
        var authMock = new Mock<IAuthService>();
        var academicMock = new Mock<IAcademicService>();
        var loggerMock = new Mock<ILogger<RegisterViewModel>>();
        var viewModel = new RegisterViewModel(authMock.Object, academicMock.Object, loggerMock.Object);

        // Act
        viewModel.CurrentStep = 0;

        // Assert
        Assert.AreEqual(0, viewModel.CurrentStep);
        Assert.IsFalse(viewModel.IsStep1);
        Assert.IsFalse(viewModel.IsStep2);
        Assert.IsFalse(viewModel.IsStep3);
    }

    /// <summary>
    /// Tests that setting CurrentStep to negative values correctly updates the property and all step indicators are false.
    /// Input: Various negative integer values.
    /// Expected: CurrentStep is updated to negative value and all IsStepN properties are false.
    /// </summary>
    /// <param name="negativeValue">The negative value to set.</param>
    [TestMethod]
    [DataRow(-1)]
    [DataRow(-5)]
    [DataRow(-100)]
    [DataRow(int.MinValue)]
    public void CurrentStep_SetToNegativeValue_UpdatesPropertyAndAllStepIndicatorsFalse(int negativeValue)
    {
        // Arrange
        var authMock = new Mock<IAuthService>();
        var academicMock = new Mock<IAcademicService>();
        var loggerMock = new Mock<ILogger<RegisterViewModel>>();
        var viewModel = new RegisterViewModel(authMock.Object, academicMock.Object, loggerMock.Object);

        // Act
        viewModel.CurrentStep = negativeValue;

        // Assert
        Assert.AreEqual(negativeValue, viewModel.CurrentStep);
        Assert.IsFalse(viewModel.IsStep1);
        Assert.IsFalse(viewModel.IsStep2);
        Assert.IsFalse(viewModel.IsStep3);
    }

    /// <summary>
    /// Tests that getting CurrentStep returns the correct value after multiple sets.
    /// Input: Sequential value changes.
    /// Expected: Get returns the most recently set value.
    /// </summary>
    [TestMethod]
    public void CurrentStep_GetAfterMultipleSets_ReturnsLastSetValue()
    {
        // Arrange
        var authMock = new Mock<IAuthService>();
        var academicMock = new Mock<IAcademicService>();
        var loggerMock = new Mock<ILogger<RegisterViewModel>>();
        var viewModel = new RegisterViewModel(authMock.Object, academicMock.Object, loggerMock.Object);

        // Act
        viewModel.CurrentStep = 2;
        viewModel.CurrentStep = 5;
        viewModel.CurrentStep = -3;
        int result = viewModel.CurrentStep;

        // Assert
        Assert.AreEqual(-3, result);
    }

    /// <summary>
    /// Tests that setting CurrentStep to the same value multiple times only raises PropertyChanged once (on first change from initial value).
    /// Input: Same value set multiple times.
    /// Expected: PropertyChanged is raised only when value actually changes.
    /// </summary>
    [TestMethod]
    public void CurrentStep_SetSameValueMultipleTimes_RaisesEventOnlyOnChange()
    {
        // Arrange
        var authMock = new Mock<IAuthService>();
        var academicMock = new Mock<IAcademicService>();
        var loggerMock = new Mock<ILogger<RegisterViewModel>>();
        var viewModel = new RegisterViewModel(authMock.Object, academicMock.Object, loggerMock.Object);
        int eventCount = 0;
        viewModel.PropertyChanged += (sender, e) => { if (e.PropertyName == "CurrentStep") eventCount++; };

        // Act
        viewModel.CurrentStep = 2;  // First change, should raise event
        viewModel.CurrentStep = 2;  // Same value, should not raise
        viewModel.CurrentStep = 2;  // Same value, should not raise

        // Assert
        Assert.AreEqual(1, eventCount);
    }

    /// <summary>
    /// Tests that the Phone property returns empty string as its default initial value.
    /// Input: None (testing initial state).
    /// Expected: Phone property returns empty string.
    /// </summary>
    [TestMethod]
    public void Phone_InitialValue_ReturnsEmptyString()
    {
        // Arrange
        var mockAuthService = new Mock<IAuthService>();
        var mockAcademicService = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<RegisterViewModel>>();

        // Act
        var viewModel = new RegisterViewModel(mockAuthService.Object, mockAcademicService.Object, mockLogger.Object);

        // Assert
        Assert.AreEqual(string.Empty, viewModel.Phone);
    }

    /// <summary>
    /// Tests that setting the Phone property to a valid phone number updates the property correctly.
    /// Input: Various valid phone number formats.
    /// Expected: Property value is updated to the provided value.
    /// </summary>
    /// <param name="phoneNumber">The phone number to set.</param>
    [TestMethod]
    [DataRow("+1234567890")]
    [DataRow("0700123456")]
    [DataRow("+256 700 123 456")]
    [DataRow("+1-555-123-4567")]
    [DataRow("(555) 123-4567")]
    [DataRow("555.123.4567")]
    [DataRow("+44-20-1234-5678")]
    [DataRow("1234567890")]
    public void Phone_SetValidPhoneNumber_UpdatesProperty(string phoneNumber)
    {
        // Arrange
        var mockAuthService = new Mock<IAuthService>();
        var mockAcademicService = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<RegisterViewModel>>();
        var viewModel = new RegisterViewModel(mockAuthService.Object, mockAcademicService.Object, mockLogger.Object);

        // Act
        viewModel.Phone = phoneNumber;

        // Assert
        Assert.AreEqual(phoneNumber, viewModel.Phone);
    }

    /// <summary>
    /// Tests that setting the Phone property to an empty string updates the property correctly.
    /// Input: Empty string.
    /// Expected: Property value is set to empty string.
    /// </summary>
    [TestMethod]
    public void Phone_SetEmptyString_UpdatesProperty()
    {
        // Arrange
        var mockAuthService = new Mock<IAuthService>();
        var mockAcademicService = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<RegisterViewModel>>();
        var viewModel = new RegisterViewModel(mockAuthService.Object, mockAcademicService.Object, mockLogger.Object);
        viewModel.Phone = "+1234567890";

        // Act
        viewModel.Phone = string.Empty;

        // Assert
        Assert.AreEqual(string.Empty, viewModel.Phone);
    }

    /// <summary>
    /// Tests that the Phone property handles whitespace-only strings correctly.
    /// Input: Various whitespace-only strings (spaces, tabs, newlines).
    /// Expected: Property value is updated to the whitespace string.
    /// </summary>
    /// <param name="whitespace">The whitespace string to test.</param>
    [TestMethod]
    [DataRow(" ")]
    [DataRow("   ")]
    [DataRow("\t")]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [DataRow(" \t\n ")]
    public void Phone_SetWhitespaceString_UpdatesProperty(string whitespace)
    {
        // Arrange
        var mockAuthService = new Mock<IAuthService>();
        var mockAcademicService = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<RegisterViewModel>>();
        var viewModel = new RegisterViewModel(mockAuthService.Object, mockAcademicService.Object, mockLogger.Object);

        // Act
        viewModel.Phone = whitespace;

        // Assert
        Assert.AreEqual(whitespace, viewModel.Phone);
    }

    /// <summary>
    /// Tests that the Phone property handles very long strings correctly.
    /// Input: A string with 10000 characters.
    /// Expected: Property value is updated to the very long string.
    /// </summary>
    [TestMethod]
    public void Phone_SetVeryLongString_UpdatesProperty()
    {
        // Arrange
        var mockAuthService = new Mock<IAuthService>();
        var mockAcademicService = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<RegisterViewModel>>();
        var viewModel = new RegisterViewModel(mockAuthService.Object, mockAcademicService.Object, mockLogger.Object);
        var veryLongPhone = new string('1', 10000);

        // Act
        viewModel.Phone = veryLongPhone;

        // Assert
        Assert.AreEqual(veryLongPhone, viewModel.Phone);
    }

    /// <summary>
    /// Tests that the Phone property handles special characters commonly used in phone numbers.
    /// Input: Phone numbers with special formatting characters (+, -, (), spaces, dots).
    /// Expected: Property value is updated correctly with special characters.
    /// </summary>
    /// <param name="phoneWithSpecialChars">Phone number with special characters.</param>
    [TestMethod]
    [DataRow("+1 (555) 123-4567")]
    [DataRow("+44-20-1234-5678")]
    [DataRow("(555) 123-4567")]
    [DataRow("555.123.4567")]
    [DataRow("+256 700 123 456")]
    [DataRow("+1-800-FLOWERS")]
    [DataRow("*#123#")]
    public void Phone_SetPhoneWithSpecialCharacters_UpdatesProperty(string phoneWithSpecialChars)
    {
        // Arrange
        var mockAuthService = new Mock<IAuthService>();
        var mockAcademicService = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<RegisterViewModel>>();
        var viewModel = new RegisterViewModel(mockAuthService.Object, mockAcademicService.Object, mockLogger.Object);

        // Act
        viewModel.Phone = phoneWithSpecialChars;

        // Assert
        Assert.AreEqual(phoneWithSpecialChars, viewModel.Phone);
    }

    /// <summary>
    /// Tests that the Phone property handles strings with control characters.
    /// Input: Strings containing control characters (null terminator, bell, etc.).
    /// Expected: Property value is updated to include control characters.
    /// </summary>
    [TestMethod]
    public void Phone_SetStringWithControlCharacters_UpdatesProperty()
    {
        // Arrange
        var mockAuthService = new Mock<IAuthService>();
        var mockAcademicService = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<RegisterViewModel>>();
        var viewModel = new RegisterViewModel(mockAuthService.Object, mockAcademicService.Object, mockLogger.Object);
        var phoneWithControlChars = "123\u0000456\u0007";

        // Act
        viewModel.Phone = phoneWithControlChars;

        // Assert
        Assert.AreEqual(phoneWithControlChars, viewModel.Phone);
    }

    /// <summary>
    /// Tests that the Phone property handles invalid or unusual characters.
    /// Input: Strings with letters, symbols, emojis, and other invalid characters.
    /// Expected: Property value is updated (no validation is enforced by the property).
    /// </summary>
    /// <param name="invalidPhone">Phone string with invalid characters.</param>
    [TestMethod]
    [DataRow("abc123")]
    [DataRow("!@#$%^&*()")]
    [DataRow("phone number")]
    [DataRow("123-abc-4567")]
    [DataRow("📱1234567890")]
    [DataRow("αβγδ1234")]
    [DataRow("<script>alert('xss')</script>")]
    public void Phone_SetInvalidCharacters_UpdatesProperty(string invalidPhone)
    {
        // Arrange
        var mockAuthService = new Mock<IAuthService>();
        var mockAcademicService = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<RegisterViewModel>>();
        var viewModel = new RegisterViewModel(mockAuthService.Object, mockAcademicService.Object, mockLogger.Object);

        // Act
        viewModel.Phone = invalidPhone;

        // Assert
        Assert.AreEqual(invalidPhone, viewModel.Phone);
    }

    /// <summary>
    /// Tests that the Phone property can be updated multiple times with different values
    /// and raises PropertyChanged event for each distinct change.
    /// Input: Multiple different phone number values set sequentially.
    /// Expected: Property is updated for each set and PropertyChanged is raised for each change.
    /// </summary>
    [TestMethod]
    public void Phone_SetMultipleDifferentValues_RaisesPropertyChangedForEachChange()
    {
        // Arrange
        var mockAuthService = new Mock<IAuthService>();
        var mockAcademicService = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<RegisterViewModel>>();
        var viewModel = new RegisterViewModel(mockAuthService.Object, mockAcademicService.Object, mockLogger.Object);
        int eventRaisedCount = 0;
        viewModel.PropertyChanged += (sender, args) => { if (args.PropertyName == "Phone") eventRaisedCount++; };

        // Act
        viewModel.Phone = "+1234567890";
        viewModel.Phone = "+9876543210";
        viewModel.Phone = "(555) 123-4567";

        // Assert
        Assert.AreEqual(3, eventRaisedCount);
        Assert.AreEqual("(555) 123-4567", viewModel.Phone);
    }

    /// <summary>
    /// Tests that the Phone property handles Unicode characters correctly.
    /// Input: Strings with various Unicode characters.
    /// Expected: Property value is updated to include Unicode characters.
    /// </summary>
    /// <param name="unicodePhone">Phone string with Unicode characters.</param>
    [TestMethod]
    [DataRow("你好1234567890")]
    [DataRow("日本語123")]
    [DataRow("한글-555-1234")]
    [DataRow("Ñoño: +123456")]
    [DataRow("Müller: 1234567")]
    public void Phone_SetUnicodeCharacters_UpdatesProperty(string unicodePhone)
    {
        // Arrange
        var mockAuthService = new Mock<IAuthService>();
        var mockAcademicService = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<RegisterViewModel>>();
        var viewModel = new RegisterViewModel(mockAuthService.Object, mockAcademicService.Object, mockLogger.Object);

        // Act
        viewModel.Phone = unicodePhone;

        // Assert
        Assert.AreEqual(unicodePhone, viewModel.Phone);
    }

    /// <summary>
    /// Tests that setting the Phone property to the same empty string multiple times
    /// does not raise PropertyChanged event after the first set.
    /// Input: Empty string set when property is already empty.
    /// Expected: PropertyChanged is not raised.
    /// </summary>
    [TestMethod]
    public void Phone_SetEmptyStringWhenAlreadyEmpty_DoesNotRaisePropertyChanged()
    {
        // Arrange
        var mockAuthService = new Mock<IAuthService>();
        var mockAcademicService = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<RegisterViewModel>>();
        var viewModel = new RegisterViewModel(mockAuthService.Object, mockAcademicService.Object, mockLogger.Object);
        int eventRaisedCount = 0;
        viewModel.PropertyChanged += (sender, args) => { if (args.PropertyName == "Phone") eventRaisedCount++; };

        // Act
        viewModel.Phone = string.Empty;

        // Assert
        Assert.AreEqual(0, eventRaisedCount);
    }

    /// <summary>
    /// Tests that setting the Phone property with boundary length strings works correctly.
    /// Input: Strings at common phone number length boundaries.
    /// Expected: Property value is updated correctly.
    /// </summary>
    /// <param name="length">The length of the phone number string.</param>
    [TestMethod]
    [DataRow(0)]
    [DataRow(1)]
    [DataRow(10)]
    [DataRow(15)]
    [DataRow(20)]
    [DataRow(50)]
    [DataRow(100)]
    public void Phone_SetVariableLengthStrings_UpdatesProperty(int length)
    {
        // Arrange
        var mockAuthService = new Mock<IAuthService>();
        var mockAcademicService = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<RegisterViewModel>>();
        var viewModel = new RegisterViewModel(mockAuthService.Object, mockAcademicService.Object, mockLogger.Object);
        var phoneNumber = new string('1', length);

        // Act
        viewModel.Phone = phoneNumber;

        // Assert
        Assert.AreEqual(phoneNumber, viewModel.Phone);
        Assert.AreEqual(length, viewModel.Phone.Length);
    }

    /// <summary>
    /// Tests that setting SelectedFaculty with a LookupItem having null Id throws no exception
    /// but may cause issues when LoadDepartmentsAsync is called.
    /// Input: LookupItem with null Id.
    /// Expected: Property is set, but behavior depends on LoadDepartmentsAsync null handling.
    /// </summary>
    [TestMethod]
    public void SelectedFaculty_SetWithNullId_HandlesGracefully()
    {
        // Arrange
        var mockAuthService = new Mock<IAuthService>();
        var mockAcademicService = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<RegisterViewModel>>();
        var viewModel = new RegisterViewModel(mockAuthService.Object, mockAcademicService.Object, mockLogger.Object);

        var facultyWithNullId = new LookupItem { Id = null!, Name = "Test Faculty" };

        // Act & Assert - This should not throw during property set
        viewModel.SelectedFaculty = facultyWithNullId;

        // Assert
        Assert.AreEqual(facultyWithNullId, viewModel.SelectedFaculty);
        Assert.IsNull(viewModel.SelectedDepartment);
        Assert.AreEqual(0, viewModel.Departments.Count);
        Assert.AreEqual(0, viewModel.Programs.Count);
    }

    /// <summary>
    /// Tests that setting SelectedFaculty with LookupItem having whitespace-only Id
    /// still triggers LoadDepartmentsAsync with that whitespace Id.
    /// Input: LookupItem with whitespace-only Id (spaces, tabs, newlines).
    /// Expected: Property is set and LoadDepartmentsAsync is called with whitespace Id.
    /// </summary>
    [TestMethod]
    [DataRow("   ")]
    [DataRow("\t")]
    [DataRow("\n")]
    [DataRow(" \t\n ")]
    public void SelectedFaculty_SetWithWhitespaceId_TriggersLoadDepartments(string whitespaceId)
    {
        // Arrange
        var mockAuthService = new Mock<IAuthService>();
        var mockAcademicService = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<RegisterViewModel>>();
        var viewModel = new RegisterViewModel(mockAuthService.Object, mockAcademicService.Object, mockLogger.Object);

        var faculty = new LookupItem { Id = whitespaceId, Name = "Test Faculty" };

        // Act
        viewModel.SelectedFaculty = faculty;

        // Assert
        Assert.AreEqual(faculty, viewModel.SelectedFaculty);
        Assert.IsNull(viewModel.SelectedDepartment);
        Assert.AreEqual(0, viewModel.Departments.Count);
        Assert.AreEqual(0, viewModel.Programs.Count);
    }

    /// <summary>
    /// Tests that setting SelectedFaculty with LookupItem having very long Id
    /// handles the scenario correctly.
    /// Input: LookupItem with very long Id string (10000 characters).
    /// Expected: Property is set and LoadDepartmentsAsync is called with the long Id.
    /// </summary>
    [TestMethod]
    public void SelectedFaculty_SetWithVeryLongId_HandlesCorrectly()
    {
        // Arrange
        var mockAuthService = new Mock<IAuthService>();
        var mockAcademicService = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<RegisterViewModel>>();
        var viewModel = new RegisterViewModel(mockAuthService.Object, mockAcademicService.Object, mockLogger.Object);

        var veryLongId = new string('a', 10000);
        var faculty = new LookupItem { Id = veryLongId, Name = "Test Faculty" };

        // Act
        viewModel.SelectedFaculty = faculty;

        // Assert
        Assert.AreEqual(faculty, viewModel.SelectedFaculty);
        Assert.IsNull(viewModel.SelectedDepartment);
        Assert.AreEqual(0, viewModel.Departments.Count);
        Assert.AreEqual(0, viewModel.Programs.Count);
    }

    /// <summary>
    /// Tests that setting SelectedFaculty with LookupItem having special characters in Id
    /// handles the scenario correctly.
    /// Input: LookupItem with special characters in Id.
    /// Expected: Property is set and LoadDepartmentsAsync is called with the special character Id.
    /// </summary>
    [TestMethod]
    [DataRow("!@#$%^&*()")]
    [DataRow("id-with-dashes")]
    [DataRow("id_with_underscores")]
    [DataRow("id.with.dots")]
    [DataRow("id/with/slashes")]
    [DataRow("id\\with\\backslashes")]
    [DataRow("id with spaces")]
    [DataRow("id\twith\ttabs")]
    [DataRow("id\nwith\nnewlines")]
    public void SelectedFaculty_SetWithSpecialCharactersInId_HandlesCorrectly(string specialId)
    {
        // Arrange
        var mockAuthService = new Mock<IAuthService>();
        var mockAcademicService = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<RegisterViewModel>>();
        var viewModel = new RegisterViewModel(mockAuthService.Object, mockAcademicService.Object, mockLogger.Object);

        var faculty = new LookupItem { Id = specialId, Name = "Test Faculty" };

        // Act
        viewModel.SelectedFaculty = faculty;

        // Assert
        Assert.AreEqual(faculty, viewModel.SelectedFaculty);
        Assert.IsNull(viewModel.SelectedDepartment);
        Assert.AreEqual(0, viewModel.Departments.Count);
        Assert.AreEqual(0, viewModel.Programs.Count);
    }

    /// <summary>
    /// Tests that setting SelectedFaculty clears Departments collection even when it contains
    /// a large number of items.
    /// Input: Departments collection with 1000 items, then set new faculty.
    /// Expected: All items are cleared from Departments collection.
    /// </summary>
    [TestMethod]
    public void SelectedFaculty_SetWithLargeDepartmentsCollection_ClearsAllItems()
    {
        // Arrange
        var mockAuthService = new Mock<IAuthService>();
        var mockAcademicService = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<RegisterViewModel>>();
        var viewModel = new RegisterViewModel(mockAuthService.Object, mockAcademicService.Object, mockLogger.Object);

        // Add 1000 items to Departments
        for (int i = 0; i < 1000; i++)
        {
            viewModel.Departments.Add(new LookupItem { Id = $"dept-{i}", Name = $"Department {i}" });
        }

        var faculty = new LookupItem { Id = "faculty-1", Name = "Faculty 1" };

        // Act
        viewModel.SelectedFaculty = faculty;

        // Assert
        Assert.AreEqual(0, viewModel.Departments.Count);
        Assert.AreEqual(0, viewModel.Programs.Count);
        Assert.IsNull(viewModel.SelectedDepartment);
    }

    /// <summary>
    /// Tests that setting SelectedFaculty clears Programs collection even when it contains
    /// a large number of items.
    /// Input: Programs collection with 1000 items, then set new faculty.
    /// Expected: All items are cleared from Programs collection.
    /// </summary>
    [TestMethod]
    public void SelectedFaculty_SetWithLargeProgramsCollection_ClearsAllItems()
    {
        // Arrange
        var mockAuthService = new Mock<IAuthService>();
        var mockAcademicService = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<RegisterViewModel>>();
        var viewModel = new RegisterViewModel(mockAuthService.Object, mockAcademicService.Object, mockLogger.Object);

        // Add 1000 items to Programs
        for (int i = 0; i < 1000; i++)
        {
            viewModel.Programs.Add(new LookupItem { Id = $"prog-{i}", Name = $"Program {i}" });
        }

        var faculty = new LookupItem { Id = "faculty-1", Name = "Faculty 1" };

        // Act
        viewModel.SelectedFaculty = faculty;

        // Assert
        Assert.AreEqual(0, viewModel.Programs.Count);
        Assert.AreEqual(0, viewModel.Departments.Count);
        Assert.IsNull(viewModel.SelectedDepartment);
    }

    /// <summary>
    /// Tests that setting SelectedFaculty when SelectedDepartment is already null
    /// still sets SelectedDepartment to null (idempotent operation).
    /// Input: SelectedDepartment is null, then set new faculty.
    /// Expected: SelectedDepartment remains null, collections cleared.
    /// </summary>
    [TestMethod]
    public void SelectedFaculty_SetWhenSelectedDepartmentIsNull_SetsSelectedDepartmentToNull()
    {
        // Arrange
        var mockAuthService = new Mock<IAuthService>();
        var mockAcademicService = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<RegisterViewModel>>();
        var viewModel = new RegisterViewModel(mockAuthService.Object, mockAcademicService.Object, mockLogger.Object);

        Assert.IsNull(viewModel.SelectedDepartment); // Verify it's null initially

        var faculty = new LookupItem { Id = "faculty-1", Name = "Faculty 1" };

        // Act
        viewModel.SelectedFaculty = faculty;

        // Assert
        Assert.IsNull(viewModel.SelectedDepartment);
        Assert.AreEqual(0, viewModel.Departments.Count);
        Assert.AreEqual(0, viewModel.Programs.Count);
    }

    /// <summary>
    /// Tests that setting SelectedFaculty when SelectedDepartment has a value
    /// correctly resets SelectedDepartment to null.
    /// Input: SelectedDepartment has a value, then set new faculty.
    /// Expected: SelectedDepartment is set to null, collections cleared.
    /// </summary>
    [TestMethod]
    public void SelectedFaculty_SetWhenSelectedDepartmentHasValue_ResetsSelectedDepartmentToNull()
    {
        // Arrange
        var mockAuthService = new Mock<IAuthService>();
        var mockAcademicService = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<RegisterViewModel>>();
        var viewModel = new RegisterViewModel(mockAuthService.Object, mockAcademicService.Object, mockLogger.Object);

        var initialDepartment = new LookupItem { Id = "dept-1", Name = "Department 1" };
        viewModel.Departments.Add(initialDepartment);

        // Manually set SelectedDepartment through reflection or direct access
        // Since we can't directly access the setter without triggering its logic,
        // we'll set a faculty first, then add a department and select it
        var initialFaculty = new LookupItem { Id = "faculty-0", Name = "Faculty 0" };
        viewModel.SelectedFaculty = initialFaculty;
        viewModel.Departments.Add(initialDepartment);
        viewModel.SelectedDepartment = initialDepartment;

        Assert.IsNotNull(viewModel.SelectedDepartment);

        var newFaculty = new LookupItem { Id = "faculty-1", Name = "Faculty 1" };

        // Act
        viewModel.SelectedFaculty = newFaculty;

        // Assert
        Assert.IsNull(viewModel.SelectedDepartment);
        Assert.AreEqual(0, viewModel.Departments.Count);
        Assert.AreEqual(0, viewModel.Programs.Count);
    }

    /// <summary>
    /// Tests that PropertyChanged event contains correct property name when SelectedFaculty changes.
    /// Input: Set SelectedFaculty to a new value.
    /// Expected: PropertyChanged event is raised with "SelectedFaculty" as property name.
    /// </summary>
    [TestMethod]
    public void SelectedFaculty_SetNewValue_RaisesPropertyChangedWithCorrectPropertyName()
    {
        // Arrange
        var mockAuthService = new Mock<IAuthService>();
        var mockAcademicService = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<RegisterViewModel>>();
        var viewModel = new RegisterViewModel(mockAuthService.Object, mockAcademicService.Object, mockLogger.Object);

        string? raisedPropertyName = null;
        viewModel.PropertyChanged += (sender, e) => raisedPropertyName = e.PropertyName;

        var faculty = new LookupItem { Id = "faculty-1", Name = "Faculty 1" };

        // Act
        viewModel.SelectedFaculty = faculty;

        // Assert
        Assert.AreEqual("SelectedFaculty", raisedPropertyName);
    }

    /// <summary>
    /// Tests that setting SelectedFaculty to null after having a value
    /// raises PropertyChanged event with correct property name.
    /// Input: Set SelectedFaculty to value then to null.
    /// Expected: PropertyChanged event is raised with "SelectedFaculty" as property name.
    /// </summary>
    [TestMethod]
    public void SelectedFaculty_SetToNullAfterValue_RaisesPropertyChangedWithCorrectPropertyName()
    {
        // Arrange
        var mockAuthService = new Mock<IAuthService>();
        var mockAcademicService = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<RegisterViewModel>>();
        var viewModel = new RegisterViewModel(mockAuthService.Object, mockAcademicService.Object, mockLogger.Object);

        var faculty = new LookupItem { Id = "faculty-1", Name = "Faculty 1" };
        viewModel.SelectedFaculty = faculty;

        string? raisedPropertyName = null;
        viewModel.PropertyChanged += (sender, e) => raisedPropertyName = e.PropertyName;

        // Act
        viewModel.SelectedFaculty = null;

        // Assert
        Assert.AreEqual("SelectedFaculty", raisedPropertyName);
    }

    /// <summary>
    /// Tests that setting YearOfStudy to various valid integer values updates the property correctly.
    /// Input: Various integer values including typical values, zero, negative values, and boundary values.
    /// Expected: Property value is updated to the new value.
    /// </summary>
    /// <param name="value">The value to set on YearOfStudy property.</param>
    [TestMethod]
    [DataRow(1)]
    [DataRow(2)]
    [DataRow(3)]
    [DataRow(4)]
    [DataRow(5)]
    [DataRow(6)]
    [DataRow(7)]
    [DataRow(8)]
    [DataRow(0)]
    [DataRow(-1)]
    [DataRow(-10)]
    [DataRow(-100)]
    [DataRow(10)]
    [DataRow(100)]
    [DataRow(1000)]
    [DataRow(int.MinValue)]
    [DataRow(int.MaxValue)]
    public void YearOfStudy_SetValue_UpdatesPropertyCorrectly(int value)
    {
        // Arrange
        var mockAuthService = new Mock<IAuthService>();
        var mockAcademicService = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<RegisterViewModel>>();
        var viewModel = new RegisterViewModel(mockAuthService.Object, mockAcademicService.Object, mockLogger.Object);

        // Act
        viewModel.YearOfStudy = value;

        // Assert
        Assert.AreEqual(value, viewModel.YearOfStudy);
    }

    /// <summary>
    /// Tests that setting YearOfStudy to a different value raises PropertyChanged event with correct property name.
    /// Input: A new value different from the default value.
    /// Expected: PropertyChanged event is raised with PropertyName = "YearOfStudy".
    /// </summary>
    [TestMethod]
    public void YearOfStudy_SetDifferentValue_RaisesPropertyChangedEvent()
    {
        // Arrange
        var mockAuthService = new Mock<IAuthService>();
        var mockAcademicService = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<RegisterViewModel>>();
        var viewModel = new RegisterViewModel(mockAuthService.Object, mockAcademicService.Object, mockLogger.Object);
        string? raisedPropertyName = null;
        viewModel.PropertyChanged += (sender, args) => raisedPropertyName = args.PropertyName;

        // Act
        viewModel.YearOfStudy = 3;

        // Assert
        Assert.AreEqual("YearOfStudy", raisedPropertyName);
    }

    /// <summary>
    /// Tests that setting YearOfStudy multiple times with different values raises PropertyChanged event for each change.
    /// Input: Sequential different values (1, 2, 3, 4).
    /// Expected: PropertyChanged is raised for each value change.
    /// </summary>
    [TestMethod]
    public void YearOfStudy_SetMultipleDifferentValues_RaisesPropertyChangedEventForEachChange()
    {
        // Arrange
        var mockAuthService = new Mock<IAuthService>();
        var mockAcademicService = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<RegisterViewModel>>();
        var viewModel = new RegisterViewModel(mockAuthService.Object, mockAcademicService.Object, mockLogger.Object);
        int eventRaisedCount = 0;
        viewModel.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == "YearOfStudy")
                eventRaisedCount++;
        };

        // Act
        viewModel.YearOfStudy = 2;
        viewModel.YearOfStudy = 3;
        viewModel.YearOfStudy = 4;

        // Assert
        Assert.AreEqual(3, eventRaisedCount);
    }

    /// <summary>
    /// Tests that setting YearOfStudy to zero (edge case) updates the property correctly.
    /// Input: 0.
    /// Expected: Property value is updated to 0.
    /// </summary>
    [TestMethod]
    public void YearOfStudy_SetToZero_UpdatesPropertyCorrectly()
    {
        // Arrange
        var mockAuthService = new Mock<IAuthService>();
        var mockAcademicService = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<RegisterViewModel>>();
        var viewModel = new RegisterViewModel(mockAuthService.Object, mockAcademicService.Object, mockLogger.Object);

        // Act
        viewModel.YearOfStudy = 0;

        // Assert
        Assert.AreEqual(0, viewModel.YearOfStudy);
    }

    /// <summary>
    /// Tests that setting YearOfStudy to negative values (edge case) updates the property correctly.
    /// Input: Various negative integer values.
    /// Expected: Property value is updated to the negative value.
    /// </summary>
    /// <param name="negativeValue">The negative value to set.</param>
    [TestMethod]
    [DataRow(-1)]
    [DataRow(-5)]
    [DataRow(-100)]
    [DataRow(-1000)]
    [DataRow(int.MinValue)]
    public void YearOfStudy_SetToNegativeValue_UpdatesPropertyCorrectly(int negativeValue)
    {
        // Arrange
        var mockAuthService = new Mock<IAuthService>();
        var mockAcademicService = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<RegisterViewModel>>();
        var viewModel = new RegisterViewModel(mockAuthService.Object, mockAcademicService.Object, mockLogger.Object);

        // Act
        viewModel.YearOfStudy = negativeValue;

        // Assert
        Assert.AreEqual(negativeValue, viewModel.YearOfStudy);
    }

    /// <summary>
    /// Tests that setting YearOfStudy to int.MinValue boundary value updates the property correctly.
    /// Input: int.MinValue.
    /// Expected: Property value is updated to int.MinValue.
    /// </summary>
    [TestMethod]
    public void YearOfStudy_SetToMinValue_UpdatesPropertyCorrectly()
    {
        // Arrange
        var mockAuthService = new Mock<IAuthService>();
        var mockAcademicService = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<RegisterViewModel>>();
        var viewModel = new RegisterViewModel(mockAuthService.Object, mockAcademicService.Object, mockLogger.Object);

        // Act
        viewModel.YearOfStudy = int.MinValue;

        // Assert
        Assert.AreEqual(int.MinValue, viewModel.YearOfStudy);
    }

    /// <summary>
    /// Tests that setting YearOfStudy to int.MaxValue boundary value updates the property correctly.
    /// Input: int.MaxValue.
    /// Expected: Property value is updated to int.MaxValue.
    /// </summary>
    [TestMethod]
    public void YearOfStudy_SetToMaxValue_UpdatesPropertyCorrectly()
    {
        // Arrange
        var mockAuthService = new Mock<IAuthService>();
        var mockAcademicService = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<RegisterViewModel>>();
        var viewModel = new RegisterViewModel(mockAuthService.Object, mockAcademicService.Object, mockLogger.Object);

        // Act
        viewModel.YearOfStudy = int.MaxValue;

        // Assert
        Assert.AreEqual(int.MaxValue, viewModel.YearOfStudy);
    }

    /// <summary>
    /// Tests that getting YearOfStudy returns the most recently set value after multiple sets.
    /// Input: Sequential value changes (1 -> 2 -> 5 -> 3).
    /// Expected: Getter returns the most recently set value.
    /// </summary>
    [TestMethod]
    public void YearOfStudy_GetAfterMultipleSets_ReturnsLastSetValue()
    {
        // Arrange
        var mockAuthService = new Mock<IAuthService>();
        var mockAcademicService = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<RegisterViewModel>>();
        var viewModel = new RegisterViewModel(mockAuthService.Object, mockAcademicService.Object, mockLogger.Object);

        // Act
        viewModel.YearOfStudy = 2;
        viewModel.YearOfStudy = 5;
        viewModel.YearOfStudy = 3;

        // Assert
        Assert.AreEqual(3, viewModel.YearOfStudy);
    }

    /// <summary>
    /// Tests that setting YearOfStudy to typical academic year values (1-7) works correctly.
    /// Input: Typical academic year values.
    /// Expected: Property value is updated correctly for each typical value.
    /// </summary>
    /// <param name="typicalValue">A typical academic year value.</param>
    [TestMethod]
    [DataRow(1)]
    [DataRow(2)]
    [DataRow(3)]
    [DataRow(4)]
    [DataRow(5)]
    [DataRow(6)]
    [DataRow(7)]
    public void YearOfStudy_SetTypicalAcademicYearValue_UpdatesPropertyCorrectly(int typicalValue)
    {
        // Arrange
        var mockAuthService = new Mock<IAuthService>();
        var mockAcademicService = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<RegisterViewModel>>();
        var viewModel = new RegisterViewModel(mockAuthService.Object, mockAcademicService.Object, mockLogger.Object);

        // Act
        viewModel.YearOfStudy = typicalValue;

        // Assert
        Assert.AreEqual(typicalValue, viewModel.YearOfStudy);
    }

    /// <summary>
    /// Tests that setting YearOfStudy from default value (1) to same value (1) does not raise PropertyChanged event.
    /// Input: Default value (1) set again.
    /// Expected: PropertyChanged event is not raised.
    /// </summary>
    [TestMethod]
    public void YearOfStudy_SetDefaultValueAgain_DoesNotRaisePropertyChangedEvent()
    {
        // Arrange
        var mockAuthService = new Mock<IAuthService>();
        var mockAcademicService = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<RegisterViewModel>>();
        var viewModel = new RegisterViewModel(mockAuthService.Object, mockAcademicService.Object, mockLogger.Object);
        int eventRaisedCount = 0;
        viewModel.PropertyChanged += (sender, args) => eventRaisedCount++;

        // Act
        viewModel.YearOfStudy = 1;

        // Assert
        Assert.AreEqual(0, eventRaisedCount);
    }

    /// <summary>
    /// Tests that PropertyChanged event contains correct sender when YearOfStudy changes.
    /// Input: A new value different from default.
    /// Expected: PropertyChanged event sender is the viewModel instance.
    /// </summary>
    [TestMethod]
    public void YearOfStudy_SetValue_PropertyChangedEventHasCorrectSender()
    {
        // Arrange
        var mockAuthService = new Mock<IAuthService>();
        var mockAcademicService = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<RegisterViewModel>>();
        var viewModel = new RegisterViewModel(mockAuthService.Object, mockAcademicService.Object, mockLogger.Object);
        object? eventSender = null;
        viewModel.PropertyChanged += (sender, args) => eventSender = sender;

        // Act
        viewModel.YearOfStudy = 4;

        // Assert
        Assert.AreSame(viewModel, eventSender);
    }

    /// <summary>
    /// Tests that setting SelectedStudyMode to null when already null does not raise PropertyChanged event.
    /// Verifies that redundant null assignments are optimized.
    /// </summary>
    [TestMethod]
    public void SelectedStudyMode_SetNullWhenAlreadyNull_DoesNotRaisePropertyChanged()
    {
        // Arrange
        var mockAuthService = new Mock<IAuthService>();
        var mockAcademicService = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<RegisterViewModel>>();
        var viewModel = new RegisterViewModel(mockAuthService.Object, mockAcademicService.Object, mockLogger.Object);
        var propertyChangedCount = 0;
        viewModel.PropertyChanged += (sender, e) =>
        {
            if (e.PropertyName == "SelectedStudyMode")
                propertyChangedCount++;
        };

        // Act
        viewModel.SelectedStudyMode = null;

        // Assert
        Assert.IsNull(viewModel.SelectedStudyMode);
        Assert.AreEqual(0, propertyChangedCount);
    }

    /// <summary>
    /// Tests that setting SelectedStudyMode from non-null to null raises PropertyChanged event.
    /// Verifies state transition handling.
    /// </summary>
    [TestMethod]
    public void SelectedStudyMode_SetFromNonNullToNull_RaisesPropertyChanged()
    {
        // Arrange
        var mockAuthService = new Mock<IAuthService>();
        var mockAcademicService = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<RegisterViewModel>>();
        var viewModel = new RegisterViewModel(mockAuthService.Object, mockAcademicService.Object, mockLogger.Object);
        var lookupItem = new LookupItem { Id = "online", Name = "Online" };
        viewModel.SelectedStudyMode = lookupItem;
        var propertyChangedRaised = false;
        viewModel.PropertyChanged += (sender, e) =>
        {
            if (e.PropertyName == "SelectedStudyMode")
                propertyChangedRaised = true;
        };

        // Act
        viewModel.SelectedStudyMode = null;

        // Assert
        Assert.IsNull(viewModel.SelectedStudyMode);
        Assert.IsTrue(propertyChangedRaised);
    }

    /// <summary>
    /// Tests that setting SelectedStudyMode from null to non-null raises PropertyChanged event.
    /// Verifies state transition handling from initial null state.
    /// </summary>
    [TestMethod]
    public void SelectedStudyMode_SetFromNullToNonNull_RaisesPropertyChanged()
    {
        // Arrange
        var mockAuthService = new Mock<IAuthService>();
        var mockAcademicService = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<RegisterViewModel>>();
        var viewModel = new RegisterViewModel(mockAuthService.Object, mockAcademicService.Object, mockLogger.Object);
        var lookupItem = new LookupItem { Id = "hybrid", Name = "Hybrid" };
        var propertyChangedRaised = false;
        viewModel.PropertyChanged += (sender, e) =>
        {
            if (e.PropertyName == "SelectedStudyMode")
                propertyChangedRaised = true;
        };

        // Act
        viewModel.SelectedStudyMode = lookupItem;

        // Assert
        Assert.AreEqual(lookupItem, viewModel.SelectedStudyMode);
        Assert.IsTrue(propertyChangedRaised);
    }

    /// <summary>
    /// Tests that setting SelectedStudyMode with LookupItem containing very long strings works correctly.
    /// Verifies boundary condition handling for string length.
    /// </summary>
    [TestMethod]
    public void SelectedStudyMode_SetValueWithVeryLongStrings_StoresValueCorrectly()
    {
        // Arrange
        var mockAuthService = new Mock<IAuthService>();
        var mockAcademicService = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<RegisterViewModel>>();
        var viewModel = new RegisterViewModel(mockAuthService.Object, mockAcademicService.Object, mockLogger.Object);
        var longId = new string('A', 10000);
        var longName = new string('B', 10000);
        var lookupItem = new LookupItem { Id = longId, Name = longName };

        // Act
        viewModel.SelectedStudyMode = lookupItem;

        // Assert
        Assert.AreEqual(lookupItem, viewModel.SelectedStudyMode);
        Assert.AreEqual(longId, viewModel.SelectedStudyMode.Id);
        Assert.AreEqual(longName, viewModel.SelectedStudyMode.Name);
    }

    /// <summary>
    /// Tests that setting SelectedStudyMode with LookupItem containing whitespace-only strings works correctly.
    /// Verifies handling of whitespace edge cases.
    /// </summary>
    [TestMethod]
    [DataRow("   ", "   ")]
    [DataRow("\t", "\n")]
    [DataRow(" \t\n ", " \r\n ")]
    public void SelectedStudyMode_SetValueWithWhitespaceStrings_StoresValueCorrectly(string id, string name)
    {
        // Arrange
        var mockAuthService = new Mock<IAuthService>();
        var mockAcademicService = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<RegisterViewModel>>();
        var viewModel = new RegisterViewModel(mockAuthService.Object, mockAcademicService.Object, mockLogger.Object);
        var lookupItem = new LookupItem { Id = id, Name = name };

        // Act
        viewModel.SelectedStudyMode = lookupItem;

        // Assert
        Assert.AreEqual(lookupItem, viewModel.SelectedStudyMode);
        Assert.AreEqual(id, viewModel.SelectedStudyMode.Id);
        Assert.AreEqual(name, viewModel.SelectedStudyMode.Name);
    }

    /// <summary>
    /// Tests that setting SelectedStudyMode with different LookupItem instances having same property values
    /// raises PropertyChanged due to reference inequality.
    /// Verifies reference-based comparison in SetProperty.
    /// </summary>
    [TestMethod]
    public void SelectedStudyMode_SetDifferentInstancesSameValues_RaisesPropertyChanged()
    {
        // Arrange
        var mockAuthService = new Mock<IAuthService>();
        var mockAcademicService = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<RegisterViewModel>>();
        var viewModel = new RegisterViewModel(mockAuthService.Object, mockAcademicService.Object, mockLogger.Object);
        var lookupItem1 = new LookupItem { Id = "evening", Name = "Evening" };
        var lookupItem2 = new LookupItem { Id = "evening", Name = "Evening" };
        viewModel.SelectedStudyMode = lookupItem1;
        var propertyChangedRaised = false;
        viewModel.PropertyChanged += (sender, e) =>
        {
            if (e.PropertyName == "SelectedStudyMode")
                propertyChangedRaised = true;
        };

        // Act
        viewModel.SelectedStudyMode = lookupItem2;

        // Assert
        Assert.AreEqual(lookupItem2, viewModel.SelectedStudyMode);
        Assert.IsTrue(propertyChangedRaised);
    }

    /// <summary>
    /// Tests that SelectedStudyMode property correctly handles multiple rapid value changes.
    /// Verifies that PropertyChanged is raised for each distinct change.
    /// </summary>
    [TestMethod]
    public void SelectedStudyMode_MultipleRapidChanges_RaisesPropertyChangedForEachChange()
    {
        // Arrange
        var mockAuthService = new Mock<IAuthService>();
        var mockAcademicService = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<RegisterViewModel>>();
        var viewModel = new RegisterViewModel(mockAuthService.Object, mockAcademicService.Object, mockLogger.Object);
        var lookupItem1 = new LookupItem { Id = "fulltime", Name = "Full Time" };
        var lookupItem2 = new LookupItem { Id = "parttime", Name = "Part Time" };
        var lookupItem3 = new LookupItem { Id = "online", Name = "Online" };
        var propertyChangedCount = 0;
        viewModel.PropertyChanged += (sender, e) =>
        {
            if (e.PropertyName == "SelectedStudyMode")
                propertyChangedCount++;
        };

        // Act
        viewModel.SelectedStudyMode = lookupItem1;
        viewModel.SelectedStudyMode = lookupItem2;
        viewModel.SelectedStudyMode = lookupItem3;
        viewModel.SelectedStudyMode = null;

        // Assert
        Assert.IsNull(viewModel.SelectedStudyMode);
        Assert.AreEqual(4, propertyChangedCount);
    }

    /// <summary>
    /// Tests that SelectedStudyMode property handles LookupItem with control characters correctly.
    /// Verifies handling of control characters in string properties.
    /// </summary>
    [TestMethod]
    public void SelectedStudyMode_SetValueWithControlCharacters_StoresValueCorrectly()
    {
        // Arrange
        var mockAuthService = new Mock<IAuthService>();
        var mockAcademicService = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<RegisterViewModel>>();
        var viewModel = new RegisterViewModel(mockAuthService.Object, mockAcademicService.Object, mockLogger.Object);
        var lookupItem = new LookupItem { Id = "id\u0000\u0001\u0002", Name = "name\r\n\t" };

        // Act
        viewModel.SelectedStudyMode = lookupItem;

        // Assert
        Assert.AreEqual(lookupItem, viewModel.SelectedStudyMode);
        Assert.AreEqual("id\u0000\u0001\u0002", viewModel.SelectedStudyMode.Id);
        Assert.AreEqual("name\r\n\t", viewModel.SelectedStudyMode.Name);
    }

    /// <summary>
    /// Tests that OnPropertyChanged raises PropertyChanged event with valid property name.
    /// Input: Valid property name string.
    /// Expected: PropertyChanged event is raised with the specified property name.
    /// </summary>
    [TestMethod]
    [DataRow("FirstName")]
    [DataRow("LastName")]
    [DataRow("Email")]
    [DataRow("Password")]
    [DataRow("Phone")]
    [DataRow("Gender")]
    [DataRow("CurrentStep")]
    [DataRow("IsOtpSent")]
    [DataRow("IsOtpVerified")]
    [DataRow("OtpCode")]
    [DataRow("SelectedUniversity")]
    [DataRow("SelectedFaculty")]
    [DataRow("SelectedDepartment")]
    [DataRow("SelectedProgram")]
    [DataRow("ErrorMessage")]
    public void OnPropertyChanged_ValidPropertyName_RaisesPropertyChangedEvent(string propertyName)
    {
        // Arrange
        Mock<IAuthService> mockAuthService = new Mock<IAuthService>();
        Mock<IAcademicService> mockAcademicService = new Mock<IAcademicService>();
        Mock<ILogger<RegisterViewModel>> mockLogger = new Mock<ILogger<RegisterViewModel>>();
        TestableRegisterViewModel viewModel = new TestableRegisterViewModel(mockAuthService.Object, mockAcademicService.Object, mockLogger.Object);
        bool eventRaised = false;
        string? raisedPropertyName = null;

        viewModel.PropertyChanged += (sender, e) =>
        {
            eventRaised = true;
            raisedPropertyName = e.PropertyName;
        };

        // Act
        viewModel.ExposedOnPropertyChanged(propertyName);

        // Assert
        Assert.IsTrue(eventRaised, "PropertyChanged event should be raised");
        Assert.AreEqual(propertyName, raisedPropertyName, "PropertyName should match the input parameter");
    }

    /// <summary>
    /// Tests that OnPropertyChanged raises PropertyChanged event with empty string.
    /// Input: Empty string.
    /// Expected: PropertyChanged event is raised with empty string as property name.
    /// </summary>
    [TestMethod]
    public void OnPropertyChanged_EmptyString_RaisesPropertyChangedEvent()
    {
        // Arrange
        Mock<IAuthService> mockAuthService = new Mock<IAuthService>();
        Mock<IAcademicService> mockAcademicService = new Mock<IAcademicService>();
        Mock<ILogger<RegisterViewModel>> mockLogger = new Mock<ILogger<RegisterViewModel>>();
        TestableRegisterViewModel viewModel = new TestableRegisterViewModel(mockAuthService.Object, mockAcademicService.Object, mockLogger.Object);
        bool eventRaised = false;
        string? raisedPropertyName = null;

        viewModel.PropertyChanged += (sender, e) =>
        {
            eventRaised = true;
            raisedPropertyName = e.PropertyName;
        };

        // Act
        viewModel.ExposedOnPropertyChanged(string.Empty);

        // Assert
        Assert.IsTrue(eventRaised, "PropertyChanged event should be raised");
        Assert.AreEqual(string.Empty, raisedPropertyName, "PropertyName should be empty string");
    }

    /// <summary>
    /// Tests that OnPropertyChanged raises PropertyChanged event with whitespace strings.
    /// Input: Various whitespace-only strings.
    /// Expected: PropertyChanged event is raised with the whitespace string as property name.
    /// </summary>
    [TestMethod]
    [DataRow(" ")]
    [DataRow("   ")]
    [DataRow("\t")]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [DataRow(" \t\n ")]
    public void OnPropertyChanged_WhitespaceString_RaisesPropertyChangedEvent(string propertyName)
    {
        // Arrange
        Mock<IAuthService> mockAuthService = new Mock<IAuthService>();
        Mock<IAcademicService> mockAcademicService = new Mock<IAcademicService>();
        Mock<ILogger<RegisterViewModel>> mockLogger = new Mock<ILogger<RegisterViewModel>>();
        TestableRegisterViewModel viewModel = new TestableRegisterViewModel(mockAuthService.Object, mockAcademicService.Object, mockLogger.Object);
        bool eventRaised = false;
        string? raisedPropertyName = null;

        viewModel.PropertyChanged += (sender, e) =>
        {
            eventRaised = true;
            raisedPropertyName = e.PropertyName;
        };

        // Act
        viewModel.ExposedOnPropertyChanged(propertyName);

        // Assert
        Assert.IsTrue(eventRaised, "PropertyChanged event should be raised");
        Assert.AreEqual(propertyName, raisedPropertyName, "PropertyName should match the input parameter");
    }

    /// <summary>
    /// Tests that OnPropertyChanged raises event with very long property name strings.
    /// Input: Very long string (1000+ characters).
    /// Expected: PropertyChanged event is raised with the entire long string as property name.
    /// </summary>
    [TestMethod]
    public void OnPropertyChanged_VeryLongString_RaisesPropertyChangedEvent()
    {
        // Arrange
        Mock<IAuthService> mockAuthService = new Mock<IAuthService>();
        Mock<IAcademicService> mockAcademicService = new Mock<IAcademicService>();
        Mock<ILogger<RegisterViewModel>> mockLogger = new Mock<ILogger<RegisterViewModel>>();
        TestableRegisterViewModel viewModel = new TestableRegisterViewModel(mockAuthService.Object, mockAcademicService.Object, mockLogger.Object);
        string longPropertyName = new string('A', 10000);
        bool eventRaised = false;
        string? raisedPropertyName = null;

        viewModel.PropertyChanged += (sender, e) =>
        {
            eventRaised = true;
            raisedPropertyName = e.PropertyName;
        };

        // Act
        viewModel.ExposedOnPropertyChanged(longPropertyName);

        // Assert
        Assert.IsTrue(eventRaised, "PropertyChanged event should be raised");
        Assert.AreEqual(longPropertyName, raisedPropertyName, "PropertyName should match the input parameter");
    }

    /// <summary>
    /// Tests that OnPropertyChanged raises event with special characters in property name.
    /// Input: Strings with special characters.
    /// Expected: PropertyChanged event is raised with the special characters preserved.
    /// </summary>
    [TestMethod]
    [DataRow("Property!@#$%^&*()")]
    [DataRow("Property<>?:\"{}|")]
    [DataRow("Property~`")]
    [DataRow("Property_With_Underscores")]
    [DataRow("Property-With-Dashes")]
    public void OnPropertyChanged_SpecialCharacters_RaisesPropertyChangedEvent(string propertyName)
    {
        // Arrange
        Mock<IAuthService> mockAuthService = new Mock<IAuthService>();
        Mock<IAcademicService> mockAcademicService = new Mock<IAcademicService>();
        Mock<ILogger<RegisterViewModel>> mockLogger = new Mock<ILogger<RegisterViewModel>>();
        TestableRegisterViewModel viewModel = new TestableRegisterViewModel(mockAuthService.Object, mockAcademicService.Object, mockLogger.Object);
        bool eventRaised = false;
        string? raisedPropertyName = null;

        viewModel.PropertyChanged += (sender, e) =>
        {
            eventRaised = true;
            raisedPropertyName = e.PropertyName;
        };

        // Act
        viewModel.ExposedOnPropertyChanged(propertyName);

        // Assert
        Assert.IsTrue(eventRaised, "PropertyChanged event should be raised");
        Assert.AreEqual(propertyName, raisedPropertyName, "PropertyName should match the input parameter");
    }

    /// <summary>
    /// Tests that OnPropertyChanged raises event with Unicode characters in property name.
    /// Input: Strings with Unicode characters.
    /// Expected: PropertyChanged event is raised with Unicode characters preserved.
    /// </summary>
    [TestMethod]
    [DataRow("Property名前")]
    [DataRow("PropertyПривет")]
    [DataRow("Property你好")]
    [DataRow("Property😀")]
    [DataRow("Propertyñáéíóú")]
    public void OnPropertyChanged_UnicodeCharacters_RaisesPropertyChangedEvent(string propertyName)
    {
        // Arrange
        Mock<IAuthService> mockAuthService = new Mock<IAuthService>();
        Mock<IAcademicService> mockAcademicService = new Mock<IAcademicService>();
        Mock<ILogger<RegisterViewModel>> mockLogger = new Mock<ILogger<RegisterViewModel>>();
        TestableRegisterViewModel viewModel = new TestableRegisterViewModel(mockAuthService.Object, mockAcademicService.Object, mockLogger.Object);
        bool eventRaised = false;
        string? raisedPropertyName = null;

        viewModel.PropertyChanged += (sender, e) =>
        {
            eventRaised = true;
            raisedPropertyName = e.PropertyName;
        };

        // Act
        viewModel.ExposedOnPropertyChanged(propertyName);

        // Assert
        Assert.IsTrue(eventRaised, "PropertyChanged event should be raised");
        Assert.AreEqual(propertyName, raisedPropertyName, "PropertyName should match the input parameter");
    }

    /// <summary>
    /// Tests that OnPropertyChanged can be called multiple times with the same property name.
    /// Input: Same property name called multiple times.
    /// Expected: PropertyChanged event is raised each time, even with duplicate names.
    /// </summary>
    [TestMethod]
    public void OnPropertyChanged_MultipleCallsWithSameName_RaisesEventForEach()
    {
        // Arrange
        Mock<IAuthService> mockAuthService = new Mock<IAuthService>();
        Mock<IAcademicService> mockAcademicService = new Mock<IAcademicService>();
        Mock<ILogger<RegisterViewModel>> mockLogger = new Mock<ILogger<RegisterViewModel>>();
        TestableRegisterViewModel viewModel = new TestableRegisterViewModel(mockAuthService.Object, mockAcademicService.Object, mockLogger.Object);
        int eventRaisedCount = 0;

        viewModel.PropertyChanged += (sender, e) =>
        {
            eventRaisedCount++;
        };

        // Act
        viewModel.ExposedOnPropertyChanged("FirstName");
        viewModel.ExposedOnPropertyChanged("FirstName");
        viewModel.ExposedOnPropertyChanged("FirstName");

        // Assert
        Assert.AreEqual(3, eventRaisedCount, "PropertyChanged event should be raised three times");
    }

    /// <summary>
    /// Tests that OnPropertyChanged handles control characters in property name.
    /// Input: Strings with control characters.
    /// Expected: PropertyChanged event is raised with control characters preserved.
    /// </summary>
    [TestMethod]
    [DataRow("Property\u0000WithNull")]
    [DataRow("Property\u0001WithControlChar")]
    [DataRow("Property\u001FWithUnitSeparator")]
    public void OnPropertyChanged_ControlCharacters_RaisesPropertyChangedEvent(string propertyName)
    {
        // Arrange
        Mock<IAuthService> mockAuthService = new Mock<IAuthService>();
        Mock<IAcademicService> mockAcademicService = new Mock<IAcademicService>();
        Mock<ILogger<RegisterViewModel>> mockLogger = new Mock<ILogger<RegisterViewModel>>();
        TestableRegisterViewModel viewModel = new TestableRegisterViewModel(mockAuthService.Object, mockAcademicService.Object, mockLogger.Object);
        bool eventRaised = false;
        string? raisedPropertyName = null;

        viewModel.PropertyChanged += (sender, e) =>
        {
            eventRaised = true;
            raisedPropertyName = e.PropertyName;
        };

        // Act
        viewModel.ExposedOnPropertyChanged(propertyName);

        // Assert
        Assert.IsTrue(eventRaised, "PropertyChanged event should be raised");
        Assert.AreEqual(propertyName, raisedPropertyName, "PropertyName should match the input parameter");
    }

    /// <summary>
    /// Tests that IsStep3 correctly reflects changes when CurrentStep is updated from a non-3 value to 3.
    /// Input: CurrentStep transitions from 1 to 3.
    /// Expected: IsStep3 changes from false to true.
    /// </summary>
    [TestMethod]
    public void IsStep3_WhenCurrentStepChangesTo3_ReturnsTrue()
    {
        // Arrange
        var authServiceMock = new Mock<IAuthService>();
        var academicServiceMock = new Mock<IAcademicService>();
        var loggerMock = new Mock<ILogger<RegisterViewModel>>();
        var viewModel = new RegisterViewModel(authServiceMock.Object, academicServiceMock.Object, loggerMock.Object);
        viewModel.CurrentStep = 1;

        // Act
        var beforeChange = viewModel.IsStep3;
        viewModel.CurrentStep = 3;
        var afterChange = viewModel.IsStep3;

        // Assert
        Assert.IsFalse(beforeChange);
        Assert.IsTrue(afterChange);
    }

    /// <summary>
    /// Tests that IsStep3 correctly reflects changes when CurrentStep is updated from 3 to a non-3 value.
    /// Input: CurrentStep transitions from 3 to 2.
    /// Expected: IsStep3 changes from true to false.
    /// </summary>
    [TestMethod]
    public void IsStep3_WhenCurrentStepChangesFrom3_ReturnsFalse()
    {
        // Arrange
        var authServiceMock = new Mock<IAuthService>();
        var academicServiceMock = new Mock<IAcademicService>();
        var loggerMock = new Mock<ILogger<RegisterViewModel>>();
        var viewModel = new RegisterViewModel(authServiceMock.Object, academicServiceMock.Object, loggerMock.Object);
        viewModel.CurrentStep = 3;

        // Act
        var beforeChange = viewModel.IsStep3;
        viewModel.CurrentStep = 2;
        var afterChange = viewModel.IsStep3;

        // Assert
        Assert.IsTrue(beforeChange);
        Assert.IsFalse(afterChange);
    }

    /// <summary>
    /// Tests that IsStep3 returns false for the initial default value of CurrentStep.
    /// Input: Newly created ViewModel with default CurrentStep value of 1.
    /// Expected: IsStep3 returns false.
    /// </summary>
    [TestMethod]
    public void IsStep3_WithDefaultCurrentStepValue_ReturnsFalse()
    {
        // Arrange
        var authServiceMock = new Mock<IAuthService>();
        var academicServiceMock = new Mock<IAcademicService>();
        var loggerMock = new Mock<ILogger<RegisterViewModel>>();
        var viewModel = new RegisterViewModel(authServiceMock.Object, academicServiceMock.Object, loggerMock.Object);

        // Act
        var result = viewModel.IsStep3;

        // Assert
        Assert.IsFalse(result);
        Assert.AreEqual(1, viewModel.CurrentStep);
    }

    /// <summary>
    /// Tests that multiple consecutive reads of IsStep3 return consistent results when CurrentStep is 3.
    /// Input: CurrentStep = 3, reading IsStep3 multiple times.
    /// Expected: All reads return true.
    /// </summary>
    [TestMethod]
    public void IsStep3_MultipleReadsWhenCurrentStepIs3_ReturnsConsistentTrue()
    {
        // Arrange
        var authServiceMock = new Mock<IAuthService>();
        var academicServiceMock = new Mock<IAcademicService>();
        var loggerMock = new Mock<ILogger<RegisterViewModel>>();
        var viewModel = new RegisterViewModel(authServiceMock.Object, academicServiceMock.Object, loggerMock.Object);
        viewModel.CurrentStep = 3;

        // Act
        var result1 = viewModel.IsStep3;
        var result2 = viewModel.IsStep3;
        var result3 = viewModel.IsStep3;

        // Assert
        Assert.IsTrue(result1);
        Assert.IsTrue(result2);
        Assert.IsTrue(result3);
    }

    /// <summary>
    /// Tests that multiple consecutive reads of IsStep3 return consistent results when CurrentStep is not 3.
    /// Input: CurrentStep = 2, reading IsStep3 multiple times.
    /// Expected: All reads return false.
    /// </summary>
    [TestMethod]
    public void IsStep3_MultipleReadsWhenCurrentStepIsNot3_ReturnsConsistentFalse()
    {
        // Arrange
        var authServiceMock = new Mock<IAuthService>();
        var academicServiceMock = new Mock<IAcademicService>();
        var loggerMock = new Mock<ILogger<RegisterViewModel>>();
        var viewModel = new RegisterViewModel(authServiceMock.Object, academicServiceMock.Object, loggerMock.Object);
        viewModel.CurrentStep = 2;

        // Act
        var result1 = viewModel.IsStep3;
        var result2 = viewModel.IsStep3;
        var result3 = viewModel.IsStep3;

        // Assert
        Assert.IsFalse(result1);
        Assert.IsFalse(result2);
        Assert.IsFalse(result3);
    }

    /// <summary>
    /// Tests that the Email property returns an empty string as its initial default value.
    /// Input: None (initial state).
    /// Expected: Email property returns empty string.
    /// </summary>
    [TestMethod]
    public void Email_InitialValue_ReturnsEmptyString()
    {
        // Arrange
        var viewModel = new RegisterViewModel(_mockAuthService.Object, _mockAcademicService.Object, _mockLogger.Object);

        // Act
        var result = viewModel.Email;

        // Assert
        Assert.AreEqual(string.Empty, result);
    }

    /// <summary>
    /// Tests that setting the Email property to valid email format strings updates the value correctly.
    /// Input: Various valid email format strings.
    /// Expected: Property value is updated to the provided email string.
    /// </summary>
    /// <param name="email">The email value to set.</param>
    [TestMethod]
    [DataRow("test@example.com")]
    [DataRow("user.name@example.com")]
    [DataRow("user+tag@example.co.uk")]
    [DataRow("firstname.lastname@domain.com")]
    [DataRow("email@subdomain.example.com")]
    [DataRow("123@example.com")]
    [DataRow("email@123.123.123.123")]
    [DataRow("_test@example.com")]
    [DataRow("test_email@example.com")]
    [DataRow("test-email@example.com")]
    [DataRow("a@b.c")]
    public void Email_SetValidEmailFormat_UpdatesValue(string email)
    {
        // Arrange
        var viewModel = new RegisterViewModel(_mockAuthService.Object, _mockAcademicService.Object, _mockLogger.Object);

        // Act
        viewModel.Email = email;

        // Assert
        Assert.AreEqual(email, viewModel.Email);
    }

    /// <summary>
    /// Tests that setting the Email property to invalid or non-standard email formats still updates the value.
    /// The property does not perform email validation, so any string should be accepted.
    /// Input: Various invalid email format strings.
    /// Expected: Property value is updated to the provided string.
    /// </summary>
    /// <param name="invalidEmail">The invalid email string to set.</param>
    [TestMethod]
    [DataRow("notanemail")]
    [DataRow("missing@domain")]
    [DataRow("@example.com")]
    [DataRow("user@")]
    [DataRow("user name@example.com")]
    [DataRow("user@@example.com")]
    [DataRow("user@domain@example.com")]
    public void Email_SetInvalidEmailFormat_UpdatesValue(string invalidEmail)
    {
        // Arrange
        var viewModel = new RegisterViewModel(_mockAuthService.Object, _mockAcademicService.Object, _mockLogger.Object);

        // Act
        viewModel.Email = invalidEmail;

        // Assert
        Assert.AreEqual(invalidEmail, viewModel.Email);
    }

    /// <summary>
    /// Tests that setting the Email property to a new value raises the PropertyChanged event with correct property name.
    /// Input: A new email value different from current value.
    /// Expected: PropertyChanged event is raised with PropertyName = "Email".
    /// </summary>
    [TestMethod]
    public void Email_SetNewValue_RaisesPropertyChangedEvent()
    {
        // Arrange
        var viewModel = new RegisterViewModel(_mockAuthService.Object, _mockAcademicService.Object, _mockLogger.Object);
        var propertyChangedRaised = false;
        string? raisedPropertyName = null;

        viewModel.PropertyChanged += (sender, args) =>
        {
            propertyChangedRaised = true;
            raisedPropertyName = args.PropertyName;
        };

        // Act
        viewModel.Email = "test@example.com";

        // Assert
        Assert.IsTrue(propertyChangedRaised);
        Assert.AreEqual("Email", raisedPropertyName);
    }

    /// <summary>
    /// Tests that setting the Email property to the same value does not raise the PropertyChanged event.
    /// This verifies that SetProperty correctly checks for value equality before raising the event.
    /// Input: Setting the same email value twice.
    /// Expected: PropertyChanged event is raised only on the first set, not the second.
    /// </summary>
    [TestMethod]
    public void Email_SetSameValue_DoesNotRaisePropertyChangedEvent()
    {
        // Arrange
        var viewModel = new RegisterViewModel(_mockAuthService.Object, _mockAcademicService.Object, _mockLogger.Object);
        var email = "test@example.com";
        viewModel.Email = email;

        var propertyChangedCount = 0;
        viewModel.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == "Email")
            {
                propertyChangedCount++;
            }
        };

        // Act
        viewModel.Email = email;

        // Assert
        Assert.AreEqual(0, propertyChangedCount);
    }

    /// <summary>
    /// Tests that setting the Email property multiple times with different values correctly updates the property each time and raises PropertyChanged.
    /// Input: Multiple different email values set sequentially.
    /// Expected: Property value is updated each time and PropertyChanged is raised for each change.
    /// </summary>
    [TestMethod]
    public void Email_SetMultipleDifferentValues_UpdatesAndRaisesPropertyChangedEachTime()
    {
        // Arrange
        var viewModel = new RegisterViewModel(_mockAuthService.Object, _mockAcademicService.Object, _mockLogger.Object);
        var propertyChangedCount = 0;

        viewModel.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == "Email")
            {
                propertyChangedCount++;
            }
        };

        // Act
        viewModel.Email = "first@example.com";
        viewModel.Email = "second@example.com";
        viewModel.Email = "third@example.com";

        // Assert
        Assert.AreEqual("third@example.com", viewModel.Email);
        Assert.AreEqual(3, propertyChangedCount);
    }

    /// <summary>
    /// Tests that setting Email from a non-empty value back to empty string raises PropertyChanged.
    /// Input: First set to non-empty email, then set to empty string.
    /// Expected: PropertyChanged is raised for both changes.
    /// </summary>
    [TestMethod]
    public void Email_SetToEmptyFromNonEmpty_RaisesPropertyChanged()
    {
        // Arrange
        var viewModel = new RegisterViewModel(_mockAuthService.Object, _mockAcademicService.Object, _mockLogger.Object);
        viewModel.Email = "test@example.com";

        var propertyChangedCount = 0;
        viewModel.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == "Email")
            {
                propertyChangedCount++;
            }
        };

        // Act
        viewModel.Email = string.Empty;

        // Assert
        Assert.AreEqual(string.Empty, viewModel.Email);
        Assert.AreEqual(1, propertyChangedCount);
    }

    /// <summary>
    /// Tests that setting the Email property with control characters is handled correctly.
    /// Input: Strings containing various control characters.
    /// Expected: Property value is updated to the string with control characters.
    /// </summary>
    [TestMethod]
    [DataRow("email\u0000@example.com")]
    [DataRow("email\u0001@example.com")]
    [DataRow("email\u0002@example.com")]
    [DataRow("email\u001F@example.com")]
    [DataRow("email\u007F@example.com")]
    public void Email_SetValueWithControlCharacters_UpdatesValue(string emailWithControlChars)
    {
        // Arrange
        var viewModel = new RegisterViewModel(_mockAuthService.Object, _mockAcademicService.Object, _mockLogger.Object);

        // Act
        viewModel.Email = emailWithControlChars;

        // Assert
        Assert.AreEqual(emailWithControlChars, viewModel.Email);
    }

    /// <summary>
    /// Tests that setting the Email property with Unicode characters from various languages is handled correctly.
    /// Input: Email strings with Unicode characters.
    /// Expected: Property value is updated to the Unicode string.
    /// </summary>
    [TestMethod]
    [DataRow("用户@例え.jp")]
    [DataRow("пользователь@тест.ru")]
    [DataRow("उपयोगकर्ता@example.com")]
    [DataRow("사용자@example.com")]
    [DataRow("مستخدم@example.com")]
    public void Email_SetValueWithUnicodeCharacters_UpdatesValue(string emailWithUnicode)
    {
        // Arrange
        var viewModel = new RegisterViewModel(_mockAuthService.Object, _mockAcademicService.Object, _mockLogger.Object);

        // Act
        viewModel.Email = emailWithUnicode;

        // Assert
        Assert.AreEqual(emailWithUnicode, viewModel.Email);
    }

    /// <summary>
    /// Tests that the Email property correctly handles edge case boundary values for string length.
    /// Input: Single character string.
    /// Expected: Property value is updated to the single character.
    /// </summary>
    [TestMethod]
    public void Email_SetSingleCharacter_UpdatesValue()
    {
        // Arrange
        var viewModel = new RegisterViewModel(_mockAuthService.Object, _mockAcademicService.Object, _mockLogger.Object);

        // Act
        viewModel.Email = "a";

        // Assert
        Assert.AreEqual("a", viewModel.Email);
    }

    /// <summary>
    /// Tests that setting SelectedDepartment to a non-null value from null updates the property,
    /// clears Programs collection, sets SelectedProgram to null, and raises PropertyChanged event.
    /// Input: Valid LookupItem instance when SelectedDepartment is null.
    /// Expected: Property is updated, Programs cleared, SelectedProgram set to null, and PropertyChanged raised.
    /// </summary>
    [TestMethod]
    public void SelectedDepartment_SetNonNullFromNull_UpdatesPropertyAndClearsDependentState()
    {
        // Arrange
        var mockAuthService = new Mock<IAuthService>();
        var mockAcademicService = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<RegisterViewModel>>();
        var viewModel = new RegisterViewModel(mockAuthService.Object, mockAcademicService.Object, mockLogger.Object);

        var department = new LookupItem { Id = "dept123", Name = "Computer Science" };
        viewModel.Programs.Add(new LookupItem { Id = "prog1", Name = "Program 1" });
        viewModel.SelectedProgram = new LookupItem { Id = "prog1", Name = "Program 1" };

        var propertyChangedRaised = false;
        viewModel.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == nameof(viewModel.SelectedDepartment))
            {
                propertyChangedRaised = true;
            }
        };

        // Act
        viewModel.SelectedDepartment = department;

        // Assert
        Assert.AreEqual(department, viewModel.SelectedDepartment);
        Assert.IsNull(viewModel.SelectedProgram);
        Assert.AreEqual(0, viewModel.Programs.Count);
        Assert.IsTrue(propertyChangedRaised);
    }

    /// <summary>
    /// Tests that setting SelectedDepartment to null from a non-null value updates the property,
    /// clears Programs collection, sets SelectedProgram to null, and raises PropertyChanged event.
    /// Input: Null value when SelectedDepartment has a non-null value.
    /// Expected: Property set to null, Programs cleared, SelectedProgram set to null, and PropertyChanged raised.
    /// </summary>
    [TestMethod]
    public void SelectedDepartment_SetNullFromNonNull_UpdatesPropertyAndClearsDependentState()
    {
        // Arrange
        var mockAuthService = new Mock<IAuthService>();
        var mockAcademicService = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<RegisterViewModel>>();
        var viewModel = new RegisterViewModel(mockAuthService.Object, mockAcademicService.Object, mockLogger.Object);

        var department = new LookupItem { Id = "dept123", Name = "Computer Science" };
        viewModel.SelectedDepartment = department;
        viewModel.Programs.Add(new LookupItem { Id = "prog1", Name = "Program 1" });
        viewModel.SelectedProgram = new LookupItem { Id = "prog1", Name = "Program 1" };

        var propertyChangedRaised = false;
        viewModel.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == nameof(viewModel.SelectedDepartment))
            {
                propertyChangedRaised = true;
            }
        };

        // Act
        viewModel.SelectedDepartment = null;

        // Assert
        Assert.IsNull(viewModel.SelectedDepartment);
        Assert.IsNull(viewModel.SelectedProgram);
        Assert.AreEqual(0, viewModel.Programs.Count);
        Assert.IsTrue(propertyChangedRaised);
    }

    /// <summary>
    /// Tests that setting SelectedDepartment to a different non-null value updates the property,
    /// clears Programs collection, sets SelectedProgram to null, and raises PropertyChanged event.
    /// Input: Different LookupItem instance when SelectedDepartment already has a value.
    /// Expected: Property updated, Programs cleared, SelectedProgram set to null, and PropertyChanged raised.
    /// </summary>
    [TestMethod]
    public void SelectedDepartment_SetDifferentNonNullValue_UpdatesPropertyAndClearsDependentState()
    {
        // Arrange
        var mockAuthService = new Mock<IAuthService>();
        var mockAcademicService = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<RegisterViewModel>>();
        var viewModel = new RegisterViewModel(mockAuthService.Object, mockAcademicService.Object, mockLogger.Object);

        var department1 = new LookupItem { Id = "dept123", Name = "Computer Science" };
        var department2 = new LookupItem { Id = "dept456", Name = "Engineering" };
        viewModel.SelectedDepartment = department1;
        viewModel.Programs.Add(new LookupItem { Id = "prog1", Name = "Program 1" });
        viewModel.SelectedProgram = new LookupItem { Id = "prog1", Name = "Program 1" };

        var propertyChangedRaised = false;
        viewModel.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == nameof(viewModel.SelectedDepartment))
            {
                propertyChangedRaised = true;
            }
        };

        // Act
        viewModel.SelectedDepartment = department2;

        // Assert
        Assert.AreEqual(department2, viewModel.SelectedDepartment);
        Assert.IsNull(viewModel.SelectedProgram);
        Assert.AreEqual(0, viewModel.Programs.Count);
        Assert.IsTrue(propertyChangedRaised);
    }

    /// <summary>
    /// Tests that setting SelectedDepartment with a LookupItem having empty Id string
    /// still triggers the cascading update behavior.
    /// Input: LookupItem with empty string Id.
    /// Expected: Property updated, Programs cleared, SelectedProgram set to null.
    /// </summary>
    [TestMethod]
    public void SelectedDepartment_SetDepartmentWithEmptyId_ClearsProgramsAndTriggersUpdate()
    {
        // Arrange
        var mockAuthService = new Mock<IAuthService>();
        var mockAcademicService = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<RegisterViewModel>>();
        var viewModel = new RegisterViewModel(mockAuthService.Object, mockAcademicService.Object, mockLogger.Object);

        var departmentWithEmptyId = new LookupItem { Id = string.Empty, Name = "Department" };
        viewModel.Programs.Add(new LookupItem { Id = "prog1", Name = "Program 1" });
        viewModel.SelectedProgram = new LookupItem { Id = "prog1", Name = "Program 1" };

        // Act
        viewModel.SelectedDepartment = departmentWithEmptyId;

        // Assert
        Assert.AreEqual(departmentWithEmptyId, viewModel.SelectedDepartment);
        Assert.IsNull(viewModel.SelectedProgram);
        Assert.AreEqual(0, viewModel.Programs.Count);
    }

    /// <summary>
    /// Tests that setting SelectedDepartment with a LookupItem containing whitespace-only Id
    /// handles the scenario correctly.
    /// Input: LookupItem with whitespace-only Id.
    /// Expected: Property updated, Programs cleared, SelectedProgram set to null.
    /// </summary>
    [TestMethod]
    [DataRow("   ")]
    [DataRow("\t")]
    [DataRow("\n")]
    [DataRow(" \t\n ")]
    public void SelectedDepartment_SetDepartmentWithWhitespaceId_HandlesCorrectly(string whitespaceId)
    {
        // Arrange
        var mockAuthService = new Mock<IAuthService>();
        var mockAcademicService = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<RegisterViewModel>>();
        var viewModel = new RegisterViewModel(mockAuthService.Object, mockAcademicService.Object, mockLogger.Object);

        var departmentWithWhitespaceId = new LookupItem { Id = whitespaceId, Name = "Department" };
        viewModel.Programs.Add(new LookupItem { Id = "prog1", Name = "Program 1" });
        viewModel.SelectedProgram = new LookupItem { Id = "prog1", Name = "Program 1" };

        // Act
        viewModel.SelectedDepartment = departmentWithWhitespaceId;

        // Assert
        Assert.AreEqual(departmentWithWhitespaceId, viewModel.SelectedDepartment);
        Assert.IsNull(viewModel.SelectedProgram);
        Assert.AreEqual(0, viewModel.Programs.Count);
    }

    /// <summary>
    /// Tests that SelectedDepartment property returns null as its initial default value.
    /// Input: Newly instantiated RegisterViewModel.
    /// Expected: SelectedDepartment returns null.
    /// </summary>
    [TestMethod]
    public void SelectedDepartment_InitialValue_ReturnsNull()
    {
        // Arrange
        var mockAuthService = new Mock<IAuthService>();
        var mockAcademicService = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<RegisterViewModel>>();

        // Act
        var viewModel = new RegisterViewModel(mockAuthService.Object, mockAcademicService.Object, mockLogger.Object);

        // Assert
        Assert.IsNull(viewModel.SelectedDepartment);
    }

    /// <summary>
    /// Tests that setting SelectedDepartment raises PropertyChanged for dependent properties
    /// SelectedProgram when value changes.
    /// Input: Valid LookupItem instance.
    /// Expected: PropertyChanged raised for SelectedProgram.
    /// </summary>
    [TestMethod]
    public void SelectedDepartment_SetValue_RaisesPropertyChangedForDependentProperties()
    {
        // Arrange
        var mockAuthService = new Mock<IAuthService>();
        var mockAcademicService = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<RegisterViewModel>>();
        var viewModel = new RegisterViewModel(mockAuthService.Object, mockAcademicService.Object, mockLogger.Object);

        var department = new LookupItem { Id = "dept123", Name = "Computer Science" };
        viewModel.SelectedProgram = new LookupItem { Id = "prog1", Name = "Program 1" };

        var selectedProgramChangedRaised = false;
        viewModel.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == nameof(viewModel.SelectedProgram))
            {
                selectedProgramChangedRaised = true;
            }
        };

        // Act
        viewModel.SelectedDepartment = department;

        // Assert
        Assert.IsTrue(selectedProgramChangedRaised);
    }

    /// <summary>
    /// Tests that setting SelectedDepartment with very long Id string handles correctly.
    /// Input: LookupItem with very long Id (10000 characters).
    /// Expected: Property updated, Programs cleared, SelectedProgram set to null.
    /// </summary>
    [TestMethod]
    public void SelectedDepartment_SetDepartmentWithVeryLongId_HandlesCorrectly()
    {
        // Arrange
        var mockAuthService = new Mock<IAuthService>();
        var mockAcademicService = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<RegisterViewModel>>();
        var viewModel = new RegisterViewModel(mockAuthService.Object, mockAcademicService.Object, mockLogger.Object);

        var veryLongId = new string('a', 10000);
        var departmentWithVeryLongId = new LookupItem { Id = veryLongId, Name = "Department" };
        viewModel.Programs.Add(new LookupItem { Id = "prog1", Name = "Program 1" });

        // Act
        viewModel.SelectedDepartment = departmentWithVeryLongId;

        // Assert
        Assert.AreEqual(departmentWithVeryLongId, viewModel.SelectedDepartment);
        Assert.IsNull(viewModel.SelectedProgram);
        Assert.AreEqual(0, viewModel.Programs.Count);
    }

    /// <summary>
    /// Tests that setting SelectedDepartment with LookupItem containing Unicode characters in Id
    /// handles correctly.
    /// Input: LookupItem with Unicode characters in Id.
    /// Expected: Property updated, Programs cleared, SelectedProgram set to null.
    /// </summary>
    [TestMethod]
    [DataRow("部門123")]
    [DataRow("отдел456")]
    [DataRow("قسم789")]
    [DataRow("😀dept")]
    public void SelectedDepartment_SetDepartmentWithUnicodeId_HandlesCorrectly(string unicodeId)
    {
        // Arrange
        var mockAuthService = new Mock<IAuthService>();
        var mockAcademicService = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<RegisterViewModel>>();
        var viewModel = new RegisterViewModel(mockAuthService.Object, mockAcademicService.Object, mockLogger.Object);

        var departmentWithUnicodeId = new LookupItem { Id = unicodeId, Name = "Department" };
        viewModel.Programs.Add(new LookupItem { Id = "prog1", Name = "Program 1" });

        // Act
        viewModel.SelectedDepartment = departmentWithUnicodeId;

        // Assert
        Assert.AreEqual(departmentWithUnicodeId, viewModel.SelectedDepartment);
        Assert.IsNull(viewModel.SelectedProgram);
        Assert.AreEqual(0, viewModel.Programs.Count);
    }

    /// <summary>
    /// Tests that setting SelectedDepartment multiple times with different values updates correctly each time.
    /// Input: Multiple different LookupItem instances set sequentially.
    /// Expected: Property updated correctly for each set, Programs cleared each time.
    /// </summary>
    [TestMethod]
    public void SelectedDepartment_SetMultipleDifferentValues_UpdatesCorrectlyEachTime()
    {
        // Arrange
        var mockAuthService = new Mock<IAuthService>();
        var mockAcademicService = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<RegisterViewModel>>();
        var viewModel = new RegisterViewModel(mockAuthService.Object, mockAcademicService.Object, mockLogger.Object);

        var department1 = new LookupItem { Id = "dept1", Name = "Department 1" };
        var department2 = new LookupItem { Id = "dept2", Name = "Department 2" };
        var department3 = new LookupItem { Id = "dept3", Name = "Department 3" };

        // Act & Assert
        viewModel.SelectedDepartment = department1;
        Assert.AreEqual(department1, viewModel.SelectedDepartment);

        viewModel.Programs.Add(new LookupItem { Id = "prog1", Name = "Program 1" });
        viewModel.SelectedDepartment = department2;
        Assert.AreEqual(department2, viewModel.SelectedDepartment);
        Assert.AreEqual(0, viewModel.Programs.Count);

        viewModel.Programs.Add(new LookupItem { Id = "prog2", Name = "Program 2" });
        viewModel.SelectedDepartment = department3;
        Assert.AreEqual(department3, viewModel.SelectedDepartment);
        Assert.AreEqual(0, viewModel.Programs.Count);
    }

    /// <summary>
    /// Tests that SelectedSemester handles LookupItem with very long strings in Id and Name properties.
    /// Input: LookupItem with Id and Name containing 5000 characters each.
    /// Expected: Property stores and retrieves the LookupItem correctly and raises PropertyChanged event.
    /// </summary>
    [TestMethod]
    public void SelectedSemester_SetLookupItemWithVeryLongStrings_StoresAndRetrievesCorrectly()
    {
        // Arrange
        var mockAuthService = new Mock<IAuthService>();
        var mockAcademicService = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<RegisterViewModel>>();
        var viewModel = new RegisterViewModel(mockAuthService.Object, mockAcademicService.Object, mockLogger.Object);
        var veryLongString = new string('A', 5000);
        var lookupItem = new LookupItem { Id = veryLongString, Name = veryLongString };
        var propertyChangedRaised = false;
        viewModel.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == nameof(viewModel.SelectedSemester))
                propertyChangedRaised = true;
        };

        // Act
        viewModel.SelectedSemester = lookupItem;

        // Assert
        Assert.IsNotNull(viewModel.SelectedSemester);
        Assert.AreEqual(veryLongString, viewModel.SelectedSemester.Id);
        Assert.AreEqual(veryLongString, viewModel.SelectedSemester.Name);
        Assert.IsTrue(propertyChangedRaised);
    }

    /// <summary>
    /// Tests that SelectedSemester handles LookupItem with whitespace-only strings in Id and Name.
    /// Input: LookupItem with various whitespace patterns (spaces, tabs, newlines).
    /// Expected: Property stores the whitespace strings correctly.
    /// </summary>
    /// <param name="whitespaceId">Whitespace string for Id property.</param>
    /// <param name="whitespaceName">Whitespace string for Name property.</param>
    [TestMethod]
    [DataRow("   ", "   ")]
    [DataRow("\t", "\t")]
    [DataRow("\n", "\n")]
    [DataRow("\r\n", "\r\n")]
    [DataRow(" \t\n ", " \t\n ")]
    [DataRow("     ", "\t\t\t")]
    public void SelectedSemester_SetLookupItemWithWhitespaceStrings_StoresCorrectly(string whitespaceId, string whitespaceName)
    {
        // Arrange
        var mockAuthService = new Mock<IAuthService>();
        var mockAcademicService = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<RegisterViewModel>>();
        var viewModel = new RegisterViewModel(mockAuthService.Object, mockAcademicService.Object, mockLogger.Object);
        var lookupItem = new LookupItem { Id = whitespaceId, Name = whitespaceName };

        // Act
        viewModel.SelectedSemester = lookupItem;

        // Assert
        Assert.IsNotNull(viewModel.SelectedSemester);
        Assert.AreEqual(whitespaceId, viewModel.SelectedSemester.Id);
        Assert.AreEqual(whitespaceName, viewModel.SelectedSemester.Name);
    }

    /// <summary>
    /// Tests that SelectedSemester handles LookupItem with special characters in Id and Name.
    /// Input: LookupItem with various special characters.
    /// Expected: Property stores and retrieves the special characters correctly.
    /// </summary>
    /// <param name="specialId">String with special characters for Id.</param>
    /// <param name="specialName">String with special characters for Name.</param>
    [TestMethod]
    [DataRow("!@#$%^&*()", "Special!@#$")]
    [DataRow("<>?:\"|{}[]", "Brackets<>[]{}")]
    [DataRow("semester-2024", "Spring-2024")]
    [DataRow("semester_2024", "Fall_2024")]
    [DataRow("semester.2024", "Winter.2024")]
    [DataRow("semester/2024", "Summer/2024")]
    [DataRow("semester\\2024", "Test\\Semester")]
    public void SelectedSemester_SetLookupItemWithSpecialCharacters_StoresCorrectly(string specialId, string specialName)
    {
        // Arrange
        var mockAuthService = new Mock<IAuthService>();
        var mockAcademicService = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<RegisterViewModel>>();
        var viewModel = new RegisterViewModel(mockAuthService.Object, mockAcademicService.Object, mockLogger.Object);
        var lookupItem = new LookupItem { Id = specialId, Name = specialName };

        // Act
        viewModel.SelectedSemester = lookupItem;

        // Assert
        Assert.IsNotNull(viewModel.SelectedSemester);
        Assert.AreEqual(specialId, viewModel.SelectedSemester.Id);
        Assert.AreEqual(specialName, viewModel.SelectedSemester.Name);
    }

    /// <summary>
    /// Tests that SelectedSemester handles LookupItem with Unicode characters including international text and emojis.
    /// Input: LookupItem with Unicode characters.
    /// Expected: Property stores and retrieves Unicode characters correctly.
    /// </summary>
    /// <param name="unicodeId">Unicode string for Id.</param>
    /// <param name="unicodeName">Unicode string for Name.</param>
    [TestMethod]
    [DataRow("学期1", "第一学期")]
    [DataRow("Семестр1", "Первый семестр")]
    [DataRow("学期😀", "Semester😁")]
    [DataRow("Τρίμηνο", "Πρώτο τρίμηνο")]
    [DataRow("सेमेस्टर", "पहला सेमेस्टर")]
    [DataRow("🎓📚", "🏫📖")]
    public void SelectedSemester_SetLookupItemWithUnicodeCharacters_StoresCorrectly(string unicodeId, string unicodeName)
    {
        // Arrange
        var mockAuthService = new Mock<IAuthService>();
        var mockAcademicService = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<RegisterViewModel>>();
        var viewModel = new RegisterViewModel(mockAuthService.Object, mockAcademicService.Object, mockLogger.Object);
        var lookupItem = new LookupItem { Id = unicodeId, Name = unicodeName };

        // Act
        viewModel.SelectedSemester = lookupItem;

        // Assert
        Assert.IsNotNull(viewModel.SelectedSemester);
        Assert.AreEqual(unicodeId, viewModel.SelectedSemester.Id);
        Assert.AreEqual(unicodeName, viewModel.SelectedSemester.Name);
    }

    /// <summary>
    /// Tests that SelectedSemester handles LookupItem with control characters in Id and Name.
    /// Input: LookupItem with control characters including null character, bell, etc.
    /// Expected: Property stores control characters correctly.
    /// </summary>
    [TestMethod]
    public void SelectedSemester_SetLookupItemWithControlCharacters_StoresCorrectly()
    {
        // Arrange
        var mockAuthService = new Mock<IAuthService>();
        var mockAcademicService = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<RegisterViewModel>>();
        var viewModel = new RegisterViewModel(mockAuthService.Object, mockAcademicService.Object, mockLogger.Object);
        var idWithControlChars = "Semester\u0000\u0001\u0002";
        var nameWithControlChars = "Name\u0007\u0008\u0009";
        var lookupItem = new LookupItem { Id = idWithControlChars, Name = nameWithControlChars };

        // Act
        viewModel.SelectedSemester = lookupItem;

        // Assert
        Assert.IsNotNull(viewModel.SelectedSemester);
        Assert.AreEqual(idWithControlChars, viewModel.SelectedSemester.Id);
        Assert.AreEqual(nameWithControlChars, viewModel.SelectedSemester.Name);
    }

    /// <summary>
    /// Tests that SelectedSemester handles LookupItem with mixed content (whitespace, special chars, unicode).
    /// Input: LookupItem with complex string combinations.
    /// Expected: Property stores mixed content correctly.
    /// </summary>
    [TestMethod]
    public void SelectedSemester_SetLookupItemWithMixedContent_StoresCorrectly()
    {
        // Arrange
        var mockAuthService = new Mock<IAuthService>();
        var mockAcademicService = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<RegisterViewModel>>();
        var viewModel = new RegisterViewModel(mockAuthService.Object, mockAcademicService.Object, mockLogger.Object);
        var mixedId = "  Semester-2024 学期 😀  ";
        var mixedName = "\tSpring/Fall 学期\n@#$";
        var lookupItem = new LookupItem { Id = mixedId, Name = mixedName };

        // Act
        viewModel.SelectedSemester = lookupItem;

        // Assert
        Assert.IsNotNull(viewModel.SelectedSemester);
        Assert.AreEqual(mixedId, viewModel.SelectedSemester.Id);
        Assert.AreEqual(mixedName, viewModel.SelectedSemester.Name);
    }

    /// <summary>
    /// Tests that setting SelectedSemester to different LookupItem instances with identical string values
    /// raises PropertyChanged event due to reference inequality.
    /// Input: Two different LookupItem instances with same Id and Name values.
    /// Expected: PropertyChanged event is raised for each set operation.
    /// </summary>
    [TestMethod]
    public void SelectedSemester_SetDifferentInstancesWithIdenticalValues_RaisesPropertyChangedForEach()
    {
        // Arrange
        var mockAuthService = new Mock<IAuthService>();
        var mockAcademicService = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<RegisterViewModel>>();
        var viewModel = new RegisterViewModel(mockAuthService.Object, mockAcademicService.Object, mockLogger.Object);
        var lookupItem1 = new LookupItem { Id = "sem1", Name = "Semester 1" };
        var lookupItem2 = new LookupItem { Id = "sem1", Name = "Semester 1" };
        var eventCount = 0;
        viewModel.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == nameof(viewModel.SelectedSemester))
                eventCount++;
        };

        // Act
        viewModel.SelectedSemester = lookupItem1;
        viewModel.SelectedSemester = lookupItem2;

        // Assert
        Assert.AreEqual(2, eventCount);
        Assert.IsNotNull(viewModel.SelectedSemester);
        Assert.AreEqual("sem1", viewModel.SelectedSemester.Id);
    }

    /// <summary>
    /// Tests that SelectedSemester handles rapid alternating changes between null and non-null values.
    /// Input: Alternating null and LookupItem values multiple times.
    /// Expected: Property updates correctly each time and PropertyChanged is raised for each change.
    /// </summary>
    [TestMethod]
    public void SelectedSemester_RapidAlternatingNullAndNonNull_UpdatesCorrectlyEachTime()
    {
        // Arrange
        var mockAuthService = new Mock<IAuthService>();
        var mockAcademicService = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<RegisterViewModel>>();
        var viewModel = new RegisterViewModel(mockAuthService.Object, mockAcademicService.Object, mockLogger.Object);
        var lookupItem = new LookupItem { Id = "sem1", Name = "Semester 1" };
        var eventCount = 0;
        viewModel.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == nameof(viewModel.SelectedSemester))
                eventCount++;
        };

        // Act & Assert
        viewModel.SelectedSemester = lookupItem;
        Assert.IsNotNull(viewModel.SelectedSemester);

        viewModel.SelectedSemester = null;
        Assert.IsNull(viewModel.SelectedSemester);

        viewModel.SelectedSemester = lookupItem;
        Assert.IsNotNull(viewModel.SelectedSemester);

        viewModel.SelectedSemester = null;
        Assert.IsNull(viewModel.SelectedSemester);

        Assert.AreEqual(4, eventCount);
    }

    /// <summary>
    /// Tests that SelectedSemester correctly handles numeric-only strings in LookupItem properties.
    /// Input: LookupItem with numeric strings, including edge cases like "0", negative numbers as strings.
    /// Expected: Property stores numeric strings correctly.
    /// </summary>
    /// <param name="numericId">Numeric string for Id.</param>
    /// <param name="numericName">Numeric string for Name.</param>
    [TestMethod]
    [DataRow("0", "0")]
    [DataRow("1", "2")]
    [DataRow("-1", "-100")]
    [DataRow("2147483647", "2147483647")]
    [DataRow("-2147483648", "-2147483648")]
    [DataRow("999999999999999999", "123456789012345678")]
    public void SelectedSemester_SetLookupItemWithNumericStrings_StoresCorrectly(string numericId, string numericName)
    {
        // Arrange
        var mockAuthService = new Mock<IAuthService>();
        var mockAcademicService = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<RegisterViewModel>>();
        var viewModel = new RegisterViewModel(mockAuthService.Object, mockAcademicService.Object, mockLogger.Object);
        var lookupItem = new LookupItem { Id = numericId, Name = numericName };

        // Act
        viewModel.SelectedSemester = lookupItem;

        // Assert
        Assert.IsNotNull(viewModel.SelectedSemester);
        Assert.AreEqual(numericId, viewModel.SelectedSemester.Id);
        Assert.AreEqual(numericName, viewModel.SelectedSemester.Name);
    }

    /// <summary>
    /// Tests that the Gender property returns an empty string as its initial value.
    /// Input: None (testing initial state).
    /// Expected: Gender property returns empty string.
    /// </summary>
    [TestMethod]
    public void Gender_InitialValue_ReturnsEmptyString()
    {
        // Arrange & Act
        var viewModel = new RegisterViewModel(_mockAuthService.Object, _mockAcademicService.Object, _mockLogger.Object);

        // Assert
        Assert.AreEqual(string.Empty, viewModel.Gender);
    }

    /// <summary>
    /// Tests that setting the Gender property with Unicode characters is handled correctly.
    /// Input: Strings containing Unicode characters including emojis and various language scripts.
    /// Expected: Property value is updated to the Unicode string.
    /// </summary>
    [TestMethod]
    [DataRow("性別")]
    [DataRow("جنس")]
    [DataRow("género")]
    [DataRow("😀")]
    [DataRow("♂♀")]
    public void Gender_SetValueWithUnicodeCharacters_UpdatesProperty(string unicodeValue)
    {
        // Arrange
        var viewModel = new RegisterViewModel(_mockAuthService.Object, _mockAcademicService.Object, _mockLogger.Object);

        // Act
        viewModel.Gender = unicodeValue;

        // Assert
        Assert.AreEqual(unicodeValue, viewModel.Gender);
    }

    /// <summary>
    /// Tests that setting the Gender property to null at runtime updates the property.
    /// Input: Null value (suppressing nullable warning for runtime testing).
    /// Expected: Property is set to null without throwing an exception.
    /// </summary>
    [TestMethod]
    public void Gender_SetNull_UpdatesProperty()
    {
        // Arrange
        var viewModel = new RegisterViewModel(_mockAuthService.Object, _mockAcademicService.Object, _mockLogger.Object);

        // Act
        viewModel.Gender = null!;

        // Assert
        Assert.IsNull(viewModel.Gender);
    }

    /// <summary>
    /// Tests that setting Gender property to a very long string handles the value correctly.
    /// Input: String with 50000 characters.
    /// Expected: Property is updated to the very long string.
    /// </summary>
    [TestMethod]
    public void Gender_SetVeryLongString_UpdatesProperty()
    {
        // Arrange
        var viewModel = new RegisterViewModel(_mockAuthService.Object, _mockAcademicService.Object, _mockLogger.Object);
        var veryLongString = new string('A', 50000);

        // Act
        viewModel.Gender = veryLongString;

        // Assert
        Assert.AreEqual(veryLongString, viewModel.Gender);
    }

    /// <summary>
    /// Tests that setting Gender property raises PropertyChanged event multiple times for multiple different values.
    /// Input: Three different values set sequentially.
    /// Expected: PropertyChanged event is raised three times, once for each different value.
    /// </summary>
    [TestMethod]
    public void Gender_SetMultipleDifferentValues_RaisesPropertyChangedForEachChange()
    {
        // Arrange
        var viewModel = new RegisterViewModel(_mockAuthService.Object, _mockAcademicService.Object, _mockLogger.Object);
        int eventRaisedCount = 0;
        viewModel.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == "Gender")
                eventRaisedCount++;
        };

        // Act
        viewModel.Gender = "Male";
        viewModel.Gender = "Female";
        viewModel.Gender = "Other";

        // Assert
        Assert.AreEqual(3, eventRaisedCount);
    }

    /// <summary>
    /// Tests that setting Gender to empty string when already empty does not raise PropertyChanged event.
    /// Input: Empty string set when Gender is already empty string (initial state).
    /// Expected: PropertyChanged event is not raised.
    /// </summary>
    [TestMethod]
    public void Gender_SetEmptyWhenAlreadyEmpty_DoesNotRaisePropertyChanged()
    {
        // Arrange
        var viewModel = new RegisterViewModel(_mockAuthService.Object, _mockAcademicService.Object, _mockLogger.Object);
        int eventRaisedCount = 0;
        viewModel.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == "Gender")
                eventRaisedCount++;
        };

        // Act
        viewModel.Gender = string.Empty;

        // Assert
        Assert.AreEqual(0, eventRaisedCount);
    }

    /// <summary>
    /// Tests that setting Gender from non-empty value back to empty string updates correctly and raises PropertyChanged.
    /// Input: First set to "Male", then set to empty string.
    /// Expected: Property is updated to empty string and PropertyChanged is raised.
    /// </summary>
    [TestMethod]
    public void Gender_SetFromNonEmptyToEmpty_UpdatesPropertyAndRaisesEvent()
    {
        // Arrange
        var viewModel = new RegisterViewModel(_mockAuthService.Object, _mockAcademicService.Object, _mockLogger.Object);
        viewModel.Gender = "Male";

        bool eventRaised = false;
        viewModel.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == "Gender")
                eventRaised = true;
        };

        // Act
        viewModel.Gender = string.Empty;

        // Assert
        Assert.AreEqual(string.Empty, viewModel.Gender);
        Assert.IsTrue(eventRaised);
    }

    /// <summary>
    /// Tests that setting SelectedUniversity to a non-null value from null clears dependent collections,
    /// sets SelectedFaculty to null, and triggers LoadFacultiesAsync with the correct university Id.
    /// Input: Valid LookupItem with Id="uni1" and Name="University 1".
    /// Expected: Faculties, Departments, and Programs collections are cleared, SelectedFaculty is null,
    /// and LoadFacultiesAsync is called asynchronously.
    /// </summary>
    [TestMethod]
    public void SelectedUniversity_SetNonNullFromNull_ClearsCollectionsAndTriggersLoad()
    {
        // Arrange
        var mockAuthService = new Mock<IAuthService>();
        var mockAcademicService = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<RegisterViewModel>>();
        mockAcademicService.Setup(s => s.GetFacultiesAsync(It.IsAny<string>()))
            .ReturnsAsync(new List<LookupItem>());
        var viewModel = new RegisterViewModel(mockAuthService.Object, mockAcademicService.Object, mockLogger.Object);

        viewModel.Faculties.Add(new LookupItem { Id = "f1", Name = "Faculty 1" });
        viewModel.Departments.Add(new LookupItem { Id = "d1", Name = "Department 1" });
        viewModel.Programs.Add(new LookupItem { Id = "p1", Name = "Program 1" });

        var university = new LookupItem { Id = "uni1", Name = "University 1" };

        // Act
        viewModel.SelectedUniversity = university;
        Task.Delay(100).Wait();

        // Assert
        Assert.AreEqual(university, viewModel.SelectedUniversity);
        Assert.IsNull(viewModel.SelectedFaculty);
        Assert.AreEqual(0, viewModel.Faculties.Count);
        Assert.AreEqual(0, viewModel.Departments.Count);
        Assert.AreEqual(0, viewModel.Programs.Count);
    }

    /// <summary>
    /// Tests that setting SelectedUniversity to null from a non-null value clears dependent collections,
    /// sets SelectedFaculty to null, and does NOT trigger LoadFacultiesAsync.
    /// Input: Setting to null after having a valid university selected.
    /// Expected: Collections are cleared, SelectedFaculty is null, LoadFacultiesAsync is not called.
    /// </summary>
    [TestMethod]
    public void SelectedUniversity_SetNullFromNonNull_ClearsCollectionsWithoutLoad()
    {
        // Arrange
        var mockAuthService = new Mock<IAuthService>();
        var mockAcademicService = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<RegisterViewModel>>();
        mockAcademicService.Setup(s => s.GetFacultiesAsync(It.IsAny<string>()))
            .ReturnsAsync(new List<LookupItem>());
        var viewModel = new RegisterViewModel(mockAuthService.Object, mockAcademicService.Object, mockLogger.Object);

        var university = new LookupItem { Id = "uni1", Name = "University 1" };
        viewModel.SelectedUniversity = university;
        Task.Delay(100).Wait();
        mockAcademicService.Invocations.Clear();

        viewModel.Faculties.Add(new LookupItem { Id = "f1", Name = "Faculty 1" });
        viewModel.Departments.Add(new LookupItem { Id = "d1", Name = "Department 1" });
        viewModel.Programs.Add(new LookupItem { Id = "p1", Name = "Program 1" });

        // Act
        viewModel.SelectedUniversity = null;
        Task.Delay(100).Wait();

        // Assert
        Assert.IsNull(viewModel.SelectedUniversity);
        Assert.IsNull(viewModel.SelectedFaculty);
        Assert.AreEqual(0, viewModel.Faculties.Count);
        Assert.AreEqual(0, viewModel.Departments.Count);
        Assert.AreEqual(0, viewModel.Programs.Count);
        mockAcademicService.Verify(s => s.GetFacultiesAsync(It.IsAny<string>()), Times.Never);
    }

    /// <summary>
    /// Tests that setting SelectedUniversity to a different non-null value clears collections
    /// and triggers LoadFacultiesAsync with the new university Id.
    /// Input: Two different LookupItem instances with different Ids.
    /// Expected: Collections are cleared, SelectedFaculty is reset, LoadFacultiesAsync is called with new Id.
    /// </summary>
    [TestMethod]
    public void SelectedUniversity_SetDifferentNonNullValue_ClearsCollectionsAndLoadsNew()
    {
        // Arrange
        var mockAuthService = new Mock<IAuthService>();
        var mockAcademicService = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<RegisterViewModel>>();
        mockAcademicService.Setup(s => s.GetFacultiesAsync(It.IsAny<string>()))
            .ReturnsAsync(new List<LookupItem>());
        var viewModel = new RegisterViewModel(mockAuthService.Object, mockAcademicService.Object, mockLogger.Object);

        var university1 = new LookupItem { Id = "uni1", Name = "University 1" };
        viewModel.SelectedUniversity = university1;
        Task.Delay(100).Wait();

        viewModel.Faculties.Add(new LookupItem { Id = "f1", Name = "Faculty 1" });
        viewModel.Departments.Add(new LookupItem { Id = "d1", Name = "Department 1" });
        viewModel.Programs.Add(new LookupItem { Id = "p1", Name = "Program 1" });

        var university2 = new LookupItem { Id = "uni2", Name = "University 2" };

        // Act
        viewModel.SelectedUniversity = university2;
        Task.Delay(100).Wait();

        // Assert
        Assert.AreEqual(university2, viewModel.SelectedUniversity);
        Assert.IsNull(viewModel.SelectedFaculty);
        Assert.AreEqual(0, viewModel.Faculties.Count);
        Assert.AreEqual(0, viewModel.Departments.Count);
        Assert.AreEqual(0, viewModel.Programs.Count);
    }

    /// <summary>
    /// Tests that setting SelectedUniversity with a LookupItem having an empty Id string
    /// still triggers collection clearing and LoadFacultiesAsync with empty string.
    /// Input: LookupItem with Id = "" (empty string).
    /// Expected: Collections are cleared and LoadFacultiesAsync is called with empty string.
    /// </summary>
    [TestMethod]
    public void SelectedUniversity_SetWithEmptyId_ClearsCollectionsAndTriggersLoad()
    {
        // Arrange
        var mockAuthService = new Mock<IAuthService>();
        var mockAcademicService = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<RegisterViewModel>>();
        mockAcademicService.Setup(s => s.GetFacultiesAsync(It.IsAny<string>()))
            .ReturnsAsync(new List<LookupItem>());
        var viewModel = new RegisterViewModel(mockAuthService.Object, mockAcademicService.Object, mockLogger.Object);

        viewModel.Faculties.Add(new LookupItem { Id = "f1", Name = "Faculty 1" });
        var university = new LookupItem { Id = "", Name = "Test University" };

        // Act
        viewModel.SelectedUniversity = university;
        Task.Delay(100).Wait();

        // Assert
        Assert.AreEqual(university, viewModel.SelectedUniversity);
        Assert.AreEqual(0, viewModel.Faculties.Count);
    }

    /// <summary>
    /// Tests that setting SelectedUniversity raises PropertyChanged event when the value changes.
    /// Input: New non-null LookupItem.
    /// Expected: PropertyChanged event is raised with property name "SelectedUniversity".
    /// </summary>
    [TestMethod]
    public void SelectedUniversity_SetNewValue_RaisesPropertyChanged()
    {
        // Arrange
        var mockAuthService = new Mock<IAuthService>();
        var mockAcademicService = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<RegisterViewModel>>();
        mockAcademicService.Setup(s => s.GetFacultiesAsync(It.IsAny<string>()))
            .ReturnsAsync(new List<LookupItem>());
        var viewModel = new RegisterViewModel(mockAuthService.Object, mockAcademicService.Object, mockLogger.Object);

        var university = new LookupItem { Id = "uni1", Name = "University 1" };
        var propertyChangedRaised = false;
        string? propertyName = null;

        viewModel.PropertyChanged += (sender, e) =>
        {
            if (e.PropertyName == nameof(viewModel.SelectedUniversity))
            {
                propertyChangedRaised = true;
                propertyName = e.PropertyName;
            }
        };

        // Act
        viewModel.SelectedUniversity = university;

        // Assert
        Assert.IsTrue(propertyChangedRaised);
        Assert.AreEqual(nameof(viewModel.SelectedUniversity), propertyName);
    }

    /// <summary>
    /// Tests that setting SelectedUniversity resets SelectedFaculty to null even when SelectedFaculty was previously set.
    /// Input: Set university after SelectedFaculty has a value.
    /// Expected: SelectedFaculty is set to null.
    /// </summary>
    [TestMethod]
    public void SelectedUniversity_SetValue_ResetsSelectedFacultyToNull()
    {
        // Arrange
        var mockAuthService = new Mock<IAuthService>();
        var mockAcademicService = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<RegisterViewModel>>();
        mockAcademicService.Setup(s => s.GetFacultiesAsync(It.IsAny<string>()))
            .ReturnsAsync(new List<LookupItem>());
        var viewModel = new RegisterViewModel(mockAuthService.Object, mockAcademicService.Object, mockLogger.Object);

        var initialUniversity = new LookupItem { Id = "uni1", Name = "University 1" };
        viewModel.SelectedUniversity = initialUniversity;
        var faculty = new LookupItem { Id = "f1", Name = "Faculty 1" };
        viewModel.SelectedFaculty = faculty;

        var newUniversity = new LookupItem { Id = "uni2", Name = "University 2" };

        // Act
        viewModel.SelectedUniversity = newUniversity;

        // Assert
        Assert.IsNull(viewModel.SelectedFaculty);
    }

    /// <summary>
    /// Tests that setting SelectedUniversity clears all three dependent collections.
    /// Input: University change with pre-populated Faculties, Departments, and Programs collections.
    /// Expected: All three collections are cleared.
    /// </summary>
    [TestMethod]
    public void SelectedUniversity_SetValue_ClearsAllThreeDependentCollections()
    {
        // Arrange
        var mockAuthService = new Mock<IAuthService>();
        var mockAcademicService = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<RegisterViewModel>>();
        mockAcademicService.Setup(s => s.GetFacultiesAsync(It.IsAny<string>()))
            .ReturnsAsync(new List<LookupItem>());
        var viewModel = new RegisterViewModel(mockAuthService.Object, mockAcademicService.Object, mockLogger.Object);

        viewModel.Faculties.Add(new LookupItem { Id = "f1", Name = "Faculty 1" });
        viewModel.Faculties.Add(new LookupItem { Id = "f2", Name = "Faculty 2" });
        viewModel.Departments.Add(new LookupItem { Id = "d1", Name = "Department 1" });
        viewModel.Departments.Add(new LookupItem { Id = "d2", Name = "Department 2" });
        viewModel.Programs.Add(new LookupItem { Id = "p1", Name = "Program 1" });
        viewModel.Programs.Add(new LookupItem { Id = "p2", Name = "Program 2" });

        var university = new LookupItem { Id = "uni1", Name = "University 1" };

        // Act
        viewModel.SelectedUniversity = university;

        // Assert
        Assert.AreEqual(0, viewModel.Faculties.Count);
        Assert.AreEqual(0, viewModel.Departments.Count);
        Assert.AreEqual(0, viewModel.Programs.Count);
    }

    /// <summary>
    /// Tests that setting SelectedUniversity does not raise PropertyChanged when set to the same value.
    /// Input: Same LookupItem instance set twice.
    /// Expected: PropertyChanged is raised only once (on first set).
    /// </summary>
    [TestMethod]
    public void SelectedUniversity_SetSameValue_DoesNotRaisePropertyChangedSecondTime()
    {
        // Arrange
        var mockAuthService = new Mock<IAuthService>();
        var mockAcademicService = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<RegisterViewModel>>();
        mockAcademicService.Setup(s => s.GetFacultiesAsync(It.IsAny<string>()))
            .ReturnsAsync(new List<LookupItem>());
        var viewModel = new RegisterViewModel(mockAuthService.Object, mockAcademicService.Object, mockLogger.Object);

        var university = new LookupItem { Id = "uni1", Name = "University 1" };
        viewModel.SelectedUniversity = university;

        var eventRaisedCount = 0;
        viewModel.PropertyChanged += (sender, e) =>
        {
            if (e.PropertyName == nameof(viewModel.SelectedUniversity))
            {
                eventRaisedCount++;
            }
        };

        // Act
        viewModel.SelectedUniversity = university;

        // Assert
        Assert.AreEqual(0, eventRaisedCount);
    }

    /// <summary>
    /// Tests that SelectedUniversity initializes to null by default.
    /// Input: None (testing default state).
    /// Expected: SelectedUniversity is null.
    /// </summary>
    [TestMethod]
    public void SelectedUniversity_DefaultValue_IsNull()
    {
        // Arrange
        var mockAuthService = new Mock<IAuthService>();
        var mockAcademicService = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<RegisterViewModel>>();
        var viewModel = new RegisterViewModel(mockAuthService.Object, mockAcademicService.Object, mockLogger.Object);

        // Act & Assert
        Assert.IsNull(viewModel.SelectedUniversity);
    }

    /// <summary>
    /// Tests that setting SelectedUniversity with whitespace-only Id triggers load correctly.
    /// Input: LookupItem with Id containing only whitespace.
    /// Expected: Collections are cleared and property is set.
    /// </summary>
    [TestMethod]
    [DataRow("   ")]
    [DataRow("\t")]
    [DataRow("\n")]
    public void SelectedUniversity_SetWithWhitespaceId_ClearsCollectionsAndSetsProperty(string whitespaceId)
    {
        // Arrange
        var mockAuthService = new Mock<IAuthService>();
        var mockAcademicService = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<RegisterViewModel>>();
        mockAcademicService.Setup(s => s.GetFacultiesAsync(It.IsAny<string>()))
            .ReturnsAsync(new List<LookupItem>());
        var viewModel = new RegisterViewModel(mockAuthService.Object, mockAcademicService.Object, mockLogger.Object);

        viewModel.Faculties.Add(new LookupItem { Id = "f1", Name = "Faculty 1" });
        var university = new LookupItem { Id = whitespaceId, Name = "Test University" };

        // Act
        viewModel.SelectedUniversity = university;
        Task.Delay(100).Wait();

        // Assert
        Assert.AreEqual(university, viewModel.SelectedUniversity);
        Assert.AreEqual(0, viewModel.Faculties.Count);
    }

    /// <summary>
    /// Tests that the SelectedAcademicYear property returns null by default when a new instance is created.
    /// Input: None (initial state).
    /// Expected: SelectedAcademicYear property returns null.
    /// </summary>
    [TestMethod]
    public void SelectedAcademicYear_InitialValue_ReturnsNull()
    {
        // Arrange
        var authServiceMock = new Mock<IAuthService>();
        var academicServiceMock = new Mock<IAcademicService>();
        var loggerMock = new Mock<ILogger<RegisterViewModel>>();

        // Act
        var viewModel = new RegisterViewModel(authServiceMock.Object, academicServiceMock.Object, loggerMock.Object);

        // Assert
        Assert.IsNull(viewModel.SelectedAcademicYear);
    }

    /// <summary>
    /// Tests that setting SelectedAcademicYear to a valid LookupItem stores the value correctly.
    /// Input: Valid LookupItem instance with non-empty Id and Name.
    /// Expected: Property returns the same LookupItem instance that was set.
    /// </summary>
    [TestMethod]
    public void SelectedAcademicYear_SetValidLookupItem_StoresAndReturnsValue()
    {
        // Arrange
        var authServiceMock = new Mock<IAuthService>();
        var academicServiceMock = new Mock<IAcademicService>();
        var loggerMock = new Mock<ILogger<RegisterViewModel>>();
        var viewModel = new RegisterViewModel(authServiceMock.Object, academicServiceMock.Object, loggerMock.Object);
        var academicYear = new LookupItem { Id = "2023-2024", Name = "Academic Year 2023-2024" };

        // Act
        viewModel.SelectedAcademicYear = academicYear;

        // Assert
        Assert.AreEqual(academicYear, viewModel.SelectedAcademicYear);
        Assert.AreEqual("2023-2024", viewModel.SelectedAcademicYear.Id);
        Assert.AreEqual("Academic Year 2023-2024", viewModel.SelectedAcademicYear.Name);
    }

    /// <summary>
    /// Tests that setting SelectedAcademicYear to different LookupItem instances with same content raises PropertyChanged.
    /// Input: Two different LookupItem instances with identical Id and Name values.
    /// Expected: PropertyChanged is raised because instances are different (reference equality check).
    /// </summary>
    [TestMethod]
    public void SelectedAcademicYear_SetToDifferentInstanceWithSameContent_RaisesPropertyChanged()
    {
        // Arrange
        var authServiceMock = new Mock<IAuthService>();
        var academicServiceMock = new Mock<IAcademicService>();
        var loggerMock = new Mock<ILogger<RegisterViewModel>>();
        var viewModel = new RegisterViewModel(authServiceMock.Object, academicServiceMock.Object, loggerMock.Object);
        var academicYear1 = new LookupItem { Id = "2023-2024", Name = "Academic Year 2023-2024" };
        var academicYear2 = new LookupItem { Id = "2023-2024", Name = "Academic Year 2023-2024" };
        viewModel.SelectedAcademicYear = academicYear1;
        int eventRaisedCount = 0;
        viewModel.PropertyChanged += (sender, e) => eventRaisedCount++;

        // Act
        viewModel.SelectedAcademicYear = academicYear2;

        // Assert
        Assert.AreEqual(1, eventRaisedCount);
        Assert.AreEqual(academicYear2, viewModel.SelectedAcademicYear);
    }

    /// <summary>
    /// Tests that setting SelectedAcademicYear with whitespace-only strings in LookupItem properties stores correctly.
    /// Input: LookupItem with whitespace strings for Id and Name.
    /// Expected: Property stores and returns the LookupItem with whitespace strings.
    /// </summary>
    [TestMethod]
    [DataRow("   ", "   ")]
    [DataRow("\t", "\t")]
    [DataRow("\n", "\n")]
    [DataRow(" \t\n ", " \t\n ")]
    public void SelectedAcademicYear_SetWithWhitespaceStrings_StoresValue(string id, string name)
    {
        // Arrange
        var authServiceMock = new Mock<IAuthService>();
        var academicServiceMock = new Mock<IAcademicService>();
        var loggerMock = new Mock<ILogger<RegisterViewModel>>();
        var viewModel = new RegisterViewModel(authServiceMock.Object, academicServiceMock.Object, loggerMock.Object);
        var academicYear = new LookupItem { Id = id, Name = name };

        // Act
        viewModel.SelectedAcademicYear = academicYear;

        // Assert
        Assert.AreEqual(academicYear, viewModel.SelectedAcademicYear);
        Assert.AreEqual(id, viewModel.SelectedAcademicYear.Id);
        Assert.AreEqual(name, viewModel.SelectedAcademicYear.Name);
    }

    /// <summary>
    /// Tests that setting SelectedAcademicYear with special characters in LookupItem properties stores correctly.
    /// Input: LookupItem with special characters, unicode, and control characters.
    /// Expected: Property stores and returns the LookupItem with special characters.
    /// </summary>
    [TestMethod]
    [DataRow("2023/2024", "Academic Year 2023/2024")]
    [DataRow("id-with-dashes", "Name with spaces")]
    [DataRow("!@#$%^&*()", "Special chars: !@#$%^&*()")]
    [DataRow("年份2023", "学年2023-2024")]
    [DataRow("id\u0000null", "name\u0001control")]
    public void SelectedAcademicYear_SetWithSpecialCharacters_StoresValue(string id, string name)
    {
        // Arrange
        var authServiceMock = new Mock<IAuthService>();
        var academicServiceMock = new Mock<IAcademicService>();
        var loggerMock = new Mock<ILogger<RegisterViewModel>>();
        var viewModel = new RegisterViewModel(authServiceMock.Object, academicServiceMock.Object, loggerMock.Object);
        var academicYear = new LookupItem { Id = id, Name = name };

        // Act
        viewModel.SelectedAcademicYear = academicYear;

        // Assert
        Assert.AreEqual(academicYear, viewModel.SelectedAcademicYear);
        Assert.AreEqual(id, viewModel.SelectedAcademicYear.Id);
        Assert.AreEqual(name, viewModel.SelectedAcademicYear.Name);
    }

    /// <summary>
    /// Tests that setting SelectedAcademicYear with very long strings in LookupItem properties stores correctly.
    /// Input: LookupItem with very long Id and Name strings (1000+ characters).
    /// Expected: Property stores and returns the LookupItem with very long strings.
    /// </summary>
    [TestMethod]
    public void SelectedAcademicYear_SetWithVeryLongStrings_StoresValue()
    {
        // Arrange
        var authServiceMock = new Mock<IAuthService>();
        var academicServiceMock = new Mock<IAcademicService>();
        var loggerMock = new Mock<ILogger<RegisterViewModel>>();
        var viewModel = new RegisterViewModel(authServiceMock.Object, academicServiceMock.Object, loggerMock.Object);
        var longId = new string('A', 1000);
        var longName = new string('B', 1000);
        var academicYear = new LookupItem { Id = longId, Name = longName };

        // Act
        viewModel.SelectedAcademicYear = academicYear;

        // Assert
        Assert.AreEqual(academicYear, viewModel.SelectedAcademicYear);
        Assert.AreEqual(longId, viewModel.SelectedAcademicYear.Id);
        Assert.AreEqual(longName, viewModel.SelectedAcademicYear.Name);
    }

    /// <summary>
    /// Tests that PropertyChanged event is raised for each different value when set multiple times consecutively.
    /// Input: Three different LookupItem instances set consecutively.
    /// Expected: PropertyChanged event is raised three times, once for each set.
    /// </summary>
    [TestMethod]
    public void SelectedAcademicYear_SetMultipleDifferentValuesConsecutively_RaisesPropertyChangedForEach()
    {
        // Arrange
        var authServiceMock = new Mock<IAuthService>();
        var academicServiceMock = new Mock<IAcademicService>();
        var loggerMock = new Mock<ILogger<RegisterViewModel>>();
        var viewModel = new RegisterViewModel(authServiceMock.Object, academicServiceMock.Object, loggerMock.Object);
        var academicYear1 = new LookupItem { Id = "2021-2022", Name = "Academic Year 2021-2022" };
        var academicYear2 = new LookupItem { Id = "2022-2023", Name = "Academic Year 2022-2023" };
        var academicYear3 = new LookupItem { Id = "2023-2024", Name = "Academic Year 2023-2024" };
        int eventRaisedCount = 0;
        viewModel.PropertyChanged += (sender, e) =>
        {
            if (e.PropertyName == "SelectedAcademicYear")
                eventRaisedCount++;
        };

        // Act
        viewModel.SelectedAcademicYear = academicYear1;
        viewModel.SelectedAcademicYear = academicYear2;
        viewModel.SelectedAcademicYear = academicYear3;

        // Assert
        Assert.AreEqual(3, eventRaisedCount);
        Assert.AreEqual(academicYear3, viewModel.SelectedAcademicYear);
    }

    /// <summary>
    /// Tests that setting SecondName from empty string to non-empty value raises PropertyChanged event.
    /// Input: Initial empty string, then set to "Doe".
    /// Expected: PropertyChanged event is raised.
    /// </summary>
    [TestMethod]
    public void SecondName_SetFromEmptyToNonEmpty_RaisesPropertyChangedEvent()
    {
        // Arrange
        var mockAuthService = new Mock<IAuthService>();
        var mockAcademicService = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<RegisterViewModel>>();
        var viewModel = new RegisterViewModel(mockAuthService.Object, mockAcademicService.Object, mockLogger.Object);
        var eventRaised = false;

        viewModel.PropertyChanged += (sender, e) =>
        {
            if (e.PropertyName == nameof(viewModel.SecondName))
            {
                eventRaised = true;
            }
        };

        // Act
        viewModel.SecondName = "Doe";

        // Assert
        Assert.IsTrue(eventRaised);
        Assert.AreEqual("Doe", viewModel.SecondName);
    }

    /// <summary>
    /// Tests that setting SecondName from non-empty value back to empty string raises PropertyChanged event.
    /// Input: Set to "Doe" first, then to empty string.
    /// Expected: PropertyChanged event is raised for the second set.
    /// </summary>
    [TestMethod]
    public void SecondName_SetFromNonEmptyToEmpty_RaisesPropertyChangedEvent()
    {
        // Arrange
        var mockAuthService = new Mock<IAuthService>();
        var mockAcademicService = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<RegisterViewModel>>();
        var viewModel = new RegisterViewModel(mockAuthService.Object, mockAcademicService.Object, mockLogger.Object);
        viewModel.SecondName = "Doe";

        var eventRaised = false;
        viewModel.PropertyChanged += (sender, e) =>
        {
            if (e.PropertyName == nameof(viewModel.SecondName))
            {
                eventRaised = true;
            }
        };

        // Act
        viewModel.SecondName = string.Empty;

        // Assert
        Assert.IsTrue(eventRaised);
        Assert.AreEqual(string.Empty, viewModel.SecondName);
    }

    /// <summary>
    /// Tests that setting SecondName with strings containing numbers and special symbols works correctly.
    /// Input: Strings with numbers, hyphens, apostrophes, and mixed content.
    /// Expected: Property value is updated correctly.
    /// </summary>
    /// <param name="value">The value to set.</param>
    [TestMethod]
    [DataRow("Smith-Jones")]
    [DataRow("O'Malley")]
    [DataRow("Di Giovanni")]
    [DataRow("Smith2")]
    [DataRow("Doe3rd")]
    [DataRow("Name-With-Hyphens")]
    [DataRow("Name's")]
    public void SecondName_SetValueWithNumbersAndSymbols_UpdatesPropertyCorrectly(string value)
    {
        // Arrange
        var mockAuthService = new Mock<IAuthService>();
        var mockAcademicService = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<RegisterViewModel>>();
        var viewModel = new RegisterViewModel(mockAuthService.Object, mockAcademicService.Object, mockLogger.Object);

        // Act
        viewModel.SecondName = value;

        // Assert
        Assert.AreEqual(value, viewModel.SecondName);
    }

    /// <summary>
    /// Tests that setting SecondName to empty string when already empty does not raise PropertyChanged event.
    /// Input: Empty string set twice (initial state and explicit set).
    /// Expected: PropertyChanged event is not raised.
    /// </summary>
    [TestMethod]
    public void SecondName_SetEmptyWhenAlreadyEmpty_DoesNotRaisePropertyChangedEvent()
    {
        // Arrange
        var mockAuthService = new Mock<IAuthService>();
        var mockAcademicService = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<RegisterViewModel>>();
        var viewModel = new RegisterViewModel(mockAuthService.Object, mockAcademicService.Object, mockLogger.Object);

        var eventRaisedCount = 0;
        viewModel.PropertyChanged += (sender, e) =>
        {
            if (e.PropertyName == nameof(viewModel.SecondName))
            {
                eventRaisedCount++;
            }
        };

        // Act
        viewModel.SecondName = string.Empty;

        // Assert
        Assert.AreEqual(0, eventRaisedCount);
        Assert.AreEqual(string.Empty, viewModel.SecondName);
    }

    /// <summary>
    /// Tests that the IsOtpSent property has the correct default value of false when RegisterViewModel is instantiated.
    /// Input: None (testing initial state).
    /// Expected: IsOtpSent returns false.
    /// </summary>
    [TestMethod]
    public void IsOtpSent_InitialValue_ReturnsFalse()
    {
        // Arrange
        var mockAuthService = new Mock<IAuthService>();
        var mockAcademicService = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<RegisterViewModel>>();
        var viewModel = new RegisterViewModel(mockAuthService.Object, mockAcademicService.Object, mockLogger.Object);

        // Act
        var result = viewModel.IsOtpSent;

        // Assert
        Assert.IsFalse(result);
    }

    /// <summary>
    /// Tests that setting the IsOtpSent property to a new value updates the property correctly and raises PropertyChanged event when the value changes.
    /// Tests all possible boolean value transitions (false→true, true→false) and same-value scenarios.
    /// Input: Initial value and new value combinations.
    /// Expected: Property is updated and PropertyChanged event is raised only when the value actually changes.
    /// </summary>
    /// <param name="initialValue">The initial value to set the property to.</param>
    /// <param name="newValue">The new value to set the property to.</param>
    /// <param name="shouldRaisePropertyChanged">Whether PropertyChanged event should be raised.</param>
    [TestMethod]
    [DataRow(false, true, true, DisplayName = "IsOtpSent from false to true raises PropertyChanged")]
    [DataRow(true, false, true, DisplayName = "IsOtpSent from true to false raises PropertyChanged")]
    [DataRow(false, false, false, DisplayName = "IsOtpSent from false to false does not raise PropertyChanged")]
    [DataRow(true, true, false, DisplayName = "IsOtpSent from true to true does not raise PropertyChanged")]
    public void IsOtpSent_SetValue_UpdatesPropertyAndRaisesPropertyChangedOnlyWhenValueChanges(bool initialValue, bool newValue, bool shouldRaisePropertyChanged)
    {
        // Arrange
        var mockAuthService = new Mock<IAuthService>();
        var mockAcademicService = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<RegisterViewModel>>();
        var viewModel = new RegisterViewModel(mockAuthService.Object, mockAcademicService.Object, mockLogger.Object);
        viewModel.IsOtpSent = initialValue;

        bool propertyChangedRaised = false;
        string? raisedPropertyName = null;
        viewModel.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == nameof(viewModel.IsOtpSent))
            {
                propertyChangedRaised = true;
                raisedPropertyName = args.PropertyName;
            }
        };

        // Act
        viewModel.IsOtpSent = newValue;

        // Assert
        Assert.AreEqual(newValue, viewModel.IsOtpSent);
        Assert.AreEqual(shouldRaisePropertyChanged, propertyChangedRaised);
        if (shouldRaisePropertyChanged)
        {
            Assert.AreEqual(nameof(viewModel.IsOtpSent), raisedPropertyName);
        }
    }

    /// <summary>
    /// Tests that getting the IsOtpSent property returns the correct value after setting it.
    /// Input: Setting property to true and false.
    /// Expected: Getter returns the exact value that was set.
    /// </summary>
    /// <param name="value">The boolean value to set and verify.</param>
    [TestMethod]
    [DataRow(true, DisplayName = "IsOtpSent get returns true")]
    [DataRow(false, DisplayName = "IsOtpSent get returns false")]
    public void IsOtpSent_GetValue_ReturnsSetValue(bool value)
    {
        // Arrange
        var mockAuthService = new Mock<IAuthService>();
        var mockAcademicService = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<RegisterViewModel>>();
        var viewModel = new RegisterViewModel(mockAuthService.Object, mockAcademicService.Object, mockLogger.Object);

        // Act
        viewModel.IsOtpSent = value;
        var result = viewModel.IsOtpSent;

        // Assert
        Assert.AreEqual(value, result);
    }

    /// <summary>
    /// Tests that setting the IsOtpSent property multiple times with alternating values raises PropertyChanged event for each actual change.
    /// Input: Sequence of alternating boolean values (false→true→false→true).
    /// Expected: PropertyChanged event is raised for each value change, but not when setting to the same value.
    /// </summary>
    [TestMethod]
    public void IsOtpSent_SetMultipleDifferentValues_RaisesPropertyChangedForEachChange()
    {
        // Arrange
        var mockAuthService = new Mock<IAuthService>();
        var mockAcademicService = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<RegisterViewModel>>();
        var viewModel = new RegisterViewModel(mockAuthService.Object, mockAcademicService.Object, mockLogger.Object);

        int propertyChangedCount = 0;
        viewModel.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == nameof(viewModel.IsOtpSent))
            {
                propertyChangedCount++;
            }
        };

        // Act
        viewModel.IsOtpSent = true;  // Change from false to true
        viewModel.IsOtpSent = true;  // No change (true to true)
        viewModel.IsOtpSent = false; // Change from true to false
        viewModel.IsOtpSent = false; // No change (false to false)
        viewModel.IsOtpSent = true;  // Change from false to true

        // Assert
        Assert.AreEqual(3, propertyChangedCount);
        Assert.IsTrue(viewModel.IsOtpSent);
    }

    /// <summary>
    /// Tests that setting the IsOtpSent property raises PropertyChanged event with the correct property name.
    /// Input: Setting property to true.
    /// Expected: PropertyChanged event is raised with PropertyName equal to "IsOtpSent".
    /// </summary>
    [TestMethod]
    public void IsOtpSent_SetValue_RaisesPropertyChangedWithCorrectPropertyName()
    {
        // Arrange
        var mockAuthService = new Mock<IAuthService>();
        var mockAcademicService = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<RegisterViewModel>>();
        var viewModel = new RegisterViewModel(mockAuthService.Object, mockAcademicService.Object, mockLogger.Object);

        string? raisedPropertyName = null;
        viewModel.PropertyChanged += (sender, args) =>
        {
            raisedPropertyName = args.PropertyName;
        };

        // Act
        viewModel.IsOtpSent = true;

        // Assert
        Assert.AreEqual(nameof(viewModel.IsOtpSent), raisedPropertyName);
    }

    /// <summary>
    /// Tests that setting the IsOtpSent property to the same value multiple times consecutively does not raise PropertyChanged event after the first set.
    /// Input: Setting property to true three times consecutively.
    /// Expected: PropertyChanged event is raised only once (on the first change from default false to true).
    /// </summary>
    [TestMethod]
    public void IsOtpSent_SetSameValueMultipleTimes_RaisesPropertyChangedOnlyOnce()
    {
        // Arrange
        var mockAuthService = new Mock<IAuthService>();
        var mockAcademicService = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<RegisterViewModel>>();
        var viewModel = new RegisterViewModel(mockAuthService.Object, mockAcademicService.Object, mockLogger.Object);

        int propertyChangedCount = 0;
        viewModel.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == nameof(viewModel.IsOtpSent))
            {
                propertyChangedCount++;
            }
        };

        // Act
        viewModel.IsOtpSent = true;  // First change from false to true
        viewModel.IsOtpSent = true;  // Same value
        viewModel.IsOtpSent = true;  // Same value

        // Assert
        Assert.AreEqual(1, propertyChangedCount);
        Assert.IsTrue(viewModel.IsOtpSent);
    }

    /// <summary>
    /// Tests that the IsOtpSent property correctly handles transition from true to false.
    /// Input: Setting property to true, then to false.
    /// Expected: Property value is false and PropertyChanged event was raised for both transitions.
    /// </summary>
    [TestMethod]
    public void IsOtpSent_TransitionFromTrueToFalse_UpdatesCorrectlyAndRaisesPropertyChanged()
    {
        // Arrange
        var mockAuthService = new Mock<IAuthService>();
        var mockAcademicService = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<RegisterViewModel>>();
        var viewModel = new RegisterViewModel(mockAuthService.Object, mockAcademicService.Object, mockLogger.Object);

        viewModel.IsOtpSent = true;

        int propertyChangedCount = 0;
        viewModel.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == nameof(viewModel.IsOtpSent))
            {
                propertyChangedCount++;
            }
        };

        // Act
        viewModel.IsOtpSent = false;

        // Assert
        Assert.IsFalse(viewModel.IsOtpSent);
        Assert.AreEqual(1, propertyChangedCount);
    }

    /// <summary>
    /// Tests that the IsOtpSent property correctly handles transition from false to true.
    /// Input: Explicitly setting property to false (same as default), then to true.
    /// Expected: Property value is true and PropertyChanged event was raised for the transition to true.
    /// </summary>
    [TestMethod]
    public void IsOtpSent_TransitionFromFalseToTrue_UpdatesCorrectlyAndRaisesPropertyChanged()
    {
        // Arrange
        var mockAuthService = new Mock<IAuthService>();
        var mockAcademicService = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<RegisterViewModel>>();
        var viewModel = new RegisterViewModel(mockAuthService.Object, mockAcademicService.Object, mockLogger.Object);

        viewModel.IsOtpSent = false; // Explicitly set to false

        int propertyChangedCount = 0;
        viewModel.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == nameof(viewModel.IsOtpSent))
            {
                propertyChangedCount++;
            }
        };

        // Act
        viewModel.IsOtpSent = true;

        // Assert
        Assert.IsTrue(viewModel.IsOtpSent);
        Assert.AreEqual(1, propertyChangedCount);
    }

    /// <summary>
    /// Tests that setting SelectedEntryScheme to a valid LookupItem updates the property correctly.
    /// Input: Valid LookupItem with Id and Name.
    /// Expected: Property is set to the provided value.
    /// </summary>
    [TestMethod]
    public void SelectedEntryScheme_SetValidValue_UpdatesProperty()
    {
        // Arrange
        var mockAuthService = new Mock<IAuthService>();
        var mockAcademicService = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<RegisterViewModel>>();
        var viewModel = new RegisterViewModel(mockAuthService.Object, mockAcademicService.Object, mockLogger.Object);
        var lookupItem = new LookupItem { Id = "scheme1", Name = "Direct Entry" };

        // Act
        viewModel.SelectedEntryScheme = lookupItem;

        // Assert
        Assert.AreEqual(lookupItem, viewModel.SelectedEntryScheme);
        Assert.AreEqual("scheme1", viewModel.SelectedEntryScheme.Id);
        Assert.AreEqual("Direct Entry", viewModel.SelectedEntryScheme.Name);
    }

    /// <summary>
    /// Tests that setting SelectedEntryScheme to null updates the property correctly.
    /// Input: Null value.
    /// Expected: Property is set to null.
    /// </summary>
    [TestMethod]
    public void SelectedEntryScheme_SetNull_UpdatesProperty()
    {
        // Arrange
        var mockAuthService = new Mock<IAuthService>();
        var mockAcademicService = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<RegisterViewModel>>();
        var viewModel = new RegisterViewModel(mockAuthService.Object, mockAcademicService.Object, mockLogger.Object);
        var lookupItem = new LookupItem { Id = "scheme1", Name = "Direct Entry" };
        viewModel.SelectedEntryScheme = lookupItem;

        // Act
        viewModel.SelectedEntryScheme = null;

        // Assert
        Assert.IsNull(viewModel.SelectedEntryScheme);
    }

    /// <summary>
    /// Tests that setting SelectedEntryScheme to a new value raises PropertyChanged event with the correct property name.
    /// Input: Valid LookupItem.
    /// Expected: PropertyChanged event is raised with PropertyName = "SelectedEntryScheme".
    /// </summary>
    [TestMethod]
    public void SelectedEntryScheme_SetNewValue_RaisesPropertyChangedEvent()
    {
        // Arrange
        var mockAuthService = new Mock<IAuthService>();
        var mockAcademicService = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<RegisterViewModel>>();
        var viewModel = new RegisterViewModel(mockAuthService.Object, mockAcademicService.Object, mockLogger.Object);
        var lookupItem = new LookupItem { Id = "scheme1", Name = "Direct Entry" };
        string? raisedPropertyName = null;
        viewModel.PropertyChanged += (sender, args) => raisedPropertyName = args.PropertyName;

        // Act
        viewModel.SelectedEntryScheme = lookupItem;

        // Assert
        Assert.AreEqual("SelectedEntryScheme", raisedPropertyName);
    }

    /// <summary>
    /// Tests that setting SelectedEntryScheme to different LookupItem instances raises PropertyChanged for each change.
    /// Input: Multiple different LookupItem instances.
    /// Expected: PropertyChanged event is raised for each distinct set operation.
    /// </summary>
    [TestMethod]
    public void SelectedEntryScheme_SetDifferentValues_RaisesPropertyChangedForEachChange()
    {
        // Arrange
        var mockAuthService = new Mock<IAuthService>();
        var mockAcademicService = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<RegisterViewModel>>();
        var viewModel = new RegisterViewModel(mockAuthService.Object, mockAcademicService.Object, mockLogger.Object);
        var lookupItem1 = new LookupItem { Id = "scheme1", Name = "Direct Entry" };
        var lookupItem2 = new LookupItem { Id = "scheme2", Name = "Mature Entry" };
        var lookupItem3 = new LookupItem { Id = "scheme3", Name = "Government Sponsorship" };
        int eventRaisedCount = 0;
        viewModel.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == "SelectedEntryScheme")
                eventRaisedCount++;
        };

        // Act
        viewModel.SelectedEntryScheme = lookupItem1;
        viewModel.SelectedEntryScheme = lookupItem2;
        viewModel.SelectedEntryScheme = lookupItem3;

        // Assert
        Assert.AreEqual(3, eventRaisedCount);
        Assert.AreEqual(lookupItem3, viewModel.SelectedEntryScheme);
    }

    /// <summary>
    /// Tests that setting SelectedEntryScheme from null to a value and back to null raises PropertyChanged for each transition.
    /// Input: null -> LookupItem -> null.
    /// Expected: PropertyChanged is raised for both transitions.
    /// </summary>
    [TestMethod]
    public void SelectedEntryScheme_SetFromNullToValueToNull_RaisesPropertyChangedForEachTransition()
    {
        // Arrange
        var mockAuthService = new Mock<IAuthService>();
        var mockAcademicService = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<RegisterViewModel>>();
        var viewModel = new RegisterViewModel(mockAuthService.Object, mockAcademicService.Object, mockLogger.Object);
        var lookupItem = new LookupItem { Id = "scheme1", Name = "Direct Entry" };
        int eventRaisedCount = 0;
        viewModel.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == "SelectedEntryScheme")
                eventRaisedCount++;
        };

        // Act
        viewModel.SelectedEntryScheme = lookupItem;
        viewModel.SelectedEntryScheme = null;

        // Assert
        Assert.AreEqual(2, eventRaisedCount);
        Assert.IsNull(viewModel.SelectedEntryScheme);
    }

    /// <summary>
    /// Tests that SelectedEntryScheme handles LookupItem with empty strings for Id and Name correctly.
    /// Input: LookupItem with empty Id and empty Name.
    /// Expected: Property is set correctly and PropertyChanged is raised.
    /// </summary>
    [TestMethod]
    public void SelectedEntryScheme_SetLookupItemWithEmptyStrings_UpdatesPropertyAndRaisesPropertyChanged()
    {
        // Arrange
        var mockAuthService = new Mock<IAuthService>();
        var mockAcademicService = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<RegisterViewModel>>();
        var viewModel = new RegisterViewModel(mockAuthService.Object, mockAcademicService.Object, mockLogger.Object);
        var lookupItem = new LookupItem { Id = "", Name = "" };
        string? raisedPropertyName = null;
        viewModel.PropertyChanged += (sender, args) => raisedPropertyName = args.PropertyName;

        // Act
        viewModel.SelectedEntryScheme = lookupItem;

        // Assert
        Assert.AreEqual(lookupItem, viewModel.SelectedEntryScheme);
        Assert.AreEqual("", viewModel.SelectedEntryScheme.Id);
        Assert.AreEqual("", viewModel.SelectedEntryScheme.Name);
        Assert.AreEqual("SelectedEntryScheme", raisedPropertyName);
    }

    /// <summary>
    /// Tests that SelectedEntryScheme handles LookupItem with whitespace strings for Id and Name correctly.
    /// Input: LookupItem with whitespace-only Id and Name.
    /// Expected: Property is set correctly.
    /// </summary>
    [TestMethod]
    [DataRow("   ", "   ")]
    [DataRow("\t", "\n")]
    [DataRow(" ", "\r\n")]
    [DataRow("  \t  ", "  \n  ")]
    public void SelectedEntryScheme_SetLookupItemWithWhitespace_UpdatesProperty(string id, string name)
    {
        // Arrange
        var mockAuthService = new Mock<IAuthService>();
        var mockAcademicService = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<RegisterViewModel>>();
        var viewModel = new RegisterViewModel(mockAuthService.Object, mockAcademicService.Object, mockLogger.Object);
        var lookupItem = new LookupItem { Id = id, Name = name };

        // Act
        viewModel.SelectedEntryScheme = lookupItem;

        // Assert
        Assert.AreEqual(lookupItem, viewModel.SelectedEntryScheme);
        Assert.AreEqual(id, viewModel.SelectedEntryScheme.Id);
        Assert.AreEqual(name, viewModel.SelectedEntryScheme.Name);
    }

    /// <summary>
    /// Tests that SelectedEntryScheme handles LookupItem with special characters in Id and Name correctly.
    /// Input: LookupItem with special characters.
    /// Expected: Property is set correctly.
    /// </summary>
    [TestMethod]
    [DataRow("scheme!@#$%", "Entry Scheme with Special Chars!@#")]
    [DataRow("scheme-123_abc", "Entry_Scheme-Name")]
    [DataRow("схема1", "схема входа")]
    [DataRow("スキーム1", "エントリースキーム")]
    [DataRow("scheme😀", "Entry Scheme 🎓")]
    public void SelectedEntryScheme_SetLookupItemWithSpecialCharacters_UpdatesProperty(string id, string name)
    {
        // Arrange
        var mockAuthService = new Mock<IAuthService>();
        var mockAcademicService = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<RegisterViewModel>>();
        var viewModel = new RegisterViewModel(mockAuthService.Object, mockAcademicService.Object, mockLogger.Object);
        var lookupItem = new LookupItem { Id = id, Name = name };

        // Act
        viewModel.SelectedEntryScheme = lookupItem;

        // Assert
        Assert.AreEqual(lookupItem, viewModel.SelectedEntryScheme);
        Assert.AreEqual(id, viewModel.SelectedEntryScheme.Id);
        Assert.AreEqual(name, viewModel.SelectedEntryScheme.Name);
    }

    /// <summary>
    /// Tests that SelectedEntryScheme handles LookupItem with very long strings for Id and Name correctly.
    /// Input: LookupItem with very long Id and Name (1000+ characters each).
    /// Expected: Property is set correctly.
    /// </summary>
    [TestMethod]
    public void SelectedEntryScheme_SetLookupItemWithVeryLongStrings_UpdatesProperty()
    {
        // Arrange
        var mockAuthService = new Mock<IAuthService>();
        var mockAcademicService = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<RegisterViewModel>>();
        var viewModel = new RegisterViewModel(mockAuthService.Object, mockAcademicService.Object, mockLogger.Object);
        var longId = new string('a', 5000);
        var longName = new string('b', 5000);
        var lookupItem = new LookupItem { Id = longId, Name = longName };

        // Act
        viewModel.SelectedEntryScheme = lookupItem;

        // Assert
        Assert.AreEqual(lookupItem, viewModel.SelectedEntryScheme);
        Assert.AreEqual(longId, viewModel.SelectedEntryScheme.Id);
        Assert.AreEqual(longName, viewModel.SelectedEntryScheme.Name);
    }

    /// <summary>
    /// Tests that SelectedEntryScheme handles LookupItem with control characters in Id and Name correctly.
    /// Input: LookupItem with control characters.
    /// Expected: Property is set correctly.
    /// </summary>
    [TestMethod]
    [DataRow("scheme\u0000id", "name\u0000entry")]
    [DataRow("scheme\u0001id", "name\u0002entry")]
    [DataRow("scheme\rid", "name\nentry")]
    [DataRow("scheme\tid", "name\tentry")]
    public void SelectedEntryScheme_SetLookupItemWithControlCharacters_UpdatesProperty(string id, string name)
    {
        // Arrange
        var mockAuthService = new Mock<IAuthService>();
        var mockAcademicService = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<RegisterViewModel>>();
        var viewModel = new RegisterViewModel(mockAuthService.Object, mockAcademicService.Object, mockLogger.Object);
        var lookupItem = new LookupItem { Id = id, Name = name };

        // Act
        viewModel.SelectedEntryScheme = lookupItem;

        // Assert
        Assert.AreEqual(lookupItem, viewModel.SelectedEntryScheme);
        Assert.AreEqual(id, viewModel.SelectedEntryScheme.Id);
        Assert.AreEqual(name, viewModel.SelectedEntryScheme.Name);
    }

    /// <summary>
    /// Tests that setting SelectedEntryScheme multiple times with alternating null and non-null values works correctly.
    /// Input: Alternating between null and different LookupItem instances.
    /// Expected: PropertyChanged is raised for each change and property value is updated correctly.
    /// </summary>
    [TestMethod]
    public void SelectedEntryScheme_AlternateNullAndNonNullValues_UpdatesCorrectlyEachTime()
    {
        // Arrange
        var mockAuthService = new Mock<IAuthService>();
        var mockAcademicService = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<RegisterViewModel>>();
        var viewModel = new RegisterViewModel(mockAuthService.Object, mockAcademicService.Object, mockLogger.Object);
        var lookupItem1 = new LookupItem { Id = "scheme1", Name = "Entry1" };
        var lookupItem2 = new LookupItem { Id = "scheme2", Name = "Entry2" };
        int eventRaisedCount = 0;
        viewModel.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == "SelectedEntryScheme")
                eventRaisedCount++;
        };

        // Act & Assert
        viewModel.SelectedEntryScheme = lookupItem1;
        Assert.AreEqual(lookupItem1, viewModel.SelectedEntryScheme);
        Assert.AreEqual(1, eventRaisedCount);

        viewModel.SelectedEntryScheme = null;
        Assert.IsNull(viewModel.SelectedEntryScheme);
        Assert.AreEqual(2, eventRaisedCount);

        viewModel.SelectedEntryScheme = lookupItem2;
        Assert.AreEqual(lookupItem2, viewModel.SelectedEntryScheme);
        Assert.AreEqual(3, eventRaisedCount);

        viewModel.SelectedEntryScheme = null;
        Assert.IsNull(viewModel.SelectedEntryScheme);
        Assert.AreEqual(4, eventRaisedCount);
    }

    /// <summary>
    /// Tests that GoToStep3 sets CurrentStep to 3 when called.
    /// Input: None (method has no parameters).
    /// Expected: CurrentStep property equals 3.
    /// </summary>
    [TestMethod]
    public void GoToStep3_Called_SetsCurrentStepTo3()
    {
        // Arrange
        var authServiceMock = new Mock<IAuthService>();
        var academicServiceMock = new Mock<IAcademicService>();
        var loggerMock = new Mock<ILogger<RegisterViewModel>>();
        var viewModel = new RegisterViewModel(authServiceMock.Object, academicServiceMock.Object, loggerMock.Object);

        // Act
        viewModel.GoToStep3();

        // Assert
        Assert.AreEqual(3, viewModel.CurrentStep);
    }

    /// <summary>
    /// Tests that GoToStep3 sets IsStep3 to true after execution.
    /// Input: None.
    /// Expected: IsStep3 returns true.
    /// </summary>
    [TestMethod]
    public void GoToStep3_Called_SetsIsStep3ToTrue()
    {
        // Arrange
        var authServiceMock = new Mock<IAuthService>();
        var academicServiceMock = new Mock<IAcademicService>();
        var loggerMock = new Mock<ILogger<RegisterViewModel>>();
        var viewModel = new RegisterViewModel(authServiceMock.Object, academicServiceMock.Object, loggerMock.Object);

        // Act
        viewModel.GoToStep3();

        // Assert
        Assert.IsTrue(viewModel.IsStep3);
    }

    /// <summary>
    /// Tests that GoToStep3 sets IsStep1 to false after execution.
    /// Input: None.
    /// Expected: IsStep1 returns false.
    /// </summary>
    [TestMethod]
    public void GoToStep3_Called_SetsIsStep1ToFalse()
    {
        // Arrange
        var authServiceMock = new Mock<IAuthService>();
        var academicServiceMock = new Mock<IAcademicService>();
        var loggerMock = new Mock<ILogger<RegisterViewModel>>();
        var viewModel = new RegisterViewModel(authServiceMock.Object, academicServiceMock.Object, loggerMock.Object);

        // Act
        viewModel.GoToStep3();

        // Assert
        Assert.IsFalse(viewModel.IsStep1);
    }

    /// <summary>
    /// Tests that GoToStep3 sets IsStep2 to false after execution.
    /// Input: None.
    /// Expected: IsStep2 returns false.
    /// </summary>
    [TestMethod]
    public void GoToStep3_Called_SetsIsStep2ToFalse()
    {
        // Arrange
        var authServiceMock = new Mock<IAuthService>();
        var academicServiceMock = new Mock<IAcademicService>();
        var loggerMock = new Mock<ILogger<RegisterViewModel>>();
        var viewModel = new RegisterViewModel(authServiceMock.Object, academicServiceMock.Object, loggerMock.Object);

        // Act
        viewModel.GoToStep3();

        // Assert
        Assert.IsFalse(viewModel.IsStep2);
    }

    /// <summary>
    /// Tests that GoToStep3 sets CurrentStep to 3 from various initial step values.
    /// Input: Different initial values for CurrentStep including edge cases.
    /// Expected: CurrentStep is always set to 3 regardless of initial value.
    /// </summary>
    /// <param name="initialStep">The initial value of CurrentStep before calling GoToStep3.</param>
    [TestMethod]
    [DataRow(1)]
    [DataRow(2)]
    [DataRow(3)]
    [DataRow(0)]
    [DataRow(-1)]
    [DataRow(4)]
    [DataRow(10)]
    [DataRow(100)]
    [DataRow(int.MinValue)]
    [DataRow(int.MaxValue)]
    public void GoToStep3_FromVariousInitialSteps_AlwaysSetsCurrentStepTo3(int initialStep)
    {
        // Arrange
        var authServiceMock = new Mock<IAuthService>();
        var academicServiceMock = new Mock<IAcademicService>();
        var loggerMock = new Mock<ILogger<RegisterViewModel>>();
        var viewModel = new RegisterViewModel(authServiceMock.Object, academicServiceMock.Object, loggerMock.Object);
        viewModel.CurrentStep = initialStep;

        // Act
        viewModel.GoToStep3();

        // Assert
        Assert.AreEqual(3, viewModel.CurrentStep);
        Assert.IsTrue(viewModel.IsStep3);
        Assert.IsFalse(viewModel.IsStep1);
        Assert.IsFalse(viewModel.IsStep2);
    }

    /// <summary>
    /// Tests that GoToStep3 raises PropertyChanged events for IsStep1, IsStep2, and IsStep3.
    /// Input: None.
    /// Expected: PropertyChanged events are raised for all three step indicator properties.
    /// </summary>
    [TestMethod]
    public void GoToStep3_Called_RaisesPropertyChangedForStepIndicators()
    {
        // Arrange
        var authServiceMock = new Mock<IAuthService>();
        var academicServiceMock = new Mock<IAcademicService>();
        var loggerMock = new Mock<ILogger<RegisterViewModel>>();
        var viewModel = new RegisterViewModel(authServiceMock.Object, academicServiceMock.Object, loggerMock.Object);

        var propertyChangedEvents = new List<string>();
        viewModel.PropertyChanged += (sender, e) => propertyChangedEvents.Add(e.PropertyName ?? string.Empty);

        // Act
        viewModel.GoToStep3();

        // Assert
        Assert.IsTrue(propertyChangedEvents.Contains("IsStep1"), "PropertyChanged for IsStep1 was not raised");
        Assert.IsTrue(propertyChangedEvents.Contains("IsStep2"), "PropertyChanged for IsStep2 was not raised");
        Assert.IsTrue(propertyChangedEvents.Contains("IsStep3"), "PropertyChanged for IsStep3 was not raised");
    }

    /// <summary>
    /// Tests that GoToStep3 raises PropertyChanged event for CurrentStep through the property setter.
    /// Input: None.
    /// Expected: PropertyChanged event is raised for CurrentStep.
    /// </summary>
    [TestMethod]
    public void GoToStep3_Called_RaisesPropertyChangedForCurrentStep()
    {
        // Arrange
        var authServiceMock = new Mock<IAuthService>();
        var academicServiceMock = new Mock<IAcademicService>();
        var loggerMock = new Mock<ILogger<RegisterViewModel>>();
        var viewModel = new RegisterViewModel(authServiceMock.Object, academicServiceMock.Object, loggerMock.Object);

        var propertyChangedEvents = new List<string>();
        viewModel.PropertyChanged += (sender, e) => propertyChangedEvents.Add(e.PropertyName ?? string.Empty);

        // Act
        viewModel.GoToStep3();

        // Assert
        Assert.IsTrue(propertyChangedEvents.Contains("CurrentStep"), "PropertyChanged for CurrentStep was not raised");
    }

    /// <summary>
    /// Tests that calling GoToStep3 multiple times is idempotent and maintains consistent state.
    /// Input: Multiple consecutive calls to GoToStep3.
    /// Expected: CurrentStep remains 3 and all step indicators remain in correct state.
    /// </summary>
    [TestMethod]
    public void GoToStep3_CalledMultipleTimes_MaintainsIdempotency()
    {
        // Arrange
        var authServiceMock = new Mock<IAuthService>();
        var academicServiceMock = new Mock<IAcademicService>();
        var loggerMock = new Mock<ILogger<RegisterViewModel>>();
        var viewModel = new RegisterViewModel(authServiceMock.Object, academicServiceMock.Object, loggerMock.Object);

        // Act
        viewModel.GoToStep3();
        viewModel.GoToStep3();
        viewModel.GoToStep3();

        // Assert
        Assert.AreEqual(3, viewModel.CurrentStep);
        Assert.IsTrue(viewModel.IsStep3);
        Assert.IsFalse(viewModel.IsStep1);
        Assert.IsFalse(viewModel.IsStep2);
    }

    /// <summary>
    /// Tests that GoToStep3 raises all expected PropertyChanged events in a single call.
    /// Input: None.
    /// Expected: PropertyChanged events are raised for CurrentStep, IsStep1, IsStep2, and IsStep3.
    /// </summary>
    [TestMethod]
    public void GoToStep3_Called_RaisesAllExpectedPropertyChangedEvents()
    {
        // Arrange
        var authServiceMock = new Mock<IAuthService>();
        var academicServiceMock = new Mock<IAcademicService>();
        var loggerMock = new Mock<ILogger<RegisterViewModel>>();
        var viewModel = new RegisterViewModel(authServiceMock.Object, academicServiceMock.Object, loggerMock.Object);

        var propertyChangedEvents = new List<string>();
        viewModel.PropertyChanged += (sender, e) => propertyChangedEvents.Add(e.PropertyName ?? string.Empty);

        // Act
        viewModel.GoToStep3();

        // Assert
        Assert.IsTrue(propertyChangedEvents.Contains("CurrentStep"));
        Assert.IsTrue(propertyChangedEvents.Contains("IsStep1"));
        Assert.IsTrue(propertyChangedEvents.Contains("IsStep2"));
        Assert.IsTrue(propertyChangedEvents.Contains("IsStep3"));
    }

    /// <summary>
    /// Tests that GoToStep3 transitions correctly when CurrentStep is already at step 3.
    /// Input: CurrentStep initially set to 3.
    /// Expected: CurrentStep remains 3, PropertyChanged events are still raised.
    /// </summary>
    [TestMethod]
    public void GoToStep3_WhenAlreadyAtStep3_MaintainsStateAndRaisesEvents()
    {
        // Arrange
        var authServiceMock = new Mock<IAuthService>();
        var academicServiceMock = new Mock<IAcademicService>();
        var loggerMock = new Mock<ILogger<RegisterViewModel>>();
        var viewModel = new RegisterViewModel(authServiceMock.Object, academicServiceMock.Object, loggerMock.Object);
        viewModel.CurrentStep = 3;

        var propertyChangedEvents = new List<string>();
        viewModel.PropertyChanged += (sender, e) => propertyChangedEvents.Add(e.PropertyName ?? string.Empty);

        // Act
        viewModel.GoToStep3();

        // Assert
        Assert.AreEqual(3, viewModel.CurrentStep);
        Assert.IsTrue(viewModel.IsStep3);
        Assert.IsTrue(propertyChangedEvents.Contains("IsStep1"));
        Assert.IsTrue(propertyChangedEvents.Contains("IsStep2"));
        Assert.IsTrue(propertyChangedEvents.Contains("IsStep3"));
    }

    /// <summary>
    /// Tests that GoToStep3 correctly handles transition from step 1 (default initial state).
    /// Input: Default initialized RegisterViewModel with CurrentStep = 1.
    /// Expected: CurrentStep changes to 3, IsStep1 becomes false, IsStep3 becomes true.
    /// </summary>
    [TestMethod]
    public void GoToStep3_FromDefaultStep1_TransitionsCorrectly()
    {
        // Arrange
        var authServiceMock = new Mock<IAuthService>();
        var academicServiceMock = new Mock<IAcademicService>();
        var loggerMock = new Mock<ILogger<RegisterViewModel>>();
        var viewModel = new RegisterViewModel(authServiceMock.Object, academicServiceMock.Object, loggerMock.Object);

        Assert.AreEqual(1, viewModel.CurrentStep, "Precondition: Initial CurrentStep should be 1");
        Assert.IsTrue(viewModel.IsStep1, "Precondition: IsStep1 should be true initially");

        // Act
        viewModel.GoToStep3();

        // Assert
        Assert.AreEqual(3, viewModel.CurrentStep);
        Assert.IsFalse(viewModel.IsStep1);
        Assert.IsTrue(viewModel.IsStep3);
    }

    /// <summary>
    /// Tests that GoToStep3 correctly handles transition from step 2.
    /// Input: CurrentStep initially set to 2.
    /// Expected: CurrentStep changes to 3, IsStep2 becomes false, IsStep3 becomes true.
    /// </summary>
    [TestMethod]
    public void GoToStep3_FromStep2_TransitionsCorrectly()
    {
        // Arrange
        var authServiceMock = new Mock<IAuthService>();
        var academicServiceMock = new Mock<IAcademicService>();
        var loggerMock = new Mock<ILogger<RegisterViewModel>>();
        var viewModel = new RegisterViewModel(authServiceMock.Object, academicServiceMock.Object, loggerMock.Object);
        viewModel.CurrentStep = 2;

        Assert.AreEqual(2, viewModel.CurrentStep, "Precondition: CurrentStep should be 2");
        Assert.IsTrue(viewModel.IsStep2, "Precondition: IsStep2 should be true");

        // Act
        viewModel.GoToStep3();

        // Assert
        Assert.AreEqual(3, viewModel.CurrentStep);
        Assert.IsFalse(viewModel.IsStep2);
        Assert.IsTrue(viewModel.IsStep3);
    }

    /// <summary>
    /// Tests that GoToStep3 with negative initial CurrentStep value transitions correctly to step 3.
    /// Input: CurrentStep set to negative values.
    /// Expected: CurrentStep changes to 3 regardless of negative initial value.
    /// </summary>
    /// <param name="negativeStep">The negative value to set CurrentStep to initially.</param>
    [TestMethod]
    [DataRow(-1)]
    [DataRow(-10)]
    [DataRow(-100)]
    [DataRow(int.MinValue)]
    public void GoToStep3_FromNegativeStep_TransitionsTo3(int negativeStep)
    {
        // Arrange
        var authServiceMock = new Mock<IAuthService>();
        var academicServiceMock = new Mock<IAcademicService>();
        var loggerMock = new Mock<ILogger<RegisterViewModel>>();
        var viewModel = new RegisterViewModel(authServiceMock.Object, academicServiceMock.Object, loggerMock.Object);
        viewModel.CurrentStep = negativeStep;

        // Act
        viewModel.GoToStep3();

        // Assert
        Assert.AreEqual(3, viewModel.CurrentStep);
        Assert.IsTrue(viewModel.IsStep3);
    }

    /// <summary>
    /// Tests that GoToStep3 with large initial CurrentStep values transitions correctly to step 3.
    /// Input: CurrentStep set to large positive values.
    /// Expected: CurrentStep changes to 3 regardless of large initial value.
    /// </summary>
    /// <param name="largeStep">The large value to set CurrentStep to initially.</param>
    [TestMethod]
    [DataRow(10)]
    [DataRow(100)]
    [DataRow(1000)]
    [DataRow(int.MaxValue)]
    public void GoToStep3_FromLargeStep_TransitionsTo3(int largeStep)
    {
        // Arrange
        var authServiceMock = new Mock<IAuthService>();
        var academicServiceMock = new Mock<IAcademicService>();
        var loggerMock = new Mock<ILogger<RegisterViewModel>>();
        var viewModel = new RegisterViewModel(authServiceMock.Object, academicServiceMock.Object, loggerMock.Object);
        viewModel.CurrentStep = largeStep;

        // Act
        viewModel.GoToStep3();

        // Assert
        Assert.AreEqual(3, viewModel.CurrentStep);
        Assert.IsTrue(viewModel.IsStep3);
    }

    /// <summary>
    /// Tests that GoToStep3 with zero initial CurrentStep transitions correctly to step 3.
    /// Input: CurrentStep set to 0.
    /// Expected: CurrentStep changes to 3, all step indicators reflect step 3.
    /// </summary>
    [TestMethod]
    public void GoToStep3_FromZeroStep_TransitionsTo3()
    {
        // Arrange
        var authServiceMock = new Mock<IAuthService>();
        var academicServiceMock = new Mock<IAcademicService>();
        var loggerMock = new Mock<ILogger<RegisterViewModel>>();
        var viewModel = new RegisterViewModel(authServiceMock.Object, academicServiceMock.Object, loggerMock.Object);
        viewModel.CurrentStep = 0;

        // Act
        viewModel.GoToStep3();

        // Assert
        Assert.AreEqual(3, viewModel.CurrentStep);
        Assert.IsFalse(viewModel.IsStep1);
        Assert.IsFalse(viewModel.IsStep2);
        Assert.IsTrue(viewModel.IsStep3);
    }

    /// <summary>
    /// Tests that the OtherNames property returns an empty string as its initial value.
    /// </summary>
    [TestMethod]
    public void OtherNames_GetInitialValue_ReturnsEmptyString()
    {
        // Arrange
        var mockAuthService = new Mock<IAuthService>();
        var mockAcademicService = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<RegisterViewModel>>();
        var viewModel = new RegisterViewModel(mockAuthService.Object, mockAcademicService.Object, mockLogger.Object);

        // Act
        var result = viewModel.OtherNames;

        // Assert
        Assert.AreEqual(string.Empty, result);
    }

    /// <summary>
    /// Tests that setting the OtherNames property with various valid string values updates the property correctly.
    /// Tests normal values, empty strings, whitespace, special characters, and international names.
    /// </summary>
    /// <param name="value">The value to set on the OtherNames property.</param>
    [TestMethod]
    [DataRow("Michael")]
    [DataRow("John Paul")]
    [DataRow("")]
    [DataRow(" ")]
    [DataRow("   \t\n")]
    [DataRow("O'Connor")]
    [DataRow("Jean-Pierre")]
    [DataRow("María José")]
    [DataRow("李明华")]
    [DataRow("Müller")]
    [DataRow("!@#$%^&*()")]
    [DataRow("Name with special chars: é, ñ, ü")]
    [DataRow("abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789")]
    public void OtherNames_SetValidValue_UpdatesProperty(string value)
    {
        // Arrange
        var mockAuthService = new Mock<IAuthService>();
        var mockAcademicService = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<RegisterViewModel>>();
        var viewModel = new RegisterViewModel(mockAuthService.Object, mockAcademicService.Object, mockLogger.Object);

        // Act
        viewModel.OtherNames = value;

        // Assert
        Assert.AreEqual(value, viewModel.OtherNames);
    }

    /// <summary>
    /// Tests that setting the OtherNames property raises the PropertyChanged event with the correct property name.
    /// </summary>
    [TestMethod]
    public void OtherNames_SetValue_RaisesPropertyChangedEvent()
    {
        // Arrange
        var mockAuthService = new Mock<IAuthService>();
        var mockAcademicService = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<RegisterViewModel>>();
        var viewModel = new RegisterViewModel(mockAuthService.Object, mockAcademicService.Object, mockLogger.Object);
        string? changedPropertyName = null;
        viewModel.PropertyChanged += (sender, args) => changedPropertyName = args.PropertyName;

        // Act
        viewModel.OtherNames = "TestName";

        // Assert
        Assert.AreEqual("OtherNames", changedPropertyName);
    }

    /// <summary>
    /// Tests that setting the OtherNames property to the same value does not raise PropertyChanged event.
    /// This verifies that SetProperty correctly checks for value equality before raising the event.
    /// </summary>
    [TestMethod]
    public void OtherNames_SetSameValue_DoesNotRaisePropertyChangedEvent()
    {
        // Arrange
        var mockAuthService = new Mock<IAuthService>();
        var mockAcademicService = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<RegisterViewModel>>();
        var viewModel = new RegisterViewModel(mockAuthService.Object, mockAcademicService.Object, mockLogger.Object);
        viewModel.OtherNames = "SameName";
        int eventRaisedCount = 0;
        viewModel.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == "OtherNames")
                eventRaisedCount++;
        };

        // Act
        viewModel.OtherNames = "SameName";

        // Assert
        Assert.AreEqual(0, eventRaisedCount);
    }

    /// <summary>
    /// Tests that setting the OtherNames property multiple times with different values correctly updates the property each time.
    /// </summary>
    [TestMethod]
    public void OtherNames_SetMultipleDifferentValues_UpdatesPropertyEachTime()
    {
        // Arrange
        var mockAuthService = new Mock<IAuthService>();
        var mockAcademicService = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<RegisterViewModel>>();
        var viewModel = new RegisterViewModel(mockAuthService.Object, mockAcademicService.Object, mockLogger.Object);

        // Act & Assert
        viewModel.OtherNames = "First";
        Assert.AreEqual("First", viewModel.OtherNames);

        viewModel.OtherNames = "Second";
        Assert.AreEqual("Second", viewModel.OtherNames);

        viewModel.OtherNames = "Third";
        Assert.AreEqual("Third", viewModel.OtherNames);
    }

    /// <summary>
    /// Tests that setting the OtherNames property with control characters is handled correctly.
    /// </summary>
    [TestMethod]
    public void OtherNames_SetValueWithControlCharacters_UpdatesProperty()
    {
        // Arrange
        var mockAuthService = new Mock<IAuthService>();
        var mockAcademicService = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<RegisterViewModel>>();
        var viewModel = new RegisterViewModel(mockAuthService.Object, mockAcademicService.Object, mockLogger.Object);
        var valueWithControlChars = "Name\u0000With\u0001Control\u0002Chars";

        // Act
        viewModel.OtherNames = valueWithControlChars;

        // Assert
        Assert.AreEqual(valueWithControlChars, viewModel.OtherNames);
    }

    /// <summary>
    /// Tests that setting the OtherNames property with Unicode characters is handled correctly.
    /// </summary>
    [TestMethod]
    [DataRow("Müller")]
    [DataRow("Sánchez")]
    [DataRow("Østerberg")]
    [DataRow("Žigić")]
    [DataRow("中文名字")]
    [DataRow("日本語")]
    [DataRow("한글이름")]
    [DataRow("😀😁😂")]
    public void OtherNames_SetValueWithUnicodeCharacters_UpdatesProperty(string value)
    {
        // Arrange
        var mockAuthService = new Mock<IAuthService>();
        var mockAcademicService = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<RegisterViewModel>>();
        var viewModel = new RegisterViewModel(mockAuthService.Object, mockAcademicService.Object, mockLogger.Object);

        // Act
        viewModel.OtherNames = value;

        // Assert
        Assert.AreEqual(value, viewModel.OtherNames);
    }

    /// <summary>
    /// Tests that setting the OtherNames property with a very long string is handled correctly.
    /// </summary>
    [TestMethod]
    public void OtherNames_SetVeryLongString_UpdatesProperty()
    {
        // Arrange
        var mockAuthService = new Mock<IAuthService>();
        var mockAcademicService = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<RegisterViewModel>>();
        var viewModel = new RegisterViewModel(mockAuthService.Object, mockAcademicService.Object, mockLogger.Object);
        var veryLongString = new string('A', 10000);

        // Act
        viewModel.OtherNames = veryLongString;

        // Assert
        Assert.AreEqual(veryLongString, viewModel.OtherNames);
    }

    /// <summary>
    /// Tests that setting the OtherNames property with strings containing newlines and tabs is handled correctly.
    /// </summary>
    [TestMethod]
    [DataRow("Name\r\nWith\r\nNewlines")]
    [DataRow("Name\tWith\tTabs")]
    [DataRow("\r")]
    [DataRow("\n")]
    [DataRow("\r\n")]
    public void OtherNames_SetValueWithWhitespaceCharacters_UpdatesProperty(string value)
    {
        // Arrange
        var mockAuthService = new Mock<IAuthService>();
        var mockAcademicService = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<RegisterViewModel>>();
        var viewModel = new RegisterViewModel(mockAuthService.Object, mockAcademicService.Object, mockLogger.Object);

        // Act
        viewModel.OtherNames = value;

        // Assert
        Assert.AreEqual(value, viewModel.OtherNames);
    }

    /// <summary>
    /// Tests that setting the Password property with control characters is handled correctly.
    /// Input: Strings containing various control characters.
    /// Expected: Property returns the string with control characters.
    /// </summary>
    /// <param name="controlCharPassword">Password with control characters.</param>
    [TestMethod]
    [DataRow("pwd\u0000control")]
    [DataRow("pwd\u0001test")]
    [DataRow("pwd\u001Fchar")]
    [DataRow("pwd\bbackspace")]
    [DataRow("pwd\fformfeed")]
    public void Password_SetStringWithControlCharacters_ReturnsStringWithControlCharacters(string controlCharPassword)
    {
        // Arrange
        var viewModel = new RegisterViewModel(_authServiceMock.Object, _academicServiceMock.Object, _loggerMock.Object);

        // Act
        viewModel.Password = controlCharPassword;

        // Assert
        Assert.AreEqual(controlCharPassword, viewModel.Password);
    }

    /// <summary>
    /// Tests that setting Password multiple times with different values raises PropertyChanged event each time.
    /// Input: Sequential different password values.
    /// Expected: PropertyChanged event is raised for each change.
    /// </summary>
    [TestMethod]
    public void Password_SetMultipleDifferentValues_RaisesPropertyChangedEventEachTime()
    {
        // Arrange
        var viewModel = new RegisterViewModel(_authServiceMock.Object, _academicServiceMock.Object, _loggerMock.Object);
        int eventRaiseCount = 0;
        viewModel.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == "Password")
                eventRaiseCount++;
        };

        // Act
        viewModel.Password = "first";
        viewModel.Password = "second";
        viewModel.Password = "third";

        // Assert
        Assert.AreEqual(3, eventRaiseCount);
    }

    /// <summary>
    /// Tests that Password property handles strings with mixed character types correctly.
    /// Input: Passwords with alphanumeric, special characters, and unicode mixed together.
    /// Expected: Property returns the exact string.
    /// </summary>
    /// <param name="mixedPassword">Password with mixed character types.</param>
    [TestMethod]
    [DataRow("Abc123!@#")]
    [DataRow("Test123パスワード")]
    [DataRow("混合Password123!")]
    [DataRow("Ñoño123!@#$")]
    public void Password_SetStringWithMixedCharacters_ReturnsStringWithMixedCharacters(string mixedPassword)
    {
        // Arrange
        var viewModel = new RegisterViewModel(_authServiceMock.Object, _academicServiceMock.Object, _loggerMock.Object);

        // Act
        viewModel.Password = mixedPassword;

        // Assert
        Assert.AreEqual(mixedPassword, viewModel.Password);
    }

    /// <summary>
    /// Tests that setting Password from empty to non-empty value raises PropertyChanged.
    /// Input: Empty string followed by a non-empty password.
    /// Expected: PropertyChanged event is raised.
    /// </summary>
    [TestMethod]
    public void Password_SetFromEmptyToNonEmpty_RaisesPropertyChangedEvent()
    {
        // Arrange
        var viewModel = new RegisterViewModel(_authServiceMock.Object, _academicServiceMock.Object, _loggerMock.Object);
        viewModel.Password = "";
        string? raisedPropertyName = null;
        viewModel.PropertyChanged += (sender, args) => raisedPropertyName = args.PropertyName;

        // Act
        viewModel.Password = "newPassword";

        // Assert
        Assert.AreEqual("Password", raisedPropertyName);
    }

    /// <summary>
    /// Tests that setting Password from non-empty to empty value raises PropertyChanged.
    /// Input: Non-empty password followed by empty string.
    /// Expected: PropertyChanged event is raised.
    /// </summary>
    [TestMethod]
    public void Password_SetFromNonEmptyToEmpty_RaisesPropertyChangedEvent()
    {
        // Arrange
        var viewModel = new RegisterViewModel(_authServiceMock.Object, _academicServiceMock.Object, _loggerMock.Object);
        viewModel.Password = "existingPassword";
        string? raisedPropertyName = null;
        viewModel.PropertyChanged += (sender, args) => raisedPropertyName = args.PropertyName;

        // Act
        viewModel.Password = "";

        // Assert
        Assert.AreEqual("Password", raisedPropertyName);
    }

    /// <summary>
    /// Tests that setting SelectedProgram to a new value raises PropertyChanged event.
    /// Input: Valid LookupItem instance.
    /// Expected: PropertyChanged event is raised with "SelectedProgram" as property name.
    /// </summary>
    [TestMethod]
    public void SelectedProgram_SetNewValue_RaisesPropertyChangedEvent()
    {
        // Arrange
        var mockAuthService = new Mock<IAuthService>();
        var mockAcademicService = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<RegisterViewModel>>();
        var viewModel = new RegisterViewModel(mockAuthService.Object, mockAcademicService.Object, mockLogger.Object);
        var lookupItem = new LookupItem { Id = "prog-001", Name = "Computer Science" };
        string? propertyName = null;
        viewModel.PropertyChanged += (sender, args) => propertyName = args.PropertyName;

        // Act
        viewModel.SelectedProgram = lookupItem;

        // Assert
        Assert.AreEqual("SelectedProgram", propertyName);
    }

    /// <summary>
    /// Tests that setting SelectedProgram to a LookupItem with empty string properties works correctly.
    /// Input: LookupItem with empty Id and empty Name.
    /// Expected: Property stores the LookupItem with empty strings correctly.
    /// </summary>
    [TestMethod]
    public void SelectedProgram_SetLookupItemWithEmptyStrings_StoresValueCorrectly()
    {
        // Arrange
        var mockAuthService = new Mock<IAuthService>();
        var mockAcademicService = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<RegisterViewModel>>();
        var viewModel = new RegisterViewModel(mockAuthService.Object, mockAcademicService.Object, mockLogger.Object);
        var lookupItem = new LookupItem { Id = "", Name = "" };

        // Act
        viewModel.SelectedProgram = lookupItem;

        // Assert
        Assert.AreEqual(lookupItem, viewModel.SelectedProgram);
        Assert.AreEqual("", viewModel.SelectedProgram.Id);
        Assert.AreEqual("", viewModel.SelectedProgram.Name);
    }

    /// <summary>
    /// Tests that setting SelectedProgram from non-null to null raises PropertyChanged event.
    /// Input: First a valid LookupItem, then null.
    /// Expected: PropertyChanged event is raised for both changes.
    /// </summary>
    [TestMethod]
    public void SelectedProgram_SetFromNonNullToNull_RaisesPropertyChangedEvent()
    {
        // Arrange
        var mockAuthService = new Mock<IAuthService>();
        var mockAcademicService = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<RegisterViewModel>>();
        var viewModel = new RegisterViewModel(mockAuthService.Object, mockAcademicService.Object, mockLogger.Object);
        var lookupItem = new LookupItem { Id = "prog-001", Name = "Computer Science" };
        int eventRaisedCount = 0;
        viewModel.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == "SelectedProgram")
                eventRaisedCount++;
        };

        // Act
        viewModel.SelectedProgram = lookupItem;
        viewModel.SelectedProgram = null;

        // Assert
        Assert.AreEqual(2, eventRaisedCount);
        Assert.IsNull(viewModel.SelectedProgram);
    }

    /// <summary>
    /// Tests that setting SelectedProgram to null twice does not raise PropertyChanged event on the second set.
    /// Input: null set twice consecutively.
    /// Expected: PropertyChanged event is not raised (property starts as null, setting to null again doesn't change it).
    /// </summary>
    [TestMethod]
    public void SelectedProgram_SetNullTwice_DoesNotRaisePropertyChangedEvent()
    {
        // Arrange
        var mockAuthService = new Mock<IAuthService>();
        var mockAcademicService = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<RegisterViewModel>>();
        var viewModel = new RegisterViewModel(mockAuthService.Object, mockAcademicService.Object, mockLogger.Object);
        int eventRaisedCount = 0;
        viewModel.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == "SelectedProgram")
                eventRaisedCount++;
        };

        // Act
        viewModel.SelectedProgram = null;
        viewModel.SelectedProgram = null;

        // Assert
        Assert.AreEqual(0, eventRaisedCount);
        Assert.IsNull(viewModel.SelectedProgram);
    }

    /// <summary>
    /// Tests that setting SelectedProgram from null to a valid value raises PropertyChanged event.
    /// Input: Starting from null (default state), set to valid LookupItem.
    /// Expected: PropertyChanged event is raised once.
    /// </summary>
    [TestMethod]
    public void SelectedProgram_SetFromNullToNonNull_RaisesPropertyChangedEvent()
    {
        // Arrange
        var mockAuthService = new Mock<IAuthService>();
        var mockAcademicService = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<RegisterViewModel>>();
        var viewModel = new RegisterViewModel(mockAuthService.Object, mockAcademicService.Object, mockLogger.Object);
        var lookupItem = new LookupItem { Id = "prog-001", Name = "Computer Science" };
        int eventRaisedCount = 0;
        viewModel.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == "SelectedProgram")
                eventRaisedCount++;
        };

        // Act
        viewModel.SelectedProgram = lookupItem;

        // Assert
        Assert.AreEqual(1, eventRaisedCount);
        Assert.AreEqual(lookupItem, viewModel.SelectedProgram);
    }

    /// <summary>
    /// Tests that setting SelectedProgram with various LookupItem property values works correctly.
    /// Input: LookupItem instances with edge case values including whitespace, special characters, and very long strings.
    /// Expected: Property stores each value correctly and raises PropertyChanged for each distinct change.
    /// </summary>
    [TestMethod]
    [DataRow("", "", DisplayName = "Empty Id and Name")]
    [DataRow("prog-001", "", DisplayName = "Valid Id, empty Name")]
    [DataRow("", "Computer Science", DisplayName = "Empty Id, valid Name")]
    [DataRow(" ", " ", DisplayName = "Whitespace Id and Name")]
    [DataRow("prog-with-special-chars!@#", "Name with symbols $%^&*()", DisplayName = "Special characters")]
    [DataRow("very-long-id-abcdefghijklmnopqrstuvwxyz0123456789", "Very Long Name That Contains Many Characters To Test Boundary Conditions", DisplayName = "Very long strings")]
    public void SelectedProgram_SetWithVariousLookupItemValues_StoresValueCorrectly(string id, string name)
    {
        // Arrange
        var mockAuthService = new Mock<IAuthService>();
        var mockAcademicService = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<RegisterViewModel>>();
        var viewModel = new RegisterViewModel(mockAuthService.Object, mockAcademicService.Object, mockLogger.Object);
        var lookupItem = new LookupItem { Id = id, Name = name };

        // Act
        viewModel.SelectedProgram = lookupItem;

        // Assert
        Assert.AreEqual(lookupItem, viewModel.SelectedProgram);
        Assert.AreEqual(id, viewModel.SelectedProgram.Id);
        Assert.AreEqual(name, viewModel.SelectedProgram.Name);
    }

    /// <summary>
    /// Tests that setting SelectedProgram with different LookupItem instances having identical property values raises PropertyChanged.
    /// Input: Two different LookupItem instances with same Id and Name values.
    /// Expected: PropertyChanged is raised for both sets because instances are different (reference equality).
    /// </summary>
    [TestMethod]
    public void SelectedProgram_SetDifferentInstancesWithSameValues_RaisesPropertyChangedForEach()
    {
        // Arrange
        var mockAuthService = new Mock<IAuthService>();
        var mockAcademicService = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<RegisterViewModel>>();
        var viewModel = new RegisterViewModel(mockAuthService.Object, mockAcademicService.Object, mockLogger.Object);
        var lookupItem1 = new LookupItem { Id = "prog-001", Name = "Computer Science" };
        var lookupItem2 = new LookupItem { Id = "prog-001", Name = "Computer Science" };
        int eventRaisedCount = 0;
        viewModel.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == "SelectedProgram")
                eventRaisedCount++;
        };

        // Act
        viewModel.SelectedProgram = lookupItem1;
        viewModel.SelectedProgram = lookupItem2;

        // Assert
        Assert.AreEqual(2, eventRaisedCount);
        Assert.AreEqual(lookupItem2, viewModel.SelectedProgram);
    }

    /// <summary>
    /// Tests that the ErrorMessage property returns empty string as the default initial value.
    /// Input: None (testing initial state).
    /// Expected: ErrorMessage property returns empty string.
    /// </summary>
    [TestMethod]
    public void ErrorMessage_InitialValue_ReturnsEmptyString()
    {
        // Arrange
        Mock<IAuthService> mockAuthService = new Mock<IAuthService>();
        Mock<IAcademicService> mockAcademicService = new Mock<IAcademicService>();
        Mock<ILogger<RegisterViewModel>> mockLogger = new Mock<ILogger<RegisterViewModel>>();
        RegisterViewModel viewModel = new RegisterViewModel(mockAuthService.Object, mockAcademicService.Object, mockLogger.Object);

        // Act
        string result = viewModel.ErrorMessage;

        // Assert
        Assert.AreEqual(string.Empty, result);
    }

    /// <summary>
    /// Tests that setting the ErrorMessage property with various string values updates the property correctly.
    /// Input: Various valid string values including empty, whitespace, normal text, and special characters.
    /// Expected: Property value is updated to the set value.
    /// </summary>
    /// <param name="testValue">The value to set on the ErrorMessage property.</param>
    [TestMethod]
    [DataRow("")]
    [DataRow("Error occurred")]
    [DataRow("Invalid email format")]
    [DataRow("Connection failed")]
    [DataRow(" ")]
    [DataRow("   ")]
    [DataRow("\t")]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [DataRow("Error: !@#$%^&*()")]
    [DataRow("Error with unicode: 你好世界")]
    [DataRow("Error with emoji: 😀😁😂")]
    [DataRow("Error\x00with\x01control\x02chars")]
    [DataRow("A")]
    [DataRow("Error message with special chars: é, ñ, ü, ç")]
    public void ErrorMessage_SetValue_UpdatesPropertyCorrectly(string testValue)
    {
        // Arrange
        Mock<IAuthService> mockAuthService = new Mock<IAuthService>();
        Mock<IAcademicService> mockAcademicService = new Mock<IAcademicService>();
        Mock<ILogger<RegisterViewModel>> mockLogger = new Mock<ILogger<RegisterViewModel>>();
        RegisterViewModel viewModel = new RegisterViewModel(mockAuthService.Object, mockAcademicService.Object, mockLogger.Object);

        // Act
        viewModel.ErrorMessage = testValue;

        // Assert
        Assert.AreEqual(testValue, viewModel.ErrorMessage);
    }

    /// <summary>
    /// Tests that setting the ErrorMessage property with a very long string updates the property correctly.
    /// Input: String with 10000 characters.
    /// Expected: Property value is updated to the very long string.
    /// </summary>
    [TestMethod]
    public void ErrorMessage_SetVeryLongString_UpdatesPropertyCorrectly()
    {
        // Arrange
        Mock<IAuthService> mockAuthService = new Mock<IAuthService>();
        Mock<IAcademicService> mockAcademicService = new Mock<IAcademicService>();
        Mock<ILogger<RegisterViewModel>> mockLogger = new Mock<ILogger<RegisterViewModel>>();
        RegisterViewModel viewModel = new RegisterViewModel(mockAuthService.Object, mockAcademicService.Object, mockLogger.Object);
        string veryLongString = new string('x', 10000);

        // Act
        viewModel.ErrorMessage = veryLongString;

        // Assert
        Assert.AreEqual(veryLongString, viewModel.ErrorMessage);
    }

    /// <summary>
    /// Tests that setting the ErrorMessage property raises the PropertyChanged event with the correct property name.
    /// Input: A new string value different from the current value.
    /// Expected: PropertyChanged event is raised with PropertyName = "ErrorMessage".
    /// </summary>
    [TestMethod]
    public void ErrorMessage_SetNewValue_RaisesPropertyChangedEvent()
    {
        // Arrange
        Mock<IAuthService> mockAuthService = new Mock<IAuthService>();
        Mock<IAcademicService> mockAcademicService = new Mock<IAcademicService>();
        Mock<ILogger<RegisterViewModel>> mockLogger = new Mock<ILogger<RegisterViewModel>>();
        RegisterViewModel viewModel = new RegisterViewModel(mockAuthService.Object, mockAcademicService.Object, mockLogger.Object);
        string? raisedPropertyName = null;
        viewModel.PropertyChanged += (sender, e) => raisedPropertyName = e.PropertyName;

        // Act
        viewModel.ErrorMessage = "New error message";

        // Assert
        Assert.AreEqual("ErrorMessage", raisedPropertyName);
    }

    /// <summary>
    /// Tests that setting the ErrorMessage property to the same value does not raise PropertyChanged event.
    /// Input: Same string value set twice.
    /// Expected: PropertyChanged event is raised only once (on the first set).
    /// </summary>
    [TestMethod]
    public void ErrorMessage_SetSameValue_DoesNotRaisePropertyChangedEvent()
    {
        // Arrange
        Mock<IAuthService> mockAuthService = new Mock<IAuthService>();
        Mock<IAcademicService> mockAcademicService = new Mock<IAcademicService>();
        Mock<ILogger<RegisterViewModel>> mockLogger = new Mock<ILogger<RegisterViewModel>>();
        RegisterViewModel viewModel = new RegisterViewModel(mockAuthService.Object, mockAcademicService.Object, mockLogger.Object);
        string testValue = "Test error message";
        viewModel.ErrorMessage = testValue;

        int eventRaisedCount = 0;
        viewModel.PropertyChanged += (sender, e) => { if (e.PropertyName == "ErrorMessage") eventRaisedCount++; };

        // Act
        viewModel.ErrorMessage = testValue;

        // Assert
        Assert.AreEqual(0, eventRaisedCount);
    }

    /// <summary>
    /// Tests that setting the ErrorMessage property multiple times with different values updates correctly each time.
    /// Input: Multiple different string values set sequentially.
    /// Expected: Property value is updated to the last set value and PropertyChanged is raised for each change.
    /// </summary>
    [TestMethod]
    public void ErrorMessage_SetMultipleDifferentValues_UpdatesCorrectlyEachTime()
    {
        // Arrange
        Mock<IAuthService> mockAuthService = new Mock<IAuthService>();
        Mock<IAcademicService> mockAcademicService = new Mock<IAcademicService>();
        Mock<ILogger<RegisterViewModel>> mockLogger = new Mock<ILogger<RegisterViewModel>>();
        RegisterViewModel viewModel = new RegisterViewModel(mockAuthService.Object, mockAcademicService.Object, mockLogger.Object);
        int eventRaisedCount = 0;
        viewModel.PropertyChanged += (sender, e) => { if (e.PropertyName == "ErrorMessage") eventRaisedCount++; };

        // Act
        viewModel.ErrorMessage = "First error";
        viewModel.ErrorMessage = "Second error";
        viewModel.ErrorMessage = "Third error";

        // Assert
        Assert.AreEqual("Third error", viewModel.ErrorMessage);
        Assert.AreEqual(3, eventRaisedCount);
    }

    /// <summary>
    /// Tests that setting the ErrorMessage property handles null value at runtime.
    /// Input: null value (runtime scenario despite non-nullable annotation).
    /// Expected: Property value is set to null without throwing exception.
    /// </summary>
    [TestMethod]
    public void ErrorMessage_SetNull_HandlesNullValue()
    {
        // Arrange
        Mock<IAuthService> mockAuthService = new Mock<IAuthService>();
        Mock<IAcademicService> mockAcademicService = new Mock<IAcademicService>();
        Mock<ILogger<RegisterViewModel>> mockLogger = new Mock<ILogger<RegisterViewModel>>();
        RegisterViewModel viewModel = new RegisterViewModel(mockAuthService.Object, mockAcademicService.Object, mockLogger.Object);

        // Act
        viewModel.ErrorMessage = null!;

        // Assert
        Assert.IsNull(viewModel.ErrorMessage);
    }

    /// <summary>
    /// Tests that setting ErrorMessage to empty string when already empty does not raise PropertyChanged event.
    /// Input: Empty string set twice consecutively.
    /// Expected: PropertyChanged event is not raised on the second set.
    /// </summary>
    [TestMethod]
    public void ErrorMessage_SetEmptyStringWhenAlreadyEmpty_DoesNotRaisePropertyChanged()
    {
        // Arrange
        Mock<IAuthService> mockAuthService = new Mock<IAuthService>();
        Mock<IAcademicService> mockAcademicService = new Mock<IAcademicService>();
        Mock<ILogger<RegisterViewModel>> mockLogger = new Mock<ILogger<RegisterViewModel>>();
        RegisterViewModel viewModel = new RegisterViewModel(mockAuthService.Object, mockAcademicService.Object, mockLogger.Object);

        int eventRaisedCount = 0;
        viewModel.PropertyChanged += (sender, e) => { if (e.PropertyName == "ErrorMessage") eventRaisedCount++; };

        // Act
        viewModel.ErrorMessage = string.Empty;

        // Assert
        Assert.AreEqual(0, eventRaisedCount);
    }

    /// <summary>
    /// Tests that ErrorMessage property handles strings with mixed whitespace and special characters correctly.
    /// Input: Strings with various combinations of whitespace and special characters.
    /// Expected: Property value is updated correctly.
    /// </summary>
    /// <param name="testValue">The value to set on the ErrorMessage property.</param>
    [TestMethod]
    [DataRow(" \t Error \n ")]
    [DataRow("\r\n\t   Error message   \t\r\n")]
    [DataRow("Error: <script>alert('xss')</script>")]
    [DataRow("Error: SQL'; DROP TABLE Users;--")]
    [DataRow("Error\u0000\u0001\u0002\u0003")]
    public void ErrorMessage_SetMixedWhitespaceAndSpecialChars_UpdatesPropertyCorrectly(string testValue)
    {
        // Arrange
        Mock<IAuthService> mockAuthService = new Mock<IAuthService>();
        Mock<IAcademicService> mockAcademicService = new Mock<IAcademicService>();
        Mock<ILogger<RegisterViewModel>> mockLogger = new Mock<ILogger<RegisterViewModel>>();
        RegisterViewModel viewModel = new RegisterViewModel(mockAuthService.Object, mockAcademicService.Object, mockLogger.Object);

        // Act
        viewModel.ErrorMessage = testValue;

        // Assert
        Assert.AreEqual(testValue, viewModel.ErrorMessage);
    }

    /// <summary>
    /// Tests that setting ErrorMessage from empty to non-empty and back to empty works correctly.
    /// Input: Empty string, then non-empty string, then empty string again.
    /// Expected: Property value is updated correctly each time and PropertyChanged is raised for each change.
    /// </summary>
    [TestMethod]
    public void ErrorMessage_SetEmptyToNonEmptyToEmpty_UpdatesCorrectlyEachTime()
    {
        // Arrange
        Mock<IAuthService> mockAuthService = new Mock<IAuthService>();
        Mock<IAcademicService> mockAcademicService = new Mock<IAcademicService>();
        Mock<ILogger<RegisterViewModel>> mockLogger = new Mock<ILogger<RegisterViewModel>>();
        RegisterViewModel viewModel = new RegisterViewModel(mockAuthService.Object, mockAcademicService.Object, mockLogger.Object);
        int eventRaisedCount = 0;
        viewModel.PropertyChanged += (sender, e) => { if (e.PropertyName == "ErrorMessage") eventRaisedCount++; };

        // Act & Assert
        Assert.AreEqual(string.Empty, viewModel.ErrorMessage);

        viewModel.ErrorMessage = "Error occurred";
        Assert.AreEqual("Error occurred", viewModel.ErrorMessage);
        Assert.AreEqual(1, eventRaisedCount);

        viewModel.ErrorMessage = string.Empty;
        Assert.AreEqual(string.Empty, viewModel.ErrorMessage);
        Assert.AreEqual(2, eventRaisedCount);
    }

    /// <summary>
    /// Tests that ErrorMessage property handles maximum length strings at boundary conditions.
    /// Input: Strings with lengths at various boundary values.
    /// Expected: Property value is updated correctly for all boundary lengths.
    /// </summary>
    /// <param name="length">The length of the string to test.</param>
    [TestMethod]
    [DataRow(0)]
    [DataRow(1)]
    [DataRow(255)]
    [DataRow(256)]
    [DataRow(1000)]
    [DataRow(5000)]
    [DataRow(10000)]
    [DataRow(50000)]
    public void ErrorMessage_SetStringWithVariousLengths_UpdatesPropertyCorrectly(int length)
    {
        // Arrange
        Mock<IAuthService> mockAuthService = new Mock<IAuthService>();
        Mock<IAcademicService> mockAcademicService = new Mock<IAcademicService>();
        Mock<ILogger<RegisterViewModel>> mockLogger = new Mock<ILogger<RegisterViewModel>>();
        RegisterViewModel viewModel = new RegisterViewModel(mockAuthService.Object, mockAcademicService.Object, mockLogger.Object);
        string testValue = new string('E', length);

        // Act
        viewModel.ErrorMessage = testValue;

        // Assert
        Assert.AreEqual(testValue, viewModel.ErrorMessage);
        Assert.AreEqual(length, viewModel.ErrorMessage.Length);
    }

    /// <summary>
    /// Tests that IsStep1 returns true when the RegisterViewModel is first instantiated,
    /// as CurrentStep is initialized to 1.
    /// </summary>
    [TestMethod]
    public void IsStep1_InitialState_ReturnsTrue()
    {
        // Arrange
        var mockAuthService = new Mock<IAuthService>();
        var mockAcademicService = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<RegisterViewModel>>();

        // Act
        var viewModel = new RegisterViewModel(
            mockAuthService.Object,
            mockAcademicService.Object,
            mockLogger.Object);

        // Assert
        Assert.IsTrue(viewModel.IsStep1);
    }

    /// <summary>
    /// Tests that IsStep1 immediately reflects changes when CurrentStep is modified.
    /// Verifies that IsStep1 transitions correctly from true to false and back to true.
    /// </summary>
    [TestMethod]
    public void IsStep1_WhenCurrentStepChanges_ImmediatelyReflectsNewValue()
    {
        // Arrange
        var mockAuthService = new Mock<IAuthService>();
        var mockAcademicService = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<RegisterViewModel>>();

        var viewModel = new RegisterViewModel(
            mockAuthService.Object,
            mockAcademicService.Object,
            mockLogger.Object);

        // Act & Assert - Initial state
        Assert.IsTrue(viewModel.IsStep1);

        // Act & Assert - Change to step 2
        viewModel.CurrentStep = 2;
        Assert.IsFalse(viewModel.IsStep1);

        // Act & Assert - Change to step 3
        viewModel.CurrentStep = 3;
        Assert.IsFalse(viewModel.IsStep1);

        // Act & Assert - Change back to step 1
        viewModel.CurrentStep = 1;
        Assert.IsTrue(viewModel.IsStep1);
    }

    /// <summary>
    /// Tests that IsStep1 is idempotent - multiple consecutive reads return the same value
    /// without side effects when CurrentStep remains unchanged.
    /// </summary>
    [TestMethod]
    public void IsStep1_MultipleConsecutiveReads_ReturnsSameValue()
    {
        // Arrange
        var mockAuthService = new Mock<IAuthService>();
        var mockAcademicService = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<RegisterViewModel>>();

        var viewModel = new RegisterViewModel(
            mockAuthService.Object,
            mockAcademicService.Object,
            mockLogger.Object);

        viewModel.CurrentStep = 1;

        // Act
        bool firstRead = viewModel.IsStep1;
        bool secondRead = viewModel.IsStep1;
        bool thirdRead = viewModel.IsStep1;

        // Assert
        Assert.IsTrue(firstRead);
        Assert.IsTrue(secondRead);
        Assert.IsTrue(thirdRead);
    }

    /// <summary>
    /// Tests that IsStep1 returns the correct boolean value for various CurrentStep values,
    /// including boundary values and edge cases.
    /// </summary>
    /// <param name="currentStepValue">The value to set for CurrentStep.</param>
    /// <param name="expectedIsStep1">The expected return value of IsStep1.</param>
    [TestMethod]
    [DataRow(1, true, DisplayName = "CurrentStep equals 1 returns true")]
    [DataRow(0, false, DisplayName = "CurrentStep equals 0 returns false")]
    [DataRow(2, false, DisplayName = "CurrentStep equals 2 returns false")]
    [DataRow(3, false, DisplayName = "CurrentStep equals 3 returns false")]
    [DataRow(4, false, DisplayName = "CurrentStep equals 4 returns false")]
    [DataRow(-1, false, DisplayName = "CurrentStep equals -1 returns false")]
    [DataRow(-2, false, DisplayName = "CurrentStep equals -2 returns false")]
    [DataRow(-100, false, DisplayName = "CurrentStep equals -100 returns false")]
    [DataRow(100, false, DisplayName = "CurrentStep equals 100 returns false")]
    [DataRow(int.MinValue, false, DisplayName = "CurrentStep equals int.MinValue returns false")]
    [DataRow(int.MaxValue, false, DisplayName = "CurrentStep equals int.MaxValue returns false")]
    public void IsStep1_VariousCurrentStepValues_ReturnsExpectedResult(int currentStepValue, bool expectedIsStep1)
    {
        // Arrange
        var mockAuthService = new Mock<IAuthService>();
        var mockAcademicService = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<RegisterViewModel>>();

        var viewModel = new RegisterViewModel(
            mockAuthService.Object,
            mockAcademicService.Object,
            mockLogger.Object);

        // Act
        viewModel.CurrentStep = currentStepValue;
        bool result = viewModel.IsStep1;

        // Assert
        Assert.AreEqual(expectedIsStep1, result);
    }

    /// <summary>
    /// Tests that IsStep1 returns false when CurrentStep is set to adjacent values around 1.
    /// Verifies correct behavior for values immediately before and after 1.
    /// </summary>
    [TestMethod]
    [DataRow(0, DisplayName = "CurrentStep equals 0 (one less than 1)")]
    [DataRow(2, DisplayName = "CurrentStep equals 2 (one more than 1)")]
    public void IsStep1_AdjacentValuesToOne_ReturnsFalse(int adjacentValue)
    {
        // Arrange
        var mockAuthService = new Mock<IAuthService>();
        var mockAcademicService = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<RegisterViewModel>>();

        var viewModel = new RegisterViewModel(
            mockAuthService.Object,
            mockAcademicService.Object,
            mockLogger.Object);

        // Act
        viewModel.CurrentStep = adjacentValue;

        // Assert
        Assert.IsFalse(viewModel.IsStep1);
    }

    /// <summary>
    /// Tests that IsStep1 correctly evaluates to false when CurrentStep is set to
    /// other valid step values (2 and 3), corresponding to IsStep2 and IsStep3.
    /// </summary>
    [TestMethod]
    [DataRow(2, DisplayName = "CurrentStep is step 2")]
    [DataRow(3, DisplayName = "CurrentStep is step 3")]
    public void IsStep1_OtherValidStepValues_ReturnsFalse(int stepValue)
    {
        // Arrange
        var mockAuthService = new Mock<IAuthService>();
        var mockAcademicService = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<RegisterViewModel>>();

        var viewModel = new RegisterViewModel(
            mockAuthService.Object,
            mockAcademicService.Object,
            mockLogger.Object);

        // Act
        viewModel.CurrentStep = stepValue;

        // Assert
        Assert.IsFalse(viewModel.IsStep1);
    }

    /// <summary>
    /// Tests that OtpCode property handles strings with control characters correctly.
    /// Input: Strings containing control characters.
    /// Expected: Property stores the value without errors.
    /// </summary>
    [TestMethod]
    [DataRow("\u0000", DisplayName = "Null character")]
    [DataRow("\u0001", DisplayName = "Start of heading")]
    [DataRow("\u001F", DisplayName = "Unit separator")]
    [DataRow("123\u0000456", DisplayName = "OTP with embedded null")]
    public void OtpCode_SetValueWithControlCharacters_UpdatesPropertyCorrectly(string value)
    {
        // Arrange
        var authMock = new Mock<IAuthService>();
        var academicMock = new Mock<IAcademicService>();
        var loggerMock = new Mock<ILogger<RegisterViewModel>>();
        var viewModel = new RegisterViewModel(authMock.Object, academicMock.Object, loggerMock.Object);

        // Act
        viewModel.OtpCode = value;

        // Assert
        Assert.AreEqual(value, viewModel.OtpCode);
    }

    /// <summary>
    /// Tests that setting OtpCode to empty string when already empty does not raise PropertyChanged.
    /// Input: Empty string set twice consecutively.
    /// Expected: PropertyChanged is not raised on second set.
    /// </summary>
    [TestMethod]
    public void OtpCode_SetEmptyWhenAlreadyEmpty_DoesNotRaisePropertyChanged()
    {
        // Arrange
        var authMock = new Mock<IAuthService>();
        var academicMock = new Mock<IAcademicService>();
        var loggerMock = new Mock<ILogger<RegisterViewModel>>();
        var viewModel = new RegisterViewModel(authMock.Object, academicMock.Object, loggerMock.Object);

        var propertyChangedCount = 0;
        viewModel.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == "OtpCode")
            {
                propertyChangedCount++;
            }
        };

        // Act
        viewModel.OtpCode = string.Empty;

        // Assert
        Assert.AreEqual(0, propertyChangedCount);
        Assert.AreEqual(string.Empty, viewModel.OtpCode);
    }

    /// <summary>
    /// Tests that OtpCode handles boundary cases for typical OTP code lengths.
    /// Input: OTP codes of varying lengths (4, 6, 8 digits).
    /// Expected: All values are stored correctly.
    /// </summary>
    /// <param name="value">The OTP code to test.</param>
    [TestMethod]
    [DataRow("1234", DisplayName = "4-digit OTP")]
    [DataRow("123456", DisplayName = "6-digit OTP")]
    [DataRow("12345678", DisplayName = "8-digit OTP")]
    [DataRow("1", DisplayName = "Single digit")]
    [DataRow("12", DisplayName = "2-digit")]
    public void OtpCode_SetVariousLengthCodes_UpdatesPropertyCorrectly(string value)
    {
        // Arrange
        var authMock = new Mock<IAuthService>();
        var academicMock = new Mock<IAcademicService>();
        var loggerMock = new Mock<ILogger<RegisterViewModel>>();
        var viewModel = new RegisterViewModel(authMock.Object, academicMock.Object, loggerMock.Object);

        // Act
        viewModel.OtpCode = value;

        // Assert
        Assert.AreEqual(value, viewModel.OtpCode);
    }

    /// <summary>
    /// Tests that setting SelectedIntake with a LookupItem containing very long strings stores the value correctly.
    /// Input: LookupItem with very long Id and Name strings (10000 characters each).
    /// Expected: Property is updated and PropertyChanged event is raised.
    /// </summary>
    [TestMethod]
    public void SelectedIntake_SetLookupItemWithVeryLongStrings_StoresAndReturnsCorrectly()
    {
        // Arrange
        var mockAuthService = new Mock<IAuthService>();
        var mockAcademicService = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<RegisterViewModel>>();
        var viewModel = new RegisterViewModel(mockAuthService.Object, mockAcademicService.Object, mockLogger.Object);
        var veryLongString = new string('A', 10000);
        var lookupItem = new LookupItem { Id = veryLongString, Name = veryLongString };
        string? raisedPropertyName = null;
        viewModel.PropertyChanged += (sender, args) => raisedPropertyName = args.PropertyName;

        // Act
        viewModel.SelectedIntake = lookupItem;

        // Assert
        Assert.AreEqual(lookupItem, viewModel.SelectedIntake);
        Assert.AreEqual("SelectedIntake", raisedPropertyName);
    }

    /// <summary>
    /// Tests that setting SelectedIntake with a LookupItem containing special characters handles correctly.
    /// Input: LookupItem with special characters in Id and Name.
    /// Expected: Property is updated and stores the special characters correctly.
    /// </summary>
    [TestMethod]
    [DataRow("!@#$%^&*()", "Special!@#$")]
    [DataRow("<script>alert('xss')</script>", "Test<>Name")]
    [DataRow("ID-with-dashes", "Name with spaces")]
    [DataRow("id_with_underscores", "Name_With_Underscores")]
    [DataRow("123456789", "987654321")]
    public void SelectedIntake_SetLookupItemWithSpecialCharacters_StoresCorrectly(string id, string name)
    {
        // Arrange
        var mockAuthService = new Mock<IAuthService>();
        var mockAcademicService = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<RegisterViewModel>>();
        var viewModel = new RegisterViewModel(mockAuthService.Object, mockAcademicService.Object, mockLogger.Object);
        var lookupItem = new LookupItem { Id = id, Name = name };

        // Act
        viewModel.SelectedIntake = lookupItem;

        // Assert
        Assert.AreEqual(lookupItem, viewModel.SelectedIntake);
        Assert.AreEqual(id, viewModel.SelectedIntake.Id);
        Assert.AreEqual(name, viewModel.SelectedIntake.Name);
    }

    /// <summary>
    /// Tests that setting SelectedIntake with a LookupItem containing whitespace-only strings stores correctly.
    /// Input: LookupItem with whitespace-only Id and Name.
    /// Expected: Property is updated with whitespace values.
    /// </summary>
    [TestMethod]
    [DataRow("   ", "   ")]
    [DataRow("\t", "\t")]
    [DataRow("\n", "\n")]
    [DataRow(" \t\n ", " \t\n ")]
    public void SelectedIntake_SetLookupItemWithWhitespaceStrings_StoresCorrectly(string id, string name)
    {
        // Arrange
        var mockAuthService = new Mock<IAuthService>();
        var mockAcademicService = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<RegisterViewModel>>();
        var viewModel = new RegisterViewModel(mockAuthService.Object, mockAcademicService.Object, mockLogger.Object);
        var lookupItem = new LookupItem { Id = id, Name = name };

        // Act
        viewModel.SelectedIntake = lookupItem;

        // Assert
        Assert.AreEqual(lookupItem, viewModel.SelectedIntake);
        Assert.AreEqual(id, viewModel.SelectedIntake.Id);
        Assert.AreEqual(name, viewModel.SelectedIntake.Name);
    }

    /// <summary>
    /// Tests that setting SelectedIntake with a LookupItem containing control characters handles correctly.
    /// Input: LookupItem with control characters in Id and Name.
    /// Expected: Property is updated and stores the control characters correctly.
    /// </summary>
    [TestMethod]
    public void SelectedIntake_SetLookupItemWithControlCharacters_StoresCorrectly()
    {
        // Arrange
        var mockAuthService = new Mock<IAuthService>();
        var mockAcademicService = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<RegisterViewModel>>();
        var viewModel = new RegisterViewModel(mockAuthService.Object, mockAcademicService.Object, mockLogger.Object);
        var lookupItem = new LookupItem { Id = "Id\u0000With\u0001Null", Name = "Name\rWith\nNewlines" };

        // Act
        viewModel.SelectedIntake = lookupItem;

        // Assert
        Assert.AreEqual(lookupItem, viewModel.SelectedIntake);
        Assert.AreEqual("Id\u0000With\u0001Null", viewModel.SelectedIntake.Id);
        Assert.AreEqual("Name\rWith\nNewlines", viewModel.SelectedIntake.Name);
    }

    /// <summary>
    /// Tests that setting SelectedIntake with a LookupItem containing Unicode characters handles correctly.
    /// Input: LookupItem with Unicode characters in Id and Name.
    /// Expected: Property is updated and stores the Unicode characters correctly.
    /// </summary>
    [TestMethod]
    [DataRow("你好", "世界")]
    [DataRow("🎓", "📚")]
    [DataRow("Café", "Naïve")]
    [DataRow("Ñoño", "José")]
    public void SelectedIntake_SetLookupItemWithUnicodeCharacters_StoresCorrectly(string id, string name)
    {
        // Arrange
        var mockAuthService = new Mock<IAuthService>();
        var mockAcademicService = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<RegisterViewModel>>();
        var viewModel = new RegisterViewModel(mockAuthService.Object, mockAcademicService.Object, mockLogger.Object);
        var lookupItem = new LookupItem { Id = id, Name = name };

        // Act
        viewModel.SelectedIntake = lookupItem;

        // Assert
        Assert.AreEqual(lookupItem, viewModel.SelectedIntake);
        Assert.AreEqual(id, viewModel.SelectedIntake.Id);
        Assert.AreEqual(name, viewModel.SelectedIntake.Name);
    }

    /// <summary>
    /// Tests that setting SelectedIntake multiple times in rapid succession correctly updates for each change.
    /// Input: Five different LookupItem instances set sequentially.
    /// Expected: Final value is the last LookupItem set, and PropertyChanged raised for each unique change.
    /// </summary>
    [TestMethod]
    public void SelectedIntake_SetMultipleTimesRapidly_UpdatesCorrectlyForEachChange()
    {
        // Arrange
        var mockAuthService = new Mock<IAuthService>();
        var mockAcademicService = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<RegisterViewModel>>();
        var viewModel = new RegisterViewModel(mockAuthService.Object, mockAcademicService.Object, mockLogger.Object);
        var item1 = new LookupItem { Id = "1", Name = "First" };
        var item2 = new LookupItem { Id = "2", Name = "Second" };
        var item3 = new LookupItem { Id = "3", Name = "Third" };
        var item4 = new LookupItem { Id = "4", Name = "Fourth" };
        var item5 = new LookupItem { Id = "5", Name = "Fifth" };
        int eventCount = 0;
        viewModel.PropertyChanged += (sender, args) => { if (args.PropertyName == "SelectedIntake") eventCount++; };

        // Act
        viewModel.SelectedIntake = item1;
        viewModel.SelectedIntake = item2;
        viewModel.SelectedIntake = item3;
        viewModel.SelectedIntake = item4;
        viewModel.SelectedIntake = item5;

        // Assert
        Assert.AreEqual(item5, viewModel.SelectedIntake);
        Assert.AreEqual(5, eventCount);
    }

    /// <summary>
    /// Tests that setting SelectedIntake alternating between value and null raises PropertyChanged for each change.
    /// Input: Alternating sequence of LookupItem and null.
    /// Expected: PropertyChanged is raised for each transition.
    /// </summary>
    [TestMethod]
    public void SelectedIntake_AlternatingBetweenValueAndNull_RaisesPropertyChangedForEachChange()
    {
        // Arrange
        var mockAuthService = new Mock<IAuthService>();
        var mockAcademicService = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<RegisterViewModel>>();
        var viewModel = new RegisterViewModel(mockAuthService.Object, mockAcademicService.Object, mockLogger.Object);
        var lookupItem = new LookupItem { Id = "intake1", Name = "Intake 1" };
        int eventCount = 0;
        viewModel.PropertyChanged += (sender, args) => { if (args.PropertyName == "SelectedIntake") eventCount++; };

        // Act
        viewModel.SelectedIntake = lookupItem;
        viewModel.SelectedIntake = null;
        viewModel.SelectedIntake = lookupItem;
        viewModel.SelectedIntake = null;

        // Assert
        Assert.IsNull(viewModel.SelectedIntake);
        Assert.AreEqual(4, eventCount);
    }

    /// <summary>
    /// Tests that setting SelectedIntake with two different LookupItem instances having identical property values
    /// raises PropertyChanged due to reference inequality.
    /// Input: Two different instances with same Id and Name values.
    /// Expected: PropertyChanged is raised because instances are different (reference equality check).
    /// </summary>
    [TestMethod]
    public void SelectedIntake_SetDifferentInstancesWithIdenticalValues_RaisesPropertyChanged()
    {
        // Arrange
        var mockAuthService = new Mock<IAuthService>();
        var mockAcademicService = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<RegisterViewModel>>();
        var viewModel = new RegisterViewModel(mockAuthService.Object, mockAcademicService.Object, mockLogger.Object);
        var item1 = new LookupItem { Id = "intake1", Name = "Intake 1" };
        var item2 = new LookupItem { Id = "intake1", Name = "Intake 1" };
        int eventCount = 0;
        viewModel.PropertyChanged += (sender, args) => { if (args.PropertyName == "SelectedIntake") eventCount++; };

        // Act
        viewModel.SelectedIntake = item1;
        viewModel.SelectedIntake = item2;

        // Assert
        Assert.AreEqual(item2, viewModel.SelectedIntake);
        Assert.AreEqual(2, eventCount);
    }

    /// <summary>
    /// Tests that SelectedIntake correctly handles boundary case with single-character strings.
    /// Input: LookupItem with single-character Id and Name.
    /// Expected: Property is updated correctly.
    /// </summary>
    [TestMethod]
    public void SelectedIntake_SetLookupItemWithSingleCharacterStrings_StoresCorrectly()
    {
        // Arrange
        var mockAuthService = new Mock<IAuthService>();
        var mockAcademicService = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<RegisterViewModel>>();
        var viewModel = new RegisterViewModel(mockAuthService.Object, mockAcademicService.Object, mockLogger.Object);
        var lookupItem = new LookupItem { Id = "A", Name = "B" };

        // Act
        viewModel.SelectedIntake = lookupItem;

        // Assert
        Assert.AreEqual(lookupItem, viewModel.SelectedIntake);
        Assert.AreEqual("A", viewModel.SelectedIntake.Id);
        Assert.AreEqual("B", viewModel.SelectedIntake.Name);
    }

    /// <summary>
    /// Tests that InitAsync handles concurrent calls correctly by verifying that service methods
    /// may be called multiple times if both calls start before _lookupsLoaded is set.
    /// Input: Two concurrent calls to InitAsync.
    /// Expected: Both calls complete, but service methods may be called multiple times.
    /// </summary>
    [TestMethod]
    public async Task InitAsync_ConcurrentCalls_BothCallsComplete()
    {
        // Arrange
        Mock<IAuthService> mockAuthService = new Mock<IAuthService>();
        Mock<IAcademicService> mockAcademicService = new Mock<IAcademicService>();
        Mock<ILogger<RegisterViewModel>> mockLogger = new Mock<ILogger<RegisterViewModel>>();

        TaskCompletionSource<IEnumerable<LookupItem>> universitiesTcs = new TaskCompletionSource<IEnumerable<LookupItem>>();
        mockAcademicService.Setup(s => s.GetUniversitiesAsync()).Returns(universitiesTcs.Task);
        mockAcademicService.Setup(s => s.GetEntrySchemesAsync()).ReturnsAsync(new List<LookupItem>());
        mockAcademicService.Setup(s => s.GetIntakesAsync()).ReturnsAsync(new List<LookupItem>());
        mockAcademicService.Setup(s => s.GetStudyModesAsync()).ReturnsAsync(new List<LookupItem>());
        mockAcademicService.Setup(s => s.GetAcademicYearsAsync()).ReturnsAsync(new List<LookupItem>());
        mockAcademicService.Setup(s => s.GetSemestersAsync()).ReturnsAsync(new List<LookupItem>());

        RegisterViewModel viewModel = new RegisterViewModel(
            mockAuthService.Object,
            mockAcademicService.Object,
            mockLogger.Object);

        // Act
        Task firstCall = viewModel.InitAsync();
        Task secondCall = viewModel.InitAsync();

        universitiesTcs.SetResult(new List<LookupItem> { new LookupItem { Id = "1", Name = "University 1" } });

        await Task.WhenAll(firstCall, secondCall);

        // Assert - Both calls should complete without exception
        Assert.IsTrue(firstCall.IsCompletedSuccessfully);
        Assert.IsTrue(secondCall.IsCompletedSuccessfully);
    }

    /// <summary>
    /// Tests that InitAsync correctly loads mixed data where some services return items and others return empty collections.
    /// Input: Universities and Intakes return items, others return empty collections.
    /// Expected: Only Universities and Intakes collections are populated.
    /// </summary>
    [TestMethod]
    public async Task InitAsync_MixedResults_LoadsOnlyNonEmptyCollections()
    {
        // Arrange
        Mock<IAuthService> mockAuthService = new Mock<IAuthService>();
        Mock<IAcademicService> mockAcademicService = new Mock<IAcademicService>();
        Mock<ILogger<RegisterViewModel>> mockLogger = new Mock<ILogger<RegisterViewModel>>();

        List<LookupItem> universities = new List<LookupItem>
        {
            new LookupItem { Id = "1", Name = "University 1" },
            new LookupItem { Id = "2", Name = "University 2" }
        };

        List<LookupItem> intakes = new List<LookupItem>
        {
            new LookupItem { Id = "I1", Name = "Intake 1" }
        };

        mockAcademicService.Setup(s => s.GetUniversitiesAsync()).ReturnsAsync(universities);
        mockAcademicService.Setup(s => s.GetEntrySchemesAsync()).ReturnsAsync(new List<LookupItem>());
        mockAcademicService.Setup(s => s.GetIntakesAsync()).ReturnsAsync(intakes);
        mockAcademicService.Setup(s => s.GetStudyModesAsync()).ReturnsAsync(new List<LookupItem>());
        mockAcademicService.Setup(s => s.GetAcademicYearsAsync()).ReturnsAsync(new List<LookupItem>());
        mockAcademicService.Setup(s => s.GetSemestersAsync()).ReturnsAsync(new List<LookupItem>());

        RegisterViewModel viewModel = new RegisterViewModel(
            mockAuthService.Object,
            mockAcademicService.Object,
            mockLogger.Object);

        // Act
        await viewModel.InitAsync();

        // Assert
        Assert.AreEqual(2, viewModel.Universities.Count);
        Assert.AreEqual(0, viewModel.EntrySchemes.Count);
        Assert.AreEqual(1, viewModel.Intakes.Count);
        Assert.AreEqual(0, viewModel.StudyModes.Count);
        Assert.AreEqual(0, viewModel.AcademicYears.Count);
        Assert.AreEqual(0, viewModel.Semesters.Count);
    }

    /// <summary>
    /// Tests that InitAsync verifies all six academic service methods are called exactly once on first call.
    /// Input: First call to InitAsync.
    /// Expected: Each service method is called exactly once.
    /// </summary>
    [TestMethod]
    public async Task InitAsync_FirstCall_CallsAllServiceMethodsExactlyOnce()
    {
        // Arrange
        Mock<IAuthService> mockAuthService = new Mock<IAuthService>();
        Mock<IAcademicService> mockAcademicService = new Mock<IAcademicService>();
        Mock<ILogger<RegisterViewModel>> mockLogger = new Mock<ILogger<RegisterViewModel>>();

        mockAcademicService.Setup(s => s.GetUniversitiesAsync()).ReturnsAsync(new List<LookupItem>());
        mockAcademicService.Setup(s => s.GetEntrySchemesAsync()).ReturnsAsync(new List<LookupItem>());
        mockAcademicService.Setup(s => s.GetIntakesAsync()).ReturnsAsync(new List<LookupItem>());
        mockAcademicService.Setup(s => s.GetStudyModesAsync()).ReturnsAsync(new List<LookupItem>());
        mockAcademicService.Setup(s => s.GetAcademicYearsAsync()).ReturnsAsync(new List<LookupItem>());
        mockAcademicService.Setup(s => s.GetSemestersAsync()).ReturnsAsync(new List<LookupItem>());

        RegisterViewModel viewModel = new RegisterViewModel(
            mockAuthService.Object,
            mockAcademicService.Object,
            mockLogger.Object);

        // Act
        await viewModel.InitAsync();

        // Assert
        mockAcademicService.Verify(s => s.GetUniversitiesAsync(), Times.Once);
        mockAcademicService.Verify(s => s.GetEntrySchemesAsync(), Times.Once);
        mockAcademicService.Verify(s => s.GetIntakesAsync(), Times.Once);
        mockAcademicService.Verify(s => s.GetStudyModesAsync(), Times.Once);
        mockAcademicService.Verify(s => s.GetAcademicYearsAsync(), Times.Once);
        mockAcademicService.Verify(s => s.GetSemestersAsync(), Times.Once);
    }

    /// <summary>
    /// Tests that InitAsync correctly loads very large collections from service methods.
    /// Input: GetUniversitiesAsync returns 10000 items.
    /// Expected: All 10000 items are loaded into Universities collection.
    /// </summary>
    [TestMethod]
    public async Task InitAsync_VeryLargeCollection_LoadsAllItems()
    {
        // Arrange
        Mock<IAuthService> mockAuthService = new Mock<IAuthService>();
        Mock<IAcademicService> mockAcademicService = new Mock<IAcademicService>();
        Mock<ILogger<RegisterViewModel>> mockLogger = new Mock<ILogger<RegisterViewModel>>();

        List<LookupItem> largeCollection = new List<LookupItem>();
        for (int i = 0; i < 10000; i++)
        {
            largeCollection.Add(new LookupItem { Id = $"ID{i}", Name = $"Name{i}" });
        }

        mockAcademicService.Setup(s => s.GetUniversitiesAsync()).ReturnsAsync(largeCollection);
        mockAcademicService.Setup(s => s.GetEntrySchemesAsync()).ReturnsAsync(new List<LookupItem>());
        mockAcademicService.Setup(s => s.GetIntakesAsync()).ReturnsAsync(new List<LookupItem>());
        mockAcademicService.Setup(s => s.GetStudyModesAsync()).ReturnsAsync(new List<LookupItem>());
        mockAcademicService.Setup(s => s.GetAcademicYearsAsync()).ReturnsAsync(new List<LookupItem>());
        mockAcademicService.Setup(s => s.GetSemestersAsync()).ReturnsAsync(new List<LookupItem>());

        RegisterViewModel viewModel = new RegisterViewModel(
            mockAuthService.Object,
            mockAcademicService.Object,
            mockLogger.Object);

        // Act
        await viewModel.InitAsync();

        // Assert
        Assert.AreEqual(10000, viewModel.Universities.Count);
        Assert.AreEqual("ID0", viewModel.Universities[0].Id);
        Assert.AreEqual("ID9999", viewModel.Universities[9999].Id);
    }

    /// <summary>
    /// Tests that InitAsync returns immediately without calling services on third consecutive call.
    /// Input: Three consecutive calls to InitAsync.
    /// Expected: Service methods are called only once, third call returns immediately.
    /// </summary>
    [TestMethod]
    public async Task InitAsync_ThirdCall_ReturnsImmediatelyWithoutReloading()
    {
        // Arrange
        Mock<IAuthService> mockAuthService = new Mock<IAuthService>();
        Mock<IAcademicService> mockAcademicService = new Mock<IAcademicService>();
        Mock<ILogger<RegisterViewModel>> mockLogger = new Mock<ILogger<RegisterViewModel>>();

        mockAcademicService.Setup(s => s.GetUniversitiesAsync()).ReturnsAsync(new List<LookupItem> { new LookupItem { Id = "1", Name = "Uni 1" } });
        mockAcademicService.Setup(s => s.GetEntrySchemesAsync()).ReturnsAsync(new List<LookupItem>());
        mockAcademicService.Setup(s => s.GetIntakesAsync()).ReturnsAsync(new List<LookupItem>());
        mockAcademicService.Setup(s => s.GetStudyModesAsync()).ReturnsAsync(new List<LookupItem>());
        mockAcademicService.Setup(s => s.GetAcademicYearsAsync()).ReturnsAsync(new List<LookupItem>());
        mockAcademicService.Setup(s => s.GetSemestersAsync()).ReturnsAsync(new List<LookupItem>());

        RegisterViewModel viewModel = new RegisterViewModel(
            mockAuthService.Object,
            mockAcademicService.Object,
            mockLogger.Object);

        // Act
        await viewModel.InitAsync();
        await viewModel.InitAsync();
        await viewModel.InitAsync();

        // Assert
        mockAcademicService.Verify(s => s.GetUniversitiesAsync(), Times.Once);
        mockAcademicService.Verify(s => s.GetEntrySchemesAsync(), Times.Once);
        mockAcademicService.Verify(s => s.GetIntakesAsync(), Times.Once);
        mockAcademicService.Verify(s => s.GetStudyModesAsync(), Times.Once);
        mockAcademicService.Verify(s => s.GetAcademicYearsAsync(), Times.Once);
        mockAcademicService.Verify(s => s.GetSemestersAsync(), Times.Once);
        Assert.AreEqual(1, viewModel.Universities.Count);
    }

    /// <summary>
    /// Tests that InitAsync correctly handles collections with duplicate items.
    /// Input: GetEntrySchemes returns collection with duplicate LookupItem instances.
    /// Expected: All items including duplicates are loaded into the collection.
    /// </summary>
    [TestMethod]
    public async Task InitAsync_CollectionWithDuplicates_LoadsAllItemsIncludingDuplicates()
    {
        // Arrange
        Mock<IAuthService> mockAuthService = new Mock<IAuthService>();
        Mock<IAcademicService> mockAcademicService = new Mock<IAcademicService>();
        Mock<ILogger<RegisterViewModel>> mockLogger = new Mock<ILogger<RegisterViewModel>>();

        List<LookupItem> entrySchemes = new List<LookupItem>
        {
            new LookupItem { Id = "1", Name = "Scheme 1" },
            new LookupItem { Id = "1", Name = "Scheme 1" },
            new LookupItem { Id = "2", Name = "Scheme 2" }
        };

        mockAcademicService.Setup(s => s.GetUniversitiesAsync()).ReturnsAsync(new List<LookupItem>());
        mockAcademicService.Setup(s => s.GetEntrySchemesAsync()).ReturnsAsync(entrySchemes);
        mockAcademicService.Setup(s => s.GetIntakesAsync()).ReturnsAsync(new List<LookupItem>());
        mockAcademicService.Setup(s => s.GetStudyModesAsync()).ReturnsAsync(new List<LookupItem>());
        mockAcademicService.Setup(s => s.GetAcademicYearsAsync()).ReturnsAsync(new List<LookupItem>());
        mockAcademicService.Setup(s => s.GetSemestersAsync()).ReturnsAsync(new List<LookupItem>());

        RegisterViewModel viewModel = new RegisterViewModel(
            mockAuthService.Object,
            mockAcademicService.Object,
            mockLogger.Object);

        // Act
        await viewModel.InitAsync();

        // Assert
        Assert.AreEqual(3, viewModel.EntrySchemes.Count);
    }

    /// <summary>
    /// Tests that InitAsync does not call service methods when _lookupsLoaded is already true from a previous call.
    /// Input: First call completes successfully, then second call is made.
    /// Expected: Second call returns immediately without making any service calls.
    /// </summary>
    [TestMethod]
    public async Task InitAsync_WhenLookupsAlreadyLoaded_DoesNotCallServices()
    {
        // Arrange
        Mock<IAuthService> mockAuthService = new Mock<IAuthService>();
        Mock<IAcademicService> mockAcademicService = new Mock<IAcademicService>();
        Mock<ILogger<RegisterViewModel>> mockLogger = new Mock<ILogger<RegisterViewModel>>();

        int callCount = 0;
        mockAcademicService.Setup(s => s.GetUniversitiesAsync()).ReturnsAsync(() =>
        {
            callCount++;
            return new List<LookupItem>();
        });
        mockAcademicService.Setup(s => s.GetEntrySchemesAsync()).ReturnsAsync(new List<LookupItem>());
        mockAcademicService.Setup(s => s.GetIntakesAsync()).ReturnsAsync(new List<LookupItem>());
        mockAcademicService.Setup(s => s.GetStudyModesAsync()).ReturnsAsync(new List<LookupItem>());
        mockAcademicService.Setup(s => s.GetAcademicYearsAsync()).ReturnsAsync(new List<LookupItem>());
        mockAcademicService.Setup(s => s.GetSemestersAsync()).ReturnsAsync(new List<LookupItem>());

        RegisterViewModel viewModel = new RegisterViewModel(
            mockAuthService.Object,
            mockAcademicService.Object,
            mockLogger.Object);

        // Act
        await viewModel.InitAsync();
        int callCountAfterFirstInit = callCount;
        await viewModel.InitAsync();

        // Assert
        Assert.AreEqual(1, callCountAfterFirstInit);
        Assert.AreEqual(1, callCount); // Should still be 1, no additional calls
    }

    /// <summary>
    /// Tests that the constructor successfully initializes RegisterViewModel with valid dependencies.
    /// Input: Valid mock instances for IAuthService, IAcademicService, and ILogger.
    /// Expected: Instance is created without throwing exceptions.
    /// </summary>
    [TestMethod]
    public void Constructor_WithValidDependencies_InitializesSuccessfully()
    {
        // Arrange
        Mock<IAuthService> mockAuth = new Mock<IAuthService>();
        Mock<IAcademicService> mockAcademic = new Mock<IAcademicService>();
        Mock<ILogger<RegisterViewModel>> mockLogger = new Mock<ILogger<RegisterViewModel>>();

        // Act
        RegisterViewModel viewModel = new RegisterViewModel(mockAuth.Object, mockAcademic.Object, mockLogger.Object);

        // Assert
        Assert.IsNotNull(viewModel);
    }

    /// <summary>
    /// Tests that the constructor initializes SendOtpCommand property.
    /// Input: Valid mock dependencies.
    /// Expected: SendOtpCommand is not null and is of type IAsyncRelayCommand.
    /// </summary>
    [TestMethod]
    public void Constructor_WithValidDependencies_InitializesSendOtpCommand()
    {
        // Arrange
        Mock<IAuthService> mockAuth = new Mock<IAuthService>();
        Mock<IAcademicService> mockAcademic = new Mock<IAcademicService>();
        Mock<ILogger<RegisterViewModel>> mockLogger = new Mock<ILogger<RegisterViewModel>>();

        // Act
        RegisterViewModel viewModel = new RegisterViewModel(mockAuth.Object, mockAcademic.Object, mockLogger.Object);

        // Assert
        Assert.IsNotNull(viewModel.SendOtpCommand);
        Assert.IsInstanceOfType(viewModel.SendOtpCommand, typeof(IAsyncRelayCommand));
    }

    /// <summary>
    /// Tests that the constructor initializes VerifyOtpCommand property.
    /// Input: Valid mock dependencies.
    /// Expected: VerifyOtpCommand is not null and is of type IAsyncRelayCommand.
    /// </summary>
    [TestMethod]
    public void Constructor_WithValidDependencies_InitializesVerifyOtpCommand()
    {
        // Arrange
        Mock<IAuthService> mockAuth = new Mock<IAuthService>();
        Mock<IAcademicService> mockAcademic = new Mock<IAcademicService>();
        Mock<ILogger<RegisterViewModel>> mockLogger = new Mock<ILogger<RegisterViewModel>>();

        // Act
        RegisterViewModel viewModel = new RegisterViewModel(mockAuth.Object, mockAcademic.Object, mockLogger.Object);

        // Assert
        Assert.IsNotNull(viewModel.VerifyOtpCommand);
        Assert.IsInstanceOfType(viewModel.VerifyOtpCommand, typeof(IAsyncRelayCommand));
    }

    /// <summary>
    /// Tests that the constructor initializes RegisterCommand property.
    /// Input: Valid mock dependencies.
    /// Expected: RegisterCommand is not null and is of type IAsyncRelayCommand.
    /// </summary>
    [TestMethod]
    public void Constructor_WithValidDependencies_InitializesRegisterCommand()
    {
        // Arrange
        Mock<IAuthService> mockAuth = new Mock<IAuthService>();
        Mock<IAcademicService> mockAcademic = new Mock<IAcademicService>();
        Mock<ILogger<RegisterViewModel>> mockLogger = new Mock<ILogger<RegisterViewModel>>();

        // Act
        RegisterViewModel viewModel = new RegisterViewModel(mockAuth.Object, mockAcademic.Object, mockLogger.Object);

        // Assert
        Assert.IsNotNull(viewModel.RegisterCommand);
        Assert.IsInstanceOfType(viewModel.RegisterCommand, typeof(IAsyncRelayCommand));
    }

    /// <summary>
    /// Tests that the constructor initializes all three command properties simultaneously.
    /// Input: Valid mock dependencies.
    /// Expected: All three commands (SendOtpCommand, VerifyOtpCommand, RegisterCommand) are not null.
    /// </summary>
    [TestMethod]
    public void Constructor_WithValidDependencies_InitializesAllThreeCommands()
    {
        // Arrange
        Mock<IAuthService> mockAuth = new Mock<IAuthService>();
        Mock<IAcademicService> mockAcademic = new Mock<IAcademicService>();
        Mock<ILogger<RegisterViewModel>> mockLogger = new Mock<ILogger<RegisterViewModel>>();

        // Act
        RegisterViewModel viewModel = new RegisterViewModel(mockAuth.Object, mockAcademic.Object, mockLogger.Object);

        // Assert
        Assert.IsNotNull(viewModel.SendOtpCommand, "SendOtpCommand should be initialized");
        Assert.IsNotNull(viewModel.VerifyOtpCommand, "VerifyOtpCommand should be initialized");
        Assert.IsNotNull(viewModel.RegisterCommand, "RegisterCommand should be initialized");
    }

    /// <summary>
    /// Tests that the constructor handles null IAuthService parameter.
    /// Input: null for auth parameter, valid mocks for other parameters.
    /// Expected: Constructor completes (no null check in code), but accessing auth-dependent functionality may fail.
    /// </summary>
    [TestMethod]
    public void Constructor_WithNullAuthService_CompletesConstruction()
    {
        // Arrange
        Mock<IAcademicService> mockAcademic = new Mock<IAcademicService>();
        Mock<ILogger<RegisterViewModel>> mockLogger = new Mock<ILogger<RegisterViewModel>>();

        // Act
        RegisterViewModel viewModel = new RegisterViewModel(null!, mockAcademic.Object, mockLogger.Object);

        // Assert
        Assert.IsNotNull(viewModel);
        Assert.IsNotNull(viewModel.SendOtpCommand);
        Assert.IsNotNull(viewModel.VerifyOtpCommand);
        Assert.IsNotNull(viewModel.RegisterCommand);
    }

    /// <summary>
    /// Tests that the constructor handles null IAcademicService parameter.
    /// Input: null for academic parameter, valid mocks for other parameters.
    /// Expected: Constructor completes (no null check in code), but accessing academic-dependent functionality may fail.
    /// </summary>
    [TestMethod]
    public void Constructor_WithNullAcademicService_CompletesConstruction()
    {
        // Arrange
        Mock<IAuthService> mockAuth = new Mock<IAuthService>();
        Mock<ILogger<RegisterViewModel>> mockLogger = new Mock<ILogger<RegisterViewModel>>();

        // Act
        RegisterViewModel viewModel = new RegisterViewModel(mockAuth.Object, null!, mockLogger.Object);

        // Assert
        Assert.IsNotNull(viewModel);
        Assert.IsNotNull(viewModel.SendOtpCommand);
        Assert.IsNotNull(viewModel.VerifyOtpCommand);
        Assert.IsNotNull(viewModel.RegisterCommand);
    }

    /// <summary>
    /// Tests that the constructor handles null ILogger parameter.
    /// Input: null for logger parameter, valid mocks for other parameters.
    /// Expected: Constructor completes (no null check in code), but logging functionality may fail.
    /// </summary>
    [TestMethod]
    public void Constructor_WithNullLogger_CompletesConstruction()
    {
        // Arrange
        Mock<IAuthService> mockAuth = new Mock<IAuthService>();
        Mock<IAcademicService> mockAcademic = new Mock<IAcademicService>();

        // Act
        RegisterViewModel viewModel = new RegisterViewModel(mockAuth.Object, mockAcademic.Object, null!);

        // Assert
        Assert.IsNotNull(viewModel);
        Assert.IsNotNull(viewModel.SendOtpCommand);
        Assert.IsNotNull(viewModel.VerifyOtpCommand);
        Assert.IsNotNull(viewModel.RegisterCommand);
    }

    /// <summary>
    /// Tests that the constructor handles all null parameters.
    /// Input: null for all three parameters.
    /// Expected: Constructor completes but commands are still initialized.
    /// </summary>
    [TestMethod]
    public void Constructor_WithAllNullParameters_StillInitializesCommands()
    {
        // Arrange & Act
        RegisterViewModel viewModel = new RegisterViewModel(null!, null!, null!);

        // Assert
        Assert.IsNotNull(viewModel);
        Assert.IsNotNull(viewModel.SendOtpCommand);
        Assert.IsNotNull(viewModel.VerifyOtpCommand);
        Assert.IsNotNull(viewModel.RegisterCommand);
    }

    /// <summary>
    /// Tests that multiple instances of RegisterViewModel can be created independently.
    /// Input: Two separate sets of valid mock dependencies.
    /// Expected: Both instances are created successfully and are distinct objects.
    /// </summary>
    [TestMethod]
    public void Constructor_CreateMultipleInstances_CreatesIndependentInstances()
    {
        // Arrange
        Mock<IAuthService> mockAuth1 = new Mock<IAuthService>();
        Mock<IAcademicService> mockAcademic1 = new Mock<IAcademicService>();
        Mock<ILogger<RegisterViewModel>> mockLogger1 = new Mock<ILogger<RegisterViewModel>>();

        Mock<IAuthService> mockAuth2 = new Mock<IAuthService>();
        Mock<IAcademicService> mockAcademic2 = new Mock<IAcademicService>();
        Mock<ILogger<RegisterViewModel>> mockLogger2 = new Mock<ILogger<RegisterViewModel>>();

        // Act
        RegisterViewModel viewModel1 = new RegisterViewModel(mockAuth1.Object, mockAcademic1.Object, mockLogger1.Object);
        RegisterViewModel viewModel2 = new RegisterViewModel(mockAuth2.Object, mockAcademic2.Object, mockLogger2.Object);

        // Assert
        Assert.IsNotNull(viewModel1);
        Assert.IsNotNull(viewModel2);
        Assert.AreNotSame(viewModel1, viewModel2);
        Assert.AreNotSame(viewModel1.SendOtpCommand, viewModel2.SendOtpCommand);
        Assert.AreNotSame(viewModel1.VerifyOtpCommand, viewModel2.VerifyOtpCommand);
        Assert.AreNotSame(viewModel1.RegisterCommand, viewModel2.RegisterCommand);
    }

    /// <summary>
    /// Tests that constructor creates distinct command instances for each property.
    /// Input: Valid mock dependencies.
    /// Expected: SendOtpCommand, VerifyOtpCommand, and RegisterCommand are three distinct objects.
    /// </summary>
    [TestMethod]
    public void Constructor_WithValidDependencies_CreatesDistinctCommandInstances()
    {
        // Arrange
        Mock<IAuthService> mockAuth = new Mock<IAuthService>();
        Mock<IAcademicService> mockAcademic = new Mock<IAcademicService>();
        Mock<ILogger<RegisterViewModel>> mockLogger = new Mock<ILogger<RegisterViewModel>>();

        // Act
        RegisterViewModel viewModel = new RegisterViewModel(mockAuth.Object, mockAcademic.Object, mockLogger.Object);

        // Assert
        Assert.AreNotSame(viewModel.SendOtpCommand, viewModel.VerifyOtpCommand);
        Assert.AreNotSame(viewModel.SendOtpCommand, viewModel.RegisterCommand);
        Assert.AreNotSame(viewModel.VerifyOtpCommand, viewModel.RegisterCommand);
    }

    /// <summary>
    /// Helper class that exposes the protected OnPropertyChanged method for testing.
    /// </summary>
    private class TestableRegisterViewModelForOnPropertyChanged : RegisterViewModel
    {
        public TestableRegisterViewModelForOnPropertyChanged(IAuthService auth, IAcademicService academic, ILogger<RegisterViewModel> logger)
            : base(auth, academic, logger)
        {
        }

        public void ExposedOnPropertyChanged(string name)
        {
            OnPropertyChanged(name);
        }
    }

    /// <summary>
    /// Tests that OnPropertyChanged properly handles numeric strings as property names.
    /// Input: Strings containing only numbers.
    /// Expected: PropertyChanged event is raised with the numeric string.
    /// </summary>
    [TestMethod]
    [DataRow("0")]
    [DataRow("1")]
    [DataRow("123")]
    [DataRow("999999")]
    [DataRow("-1")]
    [DataRow("2147483647")]
    public void OnPropertyChanged_NumericStrings_RaisesPropertyChangedEvent(string propertyName)
    {
        // Arrange
        Mock<IAuthService> mockAuthService = new Mock<IAuthService>();
        Mock<IAcademicService> mockAcademicService = new Mock<IAcademicService>();
        Mock<ILogger<RegisterViewModel>> mockLogger = new Mock<ILogger<RegisterViewModel>>();
        TestableRegisterViewModelForOnPropertyChanged viewModel = new TestableRegisterViewModelForOnPropertyChanged(
            mockAuthService.Object,
            mockAcademicService.Object,
            mockLogger.Object);

        string? raisedPropertyName = null;
        viewModel.PropertyChanged += (sender, e) => raisedPropertyName = e.PropertyName;

        // Act
        viewModel.ExposedOnPropertyChanged(propertyName);

        // Assert
        Assert.AreEqual(propertyName, raisedPropertyName, $"PropertyChanged event should be raised with numeric string '{propertyName}'");
    }

    /// <summary>
    /// Tests that OnPropertyChanged handles mixed content strings correctly.
    /// Input: Strings with alphanumeric and special character combinations.
    /// Expected: PropertyChanged event is raised with the mixed content preserved.
    /// </summary>
    [TestMethod]
    [DataRow("Property123")]
    [DataRow("123Property")]
    [DataRow("Prop_123_Test")]
    [DataRow("Property.Item[0]")]
    [DataRow("Dictionary[\"Key\"]")]
    public void OnPropertyChanged_MixedContentStrings_RaisesPropertyChangedEvent(string propertyName)
    {
        // Arrange
        Mock<IAuthService> mockAuthService = new Mock<IAuthService>();
        Mock<IAcademicService> mockAcademicService = new Mock<IAcademicService>();
        Mock<ILogger<RegisterViewModel>> mockLogger = new Mock<ILogger<RegisterViewModel>>();
        TestableRegisterViewModelForOnPropertyChanged viewModel = new TestableRegisterViewModelForOnPropertyChanged(
            mockAuthService.Object,
            mockAcademicService.Object,
            mockLogger.Object);

        string? raisedPropertyName = null;
        viewModel.PropertyChanged += (sender, e) => raisedPropertyName = e.PropertyName;

        // Act
        viewModel.ExposedOnPropertyChanged(propertyName);

        // Assert
        Assert.AreEqual(propertyName, raisedPropertyName, $"PropertyChanged event should be raised with mixed content string");
    }

    /// <summary>
    /// Tests that OnPropertyChanged correctly handles single character property names.
    /// Input: Single character strings including letters, numbers, and symbols.
    /// Expected: PropertyChanged event is raised with the single character.
    /// </summary>
    [TestMethod]
    [DataRow("A")]
    [DataRow("z")]
    [DataRow("0")]
    [DataRow("9")]
    [DataRow("_")]
    [DataRow("!")]
    [DataRow("@")]
    public void OnPropertyChanged_SingleCharacter_RaisesPropertyChangedEvent(string propertyName)
    {
        // Arrange
        Mock<IAuthService> mockAuthService = new Mock<IAuthService>();
        Mock<IAcademicService> mockAcademicService = new Mock<IAcademicService>();
        Mock<ILogger<RegisterViewModel>> mockLogger = new Mock<ILogger<RegisterViewModel>>();
        TestableRegisterViewModelForOnPropertyChanged viewModel = new TestableRegisterViewModelForOnPropertyChanged(
            mockAuthService.Object,
            mockAcademicService.Object,
            mockLogger.Object);

        string? raisedPropertyName = null;
        viewModel.PropertyChanged += (sender, e) => raisedPropertyName = e.PropertyName;

        // Act
        viewModel.ExposedOnPropertyChanged(propertyName);

        // Assert
        Assert.AreEqual(propertyName, raisedPropertyName, $"PropertyChanged event should be raised with single character '{propertyName}'");
    }

    /// <summary>
    /// Tests that OnPropertyChanged handles strings at various length boundaries.
    /// Input: Strings of varying lengths.
    /// Expected: PropertyChanged event is raised correctly for all lengths.
    /// </summary>
    [TestMethod]
    [DataRow(0)]
    [DataRow(1)]
    [DataRow(10)]
    [DataRow(50)]
    [DataRow(100)]
    [DataRow(255)]
    [DataRow(256)]
    [DataRow(1000)]
    [DataRow(10000)]
    public void OnPropertyChanged_VariousStringLengths_RaisesPropertyChangedEvent(int length)
    {
        // Arrange
        Mock<IAuthService> mockAuthService = new Mock<IAuthService>();
        Mock<IAcademicService> mockAcademicService = new Mock<IAcademicService>();
        Mock<ILogger<RegisterViewModel>> mockLogger = new Mock<ILogger<RegisterViewModel>>();
        TestableRegisterViewModelForOnPropertyChanged viewModel = new TestableRegisterViewModelForOnPropertyChanged(
            mockAuthService.Object,
            mockAcademicService.Object,
            mockLogger.Object);

        string propertyName = new string('P', length);
        string? raisedPropertyName = null;
        viewModel.PropertyChanged += (sender, e) => raisedPropertyName = e.PropertyName;

        // Act
        viewModel.ExposedOnPropertyChanged(propertyName);

        // Assert
        Assert.AreEqual(propertyName, raisedPropertyName, $"PropertyChanged event should be raised for string of length {length}");
        Assert.AreEqual(length, raisedPropertyName?.Length ?? 0, $"Raised property name should have length {length}");
    }

    /// <summary>
    /// Tests that multiple subscribers to PropertyChanged all receive the event.
    /// Input: Valid property name with multiple event subscribers.
    /// Expected: All subscribers receive the PropertyChanged event.
    /// </summary>
    [TestMethod]
    public void OnPropertyChanged_MultipleSubscribers_AllReceiveEvent()
    {
        // Arrange
        Mock<IAuthService> mockAuthService = new Mock<IAuthService>();
        Mock<IAcademicService> mockAcademicService = new Mock<IAcademicService>();
        Mock<ILogger<RegisterViewModel>> mockLogger = new Mock<ILogger<RegisterViewModel>>();
        TestableRegisterViewModelForOnPropertyChanged viewModel = new TestableRegisterViewModelForOnPropertyChanged(
            mockAuthService.Object,
            mockAcademicService.Object,
            mockLogger.Object);

        string? subscriber1PropertyName = null;
        string? subscriber2PropertyName = null;
        string? subscriber3PropertyName = null;

        viewModel.PropertyChanged += (sender, e) => subscriber1PropertyName = e.PropertyName;
        viewModel.PropertyChanged += (sender, e) => subscriber2PropertyName = e.PropertyName;
        viewModel.PropertyChanged += (sender, e) => subscriber3PropertyName = e.PropertyName;

        // Act
        viewModel.ExposedOnPropertyChanged("TestProperty");

        // Assert
        Assert.AreEqual("TestProperty", subscriber1PropertyName, "First subscriber should receive the event");
        Assert.AreEqual("TestProperty", subscriber2PropertyName, "Second subscriber should receive the event");
        Assert.AreEqual("TestProperty", subscriber3PropertyName, "Third subscriber should receive the event");
    }

    /// <summary>
    /// Tests that OnPropertyChanged event args contain correct PropertyName.
    /// Input: Valid property name.
    /// Expected: PropertyChangedEventArgs.PropertyName matches the input property name.
    /// </summary>
    [TestMethod]
    public void OnPropertyChanged_ValidPropertyName_EventArgsContainCorrectPropertyName()
    {
        // Arrange
        Mock<IAuthService> mockAuthService = new Mock<IAuthService>();
        Mock<IAcademicService> mockAcademicService = new Mock<IAcademicService>();
        Mock<ILogger<RegisterViewModel>> mockLogger = new Mock<ILogger<RegisterViewModel>>();
        TestableRegisterViewModelForOnPropertyChanged viewModel = new TestableRegisterViewModelForOnPropertyChanged(
            mockAuthService.Object,
            mockAcademicService.Object,
            mockLogger.Object);

        PropertyChangedEventArgs? capturedEventArgs = null;
        viewModel.PropertyChanged += (sender, e) => capturedEventArgs = e;

        // Act
        viewModel.ExposedOnPropertyChanged("Email");

        // Assert
        Assert.IsNotNull(capturedEventArgs, "Event args should not be null");
        Assert.AreEqual("Email", capturedEventArgs.PropertyName, "Event args PropertyName should match the input");
    }

    /// <summary>
    /// Tests that OnPropertyChanged handles rapid successive calls efficiently.
    /// Input: 100 rapid successive calls with different property names.
    /// Expected: All events are raised correctly.
    /// </summary>
    [TestMethod]
    public void OnPropertyChanged_RapidSuccessiveCalls_RaisesAllEvents()
    {
        // Arrange
        Mock<IAuthService> mockAuthService = new Mock<IAuthService>();
        Mock<IAcademicService> mockAcademicService = new Mock<IAcademicService>();
        Mock<ILogger<RegisterViewModel>> mockLogger = new Mock<ILogger<RegisterViewModel>>();
        TestableRegisterViewModelForOnPropertyChanged viewModel = new TestableRegisterViewModelForOnPropertyChanged(
            mockAuthService.Object,
            mockAcademicService.Object,
            mockLogger.Object);

        List<string?> raisedPropertyNames = new List<string?>();
        viewModel.PropertyChanged += (sender, e) => raisedPropertyNames.Add(e.PropertyName);

        // Act
        for (int i = 0; i < 100; i++)
        {
            viewModel.ExposedOnPropertyChanged($"Property{i}");
        }

        // Assert
        Assert.AreEqual(100, raisedPropertyNames.Count, "All 100 events should be raised");
        for (int i = 0; i < 100; i++)
        {
            Assert.AreEqual($"Property{i}", raisedPropertyNames[i], $"Property name at index {i} should be correct");
        }
    }

    /// <summary>
    /// Tests that setting CurrentStep to various valid integer values updates the property correctly.
    /// Input: Valid integer values including 0, positive, negative, and typical step values.
    /// Expected: Property value is updated to the new value.
    /// </summary>
    /// <param name="newValue">The value to set for CurrentStep.</param>
    [TestMethod]
    [DataRow(0)]
    [DataRow(1)]
    [DataRow(2)]
    [DataRow(3)]
    [DataRow(4)]
    [DataRow(5)]
    [DataRow(10)]
    [DataRow(100)]
    [DataRow(-1)]
    [DataRow(-10)]
    [DataRow(-100)]
    public void CurrentStep_SetToValidValue_UpdatesProperty(int newValue)
    {
        // Arrange
        var viewModel = new RegisterViewModel(_authServiceMock.Object, _academicServiceMock.Object, _loggerMock.Object);

        // Act
        viewModel.CurrentStep = newValue;

        // Assert
        Assert.AreEqual(newValue, viewModel.CurrentStep);
    }

    /// <summary>
    /// Tests that setting CurrentStep to extreme boundary values (int.MinValue and int.MaxValue) updates the property correctly.
    /// Input: int.MinValue and int.MaxValue.
    /// Expected: Property value is updated to the extreme value.
    /// </summary>
    /// <param name="extremeValue">The extreme boundary value to set.</param>
    [TestMethod]
    [DataRow(int.MinValue)]
    [DataRow(int.MaxValue)]
    public void CurrentStep_SetToExtremeBoundaryValue_UpdatesProperty(int extremeValue)
    {
        // Arrange
        var viewModel = new RegisterViewModel(_authServiceMock.Object, _academicServiceMock.Object, _loggerMock.Object);

        // Act
        viewModel.CurrentStep = extremeValue;

        // Assert
        Assert.AreEqual(extremeValue, viewModel.CurrentStep);
    }

    /// <summary>
    /// Tests that setting CurrentStep to a different value raises the PropertyChanged event with the correct property name.
    /// Input: A new value different from the current value.
    /// Expected: PropertyChanged event is raised with PropertyName = "CurrentStep".
    /// </summary>
    [TestMethod]
    public void CurrentStep_SetToDifferentValue_RaisesPropertyChangedEvent()
    {
        // Arrange
        var viewModel = new RegisterViewModel(_authServiceMock.Object, _academicServiceMock.Object, _loggerMock.Object);
        string? raisedPropertyName = null;
        viewModel.PropertyChanged += (sender, args) => raisedPropertyName = args.PropertyName;

        // Act
        viewModel.CurrentStep = 2;

        // Assert
        Assert.AreEqual("CurrentStep", raisedPropertyName);
    }

    /// <summary>
    /// Tests that setting CurrentStep to the same value does not raise the PropertyChanged event.
    /// Input: The same value as the current value.
    /// Expected: PropertyChanged event is not raised.
    /// </summary>
    [TestMethod]
    public void CurrentStep_SetToSameValue_DoesNotRaisePropertyChangedEvent()
    {
        // Arrange
        var viewModel = new RegisterViewModel(_authServiceMock.Object, _academicServiceMock.Object, _loggerMock.Object);
        viewModel.CurrentStep = 2;
        int eventCount = 0;
        viewModel.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == "CurrentStep")
            {
                eventCount++;
            }
        };

        // Act
        viewModel.CurrentStep = 2;

        // Assert
        Assert.AreEqual(0, eventCount);
    }

    /// <summary>
    /// Tests that setting CurrentStep to zero updates the property correctly.
    /// Input: 0.
    /// Expected: Property value is updated to 0.
    /// </summary>
    [TestMethod]
    public void CurrentStep_SetToZero_UpdatesProperty()
    {
        // Arrange
        var viewModel = new RegisterViewModel(_authServiceMock.Object, _academicServiceMock.Object, _loggerMock.Object);

        // Act
        viewModel.CurrentStep = 0;

        // Assert
        Assert.AreEqual(0, viewModel.CurrentStep);
    }

    /// <summary>
    /// Tests that setting CurrentStep to negative values updates the property correctly.
    /// Input: Various negative integer values.
    /// Expected: Property value is updated to the negative value.
    /// </summary>
    /// <param name="negativeValue">The negative value to set.</param>
    [TestMethod]
    [DataRow(-1)]
    [DataRow(-5)]
    [DataRow(-100)]
    [DataRow(-1000)]
    [DataRow(int.MinValue)]
    public void CurrentStep_SetToNegativeValue_UpdatesProperty(int negativeValue)
    {
        // Arrange
        var viewModel = new RegisterViewModel(_authServiceMock.Object, _academicServiceMock.Object, _loggerMock.Object);

        // Act
        viewModel.CurrentStep = negativeValue;

        // Assert
        Assert.AreEqual(negativeValue, viewModel.CurrentStep);
    }

    /// <summary>
    /// Tests that setting CurrentStep from default value (1) to the same value (1) does not raise PropertyChanged event.
    /// Input: Setting CurrentStep to 1 when it's already 1 (initial value).
    /// Expected: PropertyChanged event is not raised.
    /// </summary>
    [TestMethod]
    public void CurrentStep_SetToInitialValueAgain_DoesNotRaisePropertyChangedEvent()
    {
        // Arrange
        var viewModel = new RegisterViewModel(_authServiceMock.Object, _academicServiceMock.Object, _loggerMock.Object);
        int eventCount = 0;
        viewModel.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == "CurrentStep")
            {
                eventCount++;
            }
        };

        // Act
        viewModel.CurrentStep = 1;

        // Assert
        Assert.AreEqual(1, viewModel.CurrentStep);
        Assert.AreEqual(0, eventCount);
    }

    /// <summary>
    /// Tests that PropertyChanged event is raised for each distinct value change when setting CurrentStep to a sequence of different values.
    /// Input: Sequence of different values: 2, 3, 1, 5.
    /// Expected: PropertyChanged event is raised 4 times (once for each distinct change).
    /// </summary>
    [TestMethod]
    public void CurrentStep_SetSequenceOfDifferentValues_RaisesPropertyChangedForEachChange()
    {
        // Arrange
        var viewModel = new RegisterViewModel(_authServiceMock.Object, _academicServiceMock.Object, _loggerMock.Object);
        int eventCount = 0;
        viewModel.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == "CurrentStep")
            {
                eventCount++;
            }
        };

        // Act
        viewModel.CurrentStep = 2;
        viewModel.CurrentStep = 3;
        viewModel.CurrentStep = 1;
        viewModel.CurrentStep = 5;

        // Assert
        Assert.AreEqual(5, viewModel.CurrentStep);
        Assert.AreEqual(4, eventCount);
    }

    /// <summary>
    /// Tests that setting SelectedProgram with LookupItem containing whitespace-only strings stores the value correctly.
    /// Input: LookupItem with whitespace-only Id and Name (spaces, tabs, newlines).
    /// Expected: Property stores the LookupItem with whitespace strings and raises PropertyChanged event.
    /// </summary>
    /// <param name="id">The whitespace string for Id.</param>
    /// <param name="name">The whitespace string for Name.</param>
    [TestMethod]
    [DataRow("   ", "   ", DisplayName = "Spaces only")]
    [DataRow("\t", "\t", DisplayName = "Tabs only")]
    [DataRow("\n", "\n", DisplayName = "Newlines only")]
    [DataRow("\r\n", "\r\n", DisplayName = "Carriage return and newline")]
    [DataRow(" \t\n ", " \t\n ", DisplayName = "Mixed whitespace")]
    [DataRow("     ", "\t\t\t\t", DisplayName = "Multiple spaces and tabs")]
    public void SelectedProgram_SetWithWhitespaceOnlyStrings_StoresValueCorrectly(string id, string name)
    {
        // Arrange
        var authMock = new Mock<IAuthService>();
        var academicMock = new Mock<IAcademicService>();
        var loggerMock = new Mock<ILogger<RegisterViewModel>>();
        var viewModel = new RegisterViewModel(authMock.Object, academicMock.Object, loggerMock.Object);
        var lookupItem = new LookupItem { Id = id, Name = name };

        // Act
        viewModel.SelectedProgram = lookupItem;

        // Assert
        Assert.IsNotNull(viewModel.SelectedProgram);
        Assert.AreEqual(id, viewModel.SelectedProgram.Id);
        Assert.AreEqual(name, viewModel.SelectedProgram.Name);
    }

    /// <summary>
    /// Tests that setting SelectedProgram with LookupItem containing special characters and unicode stores correctly.
    /// Input: LookupItem with various special characters, symbols, and unicode characters.
    /// Expected: Property stores the LookupItem correctly.
    /// </summary>
    /// <param name="id">The Id with special characters.</param>
    /// <param name="name">The Name with special characters.</param>
    [TestMethod]
    [DataRow("prog!@#$%^&*()", "Program with symbols !@#$", DisplayName = "Special symbols")]
    [DataRow("prog<>?:\"{}|", "Program brackets <>{}[]", DisplayName = "Brackets and pipes")]
    [DataRow("программа123", "Программа на русском", DisplayName = "Cyrillic characters")]
    [DataRow("程序123", "中文程序名称", DisplayName = "Chinese characters")]
    [DataRow("プログラム123", "日本語のプログラム", DisplayName = "Japanese characters")]
    [DataRow("prog😀😁😂", "Program with emojis 🎓📚", DisplayName = "Emojis")]
    [DataRow("café-résumé", "Naïve jalapeño", DisplayName = "Accented characters")]
    [DataRow("prog/path\\test", "Program/with\\slashes", DisplayName = "Path separators")]
    public void SelectedProgram_SetWithSpecialCharacters_StoresValueCorrectly(string id, string name)
    {
        // Arrange
        var authMock = new Mock<IAuthService>();
        var academicMock = new Mock<IAcademicService>();
        var loggerMock = new Mock<ILogger<RegisterViewModel>>();
        var viewModel = new RegisterViewModel(authMock.Object, academicMock.Object, loggerMock.Object);
        var lookupItem = new LookupItem { Id = id, Name = name };

        // Act
        viewModel.SelectedProgram = lookupItem;

        // Assert
        Assert.IsNotNull(viewModel.SelectedProgram);
        Assert.AreEqual(id, viewModel.SelectedProgram.Id);
        Assert.AreEqual(name, viewModel.SelectedProgram.Name);
    }

    /// <summary>
    /// Tests that setting SelectedProgram with LookupItem containing very long strings stores the value correctly.
    /// Input: LookupItem with very long strings (10000 characters for Id and Name).
    /// Expected: Property stores the LookupItem with very long strings correctly.
    /// </summary>
    [TestMethod]
    public void SelectedProgram_SetWithVeryLongStrings_StoresValueCorrectly()
    {
        // Arrange
        var authMock = new Mock<IAuthService>();
        var academicMock = new Mock<IAcademicService>();
        var loggerMock = new Mock<ILogger<RegisterViewModel>>();
        var viewModel = new RegisterViewModel(authMock.Object, academicMock.Object, loggerMock.Object);
        var veryLongId = new string('A', 10000);
        var veryLongName = new string('B', 10000);
        var lookupItem = new LookupItem { Id = veryLongId, Name = veryLongName };

        // Act
        viewModel.SelectedProgram = lookupItem;

        // Assert
        Assert.IsNotNull(viewModel.SelectedProgram);
        Assert.AreEqual(veryLongId, viewModel.SelectedProgram.Id);
        Assert.AreEqual(veryLongName, viewModel.SelectedProgram.Name);
        Assert.AreEqual(10000, viewModel.SelectedProgram.Id.Length);
        Assert.AreEqual(10000, viewModel.SelectedProgram.Name.Length);
    }

    /// <summary>
    /// Tests that setting SelectedProgram with LookupItem containing control characters stores correctly.
    /// Input: LookupItem with various control characters (null char, bell, etc.).
    /// Expected: Property stores the LookupItem with control characters.
    /// </summary>
    /// <param name="id">The Id with control characters.</param>
    /// <param name="name">The Name with control characters.</param>
    [TestMethod]
    [DataRow("prog\u0000id", "name\u0000program", DisplayName = "Null character")]
    [DataRow("prog\u0001id", "name\u0001program", DisplayName = "Start of heading")]
    [DataRow("prog\u0007id", "name\u0007program", DisplayName = "Bell character")]
    [DataRow("prog\u001Fid", "name\u001Fprogram", DisplayName = "Unit separator")]
    [DataRow("prog\bid", "name\bprogram", DisplayName = "Backspace character")]
    [DataRow("prog\fid", "name\fprogram", DisplayName = "Form feed")]
    public void SelectedProgram_SetWithControlCharacters_StoresValueCorrectly(string id, string name)
    {
        // Arrange
        var authMock = new Mock<IAuthService>();
        var academicMock = new Mock<IAcademicService>();
        var loggerMock = new Mock<ILogger<RegisterViewModel>>();
        var viewModel = new RegisterViewModel(authMock.Object, academicMock.Object, loggerMock.Object);
        var lookupItem = new LookupItem { Id = id, Name = name };

        // Act
        viewModel.SelectedProgram = lookupItem;

        // Assert
        Assert.IsNotNull(viewModel.SelectedProgram);
        Assert.AreEqual(id, viewModel.SelectedProgram.Id);
        Assert.AreEqual(name, viewModel.SelectedProgram.Name);
    }

    /// <summary>
    /// Tests that setting SelectedProgram raises PropertyChanged event with correct sender.
    /// Input: Valid LookupItem instance.
    /// Expected: PropertyChanged event is raised with the viewModel instance as sender.
    /// </summary>
    [TestMethod]
    public void SelectedProgram_SetValue_RaisesPropertyChangedEventWithCorrectSender()
    {
        // Arrange
        var authMock = new Mock<IAuthService>();
        var academicMock = new Mock<IAcademicService>();
        var loggerMock = new Mock<ILogger<RegisterViewModel>>();
        var viewModel = new RegisterViewModel(authMock.Object, academicMock.Object, loggerMock.Object);
        var lookupItem = new LookupItem { Id = "prog1", Name = "Program 1" };
        object? capturedSender = null;
        viewModel.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == nameof(viewModel.SelectedProgram))
            {
                capturedSender = sender;
            }
        };

        // Act
        viewModel.SelectedProgram = lookupItem;

        // Assert
        Assert.IsNotNull(capturedSender);
        Assert.AreSame(viewModel, capturedSender);
    }

    /// <summary>
    /// Tests that setting SelectedProgram multiple times in rapid succession updates correctly for each change.
    /// Input: Five different LookupItem instances set sequentially.
    /// Expected: Final value is the last LookupItem set, and PropertyChanged is raised for each distinct change.
    /// </summary>
    [TestMethod]
    public void SelectedProgram_MultipleRapidSequentialChanges_UpdatesCorrectlyForEach()
    {
        // Arrange
        var authMock = new Mock<IAuthService>();
        var academicMock = new Mock<IAcademicService>();
        var loggerMock = new Mock<ILogger<RegisterViewModel>>();
        var viewModel = new RegisterViewModel(authMock.Object, academicMock.Object, loggerMock.Object);
        var program1 = new LookupItem { Id = "prog1", Name = "Program 1" };
        var program2 = new LookupItem { Id = "prog2", Name = "Program 2" };
        var program3 = new LookupItem { Id = "prog3", Name = "Program 3" };
        var program4 = new LookupItem { Id = "prog4", Name = "Program 4" };
        var program5 = new LookupItem { Id = "prog5", Name = "Program 5" };
        var propertyChangedCount = 0;
        viewModel.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == nameof(viewModel.SelectedProgram))
            {
                propertyChangedCount++;
            }
        };

        // Act
        viewModel.SelectedProgram = program1;
        viewModel.SelectedProgram = program2;
        viewModel.SelectedProgram = program3;
        viewModel.SelectedProgram = program4;
        viewModel.SelectedProgram = program5;

        // Assert
        Assert.AreSame(program5, viewModel.SelectedProgram);
        Assert.AreEqual(5, propertyChangedCount);
    }

    /// <summary>
    /// Tests that setting SelectedProgram with mixed content (whitespace, special chars, unicode, numbers) stores correctly.
    /// Input: LookupItem with complex mixed string content.
    /// Expected: Property stores the mixed content correctly.
    /// </summary>
    [TestMethod]
    public void SelectedProgram_SetWithMixedComplexContent_StoresValueCorrectly()
    {
        // Arrange
        var authMock = new Mock<IAuthService>();
        var academicMock = new Mock<IAcademicService>();
        var loggerMock = new Mock<ILogger<RegisterViewModel>>();
        var viewModel = new RegisterViewModel(authMock.Object, academicMock.Object, loggerMock.Object);
        var mixedId = "  prog-123_test!@# 中文😀\t\n";
        var mixedName = "Program Name: Special-123_Test! 日本語🎓  \r\n";
        var lookupItem = new LookupItem { Id = mixedId, Name = mixedName };

        // Act
        viewModel.SelectedProgram = lookupItem;

        // Assert
        Assert.IsNotNull(viewModel.SelectedProgram);
        Assert.AreEqual(mixedId, viewModel.SelectedProgram.Id);
        Assert.AreEqual(mixedName, viewModel.SelectedProgram.Name);
    }

    /// <summary>
    /// Tests that InitAsync successfully loads all lookup data from the academic service
    /// on the first call when lookups have not been loaded yet.
    /// Input: First call to InitAsync with all service methods returning test data.
    /// Expected: All six academic service methods are called and their data is loaded
    /// into the corresponding observable collections, and _lookupsLoaded flag is set.
    /// </summary>
    [TestMethod]
    public async Task InitAsync_FirstCall_LoadsAllLookupsSuccessfully()
    {
        // Arrange
        Mock<IAuthService> mockAuthService = new Mock<IAuthService>();
        Mock<IAcademicService> mockAcademicService = new Mock<IAcademicService>();
        Mock<ILogger<RegisterViewModel>> mockLogger = new Mock<ILogger<RegisterViewModel>>();

        List<LookupItem> universities = new List<LookupItem>
        {
            new LookupItem { Id = "uni1", Name = "University 1" },
            new LookupItem { Id = "uni2", Name = "University 2" }
        };
        List<LookupItem> entrySchemes = new List<LookupItem>
        {
            new LookupItem { Id = "scheme1", Name = "Scheme 1" }
        };
        List<LookupItem> intakes = new List<LookupItem>
        {
            new LookupItem { Id = "intake1", Name = "Intake 1" }
        };
        List<LookupItem> studyModes = new List<LookupItem>
        {
            new LookupItem { Id = "mode1", Name = "Mode 1" }
        };
        List<LookupItem> academicYears = new List<LookupItem>
        {
            new LookupItem { Id = "year1", Name = "Year 1" }
        };
        List<LookupItem> semesters = new List<LookupItem>
        {
            new LookupItem { Id = "sem1", Name = "Semester 1" }
        };

        mockAcademicService.Setup(s => s.GetUniversitiesAsync()).ReturnsAsync(universities);
        mockAcademicService.Setup(s => s.GetEntrySchemesAsync()).ReturnsAsync(entrySchemes);
        mockAcademicService.Setup(s => s.GetIntakesAsync()).ReturnsAsync(intakes);
        mockAcademicService.Setup(s => s.GetStudyModesAsync()).ReturnsAsync(studyModes);
        mockAcademicService.Setup(s => s.GetAcademicYearsAsync()).ReturnsAsync(academicYears);
        mockAcademicService.Setup(s => s.GetSemestersAsync()).ReturnsAsync(semesters);

        RegisterViewModel viewModel = new RegisterViewModel(mockAuthService.Object, mockAcademicService.Object, mockLogger.Object);

        // Act
        await viewModel.InitAsync();

        // Assert
        Assert.AreEqual(2, viewModel.Universities.Count);
        Assert.AreEqual("uni1", viewModel.Universities[0].Id);
        Assert.AreEqual("uni2", viewModel.Universities[1].Id);
        Assert.AreEqual(1, viewModel.EntrySchemes.Count);
        Assert.AreEqual("scheme1", viewModel.EntrySchemes[0].Id);
        Assert.AreEqual(1, viewModel.Intakes.Count);
        Assert.AreEqual("intake1", viewModel.Intakes[0].Id);
        Assert.AreEqual(1, viewModel.StudyModes.Count);
        Assert.AreEqual("mode1", viewModel.StudyModes[0].Id);
        Assert.AreEqual(1, viewModel.AcademicYears.Count);
        Assert.AreEqual("year1", viewModel.AcademicYears[0].Id);
        Assert.AreEqual(1, viewModel.Semesters.Count);
        Assert.AreEqual("sem1", viewModel.Semesters[0].Id);

        mockAcademicService.Verify(s => s.GetUniversitiesAsync(), Times.Once);
        mockAcademicService.Verify(s => s.GetEntrySchemesAsync(), Times.Once);
        mockAcademicService.Verify(s => s.GetIntakesAsync(), Times.Once);
        mockAcademicService.Verify(s => s.GetStudyModesAsync(), Times.Once);
        mockAcademicService.Verify(s => s.GetAcademicYearsAsync(), Times.Once);
        mockAcademicService.Verify(s => s.GetSemestersAsync(), Times.Once);
    }

    /// <summary>
    /// Tests that InitAsync correctly handles empty collections returned from the academic service.
    /// Input: All academic service methods return empty collections.
    /// Expected: Observable collections are cleared and remain empty, and _lookupsLoaded flag is set.
    /// </summary>
    [TestMethod]
    public async Task InitAsync_EmptyCollections_LoadsEmptyLookupsAndSetsFlag()
    {
        // Arrange
        Mock<IAuthService> mockAuthService = new Mock<IAuthService>();
        Mock<IAcademicService> mockAcademicService = new Mock<IAcademicService>();
        Mock<ILogger<RegisterViewModel>> mockLogger = new Mock<ILogger<RegisterViewModel>>();

        mockAcademicService.Setup(s => s.GetUniversitiesAsync()).ReturnsAsync(new List<LookupItem>());
        mockAcademicService.Setup(s => s.GetEntrySchemesAsync()).ReturnsAsync(new List<LookupItem>());
        mockAcademicService.Setup(s => s.GetIntakesAsync()).ReturnsAsync(new List<LookupItem>());
        mockAcademicService.Setup(s => s.GetStudyModesAsync()).ReturnsAsync(new List<LookupItem>());
        mockAcademicService.Setup(s => s.GetAcademicYearsAsync()).ReturnsAsync(new List<LookupItem>());
        mockAcademicService.Setup(s => s.GetSemestersAsync()).ReturnsAsync(new List<LookupItem>());

        RegisterViewModel viewModel = new RegisterViewModel(mockAuthService.Object, mockAcademicService.Object, mockLogger.Object);

        // Act
        await viewModel.InitAsync();

        // Assert
        Assert.AreEqual(0, viewModel.Universities.Count);
        Assert.AreEqual(0, viewModel.EntrySchemes.Count);
        Assert.AreEqual(0, viewModel.Intakes.Count);
        Assert.AreEqual(0, viewModel.StudyModes.Count);
        Assert.AreEqual(0, viewModel.AcademicYears.Count);
        Assert.AreEqual(0, viewModel.Semesters.Count);

        await viewModel.InitAsync();
        mockAcademicService.Verify(s => s.GetUniversitiesAsync(), Times.Once);
    }

    /// <summary>
    /// Tests that InitAsync handles third and subsequent calls correctly without reloading.
    /// Input: Three consecutive calls to InitAsync.
    /// Expected: Service methods are called only once during the first call.
    /// </summary>
    [TestMethod]
    public async Task InitAsync_ThirdCall_StillDoesNotReloadLookups()
    {
        // Arrange
        Mock<IAuthService> mockAuthService = new Mock<IAuthService>();
        Mock<IAcademicService> mockAcademicService = new Mock<IAcademicService>();
        Mock<ILogger<RegisterViewModel>> mockLogger = new Mock<ILogger<RegisterViewModel>>();

        List<LookupItem> universities = new List<LookupItem> { new LookupItem { Id = "uni1", Name = "University 1" } };
        mockAcademicService.Setup(s => s.GetUniversitiesAsync()).ReturnsAsync(universities);
        mockAcademicService.Setup(s => s.GetEntrySchemesAsync()).ReturnsAsync(new List<LookupItem>());
        mockAcademicService.Setup(s => s.GetIntakesAsync()).ReturnsAsync(new List<LookupItem>());
        mockAcademicService.Setup(s => s.GetStudyModesAsync()).ReturnsAsync(new List<LookupItem>());
        mockAcademicService.Setup(s => s.GetAcademicYearsAsync()).ReturnsAsync(new List<LookupItem>());
        mockAcademicService.Setup(s => s.GetSemestersAsync()).ReturnsAsync(new List<LookupItem>());

        RegisterViewModel viewModel = new RegisterViewModel(mockAuthService.Object, mockAcademicService.Object, mockLogger.Object);

        // Act
        await viewModel.InitAsync();
        await viewModel.InitAsync();
        await viewModel.InitAsync();

        // Assert
        mockAcademicService.Verify(s => s.GetUniversitiesAsync(), Times.Once);
        mockAcademicService.Verify(s => s.GetEntrySchemesAsync(), Times.Once);
        mockAcademicService.Verify(s => s.GetIntakesAsync(), Times.Once);
        mockAcademicService.Verify(s => s.GetStudyModesAsync(), Times.Once);
        mockAcademicService.Verify(s => s.GetAcademicYearsAsync(), Times.Once);
        mockAcademicService.Verify(s => s.GetSemestersAsync(), Times.Once);
    }

    /// <summary>
    /// Tests that InitAsync loads data into all six observable collections correctly.
    /// Input: All service methods return single-item collections.
    /// Expected: Each of the six collections contains exactly one item with correct data.
    /// </summary>
    [TestMethod]
    public async Task InitAsync_AllCollections_ArePopulatedCorrectly()
    {
        // Arrange
        Mock<IAuthService> mockAuthService = new Mock<IAuthService>();
        Mock<IAcademicService> mockAcademicService = new Mock<IAcademicService>();
        Mock<ILogger<RegisterViewModel>> mockLogger = new Mock<ILogger<RegisterViewModel>>();

        mockAcademicService.Setup(s => s.GetUniversitiesAsync()).ReturnsAsync(new List<LookupItem> { new LookupItem { Id = "u1", Name = "Uni 1" } });
        mockAcademicService.Setup(s => s.GetEntrySchemesAsync()).ReturnsAsync(new List<LookupItem> { new LookupItem { Id = "e1", Name = "Entry 1" } });
        mockAcademicService.Setup(s => s.GetIntakesAsync()).ReturnsAsync(new List<LookupItem> { new LookupItem { Id = "i1", Name = "Intake 1" } });
        mockAcademicService.Setup(s => s.GetStudyModesAsync()).ReturnsAsync(new List<LookupItem> { new LookupItem { Id = "m1", Name = "Mode 1" } });
        mockAcademicService.Setup(s => s.GetAcademicYearsAsync()).ReturnsAsync(new List<LookupItem> { new LookupItem { Id = "y1", Name = "Year 1" } });
        mockAcademicService.Setup(s => s.GetSemestersAsync()).ReturnsAsync(new List<LookupItem> { new LookupItem { Id = "s1", Name = "Sem 1" } });

        RegisterViewModel viewModel = new RegisterViewModel(mockAuthService.Object, mockAcademicService.Object, mockLogger.Object);

        // Act
        await viewModel.InitAsync();

        // Assert
        Assert.AreEqual(1, viewModel.Universities.Count);
        Assert.AreEqual("u1", viewModel.Universities[0].Id);
        Assert.AreEqual(1, viewModel.EntrySchemes.Count);
        Assert.AreEqual("e1", viewModel.EntrySchemes[0].Id);
        Assert.AreEqual(1, viewModel.Intakes.Count);
        Assert.AreEqual("i1", viewModel.Intakes[0].Id);
        Assert.AreEqual(1, viewModel.StudyModes.Count);
        Assert.AreEqual("m1", viewModel.StudyModes[0].Id);
        Assert.AreEqual(1, viewModel.AcademicYears.Count);
        Assert.AreEqual("y1", viewModel.AcademicYears[0].Id);
        Assert.AreEqual(1, viewModel.Semesters.Count);
        Assert.AreEqual("s1", viewModel.Semesters[0].Id);
    }

    /// <summary>
    /// Tests that InitAsync verifies the correct order of items loaded into collections.
    /// Input: Service methods return collections with specific item order.
    /// Expected: Items appear in collections in the same order as returned from service.
    /// </summary>
    [TestMethod]
    public async Task InitAsync_ItemOrder_IsPreservedInCollections()
    {
        // Arrange
        Mock<IAuthService> mockAuthService = new Mock<IAuthService>();
        Mock<IAcademicService> mockAcademicService = new Mock<IAcademicService>();
        Mock<ILogger<RegisterViewModel>> mockLogger = new Mock<ILogger<RegisterViewModel>>();

        List<LookupItem> universities = new List<LookupItem>
        {
            new LookupItem { Id = "uni1", Name = "A University" },
            new LookupItem { Id = "uni2", Name = "B University" },
            new LookupItem { Id = "uni3", Name = "C University" }
        };

        mockAcademicService.Setup(s => s.GetUniversitiesAsync()).ReturnsAsync(universities);
        mockAcademicService.Setup(s => s.GetEntrySchemesAsync()).ReturnsAsync(new List<LookupItem>());
        mockAcademicService.Setup(s => s.GetIntakesAsync()).ReturnsAsync(new List<LookupItem>());
        mockAcademicService.Setup(s => s.GetStudyModesAsync()).ReturnsAsync(new List<LookupItem>());
        mockAcademicService.Setup(s => s.GetAcademicYearsAsync()).ReturnsAsync(new List<LookupItem>());
        mockAcademicService.Setup(s => s.GetSemestersAsync()).ReturnsAsync(new List<LookupItem>());

        RegisterViewModel viewModel = new RegisterViewModel(mockAuthService.Object, mockAcademicService.Object, mockLogger.Object);

        // Act
        await viewModel.InitAsync();

        // Assert
        Assert.AreEqual("uni1", viewModel.Universities[0].Id);
        Assert.AreEqual("uni2", viewModel.Universities[1].Id);
        Assert.AreEqual("uni3", viewModel.Universities[2].Id);
    }

    /// <summary>
    /// Tests that InitAsync handles collections with LookupItems having empty string properties.
    /// Input: Service methods return items with empty Id and Name strings.
    /// Expected: Items with empty strings are loaded correctly into collections.
    /// </summary>
    [TestMethod]
    public async Task InitAsync_LookupItemsWithEmptyStrings_AreLoadedCorrectly()
    {
        // Arrange
        Mock<IAuthService> mockAuthService = new Mock<IAuthService>();
        Mock<IAcademicService> mockAcademicService = new Mock<IAcademicService>();
        Mock<ILogger<RegisterViewModel>> mockLogger = new Mock<ILogger<RegisterViewModel>>();

        List<LookupItem> universities = new List<LookupItem>
        {
            new LookupItem { Id = "", Name = "" }
        };

        mockAcademicService.Setup(s => s.GetUniversitiesAsync()).ReturnsAsync(universities);
        mockAcademicService.Setup(s => s.GetEntrySchemesAsync()).ReturnsAsync(new List<LookupItem>());
        mockAcademicService.Setup(s => s.GetIntakesAsync()).ReturnsAsync(new List<LookupItem>());
        mockAcademicService.Setup(s => s.GetStudyModesAsync()).ReturnsAsync(new List<LookupItem>());
        mockAcademicService.Setup(s => s.GetAcademicYearsAsync()).ReturnsAsync(new List<LookupItem>());
        mockAcademicService.Setup(s => s.GetSemestersAsync()).ReturnsAsync(new List<LookupItem>());

        RegisterViewModel viewModel = new RegisterViewModel(mockAuthService.Object, mockAcademicService.Object, mockLogger.Object);

        // Act
        await viewModel.InitAsync();

        // Assert
        Assert.AreEqual(1, viewModel.Universities.Count);
        Assert.AreEqual("", viewModel.Universities[0].Id);
        Assert.AreEqual("", viewModel.Universities[0].Name);
    }

    /// <summary>
    /// Tests that InitAsync handles collections with LookupItems having special characters.
    /// Input: Service methods return items with special characters in Id and Name.
    /// Expected: Items with special characters are loaded correctly.
    /// </summary>
    [TestMethod]
    public async Task InitAsync_LookupItemsWithSpecialCharacters_AreLoadedCorrectly()
    {
        // Arrange
        Mock<IAuthService> mockAuthService = new Mock<IAuthService>();
        Mock<IAcademicService> mockAcademicService = new Mock<IAcademicService>();
        Mock<ILogger<RegisterViewModel>> mockLogger = new Mock<ILogger<RegisterViewModel>>();

        List<LookupItem> universities = new List<LookupItem>
        {
            new LookupItem { Id = "uni!@#$%^&*()", Name = "University with special chars: <>&\"'" }
        };

        mockAcademicService.Setup(s => s.GetUniversitiesAsync()).ReturnsAsync(universities);
        mockAcademicService.Setup(s => s.GetEntrySchemesAsync()).ReturnsAsync(new List<LookupItem>());
        mockAcademicService.Setup(s => s.GetIntakesAsync()).ReturnsAsync(new List<LookupItem>());
        mockAcademicService.Setup(s => s.GetStudyModesAsync()).ReturnsAsync(new List<LookupItem>());
        mockAcademicService.Setup(s => s.GetAcademicYearsAsync()).ReturnsAsync(new List<LookupItem>());
        mockAcademicService.Setup(s => s.GetSemestersAsync()).ReturnsAsync(new List<LookupItem>());

        RegisterViewModel viewModel = new RegisterViewModel(mockAuthService.Object, mockAcademicService.Object, mockLogger.Object);

        // Act
        await viewModel.InitAsync();

        // Assert
        Assert.AreEqual(1, viewModel.Universities.Count);
        Assert.AreEqual("uni!@#$%^&*()", viewModel.Universities[0].Id);
        Assert.AreEqual("University with special chars: <>&\"'", viewModel.Universities[0].Name);
    }

    /// <summary>
    /// Tests that InitAsync handles very large collections efficiently.
    /// Input: Service method returns a collection with 1000 items.
    /// Expected: All 1000 items are loaded into the collection.
    /// </summary>
    [TestMethod]
    public async Task InitAsync_LargeCollection_LoadsAllItemsSuccessfully()
    {
        // Arrange
        Mock<IAuthService> mockAuthService = new Mock<IAuthService>();
        Mock<IAcademicService> mockAcademicService = new Mock<IAcademicService>();
        Mock<ILogger<RegisterViewModel>> mockLogger = new Mock<ILogger<RegisterViewModel>>();

        List<LookupItem> universities = new List<LookupItem>();
        for (int i = 0; i < 1000; i++)
        {
            universities.Add(new LookupItem { Id = $"uni{i}", Name = $"University {i}" });
        }

        mockAcademicService.Setup(s => s.GetUniversitiesAsync()).ReturnsAsync(universities);
        mockAcademicService.Setup(s => s.GetEntrySchemesAsync()).ReturnsAsync(new List<LookupItem>());
        mockAcademicService.Setup(s => s.GetIntakesAsync()).ReturnsAsync(new List<LookupItem>());
        mockAcademicService.Setup(s => s.GetStudyModesAsync()).ReturnsAsync(new List<LookupItem>());
        mockAcademicService.Setup(s => s.GetAcademicYearsAsync()).ReturnsAsync(new List<LookupItem>());
        mockAcademicService.Setup(s => s.GetSemestersAsync()).ReturnsAsync(new List<LookupItem>());

        RegisterViewModel viewModel = new RegisterViewModel(mockAuthService.Object, mockAcademicService.Object, mockLogger.Object);

        // Act
        await viewModel.InitAsync();

        // Assert
        Assert.AreEqual(1000, viewModel.Universities.Count);
        Assert.AreEqual("uni0", viewModel.Universities[0].Id);
        Assert.AreEqual("uni999", viewModel.Universities[999].Id);
    }

    /// <summary>
    /// Tests that OtherNames property returns the initial default value of empty string.
    /// Input: None (testing initial state).
    /// Expected: OtherNames property returns empty string.
    /// </summary>
    [TestMethod]
    public void OtherNames_InitialValue_ReturnsEmptyString()
    {
        // Arrange
        var authMock = new Mock<IAuthService>();
        var academicMock = new Mock<IAcademicService>();
        var loggerMock = new Mock<ILogger<RegisterViewModel>>();
        var viewModel = new RegisterViewModel(authMock.Object, academicMock.Object, loggerMock.Object);

        // Act
        var result = viewModel.OtherNames;

        // Assert
        Assert.AreEqual(string.Empty, result);
    }

    /// <summary>
    /// Tests that setting the OtherNames property raises the PropertyChanged event with the correct property name.
    /// Input: A new string value different from the current value.
    /// Expected: PropertyChanged event is raised with PropertyName = "OtherNames".
    /// </summary>
    [TestMethod]
    public void OtherNames_SetNewValue_RaisesPropertyChangedEvent()
    {
        // Arrange
        var authMock = new Mock<IAuthService>();
        var academicMock = new Mock<IAcademicService>();
        var loggerMock = new Mock<ILogger<RegisterViewModel>>();
        var viewModel = new RegisterViewModel(authMock.Object, academicMock.Object, loggerMock.Object);
        var propertyChangedRaised = false;
        string? raisedPropertyName = null;
        viewModel.PropertyChanged += (sender, e) =>
        {
            propertyChangedRaised = true;
            raisedPropertyName = e.PropertyName;
        };

        // Act
        viewModel.OtherNames = "TestName";

        // Assert
        Assert.IsTrue(propertyChangedRaised);
        Assert.AreEqual("OtherNames", raisedPropertyName);
    }

    /// <summary>
    /// Tests that setting the OtherNames property to an empty string updates the property correctly.
    /// Input: Empty string.
    /// Expected: Property value is set to empty string.
    /// </summary>
    [TestMethod]
    public void OtherNames_SetEmptyString_UpdatesProperty()
    {
        // Arrange
        var authMock = new Mock<IAuthService>();
        var academicMock = new Mock<IAcademicService>();
        var loggerMock = new Mock<ILogger<RegisterViewModel>>();
        var viewModel = new RegisterViewModel(authMock.Object, academicMock.Object, loggerMock.Object);
        viewModel.OtherNames = "SomeName";

        // Act
        viewModel.OtherNames = string.Empty;

        // Assert
        Assert.AreEqual(string.Empty, viewModel.OtherNames);
    }

    /// <summary>
    /// Tests that setting the OtherNames property with whitespace-only strings updates the property correctly.
    /// Input: Strings containing only whitespace (spaces, tabs, newlines).
    /// Expected: Property value is updated to the whitespace string.
    /// </summary>
    /// <param name="value">The whitespace string to set.</param>
    [TestMethod]
    [DataRow("   ")]
    [DataRow("\t")]
    [DataRow("\n")]
    [DataRow(" \t\n ")]
    public void OtherNames_SetWhitespaceOnlyString_UpdatesProperty(string value)
    {
        // Arrange
        var authMock = new Mock<IAuthService>();
        var academicMock = new Mock<IAcademicService>();
        var loggerMock = new Mock<ILogger<RegisterViewModel>>();
        var viewModel = new RegisterViewModel(authMock.Object, academicMock.Object, loggerMock.Object);

        // Act
        viewModel.OtherNames = value;

        // Assert
        Assert.AreEqual(value, viewModel.OtherNames);
    }

    /// <summary>
    /// Tests that SelectedEntryScheme property returns null as its initial default value.
    /// Input: None (testing initial state).
    /// Expected: SelectedEntryScheme returns null.
    /// </summary>
    [TestMethod]
    public void SelectedEntryScheme_InitialValue_ReturnsNull()
    {
        // Arrange
        var mockAuth = new Mock<IAuthService>();
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<RegisterViewModel>>();
        var viewModel = new RegisterViewModel(mockAuth.Object, mockAcademic.Object, mockLogger.Object);

        // Act
        var result = viewModel.SelectedEntryScheme;

        // Assert
        Assert.IsNull(result);
    }

    /// <summary>
    /// Tests that setting SelectedEntryScheme to null updates the property correctly.
    /// Input: Null value.
    /// Expected: Property returns null.
    /// </summary>
    [TestMethod]
    public void SelectedEntryScheme_SetToNull_UpdatesProperty()
    {
        // Arrange
        var mockAuth = new Mock<IAuthService>();
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<RegisterViewModel>>();
        var viewModel = new RegisterViewModel(mockAuth.Object, mockAcademic.Object, mockLogger.Object);
        var scheme = new LookupItem { Id = "scheme1", Name = "Direct Entry" };
        viewModel.SelectedEntryScheme = scheme;

        // Act
        viewModel.SelectedEntryScheme = null;

        // Assert
        Assert.IsNull(viewModel.SelectedEntryScheme);
    }

    /// <summary>
    /// Tests that SelectedEntryScheme handles LookupItem with empty strings for Id and Name correctly.
    /// Input: LookupItem with empty Id and Name.
    /// Expected: Property is set correctly and PropertyChanged is raised.
    /// </summary>
    [TestMethod]
    public void SelectedEntryScheme_SetLookupItemWithEmptyStrings_UpdatesPropertyCorrectly()
    {
        // Arrange
        var mockAuth = new Mock<IAuthService>();
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<RegisterViewModel>>();
        var viewModel = new RegisterViewModel(mockAuth.Object, mockAcademic.Object, mockLogger.Object);
        var scheme = new LookupItem { Id = "", Name = "" };

        // Act
        viewModel.SelectedEntryScheme = scheme;

        // Assert
        Assert.AreEqual(scheme, viewModel.SelectedEntryScheme);
        Assert.AreEqual("", viewModel.SelectedEntryScheme.Id);
        Assert.AreEqual("", viewModel.SelectedEntryScheme.Name);
    }

    /// <summary>
    /// Tests that SelectedEntryScheme handles rapid consecutive changes correctly.
    /// Input: Multiple different LookupItem instances set in rapid succession.
    /// Expected: Final value is the last set value and PropertyChanged is raised for each change.
    /// </summary>
    [TestMethod]
    public void SelectedEntryScheme_RapidConsecutiveChanges_UpdatesCorrectlyForEachChange()
    {
        // Arrange
        var mockAuth = new Mock<IAuthService>();
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<RegisterViewModel>>();
        var viewModel = new RegisterViewModel(mockAuth.Object, mockAcademic.Object, mockLogger.Object);
        var schemes = new[]
        {
            new LookupItem { Id = "scheme1", Name = "Entry 1" },
            new LookupItem { Id = "scheme2", Name = "Entry 2" },
            new LookupItem { Id = "scheme3", Name = "Entry 3" },
            new LookupItem { Id = "scheme4", Name = "Entry 4" },
            new LookupItem { Id = "scheme5", Name = "Entry 5" }
        };
        int eventRaisedCount = 0;
        viewModel.PropertyChanged += (sender, e) => { if (e.PropertyName == "SelectedEntryScheme") eventRaisedCount++; };

        // Act
        foreach (var scheme in schemes)
        {
            viewModel.SelectedEntryScheme = scheme;
        }

        // Assert
        Assert.AreEqual(5, eventRaisedCount);
        Assert.AreEqual(schemes[4], viewModel.SelectedEntryScheme);
    }

    /// <summary>
    /// Tests that SelectedEntryScheme handles boundary case with single-character strings.
    /// Input: LookupItem with single-character Id and Name.
    /// Expected: Property is set correctly.
    /// </summary>
    [TestMethod]
    public void SelectedEntryScheme_SetLookupItemWithSingleCharacterStrings_UpdatesProperty()
    {
        // Arrange
        var mockAuth = new Mock<IAuthService>();
        var mockAcademic = new Mock<IAcademicService>();
        var mockLogger = new Mock<ILogger<RegisterViewModel>>();
        var viewModel = new RegisterViewModel(mockAuth.Object, mockAcademic.Object, mockLogger.Object);
        var scheme = new LookupItem { Id = "A", Name = "B" };

        // Act
        viewModel.SelectedEntryScheme = scheme;

        // Assert
        Assert.AreEqual(scheme, viewModel.SelectedEntryScheme);
        Assert.AreEqual("A", viewModel.SelectedEntryScheme.Id);
        Assert.AreEqual("B", viewModel.SelectedEntryScheme.Name);
    }
}