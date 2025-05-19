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

        private const float AnimationTime = 0.5f;

        public static readonly DependencyProperty JourneyHighlightColorProperty = DependencyProperty.Register(nameof(JourneyHighlightColor), typeof(Color), typeof(JourneyWebView2));
        public static readonly DependencyProperty JourneyZoomFactorProperty = DependencyProperty.Register(nameof(JourneyZoomFactor), typeof(double), typeof(JourneyWebView2));

        #endregion

        #region Fields

        private bool _isDisposed;
        private bool _isMouseDown;
        private readonly JourneyManager _journeyManager;
        private Size _journeyStepSize;
        private Point _lastMouseDownPosition;
        private Point _lastMouseMovePosition;
        private JourneyStep? _selectedStep;

        #endregion

        #region Construction

        public JourneyWebView2()
        {
            InitializeComponent();
            DataContext = this;
            
            _journeyManager = new(WebView);
            
            RefreshJourneyStepSize();
            SystemEvents.DisplaySettingsChanged += SystemEvents_DisplaySettingsChanged;
            
            _lastMouseDownPosition = new(0, 0);
            _lastMouseMovePosition = new(0, 0);
            
            JourneyZoomFactor = 1;
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
            set => SetValue(JourneyHighlightColorProperty, value);
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

        private void JourneyCanvas_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            _isMouseDown = false;
        }
        private void JourneyCanvas_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (Keyboard.Modifiers == ModifierKeys.Control)
            {
                var currentZoom = JourneyZoomFactor;
                var zoomIncrement = currentZoom / 5;
                JourneyZoomFactor = e.Delta > 0 ? Math.Min(currentZoom + zoomIncrement, 2.1) : Math.Max(currentZoom - zoomIncrement, 0.4);
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
        private void SystemEvents_DisplaySettingsChanged(object? sender, EventArgs e)
        {
            RefreshJourneyStepSize();
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
            foreach (var child in JourneyCanvas.Children)
            {
                if (child is Line line)
                {
                    line.X1 += x;
                    line.Y1 += y;
                    line.X2 += x;
                    line.Y2 += y;
                }
                else if (child is UIElement element)
                {
                    Canvas.SetLeft(element, Canvas.GetLeft(element) + x);
                    Canvas.SetTop(element, Canvas.GetTop(element) + y);
                }
            }
        }
        private void RefreshJourneyStepSize()
        {
            var divisor = 6;
            var width = SystemParameters.PrimaryScreenWidth / divisor;
            var height = SystemParameters.PrimaryScreenHeight / divisor;
            _journeyStepSize = new(width, height);
        }

        #endregion

        #endregion






















        #region Event Handlers

        private void HideJourneyAnimation_Completed(object? sender, EventArgs e)
        {
            // Clear animations.
            _selectedStep.BeginAnimation(Canvas.LeftProperty, null);
            _selectedStep.BeginAnimation(Canvas.TopProperty, null);

            WebViewGrid.Visibility = Visibility.Visible;
            JourneyCanvas.Visibility = Visibility.Collapsed;

            JourneyCanvas.Children.Clear();
        }
        private void JourneyCanvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (_isMouseDown)
            {
                var currentMousePosition = e.GetPosition(JourneyCanvas);
                var deltaX = (currentMousePosition.X - _lastMouseMovePosition.X);
                var deltaY = (currentMousePosition.Y - _lastMouseMovePosition.Y);
                PanCanvas(deltaX, deltaY);
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
            var left = (JourneyCanvas.ActualWidth / 2) - (_journeyStepSize.Width / 2f);
            var top = ((JourneyCanvas.ActualHeight / 2) - (_journeyStepSize.Height / 2f));

            // Clear animations.
            _selectedStep.BeginAnimation(Canvas.LeftProperty, null);
            _selectedStep.BeginAnimation(Canvas.TopProperty, null);

            // Set the properties to the previously stored values.
            Canvas.SetLeft(_selectedStep, left);
            Canvas.SetTop(_selectedStep, top);

            _selectedStep.IsAnimating = false;
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
                var easingFunction = new CircleEase
                {
                    EasingMode = EasingMode.EaseInOut
                };

                var scaleXAnimation = new DoubleAnimation
                {
                    From = _journeyStepSize.Width,
                    To = JourneyCanvas.ActualWidth,
                    Duration = duration,
                    EasingFunction = easingFunction
                };
                var scaleYAnimation = new DoubleAnimation
                {
                    From = _journeyStepSize.Height,
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
                    From = 0.9,
                    To = -0.5,
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

                JourneyZoomFactor = 1;
                JourneyCanvas.RenderTransformOrigin = new Point(0, 0);
                JourneyCanvas.Visibility = Visibility.Visible;

                var centerX = (WebViewGrid.ActualWidth / 2) - (_journeyStepSize.Width / 2f);
                var centerY = (WebViewGrid.ActualHeight / 2) - (_journeyStepSize.Height / 2f);

                var journeySteps = await _journeyManager.GetJourney();

                JourneyCanvas.Children.Clear();
                var canvas = JourneyCanvas;
                var root = journeySteps;

                canvas.Children.Clear();

                var rootLayout = CalculateTreeLayout(root, siblingSpacing: 1.5, levelSpacing: 2.0);
                DrawTree(rootLayout, JourneyCanvas, nodeWidth: _journeyStepSize.Width, nodeHeight: _journeyStepSize.Height);

                if (_selectedStep != null)
                {
                    PanCanvas(centerX - Canvas.GetLeft(_selectedStep), centerY - Canvas.GetTop(_selectedStep));

                    Canvas.SetLeft(_selectedStep, 0);
                    Canvas.SetTop(_selectedStep, 0);
                    _selectedStep.Width = WebViewGrid.ActualWidth;
                    _selectedStep.Height = WebViewGrid.ActualHeight;
                    _selectedStep.TextArea.Opacity = 0;
                }






                var width = WebViewGrid.ActualWidth;
                var height = WebViewGrid.ActualHeight;
                var duration = TimeSpan.FromSeconds(AnimationTime);
                var snapshotEasingFunction = new CircleEase()
                {
                    EasingMode = EasingMode.EaseInOut
                };

                var scaleXAnimation = new DoubleAnimation
                {
                    From = width,
                    To = _journeyStepSize.Width,
                    Duration = duration,
                    EasingFunction = snapshotEasingFunction
                };
                var scaleYAnimation = new DoubleAnimation
                {
                    From = height,
                    To = _journeyStepSize.Height,
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
                    From = -0.5,
                    To = 0.9,
                    Duration = duration,
                    EasingFunction = snapshotEasingFunction
                };

                scaleXAnimation.Completed += ShowJourneyAnimation_Completed;

                var delay = 400;
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
    }
}