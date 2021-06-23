using System.Diagnostics.CodeAnalysis;
using Newtonsoft.Json;

namespace VRCCC
{
    [SuppressMessage("ReSharper", "UnassignedField.Global")]    // Shaddap, Rider
    public class Subtitle
    {
        public string LanguageName;
        [JsonProperty("SubHearingImpaired")] private string _subHearingImpaired;
        [JsonIgnore] public bool SubHearingImpaired => _subHearingImpaired == "1";
        public string SubDownloadLink;
        public double Score;
    }
}