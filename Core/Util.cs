/*
 * Copyright (C) 2013 FooProject
 * * This program is free software; you can redistribute it and/or modify it under the terms of the GNU General Public License as published by
 * the Free Software Foundation; either version 3 of the License, or (at your option) any later version.

 * This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of 
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.

You should have received a copy of the GNU General Public License along with this program. If not, see <http://www.gnu.org/licenses/>.
 */
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
#if !WINDOWS_UWP
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;
#endif
using System.Text;
using Microsoft.VisualBasic;
#if WINUI
using Windows.Foundation.Diagnostics;
using Microsoft.UI.Xaml.Media.Imaging;
#endif

namespace FooEditEngine
{
    enum DebugLogLevel
    {
        Infomation, Important,
    }
    class DebugLog
    {
        public static void WriteLine(string message)
        {
            WriteLine(DebugLogLevel.Infomation, message);
        }

        public static void WriteLine(string message,params object[] args)
        {
            WriteLine(DebugLogLevel.Infomation, message, args);
        }

        public static void WriteLine(DebugLogLevel level,string message, params object[] args)
        {
#if DEBUG_DETAIL
            if(level == DebugLogLevel.Infomation) {
                if(args.Length > 0)
                    System.Diagnostics.Debug.WriteLine(message,args);
                else
                    System.Diagnostics.Debug.WriteLine(message);
            }
#endif
            if (level == DebugLogLevel.Important) {
                if(args.Length > 0)
                    System.Diagnostics.Debug.WriteLine(message,args);
                else
                    System.Diagnostics.Debug.WriteLine(message);
            }
        }
    }
    public class Util
    {
        public static string NormalizeLineFeed(IEnumerable<char> s, string linefeed)
        {
            StringBuilder result = new StringBuilder();
            int i = 0;
            var count = 0;
#if WINDOWS_UWP
            count = s.Count();
#else
            if (s.TryGetNonEnumeratedCount(out count) == false)
            {
                count = s.Count();
            }
#endif
            while (true)
            {
                if (i >= count)
                    break;
                if (s.ElementAt(i) == Document.LF_CHAR)
                {
                    result.Append(linefeed);
                    i++;
                }
                else if (s.ElementAt(i) == Document.CR_CHAR)
                {
                    if (i + 1 < count && s.ElementAt(i + 1) == Document.LF_CHAR)
                    {
                        result.Append(linefeed);
                        i += 2;
                    }
                    else
                    {
                        result.Append(linefeed);
                        i++;
                    }
                }
                else
                {
                    result.Append(s.ElementAt(i));
                    i++;
                }
            }
            return result.ToString();
        }

        public static IEnumerable<(string str, string linefeed)> EnumrateLine(IEnumerable<char> s)
        {
            StringBuilder result = new StringBuilder();
            var count = 0;
#if WINDOWS_UWP
            count = s.Count();
#else
            if (s.TryGetNonEnumeratedCount(out count) == false)
            {
                count = s.Count();
            }
#endif
            int i = 0;
            while (true)
            {
                if (i >= count)
                {
                    if (result.Length > 0)
                    {
                        yield return (result.ToString(), string.Empty);
                        result.Clear();
                    }
                    break;
                }
                if (s.ElementAt(i) == Document.LF_CHAR)
                {
                    yield return (result.ToString(), Document.LF_STR);
                    result.Clear();
                    i++;
                }
                else if (s.ElementAt(i) == Document.CR_CHAR)
                {
                    if (i + 1 < count && s.ElementAt(i + 1) == Document.LF_CHAR)
                    {
                        yield return (result.ToString(), Document.CRLF_STR);
                        result.Clear();
                        i += 2;
                    }
                    else
                    {
                        yield return (result.ToString(), Document.CR_STR);
                        result.Clear();
                        i++;
                    }
                }
                else
                {
                    result.Append(s.ElementAt(i));
                    i++;
                }
            }
        }

        public static bool IsHasNewLine(IEnumerable<char> s)
        {
            return GetNewLineLengthInTail(s) > 0;
        }

        public static int GetNewLineLengthInTail(IEnumerable<char> s)
        {
            var (len,type) =GetNewLineLengthInTailWithType(s);
            return len;
        }

        public static (int linefeedlen,string linefeedtype) GetNewLineLengthInTailWithType(IEnumerable<char> s)
        {
            int count = 0;
#if WINDOWS_UWP
            count = s.Count();
#else
            if (s.TryGetNonEnumeratedCount(out count) == false)
            {
                count = s.Count();
            }
#endif

            if (s == null || count == 0)
                return (0,null);
            int lastIndex = count - 2;
            if (lastIndex >= 0)
            {
                if (s.ElementAt(lastIndex) == Document.CR_CHAR)
                {
                    if (lastIndex + 1 < count && s.ElementAt(lastIndex + 1) == Document.LF_CHAR)
                        return (2,Document.CRLF_STR);
                    else
                        return (1,Document.LF_STR);
                }
            }
            lastIndex = count - 1;
            if (lastIndex >= 0) {
                char lastChar = s.ElementAt(lastIndex);
                if (lastChar == Document.LF_CHAR)
                    return (1,Document.LF_STR);
                if (lastChar == Document.CR_CHAR)
                    return (1, Document.CR_STR);
            }
            return (0,null);
        }

        public static string[] SpilitByLineFeed(string s)
        {
            List<string> result = new List<string>();
            foreach(var item in EnumrateLine(s))
            {
                result.Add(item.str);
            }
            return result.ToArray();
        }

        public static string TrimLineFeed(string s)
        {
            return s.Trim(Document.LF_CHAR).Trim(Document.CR_CHAR);
        }

        /// <summary>
        /// まとめて読み取るときに必要な長さ
        /// </summary>
        /// VECTOR256かつC#のchar型はUTF16なので、256bit/8/2の値を指定する
        public const int REQUIRE_BLUK_SCAN_CHAR_LEN = 16;

        /// <summary>
        /// 改行コードの有無に応じて移動量を返す
        /// </summary>
        /// <param name="span"></param>
        /// <returns>移動できなければ0を返し、改行コードが全く存在しなければ、REQUIRE_BLUK_SCAN_CHAR_LENを返す</returns>
        public static int GetMoveNextCountIfHasNotCRLF(ReadOnlySpan<char> span)
        {
#if WINDOWS_UWP
#else
            if (Vector256.IsHardwareAccelerated == false)
                return 0;

            var _CR_VECTOR = Vector256.Create((byte)'\r');
            var _LF_VECTOR = Vector256.Create((byte)'\n');

            if (span.Length < REQUIRE_BLUK_SCAN_CHAR_LEN)
                return 0;

            var memory_chunk = span;
            var vector_chunk = Vector256.Create(MemoryMarshal.Cast<char, byte>(memory_chunk));
            var vec_mask_cr = Avx2.MoveMask(Avx2.CompareEqual(vector_chunk, _CR_VECTOR));
            var vec_mask_lf = Avx2.MoveMask(Avx2.CompareEqual(vector_chunk, _LF_VECTOR));
            if ((vec_mask_cr | vec_mask_lf) == 0)
                return REQUIRE_BLUK_SCAN_CHAR_LEN;
#endif
            return 0;
        }


#if METRO || WINDOWS_UWP
        static float? _LogicalDpi;
        internal static void GetDpi(out float dpix, out float dpiy)
        {
            if(_LogicalDpi == null)
                _LogicalDpi = Windows.Graphics.Display.DisplayInformation.GetForCurrentView().LogicalDpi;
            dpix = _LogicalDpi.Value;
            dpiy = _LogicalDpi.Value;
        }

        internal static double GetScale()
        {
            float dpi;
            Util.GetDpi(out dpi, out dpi);
            return dpi / 96.0;
        }

        internal static Point GetClientPoint(Point screen, Windows.UI.Xaml.UIElement element)
        {
            //Windows10以降では補正する必要がある
            Windows.Foundation.Rect win_rect = Windows.UI.Xaml.Window.Current.CoreWindow.Bounds;
            screen = screen.Offset(-win_rect.X, -win_rect.Y);

            var gt = Windows.UI.Xaml.Window.Current.Content.TransformToVisual(element);
            return gt.TransformPoint(screen);
        }

        internal static Point GetPointInWindow(Point client, Windows.UI.Xaml.UIElement element)
        {
            //ウィンドウ内での絶対座標を取得する
            var gt = element.TransformToVisual(Windows.UI.Xaml.Window.Current.Content);
            return gt.TransformPoint(client);
        }

        internal static Point GetScreentPoint(Point client, Windows.UI.Xaml.UIElement element)
        {
            var gt = element.TransformToVisual(Windows.UI.Xaml.Window.Current.Content);
            Point p = gt.TransformPoint(client);

            //Windows10以降では補正する必要がある
            Windows.Foundation.Rect win_rect = Windows.UI.Xaml.Window.Current.CoreWindow.Bounds;
            var screenPoint = p.Offset(win_rect.X, win_rect.Y);
            return screenPoint;
        }
        internal static Windows.Foundation.Rect GetClientRect(Windows.Foundation.Rect screen, Windows.UI.Xaml.UIElement element)
        {
            //Windows10以降では補正する必要がある
            Windows.Foundation.Rect win_rect = Windows.UI.Xaml.Window.Current.CoreWindow.Bounds;
            screen.X -= win_rect.X;
            screen.Y -= win_rect.Y;

            var gt = Windows.UI.Xaml.Window.Current.Content.TransformToVisual(element);
            return gt.TransformBounds(screen);
        }
        internal static Windows.Foundation.Rect GetScreentRect(Windows.Foundation.Rect client, Windows.UI.Xaml.UIElement element)
        {
            //ウィンドウ内での絶対座標を取得する
            var gt = element.TransformToVisual(Windows.UI.Xaml.Window.Current.Content);
            Windows.Foundation.Rect screenRect = gt.TransformBounds(client);

            //Windows10以降では補正する必要がある
            Windows.Foundation.Rect win_rect = Windows.UI.Xaml.Window.Current.CoreWindow.Bounds;
            screenRect.X += win_rect.X;
            screenRect.Y += win_rect.Y;

            return screenRect;
        }
        internal static IEnumerable<char> GetEnumrator(string s)
        {
            char[] chars = s.ToCharArray();
            return chars;
        }
#elif WINUI
        static float? _LogicalDpi = 96.0f;

        internal static void SetDpi(float dpi)
        {
            _LogicalDpi = dpi;
        }

        internal static void GetDpi(out float dpix, out float dpiy)
        {
            dpix = _LogicalDpi.Value;
            dpiy = _LogicalDpi.Value;
        }

        internal static double GetScale()
        {
            float dpi;
            Util.GetDpi(out dpi, out dpi);
            return dpi / 96.0;
        }

        internal static Point GetClientPoint(Point screen, Microsoft.UI.Xaml.UIElement element)
        {
            //Windows10以降では補正する必要がある
            var appWnd = GetAppWindow(element);
            screen = screen.Offset(-appWnd.Position.X - appWnd.Size.Width + appWnd.ClientSize.Width, -appWnd.Position.Y - appWnd.Size.Height + appWnd.ClientSize.Height);

            var gt = Microsoft.UI.Xaml.Window.Current.Content.TransformToVisual(element);
            return gt.TransformPoint(screen);
        }

        internal static Point GetPointInWindow(Point client, Microsoft.UI.Xaml.UIElement element)
        {
            //ウィンドウ内での絶対座標を取得する
            var gt = element.TransformToVisual(element.XamlRoot.Content);
            return gt.TransformPoint(client);
        }

        internal static Point GetScreentPoint(Point client, Microsoft.UI.Xaml.UIElement element)
        {
            double scale = GetScale();
            var gt = element.TransformToVisual(element.XamlRoot.Content);
            Point p = gt.TransformPoint(client);
            p = p.Scale(scale); //XamlRootの(0,0)からクライアント座標までは自前でスケーリングしないといけない

            //Windows10以降では補正する必要がある
            var appWnd = GetAppWindow(element);
            var screenPoint = p.Offset(appWnd.Position.X + appWnd.Size.Width - appWnd.ClientSize.Width, appWnd.Position.Y + appWnd.Size.Height - appWnd.ClientSize.Height);
            return screenPoint;
        }
        internal static Windows.Foundation.Rect GetClientRect(Windows.Foundation.Rect screen, Microsoft.UI.Xaml.UIElement element)
        {
            //Windows10以降では補正する必要がある
            var appWnd = GetAppWindow(element);
            screen.X -= appWnd.Position.X;
            screen.Y -= appWnd.Position.Y;

            var gt = element.XamlRoot.Content.TransformToVisual(element);
            return gt.TransformBounds(screen);
        }
        internal static Windows.Foundation.Rect GetScreentRect(Windows.Foundation.Rect client, Microsoft.UI.Xaml.UIElement element)
        {
            //ウィンドウ内での絶対座標を取得する
            var gt = element.TransformToVisual(element.XamlRoot.Content);
            Windows.Foundation.Rect screenRect = gt.TransformBounds(client);

            //Windows10以降では補正する必要がある
            var appWnd = GetAppWindow(element);
            screenRect.X += appWnd.Position.X;
            screenRect.Y += appWnd.Position.Y;

            return screenRect;
        }

        internal static Point TryGetScreentPoint(Point client, Microsoft.UI.Xaml.UIElement element, out bool result)
        {
            result = false;

            double scale = GetScale();
            var gt = element.TransformToVisual(element.XamlRoot.Content);
            Point p = gt.TransformPoint(client);
            p = p.Scale(scale); //XamlRootの(0,0)からクライアント座標までは自前でスケーリングしないといけない

            //Windows10以降では補正する必要がある
            var appWnd = GetAppWindow(element);
            if (appWnd == null)
                return new Point(0, 0);
            var screenPoint = p.Offset(appWnd.Position.X + appWnd.Size.Width - appWnd.ClientSize.Width, appWnd.Position.Y + appWnd.Size.Height - appWnd.ClientSize.Height);
            result = true;
            return screenPoint;
        }

        internal static Microsoft.UI.Windowing.AppWindow GetAppWindow(Microsoft.UI.Xaml.UIElement element)
        {
            var hWnd = WinRT.Interop.WindowNative.GetWindowHandle(FooEditEngine.WinUI.FooTextBox.OwnerWindow);
            var windowID = Microsoft.UI.Win32Interop.GetWindowIdFromWindow(hWnd);
            var appWnd = Microsoft.UI.Windowing.AppWindow.GetFromWindowId(windowID);
            return appWnd;
        }
#elif WPF
        internal static void GetDpi(out float dpix, out float dpiy)
        {
            var dpiXProperty = typeof(System.Windows.SystemParameters).GetProperty("DpiX", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
            var dpiYProperty = typeof(System.Windows.SystemParameters).GetProperty("Dpi", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);

            dpix = (int)dpiXProperty.GetValue(null, null);
            dpiy = (int)dpiYProperty.GetValue(null, null);
        }

        internal static double GetScale()
        {
            float dpi;
            Util.GetDpi(out dpi, out dpi);
            return dpi / 96.0;
        }

        internal static Point GetClientPoint(Point screen, System.Windows.FrameworkElement element)
        {
            return element.PointFromScreen(screen);
        }

        internal static Point GetScreentPoint(Point client, System.Windows.FrameworkElement element)
        {
            return element.PointFromScreen(client);
        }

        internal static IEnumerable<char> GetEnumrator(string s)
        {
            return s;
        }
#else
        internal static IEnumerable<char> GetEnumrator(string s)
        {
            return s;
        }
#endif

        /// <remarks>T自体複製される</remarks>
        public static T ConvertAbsIndexToRelIndex<T>(T n, long StartIndex, long Length) where T : FooProject.Collection.IRange
        {
            n = Util.NormalizeIMaker<T>(n);

            long markerEnd = n.start + n.length - 1;

            long EndIndex = StartIndex + Length;

            if (n.start >= StartIndex && markerEnd <= EndIndex)
                n.start -= StartIndex;
            else if (n.start >= StartIndex && n.start <= EndIndex)
            {
                n.start -= StartIndex;
                n.length = EndIndex - StartIndex + 1;
            }
            else if (markerEnd >= StartIndex && markerEnd <= EndIndex)
            {
                n.start = 0;
                n.length = markerEnd - StartIndex + 1;
            }
            else if (n.start >= StartIndex && markerEnd <= EndIndex)
                n.start -= StartIndex;
            else if (n.start <= StartIndex && markerEnd > EndIndex)
            {
                n.start = 0;
                n.length = EndIndex - StartIndex + 1;
            }
            else
            {
                n.start = -1;
                n.length = 0;
            }
            return n;
        }

        public static bool IsWordSeparator(char c)
        {
            if (c == Document.CR_CHAR || c == Document.LF_CHAR || char.IsSeparator(c) || char.IsPunctuation(c) || CharUnicodeInfo.GetUnicodeCategory(c) == UnicodeCategory.MathSymbol)
                return true;
            else
                return false;
        }

        internal static void Swap<T>(ref T a, ref T b)
        {
            T c = b;
            b = a;
            a = c;
        }

        internal static string Generate(char c, int count)
        {
            StringBuilder tabstr = new StringBuilder();
            for (int j = count; j > 0; j--)
                tabstr.Append(c);
            return tabstr.ToString();
        }

        internal static Rectangle OffsetAndDeflate(Rectangle r, Size s)
        {
            return new Rectangle(r.X + s.Width,r.Y + s.Height, r.Width - s.Width, r.Height - s.Height);
        }

        /// <remarks>T自体複製される</remarks>
        public static T NormalizeIMaker<T>(T m) where T : FooProject.Collection.IRange
        {
            if (m.length > 0)
                return m;
            var new_m = m.DeepCopy();
            new_m.start = m.start + m.length;
            new_m.length = Math.Abs(m.length);
            return (T)new_m;
        }

        public static int RoundUp(double x)
        {
            return (int)(x + 0.5);
        }

    }
}
