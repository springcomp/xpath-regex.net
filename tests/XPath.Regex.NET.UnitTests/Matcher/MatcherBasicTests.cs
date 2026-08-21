using System.Diagnostics.CodeAnalysis;
using XPath.Regex.NET.Internal.Matcher;

namespace XPath.Regex.NET.UnitTests.Matcher;

[SuppressMessage("Design", "CA1062:Validate arguments of public methods", Justification = "Test data non-null")]
public class MatcherBasicTests
{
    [Theory]
    [InlineData("abc", "abc", 0, 3)]
    [InlineData("abc", "xabcx", 1, 4)]
    [InlineData("a.c", "xabc", 1, 4)]
    [InlineData("a|bc", "zzbczz", 2, 4)]
    public void Match_FindsExpectedSpan(string pattern, string input, int start, int end)
    {
        var regex = XPathRegex.Compile(pattern);
        RegexMatch? match = regex.Match(input);

        Assert.NotNull(match);
        Assert.Equal(start, match!.Index);
        Assert.Equal(end - start, match.Length);
        Assert.Equal(input.Substring(start, end - start), match.Value);
    }

    [Theory]
    [InlineData("abc", "zzz")]
    [InlineData("a.c", "abxc")]
    public void Match_ReturnsNullWhenNoMatch(string pattern, string input)
    {
        var regex = XPathRegex.Compile(pattern);
        RegexMatch? match = regex.Match(input);

        Assert.Null(match);
    }
}
