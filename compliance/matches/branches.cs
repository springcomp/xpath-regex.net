namespace XPath.Regex.NET.MatchesTests;

/// <summary>
/// Test cases from: iregexp/test/iregexp-branch.spec.ts
/// </summary>
public class BranchesTests
{
    [Theory]
    [InlineData("a|b", "a")]
    public void ValidBranches_Matches(string expression, string input)
        => Matches(expression, input);

    [Theory]
    [InlineData("a|b", "")]
    public void ValidBranches_FailsToMatch(string expression, string input)
        => FailsToMatch(expression, input);

    // Branches with an empty alternative are nullable; uplifted to anchored form so FailsToMatch is meaningful.
    [Theory]
    [InlineData("^(?:a|)$", "a")] // original: a|
    [InlineData("^(?:a||b)$", "b")] // original: a||b
    [InlineData("^(?:|)$", "")] // original: |
    [InlineData("^(?:||)$", "")] // original: ||
    public void NullableBranches_Matches(string expression, string input)
        => Matches(expression, input);

    [Theory]
    [InlineData("^(?:a|)$", "b")] // original: a|
    [InlineData("^(?:a||b)$", "c")] // original: a||b
    [InlineData("^(?:|)$", "x")] // original: |
    [InlineData("^(?:||)$", "x")] // original: ||
    public void NullableBranches_FailsToMatch(string expression, string input)
        => FailsToMatch(expression, input);
}
