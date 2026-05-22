using System;
using System.Globalization;

using Microsoft.Maui.Controls;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ssomero.Converters;

namespace Ssomero.Converters.UnitTests
{
    /// <summary>
    /// Unit tests for <see cref="InvertBoolConverter"/> class.
    /// </summary>
    [TestClass]
    public class InvertBoolConverterTests
    {
        /// <summary>
        /// Tests that Convert returns false when the input value is true.
        /// </summary>
        [TestMethod]
        public void Convert_BoolTrueValue_ReturnsFalse()
        {
            // Arrange
            var converter = new InvertBoolConverter();
            object value = true;
            Type targetType = typeof(bool);
            object parameter = null;
            CultureInfo culture = CultureInfo.InvariantCulture;

            // Act
            object result = converter.Convert(value, targetType, parameter, culture);

            // Assert
            Assert.IsInstanceOfType(result, typeof(bool));
            Assert.AreEqual(false, result);
        }

        /// <summary>
        /// Tests that Convert returns true when the input value is false.
        /// </summary>
        [TestMethod]
        public void Convert_BoolFalseValue_ReturnsTrue()
        {
            // Arrange
            var converter = new InvertBoolConverter();
            object value = false;
            Type targetType = typeof(bool);
            object parameter = null;
            CultureInfo culture = CultureInfo.InvariantCulture;

            // Act
            object result = converter.Convert(value, targetType, parameter, culture);

            // Assert
            Assert.IsInstanceOfType(result, typeof(bool));
            Assert.AreEqual(true, result);
        }

        /// <summary>
        /// Tests that Convert returns true when the input value is null.
        /// </summary>
        [TestMethod]
        public void Convert_NullValue_ReturnsTrue()
        {
            // Arrange
            var converter = new InvertBoolConverter();
            object? value = null;
            Type targetType = typeof(bool);
            object parameter = null;
            CultureInfo culture = CultureInfo.InvariantCulture;

            // Act
            object result = converter.Convert(value!, targetType, parameter, culture);

            // Assert
            Assert.IsInstanceOfType(result, typeof(bool));
            Assert.AreEqual(true, result);
        }

        /// <summary>
        /// Tests that Convert returns true when the input value is a string.
        /// Input: string value.
        /// Expected: Returns true since value is not a bool.
        /// </summary>
        [TestMethod]
        [DataRow("test")]
        [DataRow("")]
        [DataRow("   ")]
        [DataRow("true")]
        [DataRow("false")]
        public void Convert_StringValue_ReturnsTrue(string stringValue)
        {
            // Arrange
            var converter = new InvertBoolConverter();
            object value = stringValue;
            Type targetType = typeof(bool);
            object parameter = null;
            CultureInfo culture = CultureInfo.InvariantCulture;

            // Act
            object result = converter.Convert(value, targetType, parameter, culture);

            // Assert
            Assert.IsInstanceOfType(result, typeof(bool));
            Assert.AreEqual(true, result);
        }

        /// <summary>
        /// Tests that Convert returns true when the input value is a numeric type.
        /// Input: various numeric values (int, double, etc).
        /// Expected: Returns true since value is not a bool.
        /// </summary>
        [TestMethod]
        [DataRow(0)]
        [DataRow(1)]
        [DataRow(-1)]
        [DataRow(int.MaxValue)]
        [DataRow(int.MinValue)]
        public void Convert_IntValue_ReturnsTrue(int intValue)
        {
            // Arrange
            var converter = new InvertBoolConverter();
            object value = intValue;
            Type targetType = typeof(bool);
            object parameter = null;
            CultureInfo culture = CultureInfo.InvariantCulture;

            // Act
            object result = converter.Convert(value, targetType, parameter, culture);

            // Assert
            Assert.IsInstanceOfType(result, typeof(bool));
            Assert.AreEqual(true, result);
        }

        /// <summary>
        /// Tests that Convert returns true when the input value is a double.
        /// Input: various double values including special values.
        /// Expected: Returns true since value is not a bool.
        /// </summary>
        [TestMethod]
        [DataRow(0.0)]
        [DataRow(1.5)]
        [DataRow(-1.5)]
        [DataRow(double.MaxValue)]
        [DataRow(double.MinValue)]
        [DataRow(double.NaN)]
        [DataRow(double.PositiveInfinity)]
        [DataRow(double.NegativeInfinity)]
        public void Convert_DoubleValue_ReturnsTrue(double doubleValue)
        {
            // Arrange
            var converter = new InvertBoolConverter();
            object value = doubleValue;
            Type targetType = typeof(bool);
            object parameter = null;
            CultureInfo culture = CultureInfo.InvariantCulture;

            // Act
            object result = converter.Convert(value, targetType, parameter, culture);

            // Assert
            Assert.IsInstanceOfType(result, typeof(bool));
            Assert.AreEqual(true, result);
        }

        /// <summary>
        /// Tests that Convert returns true when the input value is an arbitrary object.
        /// Input: new object instance.
        /// Expected: Returns true since value is not a bool.
        /// </summary>
        [TestMethod]
        public void Convert_ObjectValue_ReturnsTrue()
        {
            // Arrange
            var converter = new InvertBoolConverter();
            object value = new object();
            Type targetType = typeof(bool);
            object parameter = null;
            CultureInfo culture = CultureInfo.InvariantCulture;

            // Act
            object result = converter.Convert(value, targetType, parameter, culture);

            // Assert
            Assert.IsInstanceOfType(result, typeof(bool));
            Assert.AreEqual(true, result);
        }

        /// <summary>
        /// Tests that Convert works correctly when targetType parameter is null.
        /// Input: bool true with null targetType.
        /// Expected: Returns false (inverted bool), targetType is not used.
        /// </summary>
        [TestMethod]
        public void Convert_NullTargetType_InvertsBoolCorrectly()
        {
            // Arrange
            var converter = new InvertBoolConverter();
            object value = true;
            Type? targetType = null;
            object parameter = null;
            CultureInfo culture = CultureInfo.InvariantCulture;

            // Act
            object result = converter.Convert(value, targetType!, parameter, culture);

            // Assert
            Assert.IsInstanceOfType(result, typeof(bool));
            Assert.AreEqual(false, result);
        }

        /// <summary>
        /// Tests that Convert works correctly when parameter is null.
        /// Input: bool false with null parameter.
        /// Expected: Returns true (inverted bool), parameter is not used.
        /// </summary>
        [TestMethod]
        public void Convert_NullParameter_InvertsBoolCorrectly()
        {
            // Arrange
            var converter = new InvertBoolConverter();
            object value = false;
            Type targetType = typeof(bool);
            object? parameter = null;
            CultureInfo culture = CultureInfo.InvariantCulture;

            // Act
            object result = converter.Convert(value, targetType, parameter, culture);

            // Assert
            Assert.IsInstanceOfType(result, typeof(bool));
            Assert.AreEqual(true, result);
        }

        /// <summary>
        /// Tests that Convert works correctly when culture parameter is null.
        /// Input: bool true with null culture.
        /// Expected: Returns false (inverted bool), culture is not used.
        /// </summary>
        [TestMethod]
        public void Convert_NullCulture_InvertsBoolCorrectly()
        {
            // Arrange
            var converter = new InvertBoolConverter();
            object value = true;
            Type targetType = typeof(bool);
            object parameter = null;
            CultureInfo? culture = null;

            // Act
            object result = converter.Convert(value, targetType, parameter, culture!);

            // Assert
            Assert.IsInstanceOfType(result, typeof(bool));
            Assert.AreEqual(false, result);
        }

        /// <summary>
        /// Tests that Convert works correctly with all parameters as non-null values.
        /// Input: bool true with various non-null parameter values.
        /// Expected: Returns false (inverted bool), unused parameters don't affect result.
        /// </summary>
        [TestMethod]
        public void Convert_AllParametersProvided_InvertsBoolCorrectly()
        {
            // Arrange
            var converter = new InvertBoolConverter();
            object value = true;
            Type targetType = typeof(string);
            object parameter = "some parameter";
            CultureInfo culture = new CultureInfo("en-US");

            // Act
            object result = converter.Convert(value, targetType, parameter, culture);

            // Assert
            Assert.IsInstanceOfType(result, typeof(bool));
            Assert.AreEqual(false, result);
        }

        /// <summary>
        /// Tests that ConvertBack inverts a true value to false.
        /// </summary>
        [TestMethod]
        public void ConvertBack_TrueValue_ReturnsFalse()
        {
            // Arrange
            var converter = new InvertBoolConverter();
            object value = true;
            Type targetType = typeof(bool);
            object parameter = null;
            CultureInfo culture = CultureInfo.InvariantCulture;

            // Act
            object result = converter.ConvertBack(value, targetType, parameter, culture);

            // Assert
            Assert.IsInstanceOfType(result, typeof(bool));
            Assert.AreEqual(false, result);
        }

        /// <summary>
        /// Tests that ConvertBack inverts a false value to true.
        /// </summary>
        [TestMethod]
        public void ConvertBack_FalseValue_ReturnsTrue()
        {
            // Arrange
            var converter = new InvertBoolConverter();
            object value = false;
            Type targetType = typeof(bool);
            object parameter = null;
            CultureInfo culture = CultureInfo.InvariantCulture;

            // Act
            object result = converter.ConvertBack(value, targetType, parameter, culture);

            // Assert
            Assert.IsInstanceOfType(result, typeof(bool));
            Assert.AreEqual(true, result);
        }

        /// <summary>
        /// Tests that ConvertBack returns false when value is null.
        /// </summary>
        [TestMethod]
        public void ConvertBack_NullValue_ReturnsFalse()
        {
            // Arrange
            var converter = new InvertBoolConverter();
            object? value = null;
            Type targetType = typeof(bool);
            object parameter = null;
            CultureInfo culture = CultureInfo.InvariantCulture;

            // Act
            object result = converter.ConvertBack(value, targetType, parameter, culture);

            // Assert
            Assert.IsInstanceOfType(result, typeof(bool));
            Assert.AreEqual(false, result);
        }

        /// <summary>
        /// Tests that ConvertBack returns false for various non-bool types.
        /// </summary>
        /// <param name="value">The non-bool value to test.</param>
        [TestMethod]
        [DataRow("true")]
        [DataRow("false")]
        [DataRow("")]
        [DataRow("   ")]
        [DataRow(0)]
        [DataRow(1)]
        [DataRow(-1)]
        [DataRow(int.MaxValue)]
        [DataRow(int.MinValue)]
        [DataRow(0.0)]
        [DataRow(1.0)]
        [DataRow(double.NaN)]
        [DataRow(double.PositiveInfinity)]
        [DataRow(double.NegativeInfinity)]
        public void ConvertBack_NonBoolValue_ReturnsFalse(object value)
        {
            // Arrange
            var converter = new InvertBoolConverter();
            Type targetType = typeof(bool);
            object parameter = null;
            CultureInfo culture = CultureInfo.InvariantCulture;

            // Act
            object result = converter.ConvertBack(value, targetType, parameter, culture);

            // Assert
            Assert.IsInstanceOfType(result, typeof(bool));
            Assert.AreEqual(false, result);
        }

        /// <summary>
        /// Tests that ConvertBack returns false when value is a complex object type.
        /// </summary>
        [TestMethod]
        public void ConvertBack_ObjectValue_ReturnsFalse()
        {
            // Arrange
            var converter = new InvertBoolConverter();
            object value = new object();
            Type targetType = typeof(bool);
            object parameter = null;
            CultureInfo culture = CultureInfo.InvariantCulture;

            // Act
            object result = converter.ConvertBack(value, targetType, parameter, culture);

            // Assert
            Assert.IsInstanceOfType(result, typeof(bool));
            Assert.AreEqual(false, result);
        }

        /// <summary>
        /// Tests that ConvertBack handles null targetType without throwing exception.
        /// </summary>
        [TestMethod]
        public void ConvertBack_NullTargetType_InvertsValueSuccessfully()
        {
            // Arrange
            var converter = new InvertBoolConverter();
            object value = true;
            Type? targetType = null;
            object parameter = null;
            CultureInfo culture = CultureInfo.InvariantCulture;

            // Act
            object result = converter.ConvertBack(value, targetType, parameter, culture);

            // Assert
            Assert.IsInstanceOfType(result, typeof(bool));
            Assert.AreEqual(false, result);
        }

        /// <summary>
        /// Tests that ConvertBack handles null culture without throwing exception.
        /// </summary>
        [TestMethod]
        public void ConvertBack_NullCulture_InvertsValueSuccessfully()
        {
            // Arrange
            var converter = new InvertBoolConverter();
            object value = true;
            Type targetType = typeof(bool);
            object parameter = null;
            CultureInfo? culture = null;

            // Act
            object result = converter.ConvertBack(value, targetType, parameter, culture);

            // Assert
            Assert.IsInstanceOfType(result, typeof(bool));
            Assert.AreEqual(false, result);
        }

        /// <summary>
        /// Tests that ConvertBack handles all parameters being null except a valid bool value.
        /// </summary>
        [TestMethod]
        public void ConvertBack_AllParametersNullExceptBoolValue_InvertsValueSuccessfully()
        {
            // Arrange
            var converter = new InvertBoolConverter();
            object value = false;
            Type? targetType = null;
            object? parameter = null;
            CultureInfo? culture = null;

            // Act
            object result = converter.ConvertBack(value, targetType, parameter, culture);

            // Assert
            Assert.IsInstanceOfType(result, typeof(bool));
            Assert.AreEqual(true, result);
        }

        /// <summary>
        /// Tests that ConvertBack correctly handles different culture settings.
        /// </summary>
        [TestMethod]
        public void ConvertBack_DifferentCultures_InvertsValueSuccessfully()
        {
            // Arrange
            var converter = new InvertBoolConverter();
            object value = true;
            Type targetType = typeof(bool);
            object parameter = null;
            CultureInfo culture = new CultureInfo("fr-FR");

            // Act
            object result = converter.ConvertBack(value, targetType, parameter, culture);

            // Assert
            Assert.IsInstanceOfType(result, typeof(bool));
            Assert.AreEqual(false, result);
        }

        /// <summary>
        /// Tests that ConvertBack correctly handles various parameter values.
        /// </summary>
        [TestMethod]
        public void ConvertBack_WithParameter_InvertsValueSuccessfully()
        {
            // Arrange
            var converter = new InvertBoolConverter();
            object value = true;
            Type targetType = typeof(bool);
            object parameter = "someParameter";
            CultureInfo culture = CultureInfo.InvariantCulture;

            // Act
            object result = converter.ConvertBack(value, targetType, parameter, culture);

            // Assert
            Assert.IsInstanceOfType(result, typeof(bool));
            Assert.AreEqual(false, result);
        }

        /// <summary>
        /// Tests that ConvertBack returns false for various string values.
        /// Input: String values including empty, whitespace, and string representations of booleans.
        /// Expected: Returns false for all non-bool values.
        /// </summary>
        [TestMethod]
        [DataRow("true")]
        [DataRow("false")]
        [DataRow("")]
        [DataRow("   ")]
        [DataRow("test")]
        [DataRow("0")]
        [DataRow("1")]
        public void ConvertBack_StringValue_ReturnsFalse(string stringValue)
        {
            // Arrange
            var converter = new InvertBoolConverter();
            object value = stringValue;
            Type targetType = typeof(bool);
            object parameter = null;
            CultureInfo culture = CultureInfo.InvariantCulture;

            // Act
            object result = converter.ConvertBack(value, targetType, parameter, culture);

            // Assert
            Assert.IsInstanceOfType(result, typeof(bool));
            Assert.AreEqual(false, result);
        }

        /// <summary>
        /// Tests that ConvertBack returns false for various integer values.
        /// Input: Integer values including zero, positive, negative, and boundary values.
        /// Expected: Returns false for all non-bool values.
        /// </summary>
        [TestMethod]
        [DataRow(0)]
        [DataRow(1)]
        [DataRow(-1)]
        [DataRow(int.MaxValue)]
        [DataRow(int.MinValue)]
        public void ConvertBack_IntValue_ReturnsFalse(int intValue)
        {
            // Arrange
            var converter = new InvertBoolConverter();
            object value = intValue;
            Type targetType = typeof(bool);
            object parameter = null;
            CultureInfo culture = CultureInfo.InvariantCulture;

            // Act
            object result = converter.ConvertBack(value, targetType, parameter, culture);

            // Assert
            Assert.IsInstanceOfType(result, typeof(bool));
            Assert.AreEqual(false, result);
        }

        /// <summary>
        /// Tests that ConvertBack returns false for various double values including special values.
        /// Input: Double values including zero, positive, negative, NaN, and infinity values.
        /// Expected: Returns false for all non-bool values.
        /// </summary>
        [TestMethod]
        [DataRow(0.0)]
        [DataRow(1.0)]
        [DataRow(-1.0)]
        [DataRow(double.MaxValue)]
        [DataRow(double.MinValue)]
        [DataRow(double.NaN)]
        [DataRow(double.PositiveInfinity)]
        [DataRow(double.NegativeInfinity)]
        public void ConvertBack_DoubleValue_ReturnsFalse(double doubleValue)
        {
            // Arrange
            var converter = new InvertBoolConverter();
            object value = doubleValue;
            Type targetType = typeof(bool);
            object parameter = null;
            CultureInfo culture = CultureInfo.InvariantCulture;

            // Act
            object result = converter.ConvertBack(value, targetType, parameter, culture);

            // Assert
            Assert.IsInstanceOfType(result, typeof(bool));
            Assert.AreEqual(false, result);
        }
    }
}