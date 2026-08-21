namespace XPath.Regex.NET.Validates;

/// <summary>
/// Test cases from: iregexp/test/iregexp-cc-char-class-esc.spec.ts
/// </summary>
public class CharClassEscapesTests
{
    [Theory]
    [InlineData(@"\p{C}")]
    [InlineData(@"\p{Cc}")]
    [InlineData(@"\p{Cf}")]
    [InlineData(@"\p{Cn}")]
    [InlineData(@"\p{Co}")]
    public void CategoryC_Others(string expression)
        => Pass(expression);

    [Theory]
    [InlineData(@"\p{Cs}")] // surrogate characters are not valid XML
    [InlineData(@"\p{Cx}")]
    public void InvalidCategoryC(string expression)
        => Fail(expression);

    [Theory]
    [InlineData(@"\p{L}")]
    [InlineData(@"\p{Ll}")]
    [InlineData(@"\p{Lm}")]
    [InlineData(@"\p{Lo}")]
    [InlineData(@"\p{Lt}")]
    [InlineData(@"\p{Lu}")]
    public void CategoryL_Letters(string expression)
        => Pass(expression);

    [Theory]
    [InlineData(@"\p{Lx}")]
    public void InvalidCategoryL(string expression)
        => Fail(expression);

    [Theory]
    [InlineData(@"\p{M}")]
    [InlineData(@"\p{Mc}")]
    [InlineData(@"\p{Me}")]
    [InlineData(@"\p{Mn}")]
    public void CategoryM_Marks(string expression)
        => Pass(expression);

    [Theory]
    [InlineData(@"\p{Mx}")]
    public void InvalidCategoryM(string expression)
        => Fail(expression);

    [Theory]
    [InlineData(@"\p{N}")]
    [InlineData(@"\p{Nd}")]
    [InlineData(@"\p{Nl}")]
    [InlineData(@"\p{No}")]
    public void CategoryN_Numbers(string expression)
        => Pass(expression);

    [Theory]
    [InlineData(@"\p{Nx}")]
    public void InvalidCategoryN(string expression)
        => Fail(expression);

    [Theory]
    [InlineData(@"\p{P}")]
    [InlineData(@"\p{Pc}")]
    [InlineData(@"\p{Pd}")]
    [InlineData(@"\p{Pe}")]
    [InlineData(@"\p{Pf}")]
    [InlineData(@"\p{Pi}")]
    [InlineData(@"\p{Po}")]
    [InlineData(@"\p{Ps}")]
    public void CategoryP_Punctuation(string expression)
        => Pass(expression);

    [Theory]
    [InlineData(@"\p{Px}")]
    public void InvalidCategoryP(string expression)
        => Fail(expression);

    [Theory]
    [InlineData(@"\p{Z}")]
    [InlineData(@"\p{Zl}")]
    [InlineData(@"\p{Zp}")]
    [InlineData(@"\p{Zs}")]
    public void CategoryZ_Separators(string expression)
        => Pass(expression);

    [Theory]
    [InlineData(@"\p{Zx}")]
    public void InvalidCategoryZ(string expression)
        => Fail(expression);

    [Theory]
    [InlineData(@"\p{S}")]
    [InlineData(@"\p{Sc}")]
    [InlineData(@"\p{Sk}")]
    [InlineData(@"\p{Sm}")]
    [InlineData(@"\p{So}")]
    public void CategoryS_Symbols(string expression)
        => Pass(expression);

    [Theory]
    [InlineData(@"\p{Sx}")]
    public void InvalidCategoryS(string expression)
        => Fail(expression);

    [Theory]
    [InlineData(@"\p{IsBasicLatin}")]
    [InlineData(@"\p{IsLatinExtended-A}")]
    public void IsBlock(string expression)
        => Pass(expression);

    [Theory]
    [InlineData(@"\p{Is}")]
    [InlineData(@"\p{IsAlpha}")]
    [InlineData(@"\p{IsLatin}")]
    public void InvalidIsBlock(string expression)
        => Fail(expression);

    [Theory]
    [InlineData(@"[\p{L}]")]
    [InlineData(@"[\p{M}]")]
    [InlineData(@"[\p{Lo}\p{Me}]")]
    public void CharacterClassInRange(string expression)
        => Pass(expression);

    [Theory]
    [InlineData(@"\p")]
    [InlineData(@"\p{")]
    [InlineData(@"\p{}")]
    public void InvalidCharacterProperties(string expression)
        => Fail(expression);

    // charClassEsc singleCharEsc tests
    [Theory]
    [InlineData(@"[\(]")]
    [InlineData(@"[\)]")]
    [InlineData(@"[\*]")]
    [InlineData(@"[\+]")]
    public void CharClassPunctuation1(string expression)
        => Pass(expression);

    [Theory]
    [InlineData(@"[\-]")]
    [InlineData(@"[\\.]")]
    public void CharClassPunctuation2(string expression)
        => Pass(expression);

    [Fact]
    public void CharClassQuestion()
    {
        Pass(@"[\?]");
    }

    [Theory]
    [InlineData(@"[\?]")]
    [InlineData(@"[\[]")]
    [InlineData(@"[\\]")]
    [InlineData(@"[\]]")]
    [InlineData(@"[\^]")]
    public void CharClassPunctuation3(string expression)
        => Pass(expression);

    [Theory]
    [InlineData(@"[\n]")]
    [InlineData(@"[\r]")]
    [InlineData(@"[\t]")]
    public void CharClassControlChars(string expression)
        => Pass(expression);

    [Theory]
    [InlineData(@"[\{]")]
    [InlineData(@"[\|]")]
    [InlineData(@"[\}]")]
    public void CharClassPunctuation4(string expression)
        => Pass(expression);

    [Theory]
    [InlineData(@"[\(\*\+\)]")]
    [InlineData(@"[\(\-\.\)]")]
    [InlineData(@"[\(\?\)]")]
    [InlineData(@"[\n\r\t]")]
    public void CharClassSequences(string expression)
        => Pass(expression);

    [Theory]
    [InlineData(@"[\a]")]
    [InlineData(@"[\0]")]
    [InlineData(@"[\,]")]
    public void InvalidCharClassEscapes(string expression)
        => Fail(expression);
}
