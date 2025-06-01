using JourneyBrowser.Interop;
using JourneyBrowser.Models;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.Wpf;
using Microsoft.Win32;
using ModernWpf;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;


namespace JourneyBrowser
{
    public partial class MainWindow : Window
    {
        #region Constants

        private const string HomePage = @"https://start.duckduckgo.com";

        #endregion

        #region Fields

        private int _selectedIndex;

        #endregion

        #region Events

        public event PropertyChangedEventHandler? PropertyChanged;

        #endregion

        #region Construction

        public MainWindow()
        {
            // Initialize window.
            InitializeComponent();
            DataContext = this;

            // Initialize Properties
            Tabs = new();

            //SystemEvents.UserPreferenceChanged += SystemEvents_UserPreferenceChanged;
        }

        #endregion

        #region Properties

        public int SelectedIndex
        {
            get => _selectedIndex;
            set
            {
                _selectedIndex = value;
                OnPropertyChanged();
            }
        }
        public ObservableCollection<BrowserTab> Tabs { get; }

        #endregion

        #region Methods

        #region Event Handlers

        private void NewTabButton_Click(object sender, RoutedEventArgs e)
        {
            var newTab = CreateTab(HomePage);
            BrowserTabControl.SelectedItem = newTab;
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            RefreshFrame();
            CreateTab(HomePage);
        }

        #endregion

        #region Private

        private BrowserTab CreateTab(string address)
        {
            var tab = new BrowserTab(address);
            Tabs.Add(tab);
            return tab;
        }
        private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion

        #endregion







        private async void ButtonJourney_Click(object sender, RoutedEventArgs e)
        {
            //var view = ((JourneyWebView2)_webView2Tabs[SelectedIndex].Content);
            //DoubleAnimation fadeAnimation;
            //if (view.IsJourneyVisible)
            //{
            //    fadeAnimation = new DoubleAnimation
            //    {
            //        From = 0.55,
            //        To = 0,
            //        Duration = TimeSpan.FromSeconds(2)
            //    };
            //}
            //else
            //{
            //    fadeAnimation = new DoubleAnimation
            //    {
            //        From = 0,
            //        To = 0.55,
            //        Duration = TimeSpan.FromSeconds(2)
            //    };
            //}

            //var c = (_webView2Tabs[SelectedIndex].FindDescendantByName("ButtonBar") as Border);
            //c?.BeginAnimation(DropShadowEffect.OpacityProperty, fadeAnimation);

            //await view.ToggleJourney();
        }


        private void WebView2_CoreWebView2InitializationCompleted(object? sender, CoreWebView2InitializationCompletedEventArgs e)
        {
            if (sender is IWebView2 webView2)
            {
                webView2.CoreWebView2InitializationCompleted -= WebView2_CoreWebView2InitializationCompleted;
                webView2.CoreWebView2.NewWindowRequested += CoreWebView2_NewWindowRequested;
            }
        }

        private void LogMsg(string msg, bool includeTimestamp = true)
        {
            if (includeTimestamp)
                msg = $"{DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss.fff")} - {msg}";

            Debug.WriteLine(msg);
        }



        private void RemoveTab(int index)
        {
            //if (index >= 0 && index < _webView2Tabs.Count)
            //{
            //    JourneyWebView2 wv = (JourneyWebView2)_webView2Tabs[index].Content;

            //    //get userDataFolder location
            //    string userDataFolder = wv.CoreWebView2.Environment.UserDataFolder;
            //    //string userDataFolder = wv.WebView2.CreationProperties.UserDataFolder;

            //    //unsubscribe from event(s)
            //    wv.CoreWebView2.NewWindowRequested -= CoreWebView2_NewWindowRequested;

            //    //get process
            //    var wvProcess = Process.GetProcessById((int)wv.CoreWebView2.BrowserProcessId);

            //    //dispose
            //    wv.Dispose();

            //    //TabItem item = _webView2Tabs[index];
            //    LogMsg($"Removing {_webView2Tabs[index].Name}");

            //    //remove
            //    _webView2Tabs.RemoveAt(index);

            //    //wait for WebView2 process to exit
            //    //wvProcess.WaitForExit();

            //    ////for security purposes, delete userDataFolder
            //    //if (!String.IsNullOrEmpty(userDataFolder) && System.IO.Directory.Exists(userDataFolder))
            //    //{
            //    //    System.IO.Directory.Delete(userDataFolder, true);
            //    //    LogMsg($"UserDataFolder '{userDataFolder}' deleted.");
            //    //}
            //}
            //else
            //{
            //    LogMsg($"Invalid index: {index}; _webView2Tabs.Count: {_webView2Tabs.Count}");
            //}
        }


        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            //if (_webView2Tabs.Count > 0)
            //{
            //    //get instance of WebView2 from last tab
            //    JourneyWebView2 wv = (JourneyWebView2)_webView2Tabs[_webView2Tabs.Count - 1].Content;

            //    //if CoreWebView2 hasn't finished initializing, it will be null
            //    if (wv.CoreWebView2?.BrowserProcessId > 0)
            //    {
            //        await wv.CoreWebView2.ExecuteScriptAsync($@"window.open('{HomePage}', '_blank');");
            //    }
            //}
            //else
            //{
            //    CreateTab(HomePage);
            //}
        }


        private async void CoreWebView2_NewWindowRequested(object? sender, CoreWebView2NewWindowRequestedEventArgs e)
        {
            e.Handled = true;

            CreateTab(e.Uri);
        }

        private void Hyperlink_Click(object sender, RoutedEventArgs e)
        {
            //Hyperlink hyperlink = (Hyperlink)sender;

            //LogMsg($"Hyperlink_Click - name: {hyperlink.Name}");

            //string hyperLinkNumStr = hyperlink.Name.Substring(hyperlink.Name.IndexOf("_") + 1);
            //int hyperLinkNum = 0;

            ////try to convert to int
            //Int32.TryParse(hyperLinkNumStr, out hyperLinkNum);

            //int index = 0;

            ////it's possible that an 'X' was clicked on a tab that wasn't selected
            ////since both the tab name and hyperlink name end with the same number,
            ////get the number from the hyperlink name and use that to find the matching 
            ////tab name
            //for (int i = 0; i < _webView2Tabs.Count; i++)
            //{
            //    TabItem item = _webView2Tabs[i];

            //    if (item.Name == $"tab_{hyperLinkNum}")
            //    {
            //        index = i;
            //        break;
            //    }
            //}

            ////set selected index
            ////BrowserTabs.SelectedIndex = index;

            //RemoveTab(index);
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            //if (_webView2Tabs != null && _webView2Tabs.Count > 0)
            //{
            //    for (int i = 0; i < _webView2Tabs.Count - 1; i++)
            //    {
            //        //remove all tabs which will dispose of each WebView2
            //        RemoveTab(i);
            //    }
            //}
        }



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

        private void ButtonDarkMode_Click(object sender, RoutedEventArgs e)
        {
            //var view = ((JourneyWebView2)_webView2Tabs[SelectedIndex].Content);
            //switch (view.PreferredColorScheme)
            //{
            //    case CoreWebView2PreferredColorScheme.Auto:
            //        view.PreferredColorScheme = CoreWebView2PreferredColorScheme.Light;
            //        break;
            //    case CoreWebView2PreferredColorScheme.Light:
            //        view.PreferredColorScheme = CoreWebView2PreferredColorScheme.Dark;
            //        break;
            //    case CoreWebView2PreferredColorScheme.Dark:
            //        view.PreferredColorScheme = CoreWebView2PreferredColorScheme.Auto;
            //        break;
            //}
        }
    }
}