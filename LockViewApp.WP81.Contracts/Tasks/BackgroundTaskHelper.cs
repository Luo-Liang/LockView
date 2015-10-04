using Microsoft.Phone.Scheduler;
using Microsoft.Phone.Shell;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using Windows.Data.Json;
using Windows.Phone.System.UserProfile;
using Windows.Storage;
using Windows.Web.Http;

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
        public static void TrySetLockScreenImage(string fileName, string cultureHint)
        {
            try
            {
                LockScreen.SetImageUri(new Uri($"ms-appx:///Transitioning.png", UriKind.Absolute));
                LockScreen.SetImageUri(new Uri(string.Format("ms-appdata:///local/{0}", fileName), UriKind.Absolute));
            }
            catch (Exception ex)
            {
                LockScreen.SetImageUri(new Uri($"ms-appx:///Outage_{cultureHint}.png", UriKind.Absolute));
            }
        }


        public static void TryUpdateTiles()
        {
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
        }

        public static void SaveAndClearUsedComposedImage(byte[] jpegBytes, string fileName)
        {
            using (IsolatedStorageFile myIsolatedStorage = IsolatedStorageFile.GetUserStoreForApplication())
            {
                string removeName = null;
                try
                {
                    var path = LockScreen.GetImageUri().GetComponents(UriComponents.Path, UriFormat.UriEscaped);
                    var idx = path.LastIndexOf('/');
                    removeName = path.Substring(idx + 1, path.Length - idx - 1);
                }
                catch { }
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
        }

        public static string GetDeviceId()
        {
            byte[] myDeviceID = (byte[])Microsoft.Phone.Info.DeviceExtendedProperties.GetValue("DeviceUniqueId");
            return Convert.ToBase64String(myDeviceID);
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
            imgRequestUrl = string.Format("{0}_{1}x{2}.jpg", imgRequestUrl.Substring(0, imgRequestUrl.LastIndexOf('_')), instance.PreviewLayoutContract.TargetWidth, instance.PreviewLayoutContract.TargetHeight);
            //imgRequestUrl = string.Format("http://www.bing.com{0}", imgRequestUrl);
            if (imgRequestUrl.StartsWith("http") == false)
                imgRequestUrl = string.Format("http://www.bing.com{0}", imgRequestUrl);
            return imgRequestUrl;
        }

        public static async Task<string> GetNASAImageFitScreenUrl(HttpClient client)
        {
            if (client == null)
                client = new HttpClient();
            var instance = LockViewApplicationState.Instance;
            var reqString = "https://api.nasa.gov/planetary/apod?concept_tags=True&api_key=mzzFYcsRbS2oVEak5fvY4Znbx6tTsAy200MiQqXF";
            var json = await client.GetStringAsync(new Uri(reqString));
            var jObj = JsonObject.Parse(json);
            return jObj.GetNamedString("url");
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
            var fileName = string.Join(string.Empty, contracts.Select(o => o.Title));
            fileName = string.Join(string.Empty, fileName.Select(o => ((int)(o % 10)).ToString()));
            if (fileName.Length > 100) fileName = fileName.Substring(0, 100);
            return $"{fileName}.jpeg";
        }
    }
}
