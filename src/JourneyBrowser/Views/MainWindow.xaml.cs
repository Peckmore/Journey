using Journey;
using JourneyBrowser.Controls;
using JourneyBrowser.Interop;
using JourneyBrowser.Models;
using JourneyBrowser.ViewModels;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.Wpf;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace JourneyBrowser.Views
{
    internal partial class MainWindow
    {
        #region Fields

        private bool? _isDarkMode;
        private JourneyIntroToolTip? _journeyIntroWindow;
        private readonly MainWindowViewModel _viewModel;

        #endregion

        #region Construction

        public MainWindow()
        {
            // Initialize window.
            InitializeComponent();

            // Set the viewmodel and subscribe to it's property changed events.
            _viewModel = new MainWindowViewModel();
            _viewModel.PropertyChanged += ViewModel_PropertyChanged;
            DataContext = _viewModel;
        }
        public MainWindow(string url)
            : this()
        {
            _viewModel.CreateTab(url);
        }

        #endregion

        #region Methods

        #region Event Handlers

        private void AddressBar_GotFocus(object sender, RoutedEventArgs e)
        {
            // If our address bar TextBox gets focus, select all of the text. This is a UI layer activity, so we don't put this in
            // the viewmodel.
            if (sender is TextBox textBox)
            {
                textBox.SelectAll();
            }
        }
        private void AddressBar_KeyDown(object sender, KeyEventArgs e)
        {
            // We want our address bar TextBox to only update its binding source when the user presses enter, so we'll handle that here in
            // the `KeyDown` event. This is a UI layer activity, so we don't put this in the viewmodel.
            if (e.Key == Key.Enter && sender is TextBox textBox)
            {
                var binding = textBox.GetBindingExpression(TextBox.TextProperty);
                binding?.UpdateSource();
                BrowserTabControl.Focus();
            }
        }
        private void JourneyIntroWindow_Closed(object? sender, EventArgs e)
        {
            // When the Journey intro window is closed, unhook all of the events on the main window as they are no longer needed.
            if (_journeyIntroWindow != null)
            {
                _journeyIntroWindow.Closed -= JourneyIntroWindow_Closed;
            }

            LocationChanged -= Window_LocationChanged;
            SizeChanged -= Window_SizeChanged;
            StateChanged -= Window_StateChanged;

            _journeyIntroWindow = null;
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
        private void ViewModel_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(MainWindowViewModel.DarkMode))
            {
                // If the `DarkMode` property on the viewmodel changes, re-apply our theme.
                ApplyTheme();
            }
        }
        private void WebView_CoreWebView2InitializationCompleted(object? sender, CoreWebView2InitializationCompletedEventArgs e)
        {
            // We could/should move this into the viewmodel, but for an app this small we'll just roll with it in the code behind.

            if (sender is IWebView2 webView2)
            {
                webView2.CoreWebView2InitializationCompleted -= WebView_CoreWebView2InitializationCompleted;
                webView2.CoreWebView2.NewWindowRequested += WebView_NewWindowRequested;
            }
        }
        private void WebView_Loaded(object sender, RoutedEventArgs e)
        {
            // We could/should move this into the viewmodel, but for an app this small we'll just roll with it in the code behind.

            if (sender is JourneyWebView2 { DataContext: BrowserTab browserTab } webView2)
            {
                browserTab.SetupActions(webView2.GoBack, webView2.GoForward, webView2.Reload, webView2.ToggleJourney);
            }
        }
        private async void WebView_Navigation(object? sender, EventArgs e)
        {
            // We could/should move this into the viewmodel, but for an app this small we'll just roll with it in the code behind.

            if (sender is JourneyWebView2 { DataContext: BrowserTab tabViewModel } webView)
            {
                tabViewModel.CanGoBack = webView.CanGoBack;
                tabViewModel.CanGoForward = webView.CanGoForward;
                tabViewModel.CanShowJourney = webView.CanShowJourney;
                tabViewModel.Title = webView.CoreWebView2.DocumentTitle;
                using (var stream = await webView.CoreWebView2.GetFaviconAsync(CoreWebView2FaviconImageFormat.Png))
                {
                    tabViewModel.FavIcon = stream == null || stream.Length == 0 ? null : BitmapFrame.Create(stream);
                }

                _viewModel.UpdateCommandStates(tabViewModel);
            }
        }
        private void WebView_NewWindowRequested(object? sender, CoreWebView2NewWindowRequestedEventArgs e)
        {
            e.Handled = true;
            _viewModel.CreateTab(e.Uri);
        }
        private void Window_Closed(object? sender, EventArgs e)
        {
            // When our window closes, unhook events and cleanup the viewmodel.
            _viewModel.PropertyChanged -= ViewModel_PropertyChanged;
            _viewModel.Dispose();
            DataContext = null;
        }
        private void Window_ContentRendered(object? sender, EventArgs e)
        {
            // Once the window is on-screen, show the Journey intro tooltip.
            ShowJourneyIntro();
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
                NativeMethods.SetWindowAttribute(windowHandle, DWMWINDOWATTRIBUTE.DWMWA_SYSTEMBACKDROP_TYPE, 3); // 3 => DWMSBT_TRANSIENTWINDOW == Acrylic
            }

            // Merge in the appropriate resource dictionary dependent upon whether the OS is in light or dark mode.
            ApplyTheme();
        }
        private void Window_LocationChanged(object? sender, EventArgs e)
        {
            PositionJourneyIntroWindow();
        }
        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            PositionJourneyIntroWindow();
        }
        private void Window_StateChanged(object? sender, EventArgs e)
        {
            PositionJourneyIntroWindow();
        }

        #endregion

        #region Private

        private void ApplyTheme()
        {
            // We only apply the theme if our current theme does not match the theme requested by the viewmodel.
            if (_isDarkMode != _viewModel.DarkMode)
            {
                _isDarkMode = _viewModel.DarkMode;

                NativeMethods.SetWindowAttribute(new WindowInteropHelper(this).Handle, DWMWINDOWATTRIBUTE.DWMWA_USE_IMMERSIVE_DARK_MODE, _viewModel.DarkMode ? 1 : 0);

                // Remove any previously merged dictionary and merge in the appropriate dictionary based on the current light/dark mode.
                var themeDictionary = new ResourceDictionary
                {
                    Source = new Uri(_viewModel.DarkMode ? "pack://application:,,,/Resources/Themes/Theme.dark.xaml"
                                                         : "pack://application:,,,/Resources/Themes/Theme.light.xaml", UriKind.Absolute)
                };

                var dictionariesToRemove = Resources.MergedDictionaries
                                                                         .Where(d => d.Source != null && (d.Source.OriginalString.Contains("Theme.dark.xaml")
                                                                             || d.Source.OriginalString.Contains("Theme.light.xaml"))).ToList();

                foreach (var dictionaryToRemove in dictionariesToRemove)
                {
                    Resources.MergedDictionaries.Remove(dictionaryToRemove);
                    _journeyIntroWindow?.Resources.Remove(dictionaryToRemove);
                }

                Resources.MergedDictionaries.Add(themeDictionary);
                _journeyIntroWindow?.Resources.MergedDictionaries.Add(themeDictionary);
            }
        }
        private void PositionJourneyIntroWindow()
        {
            if (_journeyIntroWindow != null)
            {
                // Grab the journey button, as we want to anchor the tooltip to this button.
                var journeyButton = (Button)BrowserTabControl.Template.FindName("JourneyButton", BrowserTabControl);

                if (GetWindow(journeyButton) is { } journeyButtonWindow)
                {
                    // Get the bottom-left point of the button (relative to itself).
                    var bottomLeft = new Point(0, journeyButton.ActualHeight);

                    // Transform to window coordinates.
                    var bottomLeftInWindow = journeyButton.TransformToAncestor(journeyButtonWindow).Transform(bottomLeft);

                    // Translate the point from window coordinates to screen coordinates.
                    var mainPos = PointToScreen(bottomLeftInWindow);

                    // Now position the window onscreen in the correct location.
                    _journeyIntroWindow.Left = mainPos.X - 30;
                    _journeyIntroWindow.Top = mainPos.Y;
                }
            }
        }
        private void ShowJourneyIntro()
        {
            // Only show the Journey intro window if it isn't already shown.
            if (_journeyIntroWindow == null)
            {
                // Create an instance of the window.
                _journeyIntroWindow = new JourneyIntroToolTip
                {
                    Owner = this
                };

                // Hook up our required events for when the window closes (to clean up), and also when our main window moves so we can
                // update the position of the Journey intro window.
                _journeyIntroWindow.Closed += JourneyIntroWindow_Closed;
                LocationChanged += Window_LocationChanged;
                SizeChanged += Window_SizeChanged;
                StateChanged += Window_StateChanged;

                // Set the initial position for the Journey window.
                PositionJourneyIntroWindow();

                // Apply the appropriate theme dictionary to the Journey window now that it has been created.
                var themeDictionary = new ResourceDictionary
                {
                    Source = new Uri(_viewModel.DarkMode ? "pack://application:,,,/Resources/Themes/Theme.dark.xaml"
                                                         : "pack://application:,,,/Resources/Themes/Theme.light.xaml", UriKind.Absolute)
                };
                _journeyIntroWindow?.Resources.MergedDictionaries.Add(themeDictionary);

                // Now all the setup is done, we can show the intro window.
                _journeyIntroWindow?.Show();
            }
        }

        #endregion

        #endregion
    }
}