﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;
using Svg.Interfaces;

namespace Svg
{
    public class GdiFontDefn : IFontDefn
    {
        private Font _font;

        public float Size
        {
            get { return _font.Size; }
        }
        public float SizeInPoints
        {
            get { return _font.SizeInPoints; }
        }

        public GdiFontDefn(Font font)
        {
            _font = font;
        }

        public void AddStringToPath(ISvgRenderer renderer, GraphicsPath path, string text, PointF location)
        {
            path.AddString(text, _font.FontFamily, (int)_font.Style, _font.Size, location, Engine.Factory.CreateStringFormatGenericTypographic());
        }

        //Baseline calculation to match http://bobpowell.net/formattingtext.aspx
        public float Ascent(ISvgRenderer renderer)
        {
            var ff = _font.FontFamily;
            float ascent = ff.GetCellAscent(_font.Style);
            float baselineOffset = _font.SizeInPoints / ff.GetEmHeight(_font.Style) * ascent;
            return renderer.DpiY / 72f * baselineOffset;
        }

        public IList<RectangleF> MeasureCharacters(ISvgRenderer renderer, string text)
        {
            var g = GetGraphics(renderer);
            var regions = new List<RectangleF>();
            StringFormat format;
            for (int s = 0; s <= (text.Length - 1) / 32; s++)
            {
                format = Engine.Factory.CreateStringFormatGenericTypographic();
                format.FormatFlags |= StringFormatFlags.MeasureTrailingSpaces;
                format.SetMeasurableCharacterRanges((from r in Enumerable.Range(32 * s, Math.Min(32, text.Length - 32 * s))
                                                     select new CharacterRange(r, 1)).ToArray());
                regions.AddRange(from r in g.MeasureCharacterRanges(text, _font, Engine.Factory.CreateRectangleF(0, 0, 1000, 1000), format)
                                 select r.GetBounds(g));
            }
            return regions;
        }

        public SizeF MeasureString(ISvgRenderer renderer, string text)
        {
            var g = GetGraphics(renderer);
            var provider = Engine.Factory.GetFontFamilyProvider();
            StringFormat format = provider.GenericTypographic;
            format.SetMeasurableCharacterRanges(new CharacterRange[] { new CharacterRange(0, text.Length) });
            format.FormatFlags |= StringFormatFlags.MeasureTrailingSpaces;
            Region[] r = g.MeasureCharacterRanges(text, _font, Engine.Factory.CreateRectangleF(0, 0, 1000, 1000), format);
            RectangleF rect = r[0].GetBounds(g);

            return Engine.Factory.CreateSizeF(rect.Width, Ascent(renderer));
        }

        private static Graphics _graphics;
        private static Graphics GetGraphics(object renderer)
        {
            var provider = renderer as IGraphicsProvider;
            if (provider == null)
            {
                if (_graphics == null)
                {
                    var bmp = Engine.Factory.CreateBitmap(1, 1);
                    _graphics = Engine.Factory.CreateGraphicsFromImage(bmp);
                }
                return _graphics;
            }
            else
            {
                return provider.GetGraphics();
            }
        }

        public void Dispose()
        {
            _font.Dispose();
        }
    }
}