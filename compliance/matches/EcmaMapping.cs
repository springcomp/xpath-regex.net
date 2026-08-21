namespace XPath.Regex.NET.MatchesTests;

/// <summary>
/// Test cases from: iregexp/test/iregexp-ecma.spec.ts
/// </summary>
public class EcmaMappingTests
{
    [Theory]
    [InlineData("a", "a")]
    [InlineData(@"\.", ".")]
    [InlineData(@"\.\\", @".\")]
    [InlineData("[a-z.+]", "m")]
    public void ValidEcmaPatterns_Matches(string expression, string input)
        => Matches(expression, input);

    [Theory]
    [InlineData("a", "")]
    [InlineData(@"\.", "")]
    [InlineData(@"\.\\", "")]
    [InlineData("[a-z.+]", "")]
    public void ValidEcmaPatterns_FailsToMatch(string expression, string input)
        => FailsToMatch(expression, input);

    // ".*" is an unbounded wildcard-star: nullable and matches any string, even anchored -- no FailsToMatch case exists.
    [Theory]
    [InlineData(".*", "")]
    public void ValidEcmaPatternsUnboundedWildcard_Matches(string expression, string input)
        => Matches(expression, input);
}
