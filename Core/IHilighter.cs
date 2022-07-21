/*
 * Copyright (C) 2013 FooProject
 * * This program is free software; you can redistribute it and/or modify it under the terms of the GNU General Public License as published by
 * the Free Software Foundation; either version 3 of the License, or (at your option) any later version.

 * This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of 
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.

You should have received a copy of the GNU General Public License along with this program. If not, see <http://www.gnu.org/licenses/>.
 */
using System;

namespace FooEditEngine
{
    /// <summary>
    /// トークンのタイプを表す
    /// </summary>
    public enum TokenType
    {
        /// <summary>
        /// どのカテゴリーにも属さないトークンを表す
        /// </summary>
        None = 0,
        /// <summary>
        /// キーワード1として表示するトークンを表す
        /// </summary>
        Keyword1,
        /// <summary>
        /// キーワード2として表示するトークンを表す
        /// </summary>
        Keyword2,
        /// <summary>
        /// コメントとして表示するトークンを表す
        /// </summary>
        Comment,
        /// <summary>
        /// 文字リテラルとして表示するトークンを表す
        /// </summary>
        Literal,
        /// <summary>
        /// コントロールとして表示するトークンを表す
        /// </summary>
        Control,
    }

    /// <summary>
    /// イベントデータを表す
    /// </summary>
    public class TokenSpilitEventArgs
    {
        /// <summary>
        /// 単語長
        /// </summary>
        public int length;
        /// <summary>
        /// トークンのタイプ
        /// </summary>
        public TokenType type;
        /// <summary>
        /// トークンの切り出しをやめるなら真をセットし、そうでないなら偽をセットする（規定値は偽）
        /// </summary>
        public bool breaked;
        /// <summary>
        /// トークンがあるインデックス
        /// </summary>
        public int index;

        /// <summary>
        /// コンストラクター
        /// </summary>
        public TokenSpilitEventArgs()
        {
        }

        /// <summary>
        /// コンストラクター
        /// </summary>
        /// <param name="index">開始インデックス</param>
        /// <param name="length">長さ</param>
        /// <param name="type">トークンタイプ</param>
        public TokenSpilitEventArgs(int index,int length, TokenType type)
        {
            this.length = length;
            this.type = type;
            this.index = index;
            this.breaked = false;
        }
    }

    /// <summary>
    /// トークンが切り出された時に呼ばれるイベント
    /// </summary>
    /// <param name="state">イベントデータ</param>
    /// <returns></returns>
    public delegate void TokenSpilitHandeler(TokenSpilitEventArgs state);

    /// <summary>
    /// シンタックスハイライトを行うためのインターフェイス
    /// </summary>
    public interface IHilighter
    {
        /// <summary>
        /// 初期状態に戻す
        /// </summary>
        void Reset();

        /// <summary>
        /// ハイライト処理を実行します
        /// </summary>
        /// <param name="text">対象となる文字列</param>
        /// <param name="length">文字列の長さ</param>
        /// <param name="action">トークンが切り出されたときに呼び出す関数</param>
        /// <returns>エンクロージャーレベル。開始エンクロージャーだけを検出した場合は1以上の値を返し、
        /// 終了エンクロージャーだけを検出した場合を-1以下の値を返すようにします。
        /// 何も検出しなかった場合、開始エンクロージャーと終了エンクロージャーが対になっている場合、
        /// エンクロージャー内で開始エンクロージャーを検出した場合は0を返します
        /// なお、開始エンクロージャーがすでに検出されている状態で検出したことを返した場合、その結果は無視されます
        /// </returns>
        /// <example>
        /// int DoHilight(string text,int length, TokenSpilitHandeler action)
        /// {
        ///     if(length > 3 &amp;&amp; text == "foo")
        ///         action(new TokenSpilitEventArgs(0,3,TokenType.Keyword1);
        ///     return 0;
        /// }
        /// </example>
        int DoHilight(string text,int length, TokenSpilitHandeler action);
    }
}
