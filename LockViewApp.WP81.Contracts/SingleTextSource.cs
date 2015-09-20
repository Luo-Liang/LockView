using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using InfoViewApp.WP81.InterestGathering;

namespace LockViewApp.WP81.Contracts
{
    public class SingleTextSource : InfoViewApp.WP81.InterestGathering.InterestGatherer
    {
        public string FirstLine { get; set; } = "";
        public string Title { get; set; } = "";
        public string SecondLine { get; set; } = "";
        public override RequestMetaData GetMetaData()
        {
            return new RequestMetaData()
            {
                BytePerRequest = FirstLine.Length + Title.Length + SecondLine.Length,
                UpdatePerDay = 1,
                TypicalComputationInSec = 5
            };
        }

        public async override Task<InterestContent> RequestContent(InterestRequest request)
        {
            return new InterestContent()
            {
                Content = FirstLine,
                Title = Title,
                Publisher = SecondLine
            };
        }

        public SingleTextSource()
        {
        }
    }
}
