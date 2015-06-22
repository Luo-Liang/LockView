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
        }
        Stream BackgroundPreview { get; set; }
        public IRandomAccessStream AccessStream { get; set; }
        public InterestGathering.IInterestGatherer SelectedProvider{ get; set; }
    }
}
