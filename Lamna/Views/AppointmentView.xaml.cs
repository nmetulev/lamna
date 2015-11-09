using Lamna.Data;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI;
using Windows.UI.Core;
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

        Dictionary<string, ObservableCollection<LocationPicture>> Pictures;

        public AppointmentView()
        {
            this.InitializeComponent();
            timer = new DispatcherTimer();
            timer.Tick += Timer_Tick;
            timer.Interval = TimeSpan.FromSeconds(3);

            Pictures = new Dictionary<string, ObservableCollection<LocationPicture>>();
            Pictures.Add("Roof", new ObservableCollection<LocationPicture>());
            Pictures.Add("Garage", new ObservableCollection<LocationPicture>());
            Pictures.Add("Exterior", new ObservableCollection<LocationPicture>());
            Pictures.Add("Electrical", new ObservableCollection<LocationPicture>());
            Pictures.Add("Plumbing", new ObservableCollection<LocationPicture>());
            Pictures.Add("Kitchen", new ObservableCollection<LocationPicture>());
            Pictures.Add("Bathroom", new ObservableCollection<LocationPicture>());
            Pictures.Add("Heating", new ObservableCollection<LocationPicture>());
            Pictures.Add("Air Conditioning", new ObservableCollection<LocationPicture>());
            Pictures.Add("Interior", new ObservableCollection<LocationPicture>());

            foreach (var item in Pictures)
            {
                var pivotItem = new PivotItem() { Header = item.Key };
                var gridview = new GridView()
                {
                    ItemsSource = item.Value,
                    ItemTemplate = this.Resources["PicturesTemplate"] as DataTemplate,
                    IsItemClickEnabled = true,
                    SelectionMode = ListViewSelectionMode.None,
                    ItemsPanel = Resources["PicturesPanel"] as ItemsPanelTemplate
                };

                gridview.ItemClick += GridView_ItemClick;

                pivotItem.Content = gridview;
                MainPivot.Items.Add(pivotItem);
            }

            MainPivot.SelectionChanged += MainPivot_SelectionChanged;

        }


        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.Parameter != null)
            {
                Data = await DataSource.GetInstance().GetAppointmentAsync(e.Parameter as string) ;
                StaticMapImage.Source = new BitmapImage(MapService.GetAerialImageUrl(Data.Location, 19, 2000, 500));
                
                foreach (var pic in Data.Pictures)
                {

                    var collection = Pictures[pic.Location.ToString()];
                    collection.Add(pic);
                }

            }

            
            //SystemNavigationManager.GetForCurrentView().BackRequested += BackButtonBackRequested;

            InitializeInker();
            
        }

        protected async override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            await DataSource.GetInstance().UpdateAppointmentsAsync();
            //SystemNavigationManager.GetForCurrentView().BackRequested -= BackButtonBackRequested;
        }

        //private void BackButtonBackRequested(object sender, BackRequestedEventArgs e)
        //{
        //    if (EditorContainer.Visibility == Visibility.Visible)
        //    {
        //        CloseEditor();
        //        e.Handled = true;
        //    }
        //}

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

                TextBox box = elements.Where(el => el is TextBox && (el as TextBox).IsEnabled).FirstOrDefault() as TextBox;

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


        private void RefreshCommandSelection()
        {
            if (EditorContainer.Visibility == Visibility.Visible)
            {
                CameraButton.Visibility = Visibility.Collapsed;
                GenerateButton.Visibility = Visibility.Collapsed;
                SaveButton.Visibility = Visibility.Visible;
                CancelButton.Visibility = Visibility.Visible;
                DeleteButton.Visibility = Visibility.Visible;
                CommandSeperator.Visibility = Visibility.Collapsed;
                PasteImageButton.Visibility = Visibility.Collapsed;
            }
            else
            {
                CameraButton.Visibility = Visibility.Visible;
                GenerateButton.Visibility = Visibility.Visible;
                SaveButton.Visibility = Visibility.Collapsed;
                CancelButton.Visibility = Visibility.Collapsed;
                DeleteButton.Visibility = Visibility.Collapsed;

                if (MainPivot.SelectedIndex == 0)
                {
                    CommandSeperator.Visibility = Visibility.Collapsed;
                    PasteImageButton.Visibility = Visibility.Collapsed;
                }
                else
                {
                    CommandSeperator.Visibility = Visibility.Visible;
                    PasteImageButton.Visibility = Visibility.Visible;
                }
            }
        }

        private void MainPivot_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            RefreshCommandSelection();
        }


        private void CameraButton_Click(object sender, RoutedEventArgs e)
        {
            ((App)App.Current).MainFrame.Navigate(typeof(CameraView), Data.Id);
        }
        
        private void GridView_ItemClick(object sender, ItemClickEventArgs e)
        {
            var pic = e.ClickedItem as LocationPicture;
            Editor.Picture = pic;
            EditorContainer.Visibility = Visibility.Visible;
            RefreshCommandSelection();
        }

        private void DiscardPictureClicked(object sender, RoutedEventArgs e)
        {
            CloseEditor();
        }

        private void SavePictureClicked(object sender, RoutedEventArgs e)
        {
            CloseEditor();
            Editor.SavePicture();
        }

        private void CloseEditor()
        {
            EditorContainer.Visibility = Visibility.Collapsed;

            RefreshCommandSelection();
        }
        
        private async void PasteImageButton_Click(object sender, RoutedEventArgs e)
        {
            Button bt = sender as Button;
            bt.IsEnabled = false;
            var dataPackageView = Windows.ApplicationModel.DataTransfer.Clipboard.GetContent();
            if (dataPackageView.Contains(StandardDataFormats.Bitmap))
            {
                IRandomAccessStreamReference imageReceived = null;

                try

                {

                    imageReceived = await dataPackageView.GetBitmapAsync();
                    var id = Guid.NewGuid().ToString();
                    using (var stream = await imageReceived.OpenReadAsync())
                    {
                        var decoder = await BitmapDecoder.CreateAsync(stream);

                        var localFolder = ApplicationData.Current.LocalFolder;
                        var file = await localFolder.CreateFileAsync(id + ".jpg", CreationCollisionOption.ReplaceExisting);

                        using (var outputStream = await file.OpenAsync(FileAccessMode.ReadWrite))
                        {
                            var encoder = await BitmapEncoder.CreateForTranscodingAsync(outputStream, decoder);
                            await encoder.FlushAsync();
                        }
                    }

                    LocationPicture pic = new LocationPicture();
                    pic.ID = id;
                    pic.RawImageUri = pic.ImageUri = "ms-appdata:///local/" + id + ".jpg";

                    LocationEnumaration loc;
                    string currentLoc = (MainPivot.SelectedItem as PivotItem).Header as string;
                    Enum.TryParse<LocationEnumaration>(currentLoc, out loc);
                    pic.Location = loc;

                    Data.Pictures.Add(pic);
                    Pictures[currentLoc].Add(pic);


                    // Don't save until navigating from page for demo purposes
                    //await DataSource.GetInstance().UpdateAppointmentsAsync();


                }
                catch (Exception ex)

                {

                }


            }
            
            bt.IsEnabled = true;
        }

        private async void GenerateButton_Click(object sender, RoutedEventArgs e)
        {
            Button bt = sender as Button;
            bt.IsEnabled = false;
            var file = await Windows.ApplicationModel.Package.Current.InstalledLocation.GetFileAsync(@"Report.docx");

            if (file != null)
            {
                await Windows.System.Launcher.LaunchFileAsync(file);
            }
            bt.IsEnabled = true;
        }

        private async void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            Button bt = sender as Button;
            bt.IsEnabled = false;

            Data.Pictures.Remove(Editor.Picture);
            Pictures[(MainPivot.SelectedItem as PivotItem).Header as string].Remove(Editor.Picture);

            CloseEditor();

            await DataSource.GetInstance().UpdateAppointmentsAsync();

            bt.IsEnabled = true;
        }
    }
}
