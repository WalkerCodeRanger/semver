using System.Collections.Generic;
using Semver.Utility;

namespace Semver.Parsing
{
    internal static class PrereleaseIdentifiers
    {
        public static readonly IReadOnlyList<PrereleaseIdentifier> Zero
            = new List<PrereleaseIdentifier>(1) { PrereleaseIdentifier.Zero }.AsReadOnly();
    }
}
