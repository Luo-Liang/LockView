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

namespace CompositorTest
{
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
                wb.Lock();
                var bmp = new System.Drawing.Bitmap(wb.PixelWidth, wb.PixelHeight,
                  wb.BackBufferStride,
                  System.Drawing.Imaging.PixelFormat.Format32bppPArgb,
                  wb.BackBuffer);

                Graphics g = System.Drawing.Graphics.FromImage(bmp); //                // ...and finally:
                g.Apply(new OverlayLayout()
                {
                    AutoExpand = true,
                    Origin = new System.Drawing.Point() { X = 10, Y = 10 },
                    ParagraphSpacing = 10,
                    TargetHeight = 300,
                    TargetWidth = 400
                }, new OverlayContext()
                {
                    FirstLine = "Test First Line Is Here",
                    SecondLine = "Detailed Content Line Goes Here",
                    Title = "Title Goes Here"
                }, new OverlayFormatting()
                {
                    FirstLineFont = new Font("Segoe UI", 12),
                    SecondLineFont = new Font("Segoe UI", 10),
                    TitleFont = new Font("Segoe UI", 16),
                    ForegroundFirstLine = new SolidBrush(System.Drawing.Color.Black),
                    ForegroundSecondLine = new SolidBrush(System.Drawing.Color.Black),
                    ForegroundTitle = new SolidBrush(System.Drawing.Color.Black),
                });
                g.Dispose();
                bmp.Dispose();
                wb.AddDirtyRect(new Int32Rect(0, 0, wb.PixelWidth, wb.PixelHeight));
                wb.Unlock();
                targetImg.Source = wb;
            }
        }
    }
}
