using XPath.Regex.NET.Internal.Parser;

namespace XPath.Regex.NET.UnitTests.Parser;

public sealed class PreprocessorTests
{
    [Fact]
    public void Process_FreeSpacing_StripsWhitespaceOutsideCharacterClass()
    {
        RegexFlags flags = RegexFlags.Parse("x", RegexDialect.XPath30);

        PreprocessedPattern result = Preprocessor.Process("a b[ c ]d", flags);

        Assert.Equal("ab[ c ]d", result.EffectivePattern);
        Assert.False(result.IsLiteralMode);
    }

    [Fact]
    public void Process_FreeSpacing_StripsCommentUntilLineBreak()
    {
        RegexFlags flags = RegexFlags.Parse("x", RegexDialect.XPath30);

        PreprocessedPattern result = Preprocessor.Process("ab# comment\ncd", flags);

        Assert.Equal("abcd", result.EffectivePattern);
    }

    [Fact]
    public void Process_FreeSpacing_CommentAtEndOfPattern()
    {
        RegexFlags flags = RegexFlags.Parse("x", RegexDialect.XPath30);

        PreprocessedPattern result = Preprocessor.Process("ab# trailing", flags);

        Assert.Equal("ab", result.EffectivePattern);
    }

    [Fact]
    public void Process_FreeSpacing_EscapedHashIsNotComment()
    {
        RegexFlags flags = RegexFlags.Parse("x", RegexDialect.XPath30);

        PreprocessedPattern result = Preprocessor.Process("a\\#b", flags);

        Assert.Equal("a\\#b", result.EffectivePattern);
    }

    [Fact]
    public void Process_LiteralMode_BypassesFreeSpacing()
    {
        RegexFlags flags = RegexFlags.Parse("xq", RegexDialect.XPath30);

        PreprocessedPattern result = Preprocessor.Process("a b#c", flags);

        Assert.True(result.IsLiteralMode);
        Assert.Equal("a b#c", result.EffectivePattern);
    }

    [Fact]
    public void Process_SourceMap_PreservesOriginalOffsetsAfterStripping()
    {
        RegexFlags flags = RegexFlags.Parse("x", RegexDialect.XPath30);

        PreprocessedPattern result = Preprocessor.Process("a b#c\nd", flags);

        Assert.Equal("abd", result.EffectivePattern);
        Assert.Equal(0, result.ToOriginalOffset(0));
        Assert.Equal(2, result.ToOriginalOffset(1));
        Assert.Equal(6, result.ToOriginalOffset(2));
        Assert.Equal(7, result.ToOriginalBoundaryOffset(3));
    }
}
