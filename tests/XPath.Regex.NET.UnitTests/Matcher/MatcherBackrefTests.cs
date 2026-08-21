namespace XPath.Regex.NET.UnitTests.Matcher;

public class MatcherBackrefTests
{
    [Fact]
    public void Backref_MatchesCapturedText()
    {
        var regex = XPathRegex.Compile("(ab)c\\1");
        RegexMatch? match = regex.Match("zzabcabzz");

        Assert.NotNull(match);
        Assert.Equal("abcab", match!.Value);
    }

    [Fact]
    public void Backref_CaseInsensitive()
    {
        var regex = XPathRegex.Compile("(ab)c\\1", "i");
        RegexMatch? match = regex.Match("zzabcABzz");

        Assert.NotNull(match);
        Assert.Equal("abcAB", match!.Value);
    }

    [Fact]
    public void Backref_MissingGroupFails()
    {
        var regex = XPathRegex.Compile("(a)?\\1b");
        RegexMatch? match = regex.Match("b");

        Assert.Null(match);
    }

    [Fact]
    public void Backref_AlternationPreservesEmptyCaptureState()
    {
        var regex = XPathRegex.Compile(@"(|())\2x");
        RegexMatch? match = regex.Match("x");

        Assert.NotNull(match);
        Assert.Equal("x", match!.Value);
    }
}
