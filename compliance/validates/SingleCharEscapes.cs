namespace XPath.Regex.NET.Validates;

/// <summary>
/// Test cases from: iregexp/test/iregexp-cc-single-char-esc.spec.ts
/// </summary>
public class SingleCharEscapesTests
{
    [Theory]
    [InlineData(@"\(")]
    [InlineData(@"\)")]
    [InlineData(@"\*")]
    [InlineData(@"\+")]
    public void EscapesPunctuation1(string expression)
        => Pass(expression);

    [Theory]
    [InlineData(@"\-")]
    [InlineData(@"\.")]
    public void EscapesPunctuation2(string expression)
        => Pass(expression);

    [Fact]
    public void EscapesQuestion()
    {
        Pass(@"\?");
    }

    [Theory]
    [InlineData(@"\?")]
    [InlineData(@"\[")]
    [InlineData(@"\\")]
    [InlineData(@"\]")]
    [InlineData(@"\^")]
    public void EscapesPunctuation3(string expression)
        => Pass(expression);

    [Theory]
    [InlineData(@"\n")]
    [InlineData(@"\r")]
    [InlineData(@"\t")]
    public void EscapesControlChars(string expression)
        => Pass(expression);

    [Theory]
    [InlineData(@"\{")]
    [InlineData(@"\|")]
    [InlineData(@"\}")]
    public void EscapesPunctuation4(string expression)
        => Pass(expression);

    [Theory]
    [InlineData(@"\(\*\+\)")]
    [InlineData(@"\(\-\.\)")]
    [InlineData(@"\(\?\)")]
    [InlineData(@"\n\r\t")]
    public void ValidSequences(string expression)
        => Pass(expression);

    [Theory]
    [InlineData(@"\a")]
    [InlineData(@"\0")]
    [InlineData(@"\,")]
    public void InvalidEscapes(string expression)
        => Fail(expression);
}
