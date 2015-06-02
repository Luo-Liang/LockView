using GraphicsOverlay;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Web;

namespace InfoView.DataContract
{
    [DataContract]
    public struct OverlayFormattingContract
    {
        static Dictionary<string, Color> Colors;
        static OverlayFormattingContract()
        {
            Colors = typeof(Color).GetProperties(System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public).Select<PropertyInfo, Color>(pi => (Color)pi.GetValue(null)).ToDictionary<Color, string>(p => p.Name);
        }
        [DataMember]
        public FontContract TitleFont { get; set; }
        [DataMember]
        public string ForegroundTitle { get; set; }
        [DataMember]
        public string BackgroundTitle { get; set; }
        [DataMember]
        public FontContract FirstLineFont { get; set; }
        [DataMember]
        public string ForegroundFirstLine { get; set; }
        [DataMember]
        public string BackgroundFirstLine { get; set; }
        [DataMember]
        public FontContract SecondLineFont { get; set; }
        [DataMember]
        public string ForegroundSecondLine { get; set; }
        [DataMember]
        public string BackgroundSecondLine { get; set; }
        public OverlayFormatting ToOverlayFormatting()
        {
            return new OverlayFormatting()
            {
                BackgroundFirstLine = new SolidBrush(Colors[this.BackgroundFirstLine]),
                ForegroundFirstLine = new SolidBrush(Colors[this.ForegroundFirstLine]),
                BackgroundTitle = new SolidBrush(Colors[this.BackgroundTitle]),
                ForegroundTitle = new SolidBrush(Colors[this.ForegroundTitle]),
                BackgroundSecondLine = new SolidBrush(Colors[this.BackgroundSecondLine]),
                ForegroundSecondLine = new SolidBrush(Colors[this.ForegroundSecondLine]),

                TitleFont = new Font(this.TitleFont.FontFamily, this.TitleFont.FontSize),
                FirstLineFont = new Font(this.FirstLineFont.FontFamily, this.FirstLineFont.FontSize),
                SecondLineFont = new Font(this.SecondLineFont.FontFamily, this.SecondLineFont.FontSize)
            };
        }
    }

    [DataContract]
    public struct OverlayContextContract
    {
        [DataMember]
        public string Title { get; set; }
        [DataMember]
        public string FirstLine { get; set; }
        [DataMember] 
        public string SecondLine { get; set; }
        public OverlayContext ToOverlayContext()
        {
            return new OverlayContext()
            {
                FirstLine = this.FirstLine,
                SecondLine = this.SecondLine,
                Title = this.Title
            };
        }
    }
    [DataContract]
    public struct OverlayLayoutContract
    {
        [DataMember]
        public bool AutoExpand { get; set; }
        [DataMember]
        public Point Origin { get; set; }
        [DataMember]
        public int ParagraphSpacing { get; set; }
        [DataMember]
        public int TargetWidth { get; set; }
        [DataMember]
        public int TargetHeight { get; set; }
        public OverlayLayout ToOverlayLayout()
        {
            return new OverlayLayout()
            {
                AutoExpand = this.AutoExpand,
                Origin = new System.Drawing.Point(this.Origin.X,this.Origin.Y),
                ParagraphSpacing = this.ParagraphSpacing,
                TargetHeight = this.TargetHeight,
                TargetWidth = this.TargetWidth
            };
        }
    }

    [DataContract]
    public struct FontContract
    {
        [DataMember]
        public string FontFamily { get; set; }
        [DataMember]
        public int FontSize { get; set; }
    }
}