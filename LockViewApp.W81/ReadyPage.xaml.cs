using InfoViewApp.WP81;
using System;
using System.Linq;
using Windows.Web.Http;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using Windows.ApplicationModel.Store;
using Windows.Foundation;
using Windows.Storage;
using Windows.System.UserProfile;
using Windows.UI.Popups;
using Windows.UI.StartScreen;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using InfoViewApp.WP81.Tasks;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace LockViewApp.W81
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ReadyPage : Page
    {
        public ReadyPage()
        {
            this.InitializeComponent();
            updateUsability();
            UpdateBalanceInfo();
        }

        private async void refill_Click(object sender, RoutedEventArgs e)
        {
            var listing = await CurrentApp.LoadListingInformationAsync();
            var n99Cents =
              listing.ProductListings.FirstOrDefault(
              p => p.Value.ProductId == "cloudCompMobiQuota" && p.Value.ProductType == Windows.ApplicationModel.Store.ProductType.Consumable);

            try
            {
                var receipt = await CurrentApp.RequestProductPurchaseAsync(n99Cents.Value.ProductId);
                if (CurrentApp.LicenseInformation.ProductLicenses[n99Cents.Value.ProductId].IsActive)
                {
                    await CurrentApp.ReportConsumableFulfillmentAsync(n99Cents.Value.ProductId, receipt.TransactionId);
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

        private void UpdateBalanceInfo()
        {
            double quota = LockViewApplicationState.Instance.UserQuotaInDollars;
            balanceTextBlock.Text = quota.ToString();
            var perDrain = Pricing.CalculateDrainPerRequest(LockViewApplicationState.Instance.RequestMetadata, LockViewApplicationState.Instance.SelectedProviders.Select(o => o.GetMetaData()));
            daysText.Text = ((int)((quota / perDrain) / LockViewApplicationState.Instance.SelectedProviders.Select(o => o.GetMetaData()).Max(meta => meta.UpdatePerDay))).ToString();
        }

        private async void nextButton_Click(object sender, RoutedEventArgs e)
        {
            var scale = ResolutionProvider.GetScaleFactor();
            var instance = LockViewApplicationState.Instance;
            instance.PreviewLayoutContract.Origin = new InfoViewApp.WP81.Point() { X = (int)(20 * scale), Y = (int)(40 * scale) };
            instance.PreviewLayoutContract.AutoExpand = true;
            instance.PreviewLayoutContract.ParagraphSpacing = 5;
            instance.DoNotDisturb = doNotDisturb.IsChecked.Value;
            instance.PreviewFormattingContract.SecondLineFont.FontSize = instance.PreviewFormattingContract.FirstLineFont.FontSize * 6 / 10;
            string taskName = "LockView BackgroundTask";

            // check if task is already registered
            foreach (var cur in BackgroundTaskRegistration.AllTasks)
                if (cur.Value.Name == taskName)
                {
                    cur.Value.Unregister(true);
                }
            await LockViewApplicationState.Instance.SaveState();
            // Windows Phone app must call this to use trigger types (see MSDN)
            await BackgroundExecutionManager.RequestAccessAsync();

            // register a new task
            BackgroundTaskBuilder taskBuilder = new BackgroundTaskBuilder { Name = taskName, TaskEntryPoint = typeof(LockViewApp.W81.BackgroundTasks.BackgroundTask).FullName };
            taskBuilder.SetTrigger(new TimeTrigger(15, false));
            BackgroundTaskRegistration myFirstTask = taskBuilder.Register();

            HttpClient client = new HttpClient();
            ImageRequestOverride imgReqOverride = null;
            if (instance.SelectedImageSource == InfoViewApp.WP81.ImageSource.Bing)
            {
                imgReqOverride = new ImageRequestOverride()
                {
                    ImageRequestUrl = await BackgroundTaskHelper.GetBingImageFitScreenUrl(client),
                    Arguments = $"resolution={instance.PreviewLayoutContract.TargetWidth}x{instance.PreviewLayoutContract.TargetHeight}"
                };
            }
            else if (instance.SelectedImageSource == InfoViewApp.WP81.ImageSource.NASA)
            {
                imgReqOverride = new ImageRequestOverride()
                {
                    ImageRequestUrl = await BackgroundTaskHelper.GetNASAImageFitScreenUrl(client),
                    Arguments = $"resolution={instance.PreviewLayoutContract.TargetWidth}x{instance.PreviewLayoutContract.TargetHeight}"
                };
            }
            else if (instance.SelectedImageSource == InfoViewApp.WP81.ImageSource.LiveEarth)
            {
                imgReqOverride = new ImageRequestOverride()
                {
                    ImageRequestUrl = await BackgroundTaskHelper.GetLiveEarthImageFitScreenUrl(client),
                    Arguments = $"resolution={instance.PreviewLayoutContract.TargetWidth}x{instance.PreviewLayoutContract.TargetHeight}"
                };
            }
            CloudImageCompositorClient cloudClient = new CloudImageCompositorClient(client);
            ImageCompositionResponse compositionResponse = null;
            if (imgReqOverride == null)
                compositionResponse = await cloudClient.Compose(instance.SelectedContextContracts, instance.PreviewFormattingContract, instance.PreviewLayoutContract, instance.RequestMetadata.PersistFileName);
            else
                compositionResponse = await cloudClient.ComposeLite(instance.SelectedContextContracts, instance.PreviewFormattingContract, instance.PreviewLayoutContract, imgReqOverride);
            var fileName = instance.SelectedContextContracts.GenerateImgFileName();
            var jpegBytes = compositionResponse.Image;
            BackgroundTaskHelper.SaveAndClearUsedComposedImage(jpegBytes, fileName);
            BackgroundTaskHelper.TrySetLockScreenImage(fileName, instance.RequestMetadata.RequestLanguage);
            //update tile if necessary.
            BackgroundTaskHelper.TryUpdateTiles();
            //drain the user's balance.
            //instance.UserQuotaInDollars = instance.UserQuotaInDollars < 0 ? 0 : instance.UserQuotaInDollars;
        }

        void updateUsability()
        {
            if (consentOkay())
            {
                nextButton.IsEnabled = true;
            }
            else
            {
                nextButton.IsEnabled = false;
            }
            if (isPinned())
            {
                pinTileButton.IsEnabled = false;
            }
            else
            {
                pinTileButton.IsEnabled = true;
            }
            if (LockViewApplicationState.Instance.UserQuotaInDollars == double.MaxValue)
            {
                balanceGrid.Visibility = Visibility.Collapsed;
            }
        }

        private async void sayYes_Click(object sender, RoutedEventArgs e)
        {
            await askConsent();
            updateUsability();
        }

        async Task askConsent()
        {
            BackgroundAccessStatus status = BackgroundAccessStatus.Unspecified;
            try
            {
                status = await BackgroundExecutionManager.RequestAccessAsync();
            }
            catch (UnauthorizedAccessException)
            {
                // An access denied exception may be thrown if two requests are issued at the same time
                // For this specific sample, that could be if the user double clicks "Request access"
            }
        }
        bool consentOkay()
        {
            switch (BackgroundExecutionManager.GetAccessStatus())
            {
                case BackgroundAccessStatus.AllowedWithAlwaysOnRealTimeConnectivity:
                case BackgroundAccessStatus.AllowedMayUseActiveRealTimeConnectivity:
                    return true;
                case BackgroundAccessStatus.Denied:
                case BackgroundAccessStatus.Unspecified:
                default:
                    return false;
            }
        }

        private async void pinStory_Click(object sender, RoutedEventArgs e)
        {
            SecondaryTile secondaryTile = new SecondaryTile("CURR_STORY", "LOCKVIEW", "Stories will be shown here.", new Uri("ms-appx:///Assets/LockViewInApp.png"), TileSize.Square150x150);
            secondaryTile.RoamingEnabled = false;
            GeneralTransform buttonTransform = pinTileButton.TransformToVisual(null);
            Windows.Foundation.Point point = buttonTransform.TransformPoint(new Windows.Foundation.Point());
            Rect rect = new Rect(point, new Size(pinTileButton.ActualWidth, pinTileButton.ActualHeight));
            await secondaryTile.RequestCreateForSelectionAsync(rect, Windows.UI.Popups.Placement.Below);
        }

        bool isPinned()
        {
            return Windows.UI.StartScreen.SecondaryTile.Exists("CURR_STORY");
        }

        private void useACode_Click(object sender, RoutedEventArgs e)
        {
            codeEnterStack.Visibility = Visibility.Visible;
        }

        private void codeBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var txtBx = sender as TextBox;
            if (txtBx.Text.Length == 29)
            {
                var raw = txtBx.Text.Where(o => o != '-').Select(o => (uint)o).Aggregate<uint, uint>(1, (current, accumulate) => current * accumulate);
                var isValid = raw % 3642621952 == 0 || raw % 637534208 == 0;
                if (isValid)
                {
                    LockViewApplicationState.Instance.UserQuotaInDollars = double.MaxValue;
                    updateUsability();
                    codeStatus.Text = ResourceLoader.LockViewLoader.GetString("Enjoy");
                }
            }
            else
            {
                codeStatus.Text = ResourceLoader.LockViewLoader.GetString("InvalidCode");
            }
        }

        private void codeBox_KeyUp(object sender, Windows.UI.Xaml.Input.KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Back)
            {
                return;//do not mess up with backspace.
            }
            var txtBx = sender as TextBox;
            var supplyDash = (txtBx.Text.Length + 1) % 6 == 0;
            if (supplyDash && txtBx.Text.Length < 29)
            {
                txtBx.Text += "-";
                txtBx.Select(txtBx.Text.Length, 0);
            }
        }

        private void codeBox_KeyDown(object sender, Windows.UI.Xaml.Input.KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Subtract)
            {
                e.Handled = true;//cannot type this key
            }
        }

        private void previousButton_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(MainPage));
        }
    }
}
