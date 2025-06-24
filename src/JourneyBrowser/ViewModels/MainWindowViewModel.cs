using CommunityToolkit.Mvvm.Input;
using JourneyBrowser.Models;
using Microsoft.Web.WebView2.Core;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace JourneyBrowser.ViewModels
{
    internal class MainWindowViewModel : INotifyPropertyChanged
    {
        #region Fields

        private CoreWebView2PreferredColorScheme _preferredColorScheme;
        private BrowserTab? _selectedTab;

        #endregion

        #region Events

        public event PropertyChangedEventHandler? PropertyChanged;

        #endregion

        #region Construction

        public MainWindowViewModel()
        {
            // Set fields
            PreferredColorScheme = CoreWebView2PreferredColorScheme.Auto;

            // Set properties
            BackCommand = new RelayCommand(ExecutedBackCommand, CanExecuteBackCommand);
            CloseTabCommand = new RelayCommand(ExecutedCloseTabCommand);
            ForwardCommand = new RelayCommand(ExecutedForwardCommand, CanExecuteForwardCommand);
            HomeCommand = new RelayCommand(ExecutedHomeCommand);
            JourneyCommand = new RelayCommand(ExecutedJourneyCommand, CanExecuteJourneyCommand);
            NewTabCommand = new RelayCommand(ExecutedNewTabCommand);
            ReloadCommand = new RelayCommand(ExecutedReloadCommand);
            Tabs = new();
        }

        #endregion

        #region Properties

        public IRelayCommand BackCommand { get; }
        public IRelayCommand CloseTabCommand { get; }
        public IRelayCommand ForwardCommand { get; }
        public IRelayCommand HomeCommand { get; }
        public IRelayCommand JourneyCommand { get; }
        public IRelayCommand NewTabCommand { get; }
        public CoreWebView2PreferredColorScheme PreferredColorScheme
        {
            get => _preferredColorScheme;
            set
            {
                if (_preferredColorScheme != value)
                {
                    _preferredColorScheme = value;
                    NotifyPropertyChanged();
                }
            }
        }
        public IRelayCommand ReloadCommand { get; }
        public BrowserTab? SelectedTab
        {
            get => _selectedTab;
            set
            {
                if (_selectedTab != value)
                {
                    _selectedTab = value;
                    UpdateCommandStates();
                    NotifyPropertyChanged();
                }
            }
        }
        public Func<BrowserTab> TabFactory => () => new BrowserTab(MainWindow.HomePage);
        public ObservableCollection<BrowserTab> Tabs { get; }

        #endregion

        #region Methods

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
        private void ExecutedBackCommand()
        {
            _selectedTab?.GoBack();
        }
        private void ExecutedCloseTabCommand()
        {
            CloseTab();
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
        private void ExecutedNewTabCommand()
        {
            CreateTab(MainWindow.HomePage);
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

        public void CloseTab()
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
        public void CreateTab(string address)
        {
            var newTab = new BrowserTab(address);
            Tabs.Add(newTab);
            SelectedTab = newTab;
        }
        public void UpdateCommandStates(BrowserTab tab)
        {
            if (tab == _selectedTab)
            {
                UpdateCommandStates();
            }
        }

        #endregion

        #endregion
    }
}