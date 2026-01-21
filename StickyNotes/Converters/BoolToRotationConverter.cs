using System.Globalization;
using System.Windows.Data;

namespace StickyNotes.Converters
{
    public class BoolToRotationConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isPinned)
            {
                return isPinned ? 0 : 45; // 0° for pinned, 45° for unpinned
            }
            return 0;
        } // Convert

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        } // ConvertBack
    } // BoolToRotationConverter
} // namespace