using System;
using Android.App;
using Android.Content;
using Android.Graphics;
using Android.Util;
using Android.Views;
using Android.Widget;
using Svg.Core;
using Svg.Core.Tools;
using Svg.Droid.Editor.Services;
using GestureDetector = Svg.Droid.Editor.Services.GestureDetector;

namespace Svg.Droid.Editor
{
    public class SvgDrawingCanvasView : ImageView
    {
        private readonly GestureDetector _detector;
        private SvgDrawingCanvas _drawingCanvas;

        public SvgDrawingCanvas DrawingCanvas
        {
            get { return _drawingCanvas; }
            set { _drawingCanvas = value; }
        }

        public SvgDrawingCanvasView(Context context, IAttributeSet attr) : base(context, attr)
        {
            // Initialize SVG Platform and tie together PCL and platform specific modules
            SvgPlatformSetup.Init(new SvgAndroidPlatformOptions(context) {EnableFastTextRendering = true});
            
            _drawingCanvas = new SvgDrawingCanvas();
            _detector = new GestureDetector(this.Context, (e) => DrawingCanvas.OnEvent(e));

            Engine.Register<ITextInputService, TextInputService>(() => new TextInputService(context));
        }

        public override bool OnTouchEvent(MotionEvent ev)
        {
            // this is intentionally not awaited
            _detector.OnTouch(ev).ConfigureAwait(false);
            
            return true;
        }

        protected override void OnDraw(Canvas canvas)
        {
            // this is intentionally not awaited
            DrawingCanvas.OnDraw(new AndroidCanvasRenderer(canvas))
                .ContinueWith(t => base.OnDraw(canvas));
        }

        protected override void OnAttachedToWindow()
        {
            base.OnAttachedToWindow();
            _drawingCanvas.CanvasInvalidated -= OnCanvasInvalidated;
            _drawingCanvas.CanvasInvalidated += OnCanvasInvalidated;
            _drawingCanvas.ToolCommandsChanged -= OnToolCommandsChanged;
            _drawingCanvas.ToolCommandsChanged += OnToolCommandsChanged;
        }

        protected override void OnDetachedFromWindow()
        {
            _drawingCanvas.CanvasInvalidated -= OnCanvasInvalidated;
            _drawingCanvas.ToolCommandsChanged -= OnToolCommandsChanged;
            base.OnDetachedFromWindow();
        }

        private void OnCanvasInvalidated(object sender, EventArgs e)
        {
            Invalidate();
        }

        private void OnToolCommandsChanged(object sender, EventArgs e)
        {
            ((Activity)this.Context).InvalidateOptionsMenu();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                DrawingCanvas?.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}