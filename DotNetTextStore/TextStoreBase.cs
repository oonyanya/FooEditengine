// TSF のデバッグ表示を行うかどうか？
//#define TSF_DEBUG_OUTPUT
//#define TSF_DEBUG_OUTPUT_DISPLAY_ATTR
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Globalization;

using DotNetTextStore.UnmanagedAPI.TSF;
using DotNetTextStore.UnmanagedAPI.TSF.TextStore;
using DotNetTextStore.UnmanagedAPI.WinDef;
using DotNetTextStore.UnmanagedAPI.WinError;

namespace DotNetTextStore
{
#if TSF_DEBUG_OUTPUT
    /// <summary>コールスタックの階層にあわせてインデントしてデバッグ表示するクラス。</summary>
    public class DebugOut : IDisposable
    {
        public DebugOut(string i_string, params object[] i_params)
        {
            _text = string.Format(i_string, i_params);

            s_callCount++;
            Debug.WriteLine("");
            Debug.WriteLine(string.Format("{0, 4} : ↓↓↓ ", s_callCount) + _text);
        }

        public static void Print(string i_string, params object[] i_params)
        {
            Debug.WriteLine(i_string, i_params);
        }

        public void Dispose()
        {
            s_callCount++;
            Debug.WriteLine(string.Format("{0, 4} : ↑↑↑ ", s_callCount) + _text);
        }

        public static string GetCaller([CallerMemberName] string caller="")
        {
            return caller;
        }

        string      _text;
        static int  s_callCount = 0;
    }
#endif

    //=============================================================================================


    public struct TextDisplayAttribute
    {
        public int startIndex;
        public int endIndex;
        public TF_DISPLAYATTRIBUTE attribute;
    }

    //========================================================================================

    public struct TextSelection
    {
        public int start;
        public int end;
        public TextSelection(int start = 0,int end = 0)
        {
            this.start = start;
            this.end = end;
        }
    }
    

    /// <summary>Dispose() で TextStore のロック解除を行うクラス。</summary>
    public class Unlocker : IDisposable
    {
        /// <summary>コンストラクタ</summary>
        public Unlocker(TextStoreBase io_textStore)
        {
            _textStore = io_textStore;
        }
        /// <summary>ロックが成功したかどうか調べる。</summary>
        public bool IsLocked
        {
            get { return _textStore != null; }
        }
        /// <summary>アンロックを行う。</summary>
        void IDisposable.Dispose()
        {
            if (_textStore != null)
            {
                _textStore.UnlockDocument();
                _textStore = null;
            }
        }

        /// <summary>アンロックを行うテキストストア</summary>
        TextStoreBase _textStore;
    }

    public abstract class TextStoreBase
    {
        public delegate bool IsReadOnlyHandler();
        public event IsReadOnlyHandler IsReadOnly;

        public delegate bool IsLoadingHandler();
        public event IsLoadingHandler IsLoading;

        public event Func<int> GetSelectionCount;

        public delegate int GetStringLengthHandler();
        public event GetStringLengthHandler GetStringLength;

        public delegate void GetSelectionIndexHandler(int start_index, int max_count, out TextSelection[] sel);
        public event GetSelectionIndexHandler GetSelectionIndex;

        public delegate void SetSelectionIndexHandler(TextSelection[] sel);
        public event SetSelectionIndexHandler SetSelectionIndex;

        public delegate string GetStringHandler(int start, int length);
        public event GetStringHandler GetString;

        public delegate void InsertAtSelectionHandler(string i_value,ref int o_startIndex,ref int o_endIndex);
        public event InsertAtSelectionHandler InsertAtSelection;

        public delegate void GetScreenExtentHandler(
            out POINT o_pointTopLeft,
            out POINT o_pointBottomRight
        );
        public event GetScreenExtentHandler GetScreenExtent;

        public delegate void GetStringExtentHandler(
            int i_startIndex,
            int i_endIndex,
            out POINT o_pointTopLeft,
            out POINT o_pointBottomRight
        );
        public event GetStringExtentHandler GetStringExtent;

        public delegate bool CompositionStartedHandler();
        public event CompositionStartedHandler CompositionStarted;

        public delegate void CompostionUpdateHandler(int start, int end);
        public event CompostionUpdateHandler CompositionUpdated;

        public delegate void CompositionEndedHandler();
        public event CompositionEndedHandler CompositionEnded;

        bool _allow_multi_sel = false;

        #region "生成と破棄"
        public TextStoreBase(bool allow_multi_sel = false)
        {
#if TSF_DEBUG_OUTPUT
            using(var dbgout = new DebugOut("{0}()", DebugOut.GetCaller()) )
#endif
            {
                this._allow_multi_sel = allow_multi_sel;
                try
                {
                    // スレッドマネージャ－の生成
                    CreateThreadMgr();
                    // カテゴリマネージャーの生成
                    CreateCategoryMgr();
                    // 表示属性マネージャーの生成
                    CreateDisplayAttributeMgr();

                    // ドキュメントマネージャーの生成
                    _threadMgr.CreateDocumentMgr(out _documentMgr);

                    // スレッドマネージャのアクティブ化
                    int clientId = 0;
                    _threadMgr.Activate(out clientId);

                    // コンテキストの生成
                    _documentMgr.CreateContext(clientId, 0, this, out _context, out _editCookie);

                    // コンテキストの push
                    _documentMgr.Push(_context);

                    // ファンクションプロバイダーを取得する。
                    Guid guid = TfDeclarations.GUID_SYSTEM_FUNCTIONPROVIDER;
                    _threadMgr.GetFunctionProvider(ref guid, out _functionProvider);

                    // ITfReconversion オブジェクトを取得する。
                    var guidNull = new Guid();
                    var guidReconversion = new Guid("4cea93c0-0a58-11d3-8df0-00105a2799b5");    //ITfFnReconversionの定義から
                    object reconversion = null;
                    _functionProvider.GetFunction(
                        ref guidNull,
                        ref guidReconversion,
                        out reconversion
                    );
                    _reconversion = reconversion as ITfFnReconversion;

                    // MODEBIAS の初期化
                    uint guidAtom = 0;
                    Guid guidModebiasNone = TfDeclarations.GUID_MODEBIAS_NONE;
                    _categoryMgr.RegisterGUID(ref guidModebiasNone, out guidAtom);
                    _attributeInfo[0].attrID = TfDeclarations.GUID_PROP_MODEBIAS;
                    _attributeInfo[0].flags = AttributeInfoFlags.None;
                    _attributeInfo[0].currentValue.vt = (short)VarEnum.VT_EMPTY;
                    _attributeInfo[0].defaultValue.vt = (short)VarEnum.VT_I4;
                    _attributeInfo[0].defaultValue.data1 = (IntPtr)guidAtom;
                }
                catch (Exception exception)
                {
                    Debug.WriteLine(exception.Message);
                    Dispose(false);
                }
            }
        }

        /// <summary>
        /// オブジェクトの破棄を行う。このメソッドは必ず呼び出す必要があります
        /// </summary>
        /// <param name="flag"></param>
        protected void Dispose(bool flag)
        {
            if (flag)
                return;
#if TSF_DEBUG_OUTPUT
            using(var dbgout = new DebugOut("{0}()", DebugOut.GetCaller()) )
#endif
            {
                ReleaseComObject("_reconversion", ref _reconversion);
                ReleaseComObject("_functionProvider", ref _functionProvider);
                ReleaseComObject("_context", ref _context);
                DestroyDocumentMgr();
                DestroyDisplayAttributeMgr();
                DestroyCategoryMgr();
                DestroyThreadMgr();

                GC.SuppressFinalize(this);
            }
        }

        /// <summary>
        /// スレッドマネージャの生成。このメソッドの実装は必須です
        /// </summary>
        /// 
        /// <exception cref="COMException">
        /// スレッドマネージャーの生成に失敗した場合。
        /// </exception>
        protected virtual void CreateThreadMgr()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// スレッドマネージャーの解放。
        /// </summary>
        void DestroyThreadMgr()
        {
            if (_threadMgr != null)
            {
                try { _threadMgr.Deactivate(); }
                catch (Exception) { }

                ReleaseComObject("_threadMgr", ref _threadMgr);
            }
        }

        /// <summary>
        /// カテゴリマネージャーの生成。
        /// </summary>
        /// 
        /// <exception cref="COMException">
        /// カテゴリマネージャーの生成に失敗した場合。
        /// </exception>
        void CreateCategoryMgr()
        {
            if (_categoryMgr == null)
            {
                var clsid = Marshal.GetTypeFromCLSID(TfDeclarations.CLSID_TF_CategoryMgr);
                _categoryMgr = Activator.CreateInstance(clsid) as ITfCategoryMgr;

                if (_categoryMgr == null)
                {
                    const string message = "カテゴリマネージャーの生成に失敗しました。";
                    Debug.WriteLine(message);
                    throw new COMException(message, HRESULT.E_NOTIMPL);
                }
            }
        }

        /// <summary>
        /// カテゴリマネージャーの解放。
        /// </summary>
        void DestroyCategoryMgr()
        {
            ReleaseComObject("_categoryMgr", ref _categoryMgr);
        }

        /// <summary>
        /// 表示属性マネージャーの生成。
        /// </summary>
        /// 
        /// <exception cref="COMException">
        /// 表示属性マネージャーの生成に失敗した場合。
        /// </exception>
        void CreateDisplayAttributeMgr()
        {
            if (_displayAttributeMgr == null)
            {
                var clsid = Marshal.GetTypeFromCLSID(TfDeclarations.CLSID_TF_DisplayAttributeMgr);
                _displayAttributeMgr = Activator.CreateInstance(clsid) as ITfDisplayAttributeMgr;

                if (_displayAttributeMgr == null)
                {
                    const string message = "表示属性マネージャーの生成に失敗しました。";
                    Debug.WriteLine(message);
                    throw new COMException(message, HRESULT.E_NOTIMPL);
                }
            }
        }

        /// <summary>
        /// 表示属性マネージャーの解放。
        /// </summary>
        void DestroyDisplayAttributeMgr()
        {
            ReleaseComObject("_displayAttributeMgr", ref _displayAttributeMgr);
        }

        /// <summary>
        /// ドキュメントマネージャーの解放
        /// </summary>
        void DestroyDocumentMgr()
        {
            if (_documentMgr != null)
            {
                try { _documentMgr.Pop(PopFlags.TF_POPF_ALL); }
                catch (Exception) { }

                ReleaseComObject("_documentMgr", ref _documentMgr);
            }
        }

        /// <summary>
        /// COM オブジェクトのリリースとデバッグメッセージ出力。
        /// </summary>
        protected static void ReleaseComObject<ComObject>(
            string i_objectName,
            ref ComObject io_comObject
        )
        {
            if (io_comObject != null)
            {
                var refCount = Marshal.ReleaseComObject(io_comObject);
#if TSF_DEBUG_OUTPUT
                    Debug.WriteLine(
                        "Marshal.ReleaseComObject({0}) returns {1}.",
                        i_objectName,
                        refCount
                    );
#endif

                io_comObject = default(ComObject);
            }
        }
        #endregion

        #region "コントロール側が状況に応じて呼び出さなければいけない TSF に通知を送るメソッド"
        /// <summary>
        /// 選択領域が変更されたことをTSFに伝える。各種ハンドラ内からコールしてはいけない。
        /// </summary>
        public void NotifySelectionChanged()
        {
            if( _sink != null )
            {
#if TSF_DEBUG_OUTPUT
                DebugOut.Print(DebugOut.GetCaller());
#endif
                if ((_adviseFlags & AdviseFlags.TS_AS_SEL_CHANGE) != 0)
                    _sink.OnSelectionChange();
            }
        }

        
        //=========================================================================================


        /// <summary>
        /// テキストが変更されたことをTSFに伝える。各種ハンドラ内からコールしてはいけない。
        /// </summary>
        void NotifyTextChanged(TS_TEXTCHANGE textChange)
        {
            if( _sink != null )
            {
#if TSF_DEBUG_OUTPUT
                DebugOut.Print(DebugOut.GetCaller());
#endif
                if ((_adviseFlags & AdviseFlags.TS_AS_TEXT_CHANGE) != 0)
                    _sink.OnTextChange(0, ref textChange);
                _sink.OnLayoutChange(TsLayoutCode.TS_LC_CHANGE, 1);
            }
        }

        
        //=========================================================================================

        /// <summary>
        /// テキストが変更されたことをTSFに伝える。各種ハンドラ内からコールしてはいけない。
        /// </summary>
        /// <param name="start">開始位置</param>
        /// <param name="oldend">更新前の終了位置</param>
        /// <param name="newend">更新後の終了位置</param>
        /// <remarks>
        /// 詳しいことはITextStoreACPSink::OnTextChangeを参照してください
        /// </remarks>
        public void NotifyTextChanged(int start,int oldend,int newend)
        {
            if (_sink != null)
            {
#if TSF_DEBUG_OUTPUT
                DebugOut.Print(DebugOut.GetCaller());
#endif
                if ((_adviseFlags & AdviseFlags.TS_AS_TEXT_CHANGE) != 0)
                {
                    var textChange = new TS_TEXTCHANGE();
                    textChange.start = start;
                    textChange.oldEnd = oldend;
                    textChange.newEnd = newend;

                    _sink.OnTextChange(0, ref textChange);
                }
                _sink.OnLayoutChange(TsLayoutCode.TS_LC_CHANGE, 1);
            }
        }

        /// <summary>
        /// テキストが変更されたことをTSFに伝える。各種ハンドラ内からコールしてはいけない。
        /// </summary>
        /// <param name="i_oldLength"></param>
        /// <param name="i_newLength"></param>
        public void NotifyTextChanged(int i_oldLength, int i_newLength)
        {
            if( _sink != null )
            {
#if TSF_DEBUG_OUTPUT
                DebugOut.Print(DebugOut.GetCaller());
#endif
                if ((_adviseFlags & AdviseFlags.TS_AS_TEXT_CHANGE) != 0)
                {
                    var textChange = new TS_TEXTCHANGE();
                    textChange.start = 0;
                    textChange.oldEnd = i_oldLength;
                    textChange.newEnd = i_newLength;

                    _sink.OnTextChange(0, ref textChange);
                }
                _sink.OnLayoutChange(TsLayoutCode.TS_LC_CHANGE, 1);
            }
        }

        
        
        //=========================================================================================


        /// <summary>コントロールがフォーカスを取得した時に呼び出さなければいけない。</summary>
        public void SetFocus()
        {
#if TSF_DEBUG_OUTPUT
            DebugOut.Print(DebugOut.GetCaller());
#endif
            if (_threadMgr != null)
                _threadMgr.SetFocus(_documentMgr);
        }
        #endregion "コントロール側が状況に応じて呼び出さなければいけない TSF に通知を送るメソッド"

        #region ロック関連
        /// <summary>
        /// ドキュメントのロックを行う。
        /// </summary>
        /// <param name="i_writable">読み書き両用ロックか？false の場合、読み取り専用。</param>
        /// <returns>Unlocker のインスタンスを返す。失敗した場合 null を返す。</returns>
        public Unlocker LockDocument(bool i_writable)
        {
            #if TSF_DEBUG_OUTPUT
            using(var dbgout = new DebugOut("{0}({1})", DebugOut.GetCaller(),
                                                        i_writable) )
            #endif
            {
                lock(this)
                {
                    if( this._lockFlags == 0 )
                    {
                        if( i_writable )
                            this._lockFlags = LockFlags.TS_LF_READWRITE;
                        else
                            this._lockFlags = LockFlags.TS_LF_READ;

                        #if TSF_DEBUG_OUTPUT
                            Debug.WriteLine("LockDocument is succeeded.");
                        #endif

                        return new Unlocker(this);
                    }
                    else
                    {
                        #if TSF_DEBUG_OUTPUT
                            Debug.WriteLine("LockDocument is failed. {0}", _lockFlags);
                        #endif

                        return null;
                    }
                }
            }
        }


        //========================================================================================

        
        /// <summary>
        /// ドキュメントのロックを行う。
        /// </summary>
        /// <param name="i_writable">読み書き両用ロックか？false の場合、読み取り専用。</param>
        /// <returns>Unlocker のインスタンスを返す。失敗した場合 null を返す。</returns>
        public Unlocker LockDocument(LockFlags i_flags)
        {
            #if TSF_DEBUG_OUTPUT
            using(var dbgout = new DebugOut("{0}({1})", DebugOut.GetCaller(),
                                                        i_flags) )
            #endif
            {
                lock(this)
                {
                    if( this._lockFlags == 0 )
                    {
                        this._lockFlags = i_flags;

                        #if TSF_DEBUG_OUTPUT
                            Debug.WriteLine("LockDocument is succeeded.");
                        #endif

                        return new Unlocker(this);
                    }
                    else
                    {
                        #if TSF_DEBUG_OUTPUT
                            Debug.WriteLine("LockDocument is failed. {0}", _lockFlags);
                        #endif

                        return null;
                    }
                }
            }
        }


        //========================================================================================

        
        /// <summary>
        /// ドキュメントのアンロックを行う。
        /// </summary>
        public void UnlockDocument()
        {
            #if TSF_DEBUG_OUTPUT
            using(var dbgout = new DebugOut("{0}()", DebugOut.GetCaller()) )
            #endif
            {
                lock(this)
                {
                    _lockFlags = 0;
                }

                if( _pendingLockUpgrade )
                {
                    _pendingLockUpgrade = false;
                    int sessionResult;
                    RequestLock(LockFlags.TS_LF_READWRITE, out sessionResult);
                }
            }
        }


        //========================================================================================

        
        /// <summary>
        /// 指定されたフラグでロックしている状態かどうか調べる。
        /// </summary>
        /// <param name="i_lockFlags"></param>
        /// <returns>ロックされている場合は true, されていない場合は false を返す。</returns>
        public bool IsLocked(LockFlags i_lockFlags)
        {
            #if TSF_DEBUG_OUTPUT
            using(var dbgout = new DebugOut("{0}({1})",
                                            DebugOut.GetCaller(), i_lockFlags) )
            #endif
            {
                #if TSF_DEBUG_OUTPUT
                    Debug.WriteLine(
                        "IsLocked() returns " + ((this._lockFlags & i_lockFlags) == i_lockFlags)
                    );
                #endif
                return (this._lockFlags & i_lockFlags) == i_lockFlags;
            }
        }

        /// <summary>
        /// ロックされているか調べる
        /// </summary>
        /// <returns>ロックされているなら真、そうでなければ偽を返す</returns>
        public bool IsLocked()
        {
#if TSF_DEBUG_OUTPUT
            using(var dbgout = new DebugOut("{0}",
                                            DebugOut.GetCaller()) )
#endif
            {
                bool retval = this._lockFlags != 0;
#if TSF_DEBUG_OUTPUT
                    Debug.WriteLine(
                        "IsLocked() returns " + retval
                    );
#endif
                return retval;
            }
        }
        #endregion

        #region ITextStroeACP,ITextStoreACP2の共通部分

        /// <summary>
        /// 文字列を挿入する。
        /// </summary>
        public void InsertTextAtSelection(string s)
        {
            TS_TEXTCHANGE textChange = new TS_TEXTCHANGE();

            using (var unlocker = LockDocument(true))
            {
                if (unlocker != null)
                {
                    int startIndex, endIndex;

                    InsertTextAtSelection(
                        UnmanagedAPI.TSF.TextStore.InsertAtSelectionFlags.TF_IAS_NOQUERY,
                        s.ToCharArray(),
                        s.Length,
                        out startIndex,
                        out endIndex,
                        out textChange
                    );
                }
            }

            // シンクの OnSelectionChange() をコール。
            NotifySelectionChanged();
            NotifyTextChanged(textChange);
        }

        public void InsertEmbeddedAtSelection(
            InsertAtSelectionFlags flags,
            object obj,
            out int start,
            out int end,
            out TS_TEXTCHANGE change
        )
        {
#if TSF_DEBUG_OUTPUT
            using(var dbgout = new DebugOut("{0}()", DebugOut.GetCaller()) )
#endif
            {
                throw new NotImplementedException();
            }
        }

        public void AdviseSink(ref Guid i_riid, object i_unknown, AdviseFlags i_mask)
        {
#if TSF_DEBUG_OUTPUT
            using(var dbgout = new DebugOut("{0}()", DebugOut.GetCaller()) )
#endif
            {
                if (i_riid != new Guid("22d44c94-a419-4542-a272-ae26093ececf")) //ITextStoreACPSinkの定義より
                {
                    throw new COMException(
                        "ITextStoreACPSink 以外のIIDが渡されました。",
                        UnmanagedAPI.WinError.HRESULT.E_INVALIDARG
                    );
                }

                // 既存のシンクのマスクのみを更新
                if (_sink == i_unknown)
                {
                    _adviseFlags = i_mask;
                }
                // シンクを複数登録しようとした
                else if (_sink != null)
                {
                    throw new COMException(
                        "既にシンクを登録済みです。",
                        UnmanagedAPI.TSF.TextStore.TsResult.CONNECT_E_ADVISELIMIT
                    );
                }
                else
                {
                    // 各種値を保存
                    _services = (ITextStoreACPServices)i_unknown;
                    _sink = (ITextStoreACPSink)i_unknown;
                    _adviseFlags = i_mask;
                }
            }
        }

        public void UnadviseSink(object i_unknown)
        {
#if TSF_DEBUG_OUTPUT
            using(var dbgout = new DebugOut("{0}()", DebugOut.GetCaller()) )
#endif
            {
                if (_sink == null || _sink != i_unknown)
                {
                    throw new COMException(
                        "シンクは登録されていません。",
                        UnmanagedAPI.TSF.TextStore.TsResult.CONNECT_E_NOCONNECTION
                    );
                }

                _services = null;
                _sink = null;
                _adviseFlags = 0;
            }
        }

        public void RequestLock(LockFlags i_lockFlags, out int o_sessionResult)
        {
#if TSF_DEBUG_OUTPUT
            using(var dbgout = new DebugOut("{0}()", DebugOut.GetCaller()) )
#endif
            {
                o_sessionResult = UnmanagedAPI.WinError.HRESULT.E_FAIL;

                if (_sink == null)
                {
                    throw new COMException(
                        "シンクが登録されていません。",
                        UnmanagedAPI.TSF.TextStore.TsResult.CONNECT_E_NOCONNECTION
                    );
                }

                if (_lockFlags != 0)   // すでにロックされた状態の場合。
                {
                    if ((i_lockFlags & LockFlags.TS_LF_SYNC) != 0)
                    {
                        o_sessionResult = TsResult.TS_E_SYNCHRONOUS;
                        return;
                    }
                    else
                        if ((_lockFlags & LockFlags.TS_LF_READWRITE) == LockFlags.TS_LF_READ
                        && (i_lockFlags & LockFlags.TS_LF_READWRITE) == LockFlags.TS_LF_READWRITE)
                        {
                            _pendingLockUpgrade = true;
                            o_sessionResult = TsResult.TS_S_ASYNC;
                            return;
                        }

                    throw new COMException();
                }

                using (var unlocker = LockDocument(i_lockFlags))
                {
                    // ロックに失敗した場合は TS_E_SYNCHRONOUS をセットして S_OK を返す。
                    if (unlocker == null)
                    {
                        o_sessionResult = TsResult.TS_E_SYNCHRONOUS;
                    }
                    // ロックに成功した場合は OnLockGranted() を呼び出す。
                    else
                    {
                        try
                        {
                            o_sessionResult = _sink.OnLockGranted(i_lockFlags);
                        }
                        catch (COMException comException)
                        {
                            Debug.WriteLine("OnLockGranted() 呼び出し中に例外が発生。");
                            Debug.WriteLine("  " + comException.Message);
                            o_sessionResult = comException.HResult;
                        }
                        catch (Exception exception)
                        {
                            Debug.WriteLine("OnLockGranted() 呼び出し中に例外が発生。");
                            Debug.WriteLine("  " + exception.Message);
                        }
                    }
                }
            }
        }

        public void GetStatus(out TS_STATUS o_documentStatus)
        {
#if TSF_DEBUG_OUTPUT
            using(var dbgout = new DebugOut("{0}()", DebugOut.GetCaller()) )
#endif
            {
                if (IsReadOnly != null && IsReadOnly())
                    o_documentStatus.dynamicFlags = DynamicStatusFlags.TS_SD_READONLY;
                if (IsLoading != null && IsLoading())
                    o_documentStatus.dynamicFlags = DynamicStatusFlags.TS_SD_LOADING;
                else
                    o_documentStatus.dynamicFlags = 0;
                o_documentStatus.staticFlags = StaticStatusFlags.TS_SS_REGIONS;
                if (this._allow_multi_sel)
                    o_documentStatus.staticFlags |= StaticStatusFlags.TS_SS_DISJOINTSEL;
            }
        }

        public void QueryInsert(
            int i_startIndex,
            int i_endIndex,
            int i_length,
            out int o_startIndex,
            out int o_endIndex
        )
        {
#if TSF_DEBUG_OUTPUT
            using(var dbgout = new DebugOut("{0}()", DebugOut.GetCaller()) )
#endif
            {
                if (GetStringLength == null)
                    throw new NotImplementedException();

                int documentLength = GetStringLength();

                if (i_startIndex < 0
                || i_startIndex > i_endIndex
                || i_endIndex > documentLength)
                {
                    throw new COMException(
                        "インデックスが無効です。",
                        UnmanagedAPI.WinError.HRESULT.E_INVALIDARG
                    );
                }
                o_startIndex = i_startIndex;
                o_endIndex = i_endIndex;

#if TSF_DEBUG_OUTPUT
                DebugOut.Print("o_startIndex:{0} o_endIndex:{1}", i_startIndex, i_endIndex);
#endif
            }
        }

        public void GetSelection(
            int i_index,
            int i_selectionBufferLength,
            TS_SELECTION_ACP[] o_selections,
            out int o_fetchedLength
        )
        {
#if TSF_DEBUG_OUTPUT
            using(var dbgout = new DebugOut("{0}()", DebugOut.GetCaller()) )
#endif
            {
                if (GetSelectionIndex == null)
                    throw new NotImplementedException();

                o_fetchedLength = 0;

                if (IsLocked(LockFlags.TS_LF_READ) == false)
                {
                    throw new COMException(
                        "読取用ロックがされていません。",
                        TsResult.TS_E_NOLOCK
                    );
                }

                // -1 は TF_DEFAULT_SELECTION。選択は1つだけしかサポートしないので、
                // TF_DEFAULT_SELECTION でもなく、0 を超える数値が指定された場合はエラー。
                if (i_index != -1 && i_index > 0 && !this._allow_multi_sel)
                {
                    throw new COMException(
                        "選択は1つだけしかサポートしていません。",
                        UnmanagedAPI.WinError.HRESULT.E_INVALIDARG
                    );
                }

                if (i_selectionBufferLength > 0)
                {
                    TextSelection[] sels;
                    GetSelectionIndex(i_index,i_selectionBufferLength,out sels);
                    if (sels == null)
                        throw new InvalidOperationException("selsはnull以外の値を返す必要があります");
                    for(int i = 0; i < sels.Length; i++)
                    {
                        int start = sels[i].start, end = sels[i].end;

                        if (start <= end)
                        {
                            o_selections[0].start = start;
                            o_selections[0].end = end;
                            o_selections[0].style.ase = TsActiveSelEnd.TS_AE_END;
                            o_selections[0].style.interimChar = false;
                        }
                        else
                        {
                            o_selections[0].start = end;
                            o_selections[0].end = start;
                            o_selections[0].style.ase = TsActiveSelEnd.TS_AE_START;
                            o_selections[0].style.interimChar = false;
                        }
                    }

                    o_fetchedLength = sels.Length;

#if TSF_DEBUG_OUTPUT
                    DebugOut.Print("sel start:{0} end:{1}", o_selections[0].start, o_selections[0].end);
#endif
                }
            }
        }

        public void SetSelection(int i_count, TS_SELECTION_ACP[] i_selections)
        {
#if TSF_DEBUG_OUTPUT
            using(var dbgout = new DebugOut("{0}({1}, {2})",
                                            DebugOut.GetCaller(),
                                            i_selections[0].start,
                                            i_selections[0].end))
#endif
            {
                if (SetSelectionIndex == null)
                    throw new NotImplementedException();

                if (i_count != 1 && !this._allow_multi_sel)
                {
                    throw new COMException(
                        "選択は1つだけしかサポートしていません。",
                        UnmanagedAPI.WinError.HRESULT.E_INVALIDARG
                    );
                }

                if (IsLocked(LockFlags.TS_LF_READWRITE) == false)
                {
                    throw new COMException(
                        "ロックされていません。",
                        TsResult.TS_E_NOLOCK
                    );
                }

                TextSelection[] sels = new TextSelection[i_count];
                for(int i = 0; i < i_count; i++)
                {
                    sels[i] = new TextSelection(i_selections[i].start, i_selections[i].end);
                }

                SetSelectionIndex(sels);

#if TSF_DEBUG_OUTPUT
                DebugOut.Print("set selection startIndex:{0} endIndex:{1}", i_selections[0].start, i_selections[0].end);
#endif
            }
        }

        public void GetText(
            int i_startIndex,
            int i_endIndex,
            char[] o_plainText,
            int i_plainTextLength,
            out int o_plainTextLength,
            TS_RUNINFO[] o_runInfos,
            int i_runInfoLength,
            out int o_runInfoLength,
            out int o_nextUnreadCharPos
        )
        {
#if TSF_DEBUG_OUTPUT
            using(var dbgout = new DebugOut("{0}()", DebugOut.GetCaller()) )
#endif
            {
                if (GetStringLength == null || GetString == null)
                    throw new NotImplementedException();

                if (IsLocked(LockFlags.TS_LF_READ) == false)
                {
                    throw new COMException(
                        "読取用ロックがされていません。",
                        TsResult.TS_E_NOLOCK
                    );
                }

                if ((i_endIndex != -1 && i_startIndex > i_endIndex)
                || i_startIndex < 0 || i_startIndex > GetStringLength()
                || i_endIndex > GetStringLength())
                {
                    throw new COMException(
                        "インデックスが無効です。",
                        UnmanagedAPI.WinError.HRESULT.E_INVALIDARG
                    );
                }

                var textLength = 0;
                var copyLength = 0;

                if (i_endIndex == -1)
                    textLength = GetStringLength() - i_startIndex;
                else
                    textLength = i_endIndex - i_startIndex;
                copyLength = Math.Min(i_plainTextLength, textLength);

                // 文字列を格納。
                var text = GetString(i_startIndex, copyLength);
#if TSF_DEBUG_OUTPUT
                DebugOut.Print("got text:{0} from {1} length {2}", text,i_startIndex,copyLength);
#endif
                for (int i = 0; i < copyLength; i++)
                {
                    o_plainText[i] = text[i];
                }

                // 文字数を格納。
                o_plainTextLength = copyLength;
                // RUNINFO を格納。
                if (i_runInfoLength > 0)
                {
                    o_runInfos[0].type = TsRunType.TS_RT_PLAIN;
                    o_runInfos[0].length = copyLength;
                }
                o_runInfoLength = 1;

                // 次の文字の位置を格納。
                o_nextUnreadCharPos = i_startIndex + copyLength;
            }
        }

        public void SetText(
            SetTextFlags i_flags,
            int i_startIndex,
            int i_endIndex,
            char[] i_text,
            int i_length,
            out TS_TEXTCHANGE o_textChange
        )
        {
#if TSF_DEBUG_OUTPUT
            using(var dbgout = new DebugOut("{0}({1}, {2})",
                                            DebugOut.GetCaller(),
                                            i_startIndex, i_endIndex) )
#endif
            {
                var selections = new TS_SELECTION_ACP[]
                {
                    new TS_SELECTION_ACP
                    {
                        start = i_startIndex,
                        end   = i_endIndex,
                        style = new TS_SELECTIONSTYLE
                        {
                            ase = TsActiveSelEnd.TS_AE_END,
                            interimChar = false
                        }
                    }
                };

                int startIndex = 0, endIndex = 0;
                SetSelection(1, selections);
                InsertTextAtSelection(
                    InsertAtSelectionFlags.TF_IAS_NOQUERY,
                    i_text,
                    i_length,
                    out startIndex,
                    out endIndex,
                    out o_textChange
                );
            }
        }

        public void GetFormattedText(int i_start, int i_end, out object o_obj)
        {
#if TSF_DEBUG_OUTPUT
            using(var dbgout = new DebugOut("{0}()", DebugOut.GetCaller()) )
#endif
            {
                throw new NotImplementedException();
            }
        }

        public void GetEmbedded(
            int i_position,
            ref Guid i_guidService,
            ref Guid i_riid,
            out object o_obj
        )
        {
#if TSF_DEBUG_OUTPUT
            using(var dbgout = new DebugOut("{0}()", DebugOut.GetCaller()) )
#endif
            {
                throw new NotImplementedException();
            }
        }

        public void QueryInsertEmbedded(
            ref Guid i_guidService,
            int i_formatEtc,
            out bool o_insertable
        )
        {
#if TSF_DEBUG_OUTPUT
            using(var dbgout = new DebugOut("{0}()", DebugOut.GetCaller()) )
#endif
            {
                throw new NotImplementedException();
            }
        }

        public void InsertEmbedded(
            InsertEmbeddedFlags i_flags,
            int i_start,
            int i_end,
            object i_obj,
            out TS_TEXTCHANGE o_textChange
        )
        {
#if TSF_DEBUG_OUTPUT
            using(var dbgout = new DebugOut("{0}()", DebugOut.GetCaller()) )
#endif
            {
                throw new NotImplementedException();
            }
        }

        public void InsertTextAtSelection(
            InsertAtSelectionFlags i_flags,
            char[] i_text,
            int i_length,
            out int o_startIndex,
            out int o_endIndex,
            out TS_TEXTCHANGE o_textChange
        )
        {
#if TSF_DEBUG_OUTPUT
            using(var dbgout = new DebugOut("{0}()", DebugOut.GetCaller()) )
#endif
            {
                if (GetSelectionIndex == null || InsertAtSelection == null)
                    throw new NotImplementedException();

                int sel_count = 1;
                if(this.GetSelectionCount != null)
                    sel_count = this.GetSelectionCount();

                if (sel_count == 0)
                    throw new InvalidOperationException("sel_countは1以上の値でなければなりません");

                //エラーになるので適当な値で埋めておく
                o_startIndex = 0;
                o_endIndex = 0;
                o_textChange.start = 0;
                o_textChange.oldEnd = 0;
                o_textChange.newEnd = 0;

                TextSelection[] sels;
                GetSelectionIndex(0, sel_count, out sels);

                for(int i = 0; i < sel_count; i++)
                {
                    // 問い合わせのみで実際には操作を行わない
                    if ((i_flags & InsertAtSelectionFlags.TF_IAS_QUERYONLY) != 0)
                    {
                        o_startIndex = Math.Min(sels[i].start, sels[i].end);
                        o_endIndex = Math.Max(sels[i].start, sels[i].end);//o_startIndex + i_length;

                        o_textChange.start = o_startIndex;
                        o_textChange.oldEnd = o_endIndex;
                        o_textChange.newEnd = o_startIndex + i_length;
                    }
                    else
                    {
                        var start = Math.Min(sels[i].start, sels[i].end);
                        var end = Math.Max(sels[i].start, sels[i].end);

#if TSF_DEBUG_OUTPUT
                    DebugOut.Print("start: {0}, end: {1}, text: {2}", start, end, new string(i_text));
#endif

                        o_startIndex = start;
                        o_endIndex = start + i_length;

                        InsertAtSelection(new string(i_text), ref o_startIndex, ref o_endIndex);

                        o_textChange.start = start;
                        o_textChange.oldEnd = end;
                        o_textChange.newEnd = o_endIndex;
                        // InsertAtSelection() 内でカーソル位置を更新しているため、ここでは不要。
                        // 改行した時に位置が狂う。
                        // SetSelectionIndex(start, start + i_length);
                    }
                }
            }
        }

        public void GetEndACP(out int o_length)
        {
#if TSF_DEBUG_OUTPUT
            using(var dbgout = new DebugOut("{0}()", DebugOut.GetCaller()) )
#endif
            {
                if (GetStringLength == null)
                    throw new NotImplementedException();

                if (IsLocked(LockFlags.TS_LF_READ) == false)
                {
                    throw new COMException(
                        "読取用ロックがされていません。",
                        TsResult.TS_E_NOLOCK
                    );
                }

                o_length = GetStringLength();
            }
        }

        public void GetActiveView(out int o_viewCookie)
        {
#if TSF_DEBUG_OUTPUT
            using(var dbgout = new DebugOut("{0}()", DebugOut.GetCaller()) )
#endif
            {
                o_viewCookie = 1;
            }
        }

        public void GetACPFromPoint(
            int i_viewCookie,
            ref POINT i_point,
            GetPositionFromPointFlags i_flags,
            out int o_index
        )
        {
#if TSF_DEBUG_OUTPUT
            using (var dbgout = new DebugOut("{0}()", DebugOut.GetCaller()))
#endif
            {
                throw new NotImplementedException();
            }
        }

        public void GetTextExt(
            int i_viewCookie,
            int i_startIndex,
            int i_endIndex,
            out RECT o_rect,
            out bool o_isClipped
        )
        {
#if TSF_DEBUG_OUTPUT
            using(var dbgout = new DebugOut("{0}()", DebugOut.GetCaller()) )
#endif
            {
                if (GetStringLength == null)
                    throw new NotImplementedException();

                // 読取用ロックの確認。
                if (IsLocked(LockFlags.TS_LF_READ) == false)
                {
                    throw new COMException(
                        "読取用ロックがされていません。",
                        TsResult.TS_E_NOLOCK
                    );
                }

                if ((i_endIndex != -1 && i_startIndex > i_endIndex)
                || i_startIndex < 0 || i_startIndex > GetStringLength()
                || i_endIndex > GetStringLength())
                {
                    throw new COMException(
                        "インデックスが無効です。",
                        UnmanagedAPI.WinError.HRESULT.E_INVALIDARG
                    );
                }

                if (i_endIndex == -1)
                    i_endIndex = GetStringLength();

                var pointTopLeft = new POINT();
                var pointBotttomRight = new POINT();
                GetStringExtent(i_startIndex, i_endIndex, out pointTopLeft, out pointBotttomRight);

                o_rect.left = (int)(pointTopLeft.x);
                o_rect.top = (int)(pointTopLeft.y);
                o_rect.bottom = (int)(pointBotttomRight.y);
                o_rect.right = (int)(pointBotttomRight.x);
                o_isClipped = false;
#if TSF_DEBUG_OUTPUT
                DebugOut.Print("rect left:{0} top:{1} bottom:{2} right:{3}", o_rect.left, o_rect.top, o_rect.bottom, o_rect.right);
#endif
            }
        }

        public void GetScreenExt(int i_viewCookie, out RECT o_rect)
        {
#if TSF_DEBUG_OUTPUT
            using(var dbgout = new DebugOut("{0}()", DebugOut.GetCaller()) )
#endif
            {
                if (GetScreenExtent == null)
                    throw new NotImplementedException();

                POINT pointTopLeft, pointBottomRight;

                GetScreenExtent(out pointTopLeft, out pointBottomRight);

                o_rect.left = (int)(pointTopLeft.x);
                o_rect.top = (int)(pointTopLeft.y);
                o_rect.bottom = (int)(pointBottomRight.y);
                o_rect.right = (int)(pointBottomRight.x);
#if TSF_DEBUG_OUTPUT
                DebugOut.Print("rect left:{0} top:{1} bottom:{2} right:{3}", o_rect.left, o_rect.top, o_rect.bottom, o_rect.right);
#endif
            }
        }

        public void RequestSupportedAttrs(
            AttributeFlags i_flags,
            int i_length,
            Guid[] i_filterAttributes
        )
        {
#if TSF_DEBUG_OUTPUT
            using(var dbgout = new DebugOut("{0}()", DebugOut.GetCaller()) )
#endif
            {
                for (int i = 0; i < i_length; i++)
                {
                    if (i_filterAttributes[i].Equals(_attributeInfo[0].attrID))
                    {
                        _attributeInfo[0].flags = AttributeInfoFlags.Requested;
                        if ((i_flags & AttributeFlags.TS_ATTR_FIND_WANT_VALUE) != 0)
                        {
                            _attributeInfo[0].flags |= AttributeInfoFlags.Default;
                        }
                        else
                        {
                            _attributeInfo[0].currentValue = _attributeInfo[0].defaultValue;
                        }
                    }
                }
            }
        }

        public void RequestAttrsAtPosition(
            int i_position,
            int i_length,
            Guid[] i_filterAttributes,
            AttributeFlags i_flags
        )
        {
#if TSF_DEBUG_OUTPUT
            using(var dbgout = new DebugOut("{0}()", DebugOut.GetCaller()) )
#endif
            {
                for (int i = 0; i < i_length; i++)
                {
                    if (i_filterAttributes[i].Equals(_attributeInfo[0].attrID))
                    {
                        _attributeInfo[0].flags = AttributeInfoFlags.Requested;
                        if ((i_flags & AttributeFlags.TS_ATTR_FIND_WANT_VALUE) != 0)
                        {
                            _attributeInfo[0].flags |= AttributeInfoFlags.Default;
                        }
                        else
                        {
                            _attributeInfo[0].currentValue = _attributeInfo[0].defaultValue;
                        }
                    }
                }
            }
        }

        public void RequestAttrsTransitioningAtPosition(
            int i_position,
            int i_length,
            Guid[] i_filterAttributes,
            AttributeFlags i_flags
        )
        {
#if TSF_DEBUG_OUTPUT
            using(var dbgout = new DebugOut("{0}()", DebugOut.GetCaller()))
#endif
            {
                // 何もしない。
            }
        }

        public void FindNextAttrTransition(
            int i_start,
            int i_halt,
            int i_length,
            Guid[] i_filterAttributes,
            AttributeFlags i_flags,
            out int o_nextIndex,
            out bool o_found,
            out int o_foundOffset
        )
        {
#if TSF_DEBUG_OUTPUT
            using(var dbgout = new DebugOut("{0}()", DebugOut.GetCaller()) )
#endif
            {
                throw new NotImplementedException();
            }
        }

        public void RetrieveRequestedAttrs(
            int i_length,
            TS_ATTRVAL[] o_attributeVals,
            out int o_fetchedLength
        )
        {
#if TSF_DEBUG_OUTPUT
            using(var dbgout = new DebugOut("{0}()", DebugOut.GetCaller()) )
#endif
            {
                o_fetchedLength = 0;
                for (int i = 0; i < _attributeInfo.Length && o_fetchedLength < i_length; i++)
                {
                    if ((_attributeInfo[i].flags & AttributeInfoFlags.Requested) != 0)
                    {
                        o_attributeVals[o_fetchedLength].overlappedId = 0;
                        o_attributeVals[o_fetchedLength].attributeId = _attributeInfo[i].attrID;
                        o_attributeVals[o_fetchedLength].val = _attributeInfo[i].currentValue;

                        if ((_attributeInfo[i].flags & AttributeInfoFlags.Default) != 0)
                            _attributeInfo[i].currentValue = _attributeInfo[i].defaultValue;

                        o_fetchedLength++;
                        _attributeInfo[i].flags = AttributeInfoFlags.None;
                    }
                }
            }
        }
        #endregion

        #region "ITfContextOwnerCompositionSink インターフェイスの実装"
        /// <summary>コンポジション入力が開始された時の処理。</summary>
        public void OnStartComposition(ITfCompositionView view, out bool ok)
        {
            #if TSF_DEBUG_OUTPUT
            using(var dbgout = new DebugOut("{0}()", DebugOut.GetCaller()) )
            #endif
            {
                if( CompositionStarted != null )
                    ok = CompositionStarted();
                else
                    ok = true;
            }
        }


        //========================================================================================


        /// <summary>コンポジションが変更された時の処理。</summary>
        public void OnUpdateComposition(ITfCompositionView view, ITfRange rangeNew)
        {
            #if TSF_DEBUG_OUTPUT
            using(var dbgout = new DebugOut("{0}()", DebugOut.GetCaller()) )
            #endif
            {
                if (rangeNew == null)
                    return;

                int start, count;
                
                ITfRangeACP rangeAcp = (ITfRangeACP)rangeNew;
                
                rangeAcp.GetExtent(out start, out count);
                
                if (this.CompositionUpdated != null)
                    this.CompositionUpdated(start, start + count);
            }
        }

        
        //========================================================================================


        /// <summary>コンポジション入力が終了した時の処理。</summary>
        public void OnEndComposition(ITfCompositionView view)
        {
            #if TSF_DEBUG_OUTPUT
            using(var dbgout = new DebugOut("{0}()", DebugOut.GetCaller()) )
            #endif
            {
                if( CompositionEnded != null )
                    CompositionEnded();
            }
        }
        #endregion "ITfContextOwnerCompositionSink インターフェイスの実装"

        #region 表示属性
        /// <summary>表示属性の取得</summary>
        public IEnumerable<TextDisplayAttribute> EnumAttributes(int start, int end)
        {
            ITfRangeACP allRange;
            _services.CreateRange(start, end, out allRange);

            foreach (TextDisplayAttribute attr in this.EnumAttributes((ITfRange)allRange))
                yield return attr;

            ReleaseComObject("allRange", ref allRange);
        }

        IEnumerable<TextDisplayAttribute> EnumAttributes(ITfRange range)
        {
#if TSF_DEBUG_OUTPUT
            using(var dbgout = new DebugOut("{0}()", DebugOut.GetCaller()))
#endif
            {
                ITfProperty property = null;    // プロパティインターフェイス
                IEnumTfRanges enumRanges;         // 範囲の列挙子
                Guid guidPropAttribute = TfDeclarations.GUID_PROP_ATTRIBUTE;

                if (_context == null || _services == null)
                    yield break;

                // GUID_PROP_ATTRIBUTE プロパティを取得。
                _context.GetProperty(ref guidPropAttribute, out property);
                if (property == null)
                    yield break;

                // 全範囲の中で表示属性プロパティをもつ範囲を列挙する。
                property.EnumRanges((int)_editCookie, out enumRanges, range);

                ITfRange[] ranges = new ITfRange[1];
                int fetchedLength = 0;
                while (HRESULT.Succeeded(enumRanges.Next(1, ranges, out fetchedLength))
                    && fetchedLength > 0)
                {
                    // ItfRange から ItfRangeACP を取得。
                    ITfRangeACP rangeACP = ranges[0] as ITfRangeACP;
                    if (rangeACP == null)
                        continue;   // 普通はあり得ない

                    // 範囲の開始位置と文字数を取得。
                    int start, count;
                    rangeACP.GetExtent(out start, out count);

                    // VARIANT 値としてプロパティ値を取得。VT_I4 の GUID ATOM がとれる。
                    VARIANT value = new VARIANT();
                    property.GetValue((int)_editCookie, ranges[0], ref value);
                    if (value.vt == (short)VarEnum.VT_I4)
                    {
                        Guid guid, clsid;
                        ITfDisplayAttributeInfo info;
                        TF_DISPLAYATTRIBUTE attribute;

                        // GUID ATOM から GUID を取得する。
                        _categoryMgr.GetGUID((int)value.data1, out guid);
                        // その GUID から IDisplayAttributeInfo インターフェイスを取得。
                        _displayAttributeMgr.GetDisplayAttributeInfo(
                            ref guid,
                            out info,
                            out clsid
                        );
                        // さらに IDisplayAttributeInfo インターフェイスから表示属性を取得する。
                        info.GetAttributeInfo(out attribute);
                        ReleaseComObject("info", ref info);

                        yield return new TextDisplayAttribute
                        {
                            startIndex = start,
                            endIndex = start + count,
                            attribute = attribute
                        };

#if TSF_DEBUG_OUTPUT_DISPLAY_ATTR
                            Debug.WriteLine(
                                "*******:::: DisplayAttribute: {0} ~ {1} :::::: *********",
                                start, start + count
                            );
                            Debug.WriteLine(attribute.bAttr);
                            Debug.WriteLine(
                                "LineColorType: {0}, {1}",
                                attribute.crLine.type, attribute.crLine.indexOrColorRef
                            );
                            Debug.WriteLine(
                                "TextColorType: {0}, {1}",
                                attribute.crText.type, attribute.crText.indexOrColorRef
                            );
                            Debug.WriteLine(
                                "BackColorType: {0}, {1}",
                                attribute.crBk.type, attribute.crBk.indexOrColorRef
                            );
                            Debug.WriteLine(
                                "Bold, Style  : {0}, {1}",
                                attribute.fBoldLine, attribute.lsStyle
                            );
#endif
                    }

                    ReleaseComObject("rangeACP", ref rangeACP);
                }

                ReleaseComObject("ranges[0]", ref ranges[0]);
                ReleaseComObject("enumRanges", ref enumRanges);
                ReleaseComObject("property", ref property);
            }
        }
        #endregion

#if METRO
        protected ITfThreadMgr2 _threadMgr;
#else
        protected ITfThreadMgr _threadMgr;
#endif
        protected ITfDocumentMgr _documentMgr;
        protected ITfFunctionProvider _functionProvider;
        protected ITfFnReconversion _reconversion;
        protected ITfContext _context;
        protected ITfCategoryMgr _categoryMgr;
        protected ITfDisplayAttributeMgr _displayAttributeMgr;
        protected ITextStoreACPSink _sink;
        protected uint _editCookie = 0;
        protected ITextStoreACPServices _services;
        protected AdviseFlags               _adviseFlags = 0;
        protected LockFlags                 _lockFlags = 0;
        protected bool                      _pendingLockUpgrade = false;

        /// <summary>
        /// AttributeInfo　で使用されるフラグ。各属性の状態を示す。
        /// </summary>
        protected enum AttributeInfoFlags
        {
            /// <summary>何もない。</summary>
            None        = 0,
            /// <summary>デフォルト値の要求。</summary>
            Default     = 1 << 0,
            /// <summary>要求された。</summary>
            Requested   = 1 << 1
        }

        protected struct AttributeInfo
        {
            public Guid                 attrID;
            public AttributeInfoFlags   flags;
            public VARIANT              currentValue;
            public VARIANT              defaultValue;
        }

        protected AttributeInfo[]       _attributeInfo = new AttributeInfo[1];

    }
}
