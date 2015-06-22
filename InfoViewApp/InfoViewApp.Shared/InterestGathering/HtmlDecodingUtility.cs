using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;

namespace InfoViewApp.InterestGathering
{
    static class HtmlDecodingUtility
    {
        static Regex _htmlRegex = new Regex("<.*?>");
        public static string HtmlDecode(string content)
        {
            return _htmlRegex.Replace(WebUtility.HtmlDecode(content), string.Empty);
        }
    }
}
