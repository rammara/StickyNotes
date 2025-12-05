using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace StickyNotes.Converters
{
    public class BoolToColorConverter : IValueConverter
    {
        public Color TrueColor { get; set; } = Colors.DarkGray;
        public Color FalseColor { get; set; } = Colors.Gray;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isPinned)
            {
                return isPinned ? new SolidColorBrush(TrueColor) : new SolidColorBrush(FalseColor);
            }
            return new SolidColorBrush(Colors.Gray);
        } // Convert

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        } // ConvertBack
    } // class BoolToColorConverter
} // namespace