using System;

namespace Semver.Ranges
{
    // TODO make these match SemVersionStyles values?
    [Flags]
    internal enum SemVersionRangeOptions
    {
        Strict = 0,
        IncludeAllPrerelease = 1,
        AllowLeadingZeros = 1 << 1,
        AllowLowerV = 1 << 2,
        AllowUpperV = 1 << 3,
        AllowV = AllowLowerV | AllowUpperV,
        AllowMetadata = 1 << 4,
        Loose = AllowLeadingZeros | AllowV | AllowMetadata,
    }
}
