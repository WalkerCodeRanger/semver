namespace Semver.Ranges
{
    internal enum StandardOperator
    {
        None = 1,
        Equals,
        LessThan,
        LessThanOrEqual,
        GreaterThan,
        GreaterThanOrEqual,
        Tilde,
        Caret,
    }
}
