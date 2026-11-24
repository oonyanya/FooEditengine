using System;
using System.Buffers;
using System.Collections.Generic;
using System.Text;
using FooProject.Collection;

namespace FooEditEngine
{
    /// <summary>
    /// ドキュメントの断片を表す
    /// </summary>
    public class DocumentSegment : ReadOnlyBigList<char>
    {
        StringBufferBase _buffer;
        long _index, _count;
        long currentIndex;
        internal DocumentSegment(StringBufferBase buf, long index, long count)
        {
            if (index < 0 || index >= buf.Count)
                throw new ArgumentOutOfRangeException("index");
            if (count < 0 || count > buf.Count - index)
                throw new ArgumentOutOfRangeException("count");

            this._buffer = buf;
            this._index = index;
            this._count = count;
            this.currentIndex = 0;
        }

        /// <summary>
        /// 文字数
        /// </summary>
        public override int Count => (int)_count;

        /// <summary>
        /// 文字数
        /// </summary>
        public long LongCount => _count;

        /// <summary>
        /// インデクサー
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        /// <exception cref="NotSupportedException">設定しようとしたときなどに発生する</exception>
        public override char this[int index]
        {
            get => GetAt(index);
            set => throw new NotSupportedException();
        }

        /// <summary>
        /// 一文字取得する
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public char GetAt(long index)
        {
            long absoulteIndex = index + _index;
            if (absoulteIndex > _buffer.Count)
                throw new ArgumentOutOfRangeException();
            return _buffer[absoulteIndex];
        }

        /// <summary>
        /// 文字列を取得する
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            ReadOnlySequence<char> seq;
            while (this.Read(out seq) > 0)
            {
                var reader = new SequenceReader<char>(seq);
                while(!reader.End)
                {
                    sb.Append(reader.CurrentSpan);
                    reader.Advance(reader.CurrentSpan.Length);
                }
            }
            return sb.ToString();
        }

        /// <summary>
        /// 文字列を読み取る
        /// </summary>
        /// <param name="result">読み取った文字列</param>
        /// <returns>実際に読み取った数を返す</returns>
        public long Read(out ReadOnlySequence<char> result)
        {
            return this.Read(StringBufferBase.BUF_BLOCKSIZE, out result);
        }

        /// <summary>
        /// 文字列を読み取る
        /// </summary>
        /// <param name="count">読み取りたい数</param>
        /// <param name="result">読み取った文字列</param>
        /// <returns>実際に読み取った数を返す</returns>
        public long Read(int count, out ReadOnlySequence<char> result)
        {
            var readIndex = this._index + this.currentIndex;
            var endIndex = this._index + this._count - 1;

            if (readIndex > endIndex)
            {
                result = new ReadOnlySequence<char>();
                return 0;
            }

            var readCount = 0L;
            if (readIndex + count > endIndex)
            {
                readCount = endIndex - readIndex + 1;
            }
            else
            {
                readCount = count;
            }

            if (readCount <= 0)
            {
                result = new ReadOnlySequence<char>();
                return 0;
            }

            result = this._buffer.GetRanges(readIndex, readCount);
            this.currentIndex += readCount;
            return readCount;
        }
    }
}
