using Semver.Ranges;

namespace Semver.Test.Builders
{
    public class RangeOverlapsTestCase
    {
        public RangeOverlapsTestCase(UnbrokenSemVersionRange firstRange, UnbrokenSemVersionRange secondRange, bool overlaps)
        {
            FirstRange = firstRange;
            SecondRange = secondRange;
            Overlaps = overlaps;
        }

        public UnbrokenSemVersionRange FirstRange { get; }
        public UnbrokenSemVersionRange SecondRange { get; }
        public bool Overlaps { get; }
    }
}
