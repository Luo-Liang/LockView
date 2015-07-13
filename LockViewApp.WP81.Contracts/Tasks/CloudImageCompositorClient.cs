using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Windows.Data.Json;
using Windows.Storage;
using Windows.Web.Http;

namespace InfoViewApp.WP81.Tasks
{
    public class CloudImageCompositorClient
    {
        public async Task<ImageCompositionResponse> Compose(OverlayContextContract PreviewContextContract,
                                  OverlayFormattingContract PreviewFormattingContract,
                                  OverlayLayoutContract PreviewLayoutContract,
                                  string fileName)
        {
            HttpClient client = new HttpClient();
            var localRequest = new ImageCompositionRequest();
            localRequest.ContextContract = PreviewContextContract;
            localRequest.FormattingContract = PreviewFormattingContract;
            localRequest.LayoutContract = PreviewLayoutContract;
            BackgroundTaskHelper.PrepareFormattingContractForScaling(localRequest.FormattingContract);
#if DEBUG
            localRequest.ContextContract.SecondLine += " @" + DateTime.Now;
#endif
            BackgroundTaskHelper.RestoreFormattingContractForSerialization(localRequest.FormattingContract);
            var file = await ApplicationData.Current.LocalFolder.GetFileAsync(fileName);
            byte[] imgBytes;//= new byte[5];
            using (var stream = await file.OpenReadAsync())
            {
                imgBytes = new byte[stream.Size];
                await stream.ReadAsync(imgBytes.AsBuffer(), (uint)imgBytes.Length, Windows.Storage.Streams.InputStreamOptions.None);
            }
            string reqContent = null;
            localRequest.RawImage = Convert.ToBase64String(imgBytes);
            await Task.Run(() => reqContent = Newtonsoft.Json.JsonConvert.SerializeObject(localRequest));
            var requestContent = new HttpStringContent(reqContent);
            requestContent.Headers.ContentType = new Windows.Web.Http.Headers.HttpMediaTypeHeaderValue("application/json");
            //https://ajax.googleapis.com/ajax/services/search/news?v=1.0&q={0}
            var response = await client.PostAsync(new Uri("http://cloudimagecomposition.azurewebsites.net/ImageComposition.svc/Compose", UriKind.Absolute),
                 requestContent);
            var responseStr=await response.Content.ReadAsStringAsync();
            var result = Newtonsoft.Json.JsonConvert.DeserializeObject<ImageCompositionResponse>(responseStr);
            return result;
        }
    }
}
