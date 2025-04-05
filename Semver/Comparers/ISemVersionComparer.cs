using System.Collections;
using System.Collections.Generic;

namespace Semver.Comparers;

/// <summary>
/// An interface that combines equality and order comparison for the <see cref="SemVersion"/>
/// class.
/// </summary>
/// <remarks>
/// This interface provides a type for the <see cref="SemVersion.PrecedenceComparer"/> and
/// <see cref="SemVersion.SortOrderComparer"/> so that separate properties aren't needed for the
/// <see cref="IEqualityComparer{T}"/> and <see cref="IComparer{T}"/> of <see cref="SemVersion"/>.
/// Consumers of this library should not implement this interface.
/// </remarks>
public interface ISemVersionComparer : IEqualityComparer<SemVersion?>, IEqualityComparer, IComparer<SemVersion?>, IComparer
{
}
