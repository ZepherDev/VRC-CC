using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Newtonsoft.Json;

namespace VRCCC
{
    [SuppressMessage("ReSharper", "UnassignedField.Global")]    // Shaddap, Rider
    public class Subtitle
    {
        public string MovieName;
        public string LanguageName;
        [JsonProperty("SubHearingImpaired")] private string _subHearingImpaired;
        [JsonIgnore] public bool SubHearingImpaired => _subHearingImpaired == "1";
        public string SubDownloadLink;
        public double Score;
        [JsonProperty("MovieByteSize")] private string _movieByteSize;
        [JsonIgnore] public int MovieByteSize => int.Parse(_movieByteSize);
        [JsonProperty("Alternatives")] public List<Subtitle> Alternatives;
    }
}