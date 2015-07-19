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
    public class OnlineSource : LanguageSourceBase
    {
        public OnlineSource()
        {
            //SourceName = "Bing";
            //this.RequestString = "http://cn.bing.com/dict/?mkt=zh-CN&setlang=ZH";
            //HeadlineSelectionPath = "//*[@id=\"sw_content\"]/div[2]/div/div[1]/div/div[2]/div[1]/div[1]/a";
            //SecondaryLineSelectionPath = "//*[@id=\"sw_content\"]/div[2]/div/div[1]/div/div[2]/div[1]/div[4]";
            //Content = ContentType.Word;
            //Language = LanguageType.EnUs;
            //TranslationLanguage = LanguageType.ZhCn;
        }
    }
    public class OfflineSource : LanguageSourceBase
    {
        public string SourcePath { get; set; }
        public int LineCount { get; set; }
        public override async Task<InterestContent> RequestContent(InterestRequest request)
        {
            InterestContent contract = new InterestContent();
            using (Stream stream = Application.GetResourceStream(new Uri(SourcePath, UriKind.Relative)).Stream)
            {
                StreamReader sr = new StreamReader(stream);
                var idx = ((int)DateTime.Now.TimeOfDay.TotalSeconds) % LineCount;
                for (int i = 0; i <= idx && !sr.EndOfStream; i++)
                {
                    var data = await sr.ReadLineAsync();
                    if (i == idx)
                    {
                        contract.Title = data.Split(' ')[0];
                        contract.Content = data.Substring(contract.Title.Length).Trim();
                        contract.Publisher = SourceName;
                    }
                }
            }
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
}
