using System;
using System.Collections.Generic;
using System.Text;

namespace InfoViewApp.InterestGathering
{
    public class InterestContent
    {
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

    interface IInterestGatherer
    {
        InterestContent RequestContent(InterestRequest request);
    }

    class InterestGatherer : IInterestGatherer
    {
        public string BaseRequestUrlTemplate = "INVALID";
        public InterestContent RequestContent(InterestRequest request)
        {
            return null;
        }
    }
}
