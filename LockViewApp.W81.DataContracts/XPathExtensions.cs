using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace intelliSys.XPath
{
    public static class XPathExtensions
    {
        [Flags]
        enum XPathParsingStates
        {
            TokenUnknown = 1,
            TokenEmpty = 2,
            TokenAsFirstGenNode = 4,
            TokenAsAttribute = 8,
            TokenAsIndex = 16,
            TokenAsAttributeValue = 32,
            TokenAsAllDescendants = 64
        }

        static XPathExtensions()
        {
            StateModificationLookup = new Dictionary<char, XPathParsingStates>();
            StateModificationLookup['['] = XPathParsingStates.TokenAsIndex;
            StateModificationLookup[']'] = XPathParsingStates.TokenEmpty;
            StateModificationLookup['/'] = XPathParsingStates.TokenAsFirstGenNode;
            StateModificationLookup['~'] = XPathParsingStates.TokenAsAllDescendants;
            StateModificationLookup['@'] = XPathParsingStates.TokenAsAttribute;
            StateModificationLookup['='] = XPathParsingStates.TokenAsAttributeValue;
        }

        static Dictionary<char, XPathParsingStates> StateModificationLookup;
        static string TokenStatusRedefinition = "[~]/@=";
        /// <summary>
        /// Can only deals with basic XPath queries. Not a complete XPath parser yet. Omit multi node selection now - it would be a easy tweak.
        /// </summary>
        /// <param name="node"></param>
        /// <param name="Xpath"></param>
        /// <returns></returns>
        public static IEnumerable<HtmlNode> SelectMultipleNodes(this HtmlNode node, string Xpath)
        {
            Xpath = Xpath.Replace("//", "~");//Let's not deal with double-char modifiers
            var currentToken = new StringBuilder();
            XPathParsingStates currStatus = XPathParsingStates.TokenEmpty;
            //Essentially we want the leaf nodes of the Xpath tree.
            IEnumerable<IEnumerable<HtmlNode>> temporaryResults = new List<List<HtmlNode>>() { new List<HtmlNode>() { node } };
            var lastToken = string.Empty;
            bool ignoreModifier = false;
            IEnumerable<IEnumerable<HtmlNode>> __DebugbackTrack = null;
            for (int i = 0; i < Xpath.Length; i++)
            {
                var currChar = Xpath[i];
                if (TokenStatusRedefinition.Contains(currChar) && !ignoreModifier)
                {
                    var token = currentToken.ToString();
                    XPathParsingStates status = currStatus;
                    status = status & ~XPathParsingStates.TokenEmpty;
                    switch (status)
                    {
                        //Token empty allow fallbaack.
                        case XPathParsingStates.TokenAsAttribute:
                            temporaryResults = temporaryResults.Select(o => o.Where(n => n.Attributes.FirstOrDefault(attr => attr.Name == token) != null));
                            break;
                        case XPathParsingStates.TokenAsFirstGenNode:
                            if (token != "*")
                                temporaryResults = temporaryResults.Select(o => o.SelectMany(n => n.Elements(token)));
                            else
                                temporaryResults = temporaryResults.Select(o => o.SelectMany(n => n.ChildNodes));
                            //needs to flatten
                            break;
                        case XPathParsingStates.TokenAsAllDescendants:
                            if (token != "*")
                                temporaryResults = temporaryResults.Select(o => o.SelectMany(n => n.Descendants(token)));
                            else
                                temporaryResults = temporaryResults.Select(o => o.SelectMany(n => n.Descendants()));
                            //needs to flatten
                            break;
                        case XPathParsingStates.TokenAsIndex:
                            if (token.Length > 0)
                                //this where problems come.
                                //it is possible the actual Parsing State is not updated yet. e.g. in [@id="MSFT"], token is "".
                                temporaryResults = temporaryResults.Select(o => o.Where((n, idx) => (idx + 1) == int.Parse(token)));
                            break;
                        case XPathParsingStates.TokenAsAttributeValue:
DEBUGLABEL:
                            var localTokenCopy = lastToken;
                            temporaryResults = temporaryResults.Select(o => o.Where(n => n.Attributes[localTokenCopy].Value == token.Trim('\"')));
                            break;
                    }
                    if (currChar == '/' || currChar == '~')
                    {
                        //flatten expression tree to a flat tree.
                        temporaryResults = temporaryResults.SelectMany(o => new[] { o });
                    }
                    if (temporaryResults.SelectMany(o => o).Count() == 0) break;
                    __DebugbackTrack = temporaryResults;
                    if (currentToken.Length != 0)
                        lastToken = currentToken.ToString();
                    currentToken.Clear();
                    currStatus = StateModificationLookup[currChar];

                }
                else
                {
                    if (currChar == '"') ignoreModifier = !ignoreModifier;
                    currentToken.Append(currChar);
                }
            }
            return temporaryResults.SelectMany(o => o);
        }

        public static HtmlNode SelectSingleNode(this HtmlNode node, string Xpath)
        {
            return SelectMultipleNodes(node, Xpath).FirstOrDefault();
        }
    }
}
