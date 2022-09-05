using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Semver.Utility
{
    /// <summary>
    /// An efficient representation of a section of a string
    /// </summary>
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
            var end = Offset + Length - 1;

            while (start <= end && Source[start] == ' ') start++;

            return new StringSegment(Source, start, end + 1 - start);
        }

        public StringSegment TrimEndSpaces()
        {
            var end = Offset + Length - 1;

            while (Offset <= end && Source[end] == ' ') end--;

            return new StringSegment(Source, Offset, end + 1 - Offset);
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

        public int IndexOf(char value, int startIndex, int count)
        {
#if DEBUG
            ValidateIndex(startIndex, nameof(startIndex));
            ValidateLength(startIndex, count, nameof(count));
#endif
            var i = Source.IndexOf(value, Offset + startIndex, count);
            return i < 0 ? i : i - Offset;
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

        private const int DisplayLimit = 100;

#if DEBUG
        private void ValidateIndex(int i, string paramName)
        {
            if (i < 0) throw new ArgumentOutOfRangeException(paramName, i, "DEBUG: Cannot be negative.");
            if (i > Length) throw new ArgumentOutOfRangeException(paramName, i, $"DEBUG: Cannot be > length {Length}.");
        }

        private void ValidateLength(int start, int length, string paramName)
        {
            if (length < 0) throw new ArgumentOutOfRangeException(paramName, length, "DEBUG: Cannot be negative.");
            if (start + length > Length) throw new ArgumentOutOfRangeException(paramName, length,
                $"DEBUG: When added to offset of {start}, must be <= length of {Length}.");
        }
#endif
    }
}
