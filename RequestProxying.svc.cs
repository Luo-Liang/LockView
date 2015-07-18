using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace InfoView
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "RequestProxying" in code, svc and config file together.
    // NOTE: In order to launch WCF Test Client for testing this service, please select RequestProxying.svc or RequestProxying.svc.cs at the Solution Explorer and start debugging.
    public class RequestProxying : IRequestProxying
    {
        static WebClient requestClient = new WebClient();
        static Regex _htmlRegex = new Regex("<.*?>");
        public async Task<string> FulfillRequestSimple(string requestString)
        {
            var response = await requestClient.DownloadStringTaskAsync(requestString.Trim('"'));
            response = WebUtility.HtmlDecode(Regex.Unescape(Regex.Replace(response, "<[^>]+>", string.Empty)));
            return response;
        }
    }
}
