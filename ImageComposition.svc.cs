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
using InfoView.DataContract;

namespace InfoView
{
    public class BingImageCache
    {
        static Uri DefaultImageUri = new Uri("http://dovecomputers.com/blog/wp-content/uploads/2012/10/Windows-XP-desktop.png");
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
        //public const string ImageLocator = "http://www.bing.com/HPImageArchive.aspx?format=js&idx=0&n=1&mkt={0}";
        const string nasaAPIKey = "mzzFYcsRbS2oVEak5fvY4Znbx6tTsAy200MiQqXF"; //<--- if you see this, it is mangled.
        public Stream TryFetchAndAdd(ImageRequestOverride iro)
        {
            ImageCacheEntry entry;
            Dictionary<string, string> argumentKeyValue = new Dictionary<string, string>();
            foreach (var item in iro.Arguments.Split('&'))
            {
                var parts = item.Split('=');
                argumentKeyValue.Add(parts[0], parts.Length == 1 ? "" : parts[1]);
            }
            var identifier = string.Format("{0}{1}", iro.ImageRequestUrl, iro.Arguments);
            if (false == CacheEntries.TryGetValue(identifier, out entry))
            {
                entry = new ImageCacheEntry();
                WebClient client = new WebClient();
                byte[] rawBytes = null;
                bool Insert = true;
                WriteableBitmap bitmap = null;
                try
                {
                    rawBytes = client.DownloadData(new Uri(iro.ImageRequestUrl));
                    //create this entry.
                    //var decoder = new JpegBitmapDecoder(new Uri(iro.ImageRequestUrl), BitmapCreateOptions.None, BitmapCacheOption.None);
                    //decoder.Frames[0].Freeze();
                    //var bmp = new BitmapImage(new Uri(iro.ImageRequestUrl, UriKind.Absolute));
                    //bmp.BeginInit();
                    //bmp.EndInit();
                    bitmap = new WriteableBitmap(1, 1, 72, 72, PixelFormats.Bgr24, BitmapPalettes.WebPalette);//<---anything
                    bitmap = bitmap.FromStream(new MemoryStream(rawBytes));
                }
                catch
                {
                    rawBytes = client.DownloadData(DefaultImageUri);
                    //create this entry.
                    //var decoder = new JpegBitmapDecoder(new Uri(iro.ImageRequestUrl), BitmapCreateOptions.None, BitmapCacheOption.None);
                    //decoder.Frames[0].Freeze();
                    //var bmp = new BitmapImage(new Uri(iro.ImageRequestUrl, UriKind.Absolute));
                    //bmp.BeginInit();
                    //bmp.EndInit();
                    bitmap = new WriteableBitmap(1, 1, 72, 72, PixelFormats.Bgr24, BitmapPalettes.WebPalette);//<---anything
                    bitmap = bitmap.FromStream(new MemoryStream(rawBytes));
                    Insert = false;
                    //replace those with defaults.
                }
                //bitmap = bitmap.FromStream(new MemoryStream(rawBytes));
                //bitmap = bitmap.FromByteArray(rawBytes);
                if (argumentKeyValue.ContainsKey("resolution"))
                {
                    var resolution = argumentKeyValue["resolution"];
                    var whString = resolution.Split('x');
                    double height = double.Parse(whString[1]),
                           width = double.Parse(whString[0]);
                    double desiredRatio = height / width;
                    double actualRatio = (double)bitmap.PixelHeight / bitmap.PixelWidth;
                    if (actualRatio <= desiredRatio)
                    {
                        //scale to croppable settings first.
                        //in this case, the user can in general select along the x-axis. (width)
                        double scale = height / bitmap.PixelHeight;
                        //now scale the height and width as appropriate.
                        bitmap = bitmap.Resize((int)(bitmap.PixelWidth * scale), (int)height, WriteableBitmapExtensions.Interpolation.NearestNeighbor);
                        //place the selection at the center of the image.
                        //entire height is now selected.
                        bitmap = bitmap.Crop(new Rect(new System.Windows.Point((int)(bitmap.PixelWidth - width) / 2, 0), new System.Windows.Size((int)width, (int)height)));
                    }
                    else
                    {
                        //symmetric
                        double scale = width / bitmap.PixelWidth;
                        bitmap = bitmap.Resize((int)width, (int)(bitmap.PixelHeight * scale), WriteableBitmapExtensions.Interpolation.NearestNeighbor);
                        bitmap = bitmap.Crop(new Rect(new System.Windows.Point(0, (int)(bitmap.PixelHeight - height) / 2), new System.Windows.Size((int)width, (int)height)));
                    }
                }
                //bitmap.Unlock();
                JpegBitmapEncoder encoder = new JpegBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(bitmap));
                if (argumentKeyValue.ContainsKey("lq"))
                {
                    //low quality.
                    encoder.QualityLevel = 70;
                }
                MemoryStream decodedStream = new MemoryStream();
                encoder.Frames[0].Freeze();
                encoder.Save(decodedStream);
                entry.ExpirationDate = DateTime.Now.AddDays(1);
                entry.UrlIdentifier = identifier;
                entry.Content = decodedStream;
                if (Insert) 
                CacheEntries.TryAdd(identifier, entry);
                //using (var fs = File.Open("c:/users/liang luo/desktop/1.jpg", FileMode.Create))
                //{
                //    encoder.Save(fs);
                //}
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
            entry.Content.Seek(0, SeekOrigin.Begin);
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
            bool isStreamCached = false;
            if (request.ImageRequestOverride != null)
            {
                memStream = imgCache.TryFetchAndAdd(request.ImageRequestOverride);
                isStreamCached = true;
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
                new[] { request.ContextContract.ToOverlayContext() },
                request.FormattingContract.ToOverlayFormatting());
            g.Save();
            //g.
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
            if (!isStreamCached)
            {
                memStream.Close();
            }
            response.Image = ImageBytes;
            return response;
        }

        public async Task<string> ComposeLegacy(string request)
        {
            return request;
        }

        public async Task<ImageCompositionResponse> ComposeV2(ImageCompositionRequestV2 request)
        {
            Stream memStream = null;
            bool isStreamCached = false;
            if (request.ImageRequestOverride != null)
            {
                memStream = imgCache.TryFetchAndAdd(request.ImageRequestOverride);
                isStreamCached = true;
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
                request.ContextContracts.Select(ctx => ctx.ToOverlayContext()).ToArray(),
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
            if (!isStreamCached)
            {
                memStream.Close();
            }
            response.Image = ImageBytes;
            return response;
        }

        public byte[] RequestImage(string request)
        {
            var parts = request.Split('?');
            var urlPart = parts[0];
            //var argumentPart = parts[1];
            var stream = imgCache.TryFetchAndAdd(new ImageRequestOverride() { ImageRequestUrl = urlPart, Arguments = parts.Length == 1 ? "" : parts[1] });
            byte[] imageBytes = new byte[stream.Length];
            stream.Seek(0, SeekOrigin.Begin);
            stream.Read(imageBytes, 0, imageBytes.Length);
            return imageBytes;
        }
    }
}
