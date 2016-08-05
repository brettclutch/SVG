using System.Drawing;
using Svg.Interfaces;

namespace Svg
{
    /// <summary>
    /// Represents and SVG ellipse element.
    /// </summary>
    [SvgElement("ellipse")]
    public class SvgEllipse : SvgVisualElement
    {
        private SvgUnit _radiusX;
        private SvgUnit _radiusY;
        private SvgUnit _centerX;
        private SvgUnit _centerY;
        private GraphicsPath _path;

        [SvgAttribute("cx")]
        public virtual SvgUnit CenterX
        {
            get { return this._centerX; }
            set
            {
            	if(_centerX != value)
	            {
	                var oldValue = _centerX;
            		this._centerX = value;
            		this.IsPathDirty = true;
            		OnAttributeChanged(new AttributeEventArgs("cx", value, oldValue));
            	}
            }
        }

        [SvgAttribute("cy")]
        public virtual SvgUnit CenterY
        {
        	get { return this._centerY; }
        	set
        	{
        		if(_centerY != value)
                {
                    var oldValue = _centerY;
                    this._centerY = value;
        			this.IsPathDirty = true;
        			OnAttributeChanged(new AttributeEventArgs("cy", value, oldValue));
        		}
        	}
        }

        [SvgAttribute("rx")]
        public virtual SvgUnit RadiusX
        {
        	get { return this._radiusX; }
        	set
        	{
        		if(_radiusX != value)
		        {
		            var oldValue = _radiusX;
        			this._radiusX = value;
        			this.IsPathDirty = true;
        			OnAttributeChanged(new AttributeEventArgs("rx", value, oldValue));
        		}
        	}
        }

        [SvgAttribute("ry")]
        public virtual SvgUnit RadiusY
        {
        	get { return this._radiusY; }
        	set
        	{
        		if(_radiusY != value)
		        {
		            var oldValue = _radiusY;
        			this._radiusY = value;
        			this.IsPathDirty = true;
        			OnAttributeChanged(new AttributeEventArgs("ry", value, oldValue));
        		}
        	}
        }

        /// <summary>
        /// Gets or sets a value to determine if anti-aliasing should occur when the element is being rendered.
        /// </summary>
        /// <value></value>
        protected override bool RequiresSmoothRendering
        {
            get { return true; }
        }

        /// <summary>
        /// Gets the bounds of the element.
        /// </summary>
        /// <value>The bounds.</value>
        public override RectangleF Bounds
        {
            get
            {
                return this.Path(null).GetBounds();
            }
        }

        /// <summary>
        /// Gets the <see cref="GraphicsPath"/> for this element.
        /// </summary>
        /// <value></value>
        public override GraphicsPath Path(ISvgRenderer renderer)
        {
            if (this._path == null || this.IsPathDirty)
            {
                var center = SvgUnit.GetDevicePoint(this._centerX, this._centerY, renderer, this);
                var radius = SvgUnit.GetDevicePoint(this._radiusX, this._radiusY, renderer, this);

                this._path = Engine.Factory.CreateGraphicsPath();
                _path.StartFigure();
                _path.AddEllipse(center.X - radius.X, center.Y - radius.Y, 2 * radius.X, 2 * radius.Y);
                _path.CloseFigure();
                this.IsPathDirty = false;
            }
            return _path;
        }

        /// <summary>
        /// Renders the <see cref="SvgElement"/> and contents to the specified <see cref="Graphics"/> object.
        /// </summary>
        /// <param name="graphics">The <see cref="Graphics"/> object to render to.</param>
        protected override void Render(ISvgRenderer renderer)
        {
            if (this._radiusX.Value > 0.0f && this._radiusY.Value > 0.0f)
            {
                base.Render(renderer);
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SvgEllipse"/> class.
        /// </summary>
        public SvgEllipse()
        {
        }



		public override SvgElement DeepCopy()
		{
			return DeepCopy<SvgEllipse>();
		}

		public override SvgElement DeepCopy<T>()
		{
			var newObj = base.DeepCopy<T>() as SvgEllipse;
			newObj.CenterX = this.CenterX;
			newObj.CenterY = this.CenterY;
			newObj.RadiusX = this.RadiusX;
			newObj.RadiusY = this.RadiusY;
			return newObj;
		}
    }
}