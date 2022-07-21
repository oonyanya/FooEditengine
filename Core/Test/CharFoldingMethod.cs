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
using FooEditEngine;

namespace FooEditEngine.Test
{
    /// <summary>
    /// テスト用折り畳みメソッドの実装
    /// </summary>
    public class CharFoldingMethod : IFoldingStrategy
    {
        /// <summary>
        /// コンストラクター
        /// </summary>
        /// <param name="begin"></param>
        /// <param name="end"></param>
        public CharFoldingMethod(char begin, char end)
        {
            this.BeginChar = begin;
            this.EndChar = end;
        }

        /// <summary>
        /// 折り畳みの開始文字を表す
        /// </summary>
        public char BeginChar
        {
            get;
            set;
        }

        /// <summary>
        /// 折り畳みの終了文字を表す
        /// </summary>
        public char EndChar
        {
            get;
            set;
        }

        /// <summary>
        /// ドキュメントを解析する
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <returns>折り畳みリストのイテレーター</returns>
        public IEnumerable<FoldingItem> AnalyzeDocument(Document doc, int start, int end)
        {
            Stack<int> BeginIndexColletion = new Stack<int>();
            for (int i = start; i <= end; i++)
            {
                if (doc[i] == this.BeginChar)
                    BeginIndexColletion.Push(i);
                if (doc[i] == this.EndChar)
                {
                    if (BeginIndexColletion.Count == 0)
                        continue;
                    int beginIndex = BeginIndexColletion.Pop();
                    if (beginIndex < i)
                    {
                        yield return new FoldingItem(beginIndex, i);
                    }
                }
            }
        }
    }
}
