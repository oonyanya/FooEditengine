using System;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security;
using DotNetTextStore.UnmanagedAPI.WinDef;


namespace DotNetTextStore.UnmanagedAPI.TSF
{
    public enum TfAnchor
    {
        TF_ANCHOR_START,
        TF_ANCHOR_END
    }

 
    public enum TfGravity
    {
        TF_GR_BACKWARD,
        TF_GR_FORWARD
    }


    public enum TfShiftDir
    {
        TF_SD_BACKWARD,
        TF_SD_FORWARD
    }


    [Flags]
    public enum PopFlags
    {
        TF_POPF_ALL = 1
    }


    public enum TfCandidateResult
    {
        CAND_FINALIZED,
        CAND_SELECTED,
        CAND_CANCELED
    }


//============================================================================


    [ComImport, Guid("AA80E808-2021-11D2-93E0-0060B067B86E"),
     InterfaceType(ComInterfaceType.InterfaceIsIUnknown)
    ]
    public interface IEnumTfDocumentMgrs
    {
        void Clone([Out, MarshalAs(UnmanagedType.Interface)] out IEnumTfDocumentMgrs ppEnum);
        void Next([In] uint ulCount, [Out, MarshalAs(UnmanagedType.Interface)] out ITfDocumentMgr rgDocumentMgr, [Out] out uint pcFetched);
        void Reset();
        void Skip([In] uint ulCount);
    }


//============================================================================


    /// <summary>
    /// カテゴリマネージャー。
    /// 
    /// <pre>
    ///   カテゴリマネージャーはテキストサービスのオブジェクトのカテゴリを管理
    ///   する。GUID アトムから GUID を取得する機能を提供する。
    /// </pre>
    /// </summary>
    [ComImport,
     Guid("c3acefb5-f69d-4905-938f-fcadcf4be830"),
     SecurityCritical,
     InterfaceType(ComInterfaceType.InterfaceIsIUnknown),
    ]
    public interface ITfCategoryMgr
    {
        [SecurityCritical]
        void stub_RegisterCategory();
        [SecurityCritical]
        void stub_UnregisterCategory();
        [SecurityCritical]
        void stub_EnumCategoriesInItem();
        [SecurityCritical]
        void stub_EnumItemsInCategory();
        [SecurityCritical]
        void stub_FindClosestCategory();
        [SecurityCritical]
        void stub_RegisterGUIDDescription();
        [SecurityCritical]
        void stub_UnregisterGUIDDescription();
        [SecurityCritical]
        void stub_GetGUIDDescription();
        [SecurityCritical]
        void stub_RegisterGUIDDWORD();
        [SecurityCritical]
        void stub_UnregisterGUIDDWORD();
        [SecurityCritical]
        void stub_GetGUIDDWORD();
        /// <summary>GUID を登録して GUID アトムを取得する。</summary>
        [PreserveSig, SecurityCritical]
        int RegisterGUID(
            [In] ref Guid   i_guid,
            [Out] out uint  o_guidAtom
        );

        /// <summary>
        /// GUID アトムから GUID を取得する。
        /// </summary>
        /// <param name="i_guidAtom">GUID アトム。</param>
        /// <param name="o_guid">GUID。</param>
        [SecurityCritical]
        void GetGUID(
            [In] int        i_guidAtom,
            [Out] out Guid  o_guid
        );

        [SecurityCritical]
        void stub_IsEqualTfGuidAtom();
    }


//============================================================================


    public enum TF_DA_COLORTYPE
    {
        TF_CT_NONE,
        TF_CT_SYSCOLOR,
        TF_CT_COLORREF
    }


    /// <summary>表示属性の線のスタイル</summary>
    public enum TF_DA_LINESTYLE
    {
        /// <summary>なし</summary>
        TF_LS_NONE,
        /// <summary>ソリッド線</summary>
        TF_LS_SOLID,
        /// <summary>ドット線</summary>
        TF_LS_DOT,
        /// <summary>ダッシュ線</summary>
        TF_LS_DASH,
        /// <summary>波線</summary>
        TF_LS_SQUIGGLE
    }


    public enum TF_DA_ATTR_INFO
    {
        TF_ATTR_CONVERTED = 2,
        TF_ATTR_FIXEDCONVERTED = 5,
        TF_ATTR_INPUT = 0,
        TF_ATTR_INPUT_ERROR = 4,
        TF_ATTR_OTHER = -1,
        TF_ATTR_TARGET_CONVERTED = 1,
        TF_ATTR_TARGET_NOTCONVERTED = 3
    }

 
    [StructLayout(LayoutKind.Sequential)]
    public struct TF_DA_COLOR
    {
        public TF_DA_COLORTYPE  type;
        public uint             indexOrColorRef;
    }


    [StructLayout(LayoutKind.Sequential)]
    public struct TF_DISPLAYATTRIBUTE
    {
        public TF_DA_COLOR      crText;
        public TF_DA_COLOR      crBk;
        public TF_DA_LINESTYLE  lsStyle;
        [MarshalAs(UnmanagedType.Bool)]
        public bool             fBoldLine;
        public TF_DA_COLOR      crLine;
        public TF_DA_ATTR_INFO  bAttr;
    }


//============================================================================


    [ComImport,
     Guid("70528852-2f26-4aea-8c96-215150578932"),
     InterfaceType(ComInterfaceType.InterfaceIsIUnknown),
     SecurityCritical
    ]
    public interface ITfDisplayAttributeInfo
    {
        [SecurityCritical]
        void stub_GetGUID();
        [SecurityCritical]
        void stub_GetDescription();
        [SecurityCritical]
        void GetAttributeInfo(out TF_DISPLAYATTRIBUTE attr);
        [SecurityCritical]
        void stub_SetAttributeInfo();
        [SecurityCritical]
        void stub_Reset();
    }


//============================================================================


    [ComImport,
     SecurityCritical,
     InterfaceType(ComInterfaceType.InterfaceIsIUnknown),
     Guid("8ded7393-5db1-475c-9e71-a39111b0ff67")
    ]
    public interface ITfDisplayAttributeMgr
    {
        [SecurityCritical]
        void OnUpdateInfo();

        [SecurityCritical]
        void stub_EnumDisplayAttributeInfo();

        [SecurityCritical]
        void GetDisplayAttributeInfo(
            [In] ref Guid                       i_guid,
            [Out] out ITfDisplayAttributeInfo   o_info,
            [Out] out Guid                      o_clsid
        );
    }

 
//============================================================================
#if !METRO
    [ComImport,
     InterfaceType(ComInterfaceType.InterfaceIsIUnknown),
     Guid("aa80e801-2021-11d2-93e0-0060b067b86e")
    ]
    public interface ITfThreadMgr
    {
        [SecurityCritical]
        void Activate(out int clientId);

        [SecurityCritical]
        void Deactivate();

        [SecurityCritical]
        void CreateDocumentMgr(out ITfDocumentMgr docMgr);
        void EnumDocumentMgrs(out IEnumTfDocumentMgrs enumDocMgrs);
        void GetFocus(out ITfDocumentMgr docMgr);
        [SecurityCritical]
        void SetFocus(ITfDocumentMgr docMgr);
        void AssociateFocus(IntPtr hwnd, ITfDocumentMgr newDocMgr, out ITfDocumentMgr prevDocMgr);
        void IsThreadFocus([MarshalAs(UnmanagedType.Bool)] out bool isFocus);
        [SecurityCritical]
        void GetFunctionProvider(ref Guid classId, out ITfFunctionProvider funcProvider);
        void EnumFunctionProviders(out IEnumTfFunctionProviders enumProviders);
        [SecurityCritical]
        void GetGlobalCompartment(out ITfCompartmentMgr compartmentMgr);
    }
#endif

    //============================================================================
    public enum TfNameFlags
    {
        NOACTIVATETIP = 0x01,
        SECUREMODE = 0x02,
        UIELEMENTENABLEDONLY = 0x04,
        COMLESS = 0x08,
        WOW16 = 0x10,
        NOACTIVATEKEYBOARDLAYOUT = 0x20,
        CONSOLE = 0x40
    }

    [ComImport,
     InterfaceType(ComInterfaceType.InterfaceIsIUnknown),
     Guid("0ab198ef-6477-4ee8-8812-6780edb82d5e")
    ]
    public interface ITfThreadMgr2
    {
        /// <summary>
        /// ITfThreadMgr2::Activate
        /// </summary>
        /// <param name="clientId"></param>
        [SecurityCritical]
        void Activate(out int clientId);

        /// <summary>
        /// ITfThreadMgr2::Deactivate
        /// </summary>
        [SecurityCritical]
        void Deactivate();

        /// <summary>
        /// ITfThreadMgr2::CreateDocumentMgr
        /// </summary>
        /// <param name="docMgr"></param>
        [SecurityCritical]
        void CreateDocumentMgr(out ITfDocumentMgr docMgr);

        /// <summary>
        /// ITfThreadMgr2::EnumDocumentMgrs
        /// </summary>
        /// <param name="enumDocMgrs"></param>
        void EnumDocumentMgrs(out IEnumTfDocumentMgrs enumDocMgrs);

        /// <summary>
        /// ITfThreadMgr2::GetFocus
        /// </summary>
        /// <param name="docMgr"></param>
        void GetFocus(out ITfDocumentMgr docMgr);

        /// <summary>
        /// ITfThreadMgr2::SetFocus
        /// </summary>
        /// <param name="docMgr"></param>
        [SecurityCritical]
        void SetFocus(ITfDocumentMgr docMgr);

        /// <summary>
        /// ITfThreadMgr2::IsThreadFocus
        /// </summary>
        /// <param name="isFocus"></param>
        void IsThreadFocus([MarshalAs(UnmanagedType.Bool)] out bool isFocus);

        /// <summary>
        /// ITfThreadMgr2::GetFunctionProvider
        /// </summary>
        /// <param name="classId"></param>
        /// <param name="funcProvider"></param>
        [SecurityCritical]
        void GetFunctionProvider(ref Guid classId, out ITfFunctionProvider funcProvider);

        /// <summary>
        /// ITfThreadMgr2::EnumFunctionProviders
        /// </summary>
        /// <param name="enumProviders"></param>
        void EnumFunctionProviders(out IEnumTfFunctionProviders enumProviders);

        /// <summary>
        /// ITfThreadMgr2::GetGlobalCompartment
        /// </summary>
        /// <param name="compartmentMgr"></param>
        [SecurityCritical]
        void GetGlobalCompartment(out ITfCompartmentMgr compartmentMgr);

        /// <summary>
        /// ITfThreadMgr2::ActivateEx
        /// </summary>
        /// <param name="clientId"></param>
        /// <param name="?"></param>
        [SecurityCritical]
        void ActivateEx(out int clientId,[In] TfNameFlags flags);

        /// <summary>
        /// ITfThreadMgr2::GetActiveFlags
        /// </summary>
        /// <param name="flags"></param>
        void GetActiveFlags(out TfNameFlags flags);

        /// <summary>
        /// ITfThreadMgr2::SuspendKeystrokeHandling
        /// </summary>
        void SuspendKeystrokeHandling();

        /// <summary>
        /// ITfThreadMgr2::ResumeKeystrokeHandling
        /// </summary>
        void ResumeKeystrokeHandling();
    }

//============================================================================


    [ComImport, 
     Guid("7dcf57ac-18ad-438b-824d-979bffb74b7c"), 
     InterfaceType(ComInterfaceType.InterfaceIsIUnknown)
    ]
    public interface ITfCompartmentMgr
    {
        [SecurityCritical,]
        void GetCompartment(ref Guid guid, out ITfCompartment comp);
        void ClearCompartment(int tid, Guid guid);
        void EnumCompartments(out object enumGuid);
    }


//============================================================================


    [ComImport,
     Guid("bb08f7a9-607a-4384-8623-056892b64371"),
     SecurityCritical,
     InterfaceType(ComInterfaceType.InterfaceIsIUnknown)
    ]
    public interface ITfCompartment
    {
        [PreserveSig, SecurityCritical]
        int SetValue(int tid, ref object varValue);
        [SecurityCritical]
        void GetValue(out object varValue);
    }


//============================================================================


    [ComImport,
     InterfaceType(ComInterfaceType.InterfaceIsIUnknown),
     Guid("E4B24DB0-0990-11D3-8DF0-00105A2799B5")
    ]
    public interface IEnumTfFunctionProviders
    {
        void Clone([Out, MarshalAs(UnmanagedType.Interface)] out IEnumTfFunctionProviders ppEnum);
        void Next([In] uint ulCount, [Out, MarshalAs(UnmanagedType.Interface)] out ITfFunctionProvider ppCmdobj, [Out] out uint pcFetch);
        void Reset();
        void Skip([In] uint ulCount);
    }

    
//============================================================================


    [ComImport,
     InterfaceType(ComInterfaceType.InterfaceIsIUnknown),
     Guid("101d6610-0990-11d3-8df0-00105a2799b5")
    ]
    public interface ITfFunctionProvider
    {
        void GetType(out Guid guid);
        void GetDescription([MarshalAs(UnmanagedType.BStr)] out string desc);
        [SecurityCritical, ]
        void GetFunction(ref Guid guid, ref Guid riid, [MarshalAs(UnmanagedType.Interface)] out object obj);
    }

 
//============================================================================


    [ComImport,
     Guid("aa80e7f4-2021-11d2-93e0-0060b067b86e"),
     InterfaceType(ComInterfaceType.InterfaceIsIUnknown)
    ]
    public interface ITfDocumentMgr
    {
        [SecurityCritical]
        void CreateContext(int clientId, uint flags, [MarshalAs(UnmanagedType.Interface)] object obj, out ITfContext context, out uint editCookie);
        [SecurityCritical, ]
        void Push(ITfContext context);
        [SecurityCritical]
        void Pop(PopFlags flags);
        void GetTop(out ITfContext context);
        [SecurityCritical, ]
        void GetBase(out ITfContext context);
        void EnumContexts([MarshalAs(UnmanagedType.Interface)] out object enumContexts);
    }


//============================================================================


    [StructLayout(LayoutKind.Sequential, Pack=4),
     Guid("75EB22F2-B0BF-46A8-8006-975A3B6EFCF1")
    ]
    public struct TF_SELECTION
    {
        [MarshalAs(UnmanagedType.Interface)]
        public ITfRange range;
        public TF_SELECTIONSTYLE style;
    }


//============================================================================


    /// <summary>
    /// 選択範囲の終了位置を示すフラグ。
    /// </summary>
    public enum TfActiveSelEnd
    {
        /// <summary>
        /// アクティブな選択は無い。
        /// </summary>
        TF_AE_NONE,
        /// <summary>
        /// 開始位置が選択範囲の終了位置である。
        /// </summary>
        TF_AE_START,
        /// <summary>
        /// 終了位置と選択範囲の終了位置は同じである。
        /// </summary>
        TF_AE_END
    }


    /// <summary>
    /// TF_SELECTION のメンバとして使用される構造体。
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct TF_SELECTIONSTYLE
    {
        /// <summary>
        /// 選択範囲の開始位置が終了位置なのか開始位置なのか示す
        /// </summary>
        public TfActiveSelEnd ase;
        /// <summary>
        /// 用途不明。説明を見ると変換中の文字のことのようだが変換中の文字に対
        /// しても true が渡されたことは無い・・・
        /// </summary>
        [MarshalAs(UnmanagedType.Bool)]
        public bool interimChar;
    }


//============================================================================


    [ComImport,
     Guid("aa80e7fd-2021-11d2-93e0-0060b067b86e"),
     InterfaceType(ComInterfaceType.InterfaceIsIUnknown)
    ]
    public interface ITfContext
    {
        int stub_RequestEditSession();
        void InWriteSession(int clientId, [MarshalAs(UnmanagedType.Bool)] out bool inWriteSession);
        void GetSelection(
            [In]　uint ec,
            [In] uint ulIndex,
            [In] uint ulCount,
            [Out, MarshalAs(UnmanagedType.LPArray, SizeParamIndex=3)] TF_SELECTION[] pSelection,
            [Out] out uint pcFetched
        );
        void stub_SetSelection();
        [SecurityCritical]
        void GetStart(int ec, out ITfRange range);
        void stub_GetEnd();
        void stub_GetActiveView();
        void stub_EnumViews();
        void stub_GetStatus();
        [SecurityCritical]
        void GetProperty(ref Guid guid, out ITfProperty property);
        void stub_GetAppProperty();

        void TrackProperties(
            [In] ref Guid[] guids,
            [In] uint propertyGuidLength,
            [In] IntPtr prgAppProp,
            [In] uint cAppProp,
            [Out, MarshalAs(UnmanagedType.Interface)] out ITfReadOnlyProperty ppProperty
        );        

        void EnumProperties([Out, MarshalAs(UnmanagedType.Interface)] out IEnumTfProperties ppEnum);
        void stub_GetDocumentMgr();
        void stub_CreateRangeBackup();
    }


//============================================================================

    
    [ComImport, Guid("19188CB0-ACA9-11D2-AFC5-00105A2799B5"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IEnumTfProperties
    {
        void Clone([Out, MarshalAs(UnmanagedType.Interface)] out IEnumTfProperties ppEnum);

        [PreserveSig, SecurityCritical]
        int Next(
            [In] uint                                                               i_count,
            [Out, MarshalAs(UnmanagedType.LPArray, SizeParamIndex=2)] ITfProperty[] o_properties,
            [Out] out int                                                           o_fetchedLength
        );

        void Reset();

        void Skip([In] uint ulCount);
    }


//============================================================================


    [ComImport,
     InterfaceType(ComInterfaceType.InterfaceIsIUnknown),
     SecurityCritical,
     Guid("aa80e7ff-2021-11d2-93e0-0060b067b86e")
    ]
    public interface ITfRange
    {
        [SecurityCritical]
        void GetText(int ec, int flags, [Out, MarshalAs(UnmanagedType.LPArray, SizeParamIndex=3)] char[] text, int countMax, out int count);
        [SecurityCritical]
        void SetText(int ec, int flags, [In, MarshalAs(UnmanagedType.LPArray, SizeParamIndex=3)] char[] text, int count);
        [SecurityCritical]
        void GetFormattedText(int ec, [MarshalAs(UnmanagedType.Interface)] out object data);
        [SecurityCritical]
        void GetEmbedded(int ec, ref Guid guidService, ref Guid iid, [MarshalAs(UnmanagedType.Interface)] out object obj);
        [SecurityCritical]
        void InsertEmbedded(int ec, int flags, [MarshalAs(UnmanagedType.Interface)] object data);
        [SecurityCritical]
        void ShiftStart(int ec, int count, out int result, int ZeroForNow);
        [SecurityCritical]
        void ShiftEnd(int ec, int count, out int result, int ZeroForNow);
        [SecurityCritical]
        void ShiftStartToRange(int ec, ITfRange range, TfAnchor position);
        [SecurityCritical]
        void ShiftEndToRange(int ec, ITfRange range, TfAnchor position);
        [SecurityCritical]
        void ShiftStartRegion(int ec, TfShiftDir dir, [MarshalAs(UnmanagedType.Bool)] out bool noRegion);
        [SecurityCritical]
        void ShiftEndRegion(int ec, TfShiftDir dir, [MarshalAs(UnmanagedType.Bool)] out bool noRegion);
        [SecurityCritical]
        void IsEmpty(int ec, [MarshalAs(UnmanagedType.Bool)] out bool empty);
        [SecurityCritical]
        void Collapse(int ec, TfAnchor position);
        [SecurityCritical]
        void IsEqualStart(int ec, ITfRange with, TfAnchor position, [MarshalAs(UnmanagedType.Bool)] out bool equal);
        [SecurityCritical]
        void IsEqualEnd(int ec, ITfRange with, TfAnchor position, [MarshalAs(UnmanagedType.Bool)] out bool equal);
        [SecurityCritical]
        void CompareStart(int ec, ITfRange with, TfAnchor position, out int result);
        [SecurityCritical]
        void CompareEnd(int ec, ITfRange with, TfAnchor position, out int result);
        [SecurityCritical]
        void AdjustForInsert(int ec, int count, [MarshalAs(UnmanagedType.Bool)] out bool insertOk);
        [SecurityCritical]
        void GetGravity(out TfGravity start, out TfGravity end);
        [SecurityCritical]
        void SetGravity(int ec, TfGravity start, TfGravity end);
        [SecurityCritical]
        void Clone(out ITfRange clone);
        [SecurityCritical]
        void GetContext(out ITfContext context);
    }


//============================================================================


    [ComImport,
     SecurityCritical,
     InterfaceType(ComInterfaceType.InterfaceIsIUnknown),
     Guid("057a6296-029b-4154-b79a-0d461d4ea94c")
    ]
    public interface ITfRangeACP
    {
        [SecurityCritical]
        void GetText(int ec, int flags, [Out, MarshalAs(UnmanagedType.LPArray, SizeParamIndex=3)] char[] text, int countMax, out int count);
        [SecurityCritical]
        void SetText(int ec, int flags, [In, MarshalAs(UnmanagedType.LPArray, SizeParamIndex=3)] char[] text, int count);
        [SecurityCritical]
        void GetFormattedText(int ec, [MarshalAs(UnmanagedType.Interface)] out object data);
        [SecurityCritical]
        void GetEmbedded(int ec, ref Guid guidService, ref Guid iid, [MarshalAs(UnmanagedType.Interface)] out object obj);
        [SecurityCritical]
        void InsertEmbedded(int ec, int flags, [MarshalAs(UnmanagedType.Interface)] object data);
        [SecurityCritical]
        void ShiftStart(int ec, int count, out int result, int ZeroForNow);
        [SecurityCritical]
        void ShiftEnd(int ec, int count, out int result, int ZeroForNow);
        [SecurityCritical]
        void ShiftStartToRange(int ec, ITfRange range, TfAnchor position);
        [SecurityCritical]
        void ShiftEndToRange(int ec, ITfRange range, TfAnchor position);
        [SecurityCritical]
        void ShiftStartRegion(int ec, TfShiftDir dir, [MarshalAs(UnmanagedType.Bool)] out bool noRegion);
        [SecurityCritical]
        void ShiftEndRegion(int ec, TfShiftDir dir, [MarshalAs(UnmanagedType.Bool)] out bool noRegion);
        [SecurityCritical]
        void IsEmpty(int ec, [MarshalAs(UnmanagedType.Bool)] out bool empty);
        [SecurityCritical]
        void Collapse(int ec, TfAnchor position);
        [SecurityCritical]
        void IsEqualStart(int ec, ITfRange with, TfAnchor position, [MarshalAs(UnmanagedType.Bool)] out bool equal);
        [SecurityCritical]
        void IsEqualEnd(int ec, ITfRange with, TfAnchor position, [MarshalAs(UnmanagedType.Bool)] out bool equal);
        [SecurityCritical]
        void CompareStart(int ec, ITfRange with, TfAnchor position, out int result);
        [SecurityCritical]
        void CompareEnd(int ec, ITfRange with, TfAnchor position, out int result);
        [SecurityCritical]
        void AdjustForInsert(int ec, int count, [MarshalAs(UnmanagedType.Bool)] out bool insertOk);
        [SecurityCritical]
        void GetGravity(out TfGravity start, out TfGravity end);
        [SecurityCritical]
        void SetGravity(int ec, TfGravity start, TfGravity end);
        [SecurityCritical]
        void Clone(out ITfRange clone);
        [SecurityCritical]
        void GetContext(out ITfContext context);
        [SecurityCritical]
        void GetExtent(out int start, out int count);
        [SecurityCritical]
        void SetExtent(int start, int count);
    }


//============================================================================


    [ComImport,
     InterfaceType(ComInterfaceType.InterfaceIsIUnknown),
     Guid("17D49A3D-F8B8-4B2F-B254-52319DD64C53"),
     SecurityCritical, 
     
    ]
    public interface ITfReadOnlyProperty
    {
        void GetType(
            [Out] out Guid o_guid
        );

        void EnumRanges(
            [In] int                                                    i_editCookie,
            [Out, MarshalAs(UnmanagedType.Interface)] out IEnumTfRanges o_enumRanges,
            [In, MarshalAs(UnmanagedType.Interface)] ITfRange           i_targetRange
        );

        void GetValue(
            [In] int                                                    i_editCookie,
            [In, MarshalAs(UnmanagedType.Interface)] ITfRange           i_range,
            [In, Out, MarshalAs(UnmanagedType.Struct)] ref VARIANT           o_varValue
        );

        void GetContext(
            [Out, MarshalAs(UnmanagedType.Interface)] out ITfContext    o_ppContext
        );
    }

 
//============================================================================


    //[ComImport,
    // Guid("e2449660-9542-11d2-bf46-00105a2799b5"),
    // InterfaceType(ComInterfaceType.InterfaceIsIUnknown), 
    // SecurityCritical, 
    // 
    //]
    //public interface ITfProperty : ITfReadOnlyProperty
    //{
    //    void FindRange(int editCookie, ITfRange inRange, out ITfRange outRange, TfAnchor position);
    //    void stub_SetValueStore();
    //    void SetValue(int editCookie, ITfRange range, object value);
    //    void Clear(int editCookie, ITfRange range);
    //}


    [ComImport, Guid("e2449660-9542-11d2-bf46-00105a2799b5"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown), SecurityCritical]
    public interface ITfProperty
    {
        void GetType(out Guid type);
        [PreserveSig]
        int EnumRanges(int editcookie, out IEnumTfRanges ranges, ITfRange targetRange);
        void GetValue(int editCookie, ITfRange range, ref VARIANT value);
        void GetContext(out ITfContext context);
        void FindRange(int editCookie, ITfRange inRange, out ITfRange outRange, TfAnchor position);
        void stub_SetValueStore();
        void SetValue(int editCookie, ITfRange range, object value);
        void Clear(int editCookie, ITfRange range);
    }

 
//============================================================================


    [StructLayout(LayoutKind.Sequential, Pack = 8), Guid("D678C645-EB6A-45C9-B4EE-0F3E3A991348")]
    public struct TF_PROPERTYVAL
    {
        public Guid guidId;
        [MarshalAs(UnmanagedType.Struct)]
        public VARIANT varValue;
    }

    
//============================================================================

   
    [ComImport, Guid("8ED8981B-7C10-4D7D-9FB3-AB72E9C75F72"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IEnumTfPropertyValue
    {
        void Clone([Out, MarshalAs(UnmanagedType.Interface)] out IEnumTfPropertyValue ppEnum);
        [PreserveSig, SecurityCritical]
        int Next([In] uint ulCount, [Out, MarshalAs(UnmanagedType.LPArray, SizeParamIndex=2)] TF_PROPERTYVAL[] values, [Out] out int fetched);
        void Reset();
        void Skip([In] uint ulCount);
    }

    
//============================================================================

    
    [ComImport, 
     Guid("f99d3f40-8e32-11d2-bf46-00105a2799b5"), 
     InterfaceType(ComInterfaceType.InterfaceIsIUnknown), 
     SecurityCritical
    ]
    public interface IEnumTfRanges
    {
        [SecurityCritical]
        void Clone(out IEnumTfRanges ranges);
        [PreserveSig, SecurityCritical]
        int Next(int count, [Out, MarshalAs(UnmanagedType.LPArray, SizeParamIndex=2)] ITfRange[] ranges, out int fetched);
        [SecurityCritical]
        void Reset();
        [PreserveSig, SecurityCritical]
        int Skip(int count);
    }


//============================================================================


    [ComImport, 
     Guid("a3ad50fb-9bdb-49e3-a843-6c76520fbf5d"), 
     InterfaceType(ComInterfaceType.InterfaceIsIUnknown)
    ]
    public interface ITfCandidateList
    {
        void EnumCandidates(out object enumCand);
        [SecurityCritical]
        void GetCandidate(int nIndex, out ITfCandidateString candstring);
        [SecurityCritical]
        void GetCandidateNum(out int nCount);
        [SecurityCritical]
        void SetResult(int nIndex, TfCandidateResult result);
    }


//============================================================================


    [ComImport,
     InterfaceType(ComInterfaceType.InterfaceIsIUnknown),
     Guid("581f317e-fd9d-443f-b972-ed00467c5d40")
    ]
    public interface ITfCandidateString
    {
        [SecurityCritical]
        void GetString([MarshalAs(UnmanagedType.BStr)] out string funcName);
        void GetIndex(out int nIndex);
    }
 

//============================================================================


    [ComImport, 
     InterfaceType(ComInterfaceType.InterfaceIsIUnknown), 
     Guid("4cea93c0-0a58-11d3-8df0-00105a2799b5")
    ]
    public interface ITfFnReconversion
    {
        void GetDisplayName([MarshalAs(UnmanagedType.BStr)] out string funcName);
        [PreserveSig, SecurityCritical, ]
        int QueryRange(ITfRange range, out ITfRange newRange, [MarshalAs(UnmanagedType.Bool)] out bool isConvertable);
        [PreserveSig, SecurityCritical, ]
        int GetReconversion(ITfRange range, out ITfCandidateList candList);
        [PreserveSig, SecurityCritical]
        int Reconvert(ITfRange range);
    }


//============================================================================


    [ComImport, 
     SecurityCritical, 
     Guid("D7540241-F9A1-4364-BEFC-DBCD2C4395B7"), 
     InterfaceType(ComInterfaceType.InterfaceIsIUnknown)
    ]
    public interface ITfCompositionView
    {
        [SecurityCritical]
        void GetOwnerClsid(out Guid clsid);
        [SecurityCritical]
        void GetRange(out ITfRange range);
    }

 
//============================================================================

#if !METRO
    /// <summary>
    /// テキストフレームワークの関数。
    /// </summary>
    public static class TextFrameworkFunctions
    {
        /// <summary>
        /// スレッドマネージャーの生成。
        /// </summary>
        /// <param name="o_threadManager">スレッドマネージャーの受け取り先。</param>
        [DllImport("msctf.dll")]
        public static extern void TF_CreateThreadMgr(
            [Out, MarshalAs(UnmanagedType.Interface)] out ITfThreadMgr o_threadManager
        );

        /// <summary>
        /// スレッドマネージャーが既に生成されている場合、そのポインタを取得する。
        /// </summary>
        /// <param name="o_threadManager">スレッドマネージャーの受け取り先。</param>
        [DllImport("msctf.dll")]
        public static extern void TF_GetThreadMgr(
            [Out, MarshalAs(UnmanagedType.Interface)] out ITfThreadMgr o_threadManager
        );
    }
#endif

//============================================================================


    /// <summary>
    /// テキストフレームワークで宣言されている定数等。
    /// </summary>
    public static class TfDeclarations
    {
        public static readonly Guid CLSID_TF_ThreadMgr = new Guid("529a9e6b-6587-4f23-ab9e-9c7d683e3c50");
        public static readonly Guid GUID_SYSTEM_FUNCTIONPROVIDER = new Guid("9a698bb0-0f21-11d3-8df1-00105a2799b5");
        public static readonly Guid CLSID_TF_DisplayAttributeMgr = new Guid("3ce74de4-53d3-4d74-8b83-431b3828ba53");
        public static readonly Guid CLSID_TF_CategoryMgr = new Guid("a4b544a1-438d-4b41-9325-869523e2d6c7");
        public static readonly Guid GUID_PROP_ATTRIBUTE = new Guid("34b45670-7526-11d2-a147-00105a2799b5");
        public static readonly Guid GUID_PROP_MODEBIAS = new Guid("372e0716-974f-40ac-a088-08cdc92ebfbc");
        public static readonly Guid GUID_MODEBIAS_NONE = Guid.Empty;
    }


    [StructLayout(LayoutKind.Sequential)]
    public struct VARIANT
    {
        [MarshalAs(UnmanagedType.I2)]
        public short vt;
        [MarshalAs(UnmanagedType.I2)]
        public short reserved1;
        [MarshalAs(UnmanagedType.I2)]
        public short reserved2;
        [MarshalAs(UnmanagedType.I2)]
        public short reserved3;
        public IntPtr data1;
        public IntPtr data2;
    }
 
}
