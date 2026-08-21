namespace XPath.Regex.NET.MatchesTests;

public partial class SmokeTests
{
    // Ready-to-use Xunit Theory tests
    [Theory]
    [InlineData("a??", "a", "")]
    [InlineData("a??", "", "")]
    [InlineData("a+?", "aaa", "a")]
    [InlineData("a*?", "aaa", "")]
    [InlineData("a{3}?", "aaaa", "aaa")]
    [InlineData("a{2,4}?", "aaaa", "aa")]
    [InlineData(".*?a", "xax", "xa")]
    [InlineData("a.*?b", "aXXbXXb", "aXXb")]
    [InlineData(".*?", "anything", "")]
    [InlineData("a.*?a", "abaaca", "aba")]
    [InlineData("(.*?)", "abc", "")]
    [InlineData("(.+?)", "abc", "a")]
    [InlineData("a??a", "aa", "a")]
    [InlineData(".*?cd", "abcd", "abcd")]
    [InlineData("a??.*?b", "aXXXb", "aXXXb")]
    [InlineData("a+?a+?", "aaaa", "aa")]
    [InlineData(".*?a.*?", "abaaca", "a")]
    [InlineData(".*?a.*?a", "abaaca", "aba")]
    public void Quantifiers(string expression, string input, string matched)
    {
        System.Diagnostics.Debug.Assert(matched != null);
        Matches(expression, input, matched);
    }
}
