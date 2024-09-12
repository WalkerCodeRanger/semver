using System;

namespace Semver.Test.Helpers;

public static class RandomExtensions
{
    public static bool NextBool(this Random random)
        // Next() returns an int in the range 0 to Int32.MaxValue
        => random.Next() > (int.MaxValue / 2);
}
