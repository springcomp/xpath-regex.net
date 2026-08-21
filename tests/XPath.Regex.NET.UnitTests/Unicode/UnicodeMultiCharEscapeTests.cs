using System.Collections.Immutable;
using XPath.Regex.NET.Internal.Unicode;

namespace XPath.Regex.NET.UnitTests.Unicode;

public class UnicodeMultiCharEscapeTests
{
    [Fact]
    public void MultiChar_i_IncludesNameStartSamples()
    {
        ImmutableArray<(int Lo, int Hi)> ranges = UnicodeClassEvaluator.ResolveMultiCharEscape('i');

        Assert.True(ContainsCodePoint(ranges, ':'));
        Assert.True(ContainsCodePoint(ranges, '_'));
        Assert.True(ContainsCodePoint(ranges, 'A'));
        Assert.True(ContainsCodePoint(ranges, 'z'));
        Assert.True(ContainsCodePoint(ranges, 0x00C0));
        Assert.False(ContainsCodePoint(ranges, '0'));
        Assert.False(ContainsCodePoint(ranges, '-'));
    }

    [Fact]
    public void MultiChar_c_IncludesNameCharExtras()
    {
        ImmutableArray<(int Lo, int Hi)> ranges = UnicodeClassEvaluator.ResolveMultiCharEscape('c');

        Assert.True(ContainsCodePoint(ranges, '0'));
        Assert.True(ContainsCodePoint(ranges, '-'));
        Assert.True(ContainsCodePoint(ranges, '.'));
        Assert.True(ContainsCodePoint(ranges, '_'));
        Assert.False(ContainsCodePoint(ranges, ' '));
        Assert.False(ContainsCodePoint(ranges, 0));
    }

    [Fact]
    public void MultiChar_I_Complements_i()
    {
        ImmutableArray<(int Lo, int Hi)> complement = UnicodeClassEvaluator.ResolveMultiCharEscape('I');

        Assert.False(ContainsCodePoint(complement, ':'));
        Assert.True(ContainsCodePoint(complement, '0'));
    }

    [Fact]
    public void MultiChar_d_IncludesAsciiDigitsAndUnicodeDecimalDigits()
    {
        ImmutableArray<(int Lo, int Hi)> ranges = UnicodeClassEvaluator.ResolveMultiCharEscape('d');

        // \d = \p{Nd} (all Unicode decimal digit characters)
        // Should include ASCII digits
        Assert.True(ContainsCodePoint(ranges, '0'));
        Assert.True(ContainsCodePoint(ranges, '5'));
        Assert.True(ContainsCodePoint(ranges, '9'));

        // Should include digits from other scripts (e.g., Arabic-Indic)
        Assert.True(ContainsCodePoint(ranges, 0x0660)); // Arabic-Indic digit 0
        Assert.True(ContainsCodePoint(ranges, 0x0669)); // Arabic-Indic digit 9

        // Should NOT be restricted to ASCII only
        Assert.True(ranges.Length > 1, "Unicode Nd category should span multiple ranges");
    }

    [Fact]
    public void MultiChar_w_IncludesLettersDigitsExcludesPunctuation()
    {
        ImmutableArray<(int Lo, int Hi)> ranges = UnicodeClassEvaluator.ResolveMultiCharEscape('w');

        // \w = [#x0000-#x10FFFF]-[\p{P}\p{Z}\p{C}] (all except Punctuation, Separator, Other)
        // Should include ASCII letters and digits
        Assert.True(ContainsCodePoint(ranges, 'A'));
        Assert.True(ContainsCodePoint(ranges, 'a'));
        Assert.True(ContainsCodePoint(ranges, '0'));

        // Should include accented letters (e.g., à, é)
        Assert.True(ContainsCodePoint(ranges, 0x00E0)); // Latin small letter a with grave
        Assert.True(ContainsCodePoint(ranges, 0x00E9)); // Latin small letter e with acute

        // Should NOT include ASCII punctuation (including underscore which is Pc - connector punctuation)
        Assert.False(ContainsCodePoint(ranges, '-')); // Pd - dash punctuation
        Assert.False(ContainsCodePoint(ranges, '.'));
        Assert.False(ContainsCodePoint(ranges, '_')); // Pc - connector punctuation

        // Should NOT include spaces (separator category)
        Assert.False(ContainsCodePoint(ranges, ' '));

        // Should NOT be restricted to ASCII only
        Assert.True(ranges.Length > 1, "Unicode word class should span multiple ranges");
    }

    private static bool ContainsCodePoint(ImmutableArray<(int Lo, int Hi)> ranges, int codePoint)
    {
        foreach ((int lo, int hi) in ranges)
        {
            if (codePoint >= lo && codePoint <= hi)
                return true;
        }

        return false;
    }
}
