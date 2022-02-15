using Newtonsoft.Json;

namespace Tidevann.Models
{
    public class GeoStedModel
    {
        [JsonProperty("ssrID")]
        public string SsrId { get; set; }

        [JsonProperty("navnetype")]
        public string Navnetype { get; set; }

        [JsonProperty("kommunenavn")]
        public string Kommunenavn { get; set; }

        [JsonProperty("fylkesnavn")]
        public string Fylkesnavn { get; set; }

        [JsonProperty("stedsnavn")]
        public string Stedsnavn { get; set; }

        [JsonProperty("aust")]
        public string Lon { get; set; }

        [JsonProperty("nord")]
        public string Lan { get; set; }

        [JsonProperty("skrivemaatestatus")]
        public string Skrivemaatestatus { get; set; }

        [JsonProperty("spraak")]
        public string Spraak { get; set; }

        [JsonProperty("skrivemaatenavn")]
        public string Skrivemaatenavn { get; set; }

        [JsonProperty("epsgKode")]
        public string EpsgKode { get; set; }

        public string PickerText { get; set; } = "Default";

        public GeoStedModel()
        {
            
        }
        public void SetPickerText()
        {
            PickerText = Stedsnavn + ", " + Kommunenavn + ", " + Fylkesnavn;
        }
    }
}
