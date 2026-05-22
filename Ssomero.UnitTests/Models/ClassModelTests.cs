using System;

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ssomero;
using Ssomero.Models;

namespace Ssomero.Models.UnitTests;



[TestClass]
public class ClassModelTests
{
    /// <summary>
    /// Tests that the StatusText property returns the correct text representation
    /// for all defined ClassStatus enum values and undefined values.
    /// </summary>
    /// <param name="status">The ClassStatus value to test.</param>
    /// <param name="expectedStatusText">The expected text representation of the status.</param>
    [TestMethod]
    [DataRow(ClassStatus.Active, "Active")]
    [DataRow(ClassStatus.Upcoming, "Upcoming")]
    [DataRow(ClassStatus.Completed, "Completed")]
    [DataRow((ClassStatus)999, "")]
    public void StatusText_WithVariousStatusValues_ReturnsExpectedText(ClassStatus status, string expectedStatusText)
    {
        // Arrange
        var classModel = new ClassModel
        {
            Status = status
        };

        // Act
        var actualStatusText = classModel.StatusText;

        // Assert
        Assert.AreEqual(expectedStatusText, actualStatusText);
    }

    /// <summary>
    /// Tests that the StatusText property returns an empty string for extreme boundary
    /// undefined enum values, ensuring the default case handles edge cases correctly.
    /// </summary>
    /// <param name="undefinedStatus">An undefined ClassStatus value (cast from int extremes).</param>
    [TestMethod]
    [DataRow(int.MinValue)]
    [DataRow(int.MaxValue)]
    [DataRow(-1)]
    [DataRow(3)]
    [DataRow(100)]
    public void StatusText_WithUndefinedEnumBoundaryValues_ReturnsEmptyString(int undefinedStatus)
    {
        // Arrange
        var classModel = new ClassModel
        {
            Status = (ClassStatus)undefinedStatus
        };

        // Act
        var actualStatusText = classModel.StatusText;

        // Assert
        Assert.AreEqual(string.Empty, actualStatusText);
    }

    /// <summary>
    /// Tests that the StatusText property never returns null, always returning
    /// a valid string (either the status text or empty string).
    /// </summary>
    /// <param name="status">The ClassStatus value to test.</param>
    [TestMethod]
    [DataRow(ClassStatus.Active)]
    [DataRow(ClassStatus.Upcoming)]
    [DataRow(ClassStatus.Completed)]
    [DataRow((ClassStatus)(-999))]
    public void StatusText_WithAnyStatusValue_NeverReturnsNull(ClassStatus status)
    {
        // Arrange
        var classModel = new ClassModel
        {
            Status = status
        };

        // Act
        var actualStatusText = classModel.StatusText;

        // Assert
        Assert.IsNotNull(actualStatusText);
    }
}