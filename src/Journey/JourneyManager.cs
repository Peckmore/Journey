using Journey.Collections;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.Wpf;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace Journey
{
    internal sealed class JourneyManager : IDisposable
    {
        #region Fields

        private IList<TreeNode<JourneyEntry>> _activePath;
        private readonly Dictionary<int, TreeNode<JourneyEntry>> _stepsIndex;
        private readonly Tree<JourneyEntry> _steps;
        private SemaphoreSlim _transitionSemaphore = new(1, 1);
        private readonly WebView2 _webView;

        #endregion

        #region Construction

        internal JourneyManager(WebView2 webView)
        {
            _activePath = new List<TreeNode<JourneyEntry>>();
            _stepsIndex = new();
            _steps = new(new(0, "Root", "Root", string.Empty, string.Empty));
            _webView = webView;

            _webView.CoreWebView2InitializationCompleted += WebView_CoreWebView2InitializationCompleted;
            _webView.NavigationCompleted += WebView_NavigationCompleted;
            _webView.NavigationStarting += WebView_NavigationStarting;
        }

        #endregion

        #region Properties

        #region Private

        private JourneyEntry? ActiveStep { get; set; }

        #endregion

        #endregion

        #region Methods

        #region Event Handlers

        private async void CoreWebView2_HistoryChanged(object? sender, object e)
        {
            await _transitionSemaphore.WaitAsync();

            try
            {
                // First, reset the status of all nodes in the active path
                foreach (var step in _activePath)
                {
                    step.Value.Type = JourneyEntryType.ArchivedStep;
                }
                _activePath.Clear();

                TreeNode<JourneyEntry> root = _steps;
                var history = await GetNavigationHistory();
                for (var i = 0; i < history.Entries.Count; i++)
                {
                    var entry = history.Entries[i];

                    if (_stepsIndex.ContainsKey(entry.Id))
                    {
                        _stepsIndex[entry.Id].Value.Update(entry);
                    }
                    else
                    {
                        var node = root.Add(entry);
                        _stepsIndex[entry.Id] = node;
                    }

                    var step = _stepsIndex[entry.Id];
                    _activePath.Add(step);

                    if (i < history.CurrentIndex)
                    {
                        step.Value.Type = JourneyEntryType.HistoryBack;
                    }
                    else if (i == history.CurrentIndex)
                    {
                        step.Value.Type = JourneyEntryType.ActiveStep;
                        ActiveStep = step.Value;
                    }
                    else if (i > history.CurrentIndex)
                    {
                        step.Value.Type = JourneyEntryType.HistoryForward;
                    }

                    root = step;
                }
            }
            finally
            {
                _transitionSemaphore.Release();
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

        private async Task<NavigationHistory> GetNavigationHistory()
        {
            var history = await _webView.CoreWebView2.CallDevToolsProtocolMethodAsync("Page.getNavigationHistory", "{}");
            return JsonConvert.DeserializeObject<NavigationHistory>(history)!;
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
            if (ActiveStep != null)
            {
                var snapshot = await TakeSnapshot();
                if (snapshot != null)
                {
                    ActiveStep.Snapshot = snapshot;
                }
            }
        }

        #endregion

        #region Public

        public void Dispose()
        {
            _webView.NavigationStarting -= WebView_NavigationStarting;
            _steps.Clear();
        }
        public async Task<TreeNode<JourneyEntry>> GetJourney()
        {
            // Update current webpage snapshot
            await UpdateActiveStepSnapshot();
            return _steps.Children[0];
        }
        public async Task GoToStep(JourneyEntry step)
        {
            switch (step.Type)
            {
                case JourneyEntryType.ActiveStep:
                    // Do nothing, we're already on the active step.
                    return;
                case JourneyEntryType.ArchivedStep:
                    // Navigate to the archived step URL.
                    await _webView.CoreWebView2.ExecuteScriptAsync($"window.open('{step.Url}', '_blank');");
                    break;
                case JourneyEntryType.HistoryBack:
                case JourneyEntryType.HistoryForward:
                    await _webView.CoreWebView2.CallDevToolsProtocolMethodAsync("Page.navigateToHistoryEntry", JsonConvert.SerializeObject(new { entryId = step.Id }));
                    break;
                default:
                    throw new NotSupportedException($"Unsupported journey entry type: {step.Type}");
            }
        }

        #endregion

        #endregion
    }
}