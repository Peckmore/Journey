using CommunityToolkit.Mvvm.Input;
using JourneyBrowser.Models;
using Microsoft.Web.WebView2.Core;
using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace JourneyBrowser.ViewModels
{
    internal class MainWindowViewModel : INotifyPropertyChanged, IDisposable
    {
        #region Fields

        private bool _isDisposed;
        private BrowserTab? _selectedTab;
        private readonly ObservableCollection<BrowserTab> _tabs;

        #endregion

        #region Events

        public event PropertyChangedEventHandler? PropertyChanged;

        #endregion

        #region Construction

        public MainWindowViewModel()
        {
            // Set fields.
            _tabs = new();

            // Set properties.
            AutoColorSchemeCommand = new RelayCommand(ExecutedAutoColorSchemeCommand);
            BackCommand = new RelayCommand(ExecutedBackCommand, CanExecuteBackCommand);
            CloseSelectedTabCommand = new RelayCommand(ExecutedCloseSelectedTabCommand);
            DarkColorSchemeCommand = new RelayCommand(ExecutedDarkColorSchemeCommand);
            ForwardCommand = new RelayCommand(ExecutedForwardCommand, CanExecuteForwardCommand);
            HomeCommand = new RelayCommand(ExecutedHomeCommand);
            JourneyCommand = new RelayCommand(ExecutedJourneyCommand, CanExecuteJourneyCommand);
            LightColorSchemeCommand = new RelayCommand(ExecutedLightColorSchemeCommand);
            NewTabCommand = new RelayCommand(ExecutedNewTabCommand);
            ReloadCommand = new RelayCommand(ExecutedReloadCommand);

            // Add our event handlers.
            Settings.Singleton.PropertyChanged += Settings_PropertyChanged;
            SystemEvents.UserPreferenceChanged += SystemEvents_UserPreferenceChanged;
        }

        #endregion

        #region Properties

        public IRelayCommand? AutoColorSchemeCommand { get; private set; }
        public IRelayCommand? BackCommand { get; private set; }
        public IRelayCommand? CloseSelectedTabCommand { get; private set; }
        public IRelayCommand? DarkColorSchemeCommand { get; private set; }
        public bool DarkMode
        {
            get
            {
                if (_isDisposed)
                {
                    throw new ObjectDisposedException("Object has been disposed.");
                }

                switch (Settings.Singleton.ColorScheme)
                {
                    case CoreWebView2PreferredColorScheme.Auto:
                        // If the preferred color scheme is set to auto, we'll check the registry to see whether the app should be in
                        // light or dark mode.
                        using (var themeRegistryKey = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize"))
                        {
                            return (themeRegistryKey?.GetValue("AppsUseLightTheme") as int? ?? 1) == 0;
                        }

                    case CoreWebView2PreferredColorScheme.Dark:
                        return true;

                    case CoreWebView2PreferredColorScheme.Light:
                    default:
                        return false;
                }
            }
        }
        public IRelayCommand? ForwardCommand { get; private set; }
        public IRelayCommand? HomeCommand { get; private set; }
        public IRelayCommand? JourneyCommand { get; private set; }
        public IRelayCommand? LightColorSchemeCommand { get; private set; }
        public IRelayCommand? NewTabCommand { get; private set; }
        public IRelayCommand? ReloadCommand { get; private set; }
        public BrowserTab? SelectedTab
        {
            get
            {
                if (_isDisposed)
                {
                    throw new ObjectDisposedException("Object has been disposed.");
                }

                return _selectedTab;
            }
            set
            {
                if (_isDisposed)
                {
                    throw new ObjectDisposedException("Object has been disposed.");
                }

                if (_selectedTab != value)
                {
                    _selectedTab = value;
                    NotifyPropertyChanged();

                    // If the selected tab has changed, the UI will need to update it's buttons.
                    UpdateCommandStates();
                }
            }
        }
        public Func<BrowserTab> TabFactory
        {
            get
            {
                if (_isDisposed)
                {
                    throw new ObjectDisposedException("Object has been disposed.");
                }

                return () => new BrowserTab(Settings.Singleton.HomePage);
            }
        }
        public ObservableCollection<BrowserTab> Tabs
        {
            get
            {
                if (_isDisposed)
                {
                    throw new ObjectDisposedException("Object has been disposed.");
                }

                return _tabs;
            }
        }

        #endregion

        #region Methods

        #region Event Handlers

        private void Settings_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            // If the setting for color scheme has changed, raised our event from the view model so our bound window will update.
            if (e.PropertyName == nameof(Settings.ColorScheme))
            {
                NotifyPropertyChanged(nameof(DarkMode));
            }
        }
        private void SystemEvents_UserPreferenceChanged(object sender, UserPreferenceChangedEventArgs e)
        {
            // If the user has updated their OS preferences, trigger a theme change event in-case the theme needs refreshing.
            if (e.Category == UserPreferenceCategory.General)
            {
                NotifyPropertyChanged(nameof(DarkMode));
            }
        }

        #endregion

        #region Private

        private bool CanExecuteBackCommand()
        {
            return _selectedTab?.CanGoBack ?? false;
        }
        private bool CanExecuteForwardCommand()
        {
            return _selectedTab?.CanGoForward ?? false;
        }
        private bool CanExecuteJourneyCommand()
        {
            return _selectedTab?.CanShowJourney ?? false;
        }
        private void Dispose(bool disposing)
        {
            if (!_isDisposed)
            {
                if (disposing)
                {
                    // Unhook from events.
                    Settings.Singleton.PropertyChanged -= Settings_PropertyChanged;
                    SystemEvents.UserPreferenceChanged -= SystemEvents_UserPreferenceChanged;

                    // Clear any fields we can.
                    _selectedTab = null;
                    AutoColorSchemeCommand = null;
                    BackCommand = null;
                    CloseSelectedTabCommand = null;
                    DarkColorSchemeCommand = null;
                    ForwardCommand = null;
                    HomeCommand = null;
                    JourneyCommand = null;
                    LightColorSchemeCommand = null;
                    NewTabCommand = null;
                    ReloadCommand = null;
                }

                _tabs.Clear();

                _isDisposed = true;
            }
        }
        private void ExecutedAutoColorSchemeCommand()
        {
            Settings.Singleton.ColorScheme = CoreWebView2PreferredColorScheme.Auto;
            NotifyPropertyChanged(nameof(DarkMode));
        }
        private void ExecutedBackCommand()
        {
            _selectedTab?.GoBack();
        }
        private void ExecutedCloseSelectedTabCommand()
        {
            var tab = _selectedTab;
            if (tab != null)
            {
                var selectedIndex = Tabs.IndexOf(tab);
                if (selectedIndex > 0)
                {
                    SelectedTab = Tabs[selectedIndex - 1];
                }
                else if (selectedIndex == 0 && Tabs.Count > 1)
                {
                    SelectedTab = Tabs[selectedIndex + 1];
                }

                Tabs.Remove(tab);
            }
        }
        private void ExecutedDarkColorSchemeCommand()
        {
            Settings.Singleton.ColorScheme = CoreWebView2PreferredColorScheme.Dark;
            NotifyPropertyChanged(nameof(DarkMode));
        }
        private void ExecutedForwardCommand()
        {
            _selectedTab?.GoForward();
        }
        private void ExecutedHomeCommand()
        {
            _selectedTab?.GoHome();
        }
        private void ExecutedJourneyCommand()
        {
            _selectedTab?.ToggleJourney();
        }
        private void ExecutedLightColorSchemeCommand()
        {
            Settings.Singleton.ColorScheme = CoreWebView2PreferredColorScheme.Light;
            NotifyPropertyChanged(nameof(DarkMode));
        }
        private void ExecutedNewTabCommand()
        {
            CreateTab(Settings.Singleton.HomePage);
        }
        private void ExecutedReloadCommand()
        {
            _selectedTab?.Reload();
        }
        private void NotifyPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        private void UpdateCommandStates()
        {
            BackCommand?.NotifyCanExecuteChanged();
            ForwardCommand?.NotifyCanExecuteChanged();
            JourneyCommand?.NotifyCanExecuteChanged();
        }

        #endregion

        #region Public

        public void CreateTab(string address)
        {
            if (_isDisposed)
            {
                throw new ObjectDisposedException("Object has been disposed.");
            }

            var newTab = new BrowserTab(address);
            Tabs.Add(newTab);
            SelectedTab = newTab;
        }
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        public void UpdateCommandStates(BrowserTab tab)
        {
            if (_isDisposed)
            {
                throw new ObjectDisposedException("Object has been disposed.");
            }

            // Only update the button states if the tab is the currently active/selected tab.
            if (tab == _selectedTab)
            {
                UpdateCommandStates();
            }
        }

        #endregion

        #endregion
    }
}