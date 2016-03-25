#if WINDOWS_PHONE
using Microsoft.Phone.Scheduler;
using Microsoft.Phone.Shell;
using System.IO.IsolatedStorage;
using Windows.Phone.System.UserProfile;
#elif WINDOWS_APP
using NotificationsExtensions.TileContent;
#endif
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using Windows.Data.Json;
using Windows.Storage;
using Windows.Web.Http;
using InfoViewApp.WP81.Tasks;

namespace InfoViewApp.WP81.Tasks
{
    public class BackgroundTaskHelper
    {
        public const string PinnedHeadlineNavId = "/MainPage.xaml?NavId=headLine";
        public const string LowBalanceNavId = "/AllSetPage.xaml?NavId=PurchaseBalance";
        public const string ImageLocator = "http://www.bing.com/HPImageArchive.aspx?format=js&idx=0&n=1&mkt={0}";
        public static async Task<string> BuildRequestString(OverlayContextContract[] contextContracts,
                                                            OverlayFormattingContract formattingContract,
                                                            OverlayLayoutContract layoutContract, string fileName)
        {
            var localRequest = new ImageCompositionRequest();
            localRequest.ContextContracts = contextContracts;
            localRequest.FormattingContract = formattingContract;
            localRequest.LayoutContract = layoutContract;
#if DEBUG
            localRequest.ContextContracts[0].SecondLine = " @" + DateTime.Now;
#endif
            var file = await ApplicationData.Current.LocalFolder.GetFileAsync(fileName);
            byte[] imgBytes;//= new byte[5];
            using (var stream = await file.OpenReadAsync())
            {
                imgBytes = new byte[stream.Size];
                await stream.ReadAsync(imgBytes.AsBuffer(), (uint)imgBytes.Length, Windows.Storage.Streams.InputStreamOptions.None);
            }
            localRequest.RawImage = imgBytes;
            //localRequest.RawImage = Convert.ToBase64String(imgBytes);
            return Newtonsoft.Json.JsonConvert.SerializeObject(localRequest);
        }


        public static string TrySetLockScreenImage(string fileName, string cultureHint)
        {
#if WINDOWS_PHONE
            try
            {
                LockScreen.SetImageUri(new Uri($"ms-appx:///Transitioning.png", UriKind.Absolute));
                LockScreen.SetImageUri(new Uri(string.Format("ms-appdata:///local/{0}", fileName), UriKind.Absolute));
                return "Success";
            }
            catch (Exception ex)
            {
                LockScreen.SetImageUri(new Uri($"ms-appx:///Outage_{cultureHint}.png", UriKind.Absolute));
                return $"filePath:ms-appdata:///local/{fileName}, exception message:{ex}";
            }
#elif WINDOWS_APP
            StorageFile imgFile = ApplicationData.Current.LocalFolder.GetFileAsync(fileName).GetAwaiter().GetResult();
            Windows.System.UserProfile.LockScreen.SetImageFileAsync(imgFile).GetAwaiter().GetResult();
#endif
        }


        public static void TryUpdateTiles()
        {
#if WINDOWS_PHONE
            var uri = new Uri(PinnedHeadlineNavId, UriKind.Relative);
            var isPinned = ShellTile.ActiveTiles.Any<ShellTile>(st => st.NavigationUri == uri);
            if (isPinned)
            {
                var tile = ShellTile.ActiveTiles.First<ShellTile>(st => st.NavigationUri == uri);
                var context = LockViewApplicationState.Instance.PreviewContextContract;
                var standardTile = new StandardTileData() { Title = context.SecondLine, BackTitle = context.Title, BackContent = context.FirstLine };
                if (context.ExtendedUri != null)
                {
                    standardTile.BackgroundImage = new Uri(context.ExtendedUri, UriKind.Absolute);
                }
                tile.Update(standardTile);
            }
#elif WINDOWS_APP
            if (Windows.UI.StartScreen.SecondaryTile.Exists("CURR_STORY"))
            {
                ITileSquare150x150Text04 squareContent = TileContentFactory.CreateTileSquare150x150Text04();
                squareContent.TextBodyWrap.Text = "Sent to a secondary tile from NotificationExtensions!";
                // Send the notification to the secondary tile by creating a secondary tile updater
                Windows.UI.Notifications.TileUpdateManager.CreateTileUpdaterForSecondaryTile("CURR_STORY").Update(squareContent.CreateNotification());
            }
#endif
        }

        public async static Task<string> RequestJsonProxied(string getAddr, HttpClient client = null)
        {
            if (client == null) client = new HttpClient();
            var requestContent = new HttpStringContent(Newtonsoft.Json.JsonConvert.SerializeObject(getAddr));
            //var requestContent = new HttpStringContent(Newtonsoft.Json.JsonConvert.SerializeObject(string.Format(ImageLocator,"en-US")));
            requestContent.Headers.ContentType = new Windows.Web.Http.Headers.HttpMediaTypeHeaderValue("application/json");
            var response = await client.PostAsync(new Uri("http://cloudimagecomposition.azurewebsites.net/ImageComposition.svc/RequestJson", UriKind.Absolute),
                 requestContent);
            return await response.Content.ReadAsStringAsync();
        }

        public static void
            SaveAndClearUsedComposedImage(byte[] jpegBytes, string fileName)
        {
#if WINDOWS_PHONE
            using (IsolatedStorageFile myIsolatedStorage = IsolatedStorageFile.GetUserStoreForApplication())
            {
                string removeName = null;
                try
                {
                    var path = LockScreen.GetImageUri().GetComponents(UriComponents.Path, UriFormat.UriEscaped);
                    var idx = path.LastIndexOf('/');
                    removeName = path.Substring(idx + 1, path.Length - idx - 1);
                }
                catch (Exception ex)
                {
                }
                if (removeName != null && myIsolatedStorage.FileExists(removeName))
                {
                    myIsolatedStorage.DeleteFile(removeName);
                }
                var file = myIsolatedStorage.CreateFile(fileName);
                using (var fs = file)
                {
                    fs.Write(jpegBytes, 0, jpegBytes.Length);
                }
            }
#elif WINDOWS_APP
            StorageFile imgFile = null;
            try
            {
                imgFile = StorageFile.GetFileFromApplicationUriAsync(Windows.System.UserProfile.LockScreen.OriginalImageFile).GetAwaiter().GetResult();
            }
            catch (Exception ex)
            { }
            if (imgFile != null)
                imgFile.DeleteAsync(StorageDeleteOption.PermanentDelete).GetAwaiter().GetResult();
            StorageFolder folder = ApplicationData.Current.LocalFolder;
            var storageFile = folder.CreateFileAsync(fileName, CreationCollisionOption.ReplaceExisting).GetAwaiter().GetResult();
            using (var randomAccessStream = storageFile.OpenAsync(FileAccessMode.ReadWrite).GetAwaiter().GetResult())
            {
                randomAccessStream.WriteAsync(jpegBytes.AsBuffer()).GetAwaiter().GetResult();
            }
#endif
        }

        public static void PrepareFormattingContractForScaling(OverlayFormattingContract instance)
        {
            var scale = LockViewApplicationState.Instance.RequestMetadata.ScaleFactor * 0.75;
            instance.FirstLineFont.FontSize = (int)(instance.FirstLineFont.FontSize * scale);
            // instance.SecondLineFont.FontSize = (int)(instance.SecondLineFont.FontSize * scale);
            instance.TitleFont.FontSize = (int)(instance.TitleFont.FontSize * scale);
        }

        public static void RestoreFormattingContractForSerialization(OverlayFormattingContract instance)
        {
            var scale = LockViewApplicationState.Instance.RequestMetadata.ScaleFactor * 0.75;
            instance.FirstLineFont.FontSize = (int)(instance.FirstLineFont.FontSize / scale);
            //  instance.SecondLineFont.FontSize = (int)(instance.SecondLineFont.FontSize / scale);
            instance.TitleFont.FontSize = (int)(instance.TitleFont.FontSize / scale);
        }

        public static void RegisterOrRenewBackgroundAgent()
        {
#if WINDOWS_PHONE
            var periodicTask = ScheduledActionService.Find("BackgroundTask");
            if (periodicTask != null)
            {
                //#if !DEBUG
                ScheduledActionService.Remove("BackgroundTask");
                //#else
                //ScheduledActionService.LaunchForTest("BackgroundTask", TimeSpan.FromSeconds(2));
                //                return;
                //#endif
            }
            periodicTask = new PeriodicTask("BackgroundTask");
            (periodicTask as ScheduledTask).Description = "Updates Lock Screen when new content is available.";
            ScheduledActionService.Add(periodicTask);
            if (Debugger.IsAttached)
                ScheduledActionService.LaunchForTest("BackgroundTask", TimeSpan.FromSeconds(2));
#elif WINDOWS_APP
            throw new NotImplementedException();
#endif
        }

        public static string GetDeviceId()
        {
#if WINDOWS_PHONE
            byte[] myDeviceID = (byte[])Microsoft.Phone.Info.DeviceExtendedProperties.GetValue("DeviceUniqueId");
            return Convert.ToBase64String(myDeviceID);
#elif WINDOWS_APP
            throw new NotImplementedException();
#endif
        }

        public static async Task<string> GetLiveEarthImageFitScreenUrl(HttpClient client)
        {
            return "lockview://fixedwallpapers/Himawari-8";
        }

        public static async Task<string> GetBingImageFitScreenUrl(HttpClient client)
        {
            if (client == null)
                client = new HttpClient();
            var instance = LockViewApplicationState.Instance;
            var lang = instance.RequestMetadata.RequestLanguage;
            var reqString = string.Format(BackgroundTaskHelper.ImageLocator, lang);
            var json = await client.GetStringAsync(new Uri(reqString));
            var jObj = JsonObject.Parse(json);
            var imgRequestUrl = jObj.GetNamedArray("images")[0].GetObject().GetNamedString("url");
            //#if WINDOWS_PHONE
            //            imgRequestUrl = string.Format("{0}_{1}x{2}.jpg", imgRequestUrl.Substring(0, imgRequestUrl.LastIndexOf('_')), instance.PreviewLayoutContract.TargetWidth, instance.PreviewLayoutContract.TargetHeight);
            //            //imgRequestUrl = string.Format("http://www.bing.com{0}", imgRequestUrl);
            //#endif
            if (imgRequestUrl.StartsWith("http") == false)
                imgRequestUrl = string.Format("http://www.bing.com{0}", imgRequestUrl);
            return imgRequestUrl;
        }

        public static async Task<string> GetNASAImageFitScreenUrl(HttpClient client)
        {
            if (client == null)
                client = new HttpClient();
            var instance = LockViewApplicationState.Instance;
            var requestContent = new HttpStringContent(Newtonsoft.Json.JsonConvert.SerializeObject("https://api.nasa.gov/planetary/apod?concept_tags=True&api_key=2iBksIVxBi8g0dsMpXFdaNgxCEJx1mrtivaKCGjn"));
            //var requestContent = new HttpStringContent(Newtonsoft.Json.JsonConvert.SerializeObject(string.Format(ImageLocator,"en-US")));
            requestContent.Headers.ContentType = new Windows.Web.Http.Headers.HttpMediaTypeHeaderValue("application/json");
            var response = await client.PostAsync(new Uri("http://cloudimagecomposition.azurewebsites.net/ImageComposition.svc/RequestJson", UriKind.Absolute),
                 requestContent);
            var responseStr = await response.Content.ReadAsStringAsync();
            var result = Newtonsoft.Json.JsonConvert.DeserializeObject<string>(responseStr);
            var jObj = JsonObject.Parse(result);
            result = jObj.GetNamedString("url");
            return result;
        }
    }


}
namespace InfoViewApp.WP81
{
    public static class Extensions
    {
        public static void CopyFromInterestContent(this OverlayContextContract context, InterestGathering.InterestContent content)
        {
            context.Title = content.Title;
            context.FirstLine = content.Content;
            context.SecondLine = content.Publisher;
            if (content.ContentUri != null)
                context.JumpUri = content.ContentUri.ToString();
            if (content.ExtensionUri != null)
                context.ExtendedUri = content.ExtensionUri.ToString();
        }

        public static string GenerateImgFileName(this IEnumerable<OverlayContextContract> contracts)
        {
            var fileName = string.Join(string.Empty, contracts.Select(o => o.Title.Length > 25 ? $"{o.Title.Substring(0, 12)}{o.Title.Substring(o.Title.Length - 13, 13)}" : o.Title));
            fileName = string.Join(string.Empty, fileName.Select(o => ((int)(o % 10)).ToString()));
            if (fileName.Length > 100) fileName = fileName.Substring(0, 100);
            return $"{fileName}.jpeg";
        }

        public static async Task<ImageRequestOverride> CreateRequestOverride(this LockViewApplicationState instance)
        {
            ImageRequestOverride imgReqOverride = null;
            if (instance.SelectedImageSource != ImageSource.Local)
            {
                imgReqOverride = new ImageRequestOverride();
                imgReqOverride.Arguments = $"resolution={instance.PreviewLayoutContract.TargetWidth}x{instance.PreviewLayoutContract.TargetHeight}&{instance.SelectedImageSourceParameters}";
                if (instance.SelectedImageSource == ImageSource.Bing)
                {
                    imgReqOverride.ImageRequestUrl = await BackgroundTaskHelper.GetBingImageFitScreenUrl(null);
                }
                else if (instance.SelectedImageSource == ImageSource.NASA)
                {
                    imgReqOverride.ImageRequestUrl = await BackgroundTaskHelper.GetNASAImageFitScreenUrl(null);
                }
                else if (instance.SelectedImageSource == ImageSource.LiveEarth)
                {
                    imgReqOverride.ImageRequestUrl = await BackgroundTaskHelper.GetLiveEarthImageFitScreenUrl(null);
                }
            }
            return imgReqOverride;
        }
    }
}
