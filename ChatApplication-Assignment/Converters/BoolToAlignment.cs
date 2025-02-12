using System;
using System.Windows;
using System.Windows.Data;

namespace ChatClient.Converters
{
    public class BoolToAlignment : IValueConverter
    {
        public object TrueValue { get; set; }
        public object FalseValue { get; set; }
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is bool boolValue)
            {
                return boolValue ? TrueValue : FalseValue;
            }
            return HorizontalAlignment.Left;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException("");
        }
    }
}