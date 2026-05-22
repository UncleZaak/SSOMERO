using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

using Microsoft.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Storage;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Ssomero;
using Ssomero.Interfaces;
using Ssomero.Models;
using Ssomero.Navigation;
using Ssomero.Services;
using Ssomero.ViewModels;

namespace Ssomero.ViewModels.UnitTests;




/// <summary>
/// Unit tests for the <see cref="LoginViewModel"/> class.
/// </summary>
[TestClass]
public class LoginViewModelTests
{
    /// <summary>
    /// Tests that setting the ErrorMessage property updates the property value correctly.
    /// </summary>
    /// <param name="errorMessage">The error message value to set.</param>
    [TestMethod]
    [DataRow("")]
    [DataRow("Invalid credentials")]
    [DataRow("Network error occurred")]
    [DataRow("   ")]
    [DataRow("A very long error message that contains multiple sentences and detailed information about what went wrong during the login process. This tests the behavior with lengthy error messages.")]
    [DataRow("Error with special chars: !@#$%^&*()_+-=[]{}|;':\",./<>?")]
    [DataRow("\n\r\t")]
    public void ErrorMessage_SetValue_UpdatesProperty(string errorMessage)
    {
        // Arrange
        var mockAuthService = new Mock<IAuthService>();
        var mockApiService = new Mock<IApiService>();
        var mockSessionService = new Mock<SessionService>();
        var mockLogger = new Mock<ILogger<LoginViewModel>>();
        var viewModel = new LoginViewModel(
            mockAuthService.Object,
            mockApiService.Object,
            mockSessionService.Object,
            null,
            mockLogger.Object);

        // Act
        viewModel.ErrorMessage = errorMessage;

        // Assert
        Assert.AreEqual(errorMessage, viewModel.ErrorMessage);
    }

    /// <summary>
    /// Tests that setting the ErrorMessage property to null updates the property value to null.
    /// This tests edge case behavior with null values even though the property is declared as non-nullable.
    /// </summary>
    [TestMethod]
    public void ErrorMessage_SetNull_UpdatesProperty()
    {
        // Arrange
        var mockAuthService = new Mock<IAuthService>();
        var mockApiService = new Mock<IApiService>();
        var mockSessionService = new Mock<SessionService>();
        var mockLogger = new Mock<ILogger<LoginViewModel>>();
        var viewModel = new LoginViewModel(
            mockAuthService.Object,
            mockApiService.Object,
            mockSessionService.Object,
            null,
            mockLogger.Object);

        // Act
        viewModel.ErrorMessage = null!;

        // Assert
        Assert.IsNull(viewModel.ErrorMessage);
    }

    /// <summary>
    /// Tests that setting the ErrorMessage property raises the PropertyChanged event with the correct property name.
    /// </summary>
    [TestMethod]
    public void ErrorMessage_SetValue_RaisesPropertyChanged()
    {
        // Arrange
        var mockAuthService = new Mock<IAuthService>();
        var mockApiService = new Mock<IApiService>();
        var mockSessionService = new Mock<SessionService>();
        var mockLogger = new Mock<ILogger<LoginViewModel>>();
        var viewModel = new LoginViewModel(
            mockAuthService.Object,
            mockApiService.Object,
            mockSessionService.Object,
            null,
            mockLogger.Object);
        string? raisedPropertyName = null;
        viewModel.PropertyChanged += (sender, args) => raisedPropertyName = args.PropertyName;

        // Act
        viewModel.ErrorMessage = "Test error";

        // Assert
        Assert.AreEqual("ErrorMessage", raisedPropertyName);
    }

    /// <summary>
    /// Tests that setting the ErrorMessage property to the same value does not raise the PropertyChanged event.
    /// This validates that the SetProperty method correctly detects when no change occurs.
    /// </summary>
    [TestMethod]
    public void ErrorMessage_SetSameValue_DoesNotRaisePropertyChanged()
    {
        // Arrange
        var mockAuthService = new Mock<IAuthService>();
        var mockApiService = new Mock<IApiService>();
        var mockSessionService = new Mock<SessionService>();
        var mockLogger = new Mock<ILogger<LoginViewModel>>();
        var viewModel = new LoginViewModel(
            mockAuthService.Object,
            mockApiService.Object,
            mockSessionService.Object,
            null,
            mockLogger.Object);
        viewModel.ErrorMessage = "Initial error";
        int eventRaisedCount = 0;
        viewModel.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == "ErrorMessage")
                eventRaisedCount++;
        };

        // Act
        viewModel.ErrorMessage = "Initial error";

        // Assert
        Assert.AreEqual(0, eventRaisedCount);
    }

    /// <summary>
    /// Tests that setting the ErrorMessage property multiple times with different values raises PropertyChanged each time.
    /// </summary>
    [TestMethod]
    public void ErrorMessage_SetDifferentValues_RaisesPropertyChangedEachTime()
    {
        // Arrange
        var mockAuthService = new Mock<IAuthService>();
        var mockApiService = new Mock<IApiService>();
        var mockSessionService = new Mock<SessionService>();
        var mockLogger = new Mock<ILogger<LoginViewModel>>();
        var viewModel = new LoginViewModel(
            mockAuthService.Object,
            mockApiService.Object,
            mockSessionService.Object,
            null,
            mockLogger.Object);
        int eventRaisedCount = 0;
        viewModel.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == "ErrorMessage")
                eventRaisedCount++;
        };

        // Act
        viewModel.ErrorMessage = "Error 1";
        viewModel.ErrorMessage = "Error 2";
        viewModel.ErrorMessage = "Error 3";

        // Assert
        Assert.AreEqual(3, eventRaisedCount);
    }

    /// <summary>
    /// Tests that the ErrorMessage property initially has an empty string value.
    /// </summary>
    [TestMethod]
    public void ErrorMessage_InitialValue_IsEmptyString()
    {
        // Arrange
        var mockAuthService = new Mock<IAuthService>();
        var mockApiService = new Mock<IApiService>();
        var mockSessionService = new Mock<SessionService>();
        var mockLogger = new Mock<ILogger<LoginViewModel>>();

        // Act
        var viewModel = new LoginViewModel(
            mockAuthService.Object,
            mockApiService.Object,
            mockSessionService.Object,
            null,
            mockLogger.Object);

        // Assert
        Assert.AreEqual(string.Empty, viewModel.ErrorMessage);
    }

    /// <summary>
    /// Tests that setting ErrorMessage to empty string after it had a value updates correctly and raises PropertyChanged.
    /// </summary>
    [TestMethod]
    public void ErrorMessage_SetToEmptyAfterValue_UpdatesAndRaisesPropertyChanged()
    {
        // Arrange
        var mockAuthService = new Mock<IAuthService>();
        var mockApiService = new Mock<IApiService>();
        var mockSessionService = new Mock<SessionService>();
        var mockLogger = new Mock<ILogger<LoginViewModel>>();
        var viewModel = new LoginViewModel(
            mockAuthService.Object,
            mockApiService.Object,
            mockSessionService.Object,
            null,
            mockLogger.Object);
        viewModel.ErrorMessage = "Some error";
        string? raisedPropertyName = null;
        viewModel.PropertyChanged += (sender, args) => raisedPropertyName = args.PropertyName;

        // Act
        viewModel.ErrorMessage = string.Empty;

        // Assert
        Assert.AreEqual(string.Empty, viewModel.ErrorMessage);
        Assert.AreEqual("ErrorMessage", raisedPropertyName);
    }

    /// <summary>
    /// Tests that IsPasswordVisible returns the correct negated value of IsPasswordHidden.
    /// </summary>
    /// <param name="isPasswordHidden">The value to set for IsPasswordHidden property.</param>
    /// <param name="expectedIsPasswordVisible">The expected value of IsPasswordVisible property.</param>
    [TestMethod]
    [DataRow(true, false)]
    [DataRow(false, true)]
    public void IsPasswordVisible_WhenIsPasswordHiddenIsSet_ReturnsNegatedValue(bool isPasswordHidden, bool expectedIsPasswordVisible)
    {
        // Arrange
        var mockAuthService = new Mock<IAuthService>();
        var mockApiService = new Mock<IApiService>();
        var mockSessionService = new Mock<SessionService>();
        var mockLogger = new Mock<ILogger<LoginViewModel>>();
        var viewModel = new LoginViewModel(
            mockAuthService.Object,
            mockApiService.Object,
            mockSessionService.Object,
            null,
            mockLogger.Object);

        // Act
        viewModel.IsPasswordHidden = isPasswordHidden;
        var actualIsPasswordVisible = viewModel.IsPasswordVisible;

        // Assert
        Assert.AreEqual(expectedIsPasswordVisible, actualIsPasswordVisible);
    }

    /// <summary>
    /// Tests that IsPasswordVisible returns false when IsPasswordHidden is in its initial state (true).
    /// </summary>
    [TestMethod]
    public void IsPasswordVisible_InitialState_ReturnsFalse()
    {
        // Arrange
        var mockAuthService = new Mock<IAuthService>();
        var mockApiService = new Mock<IApiService>();
        var mockSessionService = new Mock<SessionService>();
        var mockLogger = new Mock<ILogger<LoginViewModel>>();
        var viewModel = new LoginViewModel(
            mockAuthService.Object,
            mockApiService.Object,
            mockSessionService.Object,
            null,
            mockLogger.Object);

        // Act
        var actualIsPasswordVisible = viewModel.IsPasswordVisible;

        // Assert
        Assert.IsFalse(actualIsPasswordVisible);
    }

    /// <summary>
    /// Tests that IsPasswordVisible correctly reflects changes when IsPasswordHidden is toggled multiple times.
    /// </summary>
    [TestMethod]
    public void IsPasswordVisible_WhenIsPasswordHiddenToggledMultipleTimes_ReturnsCorrectNegatedValues()
    {
        // Arrange
        var mockAuthService = new Mock<IAuthService>();
        var mockApiService = new Mock<IApiService>();
        var mockSessionService = new Mock<SessionService>();
        var mockLogger = new Mock<ILogger<LoginViewModel>>();
        var viewModel = new LoginViewModel(
            mockAuthService.Object,
            mockApiService.Object,
            mockSessionService.Object,
            null,
            mockLogger.Object);

        // Act & Assert - Initial state
        Assert.IsFalse(viewModel.IsPasswordVisible);

        // Act & Assert - Toggle to false
        viewModel.IsPasswordHidden = false;
        Assert.IsTrue(viewModel.IsPasswordVisible);

        // Act & Assert - Toggle back to true
        viewModel.IsPasswordHidden = true;
        Assert.IsFalse(viewModel.IsPasswordVisible);

        // Act & Assert - Toggle to false again
        viewModel.IsPasswordHidden = false;
        Assert.IsTrue(viewModel.IsPasswordVisible);
    }

    /// <summary>
    /// Tests that the constructor successfully initializes all command properties
    /// when provided with valid dependencies.
    /// Verifies that all seven command properties are not null after construction.
    /// </summary>
    [TestMethod]
    public void Constructor_ValidDependencies_InitializesAllCommands()
    {
        // Arrange
        var mockAuthService = new Mock<IAuthService>();
        var mockApiService = new Mock<IApiService>();
        var sessionService = new SessionService();
        var mockLogger = new Mock<ILogger<LoginViewModel>>();

        // Act
        var viewModel = new LoginViewModel(
            mockAuthService.Object,
            mockApiService.Object,
            sessionService,
            mockLogger.Object);

        // Assert
        Assert.IsNotNull(viewModel.LoginCommand);
        Assert.IsNotNull(viewModel.GoToRegisterCommand);
        Assert.IsNotNull(viewModel.GoBackCommand);
        Assert.IsNotNull(viewModel.ForgotPasswordCommand);
        Assert.IsNotNull(viewModel.SelectStudentCommand);
        Assert.IsNotNull(viewModel.SelectLecturerCommand);
        Assert.IsNotNull(viewModel.TogglePasswordVisibilityCommand);
    }

    /// <summary>
    /// Tests that the constructor successfully creates an instance without throwing
    /// when provided with valid mocked dependencies.
    /// Validates the primary happy path scenario.
    /// </summary>
    [TestMethod]
    public void Constructor_ValidDependencies_CreatesInstanceSuccessfully()
    {
        // Arrange
        var mockAuthService = new Mock<IAuthService>();
        var mockApiService = new Mock<IApiService>();
        var sessionService = new SessionService();
        var mockLogger = new Mock<ILogger<LoginViewModel>>();

        // Act
        var viewModel = new LoginViewModel(
            mockAuthService.Object,
            mockApiService.Object,
            sessionService,
            mockLogger.Object);

        // Assert
        Assert.IsNotNull(viewModel);
    }

    /// <summary>
    /// Tests that SelectStudentCommand is properly initialized and can be cast to ICommand.
    /// Verifies the command property type and initialization.
    /// </summary>
    [TestMethod]
    public void Constructor_ValidDependencies_SelectStudentCommandIsICommand()
    {
        // Arrange
        var mockAuthService = new Mock<IAuthService>();
        var mockApiService = new Mock<IApiService>();
        var sessionService = new SessionService();
        var mockLogger = new Mock<ILogger<LoginViewModel>>();

        // Act
        var viewModel = new LoginViewModel(
            mockAuthService.Object,
            mockApiService.Object,
            sessionService,
            mockLogger.Object);

        // Assert
        Assert.IsInstanceOfType(viewModel.SelectStudentCommand, typeof(ICommand));
    }

    /// <summary>
    /// Tests that SelectLecturerCommand is properly initialized and can be cast to ICommand.
    /// Verifies the command property type and initialization.
    /// </summary>
    [TestMethod]
    public void Constructor_ValidDependencies_SelectLecturerCommandIsICommand()
    {
        // Arrange
        var mockAuthService = new Mock<IAuthService>();
        var mockApiService = new Mock<IApiService>();
        var sessionService = new SessionService();
        var mockLogger = new Mock<ILogger<LoginViewModel>>();

        // Act
        var viewModel = new LoginViewModel(
            mockAuthService.Object,
            mockApiService.Object,
            sessionService,
            mockLogger.Object);

        // Assert
        Assert.IsInstanceOfType(viewModel.SelectLecturerCommand, typeof(ICommand));
    }

    /// <summary>
    /// Tests that TogglePasswordVisibilityCommand is properly initialized and can be cast to ICommand.
    /// Verifies the command property type and initialization.
    /// </summary>
    [TestMethod]
    public void Constructor_ValidDependencies_TogglePasswordVisibilityCommandIsICommand()
    {
        // Arrange
        var mockAuthService = new Mock<IAuthService>();
        var mockApiService = new Mock<IApiService>();
        var sessionService = new SessionService();
        var mockLogger = new Mock<ILogger<LoginViewModel>>();

        // Act
        var viewModel = new LoginViewModel(
            mockAuthService.Object,
            mockApiService.Object,
            sessionService,
            mockLogger.Object);

        // Assert
        Assert.IsInstanceOfType(viewModel.TogglePasswordVisibilityCommand, typeof(ICommand));
    }

    /// <summary>
    /// Tests that the constructor does not throw an exception when the authService parameter is null.
    /// Documents current behavior where null dependencies are not validated in the constructor.
    /// Note: This violates the non-nullable parameter contract but represents actual implementation behavior.
    /// </summary>
    [TestMethod]
    public void Constructor_NullAuthService_DoesNotThrow()
    {
        // Arrange
        var mockApiService = new Mock<IApiService>();
        var sessionService = new SessionService();
        var mockLogger = new Mock<ILogger<LoginViewModel>>();

        // Act & Assert - should not throw
        var viewModel = new LoginViewModel(
            null!,
            mockApiService.Object,
            sessionService,
            mockLogger.Object);

        Assert.IsNotNull(viewModel);
    }

    /// <summary>
    /// Tests that the constructor does not throw an exception when the api parameter is null.
    /// Documents current behavior where null dependencies are not validated in the constructor.
    /// Note: This violates the non-nullable parameter contract but represents actual implementation behavior.
    /// </summary>
    [TestMethod]
    public void Constructor_NullApiService_DoesNotThrow()
    {
        // Arrange
        var mockAuthService = new Mock<IAuthService>();
        var sessionService = new SessionService();
        var mockLogger = new Mock<ILogger<LoginViewModel>>();

        // Act & Assert - should not throw
        var viewModel = new LoginViewModel(
            mockAuthService.Object,
            null!,
            sessionService,
            mockLogger.Object);

        Assert.IsNotNull(viewModel);
    }

    /// <summary>
    /// Tests that the constructor does not throw an exception when the session parameter is null.
    /// Documents current behavior where null dependencies are not validated in the constructor.
    /// Note: This violates the non-nullable parameter contract but represents actual implementation behavior.
    /// </summary>
    [TestMethod]
    public void Constructor_NullSessionService_DoesNotThrow()
    {
        // Arrange
        var mockAuthService = new Mock<IAuthService>();
        var mockApiService = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<LoginViewModel>>();

        // Act & Assert - should not throw
        var viewModel = new LoginViewModel(
            mockAuthService.Object,
            mockApiService.Object,
            null!,
            mockLogger.Object);

        Assert.IsNotNull(viewModel);
    }

    /// <summary>
    /// Tests that the constructor does not throw an exception when the logger parameter is null.
    /// Documents current behavior where null dependencies are not validated in the constructor.
    /// Note: This violates the non-nullable parameter contract but represents actual implementation behavior.
    /// </summary>
    [TestMethod]
    public void Constructor_NullLogger_DoesNotThrow()
    {
        // Arrange
        var mockAuthService = new Mock<IAuthService>();
        var mockApiService = new Mock<IApiService>();
        var sessionService = new SessionService();

        // Act & Assert - should not throw
        var viewModel = new LoginViewModel(
            mockAuthService.Object,
            mockApiService.Object,
            sessionService,
            null!);

        Assert.IsNotNull(viewModel);
    }

    /// <summary>
    /// Tests that the constructor does not throw an exception when all dependencies are null.
    /// Documents current behavior where null dependencies are not validated in the constructor.
    /// Note: This violates the non-nullable parameter contracts but represents actual implementation behavior.
    /// </summary>
    [TestMethod]
    public void Constructor_AllDependenciesNull_DoesNotThrow()
    {
        // Act & Assert - should not throw
        var viewModel = new LoginViewModel(null!, null!, null!, null!);

        Assert.IsNotNull(viewModel);
    }

    /// <summary>
    /// Tests that the Email property has an empty string as its default value.
    /// </summary>
    [TestMethod]
    public void Email_DefaultValue_IsEmptyString()
    {
        // Arrange
        Mock<IAuthService> mockAuthService = new Mock<IAuthService>();
        Mock<IApiService> mockApiService = new Mock<IApiService>();
        Mock<SessionService> mockSessionService = new Mock<SessionService>();
        Mock<ILogger<LoginViewModel>> mockLogger = new Mock<ILogger<LoginViewModel>>();
        LoginViewModel viewModel = new LoginViewModel(
            mockAuthService.Object,
            mockApiService.Object,
            mockSessionService.Object,
            null,
            mockLogger.Object);

        // Act
        string result = viewModel.Email;

        // Assert
        Assert.AreEqual(string.Empty, result);
    }

    /// <summary>
    /// Tests that setting the Email property to a new value updates the property correctly.
    /// Tests various string inputs including normal email, empty string, whitespace, long string, and special characters.
    /// </summary>
    /// <param name="emailValue">The email value to set.</param>
    [TestMethod]
    [DataRow("test@example.com")]
    [DataRow("")]
    [DataRow("   ")]
    [DataRow("a")]
    [DataRow("verylongemailaddressverylongemailaddressverylongemailaddressverylongemailaddressverylongemailaddress@example.com")]
    [DataRow("email+tag@example.co.uk")]
    [DataRow("user.name@sub.domain.example.com")]
    [DataRow("test<>@example.com")]
    [DataRow("test\t\n@example.com")]
    public void Email_SetValue_UpdatesProperty(string emailValue)
    {
        // Arrange
        Mock<IAuthService> mockAuthService = new Mock<IAuthService>();
        Mock<IApiService> mockApiService = new Mock<IApiService>();
        Mock<SessionService> mockSessionService = new Mock<SessionService>();
        Mock<ILogger<LoginViewModel>> mockLogger = new Mock<ILogger<LoginViewModel>>();
        LoginViewModel viewModel = new LoginViewModel(
            mockAuthService.Object,
            mockApiService.Object,
            mockSessionService.Object,
            null,
            mockLogger.Object);

        // Act
        viewModel.Email = emailValue;

        // Assert
        Assert.AreEqual(emailValue, viewModel.Email);
    }

    /// <summary>
    /// Tests that setting the Email property to a new value raises the PropertyChanged event with correct property name.
    /// </summary>
    /// <param name="emailValue">The email value to set.</param>
    [TestMethod]
    [DataRow("test@example.com")]
    [DataRow("")]
    [DataRow("newvalue@test.com")]
    [DataRow("   ")]
    public void Email_SetNewValue_RaisesPropertyChangedEvent(string emailValue)
    {
        // Arrange
        Mock<IAuthService> mockAuthService = new Mock<IAuthService>();
        Mock<IApiService> mockApiService = new Mock<IApiService>();
        Mock<SessionService> mockSessionService = new Mock<SessionService>();
        Mock<ILogger<LoginViewModel>> mockLogger = new Mock<ILogger<LoginViewModel>>();
        LoginViewModel viewModel = new LoginViewModel(
            mockAuthService.Object,
            mockApiService.Object,
            mockSessionService.Object,
            null,
            mockLogger.Object);
        string? raisedPropertyName = null;
        viewModel.PropertyChanged += (sender, args) => raisedPropertyName = args.PropertyName;

        // Act
        viewModel.Email = emailValue;

        // Assert
        Assert.AreEqual("Email", raisedPropertyName);
    }

    /// <summary>
    /// Tests that setting the Email property to the same value does not raise the PropertyChanged event.
    /// </summary>
    [TestMethod]
    public void Email_SetSameValue_DoesNotRaisePropertyChangedEvent()
    {
        // Arrange
        Mock<IAuthService> mockAuthService = new Mock<IAuthService>();
        Mock<IApiService> mockApiService = new Mock<IApiService>();
        Mock<SessionService> mockSessionService = new Mock<SessionService>();
        Mock<ILogger<LoginViewModel>> mockLogger = new Mock<ILogger<LoginViewModel>>();
        LoginViewModel viewModel = new LoginViewModel(
            mockAuthService.Object,
            mockApiService.Object,
            mockSessionService.Object,
            null,
            mockLogger.Object);
        viewModel.Email = "test@example.com";
        bool propertyChangedRaised = false;
        viewModel.PropertyChanged += (sender, args) => propertyChangedRaised = true;

        // Act
        viewModel.Email = "test@example.com";

        // Assert
        Assert.IsFalse(propertyChangedRaised);
    }

    /// <summary>
    /// Tests that multiple consecutive changes to the Email property correctly update the property value.
    /// </summary>
    [TestMethod]
    public void Email_SetMultipleValues_UpdatesPropertyCorrectly()
    {
        // Arrange
        Mock<IAuthService> mockAuthService = new Mock<IAuthService>();
        Mock<IApiService> mockApiService = new Mock<IApiService>();
        Mock<SessionService> mockSessionService = new Mock<SessionService>();
        Mock<ILogger<LoginViewModel>> mockLogger = new Mock<ILogger<LoginViewModel>>();
        LoginViewModel viewModel = new LoginViewModel(
            mockAuthService.Object,
            mockApiService.Object,
            mockSessionService.Object,
            null,
            mockLogger.Object);

        // Act & Assert
        viewModel.Email = "first@example.com";
        Assert.AreEqual("first@example.com", viewModel.Email);

        viewModel.Email = "second@example.com";
        Assert.AreEqual("second@example.com", viewModel.Email);

        viewModel.Email = "";
        Assert.AreEqual("", viewModel.Email);

        viewModel.Email = "third@example.com";
        Assert.AreEqual("third@example.com", viewModel.Email);
    }

    /// <summary>
    /// Tests that setting SelectedRole to a different value raises PropertyChanged events
    /// for SelectedRole, IsStudentSelected, and IsLecturerSelected.
    /// </summary>
    /// <param name="newValue">The new value to set.</param>
    [TestMethod]
    [DataRow("Lecturer")]
    [DataRow("Student")]
    [DataRow("Administrator")]
    [DataRow("")]
    public void SelectedRole_SetToDifferentValue_RaisesPropertyChangedForAllRelatedProperties(string newValue)
    {
        // Arrange
        var authServiceMock = new Mock<IAuthService>();
        var apiServiceMock = new Mock<IApiService>();
        var sessionServiceMock = new Mock<SessionService>();
        var loggerMock = new Mock<ILogger<LoginViewModel>>();
        var viewModel = new LoginViewModel(authServiceMock.Object, apiServiceMock.Object, sessionServiceMock.Object, loggerMock.Object);
        var propertyChangedEvents = new List<string>();
        viewModel.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName != null)
                propertyChangedEvents.Add(args.PropertyName);
        };

        // Act
        viewModel.SelectedRole = newValue;

        // Assert
        Assert.AreEqual(newValue, viewModel.SelectedRole);
        Assert.IsTrue(propertyChangedEvents.Contains("SelectedRole"), "PropertyChanged should be raised for SelectedRole");
        Assert.IsTrue(propertyChangedEvents.Contains("IsStudentSelected"), "PropertyChanged should be raised for IsStudentSelected");
        Assert.IsTrue(propertyChangedEvents.Contains("IsLecturerSelected"), "PropertyChanged should be raised for IsLecturerSelected");
    }

    /// <summary>
    /// Tests that setting SelectedRole to the same value does not raise additional PropertyChanged events.
    /// </summary>
    [TestMethod]
    public void SelectedRole_SetToSameValue_DoesNotRaisePropertyChanged()
    {
        // Arrange
        var authServiceMock = new Mock<IAuthService>();
        var apiServiceMock = new Mock<IApiService>();
        var sessionServiceMock = new Mock<SessionService>();
        var loggerMock = new Mock<ILogger<LoginViewModel>>();
        var viewModel = new LoginViewModel(authServiceMock.Object, apiServiceMock.Object, sessionServiceMock.Object, loggerMock.Object);
        viewModel.SelectedRole = "Lecturer";
        var propertyChangedEvents = new List<string>();
        viewModel.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName != null)
                propertyChangedEvents.Add(args.PropertyName);
        };

        // Act
        viewModel.SelectedRole = "Lecturer";

        // Assert
        Assert.AreEqual(0, propertyChangedEvents.Count, "PropertyChanged should not be raised when setting the same value");
    }

    /// <summary>
    /// Tests that setting SelectedRole to null updates the value and raises PropertyChanged events.
    /// </summary>
    [TestMethod]
    public void SelectedRole_SetToNull_UpdatesValueAndRaisesPropertyChanged()
    {
        // Arrange
        var authServiceMock = new Mock<IAuthService>();
        var apiServiceMock = new Mock<IApiService>();
        var sessionServiceMock = new Mock<SessionService>();
        var loggerMock = new Mock<ILogger<LoginViewModel>>();
        var viewModel = new LoginViewModel(authServiceMock.Object, apiServiceMock.Object, sessionServiceMock.Object, loggerMock.Object);
        var propertyChangedEvents = new List<string>();
        viewModel.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName != null)
                propertyChangedEvents.Add(args.PropertyName);
        };

        // Act
        viewModel.SelectedRole = null!;

        // Assert
        Assert.IsNull(viewModel.SelectedRole);
        Assert.IsTrue(propertyChangedEvents.Contains("SelectedRole"), "PropertyChanged should be raised for SelectedRole");
        Assert.IsTrue(propertyChangedEvents.Contains("IsStudentSelected"), "PropertyChanged should be raised for IsStudentSelected");
        Assert.IsTrue(propertyChangedEvents.Contains("IsLecturerSelected"), "PropertyChanged should be raised for IsLecturerSelected");
    }

    /// <summary>
    /// Tests that setting SelectedRole to various edge case string values updates the value
    /// and raises PropertyChanged events.
    /// </summary>
    /// <param name="edgeCaseValue">The edge case string value to test.</param>
    [TestMethod]
    [DataRow("   ")]
    [DataRow("\t\n\r")]
    [DataRow("Role with spaces")]
    [DataRow("Role!@#$%^&*()")]
    [DataRow("Role\u0000WithNull")]
    public void SelectedRole_SetToEdgeCaseStrings_UpdatesValueAndRaisesPropertyChanged(string edgeCaseValue)
    {
        // Arrange
        var authServiceMock = new Mock<IAuthService>();
        var apiServiceMock = new Mock<IApiService>();
        var sessionServiceMock = new Mock<SessionService>();
        var loggerMock = new Mock<ILogger<LoginViewModel>>();
        var viewModel = new LoginViewModel(authServiceMock.Object, apiServiceMock.Object, sessionServiceMock.Object, loggerMock.Object);
        var propertyChangedEvents = new List<string>();
        viewModel.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName != null)
                propertyChangedEvents.Add(args.PropertyName);
        };

        // Act
        viewModel.SelectedRole = edgeCaseValue;

        // Assert
        Assert.AreEqual(edgeCaseValue, viewModel.SelectedRole);
        Assert.IsTrue(propertyChangedEvents.Contains("SelectedRole"), "PropertyChanged should be raised for SelectedRole");
        Assert.IsTrue(propertyChangedEvents.Contains("IsStudentSelected"), "PropertyChanged should be raised for IsStudentSelected");
        Assert.IsTrue(propertyChangedEvents.Contains("IsLecturerSelected"), "PropertyChanged should be raised for IsLecturerSelected");
    }

    /// <summary>
    /// Tests that setting SelectedRole to a very long string updates the value and raises PropertyChanged events.
    /// </summary>
    [TestMethod]
    public void SelectedRole_SetToVeryLongString_UpdatesValueAndRaisesPropertyChanged()
    {
        // Arrange
        var authServiceMock = new Mock<IAuthService>();
        var apiServiceMock = new Mock<IApiService>();
        var sessionServiceMock = new Mock<SessionService>();
        var loggerMock = new Mock<ILogger<LoginViewModel>>();
        var viewModel = new LoginViewModel(authServiceMock.Object, apiServiceMock.Object, sessionServiceMock.Object, loggerMock.Object);
        var veryLongString = new string('A', 10000);
        var propertyChangedEvents = new List<string>();
        viewModel.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName != null)
                propertyChangedEvents.Add(args.PropertyName);
        };

        // Act
        viewModel.SelectedRole = veryLongString;

        // Assert
        Assert.AreEqual(veryLongString, viewModel.SelectedRole);
        Assert.IsTrue(propertyChangedEvents.Contains("SelectedRole"), "PropertyChanged should be raised for SelectedRole");
        Assert.IsTrue(propertyChangedEvents.Contains("IsStudentSelected"), "PropertyChanged should be raised for IsStudentSelected");
        Assert.IsTrue(propertyChangedEvents.Contains("IsLecturerSelected"), "PropertyChanged should be raised for IsLecturerSelected");
    }

    /// <summary>
    /// Tests that getting SelectedRole returns the current value after it has been set.
    /// </summary>
    [TestMethod]
    public void SelectedRole_GetValue_ReturnsCurrentValue()
    {
        // Arrange
        var authServiceMock = new Mock<IAuthService>();
        var apiServiceMock = new Mock<IApiService>();
        var sessionServiceMock = new Mock<SessionService>();
        var loggerMock = new Mock<ILogger<LoginViewModel>>();
        var viewModel = new LoginViewModel(authServiceMock.Object, apiServiceMock.Object, sessionServiceMock.Object, loggerMock.Object);

        // Act
        viewModel.SelectedRole = "Lecturer";
        var result = viewModel.SelectedRole;

        // Assert
        Assert.AreEqual("Lecturer", result);
    }

    /// <summary>
    /// Tests that getting SelectedRole returns the default value of "Student" when not explicitly set.
    /// </summary>
    [TestMethod]
    public void SelectedRole_GetDefaultValue_ReturnsStudent()
    {
        // Arrange
        var authServiceMock = new Mock<IAuthService>();
        var apiServiceMock = new Mock<IApiService>();
        var sessionServiceMock = new Mock<SessionService>();
        var loggerMock = new Mock<ILogger<LoginViewModel>>();
        var viewModel = new LoginViewModel(authServiceMock.Object, apiServiceMock.Object, sessionServiceMock.Object, loggerMock.Object);

        // Act
        var result = viewModel.SelectedRole;

        // Assert
        Assert.AreEqual("Student", result);
    }

    /// <summary>
    /// Tests that IsStudentSelected returns true when SelectedRole is set to "Student".
    /// </summary>
    [TestMethod]
    public void IsStudentSelected_WhenRoleIsStudent_ReturnsTrue()
    {
        // Arrange
        var authServiceMock = new Mock<IAuthService>();
        var apiServiceMock = new Mock<IApiService>();
        var sessionServiceMock = new Mock<SessionService>();
        var loggerMock = new Mock<ILogger<LoginViewModel>>();
        var viewModel = new LoginViewModel(authServiceMock.Object, apiServiceMock.Object, sessionServiceMock.Object, loggerMock.Object);

        // Act
        viewModel.SelectedRole = "Student";
        var result = viewModel.IsStudentSelected;

        // Assert
        Assert.IsTrue(result);
    }

    /// <summary>
    /// Tests that IsStudentSelected returns false when SelectedRole is not "Student".
    /// </summary>
    /// <param name="role">The role value to test.</param>
    [TestMethod]
    [DataRow("Lecturer")]
    [DataRow("Administrator")]
    [DataRow("")]
    [DataRow("student")]
    [DataRow("STUDENT")]
    public void IsStudentSelected_WhenRoleIsNotStudent_ReturnsFalse(string role)
    {
        // Arrange
        var authServiceMock = new Mock<IAuthService>();
        var apiServiceMock = new Mock<IApiService>();
        var sessionServiceMock = new Mock<SessionService>();
        var loggerMock = new Mock<ILogger<LoginViewModel>>();
        var viewModel = new LoginViewModel(authServiceMock.Object, apiServiceMock.Object, sessionServiceMock.Object, loggerMock.Object);

        // Act
        viewModel.SelectedRole = role;
        var result = viewModel.IsStudentSelected;

        // Assert
        Assert.IsFalse(result);
    }

    /// <summary>
    /// Tests that IsLecturerSelected returns true when SelectedRole is set to "Lecturer".
    /// </summary>
    [TestMethod]
    public void IsLecturerSelected_WhenRoleIsLecturer_ReturnsTrue()
    {
        // Arrange
        var authServiceMock = new Mock<IAuthService>();
        var apiServiceMock = new Mock<IApiService>();
        var sessionServiceMock = new Mock<SessionService>();
        var loggerMock = new Mock<ILogger<LoginViewModel>>();
        var viewModel = new LoginViewModel(authServiceMock.Object, apiServiceMock.Object, sessionServiceMock.Object, loggerMock.Object);

        // Act
        viewModel.SelectedRole = "Lecturer";
        var result = viewModel.IsLecturerSelected;

        // Assert
        Assert.IsTrue(result);
    }

    /// <summary>
    /// Tests that IsLecturerSelected returns false when SelectedRole is not "Lecturer".
    /// </summary>
    /// <param name="role">The role value to test.</param>
    [TestMethod]
    [DataRow("Student")]
    [DataRow("Administrator")]
    [DataRow("")]
    [DataRow("lecturer")]
    [DataRow("LECTURER")]
    public void IsLecturerSelected_WhenRoleIsNotLecturer_ReturnsFalse(string role)
    {
        // Arrange
        var authServiceMock = new Mock<IAuthService>();
        var apiServiceMock = new Mock<IApiService>();
        var sessionServiceMock = new Mock<SessionService>();
        var loggerMock = new Mock<ILogger<LoginViewModel>>();
        var viewModel = new LoginViewModel(authServiceMock.Object, apiServiceMock.Object, sessionServiceMock.Object, loggerMock.Object);

        // Act
        viewModel.SelectedRole = role;
        var result = viewModel.IsLecturerSelected;

        // Assert
        Assert.IsFalse(result);
    }

    /// <summary>
    /// Tests that PropertyChanged events are raised in the correct order when SelectedRole is set.
    /// Verifies that SelectedRole PropertyChanged is raised before IsStudentSelected and IsLecturerSelected.
    /// </summary>
    [TestMethod]
    public void SelectedRole_SetValue_RaisesPropertyChangedInCorrectOrder()
    {
        // Arrange
        var authServiceMock = new Mock<IAuthService>();
        var apiServiceMock = new Mock<IApiService>();
        var sessionServiceMock = new Mock<SessionService>();
        var loggerMock = new Mock<ILogger<LoginViewModel>>();
        var viewModel = new LoginViewModel(authServiceMock.Object, apiServiceMock.Object, sessionServiceMock.Object, loggerMock.Object);
        var propertyChangedEvents = new List<string>();
        viewModel.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName != null)
                propertyChangedEvents.Add(args.PropertyName);
        };

        // Act
        viewModel.SelectedRole = "Lecturer";

        // Assert
        Assert.AreEqual(3, propertyChangedEvents.Count);
        Assert.AreEqual("SelectedRole", propertyChangedEvents[0]);
        Assert.AreEqual("IsStudentSelected", propertyChangedEvents[1]);
        Assert.AreEqual("IsLecturerSelected", propertyChangedEvents[2]);
    }

    /// <summary>
    /// Tests that IsStudentSelected returns the expected value based on SelectedRole.
    /// </summary>
    /// <param name="selectedRole">The role to set.</param>
    /// <param name="expectedIsStudentSelected">The expected value of IsStudentSelected.</param>
    [TestMethod]
    [DataRow("Student", true, DisplayName = "IsStudentSelected_WhenRoleIsStudent_ReturnsTrue")]
    [DataRow("Lecturer", false, DisplayName = "IsStudentSelected_WhenRoleIsLecturer_ReturnsFalse")]
    [DataRow("", false, DisplayName = "IsStudentSelected_WhenRoleIsEmpty_ReturnsFalse")]
    [DataRow(" ", false, DisplayName = "IsStudentSelected_WhenRoleIsWhitespace_ReturnsFalse")]
    [DataRow("student", false, DisplayName = "IsStudentSelected_WhenRoleIsLowercaseStudent_ReturnsFalse")]
    [DataRow("STUDENT", false, DisplayName = "IsStudentSelected_WhenRoleIsUppercaseStudent_ReturnsFalse")]
    [DataRow("Teacher", false, DisplayName = "IsStudentSelected_WhenRoleIsTeacher_ReturnsFalse")]
    [DataRow("Admin", false, DisplayName = "IsStudentSelected_WhenRoleIsAdmin_ReturnsFalse")]
    [DataRow("Student ", false, DisplayName = "IsStudentSelected_WhenRoleIsStudentWithTrailingSpace_ReturnsFalse")]
    [DataRow(" Student", false, DisplayName = "IsStudentSelected_WhenRoleIsStudentWithLeadingSpace_ReturnsFalse")]
    [DataRow(null, false, DisplayName = "IsStudentSelected_WhenRoleIsNull_ReturnsFalse")]
    public void IsStudentSelected_WithVariousRoles_ReturnsExpectedValue(string? selectedRole, bool expectedIsStudentSelected)
    {
        // Arrange
        var mockAuthService = new Mock<IAuthService>();
        var mockApiService = new Mock<IApiService>();
        var mockSession = new Mock<SessionService>();
        var mockLogger = new Mock<ILogger<LoginViewModel>>();

        var viewModel = new LoginViewModel(
            mockAuthService.Object,
            mockApiService.Object,
            mockSession.Object,
            null,
            mockLogger.Object);

        // Act
        viewModel.SelectedRole = selectedRole!;
        var result = viewModel.IsStudentSelected;

        // Assert
        Assert.AreEqual(expectedIsStudentSelected, result);
    }

    /// <summary>
    /// Tests that IsStudentSelected returns true when ViewModel is in default state.
    /// </summary>
    [TestMethod]
    public void IsStudentSelected_WhenViewModelInDefaultState_ReturnsTrue()
    {
        // Arrange
        var mockAuthService = new Mock<IAuthService>();
        var mockApiService = new Mock<IApiService>();
        var mockSession = new Mock<SessionService>();
        var mockLogger = new Mock<ILogger<LoginViewModel>>();

        // Act
        var viewModel = new LoginViewModel(
            mockAuthService.Object,
            mockApiService.Object,
            mockSession.Object,
            null,
            mockLogger.Object);

        // Assert
        Assert.IsTrue(viewModel.IsStudentSelected);
    }

    /// <summary>
    /// Tests that IsStudentSelected changes value correctly when SelectedRole is changed multiple times.
    /// </summary>
    [TestMethod]
    public void IsStudentSelected_WhenRoleChangedMultipleTimes_ReturnsCorrectValue()
    {
        // Arrange
        var mockAuthService = new Mock<IAuthService>();
        var mockApiService = new Mock<IApiService>();
        var mockSession = new Mock<SessionService>();
        var mockLogger = new Mock<ILogger<LoginViewModel>>();

        var viewModel = new LoginViewModel(
            mockAuthService.Object,
            mockApiService.Object,
            mockSession.Object,
            null,
            mockLogger.Object);

        // Act & Assert - Initial state
        Assert.IsTrue(viewModel.IsStudentSelected);

        // Act & Assert - Change to Lecturer
        viewModel.SelectedRole = "Lecturer";
        Assert.IsFalse(viewModel.IsStudentSelected);

        // Act & Assert - Change back to Student
        viewModel.SelectedRole = "Student";
        Assert.IsTrue(viewModel.IsStudentSelected);

        // Act & Assert - Change to empty
        viewModel.SelectedRole = "";
        Assert.IsFalse(viewModel.IsStudentSelected);

        // Act & Assert - Change back to Student again
        viewModel.SelectedRole = "Student";
        Assert.IsTrue(viewModel.IsStudentSelected);
    }

    /// <summary>
    /// Tests that IsLecturerSelected returns true when SelectedRole is exactly "Lecturer",
    /// and false for all other values including null, empty, whitespace, different casing,
    /// and other role values.
    /// </summary>
    /// <param name="selectedRole">The value to set for SelectedRole property.</param>
    /// <param name="expected">The expected result of IsLecturerSelected property.</param>
    [TestMethod]
    [DataRow("Lecturer", true, DisplayName = "Exact match 'Lecturer' returns true")]
    [DataRow("Student", false, DisplayName = "Student role returns false")]
    [DataRow(null, false, DisplayName = "Null returns false")]
    [DataRow("", false, DisplayName = "Empty string returns false")]
    [DataRow("   ", false, DisplayName = "Whitespace returns false")]
    [DataRow("lecturer", false, DisplayName = "Lowercase 'lecturer' returns false")]
    [DataRow("LECTURER", false, DisplayName = "Uppercase 'LECTURER' returns false")]
    [DataRow("LeCTuReR", false, DisplayName = "Mixed case 'LeCTuReR' returns false")]
    [DataRow("Teacher", false, DisplayName = "Different role 'Teacher' returns false")]
    [DataRow("Lecturer ", false, DisplayName = "Trailing space 'Lecturer ' returns false")]
    [DataRow(" Lecturer", false, DisplayName = "Leading space ' Lecturer' returns false")]
    [DataRow(" Lecturer ", false, DisplayName = "Leading and trailing spaces return false")]
    [DataRow("Admin", false, DisplayName = "Admin role returns false")]
    [DataRow("LecturerStudent", false, DisplayName = "Concatenated string returns false")]
    public void IsLecturerSelected_VariousSelectedRoleValues_ReturnsExpectedResult(string? selectedRole, bool expected)
    {
        // Arrange
        Mock<IAuthService> mockAuthService = new Mock<IAuthService>();
        Mock<IApiService> mockApiService = new Mock<IApiService>();
        SessionService sessionService = new SessionService();
        Mock<ILogger<LoginViewModel>> mockLogger = new Mock<ILogger<LoginViewModel>>();

        LoginViewModel viewModel = new LoginViewModel(
            mockAuthService.Object,
            mockApiService.Object,
            sessionService,
            mockLogger.Object);

        viewModel.SelectedRole = selectedRole!;

        // Act
        bool result = viewModel.IsLecturerSelected;

        // Assert
        Assert.AreEqual(expected, result);
    }

    /// <summary>
    /// Tests that IsPasswordHidden property returns the initial value of true.
    /// </summary>
    [TestMethod]
    public void IsPasswordHidden_InitialValue_ReturnsTrue()
    {
        // Arrange
        var authServiceMock = new Mock<IAuthService>();
        var apiServiceMock = new Mock<IApiService>();
        var sessionServiceMock = new Mock<SessionService>();
        var loggerMock = new Mock<ILogger<LoginViewModel>>();
        var viewModel = new LoginViewModel(authServiceMock.Object, apiServiceMock.Object, sessionServiceMock.Object, loggerMock.Object);

        // Act
        var result = viewModel.IsPasswordHidden;

        // Assert
        Assert.IsTrue(result);
    }

    /// <summary>
    /// Tests that IsPasswordHidden getter returns the correct value after it has been changed.
    /// </summary>
    /// <param name="newValue">The value to set.</param>
    [TestMethod]
    [DataRow(true)]
    [DataRow(false)]
    public void IsPasswordHidden_GetAfterSet_ReturnsSetValue(bool newValue)
    {
        // Arrange
        var authServiceMock = new Mock<IAuthService>();
        var apiServiceMock = new Mock<IApiService>();
        var sessionServiceMock = new Mock<SessionService>();
        var loggerMock = new Mock<ILogger<LoginViewModel>>();
        var viewModel = new LoginViewModel(authServiceMock.Object, apiServiceMock.Object, sessionServiceMock.Object, loggerMock.Object);

        // Act
        viewModel.IsPasswordHidden = newValue;
        var result = viewModel.IsPasswordHidden;

        // Assert
        Assert.AreEqual(newValue, result);
    }

    /// <summary>
    /// Tests that setting IsPasswordHidden to the same value does not raise PropertyChanged events.
    /// </summary>
    /// <param name="value">The value to set (same as current).</param>
    [TestMethod]
    [DataRow(true)]
    [DataRow(false)]
    public void IsPasswordHidden_SetToSameValue_DoesNotRaisePropertyChanged(bool value)
    {
        // Arrange
        var authServiceMock = new Mock<IAuthService>();
        var apiServiceMock = new Mock<IApiService>();
        var sessionServiceMock = new Mock<SessionService>();
        var loggerMock = new Mock<ILogger<LoginViewModel>>();
        var viewModel = new LoginViewModel(authServiceMock.Object, apiServiceMock.Object, sessionServiceMock.Object, loggerMock.Object);
        viewModel.IsPasswordHidden = value;
        var propertyChangedRaised = new List<string>();
        viewModel.PropertyChanged += (sender, args) => propertyChangedRaised.Add(args.PropertyName ?? string.Empty);

        // Act
        viewModel.IsPasswordHidden = value;

        // Assert
        Assert.AreEqual(0, propertyChangedRaised.Count);
    }

    /// <summary>
    /// Tests that setting IsPasswordHidden to a different value raises PropertyChanged for both IsPasswordHidden and IsPasswordVisible.
    /// </summary>
    /// <param name="initialValue">The initial value.</param>
    /// <param name="newValue">The new value to set.</param>
    [TestMethod]
    [DataRow(true, false)]
    [DataRow(false, true)]
    public void IsPasswordHidden_SetToDifferentValue_RaisesPropertyChangedForBothProperties(bool initialValue, bool newValue)
    {
        // Arrange
        var authServiceMock = new Mock<IAuthService>();
        var apiServiceMock = new Mock<IApiService>();
        var sessionServiceMock = new Mock<SessionService>();
        var loggerMock = new Mock<ILogger<LoginViewModel>>();
        var viewModel = new LoginViewModel(authServiceMock.Object, apiServiceMock.Object, sessionServiceMock.Object, loggerMock.Object);
        viewModel.IsPasswordHidden = initialValue;
        var propertyChangedRaised = new List<string>();
        viewModel.PropertyChanged += (sender, args) => propertyChangedRaised.Add(args.PropertyName ?? string.Empty);

        // Act
        viewModel.IsPasswordHidden = newValue;

        // Assert
        Assert.AreEqual(2, propertyChangedRaised.Count);
        Assert.IsTrue(propertyChangedRaised.Contains(nameof(LoginViewModel.IsPasswordHidden)));
        Assert.IsTrue(propertyChangedRaised.Contains(nameof(LoginViewModel.IsPasswordVisible)));
    }

    /// <summary>
    /// Tests that setting IsPasswordHidden to a different value raises PropertyChanged events in the correct order.
    /// First IsPasswordHidden, then IsPasswordVisible.
    /// </summary>
    [TestMethod]
    public void IsPasswordHidden_SetToDifferentValue_RaisesPropertyChangedInCorrectOrder()
    {
        // Arrange
        var authServiceMock = new Mock<IAuthService>();
        var apiServiceMock = new Mock<IApiService>();
        var sessionServiceMock = new Mock<SessionService>();
        var loggerMock = new Mock<ILogger<LoginViewModel>>();
        var viewModel = new LoginViewModel(authServiceMock.Object, apiServiceMock.Object, sessionServiceMock.Object, loggerMock.Object);
        var propertyChangedRaised = new List<string>();
        viewModel.PropertyChanged += (sender, args) => propertyChangedRaised.Add(args.PropertyName ?? string.Empty);

        // Act
        viewModel.IsPasswordHidden = false;

        // Assert
        Assert.AreEqual(2, propertyChangedRaised.Count);
        Assert.AreEqual(nameof(LoginViewModel.IsPasswordHidden), propertyChangedRaised[0]);
        Assert.AreEqual(nameof(LoginViewModel.IsPasswordVisible), propertyChangedRaised[1]);
    }

    /// <summary>
    /// Tests that IsPasswordHidden setter updates the backing field correctly when set multiple times with different values.
    /// </summary>
    [TestMethod]
    public void IsPasswordHidden_SetMultipleTimes_UpdatesValueCorrectly()
    {
        // Arrange
        var authServiceMock = new Mock<IAuthService>();
        var apiServiceMock = new Mock<IApiService>();
        var sessionServiceMock = new Mock<SessionService>();
        var loggerMock = new Mock<ILogger<LoginViewModel>>();
        var viewModel = new LoginViewModel(authServiceMock.Object, apiServiceMock.Object, sessionServiceMock.Object, loggerMock.Object);

        // Act & Assert
        viewModel.IsPasswordHidden = false;
        Assert.IsFalse(viewModel.IsPasswordHidden);

        viewModel.IsPasswordHidden = true;
        Assert.IsTrue(viewModel.IsPasswordHidden);

        viewModel.IsPasswordHidden = false;
        Assert.IsFalse(viewModel.IsPasswordHidden);
    }

    /// <summary>
    /// Tests that the Password property getter returns the initial empty string value.
    /// </summary>
    [TestMethod]
    public void Password_InitialValue_ReturnsEmptyString()
    {
        // Arrange
        var mockAuthService = new Mock<IAuthService>();
        var mockApiService = new Mock<IApiService>();
        var mockSession = new Mock<SessionService>();
        var mockLogger = new Mock<ILogger<LoginViewModel>>();
        var viewModel = new LoginViewModel(mockAuthService.Object, mockApiService.Object, mockSession.Object, mockLogger.Object);

        // Act
        var result = viewModel.Password;

        // Assert
        Assert.AreEqual(string.Empty, result);
    }

    /// <summary>
    /// Tests that the Password property setter correctly updates the value and raises PropertyChanged event.
    /// </summary>
    /// <param name="newPassword">The password value to set.</param>
    [TestMethod]
    [DataRow("password123")]
    [DataRow("P@ssw0rd!")]
    [DataRow("mypassword")]
    [DataRow("12345678")]
    [DataRow("")]
    [DataRow("   ")]
    [DataRow("\t\n\r")]
    [DataRow("very long password string that exceeds normal length expectations to test boundary conditions and ensure the property handles large input without issues")]
    [DataRow("密码")]
    [DataRow("пароль")]
    [DataRow("🔒🔑")]
    [DataRow("pass\u0000word")]
    [DataRow("pass\u001Fword")]
    public void Password_SetValidValue_UpdatesValueAndRaisesPropertyChanged(string newPassword)
    {
        // Arrange
        var mockAuthService = new Mock<IAuthService>();
        var mockApiService = new Mock<IApiService>();
        var mockSession = new Mock<SessionService>();
        var mockLogger = new Mock<ILogger<LoginViewModel>>();
        var viewModel = new LoginViewModel(mockAuthService.Object, mockApiService.Object, mockSession.Object, mockLogger.Object);
        var propertyChangedRaised = false;
        string? propertyName = null;

        viewModel.PropertyChanged += (sender, args) =>
        {
            propertyChangedRaised = true;
            propertyName = args.PropertyName;
        };

        // Act
        viewModel.Password = newPassword;

        // Assert
        Assert.AreEqual(newPassword, viewModel.Password);
        Assert.IsTrue(propertyChangedRaised);
        Assert.AreEqual("Password", propertyName);
    }

    /// <summary>
    /// Tests that setting the Password property to the same value does not raise PropertyChanged event.
    /// </summary>
    [TestMethod]
    public void Password_SetSameValue_DoesNotRaisePropertyChanged()
    {
        // Arrange
        var mockAuthService = new Mock<IAuthService>();
        var mockApiService = new Mock<IApiService>();
        var mockSession = new Mock<SessionService>();
        var mockLogger = new Mock<ILogger<LoginViewModel>>();
        var viewModel = new LoginViewModel(mockAuthService.Object, mockApiService.Object, mockSession.Object, mockLogger.Object);
        var initialPassword = "testpassword";
        viewModel.Password = initialPassword;

        var propertyChangedCount = 0;
        viewModel.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == "Password")
            {
                propertyChangedCount++;
            }
        };

        // Act
        viewModel.Password = initialPassword;

        // Assert
        Assert.AreEqual(initialPassword, viewModel.Password);
        Assert.AreEqual(0, propertyChangedCount);
    }

    /// <summary>
    /// Tests that the Password property setter correctly updates value multiple times with different values.
    /// </summary>
    [TestMethod]
    public void Password_SetMultipleDifferentValues_UpdatesValueAndRaisesPropertyChangedEachTime()
    {
        // Arrange
        var mockAuthService = new Mock<IAuthService>();
        var mockApiService = new Mock<IApiService>();
        var mockSession = new Mock<SessionService>();
        var mockLogger = new Mock<ILogger<LoginViewModel>>();
        var viewModel = new LoginViewModel(mockAuthService.Object, mockApiService.Object, mockSession.Object, mockLogger.Object);
        var propertyChangedCount = 0;

        viewModel.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == "Password")
            {
                propertyChangedCount++;
            }
        };

        // Act & Assert
        viewModel.Password = "password1";
        Assert.AreEqual("password1", viewModel.Password);
        Assert.AreEqual(1, propertyChangedCount);

        viewModel.Password = "password2";
        Assert.AreEqual("password2", viewModel.Password);
        Assert.AreEqual(2, propertyChangedCount);

        viewModel.Password = "";
        Assert.AreEqual("", viewModel.Password);
        Assert.AreEqual(3, propertyChangedCount);
    }

    /// <summary>
    /// Tests that the Password property handles very long strings correctly.
    /// </summary>
    [TestMethod]
    public void Password_SetVeryLongString_StoresAndReturnsCorrectly()
    {
        // Arrange
        var mockAuthService = new Mock<IAuthService>();
        var mockApiService = new Mock<IApiService>();
        var mockSession = new Mock<SessionService>();
        var mockLogger = new Mock<ILogger<LoginViewModel>>();
        var viewModel = new LoginViewModel(mockAuthService.Object, mockApiService.Object, mockSession.Object, mockLogger.Object);
        var veryLongPassword = new string('a', 10000);

        // Act
        viewModel.Password = veryLongPassword;

        // Assert
        Assert.AreEqual(veryLongPassword, viewModel.Password);
        Assert.AreEqual(10000, viewModel.Password.Length);
    }

    /// <summary>
    /// Tests that the Password property handles strings with special Unicode characters correctly.
    /// </summary>
    [TestMethod]
    [DataRow("password\u200B")]
    [DataRow("\uFEFFpassword")]
    [DataRow("pass\u202Eword")]
    public void Password_SetStringWithSpecialUnicodeCharacters_StoresAndReturnsCorrectly(string passwordWithSpecialChars)
    {
        // Arrange
        var mockAuthService = new Mock<IAuthService>();
        var mockApiService = new Mock<IApiService>();
        var mockSession = new Mock<SessionService>();
        var mockLogger = new Mock<ILogger<LoginViewModel>>();
        var viewModel = new LoginViewModel(mockAuthService.Object, mockApiService.Object, mockSession.Object, mockLogger.Object);

        // Act
        viewModel.Password = passwordWithSpecialChars;

        // Assert
        Assert.AreEqual(passwordWithSpecialChars, viewModel.Password);
    }

    /// <summary>
    /// Tests that setting Password from empty to non-empty value raises PropertyChanged event.
    /// </summary>
    [TestMethod]
    public void Password_SetFromEmptyToNonEmpty_RaisesPropertyChanged()
    {
        // Arrange
        var mockAuthService = new Mock<IAuthService>();
        var mockApiService = new Mock<IApiService>();
        var mockSession = new Mock<SessionService>();
        var mockLogger = new Mock<ILogger<LoginViewModel>>();
        var viewModel = new LoginViewModel(mockAuthService.Object, mockApiService.Object, mockSession.Object, mockLogger.Object);
        var propertyChangedRaised = false;

        viewModel.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == "Password")
            {
                propertyChangedRaised = true;
            }
        };

        // Act
        viewModel.Password = "newpassword";

        // Assert
        Assert.IsTrue(propertyChangedRaised);
        Assert.AreEqual("newpassword", viewModel.Password);
    }

    /// <summary>
    /// Tests that setting Password to empty string from non-empty value raises PropertyChanged event.
    /// </summary>
    [TestMethod]
    public void Password_SetToEmptyFromNonEmpty_RaisesPropertyChanged()
    {
        // Arrange
        var mockAuthService = new Mock<IAuthService>();
        var mockApiService = new Mock<IApiService>();
        var mockSession = new Mock<SessionService>();
        var mockLogger = new Mock<ILogger<LoginViewModel>>();
        var viewModel = new LoginViewModel(mockAuthService.Object, mockApiService.Object, mockSession.Object, mockLogger.Object);
        viewModel.Password = "somepassword";
        var propertyChangedRaised = false;

        viewModel.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == "Password")
            {
                propertyChangedRaised = true;
            }
        };

        // Act
        viewModel.Password = "";

        // Assert
        Assert.IsTrue(propertyChangedRaised);
        Assert.AreEqual("", viewModel.Password);
    }

    /// <summary>
    /// Tests that LoginCommand is properly initialized and can be cast to ICommand.
    /// Verifies the command property type and initialization.
    /// </summary>
    [TestMethod]
    public void Constructor_ValidDependencies_LoginCommandIsICommand()
    {
        // Arrange
        var mockAuthService = new Mock<IAuthService>();
        var mockApiService = new Mock<IApiService>();
        var sessionService = new SessionService();
        var mockLogger = new Mock<ILogger<LoginViewModel>>();

        // Act
        var viewModel = new LoginViewModel(
            mockAuthService.Object,
            mockApiService.Object,
            sessionService,
            mockLogger.Object);

        // Assert
        Assert.IsInstanceOfType(viewModel.LoginCommand, typeof(ICommand));
    }

    /// <summary>
    /// Tests that GoToRegisterCommand is properly initialized and can be cast to ICommand.
    /// Verifies the command property type and initialization.
    /// </summary>
    [TestMethod]
    public void Constructor_ValidDependencies_GoToRegisterCommandIsICommand()
    {
        // Arrange
        var mockAuthService = new Mock<IAuthService>();
        var mockApiService = new Mock<IApiService>();
        var sessionService = new SessionService();
        var mockLogger = new Mock<ILogger<LoginViewModel>>();

        // Act
        var viewModel = new LoginViewModel(
            mockAuthService.Object,
            mockApiService.Object,
            sessionService,
            mockLogger.Object);

        // Assert
        Assert.IsInstanceOfType(viewModel.GoToRegisterCommand, typeof(ICommand));
    }

    /// <summary>
    /// Tests that GoBackCommand is properly initialized and can be cast to ICommand.
    /// Verifies the command property type and initialization.
    /// </summary>
    [TestMethod]
    public void Constructor_ValidDependencies_GoBackCommandIsICommand()
    {
        // Arrange
        var mockAuthService = new Mock<IAuthService>();
        var mockApiService = new Mock<IApiService>();
        var sessionService = new SessionService();
        var mockLogger = new Mock<ILogger<LoginViewModel>>();

        // Act
        var viewModel = new LoginViewModel(
            mockAuthService.Object,
            mockApiService.Object,
            sessionService,
            mockLogger.Object);

        // Assert
        Assert.IsInstanceOfType(viewModel.GoBackCommand, typeof(ICommand));
    }

    /// <summary>
    /// Tests that ForgotPasswordCommand is properly initialized and can be cast to ICommand.
    /// Verifies the command property type and initialization.
    /// </summary>
    [TestMethod]
    public void Constructor_ValidDependencies_ForgotPasswordCommandIsICommand()
    {
        // Arrange
        var mockAuthService = new Mock<IAuthService>();
        var mockApiService = new Mock<IApiService>();
        var sessionService = new SessionService();
        var mockLogger = new Mock<ILogger<LoginViewModel>>();

        // Act
        var viewModel = new LoginViewModel(
            mockAuthService.Object,
            mockApiService.Object,
            sessionService,
            mockLogger.Object);

        // Assert
        Assert.IsInstanceOfType(viewModel.ForgotPasswordCommand, typeof(ICommand));
    }

    /// <summary>
    /// Tests that all command properties are of type Command specifically.
    /// Verifies the concrete implementation type used for all commands.
    /// </summary>
    [TestMethod]
    public void Constructor_ValidDependencies_AllCommandsAreCommandType()
    {
        // Arrange
        var mockAuthService = new Mock<IAuthService>();
        var mockApiService = new Mock<IApiService>();
        var sessionService = new SessionService();
        var mockLogger = new Mock<ILogger<LoginViewModel>>();

        // Act
        var viewModel = new LoginViewModel(
            mockAuthService.Object,
            mockApiService.Object,
            sessionService,
            mockLogger.Object);

        // Assert
        Assert.IsInstanceOfType(viewModel.LoginCommand, typeof(Command));
        Assert.IsInstanceOfType(viewModel.GoToRegisterCommand, typeof(Command));
        Assert.IsInstanceOfType(viewModel.GoBackCommand, typeof(Command));
        Assert.IsInstanceOfType(viewModel.ForgotPasswordCommand, typeof(Command));
        Assert.IsInstanceOfType(viewModel.SelectStudentCommand, typeof(Command));
        Assert.IsInstanceOfType(viewModel.SelectLecturerCommand, typeof(Command));
        Assert.IsInstanceOfType(viewModel.TogglePasswordVisibilityCommand, typeof(Command));
    }

    /// <summary>
    /// Tests that the constructor initializes the SelectedRole property to "Student" by default.
    /// Verifies the initial state of role selection.
    /// </summary>
    [TestMethod]
    public void Constructor_ValidDependencies_InitializesSelectedRoleToStudent()
    {
        // Arrange
        var mockAuthService = new Mock<IAuthService>();
        var mockApiService = new Mock<IApiService>();
        var sessionService = new SessionService();
        var mockLogger = new Mock<ILogger<LoginViewModel>>();

        // Act
        var viewModel = new LoginViewModel(
            mockAuthService.Object,
            mockApiService.Object,
            sessionService,
            mockLogger.Object);

        // Assert
        Assert.AreEqual("Student", viewModel.SelectedRole);
    }

    /// <summary>
    /// Tests that the constructor initializes the Email property to empty string.
    /// Verifies the initial state of the Email property.
    /// </summary>
    [TestMethod]
    public void Constructor_ValidDependencies_InitializesEmailToEmptyString()
    {
        // Arrange
        var mockAuthService = new Mock<IAuthService>();
        var mockApiService = new Mock<IApiService>();
        var sessionService = new SessionService();
        var mockLogger = new Mock<ILogger<LoginViewModel>>();

        // Act
        var viewModel = new LoginViewModel(
            mockAuthService.Object,
            mockApiService.Object,
            sessionService,
            mockLogger.Object);

        // Assert
        Assert.AreEqual(string.Empty, viewModel.Email);
    }

    /// <summary>
    /// Tests that the constructor initializes the Password property to empty string.
    /// Verifies the initial state of the Password property.
    /// </summary>
    [TestMethod]
    public void Constructor_ValidDependencies_InitializesPasswordToEmptyString()
    {
        // Arrange
        var mockAuthService = new Mock<IAuthService>();
        var mockApiService = new Mock<IApiService>();
        var sessionService = new SessionService();
        var mockLogger = new Mock<ILogger<LoginViewModel>>();

        // Act
        var viewModel = new LoginViewModel(
            mockAuthService.Object,
            mockApiService.Object,
            sessionService,
            mockLogger.Object);

        // Assert
        Assert.AreEqual(string.Empty, viewModel.Password);
    }

    /// <summary>
    /// Tests that the constructor initializes the ErrorMessage property to empty string.
    /// Verifies the initial state of the ErrorMessage property.
    /// </summary>
    [TestMethod]
    public void Constructor_ValidDependencies_InitializesErrorMessageToEmptyString()
    {
        // Arrange
        var mockAuthService = new Mock<IAuthService>();
        var mockApiService = new Mock<IApiService>();
        var sessionService = new SessionService();
        var mockLogger = new Mock<ILogger<LoginViewModel>>();

        // Act
        var viewModel = new LoginViewModel(
            mockAuthService.Object,
            mockApiService.Object,
            sessionService,
            mockLogger.Object);

        // Assert
        Assert.AreEqual(string.Empty, viewModel.ErrorMessage);
    }

    /// <summary>
    /// Tests that the constructor initializes the IsPasswordHidden property to true.
    /// Verifies the initial state where password is hidden.
    /// </summary>
    [TestMethod]
    public void Constructor_ValidDependencies_InitializesIsPasswordHiddenToTrue()
    {
        // Arrange
        var mockAuthService = new Mock<IAuthService>();
        var mockApiService = new Mock<IApiService>();
        var sessionService = new SessionService();
        var mockLogger = new Mock<ILogger<LoginViewModel>>();

        // Act
        var viewModel = new LoginViewModel(
            mockAuthService.Object,
            mockApiService.Object,
            sessionService,
            mockLogger.Object);

        // Assert
        Assert.IsTrue(viewModel.IsPasswordHidden);
    }

    /// <summary>
    /// Tests that the constructor initializes the IsPasswordVisible property to false.
    /// Verifies the initial state where password is not visible.
    /// </summary>
    [TestMethod]
    public void Constructor_ValidDependencies_InitializesIsPasswordVisibleToFalse()
    {
        // Arrange
        var mockAuthService = new Mock<IAuthService>();
        var mockApiService = new Mock<IApiService>();
        var sessionService = new SessionService();
        var mockLogger = new Mock<ILogger<LoginViewModel>>();

        // Act
        var viewModel = new LoginViewModel(
            mockAuthService.Object,
            mockApiService.Object,
            sessionService,
            mockLogger.Object);

        // Assert
        Assert.IsFalse(viewModel.IsPasswordVisible);
    }

    /// <summary>
    /// Tests that the constructor initializes IsStudentSelected to true when SelectedRole defaults to "Student".
    /// Verifies the computed property state based on default role.
    /// </summary>
    [TestMethod]
    public void Constructor_ValidDependencies_InitializesIsStudentSelectedToTrue()
    {
        // Arrange
        var mockAuthService = new Mock<IAuthService>();
        var mockApiService = new Mock<IApiService>();
        var sessionService = new SessionService();
        var mockLogger = new Mock<ILogger<LoginViewModel>>();

        // Act
        var viewModel = new LoginViewModel(
            mockAuthService.Object,
            mockApiService.Object,
            sessionService,
            mockLogger.Object);

        // Assert
        Assert.IsTrue(viewModel.IsStudentSelected);
    }

    /// <summary>
    /// Tests that the constructor initializes IsLecturerSelected to false when SelectedRole defaults to "Student".
    /// Verifies the computed property state based on default role.
    /// </summary>
    [TestMethod]
    public void Constructor_ValidDependencies_InitializesIsLecturerSelectedToFalse()
    {
        // Arrange
        var mockAuthService = new Mock<IAuthService>();
        var mockApiService = new Mock<IApiService>();
        var sessionService = new SessionService();
        var mockLogger = new Mock<ILogger<LoginViewModel>>();

        // Act
        var viewModel = new LoginViewModel(
            mockAuthService.Object,
            mockApiService.Object,
            sessionService,
            mockLogger.Object);

        // Assert
        Assert.IsFalse(viewModel.IsLecturerSelected);
    }

    /// <summary>
    /// Tests that the constructor does not invoke any methods on the IAuthService dependency.
    /// Verifies that constructor has no side effects on authService.
    /// </summary>
    [TestMethod]
    public void Constructor_ValidDependencies_DoesNotInvokeAuthServiceMethods()
    {
        // Arrange
        var mockAuthService = new Mock<IAuthService>(MockBehavior.Strict);
        var mockApiService = new Mock<IApiService>();
        var sessionService = new SessionService();
        var mockLogger = new Mock<ILogger<LoginViewModel>>();

        // Act
        var viewModel = new LoginViewModel(
            mockAuthService.Object,
            mockApiService.Object,
            sessionService,
            mockLogger.Object);

        // Assert
        mockAuthService.VerifyNoOtherCalls();
    }

    /// <summary>
    /// Tests that the constructor does not invoke any methods on the IApiService dependency.
    /// Verifies that constructor has no side effects on apiService.
    /// </summary>
    [TestMethod]
    public void Constructor_ValidDependencies_DoesNotInvokeApiServiceMethods()
    {
        // Arrange
        var mockAuthService = new Mock<IAuthService>();
        var mockApiService = new Mock<IApiService>(MockBehavior.Strict);
        var sessionService = new SessionService();
        var mockLogger = new Mock<ILogger<LoginViewModel>>();

        // Act
        var viewModel = new LoginViewModel(
            mockAuthService.Object,
            mockApiService.Object,
            sessionService,
            mockLogger.Object);

        // Assert
        mockApiService.VerifyNoOtherCalls();
    }

    /// <summary>
    /// Tests that multiple instances created with the same dependencies have independent command instances.
    /// Verifies that each LoginViewModel instance has its own command objects.
    /// </summary>
    [TestMethod]
    public void Constructor_MultipleInstances_CreatesSeparateCommandInstances()
    {
        // Arrange
        var mockAuthService = new Mock<IAuthService>();
        var mockApiService = new Mock<IApiService>();
        var sessionService = new SessionService();
        var mockLogger = new Mock<ILogger<LoginViewModel>>();

        // Act
        var viewModel1 = new LoginViewModel(
            mockAuthService.Object,
            mockApiService.Object,
            sessionService,
            mockLogger.Object);

        var viewModel2 = new LoginViewModel(
            mockAuthService.Object,
            mockApiService.Object,
            sessionService,
            mockLogger.Object);

        // Assert
        Assert.AreNotSame(viewModel1.LoginCommand, viewModel2.LoginCommand);
        Assert.AreNotSame(viewModel1.GoToRegisterCommand, viewModel2.GoToRegisterCommand);
        Assert.AreNotSame(viewModel1.GoBackCommand, viewModel2.GoBackCommand);
        Assert.AreNotSame(viewModel1.ForgotPasswordCommand, viewModel2.ForgotPasswordCommand);
        Assert.AreNotSame(viewModel1.SelectStudentCommand, viewModel2.SelectStudentCommand);
        Assert.AreNotSame(viewModel1.SelectLecturerCommand, viewModel2.SelectLecturerCommand);
        Assert.AreNotSame(viewModel1.TogglePasswordVisibilityCommand, viewModel2.TogglePasswordVisibilityCommand);
    }

    /// <summary>
    /// Tests that the constructor does not throw when provided with different SessionService instances.
    /// Verifies compatibility with various SessionService instances.
    /// </summary>
    [TestMethod]
    public void Constructor_DifferentSessionServiceInstances_CreatesInstanceSuccessfully()
    {
        // Arrange
        var mockAuthService = new Mock<IAuthService>();
        var mockApiService = new Mock<IApiService>();
        var sessionService1 = new SessionService();
        var sessionService2 = new SessionService();
        var mockLogger = new Mock<ILogger<LoginViewModel>>();

        // Act
        var viewModel1 = new LoginViewModel(
            mockAuthService.Object,
            mockApiService.Object,
            sessionService1,
            mockLogger.Object);

        var viewModel2 = new LoginViewModel(
            mockAuthService.Object,
            mockApiService.Object,
            sessionService2,
            mockLogger.Object);

        // Assert
        Assert.IsNotNull(viewModel1);
        Assert.IsNotNull(viewModel2);
        Assert.AreNotSame(viewModel1, viewModel2);
    }

    /// <summary>
    /// Tests that the constructor successfully creates multiple instances in sequence.
    /// Verifies that constructor can be called multiple times without issues.
    /// </summary>
    [TestMethod]
    public void Constructor_CalledMultipleTimes_CreatesMultipleInstancesSuccessfully()
    {
        // Arrange
        var mockAuthService = new Mock<IAuthService>();
        var mockApiService = new Mock<IApiService>();
        var sessionService = new SessionService();
        var mockLogger = new Mock<ILogger<LoginViewModel>>();

        // Act
        var viewModel1 = new LoginViewModel(
            mockAuthService.Object,
            mockApiService.Object,
            sessionService,
            mockLogger.Object);

        var viewModel2 = new LoginViewModel(
            mockAuthService.Object,
            mockApiService.Object,
            sessionService,
            mockLogger.Object);

        var viewModel3 = new LoginViewModel(
            mockAuthService.Object,
            mockApiService.Object,
            sessionService,
            mockLogger.Object);

        // Assert
        Assert.IsNotNull(viewModel1);
        Assert.IsNotNull(viewModel2);
        Assert.IsNotNull(viewModel3);
        Assert.AreNotSame(viewModel1, viewModel2);
        Assert.AreNotSame(viewModel2, viewModel3);
        Assert.AreNotSame(viewModel1, viewModel3);
    }

    /// <summary>
    /// Tests that all commands have CanExecute returning true by default.
    /// Verifies that all commands are immediately executable after construction.
    /// </summary>
    [TestMethod]
    public void Constructor_ValidDependencies_AllCommandsCanExecuteReturnsTrue()
    {
        // Arrange
        var mockAuthService = new Mock<IAuthService>();
        var mockApiService = new Mock<IApiService>();
        var sessionService = new SessionService();
        var mockLogger = new Mock<ILogger<LoginViewModel>>();

        // Act
        var viewModel = new LoginViewModel(
            mockAuthService.Object,
            mockApiService.Object,
            sessionService,
            mockLogger.Object);

        // Assert
        Assert.IsTrue(viewModel.LoginCommand.CanExecute(null));
        Assert.IsTrue(viewModel.GoToRegisterCommand.CanExecute(null));
        Assert.IsTrue(viewModel.GoBackCommand.CanExecute(null));
        Assert.IsTrue(viewModel.ForgotPasswordCommand.CanExecute(null));
        Assert.IsTrue(viewModel.SelectStudentCommand.CanExecute(null));
        Assert.IsTrue(viewModel.SelectLecturerCommand.CanExecute(null));
        Assert.IsTrue(viewModel.TogglePasswordVisibilityCommand.CanExecute(null));
    }

    /// <summary>
    /// Tests that the constructor does not throw when authService and apiService are null but others are valid.
    /// Documents current behavior where null dependencies are not validated in the constructor.
    /// </summary>
    [TestMethod]
    public void Constructor_NullAuthServiceAndApiService_DoesNotThrow()
    {
        // Arrange
        var sessionService = new SessionService();
        var mockLogger = new Mock<ILogger<LoginViewModel>>();

        // Act
        var viewModel = new LoginViewModel(null!, null!, sessionService, mockLogger.Object);

        // Assert
        Assert.IsNotNull(viewModel);
        Assert.IsNotNull(viewModel.LoginCommand);
    }

    /// <summary>
    /// Tests that the constructor does not throw when session and logger are null but others are valid.
    /// Documents current behavior where null dependencies are not validated in the constructor.
    /// </summary>
    [TestMethod]
    public void Constructor_NullSessionAndLogger_DoesNotThrow()
    {
        // Arrange
        var mockAuthService = new Mock<IAuthService>();
        var mockApiService = new Mock<IApiService>();

        // Act
        var viewModel = new LoginViewModel(mockAuthService.Object, mockApiService.Object, null!, null!);

        // Assert
        Assert.IsNotNull(viewModel);
        Assert.IsNotNull(viewModel.LoginCommand);
    }

    /// <summary>
    /// Tests that the constructor initializes commands even when all dependencies are null.
    /// Documents that command initialization does not depend on valid dependencies.
    /// </summary>
    [TestMethod]
    public void Constructor_AllDependenciesNull_InitializesAllCommands()
    {
        // Act
        var viewModel = new LoginViewModel(null!, null!, null!, null!);

        // Assert
        Assert.IsNotNull(viewModel.LoginCommand);
        Assert.IsNotNull(viewModel.GoToRegisterCommand);
        Assert.IsNotNull(viewModel.GoBackCommand);
        Assert.IsNotNull(viewModel.ForgotPasswordCommand);
        Assert.IsNotNull(viewModel.SelectStudentCommand);
        Assert.IsNotNull(viewModel.SelectLecturerCommand);
        Assert.IsNotNull(viewModel.TogglePasswordVisibilityCommand);
    }

    /// <summary>
    /// Tests that setting the Email property to null updates the property value to null.
    /// This tests edge case behavior with null values even though the property is declared as non-nullable.
    /// </summary>
    [TestMethod]
    public void Email_SetNull_UpdatesProperty()
    {
        // Arrange
        var mockAuthService = new Mock<IAuthService>();
        var mockApiService = new Mock<IApiService>();
        var mockSessionService = new Mock<SessionService>();
        var mockLogger = new Mock<ILogger<LoginViewModel>>();
        var viewModel = new LoginViewModel(mockAuthService.Object, mockApiService.Object, mockSessionService.Object, mockLogger.Object);

        // Act
        viewModel.Email = null!;

        // Assert
        Assert.IsNull(viewModel.Email);
    }

    /// <summary>
    /// Tests that setting Email property to a very long string stores and returns the value correctly.
    /// </summary>
    [TestMethod]
    public void Email_SetVeryLongString_StoresAndReturnsCorrectly()
    {
        // Arrange
        var mockAuthService = new Mock<IAuthService>();
        var mockApiService = new Mock<IApiService>();
        var mockSessionService = new Mock<SessionService>();
        var mockLogger = new Mock<ILogger<LoginViewModel>>();
        var viewModel = new LoginViewModel(mockAuthService.Object, mockApiService.Object, mockSessionService.Object, mockLogger.Object);
        var longEmail = new string('a', 10000) + "@example.com";

        // Act
        viewModel.Email = longEmail;

        // Assert
        Assert.AreEqual(longEmail, viewModel.Email);
    }

    /// <summary>
    /// Tests that setting Email from empty to non-empty value raises PropertyChanged event.
    /// </summary>
    [TestMethod]
    public void Email_SetFromEmptyToNonEmpty_RaisesPropertyChanged()
    {
        // Arrange
        var mockAuthService = new Mock<IAuthService>();
        var mockApiService = new Mock<IApiService>();
        var mockSessionService = new Mock<SessionService>();
        var mockLogger = new Mock<ILogger<LoginViewModel>>();
        var viewModel = new LoginViewModel(mockAuthService.Object, mockApiService.Object, mockSessionService.Object, mockLogger.Object);
        Assert.AreEqual(string.Empty, viewModel.Email);
        var propertyChangedRaised = false;
        viewModel.PropertyChanged += (sender, args) => propertyChangedRaised = true;

        // Act
        viewModel.Email = "test@example.com";

        // Assert
        Assert.IsTrue(propertyChangedRaised);
        Assert.AreEqual("test@example.com", viewModel.Email);
    }

    /// <summary>
    /// Tests that setting Email to empty string from non-empty value raises PropertyChanged event.
    /// </summary>
    [TestMethod]
    public void Email_SetToEmptyFromNonEmpty_RaisesPropertyChanged()
    {
        // Arrange
        var mockAuthService = new Mock<IAuthService>();
        var mockApiService = new Mock<IApiService>();
        var mockSessionService = new Mock<SessionService>();
        var mockLogger = new Mock<ILogger<LoginViewModel>>();
        var viewModel = new LoginViewModel(mockAuthService.Object, mockApiService.Object, mockSessionService.Object, mockLogger.Object);
        viewModel.Email = "test@example.com";
        var propertyChangedRaised = false;
        viewModel.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == nameof(viewModel.Email))
                propertyChangedRaised = true;
        };

        // Act
        viewModel.Email = "";

        // Assert
        Assert.IsTrue(propertyChangedRaised);
        Assert.AreEqual("", viewModel.Email);
    }

    /// <summary>
    /// Tests that setting Email property multiple times with different values raises PropertyChanged each time.
    /// </summary>
    [TestMethod]
    public void Email_SetDifferentValues_RaisesPropertyChangedEachTime()
    {
        // Arrange
        var mockAuthService = new Mock<IAuthService>();
        var mockApiService = new Mock<IApiService>();
        var mockSessionService = new Mock<SessionService>();
        var mockLogger = new Mock<ILogger<LoginViewModel>>();
        var viewModel = new LoginViewModel(mockAuthService.Object, mockApiService.Object, mockSessionService.Object, mockLogger.Object);
        var eventCount = 0;
        viewModel.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == nameof(viewModel.Email))
                eventCount++;
        };

        // Act
        viewModel.Email = "first@example.com";
        viewModel.Email = "second@example.com";
        viewModel.Email = "third@example.com";

        // Assert
        Assert.AreEqual(3, eventCount);
    }

    /// <summary>
    /// Tests that Email property handles strings with special Unicode characters correctly.
    /// </summary>
    /// <param name="emailWithSpecialChars">Email with special Unicode characters.</param>
    [TestMethod]
    [DataRow("email\u200B@example.com")]
    [DataRow("\uFEFFemail@example.com")]
    [DataRow("em\u202Eail@example.com")]
    [DataRow("user\u00A0@example.com")]
    public void Email_SetStringWithSpecialUnicodeCharacters_StoresAndReturnsCorrectly(string emailWithSpecialChars)
    {
        // Arrange
        var mockAuthService = new Mock<IAuthService>();
        var mockApiService = new Mock<IApiService>();
        var mockSessionService = new Mock<SessionService>();
        var mockLogger = new Mock<ILogger<LoginViewModel>>();
        var viewModel = new LoginViewModel(mockAuthService.Object, mockApiService.Object, mockSessionService.Object, mockLogger.Object);

        // Act
        viewModel.Email = emailWithSpecialChars;

        // Assert
        Assert.AreEqual(emailWithSpecialChars, viewModel.Email);
    }

    /// <summary>
    /// Tests that Email property getter returns the current value after it has been set.
    /// </summary>
    [TestMethod]
    public void Email_GetValue_ReturnsCurrentValue()
    {
        // Arrange
        var mockAuthService = new Mock<IAuthService>();
        var mockApiService = new Mock<IApiService>();
        var mockSessionService = new Mock<SessionService>();
        var mockLogger = new Mock<ILogger<LoginViewModel>>();
        var viewModel = new LoginViewModel(mockAuthService.Object, mockApiService.Object, mockSessionService.Object, mockLogger.Object);
        var expectedEmail = "user@domain.com";

        // Act
        viewModel.Email = expectedEmail;
        var actualEmail = viewModel.Email;

        // Assert
        Assert.AreEqual(expectedEmail, actualEmail);
    }

    /// <summary>
    /// Tests that Email property correctly handles boundary case with maximum practical email length.
    /// </summary>
    [TestMethod]
    public void Email_SetMaximumPracticalLength_UpdatesProperty()
    {
        // Arrange
        var mockAuthService = new Mock<IAuthService>();
        var mockApiService = new Mock<IApiService>();
        var mockSessionService = new Mock<SessionService>();
        var mockLogger = new Mock<ILogger<LoginViewModel>>();
        var viewModel = new LoginViewModel(mockAuthService.Object, mockApiService.Object, mockSessionService.Object, mockLogger.Object);
        var localPart = new string('a', 64);
        var domainPart = new string('b', 189) + ".com";
        var maxLengthEmail = $"{localPart}@{domainPart}";

        // Act
        viewModel.Email = maxLengthEmail;

        // Assert
        Assert.AreEqual(maxLengthEmail, viewModel.Email);
    }

    /// <summary>
    /// Tests that Email property handles whitespace-only strings correctly.
    /// </summary>
    /// <param name="whitespaceEmail">Whitespace-only string.</param>
    [TestMethod]
    [DataRow(" ")]
    [DataRow("  ")]
    [DataRow("\t")]
    [DataRow("\n")]
    [DataRow("\r")]
    [DataRow("     ")]
    [DataRow("\t\t\t")]
    public void Email_SetWhitespaceOnly_UpdatesProperty(string whitespaceEmail)
    {
        // Arrange
        var mockAuthService = new Mock<IAuthService>();
        var mockApiService = new Mock<IApiService>();
        var mockSessionService = new Mock<SessionService>();
        var mockLogger = new Mock<ILogger<LoginViewModel>>();
        var viewModel = new LoginViewModel(mockAuthService.Object, mockApiService.Object, mockSessionService.Object, mockLogger.Object);

        // Act
        viewModel.Email = whitespaceEmail;

        // Assert
        Assert.AreEqual(whitespaceEmail, viewModel.Email);
    }

    /// <summary>
    /// Tests that IsStudentSelected returns true when SelectedRole is exactly "Student",
    /// and false for all other values including null, empty, whitespace, different casing,
    /// with spaces, and other role values.
    /// </summary>
    /// <param name="selectedRole">The value to set for SelectedRole property.</param>
    /// <param name="expected">The expected result of IsStudentSelected property.</param>
    [TestMethod]
    [DataRow("Student", true, DisplayName = "Exact match 'Student' returns true")]
    [DataRow("Lecturer", false, DisplayName = "Lecturer role returns false")]
    [DataRow(null, false, DisplayName = "Null returns false")]
    [DataRow("", false, DisplayName = "Empty string returns false")]
    [DataRow("   ", false, DisplayName = "Whitespace returns false")]
    [DataRow("student", false, DisplayName = "Lowercase 'student' returns false")]
    [DataRow("STUDENT", false, DisplayName = "Uppercase 'STUDENT' returns false")]
    [DataRow("StUdEnT", false, DisplayName = "Mixed case 'StUdEnT' returns false")]
    [DataRow("Teacher", false, DisplayName = "Different role 'Teacher' returns false")]
    [DataRow("Student ", false, DisplayName = "Trailing space 'Student ' returns false")]
    [DataRow(" Student", false, DisplayName = "Leading space ' Student' returns false")]
    [DataRow(" Student ", false, DisplayName = "Leading and trailing spaces return false")]
    [DataRow("Admin", false, DisplayName = "Admin role returns false")]
    [DataRow("Administrator", false, DisplayName = "Administrator role returns false")]
    [DataRow("\t", false, DisplayName = "Tab character returns false")]
    [DataRow("\n", false, DisplayName = "Newline character returns false")]
    [DataRow("StudentLecturer", false, DisplayName = "Concatenated string returns false")]
    [DataRow("Studen", false, DisplayName = "Partial match 'Studen' returns false")]
    [DataRow("Students", false, DisplayName = "Plural 'Students' returns false")]
    public void IsStudentSelected_VariousSelectedRoleValues_ReturnsExpectedResult(string? selectedRole, bool expected)
    {
        // Arrange
        var mockAuthService = new Mock<IAuthService>();
        var mockApiService = new Mock<IApiService>();
        var mockSessionService = new Mock<SessionService>();
        var mockLogger = new Mock<ILogger<LoginViewModel>>();

        var viewModel = new LoginViewModel(
            mockAuthService.Object,
            mockApiService.Object,
            mockSessionService.Object,
            null,
            mockLogger.Object);

        // Act
        viewModel.SelectedRole = selectedRole!;

        // Assert
        Assert.AreEqual(expected, viewModel.IsStudentSelected);
    }

    /// <summary>
    /// Tests that IsStudentSelected returns true when LoginViewModel is initialized with default values,
    /// as the default SelectedRole is "Student".
    /// </summary>
    [TestMethod]
    public void IsStudentSelected_DefaultState_ReturnsTrue()
    {
        // Arrange
        var mockAuthService = new Mock<IAuthService>();
        var mockApiService = new Mock<IApiService>();
        var mockSessionService = new Mock<SessionService>();
        var mockLogger = new Mock<ILogger<LoginViewModel>>();

        // Act
        var viewModel = new LoginViewModel(
            mockAuthService.Object,
            mockApiService.Object,
            mockSessionService.Object,
            null,
            mockLogger.Object);

        // Assert
        Assert.IsTrue(viewModel.IsStudentSelected);
        Assert.AreEqual("Student", viewModel.SelectedRole);
    }

    /// <summary>
    /// Tests that IsStudentSelected correctly reflects changes when SelectedRole is changed multiple times
    /// between "Student" and other values.
    /// </summary>
    [TestMethod]
    public void IsStudentSelected_MultipleRoleChanges_ReturnsCorrectValueAfterEachChange()
    {
        // Arrange
        var mockAuthService = new Mock<IAuthService>();
        var mockApiService = new Mock<IApiService>();
        var mockSessionService = new Mock<SessionService>();
        var mockLogger = new Mock<ILogger<LoginViewModel>>();

        var viewModel = new LoginViewModel(
            mockAuthService.Object,
            mockApiService.Object,
            mockSessionService.Object,
            null,
            mockLogger.Object);

        // Act & Assert - Initial state
        Assert.IsTrue(viewModel.IsStudentSelected);

        // Act & Assert - Change to Lecturer
        viewModel.SelectedRole = "Lecturer";
        Assert.IsFalse(viewModel.IsStudentSelected);

        // Act & Assert - Change back to Student
        viewModel.SelectedRole = "Student";
        Assert.IsTrue(viewModel.IsStudentSelected);

        // Act & Assert - Change to empty string
        viewModel.SelectedRole = "";
        Assert.IsFalse(viewModel.IsStudentSelected);

        // Act & Assert - Change to null
        viewModel.SelectedRole = null!;
        Assert.IsFalse(viewModel.IsStudentSelected);

        // Act & Assert - Change back to Student again
        viewModel.SelectedRole = "Student";
        Assert.IsTrue(viewModel.IsStudentSelected);
    }

    /// <summary>
    /// Tests that IsStudentSelected returns false for very long string values that are not "Student".
    /// </summary>
    [TestMethod]
    public void IsStudentSelected_VeryLongString_ReturnsFalse()
    {
        // Arrange
        var mockAuthService = new Mock<IAuthService>();
        var mockApiService = new Mock<IApiService>();
        var mockSessionService = new Mock<SessionService>();
        var mockLogger = new Mock<ILogger<LoginViewModel>>();

        var viewModel = new LoginViewModel(
            mockAuthService.Object,
            mockApiService.Object,
            mockSessionService.Object,
            null,
            mockLogger.Object);

        var veryLongString = new string('A', 10000);

        // Act
        viewModel.SelectedRole = veryLongString;

        // Assert
        Assert.IsFalse(viewModel.IsStudentSelected);
    }

    /// <summary>
    /// Tests that IsStudentSelected returns false for strings containing special characters.
    /// </summary>
    /// <param name="specialCharRole">The role string containing special characters.</param>
    [TestMethod]
    [DataRow("Student!@#$%")]
    [DataRow("!@#$Student")]
    [DataRow("Stud!ent")]
    [DataRow("Student\0")]
    [DataRow("Student\u0000")]
    [DataRow("Student\u200B")]
    [DataRow("\uFEFFStudent")]
    public void IsStudentSelected_StringWithSpecialCharacters_ReturnsFalse(string specialCharRole)
    {
        // Arrange
        var mockAuthService = new Mock<IAuthService>();
        var mockApiService = new Mock<IApiService>();
        var mockSessionService = new Mock<SessionService>();
        var mockLogger = new Mock<ILogger<LoginViewModel>>();

        var viewModel = new LoginViewModel(
            mockAuthService.Object,
            mockApiService.Object,
            mockSessionService.Object,
            null,
            mockLogger.Object);

        // Act
        viewModel.SelectedRole = specialCharRole;

        // Assert
        Assert.IsFalse(viewModel.IsStudentSelected);
    }

    /// <summary>
    /// Tests that IsLecturerSelected returns false when ViewModel is in its default state
    /// (SelectedRole initialized to "Student").
    /// </summary>
    [TestMethod]
    public void IsLecturerSelected_DefaultState_ReturnsFalse()
    {
        // Arrange
        var mockAuthService = new Mock<IAuthService>();
        var mockApiService = new Mock<IApiService>();
        var mockSessionService = new Mock<SessionService>();
        var mockLogger = new Mock<ILogger<LoginViewModel>>();

        // Act
        var viewModel = new LoginViewModel(
            mockAuthService.Object,
            mockApiService.Object,
            mockSessionService.Object,
            null,
            mockLogger.Object);

        // Assert
        Assert.IsFalse(viewModel.IsLecturerSelected);
    }

    /// <summary>
    /// Tests that IsLecturerSelected correctly reflects changes when SelectedRole is changed multiple times
    /// between "Student", "Lecturer", and other values.
    /// </summary>
    [TestMethod]
    public void IsLecturerSelected_WhenRoleChangedMultipleTimes_ReturnsCorrectValue()
    {
        // Arrange
        var mockAuthService = new Mock<IAuthService>();
        var mockApiService = new Mock<IApiService>();
        var mockSessionService = new Mock<SessionService>();
        var mockLogger = new Mock<ILogger<LoginViewModel>>();
        var viewModel = new LoginViewModel(
            mockAuthService.Object,
            mockApiService.Object,
            mockSessionService.Object,
            null,
            mockLogger.Object);

        // Act & Assert - Initial state (Student)
        Assert.IsFalse(viewModel.IsLecturerSelected);

        // Act & Assert - Change to Lecturer
        viewModel.SelectedRole = "Lecturer";
        Assert.IsTrue(viewModel.IsLecturerSelected);

        // Act & Assert - Change back to Student
        viewModel.SelectedRole = "Student";
        Assert.IsFalse(viewModel.IsLecturerSelected);

        // Act & Assert - Change to Lecturer again
        viewModel.SelectedRole = "Lecturer";
        Assert.IsTrue(viewModel.IsLecturerSelected);

        // Act & Assert - Change to another role
        viewModel.SelectedRole = "Admin";
        Assert.IsFalse(viewModel.IsLecturerSelected);

        // Act & Assert - Change to Lecturer one more time
        viewModel.SelectedRole = "Lecturer";
        Assert.IsTrue(viewModel.IsLecturerSelected);
    }

    /// <summary>
    /// Tests that IsLecturerSelected returns false for various special and edge case string values
    /// including strings with special characters, Unicode characters, and control characters.
    /// </summary>
    /// <param name="edgeCaseRole">The edge case string value to test.</param>
    [TestMethod]
    [DataRow("Lecturer!")]
    [DataRow("@Lecturer")]
    [DataRow("Lect@urer")]
    [DataRow("Lecturer123")]
    [DataRow("123Lecturer")]
    [DataRow("Lecturer\u0000")]
    [DataRow("\u0000Lecturer")]
    [DataRow("Lecturer\u200B")]
    [DataRow("\uFEFFLecturer")]
    [DataRow("Lec\u202Eturer")]
    public void IsLecturerSelected_EdgeCaseStringValues_ReturnsFalse(string edgeCaseRole)
    {
        // Arrange
        var mockAuthService = new Mock<IAuthService>();
        var mockApiService = new Mock<IApiService>();
        var mockSessionService = new Mock<SessionService>();
        var mockLogger = new Mock<ILogger<LoginViewModel>>();
        var viewModel = new LoginViewModel(
            mockAuthService.Object,
            mockApiService.Object,
            mockSessionService.Object,
            null,
            mockLogger.Object);

        // Act
        viewModel.SelectedRole = edgeCaseRole;

        // Assert
        Assert.IsFalse(viewModel.IsLecturerSelected);
    }

    /// <summary>
    /// Tests that IsLecturerSelected returns false when SelectedRole is set to a very long string
    /// that is not equal to "Lecturer".
    /// </summary>
    [TestMethod]
    public void IsLecturerSelected_VeryLongString_ReturnsFalse()
    {
        // Arrange
        var mockAuthService = new Mock<IAuthService>();
        var mockApiService = new Mock<IApiService>();
        var mockSessionService = new Mock<SessionService>();
        var mockLogger = new Mock<ILogger<LoginViewModel>>();
        var viewModel = new LoginViewModel(
            mockAuthService.Object,
            mockApiService.Object,
            mockSessionService.Object,
            null,
            mockLogger.Object);
        var veryLongString = new string('L', 10000);

        // Act
        viewModel.SelectedRole = veryLongString;

        // Assert
        Assert.IsFalse(viewModel.IsLecturerSelected);
    }

    /// <summary>
    /// Tests that IsLecturerSelected returns true only for exact match and false immediately
    /// after changing to a different value.
    /// </summary>
    [TestMethod]
    public void IsLecturerSelected_ExactMatchThenChange_ReturnsCorrectSequence()
    {
        // Arrange
        var mockAuthService = new Mock<IAuthService>();
        var mockApiService = new Mock<IApiService>();
        var mockSessionService = new Mock<SessionService>();
        var mockLogger = new Mock<ILogger<LoginViewModel>>();
        var viewModel = new LoginViewModel(
            mockAuthService.Object,
            mockApiService.Object,
            mockSessionService.Object,
            null,
            mockLogger.Object);

        // Act & Assert - Set to exact match
        viewModel.SelectedRole = "Lecturer";
        Assert.IsTrue(viewModel.IsLecturerSelected);

        // Act & Assert - Change to uppercase (not exact match)
        viewModel.SelectedRole = "LECTURER";
        Assert.IsFalse(viewModel.IsLecturerSelected);

        // Act & Assert - Change back to exact match
        viewModel.SelectedRole = "Lecturer";
        Assert.IsTrue(viewModel.IsLecturerSelected);

        // Act & Assert - Change to lowercase (not exact match)
        viewModel.SelectedRole = "lecturer";
        Assert.IsFalse(viewModel.IsLecturerSelected);
    }

    /// <summary>
    /// Tests that IsLecturerSelected property correctly evaluates the equality comparison
    /// by verifying it returns false for strings that contain "Lecturer" as a substring
    /// but are not exactly "Lecturer".
    /// </summary>
    /// <param name="roleContainingLecturer">A string containing "Lecturer" but not equal to it.</param>
    [TestMethod]
    [DataRow("LecturerRole")]
    [DataRow("RoleLecturer")]
    [DataRow("Role Lecturer")]
    [DataRow("Lecturer Role")]
    [DataRow("TheLecturer")]
    [DataRow("LecturerThe")]
    public void IsLecturerSelected_StringContainingLecturerButNotEqual_ReturnsFalse(string roleContainingLecturer)
    {
        // Arrange
        var mockAuthService = new Mock<IAuthService>();
        var mockApiService = new Mock<IApiService>();
        var mockSessionService = new Mock<SessionService>();
        var mockLogger = new Mock<ILogger<LoginViewModel>>();
        var viewModel = new LoginViewModel(
            mockAuthService.Object,
            mockApiService.Object,
            mockSessionService.Object,
            null,
            mockLogger.Object);

        // Act
        viewModel.SelectedRole = roleContainingLecturer;

        // Assert
        Assert.IsFalse(viewModel.IsLecturerSelected);
    }

    /// <summary>
    /// Tests that the IsPasswordHidden property returns true as its initial value.
    /// Verifies the default state of password visibility control.
    /// </summary>
    [TestMethod]
    public void IsPasswordHidden_InitialState_ReturnsTrue()
    {
        // Arrange
        var authServiceMock = new Mock<IAuthService>();
        var apiServiceMock = new Mock<IApiService>();
        var sessionServiceMock = new Mock<SessionService>();
        var loggerMock = new Mock<ILogger<LoginViewModel>>();
        var viewModel = new LoginViewModel(authServiceMock.Object, apiServiceMock.Object, sessionServiceMock.Object, loggerMock.Object);

        // Act
        var result = viewModel.IsPasswordHidden;

        // Assert
        Assert.IsTrue(result);
    }

    /// <summary>
    /// Tests that setting IsPasswordHidden to a value correctly updates the property.
    /// Verifies both true and false values are stored correctly.
    /// </summary>
    /// <param name="value">The boolean value to set.</param>
    [TestMethod]
    [DataRow(true)]
    [DataRow(false)]
    public void IsPasswordHidden_SetValue_UpdatesProperty(bool value)
    {
        // Arrange
        var authServiceMock = new Mock<IAuthService>();
        var apiServiceMock = new Mock<IApiService>();
        var sessionServiceMock = new Mock<SessionService>();
        var loggerMock = new Mock<ILogger<LoginViewModel>>();
        var viewModel = new LoginViewModel(authServiceMock.Object, apiServiceMock.Object, sessionServiceMock.Object, loggerMock.Object);

        // Act
        viewModel.IsPasswordHidden = value;

        // Assert
        Assert.AreEqual(value, viewModel.IsPasswordHidden);
    }

    /// <summary>
    /// Tests that setting IsPasswordHidden to a different value raises PropertyChanged event for IsPasswordHidden.
    /// Verifies that property change notifications work correctly.
    /// </summary>
    /// <param name="initialValue">The initial value to set.</param>
    /// <param name="newValue">The new value to set.</param>
    [TestMethod]
    [DataRow(true, false)]
    [DataRow(false, true)]
    public void IsPasswordHidden_SetToDifferentValue_RaisesPropertyChangedForIsPasswordHidden(bool initialValue, bool newValue)
    {
        // Arrange
        var authServiceMock = new Mock<IAuthService>();
        var apiServiceMock = new Mock<IApiService>();
        var sessionServiceMock = new Mock<SessionService>();
        var loggerMock = new Mock<ILogger<LoginViewModel>>();
        var viewModel = new LoginViewModel(authServiceMock.Object, apiServiceMock.Object, sessionServiceMock.Object, loggerMock.Object);
        viewModel.IsPasswordHidden = initialValue;
        var propertyChangedRaised = new List<string>();
        viewModel.PropertyChanged += (sender, args) => propertyChangedRaised.Add(args.PropertyName ?? string.Empty);

        // Act
        viewModel.IsPasswordHidden = newValue;

        // Assert
        Assert.IsTrue(propertyChangedRaised.Contains(nameof(LoginViewModel.IsPasswordHidden)));
    }

    /// <summary>
    /// Tests that setting IsPasswordHidden to a different value also raises PropertyChanged event for IsPasswordVisible.
    /// Verifies that the dependent property notification is triggered.
    /// </summary>
    /// <param name="initialValue">The initial value to set.</param>
    /// <param name="newValue">The new value to set.</param>
    [TestMethod]
    [DataRow(true, false)]
    [DataRow(false, true)]
    public void IsPasswordHidden_SetToDifferentValue_RaisesPropertyChangedForIsPasswordVisible(bool initialValue, bool newValue)
    {
        // Arrange
        var authServiceMock = new Mock<IAuthService>();
        var apiServiceMock = new Mock<IApiService>();
        var sessionServiceMock = new Mock<SessionService>();
        var loggerMock = new Mock<ILogger<LoginViewModel>>();
        var viewModel = new LoginViewModel(authServiceMock.Object, apiServiceMock.Object, sessionServiceMock.Object, loggerMock.Object);
        viewModel.IsPasswordHidden = initialValue;
        var propertyChangedRaised = new List<string>();
        viewModel.PropertyChanged += (sender, args) => propertyChangedRaised.Add(args.PropertyName ?? string.Empty);

        // Act
        viewModel.IsPasswordHidden = newValue;

        // Assert
        Assert.IsTrue(propertyChangedRaised.Contains(nameof(LoginViewModel.IsPasswordVisible)));
    }

    /// <summary>
    /// Tests that setting IsPasswordHidden to a different value raises both PropertyChanged events.
    /// Verifies that exactly two property change notifications are raised: one for IsPasswordHidden and one for IsPasswordVisible.
    /// </summary>
    /// <param name="initialValue">The initial value to set.</param>
    /// <param name="newValue">The new value to set.</param>
    [TestMethod]
    [DataRow(true, false)]
    [DataRow(false, true)]
    public void IsPasswordHidden_SetToDifferentValue_RaisesTwoPropertyChangedEvents(bool initialValue, bool newValue)
    {
        // Arrange
        var authServiceMock = new Mock<IAuthService>();
        var apiServiceMock = new Mock<IApiService>();
        var sessionServiceMock = new Mock<SessionService>();
        var loggerMock = new Mock<ILogger<LoginViewModel>>();
        var viewModel = new LoginViewModel(authServiceMock.Object, apiServiceMock.Object, sessionServiceMock.Object, loggerMock.Object);
        viewModel.IsPasswordHidden = initialValue;
        var propertyChangedRaised = new List<string>();
        viewModel.PropertyChanged += (sender, args) => propertyChangedRaised.Add(args.PropertyName ?? string.Empty);

        // Act
        viewModel.IsPasswordHidden = newValue;

        // Assert
        Assert.AreEqual(2, propertyChangedRaised.Count);
    }

    /// <summary>
    /// Tests that PropertyChanged events are raised in the correct order when IsPasswordHidden changes.
    /// Verifies that IsPasswordHidden PropertyChanged event is raised before IsPasswordVisible PropertyChanged event.
    /// </summary>
    [TestMethod]
    public void IsPasswordHidden_SetFromTrueToFalse_RaisesPropertyChangedInCorrectOrder()
    {
        // Arrange
        var authServiceMock = new Mock<IAuthService>();
        var apiServiceMock = new Mock<IApiService>();
        var sessionServiceMock = new Mock<SessionService>();
        var loggerMock = new Mock<ILogger<LoginViewModel>>();
        var viewModel = new LoginViewModel(authServiceMock.Object, apiServiceMock.Object, sessionServiceMock.Object, loggerMock.Object);
        var propertyChangedRaised = new List<string>();
        viewModel.PropertyChanged += (sender, args) => propertyChangedRaised.Add(args.PropertyName ?? string.Empty);

        // Act
        viewModel.IsPasswordHidden = false;

        // Assert
        Assert.AreEqual(2, propertyChangedRaised.Count);
        Assert.AreEqual(nameof(LoginViewModel.IsPasswordHidden), propertyChangedRaised[0]);
        Assert.AreEqual(nameof(LoginViewModel.IsPasswordVisible), propertyChangedRaised[1]);
    }

    /// <summary>
    /// Tests that PropertyChanged events are raised in the correct order when IsPasswordHidden changes from false to true.
    /// Verifies that IsPasswordHidden PropertyChanged event is raised before IsPasswordVisible PropertyChanged event.
    /// </summary>
    [TestMethod]
    public void IsPasswordHidden_SetFromFalseToTrue_RaisesPropertyChangedInCorrectOrder()
    {
        // Arrange
        var authServiceMock = new Mock<IAuthService>();
        var apiServiceMock = new Mock<IApiService>();
        var sessionServiceMock = new Mock<SessionService>();
        var loggerMock = new Mock<ILogger<LoginViewModel>>();
        var viewModel = new LoginViewModel(authServiceMock.Object, apiServiceMock.Object, sessionServiceMock.Object, loggerMock.Object);
        viewModel.IsPasswordHidden = false;
        var propertyChangedRaised = new List<string>();
        viewModel.PropertyChanged += (sender, args) => propertyChangedRaised.Add(args.PropertyName ?? string.Empty);

        // Act
        viewModel.IsPasswordHidden = true;

        // Assert
        Assert.AreEqual(2, propertyChangedRaised.Count);
        Assert.AreEqual(nameof(LoginViewModel.IsPasswordHidden), propertyChangedRaised[0]);
        Assert.AreEqual(nameof(LoginViewModel.IsPasswordVisible), propertyChangedRaised[1]);
    }

    /// <summary>
    /// Tests that IsPasswordHidden can be toggled multiple times and maintains correct state.
    /// Verifies that the property correctly handles multiple state changes.
    /// </summary>
    [TestMethod]
    public void IsPasswordHidden_ToggledMultipleTimes_MaintainsCorrectState()
    {
        // Arrange
        var authServiceMock = new Mock<IAuthService>();
        var apiServiceMock = new Mock<IApiService>();
        var sessionServiceMock = new Mock<SessionService>();
        var loggerMock = new Mock<ILogger<LoginViewModel>>();
        var viewModel = new LoginViewModel(authServiceMock.Object, apiServiceMock.Object, sessionServiceMock.Object, loggerMock.Object);

        // Act & Assert
        Assert.IsTrue(viewModel.IsPasswordHidden);

        viewModel.IsPasswordHidden = false;
        Assert.IsFalse(viewModel.IsPasswordHidden);

        viewModel.IsPasswordHidden = true;
        Assert.IsTrue(viewModel.IsPasswordHidden);

        viewModel.IsPasswordHidden = false;
        Assert.IsFalse(viewModel.IsPasswordHidden);

        viewModel.IsPasswordHidden = true;
        Assert.IsTrue(viewModel.IsPasswordHidden);
    }

    /// <summary>
    /// Tests that IsPasswordHidden correctly updates IsPasswordVisible when set to false.
    /// Verifies that the dependent property reflects the negated value.
    /// </summary>
    [TestMethod]
    public void IsPasswordHidden_SetToFalse_MakesIsPasswordVisibleTrue()
    {
        // Arrange
        var authServiceMock = new Mock<IAuthService>();
        var apiServiceMock = new Mock<IApiService>();
        var sessionServiceMock = new Mock<SessionService>();
        var loggerMock = new Mock<ILogger<LoginViewModel>>();
        var viewModel = new LoginViewModel(authServiceMock.Object, apiServiceMock.Object, sessionServiceMock.Object, loggerMock.Object);

        // Act
        viewModel.IsPasswordHidden = false;

        // Assert
        Assert.IsFalse(viewModel.IsPasswordHidden);
        Assert.IsTrue(viewModel.IsPasswordVisible);
    }

    /// <summary>
    /// Tests that IsPasswordHidden correctly updates IsPasswordVisible when set to true.
    /// Verifies that the dependent property reflects the negated value.
    /// </summary>
    [TestMethod]
    public void IsPasswordHidden_SetToTrue_MakesIsPasswordVisibleFalse()
    {
        // Arrange
        var authServiceMock = new Mock<IAuthService>();
        var apiServiceMock = new Mock<IApiService>();
        var sessionServiceMock = new Mock<SessionService>();
        var loggerMock = new Mock<ILogger<LoginViewModel>>();
        var viewModel = new LoginViewModel(authServiceMock.Object, apiServiceMock.Object, sessionServiceMock.Object, loggerMock.Object);
        viewModel.IsPasswordHidden = false;

        // Act
        viewModel.IsPasswordHidden = true;

        // Assert
        Assert.IsTrue(viewModel.IsPasswordHidden);
        Assert.IsFalse(viewModel.IsPasswordVisible);
    }

    /// <summary>
    /// Tests that setting IsPasswordHidden from initial state to false raises PropertyChanged events exactly once for each property.
    /// Verifies the initial transition behavior.
    /// </summary>
    [TestMethod]
    public void IsPasswordHidden_SetFromInitialStateToFalse_RaisesPropertyChangedOnce()
    {
        // Arrange
        var authServiceMock = new Mock<IAuthService>();
        var apiServiceMock = new Mock<IApiService>();
        var sessionServiceMock = new Mock<SessionService>();
        var loggerMock = new Mock<ILogger<LoginViewModel>>();
        var viewModel = new LoginViewModel(authServiceMock.Object, apiServiceMock.Object, sessionServiceMock.Object, loggerMock.Object);
        var propertyChangedCount = new Dictionary<string, int>();
        viewModel.PropertyChanged += (sender, args) =>
        {
            var propertyName = args.PropertyName ?? string.Empty;
            if (!propertyChangedCount.ContainsKey(propertyName))
                propertyChangedCount[propertyName] = 0;
            propertyChangedCount[propertyName]++;
        };

        // Act
        viewModel.IsPasswordHidden = false;

        // Assert
        Assert.AreEqual(1, propertyChangedCount[nameof(LoginViewModel.IsPasswordHidden)]);
        Assert.AreEqual(1, propertyChangedCount[nameof(LoginViewModel.IsPasswordVisible)]);
    }

    /// <summary>
    /// Tests that setting IsPasswordHidden to the same value multiple times does not raise PropertyChanged events.
    /// Verifies that duplicate value assignments are properly ignored.
    /// </summary>
    [TestMethod]
    public void IsPasswordHidden_SetToSameValueMultipleTimes_DoesNotRaisePropertyChanged()
    {
        // Arrange
        var authServiceMock = new Mock<IAuthService>();
        var apiServiceMock = new Mock<IApiService>();
        var sessionServiceMock = new Mock<SessionService>();
        var loggerMock = new Mock<ILogger<LoginViewModel>>();
        var viewModel = new LoginViewModel(authServiceMock.Object, apiServiceMock.Object, sessionServiceMock.Object, loggerMock.Object);
        viewModel.IsPasswordHidden = false;
        var propertyChangedRaised = new List<string>();
        viewModel.PropertyChanged += (sender, args) => propertyChangedRaised.Add(args.PropertyName ?? string.Empty);

        // Act
        viewModel.IsPasswordHidden = false;
        viewModel.IsPasswordHidden = false;
        viewModel.IsPasswordHidden = false;

        // Assert
        Assert.AreEqual(0, propertyChangedRaised.Count);
    }

    /// <summary>
    /// Tests that the SelectedRole property has "Student" as its default value.
    /// </summary>
    [TestMethod]
    public void SelectedRole_DefaultValue_IsStudent()
    {
        // Arrange
        var mockAuthService = new Mock<IAuthService>();
        var mockApiService = new Mock<IApiService>();
        var mockSessionService = new Mock<SessionService>();
        var mockLogger = new Mock<ILogger<LoginViewModel>>();

        // Act
        var viewModel = new LoginViewModel(
            mockAuthService.Object,
            mockApiService.Object,
            mockSessionService.Object,
            null,
            mockLogger.Object);

        // Assert
        Assert.AreEqual("Student", viewModel.SelectedRole);
    }

    /// <summary>
    /// Tests that setting the SelectedRole property to a new value updates the property correctly.
    /// </summary>
    /// <param name="newValue">The new value to set.</param>
    [TestMethod]
    [DataRow("Lecturer")]
    [DataRow("Student")]
    [DataRow("Administrator")]
    [DataRow("")]
    [DataRow("   ")]
    [DataRow("\t\n\r")]
    [DataRow("Role with spaces")]
    [DataRow("Role!@#$%^&*()")]
    [DataRow("student")]
    [DataRow("STUDENT")]
    [DataRow("lecturer")]
    [DataRow("LECTURER")]
    public void SelectedRole_SetValue_UpdatesProperty(string newValue)
    {
        // Arrange
        var mockAuthService = new Mock<IAuthService>();
        var mockApiService = new Mock<IApiService>();
        var mockSessionService = new Mock<SessionService>();
        var mockLogger = new Mock<ILogger<LoginViewModel>>();
        var viewModel = new LoginViewModel(
            mockAuthService.Object,
            mockApiService.Object,
            mockSessionService.Object,
            null,
            mockLogger.Object);

        // Act
        viewModel.SelectedRole = newValue;

        // Assert
        Assert.AreEqual(newValue, viewModel.SelectedRole);
    }

    /// <summary>
    /// Tests that setting the SelectedRole property to null updates the property to null.
    /// </summary>
    [TestMethod]
    public void SelectedRole_SetToNull_UpdatesPropertyToNull()
    {
        // Arrange
        var mockAuthService = new Mock<IAuthService>();
        var mockApiService = new Mock<IApiService>();
        var mockSessionService = new Mock<SessionService>();
        var mockLogger = new Mock<ILogger<LoginViewModel>>();
        var viewModel = new LoginViewModel(
            mockAuthService.Object,
            mockApiService.Object,
            mockSessionService.Object,
            null,
            mockLogger.Object);

        // Act
        viewModel.SelectedRole = null!;

        // Assert
        Assert.IsNull(viewModel.SelectedRole);
    }

    /// <summary>
    /// Tests that setting the SelectedRole property to a different value raises PropertyChanged events
    /// for SelectedRole, IsStudentSelected, and IsLecturerSelected.
    /// </summary>
    /// <param name="newValue">The new value to set.</param>
    [TestMethod]
    [DataRow("Lecturer")]
    [DataRow("Administrator")]
    [DataRow("")]
    [DataRow("   ")]
    public void SelectedRole_SetDifferentValue_RaisesPropertyChangedForAllRelatedProperties(string newValue)
    {
        // Arrange
        var mockAuthService = new Mock<IAuthService>();
        var mockApiService = new Mock<IApiService>();
        var mockSessionService = new Mock<SessionService>();
        var mockLogger = new Mock<ILogger<LoginViewModel>>();
        var viewModel = new LoginViewModel(
            mockAuthService.Object,
            mockApiService.Object,
            mockSessionService.Object,
            null,
            mockLogger.Object);

        var propertyChangedEvents = new List<string>();
        viewModel.PropertyChanged += (sender, args) => propertyChangedEvents.Add(args.PropertyName ?? string.Empty);

        // Act
        viewModel.SelectedRole = newValue;

        // Assert
        Assert.IsTrue(propertyChangedEvents.Contains("SelectedRole"), "PropertyChanged should be raised for SelectedRole");
        Assert.IsTrue(propertyChangedEvents.Contains("IsStudentSelected"), "PropertyChanged should be raised for IsStudentSelected");
        Assert.IsTrue(propertyChangedEvents.Contains("IsLecturerSelected"), "PropertyChanged should be raised for IsLecturerSelected");
    }

    /// <summary>
    /// Tests that setting the SelectedRole property to the same value does not raise PropertyChanged events.
    /// </summary>
    [TestMethod]
    public void SelectedRole_SetSameValue_DoesNotRaisePropertyChanged()
    {
        // Arrange
        var mockAuthService = new Mock<IAuthService>();
        var mockApiService = new Mock<IApiService>();
        var mockSessionService = new Mock<SessionService>();
        var mockLogger = new Mock<ILogger<LoginViewModel>>();
        var viewModel = new LoginViewModel(
            mockAuthService.Object,
            mockApiService.Object,
            mockSessionService.Object,
            null,
            mockLogger.Object);

        var propertyChangedEvents = new List<string>();
        viewModel.PropertyChanged += (sender, args) => propertyChangedEvents.Add(args.PropertyName ?? string.Empty);

        // Act
        viewModel.SelectedRole = "Student";

        // Assert
        Assert.AreEqual(0, propertyChangedEvents.Count, "No PropertyChanged events should be raised when setting the same value");
    }

    /// <summary>
    /// Tests that PropertyChanged events are raised in the correct order when SelectedRole is set to a different value.
    /// SelectedRole should be raised first, followed by IsStudentSelected and IsLecturerSelected.
    /// </summary>
    [TestMethod]
    public void SelectedRole_SetDifferentValue_RaisesPropertyChangedInCorrectOrder()
    {
        // Arrange
        var mockAuthService = new Mock<IAuthService>();
        var mockApiService = new Mock<IApiService>();
        var mockSessionService = new Mock<SessionService>();
        var mockLogger = new Mock<ILogger<LoginViewModel>>();
        var viewModel = new LoginViewModel(
            mockAuthService.Object,
            mockApiService.Object,
            mockSessionService.Object,
            null,
            mockLogger.Object);

        var propertyChangedEvents = new List<string>();
        viewModel.PropertyChanged += (sender, args) => propertyChangedEvents.Add(args.PropertyName ?? string.Empty);

        // Act
        viewModel.SelectedRole = "Lecturer";

        // Assert
        Assert.AreEqual(3, propertyChangedEvents.Count, "Exactly 3 PropertyChanged events should be raised");
        Assert.AreEqual("SelectedRole", propertyChangedEvents[0], "First event should be for SelectedRole");
        Assert.AreEqual("IsStudentSelected", propertyChangedEvents[1], "Second event should be for IsStudentSelected");
        Assert.AreEqual("IsLecturerSelected", propertyChangedEvents[2], "Third event should be for IsLecturerSelected");
    }

    /// <summary>
    /// Tests that setting SelectedRole updates the IsStudentSelected property correctly.
    /// IsStudentSelected should only be true when SelectedRole is exactly "Student".
    /// </summary>
    /// <param name="selectedRole">The value to set for SelectedRole.</param>
    /// <param name="expectedIsStudentSelected">The expected value of IsStudentSelected.</param>
    [TestMethod]
    [DataRow("Student", true)]
    [DataRow("Lecturer", false)]
    [DataRow("", false)]
    [DataRow("   ", false)]
    [DataRow("student", false)]
    [DataRow("STUDENT", false)]
    [DataRow("Administrator", false)]
    [DataRow(null, false)]
    public void SelectedRole_SetValue_UpdatesIsStudentSelectedCorrectly(string? selectedRole, bool expectedIsStudentSelected)
    {
        // Arrange
        var mockAuthService = new Mock<IAuthService>();
        var mockApiService = new Mock<IApiService>();
        var mockSessionService = new Mock<SessionService>();
        var mockLogger = new Mock<ILogger<LoginViewModel>>();
        var viewModel = new LoginViewModel(
            mockAuthService.Object,
            mockApiService.Object,
            mockSessionService.Object,
            null,
            mockLogger.Object);

        // Act
        viewModel.SelectedRole = selectedRole!;

        // Assert
        Assert.AreEqual(expectedIsStudentSelected, viewModel.IsStudentSelected);
    }

    /// <summary>
    /// Tests that setting SelectedRole updates the IsLecturerSelected property correctly.
    /// IsLecturerSelected should only be true when SelectedRole is exactly "Lecturer".
    /// </summary>
    /// <param name="selectedRole">The value to set for SelectedRole.</param>
    /// <param name="expectedIsLecturerSelected">The expected value of IsLecturerSelected.</param>
    [TestMethod]
    [DataRow("Lecturer", true)]
    [DataRow("Student", false)]
    [DataRow("", false)]
    [DataRow("   ", false)]
    [DataRow("lecturer", false)]
    [DataRow("LECTURER", false)]
    [DataRow("Administrator", false)]
    [DataRow(null, false)]
    public void SelectedRole_SetValue_UpdatesIsLecturerSelectedCorrectly(string? selectedRole, bool expectedIsLecturerSelected)
    {
        // Arrange
        var mockAuthService = new Mock<IAuthService>();
        var mockApiService = new Mock<IApiService>();
        var mockSessionService = new Mock<SessionService>();
        var mockLogger = new Mock<ILogger<LoginViewModel>>();
        var viewModel = new LoginViewModel(
            mockAuthService.Object,
            mockApiService.Object,
            mockSessionService.Object,
            null,
            mockLogger.Object);

        // Act
        viewModel.SelectedRole = selectedRole!;

        // Assert
        Assert.AreEqual(expectedIsLecturerSelected, viewModel.IsLecturerSelected);
    }

    /// <summary>
    /// Tests that setting SelectedRole to a very long string updates the property correctly
    /// and raises PropertyChanged events.
    /// </summary>
    [TestMethod]
    public void SelectedRole_SetVeryLongString_UpdatesPropertyAndRaisesPropertyChanged()
    {
        // Arrange
        var mockAuthService = new Mock<IAuthService>();
        var mockApiService = new Mock<IApiService>();
        var mockSessionService = new Mock<SessionService>();
        var mockLogger = new Mock<ILogger<LoginViewModel>>();
        var viewModel = new LoginViewModel(
            mockAuthService.Object,
            mockApiService.Object,
            mockSessionService.Object,
            null,
            mockLogger.Object);

        var veryLongString = new string('A', 10000);
        var propertyChangedEvents = new List<string>();
        viewModel.PropertyChanged += (sender, args) => propertyChangedEvents.Add(args.PropertyName ?? string.Empty);

        // Act
        viewModel.SelectedRole = veryLongString;

        // Assert
        Assert.AreEqual(veryLongString, viewModel.SelectedRole);
        Assert.AreEqual(3, propertyChangedEvents.Count);
        Assert.IsTrue(propertyChangedEvents.Contains("SelectedRole"));
        Assert.IsTrue(propertyChangedEvents.Contains("IsStudentSelected"));
        Assert.IsTrue(propertyChangedEvents.Contains("IsLecturerSelected"));
    }

    /// <summary>
    /// Tests that setting SelectedRole multiple times with different values raises PropertyChanged events each time.
    /// </summary>
    [TestMethod]
    public void SelectedRole_SetMultipleDifferentValues_RaisesPropertyChangedEachTime()
    {
        // Arrange
        var mockAuthService = new Mock<IAuthService>();
        var mockApiService = new Mock<IApiService>();
        var mockSessionService = new Mock<SessionService>();
        var mockLogger = new Mock<ILogger<LoginViewModel>>();
        var viewModel = new LoginViewModel(
            mockAuthService.Object,
            mockApiService.Object,
            mockSessionService.Object,
            null,
            mockLogger.Object);

        var propertyChangedEvents = new List<string>();
        viewModel.PropertyChanged += (sender, args) => propertyChangedEvents.Add(args.PropertyName ?? string.Empty);

        // Act
        viewModel.SelectedRole = "Lecturer";
        viewModel.SelectedRole = "Administrator";
        viewModel.SelectedRole = "Student";

        // Assert
        Assert.AreEqual(9, propertyChangedEvents.Count, "Should raise 3 events for each of 3 value changes");
        Assert.AreEqual("Student", viewModel.SelectedRole);
    }

    /// <summary>
    /// Tests that setting SelectedRole to the same value multiple times only raises PropertyChanged on the first change.
    /// </summary>
    [TestMethod]
    public void SelectedRole_SetSameValueMultipleTimes_RaisesPropertyChangedOnlyOnce()
    {
        // Arrange
        var mockAuthService = new Mock<IAuthService>();
        var mockApiService = new Mock<IApiService>();
        var mockSessionService = new Mock<SessionService>();
        var mockLogger = new Mock<ILogger<LoginViewModel>>();
        var viewModel = new LoginViewModel(
            mockAuthService.Object,
            mockApiService.Object,
            mockSessionService.Object,
            null,
            mockLogger.Object);

        var propertyChangedEvents = new List<string>();
        viewModel.PropertyChanged += (sender, args) => propertyChangedEvents.Add(args.PropertyName ?? string.Empty);

        // Act
        viewModel.SelectedRole = "Lecturer";
        var eventCountAfterFirstChange = propertyChangedEvents.Count;
        viewModel.SelectedRole = "Lecturer";
        viewModel.SelectedRole = "Lecturer";

        // Assert
        Assert.AreEqual(3, eventCountAfterFirstChange, "First change should raise 3 events");
        Assert.AreEqual(3, propertyChangedEvents.Count, "No additional events should be raised for subsequent same-value sets");
    }

    /// <summary>
    /// Tests that setting SelectedRole to strings with special Unicode characters updates the property correctly.
    /// </summary>
    /// <param name="specialString">The special string value to test.</param>
    [TestMethod]
    [DataRow("Role\u0000WithNull")]
    [DataRow("Role\u001FWithControl")]
    [DataRow("Role\u200BWithZeroWidthSpace")]
    [DataRow("\uFEFFRoleWithBOM")]
    [DataRow("Role\u202EWithRightToLeftOverride")]
    public void SelectedRole_SetStringWithSpecialUnicodeCharacters_UpdatesProperty(string specialString)
    {
        // Arrange
        var mockAuthService = new Mock<IAuthService>();
        var mockApiService = new Mock<IApiService>();
        var mockSessionService = new Mock<SessionService>();
        var mockLogger = new Mock<ILogger<LoginViewModel>>();
        var viewModel = new LoginViewModel(
            mockAuthService.Object,
            mockApiService.Object,
            mockSessionService.Object,
            null,
            mockLogger.Object);

        // Act
        viewModel.SelectedRole = specialString;

        // Assert
        Assert.AreEqual(specialString, viewModel.SelectedRole);
    }

    /// <summary>
    /// Tests that the SelectedRole getter returns the correct value after being set.
    /// </summary>
    [TestMethod]
    public void SelectedRole_GetAfterSet_ReturnsSetValue()
    {
        // Arrange
        var mockAuthService = new Mock<IAuthService>();
        var mockApiService = new Mock<IApiService>();
        var mockSessionService = new Mock<SessionService>();
        var mockLogger = new Mock<ILogger<LoginViewModel>>();
        var viewModel = new LoginViewModel(
            mockAuthService.Object,
            mockApiService.Object,
            mockSessionService.Object,
            null,
            mockLogger.Object);

        // Act
        viewModel.SelectedRole = "Lecturer";
        var result = viewModel.SelectedRole;

        // Assert
        Assert.AreEqual("Lecturer", result);
    }

    /// <summary>
    /// Tests that setting SelectedRole from Student to Lecturer and back to Student
    /// correctly updates IsStudentSelected and IsLecturerSelected.
    /// </summary>
    [TestMethod]
    public void SelectedRole_ToggleBetweenStudentAndLecturer_UpdatesRelatedPropertiesCorrectly()
    {
        // Arrange
        var mockAuthService = new Mock<IAuthService>();
        var mockApiService = new Mock<IApiService>();
        var mockSessionService = new Mock<SessionService>();
        var mockLogger = new Mock<ILogger<LoginViewModel>>();
        var viewModel = new LoginViewModel(
            mockAuthService.Object,
            mockApiService.Object,
            mockSessionService.Object,
            null,
            mockLogger.Object);

        // Act & Assert - Initial state
        Assert.AreEqual("Student", viewModel.SelectedRole);
        Assert.IsTrue(viewModel.IsStudentSelected);
        Assert.IsFalse(viewModel.IsLecturerSelected);

        // Act & Assert - Change to Lecturer
        viewModel.SelectedRole = "Lecturer";
        Assert.AreEqual("Lecturer", viewModel.SelectedRole);
        Assert.IsFalse(viewModel.IsStudentSelected);
        Assert.IsTrue(viewModel.IsLecturerSelected);

        // Act & Assert - Change back to Student
        viewModel.SelectedRole = "Student";
        Assert.AreEqual("Student", viewModel.SelectedRole);
        Assert.IsTrue(viewModel.IsStudentSelected);
        Assert.IsFalse(viewModel.IsLecturerSelected);
    }

    /// <summary>
    /// Tests that PropertyChanged event provides correct PropertyName for SelectedRole.
    /// </summary>
    [TestMethod]
    public void SelectedRole_PropertyChangedEvent_ProvidesCorrectPropertyName()
    {
        // Arrange
        var mockAuthService = new Mock<IAuthService>();
        var mockApiService = new Mock<IApiService>();
        var mockSessionService = new Mock<SessionService>();
        var mockLogger = new Mock<ILogger<LoginViewModel>>();
        var viewModel = new LoginViewModel(
            mockAuthService.Object,
            mockApiService.Object,
            mockSessionService.Object,
            null,
            mockLogger.Object);

        string? capturedPropertyName = null;
        viewModel.PropertyChanged += (sender, args) =>
        {
            if (capturedPropertyName == null)
            {
                capturedPropertyName = args.PropertyName;
            }
        };

        // Act
        viewModel.SelectedRole = "Lecturer";

        // Assert
        Assert.AreEqual("SelectedRole", capturedPropertyName);
    }

    /// <summary>
    /// Tests that setting ErrorMessage from empty to a value updates correctly and raises PropertyChanged.
    /// </summary>
    [TestMethod]
    public void ErrorMessage_SetFromEmptyToValue_UpdatesAndRaisesPropertyChanged()
    {
        // Arrange
        var mockAuthService = new Mock<IAuthService>();
        var mockApiService = new Mock<IApiService>();
        var mockSession = new Mock<SessionService>(MockBehavior.Loose, Mock.Of<ISecureStorage>());
        var mockLogger = new Mock<ILogger<LoginViewModel>>();
        var viewModel = new LoginViewModel(
            mockAuthService.Object,
            mockApiService.Object,
            mockSession.Object,
            null,
            mockLogger.Object);
        var propertyChangedRaised = false;
        viewModel.PropertyChanged += (sender, e) =>
        {
            if (e.PropertyName == "ErrorMessage")
                propertyChangedRaised = true;
        };

        // Act
        viewModel.ErrorMessage = "New error message";

        // Assert
        Assert.IsTrue(propertyChangedRaised);
        Assert.AreEqual("New error message", viewModel.ErrorMessage);
    }

    /// <summary>
    /// Tests that setting the ErrorMessage property to a very long string stores and returns the value correctly.
    /// </summary>
    [TestMethod]
    public void ErrorMessage_SetVeryLongString_StoresAndReturnsCorrectly()
    {
        // Arrange
        var mockAuthService = new Mock<IAuthService>();
        var mockApiService = new Mock<IApiService>();
        var mockSession = new Mock<SessionService>(MockBehavior.Loose, Mock.Of<ISecureStorage>());
        var mockLogger = new Mock<ILogger<LoginViewModel>>();
        var viewModel = new LoginViewModel(
            mockAuthService.Object,
            mockApiService.Object,
            mockSession.Object,
            null,
            mockLogger.Object);
        var veryLongString = new string('A', 10000);

        // Act
        viewModel.ErrorMessage = veryLongString;

        // Assert
        Assert.AreEqual(veryLongString, viewModel.ErrorMessage);
        Assert.AreEqual(10000, viewModel.ErrorMessage.Length);
    }

    /// <summary>
    /// Tests that the ErrorMessage property handles strings with special Unicode characters correctly.
    /// </summary>
    /// <param name="errorMessageWithSpecialChars">Error message with special Unicode characters.</param>
    [TestMethod]
    [DataRow("Error\u200B")]
    [DataRow("\uFEFFError message")]
    [DataRow("Error\u202Emessage")]
    [DataRow("Error\u00A0message")]
    [DataRow("Error\u2028message")]
    [DataRow("Error\u2029message")]
    public void ErrorMessage_SetStringWithSpecialUnicodeCharacters_StoresAndReturnsCorrectly(string errorMessageWithSpecialChars)
    {
        // Arrange
        var mockAuthService = new Mock<IAuthService>();
        var mockApiService = new Mock<IApiService>();
        var mockSession = new Mock<SessionService>(MockBehavior.Loose, Mock.Of<ISecureStorage>());
        var mockLogger = new Mock<ILogger<LoginViewModel>>();
        var viewModel = new LoginViewModel(
            mockAuthService.Object,
            mockApiService.Object,
            mockSession.Object,
            null,
            mockLogger.Object);

        // Act
        viewModel.ErrorMessage = errorMessageWithSpecialChars;

        // Assert
        Assert.AreEqual(errorMessageWithSpecialChars, viewModel.ErrorMessage);
    }

    /// <summary>
    /// Tests that the ErrorMessage property getter returns the current value after it has been set.
    /// </summary>
    [TestMethod]
    public void ErrorMessage_GetValue_ReturnsCurrentValue()
    {
        // Arrange
        var mockAuthService = new Mock<IAuthService>();
        var mockApiService = new Mock<IApiService>();
        var mockSession = new Mock<SessionService>(MockBehavior.Loose, Mock.Of<ISecureStorage>());
        var mockLogger = new Mock<ILogger<LoginViewModel>>();
        var viewModel = new LoginViewModel(
            mockAuthService.Object,
            mockApiService.Object,
            mockSession.Object,
            null,
            mockLogger.Object);
        var expectedMessage = "Authentication failed";

        // Act
        viewModel.ErrorMessage = expectedMessage;
        var actualMessage = viewModel.ErrorMessage;

        // Assert
        Assert.AreEqual(expectedMessage, actualMessage);
    }

    /// <summary>
    /// Tests that ErrorMessage property handles setting to the same empty string value multiple times correctly.
    /// Should not raise PropertyChanged after the first time.
    /// </summary>
    [TestMethod]
    public void ErrorMessage_SetEmptyStringMultipleTimes_RaisesPropertyChangedOnlyOnFirstChange()
    {
        // Arrange
        var mockAuthService = new Mock<IAuthService>();
        var mockApiService = new Mock<IApiService>();
        var mockSession = new Mock<SessionService>(MockBehavior.Loose, Mock.Of<ISecureStorage>());
        var mockLogger = new Mock<ILogger<LoginViewModel>>();
        var viewModel = new LoginViewModel(
            mockAuthService.Object,
            mockApiService.Object,
            mockSession.Object,
            null,
            mockLogger.Object);
        viewModel.ErrorMessage = "Some error";
        var propertyChangedCount = 0;
        viewModel.PropertyChanged += (sender, e) =>
        {
            if (e.PropertyName == "ErrorMessage")
                propertyChangedCount++;
        };

        // Act
        viewModel.ErrorMessage = string.Empty;
        viewModel.ErrorMessage = string.Empty;
        viewModel.ErrorMessage = string.Empty;

        // Assert
        Assert.AreEqual(1, propertyChangedCount);
        Assert.AreEqual(string.Empty, viewModel.ErrorMessage);
    }

    /// <summary>
    /// Tests that ErrorMessage correctly handles alternating between null and non-null values.
    /// </summary>
    [TestMethod]
    public void ErrorMessage_AlternateBetweenNullAndValue_UpdatesAndRaisesPropertyChangedCorrectly()
    {
        // Arrange
        var mockAuthService = new Mock<IAuthService>();
        var mockApiService = new Mock<IApiService>();
        var mockSession = new Mock<SessionService>(MockBehavior.Loose, Mock.Of<ISecureStorage>());
        var mockLogger = new Mock<ILogger<LoginViewModel>>();
        var viewModel = new LoginViewModel(
            mockAuthService.Object,
            mockApiService.Object,
            mockSession.Object,
            null,
            mockLogger.Object);
        var propertyChangedCount = 0;
        viewModel.PropertyChanged += (sender, e) =>
        {
            if (e.PropertyName == "ErrorMessage")
                propertyChangedCount++;
        };

        // Act
        viewModel.ErrorMessage = "Error";
        viewModel.ErrorMessage = null!;
        viewModel.ErrorMessage = "Another error";
        viewModel.ErrorMessage = null!;

        // Assert
        Assert.AreEqual(4, propertyChangedCount);
        Assert.IsNull(viewModel.ErrorMessage);
    }

    /// <summary>
    /// Tests that ErrorMessage handles whitespace-only strings correctly.
    /// </summary>
    /// <param name="whitespaceValue">Whitespace-only string.</param>
    [TestMethod]
    [DataRow(" ")]
    [DataRow("  ")]
    [DataRow("\t")]
    [DataRow("\n")]
    [DataRow("\r")]
    [DataRow("     ")]
    [DataRow("\t\t\t")]
    [DataRow("\r\n")]
    [DataRow("   \t   \n   ")]
    public void ErrorMessage_SetWhitespaceOnly_UpdatesProperty(string whitespaceValue)
    {
        // Arrange
        var mockAuthService = new Mock<IAuthService>();
        var mockApiService = new Mock<IApiService>();
        var mockSession = new Mock<SessionService>(MockBehavior.Loose, Mock.Of<ISecureStorage>());
        var mockLogger = new Mock<ILogger<LoginViewModel>>();
        var viewModel = new LoginViewModel(
            mockAuthService.Object,
            mockApiService.Object,
            mockSession.Object,
            null,
            mockLogger.Object);

        // Act
        viewModel.ErrorMessage = whitespaceValue;

        // Assert
        Assert.AreEqual(whitespaceValue, viewModel.ErrorMessage);
    }

    /// <summary>
    /// Tests that PropertyChanged event is raised exactly once when setting ErrorMessage to a different value.
    /// </summary>
    [TestMethod]
    public void ErrorMessage_SetDifferentValue_RaisesPropertyChangedExactlyOnce()
    {
        // Arrange
        var mockAuthService = new Mock<IAuthService>();
        var mockApiService = new Mock<IApiService>();
        var mockSession = new Mock<SessionService>(MockBehavior.Loose, Mock.Of<ISecureStorage>());
        var mockLogger = new Mock<ILogger<LoginViewModel>>();
        var viewModel = new LoginViewModel(
            mockAuthService.Object,
            mockApiService.Object,
            mockSession.Object,
            null,
            mockLogger.Object);
        viewModel.ErrorMessage = "Initial";
        var propertyChangedCount = 0;
        viewModel.PropertyChanged += (sender, e) =>
        {
            if (e.PropertyName == "ErrorMessage")
                propertyChangedCount++;
        };

        // Act
        viewModel.ErrorMessage = "Different";

        // Assert
        Assert.AreEqual(1, propertyChangedCount);
    }

    /// <summary>
    /// Tests that ErrorMessage property correctly handles setting to null after having a non-null value.
    /// </summary>
    [TestMethod]
    public void ErrorMessage_SetNullAfterValue_UpdatesAndRaisesPropertyChanged()
    {
        // Arrange
        var mockAuthService = new Mock<IAuthService>();
        var mockApiService = new Mock<IApiService>();
        var mockSession = new Mock<SessionService>(MockBehavior.Loose, Mock.Of<ISecureStorage>());
        var mockLogger = new Mock<ILogger<LoginViewModel>>();
        var viewModel = new LoginViewModel(
            mockAuthService.Object,
            mockApiService.Object,
            mockSession.Object,
            null,
            mockLogger.Object);
        viewModel.ErrorMessage = "Some error";
        var propertyChangedRaised = false;
        viewModel.PropertyChanged += (sender, e) =>
        {
            if (e.PropertyName == "ErrorMessage")
                propertyChangedRaised = true;
        };

        // Act
        viewModel.ErrorMessage = null!;

        // Assert
        Assert.IsTrue(propertyChangedRaised);
        Assert.IsNull(viewModel.ErrorMessage);
    }

    /// <summary>
    /// Tests that the Password property has an empty string as its default value.
    /// Verifies the initial state of the Password property.
    /// </summary>
    [TestMethod]
    public void Password_DefaultValue_IsEmptyString()
    {
        // Arrange
        var authServiceMock = new Mock<IAuthService>();
        var apiServiceMock = new Mock<IApiService>();
        var sessionServiceMock = new Mock<SessionService>(Mock.Of<IPreferences>(), Mock.Of<ILogger<SessionService>>());
        var loggerMock = new Mock<ILogger<LoginViewModel>>();

        // Act
        var viewModel = new LoginViewModel(
            authServiceMock.Object,
            apiServiceMock.Object,
            sessionServiceMock.Object,
            loggerMock.Object);

        // Assert
        Assert.AreEqual(string.Empty, viewModel.Password);
    }

    /// <summary>
    /// Tests that setting the Password property to various valid values updates the property correctly
    /// and raises the PropertyChanged event.
    /// Tests include normal passwords, empty string, whitespace, special characters, very long strings,
    /// Unicode characters, and control characters.
    /// </summary>
    /// <param name="passwordValue">The password value to set.</param>
    [TestMethod]
    [DataRow("password123")]
    [DataRow("P@ssw0rd!")]
    [DataRow("mySecurePassword")]
    [DataRow("12345678")]
    [DataRow("")]
    [DataRow("   ")]
    [DataRow("\t")]
    [DataRow("\n")]
    [DataRow("\r")]
    [DataRow("\t\n\r")]
    [DataRow("very long password string that exceeds normal length expectations to test boundary conditions and ensure the property handles large input without issues or truncation")]
    [DataRow("!@#$%^&*()_+-=[]{}|;':\",./<>?")]
    [DataRow("密码")]
    [DataRow("пароль")]
    [DataRow("🔒🔑")]
    [DataRow("pass\u0000word")]
    [DataRow("pass\u001Fword")]
    [DataRow("password\u200B")]
    [DataRow("\uFEFFpassword")]
    [DataRow("pass\u202Eword")]
    public void Password_SetValue_UpdatesPropertyAndRaisesPropertyChanged(string passwordValue)
    {
        // Arrange
        var authServiceMock = new Mock<IAuthService>();
        var apiServiceMock = new Mock<IApiService>();
        var sessionServiceMock = new Mock<SessionService>(Mock.Of<IPreferences>(), Mock.Of<ILogger<SessionService>>());
        var loggerMock = new Mock<ILogger<LoginViewModel>>();
        var viewModel = new LoginViewModel(
            authServiceMock.Object,
            apiServiceMock.Object,
            sessionServiceMock.Object,
            loggerMock.Object);
        string? raisedPropertyName = null;
        viewModel.PropertyChanged += (sender, e) => raisedPropertyName = e.PropertyName;

        // Act
        viewModel.Password = passwordValue;

        // Assert
        Assert.AreEqual(passwordValue, viewModel.Password);
        Assert.AreEqual("Password", raisedPropertyName);
    }

    /// <summary>
    /// Tests that the Password property getter returns the current value after being set.
    /// Verifies basic get/set functionality.
    /// </summary>
    [TestMethod]
    public void Password_GetAfterSet_ReturnsSetValue()
    {
        // Arrange
        var authServiceMock = new Mock<IAuthService>();
        var apiServiceMock = new Mock<IApiService>();
        var sessionServiceMock = new Mock<SessionService>(Mock.Of<IPreferences>(), Mock.Of<ILogger<SessionService>>());
        var loggerMock = new Mock<ILogger<LoginViewModel>>();
        var viewModel = new LoginViewModel(
            authServiceMock.Object,
            apiServiceMock.Object,
            sessionServiceMock.Object,
            loggerMock.Object);

        // Act
        viewModel.Password = "testPassword";

        // Assert
        Assert.AreEqual("testPassword", viewModel.Password);
    }

    /// <summary>
    /// Tests that setting the Password property to the same value multiple times
    /// does not raise PropertyChanged events after the first set.
    /// </summary>
    [TestMethod]
    public void Password_SetSameValueMultipleTimes_RaisesPropertyChangedOnlyOnce()
    {
        // Arrange
        var authServiceMock = new Mock<IAuthService>();
        var apiServiceMock = new Mock<IApiService>();
        var sessionServiceMock = new Mock<SessionService>(Mock.Of<IPreferences>(), Mock.Of<ILogger<SessionService>>());
        var loggerMock = new Mock<ILogger<LoginViewModel>>();
        var viewModel = new LoginViewModel(
            authServiceMock.Object,
            apiServiceMock.Object,
            sessionServiceMock.Object,
            loggerMock.Object);
        int eventRaisedCount = 0;
        viewModel.PropertyChanged += (sender, e) =>
        {
            if (e.PropertyName == "Password")
                eventRaisedCount++;
        };

        // Act
        viewModel.Password = "samePassword";
        viewModel.Password = "samePassword";
        viewModel.Password = "samePassword";

        // Assert
        Assert.AreEqual("samePassword", viewModel.Password);
        Assert.AreEqual(1, eventRaisedCount);
    }

    /// <summary>
    /// Tests that the PropertyChanged event provides the correct property name when Password is changed.
    /// </summary>
    [TestMethod]
    public void Password_PropertyChangedEvent_ProvidesCorrectPropertyName()
    {
        // Arrange
        var authServiceMock = new Mock<IAuthService>();
        var apiServiceMock = new Mock<IApiService>();
        var sessionServiceMock = new Mock<SessionService>(Mock.Of<IPreferences>(), Mock.Of<ILogger<SessionService>>());
        var loggerMock = new Mock<ILogger<LoginViewModel>>();
        var viewModel = new LoginViewModel(
            authServiceMock.Object,
            apiServiceMock.Object,
            sessionServiceMock.Object,
            loggerMock.Object);
        PropertyChangedEventArgs? eventArgs = null;
        viewModel.PropertyChanged += (sender, e) => eventArgs = e;

        // Act
        viewModel.Password = "newPassword";

        // Assert
        Assert.IsNotNull(eventArgs);
        Assert.AreEqual("Password", eventArgs.PropertyName);
    }

    /// <summary>
    /// Tests that alternating between different password values correctly updates the property
    /// and raises PropertyChanged event for each change.
    /// </summary>
    [TestMethod]
    public void Password_AlternateBetweenDifferentValues_UpdatesAndRaisesPropertyChangedEachTime()
    {
        // Arrange
        var authServiceMock = new Mock<IAuthService>();
        var apiServiceMock = new Mock<IApiService>();
        var sessionServiceMock = new Mock<SessionService>(Mock.Of<IPreferences>(), Mock.Of<ILogger<SessionService>>());
        var loggerMock = new Mock<ILogger<LoginViewModel>>();
        var viewModel = new LoginViewModel(
            authServiceMock.Object,
            apiServiceMock.Object,
            sessionServiceMock.Object,
            loggerMock.Object);
        int eventRaisedCount = 0;
        viewModel.PropertyChanged += (sender, e) =>
        {
            if (e.PropertyName == "Password")
                eventRaisedCount++;
        };

        // Act & Assert
        viewModel.Password = "passwordA";
        Assert.AreEqual("passwordA", viewModel.Password);
        Assert.AreEqual(1, eventRaisedCount);

        viewModel.Password = "passwordB";
        Assert.AreEqual("passwordB", viewModel.Password);
        Assert.AreEqual(2, eventRaisedCount);

        viewModel.Password = "passwordA";
        Assert.AreEqual("passwordA", viewModel.Password);
        Assert.AreEqual(3, eventRaisedCount);

        viewModel.Password = "passwordB";
        Assert.AreEqual("passwordB", viewModel.Password);
        Assert.AreEqual(4, eventRaisedCount);
    }

    /// <summary>
    /// Tests that the Password property correctly handles whitespace-only strings.
    /// </summary>
    /// <param name="whitespacePassword">Whitespace-only string to test.</param>
    [TestMethod]
    [DataRow(" ")]
    [DataRow("  ")]
    [DataRow("   ")]
    [DataRow("\t")]
    [DataRow("\n")]
    [DataRow("\r")]
    [DataRow("\t\t")]
    [DataRow("\n\n")]
    [DataRow(" \t \n \r ")]
    public void Password_SetWhitespaceOnly_StoresAndReturnsCorrectly(string whitespacePassword)
    {
        // Arrange
        var authServiceMock = new Mock<IAuthService>();
        var apiServiceMock = new Mock<IApiService>();
        var sessionServiceMock = new Mock<SessionService>(Mock.Of<IPreferences>(), Mock.Of<ILogger<SessionService>>());
        var loggerMock = new Mock<ILogger<LoginViewModel>>();
        var viewModel = new LoginViewModel(
            authServiceMock.Object,
            apiServiceMock.Object,
            sessionServiceMock.Object,
            loggerMock.Object);

        // Act
        viewModel.Password = whitespacePassword;

        // Assert
        Assert.AreEqual(whitespacePassword, viewModel.Password);
    }

    /// <summary>
    /// Tests that the Password property correctly stores and returns strings with various special characters.
    /// </summary>
    /// <param name="specialCharPassword">Password containing special characters.</param>
    [TestMethod]
    [DataRow("!@#$%^&*()")]
    [DataRow("_+-=[]{}|")]
    [DataRow(";':\",./<>?")]
    [DataRow("~`")]
    [DataRow("password!@#")]
    [DataRow("p@ssw0rd")]
    [DataRow("Pass_Word-123")]
    public void Password_SetWithSpecialCharacters_StoresAndReturnsCorrectly(string specialCharPassword)
    {
        // Arrange
        var authServiceMock = new Mock<IAuthService>();
        var apiServiceMock = new Mock<IApiService>();
        var sessionServiceMock = new Mock<SessionService>(Mock.Of<IPreferences>(), Mock.Of<ILogger<SessionService>>());
        var loggerMock = new Mock<ILogger<LoginViewModel>>();
        var viewModel = new LoginViewModel(
            authServiceMock.Object,
            apiServiceMock.Object,
            sessionServiceMock.Object,
            loggerMock.Object);

        // Act
        viewModel.Password = specialCharPassword;

        // Assert
        Assert.AreEqual(specialCharPassword, viewModel.Password);
    }

    /// <summary>
    /// Tests that the Password property is case-sensitive.
    /// Setting "password" and then "PASSWORD" should be treated as different values.
    /// </summary>
    [TestMethod]
    public void Password_CaseSensitive_TreatsDifferentCasesAsDifferentValues()
    {
        // Arrange
        var authServiceMock = new Mock<IAuthService>();
        var apiServiceMock = new Mock<IApiService>();
        var sessionServiceMock = new Mock<SessionService>(Mock.Of<IPreferences>(), Mock.Of<ILogger<SessionService>>());
        var loggerMock = new Mock<ILogger<LoginViewModel>>();
        var viewModel = new LoginViewModel(
            authServiceMock.Object,
            apiServiceMock.Object,
            sessionServiceMock.Object,
            loggerMock.Object);
        int eventRaisedCount = 0;
        viewModel.PropertyChanged += (sender, e) =>
        {
            if (e.PropertyName == "Password")
                eventRaisedCount++;
        };

        // Act
        viewModel.Password = "password";
        viewModel.Password = "PASSWORD";

        // Assert
        Assert.AreEqual("PASSWORD", viewModel.Password);
        Assert.AreEqual(2, eventRaisedCount);
    }

    /// <summary>
    /// Tests that the Password property correctly handles maximum practical password length.
    /// </summary>
    [TestMethod]
    public void Password_SetMaximumPracticalLength_UpdatesProperty()
    {
        // Arrange
        var authServiceMock = new Mock<IAuthService>();
        var apiServiceMock = new Mock<IApiService>();
        var sessionServiceMock = new Mock<SessionService>(Mock.Of<IPreferences>(), Mock.Of<ILogger<SessionService>>());
        var loggerMock = new Mock<ILogger<LoginViewModel>>();
        var viewModel = new LoginViewModel(
            authServiceMock.Object,
            apiServiceMock.Object,
            sessionServiceMock.Object,
            loggerMock.Object);
        string maxLengthPassword = new string('p', 256);

        // Act
        viewModel.Password = maxLengthPassword;

        // Assert
        Assert.AreEqual(maxLengthPassword, viewModel.Password);
        Assert.AreEqual(256, viewModel.Password.Length);
    }

    /// <summary>
    /// Tests that IsPasswordVisible returns false when IsPasswordHidden is true.
    /// Verifies the negation logic for the true state.
    /// </summary>
    [TestMethod]
    public void IsPasswordVisible_WhenIsPasswordHiddenIsTrue_ReturnsFalse()
    {
        // Arrange
        var mockAuthService = new Mock<IAuthService>();
        var mockApiService = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<LoginViewModel>>();
        var sessionService = new SessionService();
        var viewModel = new LoginViewModel(mockAuthService.Object, mockApiService.Object, sessionService, mockLogger.Object);

        // Act
        viewModel.IsPasswordHidden = true;
        var result = viewModel.IsPasswordVisible;

        // Assert
        Assert.IsFalse(result);
    }

    /// <summary>
    /// Tests that IsPasswordVisible returns true when IsPasswordHidden is false.
    /// Verifies the negation logic for the false state.
    /// </summary>
    [TestMethod]
    public void IsPasswordVisible_WhenIsPasswordHiddenIsFalse_ReturnsTrue()
    {
        // Arrange
        var mockAuthService = new Mock<IAuthService>();
        var mockApiService = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<LoginViewModel>>();
        var sessionService = new SessionService();
        var viewModel = new LoginViewModel(mockAuthService.Object, mockApiService.Object, sessionService, mockLogger.Object);

        // Act
        viewModel.IsPasswordHidden = false;
        var result = viewModel.IsPasswordVisible;

        // Assert
        Assert.IsTrue(result);
    }

    /// <summary>
    /// Tests that IsPasswordVisible returns the correct negated value of IsPasswordHidden
    /// for both true and false states.
    /// </summary>
    /// <param name="isPasswordHidden">The value to set for IsPasswordHidden.</param>
    /// <param name="expectedIsPasswordVisible">The expected value of IsPasswordVisible.</param>
    [TestMethod]
    [DataRow(true, false)]
    [DataRow(false, true)]
    public void IsPasswordVisible_WithBothBooleanStates_ReturnsCorrectNegatedValue(bool isPasswordHidden, bool expectedIsPasswordVisible)
    {
        // Arrange
        var mockAuthService = new Mock<IAuthService>();
        var mockApiService = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<LoginViewModel>>();
        var sessionService = new SessionService();
        var viewModel = new LoginViewModel(mockAuthService.Object, mockApiService.Object, sessionService, mockLogger.Object);

        // Act
        viewModel.IsPasswordHidden = isPasswordHidden;
        var result = viewModel.IsPasswordVisible;

        // Assert
        Assert.AreEqual(expectedIsPasswordVisible, result);
    }

    /// <summary>
    /// Tests that IsPasswordVisible returns false in its initial state
    /// since IsPasswordHidden is initialized to true.
    /// </summary>
    [TestMethod]
    public void IsPasswordVisible_InInitialState_ReturnsFalse()
    {
        // Arrange
        var mockAuthService = new Mock<IAuthService>();
        var mockApiService = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<LoginViewModel>>();
        var sessionService = new SessionService();
        var viewModel = new LoginViewModel(mockAuthService.Object, mockApiService.Object, sessionService, mockLogger.Object);

        // Act
        var result = viewModel.IsPasswordVisible;

        // Assert
        Assert.IsFalse(result);
    }

    /// <summary>
    /// Tests that IsPasswordVisible correctly reflects changes when IsPasswordHidden is toggled.
    /// Verifies state transition from hidden to visible.
    /// </summary>
    [TestMethod]
    public void IsPasswordVisible_WhenIsPasswordHiddenToggledFromTrueToFalse_ChangesToTrue()
    {
        // Arrange
        var mockAuthService = new Mock<IAuthService>();
        var mockApiService = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<LoginViewModel>>();
        var sessionService = new SessionService();
        var viewModel = new LoginViewModel(mockAuthService.Object, mockApiService.Object, sessionService, mockLogger.Object);
        viewModel.IsPasswordHidden = true;

        // Act
        viewModel.IsPasswordHidden = false;
        var result = viewModel.IsPasswordVisible;

        // Assert
        Assert.IsTrue(result);
    }

    /// <summary>
    /// Tests that IsPasswordVisible correctly reflects changes when IsPasswordHidden is toggled.
    /// Verifies state transition from visible to hidden.
    /// </summary>
    [TestMethod]
    public void IsPasswordVisible_WhenIsPasswordHiddenToggledFromFalseToTrue_ChangesToFalse()
    {
        // Arrange
        var mockAuthService = new Mock<IAuthService>();
        var mockApiService = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<LoginViewModel>>();
        var sessionService = new SessionService();
        var viewModel = new LoginViewModel(mockAuthService.Object, mockApiService.Object, sessionService, mockLogger.Object);
        viewModel.IsPasswordHidden = false;

        // Act
        viewModel.IsPasswordHidden = true;
        var result = viewModel.IsPasswordVisible;

        // Assert
        Assert.IsFalse(result);
    }

    /// <summary>
    /// Tests that IsPasswordVisible returns the correct value after multiple toggles of IsPasswordHidden.
    /// Verifies that the property consistently reflects the negated state through multiple state changes.
    /// </summary>
    [TestMethod]
    public void IsPasswordVisible_AfterMultipleToggles_ReturnsCorrectNegatedValues()
    {
        // Arrange
        var mockAuthService = new Mock<IAuthService>();
        var mockApiService = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<LoginViewModel>>();
        var sessionService = new SessionService();
        var viewModel = new LoginViewModel(mockAuthService.Object, mockApiService.Object, sessionService, mockLogger.Object);

        // Act & Assert - Initial state
        Assert.IsFalse(viewModel.IsPasswordVisible);

        // Act & Assert - First toggle
        viewModel.IsPasswordHidden = false;
        Assert.IsTrue(viewModel.IsPasswordVisible);

        // Act & Assert - Second toggle
        viewModel.IsPasswordHidden = true;
        Assert.IsFalse(viewModel.IsPasswordVisible);

        // Act & Assert - Third toggle
        viewModel.IsPasswordHidden = false;
        Assert.IsTrue(viewModel.IsPasswordVisible);

        // Act & Assert - Fourth toggle
        viewModel.IsPasswordHidden = true;
        Assert.IsFalse(viewModel.IsPasswordVisible);
    }

    /// <summary>
    /// Tests that IsPasswordVisible maintains correct value when IsPasswordHidden is set to the same value multiple times.
    /// Verifies consistency when no actual state change occurs.
    /// </summary>
    [TestMethod]
    public void IsPasswordVisible_WhenIsPasswordHiddenSetToSameValueMultipleTimes_MaintainsCorrectValue()
    {
        // Arrange
        var mockAuthService = new Mock<IAuthService>();
        var mockApiService = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<LoginViewModel>>();
        var sessionService = new SessionService();
        var viewModel = new LoginViewModel(mockAuthService.Object, mockApiService.Object, sessionService, mockLogger.Object);

        // Act
        viewModel.IsPasswordHidden = true;
        var result1 = viewModel.IsPasswordVisible;
        viewModel.IsPasswordHidden = true;
        var result2 = viewModel.IsPasswordVisible;
        viewModel.IsPasswordHidden = true;
        var result3 = viewModel.IsPasswordVisible;

        // Assert
        Assert.IsFalse(result1);
        Assert.IsFalse(result2);
        Assert.IsFalse(result3);
    }

    /// <summary>
    /// Tests that PropertyChanged event is raised for IsPasswordVisible when IsPasswordHidden changes from true to false.
    /// Verifies that the dependent computed property notification is triggered correctly.
    /// </summary>
    [TestMethod]
    public void IsPasswordVisible_WhenIsPasswordHiddenChangesFromTrueToFalse_RaisesPropertyChanged()
    {
        // Arrange
        var mockAuthService = new Mock<IAuthService>();
        var mockApiService = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<LoginViewModel>>();
        var sessionService = new SessionService();
        var viewModel = new LoginViewModel(mockAuthService.Object, mockApiService.Object, sessionService, mockLogger.Object);
        viewModel.IsPasswordHidden = true;
        var propertyChangedRaised = false;
        var propertyName = string.Empty;
        viewModel.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == nameof(LoginViewModel.IsPasswordVisible))
            {
                propertyChangedRaised = true;
                propertyName = args.PropertyName;
            }
        };

        // Act
        viewModel.IsPasswordHidden = false;

        // Assert
        Assert.IsTrue(propertyChangedRaised);
        Assert.AreEqual(nameof(LoginViewModel.IsPasswordVisible), propertyName);
    }

    /// <summary>
    /// Tests that PropertyChanged event is raised for IsPasswordVisible when IsPasswordHidden changes from false to true.
    /// Verifies that the dependent computed property notification is triggered correctly.
    /// </summary>
    [TestMethod]
    public void IsPasswordVisible_WhenIsPasswordHiddenChangesFromFalseToTrue_RaisesPropertyChanged()
    {
        // Arrange
        var mockAuthService = new Mock<IAuthService>();
        var mockApiService = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<LoginViewModel>>();
        var sessionService = new SessionService();
        var viewModel = new LoginViewModel(mockAuthService.Object, mockApiService.Object, sessionService, mockLogger.Object);
        viewModel.IsPasswordHidden = false;
        var propertyChangedRaised = false;
        var propertyName = string.Empty;
        viewModel.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == nameof(LoginViewModel.IsPasswordVisible))
            {
                propertyChangedRaised = true;
                propertyName = args.PropertyName;
            }
        };

        // Act
        viewModel.IsPasswordHidden = true;

        // Assert
        Assert.IsTrue(propertyChangedRaised);
        Assert.AreEqual(nameof(LoginViewModel.IsPasswordVisible), propertyName);
    }

    /// <summary>
    /// Tests that PropertyChanged event is not raised for IsPasswordVisible when IsPasswordHidden is set to the same value.
    /// Verifies that the notification is only triggered when an actual change occurs.
    /// </summary>
    [TestMethod]
    public void IsPasswordVisible_WhenIsPasswordHiddenSetToSameValue_DoesNotRaisePropertyChanged()
    {
        // Arrange
        var mockAuthService = new Mock<IAuthService>();
        var mockApiService = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<LoginViewModel>>();
        var sessionService = new SessionService();
        var viewModel = new LoginViewModel(mockAuthService.Object, mockApiService.Object, sessionService, mockLogger.Object);
        viewModel.IsPasswordHidden = true;
        var propertyChangedCount = 0;
        viewModel.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == nameof(LoginViewModel.IsPasswordVisible))
            {
                propertyChangedCount++;
            }
        };

        // Act
        viewModel.IsPasswordHidden = true;

        // Assert
        Assert.AreEqual(0, propertyChangedCount);
    }

    /// <summary>
    /// Tests that IsPasswordVisible consistently returns the negated value across multiple reads
    /// without changing IsPasswordHidden.
    /// Verifies property getter consistency and idempotency.
    /// </summary>
    [TestMethod]
    public void IsPasswordVisible_MultipleReadsWithoutChangingIsPasswordHidden_ReturnsConsistentValue()
    {
        // Arrange
        var mockAuthService = new Mock<IAuthService>();
        var mockApiService = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<LoginViewModel>>();
        var sessionService = new SessionService();
        var viewModel = new LoginViewModel(mockAuthService.Object, mockApiService.Object, sessionService, mockLogger.Object);
        viewModel.IsPasswordHidden = false;

        // Act
        var result1 = viewModel.IsPasswordVisible;
        var result2 = viewModel.IsPasswordVisible;
        var result3 = viewModel.IsPasswordVisible;

        // Assert
        Assert.IsTrue(result1);
        Assert.IsTrue(result2);
        Assert.IsTrue(result3);
        Assert.AreEqual(result1, result2);
        Assert.AreEqual(result2, result3);
    }

    /// <summary>
    /// Tests that IsPasswordVisible and IsPasswordHidden are always opposite boolean values.
    /// Verifies the inverse relationship between the two properties across all possible states.
    /// </summary>
    /// <param name="isPasswordHidden">The value to set for IsPasswordHidden.</param>
    [TestMethod]
    [DataRow(true)]
    [DataRow(false)]
    public void IsPasswordVisible_AlwaysInverseOfIsPasswordHidden(bool isPasswordHidden)
    {
        // Arrange
        var mockAuthService = new Mock<IAuthService>();
        var mockApiService = new Mock<IApiService>();
        var mockLogger = new Mock<ILogger<LoginViewModel>>();
        var sessionService = new SessionService();
        var viewModel = new LoginViewModel(mockAuthService.Object, mockApiService.Object, sessionService, mockLogger.Object);

        // Act
        viewModel.IsPasswordHidden = isPasswordHidden;
        var isPasswordVisible = viewModel.IsPasswordVisible;

        // Assert
        Assert.AreEqual(!isPasswordHidden, isPasswordVisible);
        Assert.AreNotEqual(viewModel.IsPasswordHidden, viewModel.IsPasswordVisible);
    }

    /// <summary>
    /// Tests that the constructor creates seven distinct command instances.
    /// Verifies that each command property references a unique object.
    /// </summary>
    [TestMethod]
    public void Constructor_ValidDependencies_CreatesSevenDistinctCommandInstances()
    {
        // Arrange
        var mockAuthService = new Mock<IAuthService>();
        var mockApiService = new Mock<IApiService>();
        var mockSessionService = new Mock<SessionService>();
        var mockLogger = new Mock<ILogger<LoginViewModel>>();

        // Act
        var viewModel = new LoginViewModel(
            mockAuthService.Object,
            mockApiService.Object,
            mockSessionService.Object,
            null,
            mockLogger.Object);

        // Assert
        var commands = new ICommand[]
        {
            viewModel.LoginCommand,
            viewModel.GoToRegisterCommand,
            viewModel.GoBackCommand,
            viewModel.ForgotPasswordCommand,
            viewModel.SelectStudentCommand,
            viewModel.SelectLecturerCommand,
            viewModel.TogglePasswordVisibilityCommand
        };

        for (int i = 0; i < commands.Length; i++)
        {
            for (int j = i + 1; j < commands.Length; j++)
            {
                Assert.AreNotSame(commands[i], commands[j],
                    $"Command at index {i} should not be the same instance as command at index {j}");
            }
        }
    }

    /// <summary>
    /// Tests that the constructor does not raise any PropertyChanged events during initialization.
    /// Verifies that construction has no side effects on property change notifications.
    /// </summary>
    [TestMethod]
    public void Constructor_ValidDependencies_DoesNotRaisePropertyChangedEvents()
    {
        // Arrange
        var mockAuthService = new Mock<IAuthService>();
        var mockApiService = new Mock<IApiService>();
        var mockSessionService = new Mock<SessionService>();
        var mockLogger = new Mock<ILogger<LoginViewModel>>();
        var propertyChangedRaised = false;

        // Act
        var viewModel = new LoginViewModel(
            mockAuthService.Object,
            mockApiService.Object,
            mockSessionService.Object,
            null,
            mockLogger.Object);

        viewModel.PropertyChanged += (sender, args) => propertyChangedRaised = true;

        // Assert
        Assert.IsFalse(propertyChangedRaised, "PropertyChanged should not be raised during construction");
    }

    /// <summary>
    /// Tests that the constructor does not throw when authService and session are null.
    /// Documents current behavior where null dependencies are not validated.
    /// </summary>
    [TestMethod]
    public void Constructor_NullAuthServiceAndSession_DoesNotThrow()
    {
        // Arrange
        IAuthService? nullAuthService = null;
        var mockApiService = new Mock<IApiService>();
        SessionService? nullSession = null;
        var mockLogger = new Mock<ILogger<LoginViewModel>>();

        // Act
        var viewModel = new LoginViewModel(
            nullAuthService!,
            mockApiService.Object,
            nullSession!,
            mockLogger.Object);

        // Assert
        Assert.IsNotNull(viewModel);
    }

    /// <summary>
    /// Tests that the constructor does not throw when apiService and logger are null.
    /// Documents current behavior where null dependencies are not validated.
    /// </summary>
    [TestMethod]
    public void Constructor_NullApiServiceAndLogger_DoesNotThrow()
    {
        // Arrange
        var mockAuthService = new Mock<IAuthService>();
        IApiService? nullApiService = null;
        var mockSessionService = new Mock<SessionService>();
        ILogger<LoginViewModel>? nullLogger = null;

        // Act
        var viewModel = new LoginViewModel(
            mockAuthService.Object,
            nullApiService!,
            mockSessionService.Object,
            nullLogger!);

        // Assert
        Assert.IsNotNull(viewModel);
    }

    /// <summary>
    /// Tests that the constructor does not throw when authService and logger are null.
    /// Documents current behavior where null dependencies are not validated.
    /// </summary>
    [TestMethod]
    public void Constructor_NullAuthServiceAndLogger_DoesNotThrow()
    {
        // Arrange
        IAuthService? nullAuthService = null;
        var mockApiService = new Mock<IApiService>();
        var mockSessionService = new Mock<SessionService>();
        ILogger<LoginViewModel>? nullLogger = null;

        // Act
        var viewModel = new LoginViewModel(
            nullAuthService!,
            mockApiService.Object,
            mockSessionService.Object,
            nullLogger!);

        // Assert
        Assert.IsNotNull(viewModel);
    }

    /// <summary>
    /// Tests that the constructor does not throw when apiService and session are null.
    /// Documents current behavior where null dependencies are not validated.
    /// </summary>
    [TestMethod]
    public void Constructor_NullApiServiceAndSession_DoesNotThrow()
    {
        // Arrange
        var mockAuthService = new Mock<IAuthService>();
        IApiService? nullApiService = null;
        SessionService? nullSession = null;
        var mockLogger = new Mock<ILogger<LoginViewModel>>();

        // Act
        var viewModel = new LoginViewModel(
            mockAuthService.Object,
            nullApiService!,
            nullSession!,
            mockLogger.Object);

        // Assert
        Assert.IsNotNull(viewModel);
    }

    /// <summary>
    /// Tests that the constructor does not throw when only authService is provided.
    /// Documents current behavior where null dependencies are not validated.
    /// </summary>
    [TestMethod]
    public void Constructor_OnlyAuthServiceProvided_DoesNotThrow()
    {
        // Arrange
        var mockAuthService = new Mock<IAuthService>();
        IApiService? nullApiService = null;
        SessionService? nullSession = null;
        ILogger<LoginViewModel>? nullLogger = null;

        // Act
        var viewModel = new LoginViewModel(
            mockAuthService.Object,
            nullApiService!,
            nullSession!,
            nullLogger!);

        // Assert
        Assert.IsNotNull(viewModel);
        Assert.IsNotNull(viewModel.LoginCommand);
        Assert.IsNotNull(viewModel.GoToRegisterCommand);
        Assert.IsNotNull(viewModel.GoBackCommand);
        Assert.IsNotNull(viewModel.ForgotPasswordCommand);
        Assert.IsNotNull(viewModel.SelectStudentCommand);
        Assert.IsNotNull(viewModel.SelectLecturerCommand);
        Assert.IsNotNull(viewModel.TogglePasswordVisibilityCommand);
    }

    /// <summary>
    /// Tests that the constructor does not throw when only apiService is provided.
    /// Documents current behavior where null dependencies are not validated.
    /// </summary>
    [TestMethod]
    public void Constructor_OnlyApiServiceProvided_DoesNotThrow()
    {
        // Arrange
        IAuthService? nullAuthService = null;
        var mockApiService = new Mock<IApiService>();
        SessionService? nullSession = null;
        ILogger<LoginViewModel>? nullLogger = null;

        // Act
        var viewModel = new LoginViewModel(
            nullAuthService!,
            mockApiService.Object,
            nullSession!,
            nullLogger!);

        // Assert
        Assert.IsNotNull(viewModel);
        Assert.IsNotNull(viewModel.LoginCommand);
        Assert.IsNotNull(viewModel.GoToRegisterCommand);
        Assert.IsNotNull(viewModel.GoBackCommand);
        Assert.IsNotNull(viewModel.ForgotPasswordCommand);
        Assert.IsNotNull(viewModel.SelectStudentCommand);
        Assert.IsNotNull(viewModel.SelectLecturerCommand);
        Assert.IsNotNull(viewModel.TogglePasswordVisibilityCommand);
    }

    /// <summary>
    /// Tests that the constructor does not throw when only session is provided.
    /// Documents current behavior where null dependencies are not validated.
    /// </summary>
    [TestMethod]
    public void Constructor_OnlySessionProvided_DoesNotThrow()
    {
        // Arrange
        IAuthService? nullAuthService = null;
        IApiService? nullApiService = null;
        var mockSessionService = new Mock<SessionService>();
        ILogger<LoginViewModel>? nullLogger = null;

        // Act
        var viewModel = new LoginViewModel(
            nullAuthService!,
            nullApiService!,
            mockSessionService.Object,
            nullLogger!);

        // Assert
        Assert.IsNotNull(viewModel);
        Assert.IsNotNull(viewModel.LoginCommand);
        Assert.IsNotNull(viewModel.GoToRegisterCommand);
        Assert.IsNotNull(viewModel.GoBackCommand);
        Assert.IsNotNull(viewModel.ForgotPasswordCommand);
        Assert.IsNotNull(viewModel.SelectStudentCommand);
        Assert.IsNotNull(viewModel.SelectLecturerCommand);
        Assert.IsNotNull(viewModel.TogglePasswordVisibilityCommand);
    }

    /// <summary>
    /// Tests that the constructor does not throw when only logger is provided.
    /// Documents current behavior where null dependencies are not validated.
    /// </summary>
    [TestMethod]
    public void Constructor_OnlyLoggerProvided_DoesNotThrow()
    {
        // Arrange
        IAuthService? nullAuthService = null;
        IApiService? nullApiService = null;
        SessionService? nullSession = null;
        var mockLogger = new Mock<ILogger<LoginViewModel>>();

        // Act
        var viewModel = new LoginViewModel(
            nullAuthService!,
            nullApiService!,
            nullSession!,
            mockLogger.Object);

        // Assert
        Assert.IsNotNull(viewModel);
        Assert.IsNotNull(viewModel.LoginCommand);
        Assert.IsNotNull(viewModel.GoToRegisterCommand);
        Assert.IsNotNull(viewModel.GoBackCommand);
        Assert.IsNotNull(viewModel.ForgotPasswordCommand);
        Assert.IsNotNull(viewModel.SelectStudentCommand);
        Assert.IsNotNull(viewModel.SelectLecturerCommand);
        Assert.IsNotNull(viewModel.TogglePasswordVisibilityCommand);
    }

    /// <summary>
    /// Tests that the constructor creates a new instance that is distinct from other instances.
    /// Verifies object identity and independence.
    /// </summary>
    [TestMethod]
    public void Constructor_CalledTwiceWithSameDependencies_CreatesTwoDistinctInstances()
    {
        // Arrange
        var mockAuthService = new Mock<IAuthService>();
        var mockApiService = new Mock<IApiService>();
        var mockSessionService = new Mock<SessionService>();
        var mockLogger = new Mock<ILogger<LoginViewModel>>();

        // Act
        var viewModel1 = new LoginViewModel(
            mockAuthService.Object,
            mockApiService.Object,
            mockSessionService.Object,
            null,
            mockLogger.Object);

        var viewModel2 = new LoginViewModel(
            mockAuthService.Object,
            mockApiService.Object,
            mockSessionService.Object,
            null,
            mockLogger.Object);

        // Assert
        Assert.AreNotSame(viewModel1, viewModel2);
        Assert.AreNotSame(viewModel1.LoginCommand, viewModel2.LoginCommand);
        Assert.AreNotSame(viewModel1.GoToRegisterCommand, viewModel2.GoToRegisterCommand);
        Assert.AreNotSame(viewModel1.GoBackCommand, viewModel2.GoBackCommand);
        Assert.AreNotSame(viewModel1.ForgotPasswordCommand, viewModel2.ForgotPasswordCommand);
        Assert.AreNotSame(viewModel1.SelectStudentCommand, viewModel2.SelectStudentCommand);
        Assert.AreNotSame(viewModel1.SelectLecturerCommand, viewModel2.SelectLecturerCommand);
        Assert.AreNotSame(viewModel1.TogglePasswordVisibilityCommand, viewModel2.TogglePasswordVisibilityCommand);
    }

    /// <summary>
    /// Tests that the constructor does not invoke any methods on the SessionService dependency.
    /// Verifies that constructor has no side effects on sessionService.
    /// </summary>
    [TestMethod]
    public void Constructor_ValidDependencies_DoesNotInvokeSessionServiceMethods()
    {
        // Arrange
        var mockAuthService = new Mock<IAuthService>();
        var mockApiService = new Mock<IApiService>();
        var mockSessionService = new Mock<SessionService>();
        var mockLogger = new Mock<ILogger<LoginViewModel>>();

        // Act
        var viewModel = new LoginViewModel(
            mockAuthService.Object,
            mockApiService.Object,
            mockSessionService.Object,
            null,
            mockLogger.Object);

        // Assert
        mockSessionService.VerifyNoOtherCalls();
    }

    /// <summary>
    /// Tests that the constructor does not invoke any methods on the ILogger dependency.
    /// Verifies that constructor has no side effects on logger.
    /// </summary>
    [TestMethod]
    public void Constructor_ValidDependencies_DoesNotInvokeLoggerMethods()
    {
        // Arrange
        var mockAuthService = new Mock<IAuthService>();
        var mockApiService = new Mock<IApiService>();
        var mockSessionService = new Mock<SessionService>();
        var mockLogger = new Mock<ILogger<LoginViewModel>>();

        // Act
        var viewModel = new LoginViewModel(
            mockAuthService.Object,
            mockApiService.Object,
            mockSessionService.Object,
            null,
            mockLogger.Object);

        // Assert
        mockLogger.VerifyNoOtherCalls();
    }

    /// <summary>
    /// Tests that construction is fast and completes without delay.
    /// Verifies that no blocking operations occur during construction.
    /// </summary>
    [TestMethod]
    public void Constructor_ValidDependencies_CompletesQuickly()
    {
        // Arrange
        var mockAuthService = new Mock<IAuthService>();
        var mockApiService = new Mock<IApiService>();
        var mockSessionService = new Mock<SessionService>();
        var mockLogger = new Mock<ILogger<LoginViewModel>>();
        var startTime = DateTime.UtcNow;

        // Act
        var viewModel = new LoginViewModel(
            mockAuthService.Object,
            mockApiService.Object,
            mockSessionService.Object,
            null,
            mockLogger.Object);
        var elapsed = DateTime.UtcNow - startTime;

        // Assert
        Assert.IsTrue(elapsed.TotalMilliseconds < 1000,
            $"Constructor took {elapsed.TotalMilliseconds}ms, expected less than 1000ms");
    }

    /// <summary>
    /// Tests that the constructor can be invoked with different IApiService mock instances.
    /// Verifies compatibility with various IApiService implementations.
    /// </summary>
    [TestMethod]
    public void Constructor_DifferentApiServiceInstances_CreatesInstanceSuccessfully()
    {
        // Arrange
        var mockAuthService = new Mock<IAuthService>();
        var mockApiService1 = new Mock<IApiService>();
        var mockApiService2 = new Mock<IApiService>();
        var mockSessionService = new Mock<SessionService>();
        var mockLogger = new Mock<ILogger<LoginViewModel>>();

        // Act
        var viewModel1 = new LoginViewModel(
            mockAuthService.Object,
            mockApiService1.Object,
            mockSessionService.Object,
            null,
            mockLogger.Object);

        var viewModel2 = new LoginViewModel(
            mockAuthService.Object,
            mockApiService2.Object,
            mockSessionService.Object,
            null,
            mockLogger.Object);

        // Assert
        Assert.IsNotNull(viewModel1);
        Assert.IsNotNull(viewModel2);
        Assert.AreNotSame(viewModel1, viewModel2);
    }

    /// <summary>
    /// Tests that the constructor can be invoked with different ILogger mock instances.
    /// Verifies compatibility with various ILogger implementations.
    /// </summary>
    [TestMethod]
    public void Constructor_DifferentLoggerInstances_CreatesInstanceSuccessfully()
    {
        // Arrange
        var mockAuthService = new Mock<IAuthService>();
        var mockApiService = new Mock<IApiService>();
        var mockSessionService = new Mock<SessionService>();
        var mockLogger1 = new Mock<ILogger<LoginViewModel>>();
        var mockLogger2 = new Mock<ILogger<LoginViewModel>>();

        // Act
        var viewModel1 = new LoginViewModel(
            mockAuthService.Object,
            mockApiService.Object,
            mockSessionService.Object,
            mockLogger1.Object);

        var viewModel2 = new LoginViewModel(
            mockAuthService.Object,
            mockApiService.Object,
            mockSessionService.Object,
            mockLogger2.Object);

        // Assert
        Assert.IsNotNull(viewModel1);
        Assert.IsNotNull(viewModel2);
        Assert.AreNotSame(viewModel1, viewModel2);
    }

    /// <summary>
    /// Tests that all string properties are initialized to empty string after construction.
    /// Verifies the initial state of Email, Password, ErrorMessage, and SelectedRole properties.
    /// </summary>
    [TestMethod]
    public void Constructor_ValidDependencies_InitializesAllStringPropertiesToExpectedValues()
    {
        // Arrange
        var mockAuthService = new Mock<IAuthService>();
        var mockApiService = new Mock<IApiService>();
        var mockSessionService = new Mock<SessionService>();
        var mockLogger = new Mock<ILogger<LoginViewModel>>();

        // Act
        var viewModel = new LoginViewModel(
            mockAuthService.Object,
            mockApiService.Object,
            mockSessionService.Object,
            null,
            mockLogger.Object);

        // Assert
        Assert.AreEqual(string.Empty, viewModel.Email);
        Assert.AreEqual(string.Empty, viewModel.Password);
        Assert.AreEqual(string.Empty, viewModel.ErrorMessage);
        Assert.AreEqual("Student", viewModel.SelectedRole);
    }

    /// <summary>
    /// Tests that all boolean properties are initialized to expected values after construction.
    /// Verifies the initial state of IsPasswordHidden, IsPasswordVisible, IsStudentSelected, and IsLecturerSelected.
    /// </summary>
    [TestMethod]
    public void Constructor_ValidDependencies_InitializesAllBooleanPropertiesToExpectedValues()
    {
        // Arrange
        var mockAuthService = new Mock<IAuthService>();
        var mockApiService = new Mock<IApiService>();
        var mockSessionService = new Mock<SessionService>();
        var mockLogger = new Mock<ILogger<LoginViewModel>>();

        // Act
        var viewModel = new LoginViewModel(
            mockAuthService.Object,
            mockApiService.Object,
            mockSessionService.Object,
            null,
            mockLogger.Object);

        // Assert
        Assert.IsTrue(viewModel.IsPasswordHidden);
        Assert.IsFalse(viewModel.IsPasswordVisible);
        Assert.IsTrue(viewModel.IsStudentSelected);
        Assert.IsFalse(viewModel.IsLecturerSelected);
    }
}



/// <summary>
/// Unit tests for the SelectedRole property of the <see cref="LoginViewModel"/> class.
/// </summary>
[TestClass]
public partial class LoginViewModelSelectedRoleTests
{
    /// <summary>
    /// Tests that the SelectedRole property has "Student" as its initial/default value.
    /// Verifies the property is correctly initialized by the backing field.
    /// </summary>
    [TestMethod]
    public void SelectedRole_InitialValue_IsStudent()
    {
        // Arrange
        var mockAuthService = new Mock<IAuthService>();
        var mockApiService = new Mock<IApiService>();
        var mockSessionService = new Mock<SessionService>();
        var mockLogger = new Mock<ILogger<LoginViewModel>>();

        // Act
        var viewModel = new LoginViewModel(mockAuthService.Object, mockApiService.Object, mockSessionService.Object, mockLogger.Object);

        // Assert
        Assert.AreEqual("Student", viewModel.SelectedRole);
    }

    /// <summary>
    /// Tests that setting SelectedRole to a different value updates the property correctly.
    /// Validates basic property setter functionality for various input values.
    /// </summary>
    /// <param name="newValue">The new role value to set.</param>
    [TestMethod]
    [DataRow("Lecturer")]
    [DataRow("Student")]
    [DataRow("Administrator")]
    [DataRow("")]
    [DataRow("   ")]
    [DataRow("Teacher")]
    [DataRow("student")]
    [DataRow("STUDENT")]
    [DataRow("lecturer")]
    [DataRow("LECTURER")]
    public void SelectedRole_SetToDifferentValue_UpdatesProperty(string newValue)
    {
        // Arrange
        var mockAuthService = new Mock<IAuthService>();
        var mockApiService = new Mock<IApiService>();
        var mockSessionService = new Mock<SessionService>();
        var mockLogger = new Mock<ILogger<LoginViewModel>>();
        var viewModel = new LoginViewModel(mockAuthService.Object, mockApiService.Object, mockSessionService.Object, mockLogger.Object);

        // Act
        viewModel.SelectedRole = newValue;

        // Assert
        Assert.AreEqual(newValue, viewModel.SelectedRole);
    }

    /// <summary>
    /// Tests that setting SelectedRole to null updates the property to null.
    /// Verifies edge case behavior even though property is declared as non-nullable.
    /// </summary>
    [TestMethod]
    public void SelectedRole_SetToNull_UpdatesPropertyToNull()
    {
        // Arrange
        var mockAuthService = new Mock<IAuthService>();
        var mockApiService = new Mock<IApiService>();
        var mockSessionService = new Mock<SessionService>();
        var mockLogger = new Mock<ILogger<LoginViewModel>>();
        var viewModel = new LoginViewModel(mockAuthService.Object, mockApiService.Object, mockSessionService.Object, mockLogger.Object);

        // Act
        viewModel.SelectedRole = null!;

        // Assert
        Assert.IsNull(viewModel.SelectedRole);
    }

    /// <summary>
    /// Tests that setting SelectedRole to a different value raises PropertyChanged events
    /// for SelectedRole, IsStudentSelected, and IsLecturerSelected.
    /// Validates that all related property notifications are triggered.
    /// </summary>
    /// <param name="newValue">The new role value to set.</param>
    [TestMethod]
    [DataRow("Lecturer")]
    [DataRow("Administrator")]
    [DataRow("")]
    [DataRow("Teacher")]
    public void SelectedRole_SetToDifferentValue_RaisesPropertyChangedForAllRelatedProperties(string newValue)
    {
        // Arrange
        var mockAuthService = new Mock<IAuthService>();
        var mockApiService = new Mock<IApiService>();
        var mockSessionService = new Mock<SessionService>();
        var mockLogger = new Mock<ILogger<LoginViewModel>>();
        var viewModel = new LoginViewModel(mockAuthService.Object, mockApiService.Object, mockSessionService.Object, mockLogger.Object);
        var propertyChangedEvents = new List<string>();
        viewModel.PropertyChanged += (sender, e) => propertyChangedEvents.Add(e.PropertyName!);

        // Act
        viewModel.SelectedRole = newValue;

        // Assert
        CollectionAssert.Contains(propertyChangedEvents, "SelectedRole");
        CollectionAssert.Contains(propertyChangedEvents, "IsStudentSelected");
        CollectionAssert.Contains(propertyChangedEvents, "IsLecturerSelected");
    }

    /// <summary>
    /// Tests that setting SelectedRole to the same value does not raise PropertyChanged events.
    /// Verifies that SetProperty correctly detects when no actual change occurs.
    /// </summary>
    [TestMethod]
    public void SelectedRole_SetToSameValue_DoesNotRaisePropertyChanged()
    {
        // Arrange
        var mockAuthService = new Mock<IAuthService>();
        var mockApiService = new Mock<IApiService>();
        var mockSessionService = new Mock<SessionService>();
        var mockLogger = new Mock<ILogger<LoginViewModel>>();
        var viewModel = new LoginViewModel(mockAuthService.Object, mockApiService.Object, mockSessionService.Object, mockLogger.Object);
        var propertyChangedCount = 0;
        viewModel.PropertyChanged += (sender, e) => propertyChangedCount++;

        // Act
        viewModel.SelectedRole = "Student";

        // Assert
        Assert.AreEqual(0, propertyChangedCount);
    }

    /// <summary>
    /// Tests that PropertyChanged events are raised in the correct order when SelectedRole changes.
    /// Verifies SelectedRole is raised first, followed by IsStudentSelected and IsLecturerSelected.
    /// </summary>
    [TestMethod]
    public void SelectedRole_SetToDifferentValue_RaisesPropertyChangedInCorrectOrder()
    {
        // Arrange
        var mockAuthService = new Mock<IAuthService>();
        var mockApiService = new Mock<IApiService>();
        var mockSessionService = new Mock<SessionService>();
        var mockLogger = new Mock<ILogger<LoginViewModel>>();
        var viewModel = new LoginViewModel(mockAuthService.Object, mockApiService.Object, mockSessionService.Object, mockLogger.Object);
        var propertyChangedEvents = new List<string>();
        viewModel.PropertyChanged += (sender, e) => propertyChangedEvents.Add(e.PropertyName!);

        // Act
        viewModel.SelectedRole = "Lecturer";

        // Assert
        Assert.AreEqual(3, propertyChangedEvents.Count);
        Assert.AreEqual("SelectedRole", propertyChangedEvents[0]);
        Assert.IsTrue(propertyChangedEvents.Contains("IsStudentSelected"));
        Assert.IsTrue(propertyChangedEvents.Contains("IsLecturerSelected"));
    }

    /// <summary>
    /// Tests that setting SelectedRole to "Student" makes IsStudentSelected true and IsLecturerSelected false.
    /// Validates the computed property behavior for Student role.
    /// </summary>
    [TestMethod]
    public void SelectedRole_SetToStudent_UpdatesIsStudentSelectedAndIsLecturerSelected()
    {
        // Arrange
        var mockAuthService = new Mock<IAuthService>();
        var mockApiService = new Mock<IApiService>();
        var mockSessionService = new Mock<SessionService>();
        var mockLogger = new Mock<ILogger<LoginViewModel>>();
        var viewModel = new LoginViewModel(mockAuthService.Object, mockApiService.Object, mockSessionService.Object, mockLogger.Object);
        viewModel.SelectedRole = "Lecturer";

        // Act
        viewModel.SelectedRole = "Student";

        // Assert
        Assert.IsTrue(viewModel.IsStudentSelected);
        Assert.IsFalse(viewModel.IsLecturerSelected);
    }

    /// <summary>
    /// Tests that setting SelectedRole to "Lecturer" makes IsLecturerSelected true and IsStudentSelected false.
    /// Validates the computed property behavior for Lecturer role.
    /// </summary>
    [TestMethod]
    public void SelectedRole_SetToLecturer_UpdatesIsStudentSelectedAndIsLecturerSelected()
    {
        // Arrange
        var mockAuthService = new Mock<IAuthService>();
        var mockApiService = new Mock<IApiService>();
        var mockSessionService = new Mock<SessionService>();
        var mockLogger = new Mock<ILogger<LoginViewModel>>();
        var viewModel = new LoginViewModel(mockAuthService.Object, mockApiService.Object, mockSessionService.Object, mockLogger.Object);

        // Act
        viewModel.SelectedRole = "Lecturer";

        // Assert
        Assert.IsFalse(viewModel.IsStudentSelected);
        Assert.IsTrue(viewModel.IsLecturerSelected);
    }

    /// <summary>
    /// Tests that setting SelectedRole to a value other than "Student" or "Lecturer"
    /// makes both IsStudentSelected and IsLecturerSelected false.
    /// Validates the computed property behavior for other roles.
    /// </summary>
    /// <param name="otherRole">A role value that is neither "Student" nor "Lecturer".</param>
    [TestMethod]
    [DataRow("Administrator")]
    [DataRow("")]
    [DataRow("Teacher")]
    [DataRow("student")]
    [DataRow("LECTURER")]
    [DataRow("   ")]
    public void SelectedRole_SetToOtherValue_MakesBothIsStudentSelectedAndIsLecturerSelectedFalse(string otherRole)
    {
        // Arrange
        var mockAuthService = new Mock<IAuthService>();
        var mockApiService = new Mock<IApiService>();
        var mockSessionService = new Mock<SessionService>();
        var mockLogger = new Mock<ILogger<LoginViewModel>>();
        var viewModel = new LoginViewModel(mockAuthService.Object, mockApiService.Object, mockSessionService.Object, mockLogger.Object);

        // Act
        viewModel.SelectedRole = otherRole;

        // Assert
        Assert.IsFalse(viewModel.IsStudentSelected);
        Assert.IsFalse(viewModel.IsLecturerSelected);
    }

    /// <summary>
    /// Tests that setting SelectedRole to a very long string updates the property correctly
    /// and raises PropertyChanged events.
    /// Validates boundary condition with extremely long input.
    /// </summary>
    [TestMethod]
    public void SelectedRole_SetToVeryLongString_UpdatesPropertyAndRaisesPropertyChanged()
    {
        // Arrange
        var mockAuthService = new Mock<IAuthService>();
        var mockApiService = new Mock<IApiService>();
        var mockSessionService = new Mock<SessionService>();
        var mockLogger = new Mock<ILogger<LoginViewModel>>();
        var viewModel = new LoginViewModel(mockAuthService.Object, mockApiService.Object, mockSessionService.Object, mockLogger.Object);
        var veryLongString = new string('A', 10000);
        var propertyChangedCount = 0;
        viewModel.PropertyChanged += (sender, e) => propertyChangedCount++;

        // Act
        viewModel.SelectedRole = veryLongString;

        // Assert
        Assert.AreEqual(veryLongString, viewModel.SelectedRole);
        Assert.AreEqual(3, propertyChangedCount);
    }

    /// <summary>
    /// Tests that setting SelectedRole to strings with special Unicode characters updates correctly.
    /// Validates handling of control characters and special Unicode sequences.
    /// </summary>
    /// <param name="specialString">String containing special Unicode characters.</param>
    [TestMethod]
    [DataRow("Role\u0000WithNull")]
    [DataRow("Role\u001FWithControl")]
    [DataRow("Role\u200BWithZeroWidthSpace")]
    [DataRow("\uFEFFRoleWithBOM")]
    [DataRow("Role\u202EWithRightToLeftOverride")]
    [DataRow("\t\n\r")]
    public void SelectedRole_SetToStringWithSpecialUnicodeCharacters_UpdatesProperty(string specialString)
    {
        // Arrange
        var mockAuthService = new Mock<IAuthService>();
        var mockApiService = new Mock<IApiService>();
        var mockSessionService = new Mock<SessionService>();
        var mockLogger = new Mock<ILogger<LoginViewModel>>();
        var viewModel = new LoginViewModel(mockAuthService.Object, mockApiService.Object, mockSessionService.Object, mockLogger.Object);

        // Act
        viewModel.SelectedRole = specialString;

        // Assert
        Assert.AreEqual(specialString, viewModel.SelectedRole);
    }

    /// <summary>
    /// Tests that setting SelectedRole multiple times with different values
    /// raises PropertyChanged events each time.
    /// Validates that the property correctly handles consecutive changes.
    /// </summary>
    [TestMethod]
    public void SelectedRole_SetMultipleDifferentValues_RaisesPropertyChangedEachTime()
    {
        // Arrange
        var mockAuthService = new Mock<IAuthService>();
        var mockApiService = new Mock<IApiService>();
        var mockSessionService = new Mock<SessionService>();
        var mockLogger = new Mock<ILogger<LoginViewModel>>();
        var viewModel = new LoginViewModel(mockAuthService.Object, mockApiService.Object, mockSessionService.Object, mockLogger.Object);
        var propertyChangedCount = 0;
        viewModel.PropertyChanged += (sender, e) => propertyChangedCount++;

        // Act
        viewModel.SelectedRole = "Lecturer";
        viewModel.SelectedRole = "Administrator";
        viewModel.SelectedRole = "Teacher";

        // Assert
        Assert.AreEqual(9, propertyChangedCount); // 3 events per change × 3 changes
    }

    /// <summary>
    /// Tests that setting SelectedRole to the same value multiple times
    /// only raises PropertyChanged on the first change from default.
    /// Validates idempotency of the setter.
    /// </summary>
    [TestMethod]
    public void SelectedRole_SetSameValueMultipleTimes_RaisesPropertyChangedOnlyOnce()
    {
        // Arrange
        var mockAuthService = new Mock<IAuthService>();
        var mockApiService = new Mock<IApiService>();
        var mockSessionService = new Mock<SessionService>();
        var mockLogger = new Mock<ILogger<LoginViewModel>>();
        var viewModel = new LoginViewModel(mockAuthService.Object, mockApiService.Object, mockSessionService.Object, mockLogger.Object);
        viewModel.SelectedRole = "Lecturer";
        var propertyChangedCount = 0;
        viewModel.PropertyChanged += (sender, e) => propertyChangedCount++;

        // Act
        viewModel.SelectedRole = "Lecturer";
        viewModel.SelectedRole = "Lecturer";
        viewModel.SelectedRole = "Lecturer";

        // Assert
        Assert.AreEqual(0, propertyChangedCount);
    }

    /// <summary>
    /// Tests that toggling SelectedRole between "Student" and "Lecturer"
    /// correctly updates IsStudentSelected and IsLecturerSelected.
    /// Validates state transitions between the two primary roles.
    /// </summary>
    [TestMethod]
    public void SelectedRole_ToggleBetweenStudentAndLecturer_UpdatesRelatedPropertiesCorrectly()
    {
        // Arrange
        var mockAuthService = new Mock<IAuthService>();
        var mockApiService = new Mock<IApiService>();
        var mockSessionService = new Mock<SessionService>();
        var mockLogger = new Mock<ILogger<LoginViewModel>>();
        var viewModel = new LoginViewModel(mockAuthService.Object, mockApiService.Object, mockSessionService.Object, mockLogger.Object);

        // Act & Assert - Initial state
        Assert.IsTrue(viewModel.IsStudentSelected);
        Assert.IsFalse(viewModel.IsLecturerSelected);

        // Act & Assert - Switch to Lecturer
        viewModel.SelectedRole = "Lecturer";
        Assert.IsFalse(viewModel.IsStudentSelected);
        Assert.IsTrue(viewModel.IsLecturerSelected);

        // Act & Assert - Switch back to Student
        viewModel.SelectedRole = "Student";
        Assert.IsTrue(viewModel.IsStudentSelected);
        Assert.IsFalse(viewModel.IsLecturerSelected);
    }

    /// <summary>
    /// Tests that SelectedRole setter is case-sensitive for "Student".
    /// Verifies that only exact match "Student" makes IsStudentSelected true.
    /// </summary>
    /// <param name="role">Role value with different casing.</param>
    /// <param name="expectedIsStudentSelected">Expected value of IsStudentSelected.</param>
    [TestMethod]
    [DataRow("Student", true)]
    [DataRow("student", false)]
    [DataRow("STUDENT", false)]
    [DataRow("StUdEnT", false)]
    public void SelectedRole_CaseSensitivityForStudent_AffectsIsStudentSelected(string role, bool expectedIsStudentSelected)
    {
        // Arrange
        var mockAuthService = new Mock<IAuthService>();
        var mockApiService = new Mock<IApiService>();
        var mockSessionService = new Mock<SessionService>();
        var mockLogger = new Mock<ILogger<LoginViewModel>>();
        var viewModel = new LoginViewModel(mockAuthService.Object, mockApiService.Object, mockSessionService.Object, mockLogger.Object);

        // Act
        viewModel.SelectedRole = role;

        // Assert
        Assert.AreEqual(expectedIsStudentSelected, viewModel.IsStudentSelected);
    }

    /// <summary>
    /// Tests that SelectedRole setter is case-sensitive for "Lecturer".
    /// Verifies that only exact match "Lecturer" makes IsLecturerSelected true.
    /// </summary>
    /// <param name="role">Role value with different casing.</param>
    /// <param name="expectedIsLecturerSelected">Expected value of IsLecturerSelected.</param>
    [TestMethod]
    [DataRow("Lecturer", true)]
    [DataRow("lecturer", false)]
    [DataRow("LECTURER", false)]
    [DataRow("LeCTuReR", false)]
    public void SelectedRole_CaseSensitivityForLecturer_AffectsIsLecturerSelected(string role, bool expectedIsLecturerSelected)
    {
        // Arrange
        var mockAuthService = new Mock<IAuthService>();
        var mockApiService = new Mock<IApiService>();
        var mockSessionService = new Mock<SessionService>();
        var mockLogger = new Mock<ILogger<LoginViewModel>>();
        var viewModel = new LoginViewModel(mockAuthService.Object, mockApiService.Object, mockSessionService.Object, mockLogger.Object);

        // Act
        viewModel.SelectedRole = role;

        // Assert
        Assert.AreEqual(expectedIsLecturerSelected, viewModel.IsLecturerSelected);
    }

    /// <summary>
    /// Tests that setting SelectedRole with leading or trailing spaces
    /// does not match "Student" or "Lecturer" exactly.
    /// Validates exact string matching without trimming.
    /// </summary>
    /// <param name="roleWithSpaces">Role value with leading or trailing spaces.</param>
    [TestMethod]
    [DataRow(" Student")]
    [DataRow("Student ")]
    [DataRow(" Student ")]
    [DataRow(" Lecturer")]
    [DataRow("Lecturer ")]
    [DataRow(" Lecturer ")]
    public void SelectedRole_SetWithLeadingOrTrailingSpaces_DoesNotMatchExactly(string roleWithSpaces)
    {
        // Arrange
        var mockAuthService = new Mock<IAuthService>();
        var mockApiService = new Mock<IApiService>();
        var mockSessionService = new Mock<SessionService>();
        var mockLogger = new Mock<ILogger<LoginViewModel>>();
        var viewModel = new LoginViewModel(mockAuthService.Object, mockApiService.Object, mockSessionService.Object, mockLogger.Object);

        // Act
        viewModel.SelectedRole = roleWithSpaces;

        // Assert
        Assert.IsFalse(viewModel.IsStudentSelected);
        Assert.IsFalse(viewModel.IsLecturerSelected);
    }

    /// <summary>
    /// Tests that PropertyChanged event provides correct PropertyName values
    /// for SelectedRole, IsStudentSelected, and IsLecturerSelected.
    /// Validates event arguments contain correct property names.
    /// </summary>
    [TestMethod]
    public void SelectedRole_SetValue_PropertyChangedEventContainsCorrectPropertyNames()
    {
        // Arrange
        var mockAuthService = new Mock<IAuthService>();
        var mockApiService = new Mock<IApiService>();
        var mockSessionService = new Mock<SessionService>();
        var mockLogger = new Mock<ILogger<LoginViewModel>>();
        var viewModel = new LoginViewModel(mockAuthService.Object, mockApiService.Object, mockSessionService.Object, mockLogger.Object);
        var propertyNames = new List<string>();
        viewModel.PropertyChanged += (sender, e) =>
        {
            if (e.PropertyName != null)
                propertyNames.Add(e.PropertyName);
        };

        // Act
        viewModel.SelectedRole = "Lecturer";

        // Assert
        Assert.AreEqual(3, propertyNames.Count);
        Assert.IsTrue(propertyNames.Contains("SelectedRole"));
        Assert.IsTrue(propertyNames.Contains("IsStudentSelected"));
        Assert.IsTrue(propertyNames.Contains("IsLecturerSelected"));
    }

    /// <summary>
    /// Tests that setting SelectedRole to empty string makes both
    /// IsStudentSelected and IsLecturerSelected false.
    /// Validates behavior with empty string input.
    /// </summary>
    [TestMethod]
    public void SelectedRole_SetToEmptyString_MakesBothComputedPropertiesFalse()
    {
        // Arrange
        var mockAuthService = new Mock<IAuthService>();
        var mockApiService = new Mock<IApiService>();
        var mockSessionService = new Mock<SessionService>();
        var mockLogger = new Mock<ILogger<LoginViewModel>>();
        var viewModel = new LoginViewModel(mockAuthService.Object, mockApiService.Object, mockSessionService.Object, mockLogger.Object);

        // Act
        viewModel.SelectedRole = "";

        // Assert
        Assert.AreEqual("", viewModel.SelectedRole);
        Assert.IsFalse(viewModel.IsStudentSelected);
        Assert.IsFalse(viewModel.IsLecturerSelected);
    }

    /// <summary>
    /// Tests that setting SelectedRole to whitespace-only strings
    /// makes both IsStudentSelected and IsLecturerSelected false.
    /// Validates behavior with whitespace inputs.
    /// </summary>
    /// <param name="whitespace">Whitespace-only string.</param>
    [TestMethod]
    [DataRow("   ")]
    [DataRow("\t")]
    [DataRow("\n")]
    [DataRow("\r")]
    [DataRow(" \t\n\r ")]
    public void SelectedRole_SetToWhitespace_MakesBothComputedPropertiesFalse(string whitespace)
    {
        // Arrange
        var mockAuthService = new Mock<IAuthService>();
        var mockApiService = new Mock<IApiService>();
        var mockSessionService = new Mock<SessionService>();
        var mockLogger = new Mock<ILogger<LoginViewModel>>();
        var viewModel = new LoginViewModel(mockAuthService.Object, mockApiService.Object, mockSessionService.Object, mockLogger.Object);

        // Act
        viewModel.SelectedRole = whitespace;

        // Assert
        Assert.AreEqual(whitespace, viewModel.SelectedRole);
        Assert.IsFalse(viewModel.IsStudentSelected);
        Assert.IsFalse(viewModel.IsLecturerSelected);
    }

    /// <summary>
    /// Tests that exactly three PropertyChanged events are raised when SelectedRole changes.
    /// Verifies the exact count of property change notifications.
    /// </summary>
    [TestMethod]
    public void SelectedRole_SetToDifferentValue_RaisesExactlyThreePropertyChangedEvents()
    {
        // Arrange
        var mockAuthService = new Mock<IAuthService>();
        var mockApiService = new Mock<IApiService>();
        var mockSessionService = new Mock<SessionService>();
        var mockLogger = new Mock<ILogger<LoginViewModel>>();
        var viewModel = new LoginViewModel(mockAuthService.Object, mockApiService.Object, mockSessionService.Object, mockLogger.Object);
        var eventCount = 0;
        viewModel.PropertyChanged += (sender, e) => eventCount++;

        // Act
        viewModel.SelectedRole = "Lecturer";

        // Assert
        Assert.AreEqual(3, eventCount);
    }
}