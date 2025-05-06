using System.ComponentModel;
using System.Windows.Controls;
using System.Windows.Media;

namespace Journey
{
    internal partial class JourneyStep : UserControl, INotifyPropertyChanged
    {
        #region Fields
        
        private bool _isAnimating;

        #endregion

        #region Fields

        public event PropertyChangedEventHandler? PropertyChanged;

        #endregion

        #region Construction

        public JourneyStep()
        {
            InitializeComponent();

            RenderOptions.SetBitmapScalingMode(SnapshotImage, BitmapScalingMode.LowQuality);
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

        #endregion

        #region Methods

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}