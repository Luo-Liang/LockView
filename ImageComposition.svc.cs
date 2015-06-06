using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;
using System.Windows.Media.Imaging;
using GraphicsOverlay;
using System.Windows;
using System.Windows.Media;

namespace InfoView
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "Service1" in code, svc and config file together.
    // NOTE: In order to launch WCF Test Client for testing this service, please select Service1.svc or Service1.svc.cs at the Solution Explorer and start debugging.
    public class ImageComposition : IImageCompositionService
    {
        public ImageCompositionResponse Compose(ImageCompositionRequest request)
        {
            var img = request.RawImage;
            WriteableBitmap wb = new WriteableBitmap(1, 1, 1, 1, PixelFormats.Default, BitmapPalettes.Halftone256Transparent);
            wb.FromByteArray(img);
            wb.Lock();
            var bmp = new Bitmap(
              wb.PixelWidth, wb.PixelHeight,
              wb.BackBufferStride,
              System.Drawing.Imaging.PixelFormat.Format32bppPArgb,
              wb.BackBuffer);
            Graphics g = System.Drawing.Graphics.FromImage(bmp); //                // ...and finally:
            g.Apply(request.LayoutContract.ToOverlayLayout(),
                request.ContextContract.ToOverlayContext(),
                request.FormattingContract.ToOverlayFormatting());
            g.Dispose();
            bmp.Dispose();
            wb.AddDirtyRect(new Int32Rect(0, 0, wb.PixelWidth, wb.PixelHeight));
            wb.Unlock();
            return new ImageCompositionResponse()
            {
                Image = wb.ToByteArray(),
                Result = CompositionResult.Changed
            };
        }
    }
}
