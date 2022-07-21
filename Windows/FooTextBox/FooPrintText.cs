/*
 * Copyright (C) 2013 FooProject
 * * This program is free software; you can redistribute it and/or modify it under the terms of the GNU General Public License as published by
 * the Free Software Foundation; either version 3 of the License, or (at your option) any later version.

 * This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of 
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.

You should have received a copy of the GNU General Public License along with this program. If not, see <http://www.gnu.org/licenses/>.
 */
using System;
using System.Drawing;
using System.Drawing.Printing;

namespace FooEditEngine.Windows
{
    /// <summary>
    /// イベントデータ
    /// </summary>
    public class ParseCommandEventArgs
    {
        /// <summary>
        /// ページ番号
        /// </summary>
        public int PageNumber;
        /// <summary>
        /// プリンターの設定
        /// </summary>
        public PrinterSettings PrinterSetting;
        /// <summary>
        /// 処理前の文字列
        /// </summary>
        public string Original;
        /// <summary>
        /// コンストラクター
        /// </summary>
        /// <param name="nowPage">印刷中のページ番号</param>
        /// <param name="setting">プリンターの設定</param>
        /// <param name="org">処理前の文字列</param>
        public ParseCommandEventArgs(int nowPage,PrinterSettings setting,string org)
        {
            this.PageNumber = nowPage;
            this.PrinterSetting = setting;
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
        PrintableView view;
        PrintableTextRender render;
        int PageNumber;

        /// <summary>
        /// コンストラクター
        /// </summary>
        public FooPrintText()
        {
            this.PrintDocument = new PrintDocument();
            this.PrintDocument.PrintPage += new PrintPageEventHandler(PrintDocument_PrintPage);
            this.PrintDocument.EndPrint += new PrintEventHandler(PrintDocument_EndPrint);
            this.ParseHF = new ParseCommandHandler((s, e) => { return e.Original; });
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
        /// プリントドキュメント
        /// </summary>
        public PrintDocument PrintDocument
        {
            get;
            private set;
        }

        /// <summary>
        /// 右から左に表示するなら真
        /// </summary>
        public bool RightToLeft
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
        /// 印刷に使用するフォント
        /// </summary>
        public Font Font
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
        /// 余白
        /// </summary>
        public Padding Padding
        {
            get;
            set;
        }

        /// <summary>
        /// 前景色
        /// </summary>
        public System.Drawing.Color Foreground
        {
            get;
            set;
        }

        /// <summary>
        /// ヘッダーやフッターを処理する
        /// </summary>
        public ParseCommandHandler ParseHF;

        void PrintDocument_PrintPage(object sender, PrintPageEventArgs e)
        {
            if (this.Font == null || this.Document == null)
                throw new InvalidOperationException();

            if (view == null)
            {
                this.render = new PrintableTextRender(this.Font, e.Graphics);
                this.render.Foreground = this.Foreground;
                this.render.RightToLeft = this.RightToLeft;
                Document documentSnap = new Document(this.Document);
                documentSnap.LayoutLines.Render = render;
                this.view = new PrintableView(documentSnap, this.render,this.Padding);
                this.view.PageBound = e.MarginBounds;
                this.PageNumber = 1;
                documentSnap.LineBreak = this.LineBreakMethod;
                documentSnap.LineBreakCharCount = this.LineBreakCharCount;
                documentSnap.DrawLineNumber = this.DrawLineNumber;
                documentSnap.UrlMark = this.Document.UrlMark;
                documentSnap.PerformLayout(false);
            }

            if (e.PageSettings.PrinterSettings.PrintRange == PrintRange.SomePages)
            {
                for (; this.PageNumber < e.PageSettings.PrinterSettings.FromPage; this.PageNumber++)
                {
                    if (this.view.TryPageDown())
                        return;
                }
            }

            this.view.Header = this.ParseHF(this, new ParseCommandEventArgs(this.PageNumber, e.PageSettings.PrinterSettings,this.Header));
            this.view.Footer = this.ParseHF(this, new ParseCommandEventArgs(this.PageNumber, e.PageSettings.PrinterSettings, this.Footer));

            this.render.BeginDraw(e.Graphics);

            this.view.Draw(e.MarginBounds);

            e.HasMorePages = !this.view.TryPageDown();

            this.render.EndDraw();

            this.PageNumber++;

            if (e.HasMorePages && e.PageSettings.PrinterSettings.PrintRange == PrintRange.SomePages && this.PageNumber > e.PageSettings.PrinterSettings.ToPage)
                e.HasMorePages = false;
        }

        void PrintDocument_EndPrint(object sender, PrintEventArgs e)
        {
            this.view.Dispose();
            this.view = null;
            this.render = null;
        }

    }
}
