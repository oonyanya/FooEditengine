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

namespace FooEditEngine
{
    /// <summary>
    /// 選択領域を表すクラス
    /// </summary>
    struct Selection : IRange,IEqualityComparer<Selection>
    {
        public int start
        {
            get;
            set;
        }

        public int length
        {
            get;
            set;
        }

        public static Selection Create(int start, int length)
        {
            return new Selection { start = start, length = length};
        }

        public bool Equals(Selection x, Selection y)
        {
            return x.length == y.length && x.start == y.start;
        }

        public int GetHashCode(Selection obj)
        {
            return this.start ^ this.length;
        }
    }

    /// <summary>
    /// 選択範囲が更新されたことを通知するデリゲート
    /// </summary>
    /// <param name="sender">送信元クラス</param>
    /// <param name="e">イベントデータ</param>
    public delegate void SelectChangeEventHandler(object sender,EventArgs e);

    /// <summary>
    /// 選択範囲を格納するコレクションを表します
    /// </summary>
    sealed class SelectCollection : IEnumerable<Selection>
    {
        RangeCollection<Selection> collection;

        /// <summary>
        /// コンストラクター
        /// </summary>
        public SelectCollection()
            : this(null)
        {
        }

        /// <summary>
        /// コンストラクター
        /// </summary>
        /// <param name="selections">コレクションを表します</param>
        public SelectCollection(IEnumerable<Selection> selections)
        {
            if (selections != null)
                collection = new RangeCollection<Selection>(selections);
            else
                collection = new RangeCollection<Selection>();
            this.SelectChange += new SelectChangeEventHandler((s,e)=>{});
        }

        /// <summary>
        /// 選択領域が更新されたことを通知します
        /// </summary>
        public event SelectChangeEventHandler SelectChange;

        /// <summary>
        /// インデクサー
        /// </summary>
        /// <param name="i">0から始まるインデックス</param>
        /// <returns>選択領域を返します</returns>
        public Selection this[int i]
        {
            get
            {
                return this.collection[i];
            }
            set
            {
                this.collection[i] = value;
            }
        }

        /// <summary>
        /// 追加します
        /// </summary>
        /// <param name="sel">選択領域</param>
        public void Add(Selection sel)
        {
            this.collection.Add(sel);
            this.SelectChange(this, null);
        }

        /// <summary>
        /// すべて削除します
        /// </summary>
        public void Clear()
        {
            this.collection.Clear();
            this.SelectChange(this, null);
        }

        /// <summary>
        /// 要素を取得します
        /// </summary>
        /// <param name="index">インデックス</param>
        /// <param name="length">長さ</param>
        /// <returns>要素を表すイテレーター</returns>
        public IEnumerable<Selection> Get(int index, int length)
        {
            return this.collection.Get(index, length);
        }

        /// <summary>
        /// 格納されている選択領域の数を返します
        /// </summary>
        public int Count
        {
            get
            {
                return this.collection.Count;
            }
        }

        #region IEnumerable<Selection> メンバー

        /// <summary>
        /// 列挙子を返します
        /// </summary>
        /// <returns>IEnumeratorオブジェクトを返す</returns>
        public IEnumerator<Selection> GetEnumerator()
        {
            for (int i = 0; i < this.collection.Count; i++)
                yield return this.collection[i];
        }

        #endregion

        #region IEnumerable メンバー

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
