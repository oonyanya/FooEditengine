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
using FooProject.Collection.DataStore;

namespace FooEditEngine
{
    class ResourceManager<TKey, TValue>
    {
        ICacheList<TKey, TValue> collection = new TwoQueueCacheList<TKey, TValue>();

        public ResourceManager()
        {
            this.collection.CacheOuted += Collection_CacheOuted;
        }

        private void Collection_CacheOuted(CacheOutedEventArgs<TKey, TValue> ev)
        {
            var key = ev.Key;
            var outed_item = ev.Value as IDisposable;
            if (outed_item != null) {
                outed_item.Dispose();
            }
        }

        /// <summary>
        /// 任意のキーに関連付けられている値を取得・設定する
        /// </summary>
        /// <param name="key">キー</param>
        /// <returns>関連付けられている値</returns>
        public TValue this[TKey key]
        {
            get
            {
                TValue item;
                this.collection.TryGet(key,out item);
                return item;
            }
            set
            {
                TValue item;
                this.collection.TryGet(key, out item);
                if (item != null) {
                    ((IDisposable)item).Dispose();
                }
                collection.Set(key, value);
            }
        }

        public void Add(TKey key,TValue value)
        {
            collection.Set(key, value);
        }

        public bool TryGetValue(TKey key,out TValue value)
        {
            return collection.TryGet(key, out value);
        }

        /// <summary>
        /// すべて削除する
        /// </summary>
        /// <remarks>IDispseableを継承している場合、Dispose()が呼び出されます</remarks>
        public void Clear()
        {
            collection.Flush();
        }
    }
}
