using Journey.Tree;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.Wpf;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace Journey
{
    /// <summary>
    /// Used to manage the "journey" of a WebView2 instance, tracking navigation history and events.
    /// </summary>
    internal sealed class JourneyManager : IDisposable
    {
        #region Fields

        private readonly List<TreeNode<NavigationEntry>> _activePath;
        private NavigationEntry? _activeStep;
        private readonly Tree<NavigationEntry> _steps;
        private readonly Dictionary<int, TreeNode<NavigationEntry>> _stepsIndex;
        private readonly WebView2 _webView;

        #endregion

        #region Construction

        internal JourneyManager(WebView2 webView)
        {
            // Set our fields.
            _activePath = [];
            _steps = new(new(-1, "Root", "Root", string.Empty, string.Empty));
            _stepsIndex = [];
            _webView = webView;

            // Hook into the required WebView2 events of the supplied WebView2 instance.
            _webView.CoreWebView2InitializationCompleted += WebView_CoreWebView2InitializationCompleted;
            _webView.NavigationCompleted += WebView_NavigationCompleted;
            _webView.NavigationStarting += WebView_NavigationStarting;
        }

        #endregion

        #region Methods

        #region Event Handlers

        private async void CoreWebView2_HistoryChanged(object? sender, object e)
        {
            // This event handler tracks whenever the WebView2 navigation history changes, which seems to be the most reliable way of
            // detecting "genuine" navigation changes (i.e., not just script changes or other non-navigation events).

            // First, reset the status of all nodes in the active path. We only need to worry about the active path, as all nodes not in
            // the active path will already be in the correct (archived) state. Once we've done that, clear the active path as the user
            // may have branched away from an earlier step.
            foreach (var step in _activePath)
            {
                step.Value.Type = NavigationEntryType.ArchivedStep;
            }
            _activePath.Clear();

            // Now, we need to get the current "session history" from the WebView2 instance, which we do using the Dev Tools. The history
            // is given to us as a JSON string, which we'll then deserialize.
            var historyJson = await _webView.CoreWebView2.CallDevToolsProtocolMethodAsync("Page.getNavigationHistory", "{}");
            var history = JsonConvert.DeserializeObject<SessionHistory>(historyJson)!;

            // Our journey tree is created with a dummy root node. Once we get navigation history, we update the root node with the real
            // information.
            if (_steps.Value.Id == -1)
            {
                var entry = history.Entries[0];
                _steps.Value.Update(entry);
                _stepsIndex[entry.Id] = _steps;
            }

            // Now we're going to work through our existing tree and update it with the new NavigationEntry items. If an entry already
            // exists, we'll update it, otherwise we'll add it.
            TreeNode<NavigationEntry> root = _steps;
            for (var i = 0; i < history.Entries.Count; i++)
            {
                // Grab the next NavigationEntry.
                var entry = history.Entries[i];

                // If the entry already exists in our index, update it, otherwise add it to the index and tree.
                if (_stepsIndex.TryGetValue(entry.Id, out var value))
                {
                    value.Value.Update(entry);
                }
                else
                {
                    var node = root.Add(entry);

                    // We keep a dictionary of all the tree nodes, indexed against their ID, so we can quickly find them later, rather than
                    // searching the tree.
                    _stepsIndex[entry.Id] = node;
                }

                // Grab the newly added/updated node to work with it.
                var step = _stepsIndex[entry.Id];

                // Add this step to the active path.
                _activePath.Add(step);

                // Now set the NavigationEntry type, based on whether it is before or after the index of the current page.
                if (i < history.CurrentIndex)
                {
                    // The step is before the current index, so it is a history back step.
                    step.Value.Type = NavigationEntryType.HistoryBack;
                }
                else if (i == history.CurrentIndex)
                {
                    // The step is the current active step.
                    step.Value.Type = NavigationEntryType.ActiveStep;
                    _activeStep = step.Value;
                }
                else if (i > history.CurrentIndex)
                {
                    // The step is after the current index, so it is a history forward step.
                    step.Value.Type = NavigationEntryType.HistoryForward;
                }

                // Set our current treenode to the newly added/updated node.
                root = step;
            }
        }
        private void WebView_CoreWebView2InitializationCompleted(object? sender, CoreWebView2InitializationCompletedEventArgs e)
        {
            // Once the CoreWebView2 instance has initialized, we can remove the handler to the event, and add a handler to the history
            // changed event.

            _webView.CoreWebView2InitializationCompleted -= WebView_CoreWebView2InitializationCompleted;
            _webView.CoreWebView2.HistoryChanged += CoreWebView2_HistoryChanged;
        }
        private async void WebView_NavigationCompleted(object? sender, CoreWebView2NavigationCompletedEventArgs e)
        {
            // When the WebView2 instance signals that navigation has completed, we'll update the snapshot and title of the active step
            // to make sure they are accurate (as the information may not have been available when the step was created).
            await UpdateActiveStep();
        }
        private async void WebView_NavigationStarting(object? sender, CoreWebView2NavigationStartingEventArgs e)
        {
            // We're leaving the current step, so update the snapshot and title.
            await UpdateActiveStep();
        }

        #endregion

        #region Private

        private async Task UpdateActiveStep()
        {
            // This methods just grabs the title and a snapshot of the current page within the WebView2 instance, and updates the ActiveStep
            // with this information. If the ActiveStep is null, then we do nothing.

            if (_activeStep != null)
            {
                // Update the title.
                _activeStep.Title = _webView.CoreWebView2.DocumentTitle;

                // Update the FavIcon.
                using (var stream = await _webView.CoreWebView2.GetFaviconAsync(CoreWebView2FaviconImageFormat.Png))
                {
                    if (stream is { Length: > 0 })
                    {
                        _activeStep.FavIcon = BitmapFrame.Create(stream);
                    }
                }

                // Update the snapshot.
                using (var snapshotStream = new MemoryStream())
                {
                    try
                    {
                        await _webView.CoreWebView2.CapturePreviewAsync(CoreWebView2CapturePreviewImageFormat.Png, snapshotStream);
                        var snapshot = BitmapFrame.Create(snapshotStream, BitmapCreateOptions.IgnoreImageCache, BitmapCacheOption.OnLoad);
                        if (snapshot != null)
                        {
                            // We only update the snapshot if the capture did not return null, to prevent replacing a potentially valid
                            // snapshot with a null value.
                            _activeStep.Snapshot = snapshot;
                        }
                    }
                    catch
                    {
                        // If the capture fails there is nothing we can do, so just swallow the exception.
                    }
                }
            }
        }

        #endregion

        #region Public

        /// <inheritdoc />
        public void Dispose()
        {
            // Clean up our collections.
            _activePath.Clear();
            _activeStep = null;
            _steps.Clear();
            _stepsIndex.Clear();

            // Unhook our event handlers from the WebView2 instance.
            _webView.CoreWebView2InitializationCompleted -= WebView_CoreWebView2InitializationCompleted;
            _webView.CoreWebView2.HistoryChanged -= CoreWebView2_HistoryChanged;
            _webView.NavigationCompleted -= WebView_NavigationCompleted;
            _webView.NavigationStarting -= WebView_NavigationStarting;
        }
        /// <summary>
        /// Returns the current journey of the WebView2 instance.
        /// </summary>
        /// <returns>A tree of journey steps, with each node representing a visited page.</returns>
        public async Task<TreeNode<NavigationEntry>> GetJourney()
        {
            // Update the active step before returning the journey, so it represents what the user is currently looking at in the
            // WebView2 instance.
            await UpdateActiveStep();

            // Return our journey tree.
            return _steps;
        }
        /// <summary>
        /// Initiates a navigation to a specific step in the journey.
        /// </summary>
        /// <param name="step">The journey step to navigate to.</param>
        /// <exception cref="NotSupportedException">Thrown if the specified step is of an unsupported type.</exception>
        /// <remarks>Due to limitations in how we can manipulate the history of a WebView2 instance, any Archived journey step (a step
        /// that is not currently in the browser history) will be opened in a new tab.</remarks>
        public async Task GoToStep(NavigationEntry step)
        {
            switch (step.Type)
            {
                case NavigationEntryType.ActiveStep:
                    // Do nothing, we're already on the active step.
                    return;
                case NavigationEntryType.ArchivedStep:
                    // Navigate to the archived step URL by opening it in a new tab.
                    await _webView.CoreWebView2.ExecuteScriptAsync($"window.open('{step.Url}', '_blank');");
                    break;
                case NavigationEntryType.HistoryBack:
                case NavigationEntryType.HistoryForward:
                    // For history back and forward steps, we can navigate to the step using DevTools to jump to the specified history
                    // step.
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