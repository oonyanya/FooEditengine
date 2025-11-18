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
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.TextFormatting;

namespace FooEditEngine.WPF
{
    sealed class WPFRender : IPrintableTextRender, IDisposable
    {
        FontFamily _Font;
        double _FontSize, _LineHeight;
        DrawingContext Context;
        int _TabWidthChar;
        double TabWidth;
        bool _RightToLeft;
        System.Windows.Media.Color ForegroundColor, BackgroundColor, HilightColor, Keyword1Color, Keyword2Color, LiteralColor, UrlColor, ControlCharColor, CommentColor, InsertCaretColor, OverwriteCaretColor, LineMarkerColor;
        BrushStockes Brushes = new BrushStockes();
        PenStockes Pens = new PenStockes();
        VisualHost host;
        DrawingVisual visual;

        public WPFRender(FontFamily font, double fontSize)
        {
            this.ChangedRenderResource += (s, e) => { };
            this.ChangedRightToLeft += (s, e) => { };
            this.FontFamily = font;
            this.FontSize = fontSize;
        }

        public WPFRender(FooTextBox textbox, double width, double height,FrameworkElement image)
        {
            this.ChangedRenderResource += (s, e) => { };
            this.ChangedRightToLeft += (s, e) => { };
            
            this.FontFamily = textbox.FontFamily;
            this.FontSize = textbox.FontSize;
            this.ForegroundColor = textbox.Foreground;
            this.BackgroundColor = textbox.Background;
            this.ControlCharColor = textbox.ControlChar;
            this.HilightColor = textbox.Hilight;
            this.CommentColor = textbox.Comment;
            this.UrlColor = textbox.URL;
            this.Keyword1Color = textbox.Keyword1;
            this.Keyword2Color = textbox.Keyword2;
            this.LiteralColor = textbox.Literal;
            this.InsertCaretColor = textbox.InsertCaret;
            this.OverwriteCaretColor = textbox.OverwriteCaret;
            this.LineMarkerColor = textbox.LineMarker;

            this.host = (VisualHost)image;
        }

        public event EventHandler ChangedRightToLeft;

        public TextAntialiasMode TextAntialiasMode
        {
            get;
            set;
        }

        public bool ShowFullSpace
        {
            get;
            set;
        }

        public bool ShowHalfSpace
        {
            get;
            set;
        }

        public bool ShowTab
        {
            get;
            set;
        }

        public bool RightToLeft
        {
            get { return this._RightToLeft; }
            set
            {
                this._RightToLeft = value;
                this.ChangedRightToLeft(this, null);
            }
        }

        public bool InsertMode
        {
            get;
            set;
        }

        public Rectangle TextArea
        {
            get;
            set;
        }

        public FontFamily FontFamily
        {
            get
            {
                return this._Font;
            }
            set
            {
                this._Font = value;
                this.ChangedRenderResource(this, new ChangedRenderRsourceEventArgs(ResourceType.Font));
            }
        }

        public double FontSize
        {
            get
            {
                return this._FontSize;
            }
            set
            {
                this._FontSize = value;

                TextLayout layout = new TextLayout("0", this.FontFamily, this.FontSize, Brushes[this.Foreground], 0);
                this.LineNemberWidth = layout.Lines[0].Width * EditView.LineNumberLength;
                this._LineHeight = layout.Lines[0].Height;
                this.emSize = new Size(layout.Lines[0].Width, layout.Lines[0].Height);
                layout.Dispose();
                
                this.ChangedRenderResource(this, new ChangedRenderRsourceEventArgs(ResourceType.Font));
            }
        }

        public double LineNemberWidth
        {
            get;
            private set;
        }

        public double FoldingWidth
        {
            get
            {
                return 0;
            }
        }

        public int TabWidthChar
        {
            get
            {
                return this._TabWidthChar;
            }
            set
            {
                TextLayout layout = new TextLayout("a", this.FontFamily, this.FontSize, this.Brushes[this.ForegroundColor], 0);
                double width = layout.Lines[0].WidthIncludingTrailingWhitespace;
                layout.Dispose();
                this.TabWidth = width * value;
                this._TabWidthChar = value;
            }
        }

        public Size emSize
        {
            get;
            private set;
        }

        public System.Windows.Media.Color Foreground
        {
            get
            {
                return this.ForegroundColor;
            }
            set
            {
                this.ForegroundColor = value;
                this.ChangedRenderResource(this, new ChangedRenderRsourceEventArgs(ResourceType.Brush));
            }
        }

        public System.Windows.Media.Color Background
        {
            get
            {
                return this.BackgroundColor;
            }
            set
            {
                this.BackgroundColor = value;
                this.ChangedRenderResource(this, new ChangedRenderRsourceEventArgs(ResourceType.Brush));
            }
        }

        public System.Windows.Media.Color InsertCaret
        {
            get
            {
                return this.InsertCaretColor;
            }
            set
            {
                this.InsertCaretColor = value;
            }
        }

        public System.Windows.Media.Color OverwriteCaret
        {
            get
            {
                return this.OverwriteCaretColor;
            }
            set
            {
                this.OverwriteCaretColor = value;
            }
        }

        public System.Windows.Media.Color LineMarker
        {
            get
            {
                return this.LineMarkerColor;
            }
            set
            {
                this.LineMarkerColor = value;
            }
        }

        public System.Windows.Media.Color ControlChar
        {
            get
            {
                return this.ControlCharColor;
            }
            set
            {
                this.ControlCharColor = value;
                this.ChangedRenderResource(this, new ChangedRenderRsourceEventArgs(ResourceType.Brush));
            }
        }

        public System.Windows.Media.Color Url
        {
            get
            {
                return this.UrlColor;
            }
            set
            {
                this.UrlColor = value;
                this.ChangedRenderResource(this, new ChangedRenderRsourceEventArgs(ResourceType.Brush));
            }
        }

        public System.Windows.Media.Color Hilight
        {
            get
            {
                return this.HilightColor;
            }
            set
            {
                this.HilightColor = value;
                this.ChangedRenderResource(this, new ChangedRenderRsourceEventArgs(ResourceType.Brush));
            }
        }

        public System.Windows.Media.Color Comment
        {
            get
            {
                return this.CommentColor;
            }
            set
            {
                this.CommentColor = value;
                this.ChangedRenderResource(this, new ChangedRenderRsourceEventArgs(ResourceType.Brush));
            }
        }

        public System.Windows.Media.Color Literal
        {
            get
            {
                return this.LiteralColor;
            }
            set
            {
                this.LiteralColor = value;
                this.ChangedRenderResource(this, new ChangedRenderRsourceEventArgs(ResourceType.Brush));
            }
        }

        public System.Windows.Media.Color Keyword1
        {
            get
            {
                return this.Keyword1Color;
            }
            set
            {
                this.Keyword1Color = value;
                this.ChangedRenderResource(this, new ChangedRenderRsourceEventArgs(ResourceType.Brush));
            }
        }

        public System.Windows.Media.Color Keyword2
        {
            get
            {
                return this.Keyword2Color;
            }
            set
            {
                this.Keyword2Color = value;
                this.ChangedRenderResource(this, new ChangedRenderRsourceEventArgs(ResourceType.Brush));
            }
        }

        public float HeaderHeight
        {
            get { return (float)this._LineHeight; }
        }

        public float FooterHeight
        {
            get { return (float)this._LineHeight; }
        }

        public bool Printing
        {
            get;
            set;
        }

        public bool ShowLineBreak
        {
            get;
            set;
        }

        public event ChangedRenderResourceEventHandler ChangedRenderResource;

        public void DrawCachedBitmap(Rectangle rect)
        {
            return;
        }

        public void CacheContent()
        {
            return;
        }

        public bool IsVaildCache()
        {
            return false;
        }

        public void SetDrawingContext(DrawingContext dc)
        {
            this.Context = dc;
        }

        public void BegineDraw()
        {
            this.visual = new DrawingVisual();
            this.Context = this.visual.RenderOpen();
        }

        public void EndDraw()
        {
            this.Context.Close();
            this.host.AddVisual(this.visual);
        }

        public bool Resize(double width, double height)
        {
            return true;
        }

        public void DrawString(string str, double x, double y, StringAlignment align, Size layoutRect, StringColorType colorType = StringColorType.Forground)
        {
            TextAlignment TextAlign = TextAlignment.Left;
            switch (align)
            {
                case StringAlignment.Left:
                    TextAlign = TextAlignment.Left;
                    break;
                case StringAlignment.Center:
                    TextAlign = TextAlignment.Center;
                    break;
                case StringAlignment.Right:
                    TextAlign = TextAlignment.Right;
                    break;
            }
            TextLayout layout = new TextLayout(str, this.FontFamily, this.FontSize, this.Brushes[this.ForegroundColor], layoutRect.Width, TextAlign);
            layout.FlowDirection = this.RightToLeft ? FlowDirection.RightToLeft : FlowDirection.LeftToRight;
            layout.Draw(this.Context, x, y);
            layout.Dispose();
        }

        public void FillRectangle(Rectangle rect,FillRectType type)
        {
            if (this.Printing)
                return;
            switch(type)
            {
                case FillRectType.OverwriteCaret:
                    this.Context.DrawRectangle(this.Brushes[this.OverwriteCaret], null, rect);
                    break;
                case FillRectType.InsertCaret:
                    this.Context.DrawRectangle(this.Brushes[this.InsertCaret], null, rect);
                    break;
                case FillRectType.InsertPoint:
                    break;
                case FillRectType.LineMarker:
                    this.Context.DrawRectangle(this.Brushes[this.LineMarker], null, rect);
                    break;
            }
        }

        public void DrawFoldingMark(bool expand, double x, double y)
        {
            string str = expand ? "-" : "+";
            TextLayout layout = new TextLayout(str, this.FontFamily, this.FontSize, this.Brushes[this.ForegroundColor], this.FoldingWidth);
            layout.Draw(this.Context, x, y);
        }

        public void DrawLine(Point from, Point to)
        {
            Brush brush = this.Brushes[this.ForegroundColor];
            Pen pen = this.Pens.Get(brush,HilightType.Sold);
            this.Context.DrawLine(pen, from, to);
        }

        public void FillBackground(Rectangle rect)
        {
            if (this.Printing)
                return;
            this.Context.DrawRectangle(this.Brushes[this.Background], null, rect);
        }

        public void DrawOneLine(Document doc, LineToIndexTable lti, int row, double mainLayoutx, double mainLayouty)
        {
            CombineTextLayout layout = (CombineTextLayout)lti.GetLayout(row);

            if (lti.GetLengthFromLineNumber(row) == 0)
                return;

            layout.Draw(mainLayoutx, mainLayouty, (subLayout, startIndexSublayout, x, y) =>
            {
                var textLayout = (TextLayout)subLayout;
                textLayout.Draw(this.Context, x, y);
            });
        }

        IDisposable layerDisposer;
        public IDisposable BeginClipRect(Rectangle rect)
        {
            this.Context.PushClip(new RectangleGeometry(rect));
            layerDisposer = new DummyDisposer(() => {
                this.Context.Pop();
            });
            return layerDisposer;
        }

        public void EndClipRect()
        {
            layerDisposer.Dispose();
        }

        public ITextLayout CreateLaytout(Document doc, long index, long length, SyntaxInfo[] syntaxCollection, IEnumerable<Marker> MarkerRanges, IEnumerable<Selection> SelectRanges, double WrapWidth)
        {
            if (doc == null)
            {
                return this.CreateLaytout("", syntaxCollection, MarkerRanges, SelectRanges, WrapWidth);
            }
            else
            {
                var str = doc.ToString(index, length);
                return this.CreateLaytout(str, syntaxCollection, MarkerRanges, SelectRanges, WrapWidth);
            }
        }

        public ITextLayout CreateLaytout(string str, SyntaxInfo[] syntaxCollection, IEnumerable<Marker> MarkerRanges, IEnumerable<Selection> SelectRanges, double wrapwidth)
        {
            TextLayout layout;
            if(wrapwidth == LineToIndexTable.NONE_BREAK_LINE)
            {
                layout = new TextLayout(str, this.FontFamily, this.FontSize, Brushes[this.Foreground], this.TextArea.Width);
                layout.TextWarpping = TextWrapping.NoWrap;
            }
            else
            {
                layout = new TextLayout(str, this.FontFamily, this.FontSize, Brushes[this.Foreground], wrapwidth);
                layout.TextWarpping = TextWrapping.Wrap;
            }
            layout.FlowDirection = this.RightToLeft ? FlowDirection.RightToLeft : FlowDirection.LeftToRight;

            if (syntaxCollection != null)
            {
                foreach (SyntaxInfo s in syntaxCollection)
                {
                    if (s.length == 0 || s.start == -1)
                        continue;
                    Brush brush = this.Brushes[this.Foreground];
                    switch (s.type)
                    {
                        case TokenType.Comment:
                            brush = this.Brushes[this.Comment];
                            break;
                        case TokenType.Keyword1:
                            brush = this.Brushes[this.Keyword1];
                            break;
                        case TokenType.Keyword2:
                            brush = this.Brushes[this.Keyword2];
                            break;
                        case TokenType.Literal:
                            brush = this.Brushes[this.Literal];
                            break;
                    }
                    System.Diagnostics.Debug.Assert(s.index < Int32.MaxValue - 1);
                    TextEffect effect = new TextEffect(null, brush, null, (int)s.index, (int)s.length);
                    effect.Freeze();
                    layout.SetTextEffect(effect);
                }
            }

            if (MarkerRanges != null)
            {
                foreach (Marker m in MarkerRanges)
                {
                    if (m.start == -1 || m.length == 0)
                        continue;

                    Brush brush;
                    if (m.hilight == HilightType.Url)
                    {
                        brush = this.Brushes[this.Url];
                        TextEffect effect = new TextEffect(null, brush, null, (int)m.start, (int)m.length);
                        effect.Freeze();
                        layout.SetTextEffect(effect);
                    }
                    else
                    {
                        System.Windows.Media.Color color = new System.Windows.Media.Color();
                        color.A = m.color.A;
                        color.R = m.color.R;
                        color.G = m.color.G;
                        color.B = m.color.B;
                        brush = this.Brushes[color];
                    }

                    Pen pen = this.Pens.Get(brush,m.hilight);

                    TextDecorationCollection collection = new TextDecorationCollection();
                    TextDecoration decoration = new TextDecoration();
                    decoration.Pen = pen;
                    decoration.Location = TextDecorationLocation.Underline;
                    decoration.Freeze();
                    collection.Add(decoration);

                    if (m.hilight == HilightType.Squiggle)
                        layout.SetSquilleLine((int)m.start, (int)m.length, collection);
                    else
                        layout.SetTextDecoration((int)m.start, (int)m.length, collection);
                }
            }

            return layout;
        }

        public void DrawGripper(Point p, double radius)
        {
            //タッチには対応していないので実装する必要はない
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            this.Pens = null;
            this.Brushes = null;
        }
    }

    class BrushStockes
    {
        ResourceManager<System.Windows.Media.Color, Brush> collection = new ResourceManager<System.Windows.Media.Color, Brush>();
        public Brush this[System.Windows.Media.Color c]
        {
            get
            {
                Brush brush;
                if (this.collection.TryGetValue(c, out brush))
                    return brush;
                brush = new SolidColorBrush(c);
                brush.Freeze();
                this.collection.Add(c, brush);
                return brush;
            }
        }
    }

    class PenStockes
    {
        ResourceManager<Brush, ResourceManager<HilightType, Pen>> cache = new ResourceManager<Brush, ResourceManager<HilightType, Pen>>();
        public Pen Get(Brush brush, HilightType type)
        {
            ResourceManager<HilightType, Pen> hilights;
            Pen effect;
            if (this.cache.TryGetValue(brush, out hilights))
            {
                if (hilights.TryGetValue(type, out effect))
                    return effect;
                effect = CreatePen(brush,type);
                hilights.Add(type, effect);
                return effect;
            }
            effect = CreatePen(brush, type);
            hilights = new ResourceManager<HilightType, Pen>();
            hilights.Add(type, effect);
            this.cache.Add(brush, hilights);
            return effect;
        }
        Pen CreatePen(Brush brush, HilightType type)
        {
            Pen pen = new Pen(brush, 1.0f);
            switch (type)
            {
                case HilightType.Sold:
                case HilightType.Url:
                case HilightType.Squiggle:
                    pen.DashStyle = DashStyles.Solid;
                    break;
                case HilightType.Dash:
                    pen.DashStyle = DashStyles.Dash;
                    break;
                case HilightType.DashDot:
                    pen.DashStyle = DashStyles.DashDot;
                    break;
                case HilightType.DashDotDot:
                    pen.DashStyle = DashStyles.DashDotDot;
                    break;
                case HilightType.Dot:
                    pen.DashStyle = DashStyles.Dot;
                    break;
            }
            pen.Freeze();
            return pen;
        }
    }
}
