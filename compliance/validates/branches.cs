namespace XPath.Regex.NET.Validates;

/// <summary>
/// Test cases from: iregexp/test/iregexp-branch.spec.ts
/// </summary>
public class BranchesTests
{
    [Theory]
    [InlineData("a|b")]
    [InlineData("a|")]
    [InlineData("a||b")]
    [InlineData("|")]
    [InlineData("||")]
    public void ValidBranches(string expression)
        => Pass(expression);

    [Theory]
    [InlineData("|?")]
    public void InvalidBranches(string expression)
        => Fail(expression);
}
