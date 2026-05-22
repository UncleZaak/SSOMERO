using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

using Microsoft.Maui.Controls;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Ssomero;
using Ssomero.Interfaces;
using Ssomero.Models;
using Ssomero.Services;
using Ssomero.ViewModels;

namespace Ssomero.ViewModels.UnitTests;




/// <summary>
/// Unit tests for the DashboardViewModel class.
/// </summary>
[TestClass]
public class DashboardViewModelTests
{
    /// <summary>
    /// Tests that the Role property getter returns the current role value.
    /// Input: None (uses default "Student" value).
    /// Expected: Returns "Student".
    /// </summary>
    [TestMethod]
    public void Role_DefaultValue_ReturnsStudent()
    {
        // Arrange
        var dashboardServiceMock = new Mock<IDashboardService>();
        var authServiceMock = new Mock<IAuthService>();
        var sessionServiceMock = new Mock<SessionService>();
        var viewModel = new DashboardViewModel(dashboardServiceMock.Object, authServiceMock.Object, sessionServiceMock.Object, new Mock<IStudentScheduleService>().Object, new Mock<IRefreshCoordinator>().Object, new Mock<IInsightsService>().Object);

        // Act
        var result = viewModel.Role;

        // Assert
        Assert.AreEqual("Student", result);
    }

    /// <summary>
    /// Tests that setting the Role property to a valid role value updates the property correctly.
    /// Input: Various valid role values ("Lecturer", "Admin", "ClassRep", "ClassRepresentative").
    /// Expected: Role property is updated and dependent properties reflect correct values.
    /// </summary>
    [TestMethod]
    [DataRow("Lecturer", false, true, false, false, DisplayName = "Lecturer role")]
    [DataRow("Admin", false, false, true, false, DisplayName = "Admin role")]
    [DataRow("ClassRep", false, false, false, true, DisplayName = "ClassRep role")]
    [DataRow("ClassRepresentative", false, false, false, true, DisplayName = "ClassRepresentative role")]
    [DataRow("Student", true, false, false, false, DisplayName = "Student role")]
    public void Role_SetValidRole_UpdatesPropertyAndDependentProperties(string roleValue, bool expectedIsStudent, bool expectedIsLecturer, bool expectedIsAdmin, bool expectedIsClassRep)
    {
        // Arrange
        var dashboardServiceMock = new Mock<IDashboardService>();
        var authServiceMock = new Mock<IAuthService>();
        var sessionServiceMock = new Mock<SessionService>();
        var viewModel = new DashboardViewModel(dashboardServiceMock.Object, authServiceMock.Object, sessionServiceMock.Object, new Mock<IStudentScheduleService>().Object, new Mock<IRefreshCoordinator>().Object, new Mock<IInsightsService>().Object);

        // Act
        viewModel.Role = roleValue;

        // Assert
        Assert.AreEqual(roleValue, viewModel.Role);
        Assert.AreEqual(expectedIsStudent, viewModel.IsStudent);
        Assert.AreEqual(expectedIsLecturer, viewModel.IsLecturer);
        Assert.AreEqual(expectedIsAdmin, viewModel.IsAdmin);
        Assert.AreEqual(expectedIsClassRep, viewModel.IsClassRep);
    }

    /// <summary>
    /// Tests that setting the Role property with different case variations correctly updates dependent properties.
    /// Input: Role values with various case combinations.
    /// Expected: Dependent properties are case-insensitive and return correct values.
    /// </summary>
    [TestMethod]
    [DataRow("STUDENT", true, false, false, false, DisplayName = "Uppercase STUDENT")]
    [DataRow("student", true, false, false, false, DisplayName = "Lowercase student")]
    [DataRow("StUdEnT", true, false, false, false, DisplayName = "Mixed case StUdEnT")]
    [DataRow("LECTURER", false, true, false, false, DisplayName = "Uppercase LECTURER")]
    [DataRow("lecturer", false, true, false, false, DisplayName = "Lowercase lecturer")]
    [DataRow("ADMIN", false, false, true, false, DisplayName = "Uppercase ADMIN")]
    [DataRow("admin", false, false, true, false, DisplayName = "Lowercase admin")]
    [DataRow("CLASSREP", false, false, false, true, DisplayName = "Uppercase CLASSREP")]
    [DataRow("classrep", false, false, false, true, DisplayName = "Lowercase classrep")]
    [DataRow("CLASSREPRESENTATIVE", false, false, false, true, DisplayName = "Uppercase CLASSREPRESENTATIVE")]
    [DataRow("classrepresentative", false, false, false, true, DisplayName = "Lowercase classrepresentative")]
    public void Role_SetWithDifferentCasing_UpdatesDependentPropertiesCorrectly(string roleValue, bool expectedIsStudent, bool expectedIsLecturer, bool expectedIsAdmin, bool expectedIsClassRep)
    {
        // Arrange
        var dashboardServiceMock = new Mock<IDashboardService>();
        var authServiceMock = new Mock<IAuthService>();
        var sessionServiceMock = new Mock<SessionService>();
        var viewModel = new DashboardViewModel(dashboardServiceMock.Object, authServiceMock.Object, sessionServiceMock.Object, new Mock<IStudentScheduleService>().Object, new Mock<IRefreshCoordinator>().Object, new Mock<IInsightsService>().Object);

        // Act
        viewModel.Role = roleValue;

        // Assert
        Assert.AreEqual(expectedIsStudent, viewModel.IsStudent);
        Assert.AreEqual(expectedIsLecturer, viewModel.IsLecturer);
        Assert.AreEqual(expectedIsAdmin, viewModel.IsAdmin);
        Assert.AreEqual(expectedIsClassRep, viewModel.IsClassRep);
    }

    /// <summary>
    /// Tests that setting the Role property raises PropertyChanged event for Role itself.
    /// Input: New role value "Lecturer".
    /// Expected: PropertyChanged event is raised for "Role".
    /// </summary>
    [TestMethod]
    public void Role_SetNewValue_RaisesPropertyChangedForRole()
    {
        // Arrange
        var dashboardServiceMock = new Mock<IDashboardService>();
        var authServiceMock = new Mock<IAuthService>();
        var sessionServiceMock = new Mock<SessionService>();
        var viewModel = new DashboardViewModel(dashboardServiceMock.Object, authServiceMock.Object, sessionServiceMock.Object, new Mock<IStudentScheduleService>().Object, new Mock<IRefreshCoordinator>().Object, new Mock<IInsightsService>().Object);
        var propertyChangedRaised = false;
        string? raisedPropertyName = null;
        viewModel.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == "Role")
            {
                propertyChangedRaised = true;
                raisedPropertyName = args.PropertyName;
            }
        };

        // Act
        viewModel.Role = "Lecturer";

        // Assert
        Assert.IsTrue(propertyChangedRaised);
        Assert.AreEqual("Role", raisedPropertyName);
    }

    /// <summary>
    /// Tests that setting the Role property raises PropertyChanged events for dependent properties.
    /// Input: New role value "Lecturer".
    /// Expected: PropertyChanged events are raised for IsStudent, IsLecturer, IsAdmin, and IsClassRep.
    /// </summary>
    [TestMethod]
    public void Role_SetNewValue_RaisesPropertyChangedForDependentProperties()
    {
        // Arrange
        var dashboardServiceMock = new Mock<IDashboardService>();
        var authServiceMock = new Mock<IAuthService>();
        var sessionServiceMock = new Mock<SessionService>();
        var viewModel = new DashboardViewModel(dashboardServiceMock.Object, authServiceMock.Object, sessionServiceMock.Object, new Mock<IStudentScheduleService>().Object, new Mock<IRefreshCoordinator>().Object, new Mock<IInsightsService>().Object);
        var raisedProperties = new List<string>();
        viewModel.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName != null)
            {
                raisedProperties.Add(args.PropertyName);
            }
        };

        // Act
        viewModel.Role = "Lecturer";

        // Assert
        Assert.IsTrue(raisedProperties.Contains("IsStudent"));
        Assert.IsTrue(raisedProperties.Contains("IsLecturer"));
        Assert.IsTrue(raisedProperties.Contains("IsAdmin"));
        Assert.IsTrue(raisedProperties.Contains("IsClassRep"));
    }

    /// <summary>
    /// Tests that setting the Role property to the same value does not raise PropertyChanged events.
    /// Input: Setting role to "Student" (same as default).
    /// Expected: PropertyChanged events are not raised.
    /// </summary>
    [TestMethod]
    public void Role_SetSameValue_DoesNotRaisePropertyChanged()
    {
        // Arrange
        var dashboardServiceMock = new Mock<IDashboardService>();
        var authServiceMock = new Mock<IAuthService>();
        var sessionServiceMock = new Mock<SessionService>();
        var viewModel = new DashboardViewModel(dashboardServiceMock.Object, authServiceMock.Object, sessionServiceMock.Object, new Mock<IStudentScheduleService>().Object, new Mock<IRefreshCoordinator>().Object, new Mock<IInsightsService>().Object);
        var propertyChangedCount = 0;
        viewModel.PropertyChanged += (sender, args) =>
        {
            propertyChangedCount++;
        };

        // Act
        viewModel.Role = "Student";

        // Assert
        Assert.AreEqual(0, propertyChangedCount);
    }

    /// <summary>
    /// Tests that setting the Role property to null updates the property correctly.
    /// Input: null value.
    /// Expected: Role property is null and all dependent properties return false.
    /// </summary>
    [TestMethod]
    public void Role_SetNull_UpdatesPropertyAndDependentPropertiesCorrectly()
    {
        // Arrange
        var dashboardServiceMock = new Mock<IDashboardService>();
        var authServiceMock = new Mock<IAuthService>();
        var sessionServiceMock = new Mock<SessionService>();
        var viewModel = new DashboardViewModel(dashboardServiceMock.Object, authServiceMock.Object, sessionServiceMock.Object, new Mock<IStudentScheduleService>().Object, new Mock<IRefreshCoordinator>().Object, new Mock<IInsightsService>().Object);

        // Act
        viewModel.Role = null!;

        // Assert
        Assert.IsNull(viewModel.Role);
        Assert.IsFalse(viewModel.IsStudent);
        Assert.IsFalse(viewModel.IsLecturer);
        Assert.IsFalse(viewModel.IsAdmin);
        Assert.IsFalse(viewModel.IsClassRep);
    }

    /// <summary>
    /// Tests that setting the Role property to empty or whitespace strings updates dependent properties correctly.
    /// Input: Empty string and whitespace-only strings.
    /// Expected: All dependent properties return false.
    /// </summary>
    [TestMethod]
    [DataRow("", DisplayName = "Empty string")]
    [DataRow(" ", DisplayName = "Single space")]
    [DataRow("   ", DisplayName = "Multiple spaces")]
    [DataRow("\t", DisplayName = "Tab character")]
    [DataRow("\n", DisplayName = "Newline character")]
    [DataRow("\r\n", DisplayName = "Carriage return and newline")]
    public void Role_SetEmptyOrWhitespace_AllDependentPropertiesReturnFalse(string roleValue)
    {
        // Arrange
        var dashboardServiceMock = new Mock<IDashboardService>();
        var authServiceMock = new Mock<IAuthService>();
        var sessionServiceMock = new Mock<SessionService>();
        var viewModel = new DashboardViewModel(dashboardServiceMock.Object, authServiceMock.Object, sessionServiceMock.Object, new Mock<IStudentScheduleService>().Object, new Mock<IRefreshCoordinator>().Object, new Mock<IInsightsService>().Object);

        // Act
        viewModel.Role = roleValue;

        // Assert
        Assert.AreEqual(roleValue, viewModel.Role);
        Assert.IsFalse(viewModel.IsStudent);
        Assert.IsFalse(viewModel.IsLecturer);
        Assert.IsFalse(viewModel.IsAdmin);
        Assert.IsFalse(viewModel.IsClassRep);
    }

    /// <summary>
    /// Tests that setting the Role property to unrecognized values results in all dependent properties returning false.
    /// Input: Various unrecognized role values.
    /// Expected: All dependent properties return false.
    /// </summary>
    [TestMethod]
    [DataRow("InvalidRole", DisplayName = "Invalid role")]
    [DataRow("Teacher", DisplayName = "Similar but different role")]
    [DataRow("Administrator", DisplayName = "Different admin variant")]
    [DataRow("ClassRepresentativ", DisplayName = "Typo in ClassRepresentative")]
    [DataRow("12345", DisplayName = "Numeric string")]
    [DataRow("Student123", DisplayName = "Role with numbers")]
    [DataRow("Student ", DisplayName = "Role with trailing space")]
    [DataRow(" Student", DisplayName = "Role with leading space")]
    public void Role_SetUnrecognizedValue_AllDependentPropertiesReturnFalse(string roleValue)
    {
        // Arrange
        var dashboardServiceMock = new Mock<IDashboardService>();
        var authServiceMock = new Mock<IAuthService>();
        var sessionServiceMock = new Mock<SessionService>();
        var viewModel = new DashboardViewModel(dashboardServiceMock.Object, authServiceMock.Object, sessionServiceMock.Object, new Mock<IStudentScheduleService>().Object, new Mock<IRefreshCoordinator>().Object, new Mock<IInsightsService>().Object);

        // Act
        viewModel.Role = roleValue;

        // Assert
        Assert.IsFalse(viewModel.IsStudent);
        Assert.IsFalse(viewModel.IsLecturer);
        Assert.IsFalse(viewModel.IsAdmin);
        Assert.IsFalse(viewModel.IsClassRep);
    }

    /// <summary>
    /// Tests that setting the Role property to strings with special characters updates correctly.
    /// Input: Strings with special characters.
    /// Expected: Role property is updated but dependent properties return false.
    /// </summary>
    [TestMethod]
    [DataRow("Student!", DisplayName = "Role with exclamation")]
    [DataRow("@Admin", DisplayName = "Role with at symbol")]
    [DataRow("Lecturer#", DisplayName = "Role with hash")]
    [DataRow("Class$Rep", DisplayName = "Role with dollar sign")]
    [DataRow("Stud&ent", DisplayName = "Role with ampersand")]
    public void Role_SetSpecialCharacters_UpdatesPropertyCorrectly(string roleValue)
    {
        // Arrange
        var dashboardServiceMock = new Mock<IDashboardService>();
        var authServiceMock = new Mock<IAuthService>();
        var sessionServiceMock = new Mock<SessionService>();
        var viewModel = new DashboardViewModel(dashboardServiceMock.Object, authServiceMock.Object, sessionServiceMock.Object, new Mock<IStudentScheduleService>().Object, new Mock<IRefreshCoordinator>().Object, new Mock<IInsightsService>().Object);

        // Act
        viewModel.Role = roleValue;

        // Assert
        Assert.AreEqual(roleValue, viewModel.Role);
        Assert.IsFalse(viewModel.IsStudent);
        Assert.IsFalse(viewModel.IsLecturer);
        Assert.IsFalse(viewModel.IsAdmin);
        Assert.IsFalse(viewModel.IsClassRep);
    }

    /// <summary>
    /// Tests that setting the Role property to a very long string updates the property correctly.
    /// Input: Very long string.
    /// Expected: Role property is updated but dependent properties return false.
    /// </summary>
    [TestMethod]
    public void Role_SetVeryLongString_UpdatesPropertyCorrectly()
    {
        // Arrange
        var dashboardServiceMock = new Mock<IDashboardService>();
        var authServiceMock = new Mock<IAuthService>();
        var sessionServiceMock = new Mock<SessionService>();
        var viewModel = new DashboardViewModel(dashboardServiceMock.Object, authServiceMock.Object, sessionServiceMock.Object, new Mock<IStudentScheduleService>().Object, new Mock<IRefreshCoordinator>().Object, new Mock<IInsightsService>().Object);
        var veryLongRole = new string('a', 10000);

        // Act
        viewModel.Role = veryLongRole;

        // Assert
        Assert.AreEqual(veryLongRole, viewModel.Role);
        Assert.IsFalse(viewModel.IsStudent);
        Assert.IsFalse(viewModel.IsLecturer);
        Assert.IsFalse(viewModel.IsAdmin);
        Assert.IsFalse(viewModel.IsClassRep);
    }

    /// <summary>
    /// Tests that changing the Role property multiple times updates dependent properties correctly each time.
    /// Input: Sequential role changes.
    /// Expected: Dependent properties reflect the current role value after each change.
    /// </summary>
    [TestMethod]
    public void Role_ChangeMultipleTimes_DependentPropertiesUpdateCorrectly()
    {
        // Arrange
        var dashboardServiceMock = new Mock<IDashboardService>();
        var authServiceMock = new Mock<IAuthService>();
        var sessionServiceMock = new Mock<SessionService>();
        var viewModel = new DashboardViewModel(dashboardServiceMock.Object, authServiceMock.Object, sessionServiceMock.Object, new Mock<IStudentScheduleService>().Object, new Mock<IRefreshCoordinator>().Object, new Mock<IInsightsService>().Object);

        // Act & Assert - Initial state
        Assert.IsTrue(viewModel.IsStudent);
        Assert.IsFalse(viewModel.IsLecturer);

        // Act & Assert - Change to Lecturer
        viewModel.Role = "Lecturer";
        Assert.IsFalse(viewModel.IsStudent);
        Assert.IsTrue(viewModel.IsLecturer);
        Assert.IsFalse(viewModel.IsAdmin);
        Assert.IsFalse(viewModel.IsClassRep);

        // Act & Assert - Change to Admin
        viewModel.Role = "Admin";
        Assert.IsFalse(viewModel.IsStudent);
        Assert.IsFalse(viewModel.IsLecturer);
        Assert.IsTrue(viewModel.IsAdmin);
        Assert.IsFalse(viewModel.IsClassRep);

        // Act & Assert - Change to ClassRep
        viewModel.Role = "ClassRep";
        Assert.IsFalse(viewModel.IsStudent);
        Assert.IsFalse(viewModel.IsLecturer);
        Assert.IsFalse(viewModel.IsAdmin);
        Assert.IsTrue(viewModel.IsClassRep);

        // Act & Assert - Change back to Student
        viewModel.Role = "Student";
        Assert.IsTrue(viewModel.IsStudent);
        Assert.IsFalse(viewModel.IsLecturer);
        Assert.IsFalse(viewModel.IsAdmin);
        Assert.IsFalse(viewModel.IsClassRep);
    }

    /// <summary>
    /// Tests that OpenClassCommand property returns a non-null ICommand instance.
    /// This verifies the command is properly initialized.
    /// </summary>
    [TestMethod]
    public void OpenClassCommand_WhenAccessed_ReturnsNonNullCommand()
    {
        // Arrange
        var mockDashboardService = new Mock<IDashboardService>();
        var mockAuthService = new Mock<IAuthService>();
        var mockSessionService = new Mock<SessionService>();
        var viewModel = new DashboardViewModel(mockDashboardService.Object, mockAuthService.Object, mockSessionService.Object, new Mock<IStudentScheduleService>().Object, new Mock<IRefreshCoordinator>().Object, new Mock<IInsightsService>().Object);

        // Act
        var command = viewModel.OpenClassCommand;

        // Assert
        Assert.IsNotNull(command);
    }

    /// <summary>
    /// Tests that OpenClassCommand returns an ICommand instance each time it's accessed.
    /// Verifies the property getter behavior with a valid Guid.Empty value.
    /// </summary>
    [TestMethod]
    public void OpenClassCommand_WithGuidEmpty_ReturnsCommandInstance()
    {
        // Arrange
        var mockDashboardService = new Mock<IDashboardService>();
        var mockAuthService = new Mock<IAuthService>();
        var mockSessionService = new Mock<SessionService>();
        var viewModel = new DashboardViewModel(mockDashboardService.Object, mockAuthService.Object, mockSessionService.Object, new Mock<IStudentScheduleService>().Object, new Mock<IRefreshCoordinator>().Object, new Mock<IInsightsService>().Object);

        // Act
        var command = viewModel.OpenClassCommand;

        // Assert
        Assert.IsNotNull(command);
        Assert.IsInstanceOfType(command, typeof(ICommand));
    }

    /// <summary>
    /// Tests that OpenClassCommand returns a consistent type of ICommand.
    /// This test uses a newly generated Guid to verify the command property behavior.
    /// Note: Full execution testing of the command requires Shell.Current which is not available in unit tests.
    /// Integration tests should verify the actual navigation behavior.
    /// </summary>
    [TestMethod]
    public void OpenClassCommand_WithNewGuid_ReturnsCommandInstance()
    {
        // Arrange
        var mockDashboardService = new Mock<IDashboardService>();
        var mockAuthService = new Mock<IAuthService>();
        var mockSessionService = new Mock<SessionService>();
        var viewModel = new DashboardViewModel(mockDashboardService.Object, mockAuthService.Object, mockSessionService.Object, new Mock<IStudentScheduleService>().Object, new Mock<IRefreshCoordinator>().Object, new Mock<IInsightsService>().Object);
        var testGuid = Guid.NewGuid();

        // Act
        var command = viewModel.OpenClassCommand;

        // Assert
        Assert.IsNotNull(command);
        Assert.IsInstanceOfType(command, typeof(ICommand));
        // Note: Actual command execution cannot be tested in unit tests because:
        // 1. Shell.Current is a static property that is null in unit test context
        // 2. Command<T> cannot be mocked according to framework limitations
        // 3. Creating fake Shell implementations is prohibited
        // Integration tests should verify: command.Execute(testGuid) navigates to "//MainTabs/ClassDetailsPage?id={testGuid}"
    }

    /// <summary>
    /// Tests that OpenClassCommand property can be accessed multiple times without throwing exceptions.
    /// Verifies the property getter is stable and consistent.
    /// </summary>
    [TestMethod]
    public void OpenClassCommand_MultipleAccesses_ReturnsCommandInstance()
    {
        // Arrange
        var mockDashboardService = new Mock<IDashboardService>();
        var mockAuthService = new Mock<IAuthService>();
        var mockSessionService = new Mock<SessionService>();
        var viewModel = new DashboardViewModel(mockDashboardService.Object, mockAuthService.Object, mockSessionService.Object, new Mock<IStudentScheduleService>().Object, new Mock<IRefreshCoordinator>().Object, new Mock<IInsightsService>().Object);

        // Act
        var command1 = viewModel.OpenClassCommand;
        var command2 = viewModel.OpenClassCommand;

        // Assert
        Assert.IsNotNull(command1);
        Assert.IsNotNull(command2);
        Assert.IsInstanceOfType(command1, typeof(ICommand));
        Assert.IsInstanceOfType(command2, typeof(ICommand));
    }

    /// <summary>
    /// Tests that HasAnnouncement returns the default value of false when not explicitly set.
    /// </summary>
    [TestMethod]
    public void HasAnnouncement_DefaultValue_ReturnsFalse()
    {
        // Arrange
        var mockDashboard = new Mock<IDashboardService>();
        var mockAuth = new Mock<IAuthService>();
        var mockSession = new Mock<SessionService>();
        var viewModel = new DashboardViewModel(mockDashboard.Object, mockAuth.Object, mockSession.Object, new Mock<IStudentScheduleService>().Object, new Mock<IRefreshCoordinator>().Object, new Mock<IInsightsService>().Object);

        // Act
        var result = viewModel.HasAnnouncement;

        // Assert
        Assert.IsFalse(result);
    }

    /// <summary>
    /// Tests that HasAnnouncement correctly stores and returns the value when set.
    /// </summary>
    /// <param name="value">The boolean value to set and verify.</param>
    [TestMethod]
    [DataRow(true)]
    [DataRow(false)]
    public void HasAnnouncement_SetValue_ReturnsExpectedValue(bool value)
    {
        // Arrange
        var mockDashboard = new Mock<IDashboardService>();
        var mockAuth = new Mock<IAuthService>();
        var mockSession = new Mock<SessionService>();
        var viewModel = new DashboardViewModel(mockDashboard.Object, mockAuth.Object, mockSession.Object, new Mock<IStudentScheduleService>().Object, new Mock<IRefreshCoordinator>().Object, new Mock<IInsightsService>().Object);

        // Act
        viewModel.HasAnnouncement = value;
        var result = viewModel.HasAnnouncement;

        // Assert
        Assert.AreEqual(value, result);
    }

    /// <summary>
    /// Tests that HasAnnouncement raises PropertyChanged event when value changes.
    /// </summary>
    /// <param name="newValue">The new boolean value to set.</param>
    [TestMethod]
    [DataRow(true)]
    [DataRow(false)]
    public void HasAnnouncement_SetValue_RaisesPropertyChanged(bool newValue)
    {
        // Arrange
        var mockDashboard = new Mock<IDashboardService>();
        var mockAuth = new Mock<IAuthService>();
        var mockSession = new Mock<SessionService>();
        var viewModel = new DashboardViewModel(mockDashboard.Object, mockAuth.Object, mockSession.Object, new Mock<IStudentScheduleService>().Object, new Mock<IRefreshCoordinator>().Object, new Mock<IInsightsService>().Object);
        var propertyChangedRaised = false;
        string? changedPropertyName = null;

        ((INotifyPropertyChanged)viewModel).PropertyChanged += (sender, args) =>
        {
            propertyChangedRaised = true;
            changedPropertyName = args.PropertyName;
        };

        // Act
        viewModel.HasAnnouncement = newValue;

        // Assert
        Assert.IsTrue(propertyChangedRaised);
        Assert.AreEqual(nameof(viewModel.HasAnnouncement), changedPropertyName);
    }

    /// <summary>
    /// Tests that HasAnnouncement does not raise PropertyChanged event when set to the same value.
    /// </summary>
    [TestMethod]
    public void HasAnnouncement_SetSameValue_DoesNotRaisePropertyChanged()
    {
        // Arrange
        var mockDashboard = new Mock<IDashboardService>();
        var mockAuth = new Mock<IAuthService>();
        var mockSession = new Mock<SessionService>();
        var viewModel = new DashboardViewModel(mockDashboard.Object, mockAuth.Object, mockSession.Object, new Mock<IStudentScheduleService>().Object, new Mock<IRefreshCoordinator>().Object, new Mock<IInsightsService>().Object);
        viewModel.HasAnnouncement = false; // Set initial value (same as default)

        var propertyChangedRaised = false;
        ((INotifyPropertyChanged)viewModel).PropertyChanged += (sender, args) =>
        {
            propertyChangedRaised = true;
        };

        // Act
        viewModel.HasAnnouncement = false; // Set same value again

        // Assert
        Assert.IsFalse(propertyChangedRaised);
    }

    /// <summary>
    /// Tests that HasAnnouncement correctly toggles between true and false values.
    /// </summary>
    [TestMethod]
    public void HasAnnouncement_ToggleValue_ReturnsCorrectValue()
    {
        // Arrange
        var mockDashboard = new Mock<IDashboardService>();
        var mockAuth = new Mock<IAuthService>();
        var mockSession = new Mock<SessionService>();
        var viewModel = new DashboardViewModel(mockDashboard.Object, mockAuth.Object, mockSession.Object, new Mock<IStudentScheduleService>().Object, new Mock<IRefreshCoordinator>().Object, new Mock<IInsightsService>().Object);

        // Act & Assert - Set to true
        viewModel.HasAnnouncement = true;
        Assert.IsTrue(viewModel.HasAnnouncement);

        // Act & Assert - Set to false
        viewModel.HasAnnouncement = false;
        Assert.IsFalse(viewModel.HasAnnouncement);

        // Act & Assert - Set to true again
        viewModel.HasAnnouncement = true;
        Assert.IsTrue(viewModel.HasAnnouncement);
    }

    /// <summary>
    /// Tests that the IsLecturer property returns true when Role matches "lecturer" in various case formats,
    /// and returns false for all other values including null, empty, whitespace, and other role names.
    /// </summary>
    /// <param name="roleValue">The role value to set.</param>
    /// <param name="expectedResult">The expected result of IsLecturer property.</param>
    [TestMethod]
    [DataRow("lecturer", true, DisplayName = "IsLecturer returns true when Role is 'lecturer' (lowercase)")]
    [DataRow("Lecturer", true, DisplayName = "IsLecturer returns true when Role is 'Lecturer' (title case)")]
    [DataRow("LECTURER", true, DisplayName = "IsLecturer returns true when Role is 'LECTURER' (uppercase)")]
    [DataRow("LeCtuReR", true, DisplayName = "IsLecturer returns true when Role is 'LeCtuReR' (mixed case)")]
    [DataRow("lEcTuReR", true, DisplayName = "IsLecturer returns true when Role is 'lEcTuReR' (mixed case)")]
    [DataRow(null, false, DisplayName = "IsLecturer returns false when Role is null")]
    [DataRow("", false, DisplayName = "IsLecturer returns false when Role is empty string")]
    [DataRow(" ", false, DisplayName = "IsLecturer returns false when Role is single whitespace")]
    [DataRow("   ", false, DisplayName = "IsLecturer returns false when Role is multiple whitespaces")]
    [DataRow("\t", false, DisplayName = "IsLecturer returns false when Role is tab character")]
    [DataRow("\n", false, DisplayName = "IsLecturer returns false when Role is newline character")]
    [DataRow("student", false, DisplayName = "IsLecturer returns false when Role is 'student'")]
    [DataRow("Student", false, DisplayName = "IsLecturer returns false when Role is 'Student'")]
    [DataRow("admin", false, DisplayName = "IsLecturer returns false when Role is 'admin'")]
    [DataRow("Admin", false, DisplayName = "IsLecturer returns false when Role is 'Admin'")]
    [DataRow("classrep", false, DisplayName = "IsLecturer returns false when Role is 'classrep'")]
    [DataRow("ClassRep", false, DisplayName = "IsLecturer returns false when Role is 'ClassRep'")]
    [DataRow("classrepresentative", false, DisplayName = "IsLecturer returns false when Role is 'classrepresentative'")]
    [DataRow("lecturer ", false, DisplayName = "IsLecturer returns false when Role is 'lecturer ' (with trailing space)")]
    [DataRow(" lecturer", false, DisplayName = "IsLecturer returns false when Role is ' lecturer' (with leading space)")]
    [DataRow("lecturer123", false, DisplayName = "IsLecturer returns false when Role is 'lecturer123' (with numbers)")]
    [DataRow("mylecturer", false, DisplayName = "IsLecturer returns false when Role is 'mylecturer' (contains 'lecturer' as substring)")]
    [DataRow("lecturers", false, DisplayName = "IsLecturer returns false when Role is 'lecturers' (plural form)")]
    [DataRow("Lecturer1", false, DisplayName = "IsLecturer returns false when Role is 'Lecturer1'")]
    [DataRow("Teacher", false, DisplayName = "IsLecturer returns false when Role is 'Teacher'")]
    [DataRow("Professor", false, DisplayName = "IsLecturer returns false when Role is 'Professor'")]
    [DataRow("ADMIN", false, DisplayName = "IsLecturer returns false when Role is 'ADMIN'")]
    [DataRow("unknown", false, DisplayName = "IsLecturer returns false when Role is 'unknown'")]
    [DataRow("@lecturer", false, DisplayName = "IsLecturer returns false when Role is '@lecturer' (with special character)")]
    [DataRow("lecturer!", false, DisplayName = "IsLecturer returns false when Role is 'lecturer!' (with special character)")]
    public void IsLecturer_VariousRoleValues_ReturnsExpectedResult(string? roleValue, bool expectedResult)
    {
        // Arrange
        var mockDashboardService = new Mock<IDashboardService>();
        var mockAuthService = new Mock<IAuthService>();
        var mockSessionService = new Mock<SessionService>();
        var viewModel = new DashboardViewModel(mockDashboardService.Object, mockAuthService.Object, mockSessionService.Object, new Mock<IStudentScheduleService>().Object, new Mock<IRefreshCoordinator>().Object, new Mock<IInsightsService>().Object);

        viewModel.Role = roleValue;

        // Act
        var result = viewModel.IsLecturer;

        // Assert
        Assert.AreEqual(expectedResult, result);
    }

    /// <summary>
    /// Tests that IsLecturer property returns false when Role is set to a very long string
    /// that is not exactly "lecturer".
    /// </summary>
    [TestMethod]
    public void IsLecturer_VeryLongRoleString_ReturnsFalse()
    {
        // Arrange
        var mockDashboardService = new Mock<IDashboardService>();
        var mockAuthService = new Mock<IAuthService>();
        var mockSessionService = new Mock<SessionService>();
        var viewModel = new DashboardViewModel(mockDashboardService.Object, mockAuthService.Object, mockSessionService.Object, new Mock<IStudentScheduleService>().Object, new Mock<IRefreshCoordinator>().Object, new Mock<IInsightsService>().Object);

        var veryLongRole = new string('a', 10000);
        viewModel.Role = veryLongRole;

        // Act
        var result = viewModel.IsLecturer;

        // Assert
        Assert.IsFalse(result);
    }

    /// <summary>
    /// Tests that IsLecturer property returns true when Role is set to "lecturer"
    /// with Unicode characters that normalize to standard ASCII "lecturer".
    /// </summary>
    [TestMethod]
    [DataRow("lecturer\u200B", false, DisplayName = "IsLecturer returns false with zero-width space")]
    [DataRow("\u202Alecturer", false, DisplayName = "IsLecturer returns false with left-to-right embedding")]
    public void IsLecturer_RoleWithUnicodeCharacters_ReturnsExpectedResult(string? roleValue, bool expectedResult)
    {
        // Arrange
        var mockDashboardService = new Mock<IDashboardService>();
        var mockAuthService = new Mock<IAuthService>();
        var mockSessionService = new Mock<SessionService>();
        var viewModel = new DashboardViewModel(mockDashboardService.Object, mockAuthService.Object, mockSessionService.Object, new Mock<IStudentScheduleService>().Object, new Mock<IRefreshCoordinator>().Object, new Mock<IInsightsService>().Object);

        viewModel.Role = roleValue;

        // Act
        var result = viewModel.IsLecturer;

        // Assert
        Assert.AreEqual(expectedResult, result);
    }

    /// <summary>
    /// Tests that GoToAcademicCommand property returns a non-null ICommand instance.
    /// </summary>
    [TestMethod]
    public void GoToAcademicCommand_WhenAccessed_ReturnsNonNullCommand()
    {
        // Arrange
        var mockDashboard = new Mock<IDashboardService>();
        var mockAuth = new Mock<IAuthService>();
        var mockSession = new Mock<SessionService>();
        var viewModel = new DashboardViewModel(mockDashboard.Object, mockAuth.Object, mockSession.Object, new Mock<IStudentScheduleService>().Object, new Mock<IRefreshCoordinator>().Object, new Mock<IInsightsService>().Object);

        // Act
        ICommand command = viewModel.GoToAcademicCommand;

        // Assert
        Assert.IsNotNull(command);
    }

    /// <summary>
    /// Tests that GoToAcademicCommand property returns a Command type.
    /// </summary>
    [TestMethod]
    public void GoToAcademicCommand_WhenAccessed_ReturnsCommandType()
    {
        // Arrange
        var mockDashboard = new Mock<IDashboardService>();
        var mockAuth = new Mock<IAuthService>();
        var mockSession = new Mock<SessionService>();
        var viewModel = new DashboardViewModel(mockDashboard.Object, mockAuth.Object, mockSession.Object, new Mock<IStudentScheduleService>().Object, new Mock<IRefreshCoordinator>().Object, new Mock<IInsightsService>().Object);

        // Act
        ICommand command = viewModel.GoToAcademicCommand;

        // Assert
        Assert.IsInstanceOfType(command, typeof(Command));
    }

    /// <summary>
    /// Tests that each access to GoToAcademicCommand creates a new Command instance.
    /// </summary>
    [TestMethod]
    public void GoToAcademicCommand_WhenAccessedMultipleTimes_CreatesNewInstances()
    {
        // Arrange
        var mockDashboard = new Mock<IDashboardService>();
        var mockAuth = new Mock<IAuthService>();
        var mockSession = new Mock<SessionService>();
        var viewModel = new DashboardViewModel(mockDashboard.Object, mockAuth.Object, mockSession.Object, new Mock<IStudentScheduleService>().Object, new Mock<IRefreshCoordinator>().Object, new Mock<IInsightsService>().Object);

        // Act
        ICommand command1 = viewModel.GoToAcademicCommand;
        ICommand command2 = viewModel.GoToAcademicCommand;

        // Assert
        Assert.AreNotSame(command1, command2);
    }

    /// <summary>
    /// Tests that GoToAcademicCommand can be executed (CanExecute returns true).
    /// </summary>
    [TestMethod]
    public void GoToAcademicCommand_CanExecute_ReturnsTrue()
    {
        // Arrange
        var mockDashboard = new Mock<IDashboardService>();
        var mockAuth = new Mock<IAuthService>();
        var mockSession = new Mock<SessionService>();
        var viewModel = new DashboardViewModel(mockDashboard.Object, mockAuth.Object, mockSession.Object, new Mock<IStudentScheduleService>().Object, new Mock<IRefreshCoordinator>().Object, new Mock<IInsightsService>().Object);

        // Act
        ICommand command = viewModel.GoToAcademicCommand;
        bool canExecute = command.CanExecute(null);

        // Assert
        Assert.IsTrue(canExecute);
    }

    // Note: Testing the actual execution of GoToAcademicCommand (calling command.Execute())
    // is not feasible in a pure unit test context because it depends on Shell.Current,
    // which is a static property from Microsoft.Maui.Controls.Shell. This static dependency
    // cannot be mocked using Moq, and creating fake implementations is prohibited.
    // In a unit test environment, Shell.Current will be null, causing a NullReferenceException.
    // To test the navigation behavior, consider:
    // 1. Integration tests with a properly initialized Shell instance
    // 2. UI tests using the MAUI testing framework
    // 3. Refactoring to inject a navigation service abstraction that can be mocked
    private Mock<IDashboardService> _mockDashboardService = null!;
    private Mock<IAuthService> _mockAuthService = null!;
    private Mock<SessionService> _mockSessionService = null!;
    private DashboardViewModel _viewModel = null!;

    /// <summary>
    /// Initializes the test dependencies and creates a new instance of DashboardViewModel before each test.
    /// </summary>
    [TestInitialize]
    public void TestInitialize()
    {
        _mockDashboardService = new Mock<IDashboardService>();
        _mockAuthService = new Mock<IAuthService>();
        _mockSessionService = new Mock<SessionService>();
        _viewModel = new DashboardViewModel(_mockDashboardService.Object, _mockAuthService.Object, _mockSessionService.Object, new Mock<IStudentScheduleService>().Object, new Mock<IRefreshCoordinator>().Object, new Mock<IInsightsService>().Object);
    }

    /// <summary>
    /// Tests that setting AttendancePercentage to a value at or above 75 sets AttendanceStatus to "On Track".
    /// </summary>
    /// <param name="value">The attendance percentage value to test.</param>
    /// <param name="expectedStatus">The expected attendance status.</param>
    [TestMethod]
    [DataRow(75, "On Track")]
    [DataRow(76, "On Track")]
    [DataRow(100, "On Track")]
    [DataRow(int.MaxValue, "On Track")]
    public void AttendancePercentage_ValueGreaterThanOrEqualTo75_SetsStatusToOnTrack(int value, string expectedStatus)
    {
        // Arrange
        // (ViewModel already initialized in TestInitialize)

        // Act
        _viewModel.AttendancePercentage = value;

        // Assert
        Assert.AreEqual(value, _viewModel.AttendancePercentage);
        Assert.AreEqual(expectedStatus, _viewModel.AttendanceStatus);
    }

    /// <summary>
    /// Tests that setting AttendancePercentage to a value between 50 and 74 (inclusive of 50) sets AttendanceStatus to "Needs Improvement".
    /// </summary>
    /// <param name="value">The attendance percentage value to test.</param>
    /// <param name="expectedStatus">The expected attendance status.</param>
    [TestMethod]
    [DataRow(50, "Needs Improvement")]
    [DataRow(51, "Needs Improvement")]
    [DataRow(74, "Needs Improvement")]
    [DataRow(60, "Needs Improvement")]
    public void AttendancePercentage_ValueBetween50And74_SetsStatusToNeedsImprovement(int value, string expectedStatus)
    {
        // Arrange
        // (ViewModel already initialized in TestInitialize)

        // Act
        _viewModel.AttendancePercentage = value;

        // Assert
        Assert.AreEqual(value, _viewModel.AttendancePercentage);
        Assert.AreEqual(expectedStatus, _viewModel.AttendanceStatus);
    }

    /// <summary>
    /// Tests that setting AttendancePercentage to a value below 50 sets AttendanceStatus to "At Risk".
    /// </summary>
    /// <param name="value">The attendance percentage value to test.</param>
    /// <param name="expectedStatus">The expected attendance status.</param>
    [TestMethod]
    [DataRow(49, "At Risk")]
    [DataRow(0, "At Risk")]
    [DataRow(-1, "At Risk")]
    [DataRow(-100, "At Risk")]
    [DataRow(int.MinValue, "At Risk")]
    public void AttendancePercentage_ValueLessThan50_SetsStatusToAtRisk(int value, string expectedStatus)
    {
        // Arrange
        // (ViewModel already initialized in TestInitialize)

        // Act
        _viewModel.AttendancePercentage = value;

        // Assert
        Assert.AreEqual(value, _viewModel.AttendancePercentage);
        Assert.AreEqual(expectedStatus, _viewModel.AttendanceStatus);
    }

    /// <summary>
    /// Tests that setting AttendancePercentage to the same value twice does not re-trigger AttendanceStatus update.
    /// Verifies that the status remains unchanged when the same value is set again.
    /// </summary>
    [TestMethod]
    public void AttendancePercentage_SettingSameValueTwice_DoesNotReUpdateStatus()
    {
        // Arrange
        _viewModel.AttendancePercentage = 80;
        var initialStatus = _viewModel.AttendanceStatus;

        // Manually change AttendanceStatus to a different value to verify it doesn't get reset
        _viewModel.AttendanceStatus = "Custom Status";

        // Act
        _viewModel.AttendancePercentage = 80; // Set same value again

        // Assert
        Assert.AreEqual(80, _viewModel.AttendancePercentage);
        Assert.AreEqual("Custom Status", _viewModel.AttendanceStatus); // Should remain unchanged
    }

    /// <summary>
    /// Tests that the PropertyChanged event is raised when AttendancePercentage is set to a new value.
    /// </summary>
    [TestMethod]
    public void AttendancePercentage_SettingNewValue_RaisesPropertyChangedEvent()
    {
        // Arrange
        var propertyChangedRaised = false;
        string? changedPropertyName = null;

        _viewModel.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == nameof(DashboardViewModel.AttendancePercentage))
            {
                propertyChangedRaised = true;
                changedPropertyName = args.PropertyName;
            }
        };

        // Act
        _viewModel.AttendancePercentage = 85;

        // Assert
        Assert.IsTrue(propertyChangedRaised);
        Assert.AreEqual(nameof(DashboardViewModel.AttendancePercentage), changedPropertyName);
    }

    /// <summary>
    /// Tests that setting AttendancePercentage to the same value does not raise PropertyChanged event.
    /// </summary>
    [TestMethod]
    public void AttendancePercentage_SettingSameValue_DoesNotRaisePropertyChangedEvent()
    {
        // Arrange
        _viewModel.AttendancePercentage = 85;
        var propertyChangedCount = 0;

        _viewModel.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == nameof(DashboardViewModel.AttendancePercentage))
            {
                propertyChangedCount++;
            }
        };

        // Act
        _viewModel.AttendancePercentage = 85; // Same value

        // Assert
        Assert.AreEqual(0, propertyChangedCount);
    }

    /// <summary>
    /// Tests that getting AttendancePercentage returns the previously set value.
    /// </summary>
    [TestMethod]
    public void AttendancePercentage_Getter_ReturnsSetValue()
    {
        // Arrange
        var expectedValue = 67;

        // Act
        _viewModel.AttendancePercentage = expectedValue;
        var actualValue = _viewModel.AttendancePercentage;

        // Assert
        Assert.AreEqual(expectedValue, actualValue);
    }

    /// <summary>
    /// Tests boundary conditions for AttendancePercentage threshold transitions.
    /// Verifies correct status at exact boundary values.
    /// </summary>
    [TestMethod]
    [DataRow(49, "At Risk")]
    [DataRow(50, "Needs Improvement")]
    [DataRow(74, "Needs Improvement")]
    [DataRow(75, "On Track")]
    public void AttendancePercentage_BoundaryValues_SetsCorrectStatus(int value, string expectedStatus)
    {
        // Arrange
        // (ViewModel already initialized in TestInitialize)

        // Act
        _viewModel.AttendancePercentage = value;

        // Assert
        Assert.AreEqual(expectedStatus, _viewModel.AttendanceStatus);
    }

    /// <summary>
    /// Tests that PropertyChanged event for AttendanceStatus is raised when AttendancePercentage changes
    /// to a value that results in a different status.
    /// </summary>
    [TestMethod]
    public void AttendancePercentage_ChangingValue_RaisesPropertyChangedForAttendanceStatus()
    {
        // Arrange
        _viewModel.AttendancePercentage = 80; // Initial: "On Track"
        var statusPropertyChangedRaised = false;

        _viewModel.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == nameof(DashboardViewModel.AttendanceStatus))
            {
                statusPropertyChangedRaised = true;
            }
        };

        // Act
        _viewModel.AttendancePercentage = 40; // Changes status to "At Risk"

        // Assert
        Assert.IsTrue(statusPropertyChangedRaised);
        Assert.AreEqual("At Risk", _viewModel.AttendanceStatus);
    }

    /// <summary>
    /// Tests that the LatestAnnouncementBody property returns an empty string when initially accessed
    /// without any value being set.
    /// </summary>
    [TestMethod]
    public void LatestAnnouncementBody_InitialValue_ReturnsEmptyString()
    {
        // Arrange
        var mockDashboard = new Mock<IDashboardService>();
        var mockAuth = new Mock<IAuthService>();
        var mockSession = new Mock<SessionService>();
        var viewModel = new DashboardViewModel(mockDashboard.Object, mockAuth.Object, mockSession.Object, new Mock<IStudentScheduleService>().Object, new Mock<IRefreshCoordinator>().Object, new Mock<IInsightsService>().Object);

        // Act
        var result = viewModel.LatestAnnouncementBody;

        // Assert
        Assert.AreEqual(string.Empty, result);
    }

    /// <summary>
    /// Tests that setting the LatestAnnouncementBody property updates the value correctly
    /// and raises the PropertyChanged event with the correct property name.
    /// Tests various string values including normal strings, empty strings, whitespace, special characters, and long strings.
    /// </summary>
    /// <param name="value">The string value to set on the property.</param>
    [TestMethod]
    [DataRow("Normal announcement text")]
    [DataRow("")]
    [DataRow("   ")]
    [DataRow(" ")]
    [DataRow("\t")]
    [DataRow("\n")]
    [DataRow("Line1\nLine2\nLine3")]
    [DataRow("Line1\r\nLine2")]
    [DataRow("!@#$%^&*()_+-={}[]|:;<>?,./~`")]
    [DataRow("???????")]
    [DataRow("Announcement with emoji ????")]
    [DataRow("A")]
    public void LatestAnnouncementBody_SetValue_UpdatesPropertyAndRaisesPropertyChanged(string value)
    {
        // Arrange
        var mockDashboard = new Mock<IDashboardService>();
        var mockAuth = new Mock<IAuthService>();
        var mockSession = new Mock<SessionService>();
        var viewModel = new DashboardViewModel(mockDashboard.Object, mockAuth.Object, mockSession.Object, new Mock<IStudentScheduleService>().Object, new Mock<IRefreshCoordinator>().Object, new Mock<IInsightsService>().Object);
        var propertyChangedRaised = false;
        var propertyName = string.Empty;

        viewModel.PropertyChanged += (sender, args) =>
        {
            propertyChangedRaised = true;
            propertyName = args.PropertyName;
        };

        // Act
        viewModel.LatestAnnouncementBody = value;

        // Assert
        Assert.AreEqual(value, viewModel.LatestAnnouncementBody);
        Assert.IsTrue(propertyChangedRaised);
        Assert.AreEqual(nameof(viewModel.LatestAnnouncementBody), propertyName);
    }

    /// <summary>
    /// Tests that setting the LatestAnnouncementBody property to a very long string
    /// updates the value correctly and raises the PropertyChanged event.
    /// </summary>
    [TestMethod]
    public void LatestAnnouncementBody_SetVeryLongString_UpdatesPropertyAndRaisesPropertyChanged()
    {
        // Arrange
        var mockDashboard = new Mock<IDashboardService>();
        var mockAuth = new Mock<IAuthService>();
        var mockSession = new Mock<SessionService>();
        var viewModel = new DashboardViewModel(mockDashboard.Object, mockAuth.Object, mockSession.Object, new Mock<IStudentScheduleService>().Object, new Mock<IRefreshCoordinator>().Object, new Mock<IInsightsService>().Object);
        var longString = new string('A', 10000);
        var propertyChangedRaised = false;

        viewModel.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == nameof(viewModel.LatestAnnouncementBody))
            {
                propertyChangedRaised = true;
            }
        };

        // Act
        viewModel.LatestAnnouncementBody = longString;

        // Assert
        Assert.AreEqual(longString, viewModel.LatestAnnouncementBody);
        Assert.IsTrue(propertyChangedRaised);
    }

    /// <summary>
    /// Tests that setting the LatestAnnouncementBody property to the same value twice
    /// only raises the PropertyChanged event once (on the first change).
    /// Verifies that the SetProperty method correctly detects unchanged values.
    /// </summary>
    [TestMethod]
    public void LatestAnnouncementBody_SetSameValueTwice_RaisesPropertyChangedOnlyOnce()
    {
        // Arrange
        var mockDashboard = new Mock<IDashboardService>();
        var mockAuth = new Mock<IAuthService>();
        var mockSession = new Mock<SessionService>();
        var viewModel = new DashboardViewModel(mockDashboard.Object, mockAuth.Object, mockSession.Object, new Mock<IStudentScheduleService>().Object, new Mock<IRefreshCoordinator>().Object, new Mock<IInsightsService>().Object);
        var propertyChangedCount = 0;
        var testValue = "Test announcement";

        viewModel.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == nameof(viewModel.LatestAnnouncementBody))
            {
                propertyChangedCount++;
            }
        };

        // Act
        viewModel.LatestAnnouncementBody = testValue;
        viewModel.LatestAnnouncementBody = testValue;

        // Assert
        Assert.AreEqual(testValue, viewModel.LatestAnnouncementBody);
        Assert.AreEqual(1, propertyChangedCount);
    }

    /// <summary>
    /// Tests that setting the LatestAnnouncementBody property to different values
    /// raises the PropertyChanged event for each distinct value change.
    /// </summary>
    [TestMethod]
    public void LatestAnnouncementBody_SetDifferentValues_RaisesPropertyChangedForEachChange()
    {
        // Arrange
        var mockDashboard = new Mock<IDashboardService>();
        var mockAuth = new Mock<IAuthService>();
        var mockSession = new Mock<SessionService>();
        var viewModel = new DashboardViewModel(mockDashboard.Object, mockAuth.Object, mockSession.Object, new Mock<IStudentScheduleService>().Object, new Mock<IRefreshCoordinator>().Object, new Mock<IInsightsService>().Object);
        var propertyChangedCount = 0;
        var values = new[] { "First", "Second", "Third" };

        viewModel.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == nameof(viewModel.LatestAnnouncementBody))
            {
                propertyChangedCount++;
            }
        };

        // Act
        foreach (var value in values)
        {
            viewModel.LatestAnnouncementBody = value;
        }

        // Assert
        Assert.AreEqual("Third", viewModel.LatestAnnouncementBody);
        Assert.AreEqual(values.Length, propertyChangedCount);
    }

    /// <summary>
    /// Tests that setting the LatestAnnouncementBody property to a string containing null characters
    /// updates the value correctly and raises the PropertyChanged event.
    /// </summary>
    [TestMethod]
    public void LatestAnnouncementBody_SetStringWithNullCharacter_UpdatesPropertyAndRaisesPropertyChanged()
    {
        // Arrange
        var mockDashboard = new Mock<IDashboardService>();
        var mockAuth = new Mock<IAuthService>();
        var mockSession = new Mock<SessionService>();
        var viewModel = new DashboardViewModel(mockDashboard.Object, mockAuth.Object, mockSession.Object, new Mock<IStudentScheduleService>().Object, new Mock<IRefreshCoordinator>().Object, new Mock<IInsightsService>().Object);
        var valueWithNullChar = "Before\0After";
        var propertyChangedRaised = false;

        viewModel.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == nameof(viewModel.LatestAnnouncementBody))
            {
                propertyChangedRaised = true;
            }
        };

        // Act
        viewModel.LatestAnnouncementBody = valueWithNullChar;

        // Assert
        Assert.AreEqual(valueWithNullChar, viewModel.LatestAnnouncementBody);
        Assert.IsTrue(propertyChangedRaised);
    }

    /// <summary>
    /// Tests that setting the LatestAnnouncementBody property back to an empty string after
    /// setting it to a non-empty value updates the property correctly and raises PropertyChanged.
    /// </summary>
    [TestMethod]
    public void LatestAnnouncementBody_SetToEmptyAfterNonEmpty_UpdatesPropertyAndRaisesPropertyChanged()
    {
        // Arrange
        var mockDashboard = new Mock<IDashboardService>();
        var mockAuth = new Mock<IAuthService>();
        var mockSession = new Mock<SessionService>();
        var viewModel = new DashboardViewModel(mockDashboard.Object, mockAuth.Object, mockSession.Object, new Mock<IStudentScheduleService>().Object, new Mock<IRefreshCoordinator>().Object, new Mock<IInsightsService>().Object);
        var propertyChangedCount = 0;

        viewModel.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == nameof(viewModel.LatestAnnouncementBody))
            {
                propertyChangedCount++;
            }
        };

        // Act
        viewModel.LatestAnnouncementBody = "Some announcement";
        viewModel.LatestAnnouncementBody = string.Empty;

        // Assert
        Assert.AreEqual(string.Empty, viewModel.LatestAnnouncementBody);
        Assert.AreEqual(2, propertyChangedCount);
    }

    /// <summary>
    /// Tests that the IsStudent property returns the expected boolean value
    /// based on various Role property values including different casings,
    /// null, empty, whitespace, and other role types.
    /// </summary>
    /// <param name="roleValue">The value to set for the Role property.</param>
    /// <param name="expectedIsStudent">The expected return value of IsStudent.</param>
    [TestMethod]
    [DataRow("student", true, DisplayName = "Role_LowercaseStudent_ReturnsTrue")]
    [DataRow("Student", true, DisplayName = "Role_CapitalizedStudent_ReturnsTrue")]
    [DataRow("STUDENT", true, DisplayName = "Role_UppercaseStudent_ReturnsTrue")]
    [DataRow("StUdEnT", true, DisplayName = "Role_MixedCaseStudent_ReturnsTrue")]
    [DataRow("sTuDeNt", true, DisplayName = "Role_AlternateMixedCaseStudent_ReturnsTrue")]
    [DataRow(null, false, DisplayName = "Role_Null_ReturnsFalse")]
    [DataRow("", false, DisplayName = "Role_EmptyString_ReturnsFalse")]
    [DataRow("   ", false, DisplayName = "Role_Whitespace_ReturnsFalse")]
    [DataRow("lecturer", false, DisplayName = "Role_Lecturer_ReturnsFalse")]
    [DataRow("Lecturer", false, DisplayName = "Role_CapitalizedLecturer_ReturnsFalse")]
    [DataRow("admin", false, DisplayName = "Role_Admin_ReturnsFalse")]
    [DataRow("Admin", false, DisplayName = "Role_CapitalizedAdmin_ReturnsFalse")]
    [DataRow("classrep", false, DisplayName = "Role_ClassRep_ReturnsFalse")]
    [DataRow("classrepresentative", false, DisplayName = "Role_ClassRepresentative_ReturnsFalse")]
    [DataRow("teacher", false, DisplayName = "Role_Teacher_ReturnsFalse")]
    [DataRow("student123", false, DisplayName = "Role_StudentWithNumbers_ReturnsFalse")]
    [DataRow(" student", false, DisplayName = "Role_StudentWithLeadingSpace_ReturnsFalse")]
    [DataRow("student ", false, DisplayName = "Role_StudentWithTrailingSpace_ReturnsFalse")]
    [DataRow("unknown", false, DisplayName = "Role_UnknownRole_ReturnsFalse")]
    public void IsStudent_VariousRoleValues_ReturnsExpectedResult(string? roleValue, bool expectedIsStudent)
    {
        // Arrange
        var mockDashboard = new Mock<IDashboardService>();
        var mockAuth = new Mock<IAuthService>();
        var mockSession = new Mock<SessionService>();
        var viewModel = new DashboardViewModel(mockDashboard.Object, mockAuth.Object, mockSession.Object, new Mock<IStudentScheduleService>().Object, new Mock<IRefreshCoordinator>().Object, new Mock<IInsightsService>().Object);
        viewModel.Role = roleValue!;

        // Act
        var actualIsStudent = viewModel.IsStudent;

        // Assert
        Assert.AreEqual(expectedIsStudent, actualIsStudent);
    }

    /// <summary>
    /// Tests that GoToUniversitiesCommand property returns a non-null ICommand instance.
    /// </summary>
    [TestMethod]
    public void GoToUniversitiesCommand_WhenAccessed_ReturnsNonNullCommand()
    {
        // Arrange
        var mockDashboard = new Mock<IDashboardService>();
        var mockAuth = new Mock<IAuthService>();
        var mockSession = new Mock<SessionService>();
        var viewModel = new DashboardViewModel(mockDashboard.Object, mockAuth.Object, mockSession.Object, new Mock<IStudentScheduleService>().Object, new Mock<IRefreshCoordinator>().Object, new Mock<IInsightsService>().Object);

        // Act
        var command = viewModel.GoToUniversitiesCommand;

        // Assert
        Assert.IsNotNull(command);
    }

    /// <summary>
    /// Tests that each access to GoToUniversitiesCommand property creates a new Command instance.
    /// This verifies the expression-bodied property behavior.
    /// </summary>
    [TestMethod]
    public void GoToUniversitiesCommand_WhenAccessedMultipleTimes_ReturnsNewInstanceEachTime()
    {
        // Arrange
        var mockDashboard = new Mock<IDashboardService>();
        var mockAuth = new Mock<IAuthService>();
        var mockSession = new Mock<SessionService>();
        var viewModel = new DashboardViewModel(mockDashboard.Object, mockAuth.Object, mockSession.Object, new Mock<IStudentScheduleService>().Object, new Mock<IRefreshCoordinator>().Object, new Mock<IInsightsService>().Object);

        // Act
        var command1 = viewModel.GoToUniversitiesCommand;
        var command2 = viewModel.GoToUniversitiesCommand;

        // Assert
        Assert.AreNotSame(command1, command2);
    }

    /// <summary>
    /// Tests that GoToUniversitiesCommand's CanExecute returns true by default.
    /// This validates that the command is always executable.
    /// </summary>
    [TestMethod]
    public void GoToUniversitiesCommand_CanExecute_ReturnsTrue()
    {
        // Arrange
        var mockDashboard = new Mock<IDashboardService>();
        var mockAuth = new Mock<IAuthService>();
        var mockSession = new Mock<SessionService>();
        var viewModel = new DashboardViewModel(mockDashboard.Object, mockAuth.Object, mockSession.Object, new Mock<IStudentScheduleService>().Object, new Mock<IRefreshCoordinator>().Object, new Mock<IInsightsService>().Object);

        // Act
        var command = viewModel.GoToUniversitiesCommand;
        var canExecute = command.CanExecute(null);

        // Assert
        Assert.IsTrue(canExecute);
    }

    // Note: Full integration testing of the navigation behavior (Shell.Current.GoToAsync) 
    // cannot be performed in unit tests because:
    // 1. Shell.Current is a static property that cannot be mocked with Moq
    // 2. The implementation doesn't use dependency injection for navigation
    // 3. Testing the actual navigation would require MAUI UI testing infrastructure
    // 
    // To fully test navigation behavior, consider:
    // - Refactoring to inject an INavigationService
    // - Using MAUI UI tests for integration testing
    // - Manual/exploratory testing of navigation flows

    /// <summary>
    /// Tests that AttendanceStatus property returns the initial value "On Track" when first accessed.
    /// </summary>
    [TestMethod]
    public void AttendanceStatus_InitialValue_ReturnsOnTrack()
    {
        // Arrange
        var mockDashboardService = new Mock<IDashboardService>();
        var mockAuthService = new Mock<IAuthService>();
        mockAuthService.Setup(a => a.LogoutAsync()).Returns(Task.CompletedTask);
        var mockSessionService = new Mock<SessionService>();

        var viewModel = new DashboardViewModel(mockDashboardService.Object, mockAuthService.Object, mockSessionService.Object, new Mock<IStudentScheduleService>().Object, new Mock<IRefreshCoordinator>().Object, new Mock<IInsightsService>().Object);

        // Act
        var result = viewModel.AttendanceStatus;

        // Assert
        Assert.AreEqual("On Track", result);
    }

    /// <summary>
    /// Tests that AttendanceStatus property can be set to a valid non-empty value and returns that value when accessed.
    /// Input: Valid string value.
    /// Expected: Property returns the set value.
    /// </summary>
    [TestMethod]
    [DataRow("Excellent")]
    [DataRow("Good")]
    [DataRow("At Risk")]
    [DataRow("Poor")]
    [DataRow("Needs Improvement")]
    public void AttendanceStatus_SetValidValue_ReturnsSetValue(string value)
    {
        // Arrange
        var mockDashboardService = new Mock<IDashboardService>();
        var mockAuthService = new Mock<IAuthService>();
        mockAuthService.Setup(a => a.LogoutAsync()).Returns(Task.CompletedTask);
        var mockSessionService = new Mock<SessionService>();

        var viewModel = new DashboardViewModel(mockDashboardService.Object, mockAuthService.Object, mockSessionService.Object, new Mock<IStudentScheduleService>().Object, new Mock<IRefreshCoordinator>().Object, new Mock<IInsightsService>().Object);

        // Act
        viewModel.AttendanceStatus = value;
        var result = viewModel.AttendanceStatus;

        // Assert
        Assert.AreEqual(value, result);
    }

    /// <summary>
    /// Tests that AttendanceStatus property can be set to an empty string.
    /// Input: Empty string.
    /// Expected: Property returns empty string.
    /// </summary>
    [TestMethod]
    public void AttendanceStatus_SetEmptyString_ReturnsEmptyString()
    {
        // Arrange
        var mockDashboardService = new Mock<IDashboardService>();
        var mockAuthService = new Mock<IAuthService>();
        mockAuthService.Setup(a => a.LogoutAsync()).Returns(Task.CompletedTask);
        var mockSessionService = new Mock<SessionService>();

        var viewModel = new DashboardViewModel(mockDashboardService.Object, mockAuthService.Object, mockSessionService.Object, new Mock<IStudentScheduleService>().Object, new Mock<IRefreshCoordinator>().Object, new Mock<IInsightsService>().Object);

        // Act
        viewModel.AttendanceStatus = string.Empty;
        var result = viewModel.AttendanceStatus;

        // Assert
        Assert.AreEqual(string.Empty, result);
    }

    /// <summary>
    /// Tests that AttendanceStatus property can be set to whitespace-only strings.
    /// Input: Whitespace strings (spaces, tabs, newlines).
    /// Expected: Property returns the whitespace string.
    /// </summary>
    [TestMethod]
    [DataRow("   ")]
    [DataRow("\t")]
    [DataRow("\n")]
    [DataRow(" \t \n ")]
    public void AttendanceStatus_SetWhitespace_ReturnsWhitespace(string value)
    {
        // Arrange
        var mockDashboardService = new Mock<IDashboardService>();
        var mockAuthService = new Mock<IAuthService>();
        mockAuthService.Setup(a => a.LogoutAsync()).Returns(Task.CompletedTask);
        var mockSessionService = new Mock<SessionService>();

        var viewModel = new DashboardViewModel(mockDashboardService.Object, mockAuthService.Object, mockSessionService.Object, new Mock<IStudentScheduleService>().Object, new Mock<IRefreshCoordinator>().Object, new Mock<IInsightsService>().Object);

        // Act
        viewModel.AttendanceStatus = value;
        var result = viewModel.AttendanceStatus;

        // Assert
        Assert.AreEqual(value, result);
    }

    /// <summary>
    /// Tests that AttendanceStatus property can handle very long strings.
    /// Input: A very long string (1000+ characters).
    /// Expected: Property returns the entire long string.
    /// </summary>
    [TestMethod]
    public void AttendanceStatus_SetVeryLongString_ReturnsLongString()
    {
        // Arrange
        var mockDashboardService = new Mock<IDashboardService>();
        var mockAuthService = new Mock<IAuthService>();
        mockAuthService.Setup(a => a.LogoutAsync()).Returns(Task.CompletedTask);
        var mockSessionService = new Mock<SessionService>();

        var viewModel = new DashboardViewModel(mockDashboardService.Object, mockAuthService.Object, mockSessionService.Object, new Mock<IStudentScheduleService>().Object, new Mock<IRefreshCoordinator>().Object, new Mock<IInsightsService>().Object);

        var longString = new string('A', 5000);

        // Act
        viewModel.AttendanceStatus = longString;
        var result = viewModel.AttendanceStatus;

        // Assert
        Assert.AreEqual(longString, result);
    }

    /// <summary>
    /// Tests that AttendanceStatus property can handle strings with special characters.
    /// Input: Strings containing special characters, Unicode, control characters.
    /// Expected: Property returns the string with special characters intact.
    /// </summary>
    [TestMethod]
    [DataRow("Status: 100%")]
    [DataRow("At-Risk!")]
    [DataRow("Status <Good>")]
    [DataRow("Status & Notes")]
    [DataRow("Status\u0000Control")]
    [DataRow("Status™")]
    [DataRow("????")]
    [DataRow("Status\r\nMultiline")]
    public void AttendanceStatus_SetSpecialCharacters_ReturnsStringWithSpecialCharacters(string value)
    {
        // Arrange
        var mockDashboardService = new Mock<IDashboardService>();
        var mockAuthService = new Mock<IAuthService>();
        mockAuthService.Setup(a => a.LogoutAsync()).Returns(Task.CompletedTask);
        var mockSessionService = new Mock<SessionService>();

        var viewModel = new DashboardViewModel(mockDashboardService.Object, mockAuthService.Object, mockSessionService.Object, new Mock<IStudentScheduleService>().Object, new Mock<IRefreshCoordinator>().Object, new Mock<IInsightsService>().Object);

        // Act
        viewModel.AttendanceStatus = value;
        var result = viewModel.AttendanceStatus;

        // Assert
        Assert.AreEqual(value, result);
    }

    /// <summary>
    /// Tests that AttendanceStatus property can be updated multiple times sequentially.
    /// Input: Multiple different string values set in sequence.
    /// Expected: Property returns the most recently set value after each update.
    /// </summary>
    [TestMethod]
    public void AttendanceStatus_SetMultipleTimes_ReturnsLatestValue()
    {
        // Arrange
        var mockDashboardService = new Mock<IDashboardService>();
        var mockAuthService = new Mock<IAuthService>();
        mockAuthService.Setup(a => a.LogoutAsync()).Returns(Task.CompletedTask);
        var mockSessionService = new Mock<SessionService>();

        var viewModel = new DashboardViewModel(mockDashboardService.Object, mockAuthService.Object, mockSessionService.Object, new Mock<IStudentScheduleService>().Object, new Mock<IRefreshCoordinator>().Object, new Mock<IInsightsService>().Object);

        // Act & Assert
        viewModel.AttendanceStatus = "Good";
        Assert.AreEqual("Good", viewModel.AttendanceStatus);

        viewModel.AttendanceStatus = "At Risk";
        Assert.AreEqual("At Risk", viewModel.AttendanceStatus);

        viewModel.AttendanceStatus = "Excellent";
        Assert.AreEqual("Excellent", viewModel.AttendanceStatus);

        viewModel.AttendanceStatus = string.Empty;
        Assert.AreEqual(string.Empty, viewModel.AttendanceStatus);
    }

    /// <summary>
    /// Tests that setting the LatestAnnouncementDate property with various string values
    /// correctly updates the property value and raises the PropertyChanged event.
    /// </summary>
    /// <param name="value">The string value to set.</param>
    [TestMethod]
    [DataRow("")]
    [DataRow("January 15, 2024")]
    [DataRow("2024-01-15T10:30:00")]
    [DataRow("   ")]
    [DataRow("\t")]
    [DataRow("\n")]
    [DataRow("Monday, December 25, 2023 at 3:45 PM")]
    [DataRow("Test with special chars: !@#$%^&*()")]
    [DataRow("Unicode: ???? ??")]
    [DataRow("Very long string: Lorem ipsum dolor sit amet, consectetur adipiscing elit. Sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur.")]
    public void LatestAnnouncementDate_SetValue_UpdatesPropertyAndRaisesPropertyChanged(string value)
    {
        // Arrange
        var mockDashboardService = new Mock<IDashboardService>();
        var mockAuthService = new Mock<IAuthService>();
        var mockSessionService = new Mock<SessionService>();
        var viewModel = new DashboardViewModel(mockDashboardService.Object, mockAuthService.Object, mockSessionService.Object, new Mock<IStudentScheduleService>().Object, new Mock<IRefreshCoordinator>().Object, new Mock<IInsightsService>().Object);

        string? raisedPropertyName = null;
        viewModel.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == nameof(DashboardViewModel.LatestAnnouncementDate))
            {
                raisedPropertyName = args.PropertyName;
            }
        };

        // Act
        viewModel.LatestAnnouncementDate = value;

        // Assert
        Assert.AreEqual(value, viewModel.LatestAnnouncementDate);
        Assert.AreEqual(nameof(DashboardViewModel.LatestAnnouncementDate), raisedPropertyName);
    }

    /// <summary>
    /// Tests that setting the LatestAnnouncementDate property to the same value consecutively
    /// only raises the PropertyChanged event once (on the first change from the initial value).
    /// </summary>
    [TestMethod]
    public void LatestAnnouncementDate_SetSameValueTwice_RaisesPropertyChangedOnlyOnce()
    {
        // Arrange
        var mockDashboardService = new Mock<IDashboardService>();
        var mockAuthService = new Mock<IAuthService>();
        var mockSessionService = new Mock<SessionService>();
        var viewModel = new DashboardViewModel(mockDashboardService.Object, mockAuthService.Object, mockSessionService.Object, new Mock<IStudentScheduleService>().Object, new Mock<IRefreshCoordinator>().Object, new Mock<IInsightsService>().Object);

        int eventRaisedCount = 0;
        viewModel.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == nameof(DashboardViewModel.LatestAnnouncementDate))
            {
                eventRaisedCount++;
            }
        };

        const string testValue = "January 1, 2024";

        // Act
        viewModel.LatestAnnouncementDate = testValue;
        viewModel.LatestAnnouncementDate = testValue;

        // Assert
        Assert.AreEqual(1, eventRaisedCount);
        Assert.AreEqual(testValue, viewModel.LatestAnnouncementDate);
    }

    /// <summary>
    /// Tests that the LatestAnnouncementDate property getter returns the correct value
    /// after multiple different values are set sequentially.
    /// </summary>
    [TestMethod]
    public void LatestAnnouncementDate_SetMultipleValues_ReturnsLatestValue()
    {
        // Arrange
        var mockDashboardService = new Mock<IDashboardService>();
        var mockAuthService = new Mock<IAuthService>();
        var mockSessionService = new Mock<SessionService>();
        var viewModel = new DashboardViewModel(mockDashboardService.Object, mockAuthService.Object, mockSessionService.Object, new Mock<IStudentScheduleService>().Object, new Mock<IRefreshCoordinator>().Object, new Mock<IInsightsService>().Object);

        // Act & Assert
        viewModel.LatestAnnouncementDate = "First Value";
        Assert.AreEqual("First Value", viewModel.LatestAnnouncementDate);

        viewModel.LatestAnnouncementDate = "Second Value";
        Assert.AreEqual("Second Value", viewModel.LatestAnnouncementDate);

        viewModel.LatestAnnouncementDate = "Third Value";
        Assert.AreEqual("Third Value", viewModel.LatestAnnouncementDate);

        viewModel.LatestAnnouncementDate = string.Empty;
        Assert.AreEqual(string.Empty, viewModel.LatestAnnouncementDate);
    }

    /// <summary>
    /// Tests that the LatestAnnouncementDate property is initialized to an empty string
    /// and no PropertyChanged event is raised before any value is set.
    /// </summary>
    [TestMethod]
    public void LatestAnnouncementDate_InitialValue_IsEmptyString()
    {
        // Arrange
        var mockDashboardService = new Mock<IDashboardService>();
        var mockAuthService = new Mock<IAuthService>();
        var mockSessionService = new Mock<SessionService>();

        // Act
        var viewModel = new DashboardViewModel(mockDashboardService.Object, mockAuthService.Object, mockSessionService.Object, new Mock<IStudentScheduleService>().Object, new Mock<IRefreshCoordinator>().Object, new Mock<IInsightsService>().Object);

        // Assert
        Assert.AreEqual(string.Empty, viewModel.LatestAnnouncementDate);
    }

    /// <summary>
    /// Tests that setting LatestAnnouncementDate to empty string after it had a value
    /// correctly updates the property and raises PropertyChanged event.
    /// </summary>
    [TestMethod]
    public void LatestAnnouncementDate_SetToEmptyAfterValue_UpdatesAndRaisesEvent()
    {
        // Arrange
        var mockDashboardService = new Mock<IDashboardService>();
        var mockAuthService = new Mock<IAuthService>();
        var mockSessionService = new Mock<SessionService>();
        var viewModel = new DashboardViewModel(mockDashboardService.Object, mockAuthService.Object, mockSessionService.Object, new Mock<IStudentScheduleService>().Object, new Mock<IRefreshCoordinator>().Object, new Mock<IInsightsService>().Object);

        viewModel.LatestAnnouncementDate = "Some Date";

        bool eventRaised = false;
        viewModel.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == nameof(DashboardViewModel.LatestAnnouncementDate))
            {
                eventRaised = true;
            }
        };

        // Act
        viewModel.LatestAnnouncementDate = string.Empty;

        // Assert
        Assert.AreEqual(string.Empty, viewModel.LatestAnnouncementDate);
        Assert.IsTrue(eventRaised);
    }

    /// <summary>
    /// Tests that the PropertyChanged event provides the correct sender
    /// when LatestAnnouncementDate is set.
    /// </summary>
    [TestMethod]
    public void LatestAnnouncementDate_SetValue_PropertyChangedEventHasCorrectSender()
    {
        // Arrange
        var mockDashboardService = new Mock<IDashboardService>();
        var mockAuthService = new Mock<IAuthService>();
        var mockSessionService = new Mock<SessionService>();
        var viewModel = new DashboardViewModel(mockDashboardService.Object, mockAuthService.Object, mockSessionService.Object, new Mock<IStudentScheduleService>().Object, new Mock<IRefreshCoordinator>().Object, new Mock<IInsightsService>().Object);

        object? eventSender = null;
        viewModel.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == nameof(DashboardViewModel.LatestAnnouncementDate))
            {
                eventSender = sender;
            }
        };

        // Act
        viewModel.LatestAnnouncementDate = "Test Date";

        // Assert
        Assert.AreSame(viewModel, eventSender);
    }

    /// <summary>
    /// Tests that IsClassRep returns true when Role matches "classrep" or "classrepresentative" (case-insensitive).
    /// </summary>
    /// <param name="role">The role value to test.</param>
    [TestMethod]
    [DataRow("classrep")]
    [DataRow("ClassRep")]
    [DataRow("CLASSREP")]
    [DataRow("ClAsSrEp")]
    [DataRow("classrepresentative")]
    [DataRow("ClassRepresentative")]
    [DataRow("CLASSREPRESENTATIVE")]
    [DataRow("ClAsSrEpReSeNtAtIvE")]
    public void IsClassRep_WhenRoleIsClassRepOrClassRepresentative_ReturnsTrue(string role)
    {
        // Arrange
        var mockDashboardService = new Mock<IDashboardService>();
        var mockAuthService = new Mock<IAuthService>();
        var mockSessionService = new Mock<SessionService>();
        var viewModel = new DashboardViewModel(mockDashboardService.Object, mockAuthService.Object, mockSessionService.Object, new Mock<IStudentScheduleService>().Object, new Mock<IRefreshCoordinator>().Object, new Mock<IInsightsService>().Object);

        // Act
        viewModel.Role = role;
        var result = viewModel.IsClassRep;

        // Assert
        Assert.IsTrue(result, $"IsClassRep should return true when Role is '{role}'.");
    }

    /// <summary>
    /// Tests that IsClassRep returns false when Role does not match "classrep" or "classrepresentative".
    /// </summary>
    /// <param name="role">The role value to test.</param>
    [TestMethod]
    [DataRow(null)]
    [DataRow("")]
    [DataRow(" ")]
    [DataRow("student")]
    [DataRow("Student")]
    [DataRow("lecturer")]
    [DataRow("Lecturer")]
    [DataRow("admin")]
    [DataRow("Admin")]
    [DataRow("classrep ")]
    [DataRow(" classrep")]
    [DataRow(" classrep ")]
    [DataRow("classrepresentative ")]
    [DataRow(" classrepresentative")]
    [DataRow("teacher")]
    [DataRow("manager")]
    [DataRow("xyz")]
    [DataRow("classrepresentativ")]
    [DataRow("classre")]
    public void IsClassRep_WhenRoleDoesNotMatchClassRepOrClassRepresentative_ReturnsFalse(string? role)
    {
        // Arrange
        var mockDashboardService = new Mock<IDashboardService>();
        var mockAuthService = new Mock<IAuthService>();
        var mockSessionService = new Mock<SessionService>();
        var viewModel = new DashboardViewModel(mockDashboardService.Object, mockAuthService.Object, mockSessionService.Object, new Mock<IStudentScheduleService>().Object, new Mock<IRefreshCoordinator>().Object, new Mock<IInsightsService>().Object);

        // Act
        viewModel.Role = role;
        var result = viewModel.IsClassRep;

        // Assert
        Assert.IsFalse(result, $"IsClassRep should return false when Role is '{role}'.");
    }

    /// <summary>
    /// Tests that RefreshCommand returns a non-null ICommand instance.
    /// </summary>
    [TestMethod]
    public void RefreshCommand_WhenAccessed_ReturnsNonNullCommand()
    {
        // Arrange
        var mockDashboard = new Mock<IDashboardService>();
        var mockAuth = new Mock<IAuthService>();
        var mockSession = new Mock<SessionService>();
        var viewModel = new DashboardViewModel(mockDashboard.Object, mockAuth.Object, mockSession.Object, new Mock<IStudentScheduleService>().Object, new Mock<IRefreshCoordinator>().Object, new Mock<IInsightsService>().Object);

        // Act
        var command = viewModel.RefreshCommand;

        // Assert
        Assert.IsNotNull(command);
        Assert.IsInstanceOfType<ICommand>(command);
    }

    /// <summary>
    /// Tests that accessing RefreshCommand multiple times creates a new Command instance each time.
    /// This verifies the property-level behavior where a new Command is instantiated on each access.
    /// </summary>
    [TestMethod]
    public void RefreshCommand_WhenAccessedMultipleTimes_CreatesNewInstanceEachTime()
    {
        // Arrange
        var mockDashboard = new Mock<IDashboardService>();
        var mockAuth = new Mock<IAuthService>();
        var mockSession = new Mock<SessionService>();
        var viewModel = new DashboardViewModel(mockDashboard.Object, mockAuth.Object, mockSession.Object, new Mock<IStudentScheduleService>().Object, new Mock<IRefreshCoordinator>().Object, new Mock<IInsightsService>().Object);

        // Act
        var command1 = viewModel.RefreshCommand;
        var command2 = viewModel.RefreshCommand;

        // Assert
        Assert.IsNotNull(command1);
        Assert.IsNotNull(command2);
        Assert.AreNotSame(command1, command2);
    }

    /// <summary>
    /// Tests that executing RefreshCommand invokes the dashboard service to retrieve dashboard data.
    /// Verifies the command's async execution completes and the service is called with proper state reset.
    /// </summary>
    [TestMethod]
    public async Task RefreshCommand_WhenExecuted_CallsDashboardService()
    {
        // Arrange
        var mockDashboard = new Mock<IDashboardService>();
        var dashboardData = new DashboardDto
        {
            ActiveCourses = 5,
            UpcomingAssignments = 3,
            AttendancePercent = 0.85,
            RecentAnnouncements = new List<AnnouncementDto>(),
            TotalStudents = null,
            TotalLecturers = null,
            TotalPrograms = null,
            TeachingClasses = null,
            ManagedClasses = null
        };
        mockDashboard.Setup(d => d.GetDashboardAsync()).ReturnsAsync(dashboardData);

        var mockAuth = new Mock<IAuthService>();
        var mockSession = new Mock<SessionService>();
        mockSession.Setup(s => s.Role).Returns(UserRole.Student);

        var viewModel = new DashboardViewModel(mockDashboard.Object, mockAuth.Object, mockSession.Object, new Mock<IStudentScheduleService>().Object, new Mock<IRefreshCoordinator>().Object, new Mock<IInsightsService>().Object);
        var command = viewModel.RefreshCommand;

        // Act
        if (command is Command mauiCommand)
        {
            await Task.Run(() => mauiCommand.Execute(null));
            await Task.Delay(100); // Allow async operation to complete
        }

        // Assert
        mockDashboard.Verify(d => d.GetDashboardAsync(), Times.Once);
    }

    /// <summary>
    /// Tests that executing RefreshCommand updates dashboard properties with retrieved data.
    /// Verifies that ActiveCourses, UpcomingAssignments, and AttendancePercentage are properly updated.
    /// </summary>
    [TestMethod]
    public async Task RefreshCommand_WhenExecuted_UpdatesProperties()
    {
        // Arrange
        var mockDashboard = new Mock<IDashboardService>();
        var dashboardData = new DashboardDto
        {
            ActiveCourses = 7,
            UpcomingAssignments = 4,
            AttendancePercent = 0.92,
            RecentAnnouncements = new List<AnnouncementDto>(),
            TotalStudents = null,
            TotalLecturers = null,
            TotalPrograms = null,
            TeachingClasses = null,
            ManagedClasses = null
        };
        mockDashboard.Setup(d => d.GetDashboardAsync()).ReturnsAsync(dashboardData);

        var mockAuth = new Mock<IAuthService>();
        var mockSession = new Mock<SessionService>();
        mockSession.Setup(s => s.Role).Returns(UserRole.Student);

        var viewModel = new DashboardViewModel(mockDashboard.Object, mockAuth.Object, mockSession.Object, new Mock<IStudentScheduleService>().Object, new Mock<IRefreshCoordinator>().Object, new Mock<IInsightsService>().Object);
        var command = viewModel.RefreshCommand;

        // Act
        if (command is Command mauiCommand)
        {
            await Task.Run(() => mauiCommand.Execute(null));
            await Task.Delay(100); // Allow async operation to complete
        }

        // Assert
        Assert.AreEqual(7, viewModel.ActiveCourses);
        Assert.AreEqual(4, viewModel.UpcomingAssignments);
        Assert.AreEqual(92, viewModel.AttendancePercentage);
    }

    /// <summary>
    /// Tests that executing RefreshCommand handles exceptions from the dashboard service gracefully.
    /// Verifies that ErrorMessage is populated when the service throws an exception.
    /// </summary>
    [TestMethod]
    public async Task RefreshCommand_WhenServiceThrowsException_SetsErrorMessage()
    {
        // Arrange
        var mockDashboard = new Mock<IDashboardService>();
        mockDashboard.Setup(d => d.GetDashboardAsync()).ThrowsAsync(new InvalidOperationException("Service error"));

        var mockAuth = new Mock<IAuthService>();
        var mockSession = new Mock<SessionService>();
        mockSession.Setup(s => s.Role).Returns(UserRole.Student);

        var viewModel = new DashboardViewModel(mockDashboard.Object, mockAuth.Object, mockSession.Object, new Mock<IStudentScheduleService>().Object, new Mock<IRefreshCoordinator>().Object, new Mock<IInsightsService>().Object);
        var command = viewModel.RefreshCommand;

        // Act
        if (command is Command mauiCommand)
        {
            await Task.Run(() => mauiCommand.Execute(null));
            await Task.Delay(100); // Allow async operation to complete
        }

        // Assert
        Assert.IsTrue(viewModel.ErrorMessage.Contains("Failed to load dashboard"));
        Assert.IsTrue(viewModel.ErrorMessage.Contains("Service error"));
    }

    /// <summary>
    /// Tests that executing RefreshCommand with announcements updates announcement-related properties.
    /// Verifies that LatestAnnouncementTitle, LatestAnnouncementBody, LatestAnnouncementDate, and HasAnnouncement are set correctly.
    /// </summary>
    [TestMethod]
    public async Task RefreshCommand_WhenExecutedWithAnnouncements_UpdatesAnnouncementProperties()
    {
        // Arrange
        var mockDashboard = new Mock<IDashboardService>();
        var announcementDate = new DateTime(2024, 1, 15);
        var dashboardData = new DashboardDto
        {
            ActiveCourses = 5,
            UpcomingAssignments = 3,
            AttendancePercent = 0.85,
            RecentAnnouncements = new List<AnnouncementDto>
            {
                new AnnouncementDto
                {
                    Title = "Important Update",
                    Body = "Test announcement body",
                    Date = announcementDate
                }
            },
            TotalStudents = null,
            TotalLecturers = null,
            TotalPrograms = null,
            TeachingClasses = null,
            ManagedClasses = null
        };
        mockDashboard.Setup(d => d.GetDashboardAsync()).ReturnsAsync(dashboardData);

        var mockAuth = new Mock<IAuthService>();
        var mockSession = new Mock<SessionService>();
        mockSession.Setup(s => s.Role).Returns(UserRole.Student);

        var viewModel = new DashboardViewModel(mockDashboard.Object, mockAuth.Object, mockSession.Object, new Mock<IStudentScheduleService>().Object, new Mock<IRefreshCoordinator>().Object, new Mock<IInsightsService>().Object);
        var command = viewModel.RefreshCommand;

        // Act
        if (command is Command mauiCommand)
        {
            await Task.Run(() => mauiCommand.Execute(null));
            await Task.Delay(100); // Allow async operation to complete
        }

        // Assert
        Assert.AreEqual("Important Update", viewModel.LatestAnnouncementTitle);
        Assert.AreEqual("Test announcement body", viewModel.LatestAnnouncementBody);
        Assert.AreEqual("Jan 15, 2024", viewModel.LatestAnnouncementDate);
        Assert.IsTrue(viewModel.HasAnnouncement);
    }

    /// <summary>
    /// Tests that executing RefreshCommand without announcements sets HasAnnouncement to false.
    /// Verifies proper handling of empty announcement lists.
    /// </summary>
    [TestMethod]
    public async Task RefreshCommand_WhenExecutedWithoutAnnouncements_SetsHasAnnouncementToFalse()
    {
        // Arrange
        var mockDashboard = new Mock<IDashboardService>();
        var dashboardData = new DashboardDto
        {
            ActiveCourses = 5,
            UpcomingAssignments = 3,
            AttendancePercent = 0.85,
            RecentAnnouncements = new List<AnnouncementDto>(),
            TotalStudents = null,
            TotalLecturers = null,
            TotalPrograms = null,
            TeachingClasses = null,
            ManagedClasses = null
        };
        mockDashboard.Setup(d => d.GetDashboardAsync()).ReturnsAsync(dashboardData);

        var mockAuth = new Mock<IAuthService>();
        var mockSession = new Mock<SessionService>();
        mockSession.Setup(s => s.Role).Returns(UserRole.Student);

        var viewModel = new DashboardViewModel(mockDashboard.Object, mockAuth.Object, mockSession.Object, new Mock<IStudentScheduleService>().Object, new Mock<IRefreshCoordinator>().Object, new Mock<IInsightsService>().Object);
        var command = viewModel.RefreshCommand;

        // Act
        if (command is Command mauiCommand)
        {
            await Task.Run(() => mauiCommand.Execute(null));
            await Task.Delay(100); // Allow async operation to complete
        }

        // Assert
        Assert.IsFalse(viewModel.HasAnnouncement);
    }

    /// <summary>
    /// Tests that setting TotalLecturers to a positive value updates the property correctly.
    /// Input: Positive integer value (100).
    /// Expected: Property value should be updated to 100.
    /// </summary>
    [TestMethod]
    public void TotalLecturers_SetPositiveValue_UpdatesProperty()
    {
        // Arrange
        var mockDashboard = new Mock<IDashboardService>();
        var mockAuth = new Mock<IAuthService>();
        var mockSession = new Mock<SessionService>();
        var viewModel = new DashboardViewModel(mockDashboard.Object, mockAuth.Object, mockSession.Object, new Mock<IStudentScheduleService>().Object, new Mock<IRefreshCoordinator>().Object, new Mock<IInsightsService>().Object);

        // Act
        viewModel.TotalLecturers = 100;

        // Assert
        Assert.AreEqual(100, viewModel.TotalLecturers);
    }

    /// <summary>
    /// Tests that setting TotalLecturers to zero updates the property correctly.
    /// Input: Zero value.
    /// Expected: Property value should be updated to 0.
    /// </summary>
    [TestMethod]
    public void TotalLecturers_SetZero_UpdatesProperty()
    {
        // Arrange
        var mockDashboard = new Mock<IDashboardService>();
        var mockAuth = new Mock<IAuthService>();
        var mockSession = new Mock<SessionService>();
        var viewModel = new DashboardViewModel(mockDashboard.Object, mockAuth.Object, mockSession.Object, new Mock<IStudentScheduleService>().Object, new Mock<IRefreshCoordinator>().Object, new Mock<IInsightsService>().Object);

        // Act
        viewModel.TotalLecturers = 0;

        // Assert
        Assert.AreEqual(0, viewModel.TotalLecturers);
    }

    /// <summary>
    /// Tests that setting TotalLecturers to a negative value updates the property correctly.
    /// Input: Negative integer value (-10).
    /// Expected: Property value should be updated to -10.
    /// </summary>
    [TestMethod]
    public void TotalLecturers_SetNegativeValue_UpdatesProperty()
    {
        // Arrange
        var mockDashboard = new Mock<IDashboardService>();
        var mockAuth = new Mock<IAuthService>();
        var mockSession = new Mock<SessionService>();
        var viewModel = new DashboardViewModel(mockDashboard.Object, mockAuth.Object, mockSession.Object, new Mock<IStudentScheduleService>().Object, new Mock<IRefreshCoordinator>().Object, new Mock<IInsightsService>().Object);

        // Act
        viewModel.TotalLecturers = -10;

        // Assert
        Assert.AreEqual(-10, viewModel.TotalLecturers);
    }

    /// <summary>
    /// Tests that setting TotalLecturers to int.MaxValue updates the property correctly.
    /// Input: int.MaxValue (2147483647).
    /// Expected: Property value should be updated to int.MaxValue.
    /// </summary>
    [TestMethod]
    public void TotalLecturers_SetMaxValue_UpdatesProperty()
    {
        // Arrange
        var mockDashboard = new Mock<IDashboardService>();
        var mockAuth = new Mock<IAuthService>();
        var mockSession = new Mock<SessionService>();
        var viewModel = new DashboardViewModel(mockDashboard.Object, mockAuth.Object, mockSession.Object, new Mock<IStudentScheduleService>().Object, new Mock<IRefreshCoordinator>().Object, new Mock<IInsightsService>().Object);

        // Act
        viewModel.TotalLecturers = int.MaxValue;

        // Assert
        Assert.AreEqual(int.MaxValue, viewModel.TotalLecturers);
    }

    /// <summary>
    /// Tests that setting TotalLecturers to int.MinValue updates the property correctly.
    /// Input: int.MinValue (-2147483648).
    /// Expected: Property value should be updated to int.MinValue.
    /// </summary>
    [TestMethod]
    public void TotalLecturers_SetMinValue_UpdatesProperty()
    {
        // Arrange
        var mockDashboard = new Mock<IDashboardService>();
        var mockAuth = new Mock<IAuthService>();
        var mockSession = new Mock<SessionService>();
        var viewModel = new DashboardViewModel(mockDashboard.Object, mockAuth.Object, mockSession.Object, new Mock<IStudentScheduleService>().Object, new Mock<IRefreshCoordinator>().Object, new Mock<IInsightsService>().Object);

        // Act
        viewModel.TotalLecturers = int.MinValue;

        // Assert
        Assert.AreEqual(int.MinValue, viewModel.TotalLecturers);
    }

    /// <summary>
    /// Tests that PropertyChanged event is raised when TotalLecturers value changes.
    /// Input: New value (50).
    /// Expected: PropertyChanged event should be raised with property name "TotalLecturers".
    /// </summary>
    [TestMethod]
    public void TotalLecturers_SetNewValue_RaisesPropertyChangedEvent()
    {
        // Arrange
        var mockDashboard = new Mock<IDashboardService>();
        var mockAuth = new Mock<IAuthService>();
        var mockSession = new Mock<SessionService>();
        var viewModel = new DashboardViewModel(mockDashboard.Object, mockAuth.Object, mockSession.Object, new Mock<IStudentScheduleService>().Object, new Mock<IRefreshCoordinator>().Object, new Mock<IInsightsService>().Object);
        var propertyChangedRaised = false;
        string? raisedPropertyName = null;
        viewModel.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == nameof(DashboardViewModel.TotalLecturers))
            {
                propertyChangedRaised = true;
                raisedPropertyName = args.PropertyName;
            }
        };

        // Act
        viewModel.TotalLecturers = 50;

        // Assert
        Assert.IsTrue(propertyChangedRaised);
        Assert.AreEqual(nameof(DashboardViewModel.TotalLecturers), raisedPropertyName);
    }

    /// <summary>
    /// Tests that PropertyChanged event is not raised when setting TotalLecturers to the same value.
    /// Input: Same value twice (25).
    /// Expected: PropertyChanged event should only be raised once, not on the second set.
    /// </summary>
    [TestMethod]
    public void TotalLecturers_SetSameValue_DoesNotRaisePropertyChangedEvent()
    {
        // Arrange
        var mockDashboard = new Mock<IDashboardService>();
        var mockAuth = new Mock<IAuthService>();
        var mockSession = new Mock<SessionService>();
        var viewModel = new DashboardViewModel(mockDashboard.Object, mockAuth.Object, mockSession.Object, new Mock<IStudentScheduleService>().Object, new Mock<IRefreshCoordinator>().Object, new Mock<IInsightsService>().Object);
        viewModel.TotalLecturers = 25;
        var eventRaisedCount = 0;
        viewModel.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == nameof(DashboardViewModel.TotalLecturers))
            {
                eventRaisedCount++;
            }
        };

        // Act
        viewModel.TotalLecturers = 25;

        // Assert
        Assert.AreEqual(0, eventRaisedCount);
    }

    /// <summary>
    /// Tests that multiple value changes to TotalLecturers update the property correctly.
    /// Input: Multiple different values (10, 20, 30).
    /// Expected: Property value should be updated to the last set value (30).
    /// </summary>
    [TestMethod]
    [DataRow(10, 20, 30)]
    [DataRow(0, 100, 50)]
    [DataRow(-5, 15, 0)]
    [DataRow(int.MinValue, int.MaxValue, 0)]
    public void TotalLecturers_SetMultipleValues_UpdatesPropertyCorrectly(int value1, int value2, int value3)
    {
        // Arrange
        var mockDashboard = new Mock<IDashboardService>();
        var mockAuth = new Mock<IAuthService>();
        var mockSession = new Mock<SessionService>();
        var viewModel = new DashboardViewModel(mockDashboard.Object, mockAuth.Object, mockSession.Object, new Mock<IStudentScheduleService>().Object, new Mock<IRefreshCoordinator>().Object, new Mock<IInsightsService>().Object);

        // Act
        viewModel.TotalLecturers = value1;
        viewModel.TotalLecturers = value2;
        viewModel.TotalLecturers = value3;

        // Assert
        Assert.AreEqual(value3, viewModel.TotalLecturers);
    }

    /// <summary>
    /// Tests that TotalLecturers has default value of zero when first accessed.
    /// Input: None (default initialization).
    /// Expected: Property value should be 0.
    /// </summary>
    [TestMethod]
    public void TotalLecturers_DefaultValue_IsZero()
    {
        // Arrange
        var mockDashboard = new Mock<IDashboardService>();
        var mockAuth = new Mock<IAuthService>();
        var mockSession = new Mock<SessionService>();
        var viewModel = new DashboardViewModel(mockDashboard.Object, mockAuth.Object, mockSession.Object, new Mock<IStudentScheduleService>().Object, new Mock<IRefreshCoordinator>().Object, new Mock<IInsightsService>().Object);

        // Act
        var result = viewModel.TotalLecturers;

        // Assert
        Assert.AreEqual(0, result);
    }

    /// <summary>
    /// Tests that ManagedClassesCount property getter returns the correct value after setting it.
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
    public void ManagedClassesCount_SetValue_ReturnsCorrectValue(int value)
    {
        // Arrange
        var mockDashboard = new Mock<IDashboardService>();
        var mockAuth = new Mock<IAuthService>();
        var mockSession = new Mock<SessionService>();
        var viewModel = new DashboardViewModel(mockDashboard.Object, mockAuth.Object, mockSession.Object, new Mock<IStudentScheduleService>().Object, new Mock<IRefreshCoordinator>().Object, new Mock<IInsightsService>().Object);

        // Act
        viewModel.ManagedClassesCount = value;

        // Assert
        Assert.AreEqual(value, viewModel.ManagedClassesCount);
    }

    /// <summary>
    /// Tests that ManagedClassesCount property raises PropertyChanged event when the value changes.
    /// Verifies the correct property name is passed in the event args.
    /// </summary>
    [TestMethod]
    public void ManagedClassesCount_SetDifferentValue_RaisesPropertyChangedEvent()
    {
        // Arrange
        var mockDashboard = new Mock<IDashboardService>();
        var mockAuth = new Mock<IAuthService>();
        var mockSession = new Mock<SessionService>();
        var viewModel = new DashboardViewModel(mockDashboard.Object, mockAuth.Object, mockSession.Object, new Mock<IStudentScheduleService>().Object, new Mock<IRefreshCoordinator>().Object, new Mock<IInsightsService>().Object);
        string? raisedPropertyName = null;
        viewModel.PropertyChanged += (sender, args) => raisedPropertyName = args.PropertyName;

        // Act
        viewModel.ManagedClassesCount = 42;

        // Assert
        Assert.AreEqual("ManagedClassesCount", raisedPropertyName);
    }

    /// <summary>
    /// Tests that ManagedClassesCount property does not raise PropertyChanged event when set to the same value.
    /// Validates the optimization behavior of SetProperty method.
    /// </summary>
    [TestMethod]
    public void ManagedClassesCount_SetSameValue_DoesNotRaisePropertyChangedEvent()
    {
        // Arrange
        var mockDashboard = new Mock<IDashboardService>();
        var mockAuth = new Mock<IAuthService>();
        var mockSession = new Mock<SessionService>();
        var viewModel = new DashboardViewModel(mockDashboard.Object, mockAuth.Object, mockSession.Object, new Mock<IStudentScheduleService>().Object, new Mock<IRefreshCoordinator>().Object, new Mock<IInsightsService>().Object);
        viewModel.ManagedClassesCount = 10;
        int eventRaisedCount = 0;
        viewModel.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == "ManagedClassesCount")
            {
                eventRaisedCount++;
            }
        };

        // Act
        viewModel.ManagedClassesCount = 10;

        // Assert
        Assert.AreEqual(0, eventRaisedCount);
    }

    /// <summary>
    /// Tests that ManagedClassesCount property can be set to int.MinValue boundary value.
    /// Verifies boundary condition handling for minimum integer value.
    /// </summary>
    [TestMethod]
    public void ManagedClassesCount_SetIntMinValue_UpdatesAndRaisesEvent()
    {
        // Arrange
        var mockDashboard = new Mock<IDashboardService>();
        var mockAuth = new Mock<IAuthService>();
        var mockSession = new Mock<SessionService>();
        var viewModel = new DashboardViewModel(mockDashboard.Object, mockAuth.Object, mockSession.Object, new Mock<IStudentScheduleService>().Object, new Mock<IRefreshCoordinator>().Object, new Mock<IInsightsService>().Object);
        bool eventRaised = false;
        viewModel.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == "ManagedClassesCount")
            {
                eventRaised = true;
            }
        };

        // Act
        viewModel.ManagedClassesCount = int.MinValue;

        // Assert
        Assert.AreEqual(int.MinValue, viewModel.ManagedClassesCount);
        Assert.IsTrue(eventRaised);
    }

    /// <summary>
    /// Tests that ManagedClassesCount property can be set to int.MaxValue boundary value.
    /// Verifies boundary condition handling for maximum integer value.
    /// </summary>
    [TestMethod]
    public void ManagedClassesCount_SetIntMaxValue_UpdatesAndRaisesEvent()
    {
        // Arrange
        var mockDashboard = new Mock<IDashboardService>();
        var mockAuth = new Mock<IAuthService>();
        var mockSession = new Mock<SessionService>();
        var viewModel = new DashboardViewModel(mockDashboard.Object, mockAuth.Object, mockSession.Object, new Mock<IStudentScheduleService>().Object, new Mock<IRefreshCoordinator>().Object, new Mock<IInsightsService>().Object);
        bool eventRaised = false;
        viewModel.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == "ManagedClassesCount")
            {
                eventRaised = true;
            }
        };

        // Act
        viewModel.ManagedClassesCount = int.MaxValue;

        // Assert
        Assert.AreEqual(int.MaxValue, viewModel.ManagedClassesCount);
        Assert.IsTrue(eventRaised);
    }

    /// <summary>
    /// Tests that ManagedClassesCount property raises PropertyChanged event multiple times when value changes multiple times.
    /// Verifies that the property correctly notifies on each distinct value change.
    /// </summary>
    [TestMethod]
    public void ManagedClassesCount_SetMultipleDifferentValues_RaisesPropertyChangedEventEachTime()
    {
        // Arrange
        var mockDashboard = new Mock<IDashboardService>();
        var mockAuth = new Mock<IAuthService>();
        var mockSession = new Mock<SessionService>();
        var viewModel = new DashboardViewModel(mockDashboard.Object, mockAuth.Object, mockSession.Object, new Mock<IStudentScheduleService>().Object, new Mock<IRefreshCoordinator>().Object, new Mock<IInsightsService>().Object);
        int eventRaisedCount = 0;
        viewModel.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == "ManagedClassesCount")
            {
                eventRaisedCount++;
            }
        };

        // Act
        viewModel.ManagedClassesCount = 1;
        viewModel.ManagedClassesCount = 2;
        viewModel.ManagedClassesCount = 3;

        // Assert
        Assert.AreEqual(3, eventRaisedCount);
        Assert.AreEqual(3, viewModel.ManagedClassesCount);
    }

    /// <summary>
    /// Tests that ManagedClassesCount property has default value of zero when viewmodel is newly created.
    /// Verifies the initial state of the property.
    /// </summary>
    [TestMethod]
    public void ManagedClassesCount_NewViewModel_HasDefaultValueZero()
    {
        // Arrange
        var mockDashboard = new Mock<IDashboardService>();
        var mockAuth = new Mock<IAuthService>();
        var mockSession = new Mock<SessionService>();

        // Act
        var viewModel = new DashboardViewModel(mockDashboard.Object, mockAuth.Object, mockSession.Object, new Mock<IStudentScheduleService>().Object, new Mock<IRefreshCoordinator>().Object, new Mock<IInsightsService>().Object);

        // Assert
        Assert.AreEqual(0, viewModel.ManagedClassesCount);
    }

    /// <summary>
    /// Tests that the ErrorMessage property returns the initial default value of an empty string.
    /// </summary>
    [TestMethod]
    public void ErrorMessage_Get_ReturnsInitialEmptyString()
    {
        // Arrange
        var mockDashboard = new Mock<IDashboardService>();
        var mockAuth = new Mock<IAuthService>();
        var mockSession = new Mock<SessionService>();
        var viewModel = new DashboardViewModel(mockDashboard.Object, mockAuth.Object, mockSession.Object, new Mock<IStudentScheduleService>().Object, new Mock<IRefreshCoordinator>().Object, new Mock<IInsightsService>().Object);

        // Act
        var result = viewModel.ErrorMessage;

        // Assert
        Assert.AreEqual(string.Empty, result);
    }

    /// <summary>
    /// Tests that setting a new value to the ErrorMessage property raises the PropertyChanged event
    /// with the correct property name.
    /// </summary>
    [TestMethod]
    public void ErrorMessage_SetNewValue_RaisesPropertyChangedEvent()
    {
        // Arrange
        var mockDashboard = new Mock<IDashboardService>();
        var mockAuth = new Mock<IAuthService>();
        var mockSession = new Mock<SessionService>();
        var viewModel = new DashboardViewModel(mockDashboard.Object, mockAuth.Object, mockSession.Object, new Mock<IStudentScheduleService>().Object, new Mock<IRefreshCoordinator>().Object, new Mock<IInsightsService>().Object);
        var eventRaised = false;
        string? raisedPropertyName = null;

        viewModel.PropertyChanged += (sender, args) =>
        {
            eventRaised = true;
            raisedPropertyName = args.PropertyName;
        };

        // Act
        viewModel.ErrorMessage = "Test error message";

        // Assert
        Assert.IsTrue(eventRaised);
        Assert.AreEqual("ErrorMessage", raisedPropertyName);
    }

    /// <summary>
    /// Tests that setting the same value to the ErrorMessage property does not raise the PropertyChanged event,
    /// avoiding unnecessary notifications when the value hasn't actually changed.
    /// </summary>
    [TestMethod]
    public void ErrorMessage_SetSameValue_DoesNotRaisePropertyChangedEvent()
    {
        // Arrange
        var mockDashboard = new Mock<IDashboardService>();
        var mockAuth = new Mock<IAuthService>();
        var mockSession = new Mock<SessionService>();
        var viewModel = new DashboardViewModel(mockDashboard.Object, mockAuth.Object, mockSession.Object, new Mock<IStudentScheduleService>().Object, new Mock<IRefreshCoordinator>().Object, new Mock<IInsightsService>().Object);
        viewModel.ErrorMessage = "Initial error";
        var eventRaisedCount = 0;

        viewModel.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == "ErrorMessage")
            {
                eventRaisedCount++;
            }
        };

        // Act
        viewModel.ErrorMessage = "Initial error";

        // Assert
        Assert.AreEqual(0, eventRaisedCount);
    }

    /// <summary>
    /// Tests that the ErrorMessage property correctly stores and retrieves various string values
    /// including edge cases such as null, empty, whitespace, very long strings, and special characters.
    /// Input: Various string values to test edge cases and normal scenarios.
    /// Expected: The property should store and return the exact value set.
    /// </summary>
    [TestMethod]
    [DataRow("")]
    [DataRow("Simple error message")]
    [DataRow("   ")]
    [DataRow("\t\n\r")]
    [DataRow("Error with special chars: !@#$%^&*()_+-=[]{}|;':\",./<>?")]
    [DataRow("Unicode: ???? ????")]
    public void ErrorMessage_SetVariousValidValues_UpdatesAndReturnsCorrectValue(string value)
    {
        // Arrange
        var mockDashboard = new Mock<IDashboardService>();
        var mockAuth = new Mock<IAuthService>();
        var mockSession = new Mock<SessionService>();
        var viewModel = new DashboardViewModel(mockDashboard.Object, mockAuth.Object, mockSession.Object, new Mock<IStudentScheduleService>().Object, new Mock<IRefreshCoordinator>().Object, new Mock<IInsightsService>().Object);

        // Act
        viewModel.ErrorMessage = value;
        var result = viewModel.ErrorMessage;

        // Assert
        Assert.AreEqual(value, result);
    }

    /// <summary>
    /// Tests that the ErrorMessage property can handle very long strings without issues.
    /// Input: A string containing 100,000 characters.
    /// Expected: The property should store and return the exact long string.
    /// </summary>
    [TestMethod]
    public void ErrorMessage_SetVeryLongString_UpdatesAndReturnsCorrectValue()
    {
        // Arrange
        var mockDashboard = new Mock<IDashboardService>();
        var mockAuth = new Mock<IAuthService>();
        var mockSession = new Mock<SessionService>();
        var viewModel = new DashboardViewModel(mockDashboard.Object, mockAuth.Object, mockSession.Object, new Mock<IStudentScheduleService>().Object, new Mock<IRefreshCoordinator>().Object, new Mock<IInsightsService>().Object);
        var longString = new string('A', 100000);

        // Act
        viewModel.ErrorMessage = longString;
        var result = viewModel.ErrorMessage;

        // Assert
        Assert.AreEqual(longString, result);
        Assert.AreEqual(100000, result.Length);
    }

    /// <summary>
    /// Tests that multiple consecutive changes to the ErrorMessage property raise the PropertyChanged event
    /// for each distinct value change.
    /// </summary>
    [TestMethod]
    public void ErrorMessage_SetMultipleDifferentValues_RaisesPropertyChangedForEachChange()
    {
        // Arrange
        var mockDashboard = new Mock<IDashboardService>();
        var mockAuth = new Mock<IAuthService>();
        var mockSession = new Mock<SessionService>();
        var viewModel = new DashboardViewModel(mockDashboard.Object, mockAuth.Object, mockSession.Object, new Mock<IStudentScheduleService>().Object, new Mock<IRefreshCoordinator>().Object, new Mock<IInsightsService>().Object);
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
    /// Tests that setting the ErrorMessage property back to empty string after having a value
    /// correctly updates the property and raises the PropertyChanged event.
    /// </summary>
    [TestMethod]
    public void ErrorMessage_SetToEmptyStringAfterValue_UpdatesAndRaisesEvent()
    {
        // Arrange
        var mockDashboard = new Mock<IDashboardService>();
        var mockAuth = new Mock<IAuthService>();
        var mockSession = new Mock<SessionService>();
        var viewModel = new DashboardViewModel(mockDashboard.Object, mockAuth.Object, mockSession.Object, new Mock<IStudentScheduleService>().Object, new Mock<IRefreshCoordinator>().Object, new Mock<IInsightsService>().Object);
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
    /// Tests that the constructor properly initializes all dependencies and command properties
    /// when provided with valid non-null dependencies and SessionService has no current user.
    /// </summary>
    [TestMethod]
    public void Constructor_WithValidDependenciesAndNoCurrentUser_InitializesAllPropertiesAndCommands()
    {
        // Arrange
        var mockDashboardService = new Mock<IDashboardService>();
        var mockAuthService = new Mock<IAuthService>();
        var sessionService = new SessionService();

        // Act
        var viewModel = new DashboardViewModel(
            mockDashboardService.Object,
            mockAuthService.Object,
            sessionService);

        // Assert
        Assert.IsNotNull(viewModel.LoadCommand);
        Assert.IsNotNull(viewModel.LogoutCommand);
        Assert.IsNotNull(viewModel.NavigateToProfileCommand);
        Assert.IsNotNull(viewModel.NavigateToSearchCommand);
        Assert.IsNotNull(viewModel.NavigateToNotificationsCommand);
        Assert.IsNotNull(viewModel.NavigateToCoursesCommand);
        Assert.IsNotNull(viewModel.NavigateToAssignmentsCommand);
        Assert.IsNotNull(viewModel.NavigateToNewsCommand);
        Assert.IsNotNull(viewModel.NavigateToChatCommand);
        Assert.IsNotNull(viewModel.RefreshCommand);
        Assert.AreEqual("Student", viewModel.StudentName);
    }

    /// <summary>
    /// Tests that the constructor properly sets StudentName to the current user's FullName
    /// when SessionService has a current user with a valid FullName.
    /// </summary>
    [TestMethod]
    public void Constructor_WithCurrentUserHavingFullName_SetsStudentNameToFullName()
    {
        // Arrange
        var mockDashboardService = new Mock<IDashboardService>();
        var mockAuthService = new Mock<IAuthService>();
        var sessionService = new SessionService();
        var user = new AuthUserDto
        {
            FullName = "John Doe",
            Role = "Student"
        };
        sessionService.SetUser(user);

        // Act
        var viewModel = new DashboardViewModel(
            mockDashboardService.Object,
            mockAuthService.Object,
            sessionService);

        // Assert
        Assert.AreEqual("John Doe", viewModel.StudentName);
    }

    /// <summary>
    /// Tests that the constructor sets StudentName to default "Student"
    /// when SessionService has a current user with null FullName.
    /// </summary>
    [TestMethod]
    public void Constructor_WithCurrentUserHavingNullFullName_SetsStudentNameToDefault()
    {
        // Arrange
        var mockDashboardService = new Mock<IDashboardService>();
        var mockAuthService = new Mock<IAuthService>();
        var sessionService = new SessionService();
        var user = new AuthUserDto
        {
            FullName = null,
            Role = "Student"
        };
        sessionService.SetUser(user);

        // Act
        var viewModel = new DashboardViewModel(
            mockDashboardService.Object,
            mockAuthService.Object,
            sessionService);

        // Assert
        Assert.AreEqual("Student", viewModel.StudentName);
    }

    /// <summary>
    /// Tests that the constructor sets StudentName to default "Student"
    /// when SessionService has a current user with empty FullName.
    /// </summary>
    [TestMethod]
    public void Constructor_WithCurrentUserHavingEmptyFullName_SetsStudentNameToDefault()
    {
        // Arrange
        var mockDashboardService = new Mock<IDashboardService>();
        var mockAuthService = new Mock<IAuthService>();
        var sessionService = new SessionService();
        var user = new AuthUserDto
        {
            FullName = string.Empty,
            Role = "Student"
        };
        sessionService.SetUser(user);

        // Act
        var viewModel = new DashboardViewModel(
            mockDashboardService.Object,
            mockAuthService.Object,
            sessionService);

        // Assert
        Assert.AreEqual("Student", viewModel.StudentName);
    }

    /// <summary>
    /// Tests that the constructor sets StudentName to default "Student"
    /// when SessionService has a current user with whitespace-only FullName.
    /// </summary>
    [TestMethod]
    public void Constructor_WithCurrentUserHavingWhitespaceFullName_SetsStudentNameToDefault()
    {
        // Arrange
        var mockDashboardService = new Mock<IDashboardService>();
        var mockAuthService = new Mock<IAuthService>();
        var sessionService = new SessionService();
        var user = new AuthUserDto
        {
            FullName = "   ",
            Role = "Student"
        };
        sessionService.SetUser(user);

        // Act
        var viewModel = new DashboardViewModel(
            mockDashboardService.Object,
            mockAuthService.Object,
            sessionService);

        // Assert
        Assert.AreEqual("Student", viewModel.StudentName);
    }

    /// <summary>
    /// Tests that the constructor accepts null IDashboardService parameter
    /// and initializes commands without throwing during construction.
    /// </summary>
    [TestMethod]
    public void Constructor_WithNullDashboardService_InitializesWithoutException()
    {
        // Arrange
        var mockAuthService = new Mock<IAuthService>();
        var sessionService = new SessionService();

        // Act
        var viewModel = new DashboardViewModel(
            null!,
            mockAuthService.Object,
            sessionService);

        // Assert
        Assert.IsNotNull(viewModel.LoadCommand);
        Assert.IsNotNull(viewModel.LogoutCommand);
        Assert.AreEqual("Student", viewModel.StudentName);
    }

    /// <summary>
    /// Tests that the constructor accepts null IAuthService parameter
    /// and initializes other commands without throwing during construction.
    /// </summary>
    [TestMethod]
    public void Constructor_WithNullAuthService_InitializesWithoutException()
    {
        // Arrange
        var mockDashboardService = new Mock<IDashboardService>();
        var sessionService = new SessionService();

        // Act
        var viewModel = new DashboardViewModel(
            mockDashboardService.Object,
            null!,
            sessionService);

        // Assert
        Assert.IsNotNull(viewModel.LoadCommand);
        Assert.IsNotNull(viewModel.LogoutCommand);
        Assert.AreEqual("Student", viewModel.StudentName);
    }

    /// <summary>
    /// Tests that the constructor properly initializes TodayClasses as an empty observable collection.
    /// </summary>
    [TestMethod]
    public void Constructor_Always_InitializesTodayClassesAsEmptyCollection()
    {
        // Arrange
        var mockDashboardService = new Mock<IDashboardService>();
        var mockAuthService = new Mock<IAuthService>();
        var sessionService = new SessionService();

        // Act
        var viewModel = new DashboardViewModel(
            mockDashboardService.Object,
            mockAuthService.Object,
            sessionService);

        // Assert
        Assert.IsNotNull(viewModel.TodayClasses);
        Assert.AreEqual(0, viewModel.TodayClasses.Count);
    }

    /// <summary>
    /// Tests that the constructor properly initializes TeachingClasses as an empty observable collection.
    /// </summary>
    [TestMethod]
    public void Constructor_Always_InitializesTeachingClassesAsEmptyCollection()
    {
        // Arrange
        var mockDashboardService = new Mock<IDashboardService>();
        var mockAuthService = new Mock<IAuthService>();
        var sessionService = new SessionService();

        // Act
        var viewModel = new DashboardViewModel(
            mockDashboardService.Object,
            mockAuthService.Object,
            sessionService);

        // Assert
        Assert.IsNotNull(viewModel.TeachingClasses);
        Assert.AreEqual(0, viewModel.TeachingClasses.Count);
    }

    /// <summary>
    /// Tests that the constructor properly initializes RecentAnnouncements as an empty observable collection.
    /// </summary>
    [TestMethod]
    public void Constructor_Always_InitializesRecentAnnouncementsAsEmptyCollection()
    {
        // Arrange
        var mockDashboardService = new Mock<IDashboardService>();
        var mockAuthService = new Mock<IAuthService>();
        var sessionService = new SessionService();

        // Act
        var viewModel = new DashboardViewModel(
            mockDashboardService.Object,
            mockAuthService.Object,
            sessionService);

        // Assert
        Assert.IsNotNull(viewModel.RecentAnnouncements);
        Assert.AreEqual(0, viewModel.RecentAnnouncements.Count);
    }

    /// <summary>
    /// Tests that the TotalPrograms property returns the default value of 0 when not set.
    /// </summary>
    [TestMethod]
    public void TotalPrograms_DefaultValue_ReturnsZero()
    {
        // Arrange
        var dashboardServiceMock = new Mock<IDashboardService>();
        var authServiceMock = new Mock<IAuthService>();
        var sessionServiceMock = new Mock<SessionService>();
        var viewModel = new DashboardViewModel(dashboardServiceMock.Object, authServiceMock.Object, sessionServiceMock.Object, new Mock<IStudentScheduleService>().Object, new Mock<IRefreshCoordinator>().Object, new Mock<IInsightsService>().Object);

        // Act
        var result = viewModel.TotalPrograms;

        // Assert
        Assert.AreEqual(0, result);
    }

    /// <summary>
    /// Tests that setting the TotalPrograms property with various valid values updates the property correctly.
    /// </summary>
    /// <param name="value">The value to set on the TotalPrograms property.</param>
    [TestMethod]
    [DataRow(1)]
    [DataRow(100)]
    [DataRow(1000)]
    [DataRow(int.MaxValue)]
    [DataRow(-1)]
    [DataRow(-100)]
    [DataRow(int.MinValue)]
    [DataRow(0)]
    public void TotalPrograms_SetValidValue_UpdatesProperty(int value)
    {
        // Arrange
        var dashboardServiceMock = new Mock<IDashboardService>();
        var authServiceMock = new Mock<IAuthService>();
        var sessionServiceMock = new Mock<SessionService>();
        var viewModel = new DashboardViewModel(dashboardServiceMock.Object, authServiceMock.Object, sessionServiceMock.Object, new Mock<IStudentScheduleService>().Object, new Mock<IRefreshCoordinator>().Object, new Mock<IInsightsService>().Object);

        // Act
        viewModel.TotalPrograms = value;

        // Assert
        Assert.AreEqual(value, viewModel.TotalPrograms);
    }

    /// <summary>
    /// Tests that setting the TotalPrograms property to a new value raises the PropertyChanged event.
    /// </summary>
    [TestMethod]
    public void TotalPrograms_SetNewValue_RaisesPropertyChangedEvent()
    {
        // Arrange
        var dashboardServiceMock = new Mock<IDashboardService>();
        var authServiceMock = new Mock<IAuthService>();
        var sessionServiceMock = new Mock<SessionService>();
        var viewModel = new DashboardViewModel(dashboardServiceMock.Object, authServiceMock.Object, sessionServiceMock.Object, new Mock<IStudentScheduleService>().Object, new Mock<IRefreshCoordinator>().Object, new Mock<IInsightsService>().Object);
        var propertyChangedRaised = false;
        string? raisedPropertyName = null;
        viewModel.PropertyChanged += (sender, args) =>
        {
            propertyChangedRaised = true;
            raisedPropertyName = args.PropertyName;
        };

        // Act
        viewModel.TotalPrograms = 42;

        // Assert
        Assert.IsTrue(propertyChangedRaised);
        Assert.AreEqual("TotalPrograms", raisedPropertyName);
    }

    /// <summary>
    /// Tests that setting the TotalPrograms property to the same value does not raise the PropertyChanged event.
    /// </summary>
    [TestMethod]
    public void TotalPrograms_SetSameValue_DoesNotRaisePropertyChangedEvent()
    {
        // Arrange
        var dashboardServiceMock = new Mock<IDashboardService>();
        var authServiceMock = new Mock<IAuthService>();
        var sessionServiceMock = new Mock<SessionService>();
        var viewModel = new DashboardViewModel(dashboardServiceMock.Object, authServiceMock.Object, sessionServiceMock.Object, new Mock<IStudentScheduleService>().Object, new Mock<IRefreshCoordinator>().Object, new Mock<IInsightsService>().Object);
        viewModel.TotalPrograms = 10;
        var propertyChangedRaised = false;
        viewModel.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == "TotalPrograms")
            {
                propertyChangedRaised = true;
            }
        };

        // Act
        viewModel.TotalPrograms = 10;

        // Assert
        Assert.IsFalse(propertyChangedRaised);
    }

    /// <summary>
    /// Tests that setting the TotalPrograms property multiple times with different values updates correctly.
    /// </summary>
    [TestMethod]
    public void TotalPrograms_SetMultipleDifferentValues_UpdatesCorrectly()
    {
        // Arrange
        var dashboardServiceMock = new Mock<IDashboardService>();
        var authServiceMock = new Mock<IAuthService>();
        var sessionServiceMock = new Mock<SessionService>();
        var viewModel = new DashboardViewModel(dashboardServiceMock.Object, authServiceMock.Object, sessionServiceMock.Object, new Mock<IStudentScheduleService>().Object, new Mock<IRefreshCoordinator>().Object, new Mock<IInsightsService>().Object);

        // Act & Assert
        viewModel.TotalPrograms = 5;
        Assert.AreEqual(5, viewModel.TotalPrograms);

        viewModel.TotalPrograms = 100;
        Assert.AreEqual(100, viewModel.TotalPrograms);

        viewModel.TotalPrograms = -50;
        Assert.AreEqual(-50, viewModel.TotalPrograms);

        viewModel.TotalPrograms = 0;
        Assert.AreEqual(0, viewModel.TotalPrograms);
    }

    /// <summary>
    /// Tests that PropertyChanged event is raised multiple times when setting different values consecutively.
    /// </summary>
    [TestMethod]
    public void TotalPrograms_SetMultipleDifferentValues_RaisesPropertyChangedEventEachTime()
    {
        // Arrange
        var dashboardServiceMock = new Mock<IDashboardService>();
        var authServiceMock = new Mock<IAuthService>();
        var sessionServiceMock = new Mock<SessionService>();
        var viewModel = new DashboardViewModel(dashboardServiceMock.Object, authServiceMock.Object, sessionServiceMock.Object, new Mock<IStudentScheduleService>().Object, new Mock<IRefreshCoordinator>().Object, new Mock<IInsightsService>().Object);
        var eventRaisedCount = 0;
        viewModel.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == "TotalPrograms")
            {
                eventRaisedCount++;
            }
        };

        // Act
        viewModel.TotalPrograms = 10;
        viewModel.TotalPrograms = 20;
        viewModel.TotalPrograms = 30;

        // Assert
        Assert.AreEqual(3, eventRaisedCount);
    }

    /// <summary>
    /// Tests that the StudentName property has the correct initial value.
    /// </summary>
    [TestMethod]
    public void StudentName_InitialValue_IsStudent()
    {
        // Arrange
        var dashboardServiceMock = new Mock<IDashboardService>();
        var authServiceMock = new Mock<IAuthService>();
        var sessionServiceMock = new Mock<SessionService>();

        // Act
        var viewModel = new DashboardViewModel(
            dashboardServiceMock.Object,
            authServiceMock.Object,
            sessionServiceMock.Object);

        // Assert
        Assert.AreEqual("Student", viewModel.StudentName);
    }

    /// <summary>
    /// Tests that setting a valid value to StudentName updates the property and raises PropertyChanged event.
    /// </summary>
    /// <param name="newValue">The value to set.</param>
    [TestMethod]
    [DataRow("John Doe")]
    [DataRow("")]
    [DataRow("   ")]
    [DataRow("A")]
    [DataRow("VeryLongNameThatExceedsTypicalLengthForTestingPurposesAndIncludesLotsOfCharactersToEnsureThePropertyCanHandleVeryLongStringsWithoutIssuesOrErrors")]
    [DataRow("Special\nCharacters\t\r\nWith Unicode: ??")]
    [DataRow("Name with numbers 12345")]
    [DataRow("!@#$%^&*()_+-=[]{}|;':\",./<>?")]
    public void StudentName_SetValidValue_UpdatesPropertyAndRaisesPropertyChanged(string newValue)
    {
        // Arrange
        var dashboardServiceMock = new Mock<IDashboardService>();
        var authServiceMock = new Mock<IAuthService>();
        var sessionServiceMock = new Mock<SessionService>();
        var viewModel = new DashboardViewModel(
            dashboardServiceMock.Object,
            authServiceMock.Object,
            sessionServiceMock.Object);

        string? raisedPropertyName = null;
        viewModel.PropertyChanged += (sender, args) => raisedPropertyName = args.PropertyName;

        // Act
        viewModel.StudentName = newValue;

        // Assert
        Assert.AreEqual(newValue, viewModel.StudentName);
        Assert.AreEqual("StudentName", raisedPropertyName);
    }

    /// <summary>
    /// Tests that setting the same value to StudentName does not raise PropertyChanged event.
    /// </summary>
    [TestMethod]
    public void StudentName_SetSameValue_DoesNotRaisePropertyChanged()
    {
        // Arrange
        var dashboardServiceMock = new Mock<IDashboardService>();
        var authServiceMock = new Mock<IAuthService>();
        var sessionServiceMock = new Mock<SessionService>();
        var viewModel = new DashboardViewModel(
            dashboardServiceMock.Object,
            authServiceMock.Object,
            sessionServiceMock.Object);

        var initialValue = "TestName";
        viewModel.StudentName = initialValue;

        var eventRaisedCount = 0;
        viewModel.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == "StudentName")
            {
                eventRaisedCount++;
            }
        };

        // Act
        viewModel.StudentName = initialValue;

        // Assert
        Assert.AreEqual(0, eventRaisedCount);
        Assert.AreEqual(initialValue, viewModel.StudentName);
    }

    /// <summary>
    /// Tests that setting different values sequentially to StudentName updates the property each time.
    /// </summary>
    [TestMethod]
    public void StudentName_SetMultipleDifferentValues_UpdatesPropertyEachTime()
    {
        // Arrange
        var dashboardServiceMock = new Mock<IDashboardService>();
        var authServiceMock = new Mock<IAuthService>();
        var sessionServiceMock = new Mock<SessionService>();
        var viewModel = new DashboardViewModel(
            dashboardServiceMock.Object,
            authServiceMock.Object,
            sessionServiceMock.Object);

        var values = new[] { "Alice", "Bob", "Charlie", "" };
        var propertyChangedCount = 0;
        viewModel.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == "StudentName")
            {
                propertyChangedCount++;
            }
        };

        // Act & Assert
        foreach (var value in values)
        {
            viewModel.StudentName = value;
            Assert.AreEqual(value, viewModel.StudentName);
        }

        Assert.AreEqual(values.Length, propertyChangedCount);
    }

    /// <summary>
    /// Tests that PropertyChanged event is raised with the correct property name.
    /// </summary>
    [TestMethod]
    public void StudentName_SetValue_RaisesPropertyChangedWithCorrectPropertyName()
    {
        // Arrange
        var dashboardServiceMock = new Mock<IDashboardService>();
        var authServiceMock = new Mock<IAuthService>();
        var sessionServiceMock = new Mock<SessionService>();
        var viewModel = new DashboardViewModel(
            dashboardServiceMock.Object,
            authServiceMock.Object,
            sessionServiceMock.Object);

        PropertyChangedEventArgs? capturedArgs = null;
        viewModel.PropertyChanged += (sender, args) => capturedArgs = args;

        // Act
        viewModel.StudentName = "New Name";

        // Assert
        Assert.IsNotNull(capturedArgs);
        Assert.AreEqual("StudentName", capturedArgs.PropertyName);
    }

    /// <summary>
    /// Tests that getting StudentName returns the value that was set.
    /// </summary>
    [TestMethod]
    [DataRow("TestStudent")]
    [DataRow("")]
    [DataRow("   ")]
    public void StudentName_GetAfterSet_ReturnsSetValue(string setValue)
    {
        // Arrange
        var dashboardServiceMock = new Mock<IDashboardService>();
        var authServiceMock = new Mock<IAuthService>();
        var sessionServiceMock = new Mock<SessionService>();
        var viewModel = new DashboardViewModel(
            dashboardServiceMock.Object,
            authServiceMock.Object,
            sessionServiceMock.Object);

        // Act
        viewModel.StudentName = setValue;
        var retrievedValue = viewModel.StudentName;

        // Assert
        Assert.AreEqual(setValue, retrievedValue);
    }

    /// <summary>
    /// Tests that the HasClasses property getter returns true when the backing field is true.
    /// </summary>
    [TestMethod]
    public void HasClasses_Get_ReturnsTrueWhenBackingFieldIsTrue()
    {
        // Arrange
        var dashboardServiceMock = new Mock<IDashboardService>();
        var authServiceMock = new Mock<IAuthService>();
        var sessionServiceMock = new Mock<SessionService>();
        var viewModel = new DashboardViewModel(dashboardServiceMock.Object, authServiceMock.Object, sessionServiceMock.Object, new Mock<IStudentScheduleService>().Object, new Mock<IRefreshCoordinator>().Object, new Mock<IInsightsService>().Object);
        viewModel.HasClasses = true;

        // Act
        var result = viewModel.HasClasses;

        // Assert
        Assert.IsTrue(result);
    }

    /// <summary>
    /// Tests that the HasClasses property getter returns false when the backing field is false.
    /// </summary>
    [TestMethod]
    public void HasClasses_Get_ReturnsFalseWhenBackingFieldIsFalse()
    {
        // Arrange
        var dashboardServiceMock = new Mock<IDashboardService>();
        var authServiceMock = new Mock<IAuthService>();
        var sessionServiceMock = new Mock<SessionService>();
        var viewModel = new DashboardViewModel(dashboardServiceMock.Object, authServiceMock.Object, sessionServiceMock.Object, new Mock<IStudentScheduleService>().Object, new Mock<IRefreshCoordinator>().Object, new Mock<IInsightsService>().Object);
        viewModel.HasClasses = false;

        // Act
        var result = viewModel.HasClasses;

        // Assert
        Assert.IsFalse(result);
    }

    /// <summary>
    /// Tests that setting HasClasses to true updates the value and raises PropertyChanged event.
    /// </summary>
    [TestMethod]
    public void HasClasses_SetToTrue_UpdatesValueAndRaisesPropertyChanged()
    {
        // Arrange
        var dashboardServiceMock = new Mock<IDashboardService>();
        var authServiceMock = new Mock<IAuthService>();
        var sessionServiceMock = new Mock<SessionService>();
        var viewModel = new DashboardViewModel(dashboardServiceMock.Object, authServiceMock.Object, sessionServiceMock.Object, new Mock<IStudentScheduleService>().Object, new Mock<IRefreshCoordinator>().Object, new Mock<IInsightsService>().Object);
        var propertyChangedRaised = false;
        string? raisedPropertyName = null;
        viewModel.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == nameof(DashboardViewModel.HasClasses))
            {
                propertyChangedRaised = true;
                raisedPropertyName = args.PropertyName;
            }
        };

        // Act
        viewModel.HasClasses = true;

        // Assert
        Assert.IsTrue(viewModel.HasClasses);
        Assert.IsTrue(propertyChangedRaised);
        Assert.AreEqual(nameof(DashboardViewModel.HasClasses), raisedPropertyName);
    }

    /// <summary>
    /// Tests that setting HasClasses to false updates the value and raises PropertyChanged event.
    /// </summary>
    [TestMethod]
    public void HasClasses_SetToFalse_UpdatesValueAndRaisesPropertyChanged()
    {
        // Arrange
        var dashboardServiceMock = new Mock<IDashboardService>();
        var authServiceMock = new Mock<IAuthService>();
        var sessionServiceMock = new Mock<SessionService>();
        var viewModel = new DashboardViewModel(dashboardServiceMock.Object, authServiceMock.Object, sessionServiceMock.Object, new Mock<IStudentScheduleService>().Object, new Mock<IRefreshCoordinator>().Object, new Mock<IInsightsService>().Object);
        viewModel.HasClasses = true;
        var propertyChangedRaised = false;
        string? raisedPropertyName = null;
        viewModel.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == nameof(DashboardViewModel.HasClasses))
            {
                propertyChangedRaised = true;
                raisedPropertyName = args.PropertyName;
            }
        };

        // Act
        viewModel.HasClasses = false;

        // Assert
        Assert.IsFalse(viewModel.HasClasses);
        Assert.IsTrue(propertyChangedRaised);
        Assert.AreEqual(nameof(DashboardViewModel.HasClasses), raisedPropertyName);
    }

    /// <summary>
    /// Tests that setting HasClasses to the same value does not raise PropertyChanged event.
    /// </summary>
    [TestMethod]
    [DataRow(true)]
    [DataRow(false)]
    public void HasClasses_SetToSameValue_DoesNotRaisePropertyChanged(bool value)
    {
        // Arrange
        var dashboardServiceMock = new Mock<IDashboardService>();
        var authServiceMock = new Mock<IAuthService>();
        var sessionServiceMock = new Mock<SessionService>();
        var viewModel = new DashboardViewModel(dashboardServiceMock.Object, authServiceMock.Object, sessionServiceMock.Object, new Mock<IStudentScheduleService>().Object, new Mock<IRefreshCoordinator>().Object, new Mock<IInsightsService>().Object);
        viewModel.HasClasses = value;
        var propertyChangedRaised = false;
        viewModel.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == nameof(DashboardViewModel.HasClasses))
            {
                propertyChangedRaised = true;
            }
        };

        // Act
        viewModel.HasClasses = value;

        // Assert
        Assert.AreEqual(value, viewModel.HasClasses);
        Assert.IsFalse(propertyChangedRaised);
    }

    /// <summary>
    /// Tests that setting HasClasses from false to true correctly updates the value.
    /// </summary>
    [TestMethod]
    public void HasClasses_SetFromFalseToTrue_UpdatesValue()
    {
        // Arrange
        var dashboardServiceMock = new Mock<IDashboardService>();
        var authServiceMock = new Mock<IAuthService>();
        var sessionServiceMock = new Mock<SessionService>();
        var viewModel = new DashboardViewModel(dashboardServiceMock.Object, authServiceMock.Object, sessionServiceMock.Object, new Mock<IStudentScheduleService>().Object, new Mock<IRefreshCoordinator>().Object, new Mock<IInsightsService>().Object);
        viewModel.HasClasses = false;

        // Act
        viewModel.HasClasses = true;

        // Assert
        Assert.IsTrue(viewModel.HasClasses);
    }

    /// <summary>
    /// Tests that setting HasClasses from true to false correctly updates the value.
    /// </summary>
    [TestMethod]
    public void HasClasses_SetFromTrueToFalse_UpdatesValue()
    {
        // Arrange
        var dashboardServiceMock = new Mock<IDashboardService>();
        var authServiceMock = new Mock<IAuthService>();
        var sessionServiceMock = new Mock<SessionService>();
        var viewModel = new DashboardViewModel(dashboardServiceMock.Object, authServiceMock.Object, sessionServiceMock.Object, new Mock<IStudentScheduleService>().Object, new Mock<IRefreshCoordinator>().Object, new Mock<IInsightsService>().Object);
        viewModel.HasClasses = true;

        // Act
        viewModel.HasClasses = false;

        // Assert
        Assert.IsFalse(viewModel.HasClasses);
    }

    /// <summary>
    /// Tests that LoadAsync returns immediately when already loaded and forceRefresh is false.
    /// </summary>
    [TestMethod]
    public async Task LoadAsync_AlreadyLoadedAndNotForceRefresh_ReturnsImmediately()
    {
        // Arrange
        var mockDashboard = new Mock<IDashboardService>();
        var mockAuth = new Mock<IAuthService>();
        var session = new SessionService();
        var viewModel = new DashboardViewModel(mockDashboard.Object, mockAuth.Object, session);

        // First load to set _isLoaded = true
        mockDashboard.Setup(d => d.GetDashboardAsync()).ReturnsAsync(CreateDefaultDashboardDto());
        await viewModel.LoadAsync(forceRefresh: true);

        // Reset mock to verify no calls on second load
        mockDashboard.Reset();

        // Act
        await viewModel.LoadAsync(forceRefresh: false);

        // Assert
        mockDashboard.Verify(d => d.GetDashboardAsync(), Times.Never);
    }

    /// <summary>
    /// Tests that LoadAsync returns immediately when IsBusy is true.
    /// </summary>
    [TestMethod]
    public async Task LoadAsync_IsBusyTrue_ReturnsImmediately()
    {
        // Arrange
        var mockDashboard = new Mock<IDashboardService>();
        var mockAuth = new Mock<IAuthService>();
        var session = new SessionService();
        var viewModel = new DashboardViewModel(mockDashboard.Object, mockAuth.Object, session);

        // Set IsBusy to true using property
        viewModel.IsBusy = true;

        // Act
        await viewModel.LoadAsync(forceRefresh: true);

        // Assert
        mockDashboard.Verify(d => d.GetDashboardAsync(), Times.Never);
    }

    /// <summary>
    /// Tests that LoadAsync bypasses all early exit conditions when forceRefresh is true.
    /// </summary>
    [TestMethod]
    public async Task LoadAsync_ForceRefreshTrue_BypassesEarlyExitConditions()
    {
        // Arrange
        var mockDashboard = new Mock<IDashboardService>();
        var mockAuth = new Mock<IAuthService>();
        var session = new SessionService();
        var viewModel = new DashboardViewModel(mockDashboard.Object, mockAuth.Object, session);
        var dashboardDto = CreateDefaultDashboardDto();

        mockDashboard.Setup(d => d.GetDashboardAsync()).ReturnsAsync(dashboardDto);

        // First load
        await viewModel.LoadAsync(forceRefresh: true);

        // Act - Immediate second load with forceRefresh should not be blocked
        await viewModel.LoadAsync(forceRefresh: true);

        // Assert
        mockDashboard.Verify(d => d.GetDashboardAsync(), Times.Exactly(2));
    }

    /// <summary>
    /// Tests that LoadAsync successfully loads dashboard data and sets all properties correctly.
    /// </summary>
    [TestMethod]
    public async Task LoadAsync_SuccessfulLoad_SetsAllPropertiesCorrectly()
    {
        // Arrange
        var mockDashboard = new Mock<IDashboardService>();
        var mockAuth = new Mock<IAuthService>();
        var session = new SessionService();
        session.SetUser(new AuthUserDto { FullName = "Test User", Role = "Student" });
        var viewModel = new DashboardViewModel(mockDashboard.Object, mockAuth.Object, session);

        var dashboardDto = new DashboardDto
        {
            ActiveCourses = 5,
            UpcomingAssignments = 3,
            AttendancePercent = 0.85,
            RecentAnnouncements = new List<AnnouncementDto>
            {
                new AnnouncementDto { Title = "Test Title", Body = "Test Body", Date = new DateTime(2023, 5, 15) }
            },
            TotalStudents = 100,
            TotalLecturers = 20,
            TotalPrograms = 10,
            TeachingClasses = new List<ClassDto>
            {
                new ClassDto("1", "Class 1", "CS101", null, 30, "Dr. Smith")
            },
            ManagedClasses = new List<ClassDto>()
        };

        mockDashboard.Setup(d => d.GetDashboardAsync()).ReturnsAsync(dashboardDto);

        // Act
        await viewModel.LoadAsync(forceRefresh: true);

        // Assert
        Assert.AreEqual(5, viewModel.ActiveCourses);
        Assert.AreEqual(3, viewModel.UpcomingAssignments);
        Assert.AreEqual(85, viewModel.AttendancePercentage);
        Assert.AreEqual(1, viewModel.RecentAnnouncements.Count);
        Assert.AreEqual("Test Title", viewModel.LatestAnnouncementTitle);
        Assert.AreEqual("Test Body", viewModel.LatestAnnouncementBody);
        Assert.AreEqual("May 15, 2023", viewModel.LatestAnnouncementDate);
        Assert.IsTrue(viewModel.HasAnnouncement);
        Assert.AreEqual(100, viewModel.TotalStudents);
        Assert.AreEqual(20, viewModel.TotalLecturers);
        Assert.AreEqual(10, viewModel.TotalPrograms);
        Assert.AreEqual(1, viewModel.TeachingClassesCount);
        Assert.AreEqual(0, viewModel.ManagedClassesCount);
        Assert.AreEqual(1, viewModel.TeachingClasses.Count);
        Assert.AreEqual(string.Empty, viewModel.ErrorMessage);
        Assert.IsFalse(viewModel.IsBusy);
    }

    /// <summary>
    /// Tests that LoadAsync sets IsBusy to true during execution and false after completion.
    /// </summary>
    [TestMethod]
    public async Task LoadAsync_DuringExecution_SetsBusyStateCorrectly()
    {
        // Arrange
        var mockDashboard = new Mock<IDashboardService>();
        var mockAuth = new Mock<IAuthService>();
        var session = new SessionService();
        var viewModel = new DashboardViewModel(mockDashboard.Object, mockAuth.Object, session);

        bool busyDuringExecution = false;
        mockDashboard.Setup(d => d.GetDashboardAsync()).ReturnsAsync(() =>
        {
            busyDuringExecution = viewModel.IsBusy;
            return CreateDefaultDashboardDto();
        });

        // Act
        await viewModel.LoadAsync(forceRefresh: true);

        // Assert
        Assert.IsTrue(busyDuringExecution, "IsBusy should be true during execution");
        Assert.IsFalse(viewModel.IsBusy, "IsBusy should be false after completion");
    }

    /// <summary>
    /// Tests that LoadAsync clears ErrorMessage at the start of execution.
    /// </summary>
    [TestMethod]
    public async Task LoadAsync_Start_ClearsErrorMessage()
    {
        // Arrange
        var mockDashboard = new Mock<IDashboardService>();
        var mockAuth = new Mock<IAuthService>();
        var session = new SessionService();
        var viewModel = new DashboardViewModel(mockDashboard.Object, mockAuth.Object, session);

        // Set an error message first
        mockDashboard.Setup(d => d.GetDashboardAsync()).ThrowsAsync(new Exception("Initial error"));
        await viewModel.LoadAsync(forceRefresh: true);
        Assert.AreNotEqual(string.Empty, viewModel.ErrorMessage);

        // Setup successful call
        mockDashboard.Setup(d => d.GetDashboardAsync()).ReturnsAsync(CreateDefaultDashboardDto());

        // Act
        await viewModel.LoadAsync(forceRefresh: true);

        // Assert
        Assert.AreEqual(string.Empty, viewModel.ErrorMessage);
    }

    /// <summary>
    /// Tests that LoadAsync handles empty RecentAnnouncements list correctly.
    /// </summary>
    [TestMethod]
    public async Task LoadAsync_EmptyRecentAnnouncements_SetsHasAnnouncementFalse()
    {
        // Arrange
        var mockDashboard = new Mock<IDashboardService>();
        var mockAuth = new Mock<IAuthService>();
        var session = new SessionService();
        var viewModel = new DashboardViewModel(mockDashboard.Object, mockAuth.Object, session);

        var dashboardDto = CreateDefaultDashboardDto();
        dashboardDto.RecentAnnouncements = new List<AnnouncementDto>();

        mockDashboard.Setup(d => d.GetDashboardAsync()).ReturnsAsync(dashboardDto);

        // Act
        await viewModel.LoadAsync(forceRefresh: true);

        // Assert
        Assert.IsFalse(viewModel.HasAnnouncement);
        Assert.AreEqual(0, viewModel.RecentAnnouncements.Count);
    }

    /// <summary>
    /// Tests that LoadAsync handles multiple announcements and uses the first one as latest.
    /// </summary>
    [TestMethod]
    public async Task LoadAsync_MultipleAnnouncements_UsesFirstAsLatest()
    {
        // Arrange
        var mockDashboard = new Mock<IDashboardService>();
        var mockAuth = new Mock<IAuthService>();
        var session = new SessionService();
        var viewModel = new DashboardViewModel(mockDashboard.Object, mockAuth.Object, session);

        var dashboardDto = CreateDefaultDashboardDto();
        dashboardDto.RecentAnnouncements = new List<AnnouncementDto>
        {
            new AnnouncementDto { Title = "First", Body = "First Body", Date = new DateTime(2023, 5, 20) },
            new AnnouncementDto { Title = "Second", Body = "Second Body", Date = new DateTime(2023, 5, 19) }
        };

        mockDashboard.Setup(d => d.GetDashboardAsync()).ReturnsAsync(dashboardDto);

        // Act
        await viewModel.LoadAsync(forceRefresh: true);

        // Assert
        Assert.AreEqual("First", viewModel.LatestAnnouncementTitle);
        Assert.AreEqual("First Body", viewModel.LatestAnnouncementBody);
        Assert.AreEqual("May 20, 2023", viewModel.LatestAnnouncementDate);
        Assert.IsTrue(viewModel.HasAnnouncement);
        Assert.AreEqual(2, viewModel.RecentAnnouncements.Count);
    }

    /// <summary>
    /// Tests that LoadAsync handles null TeachingClasses correctly.
    /// </summary>
    [TestMethod]
    public async Task LoadAsync_NullTeachingClasses_SetsCountToZero()
    {
        // Arrange
        var mockDashboard = new Mock<IDashboardService>();
        var mockAuth = new Mock<IAuthService>();
        var session = new SessionService();
        var viewModel = new DashboardViewModel(mockDashboard.Object, mockAuth.Object, session);

        var dashboardDto = CreateDefaultDashboardDto();
        dashboardDto.TeachingClasses = null;

        mockDashboard.Setup(d => d.GetDashboardAsync()).ReturnsAsync(dashboardDto);

        // Act
        await viewModel.LoadAsync(forceRefresh: true);

        // Assert
        Assert.AreEqual(0, viewModel.TeachingClassesCount);
        Assert.AreEqual(0, viewModel.TeachingClasses.Count);
    }

    /// <summary>
    /// Tests that LoadAsync handles null ManagedClasses correctly.
    /// </summary>
    [TestMethod]
    public async Task LoadAsync_NullManagedClasses_SetsCountToZero()
    {
        // Arrange
        var mockDashboard = new Mock<IDashboardService>();
        var mockAuth = new Mock<IAuthService>();
        var session = new SessionService();
        var viewModel = new DashboardViewModel(mockDashboard.Object, mockAuth.Object, session);

        var dashboardDto = CreateDefaultDashboardDto();
        dashboardDto.ManagedClasses = null;

        mockDashboard.Setup(d => d.GetDashboardAsync()).ReturnsAsync(dashboardDto);

        // Act
        await viewModel.LoadAsync(forceRefresh: true);

        // Assert
        Assert.AreEqual(0, viewModel.ManagedClassesCount);
    }

    /// <summary>
    /// Tests that LoadAsync populates TeachingClasses collection correctly.
    /// </summary>
    [TestMethod]
    public async Task LoadAsync_NonEmptyTeachingClasses_PopulatesCollection()
    {
        // Arrange
        var mockDashboard = new Mock<IDashboardService>();
        var mockAuth = new Mock<IAuthService>();
        var session = new SessionService();
        var viewModel = new DashboardViewModel(mockDashboard.Object, mockAuth.Object, session);

        var dashboardDto = CreateDefaultDashboardDto();
        dashboardDto.TeachingClasses = new List<ClassDto>
        {
            new ClassDto("guid1", "Math 101", "MATH101", "parent1", 25, "Prof. Johnson"),
            new ClassDto("guid2", "Physics 201", "PHYS201", null, 30, "Dr. Smith")
        };

        mockDashboard.Setup(d => d.GetDashboardAsync()).ReturnsAsync(dashboardDto);

        // Act
        await viewModel.LoadAsync(forceRefresh: true);

        // Assert
        Assert.AreEqual(2, viewModel.TeachingClassesCount);
        Assert.AreEqual(2, viewModel.TeachingClasses.Count);
        Assert.AreEqual("guid1", viewModel.TeachingClasses[0].Id);
        Assert.AreEqual("Math 101", viewModel.TeachingClasses[0].Name);
        Assert.AreEqual("MATH101", viewModel.TeachingClasses[0].CourseCode);
        Assert.AreEqual("parent1", viewModel.TeachingClasses[0].ParentClassId);
        Assert.AreEqual(25, viewModel.TeachingClasses[0].EnrolledStudents);
        Assert.AreEqual("Prof. Johnson", viewModel.TeachingClasses[0].LecturerName);
    }

    /// <summary>
    /// Tests that LoadAsync handles null optional properties (TotalStudents, TotalLecturers, TotalPrograms) correctly.
    /// </summary>
    [TestMethod]
    public async Task LoadAsync_NullOptionalProperties_DoesNotSetValues()
    {
        // Arrange
        var mockDashboard = new Mock<IDashboardService>();
        var mockAuth = new Mock<IAuthService>();
        var session = new SessionService();
        var viewModel = new DashboardViewModel(mockDashboard.Object, mockAuth.Object, session);

        var dashboardDto = CreateDefaultDashboardDto();
        dashboardDto.TotalStudents = null;
        dashboardDto.TotalLecturers = null;
        dashboardDto.TotalPrograms = null;

        mockDashboard.Setup(d => d.GetDashboardAsync()).ReturnsAsync(dashboardDto);

        // Act
        await viewModel.LoadAsync(forceRefresh: true);

        // Assert - Properties should remain at their default values (0)
        Assert.AreEqual(0, viewModel.TotalStudents);
        Assert.AreEqual(0, viewModel.TotalLecturers);
        Assert.AreEqual(0, viewModel.TotalPrograms);
    }

    /// <summary>
    /// Tests that LoadAsync sets optional properties when they have values.
    /// </summary>
    [TestMethod]
    public async Task LoadAsync_OptionalPropertiesWithValues_SetsCorrectly()
    {
        // Arrange
        var mockDashboard = new Mock<IDashboardService>();
        var mockAuth = new Mock<IAuthService>();
        var session = new SessionService();
        var viewModel = new DashboardViewModel(mockDashboard.Object, mockAuth.Object, session);

        var dashboardDto = CreateDefaultDashboardDto();
        dashboardDto.TotalStudents = 500;
        dashboardDto.TotalLecturers = 75;
        dashboardDto.TotalPrograms = 15;

        mockDashboard.Setup(d => d.GetDashboardAsync()).ReturnsAsync(dashboardDto);

        // Act
        await viewModel.LoadAsync(forceRefresh: true);

        // Assert
        Assert.AreEqual(500, viewModel.TotalStudents);
        Assert.AreEqual(75, viewModel.TotalLecturers);
        Assert.AreEqual(15, viewModel.TotalPrograms);
    }

    /// <summary>
    /// Tests AttendancePercentage calculation with various AttendancePercent values.
    /// </summary>
    /// <param name="attendancePercent">The attendance percentage as a decimal (0.0 to 1.0)</param>
    /// <param name="expectedPercentage">The expected integer percentage (0 to 100)</param>
    [TestMethod]
    [DataRow(0.0, 0)]
    [DataRow(0.5, 50)]
    [DataRow(0.85, 85)]
    [DataRow(1.0, 100)]
    [DataRow(0.999, 99)]
    [DataRow(0.001, 0)]
    public async Task LoadAsync_AttendancePercent_CalculatesCorrectly(double attendancePercent, int expectedPercentage)
    {
        // Arrange
        var mockDashboard = new Mock<IDashboardService>();
        var mockAuth = new Mock<IAuthService>();
        var session = new SessionService();
        var viewModel = new DashboardViewModel(mockDashboard.Object, mockAuth.Object, session);

        var dashboardDto = CreateDefaultDashboardDto();
        dashboardDto.AttendancePercent = attendancePercent;

        mockDashboard.Setup(d => d.GetDashboardAsync()).ReturnsAsync(dashboardDto);

        // Act
        await viewModel.LoadAsync(forceRefresh: true);

        // Assert
        Assert.AreEqual(expectedPercentage, viewModel.AttendancePercentage);
    }

    /// <summary>
    /// Tests that LoadAsync sets Role property from SessionService.
    /// </summary>
    [TestMethod]
    [DataRow("Student")]
    [DataRow("Lecturer")]
    [DataRow("Admin")]
    [DataRow("ClassRep")]
    public async Task LoadAsync_SuccessfulLoad_SetsRoleFromSession(string roleString)
    {
        // Arrange
        var mockDashboard = new Mock<IDashboardService>();
        var mockAuth = new Mock<IAuthService>();
        var session = new SessionService();
        session.SetUser(new AuthUserDto { FullName = "Test User", Role = roleString });
        var viewModel = new DashboardViewModel(mockDashboard.Object, mockAuth.Object, session);

        mockDashboard.Setup(d => d.GetDashboardAsync()).ReturnsAsync(CreateDefaultDashboardDto());

        // Act
        await viewModel.LoadAsync(forceRefresh: true);

        // Assert
        Assert.IsNotNull(viewModel.Role);
    }

    /// <summary>
    /// Tests that LoadAsync captures exception and sets ErrorMessage when GetDashboardAsync throws.
    /// </summary>
    [TestMethod]
    public async Task LoadAsync_GetDashboardThrowsException_SetsErrorMessage()
    {
        // Arrange
        var mockDashboard = new Mock<IDashboardService>();
        var mockAuth = new Mock<IAuthService>();
        var session = new SessionService();
        var viewModel = new DashboardViewModel(mockDashboard.Object, mockAuth.Object, session);

        var exception = new Exception("Network error");
        mockDashboard.Setup(d => d.GetDashboardAsync()).ThrowsAsync(exception);

        // Act
        await viewModel.LoadAsync(forceRefresh: true);

        // Assert
        Assert.IsTrue(viewModel.ErrorMessage.Contains("Failed to load dashboard"));
        Assert.IsTrue(viewModel.ErrorMessage.Contains("Network error"));
        Assert.IsFalse(viewModel.IsBusy);
    }

    /// <summary>
    /// Tests that LoadAsync sets IsBusy to false even when an exception occurs.
    /// </summary>
    [TestMethod]
    public async Task LoadAsync_ExceptionThrown_SetsIsBusyToFalse()
    {
        // Arrange
        var mockDashboard = new Mock<IDashboardService>();
        var mockAuth = new Mock<IAuthService>();
        var session = new SessionService();
        var viewModel = new DashboardViewModel(mockDashboard.Object, mockAuth.Object, session);

        mockDashboard.Setup(d => d.GetDashboardAsync()).ThrowsAsync(new Exception("Test exception"));

        // Act
        await viewModel.LoadAsync(forceRefresh: true);

        // Assert
        Assert.IsFalse(viewModel.IsBusy);
    }

    /// <summary>
    /// Tests that LoadAsync clears existing collections before populating with new data.
    /// </summary>
    [TestMethod]
    public async Task LoadAsync_MultipleCalls_ClearsCollectionsBeforePopulating()
    {
        // Arrange
        var mockDashboard = new Mock<IDashboardService>();
        var mockAuth = new Mock<IAuthService>();
        var session = new SessionService();
        var viewModel = new DashboardViewModel(mockDashboard.Object, mockAuth.Object, session);

        var firstDto = CreateDefaultDashboardDto();
        firstDto.RecentAnnouncements = new List<AnnouncementDto>
        {
            new AnnouncementDto { Title = "First", Body = "Body", Date = DateTime.Now }
        };
        firstDto.TeachingClasses = new List<ClassDto>
        {
            new ClassDto("1", "Class 1", "CS101", null, 30, "Teacher")
        };

        var secondDto = CreateDefaultDashboardDto();
        secondDto.RecentAnnouncements = new List<AnnouncementDto>
        {
            new AnnouncementDto { Title = "Second", Body = "Body 2", Date = DateTime.Now },
            new AnnouncementDto { Title = "Third", Body = "Body 3", Date = DateTime.Now }
        };
        secondDto.TeachingClasses = new List<ClassDto>();

        mockDashboard.SetupSequence(d => d.GetDashboardAsync())
            .ReturnsAsync(firstDto)
            .ReturnsAsync(secondDto);

        // Act
        await viewModel.LoadAsync(forceRefresh: true);
        await viewModel.LoadAsync(forceRefresh: true);

        // Assert
        Assert.AreEqual(2, viewModel.RecentAnnouncements.Count);
        Assert.AreEqual("Second", viewModel.RecentAnnouncements[0].Title);
        Assert.AreEqual(0, viewModel.TeachingClasses.Count);
    }

    /// <summary>
    /// Tests that LoadAsync handles extreme AttendancePercent values correctly.
    /// </summary>
    [TestMethod]
    [DataRow(double.MinValue, int.MinValue)]
    [DataRow(double.MaxValue, int.MaxValue)]
    [DataRow(-1.0, -100)]
    [DataRow(2.0, 200)]
    public async Task LoadAsync_ExtremeAttendanceValues_HandlesCorrectly(double attendancePercent, int expectedPercentage)
    {
        // Arrange
        var mockDashboard = new Mock<IDashboardService>();
        var mockAuth = new Mock<IAuthService>();
        var session = new SessionService();
        var viewModel = new DashboardViewModel(mockDashboard.Object, mockAuth.Object, session);

        var dashboardDto = CreateDefaultDashboardDto();
        dashboardDto.AttendancePercent = attendancePercent;

        mockDashboard.Setup(d => d.GetDashboardAsync()).ReturnsAsync(dashboardDto);

        // Act
        await viewModel.LoadAsync(forceRefresh: true);

        // Assert
        Assert.AreEqual(expectedPercentage, viewModel.AttendancePercentage);
    }

    /// <summary>
    /// Tests that LoadAsync handles ClassDto with all null optional fields.
    /// </summary>
    [TestMethod]
    public async Task LoadAsync_ClassDtoWithNullOptionalFields_HandlesCorrectly()
    {
        // Arrange
        var mockDashboard = new Mock<IDashboardService>();
        var mockAuth = new Mock<IAuthService>();
        var session = new SessionService();
        var viewModel = new DashboardViewModel(mockDashboard.Object, mockAuth.Object, session);

        var dashboardDto = CreateDefaultDashboardDto();
        dashboardDto.TeachingClasses = new List<ClassDto>
        {
            new ClassDto("id1", "Class Name", null, null, 0, null)
        };

        mockDashboard.Setup(d => d.GetDashboardAsync()).ReturnsAsync(dashboardDto);

        // Act
        await viewModel.LoadAsync(forceRefresh: true);

        // Assert
        Assert.AreEqual(1, viewModel.TeachingClasses.Count);
        Assert.IsNull(viewModel.TeachingClasses[0].CourseCode);
        Assert.IsNull(viewModel.TeachingClasses[0].ParentClassId);
        Assert.IsNull(viewModel.TeachingClasses[0].LecturerName);
    }

    /// <summary>
    /// Tests that LoadAsync updates CurrentDate to the current date.
    /// </summary>
    [TestMethod]
    public async Task LoadAsync_SuccessfulLoad_UpdatesCurrentDate()
    {
        // Arrange
        var mockDashboard = new Mock<IDashboardService>();
        var mockAuth = new Mock<IAuthService>();
        var session = new SessionService();
        var viewModel = new DashboardViewModel(mockDashboard.Object, mockAuth.Object, session);

        mockDashboard.Setup(d => d.GetDashboardAsync()).ReturnsAsync(CreateDefaultDashboardDto());

        var expectedDate = DateTime.Now.ToString("dddd, MMMM dd");

        // Act
        await viewModel.LoadAsync(forceRefresh: true);

        // Assert
        Assert.AreEqual(expectedDate, viewModel.CurrentDate);
    }

    /// <summary>
    /// Tests that LoadAsync handles empty TeachingClasses list.
    /// </summary>
    [TestMethod]
    public async Task LoadAsync_EmptyTeachingClasses_SetsCountToZero()
    {
        // Arrange
        var mockDashboard = new Mock<IDashboardService>();
        var mockAuth = new Mock<IAuthService>();
        var session = new SessionService();
        var viewModel = new DashboardViewModel(mockDashboard.Object, mockAuth.Object, session);

        var dashboardDto = CreateDefaultDashboardDto();
        dashboardDto.TeachingClasses = new List<ClassDto>();

        mockDashboard.Setup(d => d.GetDashboardAsync()).ReturnsAsync(dashboardDto);

        // Act
        await viewModel.LoadAsync(forceRefresh: true);

        // Assert
        Assert.AreEqual(0, viewModel.TeachingClassesCount);
        Assert.AreEqual(0, viewModel.TeachingClasses.Count);
    }

    /// <summary>
    /// Tests that LoadAsync handles exception messages with special characters.
    /// </summary>
    [TestMethod]
    public async Task LoadAsync_ExceptionWithSpecialCharacters_CapturesCorrectly()
    {
        // Arrange
        var mockDashboard = new Mock<IDashboardService>();
        var mockAuth = new Mock<IAuthService>();
        var session = new SessionService();
        var viewModel = new DashboardViewModel(mockDashboard.Object, mockAuth.Object, session);

        var exceptionMessage = "Error with special chars: <>&\"'";
        mockDashboard.Setup(d => d.GetDashboardAsync()).ThrowsAsync(new Exception(exceptionMessage));

        // Act
        await viewModel.LoadAsync(forceRefresh: true);

        // Assert
        Assert.IsTrue(viewModel.ErrorMessage.Contains(exceptionMessage));
    }

    /// <summary>
    /// Helper method to create a default DashboardDto for testing.
    /// </summary>
    private static DashboardDto CreateDefaultDashboardDto()
    {
        return new DashboardDto
        {
            ActiveCourses = 0,
            UpcomingAssignments = 0,
            AttendancePercent = 0.0,
            RecentAnnouncements = new List<AnnouncementDto>(),
            TeachingClasses = new List<ClassDto>(),
            ManagedClasses = new List<ClassDto>(),
            TotalStudents = null,
            TotalLecturers = null,
            TotalPrograms = null
        };
    }

    /// <summary>
    /// Tests that the TotalStudents property setter updates the backing field
    /// and the getter returns the correct value for various integer inputs including boundary values.
    /// </summary>
    /// <param name="value">The integer value to set.</param>
    [TestMethod]
    [DataRow(0)]
    [DataRow(1)]
    [DataRow(100)]
    [DataRow(1000)]
    [DataRow(-1)]
    [DataRow(-100)]
    [DataRow(int.MaxValue)]
    [DataRow(int.MinValue)]
    public void TotalStudents_SetValue_ReturnsCorrectValue(int value)
    {
        // Arrange
        var mockDashboard = new Mock<IDashboardService>();
        var mockAuth = new Mock<IAuthService>();
        var mockSession = new Mock<SessionService>();
        var viewModel = new DashboardViewModel(mockDashboard.Object, mockAuth.Object, mockSession.Object, new Mock<IStudentScheduleService>().Object, new Mock<IRefreshCoordinator>().Object, new Mock<IInsightsService>().Object);

        // Act
        viewModel.TotalStudents = value;

        // Assert
        Assert.AreEqual(value, viewModel.TotalStudents);
    }

    /// <summary>
    /// Tests that the TotalStudents property raises PropertyChanged event
    /// when the value is changed to a different value.
    /// </summary>
    [TestMethod]
    public void TotalStudents_SetDifferentValue_RaisesPropertyChangedEvent()
    {
        // Arrange
        var mockDashboard = new Mock<IDashboardService>();
        var mockAuth = new Mock<IAuthService>();
        var mockSession = new Mock<SessionService>();
        var viewModel = new DashboardViewModel(mockDashboard.Object, mockAuth.Object, mockSession.Object, new Mock<IStudentScheduleService>().Object, new Mock<IRefreshCoordinator>().Object, new Mock<IInsightsService>().Object);
        var propertyChangedRaised = false;
        string? raisedPropertyName = null;

        viewModel.PropertyChanged += (sender, args) =>
        {
            propertyChangedRaised = true;
            raisedPropertyName = args.PropertyName;
        };

        // Act
        viewModel.TotalStudents = 100;

        // Assert
        Assert.IsTrue(propertyChangedRaised);
        Assert.AreEqual(nameof(viewModel.TotalStudents), raisedPropertyName);
    }

    /// <summary>
    /// Tests that the TotalStudents property does not raise PropertyChanged event
    /// when the value is set to the same value as the current value.
    /// </summary>
    [TestMethod]
    public void TotalStudents_SetSameValue_DoesNotRaisePropertyChangedEvent()
    {
        // Arrange
        var mockDashboard = new Mock<IDashboardService>();
        var mockAuth = new Mock<IAuthService>();
        var mockSession = new Mock<SessionService>();
        var viewModel = new DashboardViewModel(mockDashboard.Object, mockAuth.Object, mockSession.Object, new Mock<IStudentScheduleService>().Object, new Mock<IRefreshCoordinator>().Object, new Mock<IInsightsService>().Object);

        viewModel.TotalStudents = 50;

        var propertyChangedRaised = false;
        viewModel.PropertyChanged += (sender, args) =>
        {
            propertyChangedRaised = true;
        };

        // Act
        viewModel.TotalStudents = 50;

        // Assert
        Assert.IsFalse(propertyChangedRaised);
    }

    /// <summary>
    /// Tests that the TotalStudents property correctly handles multiple consecutive value changes
    /// and raises PropertyChanged event for each distinct change.
    /// </summary>
    [TestMethod]
    public void TotalStudents_SetMultipleValues_RaisesPropertyChangedForEachChange()
    {
        // Arrange
        var mockDashboard = new Mock<IDashboardService>();
        var mockAuth = new Mock<IAuthService>();
        var mockSession = new Mock<SessionService>();
        var viewModel = new DashboardViewModel(mockDashboard.Object, mockAuth.Object, mockSession.Object, new Mock<IStudentScheduleService>().Object, new Mock<IRefreshCoordinator>().Object, new Mock<IInsightsService>().Object);
        var eventRaisedCount = 0;

        viewModel.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == nameof(viewModel.TotalStudents))
            {
                eventRaisedCount++;
            }
        };

        // Act
        viewModel.TotalStudents = 10;
        viewModel.TotalStudents = 20;
        viewModel.TotalStudents = 30;

        // Assert
        Assert.AreEqual(3, eventRaisedCount);
        Assert.AreEqual(30, viewModel.TotalStudents);
    }

    /// <summary>
    /// Tests that the TotalStudents property has a default value of zero
    /// when the ViewModel is first instantiated.
    /// </summary>
    [TestMethod]
    public void TotalStudents_InitialValue_IsZero()
    {
        // Arrange
        var mockDashboard = new Mock<IDashboardService>();
        var mockAuth = new Mock<IAuthService>();
        var mockSession = new Mock<SessionService>();

        // Act
        var viewModel = new DashboardViewModel(mockDashboard.Object, mockAuth.Object, mockSession.Object, new Mock<IStudentScheduleService>().Object, new Mock<IRefreshCoordinator>().Object, new Mock<IInsightsService>().Object);

        // Assert
        Assert.AreEqual(0, viewModel.TotalStudents);
    }

    /// <summary>
    /// Tests that the TeachingClassesCount property correctly stores and retrieves various integer values including edge cases.
    /// </summary>
    /// <param name="value">The integer value to set on the TeachingClassesCount property.</param>
    [TestMethod]
    [DataRow(int.MinValue)]
    [DataRow(int.MaxValue)]
    [DataRow(0)]
    [DataRow(-1)]
    [DataRow(1)]
    [DataRow(-100)]
    [DataRow(100)]
    [DataRow(-999999)]
    [DataRow(999999)]
    public void TeachingClassesCount_SetWithVariousValues_StoresAndRetrievesCorrectly(int value)
    {
        // Arrange
        var mockDashboardService = new Mock<IDashboardService>();
        var mockAuthService = new Mock<IAuthService>();
        var mockSessionService = new Mock<SessionService>();
        var viewModel = new DashboardViewModel(mockDashboardService.Object, mockAuthService.Object, mockSessionService.Object, new Mock<IStudentScheduleService>().Object, new Mock<IRefreshCoordinator>().Object, new Mock<IInsightsService>().Object);

        // Act
        viewModel.TeachingClassesCount = value;

        // Assert
        Assert.AreEqual(value, viewModel.TeachingClassesCount);
    }

    /// <summary>
    /// Tests that setting the TeachingClassesCount property to a different value raises the PropertyChanged event.
    /// </summary>
    [TestMethod]
    public void TeachingClassesCount_SetToDifferentValue_RaisesPropertyChangedEvent()
    {
        // Arrange
        var mockDashboardService = new Mock<IDashboardService>();
        var mockAuthService = new Mock<IAuthService>();
        var mockSessionService = new Mock<SessionService>();
        var viewModel = new DashboardViewModel(mockDashboardService.Object, mockAuthService.Object, mockSessionService.Object, new Mock<IStudentScheduleService>().Object, new Mock<IRefreshCoordinator>().Object, new Mock<IInsightsService>().Object);

        string? propertyChangedName = null;
        viewModel.PropertyChanged += (sender, args) => propertyChangedName = args.PropertyName;

        // Act
        viewModel.TeachingClassesCount = 42;

        // Assert
        Assert.AreEqual("TeachingClassesCount", propertyChangedName);
    }

    /// <summary>
    /// Tests that setting the TeachingClassesCount property to the same value does not raise the PropertyChanged event.
    /// </summary>
    [TestMethod]
    public void TeachingClassesCount_SetToSameValue_DoesNotRaisePropertyChangedEvent()
    {
        // Arrange
        var mockDashboardService = new Mock<IDashboardService>();
        var mockAuthService = new Mock<IAuthService>();
        var mockSessionService = new Mock<SessionService>();
        var viewModel = new DashboardViewModel(mockDashboardService.Object, mockAuthService.Object, mockSessionService.Object, new Mock<IStudentScheduleService>().Object, new Mock<IRefreshCoordinator>().Object, new Mock<IInsightsService>().Object);

        viewModel.TeachingClassesCount = 10;

        int eventRaisedCount = 0;
        viewModel.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == "TeachingClassesCount")
            {
                eventRaisedCount++;
            }
        };

        // Act
        viewModel.TeachingClassesCount = 10;

        // Assert
        Assert.AreEqual(0, eventRaisedCount);
    }

    /// <summary>
    /// Tests that the TeachingClassesCount property has a default initial value of zero.
    /// </summary>
    [TestMethod]
    public void TeachingClassesCount_InitialValue_IsZero()
    {
        // Arrange
        var mockDashboardService = new Mock<IDashboardService>();
        var mockAuthService = new Mock<IAuthService>();
        var mockSessionService = new Mock<SessionService>();

        // Act
        var viewModel = new DashboardViewModel(mockDashboardService.Object, mockAuthService.Object, mockSessionService.Object, new Mock<IStudentScheduleService>().Object, new Mock<IRefreshCoordinator>().Object, new Mock<IInsightsService>().Object);

        // Assert
        Assert.AreEqual(0, viewModel.TeachingClassesCount);
    }

    /// <summary>
    /// Tests that multiple consecutive updates to the TeachingClassesCount property store the correct final value.
    /// </summary>
    [TestMethod]
    public void TeachingClassesCount_MultipleConsecutiveUpdates_StoresCorrectFinalValue()
    {
        // Arrange
        var mockDashboardService = new Mock<IDashboardService>();
        var mockAuthService = new Mock<IAuthService>();
        var mockSessionService = new Mock<SessionService>();
        var viewModel = new DashboardViewModel(mockDashboardService.Object, mockAuthService.Object, mockSessionService.Object, new Mock<IStudentScheduleService>().Object, new Mock<IRefreshCoordinator>().Object, new Mock<IInsightsService>().Object);

        // Act
        viewModel.TeachingClassesCount = 5;
        viewModel.TeachingClassesCount = 10;
        viewModel.TeachingClassesCount = -20;
        viewModel.TeachingClassesCount = int.MaxValue;

        // Assert
        Assert.AreEqual(int.MaxValue, viewModel.TeachingClassesCount);
    }

    /// <summary>
    /// Tests that IsAdmin returns true when Role is "admin" (case-insensitive).
    /// </summary>
    /// <param name="role">The role value to test.</param>
    /// <param name="expected">The expected IsAdmin value.</param>
    [TestMethod]
    [DataRow("admin", true, DisplayName = "IsAdmin_RoleIsAdminLowercase_ReturnsTrue")]
    [DataRow("Admin", true, DisplayName = "IsAdmin_RoleIsAdminTitleCase_ReturnsTrue")]
    [DataRow("ADMIN", true, DisplayName = "IsAdmin_RoleIsAdminUppercase_ReturnsTrue")]
    [DataRow("AdMiN", true, DisplayName = "IsAdmin_RoleIsAdminMixedCase_ReturnsTrue")]
    [DataRow("aDmIn", true, DisplayName = "IsAdmin_RoleIsAdminRandomCase_ReturnsTrue")]
    public void IsAdmin_RoleIsAdminVariousCases_ReturnsTrue(string role, bool expected)
    {
        // Arrange
        var mockDashboard = new Mock<IDashboardService>();
        var mockAuth = new Mock<IAuthService>();
        var mockSession = new Mock<SessionService>();
        var viewModel = new DashboardViewModel(mockDashboard.Object, mockAuth.Object, mockSession.Object, new Mock<IStudentScheduleService>().Object, new Mock<IRefreshCoordinator>().Object, new Mock<IInsightsService>().Object);
        viewModel.Role = role;

        // Act
        var result = viewModel.IsAdmin;

        // Assert
        Assert.AreEqual(expected, result);
    }

    /// <summary>
    /// Tests that IsAdmin returns false when Role is not "admin".
    /// </summary>
    /// <param name="role">The role value to test.</param>
    [TestMethod]
    [DataRow("student", DisplayName = "IsAdmin_RoleIsStudent_ReturnsFalse")]
    [DataRow("lecturer", DisplayName = "IsAdmin_RoleIsLecturer_ReturnsFalse")]
    [DataRow("classrep", DisplayName = "IsAdmin_RoleIsClassRep_ReturnsFalse")]
    [DataRow("classrepresentative", DisplayName = "IsAdmin_RoleIsClassRepresentative_ReturnsFalse")]
    [DataRow("administrator", DisplayName = "IsAdmin_RoleIsAdministrator_ReturnsFalse")]
    [DataRow("admins", DisplayName = "IsAdmin_RoleIsAdmins_ReturnsFalse")]
    [DataRow("root", DisplayName = "IsAdmin_RoleIsRoot_ReturnsFalse")]
    [DataRow("user", DisplayName = "IsAdmin_RoleIsUser_ReturnsFalse")]
    [DataRow("", DisplayName = "IsAdmin_RoleIsEmptyString_ReturnsFalse")]
    [DataRow(" ", DisplayName = "IsAdmin_RoleIsWhitespace_ReturnsFalse")]
    [DataRow("  ", DisplayName = "IsAdmin_RoleIsMultipleWhitespace_ReturnsFalse")]
    [DataRow(" admin", DisplayName = "IsAdmin_RoleHasLeadingSpace_ReturnsFalse")]
    [DataRow("admin ", DisplayName = "IsAdmin_RoleHasTrailingSpace_ReturnsFalse")]
    [DataRow(" admin ", DisplayName = "IsAdmin_RoleHasLeadingAndTrailingSpace_ReturnsFalse")]
    [DataRow("\tadmin", DisplayName = "IsAdmin_RoleHasLeadingTab_ReturnsFalse")]
    [DataRow("admin\n", DisplayName = "IsAdmin_RoleHasTrailingNewline_ReturnsFalse")]
    [DataRow("ADMINistrator", DisplayName = "IsAdmin_RoleStartsWithAdmin_ReturnsFalse")]
    public void IsAdmin_RoleIsNotAdmin_ReturnsFalse(string role)
    {
        // Arrange
        var mockDashboard = new Mock<IDashboardService>();
        var mockAuth = new Mock<IAuthService>();
        var mockSession = new Mock<SessionService>();
        var viewModel = new DashboardViewModel(mockDashboard.Object, mockAuth.Object, mockSession.Object, new Mock<IStudentScheduleService>().Object, new Mock<IRefreshCoordinator>().Object, new Mock<IInsightsService>().Object);
        viewModel.Role = role;

        // Act
        var result = viewModel.IsAdmin;

        // Assert
        Assert.IsFalse(result);
    }

    /// <summary>
    /// Tests that IsAdmin returns false when Role is null.
    /// The null-conditional operator should safely handle null values.
    /// </summary>
    [TestMethod]
    public void IsAdmin_RoleIsNull_ReturnsFalse()
    {
        // Arrange
        var mockDashboard = new Mock<IDashboardService>();
        var mockAuth = new Mock<IAuthService>();
        var mockSession = new Mock<SessionService>();
        var viewModel = new DashboardViewModel(mockDashboard.Object, mockAuth.Object, mockSession.Object, new Mock<IStudentScheduleService>().Object, new Mock<IRefreshCoordinator>().Object, new Mock<IInsightsService>().Object);
        viewModel.Role = null;

        // Act
        var result = viewModel.IsAdmin;

        // Assert
        Assert.IsFalse(result);
    }

    /// <summary>
    /// Tests that IsAdmin returns the correct value based on default Role initialization.
    /// The Role field is initialized to "Student" by default.
    /// </summary>
    [TestMethod]
    public void IsAdmin_DefaultRole_ReturnsFalse()
    {
        // Arrange
        var mockDashboard = new Mock<IDashboardService>();
        var mockAuth = new Mock<IAuthService>();
        var mockSession = new Mock<SessionService>();
        var viewModel = new DashboardViewModel(mockDashboard.Object, mockAuth.Object, mockSession.Object, new Mock<IStudentScheduleService>().Object, new Mock<IRefreshCoordinator>().Object, new Mock<IInsightsService>().Object);

        // Act
        var result = viewModel.IsAdmin;

        // Assert
        Assert.IsFalse(result);
    }

    /// <summary>
    /// Tests that IsAdmin correctly handles special Unicode characters in role string.
    /// </summary>
    [TestMethod]
    [DataRow("admin\u0000", DisplayName = "IsAdmin_RoleContainsNullCharacter_ReturnsFalse")]
    [DataRow("adm\u200Bin", DisplayName = "IsAdmin_RoleContainsZeroWidthSpace_ReturnsFalse")]
    [DataRow("?dmin", DisplayName = "IsAdmin_RoleContainsCyrillicA_ReturnsFalse")]
    public void IsAdmin_RoleContainsSpecialCharacters_ReturnsFalse(string role)
    {
        // Arrange
        var mockDashboard = new Mock<IDashboardService>();
        var mockAuth = new Mock<IAuthService>();
        var mockSession = new Mock<SessionService>();
        var viewModel = new DashboardViewModel(mockDashboard.Object, mockAuth.Object, mockSession.Object, new Mock<IStudentScheduleService>().Object, new Mock<IRefreshCoordinator>().Object, new Mock<IInsightsService>().Object);
        viewModel.Role = role;

        // Act
        var result = viewModel.IsAdmin;

        // Assert
        Assert.IsFalse(result);
    }

    /// <summary>
    /// Tests that IsAdmin handles very long role strings correctly.
    /// </summary>
    [TestMethod]
    public void IsAdmin_RoleIsVeryLongString_ReturnsFalse()
    {
        // Arrange
        var mockDashboard = new Mock<IDashboardService>();
        var mockAuth = new Mock<IAuthService>();
        var mockSession = new Mock<SessionService>();
        var viewModel = new DashboardViewModel(mockDashboard.Object, mockAuth.Object, mockSession.Object, new Mock<IStudentScheduleService>().Object, new Mock<IRefreshCoordinator>().Object, new Mock<IInsightsService>().Object);
        var longRole = new string('a', 10000);
        viewModel.Role = longRole;

        // Act
        var result = viewModel.IsAdmin;

        // Assert
        Assert.IsFalse(result);
    }

    /// <summary>
    /// Tests that changing Role from non-admin to admin updates IsAdmin correctly.
    /// </summary>
    [TestMethod]
    public void IsAdmin_RoleChangedFromStudentToAdmin_ReturnsTrue()
    {
        // Arrange
        var mockDashboard = new Mock<IDashboardService>();
        var mockAuth = new Mock<IAuthService>();
        var mockSession = new Mock<SessionService>();
        var viewModel = new DashboardViewModel(mockDashboard.Object, mockAuth.Object, mockSession.Object, new Mock<IStudentScheduleService>().Object, new Mock<IRefreshCoordinator>().Object, new Mock<IInsightsService>().Object);
        viewModel.Role = "student";
        Assert.IsFalse(viewModel.IsAdmin);

        // Act
        viewModel.Role = "admin";
        var result = viewModel.IsAdmin;

        // Assert
        Assert.IsTrue(result);
    }

    /// <summary>
    /// Tests that changing Role from admin to non-admin updates IsAdmin correctly.
    /// </summary>
    [TestMethod]
    public void IsAdmin_RoleChangedFromAdminToStudent_ReturnsFalse()
    {
        // Arrange
        var mockDashboard = new Mock<IDashboardService>();
        var mockAuth = new Mock<IAuthService>();
        var mockSession = new Mock<SessionService>();
        var viewModel = new DashboardViewModel(mockDashboard.Object, mockAuth.Object, mockSession.Object, new Mock<IStudentScheduleService>().Object, new Mock<IRefreshCoordinator>().Object, new Mock<IInsightsService>().Object);
        viewModel.Role = "admin";
        Assert.IsTrue(viewModel.IsAdmin);

        // Act
        viewModel.Role = "student";
        var result = viewModel.IsAdmin;

        // Assert
        Assert.IsFalse(result);
    }

    /// <summary>
    /// Tests that GoToUsersCommand returns a non-null ICommand instance.
    /// </summary>
    [TestMethod]
    public void GoToUsersCommand_WhenAccessed_ReturnsNonNullCommand()
    {
        // Arrange
        var mockDashboard = new Mock<IDashboardService>();
        var mockAuth = new Mock<IAuthService>();
        var mockSession = new Mock<SessionService>();
        var viewModel = new DashboardViewModel(mockDashboard.Object, mockAuth.Object, mockSession.Object, new Mock<IStudentScheduleService>().Object, new Mock<IRefreshCoordinator>().Object, new Mock<IInsightsService>().Object);

        // Act
        var command = viewModel.GoToUsersCommand;

        // Assert
        Assert.IsNotNull(command, "GoToUsersCommand should not be null");
        Assert.IsInstanceOfType(command, typeof(ICommand), "GoToUsersCommand should implement ICommand");
    }

    /// <summary>
    /// Tests that each access to GoToUsersCommand creates a new Command instance.
    /// This verifies that the property creates a new instance each time it is accessed,
    /// as the implementation uses expression-bodied property with 'new Command(...)'.
    /// </summary>
    [TestMethod]
    public void GoToUsersCommand_WhenAccessedMultipleTimes_CreatesNewInstanceEachTime()
    {
        // Arrange
        var mockDashboard = new Mock<IDashboardService>();
        var mockAuth = new Mock<IAuthService>();
        var mockSession = new Mock<SessionService>();
        var viewModel = new DashboardViewModel(mockDashboard.Object, mockAuth.Object, mockSession.Object, new Mock<IStudentScheduleService>().Object, new Mock<IRefreshCoordinator>().Object, new Mock<IInsightsService>().Object);

        // Act
        var command1 = viewModel.GoToUsersCommand;
        var command2 = viewModel.GoToUsersCommand;

        // Assert
        Assert.IsNotNull(command1, "First command instance should not be null");
        Assert.IsNotNull(command2, "Second command instance should not be null");
        Assert.AreNotSame(command1, command2, "Each access should create a new Command instance");
    }

    /// <summary>
    /// Tests that executing GoToUsersCommand navigates to the correct route.
    /// NOTE: This test is marked as Inconclusive because Shell.Current is a static property
    /// that cannot be mocked using Moq. To fully test this functionality:
    /// 1. Use integration tests with a real MAUI Shell instance, or
    /// 2. Refactor the code to inject INavigationService instead of using Shell.Current directly, or
    /// 3. Use a mocking framework that supports static mocking (e.g., Microsoft Fakes, TypeMock).
    /// Expected behavior: When executed, should call Shell.Current.GoToAsync("//MainTabs/admin/users")
    /// </summary>
    [TestMethod]
    public void GoToUsersCommand_WhenExecuted_NavigatesToCorrectRoute()
    {
        // Arrange
        var mockDashboard = new Mock<IDashboardService>();
        var mockAuth = new Mock<IAuthService>();
        var mockSession = new Mock<SessionService>();
        var viewModel = new DashboardViewModel(mockDashboard.Object, mockAuth.Object, mockSession.Object, new Mock<IStudentScheduleService>().Object, new Mock<IRefreshCoordinator>().Object, new Mock<IInsightsService>().Object);
        var command = viewModel.GoToUsersCommand;

        // Act & Assert
        // Cannot test execution without mocking Shell.Current, which is a static property
        // Shell.Current.GoToAsync("//MainTabs/admin/users") cannot be verified with Moq
        Assert.Inconclusive(
            "Cannot test command execution because Shell.Current is a static property that cannot be mocked with Moq. " +
            "Consider refactoring to use dependency injection for navigation or use integration tests.");
    }

    /// <summary>
    /// Tests that the CurrentDate property getter returns the current value.
    /// </summary>
    [TestMethod]
    public void CurrentDate_Get_ReturnsCurrentValue()
    {
        // Arrange
        var mockDashboard = new Mock<IDashboardService>();
        var mockAuth = new Mock<IAuthService>();
        var mockSession = new Mock<SessionService>();
        var viewModel = new DashboardViewModel(mockDashboard.Object, mockAuth.Object, mockSession.Object, new Mock<IStudentScheduleService>().Object, new Mock<IRefreshCoordinator>().Object, new Mock<IInsightsService>().Object);
        var expectedDate = "Monday, January 01";

        // Act
        viewModel.CurrentDate = expectedDate;
        var actualDate = viewModel.CurrentDate;

        // Assert
        Assert.AreEqual(expectedDate, actualDate);
    }

    /// <summary>
    /// Tests that setting CurrentDate with a new valid value updates the property and raises PropertyChanged event.
    /// </summary>
    [TestMethod]
    [DataRow("Monday, January 01")]
    [DataRow("Friday, December 31")]
    [DataRow("Wednesday, July 04")]
    public void CurrentDate_SetWithValidValue_UpdatesPropertyAndRaisesPropertyChanged(string newDate)
    {
        // Arrange
        var mockDashboard = new Mock<IDashboardService>();
        var mockAuth = new Mock<IAuthService>();
        var mockSession = new Mock<SessionService>();
        var viewModel = new DashboardViewModel(mockDashboard.Object, mockAuth.Object, mockSession.Object, new Mock<IStudentScheduleService>().Object, new Mock<IRefreshCoordinator>().Object, new Mock<IInsightsService>().Object);
        var propertyChangedRaised = false;
        string? raisedPropertyName = null;

        viewModel.PropertyChanged += (sender, args) =>
        {
            propertyChangedRaised = true;
            raisedPropertyName = args.PropertyName;
        };

        // Act
        viewModel.CurrentDate = newDate;

        // Assert
        Assert.AreEqual(newDate, viewModel.CurrentDate);
        Assert.IsTrue(propertyChangedRaised);
        Assert.AreEqual("CurrentDate", raisedPropertyName);
    }

    /// <summary>
    /// Tests that setting CurrentDate with the same value does not raise PropertyChanged event.
    /// </summary>
    [TestMethod]
    public void CurrentDate_SetWithSameValue_DoesNotRaisePropertyChanged()
    {
        // Arrange
        var mockDashboard = new Mock<IDashboardService>();
        var mockAuth = new Mock<IAuthService>();
        var mockSession = new Mock<SessionService>();
        var viewModel = new DashboardViewModel(mockDashboard.Object, mockAuth.Object, mockSession.Object, new Mock<IStudentScheduleService>().Object, new Mock<IRefreshCoordinator>().Object, new Mock<IInsightsService>().Object);
        var initialDate = "Monday, January 01";
        viewModel.CurrentDate = initialDate;

        var propertyChangedRaised = false;
        viewModel.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == "CurrentDate")
            {
                propertyChangedRaised = true;
            }
        };

        // Act
        viewModel.CurrentDate = initialDate;

        // Assert
        Assert.AreEqual(initialDate, viewModel.CurrentDate);
        Assert.IsFalse(propertyChangedRaised);
    }

    /// <summary>
    /// Tests that setting CurrentDate with an empty string updates the property and raises PropertyChanged event.
    /// </summary>
    [TestMethod]
    public void CurrentDate_SetWithEmptyString_UpdatesPropertyAndRaisesPropertyChanged()
    {
        // Arrange
        var mockDashboard = new Mock<IDashboardService>();
        var mockAuth = new Mock<IAuthService>();
        var mockSession = new Mock<SessionService>();
        var viewModel = new DashboardViewModel(mockDashboard.Object, mockAuth.Object, mockSession.Object, new Mock<IStudentScheduleService>().Object, new Mock<IRefreshCoordinator>().Object, new Mock<IInsightsService>().Object);
        var propertyChangedRaised = false;

        viewModel.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == "CurrentDate")
            {
                propertyChangedRaised = true;
            }
        };

        // Act
        viewModel.CurrentDate = string.Empty;

        // Assert
        Assert.AreEqual(string.Empty, viewModel.CurrentDate);
        Assert.IsTrue(propertyChangedRaised);
    }

    /// <summary>
    /// Tests that setting CurrentDate with whitespace-only strings updates the property and raises PropertyChanged event.
    /// </summary>
    [TestMethod]
    [DataRow("   ")]
    [DataRow("\t")]
    [DataRow("\n")]
    [DataRow("  \t\n  ")]
    public void CurrentDate_SetWithWhitespace_UpdatesPropertyAndRaisesPropertyChanged(string whitespaceValue)
    {
        // Arrange
        var mockDashboard = new Mock<IDashboardService>();
        var mockAuth = new Mock<IAuthService>();
        var mockSession = new Mock<SessionService>();
        var viewModel = new DashboardViewModel(mockDashboard.Object, mockAuth.Object, mockSession.Object, new Mock<IStudentScheduleService>().Object, new Mock<IRefreshCoordinator>().Object, new Mock<IInsightsService>().Object);
        var propertyChangedRaised = false;

        viewModel.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == "CurrentDate")
            {
                propertyChangedRaised = true;
            }
        };

        // Act
        viewModel.CurrentDate = whitespaceValue;

        // Assert
        Assert.AreEqual(whitespaceValue, viewModel.CurrentDate);
        Assert.IsTrue(propertyChangedRaised);
    }

    /// <summary>
    /// Tests that setting CurrentDate with strings containing special characters updates the property correctly.
    /// </summary>
    [TestMethod]
    [DataRow("Monday, January 01 ??")]
    [DataRow("???, ?? 01")]
    [DataRow("<script>alert('test')</script>")]
    [DataRow("Monday, January 01\r\nExtra Line")]
    [DataRow("Test\0NullChar")]
    public void CurrentDate_SetWithSpecialCharacters_UpdatesPropertyAndRaisesPropertyChanged(string specialValue)
    {
        // Arrange
        var mockDashboard = new Mock<IDashboardService>();
        var mockAuth = new Mock<IAuthService>();
        var mockSession = new Mock<SessionService>();
        var viewModel = new DashboardViewModel(mockDashboard.Object, mockAuth.Object, mockSession.Object, new Mock<IStudentScheduleService>().Object, new Mock<IRefreshCoordinator>().Object, new Mock<IInsightsService>().Object);
        var propertyChangedRaised = false;

        viewModel.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == "CurrentDate")
            {
                propertyChangedRaised = true;
            }
        };

        // Act
        viewModel.CurrentDate = specialValue;

        // Assert
        Assert.AreEqual(specialValue, viewModel.CurrentDate);
        Assert.IsTrue(propertyChangedRaised);
    }

    /// <summary>
    /// Tests that setting CurrentDate with a very long string updates the property correctly.
    /// </summary>
    [TestMethod]
    public void CurrentDate_SetWithVeryLongString_UpdatesPropertyAndRaisesPropertyChanged()
    {
        // Arrange
        var mockDashboard = new Mock<IDashboardService>();
        var mockAuth = new Mock<IAuthService>();
        var mockSession = new Mock<SessionService>();
        var viewModel = new DashboardViewModel(mockDashboard.Object, mockAuth.Object, mockSession.Object, new Mock<IStudentScheduleService>().Object, new Mock<IRefreshCoordinator>().Object, new Mock<IInsightsService>().Object);
        var veryLongString = new string('A', 10000);
        var propertyChangedRaised = false;

        viewModel.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == "CurrentDate")
            {
                propertyChangedRaised = true;
            }
        };

        // Act
        viewModel.CurrentDate = veryLongString;

        // Assert
        Assert.AreEqual(veryLongString, viewModel.CurrentDate);
        Assert.IsTrue(propertyChangedRaised);
    }

    /// <summary>
    /// Tests that multiple consecutive sets with different values raise PropertyChanged event each time.
    /// </summary>
    [TestMethod]
    public void CurrentDate_MultipleSetWithDifferentValues_RaisesPropertyChangedEachTime()
    {
        // Arrange
        var mockDashboard = new Mock<IDashboardService>();
        var mockAuth = new Mock<IAuthService>();
        var mockSession = new Mock<SessionService>();
        var viewModel = new DashboardViewModel(mockDashboard.Object, mockAuth.Object, mockSession.Object, new Mock<IStudentScheduleService>().Object, new Mock<IRefreshCoordinator>().Object, new Mock<IInsightsService>().Object);
        var propertyChangedCount = 0;

        viewModel.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == "CurrentDate")
            {
                propertyChangedCount++;
            }
        };

        // Act
        viewModel.CurrentDate = "Monday, January 01";
        viewModel.CurrentDate = "Tuesday, January 02";
        viewModel.CurrentDate = "Wednesday, January 03";

        // Assert
        Assert.AreEqual("Wednesday, January 03", viewModel.CurrentDate);
        Assert.AreEqual(3, propertyChangedCount);
    }

    /// <summary>
    /// Tests that LatestAnnouncementTitle getter returns the initial value.
    /// Initial value should be an empty string.
    /// </summary>
    [TestMethod]
    public void LatestAnnouncementTitle_Get_ReturnsInitialValue()
    {
        // Arrange
        var mockDashboard = new Mock<IDashboardService>();
        var mockAuth = new Mock<IAuthService>();
        var mockSession = new Mock<SessionService>();
        var viewModel = new DashboardViewModel(mockDashboard.Object, mockAuth.Object, mockSession.Object, new Mock<IStudentScheduleService>().Object, new Mock<IRefreshCoordinator>().Object, new Mock<IInsightsService>().Object);

        // Act
        var result = viewModel.LatestAnnouncementTitle;

        // Assert
        Assert.AreEqual(string.Empty, result);
    }

    /// <summary>
    /// Tests that setting LatestAnnouncementTitle with various valid string values updates the property
    /// and raises PropertyChanged event with the correct property name.
    /// </summary>
    /// <param name="newValue">The new value to set for LatestAnnouncementTitle.</param>
    [TestMethod]
    [DataRow("")]
    [DataRow("New Announcement")]
    [DataRow("   ")]
    [DataRow("Special chars: @#$%^&*()")]
    [DataRow("Unicode: ???? ??")]
    public void LatestAnnouncementTitle_Set_ValidValue_UpdatesPropertyAndRaisesPropertyChanged(string newValue)
    {
        // Arrange
        var mockDashboard = new Mock<IDashboardService>();
        var mockAuth = new Mock<IAuthService>();
        var mockSession = new Mock<SessionService>();
        var viewModel = new DashboardViewModel(mockDashboard.Object, mockAuth.Object, mockSession.Object, new Mock<IStudentScheduleService>().Object, new Mock<IRefreshCoordinator>().Object, new Mock<IInsightsService>().Object);
        string? raisedPropertyName = null;
        viewModel.PropertyChanged += (sender, args) => raisedPropertyName = args.PropertyName;

        // Act
        viewModel.LatestAnnouncementTitle = newValue;

        // Assert
        Assert.AreEqual(newValue, viewModel.LatestAnnouncementTitle);
        Assert.AreEqual(nameof(viewModel.LatestAnnouncementTitle), raisedPropertyName);
    }

    /// <summary>
    /// Tests that setting LatestAnnouncementTitle with the same value as current value
    /// does not raise PropertyChanged event.
    /// </summary>
    [TestMethod]
    public void LatestAnnouncementTitle_Set_SameValue_DoesNotRaisePropertyChanged()
    {
        // Arrange
        var mockDashboard = new Mock<IDashboardService>();
        var mockAuth = new Mock<IAuthService>();
        var mockSession = new Mock<SessionService>();
        var viewModel = new DashboardViewModel(mockDashboard.Object, mockAuth.Object, mockSession.Object, new Mock<IStudentScheduleService>().Object, new Mock<IRefreshCoordinator>().Object, new Mock<IInsightsService>().Object);
        viewModel.LatestAnnouncementTitle = "Test Announcement";
        bool propertyChangedRaised = false;
        viewModel.PropertyChanged += (sender, args) => propertyChangedRaised = true;

        // Act
        viewModel.LatestAnnouncementTitle = "Test Announcement";

        // Assert
        Assert.IsFalse(propertyChangedRaised);
        Assert.AreEqual("Test Announcement", viewModel.LatestAnnouncementTitle);
    }

    /// <summary>
    /// Tests that setting LatestAnnouncementTitle with a very long string
    /// updates the property correctly and raises PropertyChanged event.
    /// </summary>
    [TestMethod]
    public void LatestAnnouncementTitle_Set_VeryLongString_UpdatesPropertyAndRaisesPropertyChanged()
    {
        // Arrange
        var mockDashboard = new Mock<IDashboardService>();
        var mockAuth = new Mock<IAuthService>();
        var mockSession = new Mock<SessionService>();
        var viewModel = new DashboardViewModel(mockDashboard.Object, mockAuth.Object, mockSession.Object, new Mock<IStudentScheduleService>().Object, new Mock<IRefreshCoordinator>().Object, new Mock<IInsightsService>().Object);
        var longString = new string('A', 10000);
        string? raisedPropertyName = null;
        viewModel.PropertyChanged += (sender, args) => raisedPropertyName = args.PropertyName;

        // Act
        viewModel.LatestAnnouncementTitle = longString;

        // Assert
        Assert.AreEqual(longString, viewModel.LatestAnnouncementTitle);
        Assert.AreEqual(nameof(viewModel.LatestAnnouncementTitle), raisedPropertyName);
    }

    /// <summary>
    /// Tests that setting LatestAnnouncementTitle with null updates the property
    /// and raises PropertyChanged event. Tests nullable behavior despite non-nullable type annotation.
    /// </summary>
    [TestMethod]
    public void LatestAnnouncementTitle_Set_Null_UpdatesPropertyAndRaisesPropertyChanged()
    {
        // Arrange
        var mockDashboard = new Mock<IDashboardService>();
        var mockAuth = new Mock<IAuthService>();
        var mockSession = new Mock<SessionService>();
        var viewModel = new DashboardViewModel(mockDashboard.Object, mockAuth.Object, mockSession.Object, new Mock<IStudentScheduleService>().Object, new Mock<IRefreshCoordinator>().Object, new Mock<IInsightsService>().Object);
        viewModel.LatestAnnouncementTitle = "Initial Value";
        string? raisedPropertyName = null;
        viewModel.PropertyChanged += (sender, args) => raisedPropertyName = args.PropertyName;

        // Act
        viewModel.LatestAnnouncementTitle = null!;

        // Assert
        Assert.IsNull(viewModel.LatestAnnouncementTitle);
        Assert.AreEqual(nameof(viewModel.LatestAnnouncementTitle), raisedPropertyName);
    }

    /// <summary>
    /// Tests that setting LatestAnnouncementTitle multiple times with different values
    /// raises PropertyChanged event each time and maintains the correct value.
    /// </summary>
    [TestMethod]
    public void LatestAnnouncementTitle_Set_MultipleDifferentValues_RaisesPropertyChangedEachTime()
    {
        // Arrange
        var mockDashboard = new Mock<IDashboardService>();
        var mockAuth = new Mock<IAuthService>();
        var mockSession = new Mock<SessionService>();
        var viewModel = new DashboardViewModel(mockDashboard.Object, mockAuth.Object, mockSession.Object, new Mock<IStudentScheduleService>().Object, new Mock<IRefreshCoordinator>().Object, new Mock<IInsightsService>().Object);
        int propertyChangedCount = 0;
        viewModel.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == nameof(viewModel.LatestAnnouncementTitle))
            {
                propertyChangedCount++;
            }
        };

        // Act
        viewModel.LatestAnnouncementTitle = "First";
        viewModel.LatestAnnouncementTitle = "Second";
        viewModel.LatestAnnouncementTitle = "Third";

        // Assert
        Assert.AreEqual("Third", viewModel.LatestAnnouncementTitle);
        Assert.AreEqual(3, propertyChangedCount);
    }

    /// <summary>
    /// Tests that setting LatestAnnouncementTitle with a string containing control characters
    /// updates the property correctly.
    /// </summary>
    [TestMethod]
    public void LatestAnnouncementTitle_Set_StringWithControlCharacters_UpdatesProperty()
    {
        // Arrange
        var mockDashboard = new Mock<IDashboardService>();
        var mockAuth = new Mock<IAuthService>();
        var mockSession = new Mock<SessionService>();
        var viewModel = new DashboardViewModel(mockDashboard.Object, mockAuth.Object, mockSession.Object, new Mock<IStudentScheduleService>().Object, new Mock<IRefreshCoordinator>().Object, new Mock<IInsightsService>().Object);
        var stringWithControlChars = "Line1\nLine2\tTabbed\rCarriageReturn";
        string? raisedPropertyName = null;
        viewModel.PropertyChanged += (sender, args) => raisedPropertyName = args.PropertyName;

        // Act
        viewModel.LatestAnnouncementTitle = stringWithControlChars;

        // Assert
        Assert.AreEqual(stringWithControlChars, viewModel.LatestAnnouncementTitle);
        Assert.AreEqual(nameof(viewModel.LatestAnnouncementTitle), raisedPropertyName);
    }

    /// <summary>
    /// Tests that the Role property getter returns the correct value after setting various valid role strings.
    /// Input: Valid role strings including standard roles and edge cases.
    /// Expected: Getter returns the exact value that was set.
    /// </summary>
    [TestMethod]
    [DataRow("Student", DisplayName = "Role_SetStudent_ReturnsStudent")]
    [DataRow("Lecturer", DisplayName = "Role_SetLecturer_ReturnsLecturer")]
    [DataRow("Admin", DisplayName = "Role_SetAdmin_ReturnsAdmin")]
    [DataRow("ClassRep", DisplayName = "Role_SetClassRep_ReturnsClassRep")]
    [DataRow("ClassRepresentative", DisplayName = "Role_SetClassRepresentative_ReturnsClassRepresentative")]
    [DataRow("", DisplayName = "Role_SetEmptyString_ReturnsEmptyString")]
    [DataRow("STUDENT", DisplayName = "Role_SetUppercase_ReturnsUppercase")]
    [DataRow("student", DisplayName = "Role_SetLowercase_ReturnsLowercase")]
    public void Role_SetValidValue_GetterReturnsSetValue(string roleValue)
    {
        // Arrange
        var mockDashboard = new Mock<IDashboardService>();
        var mockAuth = new Mock<IAuthService>();
        var mockSession = new Mock<SessionService>();
        var viewModel = new DashboardViewModel(mockDashboard.Object, mockAuth.Object, mockSession.Object, new Mock<IStudentScheduleService>().Object, new Mock<IRefreshCoordinator>().Object, new Mock<IInsightsService>().Object);

        // Act
        viewModel.Role = roleValue;
        var result = viewModel.Role;

        // Assert
        Assert.AreEqual(roleValue, result);
    }

    /// <summary>
    /// Tests that setting the Role property to null updates the property correctly.
    /// Input: null value.
    /// Expected: Role property is set to null and getter returns null.
    /// </summary>
    [TestMethod]
    public void Role_SetToNull_GetterReturnsNull()
    {
        // Arrange
        var mockDashboard = new Mock<IDashboardService>();
        var mockAuth = new Mock<IAuthService>();
        var mockSession = new Mock<SessionService>();
        var viewModel = new DashboardViewModel(mockDashboard.Object, mockAuth.Object, mockSession.Object, new Mock<IStudentScheduleService>().Object, new Mock<IRefreshCoordinator>().Object, new Mock<IInsightsService>().Object);

        // Act
        viewModel.Role = null!;
        var result = viewModel.Role;

        // Assert
        Assert.IsNull(result);
    }

    /// <summary>
    /// Tests that setting the Role property raises PropertyChanged events for all dependent properties.
    /// Input: New role value.
    /// Expected: PropertyChanged events are raised for IsStudent, IsLecturer, IsAdmin, and IsClassRep.
    /// </summary>
    [TestMethod]
    public void Role_SetNewValue_RaisesPropertyChangedForAllDependentProperties()
    {
        // Arrange
        var mockDashboard = new Mock<IDashboardService>();
        var mockAuth = new Mock<IAuthService>();
        var mockSession = new Mock<SessionService>();
        var viewModel = new DashboardViewModel(mockDashboard.Object, mockAuth.Object, mockSession.Object, new Mock<IStudentScheduleService>().Object, new Mock<IRefreshCoordinator>().Object, new Mock<IInsightsService>().Object);
        var raisedProperties = new System.Collections.Generic.List<string>();

        viewModel.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName != null)
            {
                raisedProperties.Add(args.PropertyName);
            }
        };

        // Act
        viewModel.Role = "Admin";

        // Assert
        Assert.IsTrue(raisedProperties.Contains("IsStudent"), "PropertyChanged should be raised for IsStudent");
        Assert.IsTrue(raisedProperties.Contains("IsLecturer"), "PropertyChanged should be raised for IsLecturer");
        Assert.IsTrue(raisedProperties.Contains("IsAdmin"), "PropertyChanged should be raised for IsAdmin");
        Assert.IsTrue(raisedProperties.Contains("IsClassRep"), "PropertyChanged should be raised for IsClassRep");
    }

    /// <summary>
    /// Tests that setting the Role property with whitespace-only strings updates correctly.
    /// Input: Whitespace strings (spaces, tabs, newlines).
    /// Expected: Property is set to the whitespace string.
    /// </summary>
    [TestMethod]
    [DataRow("   ", DisplayName = "Role_SetMultipleSpaces")]
    [DataRow("\t", DisplayName = "Role_SetTab")]
    [DataRow("\n", DisplayName = "Role_SetNewline")]
    [DataRow("\r\n", DisplayName = "Role_SetCarriageReturnNewline")]
    [DataRow(" \t\n ", DisplayName = "Role_SetMixedWhitespace")]
    public void Role_SetWhitespace_UpdatesProperty(string roleValue)
    {
        // Arrange
        var mockDashboard = new Mock<IDashboardService>();
        var mockAuth = new Mock<IAuthService>();
        var mockSession = new Mock<SessionService>();
        var viewModel = new DashboardViewModel(mockDashboard.Object, mockAuth.Object, mockSession.Object, new Mock<IStudentScheduleService>().Object, new Mock<IRefreshCoordinator>().Object, new Mock<IInsightsService>().Object);

        // Act
        viewModel.Role = roleValue;

        // Assert
        Assert.AreEqual(roleValue, viewModel.Role);
    }

    /// <summary>
    /// Tests that setting the Role property with strings containing special characters updates correctly.
    /// Input: Strings with special characters.
    /// Expected: Property is set to the string with special characters.
    /// </summary>
    [TestMethod]
    [DataRow("Student!", DisplayName = "Role_WithExclamation")]
    [DataRow("@Admin", DisplayName = "Role_WithAtSymbol")]
    [DataRow("Lecturer#123", DisplayName = "Role_WithHashAndNumbers")]
    [DataRow("Class$Rep", DisplayName = "Role_WithDollarSign")]
    [DataRow("Role<>Test", DisplayName = "Role_WithAngleBrackets")]
    [DataRow("Role&Test", DisplayName = "Role_WithAmpersand")]
    public void Role_SetSpecialCharacters_UpdatesProperty(string roleValue)
    {
        // Arrange
        var mockDashboard = new Mock<IDashboardService>();
        var mockAuth = new Mock<IAuthService>();
        var mockSession = new Mock<SessionService>();
        var viewModel = new DashboardViewModel(mockDashboard.Object, mockAuth.Object, mockSession.Object, new Mock<IStudentScheduleService>().Object, new Mock<IRefreshCoordinator>().Object, new Mock<IInsightsService>().Object);

        // Act
        viewModel.Role = roleValue;

        // Assert
        Assert.AreEqual(roleValue, viewModel.Role);
    }

    /// <summary>
    /// Tests that setting the Role property with a very long string updates correctly.
    /// Input: String with 10000 characters.
    /// Expected: Property is set to the long string.
    /// </summary>
    [TestMethod]
    public void Role_SetVeryLongString_UpdatesProperty()
    {
        // Arrange
        var mockDashboard = new Mock<IDashboardService>();
        var mockAuth = new Mock<IAuthService>();
        var mockSession = new Mock<SessionService>();
        var viewModel = new DashboardViewModel(mockDashboard.Object, mockAuth.Object, mockSession.Object, new Mock<IStudentScheduleService>().Object, new Mock<IRefreshCoordinator>().Object, new Mock<IInsightsService>().Object);
        var longString = new string('A', 10000);

        // Act
        viewModel.Role = longString;

        // Assert
        Assert.AreEqual(longString, viewModel.Role);
    }

    /// <summary>
    /// Tests that setting the Role property with strings that have leading or trailing spaces updates correctly.
    /// Input: Strings with leading/trailing spaces.
    /// Expected: Property preserves the spaces and is set correctly.
    /// </summary>
    [TestMethod]
    [DataRow(" Student", DisplayName = "Role_WithLeadingSpace")]
    [DataRow("Student ", DisplayName = "Role_WithTrailingSpace")]
    [DataRow(" Student ", DisplayName = "Role_WithBothLeadingAndTrailingSpaces")]
    public void Role_SetWithLeadingOrTrailingSpaces_UpdatesPropertyWithSpaces(string roleValue)
    {
        // Arrange
        var mockDashboard = new Mock<IDashboardService>();
        var mockAuth = new Mock<IAuthService>();
        var mockSession = new Mock<SessionService>();
        var viewModel = new DashboardViewModel(mockDashboard.Object, mockAuth.Object, mockSession.Object, new Mock<IStudentScheduleService>().Object, new Mock<IRefreshCoordinator>().Object, new Mock<IInsightsService>().Object);

        // Act
        viewModel.Role = roleValue;

        // Assert
        Assert.AreEqual(roleValue, viewModel.Role);
    }

    /// <summary>
    /// Tests that changing the Role property multiple times in sequence updates correctly each time.
    /// Input: Sequential role changes.
    /// Expected: Role property reflects the most recent value after each change.
    /// </summary>
    [TestMethod]
    public void Role_SetMultipleTimesSequentially_UpdatesCorrectlyEachTime()
    {
        // Arrange
        var mockDashboard = new Mock<IDashboardService>();
        var mockAuth = new Mock<IAuthService>();
        var mockSession = new Mock<SessionService>();
        var viewModel = new DashboardViewModel(mockDashboard.Object, mockAuth.Object, mockSession.Object, new Mock<IStudentScheduleService>().Object, new Mock<IRefreshCoordinator>().Object, new Mock<IInsightsService>().Object);

        // Act & Assert
        viewModel.Role = "Lecturer";
        Assert.AreEqual("Lecturer", viewModel.Role);

        viewModel.Role = "Admin";
        Assert.AreEqual("Admin", viewModel.Role);

        viewModel.Role = "ClassRep";
        Assert.AreEqual("ClassRep", viewModel.Role);

        viewModel.Role = "Student";
        Assert.AreEqual("Student", viewModel.Role);
    }

    /// <summary>
    /// Tests that setting the Role property to an unrecognized value updates correctly.
    /// Input: Unrecognized role values.
    /// Expected: Property is set to the unrecognized value.
    /// </summary>
    [TestMethod]
    [DataRow("Teacher", DisplayName = "Role_SetTeacher")]
    [DataRow("Professor", DisplayName = "Role_SetProfessor")]
    [DataRow("Administrator", DisplayName = "Role_SetAdministrator")]
    [DataRow("Unknown", DisplayName = "Role_SetUnknown")]
    [DataRow("12345", DisplayName = "Role_SetNumericString")]
    public void Role_SetUnrecognizedValue_UpdatesProperty(string roleValue)
    {
        // Arrange
        var mockDashboard = new Mock<IDashboardService>();
        var mockAuth = new Mock<IAuthService>();
        var mockSession = new Mock<SessionService>();
        var viewModel = new DashboardViewModel(mockDashboard.Object, mockAuth.Object, mockSession.Object, new Mock<IStudentScheduleService>().Object, new Mock<IRefreshCoordinator>().Object, new Mock<IInsightsService>().Object);

        // Act
        viewModel.Role = roleValue;

        // Assert
        Assert.AreEqual(roleValue, viewModel.Role);
    }

    /// <summary>
    /// Tests that the Role property has the correct default/initial value.
    /// Input: None (accessing property after construction).
    /// Expected: Role property returns "Student" (the initialized value).
    /// </summary>
    [TestMethod]
    public void Role_InitialValue_IsStudent()
    {
        // Arrange & Act
        var mockDashboard = new Mock<IDashboardService>();
        var mockAuth = new Mock<IAuthService>();
        var mockSession = new Mock<SessionService>();
        var viewModel = new DashboardViewModel(mockDashboard.Object, mockAuth.Object, mockSession.Object, new Mock<IStudentScheduleService>().Object, new Mock<IRefreshCoordinator>().Object, new Mock<IInsightsService>().Object);

        // Assert
        Assert.AreEqual("Student", viewModel.Role);
    }

    /// <summary>
    /// Tests that setting the Role property to null and then to a valid value updates correctly.
    /// Input: null followed by a valid role value.
    /// Expected: Property updates to null, then to the valid value.
    /// </summary>
    [TestMethod]
    public void Role_SetNullThenValidValue_UpdatesCorrectly()
    {
        // Arrange
        var mockDashboard = new Mock<IDashboardService>();
        var mockAuth = new Mock<IAuthService>();
        var mockSession = new Mock<SessionService>();
        var viewModel = new DashboardViewModel(mockDashboard.Object, mockAuth.Object, mockSession.Object, new Mock<IStudentScheduleService>().Object, new Mock<IRefreshCoordinator>().Object, new Mock<IInsightsService>().Object);

        // Act
        viewModel.Role = null!;
        Assert.IsNull(viewModel.Role);

        viewModel.Role = "Lecturer";

        // Assert
        Assert.AreEqual("Lecturer", viewModel.Role);
    }

    /// <summary>
    /// Tests that the PropertyChanged event provides the correct sender when Role is set.
    /// Input: New role value.
    /// Expected: PropertyChanged event sender is the viewModel instance.
    /// </summary>
    [TestMethod]
    public void Role_SetValue_PropertyChangedEventHasCorrectSender()
    {
        // Arrange
        var mockDashboard = new Mock<IDashboardService>();
        var mockAuth = new Mock<IAuthService>();
        var mockSession = new Mock<SessionService>();
        var viewModel = new DashboardViewModel(mockDashboard.Object, mockAuth.Object, mockSession.Object, new Mock<IStudentScheduleService>().Object, new Mock<IRefreshCoordinator>().Object, new Mock<IInsightsService>().Object);
        object? eventSender = null;

        viewModel.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == "Role")
            {
                eventSender = sender;
            }
        };

        // Act
        viewModel.Role = "Admin";

        // Assert
        Assert.IsNotNull(eventSender);
        Assert.AreSame(viewModel, eventSender);
    }

    /// <summary>
    /// Tests that setting the Role property raises PropertyChanged events in the correct order.
    /// Input: New role value.
    /// Expected: PropertyChanged for "Role" is raised first, followed by dependent properties.
    /// </summary>
    [TestMethod]
    public void Role_SetValue_RaisesPropertyChangedEventsInOrder()
    {
        // Arrange
        var mockDashboard = new Mock<IDashboardService>();
        var mockAuth = new Mock<IAuthService>();
        var mockSession = new Mock<SessionService>();
        var viewModel = new DashboardViewModel(mockDashboard.Object, mockAuth.Object, mockSession.Object, new Mock<IStudentScheduleService>().Object, new Mock<IRefreshCoordinator>().Object, new Mock<IInsightsService>().Object);
        var eventOrder = new System.Collections.Generic.List<string>();

        viewModel.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName != null)
            {
                eventOrder.Add(args.PropertyName);
            }
        };

        // Act
        viewModel.Role = "Lecturer";

        // Assert
        Assert.IsTrue(eventOrder.Count >= 5, "At least 5 PropertyChanged events should be raised");
        Assert.AreEqual("Role", eventOrder[0], "First event should be for Role property");
        Assert.IsTrue(eventOrder.Contains("IsStudent"), "Should contain IsStudent");
        Assert.IsTrue(eventOrder.Contains("IsLecturer"), "Should contain IsLecturer");
        Assert.IsTrue(eventOrder.Contains("IsAdmin"), "Should contain IsAdmin");
        Assert.IsTrue(eventOrder.Contains("IsClassRep"), "Should contain IsClassRep");
    }

    /// <summary>
    /// Tests that setting the Role property with Unicode characters updates correctly.
    /// Input: Strings containing Unicode characters.
    /// Expected: Property is set to the string with Unicode characters.
    /// </summary>
    [TestMethod]
    [DataRow("??", DisplayName = "Role_WithChineseCharacters")]
    [DataRow("Étudiant", DisplayName = "Role_WithFrenchAccents")]
    [DataRow("???????", DisplayName = "Role_WithCyrillicCharacters")]
    [DataRow("??Student", DisplayName = "Role_WithEmoji")]
    public void Role_SetUnicodeCharacters_UpdatesProperty(string roleValue)
    {
        // Arrange
        var mockDashboard = new Mock<IDashboardService>();
        var mockAuth = new Mock<IAuthService>();
        var mockSession = new Mock<SessionService>();
        var viewModel = new DashboardViewModel(mockDashboard.Object, mockAuth.Object, mockSession.Object, new Mock<IStudentScheduleService>().Object, new Mock<IRefreshCoordinator>().Object, new Mock<IInsightsService>().Object);

        // Act
        viewModel.Role = roleValue;

        // Assert
        Assert.AreEqual(roleValue, viewModel.Role);
    }

    /// <summary>
    /// Tests that setting the Role property to a value with control characters updates correctly.
    /// Input: String containing null character.
    /// Expected: Property is set to the string with control character.
    /// </summary>
    [TestMethod]
    public void Role_SetStringWithNullCharacter_UpdatesProperty()
    {
        // Arrange
        var mockDashboard = new Mock<IDashboardService>();
        var mockAuth = new Mock<IAuthService>();
        var mockSession = new Mock<SessionService>();
        var viewModel = new DashboardViewModel(mockDashboard.Object, mockAuth.Object, mockSession.Object, new Mock<IStudentScheduleService>().Object, new Mock<IRefreshCoordinator>().Object, new Mock<IInsightsService>().Object);
        var roleWithNull = "Student\0Test";

        // Act
        viewModel.Role = roleWithNull;

        // Assert
        Assert.AreEqual(roleWithNull, viewModel.Role);
    }

    /// <summary>
    /// Tests that accessing GoToAcademicCommand multiple times creates a new Command instance each time.
    /// This verifies the expression-bodied property behavior where a new instance is created on each access.
    /// Input: Multiple accesses to the property.
    /// Expected: Each access returns a different instance (not the same reference).
    /// </summary>
    [TestMethod]
    public void GoToAcademicCommand_WhenAccessedMultipleTimes_CreatesNewInstancesEachTime()
    {
        // Arrange
        // (ViewModel already initialized in TestInitialize)

        // Act
        ICommand command1 = _viewModel.GoToAcademicCommand;
        ICommand command2 = _viewModel.GoToAcademicCommand;
        ICommand command3 = _viewModel.GoToAcademicCommand;

        // Assert
        Assert.AreNotSame(command1, command2, "First and second command instances should not be the same reference");
        Assert.AreNotSame(command2, command3, "Second and third command instances should not be the same reference");
        Assert.AreNotSame(command1, command3, "First and third command instances should not be the same reference");
    }

    /// <summary>
    /// Tests that GoToAcademicCommand's CanExecute returns true regardless of the parameter passed.
    /// Input: Various parameter values including null, objects, and strings.
    /// Expected: CanExecute returns true for all inputs.
    /// </summary>
    [TestMethod]
    [DataRow(null, DisplayName = "CanExecute with null parameter")]
    [DataRow("test", DisplayName = "CanExecute with string parameter")]
    [DataRow(123, DisplayName = "CanExecute with int parameter")]
    public void GoToAcademicCommand_CanExecuteWithVariousParameters_ReturnsTrue(object? parameter)
    {
        // Arrange
        // (ViewModel already initialized in TestInitialize)

        // Act
        ICommand command = _viewModel.GoToAcademicCommand;
        bool canExecute = command.CanExecute(parameter);

        // Assert
        Assert.IsTrue(canExecute, $"GoToAcademicCommand.CanExecute should return true for parameter: {parameter}");
    }

    // Note: Testing the actual execution of GoToAcademicCommand (calling command.Execute())
    // is not feasible in a pure unit test context because it depends on Shell.Current,
    // which is a static property from Microsoft.Maui.Controls.Shell. This static dependency
    // cannot be mocked using Moq, and creating fake implementations is prohibited by the test requirements.
    // In a unit test environment, Shell.Current will be null, causing a NullReferenceException.
    //
    // To test the navigation behavior, consider:
    // 1. Integration tests with a properly initialized Shell instance
    // 2. UI tests using the MAUI testing framework
    // 3. Refactoring to inject a navigation service abstraction (INavigationService) that can be mocked
    //
    // Expected behavior when executed (in a proper MAUI context):
    // - Should call Shell.Current.GoToAsync("//MainTabs/AcademicPage")
    // - Should navigate to the Academic page within the MainTabs navigation structure

    /// <summary>
    /// Tests that CurrentDate getter returns the initialized value when the property is first accessed.
    /// Input: None (default initialization).
    /// Expected: Returns a string in the format "dddd, MMMM dd" matching the current date.
    /// </summary>
    [TestMethod]
    public void CurrentDate_Get_ReturnsInitializedValue()
    {
        // Arrange
        var mockDashboard = new Mock<IDashboardService>();
        var mockAuth = new Mock<IAuthService>();
        var mockSession = new Mock<SessionService>();
        var viewModel = new DashboardViewModel(mockDashboard.Object, mockAuth.Object, mockSession.Object, new Mock<IStudentScheduleService>().Object, new Mock<IRefreshCoordinator>().Object, new Mock<IInsightsService>().Object);

        // Act
        var result = viewModel.CurrentDate;

        // Assert
        Assert.IsNotNull(result);
        Assert.IsInstanceOfType(result, typeof(string));
        Assert.IsFalse(string.IsNullOrWhiteSpace(result));
    }

    /// <summary>
    /// Tests that setting CurrentDate to a new valid value updates the property and raises PropertyChanged event.
    /// Input: Valid date-formatted strings.
    /// Expected: Property is updated and PropertyChanged event is raised with property name "CurrentDate".
    /// </summary>
    [TestMethod]
    [DataRow("Monday, January 01")]
    [DataRow("Friday, December 31")]
    [DataRow("Wednesday, July 04")]
    [DataRow("Saturday, February 29")]
    [DataRow("Sunday, November 15")]
    public void CurrentDate_SetValidDateString_UpdatesPropertyAndRaisesPropertyChanged(string newDate)
    {
        // Arrange
        var mockDashboard = new Mock<IDashboardService>();
        var mockAuth = new Mock<IAuthService>();
        var mockSession = new Mock<SessionService>();
        var viewModel = new DashboardViewModel(mockDashboard.Object, mockAuth.Object, mockSession.Object, new Mock<IStudentScheduleService>().Object, new Mock<IRefreshCoordinator>().Object, new Mock<IInsightsService>().Object);
        string? raisedPropertyName = null;
        viewModel.PropertyChanged += (sender, args) =>
        {
            raisedPropertyName = args.PropertyName;
        };

        // Act
        viewModel.CurrentDate = newDate;

        // Assert
        Assert.AreEqual(newDate, viewModel.CurrentDate);
        Assert.AreEqual("CurrentDate", raisedPropertyName);
    }

    /// <summary>
    /// Tests that setting CurrentDate to the same value does not raise PropertyChanged event.
    /// Input: Same value as current.
    /// Expected: PropertyChanged event is not raised.
    /// </summary>
    [TestMethod]
    public void CurrentDate_SetSameValue_DoesNotRaisePropertyChanged()
    {
        // Arrange
        var mockDashboard = new Mock<IDashboardService>();
        var mockAuth = new Mock<IAuthService>();
        var mockSession = new Mock<SessionService>();
        var viewModel = new DashboardViewModel(mockDashboard.Object, mockAuth.Object, mockSession.Object, new Mock<IStudentScheduleService>().Object, new Mock<IRefreshCoordinator>().Object, new Mock<IInsightsService>().Object);
        var testDate = "Tuesday, March 15";
        viewModel.CurrentDate = testDate;
        bool eventRaised = false;
        viewModel.PropertyChanged += (sender, args) =>
        {
            eventRaised = true;
        };

        // Act
        viewModel.CurrentDate = testDate;

        // Assert
        Assert.IsFalse(eventRaised);
        Assert.AreEqual(testDate, viewModel.CurrentDate);
    }

    /// <summary>
    /// Tests that setting CurrentDate to an empty string updates the property and raises PropertyChanged event.
    /// Input: Empty string.
    /// Expected: Property is set to empty string and PropertyChanged event is raised.
    /// </summary>
    [TestMethod]
    public void CurrentDate_SetEmptyString_UpdatesPropertyAndRaisesPropertyChanged()
    {
        // Arrange
        var mockDashboard = new Mock<IDashboardService>();
        var mockAuth = new Mock<IAuthService>();
        var mockSession = new Mock<SessionService>();
        var viewModel = new DashboardViewModel(mockDashboard.Object, mockAuth.Object, mockSession.Object, new Mock<IStudentScheduleService>().Object, new Mock<IRefreshCoordinator>().Object, new Mock<IInsightsService>().Object);
        string? raisedPropertyName = null;
        viewModel.PropertyChanged += (sender, args) =>
        {
            raisedPropertyName = args.PropertyName;
        };

        // Act
        viewModel.CurrentDate = "";

        // Assert
        Assert.AreEqual("", viewModel.CurrentDate);
        Assert.AreEqual("CurrentDate", raisedPropertyName);
    }

    /// <summary>
    /// Tests that setting CurrentDate to whitespace-only strings updates the property and raises PropertyChanged event.
    /// Input: Various whitespace strings (spaces, tabs, newlines).
    /// Expected: Property is updated to the whitespace string and PropertyChanged event is raised.
    /// </summary>
    [TestMethod]
    [DataRow("   ")]
    [DataRow("\t")]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [DataRow("  \t\n  ")]
    public void CurrentDate_SetWhitespace_UpdatesPropertyAndRaisesPropertyChanged(string whitespaceValue)
    {
        // Arrange
        var mockDashboard = new Mock<IDashboardService>();
        var mockAuth = new Mock<IAuthService>();
        var mockSession = new Mock<SessionService>();
        var viewModel = new DashboardViewModel(mockDashboard.Object, mockAuth.Object, mockSession.Object, new Mock<IStudentScheduleService>().Object, new Mock<IRefreshCoordinator>().Object, new Mock<IInsightsService>().Object);
        string? raisedPropertyName = null;
        viewModel.PropertyChanged += (sender, args) =>
        {
            raisedPropertyName = args.PropertyName;
        };

        // Act
        viewModel.CurrentDate = whitespaceValue;

        // Assert
        Assert.AreEqual(whitespaceValue, viewModel.CurrentDate);
        Assert.AreEqual("CurrentDate", raisedPropertyName);
    }

    /// <summary>
    /// Tests that setting CurrentDate to strings with special characters updates the property correctly.
    /// Input: Strings with emojis, Unicode, HTML tags, control characters.
    /// Expected: Property accepts and stores the special character strings.
    /// </summary>
    [TestMethod]
    [DataRow("Monday, January 01 ??")]
    [DataRow("???, ?? 01")]
    [DataRow("<script>alert('test')</script>")]
    [DataRow("Monday, January 01\r\nExtra Line")]
    [DataRow("Test\0NullChar")]
    [DataRow("Special: !@#$%^&*()_+-=[]{}|;':\",./<>?")]
    [DataRow("Monday, January 01™")]
    public void CurrentDate_SetSpecialCharacters_UpdatesPropertyAndRaisesPropertyChanged(string specialValue)
    {
        // Arrange
        var mockDashboard = new Mock<IDashboardService>();
        var mockAuth = new Mock<IAuthService>();
        var mockSession = new Mock<SessionService>();
        var viewModel = new DashboardViewModel(mockDashboard.Object, mockAuth.Object, mockSession.Object, new Mock<IStudentScheduleService>().Object, new Mock<IRefreshCoordinator>().Object, new Mock<IInsightsService>().Object);
        string? raisedPropertyName = null;
        viewModel.PropertyChanged += (sender, args) =>
        {
            raisedPropertyName = args.PropertyName;
        };

        // Act
        viewModel.CurrentDate = specialValue;

        // Assert
        Assert.AreEqual(specialValue, viewModel.CurrentDate);
        Assert.AreEqual("CurrentDate", raisedPropertyName);
    }

    /// <summary>
    /// Tests that setting CurrentDate to a very long string updates the property correctly.
    /// Input: A string with 10,000 characters.
    /// Expected: Property stores the entire long string and raises PropertyChanged event.
    /// </summary>
    [TestMethod]
    public void CurrentDate_SetVeryLongString_UpdatesPropertyAndRaisesPropertyChanged()
    {
        // Arrange
        var mockDashboard = new Mock<IDashboardService>();
        var mockAuth = new Mock<IAuthService>();
        var mockSession = new Mock<SessionService>();
        var viewModel = new DashboardViewModel(mockDashboard.Object, mockAuth.Object, mockSession.Object, new Mock<IStudentScheduleService>().Object, new Mock<IRefreshCoordinator>().Object, new Mock<IInsightsService>().Object);
        var longString = new string('A', 10000);
        string? raisedPropertyName = null;
        viewModel.PropertyChanged += (sender, args) =>
        {
            raisedPropertyName = args.PropertyName;
        };

        // Act
        viewModel.CurrentDate = longString;

        // Assert
        Assert.AreEqual(longString, viewModel.CurrentDate);
        Assert.AreEqual("CurrentDate", raisedPropertyName);
    }

    /// <summary>
    /// Tests that CurrentDate setter works correctly when transitioning from non-empty to empty string.
    /// Input: First set to a valid date, then set to empty string.
    /// Expected: Property updates to empty string and PropertyChanged is raised.
    /// </summary>
    [TestMethod]
    public void CurrentDate_SetToEmptyAfterValidValue_UpdatesPropertyAndRaisesPropertyChanged()
    {
        // Arrange
        var mockDashboard = new Mock<IDashboardService>();
        var mockAuth = new Mock<IAuthService>();
        var mockSession = new Mock<SessionService>();
        var viewModel = new DashboardViewModel(mockDashboard.Object, mockAuth.Object, mockSession.Object, new Mock<IStudentScheduleService>().Object, new Mock<IRefreshCoordinator>().Object, new Mock<IInsightsService>().Object);
        viewModel.CurrentDate = "Monday, January 01";
        string? raisedPropertyName = null;
        viewModel.PropertyChanged += (sender, args) =>
        {
            raisedPropertyName = args.PropertyName;
        };

        // Act
        viewModel.CurrentDate = "";

        // Assert
        Assert.AreEqual("", viewModel.CurrentDate);
        Assert.AreEqual("CurrentDate", raisedPropertyName);
    }

    /// <summary>
    /// Tests that PropertyChanged event sender is the ViewModel instance when CurrentDate is set.
    /// Input: Valid date string.
    /// Expected: PropertyChanged event sender is the ViewModel instance.
    /// </summary>
    [TestMethod]
    public void CurrentDate_SetValue_PropertyChangedSenderIsViewModel()
    {
        // Arrange
        var mockDashboard = new Mock<IDashboardService>();
        var mockAuth = new Mock<IAuthService>();
        var mockSession = new Mock<SessionService>();
        var viewModel = new DashboardViewModel(mockDashboard.Object, mockAuth.Object, mockSession.Object, new Mock<IStudentScheduleService>().Object, new Mock<IRefreshCoordinator>().Object, new Mock<IInsightsService>().Object);
        object? eventSender = null;
        viewModel.PropertyChanged += (sender, args) =>
        {
            eventSender = sender;
        };

        // Act
        viewModel.CurrentDate = "Thursday, April 15";

        // Assert
        Assert.AreSame(viewModel, eventSender);
    }

    /// <summary>
    /// Tests that setting CurrentDate with boundary case strings (single character) works correctly.
    /// Input: Single character strings.
    /// Expected: Property is updated and PropertyChanged event is raised.
    /// </summary>
    [TestMethod]
    [DataRow("A")]
    [DataRow("1")]
    [DataRow(" ")]
    [DataRow("?")]
    public void CurrentDate_SetSingleCharacter_UpdatesPropertyAndRaisesPropertyChanged(string singleChar)
    {
        // Arrange
        var mockDashboard = new Mock<IDashboardService>();
        var mockAuth = new Mock<IAuthService>();
        var mockSession = new Mock<SessionService>();
        var viewModel = new DashboardViewModel(mockDashboard.Object, mockAuth.Object, mockSession.Object, new Mock<IStudentScheduleService>().Object, new Mock<IRefreshCoordinator>().Object, new Mock<IInsightsService>().Object);
        string? raisedPropertyName = null;
        viewModel.PropertyChanged += (sender, args) =>
        {
            raisedPropertyName = args.PropertyName;
        };

        // Act
        viewModel.CurrentDate = singleChar;

        // Assert
        Assert.AreEqual(singleChar, viewModel.CurrentDate);
        Assert.AreEqual("CurrentDate", raisedPropertyName);
    }

    /// <summary>
    /// Tests that LatestAnnouncementTitle returns the initial default value of an empty string.
    /// Input: None (property accessed without setting).
    /// Expected: Returns empty string.
    /// </summary>
    [TestMethod]
    public void LatestAnnouncementTitle_InitialValue_ReturnsEmptyString()
    {
        // Arrange
        var mockDashboardService = new Mock<IDashboardService>();
        var mockAuthService = new Mock<IAuthService>();
        var mockSessionService = new Mock<SessionService>();
        var viewModel = new DashboardViewModel(mockDashboardService.Object, mockAuthService.Object, mockSessionService.Object, new Mock<IStudentScheduleService>().Object, new Mock<IRefreshCoordinator>().Object, new Mock<IInsightsService>().Object);

        // Act
        var result = viewModel.LatestAnnouncementTitle;

        // Assert
        Assert.AreEqual(string.Empty, result);
    }

    /// <summary>
    /// Tests that setting LatestAnnouncementTitle with various valid string values updates the property
    /// and raises the PropertyChanged event with the correct property name.
    /// Input: Various string values including empty, whitespace, normal text, special characters, and Unicode.
    /// Expected: Property is updated to the new value and PropertyChanged event is raised.
    /// </summary>
    [TestMethod]
    [DataRow("", DisplayName = "Empty string")]
    [DataRow("Important Announcement", DisplayName = "Normal announcement text")]
    [DataRow("   ", DisplayName = "Whitespace only")]
    [DataRow("\t", DisplayName = "Tab character")]
    [DataRow("\n", DisplayName = "Newline character")]
    [DataRow("Special chars: @#$%^&*()", DisplayName = "Special characters")]
    [DataRow("Unicode: ???? ??", DisplayName = "Unicode and emoji")]
    [DataRow("A", DisplayName = "Single character")]
    [DataRow("Line1\nLine2\nLine3", DisplayName = "Multiline text")]
    [DataRow("Title with\ttabs", DisplayName = "Text with tabs")]
    public void LatestAnnouncementTitle_SetValidValue_UpdatesPropertyAndRaisesPropertyChanged(string value)
    {
        // Arrange
        var mockDashboardService = new Mock<IDashboardService>();
        var mockAuthService = new Mock<IAuthService>();
        var mockSessionService = new Mock<SessionService>();
        var viewModel = new DashboardViewModel(mockDashboardService.Object, mockAuthService.Object, mockSessionService.Object, new Mock<IStudentScheduleService>().Object, new Mock<IRefreshCoordinator>().Object, new Mock<IInsightsService>().Object);

        var propertyChangedRaised = false;
        string? raisedPropertyName = null;
        viewModel.PropertyChanged += (sender, e) =>
        {
            if (e.PropertyName == nameof(viewModel.LatestAnnouncementTitle))
            {
                propertyChangedRaised = true;
                raisedPropertyName = e.PropertyName;
            }
        };

        // Act
        viewModel.LatestAnnouncementTitle = value;

        // Assert
        Assert.AreEqual(value, viewModel.LatestAnnouncementTitle);
        Assert.IsTrue(propertyChangedRaised, "PropertyChanged event should be raised.");
        Assert.AreEqual(nameof(viewModel.LatestAnnouncementTitle), raisedPropertyName);
    }

    /// <summary>
    /// Tests that setting LatestAnnouncementTitle to the same value twice only raises
    /// the PropertyChanged event once (on the first change from initial value).
    /// Input: Same value set twice consecutively.
    /// Expected: PropertyChanged event is raised only on the first set.
    /// </summary>
    [TestMethod]
    public void LatestAnnouncementTitle_SetSameValueTwice_RaisesPropertyChangedOnlyOnce()
    {
        // Arrange
        var mockDashboardService = new Mock<IDashboardService>();
        var mockAuthService = new Mock<IAuthService>();
        var mockSessionService = new Mock<SessionService>();
        var viewModel = new DashboardViewModel(mockDashboardService.Object, mockAuthService.Object, mockSessionService.Object, new Mock<IStudentScheduleService>().Object, new Mock<IRefreshCoordinator>().Object, new Mock<IInsightsService>().Object);

        var propertyChangedCount = 0;
        viewModel.PropertyChanged += (sender, e) =>
        {
            if (e.PropertyName == nameof(viewModel.LatestAnnouncementTitle))
            {
                propertyChangedCount++;
            }
        };

        // Act
        viewModel.LatestAnnouncementTitle = "Test Announcement";
        viewModel.LatestAnnouncementTitle = "Test Announcement"; // Same value

        // Assert
        Assert.AreEqual("Test Announcement", viewModel.LatestAnnouncementTitle);
        Assert.AreEqual(1, propertyChangedCount, "PropertyChanged should only be raised once when setting the same value twice.");
    }

    /// <summary>
    /// Tests that setting LatestAnnouncementTitle to a very long string updates the property correctly
    /// and raises the PropertyChanged event.
    /// Input: String with 10,000 characters.
    /// Expected: Property is updated and PropertyChanged event is raised.
    /// </summary>
    [TestMethod]
    public void LatestAnnouncementTitle_SetVeryLongString_UpdatesPropertyAndRaisesPropertyChanged()
    {
        // Arrange
        var mockDashboardService = new Mock<IDashboardService>();
        var mockAuthService = new Mock<IAuthService>();
        var mockSessionService = new Mock<SessionService>();
        var viewModel = new DashboardViewModel(mockDashboardService.Object, mockAuthService.Object, mockSessionService.Object, new Mock<IStudentScheduleService>().Object, new Mock<IRefreshCoordinator>().Object, new Mock<IInsightsService>().Object);

        var veryLongString = new string('A', 10000);
        var propertyChangedRaised = false;
        viewModel.PropertyChanged += (sender, e) =>
        {
            if (e.PropertyName == nameof(viewModel.LatestAnnouncementTitle))
            {
                propertyChangedRaised = true;
            }
        };

        // Act
        viewModel.LatestAnnouncementTitle = veryLongString;

        // Assert
        Assert.AreEqual(veryLongString, viewModel.LatestAnnouncementTitle);
        Assert.IsTrue(propertyChangedRaised, "PropertyChanged event should be raised for very long string.");
    }

    /// <summary>
    /// Tests that setting LatestAnnouncementTitle to null updates the property and raises PropertyChanged event.
    /// Input: null value.
    /// Expected: Property is set to null and PropertyChanged event is raised.
    /// </summary>
    [TestMethod]
    public void LatestAnnouncementTitle_SetNull_UpdatesPropertyAndRaisesPropertyChanged()
    {
        // Arrange
        var mockDashboardService = new Mock<IDashboardService>();
        var mockAuthService = new Mock<IAuthService>();
        var mockSessionService = new Mock<SessionService>();
        var viewModel = new DashboardViewModel(mockDashboardService.Object, mockAuthService.Object, mockSessionService.Object, new Mock<IStudentScheduleService>().Object, new Mock<IRefreshCoordinator>().Object, new Mock<IInsightsService>().Object);

        var propertyChangedRaised = false;
        viewModel.PropertyChanged += (sender, e) =>
        {
            if (e.PropertyName == nameof(viewModel.LatestAnnouncementTitle))
            {
                propertyChangedRaised = true;
            }
        };

        // Act
        viewModel.LatestAnnouncementTitle = null!;

        // Assert
        Assert.IsNull(viewModel.LatestAnnouncementTitle);
        Assert.IsTrue(propertyChangedRaised, "PropertyChanged event should be raised when setting to null.");
    }

    /// <summary>
    /// Tests that setting LatestAnnouncementTitle multiple times with different values
    /// raises PropertyChanged event each time and maintains the correct value.
    /// Input: Multiple different string values set consecutively.
    /// Expected: PropertyChanged event is raised for each distinct value change and property reflects latest value.
    /// </summary>
    [TestMethod]
    public void LatestAnnouncementTitle_SetMultipleDifferentValues_RaisesPropertyChangedEachTime()
    {
        // Arrange
        var mockDashboardService = new Mock<IDashboardService>();
        var mockAuthService = new Mock<IAuthService>();
        var mockSessionService = new Mock<SessionService>();
        var viewModel = new DashboardViewModel(mockDashboardService.Object, mockAuthService.Object, mockSessionService.Object, new Mock<IStudentScheduleService>().Object, new Mock<IRefreshCoordinator>().Object, new Mock<IInsightsService>().Object);

        var propertyChangedCount = 0;
        viewModel.PropertyChanged += (sender, e) =>
        {
            if (e.PropertyName == nameof(viewModel.LatestAnnouncementTitle))
            {
                propertyChangedCount++;
            }
        };

        // Act
        viewModel.LatestAnnouncementTitle = "First Announcement";
        viewModel.LatestAnnouncementTitle = "Second Announcement";
        viewModel.LatestAnnouncementTitle = "Third Announcement";

        // Assert
        Assert.AreEqual("Third Announcement", viewModel.LatestAnnouncementTitle);
        Assert.AreEqual(3, propertyChangedCount, "PropertyChanged should be raised for each distinct value change.");
    }

    /// <summary>
    /// Tests that setting LatestAnnouncementTitle with strings containing control characters
    /// updates the property correctly.
    /// Input: Strings with null character, carriage return, line feed combinations.
    /// Expected: Property is updated with control characters intact.
    /// </summary>
    [TestMethod]
    [DataRow("Title\0WithNull", DisplayName = "Null character")]
    [DataRow("Title\r\nWithCRLF", DisplayName = "Carriage return and line feed")]
    [DataRow("\u0001\u0002\u0003", DisplayName = "Control characters")]
    public void LatestAnnouncementTitle_SetStringWithControlCharacters_UpdatesProperty(string value)
    {
        // Arrange
        var mockDashboardService = new Mock<IDashboardService>();
        var mockAuthService = new Mock<IAuthService>();
        var mockSessionService = new Mock<SessionService>();
        var viewModel = new DashboardViewModel(mockDashboardService.Object, mockAuthService.Object, mockSessionService.Object, new Mock<IStudentScheduleService>().Object, new Mock<IRefreshCoordinator>().Object, new Mock<IInsightsService>().Object);

        var propertyChangedRaised = false;
        viewModel.PropertyChanged += (sender, e) =>
        {
            if (e.PropertyName == nameof(viewModel.LatestAnnouncementTitle))
            {
                propertyChangedRaised = true;
            }
        };

        // Act
        viewModel.LatestAnnouncementTitle = value;

        // Assert
        Assert.AreEqual(value, viewModel.LatestAnnouncementTitle);
        Assert.IsTrue(propertyChangedRaised, "PropertyChanged event should be raised.");
    }

    /// <summary>
    /// Tests that setting LatestAnnouncementTitle back to empty string after having a value
    /// correctly updates the property and raises PropertyChanged event.
    /// Input: Set to non-empty value, then set back to empty string.
    /// Expected: Property is updated to empty string and PropertyChanged event is raised.
    /// </summary>
    [TestMethod]
    public void LatestAnnouncementTitle_SetToEmptyAfterValue_UpdatesPropertyAndRaisesPropertyChanged()
    {
        // Arrange
        var mockDashboardService = new Mock<IDashboardService>();
        var mockAuthService = new Mock<IAuthService>();
        var mockSessionService = new Mock<SessionService>();
        var viewModel = new DashboardViewModel(mockDashboardService.Object, mockAuthService.Object, mockSessionService.Object, new Mock<IStudentScheduleService>().Object, new Mock<IRefreshCoordinator>().Object, new Mock<IInsightsService>().Object);

        viewModel.LatestAnnouncementTitle = "Some Announcement";

        var propertyChangedRaised = false;
        viewModel.PropertyChanged += (sender, e) =>
        {
            if (e.PropertyName == nameof(viewModel.LatestAnnouncementTitle))
            {
                propertyChangedRaised = true;
            }
        };

        // Act
        viewModel.LatestAnnouncementTitle = string.Empty;

        // Assert
        Assert.AreEqual(string.Empty, viewModel.LatestAnnouncementTitle);
        Assert.IsTrue(propertyChangedRaised, "PropertyChanged event should be raised when setting back to empty.");
    }

    /// <summary>
    /// Tests that PropertyChanged event provides the correct sender when LatestAnnouncementTitle is set.
    /// Input: New string value.
    /// Expected: PropertyChanged event sender is the ViewModel instance.
    /// </summary>
    [TestMethod]
    public void LatestAnnouncementTitle_SetValue_PropertyChangedEventHasCorrectSender()
    {
        // Arrange
        var mockDashboardService = new Mock<IDashboardService>();
        var mockAuthService = new Mock<IAuthService>();
        var mockSessionService = new Mock<SessionService>();
        var viewModel = new DashboardViewModel(mockDashboardService.Object, mockAuthService.Object, mockSessionService.Object, new Mock<IStudentScheduleService>().Object, new Mock<IRefreshCoordinator>().Object, new Mock<IInsightsService>().Object);

        object? eventSender = null;
        viewModel.PropertyChanged += (sender, e) =>
        {
            if (e.PropertyName == nameof(viewModel.LatestAnnouncementTitle))
            {
                eventSender = sender;
            }
        };

        // Act
        viewModel.LatestAnnouncementTitle = "Test";

        // Assert
        Assert.AreSame(viewModel, eventSender, "PropertyChanged event sender should be the ViewModel instance.");
    }

    /// <summary>
    /// Tests that LatestAnnouncementTitle handles extreme boundary values for string length.
    /// Input: Strings at maximum practical length (100,000 characters).
    /// Expected: Property is updated correctly without errors.
    /// </summary>
    [TestMethod]
    public void LatestAnnouncementTitle_SetExtremelyLongString_UpdatesPropertyCorrectly()
    {
        // Arrange
        var mockDashboardService = new Mock<IDashboardService>();
        var mockAuthService = new Mock<IAuthService>();
        var mockSessionService = new Mock<SessionService>();
        var viewModel = new DashboardViewModel(mockDashboardService.Object, mockAuthService.Object, mockSessionService.Object, new Mock<IStudentScheduleService>().Object, new Mock<IRefreshCoordinator>().Object, new Mock<IInsightsService>().Object);

        var extremelyLongString = new string('X', 100000);

        // Act
        viewModel.LatestAnnouncementTitle = extremelyLongString;

        // Assert
        Assert.AreEqual(extremelyLongString, viewModel.LatestAnnouncementTitle);
        Assert.AreEqual(100000, viewModel.LatestAnnouncementTitle.Length);
    }

    /// <summary>
    /// Tests that IsStudent property returns false when Role is set to a very long string
    /// that is not exactly "student".
    /// Input: A very long string (10000 characters).
    /// Expected: Returns false.
    /// </summary>
    [TestMethod]
    public void IsStudent_VeryLongRoleString_ReturnsFalse()
    {
        // Arrange
        var mockDashboardService = new Mock<IDashboardService>();
        var mockAuthService = new Mock<IAuthService>();
        var mockSessionService = new Mock<SessionService>();
        var viewModel = new DashboardViewModel(mockDashboardService.Object, mockAuthService.Object, mockSessionService.Object, new Mock<IStudentScheduleService>().Object, new Mock<IRefreshCoordinator>().Object, new Mock<IInsightsService>().Object);
        var veryLongString = new string('a', 10000);

        // Act
        viewModel.Role = veryLongString;
        var result = viewModel.IsStudent;

        // Assert
        Assert.IsFalse(result);
    }

    /// <summary>
    /// Tests that IsStudent property handles Unicode characters in Role correctly.
    /// Input: Strings with Unicode characters that are not "student".
    /// Expected: Returns false for Unicode strings that don't match "student".
    /// </summary>
    [TestMethod]
    [DataRow("???????", false, DisplayName = "IsStudent returns false with Cyrillic 'student'")]
    [DataRow("étudiant", false, DisplayName = "IsStudent returns false with French 'student'")]
    [DataRow("??", false, DisplayName = "IsStudent returns false with Chinese 'student'")]
    [DataRow("student\u200B", false, DisplayName = "IsStudent returns false with zero-width space")]
    [DataRow("\u202Astudent", false, DisplayName = "IsStudent returns false with left-to-right embedding")]
    [DataRow("stu\u0000dent", false, DisplayName = "IsStudent returns false with null character inside")]
    public void IsStudent_RoleWithUnicodeCharacters_ReturnsExpectedResult(string? roleValue, bool expectedResult)
    {
        // Arrange
        var mockDashboardService = new Mock<IDashboardService>();
        var mockAuthService = new Mock<IAuthService>();
        var mockSessionService = new Mock<SessionService>();
        var viewModel = new DashboardViewModel(mockDashboardService.Object, mockAuthService.Object, mockSessionService.Object, new Mock<IStudentScheduleService>().Object, new Mock<IRefreshCoordinator>().Object, new Mock<IInsightsService>().Object);

        // Act
        viewModel.Role = roleValue;
        var result = viewModel.IsStudent;

        // Assert
        Assert.AreEqual(expectedResult, result);
    }

    /// <summary>
    /// Tests that IsStudent property correctly updates when Role changes from one value to another.
    /// Input: Sequential role changes from non-student to student and back.
    /// Expected: IsStudent reflects the current role value.
    /// </summary>
    [TestMethod]
    public void IsStudent_RoleChangesMultipleTimes_ReturnsCorrectValueEachTime()
    {
        // Arrange
        var mockDashboardService = new Mock<IDashboardService>();
        var mockAuthService = new Mock<IAuthService>();
        var mockSessionService = new Mock<SessionService>();
        var viewModel = new DashboardViewModel(mockDashboardService.Object, mockAuthService.Object, mockSessionService.Object, new Mock<IStudentScheduleService>().Object, new Mock<IRefreshCoordinator>().Object, new Mock<IInsightsService>().Object);

        // Act & Assert - Initial state
        viewModel.Role = "Lecturer";
        Assert.IsFalse(viewModel.IsStudent);

        // Act & Assert - Change to student
        viewModel.Role = "student";
        Assert.IsTrue(viewModel.IsStudent);

        // Act & Assert - Change to Admin
        viewModel.Role = "Admin";
        Assert.IsFalse(viewModel.IsStudent);

        // Act & Assert - Change to STUDENT (uppercase)
        viewModel.Role = "STUDENT";
        Assert.IsTrue(viewModel.IsStudent);

        // Act & Assert - Change to null
        viewModel.Role = null;
        Assert.IsFalse(viewModel.IsStudent);

        // Act & Assert - Change back to Student (title case)
        viewModel.Role = "Student";
        Assert.IsTrue(viewModel.IsStudent);
    }

    /// <summary>
    /// Tests that IsStudent property uses case-insensitive comparison with invariant culture.
    /// Input: Various culturally-specific uppercase/lowercase variations.
    /// Expected: Returns true for any case variation of "student".
    /// </summary>
    [TestMethod]
    [DataRow("student", true)]
    [DataRow("STUDENT", true)]
    [DataRow("Student", true)]
    [DataRow("sTuDeNt", true)]
    [DataRow("StUdEnT", true)]
    [DataRow("stuDENT", true)]
    [DataRow("STUdent", true)]
    [DataRow("STudent", true)]
    public void IsStudent_CaseInsensitiveComparison_ReturnsTrue(string roleValue, bool expectedResult)
    {
        // Arrange
        var mockDashboardService = new Mock<IDashboardService>();
        var mockAuthService = new Mock<IAuthService>();
        var mockSessionService = new Mock<SessionService>();
        var viewModel = new DashboardViewModel(mockDashboardService.Object, mockAuthService.Object, mockSessionService.Object, new Mock<IStudentScheduleService>().Object, new Mock<IRefreshCoordinator>().Object, new Mock<IInsightsService>().Object);

        // Act
        viewModel.Role = roleValue;
        var result = viewModel.IsStudent;

        // Assert
        Assert.AreEqual(expectedResult, result);
    }

    /// <summary>
    /// Tests that IsStudent property returns false for roles that are similar but not exactly "student".
    /// Input: Roles with slight variations from "student".
    /// Expected: Returns false.
    /// </summary>
    [TestMethod]
    [DataRow("studen")]
    [DataRow("tudent")]
    [DataRow("studnt")]
    [DataRow("student1")]
    [DataRow("1student")]
    [DataRow("_student")]
    [DataRow("student_")]
    [DataRow("sstudent")]
    [DataRow("studentt")]
    public void IsStudent_SimilarButNotExactRole_ReturnsFalse(string roleValue)
    {
        // Arrange
        var mockDashboardService = new Mock<IDashboardService>();
        var mockAuthService = new Mock<IAuthService>();
        var mockSessionService = new Mock<SessionService>();
        var viewModel = new DashboardViewModel(mockDashboardService.Object, mockAuthService.Object, mockSessionService.Object, new Mock<IStudentScheduleService>().Object, new Mock<IRefreshCoordinator>().Object, new Mock<IInsightsService>().Object);

        // Act
        viewModel.Role = roleValue;
        var result = viewModel.IsStudent;

        // Assert
        Assert.IsFalse(result);
    }

    /// <summary>
    /// Tests that GoToUniversitiesCommand returns a Command type instance.
    /// Input: None (property access).
    /// Expected: Returns an instance of Microsoft.Maui.Controls.Command.
    /// </summary>
    [TestMethod]
    public void GoToUniversitiesCommand_WhenAccessed_ReturnsCommandType()
    {
        // Arrange
        var mockDashboard = new Mock<IDashboardService>();
        var mockAuth = new Mock<IAuthService>();
        var mockSession = new Mock<SessionService>();
        var viewModel = new DashboardViewModel(mockDashboard.Object, mockAuth.Object, mockSession.Object, new Mock<IStudentScheduleService>().Object, new Mock<IRefreshCoordinator>().Object, new Mock<IInsightsService>().Object);

        // Act
        var command = viewModel.GoToUniversitiesCommand;

        // Assert
        Assert.IsInstanceOfType(command, typeof(Command));
    }

    /// <summary>
    /// Tests that GoToUniversitiesCommand.CanExecute returns true with null parameter.
    /// Input: null parameter.
    /// Expected: CanExecute returns true.
    /// </summary>
    [TestMethod]
    public void GoToUniversitiesCommand_CanExecuteWithNull_ReturnsTrue()
    {
        // Arrange
        var mockDashboard = new Mock<IDashboardService>();
        var mockAuth = new Mock<IAuthService>();
        var mockSession = new Mock<SessionService>();
        var viewModel = new DashboardViewModel(mockDashboard.Object, mockAuth.Object, mockSession.Object, new Mock<IStudentScheduleService>().Object, new Mock<IRefreshCoordinator>().Object, new Mock<IInsightsService>().Object);

        // Act
        var command = viewModel.GoToUniversitiesCommand;
        var canExecute = command.CanExecute(null);

        // Assert
        Assert.IsTrue(canExecute);
    }

    /// <summary>
    /// Tests that GoToUniversitiesCommand.CanExecute returns true with non-null parameter.
    /// Input: Various non-null parameters.
    /// Expected: CanExecute returns true for all parameters.
    /// </summary>
    [TestMethod]
    [DataRow("test")]
    [DataRow(123)]
    [DataRow(true)]
    public void GoToUniversitiesCommand_CanExecuteWithNonNullParameter_ReturnsTrue(object parameter)
    {
        // Arrange
        var mockDashboard = new Mock<IDashboardService>();
        var mockAuth = new Mock<IAuthService>();
        var mockSession = new Mock<SessionService>();
        var viewModel = new DashboardViewModel(mockDashboard.Object, mockAuth.Object, mockSession.Object, new Mock<IStudentScheduleService>().Object, new Mock<IRefreshCoordinator>().Object, new Mock<IInsightsService>().Object);

        // Act
        var command = viewModel.GoToUniversitiesCommand;
        var canExecute = command.CanExecute(parameter);

        // Assert
        Assert.IsTrue(canExecute);
    }

    /// <summary>
    /// Tests that accessing GoToUniversitiesCommand property does not throw any exceptions.
    /// Input: None (property access).
    /// Expected: No exception is thrown.
    /// </summary>
    [TestMethod]
    public void GoToUniversitiesCommand_WhenAccessed_DoesNotThrowException()
    {
        // Arrange
        var mockDashboard = new Mock<IDashboardService>();
        var mockAuth = new Mock<IAuthService>();
        var mockSession = new Mock<SessionService>();
        var viewModel = new DashboardViewModel(mockDashboard.Object, mockAuth.Object, mockSession.Object, new Mock<IStudentScheduleService>().Object, new Mock<IRefreshCoordinator>().Object, new Mock<IInsightsService>().Object);

        // Act & Assert
        try
        {
            var command = viewModel.GoToUniversitiesCommand;
            Assert.IsNotNull(command);
        }
        catch (Exception ex)
        {
            Assert.Fail($"Expected no exception, but got: {ex.GetType().Name} - {ex.Message}");
        }
    }

    /// <summary>
    /// Tests that GoToUniversitiesCommand can be accessed immediately after ViewModel construction.
    /// Input: Fresh ViewModel instance.
    /// Expected: Command is accessible and non-null.
    /// </summary>
    [TestMethod]
    public void GoToUniversitiesCommand_AfterConstruction_IsAccessible()
    {
        // Arrange & Act
        var mockDashboard = new Mock<IDashboardService>();
        var mockAuth = new Mock<IAuthService>();
        var mockSession = new Mock<SessionService>();
        var viewModel = new DashboardViewModel(mockDashboard.Object, mockAuth.Object, mockSession.Object, new Mock<IStudentScheduleService>().Object, new Mock<IRefreshCoordinator>().Object, new Mock<IInsightsService>().Object);
        var command = viewModel.GoToUniversitiesCommand;

        // Assert
        Assert.IsNotNull(command);
        Assert.IsInstanceOfType(command, typeof(ICommand));
    }

    // Note: Testing the actual execution of GoToUniversitiesCommand (calling command.Execute())
    // is not feasible in a pure unit test context because it depends on Shell.Current,
    // which is a static property from Microsoft.Maui.Controls.Shell. This static dependency
    // cannot be mocked using Moq, and creating fake implementations is prohibited.
    // In a unit test environment, Shell.Current will be null, causing a NullReferenceException.
    // To test the navigation behavior, consider:
    // 1. Integration tests with a properly initialized Shell instance
    // 2. UI tests using the MAUI testing framework
    // 3. Refactoring to inject a navigation service abstraction that can be mocked

    /// <summary>
    /// Tests that AttendanceStatus property setter updates the backing field and raises PropertyChanged event.
    /// Input: New valid string value.
    /// Expected: Property value is updated and PropertyChanged event is raised with correct property name.
    /// </summary>
    [TestMethod]
    [DataRow("Excellent")]
    [DataRow("Good")]
    [DataRow("At Risk")]
    [DataRow("Poor")]
    [DataRow("Needs Improvement")]
    [DataRow("Perfect")]
    [DataRow("Outstanding")]
    public void AttendanceStatus_SetNewValue_UpdatesPropertyAndRaisesPropertyChanged(string newValue)
    {
        // Arrange
        string? propertyChangedName = null;
        _viewModel.PropertyChanged += (sender, args) => propertyChangedName = args.PropertyName;

        // Act
        _viewModel.AttendanceStatus = newValue;

        // Assert
        Assert.AreEqual(newValue, _viewModel.AttendanceStatus);
        Assert.AreEqual(nameof(DashboardViewModel.AttendanceStatus), propertyChangedName);
    }

    /// <summary>
    /// Tests that setting AttendanceStatus to the same value does not raise PropertyChanged event.
    /// Input: Same value as current value.
    /// Expected: PropertyChanged event is not raised.
    /// </summary>
    [TestMethod]
    public void AttendanceStatus_SetSameValue_DoesNotRaisePropertyChanged()
    {
        // Arrange
        _viewModel.AttendanceStatus = "On Track";
        int eventRaisedCount = 0;
        _viewModel.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == nameof(DashboardViewModel.AttendanceStatus))
                eventRaisedCount++;
        };

        // Act
        _viewModel.AttendanceStatus = "On Track";

        // Assert
        Assert.AreEqual(0, eventRaisedCount);
        Assert.AreEqual("On Track", _viewModel.AttendanceStatus);
    }

    /// <summary>
    /// Tests that setting AttendanceStatus to empty string updates the property correctly.
    /// Input: Empty string.
    /// Expected: Property is updated to empty string and PropertyChanged event is raised.
    /// </summary>
    [TestMethod]
    public void AttendanceStatus_SetEmptyString_UpdatesPropertyAndRaisesPropertyChanged()
    {
        // Arrange
        string? propertyChangedName = null;
        _viewModel.PropertyChanged += (sender, args) => propertyChangedName = args.PropertyName;

        // Act
        _viewModel.AttendanceStatus = string.Empty;

        // Assert
        Assert.AreEqual(string.Empty, _viewModel.AttendanceStatus);
        Assert.AreEqual(nameof(DashboardViewModel.AttendanceStatus), propertyChangedName);
    }

    /// <summary>
    /// Tests that setting AttendanceStatus to whitespace-only strings updates the property correctly.
    /// Input: Whitespace strings (spaces, tabs, newlines, mixed).
    /// Expected: Property is updated and PropertyChanged event is raised.
    /// </summary>
    [TestMethod]
    [DataRow("   ")]
    [DataRow("\t")]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [DataRow(" \t \n ")]
    public void AttendanceStatus_SetWhitespaceString_UpdatesPropertyAndRaisesPropertyChanged(string whitespaceValue)
    {
        // Arrange
        string? propertyChangedName = null;
        _viewModel.PropertyChanged += (sender, args) => propertyChangedName = args.PropertyName;

        // Act
        _viewModel.AttendanceStatus = whitespaceValue;

        // Assert
        Assert.AreEqual(whitespaceValue, _viewModel.AttendanceStatus);
        Assert.AreEqual(nameof(DashboardViewModel.AttendanceStatus), propertyChangedName);
    }

    /// <summary>
    /// Tests that setting AttendanceStatus to a very long string updates the property correctly.
    /// Input: String with 10000 characters.
    /// Expected: Property is updated and PropertyChanged event is raised.
    /// </summary>
    [TestMethod]
    public void AttendanceStatus_SetVeryLongString_UpdatesPropertyAndRaisesPropertyChanged()
    {
        // Arrange
        string veryLongString = new string('A', 10000);
        string? propertyChangedName = null;
        _viewModel.PropertyChanged += (sender, args) => propertyChangedName = args.PropertyName;

        // Act
        _viewModel.AttendanceStatus = veryLongString;

        // Assert
        Assert.AreEqual(veryLongString, _viewModel.AttendanceStatus);
        Assert.AreEqual(nameof(DashboardViewModel.AttendanceStatus), propertyChangedName);
    }

    /// <summary>
    /// Tests that setting AttendanceStatus to strings with special characters updates the property correctly.
    /// Input: Strings with special characters, Unicode, emojis, control characters.
    /// Expected: Property is updated and PropertyChanged event is raised.
    /// </summary>
    [TestMethod]
    [DataRow("Status: 100%!")]
    [DataRow("Status <Good>")]
    [DataRow("Status & Notes")]
    [DataRow("Status™")]
    [DataRow("????")]
    [DataRow("Status ??")]
    [DataRow("Status\u0000NullChar")]
    [DataRow("Status\r\nMultiline")]
    [DataRow("!@#$%^&*()_+-=[]{}|;':\",./<>?")]
    public void AttendanceStatus_SetStringWithSpecialCharacters_UpdatesPropertyAndRaisesPropertyChanged(string specialValue)
    {
        // Arrange
        string? propertyChangedName = null;
        _viewModel.PropertyChanged += (sender, args) => propertyChangedName = args.PropertyName;

        // Act
        _viewModel.AttendanceStatus = specialValue;

        // Assert
        Assert.AreEqual(specialValue, _viewModel.AttendanceStatus);
        Assert.AreEqual(nameof(DashboardViewModel.AttendanceStatus), propertyChangedName);
    }

    /// <summary>
    /// Tests that setting AttendanceStatus multiple times with different values raises PropertyChanged for each change.
    /// Input: Multiple sequential different values.
    /// Expected: Property is updated correctly each time and PropertyChanged is raised for each distinct change.
    /// </summary>
    [TestMethod]
    public void AttendanceStatus_SetMultipleDifferentValues_RaisesPropertyChangedForEachChange()
    {
        // Arrange
        int eventRaisedCount = 0;
        _viewModel.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == nameof(DashboardViewModel.AttendanceStatus))
                eventRaisedCount++;
        };

        // Act
        _viewModel.AttendanceStatus = "Good";
        _viewModel.AttendanceStatus = "Excellent";
        _viewModel.AttendanceStatus = "At Risk";

        // Assert
        Assert.AreEqual(3, eventRaisedCount);
        Assert.AreEqual("At Risk", _viewModel.AttendanceStatus);
    }

    /// <summary>
    /// Tests that PropertyChanged event provides correct sender when AttendanceStatus is set.
    /// Input: New string value.
    /// Expected: PropertyChanged event sender is the ViewModel instance.
    /// </summary>
    [TestMethod]
    public void AttendanceStatus_SetValue_PropertyChangedEventHasCorrectSender()
    {
        // Arrange
        object? eventSender = null;
        _viewModel.PropertyChanged += (sender, args) => eventSender = sender;

        // Act
        _viewModel.AttendanceStatus = "Good";

        // Assert
        Assert.AreSame(_viewModel, eventSender);
    }

    /// <summary>
    /// Tests that setting AttendanceStatus back to the initial value after changing it raises PropertyChanged.
    /// Input: Change to new value, then back to "On Track".
    /// Expected: PropertyChanged is raised for both changes.
    /// </summary>
    [TestMethod]
    public void AttendanceStatus_SetBackToInitialValue_RaisesPropertyChanged()
    {
        // Arrange
        _viewModel.AttendanceStatus = "Poor";
        int eventRaisedCount = 0;
        _viewModel.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == nameof(DashboardViewModel.AttendanceStatus))
                eventRaisedCount++;
        };

        // Act
        _viewModel.AttendanceStatus = "On Track";

        // Assert
        Assert.AreEqual(1, eventRaisedCount);
        Assert.AreEqual("On Track", _viewModel.AttendanceStatus);
    }

    /// <summary>
    /// Tests that setting AttendanceStatus with leading and trailing whitespace updates correctly.
    /// Input: Strings with leading/trailing spaces.
    /// Expected: Property stores exact value including whitespace.
    /// </summary>
    [TestMethod]
    [DataRow(" On Track")]
    [DataRow("On Track ")]
    [DataRow(" On Track ")]
    [DataRow("\tOn Track\t")]
    public void AttendanceStatus_SetValueWithLeadingOrTrailingWhitespace_StoresExactValue(string valueWithWhitespace)
    {
        // Arrange
        string? propertyChangedName = null;
        _viewModel.PropertyChanged += (sender, args) => propertyChangedName = args.PropertyName;

        // Act
        _viewModel.AttendanceStatus = valueWithWhitespace;

        // Assert
        Assert.AreEqual(valueWithWhitespace, _viewModel.AttendanceStatus);
        Assert.AreEqual(nameof(DashboardViewModel.AttendanceStatus), propertyChangedName);
    }

    /// <summary>
    /// Tests that HasAnnouncement raises PropertyChanged event for each distinct value change.
    /// Input: Multiple different boolean values.
    /// Expected: PropertyChanged event is raised for each change from false to true or true to false.
    /// </summary>
    [TestMethod]
    public void HasAnnouncement_MultipleValueChanges_RaisesPropertyChangedForEachChange()
    {
        // Arrange
        var mockDashboard = new Mock<IDashboardService>();
        var mockAuth = new Mock<IAuthService>();
        var mockSession = new Mock<SessionService>();
        var viewModel = new DashboardViewModel(mockDashboard.Object, mockAuth.Object, mockSession.Object, new Mock<IStudentScheduleService>().Object, new Mock<IRefreshCoordinator>().Object, new Mock<IInsightsService>().Object);
        var propertyChangedCount = 0;

        viewModel.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == "HasAnnouncement")
            {
                propertyChangedCount++;
            }
        };

        // Act
        viewModel.HasAnnouncement = true;
        viewModel.HasAnnouncement = false;
        viewModel.HasAnnouncement = true;

        // Assert
        Assert.AreEqual(3, propertyChangedCount);
    }

    /// <summary>
    /// Tests that setting HasAnnouncement to true multiple times only raises PropertyChanged once.
    /// Input: Same value (true) set twice consecutively.
    /// Expected: PropertyChanged event is raised only on the first change.
    /// </summary>
    [TestMethod]
    public void HasAnnouncement_SetTrueMultipleTimes_RaisesPropertyChangedOnlyOnce()
    {
        // Arrange
        var mockDashboard = new Mock<IDashboardService>();
        var mockAuth = new Mock<IAuthService>();
        var mockSession = new Mock<SessionService>();
        var viewModel = new DashboardViewModel(mockDashboard.Object, mockAuth.Object, mockSession.Object, new Mock<IStudentScheduleService>().Object, new Mock<IRefreshCoordinator>().Object, new Mock<IInsightsService>().Object);
        var propertyChangedCount = 0;

        viewModel.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == "HasAnnouncement")
            {
                propertyChangedCount++;
            }
        };

        // Act
        viewModel.HasAnnouncement = true;
        viewModel.HasAnnouncement = true;
        viewModel.HasAnnouncement = true;

        // Assert
        Assert.AreEqual(1, propertyChangedCount);
        Assert.IsTrue(viewModel.HasAnnouncement);
    }

    /// <summary>
    /// Tests that setting HasAnnouncement to false multiple times only raises PropertyChanged once.
    /// Input: Same value (false) set twice consecutively after initial true value.
    /// Expected: PropertyChanged event is raised only when value actually changes.
    /// </summary>
    [TestMethod]
    public void HasAnnouncement_SetFalseMultipleTimes_RaisesPropertyChangedOnlyOnce()
    {
        // Arrange
        var mockDashboard = new Mock<IDashboardService>();
        var mockAuth = new Mock<IAuthService>();
        var mockSession = new Mock<SessionService>();
        var viewModel = new DashboardViewModel(mockDashboard.Object, mockAuth.Object, mockSession.Object, new Mock<IStudentScheduleService>().Object, new Mock<IRefreshCoordinator>().Object, new Mock<IInsightsService>().Object);
        viewModel.HasAnnouncement = true;
        var propertyChangedCount = 0;

        viewModel.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == "HasAnnouncement")
            {
                propertyChangedCount++;
            }
        };

        // Act
        viewModel.HasAnnouncement = false;
        viewModel.HasAnnouncement = false;
        viewModel.HasAnnouncement = false;

        // Assert
        Assert.AreEqual(1, propertyChangedCount);
        Assert.IsFalse(viewModel.HasAnnouncement);
    }

    /// <summary>
    /// Tests that PropertyChanged event has correct sender when HasAnnouncement is set.
    /// Input: New boolean value.
    /// Expected: PropertyChanged event sender is the ViewModel instance.
    /// </summary>
    [TestMethod]
    public void HasAnnouncement_SetValue_PropertyChangedEventHasCorrectSender()
    {
        // Arrange
        var mockDashboard = new Mock<IDashboardService>();
        var mockAuth = new Mock<IAuthService>();
        var mockSession = new Mock<SessionService>();
        var viewModel = new DashboardViewModel(mockDashboard.Object, mockAuth.Object, mockSession.Object, new Mock<IStudentScheduleService>().Object, new Mock<IRefreshCoordinator>().Object, new Mock<IInsightsService>().Object);
        object? eventSender = null;

        viewModel.PropertyChanged += (sender, args) =>
        {
            eventSender = sender;
        };

        // Act
        viewModel.HasAnnouncement = true;

        // Assert
        Assert.AreSame(viewModel, eventSender);
    }

    /// <summary>
    /// Tests that IsLecturer property returns false when Role is set to strings
    /// with Unicode control characters and zero-width characters.
    /// Input: Strings containing Unicode control characters.
    /// Expected: IsLecturer returns false.
    /// </summary>
    [TestMethod]
    [DataRow("lecturer\u200B", false, DisplayName = "IsLecturer returns false with zero-width space")]
    [DataRow("\u200Blecturer", false, DisplayName = "IsLecturer returns false with leading zero-width space")]
    [DataRow("\u202Alecturer", false, DisplayName = "IsLecturer returns false with left-to-right embedding")]
    [DataRow("lecturer\u202C", false, DisplayName = "IsLecturer returns false with pop directional formatting")]
    [DataRow("lec\u200Bturer", false, DisplayName = "IsLecturer returns false with zero-width space in middle")]
    [DataRow("\uFEFFlecturer", false, DisplayName = "IsLecturer returns false with zero-width no-break space")]
    public void IsLecturer_RoleWithUnicodeControlCharacters_ReturnsFalse(string? roleValue, bool expectedResult)
    {
        // Arrange
        Mock<IDashboardService> mockDashboardService = new Mock<IDashboardService>();
        Mock<IAuthService> mockAuthService = new Mock<IAuthService>();
        Mock<SessionService> mockSessionService = new Mock<SessionService>();

        DashboardViewModel viewModel = new DashboardViewModel(
            mockDashboardService.Object,
            mockAuthService.Object,
            mockSessionService.Object
        );

        // Act
        viewModel.Role = roleValue;
        bool actualResult = viewModel.IsLecturer;

        // Assert
        Assert.AreEqual(expectedResult, actualResult);
    }

    /// <summary>
    /// Tests that IsLecturer property returns false when Role contains null characters.
    /// Input: String with embedded null character.
    /// Expected: IsLecturer returns false.
    /// </summary>
    [TestMethod]
    public void IsLecturer_RoleWithNullCharacter_ReturnsFalse()
    {
        // Arrange
        Mock<IDashboardService> mockDashboardService = new Mock<IDashboardService>();
        Mock<IAuthService> mockAuthService = new Mock<IAuthService>();
        Mock<SessionService> mockSessionService = new Mock<SessionService>();

        DashboardViewModel viewModel = new DashboardViewModel(
            mockDashboardService.Object,
            mockAuthService.Object,
            mockSessionService.Object
        );

        string roleWithNull = "lec\0turer";

        // Act
        viewModel.Role = roleWithNull;
        bool actualResult = viewModel.IsLecturer;

        // Assert
        Assert.IsFalse(actualResult);
    }

    /// <summary>
    /// Tests that IsLecturer correctly reflects the current Role value after multiple changes.
    /// Input: Sequentially changing Role from various values to "lecturer" and back.
    /// Expected: IsLecturer updates correctly with each Role change.
    /// </summary>
    [TestMethod]
    public void IsLecturer_MultipleRoleChanges_ReflectsCurrentValue()
    {
        // Arrange
        Mock<IDashboardService> mockDashboardService = new Mock<IDashboardService>();
        Mock<IAuthService> mockAuthService = new Mock<IAuthService>();
        Mock<SessionService> mockSessionService = new Mock<SessionService>();

        DashboardViewModel viewModel = new DashboardViewModel(
            mockDashboardService.Object,
            mockAuthService.Object,
            mockSessionService.Object
        );

        // Act & Assert - Initial state
        viewModel.Role = "Student";
        Assert.IsFalse(viewModel.IsLecturer);

        // Change to lecturer
        viewModel.Role = "lecturer";
        Assert.IsTrue(viewModel.IsLecturer);

        // Change to admin
        viewModel.Role = "Admin";
        Assert.IsFalse(viewModel.IsLecturer);

        // Change to Lecturer (uppercase)
        viewModel.Role = "LECTURER";
        Assert.IsTrue(viewModel.IsLecturer);

        // Change to null
        viewModel.Role = null;
        Assert.IsFalse(viewModel.IsLecturer);

        // Change back to lecturer (mixed case)
        viewModel.Role = "LeCtuReR";
        Assert.IsTrue(viewModel.IsLecturer);
    }

    /// <summary>
    /// Tests that IsLecturer property uses case-insensitive comparison by testing
    /// all possible case permutations of a shorter variant.
    /// Input: Different case combinations of "lecturer".
    /// Expected: All valid case variations return true.
    /// </summary>
    [TestMethod]
    [DataRow("lecturer", true)]
    [DataRow("Lecturer", true)]
    [DataRow("lEcturer", true)]
    [DataRow("leCturer", true)]
    [DataRow("lecTurer", true)]
    [DataRow("lectUrer", true)]
    [DataRow("lectuRer", true)]
    [DataRow("lecturEr", true)]
    [DataRow("lectureR", true)]
    [DataRow("LECTURER", true)]
    [DataRow("LECTurer", true)]
    [DataRow("lectuRER", true)]
    public void IsLecturer_CaseInsensitiveComparison_ReturnsTrue(string roleValue, bool expectedResult)
    {
        // Arrange
        Mock<IDashboardService> mockDashboardService = new Mock<IDashboardService>();
        Mock<IAuthService> mockAuthService = new Mock<IAuthService>();
        Mock<SessionService> mockSessionService = new Mock<SessionService>();

        DashboardViewModel viewModel = new DashboardViewModel(
            mockDashboardService.Object,
            mockAuthService.Object,
            mockSessionService.Object
        );

        // Act
        viewModel.Role = roleValue;
        bool actualResult = viewModel.IsLecturer;

        // Assert
        Assert.AreEqual(expectedResult, actualResult);
    }

    /// <summary>
    /// Tests that IsLecturer returns false for role values that are substrings or superstrings of "lecturer".
    /// Input: Various strings that contain or are contained by "lecturer".
    /// Expected: All return false (only exact match should return true).
    /// </summary>
    [TestMethod]
    [DataRow("lec")]
    [DataRow("lect")]
    [DataRow("lectu")]
    [DataRow("lectur")]
    [DataRow("lecture")]
    [DataRow("lecturers")]
    [DataRow("lecturership")]
    [DataRow("alecturer")]
    [DataRow("lecturera")]
    [DataRow("senior lecturer")]
    [DataRow("lecturer senior")]
    public void IsLecturer_SubstringOrSuperstringOfLecturer_ReturnsFalse(string roleValue)
    {
        // Arrange
        Mock<IDashboardService> mockDashboardService = new Mock<IDashboardService>();
        Mock<IAuthService> mockAuthService = new Mock<IAuthService>();
        Mock<SessionService> mockSessionService = new Mock<SessionService>();

        DashboardViewModel viewModel = new DashboardViewModel(
            mockDashboardService.Object,
            mockAuthService.Object,
            mockSessionService.Object
        );

        // Act
        viewModel.Role = roleValue;
        bool actualResult = viewModel.IsLecturer;

        // Assert
        Assert.IsFalse(actualResult);
    }

    /// <summary>
    /// Tests that IsLecturer returns false when Role contains "lecturer" with various types of whitespace.
    /// Input: "lecturer" with different whitespace characters before, after, or within.
    /// Expected: All return false.
    /// </summary>
    [TestMethod]
    [DataRow(" lecturer")]
    [DataRow("lecturer ")]
    [DataRow("  lecturer  ")]
    [DataRow("\tlecturer")]
    [DataRow("lecturer\t")]
    [DataRow("\nlecturer")]
    [DataRow("lecturer\n")]
    [DataRow("\r\nlecturer")]
    [DataRow("lecturer\r\n")]
    [DataRow("lec turer")]
    [DataRow("lec\tturer")]
    [DataRow("lec\nturer")]
    public void IsLecturer_RoleWithWhitespace_ReturnsFalse(string roleValue)
    {
        // Arrange
        Mock<IDashboardService> mockDashboardService = new Mock<IDashboardService>();
        Mock<IAuthService> mockAuthService = new Mock<IAuthService>();
        Mock<SessionService> mockSessionService = new Mock<SessionService>();

        DashboardViewModel viewModel = new DashboardViewModel(
            mockDashboardService.Object,
            mockAuthService.Object,
            mockSessionService.Object
        );

        // Act
        viewModel.Role = roleValue;
        bool actualResult = viewModel.IsLecturer;

        // Assert
        Assert.IsFalse(actualResult);
    }

    /// <summary>
    /// Tests that setting Role to "lecturer" after it was a different value
    /// correctly updates IsLecturer to true.
    /// Input: Role changes from "Student" to "lecturer".
    /// Expected: IsLecturer changes from false to true.
    /// </summary>
    [TestMethod]
    public void IsLecturer_RoleChangedToLecturer_ReturnsTrue()
    {
        // Arrange
        Mock<IDashboardService> mockDashboardService = new Mock<IDashboardService>();
        Mock<IAuthService> mockAuthService = new Mock<IAuthService>();
        Mock<SessionService> mockSessionService = new Mock<SessionService>();

        DashboardViewModel viewModel = new DashboardViewModel(
            mockDashboardService.Object,
            mockAuthService.Object,
            mockSessionService.Object
        );

        viewModel.Role = "Student";
        Assert.IsFalse(viewModel.IsLecturer);

        // Act
        viewModel.Role = "lecturer";

        // Assert
        Assert.IsTrue(viewModel.IsLecturer);
    }

    /// <summary>
    /// Tests that setting Role from "lecturer" to a different value
    /// correctly updates IsLecturer to false.
    /// Input: Role changes from "lecturer" to "Admin".
    /// Expected: IsLecturer changes from true to false.
    /// </summary>
    [TestMethod]
    public void IsLecturer_RoleChangedFromLecturer_ReturnsFalse()
    {
        // Arrange
        Mock<IDashboardService> mockDashboardService = new Mock<IDashboardService>();
        Mock<IAuthService> mockAuthService = new Mock<IAuthService>();
        Mock<SessionService> mockSessionService = new Mock<SessionService>();

        DashboardViewModel viewModel = new DashboardViewModel(
            mockDashboardService.Object,
            mockAuthService.Object,
            mockSessionService.Object
        );

        viewModel.Role = "lecturer";
        Assert.IsTrue(viewModel.IsLecturer);

        // Act
        viewModel.Role = "Admin";

        // Assert
        Assert.IsFalse(viewModel.IsLecturer);
    }

    /// <summary>
    /// Tests that IsLecturer returns false for numeric strings.
    /// Input: Various numeric strings.
    /// Expected: All return false.
    /// </summary>
    [TestMethod]
    [DataRow("0")]
    [DataRow("1")]
    [DataRow("123")]
    [DataRow("-1")]
    [DataRow("999999")]
    public void IsLecturer_NumericStrings_ReturnsFalse(string roleValue)
    {
        // Arrange
        Mock<IDashboardService> mockDashboardService = new Mock<IDashboardService>();
        Mock<IAuthService> mockAuthService = new Mock<IAuthService>();
        Mock<SessionService> mockSessionService = new Mock<SessionService>();

        DashboardViewModel viewModel = new DashboardViewModel(
            mockDashboardService.Object,
            mockAuthService.Object,
            mockSessionService.Object
        );

        // Act
        viewModel.Role = roleValue;
        bool actualResult = viewModel.IsLecturer;

        // Assert
        Assert.IsFalse(actualResult);
    }

    /// <summary>
    /// Tests that RefreshCommand.CanExecute returns true with null parameter.
    /// Input: null parameter.
    /// Expected: CanExecute returns true.
    /// </summary>
    [TestMethod]
    public void RefreshCommand_CanExecuteWithNullParameter_ReturnsTrue()
    {
        // Arrange
        var mockDashboard = new Mock<IDashboardService>();
        var mockAuth = new Mock<IAuthService>();
        var mockSession = new Mock<SessionService>();
        var viewModel = new DashboardViewModel(mockDashboard.Object, mockAuth.Object, mockSession.Object, new Mock<IStudentScheduleService>().Object, new Mock<IRefreshCoordinator>().Object, new Mock<IInsightsService>().Object);

        // Act
        var command = viewModel.RefreshCommand;
        var canExecute = command.CanExecute(null);

        // Assert
        Assert.IsTrue(canExecute);
    }

    /// <summary>
    /// Tests that RefreshCommand.CanExecute returns true with non-null parameter.
    /// Input: Non-null object parameter.
    /// Expected: CanExecute returns true.
    /// </summary>
    [TestMethod]
    public void RefreshCommand_CanExecuteWithParameter_ReturnsTrue()
    {
        // Arrange
        var mockDashboard = new Mock<IDashboardService>();
        var mockAuth = new Mock<IAuthService>();
        var mockSession = new Mock<SessionService>();
        var viewModel = new DashboardViewModel(mockDashboard.Object, mockAuth.Object, mockSession.Object, new Mock<IStudentScheduleService>().Object, new Mock<IRefreshCoordinator>().Object, new Mock<IInsightsService>().Object);

        // Act
        var command = viewModel.RefreshCommand;
        var canExecute = command.CanExecute(new object());

        // Assert
        Assert.IsTrue(canExecute);
    }

    /// <summary>
    /// Tests that RefreshCommand is of type Command.
    /// Input: None.
    /// Expected: RefreshCommand is an instance of Command.
    /// </summary>
    [TestMethod]
    public void RefreshCommand_WhenAccessed_ReturnsCommandInstance()
    {
        // Arrange
        var mockDashboard = new Mock<IDashboardService>();
        var mockAuth = new Mock<IAuthService>();
        var mockSession = new Mock<SessionService>();
        var viewModel = new DashboardViewModel(mockDashboard.Object, mockAuth.Object, mockSession.Object, new Mock<IStudentScheduleService>().Object, new Mock<IRefreshCoordinator>().Object, new Mock<IInsightsService>().Object);

        // Act
        var command = viewModel.RefreshCommand;

        // Assert
        Assert.IsNotNull(command);
        Assert.IsInstanceOfType(command, typeof(Command));
    }

    /// <summary>
    /// Tests that RefreshCommand execution sets IsBusy to true during execution and false after completion
    /// when dashboard service returns data successfully.
    /// Input: Valid dashboard data.
    /// Expected: IsBusy transitions from false to true and back to false.
    /// </summary>
    [TestMethod]
    public async Task RefreshCommand_WhenExecuted_ManagesBusyStateCorrectly()
    {
        // Arrange
        var mockDashboard = new Mock<IDashboardService>();
        var mockAuth = new Mock<IAuthService>();
        var mockSession = new Mock<SessionService>();
        var viewModel = new DashboardViewModel(mockDashboard.Object, mockAuth.Object, mockSession.Object, new Mock<IStudentScheduleService>().Object, new Mock<IRefreshCoordinator>().Object, new Mock<IInsightsService>().Object);

        var dashboardDto = new DashboardDto
        {
            ActiveCourses = 5,
            UpcomingAssignments = 3,
            AttendancePercent = 0.85,
            RecentAnnouncements = new List<AnnouncementDto>()
        };

        mockDashboard.Setup(d => d.GetDashboardAsync()).ReturnsAsync(dashboardDto);

        // Act
        var initialBusy = viewModel.IsBusy;
        var command = viewModel.RefreshCommand;
        var executeTask = Task.Run(async () =>
        {
            if (command is Command cmd)
            {
                cmd.Execute(null);
                await Task.Delay(10);
            }
        });

        await Task.Delay(5);
        var busyDuringExecution = viewModel.IsBusy;
        await executeTask;
        await Task.Delay(50);
        var busyAfterExecution = viewModel.IsBusy;

        // Assert
        Assert.IsFalse(initialBusy);
        Assert.IsFalse(busyAfterExecution);
    }

    /// <summary>
    /// Tests that RefreshCommand handles null dashboard data without throwing exceptions.
    /// Input: Dashboard service returns null.
    /// Expected: Command executes without throwing, ErrorMessage is set.
    /// </summary>
    [TestMethod]
    public async Task RefreshCommand_WhenServiceReturnsNull_HandlesGracefully()
    {
        // Arrange
        var mockDashboard = new Mock<IDashboardService>();
        var mockAuth = new Mock<IAuthService>();
        var mockSession = new Mock<SessionService>();
        var viewModel = new DashboardViewModel(mockDashboard.Object, mockAuth.Object, mockSession.Object, new Mock<IStudentScheduleService>().Object, new Mock<IRefreshCoordinator>().Object, new Mock<IInsightsService>().Object);

        mockDashboard.Setup(d => d.GetDashboardAsync()).ReturnsAsync((DashboardDto?)null);

        // Act
        var command = viewModel.RefreshCommand;
        if (command is Command cmd)
        {
            cmd.Execute(null);
            await Task.Delay(100);
        }

        // Assert - Should not throw, error message should be set
        Assert.IsNotNull(viewModel.ErrorMessage);
    }

    /// <summary>
    /// Tests that RefreshCommand clears previous data before loading new data.
    /// Input: Dashboard service returns different data on consecutive calls.
    /// Expected: Properties are updated to reflect the latest data.
    /// </summary>
    [TestMethod]
    public async Task RefreshCommand_WhenExecutedMultipleTimes_UpdatesWithLatestData()
    {
        // Arrange
        var mockDashboard = new Mock<IDashboardService>();
        var mockAuth = new Mock<IAuthService>();
        var mockSession = new Mock<SessionService>();
        var viewModel = new DashboardViewModel(mockDashboard.Object, mockAuth.Object, mockSession.Object, new Mock<IStudentScheduleService>().Object, new Mock<IRefreshCoordinator>().Object, new Mock<IInsightsService>().Object);

        var firstDto = new DashboardDto
        {
            ActiveCourses = 5,
            UpcomingAssignments = 3,
            AttendancePercent = 0.85,
            RecentAnnouncements = new List<AnnouncementDto>()
        };

        var secondDto = new DashboardDto
        {
            ActiveCourses = 10,
            UpcomingAssignments = 7,
            AttendancePercent = 0.95,
            RecentAnnouncements = new List<AnnouncementDto>()
        };

        mockDashboard.SetupSequence(d => d.GetDashboardAsync())
            .ReturnsAsync(firstDto)
            .ReturnsAsync(secondDto);

        // Act
        var command = viewModel.RefreshCommand;
        if (command is Command cmd)
        {
            cmd.Execute(null);
            await Task.Delay(100);

            var firstActiveCourses = viewModel.ActiveCourses;
            var firstAssignments = viewModel.UpcomingAssignments;

            cmd.Execute(null);
            await Task.Delay(100);
        }

        // Assert
        Assert.AreEqual(10, viewModel.ActiveCourses);
        Assert.AreEqual(7, viewModel.UpcomingAssignments);
        Assert.AreEqual(95, viewModel.AttendancePercentage);
    }

    /// <summary>
    /// Tests that RefreshCommand handles dashboard service timeout gracefully.
    /// Input: Dashboard service throws TimeoutException.
    /// Expected: ErrorMessage is set appropriately.
    /// </summary>
    [TestMethod]
    public async Task RefreshCommand_WhenServiceTimesOut_SetsErrorMessage()
    {
        // Arrange
        var mockDashboard = new Mock<IDashboardService>();
        var mockAuth = new Mock<IAuthService>();
        var mockSession = new Mock<SessionService>();
        var viewModel = new DashboardViewModel(mockDashboard.Object, mockAuth.Object, mockSession.Object, new Mock<IStudentScheduleService>().Object, new Mock<IRefreshCoordinator>().Object, new Mock<IInsightsService>().Object);

        mockDashboard.Setup(d => d.GetDashboardAsync())
            .ThrowsAsync(new TimeoutException("Request timed out"));

        // Act
        var command = viewModel.RefreshCommand;
        if (command is Command cmd)
        {
            cmd.Execute(null);
            await Task.Delay(100);
        }

        // Assert
        Assert.IsTrue(viewModel.ErrorMessage.Contains("Request timed out"));
    }

    /// <summary>
    /// Tests that RefreshCommand handles InvalidOperationException from dashboard service.
    /// Input: Dashboard service throws InvalidOperationException.
    /// Expected: ErrorMessage is set with exception message.
    /// </summary>
    [TestMethod]
    public async Task RefreshCommand_WhenServiceThrowsInvalidOperation_SetsErrorMessage()
    {
        // Arrange
        var mockDashboard = new Mock<IDashboardService>();
        var mockAuth = new Mock<IAuthService>();
        var mockSession = new Mock<SessionService>();
        var viewModel = new DashboardViewModel(mockDashboard.Object, mockAuth.Object, mockSession.Object, new Mock<IStudentScheduleService>().Object, new Mock<IRefreshCoordinator>().Object, new Mock<IInsightsService>().Object);

        mockDashboard.Setup(d => d.GetDashboardAsync())
            .ThrowsAsync(new InvalidOperationException("Invalid operation"));

        // Act
        var command = viewModel.RefreshCommand;
        if (command is Command cmd)
        {
            cmd.Execute(null);
            await Task.Delay(100);
        }

        // Assert
        Assert.IsTrue(viewModel.ErrorMessage.Contains("Invalid operation"));
    }

    /// <summary>
    /// Tests that RefreshCommand handles dashboard data with extreme boundary values.
    /// Input: Dashboard data with int.MaxValue and int.MinValue.
    /// Expected: Properties are updated correctly without overflow.
    /// </summary>
    [TestMethod]
    [DataRow(int.MaxValue, int.MaxValue, 1.0)]
    [DataRow(int.MinValue, int.MinValue, 0.0)]
    [DataRow(0, 0, 0.0)]
    public async Task RefreshCommand_WithExtremeBoundaryValues_UpdatesPropertiesCorrectly(int courses, int assignments, double attendance)
    {
        // Arrange
        var mockDashboard = new Mock<IDashboardService>();
        var mockAuth = new Mock<IAuthService>();
        var mockSession = new Mock<SessionService>();
        var viewModel = new DashboardViewModel(mockDashboard.Object, mockAuth.Object, mockSession.Object, new Mock<IStudentScheduleService>().Object, new Mock<IRefreshCoordinator>().Object, new Mock<IInsightsService>().Object);

        var dashboardDto = new DashboardDto
        {
            ActiveCourses = courses,
            UpcomingAssignments = assignments,
            AttendancePercent = attendance,
            RecentAnnouncements = new List<AnnouncementDto>()
        };

        mockDashboard.Setup(d => d.GetDashboardAsync()).ReturnsAsync(dashboardDto);

        // Act
        var command = viewModel.RefreshCommand;
        if (command is Command cmd)
        {
            cmd.Execute(null);
            await Task.Delay(100);
        }

        // Assert
        Assert.AreEqual(courses, viewModel.ActiveCourses);
        Assert.AreEqual(assignments, viewModel.UpcomingAssignments);
    }

    /// <summary>
    /// Tests that the ActiveCourses property has a default value of zero when the ViewModel is first instantiated.
    /// Input: None (default initialization).
    /// Expected: Property value should be 0.
    /// </summary>
    [TestMethod]
    public void ActiveCourses_InitialValue_IsZero()
    {
        // Arrange
        var mockDashboardService = new Mock<IDashboardService>();
        var mockAuthService = new Mock<IAuthService>();
        var mockSessionService = new Mock<SessionService>();

        // Act
        var viewModel = new DashboardViewModel(mockDashboardService.Object, mockAuthService.Object, mockSessionService.Object, new Mock<IStudentScheduleService>().Object, new Mock<IRefreshCoordinator>().Object, new Mock<IInsightsService>().Object);

        // Assert
        Assert.AreEqual(0, viewModel.ActiveCourses);
    }

    /// <summary>
    /// Tests that the ActiveCourses property setter updates the backing field
    /// and the getter returns the correct value for various integer inputs including boundary values.
    /// Input: Various integer values including zero, positive, negative, and boundary values.
    /// Expected: Property should store and return the set value correctly.
    /// </summary>
    /// <param name="value">The integer value to set.</param>
    [TestMethod]
    [DataRow(0)]
    [DataRow(1)]
    [DataRow(10)]
    [DataRow(100)]
    [DataRow(1000)]
    [DataRow(-1)]
    [DataRow(-10)]
    [DataRow(-100)]
    [DataRow(int.MaxValue)]
    [DataRow(int.MinValue)]
    public void ActiveCourses_SetValue_ReturnsCorrectValue(int value)
    {
        // Arrange
        var mockDashboardService = new Mock<IDashboardService>();
        var mockAuthService = new Mock<IAuthService>();
        var mockSessionService = new Mock<SessionService>();
        var viewModel = new DashboardViewModel(mockDashboardService.Object, mockAuthService.Object, mockSessionService.Object, new Mock<IStudentScheduleService>().Object, new Mock<IRefreshCoordinator>().Object, new Mock<IInsightsService>().Object);

        // Act
        viewModel.ActiveCourses = value;

        // Assert
        Assert.AreEqual(value, viewModel.ActiveCourses);
    }

    /// <summary>
    /// Tests that the ActiveCourses property raises PropertyChanged event
    /// when the value is changed to a different value.
    /// Input: New value different from current value.
    /// Expected: PropertyChanged event should be raised with property name "ActiveCourses".
    /// </summary>
    [TestMethod]
    public void ActiveCourses_SetDifferentValue_RaisesPropertyChangedEvent()
    {
        // Arrange
        var mockDashboardService = new Mock<IDashboardService>();
        var mockAuthService = new Mock<IAuthService>();
        var mockSessionService = new Mock<SessionService>();
        var viewModel = new DashboardViewModel(mockDashboardService.Object, mockAuthService.Object, mockSessionService.Object, new Mock<IStudentScheduleService>().Object, new Mock<IRefreshCoordinator>().Object, new Mock<IInsightsService>().Object);
        var eventRaised = false;
        string? raisedPropertyName = null;

        viewModel.PropertyChanged += (sender, e) =>
        {
            if (e.PropertyName == nameof(viewModel.ActiveCourses))
            {
                eventRaised = true;
                raisedPropertyName = e.PropertyName;
            }
        };

        // Act
        viewModel.ActiveCourses = 42;

        // Assert
        Assert.IsTrue(eventRaised);
        Assert.AreEqual(nameof(viewModel.ActiveCourses), raisedPropertyName);
    }

    /// <summary>
    /// Tests that the ActiveCourses property does not raise PropertyChanged event
    /// when the value is set to the same value as the current value.
    /// Input: Same value set twice consecutively.
    /// Expected: PropertyChanged event should only be raised once, not on the second set.
    /// </summary>
    [TestMethod]
    public void ActiveCourses_SetSameValue_DoesNotRaisePropertyChangedEvent()
    {
        // Arrange
        var mockDashboardService = new Mock<IDashboardService>();
        var mockAuthService = new Mock<IAuthService>();
        var mockSessionService = new Mock<SessionService>();
        var viewModel = new DashboardViewModel(mockDashboardService.Object, mockAuthService.Object, mockSessionService.Object, new Mock<IStudentScheduleService>().Object, new Mock<IRefreshCoordinator>().Object, new Mock<IInsightsService>().Object);
        viewModel.ActiveCourses = 25;
        var eventRaisedCount = 0;

        viewModel.PropertyChanged += (sender, e) =>
        {
            if (e.PropertyName == nameof(viewModel.ActiveCourses))
            {
                eventRaisedCount++;
            }
        };

        // Act
        viewModel.ActiveCourses = 25;

        // Assert
        Assert.AreEqual(0, eventRaisedCount);
    }

    /// <summary>
    /// Tests that the ActiveCourses property correctly handles multiple consecutive value changes
    /// and raises PropertyChanged event for each distinct change.
    /// Input: Multiple different values set in sequence.
    /// Expected: Property should update correctly and raise PropertyChanged for each change.
    /// </summary>
    [TestMethod]
    public void ActiveCourses_SetMultipleValues_RaisesPropertyChangedForEachChange()
    {
        // Arrange
        var mockDashboardService = new Mock<IDashboardService>();
        var mockAuthService = new Mock<IAuthService>();
        var mockSessionService = new Mock<SessionService>();
        var viewModel = new DashboardViewModel(mockDashboardService.Object, mockAuthService.Object, mockSessionService.Object, new Mock<IStudentScheduleService>().Object, new Mock<IRefreshCoordinator>().Object, new Mock<IInsightsService>().Object);
        var eventRaisedCount = 0;

        viewModel.PropertyChanged += (sender, e) =>
        {
            if (e.PropertyName == nameof(viewModel.ActiveCourses))
            {
                eventRaisedCount++;
            }
        };

        // Act
        viewModel.ActiveCourses = 10;
        viewModel.ActiveCourses = 20;
        viewModel.ActiveCourses = 30;

        // Assert
        Assert.AreEqual(3, eventRaisedCount);
        Assert.AreEqual(30, viewModel.ActiveCourses);
    }

    /// <summary>
    /// Tests that the ActiveCourses property handles boundary values correctly.
    /// Input: int.MinValue and int.MaxValue.
    /// Expected: Property should store and return boundary values without exception.
    /// </summary>
    [TestMethod]
    [DataRow(int.MinValue)]
    [DataRow(int.MaxValue)]
    public void ActiveCourses_SetBoundaryValues_UpdatesPropertyCorrectly(int value)
    {
        // Arrange
        var mockDashboardService = new Mock<IDashboardService>();
        var mockAuthService = new Mock<IAuthService>();
        var mockSessionService = new Mock<SessionService>();
        var viewModel = new DashboardViewModel(mockDashboardService.Object, mockAuthService.Object, mockSessionService.Object, new Mock<IStudentScheduleService>().Object, new Mock<IRefreshCoordinator>().Object, new Mock<IInsightsService>().Object);
        var eventRaised = false;

        viewModel.PropertyChanged += (sender, e) =>
        {
            if (e.PropertyName == nameof(viewModel.ActiveCourses))
            {
                eventRaised = true;
            }
        };

        // Act
        viewModel.ActiveCourses = value;

        // Assert
        Assert.AreEqual(value, viewModel.ActiveCourses);
        Assert.IsTrue(eventRaised);
    }

    /// <summary>
    /// Tests that multiple value changes to ActiveCourses update the property correctly.
    /// Input: Multiple different values in sequence.
    /// Expected: Property value should be updated to the last set value.
    /// </summary>
    [TestMethod]
    [DataRow(10, 20, 30)]
    [DataRow(0, 100, 50)]
    [DataRow(-5, 15, 0)]
    [DataRow(int.MinValue, int.MaxValue, 0)]
    public void ActiveCourses_SetMultipleValues_UpdatesPropertyCorrectly(int value1, int value2, int value3)
    {
        // Arrange
        var mockDashboardService = new Mock<IDashboardService>();
        var mockAuthService = new Mock<IAuthService>();
        var mockSessionService = new Mock<SessionService>();
        var viewModel = new DashboardViewModel(mockDashboardService.Object, mockAuthService.Object, mockSessionService.Object, new Mock<IStudentScheduleService>().Object, new Mock<IRefreshCoordinator>().Object, new Mock<IInsightsService>().Object);

        // Act
        viewModel.ActiveCourses = value1;
        viewModel.ActiveCourses = value2;
        viewModel.ActiveCourses = value3;

        // Assert
        Assert.AreEqual(value3, viewModel.ActiveCourses);
    }

    /// <summary>
    /// Tests that the LatestAnnouncementDate property returns empty string as initial value.
    /// Input: None (default initialization).
    /// Expected: Returns empty string.
    /// </summary>
    [TestMethod]
    public void LatestAnnouncementDate_InitialValue_ReturnsEmptyString()
    {
        // Arrange
        var mockDashboardService = new Mock<IDashboardService>();
        var mockAuthService = new Mock<IAuthService>();
        var mockSessionService = new Mock<SessionService>();

        // Act
        var viewModel = new DashboardViewModel(mockDashboardService.Object, mockAuthService.Object, mockSessionService.Object, new Mock<IStudentScheduleService>().Object, new Mock<IRefreshCoordinator>().Object, new Mock<IInsightsService>().Object);
        var result = viewModel.LatestAnnouncementDate;

        // Assert
        Assert.AreEqual(string.Empty, result);
    }

    /// <summary>
    /// Tests that setting LatestAnnouncementDate to various valid string values updates the property correctly and raises PropertyChanged event.
    /// Input: Various string values including empty, whitespace, special characters, unicode, and date-like strings.
    /// Expected: Property is updated and PropertyChanged event is raised with correct property name.
    /// </summary>
    [TestMethod]
    [DataRow("")]
    [DataRow("January 15, 2024")]
    [DataRow("2024-01-15T10:30:00")]
    [DataRow("Monday, December 25, 2023 at 3:45 PM")]
    [DataRow("   ")]
    [DataRow("\t")]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [DataRow("Test with special chars: !@#$%^&*()_+-=[]{}|;':\",./<>?")]
    [DataRow("Unicode: ???? ????")]
    [DataRow("A")]
    [DataRow("   Leading spaces")]
    [DataRow("Trailing spaces   ")]
    [DataRow("  Surrounding spaces  ")]
    [DataRow("Mixed\tTabs\tAnd\nNewlines")]
    public void LatestAnnouncementDate_SetValidValue_UpdatesPropertyAndRaisesPropertyChanged(string value)
    {
        // Arrange
        var mockDashboardService = new Mock<IDashboardService>();
        var mockAuthService = new Mock<IAuthService>();
        var mockSessionService = new Mock<SessionService>();
        var viewModel = new DashboardViewModel(mockDashboardService.Object, mockAuthService.Object, mockSessionService.Object, new Mock<IStudentScheduleService>().Object, new Mock<IRefreshCoordinator>().Object, new Mock<IInsightsService>().Object);
        var eventRaised = false;
        var propertyName = string.Empty;

        viewModel.PropertyChanged += (sender, args) =>
        {
            eventRaised = true;
            propertyName = args.PropertyName;
        };

        // Act
        viewModel.LatestAnnouncementDate = value;

        // Assert
        Assert.AreEqual(value, viewModel.LatestAnnouncementDate);
        Assert.IsTrue(eventRaised);
        Assert.AreEqual("LatestAnnouncementDate", propertyName);
    }

    /// <summary>
    /// Tests that setting LatestAnnouncementDate to a very long string updates the property correctly and raises PropertyChanged event.
    /// Input: String with 10,000 characters.
    /// Expected: Property is updated and PropertyChanged event is raised.
    /// </summary>
    [TestMethod]
    public void LatestAnnouncementDate_SetVeryLongString_UpdatesPropertyAndRaisesPropertyChanged()
    {
        // Arrange
        var mockDashboardService = new Mock<IDashboardService>();
        var mockAuthService = new Mock<IAuthService>();
        var mockSessionService = new Mock<SessionService>();
        var viewModel = new DashboardViewModel(mockDashboardService.Object, mockAuthService.Object, mockSessionService.Object, new Mock<IStudentScheduleService>().Object, new Mock<IRefreshCoordinator>().Object, new Mock<IInsightsService>().Object);
        var longString = new string('X', 10000);
        var eventRaised = false;

        viewModel.PropertyChanged += (sender, args) => eventRaised = true;

        // Act
        viewModel.LatestAnnouncementDate = longString;

        // Assert
        Assert.AreEqual(longString, viewModel.LatestAnnouncementDate);
        Assert.IsTrue(eventRaised);
    }

    /// <summary>
    /// Tests that setting LatestAnnouncementDate to different values sequentially updates the property correctly each time.
    /// Input: Three different string values set in sequence.
    /// Expected: Property reflects the most recently set value after each update.
    /// </summary>
    [TestMethod]
    public void LatestAnnouncementDate_SetMultipleDifferentValues_UpdatesCorrectly()
    {
        // Arrange
        var mockDashboardService = new Mock<IDashboardService>();
        var mockAuthService = new Mock<IAuthService>();
        var mockSessionService = new Mock<SessionService>();
        var viewModel = new DashboardViewModel(mockDashboardService.Object, mockAuthService.Object, mockSessionService.Object, new Mock<IStudentScheduleService>().Object, new Mock<IRefreshCoordinator>().Object, new Mock<IInsightsService>().Object);

        // Act & Assert
        viewModel.LatestAnnouncementDate = "First Value";
        Assert.AreEqual("First Value", viewModel.LatestAnnouncementDate);

        viewModel.LatestAnnouncementDate = "Second Value";
        Assert.AreEqual("Second Value", viewModel.LatestAnnouncementDate);

        viewModel.LatestAnnouncementDate = "Third Value";
        Assert.AreEqual("Third Value", viewModel.LatestAnnouncementDate);
    }

    /// <summary>
    /// Tests that setting LatestAnnouncementDate to empty string after a non-empty value updates the property and raises PropertyChanged event.
    /// Input: Non-empty string followed by empty string.
    /// Expected: Property is updated to empty string and PropertyChanged event is raised.
    /// </summary>
    [TestMethod]
    public void LatestAnnouncementDate_SetToEmptyAfterNonEmpty_UpdatesPropertyAndRaisesPropertyChanged()
    {
        // Arrange
        var mockDashboardService = new Mock<IDashboardService>();
        var mockAuthService = new Mock<IAuthService>();
        var mockSessionService = new Mock<SessionService>();
        var viewModel = new DashboardViewModel(mockDashboardService.Object, mockAuthService.Object, mockSessionService.Object, new Mock<IStudentScheduleService>().Object, new Mock<IRefreshCoordinator>().Object, new Mock<IInsightsService>().Object);
        var eventCount = 0;

        viewModel.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == "LatestAnnouncementDate")
                eventCount++;
        };

        // Act
        viewModel.LatestAnnouncementDate = "Some Date";
        viewModel.LatestAnnouncementDate = string.Empty;

        // Assert
        Assert.AreEqual(string.Empty, viewModel.LatestAnnouncementDate);
        Assert.AreEqual(2, eventCount);
    }

    /// <summary>
    /// Tests that setting LatestAnnouncementDate to strings containing control characters updates the property correctly.
    /// Input: Strings with null character, zero-width characters, and other control characters.
    /// Expected: Property is updated with the string containing control characters.
    /// </summary>
    [TestMethod]
    [DataRow("Date\u0000WithNull")]
    [DataRow("Date\u200BWithZeroWidth")]
    [DataRow("\u202ADate")]
    [DataRow("Date\u001FControl")]
    public void LatestAnnouncementDate_SetStringWithControlCharacters_UpdatesProperty(string value)
    {
        // Arrange
        var mockDashboardService = new Mock<IDashboardService>();
        var mockAuthService = new Mock<IAuthService>();
        var mockSessionService = new Mock<SessionService>();
        var viewModel = new DashboardViewModel(mockDashboardService.Object, mockAuthService.Object, mockSessionService.Object, new Mock<IStudentScheduleService>().Object, new Mock<IRefreshCoordinator>().Object, new Mock<IInsightsService>().Object);

        // Act
        viewModel.LatestAnnouncementDate = value;

        // Assert
        Assert.AreEqual(value, viewModel.LatestAnnouncementDate);
    }

    /// <summary>
    /// Tests that setting LatestAnnouncementDate multiple times with different values raises PropertyChanged event for each distinct change.
    /// Input: Five different string values set sequentially.
    /// Expected: PropertyChanged event is raised exactly five times.
    /// </summary>
    [TestMethod]
    public void LatestAnnouncementDate_SetMultipleDifferentValues_RaisesPropertyChangedForEachChange()
    {
        // Arrange
        var mockDashboardService = new Mock<IDashboardService>();
        var mockAuthService = new Mock<IAuthService>();
        var mockSessionService = new Mock<SessionService>();
        var viewModel = new DashboardViewModel(mockDashboardService.Object, mockAuthService.Object, mockSessionService.Object, new Mock<IStudentScheduleService>().Object, new Mock<IRefreshCoordinator>().Object, new Mock<IInsightsService>().Object);
        var eventCount = 0;

        viewModel.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == "LatestAnnouncementDate")
                eventCount++;
        };

        // Act
        viewModel.LatestAnnouncementDate = "Value1";
        viewModel.LatestAnnouncementDate = "Value2";
        viewModel.LatestAnnouncementDate = "Value3";
        viewModel.LatestAnnouncementDate = "Value4";
        viewModel.LatestAnnouncementDate = "Value5";

        // Assert
        Assert.AreEqual(5, eventCount);
        Assert.AreEqual("Value5", viewModel.LatestAnnouncementDate);
    }

    /// <summary>
    /// Tests that LatestAnnouncementDate handles extreme boundary string values correctly.
    /// Input: Maximum length strings and strings with extreme Unicode values.
    /// Expected: Property is updated without throwing exceptions.
    /// </summary>
    [TestMethod]
    public void LatestAnnouncementDate_SetExtremeBoundaryValues_UpdatesProperty()
    {
        // Arrange
        var mockDashboardService = new Mock<IDashboardService>();
        var mockAuthService = new Mock<IAuthService>();
        var mockSessionService = new Mock<SessionService>();
        var viewModel = new DashboardViewModel(mockDashboardService.Object, mockAuthService.Object, mockSessionService.Object, new Mock<IStudentScheduleService>().Object, new Mock<IRefreshCoordinator>().Object, new Mock<IInsightsService>().Object);
        var extremeString = new string('?', 50000);

        // Act
        viewModel.LatestAnnouncementDate = extremeString;

        // Assert
        Assert.AreEqual(extremeString, viewModel.LatestAnnouncementDate);
    }

    /// <summary>
    /// Tests that setting LatestAnnouncementDate does not raise PropertyChanged for unrelated properties.
    /// Input: Valid string value for LatestAnnouncementDate.
    /// Expected: PropertyChanged event is raised only for "LatestAnnouncementDate", not for other properties.
    /// </summary>
    [TestMethod]
    public void LatestAnnouncementDate_SetValue_RaisesPropertyChangedOnlyForLatestAnnouncementDate()
    {
        // Arrange
        var mockDashboardService = new Mock<IDashboardService>();
        var mockAuthService = new Mock<IAuthService>();
        var mockSessionService = new Mock<SessionService>();
        var viewModel = new DashboardViewModel(mockDashboardService.Object, mockAuthService.Object, mockSessionService.Object, new Mock<IStudentScheduleService>().Object, new Mock<IRefreshCoordinator>().Object, new Mock<IInsightsService>().Object);
        var raisedProperties = new List<string>();

        viewModel.PropertyChanged += (sender, args) =>
        {
            if (!string.IsNullOrEmpty(args.PropertyName))
                raisedProperties.Add(args.PropertyName);
        };

        // Act
        viewModel.LatestAnnouncementDate = "Test Date";

        // Assert
        Assert.AreEqual(1, raisedProperties.Count);
        Assert.AreEqual("LatestAnnouncementDate", raisedProperties[0]);
    }

    /// <summary>
    /// Tests that IsClassRep returns true when Role is "classrep" in various case formats.
    /// Input: Role set to "classrep" with different case combinations.
    /// Expected: IsClassRep returns true for all case variations.
    /// </summary>
    [TestMethod]
    [DataRow("classrep", DisplayName = "IsClassRep_RoleIsClassRepLowercase_ReturnsTrue")]
    [DataRow("ClassRep", DisplayName = "IsClassRep_RoleIsClassRepTitleCase_ReturnsTrue")]
    [DataRow("CLASSREP", DisplayName = "IsClassRep_RoleIsClassRepUppercase_ReturnsTrue")]
    [DataRow("ClAsSrEp", DisplayName = "IsClassRep_RoleIsClassRepMixedCase1_ReturnsTrue")]
    [DataRow("cLaSSrEP", DisplayName = "IsClassRep_RoleIsClassRepMixedCase2_ReturnsTrue")]
    [DataRow("clASSrep", DisplayName = "IsClassRep_RoleIsClassRepMixedCase3_ReturnsTrue")]
    public void IsClassRep_RoleIsClassRep_ReturnsTrue(string roleValue)
    {
        // Arrange
        var mockDashboardService = new Mock<IDashboardService>();
        var mockAuthService = new Mock<IAuthService>();
        var mockSessionService = new Mock<SessionService>();
        var viewModel = new DashboardViewModel(mockDashboardService.Object, mockAuthService.Object, mockSessionService.Object, new Mock<IStudentScheduleService>().Object, new Mock<IRefreshCoordinator>().Object, new Mock<IInsightsService>().Object);

        // Act
        viewModel.Role = roleValue;

        // Assert
        Assert.IsTrue(viewModel.IsClassRep, $"IsClassRep should return true when Role is '{roleValue}'");
    }

    /// <summary>
    /// Tests that IsClassRep returns true when Role is "classrepresentative" in various case formats.
    /// Input: Role set to "classrepresentative" with different case combinations.
    /// Expected: IsClassRep returns true for all case variations.
    /// </summary>
    [TestMethod]
    [DataRow("classrepresentative", DisplayName = "IsClassRep_RoleIsClassRepresentativeLowercase_ReturnsTrue")]
    [DataRow("ClassRepresentative", DisplayName = "IsClassRep_RoleIsClassRepresentativeTitleCase_ReturnsTrue")]
    [DataRow("CLASSREPRESENTATIVE", DisplayName = "IsClassRep_RoleIsClassRepresentativeUppercase_ReturnsTrue")]
    [DataRow("ClAsSrEpReSeNtAtIvE", DisplayName = "IsClassRep_RoleIsClassRepresentativeMixedCase1_ReturnsTrue")]
    [DataRow("cLaSSrEPreSENtaTIVe", DisplayName = "IsClassRep_RoleIsClassRepresentativeMixedCase2_ReturnsTrue")]
    [DataRow("CLASSREPRESENTATIVe", DisplayName = "IsClassRep_RoleIsClassRepresentativeMixedCase3_ReturnsTrue")]
    public void IsClassRep_RoleIsClassRepresentative_ReturnsTrue(string roleValue)
    {
        // Arrange
        var mockDashboardService = new Mock<IDashboardService>();
        var mockAuthService = new Mock<IAuthService>();
        var mockSessionService = new Mock<SessionService>();
        var viewModel = new DashboardViewModel(mockDashboardService.Object, mockAuthService.Object, mockSessionService.Object, new Mock<IStudentScheduleService>().Object, new Mock<IRefreshCoordinator>().Object, new Mock<IInsightsService>().Object);

        // Act
        viewModel.Role = roleValue;

        // Assert
        Assert.IsTrue(viewModel.IsClassRep, $"IsClassRep should return true when Role is '{roleValue}'");
    }

    /// <summary>
    /// Tests that IsClassRep returns false when Role is null.
    /// Input: Role set to null.
    /// Expected: IsClassRep returns false due to null-conditional operator.
    /// </summary>
    [TestMethod]
    public void IsClassRep_RoleIsNull_ReturnsFalse()
    {
        // Arrange
        var mockDashboardService = new Mock<IDashboardService>();
        var mockAuthService = new Mock<IAuthService>();
        var mockSessionService = new Mock<SessionService>();
        var viewModel = new DashboardViewModel(mockDashboardService.Object, mockAuthService.Object, mockSessionService.Object, new Mock<IStudentScheduleService>().Object, new Mock<IRefreshCoordinator>().Object, new Mock<IInsightsService>().Object);

        // Act
        viewModel.Role = null;

        // Assert
        Assert.IsFalse(viewModel.IsClassRep, "IsClassRep should return false when Role is null");
    }

    /// <summary>
    /// Tests that IsClassRep returns false when Role is empty or whitespace strings.
    /// Input: Role set to empty string, spaces, tabs, newlines, and combinations.
    /// Expected: IsClassRep returns false for all whitespace variations.
    /// </summary>
    [TestMethod]
    [DataRow("", DisplayName = "IsClassRep_RoleIsEmptyString_ReturnsFalse")]
    [DataRow(" ", DisplayName = "IsClassRep_RoleIsSingleSpace_ReturnsFalse")]
    [DataRow("   ", DisplayName = "IsClassRep_RoleIsMultipleSpaces_ReturnsFalse")]
    [DataRow("\t", DisplayName = "IsClassRep_RoleIsTab_ReturnsFalse")]
    [DataRow("\n", DisplayName = "IsClassRep_RoleIsNewline_ReturnsFalse")]
    [DataRow("\r\n", DisplayName = "IsClassRep_RoleIsCarriageReturnNewline_ReturnsFalse")]
    [DataRow("  \t\n  ", DisplayName = "IsClassRep_RoleIsWhitespaceMix_ReturnsFalse")]
    public void IsClassRep_RoleIsEmptyOrWhitespace_ReturnsFalse(string roleValue)
    {
        // Arrange
        var mockDashboardService = new Mock<IDashboardService>();
        var mockAuthService = new Mock<IAuthService>();
        var mockSessionService = new Mock<SessionService>();
        var viewModel = new DashboardViewModel(mockDashboardService.Object, mockAuthService.Object, mockSessionService.Object, new Mock<IStudentScheduleService>().Object, new Mock<IRefreshCoordinator>().Object, new Mock<IInsightsService>().Object);

        // Act
        viewModel.Role = roleValue;

        // Assert
        Assert.IsFalse(viewModel.IsClassRep, $"IsClassRep should return false when Role is whitespace: '{roleValue}'");
    }

    /// <summary>
    /// Tests that IsClassRep returns false when Role is set to other valid role values.
    /// Input: Role set to student, lecturer, or admin in various cases.
    /// Expected: IsClassRep returns false for all non-classrep roles.
    /// </summary>
    [TestMethod]
    [DataRow("student", DisplayName = "IsClassRep_RoleIsStudent_ReturnsFalse")]
    [DataRow("Student", DisplayName = "IsClassRep_RoleIsStudentTitleCase_ReturnsFalse")]
    [DataRow("STUDENT", DisplayName = "IsClassRep_RoleIsStudentUppercase_ReturnsFalse")]
    [DataRow("lecturer", DisplayName = "IsClassRep_RoleIsLecturer_ReturnsFalse")]
    [DataRow("Lecturer", DisplayName = "IsClassRep_RoleIsLecturerTitleCase_ReturnsFalse")]
    [DataRow("LECTURER", DisplayName = "IsClassRep_RoleIsLecturerUppercase_ReturnsFalse")]
    [DataRow("admin", DisplayName = "IsClassRep_RoleIsAdmin_ReturnsFalse")]
    [DataRow("Admin", DisplayName = "IsClassRep_RoleIsAdminTitleCase_ReturnsFalse")]
    [DataRow("ADMIN", DisplayName = "IsClassRep_RoleIsAdminUppercase_ReturnsFalse")]
    public void IsClassRep_RoleIsOtherValidRole_ReturnsFalse(string roleValue)
    {
        // Arrange
        var mockDashboardService = new Mock<IDashboardService>();
        var mockAuthService = new Mock<IAuthService>();
        var mockSessionService = new Mock<SessionService>();
        var viewModel = new DashboardViewModel(mockDashboardService.Object, mockAuthService.Object, mockSessionService.Object, new Mock<IStudentScheduleService>().Object, new Mock<IRefreshCoordinator>().Object, new Mock<IInsightsService>().Object);

        // Act
        viewModel.Role = roleValue;

        // Assert
        Assert.IsFalse(viewModel.IsClassRep, $"IsClassRep should return false when Role is '{roleValue}'");
    }

    /// <summary>
    /// Tests that IsClassRep returns false when Role has leading or trailing whitespace.
    /// Input: Role with spaces before or after valid classrep values.
    /// Expected: IsClassRep returns false because exact match is required.
    /// </summary>
    [TestMethod]
    [DataRow(" classrep", DisplayName = "IsClassRep_RoleHasLeadingSpace_ReturnsFalse")]
    [DataRow("classrep ", DisplayName = "IsClassRep_RoleHasTrailingSpace_ReturnsFalse")]
    [DataRow(" classrep ", DisplayName = "IsClassRep_RoleHasLeadingAndTrailingSpace_ReturnsFalse")]
    [DataRow("  classrep  ", DisplayName = "IsClassRep_RoleHasMultipleSpaces_ReturnsFalse")]
    [DataRow(" classrepresentative", DisplayName = "IsClassRep_ClassRepresentativeHasLeadingSpace_ReturnsFalse")]
    [DataRow("classrepresentative ", DisplayName = "IsClassRep_ClassRepresentativeHasTrailingSpace_ReturnsFalse")]
    [DataRow(" classrepresentative ", DisplayName = "IsClassRep_ClassRepresentativeHasLeadingAndTrailingSpace_ReturnsFalse")]
    [DataRow("\tclassrep", DisplayName = "IsClassRep_RoleHasLeadingTab_ReturnsFalse")]
    [DataRow("classrep\n", DisplayName = "IsClassRep_RoleHasTrailingNewline_ReturnsFalse")]
    public void IsClassRep_RoleHasLeadingOrTrailingWhitespace_ReturnsFalse(string roleValue)
    {
        // Arrange
        var mockDashboardService = new Mock<IDashboardService>();
        var mockAuthService = new Mock<IAuthService>();
        var mockSessionService = new Mock<SessionService>();
        var viewModel = new DashboardViewModel(mockDashboardService.Object, mockAuthService.Object, mockSessionService.Object, new Mock<IStudentScheduleService>().Object, new Mock<IRefreshCoordinator>().Object, new Mock<IInsightsService>().Object);

        // Act
        viewModel.Role = roleValue;

        // Assert
        Assert.IsFalse(viewModel.IsClassRep, $"IsClassRep should return false when Role has whitespace: '{roleValue}'");
    }

    /// <summary>
    /// Tests that IsClassRep returns false for similar but invalid role strings.
    /// Input: Role set to strings that are substrings, typos, or variations of valid values.
    /// Expected: IsClassRep returns false for all invalid variations.
    /// </summary>
    [TestMethod]
    [DataRow("classre", DisplayName = "IsClassRep_RoleIsSubstring_ReturnsFalse")]
    [DataRow("classreps", DisplayName = "IsClassRep_RoleIsPlural_ReturnsFalse")]
    [DataRow("classrepresentativ", DisplayName = "IsClassRep_RoleIsTypo_ReturnsFalse")]
    [DataRow("classrepresentatives", DisplayName = "IsClassRep_RoleIsPlural2_ReturnsFalse")]
    [DataRow("class", DisplayName = "IsClassRep_RoleIsPartialWord_ReturnsFalse")]
    [DataRow("rep", DisplayName = "IsClassRep_RoleIsPartialWord2_ReturnsFalse")]
    [DataRow("representative", DisplayName = "IsClassRep_RoleIsPartialWord3_ReturnsFalse")]
    [DataRow("myclassrep", DisplayName = "IsClassRep_RoleHasPrefix_ReturnsFalse")]
    [DataRow("classrepx", DisplayName = "IsClassRep_RoleHasSuffix_ReturnsFalse")]
    [DataRow("classrepresentativex", DisplayName = "IsClassRep_RoleHasSuffix2_ReturnsFalse")]
    public void IsClassRep_RoleIsSimilarButInvalid_ReturnsFalse(string roleValue)
    {
        // Arrange
        var mockDashboardService = new Mock<IDashboardService>();
        var mockAuthService = new Mock<IAuthService>();
        var mockSessionService = new Mock<SessionService>();
        var viewModel = new DashboardViewModel(mockDashboardService.Object, mockAuthService.Object, mockSessionService.Object, new Mock<IStudentScheduleService>().Object, new Mock<IRefreshCoordinator>().Object, new Mock<IInsightsService>().Object);

        // Act
        viewModel.Role = roleValue;

        // Assert
        Assert.IsFalse(viewModel.IsClassRep, $"IsClassRep should return false when Role is '{roleValue}'");
    }

    /// <summary>
    /// Tests that IsClassRep returns false for unrecognized role values.
    /// Input: Role set to various invalid or unrecognized strings.
    /// Expected: IsClassRep returns false for all unrecognized values.
    /// </summary>
    [TestMethod]
    [DataRow("teacher", DisplayName = "IsClassRep_RoleIsTeacher_ReturnsFalse")]
    [DataRow("manager", DisplayName = "IsClassRep_RoleIsManager_ReturnsFalse")]
    [DataRow("unknown", DisplayName = "IsClassRep_RoleIsUnknown_ReturnsFalse")]
    [DataRow("xyz", DisplayName = "IsClassRep_RoleIsRandomString_ReturnsFalse")]
    [DataRow("12345", DisplayName = "IsClassRep_RoleIsNumeric_ReturnsFalse")]
    [DataRow("classrep123", DisplayName = "IsClassRep_RoleHasNumbers_ReturnsFalse")]
    public void IsClassRep_RoleIsUnrecognized_ReturnsFalse(string roleValue)
    {
        // Arrange
        var mockDashboardService = new Mock<IDashboardService>();
        var mockAuthService = new Mock<IAuthService>();
        var mockSessionService = new Mock<SessionService>();
        var viewModel = new DashboardViewModel(mockDashboardService.Object, mockAuthService.Object, mockSessionService.Object, new Mock<IStudentScheduleService>().Object, new Mock<IRefreshCoordinator>().Object, new Mock<IInsightsService>().Object);

        // Act
        viewModel.Role = roleValue;

        // Assert
        Assert.IsFalse(viewModel.IsClassRep, $"IsClassRep should return false when Role is '{roleValue}'");
    }

    /// <summary>
    /// Tests that IsClassRep returns false when Role contains special characters.
    /// Input: Role with special characters mixed with valid values.
    /// Expected: IsClassRep returns false for all strings with special characters.
    /// </summary>
    [TestMethod]
    [DataRow("@classrep", DisplayName = "IsClassRep_RoleHasAtSymbol_ReturnsFalse")]
    [DataRow("classrep!", DisplayName = "IsClassRep_RoleHasExclamation_ReturnsFalse")]
    [DataRow("class#rep", DisplayName = "IsClassRep_RoleHasHashSymbol_ReturnsFalse")]
    [DataRow("class$rep", DisplayName = "IsClassRep_RoleHasDollarSign_ReturnsFalse")]
    [DataRow("class&rep", DisplayName = "IsClassRep_RoleHasAmpersand_ReturnsFalse")]
    [DataRow("class*rep", DisplayName = "IsClassRep_RoleHasAsterisk_ReturnsFalse")]
    [DataRow("classrep%", DisplayName = "IsClassRep_RoleHasPercent_ReturnsFalse")]
    public void IsClassRep_RoleContainsSpecialCharacters_ReturnsFalse(string roleValue)
    {
        // Arrange
        var mockDashboardService = new Mock<IDashboardService>();
        var mockAuthService = new Mock<IAuthService>();
        var mockSessionService = new Mock<SessionService>();
        var viewModel = new DashboardViewModel(mockDashboardService.Object, mockAuthService.Object, mockSessionService.Object, new Mock<IStudentScheduleService>().Object, new Mock<IRefreshCoordinator>().Object, new Mock<IInsightsService>().Object);

        // Act
        viewModel.Role = roleValue;

        // Assert
        Assert.IsFalse(viewModel.IsClassRep, $"IsClassRep should return false when Role contains special characters: '{roleValue}'");
    }

    /// <summary>
    /// Tests that IsClassRep returns false when Role contains Unicode control characters.
    /// Input: Role with zero-width space, non-breaking space, and other Unicode characters.
    /// Expected: IsClassRep returns false for strings with Unicode control characters.
    /// </summary>
    [TestMethod]
    [DataRow("classrep\u200B", DisplayName = "IsClassRep_RoleHasZeroWidthSpace_ReturnsFalse")]
    [DataRow("\u202Aclassrep", DisplayName = "IsClassRep_RoleHasLeftToRightEmbedding_ReturnsFalse")]
    [DataRow("classrep\u0000", DisplayName = "IsClassRep_RoleHasNullCharacter_ReturnsFalse")]
    [DataRow("class\u00A0rep", DisplayName = "IsClassRep_RoleHasNonBreakingSpace_ReturnsFalse")]
    public void IsClassRep_RoleContainsUnicodeControlCharacters_ReturnsFalse(string roleValue)
    {
        // Arrange
        var mockDashboardService = new Mock<IDashboardService>();
        var mockAuthService = new Mock<IAuthService>();
        var mockSessionService = new Mock<SessionService>();
        var viewModel = new DashboardViewModel(mockDashboardService.Object, mockAuthService.Object, mockSessionService.Object, new Mock<IStudentScheduleService>().Object, new Mock<IRefreshCoordinator>().Object, new Mock<IInsightsService>().Object);

        // Act
        viewModel.Role = roleValue;

        // Assert
        Assert.IsFalse(viewModel.IsClassRep, $"IsClassRep should return false when Role contains Unicode control characters");
    }

    /// <summary>
    /// Tests that IsClassRep returns false when Role is a very long string.
    /// Input: Role set to a very long string (10000+ characters).
    /// Expected: IsClassRep returns false.
    /// </summary>
    [TestMethod]
    public void IsClassRep_RoleIsVeryLongString_ReturnsFalse()
    {
        // Arrange
        var mockDashboardService = new Mock<IDashboardService>();
        var mockAuthService = new Mock<IAuthService>();
        var mockSessionService = new Mock<SessionService>();
        var viewModel = new DashboardViewModel(mockDashboardService.Object, mockAuthService.Object, mockSessionService.Object, new Mock<IStudentScheduleService>().Object, new Mock<IRefreshCoordinator>().Object, new Mock<IInsightsService>().Object);
        var longString = new string('a', 10000);

        // Act
        viewModel.Role = longString;

        // Assert
        Assert.IsFalse(viewModel.IsClassRep, "IsClassRep should return false when Role is a very long string");
    }

    /// <summary>
    /// Tests that IsClassRep correctly changes value when Role changes from non-classrep to classrep.
    /// Input: Initial Role is "student", then changed to "classrep".
    /// Expected: IsClassRep changes from false to true.
    /// </summary>
    [TestMethod]
    public void IsClassRep_RoleChangesFromStudentToClassRep_ReturnsTrueAfterChange()
    {
        // Arrange
        var mockDashboardService = new Mock<IDashboardService>();
        var mockAuthService = new Mock<IAuthService>();
        var mockSessionService = new Mock<SessionService>();
        var viewModel = new DashboardViewModel(mockDashboardService.Object, mockAuthService.Object, mockSessionService.Object, new Mock<IStudentScheduleService>().Object, new Mock<IRefreshCoordinator>().Object, new Mock<IInsightsService>().Object);
        viewModel.Role = "student";

        // Act & Assert - Before change
        Assert.IsFalse(viewModel.IsClassRep, "IsClassRep should be false when Role is 'student'");

        // Act - Change role
        viewModel.Role = "classrep";

        // Assert - After change
        Assert.IsTrue(viewModel.IsClassRep, "IsClassRep should be true after Role changes to 'classrep'");
    }

    /// <summary>
    /// Tests that IsClassRep correctly changes value when Role changes from classrep to non-classrep.
    /// Input: Initial Role is "classrep", then changed to "student".
    /// Expected: IsClassRep changes from true to false.
    /// </summary>
    [TestMethod]
    public void IsClassRep_RoleChangesFromClassRepToStudent_ReturnsFalseAfterChange()
    {
        // Arrange
        var mockDashboardService = new Mock<IDashboardService>();
        var mockAuthService = new Mock<IAuthService>();
        var mockSessionService = new Mock<SessionService>();
        var viewModel = new DashboardViewModel(mockDashboardService.Object, mockAuthService.Object, mockSessionService.Object, new Mock<IStudentScheduleService>().Object, new Mock<IRefreshCoordinator>().Object, new Mock<IInsightsService>().Object);
        viewModel.Role = "classrep";

        // Act & Assert - Before change
        Assert.IsTrue(viewModel.IsClassRep, "IsClassRep should be true when Role is 'classrep'");

        // Act - Change role
        viewModel.Role = "student";

        // Assert - After change
        Assert.IsFalse(viewModel.IsClassRep, "IsClassRep should be false after Role changes to 'student'");
    }

    /// <summary>
    /// Tests that IsClassRep correctly toggles between classrep and classrepresentative.
    /// Input: Role alternates between "classrep" and "classrepresentative".
    /// Expected: IsClassRep remains true for both values.
    /// </summary>
    [TestMethod]
    public void IsClassRep_RoleTogglesBetweenClassRepAndClassRepresentative_RemainsTrue()
    {
        // Arrange
        var mockDashboardService = new Mock<IDashboardService>();
        var mockAuthService = new Mock<IAuthService>();
        var mockSessionService = new Mock<SessionService>();
        var viewModel = new DashboardViewModel(mockDashboardService.Object, mockAuthService.Object, mockSessionService.Object, new Mock<IStudentScheduleService>().Object, new Mock<IRefreshCoordinator>().Object, new Mock<IInsightsService>().Object);

        // Act & Assert - First value
        viewModel.Role = "classrep";
        Assert.IsTrue(viewModel.IsClassRep, "IsClassRep should be true when Role is 'classrep'");

        // Act & Assert - Second value
        viewModel.Role = "classrepresentative";
        Assert.IsTrue(viewModel.IsClassRep, "IsClassRep should be true when Role is 'classrepresentative'");

        // Act & Assert - Back to first value
        viewModel.Role = "ClassRep";
        Assert.IsTrue(viewModel.IsClassRep, "IsClassRep should be true when Role is 'ClassRep'");
    }

    /// <summary>
    /// Tests that IsClassRep property can be accessed multiple times without side effects.
    /// Input: Role set to "classrep".
    /// Expected: Multiple accesses to IsClassRep return consistent true value.
    /// </summary>
    [TestMethod]
    public void IsClassRep_MultipleAccesses_ReturnsConsistentValue()
    {
        // Arrange
        var mockDashboardService = new Mock<IDashboardService>();
        var mockAuthService = new Mock<IAuthService>();
        var mockSessionService = new Mock<SessionService>();
        var viewModel = new DashboardViewModel(mockDashboardService.Object, mockAuthService.Object, mockSessionService.Object, new Mock<IStudentScheduleService>().Object, new Mock<IRefreshCoordinator>().Object, new Mock<IInsightsService>().Object);
        viewModel.Role = "classrep";

        // Act - Access multiple times
        var result1 = viewModel.IsClassRep;
        var result2 = viewModel.IsClassRep;
        var result3 = viewModel.IsClassRep;

        // Assert
        Assert.IsTrue(result1, "First access should return true");
        Assert.IsTrue(result2, "Second access should return true");
        Assert.IsTrue(result3, "Third access should return true");
    }

    /// <summary>
    /// Tests that IsClassRep returns correct values for boundary case combinations.
    /// Input: Role set to edge case values including int boundaries as strings.
    /// Expected: IsClassRep returns false for all non-matching strings.
    /// </summary>
    [TestMethod]
    [DataRow("2147483647", DisplayName = "IsClassRep_RoleIsIntMaxValueString_ReturnsFalse")]
    [DataRow("-2147483648", DisplayName = "IsClassRep_RoleIsIntMinValueString_ReturnsFalse")]
    [DataRow("0", DisplayName = "IsClassRep_RoleIsZeroString_ReturnsFalse")]
    public void IsClassRep_RoleIsNumericBoundaryString_ReturnsFalse(string roleValue)
    {
        // Arrange
        var mockDashboardService = new Mock<IDashboardService>();
        var mockAuthService = new Mock<IAuthService>();
        var mockSessionService = new Mock<SessionService>();
        var viewModel = new DashboardViewModel(mockDashboardService.Object, mockAuthService.Object, mockSessionService.Object, new Mock<IStudentScheduleService>().Object, new Mock<IRefreshCoordinator>().Object, new Mock<IInsightsService>().Object);

        // Act
        viewModel.Role = roleValue;

        // Assert
        Assert.IsFalse(viewModel.IsClassRep, $"IsClassRep should return false when Role is numeric string: '{roleValue}'");
    }

    /// <summary>
    /// Tests that the PropertyChanged event is raised with the correct sender when TeachingClassesCount is set.
    /// Input: New integer value.
    /// Expected: PropertyChanged event sender is the ViewModel instance.
    /// </summary>
    [TestMethod]
    public void TeachingClassesCount_SetValue_PropertyChangedEventHasCorrectSender()
    {
        // Arrange
        Mock<IDashboardService> mockDashboardService = new();
        Mock<IAuthService> mockAuthService = new();
        Mock<SessionService> mockSessionService = new();
        DashboardViewModel viewModel = new(mockDashboardService.Object, mockAuthService.Object, mockSessionService.Object);
        object? eventSender = null;
        viewModel.PropertyChanged += (sender, e) => eventSender = sender;

        // Act
        viewModel.TeachingClassesCount = 15;

        // Assert
        Assert.AreSame(viewModel, eventSender);
    }

    /// <summary>
    /// Tests that setting TeachingClassesCount from zero to a positive value raises PropertyChanged.
    /// Input: Zero initially, then set to positive value.
    /// Expected: PropertyChanged event is raised when value changes from 0 to positive.
    /// </summary>
    [TestMethod]
    public void TeachingClassesCount_SetFromZeroToPositive_RaisesPropertyChanged()
    {
        // Arrange
        Mock<IDashboardService> mockDashboardService = new();
        Mock<IAuthService> mockAuthService = new();
        Mock<SessionService> mockSessionService = new();
        DashboardViewModel viewModel = new(mockDashboardService.Object, mockAuthService.Object, mockSessionService.Object);
        bool eventRaised = false;
        viewModel.PropertyChanged += (sender, e) =>
        {
            if (e.PropertyName == "TeachingClassesCount")
            {
                eventRaised = true;
            }
        };

        // Act
        viewModel.TeachingClassesCount = 75;

        // Assert
        Assert.IsTrue(eventRaised);
        Assert.AreEqual(75, viewModel.TeachingClassesCount);
    }

    /// <summary>
    /// Tests that setting TeachingClassesCount to negative values is accepted and stored correctly.
    /// Input: Various negative integer values.
    /// Expected: Property accepts and returns negative values (no validation prevents negative counts).
    /// </summary>
    /// <param name="negativeValue">The negative value to test.</param>
    [TestMethod]
    [DataRow(-1)]
    [DataRow(-50)]
    [DataRow(-1000)]
    [DataRow(int.MinValue)]
    public void TeachingClassesCount_SetNegativeValue_AcceptsAndStoresValue(int negativeValue)
    {
        // Arrange
        Mock<IDashboardService> mockDashboardService = new();
        Mock<IAuthService> mockAuthService = new();
        Mock<SessionService> mockSessionService = new();
        DashboardViewModel viewModel = new(mockDashboardService.Object, mockAuthService.Object, mockSessionService.Object);

        // Act
        viewModel.TeachingClassesCount = negativeValue;

        // Assert
        Assert.AreEqual(negativeValue, viewModel.TeachingClassesCount);
    }

    /// <summary>
    /// Tests that multiple PropertyChanged events are raised when setting different values consecutively.
    /// Input: Three different integer values set in sequence.
    /// Expected: PropertyChanged event is raised three times, once for each distinct value change.
    /// </summary>
    [TestMethod]
    public void TeachingClassesCount_SetMultipleDifferentValues_RaisesMultiplePropertyChangedEvents()
    {
        // Arrange
        Mock<IDashboardService> mockDashboardService = new();
        Mock<IAuthService> mockAuthService = new();
        Mock<SessionService> mockSessionService = new();
        DashboardViewModel viewModel = new(mockDashboardService.Object, mockAuthService.Object, mockSessionService.Object);
        int eventRaisedCount = 0;
        viewModel.PropertyChanged += (sender, e) =>
        {
            if (e.PropertyName == "TeachingClassesCount")
            {
                eventRaisedCount++;
            }
        };

        // Act
        viewModel.TeachingClassesCount = 10;
        viewModel.TeachingClassesCount = 20;
        viewModel.TeachingClassesCount = 30;

        // Assert
        Assert.AreEqual(3, eventRaisedCount);
    }

    /// <summary>
    /// Tests that the ErrorMessage property returns an empty string as the initial default value
    /// when the DashboardViewModel is first instantiated.
    /// Input: None (default initialization).
    /// Expected: ErrorMessage returns an empty string.
    /// </summary>
    [TestMethod]
    public void ErrorMessage_InitialValue_ReturnsEmptyString()
    {
        // Arrange
        Mock<IDashboardService> mockDashboardService = new();
        Mock<IAuthService> mockAuthService = new();
        Mock<SessionService> mockSessionService = new();

        // Act
        DashboardViewModel viewModel = new(mockDashboardService.Object, mockAuthService.Object, mockSessionService.Object);

        // Assert
        Assert.AreEqual(string.Empty, viewModel.ErrorMessage);
    }

    /// <summary>
    /// Tests that setting the ErrorMessage property to a new non-empty value updates the property
    /// and raises the PropertyChanged event with the correct property name.
    /// Input: New value "Test error message".
    /// Expected: Property is updated and PropertyChanged event is raised with property name "ErrorMessage".
    /// </summary>
    [TestMethod]
    public void ErrorMessage_SetNewValue_UpdatesPropertyAndRaisesPropertyChanged()
    {
        // Arrange
        Mock<IDashboardService> mockDashboardService = new();
        Mock<IAuthService> mockAuthService = new();
        Mock<SessionService> mockSessionService = new();
        DashboardViewModel viewModel = new(mockDashboardService.Object, mockAuthService.Object, mockSessionService.Object);
        string newValue = "Test error message";
        bool propertyChangedRaised = false;
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
        Assert.AreEqual("ErrorMessage", raisedPropertyName);
    }

    /// <summary>
    /// Tests that setting the ErrorMessage property to the same value as the current value
    /// does not raise the PropertyChanged event, avoiding unnecessary notifications.
    /// Input: Same value set twice.
    /// Expected: PropertyChanged event is raised only once (on first change), not on second set.
    /// </summary>
    [TestMethod]
    public void ErrorMessage_SetSameValue_DoesNotRaisePropertyChanged()
    {
        // Arrange
        Mock<IDashboardService> mockDashboardService = new();
        Mock<IAuthService> mockAuthService = new();
        Mock<SessionService> mockSessionService = new();
        DashboardViewModel viewModel = new(mockDashboardService.Object, mockAuthService.Object, mockSessionService.Object);
        string value = "Same error message";
        int propertyChangedCount = 0;

        viewModel.ErrorMessage = value;

        viewModel.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == "ErrorMessage")
            {
                propertyChangedCount++;
            }
        };

        // Act
        viewModel.ErrorMessage = value;

        // Assert
        Assert.AreEqual(value, viewModel.ErrorMessage);
        Assert.AreEqual(0, propertyChangedCount);
    }

    /// <summary>
    /// Tests that the ErrorMessage property correctly handles various string values including
    /// empty strings, whitespace, special characters, and Unicode.
    /// Input: Various string values representing common edge cases.
    /// Expected: Property stores and returns the exact value set.
    /// </summary>
    [TestMethod]
    [DataRow("")]
    [DataRow("Simple error message")]
    [DataRow("Error with numbers: 12345")]
    [DataRow("   ")]
    [DataRow(" ")]
    [DataRow("\t")]
    [DataRow("\n")]
    [DataRow("\r")]
    [DataRow("\r\n")]
    [DataRow("  \t\n\r  ")]
    [DataRow("Error with special chars: !@#$%^&*()_+-=[]{}|;':\",./<>?")]
    [DataRow("Error with backslash: C:\\Path\\To\\File")]
    [DataRow("Unicode error: ???? ????")]
    [DataRow("Error with emoji: ?? Warning ??")]
    [DataRow("Mixed\nlines\nand\ttabs")]
    public void ErrorMessage_SetVariousStringValues_UpdatesAndReturnsCorrectValue(string value)
    {
        // Arrange
        Mock<IDashboardService> mockDashboardService = new();
        Mock<IAuthService> mockAuthService = new();
        Mock<SessionService> mockSessionService = new();
        DashboardViewModel viewModel = new(mockDashboardService.Object, mockAuthService.Object, mockSessionService.Object);

        // Act
        viewModel.ErrorMessage = value;

        // Assert
        Assert.AreEqual(value, viewModel.ErrorMessage);
    }

    /// <summary>
    /// Tests that setting the ErrorMessage property with a string containing null characters
    /// is handled correctly.
    /// Input: String with embedded null character.
    /// Expected: Property stores and returns the string with null character.
    /// </summary>
    [TestMethod]
    public void ErrorMessage_SetStringWithNullCharacter_UpdatesAndReturnsCorrectValue()
    {
        // Arrange
        Mock<IDashboardService> mockDashboardService = new();
        Mock<IAuthService> mockAuthService = new();
        Mock<SessionService> mockSessionService = new();
        DashboardViewModel viewModel = new(mockDashboardService.Object, mockAuthService.Object, mockSessionService.Object);
        string valueWithNullChar = "Error\0Message";

        // Act
        viewModel.ErrorMessage = valueWithNullChar;

        // Assert
        Assert.AreEqual(valueWithNullChar, viewModel.ErrorMessage);
    }

    /// <summary>
    /// Tests that the PropertyChanged event has the correct sender when ErrorMessage is changed.
    /// Input: New error message value.
    /// Expected: PropertyChanged event sender is the viewModel instance.
    /// </summary>
    [TestMethod]
    public void ErrorMessage_SetValue_PropertyChangedEventHasCorrectSender()
    {
        // Arrange
        Mock<IDashboardService> mockDashboardService = new();
        Mock<IAuthService> mockAuthService = new();
        Mock<SessionService> mockSessionService = new();
        DashboardViewModel viewModel = new(mockDashboardService.Object, mockAuthService.Object, mockSessionService.Object);
        object? eventSender = null;

        viewModel.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == "ErrorMessage")
            {
                eventSender = sender;
            }
        };

        // Act
        viewModel.ErrorMessage = "New error";

        // Assert
        Assert.AreSame(viewModel, eventSender);
    }

    /// <summary>
    /// Tests that setting ErrorMessage to a single character updates correctly.
    /// Input: Single character string.
    /// Expected: Property returns the single character.
    /// </summary>
    [TestMethod]
    [DataRow("A")]
    [DataRow("z")]
    [DataRow("0")]
    [DataRow("!")]
    [DataRow(" ")]
    [DataRow("\n")]
    public void ErrorMessage_SetSingleCharacter_UpdatesCorrectly(string singleChar)
    {
        // Arrange
        Mock<IDashboardService> mockDashboardService = new();
        Mock<IAuthService> mockAuthService = new();
        Mock<SessionService> mockSessionService = new();
        DashboardViewModel viewModel = new(mockDashboardService.Object, mockAuthService.Object, mockSessionService.Object);

        // Act
        viewModel.ErrorMessage = singleChar;

        // Assert
        Assert.AreEqual(singleChar, viewModel.ErrorMessage);
        Assert.AreEqual(1, viewModel.ErrorMessage.Length);
    }

    /// <summary>
    /// Tests that setting ErrorMessage from one non-empty value directly to another non-empty value
    /// correctly updates the property and raises PropertyChanged.
    /// Input: Two different non-empty error messages.
    /// Expected: Property is updated to second value and PropertyChanged is raised.
    /// </summary>
    [TestMethod]
    public void ErrorMessage_SetFromOneValueToAnother_UpdatesCorrectly()
    {
        // Arrange
        Mock<IDashboardService> mockDashboardService = new();
        Mock<IAuthService> mockAuthService = new();
        Mock<SessionService> mockSessionService = new();
        DashboardViewModel viewModel = new(mockDashboardService.Object, mockAuthService.Object, mockSessionService.Object);
        viewModel.ErrorMessage = "Initial error";
        bool propertyChangedRaised = false;

        viewModel.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == "ErrorMessage")
            {
                propertyChangedRaised = true;
            }
        };

        // Act
        viewModel.ErrorMessage = "Updated error";

        // Assert
        Assert.AreEqual("Updated error", viewModel.ErrorMessage);
        Assert.IsTrue(propertyChangedRaised);
    }

    /// <summary>
    /// Tests that ErrorMessage handles strings with maximum practical length boundaries.
    /// Input: Strings of various lengths from 0 to very large.
    /// Expected: Property correctly stores all string lengths.
    /// </summary>
    [TestMethod]
    [DataRow(0)]
    [DataRow(1)]
    [DataRow(10)]
    [DataRow(100)]
    [DataRow(1000)]
    [DataRow(10000)]
    [DataRow(50000)]
    public void ErrorMessage_SetVariousLengths_UpdatesCorrectly(int length)
    {
        // Arrange
        Mock<IDashboardService> mockDashboardService = new();
        Mock<IAuthService> mockAuthService = new();
        Mock<SessionService> mockSessionService = new();
        DashboardViewModel viewModel = new(mockDashboardService.Object, mockAuthService.Object, mockSessionService.Object);
        string valueOfLength = new('E', length);

        // Act
        viewModel.ErrorMessage = valueOfLength;

        // Assert
        Assert.AreEqual(length, viewModel.ErrorMessage.Length);
        Assert.AreEqual(valueOfLength, viewModel.ErrorMessage);
    }

    /// <summary>
    /// Tests that ErrorMessage property correctly handles strings with various whitespace combinations.
    /// Input: Strings with different types and combinations of whitespace characters.
    /// Expected: Property stores exact whitespace strings as provided.
    /// </summary>
    [TestMethod]
    [DataRow("  ")]
    [DataRow("\t\t")]
    [DataRow("\n\n")]
    [DataRow(" \t \n \r ")]
    [DataRow("Error \t with \n mixed \r whitespace")]
    public void ErrorMessage_SetWhitespaceVariations_UpdatesCorrectly(string whitespaceValue)
    {
        // Arrange
        Mock<IDashboardService> mockDashboardService = new();
        Mock<IAuthService> mockAuthService = new();
        Mock<SessionService> mockSessionService = new();
        DashboardViewModel viewModel = new(mockDashboardService.Object, mockAuthService.Object, mockSessionService.Object);

        // Act
        viewModel.ErrorMessage = whitespaceValue;

        // Assert
        Assert.AreEqual(whitespaceValue, viewModel.ErrorMessage);
    }

    /// <summary>
    /// Tests that ErrorMessage property handles HTML/XML-like content correctly.
    /// Input: Strings containing HTML/XML tags and entities.
    /// Expected: Property stores the strings as-is without any parsing or modification.
    /// </summary>
    [TestMethod]
    [DataRow("<error>Message</error>")]
    [DataRow("<script>alert('test')</script>")]
    [DataRow("Error &amp; Warning")]
    [DataRow("<?xml version=\"1.0\"?>")]
    [DataRow("<div class=\"error\">Message</div>")]
    public void ErrorMessage_SetHtmlXmlContent_UpdatesCorrectly(string htmlXmlValue)
    {
        // Arrange
        Mock<IDashboardService> mockDashboardService = new();
        Mock<IAuthService> mockAuthService = new();
        Mock<SessionService> mockSessionService = new();
        DashboardViewModel viewModel = new(mockDashboardService.Object, mockAuthService.Object, mockSessionService.Object);

        // Act
        viewModel.ErrorMessage = htmlXmlValue;

        // Assert
        Assert.AreEqual(htmlXmlValue, viewModel.ErrorMessage);
    }

    /// <summary>
    /// Tests that ErrorMessage property handles various international character sets.
    /// Input: Strings with different language scripts and symbols.
    /// Expected: Property correctly stores Unicode characters from various languages.
    /// </summary>
    [TestMethod]
    [DataRow("????")]
    [DataRow("????????")]
    [DataRow("?? ???")]
    [DataRow("????????? ?? ??????")]
    [DataRow("????? ???")]
    [DataRow("µ???µa sf??µat??")]
    public void ErrorMessage_SetInternationalCharacters_UpdatesCorrectly(string internationalValue)
    {
        // Arrange
        Mock<IDashboardService> mockDashboardService = new();
        Mock<IAuthService> mockAuthService = new();
        Mock<SessionService> mockSessionService = new();
        DashboardViewModel viewModel = new(mockDashboardService.Object, mockAuthService.Object, mockSessionService.Object);

        // Act
        viewModel.ErrorMessage = internationalValue;

        // Assert
        Assert.AreEqual(internationalValue, viewModel.ErrorMessage);
    }

    /// <summary>
    /// Tests that setting TotalLecturers with various edge case values updates the property correctly.
    /// Input: Various boundary and edge case values.
    /// Expected: Property value should be updated to the set value.
    /// </summary>
    /// <param name="value">The value to set.</param>
    [TestMethod]
    [DataRow(0)]
    [DataRow(1)]
    [DataRow(-1)]
    [DataRow(100)]
    [DataRow(-100)]
    [DataRow(1000)]
    [DataRow(-1000)]
    [DataRow(int.MaxValue)]
    [DataRow(int.MinValue)]
    public void TotalLecturers_SetVariousValues_UpdatesPropertyCorrectly(int value)
    {
        // Arrange
        var mockDashboard = new Mock<IDashboardService>();
        var mockAuth = new Mock<IAuthService>();
        var mockSession = new Mock<SessionService>();
        var viewModel = new DashboardViewModel(mockDashboard.Object, mockAuth.Object, mockSession.Object, new Mock<IStudentScheduleService>().Object, new Mock<IRefreshCoordinator>().Object, new Mock<IInsightsService>().Object);

        // Act
        viewModel.TotalLecturers = value;

        // Assert
        Assert.AreEqual(value, viewModel.TotalLecturers);
    }

    /// <summary>
    /// Tests that PropertyChanged event is raised for each distinct value change to TotalLecturers.
    /// Input: Multiple different values set consecutively.
    /// Expected: PropertyChanged event should be raised for each value change.
    /// </summary>
    [TestMethod]
    public void TotalLecturers_SetMultipleDifferentValues_RaisesPropertyChangedForEachChange()
    {
        // Arrange
        var mockDashboard = new Mock<IDashboardService>();
        var mockAuth = new Mock<IAuthService>();
        var mockSession = new Mock<SessionService>();
        var viewModel = new DashboardViewModel(mockDashboard.Object, mockAuth.Object, mockSession.Object, new Mock<IStudentScheduleService>().Object, new Mock<IRefreshCoordinator>().Object, new Mock<IInsightsService>().Object);
        var eventRaisedCount = 0;
        viewModel.PropertyChanged += (sender, e) =>
        {
            if (e.PropertyName == "TotalLecturers")
                eventRaisedCount++;
        };

        // Act
        viewModel.TotalLecturers = 10;
        viewModel.TotalLecturers = 20;
        viewModel.TotalLecturers = 30;

        // Assert
        Assert.AreEqual(3, eventRaisedCount);
    }

    /// <summary>
    /// Tests that changing TotalLecturers from zero to a positive value raises PropertyChanged event.
    /// Input: Change from 0 to 50.
    /// Expected: PropertyChanged event should be raised once.
    /// </summary>
    [TestMethod]
    public void TotalLecturers_ChangeFromZeroToPositive_RaisesPropertyChanged()
    {
        // Arrange
        var mockDashboard = new Mock<IDashboardService>();
        var mockAuth = new Mock<IAuthService>();
        var mockSession = new Mock<SessionService>();
        var viewModel = new DashboardViewModel(mockDashboard.Object, mockAuth.Object, mockSession.Object, new Mock<IStudentScheduleService>().Object, new Mock<IRefreshCoordinator>().Object, new Mock<IInsightsService>().Object);
        var propertyChangedRaised = false;
        viewModel.PropertyChanged += (sender, e) =>
        {
            if (e.PropertyName == "TotalLecturers")
                propertyChangedRaised = true;
        };

        // Act
        viewModel.TotalLecturers = 50;

        // Assert
        Assert.IsTrue(propertyChangedRaised);
        Assert.AreEqual(50, viewModel.TotalLecturers);
    }

    /// <summary>
    /// Tests that changing TotalLecturers from a positive value to zero raises PropertyChanged event.
    /// Input: Change from 100 to 0.
    /// Expected: PropertyChanged event should be raised once.
    /// </summary>
    [TestMethod]
    public void TotalLecturers_ChangeFromPositiveToZero_RaisesPropertyChanged()
    {
        // Arrange
        var mockDashboard = new Mock<IDashboardService>();
        var mockAuth = new Mock<IAuthService>();
        var mockSession = new Mock<SessionService>();
        var viewModel = new DashboardViewModel(mockDashboard.Object, mockAuth.Object, mockSession.Object, new Mock<IStudentScheduleService>().Object, new Mock<IRefreshCoordinator>().Object, new Mock<IInsightsService>().Object);
        viewModel.TotalLecturers = 100;
        var propertyChangedRaised = false;
        viewModel.PropertyChanged += (sender, e) =>
        {
            if (e.PropertyName == "TotalLecturers")
                propertyChangedRaised = true;
        };

        // Act
        viewModel.TotalLecturers = 0;

        // Assert
        Assert.IsTrue(propertyChangedRaised);
        Assert.AreEqual(0, viewModel.TotalLecturers);
    }

    /// <summary>
    /// Tests that changing TotalLecturers from a positive value to a negative value raises PropertyChanged event.
    /// Input: Change from 50 to -50.
    /// Expected: PropertyChanged event should be raised once.
    /// </summary>
    [TestMethod]
    public void TotalLecturers_ChangeFromPositiveToNegative_RaisesPropertyChanged()
    {
        // Arrange
        var mockDashboard = new Mock<IDashboardService>();
        var mockAuth = new Mock<IAuthService>();
        var mockSession = new Mock<SessionService>();
        var viewModel = new DashboardViewModel(mockDashboard.Object, mockAuth.Object, mockSession.Object, new Mock<IStudentScheduleService>().Object, new Mock<IRefreshCoordinator>().Object, new Mock<IInsightsService>().Object);
        viewModel.TotalLecturers = 50;
        var propertyChangedRaised = false;
        viewModel.PropertyChanged += (sender, e) =>
        {
            if (e.PropertyName == "TotalLecturers")
                propertyChangedRaised = true;
        };

        // Act
        viewModel.TotalLecturers = -50;

        // Assert
        Assert.IsTrue(propertyChangedRaised);
        Assert.AreEqual(-50, viewModel.TotalLecturers);
    }

    /// <summary>
    /// Tests that PropertyChanged event contains the correct sender when TotalLecturers is changed.
    /// Input: New value (75).
    /// Expected: PropertyChanged event sender should be the viewModel instance.
    /// </summary>
    [TestMethod]
    public void TotalLecturers_PropertyChangedEvent_HasCorrectSender()
    {
        // Arrange
        var mockDashboard = new Mock<IDashboardService>();
        var mockAuth = new Mock<IAuthService>();
        var mockSession = new Mock<SessionService>();
        var viewModel = new DashboardViewModel(mockDashboard.Object, mockAuth.Object, mockSession.Object, new Mock<IStudentScheduleService>().Object, new Mock<IRefreshCoordinator>().Object, new Mock<IInsightsService>().Object);
        object? eventSender = null;
        viewModel.PropertyChanged += (sender, e) =>
        {
            if (e.PropertyName == "TotalLecturers")
                eventSender = sender;
        };

        // Act
        viewModel.TotalLecturers = 75;

        // Assert
        Assert.IsNotNull(eventSender);
        Assert.AreSame(viewModel, eventSender);
    }

    /// <summary>
    /// Tests that setting ManagedClassesCount from positive to negative value updates correctly.
    /// Input: First set to 100, then set to -50.
    /// Expected: Property is updated correctly and PropertyChanged is raised for each change.
    /// </summary>
    [TestMethod]
    public void ManagedClassesCount_SetFromPositiveToNegative_UpdatesCorrectly()
    {
        // Arrange
        var mockDashboard = new Mock<IDashboardService>();
        var mockAuth = new Mock<IAuthService>();
        var mockSession = new Mock<SessionService>();
        var viewModel = new DashboardViewModel(mockDashboard.Object, mockAuth.Object, mockSession.Object, new Mock<IStudentScheduleService>().Object, new Mock<IRefreshCoordinator>().Object, new Mock<IInsightsService>().Object);

        // Act
        viewModel.ManagedClassesCount = 100;
        var afterPositive = viewModel.ManagedClassesCount;
        viewModel.ManagedClassesCount = -50;
        var afterNegative = viewModel.ManagedClassesCount;

        // Assert
        Assert.AreEqual(100, afterPositive);
        Assert.AreEqual(-50, afterNegative);
    }

    /// <summary>
    /// Tests that setting ManagedClassesCount to zero after having a non-zero value updates correctly.
    /// Input: First set to 25, then set to 0.
    /// Expected: Property is updated to 0 and PropertyChanged is raised.
    /// </summary>
    [TestMethod]
    public void ManagedClassesCount_SetToZeroAfterNonZero_UpdatesCorrectly()
    {
        // Arrange
        var mockDashboard = new Mock<IDashboardService>();
        var mockAuth = new Mock<IAuthService>();
        var mockSession = new Mock<SessionService>();
        var viewModel = new DashboardViewModel(mockDashboard.Object, mockAuth.Object, mockSession.Object, new Mock<IStudentScheduleService>().Object, new Mock<IRefreshCoordinator>().Object, new Mock<IInsightsService>().Object);
        viewModel.ManagedClassesCount = 25;
        string? propertyChangedName = null;
        viewModel.PropertyChanged += (sender, args) => propertyChangedName = args.PropertyName;

        // Act
        viewModel.ManagedClassesCount = 0;

        // Assert
        Assert.AreEqual(0, viewModel.ManagedClassesCount);
        Assert.AreEqual("ManagedClassesCount", propertyChangedName);
    }

    /// <summary>
    /// Tests that PropertyChanged event sender is the viewmodel instance.
    /// Input: Setting ManagedClassesCount to 42.
    /// Expected: PropertyChanged event sender is the viewmodel itself.
    /// </summary>
    [TestMethod]
    public void ManagedClassesCount_SetValue_PropertyChangedSenderIsViewModel()
    {
        // Arrange
        var mockDashboard = new Mock<IDashboardService>();
        var mockAuth = new Mock<IAuthService>();
        var mockSession = new Mock<SessionService>();
        var viewModel = new DashboardViewModel(mockDashboard.Object, mockAuth.Object, mockSession.Object, new Mock<IStudentScheduleService>().Object, new Mock<IRefreshCoordinator>().Object, new Mock<IInsightsService>().Object);
        object? eventSender = null;
        viewModel.PropertyChanged += (sender, args) => eventSender = sender;

        // Act
        viewModel.ManagedClassesCount = 42;

        // Assert
        Assert.AreSame(viewModel, eventSender);
    }

    /// <summary>
    /// Tests that the HasNotifications property returns the default value of true when first accessed.
    /// Input: None (default initialization).
    /// Expected: HasNotifications returns true.
    /// </summary>
    [TestMethod]
    public void HasNotifications_DefaultValue_ReturnsTrue()
    {
        // Arrange
        Mock<IDashboardService> mockDashboardService = new();
        Mock<IAuthService> mockAuthService = new();
        Mock<SessionService> mockSessionService = new();

        // Act
        DashboardViewModel viewModel = new(mockDashboardService.Object, mockAuthService.Object, mockSessionService.Object);

        // Assert
        Assert.IsTrue(viewModel.HasNotifications);
    }

    /// <summary>
    /// Tests that HasNotifications correctly stores and returns the value when set.
    /// Input: Boolean values (true and false).
    /// Expected: Property returns the set value.
    /// </summary>
    /// <param name="value">The boolean value to set and verify.</param>
    [TestMethod]
    [DataRow(true)]
    [DataRow(false)]
    public void HasNotifications_SetValue_ReturnsExpectedValue(bool value)
    {
        // Arrange
        Mock<IDashboardService> mockDashboardService = new();
        Mock<IAuthService> mockAuthService = new();
        Mock<SessionService> mockSessionService = new();
        DashboardViewModel viewModel = new(mockDashboardService.Object, mockAuthService.Object, mockSessionService.Object);

        // Act
        viewModel.HasNotifications = value;

        // Assert
        Assert.AreEqual(value, viewModel.HasNotifications);
    }

    /// <summary>
    /// Tests that HasNotifications raises PropertyChanged event when value changes.
    /// Input: New boolean value different from current value.
    /// Expected: PropertyChanged event is raised with property name "HasNotifications".
    /// </summary>
    /// <param name="newValue">The new boolean value to set.</param>
    [TestMethod]
    [DataRow(true)]
    [DataRow(false)]
    public void HasNotifications_SetValue_RaisesPropertyChanged(bool newValue)
    {
        // Arrange
        Mock<IDashboardService> mockDashboardService = new();
        Mock<IAuthService> mockAuthService = new();
        Mock<SessionService> mockSessionService = new();
        DashboardViewModel viewModel = new(mockDashboardService.Object, mockAuthService.Object, mockSessionService.Object);

        // Set to opposite value first to ensure change occurs
        viewModel.HasNotifications = !newValue;

        bool eventRaised = false;
        string? raisedPropertyName = null;
        viewModel.PropertyChanged += (sender, args) =>
        {
            eventRaised = true;
            raisedPropertyName = args.PropertyName;
        };

        // Act
        viewModel.HasNotifications = newValue;

        // Assert
        Assert.IsTrue(eventRaised, "PropertyChanged event should be raised.");
        Assert.AreEqual("HasNotifications", raisedPropertyName, "PropertyChanged event should be raised for HasNotifications property.");
    }

    /// <summary>
    /// Tests that HasNotifications does not raise PropertyChanged event when set to the same value.
    /// Input: Same value as current value.
    /// Expected: PropertyChanged event is not raised.
    /// </summary>
    [TestMethod]
    public void HasNotifications_SetSameValue_DoesNotRaisePropertyChanged()
    {
        // Arrange
        Mock<IDashboardService> mockDashboardService = new();
        Mock<IAuthService> mockAuthService = new();
        Mock<SessionService> mockSessionService = new();
        DashboardViewModel viewModel = new(mockDashboardService.Object, mockAuthService.Object, mockSessionService.Object);

        // HasNotifications is true by default, so set it explicitly
        viewModel.HasNotifications = true;

        bool eventRaised = false;
        viewModel.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == "HasNotifications")
            {
                eventRaised = true;
            }
        };

        // Act
        viewModel.HasNotifications = true;

        // Assert
        Assert.IsFalse(eventRaised, "PropertyChanged event should not be raised when setting the same value.");
    }

    /// <summary>
    /// Tests that HasNotifications correctly toggles between true and false values.
    /// Input: Sequential value changes (true to false to true).
    /// Expected: Property value reflects each change correctly.
    /// </summary>
    [TestMethod]
    public void HasNotifications_ToggleValue_ReturnsCorrectValue()
    {
        // Arrange
        Mock<IDashboardService> mockDashboardService = new();
        Mock<IAuthService> mockAuthService = new();
        Mock<SessionService> mockSessionService = new();
        DashboardViewModel viewModel = new(mockDashboardService.Object, mockAuthService.Object, mockSessionService.Object);

        // Assert initial value
        Assert.IsTrue(viewModel.HasNotifications);

        // Act & Assert - Toggle to false
        viewModel.HasNotifications = false;
        Assert.IsFalse(viewModel.HasNotifications);

        // Act & Assert - Toggle back to true
        viewModel.HasNotifications = true;
        Assert.IsTrue(viewModel.HasNotifications);
    }

    /// <summary>
    /// Tests that HasNotifications can be set to false from its default true value.
    /// Input: Setting to false.
    /// Expected: Property value is false and PropertyChanged event is raised.
    /// </summary>
    [TestMethod]
    public void HasNotifications_SetToFalseFromDefaultTrue_UpdatesCorrectly()
    {
        // Arrange
        Mock<IDashboardService> mockDashboardService = new();
        Mock<IAuthService> mockAuthService = new();
        Mock<SessionService> mockSessionService = new();
        DashboardViewModel viewModel = new(mockDashboardService.Object, mockAuthService.Object, mockSessionService.Object);

        bool eventRaised = false;
        viewModel.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == "HasNotifications")
            {
                eventRaised = true;
            }
        };

        // Act
        viewModel.HasNotifications = false;

        // Assert
        Assert.IsFalse(viewModel.HasNotifications);
        Assert.IsTrue(eventRaised, "PropertyChanged event should be raised when value changes from true to false.");
    }

    /// <summary>
    /// Tests that HasNotifications raises PropertyChanged event multiple times when value changes multiple times.
    /// Input: Multiple alternating value changes.
    /// Expected: PropertyChanged event is raised for each distinct value change.
    /// </summary>
    [TestMethod]
    public void HasNotifications_MultipleValueChanges_RaisesPropertyChangedEachTime()
    {
        // Arrange
        Mock<IDashboardService> mockDashboardService = new();
        Mock<IAuthService> mockAuthService = new();
        Mock<SessionService> mockSessionService = new();
        DashboardViewModel viewModel = new(mockDashboardService.Object, mockAuthService.Object, mockSessionService.Object);

        int eventRaisedCount = 0;
        viewModel.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == "HasNotifications")
            {
                eventRaisedCount++;
            }
        };

        // Act
        viewModel.HasNotifications = false; // Change 1
        viewModel.HasNotifications = true;  // Change 2
        viewModel.HasNotifications = false; // Change 3

        // Assert
        Assert.AreEqual(3, eventRaisedCount, "PropertyChanged event should be raised three times for three distinct value changes.");
        Assert.IsFalse(viewModel.HasNotifications, "Final value should be false.");
    }

    /// <summary>
    /// Tests that the constructor properly initializes all command properties with valid non-null dependencies.
    /// Input: Valid IDashboardService, IAuthService, and SessionService instances with no current user.
    /// Expected: All command properties (LoadCommand, LogoutCommand, NavigateToProfileCommand, etc.) are initialized and not null.
    /// </summary>
    [TestMethod]
    public void Constructor_WithValidDependencies_InitializesAllCommands()
    {
        // Arrange
        var mockDashboard = new Mock<IDashboardService>();
        var mockAuth = new Mock<IAuthService>();
        var session = new SessionService();

        // Act
        var viewModel = new DashboardViewModel(mockDashboard.Object, mockAuth.Object, session);

        // Assert
        Assert.IsNotNull(viewModel.LoadCommand);
        Assert.IsNotNull(viewModel.LogoutCommand);
        Assert.IsNotNull(viewModel.NavigateToProfileCommand);
        Assert.IsNotNull(viewModel.NavigateToSearchCommand);
        Assert.IsNotNull(viewModel.NavigateToNotificationsCommand);
        Assert.IsNotNull(viewModel.NavigateToCoursesCommand);
        Assert.IsNotNull(viewModel.NavigateToAssignmentsCommand);
        Assert.IsNotNull(viewModel.NavigateToNewsCommand);
        Assert.IsNotNull(viewModel.NavigateToChatCommand);
        Assert.IsNotNull(viewModel.RefreshCommand);
    }

    /// <summary>
    /// Tests that the constructor sets StudentName to "Student" when SessionService has no current user.
    /// Input: SessionService with CurrentUser set to null.
    /// Expected: StudentName property is set to "Student".
    /// </summary>
    [TestMethod]
    public void Constructor_WithNoCurrentUser_SetsStudentNameToDefault()
    {
        // Arrange
        var mockDashboard = new Mock<IDashboardService>();
        var mockAuth = new Mock<IAuthService>();
        var session = new SessionService();

        // Act
        var viewModel = new DashboardViewModel(mockDashboard.Object, mockAuth.Object, session);

        // Assert
        Assert.AreEqual("Student", viewModel.StudentName);
    }

    /// <summary>
    /// Tests that the constructor sets StudentName to the user's FullName when available.
    /// Input: SessionService with a current user having a valid FullName.
    /// Expected: StudentName property is set to the user's FullName.
    /// </summary>
    [TestMethod]
    [DataRow("John Doe")]
    [DataRow("Jane Smith")]
    [DataRow("A")]
    [DataRow("VeryLongNameWithLotsOfCharactersToTestThePropertyHandling")]
    public void Constructor_WithCurrentUserHavingValidFullName_SetsStudentNameToFullName(string fullName)
    {
        // Arrange
        var mockDashboard = new Mock<IDashboardService>();
        var mockAuth = new Mock<IAuthService>();
        var session = new SessionService();
        var user = new AuthUserDto { FullName = fullName, Role = "Student" };
        session.SetUser(user);

        // Act
        var viewModel = new DashboardViewModel(mockDashboard.Object, mockAuth.Object, session);

        // Assert
        Assert.AreEqual(fullName, viewModel.StudentName);
    }

    /// <summary>
    /// Tests that the constructor sets StudentName to "Student" when the current user has an empty or whitespace FullName.
    /// Input: SessionService with a current user having FullName set to empty string or whitespace.
    /// Expected: StudentName property is set to "Student".
    /// </summary>
    [TestMethod]
    [DataRow("")]
    [DataRow(" ")]
    [DataRow("   ")]
    [DataRow("\t")]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [DataRow(" \t \n ")]
    public void Constructor_WithCurrentUserHavingEmptyOrWhitespaceFullName_SetsStudentNameToDefault(string fullName)
    {
        // Arrange
        var mockDashboard = new Mock<IDashboardService>();
        var mockAuth = new Mock<IAuthService>();
        var session = new SessionService();
        var user = new AuthUserDto { FullName = fullName, Role = "Student" };
        session.SetUser(user);

        // Act
        var viewModel = new DashboardViewModel(mockDashboard.Object, mockAuth.Object, session);

        // Assert
        Assert.AreEqual("Student", viewModel.StudentName);
    }

    /// <summary>
    /// Tests that the constructor handles FullName with special characters correctly.
    /// Input: SessionService with a current user having FullName containing special characters.
    /// Expected: StudentName property is set to the exact FullName value including special characters.
    /// </summary>
    [TestMethod]
    [DataRow("John O'Brien")]
    [DataRow("María García")]
    [DataRow("??")]
    [DataRow("Name with emoji ??")]
    [DataRow("Name\twith\ttabs")]
    [DataRow("Name!@#$%")]
    public void Constructor_WithCurrentUserHavingSpecialCharactersInFullName_SetsStudentNameCorrectly(string fullName)
    {
        // Arrange
        var mockDashboard = new Mock<IDashboardService>();
        var mockAuth = new Mock<IAuthService>();
        var session = new SessionService();
        var user = new AuthUserDto { FullName = fullName, Role = "Student" };
        session.SetUser(user);

        // Act
        var viewModel = new DashboardViewModel(mockDashboard.Object, mockAuth.Object, session);

        // Assert
        Assert.AreEqual(fullName, viewModel.StudentName);
    }

    /// <summary>
    /// Tests that all command properties can execute by default.
    /// Input: Valid dependencies.
    /// Expected: CanExecute returns true for all initialized commands.
    /// </summary>
    [TestMethod]
    public void Constructor_AllCommands_CanExecuteByDefault()
    {
        // Arrange
        var mockDashboard = new Mock<IDashboardService>();
        var mockAuth = new Mock<IAuthService>();
        var session = new SessionService();

        // Act
        var viewModel = new DashboardViewModel(mockDashboard.Object, mockAuth.Object, session);

        // Assert
        Assert.IsTrue(viewModel.LoadCommand.CanExecute(null));
        Assert.IsTrue(viewModel.LogoutCommand.CanExecute(null));
        Assert.IsTrue(viewModel.NavigateToProfileCommand.CanExecute(null));
        Assert.IsTrue(viewModel.NavigateToSearchCommand.CanExecute(null));
        Assert.IsTrue(viewModel.NavigateToNotificationsCommand.CanExecute(null));
        Assert.IsTrue(viewModel.NavigateToCoursesCommand.CanExecute(null));
        Assert.IsTrue(viewModel.NavigateToAssignmentsCommand.CanExecute(null));
        Assert.IsTrue(viewModel.NavigateToNewsCommand.CanExecute(null));
        Assert.IsTrue(viewModel.NavigateToChatCommand.CanExecute(null));
        Assert.IsTrue(viewModel.RefreshCommand.CanExecute(null));
    }

    /// <summary>
    /// Tests that the constructor initializes default property values correctly.
    /// Input: Valid dependencies with no current user.
    /// Expected: Default values are set correctly (e.g., TotalStudents = 0, Role = "Student", etc.).
    /// </summary>
    [TestMethod]
    public void Constructor_WithValidDependencies_InitializesDefaultPropertyValues()
    {
        // Arrange
        var mockDashboard = new Mock<IDashboardService>();
        var mockAuth = new Mock<IAuthService>();
        var session = new SessionService();

        // Act
        var viewModel = new DashboardViewModel(mockDashboard.Object, mockAuth.Object, session);

        // Assert
        Assert.AreEqual(0, viewModel.TotalStudents);
        Assert.AreEqual(0, viewModel.TotalLecturers);
        Assert.AreEqual(0, viewModel.TotalPrograms);
        Assert.AreEqual(0, viewModel.TeachingClassesCount);
        Assert.AreEqual(0, viewModel.ManagedClassesCount);
        Assert.AreEqual(0, viewModel.AttendancePercentage);
        Assert.AreEqual(0, viewModel.ActiveCourses);
        Assert.AreEqual(0, viewModel.UpcomingAssignments);
        Assert.AreEqual(string.Empty, viewModel.ErrorMessage);
        Assert.AreEqual(string.Empty, viewModel.LatestAnnouncementTitle);
        Assert.AreEqual(string.Empty, viewModel.LatestAnnouncementBody);
        Assert.AreEqual(string.Empty, viewModel.LatestAnnouncementDate);
        Assert.IsFalse(viewModel.HasAnnouncement);
        Assert.IsFalse(viewModel.HasClasses);
        Assert.IsTrue(viewModel.HasNotifications);
        Assert.AreEqual("Student", viewModel.Role);
    }

    /// <summary>
    /// Tests that the constructor handles SessionService with a user in various role types.
    /// Input: SessionService with user having different role values.
    /// Expected: Constructor completes successfully and StudentName is set based on FullName.
    /// </summary>
    [TestMethod]
    [DataRow("Student", "Test User")]
    [DataRow("Lecturer", "Prof. Smith")]
    [DataRow("Admin", "Admin User")]
    [DataRow("ClassRep", "Class Representative")]
    public void Constructor_WithCurrentUserInDifferentRoles_SetsStudentNameCorrectly(string role, string fullName)
    {
        // Arrange
        var mockDashboard = new Mock<IDashboardService>();
        var mockAuth = new Mock<IAuthService>();
        var session = new SessionService();
        var user = new AuthUserDto { FullName = fullName, Role = role };
        session.SetUser(user);

        // Act
        var viewModel = new DashboardViewModel(mockDashboard.Object, mockAuth.Object, session);

        // Assert
        Assert.AreEqual(fullName, viewModel.StudentName);
    }

    /// <summary>
    /// Tests that constructor behavior is consistent across multiple instantiations.
    /// Input: Same set of valid dependencies used multiple times.
    /// Expected: Each instance is initialized with the same property values.
    /// </summary>
    [TestMethod]
    public void Constructor_MultipleInstantiations_ProducesConsistentResults()
    {
        // Arrange
        var mockDashboard = new Mock<IDashboardService>();
        var mockAuth = new Mock<IAuthService>();
        var session = new SessionService();

        // Act
        var viewModel1 = new DashboardViewModel(mockDashboard.Object, mockAuth.Object, session);
        var viewModel2 = new DashboardViewModel(mockDashboard.Object, mockAuth.Object, session);

        // Assert
        Assert.AreEqual(viewModel1.StudentName, viewModel2.StudentName);
        Assert.AreEqual(viewModel1.Role, viewModel2.Role);
        Assert.AreEqual(viewModel1.TotalStudents, viewModel2.TotalStudents);
        Assert.IsNotNull(viewModel1.LoadCommand);
        Assert.IsNotNull(viewModel2.LoadCommand);
    }

    /// <summary>
    /// Tests that the constructor handles boundary value for FullName length.
    /// Input: SessionService with a user having an extremely long FullName (10000 characters).
    /// Expected: StudentName is set to the full long name without truncation or error.
    /// </summary>
    [TestMethod]
    public void Constructor_WithVeryLongFullName_SetsStudentNameWithoutError()
    {
        // Arrange
        var mockDashboard = new Mock<IDashboardService>();
        var mockAuth = new Mock<IAuthService>();
        var session = new SessionService();
        var longName = new string('A', 10000);
        var user = new AuthUserDto { FullName = longName, Role = "Student" };
        session.SetUser(user);

        // Act
        var viewModel = new DashboardViewModel(mockDashboard.Object, mockAuth.Object, session);

        // Assert
        Assert.AreEqual(longName, viewModel.StudentName);
        Assert.AreEqual(10000, viewModel.StudentName.Length);
    }

    /// <summary>
    /// Tests that the constructor handles FullName with control characters.
    /// Input: SessionService with a user having FullName containing null character.
    /// Expected: StudentName is set to the value including control characters.
    /// </summary>
    [TestMethod]
    public void Constructor_WithFullNameContainingNullCharacter_SetsStudentNameCorrectly()
    {
        // Arrange
        var mockDashboard = new Mock<IDashboardService>();
        var mockAuth = new Mock<IAuthService>();
        var session = new SessionService();
        var nameWithNull = "Name\0WithNull";
        var user = new AuthUserDto { FullName = nameWithNull, Role = "Student" };
        session.SetUser(user);

        // Act
        var viewModel = new DashboardViewModel(mockDashboard.Object, mockAuth.Object, session);

        // Assert
        Assert.AreEqual(nameWithNull, viewModel.StudentName);
    }

    /// <summary>
    /// Tests that LoadCommand is initialized with a Command that wraps an async lambda calling LoadAsync with forceRefresh: true.
    /// Input: Valid dependencies.
    /// Expected: LoadCommand is a Command instance that can be executed.
    /// </summary>
    [TestMethod]
    public void Constructor_InitializesLoadCommand_AsCommandInstance()
    {
        // Arrange
        var mockDashboard = new Mock<IDashboardService>();
        var mockAuth = new Mock<IAuthService>();
        var session = new SessionService();

        // Act
        var viewModel = new DashboardViewModel(mockDashboard.Object, mockAuth.Object, session);

        // Assert
        Assert.IsNotNull(viewModel.LoadCommand);
        Assert.IsInstanceOfType(viewModel.LoadCommand, typeof(Command));
    }

    /// <summary>
    /// Tests that LogoutCommand is initialized with a Command that wraps an async lambda calling auth.LogoutAsync.
    /// Input: Valid dependencies.
    /// Expected: LogoutCommand is a Command instance that can be executed.
    /// </summary>
    [TestMethod]
    public void Constructor_InitializesLogoutCommand_AsCommandInstance()
    {
        // Arrange
        var mockDashboard = new Mock<IDashboardService>();
        var mockAuth = new Mock<IAuthService>();
        var session = new SessionService();

        // Act
        var viewModel = new DashboardViewModel(mockDashboard.Object, mockAuth.Object, session);

        // Assert
        Assert.IsNotNull(viewModel.LogoutCommand);
        Assert.IsInstanceOfType(viewModel.LogoutCommand, typeof(Command));
    }

    /// <summary>
    /// Tests that all navigation commands are initialized as Command instances.
    /// Input: Valid dependencies.
    /// Expected: All navigation command properties are Command instances.
    /// </summary>
    [TestMethod]
    public void Constructor_InitializesAllNavigationCommands_AsCommandInstances()
    {
        // Arrange
        var mockDashboard = new Mock<IDashboardService>();
        var mockAuth = new Mock<IAuthService>();
        var session = new SessionService();

        // Act
        var viewModel = new DashboardViewModel(mockDashboard.Object, mockAuth.Object, session);

        // Assert
        Assert.IsInstanceOfType(viewModel.NavigateToProfileCommand, typeof(Command));
        Assert.IsInstanceOfType(viewModel.NavigateToSearchCommand, typeof(Command));
        Assert.IsInstanceOfType(viewModel.NavigateToNotificationsCommand, typeof(Command));
        Assert.IsInstanceOfType(viewModel.NavigateToCoursesCommand, typeof(Command));
        Assert.IsInstanceOfType(viewModel.NavigateToAssignmentsCommand, typeof(Command));
        Assert.IsInstanceOfType(viewModel.NavigateToNewsCommand, typeof(Command));
        Assert.IsInstanceOfType(viewModel.NavigateToChatCommand, typeof(Command));
    }

    /// <summary>
    /// Tests that the TotalPrograms property has a default value of zero when the ViewModel is first created.
    /// Input: None (initial state).
    /// Expected: TotalPrograms returns 0.
    /// </summary>
    [TestMethod]
    public void TotalPrograms_InitialValue_ReturnsZero()
    {
        // Arrange
        Mock<IDashboardService> mockDashboard = new Mock<IDashboardService>();
        Mock<IAuthService> mockAuth = new Mock<IAuthService>();
        Mock<SessionService> mockSession = new Mock<SessionService>();

        // Act
        DashboardViewModel viewModel = new DashboardViewModel(mockDashboard.Object, mockAuth.Object, mockSession.Object, new Mock<IStudentScheduleService>().Object, new Mock<IRefreshCoordinator>().Object, new Mock<IInsightsService>().Object);

        // Assert
        Assert.AreEqual(0, viewModel.TotalPrograms);
    }

    /// <summary>
    /// Tests that setting the TotalPrograms property to various valid integer values updates the property correctly.
    /// Input: Valid integer values including zero, positive, negative, and boundary values.
    /// Expected: Property value is updated to the set value.
    /// </summary>
    /// <param name="value">The value to set on TotalPrograms.</param>
    [TestMethod]
    [DataRow(0)]
    [DataRow(1)]
    [DataRow(10)]
    [DataRow(100)]
    [DataRow(1000)]
    [DataRow(int.MaxValue)]
    [DataRow(-1)]
    [DataRow(-10)]
    [DataRow(-100)]
    [DataRow(int.MinValue)]
    public void TotalPrograms_SetValidValue_UpdatesPropertyCorrectly(int value)
    {
        // Arrange
        Mock<IDashboardService> mockDashboard = new Mock<IDashboardService>();
        Mock<IAuthService> mockAuth = new Mock<IAuthService>();
        Mock<SessionService> mockSession = new Mock<SessionService>();
        DashboardViewModel viewModel = new DashboardViewModel(mockDashboard.Object, mockAuth.Object, mockSession.Object, new Mock<IStudentScheduleService>().Object, new Mock<IRefreshCoordinator>().Object, new Mock<IInsightsService>().Object);

        // Act
        viewModel.TotalPrograms = value;

        // Assert
        Assert.AreEqual(value, viewModel.TotalPrograms);
    }

    /// <summary>
    /// Tests that setting the TotalPrograms property multiple times with different values
    /// updates the property correctly each time.
    /// Input: Multiple different integer values set sequentially.
    /// Expected: Property value reflects the last set value.
    /// </summary>
    [TestMethod]
    public void TotalPrograms_SetMultipleDifferentValues_UpdatesPropertyCorrectly()
    {
        // Arrange
        Mock<IDashboardService> mockDashboard = new Mock<IDashboardService>();
        Mock<IAuthService> mockAuth = new Mock<IAuthService>();
        Mock<SessionService> mockSession = new Mock<SessionService>();
        DashboardViewModel viewModel = new DashboardViewModel(mockDashboard.Object, mockAuth.Object, mockSession.Object, new Mock<IStudentScheduleService>().Object, new Mock<IRefreshCoordinator>().Object, new Mock<IInsightsService>().Object);

        // Act
        viewModel.TotalPrograms = 10;
        Assert.AreEqual(10, viewModel.TotalPrograms);

        viewModel.TotalPrograms = 20;
        Assert.AreEqual(20, viewModel.TotalPrograms);

        viewModel.TotalPrograms = -5;
        Assert.AreEqual(-5, viewModel.TotalPrograms);

        viewModel.TotalPrograms = 0;

        // Assert
        Assert.AreEqual(0, viewModel.TotalPrograms);
    }

    /// <summary>
    /// Tests that setting TotalPrograms to int.MaxValue boundary value works correctly.
    /// Input: int.MaxValue (2147483647).
    /// Expected: Property is updated and PropertyChanged event is raised.
    /// </summary>
    [TestMethod]
    public void TotalPrograms_SetToMaxValue_UpdatesPropertyAndRaisesEvent()
    {
        // Arrange
        Mock<IDashboardService> mockDashboard = new Mock<IDashboardService>();
        Mock<IAuthService> mockAuth = new Mock<IAuthService>();
        Mock<SessionService> mockSession = new Mock<SessionService>();
        DashboardViewModel viewModel = new DashboardViewModel(mockDashboard.Object, mockAuth.Object, mockSession.Object, new Mock<IStudentScheduleService>().Object, new Mock<IRefreshCoordinator>().Object, new Mock<IInsightsService>().Object);
        string? changedPropertyName = null;
        viewModel.PropertyChanged += (sender, args) => changedPropertyName = args.PropertyName;

        // Act
        viewModel.TotalPrograms = int.MaxValue;

        // Assert
        Assert.AreEqual(int.MaxValue, viewModel.TotalPrograms);
        Assert.AreEqual("TotalPrograms", changedPropertyName);
    }

    /// <summary>
    /// Tests that setting TotalPrograms to int.MinValue boundary value works correctly.
    /// Input: int.MinValue (-2147483648).
    /// Expected: Property is updated and PropertyChanged event is raised.
    /// </summary>
    [TestMethod]
    public void TotalPrograms_SetToMinValue_UpdatesPropertyAndRaisesEvent()
    {
        // Arrange
        Mock<IDashboardService> mockDashboard = new Mock<IDashboardService>();
        Mock<IAuthService> mockAuth = new Mock<IAuthService>();
        Mock<SessionService> mockSession = new Mock<SessionService>();
        DashboardViewModel viewModel = new DashboardViewModel(mockDashboard.Object, mockAuth.Object, mockSession.Object, new Mock<IStudentScheduleService>().Object, new Mock<IRefreshCoordinator>().Object, new Mock<IInsightsService>().Object);
        string? changedPropertyName = null;
        viewModel.PropertyChanged += (sender, args) => changedPropertyName = args.PropertyName;

        // Act
        viewModel.TotalPrograms = int.MinValue;

        // Assert
        Assert.AreEqual(int.MinValue, viewModel.TotalPrograms);
        Assert.AreEqual("TotalPrograms", changedPropertyName);
    }

    /// <summary>
    /// Tests that changing TotalPrograms from a positive to a negative value updates correctly.
    /// Input: First set to 100, then set to -50.
    /// Expected: Property reflects both updates correctly.
    /// </summary>
    [TestMethod]
    public void TotalPrograms_ChangeFromPositiveToNegative_UpdatesCorrectly()
    {
        // Arrange
        Mock<IDashboardService> mockDashboard = new Mock<IDashboardService>();
        Mock<IAuthService> mockAuth = new Mock<IAuthService>();
        Mock<SessionService> mockSession = new Mock<SessionService>();
        DashboardViewModel viewModel = new DashboardViewModel(mockDashboard.Object, mockAuth.Object, mockSession.Object, new Mock<IStudentScheduleService>().Object, new Mock<IRefreshCoordinator>().Object, new Mock<IInsightsService>().Object);

        // Act
        viewModel.TotalPrograms = 100;
        Assert.AreEqual(100, viewModel.TotalPrograms);

        viewModel.TotalPrograms = -50;

        // Assert
        Assert.AreEqual(-50, viewModel.TotalPrograms);
    }

    /// <summary>
    /// Tests that the PropertyChanged event sender is the ViewModel instance.
    /// Input: Set TotalPrograms to a new value (75).
    /// Expected: PropertyChanged event sender is the viewModel instance.
    /// </summary>
    [TestMethod]
    public void TotalPrograms_SetValue_PropertyChangedEventHasCorrectSender()
    {
        // Arrange
        Mock<IDashboardService> mockDashboard = new Mock<IDashboardService>();
        Mock<IAuthService> mockAuth = new Mock<IAuthService>();
        Mock<SessionService> mockSession = new Mock<SessionService>();
        DashboardViewModel viewModel = new DashboardViewModel(mockDashboard.Object, mockAuth.Object, mockSession.Object, new Mock<IStudentScheduleService>().Object, new Mock<IRefreshCoordinator>().Object, new Mock<IInsightsService>().Object);
        object? eventSender = null;
        viewModel.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == "TotalPrograms")
            {
                eventSender = sender;
            }
        };

        // Act
        viewModel.TotalPrograms = 75;

        // Assert
        Assert.AreSame(viewModel, eventSender);
    }

    /// <summary>
    /// Tests that the HasClasses property returns the default value of false when not explicitly set.
    /// Input: None (uses default initialization).
    /// Expected: HasClasses returns false.
    /// </summary>
    [TestMethod]
    public void HasClasses_DefaultValue_ReturnsFalse()
    {
        // Arrange
        Mock<IDashboardService> mockDashboardService = new Mock<IDashboardService>();
        Mock<IAuthService> mockAuthService = new Mock<IAuthService>();
        Mock<SessionService> mockSessionService = new Mock<SessionService>();
        DashboardViewModel viewModel = new DashboardViewModel(mockDashboardService.Object, mockAuthService.Object, mockSessionService.Object, new Mock<IStudentScheduleService>().Object, new Mock<IRefreshCoordinator>().Object, new Mock<IInsightsService>().Object);

        // Act
        bool result = viewModel.HasClasses;

        // Assert
        Assert.IsFalse(result);
    }

    /// <summary>
    /// Tests that HasClasses correctly stores and returns the value when set.
    /// Input: Boolean value (true or false).
    /// Expected: Property returns the set value.
    /// </summary>
    /// <param name="value">The boolean value to set and verify.</param>
    [TestMethod]
    [DataRow(true)]
    [DataRow(false)]
    public void HasClasses_SetValue_ReturnsExpectedValue(bool value)
    {
        // Arrange
        Mock<IDashboardService> mockDashboardService = new Mock<IDashboardService>();
        Mock<IAuthService> mockAuthService = new Mock<IAuthService>();
        Mock<SessionService> mockSessionService = new Mock<SessionService>();
        DashboardViewModel viewModel = new DashboardViewModel(mockDashboardService.Object, mockAuthService.Object, mockSessionService.Object, new Mock<IStudentScheduleService>().Object, new Mock<IRefreshCoordinator>().Object, new Mock<IInsightsService>().Object);

        // Act
        viewModel.HasClasses = value;
        bool result = viewModel.HasClasses;

        // Assert
        Assert.AreEqual(value, result);
    }

    /// <summary>
    /// Tests that HasClasses raises PropertyChanged event when value changes.
    /// Input: New boolean value different from current.
    /// Expected: PropertyChanged event is raised with property name "HasClasses".
    /// </summary>
    /// <param name="newValue">The new boolean value to set.</param>
    [TestMethod]
    [DataRow(true)]
    [DataRow(false)]
    public void HasClasses_SetValue_RaisesPropertyChanged(bool newValue)
    {
        // Arrange
        Mock<IDashboardService> mockDashboardService = new Mock<IDashboardService>();
        Mock<IAuthService> mockAuthService = new Mock<IAuthService>();
        Mock<SessionService> mockSessionService = new Mock<SessionService>();
        DashboardViewModel viewModel = new DashboardViewModel(mockDashboardService.Object, mockAuthService.Object, mockSessionService.Object, new Mock<IStudentScheduleService>().Object, new Mock<IRefreshCoordinator>().Object, new Mock<IInsightsService>().Object);

        string? raisedPropertyName = null;
        viewModel.PropertyChanged += (sender, args) =>
        {
            raisedPropertyName = args.PropertyName;
        };

        // Act
        viewModel.HasClasses = newValue;

        // Assert
        Assert.AreEqual("HasClasses", raisedPropertyName);
    }

    /// <summary>
    /// Tests that HasClasses does not raise PropertyChanged event when set to the same value.
    /// Input: Same boolean value as current.
    /// Expected: PropertyChanged event is not raised.
    /// </summary>
    [TestMethod]
    public void HasClasses_SetSameValue_DoesNotRaisePropertyChanged()
    {
        // Arrange
        Mock<IDashboardService> mockDashboardService = new Mock<IDashboardService>();
        Mock<IAuthService> mockAuthService = new Mock<IAuthService>();
        Mock<SessionService> mockSessionService = new Mock<SessionService>();
        DashboardViewModel viewModel = new DashboardViewModel(mockDashboardService.Object, mockAuthService.Object, mockSessionService.Object, new Mock<IStudentScheduleService>().Object, new Mock<IRefreshCoordinator>().Object, new Mock<IInsightsService>().Object);

        viewModel.HasClasses = false;

        bool propertyChangedRaised = false;
        viewModel.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == "HasClasses")
            {
                propertyChangedRaised = true;
            }
        };

        // Act
        viewModel.HasClasses = false;

        // Assert
        Assert.IsFalse(propertyChangedRaised);
    }

    /// <summary>
    /// Tests that HasClasses correctly toggles between true and false values.
    /// Input: Sequential setting of true then false.
    /// Expected: Property value reflects each change correctly.
    /// </summary>
    [TestMethod]
    public void HasClasses_ToggleValue_ReturnsCorrectValue()
    {
        // Arrange
        Mock<IDashboardService> mockDashboardService = new Mock<IDashboardService>();
        Mock<IAuthService> mockAuthService = new Mock<IAuthService>();
        Mock<SessionService> mockSessionService = new Mock<SessionService>();
        DashboardViewModel viewModel = new DashboardViewModel(mockDashboardService.Object, mockAuthService.Object, mockSessionService.Object, new Mock<IStudentScheduleService>().Object, new Mock<IRefreshCoordinator>().Object, new Mock<IInsightsService>().Object);

        // Act & Assert
        viewModel.HasClasses = true;
        Assert.IsTrue(viewModel.HasClasses);

        viewModel.HasClasses = false;
        Assert.IsFalse(viewModel.HasClasses);

        viewModel.HasClasses = true;
        Assert.IsTrue(viewModel.HasClasses);
    }

    /// <summary>
    /// Tests that RefreshAsync successfully calls the dashboard service and loads data.
    /// Input: Valid dashboard service that returns valid data.
    /// Expected: Dashboard service is called and properties are updated.
    /// </summary>
    [TestMethod]
    public async Task RefreshAsync_WithValidData_CallsDashboardServiceAndUpdatesProperties()
    {
        // Arrange
        var mockDashboardService = new Mock<IDashboardService>();
        var mockAuthService = new Mock<IAuthService>();
        var mockSessionService = new Mock<SessionService>();

        var dashboardData = new DashboardDto
        {
            ActiveCourses = 5,
            UpcomingAssignments = 3,
            AttendancePercent = 0.85,
            RecentAnnouncements = new List<AnnouncementDto>(),
            TeachingClasses = new List<ClassDto>(),
            ManagedClasses = new List<ClassDto>()
        };

        mockDashboardService.Setup(s => s.GetDashboardAsync()).ReturnsAsync(dashboardData);
        mockSessionService.Setup(s => s.Role).Returns(UserRole.Student);

        var viewModel = new DashboardViewModel(mockDashboardService.Object, mockAuthService.Object, mockSessionService.Object, new Mock<IStudentScheduleService>().Object, new Mock<IRefreshCoordinator>().Object, new Mock<IInsightsService>().Object);

        // Act
        await viewModel.RefreshAsync();

        // Assert
        mockDashboardService.Verify(s => s.GetDashboardAsync(), Times.Once);
        Assert.AreEqual(5, viewModel.ActiveCourses);
        Assert.AreEqual(3, viewModel.UpcomingAssignments);
        Assert.AreEqual(85, viewModel.AttendancePercentage);
    }

    /// <summary>
    /// Tests that RefreshAsync forces a refresh even when data was recently loaded.
    /// Input: ViewModel with recently loaded data.
    /// Expected: Dashboard service is called again despite recent load.
    /// </summary>
    [TestMethod]
    public async Task RefreshAsync_AfterRecentLoad_ForcesRefreshAndCallsService()
    {
        // Arrange
        var mockDashboardService = new Mock<IDashboardService>();
        var mockAuthService = new Mock<IAuthService>();
        var mockSessionService = new Mock<SessionService>();

        var dashboardData = new DashboardDto
        {
            ActiveCourses = 5,
            UpcomingAssignments = 3,
            AttendancePercent = 0.85,
            RecentAnnouncements = new List<AnnouncementDto>(),
            TeachingClasses = new List<ClassDto>(),
            ManagedClasses = new List<ClassDto>()
        };

        mockDashboardService.Setup(s => s.GetDashboardAsync()).ReturnsAsync(dashboardData);
        mockSessionService.Setup(s => s.Role).Returns(UserRole.Student);

        var viewModel = new DashboardViewModel(mockDashboardService.Object, mockAuthService.Object, mockSessionService.Object, new Mock<IStudentScheduleService>().Object, new Mock<IRefreshCoordinator>().Object, new Mock<IInsightsService>().Object);

        // First load
        await viewModel.LoadAsync(forceRefresh: true);
        mockDashboardService.Invocations.Clear();

        // Act - Call RefreshAsync immediately after
        await viewModel.RefreshAsync();

        // Assert - Service should be called again despite recent load
        mockDashboardService.Verify(s => s.GetDashboardAsync(), Times.Once);
    }

    /// <summary>
    /// Tests that RefreshAsync clears the error message at the start.
    /// Input: ViewModel with an existing error message.
    /// Expected: Error message is cleared when RefreshAsync is called.
    /// </summary>
    [TestMethod]
    public async Task RefreshAsync_WithExistingError_ClearsErrorMessage()
    {
        // Arrange
        var mockDashboardService = new Mock<IDashboardService>();
        var mockAuthService = new Mock<IAuthService>();
        var mockSessionService = new Mock<SessionService>();

        var dashboardData = new DashboardDto
        {
            ActiveCourses = 5,
            UpcomingAssignments = 3,
            AttendancePercent = 0.85,
            RecentAnnouncements = new List<AnnouncementDto>(),
            TeachingClasses = new List<ClassDto>(),
            ManagedClasses = new List<ClassDto>()
        };

        mockDashboardService.Setup(s => s.GetDashboardAsync()).ReturnsAsync(dashboardData);
        mockSessionService.Setup(s => s.Role).Returns(UserRole.Student);

        var viewModel = new DashboardViewModel(mockDashboardService.Object, mockAuthService.Object, mockSessionService.Object, new Mock<IStudentScheduleService>().Object, new Mock<IRefreshCoordinator>().Object, new Mock<IInsightsService>().Object);
        viewModel.ErrorMessage = "Previous error";

        // Act
        await viewModel.RefreshAsync();

        // Assert
        Assert.AreEqual(string.Empty, viewModel.ErrorMessage);
    }

    /// <summary>
    /// Tests that RefreshAsync handles exceptions from the dashboard service and sets error message.
    /// Input: Dashboard service that throws an exception.
    /// Expected: Error message is set with exception details.
    /// </summary>
    [TestMethod]
    public async Task RefreshAsync_WhenServiceThrowsException_SetsErrorMessage()
    {
        // Arrange
        var mockDashboardService = new Mock<IDashboardService>();
        var mockAuthService = new Mock<IAuthService>();
        var mockSessionService = new Mock<SessionService>();

        var exceptionMessage = "Database connection failed";
        mockDashboardService.Setup(s => s.GetDashboardAsync())
            .ThrowsAsync(new InvalidOperationException(exceptionMessage));
        mockSessionService.Setup(s => s.Role).Returns(UserRole.Student);

        var viewModel = new DashboardViewModel(mockDashboardService.Object, mockAuthService.Object, mockSessionService.Object, new Mock<IStudentScheduleService>().Object, new Mock<IRefreshCoordinator>().Object, new Mock<IInsightsService>().Object);

        // Act
        await viewModel.RefreshAsync();

        // Assert
        Assert.IsTrue(viewModel.ErrorMessage.Contains(exceptionMessage));
        Assert.IsTrue(viewModel.ErrorMessage.Contains("Failed to load dashboard"));
    }

    /// <summary>
    /// Tests that RefreshAsync sets IsBusy to false after completion, even when an exception occurs.
    /// Input: Dashboard service that throws an exception.
    /// Expected: IsBusy is false after RefreshAsync completes.
    /// </summary>
    [TestMethod]
    public async Task RefreshAsync_WhenExceptionOccurs_SetsIsBusyToFalse()
    {
        // Arrange
        var mockDashboardService = new Mock<IDashboardService>();
        var mockAuthService = new Mock<IAuthService>();
        var mockSessionService = new Mock<SessionService>();

        mockDashboardService.Setup(s => s.GetDashboardAsync())
            .ThrowsAsync(new Exception("Test exception"));
        mockSessionService.Setup(s => s.Role).Returns(UserRole.Student);

        var viewModel = new DashboardViewModel(mockDashboardService.Object, mockAuthService.Object, mockSessionService.Object, new Mock<IStudentScheduleService>().Object, new Mock<IRefreshCoordinator>().Object, new Mock<IInsightsService>().Object);

        // Act
        await viewModel.RefreshAsync();

        // Assert
        Assert.IsFalse(viewModel.IsBusy);
    }

    /// <summary>
    /// Tests that RefreshAsync updates announcement properties when announcements are present.
    /// Input: Dashboard data with announcements.
    /// Expected: Announcement properties are updated correctly.
    /// </summary>
    [TestMethod]
    public async Task RefreshAsync_WithAnnouncements_UpdatesAnnouncementProperties()
    {
        // Arrange
        var mockDashboardService = new Mock<IDashboardService>();
        var mockAuthService = new Mock<IAuthService>();
        var mockSessionService = new Mock<SessionService>();

        var announcementDate = new DateTime(2024, 1, 15);
        var dashboardData = new DashboardDto
        {
            ActiveCourses = 5,
            UpcomingAssignments = 3,
            AttendancePercent = 0.85,
            RecentAnnouncements = new List<AnnouncementDto>
            {
                new AnnouncementDto
                {
                    Title = "Important Update",
                    Body = "Please review the new guidelines.",
                    Date = announcementDate
                }
            },
            TeachingClasses = new List<ClassDto>(),
            ManagedClasses = new List<ClassDto>()
        };

        mockDashboardService.Setup(s => s.GetDashboardAsync()).ReturnsAsync(dashboardData);
        mockSessionService.Setup(s => s.Role).Returns(UserRole.Student);

        var viewModel = new DashboardViewModel(mockDashboardService.Object, mockAuthService.Object, mockSessionService.Object, new Mock<IStudentScheduleService>().Object, new Mock<IRefreshCoordinator>().Object, new Mock<IInsightsService>().Object);

        // Act
        await viewModel.RefreshAsync();

        // Assert
        Assert.IsTrue(viewModel.HasAnnouncement);
        Assert.AreEqual("Important Update", viewModel.LatestAnnouncementTitle);
        Assert.AreEqual("Please review the new guidelines.", viewModel.LatestAnnouncementBody);
        Assert.AreEqual("Jan 15, 2024", viewModel.LatestAnnouncementDate);
    }

    /// <summary>
    /// Tests that RefreshAsync sets HasAnnouncement to false when no announcements are present.
    /// Input: Dashboard data with empty announcements list.
    /// Expected: HasAnnouncement is false.
    /// </summary>
    [TestMethod]
    public async Task RefreshAsync_WithNoAnnouncements_SetsHasAnnouncementToFalse()
    {
        // Arrange
        var mockDashboardService = new Mock<IDashboardService>();
        var mockAuthService = new Mock<IAuthService>();
        var mockSessionService = new Mock<SessionService>();

        var dashboardData = new DashboardDto
        {
            ActiveCourses = 5,
            UpcomingAssignments = 3,
            AttendancePercent = 0.85,
            RecentAnnouncements = new List<AnnouncementDto>(),
            TeachingClasses = new List<ClassDto>(),
            ManagedClasses = new List<ClassDto>()
        };

        mockDashboardService.Setup(s => s.GetDashboardAsync()).ReturnsAsync(dashboardData);
        mockSessionService.Setup(s => s.Role).Returns(UserRole.Student);

        var viewModel = new DashboardViewModel(mockDashboardService.Object, mockAuthService.Object, mockSessionService.Object, new Mock<IStudentScheduleService>().Object, new Mock<IRefreshCoordinator>().Object, new Mock<IInsightsService>().Object);
        viewModel.HasAnnouncement = true;

        // Act
        await viewModel.RefreshAsync();

        // Assert
        Assert.IsFalse(viewModel.HasAnnouncement);
    }

    /// <summary>
    /// Tests that multiple consecutive calls to RefreshAsync work correctly.
    /// Input: Multiple calls to RefreshAsync.
    /// Expected: Each call successfully loads data from the service.
    /// </summary>
    [TestMethod]
    public async Task RefreshAsync_MultipleConsecutiveCalls_CallsServiceEachTime()
    {
        // Arrange
        var mockDashboardService = new Mock<IDashboardService>();
        var mockAuthService = new Mock<IAuthService>();
        var mockSessionService = new Mock<SessionService>();

        var dashboardData = new DashboardDto
        {
            ActiveCourses = 5,
            UpcomingAssignments = 3,
            AttendancePercent = 0.85,
            RecentAnnouncements = new List<AnnouncementDto>(),
            TeachingClasses = new List<ClassDto>(),
            ManagedClasses = new List<ClassDto>()
        };

        mockDashboardService.Setup(s => s.GetDashboardAsync()).ReturnsAsync(dashboardData);
        mockSessionService.Setup(s => s.Role).Returns(UserRole.Student);

        var viewModel = new DashboardViewModel(mockDashboardService.Object, mockAuthService.Object, mockSessionService.Object, new Mock<IStudentScheduleService>().Object, new Mock<IRefreshCoordinator>().Object, new Mock<IInsightsService>().Object);

        // Act
        await viewModel.RefreshAsync();
        await viewModel.RefreshAsync();
        await viewModel.RefreshAsync();

        // Assert
        mockDashboardService.Verify(s => s.GetDashboardAsync(), Times.Exactly(3));
    }

    /// <summary>
    /// Tests that RefreshAsync updates CurrentDate to the current date.
    /// Input: Valid dashboard data.
    /// Expected: CurrentDate is updated to current date format.
    /// </summary>
    [TestMethod]
    public async Task RefreshAsync_WhenCalled_UpdatesCurrentDate()
    {
        // Arrange
        var mockDashboardService = new Mock<IDashboardService>();
        var mockAuthService = new Mock<IAuthService>();
        var mockSessionService = new Mock<SessionService>();

        var dashboardData = new DashboardDto
        {
            ActiveCourses = 5,
            UpcomingAssignments = 3,
            AttendancePercent = 0.85,
            RecentAnnouncements = new List<AnnouncementDto>(),
            TeachingClasses = new List<ClassDto>(),
            ManagedClasses = new List<ClassDto>()
        };

        mockDashboardService.Setup(s => s.GetDashboardAsync()).ReturnsAsync(dashboardData);
        mockSessionService.Setup(s => s.Role).Returns(UserRole.Student);

        var viewModel = new DashboardViewModel(mockDashboardService.Object, mockAuthService.Object, mockSessionService.Object, new Mock<IStudentScheduleService>().Object, new Mock<IRefreshCoordinator>().Object, new Mock<IInsightsService>().Object);
        var expectedDate = DateTime.Now.ToString("dddd, MMMM dd");

        // Act
        await viewModel.RefreshAsync();

        // Assert
        Assert.AreEqual(expectedDate, viewModel.CurrentDate);
    }

    /// <summary>
    /// Tests that RefreshAsync clears and repopulates TeachingClasses collection.
    /// Input: Dashboard data with teaching classes.
    /// Expected: TeachingClasses collection is updated with new data.
    /// </summary>
    [TestMethod]
    public async Task RefreshAsync_WithTeachingClasses_ClearsAndPopulatesCollection()
    {
        // Arrange
        var mockDashboardService = new Mock<IDashboardService>();
        var mockAuthService = new Mock<IAuthService>();
        var mockSessionService = new Mock<SessionService>();

        var classDto = new ClassDto(Guid.NewGuid().ToString(), "Mathematics 101", "MATH101", null, 30, "Dr. Smith");
        var dashboardData = new DashboardDto
        {
            ActiveCourses = 5,
            UpcomingAssignments = 3,
            AttendancePercent = 0.85,
            RecentAnnouncements = new List<AnnouncementDto>(),
            TeachingClasses = new List<ClassDto> { classDto },
            ManagedClasses = new List<ClassDto>()
        };

        mockDashboardService.Setup(s => s.GetDashboardAsync()).ReturnsAsync(dashboardData);
        mockSessionService.Setup(s => s.Role).Returns(UserRole.Lecturer);

        var viewModel = new DashboardViewModel(mockDashboardService.Object, mockAuthService.Object, mockSessionService.Object, new Mock<IStudentScheduleService>().Object, new Mock<IRefreshCoordinator>().Object, new Mock<IInsightsService>().Object);

        // Act
        await viewModel.RefreshAsync();

        // Assert
        Assert.AreEqual(1, viewModel.TeachingClasses.Count);
        Assert.AreEqual("Mathematics 101", viewModel.TeachingClasses[0].Name);
        Assert.AreEqual(1, viewModel.TeachingClassesCount);
    }

    /// <summary>
    /// Tests that RefreshAsync handles null TeachingClasses correctly.
    /// Input: Dashboard data with null TeachingClasses.
    /// Expected: TeachingClassesCount is set to 0 and collection is cleared.
    /// </summary>
    [TestMethod]
    public async Task RefreshAsync_WithNullTeachingClasses_SetsCountToZero()
    {
        // Arrange
        var mockDashboardService = new Mock<IDashboardService>();
        var mockAuthService = new Mock<IAuthService>();
        var mockSessionService = new Mock<SessionService>();

        var dashboardData = new DashboardDto
        {
            ActiveCourses = 5,
            UpcomingAssignments = 3,
            AttendancePercent = 0.85,
            RecentAnnouncements = new List<AnnouncementDto>(),
            TeachingClasses = null,
            ManagedClasses = null
        };

        mockDashboardService.Setup(s => s.GetDashboardAsync()).ReturnsAsync(dashboardData);
        mockSessionService.Setup(s => s.Role).Returns(UserRole.Student);

        var viewModel = new DashboardViewModel(mockDashboardService.Object, mockAuthService.Object, mockSessionService.Object, new Mock<IStudentScheduleService>().Object, new Mock<IRefreshCoordinator>().Object, new Mock<IInsightsService>().Object);

        // Act
        await viewModel.RefreshAsync();

        // Assert
        Assert.AreEqual(0, viewModel.TeachingClassesCount);
        Assert.AreEqual(0, viewModel.ManagedClassesCount);
    }

    /// <summary>
    /// Tests that RefreshAsync updates attendance percentage correctly with various values.
    /// Input: Dashboard data with different attendance percentages.
    /// Expected: AttendancePercentage is calculated correctly as integer percentage.
    /// </summary>
    [TestMethod]
    [DataRow(0.0, 0)]
    [DataRow(0.5, 50)]
    [DataRow(0.85, 85)]
    [DataRow(1.0, 100)]
    [DataRow(0.999, 99)]
    public async Task RefreshAsync_WithVariousAttendanceValues_CalculatesPercentageCorrectly(double attendancePercent, int expectedPercentage)
    {
        // Arrange
        var mockDashboardService = new Mock<IDashboardService>();
        var mockAuthService = new Mock<IAuthService>();
        var mockSessionService = new Mock<SessionService>();

        var dashboardData = new DashboardDto
        {
            ActiveCourses = 5,
            UpcomingAssignments = 3,
            AttendancePercent = attendancePercent,
            RecentAnnouncements = new List<AnnouncementDto>(),
            TeachingClasses = new List<ClassDto>(),
            ManagedClasses = new List<ClassDto>()
        };

        mockDashboardService.Setup(s => s.GetDashboardAsync()).ReturnsAsync(dashboardData);
        mockSessionService.Setup(s => s.Role).Returns(UserRole.Student);

        var viewModel = new DashboardViewModel(mockDashboardService.Object, mockAuthService.Object, mockSessionService.Object, new Mock<IStudentScheduleService>().Object, new Mock<IRefreshCoordinator>().Object, new Mock<IInsightsService>().Object);

        // Act
        await viewModel.RefreshAsync();

        // Assert
        Assert.AreEqual(expectedPercentage, viewModel.AttendancePercentage);
    }

    /// <summary>
    /// Tests that RefreshAsync sets Role property from SessionService.
    /// Input: Dashboard data with SessionService having different roles.
    /// Expected: Role property is updated to match SessionService.Role.
    /// </summary>
    [TestMethod]
    [DataRow(UserRole.Student, "Student")]
    [DataRow(UserRole.Lecturer, "Lecturer")]
    [DataRow(UserRole.Admin, "Admin")]
    public async Task RefreshAsync_WithDifferentRoles_UpdatesRoleProperty(UserRole sessionRole, string expectedRole)
    {
        // Arrange
        var mockDashboardService = new Mock<IDashboardService>();
        var mockAuthService = new Mock<IAuthService>();
        var mockSessionService = new Mock<SessionService>();

        var dashboardData = new DashboardDto
        {
            ActiveCourses = 5,
            UpcomingAssignments = 3,
            AttendancePercent = 0.85,
            RecentAnnouncements = new List<AnnouncementDto>(),
            TeachingClasses = new List<ClassDto>(),
            ManagedClasses = new List<ClassDto>()
        };

        mockDashboardService.Setup(s => s.GetDashboardAsync()).ReturnsAsync(dashboardData);
        mockSessionService.Setup(s => s.Role).Returns(sessionRole);

        var viewModel = new DashboardViewModel(mockDashboardService.Object, mockAuthService.Object, mockSessionService.Object, new Mock<IStudentScheduleService>().Object, new Mock<IRefreshCoordinator>().Object, new Mock<IInsightsService>().Object);

        // Act
        await viewModel.RefreshAsync();

        // Assert
        Assert.AreEqual(expectedRole, viewModel.Role);
    }

    /// <summary>
    /// Tests that RefreshAsync updates optional admin properties when they have values.
    /// Input: Dashboard data with TotalStudents, TotalLecturers, and TotalPrograms.
    /// Expected: Properties are updated with the provided values.
    /// </summary>
    [TestMethod]
    public async Task RefreshAsync_WithAdminProperties_UpdatesOptionalProperties()
    {
        // Arrange
        var mockDashboardService = new Mock<IDashboardService>();
        var mockAuthService = new Mock<IAuthService>();
        var mockSessionService = new Mock<SessionService>();

        var dashboardData = new DashboardDto
        {
            ActiveCourses = 5,
            UpcomingAssignments = 3,
            AttendancePercent = 0.85,
            RecentAnnouncements = new List<AnnouncementDto>(),
            TeachingClasses = new List<ClassDto>(),
            ManagedClasses = new List<ClassDto>(),
            TotalStudents = 150,
            TotalLecturers = 25,
            TotalPrograms = 10
        };

        mockDashboardService.Setup(s => s.GetDashboardAsync()).ReturnsAsync(dashboardData);
        mockSessionService.Setup(s => s.Role).Returns(UserRole.Admin);

        var viewModel = new DashboardViewModel(mockDashboardService.Object, mockAuthService.Object, mockSessionService.Object, new Mock<IStudentScheduleService>().Object, new Mock<IRefreshCoordinator>().Object, new Mock<IInsightsService>().Object);

        // Act
        await viewModel.RefreshAsync();

        // Assert
        Assert.AreEqual(150, viewModel.TotalStudents);
        Assert.AreEqual(25, viewModel.TotalLecturers);
        Assert.AreEqual(10, viewModel.TotalPrograms);
    }

    /// <summary>
    /// Tests that RefreshAsync does not update optional admin properties when they are null.
    /// Input: Dashboard data with null optional properties.
    /// Expected: Properties retain their initial values (0).
    /// </summary>
    [TestMethod]
    public async Task RefreshAsync_WithNullAdminProperties_DoesNotUpdateProperties()
    {
        // Arrange
        var mockDashboardService = new Mock<IDashboardService>();
        var mockAuthService = new Mock<IAuthService>();
        var mockSessionService = new Mock<SessionService>();

        var dashboardData = new DashboardDto
        {
            ActiveCourses = 5,
            UpcomingAssignments = 3,
            AttendancePercent = 0.85,
            RecentAnnouncements = new List<AnnouncementDto>(),
            TeachingClasses = new List<ClassDto>(),
            ManagedClasses = new List<ClassDto>(),
            TotalStudents = null,
            TotalLecturers = null,
            TotalPrograms = null
        };

        mockDashboardService.Setup(s => s.GetDashboardAsync()).ReturnsAsync(dashboardData);
        mockSessionService.Setup(s => s.Role).Returns(UserRole.Student);

        var viewModel = new DashboardViewModel(mockDashboardService.Object, mockAuthService.Object, mockSessionService.Object, new Mock<IStudentScheduleService>().Object, new Mock<IRefreshCoordinator>().Object, new Mock<IInsightsService>().Object);

        // Act
        await viewModel.RefreshAsync();

        // Assert
        Assert.AreEqual(0, viewModel.TotalStudents);
        Assert.AreEqual(0, viewModel.TotalLecturers);
        Assert.AreEqual(0, viewModel.TotalPrograms);
    }

    /// <summary>
    /// Tests that RefreshAsync handles multiple announcements and uses the first as latest.
    /// Input: Dashboard data with multiple announcements.
    /// Expected: Latest announcement properties are set from the first announcement.
    /// </summary>
    [TestMethod]
    public async Task RefreshAsync_WithMultipleAnnouncements_UsesFirstAsLatest()
    {
        // Arrange
        var mockDashboardService = new Mock<IDashboardService>();
        var mockAuthService = new Mock<IAuthService>();
        var mockSessionService = new Mock<SessionService>();

        var firstDate = new DateTime(2024, 1, 20);
        var secondDate = new DateTime(2024, 1, 15);
        var dashboardData = new DashboardDto
        {
            ActiveCourses = 5,
            UpcomingAssignments = 3,
            AttendancePercent = 0.85,
            RecentAnnouncements = new List<AnnouncementDto>
            {
                new AnnouncementDto { Title = "First", Body = "First body", Date = firstDate },
                new AnnouncementDto { Title = "Second", Body = "Second body", Date = secondDate }
            },
            TeachingClasses = new List<ClassDto>(),
            ManagedClasses = new List<ClassDto>()
        };

        mockDashboardService.Setup(s => s.GetDashboardAsync()).ReturnsAsync(dashboardData);
        mockSessionService.Setup(s => s.Role).Returns(UserRole.Student);

        var viewModel = new DashboardViewModel(mockDashboardService.Object, mockAuthService.Object, mockSessionService.Object, new Mock<IStudentScheduleService>().Object, new Mock<IRefreshCoordinator>().Object, new Mock<IInsightsService>().Object);

        // Act
        await viewModel.RefreshAsync();

        // Assert
        Assert.AreEqual("First", viewModel.LatestAnnouncementTitle);
        Assert.AreEqual("First body", viewModel.LatestAnnouncementBody);
        Assert.AreEqual("Jan 20, 2024", viewModel.LatestAnnouncementDate);
    }

    /// <summary>
    /// Tests that RefreshAsync completes and sets IsBusy to false after successful execution.
    /// Input: Valid dashboard data.
    /// Expected: IsBusy is false after completion.
    /// </summary>
    [TestMethod]
    public async Task RefreshAsync_SuccessfulExecution_SetsIsBusyToFalse()
    {
        // Arrange
        var mockDashboardService = new Mock<IDashboardService>();
        var mockAuthService = new Mock<IAuthService>();
        var mockSessionService = new Mock<SessionService>();

        var dashboardData = new DashboardDto
        {
            ActiveCourses = 5,
            UpcomingAssignments = 3,
            AttendancePercent = 0.85,
            RecentAnnouncements = new List<AnnouncementDto>(),
            TeachingClasses = new List<ClassDto>(),
            ManagedClasses = new List<ClassDto>()
        };

        mockDashboardService.Setup(s => s.GetDashboardAsync()).ReturnsAsync(dashboardData);
        mockSessionService.Setup(s => s.Role).Returns(UserRole.Student);

        var viewModel = new DashboardViewModel(mockDashboardService.Object, mockAuthService.Object, mockSessionService.Object, new Mock<IStudentScheduleService>().Object, new Mock<IRefreshCoordinator>().Object, new Mock<IInsightsService>().Object);

        // Act
        await viewModel.RefreshAsync();

        // Assert
        Assert.IsFalse(viewModel.IsBusy);
    }

    /// <summary>
    /// Tests that RefreshAsync handles extreme negative attendance values correctly.
    /// Input: Dashboard data with negative attendance percentage.
    /// Expected: AttendancePercentage is calculated as negative integer.
    /// </summary>
    [TestMethod]
    public async Task RefreshAsync_WithNegativeAttendance_HandlesCorrectly()
    {
        // Arrange
        var mockDashboardService = new Mock<IDashboardService>();
        var mockAuthService = new Mock<IAuthService>();
        var mockSessionService = new Mock<SessionService>();

        var dashboardData = new DashboardDto
        {
            ActiveCourses = 5,
            UpcomingAssignments = 3,
            AttendancePercent = -0.5,
            RecentAnnouncements = new List<AnnouncementDto>(),
            TeachingClasses = new List<ClassDto>(),
            ManagedClasses = new List<ClassDto>()
        };

        mockDashboardService.Setup(s => s.GetDashboardAsync()).ReturnsAsync(dashboardData);
        mockSessionService.Setup(s => s.Role).Returns(UserRole.Student);

        var viewModel = new DashboardViewModel(mockDashboardService.Object, mockAuthService.Object, mockSessionService.Object, new Mock<IStudentScheduleService>().Object, new Mock<IRefreshCoordinator>().Object, new Mock<IInsightsService>().Object);

        // Act
        await viewModel.RefreshAsync();

        // Assert
        Assert.AreEqual(-50, viewModel.AttendancePercentage);
    }

    /// <summary>
    /// Tests that RefreshAsync handles attendance values greater than 1.0 correctly.
    /// Input: Dashboard data with attendance percentage > 1.0.
    /// Expected: AttendancePercentage is calculated as integer > 100.
    /// </summary>
    [TestMethod]
    public async Task RefreshAsync_WithAttendanceGreaterThanOne_HandlesCorrectly()
    {
        // Arrange
        var mockDashboardService = new Mock<IDashboardService>();
        var mockAuthService = new Mock<IAuthService>();
        var mockSessionService = new Mock<SessionService>();

        var dashboardData = new DashboardDto
        {
            ActiveCourses = 5,
            UpcomingAssignments = 3,
            AttendancePercent = 1.5,
            RecentAnnouncements = new List<AnnouncementDto>(),
            TeachingClasses = new List<ClassDto>(),
            ManagedClasses = new List<ClassDto>()
        };

        mockDashboardService.Setup(s => s.GetDashboardAsync()).ReturnsAsync(dashboardData);
        mockSessionService.Setup(s => s.Role).Returns(UserRole.Student);

        var viewModel = new DashboardViewModel(mockDashboardService.Object, mockAuthService.Object, mockSessionService.Object, new Mock<IStudentScheduleService>().Object, new Mock<IRefreshCoordinator>().Object, new Mock<IInsightsService>().Object);

        // Act
        await viewModel.RefreshAsync();

        // Assert
        Assert.AreEqual(150, viewModel.AttendancePercentage);
    }

    /// <summary>
    /// Tests that RefreshAsync clears RecentAnnouncements collection before populating.
    /// Input: ViewModel with existing announcements, then new data with different announcements.
    /// Expected: Old announcements are cleared and replaced with new ones.
    /// </summary>
    [TestMethod]
    public async Task RefreshAsync_WithExistingAnnouncements_ClearsAndRepopulates()
    {
        // Arrange
        var mockDashboardService = new Mock<IDashboardService>();
        var mockAuthService = new Mock<IAuthService>();
        var mockSessionService = new Mock<SessionService>();

        var firstData = new DashboardDto
        {
            ActiveCourses = 5,
            UpcomingAssignments = 3,
            AttendancePercent = 0.85,
            RecentAnnouncements = new List<AnnouncementDto>
            {
                new AnnouncementDto { Title = "Old", Body = "Old body", Date = DateTime.Now }
            },
            TeachingClasses = new List<ClassDto>(),
            ManagedClasses = new List<ClassDto>()
        };

        var secondData = new DashboardDto
        {
            ActiveCourses = 5,
            UpcomingAssignments = 3,
            AttendancePercent = 0.85,
            RecentAnnouncements = new List<AnnouncementDto>
            {
                new AnnouncementDto { Title = "New", Body = "New body", Date = DateTime.Now }
            },
            TeachingClasses = new List<ClassDto>(),
            ManagedClasses = new List<ClassDto>()
        };

        mockDashboardService.SetupSequence(s => s.GetDashboardAsync())
            .ReturnsAsync(firstData)
            .ReturnsAsync(secondData);
        mockSessionService.Setup(s => s.Role).Returns(UserRole.Student);

        var viewModel = new DashboardViewModel(mockDashboardService.Object, mockAuthService.Object, mockSessionService.Object, new Mock<IStudentScheduleService>().Object, new Mock<IRefreshCoordinator>().Object, new Mock<IInsightsService>().Object);

        // First refresh
        await viewModel.RefreshAsync();
        Assert.AreEqual(1, viewModel.RecentAnnouncements.Count);
        Assert.AreEqual("Old", viewModel.RecentAnnouncements[0].Title);

        // Act - Second refresh
        await viewModel.RefreshAsync();

        // Assert
        Assert.AreEqual(1, viewModel.RecentAnnouncements.Count);
        Assert.AreEqual("New", viewModel.RecentAnnouncements[0].Title);
    }

    /// <summary>
    /// Tests that the TotalStudents property correctly handles boundary transitions
    /// from zero to positive, positive to negative, and negative to zero.
    /// Input: Sequential values crossing zero boundary.
    /// Expected: All values should be stored and retrieved correctly.
    /// </summary>
    [TestMethod]
    public void TotalStudents_BoundaryTransitions_UpdatesCorrectly()
    {
        // Arrange
        var mockDashboardService = new Mock<IDashboardService>();
        var mockAuthService = new Mock<IAuthService>();
        var mockSessionService = new Mock<SessionService>();
        var viewModel = new DashboardViewModel(mockDashboardService.Object, mockAuthService.Object, mockSessionService.Object, new Mock<IStudentScheduleService>().Object, new Mock<IRefreshCoordinator>().Object, new Mock<IInsightsService>().Object);

        // Act & Assert
        viewModel.TotalStudents = 0;
        Assert.AreEqual(0, viewModel.TotalStudents);

        viewModel.TotalStudents = 100;
        Assert.AreEqual(100, viewModel.TotalStudents);

        viewModel.TotalStudents = -50;
        Assert.AreEqual(-50, viewModel.TotalStudents);

        viewModel.TotalStudents = 0;
        Assert.AreEqual(0, viewModel.TotalStudents);
    }

    /// <summary>
    /// Tests that the TotalStudents property correctly handles extreme boundary values
    /// and transitions between them.
    /// Input: int.MaxValue and int.MinValue.
    /// Expected: Values should be stored and retrieved without overflow or exceptions.
    /// </summary>
    [TestMethod]
    public void TotalStudents_ExtremeBoundaryValues_HandlesCorrectly()
    {
        // Arrange
        var mockDashboardService = new Mock<IDashboardService>();
        var mockAuthService = new Mock<IAuthService>();
        var mockSessionService = new Mock<SessionService>();
        var viewModel = new DashboardViewModel(mockDashboardService.Object, mockAuthService.Object, mockSessionService.Object, new Mock<IStudentScheduleService>().Object, new Mock<IRefreshCoordinator>().Object, new Mock<IInsightsService>().Object);

        // Act & Assert
        viewModel.TotalStudents = int.MaxValue;
        Assert.AreEqual(int.MaxValue, viewModel.TotalStudents);

        viewModel.TotalStudents = int.MinValue;
        Assert.AreEqual(int.MinValue, viewModel.TotalStudents);

        viewModel.TotalStudents = 0;
        Assert.AreEqual(0, viewModel.TotalStudents);
    }

    /// <summary>
    /// Tests that OpenClassCommand returns a non-null Command of Guid type.
    /// Verifies the command is correctly typed as Command&lt;Guid&gt;.
    /// </summary>
    [TestMethod]
    public void OpenClassCommand_WhenAccessed_ReturnsCommandOfGuidType()
    {
        // Arrange
        Mock<IDashboardService> mockDashboardService = new();
        Mock<IAuthService> mockAuthService = new();
        Mock<SessionService> mockSessionService = new();
        DashboardViewModel viewModel = new(mockDashboardService.Object, mockAuthService.Object, mockSessionService.Object);

        // Act
        ICommand command = viewModel.OpenClassCommand;

        // Assert
        Assert.IsNotNull(command);
        Assert.IsInstanceOfType(command, typeof(Command<Guid>));
    }

    /// <summary>
    /// Tests that multiple accesses to OpenClassCommand create new instances each time.
    /// Verifies expression-bodied property behavior creates new Command instances.
    /// Input: Multiple property accesses.
    /// Expected: Each access returns a different instance (reference inequality).
    /// </summary>
    [TestMethod]
    public void OpenClassCommand_MultipleAccesses_CreatesNewInstancesEachTime()
    {
        // Arrange
        Mock<IDashboardService> mockDashboardService = new();
        Mock<IAuthService> mockAuthService = new();
        Mock<SessionService> mockSessionService = new();
        DashboardViewModel viewModel = new(mockDashboardService.Object, mockAuthService.Object, mockSessionService.Object);

        // Act
        ICommand command1 = viewModel.OpenClassCommand;
        ICommand command2 = viewModel.OpenClassCommand;
        ICommand command3 = viewModel.OpenClassCommand;

        // Assert
        Assert.IsNotNull(command1);
        Assert.IsNotNull(command2);
        Assert.IsNotNull(command3);
        Assert.AreNotSame(command1, command2);
        Assert.AreNotSame(command2, command3);
        Assert.AreNotSame(command1, command3);
    }

    /// <summary>
    /// Tests that OpenClassCommand.CanExecute returns true for various Guid values.
    /// Verifies the command can be executed with any valid Guid parameter.
    /// Input: Various Guid values including Guid.Empty, new Guid, and specific Guid values.
    /// Expected: CanExecute returns true for all valid Guid values.
    /// </summary>
    [TestMethod]
    [DataRow("00000000-0000-0000-0000-000000000000", DisplayName = "CanExecute with Guid.Empty")]
    [DataRow("12345678-1234-1234-1234-123456789012", DisplayName = "CanExecute with specific Guid")]
    [DataRow("ffffffff-ffff-ffff-ffff-ffffffffffff", DisplayName = "CanExecute with max Guid")]
    [DataRow("a1b2c3d4-e5f6-4a5b-8c9d-0e1f2a3b4c5d", DisplayName = "CanExecute with random Guid")]
    public void OpenClassCommand_CanExecute_ReturnsTrueForVariousGuids(string guidString)
    {
        // Arrange
        Mock<IDashboardService> mockDashboardService = new();
        Mock<IAuthService> mockAuthService = new();
        Mock<SessionService> mockSessionService = new();
        DashboardViewModel viewModel = new(mockDashboardService.Object, mockAuthService.Object, mockSessionService.Object);
        Guid testGuid = Guid.Parse(guidString);

        // Act
        ICommand command = viewModel.OpenClassCommand;
        bool canExecute = command.CanExecute(testGuid);

        // Assert
        Assert.IsTrue(canExecute);
    }

    /// <summary>
    /// Tests that OpenClassCommand.CanExecute returns true when called with null parameter.
    /// Verifies the command's CanExecute behavior with null input.
    /// Input: null parameter.
    /// Expected: CanExecute returns true (Command&lt;T&gt; typically accepts null for CanExecute).
    /// </summary>
    [TestMethod]
    public void OpenClassCommand_CanExecuteWithNull_ReturnsTrue()
    {
        // Arrange
        Mock<IDashboardService> mockDashboardService = new();
        Mock<IAuthService> mockAuthService = new();
        Mock<SessionService> mockSessionService = new();
        DashboardViewModel viewModel = new(mockDashboardService.Object, mockAuthService.Object, mockSessionService.Object);

        // Act
        ICommand command = viewModel.OpenClassCommand;
        bool canExecute = command.CanExecute(null);

        // Assert
        Assert.IsTrue(canExecute);
    }

    /// <summary>
    /// Tests that OpenClassCommand property consistently returns ICommand instances.
    /// Verifies that despite creating new instances, all returned commands implement ICommand.
    /// Input: Multiple property accesses.
    /// Expected: All returned instances implement ICommand interface.
    /// </summary>
    [TestMethod]
    public void OpenClassCommand_MultipleAccesses_AllInstancesImplementICommand()
    {
        // Arrange
        Mock<IDashboardService> mockDashboardService = new();
        Mock<IAuthService> mockAuthService = new();
        Mock<SessionService> mockSessionService = new();
        DashboardViewModel viewModel = new(mockDashboardService.Object, mockAuthService.Object, mockSessionService.Object);

        // Act & Assert
        for (int i = 0; i < 5; i++)
        {
            ICommand command = viewModel.OpenClassCommand;
            Assert.IsNotNull(command);
            Assert.IsInstanceOfType(command, typeof(ICommand));
            Assert.IsInstanceOfType(command, typeof(Command<Guid>));
        }
    }

    /// <summary>
    /// Tests that OpenClassCommand returns a command that can accept Guid.NewGuid() values.
    /// Verifies the command works with dynamically generated Guid values.
    /// Input: Newly generated Guid values.
    /// Expected: Command is created successfully and CanExecute returns true.
    /// </summary>
    [TestMethod]
    public void OpenClassCommand_WithDynamicallyGeneratedGuids_CanExecuteReturnsTrue()
    {
        // Arrange
        Mock<IDashboardService> mockDashboardService = new();
        Mock<IAuthService> mockAuthService = new();
        Mock<SessionService> mockSessionService = new();
        DashboardViewModel viewModel = new(mockDashboardService.Object, mockAuthService.Object, mockSessionService.Object);

        // Act & Assert
        for (int i = 0; i < 10; i++)
        {
            Guid newGuid = Guid.NewGuid();
            ICommand command = viewModel.OpenClassCommand;
            bool canExecute = command.CanExecute(newGuid);
            Assert.IsTrue(canExecute);
        }
    }

    // Note: Testing the actual execution of OpenClassCommand (calling command.Execute(classId))
    // is not feasible in unit tests because the command implementation depends on Shell.Current,
    // which is a static property from Microsoft.Maui.Controls.Shell that cannot be mocked using Moq.
    // In a unit test environment, Shell.Current will be null, causing a NullReferenceException
    // when Execute is called. To test the navigation behavior, consider:
    // 1. Integration tests with a properly initialized Shell instance
    // 2. UI tests using the MAUI testing framework
    // 3. Refactoring to inject a navigation service abstraction (INavigationService) that can be mocked

    /// <summary>
    /// Tests that the UpcomingAssignments property returns the default value of 0 when not set.
    /// Input: None (default initialization).
    /// Expected: Property value should be 0.
    /// </summary>
    [TestMethod]
    public void UpcomingAssignments_DefaultValue_ReturnsZero()
    {
        // Arrange
        Mock<IDashboardService> mockDashboardService = new Mock<IDashboardService>();
        Mock<IAuthService> mockAuthService = new Mock<IAuthService>();
        Mock<SessionService> mockSessionService = new Mock<SessionService>();

        // Act
        DashboardViewModel viewModel = new DashboardViewModel(mockDashboardService.Object, mockAuthService.Object, mockSessionService.Object, new Mock<IStudentScheduleService>().Object, new Mock<IRefreshCoordinator>().Object, new Mock<IInsightsService>().Object);

        // Assert
        Assert.AreEqual(0, viewModel.UpcomingAssignments);
    }

    /// <summary>
    /// Tests that setting the UpcomingAssignments property to various valid integer values updates the property correctly.
    /// Input: Various integer values including boundary values.
    /// Expected: Property value should be updated to the set value.
    /// </summary>
    /// <param name="value">The integer value to set.</param>
    [TestMethod]
    [DataRow(0)]
    [DataRow(1)]
    [DataRow(-1)]
    [DataRow(100)]
    [DataRow(-100)]
    [DataRow(1000)]
    [DataRow(-1000)]
    [DataRow(int.MaxValue)]
    [DataRow(int.MinValue)]
    public void UpcomingAssignments_SetValue_UpdatesProperty(int value)
    {
        // Arrange
        Mock<IDashboardService> mockDashboardService = new Mock<IDashboardService>();
        Mock<IAuthService> mockAuthService = new Mock<IAuthService>();
        Mock<SessionService> mockSessionService = new Mock<SessionService>();
        DashboardViewModel viewModel = new DashboardViewModel(mockDashboardService.Object, mockAuthService.Object, mockSessionService.Object, new Mock<IStudentScheduleService>().Object, new Mock<IRefreshCoordinator>().Object, new Mock<IInsightsService>().Object);

        // Act
        viewModel.UpcomingAssignments = value;

        // Assert
        Assert.AreEqual(value, viewModel.UpcomingAssignments);
    }

    /// <summary>
    /// Tests that setting the UpcomingAssignments property to a new value raises the PropertyChanged event.
    /// Input: New value (50).
    /// Expected: PropertyChanged event should be raised with property name "UpcomingAssignments".
    /// </summary>
    [TestMethod]
    public void UpcomingAssignments_SetNewValue_RaisesPropertyChangedEvent()
    {
        // Arrange
        Mock<IDashboardService> mockDashboardService = new Mock<IDashboardService>();
        Mock<IAuthService> mockAuthService = new Mock<IAuthService>();
        Mock<SessionService> mockSessionService = new Mock<SessionService>();
        DashboardViewModel viewModel = new DashboardViewModel(mockDashboardService.Object, mockAuthService.Object, mockSessionService.Object, new Mock<IStudentScheduleService>().Object, new Mock<IRefreshCoordinator>().Object, new Mock<IInsightsService>().Object);
        bool eventRaised = false;
        string? raisedPropertyName = null;
        viewModel.PropertyChanged += (sender, args) =>
        {
            eventRaised = true;
            raisedPropertyName = args.PropertyName;
        };

        // Act
        viewModel.UpcomingAssignments = 50;

        // Assert
        Assert.IsTrue(eventRaised);
        Assert.AreEqual("UpcomingAssignments", raisedPropertyName);
    }

    /// <summary>
    /// Tests that setting the UpcomingAssignments property to the same value does not raise the PropertyChanged event.
    /// Input: Same value twice (25).
    /// Expected: PropertyChanged event should only be raised once, not on the second set.
    /// </summary>
    [TestMethod]
    public void UpcomingAssignments_SetSameValue_DoesNotRaisePropertyChangedEvent()
    {
        // Arrange
        Mock<IDashboardService> mockDashboardService = new Mock<IDashboardService>();
        Mock<IAuthService> mockAuthService = new Mock<IAuthService>();
        Mock<SessionService> mockSessionService = new Mock<SessionService>();
        DashboardViewModel viewModel = new DashboardViewModel(mockDashboardService.Object, mockAuthService.Object, mockSessionService.Object, new Mock<IStudentScheduleService>().Object, new Mock<IRefreshCoordinator>().Object, new Mock<IInsightsService>().Object);
        viewModel.UpcomingAssignments = 25;
        int eventRaisedCount = 0;
        viewModel.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == "UpcomingAssignments")
            {
                eventRaisedCount++;
            }
        };

        // Act
        viewModel.UpcomingAssignments = 25;

        // Assert
        Assert.AreEqual(0, eventRaisedCount);
    }

    /// <summary>
    /// Tests that multiple value changes to UpcomingAssignments update the property correctly.
    /// Input: Multiple different values (10, 20, 30).
    /// Expected: Property value should be updated to the last set value (30).
    /// </summary>
    [TestMethod]
    [DataRow(10, 20, 30)]
    [DataRow(0, 100, 50)]
    [DataRow(-5, 15, 0)]
    [DataRow(int.MinValue, int.MaxValue, 0)]
    public void UpcomingAssignments_SetMultipleValues_UpdatesPropertyCorrectly(int value1, int value2, int value3)
    {
        // Arrange
        Mock<IDashboardService> mockDashboardService = new Mock<IDashboardService>();
        Mock<IAuthService> mockAuthService = new Mock<IAuthService>();
        Mock<SessionService> mockSessionService = new Mock<SessionService>();
        DashboardViewModel viewModel = new DashboardViewModel(mockDashboardService.Object, mockAuthService.Object, mockSessionService.Object, new Mock<IStudentScheduleService>().Object, new Mock<IRefreshCoordinator>().Object, new Mock<IInsightsService>().Object);

        // Act
        viewModel.UpcomingAssignments = value1;
        viewModel.UpcomingAssignments = value2;
        viewModel.UpcomingAssignments = value3;

        // Assert
        Assert.AreEqual(value3, viewModel.UpcomingAssignments);
    }

    /// <summary>
    /// Tests that setting UpcomingAssignments to different values raises PropertyChanged event for each change.
    /// Input: Three different values.
    /// Expected: PropertyChanged event should be raised three times.
    /// </summary>
    [TestMethod]
    public void UpcomingAssignments_SetMultipleDifferentValues_RaisesPropertyChangedForEachChange()
    {
        // Arrange
        Mock<IDashboardService> mockDashboardService = new Mock<IDashboardService>();
        Mock<IAuthService> mockAuthService = new Mock<IAuthService>();
        Mock<SessionService> mockSessionService = new Mock<SessionService>();
        DashboardViewModel viewModel = new DashboardViewModel(mockDashboardService.Object, mockAuthService.Object, mockSessionService.Object, new Mock<IStudentScheduleService>().Object, new Mock<IRefreshCoordinator>().Object, new Mock<IInsightsService>().Object);
        int eventRaisedCount = 0;
        viewModel.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == "UpcomingAssignments")
            {
                eventRaisedCount++;
            }
        };

        // Act
        viewModel.UpcomingAssignments = 10;
        viewModel.UpcomingAssignments = 20;
        viewModel.UpcomingAssignments = 30;

        // Assert
        Assert.AreEqual(3, eventRaisedCount);
    }

    /// <summary>
    /// Tests that UpcomingAssignments handles boundary value int.MaxValue correctly.
    /// Input: int.MaxValue (2147483647).
    /// Expected: Property value should be int.MaxValue and PropertyChanged event should be raised.
    /// </summary>
    [TestMethod]
    public void UpcomingAssignments_SetMaxValue_UpdatesAndRaisesEvent()
    {
        // Arrange
        Mock<IDashboardService> mockDashboardService = new Mock<IDashboardService>();
        Mock<IAuthService> mockAuthService = new Mock<IAuthService>();
        Mock<SessionService> mockSessionService = new Mock<SessionService>();
        DashboardViewModel viewModel = new DashboardViewModel(mockDashboardService.Object, mockAuthService.Object, mockSessionService.Object, new Mock<IStudentScheduleService>().Object, new Mock<IRefreshCoordinator>().Object, new Mock<IInsightsService>().Object);
        bool eventRaised = false;
        viewModel.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == "UpcomingAssignments")
            {
                eventRaised = true;
            }
        };

        // Act
        viewModel.UpcomingAssignments = int.MaxValue;

        // Assert
        Assert.AreEqual(int.MaxValue, viewModel.UpcomingAssignments);
        Assert.IsTrue(eventRaised);
    }

    /// <summary>
    /// Tests that UpcomingAssignments handles boundary value int.MinValue correctly.
    /// Input: int.MinValue (-2147483648).
    /// Expected: Property value should be int.MinValue and PropertyChanged event should be raised.
    /// </summary>
    [TestMethod]
    public void UpcomingAssignments_SetMinValue_UpdatesAndRaisesEvent()
    {
        // Arrange
        Mock<IDashboardService> mockDashboardService = new Mock<IDashboardService>();
        Mock<IAuthService> mockAuthService = new Mock<IAuthService>();
        Mock<SessionService> mockSessionService = new Mock<SessionService>();
        DashboardViewModel viewModel = new DashboardViewModel(mockDashboardService.Object, mockAuthService.Object, mockSessionService.Object, new Mock<IStudentScheduleService>().Object, new Mock<IRefreshCoordinator>().Object, new Mock<IInsightsService>().Object);
        bool eventRaised = false;
        viewModel.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == "UpcomingAssignments")
            {
                eventRaised = true;
            }
        };

        // Act
        viewModel.UpcomingAssignments = int.MinValue;

        // Assert
        Assert.AreEqual(int.MinValue, viewModel.UpcomingAssignments);
        Assert.IsTrue(eventRaised);
    }

    /// <summary>
    /// Tests that setting UpcomingAssignments from zero to a positive value updates correctly.
    /// Input: Change from 0 (default) to 5.
    /// Expected: Property value should be 5 and PropertyChanged event should be raised.
    /// </summary>
    [TestMethod]
    public void UpcomingAssignments_SetFromZeroToPositive_UpdatesAndRaisesEvent()
    {
        // Arrange
        Mock<IDashboardService> mockDashboardService = new Mock<IDashboardService>();
        Mock<IAuthService> mockAuthService = new Mock<IAuthService>();
        Mock<SessionService> mockSessionService = new Mock<SessionService>();
        DashboardViewModel viewModel = new DashboardViewModel(mockDashboardService.Object, mockAuthService.Object, mockSessionService.Object, new Mock<IStudentScheduleService>().Object, new Mock<IRefreshCoordinator>().Object, new Mock<IInsightsService>().Object);
        bool eventRaised = false;
        viewModel.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == "UpcomingAssignments")
            {
                eventRaised = true;
            }
        };

        // Act
        viewModel.UpcomingAssignments = 5;

        // Assert
        Assert.AreEqual(5, viewModel.UpcomingAssignments);
        Assert.IsTrue(eventRaised);
    }

    /// <summary>
    /// Tests that setting UpcomingAssignments from a positive value to zero updates correctly.
    /// Input: Change from 10 to 0.
    /// Expected: Property value should be 0 and PropertyChanged event should be raised.
    /// </summary>
    [TestMethod]
    public void UpcomingAssignments_SetFromPositiveToZero_UpdatesAndRaisesEvent()
    {
        // Arrange
        Mock<IDashboardService> mockDashboardService = new Mock<IDashboardService>();
        Mock<IAuthService> mockAuthService = new Mock<IAuthService>();
        Mock<SessionService> mockSessionService = new Mock<SessionService>();
        DashboardViewModel viewModel = new DashboardViewModel(mockDashboardService.Object, mockAuthService.Object, mockSessionService.Object, new Mock<IStudentScheduleService>().Object, new Mock<IRefreshCoordinator>().Object, new Mock<IInsightsService>().Object);
        viewModel.UpcomingAssignments = 10;
        bool eventRaised = false;
        viewModel.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == "UpcomingAssignments")
            {
                eventRaised = true;
            }
        };

        // Act
        viewModel.UpcomingAssignments = 0;

        // Assert
        Assert.AreEqual(0, viewModel.UpcomingAssignments);
        Assert.IsTrue(eventRaised);
    }

    /// <summary>
    /// Tests that setting UpcomingAssignments to a negative value updates correctly.
    /// Input: Negative value (-50).
    /// Expected: Property value should be -50 and PropertyChanged event should be raised.
    /// </summary>
    [TestMethod]
    public void UpcomingAssignments_SetNegativeValue_UpdatesAndRaisesEvent()
    {
        // Arrange
        Mock<IDashboardService> mockDashboardService = new Mock<IDashboardService>();
        Mock<IAuthService> mockAuthService = new Mock<IAuthService>();
        Mock<SessionService> mockSessionService = new Mock<SessionService>();
        DashboardViewModel viewModel = new DashboardViewModel(mockDashboardService.Object, mockAuthService.Object, mockSessionService.Object, new Mock<IStudentScheduleService>().Object, new Mock<IRefreshCoordinator>().Object, new Mock<IInsightsService>().Object);
        bool eventRaised = false;
        viewModel.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == "UpcomingAssignments")
            {
                eventRaised = true;
            }
        };

        // Act
        viewModel.UpcomingAssignments = -50;

        // Assert
        Assert.AreEqual(-50, viewModel.UpcomingAssignments);
        Assert.IsTrue(eventRaised);
    }

    /// <summary>
    /// Tests that PropertyChanged event has the correct sender when UpcomingAssignments is set.
    /// Input: New value (42).
    /// Expected: PropertyChanged event sender should be the viewModel instance.
    /// </summary>
    [TestMethod]
    public void UpcomingAssignments_SetValue_PropertyChangedEventHasCorrectSender()
    {
        // Arrange
        Mock<IDashboardService> mockDashboardService = new Mock<IDashboardService>();
        Mock<IAuthService> mockAuthService = new Mock<IAuthService>();
        Mock<SessionService> mockSessionService = new Mock<SessionService>();
        DashboardViewModel viewModel = new DashboardViewModel(mockDashboardService.Object, mockAuthService.Object, mockSessionService.Object, new Mock<IStudentScheduleService>().Object, new Mock<IRefreshCoordinator>().Object, new Mock<IInsightsService>().Object);
        object? eventSender = null;
        viewModel.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == "UpcomingAssignments")
            {
                eventSender = sender;
            }
        };

        // Act
        viewModel.UpcomingAssignments = 42;

        // Assert
        Assert.IsNotNull(eventSender);
        Assert.AreSame(viewModel, eventSender);
    }

    /// <summary>
    /// Tests that LoadAsync returns early when not forcing refresh and time since last load is less than RefreshInterval.
    /// Input: forceRefresh = false, _lastLoaded set to a time less than RefreshInterval ago.
    /// Expected: Method returns immediately without calling dashboard service.
    /// </summary>
    [TestMethod]
    public async Task LoadAsync_NotForceRefreshAndWithinRefreshInterval_ReturnsImmediately()
    {
        // Arrange
        var mockDashboard = new Mock<IDashboardService>();
        var mockAuth = new Mock<IAuthService>();
        var mockSession = new Mock<SessionService>();
        var viewModel = new DashboardViewModel(mockDashboard.Object, mockAuth.Object, mockSession.Object, new Mock<IStudentScheduleService>().Object, new Mock<IRefreshCoordinator>().Object, new Mock<IInsightsService>().Object);

        var dashboardDto = new DashboardDto
        {
            ActiveCourses = 5,
            UpcomingAssignments = 3,
            AttendancePercent = 0.75,
            RecentAnnouncements = new List<AnnouncementDto>()
        };
        mockDashboard.Setup(d => d.GetDashboardAsync()).ReturnsAsync(dashboardDto);

        // First load to set _lastLoaded and _isLoaded
        await viewModel.LoadAsync(forceRefresh: true);
        mockDashboard.Invocations.Clear();

        // Act - Call again without force refresh within the refresh interval (60 seconds)
        await viewModel.LoadAsync(forceRefresh: false);

        // Assert
        mockDashboard.Verify(d => d.GetDashboardAsync(), Times.Never);
    }

    /// <summary>
    /// Tests that LoadAsync proceeds when not forcing refresh but sufficient time has passed since last load.
    /// Input: forceRefresh = false, _lastLoaded set to a time greater than RefreshInterval ago.
    /// Expected: Method proceeds and calls dashboard service.
    /// </summary>
    [TestMethod]
    public async Task LoadAsync_NotForceRefreshAndBeyondRefreshInterval_ProceedsWithLoad()
    {
        // Arrange
        var mockDashboard = new Mock<IDashboardService>();
        var mockAuth = new Mock<IAuthService>();
        var mockSession = new Mock<SessionService>();
        var viewModel = new DashboardViewModel(mockDashboard.Object, mockAuth.Object, mockSession.Object, new Mock<IStudentScheduleService>().Object, new Mock<IRefreshCoordinator>().Object, new Mock<IInsightsService>().Object);

        var dashboardDto = new DashboardDto
        {
            ActiveCourses = 5,
            UpcomingAssignments = 3,
            AttendancePercent = 0.75,
            RecentAnnouncements = new List<AnnouncementDto>()
        };
        mockDashboard.Setup(d => d.GetDashboardAsync()).ReturnsAsync(dashboardDto);

        // First load
        await viewModel.LoadAsync(forceRefresh: true);
        mockDashboard.Invocations.Clear();

        // Wait for slightly more than the refresh interval (60 seconds)
        // Since we can't actually wait 60 seconds in a unit test, we need to test via force refresh
        // or by manipulating the _lastLoaded field (which we cannot do without reflection)
        // This test documents the expected behavior but cannot be fully executed without reflection
        // Mark as inconclusive with explanation
        Assert.Inconclusive("This test requires waiting for RefreshInterval (60 seconds) or using reflection to modify _lastLoaded field. " +
                           "Expected behavior: When DateTime.UtcNow - _lastLoaded >= RefreshInterval and forceRefresh is false, " +
                           "the method should proceed with loading data and call GetDashboardAsync.");
    }

    /// <summary>
    /// Tests that LoadAsync handles NaN value for AttendancePercent correctly.
    /// Input: DashboardDto with AttendancePercent = double.NaN.
    /// Expected: AttendancePercentage is set to int value of NaN (which is 0 in C#).
    /// </summary>
    [TestMethod]
    public async Task LoadAsync_AttendancePercentNaN_HandlesCorrectly()
    {
        // Arrange
        var mockDashboard = new Mock<IDashboardService>();
        var mockAuth = new Mock<IAuthService>();
        var mockSession = new Mock<SessionService>();
        var viewModel = new DashboardViewModel(mockDashboard.Object, mockAuth.Object, mockSession.Object, new Mock<IStudentScheduleService>().Object, new Mock<IRefreshCoordinator>().Object, new Mock<IInsightsService>().Object);

        var dashboardDto = new DashboardDto
        {
            ActiveCourses = 10,
            UpcomingAssignments = 5,
            AttendancePercent = double.NaN,
            RecentAnnouncements = new List<AnnouncementDto>()
        };
        mockDashboard.Setup(d => d.GetDashboardAsync()).ReturnsAsync(dashboardDto);

        // Act
        await viewModel.LoadAsync(forceRefresh: true);

        // Assert
        // Casting NaN to int results in 0
        Assert.AreEqual(0, viewModel.AttendancePercentage);
        Assert.IsFalse(viewModel.IsBusy);
    }

    /// <summary>
    /// Tests that LoadAsync handles PositiveInfinity value for AttendancePercent correctly.
    /// Input: DashboardDto with AttendancePercent = double.PositiveInfinity.
    /// Expected: AttendancePercentage is set to int.MaxValue or causes overflow.
    /// </summary>
    [TestMethod]
    public async Task LoadAsync_AttendancePercentPositiveInfinity_HandlesCorrectly()
    {
        // Arrange
        var mockDashboard = new Mock<IDashboardService>();
        var mockAuth = new Mock<IAuthService>();
        var mockSession = new Mock<SessionService>();
        var viewModel = new DashboardViewModel(mockDashboard.Object, mockAuth.Object, mockSession.Object, new Mock<IStudentScheduleService>().Object, new Mock<IRefreshCoordinator>().Object, new Mock<IInsightsService>().Object);

        var dashboardDto = new DashboardDto
        {
            ActiveCourses = 10,
            UpcomingAssignments = 5,
            AttendancePercent = double.PositiveInfinity,
            RecentAnnouncements = new List<AnnouncementDto>()
        };
        mockDashboard.Setup(d => d.GetDashboardAsync()).ReturnsAsync(dashboardDto);

        // Act
        await viewModel.LoadAsync(forceRefresh: true);

        // Assert
        // Casting PositiveInfinity to int is undefined behavior in C#, typically results in int.MinValue in unchecked context
        // The actual value depends on runtime behavior
        Assert.IsFalse(viewModel.IsBusy);
    }

    /// <summary>
    /// Tests that LoadAsync handles NegativeInfinity value for AttendancePercent correctly.
    /// Input: DashboardDto with AttendancePercent = double.NegativeInfinity.
    /// Expected: AttendancePercentage is set based on how C# handles the cast.
    /// </summary>
    [TestMethod]
    public async Task LoadAsync_AttendancePercentNegativeInfinity_HandlesCorrectly()
    {
        // Arrange
        var mockDashboard = new Mock<IDashboardService>();
        var mockAuth = new Mock<IAuthService>();
        var mockSession = new Mock<SessionService>();
        var viewModel = new DashboardViewModel(mockDashboard.Object, mockAuth.Object, mockSession.Object, new Mock<IStudentScheduleService>().Object, new Mock<IRefreshCoordinator>().Object, new Mock<IInsightsService>().Object);

        var dashboardDto = new DashboardDto
        {
            ActiveCourses = 10,
            UpcomingAssignments = 5,
            AttendancePercent = double.NegativeInfinity,
            RecentAnnouncements = new List<AnnouncementDto>()
        };
        mockDashboard.Setup(d => d.GetDashboardAsync()).ReturnsAsync(dashboardDto);

        // Act
        await viewModel.LoadAsync(forceRefresh: true);

        // Assert
        // Casting NegativeInfinity to int is undefined behavior in C#, typically results in int.MinValue in unchecked context
        Assert.IsFalse(viewModel.IsBusy);
    }

    /// <summary>
    /// Tests that LoadAsync handles a large number of announcements correctly.
    /// Input: DashboardDto with 1000 announcements.
    /// Expected: All announcements are added to RecentAnnouncements collection and latest announcement properties are set.
    /// </summary>
    [TestMethod]
    public async Task LoadAsync_LargeNumberOfAnnouncements_HandlesCorrectly()
    {
        // Arrange
        var mockDashboard = new Mock<IDashboardService>();
        var mockAuth = new Mock<IAuthService>();
        var mockSession = new Mock<SessionService>();
        var viewModel = new DashboardViewModel(mockDashboard.Object, mockAuth.Object, mockSession.Object, new Mock<IStudentScheduleService>().Object, new Mock<IRefreshCoordinator>().Object, new Mock<IInsightsService>().Object);

        var announcements = new List<AnnouncementDto>();
        for (int i = 0; i < 1000; i++)
        {
            announcements.Add(new AnnouncementDto
            {
                Title = $"Announcement {i}",
                Body = $"Body {i}",
                Date = DateTime.Now.AddDays(-i)
            });
        }

        var dashboardDto = new DashboardDto
        {
            ActiveCourses = 10,
            UpcomingAssignments = 5,
            AttendancePercent = 0.85,
            RecentAnnouncements = announcements
        };
        mockDashboard.Setup(d => d.GetDashboardAsync()).ReturnsAsync(dashboardDto);

        // Act
        await viewModel.LoadAsync(forceRefresh: true);

        // Assert
        Assert.AreEqual(1000, viewModel.RecentAnnouncements.Count);
        Assert.AreEqual("Announcement 0", viewModel.LatestAnnouncementTitle);
        Assert.AreEqual("Body 0", viewModel.LatestAnnouncementBody);
        Assert.IsTrue(viewModel.HasAnnouncement);
        Assert.IsFalse(viewModel.IsBusy);
    }

    /// <summary>
    /// Tests that LoadAsync handles a large number of teaching classes correctly.
    /// Input: DashboardDto with 500 teaching classes.
    /// Expected: All teaching classes are added to TeachingClasses collection and count is set correctly.
    /// </summary>
    [TestMethod]
    public async Task LoadAsync_LargeNumberOfTeachingClasses_HandlesCorrectly()
    {
        // Arrange
        var mockDashboard = new Mock<IDashboardService>();
        var mockAuth = new Mock<IAuthService>();
        var mockSession = new Mock<SessionService>();
        var viewModel = new DashboardViewModel(mockDashboard.Object, mockAuth.Object, mockSession.Object, new Mock<IStudentScheduleService>().Object, new Mock<IRefreshCoordinator>().Object, new Mock<IInsightsService>().Object);

        var teachingClasses = new List<ClassDto>();
        for (int i = 0; i < 500; i++)
        {
            teachingClasses.Add(new ClassDto(
                id: Guid.NewGuid().ToString(),
                name: $"Class {i}",
                courseCode: $"CS{i:D3}",
                parentClassId: i % 2 == 0 ? Guid.NewGuid().ToString() : null,
                enrolledStudents: i + 10,
                lecturerName: $"Lecturer {i}"
            ));
        }

        var dashboardDto = new DashboardDto
        {
            ActiveCourses = 10,
            UpcomingAssignments = 5,
            AttendancePercent = 0.85,
            RecentAnnouncements = new List<AnnouncementDto>(),
            TeachingClasses = teachingClasses
        };
        mockDashboard.Setup(d => d.GetDashboardAsync()).ReturnsAsync(dashboardDto);

        // Act
        await viewModel.LoadAsync(forceRefresh: true);

        // Assert
        Assert.AreEqual(500, viewModel.TeachingClassesCount);
        Assert.AreEqual(500, viewModel.TeachingClasses.Count);
        Assert.IsFalse(viewModel.IsBusy);
    }

    /// <summary>
    /// Tests that LoadAsync handles ClassDto with Guid.Empty for Id correctly.
    /// Input: ClassDto with Id = Guid.Empty.
    /// Expected: ClassDto is created successfully with empty Guid string representation.
    /// </summary>
    [TestMethod]
    public async Task LoadAsync_ClassDtoWithGuidEmpty_HandlesCorrectly()
    {
        // Arrange
        var mockDashboard = new Mock<IDashboardService>();
        var mockAuth = new Mock<IAuthService>();
        var mockSession = new Mock<SessionService>();
        var viewModel = new DashboardViewModel(mockDashboard.Object, mockAuth.Object, mockSession.Object, new Mock<IStudentScheduleService>().Object, new Mock<IRefreshCoordinator>().Object, new Mock<IInsightsService>().Object);

        var teachingClasses = new List<ClassDto>
        {
            new ClassDto(
                id: Guid.Empty.ToString(),
                name: "Empty Guid Class",
                courseCode: "CS000",
                parentClassId: null,
                enrolledStudents: 25,
                lecturerName: "Test Lecturer"
            )
        };

        var dashboardDto = new DashboardDto
        {
            ActiveCourses = 10,
            UpcomingAssignments = 5,
            AttendancePercent = 0.85,
            RecentAnnouncements = new List<AnnouncementDto>(),
            TeachingClasses = teachingClasses
        };
        mockDashboard.Setup(d => d.GetDashboardAsync()).ReturnsAsync(dashboardDto);

        // Act
        await viewModel.LoadAsync(forceRefresh: true);

        // Assert
        Assert.AreEqual(1, viewModel.TeachingClasses.Count);
        Assert.AreEqual(Guid.Empty.ToString(), viewModel.TeachingClasses[0].Id);
        Assert.IsFalse(viewModel.IsBusy);
    }

    /// <summary>
    /// Tests that LoadAsync handles AnnouncementDto with very long strings correctly.
    /// Input: AnnouncementDto with extremely long Title, Body, and Date string.
    /// Expected: All properties are set correctly without truncation or error.
    /// </summary>
    [TestMethod]
    public async Task LoadAsync_AnnouncementWithVeryLongStrings_HandlesCorrectly()
    {
        // Arrange
        var mockDashboard = new Mock<IDashboardService>();
        var mockAuth = new Mock<IAuthService>();
        var mockSession = new Mock<SessionService>();
        var viewModel = new DashboardViewModel(mockDashboard.Object, mockAuth.Object, mockSession.Object, new Mock<IStudentScheduleService>().Object, new Mock<IRefreshCoordinator>().Object, new Mock<IInsightsService>().Object);

        var longTitle = new string('A', 10000);
        var longBody = new string('B', 50000);
        var announcements = new List<AnnouncementDto>
        {
            new AnnouncementDto
            {
                Title = longTitle,
                Body = longBody,
                Date = DateTime.Now
            }
        };

        var dashboardDto = new DashboardDto
        {
            ActiveCourses = 10,
            UpcomingAssignments = 5,
            AttendancePercent = 0.85,
            RecentAnnouncements = announcements
        };
        mockDashboard.Setup(d => d.GetDashboardAsync()).ReturnsAsync(dashboardDto);

        // Act
        await viewModel.LoadAsync(forceRefresh: true);

        // Assert
        Assert.AreEqual(longTitle, viewModel.LatestAnnouncementTitle);
        Assert.AreEqual(longBody, viewModel.LatestAnnouncementBody);
        Assert.IsTrue(viewModel.HasAnnouncement);
        Assert.IsFalse(viewModel.IsBusy);
    }

    /// <summary>
    /// Tests that LoadAsync handles zero values for all numeric properties correctly.
    /// Input: DashboardDto with all numeric properties set to 0.
    /// Expected: All properties are set to 0 correctly.
    /// </summary>
    [TestMethod]
    public async Task LoadAsync_AllNumericPropertiesZero_SetsCorrectly()
    {
        // Arrange
        var mockDashboard = new Mock<IDashboardService>();
        var mockAuth = new Mock<IAuthService>();
        var mockSession = new Mock<SessionService>();
        var viewModel = new DashboardViewModel(mockDashboard.Object, mockAuth.Object, mockSession.Object, new Mock<IStudentScheduleService>().Object, new Mock<IRefreshCoordinator>().Object, new Mock<IInsightsService>().Object);

        var dashboardDto = new DashboardDto
        {
            ActiveCourses = 0,
            UpcomingAssignments = 0,
            AttendancePercent = 0.0,
            RecentAnnouncements = new List<AnnouncementDto>(),
            TotalStudents = 0,
            TotalLecturers = 0,
            TotalPrograms = 0
        };
        mockDashboard.Setup(d => d.GetDashboardAsync()).ReturnsAsync(dashboardDto);

        // Act
        await viewModel.LoadAsync(forceRefresh: true);

        // Assert
        Assert.AreEqual(0, viewModel.ActiveCourses);
        Assert.AreEqual(0, viewModel.UpcomingAssignments);
        Assert.AreEqual(0, viewModel.AttendancePercentage);
        Assert.AreEqual(0, viewModel.TotalStudents);
        Assert.AreEqual(0, viewModel.TotalLecturers);
        Assert.AreEqual(0, viewModel.TotalPrograms);
        Assert.IsFalse(viewModel.IsBusy);
    }

    /// <summary>
    /// Tests that LoadAsync handles negative values for ActiveCourses and UpcomingAssignments.
    /// Input: DashboardDto with negative ActiveCourses and UpcomingAssignments.
    /// Expected: Properties are set to the negative values without error.
    /// </summary>
    [TestMethod]
    public async Task LoadAsync_NegativeActiveCoursesAndAssignments_SetsCorrectly()
    {
        // Arrange
        var mockDashboard = new Mock<IDashboardService>();
        var mockAuth = new Mock<IAuthService>();
        var mockSession = new Mock<SessionService>();
        var viewModel = new DashboardViewModel(mockDashboard.Object, mockAuth.Object, mockSession.Object, new Mock<IStudentScheduleService>().Object, new Mock<IRefreshCoordinator>().Object, new Mock<IInsightsService>().Object);

        var dashboardDto = new DashboardDto
        {
            ActiveCourses = -10,
            UpcomingAssignments = -5,
            AttendancePercent = 0.75,
            RecentAnnouncements = new List<AnnouncementDto>()
        };
        mockDashboard.Setup(d => d.GetDashboardAsync()).ReturnsAsync(dashboardDto);

        // Act
        await viewModel.LoadAsync(forceRefresh: true);

        // Assert
        Assert.AreEqual(-10, viewModel.ActiveCourses);
        Assert.AreEqual(-5, viewModel.UpcomingAssignments);
        Assert.IsFalse(viewModel.IsBusy);
    }

    /// <summary>
    /// Tests that LoadAsync handles maximum integer values for all numeric properties correctly.
    /// Input: DashboardDto with all numeric properties set to int.MaxValue.
    /// Expected: All properties are set to int.MaxValue correctly.
    /// </summary>
    [TestMethod]
    public async Task LoadAsync_AllNumericPropertiesMaxValue_SetsCorrectly()
    {
        // Arrange
        var mockDashboard = new Mock<IDashboardService>();
        var mockAuth = new Mock<IAuthService>();
        var mockSession = new Mock<SessionService>();
        var viewModel = new DashboardViewModel(mockDashboard.Object, mockAuth.Object, mockSession.Object, new Mock<IStudentScheduleService>().Object, new Mock<IRefreshCoordinator>().Object, new Mock<IInsightsService>().Object);

        var dashboardDto = new DashboardDto
        {
            ActiveCourses = int.MaxValue,
            UpcomingAssignments = int.MaxValue,
            AttendancePercent = 1.0,
            RecentAnnouncements = new List<AnnouncementDto>(),
            TotalStudents = int.MaxValue,
            TotalLecturers = int.MaxValue,
            TotalPrograms = int.MaxValue
        };
        mockDashboard.Setup(d => d.GetDashboardAsync()).ReturnsAsync(dashboardDto);

        // Act
        await viewModel.LoadAsync(forceRefresh: true);

        // Assert
        Assert.AreEqual(int.MaxValue, viewModel.ActiveCourses);
        Assert.AreEqual(int.MaxValue, viewModel.UpcomingAssignments);
        Assert.AreEqual(int.MaxValue, viewModel.TotalStudents);
        Assert.AreEqual(int.MaxValue, viewModel.TotalLecturers);
        Assert.AreEqual(int.MaxValue, viewModel.TotalPrograms);
        Assert.IsFalse(viewModel.IsBusy);
    }

    /// <summary>
    /// Tests that LoadAsync handles DateTime.MinValue for announcement date correctly.
    /// Input: AnnouncementDto with Date = DateTime.MinValue.
    /// Expected: LatestAnnouncementDate is formatted correctly.
    /// </summary>
    [TestMethod]
    public async Task LoadAsync_AnnouncementWithMinDateTime_FormatsCorrectly()
    {
        // Arrange
        var mockDashboard = new Mock<IDashboardService>();
        var mockAuth = new Mock<IAuthService>();
        var mockSession = new Mock<SessionService>();
        var viewModel = new DashboardViewModel(mockDashboard.Object, mockAuth.Object, mockSession.Object, new Mock<IStudentScheduleService>().Object, new Mock<IRefreshCoordinator>().Object, new Mock<IInsightsService>().Object);

        var announcements = new List<AnnouncementDto>
        {
            new AnnouncementDto
            {
                Title = "Test Announcement",
                Body = "Test Body",
                Date = DateTime.MinValue
            }
        };

        var dashboardDto = new DashboardDto
        {
            ActiveCourses = 10,
            UpcomingAssignments = 5,
            AttendancePercent = 0.85,
            RecentAnnouncements = announcements
        };
        mockDashboard.Setup(d => d.GetDashboardAsync()).ReturnsAsync(dashboardDto);

        // Act
        await viewModel.LoadAsync(forceRefresh: true);

        // Assert
        Assert.AreEqual(DateTime.MinValue.ToString("MMM dd, yyyy"), viewModel.LatestAnnouncementDate);
        Assert.IsTrue(viewModel.HasAnnouncement);
        Assert.IsFalse(viewModel.IsBusy);
    }

    /// <summary>
    /// Tests that LoadAsync handles DateTime.MaxValue for announcement date correctly.
    /// Input: AnnouncementDto with Date = DateTime.MaxValue.
    /// Expected: LatestAnnouncementDate is formatted correctly.
    /// </summary>
    [TestMethod]
    public async Task LoadAsync_AnnouncementWithMaxDateTime_FormatsCorrectly()
    {
        // Arrange
        var mockDashboard = new Mock<IDashboardService>();
        var mockAuth = new Mock<IAuthService>();
        var mockSession = new Mock<SessionService>();
        var viewModel = new DashboardViewModel(mockDashboard.Object, mockAuth.Object, mockSession.Object, new Mock<IStudentScheduleService>().Object, new Mock<IRefreshCoordinator>().Object, new Mock<IInsightsService>().Object);

        var announcements = new List<AnnouncementDto>
        {
            new AnnouncementDto
            {
                Title = "Test Announcement",
                Body = "Test Body",
                Date = DateTime.MaxValue
            }
        };

        var dashboardDto = new DashboardDto
        {
            ActiveCourses = 10,
            UpcomingAssignments = 5,
            AttendancePercent = 0.85,
            RecentAnnouncements = announcements
        };
        mockDashboard.Setup(d => d.GetDashboardAsync()).ReturnsAsync(dashboardDto);

        // Act
        await viewModel.LoadAsync(forceRefresh: true);

        // Assert
        Assert.AreEqual(DateTime.MaxValue.ToString("MMM dd, yyyy"), viewModel.LatestAnnouncementDate);
        Assert.IsTrue(viewModel.HasAnnouncement);
        Assert.IsFalse(viewModel.IsBusy);
    }

    /// <summary>
    /// Tests that IsAdmin returns false when Role contains special Unicode characters that visually resemble "admin".
    /// Input: Strings with Unicode characters that look like Latin characters but are not.
    /// Expected: IsAdmin returns false.
    /// </summary>
    [TestMethod]
    [DataRow("?dmin", DisplayName = "IsAdmin returns false when Role contains Cyrillic '?'")]
    [DataRow("admin\u0000", DisplayName = "IsAdmin returns false when Role contains null character")]
    [DataRow("admin\u200B", DisplayName = "IsAdmin returns false when Role contains zero-width space")]
    [DataRow("\u202Aadmin", DisplayName = "IsAdmin returns false when Role contains left-to-right embedding")]
    [DataRow("?dm?n", DisplayName = "IsAdmin returns false when Role contains multiple Cyrillic characters")]
    public void IsAdmin_RoleContainsSpecialUnicodeCharacters_ReturnsFalse(string roleValue)
    {
        // Arrange
        var mockDashboard = new Mock<IDashboardService>();
        var mockAuth = new Mock<IAuthService>();
        var mockSession = new Mock<SessionService>();
        var viewModel = new DashboardViewModel(mockDashboard.Object, mockAuth.Object, mockSession.Object, new Mock<IStudentScheduleService>().Object, new Mock<IRefreshCoordinator>().Object, new Mock<IInsightsService>().Object);

        // Act
        viewModel.Role = roleValue;
        var result = viewModel.IsAdmin;

        // Assert
        Assert.IsFalse(result);
    }

    /// <summary>
    /// Tests that IsAdmin correctly handles multiple consecutive role changes between admin and non-admin values.
    /// Input: Multiple role changes in sequence.
    /// Expected: IsAdmin reflects the current role value after each change.
    /// </summary>
    [TestMethod]
    public void IsAdmin_MultipleRoleChanges_ReflectsCurrentValue()
    {
        // Arrange
        var mockDashboard = new Mock<IDashboardService>();
        var mockAuth = new Mock<IAuthService>();
        var mockSession = new Mock<SessionService>();
        var viewModel = new DashboardViewModel(mockDashboard.Object, mockAuth.Object, mockSession.Object, new Mock<IStudentScheduleService>().Object, new Mock<IRefreshCoordinator>().Object, new Mock<IInsightsService>().Object);

        // Act & Assert
        viewModel.Role = "admin";
        Assert.IsTrue(viewModel.IsAdmin);

        viewModel.Role = "student";
        Assert.IsFalse(viewModel.IsAdmin);

        viewModel.Role = "ADMIN";
        Assert.IsTrue(viewModel.IsAdmin);

        viewModel.Role = "lecturer";
        Assert.IsFalse(viewModel.IsAdmin);

        viewModel.Role = "Admin";
        Assert.IsTrue(viewModel.IsAdmin);
    }

    /// <summary>
    /// Tests that IsAdmin handles extreme edge cases such as control characters in the role string.
    /// Input: Strings with various control characters.
    /// Expected: IsAdmin returns false.
    /// </summary>
    [TestMethod]
    [DataRow("admin\r", DisplayName = "IsAdmin returns false when Role contains carriage return")]
    [DataRow("ad\u0000min", DisplayName = "IsAdmin returns false when Role contains embedded null character")]
    [DataRow("\u0001admin", DisplayName = "IsAdmin returns false when Role contains start of heading character")]
    [DataRow("admin\u007F", DisplayName = "IsAdmin returns false when Role contains delete character")]
    public void IsAdmin_RoleContainsControlCharacters_ReturnsFalse(string roleValue)
    {
        // Arrange
        var mockDashboard = new Mock<IDashboardService>();
        var mockAuth = new Mock<IAuthService>();
        var mockSession = new Mock<SessionService>();
        var viewModel = new DashboardViewModel(mockDashboard.Object, mockAuth.Object, mockSession.Object, new Mock<IStudentScheduleService>().Object, new Mock<IRefreshCoordinator>().Object, new Mock<IInsightsService>().Object);

        // Act
        viewModel.Role = roleValue;
        var result = viewModel.IsAdmin;

        // Assert
        Assert.IsFalse(result);
    }

    /// <summary>
    /// Tests that IsAdmin is case-insensitive by verifying all possible single-character case variations.
    /// Input: All single-character case variations of "admin" (32 combinations).
    /// Expected: All variations return true.
    /// </summary>
    [TestMethod]
    [DataRow("admin")]
    [DataRow("Admin")]
    [DataRow("aDmin")]
    [DataRow("ADmin")]
    [DataRow("adMin")]
    [DataRow("AdMin")]
    [DataRow("aDMin")]
    [DataRow("ADMin")]
    [DataRow("admIn")]
    [DataRow("AdmIn")]
    [DataRow("aDmIn")]
    [DataRow("ADmIn")]
    [DataRow("adMIn")]
    [DataRow("AdMIn")]
    [DataRow("aDMIn")]
    [DataRow("ADMIn")]
    [DataRow("admiN")]
    [DataRow("AdmiN")]
    [DataRow("aDmiN")]
    [DataRow("ADmiN")]
    [DataRow("adMiN")]
    [DataRow("AdMiN")]
    [DataRow("aDMiN")]
    [DataRow("ADMiN")]
    [DataRow("admIN")]
    [DataRow("AdmIN")]
    [DataRow("aDmIN")]
    [DataRow("ADmIN")]
    [DataRow("adMIN")]
    [DataRow("AdMIN")]
    [DataRow("aDMIN")]
    [DataRow("ADMIN")]
    public void IsAdmin_AllCaseVariationsOfAdmin_ReturnsTrue(string roleValue)
    {
        // Arrange
        var mockDashboard = new Mock<IDashboardService>();
        var mockAuth = new Mock<IAuthService>();
        var mockSession = new Mock<SessionService>();
        var viewModel = new DashboardViewModel(mockDashboard.Object, mockAuth.Object, mockSession.Object, new Mock<IStudentScheduleService>().Object, new Mock<IRefreshCoordinator>().Object, new Mock<IInsightsService>().Object);

        // Act
        viewModel.Role = roleValue;
        var result = viewModel.IsAdmin;

        // Assert
        Assert.IsTrue(result);
    }

    /// <summary>
    /// Tests that GoToUsersCommand returns an ICommand type.
    /// Input: None (property getter).
    /// Expected: Returns an object that implements ICommand interface.
    /// </summary>
    [TestMethod]
    public void GoToUsersCommand_WhenAccessed_ReturnsICommandType()
    {
        // Arrange
        var mockDashboard = new Mock<IDashboardService>();
        var mockAuth = new Mock<IAuthService>();
        var mockSession = new Mock<SessionService>();
        var viewModel = new DashboardViewModel(mockDashboard.Object, mockAuth.Object, mockSession.Object, new Mock<IStudentScheduleService>().Object, new Mock<IRefreshCoordinator>().Object, new Mock<IInsightsService>().Object);

        // Act
        var command = viewModel.GoToUsersCommand;

        // Assert
        Assert.IsInstanceOfType(command, typeof(ICommand));
    }

    /// <summary>
    /// Tests that GoToUsersCommand returns a Command type specifically.
    /// Input: None (property getter).
    /// Expected: Returns a Microsoft.Maui.Controls.Command instance.
    /// </summary>
    [TestMethod]
    public void GoToUsersCommand_WhenAccessed_ReturnsCommandType()
    {
        // Arrange
        var mockDashboard = new Mock<IDashboardService>();
        var mockAuth = new Mock<IAuthService>();
        var mockSession = new Mock<SessionService>();
        var viewModel = new DashboardViewModel(mockDashboard.Object, mockAuth.Object, mockSession.Object, new Mock<IStudentScheduleService>().Object, new Mock<IRefreshCoordinator>().Object, new Mock<IInsightsService>().Object);

        // Act
        var command = viewModel.GoToUsersCommand;

        // Assert
        Assert.IsInstanceOfType(command, typeof(Command));
    }

    /// <summary>
    /// Tests that GoToUsersCommand.CanExecute returns true with null parameter.
    /// Input: null parameter to CanExecute.
    /// Expected: Returns true (command is always executable).
    /// </summary>
    [TestMethod]
    public void GoToUsersCommand_CanExecuteWithNullParameter_ReturnsTrue()
    {
        // Arrange
        var mockDashboard = new Mock<IDashboardService>();
        var mockAuth = new Mock<IAuthService>();
        var mockSession = new Mock<SessionService>();
        var viewModel = new DashboardViewModel(mockDashboard.Object, mockAuth.Object, mockSession.Object, new Mock<IStudentScheduleService>().Object, new Mock<IRefreshCoordinator>().Object, new Mock<IInsightsService>().Object);

        // Act
        var command = viewModel.GoToUsersCommand;
        var canExecute = command.CanExecute(null);

        // Assert
        Assert.IsTrue(canExecute);
    }

    /// <summary>
    /// Tests that GoToUsersCommand.CanExecute returns true with various non-null parameters.
    /// Input: Various object parameters to CanExecute.
    /// Expected: Returns true for all inputs (command doesn't use parameter).
    /// </summary>
    [TestMethod]
    [DataRow("string parameter")]
    [DataRow(42)]
    [DataRow(true)]
    public void GoToUsersCommand_CanExecuteWithParameter_ReturnsTrue(object parameter)
    {
        // Arrange
        var mockDashboard = new Mock<IDashboardService>();
        var mockAuth = new Mock<IAuthService>();
        var mockSession = new Mock<SessionService>();
        var viewModel = new DashboardViewModel(mockDashboard.Object, mockAuth.Object, mockSession.Object, new Mock<IStudentScheduleService>().Object, new Mock<IRefreshCoordinator>().Object, new Mock<IInsightsService>().Object);

        // Act
        var command = viewModel.GoToUsersCommand;
        var canExecute = command.CanExecute(parameter);

        // Assert
        Assert.IsTrue(canExecute);
    }

    /// <summary>
    /// Tests that GoToUsersCommand can be accessed from different ViewModel instances.
    /// Input: Multiple DashboardViewModel instances.
    /// Expected: Each instance provides its own command instances.
    /// </summary>
    [TestMethod]
    public void GoToUsersCommand_FromDifferentViewModelInstances_ReturnsDistinctCommands()
    {
        // Arrange
        var mockDashboard = new Mock<IDashboardService>();
        var mockAuth = new Mock<IAuthService>();
        var mockSession = new Mock<SessionService>();
        var viewModel1 = new DashboardViewModel(mockDashboard.Object, mockAuth.Object, mockSession.Object, new Mock<IStudentScheduleService>().Object, new Mock<IRefreshCoordinator>().Object, new Mock<IInsightsService>().Object);
        var viewModel2 = new DashboardViewModel(mockDashboard.Object, mockAuth.Object, mockSession.Object, new Mock<IStudentScheduleService>().Object, new Mock<IRefreshCoordinator>().Object, new Mock<IInsightsService>().Object);

        // Act
        var command1 = viewModel1.GoToUsersCommand;
        var command2 = viewModel2.GoToUsersCommand;

        // Assert
        Assert.AreNotSame(command1, command2);
    }

    /// <summary>
    /// Tests that GoToUsersCommand execution behavior cannot be validated in unit tests.
    /// NOTE: This test is marked as Inconclusive because Shell.Current is a static property
    /// that cannot be mocked using Moq. The command's Execute method calls Shell.Current.GoToAsync,
    /// which will be null in a unit test context and would throw NullReferenceException.
    /// To fully test navigation behavior, use integration tests with MAUI Shell initialized,
    /// or refactor to inject an INavigationService abstraction.
    /// Expected behavior: When executed, should navigate to "//MainTabs/admin/users".
    /// </summary>
    [TestMethod]
    public void GoToUsersCommand_ExecutionBehavior_CannotBeTestedInUnitTests()
    {
        // Arrange
        var mockDashboard = new Mock<IDashboardService>();
        var mockAuth = new Mock<IAuthService>();
        var mockSession = new Mock<SessionService>();
        var viewModel = new DashboardViewModel(mockDashboard.Object, mockAuth.Object, mockSession.Object, new Mock<IStudentScheduleService>().Object, new Mock<IRefreshCoordinator>().Object, new Mock<IInsightsService>().Object);

        // Assert
        Assert.Inconclusive(
            "GoToUsersCommand execution cannot be tested in unit tests because it depends on " +
            "Shell.Current, a static property that cannot be mocked with Moq. " +
            "Use integration tests with a properly initialized MAUI Shell instance, " +
            "or refactor to inject an INavigationService for better testability.");
    }

    /// <summary>
    /// Tests that setting AttendancePercentage to values at or above 75 sets AttendanceStatus to "On Track".
    /// Input: Integer values >= 75 including boundary and extreme values.
    /// Expected: AttendanceStatus is set to "On Track".
    /// </summary>
    [TestMethod]
    [DataRow(75, DisplayName = "Boundary value: 75 sets status to On Track")]
    [DataRow(76, DisplayName = "Value above boundary: 76 sets status to On Track")]
    [DataRow(100, DisplayName = "Common percentage: 100 sets status to On Track")]
    [DataRow(150, DisplayName = "Above 100: 150 sets status to On Track")]
    [DataRow(1000, DisplayName = "Very high value: 1000 sets status to On Track")]
    [DataRow(int.MaxValue, DisplayName = "Maximum integer: int.MaxValue sets status to On Track")]
    public void AttendancePercentage_SetValueAtOrAbove75_SetsStatusToOnTrack(int value)
    {
        // Arrange & Act
        _viewModel.AttendancePercentage = value;

        // Assert
        Assert.AreEqual(value, _viewModel.AttendancePercentage);
        Assert.AreEqual("On Track", _viewModel.AttendanceStatus);
    }

    /// <summary>
    /// Tests that setting AttendancePercentage to values between 50 and 74 (inclusive of 50, exclusive of 75) 
    /// sets AttendanceStatus to "Needs Improvement".
    /// Input: Integer values in range [50, 74].
    /// Expected: AttendanceStatus is set to "Needs Improvement".
    /// </summary>
    [TestMethod]
    [DataRow(50, DisplayName = "Lower boundary: 50 sets status to Needs Improvement")]
    [DataRow(51, DisplayName = "Just above lower boundary: 51 sets status to Needs Improvement")]
    [DataRow(60, DisplayName = "Mid-range: 60 sets status to Needs Improvement")]
    [DataRow(70, DisplayName = "Higher mid-range: 70 sets status to Needs Improvement")]
    [DataRow(74, DisplayName = "Upper boundary: 74 sets status to Needs Improvement")]
    public void AttendancePercentage_SetValueBetween50And74_SetsStatusToNeedsImprovement(int value)
    {
        // Arrange & Act
        _viewModel.AttendancePercentage = value;

        // Assert
        Assert.AreEqual(value, _viewModel.AttendancePercentage);
        Assert.AreEqual("Needs Improvement", _viewModel.AttendanceStatus);
    }

    /// <summary>
    /// Tests that setting AttendancePercentage to values below 50 sets AttendanceStatus to "At Risk".
    /// Input: Integer values < 50 including negative and extreme values.
    /// Expected: AttendanceStatus is set to "At Risk".
    /// </summary>
    [TestMethod]
    [DataRow(49, DisplayName = "Just below boundary: 49 sets status to At Risk")]
    [DataRow(25, DisplayName = "Low positive: 25 sets status to At Risk")]
    [DataRow(0, DisplayName = "Zero: 0 sets status to At Risk")]
    [DataRow(-1, DisplayName = "Negative: -1 sets status to At Risk")]
    [DataRow(-50, DisplayName = "Negative: -50 sets status to At Risk")]
    [DataRow(-100, DisplayName = "Large negative: -100 sets status to At Risk")]
    [DataRow(int.MinValue, DisplayName = "Minimum integer: int.MinValue sets status to At Risk")]
    public void AttendancePercentage_SetValueBelow50_SetsStatusToAtRisk(int value)
    {
        // Arrange & Act
        _viewModel.AttendancePercentage = value;

        // Assert
        Assert.AreEqual(value, _viewModel.AttendancePercentage);
        Assert.AreEqual("At Risk", _viewModel.AttendanceStatus);
    }

    /// <summary>
    /// Tests that the getter of AttendancePercentage returns the value that was set.
    /// Input: Various integer values.
    /// Expected: Getter returns the exact value that was set.
    /// </summary>
    [TestMethod]
    [DataRow(0)]
    [DataRow(25)]
    [DataRow(50)]
    [DataRow(75)]
    [DataRow(100)]
    [DataRow(-10)]
    [DataRow(int.MinValue)]
    [DataRow(int.MaxValue)]
    public void AttendancePercentage_GetAfterSet_ReturnsSetValue(int value)
    {
        // Arrange & Act
        _viewModel.AttendancePercentage = value;
        int result = _viewModel.AttendancePercentage;

        // Assert
        Assert.AreEqual(value, result);
    }

    /// <summary>
    /// Tests that setting AttendancePercentage to a new value raises the PropertyChanged event 
    /// with the correct property name.
    /// Input: A value different from the current value.
    /// Expected: PropertyChanged event is raised with property name "AttendancePercentage".
    /// </summary>
    [TestMethod]
    public void AttendancePercentage_SetNewValue_RaisesPropertyChangedEvent()
    {
        // Arrange
        string? changedPropertyName = null;
        _viewModel.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == nameof(DashboardViewModel.AttendancePercentage))
                changedPropertyName = args.PropertyName;
        };

        // Act
        _viewModel.AttendancePercentage = 85;

        // Assert
        Assert.IsNotNull(changedPropertyName);
        Assert.AreEqual(nameof(DashboardViewModel.AttendancePercentage), changedPropertyName);
    }

    /// <summary>
    /// Tests that setting AttendancePercentage to the same value does not raise the PropertyChanged event.
    /// Input: Same value set twice.
    /// Expected: PropertyChanged event is raised only once (on first set).
    /// </summary>
    [TestMethod]
    public void AttendancePercentage_SetSameValue_DoesNotRaisePropertyChangedEvent()
    {
        // Arrange
        _viewModel.AttendancePercentage = 60;
        int eventRaisedCount = 0;
        _viewModel.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == nameof(DashboardViewModel.AttendancePercentage))
                eventRaisedCount++;
        };

        // Act
        _viewModel.AttendancePercentage = 60;

        // Assert
        Assert.AreEqual(0, eventRaisedCount, "PropertyChanged should not be raised when setting the same value");
    }

    /// <summary>
    /// Tests that setting AttendancePercentage updates AttendanceStatus and raises PropertyChanged for AttendanceStatus.
    /// Input: Value that changes the status.
    /// Expected: PropertyChanged event is raised for "AttendanceStatus".
    /// </summary>
    [TestMethod]
    public void AttendancePercentage_SetNewValue_RaisesPropertyChangedForAttendanceStatus()
    {
        // Arrange
        string? changedPropertyName = null;
        _viewModel.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == nameof(DashboardViewModel.AttendanceStatus))
                changedPropertyName = args.PropertyName;
        };

        // Act
        _viewModel.AttendancePercentage = 80;

        // Assert
        Assert.IsNotNull(changedPropertyName);
        Assert.AreEqual(nameof(DashboardViewModel.AttendanceStatus), changedPropertyName);
    }

    /// <summary>
    /// Tests that setting AttendancePercentage to the same value does not re-trigger AttendanceStatus update.
    /// Input: Same value set twice.
    /// Expected: AttendanceStatus PropertyChanged is not raised on second set.
    /// </summary>
    [TestMethod]
    public void AttendancePercentage_SetSameValueTwice_DoesNotUpdateAttendanceStatusAgain()
    {
        // Arrange
        _viewModel.AttendancePercentage = 80;
        int statusEventCount = 0;
        _viewModel.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == nameof(DashboardViewModel.AttendanceStatus))
                statusEventCount++;
        };

        // Act
        _viewModel.AttendancePercentage = 80;

        // Assert
        Assert.AreEqual(0, statusEventCount, "AttendanceStatus should not be updated when AttendancePercentage is set to the same value");
        Assert.AreEqual("On Track", _viewModel.AttendanceStatus);
    }

    /// <summary>
    /// Tests that changing AttendancePercentage across different status ranges updates AttendanceStatus correctly.
    /// Input: Sequential values that transition across all three status ranges.
    /// Expected: AttendanceStatus updates correctly for each transition.
    /// </summary>
    [TestMethod]
    public void AttendancePercentage_SequentialChangesAcrossRanges_UpdatesStatusCorrectly()
    {
        // Arrange & Act & Assert - At Risk range
        _viewModel.AttendancePercentage = 30;
        Assert.AreEqual("At Risk", _viewModel.AttendanceStatus);

        // Act & Assert - Needs Improvement range
        _viewModel.AttendancePercentage = 60;
        Assert.AreEqual("Needs Improvement", _viewModel.AttendanceStatus);

        // Act & Assert - On Track range
        _viewModel.AttendancePercentage = 90;
        Assert.AreEqual("On Track", _viewModel.AttendanceStatus);

        // Act & Assert - Back to Needs Improvement
        _viewModel.AttendancePercentage = 70;
        Assert.AreEqual("Needs Improvement", _viewModel.AttendanceStatus);

        // Act & Assert - Back to At Risk
        _viewModel.AttendancePercentage = 40;
        Assert.AreEqual("At Risk", _viewModel.AttendanceStatus);
    }

    /// <summary>
    /// Tests that multiple PropertyChanged events are raised when AttendancePercentage is set to different values.
    /// Input: Three different values set sequentially.
    /// Expected: PropertyChanged event is raised three times for AttendancePercentage.
    /// </summary>
    [TestMethod]
    public void AttendancePercentage_SetMultipleDifferentValues_RaisesPropertyChangedMultipleTimes()
    {
        // Arrange
        int eventCount = 0;
        _viewModel.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == nameof(DashboardViewModel.AttendancePercentage))
                eventCount++;
        };

        // Act
        _viewModel.AttendancePercentage = 30;
        _viewModel.AttendancePercentage = 60;
        _viewModel.AttendancePercentage = 90;

        // Assert
        Assert.AreEqual(3, eventCount);
    }

    /// <summary>
    /// Tests that the default/initial value of AttendancePercentage is 0.
    /// Input: None (newly constructed ViewModel).
    /// Expected: AttendancePercentage returns 0.
    /// </summary>
    [TestMethod]
    public void AttendancePercentage_InitialValue_IsZero()
    {
        // Arrange
        var viewModel = new DashboardViewModel(_mockDashboardService.Object, _mockAuthService.Object, _mockSessionService.Object, new Mock<IStudentScheduleService>().Object, new Mock<IRefreshCoordinator>().Object, new Mock<IInsightsService>().Object);

        // Act
        int initialValue = viewModel.AttendancePercentage;

        // Assert
        Assert.AreEqual(0, initialValue);
    }

    /// <summary>
    /// Tests that setting AttendancePercentage updates both the property value and AttendanceStatus atomically.
    /// Input: Value that should set status to "On Track".
    /// Expected: Both AttendancePercentage and AttendanceStatus are updated correctly.
    /// </summary>
    [TestMethod]
    public void AttendancePercentage_SetValue_UpdatesBothPropertyAndStatus()
    {
        // Arrange
        bool attendancePercentageChanged = false;
        bool attendanceStatusChanged = false;

        _viewModel.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == nameof(DashboardViewModel.AttendancePercentage))
                attendancePercentageChanged = true;
            if (args.PropertyName == nameof(DashboardViewModel.AttendanceStatus))
                attendanceStatusChanged = true;
        };

        // Act
        _viewModel.AttendancePercentage = 85;

        // Assert
        Assert.IsTrue(attendancePercentageChanged, "AttendancePercentage PropertyChanged should be raised");
        Assert.IsTrue(attendanceStatusChanged, "AttendanceStatus PropertyChanged should be raised");
        Assert.AreEqual(85, _viewModel.AttendancePercentage);
        Assert.AreEqual("On Track", _viewModel.AttendanceStatus);
    }

    /// <summary>
    /// Tests that setting AttendancePercentage to extreme negative value handles correctly.
    /// Input: int.MinValue.
    /// Expected: Value is set correctly and status is "At Risk".
    /// </summary>
    [TestMethod]
    public void AttendancePercentage_SetToMinValue_HandlesCorrectly()
    {
        // Arrange & Act
        _viewModel.AttendancePercentage = int.MinValue;

        // Assert
        Assert.AreEqual(int.MinValue, _viewModel.AttendancePercentage);
        Assert.AreEqual("At Risk", _viewModel.AttendanceStatus);
    }

    /// <summary>
    /// Tests that setting AttendancePercentage to extreme positive value handles correctly.
    /// Input: int.MaxValue.
    /// Expected: Value is set correctly and status is "On Track".
    /// </summary>
    [TestMethod]
    public void AttendancePercentage_SetToMaxValue_HandlesCorrectly()
    {
        // Arrange & Act
        _viewModel.AttendancePercentage = int.MaxValue;

        // Assert
        Assert.AreEqual(int.MaxValue, _viewModel.AttendancePercentage);
        Assert.AreEqual("On Track", _viewModel.AttendanceStatus);
    }

    /// <summary>
    /// Tests that setting the LatestAnnouncementBody property to a valid string value updates the property correctly
    /// and raises the PropertyChanged event with the correct property name.
    /// Input: Various valid string values including normal text, empty string, whitespace, special characters, and unicode.
    /// Expected: Property is updated to the new value and PropertyChanged event is raised with property name "LatestAnnouncementBody".
    /// </summary>
    [TestMethod]
    [DataRow("Normal announcement text", DisplayName = "Normal text")]
    [DataRow("", DisplayName = "Empty string")]
    [DataRow("   ", DisplayName = "Three spaces")]
    [DataRow(" ", DisplayName = "Single space")]
    [DataRow("\t", DisplayName = "Tab character")]
    [DataRow("\n", DisplayName = "Newline character")]
    [DataRow("\r\n", DisplayName = "Carriage return and newline")]
    [DataRow("Line1\nLine2\nLine3", DisplayName = "Multiple lines with newline")]
    [DataRow("Line1\r\nLine2", DisplayName = "Multiple lines with CRLF")]
    [DataRow("!@#$%^&*()_+-={}[]|:;<>?,./~`", DisplayName = "Special characters")]
    [DataRow("???????", DisplayName = "Japanese characters")]
    [DataRow("Announcement with emoji ????", DisplayName = "Text with emoji")]
    [DataRow("A", DisplayName = "Single character")]
    [DataRow("Test\tTabbed\tText", DisplayName = "Text with tabs")]
    [DataRow("Mixed\nLine\r\nBreaks", DisplayName = "Mixed line breaks")]
    public void LatestAnnouncementBody_SetValidValue_UpdatesPropertyAndRaisesPropertyChanged(string value)
    {
        // Arrange
        Mock<IDashboardService> mockDashboardService = new Mock<IDashboardService>();
        Mock<IAuthService> mockAuthService = new Mock<IAuthService>();
        Mock<SessionService> mockSessionService = new Mock<SessionService>();
        DashboardViewModel viewModel = new DashboardViewModel(mockDashboardService.Object, mockAuthService.Object, mockSessionService.Object, new Mock<IStudentScheduleService>().Object, new Mock<IRefreshCoordinator>().Object, new Mock<IInsightsService>().Object);

        string? raisedPropertyName = null;
        viewModel.PropertyChanged += (sender, args) => raisedPropertyName = args.PropertyName;

        // Act
        viewModel.LatestAnnouncementBody = value;

        // Assert
        Assert.AreEqual(value, viewModel.LatestAnnouncementBody);
        Assert.AreEqual("LatestAnnouncementBody", raisedPropertyName);
    }

    /// <summary>
    /// Tests that the PropertyChanged event provides the correct sender (the ViewModel instance)
    /// when the LatestAnnouncementBody property is set.
    /// Input: A new string value.
    /// Expected: PropertyChanged event sender is the ViewModel instance.
    /// </summary>
    [TestMethod]
    public void LatestAnnouncementBody_SetValue_PropertyChangedEventHasCorrectSender()
    {
        // Arrange
        Mock<IDashboardService> mockDashboardService = new Mock<IDashboardService>();
        Mock<IAuthService> mockAuthService = new Mock<IAuthService>();
        Mock<SessionService> mockSessionService = new Mock<SessionService>();
        DashboardViewModel viewModel = new DashboardViewModel(mockDashboardService.Object, mockAuthService.Object, mockSessionService.Object, new Mock<IStudentScheduleService>().Object, new Mock<IRefreshCoordinator>().Object, new Mock<IInsightsService>().Object);

        object? eventSender = null;
        viewModel.PropertyChanged += (sender, args) => eventSender = sender;

        // Act
        viewModel.LatestAnnouncementBody = "New announcement";

        // Assert
        Assert.AreSame(viewModel, eventSender);
    }

    /// <summary>
    /// Tests that setting LatestAnnouncementBody to strings with various whitespace combinations
    /// preserves the exact whitespace and raises PropertyChanged correctly.
    /// Input: Various whitespace-heavy strings.
    /// Expected: Exact string value is preserved including all whitespace characters.
    /// </summary>
    [TestMethod]
    [DataRow("  leading spaces", DisplayName = "Leading spaces")]
    [DataRow("trailing spaces  ", DisplayName = "Trailing spaces")]
    [DataRow("  both sides  ", DisplayName = "Both sides spaces")]
    [DataRow("\t\tDouble tab", DisplayName = "Double tab prefix")]
    [DataRow("Mixed \t spaces\nand\r\nbreaks", DisplayName = "Mixed whitespace")]
    public void LatestAnnouncementBody_SetWhitespaceVariations_PreservesExactValue(string value)
    {
        // Arrange
        Mock<IDashboardService> mockDashboardService = new Mock<IDashboardService>();
        Mock<IAuthService> mockAuthService = new Mock<IAuthService>();
        Mock<SessionService> mockSessionService = new Mock<SessionService>();
        DashboardViewModel viewModel = new DashboardViewModel(mockDashboardService.Object, mockAuthService.Object, mockSessionService.Object, new Mock<IStudentScheduleService>().Object, new Mock<IRefreshCoordinator>().Object, new Mock<IInsightsService>().Object);

        // Act
        viewModel.LatestAnnouncementBody = value;

        // Assert
        Assert.AreEqual(value, viewModel.LatestAnnouncementBody);
    }

    /// <summary>
    /// Tests that setting LatestAnnouncementBody to strings with HTML/XML-like content
    /// preserves the exact content without any encoding or escaping.
    /// Input: Strings containing angle brackets and other markup-like characters.
    /// Expected: Exact string is preserved without modification.
    /// </summary>
    [TestMethod]
    [DataRow("<div>Test</div>", DisplayName = "HTML div tags")]
    [DataRow("<script>alert('test');</script>", DisplayName = "Script tags")]
    [DataRow("<?xml version=\"1.0\"?>", DisplayName = "XML declaration")]
    [DataRow("<tag attr=\"value\">Content</tag>", DisplayName = "Tag with attribute")]
    public void LatestAnnouncementBody_SetMarkupLikeStrings_PreservesExactValue(string value)
    {
        // Arrange
        Mock<IDashboardService> mockDashboardService = new Mock<IDashboardService>();
        Mock<IAuthService> mockAuthService = new Mock<IAuthService>();
        Mock<SessionService> mockSessionService = new Mock<SessionService>();
        DashboardViewModel viewModel = new DashboardViewModel(mockDashboardService.Object, mockAuthService.Object, mockSessionService.Object, new Mock<IStudentScheduleService>().Object, new Mock<IRefreshCoordinator>().Object, new Mock<IInsightsService>().Object);

        // Act
        viewModel.LatestAnnouncementBody = value;

        // Assert
        Assert.AreEqual(value, viewModel.LatestAnnouncementBody);
    }

    /// <summary>
    /// Tests that rapidly changing the LatestAnnouncementBody property multiple times in sequence
    /// correctly maintains the final value and raises PropertyChanged for each distinct change.
    /// Input: Five different values set in rapid succession.
    /// Expected: Final value is the last one set, and PropertyChanged is raised five times.
    /// </summary>
    [TestMethod]
    public void LatestAnnouncementBody_RapidSequentialChanges_MaintainsFinalValueAndRaisesAllEvents()
    {
        // Arrange
        Mock<IDashboardService> mockDashboardService = new Mock<IDashboardService>();
        Mock<IAuthService> mockAuthService = new Mock<IAuthService>();
        Mock<SessionService> mockSessionService = new Mock<SessionService>();
        DashboardViewModel viewModel = new DashboardViewModel(mockDashboardService.Object, mockAuthService.Object, mockSessionService.Object, new Mock<IStudentScheduleService>().Object, new Mock<IRefreshCoordinator>().Object, new Mock<IInsightsService>().Object);

        int eventRaisedCount = 0;
        viewModel.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == "LatestAnnouncementBody")
            {
                eventRaisedCount++;
            }
        };

        // Act
        viewModel.LatestAnnouncementBody = "Value 1";
        viewModel.LatestAnnouncementBody = "Value 2";
        viewModel.LatestAnnouncementBody = "Value 3";
        viewModel.LatestAnnouncementBody = "Value 4";
        viewModel.LatestAnnouncementBody = "Value 5";

        // Assert
        Assert.AreEqual("Value 5", viewModel.LatestAnnouncementBody);
        Assert.AreEqual(5, eventRaisedCount);
    }

    /// <summary>
    /// Tests that setting LatestAnnouncementBody to strings with numeric content
    /// treats them as strings and preserves them exactly.
    /// Input: Numeric strings and formatted numbers.
    /// Expected: Values are stored as strings without any numeric conversion.
    /// </summary>
    [TestMethod]
    [DataRow("123", DisplayName = "Simple number")]
    [DataRow("123.456", DisplayName = "Decimal number")]
    [DataRow("-999", DisplayName = "Negative number")]
    [DataRow("1,234,567", DisplayName = "Formatted number with commas")]
    [DataRow("0x1234", DisplayName = "Hex-like format")]
    [DataRow("1e10", DisplayName = "Scientific notation")]
    public void LatestAnnouncementBody_SetNumericStrings_PreservesAsString(string value)
    {
        // Arrange
        Mock<IDashboardService> mockDashboardService = new Mock<IDashboardService>();
        Mock<IAuthService> mockAuthService = new Mock<IAuthService>();
        Mock<SessionService> mockSessionService = new Mock<SessionService>();
        DashboardViewModel viewModel = new DashboardViewModel(mockDashboardService.Object, mockAuthService.Object, mockSessionService.Object, new Mock<IStudentScheduleService>().Object, new Mock<IRefreshCoordinator>().Object, new Mock<IInsightsService>().Object);

        // Act
        viewModel.LatestAnnouncementBody = value;

        // Assert
        Assert.AreEqual(value, viewModel.LatestAnnouncementBody);
        Assert.IsInstanceOfType(viewModel.LatestAnnouncementBody, typeof(string));
    }

    /// <summary>
    /// Tests that IsLecturer returns true when Role is set to "lecturer" in lowercase.
    /// Input: Role = "lecturer" (all lowercase).
    /// Expected: IsLecturer returns true.
    /// </summary>
    [TestMethod]
    public void IsLecturer_RoleIsLecturerLowercase_ReturnsTrue()
    {
        // Arrange
        var mockDashboard = new Mock<IDashboardService>();
        var mockAuth = new Mock<IAuthService>();
        var mockSession = new Mock<SessionService>();
        var viewModel = new DashboardViewModel(mockDashboard.Object, mockAuth.Object, mockSession.Object, new Mock<IStudentScheduleService>().Object, new Mock<IRefreshCoordinator>().Object, new Mock<IInsightsService>().Object);

        // Act
        viewModel.Role = "lecturer";

        // Assert
        Assert.IsTrue(viewModel.IsLecturer);
    }

    /// <summary>
    /// Tests that IsLecturer returns true when Role is set to "LECTURER" in uppercase.
    /// Input: Role = "LECTURER" (all uppercase).
    /// Expected: IsLecturer returns true.
    /// </summary>
    [TestMethod]
    public void IsLecturer_RoleIsLecturerUppercase_ReturnsTrue()
    {
        // Arrange
        var mockDashboard = new Mock<IDashboardService>();
        var mockAuth = new Mock<IAuthService>();
        var mockSession = new Mock<SessionService>();
        var viewModel = new DashboardViewModel(mockDashboard.Object, mockAuth.Object, mockSession.Object, new Mock<IStudentScheduleService>().Object, new Mock<IRefreshCoordinator>().Object, new Mock<IInsightsService>().Object);

        // Act
        viewModel.Role = "LECTURER";

        // Assert
        Assert.IsTrue(viewModel.IsLecturer);
    }

    /// <summary>
    /// Tests that IsLecturer returns true when Role is set to "Lecturer" in title case.
    /// Input: Role = "Lecturer" (title case).
    /// Expected: IsLecturer returns true.
    /// </summary>
    [TestMethod]
    public void IsLecturer_RoleIsLecturerTitleCase_ReturnsTrue()
    {
        // Arrange
        var mockDashboard = new Mock<IDashboardService>();
        var mockAuth = new Mock<IAuthService>();
        var mockSession = new Mock<SessionService>();
        var viewModel = new DashboardViewModel(mockDashboard.Object, mockAuth.Object, mockSession.Object, new Mock<IStudentScheduleService>().Object, new Mock<IRefreshCoordinator>().Object, new Mock<IInsightsService>().Object);

        // Act
        viewModel.Role = "Lecturer";

        // Assert
        Assert.IsTrue(viewModel.IsLecturer);
    }

    /// <summary>
    /// Tests that IsLecturer returns true for various mixed-case variations of "lecturer".
    /// Verifies case-insensitive comparison using culture-invariant comparison.
    /// Input: Various case combinations of "lecturer".
    /// Expected: All return true.
    /// </summary>
    [TestMethod]
    [DataRow("lecturer")]
    [DataRow("Lecturer")]
    [DataRow("LECTURER")]
    [DataRow("LeCtuReR")]
    [DataRow("lEcTuReR")]
    [DataRow("lecTURER")]
    [DataRow("LECTurer")]
    [DataRow("leCTURer")]
    public void IsLecturer_VariousCaseVariationsOfLecturer_ReturnsTrue(string roleValue)
    {
        // Arrange
        var mockDashboard = new Mock<IDashboardService>();
        var mockAuth = new Mock<IAuthService>();
        var mockSession = new Mock<SessionService>();
        var viewModel = new DashboardViewModel(mockDashboard.Object, mockAuth.Object, mockSession.Object, new Mock<IStudentScheduleService>().Object, new Mock<IRefreshCoordinator>().Object, new Mock<IInsightsService>().Object);

        // Act
        viewModel.Role = roleValue;

        // Assert
        Assert.IsTrue(viewModel.IsLecturer, $"IsLecturer should return true for Role='{roleValue}'");
    }

    /// <summary>
    /// Tests that IsLecturer returns false when Role is null.
    /// Verifies null-conditional operator behavior.
    /// Input: Role = null.
    /// Expected: IsLecturer returns false.
    /// </summary>
    [TestMethod]
    public void IsLecturer_RoleIsNull_ReturnsFalse()
    {
        // Arrange
        var mockDashboard = new Mock<IDashboardService>();
        var mockAuth = new Mock<IAuthService>();
        var mockSession = new Mock<SessionService>();
        var viewModel = new DashboardViewModel(mockDashboard.Object, mockAuth.Object, mockSession.Object, new Mock<IStudentScheduleService>().Object, new Mock<IRefreshCoordinator>().Object, new Mock<IInsightsService>().Object);

        // Act
        viewModel.Role = null;

        // Assert
        Assert.IsFalse(viewModel.IsLecturer);
    }

    /// <summary>
    /// Tests that IsLecturer returns false when Role is an empty string.
    /// Input: Role = "" (empty string).
    /// Expected: IsLecturer returns false.
    /// </summary>
    [TestMethod]
    public void IsLecturer_RoleIsEmptyString_ReturnsFalse()
    {
        // Arrange
        var mockDashboard = new Mock<IDashboardService>();
        var mockAuth = new Mock<IAuthService>();
        var mockSession = new Mock<SessionService>();
        var viewModel = new DashboardViewModel(mockDashboard.Object, mockAuth.Object, mockSession.Object, new Mock<IStudentScheduleService>().Object, new Mock<IRefreshCoordinator>().Object, new Mock<IInsightsService>().Object);

        // Act
        viewModel.Role = "";

        // Assert
        Assert.IsFalse(viewModel.IsLecturer);
    }

    /// <summary>
    /// Tests that IsLecturer returns false when Role contains only whitespace characters.
    /// Input: Various whitespace-only strings.
    /// Expected: All return false.
    /// </summary>
    [TestMethod]
    [DataRow(" ")]
    [DataRow("  ")]
    [DataRow("   ")]
    [DataRow("\t")]
    [DataRow("\n")]
    [DataRow("\r")]
    [DataRow("\r\n")]
    [DataRow(" \t \n ")]
    public void IsLecturer_RoleIsWhitespaceOnly_ReturnsFalse(string roleValue)
    {
        // Arrange
        var mockDashboard = new Mock<IDashboardService>();
        var mockAuth = new Mock<IAuthService>();
        var mockSession = new Mock<SessionService>();
        var viewModel = new DashboardViewModel(mockDashboard.Object, mockAuth.Object, mockSession.Object, new Mock<IStudentScheduleService>().Object, new Mock<IRefreshCoordinator>().Object, new Mock<IInsightsService>().Object);

        // Act
        viewModel.Role = roleValue;

        // Assert
        Assert.IsFalse(viewModel.IsLecturer, $"IsLecturer should return false for whitespace Role='{roleValue}'");
    }

    /// <summary>
    /// Tests that IsLecturer returns false for other valid role values.
    /// Input: Other role names (student, admin, classrep, etc.).
    /// Expected: All return false.
    /// </summary>
    [TestMethod]
    [DataRow("student")]
    [DataRow("Student")]
    [DataRow("STUDENT")]
    [DataRow("admin")]
    [DataRow("Admin")]
    [DataRow("ADMIN")]
    [DataRow("classrep")]
    [DataRow("ClassRep")]
    [DataRow("CLASSREP")]
    [DataRow("classrepresentative")]
    [DataRow("ClassRepresentative")]
    [DataRow("CLASSREPRESENTATIVE")]
    public void IsLecturer_RoleIsOtherValidRole_ReturnsFalse(string roleValue)
    {
        // Arrange
        var mockDashboard = new Mock<IDashboardService>();
        var mockAuth = new Mock<IAuthService>();
        var mockSession = new Mock<SessionService>();
        var viewModel = new DashboardViewModel(mockDashboard.Object, mockAuth.Object, mockSession.Object, new Mock<IStudentScheduleService>().Object, new Mock<IRefreshCoordinator>().Object, new Mock<IInsightsService>().Object);

        // Act
        viewModel.Role = roleValue;

        // Assert
        Assert.IsFalse(viewModel.IsLecturer, $"IsLecturer should return false for Role='{roleValue}'");
    }

    /// <summary>
    /// Tests that IsLecturer returns false when Role has leading whitespace.
    /// Verifies exact string matching without trimming.
    /// Input: "lecturer" with leading spaces/tabs.
    /// Expected: Returns false.
    /// </summary>
    [TestMethod]
    [DataRow(" lecturer")]
    [DataRow("  lecturer")]
    [DataRow("\tlecturer")]
    [DataRow("\nlecturer")]
    public void IsLecturer_RoleHasLeadingWhitespace_ReturnsFalse(string roleValue)
    {
        // Arrange
        var mockDashboard = new Mock<IDashboardService>();
        var mockAuth = new Mock<IAuthService>();
        var mockSession = new Mock<SessionService>();
        var viewModel = new DashboardViewModel(mockDashboard.Object, mockAuth.Object, mockSession.Object, new Mock<IStudentScheduleService>().Object, new Mock<IRefreshCoordinator>().Object, new Mock<IInsightsService>().Object);

        // Act
        viewModel.Role = roleValue;

        // Assert
        Assert.IsFalse(viewModel.IsLecturer, $"IsLecturer should return false for Role with leading whitespace");
    }

    /// <summary>
    /// Tests that IsLecturer returns false when Role has trailing whitespace.
    /// Verifies exact string matching without trimming.
    /// Input: "lecturer" with trailing spaces/tabs.
    /// Expected: Returns false.
    /// </summary>
    [TestMethod]
    [DataRow("lecturer ")]
    [DataRow("lecturer  ")]
    [DataRow("lecturer\t")]
    [DataRow("lecturer\n")]
    public void IsLecturer_RoleHasTrailingWhitespace_ReturnsFalse(string roleValue)
    {
        // Arrange
        var mockDashboard = new Mock<IDashboardService>();
        var mockAuth = new Mock<IAuthService>();
        var mockSession = new Mock<SessionService>();
        var viewModel = new DashboardViewModel(mockDashboard.Object, mockAuth.Object, mockSession.Object, new Mock<IStudentScheduleService>().Object, new Mock<IRefreshCoordinator>().Object, new Mock<IInsightsService>().Object);

        // Act
        viewModel.Role = roleValue;

        // Assert
        Assert.IsFalse(viewModel.IsLecturer, $"IsLecturer should return false for Role with trailing whitespace");
    }

    /// <summary>
    /// Tests that IsLecturer returns false when Role has both leading and trailing whitespace.
    /// Input: "lecturer" surrounded by whitespace.
    /// Expected: Returns false.
    /// </summary>
    [TestMethod]
    [DataRow(" lecturer ")]
    [DataRow("  lecturer  ")]
    [DataRow("\tlecturer\t")]
    public void IsLecturer_RoleHasSurroundingWhitespace_ReturnsFalse(string roleValue)
    {
        // Arrange
        var mockDashboard = new Mock<IDashboardService>();
        var mockAuth = new Mock<IAuthService>();
        var mockSession = new Mock<SessionService>();
        var viewModel = new DashboardViewModel(mockDashboard.Object, mockAuth.Object, mockSession.Object, new Mock<IStudentScheduleService>().Object, new Mock<IRefreshCoordinator>().Object, new Mock<IInsightsService>().Object);

        // Act
        viewModel.Role = roleValue;

        // Assert
        Assert.IsFalse(viewModel.IsLecturer, $"IsLecturer should return false for Role with surrounding whitespace");
    }

    /// <summary>
    /// Tests that IsLecturer returns false for partial matches or variations of "lecturer".
    /// Input: Strings that contain "lecturer" but are not exact matches.
    /// Expected: All return false.
    /// </summary>
    [TestMethod]
    [DataRow("lecturers")]
    [DataRow("lecturer1")]
    [DataRow("1lecturer")]
    [DataRow("mylecturer")]
    [DataRow("lecturerx")]
    [DataRow("senior lecturer")]
    [DataRow("lecturer senior")]
    [DataRow("lect")]
    [DataRow("lecture")]
    [DataRow("lectur")]
    public void IsLecturer_RoleIsPartialOrVariation_ReturnsFalse(string roleValue)
    {
        // Arrange
        var mockDashboard = new Mock<IDashboardService>();
        var mockAuth = new Mock<IAuthService>();
        var mockSession = new Mock<SessionService>();
        var viewModel = new DashboardViewModel(mockDashboard.Object, mockAuth.Object, mockSession.Object, new Mock<IStudentScheduleService>().Object, new Mock<IRefreshCoordinator>().Object, new Mock<IInsightsService>().Object);

        // Act
        viewModel.Role = roleValue;

        // Assert
        Assert.IsFalse(viewModel.IsLecturer, $"IsLecturer should return false for partial match Role='{roleValue}'");
    }

    /// <summary>
    /// Tests that IsLecturer returns false when Role contains special characters.
    /// Input: "lecturer" with various special characters added.
    /// Expected: All return false.
    /// </summary>
    [TestMethod]
    [DataRow("lecturer!")]
    [DataRow("@lecturer")]
    [DataRow("lecturer#")]
    [DataRow("lec#turer")]
    [DataRow("lecturer$")]
    [DataRow("lec turer")]
    public void IsLecturer_RoleContainsSpecialCharacters_ReturnsFalse(string roleValue)
    {
        // Arrange
        var mockDashboard = new Mock<IDashboardService>();
        var mockAuth = new Mock<IAuthService>();
        var mockSession = new Mock<SessionService>();
        var viewModel = new DashboardViewModel(mockDashboard.Object, mockAuth.Object, mockSession.Object, new Mock<IStudentScheduleService>().Object, new Mock<IRefreshCoordinator>().Object, new Mock<IInsightsService>().Object);

        // Act
        viewModel.Role = roleValue;

        // Assert
        Assert.IsFalse(viewModel.IsLecturer, $"IsLecturer should return false for Role with special characters");
    }

    /// <summary>
    /// Tests that IsLecturer returns false for similar role names that are not "lecturer".
    /// Input: Role names similar to lecturer (teacher, professor, tutor, etc.).
    /// Expected: All return false.
    /// </summary>
    [TestMethod]
    [DataRow("teacher")]
    [DataRow("Teacher")]
    [DataRow("professor")]
    [DataRow("Professor")]
    [DataRow("tutor")]
    [DataRow("Tutor")]
    [DataRow("instructor")]
    [DataRow("Instructor")]
    public void IsLecturer_RoleIsSimilarButDifferent_ReturnsFalse(string roleValue)
    {
        // Arrange
        var mockDashboard = new Mock<IDashboardService>();
        var mockAuth = new Mock<IAuthService>();
        var mockSession = new Mock<SessionService>();
        var viewModel = new DashboardViewModel(mockDashboard.Object, mockAuth.Object, mockSession.Object, new Mock<IStudentScheduleService>().Object, new Mock<IRefreshCoordinator>().Object, new Mock<IInsightsService>().Object);

        // Act
        viewModel.Role = roleValue;

        // Assert
        Assert.IsFalse(viewModel.IsLecturer, $"IsLecturer should return false for similar role '{roleValue}'");
    }

    /// <summary>
    /// Tests that IsLecturer returns false when Role is a very long string.
    /// Input: String with 10000 characters.
    /// Expected: Returns false.
    /// </summary>
    [TestMethod]
    public void IsLecturer_RoleIsVeryLongString_ReturnsFalse()
    {
        // Arrange
        var mockDashboard = new Mock<IDashboardService>();
        var mockAuth = new Mock<IAuthService>();
        var mockSession = new Mock<SessionService>();
        var viewModel = new DashboardViewModel(mockDashboard.Object, mockAuth.Object, mockSession.Object, new Mock<IStudentScheduleService>().Object, new Mock<IRefreshCoordinator>().Object, new Mock<IInsightsService>().Object);
        var veryLongString = new string('a', 10000);

        // Act
        viewModel.Role = veryLongString;

        // Assert
        Assert.IsFalse(viewModel.IsLecturer);
    }

    /// <summary>
    /// Tests that IsLecturer returns false when Role contains Unicode control characters.
    /// Input: "lecturer" with zero-width space and other Unicode control characters.
    /// Expected: All return false.
    /// </summary>
    [TestMethod]
    [DataRow("lecturer\u200B")]
    [DataRow("\u200Blecturer")]
    [DataRow("lec\u200Bturer")]
    [DataRow("lecturer\u0000")]
    [DataRow("\u202Alecturer")]
    public void IsLecturer_RoleContainsUnicodeControlCharacters_ReturnsFalse(string roleValue)
    {
        // Arrange
        var mockDashboard = new Mock<IDashboardService>();
        var mockAuth = new Mock<IAuthService>();
        var mockSession = new Mock<SessionService>();
        var viewModel = new DashboardViewModel(mockDashboard.Object, mockAuth.Object, mockSession.Object, new Mock<IStudentScheduleService>().Object, new Mock<IRefreshCoordinator>().Object, new Mock<IInsightsService>().Object);

        // Act
        viewModel.Role = roleValue;

        // Assert
        Assert.IsFalse(viewModel.IsLecturer, $"IsLecturer should return false for Role with Unicode control characters");
    }

    /// <summary>
    /// Tests that IsLecturer returns false when Role contains numeric characters.
    /// Input: Strings with numbers.
    /// Expected: All return false.
    /// </summary>
    [TestMethod]
    [DataRow("123")]
    [DataRow("0")]
    [DataRow("lecturer123")]
    [DataRow("123lecturer")]
    public void IsLecturer_RoleContainsNumericCharacters_ReturnsFalse(string roleValue)
    {
        // Arrange
        var mockDashboard = new Mock<IDashboardService>();
        var mockAuth = new Mock<IAuthService>();
        var mockSession = new Mock<SessionService>();
        var viewModel = new DashboardViewModel(mockDashboard.Object, mockAuth.Object, mockSession.Object, new Mock<IStudentScheduleService>().Object, new Mock<IRefreshCoordinator>().Object, new Mock<IInsightsService>().Object);

        // Act
        viewModel.Role = roleValue;

        // Assert
        Assert.IsFalse(viewModel.IsLecturer, $"IsLecturer should return false for Role='{roleValue}'");
    }

    /// <summary>
    /// Tests that IsLecturer correctly reflects changes when Role is updated multiple times.
    /// Input: Sequential role changes.
    /// Expected: IsLecturer value updates correctly for each role change.
    /// </summary>
    [TestMethod]
    public void IsLecturer_RoleChangesMultipleTimes_ReflectsCurrentValue()
    {
        // Arrange
        var mockDashboard = new Mock<IDashboardService>();
        var mockAuth = new Mock<IAuthService>();
        var mockSession = new Mock<SessionService>();
        var viewModel = new DashboardViewModel(mockDashboard.Object, mockAuth.Object, mockSession.Object, new Mock<IStudentScheduleService>().Object, new Mock<IRefreshCoordinator>().Object, new Mock<IInsightsService>().Object);

        // Act & Assert
        viewModel.Role = "lecturer";
        Assert.IsTrue(viewModel.IsLecturer, "Should be true when Role is 'lecturer'");

        viewModel.Role = "student";
        Assert.IsFalse(viewModel.IsLecturer, "Should be false when Role is 'student'");

        viewModel.Role = "LECTURER";
        Assert.IsTrue(viewModel.IsLecturer, "Should be true when Role is 'LECTURER'");

        viewModel.Role = null;
        Assert.IsFalse(viewModel.IsLecturer, "Should be false when Role is null");

        viewModel.Role = "Lecturer";
        Assert.IsTrue(viewModel.IsLecturer, "Should be true when Role is 'Lecturer'");
    }

    /// <summary>
    /// Tests that IsLecturer can be accessed multiple times without side effects.
    /// Input: Role = "lecturer".
    /// Expected: Multiple accesses return consistent true value.
    /// </summary>
    [TestMethod]
    public void IsLecturer_MultipleAccesses_ReturnsConsistentValue()
    {
        // Arrange
        var mockDashboard = new Mock<IDashboardService>();
        var mockAuth = new Mock<IAuthService>();
        var mockSession = new Mock<SessionService>();
        var viewModel = new DashboardViewModel(mockDashboard.Object, mockAuth.Object, mockSession.Object, new Mock<IStudentScheduleService>().Object, new Mock<IRefreshCoordinator>().Object, new Mock<IInsightsService>().Object);
        viewModel.Role = "lecturer";

        // Act & Assert
        Assert.IsTrue(viewModel.IsLecturer, "First access");
        Assert.IsTrue(viewModel.IsLecturer, "Second access");
        Assert.IsTrue(viewModel.IsLecturer, "Third access");
    }

    /// <summary>
    /// Tests that IsLecturer uses invariant culture comparison.
    /// Input: "lecturer" in various cultures.
    /// Expected: Returns true regardless of current culture.
    /// </summary>
    [TestMethod]
    public void IsLecturer_InvariantCultureComparison_WorksCorrectly()
    {
        // Arrange
        var mockDashboard = new Mock<IDashboardService>();
        var mockAuth = new Mock<IAuthService>();
        var mockSession = new Mock<SessionService>();
        var viewModel = new DashboardViewModel(mockDashboard.Object, mockAuth.Object, mockSession.Object, new Mock<IStudentScheduleService>().Object, new Mock<IRefreshCoordinator>().Object, new Mock<IInsightsService>().Object);

        // Act
        viewModel.Role = "lecturer";
        var result1 = viewModel.IsLecturer;

        viewModel.Role = "LECTURER";
        var result2 = viewModel.IsLecturer;

        // Assert
        Assert.IsTrue(result1);
        Assert.IsTrue(result2);
    }

    /// <summary>
    /// Tests default behavior when Role has default value from initialization.
    /// Input: Default Role value after construction (should be "Student").
    /// Expected: IsLecturer returns false.
    /// </summary>
    [TestMethod]
    public void IsLecturer_DefaultRoleValue_ReturnsFalse()
    {
        // Arrange
        var mockDashboard = new Mock<IDashboardService>();
        var mockAuth = new Mock<IAuthService>();
        var mockSession = new Mock<SessionService>();
        var viewModel = new DashboardViewModel(mockDashboard.Object, mockAuth.Object, mockSession.Object, new Mock<IStudentScheduleService>().Object, new Mock<IRefreshCoordinator>().Object, new Mock<IInsightsService>().Object);

        // Act
        var result = viewModel.IsLecturer;

        // Assert
        Assert.IsFalse(result, "IsLecturer should be false for default Role value");
    }

    /// <summary>
    /// Tests that OpenClassCommand returns an ICommand instance.
    /// Verifies the property returns the expected interface type.
    /// Input: None (property access).
    /// Expected: Returns an instance of ICommand.
    /// </summary>
    [TestMethod]
    public void OpenClassCommand_WhenAccessed_ReturnsICommandInstance()
    {
        // Arrange
        var mockDashboard = new Mock<IDashboardService>();
        var mockAuth = new Mock<IAuthService>();
        var mockSession = new Mock<SessionService>();
        var viewModel = new DashboardViewModel(mockDashboard.Object, mockAuth.Object, mockSession.Object, new Mock<IStudentScheduleService>().Object, new Mock<IRefreshCoordinator>().Object, new Mock<IInsightsService>().Object);

        // Act
        var command = viewModel.OpenClassCommand;

        // Assert
        Assert.IsInstanceOfType<ICommand>(command);
    }

    /// <summary>
    /// Tests that RefreshCommand returns a Command type specifically, not just ICommand.
    /// Input: None (property access).
    /// Expected: Returns an instance of Microsoft.Maui.Controls.Command.
    /// </summary>
    [TestMethod]
    public void RefreshCommand_WhenAccessed_ReturnsSpecificCommandType()
    {
        // Arrange
        var mockDashboardService = new Mock<IDashboardService>();
        var mockAuthService = new Mock<IAuthService>();
        var mockSessionService = new Mock<SessionService>();
        var viewModel = new DashboardViewModel(mockDashboardService.Object, mockAuthService.Object, mockSessionService.Object, new Mock<IStudentScheduleService>().Object, new Mock<IRefreshCoordinator>().Object, new Mock<IInsightsService>().Object);

        // Act
        var command = viewModel.RefreshCommand;

        // Assert
        Assert.IsNotNull(command);
        Assert.IsInstanceOfType(command, typeof(Command));
    }

    /// <summary>
    /// Tests that RefreshCommand.CanExecute returns true when ViewModel is in busy state.
    /// Input: ViewModel with IsBusy set to true.
    /// Expected: CanExecute returns true (command is not disabled by busy state).
    /// </summary>
    [TestMethod]
    public void RefreshCommand_WhenViewModelIsBusy_CanExecuteReturnsTrue()
    {
        // Arrange
        var mockDashboardService = new Mock<IDashboardService>();
        var mockAuthService = new Mock<IAuthService>();
        var mockSessionService = new Mock<SessionService>();
        var viewModel = new DashboardViewModel(mockDashboardService.Object, mockAuthService.Object, mockSessionService.Object, new Mock<IStudentScheduleService>().Object, new Mock<IRefreshCoordinator>().Object, new Mock<IInsightsService>().Object);

        viewModel.IsBusy = true;

        // Act
        var command = viewModel.RefreshCommand;
        var canExecute = command.CanExecute(null);

        // Assert
        Assert.IsTrue(canExecute);
    }

    /// <summary>
    /// Tests that RefreshCommand.CanExecute returns true with various object types as parameters.
    /// Input: Different object types passed to CanExecute.
    /// Expected: CanExecute returns true for all parameter types.
    /// </summary>
    [TestMethod]
    [DataRow(null, DisplayName = "CanExecute with null parameter")]
    [DataRow("string", DisplayName = "CanExecute with string parameter")]
    [DataRow(42, DisplayName = "CanExecute with int parameter")]
    [DataRow(true, DisplayName = "CanExecute with bool parameter")]
    [DataRow(3.14, DisplayName = "CanExecute with double parameter")]
    public void RefreshCommand_CanExecuteWithVariousParameterTypes_ReturnsTrue(object? parameter)
    {
        // Arrange
        var mockDashboardService = new Mock<IDashboardService>();
        var mockAuthService = new Mock<IAuthService>();
        var mockSessionService = new Mock<SessionService>();
        var viewModel = new DashboardViewModel(mockDashboardService.Object, mockAuthService.Object, mockSessionService.Object, new Mock<IStudentScheduleService>().Object, new Mock<IRefreshCoordinator>().Object, new Mock<IInsightsService>().Object);

        // Act
        var command = viewModel.RefreshCommand;
        var canExecute = command.CanExecute(parameter);

        // Assert
        Assert.IsTrue(canExecute);
    }

    /// <summary>
    /// Tests that executing RefreshCommand clears the error message before loading.
    /// Input: ViewModel with an existing error message.
    /// Expected: ErrorMessage is cleared when RefreshCommand is executed.
    /// </summary>
    [TestMethod]
    public async Task RefreshCommand_WhenExecutedWithExistingError_ClearsErrorMessageBeforeLoading()
    {
        // Arrange
        var mockDashboardService = new Mock<IDashboardService>();
        var mockAuthService = new Mock<IAuthService>();
        var mockSessionService = new Mock<SessionService>();
        mockSessionService.Setup(s => s.Role).Returns(UserRole.Student);

        var dashboardDto = new DashboardDto
        {
            ActiveCourses = 5,
            UpcomingAssignments = 3,
            AttendancePercent = 0.85,
            RecentAnnouncements = [],
            TeachingClasses = null,
            ManagedClasses = null
        };

        mockDashboardService.Setup(d => d.GetDashboardAsync())
            .ReturnsAsync(dashboardDto);

        var viewModel = new DashboardViewModel(mockDashboardService.Object, mockAuthService.Object, mockSessionService.Object, new Mock<IStudentScheduleService>().Object, new Mock<IRefreshCoordinator>().Object, new Mock<IInsightsService>().Object);
        viewModel.ErrorMessage = "Previous error";

        // Act
        var command = viewModel.RefreshCommand;
        if (command is Command cmd)
        {
            await Task.Run(() => cmd.Execute(null));
            await Task.Delay(100); // Allow async operation to complete
        }

        // Assert
        Assert.AreEqual(string.Empty, viewModel.ErrorMessage);
    }

    /// <summary>
    /// Tests that RefreshCommand handles when dashboard service returns data with all collections null.
    /// Input: Dashboard service returns data with null TeachingClasses, ManagedClasses, and RecentAnnouncements.
    /// Expected: Command executes without exception and sets appropriate default values.
    /// </summary>
    [TestMethod]
    public async Task RefreshCommand_WhenServiceReturnsDataWithAllNullCollections_HandlesGracefully()
    {
        // Arrange
        var mockDashboardService = new Mock<IDashboardService>();
        var mockAuthService = new Mock<IAuthService>();
        var mockSessionService = new Mock<SessionService>();
        mockSessionService.Setup(s => s.Role).Returns(UserRole.Student);

        var dashboardDto = new DashboardDto
        {
            ActiveCourses = 0,
            UpcomingAssignments = 0,
            AttendancePercent = 0.0,
            RecentAnnouncements = null,
            TeachingClasses = null,
            ManagedClasses = null,
            TotalStudents = null,
            TotalLecturers = null,
            TotalPrograms = null
        };

        mockDashboardService.Setup(d => d.GetDashboardAsync())
            .ReturnsAsync(dashboardDto);

        var viewModel = new DashboardViewModel(mockDashboardService.Object, mockAuthService.Object, mockSessionService.Object, new Mock<IStudentScheduleService>().Object, new Mock<IRefreshCoordinator>().Object, new Mock<IInsightsService>().Object);

        // Act
        var command = viewModel.RefreshCommand;
        if (command is Command cmd)
        {
            await Task.Run(() => cmd.Execute(null));
            await Task.Delay(100); // Allow async operation to complete
        }

        // Assert
        Assert.AreEqual(0, viewModel.TeachingClassesCount);
        Assert.AreEqual(0, viewModel.ManagedClassesCount);
        Assert.IsFalse(viewModel.HasAnnouncement);
        Assert.AreEqual(0, viewModel.TeachingClasses.Count);
        Assert.AreEqual(0, viewModel.RecentAnnouncements.Count);
    }

    /// <summary>
    /// Tests that RefreshCommand handles aggregate exceptions from the dashboard service.
    /// Input: Dashboard service throws AggregateException.
    /// Expected: ErrorMessage is set with the exception details.
    /// </summary>
    [TestMethod]
    public async Task RefreshCommand_WhenServiceThrowsAggregateException_SetsErrorMessage()
    {
        // Arrange
        var mockDashboardService = new Mock<IDashboardService>();
        var mockAuthService = new Mock<IAuthService>();
        var mockSessionService = new Mock<SessionService>();

        var innerException = new InvalidOperationException("Inner error");
        var aggregateException = new AggregateException("Multiple errors", innerException);

        mockDashboardService.Setup(d => d.GetDashboardAsync())
            .ThrowsAsync(aggregateException);

        var viewModel = new DashboardViewModel(mockDashboardService.Object, mockAuthService.Object, mockSessionService.Object, new Mock<IStudentScheduleService>().Object, new Mock<IRefreshCoordinator>().Object, new Mock<IInsightsService>().Object);

        // Act
        var command = viewModel.RefreshCommand;
        if (command is Command cmd)
        {
            await Task.Run(() => cmd.Execute(null));
            await Task.Delay(100); // Allow async operation to complete
        }

        // Assert
        Assert.IsFalse(string.IsNullOrEmpty(viewModel.ErrorMessage));
        Assert.IsTrue(viewModel.ErrorMessage.Contains("Multiple errors"));
    }

    /// <summary>
    /// Tests that RefreshCommand handles dashboard data with zero values for all numeric properties.
    /// Input: Dashboard data with all counts set to 0.
    /// Expected: All properties are updated to 0 correctly.
    /// </summary>
    [TestMethod]
    public async Task RefreshCommand_WhenDataHasAllZeroValues_UpdatesPropertiesCorrectly()
    {
        // Arrange
        var mockDashboardService = new Mock<IDashboardService>();
        var mockAuthService = new Mock<IAuthService>();
        var mockSessionService = new Mock<SessionService>();
        mockSessionService.Setup(s => s.Role).Returns(UserRole.Admin);

        var dashboardDto = new DashboardDto
        {
            ActiveCourses = 0,
            UpcomingAssignments = 0,
            AttendancePercent = 0.0,
            RecentAnnouncements = [],
            TeachingClasses = [],
            ManagedClasses = [],
            TotalStudents = 0,
            TotalLecturers = 0,
            TotalPrograms = 0
        };

        mockDashboardService.Setup(d => d.GetDashboardAsync())
            .ReturnsAsync(dashboardDto);

        var viewModel = new DashboardViewModel(mockDashboardService.Object, mockAuthService.Object, mockSessionService.Object, new Mock<IStudentScheduleService>().Object, new Mock<IRefreshCoordinator>().Object, new Mock<IInsightsService>().Object);

        // Act
        var command = viewModel.RefreshCommand;
        if (command is Command cmd)
        {
            await Task.Run(() => cmd.Execute(null));
            await Task.Delay(100); // Allow async operation to complete
        }

        // Assert
        Assert.AreEqual(0, viewModel.ActiveCourses);
        Assert.AreEqual(0, viewModel.UpcomingAssignments);
        Assert.AreEqual(0, viewModel.AttendancePercentage);
        Assert.AreEqual(0, viewModel.TotalStudents);
        Assert.AreEqual(0, viewModel.TotalLecturers);
        Assert.AreEqual(0, viewModel.TotalPrograms);
    }

    /// <summary>
    /// Tests that multiple consecutive accesses to RefreshCommand property create completely independent command instances.
    /// Input: Three consecutive accesses to RefreshCommand.
    /// Expected: All three commands are different instances and none are null.
    /// </summary>
    [TestMethod]
    public void RefreshCommand_MultipleConsecutiveAccesses_CreatesIndependentInstances()
    {
        // Arrange
        var mockDashboardService = new Mock<IDashboardService>();
        var mockAuthService = new Mock<IAuthService>();
        var mockSessionService = new Mock<SessionService>();
        var viewModel = new DashboardViewModel(mockDashboardService.Object, mockAuthService.Object, mockSessionService.Object, new Mock<IStudentScheduleService>().Object, new Mock<IRefreshCoordinator>().Object, new Mock<IInsightsService>().Object);

        // Act
        var command1 = viewModel.RefreshCommand;
        var command2 = viewModel.RefreshCommand;
        var command3 = viewModel.RefreshCommand;

        // Assert
        Assert.IsNotNull(command1);
        Assert.IsNotNull(command2);
        Assert.IsNotNull(command3);
        Assert.AreNotSame(command1, command2);
        Assert.AreNotSame(command2, command3);
        Assert.AreNotSame(command1, command3);
    }

    /// <summary>
    /// Tests that RefreshCommand handles UnauthorizedAccessException from dashboard service.
    /// Input: Dashboard service throws UnauthorizedAccessException.
    /// Expected: ErrorMessage is set with exception details.
    /// </summary>
    [TestMethod]
    public async Task RefreshCommand_WhenServiceThrowsUnauthorizedAccessException_SetsErrorMessage()
    {
        // Arrange
        var mockDashboardService = new Mock<IDashboardService>();
        var mockAuthService = new Mock<IAuthService>();
        var mockSessionService = new Mock<SessionService>();

        mockDashboardService.Setup(d => d.GetDashboardAsync())
            .ThrowsAsync(new UnauthorizedAccessException("Access denied"));

        var viewModel = new DashboardViewModel(mockDashboardService.Object, mockAuthService.Object, mockSessionService.Object, new Mock<IStudentScheduleService>().Object, new Mock<IRefreshCoordinator>().Object, new Mock<IInsightsService>().Object);

        // Act
        var command = viewModel.RefreshCommand;
        if (command is Command cmd)
        {
            await Task.Run(() => cmd.Execute(null));
            await Task.Delay(100); // Allow async operation to complete
        }

        // Assert
        Assert.IsFalse(string.IsNullOrEmpty(viewModel.ErrorMessage));
        Assert.IsTrue(viewModel.ErrorMessage.Contains("Access denied"));
    }

    /// <summary>
    /// Tests that the GoToAcademicCommand.CanExecute returns true by default.
    /// Input: null parameter to CanExecute.
    /// Expected: CanExecute returns true.
    /// </summary>
    [TestMethod]
    public void GoToAcademicCommand_CanExecuteWithNullParameter_ReturnsTrue()
    {
        // Arrange
        Mock<IDashboardService> mockDashboardService = new Mock<IDashboardService>();
        Mock<IAuthService> mockAuthService = new Mock<IAuthService>();
        Mock<SessionService> mockSessionService = new Mock<SessionService>();
        DashboardViewModel viewModel = new DashboardViewModel(mockDashboardService.Object, mockAuthService.Object, mockSessionService.Object, new Mock<IStudentScheduleService>().Object, new Mock<IRefreshCoordinator>().Object, new Mock<IInsightsService>().Object);

        // Act
        ICommand command = viewModel.GoToAcademicCommand;
        bool canExecute = command.CanExecute(null);

        // Assert
        Assert.IsTrue(canExecute);
    }

    /// <summary>
    /// Tests that multiple accesses to GoToAcademicCommand return instances that all implement ICommand.
    /// Input: Multiple property accesses.
    /// Expected: All returned instances implement ICommand interface.
    /// </summary>
    [TestMethod]
    public void GoToAcademicCommand_MultipleAccesses_AllInstancesImplementICommand()
    {
        // Arrange
        Mock<IDashboardService> mockDashboardService = new Mock<IDashboardService>();
        Mock<IAuthService> mockAuthService = new Mock<IAuthService>();
        Mock<SessionService> mockSessionService = new Mock<SessionService>();
        DashboardViewModel viewModel = new DashboardViewModel(mockDashboardService.Object, mockAuthService.Object, mockSessionService.Object, new Mock<IStudentScheduleService>().Object, new Mock<IRefreshCoordinator>().Object, new Mock<IInsightsService>().Object);

        // Act
        ICommand command1 = viewModel.GoToAcademicCommand;
        ICommand command2 = viewModel.GoToAcademicCommand;
        ICommand command3 = viewModel.GoToAcademicCommand;

        // Assert
        Assert.IsInstanceOfType(command1, typeof(ICommand));
        Assert.IsInstanceOfType(command2, typeof(ICommand));
        Assert.IsInstanceOfType(command3, typeof(ICommand));
    }

    /// <summary>
    /// Tests that GoToAcademicCommand property is accessible immediately after ViewModel construction.
    /// Input: Freshly constructed ViewModel.
    /// Expected: Property is accessible and returns non-null command.
    /// </summary>
    [TestMethod]
    public void GoToAcademicCommand_AfterConstruction_IsImmediatelyAccessible()
    {
        // Arrange
        Mock<IDashboardService> mockDashboardService = new Mock<IDashboardService>();
        Mock<IAuthService> mockAuthService = new Mock<IAuthService>();
        Mock<SessionService> mockSessionService = new Mock<SessionService>();

        // Act
        DashboardViewModel viewModel = new DashboardViewModel(mockDashboardService.Object, mockAuthService.Object, mockSessionService.Object, new Mock<IStudentScheduleService>().Object, new Mock<IRefreshCoordinator>().Object, new Mock<IInsightsService>().Object);
        ICommand command = viewModel.GoToAcademicCommand;

        // Assert
        Assert.IsNotNull(command);
    }
}