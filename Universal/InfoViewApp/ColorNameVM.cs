using System;
using System.Collections.Generic;
using System.Text;
using Windows.UI;

namespace InfoViewApp.WP81
{
    public class ColorNameVM
    {
        public Color Color { get; set; }
        public string ColorName { get; set; }
    }

    public class ColorNameVMCollection : List<ColorNameVM> { }
}
