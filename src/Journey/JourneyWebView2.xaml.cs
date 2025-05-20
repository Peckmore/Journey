using Microsoft.Win32;
using NetEx.Collections;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace Journey
{
    public partial class JourneyWebView2 : IDisposable
    {
        #region Constants

        #region Private

        private const float AnimationTime = 0.5f;
        private const int SelectedStepZIndex = 1000;

        #endregion

        #region Internal

        public const double MaximumZoom = 3;
        public const double MinimumZoom = 0.25;

        #endregion

        #region Public Static

        public static readonly DependencyProperty JourneyHighlightColorProperty = DependencyProperty.Register(nameof(JourneyHighlightColor), typeof(Color), typeof(JourneyWebView2), new PropertyMetadata(Colors.DodgerBlue));
        public static readonly DependencyProperty JourneyZoomFactorProperty = DependencyProperty.Register(nameof(JourneyZoomFactor), typeof(double), typeof(JourneyWebView2), new PropertyMetadata(1d));

        #endregion

        #endregion

        #region Fields

        private bool _isDisposed;
        private bool _isMouseDown;
        private readonly JourneyManager _journeyManager;
        private Size _journeyStepSize;
        private Point _lastMouseDownPosition;
        private Point _lastMousePosition;
        private JourneyStep? _selectedStep;

        #endregion

        #region Construction

        public JourneyWebView2()
        {
            InitializeComponent();
            DataContext = this;
            
            _journeyManager = new(WebView);
            _lastMouseDownPosition = new(0, 0);
            _lastMousePosition = new(0, 0);

            RefreshJourneyStepSize();
            SystemEvents.DisplaySettingsChanged += SystemEvents_DisplaySettingsChanged;
        }

        #endregion

        #region Finalize

        ~JourneyWebView2()
        {
            Dispose(false);
        }

        #endregion

        #region Properties

        public bool IsJourneyVisible { get; private set; }
        public Color JourneyHighlightColor
        {
            get => (Color)GetValue(JourneyHighlightColorProperty);
            set
            {
                SetValue(JourneyHighlightColorProperty, value);
                var rd = new ResourceDictionary();
                rd.Source = new("pack://application:,,,/Journey;component/Themes/Generic.xaml", UriKind.RelativeOrAbsolute);
                rd[nameof(JourneyHighlightColor)] = new SolidColorBrush(value);
            }
        }
        public double JourneyZoomFactor
        {
            get => (double)GetValue(JourneyZoomFactorProperty);
            set => SetValue(JourneyZoomFactorProperty, value);
        }
        public Uri Source
        {
            get => WebView.Source;
            set => WebView.Source = value;
        }

        #endregion

        #region Methods

        #region Event Handlers

        private void CenterViewButton_Click(object sender, RoutedEventArgs e)
        {

        }
        private void ContainerGrid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            _isMouseDown = true;
            _lastMouseDownPosition = e.GetPosition(ContainerGrid);
            _lastMousePosition = _lastMouseDownPosition;
            e.Handled = true;
        }
        private void ContainerGrid_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (_isMouseDown)
            {
                var currentMousePosition = e.GetPosition(ContainerGrid);
                var deltaX = currentMousePosition.X - _lastMousePosition.X;
                var deltaY = currentMousePosition.Y - _lastMousePosition.Y;
                PanCanvas(deltaX, deltaY);
                _lastMousePosition = currentMousePosition;
                e.Handled = true;
            }
        }
        private void ContainerGrid_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            _isMouseDown = false;
        }
        private void ContainerGrid_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (Keyboard.Modifiers == ModifierKeys.Control)
            {
                // Get the current mouse position relative to the canvas
                var mousePosition = e.GetPosition(JourneyCanvas);

                ZoomCanvas(mousePosition, e.Delta < 0);
            }
            else
            {
                var scrollOffset = e.Delta < 0 ? -50 : 50;
                if (Keyboard.Modifiers == ModifierKeys.Shift)
                {
                    PanCanvas(scrollOffset, 0);
                }
                else
                {

                    PanCanvas(0, scrollOffset);
                }
            }

            e.Handled = true;
        }
        private void JourneyStep_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (sender is JourneyStep step
                && e.ClickCount == 1
                && e.GetPosition(ContainerGrid) == _lastMouseDownPosition)
            {
                _selectedStep = step;
                HideJourney();
                e.Handled = true;
            }
        }
        private void ResetZoomButton_Click(object sender, RoutedEventArgs e)
        {
            JourneyZoomFactor = 1;
        }
        private void SystemEvents_DisplaySettingsChanged(object? sender, EventArgs e)
        {
            RefreshJourneyStepSize();
        }
        private void ZoomInButton_Click(object sender, RoutedEventArgs e)
        {
            JourneyZoomFactor *= 1.1;
            //ZoomCanvas(new(((JourneyCanvas.ActualWidth / 2) / JourneyZoomFactor) - JourneyCanvasTranslateTransform.X,
            //           ((JourneyCanvas.ActualHeight / 2) / JourneyZoomFactor) - JourneyCanvasTranslateTransform.Y),
            //           false);
        }
        private void ZoomOutButton_Click(object sender, RoutedEventArgs e)
        {
            JourneyZoomFactor /= 1.1;
            //ZoomCanvas(new(((JourneyCanvas.ActualWidth / 2) / JourneyZoomFactor) - JourneyCanvasTranslateTransform.X,
            //           ((JourneyCanvas.ActualHeight / 2) / JourneyZoomFactor) - JourneyCanvasTranslateTransform.Y),
            //           true);
        }

        #endregion

        #region Private

        private void Dispose(bool disposing)
        {
            if (!_isDisposed)
            {
                if (disposing)
                {
                    JourneyCanvas.Children.Clear();
                    _journeyManager.Dispose();
                    WebView.Dispose();
                }

                // Free unmanaged resources (unmanaged objects) and override finalizer
                // Set large fields to null
                _isDisposed = true;
            }
        }
        private void PanCanvas(double x, double y)
        {
            JourneyCanvasTranslateTransform.X += x;
            JourneyCanvasTranslateTransform.Y += y;
        }
        private void RefreshJourneyStepSize()
        {
            var divisor = 6;
            var width = SystemParameters.PrimaryScreenWidth / divisor;
            var height = SystemParameters.PrimaryScreenHeight / divisor;
            _journeyStepSize = new(width, height);
        }
        private void ZoomCanvas(Point center, bool zoomOut)
        {
            Debug.WriteLine($"Point {center.ToString()}");

            // Update the ScaleTransform
            var absoluteX = center.X * JourneyZoomFactor + JourneyCanvasTranslateTransform.X;
            var absoluteY = center.Y * JourneyZoomFactor + JourneyCanvasTranslateTransform.Y;

            // Calculate the zoom factor
            var zoomIncrement = JourneyZoomFactor / 5;
            JourneyZoomFactor = zoomOut
                                ? Math.Max(JourneyZoomFactor - zoomIncrement, MinimumZoom)
                                : Math.Min(JourneyZoomFactor + zoomIncrement, MaximumZoom);

            // Adjust TranslateTransform to keep mouse position stable
            JourneyCanvasTranslateTransform.X = absoluteX - center.X * JourneyZoomFactor;
            JourneyCanvasTranslateTransform.Y = absoluteY - center.Y * JourneyZoomFactor;
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






















        #region Event Handlers

        private void HideJourneyAnimation_Completed(object? sender, EventArgs e)
        {
            WebView.Visibility = Visibility.Visible;
            JourneyContainer.Visibility = Visibility.Collapsed;

            JourneyCanvas.Children.Clear();
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
                Panel.SetZIndex(_selectedStep, SelectedStepZIndex + 1);

                IsJourneyVisible = false;

                var duration = TimeSpan.FromSeconds(AnimationTime);
                var easingFunction = new CircleEase
                {
                    EasingMode = EasingMode.EaseInOut
                };

                var scaleXAnimation = new DoubleAnimation
                {
                    From = _journeyStepSize.Width,
                    To = JourneyCanvas.ActualWidth / JourneyZoomFactor,
                    Duration = duration,
                    EasingFunction = easingFunction
                };
                var scaleYAnimation = new DoubleAnimation
                {
                    From = _journeyStepSize.Height,
                    To = JourneyCanvas.ActualHeight / JourneyZoomFactor,
                    Duration = duration,
                    EasingFunction = easingFunction
                };
                var translateXAnimation = new DoubleAnimation
                {
                    From = Canvas.GetLeft(_selectedStep),
                    To = -JourneyCanvasTranslateTransform.X / JourneyZoomFactor,
                    Duration = duration,
                    EasingFunction = easingFunction
                };
                var translateYAnimation = new DoubleAnimation
                {
                    From = Canvas.GetTop(_selectedStep),
                    To = -JourneyCanvasTranslateTransform.Y / JourneyZoomFactor,
                    Duration = duration,
                    EasingFunction = easingFunction
                };
                var titleAnimation = new DoubleAnimation
                {
                    From = 0.9,
                    To = -0.6,
                    Duration = duration,
                    EasingFunction = easingFunction
                };
                var controlsAnimation = new DoubleAnimation
                {
                    From = 0.9,
                    To = -0.6,
                    Duration = duration,
                    EasingFunction = easingFunction
                };

                scaleXAnimation.Completed += HideJourneyAnimation_Completed;

                _selectedStep.BeginAnimation(Control.WidthProperty, scaleXAnimation, HandoffBehavior.Compose);
                _selectedStep.BeginAnimation(Control.HeightProperty, scaleYAnimation, HandoffBehavior.Compose);
                _selectedStep.BeginAnimation(Canvas.LeftProperty, translateXAnimation, HandoffBehavior.Compose);
                _selectedStep.BeginAnimation(Canvas.TopProperty, translateYAnimation, HandoffBehavior.Compose);
                _selectedStep.TextArea.BeginAnimation(Control.OpacityProperty, titleAnimation, HandoffBehavior.Compose);
                JourneyControls.BeginAnimation(Control.OpacityProperty, controlsAnimation, HandoffBehavior.Compose);
            }

            return Task.CompletedTask;
        }

        private double NodeWidth => _journeyStepSize.Width;
        private double NodeHeight => _journeyStepSize.Height;
        private double HorizontalSpacing = 20;
        private double VerticalSpacing = 50;

        private void CalculateNodePositions<T>(TreeNode<T> node, double x, double y, Dictionary<TreeNode<T>, Point> positions)
        {
            positions[node] = new Point(x, y);

            double childX = x - (node.Children.Count - 1) * (NodeWidth + HorizontalSpacing) / 2;
            foreach (var child in node.Children)
            {
                CalculateNodePositions(child, childX, y + NodeHeight + VerticalSpacing, positions);
                childX += NodeWidth + HorizontalSpacing;
            }
        }
        private void DrawNodesAndConnections<T>(TreeNode<T> node, Dictionary<TreeNode<T>, Point> positions)
        {
            // Draw the current node
            var position = positions[node];
            var entry = node.Value;


            var rect = new JourneyStep
            {
                DataContext = entry,
                Width = _journeyStepSize.Width,
                Height = _journeyStepSize.Height
            };
            Canvas.SetLeft(rect, position.X);
            Canvas.SetTop(rect, position.Y);
            JourneyCanvas.Children.Add(rect);


            rect.MouseUp += JourneyStep_MouseUp;





            // Draw connections to children
            foreach (var child in node.Children)
            {
                var childPosition = positions[child];
                var line = new Line
                {
                    X1 = position.X + NodeWidth / 2,
                    Y1 = position.Y + NodeHeight,
                    X2 = childPosition.X + NodeWidth / 2,
                    Y2 = childPosition.Y,
                    Stroke = Brushes.Black,
                    StrokeThickness = 1
                };
                JourneyCanvas.Children.Add(line);

                // Recursively draw the child nodes
                DrawNodesAndConnections(child, positions);
            }






        }
        private TreeNodeLayout<JourneyEntry> CalculateTreeLayout(TreeNode<JourneyEntry> root, double siblingSpacing, double levelSpacing)
        {
            var rootLayout = BuildLayoutTree(root, null);
            FirstPass(rootLayout, siblingSpacing);
            SecondPass(rootLayout, 0, levelSpacing);
            return rootLayout;
        }

        private TreeNodeLayout<T> BuildLayoutTree<T>(TreeNode<T> node, TreeNodeLayout<T>? parent)
        {
            var layout = new TreeNodeLayout<T>(node) { Parent = parent };
            foreach (var child in node.Children)
            {
                layout.Children.Add(BuildLayoutTree(child, layout));
            }
            return layout;
        }

        private void FirstPass<T>(TreeNodeLayout<T> node, double siblingSpacing)
        {
            if (node.Children.Count == 0)
            {
                // Leaf node: No adjustment needed
                node.X = 0;
            }
            else
            {
                // Process children recursively
                foreach (var child in node.Children)
                {
                    FirstPass(child, siblingSpacing);
                }

                // Center the parent node above its children
                var leftmost = node.Children.First();
                var rightmost = node.Children.Last();
                var midpoint = (leftmost.X + rightmost.X) / 2;
                node.X = midpoint;

                // Resolve overlaps between siblings
                ResolveSiblingConflicts(node, siblingSpacing);
            }
        }

        private void ResolveSiblingConflicts<T>(TreeNodeLayout<T> node, double siblingSpacing)
        {
            for (int i = 1; i < node.Children.Count; i++)
            {
                var leftChild = node.Children[i - 1];
                var rightChild = node.Children[i];
                var gap = (leftChild.X + siblingSpacing) - rightChild.X;
                if (gap > 0)
                {
                    ShiftSubtree(rightChild, gap);
                }
            }
        }

        private void ShiftSubtree<T>(TreeNodeLayout<T> node, double shift)
        {
            node.X += shift;
            foreach (var child in node.Children)
            {
                ShiftSubtree(child, shift);
            }
        }

        private void SecondPass<T>(TreeNodeLayout<T> node, double modSum, double levelSpacing)
        {
            node.X += modSum;
            node.Y = (node.Parent?.Y ?? 0) + levelSpacing;

            foreach (var child in node.Children)
            {
                SecondPass(child, modSum + node.Mod, levelSpacing);
            }
        }

        private void DrawTree(TreeNodeLayout<JourneyEntry> rootLayout, Canvas canvas, double nodeWidth, double nodeHeight)
        {
            canvas.Children.Clear();

            // Draw nodes and connections recursively
            DrawNodeAndConnections(rootLayout, canvas, nodeWidth, nodeHeight);
        }

        private void DrawNodeAndConnections(TreeNodeLayout<JourneyEntry> node, Canvas canvas, double nodeWidth, double nodeHeight)
        {
            // Draw the node
            var rect = new JourneyStep
            {
                DataContext = node.Node.Value,
                Width = _journeyStepSize.Width,
                Height = _journeyStepSize.Height
            };
            rect.MouseUp += JourneyStep_MouseUp;
            Canvas.SetLeft(rect, node.X * nodeWidth);
            Canvas.SetTop(rect, node.Y * nodeHeight);
            canvas.Children.Add(rect);

            if (node.Node.Value.IsActive)
            {
                _selectedStep = rect;
            }

            // Draw connections to children
            foreach (var child in node.Children)
            {
                var line = new Line
                {
                    X1 = node.X * nodeWidth + nodeWidth / 2,
                    Y1 = node.Y * nodeHeight + nodeHeight,
                    X2 = child.X * nodeWidth + nodeWidth / 2,
                    Y2 = child.Y * nodeHeight,
                    Stroke = Brushes.Black,
                    StrokeThickness = 1
                };
                canvas.Children.Add(line);

                // Recursively draw the child nodes
                DrawNodeAndConnections(child, canvas, nodeWidth, nodeHeight);
            }
        }


        public async Task ShowJourney()
        {
            if (!IsJourneyVisible)
            {
                IsJourneyVisible = true;

                var stopWatch = new Stopwatch();
                stopWatch.Start();
                var delay = 400;

                JourneyZoomFactor = 1;
                JourneyCanvasTranslateTransform.X = 0;
                JourneyCanvasTranslateTransform.Y = 0;
                JourneyCanvas.Children.Clear();
                JourneyContainer.Visibility = Visibility.Visible;

                var journeySteps = await _journeyManager.GetJourney();
                var rootLayout = CalculateTreeLayout(journeySteps, siblingSpacing: 1.25, levelSpacing: 1.25);
                DrawTree(rootLayout, JourneyCanvas, nodeWidth: _journeyStepSize.Width, nodeHeight: _journeyStepSize.Height);
                
                var duration = TimeSpan.FromSeconds(AnimationTime);
                var animationEasingFunction = new CircleEase { EasingMode = EasingMode.EaseInOut };
                
                var controlsAnimation = new DoubleAnimation
                {
                    From = -0.6,
                    To = 0.9,
                    Duration = duration,
                    EasingFunction = animationEasingFunction
                };

                if (_selectedStep != null)
                {
                    PanCanvas((WebView.ActualWidth / 2) - (_journeyStepSize.Width / 2f) - Canvas.GetLeft(_selectedStep),
                              (WebView.ActualHeight / 2) - (_journeyStepSize.Height / 2f) - Canvas.GetTop(_selectedStep));

                    Panel.SetZIndex(_selectedStep, SelectedStepZIndex);

                    var scaleXAnimation = new DoubleAnimation
                    {
                        From = WebView.ActualWidth,
                        To = _journeyStepSize.Width,
                        Duration = duration,
                        EasingFunction = animationEasingFunction
                    };
                    var scaleYAnimation = new DoubleAnimation
                    {
                        From = WebView.ActualHeight,
                        To = _journeyStepSize.Height,
                        Duration = duration,
                        EasingFunction = animationEasingFunction
                    };
                    var translateXAnimation = new DoubleAnimation
                    {
                        From = 0 - JourneyCanvasTranslateTransform.X,
                        To = Canvas.GetLeft(_selectedStep),
                        Duration = duration,
                        EasingFunction = animationEasingFunction
                    };
                    var translateYAnimation = new DoubleAnimation
                    {
                        From = 0 - JourneyCanvasTranslateTransform.Y,
                        To = Canvas.GetTop(_selectedStep),
                        Duration = duration,
                        EasingFunction = animationEasingFunction
                    };
                    var titleAnimation = new DoubleAnimation
                    {
                        From = -0.6,
                        To = 0.9,
                        Duration = duration,
                        EasingFunction = animationEasingFunction
                    };

                    Canvas.SetLeft(_selectedStep, 0 - JourneyCanvasTranslateTransform.X);
                    Canvas.SetTop(_selectedStep, 0 - JourneyCanvasTranslateTransform.Y);
                    _selectedStep.Width = WebView.ActualWidth;
                    _selectedStep.Height = WebView.ActualHeight;
                    _selectedStep.TextArea.Opacity = 0;

                    stopWatch.Stop();
                    if (stopWatch.ElapsedMilliseconds < delay)
                    {
                        // HACK: Feels nasty to put a delay in here, but it prevents the flicker when showing the image control, which seems to
                        // appear, then paint the image in, causing a flicker when switching between the browser and the snapshot. Adding a small
                        // delay here seems to allow the image time to paint before the browser is hidden, removing the flicker. But is there
                        // a better way?
                        await Task.Delay(delay - (int)stopWatch.ElapsedMilliseconds);
                    }

                    _selectedStep.IsAnimating = true;
                    scaleXAnimation.Completed += (_, _) => { _selectedStep.IsAnimating = false; };
                    _selectedStep.BeginAnimation(Control.WidthProperty, scaleXAnimation, HandoffBehavior.Compose);
                    _selectedStep.BeginAnimation(Control.HeightProperty, scaleYAnimation, HandoffBehavior.Compose);
                    _selectedStep.BeginAnimation(Canvas.LeftProperty, translateXAnimation, HandoffBehavior.Compose);
                    _selectedStep.BeginAnimation(Canvas.TopProperty, translateYAnimation, HandoffBehavior.Compose);
                    _selectedStep.TextArea.BeginAnimation(Control.OpacityProperty, titleAnimation, HandoffBehavior.Compose);
                }
                else
                {
                    stopWatch.Stop();
                }

                JourneyControls.BeginAnimation(Control.OpacityProperty, controlsAnimation, HandoffBehavior.Compose);
                WebView.Visibility = Visibility.Collapsed;

                Debug.WriteLine($"Delayed for: {delay - (int)stopWatch.ElapsedMilliseconds}ms");
            }
        }

        #endregion
    }
}