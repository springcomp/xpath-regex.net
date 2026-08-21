using XPath.Regex.NET.Errors;

namespace XPath.Regex.NET.UnitTests;

/// <summary>
/// Focused tests for compile-time resource limits: <see cref="RegexCompileOptions"/>
/// validation, quantifier-bound enforcement in the parser, and program-instruction
/// budget enforcement in the compiler.
/// </summary>
public sealed class CompileLimitsTests
{
    // -----------------------------------------------------------------------
    // RegexCompileOptions validation
    // -----------------------------------------------------------------------

    [Fact]
    public void Options_Defaults_MatchDocumentedSecureDefaults()
    {
        var options = new RegexCompileOptions();

        Assert.Equal(1_000, options.MaxQuantifierBound);
        Assert.Equal(100_000, options.MaxProgramInstructions);
    }

    [Fact]
    public void Options_AtHardCeiling_Succeeds()
    {
        var options = new RegexCompileOptions(
            maxQuantifierBound: RegexCompileOptions.HardCeilingMaxQuantifierBound,
            maxProgramInstructions: RegexCompileOptions.HardCeilingMaxProgramInstructions);

        Assert.Equal(RegexCompileOptions.HardCeilingMaxQuantifierBound, options.MaxQuantifierBound);
        Assert.Equal(RegexCompileOptions.HardCeilingMaxProgramInstructions, options.MaxProgramInstructions);
    }

    [Fact]
    public void Options_AboveHardCeiling_MaxQuantifierBound_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => new RegexCompileOptions(maxQuantifierBound: RegexCompileOptions.HardCeilingMaxQuantifierBound + 1));
    }

    [Fact]
    public void Options_AboveHardCeiling_MaxProgramInstructions_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => new RegexCompileOptions(maxProgramInstructions: RegexCompileOptions.HardCeilingMaxProgramInstructions + 1));
    }

    [Fact]
    public void Options_BelowOne_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new RegexCompileOptions(maxQuantifierBound: 0));
        Assert.Throws<ArgumentOutOfRangeException>(() => new RegexCompileOptions(maxProgramInstructions: 0));
    }

    // -----------------------------------------------------------------------
    // Quantifier bound enforcement (parser)
    // -----------------------------------------------------------------------

    [Fact]
    public void Compile_ExactQuantifier_AtDefaultBound_Succeeds()
    {
        XPathRegex regex = XPathRegex.Compile("a{1000}");

        Assert.True(regex.IsMatch(new string('a', 1000)));
    }

    [Fact]
    public void Compile_ExactQuantifier_ExceedsDefaultBound_Throws()
    {
        var ex = Assert.Throws<RegexCompilationLimitExceededException>(
            () => XPathRegex.Compile("a{1001}"));

        Assert.Equal(RegexCompilationLimit.MaxQuantifierBound, ex.Limit);
    }

    [Fact]
    public void Compile_RangeQuantifier_MinExceedsDefaultBound_Throws()
    {
        var ex = Assert.Throws<RegexCompilationLimitExceededException>(
            () => XPathRegex.Compile("a{1001,2000}"));

        Assert.Equal(RegexCompilationLimit.MaxQuantifierBound, ex.Limit);
    }

    [Fact]
    public void Compile_RangeQuantifier_MaxExceedsDefaultBound_Throws()
    {
        var ex = Assert.Throws<RegexCompilationLimitExceededException>(
            () => XPathRegex.Compile("a{1,1001}"));

        Assert.Equal(RegexCompilationLimit.MaxQuantifierBound, ex.Limit);
    }

    [Fact]
    public void Compile_CustomLowerQuantifierBound_AtLimit_Succeeds()
    {
        var options = new RegexCompileOptions(maxQuantifierBound: 10);

        XPathRegex regex = XPathRegex.Compile("a{10}", RegexDialect.XPath30, options);

        Assert.True(regex.IsMatch(new string('a', 10)));
    }

    [Fact]
    public void Compile_CustomLowerQuantifierBound_ExceedsLimit_Throws()
    {
        var options = new RegexCompileOptions(maxQuantifierBound: 10);

        var ex = Assert.Throws<RegexCompilationLimitExceededException>(
            () => XPathRegex.Compile("a{11}", RegexDialect.XPath30, options));

        Assert.Equal(RegexCompilationLimit.MaxQuantifierBound, ex.Limit);
    }

    // -----------------------------------------------------------------------
    // Program instruction budget enforcement (compiler)
    // -----------------------------------------------------------------------

    [Fact]
    public void Compile_NestedRepeat_ExceedsDefaultProgramInstructionBudget_Throws()
    {
        // (a{1000}){1000}: each quantifier bound is individually within the default
        // limit, but nested expansion produces ~1,000,000 instructions, far above
        // the default 100,000 program-instruction budget.
        var ex = Assert.Throws<RegexCompilationLimitExceededException>(
            () => XPathRegex.Compile("(a{1000}){1000}"));

        Assert.Equal(RegexCompilationLimit.MaxProgramInstructions, ex.Limit);
    }

    [Fact]
    public void Compile_SmallNestedRepeat_WithinProgramInstructionBudget_Succeeds()
    {
        XPathRegex regex = XPathRegex.Compile("(a{10}){10}");

        Assert.True(regex.IsMatch(new string('a', 100)));
    }

    [Fact]
    public void Compile_CustomLowerProgramInstructionBudget_ExceedsLimit_Throws()
    {
        // Each bound (60) is well within the default quantifier bound, so this
        // exercises the compiler's instruction budget rather than the parser's.
        var options = new RegexCompileOptions(maxProgramInstructions: 50);

        var ex = Assert.Throws<RegexCompilationLimitExceededException>(
            () => XPathRegex.Compile("a{60}", RegexDialect.XPath30, options));

        Assert.Equal(RegexCompilationLimit.MaxProgramInstructions, ex.Limit);
    }

    // -----------------------------------------------------------------------
    // Overload / argument validation
    // -----------------------------------------------------------------------

    [Fact]
    public void Compile_NullOptions_Throws()
    {
        Assert.Throws<ArgumentNullException>(
            () => XPathRegex.Compile("a", RegexDialect.XPath30, options: null!));
    }

    [Fact]
    public void Compile_WithFlagsAndOptions_AppliesBoth()
    {
        var options = new RegexCompileOptions(maxQuantifierBound: 10);

        XPathRegex regex = XPathRegex.Compile("A{5}", "i", RegexDialect.XPath30, options);

        Assert.True(regex.IsMatch(new string('a', 5)));
    }
}
