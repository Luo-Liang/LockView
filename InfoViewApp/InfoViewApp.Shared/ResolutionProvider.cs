using System;
using System.Collections.Generic;
using System.Text;
using Windows.Graphics.Display;
using Windows.UI.Xaml;

namespace InfoViewApp
{
    class ResolutionProvider
    {
        public static void GetScreenSizeInPixels(out double height, out double width)
        {
            var bounds = Window.Current.Bounds;
            height = bounds.Height;
            width = bounds.Width;
#if WINDOWS_PHONE_APP
            height *= DisplayInformation.GetForCurrentView().RawPixelsPerViewPixel;
            width *= DisplayInformation.GetForCurrentView().RawPixelsPerViewPixel;
#endif
        }

        public static double GetScreenHeightWidthRatio()
        {
            double height,width;
            GetScreenSizeInPixels(out height, out width);
            return height / width;
        }
    }
}
