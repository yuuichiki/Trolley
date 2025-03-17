using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Trolley.Helpers
{
    public class BooleanToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (value != null && value is bool && (bool)value)? (object)Visibility.Visible : (object)Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (object)null;
        }
    }
}
