using Newtonsoft.Json;

namespace Journey
{
    internal class NavigationHistory
    {
        #region Properties

        [JsonProperty("currentIndex")]
        public int CurrentIndex { get; set; }

        [JsonProperty("entries")]
        public List<JourneyEntry> Entries { get; set; }

        #endregion
    }
}