using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.Wpf;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Journey
{
    internal class JourneyManager : IDisposable
    {
        #region Fields

        private bool _hasCompletedFirstNavigation;
        private readonly List<JourneyEntry> _steps;
        private readonly WebView2 _webView;

        #endregion

        #region Construction

        internal JourneyManager(WebView2 webView)
        {
            _webView = webView;
            _steps = new();

            _webView.NavigationStarting += _webView_NavigationStarting;
        }

        #endregion

        #region Methods

        #region Event Handlers

        private async void _webView_NavigationStarting(object? sender, CoreWebView2NavigationStartingEventArgs e)
        {
            if (_hasCompletedFirstNavigation)
            {
                _steps.Add(new()
                {
                    IsActive = false,
                    Url = _webView.Source.AbsoluteUri,
                    Title = _webView.CoreWebView2.DocumentTitle,
                    Snapshot = await TakeSnapshot(false)
                });
            }
            else
            {
                _hasCompletedFirstNavigation = true;
            }
        }

        #endregion

        #region Private

        private async Task<BitmapFrame> TakeSnapshot(bool active)
        {
            BitmapFrame? snapshot;
            using (var snapshotStream = new MemoryStream())
            {
                try
                {
                    await _webView.CoreWebView2.CapturePreviewAsync(CoreWebView2CapturePreviewImageFormat.Png, snapshotStream);
                    snapshot = BitmapFrame.Create(snapshotStream, BitmapCreateOptions.IgnoreImageCache, BitmapCacheOption.OnLoad);

                    //if (!active)
                    //{
                    //    var grayScaleSnapshot = new FormatConvertedBitmap();
                    //    grayScaleSnapshot.BeginInit();
                    //    grayScaleSnapshot.Source = snapshot;
                    //    grayScaleSnapshot.DestinationFormat = PixelFormats.Gray32Float;
                    //    grayScaleSnapshot.EndInit();
                    //    snapshot = BitmapFrame.Create(grayScaleSnapshot);
                    //}

                }
                catch (Exception ex)
                {
                    Debug.Assert(false, ex.Message);
                    var bitmap = new RenderTargetBitmap((int)SystemParameters.PrimaryScreenWidth,
                                                        (int)SystemParameters.PrimaryScreenHeight,
                                                        96,
                                                        96,
                                                        PixelFormats.Pbgra32);
                    var pngEncoder = new PngBitmapEncoder();
                    pngEncoder.Frames.Add(BitmapFrame.Create(bitmap));

                    using (var stream = new FileStream("blank_screen_resolution.png", FileMode.Create))
                    {
                        pngEncoder.Save(stream);
                    snapshot = BitmapFrame.Create(stream);
                    }
                }
            }

            return snapshot;
        }

        #endregion

        #region Public

        public void Dispose()
        {
            _webView.NavigationStarting -= _webView_NavigationStarting;
            _steps.Clear();
        }
        public async Task<List<JourneyEntry>> GetSteps()
        {
            var steps = _steps.ToList();

            steps.Add(new()
            {
                Title = _webView.CoreWebView2.DocumentTitle,
                Url = _webView.Source.AbsoluteUri,
                Snapshot = await TakeSnapshot(true),
                IsActive = true
            });
            return steps;
        }
        public async Task GoToStep(JourneyEntry step)
        {
            var index = _steps.IndexOf(step);

            await _webView.EnsureCoreWebView2Async();
            _webView.CoreWebView2.Navigate(step.Url);
        }

        #endregion

        #endregion
    }
}