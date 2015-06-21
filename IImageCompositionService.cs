using InfoView.DataContract;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;

namespace InfoView
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the interface name "IService1" in both code and config file together.
    [ServiceContract]
    public interface IImageCompositionService
    {
        [OperationContract]
        [WebInvoke(UriTemplate = "/Compose",
        RequestFormat = WebMessageFormat.Json,
        ResponseFormat = WebMessageFormat.Json, Method = "POST")]
        ImageCompositionResponse Compose(ImageCompositionRequest request);

        [Obsolete]
        [OperationContract]
        [WebInvoke(UriTemplate = "/ComposeLegacy",
        RequestFormat = WebMessageFormat.Json,
        ResponseFormat = WebMessageFormat.Json, Method = "POST")]
        string ComposeLegacy(string request);
    }

    [DataContract]
    public enum CompositionResult
    {
        [EnumMember]
        None,
        [EnumMember]
        Changed,
        [EnumMember]
        Unchanged,
        [EnumMember]
        Failed
    }

    // Use a data contract as illustrated in the sample below to add composite types to service operations.
    [DataContract]
    public class ImageCompositionResponse
    {
        [DataMember]
        public byte[] Image { get; set; }

        [DataMember]
        public CompositionResult Result { get; set; }
    }

    [DataContract]
    public class ImageCompositionRequest
    {
        [DataMember]
        public string InterestId { get; set; }
        [DataMember]
        public byte[] RawImage { get; set; }
        [DataMember]
        public long UserId { get; set; } //may be used for persistence in future
        [DataMember]
        public long RequestId { get; set; }
        [DataMember]
        public OverlayFormattingContract FormattingContract { get; set; }
        [DataMember]
        public OverlayLayoutContract LayoutContract { get; set; }
        [DataMember]
        public OverlayContextContract ContextContract { get; set; }
    }
}
