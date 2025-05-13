using Newtonsoft.Json;

namespace Journey
{
    internal class HistoryWrapper
    {
        [JsonProperty("currentIndex")]
        public int CurrentIndex { get; set; }

        [JsonProperty("entries")]
        public List<JourneyEntry> Entries { get; set; }
    }
}