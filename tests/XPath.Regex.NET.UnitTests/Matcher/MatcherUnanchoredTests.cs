namespace XPath.Regex.NET.UnitTests.Matcher;

public class MatcherUnanchoredTests
{
    [Fact]
    public void PrefixSearch_FindsMatchAfterEarlierFailure()
    {
        var regex = XPathRegex.Compile("a+z");
        RegexMatch? match = regex.Match("xaaaaz");

        Assert.NotNull(match);
        Assert.Equal(1, match!.Index);
        Assert.Equal("aaaaz", match.Value);
    }

    [Fact]
    public void PrefixSearch_ScansAcrossLineBreaks()
    {
        var regex = XPathRegex.Compile("a");
        RegexMatch? match = regex.Match("x\na");

        Assert.NotNull(match);
        Assert.Equal(2, match!.Index);
    }

    [Fact]
    public void PrefixSearch_AdvancesAfterEmptyMatchAtNonOrigin()
    {
        var regex = XPathRegex.Compile("a*");
        List<RegexMatch> matches = regex.Matches("ba").ToList();

        Assert.Equal(3, matches.Count);
        Assert.Equal(string.Empty, matches[0].Value);
        Assert.Equal(0, matches[0].Index);
        Assert.Equal("a", matches[1].Value);
        Assert.Equal(1, matches[1].Index);
        Assert.Equal(string.Empty, matches[2].Value);
        Assert.Equal(2, matches[2].Index);
    }
}
