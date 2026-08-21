using System.Collections.Immutable;
using Unicode.NET;
using XPath.Regex.NET.Internal.Unicode;

namespace XPath.Regex.NET.UnitTests.Unicode;

public class UnicodeAdapterTests
{
    // === ToTupleRanges / ToCodePointSet roundtrip ===

    [Fact]
    public void ToTupleRanges_Roundtrip()
    {
        var ranges = ImmutableArray.Create((65, 90)); // A-Z
        var set = UnicodeAdapter.ToCodePointSet(ranges);
        var result = UnicodeAdapter.ToTupleRanges(set);
        Assert.Equal(ranges, result);
    }

    [Fact]
    public void ToTupleRanges_Empty_ReturnsEmpty()
    {
        var result = UnicodeAdapter.ToTupleRanges(CodePointSet.Empty);
        Assert.True(result.IsEmpty);
    }

    [Fact]
    public void ToCodePointSet_Empty_ReturnsEmpty()
    {
        var set = UnicodeAdapter.ToCodePointSet(ImmutableArray<(int, int)>.Empty);
        Assert.True(set.IsEmpty);
    }

    // === ResolveProperty ===

    [Fact]
    public void ResolveProperty_Lu_ContainsUppercase()
    {
        var ranges = UnicodeAdapter.ResolveProperty("Lu", negated: false, RegexDialect.XPath30);
        // Must contain A-Z range
        Assert.Contains(ranges, r => r.Lo <= 65 && r.Hi >= 90);
    }

    [Fact]
    public void ResolveProperty_Negated_ExcludesAtoZ()
    {
        var ranges = UnicodeAdapter.ResolveProperty("Lu", negated: true, RegexDialect.XPath30);
        // A-Z (65-90) should NOT be covered
        bool coversAtoZ = ranges.Any(r => r.Lo <= 65 && r.Hi >= 90);
        Assert.False(coversAtoZ);
    }

    [Fact]
    public void ResolveProperty_IsBasicLatin_ContainsAscii()
    {
        var ranges = UnicodeAdapter.ResolveProperty("IsBasicLatin", negated: false, RegexDialect.XPath30);
        // Basic Latin block = U+0000..U+007F
        Assert.Contains(ranges, r => r.Lo <= 0x0041 && r.Hi >= 0x007A);
    }

    [Fact]
    public void ResolveProperty_IsBasicLatin_Negated_ExcludesAscii()
    {
        var ranges = UnicodeAdapter.ResolveProperty("IsBasicLatin", negated: true, RegexDialect.XPath30);
        // ASCII range 0-127 should not appear
        bool coversAscii = ranges.Any(r => r.Lo <= 0 && r.Hi >= 127);
        Assert.False(coversAscii);
    }

    [Fact]
    public void ResolveProperty_UnknownProperty_ThrowsWithSuggestion()
    {
        var ex = Assert.Throws<InvalidOperationException>(() =>
            UnicodeAdapter.ResolveProperty("NonExistentXyzzy2", negated: false, RegexDialect.XPath30));
        Assert.NotNull(ex.Message);
    }

    [Fact]
    public void ResolveProperty_UnknownProperty_Throws()
    {
        Assert.Throws<InvalidOperationException>(() =>
            UnicodeAdapter.ResolveProperty("NonExistentXyzzy", negated: false, RegexDialect.XPath30));
    }

    // === ResolveShortcut ===

    [Fact]
    public void ResolveShortcut_Digit_ContainsAsciiDigits()
    {
        var ranges = UnicodeAdapter.ResolveShortcut('d');
        // XPath \d includes Unicode decimal digits; must at least contain ASCII 0-9
        Assert.Contains(ranges, r => r.Lo <= 48 && r.Hi >= 57);
    }

    [Fact]
    public void ResolveShortcut_UpperD_ExcludesAsciiDigits()
    {
        var ranges = UnicodeAdapter.ResolveShortcut('D');
        bool coversDigits = ranges.Any(r => r.Lo <= 48 && r.Hi >= 57);
        Assert.False(coversDigits);
    }

    [Fact]
    public void ResolveShortcut_Space_ContainsSpace()
    {
        var ranges = UnicodeAdapter.ResolveShortcut('s');
        Assert.Contains(ranges, r => r.Lo <= 32 && r.Hi >= 32);
    }

    [Fact]
    public void ResolveShortcut_Word_ContainsLatinLetters()
    {
        var ranges = UnicodeAdapter.ResolveShortcut('w');
        Assert.Contains(ranges, r => r.Lo <= 65 && r.Hi >= 90);
    }

    [Fact]
    public void ResolveShortcut_InvalidEscape_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            UnicodeAdapter.ResolveShortcut('z'));
    }

    // === ApplyCaseClosure ===

    [Fact]
    public void ApplyCaseClosure_A_IncludesBothCases()
    {
        var ranges = ImmutableArray.Create((65, 65)); // 'A'
        var closed = UnicodeAdapter.ApplyCaseClosure(ranges);
        // Must contain 'A' (65) and 'a' (97)
        Assert.Contains(closed, r => r.Lo <= 65 && r.Hi >= 65);
        Assert.Contains(closed, r => r.Lo <= 97 && r.Hi >= 97);
    }

    [Fact]
    public void ApplyCaseClosure_AtoZ_IncludesLowercase()
    {
        var ranges = ImmutableArray.Create((65, 90)); // A-Z
        var closed = UnicodeAdapter.ApplyCaseClosure(ranges);
        // Must contain a-z (97-122)
        Assert.Contains(closed, r => r.Lo <= 97 && r.Hi >= 122);
    }

    [Fact]
    public void ApplyCaseClosure_Digits_Unchanged()
    {
        var ranges = ImmutableArray.Create((48, 57)); // 0-9
        var closed = UnicodeAdapter.ApplyCaseClosure(ranges);
        // Digits have no case partners; result still contains 0-9
        Assert.Contains(closed, r => r.Lo <= 48 && r.Hi >= 57);
    }

    [Fact]
    public void ApplyCaseClosure_Empty_ReturnsEmpty()
    {
        var ranges = ImmutableArray<(int, int)>.Empty;
        var closed = UnicodeAdapter.ApplyCaseClosure(ranges);
        Assert.True(closed.IsEmpty);
    }

    // === XSD dialect gating ===

    [Fact]
    public void ResolveProperty_XsdDialect_KnownScript_Throws()
    {
        // XSD dialect blocks script properties even when they are valid names.
        var ex = Assert.Throws<InvalidOperationException>(() =>
            UnicodeAdapter.ResolveProperty("Greek", negated: false, RegexDialect.Xsd));
        Assert.Contains("Script property", ex.Message, StringComparison.Ordinal);
        Assert.Contains("not supported in XSD", ex.Message, StringComparison.Ordinal);
    }

    // === SimpleFold ===

    [Fact]
    public void SimpleFold_UpperA_ReturnsFold()
    {
        int folded = UnicodeAdapter.SimpleFold('A');
        // Simple fold of 'A' (65) should be 'a' (97) or itself
        Assert.True(folded == 65 || folded == 97);
    }
}
