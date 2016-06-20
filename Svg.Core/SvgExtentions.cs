﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.IO;
using System.Xml;
using System.Threading;
using System.Globalization;
using Svg.Interfaces;
using Svg.Interfaces.Xml;

namespace Svg
{
    /// <summary>
    /// Svg helpers
    /// </summary>
    public static class SvgExtentions
    {
        public static void SetRectangle(this SvgRectangle r, RectangleF bounds)
        {
            r.X = bounds.X;
            r.Y = bounds.Y;
            r.Width = bounds.Width;
            r.Height = bounds.Height;
        }
        
        public static RectangleF GetRectangle(this SvgRectangle r)
        {
            return Engine.Factory.CreateRectangleF(r.X, r.Y, r.Width, r.Height);
        }

        public static string GetXML(this SvgDocument doc)
        {
            var ret = "";

            using (var ms = new MemoryStream())
            {
                doc.Write(ms);
                ms.Position = 0;
                using (var sr = new StreamReader(ms))
                {
                    ret = sr.ReadToEnd();
                }
            }

            return ret;
        }

        public static string GetXML(this SvgElement elem)
        {
            var result = "";
            
            using(var c = Engine.Resolve<ICultureHelper>().UsingCulture(CultureInfo.InvariantCulture))
            using (StringWriter str = new StringWriter())
            {
                using (IXmlTextWriter xml = Engine.Factory.CreateXmlTextWriter(str))
                {
                    elem.Write(xml);
                    result = str.ToString();
                }
            }

            return result;
        }
        
        public static bool HasNonEmptyCustomAttribute(this SvgElement element, string name)
        {
        	return element.CustomAttributes.ContainsKey(name) && !string.IsNullOrEmpty(element.CustomAttributes[name]);
        }
        
        public static void ApplyRecursive(this SvgElement elem, Action<SvgElement> action)
        {
        	action(elem);
        	
        	if(!(elem is SvgDocument)) //don't apply action to subtree of documents
        	{
        		foreach (var element in elem.Children)
        		{
        			element.ApplyRecursive(action);
        		}
        	}
        }
        
        public static void ApplyRecursiveDepthFirst(this SvgElement elem, Action<SvgElement> action)
        {
        	if(!(elem is SvgDocument)) //don't apply action to subtree of documents
        	{
        		foreach (var element in elem.Children)
        		{
        			element.ApplyRecursiveDepthFirst(action);
        		}
        	}
        	
        	action(elem);
        }
    }
}