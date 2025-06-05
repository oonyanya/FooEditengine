using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;

namespace FooEditEngine
{
    class CombineTextLayout : ITextLayout
    {
        List<ITextLayout> TextLayouts;

        public CombineTextLayout(IEnumerable<ITextLayout> textLayouts)
        {
            this.TextLayouts = new List<ITextLayout>();
            this.TextLayouts.AddRange(textLayouts);
            this.Width = TextLayouts.Max((layout) => layout.Width);
            this.Height = TextLayouts.Sum((layout) => layout.Height);
        }

        public ITextLayout this[int i]
        {
            get { return TextLayouts[i]; }
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

        public int AlignIndexToNearestCluster(int index, AlignDirection flow)
        {
            int splitLength = Document.MaximumLineLength;
            int relativeIndex = index;
            int layoutNumber = 0;
            while (relativeIndex >= splitLength)
            {
                relativeIndex -= splitLength;
                layoutNumber++;
            }
            int arrayIndex = layoutNumber;
            if (arrayIndex >= TextLayouts.Count)
                arrayIndex = TextLayouts.Count - 1;
            int result = TextLayouts[arrayIndex].AlignIndexToNearestCluster(relativeIndex, flow);
            result += layoutNumber * splitLength;
            return result;
        }

        public void Dispose()
        {
            foreach (ITextLayout layout in TextLayouts)
                layout.Dispose();
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
            int splitLength = Document.MaximumLineLength;
            int index = 0 ;
            int layoutNumber = 0;
            double pos_x = 0, pos_y = 0;
            while (true)
            {
                if (layoutNumber >= TextLayouts.Count)
                {
                    layoutNumber = TextLayouts.Count - 1;
                    break;
                }
                double layoutHeight = TextLayouts[layoutNumber].Height;
                if (pos_y + layoutHeight > y)
                    break;
                pos_y += layoutHeight;
                index += splitLength;
                layoutNumber++;
            }
            int relativeIndex = TextLayouts[layoutNumber].GetIndexFromPostion(x - pos_x,y - pos_y);
            return relativeIndex + index;
        }

        public Point GetPostionFromIndex(int index)
        {
            int splitLength = Document.MaximumLineLength;
            int relativeIndex = index;
            int layoutNumber = 0;
            double pos_x = 0,pos_y = 0;
            while (relativeIndex >= splitLength)
            {
                relativeIndex -= splitLength;
                pos_y += TextLayouts[layoutNumber].Height;
                layoutNumber++;
            }
            if(layoutNumber >= TextLayouts.Count)
                layoutNumber = TextLayouts.Count - 1;
            Point relativePointInSublayout = TextLayouts[layoutNumber].GetPostionFromIndex(relativeIndex);
            return new Point(pos_x + relativePointInSublayout.X, pos_y + relativePointInSublayout.Y);
        }

        public double GetWidthFromIndex(int index)
        {
            int splitLength = Document.MaximumLineLength;
            int relativeIndex = index;
            int layoutNumber = 0;
            while (relativeIndex >= splitLength)
            {
                relativeIndex -= splitLength;
                layoutNumber++;
            }
            if (layoutNumber >= TextLayouts.Count)
                layoutNumber = TextLayouts.Count - 1;
            return TextLayouts[layoutNumber].GetWidthFromIndex(relativeIndex);
        }

        public void Draw(double x, double y,Action<ITextLayout,int,double,double> action)
        {
            double pos_x = x,pos_y = y;
            int index_main_layout = 0;
            foreach (var layout in TextLayouts)
            {
                action(layout, index_main_layout, pos_x, pos_y);
                pos_y += layout.Height;
                index_main_layout += Document.MaximumLineLength;
            }
        }
    }
}
