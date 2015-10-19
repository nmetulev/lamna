using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Geolocation;
using Windows.Foundation;

namespace Lamna.Data
{
    public class Appointment
    {
        public Appointment()
        {

        }

        public Appointment(string familyName, string address, DateTime time, double latitude, double longitude, bool inProgress = false)
        {
            this.Id = Guid.NewGuid().ToString();
            this.NormalizedAnchorPoint = new Point(0.5, 1);
            this.FamilyName = familyName;
            this.Address = address;
            this.Pictures = new List<LocationPicture>();
            this.Time = time;
            InProgress = inProgress;
            Location = new Geopoint(new BasicGeoposition()
            {
                Latitude = latitude,
                Longitude = longitude
            }); 
        }

        public string Id { get; set; }
        public string FamilyName { get; set; }
        public string Address { get; set; }
        public Geopoint Location { get; set; }
        public Point NormalizedAnchorPoint { get; set; }
        public List<LocationPicture> Pictures { get; set; }
        public bool InProgress { get; set; }
        public DateTime Time { get; set; }
    }

}
