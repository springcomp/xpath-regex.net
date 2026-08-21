namespace XPath.Regex.NET.MatchesTests;

/// <summary>
/// Test cases from: iregexp/test/iregexp-cc-char-class-expr.spec.ts
/// Reduced set focusing on critical ranges and edge cases.
/// </summary>
public class CharClassExpressionsTests
{
    [Theory]
    [InlineData("[a-a]", "a")]
    [InlineData("[a-ab-bc-c]", "b")]
    public void Range_Matches(string expression, string input)
        => Matches(expression, input);

    [Theory]
    [InlineData("[a-a]", "")]
    [InlineData("[a-ab-bc-c]", "")]
    public void Range_FailsToMatch(string expression, string input)
        => FailsToMatch(expression, input);

    [Theory]
    [InlineData("[--]", "-")]
    [InlineData("[-a]", "a")]
    [InlineData("[a-]", "a")]
    [InlineData("[-a-]", "a")]
    [InlineData("[-a-b]", "a")]
    [InlineData("[-a-bc-]", "a")]
    public void LeadingOrTrailingDash_Matches(string expression, string input)
        => Matches(expression, input);

    [Theory]
    [InlineData("[--]", "")]
    [InlineData("[-a]", "")]
    [InlineData("[a-]", "")]
    [InlineData("[-a-]", "")]
    [InlineData("[-a-b]", "")]
    [InlineData("[-a-bc-]", "")]
    public void LeadingOrTrailingDash_FailsToMatch(string expression, string input)
        => FailsToMatch(expression, input);

    [Theory]
    [InlineData(@"[\n\r\t]", "\n")]
    [InlineData(@"[^\n\r\t]", "x")]
    [InlineData(@"[^\n-\r\t-]", "x")]
    public void EscapedCharacters_Matches(string expression, string input)
        => Matches(expression, input);

    [Theory]
    [InlineData(@"[\n\r\t]", "")]
    [InlineData(@"[^\n\r\t]", "")]
    [InlineData(@"[^\n-\r\t-]", "")]
    public void EscapedCharacters_FailsToMatch(string expression, string input)
        => FailsToMatch(expression, input);

    [Theory]
    [InlineData(@"[\^a-b]", "a")]
    [InlineData(@"[\^-^a-b]", "a")]
    [InlineData(@"[a-b\^]", "a")]
    [InlineData(@"[a-b^]", "a")]
    [InlineData(@"[a-b^-^]", "a")]
    public void PositiveCharacterGroup_Matches(string expression, string input)
        => Matches(expression, input);

    [Theory]
    [InlineData(@"[\^a-b]", "")]
    [InlineData(@"[\^-^a-b]", "")]
    [InlineData(@"[a-b\^]", "")]
    [InlineData(@"[a-b^]", "")]
    [InlineData(@"[a-b^-^]", "")]
    public void PositiveCharacterGroup_FailsToMatch(string expression, string input)
        => FailsToMatch(expression, input);

    [Theory]
    [InlineData("[^--]", "x")]
    [InlineData("[^-a]", "x")]
    [InlineData("[^a-]", "x")]
    [InlineData("[^-a-]", "x")]
    [InlineData("[^-a-b]", "x")]
    [InlineData("[^^a]", "x")]
    public void NegativeCharacterGroups_Matches(string expression, string input)
        => Matches(expression, input);

    [Theory]
    [InlineData("[^--]", "")]
    [InlineData("[^-a]", "")]
    [InlineData("[^a-]", "")]
    [InlineData("[^-a-]", "")]
    [InlineData("[^-a-b]", "")]
    [InlineData("[^^a]", "")]
    public void NegativeCharacterGroups_FailsToMatch(string expression, string input)
        => FailsToMatch(expression, input);
}
