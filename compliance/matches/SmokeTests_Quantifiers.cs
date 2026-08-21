using Unicode.NET;

namespace XPath.Regex.NET.MatchesTests;

public partial class SmokeTests
{
    [Theory]
    [InlineData("a+", "a")]
    [InlineData("a+", "aa")]
    [InlineData("a*", "")]
    [InlineData("a*", "a")]
    [InlineData("a*", "aa")]
    [InlineData("a?", "")]
    [InlineData("a?", "a")]
    public void QuantAny(string expression, string input)
      => Matches(expression, input);

    [Theory]
    [InlineData("a+", "b")]
    [InlineData("a+", "")]
    [InlineData("^a*b", "c")]
    [InlineData("^a?b", "c")]
    public void QuantAny_FailsToMatch(string expression, string input)
      => FailsToMatch(expression, input);

    [Theory]
    [InlineData("a{2}", "aa")]
    public void QuantExact(string expression, string input)
      => Matches(expression, input);

    [Theory]
    [InlineData("a{2}", "aaa")]
    public void QuantExact_FailsToMatch(string expression, string input)
      => Matches(expression, input);
}
