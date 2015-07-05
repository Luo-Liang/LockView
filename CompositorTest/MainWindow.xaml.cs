using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;
using System.IO.Compression;
using Newtonsoft.Json;
using System.Net;
using InfoViewApp.WP81;
namespace CompositorTest
{
    public static class ByteArrayCompressionUtility
    {

        public static byte[] Compress(byte[] inputData)
        {
            if (inputData == null)
                throw new ArgumentNullException("inputData must be non-null");

            using (var compressIntoMs = new MemoryStream())
            {
                using (var gzs = new GZipStream(compressIntoMs,
                 CompressionMode.Compress))
                {
                    gzs.Write(inputData, 0, inputData.Length);
                }
                return compressIntoMs.ToArray();
            }
        }

        public static byte[] Decompress(byte[] inputData)
        {
            if (inputData == null)
                throw new ArgumentNullException("inputData must be non-null");

            using (var compressedMs = new MemoryStream(inputData))
            {
                using (var decompressedMs = new MemoryStream())
                {
                    using (var gzs = (new GZipStream(compressedMs,
                     CompressionMode.Decompress)))
                    {
                        gzs.CopyTo(decompressedMs);
                    }
                    return decompressedMs.ToArray();
                }
            }
        }
    }
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Grid_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                // Note that you can have more than one file.
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);

                // Assuming you have one file that you care about, pass it off to whatever
                // handling code you have defined.
                var fn = files[0];
                byte[] imgBytes;
                using (var stream = File.Open(fn, FileMode.Open))
                {
                    imgBytes = new byte[stream.Length];
                    stream.Read(imgBytes, 0, (int)stream.Length);
                };
                var layout = new  InfoViewApp.WP81.OverlayLayoutContract()
                {
                    AutoExpand = true,
                    Origin = new InfoViewApp.WP81.Point() { X = 10, Y = 10 },
                    ParagraphSpacing = 10,
                    TargetHeight = 300,
                    TargetWidth = 400
                };
                var context = new InfoViewApp.WP81.OverlayContextContract()
                {
                    FirstLine = "Test First Line Is Here",
                    SecondLine = "Detailed Content Line Goes Here",
                    Title = "Title Goes Here"
                };
                var formatting = new InfoViewApp.WP81.OverlayFormattingContract()
                {
                    FirstLineFont = new InfoViewApp.WP81.FontContract() { FontFamily = "Segoe UI", FontSize = 12 },
                    SecondLineFont = new InfoViewApp.WP81.FontContract() { FontFamily = "Segoe UI", FontSize = 18 },
                    TitleFont = new InfoViewApp.WP81.FontContract() { FontFamily = "Segoe UI", FontSize = 24 },
                    ForegroundFirstLine = "White",
                    ForegroundSecondLine = "White",
                    ForegroundTitle = "White",

                };
                var request = new ImageCompositionRequest()
                {
                    ContextContract = context,
                    FormattingContract = formatting,
                    LayoutContract = layout,
                    RawImage = Convert.ToBase64String(imgBytes),
                    RequestId = 1,
                    UserId = 2,
                    InterestId = "Microsoft"
                };
                var jRequest = JsonConvert.SerializeObject(request);
                WebClient client = new WebClient();
                client.Headers.Add(HttpRequestHeader.ContentType, "application/json");
                var returnBytes = client.UploadString("http://localhost:49791/ImageComposition.svc/Compose", jRequest);
                var result = JsonConvert.DeserializeObject<ImageCompositionResponse>(returnBytes);
                var width = 432;
                var height = 432;
                var dpiX = 96d;
                var dpiY = 96d;
                var pixelFormat = PixelFormats.Pbgra32;
                var bytesPerPixel = (pixelFormat.BitsPerPixel + 7) / 8;
                var stride = bytesPerPixel * width;
                var bitmap = BitmapImage.Create(width, height, dpiX, dpiY, pixelFormat, null, Convert.FromBase64String(result.Image), stride);
                targetImg.Source  = new WriteableBitmap(BitmapFactory.ConvertToPbgra32Format(bitmap));

                //https://ajax.googleapis.com/ajax/services/search/news?v=1.0&q={0}

            }
        }
    }
    public class ImageCompositionRequest
    {
        public string InterestId { get; set; }
        public string RawImage { get; set; }
        public long UserId { get; set; } //may be used for persistence in future
        public long RequestId { get; set; }
        public OverlayFormattingContract FormattingContract { get; set; }
        public OverlayLayoutContract LayoutContract { get; set; }
        public OverlayContextContract ContextContract { get; set; }
    }
    public class ImageCompositionResponse
    {
        public string Image { get; set; }
        public string ResultString { get; set; }
    }
}
