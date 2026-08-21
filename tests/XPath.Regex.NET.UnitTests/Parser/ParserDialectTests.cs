using XPath.Regex.NET.Errors;
using XPath.Regex.NET.Internal.Parser;

namespace XPath.Regex.NET.UnitTests.Parser;

public sealed class ParserDialectTests
{
    [Theory]
    [InlineData("(?:a)", RegexDialect.Xsd)]
    [InlineData("a+?", RegexDialect.Xsd)]
    [InlineData("(a)\\1", RegexDialect.Xsd)]
    public void Parse_DialectUnsupportedFeature_ThrowsForx0002(string pattern, RegexDialect dialect)
    {
        Forx0002Exception ex = Assert.Throws<Forx0002Exception>(() => Parse(pattern, dialect));

        Assert.True(ex.PatternOffset >= 0);
    }

    [Theory]
    [InlineData("(?:a)")]
    [InlineData("a+?")]
    [InlineData("(a)\\1")]
    public void Parse_XPath30Feature_Allowed(string pattern)
    {
        ParseResult result = Parse(pattern, RegexDialect.XPath30);

        Assert.NotNull(result.Root);
    }

    private static ParseResult Parse(string pattern, RegexDialect dialect)
    {
        PreprocessedPattern preprocessed = Preprocessor.Process(pattern, RegexFlags.None);
        IReadOnlyList<RegexToken> tokens = RegexLexer.Tokenize(preprocessed, dialect);
        return RegexParser.Parse(tokens, dialect, RegexFlags.None, PermissiveUnicodeNameValidator.Instance);
    }
}
