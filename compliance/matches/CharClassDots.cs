namespace XPath.Regex.NET.MatchesTests;

/// <summary>
/// Test cases from: iregexp/test/iregexp-cc-dot.spec.ts
/// </summary>
public class CharClassDotsTests
{
    [Theory]
    [InlineData(".", "x")]
    [InlineData("..", "xy")]
    public void DotMetacharacter_Matches(string expression, string input)
        => Matches(expression, input);

    [Theory]
    [InlineData(".", "")]
    [InlineData("..", "")]
    public void DotMetacharacter_FailsToMatch(string expression, string input)
        => FailsToMatch(expression, input);

    // ".*" is an unbounded wildcard-star: nullable and matches any string of any content,
    // even when anchored (^(?:.*)$) -- no input can ever make it fail, so no FailsToMatch case exists.
    [Theory]
    [InlineData(".*", "")]
    public void UnboundedWildcard_Matches(string expression, string input)
        => Matches(expression, input);

    // ".?" is nullable but bounded to 0-1 chars; anchoring makes FailsToMatch meaningful.
    [Theory]
    [InlineData("^(?:.?)$", "a")] // original: .?
    public void OptionalWildcard_Matches(string expression, string input)
        => Matches(expression, input);

    [Theory]
    [InlineData("^(?:.?)$", "ab")] // original: .?
    public void OptionalWildcard_FailsToMatch(string expression, string input)
        => FailsToMatch(expression, input);
}
