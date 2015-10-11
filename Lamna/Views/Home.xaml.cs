using Lamna.Data;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
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
    public sealed partial class Home : Page
    {
        ObservableCollection<HomeLocation> Locations = new ObservableCollection<HomeLocation>();

        public Home()
        {
            this.InitializeComponent();

            Map.Center = DataSource.GetSource().Locations.First().Location;

            foreach (var loc in DataSource.GetSource().Locations)
            {
                Locations.Add(loc);
            }
        }

        private void LocationClicked (object sender, RoutedEventArgs e)
        {
            var loc = (sender as Button).DataContext as HomeLocation;
            ((App)App.Current).MainFrame.Navigate(typeof(LocationView), loc);
        }
    }
    
}
