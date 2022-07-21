using System;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Security;
using DotNetTextStore.UnmanagedAPI.WinDef;


namespace DotNetTextStore.UnmanagedAPI.TSF.TextStore
{
    /// <summary>
    /// ITextStoreACP::RequestLock() で使用されるロックのフラグ。
    /// </summary>
    public enum LockFlags
    {
        /// <summary>
        /// 読み取り専用。
        /// </summary>
        TS_LF_READ = 2,
        /// <summary>
        /// 読み書き両用。
        /// </summary>
        TS_LF_READWRITE = 6,
        /// <summary>
        /// 同期ロック。その他のフラグと組み合わせて使用する。
        /// </summary>
        TS_LF_SYNC = 1,
        /// <summary>
        /// 書き込み用。
        /// </summary>
        TS_LF_WRITE = 4
    }


    /// <summary>
    /// TS_STATUS 構造体の dynamicFlags メンバで使用されるフラグ
    /// </summary>
    public enum DynamicStatusFlags
    {
        /// <summary>
        /// ドキュメントはロード中の状態。
        /// </summary>
        TS_SD_LOADING = 2,
        /// <summary>
        /// ドキュメントは読み取り専用。
        /// </summary>
        TS_SD_READONLY = 1
    }


    /// <summary>
    /// TS_STATUS 構造体の staticFlags メンバで使用するフラグ。
    /// </summary>
    public enum StaticStatusFlags
    {
        /// <summary>
        /// ドキュメントは複数選択をサポートしている。
        /// </summary>
        TS_SS_DISJOINTSEL = 1,
        /// <summary>
        /// 隠しテキストを含めることはない。
        /// </summary>
        TS_SS_NOHIDDENTEXT = 8,
        /// <summary>
        /// ドキュメントは複数のリージョンを含められる。
        /// </summary>
        TS_SS_REGIONS = 2,
        /// <summary>
        /// ドキュメントは短い使用サイクルを持つ。
        /// </summary>
        TS_SS_TRANSITORY = 4,
        /// <summary>
        /// ドキュメントはタッチキーボードによるオートコレクションをサポートしている
        /// </summary>
        TS_SS_TKBAUTOCORRECTENABLE = 0x10,
        /// <summary>
        /// ドキュメントはタッチキーボードによる予測入力に対応している
        /// </summary>
        TS_SS_TKBPREDICTIONENABLE = 0x20
    }


    /// <summary>
    /// ドキュメントのステータスを示す構造体。ITextStoreACP::GetStatus() で使用される。
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack=4),
     Guid("BC7D979A-846A-444D-AFEF-0A9BFA82B961")
    ]
    public struct TS_STATUS
    {
        /// <summary>
        /// 実行時に変更できる状態フラグ。
        /// </summary>
        public DynamicStatusFlags dynamicFlags;
        /// <summary>
        /// 実行時に変更できない一貫性をもつフラグ。
        /// </summary>
        public StaticStatusFlags staticFlags;
    }


    /// <summary>
    /// ITextStoreACP::SetText() で使用されるフラグ。
    /// </summary>
    [Flags]
    public enum SetTextFlags
    {
        /// <summary>
        /// 既存のコンテントの移動(訂正)であり、特別なテキストマークアップ情報
        /// (メタデータ: .wav ファイルデータや言語IDなど)が保持される。クライア
        /// ントは保持されるマークアップ情報のタイプを定義する。
        /// </summary>
        TS_ST_CORRECTION = 1
    }


    /// <summary>
    /// ITextStoreACP::AdviseSink() メソッドに渡されるフラグ。要求する変更通知
    /// を組み合わせて使用する。
    /// </summary>
    public enum AdviseFlags
    {
        /// <summary>
        /// ドキュメントの属性が変更された。
        /// </summary>
        TS_AS_ATTR_CHANGE = 8,
        /// <summary>
        /// ドキュメントのレイアウトが変更された。
        /// </summary>
        TS_AS_LAYOUT_CHANGE = 4,
        /// <summary>
        /// テキストはドキュメント内で選択された。
        /// </summary>
        TS_AS_SEL_CHANGE = 2,
        /// <summary>
        /// ドキュメントのステータスが変更された。
        /// </summary>
        TS_AS_STATUS_CHANGE = 0x10,
        /// <summary>
        /// テキストはドキュメント内で変更された。
        /// </summary>
        TS_AS_TEXT_CHANGE = 1
    }


    /// <summary>
    /// 選択範囲を示す構造体。
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack=4),
     Guid("C4B9C33B-8A0D-4426-BEBE-D444A4701FE9")
    ]
    public struct TS_SELECTION_ACP
    {
        /// <summary>
        /// 開始文字位置。
        /// </summary>
        public int start;
        /// <summary>
        /// 終了文字位置。
        /// </summary>
        public int end;
        /// <summary>
        /// スタイル。選択範囲の開始位置が終了位置なのか開始位置なのか示すフラ
        /// グの ase と仮決定を示す真偽値 interimChar を持つ。
        /// </summary>
        public TS_SELECTIONSTYLE style;
    }


    /// <summary>
    /// TS_SELECTION_ACP のメンバとして使用される構造体。
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack=4),
     Guid("7ECC3FFA-8F73-4D91-98ED-76F8AC5B1600")
    ]
    public struct TS_SELECTIONSTYLE
    {
        /// <summary>
        /// 選択範囲の開始位置が終了位置なのか開始位置なのか示す
        /// </summary>
        public TsActiveSelEnd ase;
        /// <summary>
        /// 用途不明。説明を見ると変換中の文字のことのようだが変換中の文字に対
        /// しても true が渡されたことは無い・・・
        /// </summary>
        [MarshalAs(UnmanagedType.Bool)]
        public bool interimChar;
    }


    /// <summary>
    /// 選択範囲の終了位置を示すフラグ。
    /// </summary>
    public enum TsActiveSelEnd
    {
        /// <summary>
        /// アクティブな選択は無い。
        /// </summary>
        TS_AE_NONE,
        /// <summary>
        /// 開始位置が選択範囲の終了位置である。
        /// </summary>
        TS_AE_START,
        /// <summary>
        /// 終了位置と選択範囲の終了位置は同じである。
        /// </summary>
        TS_AE_END
    }


    /// <summary>
    /// ITextStoreACP.FindNextAttrTransition() などで使用されるフラグ。
    /// </summary>
    [Flags]
    public enum AttributeFlags
    {
        /// <summary>
        /// バックワード検索。
        /// </summary>
        TS_ATTR_FIND_BACKWARDS = 1,
        /// <summary>
        /// 不明。資料なし。
        /// </summary>
        TS_ATTR_FIND_HIDDEN = 0x20,
        /// <summary>
        /// 不明。資料なし。
        /// </summary>
        TS_ATTR_FIND_UPDATESTART = 4,
        /// <summary>
        /// 不明。資料なし。
        /// </summary>
        TS_ATTR_FIND_WANT_END = 0x10,
        /// <summary>
        /// o_foundOffset パラメータは i_start からの属性変更のオフセットを受け取る。
        /// </summary>
        TS_ATTR_FIND_WANT_OFFSET = 2,
        /// <summary>
        /// 不明。資料なし。
        /// </summary>
        TS_ATTR_FIND_WANT_VALUE = 8
    }


    /// <summary>
    /// ITextStoreACP::InsertEmbedded() で使用されるフラグ
    /// </summary>
    [Flags]
    public enum InsertEmbeddedFlags
    {
        /// <summary>
        /// 既存のコンテントの移動(訂正)であり、特別なテキストマークアップ情報
        /// (メタデータ: .wav ファイルデータや言語IDなど)が保持される。クライア
        /// ントは保持されるマークアップ情報のタイプを定義する。
        /// </summary>
        TS_IE_CORRECTION = 1
    }


    /// <summary>
    /// ITextStoreACP::InsertTextAtSelection(),
    /// ITextStoreACP::InsertEmbeddedAtSelection() で使用されるフラグ。
    /// </summary>
    [Flags]
    public enum InsertAtSelectionFlags
    {
        /// <summary>
        /// テキストは挿入され、挿入後の開始位置・終了位置を受け取るパラメータ
        /// の値は NULL にでき、TS_TEXTCHANGE 構造体は埋められなければいけない。
        /// このフラグはテキスト挿入の結果を見るために使用する。
        /// </summary>
        TF_IAS_NOQUERY = 1,
        /// <summary>
        /// テキストは実際には挿入されず、挿入後の開始位置・終了位置を受け取る
        /// パラメータはテキスト挿入の結果を含む。これらのパラメータの値は、
        /// アプリケーションがどのようにドキュメントにテキストを挿入するかに依
        /// 存している。このフラグは実際にはテキストを挿入しないでテキスト挿入
        /// の結果を見るために使用する。このフラグを使う場合はTS_TEXTCHANGE 
        /// 構造体を埋める必要はない。
        /// </summary>
        TF_IAS_QUERYONLY = 2
    }


    /// <summary>
    /// ランタイプ
    /// </summary>
    public enum TsRunType
    {
        /// <summary>
        /// プレーンテキスト。
        /// </summary>
        TS_RT_PLAIN,
        /// <summary>
        /// 不可視。
        /// </summary>
        TS_RT_HIDDEN,
        /// <summary>
        /// アプリケーションや ITextStore インターフェイスを実装するテキスト
        /// サービスによるテキスト内の組み込まれたプライベートデータタイプ。
        /// </summary>
        TS_RT_OPAQUE
    }


    /// <summary>
    /// ラン情報を示す構造体。
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack=4),
     Guid("A6231949-37C5-4B74-A24E-2A26C327201D")
    ]
    public struct TS_RUNINFO
    {
        /// <summary>
        /// テキストラン内の文字数。
        /// </summary>
        public int length;
        /// <summary>
        /// テキストランのタイプ。
        /// </summary>
        public TsRunType type;
    }


    /// <summary>
    /// 変更前および変更後の開始位置・終了位置を示す構造体。
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack=4),
     Guid("F3181BD6-BCF0-41D3-A81C-474B17EC38FB")
    ]
    public struct TS_TEXTCHANGE
    {
        /// <summary>
        /// テキストがドキュメントに挿入される前の開始位置。
        /// </summary>
        public int start;
        /// <summary>
        /// テキストがドキュメントに挿入される前の終了位置。値は挿入位置である
        /// start と同じ値となる。もし、start と異なっている場合は、テキストは
        /// テキストの挿入の前に選択されていた。
        /// </summary>
        public int oldEnd;
        /// <summary>
        /// テキストを挿入した後の終了位置。
        /// </summary>
        public int newEnd;
    }


    [StructLayout(LayoutKind.Sequential, Pack=8),
     Guid("2CC2B33F-1174-4507-B8D9-5BC0EB37C197")
    ]
    public struct TS_ATTRVAL
    {
        public Guid attributeId;
        public int overlappedId;
        public int reserved;
        public VARIANT val;
    }


    /// <summary>
    /// ITextStoreACP::GetACPFromPoint() で使用されるフラグ。
    /// 
    /// <para>詳しくは http://msdn.microsoft.com/en-us/library/ms538418(v=VS.85).aspx の Remarks 参照。</para>
    /// </summary>
    [Flags]
    public enum GetPositionFromPointFlags
    {
        /// <summary>
        /// もし、スクリーン座標での点が文字バウンディングボックスに含まれてい
        /// ないなら、もっとも近い文字位置が返される。
        /// </summary>
        GXFPF_NEAREST = 2,
        /// <summary>
        /// もし、スクリーン座標での点が文字バウンディングボックスの中に含まれ
        /// ている場合、返される文字位置は ptScreen にもっとも近いバウンディン
        /// グの端を持つ文字の位置である。
        /// </summary>
        GXFPF_ROUND_NEAREST = 1
    }


    /// <summary>
    /// ITextStoreACPSink::OnTextChange() で使用されるフラグ。
    /// </summary>
    [Flags]
    public enum OnTextChangeFlags
    {
        NONE = 0,
        TS_TC_CORRECTION = 1
    }


    /// <summary>
    /// ITextStoreACPSink::OnLayoutChange() で使用されるフラグ。
    /// </summary>
    public enum TsLayoutCode
    {
        /// <summary>
        /// ビューが生成された。
        /// </summary>
        TS_LC_CREATE,
        /// <summary>
        /// レイアウトが変更された。
        /// </summary>
        TS_LC_CHANGE,
        /// <summary>
        /// レイアウトが破棄された。
        /// </summary>
        TS_LC_DESTROY
    }


    /// <summary>
    /// GetSelection() の i_index 引数に使用する定数。
    /// </summary>
    public enum GetSelectionIndex : int
    {
        /// <summary>
        /// デフォルト選択を意味する。
        /// </summary>
        TS_DEFAULT_SELECTION = -1
    }


//============================================================================


    /// <summary>
    /// テキストストアのメソッドの戻り値(C# では COMException のエラーコード)
    /// </summary>
    public static class TsResult
    {
        /// <summary>
        /// Application does not support the data type contained in the IDataObject object to be inserted using ITextStoreACP::InsertEmbedded. 
        /// </summary>
        public const int TS_E_FORMAT = unchecked((int)0x8004020a);
        /// <summary>
        /// Parameter is not within the bounding box of any character.
        /// </summary>
        public const int TS_E_INVALIDPOINT = unchecked((int)0x80040207);
        /// <summary>
        /// Range specified extends outside the document.
        /// </summary>
        public const int TS_E_INVALIDPOS = unchecked((int)0x80040200);
        /// <summary>
        /// Object does not support the requested interface.
        /// </summary>
        public const int TS_E_NOINTERFACE = unchecked((int)0x80040204);
        /// <summary>
        /// Application has not calculated a text layout.
        /// </summary>
        public const int TS_E_NOLAYOUT = unchecked((int)0x80040206);
        /// <summary>
        /// Application does not have a read-only lock or read/write lock for the document.
        /// </summary>
        public const int TS_E_NOLOCK = unchecked((int)0x80040201);
        /// <summary>
        /// Embedded content offset is not positioned before a TF_CHAR_EMBEDDED character.
        /// </summary>
        public const int TS_E_NOOBJECT = unchecked((int)0x80040202);
        /// <summary>
        /// Document has no selection.
        /// </summary>
        public const int TS_E_NOSELECTION = unchecked((int)0x80040205);
        /// <summary>
        /// Content cannot be returned to match the service GUID.
        /// </summary>
        public const int TS_E_NOSERVICE = unchecked((int)0x80040203);
        /// <summary>
        /// Document is read-only. Cannot modify content.
        /// </summary>
        public const int TS_E_READONLY = unchecked((int)0x80040209);
        /// <summary>
        /// Document cannot be locked synchronously.
        /// </summary>
        /// <remarks>
        /// CAUTION: this value is marked as 0x00040300 in the document.
        /// </remarks>
        public const int TS_E_SYNCHRONOUS = unchecked((int)0x00040208);
        /// <summary>
        /// Document successfully received an asynchronous lock.
        /// </summary>
        public const int TS_S_ASYNC = 0x01;
        /// <summary>
        /// COMCTL.h より。SINK をこれ以上登録できない。
        /// </summary>
        public const int CONNECT_E_ADVISELIMIT = -2147220991;
        /// <summary>
        /// COMCTL.h より。SINK は登録されていない。
        /// </summary>
        public const int CONNECT_E_NOCONNECTION = -2147220992;
    }


    //============================================================================


#if !METRO
    /// <summary>
    /// テキストストア
    /// </summary>
    [ComImport,
     InterfaceType(ComInterfaceType.InterfaceIsIUnknown),
     Guid("28888fe3-c2a0-483a-a3ea-8cb1ce51ff3d")
    ]
    public interface ITextStoreACP
    {
        /// <summary>
        /// TSF マネージャのシンクインターフェイスを識別する。
        /// <para>
        /// ITextStoreACP::AdviseSink() メソッドは ITextStoreACPSink インターフェイス
        /// から新しいアドバイズシンクをインストール、または既存のアドバイズシンクの
        /// 修正を行う。シンクインターフェイスは io_unknown_cp パラメータによって指定
        /// される。
        /// </para>
        /// </summary>
        ///
        /// <param name="i_riid">
        /// シンクインターフェイスを指定する。
        /// </param>
        ///
        /// <param name="i_unknown">
        /// シンクインターフェイスへのポインタ。NULL 不可。
        /// </param>
        ///
        /// <param name="i_mask">
        /// アドバイズシンクを通知するイベントを指定する。
        ///   <list type="table">
        ///     <listheader>
        ///       <term>フラグ(値)</term>
        ///       <description>コメント</description>
        ///     </listheader>
        ///     <item>
        ///       <term>TS_AS_TEXT_CHANGE(0x1)</term>
        ///       <description>テキストはドキュメント内で変更された。</description>
        ///     </item>
        ///     <item>
        ///       <term>TS_AS_SEL_CHANGE(0x2)</term>
        ///       <description>テキストはドキュメント内で選択された。</description>
        ///     </item>
        ///     <item>
        ///       <term>TS_AS_LAYOUT_CHANGE(0x04)</term>
        ///       <description>ドキュメントのレイアウトが変更された。</description>
        ///     </item>
        ///     <item>
        ///       <term>TS_AS_ATTR_CHANGE(0x08)</term>
        ///       <description>ドキュメントの属性が変更された。</description>
        ///     </item>
        ///     <item>
        ///       <term>TS_AS_STATUS_CHANGE(0x10)</term>
        ///       <description>ドキュメントのステータスが変更された。</description>
        ///     </item>
        ///     <item>
        ///       <term>TS_AS_ALL_SINKS</term>
        ///       <description>上記全て</description>
        ///     </item>
        ///   </list>
        /// </param>
        void AdviseSink(
            [In] ref Guid                                       i_riid,
            [In, MarshalAs(UnmanagedType.Interface)] object     i_unknown,
            [In] AdviseFlags                                    i_mask
        );


        //====================================================================


        /// <summary>
        ///   アプリケーションはもはやTSFマネージャから通知を必要としないことを示す
        ///   ためにアプリケーションによって呼ばれる。TSFマネージャはシンクインター
        ///   フェイスの解放と通知の停止を行う。
        /// </summary>
        ///
        /// <param name="i_unknown">
        ///   シンクオブジェクトへのポインタ。NULL 不可。
        /// </param>
        ///
        /// <remark>
        ///   新しいシンクオブジェクトを登録する ITextStoreACP::AdviseSink メソッド
        ///   の全ての呼び出しは、このメソッドの呼び出しと対応していなければならない。
        ///   以前に登録されたシンクの dwMask パラメータを更新するだけの
        ///   ITextStoreACP::AdviseSink() メソッドの呼び出しは
        ///   ITextStoreACP::UnadviseSink() メソッドの呼び出しを必要としない。
        ///
        ///   <para>
        ///     io_unknown_cp パラメータは ITextStoreACP::AdviseSink メソッドに渡さ
        ///     れたオリジナルのポインタとして同じ COM 識別子でなければいけない。
        ///   </para>
        /// </remark>
        ///
        /// <returns>
        ///   <list type="table">
        ///     <listheader>
        ///       <term>戻り値</term>
        ///       <description>意味</description>
        ///     </listheader>
        ///     <item>
        ///       <term>S_OK</term>
        ///       <description>メソッドは成功した。</description>
        ///     </item>
        ///     <item>
        ///       <term>CONNECT_E_NOCONNECTION</term>
        ///       <description>
        ///         アクティブなシンクオブジェクトは存在しない。
        ///       </description>
        ///     </item>
        ///   </list>
        /// </returns>
        void UnadviseSink(
            [In, MarshalAs(UnmanagedType.Interface)] object i_unknown
        );


        //====================================================================


        /// <summary>
        ///   TSFマネージャがドキュメントを修正するためにドキュメントロックを提供す
        ///   るためにTSFマネージャによって呼び出される。このメソッドはドキュメント
        ///   ロックを作成するために ITextStoreACPSink::OnLockGranted() メソッドを呼
        ///   び出さなければいけない。
        /// </summary>
        ///
        /// <param name="i_lockFlags">ロック要求のタイプを指定する。
        ///   <list type="table">
        ///     <listheader>
        ///       <term>フラグ</term>
        ///       <description>意味</description>
        ///     </listheader>
        ///     <item>
        ///       <term>TS_LF_READ</term>
        ///       <description>
        ///         ドキュメントは読取専用ロックで、修正はできない。
        ///       </description>
        ///     </item>
        ///     <item>
        ///       <term>TS_LF_READWRITE</term>
        ///       <description>
        ///         ドキュメントは読み書き両用で、修正できる。
        ///       </description>
        ///     </item>
        ///     <item>
        ///       <term>TS_LF_SYNC</term>
        ///       <description>
        ///         他のフラグと一緒に指定された場合は、ドキュメントは同期ロックである。
        ///       </description>
        ///     </item>
        ///   </list>
        /// </param>
        ///
        /// <param name="o_sessionResult">
        ///   ロックリクエストが同期ならば、ロック要求の結果である
        ///   ITextStoreACP::OnLockGranted() メソッドからの HRESULT を受け取る。
        ///
        ///   <para>
        ///     ロック要求が非同期で結果が TS_S_ASYNC の場合、ドキュメントは非同期ロッ
        ///     クを受け取る。ロック要求が非同期で結果が TS_E_SYNCHRONOUS の場合は、ド
        ///     キュメントは非同期でロックできない。
        ///   </para>
        /// </param>
        ///
        /// <remark>
        ///   このメソッドはドキュメントをロックするために
        ///   ITextStoreACPSink::OnLockGranted() メソッドを使用する。アプリケーショ
        ///   ンは ITextStoreACP::RequestLock() メソッド内でドキュメントを修正したり
        ///   ITextStoreACPSink::OnTextChange() メソッドを使用して変更通知を送ったり
        ///   してはいけない。もし、アプリケーションがレポートするための変更をペンディ
        ///   ングしているなら、アプリケーションは非同期ロック要求のみを返さなければ
        ///   いけない。
        ///
        ///   <para>
        ///     アプリケーションは複数の RequestLock() メソッド呼び出しをキューに入
        ///     れることを試みてはいけない、なぜなら、アプリケーションは一つのコール
        ///     バックのみを要求されるから。もし、呼び出し元がいくつかの読取要求や一
        ///     つ以上の書き込み要求を作ったとしても、コールバックは書き込みアクセス
        ///     でなければいけない。
        ///   </para>
        ///
        ///   <para>
        ///     成功は、非同期ロックの要求にとって代わって同期ロックを要求する。失敗は
        ///     (複数の)非同期ロックの要求を同期ロックに代えることができない。実装は、
        ///     要求が存在すれば発行済みの非同期要求をまだ満たしていなければいけない。
        ///   </para>
        ///
        ///   <para>
        ///     もしロックが ITextStoreACP::RequestLock() メソッドが返る前に認められ
        ///     たなら、o_sessionResult パラメータは
        ///     ITextStoreACPSink::OnLockGranted() メソッドから返された HRESULT を受
        ///     け取る。もし、呼び出しが成功したが、ロックは後で認められるなら、
        ///     o_sessionResult パラメータは TS_S_ASYNC フラグを受け取る。もし、
        ///     RequestLock() が S_OK 以外を返した場合は o_sessionResult パラメー
        ///     ターは無視すべきである。
        ///   </para>
        ///
        ///   <para>
        ///     ドキュメントが既にロックされている状態で、同期ロック要求の場合は
        ///     o_sessionResult に TS_E_SYNCHRONOUS をセットして S_OK を返す。
        ///     これは同期要求は認められなかったことを示す。
        ///     ドキュメントが既にロックされている状態で、非同期ロック要求の場合は、
        ///     アプリケーションはリクエストをキューに追加し、 o_sessionResult に
        ///     TS_S_ASYNC をセットし、S_OK を返す。ドキュメントが有効になったとき、
        ///     アプリケーションはリクエストをキューから削除して、 OnLockGranted() 
        ///     を呼び出す。このロックリクエストのキューは任意である。アプリケーション
        ///     がサポートしなければいけないシナリオが一つある。ドキュメントが読み取
        ///     り専用ロックで、アプリケーションが OnLockGranted の内部で新しい読み
        ///     書き両用の非同期ロック要求を出した場合、RequestLock は再帰呼び出しを
        ///     引き起こす。アプリケーションは OnLockGranted() を TS_LF_READWRITE と
        ///     共に呼び出してリクエストをアップグレードすべきである。
        ///   </para>
        ///
        ///   <para>
        ///     呼び出し元が読み取り専用ロックを維持している場合を除いて、呼び出し元
        ///     はこのメソッドを再帰的に呼び出してはいけない。この場合、メソッドは非
        ///     同期に書き込み要求を尋ねるために再帰的に呼び出すことができる。書き込
        ///     みロックは読取専用ロックが終わった後に認められるだろう。
        ///   </para>
        ///
        ///   <para>
        ///     ロックの強要：アプリケーションは適切なロックのタイプが存在するかどうか
        ///     ドキュメントへのアクセスを許す前に確かめなければいけない。例えば、
        ///     GetText() を処理することを許可する前に少なくとも読み取り専用ロックが
        ///     行われているかどうか確かめなければいけない。もし、適切なロックがされ
        ///     ていなければ、アプリケーションは TF_E_NOLOCK を返さなければいけない。
        ///   </para>
        /// </remark>
        void RequestLock(
            [In] LockFlags                                  i_lockFlags,
            [Out, MarshalAs(UnmanagedType.Error)] out int   o_sessionResult
        );


        //====================================================================

        
        /// <summary>
        ///   ドキュメントのステータスを取得するために使用される。ドキュメントのステー
        ///   タスは TS_STATUS 構造体を通して返される。
        /// </summary>
        ///
        /// <param name="o_documentStatus">
        ///   ドキュメントのステータスを含む TS_STATUS 構造体を受け取る。NULL 不可。
        /// </param>
        void GetStatus(
            [Out] out TS_STATUS o_documentStatus
        );


        //====================================================================


        /// <summary>
        ///   ドキュメントが選択や挿入を許可することができるかどうかを決めるためにア
        ///   プリケーションから呼ばれる。
        ///
        ///   <para>
        ///     QueryInsert() メソッドは指定した開始位置と終了位置が有効かどうかを決
        ///     める。編集を実行する前にドキュメントの編集を調整するために使用される。
        ///     メソッドはドキュメントの範囲外の値を返してはいけない。
        ///   </para>
        /// </summary>
        ///
        /// <param name="i_startIndex">
        ///   テキストを挿入する開始位置。
        /// </param>
        ///
        /// <param name="i_endIndex">
        ///   テキストを挿入する終了位置。選択中のテキストを置換する代わりに指定した
        ///   位置へテキストを挿入する場合は、この値は i_startIndex と同じになる。
        /// </param>
        ///
        /// <param name="i_length">
        ///   置換するテキストの長さ。
        /// </param>
        ///
        /// <param name="o_startIndex">
        ///   挿入されるテキストの新しい開始位置を返す。このパラメータが NULL の場合、
        ///   テキストは指定された位置に挿入できない。この値はドキュメントの範囲外を
        ///   指定できない。
        /// </param>
        ///
        /// <param name="o_endIndex">
        ///   挿入されるテキストの新しい終了位置を返す。このパラメータが NULL の場合、
        ///   テキストは指定された位置に挿入できない。この値はドキュメントの範囲外を
        ///   指定できない
        /// </param>
        ///
        /// <remark>
        ///   o_startIndex と o_endIndex の値は、アプリケーションがどのように
        ///   ドキュメントにテキストを挿入するかに依存している。もし、o_startIndex 
        ///   と o_endIndex が i_startIndex と同じならば、カーソルは挿入後のテキ
        ///   ストの始まりにある。
        ///   もし、o_startIndex と o_endIndex が i_endIndex と同じならば、
        ///   カーソルは挿入後のテキストの終わりにある。o_startIndex と
        ///   o_endIndex の差が挿入されたテキストの長さと同じなら、挿入後、挿入さ
        ///   れたテキストはハイライトされている。
        /// </remark>
        void QueryInsert(
            [In] int        i_startIndex,
            [In] int        i_endIndex,
            [In] int        i_length,
            [Out] out int   o_startIndex, 
            [Out] out int   o_endIndex
        );


        //====================================================================

        
        /// <summary>
        ///   ドキュメント内のテキスト選択の位置を返す。このメソッドは複数テキスト選
        ///   択をサポートする。呼び出し元はこのメソッドを呼び出す前にドキュメントに
        ///   読取専用ロックをかけておかなければいけない。
        /// </summary>
        ///
        /// <param name="i_index">
        ///   処理を開始するテキスト選択を指定する。もし、TS_DEFAULT_SELECTION(-1)
        ///   定数が指定された場合、入力選択は処理を開始する。
        /// </param>
        ///
        /// <param name="i_selectionBufferLength">
        ///   返す選択の最大数を指定する。
        /// </param>
        ///
        /// <param name="o_selections">
        ///   選択されたテキストのスタイル、開始位置、終了位置を受け取る。これらの値
        ///   は TS_SELECTION_ACP 構造体に出力される。
        /// </param>
        ///
        /// <param name="o_fetchedLength">
        ///   o_selections に返された構造体の数を受け取る。
        /// </param>
        void GetSelection(
            [In] int                                                                        i_index,
            [In] int                                                                        i_selectionBufferLength,
            [Out, MarshalAs(UnmanagedType.LPArray, SizeParamIndex=1)] TS_SELECTION_ACP[]    o_selections,
            out int                                                                         o_fetchedLength
        );


        //====================================================================

        
        /// <summary>
        ///   ドキュメント内のテキストを選択する。アプリケーションはこのメソッドを呼
        ///   ぶ前に読み書き両用ロックをかけなればいけない。
        /// </summary>
        ///
        /// <param name="i_count">
        ///   i_selections 内のテキスト選択の数を指定する。
        /// </param>
        ///
        /// <param name="i_selections">
        ///   TS_SELECTION_ACP 構造体を通して選択されたテキストのスタイル、開始位置、
        ///   終了位置を指定する。
        /// 
        ///   <para>
        ///     開始位置と終了位置が同じ場合は、指定した位置にキャレットを配置する。
        ///     ドキュメント内に一度に一つのみキャレットを配置できる。
        ///   </para>
        /// </param>
        void SetSelection(
            [In] int                                                                    i_count,
            [In, MarshalAs(UnmanagedType.LPArray, SizeParamIndex=0)] TS_SELECTION_ACP[] i_selections
        );


        //====================================================================

        
        /// <summary>
        ///   指定された位置のテキストに関する情報を返す。このメソッドは可視状態およ
        ///   び不可視状態のテキストや埋め込まれたデータがテキストに関連付けられてい
        ///   るかどうかを返す。
        /// </summary>
        ///
        /// <param name="i_startIndex">
        ///   開始位置を指定する。
        /// </param>
        ///
        /// <param name="i_endIndex">
        ///   終了位置を指定する。このパラメータが -1 の場合はテキストストア内の全て
        ///   のテキストを返す。
        /// </param>
        ///
        /// <param name="o_plainText">
        ///   プレインテキストデータを受け取るバッファを指定する。このパラメータが
        ///   NULL の場合は、 cchPlainReq は 0 でなければいけない。
        /// </param>
        ///
        /// <param name="i_plainTextLength">
        ///   プレインテキストの文字数を指定する。
        /// </param>
        ///
        /// <param name="o_plainTextLength">
        ///   プレインテキストバッファへコピーされた文字数を受け取る。このパラメータ
        ///   は NULL を指定できない。値が必要でないときに使用する。
        /// </param>
        ///
        /// <param name="o_runInfos">
        ///   TS_RUNINFO　構造体の配列を受け取る。i_runInfoLength が 0 の場合は NULL。
        /// </param>
        ///
        /// <param name="i_runInfoLength">
        ///   o_runInfos の許容数を指定する。
        /// </param>
        ///
        /// <param name="o_runInfoLength">
        ///   o_runInfosに書き込まれた数を受け取る。このパラメータは NULL を指定で
        ///   きない。
        /// </param>
        ///
        /// <param name="o_nextUnreadCharPos">
        ///   次の読み込んでいない文字の位置を受け取る。このパラメータは NULL を指定
        ///   できない。
        /// </param>
        ///
        /// <remark>
        ///   このメソッドを使う呼び出し元は ITextStoreACP::RequestLock() メソッドを
        ///   呼ぶことでドキュメントに読取専用ロックをかけなければいけない。ロックし
        ///   ていない場合はメソッドは失敗し、TF_E_NOLOCK を返す。
        ///
        ///   <para>
        ///     アプリケーションはまた、内部の理由によって戻り値を切り取ることができる。
        ///     呼び出し元は必須の戻り値を取得するために戻された文字やテキストのラン
        ///     数を注意深く調査しなければいけない。戻り値が不完全ならば、戻り値が完
        ///     全なものとなるまでメソッドを繰り返し呼び出さなければいけない。
        ///   </para>
        ///
        ///   <para>
        ///     呼び出し元は i_runInfoLength パラメータを0にセットし、o_runInfos 
        ///     パラメータを NULL にすることで、プレインテキストを要求できる。しかし
        ///     ながら、呼び出し元は o_plainTextLength に非 NULL の有効な値を提
        ///     供しなければならない、パラメータを使用しないとしても。
        ///   </para>
        ///
        ///   <para>
        ///     i_endIndex が -1 の場合、ストリームの最後がセットされたものとして処
        ///     理しなければいけない。その他の場合、0 以上でなければいけない。
        ///   </para>
        ///
        ///   <para>
        ///     メソッドを抜ける際に、o_nextUnreadCharPos は戻り値によって参照
        ///     されていないストリーム内の次の文字の位置をセットされていなければなら
        ///     ない。呼び出し元はこれを使用して複数の GetText() 呼び出しで素早くテ
        ///     キストをスキャンする。
        ///   </para>
        ///
        /// </remark>
        void GetText(
            [In] int                                                                i_startIndex,
            [In] int                                                                i_endIndex,
            [Out, MarshalAs(UnmanagedType.LPArray, SizeParamIndex=3)] char[]        o_plainText,
            [In] int                                                                i_plainTextLength,
            [Out] out int                                                           o_plainTextLength,
            [Out, MarshalAs(UnmanagedType.LPArray, SizeParamIndex=6)] TS_RUNINFO[]  o_runInfos,
            [In] int                                                                i_runInfoLength,
            [Out] out int                                                           o_runInfoLength,
            [Out] out int                                                           o_nextUnreadCharPos
        );


        //====================================================================

        
        /// <summary>
        ///   SetText() メソッドは与えられた文字位置のテキスト選択をセットする。
        /// </summary>
        ///
        /// <param name="i_flags">
        ///   TS_ST_CORRECTION がセットされている場合、テキストは既存のコンテントの
        ///   移動(訂正)であり、特別なテキストマークアップ情報(メタデータ)が保持され
        ///   る - .wav ファイルデータや言語IDなど。クライアントは保持されるマークアッ
        ///   プ情報のタイプを定義する。
        /// </param>
        ///
        /// <param name="i_startIndex">
        ///   置換するテキストの開始位置を指定する。
        /// </param>
        ///
        /// <param name="i_endIndex">
        ///   置換するテキストの終了位置を指定する。-1 の場合は無視される。
        /// </param>
        ///
        /// <param name="i_text">
        ///   置き換えるテキストへのポインタを指定する。テキストの文字数は i_length
        ///   パラメータによって指定されるため、テキスト文字列は NULL 終端文字を持っ
        ///   ていない。
        /// </param>
        ///
        /// <param name="i_length">
        ///   i_text の文字数。
        /// </param>
        ///
        /// <param name="o_textChange">
        ///   次のデータをもつ TS_TEXTCHANGE 構造体へのポインタ。
        ///
        ///   <list type="table">
        ///     <listheader>
        ///       <term>メンバー名</term>
        ///       <description>意味</description>
        ///     </listheader>
        ///     <item>
        ///       <term>acpStart</term>
        ///       <description>
        ///         テキストがドキュメントに挿入される前の開始位置。
        ///       </description>
        ///     </item>
        ///     <item>
        ///       <term>acpOldEnd</term>
        ///       <description>
        ///         テキストがドキュメントに挿入される前の終了位置。値は挿入位置であ
        ///         る acpStart と同じ値となる。もし、acpStart と異なっている場合は、
        ///         テキストはテキストの挿入の前に選択されていた。
        ///       </description>
        ///     </item>
        ///     <item>
        ///       <term>acpNewEnd</term>
        ///       <description>
        ///         テキストを挿入した後の終了位置。
        ///       </description>
        ///     </item>
        ///   </list>
        /// </param>
        void SetText(
            [In] SetTextFlags                                               i_flags,
            [In] int                                                        i_startIndex,
            [In] int                                                        i_endIndex,
            [In, MarshalAs(UnmanagedType.LPArray, SizeParamIndex=4)] char[] i_text,
            [In] int                                                        i_length,
            [Out] out TS_TEXTCHANGE                                         o_textChange
        );


        //====================================================================

        
        /// <summary>
        /// 指定されたテキスト文字列について Formatted Text データを返す。呼び
        /// 出し元はこのメソッドを呼ぶ前に読み書き両用ロックをかけなければいけない。
        /// </summary>
        void GetFormattedText(
            [In] int                                                i_start,
            [In] int                                                i_end,
            [Out, MarshalAs(UnmanagedType.Interface)] out object    o_obj
        );


        //====================================================================


        /// <summary>
        /// 組み込みオブジェクトを取得する。
        /// </summary>
        void GetEmbedded(
            [In] int                                                i_position,
            [In] ref Guid                                           i_guidService,
            [In] ref Guid                                           i_riid,
            [Out, MarshalAs(UnmanagedType.Interface)] out object    o_obj
        );


        //====================================================================


        /// <summary>
        /// 組み込みオブジェクトを挿入できるか問い合わせる。
        /// </summary>
        void QueryInsertEmbedded(
            [In] ref Guid                                   i_guidService,
            [In] int                                        i_formatEtc,
            [Out, MarshalAs(UnmanagedType.Bool)] out bool   o_insertable
        );


        //====================================================================


        /// <summary>
        /// 組み込みオブジェクトを挿入する。
        /// </summary>
        void InsertEmbedded(
            [In] InsertEmbeddedFlags                        i_flags,
            [In] int                                        i_start,
            [In] int                                        i_end,
            [In, MarshalAs(UnmanagedType.Interface)] object i_obj,
            [Out] out TS_TEXTCHANGE                         o_textChange
        );


        //====================================================================


        /// <summary>
        ///   ITextStoreACP::InsertTextAtSelection() メソッドは挿入位置や選択位置に
        ///   テキストを挿入する。呼び出し元はテキストを挿入する前に読み書き両用ロッ
        ///   クをかけていなければならない。
        /// </summary>
        ///
        /// <param name="i_flags">
        ///   o_startIndex, o_endIndex パラメータと TS_TEXTCHANGE 構造体のどち
        ///   らがテキストの挿入の結果を含んでいるかを示す。
        ///   TF_IAS_NOQUERY と TF_IAS_QUERYONLY フラグは同時に指定できない。
        ///
        ///   <list type="table">
        ///     <listheader>
        ///       <term>値</term>
        ///       <description>意味</description>
        ///     </listheader>
        ///     <item>
        ///       <term>0</term>
        ///       <description>
        ///         テキスト挿入が発生し、o_startIndex と o_endIndex パラメータ
        ///         はテキスト挿入の結果を含んでいる。TS_TEXTCHANGE 構造体は 0 で埋め
        ///         られていなければならない。
        ///       </description>
        ///     </item>
        ///     <item>
        ///       <term>TF_IAS_NOQUERY</term>
        ///       <description>
        ///         テキストは挿入され、o_startIndex と o_endIndex パラメータ
        ///         の値は NULL にでき、TS_TEXTCHANGE 構造体は埋められなければいけない。
        ///         このフラグはテキスト挿入の結果を見るために使用する。
        ///       </description>
        ///     </item>
        ///     <item>
        ///       <term>TF_IAS_QUERYONLY</term>
        ///       <description>
        ///         テキストは挿入されず、o_startIndex と o_endIndex パラメータ
        ///         はテキスト挿入の結果を含む。これらのパラメータの値は、アプリケー
        ///         ションがどのようにドキュメントにテキストを挿入するかに依存している。
        ///         詳しくは注意を見ること。このフラグは実際にはテキストを挿入しない
        ///         でテキスト挿入の結果を見るために使用する。このフラグを使う場合は
        ///         TS_TEXTCHANGE 構造体を埋める必要はない。
        ///       </description>
        ///     </item>
        ///   </list>
        /// </param>
        ///
        /// <param name="i_text">
        ///   挿入する文字列へのポインタ。NULL 終端にすることができる。
        /// </param>
        ///
        /// <param name="i_length">
        ///   テキスト長を指定する。
        /// </param>
        ///
        /// <param name="o_startIndex">
        ///   テキスト挿入が発生した場所の開始位置へのポインタ。
        /// </param>
        ///
        /// <param name="o_endIndex">
        ///   テキスト挿入が発生した場所の終了位置へのポインタ。このパラメータは挿入
        ///   の場合、o_startIndex パラメータと同じ値になる。
        /// </param>
        ///
        /// <param name="o_textChange">
        ///   次のメンバーを持つ TS_TEXTCHANGE 構造体へのポインタ。
        ///
        ///   <list type="table">
        ///     <listheader>
        ///       <term>メンバー名</term>
        ///       <description>意味</description>
        ///     </listheader>
        ///     <item>
        ///       <term>acpStart</term>
        ///       <description>
        ///         テキストがドキュメントに挿入される前の開始位置。
        ///       </description>
        ///     </item>
        ///     <item>
        ///       <term>acpOldEnd</term>
        ///       <description>
        ///         テキストがドキュメントに挿入される前の終了位置。値は挿入位置であ
        ///         る acpStart と同じ値となる。もし、acpStart と異なっている場合は、
        ///         テキストはテキストの挿入の前に選択されていた。
        ///       </description>
        ///     </item>
        ///     <item>
        ///       <term>acpNewEnd</term>
        ///       <description>
        ///         テキストを挿入した後の終了位置。
        ///       </description>
        ///     </item>
        ///   </list>
        /// </param>
        ///
        /// <remark>
        ///   o_startIndex と o_endIndex パラメータの値はアプリケーションがド
        ///   キュメントにどのようにテキストを挿入したかによる。たとえば、アプリケー
        ///   ションがテキストを挿入後、挿入したテキストの開始位置にカーソルをセット
        ///   したなら、o_startIndex と o_endIndex パラメータは TS_TEXTCHANGE 
        ///   構造体の acpStart メンバと同じ値になる。
        ///   <para>
        ///     アプリケーションは ITextStoreACPSink::OnTextChange() メソッドをこの
        ///     メソッド内で呼ぶべきではない。
        ///   </para>
        /// </remark>
        void InsertTextAtSelection(
            [In] InsertAtSelectionFlags                                     i_flags,
            [In, MarshalAs(UnmanagedType.LPArray, SizeParamIndex=2)] char[] i_text,
            [In] int                                                        i_length,
            [Out] out int                                                   o_startIndex,
            [Out] out int                                                   o_endIndex,
            [Out] out TS_TEXTCHANGE                                         o_textChange
        );


        //====================================================================


        /// <summary>
        ///   選択位置もしくは挿入位置に組み込みオブジェクトを挿入する。
        /// </summary>
        void InsertEmbeddedAtSelection(
            InsertAtSelectionFlags flags,
            [MarshalAs(UnmanagedType.Interface)] object obj,
            out int start,
            out int end,
            out TS_TEXTCHANGE change
        );


        //====================================================================


        /// <summary>
        ///   ドキュメントのサポートされている属性を取得する。
        /// </summary>
        /// 
        /// <param name="i_flags">
        ///   後続の ITextStoreACP::RetrieveRequestedAttrs() メソッド呼び出し
        ///   がサポートされる属性を含むかどうかを指定する。もし、
        ///   TS_ATTR_FIND_WANT_VALUE フラグが指定されたなら、後続の
        ///   ITextStoreAcp::RetrieveRequestedAttrs() 呼び出しの後に
        ///   TS_ATTRVAL 構造体のそれらはデフォルト属性の値になる。もし、他の
        ///   フラグが指定されたなら属性がサポートされているかどうか確認する
        ///   だけで、 TS_ATTRVAL 構造体の varValue メンバには VT_EMPTY がセッ
        ///   トされる。
        /// </param>
        /// 
        /// <param name="i_length">
        ///   取得するサポートされる属性の数を指定する。
        /// </param>
        /// 
        /// <param name="i_filterAttributes">
        ///   確認するための属性が指定された TS_ATTRID データタイプへのポイン
        ///   タ。メソッドは TS_ATTRID によって指定された属性のみを返す(他の
        ///   属性をサポートしていたとしても)。
        /// </param>
        void RequestSupportedAttrs(
            [In] AttributeFlags                                             i_flags,
            [In] int                                                        i_length,
            [In, MarshalAs(UnmanagedType.LPArray, SizeParamIndex=1)] Guid[] i_filterAttributes
        );

        
        //====================================================================


        /// <summary>
        ///   指定した文字位置の属性を取得する。
        /// </summary>
        /// 
        /// <param name="i_position">
        ///   ドキュメント内の開始位置。
        /// </param>
        /// 
        /// <param name="i_length">
        ///   取得する属性の数。
        /// </param>
        /// 
        /// <param name="i_filterAttributes">
        ///   確認するための属性が指定された TS_ATTRID データタイプへのポインタ。
        /// </param>
        /// 
        /// <param name="i_flags">
        ///   0 でなければいけない。
        /// </param>
        void RequestAttrsAtPosition(
            [In] int                                                        i_position,
            [In] int                                                        i_length,
            [In, MarshalAs(UnmanagedType.LPArray, SizeParamIndex=1)] Guid[] i_filterAttributes,
            [In] AttributeFlags                                             i_flags
        );


        //====================================================================

				
        /// <summary>
        ///   指定した文字位置の属性のリストを取得する。
        /// </summary>
        /// <param name="i_position">ドキュメント内の開始位置。</param>
        /// 
        /// <param name="i_length">取得する属性の数。</param>
        /// 
        /// <param name="i_filterAttributes">
        ///   RetrieveRequestedAttr() メソッドを呼ぶための属性を指定する。
        ///   このパラメータがセットされていなければメソッドは指定された位置で
        ///   始まる属性を返す。他の指定可能な値は以下のとおり。
        ///   <list type="table">
        ///     <listheader>
        ///       <term>値</term>
        ///       <description>意味</description>
        ///     </listheader>
        ///     <item>
        ///       <term>TS_ATTR_FIND_WANT_END</term>
        ///       <description>
        ///         指定した文字位置で終了する属性を取得する。
        ///       </description>
        ///     </item>
        ///     <item>
        ///       <term>TS_ATTR_FIND_WANT_VALUD</term>
        ///       <description>
        ///         属性の取得に加えて属性の値を取得する。属性値は
        ///         ITextStoreACP::RetrieveRequestedAttrs() メソッド呼び出しの
        ///         TS_ATTRVAL 構造体の varValue メンバにセットされる。
        ///       </description>
        ///     </item>
        ///   </list>
        /// </param>
        /// <param name="i_flags"></param>
        void RequestAttrsTransitioningAtPosition(
            [In] int                                                        i_position,
            [In] int                                                        i_length,
            [In, MarshalAs(UnmanagedType.LPArray, SizeParamIndex=1)] Guid[] i_filterAttributes,
            [In] AttributeFlags                                             i_flags
        );


        //====================================================================

				
        /// <summary>
        ///   属性変更が発生している場所の文字位置を決定する。
        /// </summary>
        /// <param name="i_start">属性変更を検索する開始位置を指定する。</param>
        /// <param name="i_halt">属性変更を検索する終了位置を指定する。</param>
        /// <param name="i_length">チェックするための属性の数を指定する。</param>
        /// <param name="i_filterAttributes">
        ///   チェックするための属性を指定する TS_ATTRID データタイプへのポインタ。
        /// </param>
        /// <param name="i_flags">
        ///   検索方向を指定する。デフォルトではフォワード検索。
        ///   <list type="table">
        ///     <listheader>
        ///       <term>フラグ</term>
        ///       <description>意味</description>
        ///     </listheader>
        ///     
        ///     <item>
        ///       <term>TS_ATTR_FIND_BACKWARDS</term>
        ///       <description>バックワード検索。</description>
        ///     </item>
        ///     <item>
        ///       <term>tS_ATTR_FIND_WANT_OFFSET</term>
        ///       <description>
        ///         o_foundOffset パラメータは i_start からの属性変更のオフセットを受け取る。
        ///       </description>
        ///     </item>
        ///   </list>
        /// </param>
        /// <param name="o_nextIndex">
        ///   属性変更をチェックする次の文字位置を受け取る。
        /// </param>
        /// <param name="o_found">
        ///   属性変更が発見された場合に TRUE を受け取る。そのほかは FALSE を受け取る。
        /// </param>
        /// <param name="o_foundOffset">
        ///   属性変更の開始位置(文字位置ではない)を受け取る。TS_ATTR_FIND_WANT_OFFSET
        ///   フラグが dwFlags にセットされていれば、 acpStart からのオフセットを受け取る。
        /// </param>
        void FindNextAttrTransition(
            [In] int                                                        i_start,
            [In] int                                                        i_halt,
            [In] int                                                        i_length,
            [In, MarshalAs(UnmanagedType.LPArray, SizeParamIndex=2)] Guid[] i_filterAttributes,
            [In] AttributeFlags                                             i_flags,
            [Out] out int                                                   o_nextIndex,
            [Out, MarshalAs(UnmanagedType.Bool)] out bool                   o_found,
            [Out] out int                                                   o_foundOffset
        );

        
        //====================================================================

        
        /// <summary>
        ///   ITextStoreACP::RequestAttrsAtPosition(),
        ///   TextStoreACP::RequestAttrsTransitioningAtPosition(),
        ///   ITextStoreACP::RequestSupportedAttrs() によって取得された属性を返す。
        /// </summary>
        /// <param name="i_length">取得するサポートされる属性の数を指定する。</param>
        /// <param name="o_attributeVals">
        ///   サポートされる属性を受け取る TS_ATTRVAL 構造体へのポインタ。この
        ///   構造体のメンバはメソッド呼び出しの i_flags パラメータによる。
        /// </param>
        /// <param name="o_fetchedLength">サポートされる属性の数を受け取る。</param>
        void RetrieveRequestedAttrs(
            [In] int                                                                i_length,
            [Out, MarshalAs(UnmanagedType.LPArray, SizeParamIndex=0)] TS_ATTRVAL[]  o_attributeVals,
            [Out] out int                                                           o_fetchedLength
        );

        
        //====================================================================

        
        /// <summary>ドキュメント内の文字数を取得する。</summary>
        ///
        /// <param name="o_length">最後の文字位置 + 1 を受け取る。</param>
        void GetEndACP(
            [Out] out int o_length
        );

        
        //====================================================================

        
        /// <summary>
        /// 現在のアクティブビューの TsViewCookie データタイプを返す。
        /// </summary>
        void GetActiveView(
            [Out] out int o_viewCookie
        );

        
        //====================================================================

        
        /// <summary>
        ///   スクリーン座標をアプリケーション文字位置に変換する。
        /// </summary>
        /// <param name="i_viewCookie">コンテキストビューを指定する。</param>
        /// <param name="i_point">スクリーン座標の位置を示す POINT 構造体へのポインタ。</param>
        /// <param name="i_flags">
        ///   文字バウンディングボックスからの相対位置に基づくスクリーン座標を
        ///   戻すための文字位置を指定する。デフォルトでは、返される文字位置は
        ///   スクリーン座標を含む文字バウンディングボックスをもつ文字の位置で
        ///   ある。もし、ポイントが文字を囲むボックス外ならメソッドは NULL ま
        ///   たは TF_E_INVALIDPOINT を返す。その他のフラグは以下のとおり。
        ///   <list>
        ///     <listheader>
        ///       <term>フラグ</term>
        ///       <description>意味</description>
        ///     </listheader>
        ///     
        ///     <item>
        ///       <term>GXFPF_ROUND_NEAREST</term>
        ///       <description>
        ///         もし、スクリーン座標での点が文字バウンディングボックスの中
        ///         に含まれている場合、返される文字位置は ptScreen にもっとも
        ///         近いバウンディングの端を持つ文字の位置である。
        ///       </description>
        ///     </item>
        ///     <item>
        ///       <term>GXFPF_NEAREST</term>
        ///       <description>
        ///         もし、スクリーン座標での点が文字バウンディングボックスに含
        ///         まれていないなら、もっとも近い文字位置が返される。
        ///         <para>
        ///         http://msdn.microsoft.com/en-us/library/ms538418(v=VS.85).aspx の Remarks 参照。
        ///         </para>
        ///       </description>
        ///     </item>
        ///   </list>
        /// </param>
        /// <param name="o_index"></param>
        void GetACPFromPoint(
            [In] int                        i_viewCookie,
            [In] ref POINT                  i_point,
            [In] GetPositionFromPointFlags  i_flags,
            [Out] out int                   o_index
        );

        
        //====================================================================

        
        /// <summary>
        ///   指定した文字位置のスクリーン座標を返す。読み取り専用ロックをかけて呼ば
        ///   なければいけない。
        /// </summary>
        ///
        /// <param name="i_viewCookie">
        ///   コンテキストビューを指定する。
        /// </param>
        ///
        /// <param name="i_startIndex">
        ///   ドキュメント内の取得するテキストの開始位置を指定する。
        /// </param>
        ///
        /// <param name="i_endIndex">
        ///   ドキュメント内の取得するテキストの終了位置を指定する。
        /// </param>
        ///
        /// <param name="o_rect">
        ///   指定した文字位置のテキストのスクリーン座標でのバウンディングボックスを
        ///   受け取る。
        /// </param>
        ///
        /// <param name="o_isClipped">
        ///   バウンディングボックスがクリッピングされたかどうかを受け取る。TRUE の
        ///   場合、バウンディングボックスはクリップされたテキストを含み、完全な要求
        ///   されたテキストの範囲を含んでいない。バウンディングボックスは要求された
        ///   範囲が可視状態でないため、クリップされた。
        /// </param>
        ///
        /// <remark>
        ///   ドキュメントウィンドウが最小化されていたり、指定されたテキストが現在表
        ///   示されていないならば、メソッドは S_OK を返して o_rect パラメータに
        ///   { 0, 0, 0, 0 }をセットしなければいけない。
        ///
        ///   <para>
        ///     TSF マネージャー側から変換候補ウィンドウの表示位置を割り出すために使用
        ///     される。
        ///   </para>
        /// </remark>
        void GetTextExt(
            [In] int                                    i_viewCookie,
            [In] int                                    i_startIndex,
            [In] int                                    i_endIndex,
            [Out] out RECT                              o_rect,
            [MarshalAs(UnmanagedType.Bool)] out bool    o_isClipped
        );

        
        //====================================================================

        
        /// <summary>
        ///   テキストストリームが描画されるディスプレイサーフェイスのスクリーン座標
        ///   でのバウンディングボックスを取得する。
        /// </summary>
        ///
        /// <remark>
        ///   ドキュメントウィンドウが最小化されていたり、指定されたテキストが現在表
        ///   示されていないならば、メソッドは S_OK を返して o_rect パラメータに
        ///   { 0, 0, 0, 0 }をセットしなければいけない。
        /// </remark>
        void GetScreenExt(
            [In] int        i_viewCookie,
            [Out] out RECT  o_rect
        );

        
        //====================================================================

        
	    /// <summary>
	    ///   現在のドキュメントに一致するウィンドウのハンドルを取得する。
	    /// </summary>
	    ///
	    /// <param name="i_viewCookie">
	    ///   現在のドキュメントに一致する TsViewCookie データタイプを指定する。
	    /// </param>
	    ///
	    /// <param name="o_hwnd">
	    ///   現在のドキュメントに一致するウィンドウのハンドルへのポインタを受け取る。
	    ///   一致するウィンドウがなければ NULL にできる。
	    /// </param>
	    ///
	    /// <remark>
	    ///   ドキュメントはメモリにあるが、スクリーンに表示されていない場合や、ウィ
	    ///   ンドウがないコントロールや、ウィンドウがないコントロールのオーナーのウィ
	    ///   ンドウハンドルを認識しない場合、ドキュメントは一致するウィンドウハンド
	    ///   ルをもてない。呼び出し元は メソッドが成功したとしても o_hwnd パラメー
	    ///   タに非 NULL 値を受け取ると想定してはいけない。
	    /// </remark>
        void GetWnd(
            [In] int            i_viewCookie,
            [Out] out IntPtr    o_hwnd
        );
    }
#endif


    //============================================================================
    /// <summary>
    /// テキストストア
    /// </summary>
    [ComImport,
     InterfaceType(ComInterfaceType.InterfaceIsIUnknown),
     Guid("f86ad89f-5fe4-4b8d-bb9f-ef3797a84f1f")
    ]
    public interface ITextStoreACP2
    {
        /// <summary>
        /// ITextStoreACP2::AdviseSink
        /// </summary>
        /// <param name="i_riid"></param>
        /// <param name="i_unknown"></param>
        /// <param name="i_mask"></param>
        void AdviseSink(
            [In] ref Guid i_riid,
            [In, MarshalAs(UnmanagedType.Interface)] object i_unknown,
            [In] AdviseFlags i_mask
        );
        /// <summary>
        /// ITextStoreACP2::UnadviseSink
        /// </summary>
        /// <param name="i_unknown"></param>
        void UnadviseSink(
            [In, MarshalAs(UnmanagedType.Interface)] object i_unknown
        );
        /// <summary>
        /// ITextStoreACP2::RequestLock
        /// </summary>
        /// <param name="i_lockFlags"></param>
        /// <param name="o_sessionResult"></param>
        void RequestLock(
            [In] LockFlags i_lockFlags,
            [Out, MarshalAs(UnmanagedType.Error)] out int o_sessionResult
        );
        /// <summary>
        /// ITextStoreACP2::GetStatus
        /// </summary>
        /// <param name="o_documentStatus"></param>
        void GetStatus(
            [Out] out TS_STATUS o_documentStatus
        );
        /// <summary>
        /// ITextStoreACP2::QueryInsert
        /// </summary>
        /// <param name="i_startIndex"></param>
        /// <param name="i_endIndex"></param>
        /// <param name="i_length"></param>
        /// <param name="o_startIndex"></param>
        /// <param name="o_endIndex"></param>
        void QueryInsert(
            [In] int i_startIndex,
            [In] int i_endIndex,
            [In] int i_length,
            [Out] out int o_startIndex,
            [Out] out int o_endIndex
        );
        /// <summary>
        /// ITextStoreACP2::GetSelection
        /// </summary>
        /// <param name="i_index"></param>
        /// <param name="i_selectionBufferLength"></param>
        /// <param name="o_selections"></param>
        /// <param name="o_fetchedLength"></param>
        void GetSelection(
            [In] int i_index,
            [In] int i_selectionBufferLength,
            [Out, MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 1)] TS_SELECTION_ACP[] o_selections,
            out int o_fetchedLength
        );
        /// <summary>
        /// ITextStoreACP2::SetSelection
        /// </summary>
        /// <param name="i_count"></param>
        /// <param name="i_selections"></param>
        void SetSelection(
            [In] int i_count,
            [In, MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 0)] TS_SELECTION_ACP[] i_selections
        );
        /// <summary>
        /// ITextStoreACP2::GetText
        /// </summary>
        /// <param name="i_startIndex"></param>
        /// <param name="i_endIndex"></param>
        /// <param name="o_plainText"></param>
        /// <param name="i_plainTextLength"></param>
        /// <param name="o_plainTextLength"></param>
        /// <param name="o_runInfos"></param>
        /// <param name="i_runInfoLength"></param>
        /// <param name="o_runInfoLength"></param>
        /// <param name="o_nextUnreadCharPos"></param>
        void GetText(
            [In] int i_startIndex,
            [In] int i_endIndex,
            [Out, MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 3)] char[] o_plainText,
            [In] int i_plainTextLength,
            [Out] out int o_plainTextLength,
            [Out, MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 6)] TS_RUNINFO[] o_runInfos,
            [In] int i_runInfoLength,
            [Out] out int o_runInfoLength,
            [Out] out int o_nextUnreadCharPos
        );
        /// <summary>
        /// ITextStoreACP2::SetText
        /// </summary>
        /// <param name="i_flags"></param>
        /// <param name="i_startIndex"></param>
        /// <param name="i_endIndex"></param>
        /// <param name="i_text"></param>
        /// <param name="i_length"></param>
        /// <param name="o_textChange"></param>
        void SetText(
            [In] SetTextFlags i_flags,
            [In] int i_startIndex,
            [In] int i_endIndex,
            [In, MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 4)] char[] i_text,
            [In] int i_length,
            [Out] out TS_TEXTCHANGE o_textChange
        );
        /// <summary>
        /// ITextStoreACP2::GetFormattedText
        /// </summary>
        /// <param name="i_start"></param>
        /// <param name="i_end"></param>
        /// <param name="o_obj"></param>
        void GetFormattedText(
            [In] int i_start,
            [In] int i_end,
            [Out, MarshalAs(UnmanagedType.Interface)] out object o_obj
        );
        /// <summary>
        /// ITextStoreACP2::GetEmbedded
        /// </summary>
        /// <param name="i_position"></param>
        /// <param name="i_guidService"></param>
        /// <param name="i_riid"></param>
        /// <param name="o_obj"></param>
        void GetEmbedded(
            [In] int i_position,
            [In] ref Guid i_guidService,
            [In] ref Guid i_riid,
            [Out, MarshalAs(UnmanagedType.Interface)] out object o_obj
        );
        /// <summary>
        /// ITextStoreACP2::QueryInsertEmbedded
        /// </summary>
        /// <param name="i_guidService"></param>
        /// <param name="i_formatEtc"></param>
        /// <param name="o_insertable"></param>
        void QueryInsertEmbedded(
            [In] ref Guid i_guidService,
            [In] int i_formatEtc,
            [Out, MarshalAs(UnmanagedType.Bool)] out bool o_insertable
        );
        /// <summary>
        /// ITextStoreACP2::InsertEmbedded
        /// </summary>
        /// <param name="i_flags"></param>
        /// <param name="i_start"></param>
        /// <param name="i_end"></param>
        /// <param name="i_obj"></param>
        /// <param name="o_textChange"></param>
        void InsertEmbedded(
            [In] InsertEmbeddedFlags i_flags,
            [In] int i_start,
            [In] int i_end,
            [In, MarshalAs(UnmanagedType.Interface)] object i_obj,
            [Out] out TS_TEXTCHANGE o_textChange
        );
        /// <summary>
        /// ITextStoreACP2::InsertTextAtSelection
        /// </summary>
        /// <param name="i_flags"></param>
        /// <param name="i_text"></param>
        /// <param name="i_length"></param>
        /// <param name="o_startIndex"></param>
        /// <param name="o_endIndex"></param>
        /// <param name="o_textChange"></param>
        void InsertTextAtSelection(
            [In] InsertAtSelectionFlags i_flags,
            [In, MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 2)] char[] i_text,
            [In] int i_length,
            [Out] out int o_startIndex,
            [Out] out int o_endIndex,
            [Out] out TS_TEXTCHANGE o_textChange
        );
        /// <summary>
        /// ITextStoreACP2::InsertEmbeddedAtSelection
        /// </summary>
        /// <param name="flags"></param>
        /// <param name="obj"></param>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <param name="change"></param>
        void InsertEmbeddedAtSelection(
            InsertAtSelectionFlags flags,
            [MarshalAs(UnmanagedType.Interface)] object obj,
            out int start,
            out int end,
            out TS_TEXTCHANGE change
        );
        /// <summary>
        /// ITextStoreACP2::RequestSupportedAttrs
        /// </summary>
        /// <param name="i_flags"></param>
        /// <param name="i_length"></param>
        /// <param name="i_filterAttributes"></param>
        void RequestSupportedAttrs(
            [In] AttributeFlags i_flags,
            [In] int i_length,
            [In, MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 1)] Guid[] i_filterAttributes
        );
        /// <summary>
        /// ITextStoreACP2::RequestAttrsAtPosition
        /// </summary>
        /// <param name="i_position"></param>
        /// <param name="i_length"></param>
        /// <param name="i_filterAttributes"></param>
        /// <param name="i_flags"></param>
        void RequestAttrsAtPosition(
            [In] int i_position,
            [In] int i_length,
            [In, MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 1)] Guid[] i_filterAttributes,
            [In] AttributeFlags i_flags
        );
        /// <summary>
        /// ITextStoreACP2::RequestAttrsTransitioningAtPosition
        /// </summary>
        /// <param name="i_position"></param>
        /// <param name="i_length"></param>
        /// <param name="i_filterAttributes"></param>
        /// <param name="i_flags"></param>
        void RequestAttrsTransitioningAtPosition(
            [In] int i_position,
            [In] int i_length,
            [In, MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 1)] Guid[] i_filterAttributes,
            [In] AttributeFlags i_flags
        );
        /// <summary>
        /// ITextStoreACP2::FindNextAttrTransition
        /// </summary>
        /// <param name="i_start"></param>
        /// <param name="i_halt"></param>
        /// <param name="i_length"></param>
        /// <param name="i_filterAttributes"></param>
        /// <param name="i_flags"></param>
        /// <param name="o_nextIndex"></param>
        /// <param name="o_found"></param>
        /// <param name="o_foundOffset"></param>
        void FindNextAttrTransition(
            [In] int i_start,
            [In] int i_halt,
            [In] int i_length,
            [In, MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 2)] Guid[] i_filterAttributes,
            [In] AttributeFlags i_flags,
            [Out] out int o_nextIndex,
            [Out, MarshalAs(UnmanagedType.Bool)] out bool o_found,
            [Out] out int o_foundOffset
        );
        /// <summary>
        /// ITextStoreACP2::RetrieveRequestedAttrs
        /// </summary>
        /// <param name="i_length"></param>
        /// <param name="o_attributeVals"></param>
        /// <param name="o_fetchedLength"></param>
        void RetrieveRequestedAttrs(
            [In] int i_length,
            [Out, MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 0)] TS_ATTRVAL[] o_attributeVals,
            [Out] out int o_fetchedLength
        );
        /// <summary>
        /// ITextStoreACP2::GetEndACP
        /// </summary>
        /// <param name="o_length"></param>
        void GetEndACP(
            [Out] out int o_length
        );
        /// <summary>
        /// ITextStoreACP2::GetActiveView
        /// </summary>
        /// <param name="o_viewCookie"></param>
        void GetActiveView(
            [Out] out int o_viewCookie
        );
        /// <summary>
        /// ITextStoreACP2::GetACPFromPoint
        /// </summary>
        /// <param name="i_viewCookie"></param>
        /// <param name="i_point"></param>
        /// <param name="i_flags"></param>
        /// <param name="o_index"></param>
        void GetACPFromPoint(
            [In] int i_viewCookie,
            [In] ref POINT i_point,
            [In] GetPositionFromPointFlags i_flags,
            [Out] out int o_index
        );
        /// <summary>
        /// ITextStoreACP2::GetTextExt
        /// </summary>
        /// <param name="i_viewCookie"></param>
        /// <param name="i_startIndex"></param>
        /// <param name="i_endIndex"></param>
        /// <param name="o_rect"></param>
        /// <param name="o_isClipped"></param>
        void GetTextExt(
            [In] int i_viewCookie,
            [In] int i_startIndex,
            [In] int i_endIndex,
            [Out] out RECT o_rect,
            [MarshalAs(UnmanagedType.Bool)] out bool o_isClipped
        );
        /// <summary>
        /// ITextStoreACP2::GetScreenExt
        /// </summary>
        /// <param name="i_viewCookie"></param>
        /// <param name="o_rect"></param>
        void GetScreenExt(
            [In] int i_viewCookie,
            [Out] out RECT o_rect
        );
    }
    //============================================================================

    
    /// <summary>
    /// コンポジション関連の通知を受け取るためにアプリケーション側で実装される
    /// インターフェイス。
    /// 
    /// <para>
    ///   ITfDocumentMgr::CreateContext() が呼ばれた時に TSF マネージャは
    ///   ITextStoreACP に対してこのインターフェイスを問い合わせる。
    /// </para>
    /// </summary>
    [ComImport,
     Guid("5F20AA40-B57A-4F34-96AB-3576F377CC79"),
     InterfaceType(ComInterfaceType.InterfaceIsIUnknown)
    ]
    public interface ITfContextOwnerCompositionSink
    {
        /// <summary>
        ///   コンポジションが開始された時に TSF マネージャから呼ばれる。
        /// </summary>
        /// 
        /// <param name="i_view">
        ///   新しいコンポジションを表す ITfCompositionView へのポインタ。
        /// </param>
        /// 
        /// <param name="o_ok">
        ///   新しいコンポジションを許可するかどうかを受け取る。true の
        ///   場合はコンポジションを許可し、false の場合は許可しない。
        /// </param>
        void OnStartComposition(
            [In] ITfCompositionView                         i_view,
            [Out, MarshalAs(UnmanagedType.Bool)] out bool   o_ok
        );


        /// <summary>
        ///   既存のコンポジションが変更された時に TSF マネージャから呼ばれる。
        /// </summary>
        /// 
        /// <param name="i_view">
        ///   更新されるコンポジションを表す ITfCompositionView へのポインタ。
        /// </param>
        /// 
        /// <param name="i_rangeNew">
        ///   コンポジションが更新された後にコンポジションがカバーするテキスト
        ///   の範囲を含む ITfRange へのポインタ。
        /// </param>
        void OnUpdateComposition(
            [In] ITfCompositionView i_view,
            [In] ITfRange i_rangeNew
        );


        /// <summary>
        ///   コンポジションが終了した時に TSF マネージャから呼ばれる。
        /// </summary>
        /// <param name="i_view">
        ///   終了したコンポジションを表す ITfCompositionView へのポインタ。
        /// </param>
        void OnEndComposition(
            [In] ITfCompositionView i_view
        );
    }


//============================================================================


    /// <summary>
    /// ITextStoreACP::AdviseSink() で渡されるシンクのインターフェイス。
    /// </summary>
    [ComImport,
     Guid("22d44c94-a419-4542-a272-ae26093ececf"),
     InterfaceType(ComInterfaceType.InterfaceIsIUnknown)
    ]
    public interface ITextStoreACPSink
    {
        /// <summary>
        /// テキストが変更された時に呼び出される。
        /// </summary>
        /// <param name="i_flags">追加情報を示すフラグ。</param>
        /// <param name="i_change">テキスト変更データを含む TS_TEXTCHANGE データ。</param>
        [SecurityCritical]
        void OnTextChange(
            [In] OnTextChangeFlags  i_flags,
            [In] ref TS_TEXTCHANGE  i_change
        );


        /// <summary>
        /// ドキュメント内部で選択が変更された時に呼び出される。
        /// </summary>
        [SecurityCritical]
        void OnSelectionChange();



        /// <summary>
        /// ドキュメントのレイアウトが変更された時に呼び出される。
        /// </summary>
        /// 
        /// <param name="i_layoutCode">変更のタイプを定義する値。</param>
        /// <param name="i_viewCookie">ドキュメントを識別するアプリケーション定義のクッキー値。</param>
        [SecurityCritical]
        void OnLayoutChange(
            [In] TsLayoutCode   i_layoutCode, 
            [In] int            i_viewCookie
        );

        
        /// <summary>
        /// ステータスが変更された時に呼び出される。
        /// </summary>
        /// 
        /// <param name="i_flags">新しいステータスフラグ。</param>
        [SecurityCritical]
        void OnStatusChange(
            [In] DynamicStatusFlags i_flags
        );


        /// <summary>
        /// 一つ以上の属性が変更された時に呼び出される。
        /// </summary>
        /// 
        /// <param name="i_start">属性が変更されたテキストの開始位置。</param>
        /// <param name="i_end">属性が変更されたテキストの終了位置。</param>
        /// <param name="i_length">i_attributes パラメータの要素数。</param>
        /// <param name="i_attributes">変更された属性を識別する値。</param>
        [SecurityCritical]
        void OnAttrsChange(
            [In] int                                                        i_start,
            [In] int                                                        i_end,
            [In] int                                                        i_length,
            [In, MarshalAs(UnmanagedType.LPArray, SizeParamIndex=2)] Guid[] i_attributes
        );

        
        /// <summary>
        /// ITextStoreACP::RequestLock() でロックが成功した場合に呼び出される。
        /// </summary>
        /// 
        /// <param name="i_flags">ロックフラグ。</param>
        /// 
        /// <returns>HRESULT のエラーコード。</returns>
        [PreserveSig, SecurityCritical]
        int OnLockGranted(
            [In] LockFlags i_flags
        );


        /// <summary>
        /// エディットトランザクションが開始された時に呼び出される。
        /// 
        /// <pre>
        ///   エディットトランザクションは、一度に処理されるべきテキストの変更
        ///   のグループ。ITextStoreACPSink::OnStartEditTransaction() 呼び出し
        ///   は、テキストサービスに ITextStoreACPSink::OnEndEditTransaction()
        ///   が呼ばれるまで、テキストの変更通知をキューに入れさせることが
        ///   できる。ITextStoreACPSink::OnEndEditTransaction() が呼び出される
        ///   と、キューに入れられた変更通知はすべて処理される。
        /// </pre>
        /// 
        /// <pre>
        ///   エディットトランザクションの実装は任意。
        /// </pre>
        /// </summary>
        [SecurityCritical]
        void OnStartEditTransaction();


        /// <summary>
        /// エディットトランザクションが終了した時に呼び出される。
        /// </summary>
        /// 
        /// <pre>
        ///   エディットトランザクションは、一度に処理されるべきテキストの変更
        ///   のグループ。ITextStoreACPSink::OnStartEditTransaction() 呼び出し
        ///   は、テキストサービスに ITextStoreACPSink::OnEndEditTransaction()
        ///   が呼ばれるまで、テキストの変更通知をキューに入れさせることが
        ///   できる。ITextStoreACPSink::OnEndEditTransaction() が呼び出される
        ///   と、キューに入れられた変更通知はすべて処理される。
        /// </pre>
        /// 
        /// <pre>
        ///   エディットトランザクションの実装は任意。
        /// </pre>
        [SecurityCritical]
        void OnEndEditTransaction();
    }


//============================================================================


    [StructLayout(LayoutKind.Sequential, Pack=4),
    　Guid("E26D9E1D-691E-4F29-90D7-338DCF1F8CEF")
    ]
    public struct TF_PERSISTENT_PROPERTY_HEADER_ACP
    {
        /// <summary>
        /// プロパティを識別する GUID。
        /// </summary>
        public Guid guidOfType;
        /// <summary>
        /// プロパティの開始文字位置。
        /// </summary>
        public int  start;
        /// <summary>
        /// プロパティの文字数。
        /// </summary>
        public int  length;
        /// <summary>
        /// プロパティのバイト数。
        /// </summary>
        public uint bytes;
        /// <summary>
        /// プロパティオーナーによって定義された値。
        /// </summary>
        public uint privateValue;
        /// <summary>
        /// プロパティオーナーの CLSID。
        /// </summary>
        public Guid clsidOfTip;
    }

    
//============================================================================


    /// <summary>
    /// アプリケーション側で実装し、TSF マネージャーがドキュメントを非同期に
    /// ロードするために使用されるインターフェイス。
    /// </summary>
    [ComImport,
     InterfaceType(ComInterfaceType.InterfaceIsIUnknown), 
     Guid("4EF89150-0807-11D3-8DF0-00105A2799B5")
    ]
    public interface ITfPersistentPropertyLoaderACP
    {
        /// <summary>
        /// プロパティをロードするときに使用される。
        /// </summary>
        /// 
        /// <param name="i_propertyHeader">
        ///   ロードするプロパティを識別する TF_PERSISTENT_PROPERTY_HEADER_ACP
        ///   データ。
        /// </param>
        /// 
        /// <param name="o_stream">
        ///   ストリームオブジェクトの受け取り先。
        /// </param>
        void LoadProperty(
            [In] ref TF_PERSISTENT_PROPERTY_HEADER_ACP              i_propertyHeader,
            [Out, MarshalAs(UnmanagedType.Interface)] out IStream   o_stream
        );
    }

    
//============================================================================

    
    /// <summary>
    ///   ACP ベースのアプリケーションにいくつかの機能を提供するために TSF マ
    ///   ネージャーによって実装されるインターフェイス。
    ///   <pre>
    ///     ITextStoreACP::AdviseSink() に渡されるシンクオブジェクトに
    ///     QueryInterface() をすることで得られる。
    ///   </pre>
    /// </summary>
    [ComImport, 
     InterfaceType(ComInterfaceType.InterfaceIsIUnknown),
     Guid("AA80E901-2021-11D2-93E0-0060B067B86E")
    ]
    public interface ITextStoreACPServices
    {
        /// <summary>
        ///  ITfRange オブジェクトからプロパティを取得して、ストリームオブジェクトに
        ///  書き出す。
        /// </summary>
        void Serialize(
            [In, MarshalAs(UnmanagedType.Interface)] ITfProperty    i_property,
            [In, MarshalAs(UnmanagedType.Interface)] ITfRange       i_range,
            [Out] out TF_PERSISTENT_PROPERTY_HEADER_ACP             o_propertyHeader,
            [In, MarshalAs(UnmanagedType.Interface)] IStream        i_stream
        );


        /// <summary>
        ///  以前にシリアライズされたプロパティデータを取得してプロパティオブジェクトに
        ///  適用する。
        /// </summary>
        void Unserialize(
            [In, MarshalAs(UnmanagedType.Interface)] ITfProperty                    i_property,
            [In] ref TF_PERSISTENT_PROPERTY_HEADER_ACP                              i_propertyHeader,
            [In, MarshalAs(UnmanagedType.Interface)] IStream                        i_stream,
            [In, MarshalAs(UnmanagedType.Interface)] ITfPersistentPropertyLoaderACP i_loader
        );


        /// <summary>
        /// Forces all values of an asynchronously loaded property to be loaded.
        /// </summary>
        void ForceLoadProperty(
            [In, MarshalAs(UnmanagedType.Interface)] ITfProperty    i_property
        );


        /// <summary>
        /// 終了位置と開始位置から ITfRangeACP を生成する。
        /// </summary>
        /// <param name="i_startIndex">開始位置。</param>
        /// <param name="i_endIndex">終了位置。</param>
        /// <param name="o_range">ITfRangeACP の受け取り先。</param>
        void CreateRange(
            [In] int i_startIndex,
            [In] int i_endIndex,
            [Out, MarshalAs(UnmanagedType.Interface)] out ITfRangeACP o_range
        );
    }


}
