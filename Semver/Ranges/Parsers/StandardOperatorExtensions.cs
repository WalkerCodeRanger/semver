using System;

namespace Semver.Ranges.Parsers
{
    internal static class StandardOperatorExtensions
    {
        public static int StringLength(this StandardOperator @operator)
        {
            switch (@operator)
            {
                case StandardOperator.GreaterThanOrEqual:
                case StandardOperator.LessThanOrEqual:
                    return 2;
                case StandardOperator.Equals:
                case StandardOperator.GreaterThan:
                case StandardOperator.LessThan:
                case StandardOperator.Caret:
                case StandardOperator.Tilde:
                    return 1;
                case StandardOperator.None:
                    return 0;
                default:
                    // This code should be unreachable
                    throw new ArgumentException($"DEBUG: Invalid {nameof(StandardOperator)} value {@operator}");
            }
        }
    }
}
