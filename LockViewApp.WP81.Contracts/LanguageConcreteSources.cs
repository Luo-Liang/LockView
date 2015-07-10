using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Windows;
using InfoViewApp.WP81;
using Windows.Storage;

namespace InfoViewApp.WP81.InterestGathering.LanguageLearning
{
    public class BingSource : LanguageSourceBase
    {
        public BingSource()
        {
            RefreshTimeInMinutes = 86400;
            SourceName = "Bing";
            this.RequestString = "http://cn.bing.com/dict/?mkt=zh-CN&setlang=ZH";
            HeadlineSelectionPath = "//*[@id=\"sw_content\"]/div[2]/div/div[1]/div/div[2]/div[1]/div[1]/a";
            SecondaryLineSelectionPath = "//*[@id=\"sw_content\"]/div[2]/div/div[1]/div/div[2]/div[1]/div[4]";
            Content = ContentType.Word;
            Language = LanguageType.EnUs;
            TranslationLanguage = LanguageType.ZhCn;
        }
    }

    public class ICIBASource : LanguageSourceBase
    {
        public ICIBASource()
        {
            RefreshTimeInMinutes = 86400;
            SourceName = "ICIBA";
            this.RequestString = "http://news.iciba.com/dailysentence";
            HeadlineSelectionPath = "//*[@id=\"dailyEcont\"]/div[2]/h5[1]/a[1]";
            SecondaryLineSelectionPath = "//*[@id=\"dailyEcont\"]/div[2]/h5[2]";
            Content = LanguageSourceBase.ContentType.Sentence;
            Language = LanguageType.EnUs;
            TranslationLanguage = LanguageType.ZhCn;
        }
    }

    public class MerriamWebsterSource : LanguageSourceBase
    {
        public MerriamWebsterSource()
        {
            RefreshTimeInMinutes = 86400;
            SourceName = "Merriam Webster";
            this.RequestString = "http://www.merriam-webster.com/word-of-the-day/";
            HeadlineSelectionPath = "//strong[@class=\"main_entry_word\"]";
            SecondaryLineSelectionPath = "//span[@class=\"ssens\"]";
            Content = LanguageSourceBase.ContentType.Word;
            Language = LanguageType.EnUs;
            TranslationLanguage = LanguageType.EnUs;
        }
    }

    public class OfflineSource : LanguageSourceBase
    {
        public string SourcePath;
        public int LineCount;
        public override async Task<InterestContent> RequestContent(InterestRequest request)
        {
            InterestContent contract = new InterestContent();
            var file = await ApplicationData.Current.LocalFolder.GetFileAsync(SourcePath);
            using (Stream stream = await file.OpenStreamForReadAsync())
            {
                StreamReader sr = new StreamReader(stream);
                var idx = ((int)DateTime.Now.TimeOfDay.TotalSeconds) % LineCount;
                for (int i = 0; i <= idx && !sr.EndOfStream; i++)
                {
                    var data = await sr.ReadLineAsync();
                    if (i == idx)
                    {
                        contract.Title = data.Split(' ')[0];
                        contract.Content = data.Substring(contract.Title.Length);
                        contract.Publisher = SourceName;
                    }
                }
            }
            request.PreviousInterestContentIdentifier = contract.GetHashCode();
            return contract;
        }
        public override RequestMetaData GetMetaData()
        {
            return new RequestMetaData()
            {
                UpdatePerDay = 15,
                BytePerRequest = 0
            };
        }
    }

    public class CET46Collection : OfflineSource
    {
        public CET46Collection()
        {
            SourceName = "CET 4/6 Collection";
            //  this.RequestString = "";
            Content = LanguageSourceBase.ContentType.Word;
            SourcePath = "wl.txt";
            LineCount = 699;
            Language = LanguageType.EnUs;
            TranslationLanguage = LanguageType.ZhCn;
        }
    }

    public class TOEFLCollection : OfflineSource
    {
        public TOEFLCollection()
        {
            SourceName = "TOEFL Collection";
            Content = ContentType.Word;
            SourcePath = "toefltxt.txt";
            LineCount = 2792;
            Language = LanguageType.EnUs;
            TranslationLanguage = LanguageType.ZhCn;
        }
    }

    public class GRECollection : OfflineSource
    {
        public GRECollection()
        {
            SourceName = "GRE Collection";
            Content = ContentType.Word;
            SourcePath = "gretxt.txt";
            LineCount = 7154;
            Language = LanguageType.EnUs;
            TranslationLanguage = LanguageType.ZhCn;
        }
    }
}
