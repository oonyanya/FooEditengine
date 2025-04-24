/*
 * Copyright (C) 2013 FooProject
 * * This program is free software; you can redistribute it and/or modify it under the terms of the GNU General Public License as published by
 * the Free Software Foundation; either version 3 of the License, or (at your option) any later version.

 * This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of 
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.

You should have received a copy of the GNU General Public License along with this program. If not, see <http://www.gnu.org/licenses/>.
 */
#if METRO || WPF || WINDOWS_UWP || WINUI
using System.Linq;
using System;

#if METRO || WPF
using DotNetTextStore.UnmanagedAPI.WinDef;
using DotNetTextStore.UnmanagedAPI.TSF;
using DotNetTextStore;
#endif

namespace FooEditEngine
{
    class TextStoreHelper
    {
        long startImeDocumentIndex = 0;
        Controller controller;

        public TextStoreHelper(Controller c)
        {
            controller = c;
        }

        public bool IsAllowFullDocument()
        {
#if DISABLE_DOCUMENTFEED
            return false;
#else
            if (controller.Document.Length < Int32.MaxValue - 1)
                return true;
            else
                return false;
#endif
        }

        public bool StartCompstion()
        {
            var document = controller.Document;
            document.UndoManager.BeginUndoGroup();
            if(IsAllowFullDocument())
                this.startImeDocumentIndex = 0;
            else
                this.startImeDocumentIndex = document.LayoutLines.GetLongIndexFromLineNumber(document.CaretPostion.row);
            return true;
        }

        public void EndCompostion()
        {
            var document = controller.Document;
            document.UndoManager.EndUndoGroup();
            if (IsAllowFullDocument())
                this.startImeDocumentIndex = 0;
            else
                this.startImeDocumentIndex = document.LayoutLines.GetLongIndexFromLineNumber(document.CaretPostion.row);
        }

        public int ImeDoumentLength
        {
            get {
                return IsAllowFullDocument() ? (int)this.controller.Document.Length : 0;
            }
        }

        public string GetString(long i_startIndex, long i_endIndex)
        {
            var length = i_endIndex - i_startIndex;
            return controller.Document.ToString(i_startIndex + this.startImeDocumentIndex, length);
        }

#if METRO || WPF
        public bool ScrollToCompstionUpdated(TextStoreBase textStore,EditView view,int start, int end)
        {
            if (textStore.IsLocked() == false)
                return false;
            using (Unlocker locker = textStore.LockDocument(false))
            {
                foreach (TextDisplayAttribute attr in textStore.EnumAttributes(start, end))
                {
                    if (attr.attribute.bAttr == TF_DA_ATTR_INFO.TF_ATTR_TARGET_CONVERTED)
                    {
                        if (view.AdjustSrc(attr.startIndex + this.startImeDocumentIndex))
                        {
                            return true;
                        }
                    }
                }
            }
            return false;   
        }
#endif

        public void GetStringExtent(EditView view,long i_startIndex, long i_endIndex,out Point startPos,out Point endPos)
        {
            var document = controller.Document;
            long a_startIndex = i_startIndex + this.startImeDocumentIndex;
            TextPoint startTextPoint = view.LayoutLines.GetTextPointFromIndex(a_startIndex);
            if (i_startIndex == i_endIndex)
            {
                startPos = view.CaretLocation;
                endPos = view.CaretLocation;
            }
            else
            {
                var endIndex = i_endIndex < 0 ? document.Length - 1 : i_endIndex;
                TextPoint endTextPoint;

                startPos = view.GetPostionFromTextPoint(startTextPoint);
                endTextPoint = view.GetLayoutLineFromIndex(endIndex);
                endPos = view.GetPostionFromTextPoint(endTextPoint);
            }

            //アンダーラインを描くことがあるので少しずらす
            var layout = view.LayoutLines.GetLayout(startTextPoint.row);
            double emHeight = view.render != null ? layout.Height : 0;
            endPos.Y += emHeight + 10;
        }

        public void GetSelection(SelectCollection selectons, out TextRange sel)
        {
            if (controller.RectSelection && selectons.Count > 0)
            {
                sel.Index = selectons[0].start - this.startImeDocumentIndex;
                sel.Length = 0;
            }
            else
            {
                sel.Index = controller.SelectionStart - this.startImeDocumentIndex;
                sel.Length = controller.SelectionLength;
            }
        }

        public void SetSelectionIndex(EditView view, long i_startIndex, long i_endIndex)
        {
            long a_startIndex = i_startIndex + this.startImeDocumentIndex;
            long a_endIndex = i_endIndex + this.startImeDocumentIndex;
            if (controller.IsRectInsertMode())
            {
                TextPoint start = view.LayoutLines.GetTextPointFromIndex(a_startIndex);
                TextPoint end = view.LayoutLines.GetTextPointFromIndex(view.Selections.Last().start);
                controller.JumpCaret(i_endIndex);
                controller.Document.Select(start, a_endIndex - a_startIndex, end.row - start.row);
            }
            else if (i_startIndex == i_endIndex)
            {
                controller.JumpCaret(a_startIndex);
            }
            else
            {
                controller.Document.Select(a_startIndex, a_endIndex - a_startIndex);
            }
        }

        public void InsertTextAtSelection(string i_value, long startIndex, long endIndex, bool fromTIP = true)
        {
            controller.DoInputString(i_value, fromTIP);
        }

        public int ConvertToIMEDocument(int index)
        {
            return (int)(index - this.startImeDocumentIndex);
        }

        public long ConvertToDocument(int index)
        {
            return index + this.startImeDocumentIndex;
        }

        public void GetNotifySelectionArea(SelectCollection selections, out int startIndex,out int endIndex)
        {
            if (this.IsAllowFullDocument())
            {
                TextRange currentSelection = new TextRange();
                this.GetSelection(selections, out currentSelection);

                startIndex = (int)(currentSelection.Index);
                endIndex = (int)(currentSelection.Index + currentSelection.Length);
            }
            else
            {
                startIndex = 0;
                endIndex = 0;
            }
        }

        public void GetNotifyTextChageArea(DocumentUpdateEventArgs e, out int startIndex, out int endIndex, out int newStartIndex, out int newEndIndex) 
        {
            if (IsAllowFullDocument())
            {
                startIndex = (int)e.startIndex;
                endIndex = (int)(e.startIndex + e.removeLength);
                newStartIndex = startIndex;
                newEndIndex = (int)(startIndex + e.insertLength);
            }
            else
            {
                startIndex = 0;
                endIndex = 0;
                newStartIndex = 0;
                newEndIndex = 0;
            }
        }

    }
}
#endif
