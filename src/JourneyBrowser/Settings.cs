using Microsoft.Web.WebView2.Core;

namespace JourneyBrowser
{
    internal static class Settings
    {
        // This is mostly a dummy class to hold settings-related constants. If we were doing this properly this would be a settings
        // provider that loaded settings from somewhere (config file on disk?), handled syncing of settings with a cloud account, etc.,
        // and would be loaded via DI. However, we're only building the lightest of browser implementations, so this will suffice for
        // our purposes.

        #region Properties

        public static CoreWebView2PreferredColorScheme ColorScheme => CoreWebView2PreferredColorScheme.Auto;
        public static string HomePage => "https://start.duckduckgo.com";

        #endregion
    }
}