using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Data.Xml.Dom;
using Windows.Web.Http;

namespace InfoViewApp.WP81.InterestGathering.LanguageLearning
{
    public class LanguegeSources : List<LanguageSourceBase>
    {

    }
    public class LanguageSourceBase : InterestGathering.InterestGatherer
    {
        public enum ContentType
        {
            Word,
            Sentence
        }

        public enum LanguageType
        {
            ZhCn,
            EnUs
        }

        public int RefreshTimeInMinutes
        {
            get;
            set;
        }
        public ContentType Content
        {
            get;
            set;
        }
        public LanguageType Language { get; set; }
        public LanguageType TranslationLanguage { get; set; }
        public override string ToString()
        {
            return SourceName;
        }

        public string SourceName
        {
            get;
            set;
        }

        public string RequestString
        {
            get;
            set;
        }

        public string HeadlineSelectionPath { get; set; }

        public string SecondaryLineSelectionPath { get; set; }

        public override async Task<InterestContent> RequestContent(InterestRequest request)
        {
            HttpClient client = Client == null ? new HttpClient() : Client;
            var response = await client.GetStringAsync(new System.Uri(RequestString));
            XmlDocument document = new XmlDocument();
            document.LoadXml(response);
            IXmlNode node = null;
            IXmlNode secondaryNode = null;
            try
            {
                node = document.SelectSingleNode(HeadlineSelectionPath);
                secondaryNode = document.SelectSingleNode(SecondaryLineSelectionPath);
                var response1 = new InterestContent()
                {
                    Title = HtmlDecodingUtility.HtmlDecode(node.InnerText),
                    Content = HtmlDecodingUtility.HtmlDecode(node.InnerText),
                    Publisher = SourceName,
                    ContentUri = new Uri(RequestString)
                };
                return response1;
            }
            catch
            {
                return InterestContent.DefaultInterest;
            }
        }

        public override RequestMetaData GetMetaData()
        {
            return new RequestMetaData()
            {
                BytePerRequest = 50 * 1024,
                UpdatePerDay = 1
            };
        }
    }
}
