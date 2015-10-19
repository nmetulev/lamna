using Lamna.Data;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Input.Inking;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Xaml.Shapes;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace Lamna.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class AppointmentView : Page
    {
        public Appointment Data{ get; set; }
        DispatcherTimer timer;

        public AppointmentView()
        {
            this.InitializeComponent();
            timer = new DispatcherTimer();
            timer.Tick += Timer_Tick;
            timer.Interval = TimeSpan.FromSeconds(3);

        }

        

        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.Parameter != null)
            {
                Data = await DataSource.GetInstance().GetAppointmentAsync(e.Parameter as string) ;
                StaticMapImage.Source = new BitmapImage(MapService.GetAerialImageUrl(Data.Location, 19, 2000, 700));

            }



            InitializeInker();
            
        }

        protected async override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            await DataSource.GetInstance().UpdateAppointmentsAsync();
        }

        private void InitializeInker()
        {
            Inker.InkPresenter.InputDeviceTypes = Windows.UI.Core.CoreInputDeviceTypes.Pen;

            var drawingAttributes = new InkDrawingAttributes
            {
                DrawAsHighlighter = false,
                Color = Colors.DarkBlue,
                PenTip = PenTipShape.Circle,
                IgnorePressure = false,
                Size = new Size(3, 3)
            };
            Inker.InkPresenter.UpdateDefaultDrawingAttributes(drawingAttributes);
            Inker.InkPresenter.StrokesCollected += InkPresenter_StrokesCollected;
        }
        
        
        private void Root_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            DecideInputMethod(e.Pointer);
        }


        private void Root_PointerMoved(object sender, PointerRoutedEventArgs e)
        {
            DecideInputMethod(e.Pointer);
        }

        private bool InkerActive = false;

        private void DecideInputMethod(Pointer pointer)
        {
            if (pointer.PointerDeviceType == Windows.Devices.Input.PointerDeviceType.Pen && !InkerActive)
            {
                // enable Inker
                InkerActive = true;
                InkerContainer.Visibility = Visibility.Visible;
            }
            else if (pointer.PointerDeviceType != Windows.Devices.Input.PointerDeviceType.Pen && InkerActive)
            {
                // disable Inker
                InkerActive = false;
                RecognizeInkerText();
                timer.Stop();
                InkerContainer.Visibility = Visibility.Collapsed;

            }
        }

        private async Task RecognizeInkerText()
        {
            var inkRecognizer = new InkRecognizerContainer();
            var recognitionResults = await inkRecognizer.RecognizeAsync(Inker.InkPresenter.StrokeContainer, InkRecognitionTarget.All);

            List<TextBox> boxes = new List<TextBox>();

            foreach (var result in recognitionResults)
            {
                List<UIElement> elements = new List<UIElement>(
                    VisualTreeHelper.FindElementsInHostCoordinates(
                        new Rect(new Point(result.BoundingRect.X, result.BoundingRect.Y), 
                        new Size(result.BoundingRect.Width, result.BoundingRect.Height)), 
                    this));

                TextBox box = elements.Where(el => el is TextBox && (el as TextBox).IsEnabled).First() as TextBox;

                if (box != null)
                {
                    if (!boxes.Contains(box))
                    {
                        boxes.Add(box);
                        box.Text = "";
                    }
                    box.Text += " " + result.GetTextCandidates().FirstOrDefault().Trim();
                }
            }

            Inker.InkPresenter.StrokeContainer.Clear();
        }
        

        private void InkPresenter_StrokesCollected(InkPresenter sender, InkStrokesCollectedEventArgs args)
        {
            timer.Start();
        }

        private void Timer_Tick(object sender, object e)
        {
            timer.Stop();
            RecognizeInkerText();
        }


        private void Button_Click(object sender, RoutedEventArgs e)
        {
            ((App)App.Current).MainFrame.Navigate(typeof(CameraView), Data.Id);
        }
        
        private void GridView_ItemClick(object sender, ItemClickEventArgs e)
        {
            var pic = e.ClickedItem as LocationPicture;
            Editor.Picture = pic;
            EditorContainer.Visibility = Visibility.Visible;
        }

        private void DiscardPictureClicked(object sender, RoutedEventArgs e)
        {
            EditorContainer.Visibility = Visibility.Collapsed;
        }

        private void SavePictureClicked(object sender, RoutedEventArgs e)
        {
            EditorContainer.Visibility = Visibility.Collapsed;
            Editor.SavePicture();
        }
    }
}
