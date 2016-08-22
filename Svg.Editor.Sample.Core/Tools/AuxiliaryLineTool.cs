﻿using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Svg.Core;
using Svg.Core.Tools;

namespace Svg.Droid.SampleEditor.Core.Tools
{
    public class AuxiliaryLineTool : ToolBase
    {
        private bool _showAuxiliaryLines = true;
        private SvgDrawingCanvas _canvas;

        public bool ShowAuxiliaryLines 
        {
            get { return _showAuxiliaryLines; }
            set
            {
                _showAuxiliaryLines = value;
                if (_canvas != null)
                {
                    ShowHideAuxiliaryLines(_canvas.Document);
                }
            }
        }

        public AuxiliaryLineTool() : base("Auxiliaryline")
        {
        }

        public AuxiliaryLineTool(string jsonProperties) : base("Auxiliaryline", jsonProperties)
        {
        }

        public override Task Initialize(SvgDrawingCanvas ws)
        {
            _canvas = ws;

            Commands = new List<IToolCommand>
            {
                new ToolCommand(this, "Toogle auxiliary lines", (obj) =>
                {
                    ShowAuxiliaryLines = !ShowAuxiliaryLines;
                    _canvas.FireInvalidateCanvas();
                }, iconName:"ic_code_white_48dp.png", sortFunc: (obj) => 1000)
            };

            return Task.FromResult(true);
        }

        public override void OnDocumentChanged(SvgDocument oldDocument, SvgDocument newDocument)
        {
            UnWatchDocument(oldDocument);
            WatchDocument(newDocument);
            ShowHideAuxiliaryLines(newDocument);
        }

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
        }

        private void Unsubscribe(SvgElement child)
        {
            if (!(child is SvgVisualElement))
                return;

            child.ChildAdded -= OnChildAdded;
            child.ChildRemoved -= OnChildRemoved;
        }

        private void OnChildAdded(object sender, ChildAddedEventArgs e)
        {
            Subscribe(e.NewChild);
            ShowHideAuxiliaryLines(e.NewChild);
        }

        private void OnChildRemoved(object sender, ChildRemovedEventArgs e)
        {
            Unsubscribe(e.RemovedChild);
        }
        
        private void ShowHideAuxiliaryLines(SvgElement element)
        {
            var d = element as SvgDocument;
            if (d != null)
            {
                foreach (var vc in d.Children.OfType<SvgVisualElement>())
                {
                    ShowHideAuxiliaryLines(vc);
                }
            }
            else
            {
                var ve = element as SvgVisualElement;
                if (ve == null)
                    return;

                var isAuxLine = ve.CustomAttributes.ContainsKey("iclhelpline");
                if (isAuxLine)
                {
                    ve.Visible = ShowAuxiliaryLines;
                }
                foreach (var vc in ve.Children.OfType<SvgVisualElement>())
                {
                    ShowHideAuxiliaryLines(vc);
                }
            }
        }
    }
}