using System.Diagnostics;
using System.Windows;
using Microsoft.Phone.Scheduler;
using InfoViewApp.WP81;
using InfoViewApp.WP81.Tasks;
using System;
using Windows.Web.Http;
using Windows.Data.Json;
using System.Windows.Media.Imaging;
using System.Threading.Tasks;
using Windows.Foundation;
using InfoViewApp.WP81.InterestGathering;
using System.IO.IsolatedStorage;
using System.IO;
using Windows.Phone.System.UserProfile;
using Microsoft.Phone.Shell;
using System.Linq;
using Microsoft.Phone.Info;
using System.Net;
using Microsoft.ApplicationInsights;
using System.Collections.Generic;
using LockViewApp.WP81.Contracts;
using Windows.ApplicationModel;

namespace LockViewApp.WP81.BackgroundAgent
{
    public class ScheduledAgent : ScheduledTaskAgent
    {
        /// <remarks>
        /// ScheduledAgent constructor, initializes the UnhandledException handler
        /// </remarks>
        static ScheduledAgent()
        {
            // Subscribe to the managed exception handler
            Deployment.Current.Dispatcher.BeginInvoke(delegate
            {
                Application.Current.UnhandledException += UnhandledException;
            });
        }

        /// Code to execute on Unhandled Exceptions
        private static void UnhandledException(object sender, ApplicationUnhandledExceptionEventArgs e)
        {
            if (Debugger.IsAttached)
            {
                // An unhandled exception has occurred; break into the debugger
                Debugger.Break();
            }
            BackgroundTaskHelper.TrySetLockScreenImage("INVALID.INVALID", LockViewApplicationState.Instance.RequestMetadata.RequestLanguage);
        }
        TelemetryClient tc;
        Dictionary<string, string> telemetryProperty = new Dictionary<string, string>();
        /// <summary>
        /// Agent that runs a scheduled task
        /// </summary>
        /// <param name="task">
        /// The invoked task
        /// </param>
        /// <remarks>
        /// This method is called when a periodic or resource intensive task is invoked
        /// </remarks>
        protected async override void OnInvoke(ScheduledTask task)
        {
            var appInsightsAwaitTask = WindowsAppInitializer.InitializeAsync("8240d723 - c08b - 4d55 - 8c18 - 62cbe3c35157", WindowsCollectors.UnhandledException);
            await appInsightsAwaitTask;
            tc = new TelemetryClient();
            CollectDeviceInfo();
            if ((task.ExpirationTime - DateTime.Now).Days == 0)
            {
                //expiring soon.
                var toast = new ShellToast();
                toast.Title = AppResources.LockView;
                toast.Content = AppResources.AreYouStillThere;
                toast.NavigationUri = new Uri(BackgroundTaskHelper.LowBalanceNavId, UriKind.Relative);
                toast.Show();
            }
            try
            {
                await LaunchTask(task);
                if (LockViewApplicationState.Instance.UserQuotaInDollars < 0.05 && (DateTime.Now.DayOfYear - LockViewApplicationState.Instance.LastCheduleDOY) != 0)
                {
                    var dueTosat = new ShellToast();
                    dueTosat.Title = AppResources.LockView;
                    dueTosat.Content = AppResources.BalanceRunOut;
                    dueTosat.NavigationUri = new Uri(BackgroundTaskHelper.LowBalanceNavId, UriKind.Relative);
                    dueTosat.Show();
                }
                LockViewApplicationState.Instance.LastCheduleDOY = DateTime.Now.DayOfYear;
                await LockViewApplicationState.Instance.SaveState();
            }
            catch (Exception ex)
            {
                telemetryProperty["exception"] = ex.Message;
                //BackgroundTaskHelper.TrySetLockScreenImage("INVALID.INVALID", LockViewApplicationState.Instance.RequestMetadata.RequestLanguage);
            }
            finally
            {
                var pkgVer = Package.Current.Id.Version;
                telemetryProperty["ver"] = $"{pkgVer.Major}.{pkgVer.Minor}.{pkgVer.Build}.{pkgVer.Revision}";
                telemetryProperty["quota"] = LockViewApplicationState.Instance.UserQuotaInDollars.ToString();
                telemetryProperty["lrs"] = task.LastExitReason.ToString();
                telemetryProperty["mmu"] = $"{(int)(1000.0 * DeviceStatus.ApplicationPeakMemoryUsage / 1024.0 / 1024) / 1000}MB";
                telemetryProperty["ram"] = $"{(int)(DeviceStatus.DeviceTotalMemory / 1024.0 / 1024)}MB";
                //telemetryProperty["total allowed RAM"] = $"{DeviceStatus.ApplicationMemoryUsageLimit / 1024.0 / 1024}MB";
                tc.TrackEvent("Background Request", telemetryProperty);
                tc.Flush();
                NotifyComplete();
            }
//#if DEBUG
//            var memToast = new ShellToast();
//            memToast.Title = string.Format("{0} {1}", task.LastExitReason.ToString(), LockViewApplicationState.Instance.RequestMetadata.Phase);
//            memToast.Content = (DeviceStatus.ApplicationPeakMemoryUsage / 1024.0 / 1024).ToString();
//            memToast.Show();
//#endif

        }

        private void CollectDeviceInfo()
        {
            telemetryProperty["hid"] = BackgroundTaskHelper.GetDeviceId();
            object modelobject = null;
            if (Microsoft.Phone.Info.DeviceExtendedProperties.TryGetValue("DeviceName", out modelobject))
            {
                telemetryProperty["dnm"] = modelobject as string;
            }
            object manufacturerobject;
            if (DeviceExtendedProperties.TryGetValue("DeviceManufacturer", out manufacturerobject))
            {
                telemetryProperty["dma"] = manufacturerobject as string;
            }

        }

        protected async Task LaunchTask(ScheduledTask task)
        {
            var lastExecution = LockViewApplicationState.Instance.LastCheduleDOY;
            //make it very defensive.
            //check last run time.
            var instance = LockViewApplicationState.Instance;
            HttpClient client = new HttpClient();
            bool flag1 = false;
            if ((DateTime.Now.Hour <= 22 && DateTime.Now.Hour >= 8) || !instance.DoNotDisturb)
            {
                //can disturb the user
                //use this client to send request.
                telemetryProperty["intime"] = "yes";
                flag1 = await AcquireContentUpdateIfNecessary(client);
                telemetryProperty["should update"] = flag1.ToString();
            }
            //does the user have any quota executing that?
            if (flag1 || lastExecution < DateTime.Now.DayOfYear)
            {
                ImageRequestOverride imgReqOverride = await instance.CreateRequestOverride();
                telemetryProperty["request build ok"] = "Yes";
                //if yes, execute that if (1) the image has changed OR the content has changed.
                await UpdateLockScreenTilesIfPossible(client, task, imgReqOverride);

            }
        }

        private async Task UpdateLockScreenTilesIfPossible(HttpClient client, ScheduledTask task, ImageRequestOverride possibleOverride)
        {
            var instance = LockViewApplicationState.Instance;
            var drainPerReq = Pricing.CalculateDrainPerRequest(instance.RequestMetadata, instance.SelectedProviders.Select(o => o.GetMetaData()));
            if (instance.UserQuotaInDollars >= 0.005 || LockViewApplicationState.Instance.LastCheduleDOY != DateTime.Now.DayOfYear)
            {
                telemetryProperty["Update Accepted"] = "Yes";
                CloudImageCompositorClient cloudClient = new CloudImageCompositorClient(client);
                ImageCompositionResponse compositionResponse = null;
                if (possibleOverride == null)
                    compositionResponse = await cloudClient.Compose(instance.SelectedContextContracts, instance.PreviewFormattingContract, instance.PreviewLayoutContract, instance.RequestMetadata.PersistFileName);
                else
                    compositionResponse = await cloudClient.ComposeLite(instance.SelectedContextContracts, instance.PreviewFormattingContract, instance.PreviewLayoutContract, possibleOverride);
                telemetryProperty["Image Update Successful"] = "Yes";
                var fileName = instance.SelectedContextContracts.GenerateImgFileName();
                telemetryProperty["fn"] = fileName;
                var jpegBytes = compositionResponse.Image;
                BackgroundTaskHelper.SaveAndClearUsedComposedImage(jpegBytes, fileName);
                telemetryProperty["File Saved"] = "Yes";
                if (possibleOverride != null && possibleOverride.Arguments == "lq") return;//don't update on money penny
                telemetryProperty["swppr"] = BackgroundTaskHelper.TrySetLockScreenImage(fileName, instance.RequestMetadata.RequestLanguage);
                //update tile if necessary.
                BackgroundTaskHelper.TryUpdateTiles();
                //drain the user's balance.
                if (instance.UserQuotaInDollars != double.MaxValue) //<--- free users don't get deducted.
                    instance.UserQuotaInDollars -= drainPerReq;
                //instance.UserQuotaInDollars = instance.UserQuotaInDollars < 0 ? 0 : instance.UserQuotaInDollars;
            }
        }

        private async Task<bool> AcquireContentUpdateIfNecessary(HttpClient client)
        {
            var flag1 = false;
            var instance = LockViewApplicationState.Instance;
            foreach (var provider in instance.SelectedProviders)
            {
                provider.Client = client;//<--- use the same client to save memory.
            }
            var contents = await Task.WhenAll(instance.SelectedProviders.Select(async (o, i) => await o.RequestContent(instance.SelectedInterests[i]))); //<--- forcing eval.
            if (instance.SelectedContextContracts.Select((o, i) => !o.Equals(contents[i])).Count(o => o) > 0 || instance.SelectedImageSource == ImageSource.LiveEarth)
            {
                //are we getting the same update?
                flag1 = true;
                //set flag1 to true -- approve.
                for (int i = 0; i < instance.SelectedContextContracts.Length; i++)
                {
                    instance.SelectedContextContracts[i].CopyFromInterestContent(contents[i]);
                }
            }
            return flag1;
        }
    }
}