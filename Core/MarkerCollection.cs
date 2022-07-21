/*
 * Copyright (C) 2013 FooProject
 * * This program is free software; you can redistribute it and/or modify it under the terms of the GNU General Public License as published by
 * the Free Software Foundation; either version 3 of the License, or (at your option) any later version.

 * This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of 
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.

You should have received a copy of the GNU General Public License along with this program. If not, see <http://www.gnu.org/licenses/>.
 */
using System;
using System.Linq;
using System.Collections.Generic;

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
    public struct Marker : IRange, IEqualityComparer<Marker>
    {
        #region IRange メンバー

        /// <summary>
        /// 開始位置
        /// </summary>
        public int start
        {
            get;
            set;
        }

        /// <summary>
        /// 長さ
        /// </summary>
        public int length
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
        public static Marker Create(int start, int length, HilightType hilight)
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
        public static Marker Create(int start, int length, HilightType hilight,Color color,bool isBoldLine = false)
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

        /// <summary>
        /// ハッシュを得ます
        /// </summary>
        /// <param name="obj">マーカー</param>
        /// <returns>ハッシュ</returns>
        public int GetHashCode(Marker obj)
        {
            return this.start ^ this.length ^ (int)this.hilight;
        }
    }

    /// <summary>
    /// マーカークラスのコレクションを表します
    /// </summary>
    public sealed class MarkerCollection
    {
        Dictionary<int, RangeCollection<Marker>> collection = new Dictionary<int, RangeCollection<Marker>>();

        internal MarkerCollection()
        {
            this.Updated +=new EventHandler((s,e)=>{});
        }

        /// <summary>
        /// 更新されたことを通知します
        /// </summary>
        public event EventHandler Updated;

        internal void Add(int id,Marker m)
        {
            this.AddImpl(id, m);
            this.Updated(this, null);
        }

        void AddImpl(int id, Marker m)
        {
            RangeCollection<Marker> markers;
            if (this.collection.TryGetValue(id, out markers))
            {
                markers.Remove(m.start, m.length);
                markers.Add(m);
            }
            else
            {
                markers = new RangeCollection<Marker>();
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
            RangeCollection<Marker> markers;
            if (this.collection.TryGetValue(id, out markers))
            {
                markers.Clear();
            }
            this.Updated(this, null);
        }

        internal void RemoveAll(int id,int start, int length)
        {
            RangeCollection<Marker> markers;
            if (this.collection.TryGetValue(id, out markers))
            {
                markers.Remove(start, length);
            }
            this.Updated(this, null);
        }

        internal void RemoveAll(int id, HilightType type)
        {
            RangeCollection<Marker> markers;
            if (this.collection.TryGetValue(id, out markers))
            {
                for (int i = 0; i < markers.Count; i++)
                {
                    if (markers[i].hilight == type)
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
            RangeCollection<Marker> markers;
            if (this.collection.TryGetValue(id, out markers))
            {
                foreach (var m in markers)
                    yield return m;
            }
            yield break;
        }

        internal IEnumerable<Marker> Get(int id, int index)
        {
            RangeCollection<Marker> markers;
            if (this.collection.TryGetValue(id, out markers))
            {
                foreach (var m in markers.Get(index))
                    yield return m;
            }
            yield break;
        }

        internal IEnumerable<Marker> Get(int id, int index, int length)
        {
            RangeCollection<Marker> markers;
            if (this.collection.TryGetValue(id, out markers))
            {
                foreach (var m in markers.Get(index, length))
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
            RangeCollection<Marker> markers;
            if (this.collection.TryGetValue(id, out markers))
                markers.Clear();
            this.Updated(this, null);
        }

        /// <summary>
        /// マーカーをすべて削除します
        /// </summary>
        public void Clear()
        {
            this.collection.Clear();
            this.Updated(this, null);
        }

        internal void UpdateMarkers(int startIndex,int insertLength,int removeLength)
        {
            int deltaLength = insertLength - removeLength;
            foreach (RangeCollection<Marker> markers in this.collection.Values)
            {
                for (int i = 0; i < markers.Count; i++)
                {
                    Marker m = markers[i];
                    if (m.start + m.length - 1 < startIndex)
                    {
                        continue;
                    }
                    else
                    {
                        m.start += deltaLength;
                    }
                    markers[i] = m;
                }
            }
        }

    }
}
