/*
 * Copyright (C) 2013 FooProject
 * * This program is free software; you can redistribute it and/or modify it under the terms of the GNU General Public License as published by
 * the Free Software Foundation; either version 3 of the License, or (at your option) any later version.

 * This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of 
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.

You should have received a copy of the GNU General Public License along with this program. If not, see <http://www.gnu.org/licenses/>.
 */
using System;
using System.Collections.Generic;
using FooEditEngine;
#if WINDOWS_UWP
using Windows.UI.Xaml.Shapes;
#endif

namespace FooEditEngine
{
    class DummyDisposer : IDisposable
    {
        Action disposeAction;
        public DummyDisposer() { }
        public DummyDisposer(Action dipose)
        {
            disposeAction = dipose;
        }
        public void Dispose() 
        {
            if (disposeAction != null)
            {
                disposeAction();
            }
            disposeAction = null;
        }
    }

    class DummyRender : IEditorRender,IDisposable
    {
        public DummyRender()
        {
        }
        public bool RightToLeft
        {
            get;
            set;
        }

        public Rectangle TextArea
        {
            get;
            set;
        }

        public double LineNemberWidth
        {
            get { return 0; }
        }

        public double FoldingWidth
        {
            get { return 0; }
        }

        public Size emSize
        {
            get { return new Size(); }
        }

        public int TabWidthChar
        {
            get;
            set;
        }

        public bool ShowFullSpace
        {
            get;
            set;
        }

        public bool ShowHalfSpace
        {
            get;
            set;
        }

        public bool ShowTab
        {
            get;
            set;
        }

        public bool ShowLineBreak
        {
            get;
            set;
        }

        #pragma warning disable 0067
        //ダミーレンダーなので使わない
        public event ChangedRenderResourceEventHandler ChangedRenderResource;
        public event EventHandler ChangedRightToLeft;
        #pragma warning restore 0067

        public void DrawCachedBitmap(Rectangle rect,Rectangle rect2)
        {
            throw new NotImplementedException();
        }

        public void DrawLine(Point from, Point to)
        {
            throw new NotImplementedException();
        }

        public void CacheContent()
        {
            throw new NotImplementedException();
        }

        public bool IsVaildCache()
        {
            throw new NotImplementedException();
        }

        public void DrawString(string str, double x, double y, StringAlignment align, Size layoutRect,StringColorType type)
        {
            throw new NotImplementedException();
        }

        public void FillRectangle(Rectangle rect, FillRectType type)
        {
            throw new NotImplementedException();
        }

        public void DrawFoldingMark(bool expand, double x, double y)
        {
            throw new NotImplementedException();
        }

        public void FillBackground(Rectangle rect)
        {
            throw new NotImplementedException();
        }

        public void DrawOneLine(Document doc, LineToIndexTable lti, int row, double x, double y)
        {
            throw new NotImplementedException();
        }

        public List<LineToIndexTableData> BreakLine(Document doc,LineToIndexTable layoutLineCollection, int startIndex, int endIndex, double wrapwidth)
        {
            throw new NotImplementedException();
        }

        public ITextLayout CreateLaytout(string str, SyntaxInfo[] syntaxCollection, IEnumerable<Marker> MarkerRanges, IEnumerable<Selection> SelectRanges, double wrapwidth)
        {
            return new DummyTextLayout(str.Length);
        }

        public void DrawGripper(Point p, double radius)
        {
            throw new NotImplementedException();
        }

        public IDisposable BeginClipRect(Rectangle rect)
        {
            return new DummyDisposer();
        }

        public void EndClipRect()
        {
        }

        public void Dispose()
        {
        }

#if WINDOWS_UWP
        public double FontSize
        {
            get;
            set;
        }
        double _LineEmHeight;
        public double LineEmHeight { get => 1.0f; set => _LineEmHeight = value; }

        public void SetImeConvationInfo(Windows.UI.Text.Core.CoreTextFormatUpdatingEventArgs arg)
        {
        }

        public void DrawContent(EditView view, bool isEnabled, Rectangle updateRect)
        {
        }

        public bool Resize(Windows.UI.Xaml.Shapes.Rectangle rectangle, double width, double height)
        {
            return false;
        }
#endif
    }
    class DummyTextLayout : ITextLayout
    {
        int _charnum;
        public DummyTextLayout(int charnum)
        {
            this._charnum = charnum;
        }
        public double Width
        {
            get { return _charnum; }
        }

        public double Height
        {
            get { return 10; }
        }

        public bool Disposed
        {
            get;
            private set;
        }

        public bool Invaild
        {
            get { return false; }
        }

        public int GetIndexFromColPostion(double x)
        {
            return (int)x;
        }

        public double GetWidthFromIndex(int index)
        {
            return 1;
        }

        public double GetColPostionFromIndex(int index)
        {
            return index;
        }

        public int AlignIndexToNearestCluster(int index, AlignDirection flow)
        {
            if(flow == AlignDirection.Back)
                return Math.Max(index - 1,0);
            if (flow == AlignDirection.Forward)
                return index + 1;
            throw new ArgumentOutOfRangeException("flowの値がおかしい");
        }

        public void Dispose()
        {
            this.Disposed = true;
        }

        public Point GetPostionFromIndex(int index)
        {
            return new Point(index,0);
        }

        public int GetIndexFromPostion(double x, double y)
        {
            return (int)x;
        }
    }

    class DummyHilighter : IHilighter
    {
        char spiliter;
        public DummyHilighter(char spiliter)
        {
            this.spiliter = spiliter;
        }
        public int DoHilight(string text, int length, TokenSpilitHandeler action)
        {
            if(text == string.Empty)
                return 0;
            var tokens = text.Split(spiliter);
            int index = 0;
            foreach(var token in tokens)
            {
                //面倒なので\n前提
                if (token[0] == Document.NewLine)
                    break;
                action(new TokenSpilitEventArgs(index, token.Length, TokenType.Keyword1));
                index += token.Length;
            }
            return 0;
        }

        public void Reset()
        {
        }
    }
}
