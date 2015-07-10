﻿using System;
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
using System.IO.IsolatedStorage;
using System.Windows.Media;

namespace InfoViewApp.WP81
{
    public partial class AllSetPage : PhoneApplicationPage
    {
        const string PinnedHeadlineNavId = "/MainPage.xaml?NavId=headLine";
        public AllSetPage()
        {
            InitializeComponent();
            var metaData = LockViewApplicationState.Instance.RequestMetadata;
            var providerMetaData = LockViewApplicationState.Instance.SelectedProvider.GetMetaData();
            computePriceRun.Text = "$" + Pricing.ComputationPricePerHour + "/hr";
            trafficPriceRun.Text = "$" + Pricing.TrafficPricePerGB + "/GB";
            sizePerRequestRun.Text = (metaData.ImageBytesPerRequest + providerMetaData.BytePerRequest) / 1024 + "KB";
            requestPerDayRun.Text = providerMetaData.UpdatePerDay + " (Estimated)";
            var DrainPerRequest = Pricing.CalculateDrainPerRequest(LockViewApplicationState.Instance.RequestMetadata, LockViewApplicationState.Instance.SelectedProvider.GetMetaData());
            _099PriceDaysRun.Text = Math.Ceiling(0.99 / (DrainPerRequest * providerMetaData.UpdatePerDay)).ToString();
            priceCalcMsgBx = Resources["priceCalcMsgBx"] as CustomMessageBox;
            Resources.Remove("priceCalcMsgBx");
            days.Text = _099PriceDaysRun.Text;
            quotaPurchase.Content = "purchase " + days.Text + " days for $0.99";
            remainingQuota.Text = Math.Ceiling(LockViewApplicationState.Instance.UserQuotaInDollars / (DrainPerRequest * providerMetaData.UpdatePerDay)).ToString();
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
            var scale = ResolutionProvider.GetScaleFactor();
            var instance = LockViewApplicationState.Instance;
            instance.PreviewFormattingContract.FirstLineFont.FontSize = (int)(instance.PreviewFormattingContract.FirstLineFont.FontSize * scale);
            instance.PreviewFormattingContract.SecondLineFont.FontSize = (int)(instance.PreviewFormattingContract.SecondLineFont.FontSize * scale);
            instance.PreviewFormattingContract.TitleFont.FontSize = (int)(instance.PreviewFormattingContract.TitleFont.FontSize * scale);
            instance.PreviewLayoutContract.Origin = new Point() { X = (int)(20 * scale), Y = (int)(20 * scale) };
            instance.PreviewLayoutContract.AutoExpand = true;
            instance.PreviewLayoutContract.ParagraphSpacing = (int)(10 * scale);
            double height, width;
            ResolutionProvider.GetScreenSizeInPixels(out height, out width);
            instance.PreviewLayoutContract.TargetHeight = (int)height;
            instance.PreviewLayoutContract.TargetWidth = (int)width;
            Tasks.CloudImageCompositorClient client = new Tasks.CloudImageCompositorClient();
            var response = await client.Compose(LockViewApplicationState.Instance.PreviewContextContract,
                LockViewApplicationState.Instance.PreviewFormattingContract,
                LockViewApplicationState.Instance.PreviewLayoutContract,
                LockViewApplicationState.Instance.PersistFileName);
            //WriteableBitmap bitmap = new WriteableBitmap((int)width, (int)height);
            var jpegBytes = Convert.FromBase64String(response.Image);
            var fileName = "wall.jpeg";
            using (IsolatedStorageFile myIsolatedStorage = IsolatedStorageFile.GetUserStoreForApplication())
            {
                if (myIsolatedStorage.FileExists(fileName))
                {
                    myIsolatedStorage.DeleteFile(fileName);
                }
                var file = myIsolatedStorage.CreateFile(fileName);
                using (var fs = file)
                {
                    await Task.Run(() => fs.Write(jpegBytes, 0, jpegBytes.Length));
                }
            }
            progressRing.Visibility = Visibility.Collapsed;
            SaveBtn.Visibility = Visibility.Visible;
            try
            {
                LockScreen.SetImageUri(new Uri("ms-appx:///LockView.png", UriKind.Absolute));
            }
            catch (Exception ex)
            {
            }
            LockScreen.SetImageUri(new Uri("ms-appdata:///local/wall.jpeg", UriKind.Absolute));
            var isPinned = ShellTile.ActiveTiles.Any<ShellTile>(st => st.NavigationUri == new Uri(PinnedHeadlineNavId, UriKind.Relative));
            if (isPinned)
            {
                var tile = ShellTile.ActiveTiles.First<ShellTile>(st => st.NavigationUri == new Uri(PinnedHeadlineNavId, UriKind.Relative));
                var context = LockViewApplicationState.Instance.PreviewContextContract;
                var standardTile = new StandardTileData() { Title = "LockView", BackTitle = context.Title, BackContent = context.FirstLine };
                if (context.ExtendedUri != null)
                {
                    standardTile.BackgroundImage = new Uri(context.ExtendedUri, UriKind.Absolute);
                }
                tile.Update(standardTile);
            }
        }
        CustomMessageBox priceCalcMsgBx;
        private void priceCalculationLink_Click(object se1nder, RoutedEventArgs e)
        {
            priceCalcMsgBx.Show();
            priceCalculationLink.Visibility = Visibility.Collapsed;
        }

        private void quotaRunOut_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("If your balance runs out, you will still receive updates, but at a significantly lowered rate. We'll remind you if you're out of balance soon.", "WHAT IF QUOTA RUNS OUT...", MessageBoxButton.OK);
        }

        private void quotaPurchase_Click(object sender, RoutedEventArgs e)
        {

        }

        private void dontwattopayLink_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("While your balance lasts, you receive full feature of the application. As an encouragement to support our work, you will have to reinstall this app to get some new balance every few days.", "I'M NOT A FAN OF PAID APP...", MessageBoxButton.OK);
        }
    }
}