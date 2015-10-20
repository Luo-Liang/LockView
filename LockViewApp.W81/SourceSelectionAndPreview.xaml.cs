using InfoViewApp.WP81;
using System;
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
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace LockViewApp.W81
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class SourceSelectionAndPreview : Page
    {
        public SourceSelectionAndPreview()
        {
            this.InitializeComponent();
            this.SizeChanged += SourceSelectionAndPreview_SizeChanged;
            this.Loaded += SourceSelectionAndPreview_Loaded;
        }

        private void SourceSelectionAndPreview_Loaded(object sender, RoutedEventArgs e)
        {
            RenderPreview();
        }

        WriteableBitmap selectedImage;
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            selectedImage = e.Parameter as WriteableBitmap;
            imageCropper.Source = selectedImage;
            //RenderPreview();
        }

        private void SourceSelectionAndPreview_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            RenderPreview();
        }

        void RenderPreview()
        {
            var height = LockViewApplicationState.Instance.PreviewLayoutContract.TargetHeight;
            var width = LockViewApplicationState.Instance.PreviewLayoutContract.TargetWidth;
            var boundingBoxhwRatio = (boundingBox.ActualHeight-50) / (boundingBox.ActualWidth-30);
            var actualScreenRatio = 1.0 * height / width;
            if (actualScreenRatio > boundingBoxhwRatio)
            {
                //align height.
                mockScreen.Height = boundingBox.ActualHeight-50;
                mockScreen.Width = width * (boundingBox.ActualHeight-50) / height;
            }
            else
            {
                mockScreen.Width = boundingBox.ActualWidth-30;
                mockScreen.Height = height * (boundingBox.ActualWidth-30) / width;
            }
            //mockScreen.Height = imageViewBox.Height;
            //mockScreen.Width = imageViewBox.Width;
            //imageCropper.Height = imageViewBox.Height;
            //imageCropper.Width = imageViewBox.Width;
        }

        private void listBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
    }
}
