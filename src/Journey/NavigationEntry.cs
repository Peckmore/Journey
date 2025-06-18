using System.Windows.Media.Imaging;

namespace Journey
{
    /// <summary>
    /// Represents an entry in the WebView2 "session history"/travellog (obtained via DevTools), and also a step in the users journey.
    /// </summary>
    internal sealed class NavigationEntry
    {
        #region Construction

        public NavigationEntry(int id, string title, string transitionType, string url, string userTypedUrl)
        {
            Id = id;
            Title = title;
            TransitionType = transitionType;
            Type = NavigationEntryType.ArchivedStep;
            Url = url;
            UserTypedUrl = userTypedUrl;
        }

        #endregion

        #region Properties

        public BitmapSource? FavIcon { get; set; }
        public int Id { get; private set; }
        public BitmapSource? Snapshot { get; set; }
        public string Title { get; set; }
        public string TransitionType { get; set; }
        public NavigationEntryType Type { get; set; }
        public string Url { get; private set; }
        public string UserTypedUrl { get; set; }

        #endregion

        #region Methods

        public void Update(NavigationEntry entry)
        {
            // When we deserialize the history, we'll get new NavigationEntry objects each time. This method allows us to update an existing
            // entry (already in the tree) with the data from the deserialized entry.

            Id = entry.Id;
            Title = entry.Title;
            TransitionType = entry.TransitionType;
            Url = entry.Url;
            UserTypedUrl = entry.UserTypedUrl;

            // Don't replace a valid FavIcon with a blank one.
            if (entry.FavIcon != null)
            {
                FavIcon = entry.FavIcon;
            }

            // Don't replace a valid snapshot with a blank one.
            if (entry.Snapshot != null)
            {
                Snapshot = entry.Snapshot;
            }
        }

        #endregion
    }
}