using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace InfoViewApp.InterestGathering
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
        public override int GetHashCode()
        {
            return Title.GetHashCode() ^ Content.GetHashCode() ^ Publisher.GetHashCode();
        }
    }



    public class InterestRequest
    {
        public string InterestString { get; set; }
        public int PreviousInterestContentIdentifier { get; set; }
    }

    public interface IInterestGatherer
    {
        Task<InterestContent> RequestContent(InterestRequest request);
        string SourceName { get; }
    }

    class InterestGatherer : IInterestGatherer
    {
        public string BaseRequestUrlTemplate = "INVALID";
        public string SourceName { get; set; }
        public async Task<InterestContent> RequestContent(InterestRequest request)
        {
            return InterestContent.DefaultInterest;
        }
    }
}
