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
    }
}
