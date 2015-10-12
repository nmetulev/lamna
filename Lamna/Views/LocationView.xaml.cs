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
using Windows.UI.Xaml.Navigation;
using Windows.UI.Xaml.Shapes;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace Lamna.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class LocationView : Page
    {
        public HomeLocation HomeLocation{ get; set; }

        public LocationView()
        {
            this.InitializeComponent();

        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.Parameter != null)
            {
                HomeLocation = e.Parameter as HomeLocation;
            }

            InitializeInker();
        }

        private void InitializeInker()
        {
            Inker.InkPresenter.InputDeviceTypes = Windows.UI.Core.CoreInputDeviceTypes.Mouse;

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

        private async Task RecognizeInkerText()
        {
            var inkRecognizer = new InkRecognizerContainer();
            var recognitionResults = await inkRecognizer.RecognizeAsync(Inker.InkPresenter.StrokeContainer, InkRecognitionTarget.All);

            foreach (var result in recognitionResults)
            {
                List<UIElement> elements = new List<UIElement>(
                    VisualTreeHelper.FindElementsInHostCoordinates(
                        new Rect(new Point(result.BoundingRect.X, result.BoundingRect.Y), 
                        new Size(result.BoundingRect.Width, result.BoundingRect.Height)), 
                    this));

                //var elipse = new Ellipse();
                //elipse.Height = 2;
                //elipse.Width = 2;
                //elipse.Fill = new SolidColorBrush(Colors.Red);
                //Canvas.SetLeft(elipse, result.BoundingRect.X - 1);
                //Canvas.SetTop(elipse, result.BoundingRect.Y - 1);
                //canvasce.Children.Add(elipse);

                TextBox box = elements.Where(el => el is TextBox && (el as TextBox).IsEnabled).First() as TextBox;

                if (box != null) box.Text = result.GetTextCandidates().FirstOrDefault();
            }

            Inker.InkPresenter.StrokeContainer.Clear();
        }
        

        private void InkPresenter_StrokesCollected(InkPresenter sender, InkStrokesCollectedEventArgs args)
        {
            Debug.WriteLine("StrokesCollected");
        }
        

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            ((App)App.Current).MainFrame.Navigate(typeof(CameraView), HomeLocation);
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            RecognizeInkerText();
        }
    }
}
