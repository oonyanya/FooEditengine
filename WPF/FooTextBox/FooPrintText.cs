/*
 * Copyright (C) 2013 FooProject
 * * This program is free software; you can redistribute it and/or modify it under the terms of the GNU General Public License as published by
 * the Free Software Foundation; either version 3 of the License, or (at your option) any later version.

 * This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of 
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.

You should have received a copy of the GNU General Public License along with this program. If not, see <http://www.gnu.org/licenses/>.
 */
using System;
using System.Threading.Tasks;
using System.Printing;
using System.Windows;
using System.Windows.Xps;
using Shapes = System.Windows.Shapes;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Documents.Serialization;
using System.Windows.Media;

namespace FooEditEngine.WPF
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
        public ParseCommandEventArgs(int nowPage,int maxPage,string org)
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
    public delegate string ParseCommandHandler(object sender,ParseCommandEventArgs e);

    /// <summary>
    /// 印刷用のクラス
    /// </summary>
    public class FooPrintText
    {
        /// <summary>
        /// コンストラクター
        /// </summary>
        public FooPrintText()
        {
            this.ParseHF = new ParseCommandHandler((s, e) => { return e.Original; });
        }

        /// <summary>
        /// 印刷する最小のページ番号
        /// </summary>
        public int StartPage
        {
            get;
            set;
        }

        /// <summary>
        /// 印刷する最大のページ番号
        /// </summary>
        public int EndPage
        {
            get;
            set;
        }

        /// <summary>
        /// 印刷する領域の大きさ
        /// </summary>
        public System.Windows.Rect PageRect
        {
            get;
            set;
        }

        /// <summary>
        /// 対象となるドキュメント
        /// </summary>
        public Document Document
        {
            get;
            set;
        }

        /// <summary>
        /// レタリング時のフロー方向を示す
        /// </summary>
        public FlowDirection FlowDirection
        {
            get;
            set;
        }

        /// <summary>
        /// 行番号を表示するかどうか
        /// </summary>
        public bool DrawLineNumber
        {
            get;
            set;
        }

        /// <summary>
        /// ハイパーリンクに下線を引くなら真
        /// </summary>
        public bool MarkURL
        {
            get;
            set;
        }

        /// <summary>
        /// デフォルトの文字ブラシ
        /// </summary>
        public System.Windows.Media.Color Foreground
        {
            get;
            set;
        }

        /// <summary>
        /// URLを表すブラシ
        /// </summary>
        public System.Windows.Media.Color URL
        {
            get;
            set;
        }

        /// <summary>
        /// キーワード１を表すブラシ
        /// </summary>
        public System.Windows.Media.Color Keyword1
        {
            get;
            set;
        }

        /// <summary>
        /// キーワード２を表すブラシ
        /// </summary>
        public System.Windows.Media.Color Keyword2
        {
            get;
            set;
        }

        /// <summary>
        /// コメントを表すブラシ
        /// </summary>
        public System.Windows.Media.Color Comment
        {
            get;
            set;
        }

        /// <summary>
        /// 文字リテラルを表すブラシ
        /// </summary>
        public System.Windows.Media.Color Litral
        {
            get;
            set;
        }

        /// <summary>
        /// 印刷に使用するフォント
        /// </summary>
        public FontFamily Font
        {
            get;
            set;
        }

        /// <summary>
        /// フォントサイズ
        /// </summary>
        public double FontSize
        {
            get;
            set;
        }

        /// <summary>
        /// 折り返しの方法を指定する
        /// </summary>
        public LineBreakMethod LineBreakMethod
        {
            get;
            set;
        }

        /// <summary>
        /// 折り返した時の文字数を指定する
        /// </summary>
        public int LineBreakCharCount
        {
            get;
            set;
        }

        /// <summary>
        /// ヘッダー
        /// </summary>
        public string Header
        {
            get;
            set;
        }

        /// <summary>
        /// フッター
        /// </summary>
        public string Footer
        {
            get;
            set;
        }

        /// <summary>
        /// シンタックスハイライター
        /// </summary>
        public IHilighter Hilighter
        {
            get;
            set;
        }

        /// <summary>
        /// 余白
        /// </summary>
        public Padding Padding
        {
            get;
            set;
        }

        /// <summary>
        /// ヘッダーやフッターを処理する
        /// </summary>
        public ParseCommandHandler ParseHF;

        /// <summary>
        /// 印刷する
        /// </summary>
        /// <param name="pd">プリントダイアログ</param>
        public void Print(PrintDialog pd)
        {
            if (this.Font == null || this.Document == null)
                throw new InvalidOperationException();

            WPFRender render = new WPFRender(this.Font, this.FontSize);
            render.Foreground = this.Foreground;
            render.Comment = this.Comment;
            render.Keyword1 = this.Keyword1;
            render.Keyword2 = this.Keyword2;
            render.Literal = this.Litral;
            render.Url = this.URL;
            render.RightToLeft = this.FlowDirection == System.Windows.FlowDirection.RightToLeft;
            render.Printing = true;
            Document documentSnap = new Document(this.Document);
            documentSnap.LayoutLines.Render = render;
            PrintableView view = new PrintableView(documentSnap, render,this.Padding);
            view.Header = this.Header;
            view.Footer = this.Footer;
            view.PageBound = this.PageRect;
            view.Hilighter = this.Hilighter;
            documentSnap.LineBreak = this.LineBreakMethod;
            documentSnap.LineBreakCharCount = this.LineBreakCharCount;
            documentSnap.DrawLineNumber = this.DrawLineNumber;
            documentSnap.UrlMark = this.MarkURL;
            documentSnap.PerformLayout(false);

            try
            {
                FixedDocument fd = new FixedDocument();
                fd.DocumentPaginator.PageSize = this.PageRect.Size;
                
                int currentPage = 0;

                bool result = false;

                while (!result)
                {
                    if (this.EndPage != -1 && currentPage > this.EndPage)
                        break;

                    if (this.StartPage == -1 || currentPage >= this.StartPage)
                    {
                        PageContent pc = new PageContent();
                        
                        FixedPage fp = new FixedPage();
                        fp.Width = this.PageRect.Width;
                        fp.Height = this.PageRect.Height;

                        pc.Child = fp;

                        view.Header = this.ParseHF(this, new ParseCommandEventArgs(currentPage, this.EndPage, this.Header));
                        view.Footer = this.ParseHF(this, new ParseCommandEventArgs(currentPage, this.EndPage, this.Footer));

                        DrawingVisual dv = new DrawingVisual();

                        using (DrawingContext dc = dv.RenderOpen())
                        {
                            render.SetDrawingContext(dc);
                            view.Draw(view.PageBound);
                        }

                        VisualHost host = new VisualHost();
                        host.AddVisual(dv);

                        fp.Children.Add(host);

                        fd.Pages.Add(pc);
                    }
                    result = view.TryPageDown();
                    currentPage++;
                }

                pd.PrintDocument(fd.DocumentPaginator,"");
            }
            catch (PrintingCanceledException)
            {
            }
            finally
            {
                view.Dispose();
            }
        }
    }

    class VisualHost : FrameworkElement
    {
        private List<Visual> fVisuals;

        public VisualHost()
        {
            fVisuals = new List<Visual>();
        }

        protected override Visual GetVisualChild(int index)
        {
            return fVisuals[index];
        }

        protected override int VisualChildrenCount
        {
            get { return fVisuals.Count; }
        }

        public void AddVisual(Visual visual)
        {
            fVisuals.Add(visual);
            base.AddVisualChild(visual);
        }

        public void RemoveVisual(Visual visual)
        {
            fVisuals.Remove(visual);
            base.RemoveVisualChild(visual);
        }
    }
}
