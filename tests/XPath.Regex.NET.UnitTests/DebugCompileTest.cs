namespace XPath.Regex.NET.UnitTests;

public class DebugCompileTest
{
    [Fact]
    public void BackrefGreedyMatch()
    {
        // Test greedy quantifier with backref
        var regex = XPathRegex.Compile(@"(a+)b\1");
        var match = regex.Match("aabaa");

        // Expected: 'aabaa' because (a+) should match 'aa' greedily, then 'b', then \1 matches 'aa'
        // But currently gets 'aba' because (a+) only matches 'a'
        Assert.NotNull(match);
        Assert.Equal("aabaa", match.Value);
    }

    [Fact]
    public void NestedBackrefGreedyMatch()
    {
        var regex = XPathRegex.Compile(@"((a+)b)\1");
        var match = regex.Match("aabaabaab");

        // Expected: 'aabaab' because ((a+)b) = 'aabaab' (group 1), then \1 matches again
        Assert.NotNull(match);
        Assert.Equal("aabaab", match.Value);
    }
}
