using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Windows.Phone.System.UserProfile;
using System.Windows.Media.Imaging;
using Windows.Storage;
using System.Threading.Tasks;
using System.IO;
using System.Diagnostics;

namespace InfoViewApp.WP81
{
    public partial class AllSetPage : PhoneApplicationPage
    {
        const string PinnedHeadlineNavId = "/MainPage.xaml?NavId=headLine";
        public AllSetPage()
        {
            InitializeComponent();
        }
        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            setAsLockScreenProvider.Visibility = LockScreenManager.IsProvidedByCurrentApplication ? Visibility.Collapsed : Visibility.Visible;
            var isPinned = ShellTile.ActiveTiles.Any<ShellTile>(st => st.NavigationUri == new Uri(PinnedHeadlineNavId, UriKind.Relative));
            PinFrontStory.Visibility = isPinned ? Visibility.Collapsed : Visibility.Visible;
            button.IsEnabled = !LockScreenManager.IsProvidedByCurrentApplication;
            double height, width;
            ResolutionProvider.GetScreenSizeInPixels(out height, out width);
            image.Source = await OpenBitmapFromFile(LockViewApplicationState.Instance.PersistFileName, (int)width, (int)height);

        }

        private async void button_Click(object sender, RoutedEventArgs e)
        {
            await LockScreenManager.RequestAccessAsync();
            setAsLockScreenProvider.Visibility = LockScreenManager.IsProvidedByCurrentApplication ? Visibility.Collapsed : Visibility.Visible;
        }

        private void shortcutButton_Click(object sender, RoutedEventArgs e)
        {
            ShellTile.Create(new Uri(PinnedHeadlineNavId, UriKind.Relative), new StandardTileData() { }, false);
            var isPinned = ShellTile.ActiveTiles.Any<ShellTile>(st => st.NavigationUri == new Uri(PinnedHeadlineNavId, UriKind.Relative));
            PinFrontStory.Visibility = isPinned ? Visibility.Collapsed : Visibility.Visible;
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
        private async void SaveBtn_Click(object sender, RoutedEventArgs e)
        {
            while (NavigationService.CanGoBack)
                NavigationService.RemoveBackEntry();
            //save state
            //launch task
            //close app.
            progressRing.Visibility = Visibility.Visible;
            SaveBtn.Visibility = Visibility.Collapsed;
            await LockViewApplicationState.Instance.SaveState();
            Tasks.CloudImageCompositorClient client = new Tasks.CloudImageCompositorClient();
            var response = await client.Compose(LockViewApplicationState.Instance.PreviewContextContract,
                LockViewApplicationState.Instance.PreviewFormattingContract,
                LockViewApplicationState.Instance.PreviewLayoutContract,
                LockViewApplicationState.Instance.PersistFileName);
            double height, width;
            ResolutionProvider.GetScreenSizeInPixels(out height,out width);
            //WriteableBitmap bitmap = new WriteableBitmap((int)width, (int)height);
            var jpegBytes = Convert.FromBase64String(response.Image);
            var file = await ApplicationData.Current.LocalFolder.CreateFileAsync("wall.jpeg", CreationCollisionOption.ReplaceExisting);
            using (var fs = await file.OpenStreamForWriteAsync())
                await Task.Run(() => fs.WriteAsync(jpegBytes, 0, jpegBytes.Length));
            image.Source =await OpenBitmapFromFile("wall.jpeg", (int)width, (int)height);
            progressRing.Visibility = Visibility.Collapsed;
            SaveBtn.Visibility = Visibility.Visible;
        }
    }
}