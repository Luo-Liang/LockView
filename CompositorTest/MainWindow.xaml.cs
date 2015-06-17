﻿using System;
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
using GraphicsOverlay;
using System.IO;
using CompositorTest.CloudComposition;
using InfoView.DataContract;
using System.IO.Compression;

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
                WriteableBitmap wb = new WriteableBitmap(1, 1, 1, 1, PixelFormats.Gray32Float, BitmapPalettes.BlackAndWhite);
                using (var stream = File.Open(fn, FileMode.Open))
                {
                    wb = wb.FromStream(stream);
                };
                var layout = new OverlayLayout()
                {
                    AutoExpand = true,
                    Origin = new System.Drawing.Point() { X = 10, Y = 10 },
                    ParagraphSpacing = 10,
                    TargetHeight = 300,
                    TargetWidth = 400
                };
                var context = new OverlayContext()
                {
                    FirstLine = "Test First Line Is Here",
                    SecondLine = "Detailed Content Line Goes Here",
                    Title = "Title Goes Here"
                };
                var formatting = new OverlayFormatting()
                {
                    FirstLineFont = new Font("Segoe UI", 12),
                    SecondLineFont = new Font("Segoe UI", 10),
                    TitleFont = new Font("Segoe UI", 16),
                    ForegroundFirstLine = new SolidBrush(System.Drawing.Color.Black),
                    ForegroundSecondLine = new SolidBrush(System.Drawing.Color.Black),
                    ForegroundTitle = new SolidBrush(System.Drawing.Color.Black),

                };
                var imgBytes = ByteArrayCompressionUtility.Compress(wb.ToByteArray());
                OverlayLayoutContract c;
                CloudComposition.ImageCompositionServiceClient client = new ImageCompositionServiceClient();
                var response = client.Compose(new ImageCompositionRequest()
                {
                    ContextContract = OverlayContextContract.FromOverlayContext(context),
                    FormattingContract = OverlayFormattingContract.FromOverlayFormatting(formatting),
                    LayoutContract = OverlayLayoutContract.FromOverlayLayout(layout),
                    RawImage = imgBytes,
                    RequestId = 1,
                    UserId = 2,
                    InterestId = "Microsoft"
                });
                targetImg.Source = wb.FromByteArray(response.Image);
            }
        }
    }
}
