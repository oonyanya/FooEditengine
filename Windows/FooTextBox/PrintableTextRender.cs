using System;
using System.Collections.Generic;
using System.Drawing;

namespace FooEditEngine.Windows
{
    class PrintableTextLayout : ITextLayout
    {
        List<String> lines = new List<string>();
        Font font;
        StringFormat sf;
        public PrintableTextLayout(Font font, Graphics g, StringFormat sf, double maxwidth,String str)
        {
            this.Disposed = false;
            this.sf = sf;
            this.font = font;
            this.Height = font.Height;

            if (maxwidth == LineToIndexTable.NONE_BREAK_LINE)
            {
                lines.Add(str);
                return;
            }

            int fitlen, index = 0;
            do
            {
                int linesFilled;
                SizeF metrics = g.MeasureString(str.Substring(index), font, new SizeF((float)maxwidth, font.Height + 1), sf, out fitlen, out linesFilled);
                if (metrics.Width > Width)
                    this.Width = metrics.Width;
                this.Height += metrics.Height;
                lines.Add(str.Substring(index, fitlen));
                index += fitlen;
            } while (index < str.Length);
        }
        public double Width
        {
            get;
            private set;
        }

        public double Height
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
            return 0;
        }

        public int AlignIndexToNearestCluster(int index, AlignDirection flow)
        {
            return 0;
        }

        public void Dispose()
        {
            this.Disposed = true;
        }

        public int GetIndexFromPostion(double x, double y)
        {
            throw new NotImplementedException();
        }

        public Point GetPostionFromIndex(int index)
        {
            throw new NotImplementedException();
        }

        public void Draw(Graphics g,double x, double y,System.Drawing.Color fore)
        {
            double posy = y;
            foreach(string line in this.lines)
            {
                g.DrawString(line, this.font, new SolidBrush(fore), new PointF((float)x, (float)posy), this.sf);
                var size = g.MeasureString(line,this.font);
                posy += size.Height;
            }
        }
    }
    class PrintableTextRender : IPrintableTextRender
    {
        StringFormat sf;
        Font font;
        Graphics g;

        public PrintableTextRender(Font font, Graphics g)
        {
            this.font = font;
            this.sf = StringFormat.GenericTypographic;
            this.sf.FormatFlags |= StringFormatFlags.MeasureTrailingSpaces;
            this.g = g;
        }
#pragma warning disable 0067
        public event ChangedRenderResourceEventHandler ChangedRenderResource;
        public event EventHandler ChangedRightToLeft;
#pragma warning restore 0067

        public void BeginDraw(Graphics g)
        {
            this.g = g;
            if(this.RightToLeft)
                this.sf.FormatFlags |= StringFormatFlags.DirectionRightToLeft;
        }

        public void EndDraw()
        {
        }

        public ITextLayout CreateLaytout(string str)
        {
            return new PrintableTextLayout(this.font,this.g,this.sf,(float)LineToIndexTable.NONE_BREAK_LINE ,str);
        }

        public float HeaderHeight { get { return this.font.Height; } }

        public float FooterHeight { get { return this.font.Height; } }

        const int LineNumberFiledLength = 6;

        public double LineNemberWidth
        {
            get
            {
                int length = LineNumberFiledLength;
                length++;   //余白を確保する
                SizeF metrics = g.MeasureString("0", this.font, Int16.MaxValue, this.sf);
                return metrics.Width * length;
            }
        }

        public double FoldingWidth
        {
            get
            {
                return 0;
            }
        }

        public bool RightToLeft
        {
            get;
            set;
        }

        public bool InsertMode
        {
            get;
            set;
        }

        public Rectangle TextArea
        {
            get;
            set;
        }

        public Size emSize
        {
            get
            {
                SizeF metrics = g.MeasureString("0", this.font, Int16.MaxValue, this.sf);
                return new Size(metrics.Width, metrics.Height);
            }
        }

        public void DrawCachedBitmap(Rectangle rect)
        {
        }

        public void CacheContent()
        {
        }

        public bool IsVaildCache()
        {
            return false;
        }

        public void DrawLine(Point from, Point to)
        {
        }

        public void DrawString(string str, double x, double y)
        {
            g.DrawString(str, this.font, new SolidBrush(this.Foreground), new PointF((float)x, (float)y), this.sf);
        }

        public void DrawCaret(Rectangle rect, bool transparent)
        {
        }

        public void DrawFoldingMark(bool expand, double x, double y)
        {
        }

        public void DrawString(string str, double x, double y, StringAlignment align, Size layoutRect,StringColorType colorType = StringColorType.Forground)
        {
            System.Drawing.StringAlignment old = this.sf.Alignment;
            
            this.sf.Alignment = System.Drawing.StringAlignment.Center;

            g.DrawString(str, this.font, new SolidBrush(this.Foreground), new RectangleF((float)x, (float)y, (float)layoutRect.Width, (float)layoutRect.Height), this.sf);
            
            this.sf.Alignment = old;
        }

        public void DrawOneLine(Document doc,LineToIndexTable lti, int row, double x, double y)
        {
            PrintableTextLayout layout = (PrintableTextLayout)lti.GetLayout(row);
            layout.Draw(g, x, y, this.Foreground);
        }

        public void BeginClipRect(Rectangle rect)
        {
            g.Clip = new Region(rect);
        }

        public void EndClipRect()
        {
            g.Clip = new Region();
        }

        public void FillRectangle(Rectangle rect, FillRectType type)
        {
        }

        public void FillBackground(Rectangle rect)
        {
        }

        public void DrawGripper(Point p, double radius)
        {
            //タッチには対応していないので実装する必要はない
            throw new NotImplementedException();
        }

        public ITextLayout CreateLaytout(string str, SyntaxInfo[] syntaxCollection, IEnumerable<Marker> MarkerRanges, IEnumerable<Selection> Selections, double WrapWidth)
        {
            return new PrintableTextLayout(this.font, this.g, this.sf, WrapWidth, str);
        }

        public System.Drawing.Color Foreground
        {
            get;
            set;
        }

        public int TabWidthChar
        {
            get {
                float taboffset;
                float[] tabstops = this.sf.GetTabStops(out taboffset);
                if (tabstops.Length == 0)
                    return 0;
                return (int)tabstops[0];
            }
            set { this.sf.SetTabStops(0,new float[]{value});}
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
    }
}
