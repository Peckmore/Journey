using CommunityToolkit.Mvvm.Input;
using Journey;
using JourneyBrowser.Interop;
using JourneyBrowser.Models;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.Wpf;
using Microsoft.Win32;
using ModernWpf;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;

namespace JourneyBrowser
{
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        #region Constants

        private const string HomePage = @"https://start.duckduckgo.com";

        #endregion

        #region Fields

        #region Private

        private CoreWebView2PreferredColorScheme _colorScheme;
        private bool _isDarkMode;
        private BrowserTab? _selectedTab;

        #endregion

        #region Public Static

        /// <summary>
        /// Command to close a browsing tab.
        /// </summary>
        public static readonly ICommand CloseTabCommand = new RoutedCommand();
        /// <summary>
        /// Command to navigate the current tab to the Home page.
        /// </summary>
        public static readonly ICommand HomeCommand = new RoutedCommand();
        /// <summary>
        /// Command to create a new browsing tab.
        /// </summary>
        public static readonly ICommand NewTabCommand = new RoutedCommand();
        /// <summary>
        /// Command to refresh the current tab.
        /// </summary>
        public static readonly ICommand RefreshCommand = new RoutedCommand();

        #endregion

        #endregion

        #region Events

        /// <inheritdoc />
        public event PropertyChangedEventHandler? PropertyChanged;

        #endregion

        #region Construction

        public MainWindow()
        {
            // Set fields and properties
            _colorScheme = CoreWebView2PreferredColorScheme.Auto;
            Tabs = new();

            // Wire up our commands
            BackCommand = new RelayCommand(ExecutedBackCommand, CanExecuteBackCommand);
            ForwardCommand = new RelayCommand(ExecutedForwardCommand, CanExecuteForwardCommand);
            JourneyCommand = new RelayCommand(ExecutedJourneyCommand, CanExecuteJourneyCommand);

            // Initialize window.
            InitializeComponent();

            // Merge in the appropriate resource dictionary dependent upon whether the OS is in light or dark mode.
            ApplyTheme();

            SystemEvents.UserPreferenceChanged += SystemEvents_UserPreferenceChanged;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Command to navigate the current tab back one step in it's session history.
        /// </summary>
        public IRelayCommand BackCommand { get; }
        /// <summary>
        /// Command to navigate the current tab forward one step in it's session history.
        /// </summary>
        public IRelayCommand ForwardCommand { get; }
        /// <summary>
        /// Command to open the Journey view for the current tab.
        /// </summary>
        public IRelayCommand JourneyCommand { get; }
        public BrowserTab? SelectedTab
        {
            get => _selectedTab;
            set
            {
                if (_selectedTab != value)
                {
                    _selectedTab = value;
                    OnPropertyChanged();
                }
            }
        }
        public ObservableCollection<BrowserTab> Tabs { get; }

        #endregion

        #region Methods

        #region Event Handlers

        private void AddressBar_GotFocus(object sender, RoutedEventArgs e)
        {
            if (sender is TextBox textBox)
            {
                textBox.SelectAll();
            }
        }
        private void AddressBar_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (sender is TextBox textBox)
                {
                    var binding = textBox.GetBindingExpression(TextBox.TextProperty);
                    if (binding != null)
                    {
                        binding.UpdateSource();
                    }
                    BrowserTabControl.Focus();
                }
            }
        }
        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is ToggleButton settingsButton)
            {
                // Get the context menu for the button.
                var menu = settingsButton.ContextMenu;

                // Update the positioning for the context menu.
                menu.PlacementTarget = settingsButton;
                menu.Placement = PlacementMode.Bottom;

                // Open the context menu.
                menu.IsOpen = true;
            }
        }
        private void SettingsMenu_Closed(object sender, RoutedEventArgs e)
        {
            if (sender is ContextMenu menu && menu.PlacementTarget is ToggleButton button)
            {
                // Set our button to not be checked when the menu is closed.
                button.IsChecked = false;
            }
        }
        private void SettingsMenu_Opened(object sender, RoutedEventArgs e)
        {
            if (sender is ContextMenu menu && menu.PlacementTarget is ToggleButton button)
            {
                // Align right edge of the menu to the right edge of the button.
                menu.HorizontalOffset = button.ActualWidth - menu.ActualWidth + 10;
                menu.VerticalOffset = -10;
            }
        }
        private void WebView_CoreWebView2InitializationCompleted(object? sender, CoreWebView2InitializationCompletedEventArgs e)
        {
            if (sender is IWebView2 webView2)
            {
                webView2.CoreWebView2InitializationCompleted -= WebView_CoreWebView2InitializationCompleted;
                webView2.CoreWebView2.NewWindowRequested += WebView_NewWindowRequested;
            }
        }
        private void WebView_Navigation(object? sender, EventArgs e)
        {
            if (sender is JourneyWebView2 { DataContext: BrowserTab tabViewModel } webView)
            {
                tabViewModel.CanGoBack = webView.CanGoBack;
                tabViewModel.CanGoForward = webView.CanGoForward;
                tabViewModel.CanShowJourney = webView.CanShowJourney;
                tabViewModel.Title = webView.CoreWebView2.DocumentTitle;
                BackCommand.NotifyCanExecuteChanged();
                ForwardCommand.NotifyCanExecuteChanged();
                JourneyCommand.NotifyCanExecuteChanged();
            }
        }
        private void WebView_NewWindowRequested(object? sender, CoreWebView2NewWindowRequestedEventArgs e)
        {
            e.Handled = true;
            CreateTab(e.Uri);
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            RefreshFrame();
            CreateTab(HomePage);
        }

        #endregion

        #region Private
        
        private void ApplyTheme()
        {
            // Set a flag to indicate whether we should use dark mode.
            var isDark = false;
            if (_colorScheme == CoreWebView2PreferredColorScheme.Dark)
            {
                // If the WebView2 profile is set to dark mode, we'll use that.
                isDark = true;
            }
            else if (_colorScheme == CoreWebView2PreferredColorScheme.Auto)
            {
                // If the WebView2 profile is set to auto, we'll check the registry to see whether the app should be in light or dark mode.
                using (var themeRegistryKey = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize"))
                {
                    isDark = (themeRegistryKey?.GetValue("AppsUseLightTheme") as int? ?? 1) == 0;
                }
            }

            if (_isDarkMode != isDark)
            {
                _isDarkMode = isDark;

                // Remove any previously merged dictionary and merge in the appropriate dictionary based on the current light/dark mode.
                var themeDictionary = new ResourceDictionary
                {
                    Source = new Uri(isDark ? "pack://application:,,,/Resources/Themes/Theme.Dark.xaml"
                                            : "pack://application:,,,/Resources/Themes/Theme.Light.xaml", UriKind.Absolute)
                };

                var dictionariesToRemove = Resources.MergedDictionaries
                                                                         .Where(d => d.Source != null && (d.Source.OriginalString.Contains("Theme.Dark.xaml")
                                                                             || d.Source.OriginalString.Contains("Theme.Light.xaml"))).ToList();

                foreach (var dict in dictionariesToRemove)
                {
                    Resources.MergedDictionaries.Remove(dict);
                }

                Resources.MergedDictionaries.Add(themeDictionary);
            }
        }
        private bool CanExecuteBackCommand()
        {
            return SelectedTab?.CanGoBack ?? false;
        }
        private bool CanExecuteForwardCommand()
        {
            return SelectedTab?.CanGoForward ?? false;
        }
        private bool CanExecuteJourneyCommand()
        {
            return SelectedTab?.CanShowJourney ?? false;
        }
        private void CloseTab(BrowserTab tab)
        {
            if (Tabs.Contains(tab))
            {
                var selectedIndex = Tabs.IndexOf(tab);
                if (selectedIndex > 0)
                {
                    SelectedTab = Tabs[selectedIndex- 1];
                }
                else if (selectedIndex == 0 && Tabs.Count > 1)
                {
                    SelectedTab = Tabs[selectedIndex + 1];
                }

                Tabs.Remove(tab);
            }
        }
        private void CreateTab(string address)
        {
            var newTab = new BrowserTab(address);
            Tabs.Add(newTab);
            BrowserTabControl.SelectedItem = newTab;
        }
        private void ExecutedBackCommand()
        {
            GetCurrentWebView()?.GoBack();
        }
        private void ExecutedCloseTabCommand(object sender, ExecutedRoutedEventArgs e)
        {
            if (e.Parameter is BrowserTab tab)
            {
                CloseTab(tab);
            }
        }
        private void ExecutedForwardCommand()
        {
            GetCurrentWebView()?.GoForward();
        }
        private void ExecutedHomeCommand(object sender, ExecutedRoutedEventArgs e)
        {
            if (GetCurrentWebView() is { } webView)
            {
                webView.Source = new(HomePage);
            }
        }
        private async void ExecutedJourneyCommand()
        {
            if (GetCurrentWebView() is JourneyWebView2 webView)
            {
                await webView.ToggleJourney();
            }
        }
        private void ExecutedNewTabCommand(object sender, ExecutedRoutedEventArgs e)
        {
            CreateTab(HomePage);
        }
        private void ExecutedRefreshCommand(object sender, ExecutedRoutedEventArgs e)
        {
            GetCurrentWebView()?.Reload();
        }
        private T? FindVisualChild<T>(DependencyObject obj)
            where T : DependencyObject
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(obj); i++)
            {
                var child = VisualTreeHelper.GetChild(obj, i);
                if (child != null && child is T)
                {
                    return (T)child;
                }
                else
                {
                    T childOfChild = FindVisualChild<T>(child);
                    if (childOfChild != null)
                        return childOfChild;
                }
            }
            return null;
        }
        private IWebView2? GetCurrentWebView()
        {
            if (BrowserTabControl.ItemContainerGenerator.ContainerFromItem(BrowserTabControl.SelectedItem) is { } tabItem)
            {
                var webView = FindVisualChild<JourneyWebView2>(BrowserTabControl);
                return webView;
            }

            return null;
        }
        private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion

        #endregion


        void OnLoaded(object sender, RoutedEventArgs e)
        {
            RefreshDarkMode();
            ThemeManager.Current.ActualApplicationThemeChanged += (_, _) => RefreshDarkMode();
        }

        private void RefreshFrame()
        {
            IntPtr mainWindowPtr = new WindowInteropHelper(this).Handle;
            HwndSource mainWindowSrc = HwndSource.FromHwnd(mainWindowPtr);
            mainWindowSrc.CompositionTarget.BackgroundColor = Color.FromArgb(0, 0, 0, 0);

            NativeMethods.SetWindowAttribute(new WindowInteropHelper(this).Handle,
                               DWMWINDOWATTRIBUTE.DWMWA_SYSTEMBACKDROP_TYPE,
                               (int)DWM_SYSTEMBACKDROP_TYPE.DWMSBT_TRANSIENTWINDOW);
        }

        private void RefreshDarkMode()
        {
            using var key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize");
            var isDark = (key?.GetValue("AppsUseLightTheme") as int? ?? 1) == 0;
            int flag = isDark ? 1 : 0;
            NativeMethods.SetWindowAttribute(
                new WindowInteropHelper(this).Handle,
                DWMWINDOWATTRIBUTE.DWMWA_USE_IMMERSIVE_DARK_MODE,
                flag);
            NativeMethods.SetWindowAttribute(
                new WindowInteropHelper(this).Handle,
                DWMWINDOWATTRIBUTE.DWMWA_SYSTEMBACKDROP_TYPE,
                (int)DWM_SYSTEMBACKDROP_TYPE.DWMSBT_TRANSIENTWINDOW);
        }
        private void SystemEvents_UserPreferenceChanged(object sender, UserPreferenceChangedEventArgs e)
        {
            if (e.Category == UserPreferenceCategory.General)
            {
                RefreshDarkMode();
            }
        }
    }
}