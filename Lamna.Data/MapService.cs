using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Geolocation;
using Windows.Storage;
using Windows.Storage.Streams;

namespace Lamna.Data
{
    public class MapService
    {

        public static async Task<StorageFile> GetStaticRoute(double width, double height, params Geopoint[] waypoints)
        {
            if (waypoints == null || waypoints.Length == 0) return null;

            var rawUrl = "http://dev.virtualearth.net/REST/v1/Imagery/Map/Road/Routes?";
            for (var i = 0; i < waypoints.Length; i++)
            {
                rawUrl += "waypoint." + (i + 1) + "=" +
                        waypoints[i].Position.Latitude +
                        "," +
                        waypoints[i].Position.Longitude +
                        ";66;" + (i + 1) +
                        "&";
            }

            if (width != 0 && height != 0)
            {
                rawUrl += "ms=" + width + "," + height + "&";
            }

            rawUrl += "key=" + DataSource.BingMapsKey;

            var url = new Uri(rawUrl);

            IRandomAccessStreamReference thumbnail = RandomAccessStreamReference.CreateFromUri(url);

            return await StorageFile.CreateStreamedFileFromUriAsync("map.png", url, thumbnail);

        }

    }
}
