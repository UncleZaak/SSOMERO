using System;

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ssomero;
using Ssomero.Models;

namespace Ssomero.Models.UnitTests;




/// <summary>
/// Unit tests for the <see cref="UserItem"/> class.
/// </summary>
[TestClass]
public class UserItemTests
{
    /// <summary>
    /// Tests that the Initials property returns "?" when Name is an empty string.
    /// </summary>
    [TestMethod]
    public void Initials_EmptyName_ReturnsQuestionMark()
    {
        // Arrange
        var userItem = new UserItem { Name = string.Empty };

        // Act
        var result = userItem.Initials;

        // Assert
        Assert.AreEqual("?", result);
    }

    /// <summary>
    /// Tests that the Initials property returns "?" when Name contains only whitespace.
    /// </summary>
    [TestMethod]
    [DataRow("   ")]
    [DataRow(" ")]
    [DataRow("\t")]
    [DataRow("\n")]
    [DataRow("  \t  \n  ")]
    public void Initials_WhitespaceOnlyName_ReturnsQuestionMark(string name)
    {
        // Arrange
        var userItem = new UserItem { Name = name };

        // Act
        var result = userItem.Initials;

        // Assert
        Assert.AreEqual("?", result);
    }

    /// <summary>
    /// Tests that the Initials property returns the first character uppercased when Name is a single character.
    /// </summary>
    [TestMethod]
    [DataRow("a", "A")]
    [DataRow("Z", "Z")]
    [DataRow("x", "X")]
    [DataRow("1", "1")]
    [DataRow("@", "@")]
    public void Initials_SingleCharacterName_ReturnsUppercaseCharacter(string name, string expected)
    {
        // Arrange
        var userItem = new UserItem { Name = name };

        // Act
        var result = userItem.Initials;

        // Assert
        Assert.AreEqual(expected, result);
    }

    /// <summary>
    /// Tests that the Initials property returns the first character uppercased when Name is a single word.
    /// </summary>
    [TestMethod]
    [DataRow("John", "J")]
    [DataRow("alice", "A")]
    [DataRow("BOB", "B")]
    [DataRow("Mary", "M")]
    [DataRow("x", "X")]
    public void Initials_SingleWordName_ReturnsFirstCharacterUppercase(string name, string expected)
    {
        // Arrange
        var userItem = new UserItem { Name = name };

        // Act
        var result = userItem.Initials;

        // Assert
        Assert.AreEqual(expected, result);
    }

    /// <summary>
    /// Tests that the Initials property returns the first characters of the first two words uppercased when Name has two words.
    /// </summary>
    [TestMethod]
    [DataRow("John Doe", "JD")]
    [DataRow("alice bob", "AB")]
    [DataRow("MARY JANE", "MJ")]
    [DataRow("Peter Parker", "PP")]
    [DataRow("a b", "AB")]
    public void Initials_TwoWordName_ReturnsFirstTwoInitialsUppercase(string name, string expected)
    {
        // Arrange
        var userItem = new UserItem { Name = name };

        // Act
        var result = userItem.Initials;

        // Assert
        Assert.AreEqual(expected, result);
    }

    /// <summary>
    /// Tests that the Initials property returns only the first characters of the first two words when Name has three or more words.
    /// </summary>
    [TestMethod]
    [DataRow("John Michael Doe", "JM")]
    [DataRow("Alice Bob Charlie David", "AB")]
    [DataRow("Mary Jane Watson Parker", "MJ")]
    [DataRow("a b c", "AB")]
    public void Initials_ThreeOrMoreWordName_ReturnsFirstTwoInitialsUppercase(string name, string expected)
    {
        // Arrange
        var userItem = new UserItem { Name = name };

        // Act
        var result = userItem.Initials;

        // Assert
        Assert.AreEqual(expected, result);
    }

    /// <summary>
    /// Tests that the Initials property handles multiple consecutive spaces correctly by removing empty entries.
    /// </summary>
    [TestMethod]
    [DataRow("John    Doe", "JD")]
    [DataRow("Alice     Bob     Charlie", "AB")]
    [DataRow("Mary  Jane", "MJ")]
    public void Initials_MultipleSpacesBetweenWords_ReturnsCorrectInitials(string name, string expected)
    {
        // Arrange
        var userItem = new UserItem { Name = name };

        // Act
        var result = userItem.Initials;

        // Assert
        Assert.AreEqual(expected, result);
    }

    /// <summary>
    /// Tests that the Initials property handles leading and trailing spaces correctly.
    /// </summary>
    [TestMethod]
    [DataRow("  John", "J")]
    [DataRow("Alice  ", "A")]
    [DataRow("  Bob  ", "B")]
    [DataRow("  John Doe  ", "JD")]
    [DataRow("  Alice   Bob  ", "AB")]
    public void Initials_LeadingOrTrailingSpaces_ReturnsCorrectInitials(string name, string expected)
    {
        // Arrange
        var userItem = new UserItem { Name = name };

        // Act
        var result = userItem.Initials;

        // Assert
        Assert.AreEqual(expected, result);
    }

    /// <summary>
    /// Tests that the Initials property converts lowercase names to uppercase.
    /// </summary>
    [TestMethod]
    [DataRow("john", "J")]
    [DataRow("alice bob", "AB")]
    [DataRow("mary jane watson", "MJ")]
    public void Initials_LowercaseNames_ReturnsUppercaseInitials(string name, string expected)
    {
        // Arrange
        var userItem = new UserItem { Name = name };

        // Act
        var result = userItem.Initials;

        // Assert
        Assert.AreEqual(expected, result);
    }

    /// <summary>
    /// Tests that the Initials property handles names with special characters correctly.
    /// </summary>
    [TestMethod]
    [DataRow("O'Brien", "O")]
    [DataRow("Jean-Luc Picard", "JP")]
    [DataRow("Mary-Jane Watson", "MW")]
    [DataRow("John's Doe", "JD")]
    [DataRow("123 456", "14")]
    [DataRow("@User #Name", "@#")]
    public void Initials_NamesWithSpecialCharacters_ReturnsCorrectInitials(string name, string expected)
    {
        // Arrange
        var userItem = new UserItem { Name = name };

        // Act
        var result = userItem.Initials;

        // Assert
        Assert.AreEqual(expected, result);
    }

    /// <summary>
    /// Tests that the Initials property handles Unicode characters correctly.
    /// </summary>
    [TestMethod]
    [DataRow("Åke", "Å")]
    [DataRow("José García", "JG")]
    [DataRow("François Müller", "FM")]
    [DataRow("Владимир Путин", "ВП")]
    [DataRow("李明", "李明")]
    public void Initials_UnicodeCharacters_ReturnsCorrectInitials(string name, string expected)
    {
        // Arrange
        var userItem = new UserItem { Name = name };

        // Act
        var result = userItem.Initials;

        // Assert
        Assert.AreEqual(expected, result);
    }

    /// <summary>
    /// Tests that the Initials property handles very long names with many words correctly.
    /// </summary>
    [TestMethod]
    public void Initials_VeryLongNameWithManyWords_ReturnsFirstTwoInitials()
    {
        // Arrange
        var userItem = new UserItem { Name = "Pablo Diego José Francisco de Paula Juan Nepomuceno María de los Remedios Cipriano de la Santísima Trinidad Ruiz y Picasso" };

        // Act
        var result = userItem.Initials;

        // Assert
        Assert.AreEqual("PD", result);
    }

    /// <summary>
    /// Tests that the Initials property handles mixed case names correctly.
    /// </summary>
    [TestMethod]
    [DataRow("JoHn", "J")]
    [DataRow("aLiCe BoB", "AB")]
    [DataRow("MaRy JaNe", "MJ")]
    public void Initials_MixedCaseNames_ReturnsUppercaseInitials(string name, string expected)
    {
        // Arrange
        var userItem = new UserItem { Name = name };

        // Act
        var result = userItem.Initials;

        // Assert
        Assert.AreEqual(expected, result);
    }

    /// <summary>
    /// Tests that IsActive returns true when Status equals "Active".
    /// </summary>
    [TestMethod]
    public void IsActive_StatusIsActive_ReturnsTrue()
    {
        // Arrange
        var userItem = new UserItem
        {
            Status = "Active"
        };

        // Act
        var result = userItem.IsActive;

        // Assert
        Assert.IsTrue(result);
    }

    /// <summary>
    /// Tests that IsActive returns false for various non-"Active" Status values.
    /// Validates case sensitivity, empty strings, whitespace, and different status values.
    /// </summary>
    /// <param name="status">The Status value to test.</param>
    [TestMethod]
    [DataRow("")]
    [DataRow("active")]
    [DataRow("ACTIVE")]
    [DataRow("AcTiVe")]
    [DataRow(" Active")]
    [DataRow("Active ")]
    [DataRow(" Active ")]
    [DataRow("   ")]
    [DataRow("Suspended")]
    [DataRow("Inactive")]
    [DataRow("Pending")]
    [DataRow("Disabled")]
    [DataRow("Blocked")]
    [DataRow("Active\n")]
    [DataRow("\tActive")]
    [DataRow("Active\r\n")]
    [DataRow("xyz123!@#")]
    public void IsActive_StatusIsNotExactlyActive_ReturnsFalse(string status)
    {
        // Arrange
        var userItem = new UserItem
        {
            Status = status
        };

        // Act
        var result = userItem.IsActive;

        // Assert
        Assert.IsFalse(result);
    }

    /// <summary>
    /// Tests the DisplaySubtitle property when Role is "Student" and Program has various values.
    /// Validates that when the user is a student, the subtitle displays the program name,
    /// or "No program" when the program is null.
    /// </summary>
    /// <param name="program">The program value to test (can be null).</param>
    /// <param name="expected">The expected DisplaySubtitle value.</param>
    [TestMethod]
    [DataRow("Computer Science", "Computer Science", DisplayName = "Student with valid program")]
    [DataRow("", "", DisplayName = "Student with empty program")]
    [DataRow("   ", "   ", DisplayName = "Student with whitespace program")]
    [DataRow(null, "No program", DisplayName = "Student with null program")]
    [DataRow("Engineering & Technology", "Engineering & Technology", DisplayName = "Student with special characters in program")]
    [DataRow("ThisIsAVeryLongProgramNameThatExceedsTypicalLengthsToTestBoundaryConditionsAndEnsureThePropertyHandlesItCorrectly", "ThisIsAVeryLongProgramNameThatExceedsTypicalLengthsToTestBoundaryConditionsAndEnsureThePropertyHandlesItCorrectly", DisplayName = "Student with very long program name")]
    public void DisplaySubtitle_RoleIsStudent_ReturnsProgramOrDefaultMessage(string? program, string expected)
    {
        // Arrange
        var userItem = new UserItem
        {
            Role = "Student",
            Program = program,
            StaffId = "SomeStaffId"
        };

        // Act
        var result = userItem.DisplaySubtitle;

        // Assert
        Assert.AreEqual(expected, result);
    }

    /// <summary>
    /// Tests the DisplaySubtitle property when Role is not "Student" and StaffId has various values.
    /// Validates that when the user is not a student (e.g., lecturer, admin), the subtitle displays
    /// the staff ID, or "Lecturer" when the staff ID is null.
    /// </summary>
    /// <param name="role">The role value to test.</param>
    /// <param name="staffId">The staff ID value to test (can be null).</param>
    /// <param name="expected">The expected DisplaySubtitle value.</param>
    [TestMethod]
    [DataRow("Lecturer", "STAFF123", "STAFF123", DisplayName = "Lecturer with valid staff ID")]
    [DataRow("Lecturer", null, "Lecturer", DisplayName = "Lecturer with null staff ID")]
    [DataRow("Lecturer", "", "", DisplayName = "Lecturer with empty staff ID")]
    [DataRow("Lecturer", "   ", "   ", DisplayName = "Lecturer with whitespace staff ID")]
    [DataRow("Admin", "ADMIN001", "ADMIN001", DisplayName = "Admin role with valid staff ID")]
    [DataRow("Admin", null, "Lecturer", DisplayName = "Admin role with null staff ID")]
    [DataRow("", "STAFF456", "STAFF456", DisplayName = "Empty role with valid staff ID")]
    [DataRow("", null, "Lecturer", DisplayName = "Empty role with null staff ID")]
    [DataRow("   ", "STAFF789", "STAFF789", DisplayName = "Whitespace role with valid staff ID")]
    [DataRow("UnknownRole", null, "Lecturer", DisplayName = "Unknown role with null staff ID")]
    [DataRow("Lecturer", "STAFF@#$%", "STAFF@#$%", DisplayName = "Lecturer with special characters in staff ID")]
    [DataRow("Lecturer", "ThisIsAVeryLongStaffIdThatExceedsTypicalLengthsToTestBoundaryConditions", "ThisIsAVeryLongStaffIdThatExceedsTypicalLengthsToTestBoundaryConditions", DisplayName = "Lecturer with very long staff ID")]
    public void DisplaySubtitle_RoleIsNotStudent_ReturnsStaffIdOrDefaultMessage(string role, string? staffId, string expected)
    {
        // Arrange
        var userItem = new UserItem
        {
            Role = role,
            StaffId = staffId,
            Program = "SomeProgram"
        };

        // Act
        var result = userItem.DisplaySubtitle;

        // Assert
        Assert.AreEqual(expected, result);
    }

    /// <summary>
    /// Tests the DisplaySubtitle property with case-sensitive role comparison.
    /// Validates that the Role comparison is case-sensitive, so variations like
    /// "student", "STUDENT", or "StUdEnT" do not match "Student" and use the StaffId path.
    /// </summary>
    /// <param name="role">The role value with different casing.</param>
    /// <param name="staffId">The staff ID value.</param>
    /// <param name="expected">The expected DisplaySubtitle value.</param>
    [TestMethod]
    [DataRow("student", "STAFF123", "STAFF123", DisplayName = "Lowercase 'student' uses StaffId path")]
    [DataRow("STUDENT", "STAFF456", "STAFF456", DisplayName = "Uppercase 'STUDENT' uses StaffId path")]
    [DataRow("StUdEnT", "STAFF789", "STAFF789", DisplayName = "Mixed case 'StUdEnT' uses StaffId path")]
    [DataRow("student", null, "Lecturer", DisplayName = "Lowercase 'student' with null StaffId returns 'Lecturer'")]
    [DataRow("STUDENT", null, "Lecturer", DisplayName = "Uppercase 'STUDENT' with null StaffId returns 'Lecturer'")]
    public void DisplaySubtitle_RoleWithDifferentCasing_UsesCaseSensitiveComparison(string role, string? staffId, string expected)
    {
        // Arrange
        var userItem = new UserItem
        {
            Role = role,
            StaffId = staffId,
            Program = "Computer Science"
        };

        // Act
        var result = userItem.DisplaySubtitle;

        // Assert
        Assert.AreEqual(expected, result);
    }

    /// <summary>
    /// Tests the DisplaySubtitle property when both Program and StaffId have different values,
    /// ensuring the correct path is chosen based on the Role.
    /// </summary>
    [TestMethod]
    [DataRow("Student", "Engineering", "STAFF999", "Engineering", DisplayName = "Student role prefers Program over StaffId")]
    [DataRow("Lecturer", "Engineering", "STAFF999", "STAFF999", DisplayName = "Lecturer role prefers StaffId over Program")]
    public void DisplaySubtitle_BothProgramAndStaffIdSet_ReturnsCorrectValueBasedOnRole(string role, string? program, string? staffId, string expected)
    {
        // Arrange
        var userItem = new UserItem
        {
            Role = role,
            Program = program,
            StaffId = staffId
        };

        // Act
        var result = userItem.DisplaySubtitle;

        // Assert
        Assert.AreEqual(expected, result);
    }

    /// <summary>
    /// Tests that IsSuspended returns true when Status is exactly "Suspended".
    /// </summary>
    [TestMethod]
    public void IsSuspended_WhenStatusIsSuspended_ReturnsTrue()
    {
        // Arrange
        var userItem = new UserItem
        {
            Status = "Suspended"
        };

        // Act
        bool result = userItem.IsSuspended;

        // Assert
        Assert.IsTrue(result);
    }

    /// <summary>
    /// Tests that IsSuspended returns false when Status is not "Suspended".
    /// Covers various edge cases including empty strings, case variations, whitespace, and other status values.
    /// </summary>
    /// <param name="status">The status value to test.</param>
    [TestMethod]
    [DataRow("Active")]
    [DataRow("")]
    [DataRow("suspended")]
    [DataRow("SUSPENDED")]
    [DataRow("SuSpEnDeD")]
    [DataRow(" Suspended")]
    [DataRow("Suspended ")]
    [DataRow(" Suspended ")]
    [DataRow("Suspend")]
    [DataRow("Suspendedd")]
    [DataRow("Pending")]
    [DataRow("Inactive")]
    [DataRow("Banned")]
    [DataRow("Suspended!")]
    [DataRow("@Suspended")]
    [DataRow("Sus pended")]
    [DataRow("\tSuspended")]
    [DataRow("Suspended\n")]
    [DataRow("   ")]
    public void IsSuspended_WhenStatusIsNotSuspended_ReturnsFalse(string status)
    {
        // Arrange
        var userItem = new UserItem
        {
            Status = status
        };

        // Act
        bool result = userItem.IsSuspended;

        // Assert
        Assert.IsFalse(result);
    }

    /// <summary>
    /// Tests that the Initials property handles names with tab characters.
    /// Since Split only splits on space ' ', tabs are treated as part of the word.
    /// </summary>
    /// <param name="name">The name with tab characters to test.</param>
    /// <param name="expected">The expected initials.</param>
    [TestMethod]
    [DataRow("John\tDoe", "J")]
    [DataRow("Alice\tBob\tCharlie", "A")]
    public void Initials_NameWithTabCharacters_TreatsTabAsPartOfWord(string name, string expected)
    {
        // Arrange
        var userItem = new UserItem { Name = name };

        // Act
        var result = userItem.Initials;

        // Assert
        Assert.AreEqual(expected, result);
    }

    /// <summary>
    /// Tests that the Initials property handles names with newline characters.
    /// Since Split only splits on space ' ', newlines are treated as part of the word.
    /// </summary>
    /// <param name="name">The name with newline characters to test.</param>
    /// <param name="expected">The expected initials.</param>
    [TestMethod]
    [DataRow("John\nDoe", "J")]
    [DataRow("Alice\r\nBob", "A")]
    public void Initials_NameWithNewlineCharacters_TreatsNewlineAsPartOfWord(string name, string expected)
    {
        // Arrange
        var userItem = new UserItem { Name = name };

        // Act
        var result = userItem.Initials;

        // Assert
        Assert.AreEqual(expected, result);
    }

    /// <summary>
    /// Tests that the Initials property handles very long single word names correctly.
    /// Validates that the range operator [..1] works with long strings.
    /// </summary>
    [TestMethod]
    public void Initials_VeryLongSingleWord_ReturnsFirstCharacterUppercase()
    {
        // Arrange
        var longName = new string('a', 10000);
        var userItem = new UserItem { Name = longName };

        // Act
        var result = userItem.Initials;

        // Assert
        Assert.AreEqual("A", result);
    }

    /// <summary>
    /// Tests that the Initials property handles names with numeric characters correctly.
    /// Numbers should be uppercased (no change for digits).
    /// </summary>
    /// <param name="name">The name with numeric characters to test.</param>
    /// <param name="expected">The expected initials.</param>
    [TestMethod]
    [DataRow("123", "1")]
    [DataRow("456 789", "47")]
    [DataRow("9Test", "9")]
    public void Initials_NameWithNumericCharacters_ReturnsCorrectInitials(string name, string expected)
    {
        // Arrange
        var userItem = new UserItem { Name = name };

        // Act
        var result = userItem.Initials;

        // Assert
        Assert.AreEqual(expected, result);
    }

    /// <summary>
    /// Tests that DisplaySubtitle returns "No program" when Role is "Student" and Program is null.
    /// </summary>
    [TestMethod]
    public void DisplaySubtitle_RoleIsStudentAndProgramIsNull_ReturnsNoProgram()
    {
        // Arrange
        var userItem = new UserItem
        {
            Role = "Student",
            Program = null,
            StaffId = null
        };

        // Act
        var result = userItem.DisplaySubtitle;

        // Assert
        Assert.AreEqual("No program", result);
    }

    /// <summary>
    /// Tests that DisplaySubtitle returns the Program value when Role is "Student" and Program is not null.
    /// </summary>
    /// <param name="program">The program value to test.</param>
    [TestMethod]
    [DataRow("Computer Science")]
    [DataRow("Engineering")]
    [DataRow("")]
    [DataRow("   ")]
    [DataRow("Business Administration & Management")]
    [DataRow("ThisIsAVeryLongProgramNameThatExceedsTypicalLengthsToTestBoundaryConditionsAndEnsureThePropertyHandlesItCorrectlyWithoutAnyIssues")]
    public void DisplaySubtitle_RoleIsStudentAndProgramIsNotNull_ReturnsProgram(string program)
    {
        // Arrange
        var userItem = new UserItem
        {
            Role = "Student",
            Program = program,
            StaffId = "STAFF123"
        };

        // Act
        var result = userItem.DisplaySubtitle;

        // Assert
        Assert.AreEqual(program, result);
    }

    /// <summary>
    /// Tests that DisplaySubtitle returns "Lecturer" when Role is not "Student" and StaffId is null.
    /// </summary>
    /// <param name="role">The role value to test.</param>
    [TestMethod]
    [DataRow("Lecturer")]
    [DataRow("Admin")]
    [DataRow("")]
    [DataRow("   ")]
    [DataRow("SuperAdmin")]
    [DataRow("Manager")]
    public void DisplaySubtitle_RoleIsNotStudentAndStaffIdIsNull_ReturnsLecturer(string role)
    {
        // Arrange
        var userItem = new UserItem
        {
            Role = role,
            Program = "Computer Science",
            StaffId = null
        };

        // Act
        var result = userItem.DisplaySubtitle;

        // Assert
        Assert.AreEqual("Lecturer", result);
    }

    /// <summary>
    /// Tests that DisplaySubtitle returns the StaffId value when Role is not "Student" and StaffId is not null.
    /// </summary>
    /// <param name="role">The role value to test.</param>
    /// <param name="staffId">The staff ID value to test.</param>
    [TestMethod]
    [DataRow("Lecturer", "STAFF123")]
    [DataRow("Admin", "ADMIN001")]
    [DataRow("", "STAFF456")]
    [DataRow("   ", "STAFF789")]
    [DataRow("Professor", "")]
    [DataRow("Dean", "   ")]
    [DataRow("Coordinator", "STAFF@#$%^&*")]
    [DataRow("Manager", "ThisIsAVeryLongStaffIdThatExceedsTypicalLengthsToTestBoundaryConditionsAndEnsureThePropertyHandlesItCorrectlyWithoutAnyIssues")]
    public void DisplaySubtitle_RoleIsNotStudentAndStaffIdIsNotNull_ReturnsStaffId(string role, string staffId)
    {
        // Arrange
        var userItem = new UserItem
        {
            Role = role,
            Program = "Computer Science",
            StaffId = staffId
        };

        // Act
        var result = userItem.DisplaySubtitle;

        // Assert
        Assert.AreEqual(staffId, result);
    }

    /// <summary>
    /// Tests that DisplaySubtitle comparison is case-sensitive for Role.
    /// When Role is not exactly "Student", it should use the StaffId path.
    /// </summary>
    /// <param name="role">The role value with different casing.</param>
    /// <param name="staffId">The staff ID value.</param>
    /// <param name="expectedResult">The expected DisplaySubtitle value.</param>
    [TestMethod]
    [DataRow("student", null, "Lecturer")]
    [DataRow("STUDENT", null, "Lecturer")]
    [DataRow("StUdEnT", null, "Lecturer")]
    [DataRow("sTuDeNt", null, "Lecturer")]
    [DataRow("student", "STAFF123", "STAFF123")]
    [DataRow("STUDENT", "STAFF456", "STAFF456")]
    [DataRow(" Student", null, "Lecturer")]
    [DataRow("Student ", null, "Lecturer")]
    [DataRow(" Student ", null, "Lecturer")]
    public void DisplaySubtitle_RoleCaseSensitivity_UsesCorrectPath(string role, string? staffId, string expectedResult)
    {
        // Arrange
        var userItem = new UserItem
        {
            Role = role,
            Program = "Engineering",
            StaffId = staffId
        };

        // Act
        var result = userItem.DisplaySubtitle;

        // Assert
        Assert.AreEqual(expectedResult, result);
    }

    /// <summary>
    /// Tests that DisplaySubtitle handles both Program and StaffId being null correctly.
    /// When Role is "Student" and Program is null, it should return "No program".
    /// When Role is not "Student" and StaffId is null, it should return "Lecturer".
    /// </summary>
    /// <param name="role">The role value to test.</param>
    /// <param name="expectedResult">The expected DisplaySubtitle value.</param>
    [TestMethod]
    [DataRow("Student", "No program")]
    [DataRow("Lecturer", "Lecturer")]
    [DataRow("Admin", "Lecturer")]
    [DataRow("", "Lecturer")]
    [DataRow("Professor", "Lecturer")]
    public void DisplaySubtitle_BothProgramAndStaffIdNull_ReturnsCorrectDefaultValue(string role, string expectedResult)
    {
        // Arrange
        var userItem = new UserItem
        {
            Role = role,
            Program = null,
            StaffId = null
        };

        // Act
        var result = userItem.DisplaySubtitle;

        // Assert
        Assert.AreEqual(expectedResult, result);
    }

    /// <summary>
    /// Tests that DisplaySubtitle handles edge cases with special characters in Role.
    /// Since the comparison is exact, any Role with special characters should not match "Student".
    /// </summary>
    /// <param name="role">The role value with special characters.</param>
    [TestMethod]
    [DataRow("Student!")]
    [DataRow("@Student")]
    [DataRow("Student#")]
    [DataRow("Stud€nt")]
    [DataRow("Stu dent")]
    [DataRow("Student\n")]
    [DataRow("\tStudent")]
    [DataRow("Student\r\n")]
    public void DisplaySubtitle_RoleWithSpecialCharacters_UsesStaffIdPath(string role)
    {
        // Arrange
        var userItem = new UserItem
        {
            Role = role,
            Program = "Engineering",
            StaffId = "STAFF123"
        };

        // Act
        var result = userItem.DisplaySubtitle;

        // Assert
        Assert.AreEqual("STAFF123", result);
    }

    /// <summary>
    /// Tests that DisplaySubtitle handles empty and whitespace-only values for Program and StaffId correctly.
    /// Empty and whitespace strings should be returned as-is, not treated as null.
    /// </summary>
    /// <param name="role">The role value.</param>
    /// <param name="program">The program value (can be empty or whitespace).</param>
    /// <param name="staffId">The staff ID value (can be empty or whitespace).</param>
    /// <param name="expectedResult">The expected DisplaySubtitle value.</param>
    [TestMethod]
    [DataRow("Student", "", "STAFF123", "")]
    [DataRow("Student", "   ", "STAFF123", "   ")]
    [DataRow("Student", "\t", "STAFF123", "\t")]
    [DataRow("Lecturer", "Engineering", "", "")]
    [DataRow("Lecturer", "Engineering", "   ", "   ")]
    [DataRow("Lecturer", "Engineering", "\t\n", "\t\n")]
    public void DisplaySubtitle_EmptyOrWhitespaceValues_ReturnsEmptyOrWhitespace(string role, string? program, string? staffId, string expectedResult)
    {
        // Arrange
        var userItem = new UserItem
        {
            Role = role,
            Program = program,
            StaffId = staffId
        };

        // Act
        var result = userItem.DisplaySubtitle;

        // Assert
        Assert.AreEqual(expectedResult, result);
    }

    /// <summary>
    /// Tests that IsSuspended returns true when Status is exactly "Suspended".
    /// </summary>
    [TestMethod]
    public void IsSuspended_StatusIsSuspended_ReturnsTrue()
    {
        // Arrange
        var userItem = new UserItem
        {
            Id = Guid.NewGuid(),
            Name = "Test User",
            Email = "test@example.com",
            Role = "Student",
            Status = "Suspended",
            IsApproved = false,
            CreatedAt = DateTime.UtcNow
        };

        // Act
        var result = userItem.IsSuspended;

        // Assert
        Assert.IsTrue(result);
    }

    /// <summary>
    /// Tests that IsSuspended returns false for various non-"Suspended" Status values.
    /// Validates case sensitivity, empty strings, whitespace, partial matches, and different status values.
    /// </summary>
    /// <param name="status">The Status value to test.</param>
    [TestMethod]
    [DataRow("")]
    [DataRow("suspended")]
    [DataRow("SUSPENDED")]
    [DataRow("SuSpEnDeD")]
    [DataRow(" Suspended")]
    [DataRow("Suspended ")]
    [DataRow(" Suspended ")]
    [DataRow("   ")]
    [DataRow("Active")]
    [DataRow("Pending")]
    [DataRow("Inactive")]
    [DataRow("Banned")]
    [DataRow("Disabled")]
    [DataRow("Suspend")]
    [DataRow("Suspendedd")]
    [DataRow("Suspended!")]
    [DataRow("@Suspended")]
    [DataRow("Sus pended")]
    [DataRow("\tSuspended")]
    [DataRow("Suspended\n")]
    [DataRow("Suspended\r\n")]
    [DataRow("\nSuspended")]
    [DataRow("xyz123!@#")]
    public void IsSuspended_StatusIsNotExactlySuspended_ReturnsFalse(string status)
    {
        // Arrange
        var userItem = new UserItem
        {
            Id = Guid.NewGuid(),
            Name = "Test User",
            Email = "test@example.com",
            Role = "Student",
            Status = status,
            IsApproved = false,
            CreatedAt = DateTime.UtcNow
        };

        // Act
        var result = userItem.IsSuspended;

        // Assert
        Assert.IsFalse(result);
    }

    /// <summary>
    /// Tests that DisplaySubtitle returns the Program value when Role is exactly "Student" and Program is not null.
    /// Validates various Program values including empty strings, whitespace, normal text, special characters, and very long strings.
    /// </summary>
    /// <param name="program">The Program value to test.</param>
    /// <param name="expected">The expected DisplaySubtitle result.</param>
    [TestMethod]
    [DataRow("Computer Science", "Computer Science", DisplayName = "Student with normal program name")]
    [DataRow("Engineering", "Engineering", DisplayName = "Student with another program name")]
    [DataRow("", "", DisplayName = "Student with empty string program")]
    [DataRow("   ", "   ", DisplayName = "Student with whitespace-only program")]
    [DataRow("\t", "\t", DisplayName = "Student with tab character program")]
    [DataRow("\n", "\n", DisplayName = "Student with newline character program")]
    [DataRow("Business & Economics", "Business & Economics", DisplayName = "Student with special characters in program")]
    [DataRow("Program@#$%^&*()", "Program@#$%^&*()", DisplayName = "Student with various special characters")]
    [DataRow("ThisIsAVeryLongProgramNameThatExceedsTypicalLengthsToTestBoundaryConditionsAndEnsureThePropertyHandlesItCorrectlyWithoutAnyIssuesOrErrors", "ThisIsAVeryLongProgramNameThatExceedsTypicalLengthsToTestBoundaryConditionsAndEnsureThePropertyHandlesItCorrectlyWithoutAnyIssuesOrErrors", DisplayName = "Student with very long program name")]
    public void DisplaySubtitle_RoleIsStudentAndProgramIsNotNull_ReturnsProgram(string program, string expected)
    {
        // Arrange
        var userItem = new UserItem
        {
            Role = "Student",
            Program = program,
            StaffId = "STAFF999"
        };

        // Act
        var result = userItem.DisplaySubtitle;

        // Assert
        Assert.AreEqual(expected, result);
    }

    /// <summary>
    /// Tests that DisplaySubtitle returns the StaffId value when Role is not "Student" and StaffId is not null.
    /// Validates various StaffId values including empty strings, whitespace, normal text, special characters, and very long strings.
    /// </summary>
    /// <param name="role">The Role value to test.</param>
    /// <param name="staffId">The StaffId value to test.</param>
    /// <param name="expected">The expected DisplaySubtitle result.</param>
    [TestMethod]
    [DataRow("Lecturer", "STAFF123", "STAFF123", DisplayName = "Lecturer with normal staff ID")]
    [DataRow("Admin", "ADMIN001", "ADMIN001", DisplayName = "Admin with normal staff ID")]
    [DataRow("Professor", "", "", DisplayName = "Professor with empty string staff ID")]
    [DataRow("Dean", "   ", "   ", DisplayName = "Dean with whitespace staff ID")]
    [DataRow("", "STAFF456", "STAFF456", DisplayName = "Empty role with staff ID")]
    [DataRow("Manager", "\t", "\t", DisplayName = "Manager with tab character staff ID")]
    [DataRow("Coordinator", "STAFF@#$%", "STAFF@#$%", DisplayName = "Coordinator with special characters in staff ID")]
    [DataRow("Lecturer", "ThisIsAVeryLongStaffIdThatExceedsTypicalLengthsToTestBoundaryConditionsAndEnsureThePropertyHandlesItCorrectlyWithoutAnyIssuesOrErrors", "ThisIsAVeryLongStaffIdThatExceedsTypicalLengthsToTestBoundaryConditionsAndEnsureThePropertyHandlesItCorrectlyWithoutAnyIssuesOrErrors", DisplayName = "Lecturer with very long staff ID")]
    public void DisplaySubtitle_RoleIsNotStudentAndStaffIdIsNotNull_ReturnsStaffId(string role, string staffId, string expected)
    {
        // Arrange
        var userItem = new UserItem
        {
            Role = role,
            Program = "Computer Science",
            StaffId = staffId
        };

        // Act
        var result = userItem.DisplaySubtitle;

        // Assert
        Assert.AreEqual(expected, result);
    }

    /// <summary>
    /// Tests that DisplaySubtitle handles Role values with special characters correctly.
    /// Since the comparison is exact string match with "Student", any Role with leading/trailing spaces,
    /// special characters, or control characters should not match and use the StaffId path.
    /// </summary>
    /// <param name="role">The Role value with special characters or whitespace.</param>
    /// <param name="expected">The expected DisplaySubtitle result.</param>
    [TestMethod]
    [DataRow(" Student", "Lecturer", DisplayName = "Role with leading space uses StaffId path")]
    [DataRow("Student ", "Lecturer", DisplayName = "Role with trailing space uses StaffId path")]
    [DataRow(" Student ", "Lecturer", DisplayName = "Role with both leading and trailing spaces uses StaffId path")]
    [DataRow("Student!", "Lecturer", DisplayName = "Role with exclamation mark uses StaffId path")]
    [DataRow("@Student", "Lecturer", DisplayName = "Role with @ symbol uses StaffId path")]
    [DataRow("Student#", "Lecturer", DisplayName = "Role with # symbol uses StaffId path")]
    [DataRow("Stu dent", "Lecturer", DisplayName = "Role with space in middle uses StaffId path")]
    [DataRow("Student\n", "Lecturer", DisplayName = "Role with newline uses StaffId path")]
    [DataRow("\tStudent", "Lecturer", DisplayName = "Role with tab uses StaffId path")]
    public void DisplaySubtitle_RoleWithSpecialCharactersOrWhitespace_UsesStaffIdPath(string role, string expected)
    {
        // Arrange
        var userItem = new UserItem
        {
            Role = role,
            Program = "Engineering",
            StaffId = null
        };

        // Act
        var result = userItem.DisplaySubtitle;

        // Assert
        Assert.AreEqual(expected, result);
    }
}