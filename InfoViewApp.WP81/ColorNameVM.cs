using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace InfoViewApp.WP81
{
    public class ColorNameVM
    {
        public Color Color { get; set; }
        public string ColorName { get; set; }
    }

    public class ColorNameVMCollection : List<ColorNameVM> { }
}
