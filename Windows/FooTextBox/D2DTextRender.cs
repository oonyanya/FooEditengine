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
using System.Text;
using System.Windows;
using System.Drawing;
using FooEditEngine;
using SharpDX;
using D3D11 = SharpDX.Direct3D11;
using D2D = SharpDX.Direct2D1;
using DW = SharpDX.DirectWrite;
using DXGI = SharpDX.DXGI;
using System.Runtime.InteropServices;

namespace FooEditEngine.Windows
{

    sealed class D2DTextRender : D2DRenderCommon, IEditorRender, IDisposable
    {
        FooTextBox TextBox;
        string fontName;
        float fontSize;
        D2D.Device1 device2d;
        D3D11.Device device;
        DXGI.SwapChain swapchain;
        D2D.Bitmap bmp_d2d;
        DXGI.Device device_dxgi;
        DXGI.Factory2 factory_dxgi;

        public D2DTextRender(FooTextBox textbox)
        {
            this.TextBox = textbox;

            textbox.SizeChanged += new EventHandler(textbox_SizeChanged);
            textbox.FontChanged += new EventHandler(textbox_FontChanged);

            Size size = textbox.Size;
            this.fontName = textbox.Font.Name;
            this.fontSize = textbox.Font.Size;
            this.InitTextFormat(textbox.Font.Name, (float)textbox.Font.Size);

            this.CreateDevice();

            //初期化ができないので適当なサイズで作る
            this.ReConstructRenderAndResource(100, 100);
        }

        private void CreateDevice()
        {
            SharpDX.Direct3D.FeatureLevel[] levels = new SharpDX.Direct3D.FeatureLevel[]{
                SharpDX.Direct3D.FeatureLevel.Level_11_0,
                SharpDX.Direct3D.FeatureLevel.Level_10_1,
                SharpDX.Direct3D.FeatureLevel.Level_10_0,
                SharpDX.Direct3D.FeatureLevel.Level_9_3,
                SharpDX.Direct3D.FeatureLevel.Level_9_2,
                SharpDX.Direct3D.FeatureLevel.Level_9_1};
            foreach (var level in levels)
            {
                try
                {
                    this.device = new D3D11.Device(SharpDX.Direct3D.DriverType.Hardware , D3D11.DeviceCreationFlags.BgraSupport, level);
                    break;
                }
                catch
                {
                    continue;
                }
            }
            if (this.device == null)
                throw new PlatformNotSupportedException("DirectXデバイスの作成に失敗しました");

            device_dxgi = this.device.QueryInterface<DXGI.Device>();
            factory_dxgi = device_dxgi.Adapter.GetParent<DXGI.Factory2>();
        }

        public override void GetDpi(out float dpix, out float dpiy)
        {
            IntPtr hDc = NativeMethods.GetDC(IntPtr.Zero);
            dpix = NativeMethods.GetDeviceCaps(hDc, NativeMethods.LOGPIXELSX);
            dpiy = NativeMethods.GetDeviceCaps(hDc, NativeMethods.LOGPIXELSY);
            NativeMethods.ReleaseDC(IntPtr.Zero, hDc);
        }

        void textbox_FontChanged(object sender, EventArgs e)
        {
            FooTextBox textbox = (FooTextBox)sender;
            Font font = textbox.Font;
            this.fontName = font.Name;
            this.fontSize = font.Size;
            DW.FontWeight weigth = font.Bold ? DW.FontWeight.Bold : DW.FontWeight.Normal;
            DW.FontStyle style = font.Italic ? DW.FontStyle.Italic : DW.FontStyle.Normal;
            this.InitTextFormat(font.Name, font.Size,weigth,style);
        }

        void textbox_SizeChanged(object sender, EventArgs e)
        {
            FooTextBox textbox = (FooTextBox)sender;
            this.ReConstructRenderAndResource(this.TextBox.Width, this.TextBox.Height);
        }

        public static Color4 ToColor4(System.Drawing.Color color)
        {
            return new Color4(color.R / 255.0f, color.G / 255.0f, color.B / 255.0f, color.A / 255.0f);
        }


        public void DrawOneLine(Document doc,LineToIndexTable lti, int row, double x, double y)
        {
            this.DrawOneLine(doc,
                lti,
                row,
                x,
                y,
                null);
        }

        public override void DrawCachedBitmap(Rectangle dst,Rectangle src)
        {
            if (this.render == null || this.render.IsDisposed)
                return;
            render.DrawBitmap(this.cachedBitMap, dst, 1.0f, D2D.BitmapInterpolationMode.Linear, src);
        }

        public override void CacheContent()
        {
            if (this.render == null || this.cachedBitMap == null || this.cachedBitMap.IsDisposed || this.render.IsDisposed)
                return;
            render.Flush();
            this.cachedBitMap.CopyFromBitmap(this.bmp_d2d);
            this.hasCache = true;
        }

        public void DrawContent(EditView view,Rectangle updateRect)
        {
            base.BegineDraw();
            view.Draw(updateRect);
            base.EndDraw();
            swapchain.Present(1, DXGI.PresentFlags.None);
        }

        public void ReConstructRenderAndResource(double width, double height)
        {
            this.DestructRenderAndResource();
            this.ConstructRenderAndResource(width, height);
        }

        public void ConstructRenderAndResource(double width, double height)
        {

            float dpiX, dpiY;
            this.GetDpi(out dpiX, out dpiY);

            var desc = new DXGI.SwapChainDescription()
            {
                BufferCount = 1,
                ModeDescription = new DXGI.ModeDescription((int)width, (int)height,
                    new DXGI.Rational(60, 1), DXGI.Format.B8G8R8A8_UNorm),
                IsWindowed = true,
                OutputHandle = TextBox.Handle,
                SampleDescription = new DXGI.SampleDescription(1, 0),
                SwapEffect = DXGI.SwapEffect.Discard,
                Usage = DXGI.Usage.RenderTargetOutput
            };
            this.swapchain = new DXGI.SwapChain(factory_dxgi, device, desc);

            this.device2d = new D2D.Device1(this._factory.D2DFactory, device_dxgi);

            this.render = new D2D.DeviceContext1(this.device2d, D2D.DeviceContextOptions.None);

            D2D.BitmapProperties bmpProp = new D2D.BitmapProperties();
            bmpProp.DpiX = dpiX;
            bmpProp.DpiY = dpiY;
            bmpProp.PixelFormat = new D2D.PixelFormat(DXGI.Format.B8G8R8A8_UNorm, D2D.AlphaMode.Premultiplied);
            this.bmp_d2d = new D2D.Bitmap(this.render, DXGI.Surface.FromSwapChain(swapchain,0), bmpProp);
            this.cachedBitMap = new D2D.Bitmap(this.render, new SharpDX.Size2((int)width, (int)height), bmpProp);
            this.hasCache = false;

            this.render.Target = this.bmp_d2d;

            this.textRender = new CustomTextRenderer(this._factory, this.Foreground);

            this.renderSize = new Size(width, height);

            //デフォルト値を反映させる
            this.Foreground = ToColor4(this.TextBox.Foreground);
            this.Background = ToColor4(this.TextBox.Background);
            this.ControlChar = ToColor4(this.TextBox.ControlChar);
            this.Url = ToColor4(this.TextBox.Url);
            this.Keyword1 = ToColor4(this.TextBox.Keyword1);
            this.Keyword2 = ToColor4(this.TextBox.Keyword2);
            this.Literal = ToColor4(this.TextBox.Literal);
            this.Comment = ToColor4(this.TextBox.Comment);
            this.Hilight = ToColor4(this.TextBox.Hilight);
            this.LineMarker = ToColor4(this.TextBox.LineMarker);
            this.InsertCaret = ToColor4(this.TextBox.InsertCaret);
            this.OverwriteCaret = ToColor4(this.TextBox.OverwriteCaret);
            this.UpdateArea = ToColor4(this.TextBox.UpdateArea);
            this.HilightForeground = ToColor4(this.TextBox.HilightForeground);
        }

        public void DestructRenderAndResource()
        {
            this.hasCache = false;
            if (this.bmp_d2d != null)
                this.bmp_d2d.Dispose();
            if (this.cachedBitMap != null)
                this.cachedBitMap.Dispose();
            if (this.textRender != null)
                this.textRender.Dispose();
            if (this.device2d != null)
                this.device2d.Dispose();
            if (this.render != null)
                this.render.Dispose();
            if (this.swapchain != null)
                this.swapchain.Dispose();
        }
    }

    internal static class NativeMethods
    {
        public const int LOGPIXELSX = 88;
        public const int LOGPIXELSY = 90;

        [DllImport("gdi32.dll")]
        public static extern int GetDeviceCaps(IntPtr hDc, int nIndex);

        [DllImport("user32.dll")]
        public static extern IntPtr GetDC(IntPtr hWnd);

        [DllImport("user32.dll")]
        public static extern int ReleaseDC(IntPtr hWnd, IntPtr hDc);
    }
}
