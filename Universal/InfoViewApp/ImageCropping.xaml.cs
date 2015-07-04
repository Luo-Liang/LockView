using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Data.Json;
using Windows.Devices.Input;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Globalization;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Streams;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkID=390556

namespace InfoViewApp.WP81
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ImageCropping : Page
    {
        WriteableBitmap WB_CapturedImage;//for original image
        WriteableBitmap WB_CroppedImage;//for cropped image
        //Variables for the crop feature
        Windows.Foundation.Point Point1, Point2;
        public ImageCropping()
        {
            InitializeComponent();
            //fire when render frame
        }
        const string Locator = "http://www.bing.com/HPImageArchive.aspx?format=js&idx=0&n=1&mkt={0}";
        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            var parameter = e.Parameter;
            var retrieveSource = parameter.ToString().Split(':')[1];
            if (retrieveSource == "library")
            {
                FileOpenPicker openPicker = new FileOpenPicker();
                openPicker.ViewMode = PickerViewMode.Thumbnail;
                openPicker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;
                openPicker.FileTypeFilter.Add(".jpg");
                openPicker.FileTypeFilter.Add(".jpeg");
                openPicker.FileTypeFilter.Add(".png");
                openPicker.PickSingleFileAndContinue();
            }
            else if (retrieveSource == "bing")
            {
                var lang = Language;
                var reqString = string.Format(Locator, lang);
                HttpClient client = new HttpClient();
                progressRing.Visibility = Visibility.Visible;
                SaveBtn.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                try
                {
                    var json = await client.GetStringAsync(reqString);
                    var jObj = JsonObject.Parse(json);
                    var imgRequestUrl = jObj.GetNamedArray("images")[0].GetObject().GetNamedString("url");
                    var response = await client.GetAsync(string.Format("http://www.bing.com{0}", imgRequestUrl));
                    WB_CapturedImage = new WriteableBitmap(1, 1);
                    WB_CapturedImage = await WB_CapturedImage.FromStream(await response.Content.ReadAsStreamAsync());
                    OriginalImage.Source = WB_CapturedImage = LoadScaledImage(WB_CapturedImage);
                }
                catch { }
                progressRing.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                SaveBtn.Visibility = Windows.UI.Xaml.Visibility.Visible;
            }
        }

        private WriteableBitmap LoadScaledImage(WriteableBitmap WB_CapturedImage)
        {
            var imgHeight = WB_CapturedImage.PixelHeight;
            var imgWidth = WB_CapturedImage.PixelWidth;
            double widthPixel, heightPixel;
            ResolutionProvider.GetScreenSizeInPixels(out heightPixel, out widthPixel);
            if (widthPixel / heightPixel > imgWidth / imgHeight)
            {
                //swipe up and down
                return WB_CapturedImage.Resize((int)widthPixel, (int)((widthPixel / imgWidth) * imgHeight), WriteableBitmapExtensions.Interpolation.NearestNeighbor);
            }
            else
            {
                //swipe left and right.
                return WB_CapturedImage.Resize((int)(imgWidth * (heightPixel / imgHeight)), (int)heightPixel, WriteableBitmapExtensions.Interpolation.NearestNeighbor);
            }
        }

        private async void SaveBtn_Click(object sender, RoutedEventArgs e)
        {
            var hO = canvas.HorizontalOffset;
            var vO = canvas.VerticalOffset;
            //height larger than width.
            double imgRatio = 1.0 * WB_CapturedImage.PixelHeight / WB_CapturedImage.PixelWidth;
            double screenRatio = ResolutionProvider.GetScreenHeightWidthRatio();
            if (imgRatio > 1)
            {
                //user swipes up and down.
                double heightExtent = vO / canvas.ExtentWidth;
                int actualHeight = (int)(heightExtent * WB_CapturedImage.PixelHeight);
                WB_CroppedImage = WB_CapturedImage.Crop(0, actualHeight, WB_CapturedImage.PixelWidth, (int)(screenRatio * WB_CapturedImage.PixelWidth));
            }
            else
            {
                double widthExtent = hO / canvas.ExtentWidth;
                int actualWidth = (int)(widthExtent * WB_CapturedImage.PixelWidth);
                WB_CroppedImage = WB_CapturedImage.Crop(actualWidth, 0, (int)(WB_CapturedImage.PixelHeight / screenRatio), WB_CapturedImage.PixelHeight);
            }
            OriginalImage.Source = WB_CroppedImage;
            //this is a jpeg stream now.
            double widthPixel, heightPixel;
            ResolutionProvider.GetScreenSizeInPixels(out heightPixel, out widthPixel);
            WB_CapturedImage = WB_CapturedImage.Resize((int)widthPixel, (int)heightPixel, WriteableBitmapExtensions.Interpolation.NearestNeighbor);
            await SaveBitmapAsJpeg(LockViewApplicationState.Instance.PersistFileName, WB_CapturedImage);
            Frame.Navigate(typeof(Interest));
        }

        async Task SaveBitmapAsJpeg(string fileName, WriteableBitmap bitmap)
        {
            try
            {
                var file = await ApplicationData.Current.LocalFolder.CreateFileAsync(fileName);
                using (var fs = await file.OpenStreamForWriteAsync())
                    await bitmap.ToStreamAsJpeg(fs.AsRandomAccessStream());
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Exception during save as Jpeg: " + ex.Message);
            }
        }

        private void OriginalImage_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            Point1 = e.GetCurrentPoint(OriginalImage).Position;
            //Set first touchable cordinates as point1
            Point2 = Point1;
        }

        private void OriginalImage_PointerMoved(object sender, PointerRoutedEventArgs e)
        {
            Point2 = e.GetCurrentPoint(OriginalImage).Position;
        }

        private void OriginalImage_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            Point2 = e.GetCurrentPoint(OriginalImage).Position;
        }

        public async void ContinueFileOpenPicker(Windows.ApplicationModel.Activation.FileOpenPickerContinuationEventArgs args)
        {
            if (args.Files.Count > 0)
            {
                var storageFile = args.Files[0];
                WB_CapturedImage = new WriteableBitmap(1, 1);
                WB_CapturedImage = await WB_CapturedImage.FromStream(await storageFile.OpenStreamForReadAsync());
                OriginalImage.Source = WB_CapturedImage = LoadScaledImage(WB_CapturedImage);
                //OriginalImage.Height = WB_CapturedImage.PixelHeight;
                //OriginalImage.Width = WB_CapturedImage.PixelWidth;
            }
            else
            {
                Frame.GoBack();
            }
        }
    }
}
