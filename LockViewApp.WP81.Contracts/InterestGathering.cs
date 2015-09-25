using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Windows.Web.Http;

namespace InfoViewApp.WP81.InterestGathering
{
    public class InterestContent
    {
        public static readonly InterestContent DefaultInterest = new InterestContent()
        {
            Title = "No result",
            Content = "This interest is not available right now. Check that your internet is working, or change a phrase and try again.",
            Publisher = "Invalid Request"
        };
        public string Title { get; set; }
        public string Content { get; set; }
        public string Publisher { get; set; }
        public Uri ExtensionUri { get; set; }
        public Uri ContentUri { get; set; }
        public override int GetHashCode()
        {
            return Title.GetHashCode() ^ Content.GetHashCode() ^ Publisher.GetHashCode();
        }
    }




    public class InterestRequest
    {
        public string InterestString { get; set; }
    }

    public interface IInterestGatherer
    {
        Task<InterestContent> RequestContent(InterestRequest request);
        string SourceName { get; }
        RequestMetaData GetMetaData();
    }

    public class RequestMetaData
    {
        public int BytePerRequest { get; set; }
        public int UpdatePerDay { get; set; }
        public int TypicalComputationInSec { get; set; }
        public RequestMetaData()
        {
            TypicalComputationInSec = 5;
        }
    }

    public abstract class InterestGatherer : IInterestGatherer
    {
        [XmlIgnore]
        /// <summary>
        /// Reuse the same client if possible to save memory on Low end devices.
        /// </summary>
        public HttpClient Client { get; set; }
        public string BaseRequestUrlTemplate = "INVALID";
        /// <summary>
        /// Usually, this is a image of the source.
        /// </summary>
        public string ExtendedContentUrl { get; set; }
        public string SourceName { get; set; }
        public abstract Task<InterestContent> RequestContent(InterestRequest request);
        public abstract RequestMetaData GetMetaData();
        public string ApplicableCulture { get; set; }
    }
}
