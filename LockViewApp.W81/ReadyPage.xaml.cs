using InfoViewApp.WP81;
using LockViewApp.W81.BackgroundTasks;
using System;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using Windows.ApplicationModel.Store;
using Windows.Foundation;
using Windows.Storage;
using Windows.System.UserProfile;
using Windows.UI.StartScreen;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

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
            string taskName = "LockView BackgroundTask";

            // check if task is already registered
            foreach (var cur in BackgroundTaskRegistration.AllTasks)
                if (cur.Value.Name == taskName)
                {
                    cur.Value.Unregister(true);
                }

            // Windows Phone app must call this to use trigger types (see MSDN)
            await BackgroundExecutionManager.RequestAccessAsync();

            // register a new task
            BackgroundTaskBuilder taskBuilder = new BackgroundTaskBuilder { Name = taskName, TaskEntryPoint = typeof(BackgroundTask).FullName };
            taskBuilder.SetTrigger(new TimeTrigger(15, false));
            BackgroundTaskRegistration myFirstTask = taskBuilder.Register();

          
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
        }

        private async void sayYes_Click(object sender, RoutedEventArgs e)
        {
            await askConsent();
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
    }
}
