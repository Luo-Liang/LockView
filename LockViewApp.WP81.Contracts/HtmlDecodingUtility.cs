using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;

namespace InfoViewApp.WP81.InterestGathering
{
    static class HtmlDecodingUtility
    {
        static Regex _htmlRegex = new Regex("<.*?>");
        public static string HtmlDecode(string value)
        {
            if (value == null) return null;

            int maxLength = 126;
            int strLength = 0;
            string fixedString = "";

            // Remove HTML tags and newline characters from the text, and decode HTML encoded characters. 
            // This is a basic method. Additional code would be needed to more thoroughly  
            // remove certain elements, such as embedded Javascript. 

            // Remove HTML tags. 
            fixedString = value.Replace("<![CDATA[", "").Replace("]]>", "");

            fixedString = Regex.Replace(fixedString.ToString(), "<[^>]+>", string.Empty);

            // Remove newline characters.
            fixedString = fixedString.Replace("\r", "").Replace("\n", "").Replace("\t", "");

            // Remove encoded HTML characters.
            fixedString = _htmlRegex.Replace(WebUtility.HtmlDecode(value), string.Empty);

            strLength = fixedString.ToString().Length;

            // Some feed management tools include an image tag in the Description field of an RSS feed, 
            // so even if the Description field (and thus, the Summary property) is not populated, it could still contain HTML. 
            // Due to this, after we strip tags from the string, we should return null if there is nothing left in the resulting string. 
            if (strLength == 0)
            {
                return null;
            }

            // Truncate the text if it is too long. 
            else if (strLength >= maxLength)
            {
                fixedString = fixedString.Substring(0, maxLength);

                // Unless we take the next step, the string truncation could occur in the middle of a word.
                // Using LastIndexOf we can find the last space character in the string and truncate there.
                var lastIdx = fixedString.LastIndexOf(" ");
                if (lastIdx >= 0)
                    fixedString = fixedString.Substring(0, lastIdx);
                fixedString = fixedString.Trim() + "...";
            }

            return fixedString;
        }
    }
}
