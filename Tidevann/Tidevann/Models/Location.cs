

namespace Tidevann.Models
{
    public class Location
    {
        
        public string Name { get; set; }
        public double Lan { get; set; }
        public double Lon { get; set; }

        public Location(string nam, double lan, double lon)
        {
            Name = nam;
            Lan = lan;
            Lon = lon;
        }
    }
}
