using InfoViewApp.WP81.InterestGathering;
using InfoViewApp.WP81.InterestGathering.LanguageLearning;
using InfoViewApp.WP81.InterestGathering.NewsFeed;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Windows.Storage;
using Windows.Storage.Streams;

namespace InfoViewApp.WP81
{
    public class LockViewApplicationState
    {
        const string SettingInstance = "Settings.xml";
        public static LockViewApplicationState Instance { get; private set; }
        static LockViewApplicationState()
        {
            Instance = new LockViewApplicationState();
        }
        private LockViewApplicationState()
        {
            try
            {
                XmlSerializer serializer = new XmlSerializer(typeof(LockViewApplicationState), new[] { typeof(LanguageSourceBase), typeof(NewsFeedCategory), typeof(InterestGatherer) });
                using (var fs = ApplicationData.Current.LocalFolder.OpenStreamForReadAsync(SettingInstance).Result)
                {
                    XmlSerializer xmlSerializer = new XmlSerializer(typeof(LockViewApplicationState), new[] { typeof(InterestGatherer) });
                    Instance = (LockViewApplicationState)xmlSerializer.Deserialize(fs);
                }
            }
            catch
            {
                PersistFileName = "MyBg.jpeg";
                PreviewFormattingContract = new OverlayFormattingContract()
                {
                    BackgroundSecondLine = "Transparent",
                    BackgroundFirstLine = "Transparent",
                    BackgroundTitle = "Transparent",
                    FirstLineFont = new FontContract() { FontSize = 18, FontFamily = "Segoe UI" },
                    SecondLineFont = new FontContract() { FontSize = 14, FontFamily = "Segoe UI" },
                    TitleFont = new FontContract() { FontSize = 24, FontFamily = "Segoe UI" },
                    ForegroundFirstLine = "White",
                    ForegroundSecondLine = "Gray",
                    ForegroundTitle = "White"
                };

                PreviewContextContract = new OverlayContextContract();
                PreviewLayoutContract = new OverlayLayoutContract();
            }
        }
        Stream BackgroundPreview { get; set; }
        public string PersistFileName { get; set; }
        public InterestGathering.InterestGatherer SelectedProvider { get; set; }
        public OverlayContextContract PreviewContextContract { get; set; }
        public OverlayFormattingContract PreviewFormattingContract { get; set; }
        public OverlayLayoutContract PreviewLayoutContract { get; set; }
        public async Task SaveState()
        {
            var file = await ApplicationData.Current.LocalFolder.CreateFileAsync(SettingInstance, CreationCollisionOption.ReplaceExisting);
            using (var fs = await file.OpenStreamForWriteAsync())
            {
                XmlSerializer xmlSerializer = new XmlSerializer(typeof(LockViewApplicationState),new[] {typeof(InterestGatherer)});
                xmlSerializer.Serialize(fs, this);
            }
        }
    }
}
