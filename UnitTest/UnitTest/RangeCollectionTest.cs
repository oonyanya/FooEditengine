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
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using FooEditEngine;

namespace UnitTest
{
    [TestClass]
    public class RangeCollectionTest
    {
        class MyRangeItem : FooProject.Collection.IRange
        {
            public MyRangeItem(long start, long length)
            {
                this.start = start;
                this.length = length;
            }

            public long start
            {
                get;
                set;
            }

            public long length
            {
                get;
                set;
            }

            public FooProject.Collection.IRange DeepCopy()
            {
                var newItem = new MyRangeItem(this.start, this.length);
                return newItem;
            }
        }

        [TestMethod]
        public void AddTest()
        {
            RangeCollection<MyRangeItem> collection = new RangeCollection<MyRangeItem>();
            collection.Add(new MyRangeItem(10, 10));
            collection.Add(new MyRangeItem(1, 10));
            Assert.IsTrue(collection[0].start == 1 && collection[1].start == 10);
        }

        [TestMethod]
        public void ReplaceTest()
        {
            RangeCollection<MyRangeItem> collection = new RangeCollection<MyRangeItem>();
            collection.Add(new MyRangeItem(10, 10));
            collection.Add(new MyRangeItem(0, 10));
            var new_collection = new List<MyRangeItem>();
            new_collection.Add(new MyRangeItem(10, 10));
            new_collection.Add(new MyRangeItem(20, 10));
            collection.ReplaceRange(1, new_collection, 0, 20);
            Assert.IsTrue(
                collection.GetLineHeadIndex(0) == 0 &&
                collection.GetLineHeadIndex(1) == 10 &&
                collection.GetLineHeadIndex(2) == 20 &&
                collection.GetLineHeadIndex(3) == 30
                );
            new_collection = new List<MyRangeItem>();
            new_collection.Add(new MyRangeItem(10, 5));
            new_collection.Add(new MyRangeItem(15, 5));
            collection.ReplaceRange(1, new_collection, 2, -10);
            Assert.IsTrue(
                collection.GetLineHeadIndex(0) == 0 &&
                collection.GetLineHeadIndex(1) == 10 &&
                collection.GetLineHeadIndex(2) == 15 &&
                collection.GetLineHeadIndex(3) == 20
                );
        }

        [TestMethod]
        public void QueryRangeItemTest()
        {
            RangeCollection<MyRangeItem> collection = new RangeCollection<MyRangeItem>();
            collection.Add(new MyRangeItem(1, 10));
            var result = collection.Get(1).ToList();
            Assert.IsTrue(result[0].start == 1 && result[0].length == 10);

            result = collection.Get(0, 20).ToList();
            Assert.IsTrue(result[0].start == 1 && result[0].length == 10);

            collection.Add(new MyRangeItem(15, 10));
            result = collection.Get(0, 20).ToList();
            Assert.IsTrue(result[0].start == 1 && result[0].length == 10);
            Assert.IsTrue(result[1].start == 15 && result[0].length == 10);
        }

        [TestMethod]
        public void RemoveRangeItemTest()
        {
            RangeCollection<MyRangeItem> collection = new RangeCollection<MyRangeItem>();
            collection.Add(new MyRangeItem(1, 10));
            collection.Add(new MyRangeItem(20, 10));

            collection.Remove(0, 15);
            
            var result = collection.ToList();
            Assert.IsTrue(result[0].start == 20 && result[0].length == 10);

            collection.Remove(20,1);
            Assert.IsTrue(collection.Count == 0);
        }
    }
}
