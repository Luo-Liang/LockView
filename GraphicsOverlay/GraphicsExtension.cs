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
        /// <summary>
        /// 
        /// </summary>
        /// <param name="graphics"></param>
        /// <param name="maxWidth"></param>
        /// <param name="maxHeight"></param>
        /// <param name="startingY"></param>
        /// <param name="startingX"></param>
        /// <param name="str2Draw"></param>
        /// <param name="font"></param>
        /// <param name="brush"></param>
        /// <param name="success"></param>
        /// <returns>If for any reason, most likely due to insufficent space, false is returned.</returns>
        static int applyText(Graphics graphics,
                             int maxWidth,
                             int maxHeight,
                             int startingY,
                             int startingX,
                             string str2Draw,
                             Font font,
                             Brush brush,
                             out bool success)
        {
            success = true;
            var desiredHeight = (int)Math.Ceiling(graphics.MeasureString(str2Draw, font, maxWidth).Height);
            var rect = new Rectangle(startingX, startingY, maxWidth, Math.Min(desiredHeight, maxHeight));
            if (desiredHeight > maxHeight)
            {
                success = false;
            }
            graphics.DrawString(str2Draw, font, brush, rect);
            return startingY + rect.Height;
        }
        static int applyContext(this Graphics graphics,
                                int maxWidth,
                                int maxHeight,
                                int startingY,
                                int startingX,
                                OverlayLayout layout,
                                OverlayContext context,
                                OverlayFormatting formatting,
                                out bool shouldContinue)
        {
            shouldContinue = true;
            startingY = applyText(graphics, maxWidth, maxHeight, startingY, layout.Origin.X, context.Title, formatting.TitleFont, formatting.ForegroundTitle, out shouldContinue);
            maxHeight -= startingY;
            if (shouldContinue)
                startingY = applyText(graphics, maxWidth, maxHeight - startingY, startingY, layout.Origin.X, context.FirstLine, formatting.FirstLineFont, formatting.ForegroundFirstLine, out shouldContinue);
            maxHeight -= startingY;
            if (shouldContinue)
                startingY = applyText(graphics, maxWidth, maxHeight, startingY, layout.Origin.X, context.SecondLine, formatting.SecondLineFont, formatting.ForegroundSecondLine, out shouldContinue);
            return startingY;
        }
        public static void Apply(this Graphics graphics,
                                 OverlayLayout layout,
                                 OverlayContext context,
                                 OverlayContext[] secondaryContexts,
                                 OverlayFormatting formatting)
        {
            int maxWidth = layout.TargetWidth - 2 * layout.Origin.X;
            int maxHeight = layout.TargetHeight - 2 * layout.Origin.Y;
            int startingY = layout.Origin.Y;
            int startingX = layout.Origin.X;
            bool shouldContinue = true;
            startingY = applyContext(graphics, maxWidth, maxHeight, startingY, startingX, layout, context, formatting, out shouldContinue);
            maxHeight -= startingY;
            if (secondaryContexts != null)
            {
                foreach (var ctx in secondaryContexts)
                {
                    if (shouldContinue)
                        startingY = applyContext(graphics, maxWidth, maxHeight, startingY, startingX, layout, ctx, formatting, out shouldContinue);
                    maxHeight -= startingY;
                }
            }
        }
    }
}
