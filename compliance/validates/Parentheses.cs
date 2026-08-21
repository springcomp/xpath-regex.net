namespace XPath.Regex.NET.Validates;

/// <summary>
/// Test cases from: iregexp/test/iregexp-parens.spec.ts
/// </summary>
public class ParenthesesTests
{
    [Theory]
    [InlineData("(a)")]
    [InlineData("(a)+")]
    [InlineData("(a){2}")]
    [InlineData("(a){2,3}")]
    public void ValidParens(string expression)
        => Pass(expression);

    [Fact]
    public void Sequence()
    {
        Pass("a(a)a");
    }

    [Theory]
    [InlineData("(")]
    [InlineData(")")]
    [InlineData("(a")]
    [InlineData("a)")]
    public void MismatchedParens(string expression)
        => Fail(expression);
}
