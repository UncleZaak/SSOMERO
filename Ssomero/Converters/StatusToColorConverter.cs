using System;
using System.Globalization;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

namespace Ssomero.Converters;

public class StatusToColorConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var status = value?.ToString() ?? string.Empty;
        return status switch
        {
            "Active" => Color.FromArgb("#22C55E"),
            "Suspended" => Color.FromArgb("#F59E0B"),
            "Deactivated" => Color.FromArgb("#9CA3AF"),
            _ => Color.FromArgb("#6B7280")
        };
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
