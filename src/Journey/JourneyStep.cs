using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Journey
{
    /// <summary>
    /// Represents a step within the Journey view of a <see cref="JourneyWebView2" /> control.
    /// </summary>
    [TemplatePart(Name = "PART_TextArea", Type = typeof(Border))]
    public sealed class JourneyStep : Control, INotifyPropertyChanged
    {
        #region Fields

        #region Private

        private bool _isAnimating;

        #endregion

        #region Public Static

        /// <summary>
        /// DependencyProperty for <see cref="CornerRadius" /> property.
        /// </summary>
        public static readonly DependencyProperty CornerRadiusProperty = DependencyProperty.Register(nameof(CornerRadius),
                                                                                                     typeof(CornerRadius),
                                                                                                     typeof(JourneyStep),
                                                                                                     new FrameworkPropertyMetadata(new CornerRadius(),
                                                                                                                                   FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender));
        /// <summary>
        /// DependencyProperty for <see cref="FontFamilyBold" /> property.
        /// </summary>
        public static readonly DependencyProperty FontFamilyBoldProperty = DependencyProperty.Register(nameof(FontFamilyBold),
                                                                                                       typeof(FontFamily),
                                                                                                       typeof(JourneyStep));

        #endregion

        #endregion

        #region Events

        /// <inheritdoc />
        public event PropertyChangedEventHandler? PropertyChanged;

        #endregion

        #region Construction

        internal JourneyStep(NavigationEntry model)
        {
            // Set the model for the control instance.
            DataContext = model;
            JourneyEntry = model;

            // Apply our default style key.
            DefaultStyleKey = typeof(JourneyStep);
        }

        #endregion

        #region Properties

        #region Internal

        internal NavigationEntry JourneyEntry { get; }
        internal UIElement? TextArea { get; private set; }

        #endregion

        #region Public

        /// <summary>
        /// A flag to indicate whether the control is currently animating to/from the full <see cref="JourneyWebView2" /> size and the
        /// smaller "step" size.
        /// </summary>
        public bool IsAnimating
        {
            get => _isAnimating;
            internal set
            {
                if (_isAnimating != value)
                {
                    _isAnimating = value;
                    OnPropertyChanged(nameof(IsAnimating));
                }
            }
        }
        /// <summary>
        /// The CornerRadius property allows users to control the roundness of the corners independently by setting a radius value for
        /// each corner.
        /// </summary>
        public CornerRadius CornerRadius
        {
            get => (CornerRadius)GetValue(CornerRadiusProperty);
            set => SetValue(CornerRadiusProperty, value);
        }
        /// <summary>
        /// The font family of the desired bold font. This will only affect parts of the default template that use bold text.
        /// </summary>
        public FontFamily FontFamilyBold
        {
            get => (FontFamily)GetValue(FontFamilyBoldProperty);
            set => SetValue(FontFamilyBoldProperty, value);
        }

        #endregion

        #endregion

        #region Methods

        #region Private

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion

        #region Public

        /// <inheritdoc />
        public override void OnApplyTemplate()
        {
            TextArea = GetTemplateChild("PART_TextArea") as UIElement;
        }

        #endregion

        #endregion
    }
}