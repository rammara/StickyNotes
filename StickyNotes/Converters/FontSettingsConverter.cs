using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace StickyNotes.Converters
{
    public class FontSettingsConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is System.Drawing.Font winFormsFont)
            {
                var fontFamily = new System.Windows.Media.FontFamily(winFormsFont.FontFamily.Name);
                var fontSize = winFormsFont.Size * 96.0 / 72.0; // Конвертация точек в пиксели

                return new
                {
                    FontFamily = fontFamily,
                    FontSize = fontSize,
                    FontWeight = winFormsFont.Bold ? FontWeights.Bold : FontWeights.Normal,
                    FontStyle = winFormsFont.Italic ? FontStyles.Italic : FontStyles.Normal
                };
            }

            return DependencyProperty.UnsetValue;
        } // Convert

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        } // ConvertBack
    } // FontSettingsConverter
} // namespace