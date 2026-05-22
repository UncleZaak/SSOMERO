using System;
using System.ComponentModel;
using System.Windows.Input;

using Microsoft.Maui.Controls;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Ssomero.Interfaces;
using Ssomero.Models;
using Ssomero.Services;
using Ssomero.ViewModels;
using Microsoft.Extensions.Logging;

namespace Ssomero.ViewModels.UnitTests;




/// <summary>
/// Unit tests for the ProfileViewModel class.
/// </summary>
[TestClass]
public partial class ProfileViewModelTests
{
    /// <summary>
    /// Tests that the FullName property returns the value that was set.
    /// </summary>
    [TestMethod]
    public void FullName_SetValidValue_ReturnsSetValue()
    {
        // Arrange
        var mockAuthService = new Mock<IAuthService>();
        var mockSessionService = new Mock<SessionService>();
        var viewModel = new ProfileViewModel(mockAuthService.Object, new Mock<IProfileService>().Object, mockSessionService.Object, new Mock<ILogger<ProfileViewModel>>().Object);
        const string expectedValue = "John Doe";

        // Act
        viewModel.FullName = expectedValue;
        var actualValue = viewModel.FullName;

        // Assert
        Assert.AreEqual(expectedValue, actualValue);
    }

    /// <summary>
    /// Tests that setting the FullName property raises the PropertyChanged event with the correct property name.
    /// </summary>
    [TestMethod]
    public void FullName_SetDifferentValue_RaisesPropertyChangedEvent()
    {
        // Arrange
        var mockAuthService = new Mock<IAuthService>();
        var mockSessionService = new Mock<SessionService>();
        var viewModel = new ProfileViewModel(mockAuthService.Object, new Mock<IProfileService>().Object, mockSessionService.Object, new Mock<ILogger<ProfileViewModel>>().Object);
        string? raisedPropertyName = null;
        viewModel.PropertyChanged += (sender, args) => raisedPropertyName = args.PropertyName;

        // Act
        viewModel.FullName = "Jane Smith";

        // Assert
        Assert.AreEqual("FullName", raisedPropertyName);
    }

    /// <summary>
    /// Tests that setting the FullName property to the same value does not raise the PropertyChanged event.
    /// </summary>
    [TestMethod]
    public void FullName_SetSameValue_DoesNotRaisePropertyChangedEvent()
    {
        // Arrange
        var mockAuthService = new Mock<IAuthService>();
        var mockSessionService = new Mock<SessionService>();
        var viewModel = new ProfileViewModel(mockAuthService.Object, new Mock<IProfileService>().Object, mockSessionService.Object, new Mock<ILogger<ProfileViewModel>>().Object);
        viewModel.FullName = "Same Value";
        int eventRaisedCount = 0;
        viewModel.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == "FullName")
                eventRaisedCount++;
        };

        // Act
        viewModel.FullName = "Same Value";

        // Assert
        Assert.AreEqual(0, eventRaisedCount);
    }

    /// <summary>
    /// Tests that the FullName property can be set to an empty string.
    /// </summary>
    [TestMethod]
    public void FullName_SetEmptyString_ReturnsEmptyString()
    {
        // Arrange
        var mockAuthService = new Mock<IAuthService>();
        var mockSessionService = new Mock<SessionService>();
        var viewModel = new ProfileViewModel(mockAuthService.Object, new Mock<IProfileService>().Object, mockSessionService.Object, new Mock<ILogger<ProfileViewModel>>().Object);

        // Act
        viewModel.FullName = string.Empty;

        // Assert
        Assert.AreEqual(string.Empty, viewModel.FullName);
    }

    /// <summary>
    /// Tests that the FullName property handles whitespace-only strings correctly.
    /// </summary>
    /// <param name="value">The whitespace string value to test.</param>
    [TestMethod]
    [DataRow("   ")]
    [DataRow("\t")]
    [DataRow("\n")]
    [DataRow(" \t\n ")]
    public void FullName_SetWhitespaceString_ReturnsWhitespaceString(string value)
    {
        // Arrange
        var mockAuthService = new Mock<IAuthService>();
        var mockSessionService = new Mock<SessionService>();
        var viewModel = new ProfileViewModel(mockAuthService.Object, new Mock<IProfileService>().Object, mockSessionService.Object, new Mock<ILogger<ProfileViewModel>>().Object);

        // Act
        viewModel.FullName = value;

        // Assert
        Assert.AreEqual(value, viewModel.FullName);
    }

    /// <summary>
    /// Tests that the FullName property can handle very long strings.
    /// </summary>
    [TestMethod]
    public void FullName_SetVeryLongString_ReturnsVeryLongString()
    {
        // Arrange
        var mockAuthService = new Mock<IAuthService>();
        var mockSessionService = new Mock<SessionService>();
        var viewModel = new ProfileViewModel(mockAuthService.Object, new Mock<IProfileService>().Object, mockSessionService.Object, new Mock<ILogger<ProfileViewModel>>().Object);
        var longString = new string('A', 10000);

        // Act
        viewModel.FullName = longString;

        // Assert
        Assert.AreEqual(longString, viewModel.FullName);
    }

    /// <summary>
    /// Tests that the FullName property handles strings with special characters correctly.
    /// </summary>
    /// <param name="value">The string with special characters to test.</param>
    [TestMethod]
    [DataRow("John@Doe#123")]
    [DataRow("Name with émojis ????")]
    [DataRow("Name\u0000WithNull")]
    [DataRow("Name<>&\"'")]
    [DataRow("Name\r\nWith\tEscapes")]
    public void FullName_SetStringWithSpecialCharacters_ReturnsStringWithSpecialCharacters(string value)
    {
        // Arrange
        var mockAuthService = new Mock<IAuthService>();
        var mockSessionService = new Mock<SessionService>();
        var viewModel = new ProfileViewModel(mockAuthService.Object, new Mock<IProfileService>().Object, mockSessionService.Object, new Mock<ILogger<ProfileViewModel>>().Object);

        // Act
        viewModel.FullName = value;

        // Assert
        Assert.AreEqual(value, viewModel.FullName);
    }

    /// <summary>
    /// Tests that setting multiple different values to FullName raises PropertyChanged event for each change.
    /// </summary>
    [TestMethod]
    public void FullName_SetMultipleDifferentValues_RaisesPropertyChangedEventForEachChange()
    {
        // Arrange
        var mockAuthService = new Mock<IAuthService>();
        var mockSessionService = new Mock<SessionService>();
        var viewModel = new ProfileViewModel(mockAuthService.Object, new Mock<IProfileService>().Object, mockSessionService.Object, new Mock<ILogger<ProfileViewModel>>().Object);
        int eventRaisedCount = 0;
        viewModel.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == "FullName")
                eventRaisedCount++;
        };

        // Act
        viewModel.FullName = "First Name";
        viewModel.FullName = "Second Name";
        viewModel.FullName = "Third Name";

        // Assert
        Assert.AreEqual(3, eventRaisedCount);
    }

    /// <summary>
    /// Tests that the FullName property's initial value is an empty string.
    /// </summary>
    [TestMethod]
    public void FullName_InitialValue_IsEmptyString()
    {
        // Arrange
        var mockAuthService = new Mock<IAuthService>();
        var mockSessionService = new Mock<SessionService>();

        // Act
        var viewModel = new ProfileViewModel(mockAuthService.Object, new Mock<IProfileService>().Object, mockSessionService.Object, new Mock<ILogger<ProfileViewModel>>().Object);

        // Assert
        Assert.AreEqual(string.Empty, viewModel.FullName);
    }

    /// <summary>
    /// Tests that setting FullName to a value and then back to empty string raises PropertyChanged event both times.
    /// </summary>
    [TestMethod]
    public void FullName_SetToValueThenEmpty_RaisesPropertyChangedEventTwice()
    {
        // Arrange
        var mockAuthService = new Mock<IAuthService>();
        var mockSessionService = new Mock<SessionService>();
        var viewModel = new ProfileViewModel(mockAuthService.Object, new Mock<IProfileService>().Object, mockSessionService.Object, new Mock<ILogger<ProfileViewModel>>().Object);
        int eventRaisedCount = 0;
        viewModel.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == "FullName")
                eventRaisedCount++;
        };

        // Act
        viewModel.FullName = "John Doe";
        viewModel.FullName = string.Empty;

        // Assert
        Assert.AreEqual(2, eventRaisedCount);
    }

    /// <summary>
    /// Tests that the constructor initializes successfully with valid parameters and no current user.
    /// Verifies that LogoutCommand is initialized and RefreshProfile sets default "Student" values.
    /// </summary>
    [TestMethod]
    public void Constructor_WithValidParametersAndNoUser_InitializesWithDefaultValues()
    {
        // Arrange
        var mockAuthService = new Mock<IAuthService>();
        var sessionService = new SessionService();

        // Act
        var viewModel = new ProfileViewModel(mockAuthService.Object, new Mock<IProfileService>().Object, sessionService, new Mock<ILogger<ProfileViewModel>>().Object);

        // Assert
        Assert.IsNotNull(viewModel.LogoutCommand);
        Assert.AreEqual("Student", viewModel.FullName);
        Assert.AreEqual(string.Empty, viewModel.Email);
        Assert.AreEqual("Student", viewModel.Role);
        Assert.AreEqual("S", viewModel.Initials);
    }

    /// <summary>
    /// Tests that the constructor initializes successfully with valid parameters and a current user.
    /// Verifies that LogoutCommand is initialized and RefreshProfile populates properties from the user.
    /// </summary>
    [TestMethod]
    public void Constructor_WithValidParametersAndUser_InitializesWithUserData()
    {
        // Arrange
        var mockAuthService = new Mock<IAuthService>();
        var sessionService = new SessionService();
        var user = new AuthUserDto
        {
            FullName = "John Doe",
            Email = "john.doe@example.com",
            Role = "Admin"
        };
        sessionService.SetUser(user);

        // Act
        var viewModel = new ProfileViewModel(mockAuthService.Object, new Mock<IProfileService>().Object, sessionService, new Mock<ILogger<ProfileViewModel>>().Object);

        // Assert
        Assert.IsNotNull(viewModel.LogoutCommand);
        Assert.AreEqual("John Doe", viewModel.FullName);
        Assert.AreEqual("john.doe@example.com", viewModel.Email);
        Assert.AreEqual("Admin", viewModel.Role);
        Assert.AreEqual("JD", viewModel.Initials);
    }

    /// <summary>
    /// Tests that the constructor handles a user with null FullName by defaulting to "Student".
    /// Verifies that properties are set correctly when user.FullName is null.
    /// </summary>
    [TestMethod]
    public void Constructor_WithUserHavingNullFullName_SetsDefaultStudentName()
    {
        // Arrange
        var mockAuthService = new Mock<IAuthService>();
        var sessionService = new SessionService();
        var user = new AuthUserDto
        {
            FullName = null,
            Email = "test@example.com",
            Role = "Student"
        };
        sessionService.SetUser(user);

        // Act
        var viewModel = new ProfileViewModel(mockAuthService.Object, new Mock<IProfileService>().Object, sessionService, new Mock<ILogger<ProfileViewModel>>().Object);

        // Assert
        Assert.IsNotNull(viewModel.LogoutCommand);
        Assert.AreEqual("Student", viewModel.FullName);
        Assert.AreEqual("test@example.com", viewModel.Email);
        Assert.AreEqual("Student", viewModel.Role);
        Assert.AreEqual("S", viewModel.Initials);
    }

    /// <summary>
    /// Tests that the constructor handles a user with empty FullName by defaulting to "Student".
    /// Verifies that properties are set correctly when user.FullName is an empty string.
    /// </summary>
    [TestMethod]
    public void Constructor_WithUserHavingEmptyFullName_SetsDefaultStudentName()
    {
        // Arrange
        var mockAuthService = new Mock<IAuthService>();
        var sessionService = new SessionService();
        var user = new AuthUserDto
        {
            FullName = string.Empty,
            Email = "test@example.com",
            Role = "Student"
        };
        sessionService.SetUser(user);

        // Act
        var viewModel = new ProfileViewModel(mockAuthService.Object, new Mock<IProfileService>().Object, sessionService, new Mock<ILogger<ProfileViewModel>>().Object);

        // Assert
        Assert.IsNotNull(viewModel.LogoutCommand);
        Assert.AreEqual("Student", viewModel.FullName);
        Assert.AreEqual("test@example.com", viewModel.Email);
        Assert.AreEqual("Student", viewModel.Role);
        Assert.AreEqual("S", viewModel.Initials);
    }

    /// <summary>
    /// Tests that the constructor handles a user with whitespace-only FullName by defaulting to "Student".
    /// Verifies that properties are set correctly when user.FullName contains only whitespace.
    /// </summary>
    [TestMethod]
    public void Constructor_WithUserHavingWhitespaceFullName_SetsDefaultStudentName()
    {
        // Arrange
        var mockAuthService = new Mock<IAuthService>();
        var sessionService = new SessionService();
        var user = new AuthUserDto
        {
            FullName = "   ",
            Email = "test@example.com",
            Role = "Lecturer"
        };
        sessionService.SetUser(user);

        // Act
        var viewModel = new ProfileViewModel(mockAuthService.Object, new Mock<IProfileService>().Object, sessionService, new Mock<ILogger<ProfileViewModel>>().Object);

        // Assert
        Assert.IsNotNull(viewModel.LogoutCommand);
        Assert.AreEqual("Student", viewModel.FullName);
        Assert.AreEqual("test@example.com", viewModel.Email);
        Assert.AreEqual("Lecturer", viewModel.Role);
        Assert.AreEqual("S", viewModel.Initials);
    }

    /// <summary>
    /// Tests that the constructor handles different user roles correctly.
    /// Verifies that the Role property is set to the string representation of the role.
    /// </summary>
    [TestMethod]
    [DataRow("admin", "Admin", DisplayName = "Admin role")]
    [DataRow("lecturer", "Lecturer", DisplayName = "Lecturer role")]
    [DataRow("classrepresentative", "ClassRepresentative", DisplayName = "ClassRepresentative role")]
    [DataRow("student", "Student", DisplayName = "Student role")]
    [DataRow("unknown", "Student", DisplayName = "Unknown role defaults to Student")]
    public void Constructor_WithDifferentUserRoles_SetsRoleCorrectly(string roleInput, string expectedRole)
    {
        // Arrange
        var mockAuthService = new Mock<IAuthService>();
        var sessionService = new SessionService();
        var user = new AuthUserDto
        {
            FullName = "Test User",
            Email = "test@example.com",
            Role = roleInput
        };
        sessionService.SetUser(user);

        // Act
        var viewModel = new ProfileViewModel(mockAuthService.Object, new Mock<IProfileService>().Object, sessionService, new Mock<ILogger<ProfileViewModel>>().Object);

        // Assert
        Assert.AreEqual(expectedRole, viewModel.Role);
    }

    /// <summary>
    /// Tests that the constructor accepts null authService without throwing immediately.
    /// The constructor assigns null to _authService field but does not use it during construction.
    /// </summary>
    [TestMethod]
    public void Constructor_WithNullAuthService_DoesNotThrowDuringConstruction()
    {
        // Arrange
        IAuthService? nullAuthService = null;
        var sessionService = new SessionService();

        // Act
        var viewModel = new ProfileViewModel(nullAuthService!, new Mock<IProfileService>().Object, sessionService, new Mock<ILogger<ProfileViewModel>>().Object);

        // Assert
        Assert.IsNotNull(viewModel);
        Assert.IsNotNull(viewModel.LogoutCommand);
    }

    /// <summary>
    /// Tests that the constructor handles users with very long names correctly.
    /// Verifies that long names are processed and initials are extracted properly.
    /// </summary>
    [TestMethod]
    public void Constructor_WithUserHavingVeryLongName_InitializesCorrectly()
    {
        // Arrange
        var mockAuthService = new Mock<IAuthService>();
        var sessionService = new SessionService();
        var longName = new string('A', 500) + " " + new string('B', 500);
        var user = new AuthUserDto
        {
            FullName = longName,
            Email = "test@example.com",
            Role = "Student"
        };
        sessionService.SetUser(user);

        // Act
        var viewModel = new ProfileViewModel(mockAuthService.Object, new Mock<IProfileService>().Object, sessionService, new Mock<ILogger<ProfileViewModel>>().Object);

        // Assert
        Assert.IsNotNull(viewModel.LogoutCommand);
        Assert.AreEqual(longName, viewModel.FullName);
        Assert.AreEqual("test@example.com", viewModel.Email);
    }

    /// <summary>
    /// Tests that the constructor handles users with special characters in names.
    /// Verifies that names with special characters are stored correctly.
    /// </summary>
    [TestMethod]
    public void Constructor_WithUserHavingSpecialCharactersInName_InitializesCorrectly()
    {
        // Arrange
        var mockAuthService = new Mock<IAuthService>();
        var sessionService = new SessionService();
        var user = new AuthUserDto
        {
            FullName = "Jean-François O'Neil",
            Email = "test@example.com",
            Role = "Student"
        };
        sessionService.SetUser(user);

        // Act
        var viewModel = new ProfileViewModel(mockAuthService.Object, new Mock<IProfileService>().Object, sessionService, new Mock<ILogger<ProfileViewModel>>().Object);

        // Assert
        Assert.IsNotNull(viewModel.LogoutCommand);
        Assert.AreEqual("Jean-François O'Neil", viewModel.FullName);
        Assert.AreEqual("test@example.com", viewModel.Email);
    }

    /// <summary>
    /// Tests that the constructor handles users with empty email correctly.
    /// Verifies that empty email strings are stored as-is.
    /// </summary>
    [TestMethod]
    public void Constructor_WithUserHavingEmptyEmail_InitializesCorrectly()
    {
        // Arrange
        var mockAuthService = new Mock<IAuthService>();
        var sessionService = new SessionService();
        var user = new AuthUserDto
        {
            FullName = "Test User",
            Email = string.Empty,
            Role = "Student"
        };
        sessionService.SetUser(user);

        // Act
        var viewModel = new ProfileViewModel(mockAuthService.Object, new Mock<IProfileService>().Object, sessionService, new Mock<ILogger<ProfileViewModel>>().Object);

        // Assert
        Assert.IsNotNull(viewModel.LogoutCommand);
        Assert.AreEqual(string.Empty, viewModel.Email);
    }

    /// <summary>
    /// Tests that the constructor handles users with null email correctly.
    /// Verifies that null email values are stored properly.
    /// </summary>
    [TestMethod]
    public void Constructor_WithUserHavingNullEmail_InitializesCorrectly()
    {
        // Arrange
        var mockAuthService = new Mock<IAuthService>();
        var sessionService = new SessionService();
        var user = new AuthUserDto
        {
            FullName = "Test User",
            Email = null!,
            Role = "Student"
        };
        sessionService.SetUser(user);

        // Act
        var viewModel = new ProfileViewModel(mockAuthService.Object, new Mock<IProfileService>().Object, sessionService, new Mock<ILogger<ProfileViewModel>>().Object);

        // Assert
        Assert.IsNotNull(viewModel.LogoutCommand);
    }

    /// <summary>
    /// Tests that the constructor handles a user with single-word name.
    /// Verifies that initials are extracted correctly from single-word names.
    /// </summary>
    [TestMethod]
    public void Constructor_WithUserHavingSingleWordName_InitializesCorrectly()
    {
        // Arrange
        var mockAuthService = new Mock<IAuthService>();
        var sessionService = new SessionService();
        var user = new AuthUserDto
        {
            FullName = "Madonna",
            Email = "madonna@example.com",
            Role = "Student"
        };
        sessionService.SetUser(user);

        // Act
        var viewModel = new ProfileViewModel(mockAuthService.Object, new Mock<IProfileService>().Object, sessionService, new Mock<ILogger<ProfileViewModel>>().Object);

        // Assert
        Assert.IsNotNull(viewModel.LogoutCommand);
        Assert.AreEqual("Madonna", viewModel.FullName);
        Assert.AreEqual("madonna@example.com", viewModel.Email);
    }

    /// <summary>
    /// Tests that the Role property returns the default value of empty string when not set.
    /// </summary>
    [TestMethod]
    public void Role_DefaultValue_ReturnsEmptyString()
    {
        // Arrange
        Mock<IAuthService> authServiceMock = new Mock<IAuthService>();
        Mock<SessionService> sessionServiceMock = new Mock<SessionService>();
        ProfileViewModel viewModel = new ProfileViewModel(authServiceMock.Object, new Mock<IProfileService>().Object, sessionServiceMock.Object, new Mock<ILogger<ProfileViewModel>>().Object);

        // Act
        string result = viewModel.Role;

        // Assert
        Assert.AreEqual(string.Empty, result);
    }

    /// <summary>
    /// Tests that the Role property correctly sets and gets various string values.
    /// </summary>
    /// <param name="value">The value to set on the Role property.</param>
    [TestMethod]
    [DataRow("Admin")]
    [DataRow("User")]
    [DataRow("Manager")]
    [DataRow("")]
    [DataRow(" ")]
    [DataRow("   ")]
    [DataRow("Role with spaces")]
    [DataRow("RoleWithSpecialChars!@#$%^&*()")]
    [DataRow("VeryLongRoleNameThatExceedsNormalExpectationsAndContainsManyCharactersToTestBoundaryConditionsForStringHandling")]
    public void Role_SetValue_ReturnsSetValue(string value)
    {
        // Arrange
        Mock<IAuthService> authServiceMock = new Mock<IAuthService>();
        Mock<SessionService> sessionServiceMock = new Mock<SessionService>();
        ProfileViewModel viewModel = new ProfileViewModel(authServiceMock.Object, new Mock<IProfileService>().Object, sessionServiceMock.Object, new Mock<ILogger<ProfileViewModel>>().Object);

        // Act
        viewModel.Role = value;
        string result = viewModel.Role;

        // Assert
        Assert.AreEqual(value, result);
    }

    /// <summary>
    /// Tests that the Role property raises PropertyChanged event when value changes.
    /// </summary>
    [TestMethod]
    public void Role_SetNewValue_RaisesPropertyChangedEvent()
    {
        // Arrange
        Mock<IAuthService> authServiceMock = new Mock<IAuthService>();
        Mock<SessionService> sessionServiceMock = new Mock<SessionService>();
        ProfileViewModel viewModel = new ProfileViewModel(authServiceMock.Object, new Mock<IProfileService>().Object, sessionServiceMock.Object, new Mock<ILogger<ProfileViewModel>>().Object);
        bool eventRaised = false;
        string? propertyName = null;

        viewModel.PropertyChanged += (sender, args) =>
        {
            eventRaised = true;
            propertyName = args.PropertyName;
        };

        // Act
        viewModel.Role = "Administrator";

        // Assert
        Assert.IsTrue(eventRaised);
        Assert.AreEqual("Role", propertyName);
    }

    /// <summary>
    /// Tests that the Role property does not raise PropertyChanged event when set to the same value.
    /// </summary>
    [TestMethod]
    public void Role_SetSameValue_DoesNotRaisePropertyChangedEvent()
    {
        // Arrange
        Mock<IAuthService> authServiceMock = new Mock<IAuthService>();
        Mock<SessionService> sessionServiceMock = new Mock<SessionService>();
        ProfileViewModel viewModel = new ProfileViewModel(authServiceMock.Object, new Mock<IProfileService>().Object, sessionServiceMock.Object, new Mock<ILogger<ProfileViewModel>>().Object);
        int eventCount = 0;

        viewModel.Role = "Manager";

        viewModel.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == "Role")
            {
                eventCount++;
            }
        };

        // Act
        viewModel.Role = "Manager";

        // Assert
        Assert.AreEqual(0, eventCount);
    }

    /// <summary>
    /// Tests that the Role property raises PropertyChanged event multiple times for different values.
    /// </summary>
    [TestMethod]
    public void Role_SetDifferentValues_RaisesPropertyChangedEventMultipleTimes()
    {
        // Arrange
        Mock<IAuthService> authServiceMock = new Mock<IAuthService>();
        Mock<SessionService> sessionServiceMock = new Mock<SessionService>();
        ProfileViewModel viewModel = new ProfileViewModel(authServiceMock.Object, new Mock<IProfileService>().Object, sessionServiceMock.Object, new Mock<ILogger<ProfileViewModel>>().Object);
        int eventCount = 0;

        viewModel.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == "Role")
            {
                eventCount++;
            }
        };

        // Act
        viewModel.Role = "Admin";
        viewModel.Role = "User";
        viewModel.Role = "Manager";

        // Assert
        Assert.AreEqual(3, eventCount);
    }

    /// <summary>
    /// Tests that the Role property correctly handles setting from empty string to non-empty string.
    /// </summary>
    [TestMethod]
    public void Role_SetFromEmptyToNonEmpty_RaisesPropertyChangedEvent()
    {
        // Arrange
        Mock<IAuthService> authServiceMock = new Mock<IAuthService>();
        Mock<SessionService> sessionServiceMock = new Mock<SessionService>();
        ProfileViewModel viewModel = new ProfileViewModel(authServiceMock.Object, new Mock<IProfileService>().Object, sessionServiceMock.Object, new Mock<ILogger<ProfileViewModel>>().Object);
        bool eventRaised = false;

        viewModel.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == "Role")
            {
                eventRaised = true;
            }
        };

        // Act
        viewModel.Role = "Developer";

        // Assert
        Assert.IsTrue(eventRaised);
        Assert.AreEqual("Developer", viewModel.Role);
    }

    /// <summary>
    /// Tests that the Role property correctly handles setting from non-empty to empty string.
    /// </summary>
    [TestMethod]
    public void Role_SetFromNonEmptyToEmpty_RaisesPropertyChangedEvent()
    {
        // Arrange
        Mock<IAuthService> authServiceMock = new Mock<IAuthService>();
        Mock<SessionService> sessionServiceMock = new Mock<SessionService>();
        ProfileViewModel viewModel = new ProfileViewModel(authServiceMock.Object, new Mock<IProfileService>().Object, sessionServiceMock.Object, new Mock<ILogger<ProfileViewModel>>().Object);
        viewModel.Role = "Admin";
        bool eventRaised = false;

        viewModel.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == "Role")
            {
                eventRaised = true;
            }
        };

        // Act
        viewModel.Role = string.Empty;

        // Assert
        Assert.IsTrue(eventRaised);
        Assert.AreEqual(string.Empty, viewModel.Role);
    }

    /// <summary>
    /// Verifies that the Initials property returns an empty string when initially accessed.
    /// </summary>
    [TestMethod]
    public void Initials_Get_ReturnsInitialEmptyString()
    {
        // Arrange
        var mockAuthService = new Mock<IAuthService>();
        var mockSessionService = new Mock<SessionService>();
        var viewModel = new ProfileViewModel(mockAuthService.Object, new Mock<IProfileService>().Object, mockSessionService.Object, new Mock<ILogger<ProfileViewModel>>().Object);

        // Act
        var result = viewModel.Initials;

        // Assert
        Assert.AreEqual(string.Empty, result);
    }

    /// <summary>
    /// Verifies that setting a valid string value to the Initials property updates the property
    /// and raises the PropertyChanged event with the correct property name.
    /// </summary>
    /// <param name="value">The value to set.</param>
    [TestMethod]
    [DataRow("AB")]
    [DataRow("JD")]
    [DataRow("X")]
    [DataRow("ABC")]
    public void Initials_SetValidString_UpdatesPropertyAndRaisesPropertyChanged(string value)
    {
        // Arrange
        var mockAuthService = new Mock<IAuthService>();
        var mockSessionService = new Mock<SessionService>();
        var viewModel = new ProfileViewModel(mockAuthService.Object, new Mock<IProfileService>().Object, mockSessionService.Object, new Mock<ILogger<ProfileViewModel>>().Object);
        var propertyChangedRaised = false;
        var propertyName = string.Empty;

        viewModel.PropertyChanged += (sender, args) =>
        {
            propertyChangedRaised = true;
            propertyName = args.PropertyName ?? string.Empty;
        };

        // Act
        viewModel.Initials = value;

        // Assert
        Assert.AreEqual(value, viewModel.Initials);
        Assert.IsTrue(propertyChangedRaised);
        Assert.AreEqual("Initials", propertyName);
    }

    /// <summary>
    /// Verifies that setting the same value twice to the Initials property does not raise
    /// the PropertyChanged event on the second assignment.
    /// </summary>
    [TestMethod]
    public void Initials_SetSameValueTwice_DoesNotRaisePropertyChangedSecondTime()
    {
        // Arrange
        var mockAuthService = new Mock<IAuthService>();
        var mockSessionService = new Mock<SessionService>();
        var viewModel = new ProfileViewModel(mockAuthService.Object, new Mock<IProfileService>().Object, mockSessionService.Object, new Mock<ILogger<ProfileViewModel>>().Object);
        var eventRaiseCount = 0;

        viewModel.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == "Initials")
            {
                eventRaiseCount++;
            }
        };

        // Act
        viewModel.Initials = "AB";
        viewModel.Initials = "AB";

        // Assert
        Assert.AreEqual("AB", viewModel.Initials);
        Assert.AreEqual(1, eventRaiseCount);
    }

    /// <summary>
    /// Verifies that setting an empty string to the Initials property updates the property correctly
    /// and raises the PropertyChanged event.
    /// </summary>
    [TestMethod]
    public void Initials_SetEmptyString_UpdatesPropertyAndRaisesPropertyChanged()
    {
        // Arrange
        var mockAuthService = new Mock<IAuthService>();
        var mockSessionService = new Mock<SessionService>();
        var viewModel = new ProfileViewModel(mockAuthService.Object, new Mock<IProfileService>().Object, mockSessionService.Object, new Mock<ILogger<ProfileViewModel>>().Object);
        viewModel.Initials = "AB";
        var propertyChangedRaised = false;

        viewModel.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == "Initials")
            {
                propertyChangedRaised = true;
            }
        };

        // Act
        viewModel.Initials = string.Empty;

        // Assert
        Assert.AreEqual(string.Empty, viewModel.Initials);
        Assert.IsTrue(propertyChangedRaised);
    }

    /// <summary>
    /// Verifies that setting whitespace strings to the Initials property updates the property correctly.
    /// </summary>
    /// <param name="value">The whitespace string to set.</param>
    [TestMethod]
    [DataRow(" ")]
    [DataRow("  ")]
    [DataRow("\t")]
    [DataRow("\n")]
    [DataRow(" \t\n ")]
    public void Initials_SetWhitespaceString_UpdatesProperty(string value)
    {
        // Arrange
        var mockAuthService = new Mock<IAuthService>();
        var mockSessionService = new Mock<SessionService>();
        var viewModel = new ProfileViewModel(mockAuthService.Object, new Mock<IProfileService>().Object, mockSessionService.Object, new Mock<ILogger<ProfileViewModel>>().Object);

        // Act
        viewModel.Initials = value;

        // Assert
        Assert.AreEqual(value, viewModel.Initials);
    }

    /// <summary>
    /// Verifies that setting a very long string to the Initials property updates the property correctly.
    /// </summary>
    [TestMethod]
    public void Initials_SetLongString_UpdatesProperty()
    {
        // Arrange
        var mockAuthService = new Mock<IAuthService>();
        var mockSessionService = new Mock<SessionService>();
        var viewModel = new ProfileViewModel(mockAuthService.Object, new Mock<IProfileService>().Object, mockSessionService.Object, new Mock<ILogger<ProfileViewModel>>().Object);
        var longString = new string('A', 10000);

        // Act
        viewModel.Initials = longString;

        // Assert
        Assert.AreEqual(longString, viewModel.Initials);
    }

    /// <summary>
    /// Verifies that setting strings with special characters to the Initials property updates the property correctly.
    /// </summary>
    /// <param name="value">The string with special characters to set.</param>
    [TestMethod]
    [DataRow("A@B")]
    [DataRow("!@#$%")]
    [DataRow("A\u0000B")]
    [DataRow("????")]
    [DataRow("A\rB\nC")]
    public void Initials_SetSpecialCharacters_UpdatesProperty(string value)
    {
        // Arrange
        var mockAuthService = new Mock<IAuthService>();
        var mockSessionService = new Mock<SessionService>();
        var viewModel = new ProfileViewModel(mockAuthService.Object, new Mock<IProfileService>().Object, mockSessionService.Object, new Mock<ILogger<ProfileViewModel>>().Object);

        // Act
        viewModel.Initials = value;

        // Assert
        Assert.AreEqual(value, viewModel.Initials);
    }

    /// <summary>
    /// Verifies that setting different values multiple times to the Initials property updates
    /// the property correctly each time and raises the PropertyChanged event for each change.
    /// </summary>
    [TestMethod]
    public void Initials_SetMultipleTimes_UpdatesPropertyEachTimeAndRaisesEvent()
    {
        // Arrange
        var mockAuthService = new Mock<IAuthService>();
        var mockSessionService = new Mock<SessionService>();
        var viewModel = new ProfileViewModel(mockAuthService.Object, new Mock<IProfileService>().Object, mockSessionService.Object, new Mock<ILogger<ProfileViewModel>>().Object);
        var eventRaiseCount = 0;

        viewModel.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == "Initials")
            {
                eventRaiseCount++;
            }
        };

        // Act & Assert
        viewModel.Initials = "AB";
        Assert.AreEqual("AB", viewModel.Initials);
        Assert.AreEqual(1, eventRaiseCount);

        viewModel.Initials = "CD";
        Assert.AreEqual("CD", viewModel.Initials);
        Assert.AreEqual(2, eventRaiseCount);

        viewModel.Initials = "EF";
        Assert.AreEqual("EF", viewModel.Initials);
        Assert.AreEqual(3, eventRaiseCount);
    }

    /// <summary>
    /// Tests that RefreshProfile sets default values when CurrentUser is null.
    /// </summary>
    [TestMethod]
    public void RefreshProfile_WhenCurrentUserIsNull_SetsDefaultValues()
    {
        // Arrange
        Mock<IAuthService> mockAuthService = new Mock<IAuthService>();
        SessionService sessionService = new SessionService();
        ProfileViewModel viewModel = new ProfileViewModel(mockAuthService.Object, new Mock<IProfileService>().Object, sessionService, new Mock<ILogger<ProfileViewModel>>().Object);

        // Act
        viewModel.RefreshProfile();

        // Assert
        Assert.AreEqual("Student", viewModel.FullName);
        Assert.AreEqual(string.Empty, viewModel.Email);
        Assert.AreEqual("Student", viewModel.Role);
        Assert.AreEqual("S", viewModel.Initials);
    }

    /// <summary>
    /// Tests that RefreshProfile correctly sets properties when CurrentUser has valid data.
    /// </summary>
    [TestMethod]
    [DataRow("John Doe", "john.doe@example.com", "admin", "Admin", "JD")]
    [DataRow("Jane Smith", "jane.smith@example.com", "lecturer", "Lecturer", "JS")]
    [DataRow("Bob Johnson", "bob.johnson@example.com", "classrep", "ClassRepresentative", "BJ")]
    [DataRow("Alice Brown", "alice.brown@example.com", "student", "Student", "AB")]
    public void RefreshProfile_WhenCurrentUserHasValidData_SetsPropertiesCorrectly(
        string fullName, string email, string role, string expectedRole, string expectedInitials)
    {
        // Arrange
        Mock<IAuthService> mockAuthService = new Mock<IAuthService>();
        SessionService sessionService = new SessionService();
        AuthUserDto user = new AuthUserDto
        {
            Id = "123",
            FullName = fullName,
            Email = email,
            Role = role
        };
        sessionService.SetUser(user);
        ProfileViewModel viewModel = new ProfileViewModel(mockAuthService.Object, new Mock<IProfileService>().Object, sessionService, new Mock<ILogger<ProfileViewModel>>().Object);

        // Act
        viewModel.RefreshProfile();

        // Assert
        Assert.AreEqual(fullName, viewModel.FullName);
        Assert.AreEqual(email, viewModel.Email);
        Assert.AreEqual(expectedRole, viewModel.Role);
        Assert.AreEqual(expectedInitials, viewModel.Initials);
    }

    /// <summary>
    /// Tests that RefreshProfile defaults FullName to "Student" when user FullName is null.
    /// </summary>
    [TestMethod]
    public void RefreshProfile_WhenUserFullNameIsNull_SetsFullNameToStudent()
    {
        // Arrange
        Mock<IAuthService> mockAuthService = new Mock<IAuthService>();
        SessionService sessionService = new SessionService();
        AuthUserDto user = new AuthUserDto
        {
            Id = "123",
            FullName = null!,
            Email = "test@example.com",
            Role = "student"
        };
        sessionService.SetUser(user);
        ProfileViewModel viewModel = new ProfileViewModel(mockAuthService.Object, new Mock<IProfileService>().Object, sessionService, new Mock<ILogger<ProfileViewModel>>().Object);

        // Act
        viewModel.RefreshProfile();

        // Assert
        Assert.AreEqual("Student", viewModel.FullName);
        Assert.AreEqual("test@example.com", viewModel.Email);
        Assert.AreEqual("S", viewModel.Initials);
    }

    /// <summary>
    /// Tests that RefreshProfile defaults FullName to "Student" when user FullName is empty.
    /// </summary>
    [TestMethod]
    public void RefreshProfile_WhenUserFullNameIsEmpty_SetsFullNameToStudent()
    {
        // Arrange
        Mock<IAuthService> mockAuthService = new Mock<IAuthService>();
        SessionService sessionService = new SessionService();
        AuthUserDto user = new AuthUserDto
        {
            Id = "123",
            FullName = string.Empty,
            Email = "test@example.com",
            Role = "student"
        };
        sessionService.SetUser(user);
        ProfileViewModel viewModel = new ProfileViewModel(mockAuthService.Object, new Mock<IProfileService>().Object, sessionService, new Mock<ILogger<ProfileViewModel>>().Object);

        // Act
        viewModel.RefreshProfile();

        // Assert
        Assert.AreEqual("Student", viewModel.FullName);
        Assert.AreEqual("test@example.com", viewModel.Email);
        Assert.AreEqual("S", viewModel.Initials);
    }

    /// <summary>
    /// Tests that RefreshProfile defaults FullName to "Student" when user FullName is whitespace.
    /// </summary>
    [TestMethod]
    [DataRow("   ")]
    [DataRow("\t")]
    [DataRow("\n")]
    [DataRow("  \t  \n  ")]
    public void RefreshProfile_WhenUserFullNameIsWhitespace_SetsFullNameToStudent(string whitespace)
    {
        // Arrange
        Mock<IAuthService> mockAuthService = new Mock<IAuthService>();
        SessionService sessionService = new SessionService();
        AuthUserDto user = new AuthUserDto
        {
            Id = "123",
            FullName = whitespace,
            Email = "test@example.com",
            Role = "student"
        };
        sessionService.SetUser(user);
        ProfileViewModel viewModel = new ProfileViewModel(mockAuthService.Object, new Mock<IProfileService>().Object, sessionService, new Mock<ILogger<ProfileViewModel>>().Object);

        // Act
        viewModel.RefreshProfile();

        // Assert
        Assert.AreEqual("Student", viewModel.FullName);
        Assert.AreEqual("test@example.com", viewModel.Email);
        Assert.AreEqual("S", viewModel.Initials);
    }

    /// <summary>
    /// Tests that RefreshProfile correctly handles single name (no spaces).
    /// </summary>
    [TestMethod]
    public void RefreshProfile_WhenUserFullNameIsSingleWord_SetsInitialsToFirstLetter()
    {
        // Arrange
        Mock<IAuthService> mockAuthService = new Mock<IAuthService>();
        SessionService sessionService = new SessionService();
        AuthUserDto user = new AuthUserDto
        {
            Id = "123",
            FullName = "Madonna",
            Email = "madonna@example.com",
            Role = "student"
        };
        sessionService.SetUser(user);
        ProfileViewModel viewModel = new ProfileViewModel(mockAuthService.Object, new Mock<IProfileService>().Object, sessionService, new Mock<ILogger<ProfileViewModel>>().Object);

        // Act
        viewModel.RefreshProfile();

        // Assert
        Assert.AreEqual("Madonna", viewModel.FullName);
        Assert.AreEqual("madonna@example.com", viewModel.Email);
        Assert.AreEqual("M", viewModel.Initials);
    }

    /// <summary>
    /// Tests that RefreshProfile correctly handles multiple name parts (more than 2 words).
    /// </summary>
    [TestMethod]
    public void RefreshProfile_WhenUserFullNameHasMultipleParts_SetsInitialsToFirstAndLast()
    {
        // Arrange
        Mock<IAuthService> mockAuthService = new Mock<IAuthService>();
        SessionService sessionService = new SessionService();
        AuthUserDto user = new AuthUserDto
        {
            Id = "123",
            FullName = "John Michael Smith",
            Email = "jms@example.com",
            Role = "student"
        };
        sessionService.SetUser(user);
        ProfileViewModel viewModel = new ProfileViewModel(mockAuthService.Object, new Mock<IProfileService>().Object, sessionService, new Mock<ILogger<ProfileViewModel>>().Object);

        // Act
        viewModel.RefreshProfile();

        // Assert
        Assert.AreEqual("John Michael Smith", viewModel.FullName);
        Assert.AreEqual("jms@example.com", viewModel.Email);
        Assert.AreEqual("JS", viewModel.Initials);
    }

    /// <summary>
    /// Tests that RefreshProfile correctly handles email with various formats.
    /// </summary>
    [TestMethod]
    [DataRow("")]
    [DataRow("test@example.com")]
    [DataRow("user.name+tag@example.co.uk")]
    public void RefreshProfile_WhenUserHasDifferentEmailFormats_SetsEmailCorrectly(string email)
    {
        // Arrange
        Mock<IAuthService> mockAuthService = new Mock<IAuthService>();
        SessionService sessionService = new SessionService();
        AuthUserDto user = new AuthUserDto
        {
            Id = "123",
            FullName = "Test User",
            Email = email,
            Role = "student"
        };
        sessionService.SetUser(user);
        ProfileViewModel viewModel = new ProfileViewModel(mockAuthService.Object, new Mock<IProfileService>().Object, sessionService, new Mock<ILogger<ProfileViewModel>>().Object);

        // Act
        viewModel.RefreshProfile();

        // Assert
        Assert.AreEqual(email, viewModel.Email);
    }

    /// <summary>
    /// Tests that RefreshProfile correctly handles names with leading/trailing spaces.
    /// </summary>
    [TestMethod]
    public void RefreshProfile_WhenUserFullNameHasLeadingTrailingSpaces_TrimsAndSetsInitials()
    {
        // Arrange
        Mock<IAuthService> mockAuthService = new Mock<IAuthService>();
        SessionService sessionService = new SessionService();
        AuthUserDto user = new AuthUserDto
        {
            Id = "123",
            FullName = "  John   Doe  ",
            Email = "john@example.com",
            Role = "student"
        };
        sessionService.SetUser(user);
        ProfileViewModel viewModel = new ProfileViewModel(mockAuthService.Object, new Mock<IProfileService>().Object, sessionService, new Mock<ILogger<ProfileViewModel>>().Object);

        // Act
        viewModel.RefreshProfile();

        // Assert
        Assert.AreEqual("  John   Doe  ", viewModel.FullName);
        Assert.AreEqual("JD", viewModel.Initials);
    }

    /// <summary>
    /// Tests that RefreshProfile can be called multiple times and updates properties correctly.
    /// </summary>
    [TestMethod]
    public void RefreshProfile_WhenCalledMultipleTimes_UpdatesPropertiesCorrectly()
    {
        // Arrange
        Mock<IAuthService> mockAuthService = new Mock<IAuthService>();
        SessionService sessionService = new SessionService();
        ProfileViewModel viewModel = new ProfileViewModel(mockAuthService.Object, new Mock<IProfileService>().Object, sessionService, new Mock<ILogger<ProfileViewModel>>().Object);

        // Act & Assert - First call with null user
        viewModel.RefreshProfile();
        Assert.AreEqual("Student", viewModel.FullName);
        Assert.AreEqual(string.Empty, viewModel.Email);

        // Set a user
        AuthUserDto user = new AuthUserDto
        {
            Id = "123",
            FullName = "Jane Doe",
            Email = "jane@example.com",
            Role = "admin"
        };
        sessionService.SetUser(user);

        // Act & Assert - Second call with valid user
        viewModel.RefreshProfile();
        Assert.AreEqual("Jane Doe", viewModel.FullName);
        Assert.AreEqual("jane@example.com", viewModel.Email);
        Assert.AreEqual("Admin", viewModel.Role);
        Assert.AreEqual("JD", viewModel.Initials);

        // Clear the session
        sessionService.Clear();

        // Act & Assert - Third call after clearing
        viewModel.RefreshProfile();
        Assert.AreEqual("Student", viewModel.FullName);
        Assert.AreEqual(string.Empty, viewModel.Email);
        Assert.AreEqual("Student", viewModel.Role);
        Assert.AreEqual("S", viewModel.Initials);
    }

    /// <summary>
    /// Tests that RefreshProfile correctly handles lowercase initials conversion.
    /// </summary>
    [TestMethod]
    public void RefreshProfile_WhenUserFullNameHasLowercaseLetters_ConvertsInitialsToUppercase()
    {
        // Arrange
        Mock<IAuthService> mockAuthService = new Mock<IAuthService>();
        SessionService sessionService = new SessionService();
        AuthUserDto user = new AuthUserDto
        {
            Id = "123",
            FullName = "john doe",
            Email = "john@example.com",
            Role = "student"
        };
        sessionService.SetUser(user);
        ProfileViewModel viewModel = new ProfileViewModel(mockAuthService.Object, new Mock<IProfileService>().Object, sessionService, new Mock<ILogger<ProfileViewModel>>().Object);

        // Act
        viewModel.RefreshProfile();

        // Assert
        Assert.AreEqual("john doe", viewModel.FullName);
        Assert.AreEqual("JD", viewModel.Initials);
    }

    /// <summary>
    /// Tests that RefreshProfile correctly handles special characters in names.
    /// </summary>
    [TestMethod]
    [DataRow("Jean-Pierre Dubois", "JD")]
    [DataRow("Mary O'Brien", "MO")]
    [DataRow("José García", "JG")]
    public void RefreshProfile_WhenUserFullNameHasSpecialCharacters_SetsInitialsCorrectly(string fullName, string expectedInitials)
    {
        // Arrange
        Mock<IAuthService> mockAuthService = new Mock<IAuthService>();
        SessionService sessionService = new SessionService();
        AuthUserDto user = new AuthUserDto
        {
            Id = "123",
            FullName = fullName,
            Email = "test@example.com",
            Role = "student"
        };
        sessionService.SetUser(user);
        ProfileViewModel viewModel = new ProfileViewModel(mockAuthService.Object, new Mock<IProfileService>().Object, sessionService, new Mock<ILogger<ProfileViewModel>>().Object);

        // Act
        viewModel.RefreshProfile();

        // Assert
        Assert.AreEqual(fullName, viewModel.FullName);
        Assert.AreEqual(expectedInitials, viewModel.Initials);
    }

    /// <summary>
    /// Tests that the Email property getter returns the expected value after initialization.
    /// Input: Default initialization.
    /// Expected: Returns empty string.
    /// </summary>
    [TestMethod]
    public void Email_InitialValue_ReturnsEmptyString()
    {
        // Arrange
        var authServiceMock = new Mock<IAuthService>();
        var sessionServiceMock = new Mock<SessionService>();
        var viewModel = new ProfileViewModel(authServiceMock.Object, new Mock<IProfileService>().Object, sessionServiceMock.Object, new Mock<ILogger<ProfileViewModel>>().Object);

        // Act
        var result = viewModel.Email;

        // Assert
        Assert.AreEqual(string.Empty, result);
    }

    /// <summary>
    /// Tests that the Email property setter updates the value correctly and raises PropertyChanged event.
    /// Input: Valid email string.
    /// Expected: Email property is updated and PropertyChanged event is raised.
    /// </summary>
    [TestMethod]
    [DataRow("user@example.com")]
    [DataRow("test.user@domain.co.uk")]
    [DataRow("admin@company.org")]
    public void Email_SetValidValue_UpdatesPropertyAndRaisesPropertyChanged(string email)
    {
        // Arrange
        var authServiceMock = new Mock<IAuthService>();
        var sessionServiceMock = new Mock<SessionService>();
        var viewModel = new ProfileViewModel(authServiceMock.Object, new Mock<IProfileService>().Object, sessionServiceMock.Object, new Mock<ILogger<ProfileViewModel>>().Object);
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
        Assert.AreEqual(nameof(ProfileViewModel.Email), changedPropertyName);
    }

    /// <summary>
    /// Tests that the Email property setter handles empty string correctly.
    /// Input: Empty string.
    /// Expected: Email property is updated to empty string and PropertyChanged event is raised.
    /// </summary>
    [TestMethod]
    public void Email_SetEmptyString_UpdatesPropertyAndRaisesPropertyChanged()
    {
        // Arrange
        var authServiceMock = new Mock<IAuthService>();
        var sessionServiceMock = new Mock<SessionService>();
        var viewModel = new ProfileViewModel(authServiceMock.Object, new Mock<IProfileService>().Object, sessionServiceMock.Object, new Mock<ILogger<ProfileViewModel>>().Object);
        viewModel.Email = "initial@example.com";
        var propertyChangedRaised = false;

        viewModel.PropertyChanged += (sender, args) =>
        {
            propertyChangedRaised = true;
        };

        // Act
        viewModel.Email = string.Empty;

        // Assert
        Assert.AreEqual(string.Empty, viewModel.Email);
        Assert.IsTrue(propertyChangedRaised);
    }

    /// <summary>
    /// Tests that the Email property setter handles whitespace strings correctly.
    /// Input: Whitespace-only strings (space, tab, newline).
    /// Expected: Email property is updated to the whitespace string and PropertyChanged event is raised.
    /// </summary>
    [TestMethod]
    [DataRow(" ")]
    [DataRow("   ")]
    [DataRow("\t")]
    [DataRow("\n")]
    [DataRow("\r\n")]
    public void Email_SetWhitespaceString_UpdatesPropertyAndRaisesPropertyChanged(string whitespace)
    {
        // Arrange
        var authServiceMock = new Mock<IAuthService>();
        var sessionServiceMock = new Mock<SessionService>();
        var viewModel = new ProfileViewModel(authServiceMock.Object, new Mock<IProfileService>().Object, sessionServiceMock.Object, new Mock<ILogger<ProfileViewModel>>().Object);
        var propertyChangedRaised = false;

        viewModel.PropertyChanged += (sender, args) =>
        {
            propertyChangedRaised = true;
        };

        // Act
        viewModel.Email = whitespace;

        // Assert
        Assert.AreEqual(whitespace, viewModel.Email);
        Assert.IsTrue(propertyChangedRaised);
    }

    /// <summary>
    /// Tests that the Email property setter handles strings with special characters correctly.
    /// Input: Strings with special characters.
    /// Expected: Email property is updated and PropertyChanged event is raised.
    /// </summary>
    [TestMethod]
    [DataRow("user+tag@example.com")]
    [DataRow("user_name@example.com")]
    [DataRow("user-name@example.com")]
    [DataRow("user.name+tag@sub.example.com")]
    [DataRow("user@domain-with-dash.com")]
    public void Email_SetStringWithSpecialCharacters_UpdatesPropertyAndRaisesPropertyChanged(string email)
    {
        // Arrange
        var authServiceMock = new Mock<IAuthService>();
        var sessionServiceMock = new Mock<SessionService>();
        var viewModel = new ProfileViewModel(authServiceMock.Object, new Mock<IProfileService>().Object, sessionServiceMock.Object, new Mock<ILogger<ProfileViewModel>>().Object);
        var propertyChangedRaised = false;

        viewModel.PropertyChanged += (sender, args) =>
        {
            propertyChangedRaised = true;
        };

        // Act
        viewModel.Email = email;

        // Assert
        Assert.AreEqual(email, viewModel.Email);
        Assert.IsTrue(propertyChangedRaised);
    }

    /// <summary>
    /// Tests that the Email property setter handles very long strings correctly.
    /// Input: Very long string (1000 characters).
    /// Expected: Email property is updated and PropertyChanged event is raised.
    /// </summary>
    [TestMethod]
    public void Email_SetVeryLongString_UpdatesPropertyAndRaisesPropertyChanged()
    {
        // Arrange
        var authServiceMock = new Mock<IAuthService>();
        var sessionServiceMock = new Mock<SessionService>();
        var viewModel = new ProfileViewModel(authServiceMock.Object, new Mock<IProfileService>().Object, sessionServiceMock.Object, new Mock<ILogger<ProfileViewModel>>().Object);
        var longEmail = new string('a', 1000) + "@example.com";
        var propertyChangedRaised = false;

        viewModel.PropertyChanged += (sender, args) =>
        {
            propertyChangedRaised = true;
        };

        // Act
        viewModel.Email = longEmail;

        // Assert
        Assert.AreEqual(longEmail, viewModel.Email);
        Assert.IsTrue(propertyChangedRaised);
    }

    /// <summary>
    /// Tests that the Email property setter does not raise PropertyChanged event when the value is the same.
    /// Input: Same value as current.
    /// Expected: Email property remains unchanged and PropertyChanged event is not raised.
    /// </summary>
    [TestMethod]
    public void Email_SetSameValue_DoesNotRaisePropertyChanged()
    {
        // Arrange
        var authServiceMock = new Mock<IAuthService>();
        var sessionServiceMock = new Mock<SessionService>();
        var viewModel = new ProfileViewModel(authServiceMock.Object, new Mock<IProfileService>().Object, sessionServiceMock.Object, new Mock<ILogger<ProfileViewModel>>().Object);
        var email = "user@example.com";
        viewModel.Email = email;
        var propertyChangedRaised = false;

        viewModel.PropertyChanged += (sender, args) =>
        {
            propertyChangedRaised = true;
        };

        // Act
        viewModel.Email = email;

        // Assert
        Assert.AreEqual(email, viewModel.Email);
        Assert.IsFalse(propertyChangedRaised);
    }

    /// <summary>
    /// Tests that the Email property setter handles strings with control characters correctly.
    /// Input: Strings with control characters.
    /// Expected: Email property is updated and PropertyChanged event is raised.
    /// </summary>
    [TestMethod]
    [DataRow("user\u0000@example.com")]
    [DataRow("user\u0001@example.com")]
    [DataRow("user\u001F@example.com")]
    public void Email_SetStringWithControlCharacters_UpdatesPropertyAndRaisesPropertyChanged(string email)
    {
        // Arrange
        var authServiceMock = new Mock<IAuthService>();
        var sessionServiceMock = new Mock<SessionService>();
        var viewModel = new ProfileViewModel(authServiceMock.Object, new Mock<IProfileService>().Object, sessionServiceMock.Object, new Mock<ILogger<ProfileViewModel>>().Object);
        var propertyChangedRaised = false;

        viewModel.PropertyChanged += (sender, args) =>
        {
            propertyChangedRaised = true;
        };

        // Act
        viewModel.Email = email;

        // Assert
        Assert.AreEqual(email, viewModel.Email);
        Assert.IsTrue(propertyChangedRaised);
    }

    /// <summary>
    /// Tests that the Email property setter handles Unicode characters correctly.
    /// Input: Strings with Unicode characters.
    /// Expected: Email property is updated and PropertyChanged event is raised.
    /// </summary>
    [TestMethod]
    [DataRow("??@??.jp")]
    [DataRow("utilisateur@société.fr")]
    [DataRow("usuario@compañía.es")]
    public void Email_SetStringWithUnicodeCharacters_UpdatesPropertyAndRaisesPropertyChanged(string email)
    {
        // Arrange
        var authServiceMock = new Mock<IAuthService>();
        var sessionServiceMock = new Mock<SessionService>();
        var viewModel = new ProfileViewModel(authServiceMock.Object, new Mock<IProfileService>().Object, sessionServiceMock.Object, new Mock<ILogger<ProfileViewModel>>().Object);
        var propertyChangedRaised = false;

        viewModel.PropertyChanged += (sender, args) =>
        {
            propertyChangedRaised = true;
        };

        // Act
        viewModel.Email = email;

        // Assert
        Assert.AreEqual(email, viewModel.Email);
        Assert.IsTrue(propertyChangedRaised);
    }

    /// <summary>
    /// Tests that the Initials property returns an empty string initially.
    /// Input: Default initialization.
    /// Expected: Initials property returns empty string.
    /// </summary>
    [TestMethod]
    public void Initials_InitialValue_ReturnsEmptyString()
    {
        // Arrange
        var mockAuthService = new Mock<IAuthService>();
        var mockSession = new Mock<SessionService>();

        // Act
        var viewModel = new ProfileViewModel(mockAuthService.Object, new Mock<IProfileService>().Object, mockSession.Object, new Mock<ILogger<ProfileViewModel>>().Object);

        // Assert
        Assert.AreEqual(string.Empty, viewModel.Initials);
    }

    /// <summary>
    /// Tests that setting the same value twice does not raise PropertyChanged event on the second assignment.
    /// Input: Same string value set consecutively.
    /// Expected: PropertyChanged event is raised only once on the first assignment.
    /// </summary>
    [TestMethod]
    public void Initials_SetSameValueTwice_DoesNotRaisePropertyChangedOnSecondSet()
    {
        // Arrange
        var mockAuthService = new Mock<IAuthService>();
        var mockSession = new Mock<SessionService>();
        var viewModel = new ProfileViewModel(mockAuthService.Object, new Mock<IProfileService>().Object, mockSession.Object, new Mock<ILogger<ProfileViewModel>>().Object);
        viewModel.Initials = "AB";
        int eventCount = 0;
        viewModel.PropertyChanged += (sender, args) => { if (args.PropertyName == "Initials") eventCount++; };

        // Act
        viewModel.Initials = "AB";

        // Assert
        Assert.AreEqual("AB", viewModel.Initials);
        Assert.AreEqual(0, eventCount);
    }

    /// <summary>
    /// Tests that setting Initials to whitespace-only strings updates the property correctly.
    /// Input: Various whitespace strings (space, tab, newline, combinations).
    /// Expected: Initials property is updated to the whitespace string and PropertyChanged event is raised.
    /// </summary>
    [TestMethod]
    [DataRow(" ")]
    [DataRow("  ")]
    [DataRow("   ")]
    [DataRow("\t")]
    [DataRow("\n")]
    [DataRow("\r")]
    [DataRow("\r\n")]
    [DataRow(" \t\n ")]
    [DataRow("\t\t\t")]
    public void Initials_SetWhitespaceString_UpdatesPropertyAndRaisesPropertyChanged(string value)
    {
        // Arrange
        var mockAuthService = new Mock<IAuthService>();
        var mockSession = new Mock<SessionService>();
        var viewModel = new ProfileViewModel(mockAuthService.Object, new Mock<IProfileService>().Object, mockSession.Object, new Mock<ILogger<ProfileViewModel>>().Object);
        string? propertyChangedName = null;
        viewModel.PropertyChanged += (sender, args) => propertyChangedName = args.PropertyName;

        // Act
        viewModel.Initials = value;

        // Assert
        Assert.AreEqual(value, viewModel.Initials);
        Assert.AreEqual("Initials", propertyChangedName);
    }

    /// <summary>
    /// Tests that setting Initials to a very long string updates the property correctly.
    /// Input: String with 1000 characters.
    /// Expected: Initials property is updated to the long string and PropertyChanged event is raised.
    /// </summary>
    [TestMethod]
    public void Initials_SetVeryLongString_UpdatesPropertyAndRaisesPropertyChanged()
    {
        // Arrange
        var mockAuthService = new Mock<IAuthService>();
        var mockSession = new Mock<SessionService>();
        var viewModel = new ProfileViewModel(mockAuthService.Object, new Mock<IProfileService>().Object, mockSession.Object, new Mock<ILogger<ProfileViewModel>>().Object);
        var longString = new string('A', 1000);
        string? propertyChangedName = null;
        viewModel.PropertyChanged += (sender, args) => propertyChangedName = args.PropertyName;

        // Act
        viewModel.Initials = longString;

        // Assert
        Assert.AreEqual(longString, viewModel.Initials);
        Assert.AreEqual("Initials", propertyChangedName);
    }

    /// <summary>
    /// Tests that setting Initials to strings with special characters updates the property correctly.
    /// Input: Strings containing special characters, Unicode, emoji, control characters.
    /// Expected: Initials property is updated and PropertyChanged event is raised.
    /// </summary>
    [TestMethod]
    [DataRow("A@B")]
    [DataRow("!@#$%^&*()")]
    [DataRow("A\u0000B")]
    [DataRow("????")]
    [DataRow("A\rB\nC")]
    [DataRow("A<>&\"'B")]
    [DataRow("José")]
    [DataRow("Müller")]
    [DataRow("??")]
    public void Initials_SetStringWithSpecialCharacters_UpdatesPropertyAndRaisesPropertyChanged(string value)
    {
        // Arrange
        var mockAuthService = new Mock<IAuthService>();
        var mockSession = new Mock<SessionService>();
        var viewModel = new ProfileViewModel(mockAuthService.Object, new Mock<IProfileService>().Object, mockSession.Object, new Mock<ILogger<ProfileViewModel>>().Object);
        string? propertyChangedName = null;
        viewModel.PropertyChanged += (sender, args) => propertyChangedName = args.PropertyName;

        // Act
        viewModel.Initials = value;

        // Assert
        Assert.AreEqual(value, viewModel.Initials);
        Assert.AreEqual("Initials", propertyChangedName);
    }

    /// <summary>
    /// Tests that setting multiple different values consecutively raises PropertyChanged event for each change.
    /// Input: Sequence of different string values.
    /// Expected: PropertyChanged event is raised for each distinct value change.
    /// </summary>
    [TestMethod]
    public void Initials_SetMultipleDifferentValues_RaisesPropertyChangedForEachChange()
    {
        // Arrange
        var mockAuthService = new Mock<IAuthService>();
        var mockSession = new Mock<SessionService>();
        var viewModel = new ProfileViewModel(mockAuthService.Object, new Mock<IProfileService>().Object, mockSession.Object, new Mock<ILogger<ProfileViewModel>>().Object);
        int eventCount = 0;
        viewModel.PropertyChanged += (sender, args) => { if (args.PropertyName == "Initials") eventCount++; };

        // Act
        viewModel.Initials = "AB";
        viewModel.Initials = "CD";
        viewModel.Initials = "EF";

        // Assert
        Assert.AreEqual("EF", viewModel.Initials);
        Assert.AreEqual(3, eventCount);
    }

    /// <summary>
    /// Tests that setting Initials from empty to a value and back to empty raises PropertyChanged both times.
    /// Input: Empty string -> "AB" -> empty string.
    /// Expected: PropertyChanged event is raised twice (once for each change).
    /// </summary>
    [TestMethod]
    public void Initials_SetFromEmptyToValueAndBackToEmpty_RaisesPropertyChangedTwice()
    {
        // Arrange
        var mockAuthService = new Mock<IAuthService>();
        var mockSession = new Mock<SessionService>();
        var viewModel = new ProfileViewModel(mockAuthService.Object, new Mock<IProfileService>().Object, mockSession.Object, new Mock<ILogger<ProfileViewModel>>().Object);
        int eventCount = 0;
        viewModel.PropertyChanged += (sender, args) => { if (args.PropertyName == "Initials") eventCount++; };

        // Act
        viewModel.Initials = "AB";
        viewModel.Initials = string.Empty;

        // Assert
        Assert.AreEqual(string.Empty, viewModel.Initials);
        Assert.AreEqual(2, eventCount);
    }

    /// <summary>
    /// Tests that the PropertyChanged event provides the correct property name.
    /// Input: Any valid string value.
    /// Expected: PropertyChanged event args contain property name "Initials".
    /// </summary>
    [TestMethod]
    public void Initials_SetValue_PropertyChangedEventContainsCorrectPropertyName()
    {
        // Arrange
        var mockAuthService = new Mock<IAuthService>();
        var mockSession = new Mock<SessionService>();
        var viewModel = new ProfileViewModel(mockAuthService.Object, new Mock<IProfileService>().Object, mockSession.Object, new Mock<ILogger<ProfileViewModel>>().Object);
        PropertyChangedEventArgs? eventArgs = null;
        viewModel.PropertyChanged += (sender, args) => eventArgs = args;

        // Act
        viewModel.Initials = "XY";

        // Assert
        Assert.IsNotNull(eventArgs);
        Assert.AreEqual("Initials", eventArgs.PropertyName);
    }

    /// <summary>
    /// Tests that setting Initials with strings containing only control characters updates the property.
    /// Input: Strings with various control characters.
    /// Expected: Initials property is updated and PropertyChanged event is raised.
    /// </summary>
    [TestMethod]
    [DataRow("\u0000")]
    [DataRow("\u0001")]
    [DataRow("\u001F")]
    [DataRow("\u007F")]
    public void Initials_SetStringWithControlCharactersOnly_UpdatesPropertyAndRaisesPropertyChanged(string value)
    {
        // Arrange
        var mockAuthService = new Mock<IAuthService>();
        var mockSession = new Mock<SessionService>();
        var viewModel = new ProfileViewModel(mockAuthService.Object, new Mock<IProfileService>().Object, mockSession.Object, new Mock<ILogger<ProfileViewModel>>().Object);
        string? propertyChangedName = null;
        viewModel.PropertyChanged += (sender, args) => propertyChangedName = args.PropertyName;

        // Act
        viewModel.Initials = value;

        // Assert
        Assert.AreEqual(value, viewModel.Initials);
        Assert.AreEqual("Initials", propertyChangedName);
    }

    /// <summary>
    /// Tests that setting Initials to the same value after multiple changes does not raise PropertyChanged.
    /// Input: "AB" -> "CD" -> "AB" (same as first value).
    /// Expected: PropertyChanged is not raised for the third assignment.
    /// </summary>
    [TestMethod]
    public void Initials_SetSameValueAfterDifferentValue_DoesNotRaisePropertyChangedOnSecondOccurrence()
    {
        // Arrange
        var mockAuthService = new Mock<IAuthService>();
        var mockSession = new Mock<SessionService>();
        var viewModel = new ProfileViewModel(mockAuthService.Object, new Mock<IProfileService>().Object, mockSession.Object, new Mock<ILogger<ProfileViewModel>>().Object);
        viewModel.Initials = "AB";
        viewModel.Initials = "CD";
        int eventCount = 0;
        viewModel.PropertyChanged += (sender, args) => { if (args.PropertyName == "Initials") eventCount++; };

        // Act
        viewModel.Initials = "CD";

        // Assert
        Assert.AreEqual("CD", viewModel.Initials);
        Assert.AreEqual(0, eventCount);
    }

    /// <summary>
    /// Tests boundary case of int.MaxValue length string (if memory allows).
    /// Input: Extremely long string.
    /// Expected: Property handles large strings without error.
    /// Note: This test may be skipped if memory constraints prevent creating such a large string.
    /// </summary>
    [TestMethod]
    public void Initials_SetExtremelyLongString_UpdatesPropertyCorrectly()
    {
        // Arrange
        var mockAuthService = new Mock<IAuthService>();
        var mockSession = new Mock<SessionService>();
        var viewModel = new ProfileViewModel(mockAuthService.Object, new Mock<IProfileService>().Object, mockSession.Object, new Mock<ILogger<ProfileViewModel>>().Object);
        var veryLongString = new string('Z', 100000);
        string? propertyChangedName = null;
        viewModel.PropertyChanged += (sender, args) => propertyChangedName = args.PropertyName;

        // Act
        viewModel.Initials = veryLongString;

        // Assert
        Assert.AreEqual(veryLongString, viewModel.Initials);
        Assert.AreEqual("Initials", propertyChangedName);
    }

    /// <summary>
    /// Tests that setting Initials to lowercase letters stores them as-is without conversion.
    /// Input: Lowercase string values.
    /// Expected: Initials property stores lowercase values without modification.
    /// </summary>
    [TestMethod]
    [DataRow("ab")]
    [DataRow("jd")]
    [DataRow("xyz")]
    public void Initials_SetLowercaseString_StoresValueAsIs(string value)
    {
        // Arrange
        var mockAuthService = new Mock<IAuthService>();
        var mockSession = new Mock<SessionService>();
        var viewModel = new ProfileViewModel(mockAuthService.Object, new Mock<IProfileService>().Object, mockSession.Object, new Mock<ILogger<ProfileViewModel>>().Object);

        // Act
        viewModel.Initials = value;

        // Assert
        Assert.AreEqual(value, viewModel.Initials);
    }

    /// <summary>
    /// Tests that setting Initials with mixed case strings stores them as-is.
    /// Input: Mixed case string values.
    /// Expected: Initials property preserves the exact case.
    /// </summary>
    [TestMethod]
    [DataRow("Ab")]
    [DataRow("aB")]
    [DataRow("AbC")]
    [DataRow("aBc")]
    public void Initials_SetMixedCaseString_PreservesCase(string value)
    {
        // Arrange
        var mockAuthService = new Mock<IAuthService>();
        var mockSession = new Mock<SessionService>();
        var viewModel = new ProfileViewModel(mockAuthService.Object, new Mock<IProfileService>().Object, mockSession.Object, new Mock<ILogger<ProfileViewModel>>().Object);

        // Act
        viewModel.Initials = value;

        // Assert
        Assert.AreEqual(value, viewModel.Initials);
    }

    /// <summary>
    /// Tests that RefreshProfile sets Email to null when CurrentUser.Email is null.
    /// Input: CurrentUser with null Email property.
    /// Expected: Email property is set to null.
    /// </summary>
    [TestMethod]
    public void RefreshProfile_WhenUserEmailIsNull_SetsEmailToNull()
    {
        // Arrange
        Mock<IAuthService> mockAuthService = new Mock<IAuthService>();
        SessionService sessionService = new SessionService();
        AuthUserDto user = new AuthUserDto
        {
            Id = "123",
            FullName = "John Doe",
            Email = null!,
            Role = "student"
        };
        sessionService.SetUser(user);
        ProfileViewModel viewModel = new ProfileViewModel(mockAuthService.Object, new Mock<IProfileService>().Object, sessionService, new Mock<ILogger<ProfileViewModel>>().Object);

        // Act
        viewModel.RefreshProfile();

        // Assert
        Assert.AreEqual("John Doe", viewModel.FullName);
        Assert.IsNull(viewModel.Email);
        Assert.AreEqual("Student", viewModel.Role);
        Assert.AreEqual("JD", viewModel.Initials);
    }

    /// <summary>
    /// Tests that RefreshProfile handles very long user names correctly.
    /// Input: CurrentUser with very long FullName.
    /// Expected: FullName is set correctly and initials are extracted from first and last parts.
    /// </summary>
    [TestMethod]
    public void RefreshProfile_WhenUserFullNameIsVeryLong_HandlesCorrectly()
    {
        // Arrange
        Mock<IAuthService> mockAuthService = new Mock<IAuthService>();
        SessionService sessionService = new SessionService();
        string veryLongName = new string('A', 500) + " " + new string('Z', 500);
        AuthUserDto user = new AuthUserDto
        {
            Id = "123",
            FullName = veryLongName,
            Email = "test@example.com",
            Role = "admin"
        };
        sessionService.SetUser(user);
        ProfileViewModel viewModel = new ProfileViewModel(mockAuthService.Object, new Mock<IProfileService>().Object, sessionService, new Mock<ILogger<ProfileViewModel>>().Object);

        // Act
        viewModel.RefreshProfile();

        // Assert
        Assert.AreEqual(veryLongName, viewModel.FullName);
        Assert.AreEqual("test@example.com", viewModel.Email);
        Assert.AreEqual("Admin", viewModel.Role);
        Assert.AreEqual("AZ", viewModel.Initials);
    }

    /// <summary>
    /// Tests that RefreshProfile handles names with multiple spaces between words.
    /// Input: CurrentUser with FullName containing multiple spaces.
    /// Expected: Initials are extracted from first and last non-empty parts.
    /// </summary>
    [TestMethod]
    public void RefreshProfile_WhenUserFullNameHasMultipleSpaces_ExtractsInitialsCorrectly()
    {
        // Arrange
        Mock<IAuthService> mockAuthService = new Mock<IAuthService>();
        SessionService sessionService = new SessionService();
        AuthUserDto user = new AuthUserDto
        {
            Id = "123",
            FullName = "John    Michael    Doe",
            Email = "john@example.com",
            Role = "lecturer"
        };
        sessionService.SetUser(user);
        ProfileViewModel viewModel = new ProfileViewModel(mockAuthService.Object, new Mock<IProfileService>().Object, sessionService, new Mock<ILogger<ProfileViewModel>>().Object);

        // Act
        viewModel.RefreshProfile();

        // Assert
        Assert.AreEqual("John    Michael    Doe", viewModel.FullName);
        Assert.AreEqual("john@example.com", viewModel.Email);
        Assert.AreEqual("Lecturer", viewModel.Role);
        Assert.AreEqual("JD", viewModel.Initials);
    }

    /// <summary>
    /// Tests that RefreshProfile handles names with only spaces (whitespace-only) correctly.
    /// Input: CurrentUser with FullName containing only spaces.
    /// Expected: FullName defaults to "Student" and Initials to "S".
    /// </summary>
    [TestMethod]
    [DataRow("     ")]
    [DataRow("\t\t\t")]
    [DataRow("  \n  \r\n  ")]
    public void RefreshProfile_WhenUserFullNameIsOnlyWhitespace_SetsDefaultValues(string whitespace)
    {
        // Arrange
        Mock<IAuthService> mockAuthService = new Mock<IAuthService>();
        SessionService sessionService = new SessionService();
        AuthUserDto user = new AuthUserDto
        {
            Id = "123",
            FullName = whitespace,
            Email = "test@example.com",
            Role = "admin"
        };
        sessionService.SetUser(user);
        ProfileViewModel viewModel = new ProfileViewModel(mockAuthService.Object, new Mock<IProfileService>().Object, sessionService, new Mock<ILogger<ProfileViewModel>>().Object);

        // Act
        viewModel.RefreshProfile();

        // Assert
        Assert.AreEqual("Student", viewModel.FullName);
        Assert.AreEqual("test@example.com", viewModel.Email);
        Assert.AreEqual("Admin", viewModel.Role);
        Assert.AreEqual("S", viewModel.Initials);
    }

    /// <summary>
    /// Tests that RefreshProfile handles role "classrep" correctly.
    /// Input: CurrentUser with role "classrep".
    /// Expected: Role is set to "ClassRepresentative".
    /// </summary>
    [TestMethod]
    public void RefreshProfile_WhenUserRoleIsClassRep_SetsRoleToClassRepresentative()
    {
        // Arrange
        Mock<IAuthService> mockAuthService = new Mock<IAuthService>();
        SessionService sessionService = new SessionService();
        AuthUserDto user = new AuthUserDto
        {
            Id = "123",
            FullName = "Class Rep",
            Email = "classrep@example.com",
            Role = "classrep"
        };
        sessionService.SetUser(user);
        ProfileViewModel viewModel = new ProfileViewModel(mockAuthService.Object, new Mock<IProfileService>().Object, sessionService, new Mock<ILogger<ProfileViewModel>>().Object);

        // Act
        viewModel.RefreshProfile();

        // Assert
        Assert.AreEqual("Class Rep", viewModel.FullName);
        Assert.AreEqual("classrep@example.com", viewModel.Email);
        Assert.AreEqual("ClassRepresentative", viewModel.Role);
        Assert.AreEqual("CR", viewModel.Initials);
    }

    /// <summary>
    /// Tests that RefreshProfile handles role "class_representative" correctly.
    /// Input: CurrentUser with role "class_representative".
    /// Expected: Role is set to "ClassRepresentative".
    /// </summary>
    [TestMethod]
    public void RefreshProfile_WhenUserRoleIsClassRepresentativeWithUnderscore_SetsRoleToClassRepresentative()
    {
        // Arrange
        Mock<IAuthService> mockAuthService = new Mock<IAuthService>();
        SessionService sessionService = new SessionService();
        AuthUserDto user = new AuthUserDto
        {
            Id = "123",
            FullName = "Representative",
            Email = "rep@example.com",
            Role = "class_representative"
        };
        sessionService.SetUser(user);
        ProfileViewModel viewModel = new ProfileViewModel(mockAuthService.Object, new Mock<IProfileService>().Object, sessionService, new Mock<ILogger<ProfileViewModel>>().Object);

        // Act
        viewModel.RefreshProfile();

        // Assert
        Assert.AreEqual("Representative", viewModel.FullName);
        Assert.AreEqual("rep@example.com", viewModel.Email);
        Assert.AreEqual("ClassRepresentative", viewModel.Role);
        Assert.AreEqual("R", viewModel.Initials);
    }

    /// <summary>
    /// Tests that RefreshProfile handles unrecognized role values by defaulting to Student.
    /// Input: CurrentUser with unrecognized role value.
    /// Expected: Role is set to "Student".
    /// </summary>
    [TestMethod]
    [DataRow("")]
    [DataRow("unknown")]
    [DataRow("ADMIN")]
    [DataRow("Teacher")]
    [DataRow("InvalidRole")]
    public void RefreshProfile_WhenUserRoleIsUnrecognized_SetsRoleToStudent(string role)
    {
        // Arrange
        Mock<IAuthService> mockAuthService = new Mock<IAuthService>();
        SessionService sessionService = new SessionService();
        AuthUserDto user = new AuthUserDto
        {
            Id = "123",
            FullName = "Test User",
            Email = "test@example.com",
            Role = role
        };
        sessionService.SetUser(user);
        ProfileViewModel viewModel = new ProfileViewModel(mockAuthService.Object, new Mock<IProfileService>().Object, sessionService, new Mock<ILogger<ProfileViewModel>>().Object);

        // Act
        viewModel.RefreshProfile();

        // Assert
        Assert.AreEqual("Test User", viewModel.FullName);
        Assert.AreEqual("test@example.com", viewModel.Email);
        Assert.AreEqual("Student", viewModel.Role);
        Assert.AreEqual("TU", viewModel.Initials);
    }

    /// <summary>
    /// Tests that RefreshProfile handles null user role by defaulting to Student.
    /// Input: CurrentUser with null role.
    /// Expected: Role is set to "Student".
    /// </summary>
    [TestMethod]
    public void RefreshProfile_WhenUserRoleIsNull_SetsRoleToStudent()
    {
        // Arrange
        Mock<IAuthService> mockAuthService = new Mock<IAuthService>();
        SessionService sessionService = new SessionService();
        AuthUserDto user = new AuthUserDto
        {
            Id = "123",
            FullName = "Test User",
            Email = "test@example.com",
            Role = null!
        };
        sessionService.SetUser(user);
        ProfileViewModel viewModel = new ProfileViewModel(mockAuthService.Object, new Mock<IProfileService>().Object, sessionService, new Mock<ILogger<ProfileViewModel>>().Object);

        // Act
        viewModel.RefreshProfile();

        // Assert
        Assert.AreEqual("Test User", viewModel.FullName);
        Assert.AreEqual("test@example.com", viewModel.Email);
        Assert.AreEqual("Student", viewModel.Role);
        Assert.AreEqual("TU", viewModel.Initials);
    }

    /// <summary>
    /// Tests that RefreshProfile handles mixed case role values correctly.
    /// Input: CurrentUser with mixed case role values.
    /// Expected: Role is normalized and set correctly.
    /// </summary>
    [TestMethod]
    [DataRow("Admin", "Admin")]
    [DataRow("ADMIN", "Admin")]
    [DataRow("AdMiN", "Admin")]
    [DataRow("Lecturer", "Lecturer")]
    [DataRow("LECTURER", "Lecturer")]
    [DataRow("ClassRepresentative", "ClassRepresentative")]
    [DataRow("CLASSREPRESENTATIVE", "ClassRepresentative")]
    public void RefreshProfile_WhenUserRoleHasMixedCase_NormalizesRoleCorrectly(string inputRole, string expectedRole)
    {
        // Arrange
        Mock<IAuthService> mockAuthService = new Mock<IAuthService>();
        SessionService sessionService = new SessionService();
        AuthUserDto user = new AuthUserDto
        {
            Id = "123",
            FullName = "Test User",
            Email = "test@example.com",
            Role = inputRole
        };
        sessionService.SetUser(user);
        ProfileViewModel viewModel = new ProfileViewModel(mockAuthService.Object, new Mock<IProfileService>().Object, sessionService, new Mock<ILogger<ProfileViewModel>>().Object);

        // Act
        viewModel.RefreshProfile();

        // Assert
        Assert.AreEqual("Test User", viewModel.FullName);
        Assert.AreEqual("test@example.com", viewModel.Email);
        Assert.AreEqual(expectedRole, viewModel.Role);
        Assert.AreEqual("TU", viewModel.Initials);
    }

    /// <summary>
    /// Tests that RefreshProfile handles empty email string correctly.
    /// Input: CurrentUser with empty Email property.
    /// Expected: Email property is set to empty string.
    /// </summary>
    [TestMethod]
    public void RefreshProfile_WhenUserEmailIsEmpty_SetsEmailToEmpty()
    {
        // Arrange
        Mock<IAuthService> mockAuthService = new Mock<IAuthService>();
        SessionService sessionService = new SessionService();
        AuthUserDto user = new AuthUserDto
        {
            Id = "123",
            FullName = "John Doe",
            Email = string.Empty,
            Role = "student"
        };
        sessionService.SetUser(user);
        ProfileViewModel viewModel = new ProfileViewModel(mockAuthService.Object, new Mock<IProfileService>().Object, sessionService, new Mock<ILogger<ProfileViewModel>>().Object);

        // Act
        viewModel.RefreshProfile();

        // Assert
        Assert.AreEqual("John Doe", viewModel.FullName);
        Assert.AreEqual(string.Empty, viewModel.Email);
        Assert.AreEqual("Student", viewModel.Role);
        Assert.AreEqual("JD", viewModel.Initials);
    }

    /// <summary>
    /// Tests that RefreshProfile handles names starting with whitespace correctly.
    /// Input: CurrentUser with FullName that has leading and trailing whitespace but is not whitespace-only.
    /// Expected: FullName is preserved as-is, initials are extracted from trimmed parts.
    /// </summary>
    [TestMethod]
    public void RefreshProfile_WhenUserFullNameHasLeadingAndTrailingWhitespace_PreservesNameAndExtractsInitials()
    {
        // Arrange
        Mock<IAuthService> mockAuthService = new Mock<IAuthService>();
        SessionService sessionService = new SessionService();
        AuthUserDto user = new AuthUserDto
        {
            Id = "123",
            FullName = "  John   Doe  ",
            Email = "john@example.com",
            Role = "admin"
        };
        sessionService.SetUser(user);
        ProfileViewModel viewModel = new ProfileViewModel(mockAuthService.Object, new Mock<IProfileService>().Object, sessionService, new Mock<ILogger<ProfileViewModel>>().Object);

        // Act
        viewModel.RefreshProfile();

        // Assert
        Assert.AreEqual("  John   Doe  ", viewModel.FullName);
        Assert.AreEqual("john@example.com", viewModel.Email);
        Assert.AreEqual("Admin", viewModel.Role);
        Assert.AreEqual("JD", viewModel.Initials);
    }

    /// <summary>
    /// Tests that RefreshProfile handles names with tabs and newlines correctly.
    /// Input: CurrentUser with FullName containing tabs and newlines between words.
    /// Expected: FullName is preserved, initials are extracted from non-empty parts.
    /// </summary>
    [TestMethod]
    public void RefreshProfile_WhenUserFullNameHasTabsAndNewlines_ExtractsInitialsCorrectly()
    {
        // Arrange
        Mock<IAuthService> mockAuthService = new Mock<IAuthService>();
        SessionService sessionService = new SessionService();
        AuthUserDto user = new AuthUserDto
        {
            Id = "123",
            FullName = "John\tMiddle\nDoe",
            Email = "john@example.com",
            Role = "lecturer"
        };
        sessionService.SetUser(user);
        ProfileViewModel viewModel = new ProfileViewModel(mockAuthService.Object, new Mock<IProfileService>().Object, sessionService, new Mock<ILogger<ProfileViewModel>>().Object);

        // Act
        viewModel.RefreshProfile();

        // Assert
        Assert.AreEqual("John\tMiddle\nDoe", viewModel.FullName);
        Assert.AreEqual("john@example.com", viewModel.Email);
        Assert.AreEqual("Lecturer", viewModel.Role);
        Assert.AreEqual("JD", viewModel.Initials);
    }

    /// <summary>
    /// Tests that RefreshProfile handles names with numbers correctly.
    /// Input: CurrentUser with FullName containing numbers.
    /// Expected: FullName is preserved, initials are extracted from first characters of parts.
    /// </summary>
    [TestMethod]
    public void RefreshProfile_WhenUserFullNameHasNumbers_ExtractsInitialsCorrectly()
    {
        // Arrange
        Mock<IAuthService> mockAuthService = new Mock<IAuthService>();
        SessionService sessionService = new SessionService();
        AuthUserDto user = new AuthUserDto
        {
            Id = "123",
            FullName = "John123 Doe456",
            Email = "john@example.com",
            Role = "student"
        };
        sessionService.SetUser(user);
        ProfileViewModel viewModel = new ProfileViewModel(mockAuthService.Object, new Mock<IProfileService>().Object, sessionService, new Mock<ILogger<ProfileViewModel>>().Object);

        // Act
        viewModel.RefreshProfile();

        // Assert
        Assert.AreEqual("John123 Doe456", viewModel.FullName);
        Assert.AreEqual("john@example.com", viewModel.Email);
        Assert.AreEqual("Student", viewModel.Role);
        Assert.AreEqual("JD", viewModel.Initials);
    }

    /// <summary>
    /// Tests that RefreshProfile can transition from null user to valid user.
    /// Input: Initially null CurrentUser, then set to valid user and RefreshProfile called again.
    /// Expected: Properties transition from default to user-specific values.
    /// </summary>
    [TestMethod]
    public void RefreshProfile_WhenTransitioningFromNullToValidUser_UpdatesPropertiesCorrectly()
    {
        // Arrange
        Mock<IAuthService> mockAuthService = new Mock<IAuthService>();
        SessionService sessionService = new SessionService();
        ProfileViewModel viewModel = new ProfileViewModel(mockAuthService.Object, new Mock<IProfileService>().Object, sessionService, new Mock<ILogger<ProfileViewModel>>().Object);

        // Verify initial state (null user)
        Assert.AreEqual("Student", viewModel.FullName);
        Assert.AreEqual(string.Empty, viewModel.Email);
        Assert.AreEqual("Student", viewModel.Role);
        Assert.AreEqual("S", viewModel.Initials);

        // Set user and refresh
        AuthUserDto user = new AuthUserDto
        {
            Id = "123",
            FullName = "Jane Smith",
            Email = "jane@example.com",
            Role = "admin"
        };
        sessionService.SetUser(user);

        // Act
        viewModel.RefreshProfile();

        // Assert
        Assert.AreEqual("Jane Smith", viewModel.FullName);
        Assert.AreEqual("jane@example.com", viewModel.Email);
        Assert.AreEqual("Admin", viewModel.Role);
        Assert.AreEqual("JS", viewModel.Initials);
    }

    /// <summary>
    /// Tests that RefreshProfile can transition from valid user to null user.
    /// Input: Initially valid CurrentUser, then cleared and RefreshProfile called again.
    /// Expected: Properties transition from user-specific to default values.
    /// </summary>
    [TestMethod]
    public void RefreshProfile_WhenTransitioningFromValidUserToNull_UpdatesPropertiesCorrectly()
    {
        // Arrange
        Mock<IAuthService> mockAuthService = new Mock<IAuthService>();
        SessionService sessionService = new SessionService();
        AuthUserDto user = new AuthUserDto
        {
            Id = "123",
            FullName = "Jane Smith",
            Email = "jane@example.com",
            Role = "admin"
        };
        sessionService.SetUser(user);
        ProfileViewModel viewModel = new ProfileViewModel(mockAuthService.Object, new Mock<IProfileService>().Object, sessionService, new Mock<ILogger<ProfileViewModel>>().Object);

        // Verify initial state (valid user)
        Assert.AreEqual("Jane Smith", viewModel.FullName);
        Assert.AreEqual("jane@example.com", viewModel.Email);
        Assert.AreEqual("Admin", viewModel.Role);
        Assert.AreEqual("JS", viewModel.Initials);

        // Clear user and refresh
        sessionService.Clear();

        // Act
        viewModel.RefreshProfile();

        // Assert
        Assert.AreEqual("Student", viewModel.FullName);
        Assert.AreEqual(string.Empty, viewModel.Email);
        Assert.AreEqual("Student", viewModel.Role);
        Assert.AreEqual("S", viewModel.Initials);
    }

    /// <summary>
    /// Tests that RefreshProfile handles email with whitespace correctly.
    /// Input: CurrentUser with Email containing whitespace.
    /// Expected: Email is set as-is with whitespace preserved.
    /// </summary>
    [TestMethod]
    public void RefreshProfile_WhenUserEmailHasWhitespace_PreservesWhitespace()
    {
        // Arrange
        Mock<IAuthService> mockAuthService = new Mock<IAuthService>();
        SessionService sessionService = new SessionService();
        AuthUserDto user = new AuthUserDto
        {
            Id = "123",
            FullName = "John Doe",
            Email = "  john@example.com  ",
            Role = "student"
        };
        sessionService.SetUser(user);
        ProfileViewModel viewModel = new ProfileViewModel(mockAuthService.Object, new Mock<IProfileService>().Object, sessionService, new Mock<ILogger<ProfileViewModel>>().Object);

        // Act
        viewModel.RefreshProfile();

        // Assert
        Assert.AreEqual("John Doe", viewModel.FullName);
        Assert.AreEqual("  john@example.com  ", viewModel.Email);
        Assert.AreEqual("Student", viewModel.Role);
        Assert.AreEqual("JD", viewModel.Initials);
    }

    /// <summary>
    /// Tests that the FullName property returns the initial empty string value.
    /// Input: Newly created ProfileViewModel.
    /// Expected: FullName returns empty string.
    /// </summary>
    [TestMethod]
    public void FullName_InitialValue_ReturnsEmptyString()
    {
        // Arrange
        var mockAuthService = new Mock<IAuthService>();
        var mockSessionService = new Mock<SessionService>();
        var viewModel = new ProfileViewModel(mockAuthService.Object, new Mock<IProfileService>().Object, mockSessionService.Object, new Mock<ILogger<ProfileViewModel>>().Object);

        // Act
        var actualValue = viewModel.FullName;

        // Assert
        Assert.AreEqual(string.Empty, actualValue);
    }

    /// <summary>
    /// Tests that setting FullName from a value to empty string and back raises PropertyChanged events.
    /// Input: Non-empty value -> empty string -> non-empty value.
    /// Expected: PropertyChanged event is raised for each change.
    /// </summary>
    [TestMethod]
    public void FullName_SetToValueThenEmptyThenValue_RaisesPropertyChangedEventForEachChange()
    {
        // Arrange
        var mockAuthService = new Mock<IAuthService>();
        var mockSessionService = new Mock<SessionService>();
        var viewModel = new ProfileViewModel(mockAuthService.Object, new Mock<IProfileService>().Object, mockSessionService.Object, new Mock<ILogger<ProfileViewModel>>().Object);
        int eventRaisedCount = 0;
        viewModel.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == "FullName")
                eventRaisedCount++;
        };

        // Act
        viewModel.FullName = "Value";
        viewModel.FullName = string.Empty;
        viewModel.FullName = "Another Value";

        // Assert
        Assert.AreEqual(3, eventRaisedCount);
    }

    /// <summary>
    /// Tests that setting FullName with leading and trailing spaces preserves them.
    /// Input: String with leading and trailing whitespace.
    /// Expected: FullName returns the string with whitespace preserved.
    /// </summary>
    [TestMethod]
    public void FullName_SetStringWithLeadingTrailingSpaces_PreservesSpaces()
    {
        // Arrange
        var mockAuthService = new Mock<IAuthService>();
        var mockSessionService = new Mock<SessionService>();
        var viewModel = new ProfileViewModel(mockAuthService.Object, new Mock<IProfileService>().Object, mockSessionService.Object, new Mock<ILogger<ProfileViewModel>>().Object);
        const string valueWithSpaces = "  John Doe  ";

        // Act
        viewModel.FullName = valueWithSpaces;

        // Assert
        Assert.AreEqual(valueWithSpaces, viewModel.FullName);
    }

    /// <summary>
    /// Tests that FullName handles strings with control characters.
    /// Input: Strings with various control characters.
    /// Expected: FullName returns the string with control characters as-is.
    /// </summary>
    [TestMethod]
    [DataRow("Name\u0001Control")]
    [DataRow("Name\u001FControl")]
    [DataRow("Name\u007FControl")]
    public void FullName_SetStringWithControlCharacters_ReturnsStringWithControlCharacters(string value)
    {
        // Arrange
        var mockAuthService = new Mock<IAuthService>();
        var mockSessionService = new Mock<SessionService>();
        var viewModel = new ProfileViewModel(mockAuthService.Object, new Mock<IProfileService>().Object, mockSessionService.Object, new Mock<ILogger<ProfileViewModel>>().Object);

        // Act
        viewModel.FullName = value;

        // Assert
        Assert.AreEqual(value, viewModel.FullName);
    }

    /// <summary>
    /// Tests that FullName correctly handles boundary case of maximum practical string length.
    /// Input: Extremely long string (100,000 characters).
    /// Expected: FullName returns the extremely long string correctly.
    /// </summary>
    [TestMethod]
    public void FullName_SetExtremelyLongString_ReturnsExtremelyLongString()
    {
        // Arrange
        var mockAuthService = new Mock<IAuthService>();
        var mockSessionService = new Mock<SessionService>();
        var viewModel = new ProfileViewModel(mockAuthService.Object, new Mock<IProfileService>().Object, mockSessionService.Object, new Mock<ILogger<ProfileViewModel>>().Object);
        var extremelyLongString = new string('X', 100000);

        // Act
        viewModel.FullName = extremelyLongString;

        // Assert
        Assert.AreEqual(extremelyLongString, viewModel.FullName);
    }

    /// <summary>
    /// Tests that setting same value repeatedly does not raise PropertyChanged event.
    /// Input: Same value set three times consecutively.
    /// Expected: PropertyChanged event is raised only for the first assignment.
    /// </summary>
    [TestMethod]
    public void FullName_SetSameValueMultipleTimes_RaisesPropertyChangedOnlyOnce()
    {
        // Arrange
        var mockAuthService = new Mock<IAuthService>();
        var mockSessionService = new Mock<SessionService>();
        var viewModel = new ProfileViewModel(mockAuthService.Object, new Mock<IProfileService>().Object, mockSessionService.Object, new Mock<ILogger<ProfileViewModel>>().Object);
        int eventRaisedCount = 0;
        viewModel.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == "FullName")
                eventRaisedCount++;
        };

        // Act
        viewModel.FullName = "Same";
        viewModel.FullName = "Same";
        viewModel.FullName = "Same";

        // Assert
        Assert.AreEqual(1, eventRaisedCount);
    }

    /// <summary>
    /// Tests that PropertyChanged event args contain the correct property name.
    /// Input: Any new value.
    /// Expected: PropertyChanged event args have PropertyName equal to "FullName".
    /// </summary>
    [TestMethod]
    public void FullName_SetValue_PropertyChangedEventContainsCorrectPropertyName()
    {
        // Arrange
        var mockAuthService = new Mock<IAuthService>();
        var mockSessionService = new Mock<SessionService>();
        var viewModel = new ProfileViewModel(mockAuthService.Object, new Mock<IProfileService>().Object, mockSessionService.Object, new Mock<ILogger<ProfileViewModel>>().Object);
        string? capturedPropertyName = null;
        viewModel.PropertyChanged += (sender, args) => capturedPropertyName = args.PropertyName;

        // Act
        viewModel.FullName = "Test Name";

        // Assert
        Assert.AreEqual("FullName", capturedPropertyName);
    }

    /// <summary>
    /// Tests that FullName handles strings with numeric characters.
    /// Input: Strings containing numbers.
    /// Expected: FullName returns the string with numbers as-is.
    /// </summary>
    [TestMethod]
    [DataRow("John123")]
    [DataRow("123")]
    [DataRow("User001")]
    public void FullName_SetStringWithNumbers_ReturnsStringWithNumbers(string value)
    {
        // Arrange
        var mockAuthService = new Mock<IAuthService>();
        var mockSessionService = new Mock<SessionService>();
        var viewModel = new ProfileViewModel(mockAuthService.Object, new Mock<IProfileService>().Object, mockSessionService.Object, new Mock<ILogger<ProfileViewModel>>().Object);

        // Act
        viewModel.FullName = value;

        // Assert
        Assert.AreEqual(value, viewModel.FullName);
    }

    /// <summary>
    /// Tests that FullName handles mixed case strings.
    /// Input: Strings with mixed case characters.
    /// Expected: FullName returns the string with case preserved.
    /// </summary>
    [TestMethod]
    [DataRow("JoHn DoE")]
    [DataRow("UPPERCASE")]
    [DataRow("lowercase")]
    [DataRow("MiXeD CaSe")]
    public void FullName_SetMixedCaseString_PreservesCase(string value)
    {
        // Arrange
        var mockAuthService = new Mock<IAuthService>();
        var mockSessionService = new Mock<SessionService>();
        var viewModel = new ProfileViewModel(mockAuthService.Object, new Mock<IProfileService>().Object, mockSessionService.Object, new Mock<ILogger<ProfileViewModel>>().Object);

        // Act
        viewModel.FullName = value;

        // Assert
        Assert.AreEqual(value, viewModel.FullName);
    }

    /// <summary>
    /// Tests that the constructor initializes LogoutCommand to a non-null value.
    /// Input: Valid authService and session parameters.
    /// Expected: LogoutCommand is not null after construction.
    /// </summary>
    [TestMethod]
    public void Constructor_WithValidParameters_InitializesLogoutCommandNotNull()
    {
        // Arrange
        Mock<IAuthService> mockAuthService = new Mock<IAuthService>();
        SessionService sessionService = new SessionService();

        // Act
        ProfileViewModel viewModel = new ProfileViewModel(mockAuthService.Object, new Mock<IProfileService>().Object, sessionService, new Mock<ILogger<ProfileViewModel>>().Object);

        // Assert
        Assert.IsNotNull(viewModel.LogoutCommand);
    }

    /// <summary>
    /// Tests that the constructor initializes LogoutCommand to an ICommand instance.
    /// Input: Valid authService and session parameters.
    /// Expected: LogoutCommand is of type ICommand.
    /// </summary>
    [TestMethod]
    public void Constructor_WithValidParameters_InitializesLogoutCommandAsICommand()
    {
        // Arrange
        Mock<IAuthService> mockAuthService = new Mock<IAuthService>();
        SessionService sessionService = new SessionService();

        // Act
        ProfileViewModel viewModel = new ProfileViewModel(mockAuthService.Object, new Mock<IProfileService>().Object, sessionService, new Mock<ILogger<ProfileViewModel>>().Object);

        // Assert
        Assert.IsInstanceOfType(viewModel.LogoutCommand, typeof(ICommand));
    }

    /// <summary>
    /// Tests that the constructor with null authService but valid session initializes successfully.
    /// Input: Null authService and valid session.
    /// Expected: ViewModel is created and properties are initialized with default values.
    /// </summary>
    [TestMethod]
    public void Constructor_WithNullAuthServiceAndValidSession_InitializesWithDefaultValues()
    {
        // Arrange
        SessionService sessionService = new SessionService();

        // Act
        ProfileViewModel viewModel = new ProfileViewModel(null!, new Mock<IProfileService>().Object, sessionService, new Mock<ILogger<ProfileViewModel>>().Object);

        // Assert
        Assert.IsNotNull(viewModel);
        Assert.IsNotNull(viewModel.LogoutCommand);
        Assert.AreEqual("Student", viewModel.FullName);
        Assert.AreEqual(string.Empty, viewModel.Email);
        Assert.AreEqual("Student", viewModel.Role);
        Assert.AreEqual("S", viewModel.Initials);
    }

    /// <summary>
    /// Tests that the constructor calls RefreshProfile which sets all properties correctly.
    /// Input: Valid parameters with a user having all properties set.
    /// Expected: All properties (FullName, Email, Role, Initials) are set from the user.
    /// </summary>
    [TestMethod]
    public void Constructor_WithCompleteUserData_SetsAllPropertiesCorrectly()
    {
        // Arrange
        Mock<IAuthService> mockAuthService = new Mock<IAuthService>();
        SessionService sessionService = new SessionService();
        AuthUserDto user = new AuthUserDto
        {
            Id = "test-id-123",
            FullName = "Test User Name",
            Email = "testuser@example.com",
            Role = "admin"
        };
        sessionService.SetUser(user);

        // Act
        ProfileViewModel viewModel = new ProfileViewModel(mockAuthService.Object, new Mock<IProfileService>().Object, sessionService, new Mock<ILogger<ProfileViewModel>>().Object);

        // Assert
        Assert.AreEqual("Test User Name", viewModel.FullName);
        Assert.AreEqual("testuser@example.com", viewModel.Email);
        Assert.AreEqual("Admin", viewModel.Role);
        Assert.AreEqual("TN", viewModel.Initials);
        Assert.IsNotNull(viewModel.LogoutCommand);
    }

    /// <summary>
    /// Tests that the constructor handles minimum valid user data correctly.
    /// Input: User with only required fields (Id and Role), null FullName and Email.
    /// Expected: Default values are used for null properties.
    /// </summary>
    [TestMethod]
    public void Constructor_WithMinimalUserData_UsesDefaultsForNullProperties()
    {
        // Arrange
        Mock<IAuthService> mockAuthService = new Mock<IAuthService>();
        SessionService sessionService = new SessionService();
        AuthUserDto user = new AuthUserDto
        {
            Id = "minimal-id",
            FullName = null!,
            Email = null!,
            Role = "student"
        };
        sessionService.SetUser(user);

        // Act
        ProfileViewModel viewModel = new ProfileViewModel(mockAuthService.Object, new Mock<IProfileService>().Object, sessionService, new Mock<ILogger<ProfileViewModel>>().Object);

        // Assert
        Assert.AreEqual("Student", viewModel.FullName);
        Assert.IsNull(viewModel.Email);
        Assert.AreEqual("Student", viewModel.Role);
        Assert.AreEqual("S", viewModel.Initials);
    }

    /// <summary>
    /// Tests that RefreshProfile handles names with only special characters correctly.
    /// Input: CurrentUser with FullName containing only special characters.
    /// Expected: FullName is preserved, initials are extracted from first character(s).
    /// </summary>
    [TestMethod]
    [DataRow("@#$", "@")]
    [DataRow("!@# $%^", "!^")]
    [DataRow("*** +++", "*+")]
    public void RefreshProfile_WhenUserFullNameHasOnlySpecialCharacters_ExtractsInitialsCorrectly(string fullName, string expectedInitials)
    {
        // Arrange
        Mock<IAuthService> mockAuthService = new Mock<IAuthService>();
        SessionService sessionService = new SessionService();
        AuthUserDto user = new AuthUserDto
        {
            Id = "123",
            FullName = fullName,
            Email = "test@example.com",
            Role = "student"
        };
        sessionService.SetUser(user);
        ProfileViewModel viewModel = new ProfileViewModel(mockAuthService.Object, new Mock<IProfileService>().Object, sessionService, new Mock<ILogger<ProfileViewModel>>().Object);

        // Act
        viewModel.RefreshProfile();

        // Assert
        Assert.AreEqual(fullName, viewModel.FullName);
        Assert.AreEqual(expectedInitials, viewModel.Initials);
    }

    /// <summary>
    /// Tests that RefreshProfile handles names with emoji characters correctly.
    /// Input: CurrentUser with FullName containing emoji.
    /// Expected: FullName is preserved, initials are extracted from emoji characters.
    /// </summary>
    [TestMethod]
    [DataRow("?? ??", "????")]
    [DataRow("??", "??")]
    [DataRow("Test ??", "T??")]
    public void RefreshProfile_WhenUserFullNameHasEmoji_ExtractsInitialsCorrectly(string fullName, string expectedInitials)
    {
        // Arrange
        Mock<IAuthService> mockAuthService = new Mock<IAuthService>();
        SessionService sessionService = new SessionService();
        AuthUserDto user = new AuthUserDto
        {
            Id = "123",
            FullName = fullName,
            Email = "test@example.com",
            Role = "student"
        };
        sessionService.SetUser(user);
        ProfileViewModel viewModel = new ProfileViewModel(mockAuthService.Object, new Mock<IProfileService>().Object, sessionService, new Mock<ILogger<ProfileViewModel>>().Object);

        // Act
        viewModel.RefreshProfile();

        // Assert
        Assert.AreEqual(fullName, viewModel.FullName);
        Assert.AreEqual(expectedInitials, viewModel.Initials);
    }

    /// <summary>
    /// Tests that RefreshProfile handles names with mixed whitespace (spaces, tabs, newlines) correctly.
    /// Input: CurrentUser with FullName containing various types of whitespace.
    /// Expected: Whitespace characters are treated as separators, initials extracted from non-empty parts.
    /// </summary>
    [TestMethod]
    [DataRow("John\tDoe", "JD")]
    [DataRow("John\nDoe", "JD")]
    [DataRow("John\r\nDoe", "JD")]
    [DataRow("John \t \n Doe", "JD")]
    public void RefreshProfile_WhenUserFullNameHasMixedWhitespace_ExtractsInitialsCorrectly(string fullName, string expectedInitials)
    {
        // Arrange
        Mock<IAuthService> mockAuthService = new Mock<IAuthService>();
        SessionService sessionService = new SessionService();
        AuthUserDto user = new AuthUserDto
        {
            Id = "123",
            FullName = fullName,
            Email = "test@example.com",
            Role = "student"
        };
        sessionService.SetUser(user);
        ProfileViewModel viewModel = new ProfileViewModel(mockAuthService.Object, new Mock<IProfileService>().Object, sessionService, new Mock<ILogger<ProfileViewModel>>().Object);

        // Act
        viewModel.RefreshProfile();

        // Assert
        Assert.AreEqual(fullName, viewModel.FullName);
        Assert.AreEqual(expectedInitials, viewModel.Initials);
    }

    /// <summary>
    /// Tests that RefreshProfile handles names with control characters correctly.
    /// Input: CurrentUser with FullName containing control characters.
    /// Expected: FullName is preserved as-is, initials extracted from available characters.
    /// </summary>
    [TestMethod]
    [DataRow("John\u0000Doe", "JD")]
    [DataRow("Test\u0001Name", "TN")]
    public void RefreshProfile_WhenUserFullNameHasControlCharacters_HandlesCorrectly(string fullName, string expectedInitials)
    {
        // Arrange
        Mock<IAuthService> mockAuthService = new Mock<IAuthService>();
        SessionService sessionService = new SessionService();
        AuthUserDto user = new AuthUserDto
        {
            Id = "123",
            FullName = fullName,
            Email = "test@example.com",
            Role = "student"
        };
        sessionService.SetUser(user);
        ProfileViewModel viewModel = new ProfileViewModel(mockAuthService.Object, new Mock<IProfileService>().Object, sessionService, new Mock<ILogger<ProfileViewModel>>().Object);

        // Act
        viewModel.RefreshProfile();

        // Assert
        Assert.AreEqual(fullName, viewModel.FullName);
        Assert.AreEqual(expectedInitials, viewModel.Initials);
    }

    /// <summary>
    /// Tests that RefreshProfile handles names with three or more parts correctly.
    /// Input: CurrentUser with FullName containing multiple words.
    /// Expected: Initials are extracted from first and last parts only.
    /// </summary>
    [TestMethod]
    [DataRow("John Michael Doe", "JD")]
    [DataRow("A B C D E", "AE")]
    [DataRow("First Middle1 Middle2 Last", "FL")]
    public void RefreshProfile_WhenUserFullNameHasThreeOrMoreParts_ExtractsFirstAndLastInitials(string fullName, string expectedInitials)
    {
        // Arrange
        Mock<IAuthService> mockAuthService = new Mock<IAuthService>();
        SessionService sessionService = new SessionService();
        AuthUserDto user = new AuthUserDto
        {
            Id = "123",
            FullName = fullName,
            Email = "test@example.com",
            Role = "student"
        };
        sessionService.SetUser(user);
        ProfileViewModel viewModel = new ProfileViewModel(mockAuthService.Object, new Mock<IProfileService>().Object, sessionService, new Mock<ILogger<ProfileViewModel>>().Object);

        // Act
        viewModel.RefreshProfile();

        // Assert
        Assert.AreEqual(fullName, viewModel.FullName);
        Assert.AreEqual(expectedInitials, viewModel.Initials);
    }

    /// <summary>
    /// Tests that RefreshProfile handles names with single character parts correctly.
    /// Input: CurrentUser with FullName where parts are single characters.
    /// Expected: Initials are extracted correctly from single character parts.
    /// </summary>
    [TestMethod]
    [DataRow("A B", "AB")]
    [DataRow("X Y Z", "XZ")]
    [DataRow("I", "I")]
    public void RefreshProfile_WhenUserFullNameHasSingleCharacterParts_ExtractsInitialsCorrectly(string fullName, string expectedInitials)
    {
        // Arrange
        Mock<IAuthService> mockAuthService = new Mock<IAuthService>();
        SessionService sessionService = new SessionService();
        AuthUserDto user = new AuthUserDto
        {
            Id = "123",
            FullName = fullName,
            Email = "test@example.com",
            Role = "student"
        };
        sessionService.SetUser(user);
        ProfileViewModel viewModel = new ProfileViewModel(mockAuthService.Object, new Mock<IProfileService>().Object, sessionService, new Mock<ILogger<ProfileViewModel>>().Object);

        // Act
        viewModel.RefreshProfile();

        // Assert
        Assert.AreEqual(fullName, viewModel.FullName);
        Assert.AreEqual(expectedInitials, viewModel.Initials);
    }

    /// <summary>
    /// Tests that RefreshProfile handles email with control characters correctly.
    /// Input: CurrentUser with Email containing control characters.
    /// Expected: Email is set as-is without modification.
    /// </summary>
    [TestMethod]
    [DataRow("test\u0000@example.com")]
    [DataRow("user\u0001@domain.com")]
    public void RefreshProfile_WhenUserEmailHasControlCharacters_PreservesEmail(string email)
    {
        // Arrange
        Mock<IAuthService> mockAuthService = new Mock<IAuthService>();
        SessionService sessionService = new SessionService();
        AuthUserDto user = new AuthUserDto
        {
            Id = "123",
            FullName = "John Doe",
            Email = email,
            Role = "student"
        };
        sessionService.SetUser(user);
        ProfileViewModel viewModel = new ProfileViewModel(mockAuthService.Object, new Mock<IProfileService>().Object, sessionService, new Mock<ILogger<ProfileViewModel>>().Object);

        // Act
        viewModel.RefreshProfile();

        // Assert
        Assert.AreEqual(email, viewModel.Email);
    }

    /// <summary>
    /// Tests that RefreshProfile handles very long email addresses correctly.
    /// Input: CurrentUser with very long Email string.
    /// Expected: Email is set correctly without truncation or error.
    /// </summary>
    [TestMethod]
    public void RefreshProfile_WhenUserEmailIsVeryLong_HandlesCorrectly()
    {
        // Arrange
        Mock<IAuthService> mockAuthService = new Mock<IAuthService>();
        SessionService sessionService = new SessionService();
        string veryLongEmail = new string('a', 500) + "@" + new string('b', 500) + ".com";
        AuthUserDto user = new AuthUserDto
        {
            Id = "123",
            FullName = "John Doe",
            Email = veryLongEmail,
            Role = "student"
        };
        sessionService.SetUser(user);
        ProfileViewModel viewModel = new ProfileViewModel(mockAuthService.Object, new Mock<IProfileService>().Object, sessionService, new Mock<ILogger<ProfileViewModel>>().Object);

        // Act
        viewModel.RefreshProfile();

        // Assert
        Assert.AreEqual(veryLongEmail, viewModel.Email);
        Assert.AreEqual("John Doe", viewModel.FullName);
    }

    /// <summary>
    /// Tests that RefreshProfile correctly handles repeated calls with same user data.
    /// Input: Multiple calls to RefreshProfile with unchanged user.
    /// Expected: Properties remain consistent across all calls.
    /// </summary>
    [TestMethod]
    public void RefreshProfile_WhenCalledRepeatedlyWithSameUser_MaintainsConsistency()
    {
        // Arrange
        Mock<IAuthService> mockAuthService = new Mock<IAuthService>();
        SessionService sessionService = new SessionService();
        AuthUserDto user = new AuthUserDto
        {
            Id = "123",
            FullName = "John Doe",
            Email = "john.doe@example.com",
            Role = "student"
        };
        sessionService.SetUser(user);
        ProfileViewModel viewModel = new ProfileViewModel(mockAuthService.Object, new Mock<IProfileService>().Object, sessionService, new Mock<ILogger<ProfileViewModel>>().Object);

        // Act
        viewModel.RefreshProfile();
        string firstFullName = viewModel.FullName;
        string firstEmail = viewModel.Email;
        string firstRole = viewModel.Role;
        string firstInitials = viewModel.Initials;

        viewModel.RefreshProfile();
        string secondFullName = viewModel.FullName;
        string secondEmail = viewModel.Email;
        string secondRole = viewModel.Role;
        string secondInitials = viewModel.Initials;

        viewModel.RefreshProfile();

        // Assert
        Assert.AreEqual(firstFullName, secondFullName);
        Assert.AreEqual(firstEmail, secondEmail);
        Assert.AreEqual(firstRole, secondRole);
        Assert.AreEqual(firstInitials, secondInitials);
        Assert.AreEqual("John Doe", viewModel.FullName);
        Assert.AreEqual("john.doe@example.com", viewModel.Email);
        Assert.AreEqual("Student", viewModel.Role);
        Assert.AreEqual("JD", viewModel.Initials);
    }

    /// <summary>
    /// Tests that RefreshProfile handles names with alternating spaces correctly.
    /// Input: CurrentUser with FullName containing multiple consecutive spaces.
    /// Expected: Multiple spaces are treated as separators, empty parts are removed.
    /// </summary>
    [TestMethod]
    [DataRow("John     Doe", "JD")]
    [DataRow("A          Z", "AZ")]
    [DataRow("First      Middle      Last", "FL")]
    public void RefreshProfile_WhenUserFullNameHasMultipleConsecutiveSpaces_ExtractsInitialsCorrectly(string fullName, string expectedInitials)
    {
        // Arrange
        Mock<IAuthService> mockAuthService = new Mock<IAuthService>();
        SessionService sessionService = new SessionService();
        AuthUserDto user = new AuthUserDto
        {
            Id = "123",
            FullName = fullName,
            Email = "test@example.com",
            Role = "student"
        };
        sessionService.SetUser(user);
        ProfileViewModel viewModel = new ProfileViewModel(mockAuthService.Object, new Mock<IProfileService>().Object, sessionService, new Mock<ILogger<ProfileViewModel>>().Object);

        // Act
        viewModel.RefreshProfile();

        // Assert
        Assert.AreEqual(fullName, viewModel.FullName);
        Assert.AreEqual(expectedInitials, viewModel.Initials);
    }

    /// <summary>
    /// Tests that RefreshProfile handles email with Unicode characters correctly.
    /// Input: CurrentUser with Email containing Unicode characters.
    /// Expected: Email is preserved with all Unicode characters intact.
    /// </summary>
    [TestMethod]
    [DataRow("??@??.jp")]
    [DataRow("tëst@dömäin.com")]
    [DataRow("user@?????.ru")]
    public void RefreshProfile_WhenUserEmailHasUnicodeCharacters_PreservesEmail(string email)
    {
        // Arrange
        Mock<IAuthService> mockAuthService = new Mock<IAuthService>();
        SessionService sessionService = new SessionService();
        AuthUserDto user = new AuthUserDto
        {
            Id = "123",
            FullName = "John Doe",
            Email = email,
            Role = "student"
        };
        sessionService.SetUser(user);
        ProfileViewModel viewModel = new ProfileViewModel(mockAuthService.Object, new Mock<IProfileService>().Object, sessionService, new Mock<ILogger<ProfileViewModel>>().Object);

        // Act
        viewModel.RefreshProfile();

        // Assert
        Assert.AreEqual(email, viewModel.Email);
    }

    /// <summary>
    /// Tests that RefreshProfile handles names with lowercase letters correctly and converts initials to uppercase.
    /// Input: CurrentUser with FullName in various cases.
    /// Expected: FullName is preserved, but initials are always uppercase.
    /// </summary>
    [TestMethod]
    [DataRow("john doe", "JD")]
    [DataRow("JOHN DOE", "JD")]
    [DataRow("JoHn DoE", "JD")]
    [DataRow("jOhN dOe", "JD")]
    public void RefreshProfile_WhenUserFullNameHasVariousCases_ConvertsInitialsToUppercase(string fullName, string expectedInitials)
    {
        // Arrange
        Mock<IAuthService> mockAuthService = new Mock<IAuthService>();
        SessionService sessionService = new SessionService();
        AuthUserDto user = new AuthUserDto
        {
            Id = "123",
            FullName = fullName,
            Email = "test@example.com",
            Role = "student"
        };
        sessionService.SetUser(user);
        ProfileViewModel viewModel = new ProfileViewModel(mockAuthService.Object, new Mock<IProfileService>().Object, sessionService, new Mock<ILogger<ProfileViewModel>>().Object);

        // Act
        viewModel.RefreshProfile();

        // Assert
        Assert.AreEqual(fullName, viewModel.FullName);
        Assert.AreEqual(expectedInitials, viewModel.Initials);
    }
}



/// <summary>
/// Unit tests for the Role property of the ProfileViewModel class.
/// </summary>
[TestClass]
public partial class ProfileViewModelTests_Role
{
    /// <summary>
    /// Tests that the Role property returns an empty string as its default value.
    /// Input: Default initialization.
    /// Expected: Role property returns an empty string.
    /// </summary>
    [TestMethod]
    public void Role_DefaultValue_ReturnsEmptyString()
    {
        // Arrange
        var mockAuthService = new Mock<IAuthService>();
        var mockSession = new Mock<SessionService>();

        // Act
        var viewModel = new ProfileViewModel(mockAuthService.Object, new Mock<IProfileService>().Object, mockSession.Object, new Mock<ILogger<ProfileViewModel>>().Object);

        // Assert
        Assert.AreEqual(string.Empty, viewModel.Role);
    }

    /// <summary>
    /// Tests that the Role property correctly sets and returns various string values.
    /// Input: Various valid string values including empty, whitespace, special characters, and long strings.
    /// Expected: Role property returns the exact value that was set.
    /// </summary>
    /// <param name="value">The value to set on the Role property.</param>
    [TestMethod]
    [DataRow("Admin")]
    [DataRow("User")]
    [DataRow("Manager")]
    [DataRow("Student")]
    [DataRow("Lecturer")]
    [DataRow("ClassRepresentative")]
    [DataRow("")]
    [DataRow(" ")]
    [DataRow("   ")]
    [DataRow("\t")]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [DataRow(" \t\n ")]
    [DataRow("Role with spaces")]
    [DataRow("Role-with-dashes")]
    [DataRow("Role_with_underscores")]
    [DataRow("RoleWithSpecialChars!@#$%^&*()")]
    [DataRow("Role<>&\"'")]
    [DataRow("José")]
    [DataRow("Müller")]
    [DataRow("??")]
    [DataRow("VeryLongRoleNameThatExceedsNormalExpectationsAndContainsManyCharactersToTestBoundaryConditionsForStringHandlingAndMemoryAllocation")]
    public void Role_SetValue_ReturnsSetValue(string value)
    {
        // Arrange
        var mockAuthService = new Mock<IAuthService>();
        var mockSession = new Mock<SessionService>();
        var viewModel = new ProfileViewModel(mockAuthService.Object, new Mock<IProfileService>().Object, mockSession.Object, new Mock<ILogger<ProfileViewModel>>().Object);

        // Act
        viewModel.Role = value;

        // Assert
        Assert.AreEqual(value, viewModel.Role);
    }

    /// <summary>
    /// Tests that the Role property raises the PropertyChanged event when a new value is set.
    /// Input: A different value from the current value.
    /// Expected: PropertyChanged event is raised with property name "Role".
    /// </summary>
    [TestMethod]
    public void Role_SetNewValue_RaisesPropertyChangedEvent()
    {
        // Arrange
        var mockAuthService = new Mock<IAuthService>();
        var mockSession = new Mock<SessionService>();
        var viewModel = new ProfileViewModel(mockAuthService.Object, new Mock<IProfileService>().Object, mockSession.Object, new Mock<ILogger<ProfileViewModel>>().Object);
        var eventRaised = false;
        string? propertyName = null;

        viewModel.PropertyChanged += (sender, args) =>
        {
            eventRaised = true;
            propertyName = args.PropertyName;
        };

        // Act
        viewModel.Role = "Admin";

        // Assert
        Assert.IsTrue(eventRaised);
        Assert.AreEqual("Role", propertyName);
    }

    /// <summary>
    /// Tests that the Role property does not raise the PropertyChanged event when set to the same value.
    /// Input: The same value as the current value.
    /// Expected: PropertyChanged event is not raised.
    /// </summary>
    [TestMethod]
    public void Role_SetSameValue_DoesNotRaisePropertyChangedEvent()
    {
        // Arrange
        var mockAuthService = new Mock<IAuthService>();
        var mockSession = new Mock<SessionService>();
        var viewModel = new ProfileViewModel(mockAuthService.Object, new Mock<IProfileService>().Object, mockSession.Object, new Mock<ILogger<ProfileViewModel>>().Object);

        viewModel.Role = "Admin";

        var eventRaised = false;
        viewModel.PropertyChanged += (sender, args) =>
        {
            eventRaised = true;
        };

        // Act
        viewModel.Role = "Admin";

        // Assert
        Assert.IsFalse(eventRaised);
    }

    /// <summary>
    /// Tests that the Role property raises the PropertyChanged event multiple times when different values are set consecutively.
    /// Input: Multiple different string values.
    /// Expected: PropertyChanged event is raised for each distinct value change.
    /// </summary>
    [TestMethod]
    public void Role_SetDifferentValues_RaisesPropertyChangedEventMultipleTimes()
    {
        // Arrange
        var mockAuthService = new Mock<IAuthService>();
        var mockSession = new Mock<SessionService>();
        var viewModel = new ProfileViewModel(mockAuthService.Object, new Mock<IProfileService>().Object, mockSession.Object, new Mock<ILogger<ProfileViewModel>>().Object);
        var eventCount = 0;

        viewModel.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == "Role")
            {
                eventCount++;
            }
        };

        // Act
        viewModel.Role = "Admin";
        viewModel.Role = "User";
        viewModel.Role = "Manager";

        // Assert
        Assert.AreEqual(3, eventCount);
    }

    /// <summary>
    /// Tests that the Role property raises the PropertyChanged event when changing from empty string to a non-empty value.
    /// Input: Empty string initially, then set to non-empty string.
    /// Expected: PropertyChanged event is raised.
    /// </summary>
    [TestMethod]
    public void Role_SetFromEmptyToNonEmpty_RaisesPropertyChangedEvent()
    {
        // Arrange
        var mockAuthService = new Mock<IAuthService>();
        var mockSession = new Mock<SessionService>();
        var viewModel = new ProfileViewModel(mockAuthService.Object, new Mock<IProfileService>().Object, mockSession.Object, new Mock<ILogger<ProfileViewModel>>().Object);
        var eventRaised = false;

        Assert.AreEqual(string.Empty, viewModel.Role);

        viewModel.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == "Role")
            {
                eventRaised = true;
            }
        };

        // Act
        viewModel.Role = "Admin";

        // Assert
        Assert.IsTrue(eventRaised);
        Assert.AreEqual("Admin", viewModel.Role);
    }

    /// <summary>
    /// Tests that the Role property raises the PropertyChanged event when changing from a non-empty value to an empty string.
    /// Input: Non-empty string initially, then set to empty string.
    /// Expected: PropertyChanged event is raised.
    /// </summary>
    [TestMethod]
    public void Role_SetFromNonEmptyToEmpty_RaisesPropertyChangedEvent()
    {
        // Arrange
        var mockAuthService = new Mock<IAuthService>();
        var mockSession = new Mock<SessionService>();
        var viewModel = new ProfileViewModel(mockAuthService.Object, new Mock<IProfileService>().Object, mockSession.Object, new Mock<ILogger<ProfileViewModel>>().Object);

        viewModel.Role = "Admin";

        var eventRaised = false;
        viewModel.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == "Role")
            {
                eventRaised = true;
            }
        };

        // Act
        viewModel.Role = string.Empty;

        // Assert
        Assert.IsTrue(eventRaised);
        Assert.AreEqual(string.Empty, viewModel.Role);
    }

    /// <summary>
    /// Tests that the Role property correctly handles strings with control characters.
    /// Input: Strings containing control characters.
    /// Expected: Role property stores and returns the value as-is.
    /// </summary>
    /// <param name="value">The string with control characters to test.</param>
    [TestMethod]
    [DataRow("Role\u0000")]
    [DataRow("Role\u0001")]
    [DataRow("Role\u001F")]
    [DataRow("Role\u007F")]
    [DataRow("\u0000\u0001\u001F")]
    public void Role_SetStringWithControlCharacters_StoresValueAsIs(string value)
    {
        // Arrange
        var mockAuthService = new Mock<IAuthService>();
        var mockSession = new Mock<SessionService>();
        var viewModel = new ProfileViewModel(mockAuthService.Object, new Mock<IProfileService>().Object, mockSession.Object, new Mock<ILogger<ProfileViewModel>>().Object);

        // Act
        viewModel.Role = value;

        // Assert
        Assert.AreEqual(value, viewModel.Role);
    }

    /// <summary>
    /// Tests that the Role property correctly handles very long strings.
    /// Input: String with 10000 characters.
    /// Expected: Role property stores and returns the entire value.
    /// </summary>
    [TestMethod]
    public void Role_SetVeryLongString_StoresAndReturnsCompleteValue()
    {
        // Arrange
        var mockAuthService = new Mock<IAuthService>();
        var mockSession = new Mock<SessionService>();
        var viewModel = new ProfileViewModel(mockAuthService.Object, new Mock<IProfileService>().Object, mockSession.Object, new Mock<ILogger<ProfileViewModel>>().Object);
        var longString = new string('A', 10000);

        // Act
        viewModel.Role = longString;

        // Assert
        Assert.AreEqual(longString, viewModel.Role);
        Assert.AreEqual(10000, viewModel.Role.Length);
    }

    /// <summary>
    /// Tests that the Role property handles emoji and Unicode characters correctly.
    /// Input: Strings with emoji and various Unicode characters.
    /// Expected: Role property stores and returns the value correctly.
    /// </summary>
    /// <param name="value">The string with emoji/Unicode to test.</param>
    [TestMethod]
    [DataRow("Admin??")]
    [DataRow("??????")]
    [DataRow("Rôle")]
    [DataRow("????")]
    [DataRow("??")]
    [DataRow("??")]
    public void Role_SetStringWithEmojiAndUnicode_StoresValueCorrectly(string value)
    {
        // Arrange
        var mockAuthService = new Mock<IAuthService>();
        var mockSession = new Mock<SessionService>();
        var viewModel = new ProfileViewModel(mockAuthService.Object, new Mock<IProfileService>().Object, mockSession.Object, new Mock<ILogger<ProfileViewModel>>().Object);

        // Act
        viewModel.Role = value;

        // Assert
        Assert.AreEqual(value, viewModel.Role);
    }

    /// <summary>
    /// Tests that setting the same value after multiple different values does not raise PropertyChanged.
    /// Input: Set "Admin", then "User", then "Admin" again.
    /// Expected: PropertyChanged is raised only for the first two changes, not the third.
    /// </summary>
    [TestMethod]
    public void Role_SetSameValueAfterDifferentValue_DoesNotRaisePropertyChangedOnRepeat()
    {
        // Arrange
        var mockAuthService = new Mock<IAuthService>();
        var mockSession = new Mock<SessionService>();
        var viewModel = new ProfileViewModel(mockAuthService.Object, new Mock<IProfileService>().Object, mockSession.Object, new Mock<ILogger<ProfileViewModel>>().Object);
        var eventCount = 0;

        viewModel.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == "Role")
            {
                eventCount++;
            }
        };

        // Act
        viewModel.Role = "Admin";
        viewModel.Role = "User";
        viewModel.Role = "Admin";

        // Assert
        Assert.AreEqual(2, eventCount);
        Assert.AreEqual("Admin", viewModel.Role);
    }

    /// <summary>
    /// Tests that the PropertyChanged event args contain the correct property name.
    /// Input: Any valid value change.
    /// Expected: PropertyChanged event args have PropertyName equal to "Role".
    /// </summary>
    [TestMethod]
    public void Role_PropertyChangedEvent_ContainsCorrectPropertyName()
    {
        // Arrange
        var mockAuthService = new Mock<IAuthService>();
        var mockSession = new Mock<SessionService>();
        var viewModel = new ProfileViewModel(mockAuthService.Object, new Mock<IProfileService>().Object, mockSession.Object, new Mock<ILogger<ProfileViewModel>>().Object);
        string? capturedPropertyName = null;

        viewModel.PropertyChanged += (sender, args) =>
        {
            capturedPropertyName = args.PropertyName;
        };

        // Act
        viewModel.Role = "TestRole";

        // Assert
        Assert.AreEqual("Role", capturedPropertyName);
    }

    /// <summary>
    /// Tests that the Role property correctly handles mixed case strings.
    /// Input: Strings with mixed uppercase and lowercase characters.
    /// Expected: Role property preserves the exact casing.
    /// </summary>
    /// <param name="value">The mixed case string to test.</param>
    [TestMethod]
    [DataRow("aDmIn")]
    [DataRow("UsEr")]
    [DataRow("MaNaGeR")]
    [DataRow("sTuDeNt")]
    public void Role_SetMixedCaseString_PreservesExactCasing(string value)
    {
        // Arrange
        var mockAuthService = new Mock<IAuthService>();
        var mockSession = new Mock<SessionService>();
        var viewModel = new ProfileViewModel(mockAuthService.Object, new Mock<IProfileService>().Object, mockSession.Object, new Mock<ILogger<ProfileViewModel>>().Object);

        // Act
        viewModel.Role = value;

        // Assert
        Assert.AreEqual(value, viewModel.Role);
    }

    /// <summary>
    /// Tests that setting multiple values in rapid succession updates the property correctly each time.
    /// Input: Sequence of different values set consecutively.
    /// Expected: Each value is stored correctly and PropertyChanged is raised for each change.
    /// </summary>
    [TestMethod]
    public void Role_SetMultipleValuesInSequence_UpdatesCorrectlyEachTime()
    {
        // Arrange
        var mockAuthService = new Mock<IAuthService>();
        var mockSession = new Mock<SessionService>();
        var viewModel = new ProfileViewModel(mockAuthService.Object, new Mock<IProfileService>().Object, mockSession.Object, new Mock<ILogger<ProfileViewModel>>().Object);

        // Act & Assert
        viewModel.Role = "Admin";
        Assert.AreEqual("Admin", viewModel.Role);

        viewModel.Role = "User";
        Assert.AreEqual("User", viewModel.Role);

        viewModel.Role = "Manager";
        Assert.AreEqual("Manager", viewModel.Role);

        viewModel.Role = string.Empty;
        Assert.AreEqual(string.Empty, viewModel.Role);

        viewModel.Role = "Student";
        Assert.AreEqual("Student", viewModel.Role);
    }
}



/// <summary>
/// Unit tests for the Email property of ProfileViewModel class.
/// </summary>
[TestClass]
public partial class ProfileViewModelTests_Email
{
    /// <summary>
    /// Tests that the Email property returns empty string as initial value.
    /// Input: Default initialization.
    /// Expected: Email property returns empty string.
    /// </summary>
    [TestMethod]
    public void Email_InitialValue_ReturnsEmptyString()
    {
        // Arrange
        var mockAuthService = new Mock<IAuthService>();
        var mockSession = new Mock<SessionService>();

        // Act
        var viewModel = new ProfileViewModel(mockAuthService.Object, new Mock<IProfileService>().Object, mockSession.Object, new Mock<ILogger<ProfileViewModel>>().Object);

        // Assert
        Assert.AreEqual(string.Empty, viewModel.Email);
    }

    /// <summary>
    /// Tests that the Email property setter updates the value correctly and raises PropertyChanged event.
    /// Input: Valid email string values.
    /// Expected: Email property is updated and PropertyChanged event is raised with correct property name.
    /// </summary>
    [TestMethod]
    [DataRow("user@example.com")]
    [DataRow("test.user@domain.co.uk")]
    [DataRow("admin@company.org")]
    [DataRow("user+tag@example.com")]
    [DataRow("user_name@example.com")]
    [DataRow("user-name@example.com")]
    public void Email_SetValidValue_UpdatesPropertyAndRaisesPropertyChanged(string email)
    {
        // Arrange
        var mockAuthService = new Mock<IAuthService>();
        var mockSession = new Mock<SessionService>();
        var viewModel = new ProfileViewModel(mockAuthService.Object, new Mock<IProfileService>().Object, mockSession.Object, new Mock<ILogger<ProfileViewModel>>().Object);
        string? raisedPropertyName = null;
        viewModel.PropertyChanged += (sender, args) => raisedPropertyName = args.PropertyName;

        // Act
        viewModel.Email = email;

        // Assert
        Assert.AreEqual(email, viewModel.Email);
        Assert.AreEqual(nameof(viewModel.Email), raisedPropertyName);
    }

    /// <summary>
    /// Tests that the Email property setter handles empty string correctly.
    /// Input: Empty string.
    /// Expected: Email property is updated to empty string and PropertyChanged event is raised.
    /// </summary>
    [TestMethod]
    public void Email_SetEmptyString_UpdatesPropertyAndRaisesPropertyChanged()
    {
        // Arrange
        var mockAuthService = new Mock<IAuthService>();
        var mockSession = new Mock<SessionService>();
        var viewModel = new ProfileViewModel(mockAuthService.Object, new Mock<IProfileService>().Object, mockSession.Object, new Mock<ILogger<ProfileViewModel>>().Object);
        viewModel.Email = "initial@example.com";
        string? raisedPropertyName = null;
        viewModel.PropertyChanged += (sender, args) => raisedPropertyName = args.PropertyName;

        // Act
        viewModel.Email = string.Empty;

        // Assert
        Assert.AreEqual(string.Empty, viewModel.Email);
        Assert.AreEqual(nameof(viewModel.Email), raisedPropertyName);
    }

    /// <summary>
    /// Tests that the Email property setter handles whitespace strings correctly.
    /// Input: Whitespace-only strings (space, tab, newline, combinations).
    /// Expected: Email property is updated to the whitespace string and PropertyChanged event is raised.
    /// </summary>
    [TestMethod]
    [DataRow(" ")]
    [DataRow("   ")]
    [DataRow("\t")]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [DataRow(" \t\n ")]
    [DataRow("\t\t\t")]
    public void Email_SetWhitespaceString_UpdatesPropertyAndRaisesPropertyChanged(string whitespace)
    {
        // Arrange
        var mockAuthService = new Mock<IAuthService>();
        var mockSession = new Mock<SessionService>();
        var viewModel = new ProfileViewModel(mockAuthService.Object, new Mock<IProfileService>().Object, mockSession.Object, new Mock<ILogger<ProfileViewModel>>().Object);
        string? raisedPropertyName = null;
        viewModel.PropertyChanged += (sender, args) => raisedPropertyName = args.PropertyName;

        // Act
        viewModel.Email = whitespace;

        // Assert
        Assert.AreEqual(whitespace, viewModel.Email);
        Assert.AreEqual(nameof(viewModel.Email), raisedPropertyName);
    }

    /// <summary>
    /// Tests that the Email property setter handles strings with special characters correctly.
    /// Input: Strings with special characters including email-specific characters.
    /// Expected: Email property is updated and PropertyChanged event is raised.
    /// </summary>
    [TestMethod]
    [DataRow("user.name+tag@sub.example.com")]
    [DataRow("user@domain-with-dash.com")]
    [DataRow("user<>&\"'@example.com")]
    [DataRow("user!#$%&*@example.com")]
    [DataRow("user=?^_`{|}~@example.com")]
    public void Email_SetStringWithSpecialCharacters_UpdatesPropertyAndRaisesPropertyChanged(string email)
    {
        // Arrange
        var mockAuthService = new Mock<IAuthService>();
        var mockSession = new Mock<SessionService>();
        var viewModel = new ProfileViewModel(mockAuthService.Object, new Mock<IProfileService>().Object, mockSession.Object, new Mock<ILogger<ProfileViewModel>>().Object);
        string? raisedPropertyName = null;
        viewModel.PropertyChanged += (sender, args) => raisedPropertyName = args.PropertyName;

        // Act
        viewModel.Email = email;

        // Assert
        Assert.AreEqual(email, viewModel.Email);
        Assert.AreEqual(nameof(viewModel.Email), raisedPropertyName);
    }

    /// <summary>
    /// Tests that the Email property setter handles very long strings correctly.
    /// Input: Very long string (1000 characters).
    /// Expected: Email property is updated and PropertyChanged event is raised.
    /// </summary>
    [TestMethod]
    public void Email_SetVeryLongString_UpdatesPropertyAndRaisesPropertyChanged()
    {
        // Arrange
        var mockAuthService = new Mock<IAuthService>();
        var mockSession = new Mock<SessionService>();
        var viewModel = new ProfileViewModel(mockAuthService.Object, new Mock<IProfileService>().Object, mockSession.Object, new Mock<ILogger<ProfileViewModel>>().Object);
        var longEmail = new string('a', 1000) + "@example.com";
        string? raisedPropertyName = null;
        viewModel.PropertyChanged += (sender, args) => raisedPropertyName = args.PropertyName;

        // Act
        viewModel.Email = longEmail;

        // Assert
        Assert.AreEqual(longEmail, viewModel.Email);
        Assert.AreEqual(nameof(viewModel.Email), raisedPropertyName);
    }

    /// <summary>
    /// Tests that the Email property setter does not raise PropertyChanged event when the value is the same.
    /// Input: Same value as current.
    /// Expected: Email property remains unchanged and PropertyChanged event is not raised.
    /// </summary>
    [TestMethod]
    public void Email_SetSameValue_DoesNotRaisePropertyChanged()
    {
        // Arrange
        var mockAuthService = new Mock<IAuthService>();
        var mockSession = new Mock<SessionService>();
        var viewModel = new ProfileViewModel(mockAuthService.Object, new Mock<IProfileService>().Object, mockSession.Object, new Mock<ILogger<ProfileViewModel>>().Object);
        var email = "test@example.com";
        viewModel.Email = email;
        int eventRaisedCount = 0;
        viewModel.PropertyChanged += (sender, args) => eventRaisedCount++;

        // Act
        viewModel.Email = email;

        // Assert
        Assert.AreEqual(email, viewModel.Email);
        Assert.AreEqual(0, eventRaisedCount);
    }

    /// <summary>
    /// Tests that the Email property setter handles strings with control characters correctly.
    /// Input: Strings with control characters.
    /// Expected: Email property is updated and PropertyChanged event is raised.
    /// </summary>
    [TestMethod]
    [DataRow("user\u0000@example.com")]
    [DataRow("user\u0001@example.com")]
    [DataRow("user\u001F@example.com")]
    [DataRow("user\u007F@example.com")]
    public void Email_SetStringWithControlCharacters_UpdatesPropertyAndRaisesPropertyChanged(string email)
    {
        // Arrange
        var mockAuthService = new Mock<IAuthService>();
        var mockSession = new Mock<SessionService>();
        var viewModel = new ProfileViewModel(mockAuthService.Object, new Mock<IProfileService>().Object, mockSession.Object, new Mock<ILogger<ProfileViewModel>>().Object);
        string? raisedPropertyName = null;
        viewModel.PropertyChanged += (sender, args) => raisedPropertyName = args.PropertyName;

        // Act
        viewModel.Email = email;

        // Assert
        Assert.AreEqual(email, viewModel.Email);
        Assert.AreEqual(nameof(viewModel.Email), raisedPropertyName);
    }

    /// <summary>
    /// Tests that the Email property setter handles Unicode characters correctly.
    /// Input: Strings with Unicode characters (international email addresses).
    /// Expected: Email property is updated and PropertyChanged event is raised.
    /// </summary>
    [TestMethod]
    [DataRow("??@??.jp")]
    [DataRow("utilisateur@société.fr")]
    [DataRow("usuario@compañía.es")]
    [DataRow("benutzer@büro.de")]
    [DataRow("???st??@pa??de??µa.gr")]
    public void Email_SetStringWithUnicodeCharacters_UpdatesPropertyAndRaisesPropertyChanged(string email)
    {
        // Arrange
        var mockAuthService = new Mock<IAuthService>();
        var mockSession = new Mock<SessionService>();
        var viewModel = new ProfileViewModel(mockAuthService.Object, new Mock<IProfileService>().Object, mockSession.Object, new Mock<ILogger<ProfileViewModel>>().Object);
        string? raisedPropertyName = null;
        viewModel.PropertyChanged += (sender, args) => raisedPropertyName = args.PropertyName;

        // Act
        viewModel.Email = email;

        // Assert
        Assert.AreEqual(email, viewModel.Email);
        Assert.AreEqual(nameof(viewModel.Email), raisedPropertyName);
    }

    /// <summary>
    /// Tests that setting multiple different values to Email raises PropertyChanged event for each change.
    /// Input: Sequence of different email values.
    /// Expected: PropertyChanged event is raised for each distinct value change.
    /// </summary>
    [TestMethod]
    public void Email_SetMultipleDifferentValues_RaisesPropertyChangedForEachChange()
    {
        // Arrange
        var mockAuthService = new Mock<IAuthService>();
        var mockSession = new Mock<SessionService>();
        var viewModel = new ProfileViewModel(mockAuthService.Object, new Mock<IProfileService>().Object, mockSession.Object, new Mock<ILogger<ProfileViewModel>>().Object);
        int eventRaisedCount = 0;
        viewModel.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == nameof(viewModel.Email))
                eventRaisedCount++;
        };

        // Act
        viewModel.Email = "first@example.com";
        viewModel.Email = "second@example.com";
        viewModel.Email = "third@example.com";

        // Assert
        Assert.AreEqual("third@example.com", viewModel.Email);
        Assert.AreEqual(3, eventRaisedCount);
    }

    /// <summary>
    /// Tests that setting Email from empty to a value and back to empty raises PropertyChanged event both times.
    /// Input: Empty string -> valid email -> empty string.
    /// Expected: PropertyChanged event is raised twice (once for each change).
    /// </summary>
    [TestMethod]
    public void Email_SetFromEmptyToValueAndBackToEmpty_RaisesPropertyChangedTwice()
    {
        // Arrange
        var mockAuthService = new Mock<IAuthService>();
        var mockSession = new Mock<SessionService>();
        var viewModel = new ProfileViewModel(mockAuthService.Object, new Mock<IProfileService>().Object, mockSession.Object, new Mock<ILogger<ProfileViewModel>>().Object);
        int eventRaisedCount = 0;
        viewModel.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == nameof(viewModel.Email))
                eventRaisedCount++;
        };

        // Act
        viewModel.Email = "test@example.com";
        viewModel.Email = string.Empty;

        // Assert
        Assert.AreEqual(string.Empty, viewModel.Email);
        Assert.AreEqual(2, eventRaisedCount);
    }

    /// <summary>
    /// Tests that the PropertyChanged event provides the correct property name when Email changes.
    /// Input: Any valid string value.
    /// Expected: PropertyChanged event args contain property name "Email".
    /// </summary>
    [TestMethod]
    public void Email_SetValue_PropertyChangedEventContainsCorrectPropertyName()
    {
        // Arrange
        var mockAuthService = new Mock<IAuthService>();
        var mockSession = new Mock<SessionService>();
        var viewModel = new ProfileViewModel(mockAuthService.Object, new Mock<IProfileService>().Object, mockSession.Object, new Mock<ILogger<ProfileViewModel>>().Object);
        string? raisedPropertyName = null;
        viewModel.PropertyChanged += (sender, args) => raisedPropertyName = args.PropertyName;

        // Act
        viewModel.Email = "test@example.com";

        // Assert
        Assert.AreEqual("Email", raisedPropertyName);
    }

    /// <summary>
    /// Tests that setting Email with strings containing only control characters updates the property.
    /// Input: Strings with various control characters only.
    /// Expected: Email property is updated and PropertyChanged event is raised.
    /// </summary>
    [TestMethod]
    [DataRow("\u0000")]
    [DataRow("\u0001\u0002")]
    [DataRow("\u001F")]
    [DataRow("\u007F")]
    public void Email_SetStringWithControlCharactersOnly_UpdatesPropertyAndRaisesPropertyChanged(string value)
    {
        // Arrange
        var mockAuthService = new Mock<IAuthService>();
        var mockSession = new Mock<SessionService>();
        var viewModel = new ProfileViewModel(mockAuthService.Object, new Mock<IProfileService>().Object, mockSession.Object, new Mock<ILogger<ProfileViewModel>>().Object);
        string? raisedPropertyName = null;
        viewModel.PropertyChanged += (sender, args) => raisedPropertyName = args.PropertyName;

        // Act
        viewModel.Email = value;

        // Assert
        Assert.AreEqual(value, viewModel.Email);
        Assert.AreEqual(nameof(viewModel.Email), raisedPropertyName);
    }

    /// <summary>
    /// Tests that setting Email to the same value after multiple changes does not raise PropertyChanged.
    /// Input: "first@example.com" -> "second@example.com" -> "first@example.com" (same as first value).
    /// Expected: PropertyChanged is not raised for the third assignment.
    /// </summary>
    [TestMethod]
    public void Email_SetSameValueAfterDifferentValue_DoesNotRaisePropertyChangedOnSecondOccurrence()
    {
        // Arrange
        var mockAuthService = new Mock<IAuthService>();
        var mockSession = new Mock<SessionService>();
        var viewModel = new ProfileViewModel(mockAuthService.Object, new Mock<IProfileService>().Object, mockSession.Object, new Mock<ILogger<ProfileViewModel>>().Object);
        var firstEmail = "first@example.com";
        viewModel.Email = firstEmail;
        viewModel.Email = "second@example.com";
        int eventRaisedCount = 0;
        viewModel.PropertyChanged += (sender, args) => eventRaisedCount++;

        // Act
        viewModel.Email = firstEmail;

        // Assert
        Assert.AreEqual(firstEmail, viewModel.Email);
        Assert.AreEqual(0, eventRaisedCount);
    }

    /// <summary>
    /// Tests that setting Email handles strings with emojis and special Unicode symbols correctly.
    /// Input: Strings with emoji and special Unicode symbols.
    /// Expected: Email property is updated and PropertyChanged event is raised.
    /// </summary>
    [TestMethod]
    [DataRow("user??@example.com")]
    [DataRow("test????@domain.com")]
    [DataRow("user@emoji??.com")]
    public void Email_SetStringWithEmojis_UpdatesPropertyAndRaisesPropertyChanged(string email)
    {
        // Arrange
        var mockAuthService = new Mock<IAuthService>();
        var mockSession = new Mock<SessionService>();
        var viewModel = new ProfileViewModel(mockAuthService.Object, new Mock<IProfileService>().Object, mockSession.Object, new Mock<ILogger<ProfileViewModel>>().Object);
        string? raisedPropertyName = null;
        viewModel.PropertyChanged += (sender, args) => raisedPropertyName = args.PropertyName;

        // Act
        viewModel.Email = email;

        // Assert
        Assert.AreEqual(email, viewModel.Email);
        Assert.AreEqual(nameof(viewModel.Email), raisedPropertyName);
    }

    /// <summary>
    /// Tests that setting Email with leading and trailing whitespace preserves the whitespace.
    /// Input: Strings with leading and trailing whitespace.
    /// Expected: Email property stores the value as-is with whitespace preserved.
    /// </summary>
    [TestMethod]
    [DataRow("  user@example.com  ")]
    [DataRow("\tuser@example.com\t")]
    [DataRow("\nuser@example.com\n")]
    public void Email_SetStringWithLeadingTrailingWhitespace_PreservesWhitespace(string email)
    {
        // Arrange
        var mockAuthService = new Mock<IAuthService>();
        var mockSession = new Mock<SessionService>();
        var viewModel = new ProfileViewModel(mockAuthService.Object, new Mock<IProfileService>().Object, mockSession.Object, new Mock<ILogger<ProfileViewModel>>().Object);

        // Act
        viewModel.Email = email;

        // Assert
        Assert.AreEqual(email, viewModel.Email);
    }

    /// <summary>
    /// Tests that Email property handles extremely long email addresses (boundary test).
    /// Input: Email with 10000 characters before @ symbol.
    /// Expected: Email property is updated correctly without errors.
    /// </summary>
    [TestMethod]
    public void Email_SetExtremelyLongString_UpdatesPropertyCorrectly()
    {
        // Arrange
        var mockAuthService = new Mock<IAuthService>();
        var mockSession = new Mock<SessionService>();
        var viewModel = new ProfileViewModel(mockAuthService.Object, new Mock<IProfileService>().Object, mockSession.Object, new Mock<ILogger<ProfileViewModel>>().Object);
        var extremelyLongEmail = new string('x', 10000) + "@example.com";

        // Act
        viewModel.Email = extremelyLongEmail;

        // Assert
        Assert.AreEqual(extremelyLongEmail, viewModel.Email);
    }

    /// <summary>
    /// Tests that Email property handles mixed case correctly without modification.
    /// Input: Mixed case email strings.
    /// Expected: Email property preserves the exact case.
    /// </summary>
    [TestMethod]
    [DataRow("User@Example.Com")]
    [DataRow("TEST@EXAMPLE.COM")]
    [DataRow("TeSt@ExAmPlE.CoM")]
    public void Email_SetMixedCaseString_PreservesCase(string email)
    {
        // Arrange
        var mockAuthService = new Mock<IAuthService>();
        var mockSession = new Mock<SessionService>();
        var viewModel = new ProfileViewModel(mockAuthService.Object, new Mock<IProfileService>().Object, mockSession.Object, new Mock<ILogger<ProfileViewModel>>().Object);

        // Act
        viewModel.Email = email;

        // Assert
        Assert.AreEqual(email, viewModel.Email);
    }
}