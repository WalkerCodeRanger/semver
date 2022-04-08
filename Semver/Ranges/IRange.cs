namespace Semver.Ranges
{
    /// <summary>
    /// An interface for determining if a <see cref="SemVersion"/> is included in a range of versions.
    /// </summary>
    public interface IRange
    {
        /// <summary>
        /// Returns whether the specified version is included in this range.
        /// </summary>
        /// <param name="version">The version to check if it's included in this range.</param>
        /// <returns>True if the version is included in this range.</returns>
        bool Includes(SemVersion version);
    }
}