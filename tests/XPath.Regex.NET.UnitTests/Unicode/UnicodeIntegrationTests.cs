using XPath.Regex.NET.Errors;

namespace XPath.Regex.NET.UnitTests.Unicode;

public class UnicodeIntegrationTests
{
    // === \p{Category} ===

    [Fact]
    public void PropertyEscape_Lu_MatchesUppercase()
    {
        var regex = XPathRegex.Compile(@"\p{Lu}+");
        Assert.True(regex.IsMatch("ABC"));
        Assert.NotNull(regex.Match("hello WORLD"));
        Assert.Equal("WORLD", regex.Match("hello WORLD")!.Value);
    }

    [Fact]
    public void PropertyEscape_Ll_MatchesLowercase()
    {
        var regex = XPathRegex.Compile(@"\p{Ll}+");
        Assert.True(regex.IsMatch("abc"));
        Assert.NotNull(regex.Match("HELLO world"));
        Assert.Equal("world", regex.Match("HELLO world")!.Value);
    }

    [Fact]
    public void PropertyEscape_Nd_MatchesDigits()
    {
        var regex = XPathRegex.Compile(@"\p{Nd}+");
        Assert.Equal("42", regex.Match("abc42def")!.Value);
    }

    [Fact]
    public void PropertyEscape_L_MatchesAllLetters()
    {
        var regex = XPathRegex.Compile(@"\p{L}+");
        Assert.Equal("Hello", regex.Match("Hello 123")!.Value);
    }

    // === \P{Category} (complement) ===

    [Fact]
    public void ComplementEscape_P_Lu_MatchesNonUppercase()
    {
        var regex = XPathRegex.Compile(@"\P{Lu}+");
        var match = regex.Match("ABCdef");
        Assert.NotNull(match);
        Assert.Equal("def", match!.Value);
    }

    // === Block escapes ===

    [Fact]
    public void PropertyEscape_IsBasicLatin_MatchesAscii()
    {
        var regex = XPathRegex.Compile(@"\p{IsBasicLatin}+");
        Assert.True(regex.IsMatch("Hello"));
    }

    // === In char class ===

    [Fact]
    public void PropertyEscapeInCharClass_Works()
    {
        var regex = XPathRegex.Compile(@"[\p{Lu}\p{Nd}]+");
        Assert.Equal("A1B2", regex.Match("A1B2 cd")!.Value);
    }

    [Fact]
    public void PropertyEscapeWithSubtraction_Works()
    {
        // All letters except vowels (uppercase)
        var regex = XPathRegex.Compile(@"[\p{Lu}-[AEIOU]]+");
        var match = regex.Match("HELLO");
        Assert.NotNull(match);
        Assert.Equal("H", match!.Value);
    }

    // === XSD dialect ===

    [Fact]
    public void Xsd_PropertyEscape_FullMatch()
    {
        // XSD is full-string anchored
        var regex = XPathRegex.Compile(@"\p{Lu}+", RegexDialect.Xsd);
        Assert.True(regex.IsMatch("ABC"));
        Assert.False(regex.IsMatch("ABCdef"));
    }

    // === Invalid property ===

    [Fact]
    public void InvalidProperty_ThrowsForx0002()
    {
        Assert.Throws<Forx0002Exception>(() => XPathRegex.Compile(@"\p{NonExistent}"));
    }

    // === Multi-char escapes still work ===

    [Fact]
    public void MultiCharEscape_d_StillWorks()
    {
        var regex = XPathRegex.Compile(@"\d+");
        Assert.Equal("123", regex.Match("abc123def")!.Value);
    }

    [Fact]
    public void MultiCharEscape_w_StillWorks()
    {
        var regex = XPathRegex.Compile(@"\w+");
        Assert.True(regex.IsMatch("hello"));
    }

    [Fact]
    public void MultiCharEscape_s_StillWorks()
    {
        var regex = XPathRegex.Compile(@"\s+");
        Assert.Equal(" ", regex.Match("hello world")!.Value);
    }
}
