﻿using System;
using System.Linq;
using System.Threading.Tasks;
using Svg.Core.Events;
using Svg.Core.Interfaces;
using Svg.Core.UndoRedo;
using Svg.Transforms;

namespace Svg.Core.Tools
{
    public interface ITextInputService
    {
        Task<string> GetUserInput(string title, string textValue);
    }

    public class TextTool : UndoableToolBase
    {
        // if user moves cursor, she does not want to add/edit text
        private bool _moveEventWasRegistered;

        public TextTool(IUndoRedoService undoRedoService) : base("Text", undoRedoService)
        {
            IconName = "ic_text_fields_white_48dp.png";
            ToolUsage = ToolUsage.Explicit;
            ToolType = ToolType.Create;
        }

        private ITextInputService TextInputService => Engine.Resolve<ITextInputService>();

        public override Task Initialize(SvgDrawingCanvas ws)
        {
            IsActive = false;

            return base.Initialize(ws);
        }

        public override async Task OnUserInput(UserInputEvent @event, SvgDrawingCanvas ws)
        {
            if (!IsActive)
                return;

            // if user moves cursor, she does not want to add/edit text
            var me = @event as MoveEvent;
            if (me != null)
            {
                // if user moves with thumb we do not want to add text on pointer-up
                var isMove = Math.Sqrt(Math.Pow(me.AbsoluteDelta.X, 2) + Math.Pow(me.AbsoluteDelta.Y, 2)) > 20d;
                if (isMove)
                    _moveEventWasRegistered = true;
            }

            var pe = @event as PointerEvent;
            if (pe != null && pe.EventType == EventType.PointerDown)
            {
                _moveEventWasRegistered = false;
            }
            else if (pe != null && pe.EventType == EventType.PointerUp)
            {
                if (_moveEventWasRegistered)
                {
                    return;
                }

                var dX = pe.Pointer1Position.X - pe.Pointer1Down.X;
                var dY = pe.Pointer1Position.Y - pe.Pointer1Down.Y;

                // if Point-Down and Point-Up are merely the same
                if (dX < 20 && dY < 20)
                {
                    // if there is text below the pointer, edit it
                    var e = ws.GetElementsUnderPointer<SvgTextBase>(pe.Pointer1Position, 20).FirstOrDefault();

                    if (e != null)
                    {
                        // primitive handling of text spans
                        var span = e.Children.OfType<SvgTextSpan>().FirstOrDefault();
                        if (span != null)
                            e = span;

                        var txt = await TextInputService.GetUserInput("Edit text", e.Text?.Trim());

                        // make sure there is at least empty text in it so we actually still have a bounding box!!
                        if (string.IsNullOrEmpty(txt?.Trim()))
                            txt = "  ";

                        // if text was removed, and parent was document, remove element
                        // if parent was not the document, then this would be a text within another group and should not be removed
                        if (string.IsNullOrWhiteSpace(txt) && e.Parent is SvgDocument)
                        {
                            var parent = e.Parent;
                            UndoRedoService.ExecuteCommand(new UndoableActionCommand("Remove text", o =>
                            {
                                parent.Children.Remove(e);
                                Canvas.FireInvalidateCanvas();
                            }, o =>
                            {
                                parent.Children.Add(e);
                                Canvas.FireInvalidateCanvas();
                            }));
                        }
                        else if (!string.Equals(e.Text, txt))
                        {
                            var formerText = e.Text;
                            UndoRedoService.ExecuteCommand(new UndoableActionCommand("Edit text", o =>
                            {
                                e.Text = txt;
                                Canvas.FireInvalidateCanvas();
                            }, o =>
                            {
                                e.Text = formerText;
                                Canvas.FireInvalidateCanvas();
                            }));
                        }
                    }
                    // else add new text   
                    else
                    {
                        var txt = await TextInputService.GetUserInput("Add text", null);
                        // only add if user really entered text.
                        if (!string.IsNullOrWhiteSpace(txt))
                        {
                            var t = new SvgText(txt)
                            {
                                FontSize = new SvgUnit(SvgUnitType.Pixel, 20),
                                Stroke = new SvgColourServer(Engine.Factory.CreateColorFromArgb(255, 0, 0, 0)),
                                Fill = new SvgColourServer(Engine.Factory.CreateColorFromArgb(255, 0, 0, 0))
                            };

                            var relativePointer = ws.ScreenToCanvas(pe.Pointer1Position);
                            var childBounds = t.Bounds;
                            var halfRelChildWidth = childBounds.Width / 2;
                            var halfRelChildHeight = childBounds.Height / 2;

                            var x = relativePointer.X - halfRelChildWidth;
                            var y = relativePointer.Y - halfRelChildHeight;
                            //t.X = new SvgUnitCollection {new SvgUnit(SvgUnitType.Pixel, x)};
                            //t.Y = new SvgUnitCollection { new SvgUnit(SvgUnitType.Pixel, y) };
                            t.Transforms.Add(new SvgTranslate(x, y));

                            UndoRedoService.ExecuteCommand(new UndoableActionCommand("Add text", o =>
                            {
                                ws.Document.Children.Add(t);
                                ws.FireInvalidateCanvas();
                            }, o =>
                            {
                                ws.Document.Children.Remove(t);
                                ws.FireInvalidateCanvas();
                            }));
                        }
                    }
                }
            }
        }

    }
}
