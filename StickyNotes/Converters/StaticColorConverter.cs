using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace StickyNotes.Converters
{
    public class StaticColorConverter : IValueConverter
    {
        public Color Color { get; set; } = Colors.Gray;
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return new SolidColorBrush(Color);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}