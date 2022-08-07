namespace Semver.Ranges.Parsers
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
