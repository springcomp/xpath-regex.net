using System.Text;
using XPath.Regex.NET;
using XPath.Regex.NET.Errors;
using XPath.Regex.NET.Internal.Matcher;
using XPath.Regex.NET.Internal.Operations;

namespace XPath.Regex.NET.UnitTests.Operations;

public class ReplacementParserTests
{
    private static string Apply(string replacement, RegexFlags flags, int captureCount, int[] captures)
    {
        ParsedReplacement parsed = ReplacementParser.Parse(replacement, flags, captureCount);
        var ctx = new MatchContext(captures[0], captures[1], captures);
        var sb = new StringBuilder();
        parsed.Apply(ctx, string.Empty, sb);
        return sb.ToString();
    }

    private static string ApplyWithInput(string replacement, RegexFlags flags, int captureCount, int[] captures, string input)
    {
        ParsedReplacement parsed = ReplacementParser.Parse(replacement, flags, captureCount);
        var ctx = new MatchContext(captures[0], captures[1], captures);
        var sb = new StringBuilder();
        parsed.Apply(ctx, input, sb);
        return sb.ToString();
    }

    // 1 – plain literal
    [Fact]
    public void Parse_PlainLiteral()
    {
        // captures: group0 only, irrelevant
        string result = Apply("hello", RegexFlags.None, 0, [0, 0]);
        Assert.Equal("hello", result);
    }

    // 2 – $0 expands to full match
    [Fact]
    public void Parse_DollarZero_FullMatch()
    {
        // input = "hello", captures[0]=0, captures[1]=5
        string input = "hello";
        ParsedReplacement parsed = ReplacementParser.Parse("$0", RegexFlags.None, 0);
        var ctx = new MatchContext(0, 5, [0, 5]);
        var sb = new StringBuilder();
        parsed.Apply(ctx, input, sb);
        Assert.Equal("hello", sb.ToString());
    }

    // 3 – $1$2 expands groups 1 and 2
    [Fact]
    public void Parse_GroupRefs()
    {
        string input = "abcd";
        // group0: 0-4, group1: 0-2 ("ab"), group2: 2-4 ("cd")
        int[] captures = [0, 4, 0, 2, 2, 4];
        ParsedReplacement parsed = ReplacementParser.Parse("$1$2", RegexFlags.None, 2);
        var ctx = new MatchContext(0, 4, captures);
        var sb = new StringBuilder();
        parsed.Apply(ctx, input, sb);
        Assert.Equal("abcd", sb.ToString());
    }

    // 4 – \$ → literal '$'
    [Fact]
    public void Parse_EscapedDollar()
    {
        string result = Apply(@"\$", RegexFlags.None, 0, [0, 0]);
        Assert.Equal("$", result);
    }

    // 5 – \\ → literal '\'
    [Fact]
    public void Parse_EscapedBackslash()
    {
        string result = Apply(@"\\", RegexFlags.None, 0, [0, 0]);
        Assert.Equal(@"\", result);
    }

    // 6 – bare $ → FORX0004
    [Fact]
    public void Parse_BareDollar_ThrowsForx0004()
    {
        Assert.Throws<Forx0004Exception>(() => ReplacementParser.Parse("$", RegexFlags.None, 0));
    }

    // 7 – \x → FORX0004
    [Fact]
    public void Parse_InvalidEscape_ThrowsForx0004()
    {
        Assert.Throws<Forx0004Exception>(() => ReplacementParser.Parse(@"\x", RegexFlags.None, 0));
    }

    // 8 – $10 when S=1: strip last digit '0', N=1 ≤ S=1 → group1 + literal "0"
    [Fact]
    public void Parse_GroupRef10_StripLastDigit()
    {
        string input = "hello";
        // group0: 0-5, group1: 0-3 ("hel")
        int[] captures = [0, 5, 0, 3];
        ParsedReplacement parsed = ReplacementParser.Parse("$10", RegexFlags.None, 1);
        var ctx = new MatchContext(0, 5, captures);
        var sb = new StringBuilder();
        parsed.Apply(ctx, input, sb);
        // group1 = "hel", trailing "0" → "hel0"
        Assert.Equal("hel0", sb.ToString());
    }

    // 9 – $23 when S=3: 23>9, strip '3' → N=2 ≤ S=3 → group2 + literal "3"
    [Fact]
    public void Parse_GroupRef23_S3()
    {
        string input = "abcdef";
        // group0:0-6, group1:0-2("ab"), group2:2-4("cd"), group3:4-6("ef")
        int[] captures = [0, 6, 0, 2, 2, 4, 4, 6];
        ParsedReplacement parsed = ReplacementParser.Parse("$23", RegexFlags.None, 3);
        var ctx = new MatchContext(0, 6, captures);
        var sb = new StringBuilder();
        parsed.Apply(ctx, input, sb);
        // group2 = "cd", trailing "3" → "cd3"
        Assert.Equal("cd3", sb.ToString());
    }

    // 10 – q flag: entire replacement is literal, no $ interpretation
    [Fact]
    public void Parse_LiteralFlag_NoDollarExpansion()
    {
        var flags = RegexFlags.Parse("q", RegexDialect.XPath30);
        string result = Apply("$1", flags, 1, [0, 1, 0, 1]);
        Assert.Equal("$1", result);
    }
}
