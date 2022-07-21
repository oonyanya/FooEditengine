﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Brushes;
using Microsoft.Graphics.Canvas.Text;
using FooEditEngine;

namespace FooEditEngine.WinUI
{
    class Win2DTextLayout : ITextLayout
    {
        Win2DResourceFactory _factory;
        CanvasTextLayout _layout;
        int? _lineBreakIndex;
        public Win2DTextLayout(Win2DResourceFactory render, string str, CanvasTextFormat format, double width, double height, float dip, bool showLineBreak, double lineHeight)
        {
            str = str.Trim(Document.NewLine);   //取り除かないとキャレットの動きがおかしくなる
            if (showLineBreak)
            {
                str += '↵';
                _lineBreakIndex = str.Length;
            }

            this._layout = new CanvasTextLayout(render.Device, str, format, (float)width, (float)height);
            this._layout.Options = CanvasDrawTextOptions.EnableColorFont;
            this._layout.LineSpacingMode = CanvasLineSpacingMode.Uniform;
            this._layout.LineSpacing = (float)lineHeight;
            this._layout.LineSpacingBaseline = (float)Math.Round(lineHeight * 0.8);
            this._factory = render;
        }

        internal CanvasTextLayout RawLayout => this._layout;

        double? _width;
        public double Width
        {
            get
            {
                if (_width == null)
                    this._width = this._layout.LayoutBounds.Width;
                return _width.Value;
            }
        }

        double? _height;
        public double Height
        {
            get
            {
                if (_height == null)
                    this._height = this._layout.LayoutBounds.Height;
                return _height.Value;
            }
        }

        public bool Disposed
        {
            get;
            set;
        }

        public bool Invaild
        {
            get;
            set;
        }

        public bool RightToLeft
        {
            get
            {
                return this._layout.Direction == CanvasTextDirection.RightToLeftThenBottomToTop;
            }
            set
            {
                if (value)
                    this._layout.Direction = CanvasTextDirection.RightToLeftThenBottomToTop;
                else
                    this._layout.Direction = CanvasTextDirection.LeftToRightThenTopToBottom;
            }
        }

        //CustomeTextRenderを使うと重くなる
        public IList<Marker> Markers;
        public IList<Selection> Selects;

        public int AlignIndexToNearestCluster(int index, AlignDirection flow)
        {
            CanvasTextLayoutRegion r;
            this._layout.GetCaretPosition(index, false, out r);
            if (flow == AlignDirection.Forward)
                return r.CharacterIndex + r.CharacterCount;
            return r.CharacterIndex;
        }

        public void Dispose()
        {
            this.Disposed = true;
            this.Invaild = true;
            this._layout.Dispose();
        }

        public double GetColPostionFromIndex(int index)
        {
            return this.GetPostionFromIndex(index).X;
        }

        public int GetIndexFromColPostion(double colpos)
        {
            return this.GetIndexFromPostion(colpos, 0);
        }

        public int GetIndexFromPostion(double x, double y)
        {
            CanvasTextLayoutRegion r;
            this._layout.HitTest((float)x, (float)y,out r);
            return r.CharacterIndex;
        }

        public Point GetPostionFromIndex(int index)
        {
            var v = this._layout.GetCaretPosition(index, false);
            return new Point(v.X, v.Y);
        }

        public double GetWidthFromIndex(int index)
        {
            CanvasTextLayoutRegion r;
            this._layout.GetCaretPosition(index, false, out r);
            return r.LayoutBounds.Width ;
        }
        public void SetLineBreakBrush(Windows.UI.Color ctrlColor)
        {
            if (this._lineBreakIndex != null)
                this._layout.SetBrush(this._lineBreakIndex.Value, 1, this._factory.CreateSolidColorBrush(ctrlColor));
        }

    }
}
