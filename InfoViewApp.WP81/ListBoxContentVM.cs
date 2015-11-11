using System;
using System.Collections.Generic;
using System.Text;

namespace InfoViewApp.WP81
{
    public class ListBoxContentVM
    {
        public string FirstLine { get; set; }
        public string SecondLine { get; set; }
        public bool FeatureEnabled { get; set; }
#if WINDOWS_PHONE
        public Uri NavigationPath { get; set; }
#elif WINDOWS_APP
        public string NavigationType { get; set; }
#endif
        public ListBoxContentVM()
        {
            FeatureEnabled = true;
        }
    }

    //Generic solution for all platforms. 
    public class ListBoxContentVMCollection : List<ListBoxContentVM>
    {
        public ListBoxContentVMCollection() { }
    }

    public class IntegerCollection : List<int>
    {
#if WINDOWS_APP
        public IntegerCollection()
        {
            for (int i = 10; i <= 52; i++) this.Add(i);
        }
#endif
    }
}
