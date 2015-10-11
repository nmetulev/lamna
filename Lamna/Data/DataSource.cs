using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lamna.Data
{
    public class DataSource
    {
        static private DataSource source;

        public List<HomeLocation> Locations { get; set; } = new List<HomeLocation>();

        private DataSource ()
        {
            Locations.Add(new HomeLocation("Metulev Family", "10351 NE 10th St, Bellevue, WA 98004", 47.619225, -122.20239));
            Locations.Add(new HomeLocation("Gates Family", "95th Ave NE, Bellevue, WA 98004", 47.645161, -122.213463));
            Locations.Add(new HomeLocation("Nadela Family", "73rd Ave NE, Medina, WA 98039", 47.625149, -122.241905));
        }

        static public DataSource GetSource()
        {
            if (source == null)
                source = new DataSource();
            return source;
        }


    }
}
