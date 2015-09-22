using Newtonsoft.Json;
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

    public class ImageRequestOverride
    {
        public string ImageRequestUrl { get; set; }
        public string Arguments { get; set; }
    }

    /// <summary>
    /// Must keep in sync with the service's format.
    /// </summary>
    public class ImageCompositionRequest
    {
        public byte[] RawImage { get; set; }
        public ImageRequestOverride ImageRequestOverride { get; set; }
        public OverlayFormattingContract FormattingContract { get; set; }
        public OverlayLayoutContract LayoutContract { get; set; }
        public OverlayContextContract ContextContract { get; set; }
        public OverlayContextContract[] SecondaryContextContracts { get; set; }
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
        [JsonIgnore]
        public string ExtendedUri { get; set; }
        [JsonIgnore]
        public string JumpUri { get; set; }
        public string FirstLine { get; set; }
        public string SecondLine { get; set; }
        public string Title { get; set; }
        public override bool Equals(object obj)
        {
            var other = obj as OverlayContextContract;
            if (other != null)
            {
                return other.Title == Title && other.FirstLine == FirstLine && other.JumpUri == JumpUri;
            }
            var other1 = obj as InterestGathering.InterestContent;
            if (other1 != null)
            {
                return other1.Title == Title && other1.Content == FirstLine;
            }
            return base.Equals(obj);
        }
    }

    public enum ImageSource
    {
        Local,
        Bing
    }

    public class Point
    {
        public int X { get; set; }
        public int Y { get; set; }
    }

    public class ImageCompositionResponse
    {
        public byte[] Image { get; set; }
        public string ResultString { get; set; }
    }
}