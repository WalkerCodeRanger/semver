using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using SysStringSegment = Microsoft.Extensions.Primitives.StringSegment;

namespace Semver.Utility
{
    /// <summary>
    /// An efficient representation of a section of a string
    /// </summary>
    // TODO switch to Microsoft.Extensions.Primitives.StringSegment
    [StructLayout(LayoutKind.Auto)]
    internal readonly struct StringSegment
    {
        public StringSegment(string source, int offset, int length)
        {
#if DEBUG
            if (source is null) throw new ArgumentNullException(nameof(source), "DEBUG: Value cannot be null.");
            if (offset < 0) throw new ArgumentOutOfRangeException(nameof(offset), offset, "DEBUG: Cannot be negative.");
            if (offset > source.Length)
                throw new ArgumentOutOfRangeException(nameof(offset), offset,
                    $"DEBUG: Must be <= length of {length}. String:\r\n{source}");

            if (length < 0) throw new ArgumentOutOfRangeException(nameof(length), length, "DEBUG: Cannot be negative.");
            if (offset + length > source.Length)
                throw new ArgumentOutOfRangeException(nameof(length), length,
                    $"DEBUG: When added to offset of {offset}, must be <= length of {length}. String:\r\n{source}");
#endif
            Source = source;
            Offset = offset;
            Length = length;
        }

        public readonly string Source;
        public readonly int Offset;
        public readonly int Length;

        public bool IsEmpty => Length == 0;

        public char this[int i]
        {
            get
            {
#if DEBUG
                ValidateIndex(i, nameof(i));
#endif
                return Source[Offset + i];
            }
        }

        public StringSegment TrimStartSpaces()
        {
            var start = Offset;
            var end = start + Length - 1;

            while (start <= end && Source[start] == ' ') start++;

            return new StringSegment(Source, start, end + 1 - start);
        }

        public StringSegment TrimStartWhitespace()
        {
            var start = Offset;
            var end = start + Length - 1;

            while (start <= end && char.IsWhiteSpace(Source[start])) start++;

            return new StringSegment(Source, start, end + 1 - start);
        }

        public StringSegment TrimEndWhitespace()
        {
            var end = Offset + Length - 1;

            while (Offset <= end && char.IsWhiteSpace(Source[end])) end--;

            return new StringSegment(Source, Offset, end + 1 - Offset);
        }

        /// <summary>
        /// Trim leading zeros from a numeric string segment. If the segment consists of all zeros,
        /// return <c>"0"</c>.
        /// </summary>
        /// <remarks>The standard <see cref="string.TrimStart(char[])"/> method handles all zeros
        /// by returning <c>""</c>. This efficiently handles the kind of trimming needed.</remarks>
        public StringSegment TrimLeadingZeros()
        {
            int start = Offset;
            var end = start + Length - 1;
            for (; start < end; start++)
                if (Source[start] != '0')
                    break;

            return new StringSegment(Source, start, end + 1 - start);
        }

        public StringSegment Subsegment(int start, int length)
        {
#if DEBUG
            ValidateIndex(start, nameof(start));
            ValidateLength(start, length, nameof(length));
#endif
            return new StringSegment(Source, Offset + start, length);
        }

        public StringSegment Subsegment(int start)
        {
#if DEBUG
            ValidateIndex(start, nameof(start));
#endif
            return new StringSegment(Source, Offset + start, Length - start);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public StringSegment EmptySubsegment() => new(Source, Offset, 0);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int IndexOf(char value)
        {
            var i = Source.IndexOf(value, Offset, Length);
            return i < 0 ? i : i - Offset;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int IndexOf(char value, int startIndex)
        {
#if DEBUG
            ValidateIndex(startIndex, nameof(startIndex));
#endif
            var i = Source.IndexOf(value, Offset + startIndex, Length - startIndex);
            return i < 0 ? i : i - Offset;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int IndexOf(char value, int startIndex, int count)
        {
#if DEBUG
            ValidateIndex(startIndex, nameof(startIndex));
            ValidateLength(startIndex, count, nameof(count));
#endif
            var i = Source.IndexOf(value, Offset + startIndex, count);
            return i < 0 ? i : i - Offset;
        }

        public int SplitCount(char c)
        {
            int count = 1; // Always one more item than there are separators
            var end = Offset + Length;
            // Use `for` instead of `foreach` to ensure performance
            for (int i = Offset; i < end; i++)
                if (Source[i] == c)
                    count++;

            return count;
        }

        public IEnumerable<StringSegment> Split(char c)
        {
            var start = Offset;
            var end = start + Length;
            // Use `for` instead of `foreach` to ensure performance
            for (int i = start; i < end; i++)
                if (Source[i] == c)
                {
                    yield return Subsegment(start - Offset, i - start);
                    start = i + 1;
                }

            // The final segment from the last separator to the end of the string
            yield return Subsegment(start - Offset);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SplitBeforeFirst(char c, out StringSegment left, out StringSegment right)
        {
            var index = IndexOf(c);
            var self = this; // make a copy of this in case assigning to left or right modifies it
            if (index >= 0)
            {
                left = self.Subsegment(0, index);
                right = self.Subsegment(index);
            }
            else
            {
                left = self;
                right = self.EmptySubsegment();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator StringSegment(string value)
            => new StringSegment(value, 0, value.Length);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override string ToString() => Source.Substring(Offset, Length);

        public string ToStringLimitLength()
        {
            if (Length > DisplayLimit) return Subsegment(0, DisplayLimit - 3) + "...";

            return ToString();
        }

        public static implicit operator SysStringSegment(StringSegment segment)
            => new SysStringSegment(segment.Source, segment.Offset, segment.Length);

        public static explicit operator StringSegment(SysStringSegment segment)
            => new StringSegment(segment.Buffer!, segment.Offset, segment.Length);

        private const int DisplayLimit = 100;

#if DEBUG
        [ExcludeFromCodeCoverage]
        private void ValidateIndex(int i, string paramName)
        {
            if (i < 0) throw new ArgumentOutOfRangeException(paramName, i, "DEBUG: Cannot be negative.");
            if (i > Length) throw new ArgumentOutOfRangeException(paramName, i, $"DEBUG: Cannot be > length {Length}.");
        }

        [ExcludeFromCodeCoverage]
        private void ValidateLength(int start, int length, string paramName)
        {
            if (length < 0) throw new ArgumentOutOfRangeException(paramName, length, "DEBUG: Cannot be negative.");
            if (start + length > Length) throw new ArgumentOutOfRangeException(paramName, length,
                $"DEBUG: When added to offset of {start}, must be <= length of {Length}.");
        }
#endif
    }
}
