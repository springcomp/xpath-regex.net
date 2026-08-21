using XPath.Regex.NET;
using XPath.Regex.NET.Errors;

namespace XPath.Regex.NET.UnitTests.Operations;

public class AnalyzeStringTests
{
    // 1 – match in middle produces 3 regions
    [Fact]
    public void AnalyzeString_MatchInMiddle()
    {
        var rx = XPathRegex.Compile(@"(\d+)");
        AnalyzeStringResult result = rx.AnalyzeString("abc123def");

        Assert.Equal(3, result.Regions.Count);
        Assert.IsType<NonMatchRegion>(result.Regions[0]);
        Assert.Equal("abc", result.Regions[0].Value);

        var matchRegion = Assert.IsType<MatchRegion>(result.Regions[1]);
        Assert.Equal("123", matchRegion.Value);
        Assert.Single(matchRegion.Groups);
        Assert.Equal(1, matchRegion.Groups[0].Number);
        Assert.Equal("123", matchRegion.Groups[0].Value);
        Assert.True(matchRegion.Groups[0].Success);

        Assert.IsType<NonMatchRegion>(result.Regions[2]);
        Assert.Equal("def", result.Regions[2].Value);
    }

    // 2 – entire input matches → single MatchRegion
    [Fact]
    public void AnalyzeString_EntireInputMatches()
    {
        var rx = XPathRegex.Compile(@"(\d+)");
        AnalyzeStringResult result = rx.AnalyzeString("123");

        Assert.Single(result.Regions);
        var matchRegion = Assert.IsType<MatchRegion>(result.Regions[0]);
        Assert.Equal("123", matchRegion.Value);
        Assert.Equal("123", matchRegion.Groups[0].Value);
    }

    // 3 – no match → single NonMatchRegion
    [Fact]
    public void AnalyzeString_NoMatch()
    {
        var rx = XPathRegex.Compile(@"(\d+)");
        AnalyzeStringResult result = rx.AnalyzeString("abc");

        Assert.Single(result.Regions);
        Assert.IsType<NonMatchRegion>(result.Regions[0]);
        Assert.Equal("abc", result.Regions[0].Value);
    }

    // 4 – empty input → single NonMatchRegion with empty string
    [Fact]
    public void AnalyzeString_EmptyInput()
    {
        var rx = XPathRegex.Compile(@"(\d+)");
        AnalyzeStringResult result = rx.AnalyzeString("");

        Assert.Single(result.Regions);
        Assert.IsType<NonMatchRegion>(result.Regions[0]);
        Assert.Equal("", result.Regions[0].Value);
    }

    // 5 – two capturing groups side by side
    [Fact]
    public void AnalyzeString_TwoGroups()
    {
        var rx = XPathRegex.Compile("(a)(b)");
        AnalyzeStringResult result = rx.AnalyzeString("xaby");

        Assert.Equal(3, result.Regions.Count);
        Assert.Equal("x", result.Regions[0].Value);

        var matchRegion = Assert.IsType<MatchRegion>(result.Regions[1]);
        Assert.Equal("ab", matchRegion.Value);
        Assert.Equal(2, matchRegion.Groups.Count);
        Assert.Equal(1, matchRegion.Groups[0].Number);
        Assert.Equal("a", matchRegion.Groups[0].Value);
        Assert.Equal(2, matchRegion.Groups[1].Number);
        Assert.Equal("b", matchRegion.Groups[1].Value);

        Assert.Equal("y", result.Regions[2].Value);
    }

    // 6 – nested capturing groups
    [Fact]
    public void AnalyzeString_NestedGroups()
    {
        var rx = XPathRegex.Compile("(a(b)c)");
        AnalyzeStringResult result = rx.AnalyzeString("abc");

        Assert.Single(result.Regions);
        var matchRegion = Assert.IsType<MatchRegion>(result.Regions[0]);
        Assert.Equal("abc", matchRegion.Value);

        Assert.Single(matchRegion.Groups);
        CapturedGroup outer = matchRegion.Groups[0];
        Assert.Equal(1, outer.Number);
        Assert.Equal("abc", outer.Value);

        Assert.Single(outer.Groups);
        CapturedGroup inner = outer.Groups[0];
        Assert.Equal(2, inner.Number);
        Assert.Equal("b", inner.Value);
    }

    // 7 – FORX0003 when pattern can match empty
    [Fact]
    public void AnalyzeString_ThrowsForx0003_WhenCanMatchEmpty()
    {
        var rx = XPathRegex.Compile("a*");
        Assert.Throws<Forx0003Exception>(() => rx.AnalyzeString("b"));
    }

    // 8 – null input treated as empty string
    [Fact]
    public void AnalyzeString_NullInput_TreatedAsEmpty()
    {
        var rx = XPathRegex.Compile(@"\d+");
        AnalyzeStringResult result = rx.AnalyzeString(null);

        Assert.Single(result.Regions);
        Assert.IsType<NonMatchRegion>(result.Regions[0]);
        Assert.Equal("", result.Regions[0].Value);
    }
}
