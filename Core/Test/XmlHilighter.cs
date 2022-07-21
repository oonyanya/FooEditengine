/*
 * Copyright (C) 2013 FooProject
 * * This program is free software; you can redistribute it and/or modify it under the terms of the GNU General Public License as published by
 * the Free Software Foundation; either version 3 of the License, or (at your option) any later version.

 * This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of 
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.

You should have received a copy of the GNU General Public License along with this program. If not, see <http://www.gnu.org/licenses/>.
 */
using System;
using System.Text;
using FooEditEngine;

namespace FooEditEngine.Test
{
    enum TextParserMode
    {
        Literal,
        ScriptPart,
        MultiLineComment,
        TextPart,
    }
    /// <summary>
    /// XMLドキュメント用ハイライター
    /// </summary>
    public class XmlHilighter : IHilighter
    {
        private TextParserMode mode;
        private StringBuilder word;
        private TokenType KeyWordType;

        /// <summary>
        /// コンストラクター
        /// </summary>
        public XmlHilighter()
        {
            this.word = new StringBuilder();
            this.Reset();
        }

        #region IHilighter メンバー

        /// <summary>
        /// 状態をリセットする
        /// </summary>
        public void Reset()
        {
            this.mode = TextParserMode.TextPart;
            this.KeyWordType = TokenType.None;
            this.word.Clear();
        }

        /// <summary>
        /// シンタックスハイライトを行う
        /// </summary>
        /// <param name="text"></param>
        /// <param name="length"></param>
        /// <param name="action"></param>
        /// <returns></returns>
        public int DoHilight(string text, int length, TokenSpilitHandeler action)
        {
            int encloserLevel = 0;
            int i,wordPos = 0;
            for (i = 0; i < length;)
            {
                if (IsMatch(text,i,"<!--"))
                {
                    encloserLevel++;
                    if (TransModeAndAction(TextParserMode.MultiLineComment, action, word, 4, false,ref i, wordPos))
                        break;
                }
                else if (IsMatch(text, i, "-->"))
                {
                    encloserLevel--;
                    if (TransModeAndAction(TextParserMode.TextPart, action, word, 3, true, ref i, wordPos))
                        break;
                }
                else if (text[i] == '<' && this.mode != TextParserMode.MultiLineComment)
                {
                    encloserLevel++;
                    if (TransModeAndAction(TextParserMode.ScriptPart, action, word, 1, true, ref i, wordPos))
                        break;
                    this.KeyWordType = TokenType.Keyword1;
                }
                else if (text[i] == '>' && this.mode == TextParserMode.ScriptPart)
                {
                    encloserLevel--;
                    if (TransModeAndAction(TextParserMode.TextPart, action, word, 1, false, ref i, wordPos))
                        break;
                }
                else if (IsMatch(text, i, "![CDATA[") && this.mode == TextParserMode.ScriptPart)
                {
                    encloserLevel++;
                    if (TransModeAndAction(TextParserMode.Literal, action, word, 8, false, ref i, wordPos))
                        break;
                }
                else if ((text[i] == '\"' || text[i] == '\'') && this.mode == TextParserMode.ScriptPart)
                {
                    encloserLevel++;
                    if (TransModeAndAction(TextParserMode.Literal, action, word, 1, false, ref i, wordPos))
                        break;
                }
                else if ((text[i] == '\"' || text[i] == '\'' ) && this.mode == TextParserMode.Literal)
                {
                    encloserLevel--;
                    if (TransModeAndAction(TextParserMode.ScriptPart, action, word, 1, true, ref i, wordPos))
                        break;
                }
                else if (IsMatch(text, i, "]]") && this.mode == TextParserMode.Literal)
                {
                    encloserLevel--;
                    if (TransModeAndAction(TextParserMode.ScriptPart, action, word, 2, true, ref i, wordPos))
                        break;
                }
                else if (text[i] == ' ')
                {
                    if (TransModeAndAction(this.mode, action, word, 1, false, ref i, wordPos))
                        break;
                    this.KeyWordType = TokenType.Keyword2;
                }
                else if (text[i] == '=')
                {
                    if (TransModeAndAction(this.mode, action, word, 1, false, ref i, wordPos))
                        break;
                    this.KeyWordType = TokenType.None;
                }
                else
                {
                    if (word.Length == 0)
                        wordPos = i;
                    word.Append(text[i]);
                    i++;
                    continue;
                }
            }

            if (word.Length > 0)
            {
                action(new TokenSpilitEventArgs(wordPos,word.Length, GetMode(this.mode,KeyWordType)));
                word.Clear();
            }

            return encloserLevel;
        }

        #endregion

        /// <summary>
        /// 文字列が一致するかどうか確かめる
        /// </summary>
        /// <param name="s">検査される文字列</param>
        /// <param name="index">検査を開始するインデックス</param>
        /// <param name="pattern">検査対象の文字列</param>
        /// <returns></returns>
        private bool IsMatch(string s, int index, string pattern)
        {
            if (index  + pattern.Length >= s.Length)
                return false;
            bool result = false;
            for (int i = index, j = 0; i < index + pattern.Length; i++, j++)
            {
                if ((j == 0 || j > 0 && result) && s[i] == pattern[j])
                    result = true;
                else
                    result = false;
            }

            return result;
        }

        private bool TransModeAndAction(TextParserMode toMode, TokenSpilitHandeler action, StringBuilder word, int tokenLength, bool TranAfterAction, ref int index, int wordPos)
        {
            TokenSpilitEventArgs e = new TokenSpilitEventArgs();

            if (word.Length > 0)
            {
                e.index = wordPos;
                e.length = word.Length;
                e.type = GetMode(this.mode, this.KeyWordType);
                action(e);
                word.Clear();
                if (e.breaked)
                    return true;
            }

            if (TranAfterAction == false)
                this.mode = toMode;

            e.index = index;
            e.length = tokenLength;
            e.type = GetMode(this.mode, TokenType.None);
            action(e);
            if (e.breaked)
                return true;

            if (TranAfterAction)
                this.mode = toMode;

            index += tokenLength;

            return false;
        }
        
        private TokenType GetMode(TextParserMode mode,TokenType isKeyword)
        {
            switch (mode)
            {
                case TextParserMode.Literal:
                    return TokenType.Literal;
                case TextParserMode.ScriptPart:
                    return isKeyword;
                case TextParserMode.MultiLineComment:
                    return TokenType.Comment;
                default:
                    return TokenType.None;
            }
        }
    }
}
