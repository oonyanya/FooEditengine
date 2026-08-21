/*
 * Copyright (C) 2013 FooProject
 * * This program is free software; you can redistribute it and/or modify it under the terms of the GNU General Public License as published by
 * the Free Software Foundation; either version 3 of the License, or (at your option) any later version.

 * This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of 
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.

You should have received a copy of the GNU General Public License along with this program. If not, see <http://www.gnu.org/licenses/>.
 */
using System;
using System.Buffers;
using System.CodeDom;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using FooProject.Collection;

namespace FooEditEngine
{
    /// <summary>
    /// 既定のIDリスト
    /// </summary>
    public static class MarkerIDs
    {
        /// <summary>
        /// デフォルトIDを表す
        /// </summary>
        public static int Defalut = 0;
        /// <summary>
        /// URLを表す
        /// </summary>
        public static int URL = 1;
        /// <summary>
        /// IMEの変換候補を表す
        /// </summary>
        public static int IME = -1;
    }
    /// <summary>
    /// マーカーのタイプを表す列挙体
    /// </summary>
    public enum HilightType
    {
        /// <summary>
        /// マーカーとして表示しないことを表す
        /// </summary>
        None,
        /// <summary>
        /// 選択状態を表す
        /// </summary>
        Select,
        /// <summary>
        /// URLを表す
        /// </summary>
        Url,
        /// <summary>
        /// 実線を表す
        /// </summary>
        Sold,
        /// <summary>
        /// 破線を表す
        /// </summary>
        Dash,
        /// <summary>
        /// 一点鎖線を表す
        /// </summary>
        DashDot,
        /// <summary>
        /// 二点鎖線を表す
        /// </summary>
        DashDotDot,
        /// <summary>
        /// 点線を表す
        /// </summary>
        Dot,
        /// <summary>
        /// 波線を表す
        /// </summary>
        Squiggle,
    }

    public readonly struct MarkerData : IEqualityComparer<MarkerData>, IEquatable<MarkerData>
    {
        /// <summary>
        /// マーカーのタイプ
        /// </summary>
        public HilightType hilight { get; }

        /// <summary>
        /// 色を指定する
        /// </summary>
        public Color color { get; }

        /// <summary>
        /// 線を太くするかどうか
        /// </summary>
        public bool isBoldLine { get; }

        public MarkerData(HilightType hilight, bool isBoldLine = false)
        {
            this.hilight = hilight;
            this.color = new Color();
            this.isBoldLine = isBoldLine;
        }

        public MarkerData(HilightType hilight, Color color, bool isBoldLine = false)
        {
            this.hilight = hilight;
            this.color = color;
            this.isBoldLine = isBoldLine;
        }

        public override bool Equals(object obj) {
            return (obj is MarkerData other) && this.Equals(other);
        }

        public bool Equals(MarkerData x, MarkerData y)
        {
            return x.hilight == y.hilight && x.color.Equals(y.color) && x.isBoldLine == isBoldLine;
        }

        public int GetHashCode([DisallowNull] MarkerData obj)
        {
            return obj.hilight.GetHashCode() ^ obj.color.GetHashCode() ^ obj.isBoldLine.GetHashCode();
        }

        public bool Equals(MarkerData other)
        {
            return this.hilight == other.hilight && this.color.Equals(other.color) && this.isBoldLine == other.isBoldLine;
        }
    }

    /// <summary>
    /// マーカー自身を表します
    /// </summary>
    public class Marker : FooProject.Collection.IRleArrayRangeItem<MarkerData>, IEqualityComparer<Marker>, IEquatable<Marker>
    {
        #region IRange メンバー

        /// <summary>
        /// 開始位置
        /// </summary>
        public long start
        {
            get;
            set;
        }

        /// <summary>
        /// 長さ
        /// </summary>
        public long length
        {
            get;
            set;
        }

        public MarkerData Value
        {
            get;
            set;
        }

        #endregion

        /// <summary>
        /// マーカーのタイプ
        /// </summary>
        public HilightType hilight { get {  return Value.hilight; }  }

        /// <summary>
        /// 色を指定する
        /// </summary>
        public Color color { get { return Value.color; } }

        /// <summary>
        /// 線を太くするかどうか
        /// </summary>
        public bool isBoldLine { get { return Value.isBoldLine; } }

        /// <summary>
        /// マーカーを作成します
        /// </summary>
        /// <param name="start">開始インデックス</param>
        /// <param name="length">長さ</param>
        /// <param name="hilight">タイプ</param>
        /// <returns>マーカー</returns>
        public static Marker Create(long start, long length, MarkerData data)
        {
            return new Marker { start = start, length = length, Value = data };
        }

        /// <summary>
        /// マーカーを作成します
        /// </summary>
        /// <param name="start">開始インデックス</param>
        /// <param name="length">長さ</param>
        /// <param name="hilight">タイプ</param>
        /// <returns>マーカー</returns>
        public static Marker Create(long start, long length, HilightType hilight)
        {
            return new Marker { start = start, length = length, Value = new MarkerData( hilight, false)};
        }

        /// <summary>
        /// マーカーを作成します
        /// </summary>
        /// <param name="start">開始インデックス</param>
        /// <param name="length">長さ</param>
        /// <param name="hilight">タイプ</param>
        /// <param name="color">色</param>
        /// <param name="isBoldLine">線を太くするかどうか</param>
        /// <returns>マーカー</returns>
        public static Marker Create(long start, long length, HilightType hilight,Color color,bool isBoldLine = false)
        {
            return new Marker { start = start, length = length, Value = new MarkerData(hilight, color, isBoldLine) };
        }

        /// <summary>
        /// 等しいかどうかを調べます
        /// </summary>
        /// <param name="x">比較されるマーカー</param>
        /// <param name="y">比較するマーカー</param>
        /// <returns>等しいなら真。そうでなければ偽</returns>
        public bool Equals(Marker x, Marker y)
        {
            return x.Value.Equals(y.Value) && x.length == y.length && x.start == y.start;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as Marker);
        }

        /// <summary>
        /// ハッシュを得ます
        /// </summary>
        /// <param name="obj">マーカー</param>
        /// <returns>ハッシュ</returns>
        public int GetHashCode(Marker obj)
        {
            return (int)this.start ^ (int)this.length ^ (int)this.hilight ^ (int)(this.start >> 32);
        }

        public FooProject.Collection.IRange DeepCopy()
        {
            var newItem = new Marker();
            newItem.start = this.start;
            newItem.length = this.length;
            newItem.Value = new MarkerData(this.hilight, this.color, this.isBoldLine);
            return newItem;
        }

        public bool Equals(Marker other)
        {
            return this.Value.Equals(other.Value) && this.length == other.length && this.start == other.start;
        }
    }

    public class MarkerRleCollection : IRangeCollection<Marker>
    {
        class MarkerDataRleCollection : BigRleArrayCollectionBase<MarkerData>
        {
            protected override IRleArrayRangeItem<MarkerData> CreateItem(MarkerData value, long start = -1, long length = -1)
            {
                return Marker.Create(start, length, value);
            }
        }

        MarkerDataRleCollection collection = new MarkerDataRleCollection();

        public int Count => this.collection.Count;

        public MarkerRleCollection()
        {
            this.collection.Add(Marker.Create(0, 0, HilightType.None));
        }

        public void UpdateStartIndex(long deltaLength, long startRow)
        {
            var index = 0L;
            var item = this.collection.Get(startRow, out index);
            item.length += deltaLength;
            this.collection.SetAt(index, item);
        }

        public void AddOrInsert(Marker m)
        {
            if (this.collection.Count == 0)
            {
                //マーカーが存在しないときに０より大きな値の奴を突っ込むと表示がおかしくなる
                if (m.start > 0)
                {
                    this.Add(Marker.Create(0, m.start, HilightType.None));
                }
                this.collection.Add(m);
            }
            else
            {
                this.collection.RemoveRange(m.start, m.length);
                this.collection.Insert(m);
            }
        }

        public void Add(Marker item)
        {
            this.collection.Add(item);
        }

        Marker IRangeCollection<Marker>.GetAt(long index)
        {
            return (Marker)this.collection.GetAt(index);
        }

        IEnumerable<Marker> IRangeCollection<Marker>.GetRanges(long index)
        {
            if (this.collection.Count > 0)
            {
                var ranges = this.collection.GetRanges(index, this.collection.TotalRangeCount - index);
                foreach (var m in ranges.Where(m => m.Value.hilight != HilightType.None))
                    yield return (Marker)m;
            }
        }

        IEnumerable<Marker> IRangeCollection<Marker>.GetRanges(long start, long length)
        {
            if (this.collection.Count > 0)
            {
                var ranges = this.collection.GetRanges(start, length);
                foreach (var m in ranges.Where(m => m.Value.hilight != HilightType.None))
                    yield return (Marker)m;
            }
        }

        public void Insert(Marker item)
        {
            this.Insert(item);
        }

        IEnumerator<Marker> IEnumerable<Marker>.GetEnumerator()
        {
            foreach (var m in this.collection.GetRanges(0,this.collection.TotalRangeCount).Where(m => m.Value.hilight != HilightType.None))
                yield return (Marker)m;
        }

        public void Clear()
        {
            this.collection.Clear();
            this.collection.Add(Marker.Create(0, 0, HilightType.None));
        }

        public void RemoveRange(long start, long length)
        {
            if (collection.Count > 0)
            {
                collection.RemoveRange(start, length);
                collection.Insert(Marker.Create(start, length, HilightType.None));
            }
        }

        public void RemoveAt(long startRow)
        {
            collection.RemoveAt(startRow);
        }

        public IEnumerator GetEnumerator()
        {
            throw new NotImplementedException();
        }
        public void UpdateMarkers(long startIndex, long insertLength, long removeLength)
        {
            if (collection.Count > 0)
            {
                this.RemoveRange(startIndex, removeLength);
                this.UpdateStartIndex(insertLength, startIndex);
            }
        }
    }

    /// <summary>
    /// マーカークラスのコレクションを表します
    /// </summary>
    public sealed class MarkerCollection
    {
        Dictionary<int, IRangeCollection<Marker>> collection = new Dictionary<int, IRangeCollection<Marker>>();

        internal MarkerCollection()
        {
            var list = new int[] { MarkerIDs.Defalut, MarkerIDs.IME, MarkerIDs.URL };
            foreach (var id in list)
            {
                var markers = new MarkerRleCollection();
                this.collection.Add(id, markers);
            }
            this.Updated +=new EventHandler((s,e)=>{});
        }

        /// <summary>
        /// 更新されたことを通知します
        /// </summary>
        public event EventHandler Updated;

        internal void OnInit(Document doc)
        {
            this.Clear();
            foreach (var markers in this.collection.Values)
            {
                if(markers is MarkerRleCollection)
                    markers.Add(Marker.Create(0, doc.StringBuffer.Length, HilightType.None));
            }
        }

        internal void Add(int id,Marker m)
        {
            this.AddImpl(id, m);
            this.Updated(this, null);
        }

        void AddImpl(int id, Marker m)
        {
            IRangeCollection<Marker> markers;
            if (this.collection.TryGetValue(id, out markers))
            {
                markers.AddOrInsert(m);
            }
            else
            {
                markers = new MarkerRleCollection();
                markers.Add(m);
                this.collection.Add(id, markers);
            }
        }

        internal void AddRange(int id, IEnumerable<Marker> collection)
        {
            foreach (Marker m in collection)
                this.AddImpl(id, m);
            this.Updated(this, null);
        }

        internal void RemoveAll(int id)
        {
            IRangeCollection<Marker> markers;
            if (this.collection.TryGetValue(id, out markers))
            {
                markers.Clear();
            }
            this.Updated(this, null);
        }

        internal void RemoveAll(int id, long start, long length)
        {
            IRangeCollection<Marker> markers;
            if (this.collection.TryGetValue(id, out markers))
            {
                markers.RemoveRange(start, length);
            }
            this.Updated(this, null);
        }

        internal void RemoveAll(int id, HilightType type)
        {
            IRangeCollection<Marker> markers;
            if (this.collection.TryGetValue(id, out markers))
            {
                for (int i = 0; i < markers.Count; i++)
                {
                    if (markers.GetAt(i).Value.hilight == type)
                        markers.RemoveAt(i);
                }
            }
            this.Updated(this, null);
        }

        internal IEnumerable<int> IDs
        {
            get
            {
                return this.collection.Keys;
            }
        }

        internal IEnumerable<Marker> Get(int id)
        {
            IRangeCollection<Marker> markers;
            if (this.collection.TryGetValue(id, out markers))
            {
                foreach (var m in markers.Where(m => m.Value.hilight != HilightType.None))
                    yield return (Marker)m;
            }
            yield break;
        }

        internal IEnumerable<Marker> Get(int id, long index)
        {
            IRangeCollection<Marker> markers;
            if (this.collection.TryGetValue(id, out markers))
            {
                return markers.GetRanges(index);
            }
            return [];
        }

        internal IEnumerable<Marker> Get(int id, long index, long length)
        {
            IRangeCollection<Marker> markers;
            if (this.collection.TryGetValue(id, out markers))
            {
                return markers.GetRanges(index, length);
            }
            return [];
        }

        /// <summary>
        /// マーカーをすべて削除します
        /// </summary>
        /// <param name="id">マーカーＩＤ</param>
        public void Clear(int id)
        {
            IRangeCollection<Marker> markers;
            if (this.collection.TryGetValue(id, out markers))
            {
                markers.Clear();
            }
            this.Updated(this, null);
        }

        /// <summary>
        /// マーカーをすべて削除します
        /// </summary>
        public void Clear()
        {
            this.collection.Clear();
            foreach (var id in this.IDs)
            {
                this.collection.Clear();
            }
            this.Updated(this, null);
        }

        internal void UpdateMarkers(long startIndex, long insertLength, long removeLength)
        {
            foreach (var markers in this.collection.Values)
            {
                markers.UpdateMarkers(startIndex,insertLength, removeLength);
            }
        }

    }
}
