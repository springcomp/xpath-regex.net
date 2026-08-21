namespace XPath.Regex.NET.MatchesTests;

/// <summary>
/// Test cases from: iregexp/test/iregexp-parens.spec.ts
/// </summary>
public class ParenthesesTests
{
    [Theory]
    [InlineData("(a)", "a")]
    [InlineData("(a)+", "a")]
    [InlineData("(a){2}", "aa")]
    [InlineData("(a){2,3}", "aa")]
    public void ValidParens_Matches(string expression, string input)
        => Matches(expression, input);

    [Theory]
    [InlineData("(a)", "")]
    [InlineData("(a)+", "")]
    [InlineData("(a){2}", "")]
    [InlineData("(a){2,3}", "")]
    public void ValidParens_FailsToMatch(string expression, string input)
        => FailsToMatch(expression, input);

    [Fact]
    public void Sequence_Matches()
    {
        Matches("a(a)a", "aaa");
    }

    [Fact]
    public void Sequence_FailsToMatch()
    {
        FailsToMatch("a(a)a", "");
    }
}
