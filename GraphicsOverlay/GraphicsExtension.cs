using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GraphicsOverlay
{
    public static class GraphicsExtension
    {
        //returns the next startingY.
        static int applyText(Graphics graphics, int maxWidth, int maxHeight, int startingY, int startingX, string str2Draw, Font font, Brush brush)
        {
            var desiredHeight = (int)Math.Ceiling(graphics.MeasureString(str2Draw, font, maxWidth).Height);
            var rect = new Rectangle(startingX, startingY, maxWidth, Math.Min(desiredHeight, maxHeight));
            graphics.DrawString(str2Draw, font, brush, rect);
            return startingY + rect.Height;
        }
        public static void Apply(this Graphics graphics, OverlayLayout layout, OverlayContext context, OverlayFormatting formatting)
        {
            int maxWidth = layout.TargetWidth - 2 * layout.Origin.X;
            int maxHeight = layout.TargetHeight - 2 * layout.Origin.Y;
            int startingY = layout.Origin.Y;
            startingY = applyText(graphics, maxWidth, maxHeight, startingY, layout.Origin.X, context.Title, formatting.TitleFont, formatting.ForegroundTitle);
            maxHeight -= startingY;
            startingY = applyText(graphics, maxWidth, maxHeight - startingY, startingY, layout.Origin.X, context.FirstLine, formatting.FirstLineFont, formatting.ForegroundFirstLine);
            maxHeight -= startingY;
            startingY = applyText(graphics, maxWidth, maxHeight, startingY, layout.Origin.X, context.SecondLine, formatting.SecondLineFont, formatting.ForegroundSecondLine);
        }
    }
}
