using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lamna.Data
{
    public class LocationPicture
    {
        public string ID { get; set; }
        public string InkID { get; set; }
        public LocationEnumaration Location { get; set; }
        public DefectEnumeration Defect { get; set; }
        public string Note { get; set; }
    }
}
