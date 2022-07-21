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

namespace FooEditEngine
{
    /// <summary>
    /// フォールティング作成の方法を表す
    /// </summary>
    public interface IFoldingStrategy
    {
        /// <summary>
        /// ドキュメントを解析する
        /// </summary>
        /// <param name="doc">ドキュメント</param>
        /// <param name="start">開始インデックス</param>
        /// <param name="end">終了インデックス</param>
        /// <returns>作成したフォールディングを表すイテレーター</returns>
        IEnumerable<FoldingItem> AnalyzeDocument(Document doc,int start,int end);
    }
}
