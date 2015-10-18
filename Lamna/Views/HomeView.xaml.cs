using Lamna.Data;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Devices.Geolocation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Maps;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace Lamna.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class HomeView : Page, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            var propertyChanged = this.PropertyChanged;

            if (propertyChanged != null)
            {
                propertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private List<Appointment> _upcoming;

        public List<Appointment> Upcoming
        {
            get { return _upcoming; }
            set { _upcoming = value; RaisePropertyChanged(); }
        }

        private List<Appointment> _inProgress;

        public List<Appointment> InProgress
        {
            get { return _inProgress; }
            set { _inProgress = value; RaisePropertyChanged(); }
        }

        public HomeView()
        {
            this.InitializeComponent();

            Map.MapServiceToken = DataSource.BingMapsKey;
        }


        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            var appointments = await DataSource.GetInstance().GetAppointmentsAsync();
            Map.Center = appointments.First().Location;

            Upcoming = new List<Appointment>(appointments.Where(x => x.InProgress == false));
            InProgress = new List<Appointment>(appointments.Where(x => x.InProgress == true));
            
        }

        private void LocationClicked (object sender, RoutedEventArgs e)
        {
            var appointment = (sender as Button).DataContext as Appointment;
            ((App)App.Current).MainFrame.Navigate(typeof(AppointmentView), appointment.Id);
        }
    }
    
}
