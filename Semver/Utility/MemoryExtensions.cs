using System;
using System.Runtime.InteropServices;

namespace Semver.Utility
{
    internal static class MemoryExtensions
    {
        public static StringSegment ToSegment(this ReadOnlyMemory<char> chars)
        {
            if (MemoryMarshal.TryGetString(chars, out string? source, out int start, out int length))
                return new StringSegment(source, start, length);
            throw new InvalidOperationException($"Cannot convert {nameof(ReadOnlyMemory<char>)} to {nameof(StringSegment)} if not derived from a {nameof(String)}.");
        }
    }
}
