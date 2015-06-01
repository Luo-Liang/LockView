using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace InfoView.DataContract
{
    [DataContract]
    public struct OverlayFormattingContract
    {
        [DataMember]
        public string TitleFont { get; set; }
        [DataMember]
        public string ForegroundTitle { get; set; }
        [DataMember]
        public string BackgroundTitle { get; set; }
        [DataMember]
        public string FirstLineFont { get; set; }
        [DataMember]
        public string ForegroundFirstLine { get; set; }
        [DataMember]
        public string BackgroundFirstLine { get; set; }
        [DataMember]
        public string SecondLineFont { get; set; }
        [DataMember]
        public string ForegroundSecondLine { get; set; }
        [DataMember]
        public string BackgroundSecondLine { get; set; }
    }

    [DataContract]
    public struct OverlayContextContract
    {
        [DataMember]
        public string Title { get; set; }
        [DataMember]
        public string FirstLine { get; set; }
        [DataMember]
        public string SecondLine { get; set; }

    }
    [DataContract]
    public struct OverlayBehavior
    {
        [DataMember]
        public bool AutoExpand { get; set; }
        [DataMember]
        public Point Origin { get; set; }
    }
}