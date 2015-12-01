using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Geolocation;
using Windows.Storage;

namespace Lamna.Data
{
    public class DataSource
    {
        const string cacheKey = "lamna_data";
        const string userCacheKey = "lamna_user";

        public static string BingMapsKey = "<ADD YOUR BING MAPS API KEY HERE>";

        public static DateTime Now = new DateTime(2015, 10, 21, 0, 0, 0);

        static private DataSource source;
        private FileService _fileService;

        private List<Appointment> _appointments { get; set; }

        private Account _user;

        static public DataSource GetInstance()
        {
            if (source == null)
                source = new DataSource();
            return source;
        }

        private DataSource ()
        {
            _fileService = new FileService();

            
        }

        public Account GetUser()
        {
            if (_user != null) return _user;

            object user;
            ApplicationData.Current.LocalSettings.Values.TryGetValue(userCacheKey, out user);

            if (user != null)
            {
                string userString = user as string;
                _user = JsonConvert.DeserializeObject<Account>(userString);

            }

            return _user;
        }

        public void SaveUser(Account user)
        {
            string userString = JsonConvert.SerializeObject(user);
            ApplicationData.Current.LocalSettings.Values.Remove(userCacheKey);
            ApplicationData.Current.LocalSettings.Values.Add(userCacheKey, userString);
            _user = user;
        }

        public async Task<List<Appointment>> GetAppointmentsAsync()
        {
            var toReturn = _appointments ?? (_appointments = await _fileService.ReadAsync<Appointment>(cacheKey) ?? new List<Appointment>());

            if (toReturn.Count == 0)
            {
                toReturn.Add(new Appointment("Metulev", "73rd Ave NE, Medina, WA 98039", new DateTime(2015, 10, 21, 0, 30, 0), 47.625149, -122.241905));
                toReturn.Add(new Appointment("Chauhan", "95th Ave NE, Bellevue, WA 98004", new DateTime(2015, 10, 21, 2, 30, 0), 47.645161, -122.213463));
                toReturn.Add(new Appointment("Burtoft", "10450 NE 4th St, Bellevue, WA 98004", new DateTime(2015, 10, 21, 5, 30, 0), 47.619225, -122.20239));
                toReturn.Add(new Appointment("Sardo", "809 21st Ave, Seattle, WA 98122", new DateTime(2015, 10, 21, 7, 30, 0), 47.609097, -122.305031));
                toReturn.Add(new Appointment("Crawford", "681 19th Ave E, Seattle, WA 98112", new DateTime(2015, 10, 21, 7, 30, 0), 47.625072, -122.307472, true));
                toReturn.Add(new Appointment("Catuhe", "95th Ave NE, Bellevue, WA 98004", new DateTime(2015, 10, 21, 2, 30, 0), 47.645161, -122.213463, true));
                toReturn.Add(new Appointment("Kolesnikov", "10351 NE 10th St, Bellevue, WA 98004", new DateTime(2015, 10, 21, 0, 30, 0), 47.619225, -122.20239, true));
                toReturn.Add(new Appointment("Brown", "95th Ave NE, Bellevue, WA 98004", new DateTime(2015, 10, 21, 2, 30, 0), 47.645161, -122.213463, true));
                toReturn.Add(new Appointment("Willhelmsen", "73rd Ave NE, Medina, WA 98039", new DateTime(2015, 10, 21, 5, 0, 0), 47.625149, -122.241905, true));
                await _fileService.WriteAsync<Appointment>(cacheKey, toReturn);
                _appointments = toReturn;
            }

            return toReturn;
        }

        public async Task<Appointment> GetAppointmentAsync(string key)
        {
            return (await GetAppointmentsAsync()).FirstOrDefault(x => x.Id.Equals(key));
        }

        public async Task UpdateAppointmentAsync(Appointment appointment)
        {
            var itemToUpdate = await GetAppointmentAsync(appointment.Id);
            _appointments.Remove(itemToUpdate);
            _appointments.Add(appointment);
            await _fileService.WriteAsync<Appointment>(cacheKey, _appointments);
        }

        public Task UpdateAppointmentsAsync()
        {
            return _fileService.WriteAsync<Appointment>(cacheKey, _appointments);
        }

        public static Geopoint GetCurrentLocation()
        {
            return new Geopoint(new BasicGeoposition()
            {
                Latitude = 47.609097,
                Longitude = -122.305031,
            });
        }

    }
}
