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
        private bool _floaterActive = true;

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
            Map.Style = MapStyle.AerialWithRoads;
        }


        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            var appointments = await DataSource.GetInstance().GetAppointmentsAsync();
            Map.Center = appointments.First().Location;

            Upcoming = new List<Appointment>(appointments.Where(x => x.InProgress == false));
            InProgress = new List<Appointment>(appointments.Where(x => x.InProgress == true));

            Window.Current.SizeChanged += Current_SizeChanged;
            
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            Window.Current.SizeChanged -= Current_SizeChanged;

        }

        private void Current_SizeChanged(object sender, Windows.UI.Core.WindowSizeChangedEventArgs e)
        {
            if (!_floaterActive && e.Size.Width < 700)
            {
                FloaterTransform.TranslateY = e.Size.Height - 110;
            }
            else if (!_floaterActive)
            {
                ToggleFloater();
            }
        }

        private void LocationClicked (object sender, RoutedEventArgs e)
        {
            var appointment = (sender as Button).DataContext as Appointment;
            ((App)App.Current).MainFrame.Navigate(typeof(AppointmentView), appointment.Id);
        }


        private void FloaterToggleButton_Click(object sender, RoutedEventArgs e)
        {
            ToggleFloater();
        }

        private void ToggleFloater()
        {
            if (_floaterActive)
            {
                _floaterActive = false;
                FloaterTransformAnimation.From = 0;
                FloaterTransformAnimation.To = Window.Current.Bounds.Height - 110;
                FloaterStoryboard.Stop();
                FloaterStoryboard.Begin();

                FloaterToggleSymbol.Symbol = Symbol.Up;
                FloaterScrollviewer.ChangeView(0, 0, 1);
                FloaterScrollviewer.VerticalScrollMode = ScrollMode.Disabled;
                FloaterScrollviewer.VerticalScrollBarVisibility = ScrollBarVisibility.Disabled;
            }
            else
            {
                _floaterActive = true;
                FloaterTransformAnimation.To = 0;
                FloaterTransformAnimation.From = Window.Current.Bounds.Height - 110;

                FloaterStoryboard.Stop();
                FloaterStoryboard.Begin();

                FloaterToggleSymbol.Symbol = Symbol.DockBottom;
                FloaterScrollviewer.VerticalScrollMode = ScrollMode.Auto;
                FloaterScrollviewer.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
            }
        }

        private async void ShowOnMapClicked(object sender, RoutedEventArgs e)
        {
            if (Window.Current.Bounds.Width < 700 && _floaterActive)
            {
                ToggleFloater();
            }
            var appointment = (sender as Button).DataContext as Appointment;
            await Map.TrySetViewAsync(appointment.Location);
            await Map.TryZoomToAsync(16);
        }
    }
    
}
