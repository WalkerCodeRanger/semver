namespace Semver.Ranges.Comparers.Npm
{
    internal enum ComparatorOp
    {
        LessThan,
        GreaterThan,
        LessThanOrEqualTo,
        GreaterThanOrEqualTo,
        Equals,
        CompatibleWith,
        ReasonablyClose
    }
}
