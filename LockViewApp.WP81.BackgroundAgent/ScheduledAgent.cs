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
        }

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
#if !DEBUG
            try
            {
#endif
            if (DeviceStatus.DeviceTotalMemory >> 29 < 1)
            {
                //low ram device.
                await LaunchLowRAMTask(task);
            }
            else
            {
                await LaunchTask(task);
            }
            await LockViewApplicationState.Instance.SaveState();
#if DEBUG
            var toast = new ShellToast();
            toast.Title = task.LastExitReason.ToString() + " " + LockViewApplicationState.Instance.RequestMetadata.Phase;
            toast.Content = (DeviceStatus.ApplicationPeakMemoryUsage / 1024.0 / 1024).ToString();
            toast.Show();
#else
            }
            catch
            {

            }
#endif
            NotifyComplete();

        }

        protected async Task LaunchLowRAMTask(ScheduledTask task)
        {
            //
            //check phase first.
            var instance = LockViewApplicationState.Instance;
            HttpClient client = new HttpClient();
            if (instance.RequestMetadata.Phase == LockViewRequestMetadata.TaskPhase.Tick)
            {
                //download necessary files.
#if !DEBUG
                if (task.LastScheduledTime.DayOfYear < DateTime.Now.DayOfYear && LockViewApplicationState.Instance.SelectedImageSource == ImageSource.Bing)
#endif
                {
                    //has time passed yet?
                    //"MyBg.jpeg"
                    await StoreDownloadedImage(client);
                    instance.RequestMetadata.Phase = LockViewRequestMetadata.TaskPhase.Tack;
                }
            }
            else if (instance.RequestMetadata.Phase == LockViewRequestMetadata.TaskPhase.Tack)
            {
#if !DEBUG
                if (DateTime.Now.Hour <= 22 && DateTime.Now.Hour >= 8)
#endif
                {
                    //can disturb the user
                    //use this client to send request.
                    await AcquireContentUpdateIfNecessary(client);
                    instance.RequestMetadata.Phase = LockViewRequestMetadata.TaskPhase.Toe;
                }
            }
            else
            {
                //for a  tock part of the task, send request immediately.
                await UpdateLockScreenTilesIfPossible(client, task);
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
            bool flag1 = false, flag2 = false;
#if !DEBUG
            if (task.LastScheduledTime.DayOfYear < DateTime.Now.DayOfYear && LockViewApplicationState.Instance.SelectedImageSource == ImageSource.Bing)
#endif
            {
                //has time passed yet?
                //"MyBg.jpeg"
                await StoreDownloadedImage(client);
                flag2 = true;
            }
#if !DEBUG
            if (DateTime.Now.Hour <= 22 && DateTime.Now.Hour >= 8)
#endif
            {
                //can disturb the user
                //use this client to send request.
                flag1 = await AcquireContentUpdateIfNecessary(client);
            }
            //does the user have any quota executing that?
#if !DEBUG
            if (flag1 || flag2)
            {
#endif
            //if yes, execute that if (1) the image has changed OR the content has changed.
            await UpdateLockScreenTilesIfPossible(client, task);

#if !DEBUG
            }
#endif
        }

        private static async Task UpdateLockScreenTilesIfPossible(HttpClient client, ScheduledTask task)
        {
            var instance = LockViewApplicationState.Instance;
            var drainPerReq = Pricing.CalculateDrainPerRequest(instance.RequestMetadata, instance.SelectedProvider.GetMetaData());
#if !DEBUG
            if (instance.UserQuotaInDollars - drainPerReq >= 0 || task.LastScheduledTime.DayOfYear < DateTime.Now.DayOfYear)
            {
#endif
            CloudImageCompositorClient cloudClient = new CloudImageCompositorClient(client);
            var compositionResponse = await cloudClient.Compose(instance.PreviewContextContract, instance.PreviewFormattingContract, instance.PreviewLayoutContract, instance.RequestMetadata.PersistFileName);
            var fileName = DateTime.Now.ToBinary().ToString() + ".jpeg";
            var jpegBytes = compositionResponse.Image;
            BackgroundTaskHelper.SaveAndClearUsedComposedImage(jpegBytes, fileName);
            //update tile and/or lock screen image.
            BackgroundTaskHelper.TrySetLockScreenImage(fileName);
            //update tile if necessary.
            BackgroundTaskHelper.TryUpdateTiles();
            //drain the user's balance.
            instance.UserQuotaInDollars -= drainPerReq;
            if (instance.UserQuotaInDollars - drainPerReq < 0)
            {
                var toast = new ShellToast();
                toast.Title = "LOCKVIEW ALERT";
                toast.Content = "Your balance has run out. Update is now minimum.";
                toast.NavigationUri = new Uri(BackgroundTaskHelper.LowBalanceNavId, UriKind.Relative);
                toast.Show();
            }
#if !DEBUG
            }
#endif
        }

        private static async Task<bool> AcquireContentUpdateIfNecessary(HttpClient client)
        {
            var flag1 = false;
            var instance = LockViewApplicationState.Instance;
            instance.SelectedProvider.Client = client;
            var content = await instance.SelectedProvider.RequestContent(LockViewApplicationState.Instance.SelectedInterest);
            if (content.GetHashCode() != instance.PreviewContextContract.GetHashCode())
            {
                //are we getting the same update?
                flag1 = true;
                //set flag1 to true -- approve.
                instance.PreviewContextContract.CopyFromInterestContent(content);
            }

            return flag1;
        }

        private static async Task StoreDownloadedImage(HttpClient client)
        {
            var instance = LockViewApplicationState.Instance;
            var lang = instance.RequestMetadata.RequestLanguage;
            var reqString = string.Format(BackgroundTaskHelper.ImageLocator, lang);
            var json = await client.GetStringAsync(new Uri(reqString));
            var jObj = JsonObject.Parse(json);
            var imgRequestUrl = jObj.GetNamedArray("images")[0].GetObject().GetNamedString("url");
            imgRequestUrl = string.Format("{0}_{1}x{2}.jpg", imgRequestUrl.Substring(0, imgRequestUrl.LastIndexOf('_')), instance.PreviewLayoutContract.TargetWidth, instance.PreviewLayoutContract.TargetHeight);
            var imgUrl = string.Format("http://www.bing.com{0}", imgRequestUrl);
            var response = await client.GetAsync(new Uri(imgUrl));
            using (IsolatedStorageFile myIsolatedStorage = IsolatedStorageFile.GetUserStoreForApplication())
            {
                var fileName = LockViewApplicationState.Instance.RequestMetadata.PersistFileName;
                if (myIsolatedStorage.FileExists(fileName))
                {
                    myIsolatedStorage.DeleteFile(fileName);
                }
                var file = myIsolatedStorage.CreateFile(fileName);
                using (var fs = file)
                {
                    await response.Content.WriteToStreamAsync(fs.AsOutputStream());
                    instance.RequestMetadata.ImageBytesPerRequest = (int)fs.Length;
                }
            }
        }

    }
}