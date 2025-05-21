using Newtonsoft.Json;
using System.Collections.Generic;

namespace Journey
{
    internal sealed class NavigationHistory
    {
        #region Properties

        [JsonProperty("currentIndex")]
        public int CurrentIndex { get; set; }

        [JsonProperty("entries")]
        public List<JourneyEntry> Entries { get; set; }

        #endregion
    }
}