﻿using System;
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
        /// <summary>
        /// On money penny device, an HttpClient will take a lot of memory. 
        /// </summary>
        public HttpClient Client { get; set; }
        public async Task<ImageCompositionResponse> Compose(OverlayContextContract[] PreviewContextContract,
                                                            OverlayFormattingContract PreviewFormattingContract,
                                                            OverlayLayoutContract PreviewLayoutContract,
                                                            string fileName)
        {
            HttpClient client = Client;
            var localRequest = new ImageCompositionRequest();
            localRequest.ContextContracts = PreviewContextContract;
            localRequest.FormattingContract = PreviewFormattingContract;
            localRequest.LayoutContract = PreviewLayoutContract;
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
            string reqContent = null;
            localRequest.RawImage = imgBytes;
            //localRequest.RawImage = Convert.ToBase64String(imgBytes);
            reqContent = Newtonsoft.Json.JsonConvert.SerializeObject(localRequest);
            localRequest = null;
            var requestContent = new HttpStringContent(reqContent);
            reqContent = null;
            GC.AddMemoryPressure(2048);
            GC.Collect();
            requestContent.Headers.ContentType = new Windows.Web.Http.Headers.HttpMediaTypeHeaderValue("application/json");
            //https://ajax.googleapis.com/ajax/services/search/news?v=1.0&q={0}
            var response = await client.PostAsync(new Uri("http://cloudimagecomposition.azurewebsites.net/ImageComposition.svc/ComposeV2", UriKind.Absolute),
                 requestContent);
            var responseStr = await response.Content.ReadAsStringAsync();
            var result = Newtonsoft.Json.JsonConvert.DeserializeObject<ImageCompositionResponse>(responseStr);
            return result;
        }

        public async Task<ImageCompositionResponse> ComposeLite(OverlayContextContract[] PreviewContextContract,
                                                                OverlayFormattingContract PreviewFormattingContract,
                                                                OverlayLayoutContract PreviewLayoutContract,
                                                                ImageRequestOverride imgReqOverride)
        {

            HttpClient client = Client;
            var localRequest = new ImageCompositionRequest();
            localRequest.ContextContracts = PreviewContextContract;
            localRequest.FormattingContract = PreviewFormattingContract;
            localRequest.LayoutContract = PreviewLayoutContract;
#if DEBUG
            localRequest.ContextContracts[0].SecondLine = " @" + DateTime.Now;
#endif
            localRequest.ImageRequestOverride = imgReqOverride;
            //localRequest.RawImage = Convert.ToBase64String(imgBytes);
            var reqContent = Newtonsoft.Json.JsonConvert.SerializeObject(localRequest);
            var requestContent = new HttpStringContent(reqContent);
            requestContent.Headers.ContentType = new Windows.Web.Http.Headers.HttpMediaTypeHeaderValue("application/json");
            //https://ajax.googleapis.com/ajax/services/search/news?v=1.0&q={0}

            //http://cloudimagecomposition.azurewebsites.net/ImageComposition.svc/ComposeV2"
            var response = await client.PostAsync(new Uri("http://cloudimagecomposition.azurewebsites.net/ImageComposition.svc/ComposeV2", UriKind.Absolute),
                 requestContent);
            var responseStr = await response.Content.ReadAsStringAsync();
            var result = Newtonsoft.Json.JsonConvert.DeserializeObject<ImageCompositionResponse>(responseStr);
            return result;
        }



        public CloudImageCompositorClient(HttpClient client)
        {
            Client = client;
        }
        public CloudImageCompositorClient()
        {
            Client = new HttpClient();
        }
    }
}
