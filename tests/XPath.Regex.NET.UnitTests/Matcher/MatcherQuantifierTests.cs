namespace XPath.Regex.NET.UnitTests.Matcher;

public class MatcherQuantifierTests
{
    [Fact]
    public void Greedy_TakesLongest()
    {
        var regex = XPathRegex.Compile("a*");
        RegexMatch? match = regex.Match("aaab");
        Assert.NotNull(match);
        Assert.Equal("aaa", match!.Value);
    }

    [Fact]
    public void Reluctant_TakesShortest()
    {
        // Reluctant a*? tries fewest a's, but leftmost-first starts at pos 0.
        // At pos 0, reluctant tries 0 a's → b at pos 0 fails, 1 a → b at 1 fails, etc.
        // Eventually consumes all 3 a's then matches b → "aaab".
        var regex = XPathRegex.Compile("a*?b");
        RegexMatch? match = regex.Match("aaab");
        Assert.NotNull(match);
        Assert.Equal("aaab", match!.Value);
    }

    [Fact]
    public void Reluctant_ShortestWhenPossible()
    {
        // When reluctant can produce shorter match at same start:
        // a+?  on "aaa" → matches "a" (shortest: 1 char).
        var regex = XPathRegex.Compile("a+?");
        RegexMatch? match = regex.Match("aaa");
        Assert.NotNull(match);
        Assert.Equal("a", match!.Value);
    }
}
