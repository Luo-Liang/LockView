using InfoViewApp.WP81;
using InfoViewApp.WP81.Tasks;
using Microsoft.ApplicationInsights;
using NotificationsExtensions.ToastContent;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using Windows.Storage;
using Windows.System.UserProfile;
using Windows.UI.Notifications;
using Windows.Web.Http;

namespace LockViewApp.W81.BackgroundTasks
{
    public sealed class BackgroundTask : IBackgroundTask
    {
        public async void Run(IBackgroundTaskInstance taskInstance)
        {
            var deferral = taskInstance.GetDeferral();
            var instance = LockViewApplicationState.Instance;
            var dict = new Dictionary<string, string>();
            try
            {
                TelemetryClient telemetryClient = new TelemetryClient();
                HttpClient client = new HttpClient();
                if ((DateTime.Now.Hour <= 22 && DateTime.Now.Hour >= 8) || !instance.DoNotDisturb)
                {
                    dict["in-time"] = "true";
                    foreach (var provider in instance.SelectedProviders)
                    {
                        provider.Client = client;
                    }
                    var contents = await Task.WhenAll(instance.SelectedProviders.Select(async (o, i) => await o.RequestContent(instance.SelectedInterests[i])));
                    if (instance.SelectedContextContracts.Select((o, i) => !o.Equals(contents[i])).Count(o => o) > 0 || System.Diagnostics.Debugger.IsAttached)
                    {
                        dict["update-accepted"] = "true";
                        //are we getting the same update?
                        //set flag1 to true -- approve.
                        for (int i = 0; i < instance.SelectedContextContracts.Length; i++)
                        {
                            instance.SelectedContextContracts[i].CopyFromInterestContent(contents[i]);
                        }

                        ImageRequestOverride imgReqOverride = null;
                        if (instance.SelectedImageSource == ImageSource.Bing)
                        {
                            imgReqOverride = new ImageRequestOverride()
                            {
                                ImageRequestUrl = await BackgroundTaskHelper.GetBingImageFitScreenUrl(client),
                                Arguments = $"resolution={instance.PreviewLayoutContract.TargetWidth}x{instance.PreviewLayoutContract.TargetHeight}"
                            };
                        }
                        else if (instance.SelectedImageSource == ImageSource.NASA)
                        {
                            imgReqOverride = new ImageRequestOverride()
                            {
                                ImageRequestUrl = await BackgroundTaskHelper.GetNASAImageFitScreenUrl(client),
                                Arguments = $"resolution={instance.PreviewLayoutContract.TargetWidth}x{instance.PreviewLayoutContract.TargetHeight}"
                            };
                        }
                        var drainPerReq = Pricing.CalculateDrainPerRequest(instance.RequestMetadata, instance.SelectedProviders.Select(o => o.GetMetaData()));
                        if (instance.UserQuotaInDollars - drainPerReq >= 0 || instance.BackgroundTaskLastRun.DayOfYear < DateTime.Now.DayOfYear)
                        {
                            dict["update-successful"] = "true";
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
                            if (instance.UserQuotaInDollars != double.MaxValue) //<--- free users don't get deducted.
                                instance.UserQuotaInDollars -= drainPerReq;
                            //instance.UserQuotaInDollars = instance.UserQuotaInDollars < 0 ? 0 : instance.UserQuotaInDollars;
                            instance.BackgroundTaskLastRun = DateTime.Now;
                        }
                        if (instance.UserQuotaInDollars - drainPerReq < 0 && (DateTime.Now.DayOfYear - instance.BackgroundTaskLastRun.DayOfYear) != 0)
                        {
                            IToastNotificationContent toastContent = null;
                            IToastText02 templateContent = ToastContentFactory.CreateToastText02();
                            templateContent.TextHeading.Text = "LOCKVIEW";
                            templateContent.TextBodyWrap.Text = "Your balance has run out!";
                            toastContent = templateContent;
                            ToastNotification toast = toastContent.CreateNotification();

                            // If you have other applications in your package, you can specify the AppId of
                            // the app to create a ToastNotifier for that application
                            ToastNotificationManager.CreateToastNotifier().Show(toast);
                        }
                    }
                }
                //gather app insights information.
                TelemetryClient tc = new TelemetryClient();
                dict["quota"] = instance.UserQuotaInDollars.ToString();
                dict["finish-time"] = DateTime.UtcNow.ToString();
                tc.TrackEvent("background task non-mobile",dict);
            }
            catch (Exception ex)
            {

            }

            await instance.SaveState();
            deferral.Complete();
        }
    }
}
