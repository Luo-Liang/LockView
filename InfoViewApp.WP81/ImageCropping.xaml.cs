using Microsoft.Phone.Controls;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using Windows.Data.Json;
using Windows.Storage;
using Windows.Storage.Pickers;
using System.Windows.Navigation;
using System.Windows;
using Windows.Web.Http;
using Windows.Storage.Streams;
using System.Windows.Input;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkID=390556

namespace InfoViewApp
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ImageCropping : PhoneApplicationPage
    {
        WriteableBitmap WB_CapturedImage;//for original image
        WriteableBitmap WB_CroppedImage;//for cropped image
        //Variables for the crop feature
        System.Windows.Point Point1, Point2;
        public ImageCropping()
        {
            InitializeComponent();
            //fire when render frame
        }
        const string Locator = "http://www.bing.com/HPImageArchive.aspx?format=js&idx=0&n=1&mkt={0}";
        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            var parameter = NavigationContext.QueryString["ImgSrc"];
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
                SaveBtn.Visibility = Visibility.Collapsed;
                try
                {
                    var json = await client.GetStringAsync(new Uri(reqString));
                    var jObj = JsonObject.Parse(json);
                    var imgRequestUrl = jObj.GetNamedArray("images")[0].GetObject().GetNamedString("url");
                    var response = await client.GetAsync(new Uri(string.Format("http://www.bing.com{0}", imgRequestUrl)));
                    WB_CapturedImage = new WriteableBitmap(1, 1);
                    WB_CapturedImage = WB_CapturedImage.FromStream((await response.Content.ReadAsInputStreamAsync()).AsStreamForRead());
                    OriginalImage.Source = WB_CapturedImage = LoadScaledImage(WB_CapturedImage);
                }
                catch { }
                progressRing.Visibility = Visibility.Collapsed;
                SaveBtn.Visibility = Visibility.Visible;
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
            //Frame.Navigate(typeof(Interest));
        }

        async Task SaveBitmapAsJpeg(string fileName, WriteableBitmap bitmap)
        {
            try
            {
                var file = await ApplicationData.Current.LocalFolder.CreateFileAsync(fileName);
                using (var fs = await file.OpenStreamForWriteAsync())
                    bitmap.SaveJpeg(fs, bitmap.PixelWidth, bitmap.PixelHeight, 0, 100);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Exception during save as Jpeg: " + ex.Message);
            }
        }

        private void OriginalImage_PointerPressed(object sender, MouseEventArgs e)
        {
            Point1 = e.GetPosition(OriginalImage);
            //Set first touchable cordinates as point1
            Point2 = Point1;
        }

        private void OriginalImage_PointerMoved(object sender, MouseEventArgs e)
        {
            Point2 = e.GetPosition(OriginalImage);
        }

        private void OriginalImage_PointerReleased(object sender, MouseEventArgs e)
        {
            Point2 = e.GetPosition(OriginalImage);
        }

        public async void ContinueFileOpenPicker(Windows.ApplicationModel.Activation.FileOpenPickerContinuationEventArgs args)
        {
            if (args.Files.Count > 0)
            {
                var storageFile = args.Files[0];
                WB_CapturedImage = new WriteableBitmap(1, 1);
                WB_CapturedImage = WB_CapturedImage.FromStream(await storageFile.OpenStreamForReadAsync());
                OriginalImage.Source = WB_CapturedImage = LoadScaledImage(WB_CapturedImage);
                //OriginalImage.Height = WB_CapturedImage.PixelHeight;
                //OriginalImage.Width = WB_CapturedImage.PixelWidth;
            }
            else
            {
                NavigationService.GoBack();
            }
        }
    }
}
