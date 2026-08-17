using System;
using System.Collections.Generic;

namespace FooEditEngine
{
    public interface IRangeCollection<T> : IEnumerable<T>
    {
        int Count { get; }

        void Add(T item);
        void Clear();
        T GetAt(long index);
        IEnumerable<T> GetRanges(long index);
        IEnumerable<T> GetRanges(long start, long length);
        void Insert(T item);
        void RemoveRange(long start, long length);
        void RemoveAt(long startRow);
        void UpdateStartIndex(long deltaLength, long startRow);
    }
}