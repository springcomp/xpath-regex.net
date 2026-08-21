namespace XPath.Regex.NET.UnitTests.Matcher;

public class MatcherLeftmostTests
{
    [Fact]
    public void Alternation_OrderRespected()
    {
        var regex = XPathRegex.Compile("ab|a");
        RegexMatch? match = regex.Match("ab");

        Assert.NotNull(match);
        Assert.Equal("ab", match!.Value);
    }

    [Fact]
    public void EarlyStartWins()
    {
        var regex = XPathRegex.Compile("(a|aa)b");
        RegexMatch? match = regex.Match("aab");

        Assert.NotNull(match);
        Assert.Equal("aab", match!.Value);
    }
}
