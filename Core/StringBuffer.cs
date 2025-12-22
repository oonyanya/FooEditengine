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
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Pipelines;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using FooProject.Collection;
using FooProject.Collection.DataStore;
using Nito.AsyncEx;

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

    abstract class StringBufferBase : IEnumerable<char>, IRandomEnumrator<char>, IDisposable
    {
        //LOHの都合上、このくらいの値がちょうどいい
        const int BUF_BLOCKSIZE = 32768;

        const int MaxSemaphoreCount = 1;
        AsyncReaderWriterLock rwlock = new AsyncReaderWriterLock();
        protected BigList<char> buf { get; private set; }
        protected CharReader Reader { get; set; }

        public StringBufferBase()
        {
            this.Update = (s, e) => { };
        }

        public DocumentBufferType BufferType { get; protected set; }

        protected void Init()
        {
            this.buf = new BigList<char>();
            this.buf.BlockSize = BUF_BLOCKSIZE;
            //BigList<T>.FIBONACCIに書かれている数値で、Int.MaxValue以外の奴なら設定しても問題はない
            this.buf.MaxCapacity = (long)1836311903 * (long)BUF_BLOCKSIZE;            
        }

        public char this[long index]
        {
            get
            {
                if (buf == null)
                    throw new InvalidOperationException("must be call Init");
                char c = buf.Get(index);
                return c;
            }
        }

        public string ToString(long index, long length)
        {
            if (buf == null)
                throw new InvalidOperationException("must be call Init");
            StringBuilder temp = new StringBuilder();
            temp.Clear();
            for (long i = index; i < index + length; i++)
                temp.Append(buf.Get(i));
            return temp.ToString();
        }

        public void CopyTo(char[] array, long index, long length)
        {
            if (buf == null)
                throw new InvalidOperationException("must be call Init");
            var range = this.buf.GetRangeEnumerable(index, length);
            int i = 0;
            foreach (var c in range)
            {
                array[i++] = c;
            }
        }

        public long Length
        {
            get {
                if (buf == null)
                    throw new InvalidOperationException("must be call Init");
                return this.buf.LongCount;
            }
        }

        public long Count
        {
            get {
                if (buf == null)
                    throw new InvalidOperationException("must be call Init");
                return this.buf.LongCount;
            }
        }

        internal DocumentUpdateEventHandler Update;

        internal void Allocate(long count)
        {
        }

        internal virtual StringBufferBase Clone()
        {
            throw new NotImplementedException();
        }

        internal virtual void Replace(StringBufferBase buf)
        {
            if(this.buf != null)
            {
                this.Clear();
            }
            this.buf = buf.buf;
            this.buf.BlockSize = buf.buf.BlockSize;
            this.buf.MaxCapacity = buf.buf.MaxCapacity;
        }

        internal void AddRange(IEnumerable<char> chars)
        {
            if (buf == null)
                throw new InvalidOperationException("must be call Init");
            this.buf.AddRange(chars);
        }

        internal void InsertRange(long index, IEnumerable<char> chars)
        {
            if (buf == null)
                throw new InvalidOperationException("must be call Init");
            this.buf.InsertRange(index, chars);
        }

        internal void RemoveRange(long index, long length)
        {
            if (buf == null)
                throw new InvalidOperationException("must be call Init");
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
            if (buf == null)
                throw new InvalidOperationException("must be call Init");
            this.buf.Clear();
        }

        protected virtual IComposableList<char> CreateList(OnLoadAsyncResult<IEnumerable<char>> load_result, int read_count)
        {
            var str = new FixedList<char>(read_count, read_count);
            str.AddRange(load_result.Value);

            return str;
        }

        protected virtual IPinableContainer<IComposableList<char>> CreatePin(IComposableList<char> list, OnLoadAsyncResult<IEnumerable<char>> load_result)
        {
            var pinableContent = this.buf.CustomBuilder.DataStore.CreatePinableContainer(list);
            return pinableContent;
        }

        internal virtual async Task LoadAsync(Stream stream, Encoding enc,Func<IComposableList<char>,Task<bool>> action_async = null, long load_len_bytes = -1, int buffer_size = -1)
        {
            var reader = new CharReader(stream, enc, buffer_size);

            long left_load_len = load_len_bytes;
            while (left_load_len == -1 || left_load_len >= 0) {
                var readResult = await reader.LoadAsync(this.buf.BlockSize);
                if (readResult.Value == null)
                    break;

                if(left_load_len >= 0)
                    left_load_len -= readResult.ReadBytes;

                var str = CreateList(readResult,this.buf.BlockSize);

                if (action_async != null)
                {
                    //０以下の値なら中断する
                    if (await action_async(str) == false)
                        break;
                }

                var pinableContent = this.CreatePin(str,readResult);

                using (await this.GetWriterLockAsync())
                {
                    this.buf.Add(pinableContent);
                }
            }

            this.Reader = reader;
        }

        internal IEnumerable<char> GetEnumerator(long start, long length)
        {
            if (buf == null)
                throw new InvalidOperationException("must be call Init");
            return this.buf.GetRangeEnumerable(start, length);
        }

        #region IEnumerable<char> メンバー

        public IEnumerator<char> GetEnumerator()
        {
            if (buf == null)
                throw new InvalidOperationException("must be call Init");
            for (long i = 0; i < this.Length; i++)
                yield return this.buf.Get(i);
        }

        #endregion

        #region IEnumerable メンバー

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            if (buf == null)
                throw new InvalidOperationException("must be call Init");
            for (long i = 0; i < this.Length; i++)
                yield return this.buf.Get(i);
        }

        #endregion

        internal virtual void Flush()
        {
        }

        public virtual void Dispose()
        {
        }
    }

    sealed class StringBuffer : StringBufferBase
    {
        public StringBuffer() : base()
        {
            this.BufferType = DocumentBufferType.Memory;
            this.Init();
        }

        internal override StringBufferBase Clone()
        {
            var newbuf = new StringBuffer();
            newbuf.buf.AddRange(this.buf);
            return newbuf;
        }
    }

    sealed class DiskBaseStringBuffer : StringBufferBase
    {
        //ディスクバッファーを使用しないことを示す値
        const int NOUSE_DISKBUFFER_SIZE = -1;

        DiskPinableContentDataStore<IComposableList<char>> diskDataStore = null;
        int cacheSize = NOUSE_DISKBUFFER_SIZE;
        string workfile_path = null;

        public DiskBaseStringBuffer(string workfile_path = null,int cache_size = NOUSE_DISKBUFFER_SIZE) : base()
        {
            this.BufferType = DocumentBufferType.Disk;
            this.Init(workfile_path, cache_size);
        }

        private void Init(string workfile_path = null, int cache_size = NOUSE_DISKBUFFER_SIZE)
        {
            base.Init();
            //4以上の値を指定しないとうまく動かないので、それ以外の値はメモリーに保存する
            if (cache_size >= CacheParameters.MINCACHESIZE)
            {
                var serializer = new StringBufferSerializer();
                this.diskDataStore = DiskPinableContentDataStore<IComposableList<char>>.Create(serializer, workfile_path, cache_size);
                buf.CustomBuilder.DataStore = diskDataStore;
                this.cacheSize = cache_size;
                this.workfile_path = workfile_path;
            }
        }

        internal int CacheSize
        {
            get { return this.cacheSize; }
        }

        internal string WorkfilePath
        {
            get { return workfile_path; }
        }

        internal override StringBufferBase Clone()
        {
            var newbuf = new DiskBaseStringBuffer(this.workfile_path,this.cacheSize);
            newbuf.buf.AddRange(this.buf);
            System.Diagnostics.Debug.Assert(newbuf.cacheSize == this.cacheSize);
            System.Diagnostics.Debug.Assert(newbuf.workfile_path == this.workfile_path);
            return newbuf;
        }

        internal override void Flush()
        {
            this.diskDataStore.Commit();
        }

        public override void Dispose()
        {
            if (this.diskDataStore != null)
            {
                this.diskDataStore.Dispose();
            }
        }
    }

    sealed class FileMappingStringBuffer : StringBufferBase
    {
        //ディスクバッファーを使用しないことを示す値
        const int NOUSE_CACHE_SIZE = -1;

        ReadOnlyCharDataStore readOnlyCharDataStore = null;
        DiskPinableContentDataStore<IComposableList<char>> diskDataStore = null;
        int diskCacheSize = NOUSE_CACHE_SIZE;
        int mappingCacheSize = NOUSE_CACHE_SIZE;
        string workfile_path = null;

        public FileMappingStringBuffer(string workfile_path = null, int mapping_cache_size = NOUSE_CACHE_SIZE, int disk_cache_size = NOUSE_CACHE_SIZE) : base()
        {
            this.Init(workfile_path, mapping_cache_size, disk_cache_size);
        }

        private void Init(string workfile_path = null, int mapping_cache_size = NOUSE_CACHE_SIZE, int disk_cache_size = NOUSE_CACHE_SIZE)
        {
            base.Init();
            if(mapping_cache_size > CacheParameters.MINCACHESIZE)
            {
                this.readOnlyCharDataStore = new ReadOnlyCharDataStore(null, mapping_cache_size);
                this.mappingCacheSize = mapping_cache_size;
            }
            else
            {
                this.readOnlyCharDataStore = new ReadOnlyCharDataStore(null);
            }
            //4以上の値を指定しないとうまく動かないので、それ以外の値はメモリーに保存する
            if (disk_cache_size >= CacheParameters.MINCACHESIZE)
            {
                var serializer = new StringBufferSerializer();
                this.diskDataStore = DiskPinableContentDataStore<IComposableList<char>>.Create(serializer, workfile_path, disk_cache_size);
                this.readOnlyCharDataStore.SecondaryDataStore = this.diskDataStore;
                this.diskCacheSize = disk_cache_size;
                this.workfile_path = workfile_path;
                this.BufferType = DocumentBufferType.Disk | DocumentBufferType.FileMapping;
            }
            else
            {
                this.readOnlyCharDataStore.SecondaryDataStore = new MemoryPinableContentDataStore<IComposableList<char>>();
                this.BufferType = DocumentBufferType.Memory | DocumentBufferType.FileMapping;
            }
            buf.CustomBuilder.DataStore = this.readOnlyCharDataStore;
        }

        protected override IComposableList<char> CreateList(OnLoadAsyncResult<IEnumerable<char>> load_result, int read_count)
        {
            var str = new ReadOnlyComposableList<char>(load_result.Value);
            return str;
        }

        protected override IPinableContainer<IComposableList<char>> CreatePin(IComposableList<char> list,OnLoadAsyncResult<IEnumerable<char>> load_result)
        {
            var newResult = OnLoadAsyncResult<IComposableList<char>>.Create(list, load_result);

            var pinableContent = this.readOnlyCharDataStore.Load(newResult);

            return pinableContent;
        }

        internal async override Task LoadAsync(Stream stream, Encoding enc, Func<IComposableList<char>, Task<bool>> action_async = null, long load_len_bytes = -1, int buffer_size = -1)
        {
            await base.LoadAsync(stream, enc, action_async,load_len_bytes, buffer_size);
            this.readOnlyCharDataStore.Reader = this.Reader;
        }

        internal override StringBufferBase Clone()
        {
            throw new NotSupportedException("ファイルマッピングモード時の複製はサポートされていません");
        }

        internal int CacheSize
        {
            get { return this.diskCacheSize; }
        }

        internal string WorkfilePath
        {
            get { return workfile_path; }
        }

    }

}