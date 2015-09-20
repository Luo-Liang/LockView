using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Data.Xml.Dom;
using Windows.Web.Http;

namespace InfoViewApp.WP81.InterestGathering.LanguageLearning
{
    public class LanguegeSources : List<LanguageSourceBase>
    {

    }
    public class LanguageSourceBase : InterestGathering.InterestGatherer
    {
        public enum ContentType
        {
            Word,
            Sentence
        }

        public enum LanguageType
        {
            ZhCn,
            EnUs
        }

        public ContentType Content
        {
            get;
            set;
        }
        public LanguageType Language { get; set; }
        public LanguageType TranslationLanguage { get; set; }
        public override string ToString()
        {
            return SourceName;
        }

        public string RequestString
        {
            get;
            set;
        }



        public override RequestMetaData GetMetaData()
        {
            return new RequestMetaData()
            {
                BytePerRequest = 5 * 1024,
                UpdatePerDay = 1
            };
        }

        public override Task<InterestContent> RequestContent(InterestRequest request)
        {
            throw new NotImplementedException();
        }
    }
}
