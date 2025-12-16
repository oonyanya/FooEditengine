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
using SharpDX;
using D2D = SharpDX.Direct2D1;
using DW = SharpDX.DirectWrite;

namespace FooEditEngine
{
    sealed class MyTextLayout : ITextLayout
    {
        double _height = double.NaN;
        double _width = double.NaN;
        int lineBreakIndex = 0;
        StringAlignment _strAlign;

        internal MyTextLayout(DW.Factory dwFactory,string str,DW.TextFormat format,double width,double height,float dip,bool showLineBreak)
        {
            if (dwFactory.IsDisposed)
            {
                throw new InvalidOperationException();
            }
            var (linefeedlen, linefeedtype) = Util.GetNewLineLengthInTailWithType(str);
            str = Util.TrimLineFeed(str);   //取り除かないとキャレットの動きがおかしくなる
            if (showLineBreak)
            {
                this.lineBreakIndex = str.Length;
                switch(linefeedtype)
                {
                    case Document.CRLF_STR:
                        str = str + "↵";
                        break;
                    case Document.CR_STR:
                        str = str + "↓";
                        break;
                    case Document.LF_STR:
                        str = str + "→";
                        break;
                }
            }
            this.layout = new DW.TextLayout(dwFactory,
                str,
                format,
                (float)width,
                (float)height);
            this.Disposed = false;
            this.ShowLineBreak = showLineBreak;
        }

        DW.TextLayout layout;

        public bool ShowLineBreak
        {
            get;
            private set;
        }

        public bool Disposed
        {
            get;
            private set;
        }

        public bool Invaild
        {
            get;
            set;
        }

        public float MaxWidth
        {
            get
            {
                return this.layout.MaxWidth;
            }
        }

        public float MaxHeight
        {
            get
            {
                return this.layout.MaxHeight;
            }
        }

        public bool RightToLeft
        {
            get
            {
                return this.layout.ReadingDirection == DW.ReadingDirection.RightToLeft;
            }
            set
            {
                if (value)
                    this.layout.ReadingDirection = DW.ReadingDirection.RightToLeft;
                else
                    this.layout.ReadingDirection = DW.ReadingDirection.LeftToRight;
            }
        }
        
        public void SetLineSpacing(double height)
        {
            this.layout.SetLineSpacing(DW.LineSpacingMethod.Uniform, (float)height, (float)Math.Round(height * 0.8));
        }

        public int GetIndexFromColPostion(double x)
        {
            return this.GetIndexFromPostion(x, 0);
        }

        public double GetWidthFromIndex(int index)
        {
            float x, y;
            DW.HitTestMetrics metrics;
            metrics = this.layout.HitTestTextPosition(index, false, out x, out y);
            float x2;
            layout.HitTestTextPosition(index, true, out x2, out y);

            return x2 - x;
        }

        public double GetColPostionFromIndex(int index)
        {
            Point p = this.GetPostionFromIndex(index);
            return p.X;
        }

        public int GetIndexFromPostion(double x, double y)
        {
            SharpDX.Mathematics.Interop.RawBool isTrailing, isInsed;
            DW.HitTestMetrics metrics;
            metrics = this.layout.HitTestPoint((float)x, (float)y, out isTrailing, out isInsed);
            int index;
            if (isTrailing)
                index = metrics.TextPosition + metrics.Length;
            else
                index = metrics.TextPosition;
            if (this.ShowLineBreak && index == this.lineBreakIndex + 1) //改行マークの後ろにヒットしたら
                index--;
            return index;
        }

        public Point GetPostionFromIndex(int index)
        {
            float x, y;
            DW.HitTestMetrics metrics;
            metrics = this.layout.HitTestTextPosition(index, false, out x, out y);
            return new Point(x,y);
        }

        public int AlignIndexToNearestCluster(int index, AlignDirection flow)
        {
            float x, y;
            DW.HitTestMetrics metrics;
            metrics = this.layout.HitTestTextPosition(index, false, out x, out y);

            if (flow == AlignDirection.Forward)
                return Util.RoundUp(metrics.TextPosition + metrics.Length);
            else if (flow == AlignDirection.Back)
                return Util.RoundUp(metrics.TextPosition);
            throw new ArgumentOutOfRangeException();
        }

        public double Width
        {
            get
            {
                if(double.IsNaN(this._width))
                    this._width = Util.RoundUp(this.layout.Metrics.WidthIncludingTrailingWhitespace);
                return this._width;
            }
        }

        public double Height
        {
            get
            {
                if (!double.IsNaN(this._height))
                    return this._height;
                this._height = Util.RoundUp(this.layout.Metrics.Height);
                return this._height;
            }
        }

        public StringAlignment StringAlignment
        {
            get
            {
                return this._strAlign;
            }
            set
            {
                this._strAlign = value;
                switch (value)
                {
                    case StringAlignment.Left:
                        layout.TextAlignment = DW.TextAlignment.Leading;
                        break;
                    case StringAlignment.Center:
                        layout.TextAlignment = DW.TextAlignment.Center;
                        break;
                    case StringAlignment.Right:
                        layout.TextAlignment = DW.TextAlignment.Trailing;
                        break;
                }
            }
        }

        public void Draw(D2D.RenderTarget d2drender, DW.TextRenderer render, float x, float y)
        {
            this.layout.Draw(d2drender, render, x, y);
        }

        public void Draw(D2D.RenderTarget render, float x, float y, D2D.Brush brush)
        {
            render.DrawTextLayout(new Vector2((float)x, (float)y), this.layout, brush);
        }

        public void Dispose()
        {
            this.layout.Dispose();
            this.Disposed = true;
            GC.SuppressFinalize(this);
        }

        public IList<Marker> Markers;
        public IList<Selection> Selects;

        public void SetDrawingEffect(ComObject resource, DW.TextRange range)
        {
            this.layout.SetDrawingEffect(resource.NativePointer, range);
        }

        public void SetLineBreakBrush(D2D.Brush brush)
        {
            if(this.ShowLineBreak)
                this.layout.SetDrawingEffect(brush.NativePointer, new DW.TextRange(lineBreakIndex, 1));
        }

        public void SetUnderline(bool underline, DW.TextRange range)
        {
            this.layout.SetUnderline(underline, range);
        }

        public DW.HitTestMetrics[] HitTestTextRange(int start, int length, float x, float y)
        {
            return this.layout.HitTestTextRange(start, length, x, y);
        }

        public void SetInlineObject(DW.InlineObject o, DW.TextRange range)
        {
            this.layout.SetInlineObject(o, range);
        }

        public double GetColHeightFromIndex(int index)
        {
            float x, y;
            DW.HitTestMetrics metrics;
            metrics = this.layout.HitTestTextPosition(index, false, out x, out y);
            return metrics.Height;
        }
    }
}
#endif