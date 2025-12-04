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
using System.Text.RegularExpressions;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using FooEditEngine;

namespace UnitTest
{
    [TestClass]
    public class ViewBaseTest
    {
        [TestMethod]
        public void GetTextPointFromPostionTest()
        {
            const int TestLineCount = 4;
            const string TestText = "0123456789\n0123456789\n0123456789\n0123456789";

            DummyRender render = new DummyRender();
            Document doc = new Document();
            doc.LayoutLines.Render = render;
            DummyView view = new DummyView(doc, render);
            view.PageBound = new Rectangle(0, 0, 200, TestLineCount * DummyTextLayout.TestLineHeight);
            doc.Clear();
            doc.Append(TestText);

            //DummyTextLayoutなので常に文字の幅と行の幅や高さは固定。以後同じ。
            var result = view.GetTextPointFromPostion(new Point(0, 0), TextPointSearchRange.Full);
            Assert.AreEqual(0, result.row);
            Assert.AreEqual(0, result.col);

            result = view.GetTextPointFromPostion(new Point(25, 0), TextPointSearchRange.Full);
            Assert.AreEqual(0, result.row);
            Assert.AreEqual(1, result.col);

            result = view.GetTextPointFromPostion(new Point(195, 0), TextPointSearchRange.Full);
            Assert.AreEqual(0, result.row);
            Assert.AreEqual(9, result.col);

            result = view.GetTextPointFromPostion(new Point(0, 80), TextPointSearchRange.Full);
            Assert.AreEqual(3, result.row);
            Assert.AreEqual(0, result.col);

            result = view.GetTextPointFromPostion(new Point(195, 80), TextPointSearchRange.Full);
            Assert.AreEqual(3, result.row);
            Assert.AreEqual(9, result.col);

            view.PageBound = new Rectangle(0, 0, 50, 4 * DummyTextLayout.TestLineHeight);
            Document.MaximumLineLength = 5;
            doc.PerformLayout(false);
            doc.LayoutLines.FetchLine(TestLineCount);

            result = view.GetTextPointFromPostion(new Point(0, 0), TextPointSearchRange.Full);
            Assert.AreEqual(0, result.row);
            Assert.AreEqual(0, result.col);

            result = view.GetTextPointFromPostion(new Point(25, 0), TextPointSearchRange.Full);
            Assert.AreEqual(0, result.row);
            Assert.AreEqual(1, result.col);

            result = view.GetTextPointFromPostion(new Point(95, 20), TextPointSearchRange.Full);
            Assert.AreEqual(0, result.row);
            Assert.AreEqual(9, result.col);

            result = view.GetTextPointFromPostion(new Point(0, 180), TextPointSearchRange.Full);
            Assert.AreEqual(3, result.row);
            Assert.AreEqual(0, result.col);

            result = view.GetTextPointFromPostion(new Point(195, 220), TextPointSearchRange.Full);
            Assert.AreEqual(3, result.row);
            Assert.AreEqual(9, result.col);

        }

        [TestMethod]
        public void GetRectFromIndexTest()
        {
            const int TestLineCount = 4;
            const string TestText = "0123456789\n0123456789\n0123456789\n0123456789";

            DummyRender render = new DummyRender();
            Document doc = new Document();
            doc.LayoutLines.Render = render;
            DummyView view = new DummyView(doc, render);
            view.PageBound = new Rectangle(0, 0, 200, TestLineCount * DummyTextLayout.TestLineHeight);
            doc.Clear();
            doc.Append(TestText);
            doc.PerformLayout(false);
            doc.LayoutLines.FetchLine(TestLineCount);
            view.CalculateLineCountOnScreen();

            var result = view.GetRectFromIndex(0,20,20);
            Assert.AreEqual(-10, result.X);
            Assert.AreEqual(20, result.Y);
            Assert.AreEqual(DummyTextLayout.TestCharWidth, result.Width);
            Assert.AreEqual(DummyTextLayout.TestLineHeight, result.Height);
        }

        [TestMethod]
        public void TryPixelScrollTest()
        {
            DummyRender render = new DummyRender();
            Document doc = new Document();
            doc.LayoutLines.Render = render;
            DummyView view = new DummyView(doc, render);
            view.PageBound = new Rectangle(0, 0, 100, 30);
            doc.Clear();
            doc.Append("a\nb\nc\nd");

            bool result;
            result = view.TryScroll(0.0, 30.0);
            Assert.AreEqual(result, false);
            result = view.TryScroll(0.0, 30.0);
            Assert.AreEqual(result, false);
            result = view.TryScroll(0.0, 30.0);
            Assert.AreEqual(result, true);
            result = view.TryScroll(0.0, 30.0);
            Assert.AreEqual(result, true);
            Assert.AreEqual(doc.Src.Row, 3);

            result = view.TryScroll(0.0, -30.0);
            Assert.AreEqual(result, false);
            result = view.TryScroll(0.0, -30.0);
            Assert.AreEqual(result, false);
            result = view.TryScroll(0.0, -30.0);
            Assert.AreEqual(result, true);
            result = view.TryScroll(0.0, -30.0);
            Assert.AreEqual(doc.Src.Row, 0);
            Assert.AreEqual(result, true);
        }

        [TestMethod]
        public void TryRowScrollTest()
        {
            DummyRender render = new DummyRender();
            Document doc = new Document();
            doc.LayoutLines.Render = render;
            DummyView view = new DummyView(doc, render);
            view.PageBound = new Rectangle(0, 0, 100, 30);
            doc.Clear();
            doc.Append("a\nb\nc\nd");

            bool result = view.TryScroll(0, 3);
            Assert.AreEqual(doc.Src.Row, 3);
            Assert.AreEqual(result, false);

            result = view.TryScroll(0, 0);
            Assert.AreEqual(doc.Src.Row, 0);
            Assert.AreEqual(result, false);
        }

        [TestMethod]
        public void GetNearstRowAndOffsetYTest()
        {
            DummyRender render = new DummyRender();
            Document doc = new Document();
            doc.LayoutLines.Render = render;
            DummyView view = new DummyView(doc, render);
            view.PageBound = new Rectangle(0, 0, 100, 30);
            doc.Clear();
            doc.Append("a\nb\nc\nd");
            var r = view.GetNearstRowAndOffsetY(0, 30);
            Assert.AreEqual(r.Row, 1);
            Assert.AreEqual(r.OffsetY, 10);
            r = view.GetNearstRowAndOffsetY(1, -30);
            Assert.AreEqual(r.Row, 0);
            Assert.AreEqual(r.OffsetY, 0);
            r = view.GetNearstRowAndOffsetY(2, -30);
            Assert.AreEqual(r.Row, 0);
            Assert.AreEqual(r.OffsetY, 10);
        }
    }
}
