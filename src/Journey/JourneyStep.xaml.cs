using System.ComponentModel;
using System.Windows.Controls;
using System.Windows.Media;

namespace Journey
{
    internal sealed partial class JourneyStep : UserControl, INotifyPropertyChanged
    {
        #region Fields
        
        private bool _isAnimating;

        #endregion

        #region Fields

        public event PropertyChangedEventHandler? PropertyChanged;

        #endregion

        #region Construction

        public JourneyStep(JourneyEntry model)
        {
            InitializeComponent();
            RenderOptions.SetBitmapScalingMode(SnapshotImage, BitmapScalingMode.LowQuality);

            JourneyEntry = model;
            DataContext = model;
        }

        #endregion

        #region Properties

        public bool IsAnimating
        {
            get => _isAnimating;
            set
            {
                if (_isAnimating != value)
                {
                    _isAnimating = value;
                    OnPropertyChanged(nameof(IsAnimating));
                }
            }
        }
        public JourneyEntry JourneyEntry { get; }

        #endregion

        #region Methods

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}