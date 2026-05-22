using System;
using System.Globalization;

using Microsoft.Maui.Controls;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ssomero.Converters;

namespace Ssomero.Converters.UnitTests
{
    [TestClass]
    public class PercentToDoubleConverterTests
    {
        /// <summary>
        /// Tests that Convert returns 0.0 when value is null.
        /// </summary>
        [TestMethod]
        public void Convert_NullValue_ReturnsZero()
        {
            // Arrange
            var converter = new PercentToDoubleConverter();
            object? value = null;

            // Act
            var result = converter.Convert(value!, typeof(double), null!, CultureInfo.InvariantCulture);

            // Assert
            Assert.AreEqual(0.0, result);
        }

        /// <summary>
        /// Tests that Convert correctly handles integer values by dividing by 100 and clamping to [0.0, 1.0].
        /// </summary>
        /// <param name="input">The integer input value.</param>
        /// <param name="expected">The expected double output.</param>
        [TestMethod]
        [DataRow(0, 0.0)]
        [DataRow(50, 0.5)]
        [DataRow(100, 1.0)]
        [DataRow(-50, 0.0)]
        [DataRow(-100, 0.0)]
        [DataRow(200, 1.0)]
        [DataRow(150, 1.0)]
        [DataRow(int.MinValue, 0.0)]
        [DataRow(int.MaxValue, 1.0)]
        public void Convert_IntegerValues_ReturnsClampedPercentage(int input, double expected)
        {
            // Arrange
            var converter = new PercentToDoubleConverter();

            // Act
            var result = converter.Convert(input, typeof(double), null!, CultureInfo.InvariantCulture);

            // Assert
            Assert.AreEqual(expected, result);
        }

        /// <summary>
        /// Tests that Convert correctly handles double values by clamping to [0.0, 1.0] without division.
        /// </summary>
        /// <param name="input">The double input value.</param>
        /// <param name="expected">The expected double output.</param>
        [TestMethod]
        [DataRow(0.0, 0.0)]
        [DataRow(0.5, 0.5)]
        [DataRow(1.0, 1.0)]
        [DataRow(-0.5, 0.0)]
        [DataRow(-1.0, 0.0)]
        [DataRow(2.0, 1.0)]
        [DataRow(1.5, 1.0)]
        public void Convert_DoubleValues_ReturnsClampedValue(double input, double expected)
        {
            // Arrange
            var converter = new PercentToDoubleConverter();

            // Act
            var result = converter.Convert(input, typeof(double), null!, CultureInfo.InvariantCulture);

            // Assert
            Assert.AreEqual(expected, result);
        }

        /// <summary>
        /// Tests that Convert correctly handles extreme double values by clamping to [0.0, 1.0].
        /// </summary>
        [TestMethod]
        public void Convert_DoubleMinValue_ReturnsZero()
        {
            // Arrange
            var converter = new PercentToDoubleConverter();

            // Act
            var result = converter.Convert(double.MinValue, typeof(double), null!, CultureInfo.InvariantCulture);

            // Assert
            Assert.AreEqual(0.0, result);
        }

        /// <summary>
        /// Tests that Convert correctly handles extreme double values by clamping to [0.0, 1.0].
        /// </summary>
        [TestMethod]
        public void Convert_DoubleMaxValue_ReturnsOne()
        {
            // Arrange
            var converter = new PercentToDoubleConverter();

            // Act
            var result = converter.Convert(double.MaxValue, typeof(double), null!, CultureInfo.InvariantCulture);

            // Assert
            Assert.AreEqual(1.0, result);
        }

        /// <summary>
        /// Tests that Convert returns 0.0 when input is double.NaN (parsing fails).
        /// </summary>
        [TestMethod]
        public void Convert_DoubleNaN_ReturnsZero()
        {
            // Arrange
            var converter = new PercentToDoubleConverter();

            // Act
            var result = converter.Convert(double.NaN, typeof(double), null!, CultureInfo.InvariantCulture);

            // Assert
            Assert.AreEqual(0.0, result);
        }

        /// <summary>
        /// Tests that Convert returns 0.0 when input is double.PositiveInfinity (parsing behavior varies).
        /// </summary>
        [TestMethod]
        public void Convert_DoublePositiveInfinity_ReturnsOne()
        {
            // Arrange
            var converter = new PercentToDoubleConverter();

            // Act
            var result = converter.Convert(double.PositiveInfinity, typeof(double), null!, CultureInfo.InvariantCulture);

            // Assert
            Assert.AreEqual(1.0, result);
        }

        /// <summary>
        /// Tests that Convert returns 0.0 when input is double.NegativeInfinity (parsing behavior varies).
        /// </summary>
        [TestMethod]
        public void Convert_DoubleNegativeInfinity_ReturnsZero()
        {
            // Arrange
            var converter = new PercentToDoubleConverter();

            // Act
            var result = converter.Convert(double.NegativeInfinity, typeof(double), null!, CultureInfo.InvariantCulture);

            // Assert
            Assert.AreEqual(0.0, result);
        }

        /// <summary>
        /// Tests that Convert correctly handles string representations of integers.
        /// </summary>
        /// <param name="input">The string input value.</param>
        /// <param name="expected">The expected double output.</param>
        [TestMethod]
        [DataRow("0", 0.0)]
        [DataRow("50", 0.5)]
        [DataRow("100", 1.0)]
        [DataRow("-50", 0.0)]
        [DataRow("200", 1.0)]
        public void Convert_IntegerStrings_ReturnsClampedPercentage(string input, double expected)
        {
            // Arrange
            var converter = new PercentToDoubleConverter();

            // Act
            var result = converter.Convert(input, typeof(double), null!, CultureInfo.InvariantCulture);

            // Assert
            Assert.AreEqual(expected, result);
        }

        /// <summary>
        /// Tests that Convert correctly handles string representations of doubles.
        /// </summary>
        /// <param name="input">The string input value.</param>
        /// <param name="expected">The expected double output.</param>
        [TestMethod]
        [DataRow("0.0", 0.0)]
        [DataRow("0.5", 0.5)]
        [DataRow("1.0", 1.0)]
        [DataRow("-0.5", 0.0)]
        [DataRow("2.0", 1.0)]
        public void Convert_DoubleStrings_ReturnsClampedValue(string input, double expected)
        {
            // Arrange
            var converter = new PercentToDoubleConverter();

            // Act
            var result = converter.Convert(input, typeof(double), null!, CultureInfo.InvariantCulture);

            // Assert
            Assert.AreEqual(expected, result);
        }

        /// <summary>
        /// Tests that Convert returns 0.0 for invalid string inputs that cannot be parsed.
        /// </summary>
        /// <param name="input">The invalid string input.</param>
        [TestMethod]
        [DataRow("")]
        [DataRow("   ")]
        [DataRow("abc")]
        [DataRow("!@#$%")]
        [DataRow("not a number")]
        [DataRow("12.34.56")]
        public void Convert_InvalidStrings_ReturnsZero(string input)
        {
            // Arrange
            var converter = new PercentToDoubleConverter();

            // Act
            var result = converter.Convert(input, typeof(double), null!, CultureInfo.InvariantCulture);

            // Assert
            Assert.AreEqual(0.0, result);
        }


        /// <summary>
        /// Tests that Convert handles boolean values by returning 0.0 (ToString() produces non-numeric string).
        /// </summary>
        /// <param name="input">The boolean input value.</param>
        [TestMethod]
        [DataRow(true)]
        [DataRow(false)]
        public void Convert_BooleanValues_ReturnsZero(bool input)
        {
            // Arrange
            var converter = new PercentToDoubleConverter();

            // Act
            var result = converter.Convert(input, typeof(double), null!, CultureInfo.InvariantCulture);

            // Assert
            Assert.AreEqual(0.0, result);
        }

        /// <summary>
        /// Tests that Convert handles DateTime values by returning 0.0 (ToString() produces non-numeric string).
        /// </summary>
        [TestMethod]
        public void Convert_DateTimeValue_ReturnsZero()
        {
            // Arrange
            var converter = new PercentToDoubleConverter();
            var input = new DateTime(2024, 1, 1);

            // Act
            var result = converter.Convert(input, typeof(double), null!, CultureInfo.InvariantCulture);

            // Assert
            Assert.AreEqual(0.0, result);
        }

        /// <summary>
        /// Tests that Convert handles custom objects by calling ToString() and attempting to parse.
        /// </summary>
        [TestMethod]
        public void Convert_CustomObjectWithNumericToString_ReturnsClampedValue()
        {
            // Arrange
            var converter = new PercentToDoubleConverter();
            var input = new CustomNumericObject(75);

            // Act
            var result = converter.Convert(input, typeof(double), null!, CultureInfo.InvariantCulture);

            // Assert
            Assert.AreEqual(0.75, result);
        }

        /// <summary>
        /// Tests that Convert handles strings with leading and trailing whitespace correctly.
        /// </summary>
        /// <param name="input">The string input with whitespace.</param>
        /// <param name="expected">The expected double output.</param>
        [TestMethod]
        [DataRow("  50  ", 0.5)]
        [DataRow("\t100\t", 1.0)]
        [DataRow(" 0 ", 0.0)]
        [DataRow("  0.5  ", 0.5)]
        public void Convert_StringsWithWhitespace_ReturnsParsedValue(string input, double expected)
        {
            // Arrange
            var converter = new PercentToDoubleConverter();

            // Act
            var result = converter.Convert(input, typeof(double), null!, CultureInfo.InvariantCulture);

            // Assert
            Assert.AreEqual(expected, result);
        }

        /// <summary>
        /// Tests that Convert handles strings with positive sign correctly.
        /// </summary>
        /// <param name="input">The string input with positive sign.</param>
        /// <param name="expected">The expected double output.</param>
        [TestMethod]
        [DataRow("+50", 0.5)]
        [DataRow("+100", 1.0)]
        [DataRow("+0.5", 0.5)]
        public void Convert_StringsWithPlusSign_ReturnsParsedValue(string input, double expected)
        {
            // Arrange
            var converter = new PercentToDoubleConverter();

            // Act
            var result = converter.Convert(input, typeof(double), null!, CultureInfo.InvariantCulture);

            // Assert
            Assert.AreEqual(expected, result);
        }

        /// <summary>
        /// Tests that Convert handles decimal strings correctly by parsing as double and clamping.
        /// </summary>
        /// <param name="input">The decimal string input.</param>
        /// <param name="expected">The expected double output.</param>
        [TestMethod]
        [DataRow("0.25", 0.25)]
        [DataRow("0.75", 0.75)]
        [DataRow("1.5", 1.0)]
        [DataRow("0.999", 0.999)]
        public void Convert_DecimalStrings_ReturnsClampedDoubleValue(string input, double expected)
        {
            // Arrange
            var converter = new PercentToDoubleConverter();

            // Act
            var result = converter.Convert(input, typeof(double), null!, CultureInfo.InvariantCulture);

            // Assert
            Assert.AreEqual(expected, result);
        }

        /// <summary>
        /// Tests that Convert behavior is not affected by different culture parameter values.
        /// </summary>
        [TestMethod]
        public void Convert_DifferentCultures_ProducesSameResult()
        {
            // Arrange
            var converter = new PercentToDoubleConverter();
            var input = 50;

            // Act
            var resultInvariant = converter.Convert(input, typeof(double), null!, CultureInfo.InvariantCulture);
            var resultEnUs = converter.Convert(input, typeof(double), null!, CultureInfo.GetCultureInfo("en-US"));
            var resultFrFr = converter.Convert(input, typeof(double), null!, CultureInfo.GetCultureInfo("fr-FR"));

            // Assert
            Assert.AreEqual(0.5, resultInvariant);
            Assert.AreEqual(0.5, resultEnUs);
            Assert.AreEqual(0.5, resultFrFr);
        }

        /// <summary>
        /// Tests that Convert behavior is not affected by different targetType parameter values.
        /// </summary>
        [TestMethod]
        public void Convert_DifferentTargetTypes_ProducesSameResult()
        {
            // Arrange
            var converter = new PercentToDoubleConverter();
            var input = 50;

            // Act
            var resultDouble = converter.Convert(input, typeof(double), null!, CultureInfo.InvariantCulture);
            var resultString = converter.Convert(input, typeof(string), null!, CultureInfo.InvariantCulture);
            var resultInt = converter.Convert(input, typeof(int), null!, CultureInfo.InvariantCulture);

            // Assert
            Assert.AreEqual(0.5, resultDouble);
            Assert.AreEqual(0.5, resultString);
            Assert.AreEqual(0.5, resultInt);
        }

        /// <summary>
        /// Tests that Convert behavior is not affected by different parameter values.
        /// </summary>
        [TestMethod]
        public void Convert_DifferentParameters_ProducesSameResult()
        {
            // Arrange
            var converter = new PercentToDoubleConverter();
            var input = 50;

            // Act
            var resultNull = converter.Convert(input, typeof(double), null!, CultureInfo.InvariantCulture);
            var resultString = converter.Convert(input, typeof(double), "param", CultureInfo.InvariantCulture);
            var resultInt = converter.Convert(input, typeof(double), 123, CultureInfo.InvariantCulture);

            // Assert
            Assert.AreEqual(0.5, resultNull);
            Assert.AreEqual(0.5, resultString);
            Assert.AreEqual(0.5, resultInt);
        }

        /// <summary>
        /// Tests that Convert handles scientific notation strings correctly.
        /// </summary>
        /// <param name="input">The scientific notation string input.</param>
        /// <param name="expected">The expected double output.</param>
        [TestMethod]
        [DataRow("5e-1", 0.5)]
        [DataRow("1e0", 1.0)]
        [DataRow("2e0", 1.0)]
        [DataRow("1e-1", 0.1)]
        public void Convert_ScientificNotationStrings_ReturnsClampedValue(string input, double expected)
        {
            // Arrange
            var converter = new PercentToDoubleConverter();

            // Act
            var result = converter.Convert(input, typeof(double), null!, CultureInfo.InvariantCulture);

            // Assert
            Assert.AreEqual(expected, result);
        }

        /// <summary>
        /// Tests that Convert handles very long numeric strings correctly.
        /// </summary>
        [TestMethod]
        public void Convert_VeryLongNumericString_ReturnsClampedValue()
        {
            // Arrange
            var converter = new PercentToDoubleConverter();
            var input = "123456789012345678901234567890";

            // Act
            var result = converter.Convert(input, typeof(double), null!, CultureInfo.InvariantCulture);

            // Assert
            Assert.AreEqual(1.0, result);
        }

        /// <summary>
        /// Tests that Convert handles hexadecimal strings by returning 0.0 (not parsed as valid number).
        /// </summary>
        /// <param name="input">The hexadecimal string input.</param>
        [TestMethod]
        [DataRow("0x50")]
        [DataRow("0xFF")]
        [DataRow("0x00")]
        public void Convert_HexadecimalStrings_ReturnsZero(string input)
        {
            // Arrange
            var converter = new PercentToDoubleConverter();

            // Act
            var result = converter.Convert(input, typeof(double), null!, CultureInfo.InvariantCulture);

            // Assert
            Assert.AreEqual(0.0, result);
        }

        /// <summary>
        /// Tests that Convert handles strings with currency symbols by returning 0.0.
        /// </summary>
        /// <param name="input">The string input with currency symbol.</param>
        [TestMethod]
        [DataRow("$50")]
        [DataRow("€100")]
        [DataRow("£75")]
        public void Convert_StringsWithCurrencySymbols_ReturnsZero(string input)
        {
            // Arrange
            var converter = new PercentToDoubleConverter();

            // Act
            var result = converter.Convert(input, typeof(double), null!, CultureInfo.InvariantCulture);

            // Assert
            Assert.AreEqual(0.0, result);
        }

        /// <summary>
        /// Tests that Convert handles strings with percentage symbols by returning 0.0.
        /// </summary>
        /// <param name="input">The string input with percentage symbol.</param>
        [TestMethod]
        [DataRow("50%")]
        [DataRow("100%")]
        [DataRow("%50")]
        public void Convert_StringsWithPercentageSymbols_ReturnsZero(string input)
        {
            // Arrange
            var converter = new PercentToDoubleConverter();

            // Act
            var result = converter.Convert(input, typeof(double), null!, CultureInfo.InvariantCulture);

            // Assert
            Assert.AreEqual(0.0, result);
        }

        /// <summary>
        /// Helper class for testing custom objects with ToString() override.
        /// </summary>
        private class CustomNumericObject
        {
            private readonly int value;

            public CustomNumericObject(int value)
            {
                this.value = value;
            }

            public override string ToString()
            {
                return value.ToString();
            }
        }
    }
}