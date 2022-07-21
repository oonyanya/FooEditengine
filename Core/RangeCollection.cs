/* https://github.com/mbuchetics/RangeTree よりコピペ。このファイルのみMITライセンスに従います */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FooEditEngine
{
    /// <summary>
    /// マーカーを表す
    /// </summary>
    internal interface IRange
    {
        /// <summary>
        /// マーカーの開始位置。-1を設定した場合、そのマーカーはレタリングされません
        /// </summary>
        int start { get; set; }
        /// <summary>
        /// マーカーの長さ。0を設定した場合、そのマーカーはレタリングされません
        /// </summary>
        int length { get; set; }
    }

    internal sealed class RangeCollection<T> : IEnumerable<T>
        where T : IRange
    {
        List<T> collection;

        public RangeCollection()
            : this(null)
        {
        }

        public RangeCollection(IEnumerable<T> collection)
        {
            if (collection == null)
                this.collection = new List<T>();
            else
                this.collection = new List<T>(collection);
        }

        public T this[int i]
        {
            get
            {
                return this.collection[i];
            }
            set
            {
                this.collection[i] = value;
            }
        }

        public int Count
        {
            get
            {
                return this.collection.Count;
            }
        }

        public void Add(T item)
        {
            this.collection.Add(item);
            for (int i = this.collection.Count - 1; i >= 0; i--)
            {
                if (i > 0 && this.collection[i].start < this.collection[i - 1].start)
                {
                    T temp = this.collection[i];
                    this.collection[i] = this.collection[i - 1];
                    this.collection[i - 1] = temp;
                }
                else
                {
                    break;
                }
            }
        }

        public void Remove(int start, int length)
        {
            if (this.collection.Count == 0)
                return;

            int at = this.IndexOf(start);

            int endAt = this.IndexOf(start + length - 1);

            if(at != -1 && endAt != -1)
            {
                for (int i = endAt; i >= at; i--)
                {
                    this.collection.RemoveAt(i);
                }
            }
            else if (at != -1)
            {
                this.collection.RemoveAt(at);
            }
            else if(endAt != -1)
            {
                this.collection.RemoveAt(endAt);
            }
        }

        public void RemoveNearest(int start, int length)
        {
            if (this.collection.Count == 0)
                return;
            
            int nearAt;
            int at = this.IndexOfNearest(start, out nearAt);
            if (at == -1)
                at = nearAt;
            
            int nearEndAt;
            int endAt = this.IndexOfNearest(start + length - 1, out nearEndAt);
            if (endAt == -1)
                endAt = nearEndAt;
            
            int end = start + length - 1; 
            for (int i = endAt; i >= at; i--)
            {
                int markerEnd = this.collection[i].start + this.collection[i].length - 1;
                if (this.collection[i].start >= start && markerEnd <= end ||
                    markerEnd >= start && markerEnd <= end ||
                    this.collection[i].start >= start && this.collection[i].start <= end ||
                    this.collection[i].start < start && markerEnd > end)
                    this.collection.RemoveAt(i);
            }
        }

        public void RemoveAt(int index)
        {
            this.collection.RemoveAt(index);
        }

        public int IndexOf(int start)
        {
            int dummy;
            return this.IndexOfNearest(start, out dummy);
        }

        int IndexOfNearest(int start,out int nearIndex)
        {
            nearIndex = -1;
            int left = 0, right = this.collection.Count - 1, mid;
            while (left <= right)
            {
                mid = (left + right) / 2;
                T item = this.collection[mid];
                if (start >= item.start && start < item.start + item.length)
                {
                    return mid;
                }
                if (start < item.start)
                {
                    right = mid - 1;
                }
                else
                {
                    left = mid + 1;
                }
            }
            System.Diagnostics.Debug.Assert(left >= 0 || right >= 0);
            nearIndex = left >= 0 ? left : right;
            if (nearIndex > this.collection.Count - 1)
                nearIndex = right;
            return -1;
        }

        public IEnumerable<T> Get(int index)
        {
            int at = this.IndexOf(index);
            if (at == -1)
                yield break;
            yield return this.collection[at];
        }

        public IEnumerable<T> Get(int start, int length)
        {
            int nearAt;
            int at = this.IndexOfNearest(start,out nearAt);
            if (at == -1)
                at = nearAt;

            if (at == -1)
                yield break;

            int end = start + length - 1;
            for (int i = at; i < this.collection.Count; i++)
            {
                int markerEnd = this.collection[i].start + this.collection[i].length - 1;
                if (this.collection[i].start >= start && markerEnd <= end ||
                    markerEnd >= start && markerEnd <= end ||
                    this.collection[i].start >= start && this.collection[i].start <= end ||
                    this.collection[i].start < start && markerEnd > end)
                    yield return this.collection[i];
                else if (this.collection[i].start > start + length)
                    yield break;
            }
        }

        public void Clear()
        {
            this.collection.Clear();
        }

        public IEnumerator<T> GetEnumerator()
        {
            foreach (T item in this.collection)
                yield return item;
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }
    }

}
