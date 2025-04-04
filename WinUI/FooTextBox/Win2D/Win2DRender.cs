using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using System;

namespace FooEditEngine.WinUI
{
    /// <summary>
    /// 文字列のアンチエイリアシングモードを指定する
    /// </summary>
    public enum TextAntialiasMode
    {
        /// <summary>
        /// 最適なものが選択されます
        /// </summary>
        Default,
        /// <summary>
        /// ClearTypeでアンチエイリアシングを行います
        /// </summary>
        ClearType,
        /// <summary>
        /// グレイスケールモードでアンチエイリアシングを行います
        /// </summary>
        GrayScale,
        /// <summary>
        /// アンチエイリアシングを行いません
        /// </summary>
        Aliased,
    }

    internal class Win2DRender : Win2DRenderBase
    {
        public Win2DRender(FooTextBox textbox)
        {
            this._factory =  new Win2DResourceFactory();

            this.Foreground = ((SolidColorBrush)textbox.Foreground).Color;
            if(textbox.Background != null)
                this.Background = ((SolidColorBrush)textbox.Background).Color;
            this.HilightForeground = textbox.HilightForeground.Color;
            this.Hilight = textbox.Hilight.Color;
            this.Keyword1 = textbox.Keyword1.Color;
            this.Keyword2 = textbox.Keyword2.Color;
            this.Literal = textbox.Literal.Color;
            this.Url = textbox.URL.Color;
            this.ControlChar = textbox.ControlChar.Color;
            this.Comment = textbox.Comment.Color;
            this.InsertCaret = textbox.InsertCaret.Color;
            this.OverwriteCaret = textbox.OverwriteCaret.Color;
            this.LineMarker = textbox.LineMarker.Color;
            this.UpdateArea = textbox.UpdateArea.Color;
            this.LineNumber = textbox.LineNumber.Color;
        }

        CanvasImageSource CanvasImageSource;
        CanvasRenderTarget CanvasRenderTarget;
        bool _cached = false;
        public void CreateSurface(Microsoft.UI.Xaml.Shapes.Rectangle rect, double width, double height)
        {
            if (this.CanvasImageSource != null)
                this.CanvasImageSource = null;
            //デバイス依存の座標を渡さないといけない
            float dpix, dpiy;
            Util.GetDpi(out dpix, out dpiy);
            this.CanvasRenderTarget = new CanvasRenderTarget(this._factory.Device, (float)width, (float)height, dpiy);

            this.CanvasImageSource = new CanvasImageSource(this._factory.Device, (float)width, (float)height, dpiy);
            ImageBrush brush = new ImageBrush();
            brush.ImageSource = this.CanvasImageSource;
            rect.Fill = brush;
            _cached = false;
        }

        public bool Resize(Microsoft.UI.Xaml.Shapes.Rectangle rect, double width, double height)
        {
            if (this.CanvasImageSource != null)
            {
                Size canvasSize = this.CanvasImageSource.Size;
                if (canvasSize.Width != width || canvasSize.Height != height)
                {
                    try
                    {
                        this.CreateSurface(rect, width, height);
                        return true;
                    }
                    catch (Exception ex)
                    {
                        if (this._factory.ProcessDeviceLost(ex.HResult))
                        {
                            this.CreateSurface(rect, width, height);
                            this.OnChangedRenderResource(this, new ChangedRenderRsourceEventArgs(ResourceType.All));
                            this.IsReqestDraw = true;
                            return true;
                        }
                        else
                        {
                            throw;
                        }
                    }
                }
            }
            return false;
        }

        public override void DrawCachedBitmap(Rectangle dst, Rectangle src)
        {
            using (var session = this.CanvasImageSource.CreateDrawingSession(this.Background))
            {
                session.DrawImage(this.CanvasRenderTarget,dst,src);
            }
        }

        public override void CacheContent()
        {
            //DrawControlでキャッシュしてるので何もしない
        }

        public override bool IsVaildCache()
        {
            return _cached;
        }

        void DrawControl(Action<CanvasDrawingSession> action)
        {
            using (offScreenSession = this.CanvasRenderTarget.CreateDrawingSession())
            {
                offScreenSession.Clear(this.Background);
                offScreenSession.Antialiasing = CanvasAntialiasing.Aliased;
                action(offScreenSession);
                offScreenSession.Antialiasing = CanvasAntialiasing.Antialiased;
            }
            _cached = true;
            using (var session = this.CanvasImageSource.CreateDrawingSession(this.Background))
            {
                session.DrawImage(this.CanvasRenderTarget);
            }
        }

        public bool IsReqestDraw { get; set; }

        public void Draw(Microsoft.UI.Xaml.Shapes.Rectangle rectangle, Action<CanvasDrawingSession> action)
        {
            try
            {
                DrawControl(action);
                this.IsReqestDraw = false;
            }
            catch (Exception e)
            {
                if (this._factory.ProcessDeviceLost(e.HResult))
                {
                    this.CreateSurface(rectangle, rectangle.ActualWidth, rectangle.ActualHeight);
                    this.OnChangedRenderResource(this, new ChangedRenderRsourceEventArgs(ResourceType.All));
                    this.IsReqestDraw = true;
                }
                else
                {
                    throw;
                }
            }

        }
    }
}
