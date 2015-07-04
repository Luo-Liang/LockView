using Microsoft.Phone.Controls;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using Windows.Storage;
using System.Windows.Navigation;
using System.Windows;
using System.Windows.Input;
using System.Windows.Controls;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkID=390556

namespace InfoViewApp.WP81
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class Preview : PhoneApplicationPage
    {
        public Preview()
        {
            this.InitializeComponent();
        }

        async Task<WriteableBitmap> OpenBitmapFromFile(string fileName, int width, int height)
        {
            try
            {
                WriteableBitmap bitmap = new WriteableBitmap(width, height);

                var file = await ApplicationData.Current.LocalFolder.GetFileAsync(fileName);
                using (var fs = await file.OpenStreamForReadAsync())
                    bitmap.SetSource(fs);

                return bitmap;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Exception during file opening: " + ex.Message);
                return null;
            }
        }
        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.
        /// This parameter is typically used to configure the page.</param>
        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            //force Visual Studio intelliSense
            var img = previewImage as Image;
            double width, height;
            ResolutionProvider.GetScreenSizeInPixels(out height, out width);
            img.Source = await OpenBitmapFromFile(LockViewApplicationState.Instance.PersistFileName, (int)width, (int)height);
            previewStack.DataContext = null;//force rebind.
            previewStack.DataContext = LockViewApplicationState.Instance;
        }

        private void previewStack_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            NavigationService.Navigate(new Uri("/FontCAS.xaml", UriKind.Relative));

        }
    }
}
