using System.Globalization;
using System.Windows.Data;

namespace JourneyBrowser.Converters
{
    /// <summary>
    /// Used to convert a string containing a URL into a "friendly" URL, which contains just the "host" segment.
    /// </summary>
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

            // The value isn't a string, so return it as-is.
            return value;
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // We don't/can't support reverse conversions.

            return value;
        }

        #endregion
    }
}