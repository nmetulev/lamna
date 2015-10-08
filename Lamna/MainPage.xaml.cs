﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
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
        }

        private void MainPage_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (locked)
            {
                ContentTransform.TranslateY = e.NewSize.Height;
                LockScreenTransform.TranslateY = 0;
            }
            else
            {
                ContentTransform.TranslateY = 60;
                LockScreenTransform.TranslateY = -e.NewSize.Height + 60;
            }

            Content.Height = e.NewSize.Height - 60;
            
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            locked = false;

            RootFrame.Navigate(typeof(Views.Home));

            LoginAnimation.Stop();

            ContentAnimation.To = 60;
            ContentAnimation.From = ContentTransform.TranslateY;

            LockScreenAnimation.To = - Window.Current.Bounds.Height + 60;
            LockScreenAnimation.From = 0;

            SkyAnimation.To = 1;
            SkyAnimation.From = 0;

            LockScreenAnimation.Duration = ContentAnimation.Duration = SkyAnimation.Duration = TimeSpan.FromMilliseconds(500);

            LoginAnimation.Begin();
        }

        private void LockScreen_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (!locked)
            {
                locked = true;

                LoginAnimation.Stop();

                ContentAnimation.To = Window.Current.Bounds.Height;
                ContentAnimation.From = 60;

                LockScreenAnimation.To = 0;
                LockScreenAnimation.From = -Window.Current.Bounds.Height + 60;

                SkyAnimation.To = 0;
                SkyAnimation.From = 1;

                LockScreenAnimation.Duration = ContentAnimation.Duration = SkyAnimation.Duration = TimeSpan.FromMilliseconds(500);

                LoginAnimation.Begin();
            }
        }
    }
}
