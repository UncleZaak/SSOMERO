using System.Globalization;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

namespace Ssomero.Converters;

/// <summary>Attendance % → color: ≥75 green, ≥50 amber, &lt;50 red.</summary>
public class AttendanceColorConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        double pct = value switch
        {
            int i => i,
            double d => d,
            float f => f,
            _ => 0
        };

        return pct switch
        {
            >= 75 => Color.FromArgb("#10B981"),
            >= 50 => Color.FromArgb("#F59E0B"),
            _     => Color.FromArgb("#EF4444")
        };
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
