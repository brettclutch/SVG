﻿using System.Linq;
using System.Threading.Tasks;
using Svg.Core.Interfaces;

namespace Svg.Core.Tools
{
    /// <summary>
    /// Adds some visualizations useful for debugging
    /// </summary>
    public class DebugTool : ToolBase
    {
        private Brush _brush2;
        private Pen _pen2;
        private Brush _brush3;
        private Pen _pen3;

        public DebugTool() : base("Debug")
        {
        }

        private Brush RedBrush => _brush2 ?? (_brush2 = Svg.Engine.Factory.CreateSolidBrush(Svg.Engine.Factory.CreateColorFromArgb(255, 255, 150, 150)));
        private Pen RedPen => _pen2 ?? (_pen2 = Svg.Engine.Factory.CreatePen(RedBrush, 5));

        private Brush GreenBrush => _brush3 ?? (_brush3 = Svg.Engine.Factory.CreateSolidBrush(Svg.Engine.Factory.CreateColorFromArgb(255, 0, 128, 0)));
        private Pen GreenPen => _pen3 ?? (_pen3 = Svg.Engine.Factory.CreatePen(GreenBrush, 5));

        public override Task Initialize(SvgDrawingCanvas ws)
        {
            Commands = new[]
             {
                new ToolCommand(this, "Toggle Bounding Path", (x) =>
                {
                    BoundingPathEnabled = !BoundingPathEnabled;
                    ws.FireInvalidateCanvas();
                }, iconName: null, sortFunc:(x) => 1500),
                new ToolCommand(this, "Toggle Bounding Box", (x) =>
                {
                    BoundingBoxEnabled = !BoundingBoxEnabled;
                    ws.FireInvalidateCanvas();
                }, iconName: null, sortFunc:(x) => 1500),

            };

            return Task.FromResult(true);
        }

        private bool BoundingBoxEnabled { get; set; }

        private bool BoundingPathEnabled { get; set; }

        public override Task OnDraw(IRenderer renderer, SvgDrawingCanvas ws)
        {
            if (BoundingPathEnabled)
            {
                // for debugging: connects all element points with a red line (clockwise)
                foreach (var element in ws.Document.Children.OfType<SvgVisualElement>())
                {
                    renderer.Graphics.Save();

                    var bds = element.GetTransformedPoints();
                    renderer.DrawPolygon(bds, RedPen);
                    renderer.Graphics.Restore();
                }
            }

            if (BoundingBoxEnabled)
            {
                // for debugging: draws green line around all selectable elements
                // this shows the transformed rendermatrix
                foreach (var element in ws.Document.Children.OfType<SvgVisualElement>())
                {
                    renderer.Graphics.Save();
                    var m = renderer.Graphics.Transform.Clone();
                    m.Invert();
                    renderer.Graphics.Concat(m);

                    var box = element.GetBoundingBox(ws.GetCanvasTransformationMatrix());

                    renderer.DrawRectangle(box, GreenPen);
                    renderer.Graphics.Restore();
                }
            }

            return Task.FromResult(true);
        }
    }
}