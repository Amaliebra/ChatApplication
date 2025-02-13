using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows;

namespace ChatClient.Converters
{
    public class NullToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            bool isNull = value == null;
            bool targetVisibility = parameter == null || System.Convert.ToBoolean(parameter);

            if (isNull)
            {
                return targetVisibility ? Visibility.Visible : Visibility.Collapsed;
            }
            else
            {
                return targetVisibility ? Visibility.Collapsed : Visibility.Visible;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
