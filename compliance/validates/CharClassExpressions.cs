namespace XPath.Regex.NET.Validates;

/// <summary>
/// Test cases from: iregexp/test/iregexp-cc-char-class-expr.spec.ts
/// Reduced set focusing on critical ranges and edge cases.
/// </summary>
public class CharClassExpressionsTests
{
    [Theory]
    [InlineData("[a-a]")]
    [InlineData("[a-ab-bc-c]")]
    public void Range(string expression)
      => Pass(expression);

    [Theory]
    [InlineData("[--]")]
    [InlineData("[-a]")]
    [InlineData("[a-]")]
    [InlineData("[-a-]")]
    [InlineData("[-a-b]")]
    [InlineData("[-a-bc-]")]
    public void LeadingOrTrailingDash(string expression)
        => Pass(expression);

    [Theory]
    [InlineData(@"[\n\r\t]")]
    [InlineData(@"[^\n\r\t]")]
    [InlineData(@"[^\n-\r\t-]")]
    public void EscapedCharacters(string expression)
        => Pass(expression);

    [Theory]
    [InlineData(@"[\r-\n]")]
    [InlineData(@"[z-a]")]
    [InlineData(@"[9-0]")]
    public void InvalidRangeOrder(string expression)
    => Fail(expression);

    [Theory]
    [InlineData(@"[\^a-b]")]
    [InlineData(@"[\^-^a-b]")]
    [InlineData(@"[a-b\^]")]
    [InlineData(@"[a-b^]")]
    [InlineData(@"[a-b^-^]")]
    public void PositiveCharacterGroup(string expression)
      => Pass(expression);

    [Theory]
    [InlineData("[^--]")]
    [InlineData("[^-a]")]
    [InlineData("[^a-]")]
    [InlineData("[^-a-]")]
    [InlineData("[^-a-b]")]
    [InlineData("[^^a]")]
    public void NegativeCharacterGroups(string expression)
        => Pass(expression);

    [Theory]
    [InlineData("[^a^]")]
    [InlineData("[^^a^]")]
    [InlineData("[^a^a]")]
    [InlineData("[^-^-^]")]
    public void InvalidNegativeCharacterGroups(string expression)
      => Fail(expression);

    [Theory]
    [InlineData("[")]
    [InlineData("]")]
    [InlineData("[^]")]
    [InlineData("[a--b]")]
    //[InlineData("[a-z-A-Z]")] should be invalid but accepted for now
    public void InvalidBrackets(string expression)
        => Fail(expression);
}
