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
        /// <param name="maxY"></param>
        /// <param name="startingY"></param>
        /// <param name="startingX"></param>
        /// <param name="str2Draw"></param>
        /// <param name="font"></param>
        /// <param name="brush"></param>
        /// <param name="success"></param>
        /// <returns>If for any reason, most likely due to insufficent space, false is returned.</returns>
        static int applyText(Graphics graphics,
                             int maxWidth,
                             int maxY,
                             int startingY,
                             int startingX,
                             string str2Draw,
                             Font font,
                             Brush brush,
                             out bool success)
        {
            success = true;
            var desiredHeight = (int)Math.Ceiling(graphics.MeasureString(str2Draw, font, maxWidth).Height);
            var rect = new Rectangle(startingX, startingY, maxWidth, Math.Min(desiredHeight, maxY));
            if (desiredHeight + startingY > maxY)
            {
                success = false;
            }
            graphics.DrawString(str2Draw, font, brush, rect);
            return startingY + rect.Height;
        }
        static int applyContext(this Graphics graphics,
                                int maxWidth,
                                int maxY,
                                int startingY,
                                int startingX,
                                OverlayLayout layout,
                                OverlayContext context,
                                OverlayFormatting formatting,
                                out bool shouldContinue)
        {
            shouldContinue = true;
            startingY = applyText(graphics, maxWidth, maxY, startingY, layout.Origin.X, context.Title, formatting.TitleFont, formatting.ForegroundTitle, out shouldContinue);
            if (shouldContinue)
                startingY = applyText(graphics, maxWidth, maxY, startingY, layout.Origin.X, context.FirstLine, formatting.FirstLineFont, formatting.ForegroundFirstLine, out shouldContinue);
            if (shouldContinue)
                startingY = applyText(graphics, maxWidth, maxY, startingY, layout.Origin.X, context.SecondLine, formatting.SecondLineFont, formatting.ForegroundSecondLine, out shouldContinue);
            return startingY;
        }
        public static void Apply(this Graphics graphics,
                                 OverlayLayout layout,
                                 OverlayContext[] contexts,
                                 OverlayFormatting formatting)
        {
            int maxWidth = layout.TargetWidth - 2 * layout.Origin.X;
            int maxY = layout.TargetHeight - layout.Origin.Y;
            maxY = maxY * 2 / 3;
            int startingY = layout.Origin.Y;
            int startingX = layout.Origin.X;
            bool shouldContinue = true;
            foreach (var ctx in contexts)
            {
                if (shouldContinue)
                {
                    startingY = applyContext(graphics, maxWidth, maxY, startingY, startingX, layout, ctx, formatting, out shouldContinue);
                    startingY += 10; //<--- this is the spacing btw sources.
                }
            }
        }
    }
}
