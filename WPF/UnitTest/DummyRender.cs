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
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FooEditEngine;

namespace UnitTest
{
    class DummyRender : IEditorRender
    {
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
            get { return new Size(10,10); }
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

        public event ChangedRenderResourceEventHandler ChangedRenderResource;

        public event EventHandler ChangedRightToLeft;

        public void DrawCachedBitmap(Rectangle rect)
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

        public void DrawGripper(Point p, double radius)
        {
            throw new NotImplementedException();
        }

        public void BeginClipRect(Rectangle rect)
        {
        }

        public void EndClipRect()
        {
        }

        public ITextLayout CreateLaytout(string str, SyntaxInfo[] syntaxCollection, IEnumerable<Marker> MarkerRanges, IEnumerable<Selection> Selections, double WrapWidth)
        {
            return new DummyTextLayout();
        }

        public void DrawCachedBitmap(Rectangle dstRect, Rectangle srcRect)
        {
        }
    }
    class DummyTextLayout : ITextLayout
    {
        public double Width
        {
            get { return 100; }
        }

        public double Height
        {
            get { return 20; }
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
            return 0;
        }

        public double GetWidthFromIndex(int index)
        {
            return 0;
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

        public int GetIndexFromPostion(double x, double y)
        {
            return 0;
        }

        public Point GetPostionFromIndex(int index)
        {
            return new Point(0,0);
        }
    }
}
