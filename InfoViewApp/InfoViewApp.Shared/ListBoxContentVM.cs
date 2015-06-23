using System;
using System.Collections.Generic;
using System.Text;

namespace InfoViewApp
{
    class ListBoxContentVM
    {
        public string FirstLine { get; set; }
        public string SecondLine { get; set; }
    }

    //Generic solution for all platforms. 
    class ListBoxContentVMCollection : List<ListBoxContentVM>
    {

    }

    class IntegerCollection : List<int>
    {

    }
}
