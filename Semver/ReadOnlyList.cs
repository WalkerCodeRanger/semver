using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Semver
{
    /// <summary>
    /// Internal helper for efficiently creating empty read only lists
    /// </summary>
    internal static class ReadOnlyList<T>
    {
        public static readonly IReadOnlyList<T> Empty = new ReadOnlyCollection<T>(new List<T>());
    }
}
