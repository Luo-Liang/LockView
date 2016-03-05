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
using Microsoft.ApplicationInsights;
using Windows.ApplicationModel;
using Microsoft.Phone.Info;
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
            if (LockViewApplicationState.Instance.UserQuotaInDollars == double.MaxValue)
            {
                quotaStack.Visibility = Visibility.Collapsed;
                return;// do not bother.
            }
            var metaData = LockViewApplicationState.Instance.RequestMetadata;
            var providerMetaData = LockViewApplicationState.Instance.SelectedProviders.Select(o => o.GetMetaData());
            computePriceRun.Text = "$" + Pricing.ComputationPricePerHour + "/hr";
            trafficPriceRun.Text = "$" + Pricing.TrafficPricePerGB + "/GB";
            sizePerRequestRun.Text = (metaData.ImageBytesPerRequest + providerMetaData.Sum(o => o.BytePerRequest)) / 1024 + "KB";
            requestPerDayRun.Text = providerMetaData.Max(o => o.UpdatePerDay) + AppResources.Estimated;
            var DrainPerRequest = Pricing.CalculateDrainPerRequest(LockViewApplicationState.Instance.RequestMetadata, LockViewApplicationState.Instance.SelectedProviders.Select(o => o.GetMetaData()));
            _099PriceDaysRun.Text = Math.Ceiling(0.99 / (DrainPerRequest * providerMetaData.Max(o => o.UpdatePerDay))).ToString();
            days.Text = _099PriceDaysRun.Text;
            quotaPurchase.Content = AppResources.Purchase + days.Text + AppResources.DaysFor099;
            remainingQuota.Text = Math.Ceiling(LockViewApplicationState.Instance.UserQuotaInDollars / (DrainPerRequest * providerMetaData.Max(o => o.UpdatePerDay))).ToString();
            balanceRaw.Text = (((int)(LockViewApplicationState.Instance.UserQuotaInDollars * 1000)) / 1000.0).ToString();
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
            var LockScreenNothing2Do = !(contentStackPanel as StackPanel).Children.Any(o => o.Visibility == Visibility.Visible);
            if (LockScreenNothing2Do)
            {
                lockScreenNothingToDo.Visibility = Visibility.Visible;
            }
        }

        private async void button_Click(object sender, RoutedEventArgs e)
        {
            await LockScreenManager.RequestAccessAsync();
            await BackgroundExecutionManager.RequestAccessAsync();
            var requestStatus = BackgroundExecutionManager.GetAccessStatus();
            var allowedBg = requestStatus == BackgroundAccessStatus.AllowedWithAlwaysOnRealTimeConnectivity || requestStatus == BackgroundAccessStatus.AllowedMayUseActiveRealTimeConnectivity;
            setAsLockScreenProvider.Visibility = LockScreenManager.IsProvidedByCurrentApplication && allowedBg ? Visibility.Collapsed : Visibility.Visible;
            SaveBtn.IsEnabled = LockScreenManager.IsProvidedByCurrentApplication;
            var LockScreenNothing2Do = !(contentStackPanel as StackPanel).Children.Any(o => o.Visibility == Visibility.Visible);
            if (LockScreenNothing2Do)
            {
                lockScreenNothingToDo.Visibility = Visibility.Visible;
            }
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
            instance.DoNotDisturb = doNotDisturb.IsChecked.Value;
            instance.PreviewFormattingContract.SecondLineFont.FontSize = instance.PreviewFormattingContract.FirstLineFont.FontSize / 2;
            await instance.SaveState();
            var response = await client.Compose(LockViewApplicationState.Instance.SelectedContextContracts,
                LockViewApplicationState.Instance.PreviewFormattingContract,
                LockViewApplicationState.Instance.PreviewLayoutContract,
                LockViewApplicationState.Instance.RequestMetadata.PersistFileName);
            //restore fontSize
            progressRing.Visibility = Visibility.Collapsed;
            SaveBtn.Visibility = Visibility.Visible;
            //WriteableBitmap bitmap = new WriteableBitmap((int)width, (int)height);
            var jpegBytes = response.Image;
            var fileName = string.Format("{0}.jpeg", DateTime.Now.ToBinary());
            if (fileName.StartsWith("-")) fileName = fileName.TrimStart('-');
            BackgroundTaskHelper.SaveAndClearUsedComposedImage(jpegBytes, fileName);
            BackgroundTaskHelper.TrySetLockScreenImage(fileName, instance.RequestMetadata.RequestLanguage);
            BackgroundTaskHelper.TryUpdateTiles();
            //schedule the background task.
            BackgroundTaskHelper.RegisterOrRenewBackgroundAgent();
            if (System.Diagnostics.Debugger.IsAttached == false)
                SaveBtn.Visibility = Visibility.Collapsed;
            AllSetTitle.Text = AppResources.AllSetTitleText;
            //take a look what people use as their providers.
            var tc = new TelemetryClient();
            var property = new Dictionary<string, string>() { { "Hardware Id", BackgroundTaskHelper.GetDeviceId() } };
            var pkgVer = Package.Current.Id.Version;
            property["ver"] = $"{pkgVer.Major}.{pkgVer.Minor}.{pkgVer.Build}.{pkgVer.Revision}";
            property["quota"] = LockViewApplicationState.Instance.UserQuotaInDollars.ToString();
            property["hid"] = BackgroundTaskHelper.GetDeviceId();
            object modelobject = null;
            if (Microsoft.Phone.Info.DeviceExtendedProperties.TryGetValue("DeviceName", out modelobject))
            {
                property["dnm"] = modelobject as string;
            }
            object manufacturerobject;
            if (DeviceExtendedProperties.TryGetValue("DeviceManufacturer", out manufacturerobject))
            {
                property["dma"] = manufacturerobject as string;
            }
            for (int i = 0; i < instance.SelectedProviders.Length; i++)
            {
                property[$"Selection{i}"] = instance.SelectedProviders[i].GetType().Name;
            }
            tc.TrackEvent("Content Provider Comfirmation", property);
            var imgSrcProperty = new Dictionary<string, string>() { { "SourceName", instance.SelectedImageSource.ToString() }, { "Hardware Id", BackgroundTaskHelper.GetDeviceId() } };
            tc.TrackEvent("Image Provider Comfirmation", imgSrcProperty);

        }
        CustomMessageBox priceCalcMsgBx;
        private void priceCalculationLink_Click(object se1nder, RoutedEventArgs e)
        {
            priceCalcMsgBx.Show();
            priceCalculationLink.Visibility = Visibility.Collapsed;
        }

        private void quotaRunOut_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(AppResources.BalanceRunOutPromptText, AppResources.BalanceRunOutPromptTitle, MessageBoxButton.OK);
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
            MessageBox.Show(AppResources.NoUninstallationText, AppResources.NoUninstallation, MessageBoxButton.OK);
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
            MessageBox.Show(AppResources.WhileBalanceLastsText, AppResources.WhileBalanceLastsTitle, MessageBoxButton.OK);
        }

        private void redeem_Click
        (
            object sender,
            RoutedEventArgs e
        )
        {
            /* <toolkit:CustomMessageBox Title="REDEEM A CODE" LeftButtonContent="redeem" Height="341">
            <toolkit:CustomMessageBox.Content>
                <StackPanel>
                    <TextBlock Margin="12" TextWrapping="Wrap" Text="Enter your code below. It should look something like this: XXXXX-XXXXX-XXXXX-XXXXX."/>
                    <TextBox Text="" x:Name="codeBox"/>
                    <TextBlock Margin="12" Foreground="{StaticResource AccentBrush}" x:Name="redeemValidation" Text="This doesn't look like a valid code."/>
                </StackPanel>
            </toolkit:CustomMessageBox.Content>
        </toolkit:CustomMessageBox>*/
            //translate of the above XAML.
            StackPanel content = new StackPanel();
            content.Children.Add(new TextBlock() { Text = AppResources.RedeemACodeGuide, TextWrapping = TextWrapping.Wrap, Margin = new Thickness(12) });
            var keyBox = new TextBox();
            keyBox.TextChanged += KeyBox_TextChanged;
            keyBox.KeyUp += KeyBox_KeyUp;
            keyBox.KeyDown += KeyBox_KeyDown;
            content.Children.Add(keyBox);
            validationResultBox = new TextBlock() { Margin = new Thickness(12), Foreground = App.Current.Resources["LightAccentBrush"] as SolidColorBrush };
            content.Children.Add(validationResultBox);
            redeemMessageBox = new CustomMessageBox()
            {
                Title = AppResources.RedeemACode,
                Content = content
            };
            redeemMessageBox.LeftButtonContent = AppResources.Next;
            redeemMessageBox.IsLeftButtonEnabled = false;
            redeemMessageBox.Show();
            redeemMessageBox.Dismissed += RedeemMessageBox_Dismissed;

        }

        private void RedeemMessageBox_Dismissed(object sender, DismissedEventArgs e)
        {
            if (e.Result == CustomMessageBoxResult.LeftButton)
            {
                LockViewApplicationState.Instance.UserQuotaInDollars = double.MaxValue;
                quotaStack.Visibility = Visibility.Collapsed;
                var LockScreenNothing2Do = !(contentStackPanel as StackPanel).Children.Any(o => o.Visibility == Visibility.Visible);
                if (LockScreenNothing2Do)
                {
                    lockScreenNothingToDo.Visibility = Visibility.Visible;
                }
            }
        }

        private void KeyBox_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Subtract)
            {
                e.Handled = true;//cannot type this key
            }

        }

        private void KeyBox_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Back)
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

        CustomMessageBox redeemMessageBox;
        TextBlock validationResultBox;
        private void KeyBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var txtBx = sender as TextBox;
            if (txtBx.Text.Length == 29)
            {
                var raw = txtBx.Text.Where(o => o != '-').Select(o => (uint)o).Aggregate<uint, uint>(1, (current, accumulate) => current * accumulate);
                var isValid = raw % 3642621952 == 0 || raw % 637534208 == 0;
                if (isValid)
                {
                    validationResultBox.Text = AppResources.RedeemStatusOkay;
                    redeemMessageBox.IsLeftButtonEnabled = true;
                }
            }
            else
            {
                validationResultBox.Text = AppResources.RedeemStatusNotOkay;
            }
        }
    }
}