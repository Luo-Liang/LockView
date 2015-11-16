using InfoViewApp.WP81;
using InfoViewApp.WP81.Tasks;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel.Resources.Core;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Streams;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
using Windows.Web.Http;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace LockViewApp.W81
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
            this.SizeChanged += MainPage_SizeChanged;
            this.Loaded += MainPage_Loaded;
            nextButton.IsEnabled = false;
        }

        private void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            RedrawCropper();
        }

        void RedrawCropper()
        {
            int width = 1920, height = 1080;
            int.TryParse(resoluionHeight.Text, out height);
            int.TryParse(resolutionWidth.Text, out width);
            if (width > 1920 * 2 || height > 1200 * 2)
            {
                width = 3840;
                height = 2400;
                resoluionHeight.Text = height.ToString();
                resolutionWidth.Text = width.ToString();
            }
            LockViewApplicationState.Instance.PreviewLayoutContract.TargetHeight = height;
            LockViewApplicationState.Instance.PreviewLayoutContract.TargetWidth = width;
            var boundingBoxhwRatio = (boundingBox.ActualHeight - 50) / (boundingBox.ActualWidth - 30);
            var actualScreenRatio = 1.0 * height / width;
            if (actualScreenRatio > boundingBoxhwRatio)
            {
                //align height.
                imageViewBox.Height = boundingBox.ActualHeight - 50;
                imageViewBox.Width = width * (boundingBox.ActualHeight - 50) / height;
            }
            else
            {
                imageViewBox.Width = boundingBox.ActualWidth - 30;
                imageViewBox.Height = height * (boundingBox.ActualWidth - 30) / width;
            }
            adjustImagePreview(false);
            System.Diagnostics.Debug.Assert(imageCropper.ActualHeight / imageCropper.ActualWidth == 1.0 * height / width);
            //imageCropper.Source = adjustImagePreview(false);
        }
        private async void MainPage_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            //if (ViewManagementHelper.DetermineFullScreen())
            //{
            //    croppingGuideText.Text = "Pick a best view";
            //    croppingGuide.Text = "Pick a best view";
            //    boundingBox.Visibility = Visibility.Visible;
            //    maximizeAnimationGrid.Visibility = Visibility.Collapsed;
            //    maximize.Stop();
            //    nextButton.IsEnabled = imageCropper.Source != null;
            //}
            //else
            //{
            //    croppingGuide.Text = "Maximize the window";
            //    croppingGuideText.Text = "In order to find a best fit, maximize your window first.";
            //    nextButton.IsEnabled = false;
            //    boundingBox.Visibility = Visibility.Collapsed;
            //    maximizeAnimationGrid.Visibility = Visibility.Visible;
            //    maximize.Begin();
            //}
            RedrawCropper();
            //adjustImagePreview(false);
        }
        void MakeBusy()
        {
            nextButton.IsEnabled = false;
            busyRIng.IsActive = true;
        }
        void MakeIdle()
        {
            busyRIng.IsActive = false;
            nextButton.IsEnabled = imageCropper.Source != null;
        }
        WriteableBitmap originalMap;

        private async void listBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedItem = listBox.SelectedItem;
            if (selectedItem == null)
            {
                nextButton.IsEnabled = false;
                return;
            }
            //imageCropper.Source = null;
            var context = selectedItem as ListBoxContentVM;
            originalMap = new WriteableBitmap(1, 1);
            int height, width;
            int.TryParse(resoluionHeight.Text, out height);
            int.TryParse(resolutionWidth.Text, out width);
            LockViewApplicationState.Instance.PreviewLayoutContract.TargetHeight = height;
            LockViewApplicationState.Instance.PreviewLayoutContract.TargetWidth = width;
            if(context.NavigationType == "bing")
            {
                LockViewApplicationState.Instance.SelectedImageSource = InfoViewApp.WP81.ImageSource.Bing;
            }
            else if (context.NavigationType == "nasa")
            {
                LockViewApplicationState.Instance.SelectedImageSource = InfoViewApp.WP81.ImageSource.NASA;
            }
            else
            {
                LockViewApplicationState.Instance.SelectedImageSource = InfoViewApp.WP81.ImageSource.Local;
            }
            //RedrawCropper();
            try
            {
                if (context.NavigationType != "library")
                {
                    MakeBusy();
                    Windows.Web.Http.HttpClient client = new Windows.Web.Http.HttpClient();
                    Uri uri = null;
                    if (context.NavigationType == "bing")
                        uri = new Uri(await BackgroundTaskHelper.GetBingImageFitScreenUrl(client));
                    else if (context.NavigationType == "nasa")
                        uri = new Uri(await BackgroundTaskHelper.GetNASAImageFitScreenUrl(client));
                    var concatChar = '?';
                    var requestUrl = uri.ToString();
                    if (requestUrl.Contains("?")) concatChar = '&';
                    var requestParameter = $"{requestUrl}{concatChar}resolution={LockViewApplicationState.Instance.PreviewLayoutContract.TargetWidth}x{LockViewApplicationState.Instance.PreviewLayoutContract.TargetHeight}";
                    var requestContent = new HttpStringContent(Newtonsoft.Json.JsonConvert.SerializeObject(requestParameter));
                    requestContent.Headers.ContentType = new Windows.Web.Http.Headers.HttpMediaTypeHeaderValue("application/json");
                    var response = await client.PostAsync(new Uri("http://cloudimagecomposition.azurewebsites.net/ImageComposition.svc/RequestImage"), requestContent);
                    var responseStr = await response.Content.ReadAsStringAsync();
                    var rawBytes = Newtonsoft.Json.JsonConvert.DeserializeObject<byte[]>(responseStr);
                    originalMap = await originalMap.FromStream(new MemoryStream(rawBytes));
                }
                else
                {
                    FileOpenPicker picker = new FileOpenPicker();
                    picker.ViewMode = PickerViewMode.Thumbnail;
                    picker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;
                    picker.FileTypeFilter.Add(".jpg");
                    picker.FileTypeFilter.Add(".jpeg");
                    picker.FileTypeFilter.Add(".png");
                    var storageFile = await picker.PickSingleFileAsync();
                    using (var stream = await storageFile.OpenStreamForReadAsync())
                    {
                        originalMap = await originalMap.FromStream(stream);
                    }
                }
                //now resize the actual image.
                adjustImagePreview(true);
            }
            catch
            {
                croppingGuide.Text = ResourceLoader.LockViewLoader.GetString("ServiceUnavailable");
                croppingGuideText.Text = ResourceLoader.LockViewLoader.GetString("ServiceUnavailableContent");
            }
            finally
            {
                MakeIdle();
            }
        }

        private void adjustImagePreview(bool useActualTarget)
        {
            if (originalMap == null) return;
            int actualHeight, actualWidth;
            actualHeight = originalMap.PixelHeight;
            actualWidth = originalMap.PixelWidth;
            var actualRatio = 1.0 * actualHeight / actualWidth;
            int targetHeight = useActualTarget ? (int)imageViewBox.ActualHeight : (int)imageViewBox.Height;
            int targetWidth = useActualTarget ? (int)imageViewBox.ActualWidth : (int)imageViewBox.Width;
            var targetRatio = 1.0 * targetHeight / targetWidth;
            if (actualRatio > targetRatio)
            {
                //user swipe up and down.
                imageCropper.Height = actualHeight * (targetWidth - 0) / actualWidth;
                imageCropper.Width = (targetWidth - 0);
                //currentMap = originalMap.Resize(targetWidth, actualHeight * targetWidth / actualWidth, WriteableBitmapExtensions.Interpolation.NearestNeighbor);
            }
            else
            {
                imageCropper.Height = (targetHeight - 0);
                imageCropper.Width = actualWidth * (targetHeight - 0) / actualHeight;
                //currentMap = originalMap.Resize(actualWidth * targetHeight / actualHeight, targetHeight, WriteableBitmapExtensions.Interpolation.NearestNeighbor);
            }
            imageCropper.Source = originalMap;
        }

        private void resoluionHeight_TextChanged(object sender, TextChangedEventArgs e)
        {
            RedrawCropper();
        }

        private void resolutionWidth_TextChanged(object sender, TextChangedEventArgs e)
        {
            RedrawCropper();
        }

        private void repickImage_Click(object sender, RoutedEventArgs e)
        {
            listBox_SelectionChanged(null, null);
        }

        private async void nextButton_Click(object sender, RoutedEventArgs e)
        {
            var canvas = imageViewBox;
            var WB_CapturedImage = originalMap;
            var hO = canvas.HorizontalOffset;
            var vO = canvas.VerticalOffset;
            //height larger than width.
            double imgRatio = 1.0 * WB_CapturedImage.PixelHeight / WB_CapturedImage.PixelWidth;
            double screenRatio = imageViewBox.ActualHeight / imageViewBox.ActualWidth;
            if (imgRatio > screenRatio)
            {
                //user swipes up and down.
                double heightExtent = vO / canvas.ExtentHeight;
                int actualHeight = (int)(heightExtent * WB_CapturedImage.PixelHeight);
                WB_CapturedImage = WB_CapturedImage.Crop(0, actualHeight, WB_CapturedImage.PixelWidth, (int)(screenRatio * WB_CapturedImage.PixelWidth));
            }
            else
            {
                double widthExtent = hO / canvas.ExtentWidth;
                int actualWidth = (int)(widthExtent * WB_CapturedImage.PixelWidth);
                WB_CapturedImage = WB_CapturedImage.Crop(actualWidth, 0, (int)(WB_CapturedImage.PixelHeight / screenRatio), WB_CapturedImage.PixelHeight);
            }
            StorageFile sf = await ApplicationData.Current.LocalFolder.CreateFileAsync("myfile.jpg", CreationCollisionOption.ReplaceExisting);
            try
            {
                using (var stream = await sf.OpenAsync(FileAccessMode.ReadWrite))
                {
                    await WB_CapturedImage.ToStreamAsJpeg(stream);
                }
            }
            catch (Exception ex)
            {

            }
            this.Frame.Navigate(typeof(SourceSelectionAndPreview), WB_CapturedImage);
        }
    }
}
