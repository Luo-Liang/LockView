using InfoView.DataContract;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;
using System.Threading.Tasks;

namespace InfoView
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the interface name "IService1" in both code and config file together.
    [ServiceContract]
    public interface IImageCompositionService
    {
        [OperationContract]
        [WebInvoke(
        RequestFormat = WebMessageFormat.Json,
        ResponseFormat = WebMessageFormat.Json, Method = "POST")]
        Task<ImageCompositionResponse> Compose(ImageCompositionRequest request);

        [OperationContract]
        [WebInvoke(
        RequestFormat = WebMessageFormat.Json,
        ResponseFormat = WebMessageFormat.Json, Method = "POST")]
        Task<ImageCompositionResponse> ComposeV2(ImageCompositionRequestV2 request);

        [Obsolete]
        [OperationContract]
        [WebInvoke(
        RequestFormat = WebMessageFormat.Json,
        ResponseFormat = WebMessageFormat.Json, Method = "POST")]
        Task<string> ComposeLegacy(string request);
    }

    // Use a data contract as illustrated in the sample below to add composite types to service operations.
    [DataContract]
    public class ImageCompositionResponse
    {
        [DataMember]
        public byte[] Image { get; set; }
        [DataMember]
        public string ResultString { get; set; }
    }

    [DataContract, Newtonsoft.Json.JsonObject(MemberSerialization = Newtonsoft.Json.MemberSerialization.OptIn)]
    public class ImageRequestOverride
    {
        [DataMember]
        public string ImageRequestUrl { get; set; }
        [DataMember]
        public string Arguments { get; set; }
    }

    
     [DataContract] 
     public class ImageCompositionRequest
     { 
         [DataMember] 
         public byte[] RawImage { get; set; } 
         [DataMember] 
         public ImageRequestOverride ImageRequestOverride { get; set; } 
         [DataMember] 
         public OverlayFormattingContract FormattingContract { get; set; } 
         [DataMember] 
         public OverlayLayoutContract LayoutContract { get; set; } 
         [DataMember] 
         public OverlayContextContract ContextContract { get; set; } 
     }


public class ImageCompositionRequestV2
    {
        [DataMember]
        public byte[] RawImage { get; set; }
        [DataMember]
        public ImageRequestOverride ImageRequestOverride { get; set; }
        [DataMember]
        public OverlayFormattingContract FormattingContract { get; set; }
        [DataMember]
        public OverlayLayoutContract LayoutContract { get; set; }
        [DataMember]
        public OverlayContextContract[] ContextContracts { get; set; }
    }
}
