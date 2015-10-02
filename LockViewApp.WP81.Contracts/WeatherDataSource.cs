using InfoViewApp.WP81.InterestGathering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Data.Xml.Dom;
using Windows.Web.Http;

namespace LockViewApp.WP81.Contracts
{
    public class WeatherDataSource : InterestGatherer
    {
        public string CityName { get; set; }
        public bool IsImperial { get; set; }
        public string Language { get; set; }
        const string ApiKey = "9af6667faad810f1b2bd4fa2dae3d03b"; //<--- if you see this in GitHub, then it is fake.
        const string uriString = "http://www.weatherapi.net/wp-content/uploads/2014/10/openweathermap_logo.png";
        public WeatherDataSource()
        {
            ExtendedContentUrl = uriString;
        }

        public override RequestMetaData GetMetaData()
        {
            return new RequestMetaData()
            {
                BytePerRequest = 20,
                TypicalComputationInSec = 5,
                UpdatePerDay = 16
            };
        }

        public async override Task<InterestContent> RequestContent(InterestRequest request)
        {
            try {
                HttpClient client = Client == null ? new HttpClient() : Client;
                var metricModifier = (IsImperial ? "&units=imperial" : "&units=metric");
                var requestString = $"http://api.openweathermap.org/data/2.5/weather?q={CityName}{metricModifier}=metric&mode=xml&lang={Language}&APPID={ApiKey}";
                var doc = await client.GetStringAsync(new Uri(requestString));
                var document = new XmlDocument();
                document.LoadXml(doc);
                var symbolName = document.SelectSingleNode("/current/weather").Attributes.GetNamedItem("value").NodeValue.ToString();
                var tempValue = document.SelectSingleNode("/current/temperature").Attributes.GetNamedItem("value").NodeValue.ToString();
                var weatherAndTemp = $"{symbolName} ({tempValue}" + (IsImperial ? "℉)" : "℃)");
                return new InterestContent()
                {
                    Content = weatherAndTemp,
                    Title = CityName + " @ " + DateTime.Now,
                    Publisher = $"OpenWeatherMap",
                    ExtensionUri = new Uri(uriString, UriKind.Absolute)
                };
            }
            catch
            {
                return InterestContent.DefaultInterest;
            }
        }
    }
}
