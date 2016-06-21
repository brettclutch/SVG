﻿

namespace Svg
{
    /// <summary>
    /// The <see cref="SvgText"/> element defines a graphics element consisting of text.
    /// </summary>
    [SvgElement("text")]
    public class SvgText : SvgTextBase
    {
        /// <summary>
        /// Initializes the <see cref="SvgText"/> class.
        /// </summary>
        public SvgText() : base() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="SvgText"/> class.
        /// </summary>
        /// <param name="text">The text.</param>
        public SvgText(string text)
            : this()
        {
            this.Text = text;
        }

        public override SvgElement DeepCopy()
        {
            return DeepCopy<SvgText>();
        }

        public override bool ShouldWriteElement()
        {
            return (!string.IsNullOrEmpty(this.Text)) || base.ShouldWriteElement();
        }

        public override SvgElement DeepCopy<T>()
        {
            var newObj = base.DeepCopy<T>() as SvgText;
            newObj.TextAnchor = this.TextAnchor;
            newObj.WordSpacing = this.WordSpacing;
            newObj.LetterSpacing = this.LetterSpacing;
            newObj.Font = this.Font;
            newObj.FontFamily = this.FontFamily;
            newObj.FontSize = this.FontSize;
            newObj.FontWeight = this.FontWeight;
            newObj.X = this.X;
            newObj.Y = this.Y;
            return newObj;
        }
    }
}