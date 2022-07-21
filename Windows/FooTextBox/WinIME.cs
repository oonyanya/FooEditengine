/*
 * Copyright (C) 2013 FooProject
 * * This program is free software; you can redistribute it and/or modify it under the terms of the GNU General Public License as published by
 * the Free Software Foundation; either version 3 of the License, or (at your option) any later version.

 * This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of 
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.

You should have received a copy of the GNU General Public License along with this program. If not, see <http://www.gnu.org/licenses/>.
 */
using System;
using System.Drawing;
using System.Windows.Forms;
using System.Runtime.InteropServices;

using GDIP = System.Drawing;

namespace FooEditEngine.Windows
{
    internal class StartCompstionEventArgs : EventArgs
    {
    }

    internal class EndCompstionEventArgs : EventArgs
    {
    }

    internal class ImeCompstionEventArgs : EventArgs
    {
        /// <summary>
        /// 確定時の文字列
        /// </summary>
        public string InputText;
        public ImeCompstionEventArgs(string text)
        {
            this.InputText = text;
        }
    }

    internal class ImeDocumentFeedEventArgs : EventArgs
    {
        /// <summary>
        /// 前後参照文脈変換につかう文字列
        /// </summary>
        public string Pragraph = string.Empty;
        /// <summary>
        /// IMEによって挿入される位置
        /// </summary>
        public int pos = 0;
    }

    internal class ImeQueryRecovertStringEventArgs : EventArgs
    {
        /// <summary>
        /// IMEによって調整された再変換文字列の開始位置
        /// </summary>
        public int offset;
        /// <summary>
        /// IMEによって調整された再変換文字列の長さ
        /// </summary>
        public int length;
        public ImeQueryRecovertStringEventArgs(int offset, int length)
        {
            this.offset = offset;
            this.length = length;
        }
    }

    internal class ImeReconvertStringEventArgs : EventArgs
    {
        /// <summary>
        /// IMEに再変換の対象となる文字列を調整させる場合は真
        /// </summary>
        public bool AutoAdjust = false;
        /// <summary>
        /// 再変換の対象となる文字列
        /// </summary>
        public string TargetString = string.Empty;
        /// <summary>
        /// 変換中の文字列の座標
        /// </summary>
        public GDIP.Point CaretPostion = GDIP.Point.Empty;
    }

    internal delegate void StartCompstionEventHandeler(object sender, StartCompstionEventArgs e);
    internal delegate void EndCompstionEventHandeler(object sender, EndCompstionEventArgs e);
    internal delegate void ImeCompstionEventHandeler(object sender, ImeCompstionEventArgs e);
    internal delegate void ImeDocumentFeedEventHandler(object sender, ImeDocumentFeedEventArgs e);
    internal delegate void ImeReconvertStringEventHandler(object sender, ImeReconvertStringEventArgs e);
    internal delegate void ImeQueryReconvertStringEventHandler(object sender,ImeQueryRecovertStringEventArgs e);

    internal class WinIME : NativeWindow
    {
        [StructLayout(LayoutKind.Sequential)]
        struct POINT
        {
            public POINT(int x, int y) { this.x = x; this.y = y; }
            public POINT(GDIP.Point pt) { x = pt.X; y = pt.Y; }
            public Int32 x, y;
        }

        [StructLayout(LayoutKind.Sequential)]
        struct RECT
        {
            public RECT(System.Drawing.Rectangle rect)
            {
                left = rect.Left;
                top = rect.Top;
                right = rect.Right;
                bottom = rect.Bottom;
            }
            public Int32 left, top, right, bottom;
        }

        [StructLayout(LayoutKind.Sequential)]
        struct COMPOSITIONFORM
        {
            public UInt32 style;
            public POINT currentPos;
            public RECT area;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        struct RECONVERTSTRING
        {
            public UInt32 dwSize;
            public UInt32 dwVersion;
            public UInt32 dwStrLen;
            public UInt32 dwStrOffset;
            public UInt32 dwCompStrLen;
            public UInt32 dwCompStrOffset;
            public UInt32 dwTargetStrLen;
            public UInt32 dwTargetStrOffset;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        class LOGFONT
        {
            public const int LF_FACESIZE = 32;
            public int lfHeight;
            public int lfWidth;
            public int lfEscapement;
            public int lfOrientation;
            public int lfWeight;
            public byte lfItalic;
            public byte lfUnderline;
            public byte lfStrikeOut;
            public byte lfCharSet;
            public byte lfOutPrecision;
            public byte lfClipPrecision;
            public byte lfQuality;
            public byte lfPitchAndFamily;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = LF_FACESIZE)]
            public string lfFaceName;
        }

        const Int32 GCS_COMPREADSTR = 0x0001;
        const Int32 GCS_COMPSTR = 0x0008;
        const Int32 GCS_RESULTSTR = 0x0800;
        const Int32 SCS_SETSTR = (GCS_COMPREADSTR | GCS_COMPSTR);
        const int WM_IME_STARTCOMPOSITION = 0x010D;
        const int WM_IME_ENDCOMPOSITION = 0x010E;
        const int WM_IME_COMPOSITION = 0x010F;
        const int WM_IME_NOTIFY = 0x0282;
        const int WM_IME_CHAR = 0x0286;
        const int WM_IME_REQUEST = 0x0288;
        const int CFS_POINT = 0x0002;
        const int IMR_RECONVERTSTRING = 0x0004;
        const int IMR_DOCUMENTFEED = 0x0007;
        const int SCS_QUERYRECONVERTSTRING = 0x00020000;

        [DllImport("imm32.dll")]
        static extern IntPtr ImmGetContext(IntPtr hWnd);
        
        [DllImport("imm32.dll")]
        static extern Int32 ImmReleaseContext(IntPtr hWnd, IntPtr context);

        [DllImport("imm32.dll")]
        static unsafe extern Int32 ImmGetCompositionStringW(IntPtr imContext, UInt32 index, void* out_string, UInt32 maxStringLen);

        [DllImport("imm32.dll")]
        static unsafe extern Int32 ImmSetCompositionStringW(IntPtr imContext, UInt32 index, void* lpComp, UInt32 dwCompLen, void* lpRead, UInt32 readLen);

        [DllImport("imm32.dll")]
        static unsafe extern Int32 ImmSetCompositionWindow(IntPtr imContext, COMPOSITIONFORM* compForm);

        [DllImport("imm32.dll")]
        static unsafe extern Int32 ImmSetCompositionFontW (IntPtr hIMC,[In, MarshalAs(UnmanagedType.LPStruct)] LOGFONT lplf);

        [DllImport("imm32.dll")]
        static extern UInt32 ImmGetProperty(IntPtr inputLocale, UInt32 index);

        [DllImport("user32.dll")]
        static extern IntPtr GetKeyboardLayout(UInt32 threadID);

        public WinIME(Control ctrl)
        {
            if (ctrl.IsHandleCreated)
                this.AssignHandle(ctrl.Handle);
            ctrl.HandleCreated += new EventHandler(ctrl_HandleCreated);
            ctrl.HandleDestroyed += new EventHandler(ctrl_HandleDestroyed);
            this.StartCompstion += new StartCompstionEventHandeler((s,e)=>{});
            this.EndCompstion +=new EndCompstionEventHandeler((s,e)=>{});
            this.ImeCompstion +=new ImeCompstionEventHandeler((s,e)=>{});
            this.ImeDocumentFeed += new ImeDocumentFeedEventHandler((s,e)=>{});
            this.ImeReconvert += new ImeReconvertStringEventHandler((s, e) => { });
            this.ImeQueryReconvert += new ImeQueryReconvertStringEventHandler((s,e)=>{});
        }

        public event StartCompstionEventHandeler StartCompstion;
        public event EndCompstionEventHandeler EndCompstion;
        public event ImeCompstionEventHandeler ImeCompstion;
        public event ImeDocumentFeedEventHandler ImeDocumentFeed;
        public event ImeReconvertStringEventHandler ImeReconvert;
        public event ImeQueryReconvertStringEventHandler ImeQueryReconvert;

        /// <summary>
        /// コンポジッションウィンドウの位置
        /// </summary>
        public GDIP.Point Location
        {
            get { throw new NotImplementedException(); }
            set
            {
                this.SetImeCompstionWindowPos(this.Handle, value.X,value.Y);
            }
        }

        /// <summary>
        /// 変換時のフォント
        /// </summary>
        public Font Font
        {
            get { throw new NotImplementedException(); }
            set
            {
                SetImeWindowFont(this.Handle, value);
            }
        }

        protected override void WndProc(ref Message m)
        {
            switch (m.Msg)
            {
                case WinIME.WM_IME_CHAR:
                    m.Result = IntPtr.Zero;
                    return;
                case WinIME.WM_IME_COMPOSITION:
                    if ((m.LParam.ToInt32() & WinIME.GCS_RESULTSTR) != 0)
                    {
                        string text = GetImeCompstionString(this.Handle);
                        this.ImeCompstion(this, new ImeCompstionEventArgs(text));
                    }
                    break;
                case WinIME.WM_IME_STARTCOMPOSITION:
                    this.StartCompstion(this, new StartCompstionEventArgs());
                    break;
                case WinIME.WM_IME_ENDCOMPOSITION:
                    this.EndCompstion(this, new EndCompstionEventArgs());
                    break;
                case WinIME.WM_IME_REQUEST:
                    if ((int)m.WParam == WinIME.IMR_DOCUMENTFEED)
                    {
                        m.Result = HandleIMR_DocumnetFeed(m.LParam);
                        return;
                    }
                    if ((int)m.WParam == WinIME.IMR_RECONVERTSTRING)
                    {
                        m.Result = HandleIMR_ReconvertString(m.LParam);
                        return;
                    }
                    break;
            }
            base.WndProc(ref m);
        }

        unsafe private IntPtr HandleIMR_ReconvertString(IntPtr lParam)
        {
            ImeReconvertStringEventArgs e = new ImeReconvertStringEventArgs();
            this.ImeReconvert(this, e);
            RECONVERTSTRING* reconv = (RECONVERTSTRING*)lParam.ToPointer();
            char* paragraph = (char*)((byte*)reconv + sizeof(RECONVERTSTRING));
            int reconvlen = sizeof(RECONVERTSTRING) + e.TargetString.Length * sizeof(char);
            if (reconv != null)
            {
                reconv->dwSize = (uint)sizeof(RECONVERTSTRING);
                reconv->dwVersion = 0;
                reconv->dwStrLen = (uint)e.TargetString.Length;
                reconv->dwStrOffset = (uint)sizeof(RECONVERTSTRING);
                reconv->dwTargetStrLen = 0;
                reconv->dwTargetStrOffset = 0;
                for (int i = 0; i < e.TargetString.Length; i++)
                    paragraph[i] = e.TargetString[i];
                if (e.AutoAdjust)
                {
                    IntPtr ime = ImmGetContext(this.Handle);
                    ImmSetCompositionStringW(ime, SCS_QUERYRECONVERTSTRING, reconv, (uint)reconvlen, (void*)IntPtr.Zero, 0);
                    ImmReleaseContext(this.Handle, ime);
                    this.ImeQueryReconvert(this, new ImeQueryRecovertStringEventArgs((int)reconv->dwTargetStrOffset, (int)reconv->dwTargetStrLen));
                }
                else
                {
                    reconv->dwCompStrLen = (uint)e.TargetString.Length;
                    reconv->dwCompStrOffset = 0;
                }

                this.Location = e.CaretPostion;
            }
            return new IntPtr(reconvlen);
        }

        unsafe private IntPtr HandleIMR_DocumnetFeed(IntPtr lParam)
        {
            ImeDocumentFeedEventArgs e = new ImeDocumentFeedEventArgs();
            this.ImeDocumentFeed(this, e);
            if (lParam.ToInt32() != 0)
            {
                if (e.pos > e.Pragraph.Length)
                    e.pos = e.Pragraph.Length;
                else if (e.pos < 0)
                    e.pos = 0;

                RECONVERTSTRING* reconv = (RECONVERTSTRING*)lParam.ToPointer();
                char* paragraph = (char*)((byte*)reconv + sizeof(RECONVERTSTRING));
                reconv->dwSize = (uint)sizeof(RECONVERTSTRING);
                reconv->dwVersion = 0;
                reconv->dwStrLen = (uint)e.Pragraph.Length;
                reconv->dwStrOffset = (uint)sizeof(RECONVERTSTRING);
                reconv->dwCompStrLen = 0;
                reconv->dwCompStrOffset = 0;
                reconv->dwTargetStrLen = 0;
                reconv->dwTargetStrOffset = (uint)e.pos * sizeof(char);
                for (int i = 0; i < e.Pragraph.Length; i++)
                    paragraph[i] = e.Pragraph[i];
            }
            return new IntPtr(sizeof(RECONVERTSTRING) + e.Pragraph.Length * sizeof(char));
        }

        void ctrl_HandleCreated(object sender, EventArgs e)
        {
            Control ctrl = (Control)sender;
            this.AssignHandle(ctrl.Handle);
        }

        void ctrl_HandleDestroyed(object sender, EventArgs e)
        {
            this.ReleaseHandle();
        }

        string GetImeCompstionString(IntPtr window)
        {
            string text;
            unsafe
            {
                IntPtr ime;
                int len;

                ime = ImmGetContext(window);
                len = ImmGetCompositionStringW(ime, GCS_RESULTSTR, null, 0);
                fixed (char* buf = new char[len + 1])
                {
                    ImmGetCompositionStringW(ime, GCS_RESULTSTR, (void*)buf, (uint)len);
                    buf[len] = '\0';
                    text = new String(buf);
                }
                ImmReleaseContext(window, ime);
            }
            return text;
        }

        void SetImeCompstionWindowPos(IntPtr window, int x,int y)
        {
            IntPtr imContext;
            imContext = ImmGetContext(window);

            COMPOSITIONFORM compForm = new COMPOSITIONFORM();
            unsafe
            {
                compForm.style = CFS_POINT;
                compForm.currentPos = new POINT(x, y);
                compForm.area = new RECT();

                ImmSetCompositionWindow(imContext, &compForm);
            }

            ImmReleaseContext(window, imContext);
        }

        void SetImeWindowFont(IntPtr window, Font font)
        {
            IntPtr imContext;
            LOGFONT logicalFont = new LOGFONT();

            font.ToLogFont(logicalFont);

            imContext = ImmGetContext(window);
            unsafe
            {
                ImmSetCompositionFontW(imContext, logicalFont);
            }
            ImmReleaseContext(window, imContext);
        }
    }
}
