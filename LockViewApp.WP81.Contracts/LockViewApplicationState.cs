﻿using InfoViewApp.WP81.InterestGathering;
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
    public class LockViewRequestMetadata
    {
        public string RequestLanguage = "En-Us";
        public int ImageBytesPerRequest = 1024;
    }


    public class LockViewApplicationState
    {
        const string SettingInstance = "Settings.xml";
        public ImageSource SelectedImageSource { get; set; }
        public double UserQuotaInDollars { get; set; }
        public LockViewRequestMetadata RequestMetadata { get; set; }
        public static LockViewApplicationState Instance { get; private set; }
        static LockViewApplicationState()
        {
            Instance = new LockViewApplicationState();
        }
        private LockViewApplicationState()
        {
            try
            {
                using (var fs = ApplicationData.Current.LocalFolder.OpenStreamForReadAsync(SettingInstance).Result)
                {
                    XmlSerializer xmlSerializer = new XmlSerializer(typeof(LockViewApplicationState), new[] { typeof(LanguageSourceBase), typeof(NewsFeedCategory), typeof(InterestGatherer) });
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
                    FirstLineFont = new FontContract() { FontSize = 18, FontFamily = "Segoe UI Semibold" },
                    SecondLineFont = new FontContract() { FontSize = 14, FontFamily = "Segoe UI Semibold" },
                    TitleFont = new FontContract() { FontSize = 22, FontFamily = "Segoe UI Semibold" },
                    ForegroundFirstLine = "White",
                    ForegroundSecondLine = "Gray",
                    ForegroundTitle = "White"
                };

                PreviewContextContract = new OverlayContextContract();
                PreviewLayoutContract = new OverlayLayoutContract();
                RequestMetadata = new LockViewRequestMetadata();
                UserQuotaInDollars = 0.19;
                SelectedInterest = new InterestRequest();
            }
        }
        Stream BackgroundPreview { get; set; }
        public string PersistFileName { get; set; }
        public InterestRequest SelectedInterest { get; set; }
        public InterestGathering.InterestGatherer SelectedProvider { get; set; }
        public OverlayContextContract PreviewContextContract { get; set; }
        public OverlayFormattingContract PreviewFormattingContract { get; set; }
        public OverlayLayoutContract PreviewLayoutContract { get; set; }
        public async Task SaveState()
        {
            var file = await ApplicationData.Current.LocalFolder.CreateFileAsync(SettingInstance, CreationCollisionOption.ReplaceExisting);
            using (var fs = await file.OpenStreamForWriteAsync())
            {
                XmlSerializer xmlSerializer = new XmlSerializer(typeof(LockViewApplicationState), new[] { typeof(InterestGatherer),typeof(NewsFeedCategory) });
                xmlSerializer.Serialize(fs, this);
            }
        }
    }
}