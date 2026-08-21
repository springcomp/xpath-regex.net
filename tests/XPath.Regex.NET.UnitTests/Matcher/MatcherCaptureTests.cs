namespace XPath.Regex.NET.UnitTests.Matcher;

public class MatcherCaptureTests
{
    [Fact]
    public void Captures_AssignedCorrectly()
    {
        var regex = XPathRegex.Compile("(a(bc))d");
        RegexMatch? match = regex.Match("xxabcdyy");

        Assert.NotNull(match);
        Assert.Equal("abcd", match!.Value);
        Assert.Equal("abcd", match.Groups[0].Value);
        Assert.Equal("abc", match.Groups[1].Value);
        Assert.Equal("bc", match.Groups[2].Value);
    }

    [Fact]
    public void NonParticipatingGroup_MarkedFail()
    {
        var regex = XPathRegex.Compile("(a)?b");
        RegexMatch? match = regex.Match("b");

        Assert.NotNull(match);
        Assert.False(match!.Groups[1].Success);
        Assert.Equal(string.Empty, match.Groups[1].Value);
    }
}
