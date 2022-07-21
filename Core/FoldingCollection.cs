/*
 * Copyright (C) 2013 FooProject
 * * This program is free software; you can redistribute it and/or modify it under the terms of the GNU General Public License as published by
 * the Free Software Foundation; either version 3 of the License, or (at your option) any later version.

 * This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of 
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.

You should have received a copy of the GNU General Public License along with this program. If not, see <http://www.gnu.org/licenses/>.
 */
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FooEditEngine
{
    /// <summary>
    /// 新しく作成されるフォールティングアイテムを表す
    /// </summary>
    public class FoldingItem : IRangeProvider<int>
    {
        /// <summary>
        /// 開始インデックス
        /// </summary>
        public int Start
        {
            get
            {
                return this.Range.From;
            }
        }

        /// <summary>
        /// 終了インデックス
        /// </summary>
        public int End
        {
            get
            {
                return this.Range.To;
            }
        }

        /// <summary>
        /// 展開されているなら真。そうでないなら偽
        /// </summary>
        public bool Expand
        {
            get;
            internal set;
        }

        /// <summary>
        /// 内部で使用しているメンバーです。外部から参照しないでください
        /// </summary>
        public Range<int> Range
        {
            get;
            set;
        }

        /// <summary>
        /// コンストラクター
        /// </summary>
        /// <param name="start">開始インデックス</param>
        /// <param name="end">終了インデックス</param>
        /// <param name="expand">展開フラグ</param>
        public FoldingItem(int start, int end,bool expand = true)
        {
            if (start >= end)
                throw new ArgumentException("start < endである必要があります");
            this.Range = new Range<int>(start, end);
            this.Expand = expand;
        }

        internal bool IsFirstLine(LineToIndexTable layoutLines, int row)
        {
            int firstRow = layoutLines.GetLineNumberFromIndex(this.Start);
            return row == firstRow;
        }
    }

    sealed class RangeItemComparer : IComparer<FoldingItem>
    {
        public int Compare(FoldingItem x, FoldingItem y)
        {
            return x.Range.CompareTo(y.Range);
        }
    }

    /// <summary>
    /// イベントデーター
    /// </summary>
    public sealed class FoldingItemStatusChangedEventArgs : EventArgs
    {
        /// <summary>
        /// 状態に変化があったアイテム
        /// </summary>
        public FoldingItem Item;
        /// <summary>
        /// コンストラクター
        /// </summary>
        /// <param name="item">FoldingItemオブジェクト</param>
        public FoldingItemStatusChangedEventArgs(FoldingItem item)
        {
            this.Item = item;
        }
    }

    /// <summary>
    /// 折り畳み関係のコレクションを表す
    /// </summary>
    public sealed class FoldingCollection : IEnumerable<FoldingItem>
    {
        RangeTree<int, FoldingItem> collection = new RangeTree<int, FoldingItem>(new RangeItemComparer());

        internal FoldingCollection()
        {
            this.collection.AutoRebuild = false;
            this.StatusChanged += (s, e) => { };
        }

        internal void UpdateData(Document doc,int startIndex,int insertLength,int removeLength)
        {
            if (this.collection.Count == 0)
                return;
            int delta = insertLength - removeLength;
            foreach (FoldingItem item in this.collection.Items)
            {
                int endIndex = startIndex + removeLength - 1;
                if (startIndex <= item.Start)
                {
                    if ((endIndex >= item.Start && endIndex <= item.End) || endIndex > item.End)
                        item.Range = new Range<int>(item.Start, item.Start);    //ここで削除すると例外が発生する
                    else
                        item.Range = new Range<int>(item.Start + delta, item.End + delta);
                }
                else if (startIndex >= item.Start && startIndex <= item.End)
                {
                    if (endIndex > item.End)
                        item.Range = new Range<int>(item.Start, item.Start);    //ここで削除すると例外が発生する
                    else
                        item.Range = new Range<int>(item.Start, item.End + delta);
                }
            }
            this.collection.Rebuild();
        }

        internal void CollectEmptyFolding(int startIndex,int endIndex)
        {
            foreach (FoldingItem foldingData in this.GetRange(startIndex, endIndex - startIndex + 1))
                if (foldingData.Start == foldingData.End)
                    this.Remove(foldingData);
        }

        /// <summary>
        /// 状態が変わったことを表す
        /// </summary>
        public event EventHandler<FoldingItemStatusChangedEventArgs> StatusChanged;

        /// <summary>
        /// 折り畳みを追加する
        /// </summary>
        /// <param name="data">FoldingItemオブジェクト</param>
        public void Add(FoldingItem data)
        {
            foreach (FoldingItem item in this.collection.Items)
            {
                if (item.Start == data.Start && item.End == data.End)
                    return;
            }
            this.collection.Add(data);
        }

        /// <summary>
        /// 折り畳みを追加する
        /// </summary>
        /// <param name="collection">FoldingItemのコレクション</param>
        public void AddRange(IEnumerable<FoldingItem> collection)
        {
            foreach (FoldingItem data in collection)
            {
                this.Add(data);
            }
        }
        
        /// <summary>
        /// 折り畳みを削除する
        /// </summary>
        /// <param name="data">FoldingItemオブジェクト</param>
        public void Remove(FoldingItem data)
        {
            this.collection.Remove(data);
        }

        /// <summary>
        /// 指定した範囲の折り畳みを取得する
        /// </summary>
        /// <param name="index">開始インデックス</param>
        /// <param name="length">長さ</param>
        /// <returns>FoldingItemイテレーター</returns>
        public IEnumerable<FoldingItem> GetRange(int index, int length)
        {
            if (this.collection.Count == 0)
                yield break;

            this.collection.Rebuild();

            List<FoldingItem> items = this.collection.Query(new Range<int>(index, index + length - 1));
            foreach (FoldingItem item in items)
                yield return item;
        }

        /// <summary>
        /// 指定した範囲に最も近い折り畳みを取得する
        /// </summary>
        /// <param name="index">開始インデックス</param>
        /// <param name="length">長さ</param>
        /// <returns>FoldingItemオブジェクト</returns>
        public FoldingItem Get(int index, int length)
        {
            if (this.collection.Count == 0)
                return null;
            
            this.collection.Rebuild();

            List<FoldingItem> items = this.collection.Query(new Range<int>(index, index + length - 1));

            int minLength = Int32.MaxValue;
            FoldingItem minItem = null;
            foreach (FoldingItem item in items)
                if (index - item.Start < minLength)
                    minItem = item;
            return minItem;
        }

        /// <summary>
        /// すべて削除する
        /// </summary>
        public void Clear()
        {
            this.collection.Clear();
        }

        /// <summary>
        /// 展開状態を一括で変更する
        /// </summary>
        /// <param name="items"></param>
        public void ApplyExpandStatus(IEnumerable<FoldingItem> items)
        {
            foreach(var item in items)
            {
                var target_items = from i in this where i.Start == item.Start && i.End == item.End select i;
                foreach (var target_item in target_items)
                    target_item.Expand = item.Expand;
            }
        }

        /// <summary>
        /// 展開する
        /// </summary>
        /// <param name="foldingData">foldingItemオブジェクト</param>
        /// <remarks>親ノードも含めてすべて展開されます</remarks>
        public void Expand(FoldingItem foldingData)
        {
            if (this.collection.Count == 0)
                return;
            this.collection.Rebuild();
            List<FoldingItem> items = this.collection.Query(foldingData.Range);
            foreach (FoldingItem item in items)
                item.Expand = true;
            this.StatusChanged(this, new FoldingItemStatusChangedEventArgs(foldingData));
        }

        /// <summary>
        /// 折りたたむ
        /// </summary>
        /// <param name="foldingData">foldingItemオブジェクト</param>
        /// <remarks>全ての子ノードは折りたたまれます</remarks>
        public void Collapse(FoldingItem foldingData)
        {
            if (foldingData == null)
                return;
            this.collection.Rebuild();
            List<FoldingItem> items = this.collection.Query(foldingData.Range);
            foldingData.Expand = false;
            foreach (FoldingItem item in items)
                if (item.Start > foldingData.Start && item.End <= foldingData.End)
                    item.Expand = false;
            this.StatusChanged(this, new FoldingItemStatusChangedEventArgs(foldingData));
        }

        /// <summary>
        /// インデックスを含むノードが折りたたまれているかを判定する
        /// </summary>
        /// <param name="index">インデックス</param>
        /// <returns>折りたたまれていれば真を返す。そうでない場合・ノードが存在しない場合は偽を返す</returns>
        public bool IsHidden(int index)
        {
            this.collection.Rebuild();
            List<FoldingItem> items = this.collection.Query(index);
            if (items.Count == 0)
                return false;
            int hiddenCount = items.Count((item) =>{
                return !item.Expand && index > item.Start && index <= item.End;
            });
            return hiddenCount > 0;
        }

        /// <summary>
        /// 親ノードが隠されているかどうかを判定する
        /// </summary>
        /// <param name="foldingItem">判定したいノード</param>
        /// <returns>隠されていれば真を返す</returns>
        public bool IsParentHidden(FoldingItem foldingItem)
        {
            if (foldingItem == null)
                return false;
            this.collection.Rebuild();
            List<FoldingItem> items = this.collection.Query(foldingItem.Range);
            if (items.Count == 0)
                return false;
            int hiddenCount = items.Count((item) =>
            {
                //自分自身ノードか
                if (foldingItem.Range.Equals(item.Range))
                    return false;
                //ノードが親かつ隠されているかどうか
                return !item.Expand && item.Start < foldingItem.Start && item.End > foldingItem.End;
            });
            return hiddenCount > 0;
        }

        /// <summary>
        /// 親を持っているか判定する
        /// </summary>
        /// <param name="foldingItem">判定したいノード</param>
        /// <returns>親を持っていれば真を返す</returns>
        public bool IsHasParent(FoldingItem foldingItem)
        {
            if (foldingItem == null)
                return false;
            this.collection.Rebuild();
            List<FoldingItem> items = this.collection.Query(foldingItem.Range);
            if (items.Count == 0 || items.Count == 1)
                return false;
            int parentItemCount = items.Count((item) => item.Start < foldingItem.Start && item.End > foldingItem.End);
            return parentItemCount > 0;
        }

        /// <summary>
        /// 指定した範囲に属する親ノードを取得する
        /// </summary>
        /// <param name="index">開始インデックス</param>
        /// <param name="length">長さ</param>
        /// <returns>FoldingItemオブジェクト</returns>
        /// <remarks>指定した範囲には属する中で隠された親ノードだけが取得される</remarks>
        public FoldingItem GetFarestHiddenFoldingData(int index, int length)
        {
            if (this.collection.Count == 0)
                return null;
            this.collection.Rebuild();
            List<FoldingItem> items = this.collection.Query(new Range<int>(index, index + length - 1));

            //もっとも範囲の広いアイテムが親を表す
            FoldingItem parentItem = null;
            int max = 0;
            foreach(FoldingItem item in items)
            {
                int item_length = item.End -item.Start + 1;
                if(item_length > max)
                {
                    max = item_length;
                    parentItem = item;
                }
            }

            return parentItem;
        }

        /// <summary>
        /// FlodingItemの列挙子を返す
        /// </summary>
        /// <returns></returns>
        public IEnumerator<FoldingItem> GetEnumerator()
        {
            foreach (var item in this.collection.Items)
                yield return item;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }
    }
}
