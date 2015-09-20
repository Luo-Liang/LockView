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
            var response = await client.GetStringAsync(new System.Uri(RequestString));
            HtmlAgilityPack.HtmlDocument document = new HtmlAgilityPack.HtmlDocument();
            document.LoadHtml(response);
            HtmlNode node = null;
            HtmlNode secondaryNode = null;
            HtmlNode phoneticNode = null;
            try
            {
                node = document.DocumentNode.SelectSingleNode(HeadlineSelectionPath);
                secondaryNode = document.DocumentNode.SelectSingleNode(SecondaryLineSelectionPath);
                if(PhoneticSelectionPath != null)
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
            using (Stream stream = Application.GetResourceStream(new Uri(SourcePath, UriKind.Relative)).Stream)
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
