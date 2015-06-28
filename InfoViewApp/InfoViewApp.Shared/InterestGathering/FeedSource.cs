using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Windows.Web.Syndication;

namespace InfoViewApp.InterestGathering.NewsFeed
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
        Student
    }

    public class NewsFeedCategory : IInterestGatherer
    {
        public string SourceName { get; set; }
        public CategoryTopic Topic { get; set; }
        public string XmlSource { get; set; }
        public NewsFeedCategory() { }
        public NewsFeedCategory(CategoryTopic topic, string xmlAddr)
        {
            Topic = topic;
            XmlSource = xmlAddr;
        }
        public async Task<InterestContent> RequestContent(InterestRequest request)
        {
            try
            {
                HttpClient client = new HttpClient();
                var feed = new SyndicationFeed();
                feed.Load(await client.GetStringAsync(XmlSource));
                var items = feed.Items;
                var content = items[0].Summary.Text;
                var title = items[0].Title.Text;
                var publisher = SourceName;
                return new InterestContent()
                {
                    Content = content,
                    Title = title,
                    Publisher = publisher
                };
            }
            catch
            {
                return InterestContent.DefaultInterest;
            }
        }
    }

    public class CustomizedFeedSource : FeedSource { }

    public class FeedSource
    {
        public override string ToString()
        {
            return Name;
        }
        public string Name { get; set; }
        public IList<IInterestGatherer> FeedContentProviders { get; set; }
        public FeedSource()
        {
            FeedContentProviders = new List<IInterestGatherer>();
        }
    }

    public class FeedSources : List<FeedSource> { }
}
