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
        private readonly string source;
        private readonly int offset;

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
            this.source = source;
            this.offset = offset;
            Length = length;
        }

        public int Length { get; }

        public bool IsEmpty => Length == 0;

        public char this[int i]
        {
            get
            {
#if DEBUG
                ValidateIndex(i, nameof(i));
#endif
                return source[offset + i];
            }
        }

        public StringSegment TrimStartSpaces()
        {
            var start = offset;
            var end = offset + Length - 1;

            while (start <= end && source[start] == ' ') start++;

            return new StringSegment(source, start, end + 1 - start);
        }

        public StringSegment TrimEndSpaces()
        {
            var end = offset + Length - 1;

            while (offset <= end && source[end] == ' ') end--;

            return new StringSegment(source, offset, end + 1 - offset);
        }

        public StringSegment Subsegment(int start, int length)
        {
#if DEBUG
            ValidateIndex(start, nameof(start));
            ValidateLength(start, length, nameof(length));
#endif
            return new StringSegment(source, offset + start, length);
        }

        public StringSegment Subsegment(int start)
        {
#if DEBUG
            ValidateIndex(start, nameof(start));
#endif
            return new StringSegment(source, offset + start, Length - start);
        }

        public int IndexOf(char value, int startIndex, int count)
        {
#if DEBUG
            ValidateIndex(startIndex, nameof(startIndex));
            ValidateLength(startIndex, count, nameof(count));
#endif
            var i = source.IndexOf(value, offset + startIndex, count);
            return i < 0 ? i : i - offset;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator StringSegment(string value)
            => new StringSegment(value, 0, value.Length);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override string ToString() => source.Substring(offset, Length);

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
