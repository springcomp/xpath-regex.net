using XPath.Regex.NET.Errors;

namespace XPath.Regex.NET.UnitTests;

public sealed class QuantifierCompilationLimitTests
{
    [Theory]
    [InlineData("a{2147483648}")]
    [InlineData("a{2147483648,2147483648}")]
    [InlineData("a{1,2147483648}")]
    [InlineData("a{2147483648,}")]
    public void Compile_QuantifierBoundExceedsInt32_ThrowsCompilationLimitException(string pattern)
    {
        RegexCompilationLimitExceededException exception = Assert.Throws<RegexCompilationLimitExceededException>(
            () => XPathRegex.Compile(pattern));

        Assert.Equal(RegexCompilationLimit.MaxQuantifierBound, exception.Limit);
    }
}
