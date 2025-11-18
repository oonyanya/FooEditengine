/*
 * Copyright (C) 2013 FooProject
 * * This program is free software; you can redistribute it and/or modify it under the terms of the GNU General Public License as published by
 * the Free Software Foundation; either version 3 of the License, or (at your option) any later version.

 * This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of 
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.

You should have received a copy of the GNU General Public License along with this program. If not, see <http://www.gnu.org/licenses/>.
 */
#if WPF || WINFORM

#define CACHE_COLOR_BURSH

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FooEditEngine;
using SharpDX;
using D2D = SharpDX.Direct2D1;
using DW = SharpDX.DirectWrite;
using DXGI = SharpDX.DXGI;
using System.Runtime.InteropServices;

namespace FooEditEngine
{
    delegate void PreDrawOneLineHandler(MyTextLayout layout,LineToIndexTable lti,int row,int subLayoutStartIndex,double x,double y);

    delegate void GetDpiHandler(out float dpix,out float dpiy);

    /// <summary>
    /// 文字列のアンチエイリアシングモードを指定する
    /// </summary>
    public enum TextAntialiasMode
    {
        /// <summary>
        /// 最適なものが選択されます
        /// </summary>
        Default = D2D.TextAntialiasMode.Default,
        /// <summary>
        /// ClearTypeでアンチエイリアシングを行います
        /// </summary>
        ClearType = D2D.TextAntialiasMode.Cleartype,
        /// <summary>
        /// グレイスケールモードでアンチエイリアシングを行います
        /// </summary>
        GrayScale = D2D.TextAntialiasMode.Grayscale,
        /// <summary>
        /// アンチエイリアシングを行いません
        /// </summary>
        Aliased = D2D.TextAntialiasMode.Aliased,
    }

    public enum ShowSymbol
    {
        HalfSpace,
        FullSpace,
        Tab,
    }

    sealed class D2DResourceFactory
    {
        ResourceManager<Color4, D2D.SolidColorBrush> cache = new ResourceManager<Color4, D2D.SolidColorBrush>();
        D2D.DeviceContext1 _device;
        DW.Factory _DWFactory;
        D2D.Factory2 Factory;

        public D2DResourceFactory()
        {
            this.Factory = new D2D.Factory2(D2D.FactoryType.SingleThreaded);
            this._DWFactory = new DW.Factory(DW.FactoryType.Isolated);
        }

        public D2D.DeviceContext1 Device
        {
            get
            {
                return this._device;
            }
            set
            {
                this._device = value;
                this.Clear();
            }
        }

        public DW.Factory DWFactory
        {
            get
            {
                return this._DWFactory;
            }
        }

        public D2D.Factory2 D2DFactory
        {
            get
            {
                return this.Factory;
            }
        }

        public DW.TextFormat GetTextFormat(string fontName,double fontSize, DW.FontWeight fontWeigth = DW.FontWeight.Normal, DW.FontStyle fontStyle = DW.FontStyle.Normal)
        {
            return new DW.TextFormat(this._DWFactory, fontName, fontWeigth, fontStyle, (float)fontSize);
        }

        public MyTextLayout GetTextLayout(string str, DW.TextFormat format, double width, double height, float dip, bool showLineBreak)
        {
            return new MyTextLayout(this._DWFactory, str, format, width, height, dip, showLineBreak);
        }

        public D2D.SolidColorBrush GetSolidColorBrush(Color4 key)
        {
            D2D.SolidColorBrush brush;

#if CACHE_COLOR_BURSH
            bool result = cache.TryGetValue(key, out brush);
            if (!result)
            {
                brush = new D2D.SolidColorBrush(this._device, key);
                cache.Add(key, brush);
            }
#else
            brush = new D2D.SolidColorBrush(render, key);
#endif
            
            return brush;
        }

        ResourceManager<HilightType, D2D.StrokeStyle> stroke_cache = new ResourceManager<HilightType, D2D.StrokeStyle>();

        public D2D.StrokeStyle GetStroke(HilightType type)
        {
            D2D.StrokeStyle stroke;
            if (this.stroke_cache.TryGetValue(type, out stroke))
                return stroke;

            D2D.StrokeStyleProperties prop = new D2D.StrokeStyleProperties();
            prop.DashCap = D2D.CapStyle.Flat;
            prop.DashOffset = 0;
            prop.DashStyle = D2D.DashStyle.Solid;
            prop.EndCap = D2D.CapStyle.Flat;
            prop.LineJoin = D2D.LineJoin.Miter;
            prop.MiterLimit = 0;
            prop.StartCap = D2D.CapStyle.Flat;
            switch (type)
            {
                case HilightType.Sold:
                case HilightType.Url:
                case HilightType.Squiggle:
                    prop.DashStyle = D2D.DashStyle.Solid;
                    break;
                case HilightType.Dash:
                    prop.DashStyle = D2D.DashStyle.Dash;
                    break;
                case HilightType.DashDot:
                    prop.DashStyle = D2D.DashStyle.DashDot;
                    break;
                case HilightType.DashDotDot:
                    prop.DashStyle = D2D.DashStyle.DashDotDot;
                    break;
                case HilightType.Dot:
                    prop.DashStyle = D2D.DashStyle.Dot;
                    break;
            }
            stroke = new D2D.StrokeStyle(this.Factory, prop);
            this.stroke_cache.Add(type, stroke);
            return stroke;
        }

        ResourceManager<ShowSymbol, D2D.GeometryRealization> symbol_cache = new ResourceManager<ShowSymbol, D2D.GeometryRealization>();

        public D2D.GeometryRealization CreateSymbol(ShowSymbol sym, DW.TextFormat format)
        {

            D2D.GeometryRealization cached_geo = null;
            bool result = symbol_cache.TryGetValue(sym, out cached_geo);

            if (!result)
            {
                const int margin = 2;
                D2D.Geometry geo = null;
                DW.TextLayout layout = null;
                D2D.PathGeometry path = null;
                DW.TextMetrics metrics;
                D2D.StrokeStyle stroke = null;
                switch (sym)
                {
                    case ShowSymbol.FullSpace:
                        layout = new DW.TextLayout(this._DWFactory, "　", format, float.MaxValue, float.MaxValue);
                        metrics = layout.Metrics;
                        Rectangle rect = new Rectangle(margin, margin, Math.Max(1, metrics.WidthIncludingTrailingWhitespace - margin * 2), Math.Max(1, metrics.Height - margin * 2));
                        geo = new D2D.RectangleGeometry(this.Factory, rect);
                        stroke = this.GetStroke(HilightType.Dash);
                        break;
                    case ShowSymbol.HalfSpace:
                        layout = new DW.TextLayout(this._DWFactory, " ", format, float.MaxValue, float.MaxValue);
                        metrics = layout.Metrics;
                        rect = new Rectangle(margin, margin, Math.Max(1, metrics.WidthIncludingTrailingWhitespace - margin * 2), Math.Max(1, metrics.Height - margin * 2));
                        geo = new D2D.RectangleGeometry(this.Factory, rect);
                        stroke = this.GetStroke(HilightType.Sold);
                        break;
                    case ShowSymbol.Tab:
                        layout = new DW.TextLayout(this._DWFactory, "0", format, float.MaxValue, float.MaxValue);
                        metrics = layout.Metrics;
                        path = new D2D.PathGeometry(this.Factory);
                        var sink = path.Open();
                        sink.BeginFigure(new SharpDX.Mathematics.Interop.RawVector2(1,1),D2D.FigureBegin.Filled);   //少し隙間を開けないと描写されない
                        sink.AddLine(new SharpDX.Mathematics.Interop.RawVector2((float)1, (float)metrics.Height));
                        sink.EndFigure(D2D.FigureEnd.Closed);
                        sink.Close();
                        geo = path;
                        stroke = this.GetStroke(HilightType.Sold);
                        break;
                }
                cached_geo= new D2D.GeometryRealization(this.Device, geo, 1.0f, 1.0f, stroke);
                this.symbol_cache.Add(sym, cached_geo);
            }
            return cached_geo;
        }

        public void Clear()
        {
            stroke_cache.Clear();
            symbol_cache.Clear();
            cache.Clear();
        }
    }

    class D2DRenderCommon : IDisposable
    {
        TextAntialiasMode _TextAntialiasMode;
        Color4 _ControlChar,_Forground,_URL,_Hilight;
        DW.TextFormat format;
        protected CustomTextRenderer textRender;
        protected D2D.Bitmap cachedBitMap;
        int tabLength = 8;
        bool _RightToLeft;
        Color4 _Comment, _Literal, _Keyword1, _Keyword2;
        protected bool hasCache = false;
        protected Size renderSize;
        protected D2DResourceFactory _factory;

        D2D.DeviceContext1 _render;

        protected D2D.DeviceContext1 render
        {
            get { return _render; }
            set
            {
                _render = value;
                if (value != null)
                    this.TextAntialiasMode = this._TextAntialiasMode;
                this._factory.Device = render;
            }
        }

        public D2DRenderCommon()
        {
            this._factory = new D2DResourceFactory();
            this.ChangedRenderResource += (s, e) => { };
            this.ChangedRightToLeft += (s, e) => { };
            this.renderSize = new Size();
        }

        public event ChangedRenderResourceEventHandler ChangedRenderResource;

        public event EventHandler ChangedRightToLeft;

        public const int MiniumeWidth = 40;    //これ以上ないと誤操作が起こる

        public void InitTextFormat(string fontName, float fontSize, DW.FontWeight fontWeigth = DW.FontWeight.Normal,DW.FontStyle fontStyle = DW.FontStyle.Normal)
        {
            if(this.format != null)
                this.format.Dispose();

            float dpix, dpiy;
            this.GetDpi(out dpix, out dpiy);

            this.format = this._factory.GetTextFormat(fontName, fontSize * 96.0f / 72.0f, fontWeigth, fontStyle);
            this.format.WordWrapping = DW.WordWrapping.NoWrap;
            this.format.ReadingDirection = GetDWRightDirect(_RightToLeft);

            MyTextLayout layout = this._factory.GetTextLayout("0", this.format, float.MaxValue, float.MaxValue, dpix, false);
            layout.RightToLeft = false;
            this.emSize = new Size(layout.Width, layout.Height);
            layout.Dispose();

            this.TabWidthChar = this.TabWidthChar;

            this.hasCache = false;

            layout = this._factory.GetTextLayout("+", this.format, float.MaxValue, float.MaxValue, dpix, false);
            layout.RightToLeft = false;
#if METRO
            this.FoldingWidth = Math.Max(D2DRenderCommon.MiniumeWidth, layout.Width);
#else
            this.FoldingWidth = layout.Width;
#endif
            layout.Dispose();

            this._factory.Clear();

            this.OnChangedRenderResource(this,new ChangedRenderRsourceEventArgs(ResourceType.Font));
        }

        public void OnChangedRenderResource(object sender, ChangedRenderRsourceEventArgs e)
        {
            if (this.ChangedRenderResource != null)
                this.ChangedRenderResource(sender, e);
        }

        DW.ReadingDirection GetDWRightDirect(bool rtl)
        {
            return rtl ? DW.ReadingDirection.RightToLeft : DW.ReadingDirection.LeftToRight;
        }

        public bool RightToLeft
        {
            get
            {
                return _RightToLeft;
            }
            set
            {
                _RightToLeft = value;
                this.format.ReadingDirection = GetDWRightDirect(value);
                this.ChangedRightToLeft(this, null);                
            }
        }

        public TextAntialiasMode TextAntialiasMode
        {
            get
            {
                return this._TextAntialiasMode;
            }
            set
            {
                if (this.render == null)
                    throw new InvalidOperationException();
                this._TextAntialiasMode = value;
                this.render.TextAntialiasMode = (D2D.TextAntialiasMode)value;
                this.OnChangedRenderResource(this, new ChangedRenderRsourceEventArgs(ResourceType.Antialias));
            }
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

        public bool ShowLineBreak
        {
            get;
            set;
        }

        public Color4 Foreground
        {
            get
            {
                return this._Forground;
            }
            set
            {
                if (this.render == null)
                    return;
                this._Forground = value;
                if (this.textRender != null)
                    this.textRender.DefaultFore = value;
                this.OnChangedRenderResource(this, new ChangedRenderRsourceEventArgs(ResourceType.Brush));
            }
        }

        public Color4 HilightForeground
        {
            get;
            set;
        }

        public Color4 Background
        {
            get;
            set;
        }

        public Color4 InsertCaret
        {
            get;
            set;
        }

        public Color4 OverwriteCaret
        {
            get;
            set;
        }

        public Color4 LineMarker
        {
            get;
            set;
        }

        public Color4 UpdateArea
        {
            get;
            set;
        }

        public Color4 LineNumber
        {
            get;
            set;
        }

        public Color4 ControlChar
        {
            get
            {
                return this._ControlChar;
            }
            set
            {
                this._ControlChar = value;
                this.OnChangedRenderResource(this, new ChangedRenderRsourceEventArgs(ResourceType.Brush));
            }
        }

        public Color4 Url
        {
            get
            {
                return this._URL;
            }
            set
            {
                this._URL = value;
                this.OnChangedRenderResource(this, new ChangedRenderRsourceEventArgs(ResourceType.Brush));
            }
        }

        public Color4 Hilight
        {
            get
            {
                return this._Hilight;
            }
            set
            {
                this._Hilight = value;
            }
        }

        public Color4 Comment
        {
            get
            {
                return this._Comment;
            }
            set
            {
                this._Comment = value;
                this.OnChangedRenderResource(this, new ChangedRenderRsourceEventArgs(ResourceType.Brush));
            }
        }

        public Color4 Literal
        {
            get
            {
                return this._Literal;
            }
            set
            {
                this._Literal = value;
                this.OnChangedRenderResource(this, new ChangedRenderRsourceEventArgs(ResourceType.Brush));
            }
        }

        public Color4 Keyword1
        {
            get
            {
                return this._Keyword1;
            }
            set
            {
                this._Keyword1 = value;
                this.OnChangedRenderResource(this, new ChangedRenderRsourceEventArgs(ResourceType.Brush));
            }
        }

        public Color4 Keyword2
        {
            get
            {
                return this._Keyword2;
            }
            set
            {
                this._Keyword2 = value;
                this.OnChangedRenderResource(this, new ChangedRenderRsourceEventArgs(ResourceType.Brush));
            }
        }

        public Rectangle TextArea
        {
            get;
            set;
        }

        public double LineNemberWidth
        {
            get
            {
                return this.emSize.Width * EditView.LineNumberLength;
            }
        }

        public double FoldingWidth
        {
            get;
            private set;
        }

        public int TabWidthChar
        {
            get { return this.tabLength; }
            set
            {
                if (value == 0)
                    return;
                this.tabLength = value;
                float dpix, dpiy;
                this.GetDpi(out dpix, out dpiy);
                MyTextLayout layout = this._factory.GetTextLayout("0", this.format, float.MaxValue, float.MaxValue,dpiy,false);
                float width = (float)(layout.Width * value);
                this.format.IncrementalTabStop = width;
                layout.Dispose();
            }
        }

        public Size emSize
        {
            get;
            private set;
        }

        public void DrawGripper(Point p, double radius)
        {
            D2D.Ellipse ellipse = new D2D.Ellipse();
            ellipse.Point = p;
            ellipse.RadiusX = (float)radius;
            ellipse.RadiusY = (float)radius;
            this.render.FillEllipse(ellipse, this._factory.GetSolidColorBrush(this.Background));
            this.render.DrawEllipse(ellipse, this._factory.GetSolidColorBrush(this.Foreground));
        }


        public virtual void DrawCachedBitmap(Rectangle dst,Rectangle src)
        {
        }

        public virtual void CacheContent()
        {
        }

        public virtual bool IsVaildCache()
        {
            return this.hasCache;
        }

        protected void BegineDraw()
        {
            if (this.render == null || this.render.IsDisposed)
                return;
            this.render.BeginDraw();
            this.render.AntialiasMode = D2D.AntialiasMode.Aliased;
        }

        protected void EndDraw()
        {
            if (this.render == null || this.render.IsDisposed)
                return;
            this.render.AntialiasMode = D2D.AntialiasMode.PerPrimitive;
            this.render.EndDraw();
        }

        public void DrawString(string str, double x, double y, StringAlignment align, Size layoutRect, StringColorType colorType = StringColorType.Forground)
        {
            if (this.render == null || this.render.IsDisposed)
                return;
            float dpix, dpiy;
            D2D.SolidColorBrush brush;
            switch (colorType)
            {
                case StringColorType.Forground:
                    brush = this._factory.GetSolidColorBrush(this.Foreground);
                    break;
                case StringColorType.LineNumber:
                    brush = this._factory.GetSolidColorBrush(this.LineNumber);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            this.GetDpi(out dpix, out dpiy);
            MyTextLayout layout = this._factory.GetTextLayout(str, this.format, (float)layoutRect.Width, (float)layoutRect.Height,dpix,false);
            layout.StringAlignment = align;
            layout.Draw(this.render, (float)x, (float)y, brush);
            layout.Dispose();
        }

        public void DrawFoldingMark(bool expand, double x, double y)
        {
            string mark = expand ? "-" : "+";
            this.DrawString(mark, x, y,StringAlignment.Left, new Size(this.FoldingWidth, this.emSize.Height));
        }

        public void FillRectangle(Rectangle rect,FillRectType type)
        {
            if (this.render == null || this.render.IsDisposed)
                return;
            D2D.SolidColorBrush brush = null;
            switch(type)
            {
                case FillRectType.OverwriteCaret:
                    brush = this._factory.GetSolidColorBrush(this.OverwriteCaret);
                    this.render.FillRectangle(rect, brush);
                    break;
                case FillRectType.InsertCaret:
                    brush = this._factory.GetSolidColorBrush(this.InsertCaret);
                    this.render.FillRectangle(rect, brush);
                    break;
                case FillRectType.InsertPoint:
                    brush = this._factory.GetSolidColorBrush(this.Hilight);
                    this.render.FillRectangle(rect, brush);
                    break;
                case FillRectType.LineMarker:
                    brush = this._factory.GetSolidColorBrush(this.LineMarker);
                    this.render.DrawRectangle(rect, brush, EditView.LineMarkerThickness);
                    break;
                case FillRectType.UpdateArea:
                    brush = this._factory.GetSolidColorBrush(this.UpdateArea);
                    this.render.FillRectangle(rect, brush);
                    break;
                case FillRectType.Background:
                    brush = this._factory.GetSolidColorBrush(this.Background);
                    this.render.FillRectangle(rect, brush);
                    break;
            }
        }

        public void FillBackground(Rectangle rect)
        {
            if (this.render == null || this.render.IsDisposed)
                return;
            this.render.Clear(this.Background);
        }

        public void DrawOneLine(Document doc,LineToIndexTable lti, int row, double main_layout_x, double main_layout_y, PreDrawOneLineHandler PreDrawOneLine)
        {
            int lineLength = lti.GetLengthFromLineNumber(row);

            if (lineLength == 0 || this.render == null || this.render.IsDisposed)
                return;

            CombineTextLayout mainLayout = (CombineTextLayout)lti.GetLayout(row);

            mainLayout.Draw(main_layout_x, main_layout_y, (subLayout, subLayoutStartIndex, x, y) =>
            {
                MyTextLayout layout = (MyTextLayout)subLayout;

                if (PreDrawOneLine != null)
                    PreDrawOneLine(layout, lti, row, subLayoutStartIndex, x, y);

                if (layout.Markers != null)
                {
                    foreach (Marker sel in layout.Markers)
                    {
                        if (sel.length == 0 || sel.start == -1)
                            continue;
                        Color4 color = new Color4() { Alpha = sel.color.A, Red = sel.color.R, Blue = sel.color.B, Green = sel.color.G };
                        if (sel.hilight == HilightType.Url)
                            color = this.Url;
                        this.DrawMarkerEffect(layout, sel.hilight, (int)sel.start, (int)sel.length, x, y, sel.isBoldLine, color);
                    }
                }
                if (layout.Selects != null)
                {
                    foreach (Selection sel in layout.Selects)
                    {
                        if (sel.length == 0 || sel.start == -1)
                            continue;

                        this.DrawMarkerEffect(layout, HilightType.Select, (int)sel.start, (int)sel.length, x, y, false);
                    }
                }

                if (this.ShowFullSpace || this.ShowHalfSpace || this.ShowTab)
                {
                    long lineHeadIndex = lti.GetLineHeadLongIndex(row);
                    D2D.GeometryRealization geo = null;
                    int subLayoutLength = Math.Min(lineLength, Document.MaximumLineLength);
                    for (int i = 0; i < subLayoutLength; i++)
                    {
                        long indexMainLayout = i + subLayoutStartIndex + lineHeadIndex;
                        if (indexMainLayout >= lineHeadIndex + lineLength)
                            break;
                        Point pos = new Point(0, 0);
                        if (this.ShowTab && doc[indexMainLayout] == '\t')
                        {
                            pos = layout.GetPostionFromIndex(i);
                            geo = this._factory.CreateSymbol(ShowSymbol.Tab, this.format);
                        }
                        else if (this.ShowFullSpace && doc[indexMainLayout] == '　')
                        {
                            pos = layout.GetPostionFromIndex(i);
                            geo = this._factory.CreateSymbol(ShowSymbol.FullSpace, this.format);
                        }
                        else if (this.ShowHalfSpace && doc[indexMainLayout] == ' ')
                        {
                            pos = layout.GetPostionFromIndex(i);
                            geo = this._factory.CreateSymbol(ShowSymbol.HalfSpace, this.format);
                        }
                        if (geo != null)
                        {
                            var old_trans = this.render.Transform;
                            this.render.Transform = SharpDX.Matrix3x2.Translation(new Vector2((float)(pos.X + x), (float)(pos.Y + y)));
                            this.render.DrawGeometryRealization(geo, this._factory.GetSolidColorBrush(this.ControlChar));
                            this.render.Transform = old_trans;
                            geo = null;
                        }
                    }
                }

                layout.Draw(this.render, (float)x, (float)y, this._factory.GetSolidColorBrush(this.Foreground));
            });
        }

        IDisposable layerDisposer;
        public IDisposable BeginClipRect(Rectangle rect)
        {
            this.render.PushAxisAlignedClip(rect, D2D.AntialiasMode.Aliased);
            layerDisposer = new DummyDisposer(() =>
            {
                this.render.PopAxisAlignedClip();
            });
            return layerDisposer;
        }

        public void EndClipRect()
        {
            layerDisposer.Dispose();
        }

        public void SetTextColor(MyTextLayout layout,int start, int length, Color4? color)
        {
            if (color == null || start < 0)
                return;
            layout.SetDrawingEffect(this._factory.GetSolidColorBrush((Color4)color), new DW.TextRange(start, length));
        }

        public void DrawLine(Point from, Point to)
        {
            D2D.Brush brush = this._factory.GetSolidColorBrush(this.Foreground);
            D2D.StrokeStyle stroke = this._factory.GetStroke(HilightType.Sold);
            this.render.DrawLine(from, to, brush, 1.0f, stroke);
        }

        public const int BoldThickness = 2;
        public const int NormalThickness = 1;

        public void DrawMarkerEffect(MyTextLayout layout, HilightType type, int start, int length, double x, double y, bool isBold, Color4? effectColor = null)
        {
            if (type == HilightType.None || start < 0)
                return;

            float thickness = isBold ? BoldThickness : NormalThickness;

            Color4 color;
            if (effectColor != null)
                color = (Color4)effectColor;
            else if (type == HilightType.Select)
                color = this.Hilight;
            else
                color = this.Foreground;

            IMarkerEffecter effecter = null;
            D2D.SolidColorBrush brush = this._factory.GetSolidColorBrush(color);

            if (type == HilightType.Squiggle)
                effecter = new D2DSquilleLineMarker(this.render, brush, this._factory.GetStroke(HilightType.Squiggle), thickness);
            else if (type == HilightType.Select)
                effecter = new HilightMarker(this.render, brush);
            else if (type == HilightType.None)
                effecter = null;
            else
                effecter = new LineMarker(this.render, brush, this._factory.GetStroke(type), thickness);

            if (effecter != null)
            {
                bool isUnderline = type != HilightType.Select;

                DW.HitTestMetrics[] metrics = layout.HitTestTextRange(start, length, (float)x, (float)y);
                foreach (DW.HitTestMetrics metric in metrics)
                {
                    float offset = isUnderline ? metric.Height : 0;
                    effecter.Draw(metric.Left, metric.Top + offset, metric.Width, metric.Height);
                }
            }
        }

        public ITextLayout CreateLaytout(Document doc,long index,long length, SyntaxInfo[] syntaxCollection, IEnumerable<Marker> MarkerRanges, IEnumerable<Selection> SelectRanges, double WrapWidth)
        {
            if(doc == null)
            {
                return this.CreateLaytout("",syntaxCollection, MarkerRanges, SelectRanges, WrapWidth);
            }
            else
            {
                var str = doc.ToString(index,length);
                return this.CreateLaytout(str, syntaxCollection, MarkerRanges, SelectRanges, WrapWidth);
            }
        }

        public ITextLayout CreateLaytout(string str, SyntaxInfo[] syntaxCollection, IEnumerable<Marker> MarkerRanges, IEnumerable<Selection> SelectRanges,double WrapWidth)
        {
            float dpix,dpiy;
            this.GetDpi(out dpix,out dpiy);

            double layoutWidth = this.TextArea.Width;
            if (WrapWidth != LineToIndexTable.NONE_BREAK_LINE)
            {
                this.format.WordWrapping = DW.WordWrapping.Wrap;
                layoutWidth = WrapWidth;
            }
            else
            {
                this.format.WordWrapping = DW.WordWrapping.NoWrap;
            }

            bool hasNewLine = Util.IsHasNewLine(str);
            MyTextLayout newLayout = this._factory.GetTextLayout(
                str,
                this.format,
                layoutWidth,
                this.TextArea.Height,
                dpiy,
                hasNewLine && this.ShowLineBreak);
            newLayout.SetLineSpacing(this.emSize.Height);
            newLayout.SetLineBreakBrush(this._factory.GetSolidColorBrush(this.ControlChar));
            if (syntaxCollection != null)
            {
                foreach (SyntaxInfo s in syntaxCollection)
                {
                    if (s.length == 0 || s.start == -1)
                        continue;
                    D2D.SolidColorBrush brush = this._factory.GetSolidColorBrush(this.Foreground);
                    switch (s.type)
                    {
                        case TokenType.Comment:
                            brush = this._factory.GetSolidColorBrush(this.Comment);
                            break;
                        case TokenType.Keyword1:
                            brush = this._factory.GetSolidColorBrush(this.Keyword1);
                            break;
                        case TokenType.Keyword2:
                            brush = this._factory.GetSolidColorBrush(this.Keyword2);
                            break;
                        case TokenType.Literal:
                            brush = this._factory.GetSolidColorBrush(this.Literal);
                            break;
                    }
                    newLayout.SetDrawingEffect(brush, new DW.TextRange((int)s.index, (int)s.length));
                }
            }

            if (syntaxCollection != null)
            {
                foreach (SyntaxInfo s in syntaxCollection)
                {
                    if (s.length == 0 || s.start == -1)
                        continue;
                    D2D.SolidColorBrush brush = null;
                    switch (s.type)
                    {
                        case TokenType.Comment:
                            brush = this._factory.GetSolidColorBrush(this.Comment);
                            break;
                        case TokenType.Keyword1:
                            brush = this._factory.GetSolidColorBrush(this.Keyword1);
                            break;
                        case TokenType.Keyword2:
                            brush = this._factory.GetSolidColorBrush(this.Keyword2);
                            break;
                        case TokenType.Literal:
                            brush = this._factory.GetSolidColorBrush(this.Literal);
                            break;
                    }
                    newLayout.SetDrawingEffect(brush, new DW.TextRange((int)s.index, (int)s.length));
                }
            }

            if (MarkerRanges != null)
            {
                newLayout.Markers = MarkerRanges.ToArray();
                foreach (Marker sel in newLayout.Markers)
                {
                    if (sel.length == 0 || sel.start == -1)
                        continue;
                    if (sel.hilight == HilightType.Url)
                        newLayout.SetDrawingEffect(this._factory.GetSolidColorBrush(this.Url), new DW.TextRange((int)sel.start, (int)sel.length));
                }
            }

            if (SelectRanges != null)
            {
                newLayout.Selects = SelectRanges.ToArray();
                if (this.HilightForeground.Alpha > 0.0)
                {
                    foreach (Selection sel in SelectRanges)
                    {
                        if (sel.length == 0 || sel.start == -1)
                            continue;

                        newLayout.SetDrawingEffect(this._factory.GetSolidColorBrush(this.HilightForeground), new DW.TextRange((int)sel.start, (int)sel.length));
                    }
                }
            }

            this.format.WordWrapping = DW.WordWrapping.NoWrap;

            return newLayout;
       }

        bool _Disposed = false;
        public void Dispose()
        {
            if (!_Disposed)
            {
                this.Dispose(true);
                if (this.format != null)
                    this.format.Dispose();
            }
            this._Disposed = true;
        }

        protected virtual void Dispose(bool dispose)
        {
        }

        public virtual void GetDpi(out float dpix,out float dpiy)
        {
            throw new NotImplementedException();
        }

        public double GetScale()
        {
            float dpi;
            this.GetDpi(out dpi, out dpi);
            return dpi / 96.0;
        }
    }
}

#endif