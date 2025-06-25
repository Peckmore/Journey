using CommunityToolkit.Mvvm.Input;
using JourneyBrowser.Models;
using Microsoft.Web.WebView2.Core;
using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace JourneyBrowser.ViewModels
{
    internal class MainWindowViewModel : INotifyPropertyChanged
    {
        #region Fields

        private BrowserTab? _selectedTab;

        #endregion

        #region Events

        public event PropertyChangedEventHandler? PropertyChanged;

        #endregion

        #region Construction

        public MainWindowViewModel()
        {
            // Set properties
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
            Tabs = new();

            // Add our event handlers
            SystemEvents.UserPreferenceChanged += SystemEvents_UserPreferenceChanged;
        }

        #endregion

        #region Properties

        public IRelayCommand AutoColorSchemeCommand { get; }
        public IRelayCommand BackCommand { get; }
        public IRelayCommand CloseSelectedTabCommand { get; }
        public IRelayCommand DarkColorSchemeCommand { get; }
        public bool DarkMode
        {
            get
            {
                switch (Settings.ColorScheme)
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
        public IRelayCommand ForwardCommand { get; }
        public IRelayCommand HomeCommand { get; }
        public IRelayCommand JourneyCommand { get; }
        public IRelayCommand LightColorSchemeCommand { get; }
        public IRelayCommand NewTabCommand { get; }
        public IRelayCommand ReloadCommand { get; }
        public BrowserTab? SelectedTab
        {
            get => _selectedTab;
            set
            {
                if (_selectedTab != value)
                {
                    _selectedTab = value;
                    NotifyPropertyChanged();

                    // If the selected tab has changed, the UI will need to update it's buttons.
                    UpdateCommandStates();
                }
            }
        }
        public Func<BrowserTab> TabFactory => () => new BrowserTab(Settings.HomePage);
        public ObservableCollection<BrowserTab> Tabs { get; }

        #endregion

        #region Methods

        #region Event Handlers

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
        private void ExecutedAutoColorSchemeCommand()
        {
            Settings.ColorScheme = CoreWebView2PreferredColorScheme.Auto;
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
            Settings.ColorScheme = CoreWebView2PreferredColorScheme.Dark;
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
            Settings.ColorScheme = CoreWebView2PreferredColorScheme.Light;
            NotifyPropertyChanged(nameof(DarkMode));
        }
        private void ExecutedNewTabCommand()
        {
            CreateTab(Settings.HomePage);
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
            BackCommand.NotifyCanExecuteChanged();
            ForwardCommand.NotifyCanExecuteChanged();
            JourneyCommand.NotifyCanExecuteChanged();
        }

        #endregion

        #region Public

        public void CreateTab(string address)
        {
            var newTab = new BrowserTab(address);
            Tabs.Add(newTab);
            SelectedTab = newTab;
        }
        public void UpdateCommandStates(BrowserTab tab)
        {
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