using XPath.Regex.NET.Internal.Unicode;

namespace XPath.Regex.NET.UnitTests.Unicode;

public class UnicodeDialectTests
{
    [Fact]
    public void XsdDialect_BlocksScripts()
    {
        var ex = Assert.Throws<InvalidOperationException>(() =>
            UnicodeAdapter.ResolveProperty("Greek", negated: false, RegexDialect.Xsd));
        Assert.Contains("Script property 'Greek' not supported in XSD", ex.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void XsdDialect_BlocksScripts_CompoundSyntax()
    {
        var ex = Assert.Throws<InvalidOperationException>(() =>
            UnicodeAdapter.ResolveProperty("Script=Greek", negated: false, RegexDialect.Xsd));
        Assert.Contains("not supported in XSD", ex.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void XsdDialect_AllowsBlocks()
    {
        var ranges = UnicodeAdapter.ResolveProperty("IsBasicLatin", negated: false, RegexDialect.Xsd);
        Assert.NotEmpty(ranges); // Block names should succeed
    }

    [Fact]
    public void XPath30PlusDialect_AllowsScripts()
    {
        var ranges = UnicodeAdapter.ResolveProperty("Script=Greek", negated: false, RegexDialect.XPath30);
        Assert.NotEmpty(ranges); // Should succeed
    }

    [Fact]
    public void XPath30PlusDialect_AllowsScriptByName()
    {
        var ranges = UnicodeAdapter.ResolveProperty("Greek", negated: false, RegexDialect.XPath30);
        Assert.NotEmpty(ranges); // Should succeed
    }

    [Fact]
    public void UnknownProperty_ErrorMessageIncludesSuggestion()
    {
        var ex = Assert.Throws<InvalidOperationException>(() =>
            UnicodeAdapter.ResolveProperty("Lowercse", negated: false, RegexDialect.Xsd));
        Assert.Contains("Did you mean", ex.Message, StringComparison.Ordinal);
    }
}
