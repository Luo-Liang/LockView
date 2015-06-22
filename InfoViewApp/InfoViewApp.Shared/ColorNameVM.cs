using System;
using System.Collections.Generic;
using System.Text;
using Windows.UI;

namespace InfoViewApp
{
    class ColorNameVM
    {
        public Color Color { get; set; }
        public string ColorName { get; set; }
    }

    class ColorNameVMCollection : List<ColorNameVM> { }
}
