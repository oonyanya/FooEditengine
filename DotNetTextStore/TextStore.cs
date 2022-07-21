using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Globalization;

using DotNetTextStore.UnmanagedAPI.TSF;
using DotNetTextStore.UnmanagedAPI.TSF.TextStore;
using DotNetTextStore.UnmanagedAPI.WinDef;
using DotNetTextStore.UnmanagedAPI.WinError;

namespace DotNetTextStore 
{


    //========================================================================================

    /// <summary>
    ///  テキストストアの実装を担うクラス。
    /// <pre>
    ///   各ドキュメント毎に実装すべき部分をイベントとして切り離して、テキストストアの実装を楽に
    ///   させる。
    /// </pre>
    /// <pre>
    ///   使用側としては各イベントハンドラの実装、フォーカスを取得した時に SetFocus() メソッドの
    ///   呼び出し、選択やテキストが変更された時に NotifySelectionChanged() メソッドや
    ///   NotifyTextChange() メソッドの呼び出しを行う必要がある。
    /// </pre>
    public sealed class TextStore :  TextStoreBase,IDisposable, ITextStoreACP, ITfContextOwnerCompositionSink
    {
        public delegate IntPtr GetHWndHandler();
        public event GetHWndHandler GetHWnd;

        #region "生成と破棄"
        /// <summary>
        /// 後処理
        /// </summary>
        public void Dispose()
        {
            base.Dispose(false);
            GC.SuppressFinalize(this);
        }
        #endregion

        #region ITextStoreACPの実装
        public void GetWnd(int i_viewCookie, out IntPtr o_hwnd)
        {
#if TSF_DEBUG_OUTPUT
            using(var dbgout = new DebugOut("{0}()", DebugOut.GetCaller()) )
#endif
            {
                if (GetHWnd != null)
                    o_hwnd = GetHWnd();
                else
                    o_hwnd = IntPtr.Zero;
            }
        }
        #endregion

        #region TSF 関連のラッパメソッド
        /// <summary>
        /// スレッドマネージャの生成
        /// </summary>
        /// 
        /// <exception cref="COMException">
        /// スレッドマネージャーの生成に失敗した場合。
        /// </exception>
        protected override void CreateThreadMgr()
        {
            if( _threadMgr == null )
            {
                TextFrameworkFunctions.TF_GetThreadMgr(out _threadMgr);
                if( _threadMgr == null )
                {
                    Type clsid = Marshal.GetTypeFromCLSID(TfDeclarations.CLSID_TF_ThreadMgr);
                    _threadMgr = Activator.CreateInstance(clsid) as ITfThreadMgr;

                    if( _threadMgr == null )
                    {
                        const string message = "スレッドマネージャーの生成に失敗しました。";
                        Debug.WriteLine(message);
                        throw new COMException(message, HRESULT.E_NOTIMPL);
                    }
                }
            }
        }

        #endregion

    }
}
