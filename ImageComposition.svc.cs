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
using System.Collections.Concurrent;
using System.Net;
using System.Threading.Tasks;

namespace InfoView
{
    public class BingImageCache
    {
        class ImageCacheEntry
        {
            public DateTime ExpirationDate;
            public string UrlIdentifier;
            public MemoryStream Content;
        }
        ConcurrentDictionary<string, ImageCacheEntry> CacheEntries;
        WebClient CacheFetcher;
        public BingImageCache()
        {
            CacheEntries = new ConcurrentDictionary<string, ImageCacheEntry>();
            CacheFetcher = new WebClient();
        }
        public const string ImageLocator = "http://www.bing.com/HPImageArchive.aspx?format=js&idx=0&n=1&mkt={0}";
        public async Task<Stream> TryFetchAndAdd(ImageRequestOverride iro)
        {
            ImageCacheEntry entry;
            var identifier = string.Format("{0}{1}", iro.ImageRequestUrl, iro.Arguments);
            if (false == CacheEntries.TryGetValue(identifier, out entry))
            {
                entry = new ImageCacheEntry();
                //create this entry.
                JpegBitmapEncoder encoder = new JpegBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(new MemoryStream(await CacheFetcher.DownloadDataTaskAsync(iro.ImageRequestUrl))));
                if (iro.Arguments == "lq")
                {
                    //low quality.
                    encoder.QualityLevel = 70;
                }
                MemoryStream decodedStream = new MemoryStream();
                encoder.Save(decodedStream);
                entry.ExpirationDate = DateTime.Now.AddDays(1);
                entry.UrlIdentifier = identifier;
                entry.Content = decodedStream;
                CacheEntries.TryAdd(identifier, entry);
            }
            else
            {
                ImageCacheEntry notUsed;
                //it's a hit. But this value might expire. Don't expire the current request, simply get rid of it from the cache.
                foreach (var key in CacheEntries.Keys)
                {
                    CacheEntries.TryGetValue(key, out notUsed);
                    if (notUsed.ExpirationDate < DateTime.Now)
                    {
                        CacheEntries.TryRemove(key, out notUsed);
                    }
                }
            }
            return entry.Content;
        }
    }
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the public class name "Service1" in code, svc and config file together.
    // NOTE: In order to launch WCF Test Client for testing this service, please select Service1.svc or Service1.svc.cs at the Solution Explorer and start debugging.
    public class ImageComposition : IImageCompositionService
    {
        static BingImageCache imgCache = new BingImageCache();
        public async Task<ImageCompositionResponse> Compose(ImageCompositionRequest request)
        {
            Stream memStream = null;
            if (request.ImageRequestOverride != null)
            {
                memStream = await imgCache.TryFetchAndAdd(request.ImageRequestOverride);
            }
            else
            {
                memStream = new MemoryStream(request.RawImage);
            }
            memStream.Seek(0, SeekOrigin.Begin);
            var img = Image.FromStream(memStream);
            Graphics g = Graphics.FromImage(img);
            g.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighSpeed;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            g.Apply(request.LayoutContract.ToOverlayLayout(),
                request.ContextContract.ToOverlayContext(),
                request.FormattingContract.ToOverlayFormatting());
            g.Save();
            g.Dispose();
            MemoryStream imgStream = new MemoryStream();
            img.Save(imgStream, ImageFormat.Jpeg);
            imgStream.Seek(0, SeekOrigin.Begin);
            //img.Save(Environment.GetFolderPath(Environment.SpecialFolder.Desktop)+"\\abc.jpg",ImageFormat.Jpeg);
            var response = new ImageCompositionResponse();
            response.ResultString = "Okay";
            byte[] ImageBytes = new byte[imgStream.Length];
            imgStream.Read(ImageBytes, 0, ImageBytes.Length);
            imgStream.Close();
            memStream.Close();
            response.Image = ImageBytes;
            return response;
        }


        public async Task<string> ComposeLegacy(string request)
        {
            return request;
        }
    }
}
