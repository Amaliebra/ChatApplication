using System.Globalization;
using System.Windows.Data;

namespace ChatClient.Converters
{
    public class BoolToColumnConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool isOwnMessage = (bool)value;
            return isOwnMessage ? 2 : 1; // Column 2 for own (right), Column 1 for others (left)
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
