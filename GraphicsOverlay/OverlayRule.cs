using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GraphicsOverlay
{
    public class OverlayFormatting
    {
        public Font TitleFont { get; set; }
        public Brush ForegroundTitle { get; set; }
        public Brush BackgroundTitle { get; set; }
        public Font FirstLineFont { get; set; }
        public Brush ForegroundFirstLine { get; set; }
        public Brush BackgroundFirstLine { get; set; }
        public Font SecondLineFont { get; set; }
        public Brush ForegroundSecondLine { get; set; }
        public Brush BackgroundSecondLine { get; set; }
        public OverlayFormatting()
        {
            BackgroundTitle = BackgroundFirstLine = BackgroundSecondLine = new SolidBrush(Color.Transparent);
        }
    }

    public class OverlayLayout
    {
        public bool AutoExpand { get; set; }
        public Point Origin { get; set; }
        public int TargetHeight { get; set; }
        public int TargetWidth { get; set; }
        public int ParagraphSpacing { get; set; }
    }

    public class OverlayContext
    {
        public string Title { get; set; }
        public string FirstLine { get; set; }
        public string SecondLine { get; set; }
    }
}
