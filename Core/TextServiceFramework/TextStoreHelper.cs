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
#if METRO || WPF
using DotNetTextStore.UnmanagedAPI.WinDef;
using DotNetTextStore.UnmanagedAPI.TSF;
using DotNetTextStore;
#endif

namespace FooEditEngine
{
    static class TextStoreHelper
    {
        public static bool StartCompstion(Document document)
        {
            document.UndoManager.BeginUndoGroup();
            return true;
        }

        public static void EndCompostion(Document document)
        {
            document.UndoManager.EndUndoGroup();
        }

#if METRO || WPF
        public static bool ScrollToCompstionUpdated(TextStoreBase textStore,EditView view,int start, int end)
        {
            if (textStore.IsLocked() == false)
                return false;
            using (Unlocker locker = textStore.LockDocument(false))
            {
                foreach (TextDisplayAttribute attr in textStore.EnumAttributes(start, end))
                {
                    if (attr.attribute.bAttr == TF_DA_ATTR_INFO.TF_ATTR_TARGET_CONVERTED)
                    {
                        if (view.AdjustSrc(attr.startIndex))
                        {
                            return true;
                        }
                    }
                }
            }
            return false;   
        }
#endif

        public static void GetStringExtent(Document document,EditView view,int i_startIndex,int i_endIndex,out Point startPos,out Point endPos)
        {
            TextPoint startTextPoint = view.LayoutLines.GetTextPointFromIndex(i_startIndex);
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

        public static void GetSelection(Controller controller, SelectCollection selectons, out TextRange sel)
        {
            if (controller.RectSelection && selectons.Count > 0)
            {
                sel.Index = selectons[0].start;
                sel.Length = 0;
            }
            else
            {
                sel.Index = controller.SelectionStart;
                sel.Length = controller.SelectionLength;
            }
        }

        public static void SetSelectionIndex(Controller controller,EditView view,int i_startIndex,int i_endIndex)
        {
            if (controller.IsRectInsertMode())
            {
                TextPoint start = view.LayoutLines.GetTextPointFromIndex(i_startIndex);
                TextPoint end = view.LayoutLines.GetTextPointFromIndex(view.Selections.Last().start);
                controller.JumpCaret(i_endIndex);
                controller.Document.Select(start, i_endIndex - i_startIndex, end.row - start.row);
            }
            else if (i_startIndex == i_endIndex)
            {
                controller.JumpCaret(i_startIndex);
            }
            else
            {
                controller.Document.Select(i_startIndex, i_endIndex - i_startIndex);
            }
        }

        public static void InsertTextAtSelection(Controller controller,string i_value, bool fromTIP = true)
        {
            controller.DoInputString(i_value, fromTIP);
        }

#if METRO || WPF
        public static void NotifyTextChanged(TextStoreBase textStore, int startIndex,int removeLength,int insertLength)
        {
#if METRO
            //Windows8.1では同じ値にしないと日本語入力ができなくなってしまう
            int oldend = startIndex,
            newend = startIndex;
#else
            //TS_TEXTCHANGE structure's remark
            //１文字削除した場合はoldendに削除前の位置を設定し、newendとstartIndexに現在位置を設定し、
            //１文字追加した場合はoldendに追加前の位置を設定し、newendとstartIndexに現在位置を設定する
            int oldend = startIndex + removeLength,
                newend = startIndex + insertLength;
#endif
            textStore.NotifyTextChanged(startIndex, oldend, newend);
        }
#endif
    }
}
#endif
