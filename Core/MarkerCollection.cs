/*
 * Copyright (C) 2013 FooProject
 * * This program is free software; you can redistribute it and/or modify it under the terms of the GNU General Public License as published by
 * the Free Software Foundation; either version 3 of the License, or (at your option) any later version.

 * This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of 
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.

You should have received a copy of the GNU General Public License along with this program. If not, see <http://www.gnu.org/licenses/>.
 */
using System;
using System.Collections;
using System.Collections.Generic;
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

    /// <summary>
    /// マーカー自身を表します
    /// </summary>
    public class Marker : FooProject.Collection.IRange, IEqualityComparer<Marker>, FooProject.Collection.IRleArrayRangeItem
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

        #endregion

        /// <summary>
        /// マーカーのタイプ
        /// </summary>
        public HilightType hilight;

        /// <summary>
        /// 色を指定する
        /// </summary>
        public Color color;

        /// <summary>
        /// 線を太くするかどうか
        /// </summary>
        public bool isBoldLine;

        /// <summary>
        /// マーカーを作成します
        /// </summary>
        /// <param name="start">開始インデックス</param>
        /// <param name="length">長さ</param>
        /// <param name="hilight">タイプ</param>
        /// <returns>マーカー</returns>
        public static Marker Create(long start, long length, HilightType hilight)
        {
            return new Marker { start = start, length = length, hilight = hilight, color = new Color(), isBoldLine = false};
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
            return new Marker { start = start, length = length, hilight = hilight ,color = color , isBoldLine = isBoldLine };
        }

        /// <summary>
        /// 等しいかどうかを調べます
        /// </summary>
        /// <param name="x">比較されるマーカー</param>
        /// <param name="y">比較するマーカー</param>
        /// <returns>等しいなら真。そうでなければ偽</returns>
        public bool Equals(Marker x, Marker y)
        {
            return x.hilight == y.hilight && x.length == y.length && x.start == y.start;
        }

        public override bool Equals(object obj)
        {
            var other = (Marker)obj;
            return other.hilight == this.hilight && this.length == this.length && this.start == this.start;
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
            newItem.hilight = this.hilight;
            newItem.color = this.color;
            newItem.isBoldLine = this.isBoldLine;
            return newItem;
        }

        public bool EqualsValue(IRleArrayRangeItem other)
        {
            var other_marker = (Marker)other;
            return this.hilight == other_marker.hilight && this.isBoldLine == other_marker.isBoldLine && this.color.Equals(other_marker.color);
        }
    }

    public class MarkerRleCollection : IRangeCollection<Marker>
    {
        class MarkerRleCollectionInner : BigRleArrayCollectionBase<Marker>
        {
            protected override Marker CreateItem(Marker value, long start = -1, long length = -1)
            {
                var marker_start = 0L;
                var marker_length = 1L;
                if(start != -1)
                    marker_start = start;
                if (length != -1)
                    marker_length = length;
                return Marker.Create(marker_start, marker_length, value.hilight, value.color, value.isBoldLine);
            }
        }

        MarkerRleCollectionInner collection = new MarkerRleCollectionInner();

        public int Count => this.collection.Count;

        public MarkerRleCollection()
        {
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
                if (m.start > 0)
                {
                    this.Add(Marker.Create(0, m.start, HilightType.None));
                }
                this.collection.AddRange(m);
            }
            else
            {
                this.Insert(m);
            }
        }

        public void Add(Marker item)
        {
            this.collection.AddRange(item);
        }

        public Marker GetAt(long index)
        {
            return (Marker)this.collection.GetAt(index);
        }

        public IEnumerable<Marker> GetRanges(long index)
        {
            if (this.collection.Count > 0)
            {
                var ranges = this.collection.GetRanges(index, this.collection.TotalRangeCount - index);
                foreach (var m in ranges.Where(m => m.hilight != HilightType.None))
                    yield return (Marker)m;
            }
        }

        public IEnumerable<Marker> GetRanges(long start, long length)
        {
            if (this.collection.Count > 0)
            {
                var ranges = this.collection.GetRanges(start, length);
                foreach (var m in ranges.Where(m => m.hilight != HilightType.None))
                    yield return (Marker)m;
            }
        }

        public void Insert(Marker item)
        {
            this.collection.InsertRange(item.start, item);
        }

        public IEnumerator<Marker> GetEnumerator()
        {
            foreach (var m in this.collection.Where(m => m.hilight != HilightType.None))
                yield return (Marker)m;
        }

        public void Clear()
        {
            this.collection.Clear();
        }

        public void RemoveRange(long start, long length)
        {
            if (collection.Count > 0)
            {
                var near_marker_index = 0L;
                var marker_index = collection.TryIndexOfNearst(start, out near_marker_index);
                if(marker_index != -1)
                {
                    collection.RemoveRange(start, length);
                    collection.InsertRange(start, Marker.Create(start, length, HilightType.None));
                }
            }
        }

        public void RemoveAt(long startRow)
        {
            collection.RemoveAt(startRow);
        }

        public void UpdateMarkers(long startIndex, long insertLength, long removeLength)
        {
            if (collection.Count > 0)
            {
                this.RemoveRange(startIndex, removeLength);
                this.UpdateStartIndex(insertLength, startIndex);
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

    public class MarkerRangeCollection : RangeCollectionBase<Marker>
    {
        protected override Marker CreateItem(Marker value, long start = -1, long length = -1)
        {
            return Marker.Create(start, length, value.hilight, value.color, value.isBoldLine);
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
            this.Updated +=new EventHandler((s,e)=>{});
        }

        /// <summary>
        /// 更新されたことを通知します
        /// </summary>
        public event EventHandler Updated;

        internal void Initalize(int id)
        {
            var markers = new MarkerRangeCollection();
            this.collection.Add(id, markers);
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
                markers.RemoveRange(m.start, m.length);
                markers.AddOrInsert(m);
            }
            else
            {
                throw new InvalidOperationException("makers is empty");
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
            else
            {
                throw new InvalidOperationException("makers is empty");
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
            else
            {
                throw new InvalidOperationException("makers is empty");
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
                    if (markers.GetAt(i).hilight == type)
                        markers.RemoveAt(i);
                }
            }
            else
            {
                throw new InvalidOperationException("makers is empty");
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
                foreach (var m in markers)
                    yield return m;
            }
            yield break;
        }

        internal IEnumerable<Marker> Get(int id, long index)
        {
            IRangeCollection<Marker> markers;
            if (this.collection.TryGetValue(id, out markers))
            {
                foreach (var m in markers.GetRanges(index))
                    yield return m;
            }
            yield break;
        }

        internal IEnumerable<Marker> Get(int id, long index, long length)
        {
            IRangeCollection<Marker> markers;
            if (this.collection.TryGetValue(id, out markers))
            {
                foreach (var m in markers.GetRanges(index, length))
                    yield return m;
            }
            yield break;
        }

        /// <summary>
        /// マーカーをすべて削除します
        /// </summary>
        /// <param name="id">マーカーＩＤ</param>
        public void Clear(int id)
        {
            IRangeCollection<Marker> markers;
            if (this.collection.TryGetValue(id, out markers))
                markers.Clear();
            this.Updated(this, null);
        }

        /// <summary>
        /// マーカーをすべて削除します
        /// </summary>
        /// <remarks>Initaizeメソッドを呼び出す必要があります</remarks>
        public void Clear()
        {
            this.collection.Clear();
            this.Updated(this, null);
        }

        internal void UpdateMarkers(long startIndex, long insertLength, long removeLength)
        {
            long deltaLength = insertLength - removeLength;
            foreach (var markers in this.collection.Values)
            {
                markers.UpdateMarkers(startIndex, insertLength, removeLength);
            }
        }

    }
}
