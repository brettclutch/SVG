using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Svg.Core.Events;
using Svg.Core.Interfaces;
using Svg.Interfaces;
using Svg.Transforms;

namespace Svg.Core.Tools
{
    public class GridTool : ToolBase
    {
        private SvgDrawingCanvas _canvas;
        private float StepSizeY = 40;
        private double A;
        private double B;
        private double C;
        private float StepSizeX;
        private double Alpha = 27.3f;
        private double Gamma = 90f;
        private double Beta;
        private Pen _pen;

        private Pen _pen2;
        private Brush _brush;
        private Brush _brush2;

        private bool _isSnappingInProgress = false;
        private bool _areElementsMoved = false;
        private PointF _generalTranslation = null;

        public GridTool(float angle = 30f, int stepSizeY = 20)
            : base("Grid")
        {
            StepSizeY = stepSizeY;
            Alpha = angle;

            // using triangle calculation to determine the x and y steps based on stepsize (y) and angle (alpha)
            // http://www.arndt-bruenner.de/mathe/scripts/Dreiecksberechnung.htm
            /*
                                                  XXX+
                                               XXX   |
                                             XX      |
                                          XXX  B = ? |
                                        XXX          |
                                      XXX            |
                           c = ?   XXX               |
                                 XXX                 |
                               XXX                   |  a = 20
                             XXX                     |
                           XXX                       |
                         XXX                         |
                       XXX                           |
                     XXX    A = 27.3          G = 90 |
                    XX-------------------------------+
                              b = ?
             * */
            A = StepSizeY/2;
            Beta = 180f - (Alpha + Gamma);
            B = (A * SinDegree(Beta)) / SinDegree(Alpha);
            C = (A * SinDegree(Gamma)) / SinDegree(Alpha);
            StepSizeX = (float)B * 2;
        }

        public override Task Initialize(SvgDrawingCanvas ws)
        {
            _canvas = ws;
            // add tool commands
            Commands = new List<IToolCommand>
            {
                new ToolCommand(this, "Toggle Grid", (obj) =>
                {
                    IsVisible = !IsVisible;
                    _canvas.FireInvalidateCanvas();
                }, (obj) => true)
            };

            // initialize with callbacks
            this.WatchDocument(ws.Document);

            return Task.FromResult(true);
        }

        public bool IsVisible { get; set; } = true;

        private Brush Brush => _brush ?? (_brush = Svg.Engine.Factory.CreateSolidBrush(Svg.Engine.Factory.CreateColorFromArgb(255, 210, 210, 210)));
        private Brush Brush2 => _brush2 ?? (_brush2 = Svg.Engine.Factory.CreateSolidBrush(Svg.Engine.Factory.CreateColorFromArgb(255, 255, 0, 0)));
        private Pen Pen => _pen ?? (_pen = Svg.Engine.Factory.CreatePen(Brush, 1));
        private Pen Pen2 => _pen2 ?? (_pen2 = Svg.Engine.Factory.CreatePen(Brush2, 2));

        public override Task OnPreDraw(IRenderer renderer, SvgDrawingCanvas ws)
        {
            if (!IsVisible)
                return Task.FromResult(true); ;

            // draw gridlines
            DrawGridLines(renderer, ws);

            // draw debug stuff
            var canvasx = -ws.RelativeTranslate.X;
            var canvasy = -ws.RelativeTranslate.Y;
            renderer.DrawCircle(canvasx, canvasy, 50, Pen); // point should remain in top left corner on screen
            renderer.DrawCircle(0, 0, 20, Pen2); // point on canvas - should move along
            renderer.DrawLine(1f, 1f, 200f, 1f, Pen2);

            return Task.FromResult(true);
        }

        public override void OnDocumentChanged(SvgDocument oldDocument, SvgDocument newDocument)
        {
            // add watch for element snapping
            WatchDocument(newDocument);
            UnWatchDocument(oldDocument);
        }

        public override Task OnUserInput(UserInputEvent @event, SvgDrawingCanvas ws)
        {
            var me = @event as MoveEvent;
            if (me != null)
            {
                _areElementsMoved = true;
                _generalTranslation = null;
            }
            else
            {
                _generalTranslation = null;
                _areElementsMoved = false;
            }
            return Task.FromResult(true);
        }

        #region GridLines
        
        private float DrawGridLines(IRenderer renderer, SvgDrawingCanvas ws)
        {
            var canvasx = -ws.RelativeTranslate.X;
            var canvasy = -ws.RelativeTranslate.Y;

            var relativeCanvasTranslationX = (canvasx) % StepSizeX;
            var relativeCanvasTranslationY = (canvasy) % StepSizeY;

            var height = renderer.Height / ws.ZoomFactor;
            var yPosition = (height - (height % StepSizeY) + (StepSizeY * 2));
            var stepSize = (int)Math.Round(StepSizeY, 0);

            var x = canvasx - relativeCanvasTranslationX - (stepSize * 2);
            // subtract 2x stepsize so gridlines always start from "out of sight" and lines do not start from a visible x-border
            var y = canvasy - relativeCanvasTranslationY;
            var lineLength = Math.Sqrt(Math.Pow(renderer.Width, 2) + Math.Pow(renderer.Height, 2)) / ws.ZoomFactor + (stepSize * 4);

            for (var i = y - yPosition; i <= y + yPosition; i += stepSize)
            {
                DrawLineLeftToBottom(renderer, i, x, lineLength); /* \ */
            }

            for (var i = y; i <= y + 2 * yPosition; i += stepSize)
            {
                DrawLineLeftToTop(renderer, i, x, lineLength); /* / */
            }
            return canvasx;
        }
        
        // line looks like this -> /
        private void DrawLineLeftToTop(IRenderer renderer, float y, float canvasX, double lineLength)
        {
            var startX = canvasX;
            var startY = y ;
            var stopX = ((float)(lineLength * Math.Cos(Alpha * (Math.PI / 180)))) + canvasX;
            var stopY = (y - (float)(lineLength * Math.Sin(Alpha * (Math.PI / 180)))) ;
            

            renderer.DrawLine(
                startX,
                startY,
                stopX,
                stopY,
                Pen);
        }

        // line looks like this -> \
        private void DrawLineLeftToBottom(IRenderer renderer, float y, float canvasX, double lineLength)
        {
            var startX = canvasX;
            var startY = y;
            var endX = ((float)(lineLength * Math.Cos(Alpha * (Math.PI / 180)))) + canvasX;
            var endY = (y + (float)(lineLength * Math.Sin(Alpha * (Math.PI / 180))));

            renderer.DrawLine(
                startX,
                startY,
                endX,
                endY,
                Pen);
        }

        #endregion

        #region Snapping

        /// <summary>
        /// Subscribes to all visual elements "Add/RemoveChild" handlers and their "transformCollection changed" event
        /// </summary>
        /// <param name="document"></param>
        private void WatchDocument(SvgDocument document)
        {
            if (document == null)
                return;

            document.ChildAdded -= OnChildAdded;
            document.ChildAdded += OnChildAdded;

            foreach (var child in document.Children.OfType<SvgVisualElement>())
            {
                Subscribe(child);
            }
        }

        private void UnWatchDocument(SvgDocument document)
        {
            if (document == null)
                return;

            document.ChildAdded -= OnChildAdded;

            foreach (var child in document.Children.OfType<SvgVisualElement>())
            {
                Unsubscribe(child);
            }
        }

        private void Subscribe(SvgElement child)
        {
            if (!(child is SvgVisualElement))
                return;

            child.ChildAdded -= OnChildAdded;
            child.ChildAdded += OnChildAdded;
            child.ChildRemoved -= OnChildRemoved;
            child.ChildRemoved += OnChildRemoved;
            child.AttributeChanged -= OnAttributeChanged;
            child.AttributeChanged += OnAttributeChanged;
        }

        private void Unsubscribe(SvgElement child)
        {
            if (!(child is SvgVisualElement))
                return;

            child.ChildAdded -= OnChildAdded;
            child.ChildRemoved -= OnChildRemoved;
            child.AttributeChanged -= OnAttributeChanged;
        }

        private void OnChildAdded(object sender, ChildAddedEventArgs e)
        {
            Subscribe(e.NewChild);
            SnapToGrid(e.NewChild);
        }

        private void OnChildRemoved(object sender, ChildRemovedEventArgs e)
        {
            Unsubscribe(e.RemovedChild);
        }

        private void OnAttributeChanged(object sender, AttributeEventArgs e)
        {
            // if snapping is currently in progress, just skip (otherwise we might cause stackoverflowexception!
            if (_isSnappingInProgress)
                return;

            if (!string.Equals(e.Attribute, "transform"))
                return;
            var element = (SvgElement)sender;
            // otherwise we need to reevaluate the translate of that particular element
            SnapToGrid(element);
        }

        private void SnapToGrid(SvgElement element)
        {
            var ve = element as SvgVisualElement;
            if (ve == null)
                return;

            try
            {
                _isSnappingInProgress = true;
                
                // snap to grid:
                // get absolute point
                var bounds = ve.Bounds;
                var m = ve.Transforms.GetMatrix();
                var b = m.TransformRectangle(bounds);

                // determine next intersection of gridlines
                // so we determine which point P1, P2 is the nearest one
                // afterwards, see if the intermediary points (Px, Pz) are even closer 
                // and in that case transition to those.

                // so when we have the ankle points (P1, P2), determine if an intermediate point (Px, Pz) is even nearer
                /*
                                                        Px
                                                                                                  Px = P1 + Vx
                                                     XXX+XX
                                                  XXXX  | XXXX                                    Px = P1 + (P1.x + b)
                                                XXX     |    XXXX                                           (P1.y - a)
                                              XX   B = ?|       XXX
                                           XXX          |         XXX
                                         XXX            |            XXX
                         c = ?         XXX              |               XXX
                                    XXX                 |                 XXX
                                  XXX                   |  a = 20            XXX
                                XXX                     |                      XXX
                              XXX                       |                         XXX
                            XXX                         |                           XX
                          XXX                    G = 90 |                            XXX
                        XXX    A = 27.3                 |                              XXX
                    P1 XX---------------------------------------------------------------+    P2
                       XXX           b = ?              |                             XX
                          XX                            |                           XXX
                           XXX                          |                         XXX
                              XXX                       |                       XXX
                                XXX                     |                     XXX
                                  XXX                   |                  XXXX
                                    XXX                 |                XXX
                                      XXXX              |              XXX
                                         XX             |            XXX
                                          XXX           |          XXX
                                            XXX         |       XXXX
                                              XXX       |     XXX
                                                XXX     |   XXX
                                                  XXX  ++ XXX
                                                     XXX XX

                                                       Pz

                 * 
                 * */
                float deltaX, deltaY;

                var diffX = b.X % StepSizeX;
                var diffY = b.Y % StepSizeY;

                // for the first moved element, store the translation and translate all other elements by the same translation
                // so if multiple elements are moved, their position relative to each other stays the same
                if (_areElementsMoved && _generalTranslation != null)
                {
                    deltaX = _generalTranslation.X;
                    deltaY = _generalTranslation.Y;
                }
                else
                {
                    deltaX = 0f;
                    if (diffX > StepSizeX/2)
                        deltaX = StepSizeX;

                    deltaY = 0f;
                    if (diffY > StepSizeY/2)
                        deltaY = StepSizeY;


                    // see if intermediary point is even nearer but also take Y coordinate into consideration!!
                    if (diffX > StepSizeX/2)
                    {
                        // transition to intermediary point
                        deltaX = StepSizeX/2;

                        if (diffY > StepSizeY/2)
                            deltaY = StepSizeY/2;
                        else
                            deltaY = StepSizeY/2;
                    }

                    if (_generalTranslation == null)
                        _generalTranslation = Engine.Factory.CreatePointF(deltaX, deltaY);

                }

                var absoluteDeltaX = 0 - diffX + deltaX;
                var absoluteDeltaY = 0 - diffY + deltaY;

                // and translate element to that next intersection
                AddTranslate(ve, absoluteDeltaX, absoluteDeltaY);

            }
            finally
            {
                _isSnappingInProgress = false;
            }
        }

        private static void AddTranslate(SvgVisualElement element, float deltaX, float deltaY)
        {
            SvgTranslate trans = null;
            int index = -1;

            //if (element.Transforms.OfType<SvgTranslate>().Count() > 1)
            {
                for (int i = element.Transforms.Count - 1; i >= 0; i--)
                {
                    var translate = element.Transforms[i] as SvgTranslate;
                    if (translate != null)
                    {
                        trans = translate;
                        index = i;
                        break;
                    }
                }
            }

            var transforms = element.Transforms;
            if (trans == null)
            {
                trans = new SvgTranslate(deltaX, deltaY);
                transforms.Add(trans);
            }
            else
            {
                var t = new SvgTranslate(trans.X + deltaX, trans.Y + deltaY);
                transforms[index] = t; // we MUST explicitly set the transform so the "OnTransformChanged" event is fired!
            }
        }

        #endregion

        public override void Dispose()
        {
            _pen?.Dispose();
            _pen2?.Dispose();
            _brush?.Dispose();
            _brush2?.Dispose();
        }

        private static double SinDegree(double value)
        {
            return RadianToDegree(Math.Sin(DegreeToRadian(value)));
        }
        private static double DegreeToRadian(double angle)
        {
            return Math.PI * angle / 180.0;
        }
        private static double RadianToDegree(double angle)
        {
            return angle * (180.0 / Math.PI);
        }
    }
}