using System;

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Ssomero.Models;
using Ssomero.Services;

namespace Ssomero.Services.UnitTests;




/// <summary>
/// Unit tests for the <see cref="SessionService"/> class.
/// </summary>
[TestClass]
public class SessionServiceTests
{
    /// <summary>
    /// Tests that SetUser correctly sets CurrentUser and maps "admin" role to UserRole.Admin.
    /// </summary>
    [TestMethod]
    public void SetUser_AdminRole_SetsCurrentUserAndAdminRole()
    {
        // Arrange
        var sessionService = new SessionService();
        var user = new AuthUserDto
        {
            Id = "user1",
            Email = "admin@test.com",
            Role = "admin",
            FullName = "Admin User"
        };

        // Act
        sessionService.SetUser(user);

        // Assert
        Assert.IsNotNull(sessionService.CurrentUser);
        Assert.AreEqual(user, sessionService.CurrentUser);
        Assert.AreEqual(UserRole.Admin, sessionService.Role);
        Assert.IsTrue(sessionService.IsAuthenticated);
    }

    /// <summary>
    /// Tests that SetUser correctly maps "lecturer" role to UserRole.Lecturer.
    /// </summary>
    [TestMethod]
    public void SetUser_LecturerRole_SetsLecturerRole()
    {
        // Arrange
        var sessionService = new SessionService();
        var user = new AuthUserDto
        {
            Id = "user2",
            Email = "lecturer@test.com",
            Role = "lecturer",
            FullName = "Lecturer User"
        };

        // Act
        sessionService.SetUser(user);

        // Assert
        Assert.AreEqual(UserRole.Lecturer, sessionService.Role);
        Assert.AreEqual(user, sessionService.CurrentUser);
    }

    /// <summary>
    /// Tests that SetUser correctly maps role variations for ClassRepresentative.
    /// </summary>
    /// <param name="roleString">The role string to test.</param>
    [TestMethod]
    [DataRow("classrepresentative")]
    [DataRow("classrep")]
    [DataRow("class_representative")]
    public void SetUser_ClassRepresentativeRoleVariations_SetsClassRepresentativeRole(string roleString)
    {
        // Arrange
        var sessionService = new SessionService();
        var user = new AuthUserDto
        {
            Id = "user3",
            Email = "rep@test.com",
            Role = roleString,
            FullName = "Class Rep User"
        };

        // Act
        sessionService.SetUser(user);

        // Assert
        Assert.AreEqual(UserRole.ClassRepresentative, sessionService.Role);
        Assert.AreEqual(user, sessionService.CurrentUser);
    }

    /// <summary>
    /// Tests that SetUser is case-insensitive for role mapping.
    /// </summary>
    /// <param name="roleString">The role string with different casing.</param>
    /// <param name="expectedRole">The expected UserRole enum value.</param>
    [TestMethod]
    [DataRow("Admin", UserRole.Admin)]
    [DataRow("ADMIN", UserRole.Admin)]
    [DataRow("AdMiN", UserRole.Admin)]
    [DataRow("Lecturer", UserRole.Lecturer)]
    [DataRow("LECTURER", UserRole.Lecturer)]
    [DataRow("LeCTuReR", UserRole.Lecturer)]
    [DataRow("ClassRepresentative", UserRole.ClassRepresentative)]
    [DataRow("CLASSREPRESENTATIVE", UserRole.ClassRepresentative)]
    [DataRow("ClassRep", UserRole.ClassRepresentative)]
    [DataRow("CLASSREP", UserRole.ClassRepresentative)]
    [DataRow("Class_Representative", UserRole.ClassRepresentative)]
    [DataRow("CLASS_REPRESENTATIVE", UserRole.ClassRepresentative)]
    public void SetUser_CaseInsensitiveRoles_MapsCorrectly(string roleString, UserRole expectedRole)
    {
        // Arrange
        var sessionService = new SessionService();
        var user = new AuthUserDto
        {
            Id = "user4",
            Email = "test@test.com",
            Role = roleString,
            FullName = "Test User"
        };

        // Act
        sessionService.SetUser(user);

        // Assert
        Assert.AreEqual(expectedRole, sessionService.Role);
        Assert.AreEqual(user, sessionService.CurrentUser);
    }

    /// <summary>
    /// Tests that SetUser defaults to Student role for unknown role strings.
    /// </summary>
    /// <param name="roleString">An unknown role string.</param>
    [TestMethod]
    [DataRow("student")]
    [DataRow("unknown")]
    [DataRow("guest")]
    [DataRow("moderator")]
    [DataRow("")]
    [DataRow("   ")]
    [DataRow("123")]
    [DataRow("admin123")]
    [DataRow("lecturers")]
    [DataRow("class representative")]
    [DataRow("class-rep")]
    public void SetUser_UnknownOrInvalidRole_DefaultsToStudentRole(string roleString)
    {
        // Arrange
        var sessionService = new SessionService();
        var user = new AuthUserDto
        {
            Id = "user5",
            Email = "student@test.com",
            Role = roleString,
            FullName = "Student User"
        };

        // Act
        sessionService.SetUser(user);

        // Assert
        Assert.AreEqual(UserRole.Student, sessionService.Role);
        Assert.AreEqual(user, sessionService.CurrentUser);
    }

    /// <summary>
    /// Tests that SetUser handles null Role property and defaults to Student role.
    /// </summary>
    [TestMethod]
    public void SetUser_NullRole_DefaultsToStudentRole()
    {
        // Arrange
        var sessionService = new SessionService();
        var user = new AuthUserDto
        {
            Id = "user6",
            Email = "nullrole@test.com",
            Role = null!,
            FullName = "Null Role User"
        };

        // Act
        sessionService.SetUser(user);

        // Assert
        Assert.AreEqual(UserRole.Student, sessionService.Role);
        Assert.AreEqual(user, sessionService.CurrentUser);
    }

    /// <summary>
    /// Tests that SetUser with special characters in role defaults to Student.
    /// </summary>
    /// <param name="roleString">A role string with special characters.</param>
    [TestMethod]
    [DataRow("admin@")]
    [DataRow("lecturer!")]
    [DataRow("class#rep")]
    [DataRow("роль")]
    [DataRow("管理员")]
    [DataRow("admin\n")]
    [DataRow("admin\t")]
    public void SetUser_RoleWithSpecialCharacters_DefaultsToStudentRole(string roleString)
    {
        // Arrange
        var sessionService = new SessionService();
        var user = new AuthUserDto
        {
            Id = "user7",
            Email = "special@test.com",
            Role = roleString,
            FullName = "Special Char User"
        };

        // Act
        sessionService.SetUser(user);

        // Assert
        Assert.AreEqual(UserRole.Student, sessionService.Role);
        Assert.AreEqual(user, sessionService.CurrentUser);
    }

    /// <summary>
    /// Tests that SetUser handles very long role strings and defaults to Student.
    /// </summary>
    [TestMethod]
    public void SetUser_VeryLongRoleString_DefaultsToStudentRole()
    {
        // Arrange
        var sessionService = new SessionService();
        var veryLongRole = new string('a', 10000);
        var user = new AuthUserDto
        {
            Id = "user8",
            Email = "long@test.com",
            Role = veryLongRole,
            FullName = "Long Role User"
        };

        // Act
        sessionService.SetUser(user);

        // Assert
        Assert.AreEqual(UserRole.Student, sessionService.Role);
        Assert.AreEqual(user, sessionService.CurrentUser);
    }

    /// <summary>
    /// Tests that SetUser correctly updates CurrentUser and Role when called multiple times.
    /// </summary>
    [TestMethod]
    public void SetUser_CalledMultipleTimes_UpdatesCurrentUserAndRole()
    {
        // Arrange
        var sessionService = new SessionService();
        var firstUser = new AuthUserDto
        {
            Id = "user9",
            Email = "first@test.com",
            Role = "student",
            FullName = "First User"
        };
        var secondUser = new AuthUserDto
        {
            Id = "user10",
            Email = "second@test.com",
            Role = "admin",
            FullName = "Second User"
        };

        // Act
        sessionService.SetUser(firstUser);
        var firstRole = sessionService.Role;
        var firstCurrentUser = sessionService.CurrentUser;

        sessionService.SetUser(secondUser);
        var secondRole = sessionService.Role;
        var secondCurrentUser = sessionService.CurrentUser;

        // Assert
        Assert.AreEqual(UserRole.Student, firstRole);
        Assert.AreEqual(firstUser, firstCurrentUser);
        Assert.AreEqual(UserRole.Admin, secondRole);
        Assert.AreEqual(secondUser, secondCurrentUser);
        Assert.AreNotEqual(firstUser, secondUser);
    }

    /// <summary>
    /// Tests that SetUser handles role strings with leading and trailing whitespace by defaulting to Student.
    /// </summary>
    /// <param name="roleString">A role string with whitespace.</param>
    [TestMethod]
    [DataRow(" admin")]
    [DataRow("admin ")]
    [DataRow(" admin ")]
    [DataRow("  lecturer  ")]
    [DataRow("\tadmin\t")]
    [DataRow("\nlecturer\n")]
    public void SetUser_RoleWithWhitespace_DefaultsToStudentRole(string roleString)
    {
        // Arrange
        var sessionService = new SessionService();
        var user = new AuthUserDto
        {
            Id = "user11",
            Email = "whitespace@test.com",
            Role = roleString,
            FullName = "Whitespace User"
        };

        // Act
        sessionService.SetUser(user);

        // Assert
        Assert.AreEqual(UserRole.Student, sessionService.Role);
        Assert.AreEqual(user, sessionService.CurrentUser);
    }

    /// <summary>
    /// Tests that IsAuthenticated returns false when CurrentUser is null (initial state).
    /// </summary>
    [TestMethod]
    public void IsAuthenticated_WhenCurrentUserIsNull_ReturnsFalse()
    {
        // Arrange
        var sessionService = new SessionService();

        // Act
        var result = sessionService.IsAuthenticated;

        // Assert
        Assert.IsFalse(result);
    }

    /// <summary>
    /// Tests that IsAuthenticated returns true when CurrentUser is set to a valid user.
    /// </summary>
    [TestMethod]
    public void IsAuthenticated_WhenCurrentUserIsSet_ReturnsTrue()
    {
        // Arrange
        var sessionService = new SessionService();
        var user = new AuthUserDto
        {
            Role = "admin"
        };

        // Act
        sessionService.SetUser(user);
        var result = sessionService.IsAuthenticated;

        // Assert
        Assert.IsTrue(result);
    }

    /// <summary>
    /// Tests that IsAuthenticated returns false after Clear is called.
    /// </summary>
    [TestMethod]
    public void IsAuthenticated_AfterClearIsCalled_ReturnsFalse()
    {
        // Arrange
        var sessionService = new SessionService();
        var user = new AuthUserDto
        {
            Role = "admin"
        };
        sessionService.SetUser(user);

        // Act
        sessionService.Clear();
        var result = sessionService.IsAuthenticated;

        // Assert
        Assert.IsFalse(result);
    }

    /// <summary>
    /// Tests that IsAuthenticated correctly reflects state changes when user is set and then cleared.
    /// </summary>
    [TestMethod]
    public void IsAuthenticated_WhenUserSetThenCleared_ReflectsStateChangesCorrectly()
    {
        // Arrange
        var sessionService = new SessionService();
        var user = new AuthUserDto
        {
            Role = "lecturer"
        };

        // Act & Assert - Initial state
        Assert.IsFalse(sessionService.IsAuthenticated);

        // Act & Assert - After setting user
        sessionService.SetUser(user);
        Assert.IsTrue(sessionService.IsAuthenticated);

        // Act & Assert - After clearing
        sessionService.Clear();
        Assert.IsFalse(sessionService.IsAuthenticated);
    }

    /// <summary>
    /// Tests that Clear sets CurrentUser to null when a user is authenticated.
    /// </summary>
    [TestMethod]
    public void Clear_WhenUserIsSet_SetsCurrentUserToNull()
    {
        // Arrange
        SessionService sessionService = new SessionService();
        Mock<AuthUserDto> mockUser = new Mock<AuthUserDto>();
        mockUser.SetupGet(u => u.Role).Returns("admin");
        sessionService.SetUser(mockUser.Object);

        // Act
        sessionService.Clear();

        // Assert
        Assert.IsNull(sessionService.CurrentUser);
    }

    /// <summary>
    /// Tests that Clear sets Role to Student regardless of the previous role value.
    /// Tests with different initial role values to ensure proper reset behavior.
    /// </summary>
    /// <param name="initialRole">The initial role string value before calling Clear.</param>
    [TestMethod]
    [DataRow("admin")]
    [DataRow("lecturer")]
    [DataRow("classrepresentative")]
    [DataRow("student")]
    [DataRow("")]
    [DataRow(null)]
    public void Clear_WithVariousInitialRoles_SetsRoleToStudent(string? initialRole)
    {
        // Arrange
        SessionService sessionService = new SessionService();
        Mock<AuthUserDto> mockUser = new Mock<AuthUserDto>();
        mockUser.SetupGet(u => u.Role).Returns(initialRole);
        sessionService.SetUser(mockUser.Object);

        // Act
        sessionService.Clear();

        // Assert
        Assert.AreEqual(UserRole.Student, sessionService.Role);
    }

    /// <summary>
    /// Tests that Clear sets IsAuthenticated to false when a user was previously set.
    /// </summary>
    [TestMethod]
    public void Clear_WhenUserIsSet_SetsIsAuthenticatedToFalse()
    {
        // Arrange
        SessionService sessionService = new SessionService();
        Mock<AuthUserDto> mockUser = new Mock<AuthUserDto>();
        mockUser.SetupGet(u => u.Role).Returns("admin");
        sessionService.SetUser(mockUser.Object);
        Assert.IsTrue(sessionService.IsAuthenticated, "Precondition: User should be authenticated before Clear");

        // Act
        sessionService.Clear();

        // Assert
        Assert.IsFalse(sessionService.IsAuthenticated);
    }

    /// <summary>
    /// Tests that Clear can be called when CurrentUser is already null without throwing an exception.
    /// Validates idempotent behavior.
    /// </summary>
    [TestMethod]
    public void Clear_WhenAlreadyClear_DoesNotThrow()
    {
        // Arrange
        SessionService sessionService = new SessionService();

        // Act & Assert
        sessionService.Clear();
        Assert.IsNull(sessionService.CurrentUser);
        Assert.AreEqual(UserRole.Student, sessionService.Role);
        Assert.IsFalse(sessionService.IsAuthenticated);
    }

    /// <summary>
    /// Tests that Clear can be called multiple times consecutively without issues.
    /// Validates idempotent behavior when called repeatedly.
    /// </summary>
    [TestMethod]
    public void Clear_CalledMultipleTimes_RemainsIdempotent()
    {
        // Arrange
        SessionService sessionService = new SessionService();
        Mock<AuthUserDto> mockUser = new Mock<AuthUserDto>();
        mockUser.SetupGet(u => u.Role).Returns("admin");
        sessionService.SetUser(mockUser.Object);

        // Act
        sessionService.Clear();
        sessionService.Clear();
        sessionService.Clear();

        // Assert
        Assert.IsNull(sessionService.CurrentUser);
        Assert.AreEqual(UserRole.Student, sessionService.Role);
        Assert.IsFalse(sessionService.IsAuthenticated);
    }

    /// <summary>
    /// Tests that Clear properly resets all properties after a user with Admin role was set.
    /// Ensures complete state reset including CurrentUser, Role, and IsAuthenticated.
    /// </summary>
    [TestMethod]
    public void Clear_AfterAdminUserSet_ResetsAllProperties()
    {
        // Arrange
        SessionService sessionService = new SessionService();
        Mock<AuthUserDto> mockUser = new Mock<AuthUserDto>();
        mockUser.SetupGet(u => u.Role).Returns("admin");
        sessionService.SetUser(mockUser.Object);
        Assert.AreEqual(UserRole.Admin, sessionService.Role, "Precondition: Role should be Admin");

        // Act
        sessionService.Clear();

        // Assert
        Assert.IsNull(sessionService.CurrentUser);
        Assert.AreEqual(UserRole.Student, sessionService.Role);
        Assert.IsFalse(sessionService.IsAuthenticated);
    }

    /// <summary>
    /// Tests that Clear properly resets all properties after a user with Lecturer role was set.
    /// Ensures complete state reset including CurrentUser, Role, and IsAuthenticated.
    /// </summary>
    [TestMethod]
    public void Clear_AfterLecturerUserSet_ResetsAllProperties()
    {
        // Arrange
        SessionService sessionService = new SessionService();
        Mock<AuthUserDto> mockUser = new Mock<AuthUserDto>();
        mockUser.SetupGet(u => u.Role).Returns("lecturer");
        sessionService.SetUser(mockUser.Object);
        Assert.AreEqual(UserRole.Lecturer, sessionService.Role, "Precondition: Role should be Lecturer");

        // Act
        sessionService.Clear();

        // Assert
        Assert.IsNull(sessionService.CurrentUser);
        Assert.AreEqual(UserRole.Student, sessionService.Role);
        Assert.IsFalse(sessionService.IsAuthenticated);
    }

    /// <summary>
    /// Tests that Clear properly resets all properties after a user with ClassRepresentative role was set.
    /// Ensures complete state reset including CurrentUser, Role, and IsAuthenticated.
    /// </summary>
    [TestMethod]
    public void Clear_AfterClassRepresentativeUserSet_ResetsAllProperties()
    {
        // Arrange
        SessionService sessionService = new SessionService();
        Mock<AuthUserDto> mockUser = new Mock<AuthUserDto>();
        mockUser.SetupGet(u => u.Role).Returns("classrepresentative");
        sessionService.SetUser(mockUser.Object);
        Assert.AreEqual(UserRole.ClassRepresentative, sessionService.Role, "Precondition: Role should be ClassRepresentative");

        // Act
        sessionService.Clear();

        // Assert
        Assert.IsNull(sessionService.CurrentUser);
        Assert.AreEqual(UserRole.Student, sessionService.Role);
        Assert.IsFalse(sessionService.IsAuthenticated);
    }

    /// <summary>
    /// Tests that IsAuthenticated remains true after multiple SetUser calls with different users.
    /// </summary>
    [TestMethod]
    public void IsAuthenticated_AfterMultipleSetUserCalls_ReturnsTrue()
    {
        // Arrange
        var sessionService = new SessionService();
        var adminUser = new AuthUserDto
        {
            Role = "admin"
        };
        var lecturerUser = new AuthUserDto
        {
            Role = "lecturer"
        };

        // Act
        sessionService.SetUser(adminUser);
        var resultAfterFirstSet = sessionService.IsAuthenticated;
        sessionService.SetUser(lecturerUser);
        var resultAfterSecondSet = sessionService.IsAuthenticated;

        // Assert
        Assert.IsTrue(resultAfterFirstSet);
        Assert.IsTrue(resultAfterSecondSet);
    }

    /// <summary>
    /// Tests that IsAuthenticated returns true regardless of the role assigned to the user.
    /// </summary>
    /// <param name="roleString">The role string to test.</param>
    [TestMethod]
    [DataRow("admin")]
    [DataRow("lecturer")]
    [DataRow("classrepresentative")]
    [DataRow("student")]
    [DataRow("unknown")]
    [DataRow("")]
    [DataRow(null)]
    public void IsAuthenticated_WithVariousRoles_ReturnsTrue(string? roleString)
    {
        // Arrange
        var sessionService = new SessionService();
        var user = new AuthUserDto
        {
            Role = roleString
        };

        // Act
        sessionService.SetUser(user);
        var result = sessionService.IsAuthenticated;

        // Assert
        Assert.IsTrue(result);
    }

    /// <summary>
    /// Tests that IsAuthenticated correctly reflects state after multiple set and clear operations.
    /// </summary>
    [TestMethod]
    public void IsAuthenticated_AfterMultipleSetAndClearOperations_ReflectsStateCorrectly()
    {
        // Arrange
        var sessionService = new SessionService();
        var user1 = new AuthUserDto { Role = "admin" };
        var user2 = new AuthUserDto { Role = "lecturer" };

        // Act & Assert - Initial state
        Assert.IsFalse(sessionService.IsAuthenticated);

        // Act & Assert - First set
        sessionService.SetUser(user1);
        Assert.IsTrue(sessionService.IsAuthenticated);

        // Act & Assert - First clear
        sessionService.Clear();
        Assert.IsFalse(sessionService.IsAuthenticated);

        // Act & Assert - Second set
        sessionService.SetUser(user2);
        Assert.IsTrue(sessionService.IsAuthenticated);

        // Act & Assert - Second clear
        sessionService.Clear();
        Assert.IsFalse(sessionService.IsAuthenticated);
    }

    /// <summary>
    /// Tests that SetUser correctly sets all properties including IsAuthenticated after setting a valid user.
    /// </summary>
    [TestMethod]
    public void SetUser_ValidUser_SetsAllPropertiesCorrectly()
    {
        // Arrange
        var sessionService = new SessionService();
        var user = new AuthUserDto
        {
            Id = "user12",
            Email = "complete@test.com",
            Role = "admin",
            FullName = "Complete Test User"
        };

        // Act
        sessionService.SetUser(user);

        // Assert
        Assert.AreEqual(user, sessionService.CurrentUser);
        Assert.AreEqual(UserRole.Admin, sessionService.Role);
        Assert.IsTrue(sessionService.IsAuthenticated);
        Assert.AreEqual("user12", sessionService.CurrentUser.Id);
        Assert.AreEqual("complete@test.com", sessionService.CurrentUser.Email);
        Assert.AreEqual("admin", sessionService.CurrentUser.Role);
        Assert.AreEqual("Complete Test User", sessionService.CurrentUser.FullName);
    }

}