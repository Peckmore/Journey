using Newtonsoft.Json;
using System.Collections.Generic;

namespace Journey
{
    /// <summary>
    /// Used to deserialize WebView2 "session history" from a JSON string returned from the Chromium dev tools.
    /// </summary>
    internal sealed class SessionHistory
    {
        #region Properties

        /// <summary>
        /// The index in the navigation history of the current page.
        /// </summary>
        [JsonProperty("currentIndex")]
        public int CurrentIndex { get; set; }

        /// <summary>
        /// List of entries in the "session history".
        /// </summary>
        [JsonProperty("entries")]
        public List<NavigationEntry> Entries { get; set; }

        #endregion
    }
}