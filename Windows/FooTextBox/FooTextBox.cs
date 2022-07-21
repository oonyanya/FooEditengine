/*
 * Copyright (C) 2013 FooProject
 * * This program is free software; you can redistribute it and/or modify it under the terms of the GNU General Public License as published by
 * the Free Software Foundation; either version 3 of the License, or (at your option) any later version.

 * This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of 
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.

You should have received a copy of the GNU General Public License along with this program. If not, see <http://www.gnu.org/licenses/>.
 */
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;
using System.ComponentModel;
using Microsoft.Win32;

namespace FooEditEngine.Windows
{
    /// <summary>
    /// タブの幅が変更されたときに呼びされるデリゲート
    /// </summary>
    /// <param name="sender">送り主が属するクラス</param>
    /// <param name="e">イベントデータ</param>
    public delegate void TabStopChangeEventHandler(object sender, EventArgs e);

    /// <summary>
    /// InsetModeが変更されたときに呼び出されるデリゲート
    /// </summary>
    /// <param name="sender">送り主が属するクラス</param>
    /// <param name="e">イベントデータ</param>
    public delegate void InsertModeChangeEventHandler(object sender, EventArgs e);

    /// <summary>
    /// FooEditEngineを表します
    /// </summary>
    public class FooTextBox : Control
    {
        EditView View;
        Controller Controller;
        D2DTextRender render;
        BorderStyle _BoderStyle;
        HScrollBar HScrollBar;
        VScrollBar VScrollBar;
        WinIME Ime;
        System.Windows.Forms.Timer Timer;

        const int Interval = 100;

        /// <summary>
        /// コンストラクター
        /// </summary>
        public FooTextBox()
        {
            this.VScrollBar = new VScrollBar();
            this.VScrollBar.Scroll += new ScrollEventHandler(VScrollBar_Scroll);
            this.VScrollBar.Dock = DockStyle.Right;
            this.VScrollBar.Visible = true;
            this.Controls.Add(this.VScrollBar);

            this.HScrollBar = new HScrollBar();
            this.HScrollBar.Scroll += new ScrollEventHandler(HScrollBar_Scroll);
            this.HScrollBar.Dock = DockStyle.Bottom;
            this.HScrollBar.Visible = true;
            this.Controls.Add(this.HScrollBar);

            this.SetStyle(ControlStyles.ResizeRedraw, true);
            this.SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            this.SetStyle(ControlStyles.UserPaint, true);
            this.SetStyle(ControlStyles.Opaque, true);

            this.render = new D2DTextRender(this);
            this.Document = new Document();
            this.Document.LayoutLines.Render = this.render;
            this.Document.AutoComplete = new AutoCompleteBox(this);
            this.Document.AutoComplete.GetPostion = (tp, doc) => {
                var p = this.GetPostionFromTextPoint(tp);
                p.Y += (int)this.render.emSize.Height;
                return p;
            };
            this.View = new EditView(this.Document, this.render, new FooEditEngine.Padding(5, 5, 5, 5));
            this.View.SrcChanged += View_SrcChanged;
            
            this.Controller = new Controller(this.Document, this.View);
            this.Document.SelectionChanged += new EventHandler(Controller_CaretMoved);

            this.Ime = new WinIME(this);
            this.Ime.ImeCompstion += new ImeCompstionEventHandeler(Ime_ImeCompstion);
            this.Ime.StartCompstion += new StartCompstionEventHandeler(Ime_StartCompstion);
            this.Ime.EndCompstion += new EndCompstionEventHandeler(Ime_EndCompstion);
            this.Ime.ImeDocumentFeed += new ImeDocumentFeedEventHandler(Ime_ImeDocumentFeed);
            this.Ime.ImeReconvert += new ImeReconvertStringEventHandler(Ime_ImeReconvert);
            this.Ime.ImeQueryReconvert += new ImeQueryReconvertStringEventHandler(Ime_ImeQueryReconvert);

            this.Timer = new System.Windows.Forms.Timer();
            this.Timer.Tick += new EventHandler(this.Timer_Tick);
            this.Timer.Interval = Interval;
            this.SetSystemParamaters();

            this.TabStopChange += new TabStopChangeEventHandler((s, e) => { });
            this.InsetModeChange += new InsertModeChangeEventHandler((s, e) => { });
            this.SelectionChanged +=new EventHandler((s,e)=>{});

            this.RightToLeftChanged += FooTextBox_RightToLeftChanged;

            SystemEvents.UserPreferenceChanged += new UserPreferenceChangedEventHandler(SystemEvents_UserPreferenceChanged);

        }

        /// <summary>
        /// キャレットが移動したときに通知されるイベント
        /// </summary>
        public event EventHandler SelectionChanged;

        /// <summary>
        /// タブの幅が変更された時に発生するイベント
        /// </summary>
        public event TabStopChangeEventHandler TabStopChange;

        /// <summary>
        /// InsertModeが変更されたときに呼び出されるイベント
        /// </summary>
        public event InsertModeChangeEventHandler InsetModeChange;

        /// <summary>
        /// インデントモードを表す
        /// </summary>
        [DefaultValue(IndentMode.Tab)]
        public IndentMode IndentMode
        {
            get
            {
                return this.Controller.IndentMode;
            }
            set
            {
                this.Controller.IndentMode = value;
            }
        }

        /// <summary>
        /// テキスト描写に使用するアンチエイリアシングモードを表す
        /// </summary>
        [BrowsableAttribute(false)]
        public TextAntialiasMode TextAntialiasMode
        {
            get
            {
                return this.render.TextAntialiasMode;
            }
            set
            {
                this.render.TextAntialiasMode = value;
            }
        }

        /// <summary>
        /// マーカーパターンセットを表す
        /// </summary>
        [BrowsableAttribute(false)]
        public MarkerPatternSet MarkerPatternSet
        {
            get
            {
                return this.Document.MarkerPatternSet;
            }
        }

        /// <summary>
        /// 保持しているドキュメント
        /// </summary>
        [BrowsableAttribute(false)]
        public Document Document
        {
            get;
            private set;
        }

        /// <summary>
        /// 保持しているレイアウト行
        /// </summary>
        [BrowsableAttribute(false)]
        public LineToIndexTable LayoutLines
        {
            get { return this.View.LayoutLines; }
        }

        /// <summary>
        /// シンタックスハイライター
        /// </summary>
        [BrowsableAttribute(false)]
        public IHilighter Hilighter
        {
            get { return this.View.Hilighter; }
            set { this.View.Hilighter = value; this.View.LayoutLines.ClearLayoutCache(); }
        }

        /// <summary>
        /// フォールティングを作成するインターフェイスを表す
        /// </summary>
        [BrowsableAttribute(false)]
        public IFoldingStrategy FoldingStrategy
        {
            get
            {
                return this.View.LayoutLines.FoldingStrategy;
            }
            set
            {
                this.View.LayoutLines.FoldingStrategy = value;
                if (value == null)
                    this.View.LayoutLines.FoldingCollection.Clear();
            }
        }

        /// <summary>
        /// 境界線のスタイルを指定します
        /// </summary>
        public BorderStyle BorderStyle
        {
            get { return this._BoderStyle; }
            set { this._BoderStyle = value; this.UpdateStyles(); }
        }


        /// <summary>
        /// 行番号を表示するかどうか
        /// </summary>
        [DefaultValue(false)]
        public bool DrawLineNumber
        {
            get
            {
                return this.Document.DrawLineNumber;
            }
            set
            {
                this.Document.DrawLineNumber = value;
            }
        }
        
        /// <summary>
        /// ルーラーを表示するかどうか
        /// </summary>
        [DefaultValue(false)]
        public bool DrawRuler
        {
            get
            {
                return !this.Document.HideRuler;
            }
            set
            {
                this.Document.HideRuler = !value;
                this.JumpCaret(this.CaretPostion.row, this.CaretPostion.col);
            }
        }

        /// <summary>
        /// 桁折りを行う方法を指定する
        /// </summary>
        /// <remarks>
        /// 反映させるためにはレイアウト行の再構築を行う必要があります
        /// </remarks>
        [DefaultValue(LineBreakMethod.None)]
        public LineBreakMethod LineBreakMethod
        {
            get
            {
                return this.Document.LineBreak;
            }
            set
            {
                this.Document.LineBreak = value;
            }
        }

        /// <summary>
        /// 桁折り時の文字数を指定する。
        /// </summary>
        /// <remarks>
        /// 反映させるためにはレイアウト行の再構築を行う必要があります
        /// </remarks>
        [DefaultValue(80)]
        public int LineBreakCharCount
        {
            get
            {
                return this.Document.LineBreakCharCount;
            }
            set
            {
                this.Document.LineBreakCharCount = value;
            }
        }

        /// <summary>
        /// URLをマークするかどうか
        /// </summary>
        [DefaultValue(false)]
        public bool UrlMark
        {
            get
            {
                return this.Document.UrlMark;
            }
            set
            {
                this.Document.UrlMark = value;
            }
        }

        /// <summary>
        /// タブストップの幅
        /// </summary>
        [DefaultValue(4)]
        public int TabStops
        {
            get { return this.Document.TabStops; }
            set {
                this.Document.TabStops = value;
                this.View.AdjustCaretAndSrc();
                this.TabStopChange(this, null);
            }
        }

        /// <summary>
        /// 全角スペースを表示するなら真。そうでないなら偽
        /// </summary>
        [DefaultValue(false)]
        public bool ShowFullSpace
        {
            get
            {
                return this.Document.ShowFullSpace;
            }
            set
            {
                this.Document.ShowFullSpace = value;
            }
        }

        /// <summary>
        /// 半角スペースを表示するなら真。そうでないなら偽
        /// </summary>
        [DefaultValue(false)]
        public bool ShowHalfSpace
        {
            get
            {
                return this.Document.ShowHalfSpace;
            }
            set
            {
                this.Document.ShowHalfSpace = value;
            }
        }

        /// <summary>
        /// タブを表示するなら真。そうでないなら偽
        /// </summary>
        [DefaultValue(false)]
        public bool ShowTab
        {
            get
            {
                return this.Document.ShowTab;
            }
            set
            {
                this.Document.ShowTab = value;
            }
        }

        /// <summary>
        /// 改行マークを表示するなら真。そうでないなら偽
        /// </summary>
        [DefaultValue(false)]
        public bool ShowLineBreak
        {
            get
            {
                return this.Document.ShowLineBreak;
            }
            set
            {
                this.Document.ShowLineBreak = value;
            }
        }

        /// <summary>
        /// 選択中の文字列
        /// </summary>
        /// <remarks>
        /// 未選択状態で文字列を代入した場合、キャレット位置に挿入され、そうでないときは置き換えられます。
        /// </remarks>
        [BrowsableAttribute(false)]
        public string SelectedText
        {
            get { return this.Controller.SelectedText; }
            set { this.Controller.SelectedText = value; }
        }

        /// <summary>
        /// キャレット位置を表す
        /// </summary>
        [BrowsableAttribute(false)]
        public TextPoint CaretPostion
        {
            get { return this.Document.CaretPostion; }
        }

        /// <summary>
        /// 選択範囲を表す
        /// </summary>
        /// <remarks>
        /// Lengthが0の場合はキャレット位置を表します
        /// 矩形選択モードの場合、選択範囲の文字数ではなく、開始位置から終了位置までの長さとなります
        /// </remarks>
        [BrowsableAttribute(false)]
        public TextRange Selection
        {
            get { return new TextRange(this.Controller.SelectionStart,this.Controller.SelectionLength); }
            set
            {
                this.Document.Select(value.Index, value.Length);
            }
        }

        /// <summary>
        /// 挿入モードかどうか
        /// </summary>
        [DefaultValue(true)]
        public bool InsertMode
        {
            get { return this.View.InsertMode; }
            set
            {
                this.View.InsertMode = value;
                this.InsetModeChange(this, null);
            }
        }

        /// <summary>
        /// 矩形選択を行うかどうか
        /// </summary>
        [DefaultValue(false)]
        public bool RectSelection
        {
            get { return this.Controller.RectSelection; }
            set { this.Controller.RectSelection = value; }
        }

        System.Drawing.Color ForegroundColor = SystemColors.ControlText,
            HilightForegroundColor = SystemColors.HighlightText,
            BackgroundColor = SystemColors.Control,
            HilightColor = System.Drawing.Color.DeepSkyBlue,
            Keyword1Color = System.Drawing.Color.Blue,
            Keyword2Color = System.Drawing.Color.DarkCyan,
            LiteralColor = System.Drawing.Color.Brown,
            UrlColor = System.Drawing.Color.Blue,
            ControlCharColor = System.Drawing.Color.Gray,
            CommentColor = System.Drawing.Color.Green,
            InsertCaretColor = System.Drawing.Color.Black,
            OverwriteCaretColor = System.Drawing.Color.Black,
            LineMarkerColor = System.Drawing.Color.WhiteSmoke,
            UpdateAreaColor = System.Drawing.Color.MediumSeaGreen,
            LineNumberColor = System.Drawing.Color.DimGray;

        /// <summary>
        /// 前景色
        /// </summary>
        public System.Drawing.Color Foreground
        {
            get
            {
                return this.ForegroundColor;
            }
            set
            {
                this.render.Foreground = D2DTextRender.ToColor4(value);
                this.ForegroundColor = value;
            }
        }

        /// <summary>
        /// 選択時の前景色
        /// </summary>
        public System.Drawing.Color HilightForeground
        {
            get
            {
                return this.HilightForegroundColor;
            }
            set
            {
                this.render.HilightForeground = D2DTextRender.ToColor4(value);
                this.HilightForegroundColor = value;
            }
        }

        /// <summary>
        /// 背景色
        /// </summary>
        public System.Drawing.Color Background
        {
            get
            {
                return this.BackgroundColor;
            }
            set
            {
                this.render.Background = D2DTextRender.ToColor4(value);
                this.BackgroundColor = value;
            }
        }

        /// <summary>
        /// 挿入モード時のキャレット色
        /// </summary>
        public System.Drawing.Color InsertCaret
        {
            get
            {
                return this.InsertCaretColor;
            }
            set
            {
                this.InsertCaretColor = value;
                this.render.InsertCaret = D2DTextRender.ToColor4(value);
            }
        }

        /// <summary>
        /// 上書きモード時のキャレット色
        /// </summary>
        public System.Drawing.Color OverwriteCaret
        {
            get
            {
                return this.OverwriteCaretColor;
            }
            set
            {
                this.OverwriteCaretColor = value;
                this.render.OverwriteCaret = D2DTextRender.ToColor4(value);
            }
        }

        /// <summary>
        /// ラインマーカーの色
        /// </summary>
        public System.Drawing.Color LineMarker
        {
            get
            {
                return this.LineMarkerColor;
            }
            set
            {
                this.LineMarkerColor = value;
                this.render.LineMarker = D2DTextRender.ToColor4(value);
            }
        }

        /// <summary>
        /// コントロールの色
        /// </summary>
        public System.Drawing.Color ControlChar
        {
            get
            {
                return this.ControlCharColor;
            }
            set
            {
                this.ControlCharColor = value;
                this.render.ControlChar = D2DTextRender.ToColor4(value);
            }
        }

        /// <summary>
        /// 編集行フラグの色
        /// </summary>
        public System.Drawing.Color UpdateArea
        {
            get
            {
                return this.UpdateAreaColor;
            }
            set
            {
                this.UpdateAreaColor = value;
                this.render.UpdateArea = D2DTextRender.ToColor4(value);
            }
        }

        /// <summary>
        /// 行番号の色
        /// </summary>
        public System.Drawing.Color LineNumber
        {
            get
            {
                return this.LineNumberColor;
            }
            set
            {
                this.LineNumberColor = value;
                this.render.LineNumber = D2DTextRender.ToColor4(value);
            }
        }

        /// <summary>
        /// URLの色
        /// </summary>
        public System.Drawing.Color Url
        {
            get
            {
                return this.UrlColor;
            }
            set
            {
                this.UrlColor = value;
                this.render.Url = D2DTextRender.ToColor4(value);
            }
        }

        /// <summary>
        /// 選択領域の色
        /// </summary>
        public System.Drawing.Color Hilight
        {
            get
            {
                return this.HilightColor;
            }
            set
            {
                this.HilightColor = value;
                this.render.Hilight = D2DTextRender.ToColor4(value);
            }
        }

        /// <summary>
        /// コメントの色
        /// </summary>
        public System.Drawing.Color Comment
        {
            get
            {
                return this.CommentColor;
            }
            set
            {
                this.CommentColor = value;
                this.render.Comment = D2DTextRender.ToColor4(value);
            }
        }

        /// <summary>
        /// 文字リテラルの色
        /// </summary>
        public System.Drawing.Color Literal
        {
            get
            {
                return this.LiteralColor;
            }
            set
            {
                this.LiteralColor = value;
                this.render.Literal = D2DTextRender.ToColor4(value);
            }
        }

        /// <summary>
        /// キーワード1の色
        /// </summary>
        public System.Drawing.Color Keyword1
        {
            get
            {
                return this.Keyword1Color;
            }
            set
            {
                this.Keyword1Color = value;
                this.render.Keyword1 = D2DTextRender.ToColor4(value);
            }
        }

        /// <summary>
        /// キーワード2の色
        /// </summary>
        public System.Drawing.Color Keyword2
        {
            get
            {
                return this.Keyword2Color;
            }
            set
            {
                this.Keyword2Color = value;
                this.render.Keyword2 = D2DTextRender.ToColor4(value);
            }
        }

        /// <summary>
        /// キャレットに下線を描くかどうか
        /// </summary>
        [DefaultValue(false)]
        public bool DrawCaretLine
        {
            get { return !this.View.HideLineMarker; }
            set { this.View.HideLineMarker = !value; }
        }

        /// <summary>
        /// ドキュメントを選択する
        /// </summary>
        /// <param name="start">開始インデックス</param>
        /// <param name="length">長さ</param>
        public void Select(int start, int length)
        {
            this.Document.Select(start, length);
            this.HScrollBar.Value = (int)this.View.Src.X;
            this.VScrollBar.Value = this.View.Src.Row;
        }

        /// <summary>
        /// ドキュメント全体を選択する
        /// </summary>
        public void SelectAll()
        {
            this.Document.Select(0, this.Document.Length - 1);
        }

        /// <summary>
        /// 選択を解除する
        /// </summary>
        public void DeSelectAll()
        {
            this.Controller.DeSelectAll();
        }

        /// <summary>
        /// クリップボードにコピーする
        /// </summary>
        public void Copy()
        {
            string text = this.SelectedText;
            if(text != null && text != string.Empty)
                Clipboard.SetText(text);
        }

        /// <summary>
        /// クリップボードにコピーし、指定した範囲にある文字列をすべて削除します
        /// </summary>
        public void Cut()
        {
            string text = this.SelectedText;
            if (text != null && text != string.Empty)
            {
                Clipboard.SetText(text);
                this.Controller.SelectedText = "";
            }
        }

        /// <summary>
        /// クリップボードの内容をペーストします
        /// </summary>
        public void Paste()
        {
            if (Clipboard.ContainsText() == false)
                return;
            this.Controller.SelectedText = Clipboard.GetText();
        }

        /// <summary>
        /// キャレットを指定した行に移動させます
        /// </summary>
        /// <param name="index">インデックス</param>
        /// <remarks>このメソッドを呼び出すと選択状態は解除されます</remarks>
        public void JumpCaret(int index)
        {
            this.Controller.JumpCaret(index);
        }
        /// <summary>
        /// キャレットを指定した行と桁に移動させます
        /// </summary>
        /// <param name="row">行番号</param>
        /// <param name="col">桁</param>
        /// <remarks>このメソッドを呼び出すと選択状態は解除されます</remarks>
        public void JumpCaret(int row, int col)
        {
            this.Controller.JumpCaret(row, col);
        }

        /// <summary>
        /// 再描写します
        /// </summary>
        public new void Refresh()
        {
            if (this.Document.FireUpdateEvent == false)
                throw new InvalidOperationException("");
            if(this.View.CaretBlink)
                this.View.CaretBlink = true;
            this.Invalidate();
            this.Update();
        }

        /// <summary>
        /// 行の高さを取得する
        /// </summary>
        /// <param name="row">行</param>
        /// <returns>高さ</returns>
        public double GetLineHeight(int row)
        {
            if (this.Document.FireUpdateEvent == false)
                throw new InvalidOperationException("");
            return this.View.LayoutLines.GetLayout(row).Height;
        }

        /// <summary>
        /// 対応する座標を返す
        /// </summary>
        /// <param name="tp">テキストポイント</param>
        /// <returns>座標</returns>
        /// <remarks>テキストポイントがクライアント領域の原点より外にある場合、返される値は原点に丸められます</remarks>
        public System.Drawing.Point GetPostionFromTextPoint(TextPoint tp)
        {
            if (this.Document.FireUpdateEvent == false)
                throw new InvalidOperationException("");
            return this.View.GetPostionFromTextPoint(tp);
        }

        /// <summary>
        /// 対応するテキストポイントを返す
        /// </summary>
        /// <param name="p">クライアント領域の原点を左上とする座標</param>
        /// <returns>テキストポイント</returns>
        public TextPoint GetTextPointFromPostion(System.Drawing.Point p)
        {
            if (this.Document.FireUpdateEvent == false)
                throw new InvalidOperationException("");
            return this.View.GetTextPointFromPostion(p);
        }

        /// <summary>
        /// インデックスに対応する座標を得ます
        /// </summary>
        /// <param name="index">インデックス</param>
        /// <returns>座標を返す</returns>
        public System.Drawing.Point GetPostionFromIndex(int index)
        {
            if (this.Document.FireUpdateEvent == false)
                throw new InvalidOperationException("");
            TextPoint tp = this.View.GetLayoutLineFromIndex(index);
            return this.View.GetPostionFromTextPoint(tp);
        }

        /// <summary>
        /// 座標からインデックスに変換します
        /// </summary>
        /// <param name="p">座標</param>
        /// <returns>インデックスを返す</returns>
        public int GetIndexFromPostion(System.Drawing.Point p)
        {
            if (this.Document.FireUpdateEvent == false)
                throw new InvalidOperationException("");
            TextPoint tp = this.View.GetTextPointFromPostion(p);
            return this.View.GetIndexFromLayoutLine(tp);
        }

        /// <summary>
        /// レイアウト行をすべて破棄し、再度レイアウトを行う
        /// </summary>
        public void PerfomLayouts()
        {
            this.Document.PerformLayout();
            initScrollBars();
        }

        /// <summary>
        /// ストリームからドキュメントを構築する
        /// </summary>
        /// <param name="tr">TextReader</param>
        /// <param name="token">キャンセル用トークン</param>
        /// <returns>Taskオブジェクト</returns>
        public async Task LoadAsync(System.IO.TextReader tr, System.Threading.CancellationTokenSource token)
        {
            await this.Document.LoadAsync(tr, token);
        }

        /// <summary>
        /// ファイルからドキュメントを構築する
        /// </summary>
        /// <param name="filepath">ファイルパス</param>
        /// <param name="enc">エンコード</param>
        /// <param name="token">キャンセル用トークン</param>
        /// <returns>Taskオブジェクト</returns>
        public async Task LoadFileAsync(string filepath, Encoding enc, System.Threading.CancellationTokenSource token)
        {
            var fs = new System.IO.StreamReader(filepath, enc);
            await this.Document.LoadAsync(fs, token);
            fs.Close();
        }

        private void Document_LoadProgress(object sender, ProgressEventArgs e)
        {
            if (e.state == ProgressState.Start)
            {
                this.Enabled = false;
            }
            else if (e.state == ProgressState.Complete)
            {
                this.initScrollBars();
                this.OnMouseMove(new MouseEventArgs(MouseButtons.None, 0, MousePosition.X, MousePosition.Y, 0));
                this.View.CalculateLineCountOnScreen();
                this.Enabled = true;
            }
        }

        /// <summary>
        /// ドキュメントの内容をファイルに保存する
        /// </summary>
        /// <param name="filepath">ファイルパス</param>
        /// <param name="newLine">改行コード</param>
        /// <param name="enc">エンコード</param>
        /// <param name="token">キャンセル用トークン</param>
        /// <returns>Taskオブジェクト</returns>
        public async Task SaveFile(string filepath, Encoding enc, string newLine, System.Threading.CancellationTokenSource token)
        {
            var fs = new System.IO.StreamWriter(filepath, false, enc);
            fs.NewLine = newLine;
            await this.Document.SaveAsync(fs, token);
            fs.Close();
        }

        /// <summary>
        /// マウスカーソルを指定します
        /// </summary>
        public override Cursor Cursor
        {
            get
            {
                return base.Cursor;
            }
            set
            {
                base.Cursor = value;
                this.VScrollBar.Cursor = DefaultCursor;
                this.HScrollBar.Cursor = DefaultCursor;
            }
        }

        private const int WS_BORDER = 0x00800000;
        private const int WS_EX_CLIENTEDGE = 0x00000200;
        /// <summary>
        /// コントロールの外観を指定します
        /// </summary>
        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                switch (this.BorderStyle)
                {
                    case BorderStyle.Fixed3D:
                        cp.ExStyle |= WS_EX_CLIENTEDGE;
                        break;
                    case BorderStyle.FixedSingle:
                        cp.Style |= WS_BORDER;
                        break;
                }
                return cp;
            }
        }

        /// <summary>
        /// コマンド キーを処理します
        /// </summary>
        /// <param name="msg">メッセージ</param>
        /// <param name="keyData">キーデータ</param>
        /// <returns>文字がコントロールによって処理された場合は true。それ以外の場合は false。 </returns>
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            const int WM_KEYDOWN = 0x100;
            if (msg.Msg != WM_KEYDOWN)
                return base.ProcessCmdKey(ref msg, keyData);
            switch (keyData)
            {
                case Keys.Control | Keys.C:
                    this.Copy();
                    break;
                case Keys.Control | Keys.V:
                    this.Paste();
                    this.Refresh();
                    break;
                case Keys.Control | Keys.X:
                    this.Cut();
                    this.Refresh();
                    break;
                case Keys.Control | Keys.Z:
                    this.Document.UndoManager.undo();
                    this.Refresh();
                    break;
                case Keys.Control | Keys.Y:
                    this.Document.UndoManager.redo();
                    this.Refresh();
                    break;
                case Keys.Control | Keys.B:
                    if (this.Controller.RectSelection)
                        this.Controller.RectSelection = false;
                    else
                        this.Controller.RectSelection = true;
                    break;
                default:
                    return base.ProcessCmdKey(ref msg,keyData);
            }
            return true;
        }

        /// <summary>
        /// インスタンスを破棄します
        /// </summary>
        /// <param name="disposing">マネージ リソースとアンマネージ リソースの両方を解放する場合は true。アンマネージ リソースだけを解放する場合は false。</param>
        protected override void Dispose(bool disposing)
        {
            SystemEvents.UserPreferenceChanged -= new UserPreferenceChangedEventHandler(this.SystemEvents_UserPreferenceChanged);
            this.render.Dispose();
            this.Timer.Dispose();
            base.Dispose(disposing);
        }

        /// <summary>
        /// 入力可能な文字かチェックします
        /// </summary>
        /// <param name="charCode">入力しようとした文字</param>
        /// <returns>可能なら真。そうでなければ偽</returns>
        protected override bool IsInputChar(char charCode)
        {
            if ((0x20 <= charCode && charCode <= 0x7e)
                || charCode == '\r'
                || charCode == '\n'
                || charCode == ' '
                || charCode == '\t'
                || charCode == '　'
                || 0x7f < charCode)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// PaddingChangedイベントを発生させます
        /// </summary>
        /// <param name="e">インベントデータ</param>
        protected override void OnPaddingChanged(EventArgs e)
        {
            base.OnPaddingChanged(e);
            this.View.Padding = new Padding(this.Padding.Left, this.Padding.Top, this.Padding.Right, this.Padding.Bottom);
        }

        /// <summary>
        /// GotFocusイベントを発生させます
        /// </summary>
        /// <param name="e">インベントデータ</param>
        protected override void OnGotFocus(EventArgs e)
        {
            base.OnGotFocus(e);
            this.View.IsFocused = true;
            this.Timer.Start();
            this.Refresh();
        }

        /// <summary>
        /// LostFocusイベントを発生させます
        /// </summary>
        /// <param name="e">インベントデータ</param>
        protected override void OnLostFocus(EventArgs e)
        {
            base.OnLostFocus(e);
            this.View.IsFocused = false;
            this.Timer.Stop();
            this.Refresh();
        }

        /// <summary>
        /// FontChangedイベントを発生させます
        /// </summary>
        /// <param name="e">インベントデータ</param>
        protected override void OnFontChanged(EventArgs e)
        {
            if (this.DesignMode)
                return;
            base.OnFontChanged(e);
            initScrollBars();
        }

        /// <summary>
        /// MouseDownイベントを発生させます
        /// </summary>
        /// <param name="e">インベントデータ</param>
        protected override void OnMouseDown(MouseEventArgs e)
        {
            TextPoint tp = this.View.GetTextPointFromPostion(e.Location);
            if (tp == TextPoint.Null)
                return;
            int index = this.View.LayoutLines.GetIndexFromTextPoint(tp);
            
            FooMouseEventArgs mouseEvent = new FooMouseEventArgs(index, e.Button, e.Clicks, e.X, e.Y, e.Delta);
            
            base.OnMouseDown(mouseEvent);
            
            if (mouseEvent.Handled)
                return;

            if (e.Button == MouseButtons.Left)
            {
                FoldingItem foldingData = this.View.HitFoldingData(e.Location.X, tp.row);
                if (foldingData != null)
                {
                    if (foldingData.Expand)
                        this.View.LayoutLines.FoldingCollection.Collapse(foldingData);
                    else
                        this.View.LayoutLines.FoldingCollection.Expand(foldingData);
                    this.Controller.JumpCaret(foldingData.Start, false);
                }
                else
                {
                    this.Controller.JumpCaret(tp.row, tp.col, false);
                }
                this.View.IsFocused = true;
                this.Focus();
                this.Refresh();
            }
        }

        /// <summary>
        /// MouseClickイベントを発生させます
        /// </summary>
        /// <param name="e">インベントデータ</param>
        protected override void OnMouseClick(MouseEventArgs e)
        {
            int index = this.GetIndexFromPostion(e.Location);

            FooMouseEventArgs mouseEvent = new FooMouseEventArgs(index, e.Button, e.Clicks, e.X, e.Y, e.Delta);

            base.OnMouseClick(mouseEvent);
        }

        /// <summary>
        /// MouseDoubleClickイベントを発生させます
        /// </summary>
        /// <param name="e">インベントデータ</param>
        protected override void OnMouseDoubleClick(MouseEventArgs e)
        {
            TextPoint tp = this.View.GetTextPointFromPostion(e.Location);
            if (tp == TextPoint.Null)
                return;
            int index = this.View.LayoutLines.GetIndexFromTextPoint(tp);

            FooMouseEventArgs mouseEvent = new FooMouseEventArgs(index, e.Button, e.Clicks, e.X, e.Y, e.Delta);
            
            base.OnMouseDoubleClick(mouseEvent);

            if (mouseEvent.Handled)
                return;

            if (e.X < this.render.TextArea.X)
                this.Document.SelectLine(index);
            else
                this.Document.SelectWord(index);
            
            this.Refresh();
        }

        /// <summary>
        /// MouseMoveイベントを発生させます
        /// </summary>
        /// <param name="e">インベントデータ</param>
        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (this.Focused == false)
                return;

            base.OnMouseMove(e);

            if (this.View.HitTextArea(e.Location.X, e.Location.Y))
            {
                TextPoint tp = this.View.GetTextPointFromPostion(e.Location);
                if (this.Controller.IsMarker(tp, HilightType.Url))
                    this.Cursor = Cursors.Hand;
                else
                    this.Cursor = Cursors.IBeam;

                if (e.Button == MouseButtons.Left)
                {
                    this.Controller.MoveCaretAndSelect(tp, ModifierKeys.HasFlag(Keys.Control));
                    this.Refresh();
                }
            }
            else
            {
                this.Cursor = Cursors.Arrow;
            }
        }

        /// <summary>
        /// MouseWheelイベントを発生させます
        /// </summary>
        /// <param name="e">インベントデータ</param>
        protected override void OnMouseWheel(MouseEventArgs e)
        {
            base.OnMouseWheel(e);

            ScrollDirection dir = e.Delta > 0 ? ScrollDirection.Up : ScrollDirection.Down;
            this.Controller.Scroll(dir, SystemInformation.MouseWheelScrollLines, false, false);
            this.Refresh();
        }

        /// <summary>
        /// Paintイベントを発生させます
        /// </summary>
        /// <param name="e">インベントデータ</param>
        protected override void OnPaint(PaintEventArgs e)
        {
            if (DesignMode)
            {
                SolidBrush brush = new SolidBrush(this.BackColor);
                e.Graphics.FillRectangle(brush, this.ClientRectangle);
                brush.Dispose();
            }else if (this.Document.FireUpdateEvent){
                this.render.DrawContent(this.View, e.ClipRectangle);
                this.Document.IsRequestRedraw = false;
            }
            base.OnPaint(e);
        }

        /// <summary>
        /// PaintBackgroundイベントを発生させます
        /// </summary>
        /// <param name="e">インベントデータ</param>
        protected override void OnPaintBackground(PaintEventArgs e)
        {
        }

        /// <summary>
        /// PreviewKeyDownイベントを発生させます
        /// </summary>
        /// <param name="e">インベントデータ</param>
        protected override void OnPreviewKeyDown(PreviewKeyDownEventArgs e)
        {
            base.OnPreviewKeyDown(e);

            switch (e.KeyCode)
            {
                case Keys.Up:
                case Keys.Down:
                case Keys.Left:
                case Keys.Right:
                case Keys.Tab:
                    e.IsInputKey = true;
                    break;
            }
        }

        /// <summary>
        /// KeyDownイベントを発生させます
        /// </summary>
        /// <param name="e">インベントデータ</param>
        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);

            var completeBox = (AutoCompleteBox)this.Document.AutoComplete;
            if (completeBox.ProcessKeyDown(this, e, e.Control, e.Shift))
            {
                e.Handled = true;
                return;
            }

            if (e.Handled)
                return;

            double alignedPage = (int)(this.render.TextArea.Height / this.render.emSize.Height) * this.render.emSize.Height;
            switch (e.KeyCode)
            {
                case Keys.Up:
                    this.Controller.MoveCaretVertical(-1, e.Shift);
                    this.Refresh();
                    break;
                case Keys.Down:
                    this.Controller.MoveCaretVertical(+1, e.Shift);
                    this.Refresh();
                    break;
                case Keys.Left:
                    this.Controller.MoveCaretHorizontical(-1, e.Shift, e.Control);
                    this.Refresh();
                    break;
                case Keys.Right:
                    this.Controller.MoveCaretHorizontical(1, e.Shift, e.Control);
                    this.Refresh();
                    break;
                case Keys.PageUp:
                    this.Controller.ScrollByPixel(ScrollDirection.Up, alignedPage,e.Shift,true);
                    this.Refresh();
                    break;
                case Keys.PageDown:
                    this.Controller.ScrollByPixel(ScrollDirection.Down, alignedPage, e.Shift, true);
                    this.Refresh();
                    break;
                case Keys.Insert:
                    if (this.InsertMode)
                        this.InsertMode = false;
                    else
                        this.InsertMode = true;
                    break;
                case Keys.Delete:
                    this.Controller.DoDeleteAction();
                    this.Refresh();
                    break;
                case Keys.Back:
                    this.Controller.DoBackSpaceAction();
                    this.Refresh();
                    break;
                case Keys.Home:
                    if (e.Control)
                        this.Controller.JumpToHead(e.Shift);
                    else
                        this.Controller.JumpToLineHead(this.Document.CaretPostion.row, e.Shift);
                    this.Refresh();
                    break;
                case Keys.End:
                    if (e.Control)
                        this.Controller.JumpToEnd(e.Shift);
                    else
                        this.Controller.JumpToLineEnd(this.Document.CaretPostion.row, e.Shift);
                    this.Refresh();
                    break;
                case Keys.Tab:
                    if (this.Controller.SelectionLength == 0)
                        this.Controller.DoInputChar('\t');
                    else if (e.Shift)
                        this.Controller.DownIndent();
                    else
                        this.Controller.UpIndent();
                    this.Refresh();
                    break;
            }
        }

        /// <summary>
        /// KeyPressイベントを発生させます
        /// </summary>
        /// <param name="e">インベントデータ</param>
        protected override void OnKeyPress(KeyPressEventArgs e)
        {
            base.OnKeyPress(e);

            if (e.Handled)
                return;

            var completeBox = (AutoCompleteBox)this.Document.AutoComplete;
            if (completeBox.ProcessKeyPress(this, e))
                return;

            switch (e.KeyChar)
            {
                case '\r':
                    this.Controller.DoEnterAction();
                    this.Refresh();
                    break;
                case '\t':
                    break;  //OnKeyDownで処理しているので不要
                default:
                    if (IsInputChar(e.KeyChar) == false)
                        break;
                    this.Controller.DoInputChar(e.KeyChar);
                    this.Refresh();
                    break;
            }
        }

        /// <summary>
        /// ClientSizeChangedイベントを発生させます
        /// </summary>
        /// <param name="e">インベントデータ</param>
        protected override void OnClientSizeChanged(EventArgs e)
        {
            if (this.DesignMode)
                return;
            base.OnClientSizeChanged(e);
            
            this.View.PageBound = new Rectangle(0,
                0,
                Math.Max(this.ClientRectangle.Width - this.VScrollBar.Width,0),
                Math.Max(this.ClientRectangle.Height - this.HScrollBar.Height, 0));

            initScrollBars();
            this.Refresh();
        }

        void View_SrcChanged(object sender, EventArgs e)
        {
            if (this.View.Src.Row > this.VScrollBar.Maximum)
                this.VScrollBar.Maximum = this.View.Src.Row + this.View.LineCountOnScreen + 1;

            int srcX = (int)Math.Abs(this.View.Src.X);
            if (srcX > this.HScrollBar.Maximum)
                this.HScrollBar.Maximum = srcX + (int)this.View.PageBound.Width + 1;

            this.HScrollBar.Value = srcX;

            this.VScrollBar.Value = this.View.Src.Row;
        }

        void FooTextBox_RightToLeftChanged(object sender, EventArgs e)
        {
            this.Document.RightToLeft = this.RightToLeft == System.Windows.Forms.RightToLeft.Yes;
        }

        void VScrollBar_Scroll(object sender, ScrollEventArgs e)
        {
            this.View.TryScroll(this.View.Src.X, e.NewValue);
            this.Refresh();
        }

        void HScrollBar_Scroll(object sender, ScrollEventArgs e)
        {
            int toX;
            if (this.RightToLeft == System.Windows.Forms.RightToLeft.Yes)
                toX = -e.NewValue;
            else
                toX = e.NewValue;
            this.View.TryScroll(toX, this.View.Src.Row);
            this.Refresh();
        }

        void Ime_StartCompstion(object sender, StartCompstionEventArgs e)
        {
            this.Ime.Font = this.Font;
            System.Drawing.Point p = this.GetPostionFromIndex(this.Controller.SelectionStart);
            float dpi;
            this.render.GetDpi(out dpi, out dpi);
            p.X = (int)(p.X * dpi / 96);
            p.Y = (int)(p.Y * dpi / 96);
            this.Ime.Location = p;
            this.View.HideCaret = true;
        }

        void Ime_EndCompstion(object sender, EndCompstionEventArgs e)
        {
            this.View.HideCaret = false;
        }

        void Ime_ImeCompstion(object sender, ImeCompstionEventArgs e)
        {
            this.Controller.DoInputString(e.InputText);
            this.Refresh();
        }

        void Ime_ImeDocumentFeed(object sender, ImeDocumentFeedEventArgs e)
        {
            TextPoint tp = this.CaretPostion;
            e.Pragraph = this.LayoutLines[tp.row];
            e.pos = tp.col;
        }

        void Ime_ImeReconvert(object sender, ImeReconvertStringEventArgs e)
        {
            if (this.RectSelection)
                return;
            if (this.Controller.SelectionLength == 0)
            {
                TextPoint tp = this.LayoutLines.GetTextPointFromIndex(this.Controller.SelectionStart);
                e.TargetString = this.LayoutLines[tp.row];
                e.AutoAdjust = true;
            }
            else
            {
                e.TargetString = this.SelectedText;
                if (e.TargetString.Length > 40)
                    e.TargetString.Remove(40);
            }
            e.CaretPostion = this.View.CaretLocation;
        }

        void Ime_ImeQueryReconvert(object sender, ImeQueryRecovertStringEventArgs e)
        {
            TextPoint tp = this.LayoutLines.GetTextPointFromIndex(this.Controller.SelectionStart);
            tp.col = e.offset;

            int index = this.View.GetIndexFromLayoutLine(tp);

            this.Select(index, index + e.length);
        }

        void Controller_CaretMoved(object sender, EventArgs e)
        {
            this.SelectionChanged(this, null);
        }

        void initScrollBars()
        {
            this.VScrollBar.SmallChange = 1;
            this.VScrollBar.LargeChange = this.View.LineCountOnScreen;
            this.VScrollBar.Maximum = this.View.LayoutLines.Count + this.View.LineCountOnScreen + 1;
            this.HScrollBar.SmallChange = 10;
            this.HScrollBar.LargeChange = (int)this.View.PageBound.Width;
            this.HScrollBar.Maximum = this.HScrollBar.LargeChange + 1;
        }

        void Timer_Tick(object sender,EventArgs e)
        {
            if (this.Document.CaretPostion.row >= this.View.LayoutLines.Count || DesignMode)
                return;

            bool updateAll = this.View.LayoutLines.HilightAll() || this.View.LayoutLines.GenerateFolding() || this.Document.IsRequestRedraw;

            if (updateAll)
                this.Invalidate();
            else
                this.Invalidate(new System.Drawing.Rectangle(this.View.CaretLocation, this.View.GetCurrentCaretRect().Size));
        }

        void SystemEvents_UserPreferenceChanged(object sender, UserPreferenceChangedEventArgs e)
        {
            this.SetSystemParamaters();
            this.Refresh();
        }

        void SetSystemParamaters()
        {
            int CaretBlinkTime = SystemInformation.CaretBlinkTime;
            if (CaretBlinkTime == -1)
            {
                this.View.CaretBlink = false;
            }
            else
            {
                this.View.CaretBlink = true;
                this.View.CaretBlinkTime = CaretBlinkTime * 2;
            }
            this.View.CaretWidthOnInsertMode = SystemInformation.CaretWidth;
        }

    }

    /// <summary>
    /// FooEditEngineで使用するマウスイベント
    /// </summary>
    public class FooMouseEventArgs : MouseEventArgs
    {
        /// <summary>
        /// イベントが発生したインデックス
        /// </summary>
        public int index;
        /// <summary>
        /// 既定の処理を省略するなら真。そうでなければ偽
        /// </summary>
        public bool Handled;
        /// <summary>
        /// コンストラクター
        /// </summary>
        /// <param name="index">インデックス</param>
        /// <param name="button">押されているボタン</param>
        /// <param name="clicks">ボタンが押された回数</param>
        /// <param name="x">マウスカーソルがあるＸ座標</param>
        /// <param name="y">マウスカーソルがあるＹ座標</param>
        /// <param name="delta">ホイールの回転方向</param>
        public FooMouseEventArgs(int index, MouseButtons button, int clicks, int x, int y, int delta)
            : base(button, clicks, x, y, delta)
        {
            this.index = index;
        }
    }

}
