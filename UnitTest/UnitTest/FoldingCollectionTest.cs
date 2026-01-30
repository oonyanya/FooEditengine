/*
 * Copyright (C) 2013 FooProject
 * * This program is free software; you can redistribute it and/or modify it under the terms of the GNU General Public License as published by
 * the Free Software Foundation; either version 3 of the License, or (at your option) any later version.

 * This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of 
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.

You should have received a copy of the GNU General Public License along with this program. If not, see <http://www.gnu.org/licenses/>.
 */
using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using FooEditEngine;

namespace UnitTest
{
    [TestClass]
    public class FoldingCollectionTest
    {
        [TestMethod]
        public void HiddenTest()
        {
            FoldingCollection collection = new FoldingCollection();
            collection.Add(new FoldingItem(0, 10,false));
            collection.Add(new FoldingItem(1, 5));
            Assert.IsTrue(collection.IsHidden(2));
            collection = new FoldingCollection();
            collection.Add(new FoldingItem(0, 10));
            collection.Add(new FoldingItem(1, 5,false));
            Assert.IsTrue(collection.IsHidden(2));
        }
        [TestMethod]
        public void HiddenParrentTest()
        {
            FoldingCollection collection = new FoldingCollection();
            FoldingItem item = new FoldingItem(0, 10, false);
            collection.Add(item);
            FoldingItem item1 = new FoldingItem(1, 5);
            collection.Add(item1);
            Assert.IsTrue(collection.IsParentHidden(item1));
            Assert.IsTrue(!collection.IsParentHidden(item));
        }
        [TestMethod]
        public void HasParrentTest()
        {
            FoldingCollection collection = new FoldingCollection();
            FoldingItem item = new FoldingItem(0, 10, false);
            collection.Add(item);
            FoldingItem item1 = new FoldingItem(1, 5);
            collection.Add(item1);
            Assert.IsTrue(collection.IsHasParent(item1));
            Assert.IsTrue(!collection.IsHasParent(item));
        }
        [TestMethod]
        public void GetFoldingItem()
        {
            FoldingCollection collection = new FoldingCollection();
            collection.Add(new FoldingItem(0, 10));
            collection.Add(new FoldingItem(1, 5));
            collection.Add(new FoldingItem(11, 12));
            FoldingItem item = collection.Get(2, 1);
            Assert.IsTrue(item.Start == 1 && item.End == 5);
        }

        [TestMethod]
        public void GetFarestFoldingItem()
        {
            FoldingCollection collection = new FoldingCollection();
            FoldingItem item = new FoldingItem(0, 10);
            item.Expand = false;
            collection.Add(item);
            collection.Add(new FoldingItem(1, 5));
            collection.Add(new FoldingItem(11,12));
            item = collection.GetFarestHiddenFoldingData(2, 1);
            Assert.IsTrue(item.Start == 0 && item.End == 10);
        }

        [TestMethod]
        public void GetFoldingItems()
        {
            FoldingCollection collection = new FoldingCollection();
            collection.Add(new FoldingItem(0, 10));
            collection.Add(new FoldingItem(1, 5));
            collection.Add(new FoldingItem(11, 12));
            foreach (FoldingItem item in collection.GetRange(0, 10))
                Assert.IsTrue((item.Start != 11 && item.End != 12));
        }

        [TestMethod]
        public void CollapseFoldingItem()
        {
            FoldingCollection collection = new FoldingCollection();
            FoldingItem newItem = new FoldingItem(0,10);
            collection.Add(newItem);
            collection.Add(new FoldingItem(1, 5));
            collection.Add(new FoldingItem(11, 12));
            collection.Collapse(newItem);
            foreach (FoldingItem item in collection.GetRange(0, 10))
                Assert.IsFalse(item.Expand);
        }

        [TestMethod]
        public void ExpandFoldingItem()
        {
            FoldingCollection collection = new FoldingCollection();
            collection.Add(new FoldingItem(0, 10));
            FoldingItem newItem = new FoldingItem(1, 5);
            collection.Add(newItem);
            collection.Add(new FoldingItem(11, 12));
            collection.Expand(newItem);
            foreach (FoldingItem item in collection.GetRange(0, 10))
                Assert.IsTrue(item.Expand);
        }

        [TestMethod]
        public void UpdateItems()
        {
            DummyRender render = new DummyRender();
            Document doc = new Document();
            doc.LayoutLines.Render = render;
            FoldingCollection collection = new FoldingCollection();
            collection.Add(new FoldingItem(0, 10));
            collection.Add(new FoldingItem(1, 5));
            collection.Add(new FoldingItem(15, 20));
            collection.Add(new FoldingItem(16, 17));
            collection.UpdateData(doc, 11, 1, 0);
            FoldingItem[] result = collection.GetRange(16, 4).ToArray();
            Assert.IsTrue((result[0].Start == 16 && result[0].End == 21));
            Assert.IsTrue((result[1].Start == 17 && result[1].End == 18));
            collection.UpdateData(doc, 11, 0, 1);
            result = collection.GetRange(16, 4).ToArray();
            Assert.IsTrue((result[0].Start == 15 && result[0].End == 20));
            Assert.IsTrue((result[1].Start == 16 && result[1].End == 17));
        }
    }
}
