using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace ChatClient.Converters
{
    public class BoolToBubbleMarginConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool isOwnMessage = (bool)value;
            return isOwnMessage ? new Thickness(0, 2, 0, 0) : new Thickness(40, 2, 0, 0); // Adjust margins as needed
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
