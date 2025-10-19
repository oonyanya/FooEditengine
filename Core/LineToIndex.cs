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
using System.Globalization;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using FooProject.Collection;
using FooProject.Collection.DataStore;
using System.IO;

namespace FooEditEngine
{
    internal interface ITextLayout : IDisposable
    {
        /// <summary>
        /// 文字列の幅
        /// </summary>
        double Width
        {
            get;
        }

        /// <summary>
        /// 文字列の高さ
        /// </summary>
        double Height
        {
            get;
        }

        /// <summary>
        /// Disposeされているなら真を返す
        /// </summary>
        bool Disposed
        {
            get;
        }

        /// <summary>
        /// 破棄すべきなら真。そうでなければ偽
        /// </summary>
        bool Invaild
        {
            get;
        }

        /// <summary>
        /// 桁方向の座標に対応するインデックスを得る
        /// </summary>
        /// <param name="colpos">桁方向の座標</param>
        /// <returns>インデックス</returns>
        /// <remarks>行番号の幅は考慮されてないのでView以外のクラスは呼び出さないでください</remarks>
        int GetIndexFromColPostion(double colpos);

        /// <summary>
        /// インデックスに対応する文字の幅を得る
        /// </summary>
        /// <param name="index">インデックス</param>
        /// <returns>文字の幅</returns>
        double GetWidthFromIndex(int index);

        /// <summary>
        /// インデックスに対応する桁方向の座標を得る
        /// </summary>
        /// <param name="index">インデックス</param>
        /// <returns>桁方向の座標</returns>
        /// <remarks>行頭にEOFが含まれている場合、0が返ります</remarks>
        double GetColPostionFromIndex(int index);

        /// <summary>
        /// 座標に対応するインデックスを取得する
        /// </summary>
        /// <param name="x">桁方向の座標</param>
        /// <param name="y">行方向の座標</param>
        /// <returns>インデックス</returns>
        /// <remarks>行番号の幅は考慮されてないのでView以外のクラスは呼び出さないでください</remarks>
        int GetIndexFromPostion(double x, double y);

        /// <summary>
        /// インデックスに対応する座標を得る
        /// </summary>
        /// <param name="index">インデックス</param>
        /// <returns>行方向と桁方向の相対座標</returns>
        /// <remarks>行頭にEOFが含まれている場合、0が返ります</remarks>
        Point GetPostionFromIndex(int index);

        /// <summary>
        /// 適切な位置にインデックスを調整する
        /// </summary>
        /// <param name="index">インデックス</param>
        /// <param name="flow">真の場合は隣接するクラスターを指すように調整し、
        /// そうでない場合は対応するクラスターの先頭を指すように調整します</param>
        /// <returns>調整後のインデックス</returns>
        int AlignIndexToNearestCluster(int index, AlignDirection flow);
    }

    internal class SpilitStringEventArgs : EventArgs
    {
        public Document buffer;
        public long index;
        public long length;
        public long row;
        public SpilitStringEventArgs(Document buf, long index, long length, long row)
        {
            this.buffer = buf;
            this.index = index;
            this.length = length;
            this.row = row;
        }
    }

    internal struct SyntaxInfo : FooProject.Collection.IRange
    {
        public TokenType type;
        public long index;
        public long start { get { return index; } set { index = value; } }
        public long length { get; set; }
        public SyntaxInfo(long index, long length, TokenType type)
        {
            this.type = type;
            this.index = index;
            this.length = length;
        }

        public FooProject.Collection.IRange DeepCopy()
        {
            var newItem = new SyntaxInfo();
            newItem.type = this.type;
            newItem.index = this.index;
            newItem.length = this.length;
            newItem.type = this.type;
            return newItem;
        }
    }

    internal enum EncloserType
    {
        None = 0,
        Begin = 1,
        Now = 2,
        End = 3,
    }

    interface ILineInfoGenerator
    {
        void Update(Document doc, long startIndex, long insertLength, long removeLength);
        void Clear(LineToIndexTable lti);
        bool Generate(Document doc, LineToIndexTable lti, bool force = true);
    }

    public class LineToIndexTableData : IDisposable, FooProject.Collection.IRange
    {
        public long start { get; set; }

        public long length { get; set; }

        /// <summary>
        /// マーカーの開始位置。-1を設定した場合、そのマーカーはレタリングされません。正しい先頭位置を取得するにはGetLineHeadIndex()を使用してください
        /// </summary>
        public long Index { get { return this.start; } set { this.start = value; } }

        public long Length
        {
            get { return this.length; }
            set { this.length = Length; }
        }

        /// <summary>
        /// キャッシュ済みの行文字列を取得する
        /// </summary>
        public string LineString { get; internal set; }

        internal SyntaxInfo[] Syntax;
        internal EncloserType EncloserType;
        internal ITextLayout Layout;
        public bool Dirty = false;

        /// <summary>
        /// コンストラクター。LineToIndexTable以外のクラスで呼び出さないでください
        /// </summary>
        internal LineToIndexTableData()
        {
        }

        /// <summary>
        /// コンストラクター。LineToIndexTable以外のクラスで呼び出さないでください
        /// </summary>
        internal LineToIndexTableData(long index, long length, bool lineend,bool dirty, SyntaxInfo[] syntax)
        {
            this.start = index;
            this.length = length;
            this.Syntax = syntax;
            this.EncloserType = EncloserType.None;
            this.Dirty = dirty;
        }

        public void Dispose()
        {
            this.LineString = null;
            if(this.Layout != null)
            {
                this.Layout.Dispose();
            }
        }

        public FooProject.Collection.IRange DeepCopy()
        {
            var result = new LineToIndexTableData();
            result.start = this.start;
            result.length = this.length;
            result.Syntax = this.Syntax;
            result.Layout = this.Layout;
            result.Dirty = this.Dirty;
            result.EncloserType = this.EncloserType;
            return result;
        }
    }

    class LineToIndexTableDataSerializer : ISerializeData<IComposableList<LineToIndexTableData>>
    {
        public IComposableList<LineToIndexTableData> DeSerialize(byte[] inputData)
        {
            var memStream = new MemoryStream(inputData);
            var reader = new BinaryReader(memStream);
            var arrayCount = reader.ReadInt32();
            var maxcapacity = reader.ReadInt32();
            var array = new FixedRangeList<LineToIndexTableData>(arrayCount, maxcapacity);
            for(int i = 0; i < arrayCount; i++)
            {
                var item = new LineToIndexTableData();
                item.start = reader.ReadInt64();
                item.length = reader.ReadInt64();
                item.Dirty = reader.ReadBoolean();
                item.EncloserType = (EncloserType)reader.ReadInt64();
                var syntax_item_count = reader.ReadInt64();
                if (syntax_item_count > 0)
                {
                    var syntax_items = new SyntaxInfo[syntax_item_count];
                    for (int j = 0; j < syntax_item_count; j++)
                    {
                        var info = new SyntaxInfo();
                        info.type = (TokenType)reader.ReadInt64();
                        info.start = reader.ReadInt64();
                        info.length = reader.ReadInt64();
                        syntax_items[j] = info;
                    }
                    item.Syntax = syntax_items;
                }
                else
                {
                    item.Syntax = null;
                }
                array.Add(item);
            }
            //FixedRangeListを返さないとうまく動作しない
            System.Diagnostics.Debug.Assert(array is FixedRangeList<LineToIndexTableData>);
            return array;
        }

        public byte[] Serialize(IComposableList<LineToIndexTableData> data)
        {
            FixedRangeList<LineToIndexTableData> list = (FixedRangeList<LineToIndexTableData>)data;
            //内部配列の確保に時間がかかるので、書き込むメンバー数×バイト数の2倍程度をひとまず確保しておく
            var memStream = new MemoryStream(data.Count * 5 * 8 * 2);
            var writer = new BinaryWriter(memStream, Encoding.Unicode);
            //面倒なのでlongにキャストできるところはlongで書き出す
            writer.Write(list.Count);
            writer.Write(list.MaxCapacity);
            foreach(var item in list)
            {
                writer.Write(item.start);
                writer.Write(item.length);
                writer.Write(item.Dirty);
                writer.Write((long)item.EncloserType);
                if(item.Syntax == null)
                {
                    writer.Write(0L);
                }
                else
                {
                    writer.Write((long)item.Syntax.Length);
                    foreach (var s in item.Syntax)
                    {
                        writer.Write((long)s.type);
                        writer.Write(s.start);
                        writer.Write(s.length);
                    }
                }
            }
            writer.Close();
            var result = memStream.ToArray();
            memStream.Dispose();
            return result;
        }
    }

    internal delegate IList<LineToIndexTableData> SpilitStringEventHandler(object sender, SpilitStringEventArgs e);

    internal sealed class CreateLayoutEventArgs
    {
        /// <summary>
        /// 開始インデックス
        /// </summary>
        public long Index
        {
            get;
            private set;
        }
        /// <summary>
        /// 長さ
        /// </summary>
        public long Length
        {
            get;
            private set;
        }
        /// <summary>
        /// 文字列
        /// </summary>
        public string Content
        {
            get;
            private set;
        }
        public CreateLayoutEventArgs(long index, long length,string content)
        {
            this.Index = index;
            this.Length = length;
            this.Content = content;
        }
    }

    /// <summary>
    /// 行番号とインデックスを相互変換するためのクラス
    /// </summary>
    public sealed class LineToIndexTable :  IEnumerable<string>
    {
        const int MaxEntries = 100;
        BigRangeList<LineToIndexTableData> collection;
        IPinableContainerStoreWithAutoDisposer<IComposableList<LineToIndexTableData>> dataStore;
        BigRangeList<LineToIndexTableData> _Lines { get { return this.collection; } }
        Document Document;
        ITextRender render;

        const int FOLDING_INDEX = 0;
        const int SYNTAX_HIGLITHER_INDEX = 1;
        const int LAYOUT_CACHE_SIZE_MEMORY_MODE = 128;
        ILineInfoGenerator[] _generators = new ILineInfoGenerator[2];

        internal LineToIndexTable(Document buf, int cache_size = -1)
        {
            this.Document = buf;
            this.Document.Markers.Updated += Markers_Updated;
            this.collection = new BigRangeList<LineToIndexTableData>();
            //4以上の値を指定しないとうまく動かないので、それ以外の値はメモリーに保存する
            if (cache_size >= 4)
            {
                var serializer = new LineToIndexTableDataSerializer();
                this.dataStore = new DiskPinableContentDataStore<IComposableList<LineToIndexTableData>>(serializer, buf.StringBuffer.WorkfilePath, cache_size);
            }
            else
            {
                this.dataStore = new MemoryPinableContentDataStoreWithAutoDisposer<IComposableList<LineToIndexTableData>>(LAYOUT_CACHE_SIZE_MEMORY_MODE);
            }
            this.dataStore.Disposeing += DataStore_Disposeing;
            this.collection.CustomBuilder.DataStore = dataStore;
            this._generators[FOLDING_INDEX] = new FoldingGenerator();
            this._generators[SYNTAX_HIGLITHER_INDEX] = new SyntaxHilightGenerator();
            this.WrapWidth = NONE_BREAK_LINE;
#if DEBUG && !NETFX_CORE
            if (!Debugger.IsAttached)
            {
                Guid guid = Guid.NewGuid();
                string path = string.Format("{0}\\footextbox_lti_debug_{1}.log", System.IO.Path.GetTempPath(), guid);
                //TODO: .NET core3だと使えない
                //Debug.Listeners.Add(new TextWriterTraceListener(path));
                //Debug.AutoFlush = true;
            }
#endif
            this.Init();
        }

        private void DataStore_Disposeing(IComposableList<LineToIndexTableData> list)
        {
            foreach(var item in list)
            {
                item.Dispose();
            }
        }

        void Markers_Updated(object sender, EventArgs e)
        {
            this.ClearLayoutCache();
        }

        /// <summary>
        /// ITextRenderインターフェイスのインスタンス。必ずセットすること
        /// </summary>
        internal ITextRender Render
        {
            get { return this.render; }
            set
            {
                this.render = value;
            }
        }

        internal SpilitStringEventHandler SpilitString;

        /// <summary>
        /// 折り畳み関係の情報を収めたコレクション
        /// </summary>
        public FoldingCollection FoldingCollection
        {
            get
            {
                return ((FoldingGenerator)this._generators[FOLDING_INDEX]).FoldingCollection;
            }
            private set
            {
            }
        }

        /// <summary>
        /// シンタックスハイライター
        /// </summary>
        public IHilighter Hilighter
        {
            get
            {
                return ((SyntaxHilightGenerator)this._generators[SYNTAX_HIGLITHER_INDEX]).Hilighter;
            }
            set
            {
                ((SyntaxHilightGenerator)this._generators[SYNTAX_HIGLITHER_INDEX]).Hilighter = value;
                if (value == null)
                    this._generators[FOLDING_INDEX].Clear(this);
            }
        }

        /// <summary>
        /// 折り畳み
        /// </summary>
        public IFoldingStrategy FoldingStrategy
        {
            get
            {
                return ((FoldingGenerator)this._generators[FOLDING_INDEX]).FoldingStrategy;
            }
            set
            {
                ((FoldingGenerator)this._generators[FOLDING_INDEX]).FoldingStrategy = value;
                if (value == null)
                    this._generators[FOLDING_INDEX].Clear(this);
            }
        }

        /// <summary>
        /// ピクセル単位で折り返すかどうか
        /// </summary>
        public double WrapWidth
        {
            get;
            set;
        }

        /// <summary>
        /// 行を折り返さないことを表す
        /// </summary>
        public const double NONE_BREAK_LINE = -1;

        /// <summary>
        /// 保持しているレイアウトキャッシュをクリアーする
        /// </summary>
        public void ClearLayoutCache()
        {
            foreach (var items in this.dataStore.ForEachAvailableContent())
            {
                foreach (LineToIndexTableData data in items)
                {
                    data.Dispose();
                }
            }
        }

        /// <summary>
        /// 保持しているレイアウトキャッシュをクリアーする
        /// </summary>
        public void ClearLayoutCache(long index, long length)
        {
            this.ClearLayoutCache();
        }

        public int Count
        {
            get
            {
                return this.collection.Count;
            }
        }

        /// <summary>
        /// 行番号に対応する文字列を返します
        /// </summary>
        /// <param name="n"></param>
        /// <returns></returns>
        public string this[int n]
        {
            get
            {
                LineToIndexTableData data = this.GetRaw(n);
                if(data.LineString == null)
                {
                    string str = this.Document.ToString(this.GetLineHeadLongIndex(n), data.Length);
                    data.LineString = str;
                }

                return data.LineString;
            }
        }

        /// <summary>
        /// 当該行の先頭インデックスを取得する
        /// </summary>
        /// <param name="row"></param>
        /// <returns></returns>
        [Obsolete]
        public int GetLineHeadIndex(int row)
        {
            if (this.collection.Count == 0)
                return 0;
            return (int)this.collection.GetIndexIntoRange(row).Index;
        }

        /// <summary>
        /// 当該行の先頭インデックスを取得する
        /// </summary>
        /// <param name="row"></param>
        /// <returns></returns>
        public long GetLineHeadLongIndex(int row)
        {
            if (this.collection.Count == 0)
                return 0;
            return this.collection.GetWithConvertAbsolteIndex(row).Index;
        }

        internal void UpdateLineAsReplace(int row, long removedLength, long insertedLength)
        {
            if (row >=  this._Lines.Count)
                return;

            long deltaLength = insertedLength - removedLength;

            if(deltaLength != 0)
            {
                var newData = new LineToIndexTableData(this.GetLineHeadLongIndex(row), this.GetLengthFromLineNumber(row) + deltaLength, true, true, null);

                this.collection.Set(row,newData);
            }

            foreach (var generator in this._generators)
                generator.Update(this.Document, this.GetLineHeadLongIndex(row), insertedLength, removedLength);
        }

        internal int UpdateLayoutLine(long index, long removedLength, long insertedLength,bool setdirtyflag)
        {
            DebugLog.WriteLine("Replaced Index:{0} RemoveLength:{1} InsertLength:{2}", index, removedLength, insertedLength);

            int startRow, endRow;
            GetRemoveRange(index, removedLength, out startRow, out endRow);

            //行が存在しない場合、後で構築されるので何もしてはならない
            if (startRow == -1 || endRow == -1)
                return 0;

            long deltaLength = insertedLength - removedLength;

            var result = GetAnalyzeLength(startRow, endRow, index, removedLength, insertedLength);
            long HeadIndex = result.Item1;
            long analyzeLength = result.Item2;

            //挿入範囲内のドキュメントから行を生成する
            SpilitStringEventArgs e = new SpilitStringEventArgs(this.Document, HeadIndex, analyzeLength, startRow);
            IList<LineToIndexTableData> newLines = this.CreateLineList(e.index, e.length, setdirtyflag);

            int removeCount = endRow - startRow + 1;
            for (int i = startRow; i < startRow + removeCount; i++)
            {
                IDisposable item = (IDisposable)this.collection.Get(i);
                item.Dispose();
            }

            //行を挿入する
            this.collection.RemoveRange(startRow, removeCount);
            this.collection.InsertRange(startRow, newLines);

            bool addedDummyLine = this.AddDummyLine();

            foreach (var generator in this._generators)
                generator.Update(this.Document, index, insertedLength, removedLength);

            return (addedDummyLine ? 1 : 0) + newLines.Count - removeCount;
        }

        internal IEnumerable<Tuple<long, long,string>> ForEachLines(long startIndex, long endIndex, int maxCharCount = -1)
        {
            long currentLineHeadIndex = startIndex;
            long currentLineLength = 0;
            string linefeed = string.Empty;

            long i = startIndex;
            while (true)
            {
                if (i > endIndex)
                    break;
                currentLineLength++;
                char c = this.Document[i];
                if (c == Document.LF_CHAR)
                {
                    linefeed = Document.LF_STR;
                }else if (c == Document.CR_CHAR){
                    if (i + 1 <= endIndex && this.Document[i + 1] == Document.LF_CHAR)
                    {
                        currentLineLength++;
                        i++;
                        linefeed = Document.CRLF_STR;
                    }
                    else
                    {
                        linefeed = Document.CR_STR;
                    }
                }
                if (linefeed != string.Empty || (maxCharCount != -1 && currentLineLength >= maxCharCount))
                {
                    linefeed = string.Empty;
                    UnicodeCategory uc = CharUnicodeInfo.GetUnicodeCategory(c);
                    if (uc != UnicodeCategory.NonSpacingMark &&
                    uc != UnicodeCategory.SpacingCombiningMark &&
                    uc != UnicodeCategory.EnclosingMark &&
                    uc != UnicodeCategory.Surrogate)
                    {
                        if (currentLineLength > Int32.MaxValue - 1)
                            throw new InvalidOperationException("Line length is too large. It must be within Int32.MaxValue - 1");
                        yield return new Tuple<long, long,string>(currentLineHeadIndex, currentLineLength,linefeed);
                        currentLineHeadIndex += currentLineLength;
                        currentLineLength = 0;
                    }
                }
                i++;
            }
            if (currentLineLength > 0)
                yield return new Tuple<long, long,string>(currentLineHeadIndex, currentLineLength,linefeed);
        }

        IList<LineToIndexTableData> CreateLineList(long index, long length,bool setdirtyflag = false, int lineLimitLength = -1)
        {
            long startIndex = index;
            long endIndex = index + length - 1;
            List<LineToIndexTableData> output = new List<LineToIndexTableData>();

            foreach (var range in this.ForEachLines(startIndex, endIndex, lineLimitLength))
            {
                long lineHeadIndex = range.Item1;
                long lineLength = range.Item2;
                char c = this.Document[lineHeadIndex + lineLength - 1];
                bool hasNewLine = range.Item3 != string.Empty;
                LineToIndexTableData result = new LineToIndexTableData(lineHeadIndex, lineLength, hasNewLine, setdirtyflag, null);
                output.Add(result);
            }

            return output;
        }

        void GetRemoveRange(long index, long length,out int startRow,out int endRow)
        {
            if(this.TryGetLineNumberFromIndex(index, out startRow) == false)
            {
                startRow = -1;
                endRow = -1;
                return;
            }

            if (this.TryGetLineNumberFromIndex(index + length, out endRow) == false)
                endRow = this._Lines.Count - 1;

            if (endRow >= this._Lines.Count)
                endRow = this._Lines.Count - 1;
        }

        Tuple<long, long> GetAnalyzeLength(int startRow, int endRow, long updateStartIndex, long removedLength, long insertedLength)
        {
            long HeadIndex = this.GetLongIndexFromLineNumber(startRow);
            long LastIndex = this.GetLongIndexFromLineNumber(endRow) + this.GetLengthFromLineNumber(endRow) - 1;

            //SpilitStringの対象となる範囲を求める
            long fisrtPartLength = updateStartIndex - HeadIndex;
            long secondPartLength = LastIndex - (updateStartIndex + removedLength - 1);
            long analyzeLength = fisrtPartLength + secondPartLength + insertedLength;
            Debug.Assert(analyzeLength <= this.Document.Length - 1 - HeadIndex + 1);

            //分析する範囲とドキュメントの長さが一致しているかどうか
            long IndexAnayzed = HeadIndex + analyzeLength - 1;
            if (IndexAnayzed < this.Document.Length -1)
            {
                long i = IndexAnayzed;
                while (true)
                {
                    if (i >= this.Document.Length)
                        break;
                    if (this.Document.StringBuffer[i] == Document.LF_CHAR)
                        break;
                    if(this.Document.StringBuffer[i] == Document.CR_CHAR)
                    {
                        if(i + 1 < this.Document.Length && this.Document.StringBuffer[i + 1] == Document.LF_CHAR)
                        {
                            i++;
                            break;
                        }
                        else
                        {
                            break;
                        }
                    }
                    i++;
                }
                analyzeLength = i - HeadIndex + 1;
            }

            return new Tuple<long, long>(HeadIndex, analyzeLength);
        }

        bool AddDummyLine()
        {
            LineToIndexTableData dummyLine = null;
            if (this._Lines.Count == 0)
            {
                dummyLine = new LineToIndexTableData();
                this._Lines.Add(dummyLine);
                return true;
            }

            int lastLineRow = this._Lines.Count > 0 ? this._Lines.Count - 1 : 0;
            long lastLineHeadIndex = this.GetLongIndexFromLineNumber(lastLineRow);
            long lastLineLength = this.GetLengthFromLineNumber(lastLineRow);

            bool hasLastNewLine = false;
            if (this.Document.Length > 1 && this.Document[this.Document.Count - 2] == Document.CR_CHAR){
                if (this.Document[this.Document.Length - 1] == Document.LF_CHAR) {
                    hasLastNewLine = true;
                }
            }
            else if(this.Document.Length > 0){
                if (this.Document[this.Document.Length - 1] == Document.CR_CHAR){
                    hasLastNewLine = true;
                }else if (this.Document[Document.Length - 1] == Document.LF_CHAR){
                    hasLastNewLine = true;
                }
            }

            if (lastLineLength != 0 && hasLastNewLine)
            {
                long realIndex = lastLineHeadIndex + lastLineLength;
                dummyLine = new LineToIndexTableData(realIndex, 0, true, false, null);
                this._Lines.Add(dummyLine);
                return true;
            }
            return false;
        }

        /// <summary>
        /// 生データを取得します
        /// </summary>
        /// <param name="row">行</param>
        /// <returns>LineToIndexTableData</returns>
        /// <remarks>いくつかの値は実態とかけ離れた値を返します。詳しくはLineToIndexTableDataの注意事項を参照すること</remarks>
        internal LineToIndexTableData GetRaw(int row)
        {
            LineToIndexTableData lineData;
            if(this.TryGetRaw(row,out lineData))
            {
                return lineData;
            }
            throw new ArgumentOutOfRangeException();
        }

        /// <summary>
        /// 生データを取得します
        /// </summary>
        /// <param name="row">行</param>
        /// <param name="lineData">取得できた行データー。存在しないときはnullが入る。</param>
        /// <returns>取得できた場合は真を返し、そうでない場合は偽を返す。</returns>
        /// <remarks>いくつかの値は実態とかけ離れた値を返します。詳しくはLineToIndexTableDataの注意事項を参照すること。</remarks>
        internal bool TryGetRaw(int row,out LineToIndexTableData lineData)
        {
            if(row > this._Lines.Count)
            {
                lineData = null;
                return false;
            }
            lineData = this._Lines.Get(row);
            return true;
        }

        /// <summary>
        /// 指定された行までレイアウト行を構築します
        /// </summary>
        /// <param name="row">行</param>
        public void FetchLine(int row)
        {
            long startIndex = this.GetLongIndexFromLineNumber(this.collection.Count - 1);
            long totalAnayzedLength = FetchLineWithoutEvent(row);
            this.Document.FireUpdate(
                new DocumentUpdateEventArgs(UpdateType.BuildLayout,
                startIndex,
                0,
                totalAnayzedLength));
        }

        internal long FetchLineWithoutEvent(int row)
        {
            long totalAnalyzedLength = 0;
            while (row >= this._Lines.Count - 1)
            {
                //直接最終行を取得すると後々おかしくなる
                int lastRow = this.collection.Count - 1;
                long LineHeadIndex = this.GetLongIndexFromLineNumber(lastRow);
                long Length = this.GetLengthFromLineNumber(lastRow);
                if (LineHeadIndex + Length >= this.Document.Length)
                {
                    return 0;
                }
                long analyzeStartIndex = LineHeadIndex + Length;
                long analyzeLength = Document.PreloadLength;
                long documentLength = this.Document.Length;
                if (analyzeStartIndex + analyzeLength > documentLength)
                    analyzeLength = documentLength - analyzeStartIndex;
                this.UpdateLayoutLine(analyzeStartIndex, 0, analyzeLength, false);
                totalAnalyzedLength += analyzeLength;
            }
            return totalAnalyzedLength;
        }

        /// <summary>
        /// 行番号をインデックスに変換します
        /// </summary>
        /// <param name="row">行番号</param>
        /// <returns>0から始まるインデックスを返す</returns>
        [Obsolete]
        public int GetIndexFromLineNumber(int row)
        {
            return (int)GetLongIndexFromLineNumber(row);
        }

        /// <summary>
        /// 行番号をインデックスに変換します
        /// </summary>
        /// <param name="row">行番号</param>
        /// <returns>0から始まるインデックスを返す</returns>
        public long GetLongIndexFromLineNumber(int row)
        {
            if (row < 0 || row > this._Lines.Count)
                throw new ArgumentOutOfRangeException();
            return this.GetLineHeadLongIndex(row);
        }

        /// <summary>
        /// 行の長さを得ます
        /// </summary>
        /// <param name="row">行番号</param>
        /// <returns>行の文字長を返します</returns>
        public int GetLengthFromLineNumber(int row)
        {
            if (row < 0 || row > this._Lines.Count)
                throw new ArgumentOutOfRangeException();
            return (int)this.collection.Get(row).Length;
        }

        /// <summary>
        /// 更新フラグを取得します
        /// </summary>
        /// <param name="row">行番号</param>
        /// <returns>更新されていれば真。そうでなければ偽</returns>
        public bool GetDirtyFlag(int row)
        {
            if (row < 0 || row > this._Lines.Count)
                throw new ArgumentOutOfRangeException();
            return this.GetRaw(row).Dirty;
        }

        /// <summary>
        /// 行の高さを返す
        /// </summary>
        /// <param name="tp">テキストポイント</param>
        /// <returns>テキストポイントで指定された行の高さを返します</returns>
        public double GetLineHeight(TextPoint tp)
        {
            return this.render.emSize.Height * this.Render.LineEmHeight;
        }

        internal ITextLayout GetLayout(int row)
        {
            var lineData = this.GetRaw(row);
            if (lineData.Layout != null && lineData.Layout.Invaild)
            {
                lineData.Layout.Dispose();
                lineData.Layout = null;
            }
            if (lineData.Layout == null || lineData.Layout.Disposed)
                lineData.Layout = this.CreateLayout(row);
            return lineData.Layout;
        }

        internal event EventHandler<CreateLayoutEventArgs> CreateingLayout;

        ITextLayout CreateLayout(int row)
        {
            ITextLayout layout;
            LineToIndexTableData lineData = this.GetRaw(row);
            if (lineData.Length == 0)
            {
                var sublayout = this.render.CreateLaytout("", null, null, null, this.WrapWidth);
                layout = new CombineTextLayout(new ITextLayout[] {sublayout});
            }
            else
            {
                long lineHeadIndex = this.GetLineHeadLongIndex(row);

                string lineString = this.Document.ToString(lineHeadIndex, (int)lineData.length);
                lineData.LineString = lineString;

                if (this.CreateingLayout != null)
                    this.CreateingLayout(this, new CreateLayoutEventArgs(lineHeadIndex, lineData.Length, lineString));

                var watchedMarker = this.Document.MarkerPatternSet.GetMarkers(new CreateLayoutEventArgs(lineHeadIndex, lineData.Length, lineString));

                List<ITextLayout> layouts = new List<ITextLayout>();
                foreach(var touple in this.ForEachLines(lineHeadIndex, lineHeadIndex + lineData.Length - 1, Document.MaximumLineLength))
                {
                    long indexSublayout = touple.Item1;
                    long lengthSublayout = touple.Item2;
                    string content = this.Document.ToString(touple.Item1, touple.Item2);
                    var userMarkerRange = from id in this.Document.Markers.IDs
                                          from s in this.Document.Markers.Get(id, indexSublayout, lengthSublayout)
                                          let n = Util.ConvertAbsIndexToRelIndex(s, indexSublayout, lengthSublayout)
                                          select n;
                    var watchdogMarkerRange = from s in watchedMarker
                                              let n = Util.ConvertAbsIndexToRelIndex(s, indexSublayout, lengthSublayout)
                                              select n;
                    var markerRange = watchdogMarkerRange.Concat(userMarkerRange);
                    var selectRange = from s in this.Document.Selections.Get(indexSublayout, lengthSublayout)
                                      let n = Util.ConvertAbsIndexToRelIndex(s, indexSublayout, lengthSublayout)
                                      select n;
                    var syntaxRnage = lineData.Syntax == null ? new SyntaxInfo[] { }  : lineData.Syntax.Select((s) =>
                    {
                        return Util.ConvertAbsIndexToRelIndex(s, indexSublayout, lengthSublayout);
                    }).ToArray();
                    layout = this.render.CreateLaytout(content, syntaxRnage, markerRange, selectRange, this.WrapWidth);
                    layouts.Add(layout);
                }

                layout = new CombineTextLayout(layouts);
            }

            return layout;
        }

        public int IndexOfLoose(long start)
        {
            int result = (int)this.collection.GetIndexFromAbsoluteIndexIntoRange(start);
            if (result == -1)
            {
                int lastRow = this.collection.Count - 1;
                var line = this.collection.Get(lastRow);
                var lineHeadIndex = this.GetLineHeadLongIndex(lastRow);
                if (start >= lineHeadIndex && start <= lineHeadIndex + line.length)   //最終行長+1までキャレットが移動する可能性があるので
                {
                    return this.collection.Count - 1;
                }
            }
            return result;
        }

        /// <summary>
        /// インデックスを行番号に変換します
        /// </summary>
        /// <param name="index">インデックス</param>
        /// <returns>行番号を返します</returns>
        public int GetLineNumberFromIndex(long index)
        {
            var result = this.IndexOfLoose(index);

            if(result == -1)
                throw new ArgumentOutOfRangeException("該当する行が見つかりませんでした");

            return result;
        }

        /// <summary>
        /// 行番号を返す
        /// </summary>
        /// <param name="index">インデックス</param>
        /// <param name="resultRow">対応する行番号。存在しなければ-1。</param>
        /// <returns>存在しなければ偽。存在すれば真を返す。</returns>
        public bool TryGetLineNumberFromIndex(long index,out int resultRow)
        {
            resultRow = -1;
            var result = this.IndexOfLoose(index);
            if (result == -1)
                return false;

            resultRow = result;
            return true;
        }

        /// <summary>
        /// インデックスからテキストポイントに変換します
        /// </summary>
        /// <param name="index">インデックス</param>
        /// <returns>TextPoint構造体を返します</returns>
        public TextPoint GetTextPointFromIndex(long index)
        {
            TextPoint tp = new TextPoint();
            tp.row = GetLineNumberFromIndex(index);
            tp.col = (int)(index - this.GetLineHeadLongIndex(tp.row));
            Debug.Assert(tp.row < this._Lines.Count && tp.col <= this.GetRaw(tp.row).Length);
            return tp;
        }

        /// <summary>
        /// テキストポイントからインデックスに変換します
        /// </summary>
        /// <param name="tp">TextPoint構造体</param>
        /// <returns>インデックスを返します</returns>
        [Obsolete]
        public int GetIndexFromTextPoint(TextPoint tp)
        {
            return (int)GetLongIndexFromTextPoint(tp);
        }

        /// <summary>
        /// テキストポイントからインデックスに変換します
        /// </summary>
        /// <param name="tp">TextPoint構造体</param>
        /// <returns>インデックスを返します</returns>
        public long GetLongIndexFromTextPoint(TextPoint tp)
        {
            if (tp == TextPoint.Null)
                throw new ArgumentNullException("TextPoint.Null以外の値でなければなりません");
            if(tp.row < 0 || tp.row > this._Lines.Count)
                throw new ArgumentOutOfRangeException("tp.rowが設定できる範囲を超えています");
            if (tp.col < 0 || tp.col > this.GetRaw(tp.row).Length)
                throw new ArgumentOutOfRangeException("tp.colが設定できる範囲を超えています");
            return this.GetLineHeadLongIndex(tp.row) + tp.col;
        }

        /// <summary>
        /// フォールディングを再生成します
        /// </summary>
        /// <param name="force">ドキュメントが更新されていなくても再生成する</param>
        /// <returns>生成された場合は真を返す</returns>
        /// <remarks>デフォルトではドキュメントが更新されている時にだけ再生成されます</remarks>
        public bool GenerateFolding(bool force = false)
        {
            return this._generators[FOLDING_INDEX].Generate(this.Document, this, force);
        }

        /// <summary>
        /// フォールディングをすべて削除します
        /// </summary>
        public void ClearFolding()
        {
            this._generators[FOLDING_INDEX].Clear(this);
        }

        /// <summary>
        /// すべての行に対しシンタックスハイライトを行います
        /// </summary>
        public bool HilightAll(bool force = false)
        {
            return this._generators[SYNTAX_HIGLITHER_INDEX].Generate(this.Document, this, force);
        }

        /// <summary>
        /// ハイライト関連の情報をすべて削除します
        /// </summary>
        public void ClearHilight()
        {
            this._generators[SYNTAX_HIGLITHER_INDEX].Clear(this);
        }

        private void Init()
        {
            LineToIndexTableData dummy = new LineToIndexTableData();
            this._Lines.Add(dummy);
        }

        /// <summary>
        /// すべて削除します
        /// </summary>
        public void Clear()
        {
            this.ClearLayoutCache();
            this.collection.Clear();
            this.ClearFolding();
            this.Init();
        }

        internal void Trim()
        {
            this.ClearLayoutCache();
            this.dataStore.Commit();
        }

        #region IEnumerable<string> メンバー

        /// <summary>
        /// コレクションを反復処理するためのIEnumeratorを返す
        /// </summary>
        /// <returns>IEnumeratorオブジェクト</returns>
        public IEnumerator<string> GetEnumerator()
        {
            for (int i = 0; i < this._Lines.Count; i++)
                yield return this[i];
        }

        #endregion

        #region IEnumerable メンバー

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            for (int i = 0; i < this._Lines.Count; i++)
                yield return this[i];
        }

        #endregion
    }

}
