/*
 * Copyright (C) 2013 FooProject
 * * This program is free software; you can redistribute it and/or modify it under the terms of the GNU General Public License as published by
 * the Free Software Foundation; either version 3 of the License, or (at your option) any later version.

 * This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of 
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.

You should have received a copy of the GNU General Public License along with this program. If not, see <http://www.gnu.org/licenses/>.
 */
using System;
using System.Windows.Input;

namespace FooEditEngine.WPF
{
    /// <summary>
    /// FooTextBox固有のコマンド
    /// </summary>
    public static class FooTextBoxCommands
    {
        /// <summary>
        /// 選択モードの切り替えを行います
        /// </summary>
        public static RoutedUICommand ToggleRectSelectMode = new RoutedUICommand("Toggle Rect Selection",
            "ToggleRectSelect", 
            typeof(FooTextBox),
            new InputGestureCollection(){new KeyGesture(Key.B,ModifierKeys.Control)});
        /// <summary>
        /// 表示方向の切り替えを行います
        /// </summary>
        public static RoutedUICommand ToggleFlowDirection = new RoutedUICommand("Toggle Flow Direction",
            "ToggleFlowDirection",
            typeof(FooTextBox),
            null);
        /// <summary>
        /// コードポイントと文字を相互変換します
        /// </summary>
        public static RoutedUICommand ToggleCodePoint = new RoutedUICommand("Toggle ToggleCodePoint",
            "ToggleCodePoint",
            typeof(FooTextBox),
            null);
    }
}
