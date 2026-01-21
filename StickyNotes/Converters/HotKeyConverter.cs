using StickyNotes.Models;
using System;
using System.Globalization;
using System.Windows.Data;

namespace StickyNotes.Converters
{
    public class HotkeyConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Hotkey hotkey)
            {
                return hotkey.ToString();
            }

            return string.Empty;
        } // Convert

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string str)
            {
                return Hotkey.Parse(str);
            }

            return new Hotkey();
        } // ConvertBack
    } // HotkeyConverter
} // namespace