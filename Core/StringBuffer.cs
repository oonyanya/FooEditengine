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
using FooProject.Collection.DataStore;
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

    class StringBufferSerializer : ISerializeData<IComposableList<char>>
    {
        public IComposableList<char> DeSerialize(byte[] inputData)
        {
            var memStream = new MemoryStream(inputData);
            var reader = new BinaryReader(memStream, Encoding.Unicode);
            var arrayCount = reader.ReadInt32();
            var maxcapacity = reader.ReadInt32();
            var array = new FixedList<char>(arrayCount, maxcapacity);
            array.AddRange(reader.ReadChars(arrayCount));
            return array;
        }

        public byte[] Serialize(IComposableList<char> data)
        {
            FixedList<char> list = (FixedList<char>)data;
            var output = new byte[data.Count * 2 + 4 + 4]; //int32のサイズは4byte、charのサイズ2byte
            var memStream = new MemoryStream(output);
            var writer = new BinaryWriter(memStream, Encoding.Unicode);
            writer.Write(list.Count);
            writer.Write(list.MaxCapacity);
            writer.Write(list.ToArray());
            writer.Close();
            memStream.Dispose();
            return output;
        }
    }

    sealed class StringBuffer : IEnumerable<char>, IRandomEnumrator<char>, IDisposable
    {
        //LOHの都合上、このくらいの値がちょうどいい
        const int BUF_BLOCKSIZE = 32768;
        //ディスクバッファーを使用しないことを示す値
        const int NOUSE_DISKBUFFER_SIZE = -1;

        BigList<char> buf = null;
        const int MaxSemaphoreCount = 1;
        AsyncReaderWriterLock rwlock = new AsyncReaderWriterLock();
        DiskPinableContentDataStore<IComposableList<char>> diskDataStore = null;
        int cacheSize = NOUSE_DISKBUFFER_SIZE;
        string workfile_path = null;

        public StringBuffer(string workfile_path = null,int cache_size = NOUSE_DISKBUFFER_SIZE)
        {
            this.buf = GetBuffer();
            //4以上の値を指定しないとうまく動かないので、それ以外の値はメモリーに保存する
            if (cache_size >= CacheParameters.MINCACHESIZE)
            {
                var serializer = new StringBufferSerializer();
                this.diskDataStore = new DiskPinableContentDataStore<IComposableList<char>>(serializer, workfile_path, cache_size);
                buf.CustomBuilder.DataStore = diskDataStore;
                this.cacheSize = cache_size;
                this.workfile_path = workfile_path;
            }
            this.Update = (s, e) => { };
        }

        public StringBuffer(StringBuffer buffer)
            : this(buffer.workfile_path, buffer.cacheSize)
        {
            System.Diagnostics.Debug.Assert(buffer.cacheSize == this.cacheSize);
            System.Diagnostics.Debug.Assert(buffer.workfile_path == this.workfile_path);
            buf.AddRange(buffer.buf);
        }

        private static BigList<char> GetBuffer()
        {
            var buf = new BigList<char>();
            buf.BlockSize = BUF_BLOCKSIZE;
            //BigList<T>.FIBONACCIに書かれている数値で、Int.MaxValue以外の奴なら設定しても問題はない
            buf.MaxCapacity = (long)1836311903 * (long)BUF_BLOCKSIZE;
            return buf;
        }

        internal int CacheSize
        {
            get { return this.cacheSize; }
        }

        internal string WorkfilePath
        {
            get { return workfile_path; }
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

        public void CopyTo(char[] array, long index, long length)
        {
            var range = this.buf.GetRangeEnumerable(index, length);
            int i = 0;
            foreach(var c in range)
            {
                array[i++] = c;
            }
        }

        public long Length
        {
            get { return this.buf.LongCount; }
        }

        public long Count
        {
            get { return this.buf.LongCount; }
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

        internal void Flush()
        {
            this.diskDataStore.Commit();
        }

        /// <summary>
        /// 文字列を削除する
        /// </summary>
        internal void Clear()
        {
            this.buf.Clear();
        }

        public void Dispose()
        {
            if (this.diskDataStore != null)
            {
                this.diskDataStore.Dispose();
            }
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