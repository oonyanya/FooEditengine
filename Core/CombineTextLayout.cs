using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using FooProject.Collection;

namespace FooEditEngine
{
    class SubLineToIndexTableData : LineToIndexTableDataBase
    {
        public SubLineToIndexTableData(long startIndex,long length,double height,ITextLayout layout)
        {
            this.Layout = layout;
            this.start = startIndex;
            this.length = length;
            this.Height = height;
        }

        public override FooProject.Collection.IRange DeepCopy()
        {
            return new SubLineToIndexTableData(this.start,this.length,this.Height,this.Layout);
        }
    }

    class CombineTextLayout : ITextLayout
    {
        BigIndexAndHeightList<SubLineToIndexTableData> TextLayouts;

        public CombineTextLayout()
        {
            this.TextLayouts = new BigIndexAndHeightList<SubLineToIndexTableData>();
        }

        public void Add(ITextLayout layout, long startIndex, long length)
        {
            var newItem = new SubLineToIndexTableData(startIndex,length,layout.Height,layout);
            this.TextLayouts.Add(newItem);

            if (layout.Width > this.Width)
            {
                this.Width = layout.Width;
            }
            this.Height += layout.Height;
        }

        public ITextLayout this[int i]
        {
            get { return TextLayouts[i].Layout; }
        }

        public int TextLayoutCount
        {
            get { return TextLayouts.Count; }
        }

        public double Width { get; private set; }

        public double Height { get; private set; }

        public bool Disposed
        {
            get; set;
        }

        public bool Invaild
        {
            get; set;
        }

        private (int, int, int, double) GetLayoutNumberFromIndex(int index, int splitLength)
        {
            long relativeIndex = index;
            int layoutNumber = 0;
            double pos_y = 0.0;
            layoutNumber = (int)this.TextLayouts.GetIndexFromAbsoluteIndexIntoRange(index, out relativeIndex, out pos_y);
            if (layoutNumber == -1)
            {
                if (this.TextLayouts.Count > 0)
                    layoutNumber = this.TextLayouts.Count - 1;
                else
                    layoutNumber = 0;
            }

            relativeIndex = index - this.TextLayouts[layoutNumber].start;

            return ((int)relativeIndex, layoutNumber, layoutNumber, pos_y);
        }

        public int AlignIndexToNearestCluster(int index, AlignDirection flow)
        {
            int splitLength = Document.MaximumLineLength;
            int relativeIndex,layoutNumber,arrayIndex;
            double pos_y;
            (relativeIndex,arrayIndex,layoutNumber, pos_y) = GetLayoutNumberFromIndex(index, splitLength);
            int result = TextLayouts[arrayIndex].Layout.AlignIndexToNearestCluster(relativeIndex, flow);
            result += layoutNumber * splitLength;
            return result;
        }

        public void Dispose()
        {
            foreach (var sublayout in TextLayouts)
                sublayout.Layout.Dispose();
            this.Invaild = true;
            this.Disposed = true;
        }

        public double GetColPostionFromIndex(int index)
        {
            //一定の文字数を超えると強制的に改行されるので実装しなくていい
            throw new NotImplementedException();
        }

        public int GetIndexFromColPostion(double colpos)
        {
            //一定の文字数を超えると強制的に改行されるので実装しなくていい
            throw new NotImplementedException();
        }

        public int GetIndexFromPostion(double x, double y)
        {
            int index = 0;
            int layoutNumber = 0;
            double pos_x = 0, pos_y = 0;

            layoutNumber = (int)this.TextLayouts.GetIndexFromAbsoluteSumHeight(y,out pos_y);
            if(layoutNumber == - 1)
            {
                if(this.TextLayouts.Count > 0)
                    layoutNumber = this.TextLayouts.Count - 1;
                else
                    layoutNumber = 0;
            }

            index = (int)this.TextLayouts[layoutNumber].start;

            int relativeIndex = TextLayouts[layoutNumber].Layout.GetIndexFromPostion(x - pos_x, y - pos_y);
            return relativeIndex + index;
        }

        public Point GetPostionFromIndex(int index)
        {
            int splitLength = Document.MaximumLineLength;
            int relativeIndex, layoutNumber, arrayIndex;
            double pos_y;
            (relativeIndex, arrayIndex, layoutNumber, pos_y) = GetLayoutNumberFromIndex(index, splitLength);
            Point relativePointInSublayout = TextLayouts[arrayIndex].Layout.GetPostionFromIndex(relativeIndex);
            return new Point(relativePointInSublayout.X, pos_y + relativePointInSublayout.Y);
        }

        public double GetWidthFromIndex(int index)
        {
            int relativeIndex, layoutNumber, arrayIndex;
            double pos_y;
            (relativeIndex, arrayIndex, layoutNumber, pos_y) = GetLayoutNumberFromIndex(index, Document.MaximumLineLength);
            return TextLayouts[arrayIndex].Layout.GetWidthFromIndex(relativeIndex);
        }

        public void Draw(double x, double y,Action<ITextLayout,int,double,double> action)
        {
            double pos_x = x,pos_y = y;
            int index_main_layout = 0;
            foreach (var sublayout in TextLayouts)
            {
                action(sublayout.Layout, index_main_layout, pos_x, pos_y);
                pos_y += sublayout.Layout.Height;
                index_main_layout += Document.MaximumLineLength;
            }
        }
    }
}
