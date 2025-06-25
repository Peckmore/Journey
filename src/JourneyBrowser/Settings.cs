using Microsoft.Web.WebView2.Core;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace JourneyBrowser
{
    internal class Settings : INotifyPropertyChanged
    {
        // This is mostly a dummy class to hold settings-related constants. If we were doing this properly this would be a settings
        // provider that loaded settings from somewhere (config file on disk?), handled syncing of settings with a cloud account, etc.,
        // and would be loaded via DI. However, we're only building the lightest of browser implementations, so this will suffice for
        // our purposes.

        #region Fields

        private CoreWebView2PreferredColorScheme _colorScheme;

        #endregion

        #region  Events

        public event PropertyChangedEventHandler? PropertyChanged;

        #endregion

        #region Construction

        private Settings()
        {
            // Make our instance constructor private so we only create a singleton instance of the class (in lieu of proper DI).

            ColorScheme = CoreWebView2PreferredColorScheme.Auto;
        }
        static Settings()
        {
            // Create our singleton instance.
            Singleton = new();
        }

        #endregion

        #region Properties

        #region Public

        public CoreWebView2PreferredColorScheme ColorScheme
        {
            get => _colorScheme;
            set
            {
                if (_colorScheme != value)
                {
                    _colorScheme = value;
                    OnPropertyChanged();
                }
            }
        }

        public string HomePage => "https://start.duckduckgo.com";

        #endregion

        #region Public Static

        public static Settings Singleton { get; private set; }

        #endregion

        #endregion

        #region Methods

        private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}