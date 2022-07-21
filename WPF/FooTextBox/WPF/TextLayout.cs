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
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.TextFormatting;

namespace FooEditEngine.WPF
{
    sealed class TextLayout : ITextLayout
    {
        List<TextDecorationInfo> SquilleLines = new List<TextDecorationInfo>();
        List<TextLine> lines = new List<TextLine>();
        FontRendering fontRender;
        CustomTextSource textSource;
        double _width;
        public TextLayout(string s, FontFamily font, double fontSize,Brush fore,double width,TextAlignment align = TextAlignment.Left,TextWrapping wrap = TextWrapping.Wrap)
        {
            this.fontRender = new FontRendering(font, fontSize, fore, align);
            this.textSource = new CustomTextSource();
            this.textSource.Text = s;
            this.textSource.FontRendering = fontRender;
            this.textSource.EffectCollection = new TextEffectCollection();
            this.textSource.DecorationCollection = new List<TextDecorationInfo>();
            this.TextWarpping = wrap;
            this._width = width;
            this.Disposed = false;
        }

        public FlowDirection FlowDirection
        {
            get;
            set;
        }

        public bool Disposed
        {
            get;
            private set;
        }

        public bool Invaild
        {
            get
            {
                return false;
            }
        }

        public TextWrapping TextWarpping
        {
            get;
            set;
        }

        public List<TextLine> Lines
        {
            get
            {
                if (this.lines.Count == 0)
                    this.CreateLine();
                return this.lines;
            }
        }

        public void SetTextEffect(TextEffect effect)
        {
            this.textSource.EffectCollection.Add(effect);
        }

        public void SetTextDecoration(int start, int count, TextDecorationCollection collection)
        {
            this.textSource.DecorationCollection.Add(new TextDecorationInfo(start, count, collection));
        }

        public void SetSquilleLine(int start, int count, TextDecorationCollection collection)
        {
            this.SquilleLines.Add(new TextDecorationInfo(start,count,collection));
        }

        public void Draw(DrawingContext dc,double x,double y)
        {
            if(this.lines.Count == 0)
                this.CreateLine();

            foreach (TextDecorationInfo info in this.SquilleLines)
            {
                SquilleLineMarker marker = new WPFSquilleLineMarker(dc,info.DecorationCollection[0].Pen);
                foreach (TextBounds bound in this.GetTextBounds(info.Start, info.Count))
                {
                    Rect rect = bound.Rectangle;
                    marker.Draw(x + rect.Left, y + rect.Top + rect.Height, rect.Width, rect.Height);
                }
            }

            double posY = y;
            foreach(var line in this.lines)
            {
                line.Draw(dc, new System.Windows.Point(x, posY), InvertAxes.None);
                posY += line.Height;
            }
        }

        public void Dispose()
        {
            foreach (TextLine line in this.lines)
                line.Dispose();
            this.lines.Clear();
            this.Disposed = true;
        }

        void CreateLine()
        {
            TextFormatter formatter = TextFormatter.Create();

            if (textSource.Text.Length == 0)
            {
                TextLine myTextLine = formatter.FormatLine(
                    textSource,
                    0,
                    this._width,
                    new GenericTextParagraphProperties(fontRender, this.FlowDirection,this.TextWarpping),
                    null);
                lines.Add(myTextLine);
            }
            else
            {
                int textStorePosition = 0;
                this.lines.Clear();
                while (textStorePosition < textSource.Text.Length)
                {
                    TextLine myTextLine = formatter.FormatLine(
                        textSource,
                        textStorePosition,
                        this._width,
                        new GenericTextParagraphProperties(fontRender, this.FlowDirection,this.TextWarpping),
                        null);
                    lines.Add(myTextLine);
                    textStorePosition += myTextLine.Length;
                }
            }
        }

        double _actualWidth = 0;
        public double Width
        {
            get
            {
                if (_actualWidth != 0)
                    return _actualWidth;
                foreach (var line in this.Lines)
                {
                    if(line.Width > _actualWidth)
                        _actualWidth = line.Width;
                }
                return _actualWidth;
            }
        }

        double _height = 0;
        public double Height
        {
            get {
                if (_height != 0)
                    return _height;
                foreach (var line in this.Lines)
                    _height += line.Height;
                return _height;
            }
        }

        public int GetIndexFromColPostion(double x)
        {
            CharacterHit hit = this.Lines[0].GetCharacterHitFromDistance(x);
            return hit.FirstCharacterIndex;
        }

        public double GetWidthFromIndex(int index)
        {
            double width = this.Lines[0].GetDistanceFromCharacterHit(new CharacterHit(index, 1));
            return width;
        }

        public double GetColPostionFromIndex(int index)
        {
            return this.Lines[0].GetDistanceFromCharacterHit(new CharacterHit(index, 0));
        }

        public int GetIndexFromPostion(double x, double y)
        {
            return 0;
        }

        public Point GetPostionFromIndex(int index)
        {
            throw new NotImplementedException();
        }

        public int AlignIndexToNearestCluster(int index, AlignDirection flow)
        {
            CharacterHit hit = this.Lines[0].GetNextCaretCharacterHit(new CharacterHit(index, 0));
            return flow == AlignDirection.Back ? hit.FirstCharacterIndex : hit.FirstCharacterIndex + hit.TrailingLength;
        }

        public IList<TextBounds> GetTextBounds(int start, int length)
        {
            return this.Lines[0].GetTextBounds(start, length);
        }
    }

    class WPFSquilleLineMarker : SquilleLineMarker
    {
        DrawingContext Context;
        Pen Pen;
        public WPFSquilleLineMarker(DrawingContext context, Pen pen)
        {
            this.Context = context;
            this.Pen = pen;
        }

        public override void DrawLine(double x, double y, double tox, double toy)
        {
            this.Context.DrawLine(this.Pen, new System.Windows.Point(x, y), new System.Windows.Point(tox, toy));
        }
    }
}
