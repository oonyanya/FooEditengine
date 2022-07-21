/*
 * Copyright (C) 2013 FooProject
 * * This program is free software; you can redistribute it and/or modify it under the terms of the GNU General Public License as published by
 * the Free Software Foundation; either version 3 of the License, or (at your option) any later version.

 * This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of 
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.

You should have received a copy of the GNU General Public License along with this program. If not, see <http://www.gnu.org/licenses/>.
 */
#if WPF || WINFORM
using System;
using SharpDX;
using D2D = SharpDX.Direct2D1;
using DW = SharpDX.DirectWrite;

namespace FooEditEngine
{
    sealed class CustomTextRenderer : CallbackBase, DW.TextRenderer
    {
        D2DResourceFactory factory;

        public CustomTextRenderer(D2DResourceFactory f,Color4 defalut)
        {
            this.DefaultFore = defalut;
            this.factory = f;
        }

        public Color4 DefaultFore
        {
            get;
            set;
        }

        #region TextRenderer Members

        public Result DrawGlyphRun(object clientDrawingContext, float baselineOriginX, float baselineOriginY, D2D.MeasuringMode measuringMode, DW.GlyphRun glyphRun, DW.GlyphRunDescription glyphRunDescription, ComObject clientDrawingEffect)
        {
            D2D.RenderTarget render = clientDrawingContext as D2D.RenderTarget;
            if (render == null)
                return SharpDX.Result.Ok;

            D2D.SolidColorBrush foreBrush = this.factory.GetSolidColorBrush(this.DefaultFore);
            bool isDrawGlyphRun = true;
            if (clientDrawingEffect != null)
            {
                var drawingForeBrush = clientDrawingEffect as D2D.SolidColorBrush;
                var selectedEffect = clientDrawingEffect as SelectedEffect;
                var drawingEffect = clientDrawingEffect as DrawingEffect;

                if (drawingForeBrush != null)
                {
                    foreBrush = drawingForeBrush;
                }
                else if(selectedEffect != null)
                {
                    foreBrush = this.factory.GetSolidColorBrush(selectedEffect.Fore);
                }
                else if (drawingEffect != null)
                {
                    if (drawingEffect.Stroke == HilightType.Url)
                        foreBrush = this.factory.GetSolidColorBrush(drawingEffect.Fore);
                }
            }

            if(isDrawGlyphRun)
                render.DrawGlyphRun(new Vector2(baselineOriginX, baselineOriginY),
                    glyphRun,
                    foreBrush,
                    measuringMode);

            return SharpDX.Result.Ok;
        }

        RectangleF GetGlyphBound(DW.GlyphRun myGlyphRun, float baselineOriginX, float baselineOriginY,bool underline = false)
        {
            RectangleF bounds = new RectangleF();

            DW.FontMetrics fontMetrics = myGlyphRun.FontFace.Metrics;

            float ascentPixel = myGlyphRun.FontSize * fontMetrics.Ascent / fontMetrics.DesignUnitsPerEm;
            float dscentPixel = underline ?
                myGlyphRun.FontSize * (fontMetrics.Descent + fontMetrics.LineGap + fontMetrics.UnderlinePosition) / fontMetrics.DesignUnitsPerEm : 
                myGlyphRun.FontSize * (fontMetrics.Descent + fontMetrics.LineGap) / fontMetrics.DesignUnitsPerEm;

            float right = baselineOriginX;

            int glyphCount = myGlyphRun.Advances.Length;

            for (int i = 0; i < glyphCount; i++)
            {
                if (myGlyphRun.BidiLevel % 2 == 1)
                    right -= myGlyphRun.Advances[i];
                else
                    right += myGlyphRun.Advances[i];
            }

            bounds.Left = baselineOriginX;
            if (glyphCount > 0)
                bounds.Right = right;
            else if (myGlyphRun.Advances.Length == 1)
                bounds.Right = baselineOriginX + myGlyphRun.Advances[0];
            else
                bounds.Right = bounds.Left;
            bounds.Top = baselineOriginY - ascentPixel;
            bounds.Bottom = baselineOriginY + dscentPixel;

            return bounds;
        }

        public Result DrawInlineObject(object clientDrawingContext, float originX, float originY, DW.InlineObject inlineObject, bool isSideways, bool isRightToLeft, ComObject clientDrawingEffect)
        {
            D2D.RenderTarget render = clientDrawingContext as D2D.RenderTarget;
            if (render == null)
                return SharpDX.Result.Ok;

            inlineObject.Draw(render, this, originX, originY, isSideways, isRightToLeft, clientDrawingEffect);
            return Result.Ok;
        }

        public Result DrawStrikethrough(object clientDrawingContext, float baselineOriginX, float baselineOriginY, ref DW.Strikethrough strikethrough, ComObject clientDrawingEffect)
        {
            D2D.RenderTarget render = clientDrawingContext as D2D.RenderTarget;
            if (render == null)
                return SharpDX.Result.Ok;

            D2D.SolidColorBrush foreBrush = this.factory.GetSolidColorBrush(this.DefaultFore);
            DrawingEffect effect = clientDrawingEffect as DrawingEffect;
            if (clientDrawingEffect != null && clientDrawingEffect != null)
            {
                foreBrush = this.factory.GetSolidColorBrush(effect.Fore);
            }
            if (effect == null)
            {
                render.DrawLine(new Vector2(baselineOriginX, baselineOriginY + strikethrough.Offset),
                    new Vector2(baselineOriginX + strikethrough.Width - 1, baselineOriginY + strikethrough.Offset),
                    foreBrush,
                    GetThickness(render, strikethrough.Thickness));
            }
            return Result.Ok;
        }

        public Result DrawUnderline(object clientDrawingContext, float baselineOriginX, float baselineOriginY, ref DW.Underline underline, ComObject clientDrawingEffect)
        {
            D2D.RenderTarget render = clientDrawingContext as D2D.RenderTarget;
            if (render == null)
                return SharpDX.Result.Ok;

            D2D.SolidColorBrush foreBrush = this.factory.GetSolidColorBrush(this.DefaultFore);
            DrawingEffect effect = clientDrawingEffect as DrawingEffect;
            if (clientDrawingEffect != null && effect != null)
            {
                foreBrush = this.factory.GetSolidColorBrush(effect.Fore);
                float thickness = effect.isBoldLine ? D2DRenderCommon.BoldThickness : D2DRenderCommon.NormalThickness;
                if (effect.Stroke == HilightType.Squiggle)
                {
                    SquilleLineMarker marker = new D2DSquilleLineMarker(render, this.factory.GetSolidColorBrush(effect.Fore), this.factory.GetStroke(effect.Stroke), 1);
                    marker.Draw(
                        baselineOriginX, baselineOriginY + underline.Offset,
                        underline.Width, underline.RunHeight
                        );
                }
                else
                {
                    LineMarker marker = new LineMarker(render, this.factory.GetSolidColorBrush(effect.Fore), this.factory.GetStroke(effect.Stroke), GetThickness(render, thickness));
                    marker.Draw(
                        baselineOriginX, baselineOriginY + underline.Offset,
                        underline.Width, 0
                        );
                }
            }
            if (effect == null)
            {
                render.DrawLine(new Vector2(baselineOriginX, baselineOriginY + underline.Offset),
                    new Vector2(baselineOriginX + underline.Width - 1, baselineOriginY + underline.Offset),
                    foreBrush,
                    GetThickness(render,underline.Thickness));
            }

            return SharpDX.Result.Ok;
        }

        #endregion

        #region PixelSnapping Members

        public SharpDX.Mathematics.Interop.RawMatrix3x2 GetCurrentTransform(object clientDrawingContext)
        {
            D2D.RenderTarget render = clientDrawingContext as D2D.RenderTarget;
            if (render == null)
                throw new InvalidOperationException("render is null");

            SharpDX.Mathematics.Interop.RawMatrix3x2 d2Dmatrix = render.Transform;
            return d2Dmatrix;
        }

        public float GetPixelsPerDip(object clientDrawingContext)
        {
            D2D.RenderTarget render = clientDrawingContext as D2D.RenderTarget;
            if (render == null)
                throw new InvalidOperationException("render is null");

            return render.PixelSize.Width / 96f;
        }

        public bool IsPixelSnappingDisabled(object clientDrawingContext)
        {
            return false;
        }

        #endregion

        float GetThickness(D2D.RenderTarget render,float thickness)
        {
            if (render.AntialiasMode == D2D.AntialiasMode.Aliased)
                return (int)(thickness + 0.5);
            else
                return thickness;
        }

    }
}
#endif