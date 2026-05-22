using System.Globalization;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

namespace Ssomero.Converters;

/// <summary>bool → one of two string values. ConverterParameter = "TrueValue|FalseValue".</summary>
public class BoolToStringConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var parts = (parameter?.ToString() ?? "|").Split('|');
        var trueVal  = parts.Length > 0 ? parts[0] : string.Empty;
        var falseVal = parts.Length > 1 ? parts[1] : string.Empty;
        return value is true ? trueVal : falseVal;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}

/// <summary>bool → Color. ConverterParameter = "#TrueHex|#FalseHex".</summary>
public class BoolToColorConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var parts = (parameter?.ToString() ?? "#5B21B6|#E2E8F0").Split('|');
        var trueHex  = parts.Length > 0 ? parts[0] : "#5B21B6";
        var falseHex = parts.Length > 1 ? parts[1] : "#E2E8F0";
        return Color.FromArgb(value is true ? trueHex : falseHex);
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
