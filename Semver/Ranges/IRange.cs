namespace Semver.Ranges
{
    /// <summary>
    /// An interface for determining if a <see cref="SemVersion"/> is included in a range of versions.
    /// </summary>
    public interface IRange
    {
        /// <summary>
        /// Returns whether this range contains the specified version.
        /// </summary>
        /// <param name="version">The version to check if it's contained within this range.</param>
        /// <returns>True if this range contains the specified version.</returns>
        bool Contains(SemVersion version);
    }
}