using CommunityToolkit.Mvvm.Input;
using Journey;
using JourneyBrowser.Interop;
using JourneyBrowser.Models;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.Wpf;
using Microsoft.Win32;
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
    internal partial class MainWindow : Window, INotifyPropertyChanged
    {
        #region Constants

        public const string HomePage = @"https://start.duckduckgo.com";

        #endregion

        #region Fields

        #region Private

        private CoreWebView2PreferredColorScheme _colorSchemeSetting;
        private bool? _isDarkMode;
        private BrowserTab? _selectedTab;

        #endregion

        #region Public Static

        /// <summary>
        /// Command to switch dark/light mode to match system.
        /// </summary>
        public static readonly ICommand AutoModeCommand = new RoutedCommand();
        /// <summary>
        /// Command to close a browsing tab.
        /// </summary>
        public static readonly ICommand CloseTabCommand = new RoutedCommand();
        /// <summary>
        /// Command to switch to dark mode.
        /// </summary>
        public static readonly ICommand DarkModeCommand = new RoutedCommand();
        /// <summary>
        /// Command to navigate the current tab to the Home page.
        /// </summary>
        public static readonly ICommand HomeCommand = new RoutedCommand();
        /// <summary>
        /// Command to switch to light mode.
        /// </summary>
        public static readonly ICommand LightModeCommand = new RoutedCommand();
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
            _colorSchemeSetting = CoreWebView2PreferredColorScheme.Auto;
            Tabs = new();

            // Wire up our commands
            BackCommand = new RelayCommand(ExecutedBackCommand, CanExecuteBackCommand);
            ForwardCommand = new RelayCommand(ExecutedForwardCommand, CanExecuteForwardCommand);
            JourneyCommand = new RelayCommand(ExecutedJourneyCommand, CanExecuteJourneyCommand);

            // Initialize window.
            InitializeComponent();

            SystemEvents.UserPreferenceChanged += SystemEvents_UserPreferenceChanged;
        }
        public MainWindow(string url)
            : this()
        {
            CreateTab(HomePage);
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
        public Func<BrowserTab> TabFactory => () => new BrowserTab(HomePage);
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
                    binding?.UpdateSource();
                    BrowserTabControl.Focus();
                }
            }
        }
        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            // Get the context menu for the button.
            if (sender is ToggleButton { ContextMenu: { } menu } settingsButton)
            {
                // Update the positioning for the context menu.
                menu.PlacementTarget = settingsButton;
                menu.Placement = PlacementMode.Bottom;

                // Open the context menu.
                menu.IsOpen = true;
            }
        }
        private void SettingsMenu_Closed(object sender, RoutedEventArgs e)
        {
            if (sender is ContextMenu { PlacementTarget: ToggleButton button })
            {
                // Set our button to not be checked when the menu is closed.
                button.IsChecked = false;
            }
        }
        private void SettingsMenu_Opened(object sender, RoutedEventArgs e)
        {
            if (sender is ContextMenu { PlacementTarget: ToggleButton button } menu)
            {
                // Align right edge of the menu to the right edge of the button.
                menu.HorizontalOffset = button.ActualWidth - menu.ActualWidth + 10;
                menu.VerticalOffset = -10;
            }
        }
        private void SystemEvents_UserPreferenceChanged(object sender, UserPreferenceChangedEventArgs e)
        {
            if (e.Category == UserPreferenceCategory.General)
            {
                ApplyTheme();
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
            // Set our window theming.
            var windowHandle = new WindowInteropHelper(this).Handle;
            var windowHandleSource = HwndSource.FromHwnd(windowHandle);
            if (windowHandleSource is { CompositionTarget: not null })
            {
                // Set the window background to black, to allow the Acrylic/Mica effect to be applied.
                windowHandleSource.CompositionTarget.BackgroundColor = Color.FromArgb(0, 0, 0, 0);

                // Set the window attribute to Acrylic.
                NativeMethods.SetWindowAttribute(windowHandle, DWMWINDOWATTRIBUTE.DWMWA_SYSTEMBACKDROP_TYPE, 3); // 3 => DWMSBT_TRANSIENTWINDOW = Acrylic
            }

            // Merge in the appropriate resource dictionary dependent upon whether the OS is in light or dark mode.
            ApplyTheme();
        }

        #endregion

        #region Private
        
        private void ApplyTheme()
        {
            // Set a flag to indicate whether we should use dark mode.
            var darkModeRequested = false;
            if (_colorSchemeSetting == CoreWebView2PreferredColorScheme.Dark)
            {
                // If the WebView2 profile is set to dark mode, we'll use that.
                darkModeRequested = true;
            }
            else if (_colorSchemeSetting == CoreWebView2PreferredColorScheme.Auto)
            {
                // If the WebView2 profile is set to auto, we'll check the registry to see whether the app should be in light or dark mode.
                using (var themeRegistryKey = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize"))
                {
                    darkModeRequested = (themeRegistryKey?.GetValue("AppsUseLightTheme") as int? ?? 1) == 0;
                }
            }

            if (_isDarkMode != darkModeRequested)
            {
                _isDarkMode = darkModeRequested;

                NativeMethods.SetWindowAttribute(new WindowInteropHelper(this).Handle, DWMWINDOWATTRIBUTE.DWMWA_USE_IMMERSIVE_DARK_MODE, darkModeRequested ? 1 : 0);

                // Remove any previously merged dictionary and merge in the appropriate dictionary based on the current light/dark mode.
                var themeDictionary = new ResourceDictionary
                {
                    Source = new Uri(darkModeRequested ? "pack://application:,,,/Resources/Themes/Theme.Dark.xaml"
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
        private void CloseTab()
        {
            var tab = SelectedTab;
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

                if (Tabs.Count == 0)
                {
                    Close();
                }
            }
        }
        private void CreateTab(string address)
        {
            var newTab = new BrowserTab(address);
            Tabs.Add(newTab);
            BrowserTabControl.SelectedItem = newTab;
        }
        private void ExecutedAutoModeCommand(object sender, ExecutedRoutedEventArgs e)
        {
            _colorSchemeSetting = CoreWebView2PreferredColorScheme.Auto;
            ApplyTheme();
        }
        private void ExecutedBackCommand()
        {
            GetCurrentWebView()?.GoBack();
        }
        private void ExecutedCloseTabCommand(object sender, ExecutedRoutedEventArgs e)
        {
            CloseTab();
        }
        private void ExecutedDarkModeCommand(object sender, ExecutedRoutedEventArgs e)
        {
            _colorSchemeSetting = CoreWebView2PreferredColorScheme.Dark;
            ApplyTheme();
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
        private void ExecutedLightModeCommand(object sender, ExecutedRoutedEventArgs e)
        {
            _colorSchemeSetting = CoreWebView2PreferredColorScheme.Light;
            ApplyTheme();
        }
        private void ExecutedNewTabCommand(object sender, ExecutedRoutedEventArgs e)
        {
            CreateTab(HomePage);
        }
        private void ExecutedRefreshCommand(object sender, ExecutedRoutedEventArgs e)
        {
            GetCurrentWebView()?.Reload();
        }
        private T? FindVisualChild<T>(DependencyObject obj) where T : DependencyObject
        {
            for (var i = 0; i < VisualTreeHelper.GetChildrenCount(obj); i++)
            {
                var child = VisualTreeHelper.GetChild(obj, i);
                if (child is T typedChild)
                {
                    return typedChild;
                }

                var childOfChild = FindVisualChild<T>(child);
                if (childOfChild != null)
                {
                    return childOfChild;
                }
            }

            return null;
        }
        private IWebView2? GetCurrentWebView()
        {
            //if (BrowserTabControl.ItemContainerGenerator.ContainerFromItem(BrowserTabControl.SelectedItem) is not null)
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
    }
}