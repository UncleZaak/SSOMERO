using System;
using System.Globalization;

using Microsoft.Maui.Graphics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ssomero.Converters;

namespace Ssomero.Converters.UnitTests;



/// <summary>
/// Unit tests for the <see cref="StatusToColorConverter"/> class.
/// </summary>
[TestClass]
public class StatusToColorConverterTests
{
    /// <summary>
    /// Tests that the Convert method returns the correct color for known status values.
    /// Verifies that "Active", "Suspended", and "Deactivated" map to their respective colors,
    /// and unknown values map to the default gray color.
    /// </summary>
    /// <param name="statusValue">The status value to convert.</param>
    /// <param name="expectedColorHex">The expected color hex value.</param>
    [TestMethod]
    [DataRow("Active", "#22C55E")]
    [DataRow("Suspended", "#F59E0B")]
    [DataRow("Deactivated", "#9CA3AF")]
    [DataRow("Unknown", "#6B7280")]
    [DataRow("", "#6B7280")]
    [DataRow("InProgress", "#6B7280")]
    public void Convert_KnownAndUnknownStatuses_ReturnsExpectedColor(string statusValue, string expectedColorHex)
    {
        // Arrange
        var converter = new StatusToColorConverter();
        var expectedColor = Color.FromArgb(expectedColorHex);

        // Act
        var result = converter.Convert(statusValue, typeof(Color), null, CultureInfo.InvariantCulture);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsInstanceOfType(result, typeof(Color));
        var actualColor = (Color)result;
        Assert.AreEqual(expectedColor.ToHex(), actualColor.ToHex());
    }

    /// <summary>
    /// Tests that the Convert method returns the default color when value is null.
    /// Verifies that null input is handled gracefully and returns the default gray color.
    /// </summary>
    [TestMethod]
    public void Convert_NullValue_ReturnsDefaultColor()
    {
        // Arrange
        var converter = new StatusToColorConverter();
        var expectedColor = Color.FromArgb("#6B7280");

        // Act
        var result = converter.Convert(null, typeof(Color), null, CultureInfo.InvariantCulture);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsInstanceOfType(result, typeof(Color));
        var actualColor = (Color)result;
        Assert.AreEqual(expectedColor.ToHex(), actualColor.ToHex());
    }

    /// <summary>
    /// Tests that the Convert method is case-sensitive for status values.
    /// Verifies that status values with different casing do not match the exact status strings
    /// and return the default color instead.
    /// </summary>
    /// <param name="statusValue">The status value with different casing.</param>
    [TestMethod]
    [DataRow("active")]
    [DataRow("ACTIVE")]
    [DataRow("suspended")]
    [DataRow("SUSPENDED")]
    [DataRow("deactivated")]
    [DataRow("DEACTIVATED")]
    public void Convert_CaseVariations_ReturnsDefaultColor(string statusValue)
    {
        // Arrange
        var converter = new StatusToColorConverter();
        var expectedColor = Color.FromArgb("#6B7280");

        // Act
        var result = converter.Convert(statusValue, typeof(Color), null, CultureInfo.InvariantCulture);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsInstanceOfType(result, typeof(Color));
        var actualColor = (Color)result;
        Assert.AreEqual(expectedColor.ToHex(), actualColor.ToHex());
    }

    /// <summary>
    /// Tests that the Convert method handles whitespace-only strings correctly.
    /// Verifies that whitespace strings return the default color.
    /// </summary>
    /// <param name="statusValue">The whitespace string value.</param>
    [TestMethod]
    [DataRow(" ")]
    [DataRow("  ")]
    [DataRow("\t")]
    [DataRow("\n")]
    [DataRow("\r\n")]
    public void Convert_WhitespaceStrings_ReturnsDefaultColor(string statusValue)
    {
        // Arrange
        var converter = new StatusToColorConverter();
        var expectedColor = Color.FromArgb("#6B7280");

        // Act
        var result = converter.Convert(statusValue, typeof(Color), null, CultureInfo.InvariantCulture);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsInstanceOfType(result, typeof(Color));
        var actualColor = (Color)result;
        Assert.AreEqual(expectedColor.ToHex(), actualColor.ToHex());
    }

    /// <summary>
    /// Tests that the Convert method handles non-string objects by calling ToString().
    /// Verifies that objects are converted to their string representation before matching.
    /// </summary>
    /// <param name="value">The non-string value to convert.</param>
    /// <param name="expectedColorHex">The expected color hex value.</param>
    [TestMethod]
    [DataRow(123, "#6B7280")]
    [DataRow(true, "#6B7280")]
    [DataRow(false, "#6B7280")]
    public void Convert_NonStringObjects_CallsToStringAndReturnsDefaultColor(object value, string expectedColorHex)
    {
        // Arrange
        var converter = new StatusToColorConverter();
        var expectedColor = Color.FromArgb(expectedColorHex);

        // Act
        var result = converter.Convert(value, typeof(Color), null, CultureInfo.InvariantCulture);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsInstanceOfType(result, typeof(Color));
        var actualColor = (Color)result;
        Assert.AreEqual(expectedColor.ToHex(), actualColor.ToHex());
    }

    /// <summary>
    /// Tests that the Convert method handles strings with leading or trailing whitespace.
    /// Verifies that whitespace prevents exact matching and returns the default color.
    /// </summary>
    /// <param name="statusValue">The status value with whitespace.</param>
    [TestMethod]
    [DataRow(" Active")]
    [DataRow("Active ")]
    [DataRow(" Suspended")]
    [DataRow("Suspended ")]
    [DataRow(" Deactivated")]
    [DataRow("Deactivated ")]
    public void Convert_StatusWithWhitespace_ReturnsDefaultColor(string statusValue)
    {
        // Arrange
        var converter = new StatusToColorConverter();
        var expectedColor = Color.FromArgb("#6B7280");

        // Act
        var result = converter.Convert(statusValue, typeof(Color), null, CultureInfo.InvariantCulture);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsInstanceOfType(result, typeof(Color));
        var actualColor = (Color)result;
        Assert.AreEqual(expectedColor.ToHex(), actualColor.ToHex());
    }

    /// <summary>
    /// Tests that the Convert method handles very long strings correctly.
    /// Verifies that extremely long status strings return the default color.
    /// </summary>
    [TestMethod]
    public void Convert_VeryLongString_ReturnsDefaultColor()
    {
        // Arrange
        var converter = new StatusToColorConverter();
        var veryLongString = new string('A', 10000);
        var expectedColor = Color.FromArgb("#6B7280");

        // Act
        var result = converter.Convert(veryLongString, typeof(Color), null, CultureInfo.InvariantCulture);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsInstanceOfType(result, typeof(Color));
        var actualColor = (Color)result;
        Assert.AreEqual(expectedColor.ToHex(), actualColor.ToHex());
    }

    /// <summary>
    /// Tests that the Convert method handles strings with special characters correctly.
    /// Verifies that special characters in status strings return the default color.
    /// </summary>
    /// <param name="statusValue">The status value with special characters.</param>
    [TestMethod]
    [DataRow("Active!")]
    [DataRow("@Suspended")]
    [DataRow("Deactivated#")]
    [DataRow("Active\u0000")]
    [DataRow("Active\u001F")]
    public void Convert_SpecialCharacters_ReturnsDefaultColor(string statusValue)
    {
        // Arrange
        var converter = new StatusToColorConverter();
        var expectedColor = Color.FromArgb("#6B7280");

        // Act
        var result = converter.Convert(statusValue, typeof(Color), null, CultureInfo.InvariantCulture);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsInstanceOfType(result, typeof(Color));
        var actualColor = (Color)result;
        Assert.AreEqual(expectedColor.ToHex(), actualColor.ToHex());
    }

    /// <summary>
    /// Tests that the Convert method ignores the targetType parameter.
    /// Verifies that different target types do not affect the conversion result.
    /// </summary>
    [TestMethod]
    public void Convert_DifferentTargetTypes_ReturnsExpectedColor()
    {
        // Arrange
        var converter = new StatusToColorConverter();
        var expectedColor = Color.FromArgb("#22C55E");

        // Act
        var resultWithColorType = converter.Convert("Active", typeof(Color), null, CultureInfo.InvariantCulture);
        var resultWithStringType = converter.Convert("Active", typeof(string), null, CultureInfo.InvariantCulture);
        var resultWithObjectType = converter.Convert("Active", typeof(object), null, CultureInfo.InvariantCulture);

        // Assert
        Assert.IsInstanceOfType(resultWithColorType, typeof(Color));
        Assert.IsInstanceOfType(resultWithStringType, typeof(Color));
        Assert.IsInstanceOfType(resultWithObjectType, typeof(Color));
        Assert.AreEqual(expectedColor.ToHex(), ((Color)resultWithColorType).ToHex());
        Assert.AreEqual(expectedColor.ToHex(), ((Color)resultWithStringType).ToHex());
        Assert.AreEqual(expectedColor.ToHex(), ((Color)resultWithObjectType).ToHex());
    }

    /// <summary>
    /// Tests that the Convert method ignores the parameter argument.
    /// Verifies that different parameter values do not affect the conversion result.
    /// </summary>
    [TestMethod]
    public void Convert_DifferentParameters_ReturnsExpectedColor()
    {
        // Arrange
        var converter = new StatusToColorConverter();
        var expectedColor = Color.FromArgb("#22C55E");

        // Act
        var resultWithNullParam = converter.Convert("Active", typeof(Color), null, CultureInfo.InvariantCulture);
        var resultWithStringParam = converter.Convert("Active", typeof(Color), "someParam", CultureInfo.InvariantCulture);
        var resultWithIntParam = converter.Convert("Active", typeof(Color), 42, CultureInfo.InvariantCulture);

        // Assert
        Assert.IsInstanceOfType(resultWithNullParam, typeof(Color));
        Assert.IsInstanceOfType(resultWithStringParam, typeof(Color));
        Assert.IsInstanceOfType(resultWithIntParam, typeof(Color));
        Assert.AreEqual(expectedColor.ToHex(), ((Color)resultWithNullParam).ToHex());
        Assert.AreEqual(expectedColor.ToHex(), ((Color)resultWithStringParam).ToHex());
        Assert.AreEqual(expectedColor.ToHex(), ((Color)resultWithIntParam).ToHex());
    }

    /// <summary>
    /// Tests that the Convert method ignores the culture parameter.
    /// Verifies that different culture values do not affect the conversion result.
    /// </summary>
    [TestMethod]
    public void Convert_DifferentCultures_ReturnsExpectedColor()
    {
        // Arrange
        var converter = new StatusToColorConverter();
        var expectedColor = Color.FromArgb("#22C55E");

        // Act
        var resultWithInvariantCulture = converter.Convert("Active", typeof(Color), null, CultureInfo.InvariantCulture);
        var resultWithEnUsCulture = converter.Convert("Active", typeof(Color), null, new CultureInfo("en-US"));
        var resultWithFrFrCulture = converter.Convert("Active", typeof(Color), null, new CultureInfo("fr-FR"));

        // Assert
        Assert.IsInstanceOfType(resultWithInvariantCulture, typeof(Color));
        Assert.IsInstanceOfType(resultWithEnUsCulture, typeof(Color));
        Assert.IsInstanceOfType(resultWithFrFrCulture, typeof(Color));
        Assert.AreEqual(expectedColor.ToHex(), ((Color)resultWithInvariantCulture).ToHex());
        Assert.AreEqual(expectedColor.ToHex(), ((Color)resultWithEnUsCulture).ToHex());
        Assert.AreEqual(expectedColor.ToHex(), ((Color)resultWithFrFrCulture).ToHex());
    }

}