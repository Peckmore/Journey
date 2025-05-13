using Journey;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.Wpf;
using ModernWpf;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using SystemBackdropTypes;
using static SystemBackdropTypes.PInvoke.Methods;
using static SystemBackdropTypes.PInvoke.ParameterTypes;


namespace WPFSample
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            Loaded += OnLoaded;
        }
        public ObservableCollection<TabItem> WebView2Tabs
        {
            get => _webView2Tabs;
            set
            {
                if (_webView2Tabs == value)
                    return;

                //set value
                _webView2Tabs = value;

                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        private int _tabCount = 0;
        private int _selectedIndex = 0;

        private ObservableCollection<TabItem> _webView2Tabs = new ObservableCollection<TabItem>();
        public int SelectedIndex
        {
            get { return _selectedIndex; }
            set
            {
                if (_selectedIndex == value)
                    return;

                //set value
                _selectedIndex = value;

                OnPropertyChanged(nameof(SelectedIndex));
            }
        }



        private async void ButtonJourney_Click(object sender, RoutedEventArgs e)
        {
            var view = ((JourneyWebView2)_webView2Tabs[SelectedIndex].Content);
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
            //    c?.BeginAnimation(DropShadowEffect.OpacityProperty, fadeAnimation);

            await view.ToggleJourney();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            AddTab("https://www.microsoft.com");
        }


        private void AddTab(string url, string? headerText = null, string? userDataFolder = null)
        {
            AddTab(new Uri(url), headerText, userDataFolder);
        }
        private void AddTab(Uri uri, string? headerText = null, string? userDataFolder = null)
        {
            //increment
            _tabCount++;

            if (headerText == null)
                headerText = $"Tab {_tabCount}";

            //if userDataFolder hasn't been specified, create a folder in the user's temp folder
            //each WebView2 instance will have it's own folder
            if (String.IsNullOrEmpty(userDataFolder))
                userDataFolder = System.IO.Path.Combine(System.IO.Path.GetTempPath(), System.IO.Path.GetFileNameWithoutExtension(System.Reflection.Assembly.GetExecutingAssembly().Location) + _tabCount);

            //create new instance setting userDataFolder
            JourneyWebView2 journeyWebView = new JourneyWebView2();
            journeyWebView.WebView.CoreWebView2InitializationCompleted += WebView2_CoreWebView2InitializationCompleted;

            //create TextBlock
            TextBlock textBlock = new TextBlock();

            //add new Run to TextBlock
            textBlock.Inlines.Add(new Run(headerText));

            //add new Run to TextBlock
            textBlock.Inlines.Add(new Run("   "));

            //create Run
            Run runHyperlink = new Run("X");
            runHyperlink.FontFamily = new FontFamily("Monotype Corsiva");
            runHyperlink.FontWeight = FontWeights.Bold;
            runHyperlink.Foreground = new SolidColorBrush(Colors.Red);

            //add Run to HyperLink
            Hyperlink hyperlink = new Hyperlink(runHyperlink) { Name = $"hyperlink_{_tabCount}" };
            hyperlink.Click += Hyperlink_Click;

            //add Hyperlink to TextBlock
            textBlock.Inlines.Add(hyperlink);

            //create new instance and set Content
            HeaderedContentControl hcc = new HeaderedContentControl() { Content = textBlock };

            //add TabItem
            _webView2Tabs.Add(new TabItem { Header = hcc, Content = journeyWebView, Name = $"tab_{_tabCount}" });

            //navigate
            journeyWebView.Source = uri;

            //set selected index
            BrowserTabs.SelectedIndex = _webView2Tabs.Count - 1;
        }

        private void WebView2_CoreWebView2InitializationCompleted(object? sender, CoreWebView2InitializationCompletedEventArgs e)
        {
            if (sender is WebView2 webView2)
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

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void RemoveTab(int index)
        {
            if (index >= 0 && index < _webView2Tabs.Count)
            {
                JourneyWebView2 wv = (JourneyWebView2)_webView2Tabs[index].Content;

                //get userDataFolder location
                string userDataFolder = wv.WebView.CoreWebView2.Environment.UserDataFolder;
                //string userDataFolder = wv.WebView2.CreationProperties.UserDataFolder;

                //unsubscribe from event(s)
                wv.WebView.CoreWebView2.NewWindowRequested -= CoreWebView2_NewWindowRequested;

                //get process
                var wvProcess = Process.GetProcessById((int)wv.WebView.CoreWebView2.BrowserProcessId);

                //dispose
                wv.Dispose();

                //TabItem item = _webView2Tabs[index];
                LogMsg($"Removing {_webView2Tabs[index].Name}");

                //remove
                _webView2Tabs.RemoveAt(index);

                //wait for WebView2 process to exit
                //wvProcess.WaitForExit();

                ////for security purposes, delete userDataFolder
                //if (!String.IsNullOrEmpty(userDataFolder) && System.IO.Directory.Exists(userDataFolder))
                //{
                //    System.IO.Directory.Delete(userDataFolder, true);
                //    LogMsg($"UserDataFolder '{userDataFolder}' deleted.");
                //}
            }
            else
            {
                LogMsg($"Invalid index: {index}; _webView2Tabs.Count: {_webView2Tabs.Count}");
            }
        }


        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            if (_webView2Tabs.Count > 0)
            {
                //get instance of WebView2 from last tab
                JourneyWebView2 wv = (JourneyWebView2)_webView2Tabs[_webView2Tabs.Count - 1].Content;

                //if CoreWebView2 hasn't finished initializing, it will be null
                if (wv.WebView.CoreWebView2?.BrowserProcessId > 0)
                {
                    await wv.WebView.CoreWebView2.ExecuteScriptAsync($@"window.open('https://www.google.com/', '_blank');");
                }
            }
            else
            {
                AddTab("https://www.microsoft.com");
            }
        }

        private async void CoreWebView2_NewWindowRequested(object? sender, CoreWebView2NewWindowRequestedEventArgs e)
        {
            e.Handled = true;

            AddTab(e.Uri);
        }

        private void Hyperlink_Click(object sender, RoutedEventArgs e)
        {
            Hyperlink hyperlink = (Hyperlink)sender;

            LogMsg($"Hyperlink_Click - name: {hyperlink.Name}");

            string hyperLinkNumStr = hyperlink.Name.Substring(hyperlink.Name.IndexOf("_") + 1);
            int hyperLinkNum = 0;

            //try to convert to int
            Int32.TryParse(hyperLinkNumStr, out hyperLinkNum);

            int index = 0;

            //it's possible that an 'X' was clicked on a tab that wasn't selected
            //since both the tab name and hyperlink name end with the same number,
            //get the number from the hyperlink name and use that to find the matching 
            //tab name
            for (int i = 0; i < _webView2Tabs.Count; i++)
            {
                TabItem item = _webView2Tabs[i];

                if (item.Name == $"tab_{hyperLinkNum}")
                {
                    index = i;
                    break;
                }
            }

            //set selected index
            BrowserTabs.SelectedIndex = index;

            RemoveTab(index);
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            if (_webView2Tabs != null && _webView2Tabs.Count > 0)
            {
                for (int i = 0; i < _webView2Tabs.Count - 1; i++)
                {
                    //remove all tabs which will dispose of each WebView2
                    RemoveTab(i);
                }
            }
        }



        void OnLoaded(object sender, RoutedEventArgs e)
        {
            RefreshFrame();
            RefreshDarkMode();
            ThemeManager.Current.ActualApplicationThemeChanged += (_, _) => RefreshDarkMode();
        }

        private void RefreshFrame()
        {
            IntPtr mainWindowPtr = new WindowInteropHelper(this).Handle;
            HwndSource mainWindowSrc = HwndSource.FromHwnd(mainWindowPtr);
            mainWindowSrc.CompositionTarget.BackgroundColor = Color.FromArgb(0, 0, 0, 0);

            //int flag = int.Parse((string)((RadioButton)sender).Tag);
            SetWindowAttribute(
                new WindowInteropHelper(this).Handle,
                PInvoke.ParameterTypes.DWMWINDOWATTRIBUTE.DWMWA_SYSTEMBACKDROP_TYPE,
                (int)DWM_SYSTEMBACKDROP_TYPE.DWMSBT_TRANSIENTWINDOW);
        }

        private void RefreshDarkMode()
        {
            var isDark = ThemeManager.Current.ActualApplicationTheme == ApplicationTheme.Dark;
            int flag = isDark ? 1 : 0;
            SetWindowAttribute(
                new WindowInteropHelper(this).Handle,
                PInvoke.ParameterTypes.DWMWINDOWATTRIBUTE.DWMWA_USE_IMMERSIVE_DARK_MODE,
                flag);
        }
    }
}