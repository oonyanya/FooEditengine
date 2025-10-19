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
    public class UtilTest
    {
        [TestMethod]
        public void IsHasLineFeedTest()
        {
            Assert.AreEqual(true, Util.IsHasNewLine("test\r\n"));
            Assert.AreEqual(true, Util.IsHasNewLine("test\n"));
            Assert.AreEqual(true, Util.IsHasNewLine("test\r"));
            Assert.AreEqual(true, Util.IsHasNewLine("\r"));
            Assert.AreEqual(true, Util.IsHasNewLine("\n"));
            Assert.AreEqual(true, Util.IsHasNewLine("\r\n"));
            Assert.AreEqual(false, Util.IsHasNewLine("a"));
            Assert.AreEqual(false, Util.IsHasNewLine(string.Empty));
        }

        [TestMethod]
        public void GetNewLineLengthInTailTest()
        {
            Assert.AreEqual(2, Util.GetNewLineLengthInTail("test\r\n"));
            Assert.AreEqual(1, Util.GetNewLineLengthInTail("test\n"));
            Assert.AreEqual(1, Util.GetNewLineLengthInTail("test\r"));
            Assert.AreEqual(1, Util.GetNewLineLengthInTail("\r"));
            Assert.AreEqual(1, Util.GetNewLineLengthInTail("\n"));
            Assert.AreEqual(2, Util.GetNewLineLengthInTail("\r\n"));
            Assert.AreEqual(0, Util.GetNewLineLengthInTail("a"));
            Assert.AreEqual(0, Util.GetNewLineLengthInTail(string.Empty));
        }

        [TestMethod]
        public void NormalizeLineFeedTest()
        {
            string str = "test\r\ntest\r\n";
            string expected = str.Replace("\r\n", "\n");
            string result = string.Empty;
            result = Util.NormalizeLineFeed(str,"\n");
            Assert.AreEqual(expected, result);

            str = "test\rtest\r";
            expected = str.Replace("\r", "\n");
            result = Util.NormalizeLineFeed(str, "\n");
            Assert.AreEqual(expected, result);

            str = "test\ntest\n";
            expected = str.Replace("\n", "\n");
            result = Util.NormalizeLineFeed(str, "\n");
            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void SpilitLineFeedTest()
        {
            string str = "test\r\ntest\r\n";
            foreach (var line in Util.SpilitByLineFeed(str))
            {
                Assert.AreEqual("test", line);
            }
            str = "test\rtest\r";
            foreach (var line in Util.SpilitByLineFeed(str))
            {
                Assert.AreEqual("test", line);
            }
            str = "test\ntest\n";
            foreach (var line in Util.SpilitByLineFeed(str))
            {
                Assert.AreEqual("test", line);
            }
        }
    }
    [TestClass]
    public class DocumentTest
    {
        [TestMethod]
        public void InsertSingleLineTest()
        {
            DummyRender render = new DummyRender();
            Document doc = new Document();
            doc.LayoutLines.Render = render;
            DummyView view = new DummyView(doc, render);
            //\nテスト
            doc.Clear();
            doc.Append("a\nb\nc\nd");
            Assert.IsTrue(view.LayoutLines[0] == "a\n" &&
                view.LayoutLines[1] == "b\n" &&
                view.LayoutLines[2] == "c\n" &&
                view.LayoutLines[3] == "d");

            doc.Insert(2, "x");
            Assert.IsTrue(view.LayoutLines[0] == "a\n" &&
                view.LayoutLines[1] == "xb\n" &&
                view.LayoutLines[2] == "c\n" &&
                view.LayoutLines[3] == "d");

            doc.Insert(3, "x");
            Assert.IsTrue(view.LayoutLines[0] == "a\n" &&
                view.LayoutLines[1] == "xxb\n" &&
                view.LayoutLines[2] == "c\n" &&
                view.LayoutLines[3] == "d");

            doc.Insert(6, "x");
            Assert.IsTrue(view.LayoutLines[0] == "a\n" &&
                view.LayoutLines[1] == "xxb\n" &&
                view.LayoutLines[2] == "xc\n" &&
                view.LayoutLines[3] == "d");

            doc.Insert(0, "x");
            Assert.IsTrue(view.LayoutLines[0] == "xa\n" &&
                view.LayoutLines[1] == "xxb\n" &&
                view.LayoutLines[2] == "xc\n" &&
                view.LayoutLines[3] == "d");

            //\rテスト
            doc.Clear();
            doc.Append("a\rb\rc\rd");
            Assert.IsTrue(view.LayoutLines[0] == "a\r" &&
                view.LayoutLines[1] == "b\r" &&
                view.LayoutLines[2] == "c\r" &&
                view.LayoutLines[3] == "d");

            doc.Insert(2, "x");
            Assert.IsTrue(view.LayoutLines[0] == "a\r" &&
                view.LayoutLines[1] == "xb\r" &&
                view.LayoutLines[2] == "c\r" &&
                view.LayoutLines[3] == "d");

            doc.Insert(3, "x");
            Assert.IsTrue(view.LayoutLines[0] == "a\r" &&
                view.LayoutLines[1] == "xxb\r" &&
                view.LayoutLines[2] == "c\r" &&
                view.LayoutLines[3] == "d");

            doc.Insert(6, "x");
            Assert.IsTrue(view.LayoutLines[0] == "a\r" &&
                view.LayoutLines[1] == "xxb\r" &&
                view.LayoutLines[2] == "xc\r" &&
                view.LayoutLines[3] == "d");

            doc.Insert(0, "x");
            Assert.IsTrue(view.LayoutLines[0] == "xa\r" &&
                view.LayoutLines[1] == "xxb\r" &&
                view.LayoutLines[2] == "xc\r" &&
                view.LayoutLines[3] == "d");

            //\r\nテスト
            doc.Clear();
            doc.Append("a\r\nb\r\nc\r\nd");
            Assert.IsTrue(view.LayoutLines[0] == "a\r\n" &&
                view.LayoutLines[1] == "b\r\n" &&
                view.LayoutLines[2] == "c\r\n" &&
                view.LayoutLines[3] == "d");

            doc.Insert(3, "x");
            Assert.IsTrue(view.LayoutLines[0] == "a\r\n" &&
                view.LayoutLines[1] == "xb\r\n" &&
                view.LayoutLines[2] == "c\r\n" &&
                view.LayoutLines[3] == "d");

            doc.Insert(4, "x");
            Assert.IsTrue(view.LayoutLines[0] == "a\r\n" &&
                view.LayoutLines[1] == "xxb\r\n" &&
                view.LayoutLines[2] == "c\r\n" &&
                view.LayoutLines[3] == "d");

            doc.Insert(8, "x");
            Assert.IsTrue(view.LayoutLines[0] == "a\r\n" &&
                view.LayoutLines[1] == "xxb\r\n" &&
                view.LayoutLines[2] == "xc\r\n" &&
                view.LayoutLines[3] == "d");

            doc.Insert(0, "x");
            Assert.IsTrue(view.LayoutLines[0] == "xa\r\n" &&
                view.LayoutLines[1] == "xxb\r\n" &&
                view.LayoutLines[2] == "xc\r\n" &&
                view.LayoutLines[3] == "d");
        }

        [TestMethod]
        public void InsertMultiLineTest()
        {
            DummyRender render = new DummyRender();
            Document doc = new Document();
            doc.LayoutLines.Render = render;
            DummyView view = new DummyView(doc, render);

            //\nテスト
            doc.Clear();
            doc.Append("a\nb\nc\nd");

            doc.Insert(2, "f\ne");
            Assert.IsTrue(
                view.LayoutLines[0] == "a\n" &&
                view.LayoutLines[1] == "f\n" &&
                view.LayoutLines[2] == "eb\n" &&
                view.LayoutLines[3] == "c\n" &&
                view.LayoutLines[4] == "d");

            doc.Insert(3, "g\nh");
            Assert.IsTrue(
                view.LayoutLines[0] == "a\n" &&
                view.LayoutLines[1] == "fg\n" &&
                view.LayoutLines[2] == "h\n" &&
                view.LayoutLines[3] == "eb\n" &&
                view.LayoutLines[4] == "c\n" &&
                view.LayoutLines[5] == "d");

            doc.Insert(0, "x\ny");
            Assert.IsTrue(
                view.LayoutLines[0] == "x\n" &&
                view.LayoutLines[1] == "ya\n" &&
                view.LayoutLines[2] == "fg\n" &&
                view.LayoutLines[3] == "h\n" &&
                view.LayoutLines[4] == "eb\n" &&
                view.LayoutLines[5] == "c\n" &&
                view.LayoutLines[6] == "d");

            //\rテスト
            doc.Clear();
            doc.Append("a\rb\rc\rd");

            doc.Insert(2, "f\re");
            Assert.IsTrue(
                view.LayoutLines[0] == "a\r" &&
                view.LayoutLines[1] == "f\r" &&
                view.LayoutLines[2] == "eb\r" &&
                view.LayoutLines[3] == "c\r" &&
                view.LayoutLines[4] == "d");

            doc.Insert(3, "g\rh");
            Assert.IsTrue(
                view.LayoutLines[0] == "a\r" &&
                view.LayoutLines[1] == "fg\r" &&
                view.LayoutLines[2] == "h\r" &&
                view.LayoutLines[3] == "eb\r" &&
                view.LayoutLines[4] == "c\r" &&
                view.LayoutLines[5] == "d");

            doc.Insert(0, "x\ry");
            Assert.IsTrue(
                view.LayoutLines[0] == "x\r" &&
                view.LayoutLines[1] == "ya\r" &&
                view.LayoutLines[2] == "fg\r" &&
                view.LayoutLines[3] == "h\r" &&
                view.LayoutLines[4] == "eb\r" &&
                view.LayoutLines[5] == "c\r" &&
                view.LayoutLines[6] == "d");

            //\r\nテスト
            doc.Clear();
            doc.Append("a\r\nb\r\nc\r\nd");

            doc.Insert(3, "f\r\ne");
            Assert.IsTrue(
                view.LayoutLines[0] == "a\r\n" &&
                view.LayoutLines[1] == "f\r\n" &&
                view.LayoutLines[2] == "eb\r\n" &&
                view.LayoutLines[3] == "c\r\n" &&
                view.LayoutLines[4] == "d");

            doc.Insert(4, "g\r\nh");
            Assert.IsTrue(
                view.LayoutLines[0] == "a\r\n" &&
                view.LayoutLines[1] == "fg\r\n" &&
                view.LayoutLines[2] == "h\r\n" &&
                view.LayoutLines[3] == "eb\r\n" &&
                view.LayoutLines[4] == "c\r\n" &&
                view.LayoutLines[5] == "d");

            doc.Insert(0, "x\r\ny");
            Assert.IsTrue(
                view.LayoutLines[0] == "x\r\n" &&
                view.LayoutLines[1] == "ya\r\n" &&
                view.LayoutLines[2] == "fg\r\n" &&
                view.LayoutLines[3] == "h\r\n" &&
                view.LayoutLines[4] == "eb\r\n" &&
                view.LayoutLines[5] == "c\r\n" &&
                view.LayoutLines[6] == "d");
        }

        [TestMethod]
        public void RemoveSingleLineTest()
        {
            DummyRender render = new DummyRender();
            Document doc = new Document();
            doc.LayoutLines.Render = render;
            DummyView view = new DummyView(doc, render);

            //\nテスト
            doc.Clear();
            doc.Append("aa\nbb\ncc\ndd");

            doc.Remove(9, 1);
            Assert.IsTrue(
                view.LayoutLines[0] == "aa\n" &&
                view.LayoutLines[1] == "bb\n" &&
                view.LayoutLines[2] == "cc\n" &&
                view.LayoutLines[3] == "d"
                );

            doc.Remove(9, 1);
            Assert.IsTrue(
                view.LayoutLines[0] == "aa\n" &&
                view.LayoutLines[1] == "bb\n" &&
                view.LayoutLines[2] == "cc\n" &&
                view.LayoutLines[3] == ""
                );

            doc.Remove(0, 1);
            Assert.IsTrue(
                view.LayoutLines[0] == "a\n" &&
                view.LayoutLines[1] == "bb\n" &&
                view.LayoutLines[2] == "cc\n" &&
                view.LayoutLines[3] == ""
                );

            //\rテスト
            doc.Clear();
            doc.Append("aa\rbb\rcc\rdd");

            doc.Remove(9, 1);
            Assert.IsTrue(
                view.LayoutLines[0] == "aa\r" &&
                view.LayoutLines[1] == "bb\r" &&
                view.LayoutLines[2] == "cc\r" &&
                view.LayoutLines[3] == "d"
                );

            doc.Remove(9, 1);
            Assert.IsTrue(
                view.LayoutLines[0] == "aa\r" &&
                view.LayoutLines[1] == "bb\r" &&
                view.LayoutLines[2] == "cc\r" &&
                view.LayoutLines[3] == ""
                );

            doc.Remove(0, 1);
            Assert.IsTrue(
                view.LayoutLines[0] == "a\r" &&
                view.LayoutLines[1] == "bb\r" &&
                view.LayoutLines[2] == "cc\r" &&
                view.LayoutLines[3] == ""
                );

            //\r\nテスト
            doc.Clear();
            doc.Append("aa\r\nbb\r\ncc\r\ndd");

            doc.Remove(12, 1);
            Assert.IsTrue(
                view.LayoutLines[0] == "aa\r\n" &&
                view.LayoutLines[1] == "bb\r\n" &&
                view.LayoutLines[2] == "cc\r\n" &&
                view.LayoutLines[3] == "d"
                );

            doc.Remove(12, 1);
            Assert.IsTrue(
                view.LayoutLines[0] == "aa\r\n" &&
                view.LayoutLines[1] == "bb\r\n" &&
                view.LayoutLines[2] == "cc\r\n" &&
                view.LayoutLines[3] == ""
                );

            doc.Remove(0, 1);
            Assert.IsTrue(
                view.LayoutLines[0] == "a\r\n" &&
                view.LayoutLines[1] == "bb\r\n" &&
                view.LayoutLines[2] == "cc\r\n" &&
                view.LayoutLines[3] == ""
                );
        }

        [TestMethod]
        public void RemoveMultiLineTest()
        {
            DummyRender render = new DummyRender();
            Document doc = new Document();
            doc.LayoutLines.Render = render;
            DummyView view = new DummyView(doc, render);

            //\nテスト
            doc.Clear();
            doc.Append("a\n");
            doc.Append("b\n");
            doc.Append("c\n");
            doc.Append("d\n");
            doc.Append("e\n");
            doc.Append("f\n");

            doc.Remove(2, 4);
            Assert.IsTrue(
                view.LayoutLines[0] == "a\n" &&
                view.LayoutLines[1] == "d\n" &&
                view.LayoutLines[2] == "e\n" &&
                view.LayoutLines[3] == "f\n");

            doc.Remove(4, 4);
            Assert.IsTrue(
                view.LayoutLines[0] == "a\n" &&
                view.LayoutLines[1] == "d\n");

            doc.Clear();
            doc.Append("a\n");
            doc.Append("b\n");
            doc.Append("c\n");
            doc.Append("d\n");

            doc.Remove(2, 6);
            Assert.IsTrue(view.LayoutLines[0] == "a\n");

            doc.Clear();
            doc.Append("a\n");
            doc.Append("b\n");
            doc.Append("c\n");
            doc.Append("d\n");
            doc.Append("e\n");
            doc.Append("f\n");
            doc.Insert(4, "a");
            doc.Remove(2, 5);
            Assert.IsTrue(
                view.LayoutLines[0] == "a\n" &&
                view.LayoutLines[1] == "d\n" &&
                view.LayoutLines[2] == "e\n" &&
                view.LayoutLines[3] == "f\n");

            //\rテスト
            doc.Clear();
            doc.Append("a\r");
            doc.Append("b\r");
            doc.Append("c\r");
            doc.Append("d\r");
            doc.Append("e\r");
            doc.Append("f\r");

            doc.Remove(2, 4);
            Assert.IsTrue(
                view.LayoutLines[0] == "a\r" &&
                view.LayoutLines[1] == "d\r" &&
                view.LayoutLines[2] == "e\r" &&
                view.LayoutLines[3] == "f\r");

            doc.Remove(4, 4);
            Assert.IsTrue(
                view.LayoutLines[0] == "a\r" &&
                view.LayoutLines[1] == "d\r");

            doc.Clear();
            doc.Append("a\r");
            doc.Append("b\r");
            doc.Append("c\r");
            doc.Append("d\r");

            doc.Remove(2, 6);
            Assert.IsTrue(view.LayoutLines[0] == "a\r");

            doc.Clear();
            doc.Append("a\r");
            doc.Append("b\r");
            doc.Append("c\r");
            doc.Append("d\r");
            doc.Append("e\r");
            doc.Append("f\r");
            doc.Insert(4, "a");
            doc.Remove(2, 5);
            Assert.IsTrue(
                view.LayoutLines[0] == "a\r" &&
                view.LayoutLines[1] == "d\r" &&
                view.LayoutLines[2] == "e\r" &&
                view.LayoutLines[3] == "f\r");

            //\r\nテスト
            doc.Clear();
            doc.Append("a\r\n");
            doc.Append("b\r\n");
            doc.Append("c\r\n");
            doc.Append("d\r\n");
            doc.Append("e\r\n");
            doc.Append("f\r\n");

            doc.Remove(3, 6);
            Assert.IsTrue(
                view.LayoutLines[0] == "a\r\n" &&
                view.LayoutLines[1] == "d\r\n" &&
                view.LayoutLines[2] == "e\r\n" &&
                view.LayoutLines[3] == "f\r\n");

            doc.Remove(6, 6);
            Assert.IsTrue(
                view.LayoutLines[0] == "a\r\n" &&
                view.LayoutLines[1] == "d\r\n");

            doc.Clear();
            doc.Append("a\r\n");
            doc.Append("b\r\n");
            doc.Append("c\r\n");
            doc.Append("d\r\n");

            doc.Remove(3, 9);
            Assert.IsTrue(view.LayoutLines[0] == "a\r\n");

            doc.Clear();
            doc.Append("a\r\n");
            doc.Append("b\r\n");
            doc.Append("c\r\n");
            doc.Append("d\r\n");
            doc.Append("e\r\n");
            doc.Append("f\r\n");
            doc.Insert(6, "a");
            doc.Remove(3, 7);
            Assert.IsTrue(
                view.LayoutLines[0] == "a\r\n" &&
                view.LayoutLines[1] == "d\r\n" &&
                view.LayoutLines[2] == "e\r\n" &&
                view.LayoutLines[3] == "f\r\n");

        }

        [TestMethod]
        public void QueryTest()
        {
            DummyRender render = new DummyRender();
            Document doc = new Document();
            doc.LayoutLines.Render = render;
            DummyView view = new DummyView(doc, render);

            //\nテスト
            doc.Clear();
            doc.Append("a\nb\nc");

            Assert.IsTrue(view.LayoutLines.GetLongIndexFromLineNumber(1) == 2);
            Assert.IsTrue(view.LayoutLines.GetLengthFromLineNumber(1) == 2);
            Assert.IsTrue(view.LayoutLines.GetLineNumberFromIndex(2) == 1);
            TextPoint tp = view.LayoutLines.GetTextPointFromIndex(2);
            Assert.IsTrue(tp.row == 1 && tp.col == 0);
            Assert.IsTrue(view.LayoutLines.GetLongIndexFromTextPoint(tp) == 2);

            doc.Insert(2, "a");

            Assert.IsTrue(view.LayoutLines.GetLongIndexFromLineNumber(2) == 5);
            Assert.IsTrue(view.LayoutLines.GetLineNumberFromIndex(5) == 2);
            tp = view.LayoutLines.GetTextPointFromIndex(5);
            Assert.IsTrue(tp.row == 2 && tp.col == 0);
            Assert.IsTrue(view.LayoutLines.GetLongIndexFromTextPoint(tp) == 5);

            doc.Insert(0, "a");

            Assert.IsTrue(view.LayoutLines.GetLongIndexFromLineNumber(2) == 6);
            Assert.IsTrue(view.LayoutLines.GetLineNumberFromIndex(6) == 2);
            tp = view.LayoutLines.GetTextPointFromIndex(6);
            Assert.IsTrue(tp.row == 2 && tp.col == 0);
            Assert.IsTrue(view.LayoutLines.GetLongIndexFromTextPoint(tp) == 6);

            //\nテスト
            doc.Clear();
            doc.Append("a\rb\rc");

            Assert.IsTrue(view.LayoutLines.GetLongIndexFromLineNumber(1) == 2);
            Assert.IsTrue(view.LayoutLines.GetLengthFromLineNumber(1) == 2);
            Assert.IsTrue(view.LayoutLines.GetLineNumberFromIndex(2) == 1);
            tp = view.LayoutLines.GetTextPointFromIndex(2);
            Assert.IsTrue(tp.row == 1 && tp.col == 0);
            Assert.IsTrue(view.LayoutLines.GetLongIndexFromTextPoint(tp) == 2);

            doc.Insert(2, "a");

            Assert.IsTrue(view.LayoutLines.GetLongIndexFromLineNumber(2) == 5);
            Assert.IsTrue(view.LayoutLines.GetLineNumberFromIndex(5) == 2);
            tp = view.LayoutLines.GetTextPointFromIndex(5);
            Assert.IsTrue(tp.row == 2 && tp.col == 0);
            Assert.IsTrue(view.LayoutLines.GetLongIndexFromTextPoint(tp) == 5);

            doc.Insert(0, "a");

            Assert.IsTrue(view.LayoutLines.GetLongIndexFromLineNumber(2) == 6);
            Assert.IsTrue(view.LayoutLines.GetLineNumberFromIndex(6) == 2);
            tp = view.LayoutLines.GetTextPointFromIndex(6);
            Assert.IsTrue(tp.row == 2 && tp.col == 0);
            Assert.IsTrue(view.LayoutLines.GetLongIndexFromTextPoint(tp) == 6);

            //\r\nテスト
            doc.Clear();
            doc.Append("a\r\nb\r\nc");

            Assert.IsTrue(view.LayoutLines.GetLongIndexFromLineNumber(1) == 3);
            Assert.IsTrue(view.LayoutLines.GetLengthFromLineNumber(1) == 3);
            Assert.IsTrue(view.LayoutLines.GetLineNumberFromIndex(6) == 2);
            tp = view.LayoutLines.GetTextPointFromIndex(3);
            Assert.IsTrue(tp.row == 1 && tp.col == 0);
            Assert.IsTrue(view.LayoutLines.GetLongIndexFromTextPoint(tp) == 3);

            doc.Insert(3, "a");

            Assert.IsTrue(view.LayoutLines.GetLongIndexFromLineNumber(2) == 7);
            Assert.IsTrue(view.LayoutLines.GetLineNumberFromIndex(7) == 2);
            tp = view.LayoutLines.GetTextPointFromIndex(7);
            Assert.IsTrue(tp.row == 2 && tp.col == 0);
            Assert.IsTrue(view.LayoutLines.GetLongIndexFromTextPoint(tp) == 7);

            doc.Insert(0, "a");

            Assert.IsTrue(view.LayoutLines.GetLongIndexFromLineNumber(2) == 8);
            Assert.IsTrue(view.LayoutLines.GetLineNumberFromIndex(8) == 2);
            tp = view.LayoutLines.GetTextPointFromIndex(8);
            Assert.IsTrue(tp.row == 2 && tp.col == 0);
            Assert.IsTrue(view.LayoutLines.GetLongIndexFromTextPoint(tp) == 8);
        }

        [TestMethod]
        public void FindTest1()
        {
            DummyRender render = new DummyRender();
            Document doc = new Document();
            doc.LayoutLines.Render = render;
            doc.Append("is this a pen");
            doc.SetFindParam("is", false, RegexOptions.None);
            IEnumerator<SearchResult> it = doc.Find();
            it.MoveNext();
            SearchResult sr = it.Current;
            Assert.IsTrue(sr.Start == 0 && sr.End == 1);
            it.MoveNext();
            sr = it.Current;
            Assert.IsTrue(sr.Start == 5 && sr.End == 6);
        }

        [TestMethod]
        public void FindTest2()
        {
            DummyRender render = new DummyRender();
            Document doc = new Document();
            doc.LayoutLines.Render = render;
            doc.Append("is this a pen");
            doc.SetFindParam("is", false, RegexOptions.None);
            IEnumerator<SearchResult> it = doc.Find(3,4);
            it.MoveNext();
            SearchResult sr = it.Current;
            Assert.IsTrue(sr.Start == 5 && sr.End == 6);
        }

        [TestMethod]
        public void FindTest3()
        {
            DummyRender render = new DummyRender();
            Document doc = new Document();
            doc.LayoutLines.Render = render;
            doc.Append("is this a pen");
            doc.SetFindParam("is", false, RegexOptions.None);
            IEnumerator<SearchResult> it = doc.Find(0, 8);
            it.MoveNext();
            SearchResult sr = it.Current;
            doc.Replace(sr.Start, sr.End - sr.Start + 1, "aaa");
            it.MoveNext();
            sr = it.Current;
            Assert.IsTrue(sr.Start == 6 && sr.End == 7);
        }

        [TestMethod]
        public void ReaderTest1()
        {
            DummyRender render = new DummyRender();
            Document doc = new Document();
            doc.LayoutLines.Render = render;
            doc.Append("a");
            DocumentReader reader = doc.CreateReader();
            Assert.IsTrue(reader.Read() == 'a');
            Assert.IsTrue(reader.Peek() == -1);
        }

        [TestMethod]
        public void ReaderTest2()
        {
            DummyRender render = new DummyRender();
            Document doc = new Document();
            doc.LayoutLines.Render = render;
            doc.Append("abc");
            DocumentReader reader = doc.CreateReader();
            char[] buf = new char[2];
            int count = reader.Read(buf,1,2);
            Assert.IsTrue(buf[0] == 'b' && buf[1] == 'c');
            Assert.IsTrue(count == 2);
            Assert.IsTrue(reader.Peek() == -1);
        }

        [TestMethod]
        public void GetLinesText()
        {
            DummyRender render = new DummyRender();
            Document doc = new Document();
            doc.LayoutLines.Render = render;
            //\nテスト
            doc.Append("a\nb\nc");
            var result = doc.GetLines(0, doc.Length - 1).ToArray();
            Assert.AreEqual("a\n", result[0]);
            Assert.AreEqual("b\n", result[1]);
            Assert.AreEqual("c", result[2]);

            //\rテスト
            doc.Clear();
            doc.Append("a\rb\rc");
            result = doc.GetLines(0, doc.Length - 1).ToArray();
            Assert.AreEqual("a\r", result[0]);
            Assert.AreEqual("b\r", result[1]);
            Assert.AreEqual("c", result[2]);

            //\rテスト
            doc.Clear();
            doc.Append("a\r\nb\r\nc");
            result = doc.GetLines(0, doc.Length - 1).ToArray();
            Assert.AreEqual("a\r\n", result[0]);
            Assert.AreEqual("b\r\n", result[1]);
            Assert.AreEqual("c", result[2]);
        }

        [TestMethod]
        public void FetchLineAndTryGetRaw()
        {
            DummyRender render = new DummyRender();
            Document.PreloadLength = 64;
            Document doc = new Document();
            doc.LayoutLines.Render = render;

            //\nテスト
            for (int i = 0; i < 20; i++)
                doc.Append("01234567890123456789\n");

            //普通に追加すると余計なものがあるので、再構築する
            doc.PerformLayout(false);

            LineToIndexTableData lineData;
            bool result = doc.LayoutLines.TryGetRaw(20, out lineData);
            Assert.AreEqual(false, result);
            Assert.AreEqual(null, lineData);

            doc.Append("a\nb\nc");
            doc.LayoutLines.FetchLine(23);
            Assert.AreEqual("a\n", doc.LayoutLines[20]);
            Assert.AreEqual("c", doc.LayoutLines[22]);

            //\rテスト
            doc.Clear();
            for (int i = 0; i < 20; i++)
                doc.Append("01234567890123456789\r");

            //普通に追加すると余計なものがあるので、再構築する
            doc.PerformLayout(false);

            result = doc.LayoutLines.TryGetRaw(20, out lineData);
            Assert.AreEqual(false, result);
            Assert.AreEqual(null, lineData);

            doc.Append("a\rb\rc");
            doc.LayoutLines.FetchLine(23);
            Assert.AreEqual("a\r", doc.LayoutLines[20]);
            Assert.AreEqual("c", doc.LayoutLines[22]);

            //\r\nテスト
            doc.Clear();
            for (int i = 0; i < 20; i++)
                doc.Append("01234567890123456789\r\n");

            //普通に追加すると余計なものがあるので、再構築する
            doc.PerformLayout(false);

            result = doc.LayoutLines.TryGetRaw(20, out lineData);
            Assert.AreEqual(false, result);
            Assert.AreEqual(null, lineData);

            doc.Append("a\r\nb\r\nc");
            doc.LayoutLines.FetchLine(23);
            Assert.AreEqual("a\r\n", doc.LayoutLines[20]);
            Assert.AreEqual("c", doc.LayoutLines[22]);
        }

        [TestMethod]
        public void ReplaceNonRegexAllTest()
        {
            DummyRender render = new DummyRender();
            Document doc = new Document();
            doc.LayoutLines.Render = render;
            //\nテスト
            doc.Append("this is a pen\n");
            doc.Append("this is a pen\n");
            doc.SetCaretPostionWithoutEvent(1, 0);
            doc.SetFindParam("is", false, RegexOptions.None);
            doc.ReplaceAll("aaa", false);
            Assert.IsTrue(doc.ToString(0) == "thaaa aaa a pen\nthaaa aaa a pen\n");
            Assert.AreEqual(1, doc.CaretPostion.row);
            Assert.AreEqual(0, doc.CaretPostion.col);

            //\rテスト
            doc.Clear();
            doc.Append("this is a pen\r");
            doc.Append("this is a pen\r");
            doc.SetCaretPostionWithoutEvent(1, 0);
            doc.SetFindParam("is", false, RegexOptions.None);
            doc.ReplaceAll("aaa", false);
            Assert.IsTrue(doc.ToString(0) == "thaaa aaa a pen\rthaaa aaa a pen\r");
            Assert.AreEqual(1, doc.CaretPostion.row);
            Assert.AreEqual(0, doc.CaretPostion.col);

            //\r\nテスト
            doc.Clear();
            doc.Append("this is a pen\r\n");
            doc.Append("this is a pen\r\n");
            doc.SetCaretPostionWithoutEvent(1, 0);
            doc.SetFindParam("is", false, RegexOptions.None);
            doc.ReplaceAll("aaa", false);
            Assert.IsTrue(doc.ToString(0) == "thaaa aaa a pen\r\nthaaa aaa a pen\r\n");
            Assert.AreEqual(1, doc.CaretPostion.row);
            Assert.AreEqual(0, doc.CaretPostion.col);
        }

        [TestMethod]
        public void ReplaceRegexAllTest()
        {
            DummyRender render = new DummyRender();
            Document doc = new Document();
            doc.LayoutLines.Render = render;
            //\nテスト
            doc.Append("this is a pen\n");
            doc.Append("this is a pen\n");
            doc.SetCaretPostionWithoutEvent(1, 0);
            doc.SetFindParam("[a-z]+", true, RegexOptions.None);
            doc.ReplaceAll("aaa", false);
            Assert.AreEqual("aaa aaa aaa aaa\naaa aaa aaa aaa\n", doc.ToString(0));
            Assert.AreEqual(1, doc.CaretPostion.row);
            Assert.AreEqual(0, doc.CaretPostion.col);

            //\rテスト
            doc.Clear();
            doc.Append("this is a pen\r");
            doc.Append("this is a pen\r");
            doc.SetCaretPostionWithoutEvent(1, 0);
            doc.SetFindParam("[a-z]+", true, RegexOptions.None);
            doc.ReplaceAll("aaa", false);
            Assert.AreEqual("aaa aaa aaa aaa\raaa aaa aaa aaa\r", doc.ToString(0));
            Assert.AreEqual(1, doc.CaretPostion.row);
            Assert.AreEqual(0, doc.CaretPostion.col);

            //\r\nテスト
            doc.Clear();
            doc.Append("this is a pen\r\n");
            doc.Append("this is a pen\r\n");
            doc.SetCaretPostionWithoutEvent(1, 0);
            doc.SetFindParam("[a-z]+", true, RegexOptions.None);
            doc.ReplaceAll("aaa", false);
            Assert.AreEqual("aaa aaa aaa aaa\r\naaa aaa aaa aaa\r\n", doc.ToString(0));
            Assert.AreEqual(1, doc.CaretPostion.row);
            Assert.AreEqual(0, doc.CaretPostion.col);
        }

        [TestMethod]
        public void ReplaceAll2Test()
        {
            //ReplaceAll2()は改行を含めてすべての奴を置き換えるので\nのテストだけで足りる
            const int ADD_COUNT = 3000;

            DummyRender render = new DummyRender();
            Document doc = new Document();
            doc.LayoutLines.Render = render;
            Document.PreloadLength = 64;
            doc.Append("this is a pen\n");
            doc.ReplaceAll2("is", "aaa");
            Assert.IsTrue(doc.ToString(0) == "thaaa aaa a pen\n");

            doc.Clear();

            for (int i = 0; i < ADD_COUNT; i++)
            {
                doc.Append("this is a pen\n");
            }

            doc.LayoutLines.FetchLine(ADD_COUNT);
            doc.SetCaretPostionWithoutEvent(ADD_COUNT - 1, 0, false);

            doc.ReplaceAll2("is", "aaa");

            var lines = doc.LayoutLines.ForEachLines(0, doc.Count - 1);
            foreach(var line in lines)
            {
                var actual =  doc.ToString(line.Item1, line.Item2);
                if(line.Item1 < doc.Count)
                {
                    Assert.AreEqual("thaaa aaa a pen\n", actual);
                }
            }

            Assert.IsTrue(doc.LayoutLines.Count > ADD_COUNT);
            Assert.AreEqual(ADD_COUNT, doc.TotalLineCount);
            Assert.AreEqual(ADD_COUNT - 1, doc.CaretPostion.row);
            Assert.AreEqual(0, doc.CaretPostion.col);
        }

        [TestMethod]
        public void DiskbaseDocumentTest()
        {
            //ほかの所で\rや\r\nテキストはテスト済みなので\nだけで足りる
            const int ADD_COUNT = 3000;
            const string text = "this is a pen.this is a pen.this is a pen.this is a pen.this is a pen.this is a pen.\n";

            DummyRender render = new DummyRender();
            Document olddoc = new Document(4);
            Document doc = new Document(olddoc);
            olddoc.Dispose();
            doc.LayoutLines.Hilighter =new DummyHilighter('.');
            doc.LayoutLines.Render = render;
            for (int i = 0; i < ADD_COUNT; i++)
            {
                doc.Append(text);
            }
            doc.StringBuffer.Flush();
            doc.LayoutLines.Trim();
            doc.LayoutLines.HilightAll(true);
            //最終行は空行なので確かめる必要はない
            long documentIndex = 0;
            for (int i = 0; i < doc.LayoutLines.Count - 1; i++)
            {
                Assert.AreEqual(text, doc.LayoutLines[i]);
                Assert.AreEqual(documentIndex, doc.LayoutLines.GetLongIndexFromLineNumber(i));
                var lineData = doc.LayoutLines.GetRaw(i);
                var syntaxs = lineData.Syntax;
                Assert.AreEqual(6,syntaxs.Length);
                foreach(var syntax in syntaxs)
                {
                    Assert.AreEqual(TokenType.Keyword1, syntax.type);
                }
                documentIndex += lineData.length;
            }

            var replacedText = text.Replace("pen", "ratking");
            doc.ReplaceAll2("pen", "ratking");
            for (int i = 0; i < doc.LayoutLines.Count - 1; i++)
            {
                Assert.AreEqual(replacedText, doc.LayoutLines[i]);
            }
            
            doc.Dispose();
        }

        [TestMethod]
        public void LongLineTest()
        {
            DummyRender render = new DummyRender();
            Document doc = new Document();
            doc.LayoutLines.Render = render;
            for(int i = 0; i < 2; i++)
            {
                int testLength = Document.MaximumLineLength * 2;
                for (int j = 0; j < testLength; j++)
                {
                    doc.Append('a');
                }
                doc.Append(Document.LF_CHAR);
            }
            doc.PerformLayout();

            var layoutLines = doc.LayoutLines;

            Assert.AreEqual(20, layoutLines.GetLineHeight(new TextPoint(0,0)));

            ITextLayout layout = layoutLines.GetLayout(0);
            Point p = layout.GetPostionFromIndex(0);
            Assert.AreEqual(0, p.X);
            Assert.AreEqual(0, p.Y);

            p = layout.GetPostionFromIndex(Document.MaximumLineLength);
            Assert.AreEqual(0, p.X);
            Assert.AreEqual(20, p.Y);

            Assert.AreEqual(0, layout.GetIndexFromPostion(0, 0));

            Assert.AreEqual(Document.MaximumLineLength, layout.GetIndexFromPostion(0, 20));

            Assert.AreEqual(1, layout.GetWidthFromIndex(0));

            Assert.AreEqual(1, layout.GetWidthFromIndex(Document.MaximumLineLength));

            Assert.AreEqual(1,layout.AlignIndexToNearestCluster(0,AlignDirection.Forward));

            Assert.AreEqual(Document.MaximumLineLength + 1, layout.AlignIndexToNearestCluster(Document.MaximumLineLength, AlignDirection.Forward));
        }

        [TestMethod]
        public void MarkerTest()
        {
            DummyRender render = new DummyRender();
            Document doc = new Document();
            doc.LayoutLines.Render = render;
            doc.Append("this is a pen");
            doc.SetMarker(MarkerIDs.Defalut, Marker.Create(0, 4, HilightType.Sold));

            var markers = doc.Markers.Get(MarkerIDs.Defalut);
            foreach(var m in markers)
                Assert.IsTrue(m.start == 0 && m.length == 4);

            doc.SetMarker(MarkerIDs.Defalut, Marker.Create(5, 2, HilightType.Sold));
            doc.RemoveMarker(MarkerIDs.Defalut, 0, 4);
            markers = doc.Markers.Get(MarkerIDs.Defalut);
            foreach (var m in markers)
                Assert.IsTrue(m.start == 5 && m.length == 2);

            doc.Insert(5, "a");
            markers = doc.Markers.Get(MarkerIDs.Defalut, 0);
            foreach (var m in markers)
                Assert.IsTrue(m.start == 6 && m.length == 2);

            doc.Insert(10, "a");
            markers = doc.Markers.Get(MarkerIDs.Defalut, 0);
            foreach (var m in markers)
                Assert.IsTrue(m.start == 6 && m.length == 2);

            doc.SetMarker(MarkerIDs.URL, Marker.Create(0, 4, HilightType.Sold));
            doc.Markers.Clear(MarkerIDs.Defalut);
            foreach (int id in doc.Markers.IDs)
            {
                markers = doc.Markers.Get(id, 0);
                foreach (var m in markers)
                    Assert.IsTrue(m.start == 0 && m.length == 4);
            }
        }

        [TestMethod]
        public void MultiMakerTest()
        {
            DummyRender render = new DummyRender();
            Document doc = new Document();
            doc.LayoutLines.Render = render;
            doc.Append("this");
            doc.SetMarker(MarkerIDs.Defalut, Marker.Create(0, 2, HilightType.Sold));
            doc.SetMarker(MarkerIDs.Defalut, Marker.Create(2, 2, HilightType.Dash));
            var markers = doc.Markers.Get(MarkerIDs.Defalut).ToArray();
            Assert.IsTrue(markers[0].start == 0 && markers[0].length == 2 && markers[0].hilight == HilightType.Sold);
            Assert.IsTrue(markers[1].start == 2 && markers[1].length == 2 && markers[1].hilight == HilightType.Dash);

            doc.SetMarker(MarkerIDs.Defalut, Marker.Create(0, 2, HilightType.Dash));
            doc.SetMarker(MarkerIDs.Defalut, Marker.Create(2, 2, HilightType.Sold));
            markers = doc.Markers.Get(MarkerIDs.Defalut).ToArray();
            Assert.IsTrue(markers[0].start == 0 && markers[0].length == 2 && markers[0].hilight == HilightType.Dash);
            Assert.IsTrue(markers[1].start == 2 && markers[1].length == 2 && markers[1].hilight == HilightType.Sold);
        }

        [TestMethod]
        public void WatchDogTest()
        {
            RegexMarkerPattern dog = new RegexMarkerPattern(new Regex("h[a-z]+"), HilightType.Url,new Color(0,0,0,255));
            IEnumerable<Marker> result = new List<Marker>(){
                Marker.Create(0,4,HilightType.Url,new Color(0,0,0,255)),
                Marker.Create(5,4,HilightType.Url,new Color(0,0,0,255))
            };
            string str = "html haml";

            DummyRender render = new DummyRender();
            Document doc = new Document();
            doc.LayoutLines.Render = render;
            DummyView view = new DummyView(doc, render);
            doc.MarkerPatternSet.Add(MarkerIDs.Defalut, dog);
            doc.Clear();
            doc.Append(str);
            IEnumerable<Marker> actual = doc.MarkerPatternSet.GetMarkers(new CreateLayoutEventArgs(0, str.Length, str));
            this.AreEqual(result, actual);
        }

        [TestMethod]
        public void CallAutoIndentHookWhenEnter()
        {
            bool called = false;
            Document doc = new Document();
            doc.AutoIndentHook += (s, e) => { called = true; };
            doc.Replace(0, 0, doc.NewLine, true);
            Assert.AreEqual(true, called);

            called = false;
            doc.Replace(0, 0, "aaa", true);
            Assert.AreEqual(false, called);
        }

        [TestMethod]
        public void SaveAndLoadFile()
        {
            (string linefeed, string str)[] datas = new (string linefee, string str)[] {
                new ("\n","aaaa\naaaa\naaaa"),
                new ("\r", "aaaa\raaa\raaaa"),
                new ("\r\n", "aaaa\r\naaa\r\naaaa")
            };
            foreach (var content in datas) {
                Document doc = new Document();
                doc.Append(content.str);

                byte[] store = new byte[content.str.Length];
                System.IO.MemoryStream ms = new System.IO.MemoryStream(store);
                System.IO.StreamWriter sw = new System.IO.StreamWriter(ms);
                System.Threading.Tasks.Task t = doc.SaveAsync(sw);
                t.Wait();
                sw.Close();

                doc.Clear();
                ms = new System.IO.MemoryStream(store);
                System.IO.StreamReader sr = new System.IO.StreamReader(ms);
                t = doc.LoadAsync(sr);
                t.Wait();
                sr.Close();

                Assert.AreEqual(2,doc.TotalLineCount);
                Assert.AreEqual(content.linefeed, doc.NewLine);
                Assert.AreEqual(content.str, doc.ToString(0));
            }
        }

        void AreEqual<T>(IEnumerable<T> t1, IEnumerable<T> t2)
        {
            Assert.AreEqual(t1.Count(), t2.Count());

            IEnumerator<T> e1 = t1.GetEnumerator();
            IEnumerator<T> e2 = t2.GetEnumerator();

            while (e1.MoveNext() && e2.MoveNext())
            {
                Assert.AreEqual(e1.Current, e2.Current);
            }
        }
    }
}
