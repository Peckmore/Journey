using System.Windows.Media.Imaging;

namespace Journey
{
    internal enum JourneyEntryType
    {
        HistoryBack,
        HistoryFoward,
        ActiveStep,
        ArchivedStep,
    }
    internal sealed class JourneyEntry
    {
        #region Construction

        public JourneyEntry()
        { }
        public JourneyEntry(int id, string title, string transitionType, string url, string userTypeUrl)
        {
            Id = id;
            Title = title;
            TransitionType = transitionType;
            Url = url;
            UserTypedUrl = userTypeUrl;
        }

        #endregion

        #region Properties

        public int Id { get; set; }
        public bool IsActive { get; set; }
        public BitmapFrame? Snapshot { get; set; }
        public string Title { get; set; }
        public string TransitionType { get; set; }
        public string Url { get; set; }
        public string UserTypedUrl { get; set; }

        #endregion

        #region Methods

        public void Update(JourneyEntry entry)
        {
            Id = entry.Id;
            IsActive = entry.IsActive;
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