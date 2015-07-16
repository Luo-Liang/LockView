using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Windows.Data.Json;
using Windows.Web.Http;

namespace InfoViewApp.WP81.InterestGathering
{
    public class GoogleSpecificInterestGatherer : InterestGatherer
    {
        public GoogleSpecificInterestGatherer()
        {
            BaseRequestUrlTemplate = "https://ajax.googleapis.com/ajax/services/search/news?v=1.0&q={0}";
        }

        public override RequestMetaData GetMetaData()
        {
            return new RequestMetaData()
            {
                UpdatePerDay = 15,
                BytePerRequest = 1024,
            };
        }

        public override async Task<InterestContent> RequestContent(InterestRequest request)
        {
            var requestClient = Client == null ? new HttpClient() : Client;
            string response = null;
            try
            {
                response = await requestClient.GetStringAsync(new Uri(string.Format(BaseRequestUrlTemplate, request.InterestString)));
            }
            catch { }
            if (response != null)
            {
                JsonObject responseObj = null;
                if (JsonObject.TryParse(response, out responseObj))
                {
                    //responseObj is responseData
                    if (200 == responseObj.GetNamedNumber("responseStatus"))
                    {
                        //responseObj is okay.
                        JsonObject responseData = responseObj.GetNamedObject("responseData", null);
                        if (responseData != null)
                        {
                            var resultArray = responseData.GetNamedArray("results", null);
                            if (resultArray != null)
                            {
                                foreach (var item in resultArray)
                                {
                                    var candidateContent = new InterestContent()
                                    {
                                        Content = HtmlDecodingUtility.HtmlDecode(item.GetObject().GetNamedString("content", string.Empty)),
                                        Publisher = HtmlDecodingUtility.HtmlDecode(item.GetObject().GetNamedString("publisher", string.Empty)),
                                        Title = HtmlDecodingUtility.HtmlDecode(item.GetObject().GetNamedString("titleNoFormatting")),
                                        ContentUri = new Uri(item.GetObject().GetNamedString("unescapedUrl"))
                                    };
                                    return candidateContent;
                                }
                            }
                        }
                    }
                }
            }
            return null;
        }
    }
}
