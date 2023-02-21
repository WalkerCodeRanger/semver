namespace Semver.Test.TestCases
{
    public class RangesRelatedTestCase
    {
        public RangesRelatedTestCase(UnbrokenSemVersionRange x, UnbrokenSemVersionRange y, bool related)
        {
            X = x;
            Y = y;
            Related = related;
        }

        public UnbrokenSemVersionRange X { get; }
        public UnbrokenSemVersionRange Y { get; }
        public bool Related { get; }
    }
}
