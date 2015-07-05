using System;
using System.Collections.Generic;
using System.Text;

namespace InfoViewApp.WP81
{
    /// <summary>
    /// Always link this with CloudCompositionContracts.
    /// </summary>
    public class FontContract
    {
        public string FontFamily { get; set; }
        public int FontSize { get; set; }
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

    public class OverlayFormattingContract
    {
        public string BackgroundFirstLine { get; set; }
        public string BackgroundSecondLine { get; set; }
        public string BackgroundTitle { get; set; }
        public FontContract FirstLineFont { get; set; }
        public string ForegroundFirstLine { get; set; }
        public string ForegroundSecondLine { get; set; }
        public string ForegroundTitle { get; set; }
        public FontContract SecondLineFont { get; set; }
        public FontContract TitleFont { get; set; }
    }

    public class OverlayLayoutContract
    {
        public bool AutoExpand { get; set; }
        public Point Origin { get; set; }
        public int ParagraphSpacing { get; set; }
        public int TargetHeight { get; set; }
        public int TargetWidth { get; set; }
    }

    public class OverlayContextContract
    {
        public string FirstLine { get; set; }
        public string SecondLine { get; set; }
        public string Title { get; set; }
    }


    public class Point
    {
        public int X { get; set; }
        public int Y { get; set; }
    }

    public class ImageCompositionResponse
    {
        public string Image { get; set; }
        public string ResultString { get; set; }
    }
}