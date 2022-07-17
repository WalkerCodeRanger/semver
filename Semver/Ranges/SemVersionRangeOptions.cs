using System;

namespace Semver.Ranges
{
    [Flags]
    internal enum SemVersionRangeOptions
    {
        Strict = 0,
        IncludeAllPrerelease = 1,
        //AllowLeadingZeros = 1 << 1,
        //AllowLowerV = 1 << 2,
        //AllowUpperV = 1 << 3,
        //AllowV = AllowLowerV | AllowUpperV,
        //Loose = AllowLeadingZeros | AllowV,
    }
}
