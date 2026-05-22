using System;
using System.Globalization;

using Microsoft.Maui.Controls;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ssomero.Converters;

namespace Ssomero.Converters.UnitTests
{
    [TestClass]
    public class NullToBoolConverterTests
    {
        /// <summary>
        /// Tests the Convert method with various input values to ensure proper boolean conversion logic.
        /// Verifies null handling, string validation, boolean passthrough, and default behavior for other types.
        /// </summary>
        /// <param name="value">The input value to convert.</param>
        /// <param name="expected">The expected boolean result.</param>
        [TestMethod]
        [DataRow(null, false, DisplayName = "Null value returns false")]
        [DataRow("", false, DisplayName = "Empty string returns false")]
        [DataRow("   ", false, DisplayName = "Whitespace string returns false")]
        [DataRow("\t", false, DisplayName = "Tab string returns false")]
        [DataRow("\n", false, DisplayName = "Newline string returns false")]
        [DataRow("test", true, DisplayName = "Valid string returns true")]
        [DataRow("a", true, DisplayName = "Single character string returns true")]
        [DataRow(true, true, DisplayName = "True boolean returns true")]
        [DataRow(false, false, DisplayName = "False boolean returns false")]
        [DataRow(0, true, DisplayName = "Zero integer returns true")]
        [DataRow(1, true, DisplayName = "Positive integer returns true")]
        [DataRow(-1, true, DisplayName = "Negative integer returns true")]
        public void Convert_VariousInputValues_ReturnsExpectedBooleanResult(object? value, bool expected)
        {
            // Arrange
            var converter = new NullToBoolConverter();
            Type? targetType = null;
            object? parameter = null;
            CultureInfo? culture = null;

            // Act
            object result = converter.Convert(value!, targetType!, parameter, culture!);

            // Assert
            Assert.IsInstanceOfType(result, typeof(bool));
            Assert.AreEqual(expected, (bool)result);
        }

        /// <summary>
        /// Tests the Convert method with extreme numeric values to verify they are treated as truthy.
        /// </summary>
        [TestMethod]
        [DataRow(int.MaxValue, DisplayName = "Int.MaxValue returns true")]
        [DataRow(int.MinValue, DisplayName = "Int.MinValue returns true")]
        [DataRow(long.MaxValue, DisplayName = "Long.MaxValue returns true")]
        [DataRow(long.MinValue, DisplayName = "Long.MinValue returns true")]
        public void Convert_ExtremeNumericValues_ReturnsTrue(object value)
        {
            // Arrange
            var converter = new NullToBoolConverter();

            // Act
            object result = converter.Convert(value, typeof(bool), null!, null!);

            // Assert
            Assert.IsTrue((bool)result);
        }

        /// <summary>
        /// Tests the Convert method with special floating-point values (NaN, Infinity).
        /// These should return true as they are non-null, non-string, non-boolean objects.
        /// </summary>
        [TestMethod]
        [DataRow(double.NaN, DisplayName = "Double.NaN returns true")]
        [DataRow(double.PositiveInfinity, DisplayName = "Double.PositiveInfinity returns true")]
        [DataRow(double.NegativeInfinity, DisplayName = "Double.NegativeInfinity returns true")]
        [DataRow(0.0, DisplayName = "Zero double returns true")]
        public void Convert_SpecialFloatingPointValues_ReturnsTrue(double value)
        {
            // Arrange
            var converter = new NullToBoolConverter();

            // Act
            object result = converter.Convert(value, typeof(bool), null!, null!);

            // Assert
            Assert.IsTrue((bool)result);
        }

        /// <summary>
        /// Tests the Convert method with very long strings to ensure they are properly validated.
        /// A long non-whitespace string should return true.
        /// </summary>
        [TestMethod]
        public void Convert_VeryLongString_ReturnsTrue()
        {
            // Arrange
            var converter = new NullToBoolConverter();
            string veryLongString = new string('a', 10000);

            // Act
            object result = converter.Convert(veryLongString, typeof(bool), null!, null!);

            // Assert
            Assert.IsTrue((bool)result);
        }

        /// <summary>
        /// Tests the Convert method with a very long whitespace string.
        /// Should return false as it contains only whitespace.
        /// </summary>
        [TestMethod]
        public void Convert_VeryLongWhitespaceString_ReturnsFalse()
        {
            // Arrange
            var converter = new NullToBoolConverter();
            string veryLongWhitespace = new string(' ', 10000);

            // Act
            object result = converter.Convert(veryLongWhitespace, typeof(bool), null!, null!);

            // Assert
            Assert.IsFalse((bool)result);
        }

        /// <summary>
        /// Tests the Convert method with strings containing special characters.
        /// Non-whitespace special characters should result in true.
        /// </summary>
        [TestMethod]
        [DataRow("!@#$%", DisplayName = "Special characters return true")]
        [DataRow("hello\0world", DisplayName = "String with null character returns true")]
        [DataRow("text\r\nmore", DisplayName = "String with CRLF returns true")]
        public void Convert_StringsWithSpecialCharacters_ReturnsTrue(string value)
        {
            // Arrange
            var converter = new NullToBoolConverter();

            // Act
            object result = converter.Convert(value, typeof(bool), null!, null!);

            // Assert
            Assert.IsTrue((bool)result);
        }

        /// <summary>
        /// Tests the Convert method with complex object types.
        /// Non-null objects that are not strings or booleans should return true.
        /// </summary>
        [TestMethod]
        public void Convert_ComplexObjectTypes_ReturnsTrue()
        {
            // Arrange
            var converter = new NullToBoolConverter();
            var testObject = new { Name = "Test", Value = 42 };
            var dateTime = DateTime.Now;
            var guid = Guid.NewGuid();

            // Act
            object result1 = converter.Convert(testObject, typeof(bool), null!, null!);
            object result2 = converter.Convert(dateTime, typeof(bool), null!, null!);
            object result3 = converter.Convert(guid, typeof(bool), null!, null!);

            // Assert
            Assert.IsTrue((bool)result1);
            Assert.IsTrue((bool)result2);
            Assert.IsTrue((bool)result3);
        }

        /// <summary>
        /// Tests the Convert method with various targetType, parameter, and culture values.
        /// Since these parameters are not used in the implementation, they should not affect the result.
        /// </summary>
        [TestMethod]
        public void Convert_WithVariousUnusedParameters_ReturnsExpectedResult()
        {
            // Arrange
            var converter = new NullToBoolConverter();
            var testValue = "test";
            var targetType = typeof(string);
            var parameter = "someParameter";
            var culture = CultureInfo.InvariantCulture;

            // Act
            object result = converter.Convert(testValue, targetType, parameter, culture);

            // Assert
            Assert.IsTrue((bool)result);
        }

        /// <summary>
        /// Tests the Convert method with an array/collection type.
        /// Non-null arrays should return true as they are not strings or booleans.
        /// </summary>
        [TestMethod]
        public void Convert_Array_ReturnsTrue()
        {
            // Arrange
            var converter = new NullToBoolConverter();
            var emptyArray = new int[] { };
            var filledArray = new int[] { 1, 2, 3 };

            // Act
            object result1 = converter.Convert(emptyArray, typeof(bool), null!, null!);
            object result2 = converter.Convert(filledArray, typeof(bool), null!, null!);

            // Assert
            Assert.IsTrue((bool)result1);
            Assert.IsTrue((bool)result2);
        }


        /// <summary>
        /// Tests the Convert method with mixed whitespace characters.
        /// Strings containing only various types of whitespace should return false.
        /// </summary>
        [TestMethod]
        [DataRow("  \t  ", DisplayName = "Mixed spaces and tabs return false")]
        [DataRow("\r", DisplayName = "Carriage return returns false")]
        [DataRow("   \n   ", DisplayName = "Spaces with newline return false")]
        public void Convert_MixedWhitespaceStrings_ReturnsFalse(string value)
        {
            // Arrange
            var converter = new NullToBoolConverter();

            // Act
            object result = converter.Convert(value, typeof(bool), null!, null!);

            // Assert
            Assert.IsFalse((bool)result);
        }

        /// <summary>
        /// Tests the Convert method with strings that have leading/trailing whitespace but contain content.
        /// These should return true as they are not null/empty/whitespace-only.
        /// </summary>
        [TestMethod]
        [DataRow("  text  ", DisplayName = "Text with leading/trailing spaces returns true")]
        [DataRow("\tvalue\t", DisplayName = "Text with leading/trailing tabs returns true")]
        [DataRow("\ndata\n", DisplayName = "Text with leading/trailing newlines returns true")]
        public void Convert_StringsWithLeadingTrailingWhitespace_ReturnsTrue(string value)
        {
            // Arrange
            var converter = new NullToBoolConverter();

            // Act
            object result = converter.Convert(value, typeof(bool), null!, null!);

            // Assert
            Assert.IsTrue((bool)result);
        }


        /// <summary>
        /// Tests the Convert method with special floating-point values (NaN, Infinity).
        /// These should return true as they are non-null, non-string, non-boolean objects.
        /// </summary>
        [TestMethod]
        [DataRow(double.NaN, DisplayName = "Double.NaN returns true")]
        [DataRow(double.PositiveInfinity, DisplayName = "Double.PositiveInfinity returns true")]
        [DataRow(double.NegativeInfinity, DisplayName = "Double.NegativeInfinity returns true")]
        [DataRow(0.0, DisplayName = "Zero double returns true")]
        [DataRow(float.NaN, DisplayName = "Float.NaN returns true")]
        [DataRow(float.PositiveInfinity, DisplayName = "Float.PositiveInfinity returns true")]
        [DataRow(float.NegativeInfinity, DisplayName = "Float.NegativeInfinity returns true")]
        [DataRow(0.0f, DisplayName = "Zero float returns true")]
        public void Convert_SpecialFloatingPointValues_ReturnsTrue(object value)
        {
            // Arrange
            var converter = new NullToBoolConverter();

            // Act
            object result = converter.Convert(value, typeof(bool), null!, null!);

            // Assert
            Assert.IsTrue((bool)result);
        }

    }
}