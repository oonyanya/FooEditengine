using System;
using System.Buffers;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
#if !WINDOWS_UWP
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;
#endif
using System.Text;

namespace FooEditEngine
{
    /// <summary>
    /// 行の解析を行う
    /// </summary>
    public class LineAnalyzer
    {
        /// <summary>
        /// 改行コードを統一する
        /// </summary>
        /// <param name="s"></param>
        /// <param name="linefeed">統一先の改行コード</param>
        /// <returns></returns>
        public static string NormalizeLineFeed(DocumentSegment s, string linefeed)
        {
            StringBuilder result = new StringBuilder();
            ReadOnlySequence<char> seq;
            while (s.Read(out seq) > 0)
            {
#if !WINDOWS_UWP
                var reader = new SequenceReader<char>(seq);
#else
                var reader = new MySequenceReader<char>(seq);
#endif
                result.Append(LineAnalyzer.NormalizeLineFeed(reader, linefeed));
            }
            return result.ToString();
        }

        /// <summary>
        /// 改行コードを統一する
        /// </summary>
        /// <param name="s"></param>
        /// <param name="linefeed">統一先の改行コード</param>
        /// <returns></returns>
        public static string NormalizeLineFeed(string s, string linefeed)
        {
#if !WINDOWS_UWP
            var reader = new SequenceReader<char>(new ReadOnlySequence<char>(s.AsMemory()));
#else
            var reader = new MySequenceReader<char>(new ReadOnlySequence<char>(s.AsMemory()));
#endif
            return LineAnalyzer.NormalizeLineFeed(reader, linefeed);
        }

        /// <summary>
        /// 改行コードを統一する
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="linefeed">統一先の改行コード</param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
#if !WINDOWS_UWP
        static string NormalizeLineFeed(SequenceReader<char> reader, string linefeed)
#else
        static string NormalizeLineFeed(MySequenceReader<char> reader, string linefeed)
#endif
        {
            StringBuilder result = new StringBuilder();
            while (!reader.End)
            {
                var move_count = LineAnalyzer.GetMoveNextCountIfHasNotCRLF(reader.CurrentSpan.Slice(reader.CurrentSpanIndex));
                if (move_count > 0 && move_count < reader.Remaining)
                {
#if !WINDOWS_UWP
                    result.Append(reader.CurrentSpan.Slice(reader.CurrentSpanIndex, move_count));
#else
                    result.Append(reader.CurrentSpan.Slice(reader.CurrentSpanIndex, move_count).ToArray());
#endif
                    reader.Advance(move_count);
                    continue;
                }

                char c;
                if (!reader.TryPeek(out c))
                {
                    throw new InvalidOperationException("somthing wrong in readline");
                }

                if (c == Document.LF_CHAR)
                {
                    result.Append(linefeed);
                    reader.Advance(1);
                }
                else if (c == Document.CR_CHAR)
                {
                    char next_c;
                    bool peek_result = reader.TryPeek(1, out next_c);
                    if (peek_result && next_c == Document.LF_CHAR)
                    {
                        result.Append(linefeed);
                        reader.Advance(2);
                    }
                    else
                    {
                        result.Append(linefeed);
                        reader.Advance(1);
                    }
                }
                else
                {
                    result.Append(c);
                    reader.Advance(1);
                }
            }

            return result.ToString();
        }

        /// <summary>
        /// 行を列挙する
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public static IEnumerable<(string str, string linefeed)> EnumrateLine(DocumentSegment s)
        {
            StringBuilder result = new StringBuilder();
            ReadOnlySequence<char> seq;
            while (s.Read(out seq) > 0)
            {
                var reader = new MySequenceReader<char>(seq);
                while (!reader.End)
                {
                    var move_count = LineAnalyzer.GetMoveNextCountIfHasNotCRLF(reader.CurrentMemory.Span.Slice(reader.CurrentMemoryIndex));
                    if (move_count > 0 && move_count < reader.Remaining)
                    {
                        result.Append(reader.CurrentMemory.Slice(reader.CurrentMemoryIndex, move_count));
                        reader.Advance(move_count);
                        continue;
                    }

                    char c;
                    if (!reader.TryPeek(out c))
                    {
                        throw new InvalidOperationException("somthing wrong in readline");
                    }

                    if (c == Document.LF_CHAR)
                    {
                        yield return (result.ToString(), Document.LF_STR);
                        result.Clear();
                        reader.Advance(1);
                    }
                    else if (c == Document.CR_CHAR)
                    {
                        char next_c;
                        bool peek_result = reader.TryPeek(1, out next_c);
                        if (peek_result && next_c == Document.LF_CHAR)
                        {
                            yield return (result.ToString(), Document.CRLF_STR);
                            result.Clear();
                            reader.Advance(2);
                        }
                        else
                        {
                            yield return (result.ToString(), Document.CR_STR);
                            result.Clear();
                            reader.Advance(1);
                        }
                    }
                    else
                    {
                        result.Append(c);
                        reader.Advance(1);
                    }
                }
            }
            if (result.Length > 0)
            {
                yield return (result.ToString(), string.Empty);
                result.Clear();
            }
        }

        /// <summary>
        /// 行を列挙する
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public static IEnumerable<(string str, string linefeed)> EnumrateLine(string s)
        {
            StringBuilder result = new StringBuilder();
            var reader = new MySequenceReader<char>(new ReadOnlySequence<char>(s.AsMemory()));
            while (!reader.End)
            {
                var move_count = LineAnalyzer.GetMoveNextCountIfHasNotCRLF(reader.CurrentMemory.Span.Slice(reader.CurrentMemoryIndex));
                if (move_count > 0 && move_count < reader.Remaining)
                {
                    result.Append(reader.CurrentMemory.Slice(reader.CurrentMemoryIndex, move_count));
                    reader.Advance(move_count);
                    continue;
                }

                char c;
                if (!reader.TryPeek(out c))
                {
                    throw new InvalidOperationException("somthing wrong in readline");
                }

                if (c == Document.LF_CHAR)
                {
                    yield return (result.ToString(), Document.LF_STR);
                    result.Clear();
                    reader.Advance(1);
                }
                else if (c == Document.CR_CHAR)
                {
                    char next_c;
                    bool peek_result = reader.TryPeek(1, out next_c);
                    if (peek_result && next_c == Document.LF_CHAR)
                    {
                        yield return (result.ToString(), Document.CRLF_STR);
                        result.Clear();
                        reader.Advance(2);
                    }
                    else
                    {
                        yield return (result.ToString(), Document.CR_STR);
                        result.Clear();
                        reader.Advance(1);
                    }
                }
                else
                {
                    result.Append(c);
                    reader.Advance(1);
                }
            }

            if (result.Length > 0)
            {
                yield return (result.ToString(), string.Empty);
                result.Clear();
            }
        }

        /// <summary>
        /// 改行コードが存在するかどうか
        /// </summary>
        /// <param name="s"></param>
        /// <returns>存在するなら真を返す</returns>
        public static bool IsHasNewLine(IEnumerable<char> s)
        {
            return GetNewLineLengthInTail(s) > 0;
        }

        /// <summary>
        /// 末尾にある改行コードの長さを返す
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static int GetNewLineLengthInTail(IEnumerable<char> s)
        {
            var (len, type) = GetNewLineLengthInTailWithType(s);
            return len;
        }

        /// <summary>
        /// 末尾にある改行コードの長さを返す
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static (int linefeedlen, string linefeedtype) GetNewLineLengthInTailWithType(IEnumerable<char> s)
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
                return (0, null);
            int lastIndex = count - 2;
            if (lastIndex >= 0)
            {
                if (s.ElementAt(lastIndex) == Document.CR_CHAR)
                {
                    if (lastIndex + 1 < count && s.ElementAt(lastIndex + 1) == Document.LF_CHAR)
                        return (2, Document.CRLF_STR);
                    else
                        return (1, Document.LF_STR);
                }
            }
            lastIndex = count - 1;
            if (lastIndex >= 0)
            {
                char lastChar = s.ElementAt(lastIndex);
                if (lastChar == Document.LF_CHAR)
                    return (1, Document.LF_STR);
                if (lastChar == Document.CR_CHAR)
                    return (1, Document.CR_STR);
            }
            return (0, null);
        }

        /// <summary>
        /// 改行コードごとに分割する
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static string[] SpilitByLineFeed(string s)
        {
            List<string> result = new List<string>();
            foreach (var item in EnumrateLine(s))
            {
                result.Add(item.str);
            }
            return result.ToArray();
        }

        /// <summary>
        /// 改行コードを統一する
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
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

        long currentLineHeadIndex = 0;
        long currentLineLength = 0;
        string linefeed = string.Empty;

        public IEnumerable<DocumentLineInfo> EnumrateLines(ReadOnlySequence<char> seq, long offset, int maxCharCount = -1)
        {
            currentLineHeadIndex = offset;

            var reader = new MySequenceReader<char>(seq);
            //AVXなどサポートしているCPUかつどの文字にも改行コードがなければスキップする
            while (!reader.End)
            {
                var move_count = LineAnalyzer.GetMoveNextCountIfHasNotCRLF(reader.CurrentMemory.Span.Slice(reader.CurrentMemoryIndex));
                if (move_count > 0 && move_count < reader.Remaining)
                {
                    var left_count = maxCharCount - currentLineLength;
                    if (maxCharCount == -1 || left_count > move_count)
                    {
                        currentLineLength += move_count;
                        reader.Advance(move_count);
                        continue;
                    }
                    else
                    {
                        var clamped_move_count = 0L;
                        if (left_count < move_count)
                            clamped_move_count = left_count;
                        else
                            clamped_move_count = move_count;

                        currentLineLength += clamped_move_count;

                        if (currentLineLength > Int32.MaxValue - 1)
                            throw new InvalidOperationException("Line length is too large. It must be within Int32.MaxValue - 1");

                        yield return new DocumentLineInfo(currentLineHeadIndex, currentLineLength, linefeed);

                        currentLineHeadIndex += currentLineLength;
                        currentLineLength = 0;
                        linefeed = string.Empty;

                        reader.Advance(clamped_move_count);
                        continue;
                    }
                }

                currentLineLength++;

                char c;
                if (!reader.TryPeek(out c))
                {
                    throw new InvalidOperationException("somthing wrong in readline");
                }

                if (c == Document.LF_CHAR)
                {
                    linefeed = Document.LF_STR;
                }
                else if (c == Document.CR_CHAR)
                {
                    char next_c;
                    bool result = reader.TryPeek(1, out next_c);
                    if (result && next_c == Document.LF_CHAR)
                    {
                        currentLineLength++;
                        reader.Advance(1);
                        linefeed = Document.CRLF_STR;
                    }
                    else
                    {
                        linefeed = Document.CR_STR;
                    }
                }
                if (linefeed != string.Empty || (maxCharCount != -1 && currentLineLength >= maxCharCount))
                {
                    UnicodeCategory uc = CharUnicodeInfo.GetUnicodeCategory(c);
                    if (uc != UnicodeCategory.NonSpacingMark &&
                    uc != UnicodeCategory.SpacingCombiningMark &&
                    uc != UnicodeCategory.EnclosingMark &&
                    uc != UnicodeCategory.Surrogate)
                    {
                        if (currentLineLength > Int32.MaxValue - 1)
                            throw new InvalidOperationException("Line length is too large. It must be within Int32.MaxValue - 1");
                        yield return new DocumentLineInfo(currentLineHeadIndex, currentLineLength, linefeed);
                        currentLineHeadIndex += currentLineLength;
                        currentLineLength = 0;
                        linefeed = string.Empty;
                    }
                }
                reader.Advance(1);
            }

        }

        public DocumentLineInfo? Complate()
        {
            if (currentLineLength > 0)
                return new DocumentLineInfo(currentLineHeadIndex, currentLineLength, linefeed);

            return null;
        }
    }
}
