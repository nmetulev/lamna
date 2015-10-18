using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace Lamna.Data
{
    public class DataSource
    {
        const string cacheKey = "lamna_data";
        const string userCacheKey = "lamna_user";

        public static string BingMapsKey = "FySvlXjivCDPa2T4FHQB~R9u7hEyUOYk8mGfNOn599g~AiWs3OvU5Iumz9ytkmcb_lD_ZcHTZR2mNXMqWq74Nra7mEano0SbVuTWHTIxqXDU";

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
                toReturn.Add(new Appointment("Metulev Family", "10351 NE 10th St, Bellevue, WA 98004", 47.619225, -122.20239));
                toReturn.Add(new Appointment("Gates Family", "95th Ave NE, Bellevue, WA 98004", 47.645161, -122.213463));
                toReturn.Add(new Appointment("Nadela Family", "73rd Ave NE, Medina, WA 98039", 47.625149, -122.241905));
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



    }
}
