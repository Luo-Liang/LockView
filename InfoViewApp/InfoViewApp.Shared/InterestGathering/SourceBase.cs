using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Net;
using System.Xml;
using Windows.Data.Xml.Dom;

namespace InfoViewApp.InterestGathering
{
    public class LanguageSourceBase : IInterestGatherer
    {
        public enum ContentType
        {
            Word,
            Sentence
        }
        public int RefreshTimeInMinutes
        {
            get;
            set;
        }
        public ContentType Content
        {
            get;
            set;
        }

        public override string ToString()
        {
            return Name;
        }

        static string FixString(string value)
        {
            if (value == null) return null;

            int maxLength = 200;
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
            fixedString = InfoViewApp.InterestGathering.HtmlDecodingUtility.HtmlDecode(fixedString);

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
                fixedString = fixedString.Substring(0, fixedString.LastIndexOf(" "));
            }

            fixedString += "...";

            return fixedString;
        }

        public string Name
        {
            get;
            set;
        }

        public string RequestString
        {
            get;
            set;
        }

        public string HeadlineSelectionPath { get; set; }

        public string SecondaryLineSelectionPath { get; set; }

        public virtual async Task<InterestContent> RequestContent(InterestRequest request)
        {
            XmlDocument document = await XmlDocument.LoadFromUriAsync(new Uri(RequestString));
            IXmlNode node = null;
            IXmlNode secondaryNode = null;
            try
            {
                node = document.SelectSingleNode(HeadlineSelectionPath);
                secondaryNode = document.SelectSingleNode(SecondaryLineSelectionPath);
                return new InterestContent()
                {
                    Title = FixString(node.InnerText),
                    Content = FixString(node.InnerText),
                    Publisher = Name
                };
            }
            catch
            {
                return InterestContent.DefaultInterest;
            }
        }


    }
}
