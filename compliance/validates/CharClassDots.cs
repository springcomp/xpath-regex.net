namespace XPath.Regex.NET.Validates;

/// <summary>
/// Test cases from: iregexp/test/iregexp-cc-dot.spec.ts
/// </summary>
public class CharClassDotsTests
{
    [Theory]
    [InlineData(".")]
    [InlineData("..")]
    [InlineData(".*")]
    [InlineData(".?")]
    public void DotMetacharacter(string expression)
        => Pass(expression);
}
