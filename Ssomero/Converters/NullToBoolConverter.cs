using System;
using System.Globalization;
using Microsoft.Maui.Controls;

namespace Ssomero.Converters
{
    public class NullToBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return false;
            if (value is string s) return !string.IsNullOrWhiteSpace(s);
            if (value is bool b) return b;
            return true;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}