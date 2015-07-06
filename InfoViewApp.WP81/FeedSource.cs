using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Web.Http;
using Windows.Web.Syndication;
using System;

namespace InfoViewApp.WP81.InterestGathering.NewsFeed
{
    public enum CategoryTopic
    {
        Technology,
        Finance,
        Stories,
        World,
        USA,
        Business,
        Politics,
        Health,
        Entertainment,
        Travel,
        Living,
        Video,
        Student,
        Customized
    }

    public class NewsFeedCategory : InterestGatherer
    {
        //public string SourceName { get; set; }
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
                HttpClient client = new HttpClient();
                var feed = new SyndicationFeed();
                feed.Load(await client.GetStringAsync(new System.Uri(XmlSource)));
                var items = feed.Items;
                var content = HtmlDecodingUtility.HtmlDecode(items[0].Summary.Text);
                var title = items[0].Title.Text;
                var publisher = SourceName; 
                var img=feed.ImageUri;
                return new InterestContent()
                {
                    Content = content,
                    Title = title,
                    Publisher = publisher,
                    ContentExtensionUri = new Uri(ExtendedContentUrl)
                };
            }
            catch
            {
                return InterestContent.DefaultInterest;
            }
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
    }

    public class FeedSources : List<FeedSource> { }
}
