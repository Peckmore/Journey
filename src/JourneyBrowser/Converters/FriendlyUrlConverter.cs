using System.Globalization;
using System.Windows.Data;

namespace JourneyBrowser.Converters
{
    internal class FriendlyUrlConverter : IValueConverter
    {
        #region Methods

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string valueString)
            {
                try
                {
                    // Parse the address into a Uri object.
                    var uri = new Uri(valueString);

                    // Return just the "host" segment of the parsed Uri, which is what we want to display for neatness.
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