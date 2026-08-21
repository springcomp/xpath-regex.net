using Unicode.NET;
using XPath.Regex.NET;

namespace XPath.Regex.NET.MatchesTests;

public partial class SmokeTests
{
    [Fact]
    public void Xsd()
    {
        // XSD expressions are implicitely anchored
        FailsToMatch("ab", "abc", RegexDialect.Xsd);
        Matches("ab", "ab", RegexDialect.Xsd);
        Matches("^$", "^$", RegexDialect.Xsd);
    }

    // ========== GREEDY vs RELUCTANT QUANTIFIERS ==========
    // Supported in XPath 2.0+ (XSD has greedy only)

    [Theory]
    [InlineData("a+", "aaa", "aaa")]        // Greedy: consumes all
    [InlineData("a+?", "aaa", "a")]         // Reluctant: consumes minimum
    [InlineData("a*", "aaa", "aaa")]        // Greedy: zero or more
    [InlineData("a*?", "aaa", "")]          // Reluctant: zero matches
    [InlineData("a{2,4}", "aaaaa", "aaaa")] // Greedy bounded
    [InlineData("a{2,4}?", "aaaaa", "aa")]  // Reluctant bounded
    public void XPath20_GreedyVsReluctant(string expression, string input, string matched)
    {
        System.Diagnostics.Debug.Assert(matched != null);
        Matches(expression, input, matched, RegexDialect.XPath20);
    }

    // ========== CAPTURING GROUPS & BACK-REFERENCES ==========
    // Supported in XPath 2.0+

    [Theory]
    [InlineData(@"(a+)b\1", "aabaa", "aabaa")]  // Back-ref to captured group
    [InlineData(@"(.)\1", "aa", "aa")]           // Back-ref matches same char
    [InlineData(@"([ab]+)\1", "abab", "abab")] // Longer capture with back-ref
    [InlineData(@"(x)?(y)\2", "yy", "yy")]       // Back-ref with optional group
    [InlineData(@"((a+)b)\1", "aabaabaab", "aabaab")] // Back-ref to nested group
    public void XPath20_CapturingAndBackReferences(string expression, string input, string matched)
    {
        System.Diagnostics.Debug.Assert(matched != null);
        Matches(expression, input, matched, RegexDialect.XPath20);
    }

    [Fact]
    public void XPath20_BackreferenceRetainsEmptyCaptureAcrossAlternation()
    {
        Matches(@"(|())\2x", "x", "x", RegexDialect.XPath20);
    }

    // ========== NON-CAPTURING GROUPS ==========
    // Supported in XPath 3.0+ (not in XPath 2.0)

    [Theory]
    [InlineData("(?:a+)", "aaa", "aaa")]            // Non-capturing group
    [InlineData("(?:ab)+", "ababab", "ababab")]     // Non-capturing repeating
    [InlineData("a(?:b|c)d", "acd", "acd")]         // Non-capturing with alternation
    [InlineData("(?:a{2,4})b", "aaab", "aaab")]    // Non-capturing with quantifier
    public void XPath30_NonCapturingGroups(string expression, string input, string matched)
    {
        System.Diagnostics.Debug.Assert(matched != null);
        Matches(expression, input, matched, RegexDialect.XPath30);
    }

    // ========== FLAG: i (Case-Insensitive) ==========
    // Supported in all XPath versions (2.0+)

    [Theory]
    [InlineData("abc", "ABC", "ABC", "i")]         // XPath 2.0: case-insensitive
    [InlineData("[A-Z]+", "abc", "abc", "i")]      // Range includes case-variants
    [InlineData(@"\p{Ll}+", "ABC", "ABC", "i")]   // Unicode category with flag
    public void XPath20_FlagI_CaseInsensitive(string expression, string input, string matched, string flags)
    {
        var regex = XPathRegex.Compile(expression, flags, RegexDialect.XPath20);
        var match = regex.Match(input);
        Assert.NotNull(match);
        Assert.Equal(matched, match.Value);
    }

    // ========== FLAG: s (Dotall) ==========
    // Supported in all XPath versions (2.0+)

    [Theory]
    [InlineData("a.c", "a\nc", "a\nc", "s")]      // XPath 2.0: . matches newline with s
    [InlineData("a.c", "a\rc", "a\rc", "s")]      // . matches carriage return with s
    [InlineData(".*", "abc\ndef", "abc\ndef", "s")] // Greedy with multiline content
    public void XPath20_FlagS_Dotall(string expression, string input, string matched, string flags)
    {
        var regex = XPathRegex.Compile(expression, flags, RegexDialect.XPath20);
        var match = regex.Match(input);
        Assert.NotNull(match);
        Assert.Equal(matched, match.Value);
    }

    // ========== FLAG: m (Multiline) ==========
    // Supported in all XPath versions (2.0+)

    [Theory]
    [InlineData("^a", "b\nab", "a", "m")]         // XPath 2.0: ^ matches line boundary
    [InlineData("c$", "abc\ndef", "c", "m")]     // $ matches line boundary
    [InlineData("^[a-z]+$", "hello\nworld", "hello", "m")] // Line-scoped match
    public void XPath20_FlagM_Multiline(string expression, string input, string matched, string flags)
    {
        var regex = XPathRegex.Compile(expression, flags, RegexDialect.XPath20);
        var match = regex.Match(input);
        Assert.NotNull(match);
        Assert.Equal(matched, match.Value);
    }

    // ========== FLAG: x (Verbose/Extended) ==========
    // Supported in all XPath versions (2.0+)

    [Theory]
    [InlineData("a b c", "abc", "abc", "x")]       // XPath 2.0: whitespace stripped
    [InlineData("a#comment\nb", "ab", "ab", "x")] // Comments stripped in verbose mode
    [InlineData("[a b]", "a", "a", "x")]         // Whitespace in charclass NOT stripped
    public void XPath20_FlagX_Verbose(string expression, string input, string matched, string flags)
    {
        var regex = XPathRegex.Compile(expression, flags, RegexDialect.XPath20);
        var match = regex.Match(input);
        Assert.NotNull(match);
        Assert.Equal(matched, match.Value);
    }

    // ========== FLAG: q (Quote/Literal) ==========
    // Supported in XPath 3.0+ only

    [Theory]
    [InlineData("a+b", "a+b", "a+b", "q")]        // XPath 3.0: + treated literal
    [InlineData("[abc]", "[abc]", "[abc]", "q")]  // [ ] treated literal
    [InlineData("a.*z", "a.*z", "a.*z", "q")]    // . * treated literal
    [InlineData(@"(a)\1", @"(a)\1", @"(a)\1", "q")] // Grouping syntax literal
    public void XPath30_FlagQ_Quote(string expression, string input, string matched, string flags)
    {
        var regex = XPathRegex.Compile(expression, flags, RegexDialect.XPath30);
        var match = regex.Match(input);
        Assert.NotNull(match);
        Assert.Equal(matched, match.Value);
    }

    // ========== COMPOSITE: Multiple Flags ==========

    [Theory]
    [InlineData("ABC", "abc", "abc", "i")]         // XPath 2.0: i flag alone
    [InlineData("ABC a BC", "abcabc", "abcabc", "ix")] // i + x (verbose)
    [InlineData("^ABC$", "abc", "abc", "im")]     // i + m (multiline)
    [InlineData("a.+", "a\nbc", "a\nbc", "s")]    // s allows . to match newline
    public void XPath20_CompositeFlags(string expression, string input, string matched, string flags)
    {
        var regex = XPathRegex.Compile(expression, flags, RegexDialect.XPath20);
        var match = regex.Match(input);
        Assert.NotNull(match);
        Assert.Equal(matched, match.Value);
    }

    // ========== DOT BEHAVIOR BY VERSION ==========

    [Theory]
    [InlineData(".", "a", "a")]                      // XSD: . matches any char except newline
    public void Xsd_DotNotNewline(string expression, string input, string matched)
    {
        System.Diagnostics.Debug.Assert(matched != null);
        Matches(expression, input, matched, RegexDialect.Xsd);
    }

    [Theory]
    [InlineData(".", "a", "a", "")]                      // XPath 2.0: . matches char (not \n)
    [InlineData("a.b", "a\nb", "a\nb", "s")]      // With s flag: matches \n
    public void XPath20_DotBehavior(string expression, string input, string matched, string flags)
    {
        var regex = XPathRegex.Compile(expression, flags, RegexDialect.XPath20);
        var match = regex.Match(input);
        Assert.NotNull(match);
        Assert.Equal(matched, match.Value);
    }

    [Theory]
    [InlineData(".", "a", "a", "")]                      // XPath 3.0+: . excludes \n and \r
    [InlineData("a.b", "a\rb", "a\rb", "s")]      // With s flag: matches \r
    public void XPath30_DotExcludesCarriageReturn(string expression, string input, string matched, string flags)
    {
        var regex = XPathRegex.Compile(expression, flags, RegexDialect.XPath30);
        var match = regex.Match(input);
        Assert.NotNull(match);
        Assert.Equal(matched, match.Value);
    }

    // ========== ANCHORING DIFFERENCES ==========

    [Theory]
    [InlineData("^abc", "^abc", "^abc")]           // XSD: ^ is literal
    [InlineData("ab", "ab", "ab")]                 // XSD: implicit whole-string anchoring
    public void Xsd_ImplicitAnchoring(string expression, string input, string matched)
    {
        System.Diagnostics.Debug.Assert(matched != null);
        Matches(expression, input, matched, RegexDialect.Xsd);
    }

    [Theory]
    [InlineData("^abc", "abc", "abc")]             // XPath 2.0: explicit ^ anchor
    [InlineData("abc$", "abc", "abc")]             // explicit $ anchor
    [InlineData("abc", "xabcy", "abc")]            // NO implicit anchoring
    public void XPath20_ExplicitAnchoring(string expression, string input, string matched)
    {
        System.Diagnostics.Debug.Assert(matched != null);
        Matches(expression, input, matched, RegexDialect.XPath20);
    }
}
