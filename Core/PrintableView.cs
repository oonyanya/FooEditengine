/*
 * Copyright (C) 2013 FooProject
 * * This program is free software; you can redistribute it and/or modify it under the terms of the GNU General Public License as published by
 * the Free Software Foundation; either version 3 of the License, or (at your option) any later version.

 * This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of 
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.

You should have received a copy of the GNU General Public License along with this program. If not, see <http://www.gnu.org/licenses/>.
 */
using System;

namespace FooEditEngine
{
    sealed class PrintableView : ViewBase
    {
        public PrintableView(Document doc, IPrintableTextRender r, Padding margin)
            : base (doc,r,margin)
        {
        }

        public string Header
        {
            get;
            set;
        }

        public string Footer
        {
            get;
            set;
        }

        public override void Draw(Rectangle updateRect, bool force = false)
        {
            if (this.LayoutLines.Count == 0)
                return;

            if (this.Hilighter != null)
                this.Hilighter.Reset();

            Point pos = this.render.TextArea.TopLeft;
            pos.X = Src.X;
            pos.Y = 0;

            IPrintableTextRender render = (IPrintableTextRender)this.render;

            //ヘッダーを印刷する
            if (this.Header != null && this.Header != string.Empty)
            {
                this.render.DrawString(this.Header, this.PageBound.X, pos.Y, StringAlignment.Center,
                    new Size(this.PageBound.Width, render.FooterHeight));
                pos.Y += (int)render.HeaderHeight;
            }

            //レイアウト行を印刷する
            double alignedPage = (int)(this.render.TextArea.Height / this.render.emSize.Height) * this.render.emSize.Height;
            Rectangle contentArea = new Rectangle(pos.X, pos.Y, this.PageBound.Width, alignedPage);
            this.render.BeginClipRect(contentArea);

            Size lineNumberSize = new Size(this.render.LineNemberWidth, this.render.TextArea.Height);
            for (int i = Src.Row; pos.Y <= this.render.TextArea.Bottom; i++)
            {
                if (i >= this.LayoutLines.Count)
                    break;

                double layoutHeight = this.LayoutLines.GetLayout(i).Height;

                this.render.DrawOneLine(this.Document, this.LayoutLines, i, pos.X + this.render.TextArea.X, pos.Y + this.Src.OffsetY);
                if (this.Document.DrawLineNumber)
                    this.render.DrawString((i + 1).ToString(), this.PageBound.X + this.GetRealtiveX(AreaType.LineNumberArea), pos.Y + this.Src.OffsetY, StringAlignment.Right, lineNumberSize);

                pos.Y += layoutHeight;

            }

            this.render.EndClipRect();

            //フッターを印刷する
            if (this.Footer != null && this.Footer != string.Empty)
            {
                pos.Y = render.TextArea.Bottom;
                this.render.DrawString(this.Footer, this.PageBound.X, pos.Y, StringAlignment.Center,
                    new Size(this.PageBound.Width, render.FooterHeight));
            }

            return;
        }

        public bool TryPageDown()
        {
            double alignedPage = (int)(this.render.TextArea.Height / this.render.emSize.Height) * this.render.emSize.Height;
            return base.TryScroll(this.Src.X, alignedPage);
        }

        protected override void CalculateClipRect()
        {
            double x, y, width, height;

            if (this.Document.DrawLineNumber)
            {
                if (this.render.RightToLeft)
                    x = this.Padding.Left;
                else
                    x = this.render.LineNemberWidth + this.render.emSize.Width + this.Padding.Left + this.LineNumberMargin;
                width = this.PageBound.Width - this.render.LineNemberWidth - this.render.emSize.Width - this.Padding.Right - this.Padding.Left - this.LineNumberMargin;
            }
            else
            {
                x = this.Padding.Left;
                width = this.PageBound.Width  - this.Padding.Right - this.Padding.Left;
            }

            y = this.Padding.Top;
            height = this.PageBound.Height - this.Padding.Bottom - this.Padding.Top;

            if (width < 0)
                width = 0;

            if (height < 0)
                height = 0;

            IPrintableTextRender render = (IPrintableTextRender)this.render;

            if (this.Footer != null && this.Footer != string.Empty)
                height -= render.FooterHeight;
            if (this.Header != null && this.Header != string.Empty)
                height -= render.HeaderHeight;

            this.render.TextArea = new Rectangle(x, y, width, height);
        }

        public override void CalculateLineCountOnScreen()
        {
        }

        enum AreaType
        {
            LineNumberArea,
            TextArea
        }

        double GetRealtiveX(AreaType type)
        {
            switch (type)
            {
                case AreaType.LineNumberArea:
                    if (this.Document.DrawLineNumber == false)
                        throw new InvalidOperationException();
                    if (this.render.RightToLeft)
                        return this.PageBound.TopRight.X - this.render.LineNemberWidth;
                    else
                        return this.render.TextArea.X - this.render.LineNemberWidth - this.render.emSize.Width - this.LineNumberMargin;
                case AreaType.TextArea:
                    return this.render.TextArea.X;
            }
            throw new ArgumentOutOfRangeException();
        }
    }
}
