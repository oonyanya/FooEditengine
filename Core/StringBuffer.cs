/*
 * Copyright (C) 2013 FooProject
 * * This program is free software; you can redistribute it and/or modify it under the terms of the GNU General Public License as published by
 * the Free Software Foundation; either version 3 of the License, or (at your option) any later version.

 * This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of 
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.

You should have received a copy of the GNU General Public License along with this program. If not, see <http://www.gnu.org/licenses/>.
 */
//#define TEST_ASYNC
using System;
using System.IO;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Nito.AsyncEx;
using System.Threading;
using System.Threading.Tasks;
using FooProject.Collection;
using System.Collections;
using System.Runtime.CompilerServices;

namespace FooEditEngine
{
    /// <summary>
    /// ランダムアクセス可能な列挙子を提供するインターフェイス
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IRandomEnumrator<T>
    {
        /// <summary>
        /// インデクサーを表す
        /// </summary>
        /// <param name="index">インデックス</param>
        /// <returns>Tを返す</returns>
        T this[long index] { get; }

        long Count {  get; }
    }

    sealed class StringBuffer : IEnumerable<char>, IRandomEnumrator<char>
    {
        BigList<char> buf = GetBuffer();
        const int MaxSemaphoreCount = 1;
        AsyncReaderWriterLock rwlock = new AsyncReaderWriterLock();

        public StringBuffer()
        {
            this.Update = (s, e) => { };
        }

        public StringBuffer(StringBuffer buffer)
            : this()
        {
            buf.AddRange(buffer.buf);
        }

        public static BigList<char> GetBuffer()
        {
            var buf = new BigList<char>();
            buf.BlockSize = 32768;
            return buf;
        }


        public char this[long index]
        {
            get
            {
                char c = buf.Get(index);
                return c;
            }
        }

        public string ToString(long index, long length)
        {
            StringBuilder temp = new StringBuilder();
            temp.Clear();
            for (long i = index; i < index + length; i++)
                temp.Append(buf.Get(i));
            return temp.ToString();
        }

        public long Length
        {
            get { return this.buf.Count; }
        }

        public long Count
        {
            get { return this.buf.Count; }
        }

        internal DocumentUpdateEventHandler Update;

        internal void Allocate(long count)
        {
        }

        internal void Replace(StringBuffer buf)
        {
            this.Clear();
            this.buf = buf.buf;
        }

        internal void AddRange(IEnumerable<char> chars)
        {
            this.buf.AddRange(chars);
        }

        internal void InsertRange(long index, IEnumerable<char> chars)
        {
            this.buf.InsertRange(index, chars);
        }

        internal void RemoveRange(long index, long length)
        {
            this.buf.RemoveRange(index, length);
        }

        public AwaitableDisposable<IDisposable> GetWriterLockAsync()
        {
            return this.rwlock.WriterLockAsync();
        }

        public IDisposable GetWriterLock()
        {
            return this.rwlock.WriterLock();
        }

        public AwaitableDisposable<IDisposable> GetReaderLockAsync()
        {
            return this.rwlock.ReaderLockAsync();
        }

        public IDisposable GetReaderLock()
        {
            return this.rwlock.ReaderLock();
        }

        internal void OnDocumentUpdate(DocumentUpdateEventArgs e)
        {
            this.Update(this, e);
        }

        /// <summary>
        /// 文字列を削除する
        /// </summary>
        internal void Clear()
        {
            this.buf.Clear();
        }

        internal IEnumerable<char> GetEnumerator(long start, long length)
        {
            return this.buf.GetRangeEnumerable(start, length);
        }

        #region IEnumerable<char> メンバー

        public IEnumerator<char> GetEnumerator()
        {
            for (long i = 0; i < this.Length; i++)
                yield return this.buf.Get(i);
        }

        #endregion

        #region IEnumerable メンバー

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            for (long i = 0; i < this.Length; i++)
                yield return this.buf.Get(i);
        }

        #endregion
    }

}