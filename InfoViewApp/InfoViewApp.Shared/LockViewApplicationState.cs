using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Windows.Storage.Streams;

namespace InfoViewApp
{
    class LockViewApplicationState
    {
        public static LockViewApplicationState Instance { get; private set; }
        static LockViewApplicationState()
        {
            Instance = new LockViewApplicationState();
        }
        private LockViewApplicationState()
        {
            BackgroundPreview = new MemoryStream();
            AccessStream = BackgroundPreview.AsRandomAccessStream();
            PreviewFormattingContract = new OverlayFormattingContract()
            {
                BackgroundSecondLine = "Transparent",
                BackgroundFirstLine = "Transparent",
                BackgroundTitle = "Transparent",
                FirstLineFont = new FontContract() { FontSize = 15, FontFamily = "Segoe UI" },
                SecondLineFont = new FontContract() { FontSize = 12, FontFamily = "Segoe UI" },
                TitleFont = new FontContract() { FontSize = 18, FontFamily = "Segoe UI" },
                ForegroundFirstLine = "White",
                ForegroundSecondLine = "White",
                ForegroundTitle = "White"
            };

            PreviewContextContract = new OverlayContextContract();
            PreviewLayoutContract = new OverlayLayoutContract();
        }
        Stream BackgroundPreview { get; set; }
        public IRandomAccessStream AccessStream { get; set; }
        public InterestGathering.IInterestGatherer SelectedProvider { get; set; }
        public OverlayContextContract PreviewContextContract { get; set; }
        public OverlayFormattingContract PreviewFormattingContract { get; set; }
        public OverlayLayoutContract PreviewLayoutContract { get; set; }
    }
}
