using Newtonsoft.Json;
using System.Windows.Media.Imaging;

namespace Journey
{
    /// <summary>
    /// Represents an entry in the WebView2 "session history"/travellog (obtained via DevTools), and also a step in the users journey.
    /// </summary>
    public sealed class NavigationEntry
    {
        #region Construction

        [JsonConstructor]
        internal NavigationEntry(int id, string title, string transitionType, string url, string userTypedUrl)
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

        /// <summary>
        /// The FavIcon for this entry.
        /// </summary>
        public BitmapSource? FavIcon { get; set; }
        /// <summary>
        /// The WebView2 travellog ID for this entry.
        /// </summary>
        public int Id { get; private set; }
        /// <summary>
        /// The image snapshot for this entry.
        /// </summary>
        public BitmapSource? Snapshot { get; set; }
        /// <summary>
        /// The title for this entry.
        /// </summary>
        public string Title { get; set; }
        /// <summary>
        /// The type of transition to this entry.
        /// </summary>
        public string TransitionType { get; set; }
        /// <summary>
        /// The type of this entry.
        /// </summary>
        public NavigationEntryType Type { get; set; }
        /// <summary>
        /// The URL for this entry.
        /// </summary>
        public string Url { get; private set; }
        /// <summary>
        /// The URL as entered by the user for this entry.
        /// </summary>
        public string UserTypedUrl { get; set; }

        #endregion

        #region Methods

        internal void Update(NavigationEntry entry)
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