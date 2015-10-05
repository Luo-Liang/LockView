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
            if((task.ExpirationTime - DateTime.Now).Days == 0)
            {
                //expiring soon.
                var toast = new ShellToast();
                toast.Title = AppResources.LockView;
                toast.Content = AppResources.AreYouStillThere;
                toast.NavigationUri = new Uri(BackgroundTaskHelper.LowBalanceNavId, UriKind.Relative);
                toast.Show();
            }
            telemetryProperty["Hardware Id"] = BackgroundTaskHelper.GetDeviceId();
            try
            {
                if (DeviceStatus.DeviceTotalMemory >> 28 < 1)
                {
                    //low ram device.
                    await LaunchLowRAMTask(task);
                }
                else
                {
                    await LaunchTask(task);
                }
                await LockViewApplicationState.Instance.SaveState();
                telemetryProperty["exception"] = "null";
            }
            catch (Exception ex)
            {
                telemetryProperty["exception"] = ex.GetType().Name;
                BackgroundTaskHelper.TrySetLockScreenImage("INVALID.INVALID", LockViewApplicationState.Instance.RequestMetadata.RequestLanguage);
            }
            finally
            {
                telemetryProperty["last run status"] = task.LastExitReason.ToString();
                telemetryProperty["maximum memory usage"] = $"{DeviceStatus.ApplicationPeakMemoryUsage / 1024.0 / 1024}MB";
                telemetryProperty["total available RAM"] = $"{DeviceStatus.DeviceTotalMemory / 1024.0 / 1024}MB";
                //telemetryProperty["total allowed RAM"] = $"{DeviceStatus.ApplicationMemoryUsageLimit / 1024.0 / 1024}MB";
                tc.TrackEvent("User Background Request", telemetryProperty);
                tc.Flush();
                NotifyComplete();
            }
#if DEBUG
            var toast = new ShellToast();
            toast.Title = string.Format("{0} {1}", task.LastExitReason.ToString(), LockViewApplicationState.Instance.RequestMetadata.Phase);
            toast.Content = (DeviceStatus.ApplicationPeakMemoryUsage / 1024.0 / 1024).ToString();
            toast.Show();
#endif

        }
        protected async Task LaunchLowRAMTask(ScheduledTask task)
        {
            var instance = LockViewApplicationState.Instance;
            HttpClient client = new HttpClient();
            if (instance.RequestMetadata.Phase == LockViewRequestMetadata.TaskPhase.Tick)
            {
#if !DEBUG
                if (DateTime.Now.Hour <= 22 && DateTime.Now.Hour >= 8)
#endif
                {
                    var flag1 = false;
                    //can disturb the user
                    //use this client to send request.
                    flag1 = await AcquireContentUpdateIfNecessary(client);
#if !DEBUG
                    if (flag1 || task.LastScheduledTime.DayOfYear < DateTime.Now.DayOfYear)
#endif
                        instance.RequestMetadata.Phase = LockViewRequestMetadata.TaskPhase.Tack;
                }
            }
            else if (instance.RequestMetadata.Phase == LockViewRequestMetadata.TaskPhase.Tack)
            {
                ImageRequestOverride imgReqOverride = null;
                if (instance.SelectedImageSource == ImageSource.Bing)
                    imgReqOverride = new ImageRequestOverride()
                    {
                        ImageRequestUrl = await BackgroundTaskHelper.GetBingImageFitScreenUrl(client),
                        Arguments = "lq"
                    };
                else if (instance.SelectedImageSource == ImageSource.NASA)
                    imgReqOverride = new ImageRequestOverride()
                    {
                        ImageRequestUrl = await BackgroundTaskHelper.GetNASAImageFitScreenUrl(client),
                        Arguments = $"resolution={instance.PreviewLayoutContract.TargetWidth}x{instance.PreviewLayoutContract.TargetHeight}"
                    };
                //if yes, execute that if (1) the image has changed OR the content has changed.
                await UpdateLockScreenTilesIfPossible(client, task, imgReqOverride);
                instance.RequestMetadata.Phase = LockViewRequestMetadata.TaskPhase.Toe;
            }
            else
            {
                BackgroundTaskHelper.TrySetLockScreenImage(instance.SelectedContextContracts.GenerateImgFileName(), instance.RequestMetadata.RequestLanguage);
                //update tile if necessary.
                BackgroundTaskHelper.TryUpdateTiles();
                instance.RequestMetadata.Phase = LockViewRequestMetadata.TaskPhase.Tick;
            }
        }
        protected async Task LaunchTask(ScheduledTask task)
        {
            //make it very defensive.
            //check last run time.
            var lastExecution = task.LastScheduledTime;
            var instance = LockViewApplicationState.Instance;
            HttpClient client = new HttpClient();
            bool flag1 = false;
#if !DEBUG
            if (DateTime.Now.Hour <= 22 && DateTime.Now.Hour >= 8)
#endif
            {
                //can disturb the user
                //use this client to send request.
                telemetryProperty["within time frame"] = "yes";
                flag1 = await AcquireContentUpdateIfNecessary(client);
                telemetryProperty["update necessary"] = flag1.ToString();
            }
            //does the user have any quota executing that?
#if !DEBUG
            if (flag1 || lastExecution.DayOfYear < DateTime.Now.DayOfYear)
            {
#endif
                ImageRequestOverride imgReqOverride = null;
                if (instance.SelectedImageSource == ImageSource.Bing)
                    imgReqOverride = new ImageRequestOverride()
                    {
                        ImageRequestUrl = await BackgroundTaskHelper.GetBingImageFitScreenUrl(client),
                        Arguments = ""
                    };
                else if (instance.SelectedImageSource == ImageSource.NASA)
                    imgReqOverride = new ImageRequestOverride()
                    {
                        ImageRequestUrl = await BackgroundTaskHelper.GetNASAImageFitScreenUrl(client),
                        Arguments = $"resolution={instance.PreviewLayoutContract.TargetWidth}x{instance.PreviewLayoutContract.TargetHeight}"
                    };
                //if yes, execute that if (1) the image has changed OR the content has changed.
                await UpdateLockScreenTilesIfPossible(client, task, imgReqOverride);

#if !DEBUG
            }
#endif
        }

        private async Task UpdateLockScreenTilesIfPossible(HttpClient client, ScheduledTask task, ImageRequestOverride possibleOverride)
        {
            var instance = LockViewApplicationState.Instance;
            var drainPerReq = Pricing.CalculateDrainPerRequest(instance.RequestMetadata, instance.SelectedProviders.Select(o => o.GetMetaData()));
#if !DEBUG
            if (instance.UserQuotaInDollars - drainPerReq >= 0 || task.LastScheduledTime.DayOfYear < DateTime.Now.DayOfYear)
            {
#endif 
                telemetryProperty["Update Accepted"] = "Yes";
                CloudImageCompositorClient cloudClient = new CloudImageCompositorClient(client);
                ImageCompositionResponse compositionResponse = null;
                if (possibleOverride == null)
                    compositionResponse = await cloudClient.Compose(instance.SelectedContextContracts, instance.PreviewFormattingContract, instance.PreviewLayoutContract, instance.RequestMetadata.PersistFileName);
                else
                    compositionResponse = await cloudClient.ComposeLite(instance.SelectedContextContracts, instance.PreviewFormattingContract, instance.PreviewLayoutContract, possibleOverride);
                telemetryProperty["Image Update Successful"] = "Yes";
                var fileName = instance.SelectedContextContracts.GenerateImgFileName();
                telemetryProperty["File Name"] = fileName;
                var jpegBytes = compositionResponse.Image;
                BackgroundTaskHelper.SaveAndClearUsedComposedImage(jpegBytes, fileName);
                telemetryProperty["File Saved"] = "Yes";
                if (possibleOverride != null && possibleOverride.Arguments == "lq") return;//don't update on money penny
                BackgroundTaskHelper.TrySetLockScreenImage(fileName, instance.RequestMetadata.RequestLanguage);
                //update tile if necessary.
                BackgroundTaskHelper.TryUpdateTiles();
                //drain the user's balance.
                if (instance.UserQuotaInDollars != double.MaxValue) //<--- free users don't get deducted.
                    instance.UserQuotaInDollars -= drainPerReq;
                //instance.UserQuotaInDollars = instance.UserQuotaInDollars < 0 ? 0 : instance.UserQuotaInDollars;
#if !DEBUG
            }
            if (instance.UserQuotaInDollars - drainPerReq < 0)
            {
                var toast = new ShellToast();
                toast.Title = AppResources.LockView;
                toast.Content = AppResources.BalanceRunOut;
                toast.NavigationUri = new Uri(BackgroundTaskHelper.LowBalanceNavId, UriKind.Relative);
                toast.Show();
            }
#endif
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
            if (instance.SelectedContextContracts.Select((o, i) => !o.Equals(contents[i])).Count(o => o) > 0)
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