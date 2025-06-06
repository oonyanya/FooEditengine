/*
 * Copyright (C) 2013 FooProject
 * * This program is free software; you can redistribute it and/or modify it under the terms of the GNU General Public License as published by
 * the Free Software Foundation; either version 3 of the License, or (at your option) any later version.

 * This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of 
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.

You should have received a copy of the GNU General Public License along with this program. If not, see <http://www.gnu.org/licenses/>.
 */

using System;
using System.IO;
using System.ComponentModel;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using System.Runtime.CompilerServices;

namespace FooEditEngine
{
    /// <summary>
    /// オートインデントを行うためのデリゲートを表す
    /// </summary>
    /// <param name="sender">イベント発生元のオブジェクト</param>
    /// <param name="e">イベントデーター</param>
    public delegate void AutoIndentHookerHandler(object sender, EventArgs e);

    /// <summary>
    /// 進行状況を表す列挙体
    /// </summary>
    public enum ProgressState
    {
        /// <summary>
        /// 操作が開始したことを表す
        /// </summary>
        Start,
        /// <summary>
        /// 操作が終了したことを表す
        /// </summary>
        Complete,
    }
    /// <summary>
    /// 進行状況を表すためのイベントデータ
    /// </summary>
    public sealed class ProgressEventArgs : EventArgs
    {
        /// <summary>
        /// 進行状況
        /// </summary>
        public ProgressState state;
        /// <summary>
        /// コンストラクター
        /// </summary>
        /// <param name="state">ProgressStateオブジェクト</param>
        public ProgressEventArgs(ProgressState state)
        {
            this.state = state;
        }
    }

    /// <summary>
    /// 進行状況を通知するためのデリゲート
    /// </summary>
    /// <param name="sender">送信元クラス</param>
    /// <param name="e">イベントデータ</param>
    public delegate void ProgressEventHandler(object sender, ProgressEventArgs e);

    /// <summary>
    /// 更新タイプを表す列挙体
    /// </summary>
    public enum UpdateType
    {
        /// <summary>
        /// ドキュメントが置き換えられたことを表す
        /// </summary>
        Replace,
        /// <summary>
        /// ドキュメント全体が削除されたことを表す
        /// </summary>
        Clear,
        /// <summary>
        /// レイアウトが再構築されたことを表す
        /// </summary>
        RebuildLayout,
        /// <summary>
        /// レイアウトの構築が必要なことを示す
        /// </summary>
        BuildLayout,
    }

    /// <summary>
    /// 更新タイプを通知するためのイベントデータ
    /// </summary>
    public sealed class DocumentUpdateEventArgs : EventArgs
    {
        /// <summary>
        /// 値が指定されていないことを示す
        /// </summary>
        public const int EmptyValue = -1;
        /// <summary>
        /// 更新タイプ
        /// </summary>
        public UpdateType type;
        /// <summary>
        /// 開始位置
        /// </summary>
        public long startIndex;
        /// <summary>
        /// 削除された長さ
        /// </summary>
        public long removeLength;
        /// <summary>
        /// 追加された長さ
        /// </summary>
        public long insertLength;
        /// <summary>
        /// 更新イベントが発生した行。行が不明な場合や行をまたぐ場合はnullを指定すること。
        /// </summary>
        public int? row;
        /// <summary>
        /// コンストラクター
        /// </summary>
        /// <param name="type">更新タイプ</param>
        /// <param name="startIndex">開始インデックス</param>
        /// <param name="removeLength">削除された長さ</param>
        /// <param name="insertLength">追加された長さ</param>
        /// <param name="row">開始行。nullを指定することができる</param>
        public DocumentUpdateEventArgs(UpdateType type, long startIndex = EmptyValue, long removeLength = EmptyValue, long insertLength = EmptyValue, int? row = null)
        {
            this.type = type;
            this.startIndex = startIndex;
            this.removeLength = removeLength;
            this.insertLength = insertLength;
            this.row = row;
        }
    }

    /// <summary>
    /// ドキュメントに更新があったことを伝えるためのデリゲート
    /// </summary>
    /// <param name="sender">送信元クラス</param>
    /// <param name="e">イベントデータ</param>
    public delegate void DocumentUpdateEventHandler(object sender, DocumentUpdateEventArgs e);

    /// <summary>
    /// ドキュメントの管理を行う
    /// </summary>
    /// <remarks>この型のすべてのメソッド・プロパティはスレッドセーフです</remarks>
    public class Document : IEnumerable<char>, IRandomEnumrator<char>, IDisposable, INotifyPropertyChanged
    {

        Regex regex;
        Match match;
        StringBuffer buffer;
        LineToIndexTable _LayoutLines;
        bool _EnableFireUpdateEvent = true,_UrlMark = false, _DrawLineNumber = false, _HideRuler = true, _RightToLeft = false;
        LineBreakMethod _LineBreak;
        int _TabStops, _LineBreakCharCount = 80;
        bool _ShowFullSpace, _ShowHalfSpace, _ShowTab, _ShowLineBreak,_InsertMode, _HideCaret, _HideLineMarker, _RectSelection;
        IndentMode _IndentMode;

        /// <summary>
        /// 一行当たりの最大文字数
        /// </summary>
        public const int MaximumLineLength = 1000;
        /// <summary>
        /// 事前読み込みを行う長さ
        /// </summary>
        /// <remarks>値を反映させるためにはレイアウト行すべてを削除する必要があります</remarks>
        public static int PreloadLength = 1024 * 1024 * 5;

        /// <summary>
        /// コンストラクター
        /// </summary>
        /// <param name="cache_size">２の以上値を指定した場合はディスクに保存します。そうでない場合はメモリーに保存します</param>
        public Document(int cache_size = -1)
            : this(null, null, cache_size)
        {
        }

        /// <summary>
        /// コンストラクター
        /// </summary>
        /// <param name="doc">ドキュメントオブジェクト</param>
        /// <param name="cache_size">２の以上値を指定した場合はディスクに保存します。そうでない場合はメモリーに保存します</param>
        /// <remarks>docが複製されますが、プロパティは引き継がれません。また、cache_sizeはdocがnullの場合だけ反映されます。</remarks>
        public Document(Document doc,string workfile_path = null,int cache_size = -1)
        {
            if (doc == null)
                this.buffer = new StringBuffer(workfile_path, cache_size);
            else
                this.buffer = new StringBuffer(doc.buffer);
            this.buffer.Update = new DocumentUpdateEventHandler(buffer_Update);
            this.Update += new DocumentUpdateEventHandler((s, e) => { });
            this.ChangeFireUpdateEvent += new EventHandler((s, e) => { });
            this.PropertyChanged += new PropertyChangedEventHandler((s, e) => { });
            this.Markers = new MarkerCollection();
            this.UndoManager = new UndoManager();
            this._LayoutLines = new LineToIndexTable(this, this.buffer.cacheSize);
            this.MarkerPatternSet = new MarkerPatternSet(this._LayoutLines, this.Markers);
            this.MarkerPatternSet.Updated += WacthDogPattern_Updated;
            this.Selections = new SelectCollection();
            this.CaretPostion = new TextPoint();
            this.HideLineMarker = true;
            this.SelectGrippers = new GripperRectangle(new Gripper(), new Gripper());
            this.SelectionChanged += new EventHandler((s, e) => { });
            this.CaretChanged += (s, e) => { };
            this.AutoIndentHook += (s, e) => { };
            this.LineBreakChanged += (s, e) => { };
            this.Dirty = false;
        }

        void WacthDogPattern_Updated(object sender, EventArgs e)
        {
            this._LayoutLines.ClearLayoutCache();
        }

        bool _Dirty;
        /// <summary>
        /// ダーティフラグ。保存されていなければ真、そうでなければ偽。
        /// </summary>
        public bool Dirty
        {
            get
            {
                return _Dirty;
            }
            set
            {
                _Dirty = value;
                this.OnProperyChanged();
            }
        }

        /// <summary>
        /// キャレットでの選択の起点となる位置
        /// </summary>
        internal long AnchorIndex
        {
            get;
            set;
        }

        /// <summary>
        /// レタリングの開始位置を表す
        /// </summary>
        internal SrcPoint Src
        {
            get;
            set;
        }

        string _Title;
        /// <summary>
        /// ドキュメントのタイトル
        /// </summary>
        public string Title
        {
            get
            {
                return _Title;
            }
            set
            {
                _Title = value;
                this.OnProperyChanged();
            }
        }

        /// <summary>
        /// 補完候補プロセッサーが切り替わったときに発生するイベント
        /// </summary>
        public event EventHandler AutoCompleteChanged;

        AutoCompleteBoxBase _AutoComplete;
        /// <summary>
        /// 補完候補プロセッサー
        /// </summary>
        public AutoCompleteBoxBase AutoComplete
        {
            get
            {
                return this._AutoComplete;
            }
            set
            {
                this._AutoComplete = value;
                if (this.AutoCompleteChanged != null)
                    this.AutoCompleteChanged(this, null);
            }
        }

        /// <summary>
        /// 読み込み中に発生するイベント
        /// </summary>
        public event ProgressEventHandler LoadProgress;

        /// <summary>
        /// ルーラーやキャレット・行番号などの表示すべきものが変化した場合に呼び出される。ドキュメントの内容が変化した通知を受け取り場合はUpdateを使用してください
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        
        public void OnProperyChanged([CallerMemberName] string  propertyName = "")
        {
            if(this.PropertyChanged != null)
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// 全角スペースを表示するかどうか
        /// </summary>
        public bool ShowFullSpace
        {
            get { return this._ShowFullSpace; }
            set
            {
                if (this._ShowFullSpace == value)
                    return;
                this._ShowFullSpace = value;
                this.OnProperyChanged();
            }
        }

        /// <summary>
        /// 半角スペースを表示するかどうか
        /// </summary>
        public bool ShowHalfSpace
        {
            get { return this._ShowHalfSpace; }
            set
            {
                if (this._ShowHalfSpace == value)
                    return;
                this._ShowHalfSpace = value;
                this.OnProperyChanged();
            }
        }

        /// <summary>
        /// TABを表示するかどうか
        /// </summary>
        public bool ShowTab
        {
            get { return this._ShowTab; }
            set
            {
                if (this._ShowTab == value)
                    return;
                this._ShowTab = value;
                this.OnProperyChanged();
            }
        }

        /// <summary>
        /// 改行を表示するかどうか
        /// </summary>
        public bool ShowLineBreak
        {
            get { return this._ShowLineBreak; }
            set
            {
                if (this._ShowLineBreak == value)
                    return;
                this._ShowLineBreak = value;
                this.OnProperyChanged();
            }
        }

        /// <summary>
        /// 選択範囲にあるグリッパーのリスト
        /// </summary>
        internal GripperRectangle SelectGrippers
        {
            private set;
            get;
        }

        /// <summary>
        /// 右から左に表示するなら真
        /// </summary>
        public bool RightToLeft {
            get { return this._RightToLeft; }
            set
            {
                if (this._RightToLeft == value)
                    return;
                this._RightToLeft = value;
                this.OnProperyChanged();
            }
        }

        /// <summary>
        /// 矩形選択モードなら真を返し、そうでない場合は偽を返す
        /// </summary>
        public bool RectSelection
        {
            get
            {
                return this._RectSelection;
            }
            set
            {
                this._RectSelection = value;
                this.OnProperyChanged();
            }
        }

        /// <summary>
        /// インデントの方法を表す
        /// </summary>
        public IndentMode IndentMode
        {
            get
            {
                return this._IndentMode;
            }
            set
            {
                this._IndentMode = value;
                this.OnProperyChanged();
            }
        }

        /// <summary>
        /// ラインマーカーを描くなら偽。そうでなければ真
        /// </summary>
        public bool HideLineMarker
        {
            get
            {
                return this._HideLineMarker;
            }
            set
            {
                this._HideLineMarker = value;
                this.OnProperyChanged();
            }
        }

        /// <summary>
        /// キャレットを描くなら偽。そうでなければ真
        /// </summary>
        public bool HideCaret
        {
            get
            {
                return this._HideCaret;
            }
            set
            {
                this._HideCaret = value;
                this.OnProperyChanged();
            }
        }

        /// <summary>
        /// 挿入モードなら真を返し、上書きモードなら偽を返す
        /// </summary>
        public bool InsertMode
        {
            get
            {
                return this._InsertMode;
            }
            set
            {
                this._InsertMode = value;
                this.OnProperyChanged();
            }
        }

        /// <summary>
        /// ルーラーを表示しないなら真、そうでないなら偽
        /// </summary>
        public bool HideRuler
        {
            get { return this._HideRuler; }
            set
            {
                if (this._HideRuler == value)
                    return;
                this._HideRuler = value;
                this.LayoutLines.ClearLayoutCache();
                this.OnProperyChanged();
            }
        }

        TextPoint _CaretPostion;
        /// <summary>
        /// レイアウト行のどこにキャレットがあるかを表す
        /// </summary>
        /// <remarks>
        /// 存在しない行を指定した場合、一番最後の行の0桁目になる。
        /// PropertyChangedイベントは発生しないので注意すること。
        /// </remarks>
        public TextPoint CaretPostion
        {
            get
            {
                return this._CaretPostion;
            }
            set
            {
                if(this._CaretPostion != value)
                {
                    if (value.row > this.LayoutLines.Count - 1)
                        this._CaretPostion = new TextPoint(this.LayoutLines.Count - 1, 0);
                    else
                        this._CaretPostion = value;
                    this.RaiseCaretPostionChanged();
                }
            }
        }

        /// <summary>
        /// ドキュメントの行数
        /// </summary>
        public int TotalLineCount
        {
            get;
            private set;
        }

        public void RaiseCaretPostionChanged()
        {
            this.CaretChanged(this, null);
        }

        /// <summary>
        /// キャレットを指定した位置に移動させる
        /// </summary>
        /// <param name="row"></param>
        /// <param name="col"></param>
        /// <param name="autoExpand">折り畳みを展開するなら真</param>
        internal void SetCaretPostionWithoutEvent(int row, int col, bool autoExpand = true)
        {
            if (autoExpand)
            {
                long lineHeadIndex = this.LayoutLines.GetLongIndexFromLineNumber(row);
                long lineLength = this.LayoutLines.GetLengthFromLineNumber(row);
                FoldingItem foldingData = this.LayoutLines.FoldingCollection.Get(lineHeadIndex, lineLength);
                if (foldingData != null)
                {
                    if (this.LayoutLines.FoldingCollection.IsParentHidden(foldingData) || !foldingData.IsFirstLine(this.LayoutLines, row))
                    {
                        this.LayoutLines.FoldingCollection.Expand(foldingData);
                    }
                }
            }
            this._CaretPostion = new TextPoint(row, col);
        }

        /// <summary>
        /// 選択範囲コレクション
        /// </summary>
        /// <remarks>PropertyChangedイベントは発生しないので注意</remarks>
        internal SelectCollection Selections
        {
            get;
            set;
        }

        /// <summary>
        /// 行番号を表示するかどうか
        /// </summary>
        public bool DrawLineNumber
        {
            get { return this._DrawLineNumber; }
            set
            {
                if (this._DrawLineNumber == value)
                    return;
                this._DrawLineNumber = value;
                this._LayoutLines.ClearLayoutCache();
                this.OnProperyChanged();
            }
        }

        /// <summary>
        /// URLをハイパーリンクとして表示するなら真。そうでないなら偽
        /// </summary>
        public bool UrlMark
        {
            get { return this._UrlMark; }
            set
            {
                if (this._UrlMark == value)
                    return;
                this._UrlMark = value;
                if (value)
                {
                    Regex regex = new Regex("(http|https|ftp)(:\\/\\/[-_.!~*\\'()a-zA-Z0-9;\\/?:\\@&=+\\$,%#]+)");
                    this.MarkerPatternSet.Add(MarkerIDs.URL, new RegexMarkerPattern(regex, HilightType.Url, new Color()));
                }
                else
                {
                    this.MarkerPatternSet.Remove(MarkerIDs.URL);
                }
                this.OnProperyChanged();
            }
        }

        /// <summary>
        /// 桁折りの方法が変わったことを表す
        /// </summary>
        public event EventHandler LineBreakChanged;

        /// <summary>
        /// 桁折り処理の方法を指定する
        /// </summary>
        /// <remarks>
        /// 変更した場合、呼び出し側で再描写とレイアウトの再構築を行う必要があります
        /// また、StatusUpdatedではなく、LineBreakChangedイベントが発生します
        /// </remarks>
        public LineBreakMethod LineBreak
        {
            get
            {
                return this._LineBreak;
            }
            set
            {
                if (this._LineBreak == value)
                    return;
                this._LineBreak = value;
                this.LineBreakChanged(this, null);
            }
        }

        /// <summary>
        /// 折り返し行う文字数。実際に折り返しが行われる幅はem単位×この値となります
        /// </summary>
        /// <remarks>この値を変えた場合、LineBreakChangedイベントが発生します</remarks>
        public int LineBreakCharCount
        {
            get
            {
                return this._LineBreakCharCount;
            }
            set
            {
                if (this._LineBreakCharCount == value)
                    return;
                this._LineBreakCharCount = value;
                this.LineBreakChanged(this, null);
            }
        }

        /// <summary>
        /// タブの幅
        /// </summary>
        /// <remarks>変更した場合、呼び出し側で再描写する必要があります</remarks>
        public int TabStops
        {
            get { return this._TabStops; }
            set {
                if (this._TabStops == value)
                    return;
                this._TabStops = value;
                this.OnProperyChanged();
            }
        }

        /// <summary>
        /// マーカーパターンセット
        /// </summary>
        /// <remarks>PropertyChangedイベントは発生しないので注意</remarks>
        public MarkerPatternSet MarkerPatternSet
        {
            get;
            private set;
        }

        /// <summary>
        /// レイアウト行を表す
        /// </summary>
        public LineToIndexTable LayoutLines
        {
            get
            {
                return this._LayoutLines;
            }
        }

        internal void FireUpdate(DocumentUpdateEventArgs e)
        {
            this.buffer_Update(this.buffer, e);
        }

        /// <summary>
        /// ドキュメントが更新された時に呼ばれるイベント
        /// </summary>
        public event DocumentUpdateEventHandler Update;

        /// <summary>
        /// FireUpdateEventの値が変わったときに呼び出されるイベント
        /// </summary>
        public event EventHandler ChangeFireUpdateEvent;

        /// <summary>
        /// 改行コードの内部表現
        /// </summary>
        public const char NewLine = '\n';

        /// <summary>
        /// EOFの内部表現
        /// </summary>
        public const char EndOfFile = '\u001a';

        /// <summary>
        /// アンドゥ管理クラスを表す
        /// </summary>
        public UndoManager UndoManager
        {
            get;
            private set;
        }

        /// <summary>
        /// 文字列の長さ
        /// </summary>
        public long Length
        {
            get
            {
                return this.buffer.Length;
            }
        }

        public long Count
        {
            get
            {
                return this.buffer.Length;
            }
        }

        /// <summary>
        /// 変更のたびにUpdateイベントを発生させるかどうか
        /// </summary>
        /// <remarks>PropertyChangedイベントは発生しないので注意</remarks>
        public bool FireUpdateEvent
        {
            get
            {
                return this._EnableFireUpdateEvent;
            }
            set
            {
                this._EnableFireUpdateEvent = value;
                this.ChangeFireUpdateEvent(this, null);
            }
        }

        /// <summary>
        /// インデクサー
        /// </summary>
        /// <param name="i">インデックス（自然数でなければならない）</param>
        /// <returns>Char型</returns>
        public char this[long i]
        {
            get
            {
                return this.buffer[i];
            }
        }

        /// <summary>
        /// マーカーコレクション
        /// </summary>
        public MarkerCollection Markers
        {
            get;
            private set;
        }

        internal StringBuffer StringBuffer
        {
            get
            {
                return this.buffer;
            }
        }

        /// <summary>
        /// 再描写を要求しているなら真
        /// </summary>
        public bool IsRequestRedraw { get; internal set; }

        /// <summary>
        /// 再描写を要求する
        /// </summary>
        public void RequestRedraw()
        {
            this.IsRequestRedraw = true;
        }

        /// <summary>
        /// レイアウト行が構築されたときに発生するイベント
        /// </summary>
        public event EventHandler PerformLayouted;
        /// <summary>
        /// レイアウト行をすべて破棄し、再度レイアウトを行う
        /// </summary>
        /// <param name="quick">真の場合、レイアウトキャッシュのみ再構築します</param>
        public void PerformLayout(bool quick = true)
        {
            if (quick)
            {
                this.LayoutLines.ClearLayoutCache();
            }
            else
            {
                this.LayoutLines.IsFrozneDirtyFlag = true;
                this.FireUpdate(new DocumentUpdateEventArgs(UpdateType.RebuildLayout, -1, -1, -1));
                this.LayoutLines.IsFrozneDirtyFlag = false;
            }
            if (this.PerformLayouted != null)
                this.PerformLayouted(this, null);
        }

        /// <summary>
        /// オードインデントが可能になった時に通知される
        /// </summary>
        /// <remarks>
        /// FireUpdateEventの影響を受けます
        /// </remarks>
        public event AutoIndentHookerHandler AutoIndentHook;

        /// <summary>
        /// 選択領域変更時に通知される
        /// </summary>
        public event EventHandler SelectionChanged;

        /// <summary>
        /// キャレット移動時に通知される
        /// </summary>
        public event EventHandler CaretChanged;

        /// <summary>
        /// 指定された範囲を選択する
        /// </summary>
        /// <param name="start"></param>
        /// <param name="length"></param>
        /// <remarks>RectSelectionの値によって動作が変わります。真の場合は矩形選択モードに、そうでない場合は行ごとに選択されます</remarks>
        public void Select(long start, long length)
        {
            if (this.FireUpdateEvent == false)
                throw new InvalidOperationException("");
            if (start < 0 || start + length < 0 || start + length > this.Length)
                throw new ArgumentOutOfRangeException("startかendが指定できる範囲を超えてます");
            if (length > Int32.MaxValue - 1)
                throw new ArgumentOutOfRangeException("length is within Int32.MaxValue - 1");
            //選択範囲が消されたとき
            foreach (Selection sel in this.Selections)
                this.LayoutLines.ClearLayoutCache(sel.start, sel.length);
            this.Selections.Clear();
            if (length < 0)
            {
                long oldStart = start;
                start += length;
                length = oldStart - start;
            }
            if (this.RectSelection && length != 0)
            {
                TextPoint startTextPoint = this.LayoutLines.GetTextPointFromIndex(start);
                TextPoint endTextPoint = this.LayoutLines.GetTextPointFromIndex(start + length);
                this.SelectByRectangle(new TextRectangle(startTextPoint, endTextPoint));
                this.LayoutLines.ClearLayoutCache(start, length);
            }
            else if (length != 0)
            {
                this.Selections.Add(Selection.Create(start, length));
                this.LayoutLines.ClearLayoutCache(start, length);
            }
            this.SelectionChanged(this, null);
        }

        /// <summary>
        /// 矩形選択を行う
        /// </summary>
        /// <param name="tp">開始位置</param>
        /// <param name="width">桁数</param>
        /// <param name="height">行数</param>
        public void Select(TextPoint tp, long width, long height)
        {
            if (this.FireUpdateEvent == false || !this.RectSelection)
                throw new InvalidOperationException("");
            if (width > Int32.MaxValue - 1)
                throw new ArgumentOutOfRangeException("width is within Int32.MaxValue - 1");
            if (height > Int32.MaxValue - 1)
                throw new ArgumentOutOfRangeException("height is within Int32.MaxValue - 1");
            TextPoint end = tp;

            end.row = tp.row + (int)height;
            end.col = tp.col + (int)width;

            if (end.row > this.LayoutLines.Count - 1)
                throw new ArgumentOutOfRangeException("");

            this.Selections.Clear();

            this.SelectByRectangle(new TextRectangle(tp, end));

            this.SelectionChanged(this, null);
        }

        private void SelectByRectangle(TextRectangle rect)
        {
            if (this.FireUpdateEvent == false)
                throw new InvalidOperationException("");
            if (rect.TopLeft <= rect.BottomRight)
            {
                for (int i = rect.TopLeft.row; i <= rect.BottomLeft.row; i++)
                {
                    int length = this.LayoutLines.GetLengthFromLineNumber(i);
                    int leftCol = rect.TopLeft.col, rightCol = rect.TopRight.col, lastCol = length;
                    if (length > 0 && this.LayoutLines[i][length - 1] == Document.NewLine)
                        lastCol = length - 1;
                    if (lastCol < 0)
                        lastCol = 0;
                    if (rect.TopLeft.col > lastCol)
                        leftCol = lastCol;
                    if (rect.TopRight.col > lastCol)
                        rightCol = lastCol;

                    long StartIndex = this.LayoutLines.GetLongIndexFromTextPoint(new TextPoint(i, leftCol));
                    long EndIndex = this.LayoutLines.GetLongIndexFromTextPoint(new TextPoint(i, rightCol));

                    Selection sel;
                    sel = Selection.Create(StartIndex, EndIndex - StartIndex);

                    this.Selections.Add(sel);
                }
            }
        }

        /// <summary>
        /// 単語単位で選択する
        /// </summary>
        /// <param name="index">探索を開始するインデックス</param>
        /// <param name="changeAnchor">選択の起点となるとインデックスを変更するなら真。そうでなければ偽</param>
        public void SelectWord(long index, bool changeAnchor = false)
        {
            this.SelectSepartor(index, (c) => Util.IsWordSeparator(c), changeAnchor);
        }

        /// <summary>
        /// 行単位で選択する
        /// </summary>
        /// <param name="index">探索を開始するインデックス</param>
        /// <param name="changeAnchor">選択の起点となるとインデックスを変更するなら真。そうでなければ偽</param>
        public void SelectLine(long index,bool changeAnchor = false)
        {
            this.SelectSepartor(index, (c) => c == Document.NewLine, changeAnchor);
        }

        /// <summary>
        /// セパレーターで区切られた領域を取得する
        /// </summary>
        /// <param name="index">探索を開始するインデックス</param>
        /// <param name="find_sep_func">セパレーターなら真を返し、そうでないなら偽を返す</param>
        /// <returns>開始インデックス、終了インデックス</returns>
        public Tuple<long, long> GetSepartor(long index, Func<char, bool> find_sep_func)
        {
            if (find_sep_func == null)
                throw new ArgumentNullException("find_sep_func must not be null");

            if (this.Length <= 0 || index >= this.Length)
                return null;

            Document str = this;

            long start = index;
            while (start > 0 && !find_sep_func(str[start]))
                start--;

            if (find_sep_func(str[start]))
            {
                start++;
            }

            long end = index;
            while (end < this.Length && !find_sep_func(str[end]))
                end++;

            return new Tuple<long, long>(start, end);
        }

        /// <summary>
        /// セパレーターで囲まれた範囲内を選択する
        /// </summary>
        /// <param name="index">探索を開始するインデックス</param>
        /// <param name="find_sep_func">セパレーターなら真を返し、そうでないなら偽を返す</param>
        /// <param name="changeAnchor">選択の起点となるとインデックスを変更するなら真。そうでなければ偽</param>
        public void SelectSepartor(long index,Func<char,bool> find_sep_func, bool changeAnchor = false)
        {
            if (this.FireUpdateEvent == false)
                throw new InvalidOperationException("");

            if (find_sep_func == null)
                throw new ArgumentNullException("find_sep_func must not be null");

            var t = this.GetSepartor(index, find_sep_func);
            if (t == null)
                return;

            long start = t.Item1, end = t.Item2;

            this.Select(start, end - start);

            if (changeAnchor)
                this.AnchorIndex = start;
        }

        /// <summary>
        /// DocumentReaderを作成します
        /// </summary>
        /// <returns>DocumentReaderオブジェクト</returns>
        public DocumentReader CreateReader()
        {
            return new DocumentReader(this.buffer);
        }

        /// <summary>
        /// マーカーを設定する
        /// </summary>
        /// <param name="id">マーカーID</param>
        /// <param name="m">設定したいマーカー</param>
        public void SetMarker(int id,Marker m)
        {
            if (m.start < 0 || m.start + m.length > this.Length)
                throw new ArgumentOutOfRangeException("startもしくはendが指定できる範囲を超えています");
            if (m.length > Int32.MaxValue - 1)
                throw new ArgumentOutOfRangeException("Length is within Int32.MaxValue - 1");

            this.Markers.Add(id,m);
        }

        /// <summary>
        /// マーカーを削除する
        /// </summary>
        /// <param name="id">マーカーID</param>
        /// <param name="start">開始インデックス</param>
        /// <param name="length">削除する長さ</param>
        public void RemoveMarker(int id, long start, long length)
        {
            if (start < 0 || start + length > this.Length)
                throw new ArgumentOutOfRangeException("startもしくはendが指定できる範囲を超えています");
            if (length > Int32.MaxValue - 1)
                throw new ArgumentOutOfRangeException("Length is within Int32.MaxValue - 1");

            this.Markers.RemoveAll(id,start, length);
        }

        /// <summary>
        /// マーカーを削除する
        /// </summary>
        /// <param name="id">マーカーID</param>
        /// <param name="type">削除したいマーカーのタイプ</param>
        public void RemoveMarker(int id, HilightType type)
        {
            this.Markers.RemoveAll(id,type);
        }

        /// <summary>
        /// すべてのマーカーを削除する
        /// </summary>
        /// <param name="id">マーカーID</param>
        public void RemoveAllMarker(int id)
        {
            this.Markers.RemoveAll(id);
        }

        /// <summary>
        /// インデックスに対応するマーカーを得る
        /// </summary>
        /// <param name="id">マーカーID</param>
        /// <param name="index">インデックス</param>
        /// <returns>Marker構造体の列挙子</returns>
        public IEnumerable<Marker> GetMarkers(int id, long index)
        {
            if (index < 0 || index > this.Length)
                throw new ArgumentOutOfRangeException("indexが範囲を超えています");
            return this.Markers.Get(id,index);
        }

        /// <summary>
        /// 部分文字列を取得する
        /// </summary>
        /// <param name="index">開始インデックス</param>
        /// <param name="length">長さ</param>
        /// <returns>Stringオブジェクト</returns>
        public string ToString(long index, long length)
        {
            using(this.buffer.GetReaderLock())
            {
                return this.buffer.ToString(index, length);
            }
        }

        /// <summary>
        /// インデックスを開始位置とする文字列を返す
        /// </summary>
        /// <param name="index">開始インデックス</param>
        /// <returns>Stringオブジェクト</returns>
        public string ToString(long index)
        {
            return this.ToString(index, this.buffer.Length - index);
        }

        /// <summary>
        /// 行を取得する
        /// </summary>
        /// <param name="startIndex">開始インデックス</param>
        /// <param name="endIndex">終了インデックス</param>
        /// <param name="maxCharCount">最大長</param>
        /// <returns>行イテレーターが返される</returns>
        public IEnumerable<string> GetLines(long startIndex, long endIndex, int maxCharCount = -1)
        {
            foreach (Tuple<long, long> range in this.LayoutLines.ForEachLines(startIndex, endIndex, maxCharCount))
            {
                StringBuilder temp = new StringBuilder();
                temp.Clear();
                long lineEndIndex = range.Item1;
                if (range.Item2 > 0)
                    lineEndIndex += range.Item2 - 1;
                for (long i = range.Item1; i <= lineEndIndex; i++)
                    temp.Append(this.buffer[i]);
                yield return temp.ToString();
            }
        }

        /// <summary>
        /// 文字列を追加する
        /// </summary>
        /// <param name="s">追加したい文字列</param>
        /// <remarks>非同期操作中はこのメソッドを実行することはできません</remarks>
        public void Append(string s)
        {
            this.Replace(this.buffer.Length, 0, s);
        }

        /// <summary>
        /// 文字列を挿入する
        /// </summary>
        /// <param name="index">開始インデックス</param>
        /// <param name="s">追加したい文字列</param>
        /// <remarks>読み出し操作中はこのメソッドを実行することはできません</remarks>
        public void Insert(long index, string s)
        {
            this.Replace(index, 0, s);
        }

        /// <summary>
        /// 文字列を削除する
        /// </summary>
        /// <param name="index">開始インデックス</param>
        /// <param name="length">長さ</param>
        /// <remarks>読み出し操作中はこのメソッドを実行することはできません</remarks>
        public void Remove(long index, long length)
        {
            this.Replace(index, length, "");
        }

        /// <summary>
        /// ドキュメントを置き換える
        /// </summary>
        /// <param name="index">開始インデックス</param>
        /// <param name="length">長さ</param>
        /// <param name="s">文字列</param>
        /// <param name="UserInput">ユーザーからの入力として扱うなら真</param>
        /// <remarks>読み出し操作中はこのメソッドを実行することはできません</remarks>
        public void Replace(long index, long length, string s, bool UserInput = false)
        {
            if (index < 0 || index > this.buffer.Length || index + length > this.buffer.Length || length < 0)
                throw new ArgumentOutOfRangeException();
            if (length == 0 && (s == string.Empty || s == null))
                return;

            foreach(int id in this.Markers.IDs)
                this.RemoveMarker(id,index, length);

            ReplaceCommand cmd = new ReplaceCommand(this.buffer, index, length, s);
            this.UndoManager.push(cmd);
            cmd.redo();

            if (this.FireUpdateEvent && UserInput)
            {
                var input_str = string.Empty;
                if (s == Document.NewLine.ToString())
                    input_str = s;
                else if (s == string.Empty && length > 0)
                    input_str = "\b";
                //入力は終わっているので空文字を渡すが処理の都合で一部文字だけはそのまま渡す
                if (this.AutoComplete != null)
                    this.AutoComplete.ParseInput(input_str);
                if (s == Document.NewLine.ToString())
                    this.AutoIndentHook(this, null);
            }
        }

        /// <summary>
        /// 物理行をすべて削除する
        /// </summary>
        /// <remarks>Dirtyフラグも同時にクリアーされます</remarks>
        /// <remarks>非同期操作中はこのメソッドを実行することはできません</remarks>
        public void Clear()
        {
            this.buffer.Clear();
            this.buffer.OnDocumentUpdate(new DocumentUpdateEventArgs(UpdateType.Clear, 0, 0, 0));
            this.Dirty = false;
        }

        /// <summary>
        /// ストリームからドキュメントを非同期的に構築します
        /// </summary>
        /// <param name="fs">IStreamReaderオブジェクト</param>
        /// <param name="tokenSource">キャンセルトークン</param>
        /// <param name="file_size">ファイルサイズ。-1を指定しても動作しますが、読み取りが遅くなります</param>
        /// <returns>Taskオブジェクト</returns>
        /// <remarks>
        /// 読み取り操作は別スレッドで行われます。
        /// また、非同期操作中はこのメソッドを実行することはできません。
        /// </remarks>
        public async Task LoadAsync(TextReader fs, CancellationTokenSource tokenSource = null, long file_size = -1)
        {
            if (fs.Peek() == -1)
                return;

            if (this.LoadProgress != null)
                this.LoadProgress(this, new ProgressEventArgs(ProgressState.Start));

            try
            {
                //UIスレッドのやつを呼ぶ可能性がある
                await LoadAsyncCore(fs, tokenSource);
            }
            finally
            {
                this.PerformLayout(false);
                if (this.LoadProgress != null)
                    this.LoadProgress(this, new ProgressEventArgs(ProgressState.Complete));
            }
        }

        async Task LoadAsyncCore(TextReader fs, CancellationTokenSource tokenSource = null, long file_size = -1)
        {
            this.Clear();
            if (file_size > 0)
                this.buffer.Allocate(file_size);
            char[] str = new char[1024 * 1024];
            int readCount;
            int totalLineCount = 0;
            do
            {
                readCount = await fs.ReadAsync(str, 0, str.Length).ConfigureAwait(false);

                //内部形式に変換する
                var internal_str = str.Where((s) =>
                {
                    if (s == '\n')
                        totalLineCount++;
                    return s != '\r' && s != '\0';
                });

                using (await this.buffer.GetWriterLockAsync())
                {
                    //str.lengthは事前に確保しておくために使用するので影響はない
                    this.buffer.AddRange(internal_str);
                }

                if (tokenSource != null)
                    tokenSource.Token.ThrowIfCancellationRequested();
#if TEST_ASYNC
                DebugLog.WriteLine("waiting now");
                await Task.Delay(100).ConfigureAwait(false);
#endif
                Array.Clear(str, 0, str.Length);
            } while (readCount > 0);

            this.TotalLineCount = totalLineCount;
        }

        /// <summary>
        /// ストリームに非同期モードで保存します
        /// </summary>
        /// <param name="fs">IStreamWriterオブジェクト</param>
        /// <param name="tokenSource">キャンセルトークン</param>
        /// <returns>Taskオブジェクト</returns>
        /// <remarks>非同期操作中はこのメソッドを実行することはできません</remarks>
        public async Task SaveAsync(TextWriter fs, CancellationTokenSource tokenSource = null)
        {
            await this.SaveAsyncCore(fs, tokenSource);
        }

        async Task SaveAsyncCore(TextWriter fs, CancellationTokenSource tokenSource = null)
        {
            using (await this.buffer.GetReaderLockAsync())
            {
                StringBuilder line = new StringBuilder();
                for (int i = 0; i < this.Length; i++)
                {
                    char c = this[i];
                    line.Append(c);
                    if (c == Document.NewLine || i == this.Length - 1)
                    {
                        string str = line.ToString();
                        str = str.Replace(Document.NewLine.ToString(), fs.NewLine);
                        await fs.WriteAsync(str).ConfigureAwait(false);
                        line.Clear();
                        if (tokenSource != null)
                            tokenSource.Token.ThrowIfCancellationRequested();
#if TEST_ASYNC
                        System.Threading.Thread.Sleep(10);
#endif
                    }
                }
            }
        }

        /// <summary>
        /// Find()およびReplaceAll()で使用するパラメーターをセットします
        /// </summary>
        /// <param name="pattern">検索したい文字列</param>
        /// <param name="UseRegex">正規表現を使用するなら真</param>
        /// <param name="opt">RegexOptions列挙体</param>
        public void SetFindParam(string pattern, bool UseRegex, RegexOptions opt)
        {
            this.match = null;
            if (UseRegex)
                this.regex = new Regex(pattern, opt);
            else
                this.regex = new Regex(Regex.Escape(pattern), opt);
        }

        /// <summary>
        /// 現在の検索パラメーターでWatchDogを生成する
        /// </summary>
        /// <param name="type">ハイライトタイプ</param>
        /// <param name="color">色</param>
        /// <returns>WatchDogオブジェクト</returns>
        public RegexMarkerPattern CreateWatchDogByFindParam(HilightType type,Color color)
        {
            if (this.regex == null)
                throw new InvalidOperationException("SetFindParam()を呼び出してください");
            return new RegexMarkerPattern(this.regex,type,color);
        }

        /// <summary>
        /// 指定した文字列を検索します
        /// </summary>
        /// <returns>見つかった場合はSearchResult列挙子を返却します</returns>
        /// <remarks>見つかったパターン以外を置き換えた場合、正常に動作しないことがあります</remarks>
        public IEnumerator<SearchResult> Find()
        {
            return this.Find(0, this.Length);
        }

        /// <summary>
        /// 指定した文字列を検索します
        /// </summary>
        /// <returns>見つかった場合はSearchResult列挙子を返却します</returns>
        /// <param name="start">開始インデックス</param>
        /// <param name="length">検索する長さ</param>
        /// <remarks>見つかったパターン以外を置き換えた場合、正常に動作しないことがあります</remarks>
        public IEnumerator<SearchResult> Find(long start, long length)
        {
            if (this.regex == null)
                throw new InvalidOperationException();
            if (start < 0 || start >= this.Length)
                throw new ArgumentOutOfRangeException();

            long end = start + length - 1;

            if(end > this.Length - 1)
                throw new ArgumentOutOfRangeException();

            StringBuilder line = new StringBuilder();
            long oldLength = this.Length;
            for (long i = start; i <= end; i++)
            {
                char c = this[i];
                line.Append(c);
                if (c == Document.NewLine || i == end)
                {
                    this.match = this.regex.Match(line.ToString());
                    while (this.match.Success)
                    {
                        long startIndex = i - line.Length + 1 + this.match.Index;
                        long endIndex = startIndex + this.match.Length - 1;

                        yield return new SearchResult(this.match, startIndex, endIndex);

                        if (this.Length != oldLength)   //長さが変わった場合は置き換え後のパターンの終点＋１まで戻る
                        {
                            long delta = this.Length - oldLength;
                            i = endIndex + delta;
                            end = end + delta;
                            oldLength = this.Length;
                            break;
                        }

                        this.match = this.match.NextMatch();
                    }
                    line.Clear();
                }
            }
        }

        /// <summary>
        /// 任意のパターンですべて置き換えます
        /// </summary>
        /// <param name="replacePattern">置き換え後のパターン</param>
        /// <param name="groupReplace">グループ置き換えを行うなら真。そうでないなら偽</param>
        public void ReplaceAll(string replacePattern,bool groupReplace)
        {
            if (this.regex == null)
                throw new InvalidOperationException();
            ReplaceAllCommand cmd = new ReplaceAllCommand(this.buffer, this.LayoutLines, this.regex, replacePattern, groupReplace);
            this.UndoManager.push(cmd);
            cmd.redo();
        }

        /// <summary>
        /// 任意のパターンで置き換える
        /// </summary>
        /// <param name="target">対象となる文字列</param>
        /// <param name="pattern">置き換え後の文字列</param>
        /// <param name="ci">大文字も文字を区別しないなら真。そうでないなら偽</param>
        /// <remarks>
        /// 検索時に大文字小文字を区別します。また、このメソッドでは正規表現を使用することはできません
        /// </remarks>
        public void ReplaceAll2(string target, string pattern,bool ci = false)
        {
            FastReplaceAllCommand cmd = new FastReplaceAllCommand(this.buffer, this.LayoutLines, target, pattern,ci);
            this.UndoManager.push(cmd);
            cmd.redo();
        }

        #region IEnumerable<char> メンバー

        /// <summary>
        /// 列挙子を返します
        /// </summary>
        /// <returns>IEnumeratorオブジェクトを返す</returns>
        public IEnumerator<char> GetEnumerator()
        {
            return this.buffer.GetEnumerator();
        }

        #endregion

        #region IEnumerable メンバー

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }

        #endregion
        void buffer_Update(object sender, DocumentUpdateEventArgs e)
        {
            switch (e.type)
            {
                case UpdateType.RebuildLayout:
                    {
                        this._LayoutLines.Clear();
                        long analyzeLength = PreloadLength;
                        if (analyzeLength > this.Length)
                            analyzeLength = this.Length;

                        this._LayoutLines.UpdateLayoutLine(0, 0, analyzeLength);
                        long fetchedLength = this._LayoutLines.FetchLineWithoutEvent(CaretPostion.row);

                        int totalLineCount = this._LayoutLines.Count - 1;
                        foreach(var c in this.buffer.GetEnumerator(analyzeLength, this.Length - analyzeLength - fetchedLength))
                        {
                            if (c == Document.NewLine)
                                totalLineCount++;
                        };
                        this.TotalLineCount = totalLineCount;

                        break;
                    }
                case UpdateType.BuildLayout:
                    {
                        break;
                    }
                case UpdateType.Replace:
                    if (e.row == null)
                    {
                        var updateLineCount = this._LayoutLines.UpdateLayoutLine(e.startIndex, e.removeLength, e.insertLength);
                        this.Markers.UpdateMarkers(e.startIndex, e.insertLength, e.removeLength);
                        this.TotalLineCount += updateLineCount;
                    }
                    else
                    {
                        this._LayoutLines.UpdateLineAsReplace(e.row.Value, e.removeLength, e.insertLength);
                        this.Markers.UpdateMarkers(this.LayoutLines.GetLongIndexFromLineNumber(e.row.Value), e.insertLength, e.removeLength);
                    }
                    this.Dirty = true;
                    break;
                case UpdateType.Clear:
                    this.TotalLineCount = 0;
                    this._LayoutLines.Clear();
                    this.Dirty = true;
                    break;
            }
            if(this.FireUpdateEvent)
                this.Update(this, e);
        }

        internal void Trim()
        {
            this.buffer.Flush();
        }

        #region IDisposable Support
        private bool disposedValue = false; // 重複する呼び出しを検出するには

        void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    this.buffer.Clear();
                    this.LayoutLines.Clear();
                    this.buffer.Dispose();
                }

                disposedValue = true;
            }
        }

        /// <summary>
        /// ドキュメントを破棄する
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
        }
        #endregion
    }

    /// <summary>
    /// 検索結果を表す
    /// </summary>
    public class SearchResult
    {
        private Match Match;

        /// <summary>
        /// 一致した場所の開始位置を表す
        /// </summary>
        public long Start;

        /// <summary>
        /// 一致した場所の終了位置を表す
        /// </summary>
        public long End;

        /// <summary>
        /// 見つかった文字列を返す
        /// </summary>
        public string Value
        {
            get { return this.Match.Value; }
        }

        /// <summary>
        /// 指定したパターンを置き換えて返す
        /// </summary>
        /// <param name="replacement">置き換える文字列</param>
        /// <returns>置き換え後の文字列</returns>
        public string Result(string replacement)
        {
            return this.Match.Result(replacement);
        }

        /// <summary>
        /// コンストラクター
        /// </summary>
        /// <param name="m">Matchオブジェクト</param>
        /// <param name="start">開始インデックス</param>
        /// <param name="end">終了インデックス</param>
        public SearchResult(Match m, long start, long end)
        {
            this.Match = m;
            this.Start = start;
            this.End = end;
        }
    }

    /// <summary>
    /// ドキュメントリーダー
    /// </summary>
    public class DocumentReader : TextReader
    {
        StringBuffer document;      
        int currentIndex;

        /// <summary>
        /// コンストラクター
        /// </summary>
        /// <param name="doc"></param>
        internal DocumentReader(StringBuffer doc)
        {
            if (doc == null)
                throw new ArgumentNullException();
            this.document = doc;
        }

        /// <summary>
        /// 文字を取得する
        /// </summary>
        /// <returns>文字。取得できない場合は-1</returns>
        public override int Peek()
        {
            if (this.document == null)
                throw new InvalidOperationException();
            if (this.currentIndex >= this.document.Length)
                return -1;
            return this.document[this.currentIndex];
        }

        /// <summary>
        /// 文字を取得し、イテレーターを一つ進める
        /// </summary>
        /// <returns>文字。取得できない場合は-1</returns>
        public override int Read()
        {
            int c = this.Peek();
            if(c != -1)
                this.currentIndex++;
            return c;
        }

        /// <summary>
        /// 文字列を読み取りバッファーに書き込む
        /// </summary>
        /// <param name="buffer">バッファー</param>
        /// <param name="index">開始インデックス</param>
        /// <param name="count">カウント</param>
        /// <returns>読み取られた文字数</returns>
        public override int Read(char[] buffer, int index, int count)
        {
            if (this.document == null)
                throw new InvalidOperationException();

            if (buffer == null)
                throw new ArgumentNullException();

            if (this.document.Length < count)
                throw new ArgumentException();

            if (index < 0 || count < 0)
                throw new ArgumentOutOfRangeException();

            if (this.document.Length == 0)
                return 0;

            long actualCount = count;
            if (index + count - 1 > this.document.Length - 1)
                actualCount = this.document.Length - index;

            string str = this.document.ToString(index, actualCount);

            for (int i = 0; i < str.Length; i++)    //ToCharArray()だと戻った時に消えてしまう
                buffer[i] = str[i];

            this.currentIndex = (int)(index + actualCount);
            
            return (int)actualCount;
        }

        /// <summary>
        /// オブジェクトを破棄する
        /// </summary>
        /// <param name="disposing">真ならアンマネージドリソースを解放する</param>
        protected override void Dispose(bool disposing)
        {
            this.document = null;
        }

    }
}
