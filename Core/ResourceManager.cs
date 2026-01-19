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
using System.Linq;
using System.Text;

namespace FooEditEngine
{
    class ResourceManager<TKey, TValue>
    {
        Dictionary<TKey, TValue> collection = new Dictionary<TKey, TValue>();
        /// <summary>
        /// 任意のキーに関連付けられている値を取得・設定する
        /// </summary>
        /// <param name="key">キー</param>
        /// <returns>関連付けられている値</returns>
        public TValue this[TKey key]
        {
            get
            {
                return collection[key];
            }
            set
            {
                if (value is IDisposable && collection.ContainsKey(key))
                    ((IDisposable)collection[key]).Dispose();
                collection[key] = value;
            }
        }

        public void Add(TKey key,TValue value)
        {
            collection.Add(key, value);
        }

        public bool TryGetValue(TKey key,out TValue value)
        {
            return collection.TryGetValue(key, out value);
        }

        /// <summary>
        /// すべて削除する
        /// </summary>
        /// <remarks>IDispseableを継承している場合、Dispose()が呼び出されます</remarks>
        public void Clear()
        {
            if (this.collection.Count == 0)
                return;
            TValue first = this.collection.Values.First();
            if (first is IDisposable)
            {
                foreach (IDisposable v in this.collection.Values)
                    v.Dispose();
            }
            collection.Clear();
        }
    }
}
