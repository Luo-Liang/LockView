using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Windows;
using InfoViewApp.WP81;
using Windows.Storage;
using Windows.Web.Http;
using Windows.Data.Xml.Dom;
using HtmlAgilityPack;
using System.Runtime.InteropServices.WindowsRuntime;
#if WINDOWS_APP
using intelliSys.XPath;
#endif


namespace InfoViewApp.WP81.InterestGathering.LanguageLearning
{
    public class OnlineSource : LanguageSourceBase
    {
        public string HeadlineSelectionPath { get; set; }
        public string SecondaryLineSelectionPath { get; set; }

        public string PhoneticSelectionPath { get; set; }
        public override async Task<InterestContent> RequestContent(InterestRequest request)
        {
            HttpClient client = Client == null ? new HttpClient() : Client;
            client.DefaultRequestHeaders.Add("Accept-Charset", "UTF-8");
            var responseBytes = await client.GetBufferAsync(new System.Uri(RequestString));
            var response = UTF8Encoding.UTF8.GetString(responseBytes.ToArray(), 0, (int)responseBytes.Length);
            HtmlAgilityPack.HtmlDocument document = new HtmlAgilityPack.HtmlDocument();
            document.LoadHtml(response);
            HtmlNode node = null;
            HtmlNode secondaryNode = null;
            HtmlNode phoneticNode = null;
            try
            {
                node = document.DocumentNode.SelectSingleNode(HeadlineSelectionPath);
                secondaryNode = document.DocumentNode.SelectSingleNode(SecondaryLineSelectionPath);
                if (PhoneticSelectionPath != null)
                {
                    phoneticNode = document.DocumentNode.SelectSingleNode(PhoneticSelectionPath);
                }
                var response1 = new InterestContent()
                {
                    Title = HtmlDecodingUtility.HtmlDecode(node.InnerText),
                    Publisher = SourceName,
                    ContentUri = new Uri(RequestString)
                };
                response1.Content = HtmlDecodingUtility.HtmlDecode(secondaryNode.InnerText) + "  " + (phoneticNode == null ? string.Empty : HtmlDecodingUtility.HtmlDecode(phoneticNode.InnerText));
                return response1;
            }
            catch
            {
                return InterestContent.DefaultInterest;
            }
        }
        public OnlineSource()
        {
            //SourceName = "Bing";
            //this.RequestString = "http://cn.bing.com/dict/?mkt=zh-CN&setlang=ZH";
            //HeadlineSelectionPath = "//*[@id=\"sw_content\"]/div[2]/div/div[1]/div/div[2]/div[1]/div[1]/a";
            //SecondaryLineSelectionPath = "//*[@id=\"sw_content\"]/div[2]/div/div[1]/div/div[2]/div[1]/div[4]";
            //Content = ContentType.Word;
            //Language = LanguageType.EnUs;
            //TranslationLanguage = LanguageType.ZhCn;
        }
    }
    public class OfflineSource : LanguageSourceBase
    {
        public string SourcePath { get; set; }
        public int LineCount { get; set; }
        public override async Task<InterestContent> RequestContent(InterestRequest request)
        {
            InterestContent contract = new InterestContent();
#if WINDOWS_PHONE
            using (Stream stream = Application.GetResourceStream(new Uri(SourcePath, UriKind.Relative)).Stream)
#elif WINDOWS_APP
            using (Stream stream = await ((await StorageFile.GetFileFromApplicationUriAsync(new Uri($"ms-appx:///{SourcePath}"))).OpenStreamForReadAsync()))
#endif
            {
                StreamReader sr = new StreamReader(stream);
                var idx = ((int)DateTime.Now.TimeOfDay.TotalSeconds) % LineCount;
                for (int i = 0; i <= idx && !sr.EndOfStream; i++)
                {
                    var data = await sr.ReadLineAsync();
                    if (i == idx)
                    {
                        contract.Title = data.Split(' ')[0];
                        contract.Content = data.Substring(contract.Title.Length).Trim();
                        contract.Publisher = SourceName;
                    }
                }
            }
            return contract;
        }
        public override RequestMetaData GetMetaData()
        {
            return new RequestMetaData()
            {
                UpdatePerDay = 15,
                BytePerRequest = 0
            };
        }
    }
}
