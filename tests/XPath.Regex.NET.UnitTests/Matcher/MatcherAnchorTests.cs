namespace XPath.Regex.NET.UnitTests.Matcher;

public class MatcherAnchorTests
{
    [Theory]
    [InlineData("^abc", "abc", 0, 3)]
    [InlineData("abc$", "abc", 0, 3)]
    [InlineData("^abc$", "abc", 0, 3)]
    public void Anchors_DefaultMode(string pattern, string input, int start, int end)
    {
        var regex = XPathRegex.Compile(pattern);
        RegexMatch? match = regex.Match(input);

        Assert.NotNull(match);
        Assert.Equal(start, match!.Index);
        Assert.Equal(end - start, match.Length);
    }

    [Theory]
    [InlineData("^abc", "zzabc")]
    [InlineData("abc$", "abcz")]
    public void Anchors_NoMatchWhenNotAtBoundary(string pattern, string input)
    {
        var regex = XPathRegex.Compile(pattern);
        RegexMatch? match = regex.Match(input);

        Assert.Null(match);
    }

    [Fact]
    public void Anchors_SubstringMode_NoMatchWhenNotAtStart()
    {
        // ^ means start of input even in XPath substring mode.
        var regex = XPathRegex.Compile("^a", RegexDialect.XPath30);
        RegexMatch? match = regex.Match("ba");
        Assert.Null(match);
    }

    [Fact]
    public void Anchors_SubstringMode_MatchWhenAtStart()
    {
        var regex = XPathRegex.Compile("^a", RegexDialect.XPath30);
        RegexMatch? match = regex.Match("ab");
        Assert.NotNull(match);
        Assert.Equal(0, match!.Index);
    }

    [Fact]
    public void Anchors_Multiline()
    {
        var regex = XPathRegex.Compile("^abc$", "m", RegexDialect.XPath30);
        RegexMatch? match = regex.Match("zz\nabc\nzz");
        Assert.NotNull(match);
        Assert.Equal(3, match!.Index);
        Assert.Equal(3, match.Length);
    }
}
