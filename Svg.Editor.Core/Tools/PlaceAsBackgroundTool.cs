﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Svg.Core.Interfaces;

namespace Svg.Core.Tools
{
    public class PlaceAsBackgroundTool : UndoableToolBase
    {
        public PlaceAsBackgroundTool(IDictionary<string,object> properties, IUndoRedoService undoRedoService) : base("Background Image", properties, undoRedoService)
        {
            IconName = "ic_insert_photo_white_48dp.png";
        }

        public override async Task Initialize(SvgDrawingCanvas ws)
        {
            await base.Initialize(ws);

            Commands = new List<IToolCommand>
            {
                new ToolCommand(this, "Choose background image", async o =>
                {
                    ImagePath = await Engine.Resolve<IPickImageService>().PickImagePathAsync(Canvas.ScreenWidth);
                    if (ImagePath == null) return;
                    PlaceImage(ImagePath);
                }, o => ChooseBackgroundEnabled, iconName: "ic_insert_photo_white_48dp.png"),
                new ToolCommand(this, "Remove background image", o =>
                {
                    var children = Canvas.Document.Children;
                    var background = children.FirstOrDefault(x => x.CustomAttributes.ContainsKey("iclbackground"));
                    if (background != null)
                    {
                        children.Remove(background);
                        background.Dispose();

                        Canvas.ConstraintLeft = float.MinValue;
                        Canvas.ConstraintTop = float.MinValue;
                        Canvas.ConstraintRight = float.MaxValue;
                        Canvas.ConstraintBottom = float.MaxValue;

                        ImagePath = null;

                        Canvas.FireInvalidateCanvas();
                        Canvas.FireToolCommandsChanged();
                    }
                }, o => ChooseBackgroundEnabled && Canvas.Document.Children.Any(x => x.CustomAttributes.ContainsKey("iclbackground")), iconName: "ic_delete_white_48dp.png")
            };

            if (ImagePath != null)
            {
                PlaceImage(ImagePath);
            }
        }

        private void PlaceImage(string path)
        {
            try
            {
                var children = Canvas.Document.Children;
                // insert the background before the first visible element
                var index = children.IndexOf(children.FirstOrDefault(x => x is SvgVisualElement));
                // if there are no visual elements, we want to add it to the end of the list
                if (index == -1) index = children.Count;
                if (!path.StartsWith("/")) path = path.Insert(0, "/");
                var image = new SvgImage
                {
                    Href = new Uri($"file://{path}", UriKind.Absolute)
                };
                image.CustomAttributes.Add("iclbackground", "");
                image.CustomAttributes.Add("iclnosnapping", "");

                // remove already placed background
                var formerBackground = children.FirstOrDefault(x => x.CustomAttributes.ContainsKey("iclbackground"));
                if (formerBackground != null)
                {
                    children.Remove(formerBackground);
                    formerBackground.Dispose();
                }

                children.Insert(index, image);

                var size = image.GetImageSize();
                Canvas.ConstraintLeft = 0;
                Canvas.ConstraintTop = 0;
                Canvas.ConstraintRight = size.Width;
                Canvas.ConstraintBottom = size.Height;

                Canvas.FireInvalidateCanvas();
                Canvas.FireToolCommandsChanged();
            }
            catch (IOException)
            {
            }
        }

        public static readonly string ChooseBackgroundEnabledKey = @"choosebackgroundenabled";
        public static readonly string ImagePathKey = @"imagepath";

        public bool ChooseBackgroundEnabled
        {
            get
            {
                object chooseBackgroundEnabled;
                return Properties.TryGetValue(ChooseBackgroundEnabledKey, out chooseBackgroundEnabled) && Convert.ToBoolean(chooseBackgroundEnabled);
            }
            set { Properties[ChooseBackgroundEnabledKey] = value; }
        }

        public string ImagePath
        {
            get
            {
                object imagePath;
                Properties.TryGetValue(ImagePathKey, out imagePath);
                return imagePath as string;
            }
            set { Properties[ImagePathKey] = value; }
        }
    }
}