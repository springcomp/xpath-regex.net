using XPath.Regex.NET.Errors;
using XPath.Regex.NET.Internal.Parser;

namespace XPath.Regex.NET.UnitTests.Parser;

public sealed class ParserErrorTests
{
    [Theory]
    [InlineData("(", 1)]
    [InlineData("a)", 1)]
    [InlineData("[abc", 4)]
    [InlineData("[]", 1)]
    [InlineData("*a", 0)]
    [InlineData("a{,3}", 2)]
    [InlineData("a{}", 2)]
    [InlineData("a{4,2}", 1)]
    [InlineData("[z-a]", 1)]
    [InlineData("(a)\\2", 3)]
    [InlineData("\\1(a)", 0)]
    public void Parse_InvalidPattern_ThrowsForx0002WithExpectedOffset(string pattern, int expectedOffset)
    {
        Forx0002Exception ex = Assert.Throws<Forx0002Exception>(() => Parse(pattern, RegexDialect.XPath30));

        Assert.Equal(expectedOffset, ex.PatternOffset);
    }

    [Fact]
    public void Parse_FreeSpacingOffsetMapping_ReportsOriginalOffset()
    {
        RegexFlags flags = RegexFlags.Parse("x", RegexDialect.XPath30);
        Forx0002Exception ex = Assert.Throws<Forx0002Exception>(() => Parse("a # cmt\n [z-a]", RegexDialect.XPath30, flags));

        Assert.Equal(10, ex.PatternOffset);
    }

    [Fact]
    public void Parse_BackReferenceOverflow_ThrowsForx0002()
    {
        Forx0002Exception ex = Assert.Throws<Forx0002Exception>(() => Parse("(a)\\2147483648", RegexDialect.XPath30));

        Assert.Equal(3, ex.PatternOffset);
    }

    private static ParseResult Parse(string pattern, RegexDialect dialect, RegexFlags flags = default)
    {
        RegexFlags effectiveFlags = flags == default ? RegexFlags.None : flags;
        PreprocessedPattern preprocessed = Preprocessor.Process(pattern, effectiveFlags);
        IReadOnlyList<RegexToken> tokens = RegexLexer.Tokenize(preprocessed, dialect);
        return RegexParser.Parse(tokens, dialect, effectiveFlags, PermissiveUnicodeNameValidator.Instance);
    }
}
