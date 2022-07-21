using System;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security;

namespace DotNetTextStore.UnmanagedAPI.WinDef
{
    /// <summary>
    /// WindowsAPI の POINT 構造体。
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct POINT
    {
        public int x;
        public int y;
        public POINT(int x, int y)
        {
            this.x = x;
            this.y = y;
        }
    }


    /// <summary>
    /// WindowsAPI の RECT 構造体。
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct RECT
    {
        public int left;
        public int top;
        public int right;
        public int bottom;
        public RECT(int left, int top, int right, int bottom)
        {
            this.left = left;
            this.top = top;
            this.right = right;
            this.bottom = bottom;
        }
    }



}
