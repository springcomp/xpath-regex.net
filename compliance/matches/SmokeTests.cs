using Unicode.NET;

namespace XPath.Regex.NET.MatchesTests;

public partial class SmokeTests
{
    [Fact]
    public void Anchor()
    {
        FailsToMatch("^ab$", "abc");
        Matches("^ab$", "ab");
        Matches("^$", "");
    }

    [Fact]
    public void Atom()
    {
        Matches("a", "a");
        Matches("a", "aaa");
    }

    [Theory]
    [InlineData(@"\n", "\n")]
    [InlineData(@"\r", "\r")]
    [InlineData(@"\t", "\t")]
    public void SingleCharEsc_C0(string expression, string input)
      => Matches(expression, input);

    [Theory]
    [InlineData(@"\\")]
    [InlineData(@"\|")]
    [InlineData(@"\.")]
    [InlineData(@"\^")]
    [InlineData(@"\?")]
    [InlineData(@"\*")]
    [InlineData(@"\+")]
    [InlineData(@"\{")]
    [InlineData(@"\}")]
    [InlineData(@"\(")]
    [InlineData(@"\)")]
    [InlineData(@"\[")]
    [InlineData(@"\]")]
    public void SingleCharEsc(string expression)
    {
        System.Diagnostics.Debug.Assert(expression?.Length == 2);
        var input = expression[1].ToString();
        Matches(expression, input);
    }
}
