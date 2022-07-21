/*
 * Copyright (C) 2013 FooProject
 * * This program is free software; you can redistribute it and/or modify it under the terms of the GNU General Public License as published by
 * the Free Software Foundation; either version 3 of the License, or (at your option) any later version.

 * This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of 
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.

You should have received a copy of the GNU General Public License along with this program. If not, see <http://www.gnu.org/licenses/>.
 */
using Microsoft.Graphics.Canvas.Printing;
using System;
using Windows.Graphics.Printing;

namespace FooEditEngine.WinUI
{
    /// <summary>
    /// イベントデータ
    /// </summary>
    public sealed class ParseCommandEventArgs
    {
        /// <summary>
        /// 印刷中のページ番号
        /// </summary>
        public int PageNumber;
        /// <summary>
        /// ページ範囲内で許容されている最大の番号
        /// </summary>
        public int MaxPageNumber;
        /// <summary>
        /// 処理前の文字列
        /// </summary>
        public string Original;
        /// <summary>
        /// コンストラクター
        /// </summary>
        /// <param name="nowPage">印刷中のページ番号</param>
        /// <param name="maxPage">印刷すべき最大のページ番号</param>
        /// <param name="org">処理前の文字列</param>
        public ParseCommandEventArgs(int nowPage, int maxPage, string org)
        {
            this.PageNumber = nowPage;
            this.MaxPageNumber = maxPage;
            this.Original = org;
        }
    }

    /// <summary>
    /// コマンド処理用デリゲート
    /// </summary>
    /// <param name="sender">送信元のクラス</param>
    /// <param name="e">イベントデータ</param>
    /// <returns>処理後の文字列</returns>
    public delegate string ParseCommandHandler(object sender, ParseCommandEventArgs e);


    sealed class PrintableViewFactory
    {
        Padding padding;

        public PrintableViewFactory(Padding padding)
        {
            this.padding = padding;
        }

        public PrintableView CreateView(Document document, PrintPageDescription pagedesc, IPrintableTextRender render, string header, string footer)
        {
            document.LayoutLines.Render = render;
            PrintableView view = new PrintableView(document, render, padding);
            view.Header = header;
            view.Footer = footer;
            view.PageBound = new Rectangle(pagedesc.ImageableRect.X, pagedesc.ImageableRect.Y, pagedesc.ImageableRect.Width, pagedesc.ImageableRect.Height);
            document.PerformLayout(false);

            return view;
        }

    }

    public interface IFooDocumentSource
    {
        void InvalidatePreview();
    }

    /// <summary>
    /// 印刷用ソース
    /// </summary>
    public sealed class DocumentSource : IFooDocumentSource
    {
        Win2DPrintRender render;
        PrintableViewFactory factory;
        Document doc;
        PrintableView view;
        IHilighter hilighter;
        int maxPreviePageCount;

        public ParseCommandHandler ParseHF;
        public string Header = string.Empty;
        public string Fotter = string.Empty;

        /// <summary>
        /// コンストラクター
        /// </summary>
        /// <param name="textbox"></param>
        /// <param name="padding"></param>
        /// <param name="fontName"></param>
        /// <param name="fontSize"></param>
        public DocumentSource(Document doc, Padding padding, string fontName, double fontSize)
        {
            this.doc = new Document(doc);
            this.hilighter = doc.LayoutLines.Hilighter;
            this.PrintDocument = new CanvasPrintDocument();
            this.render = new Win2DPrintRender(this.PrintDocument.Device);
            this.render.InitTextFormat(fontName, (float)fontSize);
            this.PrintDocument.Preview += PrintDocument_Preview;
            this.PrintDocument.Print += PrintDocument_Print;
            this.PrintDocument.PrintTaskOptionsChanged += PrintDocument_PrintTaskOptionsChanged;
            this.factory = new PrintableViewFactory(padding);
        }

        public CanvasPrintDocument PrintDocument
        {
            get;
            private set;
        }

        public enum SyntaxHilightApplibility
        {
            Apply,
            NoApply,
        }

        public Windows.UI.Color Forground
        {
            get
            {
                return this.render.Foreground;
            }
            set
            {
                this.render.Foreground = value;
            }
        }

        public Windows.UI.Color Comment
        {
            get
            {
                return this.render.Comment;
            }
            set
            {
                this.render.Comment = value;
            }
        }

        public Windows.UI.Color Keyword1
        {
            get
            {
                return this.render.Keyword1;
            }
            set
            {
                this.render.Keyword1 = value;
            }
        }

        public Windows.UI.Color Keyword2
        {
            get
            {
                return this.render.Keyword2;
            }
            set
            {
                this.render.Keyword2 = value;
            }
        }

        public Windows.UI.Color Literal
        {
            get
            {
                return this.render.Literal;
            }
            set
            {
                this.render.Literal = value;
            }
        }

        public Windows.UI.Color Url
        {
            get
            {
                return this.render.Url;
            }
            set
            {
                this.render.Url = value;
            }
        }

        public LineBreakMethod LineBreak
        {
            get;
            set;
        }

        public int LineBreakCount
        {
            get;
            set;
        }

        [DisplayPrintOptionResourceID("SyntaxHilight")]
        public SyntaxHilightApplibility EnableHilight
        {
            get;
            set;
        }

        public enum LineNumberVisiblity
        {
            Visible,
            Hidden
        }

        [DisplayPrintOptionResourceID("ShowLineNumber")]
        public LineNumberVisiblity ShowLineNumber
        {
            get;
            set;
        }

        public void InvalidatePreview()
        {
            this.PrintDocument.InvalidatePreview();
        }

        private void PrintDocument_Print(CanvasPrintDocument sender, CanvasPrintEventArgs args)
        {
            if (this.view == null)
            {
                System.Diagnostics.Debug.WriteLine("must be make view");
                return;
            }

            view.TryScroll(0, 0);

            bool result = false;
            int currentPage = 0;

            while (!result)
            {
                if (!string.IsNullOrEmpty(this.Header))
                    view.Header = this.ParseHF(this, new ParseCommandEventArgs(currentPage, this.maxPreviePageCount, this.Header));
                if (!string.IsNullOrEmpty(this.Fotter))
                    view.Footer = this.ParseHF(this, new ParseCommandEventArgs(currentPage, this.maxPreviePageCount, this.Fotter));

                using (var ds = args.CreateDrawingSession())
                {
                    render.DrawContent(ds, view);
                }

                result = view.TryPageDown();
                currentPage++;
            }
        }

        private void PrintDocument_Preview(CanvasPrintDocument sender, CanvasPreviewEventArgs args)
        {
            if(this.view == null)
            {
                System.Diagnostics.Debug.WriteLine("must be make view");
                return;
            }

            view.TryScroll(0, 0);

            int currentPage = (int)args.PageNumber;
            for (int i = 1; i < args.PageNumber; i++)
                view.TryPageDown();

            if (!string.IsNullOrEmpty(this.Header))
                view.Header = this.ParseHF(this, new ParseCommandEventArgs(currentPage, maxPreviePageCount, this.Header));
            if (!string.IsNullOrEmpty(this.Fotter))
                view.Footer = this.ParseHF(this, new ParseCommandEventArgs(currentPage, maxPreviePageCount, this.Fotter));

            render.DrawContent(args.DrawingSession, view);
        }

        private void PrintDocument_PrintTaskOptionsChanged(CanvasPrintDocument sender, CanvasPrintTaskOptionsChangedEventArgs args)
        {
            PrintTaskOptions options = (PrintTaskOptions)args.PrintTaskOptions;
            PrintPageDescription pagedesc = options.GetPageDescription(1);

            this.doc.DrawLineNumber = this.ShowLineNumber == LineNumberVisiblity.Visible;
            this.doc.LayoutLines.Hilighter = this.EnableHilight == SyntaxHilightApplibility.Apply ? this.hilighter : null;
            this.view = this.factory.CreateView(this.doc, pagedesc, render, this.Header, this.Fotter);
            this.doc.LineBreak = this.LineBreak;
            this.doc.LineBreakCharCount = this.LineBreakCount;
            this.doc.LayoutLines.HilightAll();

            this.maxPreviePageCount = 1;

            bool result = false;
            while (!result)
            {
                if (!string.IsNullOrEmpty(this.Header))
                    view.Header = this.ParseHF(this, new ParseCommandEventArgs(this.maxPreviePageCount, this.maxPreviePageCount, this.Header));
                if (!string.IsNullOrEmpty(this.Fotter))
                    view.Footer = this.ParseHF(this, new ParseCommandEventArgs(this.maxPreviePageCount, this.maxPreviePageCount, this.Fotter));
                sender.SetIntermediatePageCount((uint)this.maxPreviePageCount);
                result = view.TryPageDown();
                this.maxPreviePageCount++;
            }

        }
    }
}
