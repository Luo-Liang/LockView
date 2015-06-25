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
            PersistFileName = "MyBg.jpeg";
            PreviewFormattingContract = new OverlayFormattingContract()
            {
                BackgroundSecondLine = "Transparent",
                BackgroundFirstLine = "Transparent",
                BackgroundTitle = "Transparent",
                FirstLineFont = new FontContract() { FontSize = 37, FontFamily = "Segoe UI" },
                SecondLineFont = new FontContract() { FontSize = 24, FontFamily = "Segoe UI" },
                TitleFont = new FontContract() { FontSize = 20, FontFamily = "Segoe UI" },
                ForegroundFirstLine = "White",
                ForegroundSecondLine = "Gray",
                ForegroundTitle = "White"
            };

            PreviewContextContract = new OverlayContextContract();
            PreviewLayoutContract = new OverlayLayoutContract();
        }
        Stream BackgroundPreview { get; set; }
        public string PersistFileName { get; set; }
        public InterestGathering.IInterestGatherer SelectedProvider { get; set; }
        public OverlayContextContract PreviewContextContract { get; set; }
        public OverlayFormattingContract PreviewFormattingContract { get; set; }
        public OverlayLayoutContract PreviewLayoutContract { get; set; }
    }
}
