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
    class ResourceManager<TKey, TValue> : Dictionary<TKey, TValue>
    {
        /// <summary>
        /// 任意のキーに関連付けられている値を取得・設定する
        /// </summary>
        /// <param name="key">キー</param>
        /// <returns>関連付けられている値</returns>
        public new TValue this[TKey key]
        {
            get
            {
                return base[key];
            }
            set
            {
                if (value is IDisposable && base.ContainsKey(key))
                    ((IDisposable)base[key]).Dispose();
                base[key] = value;
            }
        }
        /// <summary>
        /// 任意のキーに関連づけられてる値を削除する
        /// </summary>
        /// <param name="key">キー</param>
        /// <returns>IDispseableを継承している場合、Dispose()が呼び出されます</returns>
        public new bool Remove(TKey key)
        {
            TValue value;
            bool result = base.TryGetValue(key, out value);
            if (value is IDisposable)
                ((IDisposable)value).Dispose();
            if (result)
                base.Remove(key);
            return result;
        }
        /// <summary>
        /// すべて削除する
        /// </summary>
        /// <remarks>IDispseableを継承している場合、Dispose()が呼び出されます</remarks>
        public new void Clear()
        {
            if (this.Count == 0)
                return;
            TValue first = this.Values.First();
            if (first is IDisposable)
            {
                foreach (IDisposable v in this.Values)
                    v.Dispose();
            }
            base.Clear();
        }
    }
}
