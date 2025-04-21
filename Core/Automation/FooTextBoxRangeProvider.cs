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
using System.Text.RegularExpressions;
using System.Threading.Tasks;
#if METRO || WINDOWS_UWP
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Automation;
using Windows.UI.Xaml.Automation.Peers;
using Windows.UI.Xaml.Automation.Provider;
using Windows.UI.Xaml.Automation.Text;
#if METRO
using FooEditEngine.Metro;
#else
using FooEditEngine.UWP;
#endif
#endif
#if WPF
using System.Windows.Automation;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using System.Windows.Automation.Text;
using FooEditEngine;
using FooEditEngine.WPF;
#endif
#if WINUI
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Automation;
using Microsoft.UI.Xaml.Automation.Peers;
using Microsoft.UI.Xaml.Automation.Provider;
using Microsoft.UI.Xaml.Automation.Text;
using FooEditEngine.WinUI;
#endif

namespace FooEditEngine
{
#if ENABLE_AUTMATION
    /// <summary>
    /// A minimal implementation of ITextRangeProvider, used by CustomControl2AutomationPeer
    /// A real implementation is beyond the scope of this sample
    /// </summary>
    sealed class FooTextBoxRangeProvider : ITextRangeProvider
    {
        private FooTextBox textbox;
        private FooTextBoxAutomationPeer _peer;
        private long start, end;

        public FooTextBoxRangeProvider(FooTextBox textbox, FooTextBoxAutomationPeer peer)
            : this(textbox, 0, textbox.Document.Length, peer)
        {
        }
        public FooTextBoxRangeProvider(FooTextBox textbox, long start, long length, FooTextBoxAutomationPeer peer)
        {
            this.textbox = textbox;
            this.start = start;
            this.end = start + length;
            _peer = peer;
        }

        public void AddToSelection()
        {
            throw new InvalidOperationException();
        }

        public ITextRangeProvider Clone()
        {
            return new FooTextBoxRangeProvider(this.textbox, this.start, this.end - this.start, _peer);
        }

        public bool Compare(ITextRangeProvider o)
        {
            FooTextBoxRangeProvider other = o as FooTextBoxRangeProvider;
            if (other == null)
                throw new ArgumentNullException("null以外の値を指定してください");
            if (this.start == other.start && this.end == other.end)
                return true;
            else
                return false;
        }

        public int CompareEndpoints(TextPatternRangeEndpoint endpoint, ITextRangeProvider targetRange, TextPatternRangeEndpoint targetEndpoint)
        {
            FooTextBoxRangeProvider other = targetRange as FooTextBoxRangeProvider;

            if (other == null)
                throw new ArgumentException("");

            if (endpoint == TextPatternRangeEndpoint.Start)
            {
                if (targetEndpoint == TextPatternRangeEndpoint.Start)
                    return this.Compare(this.start, other.start);
                if (targetEndpoint == TextPatternRangeEndpoint.End)
                    return this.Compare(this.start, other.end);
            }
            if (endpoint == TextPatternRangeEndpoint.End)
            {
                if (targetEndpoint == TextPatternRangeEndpoint.Start)
                    return this.Compare(this.start, other.end);
                if (targetEndpoint == TextPatternRangeEndpoint.End)
                    return this.Compare(this.end, other.end);
            }
            throw new ArgumentException("endpointに未知の値が指定されました");
        }

        int Compare(long self, long other)
        {
            if (self < other)
                return -1;
            else if (self > other)
                return 1;
            else
                return 0;
        }

        public void ExpandToEnclosingUnit(TextUnit unit)
        {
            Controller ctrl = this.textbox.Controller;
            Document doc = this.textbox.Document;
            if (unit == TextUnit.Character)
            {
                this.end = this.start + 1;
                if (this.end > doc.Length)
                    this.end = this.start;
                return;
            }
            if (unit == TextUnit.Format || unit == TextUnit.Word)
            {
                var t = doc.GetSepartor(this.start, (c) => Util.IsWordSeparator(c));
                if (t == null)
                    this.start = this.end = 0;
                else
                {
                    this.start = t.Item1;
                    this.end = t.Item2;
                }
                return;
            }
            if (unit == TextUnit.Line || unit == TextUnit.Paragraph)
            {
                var t = doc.GetSepartor(this.start, (c) => c == Document.NewLine);
                if (t == null)
                    this.start = this.end = 0;
                else
                {
                    this.start = t.Item1;
                    this.end = t.Item2;
                }
                return;
            }
            if (unit == TextUnit.Page || unit == TextUnit.Document)
            {
                this.start = 0;
                this.end = this.textbox.Document.Length;
                return;
            }
            throw new NotImplementedException();
        }

        public ITextRangeProvider FindAttribute(int attribute, Object value, bool backward)
        {
            return null;
        }

        public ITextRangeProvider FindText(String text, bool backward, bool ignoreCase)
        {
            if (backward)
                throw new NotImplementedException();
            textbox.Document.SetFindParam(text, false, ignoreCase ? RegexOptions.IgnoreCase : RegexOptions.None);
            IEnumerator<SearchResult> it = textbox.Document.Find();
            if (it.MoveNext())
            {
                SearchResult sr = it.Current;
                return new FooTextBoxRangeProvider(this.textbox, sr.Start, sr.End - sr.Start + 1, _peer);
            }
            return null;
        }

        public Object GetAttributeValue(int attribute)
        {
            return null;
        }

#if METRO || WINDOWS_UWP || WINUI
        public void GetBoundingRectangles(out double[] rectangles)
#endif
#if WPF
        public double[] GetBoundingRectangles()
#endif
        {
            LineToIndexTable layoutLineCollection = this.textbox.LayoutLineCollection;
            TextPoint topLeft = layoutLineCollection.GetTextPointFromIndex(this.start);
            TextPoint bottomRight = this.textbox.LayoutLineCollection.GetTextPointFromIndex(IsNewLine(this.end) ? this.end - 1 : this.end);


#if METRO || WINDOWS_UWP || WINUI
            float dpi;
            Util.GetDpi(out dpi, out dpi);
            double scale = dpi / 96;
            Point topLeftPos = this.textbox.GetPostionFromTextPoint(topLeft);
            Point bottomRightPos = this.textbox.GetPostionFromTextPoint(bottomRight);
            topLeftPos = Util.GetPointInWindow(topLeftPos.Scale(scale), textbox);
            bottomRightPos = Util.GetPointInWindow(bottomRightPos.Scale(scale), textbox);
#endif
#if WPF
            Point topLeftPos = this.textbox.GetPostionFromTextPoint(topLeft);
            Point bottomRightPos = this.textbox.GetPostionFromTextPoint(bottomRight);
            topLeftPos = this.textbox.PointToScreen(topLeftPos);
            bottomRightPos = this.textbox.PointToScreen(bottomRightPos);
#endif

            double width = bottomRightPos.X - topLeftPos.X;
            if (width == 0)
                width = 1;
            Rectangle rect = new Rectangle(topLeftPos.X, topLeftPos.Y,
                 width,
                 bottomRightPos.Y - topLeftPos.Y + layoutLineCollection.GetLineHeight(bottomRight));

#if METRO || WINDOWS_UWP || WINUI
            rectangles = new double[4]{
                rect.X,
                rect.Y,
                rect.Width,
                rect.Height
            };
#endif
#if WPF
            return new double[4]{
                rect.X,
                rect.Y,
                rect.Width,
                rect.Height
            };
#endif
        }

        bool IsNewLine(long index)
        {
            if (this.textbox.Document.Length > 0)
                return this.textbox.Document[index == 0 ? 0 : index - 1] == Document.NewLine;
            else
                return false;
        }

        public IRawElementProviderSimple[] GetChildren()
        {
            return new IRawElementProviderSimple[0];
        }

        public IRawElementProviderSimple GetEnclosingElement()
        {
            return _peer.GetRawElementProviderSimple();
        }

        public String GetText(int maxLength)
        {
            if (this.textbox.Document.Length == 0)
                return "";
            long length = this.end - this.start;
            if (maxLength < 0)
                return this.textbox.Document.ToString(this.start, length);
            else
                return this.textbox.Document.ToString(this.start, (int)Math.Min(length, maxLength));
        }

        public int Move(TextUnit unit, int count)
        {
            if (count == 0)
                return 0;
            Controller ctrl = textbox.Controller;
            LineToIndexTable layoutLine = textbox.LayoutLineCollection;
            int moved = this.MoveEndpointByUnit(TextPatternRangeEndpoint.Start, unit, count);
            this.ExpandToEnclosingUnit(unit);
            return moved;
        }

        public void MoveEndpointByRange(TextPatternRangeEndpoint endpoint, ITextRangeProvider targetRange, TextPatternRangeEndpoint targetEndpoint)
        {
            FooTextBoxRangeProvider other = targetRange as FooTextBoxRangeProvider;

            if (other == null)
                throw new ArgumentException("");

            if (endpoint == TextPatternRangeEndpoint.Start)
            {
                if (targetEndpoint == TextPatternRangeEndpoint.Start)
                    this.start = other.start;
                if (targetEndpoint == TextPatternRangeEndpoint.End)
                    this.start = other.end;
                if (this.start > this.end)
                    this.end = this.start;
                return;
            }
            if (endpoint == TextPatternRangeEndpoint.End)
            {
                if (targetEndpoint == TextPatternRangeEndpoint.Start)
                    this.end = other.start;
                if (targetEndpoint == TextPatternRangeEndpoint.End)
                    this.end = other.end;
                return;
            }
            throw new ArgumentException("endpointに未知の値が指定されました");
        }

        public int MoveEndpointByUnit(TextPatternRangeEndpoint endpoint, TextUnit unit, int count)
        {
            if (count == 0)
                return 0;

            int moved = 0;
            TextPoint caret = TextPoint.Null, newCaret = TextPoint.Null;
            Controller ctrl = textbox.Controller;
            LineToIndexTable layoutLine = textbox.LayoutLineCollection;

            if (endpoint == TextPatternRangeEndpoint.Start)
                caret = layoutLine.GetTextPointFromIndex(this.start);
            else if (endpoint == TextPatternRangeEndpoint.End)
                caret = layoutLine.GetTextPointFromIndex(this.end);

            switch (unit)
            {
                case TextUnit.Character:
                    newCaret = ctrl.GetNextCaret(caret, count, MoveFlow.Character, out moved);
                    break;
                case TextUnit.Format:
                case TextUnit.Word:
                    newCaret = ctrl.GetNextCaret(caret, count, MoveFlow.Word, out moved);
                    break;
                case TextUnit.Line:
                    newCaret = ctrl.GetNextCaret(caret, count, MoveFlow.Line, out moved);
                    break;
                case TextUnit.Paragraph:
                    newCaret = ctrl.GetNextCaret(caret, count, MoveFlow.Paragraph, out moved);
                    break;
                case TextUnit.Page:
                case TextUnit.Document:
                    this.start = 0;
                    this.end = this.textbox.Document.Length - 1;
                    moved = 1;
                    break;
            }

            if (endpoint == TextPatternRangeEndpoint.Start)
            {
                this.start = layoutLine.GetIndexFromTextPoint(newCaret);
                if (this.start > this.end)
                    this.end = this.start;
            }
            else if (endpoint == TextPatternRangeEndpoint.End)
            {
                this.end = layoutLine.GetIndexFromTextPoint(newCaret);
                if (this.end < this.start)
                    this.start = this.end;
            }
            return moved;
        }

        public void RemoveFromSelection()
        {
            throw new InvalidOperationException();
        }

        public void ScrollIntoView(bool alignToTop)
        {
            int row = this.textbox.LayoutLineCollection.GetLineNumberFromIndex(alignToTop ? this.start : this.end);
            this.textbox.ScrollIntoView(row, alignToTop);
        }

        public void Select()
        {
            this.textbox.Select(this.start, this.end - this.start + 1);
        }
    }
#endif
}
