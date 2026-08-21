namespace XPath.Regex.NET.MatchesTests;

/// <summary>
/// Test cases from: iregexp/test/iregexp-cc-char-class-esc.spec.ts
/// </summary>
public class CharClassEscapesTests
{
    [Theory]
    [InlineData(@"\p{C}", "\u0001")]
    [InlineData(@"\p{Cc}", "\u0001")]
    [InlineData(@"\p{Cf}", "\u00AD")]
    [InlineData(@"\p{Cn}", "\u0378")]
    [InlineData(@"\p{Co}", "\uE000")]
    public void CategoryC_Others_Matches(string expression, string input)
        => Matches(expression, input);

    [Theory]
    [InlineData(@"\p{C}", "")]
    [InlineData(@"\p{Cc}", "")]
    [InlineData(@"\p{Cf}", "")]
    [InlineData(@"\p{Cn}", "")]
    [InlineData(@"\p{Co}", "")]
    public void CategoryC_Others_FailsToMatch(string expression, string input)
        => FailsToMatch(expression, input);

    [Theory]
    [InlineData(@"\p{L}", "A")]
    [InlineData(@"\p{Ll}", "a")]
    [InlineData(@"\p{Lm}", "\u02B0")]
    [InlineData(@"\p{Lo}", "\u4E00")]
    [InlineData(@"\p{Lt}", "\u01C5")]
    [InlineData(@"\p{Lu}", "A")]
    public void CategoryL_Letters_Matches(string expression, string input)
        => Matches(expression, input);

    [Theory]
    [InlineData(@"\p{L}", "")]
    [InlineData(@"\p{Ll}", "")]
    [InlineData(@"\p{Lm}", "")]
    [InlineData(@"\p{Lo}", "")]
    [InlineData(@"\p{Lt}", "")]
    [InlineData(@"\p{Lu}", "")]
    public void CategoryL_Letters_FailsToMatch(string expression, string input)
        => FailsToMatch(expression, input);

    [Theory]
    [InlineData(@"\p{M}", "\u0301")]
    [InlineData(@"\p{Mc}", "\u0903")]
    [InlineData(@"\p{Me}", "\u0488")]
    [InlineData(@"\p{Mn}", "\u0301")]
    public void CategoryM_Marks_Matches(string expression, string input)
        => Matches(expression, input);

    [Theory]
    [InlineData(@"\p{M}", "")]
    [InlineData(@"\p{Mc}", "")]
    [InlineData(@"\p{Me}", "")]
    [InlineData(@"\p{Mn}", "")]
    public void CategoryM_Marks_FailsToMatch(string expression, string input)
        => FailsToMatch(expression, input);

    [Theory]
    [InlineData(@"\p{N}", "5")]
    [InlineData(@"\p{Nd}", "5")]
    [InlineData(@"\p{Nl}", "\u2160")]
    [InlineData(@"\p{No}", "\u00BD")]
    public void CategoryN_Numbers_Matches(string expression, string input)
        => Matches(expression, input);

    [Theory]
    [InlineData(@"\p{N}", "")]
    [InlineData(@"\p{Nd}", "")]
    [InlineData(@"\p{Nl}", "")]
    [InlineData(@"\p{No}", "")]
    public void CategoryN_Numbers_FailsToMatch(string expression, string input)
        => FailsToMatch(expression, input);

    [Theory]
    [InlineData(@"\p{P}", ".")]
    [InlineData(@"\p{Pc}", "_")]
    [InlineData(@"\p{Pd}", "-")]
    [InlineData(@"\p{Pe}", ")")]
    [InlineData(@"\p{Pf}", "\u201D")]
    [InlineData(@"\p{Pi}", "\u201C")]
    [InlineData(@"\p{Po}", "!")]
    [InlineData(@"\p{Ps}", "(")]
    public void CategoryP_Punctuation_Matches(string expression, string input)
        => Matches(expression, input);

    [Theory]
    [InlineData(@"\p{P}", "")]
    [InlineData(@"\p{Pc}", "")]
    [InlineData(@"\p{Pd}", "")]
    [InlineData(@"\p{Pe}", "")]
    [InlineData(@"\p{Pf}", "")]
    [InlineData(@"\p{Pi}", "")]
    [InlineData(@"\p{Po}", "")]
    [InlineData(@"\p{Ps}", "")]
    public void CategoryP_Punctuation_FailsToMatch(string expression, string input)
        => FailsToMatch(expression, input);

    [Theory]
    [InlineData(@"\p{Z}", " ")]
    [InlineData(@"\p{Zl}", "\u2028")]
    [InlineData(@"\p{Zp}", "\u2029")]
    [InlineData(@"\p{Zs}", " ")]
    public void CategoryZ_Separators_Matches(string expression, string input)
        => Matches(expression, input);

    [Theory]
    [InlineData(@"\p{Z}", "")]
    [InlineData(@"\p{Zl}", "")]
    [InlineData(@"\p{Zp}", "")]
    [InlineData(@"\p{Zs}", "")]
    public void CategoryZ_Separators_FailsToMatch(string expression, string input)
        => FailsToMatch(expression, input);

    [Theory]
    [InlineData(@"\p{S}", "+")]
    [InlineData(@"\p{Sc}", "$")]
    [InlineData(@"\p{Sk}", "^")]
    [InlineData(@"\p{Sm}", "+")]
    [InlineData(@"\p{So}", "\u00A9")]
    public void CategoryS_Symbols_Matches(string expression, string input)
        => Matches(expression, input);

    [Theory]
    [InlineData(@"\p{S}", "")]
    [InlineData(@"\p{Sc}", "")]
    [InlineData(@"\p{Sk}", "")]
    [InlineData(@"\p{Sm}", "")]
    [InlineData(@"\p{So}", "")]
    public void CategoryS_Symbols_FailsToMatch(string expression, string input)
        => FailsToMatch(expression, input);

    [Theory]
    [InlineData(@"\p{IsBasicLatin}", "A")]
    [InlineData(@"\p{IsLatinExtended-A}", "\u0100")]
    public void IsBlock_Matches(string expression, string input)
        => Matches(expression, input);

    [Theory]
    [InlineData(@"\p{IsBasicLatin}", "")]
    [InlineData(@"\p{IsLatinExtended-A}", "")]
    public void IsBlock_FailsToMatch(string expression, string input)
        => FailsToMatch(expression, input);

    [Theory]
    [InlineData(@"[\p{L}]", "A")]
    [InlineData(@"[\p{M}]", "\u0301")]
    [InlineData(@"[\p{Lo}\p{Me}]", "\u4E00")]
    public void CharacterClassInRange_Matches(string expression, string input)
        => Matches(expression, input);

    [Theory]
    [InlineData(@"[\p{L}]", "")]
    [InlineData(@"[\p{M}]", "")]
    [InlineData(@"[\p{Lo}\p{Me}]", "")]
    public void CharacterClassInRange_FailsToMatch(string expression, string input)
        => FailsToMatch(expression, input);

    // charClassEsc singleCharEsc tests
    [Theory]
    [InlineData(@"[\(]", "(")]
    [InlineData(@"[\)]", ")")]
    [InlineData(@"[\*]", "*")]
    [InlineData(@"[\+]", "+")]
    public void CharClassPunctuation1_Matches(string expression, string input)
        => Matches(expression, input);

    [Theory]
    [InlineData(@"[\(]", "")]
    [InlineData(@"[\)]", "")]
    [InlineData(@"[\*]", "")]
    [InlineData(@"[\+]", "")]
    public void CharClassPunctuation1_FailsToMatch(string expression, string input)
        => FailsToMatch(expression, input);

    [Theory]
    [InlineData(@"[\-]", "-")]
    [InlineData(@"[\\.]", @"\")]
    public void CharClassPunctuation2_Matches(string expression, string input)
        => Matches(expression, input);

    [Theory]
    [InlineData(@"[\-]", "")]
    [InlineData(@"[\\.]", "")]
    public void CharClassPunctuation2_FailsToMatch(string expression, string input)
        => FailsToMatch(expression, input);

    [Fact]
    public void CharClassQuestion_Matches()
    {
        Matches(@"[\?]", "?");
    }

    [Fact]
    public void CharClassQuestion_FailsToMatch()
    {
        FailsToMatch(@"[\?]", "");
    }

    [Theory]
    [InlineData(@"[\?]", "?")]
    [InlineData(@"[\[]", "[")]
    [InlineData(@"[\\]", @"\")]
    [InlineData(@"[\]]", "]")]
    [InlineData(@"[\^]", "^")]
    public void CharClassPunctuation3_Matches(string expression, string input)
        => Matches(expression, input);

    [Theory]
    [InlineData(@"[\?]", "")]
    [InlineData(@"[\[]", "")]
    [InlineData(@"[\\]", "")]
    [InlineData(@"[\]]", "")]
    [InlineData(@"[\^]", "")]
    public void CharClassPunctuation3_FailsToMatch(string expression, string input)
        => FailsToMatch(expression, input);

    [Theory]
    [InlineData(@"[\n]", "\n")]
    [InlineData(@"[\r]", "\r")]
    [InlineData(@"[\t]", "\t")]
    public void CharClassControlChars_Matches(string expression, string input)
        => Matches(expression, input);

    [Theory]
    [InlineData(@"[\n]", "")]
    [InlineData(@"[\r]", "")]
    [InlineData(@"[\t]", "")]
    public void CharClassControlChars_FailsToMatch(string expression, string input)
        => FailsToMatch(expression, input);

    [Theory]
    [InlineData(@"[\{]", "{")]
    [InlineData(@"[\|]", "|")]
    [InlineData(@"[\}]", "}")]
    public void CharClassPunctuation4_Matches(string expression, string input)
        => Matches(expression, input);

    [Theory]
    [InlineData(@"[\{]", "")]
    [InlineData(@"[\|]", "")]
    [InlineData(@"[\}]", "")]
    public void CharClassPunctuation4_FailsToMatch(string expression, string input)
        => FailsToMatch(expression, input);

    [Theory]
    [InlineData(@"[\(\*\+\)]", "(")]
    [InlineData(@"[\(\-\.\)]", "(")]
    [InlineData(@"[\(\?\)]", "(")]
    [InlineData(@"[\n\r\t]", "\n")]
    public void CharClassSequences_Matches(string expression, string input)
        => Matches(expression, input);

    [Theory]
    [InlineData(@"[\(\*\+\)]", "")]
    [InlineData(@"[\(\-\.\)]", "")]
    [InlineData(@"[\(\?\)]", "")]
    [InlineData(@"[\n\r\t]", "")]
    public void CharClassSequences_FailsToMatch(string expression, string input)
        => FailsToMatch(expression, input);
}
