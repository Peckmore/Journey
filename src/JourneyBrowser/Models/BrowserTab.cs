using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Media.Imaging;

namespace JourneyBrowser.Models
{
    internal sealed class BrowserTab : INotifyPropertyChanged
    {
        #region Fields

        private string _address;
        private bool _canGoBack;
        private bool _canGoForward;
        private bool _canShowJourney;
        private BitmapSource? _favIcon;
        private Action? _goBackAction;
        private Action? _goForwardAction;
        private Action? _reloadAction;
        private string _title;
        private Func<Task>? _toggleJourneyFunction;

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
        public BitmapSource? FavIcon
        {
            get => _favIcon;
            set
            {
                if (_favIcon != value)
                {
                    _favIcon = value;
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

        #region Public

        public void GoBack()
        {
            _goBackAction?.Invoke();
        }
        public void GoForward()
        {
            _goForwardAction?.Invoke();
        }
        public void GoHome()
        {
            Address = Settings.Singleton.HomePage;
        }
        public void Reload()
        {
            _reloadAction?.Invoke();
        }
        public void SetupActions(Action goBackAction,
                                 Action goForwardAction,
                                 Action reloadAction,
                                 Func<Task> toggleJourneyFunction)
        {
            // Set our action variables, which we invoke when the user requests one of the commands that is carried out by the
            // WebView2 instance.

            _goBackAction = goBackAction;
            _goForwardAction = goForwardAction;
            _reloadAction = reloadAction;
            _toggleJourneyFunction = toggleJourneyFunction;
        }
        public void ToggleJourney()
        {
            _toggleJourneyFunction?.Invoke();
        }

        #endregion

        #endregion
    }
}