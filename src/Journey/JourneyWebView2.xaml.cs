using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace Journey
{
    public partial class JourneyView : IDisposable, INotifyPropertyChanged
    {
        #region Constants

        private const float AnimationTime = 0.35f;

        #endregion

        #region Fields

        private bool _isDisposed;
        private bool _isMouseDown;
        private readonly List<UIElement> _journeyElements;
        private readonly JourneyManager _journeyManager;
        private double _journeyStepHeight;
        private double _journeyStepSpacing;
        private double _journeyStepWidth;
        private Point _lastMouseDownPosition;
        private Point _lastMouseMovePosition;
        private JourneyStep? _selectedStep;
        private double _zoomLevel;
        private double zoomLevel;

        #endregion

        #region Events

        public event PropertyChangedEventHandler? PropertyChanged;

        #endregion

        #region Construction

        public JourneyView()
        {
            InitializeComponent();
            DataContext = this;

            WebView.EnsureCoreWebView2Async(null);

            _journeyManager = new(WebView);
            _journeyElements = new();
            _journeyStepHeight = SystemParameters.PrimaryScreenHeight / 5;
            _journeyStepWidth = SystemParameters.PrimaryScreenWidth / 5;
            _journeyStepSpacing = _journeyStepHeight / 2;
            _lastMouseDownPosition = new(0, 0);
            _lastMouseMovePosition = new(0, 0);
            
            ZoomLevel = 1;
        }

        #endregion

        #region Finalize

        ~JourneyView()
        {
            Dispose(false);
        }

        #endregion

        #region Properties

        public bool IsJourneyVisible { get; set; }
        public Uri Source
        {
            get => WebView.Source;
            set => WebView.Source = value;
        }
        public double ZoomLevel
        {
            get => _zoomLevel;
            set
            {
                if (_zoomLevel != value)
                {
                    _zoomLevel = value;
                    OnPropertyChanged(nameof(ZoomLevel));
                }
            }
        }

        #endregion

        #region Methods

        private void IncrementScroll(double deltaX, double deltaY)
        {
            foreach (var element in _journeyElements)
            {
                Canvas.SetLeft(element, Canvas.GetLeft(element) + deltaX);
                Canvas.SetTop(element, Canvas.GetTop(element) + deltaY);
            }
        }

        #region Event Handlers

        private void HideJourneyAnimation_Completed(object? sender, EventArgs e)
        {
            // Clear animations.
            _selectedStep.BeginAnimation(Canvas.LeftProperty, null);
            _selectedStep.BeginAnimation(Canvas.TopProperty, null);

            JourneyCanvas.Visibility = Visibility.Collapsed;
            WebViewGrid.Visibility = Visibility.Visible;

            _journeyElements.Clear();
            JourneyCanvas.Children.Clear();
        }
        private void JourneyCanvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (_isMouseDown)
            {
                var currentMousePosition = e.GetPosition(JourneyCanvas);
                var deltaX = (currentMousePosition.X - _lastMouseMovePosition.X);
                var deltaY = (currentMousePosition.Y - _lastMouseMovePosition.Y);
                IncrementScroll(deltaX, deltaY);
                _lastMouseMovePosition = currentMousePosition;
                e.Handled = true;
            }
        }
        private void JourneyCanvas_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            _isMouseDown = true;
            _lastMouseDownPosition = e.GetPosition(JourneyCanvas);
            _lastMouseMovePosition = _lastMouseDownPosition;
            e.Handled = true;
        }
        private void JourneyCanvas_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            _isMouseDown = false;
        }
        private void JourneyCanvas_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (Keyboard.Modifiers == ModifierKeys.Control)
            {
                if (JourneyCanvas.LayoutTransform is ScaleTransform matTrans)
                {
                    var mousePos = e.GetPosition(JourneyCanvas);
                    var zoomIncrement = _zoomLevel / 5;
                    ZoomLevel = e.Delta > 0 ? Math.Min(ZoomLevel + zoomIncrement, 2.1) : Math.Max(ZoomLevel - zoomIncrement, 0.4);
                    e.Handled = true;
                }
            }
            else
            {
                var scrollOffset = e.Delta < 0 ? -50 : 50;
                if (Keyboard.Modifiers == ModifierKeys.Shift)
                {
                    IncrementScroll(scrollOffset, 0);
                }
                else
                {

                    IncrementScroll(0, scrollOffset);
                }
                e.Handled = true;
            }
        }
        private void JourneyStep_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 1 && e.GetPosition(JourneyCanvas) == _lastMouseDownPosition)
            {
                if (sender is JourneyStep step)
                {
                    _selectedStep = step;
                    HideJourney();
                    e.Handled = true;
                }
            }
        }
        private void ShowJourneyAnimation_Completed(object? sender, EventArgs e)
        {
            // HACK: On slow performing machines the animation finishes in the incorrect position, so we'll
            // force them into the correct position to make sure that everything is where it should be
            // if this happens. However, this is a workaround, as the performance shouldn't be that slow,
            // and seems to be an issue wuth certain machines?
            var left = (JourneyCanvas.ActualWidth / 2) - (_journeyStepWidth / 2f);
            var top = ((JourneyCanvas.ActualHeight / 2) - (_journeyStepHeight / 2f));

            // Clear animations.
            _selectedStep.BeginAnimation(Canvas.LeftProperty, null);
            _selectedStep.BeginAnimation(Canvas.TopProperty, null);

            // Set the properties to the previously stored values.
            Canvas.SetLeft(_selectedStep, left);
            Canvas.SetTop(_selectedStep, top);

            _selectedStep.IsAnimating = false;
        }

        #endregion

        #region Private
        
        private void Dispose(bool disposing)
        {
            if (!_isDisposed)
            {
                if (disposing)
                {
                    _journeyManager.Dispose();
                    WebView.Dispose();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                _isDisposed = true;
            }
        }
        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion

        #region Protected

        protected override void OnLostFocus(RoutedEventArgs e)
        {
            _isMouseDown = false;

            base.OnLostFocus(e);
        }

        #endregion

        #region Public

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        public Task HideJourney()
        {
            if (IsJourneyVisible && _selectedStep != null)
            {
                _journeyManager.GoToStep(_selectedStep.DataContext as JourneyEntry);

                _selectedStep.IsAnimating = true;

                IsJourneyVisible = false;

                var duration = TimeSpan.FromSeconds(AnimationTime);
                var easingFunction = new CubicEase
                {
                    EasingMode = EasingMode.EaseInOut
                };

                var scaleXAnimation = new DoubleAnimation
                {
                    From = _journeyStepWidth,
                    To = JourneyCanvas.ActualWidth,
                    Duration = duration,
                    EasingFunction = easingFunction
                };
                var scaleYAnimation = new DoubleAnimation
                {
                    From = _journeyStepHeight,
                    To = JourneyCanvas.ActualHeight,
                    Duration = duration,
                    EasingFunction = easingFunction
                };
                var translateXAnimation = new DoubleAnimation
                {
                    From = Canvas.GetLeft(_selectedStep),
                    To = 0,
                    Duration = duration,
                    EasingFunction = easingFunction
                };
                var translateYAnimation = new DoubleAnimation
                {
                    From = Canvas.GetTop(_selectedStep),
                    To = 0,
                    Duration = duration,
                    EasingFunction = easingFunction
                };
                var titleAnimation = new DoubleAnimation
                {
                    From = 0.8,
                    To = 0,
                    Duration = duration,
                    EasingFunction = easingFunction
                };
                
                scaleXAnimation.Completed += HideJourneyAnimation_Completed;

                Panel.SetZIndex(_selectedStep, 2);
                _selectedStep.BeginAnimation(Control.WidthProperty, scaleXAnimation, HandoffBehavior.Compose);
                _selectedStep.BeginAnimation(Control.HeightProperty, scaleYAnimation, HandoffBehavior.Compose);
                _selectedStep.BeginAnimation(Canvas.LeftProperty, translateXAnimation, HandoffBehavior.Compose);
                _selectedStep.BeginAnimation(Canvas.TopProperty, translateYAnimation, HandoffBehavior.Compose);
                _selectedStep.TextArea.BeginAnimation(Control.OpacityProperty, titleAnimation, HandoffBehavior.Compose);
            }

            return Task.CompletedTask;
        }
        public async Task ShowJourney()
        {
            if (!IsJourneyVisible)
            {
                IsJourneyVisible = true;

                var stopWatch = new Stopwatch();
                stopWatch.Start();

                ZoomLevel = 1;
                JourneyCanvas.RenderTransformOrigin = new Point(0, 0);
                JourneyCanvas.Visibility = Visibility.Visible;

                var centerX = (WebViewGrid.ActualWidth / 2) - (_journeyStepWidth / 2f);
                var centerY = (WebViewGrid.ActualHeight / 2) - (_journeyStepHeight / 2f);
                var topOffset = 0d;
                var activeStepOffset = 0d;
                foreach (var entry in await _journeyManager.GetSteps())
                {
                    var step = new JourneyStep
                    {
                        DataContext = entry,
                        Width = _journeyStepWidth,
                        Height = _journeyStepHeight
                    };

                    JourneyCanvas.Children.Add(step);
                    _journeyElements.Add(step);

                    if (entry.IsActive)
                    {
                        _selectedStep = step;
                        activeStepOffset = centerY - topOffset;

                        foreach (var existingStep in _journeyElements)
                        {
                            var existingStepTop = Canvas.GetTop(existingStep);
                            Canvas.SetTop(existingStep, existingStepTop + activeStepOffset);
                        }

                        topOffset = centerY + _journeyStepHeight + _journeyStepSpacing;

                        Canvas.SetLeft(step, 0);
                        Canvas.SetTop(step, 0);
                        step.Width = WebViewGrid.ActualWidth;
                        step.Height = WebViewGrid.ActualHeight;
                        step.TextArea.Opacity = 0;
                    }
                    else
                    {
                        Canvas.SetLeft(step, centerX);
                        Canvas.SetTop(step, topOffset);

                        topOffset += _journeyStepHeight + _journeyStepSpacing;
                    }

                    step.MouseUp += JourneyStep_MouseUp;
                }

                var width = WebViewGrid.ActualWidth;
                var height = WebViewGrid.ActualHeight;
                var duration = TimeSpan.FromSeconds(AnimationTime);
                var snapshotEasingFunction = new CubicEase()
                {
                    EasingMode = EasingMode.EaseInOut
                };

                var scaleXAnimation = new DoubleAnimation
                {
                    From = width,
                    To = _journeyStepWidth,
                    Duration = duration,
                    EasingFunction = snapshotEasingFunction
                };
                var scaleYAnimation = new DoubleAnimation
                {
                    From = height,
                    To = _journeyStepHeight,
                    Duration = duration,
                    EasingFunction = snapshotEasingFunction
                };
                var translateXAnimation = new DoubleAnimation
                {
                    From = 0,
                    To = centerX,
                    Duration = duration,
                    EasingFunction = snapshotEasingFunction
                };
                var translateYAnimation = new DoubleAnimation
                {
                    From = 0,
                    To = centerY,
                    Duration = duration,
                    EasingFunction = snapshotEasingFunction
                };
                var titleAnimation = new DoubleAnimation
                {
                    From = 0,
                    To = 0.8,
                    Duration = duration,
                    EasingFunction = snapshotEasingFunction
                };

                scaleXAnimation.Completed += ShowJourneyAnimation_Completed;

                var delay = 500;
                stopWatch.Stop();
                if (stopWatch.ElapsedMilliseconds < delay)
                {
                    // HACK: Feels nasty to put a delay in here, but it prevents the flicker when showing the image control, which seems to
                    // appear, then paint the image in, causing a flicker when switching between the browser and the snapshot. Adding a small
                    // delay here seems to allow the image time to paint before the browser is hidden, removing the flicker. But is there
                    // a better way?
                    await Task.Delay(delay - (int)stopWatch.ElapsedMilliseconds);
                }

                WebViewGrid.Visibility = Visibility.Collapsed;

                if (_selectedStep != null)
                {
                    _selectedStep.IsAnimating = true;
                    _selectedStep.BeginAnimation(Control.WidthProperty, scaleXAnimation, HandoffBehavior.Compose);
                    _selectedStep.BeginAnimation(Control.HeightProperty, scaleYAnimation, HandoffBehavior.Compose);
                    _selectedStep.BeginAnimation(Canvas.LeftProperty, translateXAnimation, HandoffBehavior.Compose);
                    _selectedStep.BeginAnimation(Canvas.TopProperty, translateYAnimation, HandoffBehavior.Compose);
                    _selectedStep.TextArea.BeginAnimation(Control.OpacityProperty, titleAnimation, HandoffBehavior.Compose);
                }

                Debug.WriteLine($"Delayed for: {delay - (int)stopWatch.ElapsedMilliseconds}ms");
            }
        }

        public async Task ToggleJourney()
        {
            if (IsJourneyVisible)
            {
                await HideJourney();
            }
            else
            {
                await ShowJourney();
            }
        }

        #endregion

        #endregion
    }
}