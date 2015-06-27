using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Windows;
using InfoViewApp;
using Windows.Storage;

namespace InfoViewApp.InterestGathering
{
    public class BingSource : LanguageSourceBase
    {
        public BingSource()
        {
            RefreshTimeInMinutes = 86400;
            Name = "Bing (CHN) word of the day";
            this.RequestString = "http://cn.bing.com/dict/?mkt=zh-CN&setlang=ZH";
            HeadlineSelectionPath = "//*[@id=\"sw_content\"]/div[2]/div/div[1]/div/div[2]/div[1]/div[1]/a";
            SecondaryLineSelectionPath = "//*[@id=\"sw_content\"]/div[2]/div/div[1]/div/div[2]/div[1]/div[4]";
            Content = ContentType.Word;
        }
    }

    public class ICIBASource : LanguageSourceBase
    {
        public ICIBASource()
        {
            RefreshTimeInMinutes = 86400;
            Name = "ICIBA sentence of the day";
            this.RequestString = "http://news.iciba.com/dailysentence";
            HeadlineSelectionPath = "//*[@id=\"dailyEcont\"]/div[2]/h5[1]/a[1]";
            SecondaryLineSelectionPath = "//*[@id=\"dailyEcont\"]/div[2]/h5[2]";
            Content = LanguageSourceBase.ContentType.Sentence;
        }
    }

    public class MerriamWebsterSource : LanguageSourceBase
    {
        public MerriamWebsterSource()
        {
            RefreshTimeInMinutes = 86400;
            Name = "merriam webster word of the day";
            this.RequestString = "http://www.merriam-webster.com/word-of-the-day/";
            HeadlineSelectionPath = "//strong[@class=\"main_entry_word\"]";
            SecondaryLineSelectionPath = "//span[@class=\"ssens\"]";
            Content = LanguageSourceBase.ContentType.Word;
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
                        contract.Publisher = Name;
                    }
                }
            }
            return contract;
        }
    }

    public class CET46Collection : OfflineSource
    {
        public CET46Collection()
        {
            Name = "CET 4/6 Collection";
            //  this.RequestString = "";
            Content = LanguageSourceBase.ContentType.Word;
            SourcePath = "wl.txt";
            LineCount = 699;
        }
    }

    public class TOEFLCollection : OfflineSource
    {
        public TOEFLCollection()
        {
            Name = "TOEFL Collection";
            Content = ContentType.Word;
            SourcePath = "toefltxt.txt";
            LineCount = 2792;
        }
    }

    public class GRECollection : OfflineSource
    {
        public GRECollection()
        {
            Name = "GRE Collection";
            Content = ContentType.Word;
            SourcePath = "gretxt.txt";
            LineCount = 7154;
        }
    }
}
