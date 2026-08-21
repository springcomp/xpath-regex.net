using Unicode.NET;

namespace XPath.Regex.NET.MatchesTests;

public partial class SmokeTests
{
    [Theory]
    [InlineData(@".", "a")]
    [InlineData(@"[^\n\r]", "a")]
    [InlineData(@"\s+", " \r\n\t ")]
    [InlineData(@"[ \t\n\r]+", " \r\n\t ")]
    [InlineData(@"\S+", "^.\\?*+-()|$[],àbcdé123")]
    [InlineData(@"\i", "a")]
    [InlineData(@"\i", "_")]
    [InlineData(@"\i", ":")]
    [InlineData(@"\I", "5")]
    [InlineData(@"\c", "a")]
    [InlineData(@"\c", "5")]
    [InlineData(@"\c", "-")]
    [InlineData(@"\c", ".")]
    [InlineData(@"\C", " ")]
    [InlineData(@"\d+", "0123456789")]
    [InlineData(@"\D+", "abcXYZ .,!")]
    [InlineData(@"\w+", "abcXYZ0123àbcdé")]
    [InlineData(@"\W+", "_.,!? ")]
    public void MultiCharEscape_Matches(string expression, string input)
      => Matches(expression, input);

    [Theory]
    [InlineData(@".", "\r")]
    [InlineData(@".", "\n")]
    [InlineData(@"\s+", "abc")]
    [InlineData(@"\S+", " \r\n\t ")]
    [InlineData(@"\i", "5")]
    [InlineData(@"\I", "a")]
    [InlineData(@"\I", "_")]
    [InlineData(@"\I", ":")]
    [InlineData(@"\c", " ")]
    [InlineData(@"\C", "a")]
    [InlineData(@"\C", "5")]
    [InlineData(@"\C", "-")]
    [InlineData(@"\C", ".")]
    [InlineData(@"\d+", "abc")]
    [InlineData(@"\D+", "0123456789")]
    [InlineData(@"\w+", "_.,!? ")]
    [InlineData(@"\W+", "abcXYZ0123")]
    public void MultiCharEscape_FailsToMatch(string expression, string input)
      => FailsToMatch(expression, input);
}
