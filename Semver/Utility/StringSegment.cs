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
            if (offset > source.Length)
                throw new ArgumentOutOfRangeException(nameof(offset), offset,
                    $"Must be <= length of {length}. String:\r\n{source}");

            if (offset + length > source.Length)
                throw new ArgumentOutOfRangeException(nameof(length), length,
                    $"When added to offset of {offset}, must be <= length of {length}. String:\r\n{source}");
#endif
            this.source = source;
            this.offset = offset;
            Length = length;
        }

        public int Length { get; }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator StringSegment(string value)
            => new StringSegment(value, 0, value.Length);
    }
}
