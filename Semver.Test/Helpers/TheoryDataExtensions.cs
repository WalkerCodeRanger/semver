using Semver.Utility;
using Xunit;

namespace Semver.Test.Helpers;

public static class TheoryDataExtensions
{
    public static TheoryData<T, T> AllPairs<T>(this TheoryData<T> values)
    {
        var pairs = values.ToReadOnlyList<T>().AllPairs();
        var theoryData = new TheoryData<T, T>();
        foreach (var (left, right) in pairs)
            theoryData.Add(left, right);

        return theoryData;
    }
}
