namespace XPath.Regex.NET.MatchesTests;

/// <summary>
/// Test cases from: iregexp/test/iregexp-cc-single-char-esc.spec.ts
/// </summary>
public class SingleCharEscapesTests
{
    [Theory]
    [InlineData(@"\(", "(")]
    [InlineData(@"\)", ")")]
    [InlineData(@"\*", "*")]
    [InlineData(@"\+", "+")]
    public void EscapesPunctuation1_Matches(string expression, string input)
        => Matches(expression, input);

    [Theory]
    [InlineData(@"\(", "")]
    [InlineData(@"\)", "")]
    [InlineData(@"\*", "")]
    [InlineData(@"\+", "")]
    public void EscapesPunctuation1_FailsToMatch(string expression, string input)
        => FailsToMatch(expression, input);

    [Theory]
    [InlineData(@"\-", "-")]
    [InlineData(@"\.", ".")]
    public void EscapesPunctuation2_Matches(string expression, string input)
        => Matches(expression, input);

    [Theory]
    [InlineData(@"\-", "")]
    [InlineData(@"\.", "")]
    public void EscapesPunctuation2_FailsToMatch(string expression, string input)
        => FailsToMatch(expression, input);

    [Fact]
    public void EscapesQuestion_Matches()
    {
        Matches(@"\?", "?");
    }

    [Fact]
    public void EscapesQuestion_FailsToMatch()
    {
        FailsToMatch(@"\?", "");
    }

    [Theory]
    [InlineData(@"\?", "?")]
    [InlineData(@"\[", "[")]
    [InlineData(@"\\", @"\")]
    [InlineData(@"\]", "]")]
    [InlineData(@"\^", "^")]
    public void EscapesPunctuation3_Matches(string expression, string input)
        => Matches(expression, input);

    [Theory]
    [InlineData(@"\?", "")]
    [InlineData(@"\[", "")]
    [InlineData(@"\\", "")]
    [InlineData(@"\]", "")]
    [InlineData(@"\^", "")]
    public void EscapesPunctuation3_FailsToMatch(string expression, string input)
        => FailsToMatch(expression, input);

    [Theory]
    [InlineData(@"\n", "\n")]
    [InlineData(@"\r", "\r")]
    [InlineData(@"\t", "\t")]
    public void EscapesControlChars_Matches(string expression, string input)
        => Matches(expression, input);

    [Theory]
    [InlineData(@"\n", "")]
    [InlineData(@"\r", "")]
    [InlineData(@"\t", "")]
    public void EscapesControlChars_FailsToMatch(string expression, string input)
        => FailsToMatch(expression, input);

    [Theory]
    [InlineData(@"\{", "{")]
    [InlineData(@"\|", "|")]
    [InlineData(@"\}", "}")]
    public void EscapesPunctuation4_Matches(string expression, string input)
        => Matches(expression, input);

    [Theory]
    [InlineData(@"\{", "")]
    [InlineData(@"\|", "")]
    [InlineData(@"\}", "")]
    public void EscapesPunctuation4_FailsToMatch(string expression, string input)
        => FailsToMatch(expression, input);

    [Theory]
    [InlineData(@"\(\*\+\)", "(*+)")]
    [InlineData(@"\(\-\.\)", "(-.)")]
    [InlineData(@"\(\?\)", "(?)")]
    [InlineData(@"\n\r\t", "\n\r\t")]
    public void ValidSequences_Matches(string expression, string input)
        => Matches(expression, input);

    [Theory]
    [InlineData(@"\(\*\+\)", "")]
    [InlineData(@"\(\-\.\)", "")]
    [InlineData(@"\(\?\)", "")]
    [InlineData(@"\n\r\t", "")]
    public void ValidSequences_FailsToMatch(string expression, string input)
        => FailsToMatch(expression, input);
}
