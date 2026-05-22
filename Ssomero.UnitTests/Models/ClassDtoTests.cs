using System;

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ssomero;
using Ssomero.Models;

namespace Ssomero.Models.UnitTests;




/// <summary>
/// Unit tests for the ClassDto class.
/// </summary>
[TestClass]
public class ClassDtoTests
{
    /// <summary>
    /// Tests that IdAsGuid returns the correctly parsed Guid when Id contains a valid Guid string.
    /// </summary>
    /// <param name="idValue">The string value to set as Id.</param>
    /// <param name="expectedGuidString">The expected Guid string representation.</param>
    [TestMethod]
    [DataRow("d9a7e3b2-5c4f-4e8a-9b1d-2f3c4a5b6c7d", "d9a7e3b2-5c4f-4e8a-9b1d-2f3c4a5b6c7d")]
    [DataRow("D9A7E3B2-5C4F-4E8A-9B1D-2F3C4A5B6C7D", "d9a7e3b2-5c4f-4e8a-9b1d-2f3c4a5b6c7d")]
    [DataRow("{d9a7e3b2-5c4f-4e8a-9b1d-2f3c4a5b6c7d}", "d9a7e3b2-5c4f-4e8a-9b1d-2f3c4a5b6c7d")]
    [DataRow("(d9a7e3b2-5c4f-4e8a-9b1d-2f3c4a5b6c7d)", "d9a7e3b2-5c4f-4e8a-9b1d-2f3c4a5b6c7d")]
    [DataRow("00000000-0000-0000-0000-000000000000", "00000000-0000-0000-0000-000000000000")]
    [DataRow("ffffffff-ffff-ffff-ffff-ffffffffffff", "ffffffff-ffff-ffff-ffff-ffffffffffff")]
    [DataRow("FFFFFFFF-FFFF-FFFF-FFFF-FFFFFFFFFFFF", "ffffffff-ffff-ffff-ffff-ffffffffffff")]
    public void IdAsGuid_ValidGuidString_ReturnsCorrectGuid(string idValue, string expectedGuidString)
    {
        // Arrange
        var classDto = new ClassDto { Id = idValue };
        var expectedGuid = Guid.Parse(expectedGuidString);

        // Act
        var result = classDto.IdAsGuid;

        // Assert
        Assert.AreEqual(expectedGuid, result);
    }

    /// <summary>
    /// Tests that IdAsGuid returns Guid.Empty when Id contains an invalid Guid string.
    /// </summary>
    /// <param name="idValue">The invalid string value to set as Id.</param>
    [TestMethod]
    [DataRow("")]
    [DataRow("   ")]
    [DataRow("not-a-guid")]
    [DataRow("d9a7e3b2")]
    [DataRow("d9a7e3b2-5c4f-4e8a-9b1d-2f3c4a5b6c7d-extra")]
    [DataRow("d9a7e3b2-5c4f-4e8a-9b1d")]
    [DataRow("!@#$%^&*()")]
    [DataRow("12345")]
    [DataRow("d9a7e3b2-5c4f-4e8a-9b1d-2f3c4a5b6c7")]
    [DataRow("zzzzzzzz-zzzz-zzzz-zzzz-zzzzzzzzzzzz")]
    [DataRow("d9a7e3b2_5c4f_4e8a_9b1d_2f3c4a5b6c7d")]
    public void IdAsGuid_InvalidGuidString_ReturnsGuidEmpty(string idValue)
    {
        // Arrange
        var classDto = new ClassDto { Id = idValue };

        // Act
        var result = classDto.IdAsGuid;

        // Assert
        Assert.AreEqual(Guid.Empty, result);
    }

    /// <summary>
    /// Tests that IdAsGuid returns Guid.Empty when Id is set to a very long string.
    /// </summary>
    [TestMethod]
    public void IdAsGuid_VeryLongString_ReturnsGuidEmpty()
    {
        // Arrange
        var classDto = new ClassDto { Id = new string('a', 10000) };

        // Act
        var result = classDto.IdAsGuid;

        // Assert
        Assert.AreEqual(Guid.Empty, result);
    }

    /// <summary>
    /// Tests that IdAsGuid returns Guid.Empty when Id contains special control characters.
    /// </summary>
    [TestMethod]
    public void IdAsGuid_SpecialControlCharacters_ReturnsGuidEmpty()
    {
        // Arrange
        var classDto = new ClassDto { Id = "d9a7e3b2-5c4f-4e8a-9b1d-2f3c4a5b6c7d\0\n\r\t" };

        // Act
        var result = classDto.IdAsGuid;

        // Assert
        Assert.AreEqual(Guid.Empty, result);
    }

    /// <summary>
    /// Tests that IdAsGuid can be called multiple times and returns consistent results.
    /// </summary>
    [TestMethod]
    public void IdAsGuid_CalledMultipleTimes_ReturnsConsistentResult()
    {
        // Arrange
        var classDto = new ClassDto { Id = "d9a7e3b2-5c4f-4e8a-9b1d-2f3c4a5b6c7d" };
        var expectedGuid = Guid.Parse("d9a7e3b2-5c4f-4e8a-9b1d-2f3c4a5b6c7d");

        // Act
        var result1 = classDto.IdAsGuid;
        var result2 = classDto.IdAsGuid;
        var result3 = classDto.IdAsGuid;

        // Assert
        Assert.AreEqual(expectedGuid, result1);
        Assert.AreEqual(expectedGuid, result2);
        Assert.AreEqual(expectedGuid, result3);
        Assert.AreEqual(result1, result2);
        Assert.AreEqual(result2, result3);
    }

    /// <summary>
    /// Tests that IdAsGuid reflects changes to the Id property.
    /// </summary>
    [TestMethod]
    public void IdAsGuid_IdPropertyChanged_ReflectsNewValue()
    {
        // Arrange
        var classDto = new ClassDto { Id = "d9a7e3b2-5c4f-4e8a-9b1d-2f3c4a5b6c7d" };
        var initialGuid = Guid.Parse("d9a7e3b2-5c4f-4e8a-9b1d-2f3c4a5b6c7d");
        var newGuid = Guid.Parse("a1b2c3d4-e5f6-7890-abcd-ef1234567890");

        // Act
        var result1 = classDto.IdAsGuid;
        classDto.Id = "a1b2c3d4-e5f6-7890-abcd-ef1234567890";
        var result2 = classDto.IdAsGuid;

        // Assert
        Assert.AreEqual(initialGuid, result1);
        Assert.AreEqual(newGuid, result2);
    }

    /// <summary>
    /// Tests that IdAsGuid returns Guid.Empty when initialized with default constructor and Id not set.
    /// </summary>
    [TestMethod]
    public void IdAsGuid_DefaultConstructorIdNotModified_ReturnsGuidEmpty()
    {
        // Arrange
        var classDto = new ClassDto();

        // Act
        var result = classDto.IdAsGuid;

        // Assert
        Assert.AreEqual(Guid.Empty, result);
    }

    /// <summary>
    /// Tests that IdAsGuid returns the correct Guid when using parameterized constructor with valid Guid string.
    /// </summary>
    [TestMethod]
    public void IdAsGuid_ParameterizedConstructorWithValidGuid_ReturnsCorrectGuid()
    {
        // Arrange
        var guidString = "d9a7e3b2-5c4f-4e8a-9b1d-2f3c4a5b6c7d";
        var expectedGuid = Guid.Parse(guidString);
        var classDto = new ClassDto(guidString, "Test Class", "CS101", null, 30, "Dr. Smith");

        // Act
        var result = classDto.IdAsGuid;

        // Assert
        Assert.AreEqual(expectedGuid, result);
    }

    /// <summary>
    /// Tests that IdAsGuid returns Guid.Empty when using parameterized constructor with invalid Guid string.
    /// </summary>
    [TestMethod]
    public void IdAsGuid_ParameterizedConstructorWithInvalidGuid_ReturnsGuidEmpty()
    {
        // Arrange
        var classDto = new ClassDto("invalid-guid", "Test Class", "CS101", null, 30, "Dr. Smith");

        // Act
        var result = classDto.IdAsGuid;

        // Assert
        Assert.AreEqual(Guid.Empty, result);
    }

    /// <summary>
    /// Tests that the parameterless constructor creates a valid instance with all properties initialized to their default values.
    /// Verifies that Id and Name are set to empty strings, nullable properties are null, and EnrolledStudents is 0.
    /// </summary>
    [TestMethod]
    public void ClassDto_ParameterlessConstructor_CreatesInstanceWithDefaultValues()
    {
        // Arrange & Act
        var classDto = new ClassDto();

        // Assert
        Assert.IsNotNull(classDto);
        Assert.AreEqual(string.Empty, classDto.Id);
        Assert.AreEqual(string.Empty, classDto.Name);
        Assert.IsNull(classDto.CourseCode);
        Assert.IsNull(classDto.ParentClassId);
        Assert.AreEqual(0, classDto.EnrolledStudents);
        Assert.IsNull(classDto.LecturerName);
    }

    /// <summary>
    /// Tests that the parameterized constructor correctly assigns all provided parameters to their corresponding properties.
    /// </summary>
    /// <param name="id">The class identifier.</param>
    /// <param name="name">The class name.</param>
    /// <param name="courseCode">The course code (nullable).</param>
    /// <param name="parentClassId">The parent class identifier (nullable).</param>
    /// <param name="enrolledStudents">The number of enrolled students.</param>
    /// <param name="lecturerName">The lecturer name (nullable).</param>
    [TestMethod]
    [DataRow("class-123", "Computer Science 101", "CS101", "parent-456", 30, "Dr. Smith")]
    [DataRow("", "", "", "", 0, "")]
    [DataRow("   ", "   ", "   ", "   ", 1, "   ")]
    [DataRow("id-with-special-chars!@#$", "Name with special chars: <>?", "CS-202!", "parent@#$", 100, "Dr. O'Brien")]
    [DataRow("very-long-id-string-with-many-characters-to-test-edge-cases-1234567890", "Very Long Class Name With Many Words And Characters To Test Edge Cases", "COURSE999", "parent-long-id-12345", 500, "Professor With Very Long Name")]
    public void Constructor_ValidInputs_AssignsAllPropertiesCorrectly(string id, string name, string? courseCode, string? parentClassId, int enrolledStudents, string? lecturerName)
    {
        // Act
        var result = new ClassDto(id, name, courseCode, parentClassId, enrolledStudents, lecturerName);

        // Assert
        Assert.AreEqual(id, result.Id);
        Assert.AreEqual(name, result.Name);
        Assert.AreEqual(courseCode, result.CourseCode);
        Assert.AreEqual(parentClassId, result.ParentClassId);
        Assert.AreEqual(enrolledStudents, result.EnrolledStudents);
        Assert.AreEqual(lecturerName, result.LecturerName);
    }

    /// <summary>
    /// Tests that the parameterized constructor correctly handles null values for nullable parameters.
    /// </summary>
    /// <param name="id">The class identifier.</param>
    /// <param name="name">The class name.</param>
    /// <param name="courseCode">The course code (nullable).</param>
    /// <param name="parentClassId">The parent class identifier (nullable).</param>
    /// <param name="enrolledStudents">The number of enrolled students.</param>
    /// <param name="lecturerName">The lecturer name (nullable).</param>
    [TestMethod]
    [DataRow("id-1", "Name 1", null, null, 0, null)]
    [DataRow("id-2", "Name 2", "CS101", null, 10, null)]
    [DataRow("id-3", "Name 3", null, "parent-1", 20, null)]
    [DataRow("id-4", "Name 4", null, null, 30, "Lecturer")]
    [DataRow("id-5", "Name 5", "CS202", "parent-2", 40, null)]
    public void Constructor_NullableParameters_AssignsNullCorrectly(string id, string name, string? courseCode, string? parentClassId, int enrolledStudents, string? lecturerName)
    {
        // Act
        var result = new ClassDto(id, name, courseCode, parentClassId, enrolledStudents, lecturerName);

        // Assert
        Assert.AreEqual(id, result.Id);
        Assert.AreEqual(name, result.Name);
        Assert.AreEqual(courseCode, result.CourseCode);
        Assert.AreEqual(parentClassId, result.ParentClassId);
        Assert.AreEqual(enrolledStudents, result.EnrolledStudents);
        Assert.AreEqual(lecturerName, result.LecturerName);
    }

    /// <summary>
    /// Tests that the parameterized constructor correctly handles boundary values for the enrolledStudents parameter.
    /// </summary>
    /// <param name="enrolledStudents">The number of enrolled students to test.</param>
    [TestMethod]
    [DataRow(int.MinValue)]
    [DataRow(-1)]
    [DataRow(0)]
    [DataRow(1)]
    [DataRow(int.MaxValue)]
    public void Constructor_EnrolledStudentsBoundaryValues_AssignsCorrectly(int enrolledStudents)
    {
        // Arrange
        var id = "test-id";
        var name = "Test Name";
        string? courseCode = "CS101";
        string? parentClassId = "parent-1";
        string? lecturerName = "Test Lecturer";

        // Act
        var result = new ClassDto(id, name, courseCode, parentClassId, enrolledStudents, lecturerName);

        // Assert
        Assert.AreEqual(id, result.Id);
        Assert.AreEqual(name, result.Name);
        Assert.AreEqual(courseCode, result.CourseCode);
        Assert.AreEqual(parentClassId, result.ParentClassId);
        Assert.AreEqual(enrolledStudents, result.EnrolledStudents);
        Assert.AreEqual(lecturerName, result.LecturerName);
    }

    /// <summary>
    /// Tests that the parameterized constructor correctly handles strings with control characters and special Unicode characters.
    /// </summary>
    [TestMethod]
    public void Constructor_StringsWithControlCharacters_AssignsCorrectly()
    {
        // Arrange
        var id = "id\t\n\r";
        var name = "Name\u0000\u001F";
        var courseCode = "Course\u2022\u2023";
        var parentClassId = "Parent\uFFFD";
        var enrolledStudents = 15;
        var lecturerName = "Lecturer\u00A0\u200B";

        // Act
        var result = new ClassDto(id, name, courseCode, parentClassId, enrolledStudents, lecturerName);

        // Assert
        Assert.AreEqual(id, result.Id);
        Assert.AreEqual(name, result.Name);
        Assert.AreEqual(courseCode, result.CourseCode);
        Assert.AreEqual(parentClassId, result.ParentClassId);
        Assert.AreEqual(enrolledStudents, result.EnrolledStudents);
        Assert.AreEqual(lecturerName, result.LecturerName);
    }

    /// <summary>
    /// Tests that the parameterized constructor handles all nullable parameters set to null simultaneously.
    /// </summary>
    [TestMethod]
    public void Constructor_AllNullableParametersNull_AssignsCorrectly()
    {
        // Arrange
        var id = "test-id";
        var name = "Test Name";
        string? courseCode = null;
        string? parentClassId = null;
        var enrolledStudents = 0;
        string? lecturerName = null;

        // Act
        var result = new ClassDto(id, name, courseCode, parentClassId, enrolledStudents, lecturerName);

        // Assert
        Assert.AreEqual(id, result.Id);
        Assert.AreEqual(name, result.Name);
        Assert.IsNull(result.CourseCode);
        Assert.IsNull(result.ParentClassId);
        Assert.AreEqual(enrolledStudents, result.EnrolledStudents);
        Assert.IsNull(result.LecturerName);
    }

    /// <summary>
    /// Tests that the parameterized constructor handles negative enrolled students values correctly.
    /// </summary>
    [TestMethod]
    [DataRow(-100)]
    [DataRow(-1000)]
    [DataRow(-999999)]
    public void Constructor_NegativeEnrolledStudents_AssignsCorrectly(int enrolledStudents)
    {
        // Arrange
        var id = "test-id";
        var name = "Test Name";
        string? courseCode = "CS101";
        string? parentClassId = "parent-1";
        string? lecturerName = "Test Lecturer";

        // Act
        var result = new ClassDto(id, name, courseCode, parentClassId, enrolledStudents, lecturerName);

        // Assert
        Assert.AreEqual(enrolledStudents, result.EnrolledStudents);
    }

    /// <summary>
    /// Tests that the parameterless constructor creates a valid instance with all properties initialized to their default values.
    /// Verifies that Id and Name are set to empty strings, nullable properties are null, and EnrolledStudents is 0.
    /// </summary>
    [TestMethod]
    public void Constructor_Parameterless_InitializesAllPropertiesToDefaultValues()
    {
        // Arrange & Act
        var classDto = new ClassDto();

        // Assert
        Assert.IsNotNull(classDto);
        Assert.AreEqual(string.Empty, classDto.Id);
        Assert.AreEqual(string.Empty, classDto.Name);
        Assert.IsNull(classDto.CourseCode);
        Assert.IsNull(classDto.ParentClassId);
        Assert.AreEqual(0, classDto.EnrolledStudents);
        Assert.IsNull(classDto.LecturerName);
    }

    /// <summary>
    /// Tests that IdAsGuid returns Guid.Empty when the parameterless constructor is used and Id is not modified.
    /// This verifies that the computed property correctly handles the default empty string Id value.
    /// </summary>
    [TestMethod]
    public void Constructor_Parameterless_IdAsGuidReturnsGuidEmpty()
    {
        // Arrange & Act
        var classDto = new ClassDto();

        // Assert
        Assert.AreEqual(Guid.Empty, classDto.IdAsGuid);
    }

    /// <summary>
    /// Tests that all properties can be successfully modified after construction with the parameterless constructor.
    /// This verifies that the instance is fully mutable and properties are not read-only.
    /// </summary>
    [TestMethod]
    public void Constructor_Parameterless_AllowsPropertyModification()
    {
        // Arrange
        var classDto = new ClassDto();
        var newId = "test-id-123";
        var newName = "Test Class Name";
        var newCourseCode = "CS101";
        var newParentClassId = "parent-123";
        var newEnrolledStudents = 50;
        var newLecturerName = "Dr. Test";

        // Act
        classDto.Id = newId;
        classDto.Name = newName;
        classDto.CourseCode = newCourseCode;
        classDto.ParentClassId = newParentClassId;
        classDto.EnrolledStudents = newEnrolledStudents;
        classDto.LecturerName = newLecturerName;

        // Assert
        Assert.AreEqual(newId, classDto.Id);
        Assert.AreEqual(newName, classDto.Name);
        Assert.AreEqual(newCourseCode, classDto.CourseCode);
        Assert.AreEqual(newParentClassId, classDto.ParentClassId);
        Assert.AreEqual(newEnrolledStudents, classDto.EnrolledStudents);
        Assert.AreEqual(newLecturerName, classDto.LecturerName);
    }

    /// <summary>
    /// Tests that multiple instances created with the parameterless constructor are independent.
    /// Modifying one instance should not affect another instance.
    /// </summary>
    [TestMethod]
    public void Constructor_Parameterless_MultipleInstancesAreIndependent()
    {
        // Arrange
        var classDto1 = new ClassDto();
        var classDto2 = new ClassDto();

        // Act
        classDto1.Id = "id-1";
        classDto1.Name = "Name 1";
        classDto1.EnrolledStudents = 10;

        classDto2.Id = "id-2";
        classDto2.Name = "Name 2";
        classDto2.EnrolledStudents = 20;

        // Assert
        Assert.AreEqual("id-1", classDto1.Id);
        Assert.AreEqual("Name 1", classDto1.Name);
        Assert.AreEqual(10, classDto1.EnrolledStudents);

        Assert.AreEqual("id-2", classDto2.Id);
        Assert.AreEqual("Name 2", classDto2.Name);
        Assert.AreEqual(20, classDto2.EnrolledStudents);
    }

    /// <summary>
    /// Tests that nullable properties can be set to null after parameterless constructor initialization.
    /// Verifies that nullable properties remain properly nullable after construction.
    /// </summary>
    [TestMethod]
    public void Constructor_Parameterless_NullablePropertiesAcceptNull()
    {
        // Arrange
        var classDto = new ClassDto();

        // Act
        classDto.CourseCode = "CS101";
        classDto.ParentClassId = "parent-1";
        classDto.LecturerName = "Dr. Smith";

        classDto.CourseCode = null;
        classDto.ParentClassId = null;
        classDto.LecturerName = null;

        // Assert
        Assert.IsNull(classDto.CourseCode);
        Assert.IsNull(classDto.ParentClassId);
        Assert.IsNull(classDto.LecturerName);
    }

    /// <summary>
    /// Tests that EnrolledStudents can be set to boundary values after parameterless constructor initialization.
    /// This verifies that the int property accepts the full range of valid integer values.
    /// </summary>
    [TestMethod]
    [DataRow(int.MinValue)]
    [DataRow(-1)]
    [DataRow(0)]
    [DataRow(1)]
    [DataRow(int.MaxValue)]
    public void Constructor_Parameterless_EnrolledStudentsAcceptsBoundaryValues(int enrolledStudents)
    {
        // Arrange
        var classDto = new ClassDto();

        // Act
        classDto.EnrolledStudents = enrolledStudents;

        // Assert
        Assert.AreEqual(enrolledStudents, classDto.EnrolledStudents);
    }

    /// <summary>
    /// Tests that the parameterless constructor successfully creates a valid ClassDto instance
    /// without throwing any exceptions and with all properties initialized to their expected default values.
    /// </summary>
    [TestMethod]
    public void Constructor_Parameterless_CreatesValidInstanceWithDefaultValues()
    {
        // Arrange & Act
        ClassDto result = new ClassDto();

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(string.Empty, result.Id);
        Assert.AreEqual(string.Empty, result.Name);
        Assert.IsNull(result.CourseCode);
        Assert.IsNull(result.ParentClassId);
        Assert.AreEqual(0, result.EnrolledStudents);
        Assert.IsNull(result.LecturerName);
    }

    /// <summary>
    /// Tests that the parameterized constructor correctly handles Unicode characters in string parameters.
    /// Verifies that strings with various Unicode characters including emojis, non-Latin scripts, and special symbols
    /// are correctly assigned without data loss or corruption.
    /// </summary>
    [TestMethod]
    public void Constructor_UnicodeCharacters_AssignsCorrectly()
    {
        // Arrange
        var id = "идентификатор-123";
        var name = "名前-クラス";
        var courseCode = "课程代码-αβγ";
        var parentClassId = "родитель-עברית";
        var enrolledStudents = 15;
        var lecturerName = "Professor 教授 👨‍🏫";

        // Act
        var classDto = new ClassDto(id, name, courseCode, parentClassId, enrolledStudents, lecturerName);

        // Assert
        Assert.AreEqual(id, classDto.Id);
        Assert.AreEqual(name, classDto.Name);
        Assert.AreEqual(courseCode, classDto.CourseCode);
        Assert.AreEqual(parentClassId, classDto.ParentClassId);
        Assert.AreEqual(enrolledStudents, classDto.EnrolledStudents);
        Assert.AreEqual(lecturerName, classDto.LecturerName);
    }

    /// <summary>
    /// Tests that the parameterized constructor correctly handles very long strings for all string parameters.
    /// Verifies that extremely long strings can be assigned without truncation or errors.
    /// </summary>
    [TestMethod]
    public void Constructor_VeryLongStrings_AssignsCorrectly()
    {
        // Arrange
        var id = new string('a', 10000);
        var name = new string('b', 10000);
        var courseCode = new string('c', 10000);
        var parentClassId = new string('d', 10000);
        var enrolledStudents = 500;
        var lecturerName = new string('e', 10000);

        // Act
        var classDto = new ClassDto(id, name, courseCode, parentClassId, enrolledStudents, lecturerName);

        // Assert
        Assert.AreEqual(id, classDto.Id);
        Assert.AreEqual(10000, classDto.Id.Length);
        Assert.AreEqual(name, classDto.Name);
        Assert.AreEqual(10000, classDto.Name.Length);
        Assert.AreEqual(courseCode, classDto.CourseCode);
        Assert.AreEqual(10000, classDto.CourseCode?.Length);
        Assert.AreEqual(parentClassId, classDto.ParentClassId);
        Assert.AreEqual(10000, classDto.ParentClassId?.Length);
        Assert.AreEqual(enrolledStudents, classDto.EnrolledStudents);
        Assert.AreEqual(lecturerName, classDto.LecturerName);
        Assert.AreEqual(10000, classDto.LecturerName?.Length);
    }

    /// <summary>
    /// Tests that the parameterized constructor correctly assigns empty strings for all nullable string parameters.
    /// Verifies the distinction between null and empty string values for nullable parameters.
    /// </summary>
    [TestMethod]
    public void Constructor_EmptyStringsForNullableParameters_AssignsCorrectly()
    {
        // Arrange
        var id = "test-id";
        var name = "Test Name";
        var courseCode = "";
        var parentClassId = "";
        var enrolledStudents = 10;
        var lecturerName = "";

        // Act
        var classDto = new ClassDto(id, name, courseCode, parentClassId, enrolledStudents, lecturerName);

        // Assert
        Assert.AreEqual(id, classDto.Id);
        Assert.AreEqual(name, classDto.Name);
        Assert.AreEqual(string.Empty, classDto.CourseCode);
        Assert.AreEqual(string.Empty, classDto.ParentClassId);
        Assert.AreEqual(enrolledStudents, classDto.EnrolledStudents);
        Assert.AreEqual(string.Empty, classDto.LecturerName);
    }

    /// <summary>
    /// Tests that the parameterized constructor correctly assigns zero as the enrolled students value.
    /// Verifies that zero is a valid value and is correctly assigned without issues.
    /// </summary>
    [TestMethod]
    public void Constructor_ZeroEnrolledStudents_AssignsCorrectly()
    {
        // Arrange
        var id = "empty-class-id";
        var name = "Empty Class";
        var courseCode = "CS000";
        var parentClassId = "parent-id";
        var enrolledStudents = 0;
        var lecturerName = "Dr. Nobody";

        // Act
        var classDto = new ClassDto(id, name, courseCode, parentClassId, enrolledStudents, lecturerName);

        // Assert
        Assert.AreEqual(0, classDto.EnrolledStudents);
        Assert.AreEqual(id, classDto.Id);
        Assert.AreEqual(name, classDto.Name);
        Assert.AreEqual(courseCode, classDto.CourseCode);
        Assert.AreEqual(parentClassId, classDto.ParentClassId);
        Assert.AreEqual(lecturerName, classDto.LecturerName);
    }

    /// <summary>
    /// Tests that the parameterized constructor correctly handles a combination of edge cases simultaneously.
    /// Verifies behavior when empty id, whitespace name, null courseCode, special characters in parentClassId,
    /// negative enrolledStudents, and very long lecturerName are all used together.
    /// </summary>
    [TestMethod]
    public void Constructor_MixedEdgeCases_AssignsCorrectly()
    {
        // Arrange
        var id = "";
        var name = "   ";
        string? courseCode = null;
        var parentClassId = "!@#$%^&*()";
        var enrolledStudents = -50;
        var lecturerName = new string('x', 5000);

        // Act
        var classDto = new ClassDto(id, name, courseCode, parentClassId, enrolledStudents, lecturerName);

        // Assert
        Assert.AreEqual(id, classDto.Id);
        Assert.AreEqual(name, classDto.Name);
        Assert.IsNull(classDto.CourseCode);
        Assert.AreEqual(parentClassId, classDto.ParentClassId);
        Assert.AreEqual(enrolledStudents, classDto.EnrolledStudents);
        Assert.AreEqual(lecturerName, classDto.LecturerName);
        Assert.AreEqual(5000, classDto.LecturerName?.Length);
    }
}