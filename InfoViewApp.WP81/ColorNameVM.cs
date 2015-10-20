using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
#if WINDOWS_PHONE
using System.Windows.Media;
#elif WINDOWS_APP
using Windows.UI;
#endif

namespace InfoViewApp.WP81
{
    public class ColorNameVM
    {
        public Color Color { get; set; }
        public string ColorName { get; set; }
    }

    public class ColorNameVMCollection : List<ColorNameVM> { }
}
