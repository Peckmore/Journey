using Microsoft.Web.WebView2.Core;
using System.Globalization;
using System.Windows.Data;

namespace JourneyBrowser.Converters
{
    /// <summary>
    /// Used to convert a bool representing dark mode enabled into a WebView2 preferred color scheme.
    /// </summary>
    internal class BooleanToColorSchemeConverter : IValueConverter
    {
        #region Methods

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool valueBool)
            {
                // If the value is true, we want to use dark mode.
                return valueBool ? CoreWebView2PreferredColorScheme.Dark : CoreWebView2PreferredColorScheme.Light;
            }

            // The value isn't a bool, so return it as-is.
            return value;
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // We don't support reverse conversions.

            return value;
        }

        #endregion
    }
}