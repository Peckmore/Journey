using System.Globalization;
using System.Net;
using System.Text.RegularExpressions;
using System.Windows.Data;

namespace JourneyBrowser.Converters
{
    public class UrlSearchConverter : IValueConverter
    {
        #region Methods

        #region Private

        private static bool IsValidDnsName(string name)
        {
            if (!string.IsNullOrWhiteSpace(name))
            {
                if (name.Length <= 253)
                {
                    var label = @"[a-zA-Z0-9]([a-zA-Z0-9\-]{0,61}[a-zA-Z0-9])?";
                    var pattern = $"^{label}\\.{label}(\\.{label})*$";
                    return Regex.IsMatch(name, pattern);
                }
            }

            return false;
        }

        #endregion

        #region Public

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // We don't do anything with the URL we receive, just pass it through as-is.
            return value;
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string valueString)
            {
                // We'll do some really basic parsing here to check whether a string is a valid URL or not and, if not, convert it into
                // a search query. We're just going to cover the major use cases for the purposes of this demo, so there could be/probably
                // are fringe cases that will break on this, but that's ok for what we're after.

                // Trim the input to remove trailing whitespace.
                valueString = valueString.Trim();

                // If the input contains a scheme delimiter, then we'll assume the URL is valid and attempt to open it verbatim.
                if (valueString.Contains("://"))
                {
                    return valueString;
                }

                // If the input doesn't contain a scheme delimiter then we'll check whether it looks like a valid URL or not.
                try
                {
                    // Split the string to try and get the Host element or the input.
                    var valueStringParts = valueString.Split('/');
                    if (valueStringParts.Length > 0)
                    {
                        // Check whether we have a valid hostname or IP address.
                        if (IsValidDnsName(valueStringParts[0]) || IPAddress.TryParse(valueStringParts[0], out _))
                        {
                            // We have a valid hostname or IP address, so we'll prefix a HTTP scheme and return the value. We use HTTP
                            // for compatibility, just in-case the target doesn't support HTTPS, but most sites should redirect.
                            valueString = "http://" + valueString;
                            return valueString;
                        }
                    }
                }
                catch
                {
                    // Swallow and do nothing.
                }

                // If we get here then it wasn't a valid URL, so we'll do a search.
                return $"https://duckduckgo.com/?q={Uri.EscapeDataString(valueString)}";
            }

            return value;
        }

        #endregion

        #endregion
    }
}