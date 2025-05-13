using Journey.Tree.Overby.Collections;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.Wpf;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Security.Policy;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Journey
{
    internal class JourneyManager : IDisposable
    {
        #region Fields

        private JourneyEntry? _activeStep;
        private readonly TreeNode<JourneyEntry> _steps;
        private readonly WebView2 _webView;

        #endregion

        #region Construction

        internal JourneyManager(WebView2 webView)
        {
            _webView = webView;

            var root = new JourneyEntry(0, "Root", "Root", string.Empty, string.Empty);
            _steps = new(root);

            _webView.CoreWebView2InitializationCompleted += WebView_CoreWebView2InitializationCompleted;
            _webView.NavigationCompleted += WebView_NavigationCompleted;
            _webView.NavigationStarting += WebView_NavigationStarting;

            _webView.EnsureCoreWebView2Async();
        }

        #endregion

        #region Properties

        #region Private

        private JourneyEntry? ActiveStep
        {
            get => _activeStep;
            set
            {
                if (value != null && _activeStep != value)
                {
                    if (_activeStep != null)
                    {
                        _activeStep.IsActive = false;
                    }

                    _activeStep = value;
                    _activeStep.IsActive = true;
                }
            }
        }

        #endregion

        #region Public

        public bool BrowserCanGoBack { get; private set; }
        public bool BrowserCanGoForward { get; private set; }

        #endregion

        #endregion

        #region Methods

        #region Event Handlers

        private async void CoreWebView2_HistoryChanged(object? sender, object e)
        {
            var history = await GetHistory();
            var root = _steps;
            

            foreach (var entry in history.Entries)
            {
                if (root.Children.FirstOrDefault(x => x.Value.Id == entry.Id) is { } childElement)
                {
                    childElement.Value.Update(entry);
                }
                else
                { 
                    root.AddChild(entry);
                }

                if (entry == history.Entries[history.CurrentIndex])
                {
                    ActiveStep = entry;
                }

                root = root.Children.First(x => x.Value.Id == entry.Id);
            }

        }
        private void WebView_CoreWebView2InitializationCompleted(object? sender, CoreWebView2InitializationCompletedEventArgs e)
        {
            _webView.CoreWebView2InitializationCompleted -= WebView_CoreWebView2InitializationCompleted;
            _webView.CoreWebView2.HistoryChanged += CoreWebView2_HistoryChanged;
        }
        private async void WebView_NavigationCompleted(object? sender, CoreWebView2NavigationCompletedEventArgs e)
        {
            await UpdateActiveStepSnapshot();
            if (ActiveStep != null)
            {
                ActiveStep.Title = _webView.CoreWebView2.DocumentTitle;
            }
        }
        private async void WebView_NavigationStarting(object? sender, CoreWebView2NavigationStartingEventArgs e)
        {
            // We're leaving the current step, so update the snapshot.
            await UpdateActiveStepSnapshot();
        }

        #endregion

        #region Private

        private async Task<HistoryWrapper> GetHistory()
        {
            var history = await _webView.CoreWebView2.CallDevToolsProtocolMethodAsync("Page.getNavigationHistory", "{}");
            return JsonConvert.DeserializeObject<HistoryWrapper>(history)!;
        }
        private async Task<BitmapFrame?> TakeSnapshot()
        {
            using (var snapshotStream = new MemoryStream())
            {
                try
                {
                    await _webView.CoreWebView2.CapturePreviewAsync(CoreWebView2CapturePreviewImageFormat.Png, snapshotStream);
                    return BitmapFrame.Create(snapshotStream, BitmapCreateOptions.IgnoreImageCache, BitmapCacheOption.OnLoad);
                }
                catch
                { }
            }

            return null;
        }
        private async Task UpdateActiveStepSnapshot()
        {
            if (_activeStep != null)
            {
                var snapshot = await TakeSnapshot();
                if (snapshot != null)
                {
                    _activeStep.Snapshot = snapshot;
                }
            }
        }

        #endregion

        #region Public

        public void Dispose()
        {
            _webView.NavigationStarting -= WebView_NavigationStarting;
            //_steps.Clear();
        }
        public async Task<TreeNode<JourneyEntry>> GetJourney()
        {
            // Update current webpage snapshot
            await UpdateActiveStepSnapshot();
            return _steps;
        }
        public async Task GoToStep(JourneyEntry step)
        {
            if (!step.IsActive)
            {
                var history = await GetHistory();
                if (history.Entries.FirstOrDefault(s => s.Id == step.Id) is { })
                {
                    await _webView.CoreWebView2.CallDevToolsProtocolMethodAsync("Page.navigateToHistoryEntry", JsonConvert.SerializeObject(new { entryId = step.Id }));
                }
                else
                {
                    await _webView.CoreWebView2.ExecuteScriptAsync($"window.open('{step.Url}', '_blank');");
                }
            }
        }

        #endregion

        #endregion
    }
}