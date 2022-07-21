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
using System.Text.RegularExpressions;
using Slusser.Collections.Generic;

namespace FooEditEngine
{
    sealed class ReplaceCommand : ICommand
    {
        StringBuffer Buffer;
        TextRange ReplacementRange, ReplacedRange;
        GapBuffer<char> replacement, replaced;   //置き換え後の文字列、置き換え前の文字列

        public ReplaceCommand(StringBuffer buf, int start, int length, string str)
        {
            this.Buffer = buf;
            this.ReplacementRange = new TextRange(start,str.Length);
            this.replacement = new GapBuffer<char>();
            this.replacement.AddRange(str);
            this.ReplacedRange = new TextRange(start,length);
            this.replaced = new GapBuffer<char>();
            this.replaced.AddRange(this.Buffer.GetEnumerator(start, length));
        }

        #region ICommand メンバー

        public void undo()
        {
            this.Buffer.Replace(this.ReplacementRange.Index, this.replacement.Count,this.replaced,this.replaced.Count);
        }

        public void redo()
        {
            this.Buffer.Replace(this.ReplacedRange.Index, this.replaced.Count, this.replacement, this.replacement.Count);
        }

        public bool marge(ICommand a)
        {
            ReplaceCommand cmd = a as ReplaceCommand;
            if (cmd == null)
                return false;
            
            if (this.ReplacedRange.Length == 0 &&
                cmd.ReplacementRange.Index >= this.ReplacementRange.Index &&
                cmd.ReplacementRange.Index + cmd.ReplacementRange.Length <= this.ReplacementRange.Index + this.ReplacementRange.Length)
            {
                int bufferIndex = cmd.ReplacedRange.Index - this.ReplacementRange.Index;
                if(bufferIndex < this.replacement.Count)
                    this.replacement.RemoveRange(bufferIndex, cmd.ReplacedRange.Length);
                this.replacement.InsertRange(bufferIndex, cmd.replacement);
                return true;
            }

            if (this.ReplacedRange.Index + this.ReplacementRange.Length == cmd.ReplacedRange.Index && 
                this.ReplacedRange.Index == this.ReplacementRange.Index)
            {
                this.replaced.AddRange(cmd.replaced);
                this.replacement.AddRange(cmd.replacement);
                this.ReplacedRange.Length += cmd.ReplacedRange.Length;
                this.ReplacementRange.Length += cmd.ReplacementRange.Length;
                return true;
            }
            return false;
        }

        public bool isempty()
        {
            return this.replaced.Count == 0 && this.replacement.Count == 0;
        }
        #endregion

    }

    sealed class ReplaceAllCommand : ICommand
    {
        StringBuffer buffer;
        Regex regex;
        StringBuffer oldBuffer;
        string replacePattern;
        bool groupReplace;
        LineToIndexTable layoutLines;
        public ReplaceAllCommand(StringBuffer buffer, LineToIndexTable layoutlines, Regex regex, string replacePattern,bool groupReplace)
        {
            this.buffer = buffer;
            this.regex = regex;
            this.replacePattern = replacePattern;
            this.groupReplace = groupReplace;
            this.layoutLines = layoutlines;
        }

        public void undo()
        {
            this.buffer.Replace(this.oldBuffer);
        }

        public void redo()
        {
            this.oldBuffer = new StringBuffer(this.buffer);
            this.buffer.ReplaceRegexAll(this.layoutLines, this.regex, this.replacePattern, this.groupReplace);
        }

        public bool marge(ICommand a)
        {
            return false;
        }

        public bool isempty()
        {
            return false;
        }
    }
    sealed class FastReplaceAllCommand : ICommand
    {
        StringBuffer buffer;
        StringBuffer oldBuffer;
        string targetPattern;
        string replacePattern;
        bool caseInsensitve;
        LineToIndexTable layoutLines;
        public FastReplaceAllCommand(StringBuffer buffer,LineToIndexTable layoutlines,string targetPattern, string replacePattern,bool ci)
        {
            this.buffer = buffer;
            this.replacePattern = replacePattern;
            this.targetPattern = targetPattern;
            this.caseInsensitve = ci;
            this.layoutLines = layoutlines;
        }

        public void undo()
        {
            this.buffer.Replace(this.oldBuffer);
        }

        public void redo()
        {
            this.oldBuffer = new StringBuffer(this.buffer);
            this.buffer.ReplaceAll(this.layoutLines, this.targetPattern, this.replacePattern,this.caseInsensitve);
        }

        public bool marge(ICommand a)
        {
            return false;
        }

        public bool isempty()
        {
            return false;
        }
    }
}
