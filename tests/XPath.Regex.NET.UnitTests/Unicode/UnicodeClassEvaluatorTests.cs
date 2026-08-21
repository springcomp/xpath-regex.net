using System.Collections.Immutable;
using XPath.Regex.NET.Errors;
using XPath.Regex.NET.Internal.Unicode;

namespace XPath.Regex.NET.UnitTests.Unicode;

public class UnicodeClassEvaluatorTests
{
    private readonly UnicodeClassEvaluator _evaluator = new(RegexDialect.XPath30);

    // === Property Escape Resolution ===

    [Fact]
    public void ResolvePropertyEscape_Lu_ContainsCapitalA()
    {
        ImmutableArray<(int Lo, int Hi)> ranges = _evaluator.ResolvePropertyEscape("Lu", false);
        Assert.False(ranges.IsDefaultOrEmpty);
        Assert.True(ContainsCodePoint(ranges, 'A'));
        Assert.False(ContainsCodePoint(ranges, 'a'));
    }

    [Fact]
    public void ResolvePropertyEscape_Lu_Negated_DoesNotContainCapitalA()
    {
        ImmutableArray<(int Lo, int Hi)> ranges = _evaluator.ResolvePropertyEscape("Lu", true);
        Assert.False(ranges.IsDefaultOrEmpty);
        Assert.False(ContainsCodePoint(ranges, 'A'));
        Assert.True(ContainsCodePoint(ranges, 'a'));
    }

    [Fact]
    public void ResolvePropertyEscape_IsBasicLatin_ContainsAscii()
    {
        ImmutableArray<(int Lo, int Hi)> ranges = _evaluator.ResolvePropertyEscape("IsBasicLatin", false);
        Assert.True(ContainsCodePoint(ranges, 0x00));
        Assert.True(ContainsCodePoint(ranges, 0x7F));
    }

    [Fact]
    public void ResolvePropertyEscape_MajorCategory_L()
    {
        ImmutableArray<(int Lo, int Hi)> ranges = _evaluator.ResolvePropertyEscape("L", false);
        Assert.True(ContainsCodePoint(ranges, 'A'));
        Assert.True(ContainsCodePoint(ranges, 'a'));
        Assert.False(ContainsCodePoint(ranges, '0'));
    }

    [Fact]
    public void ResolvePropertyEscape_UnknownProperty_Throws()
    {
        Assert.Throws<Forx0002Exception>(() =>
            _evaluator.ResolvePropertyEscape("NonExistent", false));
    }

    // === Shortcut Escape Resolution ===

    [Theory]
    [InlineData('d', '0', true)]
    [InlineData('d', 'A', false)]
    [InlineData('D', '0', false)]
    [InlineData('D', 'A', true)]
    [InlineData('s', ' ', true)]
    [InlineData('s', 'A', false)]
    [InlineData('w', 'a', true)]
    [InlineData('w', ' ', false)]
    public void ResolveMultiCharEscape_ContainsExpected(char escape, char testChar, bool expected)
    {
        ImmutableArray<(int Lo, int Hi)> ranges = UnicodeClassEvaluator.ResolveMultiCharEscape(escape);
        Assert.Equal(expected, ContainsCodePoint(ranges, testChar));
    }



    // === Helper ===

    private static bool ContainsCodePoint(ImmutableArray<(int Lo, int Hi)> ranges, int cp)
    {
        foreach ((int lo, int hi) in ranges)
        {
            if (cp >= lo && cp <= hi)
                return true;
        }
        return false;
    }
}
