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
        static Dictionary<Color, string> ColorNames;
        static OverlayFormattingContract()
        {
            var systemColors = typeof(Color).GetProperties(System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public).Select<PropertyInfo, Color>(pi => (Color)pi.GetValue(null));
            Colors = systemColors.ToDictionary<Color, string>(p => p.Name);
            ColorNames = new Dictionary<Color, string>();
            foreach (var color in systemColors)
            {
                ColorNames[color] = color.Name;
            }
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
        public static OverlayFormattingContract FromOverlayFormatting(OverlayFormatting formatting)
        {
            OverlayFormattingContract contract = new OverlayFormattingContract();
            if (formatting.BackgroundFirstLine != null)
                contract.BackgroundFirstLine = ColorNames[((SolidBrush)formatting.BackgroundFirstLine).Color];
            if (formatting.BackgroundSecondLine != null)
                contract.BackgroundSecondLine = ColorNames[((SolidBrush)formatting.BackgroundSecondLine).Color];
            if (formatting.BackgroundTitle != null)
                contract.BackgroundTitle = ColorNames[((SolidBrush)formatting.BackgroundTitle).Color];
            if (formatting.FirstLineFont != null)
                contract.FirstLineFont = new FontContract() { FontFamily = formatting.FirstLineFont.Name, FontSize = (int)formatting.FirstLineFont.Size };
            if (formatting.SecondLineFont != null)
                contract.SecondLineFont = new FontContract() { FontFamily = formatting.SecondLineFont.Name, FontSize = (int)formatting.SecondLineFont.Size };
            if (formatting.TitleFont != null)
                contract.TitleFont = new FontContract() { FontFamily = formatting.TitleFont.Name, FontSize = (int)formatting.TitleFont.Size };
            if (formatting.ForegroundFirstLine != null)
                contract.ForegroundFirstLine = ColorNames[((SolidBrush)formatting.ForegroundFirstLine).Color];
            if (formatting.ForegroundSecondLine != null)
                contract.ForegroundSecondLine = ColorNames[((SolidBrush)formatting.ForegroundSecondLine).Color];
            if (formatting.ForegroundTitle != null)
                contract.ForegroundTitle = ColorNames[((SolidBrush)formatting.ForegroundTitle).Color];
            return contract;
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
        public static OverlayContextContract FromOverlayContext(OverlayContext context)
        {
            OverlayContextContract contract = new OverlayContextContract();
            contract.Title = context.Title;
            contract.FirstLine = context.FirstLine;
            contract.SecondLine = context.SecondLine;
            return contract;
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
                Origin = new System.Drawing.Point(this.Origin.X, this.Origin.Y),
                ParagraphSpacing = this.ParagraphSpacing,
                TargetHeight = this.TargetHeight,
                TargetWidth = this.TargetWidth
            };
        }
        public static OverlayLayoutContract FromOverlayLayout(OverlayLayout layout)
        {
            OverlayLayoutContract contract = new OverlayLayoutContract();
            contract.AutoExpand = layout.AutoExpand;
            contract.Origin = new Point() { X = layout.Origin.X, Y = layout.Origin.Y };
            contract.ParagraphSpacing = layout.ParagraphSpacing;
            contract.TargetHeight = layout.TargetHeight;
            contract.TargetWidth = layout.TargetWidth;
            return contract;
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