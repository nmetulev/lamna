using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Foundation.Metadata;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace Lamna
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private bool locked = true;

        public MainPage()
        {
            this.InitializeComponent();
            SizeChanged += MainPage_SizeChanged;

            if (ApiInformation.IsTypePresent(typeof(StatusBar).FullName))
            {
                StatusBar.GetForCurrentView().HideAsync();
            }

            RootFrame.Navigated += RootFrame_Navigated;
            SystemNavigationManager.GetForCurrentView().BackRequested += BackButtonBackRequested; ;

            ((App)App.Current).MainFrame = RootFrame;

            RootFrame.Navigate(typeof(Views.HomeView));


        }

        private void BackButtonBackRequested(object sender, BackRequestedEventArgs e)
        {
            if (RootFrame.CanGoBack)
            {
                RootFrame.GoBack();
                e.Handled = true;
            }
        }

        private void RootFrame_Navigated(object sender, NavigationEventArgs e)
        {
            SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = 
                RootFrame.CanGoBack ? AppViewBackButtonVisibility.Visible : AppViewBackButtonVisibility.Collapsed;
        }

        private void MainPage_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (locked)
            {
                //ContentTransform.TranslateY = e.NewSize.Height;
                LockScreenTransform.TranslateY = 0;
            }
            else
            {
               // ContentTransform.TranslateY = 20;
                LockScreenTransform.TranslateY = -e.NewSize.Height + 20;
            }

            Content.Height = e.NewSize.Height - 20;
            ContentTransform.CenterX = e.NewSize.Width / 2;
            ContentTransform.CenterY = e.NewSize.Height / 2;
            
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            locked = false;

            LoginAnimation.Stop();

            //ContentAnimation.To = 20;
            //ContentAnimation.From = Window.Current.Bounds.Height;

            ContentZoomXAnimation.To = ContentZoomYAnimation.To = 1;
            ContentZoomXAnimation.From = ContentZoomYAnimation.From = 0.8;

            ContentShadeAnimation.To = 0;
            ContentShadeAnimation.From = 1;

            LockScreenAnimation.To = - Window.Current.Bounds.Height + 20;
            LockScreenAnimation.From = 0;

            CityAnimation.To = 60;
            CityAnimation.From = 0;
            CityAnimation.Duration = TimeSpan.FromMilliseconds(2000);

            SkyAnimation.To = 1;
            SkyAnimation.From = 0;

            LockScreenAnimation.Duration =
            //ContentAnimation.Duration =
            SkyAnimation.Duration =
            ContentZoomXAnimation.Duration =
            ContentZoomYAnimation.Duration =
            ContentZoomYAnimation.Duration = TimeSpan.FromMilliseconds(500);

            LoginAnimation.Begin();
        }

        private void LockScreen_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (!locked)
            {
                locked = true;

                LoginAnimation.Stop();

                //ContentAnimation.To = Window.Current.Bounds.Height;
                //ContentAnimation.From = 20;

                ContentZoomXAnimation.To = ContentZoomYAnimation.To = 0.8;
                ContentZoomXAnimation.From = ContentZoomYAnimation.From = 1;

                ContentShadeAnimation.To = 1;
                ContentShadeAnimation.From = 0;

                LockScreenAnimation.To = 0;
                LockScreenAnimation.From = -Window.Current.Bounds.Height + 20;

                CityAnimation.To = 0;
                CityAnimation.From = 60;
                CityAnimation.Duration = TimeSpan.FromMilliseconds(2000);

                SkyAnimation.To = 0;
                SkyAnimation.From = 1;

                LockScreenAnimation.Duration =
                //ContentAnimation.Duration =
                SkyAnimation.Duration =
                ContentZoomXAnimation.Duration =
                ContentZoomYAnimation.Duration =
                ContentZoomYAnimation.Duration = TimeSpan.FromMilliseconds(500);

                LoginAnimation.Begin();
            }
        }
    }
}
