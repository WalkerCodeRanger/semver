using System;

namespace Semver.Ranges.Parsers
{
    [Flags]
    internal enum WildcardVersion : byte
    {
        None = 0,
        MajorWildcard = 1 << 3,
        MinorWildcard = 1 << 2,
        PatchWildcard = 1 << 1,
        PrereleaseWildcard = 1 << 0,

        MinorPatchWildcard = MinorWildcard | PatchWildcard,
        MajorMinorPatchWildcard = MajorWildcard | MinorPatchWildcard,
    }
}
