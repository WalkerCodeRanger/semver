using System.Collections;
using System.Collections.Generic;

namespace Semver.Comparers
{
    /// <summary>
    /// An interface that combines equality and order comparison for the <see cref="SemVersion"/>
    /// class.
    /// </summary>
    /// <remarks>
    /// This interface provides a type for the <see cref="SemVersion.PrecedenceComparer"/> and
    /// <see cref="SemVersion.SortOrderComparer"/> so that separate properties aren't needed for the
    /// <see cref="IEqualityComparer{SemVersion}"/> and <see cref="IComparer{SemVersion}"/>.
    /// </remarks>
    public interface ISemVersionComparer : IEqualityComparer<SemVersion>, IComparer<SemVersion>, IComparer
    {
    }
}
