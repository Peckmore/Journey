using System.Globalization;
using System.Windows.Data;

namespace JourneyBrowser.Converters
{
    public class UrlSearchConverter : IValueConverter
    {
        #region Methods

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string valueString)
            {
                try
                {
                    var uri = new Uri(valueString);
                    return uri.Host;
                }
                catch
                {
                    // We couldn't parse the string, so just return it as is.
                    return valueString;
                }
            }

            return value;
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }

        #endregion
    }
}