using GraphicsOverlay;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Text;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Web;

namespace InfoView.DataContract
{
    [DataContract, Newtonsoft.Json.JsonObject(MemberSerialization = Newtonsoft.Json.MemberSerialization.OptIn)]
    public struct OverlayFormattingContract
    {
        internal static FontContract DefaultFont;
        internal static SolidBrush DefaultBrush;
        internal static Dictionary<string, Color> Colors;
        internal static Dictionary<Color, string> ColorNames;
        internal static PrivateFontCollection FontCollection;
        static OverlayFormattingContract()
        {
            var systemColors = typeof(Color).GetProperties(System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public).Select<PropertyInfo, Color>(pi => (Color)pi.GetValue(null));
            Colors = systemColors.ToDictionary<Color, string>(p => p.Name);
            ColorNames = new Dictionary<Color, string>();
            foreach (var color in systemColors)
            {
                ColorNames[color] = color.Name;
            }
            DefaultFont = new FontContract() { FontFamily = "Segoe UI Semibold", FontSize = 16 };
            DefaultBrush = new SolidBrush(Color.Transparent);
            FontCollection = new PrivateFontCollection();
            var fontFiles = Directory.GetFiles($"{AppDomain.CurrentDomain.BaseDirectory}\\Fonts");
            foreach (var file in fontFiles)
                FontCollection.AddFontFile(file);

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

            if (BackgroundFirstLine == null) BackgroundFirstLine = DefaultBrush.Color.Name;
            if (BackgroundSecondLine == null) BackgroundSecondLine = DefaultBrush.Color.Name;
            if (BackgroundTitle == null) BackgroundTitle = DefaultBrush.Color.Name;
            if (ForegroundFirstLine == null) ForegroundFirstLine = DefaultBrush.Color.Name;
            if (ForegroundSecondLine == null) ForegroundSecondLine = DefaultBrush.Color.Name;
            if (ForegroundTitle == null) ForegroundTitle = DefaultBrush.Color.Name;
            var value = new OverlayFormatting()
            {
                BackgroundFirstLine = new SolidBrush(Colors[this.BackgroundFirstLine]),
                ForegroundFirstLine = new SolidBrush(Colors[this.ForegroundFirstLine]),
                BackgroundTitle = new SolidBrush(Colors[this.BackgroundTitle]),
                ForegroundTitle = new SolidBrush(Colors[this.ForegroundTitle]),
                BackgroundSecondLine = new SolidBrush(Colors[this.BackgroundSecondLine]),
                ForegroundSecondLine = new SolidBrush(Colors[this.ForegroundSecondLine])
            };
            try
            {
                value.TitleFont = new Font(new FontFamily(this.TitleFont.FontFamily, FontCollection), this.TitleFont.FontSize);
                value.FirstLineFont = new Font(new FontFamily(this.FirstLineFont.FontFamily, FontCollection), this.FirstLineFont.FontSize);
                value.SecondLineFont = new Font(new FontFamily(this.SecondLineFont.FontFamily, FontCollection), this.SecondLineFont.FontSize);
            }
            catch
            {
                value.TitleFont = new Font(TitleFont.FontFamily, this.TitleFont.FontSize);
                value.FirstLineFont = new Font(this.FirstLineFont.FontFamily, this.FirstLineFont.FontSize);
                value.SecondLineFont = new Font(this.SecondLineFont.FontFamily, this.SecondLineFont.FontSize);
            }
            return value;
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

    [DataContract, Newtonsoft.Json.JsonObject(MemberSerialization = Newtonsoft.Json.MemberSerialization.OptIn)]
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

    [DataContract, Newtonsoft.Json.JsonObject(MemberSerialization = Newtonsoft.Json.MemberSerialization.OptIn)]
    public class Point
    {
        [DataMember]
        public int X { get; set; }
        [DataMember]
        public int Y { get; set; }
    }

    [DataContract, Newtonsoft.Json.JsonObject(MemberSerialization = Newtonsoft.Json.MemberSerialization.OptIn)]
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

    [DataContract, Newtonsoft.Json.JsonObject(MemberSerialization = Newtonsoft.Json.MemberSerialization.OptIn)]
    public struct FontContract
    {
        [DataMember]
        public string FontFamily { get; set; }
        [DataMember]
        public int FontSize { get; set; }
    }

    public enum ImageSource
    {
        [DataMember]
        Local,
        [DataMember]
        Bing
    }
}