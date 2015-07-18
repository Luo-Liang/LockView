using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;
using System.Threading.Tasks;

namespace InfoView
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the interface name "IRequestProxying" in both code and config file together.
    [ServiceContract]
    public interface IRequestProxying
    {
        [OperationContract]
        [WebInvoke(
        RequestFormat = WebMessageFormat.Json,
        ResponseFormat = WebMessageFormat.Json, Method = "POST")]
        Task<string> FulfillRequestSimple(string requestString);
    }
}
