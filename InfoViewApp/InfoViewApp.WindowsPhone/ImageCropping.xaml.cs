using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Devices.Input;
using Windows.Foundation;
using Windows.Foundation.Collections;
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

namespace InfoViewApp
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
        Stream origStream;
        public ImageCropping()
        {
            InitializeComponent();
            //fire when render frame
            this.Loaded += ImageCropping_Loaded;
        }
        private void ImageCropping_Loaded(object sender, RoutedEventArgs e)
        {
            FileOpenPicker openPicker = new FileOpenPicker();
            openPicker.ViewMode = PickerViewMode.Thumbnail;
            openPicker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;
            openPicker.FileTypeFilter.Add(".jpg");
            openPicker.FileTypeFilter.Add(".jpeg");
            openPicker.FileTypeFilter.Add(".png");
            openPicker.PickSingleFileAndContinue();
            //Set WriteableBitmap with OrgianlImage
        }
        private void SaveBtn_Click(object sender, RoutedEventArgs e)
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
            WB_CroppedImage.ToStream(LockViewApplicationState.Instance.AccessStream,BitmapEncoder.JpegEncoderId);
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
                OriginalImage.Source = WB_CapturedImage;
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
