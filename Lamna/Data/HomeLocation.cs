using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Geolocation;
using Windows.Foundation;

namespace Lamna.Data
{
    public class HomeLocation
    {
        public HomeLocation(string FamilyName, string Address, double Latitude, double Longitude)
        {
            this.NormalizedAnchorPoint = new Point(0.5, 1);
            this.FamilyName = FamilyName;
            this.Address = Address;
            Location = new Geopoint(new BasicGeoposition()
            {
                Latitude = Latitude,
                Longitude = Longitude
            }); 
        }

        public string FamilyName { get; set; }
        public string Address { get; set; }
        public Geopoint Location { get; set; }
        public Point NormalizedAnchorPoint { get; set; }
    }
}
