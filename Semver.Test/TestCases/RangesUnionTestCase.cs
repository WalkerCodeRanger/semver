using System;

namespace Semver.Test.TestCases;

public class RangesUnionTestCase
{
    public RangesUnionTestCase(
        UnbrokenSemVersionRange x,
        UnbrokenSemVersionRange y,
        UnbrokenSemVersionRange? expected = null)
    {
        X = x ?? throw new ArgumentNullException(nameof(x));
        Y = y ?? throw new ArgumentNullException(nameof(y));
        Expected = expected;
    }

    public UnbrokenSemVersionRange X { get; }
    public UnbrokenSemVersionRange Y { get; }
    public UnbrokenSemVersionRange? Expected { get; }
}
