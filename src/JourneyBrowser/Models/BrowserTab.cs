using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace JourneyBrowser.Models
{
    public sealed class BrowserTab : INotifyPropertyChanged
    {
        #region Fields

        private string _address;
        private bool _canGoBack;
        private bool _canGoForward;
        private bool _canShowJourney;
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
                    OnPropertyChanged();
                }
            }
        }
        public bool CanGoBack
        {
            get => _canGoBack;
            set
            {
                if (_canGoBack != value)
                {
                    _canGoBack = value;
                    OnPropertyChanged();
                }
            }
        }
        public bool CanGoForward
        {
            get => _canGoForward;
            set
            {
                if (_canGoForward != value)
                {
                    _canGoForward = value;
                    OnPropertyChanged();
                }
            }
        }
        public bool CanShowJourney
        {
            get => _canShowJourney;
            set
            {
                if (_canShowJourney != value)
                {
                    _canShowJourney = value;
                    OnPropertyChanged();
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
                    OnPropertyChanged();
                }
            }
        }

        #endregion

        #region Methods

        #region Private

        private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion

        #endregion
    }
}