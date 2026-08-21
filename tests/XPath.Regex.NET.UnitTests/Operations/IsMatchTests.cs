using XPath.Regex.NET;

namespace XPath.Regex.NET.UnitTests.Operations;

public class IsMatchTests
{
    // 1 – substring match in XPath 3.0+ (default)
    [Fact]
    public void IsMatch_SubstringMatch_XPath30Plus()
    {
        var rx = XPathRegex.Compile("b+");
        Assert.True(rx.IsMatch("abbc"));
    }

    // 2 – no match
    [Fact]
    public void IsMatch_NoMatch_XPath30Plus()
    {
        var rx = XPathRegex.Compile("b+");
        Assert.False(rx.IsMatch("axxc"));
    }

    // 3 – XSD: full-string match required
    [Fact]
    public void IsMatch_FullString_Xsd_NoMatch()
    {
        var rx = XPathRegex.Compile("b+", RegexDialect.Xsd);
        Assert.False(rx.IsMatch("abbc"));
    }

    // 4 – XSD: full-string match with .*
    [Fact]
    public void IsMatch_FullString_Xsd_Match()
    {
        var rx = XPathRegex.Compile(".*b+.*", RegexDialect.Xsd);
        Assert.True(rx.IsMatch("abbc"));
    }

    // 5 – case-insensitive
    [Fact]
    public void IsMatch_CaseInsensitive()
    {
        var rx = XPathRegex.Compile("hello", "i");
        Assert.True(rx.IsMatch("HELLO"));
    }

    // 6 – dot does not match newline by default
    [Fact]
    public void IsMatch_DotNoNewline()
    {
        var rx = XPathRegex.Compile("a.b");
        Assert.False(rx.IsMatch("a\nb"));
    }

    // 7 – dot-all flag: dot matches newline
    [Fact]
    public void IsMatch_DotAll()
    {
        var rx = XPathRegex.Compile("a.b", "s");
        Assert.True(rx.IsMatch("a\nb"));
    }

    // 8 – NEL (U+0085) is NOT excluded by dot
    [Fact]
    public void IsMatch_DotMatchesNel()
    {
        var rx = XPathRegex.Compile("a.b");
        Assert.True(rx.IsMatch("a\u0085b")); // NEL = U+0085, 3 chars
    }

    // 9 – multi-line anchor
    [Fact]
    public void IsMatch_MultiLine_Anchor()
    {
        var rx = XPathRegex.Compile("^line2$", "m");
        Assert.True(rx.IsMatch("line1\nline2\nline3"));
    }

    // 10 – CR is NOT a multi-line anchor
    [Fact]
    public void IsMatch_MultiLine_CrNotAnchor()
    {
        var rx = XPathRegex.Compile("^line2$", "m");
        Assert.False(rx.IsMatch("line1\rline2"));
    }

    // 11 – q flag: dot is literal
    [Fact]
    public void IsMatch_LiteralFlag_DotLiteral()
    {
        var rx = XPathRegex.Compile("a.b", "q");
        Assert.True(rx.IsMatch("a.b"));
    }

    // 12 – q flag: dot does not match 'x'
    [Fact]
    public void IsMatch_LiteralFlag_DotNotMeta()
    {
        var rx = XPathRegex.Compile("a.b", "q");
        Assert.False(rx.IsMatch("axb"));
    }
}
