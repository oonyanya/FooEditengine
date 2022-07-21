using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Graphics.Canvas.Brushes;
using FooEditEngine;
using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Geometry;

namespace FooEditEngine.UWP
{
    sealed class Win2DSquilleLineMarker : SquilleLineMarker
    {
        CanvasSolidColorBrush brush;
        CanvasStrokeStyle stroke;
        CanvasDrawingSession render;
        float thickness;
        public Win2DSquilleLineMarker(CanvasDrawingSession render, CanvasSolidColorBrush brush, CanvasStrokeStyle stroke, float thickness)
        {
            this.brush = brush;
            this.stroke = stroke;
            this.thickness = thickness;
            this.render = render;
        }

        public override void DrawLine(double x, double y, double tox, double toy)
        {
            render.DrawLine((float)x, (float)y, (float)tox, (float)toy, this.brush, thickness, this.stroke);
        }
    }

    sealed class Win2DLineMarker : IMarkerEffecter
    {
        CanvasSolidColorBrush brush;
        CanvasStrokeStyle stroke;
        CanvasDrawingSession render;
        float thickness;
        public Win2DLineMarker(CanvasDrawingSession render, CanvasSolidColorBrush brush, CanvasStrokeStyle stroke, float thickness)
        {
            this.brush = brush;
            this.stroke = stroke;
            this.thickness = thickness;
            this.render = render;
        }

        public void Draw(double x, double y, double width, double height)
        {
            render.DrawLine((float)x, (float)y, (float)(x + width - 1), (float)y, this.brush, thickness, this.stroke);
        }
    }

    sealed class Win2DHilightMarker : IMarkerEffecter
    {
        CanvasSolidColorBrush brush;
        CanvasStrokeStyle stroke;
        CanvasDrawingSession render;
        public Win2DHilightMarker(CanvasDrawingSession render, CanvasSolidColorBrush brush)
        {
            this.brush = brush;
            this.render = render;
        }

        public void Draw(double x, double y, double width, double height)
        {
            render.FillRectangle((float)x, (float)y, (float)width, (float)height, brush);
        }
    }
}
