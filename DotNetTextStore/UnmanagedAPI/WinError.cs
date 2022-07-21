using System;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security;


namespace DotNetTextStore.UnmanagedAPI.WinError
{
    public static class HRESULT
    {
        /// <summary>i_errorCode が成功した値かどうか調べる。</summary>
        public static bool Succeeded(int i_errorCode)
        {
            return i_errorCode >= 0;
        }

        /// <summary>
        /// The method was successful.
        /// </summary>
        public const int S_OK = 0;
        /// <summary>
        /// The method was successful.
        /// </summary>
        public const int S_FALSE = 0x00000001;
        /// <summary>
        /// An unspecified error occurred.
        /// </summary>
        public const int E_FAIL = unchecked((int)0x80004005);
        /// <summary>
        /// An invalid parameter was passed to the returning function.
        /// </summary>
        public const int E_INVALIDARG = unchecked((int)0x80070057);
        /// <summary>
        /// The method is not implemented.
        /// </summary>
        public const int E_NOTIMPL = unchecked((int)0x80004001);
        /// <summary>
        ///  The data necessary to complete this operation is not yet available.
        /// </summary>
        public const int E_PENDING = unchecked((int)0x8000000A);
        /// <summary>
        /// There is insufficient disk space to complete operation.
        /// </summary>
        public const int STG_E_MEDIUMFULL = unchecked((int)0x80030070);
        /// <summary>
        /// Attempted to use an object that has ceased to exist.
        /// </summary>
        public const int STG_E_REVERTED = unchecked((int)0x80030102);
    }
}