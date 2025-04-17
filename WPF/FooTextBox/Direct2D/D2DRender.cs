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
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Input;
using System.Windows.Interop;
using FooEditEngine;
using DotNetTextStore;
using DotNetTextStore.UnmanagedAPI.TSF;
using DotNetTextStore.UnmanagedAPI.WinDef;
using SharpDX;
using D2D = SharpDX.Direct2D1;
using DW = SharpDX.DirectWrite;
using D3D11 = SharpDX.Direct3D11;
using D3D9 = SharpDX.Direct3D9;
using DXGI = SharpDX.DXGI;

namespace FooEditEngine.WPF
{
    sealed class D2DRender : D2DRenderCommon, IEditorRender
    {
        DotNetTextStore.TextStore store;
        D3D11.Texture2D d3d11Texture;
        DXGI.Surface surface;
        D2D.Device1 device2d;
        D3D11.Device device;
        D3D9.Device device9;
        D3D9.Surface surface9;
        double fontSize;
        FontFamily fontFamily;
        FontWeight fontWeigth;
        FontStyle fontStyle;
        D3DImage imageSource;
        D2D.Bitmap bmpd2d;

        public D2DRender(FooTextBox textbox, double width, double height, Image image)
        {
            this.fontFamily = textbox.FontFamily;
            this.fontSize = textbox.FontSize;
            this.fontWeigth = textbox.FontWeight;
            this.fontStyle = textbox.FontStyle;
            this.Foreground = ToColor4(textbox.Foreground);
            this.Background = ToColor4(textbox.Background);
            this.ControlChar = ToColor4(textbox.ControlChar);
            this.Hilight = ToColor4(textbox.Hilight);
            this.Comment = ToColor4(textbox.Comment);
            this.Url = ToColor4(textbox.URL);
            this.Keyword1 = ToColor4(textbox.Keyword1);
            this.Keyword2 = ToColor4(textbox.Keyword2);
            this.Literal = ToColor4(textbox.Literal);
            this.InsertCaret = ToColor4(textbox.InsertCaret);
            this.OverwriteCaret = ToColor4(textbox.OverwriteCaret);
            this.LineMarker = ToColor4(textbox.LineMarker);
            this.UpdateArea = ToColor4(textbox.UpdateArea);
            this.LineNumber = ToColor4(textbox.LineNumber);
            this.HilightForeground = ToColor4(textbox.HilightForeground);
            this.store = textbox.TextStore;

            this.CreateDevice();

            this.ConstructRenderAndResource(width, height);
            this.InitTextFormat(this.fontFamily.Source, (float)this.fontSize, this.GetDWFontWeigth(this.fontWeigth), this.GetDWFontStyle(this.fontStyle));

            float dpiX, dpiY;
            this.GetDpi(out dpiX, out dpiY);
            this.imageSource = new D3DImage(dpiX,dpiY);
            this.imageSource.Lock();
            this.imageSource.SetBackBuffer(D3DResourceType.IDirect3DSurface9, this.surface9.NativePointer);  //設定しないとロード時に例外が発生する
            this.imageSource.Unlock();

            image.Source = this.imageSource;
        }

        public FontFamily FontFamily
        {
            get { return this.fontFamily; }
            set
            {
                this.fontFamily = value;
                this.InitTextFormat(this.fontFamily.Source, (float)this.fontSize, this.GetDWFontWeigth(this.fontWeigth), this.GetDWFontStyle(this.fontStyle));
                this.TabWidthChar = this.TabWidthChar;
            }
        }

        public double FontSize
        {
            get { return this.fontSize; }
            set
            {
                this.fontSize = value;
                this.InitTextFormat(this.fontFamily.Source, (float)value, this.GetDWFontWeigth(this.fontWeigth), this.GetDWFontStyle(this.fontStyle));
                this.TabWidthChar = this.TabWidthChar;
            }
        }

        public FontWeight FontWeigth
        {
            get
            {
                return this.fontWeigth;
            }
            set
            {
                this.fontWeigth = value;
                this.InitTextFormat(this.fontFamily.Source, (float)this.fontSize, this.GetDWFontWeigth(value), this.GetDWFontStyle(this.fontStyle));
            }
        }

        public FontStyle FontStyle
        {
            get
            {
                return this.fontStyle;
            }
            set
            {
                this.fontStyle = value;
                this.InitTextFormat(this.fontFamily.Source, (float)this.fontSize, this.GetDWFontWeigth(this.fontWeigth), this.GetDWFontStyle(this.fontStyle));
            }
        }

        public static Color4 ToColor4(System.Windows.Media.Color color)
        {
            return new Color4(color.R / 255.0f, color.G / 255.0f, color.B / 255.0f, color.A / 255.0f);
        }

        public bool Resize(double width, double height)
        {
            if (Math.Floor(width) != Math.Floor(this.imageSource.Width) || Math.Floor(height) != Math.Floor(this.imageSource.Height))
            {
                this.ReConstructRenderAndResource(width, height);
                this.imageSource.Lock();
                this.imageSource.SetBackBuffer(D3DResourceType.IDirect3DSurface9, this.surface9.NativePointer);
                this.imageSource.Unlock();
                return true;
            }
            return false;
        }

        public void ReConstructRenderAndResource(double width, double height)
        {
            this.DestructRenderAndResource();
            this.ConstructRenderAndResource(width, height);
        }

        public void DrawContent(EditView view, bool IsEnabled, Rectangle updateRect)
        {
            if (this.imageSource.IsFrontBufferAvailable)
            {
                this.imageSource.Lock();
                this.imageSource.SetBackBuffer(D3DResourceType.IDirect3DSurface9, this.surface9.NativePointer);

                base.BegineDraw();
                if (IsEnabled)
                    view.Draw(updateRect);
                else
                    this.FillBackground(updateRect);
                base.EndDraw();
                this.device.ImmediateContext.Flush();

                this.imageSource.AddDirtyRect(new Int32Rect(0, 0, (int)this.imageSource.PixelWidth, (int)this.imageSource.PixelHeight));
                this.imageSource.Unlock();
            }
        }

        public void DrawOneLine(Document doc, LineToIndexTable lti, int row, double x, double y)
        {
            base.DrawOneLine(doc,
                lti,
                row,
                x,
                y,
                this.DrawImeConversionLine
                );
        }

        private void DrawImeConversionLine(MyTextLayout layout, LineToIndexTable lti, int row, int subLayoutStartIndex, double x, double y)
        {
            if (InputMethod.Current.ImeState != InputMethodState.On)
                return;

            using (Unlocker locker = this.store.LockDocument(false))
            {
                int lineIndex = lti.GetIndexFromLineNumber(row);
                int lineLength = lti.GetLengthFromLineNumber(row);
                foreach (TextDisplayAttribute attr in this.store.EnumAttributes(lineIndex, lineIndex + lineLength))
                {
                    if (attr.startIndex == attr.endIndex)
                        continue;
                    int length = attr.endIndex - attr.startIndex;
                    int start = attr.startIndex - lineIndex;

                    HilightType type = HilightType.None;
                    Color4? color = null;
                    switch (attr.attribute.lsStyle)
                    {
                        case TF_DA_LINESTYLE.TF_LS_DOT:
                            type = HilightType.Dot;
                            color = this.GetColor4(attr.attribute.crLine);
                            break;
                        case TF_DA_LINESTYLE.TF_LS_SOLID:
                            type = HilightType.Sold;
                            color = this.GetColor4(attr.attribute.crLine);
                            break;
                        case TF_DA_LINESTYLE.TF_LS_DASH:
                            type = HilightType.Dash;
                            color = this.GetColor4(attr.attribute.crLine);
                            break;
                        case TF_DA_LINESTYLE.TF_LS_SQUIGGLE:
                            type = HilightType.Squiggle;
                            color = this.GetColor4(attr.attribute.crLine);
                            break;
                    }

                    if (attr.attribute.crBk.type != TF_DA_COLORTYPE.TF_CT_NONE)
                    {
                        type = HilightType.Select;
                        color = this.GetColor4(attr.attribute.crBk);
                    }

                    this.DrawMarkerEffect(layout, type, start - subLayoutStartIndex, length, x, y, attr.attribute.fBoldLine, color);

                    color = this.GetColor4(attr.attribute.crText);
                    if (color != null)
                    {
                        this.SetTextColor(layout, start - subLayoutStartIndex, length, color);
                        layout.Invaild = true;
                    }
                }
            }

        }

        private Color4? GetColor4(TF_DA_COLOR cr)
        {
            COLORREF colorref;
            switch (cr.type)
            {
                case TF_DA_COLORTYPE.TF_CT_SYSCOLOR:
                    colorref = new COLORREF(NativeMethods.GetSysColor((int)cr.indexOrColorRef));
                    break;
                case TF_DA_COLORTYPE.TF_CT_COLORREF:
                    colorref = new COLORREF(cr.indexOrColorRef);
                    break;
                default:
                    return null;
            }
            return new Color4(colorref.R / 255.0f, colorref.G / 255.0f, colorref.B / 255.0f, 1);
        }

        DW.FontStyle GetDWFontStyle(FontStyle style)
        {
            return (DW.FontStyle)Enum.Parse(typeof(DW.FontStyle), style.ToString());
        }

        DW.FontWeight GetDWFontWeigth(FontWeight weigth)
        {
            return (DW.FontWeight)Enum.Parse(typeof(DW.FontWeight), weigth.ToString());
        }

        public override void DrawCachedBitmap(Rectangle dstRect, Rectangle srcRect)
        {
            if (this.render == null || this.render.IsDisposed || this.cachedBitMap == null || this.cachedBitMap.IsDisposed)
                return;
            render.DrawBitmap(this.cachedBitMap, dstRect, 1.0f, D2D.BitmapInterpolationMode.Linear, srcRect);
        }

        public override void CacheContent()
        {
            if (this.render == null || this.cachedBitMap == null || this.cachedBitMap.IsDisposed || this.render.IsDisposed)
                return;
            this.render.Flush();
            this.cachedBitMap.CopyFromBitmap(this.bmpd2d);
            this.hasCache = true;
        }

        void CreateDevice()
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
                    this.device = new D3D11.Device(SharpDX.Direct3D.DriverType.Hardware, D3D11.DeviceCreationFlags.BgraSupport, level);
                    break;
                }
                catch
                {
                    continue;
                }
            }
            if (this.device == null)
                throw new PlatformNotSupportedException("DirectX10デバイスの作成に失敗しました");

            var dxgiDevice = this.device.QueryInterface<DXGI.Device>();
            this.device2d = new D2D.Device1(this._factory.D2DFactory, dxgiDevice);
            dxgiDevice.Dispose();

            this.render = new D2D.DeviceContext1(this.device2d, D2D.DeviceContextOptions.None);

            IntPtr DesktopWnd = NativeMethods.GetDesktopWindow();
            D3D9.Direct3DEx d3dex = new D3D9.Direct3DEx();

            D3D9.PresentParameters param = new D3D9.PresentParameters();
            param.Windowed = true;
            param.SwapEffect = D3D9.SwapEffect.Discard;
            param.DeviceWindowHandle = DesktopWnd;
            param.PresentationInterval = D3D9.PresentInterval.Default;

            try
            {
                this.device9 = new D3D9.DeviceEx(
                    d3dex,
                    0,
                    D3D9.DeviceType.Hardware,
                    DesktopWnd,
                    D3D9.CreateFlags.HardwareVertexProcessing | D3D9.CreateFlags.Multithreaded | D3D9.CreateFlags.FpuPreserve,
                    param);
            }
            catch
            {
                try
                {
                    this.device9 = new D3D9.DeviceEx(
                        d3dex,
                        0,
                        D3D9.DeviceType.Hardware,
                        DesktopWnd,
                        D3D9.CreateFlags.SoftwareVertexProcessing | D3D9.CreateFlags.Multithreaded | D3D9.CreateFlags.FpuPreserve,
                        param);
                }
                catch
                {
                    throw new PlatformNotSupportedException("DirectX9デバイスの作成に失敗しました");
                }
            }
            finally
            {
                d3dex.Dispose();
            }
        }

        void DestructDevice()
        {
            if (this.device != null)
            {
                this.device.Dispose();
                this.device = null;
            }
            if(this.render != null)
            {
                this.render.Dispose();
                this.render = null;
            }
            if (this.device9 != null)
            {
                this.device9.Dispose();
                this.device9 = null;
            }
        }

        public void ConstructRenderAndResource(double width, double height)
        {
            float dpiX, dpiY;
            this.GetDpi(out dpiX, out dpiY);
            D2D.RenderTargetProperties prop = new D2D.RenderTargetProperties(
                D2D.RenderTargetType.Default,
                new D2D.PixelFormat(DXGI.Format.B8G8R8A8_UNorm, D2D.AlphaMode.Premultiplied),
                dpiX,
                dpiY,
                D2D.RenderTargetUsage.None,
                D2D.FeatureLevel.Level_DEFAULT);

            D3D11.Texture2DDescription desc = new D3D11.Texture2DDescription();
            desc.Width = (int)width;
            desc.Height = (int)height;
            desc.MipLevels = 1;
            desc.ArraySize = 1;
            desc.Format = DXGI.Format.B8G8R8A8_UNorm;
            desc.SampleDescription = new DXGI.SampleDescription(1, 0);
            desc.Usage = D3D11.ResourceUsage.Default;
            desc.BindFlags = D3D11.BindFlags.RenderTarget | D3D11.BindFlags.ShaderResource;
            desc.CpuAccessFlags = D3D11.CpuAccessFlags.None;
            desc.OptionFlags = D3D11.ResourceOptionFlags.Shared;
            this.d3d11Texture = new D3D11.Texture2D(this.device, desc);

            this.surface = this.d3d11Texture.QueryInterface<DXGI.Surface>();

            DXGI.Resource resource = this.d3d11Texture.QueryInterface<DXGI.Resource>();
            IntPtr handel = resource.SharedHandle;
            D3D9.Texture texture = new D3D9.Texture(
                this.device9,
                this.d3d11Texture.Description.Width,
                this.d3d11Texture.Description.Height,
                1,
                D3D9.Usage.RenderTarget,
                D3D9.Format.A8R8G8B8,
                D3D9.Pool.Default,
                ref handel);
            this.surface9 = texture.GetSurfaceLevel(0);
            resource.Dispose();
            texture.Dispose();

            D2D.BitmapProperties bmpProp = new D2D.BitmapProperties();
            bmpProp.DpiX = dpiX;
            bmpProp.DpiY = dpiY;
            bmpProp.PixelFormat = new D2D.PixelFormat(DXGI.Format.B8G8R8A8_UNorm, D2D.AlphaMode.Premultiplied);
            this.bmpd2d = new D2D.Bitmap(this.render, this.surface, bmpProp);
            this.cachedBitMap = new D2D.Bitmap(this.render, new Size2((int)width, (int)height), bmpProp);
            this.hasCache = false;

            this.render.Target = this.bmpd2d;

            this.renderSize = new Size(width, height);
        }

        public void DestructRenderAndResource()
        {
            this.hasCache = false;
            this.render.Target = null;
            if (this.bmpd2d != null)
                this.bmpd2d.Dispose();
            if (this.cachedBitMap != null)
                this.cachedBitMap.Dispose();
            if (this.surface9 != null)
                this.surface9.Dispose();
            if (this.surface != null)
                this.surface.Dispose();
            if (this.d3d11Texture != null)
                this.d3d11Texture.Dispose();
        }

        public override void GetDpi(out float dpix, out float dpiy)
        {
            var dpiXProperty = typeof(SystemParameters).GetProperty("DpiX", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
            var dpiYProperty = typeof(SystemParameters).GetProperty("Dpi", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);

            dpix = (int)dpiXProperty.GetValue(null, null);
            dpiy = (int)dpiYProperty.GetValue(null, null);
        }

        protected override void Dispose(bool dispose)
        {
            base.Dispose(dispose);
            this.DestructRenderAndResource();
            this.DestructDevice();
        }

    }
}
