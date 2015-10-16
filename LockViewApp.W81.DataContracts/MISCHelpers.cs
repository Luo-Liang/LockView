using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;

namespace LockViewApp.W81
{
    public static class MISCHelpers
    {
        public static void GetResolutionInPixels(out int width, out int height)
        {
            height= (int)Window.Current.Bounds.Height;
            width = (int)Window.Current.Bounds.Width;
        }
    }
}
