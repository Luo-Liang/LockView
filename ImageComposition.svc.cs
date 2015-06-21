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
using System.IO;
using System.Drawing.Imaging;

namespace InfoView
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "Service1" in code, svc and config file together.
    // NOTE: In order to launch WCF Test Client for testing this service, please select Service1.svc or Service1.svc.cs at the Solution Explorer and start debugging.
    public class ImageComposition : IImageCompositionService
    {
        public ImageCompositionResponse Compose(ImageCompositionRequest request)
        {
            MemoryStream memStream = new MemoryStream(request.RawImage);
            memStream.Seek(0, SeekOrigin.Begin);
            var img = Image.FromStream(memStream);
            Graphics g = Graphics.FromImage(img);
            g.Apply(request.LayoutContract.ToOverlayLayout(),
                request.ContextContract.ToOverlayContext(),
                request.FormattingContract.ToOverlayFormatting());
            g.Save();
            g.Dispose();
            MemoryStream imgStream = new MemoryStream();
            img.Save(imgStream, ImageFormat.Jpeg);
            imgStream.Seek(0, SeekOrigin.Begin);
            //img.Save(Environment.GetFolderPath(Environment.SpecialFolder.Desktop)+"\\abc.jpg",ImageFormat.Jpeg);
            var response =  new ImageCompositionResponse();
            response.Result = CompositionResult.Changed;
            response.Image = new byte[imgStream.Length];
            imgStream.Read(response.Image, 0, response.Image.Length);
            imgStream.Close();
            memStream.Close();
            return response;
        }
    }
}
