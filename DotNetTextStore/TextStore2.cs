//#define TSF_DEBUG_OUTPUT
using System;
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
#if METRO
    public sealed class TextStore2 : TextStoreBase, IDisposable, ITextStoreACP2, ITfContextOwnerCompositionSink
    {
        #region 生成と破棄
        public void Dispose()
        {
            base.Dispose(false);
            GC.SuppressFinalize(this);
        }

        protected override void CreateThreadMgr()
        {
            if (_threadMgr == null)
            {
                Type clsid = Marshal.GetTypeFromCLSID(TfDeclarations.CLSID_TF_ThreadMgr);
                ITfThreadMgr2 threadMgr = Activator.CreateInstance(clsid) as ITfThreadMgr2;

                if (threadMgr == null)
                {
                    const string message = "スレッドマネージャーの生成に失敗しました。";
                    Debug.WriteLine(message);
                    throw new COMException(message, HRESULT.E_NOTIMPL);
                }

                this._threadMgr = threadMgr;
            }
        }

        #endregion
    }
#else
#endif
}
