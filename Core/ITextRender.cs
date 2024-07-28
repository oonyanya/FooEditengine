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
    /// 点を表す構造体
    /// </summary>
    public struct Point
    {
        /// <summary>
        /// X座標
        /// </summary>
        public double X;
        /// <summary>
        /// Y座標
        /// </summary>
        public double Y;
        /// <summary>
        /// コンストラクター
        /// </summary>
        /// <param name="x">X座標</param>
        /// <param name="y">Y座標</param>
        public Point(double x, double y)
        {
            this.X = x;
            this.Y = y;
        }
        /// <summary>
        /// 比較演算子を実装します
        /// </summary>
        /// <param name="a">比較される方</param>
        /// <param name="b">比較対象</param>
        /// <returns>条件を満たすなら真</returns>
        public static bool operator ==(Point a, Point b)
        {
            return a.Equals(b);
        }

        /// <summary>
        /// 比較演算子を実装します
        /// </summary>
        /// <param name="a">比較される方</param>
        /// <param name="b">比較対象</param>
        /// <returns>条件を満たすなら真</returns>
        public static bool operator !=(Point a, Point b)
        {
            return !a.Equals(b);
        }

        /// <summary>
        /// 一致するかどうか
        /// </summary>
        /// <param name="o">比較対象</param>
        /// <returns>一致するなら真</returns>
        public override bool Equals(object o)
        {
            Point b = (Point)o;
            return this.X == b.X && this.Y == b.Y;
        }

        /// <summary>
        /// ハッシュを返す
        /// </summary>
        /// <returns>ハッシュを返す</returns>
        public override int GetHashCode()
        {
            int result = this.X.GetHashCode();
            result ^= this.Y.GetHashCode();
            return result;
        }

        /// <summary>
        /// 一定の倍率で拡大する
        /// </summary>
        /// <param name="scale">倍率</param>
        /// <returns></returns>
        public Point Scale(double scale)
        {
            this.X *= scale;
            this.Y *= scale;
            return this;
        }

        /// <summary>
        /// 一定方向に移動する
        /// </summary>
        /// <param name="x_offset">移動量X</param>
        /// <param name="y_offset">移動量Y</param>
        /// <returns></returns>
        public Point Offset(double x_offset, double y_offset)
        {
            this.X += x_offset;
            this.Y += y_offset;
            return this;
        }
#if WINFORM
        /// <summary>
        /// 変換演算子
        /// </summary>
        /// <param name="p"></param>
        public static implicit operator Point(System.Drawing.Point p)
        {
            return new Point(p.X, p.Y);
        }
        /// <summary>
        /// 変換演算子
        /// </summary>
        /// <param name="p"></param>
        public static implicit operator System.Drawing.Point(Point p)
        {
            return new System.Drawing.Point((int)p.X, (int)p.Y);
        }
        /// <summary>
        /// 変換演算子
        /// </summary>
        /// <param name="p"></param>
        public static implicit operator SharpDX.Mathematics.Interop.RawVector2(Point p)
        {
            return new SharpDX.Mathematics.Interop.RawVector2((float)p.X, (float)p.Y);
        }
#endif
#if WPF
        /// <summary>
        /// 変換演算子
        /// </summary>
        /// <param name="p"></param>
        public static implicit operator Point(System.Windows.Point p)
        {
            return new Point(p.X, p.Y);
        }
        /// <summary>
        /// 変換演算子
        /// </summary>
        /// <param name="p"></param>
        public static implicit operator System.Windows.Point(Point p)
        {
            return new System.Windows.Point(p.X, p.Y);
        }
        /// <summary>
        /// 変換演算子
        /// </summary>
        /// <param name="p"></param>
        public static implicit operator SharpDX.Mathematics.Interop.RawVector2(Point p)
        {
            return new SharpDX.Mathematics.Interop.RawVector2((float)p.X, (float)p.Y);
        }
#endif
#if METRO || WINDOWS_UWP || WINUI
        /// <summary>
        /// 変換演算子
        /// </summary>
        /// <param name="p"></param>
        public static implicit operator Point(Windows.Foundation.Point p)
        {
            return new Point(p.X, p.Y);
        }
        /// <summary>
        /// 変換演算子
        /// </summary>
        /// <param name="p"></param>
        public static implicit operator Windows.Foundation.Point(Point p)
        {
            return new Windows.Foundation.Point(p.X, p.Y);
        }
#endif
    }
    struct Size
    {
        public double Width;
        public double Height;
        public Size(double width, double height)
        {
            this.Width = width;
            this.Height = height;
        }

        /// <summary>
        /// 比較演算子を実装します
        /// </summary>
        /// <param name="a">比較される方</param>
        /// <param name="b">比較対象</param>
        /// <returns>条件を満たすなら真</returns>
        public static bool operator ==(Size a, Size b)
        {
            return a.Equals(b);
        }

        /// <summary>
        /// 比較演算子を実装します
        /// </summary>
        /// <param name="a">比較される方</param>
        /// <param name="b">比較対象</param>
        /// <returns>条件を満たすなら真</returns>
        public static bool operator !=(Size a, Size b)
        {
            return !a.Equals(b);
        }

        /// <summary>
        /// 一致するかどうか
        /// </summary>
        /// <param name="o">比較対象</param>
        /// <returns>一致するなら真</returns>
        public override bool Equals(object o)
        {
            Size b = (Size)o;
            return this.Width == b.Width && this.Height == b.Height;
        }

        /// <summary>
        /// ハッシュを返す
        /// </summary>
        /// <returns>ハッシュを返す</returns>
        public override int GetHashCode()
        {
            int result = this.Height.GetHashCode();
            result ^= this.Width.GetHashCode();
            return result;
        }
#if WINFORM
        public static implicit operator Size(System.Drawing.Size p)
        {
            return new Size(p.Width, p.Height);
        }
        public static implicit operator System.Drawing.Size(Size p)
        {
            return new System.Drawing.Size((int)p.Width, (int)p.Height);
        }
#endif
#if WPF
        public static implicit operator Size(System.Windows.Size p)
        {
            return new Size(p.Width, p.Height);
        }
        public static implicit operator System.Windows.Size(Size p)
        {
            return new System.Windows.Size(p.Width, p.Height);
        }
#endif
#if METRO || WINDOWS_UWP || WINUI
        public static implicit operator Size(Windows.Foundation.Size p)
        {
            return new Size(p.Width, p.Height);
        }
        public static implicit operator Windows.Foundation.Size(Size p)
        {
            return new Windows.Foundation.Size(p.Width, p.Height);
        }
#endif
    }
    struct Rectangle
    {
        public Point Location;
        public Size Size;
        public Point TopLeft
        {
            get { return this.Location; }
        }
        public Point TopRight
        {
            get { return new Point(this.Right, this.Location.Y); }
        }
        public Point BottomLeft
        {
            get { return new Point(this.Location.X, this.Bottom); }
        }
        public Point BottomRight
        {
            get { return new Point(this.Right, this.Bottom); }
        }
        public double Right
        {
            get { return this.X + this.Width; }
        }
        public double Bottom
        {
            get { return this.Y + this.Height; }
        }
        public double Height
        {
            get { return this.Size.Height; }
            set { this.Size.Height = value; }
        }
        public double Width
        {
            get { return this.Size.Width; }
            set { this.Size.Width = value; }
        }
        public double X
        {
            get { return this.Location.X; }
        }
        public double Y
        {
            get { return this.Location.Y; }
        }
        public Rectangle(double x, double y, double width, double height)
        {
            this.Location = new Point(x, y);
            this.Size = new Size(width, height);
        }
        public Rectangle(Point leftTop, Point bottomRight)
        {
            this.Location = leftTop;
            this.Size = new Size(bottomRight.X - leftTop.X, bottomRight.Y - leftTop.Y);
        }

        /// <summary>
        /// どの領域も指さないことを表す
        /// </summary>
        public static Rectangle Empty = new Rectangle(0, 0, 0, 0);

        /// <summary>
        /// 任意の点が領域内にあるなら真を返す
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        public bool IsHit(Point p)
        {
            if (p.X >= this.TopLeft.X &&
                p.Y >= this.TopLeft.Y &&
                p.X <= this.BottomRight.X &&
                p.Y <= this.BottomRight.Y)
                return true;
            return false;
        }

        /// <summary>
        /// 比較演算子を実装します
        /// </summary>
        /// <param name="a">比較される方</param>
        /// <param name="b">比較対象</param>
        /// <returns>条件を満たすなら真</returns>
        public static bool operator ==(Rectangle a, Rectangle b)
        {
            return a.Equals(b);
        }

        /// <summary>
        /// 比較演算子を実装します
        /// </summary>
        /// <param name="a">比較される方</param>
        /// <param name="b">比較対象</param>
        /// <returns>条件を満たすなら真</returns>
        public static bool operator !=(Rectangle a, Rectangle b)
        {
            return !a.Equals(b);
        }

        /// <summary>
        /// 一致するかどうか
        /// </summary>
        /// <param name="o">比較対象</param>
        /// <returns>一致するなら真</returns>
        public override bool Equals(object o)
        {
            Rectangle b = (Rectangle)o;
            return this.Location.Equals(b.Location) && this.Size.Equals(b.Size);
        }

        /// <summary>
        /// ハッシュを返す
        /// </summary>
        /// <returns>ハッシュを返す</returns>
        public override int GetHashCode()
        {
            int result = this.Location.GetHashCode();
            result ^= this.Size.GetHashCode();
            return result;
        }
#if WINFORM
        public static implicit operator Rectangle(System.Drawing.Rectangle p)
        {
            return new Rectangle(p.X,p.Y,p.Width,p.Height);
        }
        public static implicit operator System.Drawing.Rectangle(Rectangle p)
        {
            return new System.Drawing.Rectangle((int)p.X, (int)p.Y, (int)p.Width, (int)p.Height);
        }
        public static implicit operator SharpDX.Mathematics.Interop.RawRectangleF(Rectangle p)
        {
            return new SharpDX.Mathematics.Interop.RawRectangleF((float)p.X, (float)p.Y, (float)p.BottomRight.X, (float)p.BottomRight.Y);
        }
#endif
#if WPF
        public static implicit operator Rectangle(System.Windows.Rect p)
        {
            return new Rectangle(p.X,p.Y,p.Width,p.Height);
        }
        public static implicit operator System.Windows.Rect(Rectangle p)
        {
            return new System.Windows.Rect(p.X, p.Y, p.Width, p.Height);
        }
        public static implicit operator SharpDX.Mathematics.Interop.RawRectangleF(Rectangle p)
        {
            return new SharpDX.Mathematics.Interop.RawRectangleF((float)p.X, (float)p.Y, (float)p.BottomRight.X, (float)p.BottomRight.Y);
        }
#endif
#if METRO || WINDOWS_UWP || WINUI
        public static implicit operator Rectangle(Windows.Foundation.Rect p)
        {
            return new Rectangle(p.X, p.Y, p.Width, p.Height);
        }
        public static implicit operator Windows.Foundation.Rect(Rectangle p)
        {
            return new Windows.Foundation.Rect(p.X, p.Y, p.Width, p.Height);
        }
#endif
    }
    /// <summary>
    /// 色構造体
    /// </summary>
    public struct Color: IEqualityComparer<Color>
    {
        /// <summary>
        /// アルファ成分
        /// </summary>
        public byte A;
        /// <summary>
        /// 赤成分
        /// </summary>
        public byte R;
        /// <summary>
        /// 緑成分
        /// </summary>
        public byte G;
        /// <summary>
        /// 青成分
        /// </summary>
        public byte B;

        /// <summary>
        /// コンストラクター
        /// </summary>
        /// <param name="a">A成分</param>
        /// <param name="r">R成分</param>
        /// <param name="g">G成分</param>
        /// <param name="b">B成分</param>
        public Color(byte a = 255, byte r = 0, byte g = 0, byte b = 0)
        {
            this.A = a;
            this.R = r;
            this.B = b;
            this.G = g;
        }

        /// <summary>
        /// 等しいかどうかを調べます
        /// </summary>
        /// <param name="x">比較される方</param>
        /// <param name="y">比較する方</param>
        /// <returns>等しいなら真。そうでなければ偽</returns>
        public bool Equals(Color x, Color y)
        {
            return x.A == y.A && x.R == y.R && x.G == y.G && x.B == y.B;
        }

        /// <summary>
        /// ハッシュを得ます
        /// </summary>
        /// <param name="obj">Colorオブジェクト</param>
        /// <returns>ハッシュ</returns>
        public int GetHashCode(Color obj)
        {
            return this.A ^ this.R ^ this.B ^ this.G;
        }

        /// <summary>
        /// 一致するかどうか
        /// </summary>
        /// <param name="o">比較対象</param>
        /// <returns>一致するなら真</returns>
        public override bool Equals(object o)
        {
            Color b = (Color)o;
            return this.Equals(this,b);
        }

        /// <summary>
        /// ハッシュを返す
        /// </summary>
        /// <returns>ハッシュを返す</returns>
        public override int GetHashCode()
        {
            return this.GetHashCode(this);
        }
    }
    enum AlignDirection
    {
        Forward,
        Back,
    }
    enum ResourceType
    {
        All,
        Font,
        Brush,
        Antialias,
        InlineChar,
    }
    enum FillRectType
    {
        OverwriteCaret,
        InsertCaret,
        InsertPoint,
        LineMarker,
        UpdateArea,
        Background,
    }
    enum StringColorType
    {
        Forground,
        LineNumber,
    }
    class ChangedRenderRsourceEventArgs : EventArgs
    {
        public ResourceType type;
        public ChangedRenderRsourceEventArgs(ResourceType type)
        {
            this.type = type;
        }
    }
    delegate void ChangedRenderResourceEventHandler(object sender, ChangedRenderRsourceEventArgs e);
    interface ITextRender
    {
        /// <summary>
        /// 右から左に表示するなら真
        /// </summary>
        bool RightToLeft { get; set; }

        /// <summary>
        /// ドキュメントを表示する領域
        /// </summary>
        Rectangle TextArea { get; set; }

        /// <summary>
        /// 行番号の幅
        /// </summary>
        double LineNemberWidth { get; }

        /// <summary>
        /// タブの文字数
        /// </summary>
        int TabWidthChar { get; set; }

        /// <summary>
        /// 全角スペースを表示するかどうか
        /// </summary>
        bool ShowFullSpace { get; set; }

        /// <summary>
        /// 半角スペースを表示するかどうか
        /// </summary>
        bool ShowHalfSpace { get; set; }

        /// <summary>
        /// TABを表示するかどうか
        /// </summary>
        bool ShowTab { get; set; }

        /// <summary>
        /// 改行を表示するかどうか
        /// </summary>
        bool ShowLineBreak { get; set; }

        /// <summary>
        /// １文字当たりの高さと幅
        /// </summary>
        Size emSize { get; }

        /// <summary>
        /// 行の高さ。単位はem。
        /// </summary>
        double LineEmHeight { get { return 1.0; } set { } }

        /// <summary>
        /// 保持しているリソースに変化があったことを通知する
        /// </summary>
        event ChangedRenderResourceEventHandler ChangedRenderResource;

        /// <summary>
        /// RightToLeftの値が変わったことを通知する
        /// </summary>
        event EventHandler ChangedRightToLeft;

        /// <summary>
        /// 文字列を表示する
        /// </summary>
        /// <param name="str">文字列</param>
        /// <param name="x">x座標</param>
        /// <param name="y">y座標</param>
        /// <param name="align">書式方向</param>
        /// <param name="layoutRect">レイアウト領域</param>
        /// <param name="colorType">色</param>
        void DrawString(string str, double x, double y, StringAlignment align, Size layoutRect,StringColorType colorType = StringColorType.Forground);

        /// <summary>
        /// 行を表示する
        /// </summary>
        /// <param name="doc">ドキュメントオブジェクト</param>
        /// <param name="lti">LineToIndexオブジェクト</param>
        /// <param name="row">行</param>
        /// <param name="x">行の左上を表すX座標</param>
        /// <param name="y">行の左上を表すY座標</param>
        void DrawOneLine(Document doc,LineToIndexTable lti, int row, double x, double y);

        /// <summary>
        /// レイアウトを生成する
        /// </summary>
        /// <param name="str">文字列</param>
        /// <returns>ITextLayoutオブジェクト</returns>
        /// <param name="syntaxCollection">ハイライト関連の情報を保持しているコレクション</param>
        /// <param name="MarkerRanges">マーカーを保持しているコレクション。マーカーの開始位置は行の先頭を０とする相対位置としてください（位置が-1の場合表示しないこと）</param>
        /// <param name="Selections">選択領域を保持しているコレクション。マーカーの開始位置は行の先頭を０とする相対位置としてください（位置が-1の場合表示しないこと）</param>
        /// <param name="WrapWidth">折り返しの幅</param>
        ITextLayout CreateLaytout(string str, SyntaxInfo[] syntaxCollection, IEnumerable<Marker> MarkerRanges, IEnumerable<Selection> Selections,double WrapWidth);

        /// <summary>
        /// グリッパーを描く
        /// </summary>
        /// <param name="p">中心点</param>
        /// <param name="radius">半径</param>
        void DrawGripper(Point p, double radius);

        /// <summary>
        /// クリッピングを開始します
        /// </summary>
        /// <param name="rect">クリッピングする範囲</param>
        void BeginClipRect(Rectangle rect);

        /// <summary>
        /// クリッピングを終了します
        /// </summary>
        void EndClipRect();
    }
    interface IEditorRender : ITextRender
    {
        /// <summary>
        /// フォールティングエリアの幅
        /// </summary>
        double FoldingWidth { get; }

        /// <summary>
        /// キャッシュされたビットマップを描写する
        /// </summary>
        /// <param name="dstRect">転送先</param>
        /// <param name="srcRect">転送元</param>
        void DrawCachedBitmap(Rectangle dstRect,Rectangle srcRect);

        /// <summary>
        /// 線を描く
        /// </summary>
        /// <param name="from">開始座標</param>
        /// <param name="to">修了座標</param>
        void DrawLine(Point from, Point to);

        /// <summary>
        /// 描写したものをキャッシュする
        /// </summary>
        void CacheContent();

        /// <summary>
        /// キャッシュが存在するなら真を返し、そうでないなら偽を返す
        /// </summary>
        bool IsVaildCache();

        /// <summary>
        /// 四角形を描く
        /// </summary>
        /// <param name="rect"></param>
        /// <param name="type"></param>
        void FillRectangle(Rectangle rect,FillRectType type);

        /// <summary>
        /// ツリーに使用するマークを描く
        /// </summary>
        /// <param name="expand">展開済みなら真</param>
        /// <param name="x">x座標</param>
        /// <param name="y">y座標</param>
        void DrawFoldingMark(bool expand, double x, double y);

        /// <summary>
        /// 背景を塗りつぶす
        /// </summary>
        /// <param name="rect">塗りつぶすべき領域</param>
        void FillBackground(Rectangle rect);
    }

    enum StringAlignment
    {
        Left,
        Center,
        Right,
    }
    interface IPrintableTextRender : ITextRender
    {
        /// <summary>
        /// ヘッダーの高さ
        /// </summary>
        float HeaderHeight { get; }

        /// <summary>
        /// フッターの高さ
        /// </summary>
        float FooterHeight { get; }
    }
}
