namespace Semver.Test.Helpers
{
    public static class OrderingExtensions
    {
        public static string ToOperator(this Ordering ordering)
        {
            if (ordering < 0) return "<";
            if (ordering == 0) return "==";
            return ">";
        }
    }
}
