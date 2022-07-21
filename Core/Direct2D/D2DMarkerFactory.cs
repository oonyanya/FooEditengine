/*
 * Copyright (C) 2013 FooProject
 * * This program is free software; you can redistribute it and/or modify it under the terms of the GNU General Public License as published by
 * the Free Software Foundation; either version 3 of the License, or (at your option) any later version.

 * This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of 
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.

You should have received a copy of the GNU General Public License along with this program. If not, see <http://www.gnu.org/licenses/>.
 */
#if WPF || WINFORM
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using SharpDX;
using D2D = SharpDX.Direct2D1;
using DW = SharpDX.DirectWrite;

namespace FooEditEngine
{
    sealed class D2DSquilleLineMarker : SquilleLineMarker
    {
        D2D.SolidColorBrush brush;
        D2D.StrokeStyle stroke;
        D2D.RenderTarget render;
        float thickness;
        public D2DSquilleLineMarker(D2D.RenderTarget render, D2D.SolidColorBrush brush, D2D.StrokeStyle stroke, float thickness)
        {
            this.brush = brush;
            this.stroke = stroke;
            this.thickness = thickness;
            this.render = render;
        }

        public override void DrawLine(double x, double y, double tox, double toy)
        {
            this.render.DrawLine(new Vector2((float)x, (float)y), new Vector2((float)tox, (float)toy), this.brush, this.thickness, this.stroke);
        }
    }

    sealed class LineMarker : IMarkerEffecter
    {
        D2D.SolidColorBrush brush;
        D2D.StrokeStyle stroke;
        D2D.RenderTarget render;
        float thickness;
        public LineMarker(D2D.RenderTarget render, D2D.SolidColorBrush brush,D2D.StrokeStyle stroke, float thickness)
        {
            this.brush = brush;
            this.stroke = stroke;
            this.thickness = thickness;
            this.render = render;
        }

        public void Draw(double x, double y, double width, double height)
        {
            render.DrawLine(new Vector2((float)x, (float)(y)),
                new Vector2((float)(x + width - 1), (float)(y)),
                this.brush,
                this.thickness,
                this.stroke);
        }
    }

    sealed class HilightMarker : IMarkerEffecter
    {
        D2D.SolidColorBrush brush;
        D2D.RenderTarget render;
        public HilightMarker(D2D.RenderTarget render,D2D.SolidColorBrush brush)
        {
            this.brush = brush;
            this.render = render;
        }

        public void Draw(double x, double y, double width, double height)
        {
            render.FillRectangle(new RectangleF((float)x, (float)y, (float)width, (float)height), brush);
        }
    }
}
#endif