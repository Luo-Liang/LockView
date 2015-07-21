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
        static class HtmlDecodingUtility
        {
            static Regex _htmlRegex = new Regex("<.*?>");
            public static string HtmlDecode(string value)
            {
                if (value == null) return null;
                string fixedString = "";
                fixedString = value.Replace("<![CDATA[", "").Replace("]]>", "");
                fixedString = Regex.Replace(fixedString.ToString(), "<[^>]+>", string.Empty);
                fixedString = fixedString.Replace("\r", "").Replace("\n", "").Replace("\t", "");
                fixedString = _htmlRegex.Replace(WebUtility.HtmlDecode(value), string.Empty);
                return fixedString;
            }
        }
        static WebClient requestClient = new WebClient();
        static Regex _htmlRegex = new Regex("<.*?>");
        public async Task<string> FulfillRequestSimple(string requestString)
        {
            var raw = requestString.Trim('"');
            var reqUri = new Uri(raw);
            var response = await requestClient.DownloadDataTaskAsync(reqUri);
            var strResponse = Encoding.UTF8.GetString(response);
            strResponse = strResponse.Replace("\\u0026quot;", "").Replace("&quot;","");
            strResponse = WebUtility.HtmlDecode(HtmlDecodingUtility.HtmlDecode(Regex.Unescape(strResponse)));
            return strResponse;
        }
    }
}
