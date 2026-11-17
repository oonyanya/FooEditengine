/*
 * https://github.com/dotnet/dotnet/blob/release/11.0.1xx/src/runtime/src/libraries/System.Memory/src/System/Buffers/SequenceReader.cs
 *  から必要な部分だけ移植した。
 *  SequenceReaderの制約を取り除いた。
 *  移植元　The .NET Foundation
 */
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;

namespace Core
{
    internal class MySequenceReader<T>
    {
        private SequencePosition _currentPosition;
        private SequencePosition _nextPosition;
        private bool _moreData;
        private readonly long _length;

        /// <summary>
        /// True when there is no more data in the <see cref="Sequence"/>.
        /// </summary>
        public bool End => !_moreData;
        
        /// <summary>
        /// The underlying <see cref="ReadOnlySequence{T}"/> for the reader.
        /// </summary>
        public ReadOnlySequence<T> Sequence { get; }

        /// <summary>
        /// The current segment in the <see cref="Sequence"/> as a span.
        /// </summary>
        public ReadOnlyMemory<T> CurrentMemory { get; private set; }

        /// <summary>
        /// The index in the <see cref="CurrentMemory"/>.
        /// </summary>
        public int CurrentMemoryIndex { get; private set; }

        /// <summary>
        /// The total number of <typeparamref name="T"/>'s processed by the reader.
        /// </summary>
        public long Consumed { get; private set; }

        /// <summary>
        /// Remaining <typeparamref name="T"/>'s in the reader's <see cref="Sequence"/>.
        /// </summary>
        public long Remaining => Length - Consumed;

        /// <summary>
        /// Count of <typeparamref name="T"/> in the reader's <see cref="Sequence"/>.
        /// </summary>
        public long Length
        {
            get
            {
                if (_length < 0)
                {
                    // Cast-away readonly to initialize lazy field
                    Unsafe.AsRef(in _length) = Sequence.Length;
                }
                return _length;
            }
        }

        public MySequenceReader(ReadOnlySequence<T> sequence)
        {
            CurrentMemoryIndex = 0;
            Consumed = 0;
            Sequence = sequence;
            _currentPosition = sequence.Start;
            _length = -1;

            /*
             * 元々は
             * sequence.GetFirstSpan(out ReadOnlySpan<T> first, out _nextPosition);
             * となっていた。
             * ただ、こいつはinternalなので、
             * https://github.com/dotnet/dotnet/blob/release/11.0.1xx/src/runtime/src/libraries/System.Memory/src/System/Buffers/ReadOnlySequence.Helpers.cs
             * にあるGetFirstSpan()と同じことをする
             */
            CurrentMemory = sequence.First;
            if(!sequence.IsSingleSegment)
            {
                var currentSegment = (ReadOnlySequenceSegment<T>)_currentPosition.GetObject();
                _nextPosition = new SequencePosition(currentSegment.Next, (int)currentSegment.RunningIndex);
            }
            _moreData = CurrentMemory.Length > 0;

            if (!_moreData && !sequence.IsSingleSegment)
            {
                _moreData = true;
                GetNextMemory();
            }
        }

        /// <summary>
        /// Peeks at the next value without advancing the reader.
        /// </summary>
        /// <param name="value">The next value or default if at the end.</param>
        /// <returns>False if at the end of the reader.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryPeek(out T value)
        {
            if (_moreData)
            {
                value = CurrentMemory.Span[CurrentMemoryIndex];
                return true;
            }
            else
            {
                value = default;
                return false;
            }
        }

        /// <summary>
        /// Peeks at the next value at specific offset without advancing the reader.
        /// </summary>
        /// <param name="offset">The offset from current position.</param>
        /// <param name="value">The next value, or the default value if at the end of the reader.</param>
        /// <returns><c>true</c> if the reader is not at its end and the peek operation succeeded; <c>false</c> if at the end of the reader.</returns>
        public bool TryPeek(long offset, out T value)
        {
            if (offset < 0)
                throw new ArgumentOutOfRangeException("offset");

            // If we've got data and offset is not out of bounds
            if (!_moreData || Remaining <= offset)
            {
                value = default;
                return false;
            }

            // Sum CurrentSpanIndex + offset could overflow as is but the value of offset should be very large
            // because we check Remaining <= offset above so to overflow we should have a ReadOnlySequence close to 8 exabytes
            Debug.Assert(CurrentMemoryIndex + offset >= 0);

            // If offset doesn't fall inside current segment move to next until we find correct one
            if ((CurrentMemoryIndex + offset) <= CurrentMemory.Length - 1)
            {
                Debug.Assert(offset <= int.MaxValue);

                value = CurrentMemory.Span[CurrentMemoryIndex + (int)offset];
                return true;
            }
            else
            {
                long remainingOffset = offset - (CurrentMemory.Length - CurrentMemoryIndex);
                SequencePosition nextPosition = _nextPosition;
                ReadOnlyMemory<T> currentMemory;

                while (Sequence.TryGet(ref nextPosition, out currentMemory, advance: true))
                {
                    // Skip empty segment
                    if (currentMemory.Length > 0)
                    {
                        if (remainingOffset >= currentMemory.Length)
                        {
                            // Subtract current non consumed data
                            remainingOffset -= currentMemory.Length;
                        }
                        else
                        {
                            break;
                        }
                    }
                }

                value = currentMemory.Span[(int)remainingOffset];
                return true;
            }
        }

        /// <summary>
        /// Read the next value and advance the reader.
        /// </summary>
        /// <param name="value">The next value or default if at the end.</param>
        /// <returns>False if at the end of the reader.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryRead(out T value)
        {
            if (End)
            {
                value = default;
                return false;
            }

            value = CurrentMemory.Span[CurrentMemoryIndex];
            CurrentMemoryIndex++;
            Consumed++;

            if (CurrentMemoryIndex >= CurrentMemory.Length)
            {
                GetNextMemory();
            }

            return true;
        }

        /// Get the next segment with available data, if any.
        /// </summary>
        private void GetNextMemory()
        {
            if (!Sequence.IsSingleSegment)
            {
                SequencePosition previousNextPosition = _nextPosition;
                while (Sequence.TryGet(ref _nextPosition, out ReadOnlyMemory<T> memory, advance: true))
                {
                    _currentPosition = previousNextPosition;
                    if (memory.Length > 0)
                    {
                        CurrentMemory = memory;
                        CurrentMemoryIndex = 0;
                        return;
                    }
                    else
                    {
                        CurrentMemory = default;
                        CurrentMemoryIndex = 0;
                        previousNextPosition = _nextPosition;
                    }
                }
            }
            _moreData = false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Advance(long count)
        {
            const long TooBigOrNegative = unchecked((long)0xFFFFFFFF80000000);
            if ((count & TooBigOrNegative) == 0 && CurrentMemory.Length - CurrentMemoryIndex > (int)count)
            {
                CurrentMemoryIndex += (int)count;
                Consumed += count;
            }
            else
            {
                // Can't satisfy from the current span
                AdvanceToNextSpan(count);
            }
        }

        private void AdvanceToNextSpan(long count)
        {
            if (count < 0)
            {
                throw new ArgumentOutOfRangeException("count");
            }

            Consumed += count;
            while (_moreData)
            {
                int remaining = CurrentMemory.Length - CurrentMemoryIndex;

                if (remaining > count)
                {
                    CurrentMemoryIndex += (int)count;
                    count = 0;
                    break;
                }

                // As there may not be any further segments we need to
                // push the current index to the end of the span.
                CurrentMemoryIndex += remaining;
                count -= remaining;
                Debug.Assert(count >= 0);

                GetNextMemory();

                if (count == 0)
                {
                    break;
                }
            }

            if (count != 0)
            {
                // Not enough data left- adjust for where we actually ended and throw
                Consumed -= count;
                throw new ArgumentOutOfRangeException("count");
            }
        }
    }
}
