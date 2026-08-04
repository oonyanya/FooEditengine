using System;
using System.Collections.Generic;

namespace FooEditEngine
{
    public interface IRangeCollection<T> : IEnumerable<T>
    {
        int Count { get; }

        void Add(T item);
        void Clear();
        IEnumerable<T> GetRanges(long start, long length);
        void RemoveRange(long start, long length);
        void RemoveAt(long startRow);
        void UpdateStartIndex(long deltaLength, long startRow);
    }
}