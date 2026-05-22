using System;
using System.Globalization;

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ssomero.Converters;

namespace Ssomero.Converters.UnitTests;
    /// <summary>
    /// Unit tests for the <see cref="ZeroToBoolConverter"/> class.
    /// </summary>
    [TestClass]
    public class ZeroToBoolConverterTests
    {
        /// <summary>
        /// Tests that Convert returns true for int zero and false for all other int values.
        /// </summary>
        /// <param name="value">The integer value to test.</param>
        /// <param name="expected">The expected boolean result.</param>
        [TestMethod]
        [DataRow(0, true, DisplayName = "Int zero returns true")]
        [DataRow(1, false, DisplayName = "Int positive returns false")]
        [DataRow(-1, false, DisplayName = "Int negative returns false")]
        [DataRow(100, false, DisplayName = "Int large positive returns false")]
        [DataRow(-100, false, DisplayName = "Int large negative returns false")]
        [DataRow(int.MaxValue, false, DisplayName = "Int.MaxValue returns false")]
        [DataRow(int.MinValue, false, DisplayName = "Int.MinValue returns false")]
        public void Convert_IntValue_ReturnsExpectedBool(int value, bool expected)
        {
            // Arrange
            var converter = new ZeroToBoolConverter();

            // Act
            var result = converter.Convert(value, typeof(bool), null, null);

            // Assert
            Assert.IsInstanceOfType(result, typeof(bool));
            Assert.AreEqual(expected, (bool)result);
        }

        /// <summary>
        /// Tests that Convert returns true for long zero and false for all other long values.
        /// </summary>
        /// <param name="value">The long value to test.</param>
        /// <param name="expected">The expected boolean result.</param>
        [TestMethod]
        [DataRow(0L, true, DisplayName = "Long zero returns true")]
        [DataRow(1L, false, DisplayName = "Long positive returns false")]
        [DataRow(-1L, false, DisplayName = "Long negative returns false")]
        [DataRow(100L, false, DisplayName = "Long large positive returns false")]
        [DataRow(-100L, false, DisplayName = "Long large negative returns false")]
        [DataRow(long.MaxValue, false, DisplayName = "Long.MaxValue returns false")]
        [DataRow(long.MinValue, false, DisplayName = "Long.MinValue returns false")]
        public void Convert_LongValue_ReturnsExpectedBool(long value, bool expected)
        {
            // Arrange
            var converter = new ZeroToBoolConverter();

            // Act
            var result = converter.Convert(value, typeof(bool), null, null);

            // Assert
            Assert.IsInstanceOfType(result, typeof(bool));
            Assert.AreEqual(expected, (bool)result);
        }

        /// <summary>
        /// Tests that Convert returns false when the value is null.
        /// </summary>
        [TestMethod]
        public void Convert_NullValue_ReturnsFalse()
        {
            // Arrange
            var converter = new ZeroToBoolConverter();

            // Act
            var result = converter.Convert(null, typeof(bool), null, null);

            // Assert
            Assert.IsInstanceOfType(result, typeof(bool));
            Assert.IsFalse((bool)result);
        }

        /// <summary>
        /// Tests that Convert returns false for double values since double is not a supported type.
        /// </summary>
        /// <param name="value">The double value to test.</param>
        [TestMethod]
        [DataRow(0.0, DisplayName = "Double zero returns false")]
        [DataRow(1.5, DisplayName = "Double positive returns false")]
        [DataRow(-1.5, DisplayName = "Double negative returns false")]
        [DataRow(double.NaN, DisplayName = "Double.NaN returns false")]
        [DataRow(double.PositiveInfinity, DisplayName = "Double.PositiveInfinity returns false")]
        [DataRow(double.NegativeInfinity, DisplayName = "Double.NegativeInfinity returns false")]
        [DataRow(double.MaxValue, DisplayName = "Double.MaxValue returns false")]
        [DataRow(double.MinValue, DisplayName = "Double.MinValue returns false")]
        public void Convert_DoubleValue_ReturnsFalse(double value)
        {
            // Arrange
            var converter = new ZeroToBoolConverter();

            // Act
            var result = converter.Convert(value, typeof(bool), null, null);

            // Assert
            Assert.IsInstanceOfType(result, typeof(bool));
            Assert.IsFalse((bool)result);
        }

        /// <summary>
        /// Tests that Convert returns false for float values since float is not a supported type.
        /// </summary>
        /// <param name="value">The float value to test.</param>
        [TestMethod]
        [DataRow(0.0f, DisplayName = "Float zero returns false")]
        [DataRow(1.5f, DisplayName = "Float positive returns false")]
        [DataRow(-1.5f, DisplayName = "Float negative returns false")]
        [DataRow(float.NaN, DisplayName = "Float.NaN returns false")]
        [DataRow(float.PositiveInfinity, DisplayName = "Float.PositiveInfinity returns false")]
        [DataRow(float.NegativeInfinity, DisplayName = "Float.NegativeInfinity returns false")]
        public void Convert_FloatValue_ReturnsFalse(float value)
        {
            // Arrange
            var converter = new ZeroToBoolConverter();

            // Act
            var result = converter.Convert(value, typeof(bool), null, null);

            // Assert
            Assert.IsInstanceOfType(result, typeof(bool));
            Assert.IsFalse((bool)result);
        }

        /// <summary>
        /// Tests that Convert returns false for decimal values since decimal is not a supported type.
        /// </summary>
        [TestMethod]
        public void Convert_DecimalValue_ReturnsFalse()
        {
            // Arrange
            var converter = new ZeroToBoolConverter();

            // Act & Assert
            Assert.IsFalse((bool)converter.Convert(0m, typeof(bool), null, null)!);
            Assert.IsFalse((bool)converter.Convert(1.5m, typeof(bool), null, null)!);
            Assert.IsFalse((bool)converter.Convert(-1.5m, typeof(bool), null, null)!);
            Assert.IsFalse((bool)converter.Convert(decimal.MaxValue, typeof(bool), null, null)!);
            Assert.IsFalse((bool)converter.Convert(decimal.MinValue, typeof(bool), null, null)!);
        }

        /// <summary>
        /// Tests that Convert returns false for short values since short is not a supported type.
        /// </summary>
        [TestMethod]
        public void Convert_ShortValue_ReturnsFalse()
        {
            // Arrange
            var converter = new ZeroToBoolConverter();

            // Act & Assert
            Assert.IsFalse((bool)converter.Convert((short)0, typeof(bool), null, null)!);
            Assert.IsFalse((bool)converter.Convert((short)1, typeof(bool), null, null)!);
            Assert.IsFalse((bool)converter.Convert((short)-1, typeof(bool), null, null)!);
            Assert.IsFalse((bool)converter.Convert(short.MaxValue, typeof(bool), null, null)!);
            Assert.IsFalse((bool)converter.Convert(short.MinValue, typeof(bool), null, null)!);
        }

        /// <summary>
        /// Tests that Convert returns false for byte values since byte is not a supported type.
        /// </summary>
        [TestMethod]
        public void Convert_ByteValue_ReturnsFalse()
        {
            // Arrange
            var converter = new ZeroToBoolConverter();

            // Act & Assert
            Assert.IsFalse((bool)converter.Convert((byte)0, typeof(bool), null, null)!);
            Assert.IsFalse((bool)converter.Convert((byte)1, typeof(bool), null, null)!);
            Assert.IsFalse((bool)converter.Convert(byte.MaxValue, typeof(bool), null, null)!);
            Assert.IsFalse((bool)converter.Convert(byte.MinValue, typeof(bool), null, null)!);
        }

        /// <summary>
        /// Tests that Convert returns false for non-numeric types including string, bool, and object.
        /// </summary>
        [TestMethod]
        public void Convert_NonNumericTypes_ReturnsFalse()
        {
            // Arrange
            var converter = new ZeroToBoolConverter();

            // Act & Assert
            Assert.IsFalse((bool)converter.Convert("0", typeof(bool), null, null)!);
            Assert.IsFalse((bool)converter.Convert("", typeof(bool), null, null)!);
            Assert.IsFalse((bool)converter.Convert("test", typeof(bool), null, null)!);
            Assert.IsFalse((bool)converter.Convert(true, typeof(bool), null, null)!);
            Assert.IsFalse((bool)converter.Convert(false, typeof(bool), null, null)!);
            Assert.IsFalse((bool)converter.Convert(new object(), typeof(bool), null, null)!);
        }

        /// <summary>
        /// Tests that Convert correctly evaluates int zero regardless of the targetType argument.
        /// </summary>
        [TestMethod]
        public void Convert_IntZero_WorksRegardlessOfTargetType()
        {
            // Arrange
            var converter = new ZeroToBoolConverter();

            // Act & Assert
            Assert.IsTrue((bool)converter.Convert(0, typeof(bool), null, null)!);
            Assert.IsTrue((bool)converter.Convert(0, typeof(string), null, null)!);
            Assert.IsTrue((bool)converter.Convert(0, typeof(int), null, null)!);
            Assert.IsTrue((bool)converter.Convert(0, null, null, null)!);
        }

        /// <summary>
        /// Tests that Convert correctly evaluates int values regardless of the parameter and culture arguments.
        /// </summary>
        [TestMethod]
        public void Convert_IntValue_WorksRegardlessOfParameterAndCulture()
        {
            // Arrange
            var converter = new ZeroToBoolConverter();

            // Act & Assert
            Assert.IsTrue((bool)converter.Convert(0, typeof(bool), "param", CultureInfo.InvariantCulture)!);
            Assert.IsTrue((bool)converter.Convert(0, typeof(bool), 123, CultureInfo.CurrentCulture)!);
            Assert.IsFalse((bool)converter.Convert(1, typeof(bool), new object(), new CultureInfo("en-US"))!);
        }


        /// <summary>
        /// Tests that ConvertBack throws <see cref="NotSupportedException"/> for a boolean true value.
        /// </summary>
        [TestMethod]
        public void ConvertBack_BoolTrue_ThrowsNotSupportedException()
        {
            // Arrange
            var converter = new ZeroToBoolConverter();

            // Act & Assert
            try { converter.ConvertBack(true, typeof(int), null!, null!); Assert.Fail("Expected NotSupportedException."); } catch (NotSupportedException) { }
        }

        /// <summary>
        /// Tests that ConvertBack throws <see cref="NotSupportedException"/> for a boolean false value.
        /// </summary>
        [TestMethod]
        public void ConvertBack_BoolFalse_ThrowsNotSupportedException()
        {
            // Arrange
            var converter = new ZeroToBoolConverter();

            // Act & Assert
            try { converter.ConvertBack(false, typeof(int), null!, null!); Assert.Fail("Expected NotSupportedException."); } catch (NotSupportedException) { }
        }

        /// <summary>
        /// Tests that ConvertBack throws <see cref="NotSupportedException"/> when value is null.
        /// </summary>
        [TestMethod]
        public void ConvertBack_NullValue_ThrowsNotSupportedException()
        {
            // Arrange
            var converter = new ZeroToBoolConverter();

            // Act & Assert
            try { converter.ConvertBack(null!, typeof(bool), null!, null!); Assert.Fail("Expected NotSupportedException."); } catch (NotSupportedException) { }
        }

        /// <summary>
        /// Tests that ConvertBack throws <see cref="NotSupportedException"/> when targetType is null.
        /// </summary>
        [TestMethod]
        public void ConvertBack_NullTargetType_ThrowsNotSupportedException()
        {
            // Arrange
            var converter = new ZeroToBoolConverter();

            // Act & Assert
            try { converter.ConvertBack(true, null!, null!, null!); Assert.Fail("Expected NotSupportedException."); } catch (NotSupportedException) { }
        }

        /// <summary>
        /// Tests that ConvertBack throws <see cref="NotSupportedException"/> for integer and string values.
        /// </summary>
        [TestMethod]
        public void ConvertBack_IntAndStringValues_ThrowsNotSupportedException()
        {
            // Arrange
            var converter = new ZeroToBoolConverter();

            // Act & Assert
            try { converter.ConvertBack(0, typeof(bool), null!, null!); Assert.Fail("Expected NotSupportedException."); } catch (NotSupportedException) { }
            try { converter.ConvertBack(1, typeof(int), null!, null!); Assert.Fail("Expected NotSupportedException."); } catch (NotSupportedException) { }
            try { converter.ConvertBack(-1, typeof(int), null!, null!); Assert.Fail("Expected NotSupportedException."); } catch (NotSupportedException) { }
            try { converter.ConvertBack("test", typeof(string), null!, null!); Assert.Fail("Expected NotSupportedException."); } catch (NotSupportedException) { }
            try { converter.ConvertBack("", typeof(string), null!, null!); Assert.Fail("Expected NotSupportedException."); } catch (NotSupportedException) { }
        }

        /// <summary>
        /// Tests that ConvertBack throws <see cref="NotSupportedException"/> regardless of the culture argument.
        /// </summary>
        [TestMethod]
        public void ConvertBack_VariousCultures_ThrowsNotSupportedException()
        {
            // Arrange
            var converter = new ZeroToBoolConverter();

            // Act & Assert
            try { converter.ConvertBack(true, typeof(int), null!, null!); Assert.Fail("Expected NotSupportedException."); } catch (NotSupportedException) { }
            try { converter.ConvertBack(true, typeof(int), null!, CultureInfo.InvariantCulture); Assert.Fail("Expected NotSupportedException."); } catch (NotSupportedException) { }
            try { converter.ConvertBack(true, typeof(int), null!, CultureInfo.CurrentCulture); Assert.Fail("Expected NotSupportedException."); } catch (NotSupportedException) { }
            try { converter.ConvertBack(true, typeof(int), null!, new CultureInfo("en-US")); Assert.Fail("Expected NotSupportedException."); } catch (NotSupportedException) { }
            try { converter.ConvertBack(false, typeof(long), "param", new CultureInfo("fr-FR")); Assert.Fail("Expected NotSupportedException."); } catch (NotSupportedException) { }
        }

            /// <summary>
            /// Tests that ConvertBack throws <see cref="NotSupportedException"/> for complex and reference type values.
            /// </summary>
            [TestMethod]
            public void ConvertBack_ComplexObjectTypes_ThrowsNotSupportedException()
            {
                // Arrange
                var converter = new ZeroToBoolConverter();

        // Act & Assert
        try { converter.ConvertBack(new { Property = "value" }, typeof(object), null!, null!); Assert.Fail("Expected NotSupportedException."); } catch (NotSupportedException) { }
                try { converter.ConvertBack(DateTime.Now, typeof(DateTime), null!, null!); Assert.Fail("Expected NotSupportedException."); } catch (NotSupportedException) { }
                try { converter.ConvertBack(new int[] { 1, 2, 3 }, typeof(int[]), null!, null!); Assert.Fail("Expected NotSupportedException."); } catch (NotSupportedException) { }
            }
        }