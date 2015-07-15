using Microsoft.Phone.Scheduler;
using Microsoft.Phone.Shell;
using System;
using System.Collections.Generic;
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
        public static async Task<string> BuildRequestString(OverlayContextContract contextContract,
                                                            OverlayFormattingContract formattingContract,
                                                            OverlayLayoutContract layoutContract, string fileName)
        {
            var localRequest = new ImageCompositionRequest();
            localRequest.ContextContract = contextContract;
            localRequest.FormattingContract = formattingContract;
            localRequest.LayoutContract = layoutContract;
#if DEBUG
            localRequest.ContextContract.SecondLine = " @" + DateTime.Now;
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
        public static void TrySetLockScreenImage(string fileName)
        {
            try
            {
                LockScreen.SetImageUri(new Uri("ms-appx:///LockView.png", UriKind.Absolute));
            }
            catch (Exception ex)
            {
            }
            LockScreen.SetImageUri(new Uri(string.Format("ms-appdata:///local/{0}", fileName, UriKind.Absolute)));
        }

        public static void TryUpdateTiles()
        {
            var uri = new Uri(PinnedHeadlineNavId, UriKind.Relative);
            var isPinned = ShellTile.ActiveTiles.Any<ShellTile>(st => st.NavigationUri == uri);
            if (isPinned)
            {
                var tile = ShellTile.ActiveTiles.First<ShellTile>(st => st.NavigationUri == uri);
                var context = LockViewApplicationState.Instance.PreviewContextContract;
                var standardTile = new StandardTileData() { Title = "LockView", BackTitle = context.Title, BackContent = context.FirstLine };
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
                ScheduledActionService.Remove("BackgroundTask");
            }
            periodicTask = new PeriodicTask("BackgroundTask");
            (periodicTask as ScheduledTask).Description = "Updates Lock Screen when new content is available.";
            ScheduledActionService.Add(periodicTask);
            //#if DEBUG
            ScheduledActionService.LaunchForTest("BackgroundTask", TimeSpan.FromSeconds(2));
            //#endif
        }

        public static async Task<string> GetBingImageFitScreenUrl(HttpClient client)
        {
            var instance = LockViewApplicationState.Instance;
            var lang = instance.RequestMetadata.RequestLanguage;
            var reqString = string.Format(BackgroundTaskHelper.ImageLocator, lang);
            var json = await client.GetStringAsync(new Uri(reqString));
            var jObj = JsonObject.Parse(json);
            var imgRequestUrl = jObj.GetNamedArray("images")[0].GetObject().GetNamedString("url");
            imgRequestUrl = string.Format("{0}_{1}x{2}.jpg", imgRequestUrl.Substring(0, imgRequestUrl.LastIndexOf('_')), instance.PreviewLayoutContract.TargetWidth, instance.PreviewLayoutContract.TargetHeight);
            var imgUrl = string.Format("http://www.bing.com{0}", imgRequestUrl);
            return imgUrl;
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

        public static string GenerateImgFileName(this OverlayContextContract contract)
        {
            var fileName = contract.Title;
            foreach (char c in System.IO.Path.GetInvalidFileNameChars())
            {
                fileName = fileName.Replace(c, '_');
            }
            return string.Format("{0}.jpeg", fileName);
        }
    }
}
