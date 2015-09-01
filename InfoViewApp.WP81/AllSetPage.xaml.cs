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
using System.IO.IsolatedStorage;
using System.Windows.Media;
using InfoViewApp.WP81.Tasks;
using Windows.ApplicationModel.Background;
using Microsoft.Phone.Scheduler;
using InfoViewApp.WP81.Resources;
#if DEBUG
using MockIAPLib;
using Store = MockIAPLib;
#else
using Windows.ApplicationModel.Store;
#endif

namespace InfoViewApp.WP81
{
    public partial class AllSetPage : PhoneApplicationPage
    {
        public AllSetPage()
        {
            InitializeComponent();
            priceCalcMsgBx = Resources["priceCalcMsgBx"] as CustomMessageBox;
            Resources.Remove("priceCalcMsgBx");
            UpdateBalanceInfo();
            SetupMockIAP();
        }

        private void UpdateBalanceInfo()
        {
            var metaData = LockViewApplicationState.Instance.RequestMetadata;
            var providerMetaData = LockViewApplicationState.Instance.SelectedProvider.GetMetaData();
            computePriceRun.Text = "$" + Pricing.ComputationPricePerHour + "/hr";
            trafficPriceRun.Text = "$" + Pricing.TrafficPricePerGB + "/GB";
            sizePerRequestRun.Text = (metaData.ImageBytesPerRequest + providerMetaData.BytePerRequest) / 1024 + "KB";
            requestPerDayRun.Text = providerMetaData.UpdatePerDay + AppResources.Estimated;
            var DrainPerRequest = Pricing.CalculateDrainPerRequest(LockViewApplicationState.Instance.RequestMetadata, LockViewApplicationState.Instance.SelectedProvider.GetMetaData());
            _099PriceDaysRun.Text = Math.Ceiling(0.99 / (DrainPerRequest * providerMetaData.UpdatePerDay)).ToString();
            days.Text = _099PriceDaysRun.Text;
            quotaPurchase.Content = AppResources.Purchase + days.Text + AppResources.DaysFor099;
            remainingQuota.Text = Math.Ceiling(LockViewApplicationState.Instance.UserQuotaInDollars / (DrainPerRequest * providerMetaData.UpdatePerDay)).ToString();
        }

        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (e.Uri == new Uri(BackgroundTaskHelper.LowBalanceNavId, UriKind.Relative))
            {
                BackgroundTaskHelper.RegisterOrRenewBackgroundAgent();
            }
            var requestStatus = BackgroundExecutionManager.GetAccessStatus();
            var allowedBg = requestStatus == BackgroundAccessStatus.AllowedWithAlwaysOnRealTimeConnectivity || requestStatus == BackgroundAccessStatus.AllowedMayUseActiveRealTimeConnectivity;
            setAsLockScreenProvider.Visibility = LockScreenManager.IsProvidedByCurrentApplication && allowedBg ? Visibility.Collapsed : Visibility.Visible;
            var isPinned = ShellTile.ActiveTiles.Any<ShellTile>(st => st.NavigationUri == new Uri(BackgroundTaskHelper.PinnedHeadlineNavId, UriKind.Relative));
            PinFrontStory.Visibility = isPinned ? Visibility.Collapsed : Visibility.Visible;
            SaveBtn.IsEnabled = LockScreenManager.IsProvidedByCurrentApplication;
            
        }

        private async void button_Click(object sender, RoutedEventArgs e)
        {
            await LockScreenManager.RequestAccessAsync();
            await BackgroundExecutionManager.RequestAccessAsync();
            var requestStatus = BackgroundExecutionManager.GetAccessStatus();
            var allowedBg = requestStatus == BackgroundAccessStatus.AllowedWithAlwaysOnRealTimeConnectivity || requestStatus == BackgroundAccessStatus.AllowedMayUseActiveRealTimeConnectivity;
            setAsLockScreenProvider.Visibility = LockScreenManager.IsProvidedByCurrentApplication && allowedBg ? Visibility.Collapsed : Visibility.Visible;
            SaveBtn.IsEnabled = LockScreenManager.IsProvidedByCurrentApplication;
        }

        private void shortcutButton_Click(object sender, RoutedEventArgs e)
        {
            ShellTile.Create(new Uri(BackgroundTaskHelper.PinnedHeadlineNavId, UriKind.Relative), new StandardTileData() { }, false);
            var isPinned = ShellTile.ActiveTiles.Any<ShellTile>(st => st.NavigationUri == new Uri(BackgroundTaskHelper.PinnedHeadlineNavId, UriKind.Relative));
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
            //save state  //launch task   //close app.
            progressRing.Visibility = Visibility.Visible;
            SaveBtn.Visibility = Visibility.Collapsed;
            var scale = ResolutionProvider.GetScaleFactor();
            var instance = LockViewApplicationState.Instance;
            instance.PreviewLayoutContract.Origin = new Point() { X = (int)(20 * scale), Y = (int)(40 * scale) };
            instance.PreviewLayoutContract.AutoExpand = true;
            instance.PreviewLayoutContract.ParagraphSpacing = 5;
            double height, width;
            ResolutionProvider.GetScreenSizeInPixels(out height, out width);
            instance.PreviewLayoutContract.TargetHeight = (int)height;
            instance.PreviewLayoutContract.TargetWidth = (int)width;
            Tasks.CloudImageCompositorClient client = new Tasks.CloudImageCompositorClient();
            await instance.SaveState();
            var response = await client.Compose(LockViewApplicationState.Instance.PreviewContextContract,
                LockViewApplicationState.Instance.PreviewFormattingContract,
                LockViewApplicationState.Instance.PreviewLayoutContract,
                LockViewApplicationState.Instance.RequestMetadata.PersistFileName);
            //restore fontSize
            progressRing.Visibility = Visibility.Collapsed;
            SaveBtn.Visibility = Visibility.Visible;
            //WriteableBitmap bitmap = new WriteableBitmap((int)width, (int)height);
            var jpegBytes = response.Image;
            var fileName = string.Format("{0}.jpeg", DateTime.Now.ToBinary());
            BackgroundTaskHelper.SaveAndClearUsedComposedImage(jpegBytes, fileName);
            BackgroundTaskHelper.TrySetLockScreenImage(fileName);
            BackgroundTaskHelper.TryUpdateTiles();
            //schedule the background task.
            BackgroundTaskHelper.RegisterOrRenewBackgroundAgent();
        }
        CustomMessageBox priceCalcMsgBx;
        private void priceCalculationLink_Click(object se1nder, RoutedEventArgs e)
        {
            priceCalcMsgBx.Show();
            priceCalculationLink.Visibility = Visibility.Collapsed;
        }

        private void quotaRunOut_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(AppResources.NotThereYetText, AppResources.NotThereYetTitle, MessageBoxButton.OK);
        }

        private void SetupMockIAP()
        {
#if DEBUG
            try
            {
                MockIAP.Init();
                MockIAP.RunInMockMode(true);
                MockIAP.SetListingInformation(1, "en-us", "A description", "1", "LockView");

                // Add some more items manually.
                var p = new ProductListing()
                {
                    Name = "FAKE NAME",
                    ImageUri = new Uri("ms-appx:///LockViewInApp.png", UriKind.Absolute),
                    ProductId = "cloudCompMobiQuota",
                    ProductType = Windows.ApplicationModel.Store.ProductType.Consumable,
                    Keywords = new string[] { "FAKE" },
                    Description = "FAKE",
                    FormattedPrice = "1.0",
                    Tag = string.Empty
                };
                MockIAP.AddProductListing("FAKE", p);
            }
            catch { }
#endif
        }


        private async void quotaPurchase_Click(object sender, RoutedEventArgs e)
        {
            if(LockViewApplicationState.Instance.UserQuotaInDollars >= 0.05)
            {
                MessageBox.Show(AppResources.NotThereYetText, AppResources.NotThereYetTitle, MessageBoxButton.OK);
                return;
            }
            var listing = await CurrentApp.LoadListingInformationAsync();
            var n99Cents =
              listing.ProductListings.FirstOrDefault(
              p => p.Value.ProductId == "cloudCompMobiQuota" && p.Value.ProductType == Windows.ApplicationModel.Store.ProductType.Consumable);

            try
            {
#if DEBUG
                var receipt = await CurrentApp.RequestProductPurchaseAsync(n99Cents.Value.ProductId, true);
#else
                var receipt = await CurrentApp.RequestProductPurchaseAsync(n99Cents.Value.ProductId);
#endif

                if (CurrentApp.LicenseInformation.ProductLicenses[n99Cents.Value.ProductId].IsActive)
                {
#if DEBUG
                    CurrentApp.ReportProductFulfillment(n99Cents.Value.ProductId);
#else
                    await CurrentApp.ReportConsumableFulfillmentAsync(n99Cents.Value.ProductId, receipt.TransactionId);
#endif
                    LockViewApplicationState.Instance.UserQuotaInDollars += 0.99;
                    await LockViewApplicationState.Instance.SaveState();
                    UpdateBalanceInfo();
                    //"Bought 50 Points " + i++ + " times for a total of " + m_pointCount + "!";
                }
            }
            catch (Exception ex)
            {

            }
        }

        private void dontwattopayLink_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(AppResources.BalanceRunOutPromptText, AppResources.BalanceRunOutPromptTitle, MessageBoxButton.OK);
        }
    }
}