/*
 * Copyright (C) 2013 FooProject
 * * This program is free software; you can redistribute it and/or modify it under the terms of the GNU General Public License as published by
 * the Free Software Foundation; either version 3 of the License, or (at your option) any later version.

 * This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of 
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.

You should have received a copy of the GNU General Public License along with this program. If not, see <http://www.gnu.org/licenses/>.
 */

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using FooEditEngine;

namespace UnitTest
{
    [TestClass]
    public class ControllerTest
    {
        [TestMethod]
        public void SelectByWordTest()
        {
            DummyRender render = new DummyRender();
            Document doc = new Document();
            doc.LayoutLines.Render = render;
            EditView view = new EditView(doc, render);
            Controller ctrl = new Controller(doc, view);
            doc.Clear();
            doc.Append("this is a pen");
            doc.SelectWord(0);
            Assert.IsTrue(ctrl.SelectedText == "this");
        }

        [TestMethod]
        public void ConvertToChar()
        {
            DummyRender render = new DummyRender();
            Document doc = new Document();
            doc.LayoutLines.Render = render;
            EditView view = new EditView(doc, render);
            Controller ctrl = new Controller(doc, view);
            doc.Clear();
            doc.Append("U0030");
            doc.Select(0,5);
            ctrl.ConvertToChar();
            Assert.IsTrue(doc.ToString(0) == "0");
        }

        [TestMethod]
        public void ConvertToCodePoint()
        {
            DummyRender render = new DummyRender();
            Document doc = new Document();
            doc.LayoutLines.Render = render;
            EditView view = new EditView(doc, render);
            Controller ctrl = new Controller(doc, view);
            doc.Clear();
            doc.Append("0");
            doc.Select(0, 1);
            ctrl.ConvertToCodePoint();
            Assert.IsTrue(doc.ToString(0) == "U30 ");
        }

        [TestMethod]
        public void CaretTest()
        {
            DummyRender render = new DummyRender();
            Document doc = new Document();
            doc.LayoutLines.Render = render;
            EditView view = new EditView(doc, render);
            Controller ctrl = new Controller(doc, view);
            doc.Clear();
            doc.Append("abc\nef");
            ctrl.JumpCaret(1);
            Assert.IsTrue(ctrl.SelectionStart == 1);
            ctrl.JumpToLineHead(0, false);
            Assert.IsTrue(ctrl.SelectionStart == 0);
            ctrl.JumpToLineEnd(0,false);
            Assert.IsTrue(ctrl.SelectionStart == 3);
            ctrl.JumpToHead(false);
            Assert.IsTrue(ctrl.SelectionStart == 0);
            ctrl.JumpToEnd(false);
            Assert.IsTrue(ctrl.SelectionStart == 4);

            doc.Clear();
            doc.Append("a c\ndef");
            ctrl.JumpCaret(0);
            ctrl.MoveCaretHorizontical(4, false, false);
            Assert.IsTrue(ctrl.SelectionStart == 4);
            ctrl.MoveCaretHorizontical(-4, false, false);
            Assert.IsTrue(ctrl.SelectionStart == 0);
            ctrl.MoveCaretHorizontical(-1, false, false);
            Assert.IsTrue(ctrl.SelectionStart == 0);    //ドキュメントの先端を超えることはないはず
            ctrl.MoveCaretHorizontical(1, false, true);
            Assert.IsTrue(ctrl.SelectionStart == 2);

            ctrl.JumpCaret(0);
            ctrl.MoveCaretVertical(1, false);
            Assert.IsTrue(ctrl.SelectionStart == 4);
            ctrl.MoveCaretVertical(-1, false);
            Assert.IsTrue(ctrl.SelectionStart == 0);
            ctrl.MoveCaretVertical(-1, false);
            Assert.IsTrue(ctrl.SelectionStart == 0);    //ドキュメントの先端を超えることはないはず
        }

        [TestMethod]
        public void LineModeEditTest()
        {
            DummyRender render = new DummyRender();
            Document doc = new Document();
            doc.LayoutLines.Render = render;
            EditView view = new EditView(doc, render);
            Controller ctrl = new Controller(doc, view);
            doc.Clear();
            doc.Append("abc");
            ctrl.JumpCaret(0);
            ctrl.DoDeleteAction();
            Assert.IsTrue(doc.ToString(0) == "bc");
            ctrl.JumpCaret(1);
            ctrl.DoBackSpaceAction();
            Assert.IsTrue(doc.ToString(0) == "c");
            ctrl.DoInputChar('a');
            Assert.IsTrue(doc.ToString(0) == "ac");
            doc.Select(0, 2);
            ctrl.DoInputString("xb");
            Assert.IsTrue(doc.ToString(0) == "xb");
            doc.InsertMode = false;
            ctrl.JumpCaret(0);
            ctrl.DoInputChar('a');
            Assert.IsTrue(doc.ToString(0) == "ab");
            doc.Append("\n");
            ctrl.JumpCaret(2);
            ctrl.DoInputChar('a');
            Assert.IsTrue(doc.LayoutLines[0] == "aba\n");

            doc.Clear();
            doc.Append("a\na");
            doc.Select(0, 3);
            ctrl.UpIndent();
            Assert.IsTrue(doc.ToString(0) == "\ta\n\ta\n");
            ctrl.DownIndent();
            Assert.IsTrue(doc.ToString(0) == "a\na\n");
        }

        [TestMethod]
        public void SelectTest()
        {
            DummyRender render = new DummyRender();
            Document doc = new Document();
            doc.LayoutLines.Render = render;
            EditView view = new EditView(doc, render);
            Controller ctrl = new Controller(doc, view);
            doc.Clear();
            doc.Append("a\nb\nc");
            doc.Select(0, 5);
            Assert.IsTrue(ctrl.SelectedText == "a\r\nb\r\nc");
        }

        [TestMethod]
        public void ReplaceSelectionTest()
        {
            DummyRender render = new DummyRender();
            Document doc = new Document();
            doc.LayoutLines.Render = render;
            EditView view = new EditView(doc, render);
            Controller ctrl = new Controller(doc, view);
            doc.Clear();
            doc.Append("a\nb\nc");
            doc.Select(0, 5);
            ctrl.SelectedText = "a";
            doc.Select(0, 1);
            Assert.IsTrue(ctrl.SelectedText == "a");
        }

        [TestMethod]
        public void SelectByRectTest()
        {
            DummyRender render = new DummyRender();
            Document doc = new Document();
            doc.LayoutLines.Render = render;
            EditView view = new EditView(doc, render);
            Controller ctrl = new Controller(doc, view);
            doc.Clear();
            string str = "aa\nbb\ncc";
            doc.Append(str);
            ctrl.RectSelection = true;
            doc.Select(0,7);
            Assert.IsTrue(ctrl.SelectedText == "a\r\nb\r\nc\r\n");
        }

        [TestMethod]
        public void RectEditTest()
        {
            DummyRender render = new DummyRender();
            Document doc = new Document();
            doc.LayoutLines.Render = render;
            EditView view = new EditView(doc, render);
            Controller ctrl = new Controller(doc, view);
            doc.Clear();
            doc.Append("a\nb\nc");
            ctrl.RectSelection = true;
            doc.Select(0, 5);
            ctrl.DoInputString("x",true);
            Assert.IsTrue(
                view.LayoutLines[0] == "x\n" &&
                view.LayoutLines[1] == "x\n" &&
                view.LayoutLines[2] == "x");
            Assert.IsTrue(
                view.Selections[0].start == 0 &&
                view.Selections[1].start == 2 &&
                view.Selections[2].start == 4);

            ctrl.DoInputString("x", true);
            Assert.IsTrue(
                view.Selections[0].start == 1 &&
                view.Selections[1].start == 4 &&
                view.Selections[2].start == 7);

            doc.Clear();
            doc.Append("a\nb\nc");
            doc.Select(0, 4);
            ctrl.DoInputString("x");
            Assert.IsTrue(
                view.LayoutLines[0] == "xa\n" &&
                view.LayoutLines[1] == "xb\n" &&
                view.LayoutLines[2] == "xc");

            ctrl.DoBackSpaceAction();
            Assert.IsTrue(
                view.LayoutLines[0] == "a\n" &&
                view.LayoutLines[1] == "b\n" &&
                view.LayoutLines[2] == "c");
        }
    }
}
