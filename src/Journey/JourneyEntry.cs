using System.Windows.Media.Imaging;

namespace Journey
{
    internal sealed class JourneyEntry
    {
        #region Construction

        public JourneyEntry()
        { }
        public JourneyEntry(int id, string title, string transitionType, string url, string userTypedUrl)
        {
            Id = id;
            Title = title;
            TransitionType = transitionType;
            Type = JourneyEntryType.ArchivedStep;
            Url = url;
            UserTypedUrl = userTypedUrl;
        }

        #endregion

        #region Properties

        public int Id { get; set; }
        public BitmapFrame? Snapshot { get; set; }
        public string Title { get; set; }
        public string TransitionType { get; set; }
        public JourneyEntryType Type { get; set; }
        public string Url { get; set; }
        public string UserTypedUrl { get; set; }

        #endregion

        #region Methods

        public void Update(JourneyEntry entry)
        {
            Id = entry.Id;
            Title = entry.Title;
            TransitionType = entry.TransitionType;
            Url = entry.Url;
            UserTypedUrl = entry.UserTypedUrl;

            if (entry.Snapshot != null)
            {
                Snapshot = entry.Snapshot;
            }
        }

        #endregion
    }
}