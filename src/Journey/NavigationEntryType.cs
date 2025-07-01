namespace Journey
{
    /// <summary>
    /// Represents the type of entry/step within a users Journey.
    /// </summary>
    public enum NavigationEntryType
    {
        /// <summary>
        /// A web page visited before the active page that is still in the browser travellog.
        /// </summary>
        HistoryBack,

        /// <summary>
        /// A web page visited after the active page that is still in the browser travellog.
        /// </summary>
        HistoryForward,

        /// <summary>
        /// The currently active page (the page the browser is on).
        /// </summary>
        ActiveStep,

        /// <summary>
        /// A web page visited that is no longer in the browser travellog.
        /// </summary>
        ArchivedStep,
    }
}