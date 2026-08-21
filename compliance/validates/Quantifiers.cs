namespace XPath.Regex.NET.Validates;

/// <summary>
/// Test cases from: iregexp/test/iregexp-quantifiers.spec.ts
/// </summary>
public class QuantifiersTests
{
    [Theory]
    [InlineData("a*")]
    [InlineData("a+")]
    [InlineData("a?")]
    public void ValidQuantifiers(string expression)
        => Pass(expression);

    [Theory]
    [InlineData("a{2}")]
    [InlineData("a{2,3}")]
    public void ValidQuantities(string expression)
        => Pass(expression);

    [Theory]
    [InlineData("a??")]
    [InlineData("a*?")]
    [InlineData("a+?")]
    [InlineData("a{2}?")]
    [InlineData("a{2,}?")]
    [InlineData("a{2,3}?")]
    public void ValidReluctantQuantifiers(string expression)
      => Pass(expression);

    [Theory]
    [InlineData("*")]
    [InlineData("+")]
    [InlineData("?")]
    [InlineData("a**")]
    [InlineData("a++")]
    public void InvalidQuantifiers(string expression)
        => Fail(expression);

    [Theory]
    [InlineData("{")]
    [InlineData("}")]
    [InlineData("{}")]
    [InlineData("{,}")]
    [InlineData("{4,}")]
    [InlineData("{,2}")]
    [InlineData("a{")]
    [InlineData("a}")]
    [InlineData("a{}")]
    [InlineData("a{,}")]
    [InlineData("a{,2}")]
    public void InvalidQuantities(string expression)
        => Fail(expression);
}
