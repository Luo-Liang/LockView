using InfoViewApp.WP81;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using Windows.Graphics.Display;
using Windows.UI.Core;
namespace InfoViewApp.WP81
{
    class ResolutionProvider
    {
#if WINDOWS_PHONE
        public static void GetScreenSizeInPixels(out double height, out double width)
        {
            var content = Application.Current.Host.Content;
            double scale = (double)content.ScaleFactor / 100;
            int h = (int)Math.Ceiling(content.ActualHeight * scale);
            int w = (int)Math.Ceiling(content.ActualWidth * scale);
            height = h;
            width = w;
            //height *= DisplayInformation.GetForCurrentView().RawPixelsPerViewPixel;
            //width *= DisplayInformation.GetForCurrentView().RawPixelsPerViewPixel;
    }
        public static double GetScreenHeightWidthRatio()
        {
            double height, width;
            GetScreenSizeInPixels(out height, out width);
            return height / width;
        }
#endif
        static double ScaleFactor;
        public static double GetScaleFactor()
        {
            try
            {
                if (ScaleFactor != 0) return ScaleFactor;
#if WINDOWS_PHONE
                ScaleFactor = (double)Application.Current.Host.Content.ScaleFactor / 100;
#elif WINDOWS_APP
                ResolutionScale resolutionScale = DisplayInformation.GetForCurrentView().ResolutionScale;
                ScaleFactor = (double)resolutionScale / 100.0;
#endif
                return ScaleFactor;
            }
            catch
            {
                return 1.5;
            }
        }
    }
}
