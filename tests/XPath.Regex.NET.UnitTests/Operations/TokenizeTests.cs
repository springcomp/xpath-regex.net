using XPath.Regex.NET;
using XPath.Regex.NET.Errors;

namespace XPath.Regex.NET.UnitTests.Operations;

public class TokenizeTests
{
    // 1 – basic split with optional whitespace
    [Fact]
    public void Tokenize_BasicSplit()
    {
        var rx = XPathRegex.Compile(@"\s+");
        Assert.Equal(["1", "2", "3"], rx.Tokenize("1 2 3"));
    }

    // 2 – leading and trailing empty tokens
    [Fact]
    public void Tokenize_LeadingTrailingDelimiters()
    {
        // Use '|' escaped as \| for the pipe delimiter (SingleCharEsc)
        var rx = XPathRegex.Compile(@"\|");
        Assert.Equal(["", "a", ""], rx.Tokenize("|a|"));
    }

    // 3 – no match → single token = entire input
    [Fact]
    public void Tokenize_NoMatch()
    {
        var rx = XPathRegex.Compile(@"\|");
        Assert.Equal(["a"], rx.Tokenize("a"));
    }

    // 4 – empty string → empty list
    [Fact]
    public void Tokenize_EmptyInput()
    {
        var rx = XPathRegex.Compile(@"\|");
        Assert.Empty(rx.Tokenize(""));
    }

    // 5 – null input → empty list
    [Fact]
    public void Tokenize_NullInput()
    {
        var rx = XPathRegex.Compile(@"\|");
        Assert.Empty(rx.Tokenize(null));
    }

    // 6 – FORX0003 when pattern can match empty
    [Fact]
    public void Tokenize_ThrowsForx0003_WhenCanMatchEmpty()
    {
        var rx = XPathRegex.Compile("a*");
        Assert.Throws<Forx0003Exception>(() => rx.Tokenize("b"));
    }

    // 7 – whitespace splitter: leading/trailing whitespace creates empty tokens
    [Fact]
    public void Tokenize_WhitespaceSplitter_LeadingTrailing()
    {
        var rx = XPathRegex.Compile(@"\s+");
        Assert.Equal(["", "a", "b", ""], rx.Tokenize("  a  b  "));
    }

    // 8 – consecutive delimiters produce empty middle tokens
    [Fact]
    public void Tokenize_ConsecutiveDelimiters()
    {
        var rx = XPathRegex.Compile(@"\|");
        Assert.Equal(["a", "", "b"], rx.Tokenize("a||b"));
    }
}
