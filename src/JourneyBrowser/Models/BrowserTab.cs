using System.ComponentModel;

namespace JourneyBrowser.Models
{
    public sealed class BrowserTab : INotifyPropertyChanged
    {
        #region Fields

        private string _address;

        #endregion

        #region Events

        public event PropertyChangedEventHandler? PropertyChanged;

        #endregion

        #region Construction

        public BrowserTab(string address)
        {
            _address = address;
            OnPropertyChanged(nameof(Address));
        }

        #endregion

        #region Properties

        public string Address
        {
            get => _address;
            set
            {
                _address = value;
                OnPropertyChanged(nameof(Address));
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