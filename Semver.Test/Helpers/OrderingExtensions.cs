namespace Semver.Test.Helpers;

public static class OrderingExtensions
{
    public static string ToOperator(this Ordering ordering)
        => ordering switch
        {
            < 0 => "<",
            0 => "==",
            _ => ">"
        };
}
