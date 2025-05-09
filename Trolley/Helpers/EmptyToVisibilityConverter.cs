﻿using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Trolley.Helpers
{
    public class EmptyToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (value == null || value is string && (string)value == "") ? (object)Visibility.Collapsed : (object)Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (object)null;
        }
    }
}
