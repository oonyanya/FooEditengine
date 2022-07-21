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
    struct SrcPoint
    {
        public double X;
        public int Row;
        public double OffsetY;
        public SrcPoint(double x, int row, double y)
        {
            if (row < 0)
                throw new ArgumentOutOfRangeException("マイナスを値を指定することはできません");
            this.X = x;
            this.Row = row;
            this.OffsetY = y;
        }
    }

    /// <summary>
    /// テキストの範囲を表す
    /// </summary>
    public struct TextRange
    {
        /// <summary>
        /// 開始インデックス
        /// </summary>
        public int Index;
        /// <summary>
        /// 長さ
        /// </summary>
        public int Length;

        /// <summary>
        /// 空の範囲を表す
        /// </summary>
        public static TextRange Null = new TextRange(0, 0);

        /// <summary>
        /// コンストラクター
        /// </summary>
        /// <param name="index">開始インデックス</param>
        /// <param name="length">長さ</param>
        public TextRange(int index, int length)
        {
            this.Index = index;
            this.Length = length;
        }
    }

    /// <summary>
    /// 文章の位置を示すクラス
    /// </summary>
    public struct TextPoint : IComparable<TextPoint>
    {
        /// <summary>
        /// 行番号
        /// </summary>
        public int row;
        /// <summary>
        /// 桁
        /// </summary>
        public int col;

        /// <summary>
        /// TextPointがドキュメント上のどこも指していないことを表す
        /// </summary>
        public static readonly TextPoint Null = new TextPoint(-1,-1);

        /// <summary>
        /// コンストラクター
        /// </summary>
        /// <param name="row">行番号</param>
        /// <param name="col">桁</param>
        public TextPoint(int row, int col)
        {
            this.row = row;
            this.col = col;
        }

        /// <summary>
        /// 比較演算子を実装します
        /// </summary>
        /// <param name="a">比較される方</param>
        /// <param name="b">比較対象</param>
        /// <returns>条件を満たすなら真</returns>
        public static bool operator <(TextPoint a, TextPoint b)
        {
            return a.CompareTo(b) == -1;
        }

        /// <summary>
        /// 比較演算子を実装します
        /// </summary>
        /// <param name="a">比較される方</param>
        /// <param name="b">比較対象</param>
        /// <returns>条件を満たすなら真</returns>
        public static bool operator <=(TextPoint a, TextPoint b)
        {
            return a.CompareTo(b) != 1;
        }

        /// <summary>
        /// 比較演算子を実装します
        /// </summary>
        /// <param name="a">比較される方</param>
        /// <param name="b">比較対象</param>
        /// <returns>条件を満たすなら真</returns>
        public static bool operator >(TextPoint a, TextPoint b)
        {
            return a.CompareTo(b) == 1;
        }

        /// <summary>
        /// 比較演算子を実装します
        /// </summary>
        /// <param name="a">比較される方</param>
        /// <param name="b">比較対象</param>
        /// <returns>条件を満たすなら真</returns>
        public static bool operator >=(TextPoint a, TextPoint b)
        {
            return a.CompareTo(b) != -1;
        }

        /// <summary>
        /// 比較演算子を実装します
        /// </summary>
        /// <param name="a">比較される方</param>
        /// <param name="b">比較対象</param>
        /// <returns>条件を満たすなら真</returns>
        public static bool operator ==(TextPoint a, TextPoint b)
        {
            return a.Equals(b);
        }

        /// <summary>
        /// 比較演算子を実装します
        /// </summary>
        /// <param name="a">比較される方</param>
        /// <param name="b">比較対象</param>
        /// <returns>条件を満たすなら真</returns>
        public static bool operator !=(TextPoint a, TextPoint b)
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
            TextPoint b = (TextPoint)o;
            return this.col == b.col && this.row == b.row;
        }

        /// <summary>
        /// ハッシュを返す
        /// </summary>
        /// <returns>ハッシュを返す</returns>
        public override int GetHashCode()
        {
            int result = this.row.GetHashCode();
            result ^= this.col.GetHashCode();
            return result;
        }

        #region IComparable<TextPoint> メンバー

        /// <summary>
        /// 比較する
        /// </summary>
        /// <param name="other">比較対象となるTextPointオブジェクト</param>
        /// <returns>相対値を返す</returns>
        public int CompareTo(TextPoint other)
        {
            if (this.row == other.row && this.col == other.col)
                return 0;
            if (this.row < other.row)
                return -1;
            else if (this.row == other.row && this.col < other.col)
                return -1;
            else
                return 1;
        }

        #endregion
    }

    /// <summary>
    /// 文章上での矩形エリアを表す
    /// </summary>
    public struct TextRectangle
    {
        TextPoint _TopLeft;
        
        TextPoint _BottomRight;

        /// <summary>
        /// 矩形エリアがドキュメントのどこも指していないことを表す
        /// </summary>
        public static readonly TextRectangle Null = new TextRectangle(new TextPoint(-1,-1),new TextPoint(-1,-1));

        /// <summary>
        /// 左上を表す
        /// </summary>
        public TextPoint TopLeft
        {
            get
            {
                return this._TopLeft;
            }
        }

        /// <summary>
        /// 右上を表す
        /// </summary>
        public TextPoint TopRight
        {
            get
            {
                return new TextPoint(this._TopLeft.row, this._BottomRight.col);
            }
        }

        /// <summary>
        /// 左下を表す
        /// </summary>
        public TextPoint BottomLeft
        {
            get
            {
                return new TextPoint(this._BottomRight.row, this._TopLeft.col);
            }
        }

        /// <summary>
        /// 右下を表す
        /// </summary>
        public TextPoint BottomRight
        {
            get
            {
                return this._BottomRight;
            }
        }

        /// <summary>
        /// コンストラクター
        /// </summary>
        /// <param name="row">行</param>
        /// <param name="col">桁</param>
        /// <param name="height">高さ</param>
        /// <param name="width">幅</param>
        public TextRectangle(int row, int col, int height,int width)
        {
            this._TopLeft = new TextPoint(row, col);
            this._BottomRight = new TextPoint(row + height - 1, col + width - 1);
        }
        
        /// <summary>
        /// コンストラクター
        /// </summary>
        /// <param name="topLeft">矩形の左上</param>
        /// <param name="bottomRight">矩形の右下</param>
        public TextRectangle(TextPoint topLeft, TextPoint bottomRight)
        {
            this._TopLeft = topLeft;
            this._BottomRight = bottomRight;
        }

        /// <summary>
        /// 比較演算子を実装します
        /// </summary>
        /// <param name="a">比較される方</param>
        /// <param name="b">比較対象</param>
        /// <returns>条件を満たすなら真</returns>
        public static bool operator ==(TextRectangle a, TextRectangle b)
        {
            return a.Equals(b);
        }

        /// <summary>
        /// 比較演算子を実装します
        /// </summary>
        /// <param name="a">比較される方</param>
        /// <param name="b">比較対象</param>
        /// <returns>条件を満たすなら真</returns>
        public static bool operator !=(TextRectangle a, TextRectangle b)
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
            TextRectangle b = (TextRectangle)o;
            return this._TopLeft == b._TopLeft && this._BottomRight == b._BottomRight;
        }

        /// <summary>
        /// ハッシュを返す
        /// </summary>
        /// <returns>ハッシュを返す</returns>
        public override int GetHashCode()
        {
            int result = this._TopLeft.GetHashCode();
            result ^= this._BottomRight.GetHashCode();
            return result;
        }
    }
}
