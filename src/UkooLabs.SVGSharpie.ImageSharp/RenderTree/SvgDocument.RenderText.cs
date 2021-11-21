using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using UkooLabs.SVGSharpie.ImageSharp.Shapes;
using UkooLabs.SVGSharpie;
using SixLabors.ImageSharp.Drawing;

namespace UkooLabs.SVGSharpie.ImageSharp.Dom
{
    internal sealed partial class SvgDocumentRenderer<TPixel> : SvgElementWalker
        where TPixel : unmanaged, IPixel<TPixel>
    {
        public static string DefaultFont { get; set; } = "Times New Roman";
        public static string DefaultSansSerifFont { get; set; } = "Arial";
        public static string DefaultSerifFont { get; set; } = "Times New Roman";
        public override void VisitTextElement(SvgTextElement element)
        {
            base.VisitTextElement(element);

            var fonts = SystemFonts.Collection;
            fonts.TryGet(DefaultFont, out FontFamily family);

            foreach (var f in element.Style.FontFamily.Value)
            {
                var fontName = f;
                if (fontName.Equals("sans-serif"))
                {
                    fontName = DefaultSansSerifFont;
                }
                else if (fontName.Equals("serif"))
                {
                    fontName = DefaultSerifFont;
                }

                if (fonts.TryGet(fontName, out family))
                {
                    break;
                }
            }

            if (family == null)
            {
                fonts.TryGet(DefaultFont, out family);
            }

            var fontSize = element.Style.FontSize.Value.Value;
            var origin = new PointF(element.X?.Value ?? 0, element.Y?.Value ?? 0);
            var font = new Font(family, fontSize);

            var visitor = new SvgTextSpanTextVisitor();
            element.Accept(visitor);
            var text = visitor.Text;

            // offset by the ascender to account for fonts render origin of top left
            var ascender = ((font.FontMetrics.Ascender * font.Size) / (font.FontMetrics.UnitsPerEm * 72)) * 72;

            var render = new TextOptions(font)
            {
                Dpi = 72, 
                Origin = origin - new PointF(0, ascender),
                HorizontalAlignment = element.Style.TextAnchor.Value.AsHorizontalAlignment()
            };

            var glyphs = TextBuilder.GenerateGlyphs(text, render);
            foreach (var p in glyphs)
            {
                this.RenderShapeToCanvas(element, p);
            }
        }

    }


    internal class SvgTextSpanTextVisitor : SvgElementWalker
    {
        private StringBuilder sb = new StringBuilder();

        public string Text => sb.ToString();

        public override void VisitInlineTextSpanElement(SvgInlineTextSpanElement element)
        {
            sb.Append(element.Text);
        }
    }
}
