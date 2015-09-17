using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Web.Http;
using Windows.Web.Syndication;
using System;
using System.Text;

namespace InfoViewApp.WP81.InterestGathering.NewsFeed
{
    public enum CategoryTopic
    {
        Technology,
        Finance,
        Stories,
        World,
        Domestic,
        Business,
        Politics,
        Health,
        Entertainment,
        Travel,
        Living,
        Video,
        Student,
        Customized,
        Sports,
        History,
        Military,
        Society,
        Movie,
        Reading,
        Hotpicks,
        Cooking,
    }

    public class NewsFeedCategory : InterestGatherer
    {
        public CategoryTopic Topic { get; set; }
        public string XmlSource { get; set; }
        public NewsFeedCategory() { }
        public NewsFeedCategory(CategoryTopic topic, string xmlAddr)
        {
            Topic = topic;
            XmlSource = xmlAddr;
        }
        public override async Task<InterestContent> RequestContent(InterestRequest request)
        {
            try
            {
                HttpClient client = Client == null ? new HttpClient() : Client;
                var feed = new SyndicationFeed();
                var responsehrss = await client.GetStringAsync(new System.Uri(XmlSource));
                feed.Load(responsehrss);
                var items = feed.Items;
                string content = items[0].Summary.Text;
                string title = items[0].Title.Text;
                content= HtmlDecodingUtility.HtmlDecode(content);
                title =HtmlDecodingUtility.HtmlDecode(title);
                
                var publisher = SourceName; 
                var img=feed.ImageUri;
                var response = new InterestContent()
                {
                    Content = content,
                    Title = title,
                    Publisher = publisher,
                    ExtensionUri = new Uri(ExtendedContentUrl),
                };
                if (string.IsNullOrEmpty(items[0].Id) == false)
                {
                    response.ContentUri = new Uri(items[0].Id);
                }
                return response;
            }
            catch (Exception ex)
            {
                return InterestContent.DefaultInterest;
            }
        }

        public override RequestMetaData GetMetaData()
        {
            return new RequestMetaData()
            {
                BytePerRequest = 1024,
                UpdatePerDay = 15
            };
        }
    }

    public class CustomizedFeedSource : FeedSource 
    {
        public CustomizedFeedSource():base()
        {
            FeedContentProviders = new List<InterestGatherer>();
        }
    }

    public class FeedSource
    {
        public override string ToString()
        {
            return SourceName;
        }
        public string SourceName { get; set; }
        public List<InterestGatherer> FeedContentProviders { get; set; }
        public FeedSource()
        {
            FeedContentProviders = new List<InterestGatherer>();
        }
        public string ApplicableLanguageId { get; set; }
    }

    public class FeedSources : List<FeedSource> { }
}
