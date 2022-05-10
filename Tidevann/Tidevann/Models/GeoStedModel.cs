using Newtonsoft.Json;

namespace Tidevann.Models
{
    public class GeoStedModel
    {
        public string Fylkesnavn { get; set; }
        public string Stedsnavn { get; set; }
        public string Lon { get; set; }

        public string Lan { get; set; }

    


        public string Navneobjekttype { get; set; }


        public string PickerText { get; set; } = "Default";

        public GeoStedModel()
        {
            
        }
        public void SetPickerText()
        {
            PickerText = Stedsnavn + ", " + Navneobjekttype + ", " + Fylkesnavn;
        }
    }
}
