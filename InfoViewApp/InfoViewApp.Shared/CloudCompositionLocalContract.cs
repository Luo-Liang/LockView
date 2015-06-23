using System;
using System.Collections.Generic;
using System.Text;

namespace InfoViewApp
{
    /// <summary>
    /// Always link this with CloudCompositionContracts.
    /// </summary>
    public struct FontContract
    {
        public string FontFamily { get; set; }
        public int FontSize { get; set; }
    }

    struct OverlayFormattingContract
    {
        public FontContract TitleFont { get; set; }
        public string ForegroundTitle { get; set; }
        public string BackgroundTitle { get; set; }
        public FontContract FirstLineFont { get; set; }
        public string ForegroundFirstLine { get; set; }
        public string BackgroundFirstLine { get; set; }
        public FontContract SecondLineFont { get; set; }
        public string ForegroundSecondLine { get; set; }
        public string BackgroundSecondLine { get; set; }
    }

    public struct OverlayLayoutContract
    {
        public bool AutoExpand { get; set; }
        public Point Origin { get; set; }
        public int ParagraphSpacing { get; set; }
        public int TargetWidth { get; set; }
        public int TargetHeight { get; set; }
    }

    public struct OverlayContextContract
    {
        public string Title { get; set; }
        public string FirstLine { get; set; }
        public string SecondLine { get; set; }
    }


    public struct Point
    {
        public int X { get; set; }
        public int Y { get; set; }
    }

}