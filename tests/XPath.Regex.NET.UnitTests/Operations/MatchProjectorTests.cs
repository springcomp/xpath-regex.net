using System.Collections.Immutable;
using XPath.Regex.NET.Internal.Matcher;
using XPath.Regex.NET.Internal.Nfa;
using XPath.Regex.NET.Internal.Operations;

namespace XPath.Regex.NET.UnitTests.Operations;

public class MatchProjectorTests
{
    [Fact]
    public void ToRegexMatch_ProjectsParticipatingAndUnsetGroups()
    {
        MatchContext context = new(0, 3, [0, 3, 0, 1, -1, -1]);
        RegexMatch result = MatchProjector.ToRegexMatch("abc", context, CreateProgram([[], [2]]));

        Assert.Equal("abc", result.Value);
        Assert.Equal(3, result.Groups.Count);
        Assert.True(result.Groups[0].Success);
        Assert.Equal("abc", result.Groups[0].Value);
        Assert.True(result.Groups[1].Success);
        Assert.Equal("a", result.Groups[1].Value);
        Assert.False(result.Groups[2].Success);
        Assert.Equal(string.Empty, result.Groups[2].Value);
    }

    [Fact]
    public void ToMatchRegion_ProjectsNestedAndEmptyGroups()
    {
        MatchContext context = new(0, 3, [0, 3, 0, 3, 1, 1]);
        MatchRegion result = MatchProjector.ToMatchRegion("abc", context, CreateProgram([[1], [2], []]));

        CapturedGroup outer = Assert.Single(result.Groups);
        Assert.Equal(1, outer.Number);
        Assert.Equal("abc", outer.Value);
        CapturedGroup inner = Assert.Single(outer.Groups);
        Assert.Equal(2, inner.Number);
        Assert.True(inner.Success);
        Assert.Equal(string.Empty, inner.Value);
    }

    private static NfaProgram CreateProgram(int[][] children)
    {
        ImmutableArray<ImmutableArray<int>> groupChildren =
            children.Select(ImmutableArray.CreateRange).ToImmutableArray();
        return new NfaProgram([], 2, false, 0, null, groupChildren);
    }
}
