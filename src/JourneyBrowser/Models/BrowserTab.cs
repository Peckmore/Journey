using System.ComponentModel;

namespace JourneyBrowser.Models
{
    public sealed class BrowserTab : INotifyPropertyChanged
    {
        #region Fields

        private string _address;
        private string _title;

        #endregion

        #region Events

        public event PropertyChangedEventHandler? PropertyChanged;

        #endregion

        #region Construction

        public BrowserTab(string address)
        {
            _address = address;
            _title = address;
        }

        #endregion

        #region Properties

        public string Address
        {
            get => _address;
            set
            {
                if (_address != value)
                {
                    _address = value;
                    OnPropertyChanged(nameof(Address));

                    Title = value;
                }
            }
        }
        public string Title
        {
            get => _title;
            set
            {
                if (_title != value)
                {
                    _title = value;
                    OnPropertyChanged(nameof(Title));
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