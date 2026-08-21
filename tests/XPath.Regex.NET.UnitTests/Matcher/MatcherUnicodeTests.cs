namespace XPath.Regex.NET.UnitTests.Matcher;

public class MatcherUnicodeTests
{
    [Fact(Skip = "Requires surrogate-aware lexer")]
    public void SurrogatePair_MatchesCodePoint()
    {
        const string emoji = "\U0001F600"; // U+1F600
        var regex = XPathRegex.Compile(emoji);
        RegexMatch? match = regex.Match("hello \U0001F600 world");

        Assert.NotNull(match);
        Assert.Equal(emoji, match!.Value);
    }

    [Fact]
    public void CaseFold_BackrefUsesSimpleFold()
    {
        // Backref with ignore-case: capture 'A', then \1 should match 'a' (simple fold partner)
        var regex = XPathRegex.Compile("([A-Z])\\1", "i");
        RegexMatch? match = regex.Match("Aa");

        Assert.NotNull(match);
        Assert.Equal("Aa", match!.Value);
    }

    [Fact]
    public void CaseInsensitive_CharRange_MatchesUppercase()
    {
        var regex = XPathRegex.Compile("[a-z]+", "i");
        Assert.True(regex.IsMatch("ABC"));
        Assert.True(regex.IsMatch("abc"));
        Assert.True(regex.IsMatch("AbC"));
    }

    [Fact]
    public void CaseInsensitive_PropertyEscape_Lu_MatchesLowercase()
    {
        var regex = XPathRegex.Compile(@"\p{Lu}+", "i");
        Assert.True(regex.IsMatch("abc"));
        Assert.True(regex.IsMatch("ABC"));
    }
}
