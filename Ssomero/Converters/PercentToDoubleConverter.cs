using System;
using System.Globalization;
using Microsoft.Maui.Controls;

namespace Ssomero.Converters
{
    public class PercentToDoubleConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return 0.0;
            if (int.TryParse(value.ToString(), out var v))
                return Math.Clamp(v / 100.0, 0.0, 1.0);
            if (double.TryParse(value.ToString(), out var d))
                return Math.Clamp(d, 0.0, 1.0);
            return 0.0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}