using XPath.Regex.NET.Errors;
using XPath.Regex.NET.Internal.Parser;

namespace XPath.Regex.NET.UnitTests.Parser;

public sealed class LexerTests
{
    [Fact]
    public void Tokenize_FreeSpacing_UsesOriginalOffsets()
    {
        RegexFlags flags = RegexFlags.Parse("x", RegexDialect.XPath30);
        PreprocessedPattern preprocessed = Preprocessor.Process("a b", flags);

        IReadOnlyList<RegexToken> tokens = RegexLexer.Tokenize(preprocessed, RegexDialect.XPath30);

        Assert.Equal(0, tokens[0].OriginalOffset);
        Assert.Equal(2, tokens[1].OriginalOffset);
        Assert.Equal(3, tokens[2].OriginalOffset);
    }

    [Fact]
    public void Tokenize_DanglingBackslash_ThrowsForx0002WithOffset()
    {
        Forx0002Exception ex = Assert.Throws<Forx0002Exception>(() => Lex("\\"));

        Assert.Equal(0, ex.PatternOffset);
    }

    [Fact]
    public void Tokenize_UnterminatedUnicodePropertyEscape_ThrowsForx0002WithOffset()
    {
        Forx0002Exception ex = Assert.Throws<Forx0002Exception>(() => Lex("\\p{Lu"));

        Assert.Equal(0, ex.PatternOffset);
    }

    [Fact]
    public void Tokenize_EmptyUnicodePropertyEscape_ThrowsForx0002WithOffset()
    {
        Forx0002Exception ex = Assert.Throws<Forx0002Exception>(() => Lex("\\p{}"));

        Assert.Equal(0, ex.PatternOffset);
    }

    [Fact]
    public void Tokenize_InvalidEscape_ThrowsForx0002WithOffset()
    {
        Forx0002Exception ex = Assert.Throws<Forx0002Exception>(() => Lex("\\q"));

        Assert.Equal(0, ex.PatternOffset);
    }



    [Fact]
    public void Tokenize_EscapedDollar_InXsd_ThrowsForx0002()
    {
        PreprocessedPattern preprocessed = Preprocessor.Process("\\$", RegexFlags.None);

        Forx0002Exception ex = Assert.Throws<Forx0002Exception>(() => RegexLexer.Tokenize(preprocessed, RegexDialect.Xsd));

        Assert.Equal(0, ex.PatternOffset);
    }

    [Fact]
    public void Tokenize_LiteralMode_EmitsLiteralCharsOnly()
    {
        RegexFlags flags = RegexFlags.Parse("q", RegexDialect.XPath30);
        PreprocessedPattern preprocessed = Preprocessor.Process("a.b", flags);

        IReadOnlyList<RegexToken> tokens = RegexLexer.Tokenize(preprocessed, RegexDialect.XPath30);

        Assert.Collection(tokens,
            t => Assert.Equal(RegexTokenKind.LiteralChar, t.Kind),
            t => Assert.Equal(RegexTokenKind.LiteralChar, t.Kind),
            t => Assert.Equal(RegexTokenKind.LiteralChar, t.Kind),
            t => Assert.Equal(RegexTokenKind.End, t.Kind));
    }

    private static IReadOnlyList<RegexToken> Lex(string pattern)
    {
        PreprocessedPattern preprocessed = Preprocessor.Process(pattern, RegexFlags.None);
        return RegexLexer.Tokenize(preprocessed, RegexDialect.XPath30);
    }
}
