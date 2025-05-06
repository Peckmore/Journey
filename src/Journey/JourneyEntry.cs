using System.Windows.Media.Imaging;

namespace Journey
{
    internal sealed class JourneyEntry
    {
        #region Construction

        public JourneyEntry()
        {
        }

        #endregion

        #region Properties

        public bool IsActive { get; set; }
        public BitmapFrame? Snapshot { get; set; }
        public string? Title { get; set; }
        public string? Url { get; set; }

        #endregion
    }
}