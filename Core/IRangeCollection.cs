using System;
using System.Collections.Generic;
using FooProject.Collection;

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
        void AddOrInsert(T m);
        void Insert(T item);
        void RemoveRange(long start, long length);
        void RemoveAt(long startRow);
        void UpdateMarkers(long startIndex, long insertLength, long removeLength);
    }
}