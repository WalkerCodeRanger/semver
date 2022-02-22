using System.Collections;
using System.Collections.Generic;

namespace Semver.Comparers
{
    public interface ISemVersionComparer : IEqualityComparer<SemVersion>, IComparer<SemVersion>, IComparer
    {
    }
}
