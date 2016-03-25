using InfoViewApp.WP81.InterestGathering;
using InfoViewApp.WP81.InterestGathering.LanguageLearning;
using InfoViewApp.WP81.InterestGathering.NewsFeed;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Streams;
using Newtonsoft.Json;
using System.Xml.Serialization;
using LockViewApp.WP81.Contracts;
using InfoViewApp.WP81;

namespace InfoViewApp.WP81
{
    [XmlInclude(typeof(NewsFeedCategory))]
    [XmlInclude(typeof(GoogleSpecificInterestGatherer))]
    [XmlInclude(typeof(SingleTextSource))]
    [XmlInclude(typeof(OnlineSource))]
    [XmlInclude(typeof(OfflineSource))]
    [XmlInclude(typeof(WeatherDataSource))]
    [XmlInclude(typeof(NASAAPODCaptionSource))]
    public class LockViewApplicationState
    {
        const string SettingInstance = "Settings.xml";
        public ImageSource SelectedImageSource { get; set; }
        public string SelectedImageSourceParameters { get; set; }
        public double UserQuotaInDollars { get; set; }
        public LockViewRequestMetadata RequestMetadata { get; set; }
        public static LockViewApplicationState Instance { get; set; }
        static LockViewApplicationState()
        {
            try
            {
                using (var fs = ApplicationData.Current.LocalFolder.OpenStreamForReadAsync(SettingInstance).Result)
                {
                    XmlSerializer xmlSerializer = new XmlSerializer(typeof(LockViewApplicationState));
                    Instance = (LockViewApplicationState)xmlSerializer.Deserialize(fs);
                }
            }
            catch (Exception ex)
            {
                Instance = new LockViewApplicationState();
                Instance.RequestMetadata = new LockViewRequestMetadata() { RequestLanguage = "en-us", ScaleFactor = 1.0 };

#if WINDOWS_PHONE
                if (Microsoft.Phone.Info.DeviceStatus.DeviceTotalMemory >> 28 < 1)
                {
                    //low ram device.
                    Instance.RequestMetadata.Phase = LockViewRequestMetadata.TaskPhase.Tick;
                }
#endif
                Instance.PreviewFormattingContract = new OverlayFormattingContract()
                {
                    BackgroundSecondLine = "Transparent",
                    BackgroundFirstLine = "Transparent",
                    BackgroundTitle = "Transparent",
                    FirstLineFont = new FontContract() { FontSize = 20, FontFamily = "Segoe WP Semibold" },
                    SecondLineFont = new FontContract() { FontSize = 20, FontFamily = "Segoe WP Semibold" },
                    TitleFont = new FontContract() { FontSize = 26, FontFamily = "Segoe WP Black" },
                    ForegroundFirstLine = "White",
                    ForegroundSecondLine = "Gray",
                    ForegroundTitle = "White"
                };
                Instance.PreviewContextContract = new OverlayContextContract();
                Instance.PreviewLayoutContract = new OverlayLayoutContract();
                Instance.UserQuotaInDollars = 0.33;
                Instance.SelectedInterest = new InterestRequest();
            }
        }
        //expose for XML Serializer
        public LockViewApplicationState()
        {
            SelectedProviders = new InterestGatherer[1];
            SelectedContextContracts = new OverlayContextContract[1];
            SelectedInterests = new InterestRequest[1];
        }
#if WINDOWS_APP
        public DateTime BackgroundTaskLastRun { get; set; }
#endif
        public bool DoNotDisturb { get; set; }
        public InterestRequest[] SelectedInterests { get; set; }
        public InterestRequest SelectedInterest
        {
            get
            {
                return SelectedInterests[0];
            }
            set
            {
                SelectedInterests[0] = value;
            }
        }
        public InterestGathering.InterestGatherer SelectedProvider
        {
            get
            {
                return SelectedProviders[0];
            }
            set
            {
                SelectedProviders[0] = value;
            }
        }
        public InterestGatherer[] SelectedProviders { get; set; }
        public OverlayContextContract PreviewContextContract
        {
            get
            {
                return SelectedContextContracts[0];
            }
            set
            {
                SelectedContextContracts[0] = value;
            }
        }
        public OverlayContextContract[] SelectedContextContracts { get; set; }
        public OverlayFormattingContract PreviewFormattingContract { get; set; }
        public OverlayLayoutContract PreviewLayoutContract { get; set; }
        public async Task SaveState()
        {
            var file = await ApplicationData.Current.LocalFolder.CreateFileAsync(SettingInstance, CreationCollisionOption.ReplaceExisting);
            using (var fs = await file.OpenStreamForWriteAsync())
            {
                XmlSerializer xmlSerializer = new XmlSerializer(typeof(LockViewApplicationState));
                xmlSerializer.Serialize(fs, this);
            }
        }
    }
}

