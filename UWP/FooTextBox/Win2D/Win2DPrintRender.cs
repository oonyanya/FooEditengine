using System;
using System.Collections.Generic;
using System.Linq;
using Windows.UI.Text;
using Windows.UI.Xaml.Hosting;
using Windows.UI.Xaml.Media;
using Windows.UI.Composition;
using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.UI.Xaml;
using Microsoft.Graphics.Canvas.UI.Composition;
using Microsoft.Graphics.Canvas.Text;
using Microsoft.Graphics.Canvas.Brushes;
using Microsoft.Graphics.Canvas.Geometry;
using Windows.Graphics.DirectX;
using FooEditEngine;

namespace FooEditEngine.UWP
{
    class Win2DPrintRender : Win2DRenderBase,IPrintableTextRender
    {
        public Win2DPrintRender(CanvasDevice device)
        {
            this._factory = new Win2DResourceFactory(device);
        }

        public float HeaderHeight => (float)this.emSize.Height;

        public float FooterHeight => (float)this.emSize.Height;

        public void DrawContent(CanvasDrawingSession ds,PrintableView view)
        {
            this.offScreenSession = ds; //セットしないとレタリングに失敗する
            view.Draw(view.PageBound);
        }
    }
}
