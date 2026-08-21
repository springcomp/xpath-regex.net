namespace XPath.Regex.NET.Validates;

/// <summary>
/// Test cases from: iregexp/test/iregexp-ecma.spec.ts
/// Validates successful compilation of XSD/XPath regexes.
/// Note: Full ECMAScript mapping validation would require additional translation APIs.
/// </summary>
public class EcmaMappingTests
{
    [Theory]
    [InlineData("a")]
    [InlineData(@"\.")]
    [InlineData(@"\.\\")]
    [InlineData(".*")]
    [InlineData("[a-z.+]")]
    public void ValidEcmaPatterns(string expression)
        => Pass(expression);
}
