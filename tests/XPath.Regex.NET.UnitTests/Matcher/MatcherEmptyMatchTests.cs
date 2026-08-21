namespace XPath.Regex.NET.UnitTests.Matcher;

public class MatcherEmptyMatchTests
{
    [Fact]
    public void EmptyMatch_AdvancePreventsInfiniteLoop()
    {
        var regex = XPathRegex.Compile("a*");
        var matches = regex.Matches("aaa").ToList();

        // Greedy a* consumes "aaa" at pos 0, then empty match at pos 3.
        Assert.Equal(2, matches.Count);
        Assert.Equal("aaa", matches[0].Value);
        Assert.Equal(string.Empty, matches[1].Value);
        Assert.Equal(3, matches[1].Index);
    }

    [Fact]
    public void EmptyMatchAtEnd_BreaksOut()
    {
        var regex = XPathRegex.Compile("a*");
        var matches = regex.Matches("aaa");

        int lastIndex = -1;
        foreach (RegexMatch m in matches)
            lastIndex = m.Index;

        Assert.Equal(3, lastIndex);
    }
}
