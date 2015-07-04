using System;
using System.Collections.Generic;
using System.Text;

namespace InfoViewApp.WP81
{
    public class ListBoxContentVM
    {
        public string FirstLine { get; set; }
        public string SecondLine { get; set; }
        public ListBoxContentVM()
        { }
    }

    //Generic solution for all platforms. 
    public class ListBoxContentVMCollection : List<ListBoxContentVM>
    {
        public ListBoxContentVMCollection() { }
    }

    public class IntegerCollection : List<int>
    {
    }
}
