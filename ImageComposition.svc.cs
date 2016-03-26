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
using InfoView.LockViewSpecificImageHandlers;

namespace InfoView
{
    public class ServiceCache<T, K>
    {
        ConcurrentDictionary<string, ServiceCacheEntry<K>> cache = new ConcurrentDictionary<string, ServiceCacheEntry<K>>();
        internal WebClient CacheFetcher = new WebClient();
        DateTime lastService = DateTime.MinValue;
        internal class ServiceCacheEntry<K>
        {
            public DateTime ExpirationDate;
            public string UrlIdentifier;
            public K Content;
        }
        internal virtual string getEntryIdFromRequest(T request)
        {
            throw new NotImplementedException();
        }
        internal virtual ServiceCacheEntry<K> getEntryFromRequest(T request)
        {
            throw new NotImplementedException();
        }
        internal virtual void prepareContentRelease(K content)
        {
            throw new NotImplementedException();
        }
        public virtual K TryFetchAndAdd(T request)
        {
            ServiceCacheEntry<K> entry = null;
            var key = getEntryIdFromRequest(request);
            if (cache.TryGetValue(key, out entry) == false)
            {
                entry = getEntryFromRequest(request);
                cache.TryAdd(key, entry);
            }
            else if ((DateTime.Now - lastService).Minutes >= 10)
            {
                ServiceCacheEntry<K> notUsed;
                //it's a hit. But this value might expire. Don't expire the current request, simply get rid of it from the cache -- only if last service is 24 hours ago.
                foreach (var k in cache.Keys)
                {
                    cache.TryGetValue(k, out notUsed);
                    if (notUsed.ExpirationDate < DateTime.Now)
                    {
                        cache.TryRemove(key, out notUsed);
                    }
                }
                lastService = DateTime.Now;
            }
            prepareContentRelease(entry.Content);
            return entry.Content;
        }
    }

    //circumvent API restrictions
    public class ImageCache : ServiceCache<ImageRequestOverride, MemoryStream>
    {
        static Uri DefaultImageUri = new Uri("http://dovecomputers.com/blog/wp-content/uploads/2012/10/Windows-XP-desktop.png");
        ConcurrentDictionary<string, ServiceCacheEntry<MemoryStream>> CacheEntries;
        public ImageCache()
        {
            handlerRegistry = new Dictionary<string, ILockViewSpecificImageHandler>();
            handlerRegistry["lockview://fixedwallpapers/Himawari-8"] = new LockViewSpecificImageHandlers.LiveEarthImageHandler();
        }
        //public const string ImageLocator = "http://www.bing.com/HPImageArchive.aspx?format=js&idx=0&n=1&mkt={0}";
        const string nasaAPIKey = "mzzFYcsRbS2oVEak5fvY4Znbx6tTsAy200MiQqXF"; //<--- if you see this, it is mangled.
        static Dictionary<string, ILockViewSpecificImageHandler> handlerRegistry;
        static ImageCache()
        {
            handlerRegistry = new Dictionary<string, ILockViewSpecificImageHandler>();

        }
        internal override string getEntryIdFromRequest(ImageRequestOverride iro)
        {
            return string.Format("{0}&{1}", iro.ImageRequestUrl, iro.Arguments);
        }
        internal override ServiceCacheEntry<MemoryStream> getEntryFromRequest(ImageRequestOverride iro)
        {
            Dictionary<string, string> argumentKeyValue = new Dictionary<string, string>();
            foreach (var item in iro.Arguments.Split(new[] { '&' }, StringSplitOptions.RemoveEmptyEntries))
            {
                var parts = item.Split('=');
                argumentKeyValue.Add(parts[0], parts.Length == 1 ? "" : parts[1]);
            }
            ServiceCacheEntry<MemoryStream> entry = new ServiceCacheEntry<MemoryStream>();
            byte[] rawBytes = null;
            ILockViewSpecificImageHandler lockViewHandler = null;
            WriteableBitmap bitmap = null;
            //is it a LOCKVIEW protocol packet or HTTP packet?
            if (iro.ImageRequestUrl.StartsWith("lockview://"))
                lockViewHandler = handlerRegistry[iro.ImageRequestUrl];
            //TODO:CLEANUP.
            if (lockViewHandler == null)
            {
                try
                {
                    WebClient client = new WebClient();
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
                catch (Exception ex)
                {
                    var directoryPath = AppDomain.CurrentDomain.BaseDirectory + "\\Assets";
                    int maximum = Directory.GetFiles(directoryPath).Length;
                    var fileName = (DateTime.Now.Second % maximum) + 1;
                    //create this entry.
                    //var decoder = new JpegBitmapDecoder(new Uri(iro.ImageRequestUrl), BitmapCreateOptions.None, BitmapCacheOption.None);
                    //decoder.Frames[0].Freeze();
                    //var bmp = new BitmapImage(new Uri(iro.ImageRequestUrl, UriKind.Absolute));
                    //bmp.BeginInit();
                    //bmp.EndInit();
                    bitmap = new WriteableBitmap(1, 1, 72, 72, PixelFormats.Bgr24, BitmapPalettes.WebPalette);//<---anything
                    using (var stream = File.Open($"{directoryPath}\\{fileName}.jpg", FileMode.Open))
                        bitmap = bitmap.FromStream(stream);
                    //replace those with defaults.
                }
            }
            //bitmap = bitmap.FromStream(new MemoryStream(rawBytes));
            //bitmap = bitmap.FromByteArray(rawBytes);
            if (argumentKeyValue.ContainsKey("resolution"))
            {
                var resolution = argumentKeyValue["resolution"];
                var whString = resolution.Split('x');
                double height = double.Parse(whString[1]),
                       width = double.Parse(whString[0]);
                if (argumentKeyValue.ContainsKey("padblack") && argumentKeyValue["padblack"]=="true"  ||
                   (lockViewHandler!=null && lockViewHandler.GetType() == typeof(LiveEarthImageHandler))) //backward compat!
                {
                    bitmap = bitmap.FromStream(lockViewHandler.RequestImage(iro.Arguments));
                }
                else
                {
                    bitmap = bitmap.ResizeUniformly(height, width);
                }
                //TODO:: CLean up this logic to make it more general

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
            entry.UrlIdentifier = getEntryIdFromRequest(iro);
            entry.Content = decodedStream;

            if (lockViewHandler != null)
            {
                entry.ExpirationDate = DateTime.Now.Add(lockViewHandler.GetExpirationDuration(iro.Arguments));
            }
            return entry;
        }
        internal override void prepareContentRelease(MemoryStream content)
        {
            content.Seek(0, SeekOrigin.Begin);
        }
    }

    public class JsonCache : ServiceCache<string, string>
    {
        internal override ServiceCacheEntry<string> getEntryFromRequest(string request)
        {
            ServiceCacheEntry<string> entry = new ServiceCacheEntry<string>();
            entry.Content = CacheFetcher.DownloadString(request);
            entry.ExpirationDate = DateTime.Now.AddDays(1);
            entry.UrlIdentifier = getEntryIdFromRequest(request); ;
            return entry;
        }
        internal override void prepareContentRelease(string content)
        {
        }
        internal override string getEntryIdFromRequest(string request)
        {
            return request;
        }
    }
    public class ImageComposition : IImageCompositionService
    {
        static ImageCache imgCache = new ImageCache();
        static JsonCache jsonCache = new JsonCache();
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
                WriteableBitmap bitmap = bitmap = new WriteableBitmap(1, 1, 72, 72, PixelFormats.Bgr24, BitmapPalettes.WebPalette);//<---anything
                bitmap = bitmap.FromStream(memStream);
                bitmap = bitmap.ResizeUniformly(request.LayoutContract.TargetHeight, request.LayoutContract.TargetWidth);
                JpegBitmapEncoder encoder = new JpegBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(bitmap));
                MemoryStream decodedStream = new MemoryStream();
                encoder.Frames[0].Freeze();
                encoder.Save(decodedStream);
                memStream.Dispose();
                memStream = decodedStream;
                //resize also for custom images.
            }
            memStream.Seek(0, SeekOrigin.Begin);
            var img = Image.FromStream(memStream);
            Graphics g = Graphics.FromImage(img);
            g.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
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
        //@TODO: Moveout!
        public byte[] RequestImage(string request)
        {
            var parts = request.Split('?');
            var urlPart = parts[0];
            //var argumentPart = parts[1];
            var stream = imgCache.TryFetchAndAdd(ImageRequestOverride.Parse(request));
            byte[] imageBytes = new byte[stream.Length];
            stream.Seek(0, SeekOrigin.Begin);
            stream.Read(imageBytes, 0, imageBytes.Length);
            return imageBytes;
        }

        public string RequestJson(string request)
        {
            var value = jsonCache.TryFetchAndAdd(request);
            return value;
        }
    }

    static class MISCImgTools
    {
        public static WriteableBitmap ResizeUniformly(this WriteableBitmap bitmap, double height, double width)
        {
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
            return bitmap;
        }
    }
}
