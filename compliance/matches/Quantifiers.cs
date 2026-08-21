namespace XPath.Regex.NET.MatchesTests;

/// <summary>
/// Test cases from: iregexp/test/iregexp-quantifiers.spec.ts
/// </summary>
public class QuantifiersTests
{
    [Theory]
    [InlineData("a+", "a")]
    public void ValidQuantifiers_Matches(string expression, string input)
        => Matches(expression, input);

    [Theory]
    [InlineData("a+", "")]
    public void ValidQuantifiers_FailsToMatch(string expression, string input)
        => FailsToMatch(expression, input);

    // "a*" and "a?" are nullable; uplifted to anchored form so FailsToMatch is meaningful.
    [Theory]
    [InlineData("^(?:a*)$", "a")] // original: a*
    [InlineData("^(?:a?)$", "a")] // original: a?
    public void NullableQuantifiers_Matches(string expression, string input)
        => Matches(expression, input);

    [Theory]
    [InlineData("^(?:a*)$", "b")] // original: a*
    [InlineData("^(?:a?)$", "aa")] // original: a?
    public void NullableQuantifiers_FailsToMatch(string expression, string input)
        => FailsToMatch(expression, input);

    [Theory]
    [InlineData("a{2}", "aa")]
    [InlineData("a{2,3}", "aa")]
    public void ValidQuantities_Matches(string expression, string input)
        => Matches(expression, input);

    [Theory]
    [InlineData("a{2}", "")]
    [InlineData("a{2,3}", "")]
    public void ValidQuantities_FailsToMatch(string expression, string input)
        => FailsToMatch(expression, input);

    [Theory]
    [InlineData("a+?", "a")]
    [InlineData("a{2}?", "aa")]
    [InlineData("a{2,}?", "aa")]
    [InlineData("a{2,3}?", "aa")]
    public void ValidReluctantQuantifiers_Matches(string expression, string input)
        => Matches(expression, input);

    [Theory]
    [InlineData("a+?", "")]
    [InlineData("a{2}?", "")]
    [InlineData("a{2,}?", "")]
    [InlineData("a{2,3}?", "")]
    public void ValidReluctantQuantifiers_FailsToMatch(string expression, string input)
        => FailsToMatch(expression, input);

    // "a??" and "a*?" are nullable; uplifted to anchored form so FailsToMatch is meaningful.
    [Theory]
    [InlineData("^(?:a??)$", "a")] // original: a??
    [InlineData("^(?:a*?)$", "a")] // original: a*?
    public void NullableReluctantQuantifiers_Matches(string expression, string input)
        => Matches(expression, input);

    [Theory]
    [InlineData("^(?:a??)$", "aa")] // original: a??
    [InlineData("^(?:a*?)$", "b")] // original: a*?
    public void NullableReluctantQuantifiers_FailsToMatch(string expression, string input)
        => FailsToMatch(expression, input);
}
