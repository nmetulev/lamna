using Lamna.Data;
using Microsoft.Graphics.Canvas;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace Lamna.Views
{
    public sealed partial class PictureEditor : UserControl
    {
        public PictureEditor()
        {
            this.InitializeComponent();

            this.Loaded += PictureEditor_Loaded;
        }

        private void PictureEditor_Loaded(object sender, RoutedEventArgs e)
        {
            Inker.InkPresenter.InputDeviceTypes =
                                CoreInputDeviceTypes.Pen |
                                CoreInputDeviceTypes.Touch |
                                CoreInputDeviceTypes.Mouse;
        }
        
        public LocationPicture Picture
        {
            get { return (LocationPicture)GetValue(PictureProperty); }
            set
            {
                if (value != null && !value.Equals((LocationPicture)GetValue(PictureProperty)))
                {
                    SetValue(PictureProperty, value);
                    LoadPicture(value);
                }
            }
        }

        // Using a DependencyProperty as the backing store for Picture.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty PictureProperty =
            DependencyProperty.Register("Picture", typeof(LocationPicture), typeof(PictureEditor), new PropertyMetadata(null));


        private Task LoadPicture(LocationPicture pic)
        {
            Imager.Source = new BitmapImage(new Uri(pic.RawImageUri));
            return LoadInk(pic);
        }

        private async Task LoadInk(LocationPicture pic)
        {
            Inker.InkPresenter.StrokeContainer.Clear();
            StorageFile file = await TryGetFileAsync(pic.InkUri);

            if (file != null)
            {
                using (var stream = await file.OpenReadAsync())
                {
                    await Inker.InkPresenter.StrokeContainer.LoadAsync(stream);
                }
            }

        }

        private async Task SaveInk(LocationPicture pic)
        {
            var localFolder = ApplicationData.Current.LocalFolder;
            var file = await localFolder.CreateFileAsync(pic.ID + ".gif", CreationCollisionOption.ReplaceExisting);

            if (file != null && Inker.InkPresenter.StrokeContainer.GetStrokes().Count > 0)
            {
                using (var stream = await file.OpenAsync(FileAccessMode.ReadWrite))
                {
                    await Inker.InkPresenter.StrokeContainer.SaveAsync(stream);
                }
            }
            pic.InkUri = "ms-appdata:///local/" + pic.ID + ".gif";
        }

        private async Task RenderImageAsync(LocationPicture pic)
        {
            CanvasDevice device = CanvasDevice.GetSharedDevice();
            CanvasRenderTarget renderTarget = new CanvasRenderTarget(device, (int)Inker.ActualWidth, (int)Inker.ActualHeight, 96);

            var image = await CanvasBitmap.LoadAsync(device, new Uri(pic.RawImageUri));

            using (var ds = renderTarget.CreateDrawingSession())
            {
                ds.Clear(Colors.White);
                ds.DrawImage(image, image.Bounds);
                ds.DrawInk(Inker.InkPresenter.StrokeContainer.GetStrokes());
            }

            var filename = pic.ID + "_final.jpg";
            var localFolder = ApplicationData.Current.LocalFolder;

            // TODO
            // datatemplate has a handle on the file and it won't let me replace it.
            // too lazy to fix, so just create a new file
            var file = await localFolder.CreateFileAsync(filename, CreationCollisionOption.GenerateUniqueName);
            using (var fileStream = await file.OpenAsync(FileAccessMode.ReadWrite))
                await renderTarget.SaveAsync(fileStream, CanvasBitmapFileFormat.Jpeg, 1f);
            pic.ImageUri = "ms-appdata:///local/" + file.Name;
        }

        private async Task<StorageFile> TryGetFileAsync(string uri)
        {
            StorageFile file = null;
            try
            {
                file = await StorageFile.GetFileFromApplicationUriAsync(new Uri(uri));
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }

            return file;
        }

        public async Task SavePicture()
        {
            await SaveInk(Picture);
            await RenderImageAsync(Picture);
        }
        
    }
}
