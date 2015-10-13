using InfoViewApp.WP81.InterestGathering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Data.Json;

namespace LockViewApp.WP81.Contracts
{
    public class NASAAPODCaptionSource : InterestGatherer
    {
        public override RequestMetaData GetMetaData()
        {
            return new RequestMetaData()
            {
                BytePerRequest = 100,
                TypicalComputationInSec = 5,
                UpdatePerDay = 1
            };
        }

        public async override Task<InterestContent> RequestContent(InterestRequest request)
        {
            try
            {
                if (Client == null)
                    Client = new Windows.Web.Http.HttpClient();
                var jString = await Client.GetStringAsync(new Uri("https://api.nasa.gov/planetary/apod?concept_tags=True&api_key=mzzFYcsRbS2oVEak5fvY4Znbx6tTsAy200MiQqXF"));
                var jObj = JsonObject.Parse(jString);
                return new InterestContent()
                {
                    Content = HtmlDecodingUtility.HtmlDecode(jObj.GetNamedString("explanation")),
                    Title = jObj.GetNamedString("title"),
                    Publisher = "NASA",
                    ExtensionUri = new Uri("http://quest.nasa.gov/ltc/images/logo-nasa.gif")
                };
            }
            catch
            {
                return InterestContent.DefaultInterest;
            }
        }
    }
}
