using XPath.Regex.NET;
using XPath.Regex.NET.Errors;

namespace XPath.Regex.NET.UnitTests.Operations;

public class ReplaceTests
{
    // 1 – group back-reference double substitution
    [Fact]
    public void Replace_GroupDoubleRef()
    {
        var rx = XPathRegex.Compile(@"(\w+)");
        Assert.Equal("foo-foo bar-bar", rx.Replace("foo bar", "$1-$1"));
    }

    // 2 – alternation group, one branch not participating
    [Fact]
    public void Replace_NonParticipatingGroup()
    {
        var rx = XPathRegex.Compile(@"(ab)|(a)");
        Assert.Equal("[1=ab][2=]cd", rx.Replace("abcd", "[1=$1][2=$2]"));
    }

    // 3 – simple literal replacement
    [Fact]
    public void Replace_Literal()
    {
        var rx = XPathRegex.Compile("a");
        Assert.Equal("bbb", rx.Replace("aaa", "b"));
    }

    // 4 – FORX0003 when pattern can match empty
    [Fact]
    public void Replace_ThrowsForx0003_WhenCanMatchEmpty()
    {
        var rx = XPathRegex.Compile("a*");
        Assert.Throws<Forx0003Exception>(() => rx.Replace("b", "X"));
    }

    // 5 – escaped dollar in replacement → literal $
    [Fact]
    public void Replace_EscapedDollarInReplacement()
    {
        var rx = XPathRegex.Compile(@"\$");
        Assert.Equal("cost dollar5", rx.Replace("cost $5", "dollar"));
    }

    // 6 – \$0 in replacement = literal '$' then '0'
    [Fact]
    public void Replace_EscapedDollarZero()
    {
        var rx = XPathRegex.Compile(@"\d+");
        Assert.Equal("abc$0", rx.Replace("abc123", @"\$0"));
    }

    // 7 – escaped backslash in replacement → literal '\'
    [Fact]
    public void Replace_EscapedBackslash()
    {
        var rx = XPathRegex.Compile(@"\d+");
        Assert.Equal(@"abc\", rx.Replace("abc123", @"\\"));
    }

    // 8 – group ref in brackets
    [Fact]
    public void Replace_GroupRefInBrackets()
    {
        var rx = XPathRegex.Compile(@"(\d+)");
        Assert.Equal("a[1]b[2]", rx.Replace("a1b2", "[$1]"));
    }

    // 9 – back-reference with same-group open/close quote
    [Fact]
    public void Replace_BackRefSameQuote()
    {
        var rx = XPathRegex.Compile(@"(['""])(.*?)\1");
        Assert.Equal("(hello)", rx.Replace("'hello'", "($2)"));
    }

    // 10 – group ref N > captureCount and N > 9: resolve to smaller group
    [Fact]
    public void Replace_GroupRef_ExceedsCapture_ResolvesSmaller()
    {
        // Pattern has 2 groups. $3 > 2 and ≤ 9 → empty.
        var rx = XPathRegex.Compile("(a)(b)");
        Assert.Equal("", rx.Replace("ab", "$3"));
    }

    // 11 – bare $ → FORX0004
    [Fact]
    public void Replace_BareDollar_ThrowsForx0004()
    {
        var rx = XPathRegex.Compile("(a)");
        Assert.Throws<Forx0004Exception>(() => rx.Replace("a", "$"));
    }

    // 12 – no match → return input unchanged
    [Fact]
    public void Replace_NoMatch_ReturnsInputUnchanged()
    {
        var rx = XPathRegex.Compile("z");
        Assert.Equal("abc", rx.Replace("abc", "X"));
    }

    // 13 – match at start → empty prefix
    [Fact]
    public void Replace_MatchAtStart()
    {
        var rx = XPathRegex.Compile("a");
        Assert.Equal("Xbc", rx.Replace("abc", "X"));
    }

    // 14 – match at end → empty suffix
    [Fact]
    public void Replace_MatchAtEnd()
    {
        var rx = XPathRegex.Compile("c");
        Assert.Equal("abX", rx.Replace("abc", "X"));
    }

    // 15 – null input normalized to ""
    [Fact]
    public void Replace_NullInput_TreatedAsEmpty()
    {
        var rx = XPathRegex.Compile("a");
        Assert.Equal("", rx.Replace(null, "X"));
    }
}
