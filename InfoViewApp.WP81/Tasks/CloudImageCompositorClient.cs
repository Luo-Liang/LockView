using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Windows.Data.Json;
using Windows.Storage;
using Windows.Web.Http;

namespace InfoViewApp.WP81.Tasks
{
    class CloudImageCompositorClient
    {
        public async Task<ImageCompositionResponse> Compose(OverlayContextContract PreviewContextContract,
                                  OverlayFormattingContract PreviewFormattingContract,
                                  OverlayLayoutContract PreviewLayoutContract,
                                  string fileName)
        {
            HttpClient client = new HttpClient();
            var localRequest = new ImageCompositionRequest();
            var scale = ResolutionProvider.GetScaleFactor();
            localRequest.ContextContract = PreviewContextContract;
            localRequest.FormattingContract = PreviewFormattingContract;
            localRequest.FormattingContract.FirstLineFont.FontSize = (int)(localRequest.FormattingContract.FirstLineFont.FontSize*scale);
            localRequest.FormattingContract.SecondLineFont.FontSize = (int)(localRequest.FormattingContract.SecondLineFont.FontSize * scale);
            localRequest.FormattingContract.TitleFont.FontSize = (int)(localRequest.FormattingContract.TitleFont.FontSize * scale);
            localRequest.LayoutContract = PreviewLayoutContract;
            localRequest.LayoutContract.Origin = new Point() { X = (int)(20* scale), Y = (int)(20* scale) };
            localRequest.LayoutContract.AutoExpand = true;
            localRequest.LayoutContract.ParagraphSpacing = (int)(10* scale);
            double height, width;
            ResolutionProvider.GetScreenSizeInPixels(out height, out width);
            localRequest.LayoutContract.TargetHeight = (int)height;
            localRequest.LayoutContract.TargetWidth = (int)width; 
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
