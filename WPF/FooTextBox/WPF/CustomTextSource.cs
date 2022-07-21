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
using System.Text;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.TextFormatting;

namespace FooEditEngine.WPF
{
    // CustomTextSource is our implementation of TextSource.  This is required to use the WPF
    // text engine. This implementation is very simplistic as is DOES NOT monitor spans of text
    // for different properties. The entire text content is considered a single span and all 
    // changes to the size, alignment, font, etc. are applied across the entire text.
    sealed class CustomTextSource : TextSource
    {
        // Used by the TextFormatter object to retrieve a run of text from the text source.
        public override TextRun GetTextRun(int textSourceCharacterIndex)
        {
            // Make sure text source index is in bounds.
            if (textSourceCharacterIndex < 0)
                throw new ArgumentOutOfRangeException("textSourceCharacterIndex", "Value must be greater than 0.");
            if (textSourceCharacterIndex >= _text.Length)
            {
                return new TextEndOfParagraph(1);
            }

            if (this.DecorationCollection != null)
            {
                foreach (TextDecorationInfo info in this.DecorationCollection)
                {
                    if(textSourceCharacterIndex < info.Start)
                        return new TextCharacters(
                            _text,
                            textSourceCharacterIndex,
                            info.Start - textSourceCharacterIndex,
                            new GenericTextRunProperties(_currentRendering, this.EffectCollection));
                    else if (textSourceCharacterIndex >= info.Start && textSourceCharacterIndex < info.Start + info.Count)
                        return new TextCharacters(
                            _text,
                            textSourceCharacterIndex,
                            info.Start + info.Count - textSourceCharacterIndex,
                            new GenericTextRunProperties(_currentRendering, this.EffectCollection, info.DecorationCollection));
                }
            }
            return new TextCharacters(
               _text,
               textSourceCharacterIndex,
               _text.Length - textSourceCharacterIndex,
               new GenericTextRunProperties(_currentRendering, this._effectCollection));
        }

        public override TextSpan<CultureSpecificCharacterBufferRange> GetPrecedingText(int textSourceCharacterIndexLimit)
        {
            CharacterBufferRange cbr = new CharacterBufferRange(_text, 0, textSourceCharacterIndexLimit);
            return new TextSpan<CultureSpecificCharacterBufferRange>(
             textSourceCharacterIndexLimit,
             new CultureSpecificCharacterBufferRange(System.Globalization.CultureInfo.CurrentUICulture, cbr)
             );
        }

        public override int GetTextEffectCharacterIndexFromTextSourceCharacterIndex(int textSourceCharacterIndex)
        {
            return textSourceCharacterIndex;
        }

        #region Properties
        public string Text
        {
            get { return _text; }
            set { _text = value; }
        }

        public FontRendering FontRendering
        {
            get { return _currentRendering; }
            set { _currentRendering = value; }
        }

        public TextEffectCollection EffectCollection
        {
            get { return _effectCollection; }
            set { _effectCollection = value; }
        }

        public List<TextDecorationInfo> DecorationCollection
        {
            get;
            set;
        }
        #endregion

        #region Private Fields

        private string _text;      //text store
        private FontRendering _currentRendering;
        private TextEffectCollection _effectCollection;

        #endregion
    }

    struct TextDecorationInfo
    {
        public int Start;
        public int Count;
        public TextDecorationCollection DecorationCollection;
        public TextDecorationInfo(int start,int count,TextDecorationCollection collection)
        {
            this.Start = start;
            this.Count = count;
            this.DecorationCollection = collection;
        }
    }
}
