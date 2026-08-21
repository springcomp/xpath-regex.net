using XPath.Regex.NET;
using XPath.Regex.NET.Errors;

namespace XPath.Regex.NET.UnitTests;

public sealed class RegexFlagsTests
{
    [Fact]
    public void Parse_EmptyString_ReturnsNone()
    {
        RegexFlags flags = RegexFlags.Parse(string.Empty, RegexDialect.XPath30);
        Assert.Equal(RegexFlags.None, flags);
    }

    [Fact]
    public void Parse_AllFlags_XPath30Plus()
    {
        RegexFlags flags = RegexFlags.Parse("smixq", RegexDialect.XPath30);
        Assert.True(flags.DotAll);
        Assert.True(flags.MultiLine);
        Assert.True(flags.IgnoreCase);
        Assert.True(flags.FreeSpacing);
        Assert.True(flags.Literal);
    }

    [Fact]
    public void Parse_DuplicateFlag_Idempotent()
    {
        RegexFlags a = RegexFlags.Parse("ii", RegexDialect.XPath30);
        RegexFlags b = RegexFlags.Parse("i", RegexDialect.XPath30);
        Assert.Equal(a, b);
    }

    [Theory]
    [InlineData(RegexDialect.Xsd)]
    public void Parse_AnyFlagInXsd_ThrowsForx0001(RegexDialect dialect)
    {
        Assert.Throws<Forx0001Exception>(() => RegexFlags.Parse("i", dialect));
    }

    [Theory]
    [InlineData("z")]
    [InlineData("!")]
    public void Parse_UnknownFlag_ThrowsForx0001(string flags)
    {
        Assert.Throws<Forx0001Exception>(() => RegexFlags.Parse(flags, RegexDialect.XPath30));
    }

    [Fact]
    public void ToString_RoundTrip()
    {
        RegexFlags flags = RegexFlags.Parse("mix", RegexDialect.XPath30);
        // ToString order is s m i x q
        Assert.Equal("mix", flags.ToString());
    }

    [Fact]
    public void Equality_SameFlags_Equal()
    {
        RegexFlags a = RegexFlags.Parse("im", RegexDialect.XPath30);
        RegexFlags b = RegexFlags.Parse("mi", RegexDialect.XPath30);
        Assert.Equal(a, b);
    }

    [Fact]
    public void Equality_DifferentFlags_NotEqual()
    {
        RegexFlags a = RegexFlags.Parse("i", RegexDialect.XPath30);
        RegexFlags b = RegexFlags.Parse("im", RegexDialect.XPath30);
        Assert.NotEqual(a, b);
    }
}
