namespace XPath.Regex.NET.UnitTests.Matcher;

public sealed class MatcherPrefixHintTests
{
    [Fact]
    public void SelfOverlappingPrefixKeepsMatch()
    {
        RegexMatch? match = XPathRegex.Compile("aa").Match("aaa");

        Assert.NotNull(match);
        Assert.Equal(0, match!.Index);
        Assert.Equal("aa", match.Value);
    }

    [Fact]
    public void CaseInsensitivePrefixKeepsMatch()
    {
        RegexMatch? match = XPathRegex.Compile("foobar", "i").Match("xxFOOBARyy");

        Assert.NotNull(match);
        Assert.Equal(2, match!.Index);
    }

    [Fact]
    public void PrefixAppearingMultipleTimesUsesFirstValidMatch()
    {
        RegexMatch? match = XPathRegex.Compile("foobar\\d+").Match("foobarx foobar42");

        Assert.NotNull(match);
        Assert.Equal(8, match!.Index);
        Assert.Equal("foobar42", match.Value);
    }
}
