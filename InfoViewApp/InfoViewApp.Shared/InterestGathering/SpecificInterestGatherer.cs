using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Windows.Data.Json;

namespace InfoViewApp.InterestGathering
{
    class GoogleSpecificInterestGatherer : InterestGatherer
    {
        public GoogleSpecificInterestGatherer()
        {
            BaseRequestUrlTemplate = "https://ajax.googleapis.com/ajax/services/search/news?v=1.0&q={0}";
        }
        public async Task<InterestContent> RequestContent(InterestRequest request)
        {
            var requestClient = new HttpClient();
            var response = await requestClient.GetStringAsync(string.Format(BaseRequestUrlTemplate, request.InterestString));

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
                                    Title = HtmlDecodingUtility.HtmlDecode(item.GetObject().GetNamedString("titleNoFormatting"))
                                };
                                if (candidateContent.GetHashCode() != request.PreviousInterestContentIdentifier)
                                {
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
