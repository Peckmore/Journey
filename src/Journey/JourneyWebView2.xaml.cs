using Journey.Tree;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.Wpf;
using Microsoft.Win32;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace Journey
{
    /// <summary>
    /// An implementation of a WebView2 control that provides a Journey view, allowing users to visualize their navigation history as a tree diagram.
    /// </summary>
    public sealed partial class JourneyWebView2 : IWebView2, INotifyPropertyChanged, IDisposable
    {
        #region Constants

        #region Private

        private const int ActivePathLineZIndex = 10;
        private const float AnimationTime = 0.5f;
        private const double ButtonBarOpacity = 0.925d;
        private const double InactiveOpacity = 0.7f;
        private const double MaximumZoom = 5;
        private const double MinimumZoom = 0.1;
        private const int SelectedStepBorderZIndex = 20;
        private const int SelectedStepZIndex = 1000;
        private const int StepZIndex = 30;

        #endregion

        #region Public Static

        /// <summary>
        /// /// Command to hide the Journey view.
        /// </summary>
        public static readonly ICommand HideJourneyCommand = new RoutedCommand();
        /// <summary>
        /// Property to control the zoom factor of the Journey view.
        /// </summary>
        public static readonly DependencyProperty JourneyZoomFactorProperty = DependencyProperty.Register(nameof(JourneyZoomFactor), typeof(double), typeof(JourneyWebView2), new PropertyMetadata(1d));
        /// <summary>
        /// Command to reset the Journey view to the currently selected step, with the zoom factor set to 1 and the canvas panned to the home position.
        /// </summary>
        public static readonly ICommand ResetJourneyViewCommand = new RoutedCommand();
        /// <summary>
        /// Command to reset the zoom factor of the Journey view to 1.
        /// </summary>
        public static readonly ICommand ResetJourneyZoomCommand = new RoutedCommand();
        /// <summary>
        /// Command to zoom in on the Journey view.
        /// </summary>
        public static readonly ICommand ZoomInJourneyCommand = new RoutedCommand();
        /// <summary>
        /// Command to zoom out on the Journey view.
        /// </summary>
        public static readonly ICommand ZoomOutJourneyCommand = new RoutedCommand();

        #endregion

        #endregion

        #region Fields

        private Point _canvasHome;
        private bool _isDisposed;
        private bool _isJourneyVisible;
        private bool _isMouseDown;
        private readonly JourneyManager _journeyManager;
        private readonly SemaphoreSlim _journeySemaphore;
        private Size _journeyStepSize;
        private Size _journeyStepSpacing;
        private Point _lastMouseDownPosition;
        private Point _lastMousePosition;
        private JourneyStep? _selectedStep;

        #endregion

        #region Events

        /// <inheritdoc />
        public event EventHandler<CoreWebView2ContentLoadingEventArgs>? ContentLoading;
        /// <inheritdoc />
        public event EventHandler<CoreWebView2InitializationCompletedEventArgs>? CoreWebView2InitializationCompleted;
        /// <inheritdoc />
        public event EventHandler<CoreWebView2NavigationCompletedEventArgs>? NavigationCompleted;
        /// <inheritdoc />
        public event EventHandler<CoreWebView2NavigationStartingEventArgs>? NavigationStarting;
        /// <inheritdoc />
        public event PropertyChangedEventHandler? PropertyChanged;
        /// <inheritdoc />
        public event EventHandler<CoreWebView2SourceChangedEventArgs>? SourceChanged;
        /// <inheritdoc />
        public event EventHandler<CoreWebView2WebMessageReceivedEventArgs>? WebMessageReceived;
        /// <inheritdoc />
        public event EventHandler<EventArgs>? ZoomFactorChanged;

        #endregion

        #region Construction

        /// <summary>
        /// Creates a new instance of a <see cref="JourneyWebView2" /> control.
        /// Note that the control's <see cref="CoreWebView2" /> will be null until initialized.
        /// See the <see cref="WebView2" /> class documentation for an initialization overview.
        /// </summary>
        public JourneyWebView2()
        {
            // Initialize our control, and set the DataContext to itself, to bind to code behind.
            InitializeComponent();
            DataContext = this;

            // Merge in the appropriate resource dictionary dependent upon whether the OS is in light or dark mode.
            ApplyTheme();

            // Initialize our fields.
            _canvasHome = new(0, 0);
            _journeyManager = new(WebView);
            _journeySemaphore = new(1, 1);
            _lastMouseDownPosition = new(0, 0);
            _lastMousePosition = new(0, 0);

            // Wire up our WebView2 events to "pass-through".
            WebView.ContentLoading += (_, args) => { ContentLoading?.Invoke(this, args); };
            WebView.CoreWebView2InitializationCompleted += (_, args) => { CoreWebView2InitializationCompleted?.Invoke(this, args); };
            WebView.NavigationCompleted += (_, args) => { NavigationCompleted?.Invoke(this, args); };
            WebView.NavigationStarting += (_, args) => { NavigationStarting?.Invoke(this, args); };
            WebView.SourceChanged += (_, args) => { SourceChanged?.Invoke(this, args); };
            WebView.WebMessageReceived += (_, args) => { WebMessageReceived?.Invoke(this, args); };
            WebView.ZoomFactorChanged += (_, args) => { ZoomFactorChanged?.Invoke(this, args); };

            // The size of Journey "steps" is calculated as a division of the primary monitor resolution. We're keeping this simple for
            // now and just covering the scenarios of either a single monitor, or multiple monitors of the same resolution. Monitors with
            // different resolutions are a different case that would need to be accounted for in future development.
            RefreshJourneyStepSize();

            // We'll also hook into the event for resolution changes, so that we can recalculate our step size should the primary monitor
            // resolution change, and user preference changes, to update the current theme (light/dark mode) if the user changes the mode.
            SystemEvents.DisplaySettingsChanged += SystemEvents_DisplaySettingsChanged;
            SystemEvents.UserPreferenceChanged += SystemEvents_UserPreferenceChanged;
        }

        #endregion

        #region Finalize

        /// <summary>
        /// Finalizes an instance of the <see cref="JourneyWebView2"/> class.
        /// </summary>
        ~JourneyWebView2()
        {
            Dispose(false);
        }

        #endregion

        #region Properties

        /// <inheritdoc />
        public bool AllowExternalDrop
        {
            get => WebView.AllowExternalDrop;
            set => WebView.AllowExternalDrop = value;
        }
        /// <inheritdoc />
        public bool CanGoBack => WebView.CanGoBack;
        /// <inheritdoc />
        public bool CanGoForward => WebView.CanGoForward;
        /// <inheritdoc />
        public CoreWebView2 CoreWebView2 => WebView.CoreWebView2;
        /// <inheritdoc />
        public CoreWebView2CreationProperties CreationProperties
        {
            get => WebView.CreationProperties;
            set => WebView.CreationProperties = value;
        }
        /// <inheritdoc />
        public System.Drawing.Color DefaultBackgroundColor
        {
            get => WebView.DefaultBackgroundColor;
            set => WebView.DefaultBackgroundColor = value;
        }
        /// <inheritdoc />
        public System.Drawing.Color DesignModeForegroundColor
        {
            get => WebView.DesignModeForegroundColor;
            set => WebView.DesignModeForegroundColor = value;
        }
        /// <summary>
        /// Indicates whether Journey is currently being displayed for the current <see cref="JourneyWebView2"/> instance.
        /// </summary>
        /// <returns><see langword="true"/> if Journey is currently being displayed; otherwise <see langword="false"/>.</returns>
        public bool IsJourneyVisible
        {
            get => _isJourneyVisible;
            private set
            {
                if (_isJourneyVisible != value)
                {
                    _isJourneyVisible = value;
                    NotifyPropertyChanged();
                }
            }
        }
        /// <summary>
        /// The "highlight" color for the active step (the users current page) in the Journey view.
        /// </summary>
        public Brush JourneyActiveStepBackground
        {
            get => (Brush)Resources["JourneyActiveStepBackground"];
            set => Resources["JourneyActiveStepBackground"] = value;
        }
        /// <summary>
        /// The foreground color for the active step (the users current page) in the Journey view.
        /// </summary>
        public Brush JourneyActiveStepForeground
        {
            get => (Brush)Resources["JourneyActiveStepForeground"];
            set => Resources["JourneyActiveStepForeground"] = value;
        }
        /// <summary>
        /// The default background color for the Journey view.
        /// </summary>
        public Brush JourneyBackground
        {
            get => (Brush)Resources["JourneyBackground"];
            set => Resources["JourneyBackground"] = value;
        }
        /// <summary>
        /// The "highlight" color, used for mouseover effects in the Journey view.
        /// </summary>
        public Brush JourneyHighlightBackground
        {
            get => (Brush)Resources["JourneyHighlightBackground"];
            set => Resources["JourneyHighlightBackground"] = value;
        }
        /// <summary>
        /// The "highlight" foreground color, used for text displayed on top of highlighted elements in the Journey view.
        /// </summary>
        public Brush JourneyHighlightForeground
        {
            get => (Brush)Resources["JourneyHighlightForeground"];
            set => Resources["JourneyHighlightForeground"] = value;
        }
        /// <summary>
        /// The zoom factor for Journey.
        /// </summary>
        public double JourneyZoomFactor
        {
            get => (double)GetValue(JourneyZoomFactorProperty);
            set => SetValue(JourneyZoomFactorProperty, value);
        }
        /// <summary>
        /// Sets the overall color scheme of the WebView2 and Journey controls.
        /// </summary>
        public CoreWebView2PreferredColorScheme PreferredColorScheme
        {
            get => CoreWebView2?.Profile.PreferredColorScheme ?? CoreWebView2PreferredColorScheme.Light;
            set
            {
                if (CoreWebView2 != null)
                {
                    CoreWebView2.Profile.PreferredColorScheme = value;
                }
                ApplyTheme();
            }
        }
        /// <inheritdoc />
        public Uri Source
        {
            get => WebView.Source;
            set => WebView.Source = value;
        }
        /// <inheritdoc />
        public double ZoomFactor
        {
            get => WebView.ZoomFactor;
            set => WebView.ZoomFactor = value;
        }

        #endregion

        #region Methods

        #region Event Handlers

        private async void JourneyStep_MouseUp(object sender, MouseButtonEventArgs e)
        {
            // If we've received a single click event for a Journey step, check whether the mouse has moved between mouse down and
            // mouse up events (to make sure this was a click, and not a drag) and, if we have a valid click, set our selected step
            // to the step that has been clicked on, then begin to hide Journey.

            if (sender is JourneyStep step
                && e.ClickCount == 1
                && e.GetPosition(RootGrid) == _lastMouseDownPosition)
            {
                // Set our selected step to the one the user clicked on.
                _selectedStep = step;

                // Start our animation to hide Journey.
                await HideJourney();

                // Set this event to handled, there is no need to bubble it up.
                e.Handled = true;
            }
        }
        private void RootGrid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            // We only handle left clicks for now.
            if (e.ChangedButton == MouseButton.Left)
            {
                // Set our flag to indicate that the left mouse button is now down.
                _isMouseDown = true;

                // Store the position of the mouse cursor at the point the left mouse button was pressed. We'll use this when the button
                // is released to determine whether we have a click or not.
                _lastMouseDownPosition = e.GetPosition(RootGrid);

                // Set our mouse position variable to the position of the cursor also, as this is the starting point for any drag interaction
                // which will pan our Journey canvas.
                _lastMousePosition = _lastMouseDownPosition;

                // Set this event to handled, there is no need to bubble it up.
                e.Handled = true;
            }
        }
        private void RootGrid_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            // We only process mouse movements if the left mouse button is being held down.
            if (_isMouseDown)
            {
                // Get the current mouse cursor position.
                var currentMousePosition = e.GetPosition(RootGrid);

                // Calculate the delta between the current mouse cursor position and our last recorded position.
                var deltaX = currentMousePosition.X - _lastMousePosition.X;
                var deltaY = currentMousePosition.Y - _lastMousePosition.Y;

                // Call our `PanCanvas` method to pan the canvas by the delta.
                PanCanvas(deltaX, deltaY);

                // Update our last recorded mouse cursor position to the current position, ready for the next delta comparison.
                _lastMousePosition = currentMousePosition;

                // Set this event to handled, there is no need to tunnel it down/bubble it up.
                e.Handled = true;
            }
        }
        private void RootGrid_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            // We only handle left clicks for now.
            if (e.ChangedButton == MouseButton.Left)
            {
                // Set our flag to indicate that the left mouse button is now released.
                _isMouseDown = false;
            }
        }
        private void RootGrid_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            // The mouse wheel will either scroll or zoom the Journey canvas, depending on modifer keys.

            // Check whether `CTRL` is being held - if so, we'll zoom.
            if (Keyboard.Modifiers == ModifierKeys.Control)
            {
                // Get the current mouse position relative to the canvas
                var mousePosition = e.GetPosition(JourneyCanvas);

                // Zoom our canvas, centered on the current mouse position. We check the value of e.Delta to determine which way the
                // user scrolled the mouse wheel.
                ZoomCanvas(mousePosition, e.Delta < 0);
            }
            else
            {
                // `CTRL` was not held down, so we're going to pan the canvas.

                // Regardless of direction, our pan amount will be the same, so check the value of e.Delta to determine which way the
                // user scrolled the mouse wheel, and set our pan amount accordingly.
                var scrollOffset = e.Delta < 0 ? -50 : 50;

                if (Keyboard.Modifiers == ModifierKeys.Shift)
                {
                    // If `Shift` is being held, scroll horizontally...
                    PanCanvas(scrollOffset, 0);
                }
                else
                {
                    // ...otherwise scroll vertically.
                    PanCanvas(0, scrollOffset);
                }
            }

            // Set this event to handled, there is no need to tunnel it down/bubble it up.
            e.Handled = true;
        }
        private void SystemEvents_DisplaySettingsChanged(object? sender, EventArgs e)
        {
            // The system display settings have changed, so recalculate our Journey step size, in-case primary monitor resolution has
            // changed.
            RefreshJourneyStepSize();
        }
        private void SystemEvents_UserPreferenceChanged(object sender, UserPreferenceChangedEventArgs e)
        {
            if (e.Category == UserPreferenceCategory.General)
            {
                // The user may have changed their theme preferences, so re-apply the theme (light/dark mode).
                ApplyTheme();
            }
        }

        #endregion

        #region Private

        private void ApplyTheme()
        {
            // Set a flag to indicate whether we should use dark mode.
            var isDark = false;
            if (CoreWebView2?.Profile.PreferredColorScheme == CoreWebView2PreferredColorScheme.Dark)
            {
                // If the WebView2 profile is set to dark mode, we'll use that.
                isDark = true;
            }
            else if (CoreWebView2?.Profile.PreferredColorScheme == CoreWebView2PreferredColorScheme.Auto)
            {
                // If the WebView2 profile is set to auto, we'll check the registry to see whether the app should be in light or dark mode.
                using (var themeRegistryKey = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize"))
                {
                    isDark = (themeRegistryKey?.GetValue("AppsUseLightTheme") as int? ?? 1) == 0;
                }
            }

            // Remove any previously merged dictionary and merge in the appropriate dictionary based on the current light/dark mode.
            var themeDictionary = new ResourceDictionary
            {
                Source = new Uri(isDark ? "pack://application:,,,/Journey;component/Resources/Themes/Theme.Dark.xaml"
                                        : "pack://application:,,,/Journey;component/Resources/Themes/Theme.Light.xaml", UriKind.Absolute)
            };

            var dictionariesToRemove = Resources.MergedDictionaries
                                       .Where(d => d.Source != null && (d.Source.OriginalString.Contains("Theme.Dark.xaml")
                                                                        || d.Source.OriginalString.Contains("Theme.Light.xaml"))).ToList();

            foreach (var dict in dictionariesToRemove)
            {
                Resources.MergedDictionaries.Remove(dict);
            }

            Resources.MergedDictionaries.Add(themeDictionary);
        }
        private void CanExecuteZoomInCommand(object sender, CanExecuteRoutedEventArgs e)
        {
            // Check whether we can zoom in, based on whether the current zoom factor is less than the maximum allowed zoom.
            e.CanExecute = JourneyZoomFactor < MaximumZoom;
        }
        private void CanExecuteZoomOutCommand(object sender, CanExecuteRoutedEventArgs e)
        {
            // Check whether we can zoom out, based on whether the current zoom factor is greater than the minimum allowed zoom.
            e.CanExecute = JourneyZoomFactor > MinimumZoom;
        }
        private void Dispose(bool disposing)
        {
            // Check we're not already disposed.
            if (!_isDisposed)
            {
                if (disposing)
                {
                    // Clean up our managed objects.
                    JourneyCanvas.Children.Clear();
                    _journeyManager.Dispose();
                    WebView.Dispose();
                }

                _isDisposed = true;
            }
        }
        private void DrawJourneySteps(TreeNode<NavigationEntry> node, Canvas canvas)
        {
            // Create a new JourneyStep for the current node, set its size and opacity based on the type of step, then add it to the canvas.
            var nodeRect = new JourneyStep(node.Value)
            {
                Width = _journeyStepSize.Width,
                Height = _journeyStepSize.Height,
                Opacity = node.Value.Type == NavigationEntryType.ArchivedStep ? InactiveOpacity : 1f
            };
            nodeRect.MouseUp += JourneyStep_MouseUp;
            var nodeRectX = node.X * (_journeyStepSize.Width + _journeyStepSpacing.Width);
            var nodeRectY = node.Y * (_journeyStepSize.Height + _journeyStepSpacing.Height);
            Canvas.SetLeft(nodeRect, nodeRectX);
            Canvas.SetTop(nodeRect, nodeRectY);
            Panel.SetZIndex(nodeRect, StepZIndex);
            canvas.Children.Add(nodeRect);

            // If the step is the active step, create a border control with label to place behind the active step to "highlight" it
            // visually.
            if (node.Value.Type == NavigationEntryType.ActiveStep)
            {
                _selectedStep = nodeRect;

                var borderThickness = 5;
                var border = new Border()
                {
                    BorderThickness = new(borderThickness),
                    CornerRadius = new(8),
                    Height = _journeyStepSize.Height + (8 * borderThickness),
                    Width = _journeyStepSize.Width + (2 * borderThickness)
                };
                border.SetResourceReference(Control.BackgroundProperty, "JourneyActiveStepBackground");
                border.SetResourceReference(Control.BorderBrushProperty, "JourneyActiveStepBackground");

                Canvas.SetLeft(border, nodeRectX - borderThickness);
                Canvas.SetTop(border, nodeRectY - borderThickness);
                Panel.SetZIndex(border, SelectedStepBorderZIndex);
                canvas.Children.Add(border);

                var label = new Label
                {
                    Content = Journey.Resources.Strings.JourneyStep_CurrentPage,
                    FontSize = 12,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Bottom
                };
                label.SetResourceReference(Control.ForegroundProperty, "JourneyActiveStepForeground");
                border.Child = label;
            }

            // Draw the vertical component of the line from the node to it's parent (if it has one).
            if (node.Parent != null)
            {
                var nodeTopMiddle = new Point(nodeRectX + (nodeRect.Width / 2), nodeRectY);
                var otherLine = new Point(nodeTopMiddle.X, nodeTopMiddle.Y - (_journeyStepSpacing.Height / 2));
                DrawLine(nodeTopMiddle, otherLine, node.Value.Type != NavigationEntryType.ArchivedStep);
            }

            // Draw lines to the nodes children, if it has any.
            if (node.Children.Count > 0)
            {
                // Draw the vertical component of the line from the node to it's children.
                var nodeBottomMiddle = new Point(nodeRectX + (nodeRect.Width / 2), nodeRectY + nodeRect.Height);
                var otherLine = new Point(nodeBottomMiddle.X, nodeBottomMiddle.Y + (_journeyStepSpacing.Height / 2));
                DrawLine(nodeBottomMiddle, otherLine, node.Value.Type != NavigationEntryType.ArchivedStep);

                // Draw the horizontal line that connects the vertical line from each child, to the vertical line from this node.
                if (node.Children.Count > 1)
                {
                    var childrenLineStart = new Point((node.RightChild!.X * (_journeyStepSize.Width + _journeyStepSpacing.Width)) + (_journeyStepSize.Width / 2),
                                                      nodeBottomMiddle.Y + (_journeyStepSpacing.Height / 2));
                    var childrenLineEnd = new Point((node.LeftChild!.X * (_journeyStepSize.Width + _journeyStepSpacing.Width)) + (_journeyStepSize.Width / 2),
                                                    nodeBottomMiddle.Y + (_journeyStepSpacing.Height / 2));

                    DrawLine(childrenLineStart, childrenLineEnd, false);

                    if (node.Children.FirstOrDefault(n => n.Value.Type != NavigationEntryType.ArchivedStep) is { } activeNode)
                    {
                        childrenLineStart = new Point((activeNode.X * (_journeyStepSize.Width + _journeyStepSpacing.Width)) + (_journeyStepSize.Width / 2),
                                                      nodeBottomMiddle.Y + (_journeyStepSpacing.Height / 2));
                        childrenLineEnd = new Point((node.X * (_journeyStepSize.Width + _journeyStepSpacing.Width)) + (_journeyStepSize.Width / 2),
                                                    nodeBottomMiddle.Y + (_journeyStepSpacing.Height / 2));

                        DrawLine(childrenLineStart, childrenLineEnd, true);
                    }
                }
            }

            // Recursively draw child nodes.
            foreach (var baseChild in node.Children)
            {
                var child = baseChild;

                DrawJourneySteps(child, canvas);
            }
        }
        private void DrawLine(Point p1, Point p2, bool activePath)
        {
            // Draw a line on the Journey canvas between two points, with an optional "active path" style, and rounded ends.

            // Determine the brush name and width based on whether this is an active path line or not.
            var brushName = activePath ? "JourneyHighlightBackground" : "LineBrush";
            var width = activePath ? 8 : 4;

            // Create our line and add it to the Journey canvas.
            var line = new Line
            {
                StrokeThickness = width,
                X1 = p1.X,
                Y1 = p1.Y,
                X2 = p2.X,
                Y2 = p2.Y
            };
            line.SetResourceReference(Line.StrokeProperty, brushName);
            JourneyCanvas.Children.Add(line);

            // Create our start and end circles to give the line rounded ends, and add them to the Journey canvas.

            // Create and add our start circle.
            var lineStart = new Ellipse
            {
                Height = width,
                Width = width,
            };
            lineStart.SetResourceReference(Ellipse.FillProperty, brushName);
            JourneyCanvas.Children.Add(lineStart);
            Canvas.SetLeft(lineStart, p1.X - (width / 2));
            Canvas.SetTop(lineStart, p1.Y - (width / 2));

            // Create and add our end circle.
            var lineEnd = new Ellipse
            {
                Height = width,
                Width = width,
            };
            lineEnd.SetResourceReference(Ellipse.FillProperty, brushName);
            JourneyCanvas.Children.Add(lineEnd);
            Canvas.SetLeft(lineEnd, p2.X - (width / 2));
            Canvas.SetTop(lineEnd, p2.Y - (width / 2));

            // Lines will overlap, which doesn't matter for lines of the same color as they all blend in together. But for the active path
            // we want to ensure the lines appear on top of other lines, so we'll bump their Z-Index.
            if (activePath)
            {
                Panel.SetZIndex(line, ActivePathLineZIndex);
                Panel.SetZIndex(lineStart, ActivePathLineZIndex);
                Panel.SetZIndex(lineEnd, ActivePathLineZIndex);
            }
        }
        private async void ExecuteHideJourneyCommand(object sender, RoutedEventArgs e)
        {
            // Hide journey.
            await HideJourney();
        }
        private void ExecuteResetViewCommand(object sender, RoutedEventArgs e)
        {
            if (_selectedStep != null)
            {
                // Reset our zoom factor back to normal.
                JourneyZoomFactor = 1;

                // Reset our pan to show our active step.
                JourneyCanvasTranslateTransform.X = _canvasHome.X;
                JourneyCanvasTranslateTransform.Y = _canvasHome.Y;
            }
        }
        private void ExecuteResetZoomCommand(object sender, RoutedEventArgs e)
        {
            // Reset our zoom factor back to 1, zooming based around the center of the visible canvas.
            var canvasCenter = GetCanvasCenter();
            ZoomCanvas(canvasCenter, false, 1);
        }
        private void ExecuteZoomInCommand(object sender, ExecutedRoutedEventArgs e)
        {
            // Initiate a zoom in of one step.
            var canvasCenter = GetCanvasCenter();
            ZoomCanvas(canvasCenter, false);
        }
        private void ExecuteZoomOutCommand(object sender, ExecutedRoutedEventArgs e)
        {
            // Initiate a zoom out of one step.
            var canvasCenter = GetCanvasCenter();
            ZoomCanvas(canvasCenter, true);
        }
        private Point GetCanvasCenter()
        {
            // Get the visible center of the journey canvas, based around the containing root grid.
            return RootGrid.TranslatePoint(new(JourneyCanvas.ActualWidth / 2, JourneyCanvas.ActualHeight / 2), JourneyCanvas);
        }
        private void NotifyPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        private void PanCanvas(double x, double y)
        {
            // Adjust our Journey canvas translate transform by the specified amount to pan the tree diagram.
            JourneyCanvasTranslateTransform.X += x;
            JourneyCanvasTranslateTransform.Y += y;
        }
        private void RefreshJourneyStepSize()
        {
            // Calculate the display size of a Journey step as a divisor of the primary monitor resolution.

            var divisor = 6;
            var width = SystemParameters.PrimaryScreenWidth / divisor;
            var height = SystemParameters.PrimaryScreenHeight / divisor;
            _journeyStepSize = new(width, height);
            _journeyStepSpacing = new(width / 2, height / 2);
        }
        private void ZoomCanvas(Point center, bool zoomOut, double? zoomFactor = null)
        {
            // We're going to zoom in/out of our tree diagram by adjust both the scale and translate transforms of our Journey canvas.

            // Calculate our current zoom center position.
            var absoluteX = center.X * JourneyZoomFactor + JourneyCanvasTranslateTransform.X;
            var absoluteY = center.Y * JourneyZoomFactor + JourneyCanvasTranslateTransform.Y;

            // Calculate the zoom increment amount, which is just 20% +/- of the current zoom factor, and cap it to our minimum and
            // maximum values. The scale transform binds to the `JourneyZoomFactor` property, so updating this property will update
            // the scale amount.
            var zoomIncrement = JourneyZoomFactor / 5;
            if (zoomFactor == null)
            {
                JourneyZoomFactor = zoomOut ? Math.Max(JourneyZoomFactor - zoomIncrement, MinimumZoom)
                                            : Math.Min(JourneyZoomFactor + zoomIncrement, MaximumZoom);
            }
            else
            {
                JourneyZoomFactor = zoomFactor.Value;
            }

            // Now we'll adjust the TranslateTransform to keep the zoom centered around the mouse cursor position. We take our previously
            // calculated value, then adjust it for the new zoom factor.
            JourneyCanvasTranslateTransform.X = absoluteX - center.X * JourneyZoomFactor;
            JourneyCanvasTranslateTransform.Y = absoluteY - center.Y * JourneyZoomFactor;
        }

        #endregion

        #region Protected

        /// <inheritdoc />
        protected override void OnLostFocus(RoutedEventArgs e)
        {
            // If we lose focus we'll set our mouse down flag to false, as if the mouse button is released when we don't have focus we
            // won't detect the event.
            _isMouseDown = false;

            base.OnLostFocus(e);
        }
        /// <inheritdoc />
        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            base.OnRenderSizeChanged(sizeInfo);

            _canvasHome.X += (sizeInfo.NewSize.Width - sizeInfo.PreviousSize.Width) / 2;
            _canvasHome.Y += (sizeInfo.NewSize.Height - sizeInfo.PreviousSize.Height) / 2;
        }

        #endregion

        #region Public

        /// <inheritdoc/>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        /// <inheritdoc/>
        public Task EnsureCoreWebView2Async(CoreWebView2Environment environment)
        {
            return WebView.EnsureCoreWebView2Async(environment);
        }
        /// <inheritdoc/>
        public Task EnsureCoreWebView2Async(CoreWebView2Environment environment = null, CoreWebView2ControllerOptions controllerOptions = null)
        {
            return WebView.EnsureCoreWebView2Async(environment, controllerOptions);
        }
        /// <inheritdoc/>
        public Task<string> ExecuteScriptAsync(string javaScript)
        {
            return WebView.ExecuteScriptAsync(javaScript);
        }
        /// <inheritdoc/>
        public void GoBack()
        {
            WebView.GoBack();
        }
        /// <inheritdoc/>
        public void GoForward()
        {
            WebView.GoForward();
        }
        /// <summary>
        /// Hides the Journey view, and restores the control to displaying web content as a standard <see cref="IWebView2"/> implementation.
        /// </summary>
        public async Task HideJourney()
        {
            // Make sure we don't attempt to transition when a transition is already in progress.
            await _journeySemaphore.WaitAsync();

            // Only hide Journey if it is visible and a step has been selected. This should usually be the case as the selected step
            // should be the "active" step, unless another step was clicked on.
            if (_isJourneyVisible && _selectedStep != null)
            {
                try
                {
                    // Set our journey visibility flag to hidden.
                    IsJourneyVisible = false;

                    // Start our WebView2 instance navigating to the step. We want this to happen as soon as possible to provide for a
                    // smoother transition from the thumbnail image to the actual web page. If we're lucky, the page will have loaded
                    // before the animation completes, resulting in a seamless transition. In the real world this is unlikely, but by
                    // starting this early we give ourselves the best chance.
                    await _journeyManager.GoToStep(_selectedStep.JourneyEntry);

                    // Indicate to the selected step that it is about to start animating - this disables the "hover" color whilst the
                    // control animates.
                    _selectedStep.IsAnimating = true;

                    // Bring the z-index of the selected step to the highest level + 1, to guarantee it is on top of all other canvas
                    // elements.
                    Panel.SetZIndex(_selectedStep, SelectedStepZIndex + 1);

                    // Set our common duration and easing function, used for all of our animations.
                    var duration = TimeSpan.FromSeconds(AnimationTime);
                    var easingFunction = new CircleEase
                    {
                        EasingMode = EasingMode.EaseInOut
                    };

                    // Create our animations for transitioning from our Journey step back into our WebView2.
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
                    var buttonBarAnimation = new DoubleAnimation
                    {
                        From = ButtonBarOpacity,
                        To = ButtonBarOpacity - 1.5,
                        Duration = duration,
                        EasingFunction = easingFunction
                    };

                    // When the animation completes, we want to switch from the thumbnail image (which is shown "full screen") into the
                    // WebView2 control. We'll also clear our JourneyCanvas of controls, and set the visibility of the parent container
                    // to collapsed to make sure no layout or rendering occurs whilst the control isn't visible.
                    scaleXAnimation.Completed += (_, _) =>
                    {
                        // Switch visibility from our canvas to our WebView2 instance.
                        WebView.Visibility = Visibility.Visible;
                        JourneyContainer.Visibility = Visibility.Collapsed;

                        // Now that the canvas is no longer visible, remove all the child controls as we don't need to keep them around
                        // anymore.
                        JourneyCanvas.Children.Clear();

                        // Finally, our hide transition is completed, so release our semaphore.
                        _journeySemaphore.Release();
                    };

                    // Start all of our animations.
                    _selectedStep.BeginAnimation(Control.WidthProperty, scaleXAnimation, HandoffBehavior.Compose);
                    _selectedStep.BeginAnimation(Control.HeightProperty, scaleYAnimation, HandoffBehavior.Compose);
                    _selectedStep.BeginAnimation(Canvas.LeftProperty, translateXAnimation, HandoffBehavior.Compose);
                    _selectedStep.BeginAnimation(Canvas.TopProperty, translateYAnimation, HandoffBehavior.Compose);
                    _selectedStep.TextArea.BeginAnimation(Control.OpacityProperty, titleAnimation, HandoffBehavior.Compose);
                    JourneyButtonBar.BeginAnimation(Control.OpacityProperty, buttonBarAnimation, HandoffBehavior.Compose);

                    // Return so we don't release our semaphore, as this will be released when our animation finishes.
                    return;
                }
                catch
                {
                    // If something happens then there probably isn't much we can do about it, but we'll swallow the exception so we
                    // can still release our semaphore and can try hiding/showing again.
                }
            }

            // We haven't carried out an animation (wrong state, no step selected, etc.), so release our semaphore.
            _journeySemaphore.Release();
        }
        /// <inheritdoc/>
        public void NavigateToString(string htmlContent)
        {
            WebView.NavigateToString(htmlContent);
        }
        /// <inheritdoc/>
        public void Reload()
        {
            WebView.Reload();
        }
        /// <summary>
        /// Shows the Journey view, hiding all web content.
        /// </summary>
        public async Task ShowJourney()
        {
            // Make sure we don't attempt to transition when a transition is already in progress.
            await _journeySemaphore.WaitAsync();

            // Only show Journey if it is not already visible.
            if (!_isJourneyVisible)
            {
                try
                {
                    // Set our journey visibility flag to visible.
                    IsJourneyVisible = true;

                    // Start a stopwatch here to monitor how long we take to populate our Journey canvas. We introduce a small delay if
                    // we did this quickly, to give the image control time to redraw, and make the transition from browser to screenshot
                    // appear "seamless". Not the neatest approach, perhaps there is something better we can do?
                    var stopWatch = new Stopwatch();
                    stopWatch.Start();

                    // Default our zoom and pan values.
                    JourneyZoomFactor = 1;
                    JourneyCanvasTranslateTransform.X = 0;
                    JourneyCanvasTranslateTransform.Y = 0;

                    // Make sure the canvas has no child controls already (though this should have been handled by the `HideJourney`
                    // method).
                    JourneyCanvas.Children.Clear();

                    // Set our Journey container to visible. It will appear behind the WebView2 control, so won't be visible, but should
                    // start doing layout and setup of itself and child controls as they are added.
                    JourneyContainer.Visibility = Visibility.Visible;

                    // Create and place all of our Journey steps on the canvas.
                    var journeySteps = await _journeyManager.GetJourney();
                    journeySteps.PerformLayout();
                    DrawJourneySteps(journeySteps, JourneyCanvas);

                    // Pan our canvas to put the selected step in the center (once it's animation has completed).
                    PanCanvas((WebView.ActualWidth / 2) - (_journeyStepSize.Width / 2f) - Canvas.GetLeft(_selectedStep),
                              (WebView.ActualHeight / 2) - (_journeyStepSize.Height / 2f) - Canvas.GetTop(_selectedStep));

                    // Store the current canvas position as our "home" position, so we can return to this later if the user presses the
                    // "Reset View" button.
                    _canvasHome = new(JourneyCanvasTranslateTransform.X, JourneyCanvasTranslateTransform.Y);

                    // Set the selected step to the highest z-index, so it is on top of all other canvas elements. This is needed because
                    // when we make the selected step full size (before it animates) it needs to cover all other elements.
                    Panel.SetZIndex(_selectedStep, SelectedStepZIndex);

                    // Set our common duration and easing function, used for all of our animations.
                    var duration = TimeSpan.FromSeconds(AnimationTime);
                    var easingFunction = new CircleEase
                    {
                        EasingMode = EasingMode.EaseInOut
                    };

                    // Create our animations for transitioning from our WebView2 control into our Journey step. We do this before we move
                    // and scale the selected step to fill the screen so that the animation can grab the current size and position of the
                    // the step, which is where it needs to animate to.
                    var scaleXAnimation = new DoubleAnimation
                    {
                        From = WebView.ActualWidth,
                        To = _journeyStepSize.Width,
                        Duration = duration,
                        EasingFunction = easingFunction
                    };
                    var scaleYAnimation = new DoubleAnimation
                    {
                        From = WebView.ActualHeight,
                        To = _journeyStepSize.Height,
                        Duration = duration,
                        EasingFunction = easingFunction
                    };
                    var translateXAnimation = new DoubleAnimation
                    {
                        From = 0 - JourneyCanvasTranslateTransform.X,
                        To = Canvas.GetLeft(_selectedStep),
                        Duration = duration,
                        EasingFunction = easingFunction
                    };
                    var translateYAnimation = new DoubleAnimation
                    {
                        From = 0 - JourneyCanvasTranslateTransform.Y,
                        To = Canvas.GetTop(_selectedStep),
                        Duration = duration,
                        EasingFunction = easingFunction
                    };
                    var titleAnimation = new DoubleAnimation
                    {
                        From = -0.6,
                        To = 0.9,
                        Duration = duration,
                        EasingFunction = easingFunction
                    };
                    var buttonBarAnimation = new DoubleAnimation
                    {
                        From = ButtonBarOpacity - 1.5,
                        To = ButtonBarOpacity,
                        Duration = duration,
                        EasingFunction = easingFunction
                    };

                    // When the animation completes, we want to set the `IsAnimating` property of the selected step to false, so that the
                    // hover effect will enable, and release our semaphore as the transition is complete.
                    buttonBarAnimation.Completed += (_, _) =>
                    {
                        // Set `IsAnimating` to false.
                        if (_selectedStep != null)
                        {
                            _selectedStep.IsAnimating = false;
                        }

                        // Finally, our show transition is completed, so release our semaphore.
                        _journeySemaphore.Release();
                    };

                    // Now that our animations are set, we move the selected step to fill the browser area, so that the image is the same
                    // size as the WebView2 control. This should result in a seamless transition when we switch visiblity between the
                    // WebView2 control and the Journey step. We also set `IsAnimating` to true, so that the hover effect is disabled
                    // whilst the animation is in progress.
                    Canvas.SetLeft(_selectedStep, 0 - JourneyCanvasTranslateTransform.X);
                    Canvas.SetTop(_selectedStep, 0 - JourneyCanvasTranslateTransform.Y);
                    if (_selectedStep != null)
                    {
                        _selectedStep.Width = WebView.ActualWidth;
                        _selectedStep.Height = WebView.ActualHeight;
                        _selectedStep.TextArea.Opacity = 0;
                        _selectedStep.IsAnimating = true;
                    }

                    // We stop the stopwatch, and check how long we have taken to set up our Journey canvas. If we have taken less than
                    // the defined "delay" time, we wait for the remainder of the delay time before starting our animation.
                    stopWatch.Stop();
                    var delay = 400;
                    if (stopWatch.ElapsedMilliseconds < delay)
                    {
                        // HACK: Feels nasty to put a delay in here, but it helps to prevent the flicker when showing the image control,
                        // which seems to appear, then paint the image in, causing a flicker when switching between the browser and the
                        // snapshot. Adding a small delay here seems to allow the image time to paint before the browser is hidden,
                        // removing the flicker. But is there a better way?
                        await Task.Delay(delay - (int)stopWatch.ElapsedMilliseconds);
                        Debug.WriteLine($"Delayed for: {delay - (int)stopWatch.ElapsedMilliseconds}ms");
                    }

                    // Hide the WebView2 control, which will reveal the canvas behind. Our selected step should be full size, so this transition
                    // should appear seamless to the user.
                    WebView.Visibility = Visibility.Collapsed;
                    JourneyCanvas.Focus();

                    // Start all of our animations.
                    if (_selectedStep != null)
                    {
                        _selectedStep.BeginAnimation(Control.WidthProperty, scaleXAnimation, HandoffBehavior.Compose);
                        _selectedStep.BeginAnimation(Control.HeightProperty, scaleYAnimation, HandoffBehavior.Compose);
                        _selectedStep.BeginAnimation(Canvas.LeftProperty, translateXAnimation, HandoffBehavior.Compose);
                        _selectedStep.BeginAnimation(Canvas.TopProperty, translateYAnimation, HandoffBehavior.Compose);
                        _selectedStep.TextArea.BeginAnimation(Control.OpacityProperty, titleAnimation, HandoffBehavior.Compose);
                    }
                    JourneyButtonBar.BeginAnimation(Control.OpacityProperty, buttonBarAnimation, HandoffBehavior.Compose);

                    // Return so we don't release our semaphore, as this will be released when our animation finishes.
                    return;
                }
                catch
                {
                    // If something happens then there probably isn't much we can do about it, but we'll swallow the exception so we
                    // can still release our semaphore and can try hiding/showing again.
                }
            }

            // We haven't carried out an animation (wrong state, no step selected, etc., so release our semaphore.
            _journeySemaphore.Release();
        }
        /// <inheritdoc/>
        public void Stop()
        {
            WebView.Stop();
        }
        /// <summary>
        /// Toggles the visibility of Journey.
        /// </summary>
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