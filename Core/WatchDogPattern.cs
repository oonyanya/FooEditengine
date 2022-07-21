/*
 * Copyright (C) 2013 FooProject
 * * This program is free software; you can redistribute it and/or modify it under the terms of the GNU General Public License as published by
 * the Free Software Foundation; either version 3 of the License, or (at your option) any later version.

 * This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of 
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.

You should have received a copy of the GNU General Public License along with this program. If not, see <http://www.gnu.org/licenses/>.
 */
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace FooEditEngine
{
    /// <summary>
    /// IMarkerPatternインターフェイス
    /// </summary>
    public interface IMarkerPattern
    {
        /// <summary>
        /// マーカーを返す
        /// </summary>
        /// <param name="lineHeadIndex">行頭へのインデックスを表す</param>
        /// <param name="s">文字列</param>
        /// <returns>Marker列挙体を返す</returns>
        IEnumerable<Marker> GetMarker(int lineHeadIndex, string s);
    }
    /// <summary>
    /// 正規表現でマーカーの取得を行うクラス
    /// </summary>
    public sealed class RegexMarkerPattern : IMarkerPattern
    {
        Regex regex;
        HilightType type;
        Color color;
        /// <summary>
        /// コンストラクター
        /// </summary>
        /// <param name="regex">regexオブジェクト</param>
        /// <param name="type">ハイライトタイプ</param>
        /// <param name="color">色</param>
        public RegexMarkerPattern(Regex regex,HilightType type,Color color)
        {
            this.regex = regex;
            this.type = type;
            this.color = color;
        }

        /// <summary>
        /// マーカーを返す
        /// </summary>
        /// <param name="lineHeadIndex">行頭へのインデックスを表す</param>
        /// <param name="s">文字列</param>
        /// <returns>Marker列挙体を返す</returns>
        public IEnumerable<Marker> GetMarker(int lineHeadIndex, string s)
        {
            foreach (Match m in this.regex.Matches(s))
            {
                yield return Marker.Create(lineHeadIndex + m.Index, m.Length, this.type,this.color);
            }
        }
    }
    /// <summary>
    /// MarkerPatternセット
    /// </summary>
    public sealed class MarkerPatternSet
    {
        MarkerCollection markers;
        Dictionary<int, IMarkerPattern> watchDogSet = new Dictionary<int, IMarkerPattern>();

        internal MarkerPatternSet(LineToIndexTable lti,MarkerCollection markers)
        {
            this.markers = markers;
        }

        internal IEnumerable<Marker> GetMarkers(CreateLayoutEventArgs e)
        {
            foreach (int id in this.watchDogSet.Keys)
            {
                foreach (Marker m in this.watchDogSet[id].GetMarker(e.Index, e.Content))
                    yield return m;
            }
        }

        internal event EventHandler Updated;

        /// <summary>
        /// WatchDogを追加する
        /// </summary>
        /// <param name="id">マーカーID</param>
        /// <param name="dog">IMarkerPatternインターフェイス</param>
        public void Add(int id, IMarkerPattern dog)
        {
            this.watchDogSet.Add(id, dog);
            this.Updated(this, null);
        }

        /// <summary>
        /// マーカーIDが含まれているかどうかを調べる
        /// </summary>
        /// <param name="id">マーカーID</param>
        /// <returns>含まれていれば真。そうでなければ偽</returns>
        public bool Contains(int id)
        {
            return this.watchDogSet.ContainsKey(id);
        }

        /// <summary>
        /// WatchDogを追加する
        /// </summary>
        /// <param name="id">マーカーID</param>
        public void Remove(int id)
        {
            this.markers.Clear(id);
            this.watchDogSet.Remove(id);
            this.Updated(this, null);
        }
    }
}