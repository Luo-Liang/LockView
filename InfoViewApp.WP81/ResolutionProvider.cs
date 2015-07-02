using InfoViewApp.WP81;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using Windows.Graphics.Display;
using Windows.UI.Core;
namespace InfoViewApp
{
    class ResolutionProvider
    {
        public static void GetScreenSizeInPixels(out double height, out double width)
        {
            var content = Application.Current.Host.Content;

            double scale = (double)content.ScaleFactor / 100;

            int h = (int)Math.Ceiling(content.ActualHeight * scale);

            int w = (int)Math.Ceiling(content.ActualWidth * scale);

            height = h;
            width = w;
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
