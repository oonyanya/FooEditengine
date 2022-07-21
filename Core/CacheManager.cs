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
    class CacheManager<TKey,TValue> : ResourceManager<TKey,TValue>
    {
        Queue<TKey> queque = new Queue<TKey>();
        int maxCount = 100;

        public new void Add(TKey key, TValue value)
        {
            base.Add(key, value);
            if (base.Count >= maxCount)
            {
                TKey overflowedKey = queque.Dequeue();
                base.Remove(overflowedKey);
            }
            queque.Enqueue(key);
        }

        public new void Clear()
        {
            base.Clear();
            queque.Clear();
        }
    }
}
