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
   /// <summary>
   /// Class for combining Font and other text related properties. 
   /// (Typeface, Alignment, Decorations, etc)
   /// </summary>
   sealed class FontRendering
   {
      #region Constructors
      public FontRendering(
         double emSize,
         TextAlignment alignment,
         Brush textColor,
         Typeface face)
      {
         _fontSize = emSize;
         _alignment = alignment;
         _textColor = textColor;
         _typeface = face;
      }

      public FontRendering(FontFamily font,double fontSize,Brush fore,TextAlignment align)
      {
         _fontSize = fontSize;
         _alignment = align;
         _textColor = fore;
         _typeface = new Typeface(font,
            FontStyles.Normal, FontWeights.Normal, FontStretches.Normal);
      }
      #endregion

      #region Properties
      public double FontSize
      {
         get { return _fontSize; }
         set
         {
            if (value <= 0)
               throw new ArgumentOutOfRangeException("value", "Parameter Must Be Greater Than Zero.");
            if (double.IsNaN(value))
               throw new ArgumentOutOfRangeException("value", "Parameter Cannot Be NaN.");
            _fontSize = value;
         }
      }

      public TextAlignment TextAlignment
      {
         get { return _alignment; }
         set { _alignment = value; }
      }

      public Brush TextColor
      {
         get { return _textColor; }
         set { _textColor = value; }
      }

      public Typeface Typeface
      {
         get { return _typeface; }
         set { _typeface = value; }
      }
      #endregion

      #region Private Fields
      private double _fontSize;
      private TextAlignment _alignment;
      private Brush _textColor;
      private Typeface _typeface;
      #endregion
   }
}
